---
title: Quest Source Code
nav_order: 1
parent: "Developers"
---


Open Source
-----------

Quest is open-source, which means anyone can look at the code, and indeed (within the terms of the [licence agreement](https://github.com/textadventures/quest/blob/master/LICENSE)) make your own version.


Compiling Quest
---------------

This describes how to download and compile the Quest source code.

### Download

Quest can be downloaded from Github. Go to this address and over on the right is a "Clone or download" button. Download the ZIP and unpack it somewhere convenient (or even better, if you have GitHub desktop installed, use that to Clone it).

[https://github.com/textadventures/quest](https://github.com/textadventures/quest)

You also need to download [Visual Studio Community 2026](https://visualstudio.microsoft.com/downloads/), which is free from Microsoft.


### Compiling

You should have a folder called "quest", within which there are several more folders and files, one called Quest.sln. Double click that one, and this should open the Quest solution up in Visual Studio.

You should be able to build (_Build - Build Solution_) the solution; there may be some warnings but should be no errors.

To run the unit tests, do _Test - Run - All tests_, hopefully all 87 will pass.


### Running Quest

Press F5 to start the editor up. You should be able to load a game, edit it and and run it.


### Running the web editor

Copy WebEditorSettings.default.xml to a new file called WebEditorSettings.xml, and uncomment the last two lines. Point DebugFileManagerFile to a Quest file on your PC. Here is an example (last few lines only):

```
  <!-- OR, when debugging, use the Debug Plugins -->
  <add key="FileManagerType" value="WebEditor.Services.DebugEditorFileManager, WebEditor"/>
  <add key="DebugFileManagerFile" value="C:\Users\pixie\Documents\Quest Games\test5\test5.aslx"/>
</appSettings>
```

Set WebEditor as the Startup project in VS by right-clicking it in Solution Explorer.

Press F5 to start the web editor. It will open a new tab in your browser with the TextAdventures home page, but you can then go here to edit a new game:

[http://localhost:50212/Edit/Game/1](http://localhost:50212/Edit/Game/1)



The Quest Solution
------------------

See also [here](developers.html).

Generally the only folders of interest are "Resources" (containing images) and either "My Project" or "Properties" (with the actual code) - but they are plenty of exceptions.

### Dependencies

Looks to have FLEE, which is what handles code.

### docs

The documentation.

### Editor, EditorController, EditorControls

These are all for the desktop editor.

### EditorControllerTests

Unit tests for the above.

### GameBrowser

The game browser in the desktop version.

### IASL

IASL interface is the abstraction used so that the Player can handle both Legacy (Quest 4 and earlier) and WorldModel (Quest 5) games.

### Legacy

This is what runs Quest 4 (and earlier) games.

### LegacyASLTests

Unit tests for above.

### Menus

Menus for the desktop version.

### packages

Software packages that Quest uses.

### Player, PlayerController

The player for the desktop.

### PlayerControllerTests

Unit tests for above.

### Quest

The main project for the desktop version.

### Setup

This is used by Inno Setup only, when creating a Quest installer.

### Utility

Various utility classes used by the other projects.

### UtilityTests

Unit tests for above.

### WebEditor

Web version of the editor and player.

### WorldModel

This is the heart of Quest. This is where all the .aslx files are (worldmodel/worldmodel/core) and language support. All the script commands are defined in worldmodel/worldmodel/scripts, the hard-coded functions in worldmodel/worldmodel/functions/ExpressionOner.cs, and the types in worldmodel/worldmodel/types.

### WorldModelTests

Unit tests for above.



Editing Quest
-------------

### Adding new files

Visual Studio tracks files in a solution, and if you just drag a file into the folder, Visual Studio will fail to realise it is there. Either use _File - New - File_ from inside the IDE, or, if you have dragged a file into the folder, right click the folder, and select _Add - Existing File_.

Quest .aslx files need to go in this folder (or a subfolder):

```
C:\Users\pixie\Documents\quest-master\WorldModel\WorldModel\Core
```

Once the file is there, and Visual Studio has noted it is there, you can click on it in the list, and will see the properties in the box below. Set _Copy to Output Directory_ to "Copy always", so the file is copied to the various directories across the project.

At least one directory is handled differently, and you need the files added (as links) by hand; this is the "Core" folder inside "WebEditor". As before, right click the folder, and select _Add - Existing File_. Select the file(s), then from the dropdown under the "Add" button, select "Add as link". The file(s) should now appear in the list, with the same icon as the others, which is different to the icons for the master files. Again, this needs to be set to "Copy always".

The _Save all_.




Before Release
----------------

This will probably not be relevant to you, but is recorded here nevertheless so there is a record somewhere. Prior to a release, these are the steps I perform to reduce the risk of bugs. This would be the case for a beta-release as well as a normal release.

First ensure all version numbers are updated. There are lots of places! Also make sure any new files are replicated properly. Files in WorldModel should get copied to other locations when you do a Build, but you need to tell Visual Studio to do that. It is also helpful to keep a list of any new files for when you upload to textadventures.co.uk, as these will be the stumbling blocks.

- **Github**
- Synchronise with Github
- Rename the local repository
- Download a fresh from Github (this ensures I release the version currently on Github, which could be different to the version on my PC)

- **Visual Studio**
- Run unit tests, check they all pass
- Run the WebPlayer, check the game is responsive
- Run the WebEditor, check the editor is responsive
- Set to release build
- Rebuild

- **In Inno Setup**
- Compile

- Install Quest

- **In Quest**
- Check the version number
- Load `unit test_for_5_7.aslx`
- Run (ensure no failures)
- Load another game
- Check it saves
- Open a save game, check it plays normally
- Check it can be published

The file `unit test_for_5_7.aslx` can be found [here](https://github.com/textadventures/quest/blob/master/docs/util/unit%20test_for_5_7.aslx), and has tests for nearly all the code added to Quest in versions 5.7.1 and 5.7.2, and a bit for 5.8. Read about Quest unit testing [here](unit_testing.html).