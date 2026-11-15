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

Games now play in a new interface, built for phones as much as for wide screens. It follows the player's light or dark mode, and players get a button to choose **Light** or **Dark** for themselves. You choose where the panes go: beside the game, in a sheet the player opens from beside the command bar, or nowhere - and something different on a phone, where by default they're in a drawer. The map is drawn afresh too, with room pictures, and can go above the game or in the panes. On a phone, a strip of buttons under the text shows the exits you can take, so players don't have to open the drawer to move. See [The player interface](/customise/player-interface).

New games use it, and so does any game once you save it in the 6.1 editor. Games made with older versions of Quest keep the [classic interface](/customise/classic/) until then, so a game you've already published plays exactly as it did. If your game has JavaScript of its own that changes the classic interface - with `JS.setCss` or jQuery, say - the editor tells you when you open it, with a **Keep the classic interface** button. [Moving to the new interface](/customise/moving-from-classic) has what to use instead of each of the common snippets.

### Themes, colours and your own stylesheet

The game's Display tab is now two tabs: **Appearance** for how the game looks, and **Interface** for what's on screen. Gamebooks get an Appearance tab too. See [Look and feel](/customise/look-and-feel).

- **New themes** - Parchment, Noir, Storybook, Midnight and Terminal join Standard, Novella and Typewriter, and the Theme chooser shows each one as a little page in its own colours and font. Retro and Hot Dog Stand are retired, though a game that uses one keeps it. A theme now only changes the look, not what's on screen, and **Reset** takes you back to the theme's own look.
- **Light and dark colours** - a new game leaves its colours to the player's light or dark mode. Give your game colours of its own, and you can give it a dark version of each, side by side with the light one. Every colour option has a colour picker.
- **Previews** - the Appearance tab shows sample text in your game's fonts and colours, in light and dark mode, and the Interface tab shows the layout in miniature, on a wide screen and on a phone.
- **A stylesheet** - add your own CSS on the Appearance tab, with syntax highlighting. The player has custom properties and part names for it to use, so your CSS works on a phone and in dark mode, and survives future versions. See [Styling the player with CSS](/customise/css).
- **Changes show while you play** - anything you change on these tabs, including the stylesheet, shows in an open Preview straight away, without restarting the game or losing your place.
- **Script commands for the interface** - a new Interface category in the script editor shows and hides parts of the interface, adds panes, sets the command pane and more, during play. Unlike the JavaScript you needed before, they still apply after the player loads a saved game. See [Changing the interface during play](/customise/interface-scripts).

### Check your game for problems

**Check game**, in the editor's **File** menu, finds problems in your scripts without playing the game: script commands and functions that don't exist, expressions that can't be worked out, a function given too many values, and two elements with the same name. Click a problem to go straight to the script it's in. **Publish** checks the game first too, so you know about any problems before your players do. See [Check the game](/howto/testing/troubleshooting#check-the-game).

### Easier script editing

- **Add or paste in the middle of a script.** Each command's ⋯ menu adds or pastes a command directly above or below it, instead of only at the end.
- **Collapse a block.** Each branch of an `if`, and the script inside a command such as `for` or `firsttime`, can be collapsed to one line, so you can see which `else` belongs to which `if`.
- **See where you are.** The editor outlines the block you're working in, and each "Add script" button says which block it adds to.
- **See what you've selected.** A ticked command is shaded with everything nested inside it, Copy says how many commands it took, shift-click selects a range, and **Select to end** selects everything from the first ticked command down. Several ticked commands can be moved up or down together.

See [Working with a script](/tutorial/using-scripts#working-with-a-script).

### Other editor improvements

- **Turning a feature off** asks whether to keep or remove what you set up for it, since those settings would still take effect in the game. See [Turning a feature off](/howto/objects/features#turning-a-feature-off).
- **A function named after a built-in one** - a function that returns a value but has the same name as one built into the engine, such as `CapFirst`, can't be called from an expression, so the editor now warns you. See [Overriding Core library functions](/customise/overriding).
- **Pasting several lines** into a list's add-item box adds one item per line.

### Reproducing your players' bugs

Every game now has a random seed, which decides its random numbers, and a saved game remembers where its random numbers had got to. So when a player finds a problem, they can open **Save / Load** and click **Copy for a bug report**, and send you the game's seed and everything they did. Paste it into a walkthrough, set the walkthrough's new **Random seed**, and it plays out exactly as it did for them, dice rolls and all.

Walkthroughs in the editor now always play with the same random numbers, so a walkthrough through something random plays the same way every time. See [Using walkthroughs](/howto/testing/walkthroughs#random-numbers).

### For players

- **A new Save / Load dialog.** Saving takes one click, and a save with no name is called after the room the player was in. Saves are listed newest first, each with Load and a menu to overwrite, rename, save to file or delete it. The dialog asks before loading or starting again would lose progress, and when a game starts it offers to continue from the newest save.
- **`LOOK IN` and `LOOK INSIDE`** list what's in a container. See [Containers](/howto/objects/containers).
- **The loading screen shows the game.** While a game loads, and on the Begin button screen, players see its title, and its author and cover art when the player knows them, instead of a big Quest Viva heading. The browser tab shows the game's name too.

### The qv command-line tool

`qv` plays, checks, tests, previews and publishes Quest games from the command line. It's handy if you like to work in a text editor, or to run your walkthroughs automatically every time you change your game. It can also let an AI agent play your game and tell you where it got stuck. It plays games from every version of Quest, back to Quest 1. See [The qv command-line tool](/howto/testing/command-line).

### Publishing

- **Link previews.** A game you publish as HTML has your game's title and description in it, so a link shared on Discord, a forum or social media shows them. The zip export also includes your cover art for the preview. See [Where these are used](/publishing/game-details#where-these-are-used).
- **WebPlayer has been removed.** Its 6.0 version is still available, but it can't play games saved by Quest Viva 6.1. See [WebPlayer](/publishing/webplayer).

### Games saved in 6.1 are version 610

A game saved in the 6.1 editor is marked as ASL version 610 - `<asl version="610">` at the top of the file - and it's what makes a game use the new interface. Quest Viva 6.0 and earlier can't open or play a game once it's saved this way.

So the editor now warns you. When you open a game made with an older version of Quest, a banner at the top says that changing it will save it in the newer format. Its **Download original** button saves the file exactly as you opened it, even after the editor has saved your changes, so you can always go back. See [Game file versions](/reference/asl-versions).

### The editor on textadventures.co.uk stays on 6.0

The online editor at textadventures.co.uk, which opens games stored on your textadventures.co.uk account, stays on Quest Viva 6.0 and doesn't get the features added in 6.1. Your games there are still there, and you can carry on editing them in that editor. To move a game over to 6.1:

1. Go to [Your games](https://textadventures.co.uk/your-games) on textadventures.co.uk and click **Download** next to the game, under "Quest online editor games". That gives you a `.zip` of the game and its assets.
2. In Quest Viva, go to the **Create** tab and either click **Import game file** and choose the `.zip`, which makes it a local draft in your browser, or unzip it first and open the folder with **Open game folder** (**Open game…** in the desktop app).

Once you have, publish it as a `.quest` file and upload that to textadventures.co.uk - see [Publishing on textadventures.co.uk](/publishing#publishing-on-textadventurescouk).

Plenty of smaller additions and fixes besides - see the [changelog](https://github.com/textadventures/quest/blob/main/CHANGELOG.md).

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
