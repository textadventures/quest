---
layout: index
title: Quest 5.7
---


This file documents changes to Quest as of version 5.7.

New UI Options
--------------

Customisation of the UI is now much easier. There are a number of new JavaScript functions, but more importantly a lot of options can be set from the tabs of the game object, so you should not have to do any JavaScript.

### Game panes

You can now select different colour schemes for the panes on the right (or invent your own with a JavaScript function, JS.setPanes)

You can turn off individual panes, and add a new command pane and a new status pane. The command pane makes it easier to create a game with no command bar, as commands like LOOK and WAIT can be put here (with JS.setCommands). The status pane can have any HTML put in it (with JS.setCustomStatus), so could display indicators bars as well as text.
https://github.com/textadventures/quest/issues/752

### Command bar

New options for the command bar; borderless cursor or shadow box.

### Colours

You can now set the background to blend from one colour at the top to another at the bottom. You can set both the margins and the page background colour, as well as the status bar and game border.

### Other changes

The body element is now given the default font, color and background-color. This means the save confirmation text will be in the standard style for your game. This has the potential to have far-reaching effects, given the body element is the fundamental HTML element of your game, but it seems to have no side effects, and if there are some I would expect them to be improvements anyway.

Note that this means the text in the status bar at the top is now in the default font (in 5.6 it is Arial/Helvetica) (no effect on its colour).

The gamePanes div is now transparent. Again, this could affect many games, but I cannot imagine why anyone would want it a different colour to #gameBorder.

The Features, Display and Interface tabs of the game object have been rearranged and extended to support these features, and to make their placement (hopefully) more logical.

There is now a JS.setCss junction that takes an element name and a set of CSS stylings. This will make changing styles much easier (though the above will mean it should rarely be necessary).

There is an option to hide the save button in the web player (it does not stop players saving via the command bar).






Text Processor
--------------

This has been updated so that it has a few extra commands (incorporating most of the TextProcessorPlus library); "colour" (or "color") will get the text to display in that colour. Similarly, "back" will change the background colour. You can also use "i", "b", "u" or "s" for italic, bold, underline or strike-through.

> Here is some text with {colour:red:this} in red, and {b:{back:blue:that}} in bold with a blue background.

The "popup" command creates a link, clicking on it will display an information box, which can be dismissed by clicking.

> The man is carrying a {popup:spade:A spade is a tool commonly used for digging holes.} over his back.

The "either" command is similar to "if", but has a second (optional) part that is printed if the condition fails, and is more flexible with its conditions, as these now evaluated as Quest code (this does mean that strings need to go in quotes which is different to how this was in TextProcessorPlus library). 

There is also an "eval" command which will run the rest of the text as Quest code. You can also use an equals sign as a short cut. This means you can put anything into curly braces, such as function calls and complex calculations. If you actually want curly braces as curly braces and there is a chance Quest will try to interpret it as a text processor command, you can use `@@@open@@@` and `@@@close@@@`.

It also works recursively, so the output text will then get processed again. We have yet to work out when that will be useful...

A new function, `ProcessText` is used by the output functions. You could use this elsewhere, say to set an attribute with text that has gone through the text processor. You could also override it to do your own stuff.

```
// This is the same as version 5.6
"player.count = {player.count}"
 -> "player.count = 5"
 
// ... but now you can also do this, as the equals sign will cause Quest to
// treat the rest as code.
"You are in the {eval:player.parent.name}"
 -> "You are in the kitchen"
"You are in the {=player.parent.name}"
 -> "You are in the kitchen"
 
// It can be as complex as you like; it is just Quest code that results in a string.
"You are in the {=CapFirst(player.parent.name)}"
 -> "You are in the Kitchen"
 
// ... or a number
"There are {=ListCount(AllObjects())} objects"
-> "There are 6 objects"
"Carry capacity remaining: {=player.maxvolume - GetVolume(player, false)} kg"
-> "Carry capacity remaining: 5 kg"

 // And it will add brackets if it is ONLY a function call with no parameters.
"You look out the window: {=LookOutWindow}"
 -> "You look out the window: A figure is moving by the bushes"

// An example of displaying curly braces 
"player.count = @@@open@@@player.count@@@close@@@"
 -> "player.count = {player.count}"
 
// You can now do complex conditions with "either"
"You {either StartsWith(player.name, \"play\") and not player.flag:are the player}"
 -> "You are the player",
"'Oh, {either player.male_flag:he:she} is not worth it.'"
 -> "'Oh, he is not worth it.'",
```

Icelandic
---------

We are indebted to Kaspar Jan for providing a translation into Icelandic.
 
 
New functions
-------------

```
ScopeUnlockedExitsForRoom
CloneObjectAndMoveHere
```

### String utilities

```
FormatList (Split("one;two;three"))
 -> "one, two and three"
Spaces (5)
 -> "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
ToWords (42)
 -> "forty two"
ToRoman (42)
 -> "XLII"
```

### Index of string or object in list

```
IndexOf
```

### Get one at random

```
PickOneString
PickOneObject
PickOneChild
PickOneChildOfType
PickOneExit
PickOneUnlockedExit
```


Annotations
-----------

This allows authors to add notes for their own purposes to rooms (names of attributes, what happens here, things to do, etc.). This can be activated by ticking the box on the Features tab of the game object, and adds a new tab to rooms. It has no effect on game play.



Clothing
--------

This is my clothing library, which is itself an extension of Chase's wearables library. Set an object to be Wearable on the features tab to see the new Wearable tab. Advanced options will appear if you tick the box on the Features tab of the game object.

If you already use either my library or Chase's you should find this works with no effect on your part, you just need to delete the library from your game. 
 
 
 
Minor Changes
-------------

DoAskTell now adds a local variable, "text", set to the text value, for the script that runs for an unknown topic so we can now do:
msg("Mary says, 'I don't know about anything about " + text + ".")

Added a "possessive" attribute to Inanimate object, Male character, etc, with the value "its", "his", etc. This will complement the "gender" and "article" attributes. See [here](http://textadventures.co.uk/forum/samples/topic/3556/possessive-pronoun).

Added "me" as an alternative alias of the player, so if the author changes the player alias, LOOK AT ME will still work.

An object initialisation is now done directly after the game.start script runs; for any object with a script called \_initialise\_, the script will be run. This will allow library authors to automatically initialise libraries. For objects, you can turn on a new tab on the Features tab; the new tab will show this script. This will be a great way for users of the web version to set up attributes, given they have no attribute tab; attributes for an object can be set up in the \_initialise\_ script for that object, instead of having everything in the game start script.

Added an object attribute, "dropdestination", to the Room tab of rooms. If this is set, then when an item is dropped in the room it ends up at that object, rather than the room. E.g., if the player is up a tree, dropped items end up at the bottom of the tree.

Modified the "put" command so that objects that cannot be dropped also cannot be put inside containers.

The drop script for objects will now have access to a local variable "destination", which will be where the dropped item should end up (i.e., a container if the command is "put", the dropdestination if set or just the room). See [here](http://textadventures.co.uk/forum/general/topic/er9yijag3ekdrpvj4uh-ra/dropping-stuff).

The `ScopeReachableNotHeldForRoom` function now also returns the object list returned by `SecondaryScopeReachableForRoom`, which is empty by default. The `SecondaryScopeReachableForRoom` function can be overridden (takes the room as a parameter, returns an object list) to return backdrop items, such as wall, ceiling, sky and sun, to make these present in every room (or selectively in some rooms and not others; it is a function, do what you like with it).

Added an extra verb template for lookat "look" so LOOK PLATE will do the same as LOOK AT PLATE. See [here](http://textadventures.co.uk/forum/quest/topic/zwhhqiwlfecip0emay99eg/look-and-look-at).

If an exit has a "message" attribute, this will be displayed before the player is moved (unless the exit runs a script). See [here](http://textadventures.co.uk/forum/quest/topic/pzotaae1x0qc91bvnlua9q/displaying-a-message-after-choosing-an-exit).

Look directions are rarely used, so this is now a feature that must be turned on in the game object (this only changes the editor, so no effect on existing games). See [here](
https://github.com/textadventures/quest/issues/681).

If a room is dark, the game will return a message saying it is too dark to see anything. Previously it said nothing (if there was no message set), which I think was a bug. Note that the message is the same one that is seen when examining an object and it is too dark to see it. It works in English, it might not in other languages.

The `DiceRoll` function has been expanded to understand "d6" and "4d6+2" (but will still work with existing code). A good use of `DiceRoll` is to determine the effect of an attack in an RPG-style game, and the characteristics of the attack can be stored as strings in this form (which is standard in the RPG world).

The Containers tab in the editor now has comments under the scripts that explains when they run, and that names the flags, "isopen" and "locked", that track its state.

If you set a string attribute on a switchable object called "cannotswitchon", then when the player tries to turn the object on, this messagwe will be displayed instead. This will allow authors to have devices that must be fixed before they will work, or have light sources that require power or fuel. To allow the object to be turned on, just set the attribute to null, by the way.




 
 
 
Language support
----------------

If you have a language file for the game, these templates should be added. 

``` 
<template name="NeutralPossessive">its</template>
<template name="MalePossessive">his</template>
<template name="FemalePossessive">her</template>
<template name="SelfPossessive">your</template>
<template name="NeutralPluralPossessive">their</template>
<template name="MalePluralPossessive">their</template>
<template name="FemalePluralPossessive">their</template>
<template name="Nothing">nothing</template>

<template name="SelfAlt">me; myself; self</template>

<dynamictemplate name="WearSuccessful">"You put " + object.article + " on."</dynamictemplate>
<dynamictemplate name="WearUnsuccessful">"You can't wear " + object.article + "."</dynamictemplate>
<dynamictemplate name="CannotWearIfNotHeld">"You would need to get it before you can put it on."</dynamictemplate>
<dynamictemplate name="CannotRemoveIfNotHeld">"You would need to get it before you can take it off."</dynamictemplate>
<dynamictemplate name="AlreadyWearing">"You are already wearing " + object.article + "."</dynamictemplate>
<dynamictemplate name="CannotRemoveIfNotWearing">"You are not wearing " + object.article + "."</dynamictemplate>
<dynamictemplate name="NotRemovable">"You cannot remove " + object.article + "!"</dynamictemplate>
<dynamictemplate name="CannotWearOver">"You cannot wear that over " + GetDisplayGarment(object) + "."</dynamictemplate>
<dynamictemplate name="CannotWearWith">"You cannot wear that while wearing " + GetDisplayGarment(object) + "."</dynamictemplate>
<dynamictemplate name="RemoveSuccessful">"You take " + object.article + " off."</dynamictemplate>
<dynamictemplate name="RemoveFirst">"You can't remove that while wearing " + GetDisplayGarment(object) + "."</dynamictemplate>
<template name="wornmodifier">worn</template>
<!-- verb templates allow for WEAR HAT, etc., whilst WearCommand handles PUT HAT ON -->
<template name="Wear">Wear</template>
<template name="WearCommand">put #object# on</template>
<verbtemplate name="wear">wear</verbtemplate>
<verbtemplate name="wear">put on</verbtemplate>
<verbtemplate name="wear">don</verbtemplate>
<template name="Remove">Remove</template>
<template name="RemoveCommand">take #object# off</template>
<verbtemplate name="remove">remove</verbtemplate>
<verbtemplate name="remove">take off</verbtemplate>
<verbtemplate name="remove">doff</verbtemplate>
```

There are considerably more for the editor. Do a compare on Github to see what the changes are (or contact me).