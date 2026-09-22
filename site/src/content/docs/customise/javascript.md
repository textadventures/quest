---
title: Calling the game from JavaScript
description: Add your own JavaScript to a game, call it from your scripts, and send results back to the game with ASLEvent
---

Quest Viva games run in a web page, so you can add your own JavaScript to build things the standard interface doesn't have - a form, a control panel, a timer that counts in tenths of a second. This page shows how to add JavaScript to a game, how to call it from your scripts, and how your JavaScript can call back into the game.

| You want to | Use |
|---|---|
| Add your own JavaScript functions | A Javascript element, with the code in a `.js` file |
| Run JavaScript from a script | `JS.functionName (...)` - "Run Javascript" in the Output category |
| Tell the game something from JavaScript | `ASLEvent ("FunctionName", value)` |
| Ask the player a question and wait for the answer | `GetInput()`, `Ask()` or `ShowMenu()` - see [Asking the player](/howto/scripting/asking-the-player) |

Before writing any JavaScript, check whether the last row covers what you need. A question asked with `GetInput()` or `ShowMenu()` needs no JavaScript at all, works the same everywhere, and is easier to read: the script simply carries on with the answer. JavaScript is for things those can't do.

## Adding your own JavaScript

Keep your JavaScript in its own file, and add it to the game with a Javascript element:

1. In the tree, select **Advanced**, then click **Add JavaScript**.
2. Type a filename, such as `game.js`, to create a new file - or pick or upload an existing `.js` file - and click **Add JavaScript**.
3. The new element has a **Javascript** tab with a text editor for the file. Write your functions there.

The file is loaded into the page before the game starts, so its functions are ready to call from the game's start script. In code view, the element is a single line:

```xml
<javascript src="game.js"/>
```

You may see older games and guides build JavaScript up as a string, wrapped in `<script>` tags, and add it with `JS.addScript`. That still works, but a `.js` file is far easier to read and edit, as you don't need to escape every quote.

## Calling JavaScript from the game

To call a JavaScript function from a script, put `JS.` in front of its name:

```quest
JS.startCountdown (120)
```

In the editor, add "Run Javascript" from the Output category and type the call after `JS.` - here, `startCountdown (120)`.

This works for your own functions and for the ones built into the player - see [JS functions](/reference/js/) for the full list. Strings, numbers and booleans arrive in JavaScript as the same types, and a string list arrives as an array. Any line breaks in a string are removed on the way.

The call only goes one way: whatever the JavaScript function returns is ignored, so it can't hand a value back to your script. To get information back, use `ASLEvent`, below.

For a single line of JavaScript that isn't worth a function, `JS.eval` runs a string as code:

```quest
JS.eval ("document.title = 'The Cave'")
```

## Calling the game from JavaScript

`ASLEvent` is a JavaScript function the player provides. It calls a function in your game, passing it one string:

```js
ASLEvent("RingBell", "loudly");
```

The game function it calls needs exactly one parameter. Add a function (select **Advanced** in the tree, then **Add Function**), leave **Return type** as "None", and add one parameter under **Parameters** - here, `how`:

```quest
msg ("You ring the bell, " + how + ".")
```

The function name must match exactly, including capitals. If there's no function with that name, the game prints "Error - no handler for event" instead. A function with no parameters doesn't run at all.

`ASLEvent` doesn't need a `.js` file. Any HTML you print can call it - this prints a button that rings the bell:

```quest
msg ("<button type=\"button\" onclick=\"ASLEvent('RingBell', 'loudly')\">Ring the bell</button>")
```

### What happens when ASLEvent runs

An `ASLEvent` call runs your function and nothing else. It isn't a turn:

- Turn scripts don't run.
- It doesn't get its own undo step. Its changes are added to the player's previous command, so typing UNDO takes back that command and the event together. Changes made by an event before the player's first command can't be undone.
- The changes are part of the game state like any other, so they're saved when the player saves. The player counts as having unsaved progress afterwards.
- If the game is waiting for the player - for example at `GetInput()` or a menu - the function still runs straight away, and the waiting script stays paused until the player answers.

If you need turn scripts to run, call `RunTurnScripts` at the end of your function.

### Sending several values

`ASLEvent` only passes one string, so to send several values, join them together in JavaScript and split them apart in the game. For example, JavaScript could send `"Strength=4;Agility=5;Wits=1"`, and the game can split that into pairs:

```quest
foreach (pair, Split(data, ";")) {
  parts = Split(pair, "=")
  set (game.pov, LCase(StringListItem(parts, 0)), ToInt(StringListItem(parts, 1)))
}
```

Everything arrives as a string - even if the JavaScript passes a number - so use `ToInt` for numbers, as here.

## Example: a points form for character creation

This form lets the player share 10 points between three stats with - and + buttons, then sends the result to the game when they click Done. It's plain HTML, fits a phone screen, and works with the keyboard.

Put this in your `.js` file:

```js
var statsStyle = document.createElement("style");
statsStyle.textContent =
  ".stats-form { display: grid; gap: 0.5em; max-width: 20em; margin: 1em 0; }" +
  ".stats-row { display: grid; grid-template-columns: 1fr 2.75em 2.5em 2.75em; align-items: center; }" +
  ".stats-value { text-align: center; }" +
  ".stats-form button { min-height: 2.75em; font: inherit; }";
document.head.appendChild(statsStyle);

function showStatsForm(points, names) {
  var html = '<div class="stats-form">' +
    '<p>Points left: <strong class="stats-left" aria-live="polite">' + points + '</strong></p>';
  names.forEach(function (name) {
    html += '<div class="stats-row" data-stat="' + name + '">' +
      '<span>' + name + '</span>' +
      '<button type="button" aria-label="Lower ' + name + '" onclick="statsAdjust(this, -1)">-</button>' +
      '<span class="stats-value">0</span>' +
      '<button type="button" aria-label="Raise ' + name + '" onclick="statsAdjust(this, 1)">+</button>' +
      '</div>';
  });
  html += '<button type="button" onclick="statsDone(this)">Done</button></div>';
  addText(html);
}

function statsAdjust(button, change) {
  var form = button.closest(".stats-form");
  var left = form.querySelector(".stats-left");
  var value = button.parentNode.querySelector(".stats-value");
  var newValue = parseInt(value.textContent) + change;
  var newLeft = parseInt(left.textContent) - change;
  if (newValue < 0 || newLeft < 0) {
    return;
  }
  value.textContent = newValue;
  left.textContent = newLeft;
}

function statsDone(button) {
  var form = button.closest(".stats-form");
  var parts = [];
  form.querySelectorAll(".stats-row").forEach(function (row) {
    parts.push(row.dataset.stat + "=" + row.querySelector(".stats-value").textContent);
  });
  form.remove();
  ASLEvent("StatsChosen", parts.join(";"));
}
```

`showStatsForm` adds the form to the game text with the player's `addText` function. The buttons keep their values in the page itself, and call their functions with `onclick` attributes, so a form that's still on screen when the player saves carries on working after they load that save. When the player clicks Done, `statsDone` removes the form and sends something like `Strength=4;Agility=5;Wits=1` to the game.

Show the form from the game's start script (on the game's **Scripts** tab):

```quest
stats = Split("Strength;Agility;Wits", ";")
JS.showStatsForm (10, stats)
```

Then add a function called `StatsChosen`, with one parameter, `data`, to receive the result:

```quest
foreach (pair, Split(data, ";")) {
  parts = Split(pair, "=")
  set (game.pov, LCase(StringListItem(parts, 0)), ToInt(StringListItem(parts, 1)))
}
msg ("Strength " + game.pov.strength + ", agility " + game.pov.agility + ", wits " + game.pov.wits + ".")
```

The stats are now attributes on the player - `strength`, `agility` and `wits` - ready for the rest of the game to use.

The player can still type commands while the form is on screen. If the game shouldn't go any further until they've chosen, asking with `ShowMenu()` or `GetInput()` may suit you better, as the script waits for the answer - [Character creation](/howto/player/character-creation) shows that approach.

## Timers

Quest Viva's own [timers](/tutorial/using-timers-and-turn-scripts) work in whole seconds. For anything finer, run the timer in JavaScript, and use `ASLEvent` to tell the game only about the moments it cares about.

Every `ASLEvent` call runs a script in the game, so calling it on every tick of a fast timer is wasteful - and each call marks the game as having unsaved progress. Keep the fast, cosmetic part - counting, animating, redrawing - in JavaScript.

This countdown shows the time remaining in tenths of a second, and calls the game once, when it reaches zero. Add it to your `.js` file:

```js
var countdownTimer;

function startCountdown(seconds) {
  var deadline = Date.now() + seconds * 1000;
  addText('<p class="countdown"></p>');
  var displays = document.querySelectorAll(".countdown");
  var display = displays[displays.length - 1];
  clearInterval(countdownTimer);
  countdownTimer = setInterval(function () {
    var left = Math.max(0, deadline - Date.now());
    display.textContent = "Time remaining: " + (left / 1000).toFixed(1);
    if (left == 0) {
      clearInterval(countdownTimer);
      ASLEvent("CountdownFinished", "timeout");
    }
  }, 100);
}
```

Start it with `JS.startCountdown (120)` for two minutes, and add a `CountdownFinished` function with one parameter to handle the end:

```quest
msg ("Out of time!")
finish
```

## See also

- [JS functions](/reference/js/) - the JavaScript functions built into the player
- [Asking the player](/howto/scripting/asking-the-player) - questions and menus that need no JavaScript
- [Panes](/customise/panes#custom-status-pane) - showing your own HTML beside the game text
