---
title: "Functions for variables"
sidebar:
  order: 2
---

Functions that check or change the values of variables (and can be used on attributes too).

## cast
```quest
cast (value, type name)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Converts **value** to the named type and returns it. The type name can be quoted or bare - `cast(x, "int")` and `cast(x, int)` both work - and the useful ones are `int`, `double`, `string` and `boolean`.

```quest
cast (4.9, "int")
-> 4
cast (7, "double")
-> 7 (as a double)
```

Converting to `int` **truncates** towards zero rather than rounding, so `cast(4.9, "int")` is 4 and `cast(-4.9, "int")` is -4.

This is the way to turn a double into an integer, since [ToInt](#toint) only accepts strings - see [Values and types](/understanding/values-and-types#converting-between-types). It is most often needed after [Floor, Ceiling, Round or Truncate](/reference/functions/maths), which return a whole number but as a `double`.

`cast` is not a library function and is not implemented in C# alongside the other hard-coded functions either; it is handled directly by the expression evaluator, which is why you will not find it in the editor's function list. It exists for compatibility with expressions written for older versions of Quest.

## Equal
```quest
Equal (value, value)
```

Returns a [boolean](/reference/attributes/types#boolean) - **true** if the two values are the same, **false** otherwise. Generally this can be accomplished more easily using the equals sign, but if you try to compare two things that might be different types, this is the safer way, as it first compares the types, and only if they match does it compare the values (trying to compare an `int` with `null`, for example, will generate an error).

This does mean you can test an attribute that might not exist in one step instead of two. Instead of this:

```quest
if (HasInt(object, "status")) {
  if (object.status = 1) {
    msg("This has been done.")
  }
  else {
    msg ("Not done yet.")
  }
}
else {
  msg ("Not done yet.")
}
```

You can do:

```quest
if (Equal(object.status, 1)) {
  msg("This has been done.")
}
else {
  msg ("Not done yet.")
}
```

## if
```quest
if (boolean condition, value if true, value if false)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns one of two values depending on the condition. This is the expression form - for the [if](/reference/script-commands#if) script command, which runs one of two *scripts*, see the script commands page.

```quest
msg ("The lamp is " + if (lamp.switchedon, "on", "off") + ".")
```

Only the branch that is taken is evaluated, so the other one is safe to write even if it would fail.

Like [cast](#cast), this is handled by the expression evaluator rather than being a library or C# function, so it does not appear in the editor's function list.

## IsDefined
```quest
IsDefined (string variable name)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/reference/attributes/types#boolean) indicating whether the specified variable name is defined in the current scope.

## IsDouble
```quest
IsDouble (value)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/reference/attributes/types#boolean) indicating whether [ToDouble](#todouble) will accept **value**: true for an int, a double, or a string that represents a number, and false for anything else.

## IsInt
```quest
IsInt (value)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/reference/attributes/types#boolean) indicating whether [ToInt](#toint) will accept **value**: true for an int or a string that represents an integer, and false for anything else - including a double, even a whole one like `5.0`.

Because it takes any value, it can check a variable that might hold either a number or a string, such as one set from what the player typed:

```quest
if (IsInt(count)) {
  game.count = ToInt(count)
}
```

To ask what type a value is, rather than whether it will convert, use [TypeOf](#typeof): `TypeOf(count) = "int"`.

## ToDouble
```quest
ToDouble (value)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [double](/reference/attributes/types#double) - converts a string, int or double to a double.

Note: An error occurs if a string does not represent a number. Use [IsDouble](#isdouble) to check first.

See also [HasDouble](/reference/functions/attributes#hasdouble) and [GetDouble](/reference/functions/attributes#getdouble)

## ToInt
```quest
ToInt (value)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [int](/reference/attributes/types#int) - converts a string to an integer. Given an int, it returns it unchanged.

Note: An error occurs if a string does not represent an integer. Use [IsInt](#isint) to check first.

`ToInt` doesn't take a double, since that would mean throwing away anything after the point. Passing one stops with "ToInt function does not handle parameters of types double" - use [cast](#cast) for that.

See also [GetInt](/reference/functions/attributes#getint) and [HasInt](/reference/functions/attributes#hasint)

## ToString
```quest
ToString (anything)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [string](/reference/attributes/types#string). This is most useful for converting an integer or double to a string, however it will also convert an object, script, list or dictionary to a string, prefixing the string with the type (eg, "Object: player" or "List: one; two; three; "). It will even convert a string to exactly the same string!

See also [GetString](/reference/functions/attributes#getstring) and [HasString](/reference/functions/attributes#hasstring)

## TypeOf
```quest
TypeOf (object, string attribute name)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [string](/reference/attributes/types#string) representing the name of the type of the specified object's attribute.

```quest
TypeOf (value)
```

Returns a [string](/reference/attributes/types#string) representing the name of the type of the specified value.

See [Attribute Types](/reference/attributes/types/) for a list of strings that may be returned.

