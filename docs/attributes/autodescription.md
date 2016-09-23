---
layout: index
title: autodescription
---

"autodescription" is a [boolean](../types/boolean.html) attribute. If set to true, a room description automatically includes a generated list of the objects and exits within the room.

When autodescription is turned on, the following attributes of the game object configure how the description is displayed:

autodescription\_description  
[int](../types/int.html) Position of room description text within description. Default line 4.

autodescription\_description\_newline  
[boolean](../types/boolean.html) Whether to put an extra linebreak after the room description.

autodescription\_youarein  
[int](../types/int.html) Position of "You are in (room name)" within description. Default line 1.

autodescription\_youarein\_useprefix  
[boolean](../types/boolean.html) Whether to display "You are in" before the room name. Default true. If set to false, print just the room name.

autodescription\_youarein\_newline  
[boolean](../types/boolean.html) Whether to put an extra linebreak after the "You are in (room name)" line.

autodescription\_youcango  
[int](../types/int.html) Position of "You can go (exits)" within description. Default line 3.

autodescription\_youcango\_newline  
[boolean](../types/boolean.html) Whether to put an extra linebreak after the "You can go (exits)" line.

autodescription\_youcansee  
[int](../types/int.html) Position of "You can see (objects)" within description. Default line 2.

autodescription\_youcansee\_newline  
[boolean](../types/boolean.html) Whether to put an extra linebreak after the "You can see (objects)" line.


