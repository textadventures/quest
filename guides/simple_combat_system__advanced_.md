---
layout: index
title: Simple Combat System (Advanced)
---

A question that comes up frequently on the forums is how to implement a combat system. The page on [Using Types and Tabs](using_types_and_tabs__advanced_.html) started to create such a system, implementing spells. Now we will build on that. If you want to follow along, start with the [Using Types](using_types.html) page, then the [Using Types and Tabs](using_types_and_tabs__advanced_.html) page, then come back here.

This page has now been updated for Quest 5.4.

Design
------

The first step in designing any complicated system is to consider how the player will experience it. With that in mind, here is a snippet of interaction for a hypothetical combat.

''\>EXAMINE GOBLIN The Goblin is green and ugly.

\>ATTACK GOBLIN You attack the goblin with your bare hands. You hit, doing 2 points of damage. The goblin attacks you with its axe. It hits, doing 7 points of damage.

\>EQUIP GREAT SWORD You now have the great sword of Shalamaa in your hands. The goblin attacks you with its axe. It misses.

\>ATTACK GOBLIN You attack the goblin with the great sword of Shalamaa. You hit, doing 12 points of damage. It is dead. '' From this, I am deciding: I want the system to allow different weapons with different capabilities. I want to tell the player exactly what has happened, how many hits damage are done, etc. (I might change my mind about that later, but during development, this is useful). Foes attack each round, once they have started to attack.

I am going to break this into a number of parts, dealing with weapons, then with attacking foes, then handling those foes attacking the player character.

New Verbs
---------

This involves implementing three new verbs, equip, unequip and attack.

      <verb>
        <property>equip</property>
        <pattern>equip;draw</pattern>
        <defaultexpression>"You can't draw " + object.article + "."</defaultexpression>
      </verb>
      <verb>
        <property>unequip</property>
        <pattern>unequip;sheathe</pattern>
        <defaultexpression>"You can't sheathe " + object.article + "."</defaultexpression>
      </verb>
      <verb>
        <property>attack</property>
        <pattern>attack;fight;strike;kill</pattern>
        <defaultexpression>"You can't attack " + object.article + "."</defaultexpression>
      </verb>

I am using verbs here rather than commands because these are specific to a type of object, and only that type of object.

Player Object
-------------

We need a few attrbutes set on the player, and this is something that has to be done in the game file, rather than the library. You can just copy and paste this over the existing code for the player object if you are starting a new game. Otherwise, you will need to copy across each attribute.

        <object name="player">
          <inherit name="defaultplayer" />
          <attack type="int">2</attack>
          <defence type="int">0</defence>
          <armour type="int">0</armour>
          <hitpoints type="int">25</hitpoints>
          <status>-</status>
          <statusattributes type="stringdictionary">status = ;equippedstatus = !</statusattributes>
          <equippedstatus>Wielding: nothing</equippedstatus>
        </object>

Weapons
-------

For the first two verbs, we need a new type, "weapon". To work with the verbs, the weapon type needs a couple of scripts of the same name as the verbs. I have also chosen to set some attributes first. These are default values, and doing this means I can be sure that any weapon object has values set for these attributes (otherwise, I should really check if the attribute exists before using it, to avoid Quest errors). I have also set it up so all weapons can be taken by default.

      <type name="weapon">
        <attackbonus type="int">0</attackbonus>
        <damagebonus type="int">0</damagebonus>
        <damagedicenumber type="int">1</damagedicenumber>
        <damagedicesides type="int">4</damagedicesides>
        <take />
        <inventoryverbs type="stringlist">
          <value>Look at</value>
          <value>Drop</value>
          <value>Equip</value>
        </inventoryverbs>
        <equip type="script"><![CDATA[
          if (this.parent = game.pov) {
            if (not game.pov.equipped = fists and not game.pov.equipped = null) {
              msg ("You put away your " + game.pov.equipped.alias + " and draw your " + this.alias + ".")
            }
            else {
              msg ("You draw your " + this.alias + ".")
            }
            game.pov.equipped = this
            this.inventoryverbs = Split ("Look at;Drop;Unequip", ";")
            game.pov.equippedstatus = "Wielding: " + this.alias
          }
          else {
            msg ("You don't have it.")
          }
        ]]></equip>
        <unequip type="script"><![CDATA[
          if (this.parent = game.pov) {
            msg ("You put away your " + game.pov.equipped.alias + ".")
            game.pov.equipped = fists
            this.inventoryverbs = Split ("Look at;Drop;Equip", ";")
            game.pov.equippedstatus = "Wielding: nothing"
          }
          else {
            msg ("You don't have it.")
          }
        ]]></unequip>
      </type>

These are pretty straightforward, they just set an attribute called "equipped" on the player object and do a little house-keeping. First, check the player has the weapon, if he does, set or unset *player.equipped*, and print a message that this has happened. Then change the inventory verbs and *player.equippedstatus* (this string was set as a status attribute in the player object, so the player can see what weapon is equipped).

I am assuming there will be a weapon called "fists"; this is what the character uses when he has no weapon equipped. We can put the "fists" object into the library too, just to make sure it does exist. It uses the default values, so is very short.

      <object name="fists">
        <inherit name="weapon" />
      </object>

Finally for weapons as a convenience, we can have the attributes set on a tab. I am modify the existing "Magic" tab for this, relabelling it a "Combat" tab, and adding weapon to the list of types. So the start is modified to look like this:

      <tab>
        <parent>_ObjectEditor</parent>
        <caption>Combat</caption>
        <mustnotinherit>editor_room; defaultplayer</mustnotinherit>
        <control>
          <controltype>dropdowntypes</controltype>
          <caption>Type</caption>
          <types>*=None; nonattackspell=Non-attack spell; lastingspell=Lasting spell; attackspell=Attack spell; monster=Monster; weapon=Weapon</types>
          <width>150</width>
        </control>

And four new controls have been added.

        <control>
          <controltype>number</controltype>
          <caption>Attack bonus</caption>
          <attribute>attackbonus</attribute>
          <width>100</width>
          <mustinherit>weapon</mustinherit>
          <minimum>0</minimum>
        </control>
        <control>
          <controltype>number</controltype>
          <caption>Damage Dice No.</caption>
          <attribute>damagedicenumber</attribute>
          <width>100</width>
          <mustinherit>weapon</mustinherit>
          <minimum>1</minimum>
        </control>
        <control>
          <controltype>number</controltype>
          <caption>Damage Dice Sides</caption>
          <attribute>damagedicesides</attribute>
          <width>100</width>
          <mustinherit>weapon</mustinherit>
          <minimum>4</minimum>
        </control>
        <control>
          <controltype>number</controltype>
          <caption>Damage Bonus</caption>
          <attribute>damagebonus</attribute>
          <width>100</width>
          <mustinherit>weapon</mustinherit>
          <minimum>0</minimum>
        </control>
      </tab>

You should be able to go in-game and equip and unequip weapons now to test this works.

Attacking a monster
-------------------

For attacking the monster, the monster should have a few attributes set (these can also go on the "combat" tab; I will leave it to you to work out how). As before, I have set default values.

Looking ahead a little, your monster is going to need the same attributes as a weapon to determine damage when we come to looking at the monster attacking, and the quick way to sort that out is to have monsters inherit from weapons. Okay, it feels wrong, but it works because they have these properties in common; just make sure monsters cannot be picked up.

We also need an *attack* script that is run when the player attacks the monster.

      <type name="monster">
        <defence type="int">0</defence>
        <armour type="int">0</armour>
        <hitpoints type="int">10</hitpoints>
        <take type="boolean">false</take>
        <attack type="script"><![CDATA[
          if (not HasAttribute (player, "equipped")) {
            player.equipped = fists
          }
          attackroll = GetRandomInt (1, 20) - this.defence + player.attack
          attackroll = attackroll + player.equipped.attackbonus
          if (attackroll > 10) {
            damage = player.equipped.damagebonus
            for (i, 1, player.equipped.damagedicenumber) {
              damage = damage + GetRandomInt (1, player.equipped.damagedicesides) - this.armour
            }
            this.hitpoints = this.hitpoints - damage
            if (this.hitpointsyour  > 0) {
              msg ("You swing " + player.equipped.alias + " and hit, doing " + damage + " points of damage; " + this.hurt)
            }
            else {
              msg ("You swing your " + player.equipped.alias + " and hit, doing " + damage + " points of damage; " + this.death)
              Death (this)
            }
          }
          else {
            msg ("You swing your " + player.equipped.alias + " and miss.")
          }
      ]]></attack>
      </type>

The script first checks if player.equipped is null, and if it is, sets it to fists.

Then it makes the attack roll. I am using a DnD style method to resolve the attack; roll d20, add your attack bonus, add your weapon attack bonus and deduct the foe's defence bonus.

If the attack is successful, calculate the damage. We need to roll a number of dice, so this is done in a loop. I have chosen to subtract the armour inside the loop, so a d12 will do better again armoured foes, while 4d4 will do better against unarmoured foes; this gives a bit more scope to diffentiate weapons (as of 5.3 Quest has a RollDice function built in, but I think this way is easier).

For a successful hit, it will check if the foe is dead, and if it is, call the *makedead* script that spell attacks already use (and this is the power of functions and script; we have already done the work).

Go in-game and check that this is working okay.

Getting attacked by monsters
----------------------------

So the player can attack the monster, only fair that the monster gets to strike back.

The complication here is that the monster or monsters will attack every turn, and we need some way of tracking which should be attacking and which should not. The solution is to maintain an object list that holds all the attacking monsters, and each turn go through that list.

The library already has an object called element\_struct for holding data, so the object list, *attackers*, can be added to that. We need a script added to monsters to add them to that list:

        <settoattack type="script"><![CDATA[
          if (not ListContains (element_struct.attackers, this) {
            list add (element_struct.attackers, this)
          }
        ]]></settoattack>

This script needs to be invoked in the *attack* script, so monsters will start to attack once they are attacked. We also need it in the spell attack function, and you could also do it in the script for a room to have monsters attack immediately. Here is the code to invoke a script on a given object.

      do (this, "settoattack")

The *makedead* script needs updating too, so the monster is removed from the list when dead.

Now we need the turnscript. This simply goes through the list of monsters, and invokes the *attackplayer* script on each. The *foreach* script command is very useful here!

      <turnscript name="attacktheplayerturnscript">
        <enabled />
        <script>
          foreach (attacker, element_struct.attackers) {
            do (attacker, "attackplayer")
          }
        </script>
      </turnscript>

So now we need an *attackplayer* script for the monster:

        <attackplayer type="script"><![CDATA[
          attackroll = GetRandomInt (1, 20) - player.defence + this.attackbonus
          if (attackroll > 10) {
            damage = this.damagebonus
            for (i, 1, this.damagedicenumber) {
              damage = damage + GetRandomInt (1, this.damagedicesides) - player.armour
            }
            player.hitpoints = player.hitpoints - damage
            msg (this.alias + " swings at you and hits, doing " + damage + " points of damage.")
            if (this.hitpointsyour  <= 0) {
              msg ("You are dead!")
              finish
            }
          }
          else {
            msg (this.alias + " swings at you and misses.")
          }
        ]]></attackplayer>

As this is a script we could override it. Say there are archers, we could create a new type that inherits from monster, and give that a new "attackplayer" script where the text is about shotting an arrow.

Summary
-------

So that is the basics of a fully working combat and magic system. However, the monster will still attack the player even if the player has just mistyped something or if he runs to a different room, and the player can still attack a dead monster, so there is room for improvement.

If you want to see this all in action, you can download the library and demo game:

[CombatLibrary1.aslx]({{site.baseurl}}/files//CombatLibrary1.aslx) [CombatDemo1.aslx‎]({{site.baseurl}}/files//CombatDemo1.aslx)
