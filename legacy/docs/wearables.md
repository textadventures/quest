---
layout: index
title: Wearable items
---

A common feature of text adventures is items that can be worn.


Basics Clothing
---------------

Adding clothing is very simple. Let us suppose we want to add some trousers. Create an object called "trousers", and on the _Features_ tab, tick "Wearable: object can be put on and taken off". You will see there is a new tab, "Wearables". On that tab, set the garment to "Can be worn" and a bunch of new boxes will appear.

That is it. At its simplest, that is all you need to do. You can now go in game, and wear the trousers.

You should see this:

>   &gt; wear trousers

>   You put it on.

Wait, it should say "them" not "it". This is not specifically about clothing, but we might as well get it right. Go to the "Setup" tab, and set the type to "Inanimate objects (plural)". Also untick "Use default prefix and suffix" and it will not get referred to as "a trousers".

You can add specific messages for putting on and taking off items too. For the trousers, you could put "You pull on the trousers, one leg at a time." in the "Message to print when wearing" box. Now you will see:

> &gt; wear trousers

> You pull on the trousers, one leg at a time.
 
That is the basics, however, the library allows you to do rather more. What about underwear? And what if you want to ensure underwear cannot be worn over the top of the trousers?




Layers and Slots
----------------

Garments can be assigned to layers and slots. Slots are where the item is worn. Each slot is a string, for example "head". At this point, we need to make a design decision - how are we going to divide the body up? For simplicity, we will say: feet, lower, upper and head. Trousers cover the lower body, so in the wear slots section, add "lower"

As well as slots, clothing has layers. Trousers went in the default layer, 2. Create some underpants and give them a wear layer of 1, and again a wear slot "lower". Now if you go in game you will find you cannot put the underpants on if you are already wearing trousers. The verbs offered in the pane on the right will not include "Wear", and if you type it in the command bar you will see this:

> &gt; wear trousers

> You put them on.

> &gt; wear underpants

> You cannot wear that over trousers.

The logic here is that, for garments that have the same slot, you can only put on an item that has a higher layer than the items already worn, and you can only take off the garment with the highest layer.

Garments can occupy more than one slot, so overalls could be set to have both "lower" and "upper". And any garment with no slots can be worn without restrictions.


### Layer Zero

Layer zero is special; it is effectively all layers. Say you have a pair of shorts that cannot be worn under trousers or over underpants, you can set its layer to 0.


Advanced features
-----------------

Most people will not need to worry about the advanced features, so they are hidden to keep the tab uncluttered. To turn them on, go to the _Features_ tab of the game object, and tick "Show advanced options for wearables".



### Removeable

Untick this if you do not want the player to be able to remove the garment. This might be because the item is cursed, or you just want to prevent the player getting completely naked, and having to handle the reactions of the NPCs to a naked person.

If you go to the _Attributes_ tab, you can create a string attribute called "notremoveablemessage" that will be displayed when the player tries to take the garment off.


### Protection

The library gives some facilities to handle armour for a combat-orientated RPG-style game, which is discussed more below. The `armour` attribute of this item can be set in the "Protection" box.


### Bonuses and Penalties

This feature is for garments that give bonuses when worn. Bonuses (and penalties) are set in the "Wearing gives a bonus to these attributes" box. In there you can list any attributes on the player character that get increased when the garment is worn. If there is more than one, separate them with semi-colons (and no spaces). By default, the increase is 1, but you can specify other values too, so you can use a minus to give a penalty.

For example:

```
  protection;charisma+2;agility-1
```

When the player puts on this garment, his protection will increase by 1, his charisma by 2 and his agility will drop by 1. When the garment is removed, the bonuses are all lost.

The bonuses are applied by the SetBonuses function, which takes the garment as its first parameter, and a Boolean as the second. The Boolean should be true if the garment is being put on, and false if taken off.

The WEAR and REMOVE commands, together with the WearGarments function will call SetBonuses automatically. If you have garments getting put on or taken off any other way, you must remember to call SetBonuses for each garment, if you use this feature.

You can override the GarmentBonusMultiplier function to have the effects doubled or tripled in certain situations. By default it returns 1, but if the player should get double the effect, have it return 2.

NPCs are not affected by garment bonuses.


### Display Verbs

The system automatically updates the display verbs for all clothing as garments are put on and taken off. Occasionally, you may want to add your own. For verbs that should be seen when the item is in the room, rather than the player's inventory, do this on the _Object_ tab as normal.

For verbs that will be visible when the player has the item, set these on the _Wearables_ tab. There are two boxes, one for when the garment is just being carried and one for when it is worn (but only if it is the outmost garment). You can put as many verbs as you like, separated with semi-colons.

You can change the additional display verbs mid-game by modifying the `wornverbs` or `invverbs` attributes, then called `SetVerbs`. Here is an example where two verbs are added to a hat for when it is worn, and one when it is not.

```
  pink_hat.wornverbs = "Activate;Show off"
  pink_hat.invverbs = "Activate"
  SetVerbs
```

You might also want to call `SetVerbs` in the start script of the game object so any garments in the player's inventory at the start are set up correctly.



### Scripts

You can also set scripts to trigger when an item is put on or when it is taken off.


### Multistate


Tick the _Multistate?_ box to show options related to garments that can be in more than one state (such as a jacket that can be open or fastened), as described on [this page](multistate-clothing.html).





A note on inventory limits
--------------------------

This only applies if you are using the inventory limits feature.

Wearing something will automatically increase the player's inventory limit by 1, and removing it reduce it by 1. What this effectively means is that items that are worn will not contribute to the inventory limit.

In Quest, the volume limit works more like a weight limit, and the player is still carrying the weight, so putting something on or taking it off does not affect the volume limit.





More on wearables
-----------------

There are various functions that can help you when handling clothing.

### What is the player wearing?

Use the `ListClothes` function to get a string that lists the clothes currently worn (note, not a string list). If the player is naked, it will return the string "nothing". It could be used like this:

```
  msg("You are wearing " + ListClothes () + ".")
```

### What is covered?

Do you need to know if the player is wearing anything at a certain location? For example, perhaps the floor is hot and you want to know if the player is barefoot.

You can get the outer most garment for a wear slot, using the GetOuter function.

```
  GetOuter ("feet")
```

If the player is wearing socks and boots, the function will return the boots object, as they are worn on the outside. If the player has nothing on his feet, it will return null.



### Putting on and removing garments in code (starting clothing)

There are two functions, `WearGarment` and `RemoveGarment`, that you can use if you need to put garments on or take garment off the player in code (as opposed to the player doing it via the normal commands).

For example, to have the player wearing clothes at the start, use the WearGarment function. This will ensure the item is in the player's inventory, and all its attributes properly set. You can do this in the start script of the game object.

```
  WearGarment (underpants)
  WearGarment (trousers)
  WearGarment (shirt)
```

The `RemoveGarment` function works similar;y, taking the garment to be removed as a parameter. To remove all garments (without any message to the player), do this:

```
  foreach (o, GetAllChildObjects(game.pov)) {
    if (GetBoolean(o, "worn")) {
      RemoveGarment (o)
    }
  }
```

### Changing the name of clothing

Quest handles changing the name of a garment, so when it is worn, its alias has "(worn)" added to it. However, that means that if the name of a garment changes, just setting the alias is going to confuse Quest. There are, therefore, two functions to do this. The `SetAlias` function takes the name of the object and the new alias, whilst `SetListAlias` takes the name of the object, the new alias and the new list alias. For example:

```
SetAlias (trendy_jacket, "unfashionable jacket", "Unfashionable jacket")
```

You can use this with any object, by the way; they will just change the alias and list alias.


### Appropriate clothing?

_Note: You can only do this in the desktop version._

You can [override](overriding.html) a function called `TestGarment` if you want to check a garment can be worn, for example to ensure it is not too small for the player. `TestGarment` must return a boolean, and take a single parameter; the garment. It should return true if the garment can be worn. If it cannot, it should give a message to say that, and then return false.

```
  if (GetBoolean(object, "toosmall")) {
    msg("That is too small for you!")
    return (false)
  }
  return (true)
```

`TestGarment` is called after the system has already determined that the object is wearable, is held and is not currently worn, so you can safely assume these are true.

There is a corresponding function `TestRemove` that is called before an item is removed. You can see how this might be used in the "No Public Nudity" section later.


### Checking

There is no built-in system to ensure that you only add the right wear_slots. If you have some jeans in a slot called "Lower", the player will be able to wear them at the same time as the trousers in slot "lower". To check you have not done that by mistake, add this line to the start script of your game object:

```
  msg (Slots())
```

When you start the game, you should see something like this:

List: lower; feet; upper; head;

If a slot appears twice, you have a bug and can sort if out before you release your game. In the example just mentioned, you might see this:

List: lower; feet; upper; Lower; head;

Remember to delete that line from the start script before you upload your game; it is just for testing.


### A "Worn Clothing" Location

If you feel the player's inventory is getting cluttered with what the player is wearing, an option is to create a container, perhaps called "Worn Clothing", on the player object, and to have anything worn go in there. If the container is open, the clothing will get listed, but if it is closed, they will be hidden.

To get this to work successfully, you need to create a new object on the player object, and give it a Boolean attribute called `wornclothinglocation` that is set to true. On the _Features_ tab, set it to be a container, then on the _Container_ tab, set it to be a container. On the _Inventory_ tab, untick the box so it cannot be dropped.

At this point it should work fine, but there is some tidying up we can do to make it more slick (however, you should only do this if you do not have SHOW or HIDE as commands; if you do this will screw them up). On the Inventory tab, tick "Disable automatically generated display verb list", and in the list of _Inventory Verbs_, delete everything. Click _Add_ and type in "Hide".

Now go to the verbs tab, and click _Add_ there. Again, type in "Hide" and set this to run a script, and paste this in:

```
  this.isopen = false
  this.inventoryverbs = Split("Show", ";")
```

Click _Add_ there again, and this time type in "Show" and set this to run a script, and paste this in:

```
  this.isopen = true
  this.inventoryverbs = Split("Hide", ";")
```

If you are using inventory limits, increase the maximum by one to allow for this new object.


### No Public Nudity

_Note: You can only do this in the desktop version._

Let us suppose you want to ensure the player is modestly attired in public places. How might you do that? The first step is to decide what that means in game terms, and then to create a function, let us say, `IsDecent` that will test that, and return a Boolean as appropriate. How that works will depend on your game, but let us suppose there is a flag on the player "isfemale" that is true for female characters, and the important body slots are "lower" and "upper". We will also set an attribute, "private" on rooms that are private. The code might look like this:

```
if (player.isfemale and GetOuter("upper") = null) {
  // Female player not decent if topless
  return (false)
}
if (GetOuter("lower") = null) {
  // Player not decent if nothing below the waist
  return (false)
}
// Player decent
return (true)
```

For exits from private locations to public locations, you need to set the exit to run a script, and have that check `IsDecent`:

```
if (IsDecent()) {
  player.parent = this.to
}
else {
  msg ("You can't go out there looking like that!")
}
```

Finally, you need to [override](overriding.html) the `TestGarment` function.

```
if (GetBoolean(player.parent, "private")) {
  // Not a public area, so player can remove what he or she likes
  return (true)
}
// Hypothetically, what would it be like without the item?
object.worn = false
if (IsDecent()) {
  // Player would be decent, reset the flag and return true
  object.worn = true
  return (true)
}
else {
  // Player would not be decent, give message and return false
  msg ("You can't take that off!")
  return (false)
}
```


### Support for NPCs

The library offers some support for having NPCs wearing clothing. Clothing worn by the player can be worn by NPCs too.
```
  // Get an object list containing garments worn by the NPC
  ListWornFor (char)

  // Get an object list containing garments worn by the NPC
  // and visible. Garments without wear_slots assigned
  // will not be listed
  ListVisibleFor (char)

  // Gets the outermost garment worn by the NPC in the given
  // slot, or null if there is none. 
  GetOuterFor (char, slot) 

  // As GetArmour, but for the specified NPC
  GetArmourFor (char)
```

Note that `ListVisibleFor` has some limitations, as it can only guess at what is visible. If a girl is wearing tights under shorts and shoes, and you are using feet, lower, upper and head as locations, the tights will not be considered visible. Using more body locations will solve this issue; however you need to think about this as early as possible! See-through garments are not supported.



### Armour

Quest gives some facilities to handle armour. If you want to use the default armour system, then the body locations you use must be:

> feet, legs, shoulders, arms, hands, head, torso

You can set the protection for each garment the player is wearing. The function `GetArmour` will calculate the total protection worn. This is calculated by checking the protection for each location (if more than one item is worn in a location, the highest is used plus half the rest). Values for each location are added together, with head counting double and torso counting three times. If the protection value of each item can be from 0 to 5, the result from `GetArmour` can range from 0 to 100.

What you do with that is up to you!

The `UpdateArmour` function is called when the player puts on or removes a garment. By default it does nothing, but you can override it to update a status display or to set an attribute on the player with the current armour.




