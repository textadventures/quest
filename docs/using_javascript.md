---
layout: index
title: Using Javascript
---

<div class="alert alert-info">
Note: As the `InitUserInterface` can only be overriden in the desktop version, using JavaScript in the web version will be problematic..

</div>

Introduction
------------

Quest's interface is customisable with the power of HTML/CSS and Javascript. If you don't like Quest's standard layout, you can modify it how you like with HTML and CSS, and use Javascript as the bridge between your interface and your game.

Quest has a special object called "JS" (for JavaScript), and you can use this to send JavaScript code to the interface. There are several built-in methods, such as:
```
  JS.hideBorder()
  JS.showBorder()
  JS.addText("Here is some text")
```

### Using JavaScript, CSS and JQuery

When you have exhausted those possibilities you are delving into the murky would of CSS and JQuery (which is an extension of JavaScript, and is already in your game), via the eval method of JS. To understand how that fits together, we will look at each technology in turn.


#### Using CSS

Cascading style sheets (CSS) is the primary way for web pages to define the style, as opposed to the content; what font to use, colours, etc. An example might looks like this:
```
#gameBorder {
  background-color: #800080;
}
```

The first line determines what is controlled - in this case an element with the ID gameBorder (the # indicates ID rather than a class or element type). The second line defines the settings. There can be several lines, before we get to the close brace (this is the conventional way to layout CSS). For the second line, there are two parts, the name, in this case "background-color", and the value, "#800080" (which is a dark magenta).

In summary, then, this CSS code will set the background colour of something with the ID "gameBorder" to be dark magenta.


#### Using JQuery

Static web pages use CSS like that, but if you want things to change, you need JavaScript, and JQuery is a quick way to access an element in JavaScript. To do the above in JavaScript/JQuery, you would do this:
```
  $('#gameBorder').css('background-color', '#800080');
```
Notice that all the same information is there, just arranged differently, according to the syntax of JavaScript/JQuery. The `$` at the start is a shortcut to a special JQuery function that will get our HTML, and so `$('#gameBorder')` will grab the thing with the ID "gameBorder" (again, the # indicates this is an ID). Once we have that we call a method (function) called "css", and send it two parameters, the thing we want to change and the new value.

#### Using JQuery in Quest

To do it is Quest, you have to send that as a string to the JS.eval function:
```
  JS.eval("$('#gameBorder').css('background-color', '#800080');")
```
Once you have that template, you can change a shed load of setting, you just need to know what each bit (like "gameBorder") is called, what the CSS property (like "background-color") is called and what value (like "#800080") is allowed. 


Sending messages from Javascript to the game
--------------------------------------------

What about in the other direction? How do we get something from the interface, to your game - for example, when the player clicks on a special button.

To do this, the button must have an attribute, "onclick", with some Javascript code, using the special function `ASLEvent`:

     ASLEvent("ProcessButtonClick", id);

This is Javascript calling a `ProcessButtonClick` function which should be defined in the Quest game. It passes a parameter - the id of the button clicked - to indicate which button the player clicked.

For more on how to use JavaScript oin your game, yoyu might like to look [here](https://github.com/ThePix/quest/wiki#ui).
