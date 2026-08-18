---
title: "General Functions"
sidebar:
  order: 12
---



## CurrentDateUTC

CurrentDateUTC()

Returns the [int](/types#int) value of the current Unix epoch time in UTC. 

Information on the Unix timestamp can be found at [Wikipedia](https://en.wikipedia.org/wiki/Unix_time)

## Eval

Eval (string expression, dictionary parameters)

or

    Eval (string expression)

Returns the result of the specified expression.

The parameters dictionary can be used to add variables into the eval context. The parameters will be usable by the evaluated expression.

Example:

      params = NewDictionary()
      dictionary add(params, "x", 50)
      dictionary add(params, "y", 100)
      msg(Eval ("x + y", params))

This will result in "150" being printed.

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetFileData

GetFileData (string file name)

Loads the specified file and returns a [string](/types#string) containing its contents.

NOTE: This a [hard-coded function](/functions/hardcoded).

## GetFileURL

GetFileURL(string filename)

Returns a [string](/types#string) containing the full path to the specified file. The file must exist in the same directory as the game. This can be used to access a game's resources such as sounds and pictures, and pass their URLs to the player UI.

NOTE: This a [hard-coded function](/functions/hardcoded).

## Log

Log (string text)

Writes the [string](/types#string) **text** to the browser's JavaScript/developer console. This is intended for debugging during game development, not as a player-facing feature.

## RunDelegateFunction

RunDelegateFunction (object, string attribute name, any type parameters ...)

Return type is specified by the delegate definition.

Runs the specified delegate function on an object.

See [Using Delegates](/advanced-topics/using_delegates)

NOTE: This a [hard-coded function](/functions/hardcoded).
