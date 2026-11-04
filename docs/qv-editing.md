# Editing games with `qv`

Status: plan. Nothing here is built yet.

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

**`qv check <game.aslx>`** loads the game and reports problems, one per line, with the element and attribute each is in. It exits non-zero on errors, so authors can run it in CI, and `--json` gives the same report for programs.

It needs to go further than loading the game:

- **Scripts are parsed lazily** (`LazyLoadScript`). Loading a game doesn't parse its scripts. In play mode a bad script throws the first time it runs, and in edit mode it becomes a `FailedScript`. `check` has to force every script to parse.
- **Expressions** are only evaluated when a script runs, so a misspelt function name inside one (`Prnt ("x")`) goes unnoticed until then. `check` should parse each expression and resolve the function names it calls against the game's functions. Object and variable names can't always be resolved statically, so `check` only warns about those, and only when it's sure.
- **Loader errors** (`WorldModel.Errors`) and the checks the Editor already makes: name clashes (`CanAdd`), exits whose `to` doesn't exist, types that aren't defined.

Most of this belongs in Engine or EditorCore, not the CLI, so the Editor can show the same problems later.

**In `qv mcp`:**

- A `check` tool, which returns the same report for the current game.
- `start_game` with no path already re-reads the file from disk when it restarts the game. Its description should say so, so the agent knows that editing the file and restarting picks up the change.
- Walkthroughs in place of replaying commands by hand. `qv walkthrough` and `qv mcp`'s `run_walkthrough` (with `--debug-tools`, and later `--edit`) are done ([qv.md](qv.md), [qv-mcp.md](qv-mcp.md)). The plan had a `replay` argument to `start_game` here; a walkthrough is the same list of commands, saved in the game and able to check results with `assert:`, so it replaces it.

Editing is allowed for `.aslx` only. A `.quest` file is a published package, so `check` can read one but nothing edits it.

## Phase 2: teaching the agent ASL

In order of value:

1. **Fast feedback.** Phase 1. No amount of documentation stops an agent writing invalid ASL. What stops it is being told straight away.
2. **Server instructions.** MCP's `instructions` field is passed to the agent automatically. `qv mcp` sends none today. A few hundred words: ASL isn't JavaScript or C#; the basics of `if`, `foreach` and `msg`, and how to refer to an object's attribute; `=` is both assignment and comparison; look a function up with `docs` before using it, and run `check` after editing. Keep it short enough that agents read it.
3. **Built-in reference docs.** `site/src/content/docs/reference/` is 30 plain `.md` pages, about 300 KB: script commands, functions by category, attributes, elements. `Cli.csproj` includes them in the binary at build time, so the docs always match the version of `qv` and work offline. One tool, `docs`: `docs(query)` searches the pages and returns the matching sections, and `docs(page)` returns a whole page. They're too long to give the agent all at once (about 75k tokens), and most MCP clients don't read resources unless a person attaches one, so this is a tool, not resources. The how-to pages are longer and more about the Editor's screens, so they're left out at first.
4. **Lookups in the loaded game.** `list_functions` and `describe_function(name)` (and the same for types) read the loaded game, so they cover its own functions and any other libraries it includes, which the docs don't. Each result says where the definition comes from:
   - **local**: defined in the game file, including a library function the author has overridden (`MakeElementLocal` copies it into the `.aslx`)
   - **library**, with the library's filename: loaded from Core or another included library
   The loader already records this on every element (`MetaFieldDefinitions.Library` and `Filename`).

   Where the library comes from depends on the file. While a game is being edited, its `.aslx` doesn't contain the Core library: Core comes from the copy built into the engine running it, which is the same version as `qv`'s docs. A published `.quest` file has its libraries inlined, frozen at the version it was last saved with. They then load as local elements (`GameLoader` only sets `Library` on elements loaded from an included file), so for a `.quest` file the two can't be told apart. That doesn't matter for editing, which is `.aslx` only. It does mean the docs may not match an older published game exactly, which `describe_function` can show.

Separately from `qv`: an `llms.txt` for questviva.com would help agents that aren't using `qv` at all. It's a small addition to the Astro build.

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

1. `qv check`, with lazy scripts forced and expressions checked, plus the `check` tool in `qv mcp`.
2. Server instructions and the `start_game` description.
3. `docs` with the built-in reference pages.
4. ~~`replay` on `start_game`~~: done as `qv walkthrough` and `run_walkthrough`.
5. `list_functions` / `describe_function`, with where each definition comes from.
6. Dogfood: an agent fixes bugs in a few `examples/` games by editing the XML. Decide on phase 3 from what goes wrong.
