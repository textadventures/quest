---
title: GetDisplayVerbs
---

    GetDisplayVerbs (object)

Returns a [stringlist](/types/stringlist) with the current display verbs for the object. If the object is in the current player's inventory, the [inventoryverbs](/attributes/inventoryverbs) are used as a base, otherwise the [displayverbs](/attributes/displayverbs) are used. If [autodisplayverbs](/attributes/autodisplayverbs) is turned on, any verbs set up for the object will be added to the list returned.
