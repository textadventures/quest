---
layout: index
title: description
---

"description" is an object attribute which can be either a [string](../types/string.html) or a [script](../types/script.html). It is used to describe the room when the player enters.

-   If it is a string, an automatic room description is generated which lists the objects in the room and available exits (if [autodescription](autodescription.html) is turned on). Then the string is printed.
-   If it is a script, no automatic room description is generated. The script is run instead.

This attribute is only usually used for objects which are rooms. But if you have [appendobjectdescription](appendobjectdescription.html) turned on, it is also used for objects within rooms. In this case, it is a [string](../types/string.html) which is appended to the parent room description when the player enters.
