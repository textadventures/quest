---
layout: index
title: Using Display Verbs
---



When you play a text adventure using Quest, there will usually be a set of panels on the right. As well as the compass rose, there will be lists of objects in the current location and in your inventory. Or there may be hyperlinks for each object in the text. If you click on an object, buttons will appear giving short-cuts to commands with the object. These are display verbs and inventory verbs.


## Adding And Removing Verbs

There is a simple way to change the list of verbs for an object. On the _Object_ tab, at the bottom, is a section called "Display verbs". You can add and remove as appropriate. For example, if an object cannot be picked up, remove the "Take" entry from the display verbs.

By the way, you can add anything you like here, even if it makes no sense to Quest. It is, therefore, a good idea when playing through your game to click on each verb for every object to see how Quest responds (if it can be picked up, do it for both in the inventory and in the room).

Changing the object type on the _Setup_ tab will also change the verbs. Changing it to a male character, for example, will change the displayer verbs to "Look at" and "Speak to", rather than "Look at" and "Take".

When you add a verb to an object via the _Verb_ tab, Quest will automatically add that verb to both the display verbs and the inventory verb. You can stop it doing that by unticking the "Automatically generate object display verb lists" box on the _Room Descriptions_ tab (I do not know why either) of the game object. I prefer to do this, as it gives you full control over the verbs that will be shown. You can also stop verbs being generated automatically for a specific item by ticking the box on the _Object_ tab for that object.


## Adding And Removing Verbs On The Fly

So far so good, but what if you want verbs to change during the course of the game? Say there is a hat that can be worn, so you want a "Wear" verb, but when put on you want a "Remove" verb instead (actually this happens automatically as of Querst 5.7).

The verbs are held in two string list attributes, `displayverbs` and `inventoryverbs`.

There are issues to be aware of. Firstly, automatically generated verbs are not in that list (another good reason to turn the feature off).

Secondly, your object will only have those attributes if you have modified the lists on the Object tab. If you go to the Attributes tab, look for displayverbs in the list at the bottom. If it is in grey, your object is getting its list from its type, and if you try to add or remove something in the list during play, you will get this helpful message:
```
Error running script: Cannot modify the contents of this list as it is defined by an inherited type. Clone it before attempting to modify.
```


## Coding...

So how do you actually add and remove verbs? We have an object called "hat", and we want to add a "Wear" verb to the inventory list. The safe approach is to create a new list each time. This is easily done with the `Split` function. This takes two strings, the first being a list of verbs, seperated by semi-colons, the second just a semi-colon, telling Quest what to break the first list on.
```
  hat.inventoryverbs = Split("look at;Drop;Wear", ";")
```
Then when the hat is worn:
```
  hat.inventoryverbs = Split("look at;Remove", ";")
```
