---
title: Writing with an AI agent
description: Have an AI agent such as Claude Code write, fix, playtest and check your game, with the Quest Viva plugin or the qv MCP server
sidebar:
  order: 41
---

An AI agent, such as [Claude Code](https://claude.com/claude-code), can work on your game with you: add a room, fix a bug you found, write walkthroughs to test it, or play it and tell you where it got stuck. It edits your `.aslx` file directly, so it's best for games you keep in a folder of your own, and it uses [the qv command-line tool](/howto/testing/command-line) to check and play each change it makes.

Agents have seen some Quest 5 code, but they mix it up with other languages, so without help they write scripts that look right and don't run. What makes the difference is telling them how Quest Viva works and letting them check their own work, and that's what the plugin below does.

You'll need `qv` installed first: see [Installing qv](/howto/testing/command-line#installing-qv).

## The Quest Viva plugin for Claude Code

Install it once, from Claude Code:

```text
/plugin marketplace add textadventures/quest
/plugin install questviva@questviva
```

The plugin adds two things:

- **A Quest Viva skill**, which Claude loads whenever it works on an `.aslx` file. It covers the game file format, Quest's script language and its common mistakes, text adventures and gamebooks, and how to check, test, play and preview a game after each change.
- **The `qv` MCP server**, so Claude can play your game, check it, run its walkthroughs, look things up in this documentation and preview your game in your browser, without you setting anything up. It runs `qv mcp --debug-tools`. If you added the server by hand before, remove that one with `claude mcp remove qv`.

It's then there in every Claude Code session.

## Working on your game

Start Claude Code in your game's folder, the one with your `.aslx` file: open a terminal there and run `claude`, or choose the folder when you start a session in the Claude desktop app. Claude then finds your game itself, so you don't need to give its path. Then ask for what you want:

- "Add a kitchen to the east of the hall, with a locked cupboard and a key in the garden."
- "When I eat the apple it's still in my inventory. Fix it."
- "Play the game and tell me which puzzles were unfair."

If the folder holds more than one game, say which one. To start a new game, start Claude Code in an empty folder and describe the game you want.

Claude checks the game with `qv check` after every change. When it fixes a bug, it first writes a [walkthrough](/howto/testing/walkthroughs) that shows the bug, then fixes it and runs the walkthrough again to see it pass. The walkthroughs stay in your game, so you can run them yourself with `qv walkthrough` whenever you change something. To see a change yourself, ask Claude to preview the game: it gives you an address to open in your browser, which reloads whenever the game changes.

Your game can stay open in the Quest Viva editor while an agent works on it, and the editor won't save over the agent's changes: if you change something in the editor after the agent has changed the file, it stops saving and asks which to keep. **Use the changed file** loads the agent's version and drops your unsaved changes, and **Keep my changes** saves yours over it. To see the agent's changes in the editor, reload the game. The desktop app shows a bar with **Reload from disk** as soon as the agent changes the file; the editor in your browser only notices when you next change something.

## Other agents

For an agent that isn't Claude Code, there are two parts to set up yourself.

**The guidance.** The skill is also one Markdown file, [agents.md](/agents.md). Save it as `AGENTS.md` in your game's folder, which many agents read when they start, or tell your agent to read it before working on your game.

**The `qv` MCP server**, if your agent supports [MCP](https://modelcontextprotocol.io/). Add a server that runs:

```bash
qv mcp --debug-tools
```

How you add one depends on the agent; it's usually a few lines in its settings file. Without MCP, an agent can still run `qv check`, `qv walkthrough` and `qv play` as ordinary commands, and the guidance says how.

Agents that read documentation from the web can also start at [llms.txt](/llms.txt), an index of this documentation written for them, which begins with a short guide to writing a game file.

## Letting an AI agent play your game

```bash
qv mcp
```

runs an MCP server whose tools let an agent start a game and type commands, so you can ask it to play your game and tell you where it got stuck, which puzzles it found unfair or which descriptions confused it. The agent sees what a player would see, including the exits the compass shows and the lines of the status pane.

For example, to add it to Claude Code:

```bash
claude mcp add qv -- qv mcp
```

and then ask Claude to play your game, giving it the path to your `.aslx` or `.quest` file. Give a game after `qv mcp` to let the agent play only that one.

By default the agent only gets the tools a player would have, so it gets a fair game. `--debug-tools`, which the plugin uses, gives it more: it can then work out expressions against the game's current state, move the game clock on to fire timers, check the game, run its walkthroughs, preview it and publish it, and look things up in this documentation and in the game's functions. That turns it from a playtester into something that can help you track down and fix a problem, but it can also read the solution straight out of the game. So for a fair playtest, give the agent a server without `--debug-tools`, and don't let it read the game file.
