---
title: "Dictionary Functions"
sidebar:
  order: 8
---

Functions for manipulating dictionaries. For a discussion on how to use dictionaries, see [here](/howto/scripting/using_dictionaries).

## DictionaryAdd

DictionaryAdd (dictionary, string key, string value)

Adds to the dictionary an element with the specified key and value. If an element with that key already exists in the dictionary, that element will be removed first.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## DictionaryContains

DictionaryContains (dictionary, string key)

Returns a [boolean](/types#boolean) - **true** if the dictionary contains an element with the specified key.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

NOTE: This a [hard-coded function](/functions/hardcoded).

## DictionaryCount

DictionaryCount (dictionary)

Returns an [int](/types#int) - the number of items in the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

NOTE: This a [hard-coded function](/functions/hardcoded).

## DictionaryItem

DictionaryItem (dictionary, string key)

Retrieves the specified item from the dictionary. Returns a [string](/types#string) or [object](/types#object), depending on whether the dictionary is an [objectdictionary](/types#objectdictionary) or a [stringdictionary](/types#stringdictionary).

Usually you will know the type of list that you're passing in, so you should use the [StringDictionaryItem](#stringdictionaryitem), [ObjectDictionaryItem](#objectdictionaryitem) or [ScriptDictionaryItem](#scriptdictionaryitem) functions instead.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## DictionaryRemove

DictionaryRemove (dictionary, string key)

Removes from the dictionary the element with the specified key. If there is no such key, it does nothing.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## NewDictionary

NewDictionary ()

Returns an empty [dictionary](/howto/scripting/using_dictionaries). The dictionary can contain any type of data, or a mixture - for example, both objects and strings.

If the dictionary will only contain one type of data (as will usually be the case), you should use [NewStringDictionary](#newstringdictionary) or [NewObjectDictionary](#newobjectdictionary) instead.

NOTE: This a [hard-coded function](/functions/hardcoded).

## NewObjectDictionary

NewObjectDictionary ()

Returns an empty [objectdictionary](/types#objectdictionary).

NOTE: This a [hard-coded function](/functions/hardcoded).

## NewScriptDictionary

NewScriptDictionary ()

Returns an empty [scriptdictionary](/types#scriptdictionary).

NOTE: This a [hard-coded function](/functions/hardcoded).

## NewStringDictionary

NewStringDictionary ()

Returns an empty [stringdictionary](/types#stringdictionary).

NOTE: This a [hard-coded function](/functions/hardcoded).

## ObjectDictionaryItem

ObjectDictionaryItem (dictionary, string key)

Returns the [object](/types#object) specified by the dictionary key.

You can use the [DictionaryItem](#dictionaryitem) function if you don't know the type of the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

NOTE: This a [hard-coded function](/functions/hardcoded).

## QuickParams

QuickParams (string key1, any type value1)
    QuickParams (string key1, any type value1, string key2, any type value2)
    QuickParams (string key1, any type value1, string key2, any type value2, string key3, any type value3)

QuickParams offers a quick way to create a dictionary, and is especially useful for passing to a script (where local variables will be available with the key used as the name, and the value as the value). The key must therefore be a string, but the value can be of any type.

The function can take 2, 4 or 6 parameters to give a dictionary with 1, 2 or 3 entries.

```
d = QuickParams("obj", apple)
d = QuickParams("obj", apple, "count", 45)
d = QuickParams("obj", apple, "count", 45, "s", "Hmm, yummy")
```

Now you can invoke a script, passing three parameters all in one line:

```
do (npc, "givefood", QuickParams("obj", apple, "count", 45, "s", "Hmm, yummy"))
```

In this example, the "givefood" script attribute of the NPC is called. In the script, there will be three local variables available, `obj`, `count` and `s`.

## ScriptDictionaryItem

ScriptDictionaryItem (scriptdictionary, string key)

Returns the [script](/types#script) specified by the dictionary key.

You can use the [DictionaryItem](#dictionaryitem) function if you don't know the type of the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

NOTE: This a [hard-coded function](/functions/hardcoded).

## StringDictionaryItem

StringDictionaryItem (stringdictionary, string key)

Returns the [string](/types#string) specified by the dictionary key.

You can use the [DictionaryItem](#dictionaryitem) function if you don't know the type of the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

NOTE: This a [hard-coded function](/functions/hardcoded).
