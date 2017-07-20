---
layout: index
title: get input
---

    get input {script}

Waits for the user to type some text, then runs the nested script.

The nested script can evaluate the "result" string variable to work with the user's input.

Example:

      msg ("What is your name?")
      get input {
         msg ("Your name is " + result)
      }

This command was added in Quest 5.2.

Foor more information see [here](../asking_a_question.html).
