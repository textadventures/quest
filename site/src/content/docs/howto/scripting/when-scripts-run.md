---
title: When scripts run
description: Every script that runs by itself - at the start of the game, on entering or leaving a room, every turn - and the order they run in
sidebar:
  order: 10
---

Most of what happens in a game is driven by the player: they type a command, or click a verb, and a script runs. But some scripts are wired to moments in the game rather than to a command - the game starting, the player arriving in a room, a turn ending.

This page is the map of those scripts: where each one lives, when it fires, and what it is good for.

| When you want something to happen | Use |
|---|---|
| Once, as the game begins | The game's [start script](#start-script) |
| Once, as the game begins, but set up per object | An [object initialisation script](#object-initialisation-scripts) |
| Before anything is drawn, and again when a saved game is loaded | [`inituserinterface`](/howto/scripting/advanced-game-scripts) |
| Whenever the player enters any room | The game's ["script when entering a room"](#script-when-entering-a-room) |
| Whenever the player enters or leaves one particular room | That room's [_Scripts_ tab](#the-room-scripts-tab) |
| Only the first time the player enters a room | That room's [first-time scripts](#the-first-time-variants) |
| After every turn | A [turn script](/howto/scripting/using-turnscripts) |
| A set number of turns or seconds from now | A [timer or turn timeout](/howto/scripting/using-turnscripts) |
| Whenever a particular attribute changes | A [change script](/change-scripts) |
| When Quest Viva can't make sense of what the player typed | [`unresolvedcommandhandler`](/howto/scripting/advanced-game-scripts) |

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

1. the `inituserinterface` script on the [_Advanced Scripts_ tab](#the-game-advanced-scripts-tab), if the game has one - this is the earliest your own code can run, before anything at all is drawn
2. the title and author are printed (if "Display title and author when the game begins" is ticked on the _Setup_ tab)
3. the player object is worked out, and status attributes and verb lists are initialised

Then the start script runs, followed by every object's [initialisation script](#object-initialisation-scripts), and then the starting room's entry sequence.

Turn scripts do not run during the start script. If you want them to fire once before the player's first command, call `RunTurnScripts` at the end of your start script.

### Script when entering a room

This script runs every time the player enters a room - any room. It is the game-wide counterpart of a room's own "after entering" script, and it is useful for anything that should happen on every move: counting rooms visited, checking whether a pursuer has caught up, updating a status attribute.

```quest
game.roomsvisited = game.roomsvisited + 1
```

It also runs when the player is moved by a script, not just when they walk through an exit, because it is part of the room-entering sequence rather than part of the `GO` command.

### Turn scripts

The list at the bottom of the tab holds the game's [turn scripts](/howto/scripting/using-turnscripts) - scripts that run after every turn the player takes, anywhere in the game. Add one here, and give it a name if you want to be able to enable and disable it during play.

A turn script created here belongs to the game, so it is always in scope. A turn script added to a room's _Scripts_ tab only fires while the player is in that room.

Turn scripts run at the very end of a turn, after everything else - including, when the player has moved, after all of the new room's entry scripts. They do not run after a command Quest Viva did not understand; the [Time, turns and timers](/howto/scripting/using-turnscripts#what-counts-as-a-turn) page lists everything else that does not count as a turn.

## The game _Advanced Scripts_ tab

Tick "Show advanced scripts for the game object" on the game's _Features_ tab and a second tab appears, holding three scripts that are not tied to a moment in the story:

- `inituserinterface`, which runs before anything is drawn, and again every time a saved game is loaded
- `unresolvedcommandhandler`, which runs when the player types something Quest Viva cannot match
- `scopebackdrop`, which adds objects to what commands can see

See [Advanced game scripts](/howto/scripting/advanced-game-scripts) for what each one does.

## The room _Scripts_ tab

Every room has its own _Scripts_ tab, holding the scripts that fire as the player arrives and leaves. (Objects that are not rooms have no such tab - there is nothing to enter.)

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

"Before entering the room for the first time" and "after entering the room for the first time" run only on the player's first visit. Quest Viva tracks this with the room's `visited` attribute, which it sets to true at the end of the first arrival - so you do not need to keep a flag of your own.

These are the natural home for scene-setting that should not repeat:

```quest
msg ("You have never seen so many books in one place.")
IncreaseScore (5)
```

### After leaving

"After leaving the room" runs when the player leaves for somewhere else. Note that it runs on the room being left, at the *start* of the move - before any of the new room's scripts.

### Turn scripts and commands

The two lists at the bottom of the tab hold turn scripts and [commands](/howto/commands/commands#commands-for-one-room) that belong to this room. Both are scoped to the room: a turn script here only fires while the player is in this room, and a command here is only recognised while the player is in this room. This is how you give one location a verb that would make no sense anywhere else.

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
10. the turn scripts that are in scope

Each script finishes before the next one starts. A script that stops to ask the player something - with `GetInput()`, `Ask()` or `ShowMenu()` - suspends where it stands and holds up everything after it: the room description and the remaining scripts all wait until the player has answered, then the sequence carries on from where it left off.

At the very start of the game there is no old room, so step 1 is skipped, step 10 does not happen at all, and the whole sequence runs after the game's start script and the object initialisation scripts.

## Object initialisation scripts

An object can have a script of its own that runs once as the game starts. Tick "Run an initialisation script for this object" on the object's _Features_ tab, and an _Initialisation script_ tab appears.

Initialisation scripts run just after the game's start script, in turn for every object in the game. They are useful when the setup for an object belongs with the object rather than in one long start script - particularly for objects defined in a [type](/advanced-topics/using-inherited-types) or a [library](/advanced-topics/using-libraries), which have no way to add to the game's start script.

```quest
this.originalparent = this.parent
this.timesmoved = 0
```

Because the game's start script has already finished by this point, an initialisation script can rely on anything it set up. What it cannot rely on is another object's initialisation script having run - the order objects are processed in is not something to depend on.

Initialisation scripts are also run for [clones](/howto/scripting/clones), but only if you create the clone with `CloneObjectAndInitialise`. The other cloning functions copy the script across without running it, which is usually what you want - a clone that appears mid-game rarely needs the same setup as one that existed when the game began.

## Scripts attached to an attribute

A [change script](/change-scripts) runs whenever a particular attribute changes, wherever in the game the change came from. An attribute called `changedhits` on the player runs every time `player.hits` is set, whether that was a trap, a poison or a fight - so the "are they dead yet?" check only has to be written once, instead of after every line that could hurt them.

Add one from the object's _Attributes_ tab: select the attribute and click the "Add Change Script" button.

## Timers and delayed events

Turn scripts cover "every turn". For "in three turns' time", "in ten seconds' time" or "every twenty seconds", see [Time, turns and timers](/howto/scripting/using-turnscripts), which covers turn scripts, `SetTurnTimeout`, timers and `SetTimeout`, and what does and does not count as a turn.

## See also

- [Time, turns and timers](/howto/scripting/using-turnscripts)
- [Advanced game scripts](/howto/scripting/advanced-game-scripts)
- [Change scripts](/change-scripts)
- [Asking the player](/howto/scripting/asking-the-player), for what happens when a script stops half-way to wait for an answer
