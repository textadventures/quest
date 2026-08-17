---
title: FilterByNotAttribute
---

    FilterByNotAttribute (objectlist list, string attribute name, any value)

Returns a new object list containing only the objects in the given list for which the named attribute does _not_ have the given value (which can be of any type).

Note that if the value is `null` this effectively filters for objects with the named attribute, whatever the value.

See also [FilterByAttribute](/functions/filterbyattribute).

You can omit the last value, and it will be assumed to be `null`.