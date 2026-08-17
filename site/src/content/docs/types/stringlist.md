---
title: Stringlist
sidebar:
  order: 8
---

A stringlist is a [list](/types/list) that can contain a number of elements, all have to be of type [string](/types/string).

For Quest 5.3 and earlier, the format in an ASLX file is this:

     <mylist type="list">one; two; three</mylist>

As of Quest 5.4, the same list is expressed like this:

     <mylist type="list">
       <value>one</value>
       <value>two</value>
       <value>three</value>
     </mylist>

In Quest 5.4, you can still use the older semi-colon separate format with "simplestringlist":

     <mylist type="simplestringlist">one; two; three</mylist>

See [Using Lists](/howto/scripting/using_lists).
