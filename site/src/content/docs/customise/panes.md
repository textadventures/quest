---
title: Panes
description: Choose which panes appear and where they go, show status attributes as bars, give the player a pane of one-click commands, and add panes of your own
---

The panes show what the player is carrying and what's in the room, the Status pane and a compass. This page shows how to choose which panes appear and where they go, how to show your own information in them, and how to add panes of your own.

| You want to | Use |
|---|---|
| Choose which of the standard panes appear, and where | The _Interface_ tab of the `game` object |
| Show numbers or text, such as a score or the player's mood | [Status attributes](/reference/attributes/status), which appear in the Status pane |
| Show a number as a bar, such as health out of 100 | A status attribute with a maximum |
| Give the player one-click commands such as LOOK and WAIT | The command pane |
| Show your own HTML beside the game | The custom status pane, or a pane of your own |
| Show or hide a pane partway through the game | The "Show/hide part of the interface" script command |

Everything on this page works in both the new interface and the [classic interface](/customise/classic/), except where it says otherwise.

## The standard panes

Select the `game` object and go to the _Interface_ tab. **Show panes (inventory, places and objects, compass)** turns the panes on or off. With them on, you can:

- choose **Where the panes go**: **Beside the game (in a drawer on a phone)**, the default; **In a sheet opened from beside the command bar**, which keeps the text full width; or **No panes**
- choose differently for a narrow screen with **On a phone**: **Same as above**, the default, **In a drawer opened from the top bar**, the sheet, or none
- hide individual panes, with **Hide the compass**, **Hide the inventory** and **Hide places and objects**
- put Status and Compass above the inventory, so they don't move up and down as the inventory grows
- add the command pane and custom status pane, below

[Look and feel](/customise/look-and-feel#the-interface-tab) describes each option. The classic interface always has its panes beside the game, in a drawer on a phone.

The Status pane appears by itself whenever there is something to show in it: the built-in [score, health and money](/howto/score/score-health-money), and any status attributes you add.

### Pane headings

To change the pane headings, add this to the user interface initialisation script, which runs at the start and again whenever a saved game is loaded (tick "Show advanced scripts for the game object" on the _Features_ tab, and the script is on the _Advanced Scripts_ tab):

```quest
JS.QuestUI.setInterfaceString ("InventoryLabel", "You are holding")
```

The other headings are `StatusLabel`, `PlacesObjectsLabel` and `CompassLabel`. A game in another language gets these from its [language library](/customise/other-languages).

### Panes on a phone

On a narrow screen, the panes move to where **On a phone** puts them: by default, a drawer that the player opens with the ☰ button at the top. They're all still there, including your own, but the player can't see them while reading the text. Two things stand in for them:

- a strip of the exits under the text, instead of the compass
- the status attributes you choose to keep in view, in a bar under the location bar (see below)

Keep this in mind when deciding what goes in a pane. Anything the player must notice, such as health running low, should also be mentioned in the game text. And a game played mostly by clicking commands in a pane is awkward on a phone, where the player has to open the drawer for every click: the sheet layout suits it better, as it's opened from right beside the command bar.

## Status attributes as bars

In the new interface, a status attribute that's a number can be drawn as a bar, such as health out of 100, and kept in view on a phone. Both are set on the game's _Attributes_ tab:

- **Status attribute maximums (shown as bars)** - add the name of the status attribute, and its maximum: a number, such as `100`, or the name of another attribute on the same object, such as `maxhealth`. The Status pane shows the attribute's text with a bar under it.
- **Status attributes to keep in view on small screens** - add the name of a status attribute, and it's shown in a bar under the location bar whenever the panes are tucked away in their drawer or sheet, as they are on a phone.

For example, to show the player's magic points as a bar, give the `player` object two integer attributes on its _Attributes_ tab, `mp` and `maxmp`, both set to 10, and add `mp` to its status attributes with the format `Magic: !`. Then on the game's _Attributes_ tab, add `mp` to the maximums with the value `maxmp`, and to the list to keep in view.

The bar updates itself at the end of every turn, like any status attribute. The classic interface shows the text, without the bar.

## The command pane

The command pane shows a row of commands the player can click instead of typing, such as LOOK and WAIT.

Tick **Show a command pane** on the game's _Interface_ tab. Then set its commands with the "Set the command pane" script command, in the Interface category - in the game's start script, on the _Scripts_ tab, for example. In code:

```quest
SetCommandPane ("Look;Wait")
```

Separate the commands with semicolons. Each one is sent exactly as if the player had typed it, so you can use any command your game understands, including your own. To show different text from the command it sends, write the text, a colon, then the command:

```quest
SetCommandPane ("Look;Wait;Check pockets:inventory")
```

You can also pass a string list instead of the semicolons.

The game keeps the list, so the pane still shows it after the player loads a saved game. To change it during the game - to add SHOUT in a room with an echo, say - just set it again. On the room's _Scripts_ tab, in "After entering the room":

```quest
SetCommandPane ("Look;Wait;Shout")
```

and in "After leaving the room":

```quest
SetCommandPane ("Look;Wait")
```

`SetCommandPane` shows the pane too, if it isn't already shown. For a command that only works in one room, see [Commands for one room](/howto/commands/how-commands-work#commands-for-one-room).

## The custom status pane

The custom status pane shows whatever HTML you give it, so you can lay out information any way you like. Tick **Show a custom status pane** on the game's _Interface_ tab, then fill it with the "Set the content of a pane" script command, using the ID `customstatus`:

```quest
SetPaneContent ("customstatus", "<b>Condition</b><br/>Poisoned")
```

This replaces everything in the pane. It has no heading of its own, so include one if you want one.

The game keeps the pane's content, so it's still there after the player loads a saved game. Nothing updates it for you, though, so the usual pattern is a function that draws the whole pane, called whenever the information changes. This example shows the player's mood. Give the `player` object a string attribute `mood`, set to `calm`, and add a function called `UpdateMoodPane`, with this script:

```quest
SetPaneContent ("customstatus", "<b>Mood</b><br/>" + CapFirst(player.mood))
```

Call it from the game's start script, so the pane is filled in at the start. To redraw the pane whenever the mood changes, add a script attribute called `changedmood` to the `player` object, on its _Attributes_ tab, that calls `UpdateMoodPane`. Quest Viva runs a `changed` script whenever the attribute it names changes, so the rest of your game only needs to do this:

```quest
player.mood = "nervous"
```

## Adding your own panes

A game can have as many panes of its own as it needs, after the built-in ones. Add one with the "Add a pane" script command, in the Interface category, giving it an ID and a heading - in the game's start script, say:

```quest
AddPane ("notes", "Notes")
```

Then fill it with "Set the content of a pane", as for the custom status pane:

```quest
SetPaneContent ("notes", "The butler was in the kitchen at nine.")
```

The ID is how you refer to the pane from then on: with `SetPaneContent`, with "Show/hide part of the interface" to hide it, and in your stylesheet as `[data-qv-pane="notes"]` (see [Styling the player with CSS](/customise/css#part-names)). Use letters, numbers and hyphens. The game keeps your panes and their content, so they're still there after the player loads a saved game. Adding a pane that's already there does nothing, so it's safe to do from a script that runs more than once.

## Showing and hiding panes during the game

The Interface tab sets up the panes at the start of the game. To show or hide one partway through, use the "Show/hide part of the interface" script command, in the Interface category, or `SetPaneVisible`:

```quest
SetPaneVisible ("compass", false)
```

The built-in panes are `inventory`, `objects`, `status`, `compass`, `commands` (the command pane) and `customstatus`, and `panes` is all of them together. A pane of your own is its ID. The game remembers, so the pane stays hidden after the player loads a saved game. See [Changing the interface during play](/customise/interface-scripts) for the other parts you can show and hide.

## See also

- [Changing the interface during play](/customise/interface-scripts)
- [Styling the player with CSS](/customise/css) - changing how the panes look
- [Status attributes](/reference/attributes/status)
- [Score, health and money](/howto/score/score-health-money)
