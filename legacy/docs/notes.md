---
layout: index
title: Notes
---

Mutable attributes on inherited types
-------------------------------------

When you inherit an attribute from a type, the type's attributes are not copied, just pointed to. For most attribute types there's no problem with this, but lists and dictionaries are mutable - i.e. they can be changed by commands such as [list add](scripts/list_add.html), but you're still pointing to the same list.

If type "MyType" has a list attribute "TypeList", and object "MyObject" inherits from MyType, then we potentially have a problem if we call "list add (MyObject.TypeList, value)", as we would then change the list on the underlying type - affecting all other objects that inherit from it.

To prevent this, mutable attributes which are defined on types are *locked*. If you try to call the "list add" command in the example above, an error will be raised.

You can get around the problem by cloning the list or dictionary first. Quest automatically clones on assignment. This means you can write objectA.list = objectB.list, and objectA actually gets a *clone* of objectB's list, so you can change objectA's list without affecting objectB.

The same principle works for cloning an attribute defined on an underlying type - in our example above, we can clone the TypeList attribute first using this:

    MyObject.TypeList = MyObject.TypeList

Yep, it looks like assigning to the same thing. But we've made use of the fact that assignment clones mutable types to give MyObject an editable version of TypeList.
