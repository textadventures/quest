---
title: JS functions
nav_order: 27
parent: "Reference"
has_children: true
---

The `JS` object is how Quest exposes the user interface. What this means is that we can use the JS object to call JavaScript functions that will modify what the player sees. The basic format is to append the JavaScript function name with a dot, so to call `addText` (the JavaScript function Quest uses to show text on the screen), use something like this:

```
JS.addText("You are in a deep hole.")
```
