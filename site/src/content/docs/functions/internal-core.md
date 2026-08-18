---
title: "Internal Core.aslx Functions"
sidebar:
  order: 14
---

Most games shouldn't need to call these directly.

## AddExternalStylesheet

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

AddExternalStylesheet(stylesheet)

## AddStatusAttributesForElement

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

AddStatusAttributesForElement(status, element)

## CloseObject

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

CloseObject(object, ismultiple)

## CommandLink

CommandLink (string command, string link text)

Returns a [string](/types#string) containing the XML required to display a hyperlink. When the hyperlink is clicked, the specified player command will be run. For example:

     msg (CommandLink("undo", "Click here to undo the previous turn"))

outputs a link titled "Click here to undo the previous turn" - when clicked, the "undo" player command is run.

See also: [ObjectLink](#objectlink)

## CompareNames

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

CompareNames(name, value, obj, fullmatches, partialmatches)

## ContainsAccessible

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    ContainsAccessible (object parent, object child, boolean reachable)

Returns a [boolean](/types#boolean).

## ContainsReachable

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    ContainsReachable (object parent, object child)

Returns a [boolean](/types#boolean).

## ContainsVisible

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    ContainsVisible (object parent, object child)

Returns a [boolean](/types#boolean).

## DisplayHttpLink

DisplayHttpLink(string displaylink,string url, boolean https)

Displays a weblink in the gamewindow. Clicking on **displaylink** will open the **url** in an external webbrowser. You can specify to use HTTPS or not.

## DoAskTell

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

DoAskTell(object, text, property, defaultscript, defaulttemplate)

## DoDrop

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

DoDrop(object, ismultiple)

## DoTake

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

DoTake(object, ismultiple)

## FormatStatusAttribute

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

FormatStatusAttribute(attr, value, format)

## GenerateMenuChoices

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

GenerateMenuChoices(dictionary, objects)

## GetDefaultPrefix

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

GetDefaultPrefix(object)

Should be defined by the language library (but is only used within the language library, so it is safe to not define this if it is not used). When [usedefaultprefix](/attributes#usedefaultprefix) is in use for an object, this should generate the relevant default.

## GetKeywordsMatchStrength

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

GetKeywordsMatchStrength(keywords, input)

## GetPlacesObjectsList

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

GetPlacesObjectsList()

Required by Quest so it can display the "Places and Objects" list. Returns [ScopeVisibleNotHeldNotScenery](/functions/scope#scopevisiblenotheldnotscenery) with the player object excluded.

## GetTaggedName

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    GetTaggedName (object, string type, stringlist verbs)

Returns a [string](/types#string).

**This function was removed in Quest 5.4**

## GetUniqueElementName

GetUniqueElementName (string element name)

Returns a [string](/types#string) containing the specified name - if necessary with a number added to ensure it is an unused element name.

NOTE: This a [hard-coded function](/functions/hardcoded).

## Grid CalculateMapCoordinates

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

Grid\_CalculateMapCoordinates(room)

## Grid DrawPlayerInRoom

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

Grid\_DrawPlayerInRoom(room)

## Grid DrawRoom

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

Grid DrawRoom(room, redraw)

## Grid SetScale

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

Grid\_SetScale(scale)

## Grid ShowCustomLayer

Grid ShowCustomLayer (boolean visible)

Turn the custom grid drawing layer on or off.

## Grid_AddNewShapePoint

Grid_AddNewShapePoint (int x, int y)

On the custom grid drawing layer, adds a new point to a shape. To draw a custom shape, call this function for each point on the shape, then call [Grid\_DrawShape](#grid_drawshape) to complete the drawing.

## Grid_ClearCustomLayer

Grid_ClearCustomLayer ()

Clears the custom grid drawing layer.

## Grid_DrawArrow

Grid_DrawArrow (string id, int x1, int y1, int x2, int y2, string border, int borderwidth)

On the custom grid drawing layer, draws an arrow of the specified border colour and thickness from (x1,y1) to (x2,y2). The id is arbitrary - if reused, the existing arrow with the same id will be removed.

## Grid_DrawGridLines

Grid_DrawGridLines (int x1, int y1, int x2, int y2, string border)

On the custom grid drawing layer, draws a grid with line of the specified border from (x1,y1) to (x2,y2).

## Grid_DrawImage

Grid_DrawImage (string id, string url, int x, int y, int width, int height)

On the custom grid drawing layer, draws the image from the specified URL. The id is arbitrary - if you re-use the same id, the existing image will be removed.

## Grid_DrawLine

Grid_DrawLine (int x1, int y1, int x2, int y2, string border, int borderwidth)

On the custom grid drawing layer, draws a line of the specified border colour and thickness from (x1,y1) to (x2,y2).

## Grid_DrawShape

Grid_DrawShape (string id, string border, string fill, double opacity)

On the custom grid drawing layer, draws an arbitrary shape. First, specify all points using [Grid\_AddNewShapePoint](#grid_addnewshapepoint). Then call this function to place the drawing on the grid, with the specified border and fill colour and opacity between 0 and 1. The id is arbitrary - if reused, an existing shape will be replaced with this one.

## Grid_DrawSquare

Grid_DrawSquare (string id, int x, int y, int width, int height, string text, string fill)

On the custom grid drawing layer, draws a square at (x,y) with the specified width and height. A fill colour and some text to display in the centre of the square can also be specified.

## Grid_DrawSvg

Grid_DrawSvg (string instance id, string symbol id, int x, int y, int width, int height)

On the custom grid drawing layer, draws the specified SVG file (the symbol id must have been previously loaded using [Grid\_LoadSvg](#grid_loadsvg). The instance id is arbitrary - if you re-use the same instance id, the existing symbol will be removed.

## Grid_LoadSvg

Grid_LoadSvg (string data, string id)

Loads and SVG file and associates it with an id, so it can subsequently be drawn on the custom grid drawing layer using [Grid\_DrawSvg](#grid_drawsvg).

The data parameter is the raw file data for the SVG file - you can load a string with file data using the [GetFileData](/functions/general#getfiledata) function.

## Grid_Redraw

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

Grid\_Redraw

## Grid_SetCentre

Grid_SetCentre (int x, int y)

Centres the grid on the specified co-ordinates.

## HandleCommand

HandleCommand (command)

Parses the specified command.

## HandleSingleCommand

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

HandleSingleCommand(command)

## HandleSingleCommandPattern

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

HandleSingleCommandPattern(command, thiscommand, varlist)

## InitInterface

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    InitInterface

Does not return a value.

Quest will look for a function called InitInterface in any ASLX file, and if one exists then it will be called when the game begins, and also when loading a saved game.

Core.aslx defines an implementation of a InitInterface function. It does the following:

-   sets up the default game fonts and colours
-   sets up compass direction names
-   sets titles of panes ("Inventory", "Places and Objects" etc.)
-   shows or hides panes depending on the [showpanes](/attributes#showpanes) option

## InitPOV

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

InitPOV(oldPOV, newPOV)

## InitVerbsList

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

InitVerbsList

## IsGameRunning

IsGameRunning ()

Returns a [boolean](/types#boolean) indicating whether the game is currently running (i.e. false when the game has finished).

NOTE: This a [hard-coded function](/functions/hardcoded).

## ListObjectContents

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

ListObjectContents(object)

Lists the contents of the specified object, only if [isopen](/attributes#isopen) and [listchildren](/attributes#listchildren) are set.

## ObjectLink

ObjectLink (object)

Returns a [string](/types#string) containing the XML required to display a hyperlink for the specified object, with its display verbs.

See also: [CommandLink](#commandlink)

## OnEnterRoom

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    OnEnterRoom ()

Does not return a value.

## OpenObject

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

OpenObject(object)

## Populate

Populate (string regex, string input)

There is also an optional cache ID parameter:

    Populate (string regex, string input, string cache ID)

The input must be a match for the regular expression, or an error occurs.

Returns a [stringdictionary](/types#stringdictionary), keyed by the group names in the regular expression, with values set to the resolved regex groups.

Use a cache ID for improved performance if you repeatedly test strings against the same regular expression. The compiled regular expression will be cached and used again for subsequent calls to Populate (or [GetMatchStrength](/functions/string#getmatchstrength) or [IsRegexMatch](/functions/string#isregexmatch) ) using the same cache ID.

For example, given this regex which matches the text "put (object name) on (object name)":

    put (<object1>.*) on (<object2>.*)

Passing this to the Populate function with an input "put book on shelf" will return a [stringdictionary](/types#stringdictionary) where object1="book" and object2="shelf".

See also [GetMatchStrength](/functions/string#getmatchstrength), [IsRegexMatch](/functions/string#isregexmatch)

NOTE: This a [hard-coded function](/functions/hardcoded).

## ResolveName

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

ResolveName(value, objtype)

## ResolveNameInternal

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

ResolveNameInternal(value, objtype)

## ResolveNameList

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

ResolveNameList(value, scope, objtype, resultdictionary)

## ResolveNameListItem

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

ResolveNameListItem(value, scope, objtype, resultdictionary)

## RunTurnScripts

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

RunTurnScripts()

## StartGame

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    StartGame

Does not return a value.

Quest will look for a function called StartGame, and if one exists then it will be called when the game begins, except if the game is being loaded from a .quest-save file.

Core.aslx defines an implementation of a StartGame function. It does the following:

-   updates status attributes
-   if the [game](/elements#game) object has a "start" script attribute, runs that
-   displays the initial room description

## TryOpenClose

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

    TryOpenClose (boolean open, object)

Does not return a value.
