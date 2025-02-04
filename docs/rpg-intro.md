---
layout: index
title:  Introduction to RPGs
---


Introduction
------------

### What is an RPG?

In a traditional RPG (role-playing game), each player takes the part of a character in a fictional world, where that character's capabilities are defined by a set of statistics, and his or her success at any action is determined in part by those statistics and partly at random. Dungeons and Dragons is the archetypal RPG.


### What is an RPG-style text game?

For the purposes of interactive fiction, an RPG-style game is one is which the player controls a character whose capabilities are defined by a set of statistics, and his or her success at any action is determined in part by those statistics and partly at random. Those statistics can be determined to some degree by the player, and are likely to change during the game. It is assumed that combat will be an important feature of the game.

Here is an example game so you can see what we are talking about. It is based in a fantasy world, but the same ideas apply to science fiction, Wild West, cyberpunk and pulp genres too:
https://textadventures.co.uk/games/view/em15b32xd0o-y-ysvgrtcg/deeper


### So how do you create one?

The very first thing to do is to realise it is a lot of work. RPG games are both big and complicated. You are going to have to design a game system, then code it and then build a whole world.


Designing a game system
-----------------------

Before you start doing anything in Quest, you need to think about how your game will play. Forget about how we will code it, just think about what the player will experience.

The decisions you make here will affect how complicated your game is. I tried to keep combat as simple as possible in Deeper, and it is still a huge amount of code! For example, there are no ranged attacks at all, there is no consideration of position, damage is the total hits rather than by location.

To be fun, your game must offer real choices to the player. If the sword does more damage than the dagger, the sword is the better option, and there is no strategy in choosing the sword. If the dagger is better against some enemies, but not other, the player gets a meaningful choice.

### Stats

What statistics define the player, and how are they meaningful in play (why would a player want to have a good charisma)?

### Mechanics

You need to work out how combat is resolved; if this was a pen and paper game, what dice are rolled and how is success determined? How is damage resolved?

### Time or turn?

Is the game based on turns; the player attacks, then the monsters attack? Or is it time-based, with the monsters attacking every so many seconds. The latter would be more cool, but rather more complicated. Do the player's attacks have a cooldown? Is Quest up to timed combat? The timers on the web player have issue with the lag between the player's computer and the Quest server, so investigate that before going too far.

### Defence

What are the effects of armour or a shield?  Is armour handled as one outfit, or as separate pieces? Is there a drawback to using them? If sci-fi game a shield could be something that absorbs damage up to a certain point before being depleted.

Can the player parry attacks? How does that work? What weapons can parry? How does the player choose.

### Attack

Two important attributes of weapons are how often you hit with them and the damage they do. What is the difference between attacking with a dagger or a polearm or a flail? Are there situations where one weapon is better, and other situations where it is not?

How do you handle ranged attacks? Can you shoot an arrow from an adjacent room perhaps? Does an attacker get a bonus if you are holding a bow? Will you track ammo?

### Positioning

Is position significant or even tracked? In most adventure games the player is in a room, and not positioned any more than that, but traditional RPGs often take account of flanking, back-attack, higher ground. If you do track positioning, how does the player control where her character is? How does the game report it to her?

Are you go to track the direction the player (and monsters) are facing?

### Injuries

Do you track individual injuries (say a laceration to the arm), or by location (8 hits to the left arm) or just total hits?

How can the player heal, and what gets healed?

### Equipment

Can weapons, armour and shields break? 

### Stealth

Can the player sneak? If so, what is the benefit? If the player can sneak, can the monsters?

### Special stuff

Is there magic/poisons/etc.? How can magic affect combat; bonus to attack, to armour, to defence, etc.? Does casting magic use power points (how are they got back), use up the spell or what? What about magic items?

What about poison and venom?

How do you handle special effects on the monsters, such as reflecting magic, exploding on death, rusting the player's weapon?

### Companions

Will the player be able to recruit a companion (or summon an elemental or raise a zombie, etc.) to aid him? How will you resolve whether a specific enemy targets the player or the companion?



Creating
--------

### Using a library

There is a comprehensive library here, complete with detailed tutorial on how to use it:

https://github.com/ThePix/quest/wiki/CombatLib

### Coding a game system from scratch

If you cannot use the desktop version, or just want to re-invent the wheel, we have a series of guides available:

[The Zombie Apocalypse Part 1](zombie-apocalypse-1.html)

[The Zombie Apocalypse Part 2](zombie-apocalypse-2.html)

[Spells for the Zombie Apocalypse](zombie-apocalypse-spells.html)
