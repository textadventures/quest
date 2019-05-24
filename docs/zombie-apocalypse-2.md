---
layout: index
title: Zombie Apocalypse (part 2)
---


This is the second of two parts, and discusses how to add some more advanced features. Part one can be found [here](https://github.com/ThePix/quest/wiki/The-Zombie-Apocalypse-(on-the-web-version)).

Status attributes
-----------------

It would be good if the player can see at a glance how she is doing. We can use status attributes to do that. The first step (for the web version), is to give the player a string dictionary, called "statusattributes". Then we can add the attributes we want to track.

We should display both the weapon and the current ammo, if the weapon is a firearm, and these need to be done as strings, so first we will create a function that sets `player.equippedname` to the name of the current weapon, and `player.ammonote`, which will give the current and maximum ammo for a firearm.

Call the function `WeaponUpdate`, no parameters or return type, and paste in the code:

```
if (player.equipped = null) {
  player.equippedname = "-"
  player.ammonote = "-"
}
else {
  player.equippedname = Replace (player.equipped.listalias, " (equipped)", "")
  if (not HasAttribute(player.equipped, "ammo")) {
    player.ammonote = "-"
  }
  else {
    player.ammonote = player.equipped.ammo + "/" + player.equipped.ammomax
  }
}
```

Now go to the game start script. We want to call that function, then add the dictionary to the player, and then add each attribute that we are interested in.

```
// Status attributes
WeaponUpdate
player.statusattributes = NewStringDictionary()
dictionary add (player.statusattributes, "hitpoints", "Hit points: !")
dictionary add (player.statusattributes, "ammo", "Spare ammo: !")
dictionary add (player.statusattributes, "equippedname", "Weapon: !")
dictionary add (player.statusattributes, "ammonote", "Ammo: !")
```

The exclamation mark indicates where the number will go, by the way. Finally, we need to call `WeaponUpdate` anywhere the weapon can change or the ammo can change:

- equip command
- unequip command
- reload command
- shootfunction

In each case, we need to insert the function as the penultimate line (the last line being just a curly brace). For example, `equip` will end like this:

```
  object.listalias = object.listalias + " (equipped)"
  WeaponUpdate
}
```

Attack descriptions
-------------------

You may have noticed that if you shoot the zombie, you see:

> You attacks decomposing zombie and misses...

And when it attacks you:

> Decomposing zombie attacks you and misses...

Not too slick. But we can fix that, by adding a new attribute to the weapons and enemies. We will add three strings called "critdesc", "attackdesc" and "missdesc", with special codes in them.

Go to the _initialisation script_ tab of the spade, and update the script:

```
this.damage = "1d6"
this.attack = 3
this.critdesc = "You smash the spade down on #target# (#hits# hits)."
this.attackdesc = "You swing the spade at #target# (#hits# hits)."
this.missdesc = "You swing wildly and entirely miss #target#."
```

And for the pistol:

```
this.damage = "2"
this.attack = 0
this.firearmdamage = "2d8"
this.firearmattack = 3
this.ammo = 3
this.ammomax = 6
this.critdesc = "A well placed shot with the pistol on #target# (#hits# hits)."
this.attackdesc = "You shot the pistol at #target# (#hits# hits)."
this.missdesc = "You shoot wildly and entirely miss #target#."
```

You also need to add them to the zombies (and any other monsters). Just tag these lines on to the end of SpawnZombie:

```
obj.critdesc = "A well-placed blow by #attacker# sends you reeling (#hits# hits)."
obj.attackdesc = "#Attacker# has hit you (#hits# hits)."
obj.missdesc = "#Attacker# misses you."
```

We will create a new function to print them. Call it "AttackReport", with no return type, with four parameters: s, attacker, target, hits. Paste in the code:

```
s = Replace(s, "#Attacker#", CapFirst(GetDisplayAlias(attacker)))
s = Replace(s, "#attacker#", GetDisplayAlias(attacker))
s = Replace(s, "#Target#", CapFirst(GetDisplayAlias(target)))
s = Replace(s, "#target#", GetDisplayAlias(target))
s = Replace(s, "#hits#", "" + hits)
msg (s)
```

All the code does is substitute the real values with _#attacker#_ etc. Note that it preserves capitalisations. The last dozen lines of `DoAttack` need to be modified to use the new function.

```
if (roll > 15) {
  damage = damage * 3
  AttackReport (weapon.critdesc, attacker, target, damage)
  target.hitpoints = target.hitpoints - damage
}
else if (roll > 10) {
  AttackReport (weapon.attackdesc, attacker, target, damage)
  target.hitpoints = target.hitpoints - damage
}
else {
  AttackReport (weapon.missdesc, attacker, target, 0)
}
```


Multiple attacks
----------------

So the player has a choice of different weapons, perhaps we could give the zombies some options. Half the work has been done because it already works for the player's weapons. The first bit to change is the turn script. This is what we already have in the top half of the turn script:

<pre>
if (not GetBoolean(game, "notarealturn")) {
  list = NewObjectList()
  foreach (obj, GetDirectChildren(player.parent)) {
    if (HasBoolean(obj, "dead")) {
      if (not obj.dead) {
        <em>DoAttack (obj, obj, player)</em>
        list add (list, obj)
      }
    }
  }
</pre>

We need to take out the line in italics, and replace it with nine new lines. It should then look like this (with the new lines in italics):

<pre>
if (not GetBoolean(game, "notarealturn")) {
  list = NewObjectList()
  foreach (obj, GetDirectChildren(player.parent)) {
    if (HasBoolean(obj, "dead")) {
      if (not obj.dead) {
        <em>if (HasScript(obj, "selectweapon")) {</em>
          <em>do (obj, "selectweapon")</em>
        <em>}</em>
        <em>if (HasObject(obj, "weapon")) {</em>
          <em>DoAttack (obj, obj.weapon, player, false)</em>
        <em>}</em>
        <em>else {</em>
          <em>DoAttack (obj, obj, player, false)</em>
        <em>}</em>
        list add (list, obj)
      }
    }
  }
</pre>

So now if the monster has a script called "selectweapon" that will get run, and can be used to change weapons. Then, if the monster has a weapon, that is used for the attack.

### Monster attacks...

We will set up attacks for monsters just like other weapons, with the difference that they will go into a room the player cannot access. You might want to call it "monsterattacks". It is worth pointing out that these are not normal weapons, but really just a way to store data - the monsters will be linked to the weapons, rather than carrying them. This means several monsters can share the same attack.

Select the spade, click "Copy", the go to the "monsterattacks" room, and click "Paste". Change the name, then set the parameters up in the initialisation script as before. Here is an example for "vomitattack":

```
this.damage = "2d6"
this.attack = 0
this.critdesc = "#Attacker# spews noxious vomit all over you (#hits# hits)."
this.attackdesc = "#Attacker# vomits over you (#hits# hits)."
this.missdesc = "You narrowly avoid the foul vomit from #attacker#."
```

Note that even ranged attacks should be set up like this as monsters do not have to worry about ammo.

### Selecting an attack

Finally you need to add a script to the zombie to select an attack. Here is a simple example, this needs to go on the end of the `SpawnZombie` function.

```
obj.selectweapon => {
  if (RandomChance(50)) {
    this.weapon = vomitattack
  }
  else {
    this.weapon = null
  }
}
```

What this will do is 50% of the time the zombie will use the vomit attack, the rest of the time, it will use its basic attack.

Let us suppose you have other attacks set up: talonattack and kickattack. Here are some other options for the script. The first picks an attack at random each turn:

```
obj.selectweapon => {
  this.weapon = GetObject(PickOneString("vomitattack;talonattack;kickattack"))
}
```

This will select one at random, but then will stick with it for a while:

```
obj.selectweapon => {
  if (RandomChance(20) or this.weapon = null) {
    this.weapon = GetObject(PickOneString("vomitattack;talonattack;kickattack"))
  }
}
```

This will select each in turn (it starts at a random number so the zombies are not all doing the same each turn):

```
obj.weaponcount = GetRandomInt(0, 100)
obj.selectweapon => {
  names = Split("vomitattack;talonattack;kickattack", ";")
  name = StringListItem(names, this.weaponcount % ListCount(names))
  this.weapon = GetObject(name)
  this.weaponcount = this.weaponcount + 1
}
```

Perhaps the zombie has to ready its vomit attack. If we have a `vomitreadying` attack object, we could set it up like this in its initialisation script:

```
this.damage = "0"
this.attack = -100
this.critdesc = ""
this.attackdesc = ""
this.missdesc = "#Attacker#'s abdomen is gurgling alarmingly."
```

Then we can give the zombie this script:

```
obj.selectweapon => {
  if (this.weapon = vomitreadying) {
    this.weapon = vomitattack
  }
  else {
    this.weapon = GetObject(PickOneString("vomitreadying;talonattack;kickattack;talonattack;kickattack"))
  }
}
```

If the zombie was readying the vomit attack last turn, then this turn it will perform it (and perhaps the player has had a chance to protect herself). Otherwise an attack is chosen at random. Note that the talon and kick are there twice so are twice as likely to be chosen.




Searching Corpses
-----------------

Looting the dead is very much frowned upon in real life, but accepted and almost mandatory in video games, so we better implement it for our game. What we need is a search command and some addition to the zombies that adds goodies to them.

To turn on money, tick the "Money" box on the _Features_ tab of the game object, then go to the _Player_ tab of the player object and set the start money. Make sure you set a value here - it looks like it defaults to zero, but really it does not. If you want it to be zero, set it to 1, and then set it to 0. For more on how to handle money in Quest, see [here](http://docs.textadventures.co.uk/quest/shop.html).

Create a command, and give it this pattern:

> search #object#

We will keep the command very general, as you might want to have the player search all sorts of things in your game, not just monsters. All it will do is check if the object has a script called "searchscript", that it has not already been searched, and that it is not still moving. If all okay, run the script:

```
if (not HasScript(object, "searchscript")) {
  msg ("There's nothing of interest there.")
}
else if (GetBoolean(object, "searched")) {
  msg ("You find nothing more.")
}
else if (not GetBoolean(object, "dead") and HasBoolean(object, "dead")) {
  msg ("That's moving too much for you to search!")
}
else {
  do (object, "searchscript")
  object.searched = true
}
```

Now we need some things the player could find. Create a room called "treasureroom", and put some things in it, things the player might find when searching a corpse (you can be as creative as you like, and the more variety the better).

Now go to the `SpawnZombie` function, and add this to the end, to give your zombies a searchscript.

```
obj.searchscript => {
  money = GetRandomInt(1, 50)
  msg ("You find " + DisplayMoney(money) + ".")
  player.money = player.money + money
  if (randomChance(10)) {
    o = PickOneChild(treasureroom)
    msg("You also find " + GetDisplayName(o) + ".")
    CloneObjectAndMoveHere (o)
  }
}
```

So what does it do? First it picks a number from 1 to 50; this is how much money is found on the corpse. It adds that to the player's cash, and gives a message. Then there is a 10% chance of something else, something picked at random from the treasure room, which is then cloned, and put in the location.

Obviously you can modify the numbers as you like.


On death
--------

It would be nice if the display verbs changed when the zombie dies, so "search" is an option but "shoot" is not. Just update the script that runs when it dies.

```
obj.changedhitpoints => {
  if (this.hitpoints < 1) {
    msg ("It is dead!")
    this.dead = true
    this.displayverbs = Split("Look at;Search", ";")
  }
}
```

As an aside, what if the player comes across a wraith? The thing about wraiths is that when they die, there is no corpse, and they cannot be searched. How do we handle that? Change the death script like this:

```
obj.changedhitpoints => {
  if (this.hitpoints < 1) {
    msg ("It dissolves into mist. You have destroyed it!")
    this.parent = null
  }
}
```


Searching a junk pile
---------------------

If you want the player to be able to search something else, here is how to do that. We will create an object called "junk pile", and something to be found in it, let us say "diary". Put the diary in the same room as the junk pile, but on the _Setup_ tab untick the visible tab.

Turn on the initialisation script on the junk pile, and go to the tab, and paste in this code:

```
this.searchscript => {
  msg ("You find a battered book; it looks like a diary.")
  diary.visible = true
}
```