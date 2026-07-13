# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Quest Viva is an open-source system for creating and playing text adventure games. It's a .NET 10.0 C# application, the modern successor to Quest 5. There are two player implementations: **WebPlayer** (ASP.NET Core + Blazor Server, Docker-deployed) and **WasmPlayer** (pure browser-WASM, AOT-compiled, no server required — the long-term direction).

## Build & Test Commands

```bash
# Build (full solution)
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Run a single test project
dotnet test tests/EngineTests

# Run a specific test
dotnet test tests/EngineTests --filter "FullyQualifiedName~TestMethodName"

# Run WebPlayer with Docker
docker compose up --build    # WebPlayer on http://localhost:8080

# Build WasmPlayer (Debug — fast interpreter mode)
dotnet build src/WasmPlayer/WasmPlayer.csproj

# Build WasmPlayer (Release — AOT compiled, ~15s)
dotnet build --configuration Release src/WasmPlayer/WasmPlayer.csproj

# Run WasmPlayer dev server
node src/WasmPlayer/dev-server.mjs              # Debug build
node src/WasmPlayer/dev-server.mjs --release    # Release/AOT build
# Open: http://localhost:5175/?url=/examples/simple.aslx
```

Tests use MSTest with Moq (mocking) and Shouldly (assertions).

## Documentation Site (site/)

Separate Astro/Starlight project for the documentation website:

```bash
cd site
npm install
npm run dev       # Dev server
npm run build     # Production build
npm run lint      # ESLint
```

## Architecture

The solution (`QuestViva.sln`) has a layered architecture:

```
WebPlayer (ASP.NET Core + Blazor Server)  ─┐
WasmPlayer (browser-wasm, AOT)             ─┤
                                            ├─► PlayerCore ─► Engine ─► Common
EditorCore ─────────────────────────────────┘        │
                                                     └─► Legacy
```

**Key projects in `src/`:**

- **Common** — Shared types and interfaces used across all projects
- **Engine** — Core game interpreter: script execution, expression evaluation, game loading, built-in functions. Contains embedded `.aslx` files (game templates, language definitions) in `Core/`
- **PlayerCore** — Game player runtime that wraps Engine. Contains embedded UI resources (HTML, CSS, JS including jQuery UI, jPlayer)
- **EditorCore** — Game editor logic (non-UI)
- **Legacy** — Quest 4 (and earlier) backward-compatibility layer with embedded `.lib`/`.dat` files
- **WebPlayer** — ASP.NET Core web app with Blazor Razor components (`Game.razor`, `Slots.razor`, debugger)
- **WasmPlayer** — Pure browser-WASM player (`browser-wasm` target, AOT-compiled). Uses `JSImport`/`JSExport` for JS interop. Serves as a static site with no server-side .NET required. IL trimming is enabled; `WasmPlayer.linker.xml` preserves the Engine assembly (which uses reflection-based type discovery).

**Test projects in `tests/`:** EngineTests, PlayerCoreTests, EditorCoreTests, UtilityTests, LegacyTests

## Git Workflow

`main` is a protected branch (required status check `build_and_test`, required PR review, `enforce_admins` on) — direct pushes are rejected outright, including from repo admins. All changes, however small, go through a feature branch + PR.

## Releasing

Releases are managed by [release-please](https://github.com/googleapis/release-please) (`.github/workflows/release-please.yml`, config in `release-please-config.json` / `.release-please-manifest.json`). There's no manual `VERSION`-bump PR:

1. PRs must have a [Conventional Commits](https://www.conventionalcommits.org/)-prefixed title (`fix:`, `feat:`, `chore:`, etc.) — enforced by `pr-title-lint.yml`, which also restricts the optional scope (e.g. `feat(WebEditor): ...`) to an exact-case allowlist of project names, so scoped changelog entries cluster and capitalize consistently. Since PRs are squash-merged, the PR title becomes the commit message on `main` that release-please parses.
2. Every push to `main` updates a standing "release PR" that bumps `VERSION` and `CHANGELOG.md` from the commits merged since the last release.
3. Merging that release PR *is* the release: release-please tags it directly (e.g. `v6.0.0-beta.36`), which triggers `docker-publish`, `nuget-publish`, and `deploy-play`, which build/push the Docker image, publish NuGet packages, deploy play.questviva.com, and attach a GitHub Release with a changelog generated from the PR titles. The version is also embedded in the binary at build time and displayed at `/about`.

The version stays a perpetual `6.0.0-beta.N` — `release-please-config.json` uses the `prerelease` versioning strategy with `prerelease-type: beta`, so every release just increments the trailing beta number regardless of commit type (fix/feat/breaking), and it will never auto-graduate out of beta. To cut `6.0.0` for real, that needs an explicit `Release-As:` commit footer or a config change.

release-please pushes using the `RELEASE_PAT` repo secret (a PAT with Contents read/write), not the default `GITHUB_TOKEN` — tags pushed with `GITHUB_TOKEN` don't trigger other workflows, so a PAT is required for the tag-triggered workflows to fire.

`./release.sh` still works as a manual fallback if you need to tag from a clean local `main` directly (e.g. if `release-please` is broken). If you forgot to update `VERSION` before running it, the script will fail locally because the tag for the old version already exists.

Same tag also drives the WebEditor deploy on textadventures.co.uk: `deploy-play` builds WebEditor a second time (with `BASE_PATH=/questviva` instead of `/editor`), attaches `AppBundle.zip`/`WebEditor.zip` alongside `WasmPlayer.zip` to the GitHub Release, then dispatches a `webeditor-release` `repository_dispatch` event (using the `TA_DEPLOY_PAT` secret) to `alexwarren/textadventures.co.uk`, which downloads those release assets and redeploys. There used to be a separate `release-webeditor.yml` workflow keyed off `webeditor-v*` tags — that's gone now; regular `v*` releases are the only release cadence for both.

## Key Technical Details

- Target framework: .NET 10.0 with nullable reference types enabled
- Expression evaluation uses `NCalcAsync`
- Game files use `.aslx` (XML-based) format; legacy `.asl` format also supported
- Version is stored in the `VERSION` file and embedded as a resource via Common.csproj
- CI runs on GitHub Actions (build-and-test on push/PR to main for `src/` and `tests/` changes)
- Library packages (`QuestViva.Common`, `QuestViva.Engine`, `QuestViva.Legacy`, `QuestViva.PlayerCore`) are published to NuGet.org on each release tag via the `nuget-publish` workflow
