---
layout: index
title: GetNonTransparentParent
---

    GetNonTransparentParent (object)

Returns the first [object](../../../types/object.html) in the parent hierarchy that is non-transparent. If the object specified in the parameter is the player, then it is the limit of what the player can see out of - usually the object that represents the current room.

So if the player gets onto a platform within a room, or is inside a transparent box within the room, you can still find out the overall parent room by calling this function.
