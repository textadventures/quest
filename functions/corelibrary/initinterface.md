---
layout: index
title: InitInterface
---

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    InitInterface

Does not return a value.

Quest will look for a function called InitInterface in any ASLX file, and if one exists then it will be called when the game begins, and also when loading a saved game.

[Core.aslx](../..//core.aslx.html) defines an implementation of a InitInterface function. It does the following:

-   sets up the default game fonts and colours
-   sets up compass direction names
-   sets titles of panes ("Inventory", "Places and Objects" etc.)
-   shows or hides panes depending on the [showpanes](../../attributes/showpanes.html) option

