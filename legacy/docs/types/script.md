---
layout: index
title: Script
---

A script attribute contains code for Quest to run, i.e., a list of  instructions for Quest to carry out. Everything that happens in a game is controlled by script commands. Script commands can print messages, move objects around, show videos, start timers, change attributes, and much more.

Example:

     <look type="script">
       if (not fridge.isopen) {
         msg ("The fridge is open, casting its light out into the gloomy kitchen.")
       }
       else {
         msg ("A big old refrigerator sits in the corner, humming quietly.")
       }
     </look>

     
Scripts can be created by adding script commands using the user interface, or by typing code in "code view". Behind the scenes, it is all the same, so you can flip between the two as you like.

You can use [do](../scripts/do.html) or [invoke](../scripts/invoke.html) to have Quest run a script.

Let us suppose the above script is attached to an object called "fridge". You could run the script:

```
do(fridge, "look")

invoke(fridge.look)
```

If you use the `do` command, your script will have access to a local variable called `this`, which points to the object the script belongs to. This is very useful when making generic scripts; one script can be added to numerous objects, and when the script runs it can find out what it belongs to.

You can send other values to a script by adding them to a dictionary. For each name-value pair you add to the dictionary, a local variable will be available the name being the key, and the value being the value.

```
dict = NewDictionary()
dictionary add (dict, "npc", mary)
dictionary add (dict, "obj", sandwich)
do(fridge, "look", dict)
```

Now the "look" script will have access to local variables called "npc" and "obj", as well as "this". As of Quest 5.8, there is a shortcut to do that:

```
do(fridge, "look", QuickParams("npc", mary, "obj", sandwich))
```

The `QuickParams` function can take either 2, 4 or 6 parameters, allowing you to add 1, 2 or 3 variables.

You can use the `IsDefined` function within a script to determine if it has access to a certain variable. Note that it takes a string.

```
if (IsDefined("npc")) {
```

There is no way to convert a string to a script during play, by the way (though you can do something similar with the [Eval](../functions/eval.html) function).