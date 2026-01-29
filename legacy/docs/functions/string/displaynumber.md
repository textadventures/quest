---
layout: index
title: DisplayNumber
---

    DisplayNumber (int input, string format)

**New in Quest 5.7**
    
Returns a [string](../../types/string.html), the given number formatted. The format should consist of:

- any number of non-digits (optional)
- a number, the minimum number of digits left of the decimal point (padded with 0)
- a single character decimal separator
- a number, the number of digits after the decimal point
- any number of non-digits (optional)

Note that the input number will be made positive, and then divided by 10 to the power of the second number (see [Decimalise](decimalise.html)).

```
DisplayNumber(1234, "+1.1")
  -> "+123.4"
DisplayNumber(1234, "3.2")
   -> "012.34"
DisplayNumber(1234, "(1.1)")
 -> "(123.4)"
DisplayNumber(1234, "(3,3)")
 -> "(001,234)"
```

