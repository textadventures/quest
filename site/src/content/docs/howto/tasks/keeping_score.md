---
title: How to keep score
sidebar:
  order: 11
---

Quest has a simple built-in score system you can enable on the _Features_ tab of the game object — see [Score, Health and Money](/howto/world/score_health_money). This tutorial shows how to build a more flexible custom scoring system with achievements, rankings, and a SCORE command.

Many years ago I wrote a library to help track the player's score. It was pretty simple, but worked well, and five years later on I cannot think of any way to improve. So, I am going to drop it altogether! You do not need a library to do this, instead, I present a tutorial. The advantage of a tutorial is that you will learn more of Quest coding whilst following it.

This system will not just keep score, it will also allow the player to see a list of achievements, and give her a rank. The on-going score will appear in the status panel.

## Before we begin

Just for the sake of testing, we will set up a new command, with the pattern `JUMP`, and paste in this code:
```quest
  msg("You jump into the air. Hurrah!")
```
The plan is to have the player get 1 point for jumping, using this command.

By the way, quest does have a "score", which will do some of this for us. I am not going to use that because I want to show how status attributes are used.

## Status attributes

Go to the _Attributes_ tab of the game object. In the lower box, click _Add_, then type "score" and set it to an integer. Then go to the upper box, marked "Status Attributes", click _Add_, and again then type "score". You will get a second box this time, in it, paste in this:
```
  Score: !/10
```

Alternatively, you can set this up in a script instead, for example if you would rather work in code. Go to the _Scripts_ tab of the game object and paste in this to the start script at the top:

```quest
  game.score = 0
  game.statusattributes = NewStringDictionary()
  dictionary add (game.statusattributes, "score", "Score: !/10")
```

The first line is obviously setting up the "score" attribute.

Quest stores information about status attributes in dictionary attributes called `statusattributes`, on the game and player objects, and the second line creates one on the game object (Quest does this automatically for you if you use the Attributes tab instead). The third line adds one entry to that dictionary. It has two parts, the name of the attribute, "score", and the display format, "Score: !/10".


## Display format

However you set it up, your "score" status attribute should have a display format like this:

```
  Score: !/10
```

The exclamation mark is a placeholder for the actual value, so if the score is currently zero, the player will see:

```
Score: 0/10
```


## Scoring a point...

Now go back to the `JUMP` command, and change its script to this:

```quest
  msg("You jump into the air. Hurrah!")
  game.score = game.score + 1
```

You should now be able to go into the game, and see your score, and see that it goes up when you jump.


## ... And only one point

In fact the score goes up every time you type `JUMP`; really we only want that to happen once. We will build in a system to ensure that that is the case later, but let us do it a different way first. Did you know you can set attributes on commands? Paste in this code:

```quest
  msg("You jump into the air. Hurrah!")
  if (not GetBoolean(this, "alreadydone")) {
    game.score = game.score + 1
    this.alreadydone = true
  }
```

This will check the "alreadydone" on the command itself, and only increase the score if it is not yet set. By the way, "this" indicates the thing the script belongs to - the command in this case.


## Listing achievements

So we have a simple system, and that may be enough for you. However, we can improve the system to ensure the player only every gets rewarded once for any achievement, and to allow the player to check what she got points for. To do that, we will record each achievement in a string dictionary, so the first step is to set that up - either add this to the start script of the game object:
```quest
  game.score_achievements = NewStringDictionary()
```
or add a string dictionary attribute to the game object called "score_achievements" directly, on the _Attributes_ tab.

This string list will keep a list of the achievements, and we can check that to see if an achievement has already been done. Best way to do that is in a function. Go to _Functions_, and click _Add_. Give it the name "IncScore", and leave its return type to _None_. You will need to add two parameters, `str` and `inc`, and then paste in this code:

```quest
  if (not DictionaryContains(game.score_achievements, str)) {
    dictionary add (game.score_achievements, str, ToString(inc))
    game.score = game.score + inc
    msg("Your score went up by " + inc + "!")
  }
```

What we will be sending this function is a string and a number. It will check if the string is already in the dictionary, and if it is not, it will get added, the score will be increased and the player informed.

Now go back to your `JUMP` command, and delete the existing script, pasting in this instead:

```quest
  msg ("You jump into the air. Hurrah!")
  IncScore("You jumped", 5)
```

If you go into the game, you will find that you get 5 points for jumping, but only the first time.

You could have several things giving the same reward. Perhaps leaping a chasm also counts as a jump; in your `LEAP` command, just have exactly the same line of code. The score system will check whether "You jumped" is already in the list, and only hand out points if it is not.


## A SCORE command

We can add a score command, to allow the player to see how much she has scored and for what. Create a new command, and give it the pattern "score", then paste in this code:

```quest
  msg ("You have scored " + game.score + ".")
  foreach (s, game.score_achievements) {
    msg ("- " + s + " (" + DictionaryItem(game.score_achievements, s) + ")")
  }
```

The first line gives the total score, the next sets up a loop, going through the dictionary with the achievements listed. For each one, the achievement, plus points rewarded, is displayed.