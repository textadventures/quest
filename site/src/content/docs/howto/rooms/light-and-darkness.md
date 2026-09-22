---
title: Handling light and dark
description: Make rooms dark, give the player a torch or a light switch, and check whether the player can see
sidebar:
  order: 5
---

Quest Viva has a built-in system for darkness. A dark room hides its description, its contents and its exits until the player brings a light with them - or finds the switch.

Start by ticking "Lightness and darkness: rooms can be dark, and objects can light them up" on the game object's [_Features_ tab](/howto/objects/features#game-features). That reveals a _Light/Dark_ tab on every room, and a "Lightness and darkness: objects can light up a room" checkbox on each object's own _Features_ tab.

## Making a room dark

Rooms are lit by default. On the room's _Light/Dark_ tab:

| Option | What it does |
| --- | --- |
| Room is initially dark | Sets the room's `dark` attribute. |
| Description to display when the room is dark | "Use default room description", "Text" or "Run script". The room's normal description is never used while it is dark. |

Play it and you will find two things. First, you get the default dark-room message:

```
> west

You are in a cellar.
It is too dark to make anything out.
```

Second, you are stuck: nothing in the room is listed, nothing can be examined or picked up, and the exits are not shown either, so there is no way out.

## Letting light in through an exit

Select the exit back out of the dark room, go to its _Options_ tab and tick "This object is a lightsource", leaving Brightness as "Weak (shown when room is dark)". The exit is now visible and usable even in the dark - there is faint light spilling in from the room beyond.

Then go back to the room's _Light/Dark_ tab and put something in the dark description, so the player knows what they can see:

```
> west

You are in a cellar.
You can go east.
It is dark, but you can just make out an exit to the east.
```

## Weak and strong

There are three levels of light: none, weak and strong.

- **Strong** illuminates the whole room, so everything in it becomes visible and reachable.
- **Weak** only illuminates itself. A weak light source is listed and can be examined in a dark room, but nothing else is.

That is why the exit above was set to weak: it shows the way out without lighting up the room.

## Giving the player a torch

Create an object called "torch" and tick "Take" on its _Inventory_ tab. On its _Features_ tab tick "Lightness and darkness: objects can light up a room", then on the _Light/Dark_ tab that appears tick "This object is a lightsource" and set Brightness to "Strong (illuminates room)".

Carry the torch into the dark room and the room is lit. The light source does not have to be carried, though - a strong light source sitting in the room works just as well.

### Light sources in containers

A light source inside an [open or transparent container](/howto/objects/containers) still lights the room. Put it in a closed, non-transparent one and the light goes out, because the player can no longer see it.

## A light switch

Create a `lightswitch` object in the dark room and make it [switchable](/howto/objects/switchable). In "After switching on the object" put:

```quest
darkroom.dark = false
```

and in "After switching off the object":

```quest
darkroom.dark = true
```

The player needs a light to find the switch in the first place, so consider making the switch itself a weak light source - or put it in the lit room next door.

## A torch that can be switched on and off

Make the torch switchable as well. Leave "This object is a lightsource" unticked, since the torch starts off, but still set Brightness to "Strong (illuminates room)". Then in "After switching on the object":

```quest
this.lightsource = true
```

and in "After switching off the object":

```quest
this.lightsource = false
```

## A battery that runs down

Give the torch an integer attribute `battery` on the _Attributes_ tab, with a value of 5 while you are testing. Then add a [turn script](/howto/time/time-turns-and-timers) to the game's _Scripts_ tab, name it `torchturnscript`, and leave "Enabled when the game begins" unticked:

```quest
torch.battery = torch.battery - 1
if (torch.battery < 1) {
  torch.switchedon = false
  torch.lightsource = false
  DisableTurnScript (torchturnscript)
  msg ("Your torch flickers and dies.")
  torch.cannotswitchon = "You cannot turn the torch on, the battery is dead."
}
```

It checks for less than one rather than zero so the torch still fails if something odd makes it skip a value. The last line uses the switchable feature's `cannotswitchon` attribute: while it holds a string, `TURN ON` prints that string and refuses, so you do not need to check the battery in the switch-on script. Setting `switchedon` to false also runs the torch's own "After switching off" script.

The torch's switch scripts now start and stop the turn script. Switching on:

```quest
this.lightsource = true
EnableTurnScript (torchturnscript)
```

Switching off:

```quest
this.lightsource = false
DisableTurnScript (torchturnscript)
```

To recharge or replace the battery, reset both attributes:

```quest
torch.battery = 5
torch.cannotswitchon = null
```

## Checking whether it is dark

`CheckDarkness()` returns true if the player's current room is dark and there is no strong light source in it. Use it in any script that should behave differently in the dark - here, a `SEARCH` command:

```quest
if (CheckDarkness()) {
  msg ("It is too dark to search.")
}
else {
  msg ("You search but find nothing of interest.")
}
```

## Descriptions: text or script

For **rooms**, Quest Viva checks the light before anything is printed. If the room is dark it uses the "Description to display when the room is dark" - text or script - and the normal description never runs.

For **objects**, a description set as *text* is replaced by "It is too dark to make anything out." when the room is dark, unless the object is itself a light source. A description set as a *script* runs whatever the light is. In practice that only comes up for objects the player is carrying, since nothing else in a dark room can be examined, and you may decide it does not matter - the player knows what they are holding. If you do want the check, make it yourself:

```quest
if (CheckDarkness()) {
  msg (DynamicTemplate("LookAtDarkness", this))
}
else {
  msg ("A sturdy metal torch, with a switch on the side.")
}
```

`DynamicTemplate("LookAtDarkness", this)` gives the standard wording, so your game stays consistent (and translatable).

## Setting light and dark in code

| Function | What it does |
| --- | --- |
| `SetDark (room)` | Makes the room dark. |
| `SetLight (room)` | Makes the room lit. |
| `SetObjectLightstrength (object, strength)` | Sets `lightstrength` to `"weak"`, `"strong"` or `""` for none, and sets `lightsource` to match. |
| `SetExitLightstrength (exit, strength)` | The same, for an exit. |
