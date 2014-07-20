---
layout: index
title: Contains
---

    Contains (object parent, object child)

Returns a [boolean](../types/boolean.html) - **true** if the child object is contained by the parent. This doesn't necessarily mean that there is a direct parent-child relationship - for example if object A has parent B, and B has parent C, then

     Contains(C, A)

will return **true**.
