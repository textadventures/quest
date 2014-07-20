---
layout: index
title: Running a script when an attribute changes
---

You can run a script when an attribute changes by creating another script attribute, called "changed" + the name of the attribute.

For example, to run a script when the value of the "isopen" attribute changes, add a script attribute called "changedisopen".

When the player moves, the value of their "parent" attribute changes. This means you can call some script when this happens by setting a "changedparent" script attribute for the player. This is what Core.aslx uses to trigger the printing of the room description:

      <object name="player">
        <changedparent type="script">OnEnterRoom</changedparent>
      </object>

This calls the "OnEnterRoom" function every time the player moves into a different room.

Another way this might be useful would be if you had a "health" attribute for the player – you could create a "changedhealth" script which would finish the game if the health value reached zero, and maybe also set a maximum value.

As of Quest 5.1, the script has access to a variable "oldvalue" which contains the previous attribute value.

Example:

        <object name="player">
          <inherit name="defaultplayer" />
          <changedparent type="script">
            player.movement=player.movement+1
            msg ("oldparent:" + oldvalue)
            OnEnterRoom
          </changedparent>
          <movement type="int">0</movement>
          <changedmovement  type="script">
                msg ("oldmovement:" + oldvalue + " - actual moves:" + player.movement)
          </changedmovement>
        </object>
