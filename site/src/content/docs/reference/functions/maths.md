---
title: "Mathematical functions"
sidebar:
  order: 15
---

These will not be relevant to many games at all, but are available as a consequence of the .NET framework Quest Viva is built on. They are included here for completeness; if you need them, you will know what they do. There is no further documentation.

Quest Viva has `e` and `pi` as built-in constants.

These all take a single floating point number, and return the corresponding floating point number. Note that the trigonometric functions use radians rather than degrees.

* Abs
* Acos
* Asin
* Atan
* Cos
* Exp
* Log
* Log10
* Sin
* Sinh
* Sqrt
* Tan
* Tanh

These take a floating point parameter and return a whole number, but still as a floating point value - `TypeOf(Floor(2.9))` is `double`, not `int`. If you need an `int`, use `cast(Floor(2.9), "int")`; `ToInt` only accepts strings. See [Values and types](/understanding/values-and-types).

* Ceiling
* Floor
* Round
* Truncate

`Round` rounds half to even, so `Round(2.5)` is 2 and `Round(3.5)` is 4.

This one takes a floating point parameter and returns an `int`:

* Sign

These two functions take two parameters, and can be used with either floating point or integers, and return the same type.

* Max
* Min
