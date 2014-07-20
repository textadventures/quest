---
layout: index
title: GetBlockingObject
---

    GetBlockingObject (object)

Returns the [object](../../../types/object.html) which is preventing the player from reaching the specified object.

If an object is in [ScopeVisible](scopevisible.html) but not in [ScopeReachable](scopereachable.html), then it may be inside a container where the player can see it but not reach it. You can call the GetBlockingObject function to find out what is "blocking" the player from reaching the object. It will be the top-most parent which the player cannot reach through.
