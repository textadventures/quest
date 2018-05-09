---
layout: index
title: Competition Entry
---

Competitions are a good way to reach a wider audience for your Quest adventure, but you better be prepared to be judged harshly...

Probably the most significant Interactive Fiction competition is [IfComp](http://www.ifcomp.org/), run during October each year, and this page is mostly geared towards that. It will not guarantee your game is a winner, but hopefully will improve its ranking to some degree.

I originally wrote this in 2013, and have pdated it slightly. My game The Myothian Falcon was placed 24th in IFComp 2011 (hey, it was high enough to get offered a prize), and I beta-tested three out of the top five games from IFComp 2012, including the winner, so I have at least *some* experience. That said, obviously, this is just my opinion.


Starting Out
------------

Before you start creating, think about your game.

### Time

IFComp requires that games can be played in two hours. This is a practical necessity with around 30 games submitted each year, that adds up to 60 hours of playing times for the judges. They do not have time for longer games. Aim for a 1.5 to 2 hour play time.

### Originality

Try to create a game that stands out from the crowd, something with a novel hook to it. Perhaps easier said than done, but take a look at previous winners to see what I mean. Talking of which...

### Easy Puzzles

Do not make the puzzles too tricky. With only two hours playing time, if a player gets stuck on one puzzle, she might not find half your game. Of course, you need *some* challenges, the trick is to get the balance right.

### Compare to Other Entries

Take a look at some other entries from previous years, and see what works and what does not. See what the standard is. Just as important, read the reviews and see what the judges think worked and what did not. Think if common critisms might also apply to your own work, and modify it accordingly.

Implementation
--------------

### Help, Hints and Walk-though

Include some in-game system to help players get to the end. They only have two hours and if they are stuck on a puzzle with no way to cheat, they just will not see the end of your game. Make sure the clues are both good (easy to follow) and comprehensive (cover all possible problems).

IFComp requires a walk-through to prove the game is winnable, but a long list of commands is actually pretty useless to the player. Providing a walk-through that tells the player what to do, rather than what to type, will ensure they can get to the end and hopefully still enjoy the trip.

### About

Include an "about" command, so you can tell people who wrote the game, and give thanks to anyone who helped you. Include a version number.

Credit beta-testers here; you may be marked down otherwise.

### Implement Everything

Every object mentioned in the text should be implemented as an object that can be looked at in the game. Also aim to implement all the common commands such as "jump", "xyzzy", etc., even if they are not relevant. Default and error responses are *bad*.

Or submit a game without a command line, such as a CYOA or gamebook.

### Feelies

Some games include feelies. Back in the day, commercial adventure games included posters, comic books, scratch-and-sniff cards, etc. to limit piracy as much as anything. Nowadays, these feelies are virtual... so you cannot actually feel them. Nevertheless, they seem to be popular, and can help to give a game a profession touch.

Unfortunately, it is easy for players to miss feelies; if they play on line, they just will not know they exist. Happily Quest handles this well, as you can insert images, videos and audio right into your game, and Quest has support for cover art built in.

### The User Interface

Bear in mind that the vast majority of players will be playing on-line, so bear that in mind.

Think carefully what elemenbts of the user interface (UI) you want to include. By default, Quest includes a command line, hyperlinks in the text and the panes on the right. Are they all appropriate to your game?

Turning off the command bar will make it much easier to build your game, as you very much limit what the player can do, but at the cost of destroying the illusion of freedom for the player. For a traditional game, you might prefer to have only the command line.

Also think about the colours and the font. Be sure to pick a font that reflects the style of your game, and is easy to read.

The important message here is to think about the UI, and make a choice for what is right for your game, and not just use the Quest defaults.


Testing
-------

Beta-testing is getting other people to play your game before you release it. It is absolutely vital; with the best will in the world, they are sure to find spelling mistakes, objects you have not implemented, verbs you have not thought of, routes through the game you have not considered. Better these things are found during beta-testing than after a release, and especially during a competition.

### Before Beta-Testing

It is tempting to get the game to beta-testers fast, but you are really just wasting their time and yours if you know there are problems before sending it. So:

1. Play the game through and correct any mistakes you can find.

2. If using the desktop editor, open the game up in Notepad++ and use it to check the spelling (you will need to install a plug in). For the web editor, hopefully your browser has a spell checker, and you were checking the spelling as you typed.

3. Some things you might want to check, depending on your game:

-   Every room and object has an alias and a description
-   Everything mentioned in a description is actually implemented
-   The appropriate display and inventory verbs are there, and inappropriate ones are absent

4. Play the game, then try to save it. When Quest saves it does some extra error checking it does not do any other time, so this a quick test of your code. If it saved successful, you should now try to load the game. Loading will stress test you UI, so check that the game still looks the same, and the various parts of the UI still work.


### Beta-Testing

When you are ready for beta-testing, publish your game as normal, but ensure it is "Unlisted". This is important as the rules of IfComp mean your game will be disqualified if it is released opening before the competition. An unlisted game will not appear on the Text Adventurers web site, but you will be able to give your tests a link to the game.

Have a statement at the start of your game explaining that this is a beta version, what version it is and how testers can send you comments. When you upload a new version, make sure you have changed the text. Hopefully your testers will tell you which version they were looking at.

Obviously when you release, this text should be modified or deleted!


### Several Rounds, Many Testers

You should assume you will be releasing around three beta-versions, each improving on the previous. It may be a good idea to get new testers at each round of testing.

Ask on the Quest forum, you might also want to ask people outside the Quest community too (especially for a competition entry), [start here](http://www.intfiction.org/forum/viewforum.php?f=19).

### Credit Your Testers

Remember to credit your testers; they worked hard too. If this is for a competition, you may get a lower score if you do not, as people will assume the game is untested.

Further Reading
---------------

A good good article for further reading: <http://www.xyzzynews.com/xyzzy.18d.html>

A useful forum thread: <http://www.intfiction.org/forum/viewtopic.php?f=32&t=6205>

Not so good, but may be interesting: <http://inky.org/if/great-games.html>

See also this web page about beta-testing: <http://ifwiki.org/index.php/Beta-testing>
