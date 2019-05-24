---
layout: index
title: addScript
---

    JS.addScript (string text)

Inserts the text into the HTML document. This can be used for adding JavaScript or CSS, or for adding HTML that will be out of the normal sequence, such as a custom pane or dialogue panel. Use [addText](addtext.html) for game text.

```
JS.addScript("<script>function setNumber(n) { number = n; }</script>")
```