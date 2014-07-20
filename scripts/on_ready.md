---
layout: index
title: on ready
---

    on ready { script } 

Runs the nested script when any callbacks have finished.

For example, when you use an [ask](ask.html) or [get input](get_input.html) script command, Quest will wait for a response from the player and then run the nested scripts from those commands. However, any other scripts at the same level will run immediately. If you don't want this to happen, use "on ready" to make the script only run after the user has entered a command or responded to the question.

This is used by the Core library so that, for example, a room description is only displayed after any scripts which ask a question in "before enter" have run their nested scripts. This prevents the room description from being displayed while the question is still on-screen.

Generally there should be no need to use this command in your own games, as of course if you want script to run after an "ask", you can just put it inside the "ask" script block.
