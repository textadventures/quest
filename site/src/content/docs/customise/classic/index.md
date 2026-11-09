---
title: The classic interface
description: The interface from older versions of Quest - which games use it, when to choose it for a new game, and what it does differently from the new interface
---

Quest Viva has two player interfaces. A new game uses the new one, which works the same on a phone as on a wide screen, follows the player's light or dark mode, and lets you choose where the panes go. The **classic interface** is the one from Quest 5, kept so that older games play exactly as they always have.

These games use the classic interface:

- games written for Quest 5, or saved by an earlier version of Quest Viva, until they're saved again in the editor
- games written for Quest 4 or earlier
- any game with **Use the classic interface** ticked, under Advanced on the game's Interface tab

## When to choose it

Choose the classic interface only when your game's own JavaScript changes the player's interface, and you'd rather keep it than change it. That's code that:

- uses jQuery (`$(...)`) or jQuery UI
- reaches for the player's element IDs, such as `#divOutput`, `#txtCommand` or `#gamePanes`, in `JS.setCss`, `JS.eval`, `JS.addScript` or a stylesheet
- calls the player's own JavaScript functions, such as `JS.addText`, `JS.setCommands` or `JS.uiHide`, or replaces them, as in `scrollToEnd = function() {}`

None of that works in the new interface, which has none of those IDs or functions. When you open a game with code like this in the editor, a banner says what it found and offers to keep the classic interface. Click **Keep the classic interface** and the game plays as it did.

For any other game, the new interface is the better choice, and most of what games used to do with JavaScript now has an option or a script command. [Moving to the new interface](/customise/moving-from-classic) shows what to use instead of the common snippets.

## What's different

Most of the game's Appearance and Interface tab options work in both interfaces. A few work in only one, and the editor shows each of those only for the interface that uses it.

| | The new interface | The classic interface |
|---|---|---|
| Light and dark mode | Follows the player's, unless the game sets its own colours | Always light |
| **Where the panes go** and **On a phone** | Beside the game, in a sheet, or none | Always beside the game, in a drawer on a phone |
| **Accent colour**, **Interface font**, **Map position** | Yes | No |
| **Colour scheme for panes**, **Classic location style** | No: the panes follow the game's colours | Yes |
| Status attributes as bars, and kept in view on a phone | Yes | Shown as text |
| The previews on the Appearance and Interface tabs | Yes | No |
| Styling with your own CSS | [Part names and tokens](/customise/css) | [Element IDs](/customise/classic/css) |
| The player's own JavaScript functions | `QuestUI`, `sendCommand` and `ASLEvent` | All of them, as in Quest 5 - see [JS functions (classic interface)](/reference/js-classic/) |
| A `<script>` element in printed text | Doesn't run | Runs |

These work the same way in both:

- the script commands in the Interface category, and the functions behind them, such as `SetPaneVisible` and `SetCommandPane` - see [Changing the interface during play](/customise/interface-scripts)
- the **Stylesheet** box, under Advanced on the Appearance tab
- the `QuestUI` JavaScript object, and `sendCommand` and `ASLEvent` - see [Calling the game from JavaScript](/customise/javascript)

## Panes

In the classic interface, the command pane and custom status pane can also be filled with the player's own functions, `JS.setCommands` and `JS.setCustomStatus`, as games did in Quest 5. Those are lost when the player loads a saved game, unless the user interface initialisation script sets them again. The "Set the command pane" and "Set the content of a pane" script commands are kept for you, and work in both interfaces - see [Panes](/customise/panes).

## Turning it on or off

Tick or untick **Use the classic interface**, under Advanced on the game's Interface tab. The tabs change to show the options for the interface you've chosen, and Preview shows it straight away.

## See also

- [Styling the classic interface with CSS](/customise/classic/css)
- [JS functions (classic interface)](/reference/js-classic/)
- [Moving to the new interface](/customise/moving-from-classic)
