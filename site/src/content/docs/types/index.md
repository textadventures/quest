---
title: Attribute Types
sidebar:
  order: 5
---

Variables and object attributes can be any of the following types.

## Null

If Booleans seem limited in have only two possible values, null can have only one!

In fact, null is a special value for attributes that says the attribute does not exist (which is different to local variables, which can be assigned a value of `null`, but do still exist). Setting an attribute to null is the same as deleting it (when the game is saved, null attributes are not written).

You can check if an attribute is null using the "null" keyword:

     if (someobject.parent = null) { ... }

There is a "gotcha" lurking here. If your object is of a type that sets an attribute to some value, and your object sets it to another value, what happens when you set that attribute on the object to null? The attribute is removed from the object, and so reverts to being the value from the type. This may not be what you expect!

## String

A string is a piece of text (string literal), a string variable is variable that holds text.

    myStingVar = "World"

Strings can be added together in any combination of string literal (enclosed in quotes) or variables.

    myNewStringVar = "Hello " + myStringVar

They can also be used to show messages to the user with the *msg* command.

    msg (myNewStringVar)

Also see String Functions

## Script

A script attribute contains code for Quest to run, i.e., a list of  instructions for Quest to carry out. Everything that happens in a game is controlled by script commands. Script commands can print messages, move objects around, show videos, start timers, change attributes, and much more.

Example:

     <look type="script">
       if (not fridge.isopen) {
         msg ("The fridge is open, casting its light out into the gloomy kitchen.")
       }
       else {
         msg ("A big old refrigerator sits in the corner, humming quietly.")
       }
     </look>

     
Scripts can be created by adding script commands using the user interface, or by typing code in "code view". Behind the scenes, it is all the same, so you can flip between the two as you like.

You can use [do](/scripts#do) or [invoke](/scripts#invoke) to have Quest run a script.

Let us suppose the above script is attached to an object called "fridge". You could run the script:

```
do(fridge, "look")

invoke(fridge.look)
```

If you use the `do` command, your script will have access to a local variable called `this`, which points to the object the script belongs to. This is very useful when making generic scripts; one script can be added to numerous objects, and when the script runs it can find out what it belongs to.

You can send other values to a script by adding them to a dictionary. For each name-value pair you add to the dictionary, a local variable will be available the name being the key, and the value being the value.

```
dict = NewDictionary()
dictionary add (dict, "npc", mary)
dictionary add (dict, "obj", sandwich)
do(fridge, "look", dict)
```

Now the "look" script will have access to local variables called "npc" and "obj", as well as "this". There is a shortcut to do that:

```
do(fridge, "look", QuickParams("npc", mary, "obj", sandwich))
```

The `QuickParams` function can take either 2, 4 or 6 parameters, allowing you to add 1, 2 or 3 variables.

You can use the `IsDefined` function within a script to determine if it has access to a certain variable. Note that it takes a string.

```
if (IsDefined("npc")) {
```

There is no way to convert a string to a script during play, by the way (though you can do something similar with the [Eval](/functions/general#eval) function).

## Boolean

A Boolean can be either `true` or `false`. When using the GUI to create a script, they are called flags, and can be on or off. Boolean attributes are extremely use as they can tell us the current state of an object. It the torch on or off? Is the hat worn or not? Has the room been visited?

Note that you do not need to compare a Boolean to `true` or `false`. It is already one of the other. Instead of:

```
if (player.is_successful = true) {
```

Just do:

```
if (player.is_successful) {
```

If you want to test that it is not true, just add the `not` keyword:

```
if (not player.is_successful) {
```

Also note that to do any of the you need to ensure the Boolean is initialised (i.e., it has a value at the start of the game). If `player.is_successful` has not been set, then when you do one of the comparisons above you will get an error message.

Alternatively, use `GetBoolean`, which returns `true` if the attribute is `true`, or `false` if it is `false` or `null` (i.e., has not been set).

```
if (GetBoolean(player, "is_successful")) {
```

Or:

```
if (not GetBoolean(player, "is_successful")) {
```

## Int

An "int" (integer) attribute represents a whole number (which can be positive or negative).

Examples: 1, 2, -167, 37835685, 0.

An "int" attribute is represented internally as a signed 32-bit variable, which means it can range from -2147483648 to 2147483647 (so up to just over 2 billion, which is probably high enough for most games). Going outside that range will lead to some funny effects, as numbers wrap around - if you add 1 to 2147483647 you will get -2147483648!

## Double

A "double" attribute represents a number with a decimal point. It can be positive or negative.

Examples: 1.23, 5.8214, -0.12421, 0.0.

More [here](/howto/scripting/using_doubles).

## Object

An object attribute points to another object by name.

For example:

     <parent type="object">lounge</parent>

would be another way of setting the [parent](/attributes#parent) attribute of an object, if you didn't want to nest the XML definition.

## Stringlist

A stringlist is a [list](#list) that can contain a number of elements, all have to be of type [string](#string).

For Quest 5.3 and earlier, the format in an ASLX file is this:

     <mylist type="list">one; two; three</mylist>

The same list is expressed like this:

     <mylist type="list">
       <value>one</value>
       <value>two</value>
       <value>three</value>
     </mylist>

In Quest 5.4, you can still use the older semi-colon separate format with "simplestringlist":

     <mylist type="simplestringlist">one; two; three</mylist>

See [Using Lists](/howto/scripting/using_lists).

## Objectlist

An objectlist is a [list](#list) that can contain any number of elements, all of which have to be of type [object](#object).

The format in an ASLX file is:

     <mylist type="objectlist">player; object1; thing</mylist>

 See [Using Lists](/howto/scripting/using_lists) for more information.

## List

"list" is a sequence of any attribute type. The format is in the ASLX file:

     <myattribute type="list">
       <value type="string">a string value</value>
       <value type="int">123</value>
     </myattribute>

Usually it is better to use a [stringlist](#stringlist) (if all elements in the list will be strings) or an [objectlist](#objectlist) (if all elements in the list will be objects) instead.

There is more on lists [here](/howto/scripting/using_lists).

## Objectdictionary

An objectdictionary is a dictionary where keys are [strings](#string) and values are [objects](#object).

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

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## Scriptdictionary

A scriptdictionary is a dictionary which has [string](#string) keys and [script](#script) values.

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

## Dictionary

"dictionary" is a mapping of string keys to values of any attribute type.

Usually it is better to use a more specific dictionary type if you can, if you know that all the values will be of the same type. These more specific types are [stringdictionary](#stringdictionary), [objectdictionary](#objectdictionary) and [scriptdictionary](#scriptdictionary).

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

## Stringdictionary

A stringdictionary is a dictionary where both keys and values are [strings](#string).

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

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## Command pattern

Quest uses regular expressions to compare commands with what the player typed, and the regular expression is converted from a string in the background (see [here](/howto/commands/pattern_matching) for more on that). However, it also offers a simplified version, a "command pattern". This is essentially a string (such as "tie #object1# to #object2"), which Quest will convert to another string when the game start (in this case "^tie (?.*) to (?.*)$"), which can then be converted to a regular expression when required. There is not much point to command patterns outside of commands.
