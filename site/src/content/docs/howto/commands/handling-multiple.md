---
title: Handling multiple items (and all)
description: What TAKE ALL and DROP ALL include, how to exclude an object, and how to accept ALL in your own commands
sidebar:
  order: 4
---

Some commands let the player act on several objects at once, either by listing them - `DROP BAT, BALL AND HAT` - or by saying `ALL`. This page covers what Quest Viva includes when they do, and how to accept it in a command of your own.

Four built-in commands support it: TAKE (and GET), DROP, WEAR and REMOVE. No others do, and most commands don't need to.

Whichever form the player uses, the whole thing is one turn: turn scripts fire once, not once per object.

## What the player can type

- `ALL` or `EVERYTHING` - everything in the command's scope, minus the exclusions below.
- A list separated by commas, "and", or both: `TAKE BOOK, LAMP AND ROPE`.

There is no EXCEPT. `TAKE ALL EXCEPT THE HAT` is read as the name of one object, and gets "I can't see that.". Mixing `ALL` into a list doesn't work either: as soon as Quest Viva reaches the word `ALL` it uses the whole scope and ignores the rest of the list, including anything named before it.

## What ALL includes

Take a room with a rucksack (containing a book), an open cupboard (containing a ball of string), a character called Mary holding a cup, a table with an apple on it, and a door that is scenery:

```
You can see a rucksack (containing a book), a cupboard (containing a ball of string), a mary (containing a cup) and a table (on which there is an apple).

> TAKE ALL
rucksack: You pick it up.
cupboard: You can't take it.
ball of string: You pick it up.
table: You can't take it.
apple: You pick it up.
```

Several things to notice:

- The **book isn't taken**, because the rucksack it's in has already been taken. The ball of string and the apple *are* taken, because the cupboard and the table stay behind.
- The **door isn't mentioned at all**, because objects flagged as scenery are never included.
- **Mary isn't taken**, and neither is her cup. Characters are excluded automatically, and so is anything a character is carrying.
- The cupboard and the table are tried and refused. TAKE ALL doesn't quietly skip things it can't pick up.

Making a character a transparent container, as Mary is here, is a good way to show what they're carrying - it doesn't put those items into ALL.

`DROP ALL` is simpler: everything the player is carrying, with anything inside a dropped container going along with it and not listed separately.

### Excluding an object

To keep an object out of ALL, tick "Object is excluded when entering TAKE ALL" on its _Inventory_ tab. The player can still take it by naming it. Behind the scenes this sets a Boolean attribute called `not_all`.

Anything inside an excluded object is excluded too. If that isn't what you want, flag the container as scenery on its _Setup_ tab instead.

### Including a character

To make a character takeable - Mary is really a poodle - untick "Object is excluded when entering TAKE ALL" and tick "Object can be taken" as usual. Anything the character is carrying still stays out of ALL.

## Accepting ALL in your own command

Three things are needed.

**1. Turn it on.** The Command tab has no control for this, so go to the command's _Attributes_ tab, add an attribute called `allow_all`, set its type to Boolean, and tick it.

**2. Set the scope.** This is what `ALL` means for this command. Leave it blank and it means everything the player can see, which is rarely right - including, for TAKE, the things they are already carrying. Use the **Scope** box on the Command tab: TAKE uses `notheld`, while DROP, WEAR and REMOVE use `inventory`. See [Advanced scope](/howto/commands/advanced-scope) for the other values.

**3. Handle a list in the script.** With `allow_all` set, the `object` variable is always an object *list*, even when the player named a single object. A second variable, `multiple`, is true when the player said `ALL` or gave a list, and false when they named exactly one thing.

Here is the built-in TAKE command, which is a good template:

```quest
took_something = false
foreach (obj, object) {
  // if this is multiple then we should skip anything in a container that has already been taken
  // (always earlier in the list) and anything held by an NPC.
  // Scenery and anything flagged "not_all" will already be excluded
  if (not multiple or (not Contains(game.pov, obj.parent) and not DoesInherit(obj.parent, "npc_type"))) {
    DoTake (obj, multiple)
    took_something = true
  }
}
if (multiple and not took_something) {
  msg (Template("NothingToTake"))
}
```

The `multiple` checks are there for two reasons:

- **Skipping.** When the player named an object, act on it whatever the circumstances - they asked for it, and deserve a real answer. When the list came from `ALL`, silently skip the ones that would be pointless or absurd. TAKE skips anything already inside something the player just took, and anything a character is holding. Scenery and `not_all` objects are gone before the script runs.
- **Nothing to say.** If `ALL` matched nothing, or everything got skipped, the player still needs a reply. A command should never produce no output at all.

Prefix each line with the object's name when handling a list, so the player can tell the responses apart. TAKE does this inside `DoTake`; in your own script, use `OutputTextNoBr (GetDisplayAlias(obj) + ": ")` before the response.

Putting it together, a POLISH command that accepts ALL:

```quest
polished = false
foreach (obj, object) {
  if (not multiple or GetBoolean(obj, "polishable")) {
    if (multiple) {
      OutputTextNoBr (GetDisplayAlias(obj) + ": ")
    }
    msg ("You give " + obj.article + " a quick polish.")
    polished = true
  }
}
if (multiple and not polished) {
  msg ("There is nothing here worth polishing.")
}
```
