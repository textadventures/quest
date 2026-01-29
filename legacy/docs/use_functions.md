---
layout: index
title: Use functions
---

If you want to run the same script from different places, you can use functions. Let us say you have a futuristic city, and the player can teleport between any of several points. Each time he teleports, you want to charge him, and give a little description. The first time, you want to give a longer description, but you do not know which route that will be.

The way to set this up is to have each exit call a function.

To start, right click Object and select "Add Function"; give your function a good name. Like any function in a programming language, you can have parameters and a return value. In the example, the destination is a parameter, so click the parameters "Add" button and type "destination".

The rest of it is creating a script as normal. The one extra point is that you can reference the parameters as expressions.

Once you have the function set up, you just need to call it. "Call function" is an option under scripts, and when you select it, you can put in your parameters.

Returning Values
----------------

A function can return a value. You need to specify what type the returned value will be (boolean, string, object, etc.). In the script, you can set the return value by calling the Return function.

Be aware that the script will continue to run even after the Return. The function only sets the result of the function, it does not affect the flow of the script.
