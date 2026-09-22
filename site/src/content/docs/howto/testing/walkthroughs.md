---
title: Using walkthroughs
description: Record and replay a play-through, answer menus and questions from a walkthrough, and check the game with assertions
sidebar:
  order: 20
---

A walkthrough is a list of commands the game can play back for you. Record one and you can replay it whenever you like: after changing a puzzle, run the walkthrough to check the game is still winnable, and that the alternative endings still work too.

Walkthroughs are also the quickest way to hand a problem to someone else. If you report a bug, a walkthrough takes whoever looks at it straight to the point where it happens.

You can record and play back walkthroughs in the editor, or play one from inside a running game by opening the [Debugger](/howto/testing/debugging) and choosing the Walkthrough tab. Walkthroughs are stripped out when you publish, so a `.quest` file never carries them.

## Creating and recording a walkthrough

Click the "⋯" button next to *Walkthrough* in the tree and choose **Add Walkthrough** (or use "+ Add" in the toolbar and pick *Walkthrough*). Give it a name that says what it does - "win game", "bad ending", "the cellar puzzle".

The walkthrough editor is a list of steps you can add, edit and reorder by hand, with **Play** and **Record** buttons above it.

**Record** opens the game, replays any steps the walkthrough already has, and then records everything you do from there as new steps. A banner at the top of the player shows "Recording — 3 steps" as you go; click its stop button when you are done, and the steps appear in the editor. Because recording replays what is already there first, you can come back to a walkthrough later and carry on from where it ended.

Recording captures menu choices and yes/no answers as well as commands, so a walkthrough that goes through a conversation replays it correctly.

**Play** runs the steps without recording.

## Creating sub-walkthroughs

Walkthroughs often share their first half - a game with three endings has one route as far as the last room, then three different finishes. Rather than repeating those steps, make one walkthrough a child of another in the tree: when the child runs, it runs all of its parent's steps first.

To do that, click the "⋯" button next to the parent walkthrough and choose **Add Walkthrough here**, or, for one that already exists, click its own "⋯" and choose **Move to…** and pick the parent.

## Answering menus and questions

When the game stops to ask something, an ordinary step won't do - the walkthrough has to say what kind of answer it is.

| The game is waiting at | Answer it with |
|---|---|
| `Ask(...)` or the `ask` script command | `answer:yes` or `answer:no` |
| `ShowMenu(...)` or the `show menu` script command | `menu:` followed by the option's key |
| `ShowMenu (...) { }` or `Ask (...) { }`, the forms with a script block | `event:ShowMenuResponse;` followed by the option's key |
| `GetInput()` | An ordinary step containing whatever the player would type |

`answer:` only understands `yes` and `no`; anything else stops the walkthrough with "Question response was invalid".

`menu:` takes the option's *key*, not the text on screen. When the menu was built from a list, those are the same thing. When it was built from a string dictionary, the key is the dictionary key, and for a menu of objects it is the object's name:

```
take pot
menu:potato
```

If the key isn't one of the options, the walkthrough stops with "Menu response was not an option".

If you forget one of these lines altogether, the walkthrough stops and says so - "No menu response defined in walkthrough", or "Question response not defined in walkthrough". The exception is the block forms, which the walkthrough runner isn't told about: there, a `menu:` step is simply swallowed by the menu, and the walkthrough runs to the end without ever answering it. Use `event:ShowMenuResponse;…` for those - which is what Record produces anyway. (`Ask (...) { }` is a menu underneath, with the keys `[Yes]` and `[No]`.)

## Assertions

A line starting with `assert:` checks an expression that should be true at that point. The walkthrough prints the expression and then "Pass" or "Failed", and stops as soon as one fails.

```xml
<walkthrough name="main">
  <steps>
    look at tin
    open tin
    look at biscuit
    take biscuit
    assert:biscuit.parent = player
  </steps>
</walkthrough>
```

This is what turns a walkthrough from "does it still get to the end?" into a real test. Assert on the things the player can't see: a flag you set, a score, where an object ended up, how many turns something took.

## Comments

A line starting with `label:` is ignored. Use it to mark the sections of a long walkthrough:

```
label: the cellar
open trapdoor
down
```

## Timing and display

`runtime:` prints how long the walkthrough has taken so far. You can use it as often as you like.

`delay:` sets a pause in milliseconds between the steps that follow it, so you can watch what is happening rather than seeing the whole run appear at once. It applies from that line on, and you can change it again later in the same walkthrough.

```xml
<walkthrough name="main">
  <steps>
    look
    get apple
    delay:1000
    examine apple
    north
    look
    delay:200
    examine horse
    use apple with horse
    runtime:
  </steps>
</walkthrough>
```

## See also

- [Debugging your game](/howto/testing/debugging) - inspect and change attributes while the game runs
- [Asking the player](/howto/scripting/asking-the-player) - the question and menu forms the table above refers to
