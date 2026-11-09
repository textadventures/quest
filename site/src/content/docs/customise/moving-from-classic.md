---
title: Moving to the new interface
description: Move a game whose JavaScript changes the classic interface over to the new one - what to use instead of JS.setCss, jQuery, JS.setCommands, scrollToEnd and the other common snippets
---

A game from an older version of Quest plays on the [classic interface](/customise/classic/). Once you save it in the Quest Viva editor, it plays on the new one, which works better on a phone, follows the player's light or dark mode, and lets you choose where the panes go.

For most games, that's all there is to it. But a game whose own JavaScript changes the classic interface - with `JS.setCss`, jQuery, or the player's own functions - finds that code no longer works, because the new interface has none of the classic one's element IDs or functions. When you open a game like that, the editor says so in a banner at the top, with the first few places it found, and you have two choices:

- **Keep the classic interface.** Click **Keep the classic interface** in the banner, and the game plays as it always did. You can change your mind later: it's **Use the classic interface**, under Advanced on the game's Interface tab.
- **Move to the new interface**, by replacing each snippet with what this page suggests. Most of them become an option on the Appearance or Interface tab, a line in the game's stylesheet, or a script command.

## Finding what needs changing

Start with the places the banner lists. Then play the game in Preview, and open your browser's developer tools (right-click, then "Inspect") to see its console. A call to one of the classic interface's functions shows an error there, such as `setCss is not defined`. Check the game on a phone-sized window too: anything that moved parts of the page around by their IDs no longer applies.

## What to use instead

| Instead of | Use |
|---|---|
| `JS.setCss`, or `JS.addScript` with a `<style>` block | The game's **Stylesheet** - see [Styling](#styling) |
| `JS.uiHide`, `JS.uiShow`, `JS.panesVisible`, or `JS.setCss` with `display:none` | "Show/hide part of the interface" - see [Hiding parts of the interface](#hiding-parts-of-the-interface) |
| `JS.setCommands` | "Set the command pane" - see [Panes](#panes) |
| `JS.setCustomStatus` | "Set the content of a pane", with the ID `customstatus` |
| HTML for a pane of your own, added with `JS.addScript` or jQuery | "Add a pane" and "Set the content of a pane" |
| `JS.setInterfaceString` | `JS.QuestUI.setInterfaceString`, with the same names |
| `scrollToEnd = function() {}` | "Scroll to new output" off, or **Scroll to new text as it appears** on the Interface tab |
| A click handler that checks `_waitMode` and calls `endWait()` | **A click anywhere continues after a wait**, or "Continue on a click anywhere" |
| Fading `#divOutput`'s earlier text | **Fade the text from earlier turns**, or "Fade earlier output" |
| Setting `#txtCommand`'s placeholder | **Hint shown in the empty command bar**, or "Set the command bar hint" |
| Turning off the map's dragging and zooming | **Map can be dragged and zoomed**, or the script command of the same name |
| `JS.ShowGrid (0)` to hide the map | "Show/hide part of the interface", with `map` |
| `JS.setGameWidth`, `JS.setGamePadding`, `JS.setBackground`, `JS.colourBlend`, `JS.setPanes`, `JS.SetMenu...` | The options on the Appearance tab, or the stylesheet's [custom properties](/customise/css#custom-properties) |
| `JS.addText` | `msg`, or `QuestUI.addText` in your own JavaScript |
| Replacing or wrapping `addText`, `updateLocation` or other player functions to see what happens | `QuestUI.on` - see [Reacting to the player's turns](/customise/javascript#reacting-to-the-players-turns) |
| `isMobilePlayer()`, or checking the window's width | `[data-qv-narrow]` in the stylesheet - see [Phones and narrow windows](/customise/css#phones-and-narrow-windows) |
| jQuery (`$(...)`) in your own JavaScript | The browser's own functions, such as `document.querySelector` |
| `msg ("<script>...</script>")` | `JS.` and the function's name, or `JS.eval` |

The script commands are in the **Interface** category of the script editor. Each one is also a function, such as `SetPaneVisible`, and unlike the snippets they replace, what they change is kept when the player loads a saved game - see [Changing the interface during play](/customise/interface-scripts). That means much of what a game's user interface initialisation script used to do again after every load is no longer needed.

## Styling

The classic interface was styled by its element IDs, such as `#divOutput` and `#gamePanes`, usually from the user interface initialisation script. The new interface has a **Stylesheet** box instead, under Advanced on the game's Appearance tab, which is applied whenever the game starts or a saved game is loaded. Your rules there always win over the player's own, so you don't need `!important`.

Select parts of the page by their part names rather than IDs. This, in the classic interface:

```quest
JS.setCss ("#qv-status", "background:#3b2f2f;color:wheat")
JS.setCss ("#inventoryAccordion", "font-style:italic")
```

becomes the location bar's **Background colour** and **Text colour** on the Appearance tab, and this in the stylesheet:

```css
[data-qv-pane="inventory"] {
    font-style: italic;
}
```

| Classic ID | Part name |
|---|---|
| `#divOutput` | `[data-qv-part="output"]` |
| `#txtCommandDiv`, `#txtCommand` | `[data-qv-part="command-bar"]` |
| `#qv-status`, `#location` | `[data-qv-part="location"]` |
| `#gameBorder` | `[data-qv-part="border"]` |
| `#gamePanes` | `[data-qv-part="panes"]` |
| `#inventoryAccordion` | `[data-qv-pane="inventory"]` |
| `#placesObjectsAccordion` | `[data-qv-pane="objects"]` |
| `#statusVarsAccordion` | `[data-qv-pane="status"]` |
| `#compassAccordion` | `[data-qv-pane="compass"]` |
| `#commandPane`, `#customStatusPane` | `[data-qv-pane="commands"]`, `[data-qv-pane="customstatus"]` |
| `#gamePanel` | `[data-qv-part="picture-frame"]` |
| `#gridPanel` | `[data-qv-part="map"]` |

Inside each part, the new interface's elements and classes are its own business and may change, so style the parts themselves where you can, and change colours, fonts and spacing with the [custom properties](/customise/css#custom-properties) rather than on individual elements. A game that follows the player's light or dark mode needs its colours to suit both - see [Light and dark](/customise/css#light-and-dark).

## Hiding parts of the interface

Code like this, in the classic interface:

```quest
JS.uiHide ("#compassLabel, #compassAccordion")
JS.setCss ("#location", "display:none")
```

becomes:

```quest
SetPaneVisible ("compass", false)
SetPaneVisible ("location", false)
```

If it's for the whole game, untick the option on the Interface tab instead, such as **Hide the compass** or **Show location bar**. See [Changing the interface during play](/customise/interface-scripts#showing-and-hiding-parts) for the names of the other parts.

## Panes

```quest
JS.setCommands ("Look;Wait")
JS.setCustomStatus ("<b>Condition</b><br/>Poisoned")
```

becomes:

```quest
SetCommandPane ("Look;Wait")
SetPaneContent ("customstatus", "<b>Condition</b><br/>Poisoned")
```

Both are kept when the player loads a saved game, so a game that kept the command list in an attribute of its own, to set it again from the user interface initialisation script, no longer needs to. A pane you built yourself, with HTML for the classic interface's accordion, becomes one call to "Add a pane", then "Set the content of a pane" to fill it. See [Panes](/customise/panes).

## Your own JavaScript

Your own functions in `.js` files keep working, and so does calling them with `JS.`, `ASLEvent` and `sendCommand`. What changes is anything that relied on the classic interface itself:

- **jQuery** isn't loaded in the new interface. `$("#myBox").hide()` becomes `document.getElementById("myBox").hidden = true`, for example.
- **The player's functions**, such as `addText`, `endWait` or `isMobilePlayer`, don't exist. The [QuestUI object](/customise/javascript#the-questui-object) has what a game can rely on, and it's in the classic interface too, so a game can move to it before switching.
- **The player's elements** have new IDs and classes, which may change in any version. Use the part names above, or put what you need in an element of your own, printed with `msg` or added to a pane.
- **A `<script>` element** in text you print doesn't run. Put the code in a function in your `.js` file, and call it with `JS.`.

## Switching

When you've replaced everything, untick **Use the classic interface** on the Interface tab if it's ticked, and play the game through in Preview, on a wide window and a narrow one, and in light and dark mode.

## See also

- [The classic interface](/customise/classic/) - what each interface does differently
- [Styling the player with CSS](/customise/css)
- [Changing the interface during play](/customise/interface-scripts)
- [QuestUI](/reference/js/)
