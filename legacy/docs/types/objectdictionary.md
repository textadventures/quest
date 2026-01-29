---
layout: index
title: Objectdictionary
---

An objectdictionary is a dictionary where keys are [strings](string.html) and values are [objects](object.html).

The format is "key = value", separated by semicolons.

For example, for Quest 5.3 and earlier the format looks like this:

     <myattribute type="objectdictionary">first = player; second = lounge</myattribute>

For Quest 5.4 and later the format is:

     <myattribute type="objectdictionary">
       <item>
         <key>first</key>
         <value>player</value>
       </item>
       <item>
         <key>second</key>
         <value>lounge</value>
       </item>
     </myattribute>

In Quest 5.4, you can still use the old semicolon-separated format by specifying "simpleobjectdictionary":

     <myattribute type="simpleobjectdictionary">first = player; second = lounge</myattribute>

This defines:

|key|value|
|---|-----|
|first|player|
|second|lounge|

See [Using Dictionaries](../using_dictionaries.html)
