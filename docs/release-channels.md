# Release channels: stable and beta

Decided 2026-09-17, ahead of 6.0.0 leaving release candidate status. Until then every release was a prerelease, and every release went everywhere: play.questviva.com, textadventures.co.uk, GitHub's "Latest" release, and the `latest` tags on Docker and npm. Once 6.0.0 ships, 6.1 betas must be able to ship without touching any of that.

## Channels

Each version tag is either **stable** (`v6.0.0`, `v6.0.1`) or a **prerelease** (`v6.1.0-beta.1`, `v6.1.0-rc.1`): a tag with a `-` suffix is a prerelease. `.github/scripts/release-channel.sh` makes that call, and every tag-triggered workflow asks it rather than deciding for itself.

Until 6.0.0 ships, that script also treats `v6.0.0-rc.*` as stable, since those release candidates are what play.questviva.com serves today. Remove that line as part of the 6.0.0 cut (see below).

| Surface | Stable | Prerelease |
|---|---|---|
| Web app (`deploy-play.yml`) | play.questviva.com (Cloudflare Pages project `play-questviva`) | play-beta.questviva.com (project `play-questviva-beta`) |
| textadventures.co.uk `/questviva` (`finalize-release.yml`) | Redeployed | Not touched |
| GitHub Release (`release-please.yml`, `finalize-release.yml`) | Not flagged as a prerelease, marked Latest | Stays flagged as a prerelease, so `releases/latest` never returns it |
| Docker image (`docker-publish.yml`) | `:<version>` and `:latest` (what play.textadventures.co.uk's prod compose file pulls) | `:<version>` and `:beta` |
| npm package (`npm-publish.yml`) | dist-tag `latest` | dist-tag `beta` |
| NuGet packages | Stable version | Prerelease version (NuGet handles this from the version string alone) |
| Electron installers (`electron-publish.yml`) | "Quest Viva" | "Quest Viva Beta", a separate app that installs alongside the stable one |

A manual `workflow_dispatch` run of `deploy-play.yml` from a branch always deploys to the beta site, because `main` holds the next release's development work.

Because `releases/latest` skips prereleases, the download buttons (AppShell's `download-links.ts`, the docs site's `DownloadButton.astro`) and textadventures.co.uk's `LatestVersionService` keep pointing at the last stable release with no changes of their own.

## Branches

- `main` is where development happens. After 6.0.0 it becomes the 6.1 line and keeps producing prereleases (`prerelease-type: beta`).
- `release/6.0` is created from the `v6.0.0` tag and produces 6.0.x patch releases. It gets its own `release-please-config.json` with the default versioning strategy and `prerelease: false`. Fixes land on `main` first and are cherry-picked back.
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

1. Merge a `fix:`/`feat:` PR whose squash commit message carries a `Release-As: 6.0.0` footer. release-please skips creating a release PR when no changelog-visible commit has landed since the last release, and GitHub's default squash message won't keep the footer, so edit it in by hand when merging.
2. Merge the resulting release PR and check that the release lands as stable everywhere in the table above.
3. Create `release/6.0` from `v6.0.0` and give it its stable-versioning `release-please-config.json`. Add `release/**` to `release-please.yml`.
4. On `main`, remove the `6.0.0-rc.*` line from `release-channel.sh`.
5. On `main`, set `prerelease-type` to `beta` and land a changelog-visible commit with a `Release-As: 6.1.0-beta.1` footer. Without it, release-please works out the first prerelease after `6.0.0` by itself, which won't necessarily be `6.1.0-beta.1` (see "Releasing" in `CLAUDE.md`).
6. Update the "perpetual prerelease" wording in `CLAUDE.md`'s Releasing section.

## Open items before the first 6.1 beta

- [ ] Create the `play-questviva-beta` Cloudflare Pages project with the same settings as `play-questviva`, and add the play-beta.questviva.com custom domain.
- [ ] textadventures.co.uk repo: add `https://play-beta.questviva.com` to `CorsUtility.IsAllowedGamesApiOrigin`.
- [ ] textadventures.co.uk repo: make `LatestVersionService` channel-aware, so a beta Electron client (identified by the prerelease suffix in `ClientInfo.version`) is told about newer betas as well as newer stable releases. Beta installers are named `Quest Viva Beta-<version>-...`, so any asset matching there needs to allow for that.
- [x] Separate identity for beta Electron builds (see above).
- [x] A distinct icon for Quest Viva Beta (purple, with a "BETA" band).
- [ ] A banner on the beta site saying it's a beta, that its data is separate from play.questviva.com's, and linking back to the stable site.
