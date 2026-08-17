---
title: Scriptdictionary
sidebar:
  order: 12
---

A scriptdictionary is a dictionary which has [string](/types/string) keys and [script](/types/script) values.

It is defined with nested \<item\> keys for each key/value pair.

For example:

     <useon type="scriptdictionary">
       <item key="object1">
         msg ("you use object1")
       </item>
       <item key="object2">
         msg ("you use object2")
       </item>
     </useon>

See [Using Dictionaries](/howto/scripting/using_dictionaries)
