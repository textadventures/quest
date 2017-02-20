---
layout: index
title: Handling light and dark
---

Quest has a system built in for handling light and darkness in your game.


A Dark Room
-----------

The first step is to go to the features tab of the game object and tick the box "Lightness and darkness..." (actually this is optional; it just turns the editor features on, your game will run the same either way).

By default rooms are lit. We will create a dark room, called "darkroom". Create the room as normal, make exits to and from it, and give is a description. Now go to the Light/Dark tab, and tick the "Room is initially dark" checkbox.

Try the game, and you will find two things. The first is that there is no default dark room description; it is just blank. The second is that you are trapped in the dark room - there is no way to use the exit if it is too dark to see it!


A Light from the Door
---------------------

Go to the exit from this room, and on the Options tab, tick the "This object is a light source" box. In the dropdown box that appears, set it to be weak. Now the player will see and be able to use this exit, even if the room is dark - there is a faint light coming from the other room, enough to show you the way out.

Go back to the Light/Dark tab of the room, and add a description to display when dark. Perhaps: "It is dark, but you can just make out an exit to the west." Now when you play the game the room is still dark, but the exit is useable, and the player will not be trapped here.


Weak and Strong
---------------

Quest has three levels of light for objects. None at all, weak and strong. A strong light will illuminate the whole room. A weak source only illuminates itself. The exit was a weak light source, so it could be seen in the dark room but nothing else could. What we need is a strong light source.


Implementing a Torch
--------------------

Create a new object, called "torch". On the Inventory tab tick it so it can be taken. On the Features tab, tick Lightness and Darkness. Then on the Light/Dark tab, tick it as a light source and set it to be Strong.

Now go in-game. With the torch in hand, your darkroom will be illuminated.


### A Note about Containers

Quest has a sophisticated container system. If the player puts the torch in a container that is flagged as transparent, the torch will still illuminate the room.


Implementing a Light Switch
---------------------------

Create an object, lightswitch, inside the dark room. On the Features tab, make it switchable. On the Switchable tab, also make it Switchable, and fill in the message boxes. Then in the script to run when turned on, put in this (not sure what to do with code? See [here](copy_and_paste_code.html)):

```
  darkroom.dark=false
```

For the other script, you need this:

```
  darkroom.dark=true
```

Very simple, they just alter the "dark" attribute of your dark room.

If you try it out, you will find the light switch now controls the darkness of the room (you will need the torch to find the switch, but then leave the torch elsewhere to confirm the room is now lit). You could, of course, set the switch to be a weak light source, so it can be found in the dark.


Implementing a Switchable Torch
-------------------------------

We should be able to turn the torch off, to save the battery. Pretty similar to before - on the torch object, first set it to not be a light source, as it is initially turned off (but keep it as a Strong light source!), then go to the Features tab, and make it switchable. On the Switchable tab, make it Switchable (the default messages are good enough). Then in the script to run when turned on, put in this:

```
  this.lightsource=true
```

For the other script, you need this:

```
  this.lightsource=false
```

A Torch that Fails
------------------

No torch lasts forever; let us put a limit on this one. First create a new attribute for the torch, called "battery". If working offline, you can do that by going to the Attributes tab to create it, and Set it to be an integer, with a value of 5 (we want a small number whilst we are playing around; for your game you will want it much higher). If using the web version, you have no Attributes tab, so go to the Script tab of the game object, and add this code:

```
  torch.battery = 5
```

We now need a turn script. We could do this two ways: have the turn script enabled and disabled when the torch is turned on and off, or have it running all the time, but only use the battery when turned on. I am going to do the former.

Create a turn script, and make sure it is under the Object object (i.e., it is vertically aligned with your rooms, not the stuff in the rooms). Give the turn script a name, torchturnscript, and paste in this code:

```
torch.battery = torch.battery - 1
if (torch.battery < 1) {
  torch.switchedon = false
  torch.lightsource = false
  DisableTurnScript (torchturnscript)
  msg ("You torch flickers and dies.")
  torch.cannotswitchon = "You cannot turn the torch on, the battery is dead."
}
```

The first line reduces the life of the battery. If it gets to zero the rest of the script kicks in (I am checking for less than one rather than zero in case something odd happens, and it jumps to -1; I still want the torch to the fail then). Once the battery fails, we need the torch to be switched off, to not be a light source and for this turn script to stop. We also need a message to the player.

The last line sets a special attribute that (as of version 5.7) Quest will check before switching the object on; if the attribute is a string, the string is displayed, rather than turning on the item.

Now we need to go back to the torch, and the scripts on the Switchable tab. The turn off script now looks like this, as we now want to turn off the turn script when the torch is off:

```
  this.lightsource = false
  DisableTurnScript (torchturnscript)
```
The turn on script is more complicated, as we have to test if the battery is dead.
```
  if (this.battery > 0) {
    this.lightsource = true
    EnableTurnScript (torchturnscript)
  }
  else {
    msg ("No light - the battery is dead.")
    this.switchedon = false
  }
```

If the battery is good, the torch becomes a light source, and the turn script goes on.

If the battery is dead, we need to turn the torch off again, and give a message. The turning on message will fire every time, that is just how Quest works, so the fail message needs to be crafted around that.

Want to recharge or replace the battery? Here is the code:

```
torch.battery = 5
torch.cannotswitchon = null
```

Is it dark?
-----------

If you want to know if it is dark in the current room, use the `CheckDarkness` function. This will return `true` if the room is dark and there is no strong light source in it, and false otherwise. For example, for a SEARCH command, the code might look like this:

```
if (CheckDarkness()) {
  msg("It is too dark to search.")
}
else {
  msg("You search but find nothing of interest.")
}
```