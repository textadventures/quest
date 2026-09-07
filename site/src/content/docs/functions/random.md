---
title: "Randomising functions"
sidebar:
  order: 11
---

These functions all return a random value. See also [here](/howto/tasks/random).

## DiceRoll
```quest
DiceRoll (string dicetype)
```

Returns an [int](/types#int) - the result of the dice roll.

Format dicetype: [number of dice]d[number of sides]

```quest
//roll 3 dice with 6 sides
result=DiceRoll("3d6")
if (result>12){
   msg ("you hit the troll.")
} 
```

This can also handle strings like "d6+1" and "3d8-2", using these formats:

```
d[number of sides]
[number of dice]d[number of sides]
d[number of sides]+[bonus]
[number of dice]d[number of sides]+[bonus]
d[number of sides]-[penalty]
[number of dice]d[number of sides]-[penalty]
```

## GetRandomDouble
```quest
GetRandomDouble ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a random [double](/types#double) value between 0.0 and 1.0.

## GetRandomInt
```quest
GetRandomInt (integer min, integer max)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a random [int](/types#int) value between the specified maximum and minimum.

## PickOneChild
```quest
PickOneChild (object room or container)
```

Returns an [object](/types#object), picked at random from the direct children of the given object (so if the given object is a room, this would be any object in the room, but not including objects inside containers). Returns null if there are none.

## PickOneChildOfType
```quest
PickOneChildOfType (object room or container, string typename)
```

Returns an [object](/types#object), picked at random from the direct children of the given object, and is also of the given type (so if the given object is a room, this would be any object in the room, but not including objects inside containers). Returns null if there are none.

## PickOneExit
```quest
PickOneExit (object room)
```

Returns a visible exit, picked at random from the given room. Returns null if there are none. Use the `to` attribute of the exit to find what room it goes to.

## PickOneObject
```quest
PickOneObject (objectlist)
```

Returns an [object](/types#object), picked at random from the given list. Returns null if the list is empty.

## PickOneString
```quest
PickOneString (string or stringlist)
```

Returns an [string](/types#string), picked at random from the given list. If a string is given instead, the string will be split into a list, using `;` as a separator. Returns an empty string if the list is empty.

## PickOneUnlockedExit
```quest
PickOneUnlockedExit (object room)
```

Returns a visible and unlocked exit, picked at random from the given room. Returns null if there are none. Use the `to` attribute of the exit to find what room it goes to.

## RandomChance
```quest
RandomChance (integer percentile)
```

Percentile parameter should be between 0 and 100.

This function generates a random number between 1 and 100. If the result is less than or equal to the specified percentile value, this function returns true.

The effect is that if use RandomChance(10), there is a 10% chance of the function returning true, RandomChance(50) has a 50% chance of returning true, etc.
