---
title: "Functions for attributes"
sidebar:
  order: 1
---

Functions for checking and setting attributes on objects.

## DecreaseHealth
```quest
DecreaseHealth (integer amount)
```

Decreases the current player's health attribute by the specified amount.

## DecreaseMoney
```quest
DecreaseMoney (integer amount)
```

Decreases the player's money attribute by the specified amount.

## DecreaseObjectCounter
```quest
DecreaseObjectCounter (object, string counter name)
```

Decreases the named integer attribute on the object by 1. If the object doesn't already have that attribute, it is treated as starting at 0 (so the result is -1).

See also [IncreaseObjectCounter](#increaseobjectcounter).

## DecreaseScore
```quest
DecreaseScore (integer amount)
```

Decreases the game's score attribute by the specified amount.

## GetAttribute
```quest
GetAttribute (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the value of the specified object attribute. The return type will be the attribute type. Returns [null](/types#null) if the attribute does not exist.

## GetAttributeNames
```quest
GetAttributeNames (object, boolean include inherited attributes)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [stringlist](/types#stringlist) of all attribute names of the specified object, optionally including attributes set by an included type.

## GetBoolean
```quest
GetBoolean (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the object has that boolean attribute set to true, or **false** if the attribute is set to false, null or some other non-boolean value.

See also [HasBoolean](#hasboolean)

## GetDouble
```quest
GetDouble (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [double](/types#double) if the object has that double attribute, or **null** if the attribute is set to null or some other non-double value.

See also [HasDouble](#hasdouble) and [ToDouble](/functions/variables#todouble)

## GetInt
```quest
GetInt (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [int](/types#int) if the object has that integer attribute, or **null** if the attribute is set to null or some other non-integer value.

See also [HasInt](#hasint), [IsInt](/functions/variables#isint) and [ToInt](/functions/variables#toint)

## GetString
```quest
GetString (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the [string](/types#string) value of the specified object attribute. Returns [null](/types#null) if the attribute does not exist, or is not a string.

See also [ToString](/functions/variables#tostring) and [HasString](#hasstring)

## HasAttribute
```quest
HasAttribute (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns true if the object has the specified attribute.

## HasBoolean
```quest
HasBoolean (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the object *has* a boolean attribute of the specified name.

Whether the value of that attribute is true or false is unimportant - this function always returns **true** if a value has been defined.

See also [GetBoolean](#getboolean)

## HasDelegateImplementation
```quest
HasDelegateImplementation (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the specified attribute is a delegate implementation.

See [Using delegates](/advanced-topics/using_delegates)

## HasDouble
```quest
HasDouble (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the object *has* a double attribute of the specified name.

See also [GetDouble](#getdouble) and [ToDouble](/functions/variables#todouble)

## HasInt
```quest
HasInt (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the object *has* an integer attribute of the specified name.

See also [GetInt](#getint), [IsInt](/functions/variables#isint) and [ToInt](/functions/variables#toint)

## HasObject
```quest
HasObject (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the specified attribute is a reference to another object.

See also [GetObject](/functions/objects#getobject)

## HasScript
```quest
HasScript (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the specified attribute is a script.

## HasString
```quest
HasString (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the specified attribute is a string.

See also [ToString](/functions/variables#tostring) and [GetString](#getstring)

## IncreaseHealth
```quest
IncreaseHealth (integer amount)
```

Increases the current player's health attribute by the specified amount.

## IncreaseMoney
```quest
IncreaseMoney (integer amount)
```

Increases the player's money attribute by the specified amount.

## IncreaseObjectCounter
```quest
IncreaseObjectCounter (object, string counter name)
```

Increases the named integer attribute on the object by 1. If the object doesn't already have that attribute, it is treated as starting at 0 (so the result is 1).

See also [DecreaseObjectCounter](#decreaseobjectcounter).

## IncreaseScore
```quest
IncreaseScore (integer amount)
```

Increases the game's score attribute by the specified amount.

## SetObjectFlagOff
```quest
SetObjectFlagOff (object, string flag name)
```

Turns the object flag off - an object flag is simply a [boolean](/types#boolean) attribute, so:

```quest
 SetObjectFlagOff(myobject, "myflag")
```

is equivalent to

```quest
 myobject.myflag = false
```

See also [SetObjectFlagOn](#setobjectflagon)

## SetObjectFlagOn
```quest
SetObjectFlagOn (object, string flag name)
```

Turns the object flag on - an object flag is simply a [boolean](/types#boolean) attribute, so:

```quest
 SetObjectFlagOn(myobject, "myflag")
```

is equivalent to

```quest
 myobject.myflag = true
```

See also [SetObjectFlagOff](#setobjectflagoff)
