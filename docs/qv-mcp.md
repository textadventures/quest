# `qv mcp`

`qv mcp` runs an MCP server over stdio whose tools play Quest games, so an AI agent can play them: to playtest a game and report where it got stuck, for example.

```
qv mcp [game] [--seed N] [--text-only] [--debug-tools]
```

There are two ways to run it:

- **Without a game**, the agent loads games itself by passing a path to `start_game`. One server entry covers every game: register it once and ask the agent to play `~/games/mygame.quest`.
- **With a game**, the server plays only that one. `start_game` takes no path, so the agent can't switch to another game. This suits a fair playtest or an evaluation harness.

Each game gets its seed when it's loaded: the `--seed` one, or else a random one. Restarting keeps it, so a restart replays the same randomness.

## Tools

| Tool | Arguments | Does |
|---|---|---|
| `start_game` | `game`: path, optional (only when the server wasn't given a game) | Loads the game at `game` and returns its opening text. Without `game`, restarts the current game. Either way the file is read again, so restarting picks up edits to it. `~/` paths are expanded; relative paths are resolved against the server's working directory (for Claude Code, the project it was started in), so absolute paths are safest |
| `send_command` | `input` | Types `input` as a player would — a command, or the answer to a pending menu or yes/no question — and returns the game's response |

Responses are the game's plain text, without the echo of the command. Anything the text doesn't already show is added in the same form a terminal player would see: a menu or question older games expect to show in a dialog, script errors that weren't printed, `[Choose an option by its number.]` when a menu answer was ignored, and `[The game has ended. Call start_game to play again.]`.

Each response also ends with what a browser player sees beside the text:

- When the game shows its compass, the exits that would be lit, such as `[Exits: north, south, up]`. Many games leave exits out of their room descriptions because the compass shows them, so without this an agent has to guess directions.
- When the game shows its side panes and has a status pane, its lines, such as `[Status: Score: 2 | Health: 100%]`.

`--text-only` leaves both out, for a game played purely from its text.

### Debug tools

`--debug-tools` adds tools that reach past what a player can do, for debugging and fixing a game. They're off by default so that a playtesting agent gets a fair game: `evaluate` can read the solution straight out of the world model.

The server's instructions, which clients pass to the agent, say how to play. With `--debug-tools` they also say how to fix a game (edit the `.aslx` file, `check` it, restart with `start_game`, test with a walkthrough), with a short introduction to ASL and a pointer to `docs`.

| Tool | Arguments | Does |
|---|---|---|
| `evaluate` | `expression` | Evaluates an ASL expression against the game's current state. Also returns anything the game printed while evaluating it, and any script errors: an error in a function the expression calls makes that call return null. Not available for Quest 4 and earlier |
| `tick` | `seconds` | Advances the game clock, firing timers that come due |
| `send_event` | `name`, `param` | Fires an event the game's own JavaScript would send with `ASLEvent` |
| `docs` | `query` or `page`, optional | Looks things up in Quest Viva's documentation, built into `qv` so it matches its version: the reference, the "understanding" pages and the how-to guides. `query` searches, giving a command or function's own entry when it's named exactly; `page` reads a page or one section (`reference/functions/string#template`; the docs' links work as they are). With neither, lists the pages |
| `list_functions` | `filter`, `include_built_in`, `game`, optional | Lists the functions the game can call, read from its file as it is now, with their parameters and where each is defined: the game file (saying when it replaces a library function), a library the game includes, or with `include_built_in` Quest's own libraries |
| `describe_function` | `name`; `game`, optional | One function's parameters, where it's defined, its body and its docs entry. Covers functions built into the engine, such as `GetBoolean`, from the docs |
| `check` | `game`, optional | Checks the game's file as it is on disk for problems, as `qv check` does ([qv.md](qv.md)), without restarting the game. For an agent editing the game: it sees edits, and `start_game` restarts the game to play them |
| `run_walkthrough` | `name`, `transcript`, `game`, optional | Restarts the game, re-reading its file, and runs one of its walkthroughs, as `qv walkthrough` does ([qv.md](qv.md)). Returns the last step's response and whether it passed, or the step it failed at and why; `transcript` returns every step's response. Leaves the game where the walkthrough stopped, so the agent can carry on from there. With no `name`, lists the game's walkthroughs. A game's own walkthrough is often its solution, which is another reason this is a debug tool |
| `publish` | `format`, `output`, `force`, `game`, optional | Publishes the game as `qv publish` does, as a `.quest` package by default, or with `format` `html` or `zip` ([qv.md](qv.md)): checks it first and doesn't publish if there are problems, unless `force` is set. Returns where it wrote the package, the files it included and any warnings |
| `preview` | `game`, optional | Serves the game in the browser player, as `qv preview` does ([qv.md](qv.md)), and returns the URL, for the author to open or an agent's browser tool to look at. The server lasts as long as `qv mcp`, keeps its URL, and follows the current game |

`game`, on the tools that read the game's file (and `publish` and `preview`), is the path of a game to use instead of the current one, so an agent can check a game before starting it. It becomes the current game, and `start_game` with no path then starts it. A server started with a game only accepts that game's path.

## Trying it with Claude Code

Register it once, then ask Claude to play a game by its path:

```bash
claude mcp add qv -- qv mcp
```

From a quest checkout without the tool installed, build it with `dotnet build src/Cli` and register `dotnet /path/to/quest/src/Cli/bin/Debug/net10.0/qv.dll mcp` instead. Add `--debug-tools` to let Claude inspect the game's state, for example while debugging a game rather than playtesting it.

For a programmatic harness rather than an agent, `qv serve` ([qv-serve.md](qv-serve.md)) exposes the same operations over plain JSON lines.

Tools for fixing and writing games, not just playing them, are planned in [qv-editing.md](qv-editing.md).
