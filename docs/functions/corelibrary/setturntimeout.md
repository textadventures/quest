---
layout: index
title: SetTurnTimeout
---

    SetTurnTimeout (turn count, script)

or

    SetTurnTimeout (turn count){ script }

Runs the specified script after the specified number of turns.

If you may need to cancel the turnscript after creation, you can create a named turnscript using [SetTurnTimeoutID](setturntimeoutid.html).

**Note:** The `SetTurnTimeout` function has a script, rather than a block, which means that it is non-blocking and that local variables cannot be accessed inside the script. For a fuller discussion, see the note for [ShowMenu](showmenu.html).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](../../blocks_and_scripts.html).
