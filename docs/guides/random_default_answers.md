---
layout: index
title: Random default answers
---

The default answer of an command is defined in the language file of Quest5. So if you want to change this text, you have to copy your language file (for example english.aslx) from the Quest5-directory to your game directory. There you can open it with an external text editor and change templates with the name "Default...".

       
     <dynamictemplate name="DefaultHit">"You can't hit " + object.article + "."</dynamictemplate>
     

random default answers
----------------------

If you want to return more than one default message you have to define additional dynamictemplates and call them from the main-template with the function [DynamicTemplate](../functions/dynamictemplate.html)

**example for kill-command:**

     <dynamictemplate name="DefaultKill">DynamicTemplate ("DefaultKill" + ToString (GetRandomInt (1,3)) , object)  </dynamictemplate>
      <dynamictemplate name="DefaultKill1">"This would not be nice."</dynamictemplate>
      <dynamictemplate name="DefaultKill2">"No, you won't do this."</dynamictemplate>
      <dynamictemplate name="DefaultKill3">"You can't kill " + object.article + "."</dynamictemplate>

So if you want to add two more answers you have to add the dynamictemplates with the name **DefaultKill4** and **DefaultKill5** and change the upper bound of the GetRandomInt-function from 3 to 5
