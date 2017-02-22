---
layout: index
title: PickOneChildOfType
---

    PickOneChildOfType (object room or container, string typename)

**New in Quest 5.7**    

Returns an [object](../types/object.html), picked at random from the direct children of the given object, and is also of the given type (so if the given object is a room, this would be any object in the room, but not including objects inside containers). Returns null if there are none.