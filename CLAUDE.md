# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Quest Viva is an open-source system for creating and playing text adventure games. It's a .NET 9.0 C# application (v6.0.0-beta.9), the modern successor to Quest 5. The main deliverable is a web-based game player built with ASP.NET Core and Blazor.

## Build & Test Commands

```bash
# Build
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Run a single test project
dotnet test tests/EngineTests

# Run a specific test
dotnet test tests/EngineTests --filter "FullyQualifiedName~TestMethodName"

# Run with Docker
docker compose up --build    # WebPlayer on http://localhost:8080
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
WasmPlayer (Blazor WebAssembly)            ─┤
                                            ├─► PlayerCore ─► Engine ─► Utility ─► Common
EditorCore ─────────────────────────────────┘        │
                                                     └─► Legacy
```

**Key projects in `src/`:**

- **Common** — Shared types and interfaces used across all projects
- **Utility** — Helper functions and language utilities
- **Engine** — Core game interpreter: script execution, expression evaluation, game loading, built-in functions. Contains embedded `.aslx` files (game templates, language definitions) in `Core/`
- **PlayerCore** — Game player runtime that wraps Engine. Contains embedded UI resources (HTML, CSS, JS including jQuery UI, jPlayer)
- **EditorCore** — Game editor logic (non-UI)
- **Legacy** — Quest 4 (and earlier) backward-compatibility layer with embedded `.lib`/`.dat` files
- **WebPlayer** — ASP.NET Core web app with Blazor Razor components (`Game.razor`, `Slots.razor`, debugger)
- **WasmPlayer** — WebAssembly variant of the player

**Test projects in `tests/`:** EngineTests, PlayerCoreTests, EditorCoreTests, UtilityTests, LegacyTests

## Key Technical Details

- Target framework: .NET 9.0 with nullable reference types enabled
- Expression evaluation uses `Ciloci.Flee` or `NCalcSync` (migrating from the former to the latter)
- Game files use `.aslx` (XML-based) format; legacy `.asl` format also supported
- Version is stored in the `VERSION` file and embedded as a resource via Common.csproj
- CI runs on GitHub Actions (build-and-test on push/PR to main for `src/` and `tests/` changes)
- The `legacy/` directory contains parts of the Quest 5 codebase (.NET Framework) that haven't yet been migrated to .NET 9 — primarily the desktop apps and ASP.NET web apps. It is not part of the active solution
