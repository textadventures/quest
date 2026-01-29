---
layout: index
title: ScopeVisible
---

    ScopeVisible ()

Returns an [objectlist](../../types/objectlist.html) containing all the objects which the player can currently see.

These objects can be looked at.

It is the union of two lists - [ScopeVisibleNotHeld](scopevisiblenotheld.html) (all the objects the player can see which are not in the inventory) and [ScopeInventory](scopeinventory.html) (all the visible objects in the inventory).
