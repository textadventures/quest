---
layout: index
title: Custom Command Panes
---

Quest allows the player to click and object and then click on an associated verb, without typing anything. A custom command pane allows the player to click on simple one-word commands, such as WAIT and LOOK.


Setting up
----------

On the _Interface_ panel of the game object, make sure "Show Panes" is ticked, then tick "Show a custom command pane". If you start your game, you should see the new pane, but it just says "Commands" and does not do anything.

We need to set some commands. Go to the _Features_ tab of the game object, and tick "Advanced scripts", then go to the _Advanced scripts_ tab of the game object, at the top is the user interface initialisation script. This is the place to add the script command that will set up the panel.

We need to use the `JS.setCommands` function, which can take two parameters. The first is a string of commands to display, separated by semi-colons. The second is the colour to display it in, and is optional. Here is an example:

```
JS.setCommands ("Look;Wait")
```

In this example, the colour is set too. Note that you can use any [web colour](https://en.wikipedia.org/wiki/Web_colors) or hex triples.

```
JS.setCommands ("Look;Wait;Sit", "blue")
```

Go in game, and test it works!

You can change the commands at any time in your game, just be using the `JS.setCommands` function again. If the player does sit, you might want to have a line in the SIT command like this, so now there will be a "Stand" command, and not "Sit":

```
JS.setCommands ("Look;Wait;Stand", "blue")
```


Commands for specific rooms
----------------------------

You might want to set the commands when the player enters and leaves a specific room, if there are commands specific to that room. This works best for simple, one-word commands, however you can use any command Quest can understand, including commands you have added yourself.

Let us suppose we want to add a SHOUT command to a specific room (see [here](commands_for_room.html) for the best way to do that). Go to the _Scripts_ tab of the room, and add this to the script that runs when the player enters the room (not the one that only runs the first time):

```
JS.setCommands ("Look;Wait;Shout")
```

Then for the script that runs when leaving the room:

```
JS.setCommands ("Look;Wait")
```


More advanced options?
---------------------

The custom command pane is quite specific in how it works. What if it does not work quite as you like? Perhaps you want images or want clicks to do something different.

You best option is to use a custom status pane instead. This allows you to add any HTML/JavaAScript, giving you full control on what it does and how it looks.