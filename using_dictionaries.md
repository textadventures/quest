---
layout: index
title: Using Dictionaries
---

A dictionary maps keys to values - it is a look-up table which allows you to store an unlimited amount of data, retrievable by looking up the key value.

Quest has three different dictionary types. Keys are always [string](types/string.html), but the value type depends on the dictionary used:

-   [stringdictionary](types/stringdictionary.html) has values of type [string](types/string.html)
-   [scriptdictionary](types/scriptdictionary.html) has values of type [script](types/script.html)
-   [objectdictionary](types/objectdictionary.html) has values of type [object](types/object.html)

You can create some types of new dictionary during the game using the relevant function:

-   [NewStringDictionary](functions/newstringdictionary.html)
-   [NewObjectDictionary](functions/newobjectdictionary.html)

To add an item to a dictionary, use the [dictionary add](scripts/dictionary_add.html) command. To remove an item, use [dictionary remove](scripts/dictionary_remove.html).

To retrieve an item for a dictionary:

-   if the type of dictionary is variable, use [DictionaryItem](functions/dictionaryitem.html)
-   for stringdictionary, use [StringDictionaryItem](functions/stringdictionaryitem.html)
-   for scriptdictionary, use [ScriptDictionaryItem](functions/scriptdictionaryitem.html)
-   for objectdictionary, use [ObjectDictionaryItem](functions/objectdictionaryitem.html)

You can see if a dictionary contains a particular key using the [DictionaryContains](functions/dictionarycontains.html) function, and get the number of items using [DictionaryCount](functions/dictionarycount.html).
