---
title: Showing a menu
sidebar:
  order: 22
---

A menu is shown in the transcript as a numbered list of links - the player can click one or type its number. There are two ways to show one: the [ShowMenu](/reference/functions/user-interface#showmenu) function, which hands the chosen option straight back so the rest of your script carries on below it, and the older [show menu](/scripts#show-menu) script command, which runs a nested script instead. How you implement them is virtually the same.

First, you need to create a string list of options - see [Using Lists](/howto/scripting/using-lists). Then call the "show menu" command or "ShowMenu" function to display the list to the user and run a nested script after the user has made their selection.

Here is an example of how to create a menu. A new list is created, and then the entries 'female' and 'male' are added. If the player chooses an entry from the menu, that value goes into a variable called "result", and from that the variables playername and gender are set.

![](/images/ShowMenu.png)

### Further reading

For technical details, see:

- [ShowMenu](/reference/functions/user-interface#showmenu)
- [show menu](/scripts#show-menu)

The `Split` function is often useful when setting options for a menu.

The `switch` script is often useful when handling the result.

- [switch](/howto/tasks/multiple-choices-using-a-switch-script)

