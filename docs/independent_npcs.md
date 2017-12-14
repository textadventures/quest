---
layout: index
title: Making NPCs Act Independently
---

An NPC is going to feel more alive if he or she is doing things on their own initiative, rather than just reacting to what the player does. A simple way to do this is to add something to the room description for the location the NPC is in. Somethingf like this, may:

> You are in a rather grubby lounge, with a tatty settee in the centre. Mary is here, {random:scratching her nose:looking at you expectantly:looking at her phone}.

However, we will look at doing something a bit more involved, allowing the NPCs to move from one room to another, and potentially to interact with objects in each room.

I am going to describe a turn-based system, which means that each time the player does something, the NPCs get a chance to act too, but it could potentially be adapted to atime-based system. In either case, the under-lying system is the same; each NPC will have a script, called "takeaturn", and we run that each time.

A Turnscript
------------

The first thing we need, then, is a turn script (go to the _Scripts_ tab of the game object, and click "Add" in the turn scripts section, at the bottom). Give it a suitable name, and tick the box so it is enabled from the start. The code is very simple; it just goes through every object, and if it has the script, it runs it.

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

So what about the "takeaturn" script? That depends on what you want the NPC to do. Let us look at a few examples. The easiest is to have the NPC patrol a circuit of rooms. For the benefit of uses of the on-line version, we will do this in the initialisation script of the NPC, so you will need to tick that option on the _Features_ tab first.

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

We might want the NPC to pause for a turn. If the player talks to her, then it would make sense that she has done that rather than than go to another room, and the player may get annoyed if he is chasing after her as they have a conversation.

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

Perhaps you want your NPC to follow a series of actions. How would you do that?

The first step is to consider how those steps will be incorporated into the game. What we will do is have each step defined by a string in a list. You might set it up like this:

```
this.actions = NewStringList()
list add (this.actions, "Wait:player")
list add (this.actions, "'Don't tell anyone you saw me,' says Mary.")
list add (this.actions, "Move:hall")
list add (this.actions, "Move:kitchen")
list add (this.actions, "Get:knife")
list add (this.actions, "Move:hall")
list add (this.actions, "Drop:knife")
```

There are two types of entries here; text and commands. Each command consists of the command name, followed by the object name. Each turn we will go to the next item in the list; if it is text, print it, otherwise, do the command.

### The `NpcAct` function

The heart of the system, then, is a function called `NpcAct`, which returns a boolean and has two parameters, "npc" and "s" (where "s" will be the string from the list above). Here is the code:

```
ary = Split(s, ":")
if (ListCount(ary) = 1) {
  PrintIfHere (npc.parent, s)
  f = true
}
else {
  game.currentobj = GetObject (StringListItem(ary, 1))
  if (game.currentobj = null) {
    error ("Failed to find object: " + StringListItem(ary, 1))
  }
  game.currentnpc = npc
  f = Eval("Npc" + StringListItem(ary, 0) + "(game.currentnpc, game.currentobj)")
}
return (f)
```

So what it does is split the string on colons. If there is only one part (i.e., there were no colons), it just prints the text (if the player is present). Otherwise it will try to interpret the command. For that, it will try to find the object specified (and given an error if it fails).

At this point it does something a bit sneaky! It uses the `Eval` function to call a function that has a name starting "Npc" and ending with the name of the command. For "Get" that would be "NpcGet". Why do it that way? The alternative would be a `switch case` (or `if/else`) for each option, which would be a lot longer. Also, this way, you can add a new function without having to change this function, so it is extensible.

Because we are using `Eval` we need to set the variable as attributes of game. We also need to return a value, in this case a Boolean. That will be useful later.

### The "takeaturn" script

We are now ready to give the "takeaturn" script to the NPC.

```
this.takeaturn => {
  if (ListCount(this.actions) > 0) {
    s = StringListItem(this.actions, 0)
    if (NpcAct (this, s)) {
      list remove (this.actions, s)
    }
  }
}
```

What this does is take the first entry in the list of actions and send it to `NpcAct` to handle. If `NpcAct` returns `true`, then that entry is removed from the list, so next time the next entry will get used. If `NpcAct` returns `false` the entry remains and will get used again next time.

### The functions

At this point we need to set up some functions to handle individual commands. They all have to follow certain rules; the name must be "Npc" plus the command, they must return a Boolean, and they must have two parameters, `npc`, and `obj`.

Let us start with `NpcMove`.

```
oldroom = npc.parent
npc.parent = obj
if (not oldroom = npc.parent) {
  PrintIfHere (oldroom, GetDisplayName(npc) + " leaves the room.")
  PrintIfHere (npc.parent, GetDisplayName(npc) + " enters the room.")
}
return (true)
```

This is `NpcGet`:

```
obj.parent = npc
PrintIfHere (npc.parent, GetDisplayName(npc) + " picks up the " + GetDisplayAlias(obj) + ".")
return (true)
```

This is `NpcDrop`:

```
obj.parent = npc.parent
PrintIfHere (npc.parent, GetDisplayName(npc) + " drops the " + GetDisplayAlias(obj) + ".")
return (true)
```

And this is `NpcWait`:

```
if (npc.parent = obj.parent) {
  return (true)
}
else {
  return (false)
}
```


