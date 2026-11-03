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
| `start_game` | `game`: path, optional (only when the server wasn't given a game) | Loads the game at `game` and returns its opening text. Without `game`, restarts the current game. `~/` paths are expanded; relative paths are resolved against the server's working directory (for Claude Code, the project it was started in), so absolute paths are safest |
| `send_command` | `input` | Types `input` as a player would — a command, or the answer to a pending menu or yes/no question — and returns the game's response |

Responses are the game's plain text, without the echo of the command. Anything the text doesn't already show is added in the same form a terminal player would see: a menu or question older games expect to show in a dialog, script errors that weren't printed, `[Choose an option by its number.]` when a menu answer was ignored, and `[The game has ended. Call start_game to play again.]`.

When the game shows its compass, each response also ends with the exits a browser player would see lit, such as `[Exits: north, south, up]`. Many games leave exits out of their room descriptions because the compass shows them, so without this an agent has to guess directions. `--text-only` leaves the line out, for a game played purely from its text.

### Debug tools

`--debug-tools` adds tools that reach past what a player can do. They're off by default so that a playtesting agent gets a fair game: `evaluate` can read the solution straight out of the world model.

| Tool | Arguments | Does |
|---|---|---|
| `evaluate` | `expression` | Evaluates an ASL expression against the game's current state. Not available for Quest 4 and earlier |
| `tick` | `seconds` | Advances the game clock, firing timers that come due |
| `send_event` | `name`, `param` | Fires an event the game's own JavaScript would send with `ASLEvent` |

## Trying it with Claude Code

Register it once, then ask Claude to play a game by its path:

```bash
claude mcp add qv -- qv mcp
```

From a quest checkout without the tool installed, build it with `dotnet build src/Cli` and register `dotnet /path/to/quest/src/Cli/bin/Debug/net10.0/qv.dll mcp` instead. Add `--debug-tools` to let Claude inspect the game's state, for example while debugging a game rather than playtesting it.

For a programmatic harness rather than an agent, `qv serve` ([qv-serve.md](qv-serve.md)) exposes the same operations over plain JSON lines.
