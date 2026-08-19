---
title: "Dictionary functions"
sidebar:
  order: 8
---

Functions for manipulating dictionaries. For a discussion on how to use dictionaries, see [here](/howto/scripting/using_dictionaries).

## DictionaryAdd
```quest
DictionaryAdd (dictionary, string key, string value)
```

Adds to the dictionary an element with the specified key and value. If an element with that key already exists in the dictionary, that element will be removed first.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## DictionaryContains
```quest
DictionaryContains (dictionary, string key)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) - **true** if the dictionary contains an element with the specified key.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## DictionaryCount
```quest
DictionaryCount (dictionary)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an [int](/types#int) - the number of items in the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## DictionaryItem
```quest
DictionaryItem (dictionary, string key)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Retrieves the specified item from the dictionary. Returns a [string](/types#string) or [object](/types#object), depending on whether the dictionary is an [objectdictionary](/types#objectdictionary) or a [stringdictionary](/types#stringdictionary).

Usually you will know the type of list that you're passing in, so you should use the [StringDictionaryItem](#stringdictionaryitem), [ObjectDictionaryItem](#objectdictionaryitem) or [ScriptDictionaryItem](#scriptdictionaryitem) functions instead.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## DictionaryRemove
```quest
DictionaryRemove (dictionary, string key)
```

Removes from the dictionary the element with the specified key. If there is no such key, it does nothing.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## NewDictionary
```quest
NewDictionary ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an empty [dictionary](/howto/scripting/using_dictionaries). The dictionary can contain any type of data, or a mixture - for example, both objects and strings.

If the dictionary will only contain one type of data (as will usually be the case), you should use [NewStringDictionary](#newstringdictionary) or [NewObjectDictionary](#newobjectdictionary) instead.

## NewObjectDictionary
```quest
NewObjectDictionary ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an empty [objectdictionary](/types#objectdictionary).

## NewScriptDictionary
```quest
NewScriptDictionary ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an empty [scriptdictionary](/types#scriptdictionary).

## NewStringDictionary
```quest
NewStringDictionary ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns an empty [stringdictionary](/types#stringdictionary).

## ObjectDictionaryItem
```quest
ObjectDictionaryItem (dictionary, string key)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the [object](/types#object) specified by the dictionary key.

You can use the [DictionaryItem](#dictionaryitem) function if you don't know the type of the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## QuickParams
```quest
QuickParams (string key1, any type value1)
QuickParams (string key1, any type value1, string key2, any type value2)
QuickParams (string key1, any type value1, string key2, any type value2, string key3, any type value3)
```

QuickParams offers a quick way to create a dictionary, and is especially useful for passing to a script (where local variables will be available with the key used as the name, and the value as the value). The key must therefore be a string, but the value can be of any type.

The function can take 2, 4 or 6 parameters to give a dictionary with 1, 2 or 3 entries.

```quest
d = QuickParams("obj", apple)
d = QuickParams("obj", apple, "count", 45)
d = QuickParams("obj", apple, "count", 45, "s", "Hmm, yummy")
```

Now you can invoke a script, passing three parameters all in one line:

```quest
do (npc, "givefood", QuickParams("obj", apple, "count", 45, "s", "Hmm, yummy"))
```

In this example, the "givefood" script attribute of the NPC is called. In the script, there will be three local variables available, `obj`, `count` and `s`.

## ScriptDictionaryItem
```quest
ScriptDictionaryItem (scriptdictionary, string key)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the [script](/types#script) specified by the dictionary key.

You can use the [DictionaryItem](#dictionaryitem) function if you don't know the type of the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## StringDictionaryItem
```quest
StringDictionaryItem (stringdictionary, string key)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns the [string](/types#string) specified by the dictionary key.

You can use the [DictionaryItem](#dictionaryitem) function if you don't know the type of the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

