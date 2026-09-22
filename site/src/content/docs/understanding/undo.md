---
title: Undo support
description: How Quest Viva records each turn so the player can undo it, what undo can't reverse, and how to limit or disable it
sidebar:
  order: 7
---

Every change to an attribute is internally logged, which makes it easy to go backwards and undo changes on a turn-by-turn basis. Players get this for free: every game Quest Viva builds understands the UNDO command, and you don't have to do anything to enable it.

## Transactions

Every turn happens in the context of a **transaction**. When a turn begins, the previous transaction (if any) is finished and a new one is started. The parser names it after the command the player typed. "Undo" works by reversing the changes made in each transaction, so it goes back one turn at a time.

Changes are reversed; output isn't. The text the turn printed stays on the screen, and a new line appears saying what was undone:

```
> take thing
You pick it up.

> undo
Undo: take thing
```

That line comes from the `UndoTurn` dynamic template, and "Nothing to undo!" comes from the `NothingToUndo` template, so you can reword both like any other [message](/howto/text/messages).

## What the player sees

UNDO is a typed command - there's no button for it, and the built-in HELP text doesn't mention it. If undo matters to your game, say so somewhere the player will look.

Two moments print "Nothing to undo!" and surprise people:

- **At the start of a game.** There's no turn yet to go back to.
- **Immediately after loading a saved game.** A saved game records the state of the world, not the history that led to it, so loading starts a fresh undo history. The restored state is intact; it's only the ability to step back past the load that's gone.

## Timers

If your game uses timers, "undo" will also affect changes made during a timer script. When a timer script runs, any changes are appended to the latest turn's transaction. This means that if the player types "undo", all of the changes are reverted, so the effect is to "travel back in time" to the point at which the player originally entered the command.

For example, if at time "t1", the player types "take thing", then the "thing" object is moved to the inventory. 10 seconds later, a timer fires, and "thing" explodes. Now if the player types "undo", they're back at time "t1" again - the "thing" object is unexploded, and it's not in the inventory any more.

## Questions that pause the script

While a script is waiting at `GetInput()`, `Ask()`, `ShowMenu()` or `WaitForKeyPress`, it is stopped part-way through - see [Asking the player](/howto/scripting/asking-the-player). There is no turn boundary to go back to in the middle of a script, so the player can't undo at that point, and what they type goes to the question instead:

- At `GetInput()`, typing UNDO just answers the question with the word "undo".
- At `Ask()`, or a `ShowMenu()` the player can't cancel, UNDO is ignored and the question stays on screen.
- At a `ShowMenu()` the player *can* cancel, UNDO cancels the menu, the same as any other unrecognised input.

This is the same restriction that stops the player saving mid-question. Once the turn has finished, everything that happened in it - including whatever the answer changed - is a single undo point, and one UNDO reverses the lot.

## Pages

A [Pages dialogue](/howto/characters/pages) doesn't have this problem, because each choice runs as a complete turn of its own and starts its own transaction. The game stays saveable throughout, and each choice is a separate undo point.

The player can't type UNDO *during* the dialogue, though: while a dialogue is on screen their input goes to the dialogue, which either treats it as an invalid choice or, if the dialogue can be cancelled, ends it. Once the dialogue has finished, UNDO reverses the last choice - which puts the player back in the dialogue at the previous page, although its option links are not drawn again. Give the player a "Back" option of their own rather than relying on UNDO to return them to an earlier page.

## Adding your own undo points

The `start transaction` script command ends the current transaction and starts a new one, named by the text you give it. It isn't offered when you add a script command in the editor, so add it in [code view](/howto/scripting/raw-xml). This is exactly what Pages uses to make each choice undoable:

```quest
start transaction ("page: " + chosenkey)
```

Use it when one command does several things that the player should be able to step back through one at a time.

## Limiting or disabling undo

The Core library defines the "undo" command, which simply calls the script command "undo" - listed as "Undo" under "Game State" when you add a script command, in the Advanced section. That gives the player unlimited undo. To change it, copy the command into your game and edit it:

1. Click the "Tree view options" button next to the "Filter..." box, and tick "Show Library Elements".
2. Type `undo` in the "Filter..." box and select the `undo` command, which comes from `CoreCommands.aslx`.
3. Click "Copy into your game" in the banner, then edit the script. See [Overriding](/customise/overriding) for more on this.

You could refuse to undo in certain rooms by checking `game.pov.parent`, or limit how many undos the player gets. This version needs an integer attribute `undosleft` on the `game` object, set to however many you want to allow:

```quest
if (game.undosleft > 0) {
  undo
  game.undosleft = game.undosleft - 1
  msg ("(" + game.undosleft + " undos left)")
}
else {
  msg ("You have used up all your undos.")
}
```

Two things are easy to get wrong here:

- **Decrement the counter *after* calling `undo`, not before.** An undo command doesn't start a transaction of its own, so anything it changes beforehand is added to the transaction it is about to reverse - and gets reversed along with it. Set before, `game.undosleft` comes back to the value it had at the start of the turn, and the player gets infinite undos.
- **Leave the command's "isundo" attribute alone.** It's what tells the parser not to open a transaction for this turn. Without it, UNDO becomes an undoable turn in its own right and the player can't get anywhere.

## See also

- [Asking the player](/howto/scripting/asking-the-player) - why a paused script can't be undone or saved
- [Building a conversation with Pages](/howto/characters/pages)
- [undo](/reference/script-commands/#undo) and [start transaction](/reference/script-commands/#start-transaction) in the script command reference
- [Changing the game's messages](/howto/text/messages) - rewording `UndoTurn` and `NothingToUndo`
