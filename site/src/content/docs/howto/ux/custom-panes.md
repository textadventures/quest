---
title: Panes
description: Set up the panes beside the game text, add a command pane of clickable commands, and show your own information in the Status pane or a custom status pane
---

On a wide screen, the panes sit to the right of the game text: Inventory, Status, Places and Objects, and the Compass. This page shows how to choose which panes appear, how to add a pane of clickable commands, and how to show your own information, such as a health bar, in a pane.

| You want to | Use |
|---|---|
| Choose which of the standard panes appear | The _Interface_ tab of the `game` object |
| Show numbers or text, such as a score or the player's mood | [Status attributes](/tutorial/status-attributes), which appear in the Status pane |
| Show a bar, a table or other formatting | A status attribute with HTML in its format, or a custom status pane |
| Give the player one-click commands such as LOOK and WAIT | A command pane |

## The standard panes

Select the `game` object and go to the _Interface_ tab. "Show panes (Inventory, Places and Objects, Compass)" turns the panes on or off, and with them on you can turn off individual panes, move Status and Compass to the top, add a command pane or custom status pane, and choose a "Colour scheme for panes". [Look and feel](/howto/ux/ui-style#game-panes) describes each option.

The Status pane appears by itself whenever there is something to show in it: the built-in [score, health and money](/howto/world/score-health-money), and any status attributes you add.

To change the pane headings, use `JS.setInterfaceString` in the user interface initialisation script (see below):

```quest
JS.setInterfaceString ("InventoryLabel", "You are holding")
```

The other headings are `StatusLabel`, `PlacesObjectsLabel` and `CompassLabel` - see [JS functions](/js/#setinterfacestring).

### Panes on a phone

When the window is narrower than the game - on a phone, for example - the panes move into a drawer, which the player opens with the ☰ button at the top right. They are still there, including your command pane and custom status pane, but the player can't see them while reading the text.

Keep this in mind when deciding what goes in a pane. Anything the player must notice, such as health running low, should also be mentioned in the game text. And a game played mostly by clicking commands in a pane is awkward on a phone, where the player has to open the drawer for every click.

## Command pane

A command pane shows a row of commands the player can click instead of typing, such as LOOK and WAIT.

On the game's _Interface_ tab, tick "Show a command pane (use JS.setCommands to set)". Then set the commands from the user interface initialisation script: tick "Show advanced scripts for the game object" on the _Features_ tab, and use "User interface initialisation script (run at start and also when loading a saved game)" on the _Advanced Scripts_ tab. That script runs at the start of the game and again when the player loads a saved game, which rebuilds the page. In code:

```quest
JS.setCommands ("Look;Wait")
```

Separate the commands with semicolons. Each one is sent exactly as if the player had typed it, so you can use any command your game understands, including your own. To show different text from the command it sends, write the text, a colon, then the command:

```quest
JS.setCommands ("Look;Wait;Check pockets:inventory")
```

You can also give a colour for the commands as a second parameter, such as `JS.setCommands ("Look;Wait", "blue")`.

### Changing the commands during the game

You can call `JS.setCommands` from any script to change the list - to add SHOUT in a room with an echo, say. But the initialisation script sets the list again when the player loads a saved game, so keep the current list in an attribute and have the initialisation script use that.

On the game's _Attributes_ tab, add a string attribute `panecommands`, set to `Look;Wait`. Set the initialisation script to:

```quest
JS.setCommands (game.panecommands)
```

Then on the room's _Scripts_ tab, in "After entering the room":

```quest
game.panecommands = "Look;Wait;Shout"
JS.setCommands (game.panecommands)
```

and in "After leaving the room":

```quest
game.panecommands = "Look;Wait"
JS.setCommands (game.panecommands)
```

If the player saves in that room and loads the game later, the pane still shows SHOUT.

For a command that only works in one room, see [Commands for a room](/howto/commands/commands-for-room).

## Showing a bar in the Status pane

A status attribute's format can contain HTML, so you can show a bar in the ordinary Status pane without writing any code to update it. Quest Viva updates the Status pane at the end of every turn and when a saved game is loaded.

The built-in Health is a percentage, which makes it easy to draw as a bar. Tick "Health" on the game's _Features_ tab, then replace the format Quest Viva gives it by adding this to the game's start script, on the _Scripts_ tab:

```quest
dictionary remove (game.povstatusattributes, "health")
dictionary add (game.povstatusattributes, "health", "Health: !%<div style='border:1px solid;height:8px'><div style='background:#c00;height:100%;width:!%'></div></div>")
```

Every `!` in the format is replaced with the value, so the inner `div` is as wide as the player's health: 70% of the pane when health is 70. The start script runs after Quest Viva has set up Health, which is why you change the format there rather than adding "health" to the status attributes yourself.

## Custom status pane

A custom status pane shows whatever HTML you give it, so you can lay out information any way you like. Tick "Show a custom status pane (use JS.setCustomStatus to set)" on the game's _Interface_ tab, then fill it with `JS.setCustomStatus`:

```quest
JS.setCustomStatus ("<b>Condition</b><br/>Poisoned")
```

This replaces everything in the pane, including its "Status" heading, so include a heading of your own if you want one.

Nothing updates the pane for you, and after the player loads a saved game it's back to showing just "Status", so the usual pattern is a function that draws the whole pane, called from the user interface initialisation script and whenever the information changes. This example shows the player's magic points as a bar. Give the `player` object two integer attributes on its _Attributes_ tab, `mp` and `maxmp`, both set to 10. Add a function called `UpdateMagicPane`, with this script:

```quest
percent = 100 * player.mp / player.maxmp
s = "Magic: " + player.mp + "/" + player.maxmp
s = s + "<div style=\"border: 1px solid; height: 8px; margin: 4px\">"
s = s + "<div style=\"background: #36c; height: 100%; width: " + percent + "%\"></div>"
s = s + "</div>"
JS.setCustomStatus (s)
```

The bar's width is a percentage, so it fits the pane on a wide screen and in the drawer on a phone. Call the function from the user interface initialisation script:

```quest
UpdateMagicPane
```

To redraw the pane whenever the magic points change, add a script attribute called `changedmp` to the `player` object, on its _Attributes_ tab, that calls `UpdateMagicPane`. Quest Viva runs a `changed` script whenever the attribute it names changes, so a spell only needs to do this:

```quest
player.mp = player.mp - 3
msg ("You cast a spell.")
```

## See also

- [Styling the player with CSS](/howto/ux/customising-the-ui) - changing how the panes look
- [Status attributes](/tutorial/status-attributes)
- [Score, health and money](/howto/world/score-health-money)
