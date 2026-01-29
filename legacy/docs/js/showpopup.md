---
layout: index
title: showPopup/showPopupCustomSize/showPopupFullscreen
---

    JS.showPopup(title, text)

    JS.showPopupCustomSize(title, text, int width, int height)

    JS.showPopupFullscreen(title, text)

Shows a pop up, with an okay button, which the player can click to close. The first version has a fixed width (of 300 px when I checked), and the height will expand up to the full Quest windows size to accommodate the text. The second version allows for custom width and height to be set; scrollbars will be added if the text is too long. The third version will fill the Quest window (so the size will depend on how the player has it set up).

```
JS.showPopup("Hi!", "This is where it all begins")
```
