---
layout: index
title: turnscript element
---

    <turnscript name="name">attributes</turnscript>

Turnscript attributes:

enabled  
[boolean](../types/boolean.html) specifying whether turnscript is active

script  
[script](../types/script.html) specifying what to do after each turn

Note that as of 5.7.2, turnscipts run in alphabetic order (in earlier versions the order could change unexpectedly). To have turnscripts in a certain order, prefix them "ts01_", "ts02_", ... .
