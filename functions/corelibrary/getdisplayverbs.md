---
layout: index
title: GetDisplayVerbs
---

    GetDisplayVerbs (object)

Returns a [stringlist](../../types/stringlist.html) with the current display verbs for the object. If the object is in the current player's inventory, the [inventoryverbs](../../attributes/inventoryverbs.html) are used as a base, otherwise the [displayverbs](../../attributes/displayverbs.html) are used. If [autodisplayverbs](../../attributes/autodisplayverbs.html) is turned on, any verbs set up for the object will be added to the list returned.
