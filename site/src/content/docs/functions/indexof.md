---
title: IndexOf
---

    IndexOf (list, anything)

Returns an [int](/types#int) - the position of the given element in the list, or -1 if it is not in the list. Note that lists count from zero.

```
list = Split("One;Two;Three;Four")
msg(IndexOf(list, "One")
// -> 0
msg(IndexOf(list, "Four")
// -> 3
msg(IndexOf(list, "Five")
// -> -1
```