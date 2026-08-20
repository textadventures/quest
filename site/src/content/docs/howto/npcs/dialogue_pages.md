---
title: Building a conversation with Pages
sidebar:
  order: 5
---

The [Tutorial](/tutorial/using_pages) covers the basics of setting up a Pages dialogue tree. This page covers some more advanced techniques once you've got the hang of it.

## When to use Pages

Quest has several ways to let the player talk to a character - see [Introduction to conversations](/howto/npcs/conversations) for an overview. Pages are the right choice when you want a structured, multi-step exchange where each reply leads to a fixed set of further choices - the Text Adventure equivalent of a gamebook's branching passages. For a one-off list of topics with no follow-up, a [ShowMenu](/functions/user-interface#showmenu)-based menu (see [Handling SPEAK TO](/howto/npcs/speak_to)) is simpler. For a free-form "ask about anything" system, use [Ask/Tell](/howto/npcs/ask_about) instead.

The main practical advantage of Pages over `ShowMenu` is that each choice is a complete, ordinary turn - the game is never "waiting" on a menu callback, so save, load and undo all work in the middle of a conversation.

## The page object

A page is any object that inherits the `dialoguepage` type - normally created via "Add Page" in the tree, which sets this up for you along with the editor's "Page" tab. Pages are invisible and not world-scoped, so they don't clutter "look", "take all" or disambiguation menus, and it doesn't matter where in the tree you put them - directly under the character they belong to, or gathered together under a folder of their own.

Each page has:

- a **description** - the text shown when the page is displayed
- an **options** dictionary - the object name of each linked page, mapped to its link text
- a **page type** - Text (just shows the description), Script (runs a script instead of showing any text or options), or Script + Text (runs a script, then shows the description and options as normal)

## Starting and redirecting

[ShowPage](/functions/user-interface#showpage) starts a dialogue at a given page. Call it from wherever you want the conversation to begin - a verb, a command, an Ask/Tell topic, or a timer:

```quest
ShowPage (bob_chat, true, false)
```

From inside a page's own script (Script or Script + Text page types), you can redirect to a different page with [GoToPage](/functions/user-interface#gotopage) - useful for skipping a page based on game state, the same way you might branch a `SPEAK TO` response:

```quest
if (not chest.locked) {
  GoToPage (bob_chest_open)
}
else {
  GoToPage (bob_chest_locked)
}
```

If a Script or Script + Text page's script doesn't redirect anywhere else, the dialogue simply ends once it's finished running (for a plain Script page, without ever showing that page's own description).

## Ending early

A page with an empty options list ends the dialogue automatically once it's shown. To end things early from partway through a page's own script - for example, some game state means this exchange should be cut short - call [EndPageDialogue](/functions/user-interface#endpagedialogue) directly; it hides the current page's options and clears the dialogue state before the option list would otherwise have been printed.

## Building options dynamically

The editor's Options list is the easiest way to set up a page's links, but you can also add and remove them from a script, which is useful for a conversation that changes shape as the game progresses - for example, unlocking a new topic once the player has found some evidence. [AddPageLink](/functions/user-interface#addpagelink) and [RemovePageLink](/functions/user-interface#removepagelink) do this:

```quest
AddPageLink (bob_chat, bob_lab_report, "Ask about the lab report")
```

Calling `AddPageLink` again for the same source and destination replaces the existing link's text rather than adding a duplicate. To remove an option later:

```quest
RemovePageLink (bob_chat, bob_lab_report)
```

Under the hood, options are stored in the page's `options` attribute, a [stringdictionary](/types#stringdictionary) keyed by the destination page's object name - `AddPageLink`/`RemovePageLink` are just a convenient wrapper around `dictionary add`/`dictionary remove` on that attribute, so you can fall back to those directly for anything more unusual.

## Remembering what's been said

[HasSeenPage](/functions/user-interface#hasseenpage) returns whether a given page has already been shown, based on its `visited` attribute, so you can vary a page's script the second time the player reaches it - see the [Tutorial](/tutorial/using_pages#varying-page-text) for a worked example.

## See also

- [Tutorial: Using Pages](/tutorial/using_pages)
- [Creating a gamebook](/tutorial/creating_a_gamebook), for the equivalent gamebook page mechanism
- [User interface functions](/functions/user-interface) for the full ShowPage/GoToPage/EndPageDialogue/HasSeenPage reference
- [Introduction to conversations](/howto/npcs/conversations)
