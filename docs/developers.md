---
layout: index
title: Developers
---

Join In
-------

Quest is an open-source software project, [hosted on GitHub](https://github.com/textadventures/quest).

To compile the source code, you will need Visual Studio, which you can download for free. More details on how to do that [here](source_code.html).

The code is a mixture of C\# (for the internals) and VB.NET (for the GUI).


Developer Guidelines
--------------------

### Issue Tracker

The [Issue Tracker](https://github.com/textadventures/quest/issues) contains all features which need to be implemented, and all bugs which need to be fixed.

Assign yourself something that looks sensible. It is probably best to pick a small bug first, to give yourself a feel for the code before attempting something major.

You can also create items - if there's an obvious missing feature or bug, add it to the Issue Tracker, and assign it to yourself if you want to work on it. Check it's not a duplicate first. And if you're proposing a major new feature, please suggest it Discussions (or PM me) first.

Please ask me for help! I'm happy to answer your questions about how things work, and why things are the way they are. Just ask in [Quest Discussions](https://github.com/textadventures/quest/discussions).

You can create a fork in GitHub, clone it, and then push your changes to that fork. When you have finished working on your issue, send a Pull Request. I'll then review the code, and it can then be merged into the main repository.


### Translating Quest

If you know a language other than English, why not try translating the English.aslx file? The more languages Quest supports, the better, so please feel free to add any language you can speak!

See [Translating Quest](translating_quest.html) for full information.


### Adding your changes to Quest

When you have a set of changes that you want included in Quest, you first need to be sure they work! There will be tests you will need to do specific to those changes, but additionally ensure:

The unit tests all pass
The desktop version can be used to open a game to edit, and the game, the player, a room and an item will all display tabs properly
The desktop version can be used to open a game to play, and that you an save games whilst playing
The web player starts up properly, and will accept a couple of commands
The web editor starts up properly, and you can look at a couple of rooms/items

You can then commit your changes to your local Github repository. When you do so, you will need to give it a name. Please prefix the name with the appropriate code. This allows us to quickly see what changes need special attention when Quest is updated - when adding a new file, we need to ensure it is replicated properly for example.

[docs]  Changes to the documentation (including new files) in the `docs` folder only.
[lang]  Changes to existing language files, in `WorldModel\WorldModel\Core\Languages` only
[lang][new]  New language files and/or changes to existing language files, in `WorldModel\WorldModel\Core\Languages` and `WorldModel\WorldModel\Core\Templates`only
[aslx]  Changes to existing files (including language files), in `WorldModel\WorldModel\Core\Languages` and subfolders only.
[aslx][new]  New files and/or changes to existing files (including language files), in `WorldModel\WorldModel\Core\Languages` and subfolders only.

Commits that make changes outside of the `docs` and ``WorldModel\WorldModel\Core` can potentially causes big problems when updating the web player and web editor (in part because they cannot be tested properly), and so are less likely to be permitted.




Technical Overview
------------------

![](images/architecture.png "Architecture")

Here are each of the projects you'll find in Quest.sln:

- Quest: Windows Forms, VB.NET
	- GameBrowser: WPF, VB.NET. Powers the front page of the app - a game downloader and the recent files lists.
	- Player: Windows Forms. VB.NET
	- Editor: Windows Forms, VB.NET
	- EditorControls: WPF, C#. Provides the individual UI components that make up the Editor tabs.
	- JawsApi: C#. Utility to talk with the API for the JAWS screen reader.
	- Menus: Windows Forms, VB.NET. Top-level menu bar for Quest, handles the changes as you switch between Player mode and Editor mode.
- WebPlayer: ASP.NET Web Forms, C#
- WebEditor: ASP.NET MVC, C#
- PlayerController: Shared C# for both the desktop and web-based Players, and the shared HTML and JavaScript they use for the front-ends.
	- IASL: C#. Shared interfaces for both Legacy and WorldModel to communicate with PlayerController.
	- PlayerControllerTests
- EditorController: Shared C# for both the desktop and web-based Editors.
	- EditorControllerTests
- LegacyASL: VB.NET, converted from VB6. Handles playing games written for Quest versions 1.x to 4.x.
	- LegacyASLTests
- WorldModel: C#. This is the real core of Quest 5, handling everything to do with game state and the ASLX scripting language.
	- WorldModelTests
- Core.aslx: Quest's default game behaviour, default text, and the arrangement of the Editor, are all written using ASLX.
- Utility: C#. Some shared utility functions.
	- UtilityTests


### Interfaces

I have tried to create an object-oriented design that separates out the system into its logical components. There are some major interfaces used:

-   IASL: The Player component doesn't reference WorldModel directly, instead all communication between the GUI and the game must happen through this interface. This is how we can support both WorldModel (for Quest 5 games) and LegacyASL (for Quest 4 and earlier). Perhaps eventually the Player will support games written for other systems, if we can wrap other open-source components in something that implements the IASL interface.
-   IScript: All script commands implement IScript, so something that runs script commands doesn't need to know anything about the command it's running.
-   IFunction: Only the Expression class implements this. If we want to replace FLEE at some point we should just be able to drop in another class that implements IFunction.

### WorldModel

Everything in the game is an element. This makes it much easier for the Editor to work on many different things - objects, functions, etc.

Objects are elements. There are different kinds of objects (the `ObjectType` enum):

-   "Normal" objects which are just things in the game. Also, rooms are normal objects.
-   Exits
-   Commands
-   The `game` object itself

The WorldModel is the component that is in charge of tracking all the elements in a game, and maintains the UndoLogger which tracks changes. Objects can change during the game - they can be created and their fields can be changed. Other types of elements can't. The WorldModel is also used in the Editor, which gives us the benefit of being able to use the same save/load ASLX code, and also Undo support. So although non-object elements can't change while a game is being *played*, we must still provide "undo" support for other types of elements in the WorldModel as they may change while being *edited*.

WorldModel doesn't define any game logic - `Core.aslx` does. This means that a game is free to define its own logic - what objects are visible at any time, how to handle commands etc. - and that game is still playable by the same Quest runtime.

Any change to an element must be undoable. Changes to fields are automatically tracked if the field gets set to a new value, but this is not sufficient for operations such as adding something to a list, as the field will still be set to the same list. In this case we pass a class that implements IUndoAction into the UndoLogger.

The UI must never reference WorldModel directly. When playing games, everything is done via the IASL interface. When editing games, the editor UI talks only to the EditorController, not the WorldModel. This means we can have different editor GUIs eventually - it will make it much simpler to migrate the editor fully from WinForms to WPF, and also opens up the possibility of having an ASP.NET based editor, which would allow users to edit games online.

Within the Player component, as much of the GUI as possible should be HTML. This will make it easier to implement "play online" functionality as we won't have to reimplement so much in the online player.

