---
name: verify
description: How to runtime-verify changes in this repo (WasmPlayer/AppShell/ElectronApp) via Playwright scripts in tests/e2e, and wire new ones into the nightly e2e workflow
---

# Verifying changes in Quest Viva

`tests/e2e/` (`quest-e2e` package) holds one Playwright test per bug/feature,
left checked in. The suite is moving from standalone scripts to Playwright
Test (`tests/e2e/playwright.config.mjs`), one project per directory:

- **Converted so far: `site/`, `wasmplayer/` and `electron/`.** A new test
  there is a `<topic>.spec.mjs` file in that directory, using `test()` from
  `@playwright/test` (copy a neighbour). Run it with
  `npx playwright test --project=wasmplayer <topic>` from `tests/e2e`.
  Nothing to wire up: CI runs the whole directory. For `site` and
  `wasmplayer`, the server URL comes from the `baseURL` fixture (override with
  `SITE_URL` / `WASMPLAYER_URL`).
- **Not converted yet: AppShell.** These are still standalone
  `verify-<topic>.mjs` scripts, run with `node verify-<topic>.mjs <baseUrl>`;
  follow that convention for them until they move, and wire each one in as
  described below.

**Every new `verify-*.mjs` script must also get a step in `.github/workflows/e2e.yml`**, in
the job whose dev server it needs (`appshell_chromium` or
`appshell_opfs_firefox`), copying a neighbouring step's `node
verify-<topic>.mjs <baseUrl>` + `working-directory: tests/e2e` shape. The
nightly run only runs listed scripts, and the required `check_e2e_manifest` PR
check (`node tests/e2e/check-workflow-manifest.mjs`) fails otherwise. A
`PostToolUse` hook in `.claude/settings.json` runs that check whenever you
write a `verify-*.mjs` file, so its "Not wired into e2e.yml" message right
after creating a script is expected: add the step, don't ignore it.

## Browser surface (WasmPlayer / AppShell)

1. Rebuild whatever project you touched, e.g.:
   `dotnet build src/WasmPlayer/WasmPlayer.csproj` (Debug — fast interpreter,
   good enough for behavior checks; static JS like `wasm-player.js` is a
   plain copy into `bin/Debug/net11.0/browser-wasm/AppBundle/` on every
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
