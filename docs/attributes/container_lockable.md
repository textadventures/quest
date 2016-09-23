---
layout: index
title: container_lockable
---

The "container\_lockable" type is defined in CoreTypes.aslx. It implements locking and unlocking functionality. It does not inherit any other container types, so one of [container\_open](container_open.html), [container\_closed](container_closed.html) or [container\_limited](container_limited.html) should also be inherited.

By inheriting the "container\_lockable" type in an object, script is added to the [open](open.html) and [close](close.html) attributes to implement the check to see whether the object is locked before allowing open/close. The "lock" and "unlock" verbs are added to the object. The container is [locked](locked.html) by default, and the [autoopen](autoopen.html) and [autounlock](autounlock.html) attributes are set to "true".
