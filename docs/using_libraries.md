---
layout: index
title: Using and creating libraries
---

<div class="alert alert-info">
Note: Libraries are currently only available in the Windows desktop version of Quest.
</div>

Libraries allow you to reuse elements in multiple games. That might be [object types](using_inherited_types.html), [functions](creating_functions_which_return_a_value.html), or even common objects. In fact, libraries are the basis of how Quest works - Core.aslx is a library, included by default in all Quest games. It handles much of the standard text adventure game functionality - working out which objects the player can see, handling player commands, implementing containers, and much more. This means that Quest's built-in functionality is extensible and indeed replaceable.


To see what these libraries add to your game, go to the bottom left of the Quest GUI, click on Filter, and select Show Library Elements. You will see that a shed load of stuff appears in the hierarchy on the left. Everything in grey has come from a library, and if you click on it, Quest will tell you what library in a yellow banner across the top. Core.aslx adds most of the functions, all the types, commands and verbs, while English.aslx adds the templates (some of the more fundamental functions and all script commands are built-in).


Where to find libraries
-----------------------

You can find libraries at the [Libraries and Code Samples forum](http://textadventures.co.uk/forum/samples).


How to Add a Library to Your Game
---------------------------------

To add a library to your game, go the bottom of the left pane in the GUI, and expand Advanced, then click on Included Libraries. Click Add, and navigate to the library. Quest will copy the file to your game folder, and add a line of code to your game so the library is part of it. You will see a message at the top of the screen asking you to save your game, and then reload it; this will ensure the library is incorporated into the editor session.


Publishing Your Game
--------------------

When you use the publish tool before uploading your game, Quest creates a .quest file. This file includes everything from all the libraries you use, including Core.aslx (this means if Core.aslx is updated, it will not break your game because it will still be using the same version). You do not have to do anything to ensure the libraries are included (even libraries in other folders; as long as Quest could find a library when you were creatingyour game, it will be included).


Conflicts and The Order of Libraries
------------------------------------

In general, if there are several things with the same name in your game, later ones will overwrite former ones. This is good as it allows libraries to change the fundamentals of Quest. 

The upshot of this is that your libraries should be after the standard libraries, and you may need to be careful exactly what order they come in.


Your own libraries?
-------------------

If you've created some functionality in your game that you think would be useful to others, consider turning it into a library, and add a short demo game to show what it can do. This has a number of benefits:

-   Creating a demo will help you to think about other ways the library could be used (and abused), and you will end up with a more robust system
-   Others will look at your library, and test it too, perhaps suggesting improves; again, you end up with a more reliable system
-   In a few months when your game will not reload for some inexplicable reason, see if the demo will load. If it does, that library is okay, if not, the error is in the library; either way you have helped to isolate the problem.
-   Finally, of course, others can get the benefit of your efforts too.


Creating a library
------------------

So you want to create your own library...

It is pretty easy. Library files are just text files, so you need a text editor; I recommend Notepad++. Quest itself cannot handle them unfortunately. Like Quest itself, library files must be in XML, and a simple understanding of XML is useful.

The root element of a library file is `library. This means that the first line of the file should be this:
```
  <library>
```
And the last line should be this:
```
  </library>
```
Everything in between will be XML elements, as in the main game (go to _Tools - Code view_ to see the XML behind Quest). You can use a library to define objects, functions, types, and everything else that an ordinary game file can define.

The easiest way to create those XML elements is to create them in Quest, and then cut-and-paste into your library. For example, for a new function, create the function in Quest, check it works okay, then go into the code view, cut the relevant code, paste into you library. Test again.

The only tricky bit is ensuring you get the whole XML element, but remember for a function it starts:
```
  <function...
```
And ends
```
  </function>
```
And make sure you get whole lines.

Personally, I like to move all turn scripts, functions, verbs, commands and types into libraries, and leave objects (rooms and items) in the main game. For one thing, objects are a lot easier to edit in the proper editor, while functions and commands not so much. An important point here is that you do not need to plan to use libraries from the start. In fact, there is not much point until you have a fair number of functions in your game.

If you create a library, it is a good idea to add Editor elements to it, so that users of your library (even if it is only you) can easily use its features. For information, please see [Editor User Interface Elements](editor_user_interface_elements.html).

_NOTE:_ The point of creating your own libraries is you can quickly get to the code if you need to tweak something. If you prefer to use the GUI to create scripts, do not bother with your own libraries.

_NOTE:_ There is an issue with creating objects in libraries that have a parent attribute set to an object in the main file, so avoid  doing that!

_NOTE:_ If you make your library publicly available, make sure all the elements have names, so users can override them. In particular, check your verb elements.


File name and location
----------------------

Quest library files usually end .aslx, just like Quest games, but you can alternatively use .xml. You could then set your PC to open .xml files with your editor, rather than Quest.

You might also choose to keep all your library files in one location, whilst having other folders for the games themselves. Quest will handle that fine, as long as the `include` in your game file has the right path to the file.
```
  <include ref="../lib/Conversation.xml" />
```
When the game is published, Quest will get the code from the library in that folder.



Tips for Large Games
--------------------

If your game is big, you might find it convenient to break it up across several libraries, so it is easier to find things. Arrange the contents of libraries systematically. For example, put commands in one library, functions in another. Notepad++ allows you to search all files in a folder, which is a big help.

You can have library files invoked from other library files by the way, just as they are from the main game file.

Only put objects in libraries if you are sure they will not change. It is far easier to edit objects in the GUI, so generally not worth putting them in libraries.

Use comments. In libraries you can use XML comments, which look like this (if you do this in the main game, Quest will delete them!). Make it clear enough that someone else will understand what it does - that way you will when you look at it again in three months.
```
  <!--
  Returns the given object list as a string in the
  form a one, a two, a three.
  -->
```
If you have multiple libraries, organise them so those higher in the list do not require any lower in the list (i.e., do not use types or use functions in them). Quest does not care; as long as the function is there somewhere, it will be happy. However, occasionally you will screw up, and your game will not load, and you will have no idea where the error is. Create a new test game in the same folder, and add your libraries one by one, in order. When you add one and it refuses to load, the error is in that library. This will only work, however, if libraries higher do not use verbs, types, dynamic templates or functions lower in the list.

With that in mind, as a general rule, put verbs and dynamic templates at the top (but below the default libraries), followed by general functions, then types, followed by the specific function, then commands. Or put the general functions at the top, under the defaults, followed by each library dedicated to a specific part of your game, if you prefer to break it up that way. But think about it carefully; I cannot predict what your game is, so you need to work out the details yourself.