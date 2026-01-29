---
layout: index
title: grid_bordersides
---

"grid\_bordersides" is an [int](../types/int.html) attribute. It specifies which sides of the square should be drawn for for this room on the map.

You shouldn't need to set this directly, as there are types set up to make it easier to choose the correct value.

The default (as defined by [defaultobject](defaultobject.html)) draws all four sides of the square. To draw different sides, use one of these types:

gridborder\_path\_ew  
An East-West path (so draw top and bottom)

gridborder\_path\_e  
A path goes East (so draw top, bottom, left)

gridborder\_path\_w  
A path goes West (so draw top, bottom, right)

gridborder\_path\_ns  
A North-South path (so draw left and right)

gridborder\_path\_n  
A path goes North (so draw left, right, bottom)

gridborder\_path\_s  
A path goes South (so draw left, right, top)

The attribute value itself encodes the sides as individual bits (for NESW).
