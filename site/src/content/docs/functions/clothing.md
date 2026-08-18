---
title: "Clothing functions"
sidebar:
  order: 10
---

## GetArmour
```
GetArmour ()
```

Returns an [int](/types#int) giving an overall total for the armour for the player, based on protection values for items worn in specific slots.

For more on handling wearable objects, see [here](/howto/world/wearables).

## GetArmourFor
```
GetArmourFor (object character)
```

Returns an [int](/types#int) giving an overall total for the armour for the character, based on protection values for items worn in specific slots.

For more on handling wearable objects, see [here](/howto/world/wearables).

## GetOuterFor
```
GetOuterFor (object character, string slot)
```

Returns an [object](/types#object), the outermost garment (i.e., with the highest layer attribute) in the given slot, for the character. Returns `null` if there is nothing in that slot.

For more on handling wearable objects, see [here](/howto/world/wearables).

## GetOuter
```
GetOuter (string slot)
```

Returns an [object](/types#object), the outermost garment (i.e., with the highest layer attribute) in the given slot, for the player. Returns `null` if there is nothing in that slot.

For more on handling wearable objects, see [here](/howto/world/wearables).

## ListWornFor
```
ListWornFor (object character)
```

Returns an [object list](/types#objectlist) containing all the items worn by the character.

## RemoveGarment
```
RemoveGarment (object)
```

The given object will stop being worn by the player. It will remain in the player's inventory. It will stop being flagged as "worn", and have its inventory verbs updated. This function is used by the REMOVE command, and should be used any other time the player will take off a garment.

For more on handling wearable objects, see [here](/howto/world/wearables).

## WearGarment
```
WearGarment (object)
```

The given object will become worn by the player. It will be moved to the player's inventory, if not already there, be flagged as "worn", and have its inventory verbs updated. This function is used by the WEAR command, and should be used any other time the player will wear a garment (for example, to have an item worn at the start of the game).

For more on handling wearable objects, see [here](/howto/world/wearables).
