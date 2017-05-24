---
layout: index
title: ObjectListToStringList
---

    ObjectListToStringList (objectlist list, string attribute name)

**New in Quest 5.7**    

Returns a new string list containing the values or the names attribute for each object in the given list. The value of the attribute must be a string or it will not be added. If an object does not have that attribute or it is not a string, then it will be missing from the list, so the string list that is returned could well be shorter than the object list.