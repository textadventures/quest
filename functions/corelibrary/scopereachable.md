---
layout: index
title: ScopeReachable
---

    ScopeReachable ()

Returns an [objectlist](../../../types/objectlist.html) containing all the objects which the player can currently reach.

All objects in this scope can be interacted with directly by the player.

This is the union of two lists - [ScopeReachableNotHeld](scopereachablenotheld.html) (all objects the player can reach in the current room, but are not in the inventory) and [ScopeReachableInventory](scopereachableinventory.html) (all the objects the player can reach in their inventory).
