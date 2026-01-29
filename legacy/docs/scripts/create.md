---
layout: index
title: create
---

    create (string name)

or (as of Quest 5.3)

    create (string name, string type)

Creates an object with the specified name. You can subsequently access the object using the [GetObject](../functions/getobject.html) function, or just use its name directly in an expression.

If you specify a type, the object created will be of that type. The command only accepts one type name - if you want the new object to inherit multiple types, you could create one type which inherits all of those types, and specify that here.
