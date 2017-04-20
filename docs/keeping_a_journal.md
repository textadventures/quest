---
layout: index
title: Keeping a journal
---

This is a way to add a journal to your game. This is a book the player can make notes in, and that the game can add to as well when appropriate.


The journal
-----------

The first step is to add an object to the player that will be the journal. Let us call it "journal". Make sure it is in the player; Quest will try to put it in the same room. To move it to the right place, off-line you can just drag it, on-line click the _Move_ button.

We will be adding new commands later to handle the journal and what we could do is have those commands check the player has the journal, and give an error if not. However, we are going to do this differently, and stop the play dropping the journal, so we know she will always have it. Go to the _Inventory_ tab of the journal, and untick "Object can be dropped". On the _Object_ tab, delete "Drop" from the list at the bottom.

The journal needs to hold information so we need to give it a string list to do that. If off-line, you can do that on the _Attributes_ tab, add a new attribute called "entries", and set it to be a string list.

If working on-line, go to the _Scripts_ tab of the game object, and add this to the start script:
```
  journal.entries = NewStringList()
```

Read the Journal
----------------

We want the player to be able to read the journal, so go to the _Verbs_ tab of the journal, click _Add_ and type "read". At the bottom, set it to "Run a script", and paste this in:
```
  if (ListCount(this.entries) = 0) {
    msg ("You have nothing written in your journal.")
  }
  else {
    msg ("You look at your journal:")
    foreach (s, this.entries) {
      msg (s)
    }
  }
```
The first bit checks if anything is written in the journal yet, and if not then says as much. The second bit prints an introductory message, then goes through each entry in turn, printing it out.


The JOURNAL command
-------------------

This is optional, but will allow the player to type JOURNAL to see what is currently written in it, and would be useful if you turn off the panes on the right.

Create a new command, and give it the pattern "journal". Then paste in this code:
```
   do(journal, "read")
```
We have already done the hard work setting up the READ verb, so here all we need to do is invoke that script. We could paste in the same code, but doing it this way means that if we later update that code, we only have to do it once.


Adding to the Journal
---------------------

The journal can be used for two things. The game can write to it automatically when something important happens, and the player can write in it too. We will do the former first. Unfortunately for this tutorial, there are countless things that could be significant and need to be recorded; you will have to decide where and when to do that. The important bit is to add this line of code (modifying the text as appropriate of course):
```
  list add(journal.entries, "You did something important!")
```

Letting the Player Write in the Journal
---------------------------------------

We are going to do this three ways. Firstly, for USE JOURNAL. On the _Features_ tab, tick "Use/Give", then on the _Use/Give_ tab in the "Use (on its own)" section, set it to "Run script". Paste in this code:
```
  msg ("Please type the text to go in the journal")
  get input {
    list add (journal.entries, result)
    msg (result)
  }
```
The `get input` command makes Quest wait for the player to type something, and that goes into a special variable called `result`. That text then gets added to the journal entries.


The NOTE command
----------------

Like the JOURNAL command, this is optional, but useful if the right pane is turned off. We can use the same trick here too. Create a new command, and give it the pattern "note". The paste in this code:
```
   do(journal, "use")
```


The - command
-------------

We can also let the player just type in a journal entry without any command. If the input starts with a dash, the rest of the line will go into the journal.

Again, create a command, and give it this pattern: "-#text#" (no quotes). Quest will match `#text#` to any text, and will match the dash exactly, so this will match any line that starts with a dash, and the rest of the line will go into a variable called `text`.

Paste in this code:
```
  msg("You write in your journal: " + text)
  list add(journal.entries, text)
```
Probably worth pointing out to the player that she has this option.



Fancy display
-------------

Quest has a huge scope for showing text in different ways, and this is discussed else, so will not be covered here. However, I will say where to make the changes. What we want to display differently is the actual text written in the journal, so it is the "read" verb of the journal that needs updating. here is an example:
```
  if (ListCount(this.entries) = 0) {
    msg ("You have nothing written in your journal.")
  }
  else {
    msg ("You look at your journal:")
    defaultfont = game.defaultfont
    defaultforeground = game.defaultforeground
    SetFontName ("serif")
    SetForegroundColour("Blue")
    foreach (s, this.entries) {
      msg (s)
    }
    SetFontName (defaultfont)
    SetForegroundColour(defaultforeground)
  }
```
The first five lines are the same. Then the current values are saved to suitable variables, before the new vales are set. The entries are printed, and then the old values are set again to get the display back to normal.