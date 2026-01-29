---
layout: index
title: Editing in Full Code View
---


_NOTE:_ You can only access full code view in the desktop version.

_NOTE:_ It is not difficult to make changes in full code view that will cause Quest to be unable to load your game, so use with caution. It is a good idea to back up your game to another location first, just in case something goes wrong.



### Moving, copying and deleting elements

XML is very fussy, and you will get an error if end tags are missing or in the wrong order. To ensure that never happens, collapse an element before manipulating it. Look down the left side; you will see  some lines have a - in a box. Click the box, and the entire XML element will be collapsed into one line (and the - becomes +). Now you can copy or delete that line, confident that you have the entire XML element.


### Errors in XML

Collapsing elements is also a great way to find XML errors. If you can collapse the `asl` element at the top (or `library` element for a library), then your XML is formatted corrected. If not, go through it and collapse all the elements that you can. The error is somewhere in whatever is left and cannot be collapsed. When you correct an error, you should be able to collapse that bit.


### Use Notepad++

For more advanced use, and for editing libraries, I would recommend using Notepad++. 

Notepad++ can be downloaded for free from here:
https://notepad-plus-plus.org/

When you open a Quest game or library for the first time, go to the _Language_ menu, and select _XML_. Your game will now be displayed in pretty colours. This is not trivial, it does help to see what is XML code and what is actual text. You will also be able to collapse XL elements as described above.


### Find and replace

Notepad++ has a very sophisticated find and replace system. For one thing you can search backwards (useful if you are searching for a particular occurrence of something that appears numerous times and over-shoot!).

If you are using libraries, the "Find in Files" option is excellent, as you can search all the files in the game folder in one go.

It also supports Regex matching, which is very powerful in combination with replace. As an example, suppose you have a couple of dozen objects, where the `look` attribute is a script that prints a description, and then gives some stats for it. A better way would be to have a type, and give that the `look` script, and have that print a new attribute, and then give the stats. You could use a Regex find and replace to convert all those `look` script attributes to `desc` string attributes, extracting from them the description, but not the stats. It may not be something you do often, but when you do use it, it can save a lot of tedious work.