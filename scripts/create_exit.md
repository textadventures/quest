---
layout: index
title: create exit
---

    create exit (string name, object from, object to)

or

    create exit (string name, object from, object to, string type)

or, as of Quest 5.1

    create exit (string id, string name, object from, object to, string type)

Creates an exit with the specified name (usually the direction, such as "north") between two objects/rooms.

An initial type can be specified e.g. "northdirection". This will ensure that the correct [alt](../attributes/alt.html) names are applied to compass exits.

You can also specify the object name (id) to use. If not specified, an id will be automatically generated.
