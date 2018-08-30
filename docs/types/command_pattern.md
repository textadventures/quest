---
layout: index
title: Command pattern
---

Quest uses regular expressions to compare commands with what the player typed, and the regular expression is converted from a string in the background (see [here](../pattern_matching.html) for more on that). However, it also offers a simplified version, a "command pattern". This is essentially a string (such as "tie #object1# to #object2"), which Quest will convert to another string when the game start (in this case "^tie (?.*) to (?.*)$"), which can then be converted to a regular expression when required. There is not much point to command patterns outside of commands.
