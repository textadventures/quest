---
title: GetDisplayNameLink
---

For Quest 5.3 and earlier:

    GetDisplayNameLink (object, string type, stringlist verbs)

For Quest 5.4 and later, there is no verbs parameter:

    GetDisplayNameLink (object, string type)

Returns a [string](/types#string) containing the full displayed name of an object.

This will be the [prefix](/attributes/prefix) + the result from [GetDisplayAlias](/functions/corelibrary/getdisplayalias) + the [suffix](/attributes/suffix).

If type is not an empty string (and, in Quest 5.3 and earlier, a verbs list is specified), the result will include the display alias wrapped in an \<object\> tag complete with verbs. This will mean the Quest interface will display a hyperlinked object name with a menu of verbs. In Quest 5.4 and later, the [displayverbs](/attributes/displayverbs) or [inventoryverbs](/attributes/inventoryverbs) are picked up automatically depending on the object's parent.
