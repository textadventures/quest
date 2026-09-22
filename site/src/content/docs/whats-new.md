---
title: What's new in Quest Viva 6.0
description: What has changed for Quest 5 users moving to Quest Viva
sidebar:
  order: 4
  label: What's new in Quest Viva 6.0
---

Quest Viva is the successor to Quest 5, which had been Quest's current major version since 2011. Under the hood it's the same engine and the same scripting language, so if you know Quest 5, you already know Quest Viva - but a lot has changed around it.

This page is a summary for people coming from Quest 5. If you're new to Quest Viva, you don't need any of this - start with the [Introduction](/intro) instead.

## Your existing games still work

Quest Viva opens the same `.aslx` game files as Quest 5, so you can carry on working on a game you started there. The player will also run `.quest` packages, and games in the much older `.asl` and `.cas` formats, which Quest used from version 1.0 in 1998 right through to 4.1.

Games you've already published keep working too, and don't need republishing. The library code a game uses is baked into its `.quest` file when you publish, so a game published years ago goes on behaving exactly as it did, whatever a player runs it with - see [the publish process](/publishing#the-publish-process) for what a published game contains.

Quest 5 itself is no longer developed. If you need an old installer - to compare behaviour against a particular version, say - they are kept on the [archive site](https://archive.textadventures.co.uk/quest/).

## One editor, everywhere

Quest 5's editor only ran on Windows, and the online editor at textadventures.co.uk was a separate thing with its own set of quirks and limitations.

Quest Viva has a single editor, and you can use it either way:

- **In your web browser**, at [play.questviva.com](https://play.questviva.com) - Windows, Mac, Linux, Chromebooks, whatever you have, and tablets and phones too.
- **As a desktop app**, which you can [download](/download) for Windows, Mac or Linux.

These aren't two editors kept roughly in step with each other. It's the same application in both, so the browser version isn't a cut-down one the way Quest 5's was - anything you can do at your desk, you can do in a browser, and you can move between the two as it suits you.

## Where your games are kept

Quest 5's online editor kept your games on your textadventures.co.uk account, so using it meant signing up for one. Quest Viva keeps them local by default, with no account needed:

- The desktop app saves your game as an ordinary file, in a folder you choose.
- In the browser you can pick a folder too, if your browser supports it - Chrome and Edge do.
- Otherwise your game is stored inside the browser itself. Use **Backup** every so often to save a copy to disk - the editor will remind you.

If you already have games on a textadventures.co.uk account, they're still there. The editor at textadventures.co.uk can sign you in and load them, as before.

## Games run in the browser, with no server

This is the biggest change under the hood. Quest 5 games played online ran on a Windows server, with every turn making a round trip over the network. That made games feel slow, limited how many people could play at once, and meant players lost their session if their connection hiccupped.

In Quest Viva, the whole engine is compiled to WebAssembly and runs *inside the player's browser*. Once a game has loaded:

- turns are instant - there's no server round trip
- there's no limit on how many people can play at the same time
- the game keeps working if the network drops
- no Windows server is needed to host anything

The player interface is also now fully responsive, so games look right on phones and tablets as well as on a desktop.

## Host your game anywhere

Because a game no longer needs a server to run it, you're no longer tied to textadventures.co.uk. You can still [publish there](/publishing) - it's still the easiest way to find an audience - but you can also:

- upload your `.quest` file to your own website and link to it through the player
- host the player itself alongside your game, on any static web host
- export your game as HTML — a small CDN-linked file, or a zip that includes the player for self-hosting

See [Hosting your game](/publishing/hosting) for all the options.

Published `.quest` files now also follow the [Treaty of Babel](https://babel.ifarchive.org/babel.html), a standard that interactive fiction catalogues and tools use to identify games. Each one carries your game's IFID - a unique ID for your game, which is the Game ID on the Setup tab - and an iFiction record of its title, author, cover and other details. Exported HTML includes the IFID too. There's also a new version code on the Setup tab, a number you increase with each release - see [Version and Game ID](/publishing/game-details#version-and-game-id).

WebPlayer, the server-based player, has been rewritten for Quest Viva - Quest 5's needed Windows, and this one runs on cross-platform .NET, so you can host it wherever you like. WasmPlayer has rather overtaken it since, though, so it's a specialist choice now: worth it if you specifically don't want players to be able to download your `.quest` file, for a treasure hunt or a competition say. See the [WebPlayer guide](/publishing/webplayer).

## Players can save without an account

In Quest 5, saving an online game meant having a textadventures.co.uk account. Quest Viva saves games to the browser's own storage instead, with multiple save slots, and players can also save a game out to a file to keep or move to another device. No account, no login.

## New in the editor

The editor was rebuilt for Quest Viva. Most of it does what it always did - here's what's different:

- **Autosave** - your changes are saved as you make them, so there's no Save button to forget
- **Works on phones and tablets** - the layout adapts to the screen, so on a phone the element tree and its properties each get the full screen, with a back button to switch between them, and the toolbar and dialogs are sized for touch
- **A better debugger** - you can change attribute values as well as read them, search and sort the attribute list, and leave it open while you play. See [Debugging your game](/howto/testing/debugging)
- **Light and dark themes**, or match your system setting
- **Syntax highlighting and autocompletion** in Code View
- **An asset picker** for images and sounds you've already added to your game
- **Folders for functions**, so a game with lots of functions doesn't have to be one long list. See [Organising functions into folders](/howto/scripting/functions#organising-functions-into-folders)
- **Safe Mode**, a raw XML editor that opens if a game file fails to load, so a broken file is recoverable instead of lost
- **Advanced options tucked away** - instead of Quest 5's Simple Mode, every tab keeps its advanced options in a collapsed "Advanced" section, and the script command list puts the everyday commands first, so beginners see less clutter without anything being hidden from you
- **Keyboard and screen reader support**, throughout the editor and the player
- **A translated interface** - English, German and Spanish, in the browser as well as on the desktop
- **Help where you need it** - most editor tabs link to the guide for that tab, and script commands have a "?" button that opens their reference page
- **Editable included libraries** - you can edit your own libraries from the tree, instead of in a separate text editor. Changes are checked before they're saved, so a mistake can't leave your game unable to load
- **Small tweaks** - library functions are grouped by the file they come from, the tree remembers which parts you had expanded for each game and can expand or collapse everything at once, Code View can fold sections of a script, and a setting opens scripts in Code View by default

## New in the engine

Most of the scripting language is unchanged, but there are some additions:

- **Pages in text adventures.** Quest 5 only had pages in gamebooks. Quest Viva lets a text adventure switch into a page - a set of links the player chooses from - which is a much easier way to write conversations and dialogue trees. See [Using Pages](/tutorial/using-pages).
- **Asking the player something, without callbacks.** A script can now pause for the player and carry on at the next line: `GetInput()` for typed input, `Ask()` for a yes/no question, `ShowMenu()` for a menu, and `WaitForKeyPress` to wait for a keypress. These were in Quest 5 from the start, but were deprecated part-way through its life - in 5.4, and 5.5 for pausing - because they held a real thread while they waited. Quest Viva suspends the script instead, so they work properly, and the script editor offers them. The older forms that took a nested callback block - the `get input`, `ask`, `show menu` and `wait` script commands - still run, so games using them are unaffected, but they are no longer in the list of commands you can add. They were always the harder way to write anything that asks more than one question. One thing to know: while a script is waiting at one of these, the player can't save. See [Asking the player](/howto/scripting/asking-the-player) for the details, and for the callback forms of `Ask` and `ShowMenu` that don't have that limitation.
- **Questions and menus in the game text.** In games saved in Quest Viva, a question or menu now appears as numbered links in the game text instead of a pop-up window. Players can click an option or type its number.
- Plenty of smaller additions and fixes besides - the [changelog](https://github.com/textadventures/quest/blob/main/CHANGELOG.md) lists these from 6.0.0-beta.36 onwards.

A game saved in the Quest Viva editor is marked as ASL version 600 - `<asl version="600">` at the top of the file. Older games load and run unchanged - the version number just tells the engine which behaviour to use where something has been fixed in a way that would otherwise change an existing game.

## Why isn't it called Quest 6?

Because there's [another system](https://github.com/ThePix/QuestJS) already called that, although it's more commonly known as QuestJS.

## Something missing?

Quest Viva is under active development, so if there's something you relied on in Quest 5 that you can't find, please say so on [Discord](https://textadventures.co.uk/community/discord) or in [GitHub Discussions](https://github.com/textadventures/quest/discussions).
