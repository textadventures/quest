---
layout: index
title: ObjectListSort
---

    ObjectListSort (objectlist list, string attributes ...)

Returns an [objectlist](../types/objectlist.html) - a copy of the input objectlist, sorted in order of the values of the first specified attribute (and then optionally by any subsequently specified attributes).

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
     
To return the values in reverse order, use [ObjectListSortDescending](objectlistsortdescending.html).

This function was added in Quest 5.3.

NOTE: This a [hard-coded function](hardcoded.html).
