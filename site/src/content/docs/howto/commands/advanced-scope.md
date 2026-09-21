---
title: Scope
description: Control which objects a command can find - backdrop scenery present in every room, commands that reach into another room, and objects the player can see but not touch
---

When the player types LOOK AT WALL, Quest Viva has to work out which object "wall" means. The set of objects it is willing to consider is that command's **scope**. By default it is everything the player can see in the room they are in, plus everything they are carrying. Anything else gets "I can't see that."

That default is right most of the time, and wrong in some familiar situations: walls and a sky that belong in every room, a barmaid behind a counter you have modelled as a separate room, spells that live in a spellbook, stock in a shop's back room.

| You want | Use |
|---|---|
| Objects present in every room - walls, floor, sky | The game's [Backdrop scope script](#objects-present-in-every-room) |
| One command to look somewhere else - CAST, BUY, PHONE | The command's [Scope field](#setting-the-scope-on-a-command) |
| A command to prefer what the player is holding, or what they are not | The command's Scope field: `inventory` or `notheld` |
| Scope that depends on the room, or on what has happened | A [`changecommandscope` script](#scope-that-changes-as-the-game-goes-on) |
| To ask, in a script of your own, what the player can see or touch | The [scope functions](#the-scope-functions) |

## Visible and reachable

Quest Viva keeps two questions apart: can the player *see* an object, and can they *touch* it.

- An object is **visible** if nothing opaque stands between it and the player. The contents of an open container are visible; so are the contents of a closed container that is transparent, like a glass case.
- An object is **reachable** if nothing closed stands in the way. The glass case's contents are visible but not reachable until it is opened.

An object whose `visible` attribute you have set to false is in neither list. Neither is anything in a dark room, apart from the player and anything flagged as a light source.

Scenery makes no difference here. An object marked **Scenery (do not display in room description)** on its _Setup_ tab is left out of the room description and the Places and Objects pane, but it is still in scope, which is exactly what you want for a mural the player can examine.

## What a command looks at by default

An `#object#` in a command pattern matches anything **visible**. An `#exit#` matches the exits the player can use. That is all a command does for you: matching an object is no promise that the player can touch it.

So with a pearl inside a closed glass showcase, LOOK AT PEARL works - but a command that actually moves the pearl has to check. The built-in commands do it like this, and so should yours:

```quest
if (not ListContains(ScopeReachable(), object)) {
  msg (BlockingMessage(object, ""))
}
else {
  msg ("You polish the " + GetDisplayAlias(object) + ".")
}
```

`BlockingMessage` finds the thing that is in the way and produces "The showcase is not open." - or the `blockingmessage` you set on the showcase yourself.

A few built-in commands narrow their scope rather than widening it:

| Command | Scope |
|---|---|
| TAKE | `notheld` |
| DROP, WEAR, REMOVE | `inventory` |
| PUT X IN/ON Y | `object1=inventory\|object2=container` |
| TAKE X FROM Y | `object1=contents\|object2=container` |

Everything else, including every verb, uses the default.

## Setting the scope on a command

Select the command in the tree. On its _Command_ tab there is a box labelled **Scope ("inventory", "notheld", room name or object attribute, Blank for everywhere)**. Verbs have the same box on their _Verb_ tab. It takes one of these:

| Value | What it matches |
|---|---|
| *(blank)* | Everything visible - the default |
| `inventory` | Everything the player is carrying |
| `notheld` or `room` | Everything visible that the player is not carrying, and not the player |
| `container` | The visible containers |
| `contents` | Everything inside a visible container |
| `world` | Every object in the game |
| `none` | Nothing, so that a `changecommandscope` script can supply the list |
| an object name | Everything inside that object, however deeply nested |
| an attribute name | See below |

If the text is not one of the keywords and not an object name, Quest Viva looks for an attribute of that name on the room the player is in, and then on the player. If the attribute holds an object, the contents of that object are used; if it holds an object list, the objects in the list are used.

That is how you scope a spell list. Keep the spell objects in a room the player never visits, give the player an object list attribute called `knownspells` holding the ones they have learned, and give a command with the pattern `cast #object#` the scope `knownspells`. Now CAST FIREBALL works if the player knows fireball, and CAST ICEBOLT gets "I can't see that." A shop works the same way with an attribute on the room: give the shop an object attribute `stock` pointing at the storeroom, and set the BUY command's scope to `stock`.

Separate several values with a semicolon to combine them - `contents;storeroom` matches anything inside a container the player can see, and anything in the storeroom. Order does not matter. For a command with more than one object, give each variable its own scope, separated by a vertical bar:

```
object1=inventory|object2=notheld
```

### Scope is a preference, not a restriction

If nothing in the scope matches what the player typed, Quest Viva tries again against everything visible. So `inventory` on WEAR means "if the player is holding a hat, mean that one rather than the hat on the floor" - it does not stop them wearing the hat on the floor. Even `none` falls back this way.

To refuse an object outright, test it in the command's script:

```quest
if (not ListContains(ScopeInventory(), object)) {
  msg ("You are not carrying that.")
}
```

Be careful with `world`. It is every object in the game, ignoring darkness, containers and the `visible` attribute, so a command scoped to `world` will happily match an object you meant to keep hidden.

## Objects present in every room

Walls, a floor, a ceiling, the sky, the river that runs past several rooms - you do not want a copy of these in every room, but the player will still type LOOK AT WALLS. The game's backdrop scope script adds objects to every scope Quest Viva works out, wherever the player is.

On the game object's _Features_ tab, tick **Show advanced scripts for the game object**. That reveals the _Advanced Scripts_ tab, whose last box is **Backdrop scope script (add objects to the "items" object list variable)**.

Put the objects themselves in a room the player can never get to - call it `backdrops` - mark each one as scenery on its _Setup_ tab, and write:

```quest
foreach (obj, GetDirectChildren(backdrops)) {
  list add (items, obj)
}
```

Three things to know:

- **Add to the list you were given.** `items = ListCombine(items, ...)` builds a new list and throws it away, so nothing happens and there is no error to tell you.
- **Backdrops are never listed in a room description**, because that listing only looks at the room's own children. They do show up in the Places and Objects pane, though, so mark them as scenery unless you want "sky" in the pane in every room.
- **This script runs several times a turn**, so keep it short, and never call a scope function such as `ScopeVisibleForRoom` from inside it. That function runs this script, which runs that function, and the game hangs on the loading screen with no error at all.

To vary the backdrops by place, test the room the player is in:

```quest
list add (items, walls)
if (GetBoolean(game.pov.parent, "outdoors")) {
  list add (items, sky)
}
```

## Scope that changes as the game goes on

For anything the Scope field cannot express - scope that depends on which room the player is in, or on what they have done - add a `changecommandscope` script. There is no control for it, so add it on the _Attributes_ tab of the command, the room or the game object: **Add attribute...**, name it `changecommandscope`, and set its type to **Script**.

The script runs after the Scope field has been dealt with, so it adds to whatever that produced. Quest Viva runs every one it finds, in this order: the command or verb itself, the player, the room the player is in, each room containing that one, and the game object. Inside the script you have:

| Variable | Holds |
|---|---|
| `items` | The object list to add to |
| `command` | The command or verb being matched |
| `variable` | Which part of the pattern is being resolved - `object`, `object1`, `object2` |
| `objtype` | `object` or `exit` |
| `matched` | A dictionary of the object variables already resolved for this command |

The barmaid behind the counter is a room script. Put this on the bar:

```quest
foreach (obj, GetDirectChildren(behind_the_counter)) {
  list add (items, obj)
}
```

Now a command typed in the bar can reach the barmaid, and the same command typed anywhere else cannot.

Use `command` when only one command should reach further - this one on the game object lets BUY see the storeroom from anywhere:

```quest
if (command.name = "cmd_buy") {
  foreach (obj, GetDirectChildren(storeroom)) {
    list add (items, obj)
  }
}
```

Use `variable` to scope each object of a two-object command differently, and `matched` when the second object depends on the first - to offer only the keys that fit the lock the player already named, say.

## The scope functions

These return an object list, possibly empty, of what the player can currently see or reach. Use them in your own scripts rather than walking the object tree yourself. The `...ForRoom` versions take a room; the others use the room the player is in. Full details are in the [scope function reference](/reference/functions/scope).

**What the player can touch**

| Function | Returns |
|---|---|
| [`ScopeReachable`](/reference/functions/scope#scopereachable) | Everything reachable in the room, plus what the player is carrying, plus the player |
| [`ScopeReachableNotHeld`](/reference/functions/scope#scopereachablenotheld) | The same, without the player or anything they are carrying |
| [`ScopeReachableInventory`](/reference/functions/scope#scopereachableinventory) | Carried objects that are reachable - not what is inside a bag they have closed |
| [`ScopeReachableForRoom`](/reference/functions/scope#scopereachableforroom), [`ScopeReachableNotHeldForRoom`](/reference/functions/scope#scopereachablenotheldforroom) | As above, for a room you name |

**What the player can see**

| Function | Returns |
|---|---|
| [`ScopeVisible`](/reference/functions/scope#scopevisible) | Everything visible in the room, plus the inventory, plus the player |
| [`ScopeVisibleNotHeld`](/reference/functions/scope#scopevisiblenotheld) | Everything visible the player is not carrying - but still including the player object itself |
| [`ScopeVisibleNotHeldNotScenery`](/reference/functions/scope#scopevisiblenotheldnotscenery) | The same, minus scenery and minus the player - this is what the Places and Objects pane lists |
| [`ScopeInventory`](/reference/functions/scope#scopeinventory) | Everything visible that the player is carrying |
| [`ScopeVisibleNotReachable`](/reference/functions/scope#scopevisiblenotreachable) | Visible but out of reach - what is inside the glass case |
| [`ScopeVisibleLightsource`](/reference/functions/scope#scopevisiblelightsource) | Visible light sources of a given strength |
| [`ScopeVisibleForRoom`](/reference/functions/scope#scopevisibleforroom) and the other `...ForRoom` forms | As above, for a room you name |

**Exits, commands and everything else**

| Function | Returns |
|---|---|
| [`ScopeExits`](/reference/functions/scope#scopeexits) | Exits the player can use from here, locked or not |
| [`ScopeExitsForRoom`](/reference/functions/scope#scopeexitsforroom), [`ScopeUnlockedExitsForRoom`](/reference/functions/scope#scopeunlockedexitsforroom) | Exits of a room you name, all of them or only the unlocked ones |
| [`ScopeCommands`](/reference/functions/scope#scopecommands) | Global commands plus the ones local to this room |
| [`AllObjects`](/reference/functions/scope#allobjects), [`AllRooms`](/reference/functions/scope#allrooms), [`AllExits`](/reference/functions/scope#allexits), [`AllCommands`](/reference/functions/scope#allcommands), [`AllTurnScripts`](/reference/functions/scope#allturnscripts) | Everything of that kind in the game, present or not |
| [`GetDirectChildren`](/reference/functions/scope#getdirectchildren), [`GetAllChildObjects`](/reference/functions/scope#getallchildobjects) | What an object contains, one level down or all the way down |

## See also

- [Advanced game scripts](/howto/scripting/advanced-game-scripts) - the rest of the _Advanced Scripts_ tab
- [Containers and surfaces](/howto/world/containers) - open, closed and transparent, and what that does to reach
- [Handling multiple items (and all)](/howto/commands/handling-multiple) - what TAKE ALL picks up
