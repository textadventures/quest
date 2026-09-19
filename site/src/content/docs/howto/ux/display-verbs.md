---
title: Object verbs
description: Choose which verbs appear when the player clicks an object, in the text or in the panes, and change them during the game
---

When the player clicks an object's name in the text, a menu of verbs appears - "Look at", "Take" and so on. The same verbs appear as buttons when the player selects the object in the Inventory or Places and Objects pane (in the ☰ drawer on a phone). Choosing one runs that verb on the object, just as if the player had typed "take lamp".

Each object has two lists:

- **Display verbs** - used when the object is somewhere around the player, such as in the room.
- **Inventory verbs** - used when the player is carrying it.

By default, an object's display verbs are "Look at" and "Take", and its inventory verbs are "Look at", "Use" and "Drop". This page shows how to change them in the editor and during the game.

## The Object tab

Select the object and go to its _Object_ tab. The options are:

- **Other names** - words the player can type for this object, as well as its name or alias. A ball could also be called "sphere" or "orb".
- **Alias to display in "Inventory" or "Places and Objects" pane** - a different name to show in the panes only, such as "bouncy ball" for an object called "ball".
- **Link colour** - a colour for this object's links, overriding the game's link colour on the _Display_ tab.
- **Display verbs** and **Inventory verbs** - the two lists of verbs.
- **Disable automatically generated display verb list for this object** and **Only display verbs from this object's Verbs tab** - see [Verbs added automatically](#verbs-added-automatically).

The verb options only appear if the game has hyperlinks or panes turned on (on the game's _Display_ and _Interface_ tabs), because without either the player never sees them.

## Adding and removing verbs

Add and remove verbs in the **Display verbs** and **Inventory verbs** lists as you need. If the player shouldn't be able to pick an object up, for example, remove "Take" from its display verbs.

A verb in these lists is simply the start of a command. Clicking "Kick" on a ball sends "kick ball", so the game must understand that command - otherwise the player gets "I don't understand your command." Anything you add here needs a verb on the object's _Verbs_ tab (see [Using verbs](/howto/commands/using-verbs)) or a command that handles it. Play through your game and try every verb on every object, both in the room and in the inventory.

Changing an object's **Type** on the _Setup_ tab changes its default verbs. A "Male character" or "Female character" has "Look at" and "Speak to" instead of "Look at" and "Take". Some features add verbs too: an openable container adds "Open" and "Close", and a switchable object adds "Switch on" and "Switch off".

### Verbs added automatically

When you add a verb to an object's _Verbs_ tab, it's added to the menus automatically, so a ball with a "hit" verb shows "Look at", "Take" and "Hit". You don't need to add it to the lists yourself.

To control every verb by hand instead:

- For one object, tick **Disable automatically generated display verb list for this object**. Only the verbs in its lists appear.
- For one object, tick **Only display verbs from this object's Verbs tab** to show just those verbs, without the lists. The lists disappear from the tab.
- For the whole game, untick **Automatically generate object display verbs list** on the game's _Room Descriptions_ tab.

## Adding and removing verbs on the fly

Sometimes the verbs should change during the game - a "Wear" verb for a hat, which becomes "Remove" once the hat is on. (Objects set up as [wearables](/howto/world/wearables) do this for you.)

The lists are the object attributes `displayverbs` and `inventoryverbs`, and you can set them in a script. To replace a list completely, use `Split`:

```quest
hat.inventoryverbs = Split("Look at;Drop;Wear", ";")
```

And when the hat is worn:

```quest
hat.inventoryverbs = Split("Look at;Remove", ";")
```

To add or remove a single verb, leaving the others alone, use `ListCombine` and `ListExclude`:

```quest
// Add "Attack"
orc.displayverbs = ListCombine(orc.displayverbs, Split("Attack", ";"))

// Remove "Attack" again
orc.displayverbs = ListExclude(orc.displayverbs, "Attack")
```

Both make a new list, so they work whether or not the object has its own list yet.

Avoid `list add` and `list remove` here. An object whose lists you haven't changed on the _Object_ tab gets them from its type, and changing a type's list stops the game with this error:

```
Error running script: Cannot modify the contents of this list as it is defined by an inherited type. Clone it before attempting to modify.
```

Verbs added automatically from the _Verbs_ tab aren't in `displayverbs` or `inventoryverbs`, so you can't remove them this way. For an object whose verbs change during the game, tick **Disable automatically generated display verb list for this object** and keep all its verbs in the lists.
