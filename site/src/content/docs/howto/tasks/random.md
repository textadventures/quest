---
title: Randomness
description: Vary descriptions, pick random objects and exits, roll dice and decide outcomes by chance
---

A little randomness makes a game feel less mechanical: a description the player sees many times can change, an NPC can wander off in a random direction, and an attack can hit or miss. This page shows the built-in ways to do that, and some patterns for using them.

| You want | Use | In the editor |
|---|---|---|
| Varying text in a description or message | `{random:one:two:three}` | "Random text" in the text processor buttons above a text box |
| Something to happen some of the time | `RandomChance(percent)` | "If", then "random chance" |
| A random whole number | `GetRandomInt(min, max)` | "Set a variable or attribute", then "random number" |
| A random item from a list, room or exits | `PickOneString`, `PickOneObject`, `PickOneChild`, `PickOneExit`... | Code only |
| A dice roll, such as "3d6+2" | `DiceRoll(dice)` | Code only |

All of these are listed in [Randomising functions](/reference/functions/random).

## Random text

The easiest way to add variety is the text processor's `random` directive, which picks one of the options each time the text is printed:

```quest
It was a {random:blue:red:yellow} flower.
```

You can use it in any text the [text processor](/howto/world/text-processor) handles, such as a room or object description. The choice is made again every time, so if the player looks twice, the flower may have changed colour. To make a choice that sticks, see [Descriptions that don't change](#descriptions-that-dont-change) below.

## Chances and numbers

`RandomChance` takes a percentage from 0 to 100, and returns `true` that percentage of the time:

```quest
if (RandomChance(75)) {
  msg ("You hit the troll.")
}
else {
  msg ("You miss.")
}
```

`RandomChance(100)` is always `true`, and `RandomChance(0)` is always `false`.

`GetRandomInt` returns a whole number between the two numbers you give it, including both of them. This prints 1, 2 or 3, ten times:

```quest
for (i, 1, 10) {
  msg (GetRandomInt(1, 3))
}
```

`GetRandomDouble()` takes no parameters and returns a number between 0 and 1, such as 0.5395070795755296.

## Picking one from a list

These functions each return one item at random:

- `PickOneString` - a string from a string list, or from a string with the options separated by semicolons
- `PickOneObject` - an object from an object list
- `PickOneChild` - an object directly inside a room or container (not inside containers within it)
- `PickOneChildOfType` - the same, but only objects of the given type
- `PickOneExit` - a visible exit from a room
- `PickOneUnlockedExit` - a visible, unlocked exit from a room

```quest
msg ("The parrot squawks '" + PickOneString("Pieces of eight;Who's a pretty boy;Hello sailor") + "'.")
```

`PickOneExit` and `PickOneUnlockedExit` are handy for NPCs that wander about. The exit's `to` attribute is the room it leads to:

```quest
exit = PickOneUnlockedExit(cat.parent)
if (not exit = null) {
  MoveObject (cat, exit.to)
}
```

Each of these returns `null` (or an empty string, for `PickOneString`) if there's nothing to pick from.

## Rolling dice

`DiceRoll` takes a dice description in the usual RPG form - "d6", "3d6", "d6+1", "3d8-2" - and returns the total:

```quest
damage = DiceRoll(weapon.damage)
```

Here each weapon has a `damage` attribute, so a dagger might be "d4" and a great sword "3d6+2".

## One of several outcomes

To pick one of several outcomes, each equally likely, use `GetRandomInt` and a `switch`:

```quest
switch (GetRandomInt(1, 3)) {
  case (1) {
    msg ("The bat flies past your head.")
  }
  case (2) {
    msg ("The bat lands on the ceiling.")
  }
  case (3) {
    msg ("The bat flies off into the dark.")
  }
}
```

If the outcomes are just different text, `PickOneString` does the same job in one line.

You might think of doing it with a chain of `RandomChance` calls instead:

```quest
if (RandomChance(33)) {
  msg ("The bat flies past your head.")
}
else if (RandomChance(33)) {
  msg ("The bat lands on the ceiling.")
}
else {
  msg ("The bat flies off into the dark.")
}
```

This doesn't give each outcome an equal chance. The first happens a third of the time, but the second `RandomChance` is only reached the other two thirds of the time, so the second outcome happens 1/3 of 2/3 of the time - about 22% - and the last happens about 44% of the time. To make a chain like this fair, the chance at each step has to be 100 divided by the number of outcomes left: `RandomChance(33)` then `RandomChance(50)`. A chain is still useful when you want outcomes that aren't equally likely - for example, a rare event followed by a common one.

## Descriptions that don't change

Because `{random:...}` chooses again every time the text is printed, it's not right for something that should stay the same, such as the look of a particular clone. Instead, run the text through `ProcessText` once, when the clone is created, and store the result:

```quest
clone = CloneObjectAndMove(alien, room)
clone.look = ProcessText("The alien has {random:red:blue:yellow} skin.")
```

Each clone gets its own colour, and keeps it.

If the description depends on the clone's other attributes, you can refer to them as `this` in the text. `this` normally means the object in the player's current command, so set `game.text_processor_this` to the clone first:

```quest
clone = CloneObjectAndMove(alien, room)
clone.size = GetRandomInt(0, 2)
clone.weapon = CloneObjectAndMove(PickOneChild(weapons), clone)
clone.weapon_name = GetDisplayName(clone.weapon)
game.text_processor_this = clone
clone.look = ProcessText("The {select:this.size:big:huge:enormous} alien has {random:red:blue:yellow} skin, and is armed with {this.weapon_name}.")
game.text_processor_this = null
```

This gives the clone a random size, and a copy of a random weapon from a room called "weapons" that the player never visits. The `select` directive uses `this.size` to choose the word for the size, so one clone might be "The huge alien has blue skin, and is armed with a sword." The last line clears `game.text_processor_this` again, so that `this` in other text isn't left pointing at the clone.

See [Clones](/howto/scripting/clones) for more on creating clones.
