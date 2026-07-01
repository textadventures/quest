# Deployment domains: WasmEditor / WasmPlayer

Decided 2026-07-01. **Pipeline + placeholder home shipped 2026-07-01** (PR #1800) — see [webeditor-wasm-svelte.md](./webeditor-wasm-svelte.md) for editor implementation status and open questions this plan resolves.

## Motivation

The old Quest WebEditor at textadventures.co.uk requires a Windows server and is being retired in favour of WasmEditor. WasmEditor is intended to be used **offline-first**, with game files stored locally — but the File System Access API it relies on for local directory access isn't fully supported in Safari or Firefox, so a cloud-storage fallback (syncing games to Azure Blob Storage, as the old WebEditor did) is still needed for those browsers and for existing textadventures.co.uk accounts.

WasmPlayer must be same-origin with WasmEditor wherever it's deployed, so that the editor's "Preview" action can load the game without cross-origin complexity.

## Target architecture

```
questviva.com              — unchanged: Astro/Starlight docs + marketing site (site/)
play.questviva.com         — new: WasmEditor + WasmPlayer, local-only
                              (no login, no Azure) — bare homepage has
                              Play / Create sections, desktop-app style
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

Rather than hiding features per domain, show a persistent, unambiguous per-game storage indicator in the editor UI — "On this device" vs "In your Quest account" — so it's always clear where a given game lives.

## Status

Shipped 2026-07-01 (PR #1800):

- `.github/workflows/deploy-play.yml` builds WasmEditor, WasmPlayer, and WebEditor in GitHub Actions and pushes the combined output to Cloudflare Pages via `wrangler pages deploy` — confirming the build-pipeline choice below worked first try, no Cloudflare-native build needed.
- `src/WebEditor/svelte.config.js` base path is now configurable via `BASE_PATH` (root for play.questviva.com, `/questviva` still preserved for textadventures.co.uk via `release-webeditor.yml`).
- `play.questviva.com` is live: root serves the existing WebEditor open/create flow, `/player/` serves the existing WasmPlayer landing page. This is the **placeholder home** — no unified Play/Create landing page yet, per agreed scope.
- Verified: COOP/COEP headers present on all paths, `/AppBundle/_framework/dotnet.js` loads, custom domain resolves.

Subdomain name: settled on `play.questviva.com` (favoured over `app.questviva.com`: shared game links are the more common cold-audience touchpoint than editor entry, and the eventual bare homepage's Play/Create split covers the editor discovery case).

## Follow-ups (not in this pass)

- **Unified Play/Create homepage** — replace the placeholder (WebEditor at root, WasmPlayer at `/player/`) with the desktop-app-style landing page described above.
- **Template list needs a WASM export** — `/open`'s "create new game from template" calls `getGameTemplates()` → `/api/editor/games` on textadventures.co.uk (`src/WebEditor/src/lib/filesystem/server-adapter.ts`), which isn't reachable cross-origin from play.questviva.com. "Open existing file/folder" is unaffected. Fix: add a `GetGameTemplates()` WASM bridge export (mirroring the existing `CreateGameFromTemplate`) so template listing needs no server round-trip.
