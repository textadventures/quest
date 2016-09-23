---
layout: index
title: GetDisplayNameLink
---

For Quest 5.3 and earlier:

    GetDisplayNameLink (object, string type, stringlist verbs)

For Quest 5.4 and later, there is no verbs parameter:

    GetDisplayNameLink (object, string type)

Returns a [string](../../types/string.html) containing the full displayed name of an object.

This will be the [prefix](../../attributes/prefix.html) + the result from [GetDisplayAlias](getdisplayalias.html) + the [suffix](../../attributes/suffix.html).

If type is not an empty string (and, in Quest 5.3 and earlier, a verbs list is specified), the result will include the display alias wrapped in an \<object\> tag complete with verbs. This will mean the Quest interface will display a hyperlinked object name with a menu of verbs. In Quest 5.4 and later, the [displayverbs](../../attributes/displayverbs.html) or [inventoryverbs](../../attributes/inventoryverbs.html) are picked up automatically depending on the object's parent.
