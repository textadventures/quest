---
layout: index
title: How to Build a Transit System
---



Some years ago I release a library for a simple transit system. Due to changes in Quest, the library no longer works and I never got around to updating it. So instead, here it is as a tutorial.

In this systyem, the player can go to any one of a set of locations (stations, spaceports, teleportation booths, magic gateway etc.), and at that location type in a certain command. She will then be presented with a list of destinations, and if she selects one, she will go directly there.


On the Buses
------------

I am going to build a bus system in this example, so there will be certain locations that are bus stops. Each of them will have a Boolean attribute, "busstop", set to true. We also need an object dictionary to store the destinations. 

Let's limit the player at the start so she can only travel to your house and to the mall. Later we will add more destinations, an office building and the docks. The code to do this needs to be run at the start of the game, so go to the _Scripts_ tab of the game object. At the top is a script that runs at the start of the game.

Here is some example code that could be pasted in there (if you have rooms appropiately named!).

```
  Bus stop near your house.busstop = true
  Bus stop near mall.busstop = true
  Bus stop near Fenton Industries.busstop = true
  Bus stop near docks.busstop = true
  game.destinations = NewObjectDictionary()
  dictionary add(game.destinations, "Your house", Bus stop near your house)
  dictionary add(game.destinations, "The Mall", Bus stop near mall)
```

So what does that all mean? The first four lines set up the four bus stops in the game, one for each location (the first room is called `Bus stop near your house` and so on).

The fifth line sets up the dictionary. An object dictionary is a way to store information. Dictionaries allow you to retrieve an entry using a string, called a key, and in an object dictionary, all the entries are objects (which can include rooms in Quest). so what we have is a string - the name of the destination - connected to a room - the destination itself.

The last two lines, then, add entries to the dictionary. Note that when adding entries, the first parameter is the dictionary, `game.destinations`, the second is a string (the name that will shown in the list of destinations) and the third is an object (the destination itself).

You will have noticed we are only adding two bus stops - we only want the player to be able to travel to the mall and back at first.


Catching a bus
--------------

Then we need a new command. What this is will depend on the type of transport. as this is for a bus stop, I will be doing CATCH BUS, so create a new command and put in "catch bus" as the pattern.

Paste in this code:

```
if (not GetBoolean(player.parent, "busstop")) {
  msg ("No buses stop here.")
}
else {
  sl = NewStringList()
  foreach (key, game.destinations) {
    if (not ObjectDictionaryItem(game.destinations, key) = player.parent) {
      list add (sl, key)
    }
  }
  ShowMenu ("Where do you want to go?", sl, true) {
    dest = ObjectDictionaryItem(game.destinations, result)
    msg ("You take the bus to " + result)
    player.parent = dest
  }
}
```

The first three lines are checking the player is at a bus stop. This is why we set the "busstop" flag earlier; so we can check if bus travel is allowed from the room.

Then (if this is a bus stop) the command creates a new string list, and puts in it all the destination names, except the current location. The `ShowMenu` function displays the menu, with that string list as the options displayed. The selection goes into a variable called `result`, and this is used to get the destination from the dictionary. A message is displayed, and the player moved.


Adding Destinations
--------------------

As the player progresses through the game, you can give her access to new areas by adding new destinations. Exactly where you do that will depend on your game, and what it was that opened up the new area, but the code to make it happen is very simple.

```
dictionary add (game.destinations, "Downtown", Bus stop for downtown)
```


Paying a fare
-------------

If money is important in your game, you might want to charge the player for travelling (but consider if the player can get in an unwinnable situation by wasting all her money).

This needs to be updated in three places to charge the player a flat rate for each journey. First you need to give the player an integer attribute, called "money". If on-line, you will have to do that in the start script of the game object again, by adding this line (set the amount to whatever):

```
  player.money = 17
```

In the command we need to check the player has enough money, and if he does use the system, we need to deduct that from his money. The command will need to be updated to this (the fare is 5; you will need to modify it in two places if you want to change the fare):

```
if (not GetBoolean(player.parent, "busstop")) {
  msg ("No buses stop here.")
}
else if (player.money < 5) {
  msg ("You don't have enough cash for a bus.")
}
else {
  sl = NewStringList()
  foreach (key, game.destinations) {
    if (not ObjectDictionaryItem(game.destinations, key) = player.parent) {
      list add (sl, key)
    }
  }
  ShowMenu ("Where do you want to go?", sl, true) {
    dest = ObjectDictionaryItem(game.destinations, result)
    player.money = player.money - 5
    msg ("You take the bus to " + result)
    player.parent = dest
  }
}
```
