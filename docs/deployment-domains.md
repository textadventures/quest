# Deployment domains: WasmEditor / WasmPlayer

Decided 2026-07-01. **Pipeline + placeholder home shipped 2026-07-01** (PR #1800) — see [webeditor-wasm-svelte.md](./webeditor-wasm-svelte.md) for editor implementation status and open questions this plan resolves.

## Motivation

The old Quest WebEditor at textadventures.co.uk requires a Windows server and is being retired in favour of WasmEditor. WasmEditor is intended to be used **offline-first**, with game files stored locally — but the File System Access API it relies on for local directory access isn't fully supported in Safari or Firefox, so a cloud-storage fallback (syncing games to Azure Blob Storage, as the old WebEditor did) is still needed for those browsers and for existing textadventures.co.uk accounts.

WasmPlayer must be same-origin with WasmEditor wherever it's deployed, so that the editor's "Preview" action can load the game without cross-origin complexity.

## Target architecture

```
questviva.com              — unchanged: Astro/Starlight docs + marketing site (site/)
play.questviva.com         — new: WasmEditor + WasmPlayer, local-only
                              (no login, no Azure)
                              /editor — WebEditor
                              /player — WasmPlayer
                              /       — Home: Play/Create landing page
                                        (src/Home/), desktop-app style,
                                        linking to the two paths above
textadventures.co.uk       — existing: WasmEditor + WasmPlayer, same bundle
                              as play.questviva.com, but with cloud sync
                              enabled (same-origin with existing session
                              auth + API). Currently previewed at
                              /questviva.
```

Both deployments run the **same** WasmEditor/WasmPlayer app build. Which storage backends are offered (local files vs. "sign in to sync") is a runtime decision based on origin/capability, not two separate app builds.

## Why a subdomain, not a path

`site/` builds via Astro/npm; WasmEditor/WasmPlayer builds via `dotnet build` (browser-wasm AOT AppBundle). These are separate pipelines, so a subdomain backed by its own Cloudflare Pages project is simpler than path-based routing within one project (which would mean running both build systems and merging outputs). `questviva.com` itself is unaffected.

## Why textadventures.co.uk keeps hosting the app too

Cloud sync needs to be same-origin with the existing auth session/API (no cross-site cookie complexity). Safari/Firefox users — who can't use local directory storage — get full functionality (including image/sound assets) through the cloud-sync path there. This is also the retirement path for the old Windows-hosted WebEditor.

## Avoiding local-vs-synced confusion

Superseded 2026-07-13: rather than a per-game storage indicator, the two domains now hide the storage option that doesn't apply to them. `play.questviva.com` never shows "Save to server" (no backend to save to). `textadventures.co.uk` shows *only* "Save to server" — no local create/open/import/drafts — with a link out to `play.questviva.com/editor/` for anyone who wants local storage instead. This is deliberate: cloud sync on textadventures.co.uk is expected to be deprecated eventually in favour of local-first everywhere, so no new local games should be seeded there. Gated by a `PUBLIC_HAS_SERVER` build-time flag set per deployment in `deploy-play.yml` (see `src/WebEditor/src/routes/open/+page.svelte`).

## Status

Shipped 2026-07-01 (PR #1800, #1802):

- `.github/workflows/deploy-play.yml` builds WasmEditor, WasmPlayer, and WebEditor in GitHub Actions and pushes the combined output to Cloudflare Pages via `wrangler pages deploy` — confirming the build-pipeline choice below worked first try, no Cloudflare-native build needed.
- `src/WebEditor/svelte.config.js` base path is now configurable via `BASE_PATH` (`/editor` for play.questviva.com, `/questviva` still preserved for textadventures.co.uk via `release-webeditor.yml`).
- `play.questviva.com` is live at its final path layout: `/editor` (WebEditor) and `/player` (WasmPlayer). Root was intentionally left empty (404) rather than defaulting to either app — an initial pass put WasmPlayer at root to match the domain name, but that would have meant moving it back to `/player` again once the real homepage landed, breaking any links shared in the interim. Giving both apps stable paths from the start meant root only changed once, when the Home page landed (see Follow-ups below).
- Verified: COOP/COEP headers present on all paths, `/AppBundle/_framework/dotnet.js` loads, custom domain resolves, `/editor` and `/player` both serve correctly.

Subdomain name: settled on `play.questviva.com` (favoured over `app.questviva.com`: shared game links are the more common cold-audience touchpoint than editor entry, and the eventual homepage's Play/Create split covers the editor discovery case).

## Follow-ups (not in this pass)

**Games-catalog API on textadventures.co.uk** — shipped 2026-07-16 (PRs [#63](https://github.com/alexwarren/textadventures.co.uk/pull/63), [#64](https://github.com/alexwarren/textadventures.co.uk/pull/64), [#65](https://github.com/alexwarren/textadventures.co.uk/pull/65)). `GET /api/Catalog?maxAslVersion={n}` on `ApiController` — a JSON sibling of `gamesxml.php` (`LegacyGamesApiController`, the Quest 5 desktop client's endpoint), reusing the same `GetApiTopGames`/`GetApiNewGames`/`GetApiNewUploads`/`GetApiTags` repository calls but returning game `id`s (for `/player/?id=` links) instead of `DesktopDownload` raw-file URLs. `maxAslVersion` is mandatory since each caller (play.questviva.com's WasmPlayer build, Electron's) bundles a specific engine version and shouldn't be shown games written for a newer ASL version than it understands — see the `Versions` dictionary in `src/Engine/GameLoader/GameLoader.cs` in this repo, currently maxing out at `580`. Excludes non-Quest games (`ASLVersion == 0` — HTML/Inform exports also get an `R2Version` once migrated, but aren't ASL games WasmPlayer can run) and caps each tag category at 20 games (`GetApiGamesForTag` has no limit of its own; some tags run 400+ games). CORS scoped to `https://play.questviva.com` plus any `http://127.0.0.1`/`localhost` origin (`CorsUtility.IsAllowedGamesApiOrigin`), covering Electron's per-launch random loopback port without `AllowAnyOrigin`.

Also added in the same pass: `GET /api/Game/{id}` (used by WasmPlayer's `?id=` support and by the new Home page) now fires a `webhome_game_play` analytics event — it previously tracked nothing, unlike `gamesxml.php`/`DesktopDownload`. Both `Game` and `Catalog` accept a `ClientInfo` query model (`source`, `version`, `platform`) recorded as event metadata; `source`/`platform` are free strings rather than enums so future iOS/Android clients can self-report without a server change.

**Play/Create homepage at root** — implemented 2026-07-16 in `src/Home/` (this repo). A small static site (plain HTML/CSS/JS, matching WasmPlayer's existing dark aesthetic by hand rather than pulling in Tailwind/Skeleton — this page never coexists on-screen with an arbitrary game's own CSS the way WasmPlayer's chrome does, so it doesn't need that scoping machinery). Deployed to `play.questviva.com` root via a build+copy step in `deploy-play.yml`, alongside the existing `editor`/`player`/`AppBundle` steps (only that Cloudflare Pages deploy — textadventures.co.uk's separate release-asset deploy path is untouched). Desktop-app style, mirroring the Quest 5 desktop browser's Play/Create tabs (`GameBrowser/PlayBrowser.xaml` on the `v5` branch):

- **Play tab**: category list (Top 20 / Newest / New uploads / per-tag) fetched from `api/Catalog` above, with `source`/`version`/`platform` client info attached (`version` read at runtime via `fetch('VERSION')` — the repo-root `VERSION` file is copied alongside Home's static assets in `deploy-play.yml`; `source` detected via `typeof window.electronApp !== 'undefined'`, the same check `WebEditor`'s `runtime.ts` uses). Each game card links to `/player/?id={id}` — WasmPlayer already handles `?id=` end-to-end, so no player-side work was needed.
- **Create tab**: links to `/editor/`.
- Shared game links (`?url=<url>`) continue to route straight to `/player` rather than through the splash (unchanged from the original plan above).
- `home-config.js` hardcodes `maxAslVersion: 580` — must be bumped by hand alongside any Engine change to `GameLoader.cs`'s `Versions` dictionary; no runtime/build-time mechanism keeps these in sync yet.
- `platform` is read via `window.electronApp?.platform`, which `ElectronApp`'s `preload.ts` doesn't expose yet — reads through automatically once it does.
- Verified locally via `node src/Home/dev-server.mjs` + Playwright: categories render, tab switching works, the flagged non-Quest game and the uncapped-tag-count bug (both fixed server-side above) are absent from the rendered page.
- Not yet done: reusing this same static build as Electron's Phase 3 standalone launcher (see [electron-desktop-app.md](./electron-desktop-app.md#phase-3--standalone-game-launcher)) — Electron's `static-server.ts` currently serves the editor at root, not Home, and needs restructuring first (tracked there, not resolved in this pass).
