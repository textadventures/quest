---
layout: index
title: Move an object in a direction
---


Occasionally you might want to have the player move an object from one room to another by pushing or pulling it, rather than carrying it. Let us suppose there is a heavy crate; the player cannot lift it, but she could push it into the room to the west, then climb up it to get to a trapdoor in the ceiling.

Basic Command
-------------

This is pretty easy to do in its simplest form. We need a new command, with this pattern:

> push #object# #exit#

The script will only run if both the object and exit have a match, so all the script has to do is move the object to the destination of the exit (which is set in its "to" attribute), and tell the player:

```
msg ("You push " + object.article + " " + exit.alias + ".")
object.parent = exit.to
```

You might also want the player to end up in the other room - I am not sure what the player would expect. If so, then just add an extra line.

```
msg ("You push " + object.article + " " + exit.alias + ".")
object.parent = exit.to
game.pov.parent = exit.to
```

For the rest of this page, I will be moving the player as well. If you do not want that to happen, just delete that line from your code.


Limiting objects
----------------

It is likely you will not want the player to move any object in this way. There will be some that cannot be moved, and some that are too small (the player can just pick them up). We will therefore flag objects that can be pushed as "shiftable".

On the desktop version, go to the _Attributes_ tab of the crate, and add a new attribute, "shiftable". Set it to be a Boolean, and tick it so it is true.

On the web version, go to the _Features_ tab of the crate, and make sure "Run an initialisation script for this object" is ticked. Go to the _Initialisation script_ tab, and add this code:

```
this.shiftable = true
```

We then need to adjust the code for the command to check for that flag:

```
if (not GetBoolean(object, "shiftable")) {
  if (GetBoolean(object, "take")) {
    msg ("Just pick " + object.article + " up!")
  }
  else {
    msg ("Push all you like, you can't move " + object.article + ".")
  }
}
else {
  msg ("You push " + object.article + " " + exit.alias + ".")
  object.parent = exit.to
  game.pov.parent = exit.to
}
```

We use `GetBoolean` because most objects will not have a "shiftable" attribute. `GetBoolean` will return false if the "shiftable" attribute is false or if it is absent altogether.

We also check the value of the "take" attribute to give a different error message when the player tries to push a hat, compared to trying to push a wall.


Limiting directions
-------------------

We can stop the player pushing the object up or down (you may feel objects can be pushed downwards, just delete those three lines):

```
if (not GetBoolean(object, "shiftable")) {
  if (GetBoolean(object, "take")) {
    msg ("Just pick " + object.article + " up!")
  }
  else {
    msg ("Push all you like, you can't move " + object.article + ".")
  }
}
else if (exit.alias = "up") {
  msg ("You can't push " + object.article + " up there.")
}
else if (exit.alias = "down") {
  msg ("You can't push " + object.article + " down there.")
}
else {
  msg ("You push " + object.article + " " + exit.alias + ".")
  object.parent = exit.to
  game.pov.parent = exit.to
}
```

You can also limit specific exits. 

If you are using the desktop version, Go to the _Attributes_ tab of the exit, give it a new attribute, "noshifting", and type in a suitable message, such as "There is a step stopping you.".

On the web version, you would have to give the exit a name, and then set its "noshifting" attribute in the game start script, like this (for an exit called "noshiftingexit"):

```
noshiftingexit.noshifting = "There is a step stopping you."
```

Then update the code:

```
if (not GetBoolean(object, "shiftable")) {
  if (GetBoolean(object, "take")) {
    msg ("Just pick " + object.article + " up!")
  }
  else {
    msg ("Push all you like, you can't move " + object.article + ".")
  }
}
else if (HasString(exit, "noshifting")) {
  msg (exit.noshifting)
}
else if (exit.alias = "up") {
  msg ("You can't push " + object.article + " up there.")
}
else if (exit.alias = "down") {
  msg ("You can't push " + object.article + " down there.")
}
else {
  msg ("You push " + object.article + " " + exit.alias + ".")
  object.parent = exit.to
  game.pov.parent = exit.to
}
```

Other commands
---------------

You might also want to create commands for PULL (which definitely should move the player too), SHIFT and MOVE. The code will be the same, except the messages should be modified as appropriate.


Finally
-------

This is outside the topic of the page, but is included for completeness.

So the player can get the crate to the other room, how do we handle climbing on the crate to get to the trapdoor? Here is one approach.

Set the crate to be a room as well as an object (_Setup_ tab). On the _Room_ tab, set the description to be a script, and paste this in.

```
msg("You are stood on a crate in " + GetDisplayName(this.parent) + ".")
```

Give it a "climb" verb, and add this code:

```
msg("You climb on to the crate.")
game.pov.parent = this
```

On the _Exits_ tab, give it a "down" exit (to any room, but make sure it is one way). For the exit, tick the "Run a script" checkbox, and paste in this script:

You should now be able to CLIMB the crate, and use the DOWN direction to get off it.

Create a second exit from the crate, to the room above the trapdoor. Tick it to be scenery, and to run a script. This code assumes the room with the trapdoor is "other room":

```
if (this.parent.parent = other room) {
  msg ("From the crate you can just reach the trapdoor. You pull yourself through.")
  game.pov.parent = this.to
}
else {
  msg ("Nowhere to go up to from here.")
}
```
