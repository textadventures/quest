---
layout: index
title: show menu
---

    show menu (string caption, stringdictionary or stringlist options, boolean allow cancel) {script}

Shows a popup menu of options and then runs the nested script. The script can access the variable "result" which contains the result of the user selection - if a dictionary of options is passed in, the key is returned. If a list of options is passed in, the list item is returned.

If the "allow cancel" parameter is set to **true**, the Cancel button is available. If "cancel" is pressed, the variable "result" returns [null](../types/null.html).

This command was added in Quest 5.1.

**example:**

          menulist = NewStringList()
          list add (menulist, "first entry")
          list add (menulist, "second entry")
          list add (menulist, "third entry")
          show menu ("please choose now", menulist, true) {
            msg ("--" + result + "--")
            if (result<>null) {
               msg ("You have chosen the " + result)
            }
            else {
               msg ("You have chosen to press cancel")
            }
          }

<image:Show_menu.jpg>
