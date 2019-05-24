---
layout: index
title: QuickParams
---

    QuickParams (string key1, any type value1)
    QuickParams (string key1, any type value1, string key2, any type value2)
    QuickParams (string key1, any type value1, string key2, any type value2, string key3, any type value3)

QuickParams offers a quick way to create a dictionary, and is especially useful for passing to a script (where local variables will be available with the key used as the name, and the value as the value). The key must therefore be a string, but the value can be of any type.

The function can take 2, 4 or 6 parameters to give a dictionary with 1, 2 or 3 entries.

```
d = QuickParams("obj", apple)
d = QuickParams("obj", apple, "count", 45)
d = QuickParams("obj", apple, "count", 45, "s", "Hmm, yummy")
```

Now you can invoke a script, passing three parameters all in one line:

```
do (npc, "givefood", QuickParams("obj", apple, "count", 45, "s", "Hmm, yummy"))
```

In this example, the "givefood" script attribute of the NPC is called. In the script, there will be three local variables available, `obj`, `count` and `s`.