---
layout: index
title: Stringdictionary
---

A stringdictionary is a dictionary where both keys and values are [strings](string.html).

The format is "key = value", separated by semicolons.

For example (for Quest 5.3 and earlier):

     <statusattributes type="stringdictionary">turns = You have taken ! turns; health = Health !%</statusattributes>

For Quest 5.4 and later the format is:

     <statusattributes type="stringdictionary">
       <item>
         <key>turns</key>
         <value>You have taken ! turns</value>
       </item>
       <item>
         <key>health</key>
         <value>Health !%</value>
       </item>
     </statusattributes>

In Quest 5.4, you can still use the old semicolon-separated format using "simplestringdictionary":

     <statusattributes type="simplestringdictionary">turns = You have taken ! turns; health = Health !%</statusattributes>

This defines:

|key|value|
|---|-----|
|turns|You have taken ! turns|
|health|Health !%|

See [Using Dictionaries](../using_dictionaries.html)
