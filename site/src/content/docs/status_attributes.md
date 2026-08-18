---
title: Status attributes
sidebar:
  order: 3
---

Status attributes are a great way to keep the player continuously informed of her progress. You might want to display the player's current score or health or money or any number of other values.

Status attributes have their values displayed in their own pane on the right side of the screen. The pane will not be present if you have no status attributes in your game (or if you have the panes on the right turned off).

You can display as many as you want, but are limited to string, int, double and Boolean values. No objects, lists or dictionaries!



Status attributes are just attributes that you tell Quest to display - there is nothing special about the attribute itself. Quest has two lists of status attributes, one for the game object and one for the player. If the player object can change, then each one gets its own list, but only the current one will be used. Therefore you should use the game one to hold game-wide values such as the time and score, and the player one for those that relate to the player, such as health and money.


Setting up
----------

You can set up status attributes on the _Attributes_ tab of the player or game objects.

First create the attribute as normal. Let us suppose we want a "score" attribute; click the plus sign by attributes, and type in the name. Then select it to be an "int". It can also be given an initial value, but zero is fine for here.

Status attributes are at the top. Click on the plus sign for status attributes to create a new one. It will ask you for the attribute name; this will be "score" (note that this must match exactly).

You will then be asked for the format - this is how the attribute will be displayed. You can leave it blank for the default, other options are discussed later.

![](/images/status2.png)



Formatting
------------

If you leave the format blank, the default display will be the attribute name with a capital at the start, a colon, and then the value.

    Score: 0
 
This will often be fine, but occasionally you will want more control. A status attribute format is just the text you want displayed with an exclamation mark when the value will appear.

Perhaps you decide to call it "Total score", but do not want to change the name of the attribute, as that is used dozens of times already in your game. Use this format:
```
Total score: !
```
It will then be displayed:

    Total score: 0

Perhaps you want to show the total. You might use this format:

```
Total score: !/10
```
It will then be displayed:

    Total score: 0/10

Status attributes do not support the text processor, but they are displayed in HTML, so you add fancy formatting.
```
Total score: <b>!/10</b>
```


Advanced options
---------------

Occasionally you want to do something more complicated for a status attribute - for example, you might want to show both the current and the maximum ammo in a gun. The trick is to create a new string attribute that holds both, and to update that whenever either value changes. The code might looking like this, where `player.ammonote` is a string to display the values (this needs to be run whenever the values change):

```
player.ammonote = player.ammo + "/" + player.ammomax
```

The attribute can be added to the list of status attributes as before (so in the game start script:

```
dictionary add (player.statusattributes, "ammonote", "Ammo: !")
```

So how do we ensure `player.ammonote` always gets updated? With [change scripts](change_scripts.md)...



Behind the Scenes
------------------

Core.aslx updates the status attributes using an UpdateStatusAttributes function. It populates a string and then sends it to the UI using a SetStatus [request](/scripts#request).
