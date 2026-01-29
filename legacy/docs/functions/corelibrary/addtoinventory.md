---
layout: index
title: AddToInventory
---

    AddToInventory (object)

Moves the object to the inventory. This simply sets the object's parent to the current player, so:

     AddToInventory(myobject)

is equivalent to

     myobject.parent = game.pov

Added in Quest 5.2.
