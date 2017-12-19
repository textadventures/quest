---
layout: index
title: Making NPCs Patrol
---

An NPC is going to feel more alive if he or she is doing things on their own initiative, rather than just reacting to what the player does. A simple way to do this is to add something to the room description for the location the NPC is in. Something like this, maybe:

> You are in a rather grubby lounge, with a tatty settee in the centre. Mary is here, {random:scratching her nose:looking at you expectantly:looking at her phone}.

However, we will look at doing something a bit more involved, allowing the NPCs to move from one room to another (and in a later tutorial, to interact with objects in each room).

I am going to describe a turn-based system, which means that each time the player does something, the NPCs get a chance to act too, but it could potentially be adapted to a time-based system. In either case, the under-lying system is the same; each NPC will have a script, called "takeaturn", and we run that each time.

A Turnscript
------------

The first thing we need, then, is a turn script (go to the _Scripts_ tab of the game object, and click "Add" in the turn scripts section, at the bottom). Give it a suitable name, "NpcTurnScript", say, and tick the box so it is enabled from the start. The code is very simple; it just goes through every object, and if it has the script, it runs it.

```
foreach (o, AllObjects()) {
  if (HasScript(o, "takeaturn")) {
    do(o, "takeaturn")
  }
}
```

This will cause all the NPCs to act, whether the player is there to see it or not. That makes sense, we want it to seem as though they are living independent lives that go on even if the player is not there to see it. However, we only want to tell the player about it if the player _is_ present. As we will be checking that a lot, it is convenient to create a function, `PrintIfHere` (go to "Functions" in the left pane, then click "Add" in the right pane). Give it two parameters, "room" and "s", and no return type. Paste in this code:

```
if (game.pov.parent = room) {
  msg(s)
}
```

The message will only get shown if the player is in that room.

The NPC Script - Patrolling
--------------

So what about the "takeaturn" script? That depends on what you want the NPC to do. Let us look at a few examples. The easiest is to have the NPC patrol a circuit of rooms. For the benefit of uses of the on-line version, we will do this in the initialisation script of the NPC, so you will need to tick that option on the _Features_ tab first. Go to the _Initialisation script_ tab, and paste in this code (change the list of rooms to suit your game):

```
this.route = NewObjectList()
list add (this.route, lounge)
list add (this.route, kitchen)
list add (this.route, hall)

this.takeaturn => {
  oldroom = this.parent
  index = IndexOf(this.route, this.parent)
  newindex = (index + 1) % ListCount(this.route)
  this.parent = ObjectListItem(this.route, newindex)
  if (not oldroom = this.parent) {
    PrintIfHere (oldroom, "Mary leaves the room.")
    PrintIfHere (this.parent, "Mary enters the room.")
  }
}
```

The first four lines set up the route. You can make this as long or as short as you like, and in fact you could add and remove rooms to the route during the game to make the route change. The rest of the code sets up the "takeaturn" script.

First it notes the current room for the NPC, and then it works out what the next room is. The `IndexOf` function gets the position of the current room in the list of rooms, and then we add one to that to get the next one.

We potentially have a problem here, as in three turns the value of newindex would be 3, and we only have three items in the list, numbered 0, 1 and 2. We need to limit `newindex` to those values, and we do that with the modulo operator, `%`; this gives the remainder after division. `ListCount` gives 3, so we are doing the remainder after dividing by 3. 2 divided by 3 is 0, with 2 left over. 3 divided by 3 is 1 with 0 left over. Thus, when `index` is 2, `newindex` will go to 0, and we go back to the start of the list.

Now we have `newindex`, we can get the new room from the list. If the room is different, then we tell the player the NPC is moving, and we can use our `PrintIfHere` function to make the appropriate to where the player is.

Note that `IndexOf` will return -1 if the item is not in the list, so if the NPC is not in any of the rooms in the list, then `index` will get set to -1, and the `index` will get to zero, so the NPC will be moved to the first room in the sequence. This means you can start the NPC in any room you like, not necessarily one on he route.

If a room is added multiple times to the route, the NPC will stay there for a while. For example, we could have the NPC stay in the lounge for a while:

```
this.route = NewObjectList()
list add (this.route, lounge)
list add (this.route, lounge)
list add (this.route, lounge)
list add (this.route, kitchen)
list add (this.route, hall)
```

Pausing
-------

We might want the NPC to pause for a turn. If the player talks to her, then it would make sense that she has done that rather than than go to another room, and the player may get annoyed if he is chasing after her as they have a conversation. To handler that, we need to modify the turn script:

```
foreach (o, AllObjects()) {
  if (HasScript(o, "takeaturn")) {
    if (GetBoolean(o, "paused")) {
      o.paused = false
    }
    else {
      do (o, "takeaturn")
    }
  }
}
```

You do need to set the "paused" to `true` for your NPCs at the relevant points. For example, the script for speaking to the NPC might looik like this:

```
msg ("You chat to Mary for a while.")
this.paused = true
```

Now the player can talk to Mary for any number of consecutive turns, and only when the player does something else will Mary go back to patrolling.

Exploring
----------------

You can set the script on the NPC to do all sorts of things. In this example, the NPC will pick a random, unlocked exit and go that way.

```
this.takeaturn => {
  oldroom = this.parent
  exit = PickOneUnlockedExit (this.parent)
  this.parent = exit.to
  if (not oldroom = this.parent) {
    PrintIfHere (oldroom, "Mary leaves the room, heading " + exit.alias + ".")
    PrintIfHere (this.parent, "Mary enters the room, from " + ReverseDirection (exit.alias) + ".")
  }
}
```

More Complicated...
-------------------

Perhaps you want your NPC to follow a series of actions. If so, read on...
[Independent NPCs](independent_npcs.html)

