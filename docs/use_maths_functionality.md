---
layout: index
title: Use maths functionality
---

Text adventures do not have a lot of call for mathematics, but Quest does have a lot of maths functions built in nevertheless.

Basic operations
----------------

You know about +, -, \* and /, right? Quest also supports ^ and %, for raising to the power, and modulo arithmetic.

      msg ("2 to the 3 is " + (2^3))
      msg ("13 mod 3 is " + (13 % 3))

Rounding
--------

Three functions are available:

      x = 3.14
      msg ("Floor " + x + " is " + floor(x))
      msg ("Ceiling " + x + " is " + ceiling(x))
      msg ("Round " + x + " is " + round(x))
      x = 3.6
      msg ("Floor " + x + " is " + floor(x))
      msg ("Ceiling " + x + " is " + ceiling(x))
      msg ("Round " + x + " is " + round(x))
      x = -3.14
      msg ("Floor " + x + " is " + floor(x))
      msg ("Ceiling " + x + " is " + ceiling(x))
      msg ("Round " + x + " is " + round(x))
      x = -3.6
      msg ("Floor " + x + " is " + floor(x))
      msg ("Ceiling " + x + " is " + ceiling(x))
      msg ("Round " + x + " is " + round(x))

The output is:

      Floor 3.14 is 3
      Ceiling 3.14 is 4
      Round 3.14 is 3
      Floor 3.6 is 3
      Ceiling 3.6 is 4
      Round 3.6 is 4
      Floor -3.14 is -4
      Ceiling -3.14 is -3
      Round -3.14 is -3
      Floor -3.6 is -4
      Ceiling -3.6 is -3
      Round -3.6 is -4

Minimum and maximum
-------------------

Use the min and max functions:

      msg ("Min of 4 and 10 is " + min (4, 10))
      msg ("Max of 4 and 10 is " + max (4, 10))

Bitwise Operations
------------------

If you are reading a page about maths, chances are you are familiar with Boolean operations such as:

      if (door.islocked and player.haskey)

... but the operations `and`, `or` and `not` can also be used with integers (but not with integers and Booleans mixed together).

      msg ("bitwise 20 and 4 is " + (20 and 4))
      msg ("bitwise 20 or 4 is " + (20 or 4))

Twenty is 10100 in binary and 5 is 101. When you `and` them, the result is every bit they have in common, in this case just the third. When you `or` them, you get all the bits either of them have, the fifth and the third.

```
20     10100
 5       101
and      100
 or    10101
```

There is also a bitwise shift operator; it moves the binary representation of the number on the left by a number of places indicated by the number on the right (in effect, shift left multiples the right number by two to the power of the left number; right shift divides by two to the power of the right number):

      msg ("24 shifted 3 right is " + (24 >> 3))
      msg ("3 shifted 3 left is " + (3 << 3))

2 to the power 3 is eight, so the first line effectively divides by 8, to give 3, the second line multiples by eight tio give 24.

Thirty to forty years ago, this might give a significant speed boost, compared to 24 / 8. Nowadays, with faster computers, it is pretty much useless.



Tertiary Operator
-----------------

So called because it takes three values. If the first is true, it returns the second, otherwise it returns the third.

      msg (if (10 > 5, "yes it is", "no it's not"))
      msg (if (10 < 5, "yes it is", "no it's not"))
      s = if (10 < 5, "yes it is", "no it's not")
      msg (s)

The third line, for example, would otherwise be written like this:

      if (10 < 5)
        s = "yes it is"
      }
      else {
        s = "no it's not"
      }

      
The in Operator
---------------

Evaluates to true if the term on the left is in the list on the right, false otherwise.

      list = Split ("one,two,three,four", ",")
      msg ("Is it is the list? two or zero")
      msg ("two" in list)
      msg ("zero" in list)

The first line creates our list. The third line prints "true", as "two" is found in the list, the last line prints "false"; "zero" is not in the list.

If you look at the documentation, you will see this can be used in another form,

      msg ("two" in "on e", "two", "three")

However, this is not supported by quest.


Trigonometry and Other Functions
-----------------------

Every felt you cannot create the adventure game of your dreams because Quest does not support hyperbolic cosine? Fret no more!

      msg ("Absolute value of -2 is " + abs (-2))
      msg ("Sign  of -2 is " + sign (-2))
      msg ("Square root of 2 is " + sqrt (2))
      msg ("Cosine 0.5 is " + cos (0.5))
      msg ("Inverse cosine 0.5 is " + acos (0.5))
      msg ("Cosh 0.5 is " + cosh (0.5))
      msg ("2 to the 3 is " + pow (2, 3))
      msg ("The log base 10 of 7 is " + log10(7))
      msg ("The natural log of 7 is " + log(7))
      msg ("e to the power of 7 is " + exp(7))

In case you are wondering, `abs` gets the absolute value, i.e., makes it a positive value. The `sign` function returns -1 for negative numbers, 1 for positive, and zero for zero.


Constants `e` and `pi`
----------------------

They are irrational! They are transcendental! They are right there in quest!

      msg ("e is " + e)
      msg ("pi is " + pi)
     

Quest will actually allow you to go through the motions of assigning a value to these:

      e = game.pov
      msg (e)

No error will be given, but e will still be 2.718... Using e or pi as a variable name can lead to some very mysterious errors.


Indexing
--------

In most languages you can index an element in a array (or list, as they are also termed) with something like this:

      myarray[5]

It works in Quest too. You can even do it for a dictionary:

      mydictionary["peter"]

Note that these do the same as `ListItem` and 'DictionaryItem`; they return an element with an indeterminate type, and it will take Quest a moment to work out what it is handling.

Here is an example (TestFunc just prints out the value):

      list = Split ("one,two,three,four", ",")
      msg ("number 2 is " + list[2])
      d = NewDictionary()
      dictionary add (d, "Name", "Quest")
      dictionary add (d, "Author", "Alex Warren")
      msg (d["Name"])
      TestFunc (list[1])
      dictionary add (d, "" + list[1], list[2])
      msg (d["two"])

Note that in the last but one line, the key for the dictionary is '"" + list[1]`. This addition gives Quest a chance to work out what `list[1]` is.