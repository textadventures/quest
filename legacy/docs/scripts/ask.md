---
layout: index
title: ask
---

    ask (string question) {script}

Pops up a prompt for the user to choose Yes or No as the answer to the specified question, and then runs the nested script.

The nested script can check the "result" boolean variable to see the user's response - true for "yes", false for "no".

This command was added in Quest 5.1.

          ask ("Do you want to eat an apple?") {
            if (result) {
                msg("Ahhh, very tasty")
            } else {
                msg("But you should eat your daily apple!")
            }
          }
