---
title: Game file versions
description: What the version number at the top of a game file means, and what happens when the Quest Viva editor saves a game made with an older version of Quest
sidebar:
  order: 30
---

The first line of every game file says which version of Quest saved it:

```quest
<asl version="600">
```

When a newer version of Quest Viva fixes something in a way that would change how an existing game plays, the fix only applies to games with a high enough version number. Older games keep the behaviour they were written for, so a game made in Quest 5 plays the same way in Quest Viva as it always did. The [changelog](https://github.com/textadventures/quest/blob/main/CHANGELOG.md) lists what has changed in each version.

## Saving upgrades a game

The Quest Viva editor always saves a game with the current version number. So when you open a game made with an older version of Quest and change anything, it's saved with the current version number, and it picks up every fix that's tied to the version number.

Older versions of Quest can't open a game saved with a newer version number, either to edit it or to play it.

So before you edit a game made with an older version of Quest, keep a copy of it in case you need to go back to the original.
