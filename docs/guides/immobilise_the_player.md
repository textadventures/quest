---
layout: index
title: Immobilise the player
---

Occasionally you want to stop the player moving to another room, say because he is sat down, and he has to stand first, or he is tied up or whatever.

Move to another room
--------------------

One way to do this is to move him to another room with no exits, and that might be appropriate in some case. Suppose the player logs on to a computer, and while logged on, all commmands relate to the computer. The player does not need to interact or even see other objects in the room with the computer, so effectively moving the player inside the computer is a good solution.

Block the exit
--------------

If this is something that happens in one room with a small number of exits, then it is easy to just lock the exits while the player is immobilised, unlock them afterwards.

Rewrite the go command
----------------------

This is a little more complicated, but more general. What you do is set a flag on the player, when it is true, the player cannot move, when it is false or absent, he can.

The way to do this is to rewrite the go command.

Here is the modified command, just paste it into your code - but make sure it is inside the asl tags and no other tags. If in doubt, in the GUI, at the bottom left, select "Filter - Show Library Elements", then find "Go" from the list of commands (it will be abpout a third of the way down the greyed out commands). Click the "Copy" button in the top right, then go into code mode. You will find a section of code like that below, but with the name "go" and missing lines five to seven. Replace that with the code below.

      <command name="go_subverted">
        <pattern type="string"><![CDATA[^go to (?<exit>.*)$|^go (?<exit>.*)$|^(?<exit>north|east|south|west|northeast|northwest|southeast|southwest|in|out|up|down|n|e|s|w|ne|nw|se|sw|o|u|d)$]]></pattern>
        <unresolved>You can't go there.</unresolved>
        <script>
          if (GetBoolean (player, "immobilised")) {
            Print (player.immobilisedmessage)
          }
          else if (exit.locked) {
            msg (exit.lockmessage)
          }
          else if (HasScript(exit, "script")) {
            do (exit, "script")
          }
          else {
            if (exit.lookonly) {
              msg ("You can't go there.")
            }
            else {
              player.parent = exit.to
            }
          }
        </script>
      </command>

Quest does not like commands to have the same name as existing commands, so it needs a new name, but the same matching pattern.

Besides that, the only difference is that before doing anything else, the script checks for the immobilised flag on the player. If it is true, it prints a message, the value of the attribute player.immobilisedmessage. If false (or not there) it proceeds as normal.

Now to immobilise the player, your code needs to do something like this:

      player.immobilised = True
      player.immobilisedmessage = "Cannot move while seated."

To allow him to move again:

      player.immobilised = False
