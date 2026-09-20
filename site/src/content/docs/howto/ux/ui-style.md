---
title: Look and feel
description: Choose a theme, fonts and colours, and set up the panes, command bar and room descriptions from the game object's Display, Interface and Room Descriptions tabs
---

Three tabs on the "game" element control how your game looks and how it describes rooms:

| You want to change | Tab |
|---|---|
| The overall style in one go, fonts, colours, the background, links and menus | [Display](#the-display-tab) |
| Which parts of the screen appear: the map, panes, command bar, location bar, border, width, picture frame | [Interface](#the-interface-tab) |
| What the player sees on entering a room or typing `LOOK` | [Room Descriptions](#the-room-descriptions-tab) |

Everything here can be changed without code. Each option is also an attribute of `game`, so a script can change it during play too, as described [at the end](#changing-the-look-during-the-game). For anything these tabs can't do, see [Styling the player](/howto/ux/customising-the-ui).

A tip before you start: text adventures involve a lot of reading. Whatever you choose, check that the text is easy to read, including on a phone.

## The Display tab

### Theme

The **Theme** dropdown is the quickest way to change the look of your game. It sets a group of the options below in one go:

| Theme | What it does |
|---|---|
| Quest Standard | The default: black Georgia text on white, with panes on the right. |
| Novella | A narrow 650-pixel column of text with extra space at the top, like a page of a book. Turns off the panes, the room name in the location bar and the border. |
| Retro | White text on black, in the Press Start 2P web font (falling back to Lucida Console). |
| Typewriter | Novella's layout, in the Special Elite web font (falling back to Courier New). |
| Hot Dog Stand | Yellow Comic Sans on red. Mostly a demonstration of what not to do. |

A theme only provides starting values. Anything you set yourself on the Display or Interface tab wins, so you can pick Novella and then turn the panes back on, or pick Retro and change the page colour.

### Text

- **Colour** - the colour of the game text.
- **Base font** - a font from the list, each with fallbacks, such as "Georgia, serif". These work on every device.
- **Web font** - optionally, any font from [Google Fonts](https://fonts.google.com/) (the **Browse available web fonts** link opens the catalogue). The player's browser downloads it from Google when the game starts. If it can't - because the player is offline, for example - the base font is used instead, so choose a base font that's a reasonable substitute.
- **Font size** - in points. The default is 12.

For all the colour boxes on this tab and the Interface tab, you can choose a colour name from the list or type any HTML colour, such as `#336699`.

### Background

The page is the column the text appears in, and the margins are either side of it. On a phone, the page fills the screen and the margins disappear.

- **Page colour** - the background of the page.
- **Set page opacity** and **Opacity** - make the page partly see-through, from 0 (clear) to 1 (solid), so a margin image shows through it.
- **Margin colour** - the colour either side of the page.
- **Margin image** - a picture to fill the margins instead.
- **Colour blend for background?** - replaces the page and margin colours with a fade from **Colour at top** to **Colour at bottom**. The command bar keeps the page colour, so it shows up as a box of that colour against the blend.

### Hyperlinks

- **Hyperlinks: players can access object verbs by clicking object names** - on by default. Objects and exits named in the text become links: clicking an object shows a menu of its [display verbs](/howto/ux/display-verbs), and clicking an exit goes that way. Untick it for plain text. Links you write yourself with `{command:...}`, `{exit:...}` and `{page:...}` still work - see [Links](/howto/world/text-processor#links).
- **Link colour** and **Underline hyperlinks** - how links look. An object can have its own link colour on its [Object tab](/howto/ux/display-verbs#the-object-tab).
- **After using a command hyperlink, deactivate it** - each `{command:...}` link stops working once it has been clicked.

### Menus

These apply to menus the player chooses from with numbered links: the "Show a menu" script command, and the "Which do you mean?" menu when a command matches more than one object.

- **Do not clear menus after selections are made.** - leaves the menu in the text, with its links disabled, instead of removing it.
- **Print reponses after making menu selections.** - prints the chosen option, like an echoed command.

The `ShowMenu()` function (see [Asking the player](/howto/scripting/asking-the-player#menus)) always removes its menu and prints the choice, whatever these are set to.

### Verb Menus

When hyperlinks are on, these style the pop-up menu of verbs that appears when the player clicks an object: **Font**, **Font size**, **Background**, **Foreground**, **Hover background** and **Hover foreground**. On a phone, the menu's text is made larger if needed, so it's easy to tap.

## The Interface tab

### Map

**Map and Drawing Grid** adds a map that draws itself as the player explores. **Scale**, **Height (pixels)**, **Exit width** and **Exit colour** control how it looks, and **Map should respond to clicks?** lets your game react when the player clicks it. See [Showing a map](/howto/tasks/showing-a-map).

### Game panes

**Show panes (Inventory, Places and Objects, Compass)** is on by default. The panes sit to the right of the text on a wide screen. On a phone, or any window narrower than the game's width, they move into a drawer that the player opens with the ☰ button at the top right.

With panes on, you can also:

- **Turn off compass**, **Turn off inventory** or **Turn off places and objects** - hide individual panes. The Status pane appears by itself whenever there are [status attributes](/tutorial/status-attributes) to show.
- **Alternative pane order (status and compass at top)** - put Status and Compass above Inventory, so they don't move up and down as the inventory grows.
- **Show a command pane** and **Show a custom status pane** - extra panes you fill from a script. See [Panes](/howto/ux/custom-panes).
- **Colour scheme for panes** - Classic (light blue, the default), Midnight, Nature, Vanilla, Black, Blood, Tranquil or Parchment.

### Command bar

- **Show command bar** - on by default. Without it, the player can only play by clicking links and panes. See [The player interface](/howto/ux/ui-game-play) before turning it off.
- **Grey shadow box for the command bar** - a soft grey glow around the box.
- **Use a cursor instead of a box for commands?** - drops the box and shows a prompt in front of what the player types. Enter the prompt in **Cursor for commands**; the default is `>`. Any character works, such as `▶`, but check it appears in the fonts your players will have.

### Location bar

The bar across the top of the screen shows the current room's name. It also holds the Save / Load button and, on a phone, the ☰ button, so it stays on screen even if you hide the room name.

- **Show location bar** - show the room name.
- **Classic location style** - the blue bar. Untick it to use your own **Location bar colour**, **Location text colour**, **Location border colour** and **Location Bar Background Image**. With no colour set, the bar is transparent.

### Border

**Show border** draws a thin line either side of the page. Choose its colour with **Border colour**.

### Layout

These are under **Advanced** at the bottom of the tab.

- **Set a custom display width** - the width of the game in pixels, including the panes. The default is 950. The panes move into the ☰ drawer whenever the window is narrower than this, so a very wide setting puts laptops into the phone layout too.
- **Set custom padding** - the space around the text, in pixels: **Top**, **Bottom**, **Left** and **Right**.

### Picture frame

**Picture frame** adds a fixed area above the text for a picture, which can change from room to room. **Clear picture panel if room has no picture** empties it in rooms without one; otherwise the last picture stays. See [The picture frame](/howto/multimedia/images#the-picture-frame).

## The Room Descriptions tab

These options control what happens when the player enters a room or types `LOOK`.

- **Automatically generate room descriptions** - on by default. The game builds each description from the room's name, the objects in it, its exits and the description you wrote. Untick it to show only your own description, and list the objects and exits yourself.
- **Show room description when entering a room** - untick it and the player only sees the description when they type `LOOK`.
- **Put an extra newline when entering a room** - a blank line before the new room's description.
- **Display commands entered by the player** - repeat each command in the text, like `> take lamp`, including commands the player runs by clicking. With **Display hyperlinks in commands** too, the object's name in a repeated command is itself a link.
- **Put an extra newline before each turn** - a blank line before each command's output.
- **Automatically generate object display verbs list** - adds verbs from each object's _Verbs_ tab to its menus. See [Object verbs](/howto/ux/display-verbs#verbs-added-automatically).
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
