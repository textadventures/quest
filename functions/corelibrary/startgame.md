---
layout: index
title: StartGame
---

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    StartGame

Does not return a value.

Quest will look for a function called StartGame, and if one exists then it will be called when the game begins, except if the game is being loaded from a .quest-save file.

[Core.aslx](../..//core.aslx.html) defines an implementation of a StartGame function. It does the following:

-   updates status attributes
-   if the [game](../../elements/game_element.html) object has a "start" script attribute, runs that
-   displays the initial room description

