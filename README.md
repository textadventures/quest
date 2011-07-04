Quest 5.0
=========

Quest 5.0 is an attempt to create a modern text adventure game system from scratch. It will let authors create sophisticated games complete with graphics, sounds and videos, all without (necessarily) having to write code themselves, as a full visual editor will be included.

Quest reads in text adventure game files in an XML format called ASLX (Adventure Scripting Language XML). This contains definitions for all the elements in the game. Elements can be nested inside each other, and have properties which can be of various types - string, int, double etc., or scripts.

The core library is written in ASLX, meaning standard game behaviour can be completely customised. This includes the command parser.

Every turn in the game is undoable, as the UndoLogger tracks all changes made while the game is in progress.

Translation support is built-in - the core library contains no English text, using templates instead. This means it should be straightforward to use the same core library for games in other languages.

Instead of a completely text-only interface, the game output is HTML. Included in the example code is a demonstration of embedding a YouTube video in a game.

Progress
--------

The Player is basically done, so games can be loaded and played. The Editor is now fully functional, allowing games to be created and edited. The core library needs a lot of fleshing out - I aim to make it a much richer library to provide "out of the box" support for the kind of functionality that is built-in to other platforms such as Inform and TADS.

You can see Quest's previous incarnation, Quest 4.x at http://www.textadventures.co.uk/quest - note that this is a completely different product really, as Quest 5.0 is completely rewritten. However the fundamental idea is the same - allowing people to create text adventure games without having to write code.

Documentation
-------------

Documentation is available on the Quest 5.0 wiki at http://quest5.net