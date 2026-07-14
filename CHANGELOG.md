# Changelog

## [6.0.0-beta.39](https://github.com/textadventures/quest/compare/v6.0.0-beta.38...v6.0.0-beta.39) (2026-07-14)


### Features

* **ElectronApp:** Phase 1 desktop app ([#1865](https://github.com/textadventures/quest/issues/1865)) ([acd6769](https://github.com/textadventures/quest/commit/acd6769c66d36b8bd824601986c9634b9f0f5463))
* **WebEditor:** split local vs server-save UI per deployment domain ([#1862](https://github.com/textadventures/quest/issues/1862)) ([4f5ed69](https://github.com/textadventures/quest/commit/4f5ed69fb42b9da4da4d56f86def8966a6781f9c))


### Bug Fixes

* allow "main" scope in PR title lint ([#1864](https://github.com/textadventures/quest/issues/1864)) ([f127012](https://github.com/textadventures/quest/commit/f127012f96bf8bb66958c8b8ad9a752b970b3f36))

## [6.0.0-beta.38](https://github.com/textadventures/quest/compare/v6.0.0-beta.37...v6.0.0-beta.38) (2026-07-13)


### Bug Fixes

* regenerate WebEditor lockfile and harden CI against lockfile drift ([#1857](https://github.com/textadventures/quest/issues/1857)) ([2b15c8d](https://github.com/textadventures/quest/commit/2b15c8d105e3f9eb21a96a6eee5b7f9de6fd8132))

## [6.0.0-beta.37](https://github.com/textadventures/quest/compare/v6.0.0-beta.36...v6.0.0-beta.37) (2026-07-13)


### Features

* add asset picker UI for WebEditor file controls ([#1853](https://github.com/textadventures/quest/issues/1853)) ([e57a2bf](https://github.com/textadventures/quest/commit/e57a2bf3c064f4afb6626ca99999b244b0b4d87e))
* persist WebEditor local drafts in OPFS for Firefox and Safari ([#1854](https://github.com/textadventures/quest/issues/1854)) ([a0f1c3c](https://github.com/textadventures/quest/commit/a0f1c3ce63ed4cb7fcf8480dbae080fff394eb0e))
* preview server-backed editor games in WasmPlayer instead of WebPlayer ([#1852](https://github.com/textadventures/quest/issues/1852)) ([98192d9](https://github.com/textadventures/quest/commit/98192d974e44b5c74359cc819e9d848d5ffb792f))
* rename WebEditor Export to Backup, implement Publish (.quest packaging) ([#1855](https://github.com/textadventures/quest/issues/1855)) ([a6db9d3](https://github.com/textadventures/quest/commit/a6db9d32abba5f43923c0d851f22a38bfb1e8eaa))


### Bug Fixes

* deploy-play uploads to the release-please-created Release instead of recreating it ([#1850](https://github.com/textadventures/quest/issues/1850)) ([67a6d4b](https://github.com/textadventures/quest/commit/67a6d4bf6f1cda9c19e60c1df450fe0024dd763e))
* keep newest release marked "Latest" despite perpetual-beta prerelease flag ([#1849](https://github.com/textadventures/quest/issues/1849)) ([d322a0a](https://github.com/textadventures/quest/commit/d322a0a654be80fb4b85cb9d06653e62459997b4))

## [6.0.0-beta.36](https://github.com/textadventures/quest/compare/v6.0.0-beta.35...v6.0.0-beta.36) (2026-07-12)


### Bug Fixes

* base .aslx file's own duplicate template names lose to the first, not the last ([#1843](https://github.com/textadventures/quest/issues/1843)) ([4c40f89](https://github.com/textadventures/quest/commit/4c40f89507545ff9572ff9b4fa189fc2043698dc))
* NULL/Null literal fails with "Unknown object or variable" in scripts ([#1845](https://github.com/textadventures/quest/issues/1845)) ([d3192fa](https://github.com/textadventures/quest/commit/d3192fa51a9e64497483a1bddd19400c50088512))
* wedged script session could OOM-crash the shared WebPlayer process ([#1844](https://github.com/textadventures/quest/issues/1844)) ([63e52d0](https://github.com/textadventures/quest/commit/63e52d0ba8b2484a474b6152f297197051fc645e))
