---
title: "Scope functions"
sidebar:
  order: 7
---

Functions that will return a list of objects (in the loosest sense). See more [here](/howto/scripting/scopes).

## AllCommands
```
AllCommands()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [objectlist](/types#objectlist) of all commands defined in the game.

## AllExits
```
AllExits()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [objectlist](/types#objectlist) of all exits defined in the game.

## AllObjects
```
AllObjects()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [objectlist](/types#objectlist) of all objects defined in the game.

## AllRooms
```
AllRooms()
```

Returns an [objectlist](/types#objectlist) of all objects defined in the game that have "isroom" set to true. This will generally be all the rooms in the game.

## AllTurnScripts
```
AllTurnScripts()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [objectlist](/types#objectlist) of all turn scripts defined in the game.

## GetAllChildObjects
```
GetAllChildObjects (object)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [objectlist](/types#objectlist) containing all objects directly or indirectly contained by the parent object (i.e. including all children of children etc.)

For example, if object A contains object B, and object B contains object C, then GetAllChildObjects(A) returns a list containing B and C.

Use [GetDirectChildren](#getdirectchildren) instead if you only want children directly contained (in the above example, only B).

## GetDirectChildren
```
GetDirectChildren (object)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [objectlist](/types#objectlist) containing all objects directly contained by the parent object.

Use [GetAllChildObjects](#getallchildobjects) instead to return all objects directly *or* indirectly contained (i.e. including children of children).

## GetExitsList
```
GetExitsList ()
```

Returns an [objectlist](/types#objectlist) containing the exits from the current room that should be mentioned to the player: [ScopeExits](#scopeexits) with scenery exits and [look-only exits](#removelookonlyexits) filtered out.

## ListVisibleFor
```
ListVisibleFor (object character)
```

Returns an [object list](/types#objectlist) containing all the items worn by the character that are visible, i.e., not covered by another garment. For the current player, pass `game.pov`.

For more on handling wearable objects, see [here](/howto/world/wearables).

## RemoveDarkObjects
```
RemoveDarkObjects (objectlist)
```

Returns an [objectlist](/types#objectlist): the given list filtered down to objects that should still be visible in the dark - lightsources, plus anything the player is directly holding. Used when building a room's visible-objects list in the dark.

## RemoveLookOnlyExits
```
RemoveLookOnlyExits (objectlist)
```

Returns an [objectlist](/types#objectlist): the given list of exits with any exits flagged [lookonly](/attributes#lookonly) filtered out. Used by [GetExitsList](#getexitslist) so look-only exits are excluded from the exit list shown to the player.

## ScopeCommands
```
ScopeCommands ()
```

Returns an [objectlist](/types#objectlist) containing all the commands which are available to the player.

## ScopeExits
```
ScopeExits ()
```

Returns an [objectlist](/types#objectlist) containing all the exits which are available to the player (whether locked or not) from the current room.

## ScopeExitsForRoom
```
ScopeExitsForRoom(room)
```

Returns an [objectlist](/types#objectlist) containing all the exits which are available to the player (whether locked or not) in the specified room.

## ScopeInventory
```
ScopeInventory ()
```

Returns an [objectlist](/types#objectlist) containing all the visible objects which the player has in their inventory.

Used to populate the "Inventory" list, and the list of objects returned by the "inventory" command

## ScopeReachable
```
ScopeReachable ()
```

Returns an [objectlist](/types#objectlist) containing all the objects which the player can currently reach.

All objects in this scope can be interacted with directly by the player.

This is the union of two lists - [ScopeReachableNotHeld](#scopereachablenotheld) (all objects the player can reach in the current room, but are not in the inventory) and [ScopeReachableInventory](#scopereachableinventory) (all the objects the player can reach in their inventory).

## ScopeReachableForRoom
```
ScopeReachableForRoom (room)
```

Returns an [objectlist](/types#objectlist) containing all the objects which are reachable in the specific room.

All objects in this scope can be interacted with directly by the player.

If the player is in the specified room, this function returns the union of two lists - [ScopeReachableNotHeldForRoom](#scopereachablenotheldforroom) (all objects the player can reach in the current room, but are not in the inventory) and [ScopeReachableInventory](#scopereachableinventory) (all the objects the player can reach in their inventory). Otherwise it returns [ScopeReachableNotHeldForRoom](#scopereachablenotheldforroom)

## ScopeReachableInventory
```
ScopeReachableInventory ()
```

Returns an [objectlist](/types#objectlist) containing all the objects in the player's inventory that are in reach.

## ScopeReachableNotHeld
```
ScopeReachableNotHeld ()
```

Returns an [objectlist](/types#objectlist) containing all the objects which the player can reach in the current room (not including those the player currently has in their inventory).

These objects are in the current room and can be interacted with, but they're not in the player's inventory.

## ScopeReachableNotHeldForRoom
```
ScopeReachableNotHeldForRoom (room)
```

Returns an [objectlist](/types#objectlist) containing all the objects which the player can reach in the specified room (not including those the player currently has in their inventory).

These objects are in the specified room and can be interacted with, but they're not in the player's inventory.

## ScopeUnlockedExitsForRoom
```
ScopeUnlockedExitsForRoom(room)
```

Returns an [objectlist](/types#objectlist) containing all the unlocked exits which are available to the player in the specified room.

## ScopeVisible
```
ScopeVisible ()
```

Returns an [objectlist](/types#objectlist) containing all the objects which the player can currently see.

These objects can be looked at.

It is the union of two lists - [ScopeVisibleNotHeld](#scopevisiblenotheld) (all the objects the player can see which are not in the inventory) and [ScopeInventory](#scopeinventory) (all the visible objects in the inventory).

## ScopeVisibleForRoom
```
ScopeVisibleForRoom (room)
```

Returns an [objectlist](/types#objectlist) containing all the objects in the specified room which the player could see.

These objects can be looked at.

If the player is in the specified room, it is the union of two lists - [ScopeVisibleNotHeldForRoom](#scopevisiblenotheldforroom) (all the objects the player can see which are not in the inventory) and [ScopeInventory](#scopeinventory) (all the visible objects in the inventory). Otherwise it returns the list [ScopeVisibleNotHeldForRoom](#scopevisiblenotheldforroom).

## ScopeVisibleLightsource
```
ScopeVisibleLightsource (string light strength)
```

Returns an [objectlist](/types#objectlist) containing the visible objects and exits (from [ScopeVisible](#scopevisible) and [ScopeExits](#scopeexits)) that are lightsources with the given light strength ("weak" or "strong" - see [SetObjectLightstrength](/functions/core#setobjectlightstrength)). Used by [CheckDarkness](/functions/core#checkdarkness) to decide whether a dark room is lit.

## ScopeVisibleNotHeld
```
ScopeVisibleNotHeld ()
```

Returns an [objectlist](/types#objectlist) containing all the visible objects which are not in the player's inventory.

## ScopeVisibleNotHeldForRoom
```
ScopeVisibleNotHeldForRoom (room)
```

Returns an [objectlist](/types#objectlist) containing all the visible objects of the specified room which are not in the player's inventory.

## ScopeVisibleNotHeldNotScenery
```
ScopeVisibleNotHeldNotScenery ()
```

Returns an [objectlist](/types#objectlist) containing all the visible objects which are not in the player's inventory, and which are not scenery (having a "scenery" attribute set to "true").

Used to populate the "Objects" part of the "Places and Objects" list.

## ScopeVisibleNotHeldNotSceneryForRoom
```
ScopeVisibleNotHeldNotSceneryForRoom (room)
```

Returns an [objectlist](/types#objectlist) containing all the visible objects in the specified room which are not in the player's inventory, and which are not scenery (having a "scenery" attribute set to "true").

## ScopeVisibleNotReachable
```
ScopeVisibleNotReachable ()
```

Returns an [objectlist](/types#objectlist) containing all the objects which the player can see but cannot reach.

All objects in this scope can be seen, but can't be interacted with as they are either "far away" or inside a transparent container.

## ScopeVisibleNotReachableForRoom
```
ScopeVisibleNotReachableForRoom (room)
```

Returns an [objectlist](/types#objectlist) containing all the objects in the specified room which the player can see but cannot reach.

All objects in this scope can be seen, but can't be interacted with as they are either "far away" or inside a transparent container.
