---
layout: index
title: ListCompact
---

    ListCompact (any list list)

**New in Quest 5.7**    

Returns a [list](../types/list.html), based on the given list, but with any repeated entries removed and any entries that are null removed. The canonical use is when combining two lists that might have some entries in common:

```
combinedlist = ListCompact (list1 + list2)
```
