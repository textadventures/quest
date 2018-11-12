---
layout: index
title: More on Attributes
---


Most things in Quest are objects. In fact there are five types of objects. Rooms, objects and player objects are objects of the object type. Commands and verbs are objects of the command type. Exits are objects of the exit type. Turnscripts are objects of the turnscript type (timers are not objects by the way). The game object is an object of the game type.

You can use the `GetObject` function to get any of these, and all will register as an object with the `TypeOf` command.

So what makes a turnscript different to a room or the player? The answer is: attributes.

Attributes are values that are attached to an object. All objects have a "name" attribute that is unique to them. All objects have an "elementtype" attribute that is equal to "object". All objects have a "type" attribute equal to the type of the object ("object", "command", "exit", "turnscript" or "game"). Quest uses these to track what things are, and they cannot be changed during play, and indeed only the "name" attribute can be changed in the editor.

By the way, it is also worth mentioning that you cannot give anything an attribute with any of these five names: "object", "command", "turnscript", "game", "exit", "type", "elementtype", "finish".

All other attributes you can add, modify or remove as you like, both with the editor and during play, and this is what makes your game world.


Built-in Attributes
-------------------

Lots of things in Quest have built-in attributes; these are the attributes you can set on the various tabs for an object (or whatever). You can access more of them using the _Features_ tab. However, this page is more about custom attributes...


Setting Up Custom Attributes
---------------------

On the desktop version you can set up attributes on the _Attributes_ tab. The lower half lists all the attributes for this object, those in grey are what it has inherited from its types (which are listed in the upper half). To add a new attribute, click "Add", give it a name, select a type (it defaults to string), and then give it a starting value.

The web version does not support an _Attributes_ tab, so you need to do it differently. Go to the _Features_ tab, and tick "Run an initialisation script for this object", and then go to the _Initialisation script_ tab. Here we can set attributes in a script. Obviously you can do that using the GUI, but it is easier to show using code, and the code is trivial, so that is how I will show it here. In this example, two attributes are set:

```
this.colour = "red"
this.size = 15
```

Each line starts with `this`, which is a special value meaning the object the script is attached to - this object. The dot says we want something attached to the object, and the next bit is the name of attribute (like objects, these can only contain numbers, letters, spaces and underscores, and must start with a letter). Then there is an equals, followed by the value. Note that _red_ is a string, so needs to start and end with double quotes. The 15 is a number, so no quotes required.


Status Attributes
-----------------

Status attributes are just attributes that you tell Quest to display - there is nothing special about the attribute itself. Quest has two lists of status attributes, one for the game object and one for the player (if the player object can change, then each one gets its own list, but only the current one will be used). Therefore you should use the game one to hold game-wide values such as the time and score, and the player one for those that relate to the player, such as health and money.

If using the desktop version, you can set up state attributes on the _Attributes_ tab of the player or game objects. Otherwise, you will need to do it in code, in the start script of the game object.

The list is held in a string dictionary called "statusattributes", where each key is the name of the attribute, and each value is how to display it. To set this up, then, you first need to create the string dictionary, then you need to add values to it.

Here is an example, where the dictionary is added to the player, and then an attribute added to the list.

```
player.statusattributes = NewStringDictionary()
dictionary add (player.statusattributes, "hitpoints", "Hit points: !")
```

Note that the exclamation mark is a stand-in for the actual value.

Occasionally you want to do something more complicated for a status attribute - for example, you might want to show both the current and the maximum ammo is a gun. The trick is to create a new string attribute that holds both, and to update that whenever either value changes. The code might looking like this, where `player.ammonote` is a string to display the values (this needs to be run whenever the values change):

```
player.ammonote = player.ammo + "/" + player.ammomax
```

The attribute can be added to the list of status attributes as before (so in the game start script:

```
dictionary add (player.statusattributes, "ammonote", "Ammo: !")
```

So how do we ensure `player.ammonote` always gets updated? With change scripts...


Change Scripts
--------------

A change script is a special attribute, a script that will fire whenever another attribute changes. From the example above, we want to update `player.ammonote`, whenever `player.ammo` changes (and `player.ammomax` too).

If using the desktop version, you can just go to the _Attributes_ tab. Click the attribute to be tracked, `ammo`, then click on "Add Change Script" above it. You will get a new attribute, `changedammo` set to a script, and can paste the code straight in.

On the web version, you will need to do this on the initialisation script, where we set up attributes before. For example:

```
this.ammo = 6
this.ammomax = 15
this.changedammo => {
  this.ammonote = this.ammo + "/" + this.ammomax
}
this.changedammomax => {
  this.ammonote = this.ammo + "/" + this.ammomax
}
```

So the first two lines set up the attributes themselves. Then we do `changedammo`, the change script for `ammo`. Instead of `=` to assign the value, we have `=>`, which assigns a script, and then the script goes inside the curly braces. The one for ammomax is the same.

As the scripts are attached to the player, we can use `this` instead of `player`.


Capping an Attribute with a Change Script
-----------------------------------------

Often you will find you want to constrain an attribute to a range, and change scripts offer a great way to do that. Suppose we want to track purity, as a percentage, so it can range from 0 to 100.

Using the desktop version, go to the _Attributes_ tab, and select the `purity` attribute. Then click on "Add change script", and paste in this code:

if (this.purity < 0) {
  this.purity = 0
}
if (this.purity > 100) {
  this.purity = 100
}

For the web version, go to the initialisation script.

```
this.purity = 100
this.changedpurity => {
  if (this.purity < 0) {
    this.purity = 0
  }
  if (this.purity > 100) {
    this.purity = 100
  }
}
```

Now whenever Purity changes, this will fire and ensure it is in range.


Having an event fire when...
----------------------------

You may find you want something to happen when an attribute hits a certain value. The classic example is the player dies when hits go to zero.

```
this.hits = 35
this.changedhits = > {
  if (this.hits <= 0) {
    msg("You are dead!")
    finish
  }
}
```

Note that you should test if the `hits` attribute is zero _or less_, as you want the player to be dead if her hits are -5.

You can do the same for the monsters, ensuring they die when they run out of hits (editing the script as required).



Attribute names to avoid
------------------------

Do not use the following as names for attributes: **command, delegate, dynamictemplate, exit, function, game, include, object, template, timer, turnscript, type, verb**.

It may appear at first that these are okay, but when you save the game (whether during play or when editing), the attribute will be converted to an XML element with the same name. When the game is re-loaded, Quest will assume these refer to something else entire, an actual command, or whatever.

For lists and dictionaries, attribute names cannot include spaces. Again, Quest will not complain when you do it, but it will when it tries to save your game (and you might have added a shed of load of data by then). If using the web version this might only become apparent when the player tries to save the game.