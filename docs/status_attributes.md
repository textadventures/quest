---
layout: index
title: Status Attributes
---

To display a value such as "strength" in the panes to the right of the screen, use a Status Attribute.

To add a status attribute, first set it up as an ordinary attribute of either the game or the player object. Then, add its name to one of the string dictionary attributes "game.statusattributes" or "player.statusattributes" as appropriate (in the Editor, you can do this from the Attributes tab).

The key in the dictionary is the name of the attribute. The value can be left blank, or you can set it to a format string like "your strength is !".

![](Status.jpg "Status.jpg")

The status attributes "health" and "score" are build-in attributes. You can activate them in the game options tab.

Core.aslx updates the status attributes using an UpdateStatusAttributes function. It populates a string and then sends it to the UI using a SetStatus [request](scripts/request.html).
