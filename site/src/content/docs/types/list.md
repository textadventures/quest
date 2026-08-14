---
title: List
sidebar:
  order: 10
---

**New in Quest 5.4**

"list" is a sequence of any attribute type. The format is in the ASLX file:

     <myattribute type="list">
       <value type="string">a string value</value>
       <value type="int">123</value>
     </myattribute>

Usually it is better to use a [stringlist](/types/stringlist) (if all elements in the list will be strings) or an [objectlist](/types/objectlist) (if all elements in the list will be objects) instead.

There is more on lists [here](/using_lists).
