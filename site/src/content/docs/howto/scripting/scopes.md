---
title: Scopes
sidebar:
  order: 7
---

Core.aslx defines various "scope" functions. Each of these will return an object list (possible empty). Scope functions that end "ForRoom" must be given a room as a parameter (other functions will default to the current room).


## Exits

* [ScopeExits](/functions/scope#scopeexits) Exits that the player can use from the current location, i.e., all visible exits for this room, whether locked or not.
* [ScopeExitsForRoom](/functions/scope#scopeexitsforroom)
* [ScopeUnlockedExitsForRoom](/functions/scope#scopeunlockedexitsforroom)
* [AllExits](/functions/scope#allexits) All exits in the game.


## Commands

* [ScopeCommands](/functions/scope#scopecommands) Gets a list of all global commands and all commands local to the current room.
* [AllCommands](/functions/scope#allcommands) All commands in the game.


## Reachable objects

Quest makes a distinction between objects a player can reach and objects the player can see. Obviously with most objects, the player can do both, but this can be important in some situations.

Objects in a container are considered _reachable_ if the container is open. If an item's `visible` attribute is not set (i.e., it is `false`), or if the room is dark and the object is not a light source, then the object is not _reachable_.

* [ScopeReachableInventory](/functions/scope#scopereachableinventory) Items carried that are also _reachable_.
* [ScopeReachable](/functions/scope#scopereachable) All objects in this scope can be interacted with directly by the player, items in the current room or in the inventory that are _reachable_.
* [ScopeReachableForRoom](/functions/scope#scopereachableforroom) All objects in the room that are _reachable_ by the player. If this is the current room, then it will include items in the inventory that are _reachable_.
* [ScopeReachableNotHeld](/functions/scope#scopereachablenotheld) These objects are in the current room and can be interacted with, but they're not in the player's inventory.
* [ScopeReachableNotHeldForRoom](/functions/scope#scopereachablenotheldforroom)


## Visible objects

Objects in a container are considered _visible_ if it is either open or transparent (or both). If an item's `visible` attribute is not set (i.e., it is `false`), or if the room is dark and the object is not a light source, then the object is not _visible_.

* [ScopeInventory](/functions/scope#scopeinventory) All the _visible_ objects the player is carrying. Used to populate the "Inventory" list, and the list of objects returned by the "inventory" command.
* [ScopeVisible](/functions/scope#scopevisible) Any carried items and items in the room that are _visible_.
* [ScopeVisibleForRoom](/functions/scope#scopevisibleforroom) All objects in the room that are _visible_ by the player. If this is the current room, then it will include items in the inventory that are _visible_.
* [ScopeVisibleNotHeld](/functions/scope#scopevisiblenotheld) This is everything _visible_ in the current room.
* [ScopeVisibleNotHeldForRoom](/functions/scope#scopevisiblenotheldforroom)
* [ScopeVisibleNotHeldNotScenery](/functions/scope#scopevisiblenotheldnotscenery) As `ScopeVisibleNotHeld`, but excludes items tagged as scenery.
* [ScopeVisibleNotHeldNotSceneryForRoom](/functions/scope#scopevisiblenotheldnotsceneryforroom)
* [ScopeVisibleNotReachable](/functions/scope#scopevisiblenotreachable) All objects in this scope are _visible_ but not _reachable_.
* [ScopeVisibleNotReachableForRoom](/functions/scope#scopevisiblenotreachableforroom)


## Rooms

You can get a list of rooms (more specifically, objects with "isroom" set to true).

* [AllRooms](/functions/scope#allrooms)


## Others

* [AllObjects](/functions/scope#allobjects) All the objects (i.e., including rooms) in the game.
