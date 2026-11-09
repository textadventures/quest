---
title: Changing the interface during play
description: The Interface script commands - show and hide parts of the player, add panes, set the command pane, and turn autoscroll, click to continue and fading on and off - and why they still work after a saved game is loaded
---

The game's Interface tab sets up the interface at the start of the game. To change it partway through - to hide the compass while the player is blindfolded, say, or to add a pane once they find a notebook - use the script commands in the **Interface** category of the script editor. Each one is also a function you can call in code.

| Script command | Function | Does the same as this Interface tab option |
|---|---|---|
| Show/hide part of the interface | `SetPaneVisible (part, visible)` | **Show panes**, **Show location bar** and the like |
| Add a pane | `AddPane (id, title)` | |
| Set the content of a pane | `SetPaneContent (id, html)` | |
| Set the command pane | `SetCommandPane (commands)` | **Show a command pane** |
| Set the command bar hint | `SetCommandPlaceholder (text)` | **Hint shown in the empty command bar** |
| Scroll to new output | `SetAutoScroll (on)` | **Scroll to new text as it appears** |
| Continue on a click anywhere | `SetContinueOnClick (on)` | **A click anywhere continues after a wait** |
| Fade earlier output | `SetFadePreviousOutput (on)` | **Fade the text from earlier turns** |
| Map can be dragged and zoomed | `SetMapInteractive (on)` | **Map can be dragged and zoomed** |

They work the same in the new interface and the [classic interface](/customise/classic/).

## They last after loading a saved game

When the player loads a saved game, the player's page is built again from scratch, so a change made with JavaScript is lost unless the user interface initialisation script makes it again. These commands don't have that problem. Each one keeps what it set on the `game` object - mostly in the same attribute as the Interface tab option - and the game applies it again whenever it starts or a saved game is loaded. So you can use them from any script: a command, a room's "After entering the room", or a turn script.

## Showing and hiding parts

"Show/hide part of the interface" takes the name of a part, and whether to show it:

```quest
SetPaneVisible ("compass", false)
```

| Part | What it is |
|---|---|
| `panes` | All the panes |
| `inventory`, `objects`, `status`, `compass` | One of the standard panes. `objects` is Places and Objects |
| `commands`, `customstatus` | The command pane and the custom status pane |
| `location` | The location bar |
| `command-bar` | The command bar |
| `border` | The border either side of the page |
| `picture-frame` | The picture frame |
| `map` | The map |
| `exits` | The strip of exits under the text on a phone |

A pane you've added with "Add a pane" is its ID. For a part with an Interface tab option, such as the panes or the location bar, this changes the option too, so it's the same as if you'd set it on the tab.

To hide the command bar for a scene the player plays by clicking links, and bring it back afterwards:

```quest
SetPaneVisible ("command-bar", false)
```

```quest
SetPaneVisible ("command-bar", true)
```

Before hiding the command bar, check that the player can do everything they need by clicking - see [The player interface](/customise/player-interface#choosing-a-combination).

## Panes

"Add a pane", "Set the content of a pane" and "Set the command pane" are described on the [Panes](/customise/panes) page.

## Scrolling, waiting and fading

- **Scroll to new output** - on by default. Turn it off, and new text appears without the page scrolling to it, so the player reads at their own pace. Turn it back on afterwards.
- **Continue on a click anywhere** - while the game is waiting for the player (after "Wait for a key press", say), a click anywhere on the text continues, not just the Continue button.
- **Fade earlier output** - text from before the player's latest command is faded, so the newest stands out.

Turn it off for a scene, and on again in a later turn - for example, off in the script of a "read letter" command that prints a long letter, and on again when the player leaves the room:

```quest
SetAutoScroll (false)
msg ("The letter reads...")
```

```quest
SetAutoScroll (true)
```

## The command bar hint

"Set the command bar hint" changes the faint text in the empty command bar, which tells the player what to type:

```quest
SetCommandPlaceholder ("What do you say to the guard?")
```

Set it to `""` for no hint at all. To go back to the game's usual hint, set that text again.

## The map

"Map can be dragged and zoomed" lets the player move the map around and zoom in and out, and is on by default. Turn it off for a map that should stay fixed. See [Showing a map](/howto/rooms/map).

## From JavaScript

Each of these functions calls a member of `QuestUI`, the player's JavaScript interface, which your own JavaScript can call too - see [Calling the game from JavaScript](/customise/javascript#the-questui-object). A change made from JavaScript isn't kept, though, so it's lost when the player loads a saved game. Use the functions on this page from your scripts whenever you can.

## See also

- [Look and feel](/customise/look-and-feel) - the Interface tab's options
- [Panes](/customise/panes)
- [User interface functions](/reference/functions/user-interface)
