---
title: "Functions for Objects and Exits"
sidebar:
  order: 3
---

## Clone

Clone (object object to clone)

Clones the object and returns the created clone. It is generally better to use [CloneObject](#cloneobject), which will automatically give the clone an alias so it will appear identical to the player (as names must be unique). 

See also [CloneObjectAndMove](#cloneobjectandmove).

NOTE: This a [hard-coded function](/functions/hardcoded).

## CloneObject

CloneObject (object)

Returns an [object](/types#object). Helper function for cloning objects. Clones the object using the [clone](#clone) function. If the existing object did not have an alias, the new object gets an alias of the old object's name - this means that this function returns an object that looks the same to the player as the original object.

Objects inside the target object are also cloned, so if you clone a basket with a sandwich inside it, the clone of the basket will have a clone of the sandwich inside it (and if there is ham in the sandwich, that will be cloned, and so on).

As of 5.7.2, also sets the "prototype" attribute of the clone to point to the original (unless the attribute is already set). This allows you to quickly find all copies of a specific original, or to determine whether an object is the original or a copy. Note that if the make a clone of a clone, the "prototype" attribute will point to the original still.

See also [CloneObjectAndMove](#cloneobjectandmove) and [CloneObjectAndMoveHere](#cloneobjectandmovehere)

## CloneObjectAndMove

CloneObjectAndMove (object, object new parent)

Returns an [object](/types#object). Helper function for cloning objects. Clones the object using the [CloneObject](#cloneobject) function and also moves it to the specified parent. For more details, see that function.

## CloneObjectAndMoveHere

CloneObjectAndMoveHere (object)

Returns an [object](/types#object). Helper function for cloning objects. Clones the object using the [CloneObject](#cloneobject) function and also moves it to current room.  For more details, see that function.

## CreateBiExits

CreateBiExits (string direction, object from, object to)

Creates a new exit in the given direction, from and to the given rooms. Also creates a second exit in the opposite direction.

## DoesInherit

DoesInherit (object object, string type name)

Returns a [boolean](/types#boolean) indicating whether the object inherits the specified type.

    if (DoesInherit(o, "male")) {
      msg("'Hi,' he grunts.")
    }
    else if (DoesInherit(o, "female")) {
      msg("'Hi,' she smiles.")
    }
    else {
      msg("It says nothing.")
    }

Note that the types "editor_player", "editor_room" and "editor_object" are removed when you publish your game, so it is a bad idea to test for them.

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetExitByLink

GetExitByLink (object from room, object to room)

Returns a [string](/types#string) containing the name of the exit joining the specified rooms, if it exits. If it does not exist, [null](/types#null) is returned instead.

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetExitByName

GetExitByName (object from room, string direction name)

Returns a [string](/types#string) containing the name of the exit going from the specified room in the specified direction, if it exists. If it does not exist, [null](/types#null) is returned instead.

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetObject

GetObject (string object name)

Returns the [object](/types#object) of the specified name (or exit, or command, or turn script; but not timer). Returns null if the object doesn't exist.

See also [HasObject](/functions/fn-attributes#hasobject)

NOTE: This a [hard-coded function](/functions/hardcoded).

## LockExit

LockExit (exit)

Locks the specified exit, simply by setting the [locked](/attributes#locked) property to true.

## MakeExitInvisible

MakeExitInvisible (object)

Makes exit invisible.

see also [MakeExitVisible](#makeexitvisible)

## MakeExitVisible

MakeExitVisible (object)

Makes exit visible.

see also [MakeExitInvisible](#makeexitinvisible)

## MakeObjectInvisible

MakeObjectInvisible (object)

Makes object invisible.

see also [MakeObjectVisible](#makeobjectvisible)

## MakeObjectVisible

MakeObjectVisible (object)

Makes object visible.

see also [MakeObjectInvisible](#makeobjectinvisible)

## MoveObject

MoveObject(object object1, object object2)

Moves object1 to object2.

Example to move the player into a room named lounge:

    MoveObject (player, lounge)

This is just the same as setting the parent attribute:
    
    player.parent = lounge

## RemoveObject

RemoveObject(objectobject1)

Removes an object from its parent.

## UnlockExit

UnlockExit (exit)

Unlocks the specified exit, simply by setting the [locked](/attributes#locked) property to false.
