---
layout: index
title: create turnscript
---

    create turnscript (string name)

Creates a turnscript with the specified name. You can then use `GetObject` to get the turn script, and assign values to it. Here is a trivial example that will produce a turnscript that will tell you its name every turn:

    create turnscript ("test_ts")
    o = GetObject("test_ts")
    o.script => {
      msg ("turnscript=" + this.name)
    }
    o.enabled = true

It is generally easier to create the turn script in the editor, but have it disabled, and then enable it when required.
    