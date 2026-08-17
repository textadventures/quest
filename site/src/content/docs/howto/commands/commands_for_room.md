---
title: Commands Specific to a Room
sidebar:
  order: 1
---


Frequently you will want to add a command that is specific to a certain room.  If the command involves an object, then Quest will attempt to match the object (for example, `ATTACK GOBLIN`, Quest will start by trying to match `GOBLIN` to any object present). Often there will be no object, for example, the player might have to `CLIMB` or `SWIM`.

One way to do that would be to test whether you are in the right room in the command script. However, the cool way is to have two commands - one in the room itself.

Let us suppose a room where the player has to `CLIMB`.

### The general command

In most rooms, CLIMB will do nothing. We will handle that first. Create a command as usual, and give it this pattern:

    climb

Now click the "Add new script" button, select "Print" and paste in some suitable text, "Nothing to climb here."

### The room command

Now create a second command. This will be the command that actually does something. Give it the same pattern, click the "Add new script" button, select "Print" and paste in some suitable text, "You climb the drainpipe, and go in through the window." Then click the "Add new script" button again, select "Move Object". Select the player object and some suitable room.

Now the clever bit. Use the tree's "Move to..." option (in its "..." menu, or click "Move" towards the top right) to move the command into the room the player can climb in (make sure you do the right one!).

If the player is in that room, Quest will use the `CLIMB` command for the room, and the player will climb the drainpipe. Otherwise the general command will be used.

### Another room?

If you have more than one room the player can climb in, just create a new command for each one. Each will have its own text and destination, and no need for any of those `if/else` things!
