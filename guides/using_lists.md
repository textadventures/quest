---
layout: index
title: Using Lists
---

Lists are made up of any number of strings (for a [stringlist](../types/stringlist.html)) or objects (for an [objectlist](../types/objectlist.html)).

Creating Lists
--------------

Lists can be created by the [NewStringList](../functions/newstringlist.html) or [NewObjectList](../functions/newobjectlist.html) functions, or can be returned by functions such as the scope functions.

Example of generating an [objectlist](../types/objectlist.html) called *myList* and an empty [stringlist](../types/stringlist.html) called *MyStringList*:

     myList = ScopeVisibleNotHeld()
     myStringList = NewStringList()

An object or string can be retrieved from a list by its index using [ObjectListItem](../functions/objectlistitem.html) or [StringListItem](../functions/stringlistitem.html). For example, to retrieve the second item from the above list (remembering that they count from zero):

     StringListItem(myStringList, 1)

A string list can be converted to a string using the [Join](../functions/string/join.html) function. Conversely, a string can be broken up into a string list using the [Split](../functions/string/split.html) function, and this can be the easiest way to create a new string list.

     myStringList = Split("First;Second;Third", ";")

Iterating through list elements
-------------------------------

You can read a list in several ways. Note that lists are indexed from zero, so the item at index 2 will actually be the 3rd element.

-   [foreach](../scripts/foreach.html), the easiest method, will just iterate through the entire list.

<!-- -->

     foreach (item, ScopeVisible()) {
       msg ("The item is "+item.name)
     }

-   [for](../scripts/for.html) is a general loop method which will need more details about the length of your list so that it will not exceed the bounds (go past the end) of the list.

<!-- -->

     for (myItem, 0, ListCount(myList) - 1) {
       msg ("Current item is " + ObjectListItem(myList,myItem).name)
     }

Adding and removing items
-------------------------

To manipulate the list use *add* and *remove* commands.

-   [list remove](../scripts/list_remove.html) removes a named item from the list. **Note:** space between the word list and add/remove

<!-- -->

     if (ListContains(myList, player) {
       list remove(myList, player)            
     }

-   [list add](../scripts/list_add.html) adds strings or objects to a list.

<!-- -->

     list add(myStringList,"Text item 1")
     list add(myStringList,"Text item 2")            

Combining lists
---------------

To combine two lists into one, use the [ListCombine](../functions/listcombine.html) function.

     firstStringList = NewStringList()
     list add(firstStringList,"mouse")
     list add(firstStringList,"cat") 
     secondStringList = NewStringList()
     list add(secondStringList,"dog") 
     resultList = ListCombine ( firstStringList , secondStringList ) 
     foreach (item, resultList ) {
        msg (item)
     }
     // resultList contains "mouse", "cat", "dog"

To return a copy of a list with one element removed, use the [ListExclude](../functions/listexclude.html) function.

     firstStringList = NewStringList()
     list add(firstStringList,"mouse")
     list add(firstStringList,"cat") 
     list add(firstStringList,"dog")
     secondStringList = NewStringList()
     list add(secondStringList,"cat") 
     resultCombine = ListExclude ( firstStringList , secondStringList ) 
     foreach (item, resultCombine ) {
        msg (item)
     }
     // resultCombine contains "mouse" and "dog"

Sorting lists
-------------

(added in Quest 5.3.)

To sort a list, use the [ObjectListSort](../functions/objectlistsort.html) or the [ObjectListSortDescending](../functions/objectlistsortdescending.html) function.
