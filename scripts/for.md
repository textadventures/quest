---
layout: index
title: for
---

    for (iterator variable, int from, int to) { script }

As of Quest 5.1 there is an optional "step" parameter:

    for (iterator variable, int from, int to, int step) { script }

Run a script multiple times, incrementing the iterator variable between the specified limits. If a "step" parameter is specified, the iterator variable will be incremented by that amount each time (if not specified, the default step size is 1).

See [Using Lists](../guides/using_lists.html)
