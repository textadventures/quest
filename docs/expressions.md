---
layout: index
title: Expressions
---

Quest handles expressions throughout all script commands. Whereas in previous versions, things like string variables had to be denoted using special characters, in Quest you can use expressions everywhere, just as you can in ordinary programming languages.

Of course, many games will only ever need very simple expressions, and the visual editor will mean that many game authors may never need to write one directly. But, with Quest, the full power of expressions is available everywhere if you need it.

This means no more special characters are required to denote variables and functions, and also the "if" command gets the full power of expressions – you can use nested expressions with "and" and "or", negate expressions with "not", and compare expressions directly. "And" and "or" support short-circuit evaluation; however, all terms must be be evaluable. That means that an idiom like "if (HasAttribute(o, "att") and o.att)" will cause a compile error if o.att is not defined, as the expression must be *able* to be evaluated even if it is not, actually, evaluated.

Here are some example script commands containing expressions:

     foreach (obj, GetObjectsInScope("Visible")) {
       msg ("You can see: " + obj.name)
     }
     
     if ((a and b) or (c and d) or (somenumber * 3 > somethingelse)) { .... }
     
     foreach (obj, somelist + anotherlist + SomeFunctionReturningAList(blah)) {
       ....
     }
     
     MyCustomFunction(a, b, c+d)
     
     player.parent = GetRandomRoom(GetNearbyRooms(player.parent))
     
     someobject.take => {
       msg ("Blah blah")
       someobject.counter = someobject.counter + someotherobject.someproperty
     }
     

Quest also supports local variables. This means you can reuse the same simple variable name inside different functions, if you don’t need to access that value outside the function. You don’t need to declare variables in advance – you can just use them by setting them.

Global variables are now just properties of the "game" object, which means the entire state of the game is always defined entirely by object properties.
