---
title: JS functions
sidebar:
  order: 27
---

The `JS` object is how Quest exposes the user interface. What this means is that we can use the JS object to call JavaScript functions that will modify what the player sees. The basic format is to append the JavaScript function name with a dot, so to call `addText` (the JavaScript function Quest uses to show text on the screen), use something like this:

```
JS.addText("You are in a deep hole.")
```

## addScript

    JS.addScript (string text)

Inserts the text into the HTML document. This can be used for adding JavaScript or CSS, or for adding HTML that will be out of the normal sequence, such as a custom pane or dialogue panel. Use [addText](#addtext) for game text.

```
JS.addScript("<script>function setNumber(n) { number = n; }</script>")
```

## addText

    JS.addText (string text)

Inserts the given text into the page. This is how `msg` displays text. Use [addScript](#addscript) to add code, such as CSS or JavaScript, or to add HTML outside the normal text flow.

## colourBlend

    JS.colourBlend (string colour1, string colour2)

Sets a colour blend as the background, going from colour1 at the top to colour2 at the bottom.

```
JS.colourBlend("red", "#ff0080")
```

## eval

    JS.eval (string JavaScript code)

Causes the given string to be evaluated by the JavaScript engine. This is a way to run any JavaScript code in your game, which can be used to move around the components of the UI or add new ones, among other things.

## scrollToEnd

    JS.scrollToEnd ()

Moves the displayed text down to the bottom. This should happen automatically, but occasionally it is useful to be able to call it yourself from your game.

## setCommands

    JS.setCommands(string commands, string colour)

Sets the commands to be displayed on the command pane (turn the command pane on on the _Interface_ script of the game object). Commands should be sent as a string, separated by semi-colons. The colour of the text can be specified, but is optional.

```
JS.setCommands("Wait;Look")
JS.setCommands("Wait;Look;Get apple", "red")
```

## setCss

    JS.setCss (string element name, string css styling)

Sets the CSS styling for the given element. If the element name is for an ID, this should be prefixed with a #. CSS styling should be given as a serious of name-value pairs, each pair separated by a semi-colon, with a colon between the name and the value.

This example sets the `<body>` element to have the "serif" font.

```
JS.setCSS ("body", "font-family: serif")
```

This example sets styling for the element with the ID "status" (the strip across the top of the screen). It sets two properties, the background image and background colour.

```
JS.setCSS ("#status", "background-image:none; background-color: green;")
```

## setCustomStatus

    JS.setCustomStatus(string html)

Sets the HTML text to be displayed on the custom status pane (turn the command pane on on the _Interface_ script of the game object). This is an involved issue, rather than give an example, go see this [page](/howto/ux/custom_panes).

## setInterfaceString

    JS.setInterfaceString(string name, string value)

Use this to set the text of the various elements of the user interface. The values allowed for the name are:

> InventoryLabel, StatusLabel, PlacesObjectsLabel, CompassLabel
> InButtonLabel, OutButtonLabel
> EmptyListLabel, NothingSelectedLabel, TypeHereLabel, ContinueLabel

For example, to change the name of the player inventory:

```
JS.setInterfaceString("InventoryLabel", "You are holding")
```

## setPanes

    JS.setPanes (string text, string background)
    JS.setPanes (string text, string background, string text2, string background2)
    JS.setPanes (string text, string background, string text2, string background2, string highlight)

Sets the colours for the panes on the right. You can use either two, four or five parameters.

The text will be in `fore`, whilst the background will be in `back`. When an object is selected, it will be in `secFore` on a `secBack` background, if given, otherwise it will be `back` on a `fore` background (i.e., reversed colours). When a player is clicking on an object, the background will be the `highlight` colour, or orange if not given.

```
JS.setPanes("black", "white")
JS.setPanes("black", "white", "white", "#444")
JS.setPanes("black", "white", "white", "#444", blue)
```

## ShowGrid

    JS.ShowGrid (int height)

Sets the height for the grid map. Setting this to zero turns the map off, setting it to any other values turns it on.

## showPopup

    JS.showPopup(title, text)

Shows a pop up, with an okay button, which the player can click to close. This version has a fixed width (of 300 px when I checked), and the height will expand up to the full Quest windows size to accommodate the text.

```
JS.showPopup("Hi!", "This is where it all begins")
```

## showPopupCustomSize

    JS.showPopupCustomSize(title, text, int width, int height)

As [showPopup](#showpopup), but allows for custom width and height to be set; scrollbars will be added if the text is too long.

## showPopupFullscreen

    JS.showPopupFullscreen(title, text)

As [showPopup](#showpopup), but will fill the Quest window (so the size will depend on how the player has it set up).

## uiShow

    JS.uiShow(string element)

Shows the given element. Allowed values are:

> #txtCommandDiv, #location, #status

For example,

```
JS.uiShow("#status")
```

## uiHide

    JS.uiHide(string element)

Hides the given element (see [uiShow](#uishow) for the allowed values).

## updateLocation

    JS.updateLocation(string text)

Changes the location in the location bar at the top.

```
JS.updateLocation("Dining Room")
```

## updateStatus

    JS.updateStatus(string text)

Puts the given text into the status pane on the right. This should be formatted in HTML; for example, use <br/> to indicate a new line.

```
JS.updateStatus("Money: $45<br/>Health: 23")
```
