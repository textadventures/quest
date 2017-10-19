---
layout: index
title: The UI and Game-play
---

Quest offers a number of options for the player to interact with the game:

- Command bar
- Hyperlinks
- Game panes on the right

Before releasing your game, you should consider if all these are applicable to the game.

Command bar
-----------

The command bar is the traditional input method for interactive fiction. It offers the most flexibility to the player, creating a great sense (or illusion at least) of freedom. At the same time, this puts extra demands on the creator, as she has to anticipate all reasonable commands. If an object is mentioned in a room description, many players will expect to be able to look at it. You will also need to think of all possible synonyms for objects and verbs.

However, if you decide to turn off the command bar, you need to address the limitations of hyperlinks and the game panes. By default, they can only handle moving the player to another room and simple VERB OBJECT commands. How will the player do stings like LOOK, WAIT, PUT BALL IN SACK and ATTACK ORC WITH FIREBALL?

The command bar can be turned off or customised on the _Interface_ tab of the game object.


Hyperlinks
----------

Hyperlinks are the bread-and-butter of hypertext books, and Quest allows you to build games that are entirely navigated by such link, but still has a sophisticated world model (i.e., objects and rooms existing in a meaningful relationship to each other).

Quest will create hyperlinks for you. In object lists, each object will be given a link, that will show a list of appropriate options. In the exits list, each exit again will be a hyperlink.

In addition, you can use text processor commands to add your own link. Text processor commands are indicated by curly braces, with the sections separated by colons.

> If you would like help, click {command:HELP:here}.

> Perhaps you could {command:PUT BALL IN SACK:put the ball in the sack} 

The text processor command in this case is called "command", so that is the first section. The next part I put it in capitals, but it does not have to be; this is the actual command, what the player would type into the command bar. This can be as complicated as you like - just as long as Quest can understand it. The last bit is the text the player sees.

Hyperlinks can be turned off and customised from the _Display_ tab. You can give objects their own individual hyperlink colour on their _Object_ tab.


Game panes
-----------

The game panes are an alternative to hyperlinks, and may be more appropriate if you do not want your text interrupted by underlining and different colours. The compass also gives a quick indication of what exits are available. As with hyperlinks, Quest will list the appropriate verbs for an object.

The game panes can be turned off or customised on the _Interface_ tab of the game object.

An additional pane can be added for simple commands. To set the commands, you need to use the JavaScript function `setCommands`, though it is not as difficult as it sounds. Here is an example that will set LOOK and WAIT in the pane. Note that the commands are separated by semi-colons. The second form shows how to set the colour of the text.
```
JS.setCommands("Look;Wait")

JS.setCommands("Look;Wait", "blue")
```
This could be done in the start script of the game object, but you could have the displayed commands update according to the situation. If the player goes into a room with a throne, you could have "Sit" displayed, for example, and then have "Stand" displayed instead when the player is sat on the throne.


Further consideration
---------------------

It can be easier to create puzzles for a game using the command bar, as it is far less obvious to the player what to do at a certain point (in contrast to randomly linking links until something works). This can also lead to the "guess the verb" problem, where the player is trying to work out what obscure phrase the game is expecting next.

If you choose to have the command bar in addition to either hyperlinks or the game panes, be aware that some players may assume they can complete the game using exclusively one or the other.

