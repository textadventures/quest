---
layout: index
title: DevMode
---


DevMode is a special way of running your game that makes it easier when you are developing your game. It is very much like having cheat codes.

To turn on DevMode, go to the _Features_ tab of the game object, and tick the "Show DevMode options" box. Go to the _DevMode_ tab, and set it to "Active".

Settings
--------

### Change the player or starting location

It is often useful to change where the player starts whilst testing your game. You do not want to have to negotiate twenty rooms and five puzzles to get to the bit you are working on. DevMode allows you to temporarily set the player start room to a different location.

If your game allows the player to switch between multiple characters, you can also select the one active at the start. Or you might want to create a character purely for debugging; you could set that here.


### Initialisation script

This is a script that will fire when your game starts (after the game start script), but only in DevMode.

You have set the starting location so you skip those five puzzles, but perhaps there is a flag that should be set and an object you should be carrying at this point. You can use the initialisation script to modify the game world to how it should be at that point. Once you have that bit of your game working, be sure to do it from the start, to make sure it really is doable!

You could use this to check if anything needs to be done. For example, you could check every object has a description (for a room) or look attribute (for items), and also test if any room has a "TODO" note in its annotations:

```
foreach (o, AllObjects()) {
  if (not HasAttribute(o, "look") and o.description = "") {
    msg ("Description missing: " + o.name)
  }
  if (Instr(o.implementation_notes, "TODO") > 0) {
    msg ("TODO: " + o.name)
  }
}
```

### Attributes

Rather than using the DevMode initialisation script to set up attributes, you can use the attributes table. So instead of doing this in a script:

```
hat.parent = player
joanna.hair = "green"
```

You could add them to the attributes like this:

<table>
<tr><td>hat.parent</td><td>player</td></tr>
<tr><td>joanna.hair</td><td>"green"</td></tr>
</table>

Note that attributes are set before the DevMode initialisation script is run.


During Play
-----------

DevMode also offers new tools whilst playing your game. These commands all start with a #, and you can see a list by typing #? during play. Note that the commands bypass the usual checks, so #TAKE will allow you to take any object, wherever it is, even if it is invisible and cannot be taken. If it has a script to run when taken, that will not fire. The #ON and #OFF commands will turn an object on and off, even if it is not switchable. The #TO command will allow your to switch the player to any object, the #GO command will allow you to move to any object. Clearly there is scope for seriously messing up your game, for example if you try to go to the player object!

You can also set attributes whilst DevMode is active. This is done just as you would in code, but with a # at the start of the line. You can also find the value of any attribute.


Before Release
--------------

Clearly it is vital that you turn DevMode off before releasing your game!