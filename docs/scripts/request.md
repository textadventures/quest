---
layout: index
title: request
---

    request (request name, string parameter)

Raises a UI request. The request name must be specified directly - it is not a string expression.

Valid request names and parameters:

Quit  
Quits the game. Parameter is ignored.

UpdateLocation  
Updates the location bar at the top of the screen with the parameter text.

GameName  
Sets the name of the game.

FontName  
(Obsolete as of Quest 5.4) Sets the font name.

FontSize  
(Obsolete as of Quest 5.4) Sets the font size.

Background  
Sets the background to the specified HTML colour.

Foreground  
Sets the foreground to the specified HTML colour.

LinkForeground  
Sets the link foreground to the specified HTML colour.

RunScript  
Runs the specified Javascript function. To specify parameters for the function, separate them with semicolons, e.g. "myfunction; parameter1; parameter2".

SetStatus  
Sets the text for the status area on the right of the screen (under "Inventory"). If blank, the status area is removed.

ClearScreen  
Clears the screen. Parameter is ignored.

PanesVisible  
Sets whether the panes on the right of the screen are displayed. Valid values are "on" and "off" (toggling whether panes are shown), and "disabled" (turns panes off and removes the button which would let the player turn them back on).

ShowPicture  
Shows the specified picture file from the game directory.

Show  
Turns on an interface element. Valid elements are "Panes", "Location" and "Command".

Hide  
Turns off an interface element. Valid elements are "Panes", "Location" and "Command".

SetCompassDirections  
Takes a semi-colon separated list of compass direction names and assigns them to the compass buttons. These names will also then not appear as exits in the "Places and Objects" list. The default is:

<!-- -->

     northwest;north;northeast;west;east;southwest;south;southeast;up;down;in;out

The compass directions must be specified in the same order and there must be the same number of elements in the list.

Pause  
(Obsolete as of Quest 5.5) Pauses the game for the specified number of milliseconds.

Wait  
Waits for the player to press a key. The parameter is ignored. Deprecated as of Quest 5.1 and unsupported as of Quest 5.4 - use the [wait](wait.html) script command instead.

SetInterfaceString  
Takes a parameter of the form "ElementName=Value", to set the text in the user interface. Valid element names are:

-   InventoryLabel (default "Inventory")
-   PlacesObjectsLabel (default "Places and Objects")
-   CompassLabel (default "Compass")
-   InButtonLabel (default "in")
-   OutButtonLabel (default "out")
-   EmptyListLabel (default "(empty)")
-   NothingSelectedLabel (default "(nothing selected)")

RequestSave  
Requests the UI to save the game - this may bring up a "Save As" dialog if the user has not yet saved their progress. Parameter is ignored.

SetPanelContents  
Sets the static panel HTML contents.

Log  
(New in Quest 5.3) Log the specified text

Speak  
(New in Quest 5.4) Output text to speech synthesizer if enabled


