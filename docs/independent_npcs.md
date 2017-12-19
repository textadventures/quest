---
layout: index
title: Making NPCs Act Independently
---

An NPC is going to feel more alive if he or she is doing things on their own initiative, rather than just reacting to what the player does. 

_NOTE:_ This page builds on the page about [patrolling NPCs](patrolling_npcs.html), and you should read that first. In particular, you should add a turn script to your game as described on that page (note that the script is updated in the pausing section) and a `PrintIfHere` function.

_NOTE:_ For desktop users there is a library available that adds the turn script and all the functions to your game.

We have already discussed how to have an NPC explore or patrol, but perhaps you want your NPC to follow a series of actions. How would you do that?

The first step is to consider how those steps will be incorporated into the game. What we will do is have each step defined by a string in a list. You might set it up like this on the initialisation script for the NPC:

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

This is `NpcWait` (NPC will wait until the given object is present if it is the player or an NPC, the object is unlocked if it has "locked" attribute, or trhe object in held by the NPC otherwise):

```
if (obj = game.pov or HasScript(obj, "takeaturn")) {
  return (npc.parent = obj.parent)
}
else if (HasBoolean(obj, "locked")) {
  return (not obj.locked)
}
else {
  return (npc = obj.parent)
}
```

NpcGive:

```
obj.parent = game.pov
PrintIfHere (npc.parent, GetDisplayName(npc) + " gives you the " + GetDisplayAlias(obj) + ".")
return (true)
```

NpcSearch (NPC keeps moving randomly until in the same room as the object):

```
exit = PickOneUnlockedExit (npc.parent)
oldroom = npc.parent
NpcMove(npc, exit.to)
return (obj.parent = npc.parent)
```

NpcPause (NPC does nothing for 1 turn):

```
return (true)
```

Dynamic NPCs
------------

So far we have NPCs they act according to their own agenda, but do not react to what the player does. How do we address that? It is simply a case of giving the NPC a new string list.

The simplest case is where the player has killed the NPC, so now the NPC does nothing. Just set the "actions" attribute to an empty list:

```
mary.actions = NewStringList()
```

However, you could give the NPC a new agenda. say when the player talks to her:

```
msg("'Hi,' says Mary.")
msg("'Could you get the key for me?'")
msg("'Sure!'")
mary.actions = Split("Move:Apple Street;Move:Gate house;Get:gate key;Move:Gate house;Move:Apple Street;Wait:player", ";")
```

You could even give the player some options that he could ask:

```
msg("'Hi,' says Mary.")
ShowMenu("Talk about?", Split("Get the key;Meet in the shop;Wait here", ";"), false) {
  if (result = "Get the key") {
    msg("'Could you get the key for me?'")
    msg("'Sure!'")
    mary.actions = Split("Move:Apple Street;Move:Gate house;Get:gate key;Move:Gate house;Move:Apple Street;Wait:player", ";")
  }
  if (result = "Meet in the shop") {
    msg("'I'll meet you in the shop.'")
    msg("'Sure!'")
    mary.actions = Split("Move:Apple Street;Move:Potion shop;Wait:player", ";")
  }
  if (result = "Wait here") {
    msg("'Just wait here for me?'")
    msg("'Sure!'")
    mary.actions = Split("Pause:player;Pause:player;Pause:player;Pause:player;Pause:player;Wait:player", ";")
  }
}
```

In the third option, note how the NPC will pause for five turns, hopefully long enough that the player will go away, and not enough time to have come back without doing whatever. This may not be ideal, so be aware of the limitations.



Path finding
------------

If our NPCs are acting dynamically, it may not be possible to know in advance where they will be at the start of their agenda. In the example above, perhaps the player could talk to Mary when she at any number of locations, so how can we give Mary a route?

Let her work it out for herself!

Jay Nabonne write a path-finding library we can use.
http://docs.textadventures.co.uk/quest/libraries/path_library.html

We will copy two functions from there. Create a new function, name it "PathLib_GetPathExt", give it these parameters: "start", "end", "exits", "maxlength" (in that order), and set it to return an object list. Here is the code:

```
// From PathLib by Jay Nabonne
// It is more efficient to mark the rooms rather than track them in lists.
if (not HasInt(game, "pathID")) {
  game.pathID = 0
}
// Bump the path ID for this path. This saves us from having to unmark all previously marked rooms.
game.pathID = game.pathID + 1

path = null
current = NewList()
entry = PathLib_AddEntry(current, start)
dictionary add(entry, "path", NewObjectList())
length = 0
iterations = 0
while (ListCount(current) <> 0 and path = null and (maxlength = -1 or length <= maxlength)) {
  iterations = iterations + 1
  entry = current[0]
  list remove(current, entry)
  room = entry["room"]
  room.pathlib_visited = game.pathID
  if (room = end) {
    path = entry["path"]
  } else {
    foreach (exit, exits) {
      toRoom = exit.to
      if (toRoom <> null) {
        if (exit.parent = room and not GetBoolean(exit, "excludeFromPaths")) {
          // This is a room to be investigated.
          if (GetInt(toRoom, "pathlib_current") <> game.pathID and GetInt(toRoom, "pathlib_visited") <> game.pathID) {
            // We have not touched this room yet. Add its exit to the list.
            newEntry = PathLib_AddEntry(current, toRoom)
            // Assign to an object attribute to force a copy.
            game.PathLib_pathtemp = entry["path"]
            list add(game.PathLib_pathtemp, exit)
            dictionary add(newEntry, "path", game.PathLib_pathtemp)
            game.PathLib_pathtemp = null
          }
        }
      }
    }
  }
  length = ListCount(entry["path"])
}
return (path)
```

Then add a function called "PathLib_AddEntry", with the parameters parameters="list" and "room", and return type dictionary. Paste in the code:

```
// From PathLib by Jay Nabonne
entry = NewDictionary()
dictionary add(entry, "room", room)
list add(list, entry)
room.pathlib_current = game.pathID
return (entry)
```


Now we can add a GoTo command. Create another function, NpcGoTo, and as usual give it two parameters, "npc" and "obj" and have it return a Boolean. Here is the code:

```
l = PathLib_GetPathExt(npc.parent, obj, AllExits(), -1)
if (ListCount(l) = 0) {
  return (true)
}
exit = ObjectListItem(l, 0)
NpcMove (npc, exit.to)
return (ListCount(l) = 1)
```

If the NPC is already in the room, nothing will happen this turn. Otherwise, the NPC will move one room closer to the destination. If that takes the NPC to the destination, this will return `true` and that will cause this entry to be removed from the list, otherwise it returns `false` so the entry will still be on the list next turn.

Note that it calls `NpcMove` to actually move the player; this means if you change the text there, it will still be used.

Now we can do something like this, giving the NPC a destination, rather than a route:

```
msg("'Hi,' says Mary.")
msg("'Could you get the key for me?'")
msg("'Sure!'")
mary.actions = Split("GoTo:Gate house;Get:gate key;GoTo:Apple Street;Wait:player", ";")
```


Reactive NPCs
-------------

We can also have NPCs that react to what is going on. First we will add a new function, NpcScript, with the same set up as before. Here is the script:

```
npc.deletefromlist = true
if (HasScript (npc, "npcscript")) {
  d = NewDictionary()
  dictionary add (d, "item", obj)
  do (npc, "npcscript", d)
}
return (npc.deletefromlist)
```

Then you can give the NPC a script called "npcscript", and have that do, well, anything you like.

For example, you could use this to have your NPC make a decision. Give the NPC a list of instructions that includes "Script:player". When it gets to that point, the "npcscript" will run, and it could check if a certain condition has been met (perhaps the player is present, or a door is open), and if so, it could give the NPC a new list of actions.

Here is a simple example that extends a patrol route if a gateway is open:

```
if (gateway.isopen) {
  this.actions = Split("Move:Square;Move:Apple Street;Move:Gate house;Move:courtyard:Move:Gate houseScript:player", ";")
}
else {
  this.actions = Split("Move:Square;Move:Apple Street;Move:Gate house;Script:player", ";")
}
```

You can set the "deletefromlist" attribute of an NPC to have the script repeat.


### Coordinating NPCs

Bear in mind that NPCs can get delayed, say if the player talks to them, and if you want to coordinate NPCs, the best way is to have each one wait until the other is present. For example, if you want Mary to give Bob a hat, have Mary go to the rendez-vous, and then wait for Bob, and likewise have Bob go there and wait for Mary. For Bob, you can just use the Wait command ("Wait:mary") and then pause one turn ("Pause:player"), but we could have a script on Mary that has her wait until Bob is there, and when he is, give the hat to him.

```
if (this.parent = bob.parent) {
  hat.parent = bob
  PrintIfHere(this.parent, "Mary hands the hat to Bob.")
}
else {
  this.deletefromlist = false
}
```


### Goals and agendas

The script option gives the potential for NPCs to have goals that they will seek to achieve. The script could potentially select a goal, and then set the "actions" attribute accordingly.


Desktop Users
-------------

All the above is available in a library for desktop users. You will find there is a new tab where you can set an object to be an NPC, and then you can add commands to a list to have the NPC do anything you like.

[NpcLib.aslx](https://raw.githubusercontent.com/ThePix/quest/master/NpcLib.aslx)

