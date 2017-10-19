---
layout: index
title: Immobilise the player
---

Occasionally you want to stop the player moving to another room, say because he is sat down, and he has to stand first, or he is tied up or whatever. There are three ways (at least) to do this:

Move to another room
--------------------

The first way is to move him to another room with no exits. This might not be appropriate in some cases, but suppose the player logs on to a computer, and while logged on, all commmands relate to the computer. The player does not need to interact or even see other objects in the room with the computer, so effectively moving the player inside the computer is a good solution.


Block the exit
--------------

If this is something that happens in one specific room with a small number of exits, then the easiest solution is to just lock the exits while the player is immobilised, unlock them afterwards.


Rewrite the go command
----------------------

This is a little more complicated, but more general. What you do is set a string on the player when he is immobilised. The GO command checks to see if this string exists, and if it does, rather than moving the player, it prints the string.

The way to do this is to rewrite the GO command.

Create a new command, and set the pattern to be a regular expression. In the text box below, paste in this string:

> ^go to (?<exit>.*)$|^go (?<exit>.*)$|^(?<exit>north|east|south|west|northeast|northwest|southeast|southwest|in|out|up|down|n|e|s|w|ne|nw|se|sw|o|u|d)$

Then paste in this code.

```
if (HasString (player, "immobilisedmessage")) {
  Print (player.immobilisedmessage)
}
else if (exit.visible) {
  if (exit.locked) {
    msg (exit.lockmessage)
  }
  else if (exit.runscript) {
    if (HasScript(exit, "script")) {
      do (exit, "script")
    }
  }
  else if (exit.lookonly) {
    msg ("You can't go there.")
  }
  else {
    if (HasString(exit, "message")) {
      if (not exit.message = "") {
        msg (exit.message)
      }
    }
    game.pov.parent = exit.to
  }
}
else {
  msg ("You can't go there.")
}
```

As it has the same pattern as the built-in GO command, this will get used instead. The only difference is that before doing anything else, the script checks for the "immobilisedmessage" string on the player. If it is there, it gets printed. If not it proceeds as normal.

Now to immobilise the player, your code needs to do something like this:

```
player.immobilisedmessage = "Cannot move while seated."
```

To allow him to move again:

```
player.immobilisedmessage = nil
```
