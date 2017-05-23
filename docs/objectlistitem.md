---
layout: index
title: ObjectListItem
---

    ObjectListItem (objectlist, int index)

Returns the [object](../types/object.html) from the list by the specified index. The index is zero-based. (The first item is at index 0, the second is at index 1, etc.)

You can use the [ListItem](listitem.html) function if you don't know the type of the list.

Example
-------

For example, to show a specific objects' name from a list, first create an [objectlist](../types/objectlist.html) called myList, in this example it is a list of objects that can be seen currently [ScopeVisibleNotHeld](../functions/corelibrary/scopevisiblenotheld.html).

     myList = ScopeVisibleNotHeld()

Now show the name of the second item in the list. Note that the second object is at index 1.

     msg ("myList item 2 is " + ObjectListItem(myList, 1).name)

This could be used with [GetRandomInt](getrandomint.html) to remove an item from the players inventory ([ScopeInventory](../functions/corelibrary/scopeinventory.html)) and place it into the current room (for example, if you're creating a poltergeist or thief).

NOTE: This function is hard-coded and cannot be overridden.
