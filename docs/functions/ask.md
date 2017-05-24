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

Note that the variable "this" becomes undefined when running the nested script (this is different to the `ask` script command, when "this" keeps its value inside the nested script).

Also this function is not blocking; Quest will run the script associated with the `Ask` function only when the player makes a section, but all code after that will run immediately. In the code below, the player will see the yes and no options, with "You are in a room." directly below straight away; it will not wait for the question to be answered (again, this is unlike the `ask` script command).

    Ask ("Are you sure?") {
      if (result){
        msg ("Yes, you are")
      } 
    }
    msg ("You are in a room.")

You can also pass the script as a parameter:

    scr => {
      if (result) {
        msg ("Ahhh, very tasty")
      }
      else {
        msg ("But you should eat your daily apple!")
      }
    }
    Ask ("Do you want to eat an apple?", scr)