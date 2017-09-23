---
layout: index
title: Advanced Scope For Items
---


Sometimes you want the player to be able to access items in other rooms. For example:

-  background objects, such as walls and sky, that are common to a lot or all rooms
-  items in adjacent rooms, perhaps behind the counter is another room, but the player can still talk to the barmaid there
-  items in a specific locations, perhaps spells that are in a spellbook
-  items in a related location, perhaps the stock for a shop
-  items of a specific type that might be anywhere, such as NPCs that can be phoned

As of Quest 5.7, there are two new features that let you handle these situations relatively easily.

Extended Scope
--------------

The first is an extended scope. On the _Features_ tab of the game object, tick "Advanced scripts", then go to the _Advanced scripts_ tab. The script at the bottom allows you to add items to the scope Quest uses to decide what the player can reach.


### Scenery

The simplest way to use this is to set up a room, let us say it is called "scenery") of background objects, things like walls, floor and ceiling. Each object should be set to be scenery (_Setup_ tab). The script then adds each item in the scenery room to a special variable called "items":

```
foreach (obj, GetDirectChildren(scenery)) {
  list add (items, obj)
}
```

Note that each item has to be added individually to ensure the `items` variable is not lost; using the `ListCombine` function will fail.

Because we tagged the items as scenery, they will not show up in room lists, but the player can still examine them.

We can go further, and add scenery items according to the type of room. How you do that depends on how you flag the rooms of a certain type. In the code below, it is assumed that locations in the forest all start "Forest", etc.

```
if (StartsWith(game.pov.parent.name, "Forest")) {
  foreach (obj, GetDirectChildren(forest_scenery)) {
    list add (items, obj)
  }
}
else if (StartsWith(game.pov.parent.name, "Dungeon")) {
  foreach (obj, GetDirectChildren(dungeon_scenery)) {
    list add (items, obj)
  }
}
else {
  foreach (obj, GetDirectChildren(default_scenery)) {
    list add (items, obj)
  }
}
```

### Adjacent rooms

We can use the same system to allow the player to access objects in an adjacent room. This is fully compatible with the above; the code just needs to go after it. Here is an example:

```
if (game.pov.parent = bar room) {
  foreach (obj, GetDirectChildren(behind the counter)) {
    list add (items, obj)
  }
}
if (game.pov.parent = behind the counter) {
  foreach (obj, GetDirectChildren(bar room)) {
    list add (items, obj)
  }
}
```

What this does is check if the player is in the bar, and if so, it adds all the object that are behind the bar to the list. Also, if the player is behind the bar, it will add objects from the bar room. You can have as many of these as you like.


Alternative scope
-----------------

You can set the scope for a command. Quest will look for any matching objects in that place first. If it fails to find a match, it will then fall back to looking in the normal places (inventory and current room). You have five options:

```
"all"        ScopeVisible()
"inventory"  ScopeInventory
"notheld"   ScopeVisibleNotHeld
objectname   GetAllChildObjects(GetObject(objectname))
attrname     GetAllChildObjects(GetAttribute(player.parent, attrname))
```

The first is the default. The second tells Quest to look in the inventory; if the players is carrying a hat and there is another on the ground, typing WEAR HAT will put on the one being held, because WEAR is set to "inventory". Conversely, "notheld" makes Quest look in the room first.

If the text is set to the name of an object, Quest will look at the children of that object; that might be the objects in another room. This could be used for a spellbook, containing spells.

If the text is an object attribute of the current location, Quest will look at the children of that object. This could be used for a stockroom of a shop.


