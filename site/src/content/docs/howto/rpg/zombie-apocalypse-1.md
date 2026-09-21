---
title: A simple combat system
description: Spawn monsters from a prototype, fight them with melee weapons and firearms, let them fight back, and loot the bodies
---

This page builds a small combat system for a zombie game: zombies that appear in a room, weapons the player can equip, a pistol that runs out of ammo and needs reloading, zombies that attack every turn, and bodies to search. You can use the same pieces for any monsters.

It's built from a few parts, each of which is described below:

| Part | What it is |
|---|---|
| The player's health | Quest Viva's built-in Health feature |
| Weapons | Objects with attributes for how well they hit and how much damage they do |
| `DoAttack` | One function that every attack goes through - the player's and the zombies' |
| Monsters | One zombie built in the editor and kept off-stage, then cloned into rooms |
| ATTACK, SHOOT, EQUIP, UNEQUIP, RELOAD | Commands |
| Zombie attacks | A turn script |

The scripts are easiest to paste into the script editor's code view. For more about any of these, see [Commands](/tutorial/custom-commands), [Functions](/howto/scripting/creating-functions-which-return-a-value) and [Turn scripts](/howto/scripting/using-turnscripts).

To see it all working first, download the finished game, [simple-combat.aslx](/examples/simple-combat.aslx), and open it in the editor. It has a yard with a spade and a pistol, and a street where two zombies are waiting.

## The player's health

Select the `game` object, go to the _Features_ tab and tick "Health". The player now has a `health` attribute that starts at 100 and appears in the status pane as a percentage - see [Score, health and money](/howto/world/score-health-money#health).

Then, on the `game` object's _Player_ tab, fill in "Script to run when health reaches zero":

```quest
msg ("The zombies drag you to the ground. You have died.")
finish
```

## Weapons

Every weapon has four attributes, which you add on its _Attributes_ tab:

| Attribute | Type | Meaning |
|---|---|---|
| `attack` | Integer | Added to the roll to hit - higher is more accurate |
| `damage` | String | Damage done on a hit, in dice notation such as `2d4` (two four-sided dice), or a fixed number such as `2` |
| `hitmsg` | String | Printed on a hit. `#target#` is replaced by the target's name |
| `missmsg` | String | Printed on a miss |

Add a spade, tick "Take" on its _Inventory_ tab, and give it these attributes:

| Attribute | Value |
|---|---|
| `attack` | 2 |
| `damage` | `2d4` |
| `hitmsg` | `You whack #target# with your spade.` |
| `missmsg` | `You swing your spade at #target# and miss.` |

When the player isn't holding a weapon, they fight with their fists, and the player object itself is used as the weapon. So give the player object the same four attributes: `attack` 0, `damage` `1d3`, `hitmsg` `You punch #target#.` and `missmsg` `You swing a fist at #target# and miss.`

## The attack function

Every attack in the game - the player's and the zombies' - goes through one function, so the rules for hitting live in one place. Select "Advanced" in the tree and click "Add Function". Call it `DoAttack`, with four parameters: `attacker`, `weapon`, `target` and `firearm`. Leave "Return type" as it is, and give it this script:

```quest
if (firearm) {
  prefix = "firearm"
  weapon.ammo = weapon.ammo - 1
}
else {
  prefix = ""
}
roll = DiceRoll("1d20") + GetInt(weapon, prefix + "attack") - GetInt(target, "defence")
if (roll > 10) {
  damage = DiceRoll(GetAttribute(weapon, prefix + "damage"))
  s = GetString(weapon, prefix + "hitmsg") + " (" + damage + " damage)"
}
else {
  damage = 0
  s = GetString(weapon, prefix + "missmsg")
}
s = Replace(s, "#target#", GetDefiniteName(target))
s = Replace(s, "#Attacker#", CapFirst(GetDefiniteName(attacker)))
msg (s)
if (damage > 0) {
  target.health = target.health - damage
}
```

The attack hits if a twenty-sided dice roll, plus the weapon's `attack`, minus the target's `defence`, comes to more than 10. `DiceRoll` takes either dice notation like `"2d4"` or a plain number, so a weapon's `damage` can be either. `GetInt` gives 0 for an attribute the object doesn't have, so the player doesn't need a `defence` unless you want one.

A firearm has a second set of attributes with `firearm` in front - `firearmattack`, `firearmdamage` and so on - so the same pistol can be fired or used as a club. When `firearm` is `true`, the function reads those attributes instead, and uses up a round of ammo.

Both the player and the monsters use `health`. For the player, the Health feature takes care of what happens when it reaches zero. For monsters, you'll add a script to their `health` below.

## Monsters

Build one zombie in the editor, keep it in a room the player can never reach, and put copies of it - clones - into rooms as the game needs them. Each clone gets everything the original has: attributes, verbs and scripts. See [Clones](/howto/scripting/clones) for more.

Add a room called `offstage`, with no exits leading to it, and add an object called `zombie` inside it. On its _Attributes_ tab, add:

| Attribute | Type | Value |
|---|---|---|
| `health` | Integer | 12 |
| `defence` | Integer | 0 |
| `attacks` | String List | `zombie_bite`, `zombie_claw`, `zombie_vomit` |
| `adjectives` | String List | `shambling`, `rotting`, `limping`, `groaning` |

`attacks` lists the zombie's attacks, which you'll make in [Monsters fighting back](#monsters-fighting-back). `adjectives` gives each zombie a different name, so the player can tell them apart.

### When a monster dies

Still on the _Attributes_ tab, select `health` and click "Add Change Script". A change script runs every time the attribute changes, so this one runs whenever the zombie is hurt:

```quest
if (this.health <= 0 and not GetBoolean(this, "dead")) {
  msg (CapFirst(GetDefiniteName(this)) + " falls to the ground and lies still.")
  this.dead = true
  this.alias = "dead " + this.alias
}
```

Use `this` rather than `zombie` in scripts on the zombie, so that each clone changes itself and not the original.

### Putting zombies in a room

Add a function called `SpawnZombie`, with one parameter, `room`:

```quest
z = CloneObjectAndMove(zombie, room)
adjective = PickOneString(zombie.adjectives)
list remove (zombie.adjectives, adjective)
z.alias = adjective + " zombie"
```

`CloneObjectAndMove` clones the zombie and puts the clone in the room. The function then names it with an adjective picked at random from the list, removing that adjective so no two zombies share a name. Give the list more adjectives than the number of zombies in your game.

To put two zombies in a room, go to the room's _Scripts_ tab and add this to "Before entering the room for the first time":

```quest
SpawnZombie (this)
SpawnZombie (this)
```

You can call `SpawnZombie` from any script, such as when the player sets off a car alarm.

## Attacking

Add a command with the pattern `attack #object#; hit #object#; kill #object#`, and this script:

```quest
weapon = GetAttribute(game.pov, "equipped")
if (weapon = null) {
  weapon = game.pov
}
if (object = game.pov or not HasInt(object, "health")) {
  msg ("You can't attack that.")
}
else if (GetBoolean(object, "dead")) {
  msg ("It's already dead.")
}
else {
  DoAttack (game.pov, weapon, object, false)
}
```

The player's `equipped` attribute holds the weapon they're holding ready. `GetAttribute` gives `null` if it hasn't been set yet, and then the player fights bare-handed. Anything with `health` can be attacked, except the player.

Commands don't add themselves to an object's hyperlink menu, so on the zombie's _Object_ tab, add "Attack" and "Shoot" to its "Display verbs".

## Equipping weapons

Add a command with the pattern `equip #object#; wield #object#`:

```quest
if (object = game.pov or not HasAttribute(object, "damage")) {
  msg ("You can't equip that.")
}
else if (not object.parent = game.pov) {
  msg ("You aren't carrying " + object.article + ".")
}
else if (object = GetAttribute(game.pov, "equipped")) {
  msg ("You already have your " + GetDisplayAlias(object) + " ready.")
}
else {
  game.pov.equipped = object
  msg ("You ready your " + GetDisplayAlias(object) + ".")
}
```

Anything with a `damage` attribute counts as a weapon. And a command with the pattern `unequip #object#`:

```quest
if (not object = GetAttribute(game.pov, "equipped")) {
  msg ("You don't have " + object.article + " ready.")
}
else {
  game.pov.equipped = null
  msg ("You put away your " + GetDisplayAlias(object) + ".")
}
```

Both use `GetDisplayAlias`, so the messages name whichever weapon the player chose.

## Firearms

Add a pistol, tick "Take", and give it the four weapon attributes for when it's used as a club, then the firearm ones and its ammo:

| Attribute | Type | Value |
|---|---|---|
| `attack` | Integer | 0 |
| `damage` | Integer | 2 |
| `hitmsg` | String | `You pistol-whip #target#.` |
| `missmsg` | String | `You swing the pistol at #target# and miss.` |
| `firearmattack` | Integer | 5 |
| `firearmdamage` | String | `2d6` |
| `firearmhitmsg` | String | `You shoot #target#.` |
| `firearmmissmsg` | String | `You fire at #target# and miss.` |
| `ammo` | Integer | 2 |
| `maxammo` | Integer | 6 |

`ammo` is how many rounds are loaded. Set its description to show them:

```
A battered pistol, with {pistol.ammo} of {pistol.maxammo} rounds loaded.
```

Spare bullets belong to the player. Give the player object a `bullets` attribute (Integer, say 4), and to show it in the status pane, add `bullets` to "Status attributes" at the top of the _Attributes_ tab, with the format `Bullets: !` - see [Status attributes](/status-attributes).

Add a command with the pattern `shoot #object#`:

```quest
weapon = GetAttribute(game.pov, "equipped")
if (weapon = null) {
  weapon = game.pov
}
if (object = game.pov or not HasInt(object, "health")) {
  msg ("You can't shoot that.")
}
else if (GetBoolean(object, "dead")) {
  msg ("It's already dead.")
}
else if (not HasInt(weapon, "ammo")) {
  msg ("You haven't got a gun ready.")
}
else if (weapon.ammo = 0) {
  msg ("Click. Your " + GetDisplayAlias(weapon) + " is empty.")
}
else {
  DoAttack (game.pov, weapon, object, true)
}
```

A weapon with an `ammo` attribute is a firearm. When the pistol is empty, the player can still ATTACK with it, using its ordinary `attack` and `damage`.

Then add a command with the pattern `reload #object#`:

```quest
if (not HasInt(object, "ammo")) {
  msg ("You can't reload " + object.article + ".")
}
else if (object.ammo = object.maxammo) {
  msg ("It's already fully loaded.")
}
else if (game.pov.bullets = 0) {
  msg ("You haven't got any bullets left.")
}
else {
  n = object.maxammo - object.ammo
  if (n > game.pov.bullets) {
    n = game.pov.bullets
  }
  object.ammo = object.ammo + n
  game.pov.bullets = game.pov.bullets - n
  msg ("You load " + n + " bullets into your " + GetDisplayAlias(object) + ".")
}
```

## Monsters fighting back

Each zombie attack is an object with the same four attributes as a weapon. Put them in the `offstage` room with the zombie, named to match its `attacks` list. The zombie's name replaces `#Attacker#`:

| Object | `attack` | `damage` | `hitmsg` | `missmsg` |
|---|---|---|---|---|
| `zombie_bite` | 0 | `1d8` | `#Attacker# sinks its teeth into your arm.` | `#Attacker# snaps at you, but you pull away.` |
| `zombie_claw` | 2 | `1d4` | `#Attacker# rakes its nails across your face.` | `#Attacker# claws at you and misses.` |
| `zombie_vomit` | -2 | `2d6` | `#Attacker# vomits all over you.` | `#Attacker# retches, but nothing comes up.` |

Now select the `game` object, click "Add Turn Script", tick "Enabled when the game begins", and give it this script:

```quest
foreach (obj, GetDirectChildren(game.pov.parent)) {
  if (HasAttribute(obj, "attacks") and not GetBoolean(obj, "dead") and game.pov.health > 0) {
    DoAttack (obj, GetObject(PickOneString(obj.attacks)), game.pov, false)
  }
}
```

After every turn, each living zombie in the room picks one of its attacks at random and uses it on the player. Checking `game.pov.health` stops the other zombies attacking once the player is dead.

Turn scripts only run when Quest Viva understood the command, so a typing mistake doesn't give the zombies a free attack.

A different monster can share these attacks, or have its own in its `attacks` list. To make one attack more likely than another, list it twice.

## Looting

Add a second off-stage room called `loot`, and put in it the things the player might find on a body - say a tin of beans and a chocolate bar. Make them [edible](/howto/world/edible), with "Change health by" set, so they restore health.

On the zombie's _Verbs_ tab, add "search" and choose "Run a script":

```quest
if (not GetBoolean(this, "dead")) {
  msg ("Not while it's still moving!")
}
else if (GetBoolean(this, "searched")) {
  msg ("You've already searched it.")
}
else {
  this.searched = true
  if (RandomChance(50)) {
    n = DiceRoll("2d3")
    game.pov.bullets = game.pov.bullets + n
    msg ("You find " + n + " bullets.")
  }
  else {
    item = CloneObjectAndMoveHere(PickOneChild(loot))
    msg ("You find " + GetDisplayName(item) + ".")
  }
}
```

Half the time, the body has between two and six bullets. Otherwise, `PickOneChild` picks one of the things in the `loot` room and `CloneObjectAndMoveHere` puts a copy on the ground, so the same item can turn up more than once.

## Playing it

```
> equip pistol
You ready your pistol.
The rotting zombie sinks its teeth into your arm. (1 damage)
The groaning zombie vomits all over you. (7 damage)

> shoot rotting zombie
You shoot the rotting zombie. (9 damage)
The rotting zombie rakes its nails across your face. (3 damage)
The groaning zombie rakes its nails across your face. (2 damage)

> shoot rotting zombie
You shoot the rotting zombie. (7 damage)
The rotting zombie falls to the ground and lies still.
The groaning zombie retches, but nothing comes up.

> search rotting zombie
You find a chocolate bar.
The groaning zombie sinks its teeth into your arm. (2 damage)
```

## Taking it further

- **Critical hits**: in `DoAttack`, treat a roll over 18 as a critical and double the damage.
- **Armour**: give the player a `defence` attribute, and increase it when they wear something - see [Wearables](/howto/world/wearables).
- **Zombies that follow**: move living zombies into the player's room when the player leaves - see [NPCs that move](/howto/npcs/npcs-that-move).
- **Other monsters**: make another prototype, with its own `health`, `defence` and `attacks`. The commands, `DoAttack` and the turn script work for anything with `health` and an `attacks` list.
