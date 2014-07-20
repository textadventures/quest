---
layout: index
title: Creating Libraries
---

Libraries have the same format as a game ASLX file, except that the parent attribute is &lt;library&gt;.

Because libraries are otherwise ordinary ASLX files, they can define objects, functions, types, and everything else that an ordinary game file can define. However, at present they cannot be edited using Quest - you will have to edit the code with a text editor.

Quest will copy your library into the same folder as your game when you add a library through the GUI. However, you may prefer to have a separate folder for library files, especially in one library file is used by a number of games. Just add the library in code view, including the path to the library. When you publish a game, all library code gets wrapped up into your game, wherever it is on your computer.

If you create a library, it is a good idea to add Editor elements to it, so that users of your library can easily use its features. For information, please see [Editor User Interface Elements](editor_user_interface_elements.html).

See also [Using libraries](tutorial/using_libraries.html)
