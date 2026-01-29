---
layout: index
title: RandomChance
---

    RandomChance (integer percentile)

Percentile parameter should be between 0 and 100.

This function generates a random number between 1 and 100. If the result is less than or equal to the specified percentile value, this function returns true.

The effect is that if use RandomChance(10), there is a 10% chance of the function returning true, RandomChance(50) has a 50% chance of returning true, etc.
