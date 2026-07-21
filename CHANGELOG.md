# Changelog

## [6.0.0-beta.43](https://github.com/textadventures/quest/compare/v6.0.0-beta.42...v6.0.0-beta.43) (2026-07-21)


### Features

* add smart desktop-app download links to play.questviva.com and questviva.com ([#1911](https://github.com/textadventures/quest/issues/1911)) ([1bf3746](https://github.com/textadventures/quest/commit/1bf3746b4a2766f979d22671b43ac47c417c7c4f))
* **AppShell:** show a dismissible update banner for the desktop app ([#1904](https://github.com/textadventures/quest/issues/1904)) ([f2ec8fa](https://github.com/textadventures/quest/commit/f2ec8fa37080ee0d800a19f230afff2142a970ca))
* **AppShell:** warn that the editor is in preview on the Create tab ([#1914](https://github.com/textadventures/quest/issues/1914)) ([e48f2ca](https://github.com/textadventures/quest/commit/e48f2ca7510af24f630f1f9af82516713356faf9))
* **ElectronApp:** register file associations for .aslx/.quest/.asl/.cas ([#1907](https://github.com/textadventures/quest/issues/1907)) ([a154c0d](https://github.com/textadventures/quest/commit/a154c0d21b4b6c56a2cce364e892a099628f7908))
* **ElectronApp:** sign and notarize macOS builds in CI ([#1905](https://github.com/textadventures/quest/issues/1905)) ([54b963a](https://github.com/textadventures/quest/commit/54b963a5055e5cd6c710e88c44ecd9f9f388b400))


### Bug Fixes

* **AppShell:** editor toolbar tweaks (Home routing, Discord/GitHub links, autocapitalize) ([#1910](https://github.com/textadventures/quest/issues/1910)) ([46889b6](https://github.com/textadventures/quest/commit/46889b64e9455af1d60a2da0f9abb387d7f7eb3e))
* **ElectronApp:** fix Linux taskbar icon, add .deb alongside AppImage ([#1900](https://github.com/textadventures/quest/issues/1900)) ([15238a3](https://github.com/textadventures/quest/commit/15238a30400726673e05e085dc1edc0afbd46f9f))
* **Engine:** preserve insertion order in OrderedDictionary after remove+add ([#1906](https://github.com/textadventures/quest/issues/1906)) ([d4f3941](https://github.com/textadventures/quest/commit/d4f394100863f5f6d8ebb2ce144f11a6ed9062e9))
* send client info (source/version/platform) on game-load requests ([#1903](https://github.com/textadventures/quest/issues/1903)) ([907c8bd](https://github.com/textadventures/quest/commit/907c8bd07e4392e7dc6072d90525a0743e58567d))
* **WasmPlayer:** stop shipping debug symbols that fetch from raw.githubusercontent.com ([#1902](https://github.com/textadventures/quest/issues/1902)) ([f132bef](https://github.com/textadventures/quest/commit/f132bef3e3fdcc1a3d25840580bdcb484bb498eb))

## [6.0.0-beta.42](https://github.com/textadventures/quest/compare/v6.0.0-beta.41...v6.0.0-beta.42) (2026-07-19)


### Features

* **AppShell:** add a Home button to the editor toolbar ([#1894](https://github.com/textadventures/quest/issues/1894)) ([9660dee](https://github.com/textadventures/quest/commit/9660dee6ccc63f9ca6104f8ea87ac034999e7d29))
* **AppShell:** open a local game file from the Play tab ([#1885](https://github.com/textadventures/quest/issues/1885)) ([967864c](https://github.com/textadventures/quest/commit/967864cdb0a8c3ef947970def8daee20c984ac54))
* **AppShell:** replace emoji icons with Lucide and tidy the editor toolbar ([#1895](https://github.com/textadventures/quest/issues/1895)) ([794f20c](https://github.com/textadventures/quest/commit/794f20cdc726b8b0ff9f66840c777f1c09af5be3))
* **ElectronApp:** one-click local play, unrestricted autoplay, sibling resources ([#1886](https://github.com/textadventures/quest/issues/1886)) ([d94e7bb](https://github.com/textadventures/quest/commit/d94e7bb086e5949724e76b8b89bf12536def5e09))


### Bug Fixes

* **AppShell:** keep Delete button visible but disabled instead of hiding it ([#1899](https://github.com/textadventures/quest/issues/1899)) ([a4fcb29](https://github.com/textadventures/quest/commit/a4fcb29d93b143cea01c45e71fc1c43f26a19fb9))
* **AppShell:** preserve language selection when toggling game type on Create tab ([#1897](https://github.com/textadventures/quest/issues/1897)) ([88e64fc](https://github.com/textadventures/quest/commit/88e64fc7c3e483368d793a8a827bfabac4ece1d1))
* **AppShell:** stabilize the toolbar save-status pill ([#1898](https://github.com/textadventures/quest/issues/1898)) ([0d607c4](https://github.com/textadventures/quest/commit/0d607c4aa1609a7834e8c7fa73bf06bce2448fd1))
* **AppShell:** stop centering the Create tab against the full viewport ([#1893](https://github.com/textadventures/quest/issues/1893)) ([0db7825](https://github.com/textadventures/quest/commit/0db78250d6dbb26922bbe962b4d3aaad00cda158))
* drop WebKit from the e2e OPFS job, Linux WebKit has no OPFS support ([#1889](https://github.com/textadventures/quest/issues/1889)) ([ec582bc](https://github.com/textadventures/quest/commit/ec582bc04be56997742abe0700f885a02418cb6b))
* **ElectronApp:** set BrowserWindow icon on Linux ([#1882](https://github.com/textadventures/quest/issues/1882)) ([7f03a89](https://github.com/textadventures/quest/commit/7f03a89e3018e639ec1ff67433d6691b32d8cacd))
* **ElectronApp:** show a confirm dialog instead of silently blocking window close on unsaved changes ([#1891](https://github.com/textadventures/quest/issues/1891)) ([910f176](https://github.com/textadventures/quest/commit/910f1764c3c88566eff7f4b47acb31afe7d6c83e))
* **ElectronApp:** split editor/play Recent lists, focus window on File menu actions ([#1892](https://github.com/textadventures/quest/issues/1892)) ([9ff1d03](https://github.com/textadventures/quest/commit/9ff1d03e6dcac653cbdc0c1f97378420f0b0b3c0))
* **WasmPlayer:** offer saving for games launched from a local file ([#1896](https://github.com/textadventures/quest/issues/1896)) ([3d5e641](https://github.com/textadventures/quest/commit/3d5e641452f7eb39eadd2b5385ca979560d47298))
* **WasmPlayer:** time out unanswered resource-request handoffs instead of hanging forever ([#1890](https://github.com/textadventures/quest/issues/1890)) ([fad24ab](https://github.com/textadventures/quest/commit/fad24abdbffce91d0218a0de711138bbfb438753))

## [6.0.0-beta.41](https://github.com/textadventures/quest/compare/v6.0.0-beta.40...v6.0.0-beta.41) (2026-07-16)


### Features

* **WebEditor:** add Play/Create home screen ([#1881](https://github.com/textadventures/quest/issues/1881)) ([0a8730d](https://github.com/textadventures/quest/commit/0a8730d22afde5df501643fc30c804d371b9aec2))


### Bug Fixes

* **ElectronApp:** Linux app icon and About panel ([#1880](https://github.com/textadventures/quest/issues/1880)) ([6e35c49](https://github.com/textadventures/quest/commit/6e35c490bfc2026075a483be2b2cf42c16f3cd16))
* **ElectronApp:** skip ad-hoc codesign on non-macOS builds ([#1878](https://github.com/textadventures/quest/issues/1878)) ([e6fd86c](https://github.com/textadventures/quest/commit/e6fd86c458fa5c159148106ac0e8139e1ebc7da9))

## [6.0.0-beta.40](https://github.com/textadventures/quest/compare/v6.0.0-beta.39...v6.0.0-beta.40) (2026-07-15)


### Features

* **ElectronApp:** native menu bar (File/Save/Help) + Linux sandbox fix ([#1868](https://github.com/textadventures/quest/issues/1868)) ([92ce64f](https://github.com/textadventures/quest/commit/92ce64f6cb302cbfcda50d03cb661092739a6cdb))
* **ElectronApp:** new game creates its own folder ([#1873](https://github.com/textadventures/quest/issues/1873)) ([369f098](https://github.com/textadventures/quest/commit/369f098fea55dfca88c919e27ce514336f874191))
* **ElectronApp:** recently opened games list ([#1871](https://github.com/textadventures/quest/issues/1871)) ([2938810](https://github.com/textadventures/quest/commit/29388108cb4ada4dab0435577921a95ae1c3f4f4))
* **WebEditor:** autosave instead of explicit Save button ([#1877](https://github.com/textadventures/quest/issues/1877)) ([39e881b](https://github.com/textadventures/quest/commit/39e881b1af15a826cac3e9903ecf23d7a235786a))
* **WebEditor:** default to OPFS local drafts on all browsers ([#1870](https://github.com/textadventures/quest/issues/1870)) ([d588754](https://github.com/textadventures/quest/commit/d588754703943675474bcaa692beed5cee98fb9f))
* **WebEditor:** Open screen cleanup and backup-reminder banner ([#1876](https://github.com/textadventures/quest/issues/1876)) ([f65c8cf](https://github.com/textadventures/quest/commit/f65c8cf42d6c7d1eb73604c2e761f9d9e8c9d40b))


### Bug Fixes

* **ElectronApp:** arm64 Linux, mac x64 drop, ad-hoc signing ([#1866](https://github.com/textadventures/quest/issues/1866)) ([924fe85](https://github.com/textadventures/quest/commit/924fe856477a6978ef7681e40fba611d7849729d))
* **ElectronApp:** improve app icon legibility at small sizes ([#1874](https://github.com/textadventures/quest/issues/1874)) ([8810042](https://github.com/textadventures/quest/commit/8810042a20ddf89f618f6b21472ce0315e92244e))
* unbound LAUNCH_ARGS under macOS's default bash 3.2 in electron.sh ([#1872](https://github.com/textadventures/quest/issues/1872)) ([8d79377](https://github.com/textadventures/quest/commit/8d79377cf33736d5c1989433a11e514f2906051f))

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
