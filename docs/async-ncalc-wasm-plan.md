# Async NCalc → No Threading → WASM Plan

## Background

Quest Viva's Engine currently uses thread-blocking to "pause" script execution while
waiting for player input (menus, questions, `wait` commands). This is done via
`Monitor.Wait()/Pulse()` in `WorldModel.cs`. The goal is to replace this with
`async/await`, which eliminates the need for background threads and makes the Engine
compatible with a browser-hosted WASM player.

The NCalc → async NCalc → no threads → WASM chain is the right path. The expression
evaluator isn't where threads are primarily used, but async NCalc is a necessary link
because expressions can call Quest functions (via `EvaluateFunction`), and if any of
those functions need to pause (e.g. because they trigger a menu), the async-ness needs
to propagate up through the expression evaluator.

The real unlock is: replace `Monitor.Wait()/Pulse()` with `TaskCompletionSource`, make
`IScript.Execute()` → `ExecuteAsync()`, and eliminate `DoInNewThreadAndWait()`. Once
everything is `async/await`, WASM works because the game coroutine yields to the browser
event loop at `await` points rather than blocking a thread.

**Gating condition for starting this work**: NCalc confirmed stable in production.

---

## Phase 1 — Async NCalc

- Swap `NCalcSync` for NCalc's async package (`NCalc` with async expression support)
- Add `EvaluateAsync(Context c)` to `IExpression<T>` alongside (or replacing) `Evaluate()`
- Update `NcalcExpressionEvaluator` to use NCalc's async `EvaluateAsync()` with an async `EvaluateFunction` handler
- `FleeExpressionEvaluator` gets a shim (wraps sync call in `Task.FromResult`) since Flee has no async path — it stays working but is on the way out

---

## Phase 2 — Async script interface

This is the bulk of the work.

- Add `Task ExecuteAsync(Context c)` to `IScript` (keep `Execute()` alongside during migration so tests/editor don't break immediately)
- Propagate async through: `ScriptFactory` → `MultiScript` → individual script classes (`WaitScript`, `ShowMenuScript`, `AskScript`, etc.) in `src/Engine/Scripts/`
- At each await point that currently calls `Monitor.Wait()`, instead `await` a `TaskCompletionSource.Task`

Risk: this touches every `IScript` implementation and is hard to do incrementally. Worth
deciding up front whether to run a parallel async interface during migration or do a hard
cutover.

---

## Phase 3 — Replace Monitor-based blocking

In `WorldModel.cs`, replace:

```csharp
// current
lock (_waitForResponseLock) { Monitor.Wait(_waitForResponseLock); }

// new
_inputTcs = new TaskCompletionSource();
await _inputTcs.Task;
```

The response handlers (`SetMenuResponse`, `SetQuestionResponse`, `SetWaitFinished`) call
`_inputTcs.SetResult()` instead of `Monitor.Pulse()`. `DoInNewThreadAndWait()` is
deleted.

The `ThreadState` enum and `_threadLock` go away entirely. `WaitUntilFinishedWorking()`
in the player becomes `await`-based or disappears.

---

## Phase 4 — Async player surface

- `PlayerCore`'s `GameLauncher` and turn-processing become async end-to-end
- `WebPlayer`'s Blazor components `await` turn execution rather than blocking
- The existing callback-based path (`CallbackManager`) can be simplified since `async/await` subsumes the callback pattern

---

## Phase 5 — Legacy asyncification

Legacy (`src/Legacy/`) implements its own Q4 format interpreter (`V4Game.cs`) rather than
sharing the Engine's script execution pipeline. Quest 4 scripting is much more limited
than ASLX, so the surface area is a fraction of the Engine work — but it needs the same
treatment (replace blocking waits with `TaskCompletionSource`, make execution methods
async).

Do this in the same phase as the Engine rather than after, so the stack isn't partially
threaded when the WasmPlayer is built.

---

## Phase 6 — WasmPlayer

The UI approach should follow WasmEditor (pure JS + C# WASM DLL) rather than Blazor
WASM. Reasons:

- **Consistent with WasmEditor** — same build pipeline, same `WasmConfig`, same
  deployment model
- **Smaller and faster** — no Blazor framework overhead on top of the .NET WASM runtime
- **Game output is already HTML** — PlayerCore renders rich HTML output; the JS layer
  just needs to route it into a container, not render it
- **CDN / Electron target** — a pure JS shell is portable to both; Blazor WASM is not

### WasmPlayer project

- `WasmPlayer` project targeting `browser-wasm`, analogous to `WasmEditor`
- `WasmPlayerBridge.cs` — `[JSExport]` entry points for starting a game, sending player
  input, answering menus/questions
- Wire up with `WasmConfig` (already sets `UseNCalc = true`)
- Game loop runs on the browser's main thread, yielding at `await` on
  `TaskCompletionSource` tasks when waiting for player input
- JS calls `SetMenuResponse()` etc. from UI events → `TCS.SetResult()` → game coroutine
  resumes

### Key design task: `[JSImport]` callback interface

Unlike WasmEditor (where JS calls C# and C# returns data), the player's primary
communication direction is **C# → JS**: every line of output, sound, menu, and turn
completion needs to be pushed to the UI in real time. This requires `[JSImport]`
callbacks — not yet implemented in WasmEditor, but essential for the player.

Define this interface upfront before building the bridge. Events to cover:

- Text/HTML output (`Print`)
- Sound (`PlaySound`, `StopSound`)
- Show menu / ask question (pause game, await response)
- Turn begin / turn end (for UI state like disabling input mid-turn)
- Game over

### JS frontend

A lightweight Svelte (or vanilla JS) frontend analogous to `src/WebEditor/`:

- Renders game output HTML into a scrolling panel
- Routes player text input and UI responses back to `[JSExport]` methods on the bridge
- Loads the game file via the same `FileAdapter` interface as WebEditor (reuse or extract
  into a shared package)
- Handles audio via jPlayer (already embedded in PlayerCore) or a JS replacement

---

## Notes and risks

- **Flee must be removed or shimmed first** — it has no async path. This migration is a good forcing function to finish removing Flee entirely.
- **Tests will need updating** — existing tests call `Execute()` synchronously. Moving to `ExecuteAsync()` means test infrastructure changes too.
- **Callback manager overlap** — there's already a partial callback-based async mechanism (`CallbackManager`, `StartWaitAsync`). Phase 3 should decide whether to unify these or remove the old path outright.
- **SharedArrayBuffer** — even with Quest's own threading removed, the .NET WASM runtime uses `SharedArrayBuffer` internally (for the GC). The COOP/COEP headers required by WasmEditor will be needed for WasmPlayer too. Confirm the CDN target serves these headers.
