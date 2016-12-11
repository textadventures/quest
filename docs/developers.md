---
layout: index
title: Developers
---

Join In
-------

Quest is an open-source software project, [hosted on GitHub](https://github.com/textadventures/quest).

To compile the source code, you will need Visual Studio. The free [Visual Studio Community 2015](https://www.visualstudio.com/downloads/) will work. The first time you build Quest, it will need to download required packages from Nuget, so make sure that Nuget Package Restore is enabled.


Technical Overview
------------------

![](architecture.png "Architecture")

(If/when Quest 6 is released, the projects highlighted in blue become obsolete)

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