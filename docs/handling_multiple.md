---
layout: index
title: Handling Multiple Items (and All)
---


Some commands will allow the player to give a list of items to apply the action to, or to just say ALL. For example, the player can type GET ALL or DROP BAT, BALL AND HAT.

Handling ALL is not straightforward, as we need to consider exactly what items to consider.

Note that turnscripts will only fire once per command, rather than once per object.

TAKE ALL
--------

Let us suppose there is a rucksack with a book in it, an open cupboard with a ball of string in it, a character called Mary, who is holding a cup, and a table with an apple on it. There is also a door that is mentioned in the room description, and is implemented, but is just scenery, and not listed when the player types LOOK.

> You can see a rucksack (containing a book), a cupboard (containing a ball of string), a Mary (carrying a cup) and a table (on which there is an apple).
> > GET ALL
> rucksack: You pick it up.
> cupboard: You can't take it.
> ball of string: You pick it up.
> table: You can't take it.
> apple: You pick it up.

Note that Quest does not try to take the book; it is inside the rucksack, and that has been picked up already. It will get the string and the apple, though, as they are in containers that cannot be taken (even if the containers are scenery, by the way, though they would not normally appear in the room description).

There is no attempt to take the door, as it is scenery.

Quest also does not try to take Mary, as she is a character. It can be useful to set up characters as surfaces or (as in the example above) transparent containers so the player can see what they are carrying. GET ALL will also ignore any item carried by a character (but note that items inside items held by characters are not properly supported!).


### Excluding other items

Just as Mary was excluded from the ALL list, you can exclude other items, just by ticking the "Object is excluded..." box on the _Inventory_ tab (behind the scenes this sets their "not_all" flag to true).

Note that this will cause any contained objects to also be excluded, and it may be better to flag the container as scenery instead.


### Characters that can be taken

Conversely, you may want a character to be taken. Perhaps Mary is a poodle that the player can pick up. Just untick the "Object is excluded..." box. You will also need to tick the "Object can be taken" box as normal.

Note that items held by the character will still not get included in ALL.


DROP ALL
--------

What gets dropped is considerably easier

> > i
> You are carrying a rucksack (containing a book), a ball of string, a purse and an apple.
> 
> > drop all
> rucksack: You drop it.
> ball of string: You drop it.
> purse: You drop it.
> apple: You drop it.

The only thing to note is that the book is dropped inside the rucksack, so is not mentioned.


Handling ALL in your own commands
---------------------------------

For the majority of commands, it is not necessary to add the facility for ALL, and most of the built-in commands do not support it. However, if you want to allow it for your custom command, here is what you must do:

The command must have a Boolean attribute called "allow_all" set to true.

You need to set the scope. The tells Quest where to look for objects, and is a good idea for all commands.

You also also need to modify the script. For any command with "allow_all" set to true, the `object` variable will be a list of objects, rather than one object - even if the player only specifies a single object. The list will include any in the given scope, unless "not_all" is true

The script will also have access to a second variable, `multiple`, which will be true if the player said ALL or gave a list of items.

By way of an example, we will look at the script for TAKE:

```
took_something = false
foreach (obj, object) {
  // if this is multiple then we should skip anything in a container that will be taken
  // and anything held by an NPC
  if (not multiple or (not Contains(game.pov, obj.parent) and not DoesInherit(obj.parent, "npc_type"))) {
    DoTake (obj, multiple)
    took_something = true
  }
}
if (multiple and not took_something) {
  msg ("Nothing to take.")
}
```

The first line sets up a flag we will use later. Then we go though each member of the list.

For each item, we need to consider if the item should be included in an ALL list. If `multiple` is false, we need to handle it whatever - he player has specified this item. If it is true, there are some situations where we should not handle it (in this case, if the container has already been taken or if the item is held by a character, but it will be different for you).

Note that any item that is glagged as scenery or as "not_all" will already be excluded from the list.

Then the action is done. In this case, another function is called. Inside that function, if `multiple` is true, the object name and a colon are prefixed to the response.

Finally, we need to handle what happens if there was nothing in the list - for any command, you need to ensure theplayer always get some kind of a response. This will be flagged by the `took_something` flag still being false, and is only applicable if `multiple` is true.