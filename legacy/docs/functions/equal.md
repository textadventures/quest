---
layout: index
title: Equal
---

    Equal (value, value)

**New in Quest 5.7**    

Returns a [boolean](../types/boolean.html) - **true** if the two values are the same, **false** otherwise. Generally this can be accomplished more easily using the equals sign, but if you try to compare two things that might be different types, this is the safer way, as it first compares the types, and only if they match does it compare the values (trying to compare an `int` with `null`, for example, will generate an error).

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
