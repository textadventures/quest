---
title: "Timers and Turnscripts"
sidebar:
  order: 4
---



## DisableTimer

DisableTimer (timer)

Disables the specified timer.

## DisableTurnScript

DisableTurnScript (turn script)

Disables the specified turn script.

## EnableTimer

EnableTimer (timer)

Enables the specified timer. Note that this sets the `trigger` attribute as well as setting `enabled` to true.

## EnableTurnScript

EnableTurnScript (turn script)

Enables the specified turn script.

## GetTimer

GetTimer (string timer name)

Returns the [timer](/elements#timer) of the specified name. Returns null if the timer doesn't exist.

## Pause

Pause (interval)

Pauses for the given number of seconds.

The 'Pause' request is not supported for games written for Quest 5.5 or later. Use the 'SetTimeout' function instead.

## SetTimeout

SetTimeout (interval, script)

or

    SetTimeout (interval){ script }

Runs the specified script after the specified time interval.

If you may need to cancel the timer after creation, you can create a named timer using [SetTimeoutID](#settimeoutid).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks_and_scripts).

## SetTimeoutID

SetTimeoutID (integer interval, string name, script)

or

    SetTimeoutID (integer interval, string name){ script }

Runs the specified script after the specified time interval.

The name specifies the name of the timer to create. The anonymous version of this function is [SetTimeout](#settimeout).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks_and_scripts).

## SetTimerInterval

SetTimerInterval (timer, interval)

Sets the specified timer interval.

## SetTimerScript

SetTimerScript (timer, script)

or

    SetTimerScript (timer){ script }

Sets the specified timer script.

## SetTurnScript

SetTurnScript (turn script, script)

or

    SetTurnScript (turn script){ script }

Sets the script for the specified turn script.

## SetTurnTimeout

SetTurnTimeout (turn count, script)

or

    SetTurnTimeout (turn count){ script }

Runs the specified script after the specified number of turns.

If you may need to cancel the turnscript after creation, you can create a named turnscript using [SetTurnTimeoutID](#setturntimeoutid).

**Note:** The `SetTurnTimeout` function has a script, rather than a block, which means that it is non-blocking and that local variables cannot be accessed inside the script. For a fuller discussion, see the note for [ShowMenu](/functions/user-interface#showmenu).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks_and_scripts).

## SetTurnTimeoutID

SetTurnTimeoutID (integer turn count, string name, script)

or

    SetTurnTimeoutID (integer turn count, string name){ script }

Runs the specified script after the specified number of turns.

The name specifies the name of the turnscript to create. The anonymous version of this function is [SetTurnTimeout](#setturntimeout).

**Note:** This function is "non-blocking", and its script has no access to local variables. For a fuller discussion, see the note on [Blocks and Scripts](/howto/scripting/blocks_and_scripts).

## SuppressTurnscripts

SuppressTurnscripts ()

Stops all turnscripts running for one turn.

For more on turnscripts, see [here](/howto/scripting/using_turnscripts).
