---
title: Advanced game scripts
description: The game's Advanced Scripts tab - inituserinterface, unresolvedcommandhandler and scopebackdrop
sidebar:
  order: 9
---

The game object's _Advanced Scripts_ tab holds three scripts that are not tied to a moment in the story the way the [start and room scripts](/howto/scripting/when-scripts-run) are. Each one lets you take over something Quest Viva would otherwise do for itself.

The tab is hidden until you ask for it: on the game object's _Features_ tab, tick **Show advanced scripts for the game object**.

| Script | Runs when |
|---|---|
| **User interface initialisation script** (`inituserinterface`) | Before anything is drawn, at the start of the game and again whenever a saved game is loaded |
| **Unresolved command script** (`unresolvedcommandhandler`) | The player types something Quest Viva cannot match to any command or verb |
| **Backdrop scope script** (`scopebackdrop`) | Quest Viva works out which objects are present, several times a turn |

## The user interface initialisation script

`inituserinterface` is the earliest place your own code can run. It happens before the title, before the start script, and before the first room description - so anything that changes how the game looks should go here rather than in the start script, or the player sees the default appearance flash past first.

More importantly, it runs *again* every time the player loads a saved game. The start script does not: it belongs to the beginning of the story, and a loaded game is already part-way through. Anything that has to be re-applied to a fresh browser window - a background colour, a custom stylesheet, a panel you added with JavaScript - belongs in `inituserinterface`.

```quest
JS.setCss ("#qv-status", "background:#3b2f2f;color:wheat;border:none")
JS.addScript ("<style>#lstInventory li:hover { background: gold; }</style>")
```

See [Styling the player with CSS](/howto/ux/customising-the-ui) for what you can do from here. Don't print anything from this script - at the start of the game it runs before the title, so your text would appear above it.

There is also a function called `InitUserInterface`, which is empty in Core and which you can [override](/howto/scripting/creating-functions-which-return-a-value#overriding-a-built-in-function) with your own. It runs at exactly the same points, immediately before the `inituserinterface` script. Use the script on this tab unless you are writing a library, in which case overriding the function leaves the tab free for the game that includes your library.

Note that `game.pov` has not been worked out yet while this script runs, so you cannot ask where the player is.

## The unresolved command script

When Quest Viva cannot match what the player typed against any command or verb, it prints the `UnrecognisedCommand` template - "I don't understand your command." - and the turn ends. Add an `unresolvedcommandhandler` script and yours runs instead. What the player typed is in a string variable called `command`:

```quest
msg ("You try to " + command + ", but nothing comes of it.")
```

Useful things to do here:

- **Give the game its own voice.** A single unhelpful line is the one piece of text every player sees, and it usually sounds nothing like the rest of the game. (If all you want is different wording, you do not need this script at all - change the `UnrecognisedCommand` [template](/howto/world/changing-templates) instead. Use the script when the reply should vary.)
- **Log what players type.** Collecting unmatched input while testing tells you which verbs your players expect and you have not implemented.
- **Parse it yourself.** With the raw text in hand you can do your own matching - a magic-word system, a conversation mode where anything typed is treated as speech, or a fallback that strips a leading "please".

You do not need to worry about turn scripts here. An unrecognised command never finishes a turn, so turn scripts do not run after one anyway, and `SuppressTurnscripts` in this script does nothing.

Note that this script only catches text that matched nothing at all. A command that matched but named an object that is not there ("I can't see that.") never reaches it - see [Scope](/howto/commands/advanced-scope) for that case.

## The backdrop scope script

Every time Quest Viva works out which objects are present - to resolve LOOK AT WALL, to decide whether TAKE LAMP can reach the lamp, to build the list of things you can see - it runs `scopebackdrop`, if the game has one. The script is handed an object list called `items` holding everything it has found so far, and anything you add to that list counts as present too.

That is how you give a game walls, a floor, a sky or a river without creating a copy of them in every room:

```quest
list add (items, walls)
list add (items, floor)
list add (items, ceiling)
```

Keep the objects themselves somewhere off-stage - a room the player can never reach - and mark them as scenery so they do not appear in every room description.

Two things to watch. You must add to the `items` list that was passed in; building a new list and returning it, or using `ListCombine`, has no effect, because it is the passed-in list that Quest Viva goes on to use. And this script runs many times a turn, so keep it short - a loop over every object in the game will be felt.

[Scope](/howto/commands/advanced-scope) covers this in full, along with the other ways to change what a command can reach.

## See also

- [When scripts run](/howto/scripting/when-scripts-run) - every other script that fires by itself
- [Scope](/howto/commands/advanced-scope)
- [Styling the player with CSS](/howto/ux/customising-the-ui)
