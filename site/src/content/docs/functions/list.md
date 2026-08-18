---
title: "List functions"
sidebar:
  order: 6
---

Functions for manipulating lists. For a discussion on how to use lists, see [here](/howto/scripting/using_lists).

## Contains
```
Contains (object parent, object child)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the child object is contained by the parent. This doesn't necessarily mean that there is a direct parent-child relationship - for example if object A has parent B, and B has parent C, then

     Contains(C, A)

will return **true**.

## FilterByAttribute
```
FilterByAttribute (objectlist list, string attribute name, any value)
```

Returns a new object list containing only the objects in the given list for which the named attribute has the given value (which can be of any type).

Note that if the value is `null` this effectively filters for objects without the named attribute.

See also [FilterByNotAttribute](#filterbynotattribute).

You can omit the last value, and it will be assumed to be `null`.

## FilterByNotAttribute
```
FilterByNotAttribute (objectlist list, string attribute name, any value)
```

Returns a new object list containing only the objects in the given list for which the named attribute does _not_ have the given value (which can be of any type).

Note that if the value is `null` this effectively filters for objects with the named attribute, whatever the value.

See also [FilterByAttribute](#filterbyattribute).

You can omit the last value, and it will be assumed to be `null`.

## FilterByType
```
FilterByType (objectlist list, string typename)
```

Returns a new object list containing only the objects in the given list that are of the given type.

## IndexOf
```
IndexOf (list, anything)
```

Returns an [int](/types#int) - the position of the given element in the list, or -1 if it is not in the list. Note that lists count from zero.

```
list = Split("One;Two;Three;Four")
msg(IndexOf(list, "One")
// -> 0
msg(IndexOf(list, "Four")
// -> 3
msg(IndexOf(list, "Five")
// -> -1
```

## ListCombine
```
ListCombine (list, list)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Combines two [stringlists](/types#stringlist) or two [objectlists](/types#objectlist) or two generic lists (can cannot add a list of one type to another).

## ListCompact
```
ListCompact (any list list)
```

Returns a [list](/types#list), based on the given list, but with any repeated entries removed and any entries that are null removed. The canonical use is when combining two lists that might have some entries in common:

```
combinedlist = ListCompact (list1 + list2)
```

## ListContains
```
ListContains (list, any type item)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the list contains the item.

## ListCount
```
ListCount (list)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [int](/types#int) - the number of items in the list.

## ListExclude
```
ListExclude (list, any type item or list)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a copy of the [stringlist](/types#stringlist) or [objectlist](/types#objectlist), with the specified item removed, or with all the items removed if the second parameter is a list.

Note that this is different to the [list remove](/scripts#list-remove) script command, as that removes the item from the original list. ListExclude by contrast returns a copy of the list - the original list is unaffected.

## ListItem
```
ListItem (list, int index)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [string](/types#string) or an [object](/types#object), depending on whether the list is a [stringlist](/types#stringlist) or an [objectlist](/types#objectlist). Gets an item from the list by index. The index is zero-based. (The first item is at index 0, the second is at index 1, etc.)

Usually you will know the type of list that you're passing in, so you should use the [StringListItem](#stringlistitem) or [ObjectListItem](#objectlistitem) functions instead.

## NewList
```
NewList ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an empty [list](/types#list). The list can contain any type of data, or a mixture - for example, both objects and strings.

If the list will only contain one type of data (as will usually be the case), you should use [NewStringList](#newstringlist) or [NewObjectList](#newobjectlist) instead.

## NewObjectList
```
NewObjectList ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an empty [objectlist](/types#objectlist).

## NewStringList
```
NewStringList ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an empty [stringlist](/types#stringlist).

## ObjectListCompact
```
ObjectListCompact (objectlist list)
```

Returns an [objectlist](/types#list), based on the given list, but with any repeated entries removed and any entries that are null removed. The canonical use is when combining two lists that might have some entries in common:

```
combinedlist = ObjectListCompact (list1 + list2)
```

## ObjectListItem
```
ObjectListItem (objectlist, int index)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the [object](/types#object) from the list by the specified index. The index is zero-based. (The first item is at index 0, the second is at index 1, etc.)

You can use the [ListItem](#listitem) function if you don't know the type of the list.

### Example
For example, to show a specific objects' name from a list, first create an [objectlist](/types#objectlist) called myList, in this example it is a list of objects that can be seen currently [ScopeVisibleNotHeld](/functions/scope#scopevisiblenotheld).

     myList = ScopeVisibleNotHeld()

Now show the name of the second item in the list. Note that the second object is at index 1.

     msg ("myList item 2 is " + ObjectListItem(myList, 1).name)

This could be used with [GetRandomInt](/functions/random#getrandomint) to remove an item from the players inventory ([ScopeInventory](/functions/scope#scopeinventory)) and place it into the current room (for example, if you're creating a poltergeist or thief).

## ObjectListSort
```
ObjectListSort (objectlist list, string attributes ...)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [objectlist](/types#objectlist) - a copy of the input objectlist, sorted in order of the values of the first specified attribute (and then optionally by any subsequently specified attributes).

For example, to return a list of objects sorted by name:

     sortedlist = ObjectListSort(list, "name")

To return a list sorted by weight, with equivalent weight objects sorted by name:

     sortedlist = ObjectListSort(list, "weight", "name")

It is important to have all the objects in the list have the same type of attribute. If you are sorting by weight, and some objects have an integer attribute for weight and some have a double attribute, you will get an error:

> Error evaluating expression 'ObjectListSort(l, "weight")': Object must be of type Double.

On the other hand, if an object is missing the attribute, it will appear first in the list, which might not be what you are expecting!

If you are sorting using a string attribute, the list will be sorted alphabetically. The ordering is that nothing is first, then spaces, then punctuation and underscores, followed by numbers and then letters. Letters are sorted by what the letter is first, so "a" will be first, whatever the case or accents on it, but for a specific letter, lower case, then upper, then accented.

So if we have a set of objects with string attributes, they would be sorted in this order:

> null, "T", "T ", "T!", "T.", "T_", "T2", "Ta", "TA", "Tá", "Tb"

When sorting Booleans, false comes before true.

If you try to sort by object, script, list or dictionary attribute you will get an error:

> Error evaluating expression 'ObjectListSort(l, "weight")': At least one object must implement IComparable.
     
To return the values in reverse order, use [ObjectListSortDescending](#objectlistsortdescending).

## ObjectListSortDescending
```
ObjectListSortDescending (objectlist list, string attributes ...)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the reversed version of [ObjectListSort](#objectlistsort) - see that page for usage.

## ObjectListToStringList
```
ObjectListToStringList (objectlist list, string attribute name)
```

Returns a new string list containing the values or the names attribute for each object in the given list. The value of the attribute must be a string or it will not be added. If an object does not have that attribute or it is not a string, then it will be missing from the list, so the string list that is returned could well be shorter than the object list.

## RemoveSceneryObjects
```
RemoveSceneryObjects(objectlist)
```

Returns a list where all scenery objects are removed from the list **objectlist**

## StringListItem
```
StringListItem (stringlist, int index)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the [string](/types#string) from the list by the specified index. The index is zero-based. (The first item is at index 0, the second is at index 1, etc.)

You can use the [ListItem](#listitem) function if you don't know the type of the list.

## StringListSort
```
StringListSort (stringlist list)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [stringlist](/types#stringlist) - a copy of the input stringlist, sorted alphabetically.

To return the values in reverse order, use [StringListSortDescending](#stringlistsortdescending).

## StringListSortDescending
```
StringListSortDescending (stringlist list)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [stringlist](/types#stringlist) - a copy of the input stringlist, reverse sorted alphabetically.

To return the values in ascending order, use [StringListSort](#stringlistsort).

