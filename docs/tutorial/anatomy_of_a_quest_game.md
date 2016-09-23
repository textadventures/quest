---
layout: index
title: Anatomy of a Quest game
---

Every Quest game is made up of the following parts. Here are the main ones:

Elements
--------

There are various types of element:

Objects  
Objects are the basic building blocks of the game. Everything "physical" in the game is an object - that includes rooms and the player themselves. So in a simple game where the player is in a lounge, and there is a cat and a table in the lounge, there are four objects in total - the lounge, the player, the cat and the table.

Objects can contain other objects. This is done by setting the "parent" attribute. In our simple example, the lounge has no parent - it stands alone. The player is in the lounge, so the player's parent is "lounge". If the cat is sitting on the table in the lounge, then the cat's parent is "table", and the table's parent is "lounge".

Exits  
Exits connect objects (usually rooms) together. The exit has a parent, so our simple game might have an exit from the lounge by setting the exit's parent to "lounge". Exits also have a "to" direction, so this exit might point to another room, such as the kitchen. Or, it might point to an object inside the same room, such as a cupboard - this would allow the player to go inside a cupboard in the room.

Commands and Verbs  
Commands handle player input. They can exist globally, in which case the command will work everywhere. Commands can also exist inside a particular room, in which case that command will only work in that room. Commands have a pattern, such as "look at \#object\#". When the player types something, it is compared to all the available command patterns. The best match is then used to process what the player typed in. So if the player typed "look at cat", the "look at" command is matched, and it performs whatever action is necessary to print the description of the cat.

Game  
The game itself is a special kind of object - it contains attributes such as the name of the game, options such as how to print room descriptions, and display settings.

Attributes
----------

All element data is stored in **attributes**. An element can have an unlimited number of attributes. Attributes can store things such as the object description, alternative object names, the behaviour when an object is taken, which objects can be used on the object, and much more. The attribute can be of many types:

String  
A sequence of letters/numbers, for example "The cat is sitting quietly on the table". Obviously, strings are very common in text adventure games!

Integer  
A whole number, such as 1, 2, 3, 42 or 1 billion.

Script  
One or more script commands, which are instructions for Quest to carry out. Everything that happens in a game is controlled by script commands. Script commands can print messages, move objects around, show videos, start timers, change attributes, and much more.

List  
Lists of strings or objects

Dictionary  
A look-up table of strings or objects

Libraries
---------

Libraries are used to include common functionality in a game. There is a standard "Core" library that is included by default with all Quest games. This is made up of the elements above - commands, scripts and so on - and provides a lot of the standard functionality that players will expect in your game, such as the "look at" command, printing room descriptions, and so on.

[Next: Using scripts](using_scripts.html)