---
title: Fast travel
description: Let the player choose a destination from a menu - a bus route, a train line, teleport booths, or any room they've already visited - and optionally charge a fare
---

Once a game world gets large, walking everywhere gets tedious. This page shows you how to let the player jump straight to a destination they choose from a menu, in two ways:

| You want | Use |
|---|---|
| Travel between set places, like bus stops, stations or teleport booths, with new destinations opened up during the game | A [flag on each stop](#a-bus-route), and a CATCH BUS command |
| Travel to anywhere the player has already been | The built-in [`visited` attribute](#fast-travel-to-visited-rooms) |

Both can [charge a fare](#charging-a-fare) using the built-in money feature.

## A bus route

Each room that's a bus stop gets a flag. Select the room, and on its _Attributes_ tab add an attribute called `busstop`, make it a Boolean, and tick it. Do this for every stop the player can use from the start of the game.

Then add the command. Select "Commands" in the tree (underneath "game"), click "+ Add" and choose "Add Command". Enter the pattern `catch bus;take bus`, and paste this script into code view:

```quest
if (not GetBoolean(game.pov.parent, "busstop")) {
  msg ("No buses stop here.")
}
else {
  options = NewStringDictionary()
  foreach (stop, FilterByAttribute(AllObjects(), "busstop", true)) {
    if (stop <> game.pov.parent) {
      dictionary add (options, stop.name, GetDisplayAlias(stop))
    }
  }
  if (DictionaryCount(options) = 0) {
    msg ("The bus doesn't go anywhere else yet.")
  }
  else {
    choice = ShowMenu("Where do you want to go?", options, true)
    if (choice <> "") {
      msg ("The bus drops you off.")
      MoveObject (game.pov, GetObject(choice))
    }
  }
}
```

Here's what happens when the player types CATCH BUS at the high street stop:

```
> catch bus
Where do you want to go?
1: market square
```

The player clicks "market square" or types 1, and is taken there:

```
The bus drops you off.

You are in a market square.
```

How the script works:

- **Is this a bus stop?** "object has flag" (`GetBoolean`) checks the current room's `busstop` flag. It's false for any room that doesn't have the attribute at all, so you only need to add it to the stops.
- **Where can the bus go?** `FilterByAttribute(AllObjects(), "busstop", true)` finds every room with the flag ticked. The loop skips the room the player is already in, so they're never offered a trip to where they're standing.
- **What does the player see?** Each destination goes into a string dictionary, with the room's name as the key and its display alias as the text the player sees. `ShowMenu` returns the key of the option the player picks, and `GetObject` turns that name back into the room. Using a dictionary like this means you can change a room's alias without breaking the menu. See [Showing different text from what you check](/howto/scripting/asking-the-player#showing-different-text-from-what-you-check).
- **Nowhere to go.** `ShowMenu` needs at least one option, so if the player is at the only stop, the script says so instead of showing an empty menu.
- **Changing their mind.** The third parameter of `ShowMenu` is `true`, so the player doesn't have to choose. If they type something else instead, the menu goes away, `ShowMenu` returns an empty string, and they stay where they are. The command they typed isn't run, so they'll need to type it again.

In the editor, the menu step is "Set a variable or attribute", then "player's choice from a menu", and the move is "Move object" from the Objects category. We use `game.pov` rather than `player`, so the bus still works if the player changes to a different character.

### Which kind of menu?

This page uses `choice = ShowMenu(...)`, which waits for the player's choice and carries on from the next line. That keeps the whole command in one readable script, and the `options` dictionary is still available after the player has chosen.

The player can't save while that menu is on screen. For a travel menu, that doesn't matter - it's only up for a moment. If you'd rather the player could save in front of it, use the block form described in [Saving while a question is waiting](/howto/scripting/asking-the-player#saving-while-a-question-is-waiting), and keep anything the block needs in attributes rather than local variables.

### Opening up new destinations

As the player progresses, give them new places to go by setting the flag on more rooms. Wherever it makes sense in your game - when they buy a ticket, say, or when the harbour reopens - add "Set object flag" from the Variables category, choose the room and enter the flag name `busstop`. In code:

```quest
SetObjectFlagOn (harbour, "busstop")
```

The harbour now appears in the menu at every other stop, and the player can catch the bus from there too.

For a different kind of transport, change the flag, the pattern and the messages: a `station` flag and BOARD TRAIN, or a `booth` flag and USE TELEPORTER. To give a game two separate networks, such as a bus route and a train line, use a different flag for each.

## Charging a fare

To make each journey cost money, first turn on the money feature. Select "game", and on the _Features_ tab tick "Money". Give the player their starting money in "Starting money" on the player object's _Player_ tab. The player's money is then shown in the status pane, and is stored in the `money` attribute of the player object.

Then check the player can afford the fare before showing the menu, and only take the money once they've chosen a destination:

```quest
fare = 5
if (not GetBoolean(game.pov.parent, "busstop")) {
  msg ("No buses stop here.")
}
else if (game.pov.money < fare) {
  msg ("The fare is " + DisplayMoney(fare) + ", and you don't have enough.")
}
else {
  options = NewStringDictionary()
  foreach (stop, FilterByAttribute(AllObjects(), "busstop", true)) {
    if (stop <> game.pov.parent) {
      dictionary add (options, stop.name, GetDisplayAlias(stop))
    }
  }
  if (DictionaryCount(options) = 0) {
    msg ("The bus doesn't go anywhere else yet.")
  }
  else {
    choice = ShowMenu("Where do you want to go? The fare is " + DisplayMoney(fare) + ".", options, true)
    if (choice <> "") {
      DecreaseMoney (fare)
      msg ("The bus drops you off.")
      MoveObject (game.pov, GetObject(choice))
    }
  }
}
```

Because the money check comes first and nothing else can change the player's money before they choose, the fare never takes their money below zero. If they change their mind and don't choose, they aren't charged.

`DisplayMoney` shows the fare in the game's money format ("$5" by default - change it in "Format for money" on the game's _Player_ tab). `DecreaseMoney` is "Decrease money" in the Player category of the script editor. It only appears in the editor once "Money" is ticked. Without the money feature the player has no `money` attribute, so the check doesn't stop them, and `DecreaseMoney` shows an error when they travel.

Keeping the fare in a local variable, `fare`, means you only have to change it in one place. That works because `ShowMenu` waits for the choice - with the block form, you'd need to store it in an attribute instead.

Think about whether the player could strand themselves by spending all their money on fares. If they could, make sure there's always some other way to get where they need to go, or a way to earn more.

## Fast travel to visited rooms

Every room has a `visited` attribute, which Quest Viva sets to `true` when the player first enters it - including the room the game starts in. You can use it to let the player return to anywhere they've already been, which is how fast travel works in many games.

Add a command with the pattern `travel`, or put this in the script for a map, a spell or a magic ring:

```quest
options = NewStringDictionary()
foreach (room, FilterByAttribute(AllObjects(), "visited", true)) {
  if (room <> game.pov.parent) {
    dictionary add (options, room.name, GetDisplayAlias(room))
  }
}
if (DictionaryCount(options) = 0) {
  msg ("You haven't been anywhere else yet.")
}
else {
  choice = ShowMenu("Where do you want to travel to?", options, true)
  if (choice <> "") {
    MoveObject (game.pov, GetObject(choice))
  }
}
```

This works in the same way as the bus, but the list grows by itself as the player explores:

```
> travel
Where do you want to travel to?
1: high street
2: market square
3: harbour
```

To leave some rooms out - somewhere you only pass through once, or a room the player shouldn't be able to come back to - give them a Boolean attribute called `nofasttravel`, ticked, and change the `if` inside the loop to:

```quest
if (room <> game.pov.parent and not GetBoolean(room, "nofasttravel")) {
```

To add a room the player hasn't visited yet - somewhere they've read about, perhaps - set its `visited` attribute to `true` with "Set a variable or attribute" (`harbour.visited = true`). Bear in mind that also stops any "After entering the room for the first time" script on that room from running.

If your game shows a map, see [Teleporting](/howto/tasks/showing-a-map#teleporting) on the map page too.

## See also

- [Asking the player](/howto/scripting/asking-the-player) - more on `ShowMenu`
- [Score, health and money](/howto/world/score-health-money#money) - the money feature
