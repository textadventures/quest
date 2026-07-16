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
                              /player — WasmPlayer
                              /       — WebEditor, PUBLIC_SHOW_HOME=true:
                                        Play/Create Home landing page when
                                        nothing's loaded, the editor canvas
                                        once a game is; /open (Create tab)
                                        and /play/[id] (game detail) live
                                        alongside it — see Follow-ups below
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

Two follow-up fixes found by exercising the real deployed catalog against the Home page below (PRs [#66](https://github.com/alexwarren/textadventures.co.uk/pull/66), [#67](https://github.com/alexwarren/textadventures.co.uk/pull/67)): `GetApiNewGames`/`GetApiNewUploads` ordered by raw `DateAdded`, which understates recency for a game that sat unlisted/WIP before actually going live — now orders by `PublishDate ?? DateApproved ?? DateAdded`, the same fallback `Views/Games/View.cshtml` already uses for the game page's own "Published {date}" display (a shared repository method, so this also fixes the same staleness in `gamesxml.php`'s desktop feed). And the "New uploads" category was dropped from `Catalog()` entirely — once both categories shared the same date ordering, its only remaining difference from "Newest Games" was including `sandpit`/`wip`/`uncategorised` games, which isn't useful for a general-audience homepage the way it may have been for the desktop client's more community-involved audience (`gamesxml.php` keeps "New uploads" unchanged).

**Play/Create homepage at root** — first shipped 2026-07-16 as a standalone static site in `src/Home/`; **merged into WebEditor 2026-07-16** and `src/Home/` deleted. The standalone version worked (catalog rendering, tab switching, the two catalog bugs it surfaced were fixed server-side — see above) but three real gaps showed up fast: the Create tab was a cross-origin link out to a *different app* rather than feeling like "you're already in the editor"; the Play tab had no way to open a local file/URL (that lived only on WasmPlayer's own start screen at `/player/`); and adding a game-detail view (click a card → description/rating/tags/textadventures.co.uk link, with an explicit Play button, rather than launching straight into WasmPlayer) needed real routing/state, which a hand-rolled static page doesn't have. Rather than build a second bespoke app next to WebEditor's existing SvelteKit SPA, Home's Play/Create UI now lives inside it as more routes:

- **Root (`/`)** — always the Play tab (game catalog) when `PUBLIC_SHOW_HOME` is true, full stop. The editor canvas originally lived at root too (shown whenever `isLoaded`), but that broke as soon as Play-tab navigation existed alongside it: `isLoaded` deliberately never resets just from navigating elsewhere (a refresh of `/` should resume your session), so clicking "Play" or a game detail page's "Back to Play" while something was still loaded in the background kept landing back in the editor instead. A `?view=play` query-param override was tried first and fixed that, but then the *layout's* separately-computed tab-bar-visibility check (which didn't know about the override) went out of sync with what the page actually rendered, hiding the tab bar while the Play catalog showed underneath. **Resolved by giving the editor its own route, `/edit`** (moved verbatim out of the old root `+page.svelte`) — root now has no `isLoaded` branching at all, and every place that used to `goto(base || "/")` after loading a game (eight call sites in `open/+page.svelte`, plus the `?game=` server-load entry point) now targets `/edit` instead. `PUBLIC_SHOW_HOME=false` (textadventures.co.uk) unaffected: root there is purely a redirect (`/edit` if loaded, else `/open`), matching its previous behaviour exactly.
- **`/open`** — unchanged (Create tab's content), except its post-load navigation now targets `/edit`. The Play/Create tab bar (`HomeTabs.svelte`) lives in the root layout, visible on every route except `/edit` — a plain pathname check now, no `isLoaded` involved.
- **`/play/[id]`** — new game-detail route, fetching `api/GameDetails/{id}` (added specifically for this — see below). Its "Back to Play" link is a plain `{base}/`, same as the tab bar's Play link — no query param needed now that root unconditionally means Play.
- Catalog/detail fetches live in `$lib/home-catalog.ts`; `source`/`version`/`platform` client info uses `PUBLIC_WEBEDITOR_VERSION` (already baked in at build time — no more runtime `fetch('VERSION')` workaround) and the same `isElectron()` check `runtime.ts` already had. `platform` is declared optional on `window.electronApp` (`electron-types.d.ts`) since `ElectronApp`'s `preload.ts` doesn't populate it yet — reads through automatically once it does.
- `MAX_ASL_VERSION = 580` in `home-catalog.ts` still needs bumping by hand alongside any Engine change to `GameLoader.cs`'s `Versions` dictionary — same caveat as the static version had, just moved.
- Local dev: `dev.sh` now starts only WebEditor (port 5174, already proxying `/player` to WasmPlayer via `vite.config.ts`) and WasmPlayer — no separate Home dev server, no `/api`/`/game-resource` proxy workarounds (those existed only to route around Home's dev server not knowing about WasmPlayer's own dev-only quest-config.js override). `--api-proxy` (simulating textadventures.co.uk) also now sets `PUBLIC_SHOW_HOME=false` to match.
- `deploy-play.yml`'s play.questviva.com build drops `BASE_PATH=/editor` (defaults to root) and adds `PUBLIC_SHOW_HOME=true`; the assemble step copies WebEditor's build straight to `deploy/` root instead of `deploy/editor/`. textadventures.co.uk's `/questviva` build is unaffected. `build-and-test.yml`'s `build_webeditor`/`build_electron` jobs and `electron-publish.yml` gained matching `PUBLIC_SHOW_HOME=true` (Electron was already root-based, so this needed no `static-server.ts`/`main.ts` changes at all — resolving the Phase 3 restructuring question in [electron-desktop-app.md](./electron-desktop-app.md#phase-3--standalone-game-launcher) as a side effect).
- Verified locally via `dev.sh` + Playwright + `svelte-check`.
- Still not done: "open a local file/URL from the Play tab" without a hop to `/player/`'s own picker (could reuse the existing Preview `BroadcastChannel` handoff — not built yet), and Electron's recently-played list / native menu wiring for the new Play tab.
