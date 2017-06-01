---
layout: index
title: Attack of the Clones!
---


A clone is an exact copy of a prototype, and can be a useful way to quickly create several of the same things whilst a game is underway. For example, you could create a single orc, and then clone it several times to give the player a hoard to fight against, or you could implement a shop where all the goods for sale get cloned when the player purchases them, so the shop remains stocked.

Actually clones are not exact copies. Every object in Quest must have a unique name, so each clone will have its own name. This name will be the name of the prototype, with a number appended (orc1, orc2, etc.). To ensure the clone is _apparently_ identical to the player, it is best to use the `CloneObject` function (rather than the `Clone` function), so the clone will be given the name of the prototype as an alias, if none is already set.

Alternatively, use the `CloneObjectAndMove` function, which uses `CloneObject`, but then moves the clone to the given location, or the `CloneObjectAndMoveHere` function which is similar but moves the clone to the current room.

All of these function return the clone, which allows you to modify if required.

```
newgoblin = CloneObjectAndMoveHere(goblin)
newgoblin.look = "This goblin has a wooden leg."
```

Once the player is interacting with a clone, you need to ensure that it is the clone that things happen to, not the prototype. And this is where it gets complicated...

In the code above, `goblin` is an object that has the name "goblin", and this is the way of all objects in Quest. However, `newgoblin` is quite different; it is a _local variable_, which is a kind of temporary container. The first line above puts the clone into the container, and we can then use it as though it is the real thing, as you see above. That is fine in that script, but as soon as the script ends, the container is gone. The clone still exists; that is now a part of the game world, but we have lost the container called "newgoblin".

So what is the clone called? The first will be "goblin1", the second "goblin2", and so on. We need a way to refer to the clone, without knowing its name.


Commands
--------

In commands, we have a system built-in. Say there is an ATTACK command, the pattern could be:

> attack #object#

In the command script, we now have a _local variable_ called "object". It has a different name, but this is just a container like `newgoblin`, and just as with `newgoblin`, we can change it around in our script.

```
object.hitpoints = object.hitpoints - 5
```



Scripts
-------

Scripts are attached to an object, and when the object is cloned, the clone has that script too. All well and good... unless the script refers to the object by name. Suppose you have a script for LOOK, and it is set up like this:

```
msg("The goblin is green, small and looks nasty. It currently has " + goblin.hitpoints + " hit points.")
```

Now every time the player looks at the clone, she will be told how many hits the _prototype_ has! The solution is to use the special variable `this`, which refers to the owner of the script:

```
msg("The goblin is green, small and looks nasty. It currently has " + this.hitpoints + " hit points.")
```

Now when the player looks at the goblin, she will get told the hit points for the owner of the script; the clone not the prototype.

Note that the text processor does not currently support `this`, so cannot be used here.

Verbs use scripts, so again should be using `this`, not the name of the prototype.

Got a clone?
------------

Sometimes you want to check if the player has a specific item. What if you want to check if the player has a clone of a specific item? Clearly the standard `Got` function will not work here - that will check if the player has the prototype.

So let's create a new function. Call it `PlayersClone`, give it a single parameter, "obj", and have it return an object.

Paste in this code:

```
    foreach (o, ScopeInventory()) {
      if (obj.alias = o.alias) {
        return (o)
      }
    }
    return (null)
```

This assumes the prototype has an alias set; it is up to you to ensure that is so. It goes through the player's inventory until it finds an object with the same alias, and retuirns that object.

So now if we want to check if the player has the short sword, or a clone of it, we can check if this function does not return null.

```
if (not (PlayersClone(short sword)) {
  msg ("Lucky you have that short sword with you.")
}
```




