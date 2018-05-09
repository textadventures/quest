---
layout: index
title: Using Dictionaries
---

A dictionary is a data set where each entry is accessed by a string, called a key. If you think about an actual dictionary, it contains words linked to definitions. To find a specific definition, you use the word to look it up. Quest dictionaries are the same, you use a word, called a key, to look up your information, called a value.

A dictionary can never have two entries with the same key, just as a list can never have two entries with the same position. However, unlike a list, there is no order to the entries in a dictionary (in theory anyway).


As with lists, there are various types, but with dictionaries you also get a version specifically for scripts. Note that whatever the type, the key must be a string.
```
mystringdict = NewStringDictionary()
myobjectdict = NewObjectDictionary()
myscriptdict = NewScriptDictionary()
mydict = NewDictionary()
```
As with lists, you are better using a specific dictionary type, rather than a general one, if possible.

If using the off-line editor, you can add a dictionary to an object on the attributes page, but you are restricted to string and script dictionaries.



Adding and removing items
-------------------------

To add items to a dictionary, use the `dictionary add` command. To remove something from a dictionary, use `dictionary remove`. Unlike the list versions of these commands, we now need to provide a key.
```
d = NewObjectDictionary()
dictionary add (d, "table", fancy table)
dictionary add (d, "table2", fancy table)
dictionary add (d, "pov", player)
dictionary remove (d, "table")
```
As with lists, you can add the same thing as many times as you like, however, the key must be unique. If you try to add an item with a key that is already in the dictionary you will get an error. If you try to remove a key that does not exist you will again get an error.



Retrieving from dictionaries
-----------------------------

You can access an item in a dictionary using the `DictionaryItem` function. This takes the dictionary and the key as parameters. If you are using a specific dictionary type, it is again better to use the function for that type:
```
StringDictionaryItem
ScriptDictionaryItem
ObjectDictionaryItem
```
Note that Quest will throw an error if the key is not found in the dictionary, so testing before hand is usually advised.



Iterating
---------

Often you will want to go through each member of a dictionary, and as with a list we can use the `foreach` command to do this. It takes two parameters, the first being a variable to store a key in, and the second being the dictionary. It also requires a script.

This example will output each member of the dictionary. The script will be run once for each entry in the dictionary `d`, and when it runs `key` will have the key for that entry.
```
foreach(key, d) {
  msg("Entry: " + key + "=" + DictionaryItem(d, key))
}
```
Changing a dictionary whilst in a foreach loop (i.e., adding or removing entries) will cause an error.



Dictionary Contains?
---------------

Dictionaries are fussy things that will throw an error if you try to add a key that is already there, if you try to delete a key that is not, or try to retrieve a key that is not (unlike lists). The  `DictionaryContains` function, then, is extremely useful as it will tell you if the dictionary already contains the given key (there is no function that will tell you if the entry is already in the dictionary).

To check a key is not already in use before adding to the dictionary:
```
if (not DictionaryContains(dict, obj.name)) {
  dictionary add(dict, obj.name, obj)
}
```
To check a key IS already in use before removing from the dictionary:
```
if (DictionaryContains(dict, obj.name)) {
  dictionary remove(dict, obj.name, obj)
}
```
Alternatively we can use the `in` operator, which is rather less typing!
```
if (not obj.name in dict) {
  dictionary add(dict, obj.name, obj)
}
```

```
if (obj.name in dict) {
  dictionary remove(dict, obj.name, obj)
}
```

By the way, the reason there is a `DictionaryContains` function is to make it easier to use dictionaries via the GUI interface.

Other functions
---------------

The `DictionaryCount` function will return the number of entries in the dictionary.
```
msg("My dictionary has " + DictionaryCount(myDict) + " things in it.")
```


Some uses of dictionaries
-------------------------

### ShowMenu

You can use a string dictionary with the `ShowMenu` function or `show menu` command. This allows you to give a set of options, and to handle them as a corresponding set of strings. This could, for example, allow you to run a script, depending on the option chosen. Suppose "getthing", "jump" and "vomit" are all scripts on the player object:
```
options = NewStringDictionary()
dictionary add (options, "getthing", "Get the thing")
dictionary add (options, "jump", "Jump as high as you can")
dictionary add (options, "vomit", "Try to vomit the poison up")
ShowMenu ("Choose", options, false) {
  do(player, result)
}


### Script parameters

You can also use dictionaries to pass values to scripts. The key will become the name of a local variable, while the value will be its value.
```
vars = NewDictionary()
dictionary add (vars, "weapon", weapon)
dictionary add (vars, "success", "You hit the monster")
dictionary add (vars, "failure", "You missed!")
do (monster, "attack", vars)
```
In the attack script, you can now use three local variables; `weapon`, which is an object, and `success` and `failure`, two strings.
