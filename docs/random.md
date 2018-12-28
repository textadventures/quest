---
layout: index
title: Randomisation
---


There are a number of reasons why you may want to add a degree of randomness to your game. You might want to add some variation to a description, perhaps to make the world feel more a live and dynamic. This is especially true of a description the player will see numerous times, such as the description of a hub room or of an NPC who follows the player.

Randomness is very useful in RPG-type games when you want to determine the outcome of an event such as an attack.

You might also want to randomly generate descriptions of cloned objects so they do not all seem to be the same.

Quest has a suite of functions to allow these things. We will look first at what is available, and then at some examples of them in use.

Random functions
---------------

### Text processor

Not really a function, but the easiest to use. As with all [text processor](text_processor.html) directives, this is embedded in a string. The directive is called "random", and will select one text from the following list.

Here is a simple example. When the text is printed, Quest will randomly select one of "blue", "red" or "yellow".

> It was a {random:blue:red:yellow} flower.

It is worth emphasising that the colour is picked randomly every time the text is printed. If the player looks again, she may well find the flowers have changed colour. We will look at a solution to that in the examples.


### Get functions

The basic random functions get either an `int` or a `double`.

The `GetRandomDouble` function takes no parameters, and returns a value between 0.0 and 1.0. 

The `GetRandomInt` function takes two integer parameters, and will return a random number between (and including) those two numbers.

Here is a simple example that will randomly print 1, 2 or 3 ten times.

```
for (i, 1, 10) {
  msg(GetRandomInt(1, 3))
}
```


### RandomChance

The `RandomChance` function takes one integer parameter, between 0 and 100, and will return a Boolean. It will return `true` a percentage of the time equal to the number given.

```
// Always successful
success = RandomChance(100)

// Usually successful
success = RandomChance(75)

// Successful half the time
success = RandomChance(50)

// Successful only one time in 50
success = RandomChance(2)

// Always fail
success = RandomChance(0)
```

### Pick one

Quest has a number of functions that make it easy to randomly pick one example from a list:

```
PickOneChild
PickOneChildOfType
PickOneExit
PickOneObject
PickOneString
PickOneUnlockedExit
```

The `PickOneChild` and `PickOneChildOfType` functions return a random object from the given room or container. `PickOneExit` and `PickOneUnlockedExit` obviously pick an exit from the given room (useful for randomly moving NPCs). `PickOneObject` needs to be given an object list. `PickOneString` can be given a string list or a semi-colon separated string.


### Roll dice

The `RollDice` function takes a string in the standard RPG format, eg "d6+1" and "3d8-2", and returns an integer, the result of rolling the given dice.

A great example of this in use would be calculating damage for a weapon. Each weapon could be given a damage string in this format, so dagger might be "d4" and great sword could be "3d6+2". Damage after a successful attack can then be determined:

```
hits_lost = RollDice(weapon.damage)
```

Examples of use
----------------

### Selecting one option

Let us suppose you want to randomly pick one of three (or more) things. You might think you could do this:
```
if (RandomChance(33)) {
  msg("Event three happens")
}
else if (RandomChance(33)) {
  msg("Event two happens")
}
else {
  msg("Event one happens")
}
```
Event three will be selected a third of the time - so far so good. It will not be selected two thirds of the times, and if it is not, then there is a third of a chance that event two will be picked. But that is not quite what we want; that means the chance of event two is only 2/3 times 1/3, i.e. 2/9, not 1/3.

The way to approach this is to consider that if event three has not happened, then if we want events two and one to be equally likely, then there has to be a fifty percent chance of each:

```
if (RandomChance(33)) {
  msg("Event three happens")
}
else if (RandomChance(50)) {
  msg("Event two happens")
}
else {
  msg("Event one happens")
}
```
You could even think of it like this, though the `RandomChance(100)` is entirely unnecessary:
```
if (RandomChance(33)) {
  msg("Event three happens")
}
else if (RandomChance(50)) {
  msg("Event two happens")
}
else if (RandomChance(100)) {
  msg("Event one happens")
}
```
At the start there are three options to pick between, so the first is 33%. When there are only two left, it is 50% for each. When only one, there is 100% for it.

What if you have multiple events? The general formula is to count how many options are left at each point; the percentage chance is 100 divided by that. Here there are seven events. The first has a probability of 100/7. For the second, there are six remaining, so the probability is 100/6.

```
if (RandomChance(14)) {
  msg("Event seven happens")
}
else if (RandomChance(17)) {
  msg("Event six happens")
}
else if (RandomChance(20)) {
  msg("Event five happens")
}
else if (RandomChance(25)) {
  msg("Event four happens")
}
elseif (RandomChance(33)) {
  msg("Event three happens")
}
else if (RandomChance(50)) {
  msg("Event two happens")
}
else {
  msg("Event one happens")
}
```
An alternative (and conceptually simpler) approach is to use `GetRandomInt` and a `switch` statement.
```
switch (GetRandomInt(1,7)) {
  case (1) {
    msg("Event seven happens")
  }
  case (2) {
    msg("Event six happens")
  }
  case (3) {
    msg("Event five happens")
  }
  case (4) {
    msg("Event four happens")
  }
  case (5) {
    msg("Event three happens")
  }
  case (6) {
    msg("Event two happens")
  }
  case (7) {
    msg("Event one happens")
  }
}


### Permanent values for descriptions

The text processor offers a very simple randomisation technique, but the fact that it gets randomised when printing means you can get a different result each time, and sometimes that is not what you want. Fr example, if you are creating clones, and want to randomise the descriptions, you want the description of a specific clone to always be the same.

The trick is to process the text when the clone is created rather than when the player looks at it. Here is a very simple example:

```
clone.look = ProcessText("The alien has {random:red:blue:yellow} skin.")
```

It is probably more convenient to have the description in the prototype, and to process that when the clone is created.

```
clone.look = ProcessText(prototype.look)
```

If the attributes of the clone change randomly, you will need to have the description depend on those attributes. Here is a more involved example:

```
clone.size = GetRandomInt(0,2)
clone.weapon = CloneObjectAndMove(PickOneChild(weapons), clone)
clone.weapon_name = GetDisplayName(clone.weapon)
game.text_processor_this = clone
clone.look = ProcessText("The {select:this.size:big:huge:enormous} alien has {random:red:blue:yellow} skin, and is armed with {this.weapon_name}.")
```

The first line sets the clone's size. The second line gives him a weapon. This is picked at random from a room called "weapons" that the player would not have access to, and is just a stock of weapons. The selected weapon is cloned and moved to the alien. For the third line, we grab the name of the weapon.

To allow the text processor to cope with `this`, we set `game.text_processor_this` next, as `clone` is just a local variable; we do not know what the name of the clone will be when the game is running (each clone will have its own name).

Once we have all that set up, we can set the "look" attribute. The size uses the "select" directive, so is determined by `clone.size`.

Now we have a clone with a description that matches its attributes (this is also discussed on the [clones](clones.html) page).

