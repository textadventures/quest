---
layout: index
title: Undo support
---

Every change to an attribute is internally logged, which makes it easy to go backwards and undo changes on a turn-by-turn basis.

Core.aslx defines the "undo" command, which simply calls the script command "undo". This gives the player unlimited access to the "undo" command, but you could override this in your own games to set certain conditions for undoing. Perhaps you would disable undo in certain locations by setting a certain attribute in the player's parent room, or maybe you would set a limit on the number of times the command can be called. Any of this is easily achievable by creating the appropriate script around the "undo" command.

Every turn happens in the context of a **transaction**. When a turn begins, the previous transaction (if any) is finished and a new one is completed. "Undo" works by reversing the changes made in each transaction, so it will go back one turn at a time.

If your game uses timers, "undo" will also affect changes made during a timer script. When a timer script runs, any changes are appended to the latest turn's transaction. This means that if the player types "undo", all of the changes are reverted, so the effect is to "travel back in time" to the point at which the player originally entered the command.

For example, if at time "t1", the player types "take thing", then the "thing" object is moved to the inventory. 10 seconds later, a timer fires, and "thing" explodes. Now if they player types "undo", they're back at time "t1" again - the "thing" object is unexploded, and it's not in the inventory any more.
