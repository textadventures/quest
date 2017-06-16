---
layout: index
title: Custom Command and Status Panes
---


As of Quest 5.7 you can add new panes to your game.

Custom Command Pane
-------------------

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

You can change the commands at any time in your game, just be using the `JS.setCommands` function again. If the player does sit, you might want to have a line in the SIT command like this, so now there will be a "Stand" commmand, and not "Sit":

```
JS.setCommands ("Look;Wait;Stand", "blue")
```

You might want to set the commands when the player enters and leaves a specific room, if there are commands specific to that room. This works best for simple, one-word commands, however you can use any command Quest can understand, including commands you have added yourself.


Custom Status Pane
------------------

This requires a bit more effort on your part, and knowledge of HTML and CSS will be useful.

On the _Interface_ panel of the game object, make sure "Show Panes" is ticked, then tick "Show a custom status pane". If you start your game, you should see the new pane, but it just says "Status" and does not do anything.

We need to use the `JS.setCustomStatus` function, which can take one parameter, a string to display. Here is an example:

```
JS.setCustomStatus ("You are fine!")
```

At this point, you may wonder why bother? The power of the custom status pane comes from using it with HTML. This allows you to format the string to show the information nicely. Here is an example that displays the player's status in a neat table. 

```
html = "<table><tr><td width=\"50%\">Condition:</td><td>Poisoned</td></tr><tr><td></td><td>Woozy</td></tr></table>"
JS.setCustomStatus (html)
```

In your game, you would want to re-build the string whenever a attribute changes. Here is some example code that will take a string list of conditions, called "list", and display it in a table as in the previous example:

```
html = "<table><tr><td width=\"50%\">Condition:</td><td>"
html = html + Join(list, "</td></tr><tr><td></td><td>")
html = html + "</td></tr></table>"
JS.setCustomStatus (html)
```

