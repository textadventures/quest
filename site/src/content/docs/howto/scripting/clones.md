---
title: Clones
description: Copy an object at runtime to make many of the same thing, and refer to the copies afterwards
sidebar:
  order: 8
---

A clone is a copy of an object made while the game is running. Build one goblin in the editor and you can put a dozen into the game world without building a dozen goblins; sell a copy of a cake and the original stays in the stockroom, so the baker never runs out.

The object you copy is called the *prototype*. Keep prototypes somewhere the player can never go - a room called `offstage` with no exits leading to it is the usual arrangement - so the player never meets the original.

| You want | Use |
|---|---|
| A copy, which you'll place yourself | `CloneObject(prototype)` |
| A copy in a particular room or container | `CloneObjectAndMove(prototype, parent)` |
| A copy in the room the player is in | `CloneObjectAndMoveHere(prototype)` |
| A copy whose initialisation script should run | `CloneObjectAndInitialise(prototype)` |

All of them return the new clone, so you can set it up as you go:

```quest
newgoblin = CloneObjectAndMoveHere(goblin)
newgoblin.look = "This goblin has a wooden leg."
```

In the editor, these are "Clone object" and "Clone object and move", in the Objects category. To keep hold of the clone, use "Set a variable or attribute" instead and choose the function there, so the clone lands in a variable you can then work with.

## Names and aliases

Every object in a game has a unique name, so a clone can't share its prototype's. Quest Viva gives it the prototype's name with a number on the end - `goblin1`, `goblin2`, and so on - and you can't choose it.

`CloneObject` and its relatives then set two attributes for you:

- `alias`, if the prototype doesn't already have one, is set to the prototype's name. That's what the player sees, so clones of `goblin` all look like "goblin".
- `prototype` points at the object that was cloned. Cloning a clone still points at the original prototype, not at the clone it came from.

(There is also a bare `Clone` function, which does neither of these. Use `CloneObject`.)

Clones that all share an alias are awkward for the player: typing `X GOBLIN` when there are three gets them a "Please choose which 'goblin' you mean" list of three identical entries. Give each clone its own alias as you create it. Picking from a list of adjectives and removing each one as it's used keeps them distinct:

```quest
z = CloneObjectAndMove(goblin, room)
adjective = PickOneString(goblin.adjectives)
list remove (goblin.adjectives, adjective)
z.alias = adjective + " goblin"
```

That gives you "a fat goblin", "a tall goblin" and so on. Put more adjectives in the list than you will ever have goblins.

## Cloning something with contents

Cloning an object clones everything inside it, all the way down, and each copy gets its own `alias` and `prototype` in the same way. Clone a chest with a coin in it and you get a new chest containing a new coin - the original chest keeps its own.

## Writing scripts that work on clones

A clone gets copies of the prototype's verbs and scripts. The trap is a script that refers to the prototype by name, because every clone will then act on the original:

```quest
// Wrong - always reports the prototype's health
msg ("The goblin has " + goblin.health + " hit points.")
```

Use `this` instead. In a script attached to an object, `this` is the object the script is running on - the clone, not the prototype:

```quest
msg ("The goblin has " + this.health + " hit points.")
```

The same goes for verbs, and for attribute change scripts.

`this` also works in text handled by the [text processor](/howto/text/text-processor), so a plain text description can use it:

```quest
The goblin has {this.health} hit points, and looks {this.mood}.
```

In text, `this` means the object in the command the player just typed, which is the right object whenever the player is looking at, taking or otherwise acting on the clone. If you need it somewhere else - text printed by a turn script, say - set `game.text_processor_this` to the clone first, as described in [Descriptions that don't change](/howto/scripting/randomness#descriptions-that-dont-change).

For a description that should be different for each clone but then stay put, run it through `ProcessText` when you create the clone and store the result:

```quest
z = CloneObjectAndMoveHere(goblin)
z.look = ProcessText("This goblin has a {random:red:green:yellow} tunic.")
```

## Finding a clone again

The variable you put the clone in is a local variable: it lasts until the end of the script, and then it's gone. The clone stays in the game world, but nothing is called `goblin1` in your scripts. There are three ways to reach it again.

**From a command.** A command with `#object#` in its pattern gives you a local variable `object`, which is the clone the player named, already resolved. Scripts for verbs get `this`. Most of the time this is all you need:

```quest
object.health = object.health - 5
```

**With `GetClone`.** This returns the first clone of a prototype directly inside a given parent, or `null` if there isn't one. Leave the parent off and it looks inside the player:

```quest
key = GetClone(rusty key, game.pov.parent)
if (not key = null) {
  msg ("A rusty key is lying here.")
}
```

**By filtering on `prototype`.** For all of them rather than the first:

```quest
goblins = FilterByAttribute(ScopeVisible(), "prototype", goblin)
msg ("There are " + ListCount(goblins) + " goblins here.")
```

`ScopeVisible`, `ScopeReachable`, `ScopeInventory` and `GetDirectChildren(room)` all work as the first argument - see [Using lists](/howto/scripting/lists#filtering). To test a single object, check the attribute directly. `HasObject` is safest, since objects that aren't clones don't have the attribute at all:

```quest
if (HasObject(object, "prototype") and object.prototype = stick) {
  msg ("That's one of your sticks.")
}
```

Neither `GetClone` nor a `prototype` filter ever picks up the prototype itself, which has no `prototype` attribute of its own.

Here's a command that needs four sticks and uses them up. Note that list positions count from zero:

```quest
sticks = FilterByAttribute(ScopeInventory(), "prototype", stick)
if (ListCount(sticks) < 4) {
  msg ("You need at least four sticks to do that.")
}
else {
  for (i, 0, 3) {
    destroy (ObjectListItem(sticks, i).name)
  }
  msg ("You lash four sticks together into a raft.")
}
```

## Getting rid of clones

`RemoveObject` only moves an object out of the world - it still exists, and it's still written into every saved game from then on. A game that clones freely and never tidies up (a shop the player visits a hundred times, a monster spawner) will grow a large number of these.

`destroy` removes the object completely. It takes the object's *name*, not the object itself, and it destroys anything inside it too:

```quest
destroy (object.name)
```

Destroy clones you're sure the player can no longer reach. For something the player might come back to - a dead monster they could still search - `RemoveObject` or leaving it in place is safer.

## See also

- [Monsters](/howto/rpg/combat#monsters) - clones used for a whole population of enemies, with health, attacks and loot
- [Shops that never run out](/howto/score/shop#variation-shops-that-never-run-out) - selling clones from a stockroom
- [Randomness](/howto/scripting/randomness#descriptions-that-dont-change) - giving each clone its own fixed description
- [Object functions](/reference/functions/objects) - the full reference
