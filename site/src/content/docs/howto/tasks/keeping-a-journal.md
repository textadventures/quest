---
title: Journals and player notes
description: Give the player a journal that the game and the player can both write in, and a REMEMBER command that looks up what the character already knows
---

Long games ask a lot of the player's memory. This page shows two ways to help: a journal that the game writes in when something important happens and the player can add their own notes to, and a `REMEMBER` command that tells the player what their character already knows about the world.

| You want | Use |
|---|---|
| A record of what has happened, which the player can add to | A [journal object](#the-journal) with a string list of entries |
| The player to be able to type notes | [`NOTE` and `-` commands](#letting-the-player-write-notes) |
| The player to look up people, places and things their character knows about | A [`REMEMBER` command](#remembering-and-looking-things-up) that searches a string dictionary |

## The journal

The journal is an ordinary object that the player carries, with a list of entries.

1. Select the player object in the tree and click Add Object in "player". Call the new object "journal".
2. On its _Inventory_ tab, untick "Object can be dropped", so the player always has it. You can type something like "You'd better keep your journal." in "Drop message".
3. On its _Attributes_ tab, add an attribute called "entries" and set its type to "String List". Leave the list empty.

If your game shows object links or the Inventory pane, you may also want to remove "Drop" from the "Inventory verbs" list on the journal's _Object_ tab, so the player isn't offered it.

### Reading the journal

On the journal's _Verbs_ tab, add a "read" verb, set it to "Run a script", and enter:

```quest
if (ListCount(this.entries) = 0) {
  msg ("You haven't written anything in your journal yet.")
}
else {
  msg ("You read your journal:")
  foreach (entry, this.entries) {
    msg ("<i>" + entry + "</i>")
  }
}
```

The player can now type `READ JOURNAL`. Each entry is printed in italics - change the `msg` line if you'd like it to look different.

It's worth adding a shorter `JOURNAL` command too. Click "Add Command", and in "Pattern" type `journal;notes`. For its script, run the journal's "read" verb:

```quest
do (journal, "read")
```

Now there's only one script that reads the journal, so if you change how it looks, you only have to change it once.

### Writing to the journal from the game

Whenever something happens that the player should be able to look up later, add an entry to the list. In the editor, use "Add a value to a list" from the Variables category. In code:

```quest
list add (journal.entries, "The old man said the treasure is buried under the oak tree.")
msg ("(You make a note of that in your journal.)")
```

Where to do this is up to you - in the script that runs when the player talks to a character, finds a clue, or solves a puzzle. Telling the player that the journal has been updated reminds them it's there.

## Letting the player write notes

To let the player add their own notes, add a command with the pattern:

```
note #text#;write #text#;-#text#
```

`#text#` matches anything the player types, and puts it in a variable called `text` (see [Commands](/howto/commands/commands)). The last pattern means that any line starting with a dash is treated as a note, so the player can type `-the guard is lying` without typing a command at all. The script is:

```quest
list add (journal.entries, text)
msg ("You write in your journal: " + text)
```

```
> note the door code might be 1847
You write in your journal: the door code might be 1847

> -Bob is lying about the ring
You write in your journal: Bob is lying about the ring

> journal
You read your journal:
The old man said the treasure is buried under the oak tree.
the door code might be 1847
Bob is lying about the ring
```

If the player types `NOTE` on its own, that command doesn't match - `#text#` needs something to match. Add a second command with the pattern `note;write` that asks what to write, using [`GetInput()`](/howto/scripting/asking-the-player#typed-input):

```quest
msg ("What do you want to write?")
text = Trim(GetInput())
if (text = "") {
  msg ("You decide not to write anything.")
}
else {
  list add (journal.entries, text)
  msg ("You write in your journal: " + text)
}
```

`Trim` removes spaces from each end, so the `if` catches an answer that's empty or only spaces, and doesn't add a blank entry.

Mention these commands somewhere the player will see them, such as the game's introduction - players won't guess that a line starting with a dash is a note.

## Remembering and looking things up

In a game set in a detailed world, the player character knows things the player doesn't. If a character mentions the Weddle-Hoots, the player should be able to find out who they are. A `REMEMBER` command can look topics up in a string dictionary, where each key is the topic and each value is what the character remembers.

1. Select "game" in the tree, and on its _Attributes_ tab add an attribute called "knowledge". Set its type to "String dictionary".
2. Add an entry for each topic. For the key, type the words the player might use for it, separated by semicolons, in lower case - `weddle;hoots`. For the value, type what the player should see - "The Weddle-Hoots are an old aristocratic family, and not to be trusted."

Then add a command with the pattern `remember #text#;recall #text#`, and this script:

```quest
found = false
foreach (key, game.knowledge) {
  foreach (keyword, Split(key, ";")) {
    if (not found and Instr(LCase(text), keyword) > 0) {
      msg (StringDictionaryItem(game.knowledge, key))
      found = true
    }
  }
}
if (not found) {
  msg ("You don't remember anything about " + text + ".")
}
```

The script goes through each topic, and each word in its key, and checks whether that word appears anywhere in what the player typed. The first topic that matches is shown.

```
> remember the Weddle Hoots
The Weddle-Hoots are an old aristocratic family, and not to be trusted.

> recall hoots
The Weddle-Hoots are an old aristocratic family, and not to be trusted.

> remember bananas
You don't remember anything about bananas.
```

Each keyword is matched anywhere in the typed text, so a short keyword like "ring" also matches "string". Choose keywords that are distinctive. A keyword can be a phrase, such as `gold ring`, which only matches when the player types those words together.

The same approach works for any lookup command - just change the pattern and the messages. A science-fiction game might have `wiki #text#`, and a game with complicated commands could add `help #text#` for help topics. Core's `HELP` command only matches `HELP` on its own, so `HELP MAGIC` reaches your command instead.

### Adding topics during the game

You can add topics as the player learns things, for example when they read a book. Use "Add a value to a dictionary" from the Variables category, or in code:

```quest
if (not DictionaryContains(game.knowledge, "dagger;letros")) {
  dictionary add (game.knowledge, "dagger;letros", "According to legend, the Dagger of Letros was used to kill Queen Hef.")
}
```

The `if` stops the script failing if it runs a second time - a dictionary can't have the same key twice.

To change what the player remembers about a topic, remove it and add it again with the new text. The key must be exactly the same as the one you used before:

```quest
dictionary remove (game.knowledge, "weddle;hoots")
dictionary add (game.knowledge, "weddle;hoots", "The Weddle-Hoots have lost their fortune.")
```

Be careful about hiding topics until later in the game. A player who gets "You don't remember anything" once may not think to try again. For things the player discovers, a journal entry is often clearer.

## See also

- [Asking the player](/howto/scripting/asking-the-player) - `GetInput()`, menus and yes/no questions
- [Using lists](/howto/scripting/using-lists) and [Using dictionaries](/howto/scripting/using-dictionaries)
