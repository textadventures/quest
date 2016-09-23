---
layout: index
title: Implement drop...
---

Yes, I know drop is already implemented, but it could do better.

The Quest "drop" command readily handles unusual objects (drop that vase and it will break), but cannot handle unusual places (drop something when up a tree, and there it is lying on the floor of the top of the tree).

This will show you how drop can be extended to give three new options based on where the player is.

Drop an item here, it ends up over there
----------------------------------------

If the player is up a tree and he drops something, it should end up on the ground under the tree. On the top of the tree location, create an attribute "drop\_destination", as an object, and set it to the room that represents the bottom of the tree.

Drop an item, it is gone forever
--------------------------------

If the player is flying over the ocean and drops something, it is gone forever. Have a room set up that the player cannot get to (I like to call it "nowhere"), and set the "drop\_destination" attribute to there.

Give a message instead of dropping it
-------------------------------------

If the player is flying over the ocean and drops something, it is gone forever, so do not let him; explain he will lose it forever. On the location, create an attribute "drop\_prohibited", as a string, and set it to the message to display.

New Functions
-------------

To get this to work, you need to add two new functions to Quest. One of them, DoDrop, is actually replacing an existing function. One of the powers of Quest is that it will happily let you do that, so you can change how pretty much anything works by modifying existing functions.

      <function name="DropRedirection" parameters="object" type="string">
        if (HasString(player.parent, "drop_prohibited")) {
          return (player.parent.drop_prohibited)
        }
        else if (HasObject(player.parent, "drop_destination")) {
          object.parent = player.parent.drop_destination
          return (null)
        }
        else {
          object.parent = player.parent.drop_destination
          return (null)
        }
      </function>

      <function name="DoDrop" parameters="object, ismultiple"><![CDATA[
        prefix = ""
        if (ismultiple) {
          prefix = GetDisplayAlias(object) + ": "
        }
        if (not ListContains(ScopeInventory(), object)) {
          msg (prefix + DynamicTemplate("NotCarrying", object))
        }
        else if (not ListContains(ScopeReachable(), object)) {
          msg (prefix + DynamicTemplate("ObjectNotOpen", GetBlockingObject(object)))
        }
        else {
          found = true
          dropmsg = object.dropmsg
          switch (TypeOf(object, "drop")) {
            case ("script") {
              if (ismultiple) {
                msg (prefix)
              }
              do (object, "drop")
              dropmsg = ""
            }
            case ("boolean") {
              if (object.drop = true) {
                // -- start modification --
                dropmsg = DropRedirection (object)
                // -- end modification --
                if (dropmsg = null) {
                  dropmsg = DynamicTemplate("DropSuccessful", object)
                }
              }
              else {
                found = false
              }
            }
            case ("string") {
              object.parent = player.parent
              dropmsg = object.drop
            }
            default {
              found = false
            }
          }
          if (not found and dropmsg = null) {
            dropmsg = DynamicTemplate("DropUnsuccessful", object)
          }
          if (LengthOf(dropmsg) > 0) {
            msg (prefix + dropmsg)
          }
          if (HasScript(object, "ondrop")) {
            do (object, "ondrop")
          }
        }
      ]]></function>

The DropRedirection function handles all the new stuff, but the existing DoDrop function had to be changed to call DropRedirection. I split it up so we can see at a glance what is new, and have tagged the modified part of DoDrop with comments so that is clear too. This means that if a later version of Quest modified DoDrop, it should be relatively easy to update the new version of DoDrop.

You can see this all in action in this demo adventure (which includes a walk-though with assertions to confirm when the object is after dropping it):

![](DropDemo.aslx "DropDemo.aslx")
