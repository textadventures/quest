---
title: Overriding Core library functions
description: Copy a function, type, template or command out of the Core library into your game so you can change it - and what that costs you later
---

Almost everything Quest Viva does - listing the objects in a room, working out whether the player can reach something, printing "You can't see that" - is written in Quest script in the Core library, in exactly the same language your own game is written in. You can take any of it into your game and change it. That's called **overriding**.

After this page you'll know how to find and copy a library element, what can and can't be overridden, and - the part that matters most - what you're signing up for when you do.

## Before you override

Overriding is the heaviest tool in the box. Check first whether there's a lighter one:

| You want | Try first |
|---|---|
| Different wording for a standard message | [Change the template](/howto/world/changing-templates) - the same mechanism, already documented |
| Something to happen at the start of every game | The game's _Advanced Scripts_ tab - [`inituserinterface`](/howto/scripting/advanced-game-scripts) |
| To handle commands the parser doesn't recognise | `unresolvedcommandhandler`, on the same tab |
| Different behaviour for a group of objects | A [type](/advanced-topics/using-inherited-types) with the behaviour on it |
| Something to run whenever an attribute changes | A [change script](/change-scripts) |

Some Core functions exist purely to be overridden. `GridSquareClick` has nothing in it but a comment telling you to copy it into your game, and `InitUserInterface` is empty for the same reason (though the _Advanced Scripts_ tab is the easier route unless you're writing a library). Replacing those is safe. Replacing a function that does real work, like `ShowRoomDescription`, is a bigger commitment.

## Showing library elements

The library's elements are hidden in the tree by default. To show them:

1. Click the **Tree view options** button above the tree, next to the "Filter..." box.
2. Tick **Show Library Elements**.

The library's functions, commands, types and templates appear in the tree in grey, grouped under the file they came from - `Core.aslx`, `CoreCommands.aslx`, `English.aslx` and so on. Type part of a name into the "Filter..." box to find one quickly.

This is also the fastest way to read Core and see how something actually works, whether or not you intend to change it.

## Copying an element into your game

Select the element you want. A banner appears across the top of the editor:

> This element comes from a library (Core.aslx) and can't be edited directly.  **Copy into your game**

Click **Copy into your game**. The element is now part of your game, the banner disappears, and you can edit it like anything else. The toolbar's **Undo** button reverses the copy if you change your mind.

Keep the name, the parameters and the return type exactly as they were. Everything else in Core still calls the function expecting the original, so changing its shape breaks things a long way from where you changed them. Change as little as you can get away with, and test it.

### Not only functions

Types, templates, dynamic templates and commands are copied the same way. Three are worth a word of their own:

- **Templates** hold every standard message in the game, so they're the ones you're most likely to want. [Changing the game's messages](/howto/world/changing-templates) covers them properly, including varying a response at random.
- **Commands** can also be replaced without copying anything: give a command of your own the same pattern. A command in your game takes precedence over a library command matching the same text, so a new command with the pattern `i;inv;inventory` takes over from Core's `inventory`.
- **The `defaultobject` type** is worth knowing about. Every object in the game inherits it, so copying it into your game and adding an attribute gives that attribute to every object at once - see [Attributes and types](/advanced-topics/about-types#default-attributes-for-every-object).

## What can't be overridden

Not everything Quest Viva does is written in Quest script. Some functions are part of the engine itself, written in C#: `CapFirst`, `ToInt`, `ListCount`, `TypeOf`, `GetBoolean`, `IsRegexMatch` and many others, plus every [script command](/scripts/) (`msg`, `if`, `foreach`, `list add`).

These cannot be overridden. The rule is simple: **if it isn't in the tree with library elements showing, it isn't an element, and you can't replace it.**

Nothing stops you creating a function of your own called `CapFirst`. It will just never run - the engine checks its own built-in functions before it looks at the ones defined in Quest script, so yours is silently ignored, with no error and no warning. If a function you've written seems to have no effect at all, check that its name isn't already taken by the engine.

## What overriding costs you

Core.aslx is not linked to your game while it runs - it is copied into it. When you publish, every function, type, template and command the Core libraries provide is written into the `.quest` file (or the HTML export) alongside your own code. A published game is a snapshot of the library as it stood the day you published, not a pointer to whatever ships with the current engine.

That is deliberate, and it's why games written years ago still play unchanged on a current player, even where a Core function has since been rewritten or dropped.

While you are still editing, though, only the elements you have overridden are frozen. Your game file stores your own copies of those, and loads the rest of Core fresh every time you open it - so fixes and improvements in a new version of Quest Viva reach every function you haven't touched, the next time you save or publish. The moment you click **Copy into your game**, that element stops being one of them. Your copy is yours from then on, bugs and all.

In practice:

- Override the smallest thing that does the job. Copying one short function costs far less than copying the long one that calls it.
- Keep a note of what you've overridden. `Advanced > Functions` in the tree is that list for functions, but there's nothing to remind you *why*, so a comment at the top of your copy is worth the ten seconds.
- When you upgrade to a new version of Quest Viva and something behaves oddly, look at your overrides first: the original may have moved on without them.
- If the change you want could be made by setting an attribute instead, do that.

## See also

- [Creating functions](/howto/scripting/creating-functions-which-return-a-value) - writing your own from scratch
- [Using libraries](/advanced-topics/using-libraries) - packaging your changes for reuse across games
- [Editing the raw XML](/howto/scripting/codeview) - reading Core's source directly
- [Attributes and types](/advanced-topics/about-types) - what `defaultobject` does
