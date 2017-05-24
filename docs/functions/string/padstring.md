---
layout: index
title: PadString
---

    PadString (string input, int length, string pad)

**New in Quest 5.7**
    
Returns a [string](../../types/string.html) that has been padded to the given length with the given padding.

```
PadString("23", 4, "0")
 -> "0023"
PadString("12345", 4, "0")
 -> "12345"
PadString("23", 4, "0")
 -> "0023"
```
  
You can use integers too:

```
PadString(23, 4, 0)
 -> "0023"
```

It will attempt to do it if the pad string is more than 1 character, but will be approximate. If the pad string is empty, it will throw an error.

