---
title: Characters that move
description: Make characters follow the player, patrol a route, wander at random or carry out a list of tasks, and tell the player only what they can see
---

A character who stays in one room waiting to be spoken to can feel like part of the furniture. This page shows how to make characters move around on their own:

| You want a character to | See |
|---|---|
| Come with the player wherever they go | [Following the player](#following-the-player) |
| Walk the same circuit of rooms over and over | [Patrolling a route](#patrolling-a-route) |
| Wander about unpredictably | [Wandering at random](#wandering-at-random) |
| Go somewhere and do something, then stop | [Going somewhere specific](#going-somewhere-specific) |

All of them work the same way: each character that moves has a script called `takeaturn`, and a turn script runs it after every turn. Characters keep moving while the player is elsewhere, but the player only hears about it when it happens in front of them.

## Giving characters a turn

First, add a turn script that gives every character with a `takeaturn` script its turn. Select the game in the tree, go to the _Scripts_ tab, and click "Add Turn Script" under "Turn scripts - run after every turn the player takes in this game". Give it a name, such as `npcturns`, tick "Enabled when the game begins", and enter this script:

```quest
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

The `paused` check lets a character stay put for a turn - see [Pausing while the player talks to them](#pausing-while-the-player-talks-to-them).

Then, to make a character move, select them, go to the _Attributes_ tab, add an attribute called `takeaturn` and set its type to "Script". The sections below give scripts to put in it. Inside the script, `this` is the character.

## Moving a character and telling the player

When a character moves, the player should see them leave if they were in the room they left, and see them arrive if they're in the room they arrive in - and otherwise, hear nothing. It's easiest to put this in a function that every character can use.

Select "Functions" in the tree, click "+ Add" and choose "Add Function". Name it `MoveCharacter`, add two parameters, `npc` and `room`, and enter this script:

```quest
oldroom = npc.parent
if (room <> oldroom) {
  exitname = GetExitByLink(oldroom, room)
  if (game.pov.parent = oldroom) {
    if (exitname = null) {
      msg (CapFirst(GetDefiniteName(npc)) + " leaves.")
    }
    else {
      msg (CapFirst(GetDefiniteName(npc)) + " goes " + GetObject(exitname).alias + ".")
    }
  }
  npc.parent = room
  if (game.pov.parent = room) {
    msg (CapFirst(GetDefiniteName(npc)) + " comes in.")
  }
}
```

`GetExitByLink` finds the exit from one room to the other, if there is one, so the player sees "Mary goes north." rather than just "Mary leaves." `GetDefiniteName` gives "Mary" for a named character and "the cat" for anything else.

`MoveCharacter` doesn't check that there is an exit, or that it's unlocked - it simply puts the character in the new room. Change the messages to suit your game; you could give each character their own by storing them in attributes.

## Following the player

For a character who goes wherever the player goes - a dog, say - add a Boolean attribute called `following` on the _Attributes_ tab and tick it, then give the dog this `takeaturn` script:

```quest
if (GetBoolean(this, "following") and this.parent <> game.pov.parent) {
  this.parent = game.pov.parent
  msg (CapFirst(GetDefiniteName(this)) + " follows you.")
}
```

```
> NORTH

You are in a kitchen.
You can see Mary.
You can go south.
The dog follows you.
```

Because turn scripts run at the end of the turn, "The dog follows you." appears after the room description. To stop the dog following, set `dog.following = false` - the dog stays in the room it's in. Set it back to `true` and it catches up at the end of the next turn. You might do this from a "Tell to" topic on the dog's _Ask/Tell_ tab, so that `TELL DOG TO STAY` works (see [Building an Ask/Tell system](/howto/characters/ask-tell)).

For variety, use the [text processor](/howto/text/text-processor) in the message - `{random:The dog follows you.:The dog trots in behind you.:The dog pads after you, tail wagging.}` - and print it with `msg` as usual.

## Patrolling a route

For a character who walks a fixed circuit, list the rooms on their _Attributes_ tab: add an attribute called `route`, set its type to "String List", and add the rooms' names in order - for example `lounge`, `kitchen`, `lounge`, `hall`. Put the character in the first room on the list. Then give them this `takeaturn` script:

```quest
this.patrolstep = (GetInt(this, "patrolstep") + 1) % ListCount(this.route)
MoveCharacter (this, GetObject(StringListItem(this.route, this.patrolstep)))
```

Each turn, the character moves to the next room on the list, going back to the start after the last one. `patrolstep` counts where they've got to - `GetInt` gives 0 if it hasn't been set yet, and `%` wraps it back to 0 at the end of the list.

```
> WAIT
Time passes.
Mary goes north.

> WAIT
Time passes.
Mary comes in.
```

To have the character stay in a room for longer, list it several times in a row. You can change the route during the game with `Split`, as long as every item is a room's name:

```quest
Mary.route = Split("lounge;garden", ";")
```

## Wandering at random

`PickOneUnlockedExit` picks one of a room's exits at random, ignoring any that are locked or invisible. This `takeaturn` script moves a cat through a random exit about half the time:

```quest
if (RandomChance(50)) {
  exit = PickOneUnlockedExit(this.parent)
  if (exit <> null) {
    MoveCharacter (this, exit.to)
  }
}
```

`PickOneUnlockedExit` returns `null` if the room has no unlocked exits, so check before using `exit.to`. Change the 50 to make the character move more or less often.

## Going somewhere specific

Sometimes you want a character to carry out a series of steps and then stop - fetch something, meet the player somewhere, or act out a scene. You can do this by giving them an agenda: a string list of steps, taken one per turn. Give the character a `takeaturn` script like this:

```quest
if (ListCount(this.agenda) > 0) {
  step = StringListItem(this.agenda, 0)
  list remove (this.agenda, step)
  if (GetObject(step) <> null) {
    MoveCharacter (this, GetObject(step))
  }
  else if (HasScript(this, step)) {
    do (this, step)
  }
  else if (this.parent = game.pov.parent) {
    msg (step)
  }
}
```

Each step is one of:

- **the name of a room** - the character goes there
- **the name of a script attribute on the character** - that script runs, so the character can do anything you can write a script for
- **anything else** - it's printed, if the player is there to see it

Add an empty "String List" attribute called `agenda` to start with, and give the character something to do whenever it suits the story. Here, Jeeves's "speak to" verb sends him to the kitchen for a key:

```quest
msg ("'Could you fetch the key, Jeeves?'")
msg ("'Certainly, sir.'")
Jeeves.agenda = Split("kitchen;fetchkey;lounge;'Your key, sir.';givekey", ";")
```

`fetchkey` and `givekey` are scripts on Jeeves's _Attributes_ tab:

```quest
// fetchkey
MoveObject (key, this)
if (this.parent = game.pov.parent) {
  msg ("Jeeves takes the key from its hook.")
}
```

```quest
// givekey
if (this.parent = game.pov.parent) {
  MoveObject (key, game.pov)
  msg ("Jeeves hands you the key.")
}
else {
  MoveObject (key, this.parent)
}
```

If the player has wandered off by the time Jeeves gets back, `givekey` leaves the key where he is. Each step takes a turn, and because the steps are separated by semicolons, text steps can't contain one. List every room on the way, in order: the character moves one room per step, wherever those rooms are. To have a character work out its own route through the map, you need a path-finding function; ThePix's [NpcLib](https://github.com/ThePix/quest/blob/master/NpcLib.aslx) library, written for Quest 5, includes one along with a fuller agenda system.

To stop a character in their tracks, give them an empty agenda: `Jeeves.agenda = NewStringList()`. A script step can also set a new agenda - for example, one that repeats a routine by ending with the step that sets it again.

## Pausing while the player talks to them

It's frustrating to be halfway through a conversation when the other person walks off. How you stop that depends on how the player talks to the character:

- **Pages conversations** pause everyone automatically. Turn scripts don't run while a Pages conversation is going on - unless you tick "Run turn scripts during the dialogue" when you show the page - so no character moves until it's over.
- **Anything else** - a "speak to" verb, an Ask/Tell topic, a menu - can set `paused`, and the turn script above skips the character's next turn:

```quest
msg ("You chat to Mary for a while.")
this.paused = true
```

As long as the player keeps talking to Mary, she stays put. As soon as they do something else, she carries on.

## See also

- [Talking to characters](/howto/characters/talking)
- [Turns and timers](/tutorial/turns-and-timers)
- [Moving objects during the game](/tutorial/moving-objects-during-the-game)
