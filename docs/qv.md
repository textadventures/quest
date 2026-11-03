# `qv`, the Quest Viva command-line tool

`qv` plays Quest games without a browser, from any version: Quest 1–4 `.asl`/`.cas` files as well as Quest 5 and Quest Viva `.quest`/`.aslx` ones:

- `qv play <game>` — play in the terminal.
- `qv serve` — let another program drive a game over JSON lines on stdin/stdout ([qv-serve.md](qv-serve.md)).
- `qv mcp [game]` — an MCP server whose tools let an AI agent play games ([qv-mcp.md](qv-mcp.md)).

The source is `src/Cli`; [headless-session.md](headless-session.md) has the design and plans.

## Installing

Every release from 6.1 publishes `qv` two ways. Until 6.1.0 itself ships, only beta releases include it.

**As a dotnet tool**, for anyone with the .NET 10 SDK:

```bash
dotnet tool install -g QuestViva.Cli --prerelease
```

Leave out `--prerelease` once a stable 6.1 is out. `dotnet tool update -g QuestViva.Cli` updates it.

**As a standalone binary**, which needs no .NET install. Each GitHub Release has one per platform:

| Platform | Asset |
|---|---|
| macOS, Apple Silicon | `qv-osx-arm64.tar.gz` |
| Linux, x64 | `qv-linux-x64.tar.gz` |
| Linux, ARM64 | `qv-linux-arm64.tar.gz` |
| Windows, x64 | `qv-win-x64.zip` |
| Windows, ARM64 | `qv-win-arm64.zip` |

Each archive holds just the `qv` (or `qv.exe`) binary. For the latest stable release, `https://github.com/textadventures/quest/releases/latest/download/<asset>` always works; for a beta, use the release's own tag, e.g.:

```bash
curl -L https://github.com/textadventures/quest/releases/download/v6.1.0-beta.2/qv-osx-arm64.tar.gz | tar xz
```

- **Linux** needs ICU, which desktop distributions include but minimal containers often don't (`apt-get install libicu74` on Ubuntu 24.04, `apk add icu-libs` on Alpine).
- **macOS**: the binary is signed with Quest Viva's Developer ID and notarized, so Gatekeeper accepts it even when downloaded with a browser. There's no Intel Mac build, as there's none of the desktop app either; Intel Macs can use the dotnet tool.

## Building from source

```bash
dotnet build src/Cli
dotnet src/Cli/bin/Debug/net10.0/qv.dll play examples/simple.aslx
```
