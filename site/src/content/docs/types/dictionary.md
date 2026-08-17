---
title: Dictionary
sidebar:
  order: 13
---

"dictionary" is a mapping of string keys to values of any attribute type.

Usually it is better to use a more specific dictionary type if you can, if you know that all the values will be of the same type. These more specific types are [stringdictionary](/types/stringdictionary), [objectdictionary](/types/objectdictionary) and [scriptdictionary](/types/scriptdictionary).

Here is an example dictionary containing a variety of different types:

     <example type="dictionary">
       <item>
         <key>key1</key>
         <value type="string">A string value.</value>
       </item>
       <item>
         <key>key2</key>
         <value type="int">12</value>
       </item>
       <item>
         <key>key3</key>
         <value type="script">
           msg ("This is a script")
         </value>
       </item>
       <item>
         <key>key4</key>
         <value type="dictionary">
           <item>
             <key>subkey1</key>
             <value type="string">This is a string inside a dictionary inside another dictionary.</value>
           </item>
         </value>
       </item>
     </example>

See [Using Dictionaries](/howto/scripting/using_dictionaries)
