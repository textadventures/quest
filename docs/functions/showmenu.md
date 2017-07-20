---
layout: index
title: ShowMenu
---

**Note:** As of Quest 5.5, this function replaces one that was deprecated as of Quest 5.1 to display an inline menu.

    ShowMenu (string caption, stringdictionary or list options, boolean allow ignore)  { script } 

Shows an inline menu of the specified options and returns a [string](../types/string.html) variable **result** containing the user input. If a dictionary of options is passed in, the values are displayed as options, the key is returned. If a list of options is passed in, the list item is returned if a string, or (as of Quest 5.7) the name of the object.

If the "allow ignore" parameter is set to **true**, the player can ignore the menu and interact with other objects. The menu is just closed then. If the "allow ignore" parameter is set to **false**, the player must choose one entry of the menu.

Use the [show menu](../scripts/show_menu.html) script command for a popup menu.

The [Split](string/split.html) function can be useful to quickly get a list of options, whilst [switch](../scripts/switch.html) can be useful for dealing with the result. For example:

    options = Split("Red;Green;Blue;Yellow", ";")
    ShowMenu ("What is your favourite colour?", options, false) {
      switch (result) {
        case ("Red") {
          msg ("You must be very passionate. Or like a teamthat play in red.")
        }
        case ("Yellow") {
          msg ("What a bright, cheerful colour!.")
        }
        case ("Green", "Blue") {
          msg (result + "? Seriously?")
        }
      }
    }

_Prior to Quest 5.7_

Strings used for options cannot contain single or double quotes (the option will not be selectable).

_As of Quest 5.7_

ShowMenu will also take an object list, or a list of objects and strings. If the object has a link colour specified, this will be used. Note that `result` will always be a string, in the case of an object, it will be the object's name.

```
ShowMenu ("Select", ScopeInventory(), true) {
  obj = GetObject(result)
  RemoveObject(obj)
  msg ("You smash the " + obj.name + " to bits.")
}
```


**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](blocks_and_scripts.html).
