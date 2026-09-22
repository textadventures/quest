---
title: Spells and magic
description: Let the player learn spells and cast them on objects, with a mana cost shown in the status pane
---

This page shows one way to add magic to a game. By the end, the player will be able to:

- learn spells, by reading a scroll
- see how much mana they have in the status pane, and spend it to cast spells
- type CAST FROTZ ON STICK to make an object glow
- cast a spell that reverses another spell

Spells can do almost anything, so there's no single right way to build them. The approach here - each spell is an object, and one CAST command casts any of them - keeps everything about a spell in one place, and adding a new spell doesn't mean touching the command.

## Mana

Give the player a `mana` attribute and show it in the status pane. Select the player object and go to the _Attributes_ tab. Add an attribute called `mana`, make it an Integer and set it to 5. Then, in the "Status attributes" box above, add `mana` with the display text `Mana: !`. The `!` stands for the value, so the player sees "Mana: 5". See [Status attributes](/tutorial/status-attributes) for more.

Because it's a status attribute, the pane updates by itself whenever a script changes `mana` - you don't need to print it.

## Where the spells live

Each spell is an object, and where it is tells you whether the player knows it:

1. Add two rooms, `known_spells` and `unknown_spells`. They don't need exits or descriptions - the player never goes there.
2. Select "Advanced" in the tree and click "Add Type". Call it `spell`. On its _Attributes_ tab, add an Integer attribute called `cost` and set it to 1. This is the default cost of a spell.
3. Add an object called `frotz` inside `unknown_spells`. Give it the alias `Frotz`. On its _Attributes_ tab, add `spell` under "Inherited types".

Spells the player knows from the start go straight into `known_spells` instead. To make a spell more expensive, give that spell its own `cost` attribute.

## The CAST command

Add a command (see [Commands](/howto/commands/how-commands-work)) with this pattern:

```
cast #object1# on #object2#;cast #object1# at #object2#
```

Set its "Scope" box to:

```
object1=known_spells|object2=all;known_spells
```

The scope says where Quest Viva looks for each object. The spell (`object1`) can only be one the player knows - one inside `known_spells`. The target (`object2`) can be anything the player can see, or another known spell (we need that for the last spell on this page). See [Scope](/howto/commands/scope#setting-the-scope-on-a-command) for more about scopes.

Set the command's script to:

```quest
if (game.pov.mana < object1.cost) {
  msg ("You don't have enough mana to cast " + GetDisplayAlias(object1) + ".")
}
else {
  do (object1, "cast", QuickParams("target", object2))
}
```

This checks the player has enough mana, then runs the spell's own `cast` script, passing it the target in a variable called `target`.

### When the spell isn't known

If the player types the name of a spell they haven't learnt, or something that isn't a spell at all, Quest Viva can't find `object1` and would say "I can't see that." To say something better, set "Unresolved object text" to "Run script" and use:

```quest
if (key = "object1") {
  msg ("You don't know a spell called \"" + object + "\".")
}
else {
  msg ("You can't see \"" + object + "\" here.")
}
```

`key` tells you which part of the command couldn't be matched, and `object` is what the player typed for it.

Players will also type CAST FROTZ on its own. Add a second command with the pattern `cast #object#`, the scope `known_spells` and this script:

```quest
msg ("What do you want to cast " + GetDisplayAlias(object) + " on?")
```

Quest Viva prefers the command whose pattern matches more of what was typed, so CAST FROTZ ON STICK still goes to the first command.

## Frotz: making something glow

Frotz makes its target give off light. Quest Viva has [light and darkness](/howto/rooms/light-and-darkness) built in, so all the spell has to do is make the target a strong light source.

On the `frotz` object's _Attributes_ tab, add an attribute called `cast`, make it a Script, and enter:

```quest
if (DoesInherit(target, "spell")) {
  msg ("Spells can't glow.")
}
else if (GetBoolean(target, "lightsource")) {
  msg (WriteVerb(target, "be") + " already glowing.")
}
else {
  game.pov.mana = game.pov.mana - this.cost
  target.lightsource = true
  target.lightstrength = "strong"
  msg ("You cast {i:Frotz}. " + WriteVerb(target, "start") + " to glow brightly.")
}
```

`this` is the spell and `target` is what it was cast on. The first check stops the player casting Frotz on another spell, which the command's scope allows. The spell only takes the mana once it has worked, so a failed cast is free.

`WriteVerb` starts the sentence with the right pronoun and verb for the target: "It starts to glow" for a stick, "They start to glow" for a pair of boots, and "You start to glow" if the player casts it on themselves.

Casting Frotz on something the player is carrying works even in a dark room, because the things you're carrying are always in scope.

## Learning a spell

To learn a spell, the player reads a scroll. Add a `scroll` object, tick "Object can be taken" on its _Inventory_ tab, and on the _Verbs_ tab add the verb "read" with this script:

```quest
if (frotz.parent = known_spells) {
  msg ("You already know this spell.")
}
else {
  MoveObject (frotz, known_spells)
  msg ("As you read the scroll, the words sink into your memory. You have learnt {i:Frotz}!")
}
```

Learning a spell is just moving it into `known_spells`. You could equally do it when the player talks to a wizard, drinks a potion or reaches a certain level.

To let the player see which spells they know, add a command with the pattern `spells` and this script:

```quest
msg ("You know " + FormatList(GetDirectChildren(known_spells), ",", "and", "no spells") + ".")
```

## Lleps: reversing a spell

Lleps reverses another spell. Cast on Frotz, it makes Frotz put lights out instead of lighting them. Cast on Frotz again, it puts it back. Its target is a spell, which is why the command's scope includes `known_spells` for `object2`.

Add a `lleps` object to `known_spells` (or `unknown_spells`, with its own scroll), with the alias `Lleps`, inheriting `spell`. Give it this `cast` script:

```quest
if (not DoesInherit(target, "spell")) {
  msg ("Lleps only works on other spells.")
}
else if (target = this) {
  msg ("You suspect the universe would turn inside out if you did that.")
}
else {
  game.pov.mana = game.pov.mana - this.cost
  target.reversed = not GetBoolean(target, "reversed")
  msg ("You cast {i:Lleps} on {i:" + GetDisplayAlias(target) + "}. You feel it twist inside out in your memory.")
}
```

`not GetBoolean(target, "reversed")` flips the target's `reversed` flag: on if it was off, off if it was on.

Each spell then decides what "reversed" means for it. Here's Frotz's `cast` script again, with a reversed version that makes a glowing object stop glowing:

```quest
if (DoesInherit(target, "spell")) {
  msg ("Spells can't glow.")
}
else if (GetBoolean(this, "reversed")) {
  if (not GetBoolean(target, "lightsource")) {
    msg (WriteVerb(target, "be") + "n't glowing.")
  }
  else {
    game.pov.mana = game.pov.mana - this.cost
    target.lightsource = false
    msg ("You cast {i:Frotz}, and the light fades.")
  }
}
else if (GetBoolean(target, "lightsource")) {
  msg (WriteVerb(target, "be") + " already glowing.")
}
else {
  game.pov.mana = game.pov.mana - this.cost
  target.lightsource = true
  target.lightstrength = "strong"
  msg ("You cast {i:Frotz}. " + WriteVerb(target, "start") + " to glow brightly.")
}
```

Reversed Frotz works on anything that's a light source, including a torch you set up on its _Light/Dark_ tab, so the player could use it to put out a light that's guarding something.

## Adding more spells

Every new spell is an object in `known_spells` or `unknown_spells` that inherits `spell` and has a `cast` script. The script follows the same shape as Frotz:

1. Check anything that would stop the spell working - the wrong kind of target, or the target already being affected - and say so.
2. If it's reversed, take the mana and do the reverse effect.
3. Otherwise, take the mana and do the normal effect.

Mana only goes down so far, so give the player a way to get it back - a potion that adds to `mana`, or a [turn script](/tutorial/using-timers-and-turn-scripts#turn-scripts) that adds one every few turns up to a maximum.
