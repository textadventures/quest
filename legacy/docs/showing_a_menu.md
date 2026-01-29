---
layout: index
title: Showing a menu
---

There are two ways to show a menu. To have a dialogue box pop-up, use the [show menu](scripts/show_menu.html) script command. If you prefer to have an in-line menu with hyperlinks, use the [ShowMenu](functions/showmenu.html) function. How you implement them is virtually the same.

First, you need to create a string list of options - see [Using Lists](using_lists.html). Then call the "show menu" command or "ShowMenu" function to display the list to the user and run a nested script after the user has made their selection.

Here is an example how to create a menu (in this case a pop-up menu). A new list is created, and then the entries 'female' and 'male' are added. If the player chooses an entry from the menu, that value goes into a variable called "result", and from that the variables playername and gender are set.

![](images/ShowMenu.png "ShowMenu.png")

### Further reading

For technical details, see:

- [ShowMenu](functions/showmenu.html)
- [show menu](scripts/show_menu.html)

The `Split` function is often useful when setting options for a menu.

- [Split](functions/split.html)

The `select` script is often useful when handling the result.

- [select](multiple_choices___using_a_switch_script.html)

