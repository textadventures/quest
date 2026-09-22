---
title: Values and types
description: The types a value can have, how to test for them, what an unset attribute does, and how to convert between types
---

Every value in a Quest Viva game has a type. Most of the time you don't have to think about it - you set `hat.price` to 12 and it's an integer. It starts to matter when a condition never fires, when a calculation stops with an error, or when you want to know whether an attribute has been set at all.

After this page you'll know which types exist, how to find out what type something is, what happens when you read an attribute that was never set, and how to convert between types.

## The types

| Type | `TypeOf` gives | Example |
|---|---|---|
| Text | `string` | `"a bowler hat"` |
| Whole number | `int` | `12` |
| Number with a fractional part | `double` | `0.4` |
| True or false | `boolean` | `true` |
| An object, room, exit or any other element | `object` | `hat` |
| A list | `stringlist`, `objectlist`, `list` | `Split("a;b", ";")` |
| A dictionary | `stringdictionary`, `objectdictionary`, `scriptdictionary`, `dictionary` | `NewStringDictionary()` |
| A block of script | `script` | `hat.take => { ... }` |
| Nothing at all | `null` | `null` |

On the _Attributes_ tab these are the same types you pick from when you add an attribute. See also [Lists](/howto/scripting/lists), [Dictionaries](/howto/scripting/dictionaries) and, for the difference between `int` and `double`, [Maths](/howto/scripting/maths).

## Finding out what type a value is

`TypeOf` takes either a value or an object and an attribute name, and returns one of the names in the table above as a string:

```quest
msg (TypeOf(hat.price))         // int
msg (TypeOf(hat, "price"))      // int
msg (TypeOf(hat, "take"))       // boolean
```

The two-argument form is the safer one, because it also works when the attribute has never been set - it returns `"null"` instead of failing.

```quest
if (TypeOf(obj, "description") = "script") {
  do (obj, "description")
}
else {
  msg (obj.description)
}
```

## Unset attributes

An attribute that has never been set doesn't exist. What happens when you read it depends entirely on what you do with it:

```quest
msg ("Price: " + hat.nosuchattribute)
```

prints `Price: ` - joining nothing onto a string just adds nothing. But:

```quest
msg ("Price: " + (hat.nosuchattribute + 1))
```

stops with `'hat.nosuchattribute' is null (it has not been set) and cannot be used in this calculation`, and

```quest
if (hat.nosuchattribute) {
```

stops with `Object reference not set to an instance of an object`, which is the engine's way of saying the same thing less helpfully.

So the string case quietly does the wrong thing, and the other two stop the turn. In all three the fix is the same: don't read an attribute you aren't sure about.

### Testing whether an attribute is set

`HasAttribute` answers that directly:

```quest
if (HasAttribute(hat, "price")) {
  msg ("It costs " + hat.price + " gold.")
}
```

`and` and `or` stop as soon as the answer is known, so `if (HasAttribute(hat, "price") and hat.price > 10)` is safe - the second half never runs when the first is false.

There are also type-specific versions - `HasString`, `HasInt`, `HasDouble`, `HasBoolean`, `HasObject`, `HasScript` - which are true only when the attribute exists *and* holds that type. `HasAttribute(hat, "price")` is true for an integer `price`; `HasString(hat, "price")` is false.

`HasAttribute` counts attributes the object gets from its [types](/customise/object-types) as well as its own. If you need to know whether this particular object has its own copy, use `GetAttributeNames(obj, false)`, which lists only an object's own attributes - `GetAttributeNames(obj, true)` includes inherited ones.

### Reading with a default

The `Get` family reads an attribute and falls back to a sensible default rather than failing, which is usually what you want:

| Function | When the attribute isn't set |
|---|---|
| `GetString(obj, "name")` | An empty string |
| `GetInt(obj, "name")` | `0` |
| `GetDouble(obj, "name")` | `0` |
| `GetBoolean(obj, "name")` | `false` |
| `GetAttribute(obj, "name")` | `null` |

This makes counters and flags easy to write without any setup:

```quest
game.beetlescaught = GetInt(game, "beetlescaught") + 1
if (GetBoolean(npc, "hasbeenbribed")) {
  msg ("He nods you through.")
}
```

The catch is that these functions are strict about type, and a mismatch looks exactly like an unset attribute. If `hat.weight` is the double `0.4`, then `GetInt(hat, "weight")` returns `0`, not `1` - it doesn't convert, it gives up. When a `Get` call keeps returning zero, check the attribute's type on the _Attributes_ tab before anything else.

`GetBoolean` in particular is worth using everywhere in place of a bare attribute read. A flag you've never ticked in the editor, or never set in a script, isn't stored as `false` - it simply isn't there, and `GetBoolean` is what turns that into the `false` you were expecting.

## Setting an attribute to null

Assigning `null` doesn't store an empty value - it removes the attribute:

```quest
hat.colour = "red"
msg (HasAttribute(hat, "colour"))   // True
hat.colour = null
msg (HasAttribute(hat, "colour"))   // False
```

That's the only way to remove an attribute, and it's genuinely useful. If an object's type supplies a value, removing the object's own copy makes the type's value visible again:

```quest
// bob is of type "male", which sets gender to "he"
msg (bob.gender)        // he
bob.gender = "she"
msg (bob.gender)        // she
bob.gender = null
msg (bob.gender)        // he
```

The same trick works for scripts. Give a cursed object its own `take` script while the curse is on it, and set that attribute to `null` when the curse lifts - the object goes back to behaving like every other takeable object, with no special case in your code.

Note that `HasAttribute(bob, "gender")` is still `True` after setting it to `null`, because the type still supplies one.

## Comparing with null

You can compare anything with `null`, including an attribute that was never set:

```quest
if (hat.colour = null) {
  msg ("No colour set.")
}
```

Any value that *has* been set compares as not equal to `null`, whatever its type.

`=` compares loosely across number types, so `3 = 3.0` is `true`. `Equal` compares the type first, so `Equal(3, 3.0)` is `false`. Use `=` for ordinary comparisons, and `Equal` when you're comparing two values whose types you don't know - it never throws, whatever it's given.

Passing `null` where a function expects an object is a different matter, and fails immediately:

```quest
obj = null
msg (HasString(obj, "name"))
```

gives `Value cannot be null. (Parameter 'obj')`.

## Converting between types

| Function | Does |
|---|---|
| `ToString(value)` | Anything to a string |
| `ToInt(string)` | A string to an integer |
| `ToDouble(string)` | A string to a double |
| `cast(value, "int")` | A double or string to an integer, discarding anything after the point |
| `cast(value, "double")` | An integer or string to a double |
| `IsInt(string)`, `IsDouble(string)`, `IsNumeric(string)` | Whether a string will convert |

`ToInt` only accepts strings: `ToInt(4.9)` stops with `ToInt function does not handle parameters of types Double`. To turn a double into an integer, use `cast(4.9, "int")`, which gives `4`.

Always check before converting anything the player typed:

```quest
msg ("How many do you want?")
answer = GetInput()
if (IsInt(answer)) {
  game.order = ToInt(answer)
}
else {
  msg ("That isn't a number.")
}
```

`ToString` on an object gives `Object: hat`, which is no use in player-facing text - use `GetDisplayName` or `GetDisplayAlias` instead.

## See also

- [Writing code](/howto/scripting/writing-code) - expressions, operators and the common mistakes
- [Attribute functions](/reference/functions/attributes) - the full `Get`/`Has`/`Set` reference
- [Maths](/howto/scripting/maths) - rounding, and choosing between `int` and `double`
- [Using inherited types](/customise/object-types) - where an attribute's fallback value comes from
- [The Debugger](/howto/testing/debugging) - see what every attribute actually holds while the game runs
