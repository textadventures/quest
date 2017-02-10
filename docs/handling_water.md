---
layout: index
title: Handling water
---

There is more than one way to do this, but this is what I recommend. This will involve setting attributes, so you will struggle with the on-line editor, but it is certainly doable!

What we will do is to create a waterskin that can be filled and drunk from, and then set up a room with a pool of water and another with a tap.

There are also a [library](https://github.com/ThePix/quest/wiki/Liquid-Library) available.


The waterskin
-------------

So first create the waterskin, in the normal way. Give it two integer attributes, "full" and "capacity". Set them to 0 and 10 respectively. The full attribute will track how much water is in the waterskin, the capacity will be the maximum. You may want to play around with these values.

Then go to the verbs tab, and create a new verb, "fill". Paste in this code:
```
  if (this.full = this.capacity) {
    msg ("It is already full.")
  }
  else if (not GetBoolean(game.pov.parent, "watersource")) {
    msg ("No water here.")
  }
  else {
    msg ("You fill it.")
    this.full = this.capacity
  }
```
All this does is check if the waterskin is already full, then check if there is water. If all is okay, the waterskin gets filled.

You might want to give it a description like this:
```
The waterskin {if waterskin.full=0:is empty}
{if waterskin.full<>0:contains some water}.
```
The text processor is a bit limited, and you might prefer to use a script so you can say how full it is.
```
  if (this.full = 0) {
    msg ("The waterskin.")
  }
  else if (this.full = this.capacity) {
    msg ("The waterskin is full.")
  }
  else {
    msg ("The waterskin is about " + (waterskin.full  * 10) + "% full.")
  }
```

## A Source of Water

Now we will do a room with a pool of clear, fresh water. Create a room, give it a Boolean attribute, "watersource", and set it to true.Now you should be able to go in-game and fill the waterskin with water.

For a room with a tap, create the room, then create the tap as an object in it. For the tap, on the Features tab tick Switchable. On the "Switchable" tab set it to "Can be switched...". Fill in the text boxes as you like. For the first script, paste in this:
```
  this.parent.watersource = true
```
And for the second:
```
  this.parent.watersource = false
```

## Playing With Water

What else do you want to do? Well, you could empty it. Create an "empty" verb, and paste this in.
```
  if (this.full = 0) {
    msg ("It is already empty.")
  }
  else {
    msg ("You empty it.")
    this.full = 0
  }
```
Is there some point to emptying it? Perhaps there is a room with a fire, and emptying the waterskin will put the fire out. You could do that like this:
```
  if (this.full = 0) {
    msg ("It is already empty.")
  }
  else if (game.pov.parent = room_with_fire) {
    msg ("You empty it over the fire, which spits, then dies.")
    this.full = 0
    room_with_fire.fireout = true
  }
  else {
    msg ("You empty it.")
    this.full = 0
  }
```
You might want to drink from the waterskin. Add a "drink from" verb, and paste this in:
```
  if (this.full = 0) {
    msg ("It is empty.")
  }
  else {
    msg ("You take a drink from it.")
    this.full = this.full - 1
  }
```
What it does is check the waterskin is not empty, and if not reduces its contents by 1. You might want to do some other things in the second block, if the player is going to die of thirst; that is up to you to sort out!

You might want to USE the waterskin, i.e., (I guess) drink from it. USE is kind of built-in, so takes a bit of setting up. Go to the Features tab, and tick Use/Give. Then go to the Use/Give tab, and at the top, for "Use (on its own)", set the action to "Run script". This does the same thing as "drink from", so we can use that script (note that spaces are removed from the verb to make the attribute name).
```
  do(this, "drinkfrom")
```
You could have pasted the script from "drink from" in here, but this way is better in the long term. If you later decide you want to change the effects of drinking water, you only have to change it in one place, rather than remember to do both. This is a principle in software engineering called DRY (Don't Repeat Yourself).