---
layout: index
title: Introduction to Conversations
---

This page is a guide to the issues involved, and is general to all interactive fiction.

Conversations are a thorny subject; there are several ways to present them to the player and none are accepted as "the" way to do it, or acknowledged as the best. You will need to think carefully about what is best for your game (and having some characters respond to one command, and others to another is just going to annoy your players).


SAY
---------

In the tutorial the "broadcast" method was introduced:

> SAY HELLO

The player says something to everyone present. This is potentially very difficult to implement well, as you need to have each character response to anything the player might type, from SAY YOU STINK to SAY I LOVE YOU, and ideally you would want a parser just to handle this.

It will work best if there is no chance of two characters being in the same room at a time, so it is always clear who should respond.


SPEAK TO
--------

The "speak to" method is directed to a specific character:

> SPEAK TO BORIS

This could than be handled with a stock response or by offering a list of options (Quest will accept TALK TO as a synonym of SPEAK TO).

[More here](speak_to.html)



ASK/TELL
--------

Finally, the player can specify both the person to talk to and the subject:

> ASK BORIS ABOUT KEY

> TELL MARY ABOUT LOCK

> TELL BORIS TO DANCE

[More here](ask_about.html)



How it is displayed
-------------------

You should also consider whether you want both sides of the conversation in the output. Which you chose is up to you, but it will look better if you are consistent, so I suggest deciding now, before you start typing.

> \>TALK TO BORIS
>
> 'Hi,' you say to Boris, 'can you help me find the key to this door?'
>
> 'Sure, you need to look in the bedroom.'

OR

> \>TALK TO BORIS
>
> 'Hi,' says Boris, 'do you need to find the key to this door? You need to look in the bedroom.'

OR

> \>TALK TO BORIS
>
> 'Hi, do you need to find the key to this door? You need to look in the bedroom.'





See also
--------

For a more comprehensive discussion on conversations in interactive fiction, you might like to read [this article](http://emshort.wordpress.com/how-to-play/writing-if/my-articles/conversation/)

If using the desktop version, there is a [library](https://github.com/ThePix/quest/wiki/Library:-Conversations) that uses TALK TO together with dynamic menus.
