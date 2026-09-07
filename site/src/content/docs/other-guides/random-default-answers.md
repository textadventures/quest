---
title: Random default answers
sidebar:
  order: 6
---

The default answer of a command is defined in the language file. So if you want to change this text, you can copy the template(s) with the name “Default…” that you wish to modify into your game as described [here](/advanced-topics/overriding).

       
```xml
<dynamictemplate name="DefaultHit">"You can't hit " + object.article + "."</dynamictemplate>

```

## Random default answers

If you want to return more than one default message you have to define additional dynamictemplates and call them from the main-template with the function [DynamicTemplate](/functions/string#dynamictemplate)

**example for kill-command:**

```xml
<dynamictemplate name="DefaultKill">DynamicTemplate ("DefaultKill" + ToString (GetRandomInt (1,3)) , object)  </dynamictemplate>
<dynamictemplate name="DefaultKill1">"This would not be nice."</dynamictemplate>
<dynamictemplate name="DefaultKill2">"No, you won't do this."</dynamictemplate>
<dynamictemplate name="DefaultKill3">"You can't kill " + object.article + "."</dynamictemplate>
```

So if you want to add two more answers you have to add the dynamictemplates with the name **DefaultKill4** and **DefaultKill5** and change the upper bound of the `GetRandomInt` function from 3 to 5
