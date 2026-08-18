---
title: "Functions for variables"
sidebar:
  order: 2
---

Functions that check or change the values of variables (and can be used on attributes too).

## Equal
```
Equal (value, value)
```

Returns a [boolean](/types#boolean) - **true** if the two values are the same, **false** otherwise. Generally this can be accomplished more easily using the equals sign, but if you try to compare two things that might be different types, this is the safer way, as it first compares the types, and only if they match does it compare the values (trying to compare an `int` with `null`, for example, will generate an error).

This does mean you can test an attribute that might not exist in one step instead of two. Instead of this:

```
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

```
if (Equal(object.status, 1)) {
  msg("This has been done.")
}
else {
  msg ("Not done yet.")
}
```

## IsDefined
```
IsDefined (string variable name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) indicating whether the specified variable name is defined in the current scope.

## IsDouble
```
IsDouble (string number)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) indicating whether the specified string represents a double.

## IsInt
```
IsInt (string number)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) indicating whether the specified string represents an integer.

## ToDouble
```
ToDouble (string number)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [double](/types#double) - converts a valid string to a double.

Note: An error occurs if the string does not represent a number.

See also [HasDouble](/functions/attributes#hasdouble) and [GetDouble](/functions/attributes#getdouble)

## ToInt
```
ToInt (string number)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [int](/types#int) - converts a valid string to an integer.

Note: An error occurs if the string does not represent a number. Use [IsInt](#isint) to test if a string represents a number first.

See also [GetInt](/functions/attributes#getint) and [HasInt](/functions/attributes#hasint)

## ToString
```
ToString (anything)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [string](/types#string). This is most useful for converting an integer or double to a string, however it will also convert an object, script, list or dictionary to a string, prefixing the string with the type (eg, "Object: player" or "List: one; two; three"). It will even convert a string to exactly the same string!

See also [GetString](/functions/attributes#getstring) and [HasString](/functions/attributes#hasstring)

## TypeOf
```
TypeOf (object, string attribute name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [string](/types#string) representing the name of the type of the specified object's attribute.

    TypeOf (value)

Returns a [string](/types#string) representing the name of the type of the specified value.

See [Attribute Types](/types/) for a list of strings that may be returned.

