---
title: "Core.aslx functions"
sidebar:
  order: 13
---

Functions with very specific effects in the game world.

## AddDescriptionLine
```quest
AddDescriptionLine (string description so far, string line)
```

Returns a [string](/types#string). Building-block used to assemble a multi-line description: if **line** is non-empty, any text already accumulated in **description so far** is printed immediately (via [msg](/scripts#msg)) and the description is reset to just **line**; if **line** is empty, **description so far** is returned unchanged. This lets you print each non-blank line as its own paragraph while collapsing blank lines, by repeatedly calling it and printing whatever's left over at the end.

## AddToInventory
```quest
AddToInventory (object)
```

Moves the object to the inventory. This simply sets the object's parent to the current player, so:

```quest
AddToInventory(myobject)
```

is equivalent to

```quest
myobject.parent = game.pov
```

## CanReachThrough
```quest
CanReachThrough (object)
```

Returns a [boolean](/types#boolean) - **true** if the player can reach through the object.

## CanSeeThrough
```quest
CanSeeThrough (object)
```

Returns a [boolean](/types#boolean) - **true** if the player can see through the object.

## ChangePOV
```quest
ChangePOV (object)
```

Switches the player object.

## CheckDarkness
```quest
CheckDarkness()
```

Returns a [boolean](/types#boolean) - **true** if the player is in an room, which is dark and has no strong lightsources in it.

See also [SetDark](#setdark), [SetLight](#setlight), [SetObjectLightstrength](#setobjectlightstrength), [SetExitLightstrength](#setexitlightstrength)

## CloneObjectAndInitialise
```quest
CloneObjectAndInitialise (object)
```

Returns an [object](/types#object). Clones the object using [CloneObject](/functions/objects#cloneobject) and, if the clone has an `_initialise_` script, runs it. Useful for prototype objects that need to set up their own state (e.g. random stats) each time a new copy is created, rather than only when moved into a room - see also [CloneObjectAndMoveHere](/functions/objects#cloneobjectandmovehere).

## FinishTurn
```quest
FinishTurn ()
```

Ends the current turn: runs turn scripts (if `game.runturnscripts` is set and they haven't been suppressed), then updates status attributes and darkness/hyperlink state for the next command. Quest calls this automatically after each player command; you would only call it yourself if you're driving a "turn" from custom code that bypasses the normal command loop.

## FormatExitList
```quest
FormatExitList (string pre-list, objectlist exits, string pre-final, string post-list)
```

Returns a [string](/types#string) containing a formatted list of exits.

For example, this:

```quest
FormatExitList("You can go", ScopeExits(), "or", ", if you like.")
```

may return output like this:

> You can go east, west or south, if you like.

## FormatObjectList
```quest
FormatObjectList (string pre-list, object parent, string pre-final, string post-list, boolean use inventory verbs)
```

Returns a [string](/types#string) containing a formatted list of objects.

Used by [ShowRoomDescription](#showroomdescription) and the "inventory" command to display lists of visible and carried objects.

FormatObjectList will display children of listed objects within brackets, if the parent object can be seen through.

For example, this:

```quest
FormatObjectList("You can see", player.parent, "and", "in this room.", false)
```

may return output like this:

> You can see a sofa, a lamp, a box (containing a diary and a pen) and a kitten in this room.

All object names will be hyperlinked to show a menu of [displayverbs](/attributes#displayverbs). The final parameter lets you specify whether to use the [inventoryverbs](/attributes#inventoryverbs) instead.

## GetBlockingObject
```quest
GetBlockingObject (object)
```

Returns the [object](/types#object) which is preventing the player from reaching the specified object.

If an object is in [ScopeVisible](/functions/scope#scopevisible) but not in [ScopeReachable](/functions/scope#scopereachable), then it may be inside a container where the player can see it but not reach it. You can call the GetBlockingObject function to find out what is "blocking" the player from reaching the object. It will be the top-most parent which the player cannot reach through.

## GetClone
```quest
GetClone (object prototype, object parent)
```

Returns the [object](/types#object) among **parent**'s direct children whose `prototype` attribute points to **prototype**, or [null](/types#null) if there isn't one. **parent** is optional and defaults to the current player. Useful for finding a previously-made clone (see [CloneObject](/functions/objects#cloneobject)) instead of creating a new one.

## GetDefiniteName
```quest
GetDefiniteName (object)
```

Returns a [string](/types#string) containing the full displayed name of an object, prefixed with "the" if it is neither a named male or a named female.

## GetDisplayAlias
```quest
GetDisplayAlias (object)
```

Returns a [string](/types#string) containing the displayed version of the object name. This will be the [alias](/attributes#alias), if the object has one, otherwise it will just be the object name.

## GetDisplayName
```quest
GetDisplayName (object)
```

Returns a [string](/types#string) containing the full displayed name of an object.

This will be the [prefix](/attributes#prefix) + the result from [GetDisplayAlias](#getdisplayalias) + the [suffix](/attributes#suffix).

## GetDisplayNameLink

For Quest 5.3 and earlier:

```quest
GetDisplayNameLink (object, string type, stringlist verbs)
```

For Quest 5.4 and later, there is no verbs parameter:

```quest
GetDisplayNameLink (object, string type)
```

Returns a [string](/types#string) containing the full displayed name of an object.

This will be the [prefix](/attributes#prefix) + the result from [GetDisplayAlias](#getdisplayalias) + the [suffix](/attributes#suffix).

If type is not an empty string (and, in Quest 5.3 and earlier, a verbs list is specified), the result will include the display alias wrapped in an \<object\> tag complete with verbs. This will mean the Quest interface will display a hyperlinked object name with a menu of verbs. In Quest 5.4 and later, the [displayverbs](/attributes#displayverbs) or [inventoryverbs](/attributes#inventoryverbs) are picked up automatically depending on the object's parent.

## GetDisplayVerbs
```quest
GetDisplayVerbs (object)
```

Returns a [stringlist](/types#stringlist) with the current display verbs for the object. If the object is in the current player's inventory, the [inventoryverbs](/attributes#inventoryverbs) are used as a base, otherwise the [displayverbs](/attributes#displayverbs) are used. If [autodisplayverbs](/attributes#autodisplayverbs) is turned on, any verbs set up for the object will be added to the list returned.

## GetListDisplayAlias
```quest
GetListDisplayAlias (object)
```

Returns a [string](/types#string) containing the displayed version of the object name to use in the object panes. This will be the listalias, if the object has one, otherwise it will be the result of [GetDisplayAlias](#getdisplayalias).

## GetNonTransparentParent
```quest
GetNonTransparentParent (object)
```

Returns the first [object](/types#object) in the parent hierarchy that is non-transparent. If the object specified in the parameter is the player, then it is the limit of what the player can see out of - usually the object that represents the current room.

So if the player gets onto a platform within a room, or is inside a transparent box within the room, you can still find out the overall parent room by calling this function.

## GetRoomDescription
```quest
GetRoomDescription ()
```

Returns a [string](/types#string) containing the full formatted description of the current room (the current player's parent) - the same text [ShowRoomDescription](#showroomdescription) would print, including the dark-room fallback from [CheckDarkness](#checkdarkness). Useful if you want the description as text rather than having it printed immediately, e.g. to include it in a menu or a saved log.

## GetVolume
```quest
GetVolume (object, boolean inclusiveobject)
```

Returns the volume of an object with all its children.

If **inclusiveobject** is false, then the volume of all objects within **object** is returned without the volume of **object** itself.

If **inclusiveobject** is true, then the volume of all objects within **object** is returned inclusive the volume of **object** itself.

## Got
```quest
Got (object)
```

Returns a [boolean](/types#boolean) - **true** if the player has the specified object. This is just a convenient shortcut to seeing if it is within the list returned by [ScopeInventory](/functions/scope#scopeinventory).

## GridSquareClick
```quest
GridSquareClick (integer x, integer y)
```

Override hook for [grid maps](/howto/tasks/showing_a_map): the default implementation does nothing. Copy this function into your game and give it a body to handle clicks on a grid map square, using the clicked square's coordinates.

## HelperCloseObject
```quest
HelperCloseObject (object)
```

Closes the object. This simply sets the [isopen](/attributes#isopen) attribute to false.

See also [HelperOpenObject](#helperopenobject)

## HelperOpenObject
```quest
HelperOpenObject (object)
```

Opens the object. This simply sets the [isopen](/attributes#isopen) attribute to true.

See also [HelperCloseObject](#helpercloseobject)

## IsSwitchedOn
```quest
IsSwitchedOn (object)
```

Returns a [boolean](/types#boolean) - true if the object is switched on.

This function simply reads the [switchedon](/attributes#switchedon) attribute of the object.

## ListParents
```quest
ListParents (object)
```

Returns an [objectlist](/types#objectlist) of all parents of an object - the object's direct parent, the parent's parent, and so on.

## MoveObjectHere
```quest
MoveObjectHere (object)
```

Moves the object to the current player's room. Equivalent to `object.parent = game.pov.parent`. See also [MoveObject](/functions/objects#moveobject) to move an object to an arbitrary parent, and [AddToInventory](#addtoinventory) to move it into the player's inventory instead.

## SetDark
```quest
SetDark(room)
```

Set a **room** to dark. Objects and Exits can't be seen unless there is a lightsource object in this room.

See also [SetLight](#setlight), [SetObjectLightstrength](#setobjectlightstrength),[SetExitLightstrength](#setexitlightstrength), [CheckDarkness](#checkdarkness)

## SetExitLightstrength
```quest
SetExitLightstrength(exit, string value)
```

Define an **exit** as a lightsource. A possible **value** is:

"" (empty string) : exit is not a lightsource and is not visible in a dark room

"weak" : exit is visible in a dark room

"strong" : exit enlightens a dark room, so the normal room description is shown

See also [SetDark](#setdark), [SetLight](#setlight), [SetObjectLightstrength](#setobjectlightstrength), [CheckDarkness](#checkdarkness)

## SetLight
```quest
SetLight(room)
```

Set a **room** to light.

See also [SetDark](#setdark), [SetObjectLightstrength](#setobjectlightstrength), [SetExitLightstrength](#setexitlightstrength), [CheckDarkness](#checkdarkness)

## SetObjectLightstrength
```quest
SetObjectLightstrength(object, string value)
```

Define an **object** as a lightsource. A possible **value** is:

"" (empty string) : object is not a lightsource and is not visible in a dark room

"weak" : object is visible in a dark room

"strong" : object enlightens a dark room, so the normal room description is shown

See also [SetDark](#setdark), [SetLight](#setlight), [SetExitLightstrength](#setexitlightstrength), [CheckDarkness](#checkdarkness)

## ShowRoomDescription
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```quest
ShowRoomDescription ()
```

Does not return a value.

## WhereAmI
```quest
WhereAmI (string platform name)
```

Sets the `questplatform` attribute on `game` to the given string. Despite the name, this has nothing to do with the player's location - nothing in the current engine reads `questplatform` back, so this is a legacy hook rather than something new games need to call.

## SwitchOff
```quest
SwitchOff (object)
```

Switches off the specified object, simply by setting the [switchedon](/attributes#switchedon) property to false.

## SwitchOn
```quest
SwitchOn (object)
```

Switches on the specified object, simply by setting the [switchedon](/attributes#switchedon) property to true.

