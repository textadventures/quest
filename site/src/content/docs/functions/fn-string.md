---
title: "String Functions"
sidebar:
  order: 9
---

## Asc

Asc (string input)

The Asc function returns the character code of the input.

Maps to the VB.net [Asc function](http://msdn.microsoft.com/en-us/library/zew1e4wc%28v=VS.80%29.aspx).

## CapFirst

CapFirst (input)

Returns a [string](/types#string) with the first character of the input capitalised.

## Chr

Chr (int input)

The Chr function returns the character with the character code of the input.

Maps to the VB.net [Chr function](http://msdn.microsoft.com/en-us/library/613dxh46%28v=VS.80%29.aspx).

## Conjugate

Conjugate (object, string verb)

Returns the correct form of the verb for the given object, based on the "gender" attribute of the object. This allows authors to create responses neutral with respect to the object.

```
Conjugate (crowd, "be")
-> "are"
Conjugate (crowd, "do")
-> "do"
Conjugate (crowd, "sit")
-> "sit"
Conjugate (dog, "be")
-> "is"
Conjugate (dog, "do")
-> "does"
Conjugate (dog, "sit")
-> "sits"
```

See also [WriteVerb](#writeverb)

## Decimalise

Decimalise (int input, int places)

Returns a [string](/types#string) with the given number divided by 10^places and then displayed as a decimal. For example, if `places` is 2, the number is divided by 100, and then shown with two digits after the decimal point. If `places` is zero, the number is returns as a string (no decimal point).

This is especially useful for displaying money, when you are tracking the number of pennies the player has, and want to show the number of pounds.

```
Decimalise(1234, 2)
 -> "12.34"
Decimalise(1234, 1)
 -> "123.4"
Decimalise(1234, 0)
 -> "1234"
```

## DisplayMoney

DisplayMoney (int money)

Returns a [string](/types#string) that is the given number, formatted according to the string in game.moneyformat. This allows money to be printed consistently across your game.

The string game.moneyformat can be formatted in three ways:

With a single !, the value will be inserted at that point.

```
game.moneyformat = "! credits"
DisplayMoney(1234)
  -> "1234 credits"
DisplayMoney(-1234)
  -> "-1234 credits"
```

With two !, the bit between the exclamation marks will be used to format the number, using [DisplayNumber](#displaynumber). The format is a number, a separator and a second number, where the first number is the minimum number digits left of the decimal (padded with zeroes), and the second number is the number of decimal places. A + at the start will cause a + to appear at the start of the number if positive.

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
  
With three !, the bit between the first two exclamation marks will be used to format positive numbers (and zero), and the next bit for negative; again using [DisplayNumber](#displaynumber).

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

The easiest way to set game.moneyformat, is to tick "Money" on the _Features_ tab of the game object, and to set the format on the _Player_ tab. If you do not want the built-in money status attribute displayed, untick "Money" on the _Features_ tab of the game object - the format will still be set, but money will not be shown as a status attribute.

## DisplayNumber

DisplayNumber (int input, string format)

Returns a [string](/types#string), the given number formatted. The format should consist of:

- any number of non-digits (optional)
- a number, the minimum number of digits left of the decimal point (padded with 0)
- a single character decimal separator
- a number, the number of digits after the decimal point
- any number of non-digits (optional)

Note that the input number will be made positive, and then divided by 10 to the power of the second number (see [Decimalise](#decimalise)).

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

## DynamicTemplate

DynamicTemplate (string  template name, object  object)

or

    DynamicTemplate (string  template name, string  text)

Returns a [string](/types#string) containing the requested text, based on the object or string passed in.

You can pass in multiple objects. If you only pass in one, you can refer to it using the variable "object". Otherwise you can use "object1", "object2" etc.

See [Changing templates](/howto/world/changing_templates)

*Example:* We want to provide a templated message about a blocked exit.

First the dynamic template is defined as:

    <dynamictemplate name="BlockedExit">"Your exit "+object.alias+" is blocked"</dynamictemplate>

Now we could add a message expression to the script in an exit, something like:

    msg (DynamicTemplate("BlockedExit",this))

NOTE: As the script is defined in the *script* attribute of the *exit*, we use the "this" keyword to reference the current *exit* object

NOTE: This a [hard-coded function](/functions/hardcoded).

## EndsWith

EndsWith (string input, string ending)

Returns a [boolean](/types#boolean) - **true** if the input string finishes with the ending string.

## FormatList

FormatList (stringlist or object list, string joiner, string lastjoiner, string nothing)

Returns a [string](/types#string), listing the entries in the given list. For an object list, the GetDisplayName function is used to get an appropriate string. The last two entries in the list are separated by `lastjoiner`, whilst other entries are separated by `joiner`. If the list is empty the string string in `nothing` is returned.

    list = Split("one;two;three", ";") 
    msg(FormatList(list, ",", "or", "nothing"))
    // "one, two or three"
    msg(FormatList(list, ";", "; and", "nothing"))
    // "one; two; and three"
    list = NewStringList()
    msg(FormatList(list, ",", "and", "nothing"))
    // "nothing"
  

## GetMatchStrength

GetMatchStrength (string regex, string input)

There is also an optional cache ID parameter:

    GetMatchStrength (string regex, string input, string cache ID)

Returns an [int](/types#int) indicating how strongly the given input matches the regular expression.

The strength is defined as the length of the "required" parts of the string, i.e. the total length of the string *minus* the total length of all named groups.

Use a cache ID for improved performance if you repeatedly test strings against the same regular expression. The compiled regular expression will be cached and used again for subsequent calls to GetMatchStrength (or [IsRegexMatch](#isregexmatch) or [Populate](/functions/fn-internal#populate) ) using the same cache ID.

For example, given this regex which matches the text "look at " followed by any object name:

     look at (?<object>.*)

An input of "look at dog" has a strength of 8.

This is calculated as follows:

-   The string "look at dog" has a length of 11
-   The named group "object" matches the substring "dog", which has a length of 3
-   The strength therefore is 11 - 3 = 8

The strength is used by the command handling functions in CoreCommands.aslx to determine which command is the best match for a given input.

See also [IsRegexMatch](#isregexmatch), [Populate](/functions/fn-internal#populate)

NOTE: This a [hard-coded function](/functions/hardcoded).

## Instr

Instr (string input, string search)

or

    Instr (int start position, string input, string search)

Returns an [int](/types#int) representing the character position of the search string within the input, or zero if it is not found.

Maps to the VB.net [Instr function](http://msdn.microsoft.com/en-us/library/8460tsh1(VS.80).aspx).

## InstrRev

InstrRev (string input, string search)

or

    InstrRev (int start position, string input, string search)

Returns an [int](/types#int) representing the character position of the search string within the input, starting from the right side of the string.

Maps to the VB.net [InstrRev function](http://msdn.microsoft.com/en-us/library/t2ekk41a%28v=VS.80%29.aspx).

## IsNumeric

IsNumeric (string input)

Returns a [boolean](/types#boolean) - **true** if the input is numeric (i.e. a string which can be converted into a number).

## IsRegexMatch

IsRegexMatch (string regex, string)

There is also an optional cache ID parameter:

    IsRegexMatch (string regex, string, string cache ID)

Returns a [boolean](/types#boolean) - **true** if the string matches the specified regular expression.

Use a cache ID for improved performance if you repeatedly test strings against the same regular expression. The compiled regular expression will be cached and used again for subsequent calls to IsRegexMatch (or [Populate](/functions/fn-internal#populate) or [GetMatchStrength](#getmatchstrength) ) using the same cache ID.

See also [GetMatchStrength](#getmatchstrength), [Populate](/functions/fn-internal#populate)

NOTE: This a [hard-coded function](/functions/hardcoded).

## Join

Join (stringlist input, string split character)

Returns a [string](/types#string) containing each element of the input, separated by the split character. This is the inverse of the [split](#split) function.

## LCase

LCase (string input)

Returns a [string](/types#string) - the lower-case version of the input.

## Left

Left (string input, int character count)

Returns a [string](/types#string) containing characters from the left of the input string.

Maps to the VB.net [Left function](http://msdn.microsoft.com/en-US/library/y050k1wb(v=VS.80).aspx).

## LengthOf

LengthOf (string input)

Returns an [int](/types#int) containing the number of characters in the string.

## LTrim

LTrim (input)

The LTrim function removes spaces on the left side of a [string](/types#string).

## Mid

Mid (string input, int start position)

or

    Mid (string input, int start position, int character count)

Returns a [string](/types#string) containing characters from the middle of the input string. The position counts from 1.

Maps to the VB.net [Mid function](http://msdn.microsoft.com/en-us/library/05e63829(v=VS.90).aspx).

## PadString

PadString (string input, int length, string pad)

Returns a [string](/types#string) that has been padded to the given length with the given padding.

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

## ProcessText

ProcessText (string)

Returns an [string](/types#string) - the result of passing the given string through the text processor. For details on what the text processor does, see [here](/howto/world/text_processor).

## Replace

Replace (string input, string old text, string new text)

Returns a [string](/types#string) where any text matching "old text" in the input has been replaced by "new text".

## ReverseDirection

ReverseDirection (string input)

Returns a [string](/types#string), the reverse direction of the given string, so "northwest" becomes "southeast", and "in" becomes "out" (or the equivalent for the language you areusing). Only works for the full name (not "nw" for example). Unrecognised strings are returned unaltered.

## Right

Right (string input, int character count)

Returns a [string](/types#string) containing characters from the right of the input string.

Maps to the VB.net [Right function](http://msdn.microsoft.com/en-us/library/dxs6hz0a.aspx).

## RTrim

RTrim (input)

The RTrim function removes spaces on the right side of a [string](/types#string).

## SafeXML

SafeXML (string)

Returns an XML encoded version of the string that is safe for output (for example, replacing angle brackets with &amp;lt; and &amp;gt;).

NOTE: This a [hard-coded function](/functions/hardcoded).

## Spaces

Spaces (int)

Returns a [string](/types#string) - a number of spaces equal to the given number. This is useful because HTML will collapse a string of spaces into just one.

## Split

Split (string input, string split character)

Returns a [stringlist](/types#stringlist) where the input has been split into individual strings by the split character. Useful for turning a comma-separated string into a list of strings, for example.

As of version 5.7.2, you can omit the split character, and Quest will assume it is a semicolon.

    Split (string input, string split character)

These two lines are equivalent:

    list = Split("one;two;three;four", ";")
    list = Split("one;two;three;four")

## StartsWith

StartsWith (string input, string start)

Returns a [boolean](/types#boolean) - **true** if the input string begins with the start string.

## Template

Template (string template name)

Returns a [string](/types#string) containing the text for the requested template.

You can create a template in your ASLX file with a [\<template\> element](/elements#template).

See [Changing templates](/howto/world/changing_templates)

NOTE: This a [hard-coded function](/functions/hardcoded).

## ToRoman

ToRoman (int)

Returns a [string](/types#string) - the given number in Roman numerals (i.e., I, II, III...). Good for numbers from 1 to 3999 (the Romans had no zero or negative numbers, this will produce an empty string; for high numbers, it will just add more and more Ms).

## ToWords

ToWords (int)

Returns a [string](/types#string) - the given integer in word form (i.e., one, two...). Numbers outside the range -1999 to 1999 are returned as the digits, but in a string (eg "2000").

This function is part of English.aslx, and is currently only available in English. We would welcome any code for other languages, to be added to later versions of Quest.

## Trim

Trim (input)

The Trim function removes spaces on both sides of a [string](/types#string).

## UCase

UCase (string input)

Returns a [string](/types#string) - the upper-case version of the input.

## WriteVerb

WriteVerb(obj, verb)

Returns the correct form of the verb for the given object, based on the "gender" attribute of the object, together with the object name, capitalised. This allows authors to create responses neutral with respect to the object.

```
WriteVerb (crowd, "be")
-> "A crowd are"
WriteVerb (crowd, "do")
-> "A crowd do"
WriteVerb (crowd, "sit")
-> "A crowd sit"
WriteVerb (dog, "be")
-> "A dog is"
WriteVerb (dog, "do")
-> "A dog does"
WriteVerb (dog, "sit")
-> "A dog sits"
```

See also [Conjugate](#conjugate)
