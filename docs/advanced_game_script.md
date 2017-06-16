---
layout: index
title: Advanced game scripts
---

This file describes some new features for Quest 5.7. These are called advanced scripts for a reason, and will be of no interest to many users. You will need to turn the feature on by ticking it on the _Features_ tab of the game object. If you are using these scripts, it is assumed you are reasonably comfortable looking at code.



The `inituserinterface` script
------------------------------

The `InitUserInterface` function has been in Quest for a long time. It is an empty function that you can override to set up the user interface, and it gets called at the start of the game, and also when the player loads a saved game (unlike `game.start`, which only runs when the games starts, not a reload). This makes it ideal for code that customises the user interface.

The `game.inituserinterface` script is exactly the same; it is called at the start of the game, and also when the player loads a saved game. So why have both?

If you are using the web version, you cannot override `InitUserInterface`, so what this meant was your user interface was restricted to the basic options. Now, however, you can change the `game.inituserinterface` to do whatever you want!

For a starting point as to what you can do, see [here](ui-javascript.html).




The `unresolvedcommandhandler` script
-------------------------------------

This has been in Quest for even longer, but in Quest 5.7 you can add it via the GUI.

If it exists, this script gets called when Quest has no idea how to handle a command (if there is no script, Quest will just print "I don't understand your command.").

Why would you want to? Well, if you are counting turns, if would be unfair to count as a turn when the player has just mistyped something, or has typed a command you forgot to implement. You can use this to flag the turn as not to be counted, and then in your turn script, where you increment the turn counter, test to see if it was a real turn (and reset the flag).

The "command" variable will hold what the player typed, so your script might look like this:

```
game.notarealturn = game.notarealturn
msg("I do not understand \"" + command + "\"")
```

In you turn script (assuming `game.turn` is set to 0 in the start script):

```
if (GetBoolean(game, "notarealturn")) {
  game.notarealturn = false
}
else {
  game.turn = game.turn + 1
  // Anything else that happens each turn goes here
}
```


The `scopebackdrop` script
--------------------------

When the player does LOOK AT (or pretty much any command that references an object), Quest will compare the object in the typed text against all the items present. You can use this script to add to the list Quest will try to match against.

Why would you want to? Let us say your game is set in an old house; in every room there are walls, floor and ceiling, and some player might do LOOK AT WALL (and seriously, some will). This would not look good: 

>You are in a room with a bed and table; there are old posters stuck to the walls.
>
> > LOOK AT WALLS
>
> I can't see that.

However, you really do not want to have to create a wall object in every room, plus floor, plus ceiling, etc. The `scopebackdrop` script offers a solution; just add  your generic objects to the "items" list in that script. For example:

```
list add (items, wall)
list add (items, floor)
list add (items, ceiling)
```

Now when Quest is looking for a suitable object it will also consider these three.

You can make this as sophisticated as you like. If you have some locations outside, you could check to see if the name has "outside" in it, and add different things if it does.

```
if (Instr(player.parent.name, "outside") {
  list add (items, tree)
  list add (items, sky)
  list add (items, grass)
  list add (items, path)
}
else {
  list add (items, wall)
  list add (items, floor)
  list add (items, ceiling)
}
```

Or you could have separate rooms to store these generic items, and have each location linked (via an attribute called "backdrop" in the code below) to the appropriate one (say one for forest locations, one for town locations, etc.).

```
if (not room.backdrop = null) {
  foreach (o, GetDirectChildren(room.backdrop)) {
    list add (items, o)
  }
}
```

Note that you cannot use `ListCombine`, as that creates a new object list. You have to add the objects to the original list.

```
// WILL NOT WORK!!!
if (not room.backdrop = null) {
  items = ListCombine(items, GetDirectChildren(room.backdrop))
}
```
