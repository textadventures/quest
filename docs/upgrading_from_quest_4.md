---
layout: index
title: Upgrading from Quest 4
---

Backwards compatibility
-----------------------

Quest 5 can play games for all versions of Quest, right back to version 1.0. However, Quest 5 can only edit games written for Quest 5. If you are currently working on a game in Quest 4, you will have to continue editing it using Quest 4.

You can install both versions on the same computer - Quest 4 and Quest 5 live quite happily together, and have separate folders, settings and entries in the Start menu.

Basic concepts
--------------

-   Quest 4 had separate concepts of "rooms" and "objects". In Quest 5, there are just objects - a room is simply another type of object.
-   The player is itself an object in Quest 5.
-   Every object has a parent - so objects inside a container are nothing special. To add an object to a container, make sure its parent is set to the containing object, not the room.
-   Quest 4 let you add your own "properties" and "actions" to an object. It also had a separate concept of "tags" which were like "built-in" properties and actions. In Quest 5, these are all unified into attributes. Every aspect of an object is an attribute. Quest 4's properties are now [string](types/string.html) attributes, and Quest 4's actions are now [script](types/script.html) attributes.
-   Quest 4 had global strings and numeric variables. In Quest 5, these are simply attributes of the [game](elements/game.html) or [player](player.html) objects.
-   Quest 4 had separate concepts of "procedures" and "functions". Quest 5 has only functions - these can optionally take parameters, and can optionally return a value. A function that returns no value is like a procedure in Quest 4.

More power
----------

-   The standard Quest logic is now written in ASL itself, which means that all of the default behaviour can be customised or overridden. This even includes basic things such as the command parser, and the functions that determine which objects are in scope (i.e. which objects the player can see and interact with at any particular time).
-   Expressions are now available everywhere.
-   No more in-lining of expressions or variables within parameters - no special characters are needed to access variable names or functions. This means that this parameter in Quest 4:

<!-- -->

     Hello #playername#

becomes this in Quest 5:

     "Hello " + playername

-   Function calls can be nested, for example:

<!-- -->

     myfunction(otherfunction("test"), differentfunction(5))

-   Game output is now HTML. You can use simple formatting codes - see [Text formatting](tutorial/text_formatting.html). You can also completely customise the user interface [using Javascript](tutorial/using_javascript.html).

New features
------------

Quest 5 is a completely new system. It implements many things that existed in Quest 4 and adds a lot of its own. Here are just a few of the new features:

-   Undo support when playing games
-   Undo and redo in the Editor
-   Create games in multiple languages
-   Integrated download manager makes it easy for players to download your game within Quest
-   Searchable object tree makes it easier to find things in the Editor
-   Completely redesigned Script Editor - no more pop-up windows, more fluid design, and context-specific templates to help you fill out "if" conditions etc.
-   Customisable verb buttons per object, and verbs also appear on a hyperlink menu
-   Lockable containers
-   Objects that can be switched on and off
-   "Ask" and "tell" support
-   View and edit attributes inherited from types within the Editor
-   You don't always need to give a prefix for each object. Quest will automatically give your object the prefix "a" or "an" by default. If you want no prefix for your object, uncheck the "Use default prefix and suffix" option - then leave the prefix box empty.
-   Turn scripts replace Quest 4's "beforeturn" and "afterturn" tags. These are much more flexible - you can set up as many as you like and turn them on and off individually.
-   Walkthroughs allow you to test your game
-   Embed videos in your game
-   Use a static picture frame at the top of the screen
-   Lists and dictionaries give you much more power than arrays

Obsolete features
-----------------

The following Quest 4.x functionality currently has no equivalent in Quest 5:

-   Window menu
-   Packager - you cannot currently package a game into a setup EXE file

