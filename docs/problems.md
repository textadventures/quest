---
layout: index
title: Resolving Common Problems
---


Note: _This is a work in progress. It does not cover all errors, but is slowly getting closer to that state..._


Problems when installing Quest
----------------------------

Quest can only be installed on Windows. Note that more recent versions of Quest will not run on Windows XP (since Quest 5.6 I think, and since 5.7 it should even warn you if you try to install on Windows XP).

If Quest fails to install properly, uninstall it (if applicable), restart your PC, then re-install.

Try installing `vcredist_x86.exe` from [here](https://www.microsoft.com/en-gb/download/details.aspx?id=30679).

Check you have .NET 3.0, 3.5 and 4.0 installed properly.


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

If you have never got the current version of Quest to run, then see also the section on problems with installing Quest, as it could be that Quest did not install properly.

If instead you see this:

```
System.NotSupportedException: No imaging component suitable to complete this operation was found. ---> System.Runtime.InteropServices.COMException: The component cannot be found. (Exception from HRESULT: 0x88982F50)
   --- End of inner exception stack trace ---
   at System.Windows.Media.Imaging.BitmapDecoder.SetupDecoderFromUriOrStream(Uri uri, Stream stream, BitmapCacheOption cacheOption, Guid& clsId, Boolean& isOriginalWritable, Stream& uriStream, UnmanagedMemoryStream& unmanagedMemoryStream, SafeFileHandle& safeFilehandle)
   at System.Windows.Media.Imaging.BitmapDecoder.CreateFromUriOrStream(Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, RequestCachePolicy uriCachePolicy, Boolean insertInDecoderCache)
   at System.Windows.Media.Imaging.BitmapImage.FinalizeCreation()
   at System.Windows.Media.Imaging.BitmapImage.EndInit()
...
```

It could be that there is a dodgy image in the folder. Go to the game folder (it will have the name of your game, inside "Quest games" folder), and move out every file except the one that ends .aslx (or the one with the Quest icon), then try opening again. If it opens, one of the files you moved was to blame. Move a few back at a time to see which one. Note that Quests will try to open _all_ the image files in the folder when you open your game, whether used or not.



Problems when creating games
----------------------------

There are all sorts of problems that can arise as you code with Quest. Computer languagers are fussy things that expect you to type to very strict rules, and Quest is no different. Some things to check:

- Variables, attributes and objects are named consistently (if it is `hitpoints` in one place and `hit points` in another and `Hit points` in a third, it is not going to work)
- Brackets and braces need to match; if you have three open brackets and only two close brackets it is not going to work
- Quotes likewise need to quote marks at the start and end
- If a function's return type in "None" there should be no `return`; if it is not "None" then there must be
- Functions must have exactly the right number of paramters in the right order


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

This only applies to local variables, you can give these names to attributes.

### `object`, `game`, `turnscript`, `command`, `exit`,`type`

Trying to use any of these as the name of an attribute will confuse the editor. You will not get an error, but it will not do what you expect when you save your game and then load it (whether during player or when editing). The problem is that these all have special meaning for Quest when it is loading XML files, and it will, for example, assume your "object" attribute is a real object.


### Attribute names with spaces

You can use attribute names with spaces in them for strings, number, objects and scripts, but not for lists or dictionaries.

### Other attributes

Various attributes are already used by Quest. Do not do anything with "type" or "elementtype". Obviously "name", "parent", "alias", etc. have specific meanings in Quest, and trying to use them for something else will cause problems.



Understanding runtime error messages
----------------------------

Runtime errors occur when playing the game. Quest has tried to run a script, and realised there is an issue. You will get an error in the game output that will usually consist of two parts. Here is an example:

```
Error running script: Error compiling expression 'game.myflag': RootExpressionElement: Cannot convert type 'Object' to expression result of 'Boolean'
```


### Locating the error

The first part is in this format:

```
Error running script: Error compiling expression '[whatever]':
```

The `[whatever]` is the important part, as that is the code that Quest cannot understand.

If you are using the desktop version, copy the bit inside the single quotes (without the quotes) and go to _Tools - Code view_, press [Ctrl]-F, and paste in the text you just copied. Now you can search your game to quickly locate the code. Bear in mind that the same text could be at several places in your game, and some may be okay, so check each occurance.

If using the web version, it is not as easy, but you could download your game, open it in a text editor and then search for the error. You would have to then correct the on-line version, and it still may not be clear how to do that.

Either way, it may be easier to check any scripts you have changed recently, and see if the text is there using "Code view" for each script.


### Correcting the error

The second part of the message indicates what the error actually is.

```
RootExpressionElement: Cannot convert type '[something]' to expression result of 'Boolean'
```

This occurs when the text inside an `if` condition, does not work out to a Boolean. If `[something]` is "Object", then the code has resulted in `null`, and may be because an attribute does not exist (or has been spelled wrongly).

```
ArithmeticElement: Operation 'Add' is not defined for types 'Int32' and 'Object'
```

This could be 'Subtract' or whatever, and the types may be different. Again "Object" indicates `null`, and this is telling you that you are trying to add (or whatever) a number and null. Again, this is probably because an attribute does not exist or has been misspelled.

```
Unknown object or variable '[something]'
```

In this case, Quest has found `[something]` in a script, but has no idea what it is. It could be an object that you misspelled or a local variable that you have not given a value to yet.



### Errors calling functions

If the function name is wrong, you will get something like this:

```
Error running script: Error compiling expression 'msg2("some text")': FunctionCallElement: Could find not function 'msg2(String)'
```

If you have the wrong number of arguments, you might one of these (first is for hard-coded functions):

```
Error running script: Expected 1 parameter(s) in script 'msg("some text", "more text")'
Error running script: Too many parameters passed to OutputText function - 2 passed, but only 1 expected
```

If you try to set a value from a function that does not return a type, you might see one of these:

```
Error running script: Error compiling expression 'msg("some text")': FunctionCallElement: Could find not function 'msg(String)'
Error running script: Error compiling expression 'OutputText("some text")': Value cannot be null.Parameter name: key
```

For hard-coded functions, you will get an error if you do not set a value and it has a return type, and if you send it the wrong type in the parameters:

```
The following errors occurred: Error: Error adding script attribute 'start' to element 'game': Function not found: 'GetBoolean'
Error running script: Error evaluating expression 'GetBoolean("other text", "some text")': GetBoolean function expected object parameter but was passed 'other text'
```

### Error modifying the content of a list

You might see this error when you change the display or inventory verbs of an object during a game (it is possible with other lists too):

```
Error running script: Cannot modify the contents of this list as it is defined by an inherited type. Clone it before attempting to modify.
```

Somewhere in your game you will have a line like one of these:

```
list add (sword.inventoryverbs, "Equip")
list remove (hat.displayverbs, "Flatten")
```

The problem is that the two list attributes, "inventoryverbs" and "displayverbs" are set on the object's type, not on the object itself (if you are using the desktop version, go to the _Attributes_ tab, and check its source). You cannot modify the listwhen it belongs to the type.

There are two solutions. The easiest is to add something to the the list in the editor (bottom of the __ tab). That will add the list attribute to this object. You can then delete the entry; once the attribute is on your object, it is there.

Alternatively, you can gve the object a new list. The `Split`function offers an easy way to do that:

```
sword.inventoryverbs = Split("Look at;Take;Equip", ";")
```


### Error using the `do` command

If the attribute is missing or not a script, this rather misleading error message will be given:

```
Error running script: Object reference not set to an instance of an object.
```

Note that there maybe other issues that will also cause this error.