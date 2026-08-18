---
title: Developers
sidebar:
  order: 20
---

Join In
-------

Quest Viva is an open-source software project, [hosted on GitHub](https://github.com/textadventures/quest).

To compile the source code, you just need the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download) - any editor works (Visual Studio, VS Code, JetBrains Rider), or the `dotnet` CLI on its own. More details on how to do that [here](/developers/source_code).

The engine and server-side code is all C# (.NET 10). The editor's UI (AppShell) is TypeScript and Svelte.


Developer Guidelines
--------------------

### GitHub Issues

[GitHub Issues](https://github.com/textadventures/quest/issues) contains features that need to be implemented and bugs that need to be fixed. If there's an obvious missing feature or bug, open an issue - check it's not a duplicate first. If you're proposing a major new feature, it's worth raising it in [GitHub Discussions](https://github.com/textadventures/quest/discussions) first.

To contribute a change: fork the repository, make your changes on a branch, and open a Pull Request against `main`. All changes go through a PR, however small - `main` is a protected branch. Give the PR a title following [Conventional Commits](https://www.conventionalcommits.org/) format (`fix: ...`, `feat: ...`, etc.) - this is enforced, and becomes the changelog entry once merged.

Questions about how things work are welcome in [GitHub Discussions](https://github.com/textadventures/quest/discussions).

### Translating Quest

If you know a language other than English, why not try translating the English.aslx file? The more languages Quest supports, the better, so please feel free to add any language you can speak!

See [Translating Quest](/advanced-topics/translating_quest) for full information.


Technical Overview
------------------

![](/images/architecture.svg)

Here are the main projects you'll find in `QuestViva.sln`:

- **Common** - Shared types and interfaces used across all projects.
- **Engine** - The core game interpreter: script execution, expression evaluation, game loading, built-in functions. Contains Quest's default game behaviour, default text, and the editor's own UI definitions, all written in ASLX (`Engine/Core/*.aslx`).
- **Legacy** - Backward-compatibility layer for games written for Quest 1.x through 4.x.
- **PlayerCore** - Game player runtime shared by both players, wrapping Engine.
- **EditorCore** - Game editor logic (non-UI).
- **WebPlayer** - ASP.NET Core + Blazor Server web app for playing games server-side.
- **WasmPlayer** - Pure browser-WASM player, AOT-compiled, no server required. This is what powers "play online" today.
- **WasmEditor** - Browser-WASM bridge exposing EditorCore to the AppShell frontend.
- **AppShell** (`src/AppShell/`) - The SvelteKit SPA that is the actual game editor UI, talking to WasmEditor across the JS/WASM boundary.
- **ElectronApp** - Desktop app shell wrapping AppShell, for offline use.

`Engine/Core/*.aslx` (Core.aslx and friends) is inlined into a game file the moment it's saved or published, so a game keeps working unmodified even after the engine's own Core.aslx code has moved on - see the Core Library Semantics notes in `CLAUDE.md` if you're working on those files.
