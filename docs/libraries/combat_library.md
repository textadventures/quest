---
layout: index
title: A Combat System for an RPG
---

Allows the creator to (relatively) easily create an RPG.

Contributed by: <span class="author">The Pixie</span>

*You are free to use this library in your own games, but please credit me. If you do want to modify the library, then you are free to do so, but please keep some attribution to me with it, at a minimum as a comment in the XML.*

Making an RPG-type game in Quest is a popular topic on the forums. It is not straightforward! However, it did inspire this library. It covers combat, magic, buying and selling and lots more. 

This library offers a relatively simple, but comprehensive combat system, the basic mechanics being somewhat like D&D (d20, no hit locations). The system can be modified in various ways, some easier than others. Free feel to modify it as you like (but credit me!).

[Download](http://textadventures.co.uk/attachment/1122) | [Discussion](http://textadventures.co.uk/forum/samples/topic/4886/rpg-style-combat-library) | [Demo](http://textadventures.co.uk/games/view/7fdenn_yieuekubbcc2kza/quest-combat)

A character is defined by four statistics; strength, stamina, intelligence, agility. Strength determines what armour can be worn, increases weapon damage and chances to hit. Agility also affects the chances to hit, as well as the character's defence. Stamina determines hit points, intelligence the level of spell that can be learnt and cast.

The download includes the demo game and a text file called notes.txt that explains how to do a lot of things; if you are trying to work out what is going on, or ghow to set something up, try there first.

Installation guide
------------------

###1. Add the library

Download the ZIP file, and extract all the files to your game folder (you do not actually need CombatDemo.aslx or notes.txt, but you do need the other nine).

Open your game up in Quest, and in the left hand pane, expand "Advanced", at the very bottom. Click on "Included libraries", then, in the right pane, click on "Add". By Filename, click on None, to open the drop down list. CombatLib.aslx should be there (probably at the top). Select it, then save your game and reload (you should see a "Reload" button towards the top right.


###2. Edit the start script

Click on the "game" object in the left pane, then go to the "Script" tab. Towards the top, it says "Start script", followed by a series of icons. Click on the seventh, "Code view". A text box will appear. Paste this in:

    CombatInitialise(true)

This will set up all sorts of stuff when the game starts, and initiate the character creation process. Alternatively, use this to skip character creation (very useful when testing):

    CombatInitialise(false)


###3. Create

Now you need to create your world. As you populate it, go to the "Combat" tab, and set objects to be the appropriate thing. The readme.txt file in the download describes those options if it is not obvious. Take a look at how it was done in the demo too.




