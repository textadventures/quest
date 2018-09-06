---
layout: index
title: Spells for the Zombie Apocalypse
---



_Why are there spells in a zombie apocalyse? No one knows, but it is a fact._

This is an extension to the two part series on how to do combat on the web version of Quest. However, nothing in this article relies on the other two articles, you could readily add these spells to your game without even reading them (though like them, this does assume some familiarity with Quest code, at least to be able to copy-and-paste it).

Spells have a number of issues that need to be considered, and will make a lot of work for the game creator. By their nature spells can do pretty much anything, and there is no way that can be covered here. All we can hope to do is look at a few examples, and hope that gives some points about how to implement your own spells.

A list of classic spells can be found here, and we will have a go at implementing some of them.
http://www.ifwiki.org/index.php/Spells

The second problem is that spells can be cast on anything, and we need the game to be able to handle that. If we start with "frotz", which will cause something to give light, we need to create the spell so it can be cast on any object in the game, from a zombie, to your trusty spade or even yourself.

We also need to think about how spells are cast. By this I mean, what does the player need to do to be able to cast a spell. Perhaps the player must learn the spell from a wizard, or by absorbing a glyph from a scroll or just picking it up off the ground. Can she cast the spell as often as she likes, or can she only cast it once, or maybe she has to use magic points to cast it. For this tutorial, we shall say a spell needs to be learnt from a scroll, and the player has to spend magic points. Hopefully you will be able to adapt these to your own game.

For the desktop version, it would be easier to create a "spell" type, and have each spell as an object of that type. That is not an option for the web version, so we will do it quite differently, and have each spell as a separate command.

We will start by giving the player some magic points. Go to the "start" script of the game object, and add this line:

```
player.magicka = 5
```

### The Frotz Spell

Our first spell will be "Frotz", which will make an object glow. This will be relatively easy, as Quest has light and dark built in, and it affects objects that are in reach.

First, go to the _Features_ tab of the game object, and tick "Lightness and darkness". Then go make a room dark (go to its _Light/Dark_ tab, and tick "Room is initially dark"). This will allow us to test the spell later.

Now we can create the command. The pattern for our "frotz" spell will be this:

> frotz #object#;cast frotz at #object#;cast frotz at #object#

You also need to give the command a name, "frotz", so we can reference it later (commands generally do not need names, but we will do some magic later that will need these to). Here is the code:

```
if (player.magicka  < 1) {
  msg ("You don't have enough magic to do that.")
}
else if (GetBoolean(object, "lightsource")) {
  msg ("It already is!")
}
else {
  player.magicka = player.magicka - 1
  // Modify the target object
  object.lightsource = true
  object.lightstrength = "strong"
  object.frotzstatus = " " + WriteVerb(object, "be") + " shining brightly."
  if (HasString(object, "look")) {
    object.look = object.look + object.frotzstatus
  }
  else if (not HasScript(object, "look")) {
    object.look = object.frotzstatus
  }
  if (not HasString(object, "alias")) {
    object.alias = object.name
  }
  object.alias = object.alias + " (shining)"
  msg ("You cast {i:Frotz} on the " + GetDisplayName(object) + "." + object.frotzstatus)
}
```

It starts easy enough, checking if the object is already a light source and if the player has enough magic points. If all is well, the player's magic points are reduced, and then we modify the object...

We need to take account of the player casting this on a guy or a pair of shoes, and the `WriteVerb` function does just that, returning "He is" or "They are" respectively. We can then use that in the message telling the player what happened.

We also need to update the object description. If the description is just a string, we can tack `object.frotzstatus` on the end, and if it is blank (and please, let's not do that!), we can set it to be just `object.frotzstatus`. If it is a script, you will need to modify the script on each object to check if the object is glowing and react accordingly.

Then we update the object alias (or give it an object alias if there is none).


### Learning the spell

So we have a spell, and the player can cast it limited times, but she knows it from the start. We need some way to flag if this spell has been learnt. If this was an object, that would be trivial, just use a flag (i.e., a Boolean attribute). Well, in Quest, commands are objects too, and we can use attributes for them too.

So we need to modify the command script to check the flag first. We need to add three new lines at the top, and then add an `else` at the start of the next line. The top three lines we had before will then look like these six lines (indeed, the top three lines of nearly _all_ our spells will look like this):

```
if (not GetBoolean(this, "learnt")) {
  msg ("You don't know how to do that.")
}
else if (player.magicka  < 1) {
  msg ("You don't have enough magic to do that.")
}
```

Now we need a process to learn the spell. Let us say looking at a spell book does that (you may prefer to have the player read the book, or even drink a potion, look in a mirror or whatever). Create the spell book, and add this as a script to run when it is looked at:

```
firsttime {
  frotz.learnt = true
  msg ("You learn the {i:Frotz}. spell!")
}
otherwise {
  msg ("The book is entirely blank!")
}
```



### The Lleps Spell

The Lleps spell reverses any known spell. As spells are not objects (okay, I just said commands are, but when Quest tries to match text the player has typed, it only looks at _object_ objects), we will need to use "text" in the command pattern:

> lleps #text#;cast lleps at #text#;cast lleps at #text#

The code then looks like this:

```
object = null
foreach (cmd, ScopeCommands()) {
  if (text = cmd.name and GetBoolean(cmd, "learnt")) {
    object = cmd
  }
}
if (object = null) {
  msg ("You do not know a spell called " + text + ".")
}
else if (not GetBoolean(this, "learnt")) {
  msg ("You don't know how to do that.")
}
else if (player.magicka  < 1) {
  msg ("You don't have enough magic to do that.")
}
else if (object = this) {
  msg ("You suspect the universe will turn inside out if you do that.")
}
else {
  player.magicka = player.magicka - 1
  object.llepsed = not GetBoolean(object, "llepsed")
  msg ("You cast {i:Lleps} on the {i:" + CapFirst(object.name) + "}.")
}
```

I just said all our spells would start with the same six lines - this is an exception, as we have to first convert the `text` to `object`. In this case the first six lines in this case are trying to find the right spell, by searching through all commands, and looking for one that matches and that the player has learnt, and the next three lines give a message if no match was found.

At this point we have an `object` and can proceed as before. As with "Frotz" we check other conditions, including that the player is not trying to cast this on itself.

If all goes well the last few lines do the spell effect. In particular this line:

```
  object.llepsed = not GetBoolean(object, "llepsed")
```

This will set the "llepsed" flag of the target spell if not set, but will unset it if it is. This means the spell can be cast a second time on a spell to set it back to how it was.


### The Frotz Spell Again

So far so good. Now we need to change "Frotz" spell so it can be reversed. 

Note that any object that is glowing at the start of the game needs to be set up just right so the reversed Frotz spell will work properly. Hmm, this might be best set up as a function, as we will be doing the same thing several times. Create a new function, "Frotz" (this has a capital at the start, so is a different name to our command). Give it a single parameter, "object", and paste in the code, which you will recognise from before:

```
object.lightsource = true
object.lightstrength = "strong"
object.frotzstatus = " " + WriteVerb(object, "be") + " shining brightly."
if (HasString(object, "look")) {
  object.look = object.look + object.frotzstatus
}
else if (not HasScript(object, "look")) {
  object.look = object.frotzstatus
}
if (not HasString(object, "alias")) {
  object.alias = object.name
}
object.alias = object.alias + " (shining)"
```

Now go to the "frotz" command, and change its code for this:

```
if (not GetBoolean(this, "learnt")) {
  msg ("You don't know how to do that.")
}
else if (player.magicka  < 1) {
  msg ("You don't have enough magic to do that.")
}
else if (GetBoolean(object, "lightsource") and not GetBoolean(this, "llepsed")) {
  msg ("It already is!")
}
else if (not GetBoolean(object, "lightsource") and GetBoolean(this, "llepsed")) {
  msg ("It already is!")
}
else if (GetBoolean(this, "llepsed")) {
  player.magicka = player.magicka - 1
  object.lightsource = false
  if (HasString(object, "look")) {
    object.look = Replace(object.look, object.frotzstatus, "")
  }
  object.alias = Replace(object.alias, " (shining)", "")
  msg ("You cast {i:Frotz} on the " + GetDisplayName(object) + ", and it stops shining.")
}
else {
  player.magicka = player.magicka - 1
  Frotz(object)
  msg ("You cast {i:Frotz} on the " + GetDisplayName(object) + "." + object.frotzstatus)
}
```

Now any object that is glowing at the start of the game, rather than setting it to glow on the _Light/Dark_ tab, instead doing it in code, in the "start" script of the game object. This example does it for an object called "glowstone":

```
Frotz(glowstone)
```

Now we can be sure that all the glowing items can be set to not glow by the reversed Frotz spell.


### Other Spells

So now we can think about how to approach all other spells. Create a command, give it a pattern in the form we did before, and a name. The code has the general format:

```
if (not GetBoolean(this, "learnt")) {
  msg ("You don't know how to do that.")
}
else if (player.magicka  < 1) {
  msg ("You don't have enough magic to do that.")
}
else if ([Check if the object is already affected by the spell]) {
  msg ("It already is!")
}
else if ([Check any other conditions for not casting the spell]) {
  msg ("You cannot do that!")
}
else if (GetBoolean(this, "llepsed")) {
  player.magicka = player.magicka - 1
  [Update the world for the reverse effect]
  msg ([Message about the effect])
}
else {
  player.magicka = player.magicka - 1
  [Update the world for the normal effect]
  msg ([Message about the effect])
}
```

It is as easy as that!


### The Aimfiz Spell

This spell teleports the caster to someone else's location, and we will look at it as it can be cast on objects that are not here. The pattern is the usual:

> aimfiz #object#;cast aimfiz at #object#;cast aimfiz at #object#

Remember to name it "aimfiz" so it can be reversed. We will say that if it is reversed the target gets teleported to the player.

The trick here is to set the scope to "world". Quest will try to match the object against everything in the game world. 

The code then is pretty easy. We check all the possible fail scenarios as usual, then check if it is reversed, and perform the spell action. Note that the message should be before the line where the player moves so the player sees the message before the room description.

```
if (not GetBoolean(this, "learnt")) {
  msg ("You don't know how to do that.")
}
else if (player.magicka  < 1) {
  msg ("You don't have enough magic to do that.")
}
else if (object.parent = player.parent) {
  msg ("You're already together!")
}
else if (not DoesInherit(object, "npc_type")) {
  msg ("You can't {i:aimfiz} to that.")
}
else if (not ListContains(player.friends, object)) {
  msg ("You can't {i:aimfiz} to someone you do not know.")
}
else if (GetBoolean(this, "llepsed")) {
  player.magicka = player.magicka - 1
  object.parent = player.parent
  msg ("You cast {i:Aimfiz} on " + GetDisplayName(object) + ", and " + object.gender + " suddenly appears in front of you.")
}
else {
  player.magicka = player.magicka - 1
  msg ("You cast {i:Aimfiz} on " + GetDisplayName(object) + ", and sudden you are stood in the same room as " + object.article + ".")
  player.parent = object.parent
}
```

This version will only allow the player to teleport to an object that is of the "npc_type" (any object set to be male or female), and is in the list `player.friends`, so you will also need a mechanism to add NPCs to that list as the player encounters them. You could easily adapt the spell to allow the player to teleport to a known location (but what would the reverse spell do?), or to NPCs with a flag set on them (change the fifth condition). If you use ConvLib you could check the object is of the "talkingchar" type (change the fourth condition), and then that its "nevermet" flag is false (change the fifth condition); the player could then teleport to any NPC she has talked to.