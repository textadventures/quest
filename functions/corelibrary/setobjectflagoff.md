---
layout: index
title: SetObjectFlagOff
---

    SetObjectFlagOff (object, string flag name)

Turns the object flag off - an object flag is simply a [boolean](../../../types/boolean.html) attribute, so:

     SetObjectFlagOff(myobject, "myflag")

is equivalent to

     myobject.myflag = false

See also [SetObjectFlagOn](setobjectflagon.html)
