---
layout: index
title: Quest Source Code
---


Open Source
-----------

Quest is open-source (unlike, say, TADS and Inform 7), which means anyone can look at the code, and indeed (within the terms of the [licence agreement](https://github.com/textadventures/quest/blob/master/LICENSE)) make your own version.

An important implication of this is that Quest can survive its authors. It will always be possibly for YOU to maintain and update Quest.


Compiling Quest
---------------

This describes how to download and compile the Quest source code. This is my experience using Windows 10; I think you will have problems with earlier versions of Windows, but it may well be possible to do. This _might_ help if you are trying to do this on Mac or Linux, but Quest is very much a Visual Studio solution, so it may be a struggle.


### Download

Quest can be downloaded from Github. Go to this address and over on the right is a "Clone or download" button. Download the ZIP and unpack it somewhere convenient (or even better, if you have GitHub desktop installed, use that to Clone it).

[https://github.com/textadventures/quest](https://github.com/textadventures/quest)

You also need to download Visual Studio Community 2017, which is free from Microsoft.

[https://www.visualstudio.com/downloads/](https://www.visualstudio.com/downloads/)


### Compiling

You should have a folder called "quest", within which there are several more folders and files, one called Quest.sln. Double click that one, and this should open the Quest solution up in Visual Studio.

You should be able to build (_Build - Build Solution_) the solution; there may be some warnings but should be no errors.

To run the unit tests, do _Test - Run - All tests_, hopefully all 87 will pass.


### Running Quest

Press F5 to start the editor up. You should be able to load a game, edit it and and run it.


### Running the web player

You need to first create a settings file. Copy WebPlayerSettings.default.xml to a new file called WebPlayerSettings.xml, and change the settings to reflect the files on your system. These setting worked for me for a test game called test5:

```
<appSettings>
  <add key="GameFolder" value="C:\Users\pixie\Documents\Quest Games\test5\"/>
  <add key="LibraryFolder" value="C:\Users\pixie\Documents\quest-master\WorldModel\WorldModel\Core\"/>

  <add key="LogConfig" value="C:\Users\pixie\Documents\Quest Games\log\logging.xml"/>

  <add key="SessionManagerType" value="WebPlayer.DebugSessionManager, WebPlayer"/>
  <add key="FileManagerType" value="WebPlayer.DebugFileManager, WebPlayer"/>
  <add key="GameSaveFolder" value="C:\Users\pixie\Documents\Quest Games\test5\"/>
  <add key="DebugFileManagerFile" value="C:\Users\pixie\Documents\Quest Games\test5\test5.aslx"/>
  <add key="DebugFileManagerSaveFile" value="C:\Users\pixie\Documents\Quest Games\test5\test.quest-save"/>
</appSettings>
```

Set WebPlayer as the Startup project in VS by right-clicking it in Solution Explorer.

Press F5 to start the web player. It will open a new tab in your browser with the TextAdventures home page, but you can then go here to load up your test game:
 
[http://localhost:52426/Play.aspx?id=0](http://localhost:52426/Play.aspx?id=0)

Do Shift-F5 to stop the web player.


### Running the web editor

This is similar to the web player. 

Copy WebEditorSettings.default.xml to a new file called WebEditorSettings.xml, and uncomment the last two lines. Point DebugFileManagerFile to a Quest file on your PC. Here is an example (last few lines only):

```
  <!-- OR, when debugging, use the Debug Plugins -->
  <add key="FileManagerType" value="WebEditor.Services.DebugEditorFileManager, WebEditor"/>
  <add key="DebugFileManagerFile" value="C:\Users\pixie\Documents\Quest Games\test5\test5.aslx"/>
</appSettings>
```

Set WebEditor as the Startup project in VS by right-clicking it in Solution Explorer.

Press F5 to start the web editor. It will open a new tab in your browser with the TextAdventures home page, but you can then go here to edit a new game:

[http://localhost:50212/Edit/Game/0](http://localhost:50212/Edit/Game/0)



The Quest Solution
------------------

See also [here](developers.html).

Visual Studio is designed for developing huge software, and that can make it pretty daunting to use and to find anything. You will see in the first place that Quest is split into numerous projects. If you look inside each project, you will see further folders. The "bin" and "obj" folders are used by Visual Studio when you build the project, the object file for intermediate files, the binaries folder for the compiled program. In each of them is an "x86" folder, because Quest is targeting 32-bit Windows, and inside that, "debug" and "release" folders, for whether your build for debugging or release. Both "debug" and both "release" folders have copies of Quest in them, which generally you do not have to worry about, but if you do a search across the whole solution, you will get numerous hits in these files.

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

Do not know! The "I" probably stands for interface, and ASL is the scripting language.

### JawsApi

I think this is the link to the screen reader (i.e., support for vision impaired players).

### Legacy

This is what runs Quest 4 (and possibly earlier) games.

### LegacyASLTests

Unit tests for above.

### Menus

I assume this is for the desktop version.

### packages

Software packages that Quest uses.

### Player, PlayerController

The player for the desktop.

### PlayerControllerTests

Unit tests for above.

### Prototypes

JavaScript, but I am not sure what uses it.

### Quest

The main project for the desktop version.

### Setup

This is used by Inno Setup only, when creating a Quest installer.

### Utility

Is there anything in here?

### UtilityTests

Unit tests for above.

### WebEditor, WebPlayer

Web versions of the editor and player.

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
- Download a fresh from Github (this ensures I release the version currently on Githib, which could be different to the version on my PC)

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
