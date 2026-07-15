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
                              /       — empty for now; will become a
                                        Play / Create homepage, desktop-
                                        app style, linking to the two
                                        paths above
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
- `play.questviva.com` is live at its final path layout: `/editor` (WebEditor) and `/player` (WasmPlayer). Root is intentionally empty (404) rather than defaulting to either app — an initial pass put WasmPlayer at root to match the domain name, but that would have meant moving it back to `/player` again once the real homepage landed, breaking any links shared in the interim. Giving both apps stable paths from the start means root only changes once, when the homepage is built.
- Verified: COOP/COEP headers present on all paths, `/AppBundle/_framework/dotnet.js` loads, custom domain resolves, `/editor` and `/player` both serve correctly.

Subdomain name: settled on `play.questviva.com` (favoured over `app.questviva.com`: shared game links are the more common cold-audience touchpoint than editor entry, and the eventual homepage's Play/Create split covers the editor discovery case).

## Follow-ups (not in this pass)

- **Play/Create homepage at root** — a small standalone page (no WASM needed) linking to `/player` and `/editor`, desktop-app style. Shared game links (`?url=<url>`) should route straight to `/player` rather than through the splash.

Done: template listing now calls a `GetGameTemplates()` WASM bridge export (mirroring `CreateGameFromTemplate`), so it needs no server round-trip and works cross-origin on play.questviva.com.
