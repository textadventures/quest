Note: _This is only usable off-line as custom libraries are not allowed in the on-line editor._

Sometimes the existing save game mechanism is not what you want; perhaps you want a system that preserves saved game across updates to the game.

This is going to be a nightmare! Be clear on this before you start. You will need to save the state of anything that can change in your game, such as player attributes, the position of anything moveable, the state of anything that can change, such as any NPC, the visited state of every room, and perhaps more. You will need to _not_ save anything that will potentially updated (this is the issue with the existing system, it assumes anything can be changed, so saves the lot). You will also need to ensure every update can handle save games from every previous version.

Alternatively, you might want to make a series of linked games, and need a way to transfer player data from one game to another. You can use exactly the same mechanism (but do not have to worry about saving the state of every object and room, unless the player can go back to previous games).

## The Library

[SaveLoad](https://github.com/ThePix/quest/blob/master/SaveLoad.aslx)

The library adds two new commands, SAVE and LOAD.

Quest cannot save files, so the only way to do this is to present a dialog box, and ask the player to paste the code into a text file, and save that. On loading, a blank dialogue box is presented, and the text can be pasted in. The text is encrypted to stop players cheating.

After you have [added the library](https://github.com/ThePix/quest/wiki/Using-Libraries) to your game, you need to add one line to the `start` script of the game object:
```
SaveLoadInit
```
Once you have done that, you just need to add an attribute to the game object...

## The `saveloadatts` attribute

This is the tricky bit. The `saveloadatts` attribute needs to be added to the game object as a string list, and you need to add to it everything that you want saved.

Each attribute you want to save must be types in a particular format; the name of the object, the name of the attribute and the type of the attribute, each separated by a dot. For example:
```
hat.alias.string
hat.parent.object
player.achievements.list
player.strength.int
player.female.boolean
```
You can use "flag" instead of "boolean". 

Here is an example from a real game:

[[https://raw.githubusercontent.com/ThePix/quest/master/saveloadatts.png]]

### Special attributes

Your list of attributes should include `player.current_location`, but not include `player.parent`. This is to ensure the player gets moved to the location right at the end of the process, so on first enter scripts will not fire a second time.
```
player.current_location.object
```
For any object that can be moved, save the parent attribute. For an object called "teapot", it would look like this:
```
teapot.parent.object
```
If you use the `once` text processor command, you should include this attribute:
```
game.textprocessor_seen.dictionaryoflists
```
For any room that has a script that only fires the first time the player enters, you must include its visited script. If the room is called "lounge", it would look like this:
```
lounge.visited.boolean
```
Similarly, any turn script that can be turned on or off, you need to save its `enabled` attribute.

However, if a room does not have a script that only fires onfirst enter, or the turn script is on permanently, there is no need to save. You only need to save attributes that could change.

### Limitations

This cannot cope with attributes that are dictionaries or lists of objects. It cannot cope with objects that are created during game time, including cloned objects, as you will not know their names. You can still have these things in your game; you just cannot save and load them.

If you use the `firsttime/otherwise` script command, whether it has already fired cannot be saved. It is probably best avoided altogether.

## Modifying attributes

Once you release your game, you cannot modify the list of attributes. The system uses the position of a value in a string to determine the value, and if you remove an attribute from the middle of the list, all the subsequent ones will be wrong.

You can, however, _add_ to the list.

## Pre- and post-load; pre-save

Sometimes you will want to do something before and after loading, and before saving, perhaps initialising values or calculating derived attributes. For this, you can [override](https://github.com/ThePix/quest/wiki/Overriding-Functions) the `PreLoad`, `PostLoad` and `PreSave` functions (neither has any paramters or return type).

`PreLoad` fires after the game has established that some text was pasted into the load game dialogue, but before anything else.

`PostLoad` before the player is moved to the right location, but after everything else.