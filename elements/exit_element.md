---
layout: index
title: exit element
---

    <exit alias="direction or displayed exit name" name="name" to="to room">attributes</exit>

Creates an exit from the exit's parent room to the specified room.

The alias might be something like "east", "north", or the name of a room that the player can go to.

The name is optional. If no name is specified, Quest will generate a name for the exit.

Attributes:

alias  
[string](../types/string.html) exit alias

grid\_length  
[int](../types/int.html) length of exit line on map in grid units

grid\_offset\_x  
X offset of exit position on grid

grid\_offset\_y  
Y offset of exit position on grid

grid\_render  
see [grid\_render](../attributes/grid_render.html) object attribute

lightstrength  
see [lightstrength](../attributes/lightstrength.html) object attribute

locked  
[boolean](../types/boolean.html) specifying if exit is locked

lockmessage  
[string](../types/string.html) to display when exit is locked

look  
[string](../types/string.html) description to print when the player looks in this direction, or [script](../types/script.html) to run

lookonly  
[boolean](../types/boolean.html) - if true, the player can't move in this direction, only look

prefix  
[string](../types/string.html) to print before exit name in room descriptions

script  
[script](../types/script.html) to run instead of moving the player

suffix  
[string](../types/string.html) to print after exit name in room descriptions

visible  
[boolean](../types/boolean.html) - if false, exit is not available (as if the exit's parent was null)


