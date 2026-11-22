---
title: The qv command-line tool
description: Play, check, test, preview and publish Quest games from the command line with qv, and let an AI agent work on your game
sidebar:
  order: 40
---

`qv` is Quest Viva's command-line tool. It does from a terminal much of what the editor does: it plays games, checks them for problems, runs their walkthroughs, previews them in your browser and publishes them. You don't need it to write a game - everything here can be done in the editor - but it's handy if you like to work in a text editor, if you want your walkthroughs run automatically every time you change your game, or if you want an AI agent to play your game and tell you where it got stuck.

`qv` plays games from every version of Quest: Quest Viva and Quest 5 `.aslx` and `.quest` files, and the `.asl` and `.cas` files of Quest 1 to 4.

## Installing qv

There are two ways to install it.

**As a .NET tool.** If you have the [.NET 11 SDK](https://dotnet.microsoft.com/download) installed, run:

```bash
dotnet tool install -g QuestViva.Cli
```

To get a beta version, add `--prerelease`. Run `dotnet tool update -g QuestViva.Cli` to update it later.

**As a standalone program**, which needs nothing else installed. Each [Quest Viva release on GitHub](https://github.com/textadventures/quest/releases) has a download for each platform:

| Platform | Download |
|---|---|
| Mac (Apple Silicon) | `qv-osx-arm64.tar.gz` |
| Linux (x64) | `qv-linux-x64.tar.gz` |
| Linux (ARM64) | `qv-linux-arm64.tar.gz` |
| Windows (x64) | `qv-win-x64.zip` |
| Windows (ARM64) | `qv-win-arm64.zip` |

Each holds a single file, `qv` (or `qv.exe` on Windows). Put it somewhere on your `PATH`. There's no download for Intel Macs, but the .NET tool works on them. On Linux, `qv` needs the ICU library, which desktop distributions include but minimal containers often don't.

## Playing a game

```bash
qv play mygame.aslx
```

plays a game in the terminal. Type commands as you would in the browser; menus and questions show as numbered options. Add `--seed` and a number to play with the same random numbers every time.

## Checking a game for problems

```bash
qv check mygame.aslx
```

looks through the game's scripts without playing it, and lists the same problems as **Check game** in the editor: script commands and functions that don't exist, expressions that can't be worked out, functions given the wrong number of values, two elements with the same name, and a few more. See [Check the game](/howto/testing/troubleshooting#check-the-game) for what it can and can't find.

It finishes with exit code 0 when there are no problems and 1 when there are, so you can use it in a script. Add `--json` for output another program can read.

## Running walkthroughs

```bash
qv walkthrough mygame.aslx
```

runs every one of the game's [walkthroughs](/howto/testing/walkthroughs), each from a fresh start, and prints `PASS` or `FAIL` for each. Give a walkthrough's name after the game to run just that one, and add `--quiet` to see only the results.

It's stricter than the editor's **Play** button. A walkthrough fails if anything means it no longer does what you wrote: a failed assertion, a script error, a menu answer that isn't one of the options, or the game ending before the last step. So it's a good way to check that your game is still winnable after every change - and since it exits with a non-zero code when a walkthrough fails, you can run it automatically, in GitHub Actions say, every time you change your game.

Walkthroughs use the same random numbers as the editor's **Play**: the walkthrough's own **Random seed** if it has one, or otherwise the same fixed seed every time. `--seed` overrides it.

## Previewing in the browser

```bash
qv preview mygame.aslx
```

serves the game in Quest Viva's browser player on your own computer, and prints the address to open. Playing in the terminal can't show pictures, sounds, your stylesheet or your JavaScript, so this is how you see what players will see.

Leave the page open while you edit: whenever a file in the game's folder changes, the page reloads and starts the game again. The Debugger is available, as in the editor's preview. The player is built into `qv`, and only your own computer can reach it.

## Publishing

```bash
qv publish mygame.aslx
```

makes a `.quest` file of your game, as **Publish** does in the editor, ready to [upload to textadventures.co.uk](/publishing) or play in any Quest player. `--format html` makes a single web page instead, and `--format zip` a web page with a copy of the player beside it, ready to put on any web host - the same two HTML exports the editor offers. See [Hosting your game](/publishing/hosting).

It checks the game first, as `qv check` does, and won't publish if there are problems unless you add `--force`. It also warns you about any picture, sound or other file your game refers to that isn't in its folder, and if your game has no Game ID.

## AI agents

```bash
qv mcp
```

runs an [MCP](https://modelcontextprotocol.io/) server: a way for an AI agent, such as Claude, to use `qv`. Its tools let the agent play your game and tell you where it got stuck, or, with `--debug-tools`, check, test and fix it as well. See [Writing with an AI agent](/howto/testing/ai-agents), which also has the Quest Viva plugin for Claude Code.

There's also `qv serve`, which lets another program drive a game by sending it commands and reading the responses as JSON - for writing a test harness of your own, for example.
