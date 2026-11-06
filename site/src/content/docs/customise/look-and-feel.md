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

A gamebook has one of these tabs, Appearance, with the theme, fonts and colours described here, and its layout: see [Publishing your gamebook](/tutorial/gamebook/publishing/#the-look-of-it).

A tip before you start: text adventures involve a lot of reading. Whatever you choose, check that the text is easy to read, including on a phone.

## The Appearance tab

### Theme

The **Theme** is the quickest way to change the look of your game. Each theme is shown as a little page in its own colours and font, split in two for a theme with light and dark mode colours. Choosing one sets a group of the options below in one go:

| Theme | What it does |
|---|---|
| Standard | The default: Georgia text in the default light or dark mode colours, whichever the player has chosen, with panes on the right. |
| Parchment | Dark brown text on cream, with rust links, in the Literata web font (falling back to Georgia). In dark mode, cream text on deep brown. |
| Noir | Near-black text on off-white, with deep red links, in the Courier Prime web font (falling back to Courier New). In dark mode, off-white on near-black. |
| Storybook | Plum text on a soft peach page, with coral links, in the Nunito web font (falling back to Trebuchet MS), slightly larger. In dark mode, pale pink on deep plum. |
| Novella | A narrow 650-pixel column of text with extra space at the top, like a page of a book. For more of a book's feel, turn off the panes, the location bar's room name and the border on the Interface tab. |
| Typewriter | Novella's layout, in the Special Elite web font (falling back to Courier New). |
| Midnight | Soft white text on deep navy, with amber links and a plain command bar, in dark mode or not. |
| Terminal | Green text on black in the VT323 web font (falling back to Lucida Console), with a plain command bar and a `>` prompt, in dark mode or not. |

Parchment, Noir and Storybook have colours for both light and dark mode, so players can switch between them (see [Light and dark](#light-and-dark)). Standard, Novella and Typewriter use the default light or dark mode colours. Midnight and Terminal look the same in either.

Two older themes are no longer offered: Retro, white text on black in the chunky Press Start 2P font (Terminal has taken its place), and Hot Dog Stand, yellow Comic Sans on red. A game that already uses one keeps it, and can still see and change it.

A theme is only the look: the fonts, colours, width and spacing on this tab. What's on screen, such as the panes, the location bar and the border, is up to you on the Interface tab, and choosing a theme leaves it as it is. A theme only provides starting values: anything you change on this tab afterwards wins, so you can pick Terminal and then change the page colour, for example.

Choosing a different theme clears the look you've set yourself (the fonts and colours on this tab, the command bar and verb menu styles, and anything else a theme sets, such as the width), so the new theme shows as it's meant to. Your background and location bar images, your stylesheet and link underlining are kept. The editor tells you which options it cleared, and **Undo** brings back the old theme with your settings.

To go back to the theme's own look without choosing a different one, click **Reset** beside the **Theme** dropdown. Hover over it to see which of your settings it clears. It's greyed out when there's nothing to reset.

### Text

- **Base font** - a font from the list, each with fallbacks, such as "Georgia, serif". These work on every device.
- **Web font** - optionally, any font from [Google Fonts](https://fonts.google.com/) (the **Browse available web fonts** link opens the catalogue). The player's browser downloads it from Google when the game starts. If it can't - because the player is offline, for example - the base font is used instead, so choose a base font that's a reasonable substitute.
- **Font size** - in points. The default is 12.
- **Interface font** - the font of the location bar, panes, menus and buttons. Leave it empty for the default.

For all the colour boxes on these tabs, type any HTML colour, such as `DarkOrange` or `#336699`, or click the palette button beside the box to pick one. As you type, the matching colour names are suggested, and the ▾ button lists them all. To clear a colour, empty the box or click its ×. Cleared, the text, link and page colours go back to the theme's own, which for the Standard theme is **Default**: the game follows the player's light or dark mode.

### Colours

The page is the column the text appears in, and the margins are either side of it. On a phone, the page fills the screen and the margins disappear.

At the top of **Colours**, a preview shows some sample text in the game's font and colours, with the location bar and command bar when they're turned on. It shows the game in light mode and dark mode side by side, or once when the game looks the same in both, with the reason underneath. The classic interface has no preview.

The page, text, link, accent, margin and border colours are in a table with a **Light** and a **Dark** column (see [Light and dark](#light-and-dark) for the Dark one):

- **Page colour** - the background of the page.
- **Text colour** - the colour of the game's text. Leave it empty to follow the player's light or dark mode.
- **Link colour** - how links look, when objects and exits are links (on the Interface tab). An object can have its own link colour on its [Object tab](/customise/object-verbs#the-object-tab).
- **Accent colour** - the game's one highlight colour, for links, the compass and the command prompt. Leave it empty to use the link colour if you've changed the colours, or the default if you haven't.

Below the table:

- **Underline links** - whether links are underlined.
- **Margin image** - a picture to fill the margins instead of their colour.
- **Margin colour** - the colour either side of the page.
- **Border colour** - the colour of the border either side of the page, when it's shown (on the Interface tab).

#### Light and dark

A new game leaves **Text colour**, **Link colour** and **Page colour** empty, so it follows the player's light or dark mode: dark text on a light page for a player whose device is in light mode, and light text on a dark page in dark mode. An **Accent colour** on its own keeps this, and is lightened in dark mode so it stays readable.

Setting the text, link or page colour gives the game its own colours instead, which stay the same in dark mode. Any of the three you leave empty is then black text, blue links or a white page. That way, your colours are never mixed with the player's dark ones.

To give players in dark mode colours of your own too, set a page colour in the **Dark** column. Then the dark text, link and accent colours appear beside their light ones, and any of them you leave empty is the default dark mode colour. With a dark mode page colour, the game shows your light colours in light mode and your dark ones in dark mode.

While a game follows light and dark mode, either with the player's colours or with dark mode colours of your own, players get a light or dark button at the top of the game. It lets them choose **Light** or **Dark** whatever their device is set to, or go back to **Match my device**. Their choice is kept for every game they play in that browser.

A **Margin colour**, or the location bar's **Background colour** or **Text colour**, keeps the game in its light colours, without the button, until you give it a dark version in the **Dark** column too, since your light colour might not suit the player's dark text. A border colour without a dark version is the same in both modes. A colour blend, which has no dark version, keeps the game light, and so does changing the page colour from a script, with `SetBackgroundColour`. The preview at the top of **Colours** says which colour is keeping the game light.

With the [classic interface](#advanced), **Colour scheme for panes** picks the panes' colours: Classic (light blue, the default), Midnight, Nature, Vanilla, Black, Blood, Tranquil or Parchment. Otherwise the panes follow the game's own colours.

### Location bar

When the location bar is shown (on the Interface tab), give it your own **Background colour**, **Text colour**, **Border colour** and **Background image**. Without them, it follows the game's colours. Like the game's colours, its colours are in a table with a **Light** and a **Dark** column, for players in dark mode (see [Light and dark](#light-and-dark)).

With the classic interface, these appear once you untick **Classic location style**, which is the classic interface's blue bar.

### Command bar

When the command bar is shown (on the Interface tab), **Plain command bar with a prompt** drops the box or line the player types into, and shows a **Prompt** in front of what they type. The default is `>`. Any character works, such as `▶`, but check it appears in the fonts your players will have.

### Verb menus

When objects are links, **Font** and **Font size** style the pop-up menu of verbs that appears when the player clicks an object. Leave them empty for the interface font at the game's text size (with the classic interface, Arial at 9 points). On a phone, the menu's text is made larger if needed, so it's easy to tap.

### Advanced

These are under **Advanced** at the bottom of the tab.

- **Set a custom display width** - the width of the game in pixels. The panes move into the ☰ drawer whenever the window is narrower than this, so a very wide setting puts laptops into the phone layout too.
- **Set custom padding** - the space around the text, in pixels: **Top**, **Bottom**, **Left** and **Right**.
- **Stylesheet** - your own CSS, for anything these options can't do. See [Styling the player](/customise/css).

### Options from older versions

Some options from older versions of Quest are no longer offered: page opacity, a colour blend in the margins, a shadow around the command bar, and the verb menu's colours. A game that already uses one still shows it, with a **Remove** button, and it still works. The stylesheet can do the same for a new game.

## The Interface tab

At the top of the tab, a preview shows the player in miniature, on a wide screen and on a phone, in the game's colours. It shows the parts you turn on below, such as the location bar, picture frame, map, command bar and panes, where the layout puts them, with the panes in their order. Hover over a part to see its name. The classic interface has no preview.

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

`SetWebFontName` loads the font from Google Fonts and switches to it; `SetFontName` switches back to a base font. You can use as many web fonts as you like this way, and switching back to one you've already used doesn't load it again. As with the Appearance tab's web font, it needs an internet connection, so if the player is offline the text appears in the current base font instead.

For all the functions, see [User interface functions](/reference/functions/user-interface).
