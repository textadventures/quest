---
layout: index
title: Using Turnscripts
---



A turnscript is a script that fires every turn. The clue is in the name.

If you worked your way through the tutorial, you will have come across a simple turnscript to track the number of turns. Similarly, you might want to track exhaustion or hunger, or perhaps even the changing weather.

Turnscripts are also great for handling other characters. Once the player has had her turn, each of the other characters gets a chance to act.


Enabled and disabled
--------------------

By default, turnscripts are disabled when the game starts. Tick the checkbox to have them start enabled.

If you give your turnscript a name, you can turn it on and off during play, using `EnableTurnScript` and `DisableTurnScript`.


Room only
----------

If you put a turnscript in a room, then it will only fire whilst the player is also in the room.

Conversely, if you find your turnscript is not firing, check you have not accidentally put it in a room (and have enabled it)!



Running immediately
-------------------

Turnscripts run after the player has taken a turn. You may decide you want them to run at the start of the game, before the player has done anything. Go to the _Scripts_ tab of the game object, and add this to the start script.

```
RunTurnScripts
```



Suppressing turnscripts
-----------------------

There may be times you do not want turnscripts to fire just for that one turn. For example, if the player types HELP, it is, perhaps, unfair if the goblin hoard still get to attack. To stop all turnscripts for one turn, add this to the code (for example, for the HELP command, this should be added to the command script):

```
SuppressTurnscripts
```


Order of turnscripts
--------------------

At the end of each turn, all active turnscripts will be run in alphabetical order, based on their name (if you do not give a turnscript a name, it will be given a default one that begins with a "k"!). See [here](http://docs.textadventures.co.uk/quest/functions/objectlistsort.html) for how Quest orders alphabetically.



Also Consider...
----------------

If you are tracking whether a specific attribute has changed, a better way is to use a [change script](change_scripts.html).


