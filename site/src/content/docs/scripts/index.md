---
title: Script commands
sidebar:
  order: 5
---

Scripts are created in a style similar to C, with script blocks denoted by braces. Unlike C, there is no character to mark the end of a line - each script command is simply on its own line.

```quest
if (someVariable = 3) {
  msg ("Some text")
}
```

Comments are denoted by //

```quest
// this line will be ignored
```

## Setting variables

To set an object attribute to a value:

```quest
object.attribute = value 
```

To set a variable to a value:

```quest
variable = value 
```

To set an object attribute to a script:

```quest
object.attribute => { script } 
```

To set a variable to a script:

```quest
variable => { script } 
```

## ask
```quest
ask (string question) {script}
```

Pops up a prompt for the user to choose Yes or No as the answer to the specified question, and then runs the nested script.

The nested script can check the "result" boolean variable to see the user's response - true for "yes", false for "no".

```quest
ask ("Do you want to eat an apple?") {
  if (result) {
      msg("Ahhh, very tasty")
  } else {
      msg("But you should eat your daily apple!")
  }
}
```

## create
```quest
create (string name)
```

or

```quest
create (string name, string type)
```

Creates an object with the specified name. You can subsequently access the object using the [GetObject](/functions/objects#getobject) function, or just use its name directly in an expression.

If you specify a type, the object created will be of that type. The command only accepts one type name - if you want the new object to inherit multiple types, you could create one type which inherits all of those types, and specify that here.

## create exit
```quest
create exit (string alias, object from, object to)
```

or

```quest
create exit (string alias, object from, object to, string type)
```

or

```quest
create exit (string name, string alias, object from, object to, string type)
```

Creates an exit with the specified alias (usually the direction, such as "north") between two objects/rooms.

An initial type can be specified e.g. "northdirection". This will ensure that the correct [alt](/attributes#alt) names are applied to compass exits.

```quest
create exit ("northwest", fromRoom, toRoom, "northwestdirection")
```

You can also specify the object name to use. If not specified, an id will be automatically generated.

```quest
create exit ("exit_to_garden", "northwest", fromRoom, toRoom, "northwestdirection")
```

It is usually easier to make an exit in the normal way in the editor, but to set it so it is not visible; instead of then creating an exit during game play, you set this exit to be visible.

## create timer
```quest
create timer (string name)
```

Creates a timer with the specified name. You can then use `GetObject` to get the timer, and assign values to it. Here is a trivial example that will produce a timer that will tell you its name every 10 seconds:

```quest
create timer ("test_timer")
o = GetTimer ("test_timer")
msg (TypeOf(o))
o.script => {
  msg ("timer=" + this.name)
}
o.interval = 10
EnableTimer(o)
```

It is generally easier to create the timer in the editor, but have it disabled, and then enable it when required.

## create turnscript
```quest
create turnscript (string name)
```

Creates a turnscript with the specified name. You can then use `GetObject` to get the turn script, and assign values to it. Here is a trivial example that will produce a turnscript that will tell you its name every turn:

```quest
create turnscript ("test_ts")
o = GetObject("test_ts")
o.script => {
  msg ("turnscript=" + this.name)
}
o.enabled = true
```

It is generally easier to create the turn script in the editor, but have it disabled, and then enable it when required.

## destroy
```quest
destroy (string name)
```

Destroys the specified object. Note that this takes the object's name, not the object itself, as a parameter.

## dictionary add
```quest
dictionary add (dictionary, string key, any type item)
```

Adds an item to the specified dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## dictionary remove
```quest
dictionary remove (dictionary, string key)
```

Removes the specified item from the dictionary.

See [Using Dictionaries](/howto/scripting/using_dictionaries)

## do
```quest
do (object, string attribute name)
```

Runs an object's script attribute.

```quest
do (object, string attribute name, dictionary parameters)
```

Runs an object's script attribute, passing in parameters via dictionary. The key/value pairs in the dictionary will be turned into local variables for the script. The special variable "this" can be used in the script to reference the object.

## error
```quest
error (string message)
```

Stops running the current script and raises the specified error message.

## finish
```quest
finish
```

Finish the game.

## firsttime
```quest
firsttime { script1 } [ otherwise { script2 } ]
```

runs **script1** if it is the first call, otherwise **script2** is executed

## for
```quest
for (iterator variable, int from, int to) { script }
```

There is an optional "step" parameter:

```quest
for (iterator variable, int from, int to, int step) { script }
```

Run a script multiple times, incrementing the iterator variable between the specified limits. If a "step" parameter is specified, the iterator variable will be incremented by that amount each time (if not specified, the default step size is 1).

Trandionally, i, j, k... are used as iterator varable names. This simple example runs from 1 to 5, printing each value in turn:

```quest
for (i, 1, 5) {
  msg(game.i)
}
```

Generally, `foreach` offers a neater way of going through a list, but `for` can be useful for iterating through a string. This example will print each character in the string, together with its position:

```quest
s = "Hello World!"
for (i, 1, LengthOf(s)) {
  msg(i + ": " + Mid(s, i, 1))
}
```

_Note:_ The iterator variable should be a local variable, not an attribute. For example, consider this code, which uses an attribute of the game object:

```quest
for (game.i, 1, 5) {
  msg(game.i)
}
```

If `game.i` already exists, the loop will run 5 times as expected, but the value of `game.i` will keep its original value. If  `game.i` does not exist, an error will be produced.

See [Using Lists](/howto/scripting/using_lists)

## foreach
```quest
foreach (iterator variable, list) { script }
```

Run a script for each item in a list. If the list is a dictionary, the loop iterates over the dictionary keys.

_Note:_ Do not use an attribute as the iterator variable (see [here](#for)).

For more on how and why to use `foreach`, see [Using Lists](/howto/scripting/using_lists)

## get input
```quest
get input {script}
```

Waits for the user to type some text, then runs the nested script.

The nested script can evaluate the "result" string variable to work with the user's input.

Example:

```quest
msg ("What is your name?")
get input {
   msg ("Your name is " + result)
}
```

For more information see [here](/howto/tasks/asking_a_question).

## if
```quest
if (boolean expression) { script } [ else if ... ]* [ else { script } ]
```

Conditionally runs the script. If the condition fails, the `else` script is run, if present. Multiple `if/else`s can be put together. Some examples:

```quest
if (result > 10) {
  msg("Great!")
}
```
An `else` can be added (no need for a condition)
```quest
if (result > 10) {
  msg("Great!")
}
else {
  msg("Rubbish!")
}
```
Or we can have a condition; nothing gets printed if result is between 2 and 10.
```quest
if (result > 10) {
  msg("Great!")
}
else if result < 2) {
  msg("Rubbish!")
}
```
You can have as many `if/else` linked together as you need (but consider using [switch](#switch)).
```quest
if (result > 10) {
  msg("Great!")
}
else if result > 2) {
  msg("Meh...")
}
else {
  msg("Rubbish!")
}
```

Complex conditions can be used with Boolean arithmetic. 

```quest
if (result > 10 and not player.is_female) {
  msg("Good boy")
}
```

## insert
```quest
insert (string filename)
```

Outputs the contents of the specified HTML file.

**Not supported in Quest 5.4 or later.**

## invoke
```quest
invoke (script)
```

Runs a script.

```quest
invoke (script, dictionary parameters)
```

Runs a script, passing in parameters via dictionary. The key/value pairs in the dictionary will be turned into local variables for the script. See also the [do](#do) script command.

## list add
```quest
list add (list, any type item)
```

Adds an item to a list.

See [Using Lists](/howto/scripting/using_lists)

## list remove
```quest
list remove (list, any type item)
```

Removes an item from a list.

See [Using Lists](/howto/scripting/using_lists)

## msg
```quest
msg (string message)
```

Prints the specified text.

## on ready
```quest
on ready { script }
```

Runs the nested script when any callbacks have finished.

For example, when you use an [ask](#ask) or [get input](#get-input) script command, Quest will wait for a response from the player and then run the nested scripts from those commands. However, any other scripts at the same level will run immediately. If you don't want this to happen, use "on ready" to make the script only run after the user has entered a command or responded to the question.

This is used by the Core library so that, for example, a room description is only displayed after any scripts which ask a question in "before enter" have run their nested scripts. This prevents the room description from being displayed while the question is still on-screen.

Generally there should be no need to use this command in your own games, as of course if you want script to run after an "ask", you can just put it inside the "ask" script block.

Note that this does not wait for scripts attached to functions to work (such as `Ask` and `ShowMenu`). see [here](/howto/scripting/blocks_and_scripts)

## picture
```quest
picture (string filename)
```

Outputs the specified picture file.

## play sound
```quest
play sound (string file, boolean wait, boolean loop)
```

Plays a sound file (WAV or MP3 format), which must be in the same directory as the game file. If the parameter **wait** is "true", the script will stop until the sound has finished. If the parameter **loop** is "true", the sound will loop.

## request
```quest
request (request name, string parameter)
```

Raises a UI request. The request name must be specified directly - it is not a string expression. For example:

```quest
request(UpdateLocation, "The Kitchen")
```

The `request` script command is really a throw-back to the original Quest 5.0 interface, which, while it did use HTML, was not a fully-fledged browser. As of 5.3, the interface is a version of Chrome embedded in the software, and all interaction between the game world and the interface is done with JavaScript. Since then `request` has become increasingly obsolete, and it is recommended that the alternative is used. It is just possible `request` will be taken out of Quest at some date.

Valid request names, what they do, and their modern alternative:

| Request name | Effect | Use instead |
|---|---|---|
| `Background` | Sets the background to the specified HTML colour. | [SetBackgroundColour](/functions/user-interface#setbackgroundcolour) |
| `ClearScreen` | Clears the screen. Parameter is ignored. | [ClearScreen](/functions/user-interface#clearscreen) |
| `Foreground` | Sets the foreground to the specified HTML colour. | [SetForegroundColour](/functions/user-interface#setforegroundcolour) |
| `GameName` | Sets the name of the game. | [JS.setGameName(name)](/js/#setgamename) |
| `Hide` | Turns off an interface element. | [JS.uiHide(...)](/js/#uihide) |
| `LinkForeground` | Sets the link foreground to the specified HTML colour. | [SetLinkForegroundColour](/functions/internal-core#setlinkforegroundcolour) |
| `Log` | Logs the specified text. | [Log](/functions/general#log) |
| `PanesVisible` | Shows/hides the side panes. "on"/"off" toggle them; "disabled" turns them off and removes the button to turn them back on (that button appears to no longer be available). | [JS.panesVisible(true / false)](/js/#panesvisible) |
| `Pause` | Pauses the game for the specified number of milliseconds. | — |
| `Quit` | Quits the game. Parameter is ignored. | [finish](#finish) |
| `RequestSave` | Requests the UI to save the game (may prompt a "Save As" dialog). Parameter is ignored. | `requestsave()` |
| `RunScript` | Runs the specified JavaScript function. | the [JS](/js/) object, e.g. `JS.myCustomFunction(15, "some string")` |
| `SetCompassDirections` | Assigns compass direction names from a semicolon-separated list. | [JS.setCompassDirections(...)](/js/#setcompassdirections) |
| `SetInterfaceString` | Sets UI text via an `"ElementName=Value"` parameter. | [JS.setInterfaceString(...)](/js/#setinterfacestring) |
| `SetPanelContents` | Sets the static panel HTML contents. | [SetFramePicture](/functions/user-interface#setframepicture) and [ClearFramePicture](/functions/user-interface#clearframepicture) |
| `SetStatus` | Sets the status area text (right of screen, under "Inventory"); blank removes it. | [status attributes](/status_attributes) |
| `Show` | Turns on an interface element ("Panes", "Location" or "Command"). | [JS.uiShow(...)](/js/#uishow) |
| `ShowPicture` | Shows the specified picture file from the game directory. | [picture](#picture) |
| `UpdateLocation` | Updates the location bar with the parameter text. | [JS.updateLocation(location)](/js/#updatelocation) |
| `Wait` | Waits for the player to press a key. Parameter is ignored. | [wait](#wait) script command |

`FontName` and `FontSize` aren't listed above: they now raise an error rather than do anything, so use [SetFontName](/functions/user-interface#setfontname) and [SetFontSize](/functions/user-interface#setfontsize) instead.

## return
```quest
return (any type result)
```

Sets the return value of a function, and stops execution of the function immediately.

This command should only be used within a [\<function\> element](/elements#function).

## rundelegate
```quest
rundelegate (object, string attribute name, any type parameters ... )
```

Runs an object's delegate implementation script attribute, with the specified parameters.

See [Using delegates](/advanced-topics/using_delegates)

## set
```quest
set (object, string attribute name, any type value)
```

Sets a named attribute on the object.

Note that you can also use this syntax to do the same thing:

```quest
object.attribute = value 
```

You only need to use the "set" command if you are constructing the attribute name using an expression.

## show menu
```quest
show menu (string caption, stringdictionary or stringlist options, boolean allow cancel) {script}
```

Shows a popup menu of options and then runs the nested script. The script can access the variable "result" which contains the result of the user selection - if a dictionary of options is passed in, the key is returned. If a list of options is passed in, the list item is returned.

If the "allow cancel" parameter is set to **true**, the Cancel button is available. If "cancel" is pressed, the variable "result" returns [null](/types#null).

For an in-line menu, use the [ShowMenu](/functions/user-interface#showmenu) function.

**example:**

```quest
menulist = NewStringList()
list add (menulist, "first entry")
list add (menulist, "second entry")
list add (menulist, "third entry")
show menu ("please choose now", menulist, true) {
  msg ("--" + result + "--")
  if (result<>null) {
     msg ("You have chosen the " + result)
  }
  else {
     msg ("You have chosen to press cancel")
  }
}
```

## start transaction
```quest
start transaction (string command)
```

Starts a transaction in the undo-logger for the specified command, and ends the previous transaction (if one was open).

## stop sound
```quest
stop sound
```

Stops playing sounds.

## switch
```quest
switch (any type value) { case (any type value) { script } [ default { script } ] }
```

Switch is used with one or more `case` statements and an optional `default` statement. It is used to test a variable or object attribute against 2 or more possible values; a shortcut instead of writing many `if` statements. 

For more, see [here](/howto/tasks/multiple_choices_using_a_switch_script)

## undo
```quest
undo
```

Moves the game state backwards one transaction.

## wait
```quest
wait {script}
```

Waits for the user to press a key or click on a "Continue" link, and then runs the nested script. Each successive part needs to be nested inside the one before, like this:

```quest
msg ("First bit")
wait {
  msg ("Second bit")
  wait {
    msg ("Third bit")
  }
}
```

## while
```quest
while (expression) { script }
```

Run a script while the given expression returns true.
