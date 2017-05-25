---
layout: index
title: create timer
---

    create timer (string name)

Creates a timer with the specified name. You can then use `GetObject` to get the timer, and assign values to it. Here is a trivial example that will produce a timer that will tell you its name every 10 seconds:

create timer ("test_timer")
o = GetTimer ("test_timer")
msg (TypeOf(o))
o.script => {
  msg ("timer=" + this.name)
}
o.interval = 10
EnableTimer(o)

It is generally easier to create the timer in the editor, but have it disabled, and then enable it when required.
