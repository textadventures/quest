---
title: Styling the player with CSS
description: Add your own CSS to the game's stylesheet, change the player's colours, fonts and spacing with its --qv- properties, and target its parts by name, on a phone and in light and dark mode
---

The Appearance and Interface tabs of the `game` object cover most of what you'll want to change about how your game looks - see [Look and feel](/customise/look-and-feel). When they don't go far enough, you can add your own CSS, because the player is a web page. This page shows where your CSS goes, the two things it can rely on - the player's **custom properties** and its **part names** - and some recipes that work on a phone and in light and dark mode.

This page is about the new interface. For a game that uses the classic interface, see [Styling the classic interface](/customise/classic/css).

## Where your CSS goes

Select the `game` object, go to the _Appearance_ tab, and type your CSS into **Stylesheet**, under Advanced at the bottom of the tab:

```css
qv-player {
    --qv-accent: darkorange;
    --qv-radius: 0;
}

[data-qv-part="location"] {
    font-variant: small-caps;
}
```

The stylesheet is applied whenever the game starts, and again whenever the player loads a saved game, so you don't need a script for it. Changes show in Preview straight away, without restarting the game.

Your rules always win over the player's own styles, whatever their specificity, so you never need `!important` to beat them. (The player keeps all its own CSS in cascade layers, and the stylesheet isn't in one.) The one exception is an option you've set on the Appearance or Interface tab, which the player applies to the element itself: change the option instead of overriding it.

A `url(...)` in the stylesheet can't name a file in your game folder. For a picture in the margins or the location bar, use **Margin image** or the location bar's **Background image** on the Appearance tab. Anything else needs a full `https://` address.

## Custom properties

Most of the player's look comes from CSS custom properties, all starting `--qv-`. Change one, and everything that uses it changes with it, on a wide screen and on a phone. Set them on `qv-player`, the element that holds the whole player:

```css
qv-player {
    --qv-font-ui: "Trebuchet MS", sans-serif;
    --qv-output-width: 36rem;
}
```

Most of them are also set by an option on the Appearance or Interface tab, so you'll only need these for what the options don't cover.

| Property | What it is | Option |
|---|---|---|
| `--qv-font-body`, `--qv-font-size` | The game text's font and size | **Base font**, **Web font**, **Font size** |
| `--qv-font-ui` | The font of the location bar, panes, menus and buttons | **Interface font** |
| `--qv-text`, `--qv-bg`, `--qv-link` | Text, page and link colours | **Text colour**, **Page colour**, **Link colour** |
| `--qv-accent` | The highlight colour: links, the compass, the command prompt, the menu's hover, the player on the map | **Accent colour** |
| `--qv-link-decoration` | `underline` or `none` | **Underline links** |
| `--qv-page-bg` | The margins either side of the page, and the background of the panes | **Margin colour** |
| `--qv-surface` | A lightly shaded background, for the map's zoom buttons and a gamebook's choices | |
| `--qv-border-colour` | The border either side of the page, and the outlines of menus, dialogs and the command bar | **Border colour** |
| `--qv-radius` | How rounded the corners of buttons, dialogs and the command bar are | |
| `--qv-space` | The space around the text, unless you set the padding, and inside dialogs | |
| `--qv-output-width` | The width of the column of text | **Set a custom display width** |
| `--qv-padding-top`, `-bottom`, `-left`, `-right` | The space around the text | **Set custom padding** |
| `--qv-status-fg`, `--qv-status-bg`, `--qv-status-border` | The location bar's text, background and border | The location bar's colours |
| `--qv-pane-fg`, `--qv-pane-bg`, `--qv-pane-heading-fg`, `--qv-pane-heading-bg` | The panes' text and background, and their headings' | |
| `--qv-pane-highlight` | The fill of a status attribute's bar | |
| `--qv-menu-fg`, `--qv-menu-bg`, `--qv-menu-hover-fg`, `--qv-menu-hover-bg` | A verb menu's colours, and the colours of the verb under the pointer | |
| `--qv-menu-font`, `--qv-menu-font-size` | A verb menu's font and size | The verb menu's **Font** and **Font size** |
| `--qv-command-fg`, `--qv-command-bg` | The command bar's text and background | |
| `--qv-focus-ring` | The outline around whatever has keyboard focus | |

Colours can be any CSS colour, and lengths need units, such as `12pt`, `700px` or `2rem`.

`--qv-surface`, `--qv-page-bg` and `--qv-border-colour` are mixed from the text and page colours unless you set them, so they suit whatever colours the game has, in light and dark mode. The panes and the menu follow the text and page colours the same way.

## Part names

Each part of the player carries a `data-qv-part` attribute, which you can select on. These are the only names in the player's markup that are sure to stay the same: its classes, IDs and element names may change in any version.

| Part | What it is |
|---|---|
| `output` | The game text |
| `command-bar` | The command bar, with its prompt |
| `location` | The location bar across the top |
| `border` | The column holding the game |
| `picture-frame` | The picture frame above the text |
| `map` | The map, above the game or in the panes |
| `panes` | All the panes together: beside the game, or in the drawer or sheet on a phone |
| `pane` | One pane. Its `data-qv-pane` says which: `inventory`, `objects` (Places and Objects), `status`, `compass`, `commands` (the command pane), `customstatus`, `map`, or the ID of a pane you've [added yourself](/customise/panes#adding-your-own-panes) |
| `exits` | The strip of exits under the text, shown on a phone instead of the compass |
| `status-bar` | The status attributes kept in view on a phone (see [Status attributes as bars](/customise/panes#status-attributes-as-bars)) |
| `menu` | A verb menu, or a menu of choices |

So `[data-qv-part="output"]` is the game text, and `[data-qv-pane="inventory"]` is the Inventory pane:

```css
[data-qv-pane="inventory"] {
    font-style: italic;
}
```

Links in the text are `a` elements with the class `cmdlink`. Object links also have the class `elementmenu`, exit links `exitlink`, and links you write with `{command:...}` `commandlink`.

The player sizes and positions its parts itself, differently on a wide screen and on a phone. Don't set a `width`, `height` or `position` on the `border`, `panes` or `command-bar` parts: it might look right on your screen and break the phone layout. Change the width with `--qv-output-width`, or **Set a custom display width** on the Appearance tab.

## Phones and narrow windows

When the player is narrower than 900 pixels - on a phone, say - `qv-player` has a `data-qv-narrow` attribute, and the panes move to where **On a phone** puts them. Start a rule with `[data-qv-narrow]` to apply it only then:

```css
[data-qv-narrow] [data-qv-part="location"] {
    font-size: 0.9rem;
}
```

## Light and dark

Unless the game has colours of its own, it follows the player's light or dark mode (see [Light and dark](/customise/look-and-feel#light-and-dark)). `qv-player` has `data-qv-scheme="light"` or `data-qv-scheme="dark"`, whichever it's showing, so you can give any rule a dark version:

```css
[data-qv-pane="customstatus"] {
    background: #fdf6e3;
}

qv-player[data-qv-scheme="dark"] [data-qv-pane="customstatus"] {
    background: #3b3226;
}
```

A rule that uses the player's own properties, such as `var(--qv-text)` and `var(--qv-surface)`, suits both without a dark version.

## Recipes

### A different look for the location bar

For plain colours, use the location bar's **Background colour**, **Text colour** and **Border colour** on the Appearance tab. For anything more:

```css
[data-qv-part="location"] {
    letter-spacing: 0.1em;
    text-transform: uppercase;
}
```

### Square corners and a tighter layout

```css
qv-player {
    --qv-radius: 0;
    --qv-space: 0.75rem;
}
```

### A pane of your own, styled

A pane added with "Add a pane" (see [Panes](/customise/panes#adding-your-own-panes)) is a `pane` part with its ID in `data-qv-pane`. For a pane with the ID `notes`:

```css
[data-qv-pane="notes"] {
    font-family: var(--qv-font-body);
    font-size: 0.9rem;
}
```

## Testing

Before you publish, check each change:

- **At phone width.** Make your browser window narrow, or use your browser's developer tools to emulate a phone about 375 pixels wide. Check the location bar, the text, and the panes wherever **On a phone** puts them.
- **In light and dark mode.** If the game follows the player's light or dark mode, use the light or dark button at the top of the game to see both.
- **After loading a saved game.** The stylesheet is applied again for you, but check anything a script changes.

To see what's on the page and which styles apply, right-click while playing and choose "Inspect" (or similar) to open your browser's developer tools. You'll see the player's classes there too, but only the part names and custom properties are sure to stay the same.

## See also

- [Look and feel](/customise/look-and-feel) - the options on the Appearance and Interface tabs
- [Panes](/customise/panes) - choosing panes, and adding your own
- [Moving to the new interface](/customise/moving-from-classic) - instead of the classic interface's element IDs
