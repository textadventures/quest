---
layout: index
title: Hard-coded Functions and Library Functions
---

Quest functions fall into two types, hard-coded and library. Generally when you are created a text adventure, you will see no difference, but just occasionally it might be significant.


### Library Functions

Most to the Quest functions are written in Quest's own coding language. These library functions are just like functions you create yourself, the only difference is that the work has been done for you. 

If you are using the desktop version, you can find that code in the various .aslx library files where Quest is installed, and if you click the "Filter" button at the bottom left, and select "Show library elements" you will see a long list of all those functions. You can override these functions, replacing the library version with your own version.

### Hard-coded Functions

There are also several dozen functions that are hard-coded. These are written in a programming language called C#, and then compiled to be part of the software running Quest. The same is true of script commands, and in fact from a technical point of view, these functions are more like script commands, which are also coded in C#, than the library functions.

Hard-coded functions will not be found in .aslx files and will not appear in the list if you show library elements. Quest functions are very fussy about the number of parameters,but C# allows optional parameters, so some hard-coded functions have optional parameters. Others are just as fussy - but will produce different error messages if they do not like the number of parameters you sent them.

Perhaps the most important point to be aware of is that the Quest editor will let you add a function with the same name as a hard-coded function (more easily than when overriding a library function in fact), but when you play the game, it will just get ignored.
