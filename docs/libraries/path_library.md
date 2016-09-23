---
layout: index
title: Path library
---

Created by jaynabonne
---------------------

[Download](http://textadventures.co.uk/attachment/610)
[Discussion](http://textadventures.co.uk/forum/samples/topic/3214/path-library)

Have you ever wanted to know how to get from point A to point B in a game? Have you ever wondered how far away something is, or which direction to go to find it?

Attached is a simple library (and sample code illustrating) that determines paths between rooms. In its simplest form (GetPath), you pass a start room and an end room, and it will return a list of exits that must be traversed to get from start to end. It returns null if a path can not be found. The code is brute-force, but there is little alternative as there are no requirements for the topology of the rooms in a game.

There is a variant of this (GetRestrictedPath) which also takes a max length for the path. If the path has not been found within that length, it returns null. This allows for localized searches.

(There is also a more extended function - GetPathExt - which takes the list of exits to search, but that is for highly specialized sorts of searches. Yes, I have plans.)

Possible uses: - To determine the path that must be followed, say for a moving NPC or to offer the player directional hints to a target. - To determine how far apart (exit step wise) two items are. - To determine if a room is within a certain distance of the player or whether the player is within range of some room or object.

You could use this to plot a path for an NPC to follow (either to a room or the player).

You could use it for environmental messages (e.g. if an object or room produces sound, you could have different messages for what the user hears depending on how far away it is.)

You could combine them both (e.g. "You hear steps approaching from the north. Run quick!")

The sample code is a simple "find the room" game. There are three levels of help: "off'" provides no directional messages, "gps" gives you the next step in your path (like a gps), and "full" shows all the steps in the path you must take. The message is printed after each turn. It also gives you "warm" and "hot" messages as you get close.
