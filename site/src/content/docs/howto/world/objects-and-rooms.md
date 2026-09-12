---
title: Objects and rooms
sidebar:
  order: 1
---

Everything in a Quest Viva game is an object, and rooms are objects too. The difference is only how you use them: a room contains things and the player can be inside it, while an ordinary object sits in a room, in a container, or in the player's inventory.

The [tutorial](/tutorial/creating-a-simple-game) walks through creating both. This page is the reference for what each field on the basic tabs actually does.

## The _Setup_ tab

Every object has a _Setup_ tab. It holds the object's identity — what it is called, whether the player can see it, and what they are told when they look at it.

Rooms have one too, but a shorter one: a room is never visible, scenery, male or female, and its description lives on its own _Room_ tab, so only the name, alias and prefix fields appear.

### Name and alias

The **name** is the object's identifier: it must be unique across the whole game, and it is what you use in scripts (`MoveObject (brass_key, player)`). It cannot be changed by the game while it is running.

The **alias** is what the player sees, and it can be anything — spaces, punctuation, capitals — because nothing in your code depends on it. Set an alias whenever the natural name for something isn't a good identifier:

| Name | Alias |
| --- | --- |
| `brass_key` | brass key |
| `oldman` | grizzled old man |
| `note1` | crumpled note |

The player can refer to the object by its alias, or by any of the other names you add on the _Object_ tab. An alias can be changed during the game — a "stranger" who becomes "Mirren" once introduced is just an alias change.

### Visible and scenery

**Visible** controls whether the object exists for the player at all. Untick it and the object is not listed, not mentioned and not reachable — it is still in the room, but as far as the player is concerned it isn't there. Make it visible from a script when it should appear.

**Scenery** is subtler, and more useful than it looks. A scenery object is part of the room rather than an item in it: it is not listed in the room description and does not appear in the objects pane, but the player can still examine it and interact with it. This is how you write a room whose description mentions a fireplace, a portrait and three windows without the player then being shown a list of four things they can pick up.

As a rule: describe it in the room description, mark it as scenery, and let the curious player find it. If the player does manage to take a scenery object, Quest Viva clears the scenery flag automatically.

### Prefix and suffix

Quest Viva writes an object into a room description as something like "a brass key". The **prefix** is what comes before ("a", "an", "some") and the **suffix** is what comes after.

Leave "Use default prefix and suffix" ticked and Quest Viva works the prefix out from the alias. Untick it for anything irregular — a proper name that takes no article at all, an object that is always "the", a plural. The suffix is where you can add a trailing phrase that belongs to the object wherever it is listed:

> There is a brass key **hanging on a hook** here.

### Object type

The dropdown near the bottom of the tab sets whether the object is an inanimate thing, a male or female character, a named character, or a plural. This decides which pronouns Quest Viva uses when it writes about the object — "it", "him", "her", "them" — and how the default prefix comes out. Getting it right saves you writing round the problem in every message.

### Descriptions

The **"Look at" object description** is what the player is shown when they examine the object. It can be plain text, or a script if the description depends on the state of the game. Text descriptions can use the [text processor](/howto/world/text-processor), which is usually simpler than a script:

```quest
A heavy brass key{if door.locked:. It must fit something around here}.
```

The **in-room description** is a separate, optional line that appears only if the "In-room descriptions" [game feature](/howto/world/features#game-features) is enabled. Where the "look at" description is shown when the player examines the object, the in-room description is appended to the description of whatever room the object is currently in:

> You are in a dusty study. **A brass key glints on the desk.**

That makes it worth using for objects whose presence should be written into the prose of the room rather than listed underneath it. It follows the object, so the same sentence appears wherever it is moved to — which means it should be phrased so that it works anywhere, or changed when you move the object.

## The _Room_ tab

Rooms have a _Room_ tab in place of an object's _Object_ tab.

The **description** is the main thing: the text the player sees on arrival, and whenever they type `LOOK`. Like an object description it can be text or a script, and text can use the text processor.

How much you have to write here depends on the "Automatically generate room descriptions" setting on the game's [_Room Descriptions_ tab](/howto/ux/ui-style#the-room-descriptions-tab). With it on — which is the default — Quest Viva adds the list of objects and the list of exits for you, and your description only has to cover the place itself. With it off, everything the player is told is what you wrote.

The other fields are all refinements of that:

- **Room picture** — an image shown in the picture frame while the player is here. Only visible if the picture frame feature is on.
- **Description prefix** — overrides the "You are in" wording for this one room, for places where that reads badly ("You are standing on a narrow ledge").
- **Objects list prefix** — overrides the wording that introduces the object list here, in the same way.
- **Objects dropped here go to** — redirects anything the player drops. Point a clifftop at the beach below, or a room at a "lost property" room the player can't reach, and dropped objects land there instead of at the player's feet. A drop script is passed this destination — see [Taking and dropping objects](/howto/world/taking-and-dropping#drop).

## The _Objects_ tab

The _Objects_ tab lists what is inside this object, and lets you add, edit and delete those children directly.

It is the same containment relationship for every kind of object, which is what makes it worth knowing: the objects in a room, the objects in a box, the objects the player is carrying and the objects sewn into a coat are all the same thing, an object's children. A room's _Objects_ tab shows what is in the room; the player's shows their starting inventory; a container's shows its contents.

The tree in the sidebar shows the same relationship. To move an existing object to a different parent, right-click it in the tree and choose "Move to..."; this tab is where you add new ones.

Whether the player can *see* the children of a non-room object is a separate question, answered by the [_Container_ tab](/howto/world/containers) — an object with children that is not a container or a surface simply keeps them hidden.
