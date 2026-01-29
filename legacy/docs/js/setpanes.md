---
layout: index
title: setPanes
---

    JS.setPanes (string text, string background)
    JS.setPanes (string text, string background, string text2, string background2)
    JS.setPanes (string text, string background, string text2, string background2, string highlight)

**New in Quest 5.7**

Sets the colours for the panes on the right. You can use either two, four or five parameters.

The text will be in `fore`, whilst the background will be in `back`. When an object is selected, it will be in `secFore` on a `secBack` background, if given, otherwise it will be `back` on a `fore` background (i.e., reversed colours). When a player is clicking on an object, the background will be the `highlight` colour, or orange if not given.

```
JS.setPanes("black", "white")
JS.setPanes("black", "white", "white", "#444")
JS.setPanes("black", "white", "white", "#444", blue)
```