---
title: Functions
description: Write your own functions with parameters and return values, call them from scripts and expressions, and override the built-in ones
---

A function is a script with a name. Once you have written one, you can run it from anywhere in your game - a command, a verb, a room's entry script, another function - instead of building the same script again in each place. This page covers writing your own functions, giving them parameters and a return value, and replacing the ones that come with Quest Viva.

Quest Viva already has a large number of functions built in, listed in the [function reference](/reference/functions/). It also has [script commands](/scripts/), which look similar but are written in lower case (`msg`, `list add`, `firsttime`) and can't be used in an expression. When you pick "Print" in the script editor, you are adding the `msg` script command.

## When to write one

Write a function when the same work has to happen in more than one place, or when a chunk of script is long enough that the script around it has become hard to read.

| You want | Use |
|---|---|
| The same script in several commands, verbs or rooms | A function |
| A calculation whose answer you need in several places | A function with a return value |
| Different behaviour on each of several objects | A [verb](/howto/commands/using-verbs) or script attribute on each object |
| The same behaviour on every object of a kind | A [type](/advanced-topics/using-inherited-types) with the script on it |

A common case is one action the player can reach several ways. If there is a chair the player can sit on, they might type SIT, or SIT ON CHAIR, or click the chair's "Sit on" verb. Those are three different places in the editor, but they should all do the same thing, so the script goes in one function and each of the three calls it.

## Creating a function

Click **Advanced** at the bottom of the tree and choose **Add Function**. (Once your game has a function, *Functions* appears under Advanced too, and its "⋯" button has **Add Function here**.) The function's tab has four things on it:

- **Name** - by convention each word is capitalised and the spaces are left out, so `SitOnChair`, not `sit on chair`. The name has to be unique across your game and the libraries it uses.
- **Return type** - leave it as *None* unless the function works something out and hands it back. See [Returning a value](#returning-a-value).
- **Parameters** - the values the function is given when it is called. See [Parameters](#parameters).
- **Script** - the script itself, built the same way as any other.

## Calling a function

In the script editor, use **Call function** from the *Scripts* category, pick the function from the list, and fill in the parameters. In code, that is just the function's name followed by its parameters in brackets:

```quest
SitOnChair (chair)
```

A function that takes no parameters can be called with empty brackets or with nothing at all, as long as it is on a line of its own:

```quest
ResetPuzzle ()
ResetPuzzle
```

A function that returns a value can be used anywhere an expression can, including inside another function call, inside a condition, or in the [text processor](/howto/world/text-processor):

```quest
msg ("You have " + ScoreForRank(player.rank) + " points.")
if (ScoreForRank(player.rank) > 100) {
  msg ("Well done.")
}
```

```
Your rank is worth {=ScoreForRank(player.rank)} points.
```

A function whose return type is *None* can't be used like that. Doing so stops the script with "Function did not return a value".

## Parameters

Parameters let one function handle several situations. Add each one to the **Parameters** list on the function's tab, then refer to it in the script by the name you gave it, as though it were a local variable.

```quest
// SitOnChair, with one parameter named "seat"
msg ("You sit on the " + GetDisplayAlias(seat) + ".")
player.sat = true
```

The names in the function have nothing to do with the names at the call site - only the order matters. Whatever is passed first goes into the first parameter, whatever is passed second into the second, and so on. Passing the wrong number stops the script with an error that says so: "Too few parameters passed to … function - only 1 passed, but 2 expected".

So the SIT ON #object# command passes its `object` variable, and a `siton` verb on the chair passes `this`, the object the script is attached to:

```quest
// In the command
SitOnChair (object)
```

```quest
// In the chair's "sit on" verb
SitOnChair (this)
```

That leaves a decision: who checks that the thing really can be sat on? The verb doesn't need to, because it only exists on things that can. The command does, because the player can type SIT ON ANYTHING. Checking in the caller keeps the function simple:

```quest
if (not GetBoolean(object, "cansiton")) {
  msg ("That's not something you can sit on.")
}
else {
  SitOnChair (object)
}
```

Whichever you choose, be clear about what the function assumes - that it is given an object, that the object is present, that it has the attributes the script reads. A comment at the top of the function saying so is worth the two seconds it takes. Calling the parameter `seat` is not a guarantee that it is one.

## Returning a value

Set the function's **Return type** to the kind of value it hands back - string, boolean, integer, double, object, one of the list or dictionary types - and end the script with the `return` script command ("Set this function's return value" in the *Scripts* category).

```quest
// Sittables, return type "object list"
seats = NewObjectList()
foreach (o, ScopeVisible()) {
  if (GetBoolean(o, "cansiton")) {
    list add (seats, o)
  }
}
return (seats)
```

A function can have as many parameters as you like, but only one return value.

`return` also stops the function there and then - nothing after it runs. That is often the point. To find the first object that matches rather than all of them, return as soon as you have it:

```quest
// FirstSittable, return type "object"
foreach (o, ScopeVisible()) {
  if (GetBoolean(o, "cansiton")) {
    return (o)
  }
}
return (null)
```

The last line matters. If every path through the script does not end at a `return`, the call fails with "Function did not return a value". Pick a sensible default - `null` for an object, an empty list for a list, `0` or `-1` for a number - and make sure everywhere that calls the function copes with getting it.

(Games last saved in Quest 5.4 or earlier behave differently: there, `return` sets the value but the script carries on.)

## Variables inside a function

A function's script runs in its own space. Local variables you set before the call are not visible inside it, and local variables the function sets are gone when it finishes:

```quest
seat = chair
SitOnChair (seat)
// Inside SitOnChair, "seat" is only there because it is a parameter
```

`this` is not available either, even when the function was called from an object's script. If the function needs to know which object it is working on, pass it in as a parameter - that is what `SitOnChair (this)` above is doing.

Anything a function needs to share with the rest of the game has to live somewhere that outlives the call: an attribute on an object, or on `game`.

```quest
// Called several times during a turn
game.damagethisturn = game.damagethisturn + n
```

## Recursion

A function can call itself. Each call gets its own copies of the parameters, so this works as you would expect:

```quest
// Factorial, one parameter "n", return type "integer"
if (n <= 1) {
  return (1)
}
return (n * Factorial(n - 1))
```

`Factorial(5)` gives 120. The important part is the first two lines: something has to stop the recursion, or the function calls itself forever and the game gives up with an error. Recursion is the natural fit for walking a tree of objects - a container inside a container inside a room - where you don't know in advance how deep it goes.

## Organising functions into folders

Once your game has more than a handful of functions, you can group them. Click the "⋯" button next to a function in the tree and choose **Move to folder…**. Pick an existing folder, or type a name to create a new one. To take a function back out, choose *(top level)*.

A folder's own "⋯" menu moves the whole folder up or down, and **Add Function here** creates a new function already inside it.

Folders only affect how the tree looks. A function is still called by its name, wherever it is filed.

## Overriding a built-in function

Almost everything Quest Viva does is itself a function in the Core library, and you can replace any of them with your own version. Say you want room descriptions formatted differently: click the tree view options button above the tree and tick **Show Library Elements**, and the library's functions, commands and types appear in grey. Find the one you want - `ShowRoomDescription`, here - using the filter box above the tree. It opens with a banner saying it comes from a library and can't be edited directly; click **Copy into your game** on that banner, and you now have your own copy to edit.

Keep the name, return type and parameters exactly as they were. Everything else in Core still calls the function expecting the original signature, so changing it breaks things a long way from where you changed it. Script commands and a few of the most fundamental functions can't be overridden at all.

Overriding is powerful and easy to get wrong, so change as little as you can get away with, and test it.

## Testing a function

The quickest way to exercise a new function is to call it a few times from the game's start script with different values, print what comes back, and run the game - then take those lines out again. Test the awkward cases as well as the obvious ones: an empty list, a missing attribute, the object that isn't there.

For something you will want to check more than once, a [walkthrough](/howto/scripting/using-walkthroughs) with `assert:` lines does the same job automatically every time you run it, and the [Debugger](/howto/scripting/debugging-your-game) lets you watch attributes change as you play.

## See also

- [Script commands](/scripts/) and the [function reference](/reference/functions/)
- [Introduction to coding](/howto/scripting/introtocoding)
- [Using delegates](/advanced-topics/using-delegates), for storing a script on an object and calling it like a function
