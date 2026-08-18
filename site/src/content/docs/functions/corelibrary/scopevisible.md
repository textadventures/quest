---
title: ScopeVisible
---

    ScopeVisible ()

Returns an [objectlist](/types#objectlist) containing all the objects which the player can currently see.

These objects can be looked at.

It is the union of two lists - [ScopeVisibleNotHeld](/functions/corelibrary/scopevisiblenotheld) (all the objects the player can see which are not in the inventory) and [ScopeInventory](/functions/corelibrary/scopeinventory) (all the visible objects in the inventory).
