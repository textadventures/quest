---
layout: index
title: The JS Object
---

The `JS` object is how Quest exposes the user interface. What this means is that we can use the JS object to call JavaScript functions that will modify what the player sees. The basic format is to append the JavaScript function name with a dot, so to call `addText` (the JavaScript function Quest uses to show text on the screen), use something like this:

```
JS.addText("You are in a deep hole.")
```

Below is an alphabetic list of the more commonly used functions. If you add your own JavaScript functions, they too can be accessed through the `JS` object.

---


### addText(string text)

Adds the given text to the game output. Each call of this creates a new HTML `div`, with its own number, on the screen. If you want to add code (JavaScript, CSS or whatever), use `addScript`.

```
JS.addText("You are in the dining room.<br/>You can see a table.")
```



### addScript(string text)

Inserts the text into the HTML document. This can be used for adding JavaScript or CSS, or for adding HTML that will be out of the normal sequence, such as a custom pane or dialogue panel. Use `addText` for game text.

```
JS.addText("<script>function setNumber(n) { number = n; }</script>")
```



### colourBlend(string colour1, string colour2)

Sets a colour blend as the background, going from colour1 at the top to colour2 at the bottom.

```
JS.colourBlend("red", "#ff0080")
```


### eval(string javascript code)

This is the one function to rule them all - it can be used to do nearly anything. The given string, presumed to be JavaScript code is run. Obviously a fair knowledge of JavaScript is required. Note that it does not return a value; you cannot use this to determine the state of the UI or the value of a JavaScript variable (see [ASLEvent](http://docs.textadventures.co.uk/quest/ui-callback.html) for that).

```
JS.eval ("$('#commandPane').insertBefore('#inventoryLabel')")
```



### panesVisible(boolean visible)

Shows or hides the panes on the right. For example, to turn them on:

```
JS.panesVisible(true)
```



### scrollToEnd() 

Quest should scroll to the end of the text automatically, but you can use this to try to force it.

```
JS.scrollToEnd()
```



### setBackground(string col)

Sets the background colour of the game pane.

```
JS.setBackground("ff00ff")
JS.setBackground("red")
```



### SetBackgroundImage(string url)

Sets the background image of the outer pane.

```
JS.SetBackgroundImage(GetFileURL(filename))
```



### SetBackgroundOpacity(float opacity)

Sets the background opacity of the game pane. This should be a value from 1.0 (opaque) to 0.0 (invisible).

```
JS.SetBackgroundOpacity(0.5)
```



### setCommands(string commands, string colour)

Sets the commands to be displayed on the command pane (turn the command pane on on the _Interface_ script of the game object). Commands should be sent as a string, separated by semi-colons. The colour of the text can be specified, but is optional.

```
JS.setCommands("Wait;Look")
JS.setCommands("Wait;Look;Get apple", "red")
```



### setCss(string element, string cssString)

Set the CSS values of the given element. Elements identified by their ID need to be start with a hash, whilst elements identified by their class need to start with a full-stop. Different properties can be separated by semicolons, whilst the name and value should be separated by a colon.

```
JS.setCss("body", "background:red")
JS.setCss(".cmdlink", "color:blue")
JS.setCss("#location", "color:#ff8080;font-size:10pt;font-weight:heavy")
```



### setCustomStatus(string html)

Sets the HTML text to be displayed on the custom status pane (turn the command pane on on the _Interface_ script of the game object). This is an involved issue, rather than give an example, go see this [page](custom_panes.html).



### setInterfaceString(string name, string value)

Use this to set the text of the various elements of the user interface. The values allowed for the name are:

> InventoryLabel, StatusLabel, PlacesObjectsLabel, CompassLabel
> InButtonLabel, OutButtonLabel
> EmptyListLabel, NothingSelectedLabel, TypeHereLabel, ContinueLabel

For example, to change the name of the player inventory:

```
JS.setInterfaceString("InventoryLabel", "You are holding")
```



### setPanes(string fore, string back, string secFore, string secBack, string highlight)

Sets the colours for the panes on the right. You can use either two, four or five parameters.

The text with be in `fore`, whilst the background will be in `back`. When an object is selected, it will be in `secFore` on a `secBack` background, if given, otherwise it will be `back` on a `fore` background (i.e., reversed colours). When a player is clicking on an object, the background will be the `highlight` colour, or orange if not given.

```
JS.setPanes("black", "white")
JS.setPanes("black", "white", "white", "#444")
JS.setPanes("black", "white", "white", "#444", blue)
```



### SetMenuBackground(string color)

### SetMenuForeground(string color)

### SetMenuHoverBackground(string color)

### SetMenuHoverForeground(string color)

### SetMenuFontName(string font)

### SetMenuFontSize(int size)

This set of functions can be used to modify the appearance of menu you see when clicking an object hyperlink.

```
JS.SetMenuBackground("#808080")
JS.SetMenuForeground("white")
JS.SetMenuHoverBackground("yellow")
JS.SetMenuHoverForeground("red")
JS.SetMenuFontName("serif")
JS.SetMenuFontSize(8)
```



### ShowGrid(int height)

Sets the height of the map. Set it to zero to turn the map off.

```
JS.ShowGrid(160)
```


### showPopup(title, text)

### showPopupCustomSize(title, text, int width, int height)

### showPopupFullscreen(title, text)

Shows a pop up, with an okay button, which the player can click to close. The first version has a fixed width (of 300 px when I checked), and the height will expand up to the full Quest windows size to accommodate the text. The second version allows for custome width and height to be set; scrollbars will be added if the text is too long. The third version will fill the Quest window (so the size will depend on how the player has it set up).

```
JS.showPopup("Hi!", "This is where it all begins")
```



### uiShow(string element)

### uiHide(string element)

Shows or hides the given element. Allowed values are:

> #txtCommandDiv, #location, #status

For example,

```
JS.uiShow("#status")
```



### updateLocation(string text)

Changes the location in the location bar at the top.

```
JS.updateLocation("Dining Room")
```



### updateStatus(string text)

Puts the given text into the status pane on the right. This should be formated in HTML; for example, use <br/> to indicate a new line.

```
JS.updateStatus("Money: $45<br/>Health: 23")
```


