This site comprises tutorials and libraries written by me for Quest 5. Since I starting this site, I have become responsible for the updating of Quest and the official Quest documentation, so there is some stuff repeated. Some material from here is starting to disappear and reappear over there. Ultimately this will site will have libraries and guides for using them, plus the more advanced or specialised guides.

## Quest

Quest is software designed for creating and playing text adventures, and its main site is here:

http://textadventures.co.uk/

If you are just starting with Quest, you are best starting with the official tutorial, which is here:

http://docs.textadventures.co.uk/quest/tutorial/

The main documentation is here:

http://docs.textadventures.co.uk/quest/tutorial/


## Contents

[Specific tutorials](#specific)

[Libraries](#libraries)

[User Interface](#ui)

[Images](#images)



## <a name="specific"></a>Specific Tutorials

How to set up specific systems for your game.

[Dynamic Descriptions](https://github.com/ThePix/quest/wiki/Handling-Dynamic-Descriptions): How to create room descriptions that change to reflect how the game has progressed

[Keys and Locks](https://github.com/ThePix/quest/wiki/Keys-and-Locks): How to lock chests and doors.

[Enhanced Wait](https://github.com/ThePix/quest/wiki/Enhanced-Waiting): Allow the player to wait an arbitrary number of turns, but interrupt the waiting if necessary.

[Computer system](https://github.com/ThePix/quest/wiki/Modelling-a-computer-system): Modelling a computer interface that is navigated by selecting from a menu.

[Cheating](https://github.com/ThePix/quest/wiki/Cheating): Creating a CHEAT command to make debugging easier.

[Getting Input from the Player inside a Loop](https://github.com/ThePix/quest/wiki/Getting-Input-from-the-Player-inside-a-Loop): Asking the player a series of questions inside a `for`, `foreach` or `while` loop is not as straightforward as you might think.

[Types and Tabs](https://github.com/ThePix/quest/wiki/Using-Tabs-and-Types): Setting up tabs in a library can make it easier to add custom attributes to a set of similar objects.

[Multi-state clothing](https://github.com/ThePix/quest/wiki/Multistate-clothing)

## <a name="libraries"></a>Libraries

For more complicated systems, no need to re-invent the wheel, just use a library. Each of these is a tutorial, which will explain what you can do and how to do it, with a link to the library itself.

NOTE: _If using the on-line editor you will be unable to add custom libraries._

NOTE: _Some of these are now part of Quest 5.7, which is currently in beta-testing._

[Clock Library](https://github.com/ThePix/quest/wiki/Clock-Library): This allows you to track and display time in your game. Time advances with each turn, rather than in real time. For advanced users, you can also schedule events.

[CombatLib](https://github.com/ThePix/quest/wiki/CombatLib): A comprehensive library for magic and melee in a fantasy RPG. A series of tutorials will take you through how to create your own monsters, spells and equipment.

[Conversation Library](https://github.com/ThePix/quest/wiki/Conversations:-Library): Build dynamic conversations for your NPCs. Now you can also add consultables (computers, encyclopedeas, etc.) to your game.

[Lift Library](https://github.com/ThePix/quest/wiki/Lift-Library): A system where the player enters a room and presses a button to move the room to a new location.

[Liquid Library](https://github.com/ThePix/quest/wiki/Liquid-Library): Allows you to handle liquids in your game, with the capability for them to be mixed to produce new liquids.

[NPC Library](https://github.com/ThePix/quest/wiki/NpcLib): Allows your NPCs to act independently.

[Quest Library](https://github.com/ThePix/quest/wiki/Quest-Library): A way to track quests in Quest.

[Save and Load](https://github.com/ThePix/quest/wiki/Save-and-Load-Library): An alternative save/load system.

[Second Inventory Pane](https://github.com/ThePix/quest/wiki/Second-Inventory-Library): Add a second inventory pane to your game (for spells, for example).

[Shipwide Library](https://github.com/ThePix/quest/wiki/Library:-Port-and-starboard): Changes all the compass directions, to ship-style directions, port, starboard, forward and aft.

[Shop Library](https://github.com/ThePix/quest/wiki/Shop-Library): Allow the player to buy and sell goods.

[Stack Library](https://github.com/ThePix/quest/wiki/StackLib): Have items stack in the player's inventory pane.



## <a name="ui"></a>The User Interface (UI)

Quest allows you to make all sorts of changes to the user interface. Some changes can be made on the _Display_ and _Interface_ tabs of the game object, but there is a huge amount you can do besides that, using code.

[UI Part 01: Basics](https://github.com/ThePix/quest/wiki/UI-Part-01:-Basics): The basics of using code to change the interface.

[UI Part 02: HTML elements and CSS attributes](https://github.com/ThePix/quest/wiki/UI-Part-02:-HTML-elements-and-CSS-attributes): A selection CSS attributes and the values they take.

[UI Part 03: Fonts](https://github.com/ThePix/quest/wiki/UI-Part-03:-Fonts): Special considerations if using multiple fonts.

[UI Part 04: Various Tricks](https://github.com/ThePix/quest/wiki/UI-Part-04:-Various-Tricks): How to move the panes around, change some of the more obstinate elements, and other tricks.

[UI Part 05: Where and When To Do Stuff](https://github.com/ThePix/quest/wiki/UI-Part-05:-Where-and-When-To-Do-Stuff): A note about when the code needs to go to ensure your game looks the same when a saved game is loaded.

[Advanced UI Part 01: Concepts](https://github.com/ThePix/quest/wiki/Advanced-UI-Part-01:-Concepts): Understanding how Quest handles the display.

[Advanced UI Part 02: New Status Pane](https://github.com/ThePix/quest/wiki/Advanced-UI-Part-02:-New-Status-Pane): Adding a new feature that can be updated from your game, including an indicator.

[Advanced UI Part 03: New Button Pane](https://github.com/ThePix/quest/wiki/Advanced-UI-Part-03:-New-Button-Pane): Adding a feature the player can click to make something happen in the game world.

[Advanced UI Part 04: Dialogue Panels](https://github.com/ThePix/quest/wiki/Advanced-UI-Part-04:-Dialogue-Panels): Adding a dialogue panel suitable for character creation to your game.

[Advanced UI Part 05: Enhanced Compass Pane](https://github.com/ThePix/quest/wiki/Advanced-UI-Part-05:-Enhanced-Compass-Pane): Instead of putting buttons in a new pane, add them to the compass pane!

[Images Part 02: Creating images on the fly](https://github.com/ThePix/quest/wiki/Images-Part-02:-Creating-images-on-the-fly): Use SVG to generate images in Quest code.

## <a name="reference"></a>Reference

[FAQ](https://github.com/ThePix/quest/wiki/FAQ): Every web page needs one!

[A Group Project](https://github.com/ThePix/quest/wiki/Group-Project): Suggestions on how you might do this in Quest.

[On-line vs Off-line editor](https://github.com/ThePix/quest/wiki/On-line-vs-Off-line-Editor): There are limitations to using the on-line editor; this explains them.

[Publishing your game](https://github.com/ThePix/quest/wiki/Publishing-Quest-games-(inc.-size-limitations)): How to make your game available to the public.

[Tips and Tricks for Editing in Full Code View](https://github.com/ThePix/quest/wiki/Tips-and-Tricks-for-Editing-in-Full-Code-View): Occasionally it is useful to edit the XML code.

[Unit Testing](https://github.com/ThePix/quest/wiki/Unit-Testing-Libraries): Unit testing is a common technique for coders to ensure their code works. Here is how to do it in Quest.


[Quest Source Code](https://github.com/ThePix/quest/wiki/Quest-Source-Code): Getting the underlying C# code to run with Visual Studio.

[XML and HTML](https://github.com/ThePix/quest/wiki/XML-and-HTML): Related technologies used by Quest.

[YAML to Quest](https://github.com/ThePix/quest/wiki/YAML-to-Quest-(using-Ruby)): Converting a text file in YAML to Quest, using Ruby.

[Other IF systems](https://github.com/ThePix/quest/wiki/Interactive-Fiction-Authoring-Systems)