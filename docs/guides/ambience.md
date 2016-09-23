---
layout: index
title: Adding Ambience (Random Background Text)
---


This is an extension of "adding scenery to a group of room", but is a little more complicated. I suggest looking at that page first as the example buildings on that.

[Add Scenery To Groups of Rooms](scenery.html)

Let us add some flavour to the forest; random things seen in the background. As the player wanders through the forest, he might see a rabbit watching him, or butterflies flitting about. We already have a system that groups all the forest locations, so we can start with that (it is less work to have the same random options for any forest location rather than setting them up for each location individually). All we need to do, then, is have a script run when the player enters a room that displays the randomly text.

Except the player is sure to type X RABBIT. We also need to implement every one of those random items. Or do we? How about just implementing one, and disguising it as a rabbit or two butterflies as we need it?

First create an object called "flavour_object", and tick the scenery box. No need to do anything else with it.

In each of your container rooms (like e_forest in the Adding Scenery page) you need to set up three string list attributes, called "flavour", "flavournames" and "flavourdescs". You then need to add some values.

In the first, "flavour", type in the texts the player will see when entering that location, for example:

    A rabbit watches you.
    A gust of wind rustles the leaves.
    Butterflies chase each other over the flowers.

In the second, "flavournames", type in the names of the objects. This is what the player might type to look at the thing mentioned in the text.

    rabbit
    
    butterfly|butterflies

Note that I left the second blank, because there was no object. The system will handle that fine, but you must leave a blank one to keep the rest in the right place. You can put in multiple names separated by a vertical bar, as with the last.

In the third, "flavourdescs", type in the description the player will see if she looks at the object. Again, the blank one is important.

    It is a large brown bunny with big floppy ears.
    
    The butterflies are bright blue.

Then the coding. On the game object you will see on the Script tab a section called "Script when entering a room". Paste the code below in there:

    flavour_object.parent = game
    if (HasAttribute(game.pov.parent, "parent")) {
      if (HasAttribute(game.pov.parent.parent, "flavour")) {
        n = GetRandomInt(0, 20)
        if (n < ListCount(player.parent.parent.flavour)) {
          msg (StringListItem(player.parent.parent.flavour, n))
          if (HasAttribute(player.parent.parent, "flavournames") and HasAttribute(player.parent.parent, "flavourdescs")) {
            if (n < ListCount(player.parent.parent.flavournames) and n < ListCount(player.parent.parent.flavournames)) {
              if (not StringListItem(player.parent.parent.flavournames, n) = "" and not StringListItem(player.parent.parent.flavournames, n) = "") {
                flavour_object.parent = player.parent
                flavour_object.alt = Split(StringListItem(player.parent.parent.flavournames, n), "|")
                flavour_object.look = StringListItem(player.parent.parent.flavourdescs, n)
              }
            }
          }
        }
      }
    }

The code will run each time the player enters a room. First it moves the flavour_object object away to reset itself, then it checks a load of things before proceeding (that this room has a containing room, that the containing room has the needed string lists).

It then generates a random number, from 0 to 20 (you may want to adjust that; I use 5 in the example game below). If the random number is less than the number of texts, then it prints the text.

Next it checks if there are entries in the flavournames and flavourdescs lists. If there are, then it moves the flavour_object object, and then disguises it as the object mentioned, by setting its "alt" and "look" attributes (the "alt" attribute is an array of alternative names, so is more convenient than setting the "alias" attribute - and you cannot change the "name" attribute during the game).

Example game:

    <!--Saved by Quest 5.6.5510.29036-->
    <asl version="550">
      <include ref="English.aslx" />
      <include ref="Core.aslx" />
      <game name="forest">
        <gameid>14bb0e0b-4b6c-4c27-96fa-c6c7e975d392</gameid>
        <version>1.0</version>
        <firstpublished>2015</firstpublished>
        <roomenter type="script"><![CDATA[
          flavour_object.parent = game
          if (HasAttribute(game.pov.parent, "parent")) {
            if (HasAttribute(game.pov.parent.parent, "flavour")) {
              n = GetRandomInt(0, 5)
              if (n < ListCount(player.parent.parent.flavour)) {
                msg (StringListItem(player.parent.parent.flavour, n))
                if (HasAttribute(player.parent.parent, "flavournames") and HasAttribute(player.parent.parent, "flavourdescs")) {
                  if (n < ListCount(player.parent.parent.flavournames) and n < ListCount(player.parent.parent.flavournames)) {
                    if (not StringListItem(player.parent.parent.flavournames, n) = "" and not StringListItem(player.parent.parent.flavournames, n) = "") {
                      flavour_object.parent = player.parent
                      flavour_object.alt = Split(StringListItem(player.parent.parent.flavournames, n), "|")
                      flavour_object.look = StringListItem(player.parent.parent.flavourdescs, n)
                    }
                  }
                }
              }
            }
          }
        ]]></roomenter>
        <object name="flavour_object">
          <inherit name="editor_object" />
        </object>
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
          <flavour type="stringlist">
            <value>A rabbit watches you.</value>
            <value>A gust of wind rustles the leaves.</value>
            <value>Butterflies chase each other over the flowers.</value>
          </flavour>
          <flavournames type="stringlist">
            <value>rabbit</value>
            <value></value>
            <value>butterfly|butterflies</value>
          </flavournames>
          <flavourdescs type="stringlist">
            <value>It is a large brown bunny with big floppy ears.</value>
            <value></value>
            <value>The butterflies are bright blue.</value>
          </flavourdescs>
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
