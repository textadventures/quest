---
layout: index
title: Int
---

An "int" (integer) attribute represents a whole number (which can be positive or negative).

Examples: 1, 2, -167, 37835685, 0.

An "int" attribute is represented internally as a signed 32-bit variable, which means it can range from -2147483648 to 2147483647 (so up to just over 2 billion, which is probably high enough for most games). Going outside that range will lead to some funny effects, as numbers wrap around - if you add 1 to 2147483647 you will get -2147483648!
