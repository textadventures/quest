---
title: "Clothing functions"
sidebar:
  order: 10
---

## AfterRemove
```quest
AfterRemove ()
```

Override hook: called at the end of [RemoveGarment](#removegarment), after any object-specific `onafterremove` script or message has run. The default implementation does nothing - override it in your game for effects that should happen whenever *any* garment is removed. See also [AfterWear](#afterwear).

## AfterWear
```quest
AfterWear ()
```

Override hook: called at the end of [WearGarment](#weargarment), after any object-specific `onafterwear` script or message has run. The default implementation does nothing - override it in your game for effects that should happen whenever *any* garment is worn. See also [AfterRemove](#afterremove).

## ClothingBonusMultiplier
```quest
ClothingBonusMultiplier ()
```

Override hook returning an [int](/types#int), used by [SetBonuses](#setbonuses) to scale the attribute bonuses/penalties granted by worn `bonusatts`. The default implementation returns 1; override it to double or triple clothing effects in certain situations (e.g. a "buff" status). For more, see [here](/howto/world/wearables).

## GetArmour
```quest
GetArmour ()
```

Returns an [int](/types#int) giving an overall total for the armour for the player, based on protection values for items worn in specific slots.

For more on handling wearable objects, see [here](/howto/world/wearables).

## GetArmourFor
```quest
GetArmourFor (object character)
```

Returns an [int](/types#int) giving an overall total for the armour for the character, based on protection values for items worn in specific slots.

For more on handling wearable objects, see [here](/howto/world/wearables).

## GetDisplayGarment
```quest
GetDisplayGarment (object)
```

Returns a [string](/types#string): the object's displayed name as it should appear in a worn-items listing, including its prefix (or "your" if `usedefaultprefix` is set) and any multistate descriptor (e.g. "(torn)"). Used by [ListClothes](#listclothes).

## GetOuterFor
```quest
GetOuterFor (object character, string slot)
```

Returns an [object](/types#object), the outermost garment (i.e., with the highest layer attribute) in the given slot, for the character. Returns `null` if there is nothing in that slot.

For more on handling wearable objects, see [here](/howto/world/wearables).

## GetOuter
```quest
GetOuter (string slot)
```

Returns an [object](/types#object), the outermost garment (i.e., with the highest layer attribute) in the given slot, for the player. Returns `null` if there is nothing in that slot.

For more on handling wearable objects, see [here](/howto/world/wearables).

## ListWornFor
```quest
ListWornFor (object character)
```

Returns an [object list](/types#objectlist) containing all the items worn by the character.

## ListClothes
```quest
ListClothes ()
```

Returns a [string](/types#string) listing everything the current player is wearing (via [GetDisplayGarment](#getdisplaygarment) for each item), formatted as a comma-separated list with "and" before the last item, or "nothing" if nothing is worn.

## RemoveGarment
```quest
RemoveGarment (object)
```

The given object will stop being worn by the player. It will remain in the player's inventory. It will stop being flagged as "worn", and have its inventory verbs updated. This function is used by the REMOVE command, and should be used any other time the player will take off a garment.

For more on handling wearable objects, see [here](/howto/world/wearables).

## SetAlias
```quest
SetAlias (object, string alias)
```

Sets both the [alias](/attributes#alias) and list-alias of the object to the same value. Shorthand for [SetListAlias](#setlistalias) when you don't need a separate list alias. If the object is currently worn, its `display` is refreshed immediately.

## SetBonuses
```quest
SetBonuses (object garment, boolean wearing)
```

Applies (if **wearing** is true) or removes (if false) the attribute bonuses/penalties listed in the garment's `bonusatts` attribute - a semicolon-separated list of entries like `strength+2` or `speed-1` - scaled by [ClothingBonusMultiplier](#clothingbonusmultiplier). Called automatically when a garment is worn or removed.

## SetListAlias
```quest
SetListAlias (object, string alias, string list alias)
```

Sets the object's [alias](/attributes#alias) and separate list-alias (the name shown in object panes/inventory listings). If the object is currently worn, its `display` is refreshed immediately and the current alias/list-alias are also stashed as `original_alias`/`original_listalias` so they can be restored later (e.g. after a multistate change).

## SetMultistate
```quest
SetMultistate (object, integer state)
```

Sets a multistate garment's condition to the given state (a 1-based index into the object's `multistate_descriptors` list, e.g. to represent "pristine" vs "torn"). Reapplies [SetBonuses](#setbonuses) around the change so any attribute bonuses that depend on condition stay correct, and updates the garment's alias. Raises an error if **state** is out of range.

## SetVerbs
```quest
SetVerbs ()
```

Recalculates the wear/remove verb text for every wearable object the player is carrying that opts into adjective-qualified verbs (the `wear_adjverbs` attribute) and has both `wear_slots` and `wear_layer` set - e.g. so the menu offers "wear the *tattered* shirt" once you also have a clean one. Called automatically as garments are picked up, worn or removed.

## TestGarment
```quest
TestGarment (object)
```

Override hook returning a [boolean](/types#boolean), called before an object is worn. The default implementation checks the `notallowedtodress` attribute on the player and blocks (with a message) if it's set; override it in your game for custom checks, e.g. to enforce that the garment fits.

## TestRemove
```quest
TestRemove (object)
```

Override hook returning a [boolean](/types#boolean), called before an object is removed. The default implementation checks the `notallowedtoundress` attribute on the player and blocks (with a message) if it's set; override it in your game for custom checks.

## UpdateArmour
```quest
UpdateArmour ()
```

Override hook called whenever the player wears or removes a garment. The default implementation does nothing; override it to refresh a status display or store the player's current armour total (see [GetArmour](#getarmour)) on an attribute. For more, see [here](/howto/world/wearables).

## Slots
```quest
Slots ()
```

Returns a [stringlist](/types#stringlist) of every distinct `wear_slots` value used by any object in the game (e.g. "head", "torso", "feet") - the full set of clothing slots your game defines, gathered by scanning all objects rather than being declared anywhere centrally.

## WearGarment
```quest
WearGarment (object)
```

The given object will become worn by the player. It will be moved to the player's inventory, if not already there, be flagged as "worn", and have its inventory verbs updated. This function is used by the WEAR command, and should be used any other time the player will wear a garment (for example, to have an item worn at the start of the game).

For more on handling wearable objects, see [here](/howto/world/wearables).

## WornCount
```quest
WornCount ()
```

Returns an [int](/types#int): the number of items the current player is currently wearing.
