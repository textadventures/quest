---
layout: index
title: Split
---

    Split (string input, string split character)

Returns a [stringlist](../../types/stringlist.html) where the input has been split into individual strings by the split character. Useful for turning a comma-separated string into a list of strings, for example.

As of version 5.7.2, you can omit the split character, and Quest will assume it is a semicolon.

    Split (string input, string split character)

These two lines are equivalent:

    list = Split("one;two;three;four", ";")
    list = Split("one;two;three;four")
