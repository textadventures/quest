---
layout: index
title: Messing with the Location Bar
---

_NOTE:_ Basic knowledge of HTML will be useful here.

By default the location (or status) bar across the top of screen tells the player the current room. You can turn it off, and you can change how it looks, on the _Interface_ tab of the game object.

In HTML terms, it consists of two elements: the "location" element, which holds the text, and is updated when the player goes into the room; and the "status" element, which contains it, and to which the styling is applied.

Using JQuery we can change the location bar to display anything we want. The basic code (in JavaScript( is this:

```
$('#location').replaceWith('Some new HTML code')
```

That new HTML can include a new "location" element, in which case Quest will continue to update the location, or not if you do not want that.


Tracking Turns and Score
------------------------

A good example would be to show the score and number of turns in the top right corner, and keep the room name in the left corner. To do that, we will insert a table into the location bar, one row high, two columns wide. The first cell is called "location", so will still display the room name, the second is called "altlocation".

```
s = "<table width=\"100%\"><tr>"
s = s + "<td id=\"location\"></td>"
s = s + "<td id=\"altlocation\" align=\"right\">0/0</td>"
s = s + "</tr></table>"
JS.eval ("$('#location').replaceWith('" + s + "')")
if (HasAttribute(game, "pov")) {
  JS.eval ("$('#altlocation').html('" + game.score + "/" + game.turncount + "')")
}
```

I find it easiest to build up the string in steps so I can see it all, so the first four lines do that, the fifth line just replaces the "location" element.

The last three lines update the display to the current values. At the start of the game, those attributes do not exist, and we do not want this code to run (we only need it when the player reloads a saved game). So we check if the "pov" attribute of the game object has been set. If it has, we are loading a saved game, and need to update.

This needs to go in the "User interface initialisation script", on the _Advanced scripts_ tab of the game object (tick "Show advanced scripts..." on the _Features_ tab if you cannot see it).

If you go in game, you show see the score and turn... but it does not update.


Updating the display
--------------------

We need to first initialise the score and turn counter, and this has to be done in the start script, on the _Scripts_ tab of the game object, as we want this to happen at the start of the game, but not when a saved game is loaded:

```
game.turncount = 0
game.score = 0
```

Now add a new turn script, and tick it to be enabled at the start. Paste in this code, which will increment the turn counter, and then update the location bar.

```
game.turncount = game.turncount + 1
JS.eval ("$('#altlocation').html('" + game.score + "/" + game.turncount + "')")
```


Adding Commands
---------------

We can also add commands to the location bar. Just change the "User interface initialisation script" to this:

```
s = "<table width=\"100%\"><tr>"
s = s + "<td id=\"cmdlocation\" width=\"25%\">"
s = s + "<a onclick=\"ASLEvent(&apos;HandleSingleCommand&apos;, &apos;look&apos;);\" style=\"cursor:pointer\">LOOK</a> |"
s = s + "<a onclick=\"ASLEvent(&apos;HandleSingleCommand&apos;, &apos;wait&apos;);\" style=\"cursor:pointer\">WAIT</a></td>"
s = s + "<td id=\"location\" align=\"center\"></td>"
s = s + "<td id=\"altlocation\" align=\"right\">0/0</td>"
s = s + "</tr></table>"
JS.eval ("$('#location').replaceWith('" + s + "')")
if (HasAttribute(game, "pov")) {
  JS.eval ("$('#altlocation').html('" + game.score + "/" + game.turncount + "')")
}
```

The first line is the same as before, as are the last seven (except the "location" element is now centrally aligned). The difference is we have inserted these three lines:

```
s = s + "<td id=\"cmdlocation\" width=\"25%\">"
s = s + "<a onclick=\"ASLEvent(&apos;HandleSingleCommand&apos;, &apos;look&apos;);\" style=\";cursor:pointer;\">LOOK</a> |"
s = s + "<a onclick=\"ASLEvent(&apos;HandleSingleCommand&apos;, &apos;wait&apos;);\" style=\";cursor:pointer;\">WAIT</a></td>"
```

They add a new cell to the table, so now it has three columns. Note that `width=\"25%\"` adjusts the width of the new cell, you may want to modify that number to suit your game.

Our new table cell has two commands, LOOK and WAIT. When it is on the page, the HTML for the LOOK command will look like this:

```
<a onclick="ASLEvent('HandleSingleCommand', 'look');\" style="cursor:pointer;">LOOK</a>
```

The "onclick" attribute is an event handler; when the player clicks this element, run the JavaScript. In this case it runs the Quest JavaScript function, `ASLEvent`, which in turn will call the Quest function `HandleSingleCommand`, passing it the value "look". The "style" attribute changes the cursor to tell the player this is clickable.

When we put this code into Quest, we need to escape the double quotes, by putting a backslash before them, so Quest knows they are part of the string, not marking the end of it. We also need to escape the apostrophes so JQuery knows that _they_ are not marking the end of the string for it, and in this case we use the special HTML code `&apos;`.


Modifying the Style
-------------------

As we have not touced the "status" element, changes you make on the _Interface_ tab will still be applied. The one exception to that is the colour of the text for commands, because links always get displayed in a different colour. The simple way to handle that is to set the colour in the "style" attribute.

For example, to have it black (note the spelling of "color"!):
```
s = s + "<td id=\"cmdlocation\" width=\"25%\">"
s = s + "<a onclick=\"ASLEvent(&apos;HandleSingleCommand&apos;, &apos;look&apos;);\" style=\"cursor:pointer;color:black\">LOOK</a> |"
s = s + "<a onclick=\"ASLEvent(&apos;HandleSingleCommand&apos;, &apos;wait&apos;);\" style=\"cursor:pointer;color:black\">WAIT</a></td>"
```
