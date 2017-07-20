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


**Note:** The script you give to ShowMenu is in fact a parameter, and you could use it in this manner:

    ShowMenu (string caption, stringdictionary or stringlist options, boolean allow ignore, script scr) 

This is quite different to script commands such as `show menu` or indeed `for` or `if`, where the script is actually a block. An important consequence of this is that local variables will not be available inside the ShowMenu script (while they are inside the `show menu` block). You will not be able to use the `this` variable, nor will you be able to access parameters if this is inside a function, nor will you have access to `object` or `text` (or whatever) inside a command.

Let us suppose this is inside a command, with the pattern "paint #object#". This will fail; Quest will complain: "Unknown object or variable 'object'."

    options = Split("Red;Green;Blue;Yellow", ";")
    ShowMenu ("Paint " + GetDisplayAlias(object) + " what colour?", options, false) {
      msg("You paint " + GetDisplayAlias(object) + " " + LCase(result) + ".")
      object.colour = result
    }

The way to get around this limitation is to set an attribute of the game object before the `ShowMenu`, and to access that inside the script.

```
options = Split("Red;Green;Blue;Yellow", ";")
game.objecttopaint = object
ShowMenu ("Paint " + GetDisplayAlias(object) + " what colour?", options, false) {
  msg ("You paint " + GetDisplayAlias(game.objecttopaint) + " " + LCase(result) + ".")
  game.objecttopaint.colour = result
}
```
