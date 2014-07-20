---
layout: index
title: command element
---

    <commandoptional name="name"optional pattern="pattern"optional unresolved="unresolved text"optional template="template name">script</command>

or

    <commandoptional name="name">attributes</command>

Creates a command. There are two syntaxes - one syntax lets you specify a pattern, some text to display when an object is unresolved, and the script to run. The second syntax is more open and flexible, and lets you specify everything by directly setting the attributes of the command object. The second syntax is preferred, although the first may be more concise.

All commands automatically inherit a "defaultcommand" type if it exists.

Name
----

If a name is not specified, a unique name will be created. Using the first syntax allows Quest to try and create a user-friendly name by taking the first word(s) of the specified pattern; otherwise the name will be something like "k1". I recommend you always specify a name, as it will make debugging easier - the Debugger will show you a sensible name for your command. It will also let you easily change the behaviour of the command by setting its attributes when the game is in progress.

Pattern
-------

The "pattern" attribute of a command is a string - the regular expression that triggers the command. You can use friendlier syntax with type="simplepattern", which in Core.aslx is set as the implied type for a command "pattern" attribute, so you don't need to specify it. This will convert friendly syntax such as "look at \#object\#" into a regular expression. If you want to specify a regex yourself, you need to explicitly set type="string".

Unresolved
----------

The "unresolved" attribute is the text to print if the user enters the name of an object which is not in the current visible scope.

Template
--------

The "template" attribute specifies the command pattern to use, if the command pattern is defined by a [verbtemplate](verbtemplate.html).

Multiple
--------

To handle "take all" and "drop all", the "take" and "drop" commands have a "multiple" attribute. This is of implied delegate type AssociatedScope, which means that the "multiple" attribute is a function which returns an [objectlist](../types/objectlist.html).

This means that the "all" can refer to different things - for "take all", the "all" refers to the non-scenery objects in the current room. For "drop all", the "all" refers to objects in the player's inventory.
