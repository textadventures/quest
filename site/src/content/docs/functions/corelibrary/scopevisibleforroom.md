---
title: ScopeVisibleForRoom
---

    ScopeVisibleForRoom (room)

Returns an [objectlist](/types#objectlist) containing all the objects in the specified room which the player could see.

These objects can be looked at.

If the player is in the specified room, it is the union of two lists - [ScopeVisibleNotHeldForRoom](/functions/corelibrary/scopevisiblenotheldforroom) (all the objects the player can see which are not in the inventory) and [ScopeInventory](/functions/corelibrary/scopeinventory) (all the visible objects in the inventory). Otherwise it returns the list [ScopeVisibleNotHeldForRoom](/functions/corelibrary/scopevisiblenotheldforroom).
