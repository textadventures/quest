---
layout: index
title: Overriding functions
---


<div class="alert alert-info">
Note: Functions can currently only be overridden in the Windows desktop version of Quest.

</div>

A great feature of Quest is that you can create your own version of most of the built-in functions to do what you want. In object-orientate programming, this is called "overriding".



How to...
---------

So let us suppose we want to override `InitUserInterface`. This is an empty function that is designed to be overridden; all your custom formatting should go in this function.

At the bottom left of the editor, click on "Filter"and select "Show Library Elements". All the functions, commands, etc. will appear in the pane above (the ones from libraries will be in grey). Select the one you want to modify (type part of its name in the search box at the top to find it quickly).

The yellow strip across the top is because it in a library, not your main game, and it tells you which one. To the right of that is a button, "Copy". Click that and a copy of the game will be in your game.

Now you can do with it whatever you like.


Not Only Functions
------------------
You can also override templates, dynamic templates, types and commands in just the same way (commands can also be overridden by copying the pattern, so even on-line users can override commands).


But Not All Functions...
------------------------

There are two types of functions in Quest, those written in Quest code, and those written in the underlying code. You cannot override the latter; your new function will just get ignored. These functions do not appear in the list in the left pane. They include script commands, but also some of the more fundamental functions.