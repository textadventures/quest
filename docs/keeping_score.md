---
layout: index
title: How to Keep Score
---

Many years ago I wrote a library to help track the player's score. It was pretty simple, but worked well, and five years later on I cannot think of any way to improve. So, I am going drop it altogether! You do not need a library to do this, instead, I present a tutorial. The advantages of a tutorial are that you will learn more of Quest coding whilst following it, and it will be useable by anyone using the on-line editor too.

This system will not just keep score, it will also allow the player to see a list of achievements, and give her a rank. The on-going score will appear in the status panel.

## Before We Begin

Just for the sake of testing, we will set up a new command, with the pattern JUMP, and paste in this code:
```
  msg("You jump into the air. Hurrah!")
```
The plan is to have the player get 1 point for jumping, using this command.

By the way, quest does have a "score", which will do some of this for us. I am not going to use that because I want to show how status attributes are used.

Status Attributes - Off-line
------------------------------

Go to the _Attributes_ tab of the game object. In the lower box, click _Add_, then type "score" and set it to an integer. Then go to the upper box, marked "Status Attributes", click _Add_, and again then type "score". You will get a second box this time, in it, paste in this:
```
  Score: !/10
```

Status Attributes - On-line
----------------------------

on-line we have no _Attributes_ tab, so we have to do this in the start scrip. Go to the _Scripts_ tab of the game object and paste in this to the start script at the top:

```
  game.score = 0
  game.statusattributes = NewStringDictionary()
  dictionary add (game.statusattributes, "score", "Score: !/10")
```

The first line is obviously setting up the "score" attribute.

Quest stores information about status attributes in dictionary attributes called `statusattributes`, on the game and player objects, and the second line creates one on the game object (Quest does this automatically in the off-line editor). The third line adds one entry to that dictionary. It has two parts, the name of the attribute, "score", and the display format, "Score: !/10".


Display Format
---------------

However you set it up, your "score" status attribute should have a display format like this:

```
  Score: !/10
```

The exclamation mark is a placeholder for the actual value, so it the score is currently zero, the player will see:

> Score: 0/10


Scoring a Point...
------------------

No go back to the JUMP command, and change its script to this:

```
  msg("You jump into the air. Hurrah!")
  game.score = game.score + 1
```

You should now be able to go into the game, and see your score, and see that it goes up when you jump.


... And Only One Point
---------------------

In fact the score goes up every time you type JUMP; really we only want that to happen once. We will build in a system to ensure that that is the case later, but let us do it a different way first. Did you know you can set attributes on commands? Paste in this code:

```
  msg("You jump into the air. Hurrah!")
  if (not GetBoolean(this, "alreadydone")) {
    game.score = game.score + 1
    this.alreadydone = true
  }
```

This will check the "alreadydone" on the command itself, and only increase the score if it is not yet set. By the way, "this" indicates the thing the script belongs to - the command in this case.


Listing Achievements
--------------------

So we have a simple system, and that may be enough for you. However, we can improve the system to ensure the player only every gets rewarded once for any achievement, and to allow the player to check what she got points for. To do that, we will record each achievement in a string list, so the first step is to set that up. If on-line, add this to the start script of the game object:
```
  game.score_achievements = NewStringDictionary()
```
If off-line you might prefer to add a string list attribute to the game object called "score_achievements" instead.

This string list will keep a list of the achievements, and we can check that to see if an achievement has already been done. Best way to do that is in a function. God to _Functions_, and click _Add_. Give it the name "IncScore", and leave its return type to _None_. You will need to add two parameters, `str` and `inc`, and then past in this code:

```
  if (not DictionaryContains(game.score_achievements, str)) {
    dictionary add (game.score_achievements, str, ToString(inc))
    game.score = game.score + inc
    msg("Your score went up by " + inc + "!")
  }
```

What we will be sending this function is a string and a number. It will check if the string is already in the dictionary, and if it is not, it will get added, the score will be increased and the player informed.

Now go back to your JUMP command, and delete the exiting script, pasting in this instead:

```
  msg ("You jump into the air. Hurrah!")
  IncScore("You jumped", 5)
```

If you go into the game, you will find that you get 5 points for jumping, but only the first time.

You could have several things giving the same reward. Perhaps leaping a chasm also counts as a jump; in your LEAP command, just have exactly the same line of code. The score system will check whether "You jumped" is already in the list, and only hand out points if it is not.


A SCORE Command
----------------

We can add a score command, to allow the player to see how much she has score and for what. Create a new command, and give is the pattern "score", then paste in this code:

```
  msg ("You have scored " + game.score + ".")
  foreach (s, game.score_achievements) {
    msg ("- " + s + " (" + DictionaryItem(game.score_achievements, s) + ")")
  }
```

The first line gives the total score, the next sets up a loop, going through the dictionary with the achievements listed. For each one, the achievement, plus points rewarded, is displayed.