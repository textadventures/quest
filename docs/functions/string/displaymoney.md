---
layout: index
title: DisplayMoney
---

    DisplayMoney (int money)

**New in Quest 5.7**
    
Returns a [string](../../types/string.html) that is the given number, formatted according to the string in game.moneyformat. This allows money to be printed consistedly across your game.

The string game.moneyformat can be formated in three ways:

With a single !, the value will be inserted at that point.

```
game.moneyformat = "! credits"
DisplayMoney(1234)
  -> "1234 credits"
DisplayMoney(-1234)
  -> "-1234 credits"
```

With two !, the bit between the exclamation marks will be used to format the number, using [DisplayNumber](displaynumber.html). The format is a number, a separator and a second number, where the first number is the minum number digits left of the decimal (padded with zeroes), and the second number is the number of decimal places. A + at the start will cause a + to appear at the start of the number if positive.

```
game.moneyformat = "!3.2! credits"
DisplayMoney(1234)
  -> "012.34 credits"
DisplayMoney(-1234)
  -> "-012.34 credits"
game.moneyformat = "!+3,2! credits"
DisplayMoney(1234)
  -> "+012,34 credits"
DisplayMoney(-1234)
  -> "-012,34 credits"
```
  
With three !, the bit between the first two exclamation marks will be used to format positive numbers (and zero), and the next bit for negative; again using [DisplayNumber](displaynumber.html).

```
game.moneyformat = "!+3.2!-3.2! credits"
DisplayMoney(1234)
  -> "+012.34 credits"
DisplayMoney(-1234)
  -> "-012.34 credits"
game.moneyformat = "!$1,2!($1,2)!"
DisplayMoney(1234)
  -> "$12,34"
DisplayMoney(-1234)
  -> "($12,34)"
```  
  
It will throw an error if game.moneyformat is not set or not understood.

The easiest way to set game.moneyformat, is to tick "Money" on the _Features_ tab of the game object, and to set the format on the _Player_ tab. If you do not want the built-in money statis attribute displayed, untick "Money" on the _Features_ tab of the game object - the format will still be set, but money will not be shown as a status attribute.
