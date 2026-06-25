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

## Phase 1 — Async NCalc ✅

- Swapped `NCalcSync` for `NCalcAsync` (both redirect to `NCalc.Core`, which already has `EvaluateAsync`, `EvaluateAsyncFunction`, and `EvaluateBinaryAsync` events)
- Added `EvaluateAsync(Context c)` to `IExpressionEvaluator<T>` and `IDynamicExpressionEvaluator` (removed `out` covariance from `IExpressionEvaluator<T>` since `Task<T>` is invariant)
- `NcalcExpressionEvaluator` registers async event handlers and implements `EvaluateAsync` using NCalc's async path with full async parameter evaluation (`EvaluateAslFunctionAsync`, `EvaluateFunctionFromTypeAsync`, `EvaluateBinaryAsync`)
- `FleeExpressionEvaluator` gets `Task.FromResult` shims since Flee has no async path — it stays working but is on the way out
- `Expression<T>` and `ExpressionDynamic` expose `ExecuteAsync` methods
- Branch: `async-ncalc`

---

## Phase 2 — Async script interface ✅

Chose **parallel interface** (Option A): `Task ExecuteAsync(Context c)` added to `IScript`
alongside `Execute()`, so tests and the editor continue working unchanged.

- `ScriptBase` has a virtual default shim: `Execute(c); return Task.CompletedTask;`
- All control-flow scripts have proper async overrides:
  - `MultiScript` — loops and `await`s each child
  - `IfScript` — sync condition evaluation, async branches
  - `ForScript`, `WhileScript`, `ForEachScript` — sync bounds/condition, async loop body
  - `SwitchScript` (including inner `SwitchCases`) — sync expression, async matched case
  - `FirstTimeScript` — async branch scripts
  - `FunctionCallScript` — calls `RunProcedureAsync`
  - `RunDelegateScript`, `DoActionScript`, `InvokeScript` — call `RunScriptAsync`
  - `LazyLoadScript` — delegates to inner script's `ExecuteAsync`
- `WorldModel` gains `RunScriptAsync` and `RunProcedureAsync` parallel variants
- `NcalcExpressionEvaluator.EvaluateAslFunctionAsync` now uses `RunProcedureAsync` (no more `.GetAwaiter().GetResult()` in the async path)
- Pause-inducing scripts (`WaitScript`, `ShowMenuScript`, `AskScript`, etc.) still fall through to the sync shim — they'll be wired up properly in Phase 3
- Branch: `async-ncalc`

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

- **Flee must be removed before Phase 3** — it has no async path. The parallel-interface approach keeps it working for now, but once the async path is the live path, Flee will need to be gone. This migration is a good forcing function to finish removing it.
- **Tests are unaffected so far** — the parallel-interface approach (Option A) means all existing tests continue calling `Execute()` synchronously. Async-specific test coverage will be needed once Phase 3 makes `ExecuteAsync` the live path.
- **Callback manager overlap** — there's already a partial callback-based async mechanism (`CallbackManager`, `StartWaitAsync`). Phase 3 should decide whether to unify these or remove the old path outright.
- **SharedArrayBuffer** — even with Quest's own threading removed, the .NET WASM runtime uses `SharedArrayBuffer` internally (for the GC). The COOP/COEP headers required by WasmEditor will be needed for WasmPlayer too. Confirm the CDN target serves these headers.
