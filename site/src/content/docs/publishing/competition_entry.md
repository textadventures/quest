---
title: Competition entry
sidebar:
  order: 1
---

Competitions are a good way to reach a wider audience for your Quest adventure, but you better be prepared to be judged harshly...

Probably the most significant Interactive Fiction competition is [IfComp](http://www.ifcomp.org/), run during October each year, and this page is mostly geared towards that. It will not guarantee your game is a winner, but hopefully will improve its ranking to some degree.

## Starting out

Before you start creating, think about your game.

### Time

IFComp requires that games can be played in two hours. This is a practical necessity with around 30 games submitted each year, that adds up to 60 hours of playing times for the judges. They do not have time for longer games. Aim for a 1.5 to 2 hour play time.

### Originality

Try to create a game that stands out from the crowd, something with a novel hook to it. Perhaps easier said than done, but take a look at previous winners to see what I mean. Talking of which...

### Easy puzzles

Do not make the puzzles too tricky. With only two hours playing time, if a player gets stuck on one puzzle, she might not find half your game. Of course, you need *some* challenges, the trick is to get the balance right.

### Compare to other entries

Take a look at some other entries from previous years, and see what works and what does not. See what the standard is. Just as important, read the reviews and see what the judges think worked and what did not. Think if common criticisms might also apply to your own work, and modify it accordingly.

## Implementation

### Help, hints and walk-though

Include some in-game system to help players get to the end. They only have two hours and if they are stuck on a puzzle with no way to cheat, they just will not see the end of your game. Make sure the clues are both good (easy to follow) and comprehensive (cover all possible problems).

IFComp requires a walk-through to prove the game is winnable, but a long list of commands is actually pretty useless to the player. Providing a walk-through that tells the player what to do, rather than what to type, will ensure they can get to the end and hopefully still enjoy the trip.

### About

Include an "about" command, so you can tell people who wrote the game, and give thanks to anyone who helped you. Include a version number.

Credit beta-testers here; you may be marked down otherwise.

### Implement everything

Every object mentioned in the text should be implemented as an object that can be looked at in the game. Also aim to implement all the common commands such as "jump", "xyzzy", etc., even if they are not relevant. Default and error responses are *bad*.

Or submit a game without a command line, such as a CYOA or gamebook.

### Feelies

Some games include feelies. Back in the day, commercial adventure games included posters, comic books, scratch-and-sniff cards, etc. to limit piracy as much as anything. Nowadays, these feelies are virtual... so you cannot actually feel them. Nevertheless, they seem to be popular, and can help to give a game a profession touch.

Unfortunately, it is easy for players to miss feelies; if they play on line, they just will not know they exist. Happily Quest handles this well, as you can insert images, videos and audio right into your game, and Quest has support for cover art built in.

### The user interface

Bear in mind that the vast majority of players will be playing on-line, so bear that in mind.

Think carefully what elements of the user interface (UI) you want to include. By default, Quest includes a command line, hyperlinks in the text and the panes on the right. Are they all appropriate to your game?

Turning off the command bar will make it much easier to build your game, as you very much limit what the player can do, but at the cost of destroying the illusion of freedom for the player. For a traditional game, you might prefer to have only the command line.

Also think about the colours and the font. Be sure to pick a font that reflects the style of your game, and is easy to read.

The important message here is to think about the UI, and make a choice for what is right for your game, and not just use the Quest defaults.


## Testing

Beta-testing is especially important for a competition entry, since you only get one shot at a good first impression from the judges. See [Beta-testing](/publishing/publishing#beta-testing) for the general process - before-testing checklist, how to publish a private test version, and crediting testers. A few things are specific to a competition entry:

**Keep it "Unlisted", not just private.** The rules of IfComp mean your game will be disqualified if it is released publicly before the competition, so double-check its visibility stays "Unlisted" throughout testing.

**Explain that it's a beta.** Have a statement at the start of your game explaining that this is a beta version, what version it is, and how testers can send you comments - update the text with each new version so testers can tell you which one they were looking at. Remember to remove or update this text before the real release.

**Look beyond the Quest community for testers.** Ask on the Quest forum, but especially for a competition entry, it's worth asking people outside the Quest community too - [start here](http://www.intfiction.org/forum/viewforum.php?f=19).