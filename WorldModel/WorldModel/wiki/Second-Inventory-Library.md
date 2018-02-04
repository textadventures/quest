Let us suppose you want to give the player a spell book, and this will appear on the right hand side as a second inventory panel, listing the spells known. How would you go about that?

The library code was originally written by Jay Nabonne on a 
[forum post](http://textadventures.co.uk/forum/samples/topic/3789/adding-a-second-inventory-object-pane). I have updated it slightly and this tutorial is for that updated file. 

[Library](https://github.com/ThePix/quest/blob/master/InvPane2.aslx)

## Basic Set-up

First [add the library](https://github.com/ThePix/quest/wiki/Using-Libraries) to your game.

We now need somewhere to store the spells.

If you are using CombatLib, then that already exists, a room called "spells_known".

If you are _not_ using CombatLib, you will need to create a new room; to keep this tutorial simple, we will call it "spells_known", so we are all using the same procedure.

Now go to the _Features_ tab of the game object, and tick the "Show advanced scripts for the game object" box. Go to the _Advanced scripts_ and add this code to the "User interface initialisation" script:
```
  InitInv2 ("Spells")
  SetInv2(spells_known)
```
The first line sets up the inventory, giving it the name "Spells". The second line takes the objects from the room, "spells_known", and puts them into the list.

By the way, we add the code here because we need this to happen for both a new game and when loading a saved game; the `game.start` script only runs when a new game starts.

Go in game, and you should see it there. If you put an object in the "spells_known" room, you will see it in the list, but so far we cannot add anything during play (for example if you learn a spell).

## Updating

Unlike the normal inventory, the second inventory pane does not update automatically. We need to call `UpdateInv2` to do that, and obviously that needs to happen whenever it might change. For a spell book, that is whenever a spell is learnt.

If you are using CombatLib, you can [override](http://docs.textadventures.co.uk/quest/overriding.html) the `UpdateSpells` functions to do this. The code is then just:
```
UpdateInv2
```
Otherwise, you could add that to the end of the LEARN command or whatever is best for your game.


## "I can't see that."

If you are not using CombatLib, there is an issue here that if the player tries to use an object in the second inventory, she will get this message. Quest tries to match object names against objects in the current room or the player's real inventory. There are two ways around that.

**General:** If you want the player to be able to use general commands, such as LOOK AT and DROP, you can add the contents of the second inventory to the scope. Go to the  _Advanced scripts_ tab of the game object, and add this code to the "Backdrop scope script":

```
foreach (o, GetDirectChildren(spells_known)) {
  list add (items, o)
}
```

This will add everything in the second inventory to the list of items in scope, so the player can use them as normal.

** Specific:** If the items just need to respond to specific a command (such as CAST and ABOUT, as CombatLib does it), you can set the commands to look here for items. Go to the command, and in the Scope box, type in "spells_known" (without quotes). Now the command will look there first for an object to match.



## About `SetInv2`

The `SetInv2` function will take a single object or a list of objects. Above we gave it a single object, and the inventory pane was populated with all the objects inside that object. The system remembered what that object was, and so we could later use `UpdateInv2`, and the second inventory pane was then re-populated with whatever was now in that object.

If you send an object list to `SetInv2` then that list is what appears in the pane. You will have to send a new list to `SetInv2` every time the pane needs to be updated. However this technique may be required if you want to filter the list, perhaps pulling only spells from a room.