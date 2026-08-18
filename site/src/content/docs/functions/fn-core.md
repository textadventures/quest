---
title: "Core.aslx Functions"
sidebar:
  order: 13
---

Functions with very specific effects in the game world.

## AddToInventory

AddToInventory (object)

Moves the object to the inventory. This simply sets the object's parent to the current player, so:

     AddToInventory(myobject)

is equivalent to

     myobject.parent = game.pov

Added in Quest 5.2.

## CanReachThrough

CanReachThrough (object)

Returns a [boolean](/types#boolean) - **true** if the player can reach through the object.

## CanSeeThrough

CanSeeThrough (object)

Returns a [boolean](/types#boolean) - **true** if the player can see through the object.

## ChangePOV

ChangePOV (object)

Switches the player object.

## CheckDarkness

CheckDarkness()

Returns a [boolean](/types#boolean) - **true** if the player is in an room, which is dark and has no strong lightsources in it.

See also [SetDark](#setdark), [SetLight](#setlight), [SetObjectLightstrength](#setobjectlightstrength), [SetExitLightstrength](#setexitlightstrength)

This command was added in Quest 5.3.

## FormatExitList

FormatExitList (string pre-list, objectlist exits, string pre-final, string post-list)

Returns a [string](/types#string) containing a formatted list of exits.

For example, this:

    FormatExitList("You can go", ScopeExits(), "or", ", if you like.")

may return output like this:

> You can go east, west or south, if you like.

## FormatObjectList

FormatObjectList (string pre-list, object parent, string pre-final, string post-list, boolean use inventory verbs)

Returns a [string](/types#string) containing a formatted list of objects.

Used by [ShowRoomDescription](#showroomdescription) and the "inventory" command to display lists of visible and carried objects.

FormatObjectList will display children of listed objects within brackets, if the parent object can be seen through.

For example, this:

    FormatObjectList("You can see", player.parent, "and", "in this room.", false)

may return output like this:

> You can see a sofa, a lamp, a box (containing a diary and a pen) and a kitten in this room.

All object names will be hyperlinked to show a menu of [displayverbs](/attributes#displayverbs). The final parameter lets you specify whether to use the [inventoryverbs](/attributes#inventoryverbs) instead.

## GetBlockingObject

GetBlockingObject (object)

Returns the [object](/types#object) which is preventing the player from reaching the specified object.

If an object is in [ScopeVisible](/functions/fn-scope#scopevisible) but not in [ScopeReachable](/functions/fn-scope#scopereachable), then it may be inside a container where the player can see it but not reach it. You can call the GetBlockingObject function to find out what is "blocking" the player from reaching the object. It will be the top-most parent which the player cannot reach through.

## GetDefiniteName

GetDefiniteName (object)

Returns a [string](/types#string) containing the full displayed name of an object, prefixed with "the" if it is neither a named male or a named female.

## GetDisplayAlias

GetDisplayAlias (object)

Returns a [string](/types#string) containing the displayed version of the object name. This will be the [alias](/attributes#alias), if the object has one, otherwise it will just be the object name.

## GetDisplayName

GetDisplayName (object)

Returns a [string](/types#string) containing the full displayed name of an object.

This will be the [prefix](/attributes#prefix) + the result from [GetDisplayAlias](#getdisplayalias) + the [suffix](/attributes#suffix).

## GetDisplayNameLink

For Quest 5.3 and earlier:

    GetDisplayNameLink (object, string type, stringlist verbs)

For Quest 5.4 and later, there is no verbs parameter:

    GetDisplayNameLink (object, string type)

Returns a [string](/types#string) containing the full displayed name of an object.

This will be the [prefix](/attributes#prefix) + the result from [GetDisplayAlias](#getdisplayalias) + the [suffix](/attributes#suffix).

If type is not an empty string (and, in Quest 5.3 and earlier, a verbs list is specified), the result will include the display alias wrapped in an \<object\> tag complete with verbs. This will mean the Quest interface will display a hyperlinked object name with a menu of verbs. In Quest 5.4 and later, the [displayverbs](/attributes#displayverbs) or [inventoryverbs](/attributes#inventoryverbs) are picked up automatically depending on the object's parent.

## GetDisplayVerbs

GetDisplayVerbs (object)

Returns a [stringlist](/types#stringlist) with the current display verbs for the object. If the object is in the current player's inventory, the [inventoryverbs](/attributes#inventoryverbs) are used as a base, otherwise the [displayverbs](/attributes#displayverbs) are used. If [autodisplayverbs](/attributes#autodisplayverbs) is turned on, any verbs set up for the object will be added to the list returned.

## GetListDisplayAlias

GetListDisplayAlias (object)

Returns a [string](/types#string) containing the displayed version of the object name to use in the object panes. This will be the listalias, if the object has one, otherwise it will be the result of [GetDisplayAlias](#getdisplayalias).

## GetNonTransparentParent

GetNonTransparentParent (object)

Returns the first [object](/types#object) in the parent hierarchy that is non-transparent. If the object specified in the parameter is the player, then it is the limit of what the player can see out of - usually the object that represents the current room.

So if the player gets onto a platform within a room, or is inside a transparent box within the room, you can still find out the overall parent room by calling this function.

## GetVolume

GetVolume (object, boolean inclusiveobject)

Returns the volume of an object with all its children.

If **inclusiveobject** is false, then the volume of all objects within **object** is returned without the volume of **object** itself.

If **inclusiveobject** is true, then the volume of all objects within **object** is returned inclusive the volume of **object** itself.

## Got

Got (object)

Returns a [boolean](/types#boolean) - **true** if the player has the specified object. This is just a convenient shortcut to seeing if it is within the list returned by [ScopeInventory](/functions/fn-scope#scopeinventory).

## HelperCloseObject

HelperCloseObject (object)

Closes the object. This simply sets the [isopen](/attributes#isopen) attribute to false.

See also [HelperOpenObject](#helperopenobject)

## HelperOpenObject

HelperOpenObject (object)

Opens the object. This simply sets the [isopen](/attributes#isopen) attribute to true.

See also [HelperCloseObject](#helpercloseobject)

## IsSwitchedOn

IsSwitchedOn (object)

Returns a [boolean](/types#boolean) - true if the object is switched on.

This function simply reads the [switchedon](/attributes#switchedon) attribute of the object.

## ListParents

ListParents (object)

Returns an [objectlist](/types#objectlist) of all parents of an object - the object's direct parent, the parent's parent, and so on.

## SetDark

SetDark(room)

Set a **room** to dark. Objects and Exits can't be seen unless there is a lightsource object in this room.

See also [SetLight](#setlight), [SetObjectLightstrength](#setobjectlightstrength),[SetExitLightstrength](#setexitlightstrength), [CheckDarkness](#checkdarkness)

This command was added in Quest 5.3.

## SetExitLightstrength

SetExitLightstrength(exit, string value)

Define an **exit** as a lightsource. A possible **value** is:

"" (empty string) : exit is not a lightsource and is not visible in a dark room

"weak" : exit is visible in a dark room

"strong" : exit enlightens a dark room, so the normal room description is shown

See also [SetDark](#setdark), [SetLight](#setlight), [SetObjectLightstrength](#setobjectlightstrength), [CheckDarkness](#checkdarkness)

This command was added in Quest 5.3.

## SetLight

SetLight(room)

Set a **room** to light.

See also [SetDark](#setdark), [SetObjectLightstrength](#setobjectlightstrength), [SetExitLightstrength](#setexitlightstrength), [CheckDarkness](#checkdarkness)

This command was added in Quest 5.3.

## SetObjectLightstrength

SetObjectLightstrength(object, string value)

Define an **object** as a lightsource. A possible **value** is:

"" (empty string) : object is not a lightsource and is not visible in a dark room

"weak" : object is visible in a dark room

"strong" : object enlightens a dark room, so the normal room description is shown

See also [SetDark](#setdark), [SetLight](#setlight), [SetExitLightstrength](#setexitlightstrength), [CheckDarkness](#checkdarkness)

This command was added in Quest 5.3.

## ShowRoomDescription

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    ShowRoomDescription ()

Does not return a value.

## SwitchOff

SwitchOff (object)

Switches off the specified object, simply by setting the [switchedon](/attributes#switchedon) property to false.

Added in Quest 5.2.

## SwitchOn

SwitchOn (object)

Switches on the specified object, simply by setting the [switchedon](/attributes#switchedon) property to true.

Added in Quest 5.2.
