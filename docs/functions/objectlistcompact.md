---
layout: index
title: ObjectListCompact
---

    ObjectListCompact (objectlist list)

**New in Quest 5.8**    

Returns an [objectlist](../types/list.html), based on the given list, but with any repeated entries removed and any entries that are null removed. The canonical use is when combining two lists that might have some entries in common:

```
combinedlist = ObjectListCompact (list1 + list2)
