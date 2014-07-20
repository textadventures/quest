---
layout: index
title: Competition Entry
---

Competitions are a good way to reach a wider audience for your Quest adventure, but you better be prepared to be judged harshly...

Probably the most significant Interactive Fiction competition is [IfComp](http://www.ifcomp.org/), run during October each year, and this page is mostly geared towards that. It will not guarantee your game is a winner, but hopefully will improve its ranking to some degree.

Who Am I?
---------

Why should you take my advice? Well, my game The Myothian Falcon was placed 24th in IFComp 2011 (hey, it was high enough to get offered a prize), and I beta-tested three out of the top five games from IFComp 2012, including the winner, so I have at least *some* experience. That said, obviously, this is just my opinion. If you think otherwise or have more ideas, then add your comments, have your say.

### The MetaLib Library

I suggest you include this library, which will save some work for you. Its usefulness will become clear as you read down this page. You will probably need to right click on the link, and save it that way, and you may find your browser adds a .html extension, which will need to be removed before Quest will recognise it.

[MetaLib.aslx]({{site.baseurl}/files/MetaLib.aslx)

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

IFComp requires a walk-through to prove the game is winnable, but a long list of commands is actually pretty useless to the player. If you use the MetaLib library, this includes an alternative walk-though mechanism. Create an attribute called "walkthru" on the game object, and set it to be a string list. Each entry in your string list should be a descriptive step towards completing the game. If the player types WALKTHROUGH, he will get a list of steps in "invisiclues", i.e., dragging the mouse across them will make them appear.

### About

Include an "about" command, so you can tell people who wrote the game, and give thanks to anyone who helped you. Include a version number.

If you use the MetaLib library, this includes an ABOUT command. Create an attribute called "about" on the game object, and set it to be a string list. Each entry in your string list will be a paragraph when the player types ABOUT. The name, author and version will appear automatically (but remember to set these values for the game object).

Credit beta-testers here; you may be marked down otherwise.

### Implement Everything

Every object mentioned in the text should be implemented as an object that can be looked at in the game. Also aim to implement all the common commands such as "jump", "xyzzy", etc., even if they are not relevant. Default and error responses are *bad*.

### Feelies

Some games include feelies. Back in the day, commercial adventure games included posters, comic books, scratch-and-sniff cards, etc. to limit piracy as much as anything. Nowadays, these feelies are virtual... so you cannot actually feel them. Nevertheless, they seem to be popular, and can help to give a game a profession touch.

Unfortunately, it is easy for players to miss feelies; if they play on line, they just will not know they exist. Happily Quest handles this well, as you can insert images, videos and audio right into your game, and Quest 5.3 has support for cover art built in.

### Other Considerations

It has been suggested that you might be better submitting your game as an "on-line game" rather than a "Quest game", as Quest does not have a great reputation. I doubt many competition judges will down load Quest to play your game off-line, and the play on-line web page does make it clear that you can download the game anyway, so might be worth considering.

It has also been said that you are better off turning off hyperlinks and the right hand panes, and not using menus; giving the player a much more traditional game experience. I think there is something in this, some people will be turned off by the unconventional look of the game, but personally I would keep these features. I think if done well they enhance the game experience (that is the point of them), and more people will appreciate that than will dislike it (but that is just my guess).

Testing
-------

Beta-testing is getting other people to play your game before you release it. It is absolutely vital; with the best will in the world, they are sure to find spelling mistakes, objects you have not implement, verbs you have not thought of, routes through the game you have not considered. Better these things are found during beta-testing than after a release, and especially during a competition.

### Before Beta-Testing

It is tempting to get the game to beta-testers fast, but you are really just wasting their time and yours if you know there are problems before sending it. So:

1. Play the game through and correct any mistakes you can find.

2. Back up the game, then copy the entire code into a word processor; check and correct spelling, paste back into Quest. Check it still plays okay. Do this with custom libraries too if they include text the player will see.

3. Check:

-   Every room and object has an alias (if required) and a description
-   Everything mentioned in a description is actually implemented
-   The appropriate display and inventory verbs are there, and inappropriate ones are absent

If you are using MetaLib, start playing your game, and type STATS, to get a list of rooms and objects with no alias or description.

### Making Beta-Testing Easy

A few simple steps will make testing easier for both you and the testers.

1. Allow comments.

Testing is easiest if the player can just type stuff into Quest, but you really do not want Quest to try to parse that as a command. Add this command, and Quest will understand anything that starts with a \* as a comment (it should appear in the code after all other commands).

      <command name="comment">
        <pattern type="string"><![CDATA[^\*]]></pattern>
        <script>
        </script>
      </command>

If you include the MetaLib library this is already done for you.

2. Add "Boiler Plate" text

Have a statement at the start of your game explaining that this is a beta version, and how testers can add comments.

If you include the MetaLib library, you can invoke the function "BoilerPlate" in your start-up script, and when the game starts the player will get a brief text about the version and who the author is. If the version includes the word "beta", the player will also see text about beta-testing the game. Remember to remove the word "beta" when you release the game.

3. Update the version number

The introductory text will tell you, the author, which version the player was using, but it is up to you to change the version number with each beta release. If you release updates during the competition, again remember to update the version (and beta-test each update!). If this is a beta release, include the word "beta" in the version (the function *IsBeta* in MetaLib will then say if this is a beta version or not).

4. Load to the Quest web site

You can have beta-versions on the Quest web site, but make sure you flag it as unlisted (note: if it gets listed you will not be allowed to enter it into a competition). Write a comment there that this is a beta-version. Having it on the web site allows people without Windows to test it, and means players do not need to download Quest. Most players will see it this way, so this is the version that has to be perfect.

5. Transcripts

Ask Alex to save transcripts of the game, and send them to you (there is a plan to make this automatic).

6. Tell testers: *Just play and comment*

Tell potential testers all they have to do is play the game and type comments. No need to download anything, no need to send anything.

### Several Rounds, Many Testers

You should assume you will be releasing around three beta-versions, each improving on the previous. It may be a good idea to get new testers at each round of testing.

Ask on the Quest forum, you might also want to ask people outside the Quest community too (especially for a competition entry), [start here](http://www.intfiction.org/forum/viewforum.php?f=19).

### Credit Your Testers

Remember to credit your testers; they worked hard too. If this is for a competition, you may get a lower score if you do not, as people will assume the game is untested.

If you use MetaLib, use the "about" attribute of the game object as described above.

Further Reading
---------------

A good good article for further reading: <http://www.xyzzynews.com/xyzzy.18d.html>

A useful forum thread: <http://www.intfiction.org/forum/viewtopic.php?f=32&t=6205>

Not so good, but may be interesting: <http://inky.org/if/great-games.html>

See also this web page about beta-testing: <http://ifwiki.org/index.php/Beta-testing>
