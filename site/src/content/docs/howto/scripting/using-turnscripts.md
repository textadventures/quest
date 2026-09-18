---
title: Time, turns and timers
description: Run scripts every turn, a number of turns later, or after a number of real seconds, and keep a turn counter or game clock
---

A game that only ever reacts to the player can feel static. This page shows how to make things happen by themselves: a guard who comes back three turns after leaving, hunger that grows every turn, a clock that moves on as the player acts, or a bee that buzzes past every 20 seconds.

Quest Viva measures time in two ways - in turns, which pass when the player does something, and in real seconds, which pass whatever the player is doing:

| You want | Use | In the editor |
|---|---|---|
| Something to happen every turn | A turn script | "Add Turn Script", or the turn scripts list on a _Scripts_ tab |
| Something to happen once, a number of turns from now | `SetTurnTimeout` or `SetTurnTimeoutID` | "Run script after a number of turns", in the Turn Scripts category |
| Something to happen every few seconds of real time | A timer | "Add Timer" |
| Something to happen once, a number of seconds from now | `SetTimeout` or `SetTimeoutID` | "Run script after a number of seconds", in the Timers category |

Turns are usually the better choice. A player who stops to think, reads slowly, or uses a screen reader loses nothing, and the game behaves the same way every time it's played.

## Turn scripts

A turn script runs at the end of every turn, after the player's command has been handled. Hunger, a burning candle, the weather and other characters taking their turn are all jobs for a turn script.

Where you create a turn script decides when it runs:

- **For the whole game**, add it to the list at the bottom of the game's _Scripts_ tab - "Turn scripts - run after every turn the player takes in this game". It runs every turn, wherever the player is.
- **For one room**, add it to the list at the bottom of that room's _Scripts_ tab. It only runs while the player is in that room. If a turn script never seems to run, check it hasn't ended up inside a room by mistake.

The turn script editor has three settings:

- **Name** - optional, but you need one to switch the turn script on and off from a script.
- **Enabled when the game begins** - turn scripts start switched off unless you tick this.
- **Script** - what to do each turn.

To switch a named turn script on or off during play, use "Enable turn script" and "Disable turn script" from the Turn Scripts category:

```quest
EnableTurnScript (hungerturnscript)
DisableTurnScript (hungerturnscript)
```

When several turn scripts are enabled, they run in alphabetical order of their names.

Turn scripts first run at the end of the player's first turn, not when the game starts. If you need them to run once before the player types anything, call `RunTurnScripts` at the end of the game's start script - but that runs every enabled turn script, not just the one you're thinking of.

### What counts as a turn

Most commands count as a turn, including LOOK, INVENTORY and WAIT. These don't, and turn scripts don't run after them:

- A command Quest Viva doesn't understand ("I don't understand your command.")
- A command naming something that isn't there ("I can't see that.")
- HELP, SAVE, UNDO, OOPS, VERSION, RESTART and the transcript commands
- A comment - anything starting with `*`

UNDO also takes back everything the previous turn changed, including any counter a turn script increased.

In a [Pages](/tutorial/using-pages) dialogue, turn scripts don't run at all by default - not for the command that starts the conversation, and not for any of the choices. Tick "Run turn scripts during the dialogue" on the "Show page" script command (the third parameter of `ShowPage`) to make every choice count as a turn.

### Commands that shouldn't take a turn

If you add your own command that shouldn't count as a turn - a HINTS or CREDITS command, say - add "Suppress turn scripts" from the Turn Scripts category to its script:

```quest
msg ("This game was written by...")
SuppressTurnscripts
```

This only affects the turn you call it in.

## Something happening a number of turns later

To make something happen once, a set number of turns from now, use "Run script after a number of turns". Here, the player lights a fuse:

```quest
msg ("You light the fuse.")
SetTurnTimeout (3) {
  msg ("BANG!")
}
```

When you call `SetTurnTimeout` from a command or verb, that turn counts as the first of the three, so the bang comes at the end of the player's second command after lighting the fuse. `SetTurnTimeout (1)` runs the script at the end of the current turn. Turns that don't count - see above - don't count towards this either.

Behind the scenes, `SetTurnTimeout` creates a turn script that counts down, runs your script and then deletes itself. To chain events - one thing, then another two turns later - call `SetTurnTimeout (2)` again inside the script. Called from inside a turn script like this, the count starts from the next turn.

### Cancelling it

To be able to cancel the event, give it a name with "Run script after a number of turns (with name)":

```quest
msg ("The guard leaves, saying he'll be back soon.")
SetTurnTimeoutID (3, "guardreturns") {
  msg ("The guard comes back.")
}
```

The name becomes the name of the turn script, so you can check whether the event is still waiting with `GetObject`, and cancel it by destroying it with "Destroy object" from the Objects category:

```quest
if (GetObject("guardreturns") <> null) {
  destroy ("guardreturns")
}
```

Destroy it rather than disabling it. A disabled turn script still exists, so calling `SetTurnTimeoutID` with the same name again stops with an error saying there is already an object with that name. The name must also not be used by any other object in the game.

## Counting turns

Quest Viva doesn't keep a turn count for you, but a turn script can. On the game's _Attributes_ tab, add an integer attribute called `turns`, then add a game turn script, tick "Enabled when the game begins", and give it this script:

```quest
game.turns = game.turns + 1
```

Add `turns` to the game's [status attributes](/status-attributes) to show it to the player. Because it's a turn script, it only counts turns that really are turns.

## A game clock

A clock is a turn counter that also works out the time of day. Here, each turn is one minute, and `game.clock` holds the number of minutes since midnight.

On the game's _Attributes_ tab, add an integer attribute called `clock` set to 600 (10 am - 10 × 60), and a string attribute called `timeofday` set to "10:00 am". Then add a function called `ClockTime`, with one parameter, `clock`, and "Return type" set to string:

```quest
hours = (clock / 60) % 24
minutes = clock % 60
if (hours < 12) {
  suffix = "am"
}
else {
  suffix = "pm"
}
hours = hours % 12
if (hours = 0) {
  hours = 12
}
return (hours + ":" + Right("0" + minutes, 2) + " " + suffix)
```

This gives times like "10:05 am", "12:30 pm" and "12:00 am" for midnight. `Right("0" + minutes, 2)` makes sure the minutes always have two digits.

Now add an enabled game turn script:

```quest
game.clock = game.clock + 1
game.timeofday = ClockTime(game.clock)
```

Add `timeofday` to the game's status attributes with the format "Time: !" to show the clock, or use `{game.timeofday}` in any text.

Some actions take longer than others. To make mending the car take ten minutes, add the extra time in its script - the turn script adds the last minute:

```quest
msg ("You spend ten minutes mending the car.")
game.clock = game.clock + 9
```

To stop a command taking any time at all, use `SuppressTurnscripts` as described above.

## Real-time timers

A timer runs a script after a number of real seconds, whatever the player is doing. Add one with "Add Timer". The timer editor has:

- **Start timer when the game begins** - leave this unticked to start the timer from a script instead.
- **Interval (sec)** - how many seconds between each run.
- **Script** - what to do each time.

A timer keeps running its script every interval until you stop it. Use "Enable timer" and "Disable timer" from the Timers category:

```quest
EnableTimer (beetimer)
DisableTimer (beetimer)
```

Enabling a timer starts its interval again from that moment. A timer running doesn't count as a turn, so it doesn't run turn scripts.

A timer runs wherever the player is. If its message only makes sense in one room, check where the player is in its script:

```quest
if (game.pov.parent = kitchen) {
  msg ("The bee buzzes past you.")
}
```

### A time-limited puzzle

To run a script once after a number of seconds, use "Run script after a number of seconds". Here, opening a cupboard wakes a hungry alien, and the player has ten seconds to deal with it. On the cupboard's _Container_ tab, add this to "After opening the object":

```quest
msg ("A hungry alien wakes up and stares at you.")
SetTimeoutID (10, "alienattack") {
  if (ListContains(ScopeVisible(), alien)) {
    msg ("The alien leaps at you. It really was very hungry.")
    finish
  }
}
```

Then give the player a way to win. On the flame thrower's _Use/Give_ tab, under "Use this on (other object)", add a script for the alien that prints a message and removes it with `RemoveObject (alien)`. When the ten seconds are up, the script checks whether the alien is still there, so a player who got rid of it - or ran away - survives.

Like `SetTurnTimeoutID`, `SetTimeoutID` names the timer it creates, so you can cancel it before it runs with `destroy ("alienattack")`, and check whether it's still waiting with `GetTimer("alienattack")`. `SetTimeout` does the same without a name.

Think twice before using real time for a puzzle like this. Players who read slowly, use a screen reader or step away from the keyboard can lose through no fault of their own. The same puzzle with `SetTurnTimeout (3)` instead - three turns to act - is just as tense, and fairer.

## See also

- [Using timers and turn scripts](/tutorial/using-timers-and-turn-scripts) - the tutorial's step-by-step introduction
- [Change scripts](/change-scripts) - run a script whenever an attribute changes, rather than checking every turn
- [When scripts run](/howto/scripting/when-scripts-run)
