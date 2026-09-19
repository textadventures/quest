---
title: Troubleshooting
description: Find where an error came from, and fix the most common problems - games that won't load, objects the player can't see, commands that aren't recognised and script errors
---

This page helps you work out why your game isn't doing what you expect, and how to fix it. Start with the symptom:

| What happens | See |
|---|---|
| The game won't start at all | [The game won't load](#the-game-wont-load) |
| "I can't see that." | [I can't see that](#i-cant-see-that) |
| "I don't understand your command." | [Command not recognised](#command-not-recognised) |
| A message starting "Error running script" | [Script error messages](#script-error-messages) |
| Something just happens wrongly, with no error | [Finding where a problem comes from](#finding-where-a-problem-comes-from) |

## Finding where a problem comes from

### Read the error message

When a script goes wrong while the game is running, the player shows "[Sorry, an error occurred]" followed by a line like this:

```
Error running script: Error evaluating expression 'game.count + 1': 'game.count' is null (it has not been set) and cannot be used in this calculation.
```

Most messages have two parts. The text in single quotes after "Error evaluating expression" is the piece of code Quest Viva couldn't work out - here, `game.count + 1`. The part after the colon says what went wrong. [Script error messages](#script-error-messages) below lists the common ones.

To find that code in your game, click "Raw XML code view" on the editor toolbar, press Ctrl+F (Cmd+F on a Mac), and search for the quoted text. The same code may appear in several places, so check each one. If you've only just changed a script, it's often quicker to look there first.

If a game keeps failing, it stops after 20 script errors in one session, so restart it after fixing the problem.

### Use the Debugger

When there's no error but something still goes wrong - a door that won't open, a score that doesn't go up - the Debugger usually shows why. Preview the game, click "Debug", and look at the attributes of the objects involved while you play. You can also change an attribute there to try out a fix. See [Debugging your game](/howto/scripting/debugging-your-game).

### Print what's happening

To see whether a script runs at all, or what a value is at a certain point, add a temporary `msg`:

```quest
msg ("DEBUG: box is in " + box.parent.name + ", count is " + game.count)
```

If you'd rather not clutter the game text, `Log ("some text")` writes to the browser's developer console instead. Remove these lines when you've found the problem.

## The game won't load

If the player can't start the game, it shows "This game couldn't be started" followed by the problem. If the editor can't open it, it opens in Safe Mode, which shows the error above the game's raw XML so you can fix it there.

Most load errors are script typos. The message names the element and the attribute that holds the broken script:

```
Error: Error adding script attribute 'script' to element 'jump': Function not found: 'msg2'
Error: Error adding script attribute 'script' to element 'jump': Missing quote character in msg ("some text)
Error: Error adding script attribute 'script' to element 'jump': Missing '}'
```

- **Function not found** - a line starts with a function name Quest Viva doesn't recognise. Either it's misspelt, or it's a function that returns a value (such as `GetBoolean`) used on a line of its own, where its result isn't used for anything.
- **Missing quote character** - a string is missing its closing `"`.
- **Missing '}'** - a `{` has no matching `}`, for example at the end of an `if` block.

A missing `)` doesn't stop the game loading. Instead, you get `Error running script: Missing ')'` when that script runs.

```
Error: Cannot add object '': Invalid object name
```

This one usually means an attribute has one of the names Quest Viva reserves for elements in the game file - see [Names to avoid](#names-to-avoid).

## I can't see that

"I can't see that." means the player typed a command that needs an object, and Quest Viva couldn't find a matching object that the player can see. Check that:

- the object is in the same room as the player, or in something the player is carrying - not in another room, or inside a closed container
- the object's "Visible" box on the _Setup_ tab is ticked
- the room isn't dark (see [Handling light and dark](/howto/world/handling-light-and-dark))
- the word the player typed is part of the object's name or alias. Players type all sorts of words, so add the likely ones to "Other names" on the object's _Object_ tab. For example, an object with the alias "red ball" answers to BALL and RED, but not to RUBBER BALL unless you add it.

It can also come from a command of yours whose pattern caught more than you meant. With a pattern of `push #object#`, PUSH ON BUTTON matches, and Quest Viva then looks for an object called "on button". Add the longer wording as another pattern, before the shorter one: `push on #object#; push #object#`. Quest Viva tries the patterns in order, so the other way round, `push #object#` still catches PUSH ON BUTTON first.

## Command not recognised

"I don't understand your command." means nothing matched what the player typed - no built-in command, no command of yours, and no verb. If you've added a command that isn't being recognised:

- check its pattern matches what you're typing, word for word
- if the command is inside a room in the tree, it only works in that room - WAVE in a room's own command gives "I don't understand your command." everywhere else

See [How to use commands](/howto/commands/commands) for more on patterns.

## Script error messages

These are the messages you're most likely to see after "Error running script:", and what to do about them.

```
'[something]' is null (it has not been set) and cannot be used in this calculation.
```

You're doing arithmetic with an attribute that doesn't exist - usually because it's misspelt, or you never gave it a starting value. Set it on the object's _Attributes_ tab, or in the game's start script. See [Null](/howto/scripting/null).

```
Cannot convert this value to a string because it has not been set - check whether an attribute or variable has been assigned a value before using it.
```

The same problem, when printing the value - for example `msg (game.nosuch)`.

```
Error evaluating expression 'foo': Unknown object or variable 'foo'
```

Quest Viva doesn't know what `foo` is. It could be a misspelt object name, a string missing its quotes, or a local variable you haven't set yet in this script. Local variables only last until the end of the script they're set in, so to keep a value between turns, store it in an attribute (such as `game.foo`) instead.

```
Error evaluating expression 'game.nosuch': Object reference not set to an instance of an object.
Error evaluating expression 'game.s': Specified cast is not valid.
```

These come from an `if` whose condition isn't `true` or `false`. The first means the condition was null - usually a missing or misspelt attribute. The second means it was something else, such as a string or a number: check you've written a comparison, like `if (game.s = "hello")`.

```
Error evaluating expression 'msg2("some text")': Unknown function 'msg2'
```

A misspelt function name inside an expression. (On a line of its own, the same mistake stops the game loading - see above.)

```
Expected 1 parameter(s) in script 'msg ("some text", "more text")'
Too many parameters passed to OutputText function - 2 passed, but only 1 expected
```

The wrong number of parameters. The first message is for script commands such as `msg`; the second is for functions.

```
GetBoolean function expected object parameter but was passed 'other text'
```

A parameter of the wrong type - here, a string where an object was needed. Check that object names don't have quotes around them.

```
Error evaluating expression 'msg("some text")': Unknown function 'msg'
Function did not return a value
```

You've used the result of something that doesn't give one, as in `x = msg("some text")`. The first message is for a script command. The second is for a function: it happens when the function reaches its end without a `return`, and only when you use its result - calling the same function on a line of its own is fine. The function has already run by the time you see the error.

```
'hat' has no action called 'flatten'
```

A `do` asked for a script attribute that the object doesn't have. Check the attribute's name, or use `HasScript (hat, "flatten")` to check first.

```
Error evaluating expression '... game.pov.parent ...': Value cannot be null. (Parameter 'obj')
```

This comes in a burst of similar messages after the player moves, and means the player has been moved to `null` - nowhere. It happens when you move the player to an attribute or variable that hasn't been set, such as `MoveObject (player, game.target)` before setting `game.target`. Make sure it's set first.

```
Cannot modify the contents of this list as it is defined by an inherited type. Clone it before attempting to modify.
```

You're changing a list the object inherits from its type, with a line like `list add (hat.displayverbs, "Flatten")`. Give the object its own list first, for example:

```quest
hat.displayverbs = Split("Look at;Take;Flatten", ";")
```

## Other common problems

### A room description appears twice

If a room's "After entering the room" script (on the _Scripts_ tab) moves the player somewhere else, the player sees the room they walked into described, and then the room they were moved to. To turn the player back, don't let them enter at all: on the exit, tick "Run a script (instead of moving the player automatically)" and print a message there instead.

### Names to avoid

Most bad names are caught by the editor, but a few aren't:

- Don't give attributes the names `object`, `command`, `exit`, `turnscript`, `game`, `type` or `elementtype`. These mean something special in the game file, and a game with an attribute called `object` won't load again after you save it. Attributes such as `name`, `parent` and `alias` already have jobs, so only use them for those.
- Don't call local variables `e` or `pi`. They're maths constants, and assigning to them is silently ignored: after `e = game.pov`, `e` is still 2.718... You can use them as attribute names.
- Don't name objects `exit1`, `exit2` and so on. Exits you don't name yourself get those names when the game loads, and a clash can make an exit disappear.
