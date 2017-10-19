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


It can be useful to note what the prototype was when an object was cloned so you can refer to it later. You can set it like this:


```
newgoblin = CloneObjectAndMoveHere(goblin)
newgoblin.look = "This goblin has a wooden leg."
newgoblin.prototype = goblin
```


Once the player is interacting with a clone, you need to ensure that it is the clone that things happen to. And this is where it gets complicated...

In the code above, `goblin` is an object that has the name "goblin", and this is the way of all objects in Quest. However, `newgoblin` is quite different; it is a _local variable_, which is a kind of temporary container. The first line above puts the clone into the container, and we can then use it as though it is the real thing, as you see above. That is fine in that script, but as soon as the script ends, the container is gone. The clone still exists; that is now a part of the game world, but we have lost the container called "newgoblin".

So what is the clone called? The first will be "goblin1", the second "goblin2", and so on. We need a way to refer to the clone, without knowing its name.


Commands
--------

In commands, we have a system built-in. Say there is an ATTACK command, the pattern could be:

> attack #oject#

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

Now every time the player looks at the clone, she will be told how many hits the prototype has! The solution is to use the special variable `this`, which refers to the owner of the script:

```
msg("The goblin is green, small and looks nasty. It currently has " + this.hitpoints + " hit points.")
```

Now when the player looks at the goblin, she will get told the hit points for the owner of the script; the clone not the prototype.

Note that the text processor does not currently support `this`, so cannot be used here.

Verbs use scripts, so again should be using `this`, not the name of the prototype.



Finding and checking for clones
-------------------------------

Note that neither of these techniques will flag up the prototype; it is generally best to keep the prototypes somewhere the player will never find them.

Back at the start we set the "prototype" attribute on our clones. We can use that to see if an object is a clone of a certain item. In this case, we are testing a object in a local variable called "obj".


```
if (obj.prototype = goblin) {
  msg("The is a clone of the goblin")
}
```

If you want to find all the clones in the current room, you can use ScopeVisible to get everything here, and filter that to get the clones:


```
goblin_clone_list = FilterByAttribute(ScopeVisible(), "prototype", goblin)
```

If you just want to get the clones the player is actually carrying use `ScopeInventory`.

To remove a set number of clones from the player, you might do something like this:


```
sticks = FilterByAttribute(ScopeInventory(), "prototype", stick)
if (ListCount(sticks) < 4) {
  msg("You need at least four sticks to do that.")
}
else {
  for (i, 1, 4) {
    RemoveObject (ListItem(sticks, i))
  }
  msg ("You use four sticks to do that thing.")
}
```



Specialised Functions
---------------------

If you are going to be cloning several of the same type of thing in your game, you might want to create functins to do the job for you. Let's look at some examples, from an RPG style game.

The first is `CreateTreasure`. It is going to create a clone of a given object, then mix it up a bit. It has no return type, and two parameters, obj and room. Here is the code:

```
    o = CloneObjectAndMove(obj, room)
    o.prototype = obj
    if (HasString(o, "look")) o.look = ProcessText(o.look)
    o.price = o.price - GetRandomInt(o.price/-4, o.price/4)
```

It will clone any item, move it to the given room, then, to give some variety, it will call `ProcessText` on the "look" attribute. This means that if the attribute is set to this:

> This is a {random:red:blue:green} hat.

The text processor directive will get processed now, as the item is created, and so its colour will not change each time the player looks at it. The price is also varietyed to with 25% of the prce of the prototype.

The next one, `CreateProtectionPotion` is a bit more specialised, but could readily be adapted. It take a single parameter, room. It makes a clone of a specific item, masterpotionprotection, and assigns an element from one of a set of predefined objects too.

```
    o = CloneObjectAndMove(masterpotionprotection, room)
    o.prototype = masterpotionprotection
    o.element = GetObject(PickOneStr("fire|frost|necrotic"))
    o.alias = "Potion of Protection from " + CapFirst(o.element.name)
    o.listalias = o.alias
    o.price = o.price - GetRandomInt(o.price/-4, o.price/4)
```

In this case the "look" attribute is a script, which references the attribute set in the function:

```
msg ("An inky black liquid in a small glass phial. You can see the word \"" + this.element.name + "\" in runes on the cap.")
```
    
This is a more involved example, but the principle is the same. `CreateScroll` has two parameters, level and room. The prototype, masterscroll, is cloned, and various attributes set.

```
    o = CloneObjectAndMove(masterscroll, room)
    o.prototype = masterscroll
    o.element = GetObject(PickOneStr("fire|frost|divine|storm|earthmight"))
    qualifier = StringListItem(Split("Lesser ||Greater ", "|"), level % 3)
    o.alias = "Scroll of " + qualifier + CapFirst(o.element.name) + " Blast " + Roman(level / 3 + 1)
    o.listalias = o.alias
    o.look = "The scroll has a glyph of " + o.alias + " on it."
    o.price = 10 * level
    o.level = level
```    
    
    
Finally, `CreateArmour`, which has two parameters, level and room. In this case the prototype is randomly picked from a room called "garments". It will also try to pick something that is suitable to the level, specifically has a price less than 10 times the level. It will make random picks up to 6 times.

```
    count = 0
    prototype = PickFromObject(garments)
    while (prototype.price > 10 * level and 6 > count) {
      prototype = PickFromObject(garments)
    }
    o = CloneObjectAndMove(prototype, room)
    o.prototype = prototype
    o.price = o.price - GetRandomInt(o.price/-4, o.price/4)
```  

