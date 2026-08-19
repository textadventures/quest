---
title: Status attributes
sidebar:
  order: 11
---

Often you will want the player to be able to see how they are doing at a glance, perhaps to see the score or health, or how much cash they have. This can be done with status attributes.

Status attributes must be set up as ordinary attributes first. You must then tell Quest that you want these particular ones to be shown in the interface. You can do this with attributes of the player or of the game object, but not anything else in the game. We will set up a score attribute on the player object.


## Status attributes

Go to the _Attributes_ tab of the player object. In the lower box, click "Add", then type "score" and set it to an integer. Then go to the upper box, marked "Status Attributes", click Add.

We can going to give Quest two bits of information. The first is the name of the attribute, and the second is how to display it, so again type "score" for the first bit (this must be exactly as you did it before, because Quest will need to match this to the attribute). You can leave the second bit blank, and Quest will decide how to display it, but we try to do it a bit more fancy. Paste in this:
```
Score: !/10
```
The exclamation mark is a stand-in for the actual number, so when the score is zero, the player will see "Score: 0/10".

Start the game, and find that a new panel has a appeared on the right, with the score displayed!

You can use status attributes with any type of attribute (on the game or player), but it works best with numbers and strings.
