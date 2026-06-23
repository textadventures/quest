# Async NCalc → No Threading → WASM Plan

## Background

Quest Viva's Engine currently uses thread-blocking to "pause" script execution while
waiting for player input (menus, questions, `wait` commands). This is done via
`Monitor.Wait()/Pulse()` in `WorldModel.cs`. The goal is to replace this with
`async/await`, which eliminates the need for background threads and makes the Engine
compatible with Blazor WebAssembly.

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

## Phase 5 — WasmPlayer

- A `WasmPlayer` project analogous to `WasmEditor` (Blazor WASM + `[JSExport]` bridge)
- Wire it up with `WasmConfig` (already sets `UseNCalc = true`)
- No threads required — the entire game loop runs on the browser's main thread, yielding at `await` on user-input `TaskCompletionSource` tasks
- JS calls `SetMenuResponse()` etc. from browser events → `TCS.SetResult()` → game coroutine resumes

---

## Notes and risks

- **Flee must be removed or shimmed first** — it has no async path. This migration is a good forcing function to finish removing Flee entirely.
- **Tests will need updating** — existing tests call `Execute()` synchronously. Moving to `ExecuteAsync()` means test infrastructure changes too.
- **Callback manager overlap** — there's already a partial callback-based async mechanism (`CallbackManager`, `StartWaitAsync`). Phase 3 should decide whether to unify these or remove the old path outright.
- **WASM threading** — Blazor WASM has experimental multi-threading (behind a flag, requires `SharedArrayBuffer`), but the goal is to not need it. Single-threaded async is the cleaner target.
