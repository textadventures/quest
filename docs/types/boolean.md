---
layout: index
title: Boolean
---

A Boolean can be either `true` or `false`. When using the GUI to create a script, they are called flags, and can be on or off. Boolean attributes are extremely use as they can tell us the current state of an object. It the torch on or off? Is the hat worn or not? Has the room been visited?

Note that you do not need to compare a Boolean to `true` or `false`. It is already one of the other. Instead of:

```
if (player.is_successful = true) {
```

Just do:

```
if (player.is_successful) {
```

If you want to test that it is not true, just add the `not` keyword:

```
if (not player.is_successful) {
```

Also note that to do any of the you need to ensure the Boolean is initialised (i.e., it has a value at the start of the game). If `player.is_successful` has not been set, then when you do one of the comparisons above you will get an error message.

Alternatively, use `GetBoolean`, which returns `true` if the attribute is `true`, or `false` if it is `false` or `null` (i.e., has not been set).

```
if (GetBoolean(player, "is_successful")) {
```

Or:

```
if (not GetBoolean(player, "is_successful")) {
```
