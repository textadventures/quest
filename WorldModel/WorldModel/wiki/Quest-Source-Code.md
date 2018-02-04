This describes how I got the Quest source code to compile and run on my Windows 7 PC. It should be the same for any Windows version, and it _might_ help if you are trying to do this on Mac or Linux.

_NOTE:_ The new interface in Quest 5.8 may require you to use Windows 10 to get it to compile.

### Download

I downloaded Quest from Github. Go to this address and over on the right is a "Clone or download" button. Download the ZIP and unpack it somewhere convenient.

[https://github.com/textadventures/quest](https://github.com/textadventures/quest)

You also need to download Visual Studio Community 2015 or 2017, which is free from Microsoft.

Double-click Quest.sln, and this should open the Quest solution up in Visual Studio.

### Compiling

I then had to get Nuget working (following instructions [here](http://docs.nuget.org/ndocs/consume-packages/package-restore)). Use the following command in the Visual Studio Package Manager Console (_Tools > NuGet Package Manager > Package Manager Console_):
```
Update-Package –reinstall
```
When asked if I wanted to replace stuff I said no to all (L). I then closed the Nunet command line.

At this point I tried to compile (_Build - Build Solution_),  had 25 errors, and a few warning. 23 errors in the Editor project, apparently because the EditorControls project was not referenced.

_NOTE:_ As of 2018, the following seems to be unnecessary. Hopefully the correct dependencies are in the Github repository now.

I right clicked the Editor project, and opened up the properties, going to the references section. In the list of imported namespaces, I ticked TextAdventures.Quest.Controls

Similarly for GameBrowser, I right clicked the Editor project, and opened up the properties, going to the references section. This time I had to add TextAdventures.Utility to the list of important namespaces. This solved the last 2 errors.

For the GameBrowser project, still in the properties, going to the code analysis section, following advice [here](http://stackoverflow.com/questions/33637211/vs2015-warning-msb3884-could-not-find-rule-set-file), for the rule set, I selected browse, and then opened this file:
```
C:\Program Files (x86)\Microsoft Visual Studio 14.0\Team Tools\Static Analysis Tools\Rule Sets\MinimumRecommendedRules.ruleset
```
I did the same for IASL and Quest projects. This solved three warnings. Some people have also reported having to do similar for EditorController, and it is possible others too.

At this point I thought it was good enough!

### Unit Tests

To run the unit tests, I did _Test - Run - All tests_, all 85 passed.

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
 
http://localhost:52426/Play.aspx?id=1 

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

http://localhost:50212/Edit/Game/1

### Adding new files

Visual Studio tracks files in a solution, and if you just drag a file into the folder, Visual Studio will fail to realise it is there. Either use _File - New - File_ from inside the IDE, or, if you have dragged a file into the folder, right click the folder, and select _Add - Existing File_.

Quest .aslx files need to go in this folder (or a subfolder):

```
C:\Users\pixie\Documents\quest-master\WorldModel\WorldModel\Core
```

Once the file is there, and Visual Studio has noted it is there, you can click on it in the list, and will see the properties in the box below. Set _Copy to Output Directory_ to "Copy always", so the file is copied to the various directories across the project.

At least one directory is handled differently, and you need the files added (as links) by hand; this is the "Core" folder inside "WebEditor". As before, right click the folder, and select _Add - Existing File_. Select the file(s), then from the dropdown under the "Add" button, select "Add as link". The file(s) should now appear in the list, with the same icon as the others, which is different to the icons for the master files. Again, this needs to be set to "Copy always".

Save all.

### Notes

I found that occasionally a build would take forever, and then have a load of errors about being unable to copy files, because they are locked. This seems to be because Visual Studio has locked them, and I have tried to do a build or run too soon after the last run. The solution is to wait a few minutes and try again.


### Before Release

This will probably not be relevant to you, but is recorded here nevertheless. Prior to a release, these are the steps I perform prior to release to reduce the risk of bugs. This would be the case for a beta-release as well as a normal release.

- Download from Github and do the above (this ensures I release the version currently on Githib, which could be different to the version on my PC)

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
- Run
- Load another game
- Check it saves
- Open a save game, check it plays normally

The file `unit test_for_5_7.aslx` can be found [here](https://github.com/textadventures/quest/blob/master/docs/unit%20test_for_5_7.aslx), and has tests for nearly all the code added to Quest in versions 5.7.1 and 5.7.2. Read about Quest unit testing [here](https://github.com/ThePix/quest/wiki/Unit-Testing-Libraries).