---
layout: index
title: Quest 5.8
---

This file documents changes to Quest as of version 5.8.

Preamble
--------

This is my second major release for Quest, and in fact most of it has been done by other people, and in particular KV and SoonGames. Thanks also to the beta-testers and other "helpful helpers": Anonynn, Darryl Huen, DavyB, Dcode, Pertex.

Apologies if I have missed anyone. This has very much been a community effort, which I think is great.


New Interface
-------------
The big change in this version is the new interface. This is all thanks to SoonGames. Take a look! We hope you like it.


Internationalisation
--------------------

All the work on getting Quest better set up for internationalisation was also done by SoonGames. Thanks also to everyone who contributed translations:

- Danish: Benny Askham
- German: SoonGames
- Greek: Thanasis Chrysos
- Italian: Skarnisk
- Spanish: Luis

If you can help to get Quest translated for your language, or to update an existing translation, please get in contact. We can now offer translations for the editor, but only where we have the translations available, which so far is only German and, to a limited extent, Spanish.


Other changes
-------------

Quest will not allow bad attribute names in the editor ("object", "turnscript", etc.). Previously these could break your game. You can still use these in a script, which is still a problem.

A new command, TELL <char> TO <text> or <char>, <text> has been added to the _Ask/Tell_ tab.

Turnscripts have been revised a little. There is a new function, SuppressTurnscripts, which will stop all turnscripts for one turn. You might want to do this for a HELP command, for example, or for events caught by the map. The UNDO command now does this automatically, so turnscripts make more sense in that context. Also, turnscripts now run in alphabetical order (previously the order of global and local scripts would be reversed in a loaded game; now the order is always the same). If you allow multiple commands on one line in your game, each command will trigger the turnscripts.

Scope for a command can now be set to an object list attribute of the player object.

Cloning an object also clones its child objects. Now they get the correct "alias" and "prototype" attributes set.

Following a [suggestion from mrangel](http://textadventures.co.uk/forum/quest/topic/ij3dghpuok_kgo3myoj4vg/thinking-about-the-text-processor), text processor directives are now extensible.


### Thanks to KV:

You can now check if played on webplayer, desktop or mobile

RESTART command added.

Advanced popups added.

LOOK AT now counts how many times an object has been looked at.

TAKE ALL will ignore any object with "not_all" set to true (which it is for NPCs by default), and this has generally be made more sensible with regards to items in containers. Will now give the correct response if there is nothing to take. If you have your own take/drop command this should not be affected.

Transcripts and logging now possible.

Improved VERSION command.

You can now check the "isroom" attribute to see if an object is a room, and use `AllRooms` to get a list of rooms in the game.

New functions `DictionaryAdd` and `DictionaryRemove`, the first will overwrite an existing entry if it exists, rather than throw an error, the latter will do nothing if it does not exist.

### Thanks to Pertex:

Games can now send data to external sites, for processing, eg by PHP. This means you could set up a site that saves high scores, or records the number of people who chose a certain path through your game.

### Thanks to SoonGames:

DeveloperMode (see [here](https://textadventures.co.uk/forum/samples/topic/k8lt6jukx0ko-dxms8vctg/sg-developer-mode-development-tool-for-better-testing-the-running-game).

