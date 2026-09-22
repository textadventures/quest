---
title: Building from source
description: Build Quest Viva from source, run the players and the editor locally, and run the checks before you open a pull request
sidebar:
  order: 1
---

## Compiling Quest Viva

### What you need

Clone the repository from GitHub:

[https://github.com/textadventures/quest](https://github.com/textadventures/quest)

You'll need the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download), plus the WebAssembly build tools, which the two browser projects need:

```bash
dotnet workload install wasm-tools
```

Any editor works - Visual Studio, VS Code, JetBrains Rider - or you can just use the `dotnet` CLI directly, which is what the examples below use. To work on the editor's user interface or the documentation site as well, you'll also need [Node.js](https://nodejs.org/) (version 22 or later, which is what the build uses).

### Compiling

```bash
dotnet build --configuration Release
```

This builds the whole solution (`QuestViva.sln`). To run the tests:

```bash
dotnet test --configuration Release
```

There are several hundred tests across the test projects (`tests/EngineTests`, `tests/PlayerCoreTests`, `tests/EditorCoreTests`, `tests/LegacyTests`, `tests/WebPlayerTests`) - they should all pass.

To run a single test project, or filter to a specific test:

```bash
dotnet test tests/EngineTests
dotnet test tests/EngineTests --filter "FullyQualifiedName~TestMethodName"
```


## Running the players

### WasmPlayer

```bash
dotnet build src/WasmPlayer/WasmPlayer.csproj
node src/WasmPlayer/dev-server.mjs
```

Then open `http://localhost:5175/?url=/examples/simple.aslx` - the `url` parameter can point at any game file the dev server can reach, and `examples/` in the repository has a few to try.

That's the quick Debug build. The smaller, trimmed build that actually ships is a Release *publish*, not a Release build - only `publish` trims the framework:

```bash
dotnet publish src/WasmPlayer/WasmPlayer.csproj --configuration Release
node src/WasmPlayer/dev-server.mjs --release
```

It's worth testing against that one before you open a pull request that touches the player, because trimming can remove .NET types that a game's scripts reach by reflection.

### WebPlayer

```bash
docker compose up --build
```

This runs WebPlayer at `http://localhost:8080`. Alternatively, run it directly from source:

```bash
dotnet run --project src/WebPlayer/WebPlayer.csproj
```

See [WebPlayer](/publishing/webplayer) for configuration options.


## Running the editor (AppShell)

The editor is a SvelteKit app (`src/AppShell/`) that talks to the engine through a WASM bridge (`src/WasmEditor/`). The quickest way to get both running together is the root-level dev script:

```bash
./dev.sh
```

This builds WasmEditor and WasmPlayer, installs the editor's npm dependencies if they're missing, then starts the AppShell dev server (`http://localhost:5174`) alongside the WasmPlayer dev server it uses for Preview. `./dev.sh --release` does the same with the trimmed Release builds, and `./dev.sh --port 5180` moves both servers, which is handy when you want a second checkout running at the same time.

See `docs/appshell-wasm-svelte.md` in the repository for more on how the pieces fit together.


## The documentation site

This documentation is an [Astro Starlight](https://starlight.astro.build/) project in `site/`, separate from the .NET solution:

```bash
cd site
npm install
npm run dev
```

Every page on the live site has an "Edit page" link at the bottom that opens the right file on GitHub, which is often all you need for a correction.


## Tests and checks

Everything here except the end-to-end scripts runs in continuous integration on every pull request, so it's worth running whichever apply to your change before you open one.

| You changed | Run |
|---|---|
| Any C# | `dotnet build --configuration Release` and `dotnet test --configuration Release` |
| `src/AppShell/` | `npm run lint` and `npm run check` in `src/AppShell` |
| `site/` | `npm run build` in `site`, then the site checks below |
| The player, the editor or the desktop app | the relevant end-to-end scripts |

`npm run check` (type checking) and `npm run lint` (ESLint) check different things, and passing one says nothing about the other - run both.

### Documentation site checks

Run these from `site/`. Three of them inspect the built output, so run `npm run build` first:

```bash
npm run check-function-docs    # every Core library function has a reference entry
npm run check-docs-index       # the editor's docs deep-link index is up to date
npm run build
npm run check-redirects        # redirects resolve, and their anchors exist
npm run check-editor-help-urls # the editor's help links resolve
```

These exist because the editor links into this site: the help link on an element editor tab, and the documentation link on each script command, are both URLs stored in the engine's own files. A page that moves or a heading that's renamed would otherwise quietly start 404ing for authors, with nothing to catch it. If a check fails after you rename a heading, either restore the heading or update the link that points at it.

### End-to-end tests

`tests/e2e/` holds Playwright scripts (`verify-*.mjs`) that drive the real thing - WasmPlayer, the AppShell editor, WebPlayer and the packaged Electron app - against a local dev server. They're too slow to run on every pull request, so they run nightly, and on demand from the Actions tab.

Install the tooling once, inside `tests/e2e/`:

```bash
cd tests/e2e
npm ci
npx playwright install chromium
```

Then run a script against a dev server you already have running - `./dev.sh` gives you both the editor on 5174 and the player on 5175:

```bash
node verify-appshell-code-view.mjs http://localhost:5174
```

If you add a script, add it to `.github/workflows/e2e.yml` in the same change, under the job whose dev server it needs. A required check fails the pull request otherwise - the workflow runs named scripts, so an unlisted one would never run at all.

When one of these fails, check what changed in the user interface before assuming you've found a regression: the most common cause is a script looking for a control that has since been renamed or restructured.
