---
title: "Functions for Attributes"
sidebar:
  order: 1
---

Functions for checking and setting attributes on objects.

## DecreaseHealth

DecreaseHealth (integer amount)

Decreases the current player's health attribute by the specified amount.

## DecreaseMoney

DecreaseMoney (integer amount)

Decreases the player's money attribute by the specified amount.

## DecreaseScore

DecreaseScore (integer amount)

Decreases the game's score attribute by the specified amount.

## GetAttribute

GetAttribute (object, string attribute name)

Returns the value of the specified object attribute. The return type will be the attribute type. Returns [null](/types#null) if the attribute does not exist.

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetAttributeNames

GetAttributeNames (object, boolean include inherited attributes)

Returns a [stringlist](/types#stringlist) of all attribute names of the specified object, optionally including attributes set by an included type.

This function was added in Quest 5.3.

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetBoolean

GetBoolean (object, string attribute name)

Returns a [boolean](/types#boolean) - **true** if the object has that boolean attribute set to true, or **false** if the attribute is set to false, null or some other non-boolean value.

See also [HasBoolean](#hasboolean)

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetDouble

GetDouble (object, string attribute name)

Returns an [double](/types#double) if the object has that double attribute, or **null** if the attribute is set to null or some other non-double value.

See also [HasDouble](#hasdouble) and [ToDouble](/functions/fn-variables#todouble)

This function was added in Quest 5.3.

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetInt

GetInt (object, string attribute name)

Returns an [int](/types#int) if the object has that integer attribute, or **null** if the attribute is set to null or some other non-integer value.

See also [HasInt](#hasint), [IsInt](/functions/fn-variables#isint) and [ToInt](/functions/fn-variables#toint)

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetString

GetString (object, string attribute name)

Returns the [string](/types#string) value of the specified object attribute. Returns [null](/types#null) if the attribute does not exist, or is not a string.

See also [ToString](/functions/fn-variables#tostring) and [HasString](#hasstring)

NOTE: This a [hard-coded function](/functions/hardcoded).

## HasAttribute

HasAttribute (object, string attribute name)

Returns true if the object has the specified attribute.

NOTE: This a [hard-coded function](/functions/hardcoded).

## HasBoolean

HasBoolean (object, string attribute name)

Returns a [boolean](/types#boolean) - **true** if the object *has* a boolean attribute of the specified name.

Whether the value of that attribute is true or false is unimportant - this function always returns **true** if a value has been defined.

See also [GetBoolean](#getboolean)

NOTE: This a [hard-coded function](/functions/hardcoded).

## HasDelegateImplementation

HasDelegateImplementation (object, string attribute name)

Returns a [boolean](/types#boolean) - **true** if the specified attribute is a delegate implementation.

See [Using Delegates](/types/using_delegates)

NOTE: This a [hard-coded function](/functions/hardcoded).

## HasDouble

HasDouble (object, string attribute name)

Returns a [boolean](/types#boolean) - **true** if the object *has* a double attribute of the specified name.

See also [GetDouble](#getdouble) and [ToDouble](/functions/fn-variables#todouble)

This function was added in Quest 5.3.

NOTE: This a [hard-coded function](/functions/hardcoded).

## HasInt

HasInt (object, string attribute name)

Returns a [boolean](/types#boolean) - **true** if the object *has* an integer attribute of the specified name.

See also [GetInt](#getint), [IsInt](/functions/fn-variables#isint) and [ToInt](/functions/fn-variables#toint)

NOTE: This a [hard-coded function](/functions/hardcoded).

## HasObject

HasObject (object, string attribute name)

Returns a [boolean](/types#boolean) - **true** if the specified attribute is a reference to another object.

See also [GetObject](/functions/fn-objects#getobject)

NOTE: This a [hard-coded function](/functions/hardcoded).

## HasScript

HasScript (object, string attribute name)

Returns a [boolean](/types#boolean) - **true** if the specified attribute is a script.

NOTE: This a [hard-coded function](/functions/hardcoded).

## HasString

HasString (object, string attribute name)

Returns a [boolean](/types#boolean) - **true** if the specified attribute is a string.

See also [ToString](/functions/fn-variables#tostring) and [GetString](#getstring)

NOTE: This a [hard-coded function](/functions/hardcoded).

## IncreaseHealth

IncreaseHealth (integer amount)

Increases the current player's health attribute by the specified amount.

## IncreaseMoney

IncreaseMoney (integer amount)

Increases the player's money attribute by the specified amount.

## IncreaseScore

IncreaseScore (integer amount)

Increases the game's score attribute by the specified amount.

## SetObjectFlagOff

SetObjectFlagOff (object, string flag name)

Turns the object flag off - an object flag is simply a [boolean](/types#boolean) attribute, so:

     SetObjectFlagOff(myobject, "myflag")

is equivalent to

     myobject.myflag = false

See also [SetObjectFlagOn](#setobjectflagon)

## SetObjectFlagOn

SetObjectFlagOff (object, string flag name)

Turns the object flag on - an object flag is simply a [boolean](/types#boolean) attribute, so:

     SetObjectFlagOn(myobject, "myflag")

is equivalent to

     myobject.myflag = true

See also [SetObjectFlagOff](#setobjectflagoff)
