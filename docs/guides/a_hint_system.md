---
layout: index
title: A Hint System
---

Adding a hint system will allow more players to get to the end of your game, and so see more of your brilliant creation. but only if it works properly! So let us see how to do just that...

The HINT Command
----------------

First you need a HINT command, so right click on the left pane, and select Add Command. Give it a pattern, "hint;clue" and a name, say "Hint" (the name does not matter, the pattern does). In the script section, click on the seventh icon, code view, and paste the code in:

    if (HasScript (game.pov.parent, "hint")) {
      game.pov.parent.hintflag = false
      do (game.pov.parent, "hint")
    }
    if (not GetBoolean(game.pov.parent, "hintflag")) {
      flag = false
      foreach (hint, GetDirectChildren (hints)) {
        if (not flag) {
          if (not GetBoolean(hint, "passed")) {
            flag = true
            hint.done = true
            if (HasScript (hint, "look")) {
              do (hint, "look")
            }
            if (HasString (hint, "look")) {
              msg (hint.look)
            }
          }
        }
      }
      if (not flag) {
        msg ("Sorry, no more clues")
      }
    }

Hint Set-up
-----------

Create a room called "hints". This should have no exits and should not be accessible to the player.

For each hint, create an object inside the "hints" room, and give it a "look at" description. These need to be in the order the player will need them (to change the order, you can go to the Objects tab of the "hints" room).

At each stage-gate, set the "passed" attribute of the appropriate hint to true.

Note: Do not put anything else in the hints object - it will be taken as a hint

Naming Hints
------------

Personally I like to prefix hints with h\_, so the names are unique and it is obvious what it is.


Stage-gates?
------------

So what are stage-gates? A stage-gate is where the player goes from one stage of the game, where one hint is relevant, to the next, where the next hint applies. Presumably that first hint indicates how to achieve that. It could be killing the goblin, getting to a certain room, unlocking a door, finding a vital item.


Progressive hints
-----------------

You can set up a series of hints for one stage gate, so that each time the player types HINT he gets a more obvious clue. To set this up, you obviously need a hint object for each hint. Each of these hints except the last should have a "look at" *script*, rather than text, and in the script, as well as displaying a message, should set its "passed" value to true.

The script might look like this:

    msg ("Now you have the key, what can you do with it?")
    this.passed = true

In addition, the stage-gate itself must set the "passed" value to true for all the hints.

    msg ("You unlock the door")
    h_unlock1.passed = true
    h_unlock2.passed = true
    exit_to_room3.locked = false

    
Local Hints
-----------

You can have local hints. If a room has a script called "hint" this will be run. If the script sets the attribute hintflag on the room to true, then no other hints are given. The script should test a condition - is the hint still relevant? If the condition is met, a hint is given, and the hintflag set. If the condition is not met, the flag is not set and the standard hint system comes into play.

Here is an example:

    if (exit_to_room3.locked) {
      msg ("Try unlocking the door in room2")
      this.hintflag = true
    }

If the player is in the room, and types HINT this script will run. If the door is locked, an appropriate message is displayed, and hintflag is set to stop any further processing of the hint system. If the door is not locked, the hint is not relevant. No message is displayed, and as hintflag is not set, the normal hint system comes into play.

This would be useful in rooms off the main quest. Player can get a hint to solve the puzzle when in the room. However, if that puzzle is solved, the player gets a hint towards the main quest as usual.


Testing
-------

Do make sure that each and every hint works. It is easy to spell the hint name one way in one place and another elsewhere. The game will only throw an error when it comes to display it.