---
layout: index
title: Score Library
---

A simple module that lets the writer easily add scores to the game.

Contributed by: <span class="author">The Pixie</span>

You are free to use this library in your own games, without crediting me, as long as the library is not modified. If you do want to modify the library, then you are free to do so, but please keep some attribution to me with it, at a minimum as a comment in the XML.

#### To use

1.  In the start script on the game item, call the InitScore function with the following parameters:
    1.  The maximum (or winning) score, as an integer.
    2.  A list of ranks, separated by commas, as a string. Remember to enclose in double quotes.
    3.  A boolean, indicating if you have status attributes. Use true to indicate you have, false to otherwise (if you do not know what a status attribute is, it is probably safe to use false).

2.  When the player does something score worthy, call the IncScore function. This takes two parameters, the first an identifying string, the second the amount to increase the score by. The function will only change the score if the identifying string is not recognised; i.e., if the player does the same thing several times, only the first time will affect the score.

That is all that is required. The score will be displayed as an attribute, and if the player types “score”, she will get her score, rank and a list of achievements.

Optionally, you might prefer to modify the score command so it calls PrintScore, rather than PrintFullScore, so there is no list of achievements (you will need to give your new command a new name, but the same pattern). Optionally you may want to have your own PrintScore (say for another language). You will then require your own command that calls your new function. both of these are illustrated in Alt\_Score\_Demo, included in the download.

This should be robust enough to handle nagative scores and scores over the maximum score.

[Download](http://textadventures.co.uk/attachment/132)
[Discuss](http://textadventures.co.uk/forum/samples/topic/2627/library-keeping-score)

#### Functions

GetRank

*Returns:*The current rank of the player as a string

---

GetScore

*Returns:*The player's current score, as an integer

---

IncScore (**string** *identifying\_string*, **object** *increment*)

Increases the current score by *increment*, but only if it has not already been increased for this *identifying\_string*.

---

InitScore (**integer** *max*, **string** *ranks*, **boolean** *status*)

Initialises the scoring system (so call in the start script). The value *max* is the number of points to achieve to win, *ranks* is a list of ranks, separated by commas. Set *status* to true is you are manually adding other status attributes, false otherwise.

---

PrintFullScore

Prints out the score and rank of the player, with a list of achievements (*identifying\_string*s).

---

PrintScore

Prints out the score and rank of the player.

#### Player Commands

SCORE

Player's current score and rank are displayed.
