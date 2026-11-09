---
title: QuestUI
description: The player's JavaScript interface - every member of the QuestUI object, its events, and the global ASLEvent and sendCommand functions
sidebar:
  order: 27
---

`QuestUI` is the player's JavaScript interface, the same in the new interface and the classic one. Your game's scripts call it with `JS.QuestUI.`, and your own JavaScript calls it as `QuestUI.`:

```quest
JS.QuestUI.setInterfaceString ("InventoryLabel", "You are holding")
```

```js
QuestUI.sendCommand("look");
```

See [Calling the game from JavaScript](/customise/javascript) for how to add JavaScript to a game and call it.

`QuestUI` only ever gains members: none is removed or changes what it does, so a game that uses it keeps working in future versions. A name it doesn't know, such as a part or event that doesn't exist, is ignored rather than an error.

Most of these members are what Quest Viva's own libraries use to drive the interface, and a script function does the same job more simply. Where there's one, it's linked. Use it instead of `QuestUI` from your scripts: what the function changes is kept when the player loads a saved game, and a change made through `QuestUI` isn't.

For the classic interface's own functions, such as `JS.setCss` and `JS.addText`, see [JS functions (classic interface)](/reference/js-classic/). They don't exist in the new interface.

## Global functions

Besides `QuestUI`, the new interface has two functions of its own that a game can call. The classic interface has them too.

| Function | What it does |
|---|---|
| `ASLEvent(name, value)` | Calls the game function `name`, which must have one parameter, with `value` as a string. The same as `QuestUI.sendEvent`. See [Calling the game from JavaScript](/customise/javascript#calling-the-game-from-javascript) |
| `sendCommand(text)` | Runs `text` as if the player had typed it. The same as `QuestUI.sendCommand` |

## Engine

| Member | What it does |
|---|---|
| `sendCommand(text)` | Runs `text` as if the player had typed it |
| `sendEvent(name, value)` | Calls the game function `name` with `value`, like `ASLEvent` |
| `restart()` | Starts the game again |

## Output

| Member | What it does | Instead, use |
|---|---|---|
| `addText(html)` | Prints HTML at the end of the game text | `msg` |
| `clearScreen()` | Clears the game text | [ClearScreen](/reference/functions/user-interface#clearscreen) |
| `setAlignment(align)` | Aligns the text printed after it: `left`, `center` or `right` | [SetAlignment](/reference/functions/user-interface#setalignment) |
| `startSection(name)`, `endSection(name)`, `hideSection(name)` | Marks out a section of the text, and hides it | |
| `disableSection(name)` | Turns off the links in a section | |
| `disableCommandLinks()` | Turns off every `{command:...}` link in the text | |
| `updateLinks(kind, data)` | Updates the verbs of object links (`object`), or turns exit (`exit`) or command (`command`) links on and off | |
| `scrollToEnd()` | Scrolls to the latest text | |
| `addYouTube(id)`, `addVimeo(id)` | Embeds a video | [ShowYouTube](/reference/functions/user-interface#showyoutube) |
| `textEffect(kind, ...)` | Prints text with a `typewriter` or `unscramble` effect | [TextFX_Typewriter](/reference/functions/user-interface#textfx_typewriter), [TextFX_Unscramble](/reference/functions/user-interface#textfx_unscramble) |

A `<script>` element in the HTML given to `addText`, or printed with `msg`, doesn't run in the new interface.

## Layout and panes

| Member | What it does | Instead, use |
|---|---|---|
| `setVisible(part, visible)` | Shows or hides a part of the interface: see [Changing the interface during play](/customise/interface-scripts#showing-and-hiding-parts) for the names | [SetPaneVisible](/reference/functions/user-interface#setpanevisible) |
| `setPaneOrder(parts)` | Moves the named panes to the top of the panes, in that order | |
| `setLayoutPreset(name, narrow)` | Where the panes go: `sidepanes`, `bottombar` or `nopanes`, with `narrow` for a phone if different | The Interface tab's **Where the panes go** and **On a phone** |
| `setMapPosition(position)` | Where the map goes: `header` or `panes` | The Interface tab's **Map position** |
| `setPictureFrame(html)` | Sets what's in the picture frame | [SetFramePicture](/reference/functions/user-interface#setframepicture) |
| `setInterfaceString(name, text)` | Changes a piece of the interface's own text (see below) | |
| `setCompassDirections(list)` | Names the compass directions | |
| `setLocation(text)` | Sets the location bar's text | |
| `setStatusAttributes(items)` | Fills the Status pane | [Status attributes](/reference/attributes/status) |

The pieces of text `setInterfaceString` can change are `InventoryLabel`, `StatusLabel`, `PlacesObjectsLabel` and `CompassLabel` (the pane headings), `InButtonLabel` and `OutButtonLabel` (the compass's in and out buttons), `EmptyListLabel` (an empty Inventory or Places and Objects list, or `InventoryEmptyLabel` and `PlacesObjectsEmptyLabel` for just one), `NothingSelectedLabel`, `TypeHereLabel` (the command bar's hint) and `ContinueLabel` (the Continue button). Call it from the user interface initialisation script, so it's set again when a saved game is loaded.

## Panes and other author hooks

| Member | What it does | Instead, use |
|---|---|---|
| `addPane(id, title, html)` | Adds a pane of your own | [AddPane](/reference/functions/user-interface#addpane) |
| `setPaneContent(id, html)` | Fills a pane of your own, or the custom status pane (`customstatus`) | [SetPaneContent](/reference/functions/user-interface#setpanecontent) |
| `setCommands(list)` | Sets the command pane's commands | [SetCommandPane](/reference/functions/user-interface#setcommandpane) |
| `setAutoScroll(on)` | Turns scrolling to new text on or off | [SetAutoScroll](/reference/functions/user-interface#setautoscroll) |
| `setContinueOnClick(on)` | Lets a click anywhere continue after a wait | [SetContinueOnClick](/reference/functions/user-interface#setcontinueonclick) |
| `fadePreviousOutput(on)` | Fades the text from earlier turns | [SetFadePreviousOutput](/reference/functions/user-interface#setfadepreviousoutput) |
| `setCommandPlaceholder(text)` | Sets the hint in the empty command bar | [SetCommandPlaceholder](/reference/functions/user-interface#setcommandplaceholder) |
| `setMapInteractive(on)` | Lets the player drag and zoom the map | [SetMapInteractive](/reference/functions/user-interface#setmapinteractive) |
| `on(event, fn)`, `off(event, fn)` | Starts or stops calling `fn` when `event` happens (see below) | |

## Events

`QuestUI.on(event, fn)` calls `fn` whenever `event` happens, and `QuestUI.off(event, fn)` stops it. The player forgets every function when a saved game is loaded, as your JavaScript files are loaded again then, so call `on` from your `.js` file.

| Event | When | `fn` gets |
|---|---|---|
| `turnStarted` | The player starts a turn - a command, a click on a link, or an answer to a menu or question - before the game sees it | nothing |
| `turnEnded` | The game has finished the turn, and its text is on screen | nothing |
| `outputAdded` | Text is added to the game text | `{ html }` |
| `locationChanged` | The location bar changes | `{ location }` |

Timers don't start turns. See [Reacting to the player's turns](/customise/javascript#reacting-to-the-players-turns) for an example.

## Theming

| Member | What it does |
|---|---|
| `setTokens(tokens)` | Sets the player's `--qv-` custom properties, from an object of names and values. See [Custom properties](/customise/css#custom-properties) |
| `setCommandBarStyle(css)` | Styles the command bar |
| `setCommandBarPreset(name)` | `borderless`, a command bar with no box, or `shadow` |
| `setCommandPrompt(text)` | The prompt shown before the command bar |
| `addStylesheet(url)` | Loads a stylesheet - use [AddExternalStylesheet](/reference/functions/internal-core#addexternalstylesheet) |

These are what the Appearance tab's options use. To change the look yourself, the game's [stylesheet](/customise/css) is simpler, and is applied again for you when a saved game is loaded.

## The map and the transcript

`QuestUI.map` draws the map, and `QuestUI.transcript` records the transcript. Quest Viva's own libraries call them, and [Showing a map](/howto/rooms/map) is the way to use the map from a game.
