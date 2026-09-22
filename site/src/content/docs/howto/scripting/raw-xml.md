---
title: Editing the raw XML
description: Edit your whole game file, and its libraries, as XML - what it's good for, and how to do it safely
---

A Quest Viva game file is XML. Every room, object, verb, function and script in your game is an element in that file, and the editor is a way of filling in those elements without looking at the XML. The "Raw XML code view" button on the toolbar shows you the file itself.

Don't confuse this with the "Code view" button under a script, which shows a single script as [Quest script code](/howto/scripting/writing-code). That one is an everyday tool. This one edits the whole file at once, and it's for the jobs the editor can't do.

## When to use it

- **Finding the line that caused an error.** When a game stops with an error, the message usually quotes the expression that failed. Open the raw XML view, press Ctrl-F (Cmd-F on a Mac), and paste the quoted code into the search box. Bear in mind the same text may appear in several places.
- **Editing a library.** Pick the library from the "File:" dropdown at the top of the panel instead of your game file - see [Editing a library](/customise/libraries#editing-a-library). The built-in libraries that ship with the engine are listed too, marked "(read-only)": you can read them to see how something works, but you can't change them. To change one, copy what you need into your own game.
- **Bulk changes.** Renaming an attribute across two dozen objects, or moving a group of objects into a new room, is much faster as a find-and-replace than as two dozen trips round the editor.
- **Moving or copying whole elements,** including between two games.

Creating objects, functions and the rest by hand in here is possible, and almost never worth it. Let the editor do that.

## Applying changes, and what happens if you get it wrong

Editing the text doesn't change anything on its own. Click "Apply", and the editor confirms first: applying reloads the entire game from the text you typed, which means your undo history is discarded.

If the text isn't valid - malformed XML, or XML that Quest Viva can't load as a game - the change is refused. The error appears in a red bar at the top of the panel, your text stays exactly as you left it so you can fix it, and the game that's currently open is untouched. You can't destroy a loaded game by typing something wrong here.

You can still leave your game in a state that fails to load the *next* time you open it, so take a backup before a large edit: File, then "Backup…". And if a game file does fail to load, the editor offers a "Fix in Safe Mode" option, which opens the same raw XML editor on the file that wouldn't load so you can repair it.

## Moving, copying and deleting elements

XML is fussy about matching start and end tags, and an element can be hundreds of lines long. Rather than trying to select one by eye, collapse it first: look down the left-hand edge for the fold markers next to lines that open an element, and click one. The whole element folds into a single line, which you can then cut, copy or delete knowing you've got all of it and nothing else.

## Finding XML errors

Folding is also the quickest way to find an unbalanced tag. If you can fold the `asl` element at the very top - or `library`, in a library file - the XML is well formed. If you can't, work down the file folding everything that will fold. Whatever's left unfoldable contains the problem, and it'll fold once you've fixed it.

Note that this only tells you about the XML. A mistake inside a script - a missing bracket in an expression, say - is perfectly valid XML, and won't show up until that line runs in the game.

## Using an external code editor

For bigger jobs, you may prefer a general-purpose code editor, such as [Visual Studio Code](https://code.visualstudio.com/) - there are many, and most are free. This works best when your game is saved as a file in a folder, as it is in the desktop app. Close the game in Quest Viva before editing the file elsewhere, so that one doesn't overwrite the other's changes.

When you open a Quest Viva game or library, set the file's language to XML if the editor doesn't pick it up automatically. The syntax colouring helps you see what is XML and what is the text your players will read, and you'll be able to fold elements as described above.

### Find and replace

If your game uses libraries, a "find in files" feature lets you search every file in the game folder at once.

Most code editors - and the raw XML view itself - support regular expressions in find and replace, which turns a tedious edit into a single operation. Suppose two dozen objects each have a `look` script that prints a description and then some stats, and you'd rather have a type that prints a plain `desc` attribute and then the stats. A regex replace can pull the description text out of each `look` script and write it back as a `desc` attribute, in one pass. It's not something you'll do often, but when you do, it saves an afternoon.

### Spell checking

Many code editors have a spell checker, either built in or as an extension. Pointing one at your game file is a quick way to catch typos in room and object descriptions.

### Comparing files

If you end up with two versions of a file, each containing changes the other is missing, most code editors can show them side by side with the differences highlighted, either built in or with an extension.

## See also

- [Writing code](/howto/scripting/writing-code) - editing a single script as code
- [Editing a library](/customise/libraries#editing-a-library)
- [The anatomy of a Quest Viva game](/tutorial/anatomy-of-a-quest-viva-game) - what the elements in the file are
