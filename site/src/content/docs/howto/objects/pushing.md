---
title: Pushing objects between rooms
description: Let the player push a heavy object through an exit, such as PUSH CRATE NORTH, and use it in the next room
---

Some objects are too heavy to carry but can still be moved - a crate the player pushes into the next room, then stands on to reach a trapdoor. This page shows you how to add a PUSH command that takes an object and a direction, and moves both the object and the player through that exit.

## The PUSH command

Select "Commands" in the tree (underneath "game"), click "+ Add" and choose "Add Command". Enter this pattern:

```
push #object# #exit#;shove #object# #exit#
```

`#object#` matches any object the player can see, and `#exit#` matches any exit from the current room, so PUSH CRATE NORTH and SHOVE CRATE N both work. If the player names an object that isn't there, or a direction with no exit, the script doesn't run and Quest Viva replies "I can't see that."

Then add this script. In the editor, the moves are "Move object" from the Objects category - choose "player" as the object to move the player - but it's quicker to paste it into code view:

```quest
if (not GetBoolean(object, "pushable")) {
  if (GetBoolean(object, "take")) {
    msg ("You can just pick " + object.article + " up.")
  }
  else {
    msg (DynamicTemplate("DefaultPush", object))
  }
}
else if (exit.locked) {
  msg (exit.lockmessage)
}
else if (not TestExitGlobal(exit)) {
  // The player isn't allowed to leave, and has already been told why
}
else if (exit.runscript or exit.lookonly or DoesInherit(exit, "updowndirection")) {
  msg ("You can't push " + object.article + " that way.")
}
else {
  msg ("You push " + GetDefiniteName(object) + " " + exit.alias + ".")
  MoveObject (object, exit.to)
  MoveObject (game.pov, exit.to)
}
```

Most objects shouldn't move this way, so the command only works on objects you've marked as pushable. On the crate's _Attributes_ tab, add an attribute called `pushable`, make it a Boolean, and tick it.

The script then checks each reason the push might fail, in turn:

- **The object isn't pushable.** `GetBoolean` returns false when the attribute is false or missing altogether, so you only need to add it to the objects that can be pushed. Something the player could just carry gets a hint to pick it up. Anything else gets the standard reply to PUSH, "You can't push it."
- **The exit is locked.** The player sees the exit's own "Print message when locked" text, or "That way is locked." if you left it empty. Without this check, the player could push the crate through a door they can't open themselves.
- **The player isn't allowed to leave.** `TestExitGlobal` is the check the built-in GO command makes. If you've set the player's `notallowedtoexit` attribute - see [Stopping the player leaving](/howto/rooms/doors#stopping-the-player-leaving) - it prints that message and returns false.
- **The exit is one you can't push through.** Up and down exits both inherit the `updowndirection` type, which is simpler and more reliable than checking the alias for "up" and "down" - it works whatever the exit's alias says. Exits that "Run a script (instead of moving the player automatically)", such as the doors on [Doors, locks and keys](/howto/rooms/doors#a-door-between-two-rooms), are refused too, because the script decides where the player goes, not the exit's "To" room. So are look-only exits, which don't lead anywhere.

If all is well, the script prints the message, moves the object to the exit's destination (its `to` attribute) and moves the player after it. Moving the player shows them the new room, just as if they had walked there:

```
> push crate north
You push the crate north.

You are in a barn.
You can see a crate.
You can go south or up.
```

We use `game.pov` rather than `player`, so the command still works if the player changes to a different character during the game.

If the object should move but the player should stay where they are - a barrel rolled down a corridor, say - delete the last `MoveObject` line.

### Blocking a single exit

To stop the crate going through one particular exit, such as a doorway with a step, add a string attribute called `nopushing` on that exit's _Attributes_ tab containing the message to show, like "The step is too high to push the crate over." Then add this check straight after the `exit.locked` one:

```quest
else if (HasString(exit, "nopushing")) {
  msg (exit.nopushing)
}
```

### Pull and drag

To let the player PULL or DRAG the object as well, add those to the pattern, separated with semicolons:

```
push #object# #exit#;shove #object# #exit#;pull #object# #exit#;drag #object# #exit#
```

The messages say "push" whichever word the player used. If that matters, make a separate command for PULL with its own messages - the script is otherwise the same.

## Using the object in the next room

Once the crate is in the barn, you usually want it to do something there. Checking whether the crate is in the same room as the player is enough for most puzzles - you don't need the player to be standing on it.

Here, the barn has an "up" exit to the loft through a trapdoor that's too high to reach without the crate. On the exit's _Exit_ tab, tick "Run a script (instead of moving the player automatically)", and add this "Script to run":

```quest
if (crate.parent = this.parent) {
  msg ("You climb on to the crate and pull yourself up through the trapdoor.")
  MoveObject (game.pov, this.to)
}
else {
  msg ("The trapdoor is too high to reach.")
}
```

In an exit's script, `this` is the exit, so `this.parent` is the room the exit is in and `this.to` is the room it leads to. In the editor, add an "If", leave its condition set to "expression", and type `crate.parent = this.parent`.

```
> up
The trapdoor is too high to reach.

> south
...
> push crate north
You push the crate north.
...
> up
You climb on to the crate and pull yourself up through the trapdoor.

You are in a loft.
```

Because the up exit is still an ordinary exit, the player sees "up" in the list of exits and on the compass, which hints that there's a way up to be found.

## See also

- [Exits](/howto/rooms/exits) - exit scripts, `this.to` and other exit attributes
- [Doors, locks and keys](/howto/rooms/doors) - locking and unlocking exits
- [Custom commands](/tutorial/custom-commands) - more on command patterns
