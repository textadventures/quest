---
layout: index
title: Using Lists
---



A list is a sequence of entries in a prescribed order. Each entry is associated with a number, which is its position in the list.

There are three types of lists: string lists; object lists; and general lists. As you might expect, a string list can only hold strings, and an object list can only hold objects, but a general list can pretty much hold anything.

To create a list, then, we have three functions:
```
mystringlist = NewStringList()
myobjectlist = NewObjectList()
mylist = NewList()
```
It is best practice to use string lists or object lists wherever possible, rather than general lists.

If using the off-line editor, you can add a list to an object on the attributes page, but you are restricted to string lists. Object lists and general lists are useful for temporarily storing information (i.e., as local variables), but generally should not be used as attributes (sometimes it is the only way!).


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


### When adding or removing fails...

In a certain situation you may find you cannot add or remove an item from a list, and you get this error:

```
Error running script: Cannot modify the contents of this list as it is defined by an inherited type. Clone it before attempting to modify.
```

This typically happens when you try to modify the display or inventory verbs, but the list is actually an attribute of the type the object inherits from, rather than the object itself. If you are using the desktop version, you will be able to go to the _Attributes_ tab of the object, and will see that the attribute in question is greyed out.

There are two solutions.

The simplest is to add something to the list in the editor (bottom of the _object_ tab), and then delete it. This will force Quest to add the attribute to the object (you will find it is not in black on the _Attributes_ tab).

The alternative is to give the object a new list, using `NewStringList`, and then add all the values from scratch (or use the `Split` function discussed later).


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

It is good practice to consider a list as either:

- Static: The list will never have an element removed over the course of the game, so the position of each element will never change, and can be relied upon (adding elements is allowed as this will not change the position of existing elements).

- Variable: Elements can be removed from the list, so the position of an element is expected to change and cannot be reliable upon.


### Retrieving from string lists and object Lists

If you know your list will contain only strings then it is better to use a string list rather than a general list. Similarly, if the list is just going to hold objects, use an object list.

If you know that what you are retrieving is a string, you can use `StringListItem`, and similarly you can use `ObjectListItem` for an object. You _can_ use these with general lists, but there will be a risk that what you retrieve is not what you thought it was (perhaps because something got removed from the list, and the positions all changed), and it is bad idea.




### Retrieving from other lists

It can take Quest a moment to work out what the thing is with `ListItem`. Consider this code:
```
if (ListItem(l, 2) = "one") {
  msg("Here")
}
if (ListItem(l, 2) = 42) {
  msg("Here")
}
```
The first line is fine, but the second will throw an error. Quest has extracted 42 from the list, but has yet to work out that it is an integer, and so throws an error when we try to compare it to a number. Assigning the value to a variable seems to give Quest enough time to work out what it is!
```
if (ListItem(l, 2) = "one") {
  msg("Here")
}
n = ListItem(l, 2)
if (n = 42) {
  msg("Here")
}
```




Quick string lists
-------------------

A quick way to get a string list is the Split command (especially useful for menu options). The second parameter is the separator, and you can choose anything suitable (as of Quest 5.8, this is optional, and if omitted will default to a semi-colon):
```
l1 = Split("one|two|three", "|")
msg(l1)
// -> List: one; two; three; 

l1 = Split("one;two;three")
msg(l1)
// -> List: one; two; three; 
```
By the way, the `Join` function goes the other way; it converts from a string list to a string.



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
Changing a list whilst in a foreach loop (i.e., adding or removing entries) will cause an error:

```
Error running script: Collection was modified; enumeration operation may not execute.
```

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

### How many?

The `ListCount` function will return the number of entries in the list. Remember that the entries will number from zero to _one less than_ the number returned.
```
msg("My list has " + ListCount(myList) + " things in it.")
msg("The last entry is at position " + (ListCount(myList) - 1))
```

### Does it contain?

Use `ListContains` to determine if a list contains a specific entry.
```
if (ListContains(myList, player) {
  list remove(myList, player)            
}
```
Alternatively, you can use the `in` operator (but only in a local variable or attribute; extra brackets confuse it!):
```
if (player in myList) {
  list remove(myList, player)            
}
```

### Adding and taking away

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

### Compacting

If you have combined two lists, you may have one entry appear in the list multiple times. You can use `ListCompact` or `ObjectListCompact` to remove repeated elements (and null elements too). `ListCompact` can be used with any type of list, and will return a new generic list. `ObjectListCompact` can only be used with lists of objects, and will return an object list.


### Filtering

You can filter lists to pull out just the objects you are interested in. For example, you might want a list of characters in the current room. All characters are of the "npc_type" type.

```
allobjects = ScopeReachable()
charactersonly = FilterByType(allobjects)
```

You can also filter by attribute. Suppose you have clones a whole hoard of goblins, and now you want to list them. If they all have the alias "goblin", then you can do this:

```
allobjects = ScopeReachable()
goblinsonly = FilterByAttribute(allobjects, "alias", "goblin")
```

There is a reverse function, so we can get all the objects that are not goblins:

```
allobjects = ScopeReachable()
notgoblins = FilterByNotAttribute(allobjects, "alias", "goblin")
```

Perhaps you just want the characters that are not goblins. You can do that by filtering twice:

```
allobjects = ScopeReachable()
charactersonly = FilterByType(allobjects)
charactersnotgoblins = FilterByNotAttribute(charactersonly, "alias", "goblin")
```

Note that you can use any type of attribute. This example will get all the scenery objects in the room:

```
allobjects = ScopeReachable()
sceneryonly = FilterByAttribute(allobjects, "scenery", true)
```



### Where?

The `IndexOf` function can be used to get the position of an element in a list (or -1 if it is not in the list).

[IndexOf](functions/indexof.html)


### Sorting lists

You can use `StringListSort` and `StringListSortDescending` to sort a list of strings.

[StringListSort](functions/stringlistsort.html)
[StringListSortDescending](functions/stringlistsortdescending.html)

You can also use `ObjectListSort` and `ObjectListSortDescending` to sort a list of objects according to a certain attribute.

[ObjectListSort](functions/objectlistsort.html)
[ObjectListSortDescending](functions/objectlistsortdescending.html)








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




Randomising
-----------

You can pick a random string or object from a list using `PickOneObject` or `PickOneString`. If you want to do that several times, but avoid having any repeats, just remove the selected from the list.

```
list = Split ("One;Two;Three")
for (i, 1, 5) {
  if (ListCount(list) > 0) {
    s = PickOneString (list)
    list remove (list, s)
    msg (list)
  }
  else {
    msg ("Default")
  }
}
```

Inside the loop, we check if there are any left in the list. If there are, one is selected at random, removed from the list, and displayed. If the list is empty, a default message is displayed.

This also allows us to shuffle a list.

```
list = Split ("One;Two;Three")
shuffled = NewStringList()
while (ListCount(list) > 0) {
  s = PickOneString (list)
  list remove (list, s)
  list add (shuffled)
}
```


