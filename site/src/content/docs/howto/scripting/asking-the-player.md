---
title: Asking the player
description: Ask the player for typed input, a yes/no answer or a choice from a menu, and wait for a key press
---

Sometimes a script needs an answer from the player before it can carry on - their character's name, whether they really want to jump, or which flowers they want to buy. Quest Viva gives you four ways to ask:

| You want | Use | In the editor |
|---|---|---|
| Anything the player types | `GetInput()` | "Set a variable or attribute", then "player's typed input" |
| A yes or no | `Ask(question)` | "If", then "player answers a yes/no question" |
| A choice from a list | `ShowMenu(caption, options, allowCancel)` | "Set a variable or attribute", then "player's choice from a menu" |
| A key press before continuing | `WaitForKeyPress` | "Wait for key press" in the Output category |

Each of these pauses the script until the player responds, then carries on from the next line. In games saved in Quest Viva, questions and menus appear as numbered links in the game text - the player can click one, or type its number.

If you're writing a conversation with a character, also look at [Pages](/tutorial/using-pages), which are usually a better fit for a whole dialogue tree than a series of menus.

## Typed input

```quest
msg ("What is your name?")
player.alias = GetInput()
msg ("Pleased to meet you, " + player.alias + ".")
```

`GetInput()` waits for the player to type something and press Enter, then gives you what they typed as a string. We set the player's `alias` rather than `name` - an object's name can't change during the game, because Quest Viva uses it to keep track of the object. The text processor can then use it anywhere: `'Hi, {player.alias},' says the doll.`

Because the script simply waits, asking several questions is just a matter of asking them one after another:

```quest
msg ("What is your name?")
player.alias = GetInput()
msg ("How old are you?")
player.age = ToInt(GetInput())
```

`ToInt` turns the typed text into a number. If the player might type something that isn't a number, check it first with `IsInt`.

### Checking the answer

To check for a particular answer, like the answer to a riddle, compare what the player typed. Players don't always type exactly what you expect, so it's worth converting to lower case with `LCase` and matching with a [regular expression](/howto/commands/regular-expressions) rather than an exact string:

```quest
msg ("'What walks on four legs in the morning, two in the afternoon, and three in the evening?'")
answer = GetInput()
if (IsRegexMatch("^(a )?(man|woman|person|human)$", LCase(answer))) {
  msg ("'Correct!' says the sphinx, and steps aside.")
}
else {
  msg ("'\"" + answer + "\"? No!'")
}
```

That accepts "man", "A woman", "human" and so on, but not "shaman". The `^` and `$` mean the whole answer has to match, and `(a )?` makes the "a" optional.

## Yes or no

`Ask` shows a question with "Yes" and "No" options, and returns `true` or `false`:

```quest
if (Ask("Are you sure you want to jump into the volcano?")) {
  msg ("You jump. It is very hot.")
  finish
}
else {
  msg ("Very sensible.")
}
```

In the editor, add an "If" and choose "player answers a yes/no question" as the condition.

## Menus

`ShowMenu` shows a list of options and returns the one the player chose. Here, the player is talking to Cindy the flower seller - you'd put this in a "speak to" verb on Cindy:

```quest
options = Split("Red roses;Lavender;Lilies", ";")
choice = ShowMenu("What flowers do you want to buy?", options, true)
switch (choice) {
  case ("Red roses") {
    msg ("You buy some red roses from Cindy.")
    MoveObject (roses, player)
  }
  case ("Lavender") {
    msg ("You buy some lavender from Cindy.")
    MoveObject (lavender, player)
  }
  case ("Lilies") {
    msg ("You buy some lilies from Cindy.")
    MoveObject (lilies, player)
  }
}
```

The third parameter says whether the player can ignore the menu. With `true`, the player can type something else instead of choosing. The menu goes away and `ShowMenu` returns an empty string, which our `switch` doesn't match, so nothing happens. With `false`, the player has to pick one of the options before they can do anything else.

### Showing different text from what you check

In the example above, each option's text has to be typed out twice - once in the list and once in the `switch` - and if they don't match exactly, the choice silently does nothing. To avoid that, use a string dictionary instead of a list. The player sees the values, and `ShowMenu` returns the matching key:

```quest
options = NewStringDictionary()
dictionary add (options, "roses", "Red roses (5 gold)")
dictionary add (options, "lavender", "Lavender (2 gold)")
choice = ShowMenu("What flowers do you want to buy?", options, true)
if (choice = "roses") {
  ...
}
```

Now you can change the text the player sees - to add a price, say - without touching the rest of the script.

### Options that are only sometimes available

Build the list or dictionary with ordinary script, adding an option only when it applies:

```quest
options = Split("Red roses;Lavender;Lilies", ";")
if (GetBoolean(Cindy, "orchidsinstock")) {
  list add (options, "Orchids")
}
choice = ShowMenu("What flowers do you want to buy?", options, true)
```

Give "Orchids" its own `case` in the `switch` as usual. You don't need to check whether they're in stock again, because the player can't choose an option they weren't shown.

## Waiting for a key press

`WaitForKeyPress` pauses until the player presses a key. It's useful for letting the player read something before you clear the screen:

```quest
msg ("And so your adventure begins...")
WaitForKeyPress
ClearScreen
```

## Saving while a question is waiting

While a script is paused at `GetInput()`, `Ask()`, `ShowMenu()` or `WaitForKeyPress`, the player can't save the game. The script is stopped part-way through, and a saved game can't record that. For a quick question that's rarely a problem, but for a menu the player might sit on for a while, it can be.

`ShowMenu` and `Ask` also come in a second form that doesn't have this problem. Instead of returning the answer, you give them a block of script to run once the player has chosen, and the answer is in a variable called `result`:

```quest
ShowMenu ("What flowers do you want to buy?", options, true) {
  switch (result) {
    case ("Red roses") {
      msg ("You buy some red roses from Cindy.")
    }
  }
}
```

This is what the editor adds when you choose "Show a menu" or "Ask a question" from the Output category. The menu is shown and the turn ends straight away, so the player can save while it's on screen, and the block runs when they choose.

The catch is that the script doesn't pause. Everything after the `ShowMenu (...) { }` runs straight away, while the menu is still on screen, and the block runs later, as a separate script:

```quest
ShowMenu ("Paint it what colour?", options, false) {
  msg ("You paint it " + LCase(result) + ".")
}
msg ("(this line is printed before the player has chosen)")
```

Because the block is a separate script, it can't see anything local to the script that showed the menu: not local variables like `options` above, not `this`, not a command's `object` or `text`, and not a function's parameters. Using one stops the script with "Unknown object or variable 'object'". Anything the block needs has to be put somewhere that outlives the turn first - an attribute on `game` or on an object:

```quest
game.objecttopaint = object
ShowMenu ("Paint it what colour?", options, false) {
  msg ("You paint the " + GetDisplayAlias(game.objecttopaint) + " " + LCase(result) + ".")
  game.objecttopaint.colour = result
}
```

For the same reason, asking a second question means nesting a second menu inside the first block, and so on. The `on ready` script command, which waits for outstanding blocks to finish before running its own, doesn't help here - it has no way of knowing that a menu is waiting.

So, as a rule of thumb:

- Use `GetInput()`, `Ask()` and `ShowMenu()` for most questions - the script reads in order and is easy to follow.
- Use the block forms when being able to save really matters, such as a menu that's always on screen at an important moment.
- Use [Pages](/tutorial/using-pages) for conversations with several steps. Every choice is its own turn, so saving and undo work throughout.

## Older games

Games written for Quest 5 often use the `get input`, `ask`, `show menu` and `wait` script commands, which work like the block form above - the answer arrives in `result` inside a nested block. They still work, and you can still edit them, but the editor no longer offers them when you add a new script command.

`GetInput()`, `Ask()` and `ShowMenu()` need a game saved in Quest Viva. In a game still marked as Quest 5.4 to 5.8, they stop with an error that suggests the older command instead. Opening and saving the game in the Quest Viva editor upgrades it.
