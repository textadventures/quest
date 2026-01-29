---
layout: index
title: Turn-based events
---

If your game is only responding to what the player does, it feels dead. Bring it alive by having events occur that are not simply reacting to the player's actions (even if they are reacting to what he did four turns ago, it will still feel like it is not).

Here then is a simple framework for turn-based events.

Set up the framework
--------------------

1. Set an int attribute on game called "turn".

2. Create two dummy rooms, "active\_events" and "dead\_events". They should have no exits and should not be accessible by the player.

3. Copy this turnscript and type into your game code. Insert it at the end, just before the last line (which will be "</asl>")

      <turnscript name="eventhandler">
        <enabled />
        <script><![CDATA[
          game.turn = game.turn + 1
          foreach (evt, GetDirectChildren (active_events)) {
            if (evt.turn <= game.turn) {
              do (evt, "action")
              if (evt.auto) {
                evt.parent = dead_events
                if (HasAttribute (evt, "next")) {
                  evt.next.turn = evt.nextturn + game.turn
                  evt.next.parent = active_events
                }
              }
            }
          }
        ]]></script>
      </turnscript>

      <type name="event_type">
        <done type="boolean">false</done>
        <auto />
        <turn type="int">-1</turn>
        <action type="script">
          msg ("TODO")
        </action>
      </type>

Set up events
-------------

Each event needs to be an object in one of the rooms, "active\_events" and "dead\_events". The room "active\_events" is for events that are counting down, while "dead\_events" is for events that have expired or are waiting in the wings to be used.

For each event, set it to inherit "event\_type". You also need to set the "action" attribute; this is the script that will get run.

For events in "active\_events", you need to set the "turn" attribute. The event will be fired in that number of turns, and will then get moved to the "dead\_events" room.

For events in "dead\_events", you need to set up something to start them off. This needs to move the event into the "active\_events", and to set the "turn" attribute to game.turn plus the number of turns to wait. It might look something like this, when the event, event3, is set to occur in 2 turns:

      event3.turn = 2 + game.turn
      event3.parent = active_events

Chaining events
---------------

You can set up one event to start the count down to another very easily. Just set the "next" attribute to the event to be started, and the "nextturn" attribute to the number of turns to wait.

Turn off auto
-------------

By default, events are automatically moved to "dead\_events" and start the next chained event (if set) when they trigger. Setting the "auto" attribute to false stops that behaviour. This may be desirable if you want to wait until the player is in a certain room, for example. In that case, turn off auto, and the event will fire every turn. Each time it fires, you can have it test to see if the player is in the room; if she is, perform the special action, and then move the event to dead\_events in the script.

Note
----

Quest counts each player input as a turn. If the player spending 10 turns typing commands that are not recognised, that is still 10 turns.


Example game
------------

There are only two rooms and three events. Event 1 initiates the countdown to event 2, which in turn sets off event 3. Event 2 has "auto" set to false, so it keeps going until a condition is met (player in room 2), and only then starts the countdown to event 3.

    <!--Saved by Quest 5.4.4873.16527-->
    <asl version="540">
      <include ref="English.aslx" />
      <include ref="Core.aslx" />
      <dynamictemplate name="ObjectNotOpen">DoObjectNotOpen (object)</dynamictemplate>
      <game name="events">
        <gameid>17fa10af-9205-4eba-a5ad-65d3166864e7</gameid>
        <version>1.0</version>
        <firstpublished>2013</firstpublished>
        <turn type="int">0</turn>
        <statusattributes type="stringdictionary">
          <item>
            <key>turn</key>
            <value></value>
          </item>
        </statusattributes>
        <subtitle>A demonstration of simple turn-based events</subtitle>
        <author>The Pixie</author>
      </game>
      <object name="room">
        <inherit name="editor_room" />
        <alias>First Room</alias>
        <usedefaultprefix type="boolean">false</usedefaultprefix>
        <object name="player">
          <inherit name="editor_object" />
          <inherit name="editor_player" />
        </object>
        <exit alias="west" to="room2">
          <inherit name="westdirection" />
        </exit>
      </object>
      <object name="room2">
        <inherit name="editor_room" />
        <alias>Second Room</alias>
        <usedefaultprefix type="boolean">false</usedefaultprefix>
        <exit alias="east" to="room">
          <inherit name="eastdirection" />
        </exit>
      </object>
      <object name="active_events">
        <inherit name="editor_room" />
        <object name="event1">
          <inherit name="editor_object" />
          <inherit name="event_type" />
          <turn type="int">3</turn>
          <nextturn type="int">2</nextturn>
          <next type="object">event2</next>
          <action type="script">
            msg ("Event 1")
          </action>
        </object>
      </object>
      <turnscript name="eventhandler">
        <enabled />
        <script><![CDATA[
          game.turn = game.turn + 1
          foreach (evt, GetDirectChildren (active_events)) {
            if (evt.turn <= game.turn) {
              do (evt, "action")
              if (evt.auto) {
                evt.parent = dead_events
                if (HasAttribute (evt, "next")) {
                  evt.next.turn = evt.nextturn + game.turn
                  evt.next.parent = active_events
                }
              }
            }
          }
        ]]></script>
      </turnscript>
      <object name="dead_events">
        <inherit name="editor_room" />
        <object name="event2">
          <inherit name="editor_object" />
          <inherit name="event_type" />
          <auto type="boolean">false</auto>
          <action type="script">
            if (player.parent = room2) {
              msg ("Event 2")
              event3.turn = 2 + game.turn
              event3.parent = active_events
              this.parent = dead_events
            }
            else {
              msg ("... waiting")
            }
          </action>
        </object>
        <object name="event3">
          <inherit name="editor_object" />
          <inherit name="event_type" />
          <action type="script">
            msg ("Event 3")
          </action>
        </object>
      </object>
      <type name="event_type">
        <done type="boolean">false</done>
        <auto />
        <turn type="int">-1</turn>
        <action type="script">
          msg ("TODO")
        </action>
      </type>
    </asl>