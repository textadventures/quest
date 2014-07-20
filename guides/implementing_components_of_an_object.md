---
layout: index
title: Implementing components of an object
---

Occasionally with an object in an adventure you would like the player to be able to interact with a component of it, for example, a machine with a button on it. The player has to be able to press the button. If the machine cannot be moved, you can just have the button as scenery in the room, but what do we do for objects that can be carried around?

Quest actually has this facility built-in, though it may not be obvious.

Create you object first, let us say it is called "machine". Create the component, "button", as a child of that object (right click on machine, select "Add object"; alternatively you can drag an object on to another). In the object hierarchy it should look like this on the left.

![](component.png "component.png")

The master object, in this case "machine" has to be set up as a type of container called a surface, as shown above. The component, you can have as many as you need, should be set up as scenery.

That is all there is too it.

Here is a brief example in code showing how pressing the button turns the machine on.

        <asl version="520">
          <include ref="English.aslx" />
          <include ref="Core.aslx" />
          <game name="components">
            <gameid>2a1ea913-dcf9-4af0-9390-5d78e8931971</gameid>
            <version>1.0</version>
          </game>
          <object name="room">
            <inherit name="editor_room" />
            <object name="player">
              <inherit name="defaultplayer" />
            </object>
            <object name="machine">
              <inherit name="editor_object" />
              <inherit name="surface" />
              <look type="script">
                if (GetBoolean (machine, "switchedon")) {
                  msg ("A big mechanism, humming with power and slightly warm, with a button on the side.")
                }
                else {
                  msg ("A big mechanism, with a button on the side.")
                }
              </look>
              <take />
              <object name="button">
                <inherit name="editor_object" />
                <scenery />
                <look>A big red button.</look>
                <press type="script">
                  msg ("The machine starts to hum; it feels distictly warm.")
                  machine.switchedon = True
                </press>
              </object>
            </object>
          </object>
          <verb>
            <property>press</property>
            <pattern>press;push</pattern>
            <defaultexpression>"You can't press;push " + object.article + "."</defaultexpression>
          </verb>
          <walkthrough name="test">
            <steps type="list">
              x machine
              x button
              press button
              x machine
            </steps>
          </walkthrough>
        </asl>
