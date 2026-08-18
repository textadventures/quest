---
title: Building from Source
sidebar:
  order: 1
---


Compiling Quest Viva
---------------------

This describes how to download and compile the Quest Viva source code.

### Download

Clone the repository from GitHub:

[https://github.com/textadventures/quest](https://github.com/textadventures/quest)

You'll also need the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download). Any editor works - Visual Studio, VS Code, JetBrains Rider - or you can just use the `dotnet` CLI directly, which is what the examples below use.

### Compiling

```bash
dotnet build --configuration Release
```

This builds the whole solution (`QuestViva.sln`). To run the tests:

```bash
dotnet test --configuration Release
```

There are around 200 tests across the various test projects (`tests/EngineTests`, `tests/PlayerCoreTests`, `tests/EditorCoreTests`, `tests/LegacyTests`, `tests/WebPlayerTests`) - they should all pass.

To run a single test project, or filter to a specific test:

```bash
dotnet test tests/EngineTests
dotnet test tests/EngineTests --filter "FullyQualifiedName~TestMethodName"
```


Running the players
--------------------

### WebPlayer

```bash
docker compose up --build
```

This runs WebPlayer at `http://localhost:8080`. Alternatively, run it directly from source:

```bash
dotnet run --project src/WebPlayer/WebPlayer.csproj
```

See [WebPlayer](/publishing/webplayer) for configuration options.

### WasmPlayer

```bash
dotnet build src/WasmPlayer/WasmPlayer.csproj
node src/WasmPlayer/dev-server.mjs
```

Then open `http://localhost:5175/?url=/examples/simple.aslx`. For a faster AOT-compiled build closer to what actually ships, add `--configuration Release` to the build command and `--release` to the dev server command instead.


Running the editor (AppShell)
-------------------------------

The editor is a SvelteKit app (`src/AppShell/`) that talks to the engine through a WASM bridge (`src/WasmEditor/`). The quickest way to get both running together is the root-level dev script:

```bash
./dev.sh
```

This builds WasmEditor and WasmPlayer, then starts the AppShell dev server (`http://localhost:5174`) alongside the WasmPlayer dev server it uses for Preview. See `docs/appshell-wasm-svelte.md` in the repository for more on how the pieces fit together.
