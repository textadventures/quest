The on-line editor is very much like the off-line editor, but with a few important differences.


## Why Use the On-line Editor?

The on-line editor is great if you want to quickly see what Quest is capable of, but if you are using a Windows PC, and want to do anything serious, the off-line editor is considerably better.

If you are not using Windows, it maybe that you can find a Windows emulator on run on your system, but you may well be stuck with the on-line version.


## So what are the differences?

The attributes tab allows you to readily set attributes, status attributes and types. We will start with them.


## Attributes

Everything you set for an object or room is an attribute. The built-in ones are text field, tick boxes, etc. on each tab, but you can set as many other as you like. Off-line you can do that on the Attributes tab. On-line, there is no Attributes tab; you will need to set them the initialisation script of the object (turn the tab on on the _Features_ tab). It is not a convenient, but easily doable.

To add scripts, you can use the `->` operator, by the way.


## Status Attributes

Status attributes are the attributes whose values appear in a box on the right side during the game. They can be set up very easily off-line, using the Attributes tab. If you are on-line, you will need to create the dictionary yourself, and then add the attributes to it in the game start script. Here is an example:
```
  player.strength = 2
  player.statusattributes = NewStringDictionary()
  dictionary add(player.statusattributes, "strength", "Strength: !")
```
There is a corresponding dictionary for attributes on the game object.


## Verb Definitions

You may not realise it, but there are verb objects as well as verb attributes. The verb objects define how the verb works; what synonyms can be used, the response when an object is not recognised. This gets done automatically in both versions, but in the off-line version, you can edit them later. Very important if you make a mistake!

If using the on-line version, you may be better off using only commands, and avoiding verbs.


## Types

You cannot create custom types on-line. Even if you could, the absence of the Attributes tab means you could not assign them to objects or assign attributes to them.

The work-around is to not use types, and most people do not, even with the off-line editor.


## No Libraries

You cannot add new libraries to your game on-line. As a creator of libraries, I think that is a big draw back. If the library is just functions and commands, you can copy each of them into your game file, but most (possibly all) have custom types, so that is not going to work.


## No Full Code View

With the online editor, you can see a code view for individual scripts, but not for the whole game. Seeing the whole code is useful when creating custom types and custom libraries, neither of which you can do anyway. Other than that, it is a good way to mess up your code, so really this is not a drawback.


## No Overriding Functions (and Commands?)

Overriding is replacing one thing with some with the same name, but a different script. Not a problem for commands, give it a different name, but the same matching pattern

This is, however, a big deal for functions. You may not find you want to do it that often, but it can be useful in some situations.


## No Walk-Throughs

The walk-through facility offers a quickly way to get through the first half of your game so you can test the second half. You have no walk-throughs in the on-line editor, but you could create a command to jump you to the right place, though you would need to be careful to set any applicable attributes that would be set by that point.


## No Custom Aliases For Exits

In the off-line editor, you can type anything into the alias box of an exit, rather than selecting a compass direction. The exit will then appear in the object list, and can be clicked on to take the player to the exit's destination.

On-line, that is not an option; the control does not allow typing your own alias. The work around here is to give the exit a name, and then to assign an alias to it in the game start script (as with Attributes above). This has the potential to be a nightmare if you have more than a handful.


## Spell checking

Where the web version is definitely better is that your text will get spell-checked as you type (as long as your browser supports it, and most do). This is a big advantage as text adventures by their nature involve a lot of text.