# Single-file, CDN-linked self-hosted export

**Status: designed, not started.** Design notes below so this can be picked up later without re-deriving the reasoning.

## Motivation

Self-hosting a Quest game today (`site/src/content/docs/guides/hosting.md`, "Host WasmPlayer yourself") means: download a ~31 MB `WasmPlayer.zip` release asset, extract it, drop in the `.quest` file, hand-edit `quest-config.js` to point `defaultGameUrl` at it, then upload the whole folder. The goal here is to shrink that down to "download and upload one small HTML file" for authors who don't need a fully offline copy.

Two pieces make that possible:
1. **Publish the WasmPlayer runtime to npm** so a CDN (jsDelivr) can serve it — authors no longer host the 31 MB runtime themselves.
2. **Embed the game bytes directly in the exported HTML** instead of fetching them separately — removes the last local file dependency and the CORS question entirely.

### A limitation ruled out along the way

Loading the actual built `AppBundle/index.html` via `file://` in the browser (and reading the guard code at `wasm-player.js:1689-1711,1881`) confirmed that embedding the game does **not** unlock double-click-from-disk playback — the .NET WASM runtime itself loads its own `.wasm`/`.dat` files via `fetch()`/dynamic `import()`, which every browser refuses under a `file:` origin regardless of what's inlined elsewhere in the page. This is universal to browser-WASM engines (Unity WebGL, Godot HTML5 hit the same wall) and is explicitly out of scope here — the export this document describes still needs to be served over http(s) (any host, or the CDN itself), same as today, just with far less to upload. True offline double-click is what the (separately deferred) Electron packaging path is for.

### Scope for the first pass

The CDN-linked single-file export mechanism only (npm publish + embedded-game boot source + the generated HTML). Automating today's full offline-zip flow is a separate, smaller follow-up — it already has an easy path (reuse the `.quest` bytes the existing `Publish…` flow produces, plus the WasmPlayer copy already deployed alongside AppShell at `/player/` for the Preview button, zipped with `fflate` the same way `backupGame()` already does) — not designed further here.

## 1. Publish WasmPlayer to npm

- **Package name: `@textadventures/quest-viva-wasmplayer`** — scoped under the existing `textadventures` npm org (no new org needed), but specific to this artifact rather than a generic `quest-viva` name. The org scope itself is what gives room for future unrelated packages later, so the package name doesn't need to hedge for that.
- New workflow `.github/workflows/npm-publish.yml`, triggered `push: tags: ['v*']` — modeled directly on `nuget-publish.yml`'s existing pattern:
  - Cross-check `GITHUB_REF_NAME` against the `VERSION` file (`nuget-publish.yml:24-32` is the exact pattern to copy).
  - `dotnet build src/WasmPlayer/WasmPlayer.csproj -c Release` (same command `deploy-play.yml:48-49` already runs). This duplicates ~15s of build time in a second parallel tag-triggered job rather than adding cross-workflow artifact sharing — consistent with how `nuget-publish.yml`/`deploy-play.yml`/`electron-publish.yml` already run as independent parallel jobs off the same tag (see root `CLAUDE.md`'s Releasing section).
  - Generate a `package.json` inside the built `AppBundle` output (none exists there today) stamped with `name`, `private: false`, and `version` from `$VERSION` — analogous to how NuGet packs already inject `-p:Version=${{ env.VERSION }}` at pack time rather than hand-maintaining a version in a checked-in file.
  - `npm publish --access public` (required for a scoped package to be public) from inside that directory, authenticated via a **new `NPM_TOKEN` repo secret** — needs to be created in GitHub repo settings before this workflow can run; not something automatable from inside the repo.
- Verification: after the first publish, `curl -I https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@<version>/wasm-player.js` and a `.wasm` file under `_framework/` — confirms correct `content-type`/CORS. (Already spot-checked this general mechanism live against the unrelated `sql.js` npm package — both jsDelivr and unpkg served `.wasm` with `content-type: application/wasm` and `access-control-allow-origin: *` with no extra config needed.)

## 2. New boot source in wasm-player.js: embedded game bytes

- All existing boot sources (`?source=editor`, `?source=local`, `?id=`, `?url=`/`defaultGameUrl`, file picker — full trace in `wasm-player.js:1880-1992`) already converge on one sink: `startGame(bytes, filename, ...)` (`wasm-player.js:652`) → `initWasmPlayer` → `Bridge.Initialise(byte[], string)` (`WasmPlayerBridge.cs:27`), which takes a raw `Uint8Array`/`byte[]` — never base64, that's only used for individual asset resources (`wasm-player.js:171-191`, `WasmPlayerBridge.cs:522`).
- Add one new branch to the boot IIFE, checking for `window.QuestVivaEmbeddedGame` (a base64 string, set by a `<script>` tag before `wasm-player.js` loads — same pattern `quest-config.js` already uses for `window.QuestVivaConfig`, see `generated/index.html:8`). Decode to `Uint8Array` and call the existing `startGame(bytes, filename)` directly — no other code changes needed, since every other source already funnels through that same function.
- The `file:` protocol guard (`wasm-player.js:1881`) stays exactly where it is, checked first, untouched.

## 3. New export template: `<base href>`-based single-file HTML

- Generated `index.html` head starts with `<base href="https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@<pinned-version>/">`, before any other resource tag. Per URL-resolution spec, this single line:
  - Redirects every existing relative `<script src>`/`<link href>` (`wasm-player.js`, `playercore.js`, `player.js`, `chrome.css`, `playercore.css`, jQuery libs, `logo.svg`/`favicon.svg`) to the CDN, with no per-tag rewriting needed.
  - Also redirects `wasm-player.js`'s own internal relative `fetch('playercore.htm')` (`wasm-player.js:435`) and `fetch('grid.js')` (`wasm-player.js:384`) to the CDN, since plain `fetch()` calls resolve against `document.baseURI`, which `<base href>` changes.
  - Does **not** affect (and doesn't need to) `wasm-player.js:436`'s `import('./_framework/dotnet.js')` — dynamic `import()` from a classic script resolves against that script's own URL regardless of `<base href>`, so once `wasm-player.js` itself loads from the CDN, its own relative import already follows it there.
- A `<script>window.QuestVivaEmbeddedGame = "<base64>";</script>` before the `wasm-player.js` tag. No `quest-config.js` needed in this flavor — nothing else is local.
- **First implementation step should be to verify the `<base href>` reasoning empirically** against a real browser (build one by hand, serve over `http://` via e.g. `npx serve`, confirm via network-request inspection that every asset actually resolves to the CDN and nothing 404s locally) before wiring it into a generated build step — this is a correct-per-spec but fairly obscure corner of URL resolution, worth confirming directly rather than trusting the reasoning alone.
- Should live as a new standalone script, `src/WasmPlayer/scripts/export-embedded.mjs`, taking a `.quest`/`.aslx` file path and an optional version (defaults to the repo's `VERSION`), producing one `index.html`. Buildable and testable via CLI immediately, independent of any editor UI work.

## 4. Editor UI hook

Once 1–3 are proven via the standalone script, wire it into AppShell as a new File-menu item next to `Publish…` (`Toolbar.svelte:166`), reusing the exact same `.quest` package bytes that flow already produces (`CreatePublishPackage`/`AddPublishAsset`, `editor-store.ts:571-593`) as the input to the template from step 3, downloaded via the same `triggerDownload` helper `Publish…`/`Backup…` already use. Small, additive — no new architecture. Deliberately last, so the underlying mechanism is already verified working before it's exposed in the UI.

## 5. Docs

Add a new subsection to `site/src/content/docs/guides/hosting.md` (under "Host WasmPlayer yourself"): "Publish a single file (no download)". Cover: how to get the file (script initially, editor button once built), that it needs jsDelivr to be reachable (mention unpkg as a documented alternative CDN — same npm publish serves both), and to use a pinned version rather than `@latest` since the WasmPlayer JS/UI shell can change between releases even though a saved `.quest` file's own script behavior is frozen (that guarantee, per root `CLAUDE.md`'s Core.aslx section, is about engine script semantics — it says nothing about the WasmPlayer shell itself). State the `file://` limitation plainly rather than let anyone assume embedding solved it.

## Non-goals (explicitly out of scope)

- Automating the full offline-zip export — follow-up, not this pass.
- True `file://` double-click support — not achievable for a browser-WASM app.
- Steam/Electron packaging — deferred separately.

## Verification (once built)

1. `curl -I` jsDelivr URLs for the published package after first release (content-type/CORS check, same method already spot-checked against `sql.js`).
2. Hand-built embedded-game `index.html` pointed at a real (or dry-run) published version, served via `npx serve`, opened in a browser: confirm via network-request inspection that the game boots with no separate fetch for game data, and that runtime assets load from the CDN with none 404ing against the local origin.
3. Re-open the same file via `file://` and confirm the existing friendly error still fires — regression check that the new boot branch doesn't run ahead of the `location.protocol === 'file:'` guard.

## Open questions for whoever picks this up

- Confirm `@textadventures/quest-viva-wasmplayer` as the final package name (alternative considered: unscoped `quest-viva-wasmplayer`, or a more generic `@textadventures/quest-viva`).
- `NPM_TOKEN` repo secret needs to be created before `npm-publish.yml` can run.
