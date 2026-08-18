---
title: ScopeReachable
---

    ScopeReachable ()

Returns an [objectlist](/types#objectlist) containing all the objects which the player can currently reach.

All objects in this scope can be interacted with directly by the player.

This is the union of two lists - [ScopeReachableNotHeld](/functions/corelibrary/scopereachablenotheld) (all objects the player can reach in the current room, but are not in the inventory) and [ScopeReachableInventory](/functions/corelibrary/scopereachableinventory) (all the objects the player can reach in their inventory).
