---
layout: index
title: Change Script
---

A change script is a script linked to an attribute. The script runs whenever the attribute changes. It is most useful when you have an attribute that can change in several different situations, but in all of them, you want the same thing to happen. A good example is in an RPG-style game, where you want to check the player's hit points to see if he is dead. The hit points might change when the player is attacked, drinks a poison or sets off a trap. Each of those events can modify the hits, but you have one just one change script that checks if the player is alive.

Quest has some change scripts already built in. If you change the parent attribute of the player, a change script fires that calls the OnEnterRoom function. This ensures the function gets called every time, rather than replying on game creators calling it each time the player moves (in fact, this change script is on all objects, as any object can potentially be the player).

Change scripts can be created for attributes on any object, not just the player, by the way.

As an example, let us create an attribute called "hits" on the player object. In the desktop version, you can do that on the Attributes tab by clicking Add just above the bottom box. Set it to be an integer. Once it is in the list, click on it and then click the "Add Change Script button. Quest will add a new attribute, "changedhits". Easy as that.

Now make the script display the new hit points:
```
  if (player.hits > 0) {
    msg ("Hits points now " + player.hits)
  }
  else {
    msg("You are dead!")
    finish
  }
```
The web version has no Attributes tab, making change scripts rather harder to use. We will have to do this all in code, and to ensure it is up andrunning from the start, do it in the start script of the game object. Here is the code:
```
player.hits = 5
player.changedhits => {
  if (player.hits > 0) {
    msg ("Hits points now " + player.hits)
  }
  else {
    msg("You are dead!")
    finish
  }
}
```
The first line creates the attribute, and gives it an intial value. The second line creates the change script attribute, and assigns a script to it (the `=>` assigns a script to an attribute like `=` assigns a number or string. The code in the script itself is just the same.

Quest recognises an attribute as a change script if it is a script and its name starts with "changed". There are just standard scripts, and you can use them as such:
```
  do (player, "changedhits")
```


Notes
-----

### Lists

Changing the contents of a list does not trigger a change script. Quest will consider it to be the same list. Say we have an attribute called "listofstuff".
```
  // This will not trigger a change script
  list add (player.listofstuff, "item")
  // These will
  player.listofstuff = NewStringList()
  player.listofstuff = Split("first item,item", ",")
```

### Ordering

The change script will fire when the attribute changes, so be careful where you make the change in your code. In the hit points example, this is wrong:
```
  player.hitpoints = player.hitpoints - 20
  msg("You drink the liquid... and realised it was poison!")
```
If it kills the player, she will see this:

> You are died!!

> You drink the liquid... and realise it was poison!

You need to adjust the hit points _after_ the message.


### Change inside change

Be careful changing an attribute inside its own change script, you will end up in an infinite loop!


### The "parent" attribute

An attribute you may well want to react if it changes is the "parent" attribute, as this determines where an object is, what room it is currently in. You will find, however, that it already exists. You will need to click on "Make Editable Copy" to be able to do anything with it.

If the object in question is not the "player" object and will never be the player, you can just delete the existing code, and put in your own. If this is the "player" object or can be the player, you will need to add your code to the end of the existing code.


### The oldvalue variable

There is a special variable that holds the previous value of the attribute your change script is following, and this is called "oldvalue". A good example of that in use is the change script on the "parent" attribute that was just mentioned:
```
  if (game.pov = this) {
    if (IsDefined("oldvalue")) {
      OnEnterRoom (oldvalue)
    }
    else {
      OnEnterRoom (null)
    }
    if (game.gridmap) {
      MergePOVCoordinates
    }
  }
```
The first line means that this code only applies if this object is the current player point-of-view (usually that is the "player" object). If this is the player, then the player has moved. It checks to see if oldvalue exists (which it should do, but is good to check). If it exists, it is passed to OnEnterRoom, which can then run the leaving script for the old room if necessary.