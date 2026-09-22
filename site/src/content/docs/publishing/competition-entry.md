---
title: Competition entry
description: Entering IFComp and other interactive fiction competitions with a self-contained Quest Viva game
---

Competitions are a good way to reach a wider audience for your Quest Viva adventure, but be prepared to be judged harshly.

The most significant interactive fiction competition is [IFComp](https://ifcomp.org/), run during October each year, and this page is mostly geared towards that. Following it will not guarantee your game is a winner, but it should improve its ranking.

## Your entry is a web page

Most IFComp entries are now HTML: a judge opens the game in a browser and plays. Quest Viva can produce exactly that. Publish a **Zip with player included** (see [Publishing your game](/publishing)) and you have a self-contained entry with `index.html` at the top - your game, the Quest player, and nothing fetched from anywhere else. Judges need no account, no download and no knowledge of Quest.

That removes what used to be the awkward part of entering a Quest game, which was that judges had to go to textadventures.co.uk or install something to play it. IFComp does still list `.quest` among its formats, but describes it in terms of the old Windows Quest interpreter. Submit the HTML zip instead.

Two things worth setting on the game's [_Setup_ tab](/publishing/game-details) before you export:

- **Cover art.** A 512×512 PNG. Competitions and catalogues display it next to your entry, and a game without one looks unfinished in a list of eighty.
- **Game ID.** This is your game's IFID, generated for you, and it is what IFDB and other catalogues use to identify the game. Leave it alone - don't generate a new one between the comp build and the post-comp release, or the two stop being the same game.

## Starting out

Before you start creating, think about your game.

### Time

IFComp's rules say judges "must base their judgment of each game on at most the first two hours of play". You can write a game that takes longer - plenty of entries do - but anything beyond the two-hour mark is not what you are being scored on. In practice that means the first two hours have to stand on their own: reach something satisfying, and don't leave the best part until afterwards. Aim for a 1.5 to 2 hour playing time unless you have a reason not to.

The rule exists because there are a lot of entries - recent competitions have had 70 to 85 - and judges have limited time.

### Originality

Try to create a game that stands out from the crowd, something with a novel hook to it. Perhaps easier said than done, but take a look at previous winners for inspiration. Talking of which...

### Easy puzzles

Do not make the puzzles too tricky. With only two hours of judged play, a player stuck on one puzzle might not see half your game. Of course you need *some* challenges; the trick is getting the balance right.

### Compare to other entries

Take a look at some other entries from previous years, and see what works and what does not. See what the standard is. Just as important, read the reviews and see what the judges think worked and what did not. Think about whether common criticisms might also apply to your own work, and modify it accordingly.

## Implementation

### Help, hints and walkthrough

Include some in-game system to help players get to the end. They only have two hours, and if they are stuck on a puzzle with no way to cheat they will not see the end of your game. Make sure the clues are both good (easy to follow) and comprehensive (covering every possible problem).

IFComp requires a walkthrough to prove the game is winnable, but a long list of commands is pretty useless to a player. A walkthrough that tells the player what to do, rather than what to type, will get them to the end and hopefully let them still enjoy the trip.

### About

Include an "about" command, so you can tell people who wrote the game and give thanks to anyone who helped you. Include a version number.

Credit beta-testers here; you may be marked down otherwise.

### Implement everything

Every object mentioned in the text should be implemented as an object that can be looked at in the game. Also aim to implement all the common commands such as "jump" and "xyzzy", even if they are not relevant. Default and error responses are *bad*.

Or submit a game without a command line, such as a gamebook.

### Feelies

Some games include feelies. Back in the day, commercial adventure games included posters, comic books, scratch-and-sniff cards and so on, to limit piracy as much as anything. Nowadays feelies are virtual, so you cannot actually feel them. Nevertheless they are popular, and can give a game a professional touch.

Unfortunately it is easy for players to miss feelies; if they play online, they may not know they exist. Quest Viva handles this well, as you can insert images, videos and audio right into your game, and it has support for cover art built in.

### The user interface

Bear in mind that the vast majority of players will be playing online.

Think carefully about what elements of the user interface you want to include. By default, Quest Viva gives you a command line, hyperlinks in the text and the panes on the right. Are they all appropriate to your game?

Turning off the command bar makes the game much easier to build, because you very much limit what the player can do, but at the cost of destroying the illusion of freedom. For a traditional game you might prefer to have only the command line.

Also think about the colours and the font. Pick a font that reflects the style of your game and is easy to read.

The important message is to think about the interface and make a choice that is right for your game, rather than just using the Quest Viva defaults.

## Testing

Beta-testing is especially important for a competition entry, since you only get one shot at a good first impression from the judges. See [Beta-testing](/publishing#beta-testing) for the general process - the before-testing checklist, how to publish an unlisted test version, and crediting testers. A few things are specific to a competition entry:

**Keep it unlisted.** IFComp's rules mean your game will be disqualified if it is released publicly before the competition, so if you upload test builds to textadventures.co.uk, double-check that "Who can access this game?" stays on "Only people I give the link to" throughout. There is no "private" setting to look for - unlisted is what keeps the game reachable by your testers and nobody else. If you send testers an HTML build hosted somewhere of your own, keep the address unadvertised and out of search engines.

**Explain that it's a beta.** Put a statement at the start of the game saying that this is a beta version, which version it is, and how testers can send you comments. Update the text with each new version, so testers can tell you which one they were looking at. Remember to remove it before the real release.

**Look beyond the Quest community for testers.** Ask on the [textadventures Discord](https://textadventures.co.uk/community/discord), but for a competition entry it is worth asking outside the Quest community too - the [intfiction.org forums](https://intfiction.org/c/authoring/8) are where most IF authors find testers.
