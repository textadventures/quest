---
layout: index
title: drop
---

"drop" is an attribute that can be either [boolean](../types/boolean.html) or [script](../types/script.html).

-   If boolean, a value of "true" means the object can be dropped, and "false" means the object cannot be dropped. A default response (using the DropSuccessful or DropUnsuccessful template) is printed when the user drops (or tries to drop) the object, unless a [dropmsg](dropmsg.html) attribute is defined.
-   If script, the script will be run when the user tries to drop the object.

See also [take](take.html), [dropmsg](dropmsg.html).
