---
layout: index
title: request
---

    request (request name, string parameter)

Raises a UI request. The request name must be specified directly - it is not a string expression. For example:

```
request(UpdateLocation, "The Kitchen")
```

The `request` script command is really a throw-back to the original Quest 5.0 interface, which, while it did use HTML, was not a fully-fledged browser. As of 5.3, the interface is a version of Chrome embedded in the software, and all interaction between the game world and the interface is done with JavaScript. Since then `request` has become increasingly obsolete, and it is recommended that wherever possible, an alternative is used (even where the alternative uses `request` behind the scenes).

Valid request names and parameters (and alternatives):


_Quit_  

Quits the game. Parameter is ignored. Use `finish` instead.


_UpdateLocation_

Updates the location bar at the top of the screen with the parameter text. Use JS instead:

```
JS.updateLocation("The kitchen")
```


_GameName_

Sets the name of the game. Do this instead:

```
JS.setGameName("My Cool Game")
```


_FontName_

(Obsolete as of Quest 5.4) Sets the font name. Use [SetFontName](../functions/corelibrary/setfontname.html) instead.


_FontSize_

(Obsolete as of Quest 5.4) Sets the font size. Use [SetFontSize](../functions/corelibrary/setfontsize.html) instead.


_Background_

Sets the background to the specified HTML colour. Use [SetBackgroundColour](../functions/corelibrary/setbackgroundcolour.html) instead.


_Foreground_

Sets the foreground to the specified HTML colour. Use [SetForegroundColour](../functions/corelibrary/setforegroundcolour.html) instead.


_LinkForeground_

Sets the link foreground to the specified HTML colour. As of 5.7.2, use `SetLinkForegroundColour` instead.


_RunScript_

Runs the specified JavaScript function. A far better way is to use the JS object, which can be used to access any built-in JavaScript function or any you add yourself.

```
JS.myCustomFunction(15, "some string)
```


_SetStatus_

Sets the text for the status area on the right of the screen (under "Inventory"). If blank, the status area is removed. This is best done using [status attributes](../using_attributes.html).


_ClearScreen_

Clears the screen. Parameter is ignored. Use `ClearScreen` instead.


_PanesVisible_  

Sets whether the panes on the right of the screen are displayed. Valid values are "on" and "off" (toggling whether panes are shown), and "disabled" (turns panes off and removes the button which would let the player turn them back on - this button appears to no longer be available). Instead use:

```
JS.panesVisible(true)
JS.panesVisible(false)
```


_ShowPicture_  

Shows the specified picture file from the game directory. Use [picture](picture.html) instead.


_Show_

Turns on an interface element. Valid elements are "Panes", "Location" and "Command". Instead use `JS.uiShow`.

```
JS.uiShow("#gamePanes")
JS.uiShow("#location")
JS.uiShow("#txtCommandDiv")
```

You can also selectively hide or show one pane (if games panes are shown). Note that each pane has two components, so to hide the compass:

```
JS.uiHide("#compassLabel")
JS.uiHide("#compassAccordion")
```

For the inventory, do `#inventoryLabel` and `#inventoryAccordion`; for the places and objects pane, `#placesObjectsLabel` and `#placesObjectsAccordion`. For the custom status pane and the custom command pane, use `#custonStatusPane` and `#commandPane` respectively (these have only one part).


_Hide_

Turns off an interface element. Valid elements are "Panes", "Location" and "Command". Use `JS.uiHide(element)` instead (see above for details).


_SetCompassDirections_

Takes a semi-colon separated list of compass direction names and assigns them to the compass buttons. As of Quest 5.7.2, use `JS` instead, for example:

```
JS.setCompassDirections("northwest;north;northeast;west;east;southwest;whatever;southeast;up;down;in;out")
```

These names will also then not appear as exits in the "Places and Objects" list. The default is as shown in the example. The compass directions must be specified in the same order and there must be the same number of elements in the list. The exit in the compass rose will only be active if the alias of the exit matches the text you set here.


_Pause_  

(Obsolete as of Quest 5.5) Pauses the game for the specified number of milliseconds.


_Wait_

Waits for the player to press a key. The parameter is ignored. Deprecated as of Quest 5.1 and unsupported as of Quest 5.4 - use the [wait](wait.html) script command instead.


_SetInterfaceString_

Takes a parameter of the form "ElementName=Value", to set the text in the user interface. Do this instead:

```
JS.setInterfaceString("PlacesObjectsLabel", "You can see:")
```

Either way, valid element names are:

-   InventoryLabel (default "Inventory")
-   PlacesObjectsLabel (default "Places and Objects")
-   CompassLabel (default "Compass")
-   InButtonLabel (default "in")
-   OutButtonLabel (default "out")
-   EmptyListLabel (default "(empty)")
-   NothingSelectedLabel (default "(nothing selected)")


_RequestSave_

Requests the UI to save the game - this may bring up a "Save As" dialog if the user has not yet saved their progress. Parameter is ignored. As of Quest 5.7.2, use:

```
RequestSave()
```


_SetPanelContents_

Sets the static panel HTML contents. Use `SetFramePicture` and `ClearFramePicture` instead.


_Log_

(New in Quest 5.3) Log the specified text. Use [Log](../functions/corelibrary/log.html) instead.


_Speak_  

(New in Quest 5.4) Output text to speech synthesizer if enabled. As of Quest 5.7.2, use `RequestSpeak` instead:

```
RequestSpeak("Hello World")
```
