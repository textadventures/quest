---
layout: index
title: Journal Library
---

A very simple module that lets the writer easily put a journal into the game.

Contributed by: <span class="author">The Pixie</span>

You are free to use this library in your own games, without crediting me, as long as the library is not modified. If you do want to modify the library, then you are free to do so, but please keep some attribution to me with it, at a minimum as a comment in the XML.

#### To use

1.  Create at item to be used as a journal as normal
2.  In the start script on the game item, set up the journal
    1.  Call the SetJournalObject with the item you created in step 1 as the parameter (as an object)
    2.  Optionally, call the SetJournalFont to define how the journal text will be displayed. There are three parameters. SetJournalFont(font name as string, font size as integer, colour as string)

3.  Anywhere in you game where you want an event noted (if the player is carrying her journal), invoke the AddToJournal command, with the string to be added as a parameter.

[Download](http://textadventures.co.uk/attachment/695)
[Discussion](http://textadventures.co.uk/forum/samples/topic/2610/library-adding-a-journal)

#### Functions

AddToJournal (**string** *text*)

Adds the given text as a new entry in the journal.

---

ReadJournal

Displayers on the screen all the journal entries to date.

---

SetJournalFont (**string** *font*, **integer** *size*, **string** *colour*)

Sets the font to display the journal entries in.

---

SetJournalObject (**object** *obj*)

Sets an object to be the journal. There can be only one journal item; the player must be carrying the item to use the journal. If no item has been set, the journal can be used freely.

---

UserAddToJournal (**object** *obj*, **string** *my\_name*)

Allows the player to add a comment to the journal. The next line of text she types will be appended.

#### Player Commands

NOTE

Allows the player to add a comment to the journal. The next line of text she types will be appended.

---

JOURNAL

Displays the current content of the journal.

---

Also, any text the player types that starts with a hyphen will get added to the journal, rather than treated as a command (as long as it does not match another command you have created; to avoid this issue, put your custm commands in a library that is listed before this one in your game code).