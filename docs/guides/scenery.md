---
layout: index
title: Adding scenery to several rooms
---

Let us suppose we have several locations in a forest. You just know that some smart-ass player will type X TREE, and expect a reply. You could implement a tree object in every room, but there is an alternative - and it does not require any coding.

The trick is to set these locations to be transparent, and then put them inside another location that contains the tree.

Set up a location, say called e_forest. In that location, place an item, tree (with an alternative name "trees", as the player may well type LOOK AT TREE and LOOK AT TREES and we want to handle both; find "Other names" on the Object tab). You might also want to put in other objects like sky, path, bush, etc. Tick the "Scenery" checkbox.

For each forest location: Drag it into the e_forest location (i.e., rooms within the room). Then on the Attributes tab, click Add, type "transparent", set it to be a Boolean, and tick the box.

Quest will note that your forest rooms are transparent, and so the player will be able to see (but not reach) anything in the containing room.

Here is an example game to see it in action.

    <!--Saved by Quest 5.6.5510.29036-->
    <asl version="550">
      <include ref="English.aslx" />
      <include ref="Core.aslx" />
      <game name="forest">
        <gameid>14bb0e0b-4b6c-4c27-96fa-c6c7e975d392</gameid>
        <version>1.0</version>
        <firstpublished>2015</firstpublished>
      </game>
      <object name="hut">
        <inherit name="editor_room" />
        <description>You are in a wooden hut</description>
        <alias>Hut</alias>
        <object name="player">
          <inherit name="editor_object" />
          <inherit name="editor_player" />
        </object>
        <exit alias="out" to="forest_path">
          <inherit name="outdirection" />
        </exit>
      </object>
      <object name="e_forest">
        <inherit name="editor_room" />
        <object name="forest_path">
          <inherit name="editor_room" />
          <description>There is a hut you can go in, or a path going east.</description>
          <alias>Forest Path</alias>
          <transparent />
          <exit alias="east" to="forest_clearing">
            <inherit name="eastdirection" />
          </exit>
          <exit alias="in" to="hut">
            <inherit name="indirection" />
          </exit>
        </object>
        <object name="forest_clearing">
          <inherit name="editor_room" />
          <alias>Forest Clearing</alias>
          <description>A clearing.</description>
          <transparent />
          <exit alias="west" to="forest_path">
            <inherit name="westdirection" />
          </exit>
        </object>
        <object name="tree">
          <inherit name="editor_object" />
          <scenery />
          <look>It is a tall thing with leaves on.</look>
          <alt type="stringlist">
            <value>trees</value>
          </alt>
        </object>
        <object name="sky">
          <inherit name="editor_object" />
          <look>The sk is blue; it is a great day.</look>
        </object>
      </object>
    </asl>

   
  