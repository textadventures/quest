---
title: How to use functions
nav_order: 3
parent: "How To"
---

We looked at functions in the [tutorial](tutorial/more_things_to_do_with_objects.html#Using_Functions), and for a basic understanding, you are recommended looking at that. This document goes into more detail on using and creating your own functions.

Quest has a whole load of functions built in, many of them will be used in your game without you even knowing about it. You can see a full list here:

[Functions](../functions/)

Quest also has "script commands", which in many ways are like functions. One difference is that script commands are all named in lower case, while functions are all in CamelCase.

[Script commands](../scripts/)

Many of these can be accessed through the GUI; for example, when you select the "Print" script in the GUI, that is adding the `msg` script command to your game.


Using Functions
---------------

Many script commands work just like functions, and this applies to them too.

Some functions return a value, some require one or more values. The values it requires are called parameters. For example, the GetBoolean function requires an object and the name of an attribute, and it returns true if that attribute is present and set to true, and false if the attribute is set to false or missing.

In this example, my_object and "flag" are the two parameters, and they go inside brackets. The brackets tell Quest these are the parameters.
```
  return_value = GetBoolean (my_object, "flag")
```
Some functions take parameters, but do not return a value:
```
  LockExit (vault_exit)
```
Sometimes a function has no return value and no parameters:
```
  ClearScreen ()
```
You might wonder why you need the brackets, if there are no parameters. Actually, you do not.
```
  ClearScreen
```
Well, not always. If the function is on a line on its own with nothing else, Quest will handle it fine. Otherwise, you will need the brackets to ensure Quest will realise this is a function and not something else.

The type of the value a function returns and the parameters it needs is called its "type signature", and Quest will complain if you get it wrong.


Custom Functions
----------------

The power of Quest is that it lets you do so much. It has dozens of built-in functions, but you can easily create your own.

So why would you want to use a function? The basic reason is because you want to do the same thing in two or more different places. Let us say you have a futuristic city, and the player can teleport between any of several points. Each time he teleports, you want to charge him, and give a little description. The first time, you want to give a longer description, but you do not know which route that will be. The way to set this up is to have each exit call a function.

To start, right click Object and select "Add Function"; give your function a good name. Like any function in a programming language, you can have parameters and a return value. In the example, the destination is a parameter, so click the parameters "Add" button and type "destination". The rest of it is creating a script as normal. The one extra point is that you can reference the parameters as expressions. Once you have the function set up, you just need to call it. "Call function" is an option under scripts, and when you select it, you can put in your parameters.

### A More Complex Example

As another example, let us say you have a room with a bench and another room with a chair, and you want the player to be able to type SIT, SIT ON BENCH and SIT ON CHAIR. One way would be to have a function that takes the seat as a parameter. You can set up a SIT command in each room that sends it the right seat depending on the room, and you can create SIT ON verbs for the chair and the bench that use the Sit function.

So how do we create a function? Right-click in the left pane, and select "Add function". Your new function will appear. Set the return type; in this case the function will not return anything, so we can leave it as "None". Then you can add the parameters.

You can use any names you like here; there is no need for them to correspond to the names in the commands or verbs - however the order IS important! In this case there is only one parameter, and I am going to call it "seat".

Then put in your script. In this example, all it does is print a message. Whatever values are sent as parameters will automatically go into the variables in the same order. In this function we are expecting to be sent some kind of seat, and whatever it is will be held in the "seat" variable. If this function is called by the "siton" verb of the bench, then the "seat" variable will contain the bench object.


### Assumptions and Unexpected Values

You need to think about what will happen if the function is sent the wrong sort of variable. Or more specifically, what assumptions are you making about the values the function receives?

In the example we are expecting an object, we are expecting it to be something that can be sat on and we are expecting it to be there. You might assume it has a certain attribute, such as "alias" (I used the GetDisplayAlias function to avoid that assumption). It is important to think carefully about those assumptions - and I would recommend putting a comment in your code so they are explicitly stated. Just because we called it seat is no guarantee it really is a seat.

If we cannot be sure, then it may be a good idea to test what the thing is before doing anything else; otherwise you can end up with some obscure bugs. In this case, the assumptions are fair. There are four places the function is called, and in each case we can be sure the assumptions are sound. No need to do unnecessary testing.


### Validation

Once you have written your function, it is very important to test it. Go to the start script of the game object, and add some lines of code that will call your new function with a variety of parameters, and check it does what you expect. This can take some time to do properly, but can save hours later when you find a bug but have no idea where it is.

Obviously you need to remove the test lines once the function is validated. It might be best to comment them out so if it changes later, you can test again.

For how to do proper unit testing, see [Unit Testing](../unit_testing.html).


### Returning Values

A function can return a value. You need to specify what type the returned value will be (boolean, string, object, etc.). In the script, you set the return value by calling the `Return` function.

Be aware that the script will continue to run even after the `Return`. It only sets the result of the function; it does not stop the script from executing.

For more detail on creating functions with return values, see [Creating Functions](../creating_functions_which_return_a_value.html).


### Overriding Functions

Did I mention that the power of Quest is that it lets you do so much? Not only can you create your own functions, you can replace the existing ones (though not the script commands or some of the more fundamental functions). In the off-line editor anyway.

Let us suppose you want you want room descriptions to have some novel formatting (perhaps the letter 'A' in blue, to pick a common example). Click on "Filter" at the bottom left of the screen, and select "Show Library Elements". In the left pane you will see all the built in functions, commands and so in, all in grey. Find the one you want, in this case ShowRoomDescription (at the top of the pane is a filtering function, which makes the search easier). The function will appear in the right pane. You can click the "Copy" button at top left to get the function copied into your game, and then edit it as you see fit.

Just do not change the name, return type or parameters.

Of course, you can really mess Quest up by overriding functions, so some care is required...
