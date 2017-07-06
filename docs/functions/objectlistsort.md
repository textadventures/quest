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

To return the values in reverse order, use [ObjectListSortDescending](objectlistsortdescending.html).

This function was added in Quest 5.3.

NOTE: This a [hard-coded function](hardcoded.html).
