---
layout: index
title: GetAllChildObjects
---

    GetAllChildObjects (object)

Returns an [objectlist](../types/objectlist.html) containing all objects directly or indirectly contained by the parent object (i.e. including all children of children etc.)

For example, if object A contains object B, and object B contains object C, then GetAllChildObjects(A) returns a list containing B and C.

Use [GetDirectChildren](getdirectchildren.html) instead if you only want children directly contained (in the above example, only B).

NOTE: This function is hard-coded and cannot be overridden.
