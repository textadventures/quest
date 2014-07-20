---
layout: index
title: FormatObjectList
---

    FormatObjectList (string pre-list, object parent, string pre-final, string post-list, boolean use inventory verbs)

Returns a [string](../../types/string.html) containing a formatted list of objects.

Used by [ShowRoomDescription](showroomdescription.html) and the "inventory" command to display lists of visible and carried objects.

FormatObjectList will display children of listed objects within brackets, if the parent object can be seen through.

For example, this:

    FormatObjectList("You can see", player.parent, "and", "in this room.", false)

may return output like this:

> You can see a sofa, a lamp, a box (containing a diary and a pen) and a kitten in this room.

All object names will be hyperlinked to show a menu of [displayverbs](../../attributes/displayverbs.html). The final parameter lets you specify whether to use the [inventoryverbs](../../attributes/inventoryverbs.html) instead.
