# Editing games with `qv`

Status: phases 1 and 2 done: `qv check` and `qv walkthrough`, and in `qv mcp --debug-tools` the tools `check`, `run_walkthrough`, `docs`, `list_functions` and `describe_function`, with server instructions that teach enough ASL to start. Phase 3 is still a plan, and the `llms.txt` idea in phase 2 isn't done.

`qv mcp` lets an agent play a game ([qv-mcp.md](qv-mcp.md)). This plan extends it to fixing and writing games: an author asks an agent to fix a bug it found while playtesting, or to add a room, and the agent checks and plays its change in the same session.

## Why not just edit the XML?

An agent can already edit an `.aslx` file with its own file tools, and for many changes that's fine. What it can't do is tell whether the result is right, short of playing until something breaks. `qv` can add:

- **Checking.** Script parse errors, unknown functions and commands, exits to rooms that don't exist, name clashes.
- **The editor's rules.** A two-way exit needs a matching exit back. Editing a library function makes a local copy that shadows the library from then on. Types are inherited. Some properties only apply once a feature is turned on. XML that ignores these still loads, but behaves oddly in the Editor.
- **The schema.** The editor definitions record which properties each type has, what kind of value each takes and which feature unlocks it. That's hard to work out from reading Core.aslx.
- **Knowing ASL.** Agents have seen some Quest 5 code, but mix it up with other languages and older versions. See "Teaching the agent ASL" below.
- **A short loop.** Write a walkthrough that reproduces the bug, ending in an `assert:` that fails; fix the game; run the walkthrough again to see it pass, and the game's other walkthroughs to check nothing else broke. The test stays in the game for the author.

The plan starts with the parts that make editing the XML directly work well, and only adds editing tools if that turns out to be the bottleneck.

## Phase 1: checking

**`qv check <game>`** loads the game and reports problems, one per line, with the element and attribute each is in. It exits non-zero on errors, so authors can run it in CI, and `--json` gives the same report for programs. Done: see [qv.md](qv.md) for what it reports. The checking itself is `GameChecker` in Engine, so the Editor can show the same problems later. How it works, and what it settled:

- **Loaded in edit mode, parsed as in play mode.** A play-mode load stops at the first script it can't create, while an edit-mode load (the Editor's) keeps going. But edit mode also keeps some things as the Editor shows them, such as a `switch` case's comma-separated values as one expression, so `GameChecker` collects every script's text from the edit-mode game and then parses it with `EditMode` off.
- **Scripts are parsed lazily** (`LazyLoadScript`), so `GameChecker` parses them again with a `ScriptFactory` that defers nothing, nested blocks included. A script command it doesn't recognise looks like a call to a game function, so those are checked against the game's functions too.
- **Expressions** are only evaluated when a script runs. Two `internal` hooks on `WorldModel`, `ExpressionCreated` and `FunctionCallCreated`, hand `GameChecker` every expression and function-call statement as the parse creates them. It parses each expression and resolves the function names it calls.
- **Arguments.** Too many arguments is an error everywhere. Too few is an error for a statement call from ASL 520 on, as `FunctionCallScript` enforces, but fine in an expression, where the rest are null: Core itself relies on that (`QuickParams`).
- **Object and variable names aren't checked.** Whether a name is an object, a local variable or a parameter depends on what has run before. Every report is something that fails when it runs, so there are no warnings.
- **Duplicate element names.** A second element with the same name silently replaces the first, which is how a game overrides a library element, but two of the game's own elements sharing a name is a mistake. `Elements` now records the local elements it displaces.
- **Loader errors**, such as an exit to a room that doesn't exist or an undefined type, stop the loader resolving the game, so when there are any `check` reports only those, and says the scripts weren't checked.
- **Which elements.** Everything except the built-in libraries: the game file and any library the author included, which each report names. A `.quest` file has its libraries inlined, so its copies of Core functions are checked too.
- **Tried on** the repo's example and test games and the 58 `.aslx` games in quest-e2e-tests' corpus, which take about 10 seconds together. 43 corpus games are clean. The rest are real bugs: misspelt calls (`DoesInherit (obj. "spell")`), missing arguments (`MoveObject (coin, )`), empty conditions (`if () { }`), calls to functions the game doesn't have, and old saved copies of Core text-processor functions that call functions gamebooks lack.

**In `qv mcp`:**

- A `check` tool, which returns the same report for the current game's file as it is on disk. Done, with `--debug-tools` for now (later also `--edit`): the report names the game's elements, which a fair playtest shouldn't see.
- `start_game` with no path already re-reads the file from disk when it restarts the game. Its description should say so, so the agent knows that editing the file and restarting picks up the change.
- Walkthroughs in place of replaying commands by hand. `qv walkthrough` and `qv mcp`'s `run_walkthrough` (with `--debug-tools`, and later `--edit`) are done ([qv.md](qv.md), [qv-mcp.md](qv-mcp.md)). The plan had a `replay` argument to `start_game` here; a walkthrough is the same list of commands, saved in the game and able to check results with `assert:`, so it replaces it.

Editing is allowed for `.aslx` only. A `.quest` file is a published package, so `check` can read one but nothing edits it.

## Phase 2: teaching the agent ASL

Done, except `llms.txt`. In order of value:

1. **Fast feedback.** Phase 1. No amount of documentation stops an agent writing invalid ASL. What stops it is being told straight away.
2. **Server instructions** (`McpInstructions`). MCP's `instructions` field is passed to the agent automatically. Without `--debug-tools` they only say how to play. With them, they add a few hundred words: the tools for fixing a game, the edit, `check`, `start_game` loop, a walkthrough with an `assert:` as the test for a fix, and the basics of ASL (blocks, `=` as both assignment and comparison, `<>`, `and`/`or`/`not`, attributes, `this`, `msg`, calling functions), ending with "look a function up with `docs`, and run `check` after every edit". `start_game`'s description now says that restarting re-reads the file.
3. **Built-in docs** (`QuestDocs`). `Cli.csproj` embeds the site's reference, "understanding" and how-to pages (about 800 KB of markdown) in the binary, so the docs match the version of `qv` and work offline. The how-to pages went in after all: the reference links to them constantly, and they answer "how do I make a locked door". Each page is split into sections at its `## ` headings, one per function or command on reference pages. One tool, `docs`: `query` searches, showing an exact heading match (a function or command name) on its own and otherwise the three best sections in full and up to ten more by id; `page` reads a page or `page#section`, and the docs' own links work as ids. With neither, it lists the pages. It's a tool, not MCP resources, because most clients don't read resources unless a person attaches one, and the docs are too long to hand over at once.
4. **Lookups in the game** (`GameFunctions` in PlayerCore). `list_functions` lists the functions the game can call, read from its file as it is now, with parameters and where each is defined: the game file, a library the author included, or with `include_built_in` Quest's own libraries. `describe_function` adds the body and the function's docs entry, and covers engine functions such as `GetBoolean` from the docs alone. Both say when the game's function replaces a library one (`Elements.GetOverriddenLibraryElement`), and when an engine function has the same name: then a statement calls the game's function and an expression the engine's, which Core relies on with `Ask` and `ShowMenu`. That's why `check` doesn't report it. Types aren't covered yet.

   While a game is being edited, its `.aslx` doesn't contain the Core library: Core comes from the copy built into the engine running it, which is the same version as `qv`'s docs. A published `.quest` file has its libraries inlined, frozen at the version it was last saved with. They then load as the game's own elements (`GameLoader` only sets `Library` on elements loaded from an included file), so for a `.quest` file the two can't be told apart.

Separately from `qv`: an `llms.txt` for questviva.com would help agents that aren't using `qv` at all. It's a small addition to the Astro build, not done yet.

## Phase 3: editing tools (if needed)

Only if editing the XML directly turns out to be the bottleneck. Off by default, with `qv mcp --edit`: editing tools show the agent the whole world model, so like `--debug-tools` they don't belong in a fair playtest.

The tools are deliberately broad. WasmEditorBridge has about 137 `[JSExport]` methods, shaped around the editor's screens (`containerPath`, script indices, `AddElse`). Agents do much worse with dozens of small, UI-shaped tools than with a few broad ones, and they write ASL code more reliably than they build a script one tree operation at a time.

| Tool | Does |
|---|---|
| `list_elements(type?)` | The element tree, each marked local or library |
| `get_element(name)` | Its attributes, each marked as set locally, inherited (from which type) or from a library |
| `describe_type(type)` | The properties the Editor shows for it, from the editor definitions, with their value types and the feature that unlocks each |
| `create_element(kind, name, parent, ...)` | Object, room, exit (optionally with its matching exit back), command, function, timer, ... |
| `set_attribute(element, attribute, value)` | |
| `set_script(element, attribute, code)` | Script as ASL code text (`SetScriptCode`), returning parse errors straight away |
| `delete_element` / `move_element` / `undo` | |
| `save` | Writes the `.aslx` |
| `start_game` | As in phase 1, but plays the unsaved in-memory game, so the agent can try a change before saving it |

Editing a library element makes a local copy first, as the Editor does, and the result says so: from then on the game keeps that copy and doesn't pick up later changes to the library.

### Open questions

- **Where the code goes.** Much of the editing logic, and the JSON it returns, is in `WasmEditorBridge`, not EditorCore. An `EditorSession` in EditorCore, the editing counterpart to `HeadlessSession`, would let WasmEditor and `qv` share it instead of keeping two copies.
- **Saving reformats the file.** Does `EditorController.Save()` round-trip a hand-maintained `.aslx` cleanly, or reorder and reformat it? A noisy git diff on every save would be a reason to edit the XML in place instead. Test this before designing the rest of phase 3.
- **The Editor open on the same file.** AppShell keeps the file open, so `qv` and the Editor could overwrite each other's changes. At least warn.
- **Gamebooks** have different element kinds, so the tools need to know which mode the game is in.
- **Playing the unsaved game.** `HeadlessSession.StartAsync` takes game bytes. Saving the in-memory game to bytes before each play is simplest; check how long that takes on a large game.

## Order of work

1. ~~`qv check`, with lazy scripts forced and expressions checked, plus the `check` tool in `qv mcp`~~: done.
2. ~~Server instructions and the `start_game` description~~: done.
3. ~~`docs` with the built-in reference pages~~: done, with the how-to pages too.
4. ~~`replay` on `start_game`~~: done as `qv walkthrough` and `run_walkthrough`.
5. ~~`list_functions` / `describe_function`, with where each definition comes from~~: done.
6. Dogfood: an agent fixes bugs in a few `examples/` games by editing the XML. Decide on phase 3 from what goes wrong.
