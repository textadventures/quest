---
title: Using dictionaries
description: Store values you look up by name - for lookup tables, menus and passing parameters to scripts
sidebar:
  order: 5
---

A dictionary holds values you look up by name. Each entry has a key - always a string - and a value. Keys are unique: a dictionary can't have two entries with the same key, in the same way a list can't have two items in the same position.

Use a list when the order matters or you just want a collection of things; use a dictionary when you want to look something up.

There are four kinds:

| Kind | Values are | Create with |
|---|---|---|
| String dictionary | Strings | `NewStringDictionary()` |
| Object dictionary | Objects | `NewObjectDictionary()` |
| Script dictionary | Scripts | `NewScriptDictionary()` |
| Dictionary | Anything, including a mixture | `NewDictionary()` |

Use the specific kind where you can. In the editor, select an object, go to the _Attributes_ tab and add an attribute of type "String dictionary" or "Script dictionary" - those are the two the editor can store. Object and general dictionaries are built in a script.

## Adding and removing entries

`dictionary add` takes the dictionary, the key and the value. `dictionary remove` takes the dictionary and the key. In the editor they're "Add a value to a dictionary" and "Remove a value from a dictionary", in the Variables category.

```quest
prices = NewDictionary()
dictionary add (prices, "roses", 5)
dictionary add (prices, "lavender", 2)
dictionary remove (prices, "roses")
```

Dictionaries are strict about keys. Adding a key that's already there, or removing one that isn't, stops the script with an error:

> Error adding key 'a' to dictionary: An item with the same key has already been added. Key: a

> The given key 'nope' was not present in the dictionary.

`DictionaryAdd` and `DictionaryRemove` are the forgiving versions: `DictionaryAdd` replaces an entry that's already there rather than complaining, and `DictionaryRemove` does nothing if the key doesn't exist.

```quest
DictionaryAdd (prices, "lavender", 3)
DictionaryRemove (prices, "orchids")
```

## Looking things up

`StringDictionaryItem`, `ObjectDictionaryItem` and `ScriptDictionaryItem` take the dictionary and a key, and `DictionaryItem` does the same for a general dictionary:

```quest
msg ("Lavender costs " + DictionaryItem(prices, "lavender") + " gold.")
```

Asking for a key that isn't there is an error too, so check first with `DictionaryContains`, or with the `in` operator, which reads better:

```quest
if ("orchids" in prices) {
  msg ("Orchids cost " + DictionaryItem(prices, "orchids") + " gold.")
}
else {
  msg ("Cindy doesn't sell orchids.")
}
```

Put brackets round the test when you negate it - `not ("orchids" in prices)` - so that it's clear the `not` applies to the whole thing.

`DictionaryCount` gives the number of entries.

## Going through a dictionary

`foreach` over a dictionary gives you each *key*, and you look the value up inside the loop:

```quest
foreach (flower, prices) {
  msg (flower + ": " + DictionaryItem(prices, flower) + " gold")
}
```

Entries come out in the order they were added. Removing an entry and adding it again moves it to the end.

Unlike a list, a dictionary can be changed inside a loop over it: the loop works from the keys as they were when it started, so entries added during the loop aren't visited, and entries removed during it still are - which means you shouldn't look those up again.

## A menu whose options you can change

`ShowMenu` accepts a string dictionary as well as a list. The player sees the values; the function returns the key of whatever they chose. That keeps the text the player reads separate from what your script tests, so you can reword an option without touching the rest of the script:

```quest
options = NewStringDictionary()
dictionary add (options, "roses", "Red roses (5 gold)")
dictionary add (options, "lavender", "Lavender (2 gold)")
choice = ShowMenu("What flowers do you want to buy?", options, true)
if (choice = "roses") {
  msg ("You buy some red roses from Cindy.")
}
```

If each key is the name of a script attribute on an object, you can run the player's choice without a `switch` at all:

```quest
options = NewStringDictionary()
dictionary add (options, "getthing", "Get the thing")
dictionary add (options, "jump", "Jump as high as you can")
choice = ShowMenu("What do you do?", options, false)
do (game.pov, choice)
```

See [Asking the player](/howto/scripting/asking-the-player) for the rest of `ShowMenu`, including the form that lets the player save the game while the menu is on screen. For a conversation with several steps, [Pages](/tutorial/using-pages) are usually a better fit than a chain of menus.

## Passing parameters to a script

A dictionary is how you hand values to a script you run with `do` or `invoke`. Each key becomes a local variable inside that script, and the value becomes its value:

```quest
vars = NewDictionary()
dictionary add (vars, "weapon", sword)
dictionary add (vars, "success", "You hit the monster")
dictionary add (vars, "failure", "You missed!")
do (monster, "attack", vars)
```

The `attack` script on the monster can now use `weapon`, `success` and `failure` as ordinary local variables. For up to three parameters, `QuickParams` builds the dictionary in one line:

```quest
do (monster, "attack", QuickParams("weapon", sword, "success", "You hit the monster", "failure", "You missed!"))
```

## Keeping scripts in a dictionary

A script dictionary stores scripts, which you then run with `invoke`. The value you add is a script attribute of an object:

```quest
d = NewScriptDictionary()
dictionary add (d, "jump", player.jump)
invoke (ScriptDictionaryItem(d, "jump"))
```

In the editor, add a "Script dictionary" attribute and use "Add entry key…" to add an entry, then write the script for it in place. This is how Quest Viva's own ask/tell system stores an NPC's answers.

## Dictionaries in attributes

Like lists, a dictionary behaves differently once it's in an attribute. **Assigning one to an attribute stores a copy**, so changing the original afterwards doesn't change the attribute:

```quest
d = NewStringDictionary()
dictionary add (d, "roses", "Red roses")
game.options = d
dictionary add (d, "lilies", "Lilies")
msg (DictionaryCount(game.options))
// -> 1
```

**Reading the attribute gives you the real dictionary, not a copy**, so `dictionary add (game.options, ...)` - or adding to a variable you read it into - does change what's stored.

## See also

- [Dictionary functions](/reference/functions/dictionary) - the full reference
- [Using lists](/howto/scripting/using-lists) - for values in order rather than looked up by name
- [Asking the player](/howto/scripting/asking-the-player) - menus and questions in full
