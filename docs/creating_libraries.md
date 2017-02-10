---
layout: index
title: Creating Libraries
---

<div class="alert alert-info">
Note: Libraries are currently only available in the Windows desktop version of Quest.

</div>

If you are not familiar with libraries, you should go see [here](tutorial/using_libraries) first to understand how they are used. 

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
Everything in between will be XML elements, as in the main game. You can use a library to define objects, functions, types, and everything else that an ordinary game file can define.

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

Personally, I like to move all turn scripts, functions, verbs, commands and types into libraries, and leave objects (rooms and items) in the main game. An important point here is that you do not need to plan to use libraries from the start. In fact, there is not much point until you have a fair number of functions in your game.

If you create a library, it is a good idea to add Editor elements to it, so that users of your library (even if it is only you) can easily use its features. For information, please see [Editor User Interface Elements](editor_user_interface_elements.html).

_NOTE:_ The point of creating your own libraries is you can quickly get to the code if you need to tweak something. If you prefer to use the GUI to create scripts, do not bother with your own libraries.

_NOTE:_ There is an issue with creating objects in libraries that have a parent attribute set to an object in the main file, so avoid  doing that!



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