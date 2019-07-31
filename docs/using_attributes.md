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

[Status attributes](status_attributes.html) are just attributes that you tell Quest to display - there is nothing special about the attribute itself. 


Change Scripts
--------------

A [change script](change_scripts) is a special attribute, a script that will fire whenever another attribute changes. From the example above, we want to update `player.ammonote`, whenever `player.ammo` changes (and `player.ammomax` too).




Attribute names to avoid
------------------------

Quest uses "name", "type" and "elementtype" to track what things are, and will not allow you to change them during a game; only "name" can be changed in the editor.

There are several [important attributes](important_attributes.html) that already have a meaning in Quest, and you are probably best avoiding them to avoid confusion.

Do not use the following as names for attributes: **command, delegate, dynamictemplate, exit, function, game, include, object, template, timer, turnscript, type, verb**. It may appear at first that these are okay, but when you save the game (whether during play or when editing), the attribute will be converted to an XML element with the same name. When the game is re-loaded, Quest will assume these refer to something else entire, an actual command, or whatever.

For lists and dictionaries, attribute names cannot include spaces. Again, Quest will not complain when you do it, but it will when it tries to save your game (and you might have added a shed of load of data by then). If using the web version this might only become apparent when the player tries to save the game.
