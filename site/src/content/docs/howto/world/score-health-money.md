---
title: Score, health and money
description: Turn on the built-in score, health and money features, change them from scripts, and award points only once
---

Quest Viva has three built-in features for the numbers most games keep track of. This page shows how to turn them on, change them from your scripts, and build a list of achievements that each award points only once.

| Feature | Belongs to | Starts at | Status pane shows |
|---|---|---|---|
| Score | the `game` object | 0 | Score: 5 |
| Health | the player object | 100 | Health: 100% |
| Money | the player object | 0, or the player's "Starting money" | Money: $20 |

## Turning them on

Select the `game` object, go to the _Features_ tab and tick "Score", "Health" and/or "Money". Each one you tick is added to the status pane automatically, if your game shows the panes on the right. For other values you want to show there, see [Status attributes](/status-attributes).

## Changing the values

Ticking a feature adds its script commands to the "Player" category when you add a script:

![](/images/increase_decrease.png)

Each command takes the amount to add or subtract. Here is "Increase score", set to add 5 to the score:

![](/images/increase.png)

In code, these are:

```quest
IncreaseScore (5)
DecreaseScore (5)
IncreaseHealth (10)
DecreaseHealth (10)
IncreaseMoney (20)
DecreaseMoney (20)
```

These change `game.score`, and `health` and `money` on the current player object, `game.pov`. You can also read or set the attributes directly:

```quest
game.score = game.score + 5
game.pov.health = game.pov.health - 5
msg ("You have " + game.pov.money + " coins.")
```

If a feature isn't ticked, its attribute doesn't exist. The script commands won't appear in the editor, and calling one of the functions in code gives an error that tells you to tick the feature.

## Score

The score belongs to the `game` object, so it stays the same if the player [switches to a different character](/howto/tasks/changing-the-player-object). It starts at zero.

There is no built-in SCORE command for the player to type - the status pane shows the score. The next two sections show how to add one.

### Awarding points only once

`IncreaseScore` adds points every time it runs, so a command like JUMP would score again each time the player typed it. For points that belong to a single place in your game, wrap the command in "First time..." (in the "Scripts" category):

```quest
msg ("You jump into the air.")
firsttime {
  IncreaseScore (5)
}
```

`firsttime` runs its script only once in the whole game.

### A list of achievements

When the same achievement can be earned in more than one way - leaping the chasm and jumping over it, say - `firsttime` isn't enough, because each script has its own "first time". Instead, give each achievement a name and keep a record of the ones the player has earned.

Select "Advanced" in the tree and click "Add Function". Call it `AwardPoints` and give it two parameters, `achievement` and `points`. Leave "Return type" as it is, and give it this script:

```quest
if (not HasAttribute(game, "achievements")) {
  game.achievements = NewStringDictionary()
}
if (not DictionaryContains(game.achievements, achievement)) {
  dictionary add (game.achievements, achievement, ToString(points))
  IncreaseScore (points)
  msg ("[Your score has gone up by " + points + ".]")
}
```

The function stores each achievement in a string dictionary on the `game` object, creating the dictionary the first time it's needed. If the achievement is already in the dictionary, nothing happens.

Now call the function wherever the player earns points, with "Call function" in the "Scripts" category, or in code:

```quest
msg ("You leap across the chasm.")
AwardPoints ("Crossing the chasm", 5)
```

Every script that awards "Crossing the chasm" can call `AwardPoints` with the same name, and the player only gets the points once. For more about functions and parameters, see [Functions](/howto/tasks/about-functions).

To let the player see what they've scored for, add a command with the pattern `score; full score; full` and this script:

```quest
msg ("Your score is " + game.score + ".")
if (HasAttribute(game, "achievements")) {
  foreach (achievement, game.achievements) {
    msg (DictionaryItem(game.achievements, achievement) + " for " + achievement)
  }
}
```

This gives output like:

```
> full score
Your score is 6.
1 for Jumping for joy
5 for Crossing the chasm
```

## Health

Health belongs to the player object - if you have [more than one player object](/howto/tasks/changing-the-player-object), each has its own health, and the status pane shows the current one's. Health is a percentage. It starts at 100, can't go above 100 and can't go below 0: if a script sets it higher or lower, Quest Viva brings it back into range.

Health is always 100 at the start of the game (and when another player object becomes the player for the first time), even if you've given the player object a `health` attribute. To start lower, set it in the game's start script, on the _Scripts_ tab of the `game` object:

```quest
player.health = 60
```

To set what happens when health reaches zero, go to the `game` object's _Player_ tab and fill in "Script to run when health reaches zero". Usually you'll print a message and end the game:

![](/images/you_died.png)

```quest
msg ("You died!")
finish
```

Food, potions and the like can restore health. Make the object edible, then set "Change health by" on its _Edible_ tab - see [Items that can be eaten](/howto/world/edible).

If you need a different maximum, such as hit points that go up as the player gains levels, don't tick "Health". Use your own attribute with a different name, and show it with a [status attribute](/status-attributes).

## Money

Money belongs to the player object, so each player object has its own. It starts at zero, or at the "Starting money" you set on the player object's _Player_ tab. Nothing stops money going below zero, so check the player can afford something before taking the money.

### How money is shown

Set "Format for money" on the `game` object's _Player_ tab. The `!` stands for the amount, so if the player has 235:

| Format | Shows |
|---|---|
| `$!` (the default) | $235 |
| `! gold` | 235 gold |

The status pane uses this format, and so does the `DisplayMoney` function, which you can use in your own messages. The format can also control decimal places, thousands separators and negative amounts - see [DisplayMoney](/reference/functions/string#displaymoney).

### Prices

When "Money" is ticked, every object has a "Price" box on its _Inventory_ tab. Quest Viva doesn't use the price itself, but your scripts can read it as `this.price`. For example, add a "buy" verb to an object on its _Verbs_ tab, with this script:

```quest
if (this.parent = game.pov) {
  msg ("You already have it.")
}
else if (game.pov.money < this.price) {
  msg ("You can't afford it - it costs " + DisplayMoney(this.price) + ".")
}
else {
  DecreaseMoney (this.price)
  MoveObject (this, game.pov)
  msg ("You buy it for " + DisplayMoney(this.price) + ".")
}
```

"buy" and "purchase" are already understood by Quest Viva, so objects without a script just say they can't be bought.
