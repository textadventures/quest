---
title: What's new in Quest Viva
description: What has changed in each Quest Viva release, and for Quest 5 users moving to Quest Viva
sidebar:
  order: 4
  label: What's new
---

This page summarises what has changed in each Quest Viva release. If you're coming from Quest 5, start with [Quest Viva 6.0](#quest-viva-60), which covers everything that changed since then. If you're new to Quest Viva, you don't need any of this - start with the [Introduction](/intro) instead.

## Quest Viva 6.1

### A new player interface

The biggest change in 6.1 is a new interface for playing games. The old one dates back to [Quest 5.3, in 2012](https://archive.textadventures.co.uk/blog/2012/12/03/quest-5-3-beta-is-now-available). Quest Viva 6.0 made it work well on phones, but it still looked very much of its time. The new one has a cleaner, more modern look, and gives you more control over the layout.

Here's what's different:

- **Light and dark mode.** Games follow the light or dark setting on the player's device, and players can switch between the two with a button at the top of the game.
- **You choose where the panes go.** The inventory, compass and other panes can go beside the game, in a sheet that the player opens with a button next to the command bar, or you can turn them off. You can make a different choice for phones, where by default the panes go in a drawer.
- **A new map**, which can show room pictures, and can go either above the game or in the panes.

See [The player interface](/customise/player-interface).

New games use the new interface. Games made with older versions of Quest carry on using the old one, which is now called the [classic interface](/customise/classic/), so games you've already published play exactly as they always have. When you save an older game in the 6.1 editor, it switches to the new interface.

Most games will be fine with that. The exception is a game with its own JavaScript that changes the classic interface, for example using `JS.setCss` or jQuery, because that code won't work with the new one. When you open a game like that, the editor tells you, and offers to keep it on the classic interface. If you'd rather move it over, [Moving to the new interface](/customise/moving-from-classic) explains what to use instead of the most common bits of code.

### Themes, colours and stylesheets

The game's Display tab has been split in two. The **Appearance** tab sets the theme, fonts and colours, and the **Interface** tab sets which parts of the interface are shown, such as the panes, the map and the command bar. Gamebooks now have an Appearance tab too. See [Look and feel](/customise/look-and-feel).

- **New themes.** There are five new themes: Parchment, Noir, Storybook, Midnight and Terminal. The theme chooser shows you what each one looks like before you pick it. The Retro and Hot Dog Stand themes have been retired, but games that already use them aren't affected.
- **Themes don't turn things on and off any more.** Choosing a theme used to change settings such as whether the panes were shown. Now a theme only sets fonts, colours and spacing, and leaves your Interface tab settings alone. If you've changed any of a theme's fonts or colours, the new **Reset** button puts them back.
- **Dark mode colours.** A new game doesn't set any colours of its own, so it uses the standard light or dark colours, depending on the player's setting. If you do choose your own colours, you can also choose a dark mode version of each one.
- **Colour pickers** for every colour setting.
- **Previews.** The Appearance tab shows some sample text in your game's fonts and colours, in both light and dark mode. The Interface tab shows a miniature version of your game's layout, on a desktop and on a phone.
- **Your own stylesheet.** You can add your own CSS on the Appearance tab. The new interface is designed to be styled this way: it has named parts and CSS variables for you to use, so you don't need to rely on how the page happens to be built. See [Styling the player with CSS](/customise/css).
- **Changes show up in Preview straight away.** If you have your game open in Preview, any changes you make on these tabs, including to your stylesheet, show up immediately, without restarting the game.
- **Script commands for the interface.** A new Interface category of script commands lets you show and hide parts of the interface during the game, add panes, and more. Previously you'd have needed JavaScript for this, and the changes would be lost when the player loaded a saved game. See [Changing the interface during play](/customise/interface-scripts).

### Check your game for problems

**Check game**, in the editor's **File** menu, finds problems in your scripts without you having to play the game: script commands and functions that don't exist, expressions that can't be worked out, functions given too many values, and two elements with the same name. Click a problem to go straight to the script it's in. **Publish** checks your game first too, so you find out about any problems before your players do. See [Check the game](/howto/testing/troubleshooting#check-the-game).

### Script editor improvements

Long scripts, especially ones with lots of nested `if` commands, are now much easier to work with:

- **Add or paste anywhere.** A new script command used to always go at the end, and you'd have to move it up one line at a time. Now each command's ⋯ menu lets you add or paste a command directly above or below it.
- **Collapse blocks.** You can collapse the "then" and "else" parts of an `if`, and the contents of commands such as `for` and `firsttime`. This makes it much easier to see which `else` belongs to which `if`.
- **See which block you're in.** The block you're editing is outlined, and each "Add script" button says which block it's for, for example "Add script to else".
- **Clearer selections.** When you tick a command, it's highlighted along with everything nested inside it, so you can see exactly what Copy, Cut or Delete will act on. You can shift-click to select a range of commands, and move several commands up or down at once.

See [Working with a script](/tutorial/using-scripts#working-with-a-script).

### Other editor improvements

- **Turning off a feature.** Unticking a feature on the Features tab hides its tab, but anything you set up on that tab still applies in the game. The editor now asks whether you want to remove those settings too. See [Turning a feature off](/howto/objects/features#turning-a-feature-off).
- **A warning about function names.** If a function that returns a value has the same name as one of the engine's built-in functions, such as `CapFirst`, the engine always calls its own one instead, so yours can never be used in an expression. The editor now warns you about this. See [Overriding Core library functions](/customise/overriding).
- **Pasting a list.** If you paste several lines into a list, each line is added as a separate item.

### Reproducing bugs that players find

When a player reports a bug, it can be hard to make it happen again yourself, especially if your game uses random numbers. 6.1 makes this much easier.

Players can now click **Copy for a bug report** in the Save / Load dialog. This copies everything they've typed and clicked since the game started, along with the game's *random seed*, which is a number that decides which random numbers the game gets. They can then paste that into a message to you.

To replay it, paste the steps into a new walkthrough, and set the walkthrough's new **Random seed** setting to the seed from the report. When you play the walkthrough, the game makes the same random choices as it did for the player.

This works even if the player had loaded a saved game, because saved games now keep track of the random numbers too. In that case you'll need their save file as well.

Walkthroughs in the editor also now use the same random numbers every time they're run, so a walkthrough that goes through something random always plays out the same way. See [Using walkthroughs](/howto/testing/walkthroughs#random-numbers).

### For players

- **A redesigned Save / Load dialog.** Saving now takes one click, and you don't have to type a name. A save without a name is labelled with the room the player was in. The newest saves are at the top, and each save has a menu to overwrite, rename or delete it, or save it to a file. The dialog checks with the player before loading a game or starting again if they'd lose any progress, and when a game starts, it offers to carry on from their most recent save.
- **Exit buttons on phones.** On a phone, a row of buttons under the text shows which way the player can go, so they don't have to open the drawer to get to the compass. This works with the classic interface too.
- **`LOOK IN`.** `LOOK IN` and `LOOK INSIDE` now list what's inside a container. See [Containers](/howto/objects/containers).
- **The loading screen shows the game.** While a game is loading, the screen shows the game's title instead of the Quest Viva heading, and for games on textadventures.co.uk, its author and cover art too. The browser tab shows the game's name as well.

### The qv command-line tool

6.1 includes `qv`, a new command-line tool for authors. You can use it to play games in a terminal, check them for problems, run walkthroughs, preview them in a browser, and publish them. That's useful if you prefer to edit your game in a text editor, or if you want your walkthroughs to run automatically whenever you make a change.

You can also use `qv` to let an AI agent such as Claude play your game, and tell you about anywhere it got stuck.

`qv` plays games from any version of Quest, all the way back to Quest 1. See [The qv command-line tool](/howto/testing/command-line).

### Publishing

- **Link previews.** When you publish your game as HTML, the page now includes your game's title and description. So when you share a link to it on Discord, a forum or social media, the preview shows your game's details instead of Quest Viva's. If you use the zip option, the preview shows your cover art too. See [Where these are used](/publishing/game-details#where-these-are-used).
- **WebPlayer has been removed.** WebPlayer, the server-based player, isn't part of 6.1. The 6.0 version is still available, but it can't play games saved in 6.1. See [WebPlayer](/publishing/webplayer).

### Games saved in 6.1 are version 610

When you save a game in the 6.1 editor, it's marked as ASL version 610 - you'll see `<asl version="610">` at the top of the file. That's how the player knows to use the new interface. It also means Quest Viva 6.0 and earlier can't open or play the game any more.

So when you open a game made with an older version of Quest, the editor now shows a banner warning you that changing it will upgrade it. The banner has a **Download original** button, which saves a copy of the game exactly as it was when you opened it, even if you've already made changes. See [Game file versions](/reference/asl-versions).

### The editor on textadventures.co.uk stays on 6.0

The online editor at textadventures.co.uk, which opens games stored on your textadventures.co.uk account, stays on Quest Viva 6.0 and doesn't get the features added in 6.1. Your games there are still there, and you can carry on editing them in that editor. To move a game over to 6.1:

1. Go to [Your games](https://textadventures.co.uk/your-games) on textadventures.co.uk and click **Download** next to the game, under "Quest online editor games". That gives you a `.zip` of the game and its assets.
2. In Quest Viva, go to the **Create** tab and either click **Import game file** and choose the `.zip`, which makes it a local draft in your browser, or unzip it first and open the folder with **Open game folder** (**Open game…** in the desktop app).

Once you have, publish it as a `.quest` file and upload that to textadventures.co.uk - see [Publishing on textadventures.co.uk](/publishing#publishing-on-textadventurescouk).

There are plenty of smaller improvements and bug fixes too - see the [changelog](https://github.com/textadventures/quest/blob/main/CHANGELOG.md).

## Quest Viva 6.0

Quest Viva is the successor to Quest 5, which had been Quest's current major version since 2011. Under the hood it's the same engine and the same scripting language, so if you know Quest 5, you already know Quest Viva - but a lot has changed around it.

### Your existing games still work

Quest Viva opens the same `.aslx` game files as Quest 5, so you can carry on working on a game you started there. The player will also run `.quest` packages, and games in the much older `.asl` and `.cas` formats, which Quest used from version 1.0 in 1998 right through to 4.1.

Games you've already published keep working too, and don't need republishing. The library code a game uses is baked into its `.quest` file when you publish, so a game published years ago goes on behaving exactly as it did, whatever a player runs it with - see [the publish process](/publishing#the-publish-process) for what a published game contains.

Quest 5 itself is no longer developed. If you need an old installer - to compare behaviour against a particular version, say - they are kept on the [archive site](https://archive.textadventures.co.uk/quest/).

### One editor, everywhere

Quest 5's editor only ran on Windows, and the online editor at textadventures.co.uk was a separate thing with its own set of quirks and limitations.

Quest Viva has a single editor, and you can use it either way:

- **In your web browser**, at [play.questviva.com](https://play.questviva.com) - Windows, Mac, Linux, Chromebooks, whatever you have, and tablets and phones too.
- **As a desktop app**, which you can [download](/download) for Windows, Mac or Linux.

These aren't two editors kept roughly in step with each other. It's the same application in both, so the browser version isn't a cut-down one the way Quest 5's was - anything you can do at your desk, you can do in a browser, and you can move between the two as it suits you.

### Where your games are kept

Quest 5's online editor kept your games on your textadventures.co.uk account, so using it meant signing up for one. Quest Viva keeps them local by default, with no account needed:

- The desktop app saves your game as an ordinary file, in a folder you choose.
- In the browser you can pick a folder too, if your browser supports it - Chrome and Edge do.
- Otherwise your game is stored inside the browser itself. Use **Backup** every so often to save a copy to disk - the editor will remind you.

Whichever way you store it, see [Backing up your game](/howto/testing/backups) for why it's worth keeping more than one copy as you go.

If you already have games on a textadventures.co.uk account, they're still there. The editor at textadventures.co.uk can sign you in and load them, as before - but it stays on 6.0: see [Quest Viva 6.1](#the-editor-on-textadventurescouk-stays-on-60) for moving a game over.

### Games run in the browser, with no server

This is the biggest change under the hood. Quest 5 games played online ran on a Windows server, with every turn making a round trip over the network. That made games feel slow, limited how many people could play at once, and meant players lost their session if their connection hiccupped.

In Quest Viva, the whole engine is compiled to WebAssembly and runs *inside the player's browser*. Once a game has loaded:

- turns are instant - there's no server round trip
- there's no limit on how many people can play at the same time
- the game keeps working if the network drops
- no Windows server is needed to host anything

The player interface is also now fully responsive, so games look right on phones and tablets as well as on a desktop.

### Host your game anywhere

Because a game no longer needs a server to run it, you're no longer tied to textadventures.co.uk. You can still [publish there](/publishing) - it's still the easiest way to find an audience - but you can also:

- upload your `.quest` file to your own website and link to it through the player
- host the player itself alongside your game, on any static web host
- export your game as HTML — a small CDN-linked file, or a zip that includes the player for self-hosting

See [Hosting your game](/publishing/hosting) for all the options.

Published `.quest` files now also follow the [Treaty of Babel](https://babel.ifarchive.org/babel.html), a standard that interactive fiction catalogues and tools use to identify games. Each one carries your game's IFID - a unique ID for your game, which is the Game ID on the Setup tab - and an iFiction record of its title, author, cover and other details. Exported HTML includes the IFID too. There's also a new version code on the Setup tab, a number you increase with each release - see [Version and Game ID](/publishing/game-details#version-and-game-id).

WebPlayer, the server-based player, has been rewritten for Quest Viva - Quest 5's needed Windows, and this one runs on cross-platform .NET, so you can host it wherever you like. WasmPlayer has rather overtaken it since, though, so it's a specialist choice now: worth it if you specifically don't want players to be able to download your `.quest` file, for a treasure hunt or a competition say. See the [WebPlayer guide](/publishing/webplayer). 6.0 is the last version that will include WebPlayer: it will be removed in 6.1.

### Players can save without an account

In Quest 5, saving an online game meant having a textadventures.co.uk account. Quest Viva saves games to the browser's own storage instead, with multiple save slots, and players can also save a game out to a file to keep or move to another device. No account, no login.

### New in the editor

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

### New in the engine

Most of the scripting language is unchanged, but there are some additions:

- **Pages in text adventures.** Quest 5 only had pages in gamebooks. Quest Viva lets a text adventure switch into a page - a set of links the player chooses from - which is a much easier way to write conversations and dialogue trees. See [Using Pages](/tutorial/using-pages).
- **Asking the player something, without callbacks.** A script can now pause for the player and carry on at the next line: `GetInput()` for typed input, `Ask()` for a yes/no question, `ShowMenu()` for a menu, and `WaitForKeyPress` to wait for a keypress. These were in Quest 5 from the start, but were deprecated part-way through its life - in 5.4, and 5.5 for pausing - because they held a real thread while they waited. Quest Viva suspends the script instead, so they work properly, and the script editor offers them. The older forms that took a nested callback block - the `get input`, `ask`, `show menu` and `wait` script commands - still run, so games using them are unaffected, but they are no longer in the list of commands you can add. They were always the harder way to write anything that asks more than one question. One thing to know: while a script is waiting at one of these, the player can't save. See [Asking the player](/howto/scripting/asking-the-player) for the details, and for the callback forms of `Ask` and `ShowMenu` that don't have that limitation.
- **Questions and menus in the game text.** In games saved in Quest Viva, a question or menu now appears as numbered links in the game text instead of a pop-up window. Players can click an option or type its number.
- Plenty of smaller additions and fixes besides - the [changelog](https://github.com/textadventures/quest/blob/main/CHANGELOG.md) lists these from 6.0.0-beta.36 onwards.

A game saved in the Quest Viva editor is marked as ASL version 600 - `<asl version="600">` at the top of the file. Older games load and run unchanged - the version number just tells the engine which behaviour to use where something has been fixed in a way that would otherwise change an existing game. Once a game is saved in Quest Viva, Quest 5 can no longer open it. See [Game file versions](/reference/asl-versions).

## Why isn't it called Quest 6?

Because there's [another system](https://github.com/ThePix/QuestJS) already called that, although it's more commonly known as QuestJS.

## Something missing?

Quest Viva is under active development, so if there's something you relied on in Quest 5 that you can't find, please say so on [Discord](https://textadventures.co.uk/community/discord) or in [GitHub Discussions](https://github.com/textadventures/quest/discussions).
