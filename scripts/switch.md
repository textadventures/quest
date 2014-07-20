---
layout: index
title: switch
---

    switch (any type value) { case (any type value) { script } [ default { script } ] }

Switch is used with one or more *case* statements and an optional *default* statement. It is used to test a variable or object attribute against 2 or more possible values, it is used as a shortcut instead of writing many *if* statements. The switch statement identifies the variable or attribute to be tested e.g. *switch (**object.name**)*. We can then code for the various possible values for ‘**object.name**’ with ''case(“value”) {**script}** ''statement sets.

If several values should cause the same action we can use this values in on case separated by commas:

     case ( 1, 2 )

or

     case("one", "two", "three")

If we need to handle the situation where no match is found by any of our *case* statements we can use the optional *default {**script**}* statement to end the switch block.

''Example: ''We want to respond differently to the *say \#text\#* command (See tutorial [custom commands](../tutorial/custom_commands.html)) so that the response is dependent on what the user has typed.

     switch (text) {
      case ("hello") {
        msg ("You say hello and hear the echo... hello, ...hello, ...hello")
      }
      case ("help", "hint") {
        msg ("You say help me please!")
      }
      default {
        msg ("You say '" + text + "' but no one hears you")
      }
     } 
