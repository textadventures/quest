---
layout: index
title: Advanced game scripts
---


Tick the check box on the _Features_ tab of the `game` object to see the advanced game scripts.


inituserinterface
-----------------

The `InitUserInterface` function has been in Quest for a long time. It is an empty function that you can override to set up the user interface, and at gets called at the start of the game, and also when the player loads a saved game (unlike game.start, which only runs when the games starts, not a reload).

The `game.inituserinterface` script is exactly the same; it is called at the start of the game, and also when the player loads a saved game. So why have both?

If you are using the web version, you cannot override `InitUserInterface`, so what this meant was your user interface was restricted to the basic options. Now, however, you can change the `game.inituserinterface` to do whatever you want!

This is where you should put any code that modified the user interface. See [here](ui-custom.html) for more details on what you can do.


unresolvedcommandhandler
------------------------

This has been in Quest for a long time, but in Quest 5.7 you can add it via the GUI.

If it exists, this script gets called when Quest has no idea how to handle a command (otherwise Quest will just print "I don't understand your command.").

Why would you want to? Well, if you are counting turns, if would be unfair to count as a turn when the player has just mistyped something, or has typed a command you forgot to implement. You can use this to flag the turn as not to be counted, and then in your turn script, where you increment the turn counter, test to see if it was a real turn (and reset the flag).

The "command" variable will hold what the player typed, so your script might look like this:

```
game.notarealturn = game.notarealturn
msg("I do not understand \"" + command + "\"")
```

In you turn script (assuming game.turn is set to 0 in the start script):

```
if (GetBoolean(game, "notarealturn")) {
  game.notarealturn = false
}
else {
  game.turn = game.turn + 1
  // Anything else that happens each turn goes here
}
```


scopebackdrop
-------------

When the player does LOOK AT (or most other commands), Quest will compare the object in the typed text against all the items present. You can use this script to add to the list Quest will try to match against.

Why would you want to? Let us say your game is set in an old house; in every room there are walls, floor and ceiling, and some player might do LOOK AT WALL (and seriously, some will). This would not look good: 

```
You are in a room with a bed and table; there are old posters stuck to the walls.

> LOOK AT WALLS

I can't see that.
```

However, you really do not want to have to create a wall object in every room, plus floor, plus ceiling, etc. The `scopebackdrop` script offers a solution; just add  your generic objects to the "items" list in that script. For example:

```
list add (items, wall)
list add (items, floor)
list add (items, ceiling)
```

Now when Quest is looking for a suitable object it will also consider these three. See the "Extended Scope" section of [this page](advanced_scope.html) for more.