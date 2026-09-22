---
title: Designing an RPG-style game
description: Questions to settle before you build an RPG-style game, and the parts Quest Viva already provides
---

An RPG-style game is one where the player's character is defined by statistics - strength, hit points, magic - and whether an action succeeds depends partly on those numbers and partly on chance. The statistics usually change as the game goes on, and combat is usually a large part of it.

This page helps you decide what your game system needs before you start building it, and lists what Quest Viva already gives you. The rest of this section shows how to build the pieces:

- [Character creation](/howto/player/character-creation) - ask the player for a name and a class at the start of the game.
- [A simple combat system](/howto/rpg/combat) - monsters, weapons and attacks.
- [Spells and magic](/howto/rpg/spells) - spells the player learns and casts, with a mana cost.

## Designing the game system

RPG-style games are big. You need to design a game system, write the scripts for it, and then build a world to use it in. Decide how the game will play before you open the editor - forget about the code for now and think about what the player will experience.

Every rule you add makes the game more complicated to build and to test, so keep the system as simple as you can. At the same time, the player needs real choices. If the sword always does more damage than the dagger, there's no decision to make. If the dagger is better against some enemies, choosing a weapon becomes part of the game.

Work through these questions and write down your answers.

### Stats

What statistics define the player's character? How does each one matter in play - why would a player want a good charisma?

### Mechanics

How is an attack resolved? If this were a tabletop game, which dice would you roll, and what would count as a hit? How is damage worked out?

### Turns or real time?

Does the game go in turns - the player attacks, then the monsters attack? Or do monsters attack every few seconds whatever the player does? Real time is more exciting but harder to build and to balance. If you go with real time, do the player's attacks need a cooldown?

### Defence

What does armour do? Is it one outfit or separate pieces, and is there a drawback to wearing it? In a science-fiction game, a shield might absorb damage until it runs out.

Can the player parry or dodge? Which weapons can parry, and how does the player choose to do it?

### Attack

How often does each weapon hit, and how much damage does it do? What's the difference between a dagger, a polearm and a flail, and when is each one the better choice?

How do ranged attacks work? Can the player shoot into the next room? Do you track ammunition?

### Positioning

Does position matter? In most text adventures the player is simply in a room, but tabletop RPGs often care about flanking, attacking from behind or holding the high ground. If you track position, how does the player move around, and how does the game describe where everyone is? Do you track which way the player and the monsters are facing?

### Injuries

Do you track individual wounds, damage to each part of the body, or just a total? How does the player heal, and what gets healed?

### Equipment

Can weapons, armour and shields break or wear out? Can they be repaired?

### Stealth

Can the player sneak past enemies or attack them unawares? Can monsters do the same to the player?

### Magic and special effects

Is there magic? Can it improve attack, armour or defence? Does casting cost something - magic points, a scroll that's used up - and how does the player get it back? What about magic items, poison and venom?

How will you handle monsters with special abilities, such as reflecting spells, exploding when they die or rusting the player's weapon?

### Companions

Can the player recruit a companion, summon an elemental or raise a zombie to help? When there's more than one target, how does a monster decide whom to attack?

## What's built in

Quest Viva doesn't have a combat system, but many of the pieces you need are already there.

| You need | Use |
|---|---|
| Hit points, with something happening at zero | The **Health** feature. Tick "Health" on the _Features_ tab of the `game` object. The player starts with 100, it's shown in the status pane, and you can set a script to run when it reaches zero. See [Score, health and money](/howto/score/score-health-money#health). |
| Gold or other currency | The **Money** feature, on the same tab. |
| Other statistics shown on screen | [Status attributes](/reference/attributes/status). Any attribute of the player or the `game` object can be shown in the status pane, such as strength, mana or experience points. |
| Dice rolls and random events | [Random functions](/reference/functions/random): `DiceRoll`, `RandomChance`, `GetRandomInt`, and `PickOneString`, `PickOneObject`, `PickOneChild`, `PickOneExit` and friends to pick something at random. |
| Many monsters of the same kind | [Clones](/howto/scripting/clones). Build one monster in the editor, keep it somewhere the player can't reach, and copy it into the game with `CloneObjectAndMove`. |
| Monsters acting after each turn | [Turn scripts](/howto/time/time-turns-and-timers#turn-scripts), which run at the end of each turn. |
| Real-time combat | [Timers](/howto/time/time-turns-and-timers#real-time-timers), which run a script every so many seconds. |

`DiceRoll` takes dice in the usual tabletop notation, so a weapon can store its damage as a string attribute like `"2d6+1"`:

```quest
damage = DiceRoll("2d6+1")
if (RandomChance(10)) {
  msg ("A critical hit!")
  damage = damage * 2
}
msg (PickOneString(Split("The zombie groans.;The zombie lurches towards you.", ";")))
```

`RandomChance(10)` is true 10% of the time, and `PickOneString` picks one string from a list.

Libraries written by the community for Quest 5 also exist for combat and other RPG features. They haven't been tested with Quest Viva, so try one out in a test game before you build on it.
