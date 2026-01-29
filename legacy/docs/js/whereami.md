---
layout: index
title: whereAmI
---

    JS.whereAmI ()

The `whereAmI` JavaScript can be used to determine whether the player is using the desktop version, the web version or the mobile version. The JavaScript code will fire a call back, which will set the attribute game.questplatform to "desktop", "webplayer" or "mobile".

Because this uses a callback, there are a couple of issue to be aware of. Firstly, Quest will not wait for the attribute to be set. It will call the JavaScript, then move straight to the next line of your script, while another part actually does the JavaScript. This means it can be some time (for a computer, but less than a second) before the attribute is set.

Secondly, the callback will appear in any walk-through you record, like this: "event:WhereAmI;desktop". That is not a problem, except if this is used at the start of your game, say in game.start, and that is the very first thing in the walk-through. When you run the walk-through, you are likely to get random bugs, probably because this line fires before Quest is ready. A simple fix is just to delete the line from your walk-throughs.