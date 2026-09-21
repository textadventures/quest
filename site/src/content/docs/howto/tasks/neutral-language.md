---
title: Using neutral language
description: Write command responses that read correctly whatever object the player chose - singular, plural or a named character
---

When you write a command, you often don't know which object the player will use it on. Suppose you have an ATTACK command with this pattern:

```
attack #object#;strike #object#;hit #object#
```

The command's script gets the object in a variable called `object`, but it could be anything. The response has to read properly whether the player typed ATTACK ZOMBIE, ATTACK CROWD or ATTACK MARY. If it's "You attack it. It looks angry." for all three, the player won't be impressed.

Quest Viva has attributes and functions for exactly this. The text processor can't do any of it, so you build the message as an [expression](/howto/scripting/introtocoding#expressions-and-operators) - in the editor, add a "Print a message" script and change its dropdown from "message" to "expression".

## Set up your objects

The functions below rely on each object having the right type. On the object's _Setup_ tab, set the "Type" dropdown. For this example, set Mary to "Female character (named)" and the crowd to "Inanimate objects (plural)". The zombie can stay as the default, "Inanimate object".

The type sets three attributes, which you can see under "Advanced" on the same tab:

| Attribute | Zombie | Crowd | Mary |
|---|---|---|---|
| `gender` ("Gender") | it | they | she |
| `article` ("Article") | it | them | her |
| `possessive` ("Possessive") | its | their | her |

So this:

```quest
msg ("You attack " + object.article + "; " + object.gender + " looks angry.")
```

prints "You attack them; they looks angry." for the crowd. That's nearly right - the verb needs fixing, which is covered [below](#conjugation).

## Names

To refer to an object by name, use one of these functions rather than `object.name`. All of them use the object's alias if it has one, and its name if it doesn't:

- `GetDisplayAlias` gives just the alias.
- `GetDisplayName` adds the prefix: "a" or "an" by default, nothing for a named character, or whatever you've set as the object's "Prefix".
- `GetDefiniteName` adds "the", except for named characters.

Here are four objects. The shoes have "Use default prefix and suffix" unticked and "some" as their "Prefix". Zoë's name is `zoe` - it's what your scripts use - and her alias is "Zoë", which is what the player sees.

| | teapot | shoes | Mary | zoe |
|---|---|---|---|---|
| Type | Inanimate object | Inanimate objects (plural) | Female character (named) | Female character (named) |
| Alias | | | | Zoë |
| `GetDisplayAlias` | teapot | shoes | Mary | Zoë |
| `GetDisplayName` | a teapot | some shoes | Mary | Zoë |
| `GetDefiniteName` | the teapot | the shoes | Mary | Zoë |

So back to the crowd and Mary:

```quest
msg ("You can see " + GetDisplayName(object) + ".")
msg ("You attack " + GetDefiniteName(object) + ". " + object.gender + " look angry.")
```

For the crowd, that prints:

```
You can see a crowd.
You attack the crowd. they look angry.
```

and for Mary:

```
You can see Mary.
You attack Mary. she look angry.
```

## Capitalisation

The second sentence needs a capital letter. `CapFirst` capitalises the first letter of a string:

```quest
msg ("You attack " + GetDefiniteName(object) + ". " + CapFirst(object.gender) + " look angry.")
```

That gives "They look angry." for the crowd, but still "She look angry." for Mary.

## Conjugation

The verb has to agree with the object. `Conjugate` takes the object doing the action and the verb, and returns the right form: "look" for the crowd, "looks" for Mary and the zombie. Use "be" for the verb "to be", which gives "is" or "are".

```quest
msg ("You attack " + GetDefiniteName(object) + ". " + CapFirst(object.gender) + " " + Conjugate(object, "look") + " angry.")
```

```
You attack the crowd. They look angry.
You attack Mary. She looks angry.
You attack the zombie. It looks angry.
```

Starting a sentence with the object doing something is so common that `WriteVerb` does it in one go. It takes the object's `gender`, capitalises it and adds the conjugated verb. This gives exactly the same result as the last example:

```quest
msg ("You attack " + GetDefiniteName(object) + ". " + WriteVerb(object, "look") + " angry.")
```

Putting it all together:

```quest
msg (WriteVerb(object, "be") + " not amused. " + CapFirst(object.possessive) + " face says it all.")
```

prints "It is not amused. Its face says it all." for the zombie and "She is not amused. Her face says it all." for Mary.

Quest Viva's own messages are written this way. `WriteVerb(game.pov, "can't")` gives "You can't" - see [Changing the game's messages](/howto/world/changing-templates) for how to change them.
