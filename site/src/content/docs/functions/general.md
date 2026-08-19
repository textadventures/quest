---
title: "General functions"
sidebar:
  order: 12
---

## CurrentDateUTC
```
CurrentDateUTC()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the [int](/types#int) value of the current Unix epoch time in UTC. 

Information on the Unix timestamp can be found at [Wikipedia](https://en.wikipedia.org/wiki/Unix_time)

## DbgLog
```
DbgLog (string text)
```

If the `debugging` attribute on `game` is true, writes **text** to the browser's JavaScript/developer console via [Log](#log), prefixed with "DEBUGGING: ". Does nothing otherwise. See also [DbgMsg](#dbgmsg).

## DbgMsg
```
DbgMsg (string text)
```

If the `debugging` attribute on `game` is true, prints **text** to the player as a styled paragraph, prefixed with "DEBUGGING: ". Does nothing otherwise. Unlike [DbgLog](#dbglog), the output is visible to the player, not just in the developer console - useful for debug output you want to see in-game while testing.

## DisableHtmlLog
```
DisableHtmlLog ()
```

Sets the `nohtmllog` attribute on `game` to true. This is checked by other parts of Core.aslx (e.g. transcript handling) to decide whether to write to the game's own HTML activity log.

## DisableTranscript
```
DisableTranscript ()
```

Turns off transcript recording. See also [EnableTranscript](#enabletranscript) and [KillTranscript](#killtranscript).

## EnableTranscript
```
EnableTranscript ()
```

Turns on transcript recording (a saveable log of the game's text output), unless the player has opted out via the `notranscript` attribute on `game` - in which case it calls [KillTranscript](#killtranscript) instead. Normally called via [InitiateTranscript](#initiatetranscript) rather than directly. See also [DisableTranscript](#disabletranscript).

## Eval
```
Eval (string expression, dictionary parameters)
```

or

```
Eval (string expression)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the result of the specified expression.

The parameters dictionary can be used to add variables into the eval context. The parameters will be usable by the evaluated expression.

Example:

      params = NewDictionary()
      dictionary add(params, "x", 50)
      dictionary add(params, "y", 100)
      msg(Eval ("x + y", params))

This will result in "150" being printed.

## GetFileData
```
GetFileData (string file name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Loads the specified file and returns a [string](/types#string) containing its contents.

## GetFileURL
```
GetFileURL(string filename)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [string](/types#string) containing the full path to the specified file. The file must exist in the same directory as the game. This can be used to access a game's resources such as sounds and pictures, and pass their URLs to the player UI.

## InitiateTranscript
```
InitiateTranscript (string filename)
```

Starts a transcript recording under the given filename (used as the suggested name when the player saves it), printing a confirmation message and then calling [EnableTranscript](#enabletranscript). Pass an empty string to use the game's own name as the filename.

## KillTranscript
```
KillTranscript ()
```

Permanently disables transcript recording for the rest of the session by setting the `notranscript` attribute on `game`, then calls [DisableTranscript](#disabletranscript). Unlike DisableTranscript, this cannot be undone by calling [EnableTranscript](#enabletranscript) again.

## Log
```
Log (string text)
```

Writes the [string](/types#string) **text** to the browser's JavaScript/developer console. This is intended for debugging during game development, not as a player-facing feature.

## RunDelegateFunction
```
RunDelegateFunction (object, string attribute name, any type parameters ...)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Return type is specified by the delegate definition.

Runs the specified delegate function on an object.

See [Using delegates](/advanced-topics/using_delegates)

