# Release channels: stable and beta

Decided 2026-09-17, ahead of 6.0.0 leaving release candidate status. Until then every release was a prerelease, and every release went everywhere: play.questviva.com, textadventures.co.uk, GitHub's "Latest" release, and the `latest` tags on Docker and npm. Once 6.0.0 ships, 6.1 betas must be able to ship without touching any of that.

## Channels

Each version tag is either **stable** (`v6.0.0`, `v6.0.1`) or a **prerelease** (`v6.1.0-beta.1`, `v6.1.0-rc.1`): a tag with a `-` suffix is a prerelease. `.github/scripts/release-channel.sh` makes that call, and every tag-triggered workflow asks it rather than deciding for itself.

| Surface | Stable | Prerelease |
|---|---|---|
| Web app (`deploy-play.yml`) | play.questviva.com (Cloudflare Pages project `play-questviva`) | play-beta.questviva.com (project `play-questviva-beta`) |
| textadventures.co.uk `/questviva` (`finalize-release.yml`) | Redeployed | Not touched |
| GitHub Release (`release-please.yml`, `finalize-release.yml`) | Not flagged as a prerelease, marked Latest | Stays flagged as a prerelease, so `releases/latest` never returns it |
| WebPlayer Docker image (`docker-publish.yml`, on `release/6.0` only) | `:<version>` and `:latest` | None: WebPlayer was removed from `main` in 6.1, so there are no 6.1 images |
| npm package (`npm-publish.yml`) | dist-tag `latest` | dist-tag `beta` |
| NuGet packages | Stable version | Prerelease version (NuGet handles this from the version string alone) |
| Electron installers (`electron-publish.yml`) | "Quest Viva" | "Quest Viva Beta", a separate app that installs alongside the stable one |

A manual `workflow_dispatch` run of `deploy-play.yml` from a branch always deploys to the beta site, because `main` holds the next release's development work. Such a build reports `<short SHA>-<run ID>-<attempt>` as its version (in the browser console, and as the `?v=` cache key on its assets), not a version number. `VERSION` still holds the last release until the next release PR merges, so using it would give every manual deploy the same cache key. The .NET output isn't byte-reproducible, so edge caches would then mix one deploy's WebAssembly binaries with another's hash manifest, and the player would fail its integrity checks. That happened on the second manual beta deploy. The run ID keeps the key unique even when the same commit is deployed twice. Reusing a future tag's version would be worse still, since that build's cached assets would outlive the real release.

The beta site shows a banner (`BetaBanner.svelte`) on its Play and Create tabs, saying that it's a beta and that its data is separate from play.questviva.com's. `deploy-play.yml` turns it on by setting `PUBLIC_BETA_SITE` when it deploys to the beta project.

Because `releases/latest` skips prereleases, the download buttons (AppShell's `download-links.ts`, the docs site's `DownloadButton.astro`) and textadventures.co.uk's `LatestVersionService` keep pointing at the last stable release with no changes of their own.

## Branches

- `main` is where development happens. After 6.0.0 it becomes the 6.1 line and keeps producing prereleases (`prerelease-type: beta`).
- `release/6.0` is created from the `v6.0.0` tag and produces 6.0.x patch releases. It gets its own `release-please-config.json` with the `always-bump-patch` versioning strategy and `prerelease: false`. Fixes land on `main` first and are cherry-picked back.
- `release-please.yml` needs to run on pushes to both `main` and `release/**`, passing `target-branch: ${{ github.ref_name }}`, so each branch keeps its own release PR.
- A tag push runs the workflow files as they are in the tagged commit, so a `v6.0.x` tag runs `release/6.0`'s copies. Fixes to the release workflows need backporting there too.

## Keeping 6.0 users safe from betas

- **Browser storage:** play-beta.questviva.com is a separate origin, so it has its own OPFS drafts, IndexedDB saves and localStorage. A beta can't migrate or damage data that 6.0 needs to read. The flip side is that beta users don't see their stable drafts, so the beta site should say so.
- **Electron:** prerelease builds are packaged as a separate app, "Quest Viva Beta", with its own app ID, install location and user-data directory, so a beta never replaces the stable app or touches its data. See "Beta builds" in [electron-desktop-app.md](./electron-desktop-app.md).
- **ASL version:** if 6.1 introduces a new ASL version, games saved with the beta editor won't open in 6.0 players, including textadventures.co.uk's. The catalog's `maxAslVersion` filter covers the reading side, not authors uploading such games. Only raise the ASL version when a feature actually needs it, and warn authors about this when they publish from the beta editor.

## Docs site

There's one docs site, questviva.com, deployed from `main`, with no per-version copies. Content for features that aren't in a stable release yet is marked where it appears:

- a `<Since version="6.1" />` component that renders "New in 6.1 (beta)" while 6.1 is unreleased. It compares against a single stable-version constant, so bumping that constant at release time removes "(beta)" everywhere. Build it along with the first 6.1 docs change.
- `sidebar.badge` in the frontmatter of pages that are entirely new.
- a `:::caution` note on pages where the behaviour of an existing feature changes.

## 6.0.0 cut runbook

1. Merge a PR whose squash commit message carries a `Release-As: 6.0.0` footer. Its commit type doesn't matter: release-please opens or updates a release PR for any commit with a `Release-As` footer, and lists a `chore:` one under "Miscellaneous Chores". GitHub's default squash message won't keep the footer, so edit it in by hand when merging.
2. Merge the resulting release PR and check that the release lands as stable everywhere in the table above.
3. Create `release/6.0` from `v6.0.0` and give it its stable-versioning `release-please-config.json` (`versioning: always-bump-patch`, so a cherry-picked `feat:` can't turn into 6.1.0). Add `release/**` to `release-please.yml` and `build-and-test.yml` on both branches. release-please titles that branch's release PRs `chore(release/6.0): release 6.0.N`; `pr-title-lint.yml` doesn't enforce scopes, so that needs no allowlist change.
4. On `main`, remove the `6.0.0-rc.*` line from `release-channel.sh`. (Done in the same PR as step 1.)
5. On `main`, set `prerelease-type` to `beta` and land a commit with a `Release-As: 6.1.0-beta.1` footer. Without it, release-please works out the first prerelease after `6.0.0` by itself, which won't necessarily be `6.1.0-beta.1` (see "Releasing" in `CLAUDE.md`). That commit opens the `6.1.0-beta.1` release PR straight away, so leave that PR unmerged until the open items below are done.
6. Update the "perpetual prerelease" wording in `CLAUDE.md`'s Releasing section.

## Open items before the first 6.1 beta

- [x] Create the `play-questviva-beta` Cloudflare Pages project with the same settings as `play-questviva`, and add the play-beta.questviva.com custom domain.
- [x] textadventures.co.uk repo: allow `https://play-beta.questviva.com` through CORS. That took two changes: `CorsUtility.IsAllowedGamesApiOrigin` (the catalog and `/api/game/{id}`), and the playtextadventures.com Cloudflare Worker in `worker/src/index.js`, which serves the game files themselves.
- [x] textadventures.co.uk repo: make `LatestVersionService` channel-aware. A desktop client reporting a prerelease version is offered the highest-versioned published release, prerelease or stable, from GitHub's releases list, so a 6.0.x patch is never offered to a 6.1 beta. Installers are still matched by file extension, which covers the "Quest Viva Beta" names. Stable clients still only see `releases/latest`.
- [x] Separate identity for beta Electron builds (see above).
- [x] A distinct icon for Quest Viva Beta (purple, with a "BETA" band).
- [x] A banner on the beta site saying it's a beta, that its data is separate from play.questviva.com's, and linking back to the stable site.
