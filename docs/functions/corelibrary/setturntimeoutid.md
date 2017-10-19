---
layout: index
title: SetTurnTimeoutID
---

    SetTurnTimeoutID (integer turn count, string name, script)

or

    SetTurnTimeoutID (integer turn count, string name){ script }

Runs the specified script after the specified number of turns.

The name specifies the name of the turnscript to create. The anonymous version of this function is [SetTurnTimeout](setturntimeout.html).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](../../blocks_and_scripts.html).
