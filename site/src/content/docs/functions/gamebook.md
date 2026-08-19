---
title: "Gamebook functions"
sidebar:
  order: 16
---

Functions specific to the Gamebook game type (see [Creating a gamebook](/tutorial/creating_a_gamebook)), defined in GamebookCore.aslx. These aren't available in ordinary Text Adventure games.

## AddPageLink
```
AddPageLink (object source, object destination, string text)
```

Adds an option to **source** linking to **destination**, displayed as **text**. If a link to that destination already exists, it's replaced. See also [RemovePageLink](#removepagelink) and [DoPage](#dopage).

## ChangeCounter
```
ChangeCounter (string counter name, integer value)
```

Adds **value** to the named counter (a game attribute), treating a not-yet-set counter as starting at 0. **value** can be negative. See also [IncreaseCounter](#increasecounter), [DecreaseCounter](#decreasecounter), [SetCounter](#setcounter).

## DecreaseCounter
```
DecreaseCounter (string counter name)
```

Decreases the named counter (a game attribute) by 1, treating a not-yet-set counter as starting at 0 (so the result is -1). See also [IncreaseCounter](#increasecounter), [ChangeCounter](#changecounter).

## DoPage
```
DoPage (object page)
```

Displays a gamebook page: prints its picture and description, moves the player to it (for room-type pages), and shows its options. This is the core of the gamebook page-turning mechanism - the Text Adventure equivalent for NPC dialogue trees is [ShowPage](/functions/user-interface#showpage).

## HasSeenPage
```
HasSeenPage (object page)
```

Returns a [boolean](/types#boolean) - **true** if the given page has been visited before (its `visited` attribute), for conditional page text or options based on what the player has already seen.

## IncreaseCounter
```
IncreaseCounter (string counter name)
```

Increases the named counter (a game attribute) by 1, treating a not-yet-set counter as starting at 0 (so the result is 1). See also [DecreaseCounter](#decreasecounter), [ChangeCounter](#changecounter).

## MovePlayer
```
MovePlayer (object destination)
```

Moves the player to **destination**. Equivalent to `player.parent = destination`.

## RemovePageLink
```
RemovePageLink (object source, object destination)
```

Removes the option (if any) on **source** that links to **destination**. See also [AddPageLink](#addpagelink).

## RequestSpeak
```
RequestSpeak (string text)
```

Requests that **text** be read aloud, for games with text-to-speech support.

## SetCounter
```
SetCounter (string counter name, integer value)
```

Sets the named counter (a game attribute) to **value**. See also [IncreaseCounter](#increasecounter), [DecreaseCounter](#decreasecounter), [ChangeCounter](#changecounter).

## SetFlagOff
```
SetFlagOff (string flag name)
```

Sets the named flag (a boolean game attribute) to false. See also [SetFlagOn](#setflagon).

## SetFlagOn
```
SetFlagOn (string flag name)
```

Sets the named flag (a boolean game attribute) to true. See also [SetFlagOff](#setflagoff).
