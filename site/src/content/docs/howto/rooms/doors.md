---
title: Doors, locks and keys
description: Lock an exit and unlock it later, use a key, make a door that opens and closes from both sides, add a combination lock, and stop the player leaving
---

Most text adventures leave doors implied - when the player heads into the kitchen, they're assumed to open the door on the way. But sometimes a way through needs to stay shut until the player has done something: found a key, typed a code, or persuaded a guard to move. This page shows you how to build each of those.

| You want | Use |
|---|---|
| A way that's blocked until something happens | A [locked exit](#locking-an-exit), unlocked from a script |
| A locked door that a key opens | An ["unlock" verb that checks for the key](#a-locked-exit-and-a-key) |
| A door the player can open, close, lock and unlock from both sides | A [door object](#a-door-between-two-rooms) that the exits check |
| A keypad or combination lock | [`GetInput()`](#a-combination-lock) and a locked exit |
| To stop the player leaving, whichever way they try | [`notallowedtoexit`](#stopping-the-player-leaving) |

## Locking an exit

Select the exit in the tree, and on its _Exit_ tab tick "Locked". Type what the player should see when they try to go that way in "Print message when locked" - something like "The back door is locked." If you leave it empty, the player sees "That way is locked."

A locked exit is still listed in the room description and on the compass, but the player can't use it.

To unlock it during the game, the exit needs a name, so that a script can refer to it. Exits don't have one unless you give them one - the editor reminds you when you tick "Locked". Enter a name such as "garden exit" in the "Name" box.

Then, in any script, add "Unlock exit" from the Objects category and choose "garden exit". In code, that's:

```quest
UnlockExit (garden exit)
```

"Lock exit" (`LockExit`) does the opposite. Both are the same as setting the exit's `locked` attribute directly, which is handy when you want to test it in an "If":

```quest
garden exit.locked = false
```

## A locked exit and a key

Quest Viva's built-in keys are for objects, not exits - there's no "key" setting on the _Exit_ tab. The simplest way to lock an exit with a key is to give the player something to unlock.

Lock the exit and name it "garden exit" as above. Then add an object for the door in the same room - call it "back door", and tick "Scenery" on its _Setup_ tab so it isn't listed separately in the room description. On its _Verbs_ tab, add an "unlock" verb, set it to "Run a script", and add an "If" with the condition "player is carrying object" and the key:

```quest
if (Got(brass key)) {
  msg ("You unlock the door with the brass key.")
  UnlockExit (garden exit)
}
else {
  msg ("You need a key.")
}
```

The player can now type `UNLOCK DOOR`:

```
> south
The back door is locked.

> unlock door
You need a key.

> take key
You pick it up.

> unlock door
You unlock the door with the brass key.

> south
You are in a garden.
```

The same pattern works for anything that should open the way - a lever, a password, or a guard who steps aside after you've given him a sandwich. Whatever it is, the script just unlocks the exit.

## A door between two rooms

If the player needs to open and close a door, or lock it again, make the door an object that can be opened, closed and locked, and have the exits on both sides check it. The door's lock uses the built-in key handling, so you don't need to write any scripts for `OPEN`, `CLOSE`, `LOCK` or `UNLOCK`.

Say the lounge has a door to the kitchen to the west.

1. Create an object called "door" in the lounge, and tick "Scenery" on its _Setup_ tab.
2. On its _Features_ tab, tick "Container". On the _Container_ tab, set "Container type" to "Openable/Closable". Leave "Script to run when opening object" and "Script to run when closing object" empty, so Quest Viva opens and closes the door for you.
3. If it has a lock, set "Lock type" to "Lockable", set "Number of keys to unlock container" to 1 and choose the key. Tick "Locked" if it starts locked, and change "Unlock message" and "Lock message" if you like.
4. Leave the two exits (west from the lounge, east from the kitchen) unlocked. On each exit's _Exit_ tab, tick "Run a script (instead of moving the player automatically)", and add this script to both:

```quest
if (door.isopen) {
  MoveObject (game.pov, this.to)
}
else {
  msg ("The door is closed.")
}
```

`this.to` is the room the exit leads to, so the two exits can share exactly the same script. The exits don't need names, because they check the door instead of being locked themselves.

### Making the door visible from both sides

An object can only be in one room at a time. To make the door appear in whichever room the player is in, move it there as they arrive. On the _Scripts_ tab of the lounge, add this to "Before entering the room":

```quest
MoveObject (door, lounge)
```

and on the kitchen's _Scripts_ tab, the same with `kitchen`. The door is now there from both sides, including in the room the player starts in:

```
> w
The door is closed.

> open door
It is locked.

> take key
You pick it up.

> unlock door
You unlock the door.
You open it.

> w
You are in a kitchen.

> close door
You close it.

> lock door
You lock the door.

> e
The door is closed.

> open door
You unlock the door.
You open it.
```

"Automatically unlock if player has the key(s)" and "Automatically open when unlocked" are ticked by default, which is why `UNLOCK DOOR` also opens it and `OPEN DOOR` also unlocks it. Untick them if you want the player to do each step separately. The player can't lock the door while it's open unless you tick "Can be locked open".

### Telling the player

Let the player know whether the door is open by using the text processor in each room's description:

```
The lounge is very retro. The door to the west is {either door.isopen:open|closed}.
```

Scripts can check `door.isopen` and `door.locked` in the same way, or change them - setting `door.isopen = true` opens the door from both sides at once. To have the door start open, tick "Is open" on its _Container_ tab.

## A combination lock

Here the player has to type the right code into a keypad to unlock the vault. Lock the exit to the vault and name it "vault exit", as in [Locking an exit](#locking-an-exit), with a message like "The vault door is locked. There is a keypad next to it."

Add a "keypad" object. On its _Features_ tab, tick "Use/Give", then on the _Use/Give_ tab, under "Use (on its own)", set "Action" to "Run script". Ask for the code with "Set a variable or attribute", choosing "player's typed input":

```quest
msg ("Enter the code:")
code = GetInput()
if (code = "4321") {
  msg ("The keypad beeps, and the lock clicks open.")
  UnlockExit (vault exit)
}
else {
  msg ("The keypad buzzes angrily.")
}
```

`USE KEYPAD` now asks for the code and waits for the player to type it. See [Asking the player](/howto/scripting/asking-the-player) for more about `GetInput()`, including checking answers that might be typed in different ways.

### A different code every game

To stop players looking the code up, pick a random one when the game starts. On the game's _Scripts_ tab, add this to the "Start script":

```quest
game.code = ToString(GetRandomInt(1000, 9999))
```

That gives a four-digit code. `ToString` turns it into a string, because `GetInput()` always gives you a string, and the two have to match. In the keypad's script, compare against the random code instead:

```quest
if (code = game.code) {
```

Then put the code somewhere the player can find it - a note's description, say, using the text processor:

```
Someone has scribbled {game.code} on it.
```

## Stopping the player leaving

Sometimes the player shouldn't be able to go anywhere at all for a while - they're tied to a chair, or sitting in a rollercoaster car. Rather than locking every exit, set the player's `notallowedtoexit` attribute to the message you want them to see. Use "Set a variable or attribute" in whatever script ties them up:

```quest
game.pov.notallowedtoexit = "You can't go anywhere while you're tied to the chair."
```

Now every exit shows that message instead of moving the player. To let them move again, set it back to `null`:

```quest
game.pov.notallowedtoexit = null
```

This affects only the player going through exits. Scripts can still move the player with `MoveObject`, and a locked exit still shows its own locked message first.

## See also

- [Exits](/howto/rooms/exits) for everything else on the _Exit_ tab
- [Containers](/howto/objects/containers) for locked boxes and chests, which use the same key settings as the door
