---
layout: index
title: Ask
---

**Note:** The original function was deprecated as of Quest 5.1, and this version was introduced in Quest 5.4.

    Ask (string question)  { script } 

Shows an inline menu of the specified **question** and returns a [boolean](../types/boolean.html) variable **result** with **true** if the player answers "Yes" to the question.

Example:

    Ask ("Are you sure?") {
      if (result){
        msg ("Yes, you are")
      } 
    }

Use the [ask](../scripts/ask.html) script command for a popup menu.

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](blocks_and_scripts.html).
