# `qv`, the Quest Viva command-line tool

`qv` plays Quest games without a browser, from any version: Quest 1–4 `.asl`/`.cas` files as well as Quest 5 and Quest Viva `.quest`/`.aslx` ones:

- `qv play <game>` — play in the terminal.
- `qv serve` — let another program drive a game over JSON lines on stdin/stdout ([qv-serve.md](qv-serve.md)).
- `qv mcp [game]` — an MCP server whose tools let an AI agent play games ([qv-mcp.md](qv-mcp.md)).
- `qv walkthrough <game> [name]` — run a game's built-in walkthroughs, for example in CI (below).
- `qv check <game>` — find problems in a game's scripts without playing it (below).
- `qv publish <game>` — make a `.quest` package of a game, as the Editor's Publish does (below).
- `qv preview <game>` — play a game in your browser, served from its folder, reloading when its files change (below).

The source is `src/Cli`; [headless-session.md](headless-session.md) has the design and plans.

## `qv walkthrough`

```
qv walkthrough <game> [name] [--seed N] [--quiet]
```

Runs one of the game's walkthroughs, or with no name every one of them, each from a fresh start. It prints each walkthrough's transcript and then a `PASS` or `FAIL` line; `--quiet` prints only those lines. It exits non-zero if any walkthrough fails, so authors can run it in CI.

The steps are the ones the player's walkthrough runner understands: a command, `menu:<key>` and `answer:yes`/`answer:no` for a pending menu or question, `assert:<expression>`, `event:<name>;<param>`, `delay:<milliseconds>`, and `label:`, which is a note. For a Quest 4 or earlier game an assert is a condition in that syntax, such as `got <lamp>`. It differs from the player's runner in two ways:

- **Anything that means the walkthrough no longer does what its author wrote fails it**: a failed assert, one that can't be evaluated, a script error, a menu answer that isn't an option, a step that doesn't match what the game is waiting for, or the game ending before the last step. The player's runner only stops at a failed assert.
- **`delay:` advances the game clock instead of waiting.** In the player a delay is real time between steps, during which timers keep running. Here, after every step from then on, the clock moves on by the delay, a whole second at a time.

The seed is fixed, so a walkthrough that passes once passes every time: the walkthrough's own `seed` attribute (or its parent's, for a sub-walkthrough), or else 1234, as the editor's Play and Record use. `--seed` overrides both.

## `qv check`

```
qv check <game> [--json]
```

Finds problems in a game without playing it, and prints each with where it is, such as `object 'room', attribute 'description': Unknown function 'Prnt' in 'Prnt("x")'`. Many of these would otherwise only show when a player reached the script, because the engine parses a script only when it first runs. It reports:

- script commands and functions that don't exist, including inside nested blocks
- expressions that don't parse, or are empty, as in `if () { }`
- functions given more arguments than they take, or, as a statement from ASL 520 on, fewer
- two of the game's own elements with the same name, where the second silently replaces the first
- two verbs that run the same attribute, often a game verb that repeats a built-in one such as `buy`. The game shows an error as it starts and leaves some verbs out of objects' verb lists
- a function that returns a value but has the name of one built into the engine, such as `CapFirst`, whatever the case. An expression calls the engine's function instead, so this one's return value is never used
- anything that stops the game loading, such as an exit to a room that doesn't exist. Then nothing else is checked until that's fixed

It checks the game file and any library included alongside it, not the libraries built into Quest. Object and variable names aren't checked: whether a name is an object or a variable can depend on what has run before.

It exits 0 when there are no problems, 1 when there are, and 2 when it can't check the game at all (the file doesn't exist, or is from Quest 4 or earlier, which it doesn't support). `--json` prints the result as JSON for programs: `{"problems": [{"message", "kind", "element", "attribute", "filename"}], "loaded"}`, leaving out fields that don't apply.

## `qv publish`

```
qv publish <game> [--format quest|html|zip] [-o <out>] [--force] [--player <folder>] [--cdn-version <v>]
```

Publishes an `.aslx` game as the Editor's Publish does, in one of its three formats:

- `quest` (the default): a `.quest` package, the game with its included libraries inlined plus the files in its folder, to upload to textadventures.co.uk or play in any Quest player. `game.aslx` makes `game.quest`.
- `html`: one web page with the package in it, which loads the player from jsDelivr. `game.aslx` makes `game.html`. The player is pinned to `qv`'s own version, which has to be one published on npm, so this needs a released `qv` (one with the player built in, as for `qv preview`). A `qv` built from a checkout refuses unless `--cdn-version` names a published version.
- `zip`: that page with a copy of the player beside it, to put on any web host. `game.aslx` makes `game-html.zip`. The player is the one built into `qv`, or a checkout's own build, or the folder `--player` names. If the game has cover art, it goes beside the page for link previews.

The page is the player's own, with the game's title, description and IFID in its head; the Editor's two HTML exports use the same code (`PlayerExport` in PlayerCore). `-o` writes the output somewhere else.

It checks the game first, as `qv check` does, and if that finds problems it prints them and doesn't publish, unless `--force` is given. It then prints where it wrote the package and every file it put in it, and warns about:

- a file the game refers to that isn't in the package, such as a `picture` attribute or a `picture ("map.png")` in a script. It goes by the extension, so it covers the usual images, sounds, scripts, stylesheets and fonts.
- a game with no `gameid`. That's its IFID, which identifies the game on sites such as IFDB. The package is given a new one, and the warning shows the `<gameid>` line to add so the next publish keeps it. `qv publish` doesn't change the `.aslx` itself.

Which files go in: every file at the top of the game's folder, except `.aslx` and `.xml` files (the game and its libraries, which are inlined), hidden files, macOS `._` files, and earlier publish output: any `.quest` file, and the game's own `.html`, `.zip` and `-html.zip`. Subfolders aren't included, as in the Editor. The package doesn't include the game's walkthroughs, which players don't use.

It exits 0 when it publishes, 1 when it doesn't because of problems, and 2 when it can't publish the game at all, such as a `.quest` file, which is already published.

## `qv preview`

```
qv preview <game> [--port N] [--seed N] [--player <folder>]
```

Serves the game in Quest's browser player on this computer, and prints the URL to open: `http://127.0.0.1:<port>/?source=qv`. It's for what playing in the terminal can't show: the game's JavaScript and stylesheets, its layout, pictures, sounds and panes. It runs until Ctrl+C.

- The game's folder is served beside the player, so its pictures, sounds and `<javascript>` files load as they would once published, and a library of the author's own beside it (`<include ref="mylib.aslx"/>`) loads too. Hidden files and anything outside the folder aren't served.
- Changing a file in the folder reloads the page, which starts the game again. So the author can keep the page open while they, or an agent, edit the game.
- For an `.aslx`, Debug is available, as in the Editor's preview.
- It only listens on 127.0.0.1, so other computers can't reach it. `--port` picks the port, and otherwise it uses a free one. `--seed` seeds the game's random numbers, as for `qv play`, so a browser run can be compared with a headless one.

The player is built into `qv` (the published WasmPlayer, about 5.6 MB), so it's the same version as the engine `qv play` uses. A `qv` built from a checkout without it uses that checkout's own WasmPlayer build instead, and `--player` serves a WasmPlayer `AppBundle` folder you give it.

## Installing

Every release from 6.1 publishes `qv` two ways. Until 6.1.0 itself ships, only beta releases include it.

**As a dotnet tool**, for anyone with the .NET 11 SDK:

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
dotnet src/Cli/bin/Debug/net11.0/qv.dll play examples/simple.aslx
```

`qv preview` needs a WasmPlayer build too: build `src/WasmPlayer` once, and a checkout's `qv` serves it. To embed the player as a release does, publish WasmPlayer and pass its `AppBundle` as `QvPlayerBundle`:

```bash
dotnet publish src/WasmPlayer/WasmPlayer.csproj -c Release
cp -r src/WasmPlayer/bin/Release/net11.0/browser-wasm/AppBundle /tmp/qv-player
dotnet build src/Cli -c Release -p:QvPlayerBundle=/tmp/qv-player
```

It's copied aside because building the whole solution rebuilds WasmPlayer's `AppBundle` untrimmed.
