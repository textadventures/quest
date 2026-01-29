---
layout: index
title: DoesInherit
---

    DoesInherit (object object, string type name)

Returns a [boolean](../types/boolean.html) indicating whether the object inherits the specified type.

    if (DoesInherit(o, "male")) {
      msg("'Hi,' he grunts.")
    }
    else if (DoesInherit(o, "female")) {
      msg("'Hi,' she smiles.")
    }
    else {
      msg("It says nothing.")
    }

Note that the types "editor_player", "editor_room" and "editor_object" are removed when you publish your game, so it is a bad idea to test for them.

NOTE: This a [hard-coded function](hardcoded.html).
