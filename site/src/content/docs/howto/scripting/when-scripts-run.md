---
title: When scripts run
sidebar:
  order: 10
---

Most of what happens in a game is driven by the player: they type a command, or click a verb, and a script runs. But some scripts are wired to moments in the game rather than to a command — the game starting, the player arriving in a room, a turn ending.

Those scripts live on two tabs, both called _Scripts_: one on the game object, and one on each room. This page explains what each of them does, and in what order they run.

## The game _Scripts_ tab

The _Scripts_ tab of the game object holds the scripts that apply to the whole game.

### Start script

The start script runs once, as the game begins. It is where most games do their setup: giving the player their starting possessions, setting attributes the rest of the game will read, seeding a random value, starting a timer, printing an introduction.

```quest
msg ("You wake up with no memory of the last three days.")
player.hasmap = false
MoveObject (torch, player)
```

The start script runs *before* the first room description is shown, so anything you print here appears above it, and any change you make to the starting room is reflected in the description the player sees.

A few other things happen first, though, and they are worth knowing about:

- the title and author are printed (if "Display title and author when the game begins" is ticked on the _Setup_ tab)
- the player object is worked out, and status attributes and verb lists are initialised

So the start script is the first place *your* code runs — but not the first thing that happens. If you need to change the interface itself before anything is drawn, use the `inituserinterface` script on the [_Advanced Scripts_ tab](/howto/scripting/advanced-game-scripts) instead, which runs earlier still.

Turn scripts do not run during the start script. If you want them to fire once before the player's first command, call `RunTurnScripts` at the end of your start script.

### Script when entering a room

This script runs every time the player enters a room — any room. It is the game-wide counterpart of a room's own "after entering" script, and it is useful for anything that should happen on every move: counting rooms visited, checking whether a pursuer has caught up, updating a status attribute.

```quest
game.roomsvisited = game.roomsvisited + 1
```

It also runs when the player is moved by a script, not just when they walk through an exit, because it is part of the room-entering sequence rather than part of the `GO` command.

### Turn scripts

The list at the bottom of the tab holds the game's [turn scripts](/howto/scripting/using-turnscripts) — scripts that run after every turn the player takes, anywhere in the game. Add one here, and give it a name if you want to be able to enable and disable it during play.

A turn script created here belongs to the game, so it is always in scope. A turn script added to a room's _Scripts_ tab only fires while the player is in that room.

## The room _Scripts_ tab

Every room has its own _Scripts_ tab, holding the scripts that fire as the player arrives and leaves. (Objects that are not rooms have no such tab — there is nothing to enter.)

### Before and after entering

"Before entering the room" runs before the room description is shown, and "after entering the room" runs after it. That distinction is the whole point of having two: use "before" to change the room so that the description comes out right, and "after" to add something that should read as happening once the player has taken the place in.

```quest
// Before entering: make sure the description will mention the fire
if (not fireplace.lit) {
  fireplace.look = "The fireplace is cold and black."
}
```

```quest
// After entering
msg ("Somewhere above you, a floorboard creaks.")
```

### The first-time variants

"Before entering the room for the first time" and "after entering the room for the first time" run only on the player's first visit. Quest Viva tracks this with the room's `visited` attribute, which it sets to true at the end of the first arrival — so you do not need to keep a flag of your own.

These are the natural home for scene-setting that should not repeat:

```quest
msg ("You have never seen so many books in one place.")
IncreaseScore (5)
```

### After leaving

"After leaving the room" runs when the player leaves for somewhere else. Note that it runs on the room being left, at the *start* of the move — before any of the new room's scripts.

### Turn scripts and commands

The two lists at the bottom of the tab hold turn scripts and [commands](/howto/commands/commands-for-room) that belong to this room. Both are scoped to the room: a turn script here only fires while the player is in this room, and a command here is only recognised while the player is in this room. This is how you give one location a verb that would make no sense anywhere else.

### The order of events

When the player moves from one room to another, this is the order things happen in:

1. the old room's "after leaving" script
2. the new room's "before entering for the first time" script, if this is the first visit
3. the new room's "before entering" script
4. the location bar, map and room picture are updated
5. the room description is shown
6. the game object's "script when entering a room"
7. the new room's "after entering for the first time" script, if this is the first visit
8. the new room's "after entering" script
9. the new room is marked as visited

Each script finishes before the next one starts, and a script that stops to ask the player something holds up everything after it — the room description and the remaining scripts all wait until the player has answered.

The two ways of asking differ in where the asking script itself resumes, though. `GetInput()` suspends the script where it stands and carries on from the next statement once the player answers. The `get input` script command does not: it runs the rest of its own script straight away and defers only its callback block until the answer arrives. Neither lets the *next* script in the list start early, but with `get input` the tail of your own script runs before the player has typed anything, which is rarely what you want in a room-entering script.

At the very start of the game there is no old room, so step 1 is skipped, and the whole sequence runs after the game's start script.

## Object initialisation scripts

An object can have a script of its own that runs once as the game starts. Tick "Run an initialisation script for this object" on the object's _Features_ tab, and an _Initialisation script_ tab appears.

Initialisation scripts run just after the game's start script, in turn for every object in the game. They are useful when the setup for an object belongs with the object rather than in one long start script — particularly for objects defined in a [type](/advanced-topics/using-inherited-types) or a [library](/advanced-topics/using-libraries), which have no way to add to the game's start script.

```quest
this.originalparent = this.parent
this.timesmoved = 0
```

Because the game's start script has already finished by this point, an initialisation script can rely on anything it set up. What it cannot rely on is another object's initialisation script having run — the order objects are processed in is not something to depend on.

Initialisation scripts are also run for [clones](/howto/scripting/clones), but only if you create the clone with `CloneObjectAndInitialise`. The other cloning functions copy the script across without running it, which is usually what you want — a clone that appears mid-game rarely needs the same setup as one that existed when the game began.
