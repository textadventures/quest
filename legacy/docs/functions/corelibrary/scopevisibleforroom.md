---
layout: index
title: ScopeVisibleForRoom
---

    ScopeVisibleForRoom (room)

Returns an [objectlist](../../types/objectlist.html) containing all the objects in the specified room which the player could see.

These objects can be looked at.

If the player is in the specified room, it is the union of two lists - [ScopeVisibleNotHeldForRoom](scopevisiblenotheldforroom.html) (all the objects the player can see which are not in the inventory) and [ScopeInventory](scopeinventory.html) (all the visible objects in the inventory). Otherwise it returns the list [ScopeVisibleNotHeldForRoom](scopevisiblenotheldforroom.html).
