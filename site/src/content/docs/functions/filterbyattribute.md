---
title: FilterByAttribute
---

    FilterByAttribute (objectlist list, string attribute name, any value)

Returns a new object list containing only the objects in the given list for which the named attribute has the given value (which can be of any type).

Note that if the value is `null` this effectively filters for objects without the named attribute.

See also [FilterByNotAttribute](/functions/filterbynotattribute).

You can omit the last value, and it will be assumed to be `null`.