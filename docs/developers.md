---
layout: index
title: Developers
---

Join In
-------

Quest is an open-source software project, [hosted on GitHub](https://github.com/textadventures/quest).

To compile the source code, you will need Visual Studio 2010 or later. You can edit the desktop version of Quest for free using [Visual Studio Express for Windows Desktop](http://www.microsoft.com/visualstudio/eng/downloads#d-express-windows-desktop).

The code is a mixture of C\# (for the internals) and VB.NET (for the GUI).

Visual Studio may complain that it can't load the Setup project. This will happen if you do not have InstallShield installed. You can safely ignore this message. Also if you are using Visual Studio Express for Windows Desktop, you can safely ignore the warning that the WebPlayer and WebEditor projects cannot be loaded, as these are not required for loading and running the main Quest project.

Developer Guidelines
--------------------

### Issue Tracker

The [Issue Tracker](https://github.com/textadventures/quest/issues) contains all features which need to be implemented, and all bugs which need to be fixed.

Assign yourself something that looks sensible. It is probably best to pick a small bug first, to give yourself a feel for the code before attempting something major.

You can also create items - if there's an obvious missing feature or bug, add it to the Issue Tracker, and assign it to yourself if you want to work on it. Check it's not a duplicate first. And if you're proposing a major new feature, please suggest it on the forums (or email me) first.

Please ask me for help! I'm happy to answer your questions about how things work, and why things are the way they are. Just send an email to me at <alex@axeuk.com> or ask in the Quest Forum: <http://textadventures.co.uk/forum/quest>

You can create a fork in GitHub, clone it, and then push your changes to that fork. When you have finished working on your issue, send a Pull Request. I'll then review the code, and it can then be merged into the main repository.

### Translating Quest

If you know a language other than English, why not try translating the English.aslx file? The more languages Quest supports, the better, so please feel free to add any language you can speak!

See [Translating Quest](guides/translating_quest.html) for full information.

Technical Overview
------------------

### Interfaces

I have tried to create an object-oriented design that separates out the system into its logical components. There are some major interfaces used:

-   IASL: The Player component doesn't reference WorldModel directly, instead all communication between the GUI and the game must happen through this interface. This is how we can support both WorldModel (for Quest 5 games) and LegacyASL (for Quest 4 and earlier). Perhaps eventually the Player will support games written for other systems, if we can wrap other open-source components in something that implements the IASL interface.
-   IScript: All script commands implement IScript, so something that runs script commands doesn't need to know anything about the command it's running.
-   IFunction: Only the Expression class implements this. If we want to replace FLEE at some point we should just be able to drop in another class that implements IFunction.

### Design Principles

*Everything* is an element. This makes it much easier for the Editor to work on many different things - objects, functions, etc.

Objects are elements. There are different kinds of objects (the ObjectType enum):

-   "Normal" objects which are just things in the game. Also, rooms are normal objects.
-   Exits
-   Commands
-   The game object itself

The WorldModel is the component that is in charge of tracking all the elements in a game, and maintains the UndoLogger which tracks changes. Objects can change during the game - they can be created and their fields can be changed. Other types of elements can't. The WorldModel is also used in the Editor, which gives us the benefit of being able to use the same save/load ASLX code, and also Undo support. So although non-object elements can't change while a game is being *played*, we must still provide "undo" support for other types of elements in the WorldModel as they may change while being *edited*.

Worldmodel doesn't define any game logic - Core.aslx does. This means that a game is free to define its own logic - what objects are visible at any time, how to handle commands etc. - and that game is still playable by the same Quest runtime.

Any change to an element must be undoable. Changes to fields are automatically tracked if the field gets set to a new value, but this is not sufficient for operations such as adding something to a list, as the field will still be set to the same list. In this case we pass a class that implements IUndoAction into the UndoLogger.

The UI must never reference WorldModel directly. When playing games, everything is done via the IASL interface. When editing games, the editor UI talks only to the EditorController, not the WorldModel. This means we can have different editor GUIs eventually - it will make it much simpler to migrate the editor fully from WinForms to WPF, and also opens up the possibility of having an ASP.NET based editor, which would allow users to edit games online.

Within the Player component, as much of the GUI as possible should be HTML. This will make it easier to implement "play online" functionality as we won't have to reimplement so much in the online player.
