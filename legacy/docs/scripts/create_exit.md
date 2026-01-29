---
layout: index
title: create exit
---

    create exit (string alias, object from, object to)

or

    create exit (string alias, object from, object to, string type)

or, as of Quest 5.1

    create exit (string name, string alias, object from, object to, string type)

Creates an exit with the specified alias (usually the direction, such as "north") between two objects/rooms.

An initial type can be specified e.g. "northdirection". This will ensure that the correct [alt](../attributes/alt.html) names are applied to compass exits.

    create exit ("northwest", fromRoom, toRoom, "northwestdirection")

You can also specify the object name to use. If not specified, an id will be automatically generated.

    create exit ("exit_to_garden", "northwest", fromRoom, toRoom, "northwestdirection")

It is usually easier to make an exit in the normal way in the editor, but to set it so it is not visible; instead of then creating an exit during game play, you set this exit to be visible.
