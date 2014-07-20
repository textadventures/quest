---
layout: index
title: Using lockable exits
---

Creating the exit
-----------------

You can create an exit which is impassable until something else happens in your game. This could be a locked door, or perhaps something like a guard blocking the way.

Let's create a locked door in the kitchen, leading to a back garden. Create the following three elements:

-   a new room, "garden"
-   an object in the kitchen, "door"
-   an exit leading south from the kitchen to the garden

Select "Exit: garden" in the tree and tick the "Locked" box. You should see a warning message that we need to give the exit a name. This is because, to unlock the exit during the game, we will need to use a script command. The script command will need some way of referring to this particular exit, which is why we need to give it a name here. Call it something like "garden exit".

![](Lockedexit.png "Lockedexit.png")

Unlocking the exit with a script
--------------------------------

Go to the door object, and on the Verbs tab add a verb "unlock". Set it to "Run a script", and then add a command to print a message (such as "You unlock you door"). Add an "unlock exit" command, and choose "garden exit" from the list.

![](Unlockexit.png "Unlockexit.png")

Run the game and verify that the exit now works correctly:

     > south
     That way is locked.
     
     > unlock door
     You unlock the door.
     
     > south
     You are in a garden.
     You can go north.
