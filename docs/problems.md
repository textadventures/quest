---
layout: index
title: Resolving Common Problems
---


Note: _This is a work in progress..._


Problems when starting Quest
----------------------------

Occasionally we hear about a situation where the desktop version of Quest will not start. It has never happened to me, which makes finding the root cause very tricky, but for whatever reason Quest has got a double entry in the list of recently played or recently edited games. You might see an error message like this:

```
System.InvalidOperationException: An error occurred while creating the form. For more
information, see Exception.InnerException. Error: An exception was thrown in the constructor
call for the type 'GameBrowser.PlayBrowser' that matches the specified binding constraint. --->
System.Windows.Markup.XamlParseException: An exception was thrown in the constructor call for
the type 'GameBrowser.PlayBrowser' that matches the specified binding constraint. ---> 
System.ArgumentException: An item with the same key has already been added.
```

Follow these steps (thanks Jay!):

-    Run "regedit" (e.g. press the Windows key and then hit "R" and then type "regedit" in the box that pops up.) If it asks to be able to make changes to the system, say "yes".
-    Under Computer in the tree on the left, look for HKEY_CURRENT_USER and open that folder.
-    Look for SOFTWARE under that and open the folder.
-    Search down for Quest under that and open the folder.

We need to delete two keys, "Recent" and "EditorRecent" (in fact you only need to delete one; the former if the play browser is a problem, the latter if the edit browser is the problem, but if you are not sure it is safest to delete both). Right-click on the word "Recent" and then choose "Delete" from the popup menu. Choose yes to confirm. Do the same for "EditorRecent". Close the Rededit program (no need to save).

Note that when you open Quest, the "recent" lists will be empty until you begin using Quest again, but that is preferable to not being to open Quest at all!



Problems when creating games
----------------------------

There are all sorts of problems that can arise as you code with Quest. Computer languagers are fussy things that expect you to type to very strict rules, and Quest is no different. Some things to check:

- Variables, attributes and objects are named consistently (if it is `hitpoints` in one place and `hit points` in another and `Hit points` in a third, it is not going to work)
- Brackets and braces need to match; if you have three open brackets and only two close brackets it is not going to work
- Quotes likewise need to quote marks at the start and end
- If a function's return type in "None" there should be no `return`; if it is not "None" then there must be
- Functions must have exactly the right number of paramters in the right order


### Understanding error messages

The error messages usually come from deep in the Quest code, where things are not quite the same. If it is talking about an object, it probably means null. For example, if an attribute has not been set (so is null), you will see this:
```
Error running script: Object reference not set to an instance of an object.
```

If it means object, it may say "element". Or object.


### Room description appears twice

This can happen if you move the player in script on the room. Say you want to turn a player back from an exit. You might think it is a good to set up the script that runs on the destination room so it moves the player back to the original room. What happens is that the player is moved twice, and so Quest thinks it has to show the room description twice - and to add to the confusion, it does it for the current room, which will be where the player ends up.

The solution is to avoid moving the player on any of the built-in room scripts. In the example above, the script should be on the exit that goes to the destination, without moving the player at all.


### Error when player moves

Occasionally you may see this error:

> Error running script: Error evaluating expression '(not GetBoolean(game.pov.parent, "visited")) and HasScript(game.pov.parent, "beforefirstenter")': GetBoolean function expected object parameter but was passed 'null'

This happens when the player's "parent" attribute is set to null, and can happen if you try to move the player to a variable that has not been set (and Quest will think an object name you have mis-spelled to be a variable).


Names you cannot use
--------------------

Sometimes Quest is clever and will warn you or take some action if you try to give something a bad name (if you try to add an object called "game" it will call it "game1"). However, there are other times it will not...

### Items/Rooms called K1, K2, etc.

Quest automatically assigns names to anything you do not name yourself (for example, most of the exits in your game will have no name). It will name the first one K1, the second K2, and so on. What this means is that if you name anyhing in your game 'K' following by a number you are in danger of having a name collision!

### 'e' and 'pi'

You will probably never need them in a text adventure, but `e` and `pi` are both mathematical constants. Quest will happily let you assign a value to them, but will ignore the assignment.

```
e = GetExitByLink (room, room2)
msg (e)
-> 2.71828182845905
```

This only applies to local variables, you can give these names to objects and attributes.

### `object`, `game`, `turnscript`, `command`, `exit`

Trying to use any of these as the name of an attribute will confuse the editor. You will not get an error, but it will not do what you expect. You can use them as local variable and object names, but I would advise against it.

### Attribute names with spaces

You can use attribute names with spaces in them for strings, number, objects and scripts, but not for lists or dictionaries.

### Other attributes

Various attributes are already used by Quest. Do not do anything with "type" or "elementtype". Obviously "name", "parent", "alias", etc. have specific meanings in Quest, and trying to use them for something elkse will cause problems.
