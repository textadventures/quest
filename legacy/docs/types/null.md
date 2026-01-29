---
layout: index
title: Null
---

If Booleans seem limited in have only two possible values, null can have only one!

In fact, null is a special value for attributes that says the attribute does not exist (which is different to local variables, which can be assigned a value of `null`, but do still exist). Setting an attribute to null is the same as deleting it (when the game is saved, null attributes are not written).

You can check if an attribute is null using the "null" keyword:

     if (someobject.parent = null) { ... }

There is a "gotcha" lurking here. If your object is of a type that sets an attribute to some value, and your object sets it to another value, what happens when you set that attribute on the object to null? The attribute is removed from the object, and so reverts to being the value from the type. This may not be what you expect!