---
layout: index
title: ShowMenu
---

**Note:** This function was deprecated as of Quest 5.1 to display an inline menu, and redesigned for Quest 5.4.

    ShowMenu (string caption, stringdictionary or stringlist options, boolean allow ignore)  { script } 

Shows an inline menu of the specified options and returns a [string](../types/string.html) variable **result** containing the user input. If a dictionary of options is passed in, the key is returned. If a list of options is passed in, the list item is returned.

If the "allow ignore" parameter is set to **true**, the player can ignore the menu and interact with other objects. The menu is just closed then. If the "allow ignore" parameter is set to **false**, the player must choose one entry of the menu.

Use the [show menu](../scripts/show_menu.html) script command for a popup menu.
