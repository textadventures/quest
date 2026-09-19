---
title: Liquids
description: Let the player fill a container with water, drink from it, empty it and pour it over things
---

Water, wine and potions don't work well as ordinary objects - the player can't pick up a litre of water and put it in their pocket. Instead, treat the liquid as an amount stored on its container. This page builds a waterskin that the player can fill from a pool or a tap, drink from, empty, and pour over a campfire to put it out.

| You want | Use |
|---|---|
| Something the player can drink once, like a potion | The built-in "drink" verb on the object |
| Water the player can drink where it is, like a pool | The "drink" verb on the pool |
| A container that holds an amount of liquid | Integer attributes on the container, plus "fill", "empty" and "drink" verbs |
| Pouring liquid over something else | A command with two objects |

## Drinking

"drink" is one of Quest Viva's built-in verbs. Every object already understands DRINK, and says "You can't drink it." until you give it a response. To make something drinkable, select it, go to the _Verbs_ tab, add the verb "drink", and either print a message or run a script. For a potion that's used up, the script might be:

```quest
msg ("You drink the potion. You feel much stronger.")
RemoveObject (this)
```

The _Edible_ feature doesn't help here - it only adds EAT.

For a pool, the drink verb only needs a message: "You kneel and drink from the pool. The water is cold and fresh." Add "water" to the pool's _Other names_ (on the _Object_ tab), so DRINK WATER works too.

## A container of water

Create a `waterskin` object the player can take. On its _Attributes_ tab, add two Integer attributes:

- `water`, set to 0 - how much water is in it now
- `capacity`, set to 3 - how much it holds, measured in drinks

### Describing it

The simplest description uses the text processor. Set the waterskin's _"Look at" object description_ (on the _Setup_ tab) to:

```
A leather waterskin. {if waterskin.water=0:It's empty.}{if waterskin.water>0:There's some water in it.}
```

To say how full it is, change the description to "Run script" and use a script instead:

```quest
if (this.water = 0) {
  msg ("A leather waterskin. It's empty.")
}
else if (this.water = this.capacity) {
  msg ("A leather waterskin, full of water.")
}
else {
  msg ("A leather waterskin, about " + (this.water * 100 / this.capacity) + "% full.")
}
```

Both `water` and `capacity` are whole numbers, so the percentage is rounded down - one drink from a full three-drink waterskin leaves it "about 66% full".

### Drinking from it

On the waterskin's _Verbs_ tab, add "drink" and choose "Run a script":

```quest
if (this.water = 0) {
  msg ("It's empty.")
}
else {
  this.water = this.water - 1
  msg ("You take a drink of water.")
}
```

If your game tracks thirst, this is the place to reduce it.

### Emptying it

Add an "empty" verb. It isn't built in, so the editor creates a new verb for it:

```quest
if (this.water = 0) {
  msg ("It's already empty.")
}
else {
  this.water = 0
  msg ("You pour the water out on the ground.")
}
```

## Somewhere to fill it

A water source is any object with a Boolean attribute called `watersource` set to true. Give the pool one on its _Attributes_ tab.

For a tap, tick _Switchable_ on its _Features_ tab, set it to "Can be switched on/off" on the _Switchable_ tab, and give it a `watersource` attribute too. The fill verb below only counts a switchable source while it's switched on, so there's nothing to change in the tap's switch scripts - the tap's `switchedon` attribute is the only thing that needs to know whether water is running.

Now add a "fill" verb to the waterskin:

```quest
source = null
foreach (obj, ScopeReachable()) {
  if (GetBoolean(obj, "watersource")) {
    if (not DoesInherit(obj, "switchable") or GetBoolean(obj, "switchedon")) {
      source = obj
    }
  }
}
if (source = null) {
  msg ("There's no water here.")
}
else if (this.water = this.capacity) {
  msg ("It's already full.")
}
else {
  this.water = this.capacity
  msg ("You fill " + GetDefiniteName(this) + " from " + GetDefiniteName(source) + ".")
}
```

`ScopeReachable()` is every object the player can reach, so the waterskin fills from any source in the room, or one the player is carrying.

```
> fill waterskin
There's no water here.

> turn on tap
You switch it on.

> fill waterskin
You fill the waterskin from the tap.
```

If you have several containers, give each one `water` and `capacity` attributes and the same verbs. Once you're doing that for more than a couple of objects, it's worth [creating a type](/advanced-topics/using-inherited-types) that holds the attributes and verb scripts, and having each container inherit it.

## Pouring it over something

To let the player put out a campfire, you need a command that mentions two objects. Add a new command to the game with this pattern:

```
pour #object1# on #object2#;pour #object1# over #object2#;empty #object1# on #object2#;empty #object1# over #object2#
```

and this script:

```quest
if (not HasInt(object1, "water")) {
  msg ("You can't pour " + object1.article + ".")
}
else if (object1.water = 0) {
  msg (CapFirst(GetDefiniteName(object1)) + " is empty.")
}
else {
  object1.water = 0
  if (HasScript(object2, "douse")) {
    do (object2, "douse")
  }
  else {
    msg ("You pour the water over " + GetDefiniteName(object2) + ".")
  }
}
```

Any container with a `water` attribute can be poured. If the thing it's poured on has a `douse` script, that script decides what happens. Otherwise the water just runs off. EMPTY WATERSKIN on its own still runs the "empty" verb - Quest Viva picks the command because it matches more of what the player typed.

For the campfire, give it a Boolean `lit` attribute set to true, "fire" as another name, and a Script attribute called `douse`:

```quest
if (this.lit) {
  this.lit = false
  msg ("The fire hisses and spits, and goes out.")
}
else {
  msg ("You pour the water over the soggy ashes.")
}
```

Its description can use the text processor: `{if campfire.lit:The fire crackles merrily.}{if not campfire.lit:A heap of soggy ashes.}`

## Other ways the player might say it

A verb only matches the verb followed by an object, so DRINK FROM WATERSKIN and FILL WATERSKIN FROM POOL aren't understood yet. Two more commands fix that for every object in the game. The first has the pattern `drink from #object#;drink out of #object#`:

```quest
if (HasScript(object, "drink")) {
  do (object, "drink")
}
else {
  msg ("You can't drink from " + object.article + ".")
}
```

The second has the pattern `fill #object1# from #object2#;fill #object1# at #object2#;fill #object1# with #object2#`, and hands over to the fill verb, which finds the source by itself:

```quest
if (HasScript(object1, "fill")) {
  do (object1, "fill")
}
else {
  msg ("You can't fill " + object1.article + ".")
}
```

Now FILL WATERSKIN WITH WATER works beside the pool, because "water" is one of the pool's other names. The `HasScript` checks matter: they only run a verb that was set up to "Run a script", not one that prints a message.

## See also

- [Using verbs](/howto/commands/using-verbs)
- [Custom commands](/tutorial/custom-commands)
- [Custom attributes](/tutorial/custom-attributes)
- [Turning one thing into another](/howto/tasks/convert)
