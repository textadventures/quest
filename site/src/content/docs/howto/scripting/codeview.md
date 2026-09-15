---
title: Editing in full code view
sidebar:
  order: 14
---

It is not difficult to make changes in full code view that will cause Quest Viva to be unable to load your game, so use with caution. It is a good idea to back up your game first, just in case something goes wrong.

You can create objects, functions, etc. in full code view, but it is far easier to let Quest Viva do that for you.

So why use code view at all? One good reason is to find errors and typos. Whilst playing your game, if Quest Viva throws an error, it will often tell you the code that caused the error. Go into code view, press [Ctrl]-F, and paste the offending code into the search box (bear in mind that the text could be in multiple places in your code).

Another reason to use full code view is to edit a library file. Choose the library from the file list at the top of the code view, instead of your game file - see [Editing a library](/advanced-topics/using-libraries#editing-a-library).


## Moving, copying and deleting elements

XML is very fussy, and you will get an error if end tags are missing or in the wrong order. To ensure that never happens, collapse an element before manipulating it. Look down the left side; you will see fold markers next to some lines. Click one, and the entire XML element will be collapsed into one line. Now you can copy or delete that line, confident that you have the entire XML element.


## Errors in XML

Collapsing elements is also a great way to find XML errors. If you can collapse the `asl` element at the top (or `library` element for a library), then your XML is formatted corrected. If not, go through it and collapse all the elements that you can. The error is somewhere in whatever is left and cannot be collapsed. When you correct an error, you should be able to collapse that bit.


## Using an external code editor

For bigger jobs, you may prefer a general-purpose code editor, such as [Visual Studio Code](https://code.visualstudio.com/) - there are many, and most are free. This works best when your game is saved as a file in a folder, as it is in the desktop app. Close the game in Quest Viva before editing the file elsewhere, so that one doesn't overwrite the other's changes.

When you open a Quest Viva game or library, set the file's language to XML if the editor doesn't pick it up automatically. The syntax colouring helps you see what is XML code and what is actual text, and you will be able to collapse XML elements as described above.


### Find and replace

If you are using libraries, a "find in files" feature is excellent, as you can search all the files in the game folder in one go.

Most code editors also support regular expressions (regex) in find and replace, which is very powerful. As an example, suppose you have a couple of dozen objects, where the `look` attribute is a script that prints a description, and then gives some stats for it. A better way would be to have a type, and give that the `look` script, and have that print a new attribute, and then give the stats. You could use a regex find and replace to convert all those `look` script attributes to `desc` string attributes, extracting from them the description, but not the stats. It may not be something you do often, but when you do use it, it can save a lot of tedious work.

### Spell checking

Many code editors have a spell-checker, either built in or as an extension.


### Comparing files

Occasionally you may find you have edited an old version of a file, and you end up with two versions, both have bits you want that is missing from the other. Most code editors can compare two files side by side and highlight the differences between them, either built in or with an extension.
