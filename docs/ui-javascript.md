---
layout: index
title: Customising the UI - Part 1
---


The Quest user interface is a web page. Even the desktop version has a built-in browser (Chrome as it happens), and what the player sees is just the same as any other web page on the internet.

Potentially this means you can set up your game to look like _anything_. In practice, this is not trivial and at least some knowledge of HTML, CSS, JavaScript and JQuery are vital for the more advanced features. That said, we can make a lot of changes without knowing too much about any of them.


The JS Object
-------------

The `JS` object is a quick way to use JavaSript in your game. You can use it to access the built-in JavaScript functions (and your own too), and there are several that can be used to change the UI. 

```
  // Use these two to turn features on and off during play
  // Valid element names include "#txtCommandDiv", "#location"
  JS.uiShow(element)
  JS.uiHide(element)
  // For the panes on the right, use this:
  JS.panesVisible(Boolean)

  // Use these to modify the texts on the UI
  JS.setInterfaceString ("InventoryLabel", "You are holding")
  JS.setInterfaceString ("StatusLabel", "How you are doing")
  JS.setInterfaceString ("PlacesObjectsLabel", "Things you can see")
  JS.setInterfaceString ("CompassLabel", "Directions you can go")
  JS.setInterfaceString ("InButtonLabel", "In")
  JS.setInterfaceString ("OutButtonLabel", "out")
  JS.setInterfaceString ("EmptyListLabel", "Stuff all")
  JS.setInterfaceString ("NothingSelectedLabel", "-")
  JS.setInterfaceString ("TypeHereLabel", "Now what?")
  JS.setInterfaceString ("ContinueLabel", "Just press a button to get on with it")

  // Others that may or may not be useful
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
  JS.showStatusVisible(Boolean)
  JS.setBackground(colour)
```

If you want to do more than that, you need to learn a little about HTML and CSS...

HTML
----

HyperText Markup Language (HTML) is the way information is structured on a web page. If you are on the desktop version, you can click "HTML Tools" whilst playing to see the HTML behind the current view. On the web, right click, and select "View source" (or similar).

Here is an example:
```
<div id="divOutputAlign3" style="text-align: center" class="section1 title">
  <span style="font-family:'Lucida Console', Monaco, monospace;color:Black;font-size:12pt;">
    <span style="font-size:260%">
      Cool Game
    </span>
  </span>
  <br>
</div>
```

HTML code is made up of _elements_, and each element has a start tag (eg `<div>`) and an end tag (eg `</div>`). The bit between them is the content, and the content can be other elements or text or a mixture. The start tag can often have attributes, and these are important because they can give us a way to access the element. In the snippet above, the `<div>` has an attribute, id, which has the value "divOutputAlign3". The id attribute will be important later.


CSS
---

Cascading style sheets (CSS) is the primary way for web pages to define the style, as opposed to the content; that is, what font to use, colours, etc. CSS is probably the technology you need to know properly, as really there is no short-cut here. The objective here is to set styles as you want them, so knowing the underlying style system is going to serve you well, and we can only do a brief overview here.

An example of CSS for a web page might looks like this:

```
#gameBorder {
  background-color: #800080;
  color: pink;
}
```

The first line determines what is controlled - in this case an element with the ID gameBorder (the # indicates ID rather than a class or element type; the # on the next line means something quite different, it indicates a colour). The second line defines the settings. There can be several lines, before we get to the close brace (this is the conventional way to layout CSS). For the second line, there are two parts, the name, in this case "background-color", and the value, "#800080" (which is a dark magenta).

In summary, then, this CSS code will set the background colour of something with the ID "gameBorder" to be dark magenta, and the text to be pink.

CSS can also be set in HTML attributes. If you look back at the HTML example above, the `<div>` has an attribute, style, and the value is in CSS. The two `<span>` elements likewise have style attributes. The text, "Cool Game", will be in the combined style of all those elements.

If you are using the desktop version, click on "HTML Tools" whilst playing again. On the web version, for Chrome go to Option - More Tools - Developer Tools, for Firefox go to Options - Developer - Inspector, for Internet Explorer go to Options - F12 Developer Tools. These all vary a bit, but you should find you can highlight elements of HTML, and you will see what CSS applies, and where it comes from.


### A note about colours

In CSS you can use two formats for colours. The name or the hex value. About a hundred colours are named (note they have no spaces, but are case insensitive). The hex value must start with a #, followed by three pairs of characters, one pair each for red, green and blue. Each pair is the hexadecimal value, from 00 to FF. If the hex value makes no sense, stick to the names!

[https://en.wikipedia.org/wiki/Web_colors](https://en.wikipedia.org/wiki/Web_colors)


JavaScript
----------

Static web pages use CSS like that, but if you want things to change, you need JavaScript. JavaScript is a fully-fledged programming language (and is _not_ the same as Java), and has become the standard for web browsers. We will try to avoid writing JavaScript code as far as possible!


JQuery
------

JQuery is a library for JavaScript that is built in to Quest. Among other things, it offers relatively easy ways to access parts of the HTML page.

This is how JQuery/JavaScript could be used to set the styles in the CSS example.

```
  $('#gameBorder').css('background-color', '#800080');
  $('#gameBorder').css('color', 'pink');
```

Notice that all the same information is there, just arranged differently, according to the syntax of JavaScript/JQuery. The $ at the start signifies this is JQuery (it is a shorthand for a function called `JQuery`), and $('#gameBorder') will grab the thing with the ID "gameBorder" (again, the # indicates this is an ID). Once we have that we call a method (or function) called "css", and send it two parameters, the thing we want to change and the new value.



Quest
-----

Quest sets up the User Interface in the `InitInterface` function, which is defined in Core.aslx. Almost the last thing it does is call a script, "inituserinterface", on the game object (if it exists), after which game.start will run (unless the player is resuming with a saved game). The best way to modify the user interface, then, is using the "inituserinterface" script.

The big advantage of doing it this way is that this will be called whenever Quest thinks the interface needs updating, which is not just at the start of the game (for example, when the screen is cleared). You also get the bonus of having all your interface stuff in the same place, which keeps it neat.

Note, however, that you should not print anything from the "inituserinterface" script (you might feel tempted to output CSS or some JavaScripts using msg or OutputTextRawNoBr). If you do, when a saved game is reloaded, all the new text will get inserted into the top of the existing text.

To edit the script, go to the _Features_ tab of the game object, and check that "Show advanced scripts for the game object" is ticked. Then go to the _Advanced Scripts" tab. The "inituserinterface" script is at the top.

Because it is easier to show on a forum post, all the tricks here will be in code. In the desktop version, click on the seventh icon, "Code view", and a text box will appear. For the web version, the button is labelled "Code view". Just copy-and-paste code into here. You can paste in as many code blocks as you like, and it should work fine (note that that is not necessarily true of all code).


Using All That In Quest
-----------------------

So now we know where to put the code in Quest, and we know the JavaScript to do it. We just need a way to pass the JavaScript from the game to the interface. This is done using the `JS` object, for example using the `eval` function:

```
JS.eval("$('#gameBorder').css('background-color', '#800080');")
JS.eval("$('#gameBorder').css('color', 'pink');")
```

The JS object is a way to access any JavaScript function, even those you add yourself. The `eval` function is useful because it will run any JavaScript code. So the first line above is say, "JavaScript, please run this string as though it is JavaScript code", and the string to run is `$('#gameBorder').css('background-color', '#800080');`, i.e., the code we had before.

Note that this is not a way to get information from the interface; this is a one-way street. Data is going from Quest to JavaScript only (there is a way to go the other way; that is how the player's inputs get to Quest, but that is beyond the scope of this article).


Shortcuts
---------

Fortunately, as of Quest 5.7, you can use the `setCss` function to do this sort of thing. Like `eval`, this belongs to the JS object. It takes two parameters, the element and the style. The style should be in the standard CSS format, with a colon between the name and the value, and a semi-colon between each setting. The example above would therefore look like this:

```
JS.setCss("#gameBorder", "background-color;#800080;color:pink;")
```

Using this function, you can now change any element in the game (well nearly any, a few are a bit odd). You just need to know the id of the element and the right CSS to use. 


To quickly format the game panes you can use `setPanes`. This takes two, four or five parameters, all of which are colours.

```
JS.setPanes ("black", "white")
JS.setPanes ("orange", "black", "black", "orange")
JS.setPanes ("midnightblue", "skyblue", "white", "midnightblue", "blue")
```

In [part 2](ui-javascript2.html) we will look in more depth at using HTML elements and CSS.
