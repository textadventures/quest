---
layout: index
title: Much Ado About Nothing
---



The `null` object
-----------------

In Quest code, null has two slightly different meanings. It can be an empty object; that is, an object with no attributes and of the special type "null".

obj = player
msg(TypeOf(obj))
-> "object"
obj = null
msg(TypeOf(obj))
-> "null"

The variable obj is set to point to the built-in object, null, which is of the type "null".

Some functions will check if a parameter is null, some do not. Look at the `HasString` function. It takes an object and a string (the attribute name). It checks if the object is null, and gives a helpful error message if that is not the case.

obj = player
msg(HasString(obj, "name"))
-> True
obj = null
msg(HasString(obj, "name"))
-> Error running script: Error evaluating expression 'HasString(obj, "name")': HasString function expected object parameter but was passed 'null'
msg(HasString(player, obj))
-> Error running script: Error compiling expression 'HasString(player, obj)': FunctionCallElement: Could find not function 'HasString(Element, Object)'


Attributes set to `null`
------------------------

However, null also means nothing. If you set an attribute to `null`, then the attribute no longer exists.

player.equipped = null
msg(HasAttribute(player, "equipped"))
-> False

Setting an attribute to null is a useful way to remove a script. Perhaps an object does something weird when picked up. You could give it a script that fires. However, later in the game, the curse is removed, perhaps, and the script is set to null; now the default script runs.

Note that if the object is of a certain type, and the type has a value for the attribute, then setting the attribute to `null` will reset it to the value in the type. Suppose `bob` is of the type "male"...

msg(bob.gender)
-> "he"
bob.gender = "she"
msg(bob.gender)
-> "she"
bob.gender = null
msg(bob.gender)
-> "he"

In fact, behind the scenes, what happens is that when we try to access `bob.gender`, Quest first checks if Bob has that attribute, and if not, it then looks for it in the types associated with Bob.


Compared to null
----------------

You can test if most things are null, but not integers.

obj.att = "somestring"
msg (obj.att = null)
-> False
obj.att = player
msg (obj.att = null)
-> False
obj.att = 42
msg (obj.att = null)
-> Error running script: Error compiling expression 'obj.att = null': CompareElement: Operation 'Equal' is not defined for types 'Int32' and 'Null'

If you are not sure what it may be, the safe way is to use the `Equal` function:

obj.att = 42
msg (Equal(obj.att, null))
-> False

Or compare the type first):

obj.att = 42
msg (TypeOf(obj.att) = "null")
-> False
