---
layout: index
title: SetTimeout
---

    SetTimeout (interval, script)

or

    SetTimeout (interval){ script }

Runs the specified script after the specified time interval.

If you may need to cancel the timer after creation, you can create a named timer using [SetTimeoutID](settimeoutid.html).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](../blocks_and_scripts.html).
