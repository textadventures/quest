---
title: Hints
description: Add a HINT command that gives gradually stronger hints for the puzzle the player is stuck on, and a menu of hint questions
---

A puzzle that seemed obvious to you can stop a player completely. A hint system lets them get unstuck without looking up a full walkthrough, and without seeing the answers to puzzles they haven't reached yet. This page shows you how to add one.

| You want | Use |
|---|---|
| `HINT` to help with whatever the player is stuck on now, a little more each time | [Hints for each stage](#hints-for-each-stage) |
| Hints that apply only in one room | [Hints for a room](#hints-for-a-room) |
| The player to pick which puzzle they want help with | [A menu of hint questions](#a-menu-of-hint-questions) |

Quest Viva doesn't have a `HINT` command built in, so you add your own. Core's `HELP` command explains how to play in general, and only matches `HELP` or `?` on its own, so a `HINT` command doesn't clash with it.

## Hints for each stage

Most games move through stages: first the player has to get through the door, then past the bugbear, and so on. The idea is to keep the hints for each stage in a list, and keep track of which stage the player is on. Each time the player types `HINT`, they get the next hint for that stage, starting vague and ending with the answer:

```
> hint
Hint 1 of 3: Have you looked carefully at everything in the room?

> hint
Hint 2 of 3: What might be under the mat?

> hint
Hint 3 of 3: LOOK UNDER MAT, then UNLOCK DOOR.
```

### Setting up the hints

Put each stage's hints on the object that the puzzle is about - the door, the bugbear - or on the room where it happens. On that object's _Attributes_ tab, add an attribute called "hints", set its type to "String List", and add the hints in order, from the gentlest nudge to the full answer.

Then tell the game which stage the player starts on. On the game's _Scripts_ tab, add this to the "Start script" - in the editor, use "Set a variable or attribute", with `game.hintstage` on the left and `door` on the right:

```quest
game.hintstage = door
```

### Moving to the next stage

When the player solves a puzzle, set `game.hintstage` to the next one, in the same script that solves it. For example, in the door's "unlock" script:

```quest
msg ("You unlock the door.")
UnlockExit (door exit)
game.hintstage = bugbear
```

From then on, `HINT` gives the bugbear's hints. That one line is all you need at each point where the game moves on. When there's nothing left to give hints for, set `game.hintstage = null`.

If the order of your game isn't fixed, set `game.hintstage` to whichever puzzle is most useful for the player to work on next - it's just an attribute, so you can change it whenever you like.

### The HINT command

Click "Add Command", and in "Pattern" type `hint;hints;clue`. Give it this script:

```quest
SuppressTurnscripts
stage = GetAttribute(game, "hintstage")
if (stage = null) {
  msg ("There are no hints right now.")
}
else {
  total = ListCount(stage.hints)
  given = GetInt(stage, "hintsgiven")
  if (given < total) {
    given = given + 1
    stage.hintsgiven = given
  }
  msg ("Hint " + given + " of " + total + ": " + StringListItem(stage.hints, given - 1))
}
```

Here's how it works:

- `SuppressTurnscripts` stops asking for a hint from counting as a turn, just like Core's `HELP` - see [Commands that shouldn't take a turn](/howto/scripting/using-turnscripts#commands-that-shouldnt-take-a-turn).
- `GetAttribute` gets the current stage, or `null` if the game hasn't set one, or has set it back to `null`.
- `hintsgiven` counts how many hints the player has seen for this stage. `GetInt` gives 0 if it hasn't been set yet. It's stored on the stage object, so each stage has its own count.
- `StringListItem` gets a hint from the list. The list starts at 0, so hint 1 is item 0.

Once the player has seen every hint for a stage, typing `HINT` again repeats the last one.

Tell the player that `HINT` exists - in the game's introduction, for example - or they may never try it.

## Hints for a room

Some puzzles belong to a room rather than to the main story - a side quest, or an optional treasure. You can give a room its own hints, which the player gets while they're in that room, instead of the hints for the current stage.

Give the room a "hints" string list attribute, just like a stage. Then change the start of the `HINT` command so that it checks the player's room first:

```quest
SuppressTurnscripts
if (HasAttribute(game.pov.parent, "hints")) {
  stage = game.pov.parent
}
else {
  stage = GetAttribute(game, "hintstage")
}
```

The rest of the script stays the same. `game.pov.parent` is the room the player is in.

When the player solves the room's puzzle, remove the room's hints, so that `HINT` goes back to helping with the main story:

```quest
garden.hints = null
```

Setting an attribute to `null` removes it, so `HasAttribute` is then `false`.

## A menu of hint questions

Another approach, which you may know from Infocom's InvisiClues booklets, is a list of questions the player might be asking. The player picks the one they're stuck on, and sees only the answer to that. Use a string dictionary, where each key is a question and each value is its answer, and [`ShowMenu()`](/howto/scripting/asking-the-player#menus) to let the player choose.

1. Select "game" in the tree, and on its _Attributes_ tab add an attribute called "hintmenu". Set its type to "String dictionary".
2. Add an entry for each question - for example "How do I open the door?" with the answer "The key is under the mat."

Then add a command with the pattern `hint;hints;clue`:

```quest
SuppressTurnscripts
questions = NewStringList()
foreach (question, game.hintmenu) {
  list add (questions, question)
}
choice = ShowMenu("What do you need help with?", questions, true)
if (choice <> "") {
  msg (StringDictionaryItem(game.hintmenu, choice))
}
```

The `foreach` makes a list of the questions for the menu. The player sees them as numbered links, and can click one or type its number:

```
> hint
What do you need help with?
1: How do I open the door?
2: How do I get past the bugbear?

> 2
- How do I get past the bugbear?
Bugbears are allergic to jam.
```

The `true` lets the player back out of the menu by typing something else, in which case `ShowMenu` returns an empty string and nothing is shown.

The questions themselves can give things away - "How do I get past the bugbear?" tells the player there's a bugbear. If that matters, only add a question to the dictionary once the player has reached that puzzle, using "Add a value to a dictionary" from the Variables category:

```quest
dictionary add (game.hintmenu, "How do I get past the bugbear?", "Bugbears are allergic to jam.")
```

Only add each question once - a dictionary can't have the same key twice, so a script that could run more than once should check with `DictionaryContains` first.

## Testing your hints

Play through your game, typing `HINT` at every stage, to check that each hint appears when it should. A misspelt object name in a `game.hintstage` line only causes an error when that line runs, so it's easy to miss. A [walkthrough](/howto/scripting/using-walkthroughs) that includes `HINT` at each stage catches this every time you run it.
