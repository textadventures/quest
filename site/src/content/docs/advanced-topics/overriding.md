---
title: Overriding functions
sidebar:
  order: 1
---

A great feature of Quest Viva is that you can create your own version of most of the built-in functions to do what you want. In object-orientate programming, this is called "overriding".

## How to...

So let us suppose we want to override `InitUserInterface`. This is an empty function that is designed to be overridden; all your custom formatting should go in this function.

Click the tree view options button above the tree (next to the "Filter..." box), and select "Show Library Elements". All the functions, commands, etc. will appear in the tree (the ones from libraries will be in grey). Select the one you want to modify (type part of its name in the "Filter..." box to find it quickly).

The banner across the top is because it is in a library, not your main game, and it tells you which one. Click "Copy into your game" in the banner, and a copy of the function will be in your game.

Now you can do with it whatever you like.


## Not only functions
You can also override templates, dynamic templates, types and commands in just the same way (commands can also be overridden by copying the pattern).


## But not all functions...

There are two types of functions in Quest Viva, those written in Quest Viva code, and those written in the underlying code. You cannot override the latter; your new function will just get ignored. These functions do not appear in the list in the left pane. They include script commands, but also some of the more fundamental functions.