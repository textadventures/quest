---
title: Using Pages
sidebar:
  order: 10
---

Bob is alive, and thanks to Ask/Tell he'll tell you about his heart attack if you ask him directly. But Ask/Tell only works if the player already knows what to ask about. Sometimes you want to offer the player a menu of things to say, and have Bob's replies lead on to further choices - a proper branching conversation.

You could build this with `ShowMenu` (see [Handling SPEAK TO](/howto/npcs/speak_to)), but a menu-based conversation has a drawback: while the menu is open, the game is waiting on that one callback, so the player can't save or undo until they've picked an option. **Pages** solve this by turning every choice into a normal, complete turn - the same mechanism gamebooks use for their branching passages (see [Creating a gamebook](/tutorial/creating_a_gamebook)), but usable in a Text Adventure room. Nothing is "pending" between choices, so save, load and undo all work mid-conversation.

## Creating a page

A page is a special kind of object: instead of a room description, it has some text to show the player and a list of options leading to other pages.

Right-click the tree (or use "+ Add") and choose "Add Page". Call it `bob_chat`. On its "Page" tab, leave "Page type" set to "Text", and enter a description like `Bob rubs his chest gingerly. "What do you want to know?" he asks.`

Now add some options. In the "Options" list, click "Add", and when prompted for the page name enter `bob_defib` - this creates a new page for you - and for the link text enter "Ask about the defibrillator". Add a second option pointing to a new page called `bob_heart`, with the link text "Ask about his heart attack".

Now fill in the two pages you just created:

-   **bob_defib**: a description like `"That thing? No idea how it works, but I'm glad you had it handy," he says.` Leave its options list empty.
-   **bob_heart**: a description like `"One moment I was enjoying a cheeseburger, the next everything went dark. Then you showed up," he says.` Add one option, back to `bob_chat`, with the link text "Ask something else".

A page with no options automatically ends the conversation once it's shown - that's why `bob_defib` doesn't need anything special to close things off. `bob_heart` instead loops back round to `bob_chat`, so the player can keep asking things.

## Starting the conversation

Pages need something to kick them off. Go to Bob's Verbs tab and add a "speak" verb (Quest will match `TALK TO BOB` and `SPEAK TO BOB` to it - see [Handling SPEAK TO](/howto/npcs/speak_to) for more on this). For its script, switch to Code View and enter:

```quest
ShowPage (bob_chat, true, false)
```

The three parameters are: the page to start at; `allowCancel`, which if true means typing anything other than an option ends the conversation and runs normally (if false, the player is told to pick an option); and `runTurnScripts`, which controls whether turn scripts fire for each choice made - normally you want this off, so a hunger daemon or similar doesn't tick on every line of dialogue.

## Trying it out

Launch the game and type `TALK TO BOB`. You'll see Bob's greeting, followed by a numbered list of options - you can either type the number or click the link. Follow the "heart attack" branch a couple of times, then ask about the defibrillator to end the conversation. Try saving mid-conversation, then loading again - you'll find yourself right back in the chat where you left off.

## Varying page text

As with the gamebook page type, you can check whether a page has already been shown to the player with `HasSeenPage`, so a returning visit doesn't just repeat itself word for word. As an exercise, set `bob_heart`'s "Page type" to "Script + Text" - this adds a script that runs just before its description is shown - and enter:

```quest
if (HasSeenPage (bob_heart)) {
  msg ("Bob sighs, clearly expecting the question this time.")
}
```

Ask about his heart attack twice in a row, and you'll see the extra remark appear on the second visit, on top of the usual description underneath.

For more on building larger dialogue trees - including how to add and remove options while the game is running - see [Building a conversation with Pages](/howto/npcs/dialogue_pages).
