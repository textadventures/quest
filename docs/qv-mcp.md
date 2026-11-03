# `qv mcp`

`qv mcp <game>` runs an MCP server over stdio whose tools play one game, so an AI agent can play it: to playtest a game and report where it got stuck, for example.

```
qv mcp <game> [--seed N] [--debug-tools]
```

The game is fixed when the server starts, so each game is its own server entry in the client's configuration. Without `--seed` the game gets a random seed, kept for the life of the server: restarting replays the same randomness.

## Tools

| Tool | Arguments | Does |
|---|---|---|
| `start_game` | none | Starts the game, or restarts it, and returns the opening text |
| `send_command` | `input` | Types `input` as a player would — a command, or the answer to a pending menu or yes/no question — and returns the game's response |

Responses are the game's plain text, without the echo of the command. Anything the text doesn't already show is added in the same form a terminal player would see: a menu or question older games expect to show in a dialog, script errors that weren't printed, `[Choose an option by its number.]` when a menu answer was ignored, and `[The game has ended. Call start_game to play again.]`.

### Debug tools

`--debug-tools` adds tools that reach past what a player can do. They're off by default so that a playtesting agent gets a fair game: `evaluate` can read the solution straight out of the world model.

| Tool | Arguments | Does |
|---|---|---|
| `evaluate` | `expression` | Evaluates an ASL expression against the game's current state |
| `tick` | `seconds` | Advances the game clock, firing timers that come due |
| `send_event` | `name`, `param` | Fires an event the game's own JavaScript would send with `ASLEvent` |

## Trying it with Claude Code

```bash
claude mcp add qv-mygame -- qv mcp /path/to/mygame.quest
```

Then ask Claude to play the game. From a quest checkout without the tool installed, use `dotnet /path/to/quest/src/Cli/bin/Debug/net10.0/qv.dll mcp ...` as the command (after `dotnet build src/Cli`).

For a programmatic harness rather than an agent, `qv serve` ([qv-serve.md](qv-serve.md)) exposes the same operations over plain JSON lines.
