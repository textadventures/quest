---
title: What's new in Quest Viva 6.0
description: What has changed for Quest 5 users moving to Quest Viva
sidebar:
  order: 4
  label: What's new in Quest Viva 6.0
---

Quest Viva is the successor to [Quest 5](/developers/older-versions), which had been Quest's current major version since 2011. Under the hood it's the same engine and the same scripting language, so if you know Quest 5, you already know Quest Viva - but a lot has changed around it.

This page is a summary for people coming from Quest 5. If you're new to Quest Viva, you don't need any of this - start with the [Introduction](/intro) instead.

## Your existing games still work

Quest Viva opens the same `.aslx` game files as Quest 5, so you can carry on working on a game you started there. The player will also run `.quest` packages, and games written in the much older Quest 4 formats (`.asl` and `.cas`).

Published games are unaffected by any of this. When you publish, the library code your game uses is baked into the `.quest` file, so a game published years ago keeps behaving exactly as it did, whatever version of Quest Viva a player uses to run it.

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

Because a game no longer needs a server to run it, you're no longer tied to textadventures.co.uk. You can still [publish there](/publishing/publishing) - it's still the easiest way to find an audience - but you can also:

- upload your `.quest` file to your own website and link to it through the player
- host the player itself alongside your game, on any static web host
- export your whole game as a single HTML file and upload that one file anywhere

See [Hosting your game](/publishing/hosting) for all the options.

## Players can save without an account

In Quest 5, saving an online game meant having a textadventures.co.uk account. Quest Viva saves games to the browser's own storage instead, with multiple save slots, and players can also save a game out to a file to keep or move to another device. No account, no login.

## New in the editor

The editor has been rebuilt, and it picked up a lot along the way:

- **Light and dark themes**, or match your system setting
- **Code View** with syntax highlighting and autocompletion, for scripts and for the raw XML behind your game
- **Filter boxes** on the element tree and the script command picker, so you can find things by typing instead of scrolling
- **Back and forward navigation** through the elements you've been editing
- **Cut, copy, paste and move** for objects, functions and scripts, and folders you can organise them into
- **Dedicated tabs for exits and verbs**, instead of hunting through attribute lists
- **An asset picker** for choosing images and sounds you've already added to your game
- **A built-in debugger** in the player, where you can inspect and override attributes while a game runs
- **Safe Mode**, a raw XML editor that opens if a game file fails to load, so a broken file is recoverable instead of lost
- **Advanced options tucked away** by default, so the things most games need are easier to find - everything is still there when you want it
- **Keyboard and screen reader support** throughout the editor and the player

The editor interface itself is also translated - it's currently available in English, German and Spanish.

## New in the engine

Most of the scripting language is unchanged, but there are some additions:

- **Pages in text adventures.** Quest 5 only had pages in gamebooks. Quest Viva lets a text adventure switch into a page - a set of links the player chooses from - which is a much easier way to write conversations and dialogue trees. See [Using Pages](/tutorial/using_pages).
- **Lockable containers can require every key**, rather than any one of them.
- Assorted new functions and fixes - see the [changelog](https://github.com/textadventures/quest/blob/main/CHANGELOG.md) for the full list.

Games saved by Quest Viva use world model version 600. Older games load and run unchanged; the version number just tells the engine which behaviour to use where something has been fixed in a way that would otherwise change an existing game.

## What's gone

- **Vimeo videos** are no longer offered when you're building a game. Existing games that use them still play. [YouTube video](/howto/multimedia/adding_videos) is unaffected.
- **"Write log to file"** has been removed - it hadn't done anything useful for a long time.
- **WebPlayer**, the server-based player, still exists but is now a specialist option rather than how online games normally run. It's worth using if you specifically don't want players to be able to download your `.quest` file - for example, for a treasure hunt or a competition. See the [WebPlayer guide](/publishing/webplayer).

Quest Viva is under active development, so if there's something you relied on in Quest 5 that you can't find, please say so on [Discord](https://textadventures.co.uk/community/discord) or in [GitHub Discussions](https://github.com/textadventures/quest/discussions).

## Why isn't it called Quest 6?

Because there's [another system](https://github.com/ThePix/QuestJS) already called that, although it's more commonly known as QuestJS.

## Where's the documentation?

Right here - and since Quest Viva is "a modern version of Quest 5" under the hood, almost everything in these docs applies to both. The [Introduction](/intro) is a good tour of what's possible, and the [Tutorial](/tutorial/tutorial_introduction) is worth a skim even if you've used Quest 5, as the editor has moved on.
