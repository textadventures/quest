---
layout: index
title: ShowMenu
---

**Note:** As of Quest 5.5, this function replaces one that was deprecated as of Quest 5.1 to display an inline menu.

    ShowMenu (string caption, stringdictionary or stringlist options, boolean allow ignore)  { script } 

Shows an inline menu of the specified options and returns a [string](../types/string.html) variable **result** containing the user input. If a dictionary of options is passed in, the key is returned. If a list of options is passed in, the list item is returned.

If the "allow ignore" parameter is set to **true**, the player can ignore the menu and interact with other objects. The menu is just closed then. If the "allow ignore" parameter is set to **false**, the player must choose one entry of the menu.

Use the [show menu](../scripts/show_menu.html) script command for a popup menu.

Note that the variable "this" becomes undefined when running the nested script (this is different to the "show menu" script command, when "this" keeps its value inside the nested script).

