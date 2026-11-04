---
title: Look and feel
description: Choose a theme, fonts and colours, and set up the panes, command bar and room descriptions from the game object's Appearance, Interface and Room Descriptions tabs
---

Three tabs on the "game" element control how your game looks and how it describes rooms:

| You want to change | Tab |
|---|---|
| How it looks: the theme, fonts and colours, and the look of the location bar, command bar and verb menus | [Appearance](#the-appearance-tab) |
| What's on screen and how it behaves: the location bar, border, command bar, picture frame, panes, map, links, menus and game text | [Interface](#the-interface-tab) |
| What the player sees on entering a room or typing `LOOK` | [Room Descriptions](#the-room-descriptions-tab) |

Everything here can be changed without code. Each option is also an attribute of `game`, so a script can change it during play too, as described [at the end](#changing-the-look-during-the-game). For anything these tabs can't do, see [Styling the player](/customise/css).

A tip before you start: text adventures involve a lot of reading. Whatever you choose, check that the text is easy to read, including on a phone.

## The Appearance tab

### Theme

The **Theme** dropdown is the quickest way to change the look of your game. It sets a group of the options below in one go:

| Theme | What it does |
|---|---|
| Quest Standard | The default: black Georgia text on white, with panes on the right. |
| Novella | A narrow 650-pixel column of text with extra space at the top, like a page of a book. Turns off the panes, the room name in the location bar and the border. |
| Retro | White text on black, in the Press Start 2P web font (falling back to Lucida Console). |
| Typewriter | Novella's layout, in the Special Elite web font (falling back to Courier New). |
| Hot Dog Stand | Yellow Comic Sans on red. Mostly a demonstration of what not to do. |

A theme only provides starting values. Anything you set yourself on the Appearance or Interface tab wins, so you can pick Novella and then turn the panes back on, or pick Retro and change the page colour.

### Text

- **Base font** - a font from the list, each with fallbacks, such as "Georgia, serif". These work on every device.
- **Web font** - optionally, any font from [Google Fonts](https://fonts.google.com/) (the **Browse available web fonts** link opens the catalogue). The player's browser downloads it from Google when the game starts. If it can't - because the player is offline, for example - the base font is used instead, so choose a base font that's a reasonable substitute.
- **Font size** - in points. The default is 12.
- **Text colour** - the colour of the game text.
- **Interface font** - the font of the location bar, panes, menus and buttons. Leave it empty for the player's own.

For all the colour boxes on these tabs, you can choose a colour name from the list or type any HTML colour, such as `#336699`.

### Colours

The page is the column the text appears in, and the margins are either side of it. On a phone, the page fills the screen and the margins disappear.

- **Accent colour** - the game's one highlight colour, for links, the compass and the command prompt. Leave it empty to use the link colour if you've changed the colours, or the player's own if you haven't.
- **Link colour** and **Underline links** - how links look, when objects and exits are links (on the Interface tab). An object can have its own link colour on its [Object tab](/customise/object-verbs#the-object-tab).
- **Page colour** - the background of the page.
- **Margin colour** and **Margin image** - the colour either side of the page, or a picture to fill it instead.
- **Border colour** - the colour of the border either side of the page, when it's shown (on the Interface tab).

With the [classic interface](#advanced), **Colour scheme for panes** picks the panes' colours: Classic (light blue, the default), Midnight, Nature, Vanilla, Black, Blood, Tranquil or Parchment. Otherwise the panes follow the game's own colours.

### Location bar

When the location bar is shown (on the Interface tab), give it your own **Background colour**, **Text colour**, **Border colour** and **Background image**. Without them, it follows the game's colours.

With the classic interface, these appear once you untick **Classic location style**, which is the classic interface's blue bar.

### Command bar

When the command bar is shown (on the Interface tab), **Plain command bar with a prompt** drops the box or line the player types into, and shows a **Prompt** in front of what they type. The default is `>`. Any character works, such as `▶`, but check it appears in the fonts your players will have.

### Verb menus

When objects are links, **Font** and **Font size** style the pop-up menu of verbs that appears when the player clicks an object. On a phone, the menu's text is made larger if needed, so it's easy to tap.

### Advanced

These are under **Advanced** at the bottom of the tab.

- **Set a custom display width** - the width of the game in pixels. The panes move into the ☰ drawer whenever the window is narrower than this, so a very wide setting puts laptops into the phone layout too.
- **Set custom padding** - the space around the text, in pixels: **Top**, **Bottom**, **Left** and **Right**.
- **Stylesheet** - your own CSS, for anything these options can't do. See [Styling the player](/customise/css).

### Options from older versions

Some options from older versions of Quest are no longer offered: page opacity, a colour blend in the margins, a shadow around the command bar, and the verb menu's colours. A game that already uses one still shows it, with a **Remove** button, and it still works. The stylesheet can do the same for a new game.

## The Interface tab

### On screen

- **Show location bar** - the bar across the top of the screen, with the current room's name. It also holds the Save / Load button and, on a phone, the ☰ button, so it stays on screen even if you hide the room name.
- **Show border** - a thin line either side of the page. Its colour is on the Appearance tab.
- **Show command bar** - on by default. Without it, the player can only play by clicking links and panes. See [The player interface](/customise/player-interface) before turning it off. **Hint shown in the empty command bar** is a placeholder, such as "What now?".
- **Show a picture frame above the text** - a fixed area for a picture, which can change from room to room. **Empty the picture frame in rooms without a picture** clears it in rooms without one; otherwise the last picture stays. See [The picture frame](/howto/media/pictures#the-picture-frame).

### Game panes

**Show panes (inventory, places and objects, compass)** is on by default. **Where the panes go** puts them beside the game (in a drawer on a phone), in a sheet opened from beside the command bar, or nowhere, and **On a phone** can choose differently for a narrow screen.

With panes on, you can also:

- **Hide the compass**, **Hide the inventory** or **Hide places and objects** - hide individual panes. The Status pane appears by itself whenever there are [status attributes](/reference/attributes/status) to show.
- **Put status and compass above the inventory** - so they don't move up and down as the inventory grows.
- **Show a command pane** and **Show a custom status pane** - extra panes you fill from a script. See [Panes](/customise/panes).

### Map

**Show a map that draws itself as the player explores** adds the map. **Scale (pixels per grid square)**, **Height (pixels)**, **Exit width** and **Exit colour** control how it looks, **Map can be dragged and zoomed** lets the player move it, and **Map position** puts it above the game or in the panes. **Respond to clicks on the map**, under Advanced, lets your game react when the player clicks it. See [Showing a map](/howto/rooms/map).

### Links and menus

- **Make objects and exits in the text into links** - on by default. Clicking an object shows a menu of its [display verbs](/customise/object-verbs), and clicking an exit goes that way. Untick it for plain text. Links you write yourself with `{command:...}`, `{exit:...}` and `{page:...}` still work - see [Links](/howto/text/text-processor#links).
- **Deactivate a command link once it's been clicked** - each `{command:...}` link stops working after one use.
- **Keep a menu in the text after a choice is made** and **Print the chosen menu option** - for menus the player chooses from with numbered links: the "Show a menu" script command, and the "Which do you mean?" menu when a command matches more than one object. The `ShowMenu()` function (see [Asking the player](/howto/scripting/asking-the-player#menus)) always removes its menu and prints the choice, whatever these are set to.

### Game text

- **Scroll to new text as it appears** - on by default.
- **A click anywhere continues after a wait** - not just the Continue button.
- **Fade the text from earlier turns** - so the latest turn stands out.

### Advanced

**Use the classic interface** keeps the interface from older versions of Quest, for a game whose own JavaScript changes it. The options on these two tabs that only one interface uses are shown only for that one.

## The Room Descriptions tab

These options control what happens when the player enters a room or types `LOOK`.

- **Automatically generate room descriptions** - on by default. The game builds each description from the room's name, the objects in it, its exits and the description you wrote. Untick it to show only your own description, and list the objects and exits yourself.
- **Show room description when entering a room** - untick it and the player only sees the description when they type `LOOK`.
- **Put an extra newline when entering a room** - a blank line before the new room's description.
- **Display commands entered by the player** - repeat each command in the text, like `> take lamp`, including commands the player runs by clicking. With **Display hyperlinks in commands** too, the object's name in a repeated command is itself a link.
- **Put an extra newline before each turn** - a blank line before each command's output.
- **Automatically generate object display verbs list** - adds verbs from each object's _Verbs_ tab to its menus. See [Object verbs](/customise/object-verbs#verbs-added-automatically).
- **Clear screen when entering a room** - start each room on a clean screen. If the exit has a message, it's printed after the clear so the player still sees it.

### Room description layout

With automatic descriptions on, you choose the order of the four lines by numbering them 1 to 4: **Room name**, **Objects list**, **Exits list** and **Description**. Use 0 to leave a line out altogether. The default order is the room name, then the objects, the exits and your description.

- **Use "You are in" prefix (turn off to display room name in bold instead)** - "You are in a kitchen." or just **Kitchen** as a heading. Each room can change the prefix with **Description prefix** on its _Room_ tab.
- **Put a newline after the room name**, **...the objects list**, **...the exits list** and **...the description** - a blank line after that part.

## Changing the look during the game

Most of these options are read when the game starts. A few functions change the look during play:

| To change | Use |
|---|---|
| The text font | `SetFontName("'Courier New', Courier, monospace")` |
| To a web font | `SetWebFontName("Wallpoet")` |
| Text size | `SetFontSize(14)` |
| Text colour | `SetForegroundColour("DarkRed")` |
| Page colour | `SetBackgroundColour("Ivory")` |
| Margin image | `SetBackgroundImage("clouds.png")` |

The font, size and colour apply to text printed after the change, so you can switch fonts for one message and back again:

```quest
SetWebFontName ("Wallpoet")
msg ("A message scrawled on the wall reads: KEEP OUT.")
SetFontName ("Georgia, serif")
msg ("You decide to keep out.")
```

`SetWebFontName` loads the font from Google Fonts and switches to it; `SetFontName` switches back to a base font. You can use as many web fonts as you like this way, and switching back to one you've already used doesn't load it again. As with the Display tab's web font, it needs an internet connection, so if the player is offline the text appears in the current base font instead.

For all the functions, see [User interface functions](/reference/functions/user-interface).
