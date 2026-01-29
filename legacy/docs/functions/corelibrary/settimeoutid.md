---
layout: index
title: SetTimeoutID
---

    SetTimeoutID (integer interval, string name, script)

or

    SetTimeoutID (integer interval, string name){ script }

Runs the specified script after the specified time interval.

The name specifies the name of the timer to create. The anonymous version of this function is [SetTimeout](settimeout.html).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](../../blocks_and_scripts.html).
