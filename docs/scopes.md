---
layout: index
title: Scopes
---

Core.aslx defines various "scope" functions. Each of these will return an object list (possible emtpty). Scope functions that end "ForRoom" must be given a room as a parameter (other functions will default to the current room).

* [ScopeAllExitsForRoom](functions/corelibrary/scopeallexitsforroom.html)
* [ScopeExits](functions/corelibrary/scopeexits.html)
Exits that the player can use from the current location
* [ScopeExitsAll](functions/corelibrary/scopeexitsall.html)
Replaced in 5.4 by ScopeExits
* [ScopeExitsForRoom](functions/corelibrary/scopeexitsforroom.html)
* [ScopeUnlockedExitsForRoom](functions/corelibrary/scopeunlockedexitsforroom.html)
From Quest 5.7.

* [ScopeCommands](functions/corelibrary/scopecommands.html)

* [ScopeInventory](functions/corelibrary/scopeinventory.html)
Used to populate the "Inventory" list, and the list of objects returned by the "inventory" command
* [ScopeReachable](functions/corelibrary/scopereachable.html)
All objects in this scope can be interacted with directly by the player
* [ScopeReachableForRoom](functions/corelibrary/scopereachableforroom.html)
* [ScopeReachableInventory](functions/corelibrary/scopereachableinventory.html)
* [ScopeReachableNotHeld](functions/corelibrary/scopereachablenotheld.html)
These objects are in the current room and can be interacted with, but they're not in the player's inventory.
* [ScopeReachableNotHeldForRoom](functions/corelibrary/scopereachablenotheldforroom.html)
* [ScopeVisible](functions/corelibrary/scopevisible.html)
These objects can be looked at
* [ScopeVisibleForRoom](functions/corelibrary/scopevisibleforroom.html)
* [ScopeVisibleNotHeld](functions/corelibrary/scopevisiblenotheld.html)
Used to populate the "Objects" part of the "Places and Objects" list
* [ScopeVisibleNotHeldForRoom](functions/corelibrary/scopevisiblenotheldforroom.html)
* [ScopeVisibleNotHeldNotScenery](functions/corelibrary/scopevisiblenotheldnotscenery.html)
* [ScopeVisibleNotHeldNotSceneryForRoom](functions/corelibrary/scopevisiblenotheldnotsceneryforroom.html)
* [ScopeVisibleNotReachable](functions/corelibrary/scopevisiblenotreachable.html)
All objects in this scope can be seen, but can't be interacted with as they are either "far away" or inside a transparent container.
* [ScopeVisibleNotReachableForRoom](functions/corelibrary/scopevisiblenotreachableforroom.html)