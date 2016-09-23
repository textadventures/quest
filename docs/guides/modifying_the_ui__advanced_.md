---
layout: index
title: Modifying the UI (Advanced)
---

Quest sets up the User Interface (UI) in the InitInterface function, which is defined in Core.aslx. The last thing it does is call another function, InitUserInterface, which is empty by default. The best way to modify the UI, then, is to define your own InitUserInterface function.

The big advantage of doing it this way is that this will be called whenever Quest thinks the interface needs updating, which is not just at the start of the game (for example, when the screen is cleared). You also get the bonus of having all your interface stuff in the same place, which keeps it organised.

Unfortunately, this will not work if you are editing on-line, as the on-line editor will not let you create a function with the same name as one already in the game. The moral here is that if you want to do fancy stuff, use the off-line editor! If you are editing on-line, put the code in the start script on the game object, and make sure you do not clear the screen.

To create the function, right click in the left pane, and select "New function". Call it "InitUserInterface" (no quotes, no spaces, exact capitalisation).


Rename the interface features
-----------------------------

*As of Quest 5.6, this seems to be broken, but should be fixed in 5.7.*

You can rename the features of the interface using the *request* script command. It takes two parameters, the first is SetInterfaceString (note that this is not a string, so has no quote marks), the second is the instruction, i.e, the name of the feature, an equals sign and the new text. That is a string, so needs to be in double quotes. Like this:

    request (SetInterfaceString, "InventoryLabel=You are holding")
    request (SetInterfaceString, "StatusLabel=How you are doing")
    request (SetInterfaceString, "PlacesObjectsLabel=Things you can see")
    request (SetInterfaceString, "CompassLabel=Directions you can go")
    request (SetInterfaceString, "InButtonLabel=In")
    request (SetInterfaceString, "OutButtonLabel=out")
    request (SetInterfaceString, "EmptyListLabel=Stuff all")
    request (SetInterfaceString, "NothingSelectedLabel=-")
    request (SetInterfaceString, "TypeHereLabel=Now what?")
    request (SetInterfaceString, "ContinueLabel=Just press a button to get on with it")

Not sure when you see EmptyListLabel or NothingSelectedLabel, but the rest are obvious enough.

You could also do this using templates, by the way.


Change the look and feel with built-in functions
------------------------------------------------

There are some built-in functions to change the UI.

    SetFramePicture (filename)
    ClearFramePicture
    SetBackgroundOpacity (opacity)
    SetBackgroundColour(string colour)
    SetBackgroundImage (filename)
    SetAlignment(string alignment)
    SetFontName
    SetFontSize
    SetForegroundColour
    SetWebFontName

Then there are a set of options available through the request script command, allowing you to turn on or off the location at the top, the panes at the right and the command bar at the bottom (two slashes together indicates a comment, by the way, and causes Quest t ignore the rest of the line).

    // The panels on the right
    request (Show, "Panes")
    request (Hide, "Panes")
    // The text input
    request (Show, "Command")
    request (Hide, "Command")
    // the status bar at the top
    request (Show, "Location")
    request (Hide, "Location")

Most of these can be done by setting options on the game object, and that is generally a better way to do, unless you want to make changes during play.


Using the JS object
-------------------

The JS object (JavaScript) has a bunch of functions (or methods, they should probably be called).

    JS.setGameWidth(width)
    JS.hideBorder()
    JS.showBorder()
    JS.setGamePadding(top, bottom, left, right)
    JS.addExternalStyleheet(stylesheet)
    // I think the stylesheet should be a file name (or perhaps a URL) as the parameter
    JS.SetMenuBackground(colour)
    JS.SetMenuForeground(colour)
    JS.SetMenuHoverBackground(colour)
    JS.SetMenuHoverForeground(colour)
    JS.SetMenuFontName(fontname)
    // these refer to the menu that appears when the player clicks on a hyperlink in the text
    JS.SetMenuFontSize(size)
    // the size must be a string that is a number followed by "pt"
    JS.TurnOffHyperlinksUnderline()
    JS.TurnOnHyperlinksUnderline()


Using JavaScript
----------------

When you have exhausted those possibilities you are delving into the murky world of CSS and JQuery, via the eval method of JS. To understand how that fits together, we will look at each technology in turn.


Using CSS
---------

Cascading style sheets (CSS) is the primary way for web pages to define the style, as opposed to the content; what font to use, colours, etc. An example might looks like this:

    #gameBorder {
      background-color: #800080;
    }

The first line determines what is controlled - in this case an element with the ID gameBorder (the # indicates an ID rather than a class or element type). The second line defines the settings. There can be several lines, before we get to the close brace (this is the conventional way to layout CSS). For the second line, there are two parts, the name, in this case "background-color", and the value, "#800080" (which is a dark magenta).

In summary, then this CSS code will set the background colour of something with the ID "gameBorder" to be dark magenta.


Using JQuery
------------

Static web pages use CSS like that, but it you want things to change, you need JavaScript, and JQuery is a quick way to access an element in JavaScript. To do the above in JavaScript/JQuery, you would do this:

    $('#gameBorder').css('background-color', '#800080');

Notice that all the same information is there, just arranged differently, according to the syntax of JavaScript/JQuery. The $ at the start signifies this is JQuery, and $('#gameBorder') will grab the thing with the ID "gameBorder" (again, the # indicates this is an ID). Once we have that we call a method (function) called "css", and send it two parameters, the thing we want to change and the new value.


Using JQuery in Quest
---------------------

To do it is Quest, you have to send that as a string to the JS.eval function:

    JS.eval("$('#gameBorder').css('background-color', '#800080');")

Once you have that template, you can change a shed load of setting, you just need to know what each bit (like "gameBorder") is called, what the CSS property (like "background-color") is called and what value (like "#800080") is allowed. Simple...


Elements
--------

Bits of an HTML page are called elements, and "gameBorder" is just one of them. All HTML documents have an "html" element that contains everything else, and inside that it has a "head" and a "body" elements. Quest then has a few dozen elements that make up the interface inside the "body" element.

You can look at those elements as you play a game. In the off-line editor, click on HTML Tools (on-line, your browser will probably have the facility to do this too). On the left you will see a hierarchy of elements (you will need to expand them to see them all), and on the right a list of properties. Click on an element, and it will be highlighted in your game so you can see what it refers to.

Most of the interesting elements are of the type "div", and each is identified by an "id". The gameBorder one looks like this:

![](html-tools.png "html-tools.png")


CSS Properties and Values
-------------------------

There are a large number of CSS properties, to get a full list, use the internet. I will mention some of the interesting ones. You do need to be careful that you supply the right type of value, but we will look at that too. Also, be aware that CSS uses America spelling for "center" and "color".


###The color property

The colour of text is determined by the "color" property. You can set colours in a number of ways, the easiest is to use a name. This Wiki page has a full list of available names (note that there are no spaces in the name; for once, capitalisation does not matter):

[http://en.wikipedia.org/wiki/Web_colors](http://en.wikipedia.org/wiki/Web_colors)

   JS.eval("$('#gameBorder').css('color', 'blueviolet');")

You can also set colours by using the RGB code. These both set the colour to red.

   JS.eval("$('#gameBorder').css('color', 'rgb(255, 0, 0)');")
   JS.eval("$('#gameBorder').css('color', '#ff0000');")


Each splits colours in to three components: red, green, blue. In the first, each component is a number from 0 to 255. In the second, it is a hexadecimal number from 00 to ff. If you do not know what hexadecimal is, use the other format.


###The background-color property

This works just the same as color, but changes the background for this element.

   JS.eval("$('#gameBorder').css('background-color', 'blueviolet');")


###The background-image property

In theory, you should be able to set the background image for each element, but I have not got that to work. You can set it for the entire page using the SetBackgroundImage function. If anyone can do it with CSS/JQuery, let me know how!

The status bar at the top uses an image. If you want to stop that image displaying, do this:

   JS.eval("$('#status').css('background-image', 'none');")


###The width property

This will change the width of the element. You have the potential to mess up big time here, so change one element at a time and see what happens. Elements do impact on each other, so you may not see any difference. When experimenting, change the width of Quest itself to see what effect that has too.

Note that the value must include "px", which says the units are pixels.

   JS.eval("$('#gameBorder').css('width', '950px');")


###The opacity property

The opacity property defines how much this element covers the one below (the reverse of transparency). It can range from 0.0 (this element is not visible) to 1.0 (this element is completely opaque).

   JS.eval("$('#gameBorder').css('opacity', '0.5');")


###The border property

The border property lets you set borders. You can set various aspects in one go, so in this example a dashed line, 5 px wide and blue, will be added.

    JS.eval("$('#gameBorder').css('border', 'dashed 5px blue');")


The status bar at the top has a blue border. If you want to remove it, do this (also set the width to 950px to keep it aligned):

   JS.eval("$('#status').css('border', 'none');")

   
Fonts
-----

There are about a dozen "base fonts" available in Quest. These are fonts that are pretty much guaranteed to be available on any computer (or at least equivalents, so we have Arial on PC, or Helvetica on Mac or failing that sans-serif).

If you want to change the font during a game, use the SetFontName function. This allows you to list the equivalent fonts, so will ensure users on other operating systems see more-or-less the same thing.

    SetFontName("Arial, Heletica, sans-serif")
    msg("This is in Heletica")
    SetFontName("'Courier New', Courier, monospace")
    msg("This is in Courier")
    SetFontName("Impact, Charcoal, sans-serif")
    msg("This is in Charcoal")

The sans-serif and monospace are generic fonts; there are also serif, cursive and fantasy. They will all map to something on every computer, though the cursive and fantasy tend to fall well short of the names.

You also have access to web fonts. These are provided on-line by Google, and by default you can access just one in your game. To use any more, you need to call the SetWebFontName to pull the font off the internet, and then SetFontName as normal to actually use it.

    // Pull the fonts off the internet
    SetWebFontName("Wallpoet")
    SetWebFontName("Admina")

    // Now we can swap between them as much as we like
    SetFontName("Wallpoet")
    msg("This is in Wallpoet")
    SetFontName("Admina")
    msg("This is in Admina")
    SetFontName("Wallpoet")
    msg("This is in Wallpoet again")


Reordering the Panes
--------------------

You can use JQuery to rearrange the panes on the right. Why would you want to do that? If you put the compass rose a the top, then it will stay in the same place. The way it is set out normally, the compass rose jumps around depending on how many items are in the location and inventory and what the player is doing with them.

The JQuery we are going to use looks like this:

    $('#compassLabel').insertBefore('#inventoryLabel')

As before, the $ flags this as JQuery, and then we grab the element with the ID "compassLabel". Then we invoke the "insertBefore" method (function), telling JQuery it want the element to go before the inventoryLabel element.

In Quest, it will look like this (I am moving the status variables up as well, they they do not change their height much either):

    JS.eval ("$('#compassLabel').insertBefore('#inventoryLabel')")
    JS.eval ("$('#compassAccordion').insertBefore('#inventoryLabel')")
    JS.eval ("$('#statusVarsLabel').insertBefore('#inventoryLabel')")
    JS.eval ("$('#statusVarsAccordion').insertBefore('#inventoryLabel')")


Changing the Ending
-------------------

The "finish" script command terminates the game, and replaces the panes on the right with a message. You can change the default font using JQuery again, to make it consistent with your game:

    JS.eval ("$('#gamePanesFinished').css('font-family', 'Berkshire Swash');")

You can also change what gets displayed, using the JQuery html method. In this example, I am modifying the text, and adding an image (and we have to use GetFileURL to do that). I am also building the string first, and then calling JS.eval.

    s = "$('#gamePanesFinished').html('<h2>Game Over</h2>"
    s = s + "<p>This game has finished and you are dead!".</p><img src=\""
    s = s + GetFileURL("gravestone.png")
    s = s + "\" />');"
    JS.eval (s)
    finish


Other Tricks
------------

Some interesting threads from the forum.

["Classic" look](http://textadventures.co.uk/forum/quest/topic/4760/classic-look-text-adventure-and-customisation#31389)

[Make a certain letter disappear and reappear](http://textadventures.co.uk/forum/quest/topic/5121/erasing-a-specific-letter)

[Blurring text (getting drunk)](http://textadventures.co.uk/forum/quest/topic/4947/getting-drunk)

[Injecting stylesheets (CSS in a string attribute on the game object)](http://textadventures.co.uk/forum/samples/topic/4747/injecting-permanent-style-sheets-from-strings)

