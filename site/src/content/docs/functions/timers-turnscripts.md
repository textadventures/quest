---
title: "Timers and turnscripts"
sidebar:
  order: 4
---

## DisableTimer
```quest
DisableTimer (timer)
```

Disables the specified timer.

## DisableTurnScript
```quest
DisableTurnScript (turn script)
```

Disables the specified turn script.

## EnableTimer
```quest
EnableTimer (timer)
```

Enables the specified timer. Note that this sets the `trigger` attribute as well as setting `enabled` to true.

## EnableTurnScript
```quest
EnableTurnScript (turn script)
```

Enables the specified turn script.

## GetTimer
```quest
GetTimer (string timer name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the [timer](/elements#timer) of the specified name. Returns null if the timer doesn't exist.

## Pause
```quest
Pause (interval)
```

Pauses for the given number of seconds.

The 'Pause' request is not supported for games written for Quest 5.5 or later. Use the 'SetTimeout' function instead.

## SetTimeout
```quest
SetTimeout (interval, script)
```

or

```quest
SetTimeout (interval){ script }
```

Runs the specified script after the specified time interval.

If you may need to cancel the timer after creation, you can create a named timer using [SetTimeoutID](#settimeoutid).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks_and_scripts).

## SetTimeoutID
```quest
SetTimeoutID (integer interval, string name, script)
```

or

```quest
SetTimeoutID (integer interval, string name){ script }
```

Runs the specified script after the specified time interval.

The name specifies the name of the timer to create. The anonymous version of this function is [SetTimeout](#settimeout).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks_and_scripts).

## SetTimerInterval
```quest
SetTimerInterval (timer, interval)
```

Sets the specified timer interval.

## SetTimerScript
```quest
SetTimerScript (timer, script)
```

or

```quest
SetTimerScript (timer){ script }
```

Sets the specified timer script.

## SetTurnScript
```quest
SetTurnScript (turn script, script)
```

or

```quest
SetTurnScript (turn script){ script }
```

Sets the script for the specified turn script.

## SetTurnTimeout
```quest
SetTurnTimeout (turn count, script)
```

or

```quest
SetTurnTimeout (turn count){ script }
```

Runs the specified script after the specified number of turns.

If you may need to cancel the turnscript after creation, you can create a named turnscript using [SetTurnTimeoutID](#setturntimeoutid).

**Note:** The `SetTurnTimeout` function has a script, rather than a block, which means that it is non-blocking and that local variables cannot be accessed inside the script. For a fuller discussion, see the note for [ShowMenu](/functions/user-interface#showmenu).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks_and_scripts).

## SetTurnTimeoutID
```quest
SetTurnTimeoutID (integer turn count, string name, script)
```

or

```quest
SetTurnTimeoutID (integer turn count, string name){ script }
```

Runs the specified script after the specified number of turns.

The name specifies the name of the turnscript to create. The anonymous version of this function is [SetTurnTimeout](#setturntimeout).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks_and_scripts).

## SuppressTurnscripts
```quest
SuppressTurnscripts ()
```

Stops all turnscripts running for one turn.

For more on turnscripts, see [here](/howto/scripting/using_turnscripts).
