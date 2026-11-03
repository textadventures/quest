# `qv serve` protocol

`qv serve` plays a game for another program — a test harness, an agent framework, a different front-end — over stdin and stdout. Each request is one JSON object on one line; each gets exactly one response object on one line, in order. Only responses are written to stdout. The process exits when stdin closes.

A process plays one game at a time. `start` begins it, and starting again replaces it.

## Requests

Every request has an `op`. An optional `id` (any JSON value) is copied into the response.

| `op` | Fields | Does |
|---|---|---|
| `start` | `game`: path to a `.quest` or `.aslx` file; `seed`: integer, optional (default 1234) | Loads the game and returns its opening text |
| `send` | `input`: string | What a player would type: answers a pending menu (option key, option text or 1-based number) or yes/no question (`yes`, `y`, `true` or `1` is yes), and otherwise runs it as a command |
| `tick` | `seconds`: integer | Advances the game clock, firing any timers that come due |
| `event` | `name`: string; `param`: string, optional | Fires an event the game's own JavaScript would send with `ASLEvent` |
| `evaluate` | `expression`: string | Evaluates an ASL expression against the game |
| `assert` | `expression`: string | Evaluates an ASL expression as true or false |

`send` is everything a player can do. `tick` and `event` stand in for things a browser would do on its own. `evaluate` and `assert` read the world model directly, so a harness evaluating an agent should not pass them through to it.

The same seed and the same requests always give the same responses.

## Responses

`start`, `send`, `tick` and `event` return what that step produced and what the game is waiting for now:

```json
{"id":3,"ok":true,"text":"What is your favourite colour?\n1: Red\n2: Green\n","errors":[],"menu":{"caption":"What is your favourite colour?","options":[{"key":"Red","text":"Red"},{"key":"Green","text":"Green"}],"allowCancel":false,"inline":true},"question":null,"finished":false,"exits":[],"objects":[],"inventory":[],"panesVisible":true,"compassVisible":true}
```

- `text` — the step's output as plain text: trailing whitespace trimmed, runs of blank lines collapsed, and the game's echo of the command left out. `""` if there was none.
- `errors` — messages of script errors the engine logged during the step. Errors in a game's own scripts also appear in `text`, as the game prints them.
- `menu` — the menu waiting for an answer, or `null`. `inline` is true when the game has already drawn the options in `text` (v600+ games); otherwise a front-end should show them itself. An answer that matches no option cancels the menu if `allowCancel`, and is otherwise ignored, leaving the menu pending.
- `question` — `{"caption": ..., "inline": ...}` for a pending yes/no question, or `null`.
- `finished` — the game has ended. Further steps are rejected.
- `exits`, `objects`, `inventory` — what a browser player sees in the compass and side panes, as display names: the exits from the current room (compass directions and named places), the objects visible there, and what the player is carrying. `panesVisible` and `compassVisible` say whether the game shows them at all. They're always included; like `evaluate`, it's up to a harness whether to pass them on to an agent.

Waits and pauses are advanced automatically and `SetTimeout` timers are run before a step returns, so a response always reflects a game waiting for the player.

`evaluate` returns `{"ok":true,"value":...}`: a string, number, boolean or `null`, or the value's text for anything else (objects, lists). `assert` returns `{"ok":true,"result":true|false}`.

Anything that fails returns `{"ok":false,"error":"..."}`: malformed JSON, an unknown `op`, a missing or mistyped field, a game that can't be found or loaded, or a step after the game has finished. The process carries on.

## Example

```
$ qv serve
{"id":1,"op":"start","game":"examples/simple.aslx","seed":1}
{"id":1,"ok":true,"text":"Simple\n\nYou are in a room.\nYou can see a thing.\n","errors":[],"menu":null,"question":null,"finished":false,"exits":[],"objects":["thing"],"inventory":[],"panesVisible":true,"compassVisible":true}
{"id":2,"op":"send","input":"take thing"}
{"id":2,"ok":true,"text":"You can't take it.\n","errors":[],"menu":null,"question":null,"finished":false,"exits":[],"objects":["thing"],"inventory":[],"panesVisible":true,"compassVisible":true}
```
