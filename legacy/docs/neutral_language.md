---
layout: index
title: Using neutral language
---

Often when creating a text adventure you will find you do not know what you are talking about!

This is often the case with a command. Let us suppose we have an ATTACK command, with this pattern:

> attack #object#;strike #object#;hit #object#

In the command's script, you have this thing called "object", but you do not know what it is. You need your command to work - and to read properly, whether the player did ATTACK ZOMBIE, ATTACK CROWD or ATTACK MARY. If the response is "You attack it. It looks angry." for all three, the player will not be impressed. We need to make our response language neutral.


Attributes
----------

To help you, Quest has a number of attributes and functions built-in. However, you do need to set up your objects correctly. On the _Setup_ tab, in our example, set Mary to be a "Female character (named)" and set the crowd to "Inanimate objects (plural)" (the zombie will be fine as the default "inanimate object").

You can see the attributes on that same tab, and see them change when you change the type: gender, article and possessive (but note, all lower-case).

Set the response to:

> "You attack " + object.article + "; " + object.gender + " look angry."

Now if the player attacks the crowd, she will see "You attack them; they look angry."


Names
-----

Some of your objects might have aliases and some might not (as names are limited in what characters you can use). When you want to refer to an object's name, use either `GetDisplayName` or `GetDisplayAlias` or `GetDefiniteName`. All will return the alias if the object has one, or the name otherwise. `GetDisplayName` will add the prefix to the object; this is "a" or "an" (by default; depending on if it starts with a vowel or not) except for named characters. `GetDefiniteName` will add "the" before the name, if applicable.

To illustrate, here are three objects. The shoes were set up with "Default prefix" unticked, and "some" as the prefix.

<table>
  <tr><td><i>Name</i></td><td>teapot</td><td>shoes</td><td>Zoë</td></tr>
  <tr><td><i>Alias</i></td><td></td><td></td><td>Zoe</td></tr>
  <tr><td><i>Type</i></td><td>Inanimate object</td><td>Inanimate object (plural)</td><td>Female</br>character<br/>(named)</td></tr>
  <tr><td><i>GetDisplayAlias</i></td><td>teapot</td><td>shoes</td><td>Zoë</td></tr>
  <tr><td><i>GetDisplayName</i></td><td>a teapot</td><td>some shoes</td><td>Zoë</td></tr>
  <tr><td><i>GetDefiniteName</i></td><td>the teapot</td><td>the shoes</td><td>Zoë</td></tr>
</table>

So back to attacking the crowd and Mary, we can now do this:

```
"You can see " + GetDisplayName(object) + "."
-> You can see a crowd.
-> You can see Mary.

"You attack " + GetDefiniteName(object) + ". " + object.gender + " look angry."
-> You attack the crowd. they look angry.
-> You attack Mary. she look angry.
```


Capitalisation
--------------

We need a capital at the start of the second sentence. We can use the `CapFirst` function.

```
"You attack " + GetDefiniteName(object) + ". " + CapFirst(object.gender) + " look angry."
-> You attack the crowd. They look angry.
-> You attack Mary. She look angry.
```


Conjugation
------------

We also need to conjugate the verb so it is of the correct form. Quest has the `Conjugate` function for that, it takes the object that is doing the verb, followed by the verb as a string (use "be" for the verb to be, by the way).

```
"You attack " + GetDefiniteName(object) + ". " + CapFirst(object.gender) + " " + Conjugate(object, "look") + " angry."
-> You attack the crowd. They look angry.
-> You attack Mary. She looks angry.
```

Because we often want to start a sentence with the object doing the verb, Quest has a `WriteVerb` that will get the gender of the object, capitalise it and add the conjugated verb. This is used a lot in the language files for built-in responses.

```
"You attack " + GetDefiniteName(object) + ". " + WriteVerb(object, "look") + " angry."
```