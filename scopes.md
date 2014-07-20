---
layout: index
title: Scopes
---

Core.aslx defines:

-   [ScopeInventory](functions/corelibrary/scopeinventory.html)
    -   Used to populate the "Inventory" list, and the list of objects returned by the "inventory" command
-   [ScopeVisibleNotHeld](functions/corelibrary/scopevisiblenotheld.html) = ScopeReachableNotHeld + ScopeVisibleNotReachable
    -   Used to populate the "Objects" part of the "Places and Objects" list
-   [ScopeReachable](functions/corelibrary/scopereachable.html) = ScopeReachableNotHeld + ScopeInventory
    -   All objects in this scope can be interacted with directly by the player
-   [ScopeVisibleNotReachable](functions/corelibrary/scopevisiblenotreachable.html)
    -   All objects in this scope can be seen, but can't be interacted with as they are either "far away" or inside a transparent container.
-   [ScopeReachableNotHeld](functions/corelibrary/scopereachablenotheld.html)
    -   These objects are in the current room and can be interacted with, but they're not in the player's inventory.
-   [ScopeVisible](functions/corelibrary/scopevisible.html) = ScopeVisibleNotHeld + ScopeInventory
    -   These objects can be looked at
-   [ScopeExits](functions/corelibrary/scopeexits.html)
    -   Exits that the player can use from the current location

