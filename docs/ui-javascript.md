---
layout: index
title: The UI and JavaScript
---

Quest sets up the User Interface in the `InitInterface` function, which is defined in Core.aslx. The last thing it does is call another function, `InitUserInterface`, which is empty (after which game.start will run, unless the player is resuming with a saved game). The best way to modify the user interface, then, is to define your own `InitUserInterface` function.

The big advantage of doing it this way is that this will be called whenever Quest thinks the interface needs updating, which is not just at the start of the game (for example, when the screen is cleared). You also get the bonus of having all your interface stuff in the same place, which keeps it neat.

Note, however, that you should not print anything from the InitUserInterface function (you might feel tempted to output CSS or some JavaScripts using msg or OutputTextRawNoBr). If you do, when a saved game is reloaded, all the new text will get inserted into the top of the existing text.

Unfortunately, this will not work if you are editing on-line, as the web editor will not let you create a function with the same name as one already in the game, so this discussion is really restricted to the desktop version.

See the section on [Overriding functions](overriding.html) for how to override `InitInterface`.

Because it is easier to show on a forum post, all the tricks here will be in code. In the script section at the bottom, click on the seventh icon, "Code view", and a text box will appear. Just copy-and-paste code into here. You can paste in as many code blocks as you like, and it should work fine (note that that is not necessarily true of all code).


Using the JS object
-------------------

The JS object (JavaScript) has a bunch of functions (or methods, they should probably be called), which it uses to set the initial look, depending on setting on the game object. We are going to use a special function called "eval", which runs any JavaScript code.
```
  JS.eval("someJavaScriptCode();"
```
We also need to use JQuery, which is an extension to JavaScript, HTML and CSS. To understand how that fits together, we will look at each technology in turn.


### Using CSS

Cascading style sheets (CSS) is the primary way for web pages to define the style, as opposed to the content; what font to use, colours, etc. An example might looks like this:
```
#gameBorder {
  background-color: #800080;
  color: pink;
}
```

The first line determines what is controlled - in this case an element with the ID gameBorder (the # indicates ID rather than a class or element type; the # on the next line means something quite different, it indicates a colour). The second line defines the settings. There can be several lines, before we get to the close brace (this is the conventional way to layout CSS). For the second line, there are two parts, the name, in this case "background-color", and the value, "#800080" (which is a dark magenta).

In summary, then, this CSS code will set the background colour of something with the ID "gameBorder" to be dark magenta, and the text to be pink.


### Using JQuery

Static web pages use CSS like that, but it you want things to change, you need JavaScript, and JQuery is a quick way to access an element in JavaScript. To do the above in JavaScript/JQuery, you would do this:
```
  $('#gameBorder').css('background-color', '#800080');
  $('#gameBorder').css('color', 'pink');
```
Notice that all the same information is there, just arranged differently, according to the syntax of JavaScript/JQuery. The $ at the start signifies this is JQuery (it is a shorthand for a function called `JQuery`), and $('#gameBorder') will grab the thing with the ID "gameBorder" (again, the # indicates this is an ID). Once we have that we call a method (function) called "css", and send it two parameters, the thing we want to change and the new value.


### Using JQuery in Quest

To do it is Quest, you have to send that as a string to the JS.eval function:
```
  JS.eval("$('#gameBorder').css('background-color', '#800080');")
  JS.eval("$('#gameBorder').css('color', 'pink');")
```
Once you have that template, you can change a shed load of setting, you just need to know what each bit (like "gameBorder") is called, what the CSS property (like "background-color") is called and what value (like "#800080") is allowed. 

### What are the bits called?

To find out what a specific component is called, start your game, and click the "HTML Tools" button. This will show the HTML on your page, and you can click on an element to see its ID and current style. You will probably need to expand a few sections to find what you want.