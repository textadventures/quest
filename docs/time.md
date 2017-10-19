---
layout: index
title: Tracking Time
---

This describes how you can track the passage of time in your game. If you are using the desktop version, you may prefer to use the [Clock Library](https://github.com/ThePix/quest/wiki/Clock-Library), which has a lot of extra functionality for timetabling events. What is described here is a simpler version, which is also suitable for the web version.

The simplest way to track time is to say that by default, each turn is one minute (so some things will take longer). For that we need a turnscript, and we need an attribute that tracks time.

A Turnscript
------------

So create a turn script, call it "TimeTurnScript", set it to be enabled, and paste in this code:

```
if (not HasInt(game, "time")) {
  game.time = 60 * 10 + 23
}
else {
  game.time = game.time + 1
}
```

All it does is checks if the time is already set, if not it sets it to a value, if so, it adds 1 to the value.

Note that the time is set to `60 * 10 + 23`; that will be 623 minutes past midnight. I could have set it to 623, but doing it this way makes it clear that the game starts at 10:23. You can, of course, put in your own time here.


A Better Turn Script
--------------------

Two issues we can address. The first is that the turn script will not fire at the start of the game (they fire _after_ the player takes a turn), and we really need this set up from the start.

Go to the _Script_ tab of the game object, and add this to the start script:

```
RunTurnScripts
```

Now all your turn scripts will run before the player takes a turn.

The second issue is that we do not want time to pass if the player mistypes something. On the _Features_ tab, turn on advanced scripts, then on the _Advanced Scripts_ tab, for the unresolved command script, paste in this:

```
game.notarealturn = true
msg("I have no idea what that means.")
```

Then back to the turn script, and replace the script with this:

```
if (not HasInt(game, "time")) {
  game.time = 60 * 10 + 23
}
else if (not game.notarealturn) {
  game.time = game.time + 1
}
game.notarealturn = false
```

Now if the player types in nonsense Quest cannot understand, `game.notarealturn` will be set to true, and time will not get moved on.

If you have anything else that should only happen after a real turn (such as enemies attacking the player), you can add that to the above script. It would need to go after this line:

```
  game.time = game.time + 1
```

You might also want to modify some commands so a minute does not pass whilst for them. For example, HELP and perhaps INVENTORY and LOOK. Just add this line to their scripts:

```
game.notarealturn = true
```


Showing the Time
----------------

You can display the time as a status attribute (or elsewhere), and the best way is to have a "timeasstring" attribute that will hold it nicely formatted. Because of the weird way we write time (base 60, and noon is neither am or pm), the coding suddenly gets long...

```
if (not HasInt(game, "time")) {
  game.time = 60 * 10 + 23
  if (not HasAttribute(game, "statusattributes")) {
    game.statusattributes = NewStringDictionary()
  }
  dictionary add (game.statusattributes, "timeasstring", "Time: !")
}
else if (not game.notarealturn) {
  game.time = game.time + 1
}
hours = (game.time / 60) % 24
minutes = game.time % 60
if (hours = 0 and minutes = 0) {
  game.timeasstring = "Midnight"
}
else if (hours = 12 and minutes = 0) {
  game.timeasstring = "Noon"
}
else if (hours < 12) {
  game.timeasstring = hours + ":" + minutes + " am"
}
else {
  game.timeasstring = (hours - 12) + ":" + minutes + " pm"
}
game.notarealturn = false
```

Note that the third to sixth lines are adding "timeasstring" as a status attribute of the game object.


Taking Your Time
----------------

Most actions will take 1 minutes, but you can increase that just by adding to game.time. Suppose mending the car takes 10 minutes, put this in the script (1 minute passes anyway):

```
game.time = game.time + 9
```

For exits, set them to run a script, then paste in this code:

```
MoveObject(player, this.to)
game.time = game.time + 9
```


