---
title: turnscript element
sidebar:
  order: 16
---

    <turnscript name="name">attributes</turnscript>

Turnscript attributes:

enabled  
[boolean](/types/boolean) specifying whether turnscript is active

script  
[script](/types/script) specifying what to do after each turn

Note that as of 5.7.2, turnscripts run in alphabetic order (in earlier versions the order could change unexpectedly). To have turnscripts in a certain order, prefix them "ts01_", "ts02_", ... .
