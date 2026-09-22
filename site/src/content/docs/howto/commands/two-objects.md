---
title: Commands with two objects
description: Handle TIE CORD TO HOOK, BURN PAPER WITH MATCH and other actions that involve two objects
sidebar:
  order: 3
---

Some actions need two objects: `TIE CORD TO HOOK`, `CUT ROPE WITH KNIFE`, `IGNITE FIREWORK WITH MATCH`, `GET HOT COAL WITH TONGS`. There are three ways to handle them, and it's worth checking the first two before writing a command.

| You want | Use |
|---|---|
| USE X ON Y or GIVE X TO Y, for a few specific pairs | The object's _Use/Give_ tab - no code needed |
| Your own joining word ("with", "using") on one object | A [two-object verb](/howto/commands/verbs#two-object-verbs) |
| Your own wording, or objects in either order, or many pairs handled by one rule | A command |

## USE and GIVE

Quest Viva already understands `USE KNIFE ON DOOR` (or WITH instead of ON) and `GIVE SPOON TO MARY`. To set them up, select the object, tick "Use/Give" on its _Features_ tab, and go to the _Use/Give_ tab that appears.

The tab is in sections. The ones you'll want most are:

- **Use (other object) on this** - what happens when something is used on this object. Set **Action** to "Handle objects individually" and you get a list: add an entry per object, and write a script for each. "Use any other object on this" catches everything else.
- **Give (other object) to this** - the same for giving something to this object, which is usually a character.

There are matching "Use this on (other object)" and "Give this to (other object)" sections, flagged as advanced, for when it's easier to put the response on the object being carried than on the target. Quest Viva tries the target's list first, then the carried object's.

Two more sections, **Use (on its own)** and **Give (on its own)**, handle plain `USE LAMP` and `GIVE SPOON`. Set **Action** to "Default behaviour" and tick "Display menu of objects this can be used on" (or "given to") to have Quest Viva ask which object to use it on.

When nothing matches, the player gets "You can't use it that way." or "She does not want it."

## The command pattern

If USE and GIVE don't fit, add a command. The pattern needs two placeholders, and both must start with `object`:

```
tie #object1# to #object2#
```

Quest Viva matches each placeholder against the objects around the player, including any alternative names you gave them on the _Object_ tab, so `TIE THREAD TO HOOK` works too if "thread" is one of the cord's other names. Add alternative wordings by separating them with semicolons:

```
tie #object1# to #object2#; attach #object1# to #object2#; fasten #object1# to #object2#
```

That gets unwieldy if the joining word varies as well. Switch the Pattern dropdown from "Command pattern" to "Regular expression" and you can write all the variations at once - see [Pattern matching](/howto/commands/regular-expressions).

More than two objects is possible (`#object3#` and so on), but a command the player has to phrase that precisely is rarely worth having.

## Which object is which

In the script, each placeholder becomes a variable of the same name, holding the object it matched. With `tie #object1# to #object2#`, `object1` is what's being tied and `object2` is what it's being tied to. They are always in pattern order, not in the order the player typed them - so if your pattern also has a reversed alternative, `#object1#` is still the thing being tied.

Because Quest Viva only matches objects the player can see, the script doesn't need to check that either object is there. What it does need to check is whether this particular pair makes sense. The usual shape is a cascade that tests each requirement in turn and stops at the first failure:

```quest
if (not object1.parent = game.pov) {
  msg ("You are not holding " + GetDisplayAlias(object1) + ".")
}
else if (not object1 = cord) {
  msg ("You can't tie " + object1.article + " to anything.")
}
else if (not GetBoolean(object2, "attachable")) {
  msg ("You can't tie anything to " + object2.article + ".")
}
else {
  msg ("You tie the cord to the " + GetDisplayAlias(object2) + ".")
  cord.take = false
  cord.parent = game.pov.parent
  cord.attachedto = object2
}
```

Use `game.pov` rather than `player`, so the command still works if your game lets the player change character.

The third check is the one to copy: instead of naming the hook, it tests an `attachable` attribute, so anything you flag becomes a valid thing to tie the cord to. Add it on each object's _Attributes_ tab as a Boolean set to true. `GetBoolean` returns false rather than failing when an object doesn't have the attribute at all.

Storing `object2` in `cord.attachedto` records what happened, which is what an UNTIE command needs:

```quest
if (not HasObject(object, "attachedto")) {
  msg (CapFirst(GetDisplayName(object)) + " " + Conjugate(object, "be") + "n't tied to anything.")
}
else {
  msg ("You untie the cord from the " + GetDisplayAlias(object.attachedto) + ".")
  object.attachedto = null
  object.take = true
  object.parent = game.pov
}
```

## When the player is vague

If two objects in the room match what the player typed, Quest Viva asks before running the command, and nothing extra is needed from you:

```
> tie ball to hook

Please choose which 'ball' you mean:
1: red ball
2: blue ball
```

If one of the words matches nothing, the command doesn't run at all. The player gets "I can't see that.", followed by the word that failed in brackets so they know which of the two was the problem. To replace that with something in your game's voice, set **Unresolved object text** on the Command tab:

- **Text** - your own message. Quest Viva still appends the unmatched words in brackets when the pattern has more than one placeholder.
- **Run a script** - the script gets `object` (the text the player typed, as a string) and `key` (which placeholder failed, such as `"object2"`).

```quest
msg ("You look around for " + object + ", but there's nothing like that here.")
```

The **Scope** box on the same tab changes where Quest Viva looks first for each placeholder - `object1=inventory|object2=notheld`, for instance. It's a preference, not a restriction: if nothing in that scope matches, Quest Viva still falls back to everything the player can see. See [Advanced scope](/howto/commands/scope).

## Filling in the second object yourself

Sometimes the player shouldn't have to name the second object at all. `BURN PAPER` is reasonable if there's an obvious fire in the room. Give the two-object command a **Name** on the Command tab - `cmd_burn_with`, say - then write a second, one-object command that finds the missing object and calls the first one's script:

```quest
fires = FilterByAttribute(ScopeReachable(), "fire", true)
if (ListCount(fires) = 0) {
  msg ("There is no fire here.")
}
else {
  params = NewDictionary()
  dictionary add (params, "object1", object)
  dictionary add (params, "object2", ObjectListItem(fires, 0))
  do (cmd_burn_with, "script", params)
}
```

The dictionary supplies exactly the variables the other command's script expects. All the checking and all the messages stay in one place, so `BURN PAPER` and `BURN PAPER IN FIREPLACE` can never drift apart. If there's more than one fire in the room, this picks the first; offer a [menu](/howto/scripting/asking-the-player#menus) instead if that matters.
