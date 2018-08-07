---
layout: index
title: Using "doubles"
---

Doubles are an alternative number system to integers; what is the difference?


Integers 
--------

Integers are whole numbers (1, 2, 3, etc.), and in maths they can go from minus infinity, through zero, all the way up to plus infinity. Computers have limited space, and in Quest, an integer can go from -2147483647 to 2147483647 - plenty big enough for most purposes.


Non-Integers
------------

In mathematics, numbers that are expressed as a fraction or decimal are called "rational" numbers (or "real" number; rational numbers are in fact a subset of real numbers). In computing, these are called floating point number because they have a decimal point that can be anywhere in the number. While rational (and  real) numbers can range from minus infinity to plus infinity, floating point numbers are limited because of the space in a computer. They are also limited in terms of their accuracy (how many digits you can store).

More modern computers (like since the eighties or something) therefore offer a second option, a floating point number that uses double the space in the computer, and so can be considerably more accurate.

This, then, is what is known as a "double".


Why not to use doubles
----------------------

The problem with doubles is that they are approximate. Usually they are good enough, but occasionally you might see something like 6.12303176911189E-17 rather than zero. That is very close, and for calculations good enough, but it does not _look_ right.

Use integers if you possibly can.


What about money, etc.?
-----------------------

You might wonder about how you would handle money, say to represent £2.99, in Quest. Use integers!

Use integers to count the pennies, rather than doubles to count the pounds. You only need to see the pounds value when it is printed, and Quest has the `DisplayMoney` function to handle that.

For other quantities, you can do similar. If you want to track temperature to the nearest 0.1 of a degree, store the number in tenths of a degree, and output it in degrees (perhaps with the `DisplayNumber` function).


When to use doubles...
----------------------

I have been messing with Quest for over 6 years and besides experimenting for tutorials like this have never had to use anything other than integers. The only time I have seen it come up on the forum was for someone trying to track the progress of a ship across the ocean (or spaceship across space maybe), where the player specifies an angle and distance. For complex trigonmetric calculations, you are into the realm of doubles.


Trigonometry
------------

Quest supports all the usual trigonometric functions - and the obscure ones too - and others functions as well.

* Acos
* Asin
* Atan
* Cos
* Cosh
* Sin
* Sinh
* Tan
* Tanh

Note that the trigonometric functions all use radians rather than degrees. If you want to work in degrees, you will need to make your own functions. For example, you could add a function called `Sine`, returning a double and taking a single parameter, "angle", and have it convert the angle to radians before calling the built-in function.

```
return (Sin(angle * pi / 180))
```

Other Functions and Constants
---------------

The constants `e` and `pi` are built-in.

Quest also has these other functions.

* Abs
* Exp
* Log
* Log10
* Sqrt


Notes...
-------------

If you do a calculation that mixes integers and doubles, Quest will convert the integers to doubles to do the maths, and the result will be a double.

Quest offers four functions that will round your double to an integer. Note, however, that the _type_ will still be double. Rounding a double with the value 4.56 will give a double with the value 4.00.

* Ceiling
* Floor
* Round
* Truncate

To display a double to, say, three decimal places, multiple it by 1000, then round it, then divide by 1000.

```
msg("The objective is " + (Round(distance * 1000)/1000) + " km away.")
```

To convert to an integer _type_ you will need to use `cast` (note that there are no quotes around `int`).

```
my_int = cast(4.56, int)
```


