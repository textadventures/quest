---
name: verify
description: How to runtime-verify changes in this repo (WasmPlayer/AppShell/ElectronApp) with Playwright Test specs in tests/e2e, which the nightly e2e workflow runs
---

# Verifying changes in Quest Viva

`tests/e2e/` (`quest-e2e` package) holds one Playwright Test spec per
bug/feature, left checked in. `tests/e2e/playwright.config.mjs` has one
project per directory, matching the server the specs need:

| Project | Needs |
|---|---|
| `appshell` | AppShell dev server on 5174 (and WasmPlayer on 5175 for Play-tab specs) |
| `appshell-firefox` | AppShell on 5174 with a Release WasmEditor (`WASM_CONFIG=Release`), Firefox |
| `wasmplayer` | WasmPlayer dev server on 5175 |
| `electron` | a built `src/ElectronApp` (no server) |
| `site` | the docs site served on 4321 |

A new test is a `<topic>.spec.mjs` file in the right directory, using `test()`
from `@playwright/test` (copy a neighbour). Run it from `tests/e2e` with
`npx playwright test --project=<project> <topic>`. The server URL comes from
the `baseURL` fixture (override with `APPSHELL_URL` / `WASMPLAYER_URL` /
`SITE_URL`). Nothing to wire up: each nightly e2e job runs its whole project.
The required `check_e2e_manifest` PR check (`node
tests/e2e/check-workflow-manifest.mjs`) fails if a spec sits outside every
project directory, and a `PostToolUse` hook in `.claude/settings.json` runs it
whenever you write a spec, so a misplaced one is flagged straight away.

Before running a spec, check the port is free (`lsof -ti tcp:5175`) or already
serving the build you mean to test: a dev server you start on a taken port
fails to bind, and the spec then quietly runs against whatever else is there.

## Browser surface (WasmPlayer / AppShell)

1. Rebuild whatever project you touched, e.g.:
   `dotnet build src/WasmPlayer/WasmPlayer.csproj` (Debug — fast interpreter,
   good enough for behavior checks; static JS like `wasm-player.js` is a
   plain copy into `bin/Debug/net11.0/browser-wasm/AppBundle/` on every
   build, no separate step needed).
2. Start the relevant dev server, e.g. `node src/WasmPlayer/dev-server.mjs`
   (serves `http://localhost:5175`, reads `?id=`/`?url=` query params).
3. Drive it with `playwright`'s `chromium.launch()` from a spec in
   `tests/e2e/<project>/`. Use `page.route('**/some/api/**', ...)` to intercept and
   inspect outgoing requests (query params etc.) without needing a real
   backend — `route.fulfill()` with a synthetic response is enough when you
   only care about the request shape, not full game boot.

## Electron surface

1. Rebuild the touched dotnet project(s) first (WasmPlayer/WasmEditor), then:
   `cd src/ElectronApp && npm run build` — runs `tsc` (preload.ts/main.ts
   etc.) and `copy-static.mjs`, which copies `src/AppShell/build` (editor UI)
   + WasmEditor's and WasmPlayer's `AppBundle` output into
   `resources/app-static/{editor,AppBundle,player}`. **`copy-static.mjs`
   does *not* rebuild AppShell** — if `src/AppShell/build` doesn't exist yet
   or is stale relative to a change you made there, build it first (`cd
   src/AppShell && npm run build`).
2. Drive with Playwright's `_electron` launcher:
   ```js
   import { _electron as electron } from 'playwright';
   import { createRequire } from 'node:module';
   const electronAppDir = join(import.meta.dirname, '..', '..', 'src', 'ElectronApp');
   const electronExecutablePath = createRequire(join(electronAppDir, 'package.json'))('electron');
   const app = await electron.launch({
       executablePath: electronExecutablePath,
       args: [electronAppDir, `--user-data-dir=${mkdtempSync(...)}`],
   });
   const win = await app.firstWindow(); // the editor/Home window
   ```
   (In a spec under `tests/e2e/electron/`, that path has one more `'..'`.)
   See `tests/e2e/electron/play-local-file.spec.mjs` for a full working
   example (native dialog stubbing via `app.evaluate(({dialog}) => {...})`,
   waiting for a second BrowserWindow via `app.waitForEvent('window')`).
3. **Gotcha**: the player `BrowserWindow` (opened via
   `window.electronApp.player.openWindow(...)`, see `ipc/player.ts`)
   deliberately has **no preload script** — it renders untrusted game
   content, so `window.electronApp` is undefined there. Only the
   editor/Home window has the preload bridge. Don't expect
   `window.electronApp.*` to exist inside a player window.
4. **Gotcha**: to intercept a request fired immediately on a *new* window's
   first navigation (e.g. a player window's boot-time fetch), register the
   route at the browser-context level — `app.context().route(...)` — *before*
   triggering whatever opens that window. A route added on the page/window
   object after `app.waitForEvent('window')` resolves can already be too
   late.

## What NOT to do

Don't just run `dotnet test`/`npm run build`/typecheck and call it verified
— those confirm compilation, not runtime behavior. Actually launch the dev
server or the Electron app and observe the real request/DOM/behavior.
