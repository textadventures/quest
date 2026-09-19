---
title: Talking to characters
description: Choose how the player talks to your characters - SPEAK TO, Ask/Tell topics, a menu or a Pages dialogue tree - and vary what they say
---

Quest Viva gives you several ways to let the player talk to a character. This page helps you choose between them, shows the essentials of each, and explains how to make characters say different things as the game goes on.

## Choosing an approach

| You want | Use | Good for | Watch out for |
|---|---|---|---|
| One thing to say, perhaps changing with the situation | A "speak to" verb | Minor characters, hints, simple reactions | The player can't choose what to talk about |
| The player to question characters about anything | Ask/Tell topics | Mysteries and investigations, where knowing what to ask is part of the puzzle | Players have to guess topics, and every character needs sensible replies to things they don't know about |
| A single choice from a list | A "speak to" verb that shows a menu | Buying something, a one-off question | The player can't save while the menu is waiting, and each follow-up question means another menu |
| A branching conversation, where each reply leads to more choices | Pages | Longer conversations with several steps | More objects to set up - one page per step |

You can mix these in one game, and even on one character - see [Combining approaches](#combining-approaches). Try to be consistent, though: if some characters answer `ASK ABOUT` and others only `TALK TO`, players won't know which to try.

There is also `SAY`, where the player says something to whoever is listening. Quest Viva doesn't have one built in; the [tutorial](/tutorial/custom-commands) shows how to add a `SAY` command. It's hard to do well beyond a few fixed phrases, because a character would need to respond sensibly to anything the player types.

## Responding to SPEAK TO

Every character already responds to `SPEAK TO` - and to `SPEAK`, `TALK TO` and `TALK`, which all mean the same thing. Until you give them something to say, the response is "He says nothing." (or "She", and so on, depending on the character's type).

To give a character something to say, select them in the tree and go to the _Verbs_ tab. Choose "speak to" in the box at the bottom (you can also type it), and click "Add Verb". Then set the behaviour to "Print a message" and enter the text:

```
> TALK TO BORIS
'Can you help me find the key to this door?' you ask.
'Try the bedroom,' says Boris.
```

For anything more than a fixed message, choose "Run a script" instead. In code, the verb is a `speak` script on the character:

```quest
msg ("'Can you help me find the key to this door?' you ask.")
msg ("'Try the bedroom,' says Boris.")
```

A character that says the same thing every time soon seems wooden - see [Varying what characters say](#varying-what-characters-say).

## Ask/Tell topics

Ask/Tell lets the player choose the subject:

```
> ASK MARY ABOUT DR BLACK
'Me? I know nothing about the murder!'
> TELL MARY ABOUT THE POISON BOTTLE
Mary goes pale.
> TELL MARY TO DANCE
Mary dances a jig.
```

It needs to be turned on first. Select the game in the tree, go to the _Features_ tab and tick "Ask/Tell: players can ask or tell characters about selected topics". Each character then has an _Ask/Tell_ tab with three lists - "Ask about", "Tell about" and "Tell to" (the last one handles orders like `TELL MARY TO DANCE`, `ASK MARY TO DANCE` and `MARY, DANCE`).

To add a topic, type its keywords into the "Add entry key..." box under a list, separated by spaces - for example `dr doctor black murder` - and click "Add". Then enter the script to run when the player asks about it. A topic matches when a word the player types begins with one of its keywords, so `ASK MARY ABOUT BLACK`, `ASK MARY ABOUT DOCTOR BLACK` and `ASK MARY ABOUT THE MURDER` all find the same topic. If several topics match, the one matching the most letters wins. Keep keywords reasonably distinctive: `dr` also matches "drinks".

If nothing matches, the character "does not reply". To say something better, expand the tab's "Advanced" section and fill in "Script to run when asked about an unknown topic" (there's one for each list). The variable `text` holds what the player asked about:

```quest
msg ("Mary shrugs. 'I know nothing about " + text + ".'")
```

For topic lists, `ASK ABOUT` without a character, and other extensions, see [Building an Ask/Tell system](/howto/npcs/ask-about).

## A menu of topics

If you'd rather the player picked from a list, give the character a "speak to" verb with "Run a script" as its behaviour, and show a menu. In the script editor, add "Set a variable or attribute" and choose "player's choice from a menu":

```quest
topics = Split("Where is the key?;Who is the Queen?;How do I beat the troll?", ";")
topic = ShowMenu("Ask Cindy...", topics, true)
switch (topic) {
  case ("Where is the key?") {
    msg ("'Try the bedroom,' says Cindy.")
  }
  case ("Who is the Queen?") {
    msg ("'Just some girl,' says Cindy.")
  }
  case ("How do I beat the troll?") {
    msg ("'Fire stops it regenerating,' says Cindy.")
  }
}
```

The options appear as numbered links, and the player clicks one or types its number. Because the last parameter is `true`, the player can ignore the menu and type another command instead. [Asking the player](/howto/scripting/asking-the-player#menus) covers menus in more detail, including showing different text from what the script checks, and options that are only sometimes available.

This works well for a single question. If an answer should lead to further choices, use Pages instead.

## Dialogue trees with Pages

Pages let you build a conversation as a set of linked steps. Each page is an object with some text to show and a list of options, and each option leads to another page:

```
> TALK TO BOB
Bob rubs his chest. "What do you want to know?"
1: Ask about the defibrillator
2: Ask about his heart attack
```

The player clicks an option or types its number, and the page it leads to is shown with its own options.

To create a page, click "+ Add" on the toolbar and choose "Add Page". On its _Page_ tab, enter the text and add options, each pointing to another page. A page with no options ends the conversation once it's shown. [Using Pages](/tutorial/using-pages) walks through building this conversation step by step.

To start the conversation, give the character a "speak to" verb with "Run a script" as its behaviour, and add "Show a page" from the Pages category:

```quest
ShowPage (bob_chat, true, false)
```

The parameters are:

- the page to start at
- whether the player can leave the conversation. If this is `true` ("Allow the player to leave the dialogue (any other command ends it)" in the editor), typing any other command ends the conversation and runs that command. If it's `false`, the player is told to choose one of the options. The editor leaves this unticked when you add the command, so tick it if you want the player to be able to walk away.
- whether turn scripts run for each choice ("Run turn scripts during the dialogue"). Usually you want this off, so that something like a hunger counter doesn't tick for every line of conversation.

For changing a conversation's options as the game goes on, redirecting to a different page from a script, and more, see [Building a conversation with Pages](/howto/npcs/dialogue-pages).

### Menus, Pages and saving

The main practical difference between a menu and Pages is what happens while the player is deciding:

- While a script waits at `ShowMenu()`, the player can't save the game, because the script is stopped part-way through. The callback form of `ShowMenu` avoids this, at the cost of running your code in a separate block - see [Saving while a question is waiting](/howto/scripting/asking-the-player#saving-while-a-question-is-waiting).
- With Pages, every choice is an ordinary, complete turn. The player can save at any point in the conversation, and `UNDO` steps back one choice at a time.
- A conversation built from menus needs a new menu, nested inside the last one, for every step. With Pages, each step is just another page.

So use a menu for a single choice, and Pages for anything with more than one step.

## Combining approaches

The approaches work together. A common pattern is to use Pages for the main conversation, started by `TALK TO`, and Ask/Tell topics for facts the player might want to check later:

- Bob's "speak to" verb runs `ShowPage (bob_chat, true, false)`.
- Bob's "lab report" topic on the _Ask/Tell_ tab just prints a message.
- Bob's "heart attack" topic starts the conversation part-way through, with `ShowPage (bob_heart, true, false)`.

`ASK BOB ABOUT HIS HEART` then shows the `bob_heart` page and its options, just as if the player had reached it from the start of the conversation.

## Varying what characters say

Real people don't give the same answer to the same question fifteen times. There are three simple ways to vary a response.

**The first time.** For a different response the first time only, add "First time..." from the Scripts category in the script editor. In code:

```quest
firsttime {
  msg ("'Can you help me find the key to this door?' you ask.")
  msg ("'Try the bedroom,' says Boris.")
}
otherwise {
  msg ("'Have you looked in the bedroom yet?' says Boris.")
}
```

**The state of the game.** Use an "If" to check what the player has done:

```quest
if (not bedroom_door.locked) {
  msg ("'I see you finally managed to unlock the door,' says Boris.")
}
else if (Got(key)) {
  msg ("'You found the key, then,' says Boris.")
}
else {
  msg ("'Try the bedroom,' says Boris.")
}
```

Checks like these can be combined - put the `firsttime` block inside the last `else`, for example. Put the most specific checks first, since only the first one that's true runs. To remember something that has no object to check, like whether Boris has told you about the troll, set an attribute - `boris.mentionedtroll = true` - and check it later with `GetBoolean(boris, "mentionedtroll")`, which is `false` if the attribute has never been set.

**The text processor.** For small variations, you don't need a script at all. With "Print a message" as the verb's behaviour, `{once:...}` shows some text only the first time, and `{random:...:...}` picks one of several:

```
{once:The ghost looks startled to be spoken to. }{random:'Boo,' it says.:It moans softly.:It ignores you.}
```

See [The text processor](/howto/world/text-processor) for more.

All of these work in any script, so you can use them in Ask/Tell topics and menu responses too. In a Pages conversation, set a page's type to "Script + Text" to run a script before its text is shown, and use `HasSeenPage` to check whether the player has been there before - see [Using Pages](/tutorial/using-pages#varying-page-text).

## Showing both sides

Decide early whether the output shows what the player says, or only the character's reply:

```
> TALK TO BORIS
'Can you help me find the key to this door?' you ask.
'Try the bedroom,' says Boris.
```

```
> TALK TO BORIS
'Looking for the key?' says Boris. 'Try the bedroom.'
```

Either works, but mixing them across characters reads oddly.

## Characters that move

Conversations are only one part of bringing a character to life. [Characters that move](/howto/npcs/npcs-that-move) shows how to make characters follow the player, patrol a route, wander at random or go somewhere with a purpose.
