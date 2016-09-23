---
layout: index
title: delegate element
---

    <delegate name="name"optional type="type"optional parameters="parameters">properties</delegate>

Creates a delegate type. Delegates are script properties that can be called like functions. The delegate tag defines the function signature (the parameters passed to the function and its return type, if any), and then an object can provide its own implementation of the delegate function.

You can run delegate functions on objects using the [rundelegate](../scripts/rundelegate.html) command (if the delegate does not return a value) or using the [RunDelegateFunction](../functions/rundelegatefunction.html) function (for delegates that do return a value).

See [Using Delegates](../types/using_delegates.html)
