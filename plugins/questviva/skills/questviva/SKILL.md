---
name: questviva
description: Write, edit, check, play and publish Quest Viva and Quest 5 games. Use when working with .aslx game files, a .quest package, or a Quest text adventure or gamebook, including writing rooms, objects, exits, commands, verbs, functions and ASL scripts, and checking, testing, playing or previewing a game with the qv command-line tool or its MCP tools.
---

# Writing Quest Viva games

Quest Viva is a system for text adventures and gamebooks, the successor to Quest 5. A game is one `.aslx` file: XML holding the game, its rooms, objects, commands and functions, with scripts in Quest's own language, ASL. Publishing makes a `.quest` file, a zip package to play or upload.

Links in this skill such as [elements](/reference/elements/) are pages of Quest Viva's documentation. Read one with the `docs` tool of the `qv` MCP server, giving the link as `page` (`reference/elements`), or on the web under https://beta.questviva.com, such as https://beta.questviva.com/reference/elements/. https://beta.questviva.com/llms.txt lists every page.

## Before you change anything

- **Read [the ASL primer](references/asl-primer.md) first**, every time, before you write any XML or ASL. It's short, and it covers what agents most often get wrong: ASL isn't JavaScript or C#, and an `.aslx` file has rules of its own.
- **Find the game.** If the author doesn't name the file, look in the current folder, where authors usually start the agent. A game is an `.aslx` file with a `<game>` element; other `.aslx` files beside it, with no `<game>`, are its libraries. If there's more than one game, ask which. Give the MCP tools its full path.
- **Find out what kind of game it is.** A gamebook has `templatetype="gamebook"` on its `<asl>` element and includes `GamebookCore.aslx`: it's made of pages and links, not rooms and commands, so read [gamebooks](references/gamebooks.md). Everything else is a text adventure.
- **Only `.aslx` files can be edited.** A `.quest` file is a published package. Ask the author for the `.aslx` it was published from; don't unzip it and change what's inside. Quest 1 to 4 games (`.asl`, `.cas`) can be played but not checked, and this skill doesn't cover writing them.
- **The game may be open in the Quest Viva editor** while you work. The editor never saves over your changes without the author choosing to: if they edit something there after you've changed the file, it stops saving and asks which version to keep. But it doesn't show your changes until the author reloads the game. In the desktop app, a bar offers **Reload from disk** as soon as you change the file; the browser editor only notices when they next edit. So when you've changed a game that's open there, tell the author to reload it before they carry on. Write each change in one go, rather than leaving the file half edited.

For a new game, start from the template at the top of the primer, with a new GUID for `gameid` (`uuidgen` makes one) and the author's title.

## qv

`qv`, Quest Viva's command-line tool, checks, tests, plays, previews and publishes games. `qv mcp --debug-tools` runs it as an MCP server whose tools do the same things inside the conversation: `check`, `run_walkthrough`, `start_game`, `send_command`, `evaluate`, `tick`, `docs`, `list_functions`, `describe_function`, `preview` and `publish`. The Quest Viva plugin for Claude Code starts that server itself, as `qv`. Without the MCP server, the commands below do most of the same; `evaluate`, `docs`, `list_functions` and `describe_function` have no command-line equivalent, so read the docs on the web instead.

If `qv` isn't found, or the MCP server's tools aren't there, it isn't installed. Tell the author how to install it, and then to restart the agent (in Claude Code, `/mcp` can reconnect the server instead):

- with the [.NET 11 SDK](https://dotnet.microsoft.com/download): `dotnet tool install -g QuestViva.Cli --prerelease`
- or, without .NET: the `qv-<platform>` download from the newest `6.1.0-beta` release at https://github.com/textadventures/quest/releases, put somewhere on the `PATH`. Releases before 6.1 don't have `qv`.

Until then, edit carefully from the primer and the docs, and tell the author that nothing has been checked.

## The edit, check, play loop

After every change to the game:

1. **Check it**: the `check` tool, or `qv check game.aslx --json`. It reports script commands and functions that don't exist, expressions that don't parse, wrong numbers of arguments, duplicate names, clashing verbs and anything that stops the game loading, each with its element and attribute. It exits 1 when there are problems. Loading errors, such as an exit to a missing room, hide everything else: fix them and check again. A clean check doesn't mean the game works, only that its scripts can run. Object and variable names aren't checked, and nor is passing the wrong kind of thing, such as a door object to `UnlockExit`, which wants the exit: only a walkthrough or playing catches those.
2. **Test it with a walkthrough.** A walkthrough is a `<walkthrough>` in the game with a list of steps, each a command a player would type or an `assert:` with an ASL expression that must be true ([walkthroughs](/howto/testing/walkthroughs/)). Run one with `run_walkthrough` or `qv walkthrough game.aslx <name>`; with no name, `qv walkthrough` runs them all, and exits 1 if the game has none. It fails on a false `assert:`, a script error, a menu answer that isn't an option, or the game ending early, and its random numbers are seeded, so it gives the same result every time.
   - **Fixing a bug:** first add a walkthrough that reaches the bug and ends in an `assert:` that shows it, and run it to see it fail. Then fix the game and see it pass.
   - **Adding something:** add a walkthrough that uses it and asserts what should have happened.
   - **Before you finish:** run every walkthrough, to check nothing else broke.
   - Walkthroughs stay in the game, as the author's tests. Tell the author which ones you added.
3. **Play it** to see what a player sees: `start_game` with the game's path, then `send_command` for each command. `start_game` with no path restarts the game, reading the file again, so it picks up your edits. `evaluate` works out an expression against the game's state, such as `game.pov.parent` or `lamp.lit`. Without the MCP tools, write the commands as a walkthrough and run it with `qv walkthrough game.aslx <name>`, which prints each command and the game's response. Piping commands into `qv play` (`printf 'look\ntake lamp\n' | qv play game.aslx`) works too, but doesn't echo the commands.
4. **Look it up rather than guess.** With the MCP tools, `docs` searches the documentation (query `foreach`, `MoveObject`, `locked exit`) or reads a page, and it matches the version of `qv` that's running; without them, read the pages on the web. `list_functions` lists the functions the game can call, and `describe_function` shows one's parameters, its body and where it's defined: the game, one of its libraries or Quest's own.
5. **Preview it** when the change is something the text can't show, such as pictures, sound, JavaScript, stylesheets or layout: the `preview` tool, or `qv preview game.aslx`, which runs until stopped, so start it in the background. Both give a URL on 127.0.0.1 that plays the game in Quest's browser player and reloads whenever the game's folder changes. Give the author the URL to try your change; if you have a browser tool, open it yourself.
6. **Publish it** only when the author asks: `publish`, or `qv publish game.aslx` for a `.quest` file, `--format html` for one web page or `--format zip` for a web page with its player. It checks the game first and refuses if there are problems; don't add `--force` unless the author says to.

## What not to do

- **Don't copy Quest's own functions into the game.** Core's functions are always there. A function of the game's with the same name replaces Core's for the whole game, which is how an author changes built-in behaviour on purpose. Before replacing one, read it with `describe_function`, and only do it when the author wants that behaviour changed.
- **Don't edit a `.quest` file**, or the libraries built into Quest, such as `Core.aslx` and `English.aslx`, which aren't in the game's folder anyway: the game gets them from the version of Quest that runs it.
- **Don't change `gameid`**, which identifies the game when it's published, or the `version` on `<asl>`, unless the author asks to upgrade it ([game file versions](/reference/asl-versions/)).
- **Don't leave scratch files in the game's folder.** `qv publish` packs every file at the top of the folder into the `.quest` file, and `qv preview` serves them.
- **Don't reformat or reorder the file.** Keep each change to the part you meant to change, so the author can review it, and match the file's own style: its indentation, whether it uses CDATA, how it names things.

## Writing well for Quest

- Use what Core already does before writing commands of your own. Locked exits, containers, doors, switchable and edible objects, wearables, light and dark, scores and ask/tell all exist; look in [the how-to guides](/howto/) with `docs`, and see [elements](references/elements.md) for how several of them look in the XML.
- Give every room `<inherit name="editor_room" />` and every object `<inherit name="editor_object" />`, as the editor does. When an object uses a feature such as a container or a light source, also set the feature attribute (`feature_container` and so on) the editor sets, or the editor hides that object's settings for it ([feature attributes](references/elements.md#feature-attributes)).
- Every exit needs a matching exit back, unless the way really is one-way.
- Keep the author's voice. Ask before restructuring the game, renaming things or rewriting their text.

## Reference files

Read these when you need more than the above:

- [The ASL primer](references/asl-primer.md): the `.aslx` format, attributes and their types, XML escaping, the ASL script language, the text processor, commands and verbs, common mistakes. Read before writing anything.
- [Elements](references/elements.md): worked examples of rooms and exits (two-way and locked), containers and switchable objects, the features attributes, types, functions that return values, turn scripts, timers and the game's `start` script. Read when adding any of these.
- [Gamebooks](references/gamebooks.md): gamebook pages, links, page types, flags and counters, and walkthroughs for a gamebook. Read before changing a gamebook.
