---
title: ListCompact
---

    ListCompact (any list list)

Returns a [list](/types#list), based on the given list, but with any repeated entries removed and any entries that are null removed. The canonical use is when combining two lists that might have some entries in common:

```
combinedlist = ListCompact (list1 + list2)
```
