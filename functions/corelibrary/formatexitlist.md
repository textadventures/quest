---
layout: index
title: FormatExitList
---

    FormatExitList (string pre-list, objectlist exits, string pre-final, string post-list)

Returns a [string](../../../types/string.html) containing a formatted list of exits.

For example, this:

    FormatExitList("You can go", ScopeExits(), "or", ", if you like.")

may return output like this:

> You can go east, west or south, if you like.
