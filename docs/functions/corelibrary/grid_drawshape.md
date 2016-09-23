---
layout: index
title: Grid_DrawShape
---

    Grid_DrawShape (string id, string border, string fill, double opacity)

On the custom grid drawing layer, draws an arbitrary shape. First, specify all points using [Grid\_AddNewShapePoint](grid_addnewshapepoint.html). Then call this function to place the drawing on the grid, with the specified border and fill colour and opacity between 0 and 1. The id is arbitrary - if reused, an existing shape will be replaced with this one.
