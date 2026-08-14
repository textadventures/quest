---
name: verify
description: How to runtime-verify changes in this repo (WasmPlayer/WebPlayer/AppShell/ElectronApp) via Playwright, no automated verify harness exists yet
---

# Verifying changes in Quest Viva

No CI-integrated e2e suite. `tests/e2e/*.mjs` is a growing set of ad-hoc,
hand-run Playwright scripts (`quest-e2e` package, `playwright` installed
there) — one file per bug/feature, run directly with `node`, not via `npm
test`. Follow that convention: write a new `verify-<topic>.mjs` there, run it
with `node verify-<topic>.mjs`, leave it checked in for future regression
reruns.

## Browser surface (WasmPlayer / WebPlayer / AppShell)

1. Rebuild whatever project you touched, e.g.:
   `dotnet build src/WasmPlayer/WasmPlayer.csproj` (Debug — fast interpreter,
   good enough for behavior checks; static JS like `wasm-player.js` is a
   plain copy into `bin/Debug/net10.0/browser-wasm/AppBundle/` on every
   build, no separate step needed).
2. Start the relevant dev server, e.g. `node src/WasmPlayer/dev-server.mjs`
   (serves `http://localhost:5175`, reads `?id=`/`?url=` query params).
3. Drive it with `playwright`'s `chromium.launch()` from a script in
   `tests/e2e/`. Use `page.route('**/some/api/**', ...)` to intercept and
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
   src/AppShell && npm run build`; the Home/Play-tab root needs
   `PUBLIC_SHOW_HOME=true` set at that build time, see `docs/deployment-domains.md`).
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
   See `tests/e2e/verify-electron-play-local-file.mjs` for a full working
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
