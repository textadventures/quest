---
title: JavaScript to Quest Viva with ASLEvent
sidebar:
  order: 9
---

We can think of the game as two distinct parts, the game world, handled by Quest Viva, and the user interface, handled by JavaScript in the browser window (even the desktop version uses a browser). The `JS` object can be uses to pass information and commands from Quest Viva to JavaScript; how do we get information to pass the other way?

## Callback function

The callback function is a function in your game code that will be called from JavaScript. You can call it what you want (and you might have several different ones to handle different events). However, its return type must be "None" and it must take a single parameter, which will be a string.

For example, let us create a function called "InputboxCallback", with a parameter, "s". The code might look like this:

```quest
msg ("You are " + s + " years old.")
```


## ASLEvent function

Quest Viva has a special JavaScript function called `ASLEvent`, which will pass two string values from the browser/JavaScript to the game world. The first parameter has to be the name of a Quest Viva function, the second will be a string parameter to that function.

Here is a very simple example of some JavaScript code. A discussion of the language is way beyond the scope of this tutorial, but the first line says we are defining a function, and the second displays a text box on screen, putting the players response in a new variable called "answer". We then check the user actually types something (i.e., `answer` is not empty), and if so, invoke the `ASLEvent` function, which in turn will call the function we created above.

```js
function askAge() {
  var answer = prompt("How old are you?");
  if (answer != null && answer != "") {
    ASLEvent("InputboxCallback", answer);
  }
}


```
## To test...

If you want to see that in action, wrap the JavaScript in `script` tags, and put it in a string. We can then add that to the HTML document using `addScript`. In the game start script it would lok like this:

```quest
s = "<script>"
s = s + "function askAge() {"
s = s + "var answer = prompt('How old are you?');"
s = s + "if (answer != null && answer != '') {"
s = s + "ASLEvent('InputboxCallback', answer);"
s = s + "}}</script>"
JS.addScript(s)
JS.askAge()
```



## Custom status pane

Using this technique, you could change the [custom status pane](/howto/ux/custom_panes) into a control panel. Go to the game object, and turn on the custom status pane on the _Interface_ tab, then add this to the start script:

```xml
html = "<p><a onclick=\"ASLEvent('HandleClick', 'HERE')\">HERE</a><p>"
JS.setCustomStatus (html)
```

Create a new function, HandleClick, that will print its single parameter. When you go in game, you can click "HERE" and Quest Viva will respond. Obviously this does nothing more than the custom command pane, but potentially you could set up a sophisticated control panel with switches and flashing lights and sliders.


## Handling multiple parameters

If you have a lot of bits of data to pass from JavaScript to Quest Viva (say the results from a character creation dialogue), you will have to collect them altogether into one long string in JavaScript before calling ASLEvent, and then in the Quest Viva function, you will need to split them apart again. Each bit of data should be separated with a specific character, say the vertical bar, |.

The JavaScript might look like this:

```js
var s = name;
s += "|" + age;
s += "|" + eyeColour;
ASLEvent("CreatorCallback", s);
```

In Quest Viva, you can use Split to break the string up, and then handle each section. Remember to convert to integers where necessary:

```quest
l = Split(s, "|")
player.name = StringListItem(l, 0)
player.age = ToInt(StringListItem(l, 1))
player.eyecolour = StringListItem(l, 2)
```


## Timers

If you want split second timing, then `ASLEvent` is the way to go. Quest Viva's built-in timers only work in whole seconds, but a JavaScript timer can run as fast as you like, and call back into the game when something actually needs to happen.

The thing to get right is which half does the work. Every `ASLEvent` call runs a script in the game world, so firing one on every tick of a ten-per-second timer means a hundred script runs in ten seconds - and each one also marks the game as having unsaved progress. Keep the fast, cosmetic part - counting, animating, redrawing - in JavaScript, and call back only at the moments the game itself cares about.

This example counts down in tenths of a second in the [custom status pane](/howto/ux/custom_panes), and calls the game just once, when it reaches zero. Turn on the custom status pane on the game object's _Interface_ tab, then put this in your start script:

```quest
s = "<script>"
s = s + "var countdownTimer;"
s = s + "function startCountdown(seconds) {"
s = s + "  var deadline = Date.now() + parseFloat(seconds) * 1000;"
s = s + "  countdownTimer = setInterval(function () {"
s = s + "    var left = deadline - Date.now();"
s = s + "    if (left > 0) {"
s = s + "      setCustomStatus('Time remaining: ' + (left / 1000).toFixed(1));"
s = s + "    }"
s = s + "    else {"
s = s + "      clearInterval(countdownTimer);"
s = s + "      setCustomStatus('Time remaining: 0.0');"
s = s + "      ASLEvent('CountdownFinished', 'timeout');"
s = s + "    }"
s = s + "  }, 100);"
s = s + "}"
s = s + "</script>"
JS.addScript (s)
```

Start it whenever you want the clock to begin - `JS.startCountdown ("120")` for two minutes - and create a `CountdownFinished` function taking a single string parameter, to handle the end:

```quest
msg ("Out of time!")
finish
```

The same shape works for anything you want timed finely: run the timer in JavaScript, and use `ASLEvent` to tell the game about the handful of moments that matter.

There are further examples on the archived forum - a [countdown clock](https://archive.textadventures.co.uk/forum/samples/topic/gz1msne3k0_mjvoj8vpubw) that fires several events at different points along the way, and a [screen flash effect](https://archive.textadventures.co.uk/forum/samples/topic/4rajpgh0ikicac9we2rsiq) which stays entirely in JavaScript, since the game world has nothing to do while it runs.
