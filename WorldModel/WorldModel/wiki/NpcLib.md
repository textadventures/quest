An NPC is going to feel more alive if he or she is doing things on their own initiative, rather than just reacting to what the player does. The Quest documentation describes how to do this, building a system from scratch, on this page:

[Making NPCs Act Independently](http://docs.textadventures.co.uk/quest/independent_npcs.html)

However, there is a library that has done all that for you - and more besides.

[Library](https://raw.githubusercontent.com/ThePix/quest/master/NpcLib.aslx)

Once you have added the library to your game, you will see a new tab will appear for objects, allowing you to set an object to be an NPC or a group.


Giving an NPC instructions
-------------------------

To have your NPC do something, go to the _NPCs_ tab, and add actions to the list at the top. You can add two types of things to the list - plain text and commands.

Plain text will just get printed on the screen, if the player is in the same room as the NPC. Use this for actions that do not affect the game world, such as the NPC reading a book or waving at the player. Note that the text must not contain a colon.

Commands must be given in a specific format; the name of the command, a colon, an object. Some commands do not need objects, but the system will expect you to supply one anyway - just use player.

Here is a simple example, in which the NPC, Mary, first moves to the kitchen, then we have some plain text, then she picks up an object.

```
Move:kitchen
Mary is looking for something
Get:knife
```

Note that each action takes one turn (some can take several turns), and will be done whether the player is there to see it or not (but if the player is not there, he will not know it happened).


Groups
------

An NPC can belong to one or zero groups. Membership is indicated by the "group" attribute. Use the `GroupMembers(group)` function to get a list of members, or `Member(group)` to get a single, random, member. The group alias is the members of the group (use its "resetalias" script to set; the built-in functions do this for you).

A group has actions just as an NPC does (in fact the group type inherits from the NPC type). When an NPC is in a group, her actions are not done, but will resume if she leaves the group.

An NPC can "Join" a group, or a group can "Include" an NPC. Either way, the group object will be moved to the location of the NPC, but any members will not, so you should ensure the NPC is in the same room when joining a group with other members.

When the group moves via an action, all members do too.

WaitFor will make a group wait until the specified NPC joins the group. Count will wait until the number of members in the group is equal to the "count" attribute of the given object (which may well be the group itself).


Commands
--------

Commands with a * will (or may) be repeated, until some condition is met or indefinitely; others will be for one turn only.

### For NPCs only

Get, Drop, Wear, Remove: The NPC will get, drop, wear or remove the given object (wherever it might be).

Join: The NPC will join the given group.

### For groups only

Include, Exclude: The given NPC will become a part or will leave the group.

Disband: All members of the group will stop being a part of the group.

WaitFor: * Do nothing until the given NPC is a part of the group.

Count: * Do nothing until the number of members in the group is equal to the "count" attribute of the given object.

### Groups and NPCs

Open, Close, Lock, Unlock, Give: The given object is opened, closed, locked, unlocked or given to the player. If this is for a group, a random member of the group is chosen for the message.

Move: The NPC or group is moved directly to the given location.

Search: * The NPC or group is moved to an adjacent room via a random exit. This will continue until the NPC or group is in the same room as the given object.

Pause: The NPC or group does nothing for one turn.

Wait: * The NPC or group does nothing until the given object is in the same room (if player or NPC/Group), or is unlocked (has "locked" attribute) or has the object.

GoTo: * The NPC or group moves to the given location, via available exits, one room per turn.

GoToParent: * The NPC or group moves to the parent location of the given object, via available exits, one room per turn.

Follow: * The NPC or group follows the given object. If it has a "followidle" attribute, this will be used to provide idling messages when the given object does not move; this can be a string list or a string with phrases separated by semi-colons. Use the `NpcStopFollowing(npc)` function to stop the NPC or group following.

Script: * Runs the NPC or group's "npcscript", which allows you to have the NPC do anything! You can set this from the bottom of the _NPCs_ tab. If this sets the "deletefromlist" attribute of the NPC or group to false, then this command will be used next turn.



Useful Functions
---------------

Use the `Pause(npc)` function to pause an NPC for one turn; this will also ensure the whole group pauses if appropriate. ConvLib will do this for you so an NPC will pause whilst talking to the player.

The `PrintIfHere(location, msg)` function will print a message only if the player is in the given room. Whever an NPC does something this can be used to tell the player - but only if the player is presentto see it.

The `PrintIfDebug(npc, msg)` function will print a message in blue if `game.npcdebug` is set to true, and not otherwise. There is plenty of scope for confusion here with NPCs doing stuff in the background without you knowing exactly what, and this is a useful tool for keeping you informed what they are up to. Some built-in debugging will also appear. Remember to set `game.npcdebug` to false before releasing!



Customised Movement Message
-------------------

You can override the `NpcLeaving` and `NpcEntering` functions to have your own text when NPCs are moving around. However, some exits may benefit from more descriptive text. You can given them special scripts to describe the NPC coming and going with them; use `npc` as a local variable to refer to the group or NPC. Suppose we have an attic with a ladder going up to it. The "up" exit could have these scripts:

"npc_using"

```
msg (npc.alias + " " + Conjugate(npc, "head") + " up the ladder.")
```

"npc_entering_by"

```
msg (npc.alias + " " + Conjugate(npc, "climb") + " down the ladder.")
```

We use the `Conjugate` function here so the player will see "climbs" for an NPC or "climb" for a group. Note that while the player uses an exit only to leave a room, the NPC can also use it to enter the room. The exit down from the attic ight have these scripts:

"npc_using"

```
msg (npc.alias + " " + Conjugate(npc, "head") + " back down the ladder.")
```

"npc_entering_by"

```
msg (npc.alias + " " + Conjugate(npc, "climb") + " up the ladder, and stands up in the attic.")
```






So what can you do with this?
-----------------------------

By way of an example, let us suppose Mary and Bob are breaking into a house together, taking two things, then leaving together. Suppose we start with Mary waiting for Bob on the street corner. Start her in a group by setting her "group" attribute. The group could then "WaitFor" Bob, to "Join" the group.

The group could then go to the hall location inside the house, at which point it could "Disband" (disband does not need an object, so just send it player as a kind of default). Each NPC will then do their own thing, Mary going to the kitchen to get a knife, Bob to the attic to get a painting, then come back and join the group. The group, with "count" set to 2, waits for its membership Count to get to 2, at which point the group heads back to the street corner, to await the player, who they then follow.

Here is how it is set up:

Mary (starts in the street corner location, with her "group" attribute set to the Mary and Bob group object).

```
Move:kitchen
Get:knife
Move:hall
Join:Mary and Bob group
```

Bob

```
GoTo:street corner
Join:Mary and Bob group
GoTo:attic
Get:painting
GoTo:hall
Join:Mary and Bob group
```

Mary and Bob group (has its "count" attribute set to 2)

```
GoTo:hall
Disband:player
Count:Mary and Bob group
GoTo:street corner
Wait:player
Follow:player
```