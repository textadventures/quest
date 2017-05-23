---
layout: index
title: ListItem
---

    ListItem (list, int index)

Returns a [string](../types/string.html) or an [object](../types/object.html), depending on whether the list is a [stringlist](../types/stringlist.html) or an [objectlist](../types/objectlist.html). Gets an item from the list by index. The index is zero-based. (The first item is at index 0, the second is at index 1, etc.)

Usually you will know the type of list that you're passing in, so you should use the [StringListItem](stringlistitem.html) or [ObjectListItem](objectlistitem.html) functions instead.

NOTE: This function is hard-coded and cannot be overridden.
