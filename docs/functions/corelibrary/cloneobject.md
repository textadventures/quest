---
layout: index
title: CloneObject
---

    CloneObject (object)

Returns an [object](../../types/object.html). Helper function for cloning objects. Clones the object using the [clone](../../functions/clone.html) function. If the existing object did not have an alias, the new object gets an alias of the old object's name - this means that this function returns an object that looks the same to the player as the original object.

Objects inside the target object are also cloned, so if you clone a basket with a sandwich inside it, the clone of the basket will have a clone of the sandwich inside it (and if there is ham in the sandwich, that will be cloned, and so on).

As of 5.7.2, also sets the "prototype" attribute of the clone to point to the original (unless the attribute is already set). This allows you to quickly find all copies of a specific original, or to determine whether an object is the original or a copy. Note that if the make a clone of a clone, the "prototype" attribute will point to the original still.

See also [CloneObjectAndMove](cloneobjectandmove.html) and [CloneObjectAndMoveHere](cloneobjectandmovehere.html)