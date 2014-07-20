---
layout: index
title: Ask
---

**Note:** This function was deprecated as of Quest 5.1 and redesigned for Quest 5.4.

    Ask (string question)  { script } 

Shows an inline menu of the specified **question** and returns a [boolean](../types/boolean.html) variable **result** with **true** if the player answers "Yes" to the question.

Example:

    Ask ("Are you sure?") {
    <pre>  if (result=true){
        msg ("Yes, you are")
      } 

}

</pre>
Use the [ask](../scripts/ask__script_command_.html) script command for a popup menu.
