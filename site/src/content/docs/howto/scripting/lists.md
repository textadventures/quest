---
title: Using lists
description: Store several values in one place, and read, iterate, filter, combine and sort them
sidebar:
  order: 4
---

A list holds any number of values in order. Each value has a position, counting from zero: the first item is at position 0, the second at position 1, and so on.

There are three kinds:

| Kind | Holds | Create with |
|---|---|---|
| String list | Strings only | `NewStringList()`, or `Split(...)` |
| Object list | Objects only | `NewObjectList()`, or a [scope function](/reference/functions/scope) |
| List | Anything, including a mixture | `NewList()` |

Use a string list or an object list whenever you can. The functions for them return the right type, so you can chain them together, and Quest Viva can tell you when something has gone wrong.

In the editor, select an object, go to the _Attributes_ tab and add an attribute of type "String List". You can then add and remove items with the "Add item…" box. That's the only kind of list the editor stores as an attribute - object lists and general lists are usually built in a script and kept in a local variable.

## Adding and removing items

Use the `list add` and `list remove` script commands. In the editor they're "Add a value to a list" and "Remove a value from a list", in the Variables category.

```quest
verbs = NewStringList()
list add (verbs, "Poke")
list add (verbs, "Prod")
list remove (verbs, "Poke")
```

Adding something that's already in the list adds it a second time. Removing something that isn't in the list does nothing at all - no error. If a value appears more than once, `list remove` removes only the first one.

Both commands change the list in place, so you can use them on an attribute directly:

```quest
list add (hat.inventoryverbs, "Poke")
```

### "Cannot modify the contents of this list"

If the attribute belongs to a type the object inherits from rather than to the object itself, you'll get:

> Error running script: Cannot modify the contents of this list as it is defined by an inherited type. Clone it before attempting to modify.

The object needs its own copy of the list first. Assigning the list back to the object makes one:

```quest
hat.inventoryverbs = hat.inventoryverbs + "Poke"
```

After that, `list add` and `list remove` work on `hat.inventoryverbs` as normal. In the editor, adding the attribute on the object's own _Attributes_ tab has the same effect.

## Getting items out

`StringListItem` and `ObjectListItem` take the list and a position:

```quest
msg (StringListItem(verbs, 0))
msg (ObjectListItem(ScopeInventory(), 1).name)
```

`ListItem` does the same for a general list. Asking for a position that doesn't exist is an error:

> StringListItem: index 7 is out of range for this list (3 items, last index is 2)

Remember that removing an item shifts everything after it down by one, so a position is only worth storing if nothing is ever removed from the list.

## Going through a list

`foreach` runs a script once for each item, with the item in a variable you name. In the editor it's "For each…", in the Scripts category.

```quest
foreach (obj, ScopeInventory()) {
  msg ("You are carrying " + GetDisplayName(obj) + ".")
}
```

Adding to or removing from a list while you're looping over it is an error:

> Error running script: Collection was modified; enumeration operation may not execute.

So collect the items you want to remove as you go, and remove them afterwards:

```quest
dead = NewObjectList()
foreach (monster, game.monsters) {
  if (monster.health <= 0) {
    list add (dead, monster)
  }
}
foreach (monster, dead) {
  list remove (game.monsters, monster)
}
```

## Printing a list

`msg` prints a list as `List: one; two; three;`. Joining it to a string doesn't do what you might expect - `+` on a list means "add an item", so `"You have: " + l` puts the string at the *front of the list*. Use `Join` instead, which turns a string list back into a string:

```quest
msg ("You have: " + Join(l, ", "))
```

`Join` needs a string list. For an object list, use `FormatList`, which uses each object's display name and lets you set the word before the last one:

```quest
msg ("You are carrying " + FormatList(ScopeInventory(), ",", "and", "nothing") + ".")
```

`Split` goes the other way, and is the quickest way to write a string list out in full. The separator is optional and defaults to a semicolon:

```quest
flowers = Split("roses;lavender;lilies")
colours = Split("red|blue|green", "|")
```

## Lists of objects

Most object lists come from a function rather than being built by hand. `ScopeVisible`, `ScopeReachable` and `ScopeInventory` are the common ones - see [Scope functions](/reference/functions/scope) for the full set. `GetDirectChildren(room)` gives what's immediately inside a room or container, and `GetAllChildObjects(room)` also looks inside the containers within it.

### Filtering

`FilterByType` returns just the objects of a given type:

```quest
characters = FilterByType(ScopeReachable(), "npc_type")
```

`FilterByAttribute` and `FilterByNotAttribute` filter on an attribute's value, which can be of any type:

```quest
scenery = FilterByAttribute(ScopeVisible(), "scenery", true)
notgoblins = FilterByNotAttribute(ScopeVisible(), "alias", "goblin")
```

Leave the value off altogether and you filter on whether the attribute is there at all: `FilterByAttribute(ScopeVisible(), "health")` gives you everything that has a `health` attribute, whatever its value, and `FilterByNotAttribute(ScopeVisible(), "health")` everything that hasn't.

All three return a new object list and leave the original alone, so you can filter a filtered list:

```quest
wounded = FilterByAttribute(FilterByType(ScopeVisible(), "npc_type"), "health")
```

## Counting and searching

`ListCount` gives the number of items - so the last position is always `ListCount(l) - 1`. `ListContains` and the `in` operator both test whether an item is in a list, and `IndexOf` gives its position, or -1 if it isn't there:

```quest
if (player in myList) {
  list remove (myList, player)
}
msg (IndexOf(flowers, "lilies"))
```

`in` works on anything that gives you a list, including a function call: `if (hat in ScopeVisible())`.

## Combining, sorting and de-duplicating

None of these change the lists you give them - each returns a new list:

| | |
|---|---|
| `ListCombine(a, b)`, or `a + b` | Everything in `a`, then everything in `b` |
| `a * b` | The same, but without adding anything from `b` that's already in `a` |
| `ListExclude(a, b)` | `a` without any of the items in `b` - `b` can also be a single item |
| `a - item` | `a` without that item |
| `a + item` | `a` with that item added at the end |
| `ListCompact(a)` | `a` with repeats and `null` entries removed, as a general list |
| `StringListCompact(a)`, `ObjectListCompact(a)` | The same, keeping the string or object list type |
| `StringListSort(a)`, `StringListSortDescending(a)` | `a` sorted alphabetically |
| `ObjectListSort(a, "attribute")`, `ObjectListSortDescending(a, "attribute")` | `a` sorted by the value of an attribute |

`ListCombine` needs both lists to be of the same kind. `ObjectListSort` takes more than one attribute name if you want to break ties: `ObjectListSort(l, "weight", "name")`.

## Picking at random

`PickOneString` and `PickOneObject` each return a random item, or an empty string or `null` if the list is empty. [Randomness](/howto/scripting/randomness) covers them and their relatives. To pick several without repeating yourself, remove each one as you take it:

```quest
remaining = Split("Ann;Bob;Cath;Dai")
while (ListCount(remaining) > 0) {
  name = PickOneString(remaining)
  list remove (remaining, name)
  msg (name)
}
```

That also shuffles a list, if you add each pick to a second list instead of printing it.

## Lists in attributes

A list stored in an attribute behaves differently from one in a local variable, in a way that's worth knowing before it surprises you.

**Assigning a list to an attribute stores a copy of it.** Changing the original afterwards doesn't change the attribute:

```quest
l = NewStringList()
list add (l, "alpha")
game.flowers = l
list add (l, "beta")
msg (game.flowers)
// -> List: alpha;
```

The same goes for copying one attribute to another: `game.b = game.a` gives `game.b` its own list, and the two then change independently.

**Reading an attribute gives you the real list, not a copy.** So this changes `game.flowers`:

```quest
l = game.flowers
list add (l, "gamma")
msg (game.flowers)
// -> List: alpha; gamma;
```

That's usually what you want - it's what lets `list add (hat.inventoryverbs, "Poke")` work - but if you need a list you can safely pull apart, take a copy first with `ListExclude(game.flowers, NewStringList())` or by assigning it to another attribute.

Assigning one local variable to another never copies: `l2 = l` leaves both names pointing at the same list.

## See also

- [List functions](/reference/functions/list) - the full reference
- [Using dictionaries](/howto/scripting/dictionaries) - for values looked up by name rather than by position
- [Scope functions](/reference/functions/scope) - the object lists the engine can give you
