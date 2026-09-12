---
title: "Timers and turnscripts"
sidebar:
  order: 4
---

## DisableTimer
```quest
DisableTimer (timer)
```

Disables the specified timer, by setting its `enabled` attribute to false. See [EnableTimer](#enabletimer) to turn it back on.

## DisableTurnScript
```quest
DisableTurnScript (turn script)
```

Disables the specified turnscript, by setting its `enabled` attribute to false. See [EnableTurnScript](#enableturnscript) to turn it back on.

## EnableTimer
```quest
EnableTimer (timer)
```

Enables the specified timer. Note that this sets the `trigger` attribute as well as setting `enabled` to true.

## EnableTurnScript
```quest
EnableTurnScript (turn script)
```

Enables the specified turnscript, by setting its `enabled` attribute to true. See [DisableTurnScript](#disableturnscript) to turn it off again.

## GetTimer
```quest
GetTimer (string timer name)
```

<a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a>

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

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks-and-scripts).

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

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks-and-scripts).

## SetTimerInterval
```quest
SetTimerInterval (timer, interval)
```

Sets the specified timer's interval, in seconds, between each time it fires - equivalent to setting its `interval` attribute directly.

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

**Note:** The `SetTurnTimeout` function has a script, rather than a block, which means that it is non-blocking and that local variables cannot be accessed inside the script. For a fuller discussion, see the note for [ShowMenu](/reference/functions/user-interface#showmenu).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks-and-scripts).

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

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks-and-scripts).

## SuppressTurnscripts
```quest
SuppressTurnscripts ()
```

Stops all turnscripts running for one turn.

For more on turnscripts, see [here](/howto/scripting/using-turnscripts).
