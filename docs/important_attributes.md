---
layout: index
title: Important Attributes
---

Attributes are what make objects (including rooms) in Quest what they are and do what they do. Where an object is, where it can be picked up, what it looks like, whether it can be locked or worn or eaten are all handled with attributes. Furthermore, attributes are the only things that change as the game progresses.

You can add your own attributes to an object, but this page is about the built-in attributes that may be important as you craft your game.


Quest has a number of ways of naming things...


The name attribute
------------------

Everything in Quest is identified by its name attribute; effectively this is the ID of the database record. This means everything must have a name (they are some things that get names automatically, such as exits), and each must be unique.

The name is also the way to reference the object in code, and this means there are certain characters that cannot be used, including most punctuation. The name attribute can contain spaces, but not at the start or end, and it cannot contain consecutive spaces. Some people do not like spaces in names, as it looks weird in code if you are familiar with programming. It can contain digits, but not start with a digit. It can contain underscores. It can also contain upper and lower case letters. Note that when you later refer to an object by its name, the name is case-sensitive.

It is good practice to have a consistent naming policy, for example, always using lower case. That will make it easier to remember what you called it later.

The name attribute is the only one of these attribute that is required; the rest are optional.


The alias attribute (and others)
--------------------------------

The alias attribute is what the player will see when Quest mentions the object, for example in a list of objects present in the room. If it does not exist, the name attribute is used instead. Similarly, when matching objects in a command the player has typed, Quest will use the alias attribute if it exists, and the name attribute otherwise (if there is an alias attribute, it will not even attempt to match with the name).

The alias attribute can contain any characters you like, so you could use this for items that have punctuation in their title, such as "Dave's ball".


### The listalias attribute

The listalias attribute is a string, what the player will see in the _Inventory_ and _Places and Objects_ lists on the right. It can be set on the _Object_ tab. If it does not exist, Quest will use the attribute alias instead, and if that does not exist, the name attribute.

Like the alias attribute, this can contain any characters. 

The listattribute is useful if you want to capitalise objects in the inventory pane, but not in the room descriptions. You can also include HTML codes in the listalias attribute to control how the item is displayed in the pane (if you do this for the alias attribute, the HTML codes will appear in the room descriptions too).

```
<span style="background-color:yellow;">Ball</span>
```

### The alt attribute

The alt attribute is a string list of alternative names, and again is found on the _Object_ tab. Quest will use this list, in addition to alias or name, when trying to match an object to a command the player has typed. This is where you type all the synonyms for the object.

Quest will match bits of names, so if you have "Dave's ball" as the alias, and "blue orb" in the alt list, it will match all these:

> X BALL

> X DAVE

> X BLUE

> X BLUE ORB

If there is also someone called "dave" in the room, it will match that in preference to his ball for X DAVE.

You can also use the alt list in a text processor command. The `rndalt' command will pick an entry in the alt list at random.

```
You see a {rndalt:ball}
```


The parent attribute
------

The parent attribute is an object that determines where a thing is. If the parent is a room, then the object is in that room. If it is the player, then the player is carrying it. If the parent is a container, then the object is in the container.

The parent attribute also determines how an object is displayed in the editor. This means that an object in a room in the editor will also be in that room at the start of the game.

To change the parent in the desktop version of he editor, you can drag the object to its new parent. For the web version, click on the "Move" towards the top right. For any object you want the player to have at the stary, for instance, just move them to the player.

Objects can and will change there parent as the game is played. When the player picks up and then drops an item, its parent will change to the player, and then to the room. When the player moves to a different room, the parent attribute of the player changes to the new room (so the parent attrbute of the player is the current room).

You can change the parent attribute directly in code, or use the helper functions. All three of these will move the object `ball` to the current room:

```
ball.parent = player.parent
MoveObject (ball, player.parent)
MoveObjectHere (ball)
```

The `RemoveObject` function clears the parent attribute (sets it to `null`); for an item, this means it exits in limbo, rather than in a room, so the player can never get at it.

Even commands and turnscripts can have parent attributes. If they do, they will only apply when the player is inside that room.


The look and description attributes
------------------------------------

These are the descriptios the player will see. The "look" attribute is used when the player examines an object. The "description" is seen when the player enters a room. Both can be a string or a script.




The visible and scenery attributes
--------------------

These can be set on the first tab, both are flags (Booleans) and both apply to exits as well as objects.

If an object is not visible, then effectively it does not exist. The player cannot see it and cannot interact with it.

This then is a good way to keep objects "off-stage". Have the object in the room already, but with visible set to false (untick the box). When the player does whatever it is, set the object be visible, and suddenly it will be there. This can be especially useful for exits (say a hole in the wall that suddenly appears, or conversely set an exit to be invisible if it is blocked).

The scenery attribute can also be used to hide objects - but in this case your should set scenery to true (tick the box). An item that is scenery cannot be seen in the list of object for the room, but the player can still interact with it. It can be examined, picked up (depending on setting on the _Inventory_ tab), etc. This is best used for objects that are mention in the room description, so the player knows they exist, and so might want to interact with them.

The scenery attribute is set to false when an object is picked up. This means that if the player drops it in another room, it will appear in the list of objects in that room. If the object is picked up, it will be odd if it is still in the room description; you can use the text processor to handle that, using the scenery attribute.

For example:

> This is an empty room, dominated by a marble fireplace{if ornament.scenery:, with a bizarre ornament on the mantelpiece}.

The player will see:

> This is an empty room, dominated by a marble fireplace, with a bizarre ornament on the mantelpiece.

However, when the ornament is picked up, scenery will be set to false, and thereafter the player will see:

> This is an empty room, dominated by a marble fireplace.

Setting an exit to scenery can be a good idea if there are two ways it could be used. If there are stairs going down to the east, you would want to have both EAST and DOWN going to the same destination, but you might want to set one to scenery so only one appears on the compass rose (otherwise the player might think there are exits to two different locations).





The to attribute
----------------

The to attribute of an exit is an object - where the exit goes to (the parent attribute is when it comes from, of course).



The locked and isopen attributes
--------------------------------

Obviously these determine if something is locked or open (the "open" verb uses the "open" attribute, so Quest had to use "isopen" instead). Exits can be locked; containers can be opened or locked, an item that is openable/closeable can be opened.

Note that Quest will handle the setting of "isopen" for a container. However, for openable/closeable items, it is up to you to provide a script that will do that (this is to allow you to check if the item can be opens first). If you set a container to be lockable on the _Container_ tab, Quest will likewise handle the lockable attribute for you.



Various verb attributes
-----------------------

Several attributes are used by Quest to determine how the object will respond to commands:

> take, drop, use, open, close, lock, unlock


