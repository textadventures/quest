---
layout: index
title: Changing the player object
---

When you create a Quest game, there is by default one "player" object, which represents the player's point of view (POV). The player's inventory consists of all the objects that are contained by this "player" object.

As of Quest 5.3, you can now switch the POV at any time - your game remains a single-player experience, but that player can now switch between different characters. This means you could create a game where the player can explore from different points of view, or perhaps simply choose a pre-defined character when starting the game.

Any script can change the current POV by calling the "Change player object" command. So you could change the POV after asking a question in the game start script, in response to a command, or maybe after successfully solving a puzzle.

Each player object gets its own inventory and attributes. This includes status attributes, so each player could have their own health or stats, and these will be updated on-screen as the player switches between characters. For status attributes which apply across the entire game (perhaps "score" for example), you should set these on the "game" object itself, so they will apply all the time regardless of which object is the current POV.


Making an object the player
---------------------------

Before an object can become the player, you need to set it up in the editor. On the _Features_ tab, tick the "Player:..." box, then on the _Player_ tab, select "Can be player".

On the _Player_ tab of the `game` you can select which object will be the player at the start of the game (if you do not select one, it will default to `player`).


Player or character?
--------------------

Quest will handle an object different depending on whether it is the current player or not. For example, if you have two player objects "Mary" and "Bob" in a game, and it is possible for them to be together in the same room at the same time, you will want different responses for LOOK AT MARY and LOOK AT BOB depending on whether the player is currently Mary or Bob.

When the player is Mary, Quest will use the setting for Mary on the _Player_ tab, so the the description there might start "You are...". For Bob, Quest will use the setting on the _Setup_ tab, just as it does for other objects. The description on the _Setup_ tab might start "Bob is...".


![](images/Pov1.png "Pov1.png")

Say this object is Bob. On the _Player_ tab set the name, description, etc. that apply when the player is Bob, whilst on the _Setup_ tab, set them for how they will be when the player is not Bob.

`game.pov`
----------

The `game` object has an attribute called "pov", and this stores the current player object. You can use that to test who the player is. For example, you might want Bob's description to change depending on who is looking at him. The description on the _Player_ tab is fine, that is only for when the player is Bob. However, we could change the one on _Setup_ tab to a script that changes depending on who the player is when not Bob.

```
if (game.pov = Mary) {
  msg("Bob is that creepy guy with an unhealthy obsession with defibrillators.")
}
else if (game.pov = Dave) {
  msg("Bob is your best friend... though he went a weird after that incident with the defibrillator.")
}
else {
  msg("Bob is just some guy.")
}
```


Changing `game.pov`
-------------------

To change the current player object, use the `ChangePOV` function, which changes `game.pov` and also does some house-keeping (so just setting `game.pov` to the new player object may not work properly).

```
ChangePOV (bob)
```

