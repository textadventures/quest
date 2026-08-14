---
title: container_lockable
---

The "container\_lockable" type is defined in CoreTypes.aslx. It implements locking and unlocking functionality. It does not inherit any other container types, so one of [container\_open](/attributes/container_open), [container\_closed](/attributes/container_closed) or [container\_limited](/attributes/container_limited) should also be inherited.

By inheriting the "container\_lockable" type in an object, script is added to the [open](/attributes/open) and [close](/attributes/close) attributes to implement the check to see whether the object is locked before allowing open/close. The "lock" and "unlock" verbs are added to the object. The container is [locked](/attributes/locked) by default, and the [autoopen](/attributes/autoopen) and [autounlock](/attributes/autounlock) attributes are set to "true".
