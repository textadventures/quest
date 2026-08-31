---
title: JS functions
sidebar:
  order: 27
---

The `JS` object is how Quest exposes the user interface. What this means is that we can use the JS object to call JavaScript functions that will modify what the player sees. The basic format is to append the JavaScript function name with a dot, so to call `addText` (the JavaScript function Quest uses to show text on the screen), use something like this:

```quest
JS.addText("You are in a deep hole.")
```

## addExternalStylesheet

```quest
JS.addExternalStylesheet (string url)
```

Adds a `<link rel="stylesheet">` for the given URL to the page, loading an external CSS file.

## addScript

```quest
JS.addScript (string text)
```

Inserts the text into the HTML document. This can be used for adding JavaScript or CSS, or for adding HTML that will be out of the normal sequence, such as a custom pane or dialogue panel. Use [addText](#addtext) for game text.

```xml
JS.addScript("<script>function setNumber(n) { number = n; }</script>")
```

## addText

```quest
JS.addText (string text)
```

Inserts the given text into the page. This is how `msg` displays text. Use [addScript](#addscript) to add code, such as CSS or JavaScript, or to add HTML outside the normal text flow.

## AddYouTube

```quest
JS.AddYouTube (string id)
```

Embeds an autoplaying YouTube video for the given video ID. See [Adding videos](/howto/multimedia/adding_videos).

## colourBlend

```quest
JS.colourBlend (string colour1, string colour2)
```

Sets a colour blend as the background, going from colour1 at the top to colour2 at the bottom.

```quest
JS.colourBlend("red", "#ff0080")
```

## eval

```quest
JS.eval (string JavaScript code)
```

Causes the given string to be evaluated by the JavaScript engine. This is a way to run any JavaScript code in your game, which can be used to move around the components of the UI or add new ones, among other things.

## Grid_ClearAllLayers

```quest
JS.Grid_ClearAllLayers ()
```

Clears everything drawn on the map grid - rooms and any custom layers. Used when resetting the map entirely, e.g. when the player teleports to an unconnected region. See [Showing a map](/howto/tasks/showing_a_map).

## hideBorder

```quest
JS.hideBorder ()
```

Removes the border around the game area.

## panesVisible

```quest
JS.panesVisible (boolean visible)
```

Shows or hides the panes on the right of the screen.

```quest
JS.panesVisible(false)
```

## scrollToEnd

```quest
JS.scrollToEnd ()
```

Moves the displayed text down to the bottom. This should happen automatically, but occasionally it is useful to be able to call it yourself from your game.

## setBackground

```quest
JS.setBackground (string colour)
```

Sets the background colour of the game area.

## setCommands

```quest
JS.setCommands(string commands, string colour)
```

Sets the commands to be displayed on the command pane (turn the command pane on on the _Interface_ script of the game object). Commands should be sent as a string, separated by semi-colons. The colour of the text can be specified, but is optional.

```quest
JS.setCommands("Wait;Look")
JS.setCommands("Wait;Look;Get apple", "red")
```

## setCompassDirections

```quest
JS.setCompassDirections (string directions)
```

Takes a semicolon-separated list of names for the twelve compass directions - northwest, north, northeast, west, east, southwest, south, southeast, up, down, in, out, in that order - and uses them as the tooltip text for the compass buttons. These names also then stop appearing as exits in the "Places and Objects" list.

```quest
JS.setCompassDirections("northwest;north;northeast;west;east;southwest;south;southeast;up;down;in;out")
```

The compass directions must be specified in the same order and with the same number of elements as the default shown above. The exit in the compass rose is only active if the alias of the exit matches the text set here.

## setCss

```quest
JS.setCss (string element name, string css styling)
```

Sets the CSS styling for the given element. If the element name is for an ID, this should be prefixed with a #. CSS styling should be given as a serious of name-value pairs, each pair separated by a semi-colon, with a colon between the name and the value.

This example sets the `<body>` element to have the "serif" font.

```quest
JS.setCss ("body", "font-family: serif")
```

This example sets styling for the element with the ID "qv-status" (the strip across the top of the screen). It sets two properties, the background image and background colour.

```quest
JS.setCss ("#qv-status", "background-image:none; background-color: green;")
```

## setCustomStatus

```quest
JS.setCustomStatus(string html)
```

Sets the HTML text to be displayed on the custom status pane (turn the command pane on on the _Interface_ script of the game object). This is an involved issue, rather than give an example, go see this [page](/howto/ux/custom_panes).

## setGameName

```quest
JS.setGameName (string name)
```

Sets the name of the game, shown in the browser tab title.

```quest
JS.setGameName("My Cool Game")
```

## setGamePadding

```quest
JS.setGamePadding (string top, string bottom, string left, string right)
```

Sets the padding (CSS values, e.g. `"10px"`) around the game text.

## setGameWidth

```quest
JS.setGameWidth (int width)
```

Sets the maximum width, in pixels, of the game area.

## setInterfaceString

```quest
JS.setInterfaceString(string name, string value)
```

Use this to set the text of the various elements of the user interface. The values allowed for the name are:

> InventoryLabel, StatusLabel, PlacesObjectsLabel, CompassLabel
> InButtonLabel, OutButtonLabel
> EmptyListLabel, NothingSelectedLabel, TypeHereLabel, ContinueLabel

For example, to change the name of the player inventory:

```quest
JS.setInterfaceString("InventoryLabel", "You are holding")
```

## SetMenuBackground

```quest
JS.SetMenuBackground (string colour)
```

Sets the background colour of the popup menu shown when the player clicks a hyperlink in the text.

## SetMenuFontName

```quest
JS.SetMenuFontName (string fontName)
```

Sets the font used in the hyperlink popup menu.

## SetMenuFontSize

```quest
JS.SetMenuFontSize (string size)
```

Sets the font size used in the hyperlink popup menu. The size must be given as a number followed by `"pt"`, e.g. `"14pt"`.

## SetMenuForeground

```quest
JS.SetMenuForeground (string colour)
```

Sets the text colour of the hyperlink popup menu.

## SetMenuHoverBackground

```quest
JS.SetMenuHoverBackground (string colour)
```

Sets the background colour of a hyperlink popup menu item when hovered over.

## SetMenuHoverForeground

```quest
JS.SetMenuHoverForeground (string colour)
```

Sets the text colour of a hyperlink popup menu item when hovered over.

## setPanes

```quest
JS.setPanes (string text, string background)
JS.setPanes (string text, string background, string text2, string background2)
JS.setPanes (string text, string background, string text2, string background2, string highlight)
```

Sets the colours for the panes on the right. You can use either two, four or five parameters.

The text will be in `fore`, whilst the background will be in `back`. When an object is selected, it will be in `secFore` on a `secBack` background, if given, otherwise it will be `back` on a `fore` background (i.e., reversed colours). When a player is clicking on an object, the background will be the `highlight` colour, or orange if not given.

```quest
JS.setPanes("black", "white")
JS.setPanes("black", "white", "white", "#444")
JS.setPanes("black", "white", "white", "#444", blue)
```

## ShowGrid

```quest
JS.ShowGrid (int height)
```

Sets the height for the grid map. Setting this to zero turns the map off, setting it to any other values turns it on.

## showPopup

```quest
JS.showPopup(title, text)
```

Shows a pop up, with an okay button, which the player can click to close. This version has a fixed width (of 300 px when I checked), and the height will expand up to the full Quest windows size to accommodate the text.

```quest
JS.showPopup("Hi!", "This is where it all begins")
```

## showPopupCustomSize

```quest
JS.showPopupCustomSize(title, text, int width, int height)
```

As [showPopup](#showpopup), but allows for custom width and height to be set; scrollbars will be added if the text is too long.

## showPopupFullscreen

```quest
JS.showPopupFullscreen(title, text)
```

As [showPopup](#showpopup), but will fill the Quest window (so the size will depend on how the player has it set up).

## showStatusVisible

```quest
JS.showStatusVisible (boolean visible)
```

Shows or hides the status variables pane (see [status attributes](/status_attributes)).

## TurnOffHyperlinksUnderline

```quest
JS.TurnOffHyperlinksUnderline ()
```

Removes the underline from in-text command hyperlinks.

## uiHide

```quest
JS.uiHide(string element)
```

Hides the given element the same way (see [uiShow](#uishow) for the available selectors). `#gamePanes` is special-cased to behave the same as `panesVisible(false)`.

```quest
JS.uiHide("#compassLabel")
JS.uiHide("#compassAccordion")
```

## uiShow

```quest
JS.uiShow(string element)
```

Shows the given element - any CSS selector works. `#gamePanes` is special-cased to behave the same as `panesVisible(true)`.

```quest
JS.uiShow("#gamePanes")
JS.uiShow("#location")
JS.uiShow("#txtCommandDiv")
```

You can also selectively show or hide one pane (if game panes are shown). Each pane has two components, so to show/hide the compass: `#compassLabel` and `#compassAccordion`. For the inventory, use `#inventoryLabel` and `#inventoryAccordion`; for the places and objects pane, `#placesObjectsLabel` and `#placesObjectsAccordion`. For the custom status pane and the custom command pane, use `#customStatusPane` and `#commandPane` respectively (these have only one part).

## updateLocation

```quest
JS.updateLocation(string text)
```

Changes the location in the location bar at the top.

```quest
JS.updateLocation("Dining Room")
```

## updateStatus

```quest
JS.updateStatus(string text)
```

Puts the given text into the status pane on the right. This should be formatted in HTML; for example, use <br/> to indicate a new line.

```xml
JS.updateStatus("Money: $45<br/>Health: 23")
```
