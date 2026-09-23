---
title: "Gamebook functions"
sidebar:
  order: 16
---

Functions for the Gamebook game type (see [Creating a gamebook](/tutorial/gamebook/)), defined in GamebookCore.aslx. Most of them are only available in gamebooks. `AddPageLink`, `RemovePageLink` and `HasSeenPage` also work with [Pages in a text adventure](/tutorial/using-pages), along with `ShowPage`, which is listed under [user interface functions](/reference/functions/user-interface).

## AddPageLink
```quest
AddPageLink (object source, object destination, string text)
```

Adds an option to **source** linking to **destination**, displayed as **text**. If a link to that destination already exists, it's replaced. See also [RemovePageLink](#removepagelink) and [DoPage](#dopage). There's also an [AddPageLink](/reference/functions/user-interface#addpagelink) for Text Adventure dialogue pages, which works the same way.

## ChangeCounter
```quest
ChangeCounter (string counter name, integer value)
```

Adds **value** to the named counter (a game attribute), treating a not-yet-set counter as starting at 0. **value** can be negative. See also [IncreaseCounter](#increasecounter), [DecreaseCounter](#decreasecounter), [SetCounter](#setcounter).

## DecreaseCounter
```quest
DecreaseCounter (string counter name)
```

Decreases the named counter (a game attribute) by 1, treating a not-yet-set counter as starting at 0 (so the result is -1). See also [IncreaseCounter](#increasecounter), [ChangeCounter](#changecounter).

## DoPage
```quest
DoPage (object page)
```

Displays a gamebook page: prints its picture and description, moves the player to it (for room-type pages), and shows its options. This is the core of the gamebook page-turning mechanism - the Text Adventure equivalent for NPC dialogue trees is [ShowPage](/reference/functions/user-interface#showpage).

## HasSeenPage
```quest
HasSeenPage (object page)
```

Returns a [boolean](/reference/attributes/types#boolean) - **true** if the given page has been visited before (its `visited` attribute), for conditional page text or options based on what the player has already seen.

## IncreaseCounter
```quest
IncreaseCounter (string counter name)
```

Increases the named counter (a game attribute) by 1, treating a not-yet-set counter as starting at 0 (so the result is 1). See also [DecreaseCounter](#decreasecounter), [ChangeCounter](#changecounter).

## MovePlayer
```quest
MovePlayer (object destination)
```

Moves the player to **destination**. Equivalent to `player.parent = destination`.

## RemovePageLink
```quest
RemovePageLink (object source, object destination)
```

Removes the option (if any) on **source** that links to **destination**. See also [AddPageLink](#addpagelink). There's also a [RemovePageLink](/reference/functions/user-interface#removepagelink) for Text Adventure dialogue pages, which works the same way.

## SetCounter
```quest
SetCounter (string counter name, integer value)
```

Sets the named counter (a game attribute) to **value**. See also [IncreaseCounter](#increasecounter), [DecreaseCounter](#decreasecounter), [ChangeCounter](#changecounter).

## SetFlagOff
```quest
SetFlagOff (string flag name)
```

Sets the named flag (a boolean game attribute) to false. See also [SetFlagOn](#setflagon).

## SetFlagOn
```quest
SetFlagOn (string flag name)
```

Sets the named flag (a boolean game attribute) to true. See also [SetFlagOff](#setflagoff).
