---
layout: index
title: Followers
---

Occasionally it is useful to have one or more characters follow the player, whether a helpful companion, or an annoying foe. Or an annoying companion...

We will store the followers in an object list. This means you can have any number of them trailing along at one time. The first step, then, is to set this up.

Usually at this point I would say that users of the on-line editor are at a disadvantage because they cannot set attributes directly, but for once this is true for users of the off-line editor too - the _Attributes_ tab cannot handle object lists, so we will all be doing this the same way.

For the sake of this tutorial, create an object called "dog".


Setting Up
----------

On the `game` object, go to the _Scripts_ tab, and add these line to the Start script:
```
player.followers = NewObjectList()
list add (player.followers, dog)
```
Below the start script is the "Script when entering a room". In there, paste in this code:
```
foreach (o, player.followers) {
  if (HasAttribute(o, "followphrases")) {
    msg (StringListItem(o.followphrases, GetRandomInt(0, ListCount(o.followphrases) - 1)))
  }
  else {
    msg (CapFirst(GetDisplayName(o)) + " is following you.")
  }
  o.parent = player.parent
}
```
What this does is iterate through each thing in the `followers` list. For each one, it checks if there is a `followphrases` attributes. If there is, one is selected at random and printed. Otherwise a default phrase is used.

Go in game and see if the dog will follow you!

To add variety, you can add an attribute to the dog called "followphrases", set it to be a string list, and then add various phrases to it that describe the dog following you.


Adding and Removing Followers
-----------------------------

This is just a matter of adding or removing the follower from the `player.followers` list. We added the dog at the start; here it is again, together with removing the dog. Once removed, the dog will stay in the last room it followed the player into.
```
list add (player.followers, dog)

list remove (player.followers, dog)
```
You could add these scripts to commands to allow the player to control the dog. For example, for SAY STAY command, you could add this script:
```
  if (not dog.parent = player.parent) {
    msg("Who are you talking to?")
  }
  else if (not ListContains(player.followers, dog)) {
    msg("The dog gives you a strange look.")
  }
  else {
    list remove (player.followers, dog)
    msg("You dog looks at you sadly, as it sits.")
  }
```



## Some Variation

You game will feel much better if the player is not seeing the same phrase again and again. If she reads "A dog is following you." every time she goes into a new room, it will get tedious.

So mix it up a bit with the "followphrases" attribute. This is where the off-line users can look smug, they can go to the _Attributes_ tab of the follower, the dog in this case, create a new attribute called "followphrases", set it to be a string list, and start typing in phrases. On-line users will have to do this in the start script of the game object. Here is some example code for the dog.
```
  dog.followphrases = NewStringList()
  list add (dog.followphrases, "A dog enters the room behind you.")
  list add (dog.followphrases, "Sniffing cautiously, a dog follows you into the room.")
  list add (dog.followphrases, "A dog, wagging its tail, enters the room behind you.")
```