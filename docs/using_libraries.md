---
layout: index
title: Using libraries
---

<div class="alert alert-info">
Note: Libraries are currently only available in the Windows desktop version of Quest.

</div>
Libraries allow you to reuse elements in multiple games. That might be [object types](using_inherited_types.html), [functions](creating_functions_which_return_a_value.html), or even common objects. In fact, libraries are the basis of how Quest works - Core.aslx is a library, included by default in all Quest games. It handles much of the standard text adventure game functionality - working out which objects the player can see, handling player commands, implementing containers, and much more. This means that Quest's built-in functionality is extensible and indeed replaceable.


To see what these libraries add to your game, go to the bottom left of the Quest GUI, click on Filter, and select Show Library Elements. You will see a shed load of stuff appears in the hierarchy on the left. Everything in grey has come from a library, and if you click on it, Quest will tell you what library in a yellow banner across the top. Core.aslx adds most of the functions, all the types, commands and verbs, while English.aslx adds the templates (some of the more fundamental functions and all script commands are built-in).


Where to find libraries
-----------------------

You can find libraries at the [Libraries and Code Samples forum](http://textadventures.co.uk/forum/samples).


How to Add a Library to Your Game
---------------------------------

You can only use custom libraries only in the off-line editor. If you are creating your game in the browser version, this feature is not available.

There are several libraries available that you might want to add to your game. They are a quick way to add extra features without you doing too much work. Libraries for handling clothing, conversation and combat are good examples, but other exist (not necessarily starting with the letter 'C'!).

To download a library for GitHub, go to its page, and right click the RAW button, then select "Save link as..."

To add a library, go the bottom of the left pane in the GUI, and expand Advanced, then click on Included Libraries. Click Add, and navigate to the library. Quest will copy the file to your game folder, and add a line of code to your game so the library is part of it. If the library includes enhancements to the GUI, such as new tabs (and the three mentioned all do), save your game, close it, and re-open it, so the GUI gets updated, and the new tabs will appear.

By the way, if you are adding my Combat library, it is contained in several files; you will need to ensure they are all in your game directory; Quest will not do that for you.


Publishing Your Game
--------------------

When you use the publish tool before uploading your game, Quest creates a .quest file. This file includes everything from all the libraries you use, including Core.aslx (this means if Core.aslx is updated, it will not break your game because it will still be using the same version). You do not have to do anything to ensure the libraries are included.


Conflicts and The Order of Libraries
------------------------------------

In general, if there are several things with the same name in your game, later ones will overwrite former ones. This is good as it allows you to change the fundamentals of Quest. For example, the ShowRoomDescription function is used by Quest to display the description for the current room. You might want to do that differently. Just create a new function with the same name.

The upshot of this is that your libraries should be after the standard libraries.

As an aside, built-in functions and script commands cannot be overwritten; if you create a function called, for example, msg, it will be ignored. Also, note that for once filenames are not case-sensitive (everything else in Quest is).


Your own libraries?
-------------------

If you've created some functionality in your game that you think would be useful to others, consider turning it into a library, and add a short demo game to show what it can do. This has a number of benefits:

-   Creating a demo will help you to think about other ways the library could be used (and abused), and you will end up with a more robust system
-   Others will look at your library, and test it too, perhaps suggesting improves; again, you end up with a more reliable system
-   In a few weeks when your game will not reload for some inexplicable reason, see if the demo will load. If it does, that library is okay, if not, the error is in the library; either way you have helped to isolate the problem.
-   Finally, of course, others can get the benefit of your efforts too.

See [Creating Libraries](../creating_libraries.html) for more information.
