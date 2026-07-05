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

## Releasing

1. Update the `VERSION` file on `main` and push
2. Run `./release.sh`

This creates and pushes a git tag (e.g. `v6.0.0-beta.12`) which triggers the `docker-publish` GitHub Actions workflow. That workflow verifies the tag matches the `VERSION` file, then builds and pushes the Docker image tagged with that version. The version is also embedded in the binary at build time and displayed at `/about`.

If you forgot to update `VERSION`, the script will fail locally because the tag for the old version already exists.

## Key Technical Details

- Target framework: .NET 10.0 with nullable reference types enabled
- Expression evaluation uses `NCalcAsync`
- Game files use `.aslx` (XML-based) format; legacy `.asl` format also supported
- Version is stored in the `VERSION` file and embedded as a resource via Common.csproj
- CI runs on GitHub Actions (build-and-test on push/PR to main for `src/` and `tests/` changes)
- Library packages (`QuestViva.Common`, `QuestViva.Engine`, `QuestViva.Legacy`, `QuestViva.PlayerCore`) are published to NuGet.org on each release tag via the `nuget-publish` workflow
