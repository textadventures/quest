---
title: Maths
description: Arithmetic, whole numbers and decimals, rounding, the if function, the in operator and other maths functions
---

Most games only need to add and subtract - a score, a health bar, a count of coins. This page covers that, and the rest of the maths Quest Viva can do when you need it.

## Whole numbers and decimals

Quest Viva has two kinds of number: whole numbers (`int`), such as 3 or -250, and decimals (`double`), such as 2.5. Use whole numbers whenever you can:

- Dividing two whole numbers gives a whole number, dropping any remainder: `7 / 2` is 3. If either number is a decimal, the result is a decimal: `7 / 2.0` is 3.5.
- Decimals are approximate. `0.1 + 0.2` gives 0.30000000000000004, and `Cos(pi / 2)` gives 6.123233995736766E-17 rather than 0. That's close enough for a calculation, but it doesn't look right when printed.

For money, store the amount in pennies (or cents) as a whole number, and only format it when you print it. `DisplayMoney` does that using the format in `game.moneyformat`:

```quest
game.moneyformat = "£!1.2!"
msg ("That will be " + DisplayMoney(299) + ", please.")
```

That prints "That will be £2.99, please." The same idea works for other quantities: to track a temperature to a tenth of a degree, store it in tenths, and print it with `DisplayNumber (temperature, "1.1")`, which shows 215 as 21.5. See [DisplayMoney](/reference/functions/string#displaymoney) for the format.

## Arithmetic

As well as `+`, `-`, `*` and `/`, you can use `%` for the remainder after dividing, and `^` to raise to a power:

```quest
msg ("13 mod 3 is " + (13 % 3))
msg ("2 to the 3 is " + (2^3))
```

That prints "13 mod 3 is 1" and "2 to the 3 is 8". `%` is handy for telling whether one number divides exactly into another: `n % 2 = 0` is true when `n` is even.

## Rounding

There are four functions for rounding a decimal:

| Value | `Floor` | `Ceiling` | `Round` | `Truncate` |
|---|---|---|---|---|
| 3.14 | 3 | 4 | 3 | 3 |
| 3.6 | 3 | 4 | 4 | 3 |
| -3.14 | -4 | -3 | -3 | -3 |
| -3.6 | -4 | -3 | -4 | -3 |

`Floor` always rounds down, `Ceiling` always rounds up, `Round` goes to the nearest whole number, and `Truncate` just drops the decimal part. When a number is exactly halfway, `Round` goes to the nearest even number, so `Round(2.5)` is 2 and `Round(3.5)` is 4.

`Round` can also round to a number of decimal places:

```quest
msg ("The ship is " + Round(distance, 2) + " km away.")
```

With `distance` set to 12.345678, that prints "The ship is 12.35 km away."

All four functions still give you a decimal, even though it has no fractional part - `Round(4.56)` is 5, but it's the decimal 5. To turn it into a whole number, use `cast`, with no quotes around `int`:

```quest
whole = cast(Round(4.56), int)
```

## Minimum and maximum

`Min` and `Max` give the smaller or larger of two numbers. They're a neat way to keep a value within limits:

```quest
player.health = Min(player.health + 20, 100)
```

That adds 20 to the player's health, but never takes it above 100.

## Choosing a value with if

`if` can also be used as a function, which takes a condition and two values. It gives you the first value if the condition is true, and the second if it's false:

```quest
msg ("The lamp is " + if (lamp.switchedon, "on", "off") + ".")
```

That does the same as this:

```quest
if (lamp.switchedon) {
  s = "on"
}
else {
  s = "off"
}
msg ("The lamp is " + s + ".")
```

## Checking a list with in

`in` is true if the value on the left is in the list on the right:

```quest
colours = Split("red;green;blue", ";")
msg ("green" in colours)
msg ("pink" in colours)
```

That prints "True" and then "False". You don't need to make a list first - you can put the values in brackets:

```quest
if (game.pov.parent.name in ("kitchen", "larder", "scullery")) {
  msg ("You can smell baking.")
}
```

## Indexing lists and dictionaries

You can get an item from a list or a dictionary with square brackets. Lists count from 0:

```quest
colours = Split("red;green;blue", ";")
msg (colours[1])
```

That prints "green". `colours[1]` does the same as `ListItem(colours, 1)`, and `prices["apple"]` does the same as `DictionaryItem(prices, "apple")`.

## Other functions

Quest Viva has these other maths functions, and the constants `e` and `pi`:

| Function | Gives |
|---|---|
| `Abs(-2)` | The value without its sign: 2 |
| `Sign(-2)` | -1 for a negative number, 1 for a positive one, 0 for zero |
| `Sqrt(2)` | The square root: 1.4142135623730951 |
| `Pow(2, 3)` | 2 to the power of 3: 8 |
| `Exp(7)` | `e` to the power of 7 |
| `Log(7)`, `Log10(7)` | The natural logarithm, and the logarithm to base 10 |
| `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`, `Sinh`, `Cosh`, `Tanh` | Trigonometry |

The trigonometry functions work in radians, not degrees. To use degrees, multiply by `pi / 180` first:

```quest
msg (Sin(30 * pi / 180))
```

That prints 0.49999999999999994 - which, as above, is how decimals go.

Don't use `e` or `pi` as the names of local variables. Assigning to them is silently ignored, so they keep their values of 2.718... and 3.141....

## Bitwise operations

`and`, `or` and `xor` also work on whole numbers, comparing them one binary digit at a time. `<<` and `>>` shift the digits left or right:

```quest
msg (20 and 4)
msg (20 or 4)
msg (20 xor 4)
msg (3 << 3)
msg (24 >> 3)
```

These print 4, 20, 16, 24 and 3. Twenty is 10100 in binary and 4 is 100: `and` keeps the digits they both have (100, which is 4), `or` keeps the digits either has (10100, which is 20), and `xor` keeps the digits only one of them has (10000, which is 16). Shifting left by 3 multiplies by 8, and shifting right by 3 divides by 8.
