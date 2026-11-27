# Release channels: stable and beta

Decided 2026-09-17, ahead of 6.0.0 leaving release candidate status. Until then every release was a prerelease, and every release went everywhere: play.questviva.com, textadventures.co.uk, GitHub's "Latest" release, and the `latest` tags on Docker and npm. Once 6.0.0 ships, 6.1 betas must be able to ship without touching any of that.

## Channels

Each version tag is either **stable** (`v6.0.0`, `v6.0.1`) or a **prerelease** (`v6.1.0-beta.1`, `v6.1.0-rc.1`): a tag with a `-` suffix is a prerelease. `.github/scripts/release-channel.sh` makes that call, and every tag-triggered workflow asks it rather than deciding for itself.

| Surface | Stable | Prerelease |
|---|---|---|
| Web app (`deploy-play.yml`) | play.questviva.com (Cloudflare Pages project `play-questviva`), and play-beta.questviva.com too unless a prerelease of a later version is out | play-beta.questviva.com (project `play-questviva-beta`) |
| textadventures.co.uk `/questviva` (`finalize-release.yml`) | Redeployed, for a 6.0.x release only: the editor there stays on 6.0, and only `release/6.0` builds the `WebEditor.zip` it deploys | Not touched |
| GitHub Release (`release-please.yml`, `finalize-release.yml`) | Not flagged as a prerelease, marked Latest | Stays flagged as a prerelease, so `releases/latest` never returns it |
| WebPlayer Docker image (`docker-publish.yml`, on `release/6.0` only) | `:<version>` and `:latest` | None: WebPlayer was removed from `main` in 6.1, so there are no 6.1 images |
| npm package (`npm-publish.yml`) | dist-tag `latest` | dist-tag `beta` |
| NuGet packages, including the `QuestViva.Cli` dotnet tool | Stable version | Prerelease version (NuGet handles this from the version string alone) |
| `qv` binaries (`cli-publish.yml`) | Attached to the release, so `releases/latest/download/qv-<rid>.tar.gz` returns them | Attached to the prerelease only |
| Electron installers (`electron-publish.yml`) | "Quest Viva" | "Quest Viva Beta", a separate app that installs alongside the stable one |

A manual `workflow_dispatch` run of `deploy-play.yml` from a branch always deploys to the beta site, because `main` holds the next release's development work. Such a build reports `<short SHA>-<run ID>-<attempt>` as its version (in the browser console, and as the `?v=` cache key on its assets), not a version number. `VERSION` still holds the last release until the next release PR merges, so using it would give every manual deploy the same cache key. The .NET output isn't byte-reproducible, so edge caches would then mix one deploy's WebAssembly binaries with another's hash manifest, and the player would fail its integrity checks. That happened on the second manual beta deploy. The run ID keeps the key unique even when the same commit is deployed twice. Reusing a future tag's version would be worse still, since that build's cached assets would outlive the real release.

The beta site shows a banner (`BetaBanner.svelte`) on its Play and Create tabs, saying that it's a beta and that its data is separate from play.questviva.com's. `deploy-play.yml` turns it on by setting `PUBLIC_BETA_SITE` when it deploys to the beta project.

A stable release goes to the beta site as well, unless a prerelease of a later version is already out. Otherwise, from 6.1.0 until 6.2.0-beta.1, the beta site would keep the last 6.1 beta, older than the stable site and still calling itself a beta. `.github/scripts/newer-prerelease.sh` prints the newest prerelease tag newer than a stable one, counting a prerelease as older than its own release (`v6.1.0-rc.1` < `v6.1.0`), and `deploy-play.yml` deploys to the beta site too when it prints nothing. That deploy reuses the WasmEditor and WasmPlayer output and builds only AppShell again, with `PUBLIC_BETA_SITE` but not `PUBLIC_BETA_DOCS`, so its help links go to the stable docs that match it. The banner then says there's no beta just now and this is the current version, and that its data is still separate (`src/lib/channel.ts` decides this from the version: a stable one has no `-`). The check also means re-running the workflow for an older stable tag can't roll the beta site back past a newer beta. This only applies to stable tags from `main`: `v6.0.x` tags run `release/6.0`'s copy of the workflow, and 6.1 betas are newer anyway.

Because `releases/latest` skips prereleases, the download buttons (AppShell's `download-links.ts`, the docs site's `DownloadButton.astro`) and textadventures.co.uk's `LatestVersionService` keep pointing at the last stable release with no changes of their own. The beta site's button offers the newest prerelease instead, except while it's running a stable release itself, when it uses `releases/latest` like the stable site.

### Maintenance releases

A stable tag with a newer stable tag out (a 6.0.N released after 6.1.0, for textadventures.co.uk's editor) is a maintenance release. `.github/scripts/newer-stable.sh` prints the newest stable tag newer than a given one, and when it prints one:

- `deploy-play.yml` deploys neither site, which keep the newer release, but still attaches its builds to the GitHub Release, including the `WebEditor.zip` textadventures.co.uk redeploys from.
- `npm-publish.yml` gives it a `release-<major>.<minor>` dist-tag (`release-6.0`) instead of `latest`. npm rejects a dist-tag that is also a valid semver range, such as a bare `6.0`.
- `finalize-release.yml` publishes it with `--latest=false`. Just leaving out `--latest` wouldn't do, since GitHub makes a newly published stable release Latest by default.

NuGet needs nothing: its "latest" is always the highest version. `deploy-play.yml` and `npm-publish.yml` run from the tagged commit, so `release/6.0` has its own copy of this change; `finalize-release.yml` always runs from `main`'s.

## Branches

- `main` is where development happens. After 6.0.0 it becomes the 6.1 line and keeps producing prereleases (`prerelease-type: beta`).
- `release/6.1` is the stable release branch, created from the `v6.1.0` tag, and produces 6.1.x patch releases. Like each release branch, it has its own `release-please-config.json` with the `always-bump-patch` versioning strategy and `prerelease: false`. Fixes land on `main` first and are cherry-picked back.
- `release/6.0`, created from `v6.0.0`, carries on for textadventures.co.uk's editor, which stays on 6.0. A 6.0.x released now is a maintenance release (see "Maintenance releases" above).
- `release-please.yml` needs to run on pushes to both `main` and `release/**`, passing `target-branch: ${{ github.ref_name }}`, so each branch keeps its own release PR.
- A tag push runs the workflow files as they are in the tagged commit, so a `v6.1.x` tag runs `release/6.1`'s copies and a `v6.0.x` tag `release/6.0`'s. Fixes to the release workflows need backporting to both.

## Backporting

Fixes land on `main` first, then go to the stable release branch, `release/6.1`, through a PR of their own (or to `release/6.0`, the same way, for a fix textadventures.co.uk's editor needs):

1. Branch from `release/6.1` and `git cherry-pick -x` the squash commit from `main`. Several related fixes can go in one PR.
2. Title the PR `chore(release/6.1): backport #1234` (listing every PR it brings in).
3. Put the original PR titles, each followed by its PR number, in a commit override block in the PR description, separated by blank lines:

   ```
   BEGIN_COMMIT_OVERRIDE
   fix(Player): stop online games' images shifting the page or scrolling short (#2580)

   fix(Editor): another backported fix (#1235)
   END_COMMIT_OVERRIDE
   ```

4. Squash-merge it, like any other PR.

`pr-title-lint.yml` rejects any other title for a PR into a `release/**` branch, and a `chore(<branch>): backport` PR with no override block.

Why it works this way: release-please only reads commit messages, and the squash commit is titled `chore(release/6.0): backport #1234`, which it leaves out of the changelog. The override block replaces that message with the original titles, so the 6.0.N changelog lists the actual fixes, and a backported fix triggers a release at all. If the block is missing or wrong after merging, editing the merged PR's description and re-running the `release-please` workflow on `release/6.0` fixes it, since release-please reads the description again on every run. That's how 6.0.4 was released after #2756 went in without one.

Don't merge a backport with a merge commit instead. release-please applies the override to every commit that came in with the PR, so the merge commit and each cherry-picked commit would all become the override's messages, and every fix would be listed more than once. (Backports used to be merged that way, without an override, so that release-please read each cherry-picked commit's own title. That stopped working the moment one was squash-merged by mistake.)

## Keeping 6.0 users safe from betas

- **Browser storage:** play-beta.questviva.com is a separate origin, so it has its own OPFS drafts, IndexedDB saves and localStorage. A beta can't migrate or damage data that 6.0 needs to read. The flip side is that beta users don't see their stable drafts, so the beta site should say so.
- **Electron:** prerelease builds are packaged as a separate app, "Quest Viva Beta", with its own app ID, install location and user-data directory, so a beta never replaces the stable app or touches its data. See "Beta builds" in [electron-desktop-app.md](./electron-desktop-app.md).
- **ASL version:** if 6.1 introduces a new ASL version, games saved with the beta editor won't open in 6.0 players, including textadventures.co.uk's. The catalog's `maxAslVersion` filter covers the reading side, not authors uploading such games. Only raise the ASL version when a feature actually needs it, and warn authors about this when they publish from the beta editor.

## Docs site

The docs have the same two channels as the player (decided in #2720, replacing an earlier plan of one site with "New in 6.1" markers):

| Site | Built from | Documents |
|---|---|---|
| questviva.com | the stable release branch (`release/6.0`, then `release/6.1` and so on) | what's in the current stable release |
| beta.questviva.com | `main` | the next release, as it's being built |

So docs for a new feature are written on `main` as if it already shipped: pages are changed in place, with no version markers. The stable site keeps describing the current release until the next one is stable and questviva.com moves to its branch. The homepage and `/download` come from the stable branch, which suits them, since their download links point at the latest stable release.

Both are one Cloudflare Pages project, set up in its dashboard (nothing in the repo sets this):

- **Production branch:** `release/6.1`, serving questviva.com (`release/6.0` until 6.1.0). When a new stable release branch is cut, point production at it.
- **`main`**, serving beta.questviva.com through a custom domain on the `main` branch deployment.
- **`PUBLIC_BETA_SITE=true`** in the Preview environment's variables, so every non-production build is a beta build: `main`, and PR previews.

A beta build (`site/src/channel.mjs`):

- shows a banner above every page (`site/src/components/Banner.astro`), saying these are the docs for the next release and linking to questviva.com.
- has a `noindex` robots meta tag, so search results go to the stable docs.
- uses beta.questviva.com for its canonical links, sitemap and `llms.txt`.

To build one locally: `PUBLIC_BETA_SITE=true npm run build` in `site/`.

The editor links to its own channel's docs. AppShell's help links (`docs-links.ts`) go to beta.questviva.com when it's built with `PUBLIC_BETA_DOCS=true`, which `deploy-play.yml` sets for play-beta.questviva.com and `electron-publish.yml` sets for Quest Viva Beta. Help links are checked against `docs-index.generated.ts`, built from the same branch's `site/`, so each channel's editor links to pages that exist on its docs site. The Electron Help menu (`main.ts`) goes to the beta docs when the app's version is a prerelease.

### Backporting docs fixes

A docs fix that applies to the stable release lands on `main` first and is backported like a code fix (see "Backporting" above). To have it done for you, add the **`backport release/6.1`** label to the PR, before or after merging it. `backport.yml` then cherry-picks the squash commit onto `release/6.1` and opens `chore(release/6.1): backport #N` with the override block already in its description, which you squash-merge. If the cherry-pick conflicts, or the push is refused, it comments on the PR with the steps to do it by hand. The label works for code fixes too, and any `backport <branch>` label targets that branch. It pushes with the `RELEASE_PAT` secret, which needs the Workflows permission: many fixes change `.github/workflows/e2e.yml`, and GitHub refuses those pushes without it.

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

## 6.1.0 cut runbook

The 6.1.0 cut is mostly the 6.0.0 one again, with one difference: `release/6.0` doesn't stop. textadventures.co.uk's editor stays on 6.0 (see [deployment-domains.md](./deployment-domains.md)), so 6.0.x releases can still come after 6.1.0, and today's workflows would treat each one as the newest stable release.

### Before the cut

- [x] **Make 6.0.x releases after 6.1.0 maintenance-only.** Without this, a `v6.0.N` tag pushed after `v6.1.0` would deploy 6.0 to play.questviva.com, become GitHub's Latest release (and with it the download buttons, `/download`, the desktop update banner and `LatestVersionService`), and move npm's `latest` dist-tag back to 6.0.N. The fix is in, on `main` (#2965) and `release/6.0` (#2966) both, as described in "Maintenance releases" above.
- [x] **Docs pass on `main`.** The stable site is built from `main`'s next release branch (see "Docs site" above), so everything `main` says becomes the questviva.com docs. The WebPlayer page stays, since 6.0.x WebPlayer is still supported: it says WebPlayer was removed in 6.1, and that 6.0.x still gets bug fixes. `whats-new.md` covers all of 6.1, and nothing in the docs only makes sense on a beta. Check `whats-new.md` again for anything merged after this.
- [x] **Raise `MAX_ASL_VERSION`** in AppShell's `src/lib/home-catalog.ts` from 600 to 610, so the Play tab shows games saved for 6.1. textadventures.co.uk already records ASL 610 for uploaded games: its submit code reads the version with `QuestViva.PlayerCore` 6.1.0-beta.2, which knows 610, and its catalog API filters on the `maxAslVersion` the Play tab sends.
- [x] **Fix the open editor bugs worth fixing first.** #2891 (a freeze and a spurious "changed outside the editor" message on auto-save) had its likely cause fixed in 6.1.0-beta.4, by making saving a large game much faster. It stays open until the reporter confirms, which doesn't hold up the release.
- [x] **Test the build that ships.** A full e2e run on `main` passed at `e01ebc587` (run 45976628118's commit, e2e run 45975943973), after a week of nightly runs that each failed on a different spec. The quest-e2e-tests corpus wasn't run against it.
- [x] **Tell beta users how to move their work.** play-beta.questviva.com and Quest Viva Beta keep their data apart from the stable ones, and 6.1.0 doesn't move it. Once play-beta.questviva.com is running a stable release, `BetaBanner.svelte` adds that a draft can be moved over with **Backup…** in the editor's File menu and **Import game file** on play.questviva.com's Create tab (`betaBanner.moveDrafts`). It doesn't say so during a beta, since the stable editor can't open what the beta editor saved. Quest Viva Beta needs nothing: its games are already files on disk.
- [x] **Check the Microsoft Store package builds.** It had never been built: `v6.0.x` tags run `release/6.0`'s workflow, which has no `.appx` target. If it failed on the `v6.1.0` tag, `electron-publish.yml` would fail and `finalize-release.yml` would leave the release a draft. A manual stable run on `main` (run 45976628118) built `Quest Viva 6.1.0-beta.4.appx` on Windows, unsigned as the Store expects, along with the other installers. Whether Partner Center accepts its identity values (the `appx` block in `package.json`, particularly `publisherDisplayName`) only matters for the Store submission after the cut, and a wrong value can be fixed in 6.1.1.

### The cut

Done on 2026-11-27: 6.1.0 was released from #2969, `release/6.1` set up in #2972, and steps 5–8 done in the PR that added this line.

1. Merge a PR whose squash commit message carries a `Release-As: 6.1.0` footer. As for 6.0.0, GitHub's default squash message won't keep the footer, so type it in by hand when merging. The PR can be any change, for example the docs pass.
2. Merge the resulting `chore(main): release 6.1.0` PR, and check that the release landed everywhere in the table at the top of this page:
   - play.questviva.com serves 6.1.0, and so does play-beta.questviva.com, whose banner says there's no beta just now.
   - `v6.1.0` is GitHub's Latest release. Its installers and `qv` binaries are attached, so `releases/latest/download/qv-<rid>.tar.gz` works.
   - npm's `latest` and NuGet (including `QuestViva.Cli`) have 6.1.0.
   - `finalize-release.yml` logged "textadventures.co.uk stays on 6.0" and didn't redeploy it.
   - A 6.0 desktop app is offered the update. Quest Viva Beta is offered 6.1.0 too, since `LatestVersionService` offers a prerelease client the highest release. That installs the separate stable app.
3. Create `release/6.1` from `v6.1.0`. Give it `release/6.0`'s `release-please-config.json` (`versioning: always-bump-patch`, `prerelease: false`) and a `.release-please-manifest.json` of `6.1.0`. The workflows already run on `release/**`, so nothing else needs adding. Its `deploy-play.yml` is `main`'s, which sends a 6.1.x release to the beta site too until a 6.2 beta is out.
4. In the docs site's Cloudflare Pages project, point the production branch at `release/6.1`. Check that questviva.com now shows the 6.1 docs with no beta banner, and that beta.questviva.com is still built from `main` and still shows the banner.
5. Create the `backport release/6.1` label. From now on, fixes and docs fixes for the stable release are backported to `release/6.1`, and only fixes the textadventures.co.uk editor needs go to `release/6.0`. Update "Backporting" and "Backporting docs fixes" above, and the Git Workflow section of `CLAUDE.md`, to name `release/6.1`.
6. Move the Claude Code plugin onto the stable release. Its docs links now resolve on questviva.com, and NuGet has a stable `QuestViva.Cli`:
   - in `plugins/questviva/skills/questviva/SKILL.md`, `references/asl-primer.md` and `references/gamebooks.md`, change beta.questviva.com to questviva.com, drop `--prerelease`, and point the `qv` download at the latest release instead of "the newest `6.1.0-beta` release".
   - do the same in the plugin's `.claude-plugin/plugin.json` homepage and `README.md`.
   - remove "Until 6.1.0 itself ships" from `docs/qv.md`, and the matching note on the plugin in `CLAUDE.md`.
7. Seed the 6.2 prerelease line on `main` with a commit carrying a `Release-As: 6.2.0-beta.1` footer. Without it, release-please works out the next prerelease from `6.1.0` by itself, which won't be `6.2.0-beta.1` (see "Releasing" in `CLAUDE.md`). That commit opens the `6.2.0-beta.1` release PR straight away. Leave it unmerged until there's something to ship: until then, play-beta.questviva.com and the beta docs keep 6.1.0.
8. Update `CLAUDE.md`: the stable release branch in the Overview and "Documentation Site" sections, the Releasing section's 6.0.0/6.1 wording, and the `release/6.0` references that now mean `release/6.1`. Update this page's "Branches" section and the textadventures.co.uk row of its table too.

### After the cut

- [ ] textadventures.co.uk repo: raise `QuestStablePlayerMaxAslVersion` in `appsettings.json` from 600 to 610. Until then, its game pages send ASL 610 games to play-beta.questviva.com instead of play.questviva.com (`Config.QuestPlayerUrl`).
- [ ] Submit the Microsoft Store package from the `electron-publish.yml` run's artifacts to Partner Center.
