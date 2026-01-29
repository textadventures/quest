---
layout: index
title: take
---

"take" is an attribute that can be either [boolean](../types/boolean.html) or [script](../types/script.html).

-   If boolean, a value of "true" means the object can be taken, and "false" means the object cannot be taken. A default response (using the TakeSuccessful or TakeUnsuccessful template) is printed when the user takes (or tries to take) the object, unless a [takemsg](takemsg.html) attribute is defined.
-   If script, the script will be run when the user tries to take the object.

See also [drop](drop.html), [takemsg](takemsg.html).
