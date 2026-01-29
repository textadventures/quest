---
layout: index
title: SetBackgroundOpacity
---

    SetBackgroundOpacity(float value)

Sets the opacity of the background (how transparent it is). This should be a number from 0.0 (completely transparent) to 1.0 (completely opaque). Note that this function only changes the value stored by Quest, and it is necessary to call `SetBackgroundColour` to get Quest to update the UI. If you do not want to change the background colour, you can do this, for example:

```
SetBackgroundOpacity(0.5)
SetBackgroundColour(game.defaultbackground)
```
