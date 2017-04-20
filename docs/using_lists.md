---
layout: index
title: Using Lists
---



A list is a sequence of entries in a prescribed order. Each entry is associated with a number, which is its position in the list.

There are three types of lists: string list, object lists and general lists. As you might expect, a string list can only hold strings, and an object list can only hold objects, but a general list can pretty much hold anything.

To create a list, then, we have three functions:
```
mystringlist = NewStringList()
myobjectlist = NewObjectList()
mylist = NewList()
```
It is best practice to use string lists or object lists wherever possible, rather than general lists.

If using the off-line editor, you can add a list to an object on the attributes page, but you are restricted to string lists. Object lists and general lists are useful for temporarily storing information (i.e., as local variables), but generally should not be used as attributes.




Adding and removing items
---------------------------

To add items to a list, use the `list add` command. To remove something from a list, use `list remove`. Here is an example that illustrates how we can add various types to a list:
```
l = NewList()
l2 = NewList()
list add (l, "one")
list add (l, fancy table)
list add (l, game.start)
list add (l, 42)
list add (l, 9.23)
list add (l, l2)
list remove(l, fancy table)
```
If you try to add something to a list that is already in the list, it will get added a second time.

If you try to remove something from a list that is not there, nothing will happen (no error message or warning is given). If you remove something that is in the list multiple times, only one instance is removed (the first in the list).



Retrieving from lists
----------------------

You can access an item in a list using the `ListItem` function. This takes the list and the position as parameters. Note that the positions count from zero, i.e, the first member of the list is at position 0.

It is also important to note that removing an item from a list will change the position of all subsequent entries. So:
```
l = NewList()
l2 = NewList()
list add (l, "one")
list add (l, fancy table)
list add (l, game.start)
list add (l, 42)
list add (l, 9.23)
list add (l, l2)
msg(ListItem(l, 3))
// -> 42
list remove(l, fancy table)
msg(ListItem(l, 3))
// -> 9.23
```
The first `msg` prints the number 42, because that is the fourth entry, and 3 is the fourth position, counting zero as the first. When the object `fancy table` is removed from the list, the position of 42 changes; it is now third, and the number 9.23 is fourth.

#### Retrieving from string lists and object Lists

If you know your list will contain only strings then it is better to use a string list rather than a general list. Similarly, if the list is just going to hold objects, use an object list.

If you know that what you are retrieving is a string, you can use `StringListItem`, and similarly you can use `ObjectListItem` for an object. You _can_ use these with general lists, but there will be a risk that what you retrieve is not what you thought it was (perhaps because something got removed from the list, and the positions all changed), and it is bad idea.




Retrieving from other lists
----------------------------

It can take Quest a moment to work out what the thing is with `ListItem`. Consider this code:
```
if (ListItem(l, 2) = "one") msg("Here")
if (ListItem(l, 2) = 42) msg("Here")
```
The first line is fine, but the second will through an error. Quest has extracted 42 from the list, but has yet to work out that it is an integer, and so throws an error when we try to compare it to a number. Assigning the value to a variable seems to give Quest enough time to work out what it is!
```
if (ListItem(l, 2) = "one") msg("Here")
n = ListItem(l, 2)
if (n = 42) msg("Here")
```


Quick string lists
-------------------

A quick way to get a string list is the Split command (especially useful for menu options) (the second parameter is the separator, and you can choose anything suitable):
```
l1 = Split("one|two|three", "|")
msg(l1)
// -> List: one; two; three; 
```
By the way, the `Join` function goes the other way; it converts from a string list to a string,


Quick object lists
------------------

There are various scope and other functions that will return a list of objects, and should be used where possible. See the [Scopes](scopes.html) page for more details.

Some other useful functions that return object lists:

-   [AllObjects](functions/allobjects.html)
-   [GetAllChildObjects](functions/getallchildobjects.html)
-   [GetDirectChildren](functions/getdirectchildren.html)





Iterating
----------

Often you will want to go through each member of a list. Use the `foreach` command to do this. It takes two parameters, the first being a variable to store an entry in, and the second being the list. It also requires a script.

This example will output each member of the list. The script will be run once for each entry in the list `l`, and for each iteration, `x` will have the value of that entry.
```
foreach(x, l) {
  msg("Entry: " + x)
}
```
Changing a list whilst in a foreach loop (i.e., adding or removing entries) will cause an error.

One way around that is to add entries you want to remove to another list while you iterate, and then to remove them later. For example, say we have a list of monsters and want to go through it and remove any that are dead, we could do this:
```
delete = NewObjectList()
foreach(obj, listOfMonsters) {
  if (obj.dead) {
    list add(delete, obj)
  }
}
foreach (obj, delete) {
  list remove(listOfMonsters, obj)
}
```



Other functions
---------------

The `ListCount` function will return the number of entries in the list. Remember that the entries will number from zero to _one less than_ the number returned.
```
msg("My list has " + ListCount(myList) + " things in it.")
msg("The last entry is at position " + (ListCount(myList) - 1))
```
Use `ListContains` to determine if a list contains a specific entry.
```
if (ListContains(myList, player) {
  list remove(myList, player)            
}
```
The `ListCombine` function will return a new list made up by combining the two given lists. The lists must be of the same type.
```
myBigList = ListCombine(list1, list2)
```
The `ListExclude` function is the reverse, it will return a new list made up by subtracting the second list from the first. For string lists and object lists, you can also use it to exclude a single string or object as appropriate. In this example, the two `msg` commands print the same list, for both the player object is excluded.
```
l = NewObjectList()
l2 = NewObjectList()
list add (l, player)
list add (l, fancy table)
list add (l, bronze sword)
list add (l2, player)
msg (ListExclude(l, l2))
msg (ListExclude(l, player))
```
Note that `ListCombine` and `ListExclude` do not change the original lists at all.

As of Quest 5.7, the `IndexOf` function can be used to get the position of an element in a list (or -1 if it is not in the list).

You can also use `ObjectListSort` and `ObjectListSortDescending` to sort a list of objects according to a certain attribute.
http://docs.textadventures.co.uk/quest/functions/objectlistsort.html
http://docs.textadventures.co.uk/quest/functions/objectlistsortdescending.html





List arithmetic
---------------

You can use `+` and `-` on lists. These can be used to add and remove single elements from a list.
```
listOne = Split("one|two|three", "|")
listTwo = "zero" + listOne + "four"
msg(listTwo)
-> List: zero; one; two; three; four; 
listThree = listTwo - "two"
msg(listThree)
-> List: zero; one; three; four; 
listFour = listOne + listThree
msg(listFour)
-> List: one; two; three; zero; one; three; four; 
```

Also works for object lists:
```
objectListOne = NewObjectList()
objectListTwo = objectListOne + player
msg(objectListTwo)
```
And mixed lists, here in one step we add an object and a string:
```
list1 = NewList()
list2 = list1 + player + "player"
```

