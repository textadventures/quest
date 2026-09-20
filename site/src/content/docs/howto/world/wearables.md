---
title: Wearable items
description: The Wearable feature - layers and slots, advanced options, clothing with several states, and the functions for dressing the player from a script
sidebar:
  order: 6
---

Clothing is a built-in feature. Tick "Wearable: object can be put on and taken off" on the object's [_Features_ tab](/howto/world/features#object-features), then go to the _Wearable_ tab that appears and change the dropdown from "Cannot be worn" to "Can be worn".

That is enough for `WEAR` and `REMOVE` to work, and for "Wear" and "Remove" to appear as verbs in the inventory pane at the right moment. A worn garment gets "(worn)" added to its name, and cannot be dropped until it is taken off.

```
> wear trousers
You put them on.
> i
You are carrying trousers (worn).
```

If your garment comes out as "a trousers" and "You put **it** on", that is not a clothing setting: go to the object's _Setup_ tab, set the type to "Inanimate objects (plural)" and untick "Use default prefix and suffix".

## The Wearable tab

| Option | What it does |
| --- | --- |
| Can be worn? | Turns the feature on for this object. |
| Wear Layer | Which layer the garment sits in. Default 2. |
| Wear Slot | Where on the body it is worn - a list of strings you make up, such as "lower" or "head". |
| Message to print when wearing | Replaces "You put it on." |
| Message to print when removing or trying to remove | Replaces "You take it off." |
| Adapting the inventory verbs to the states | Ticked by default: Quest Viva offers "Wear" only when the garment can actually be worn and "Remove" only when it can be taken off. Untick it to manage the verb lists yourself. |

## Layers and slots

Slots are where a garment is worn, and you decide what they are. A simple division might be feet, lower, upper and head. Put "lower" in the trousers' Wear Slot list, and "lower" on a pair of underpants too, but give the underpants a Wear Layer of 1 instead of 2:

```
> wear trousers
You put them on.
> wear underpants
You cannot wear that over trousers.
```

For garments sharing a slot, the player can only put on something with a **higher** layer than what is already worn there, and can only take off the garment with the highest layer. Garments can occupy more than one slot - overalls could be both "lower" and "upper" - and a garment with no slots at all can always be worn.

Layer zero is special: it counts as every layer. A pair of shorts at layer 0 can be worn neither under trousers nor over underpants.

Slot names are case-sensitive and are never checked, so jeans in "Lower" and trousers in "lower" can be worn at the same time. To catch that, put `msg (Slots())` in the game's start script while you are building and look for near-duplicates in the list it prints. Take it out again before you publish.

## Advanced options

Tick "Show advanced options for wearables" on the game object's [_Features_ tab](/howto/world/features#game-features) and the _Wearable_ tab gains:

| Option | What it does |
| --- | --- |
| Removeable? | Untick for a cursed or permanently fixed garment. Add a string attribute `notremoveablemessage` on the _Attributes_ tab for a custom refusal. |
| Protection (if implemented!) | The `armour` attribute, for a game with its own combat system. See [RPG games](/howto/rpg/rpg-intro). |
| Wearing gives a bonus to these attributes | Semicolon-separated attributes on the player that change while the garment is worn, for example `protection;charisma+2;agility-1`. The default is +1, and the bonuses are removed when the garment comes off. NPCs are not affected. |
| Additional display verbs: Inventory / Worn | Extra verbs while the garment is carried, and while it is worn (and outermost). Semicolon-separated. |
| After wearing / After removing | Text or a script, run once the garment has been put on or taken off. |
| Multistate? | Reveals the multi-state lists - see below. |

To change the extra verbs during play, set `invverbs` or `wornverbs` and call `SetVerbs`:

```quest
pink_hat.wornverbs = "Activate;Show off"
pink_hat.invverbs = "Activate"
SetVerbs
```

## Clothing with several states

A garment can be worn in more than one way - a jacket that is open, half-buttoned or fastened. Tick "Multistate?" and four lists appear. Each line across the four describes one state, so they must all have the same number of entries, and the first state is how the garment starts whenever it is put on.

| List | For our jacket |
| --- | --- |
| Descriptor for each state (or * for none) | `unfastened`, `half-buttoned`, `*` |
| Wear slots for each state (or * for none) | `*`, `*`, `*` |
| Additional verbs for each state (or * for none) | `Fasten`, `Fasten;Unfasten`, `Unfasten` |
| Bonus to attributes (or * for default) | `*`, `*`, `warm+2` |

`*` means "nothing" for the descriptor and "the garment's own setting" for the other three. Fill in all four lists even if every entry is `*` - a multi-state garment with an empty bonuses list raises an error the moment it is worn.

Wear the jacket now and it shows as "a jacket (worn unfastened)", with a "Fasten" verb. We cannot use "Open" as the verb name, because that belongs to the container system.

The verbs in that third list are only *displayed*; you still have to write them. On the _Verbs_ tab, add a verb "fasten", set it to run a script, and move the garment with `SetMultistate (object, state)` - states are numbered from 1, and an out-of-range state raises an error:

```quest
if (not this.worn) {
  msg ("You're not wearing it.")
}
else if (this.multistate_status = 3) {
  msg ("It is already fastened.")
}
else {
  msg ("You button up the jacket.")
  SetMultistate (this, this.multistate_status + 1)
}
```

Every transition verb follows the same shape: check the garment is worn, check it is not already in that state, then change state. Note that "fasten" moves up one state at a time, so it goes to half-buttoned first and fastened second.

To make the jacket removable only when it is unfastened, set the `removeable` flag as you change state and call `SetVerbs` so the verb lists catch up. In "fasten", after `SetMultistate`:

```quest
this.removeable = false
SetVerbs
```

and in "unfasten", which calls `SetMultistate (this, this.multistate_status - 1)`, again straight after it:

```quest
if (this.multistate_status = 1) {
  this.removeable = true
}
else {
  this.removeable = false
}
SetVerbs
```

## Dressing the player from a script

`WearGarment (object)` puts a garment on: it moves the object into the player's inventory if it is not there already, sets everything up, and prints nothing. Use it in the game's start script for clothes the player begins in, outermost last:

```quest
WearGarment (underpants)
WearGarment (trousers)
WearGarment (shirt)
```

`RemoveGarment (object)` is the counterpart. To strip the player completely:

```quest
foreach (o, GetAllChildObjects(game.pov)) {
  if (GetBoolean(o, "worn")) {
    RemoveGarment (o)
  }
}
```

`ListClothes()` returns a string such as "underpants, trousers and a hat" (or "nothing"), and `GetOuter (slot)` returns the outermost garment in a slot, or null - so `GetOuter("feet")` tells you whether the player is barefoot. `ListWornFor`, `ListVisibleFor` and `GetOuterFor` do the same for any other character, taking it as their first parameter. See the [clothing functions](/reference/functions/clothing) for the full list.

## Checking before wearing or removing

[Override](/advanced-topics/overriding) `TestGarment` to veto putting something on. It takes the garment and returns a boolean - print a message and return false to refuse:

```quest
if (GetBoolean(object, "toosmall")) {
  msg ("That is too small for you!")
  return (false)
}
return (true)
```

It is called only after Quest Viva has established that the object is wearable, held and not already worn, so you do not need to check any of that.

`TestRemove` is the same thing for taking a garment off. This version keeps the player decent in public - rooms where it is acceptable have a boolean attribute `private`:

```quest
if (GetBoolean(game.pov.parent, "private")) {
  return (true)
}
// Hypothetically, what would be left if this came off?
object.worn = false
covered = GetOuter("lower")
object.worn = true
if (covered = null) {
  msg ("You can't take that off out here!")
  return (false)
}
return (true)
```

Unsetting `worn` and putting it straight back is how you ask "what would the player be wearing without this?" - the garment is not actually removed until `TestRemove` has returned true.

## Renaming a garment

Because Quest Viva keeps rewriting a worn garment's alias to add "(worn)" and any state descriptor, setting `alias` directly on a worn garment confuses it. Use `SetAlias (object, alias)`, or `SetListAlias (object, alias, listalias)` if the object pane needs a different name:

```quest
SetListAlias (trendy_jacket, "unfashionable jacket", "Unfashionable jacket")
```

Both work on any object, not just clothing.

## Worn clothing and the inventory

If you use [inventory limits](/howto/world/taking-and-dropping#inventory-limits), wearing something raises the player's limit by one and removing it lowers it again, so worn items effectively do not count. Volume limits are unaffected - the player is still carrying the weight.

To keep worn clothes out of the main inventory listing, create a [container](/howto/world/containers) on the player object, give it a boolean attribute `wornclothinglocation` set to true, and untick "Drop" on its _Inventory_ tab. Anything worn goes inside it, and closing the container hides the clothes from the listing.
