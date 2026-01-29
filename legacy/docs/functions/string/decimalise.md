---
layout: index
title: Decimalise
---

    Decimalise (int input, int places)

**New in Quest 5.7**
    
Returns a [string](../../types/string.html) with the given number divided by 10^places and then displayed as a decimal. For example, if `places` is 2, the number is divided by 100, and then shown with two digits after the decimal point. If `places` is zero, the number is returns as a string (no decimal point).

This is especially useful for displaying money, when you are tracking the number of pennies the player has, and want to show the number of pounds.


```
Decimalise(1234, 2)
 -> "12.34"
Decimalise(1234, 1)
 -> "123.4"
Decimalise(1234, 0)
 -> "1234"
```