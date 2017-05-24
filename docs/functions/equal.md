---
layout: index
title: Equal
---

    Equal (value, value)

**New in Quest 5.7**    

Returns a [boolean](../types/boolean.html) - **true** if the two values are the same, **false** otherwise. Genetralls this can be accomplished more easily using the equals sign, but if you try to compare two things that might be different types, this is the safer way, as it first compares the types, and only if they match does it compare the values (trying to compare an `int` with `null`, for example, will generate an error).
