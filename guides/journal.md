---
layout: index
title: Journal
---

This is easy to set up. Create a suitably named object (add it to the player so he starts with it in the inventory), and set its description to run a script. In the script, select Print, then expression, and for the expression type something like this: "You open your journal and try to decipher your notes. <i>" + player.jottings + "</i>"

Next, create a new attribute for the player. Call this "jottings" and set it as a string.

Now whenever something salient happens, include a script that will set a variable, and set the variable player.jottings to the expression player.jottings + " Something interesting happened."

If you want, you can let the player add comments too. Go to the Use section of the inventory tab for the journal, and set it for a script. There are three steps to the script. The first is to print a message telling the user to type some content. The next step is to set a variable. Call it new\_text, and set it to the player\_input option from the list. The third step is the same as before; adding the new text to the old. Set the variable player.jottings to the expression player.jottings + " " + new\_text.

[Journal Library](../libraries/journal_library.html)
