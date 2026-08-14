---
title: ScopeReachableForRoom
---

    ScopeReachableForRoom (room)

Returns an [objectlist](/types/objectlist) containing all the objects which are reachable in the specific room.

All objects in this scope can be interacted with directly by the player.

If the player is in the specified room, this function returns the union of two lists - [ScopeReachableNotHeldForRoom](/functions/corelibrary/scopereachablenotheldforroom) (all objects the player can reach in the current room, but are not in the inventory) and [ScopeReachableInventory](/functions/corelibrary/scopereachableinventory) (all the objects the player can reach in their inventory). Otherwise it returns [ScopeReachableNotHeldForRoom](/functions/corelibrary/scopereachablenotheldforroom)
