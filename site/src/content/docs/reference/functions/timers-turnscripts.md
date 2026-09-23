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

Returns the [timer](/reference/elements#timer) of the specified name. Returns null if the timer doesn't exist.

## Pause
```quest
Pause (interval)
```

Pauses for the given number of seconds, then carries on with the rest of the script.

**Note:** Quest 5.5 stopped supporting this, because pausing tied up a real thread. Quest Viva suspends the script instead, so it works again in games marked as ASL version 600. It still raises an error in a game whose version is 550 to 580; change the game's version to 600 or later to use it, or use [SetTimeout](#settimeout).

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

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see [Saving while a question is waiting](/howto/scripting/asking-the-player#saving-while-a-question-is-waiting).

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

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see [Saving while a question is waiting](/howto/scripting/asking-the-player#saving-while-a-question-is-waiting).

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

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see [Saving while a question is waiting](/howto/scripting/asking-the-player#saving-while-a-question-is-waiting).

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

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see [Saving while a question is waiting](/howto/scripting/asking-the-player#saving-while-a-question-is-waiting).

## SuppressTurnscripts
```quest
SuppressTurnscripts ()
```

Stops all turnscripts running for one turn.

For more on turnscripts, see [here](/howto/time/time-turns-and-timers).
