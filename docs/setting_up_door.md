---
layout: index
title: Setting Up a Door
---

Generally in text adventures doors are implied. That means that when the player heads into the kitchen, it is assumed she first opens the door - the player will not be happy if she has to open a door each time she wants to go anywhere. That said, there may be occasions when it warranted. Perhaps this is a particular special door, or the state of the door determines whether the player is found or which way the guard goes.

The problem with doors is that need to be usable from both sides. In Quest an object is in one room, but the door needs to be in two. But there is a way around that.

Let us suppose we have two rooms; the lounge with the kitchen to the west, with exits going each way. We will say the door is closed to start with.


Exits
-----

To stop the player going through the closed door, tick the "Locked" option for each exit. You will also need to give them names, let us say "exit to kitchen" and "exit to lounge". You should also change the locked message, perhaps to "The door is closed."


The Door...
-----------

Create an object in the lounge; the door. Set it to be scenery. On the _Features_ tab, set it to be a container, then on the _Container_ tab, set it to be "Openable/Closeable".

Quest tracks the state of these things by an attribute called "isopen", and while it will handle that for you for containers, for Openable/Closeables we need to do that ourself (this is because you might want to check if the object can be opened before hand, but that is not necessary in this case). We also need to change whether the exits are locked.

The top script is the one that will be run when the door is opened. That needs to unlock the exits, and open the door:

```
exit to kitchen.locked = false
exit to lounge.locked = false
this.isopen = true
```

The lower script is used when the door is closed:

```
exit to kitchen.locked = true
exit to lounge.locked = true
this.isopen = false
```


... In Both Rooms
------------------

Now we need to have the door in both rooms. We will use a bit of trickery, and make it look like it is in the second room. Go to the _Features_ tab of the game object, and tick "Show advanced scripts for the game object". Then go to the _Advanced scripts_ tab. The bottom script, "Backdrop scope script" is the one we are interested in.

The way the script works is that anything we add to the "items" list will get added to things in the current room. Add this code:

```
if (game.pov.parent = kitchen) {
  list add (items, door)
}
```

The first line checks in the player is in the kitchen, and if she is, the second line adds the door to the list. Now whenever the player is in the kitchen, it will seem as though the door is too.


Rooms
-----

It is a good idea to keep the player informed, and in this case we want her to know whether the door is open or closed. You can do that in the room description using the text processor. For example:

> The lounge is very retro. The door west is {either door.isopen:open|closed}.

This is again using the "isopen" attribute of the door object.


Starts Open?
------------

What if you want the door open at the start of the game? Just make sure the two exits are not locked, and tick the "Is open" box on the _Container_ tab of the door.

