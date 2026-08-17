---
title: PickOneChildOfType
---

    PickOneChildOfType (object room or container, string typename)

Returns an [object](/types/object), picked at random from the direct children of the given object, and is also of the given type (so if the given object is a room, this would be any object in the room, but not including objects inside containers). Returns null if there are none.