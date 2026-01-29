---
layout: index
title: setCss
---

    JS.setCss (string element name, string css styling)

**New in Quest 5.7**

Sets the CSS styling for the given element. If the element name is for an ID, this should be prefixed with a #. CSS styling should be given as a serious of name-value pairs, each pair separated by a semi-colon, with a colon between the name and the value.

This example sets the `<body>` element to have the "serif" font.

```
JS.setCSS ("body", "font-family: serif")
```

This example sets styling for the element with the ID "status" (the strip across the top of the screen). It sets two properties, the background image and background colour.

```
JS.setCSS ("#status", "background-image:none; background-color: green;")
```
