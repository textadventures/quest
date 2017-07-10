---
layout: index
title: setPanes
---

    JS.setPanes (string text, string background)
    JS.setPanes (string text, string background, string text2, string background2)
    JS.setPanes (string text, string background, string text2, string background2, string highlight)

**New in Quest 5.7**

Sets the colours for the panes on the right.

The text2 and background2 are used when an item is selected. If not given then text and background are used, but reversed. The highlight is used briefly when the widget is being clicked, and orange will be used by default.
