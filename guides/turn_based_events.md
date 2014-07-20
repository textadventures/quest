---
layout: index
title: Turn-based events
---

If your game is only responding to what the player does, it feels dead. Bring it alive by having events occur that are not simply reacting to the player's actions (even if they are reacting to what he did four turns ago, it will still feel like it is not).

Here then is a simple framework for turn-based events.

Set up the framework
--------------------

To use it:

1. Set an int attribute on game called "turn".

2. Create two dummy rooms, "active\_events" and "dead\_events". They should have no exits and should not be accessed by the player.

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

By default, events are automatically moved to "dead\_events" and start the next chained event (if set) when they trigger. Setting the "auto" attribute to false stops that behaviour. This may be desirable if you want to wait until the player is in a certain room, for example.

Example game
------------

Right-click the link and select save or something like that.

[Events.aslx]({{site.baseurl}/files/Events.aslx)

There are only two rooms and three events. Event 1 initiates the countdown to event 2, which in turn sets off event 3. Event 2 has "auto" set to false, so it keeps going until a condition is met (player in room 2), and only then starts the countdown to event 3.

Note
----

Quest counts each player input as a turn. If the player spending 10 turns typing commands that are not recognised, that is still 10 turns. Conversely, if the player types five commands in one line (eg "n.get spade.s.s.drop spade") then that is only one turn.
