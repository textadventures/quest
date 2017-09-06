---
layout: index
title: for
---

    for (iterator variable, int from, int to) { script }

As of Quest 5.1 there is an optional "step" parameter:

    for (iterator variable, int from, int to, int step) { script }

Run a script multiple times, incrementing the iterator variable between the specified limits. If a "step" parameter is specified, the iterator variable will be incremented by that amount each time (if not specified, the default step size is 1).

Trandionally, i, j, k... are used as iterator varable names. This simple example runs from 1 to 5, printing each value in turn:

    for (i, 1, 5) {
      msg(game.i)
    }

Generally, `foreach` offers a neater way of going through a list, but `for` can be useful for iterating through a string. This example will print each character in the string, together with its position:

    s = "Hello World!"
    for (i, 1, LengthOf(s)) {
      msg(i + ": " + Mid(s, i, 1))
    }


_Note:_ The iterator variable should be a local variable, not an attribute. For example, consider this code, which uses an attribute of the game object:

    for (game.i, 1, 5) {
      msg(game.i)
    }

If `game.i` already exists, the loop will run 5 times as expected, but the value of `game.i` will keep its original value. If  `game.i` does not exist, an error will be produced.

See [Using Lists](../guides/using_lists.html)
