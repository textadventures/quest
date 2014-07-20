---
layout: index
title: Using Javascript
---

<div class="alert alert-info">
Note: The Javascript editor is currently only available in the Windows desktop version of Quest.

</div>
<div class="alert alert-info">
Note: This page is out of date - it contains references to an example source code file which is not part of the current code.

</div>
Introduction
------------

Quest's interface is customisable with the power of HTML and Javascript. If you don't like Quest's standard layout, you can create your own - create the interface using HTML, and use Javascript as the bridge between your interface and your game.

For an example, look at [the latest code on CodePlex](http://quest.codeplex.com/SourceControl/BrowseLatest). Navigate to WebPlayer/Examples/twohalves.

The current versions at the time of writing this article are:

-   [twohalves.aslx](http://quest.codeplex.com/SourceControl/changeset/view/2ac3e5c8eff3#WebPlayer%2fExamples%2ftwohalves%2ftwohalves.aslx) - game file
-   [twohalves.htm](http://quest.codeplex.com/SourceControl/changeset/view/2ac3e5c8eff3#WebPlayer%2fExamples%2ftwohalves%2ftwohalves.htm) - HTML interface
-   [twohalves.js](http://quest.codeplex.com/SourceControl/changeset/view/2ac3e5c8eff3#WebPlayer%2fExamples%2ftwohalves%2ftwohalves.js) - Javascript bridging the ASLX and HTML.

HTML
----

To embed the HTML, the [insert](../scripts/insert.html) script command is used in the game's start script.

The HTML file is just a snippet - it should not include surrounding

<html>
tags, as the file will be inserted into the existing HTML output. You can define CSS styling in a \<style\> tag.

Javascript
----------

All Javascript references must be included using a [javascript element](../elements/javascript_element.html). You can add these in the Editor by going to "Javascript" under the "Advanced" section in the tree. The Editor features a built-in file editor so you can edit your .js file directly.

Turning off standard interface elements
---------------------------------------

If you're using a custom HTML/JS interface, you may want to get rid of some or all of Quest's standard interface elements. You can turn off the panes on the right of the screen by going to "game" in the tree, then on the Display tab turn off the "Show panes" option.

If you want to go even further, you can turn off the Location bar at the top of the screen, and even the input textbox at the bottom of the screen. See twohalves.aslx for an example - all you need to do is call the [request](../scripts/request.html) command in your start script to turn these elements off.

Sending messages from the game to Javascript
--------------------------------------------

So, we have our interface set up - now we just need to get the game to talk to our Javascript. In twohalves.aslx, we have a special Print function that takes two parameters - the first parameter is either 1 or 2, to specify which half of the screen to print on. The second parameter is the text to print.

This works by calling a SetSide function, also defined in the ASLX file. This function is where we call the Javascript in our .js file, and we do this using the [request](../scripts/request.html) script command:

     request (RunScript, "setCurrentSide; " + side)

This is how we can call Javascript functions directly from our Quest game. In this case we calling the setCurrentSide function defined in twohalves.js, and passing it one parameter, the value of our "side" variable.

(Internally in twohalves.js, we override the addText function. The default addText function is defined in playercore.js, and is called by Quest to print text to the screen, so by overriding it, we gain full control over Quest's output).

Sending messages from Javascript to the game
--------------------------------------------

What about in the other direction? The twohalves.aslx example not only overrides Quest's output, but it also handles input itself.

To do this, the textboxes it defines in the HTML call an inputKeydown function when a button is pressed. When the user presses Enter, this function sends a message to Quest via this line of Javascript:

     ASLEvent("ProcessInput", id + ";" + text);

This is Javascript calling a ProcessInput function which is defined in twohalves.aslx. It passes two parameters - an id of 1 or 2 to indicate which of the two textboxes was used, and the text entered.

In the twohalves.aslx file, this function makes a note of which side was used, and then calls Core.aslx's [HandleCommand](../functions/corelibrary/handlecommand.html) function to handle the player's command in the usual way.
