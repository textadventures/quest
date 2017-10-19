---
layout: index
title: JavaScript to Quest with ASLEvent
---

We can think of the game as two distinct parts, the game world, handled by Quest, and the user interface, handled by JavaScript in the browser window (even the desktop version uses a browser). The `JS` object can be uses to pass information and commands from Quest to JavaScript; how do we get information to pass the other way?

Callback function
-----------------

The callback function is a function in your game code that will be called from JavaScript. You can call it what you want (and you might have several different ones to handle different events). However, its return type must be "None" and it must take a single parameter, which will be a string.

For example, let us create a function called "InputboxCallback", with a parameter, "s". The code might look like this:

      msg ("You are " + s + " years old.")


ASLEvent function
-----------------

Quest has a special JavaScript function called `ASLEvent`, which will pass two string values from the browser/JavaScript to the game world. The first parameter has to be the name of a Quest function, the second will be a string parameter to that function.

Here is a very simple example of some JavaScript code. A discussion of the language is way beyond the scope of this tutorial, but the first line says we are defining a function, and the second displays a text box on screen, putting the players response in a new variable called "answer". We then check the user actually types something (i.e., `answer` is not empty), and if so, invoke the `ASLEvent` function, which in turn will call the function we created above.

      function askAge() {
        var answer = prompt("How old are you?");
        if (answer != null && answer != "") {
          ASLEvent("InputboxCallback", answer);
        }
      }

      
To test...
----------

If you want to see that in action, wrap the JavaScript in `script` tags, and put it in a file called "js.html". It should look like this:

      <script>
        function askAge() {
          var answer = prompt("How old are you?");
          if (answer != null && answer != "") {
            ASLEvent("InputboxCallback", answer);
          }
        }
      </script>

In the game start script add these lines:

      JS.addText (GetFileData("js.html"))
      JS.askAge()

The first line will output the content of the file, adding the JavaScript function to the game. The second line then calls your function. Start the game, you wioll be asked your age, and when you click okay, Quest will print it out.


Custom status pane
------------------

Using this technique, you could change the [custom status pane](custom_panes.html) into a control panel. Go to the game object, and turn on the custom status pane on the _Interface_ tab, then add this to the start script:

      html = "<p><a onclick=\"ASLEvent('HandleClick', 'HERE')\">HERE</a><p>"
      JS.setCustomStatus (html)

Create a new function, HandleClick, that will print its single parameter. When you go in game, you can click "HERE" and Quest will respond. Obviously this does nothing more than the custom command pane, but potentially you could set up a sophisticated control panel with switches and flashing lights and sliders.


Handling multiple parameters
--------------------

If you have a lot of bits of data to pass from JavaScript to Quest (say the results from a character creation dialogue), you will have to collect them altogether into one long string in JavaScript before calling ASLEvent, and then in the Quest function, you will need to split them apart again. Each bit of data should be separated with a specific character, say the vertical bar, |.

The JavaScript might look like this:

      var s = name;
      s += "|" + age;
      s += "|" + eyeColour;
      ASLEvent("CreatorCallback", s);

In Quest, you can use Split to break the string up, and then handle each section. Remember to convert to integers where necessary:

      l = Split(s, "|")
      player.name = StringListItem(l, 0)
      player.age = ToInt(StringListItem(l, 1))
      player.eyecolour = StringListItem(l, 2)

_NOTE:_ There is an issue to be aware of that appears when games are played on line. It seems you can only have one callback function in operation at a time. Offline this is no problem, as they happen so fast, but online, you may find that only the last one called actually completes. This means that calling ASLEvent for each bit of data is not a reliable option.


Timers
------

If you want to use split second timing, then `ASLEvent` is the way to go. The Quest timers work in whole second only, and when you play on-line, the exact timing can go awry. However, you can use a JavaScript timer, which will run on the players PC, rather than the Quest server, and have that fire events in Quest using ASLEvent.

The details are beyond the scope of this article, but you can see examples [here](http://textadventures.co.uk/forum/samples/topic/gz1msne3k0_mjvoj8vpubw/countdown) and [here](http://textadventures.co.uk/forum/samples/topic/4rajpgh0ikicac9we2rsiq/thunder-and-lightning-effect).
