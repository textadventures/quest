---
layout: index
title: if
---

    if (boolean expression) { script } [ else if ... ]* [ else { script } ]

Conditionally runs the script. If the condition fails, the `else` script is run, if present. Muliple `if/else`s can be put together. Some examples:

```
if (result > 10) {
  msg("Great!")
}
```
An `else` can be added (no need for a condition)
```
if (result > 10) {
  msg("Great!")
}
else {
  msg("Rubbish!")
}
```
Or we can have a condition; nohing gets printed if result is between 2 and 10.
```
if (result > 10) {
  msg("Great!")
}
else if result < 2) {
  msg("Rubbish!")
}
```
You can have as many `if/else` linked together as you need (but consider using [switch](switch.html)).
```
if (result > 10) {
  msg("Great!")
}
else if result > 2) {
  msg("Meh...")
}
else {
  msg("Rubbish!")
}
```

Complex conditions can be used with Boolean arithmetic. 

```
if (result > 10 and not player.is_female) {
  msg("Good boy")
}
```
