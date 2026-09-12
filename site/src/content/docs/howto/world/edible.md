---
title: Items that can be eaten
sidebar:
  order: 11
---

Food and drink turn up in a lot of games — rations that keep you going, a potion that heals you, a mushroom that probably shouldn't have been eaten. Quest Viva has a built-in feature for all of these.

Tick "Edible" on the object's [_Features_ tab](/howto/world/features#object-features), then go to the _Edible_ tab that appears and change the dropdown from "Cannot be eaten" to "Can be eaten".

That one change does three things:

- the `EAT` verb now works on the object
- an "Eat" verb appears in the object's display verbs and inventory verbs, so the player can click it
- when the object is eaten, it is destroyed

## The default behaviour

With "Can be eaten" selected, eating the object prints a message and removes it from the game. Leave "Message to print when eating" blank for the standard wording, or fill it in with your own:

```
The apple is crisp and sharp, and gone in four bites.
```

If the "Health" feature is enabled on the game's _Features_ tab, a "Change health by" box also appears. Whatever number you put there is added to the player's health when the object is eaten — so `20` for a decent meal, `-30` for the mushroom.

The player's health starts at 100 and is held between 0 and 100, so a meal cannot take them above full, and a poisoning cannot take them below nothing. When health does reach zero, the game's "script to run when health reaches zero" fires — see [Score, health and money](/howto/world/score-health-money).

## Running a script instead

For anything more involved, tick "Run a script" and write the script yourself.

The important thing to know is that a script replaces the default behaviour completely. Nothing is printed, health does not change, and — the one that catches people out — **the object is not destroyed**. Everything is up to your script:

```quest
msg ("You drink the potion. The pain in your leg fades.")
player.health = player.health + 40
player.poisoned = false
destroy ("potion")
```

That is usually what you want as soon as eating something has consequences. Some examples:

**Something left over.** The player eats the sandwich but keeps the foil:

```quest
msg ("You eat the sandwich, and screw the foil into a ball.")
MoveObject (foil, player)
destroy ("sandwich")
```

**Several helpings.** A bag of biscuits that can be eaten more than once:

```quest
this.helpings = this.helpings - 1
if (this.helpings > 0) {
  msg ("You eat a biscuit. There are " + this.helpings + " left.")
}
else {
  msg ("You eat the last biscuit and screw up the packet.")
  destroy (this.name)
}
```

Give the object a `helpings` integer attribute on its _Attributes_ tab to go with that.

**Refusing to eat.** Nothing obliges the script to let the player eat at all:

```quest
if (player.parent = kitchen) {
  msg ("You eat the stew straight from the pot.")
  destroy ("stew")
}
else {
  msg ("You would rather not eat that cold.")
}
```

Note that "Run a script" is an advanced option, so you will only see it with advanced options showing.

## Drink, and other verbs

There is no separate "drinkable" feature — a potion or a bottle of water is just an edible object whose messages talk about drinking. If you want the player to be able to type `DRINK POTION` as well as `EAT POTION`, add a "drink" [verb](/howto/commands/using-verbs) to the object on its _Verbs_ tab.

The same trick covers anything else the player might consume: `SMOKE`, `SWALLOW`, `SNIFF`. Use the edible feature for the one that destroys the object, and verbs for the rest.
