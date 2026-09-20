---
title: Turning one thing into another
description: Combine objects into something new, or have a machine transform one object into another
---

Sometimes the player needs to make something - tie a string to a branch to make a bow, or put a tray in a machine and get something else out. This page shows how to do both.

Quest Viva can't turn an object into a different object. Instead, you create every object you need at the start and keep the ones that don't exist yet out of the game. When the change happens, you remove the old objects and bring in the new one. To the player, it looks like one thing has turned into another.

Changes of amount are different. Filling a cup with water or drinking some of it changes a number on the cup, not which objects exist - see [Liquids](/howto/tasks/handling-water).

## Keeping objects out of the game

Create a room called `nowhere`, with no exits to or from it, and put the objects that don't exist yet inside it. The player can never get there, so they can't see or use those objects until you move them.

When you get rid of the parts, use the "Remove object" script command (`RemoveObject`). It takes the object out of the game altogether.

## Making a bow

Create a `string` and a `branch` that the player can take, and a `bow` in the `nowhere` room. Make the bow takeable too.

The bow can be made in several ways - MAKE BOW, TIE STRING TO BRANCH, USE STRING ON BRANCH - so put the work in a [function](/howto/tasks/about-functions) and call it from each one. Add a function called `MakeBow`, with no parameters:

```quest
if (not Got(branch)) {
  msg ("You need something to make the bow from.")
}
else if (not Got(string)) {
  msg ("You need some string.")
}
else {
  RemoveObject (string)
  RemoveObject (branch)
  MoveObject (bow, game.pov)
  msg ("You tie the string to each end of the branch. Now you have a bow - of sorts.")
}
```

In the editor, the checks are "If" with the condition "player is not carrying object", and the rest is "Remove object", "Move object" and "Print a message".

### MAKE BOW

Add a command with the pattern:

```
make bow;make a bow
```

We can't use `#object#` in the pattern here, because the bow isn't in scope yet - it's in `nowhere`. For the script, use "Call function" to call `MakeBow`:

```quest
MakeBow
```

### USE STRING ON BRANCH

Players may also try USE STRING ON BRANCH, or USE BRANCH ON STRING. The Use/Give feature handles both.

On the string, tick _Use/Give_ on the _Features_ tab. On the _Use/Give_ tab, under "Use (other object) on this", set the action to "Handle objects individually", add the branch, and give it a script that calls `MakeBow`. Then do the same under "Use this on (other object)".

The first handles USE BRANCH ON STRING, and the second handles USE STRING ON BRANCH. You only need to set this up on one of the two objects.

### TIE STRING TO BRANCH

This command refers to two objects that are both in scope, so it can use `#object1#` and `#object2#` in its pattern:

```
tie #object1# to #object2#;fasten #object1# to #object2#
```

"tie" is one of Quest Viva's built-in verbs, but a command with more fixed words, like "tie ... to ...", takes priority when it matches. The script checks the player named the right two objects, in either order:

```quest
if (object1 = string and object2 = branch) {
  MakeBow
}
else if (object1 = branch and object2 = string) {
  MakeBow
}
else {
  msg ("That's not going to work.")
}
```

```
> tie string to spoon
That's not going to work.

> tie string to branch
You tie the string to each end of the branch. Now you have a bow - of sorts.
```

## A machine that transforms things

This example is a T-remover: the player puts an object in its compartment, closes it and presses the button, and the object comes out without its letter T - a tray becomes Ray, a tape becomes an ape.

### Setting up the objects

Create a `tray` and a `tape` the player can take. In the `nowhere` room, create `ray` (set its type on the _Setup_ tab to "Male character (named)", so the game says "Ray" rather than "a Ray") and a takeable `ape`.

Each object that the machine can change needs to know what it changes into. On the tray's _Attributes_ tab, add an attribute called `convertsto`, choose "Object" as its type, and pick `ray`. Give the tape a `convertsto` of `ape`. Objects without a `convertsto` attribute come out unchanged.

### The machine

Create an object called `t_remover`, with the alias "T-remover" - object names can't contain a hyphen. On its _Object_ tab, add the other names "machine", "compartment", "button" and "red button", so the player can refer to any of them.

On the game's _Features_ tab, tick _Inventory limits_ - the editor only allows limited containers when it's on. Then, on the machine's _Features_ tab, tick _Container_, and on the _Container_ tab:

- set the container type to "Limited container" - it holds one object by default, which is what we want
- untick "Is open", so the compartment starts closed
- under "Advanced", tick "List children when object is looked at or opened", so the player can see what's inside

Tick _Use/Give_ on the machine's _Features_ tab, and on the _Use/Give_ tab set "Use (on its own)" to "Run script":

```quest
contents = GetDirectChildren(this)
if (this.isopen) {
  msg ("You press the button, but nothing happens. Perhaps the compartment needs to be closed?")
}
else if (ListCount(contents) = 0) {
  msg ("The machine hums for a moment, then stops. Perhaps it needs something in the compartment?")
}
else {
  oldobj = ObjectListItem(contents, 0)
  if (HasObject(oldobj, "convertsto")) {
    msg ("The machine rattles and shakes, there is a loud pop, and then it is quiet again.")
    MoveObject (oldobj.convertsto, this)
    RemoveObject (oldobj)
  }
  else {
    msg ("The machine clanks unhappily for a moment, then falls silent.")
  }
}
```

`GetDirectChildren(this)` is a list of the objects directly inside the machine. The machine only holds one object, so we take the first. Its `convertsto` attribute tells us what to put in its place.

The machine doesn't need any attributes of its own to track what state it's in. `isopen` and what's inside it are already known to Quest Viva, so the script just asks.

To let the player PRESS BUTTON, add a "press" verb on the _Verbs_ tab, set to "Run a script":

```quest
do (this, "use")
```

This runs the "Use (on its own)" script, so the machine behaves the same whichever way the player starts it.

### Describing it

Because the compartment has to be opened and closed, the description should say which it is. Set the _"Look at" object description_ on the _Setup_ tab to "Run script":

```quest
s = "This strange machine has a compartment, which is "
if (this.isopen) {
  s = s + "open."
}
else {
  s = s + "closed."
}
s = s + " There is a big red button on the top."
msg (s)
```

The "List children" option adds "It contains a tray." after the description when the compartment is open.

### Letting Ray out

Ray shouldn't stay in the compartment. On the _Container_ tab, add an "After opening the object" script:

```quest
if (ray.parent = this) {
  MoveObject (ray, this.parent)
  msg ("A boy leaps out of the machine! Hey, it's Ray, that kid from school.")
}
```

Here's the whole thing working:

```
> open machine
You open it.

> put tray in machine
Done.

> close machine
You close it.

> press button
The machine rattles and shakes, there is a loud pop, and then it is quiet again.

> open machine
You open it.
A boy leaps out of the machine! Hey, it's Ray, that kid from school.
```

If the player can find many objects with a T in their names, make sure the game can still be finished whatever they put in the machine.

## See also

- [Containers and surfaces](/howto/world/containers)
- [Custom commands](/tutorial/custom-commands)
- [Using verbs](/howto/commands/using-verbs)
