---
title: "Internal Core.aslx functions"
sidebar:
  order: 14
---

Most games shouldn't need to call these directly.

## _DoRemove
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_DoRemove(object)
```

## _DoWear
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_DoWear(object)
```

## _GetList
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_GetList(char, slot)
```

## _GetOuterForGarment
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_GetOuterForGarment(char, garment)
```

## _GetSlotArmour
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_GetSlotArmour(char, slot)
```

## _GetWornClothingLocation
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_GetWornClothingLocation()
```

## _SetGarmentAlias
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_SetGarmentAlias(object)
```

## _SetMultistate
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_SetMultistate(object)
```

## _SetVerbList
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_SetVerbList(garment, list, s)
```

## _SetVerbsForGarment
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
_SetVerbsForGarment(garment)
```

## AddExternalStylesheet
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
AddExternalStylesheet(stylesheet)
```

## AddStatusAttributesForElement
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
AddStatusAttributesForElement(status, element)
```

## AddToResolvedNames
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
AddToResolvedNames(var, result)
```

## BlockingMessage
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
BlockingMessage(blocked, prefix)
```

## CheckLimits
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
CheckLimits(object, prefix)
```

## ClearMenu
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ClearMenu()
```

## ClearPageDialogueState
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ClearPageDialogueState()
```

## CloseObject
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
CloseObject(object, ismultiple)
```

## CommandLink
```
CommandLink (string command, string link text)
```

Returns a [string](/types#string) containing the XML required to display a hyperlink. When the hyperlink is clicked, the specified player command will be run. For example:

     msg (CommandLink("undo", "Click here to undo the previous turn"))

outputs a link titled "Click here to undo the previous turn" - when clicked, the "undo" player command is run.

See also: [ObjectLink](#objectlink)

## CompareNames
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
CompareNames(name, value, obj, fullmatches, partialmatches)
```

## ContainsAccessible
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ContainsAccessible (object parent, object child, boolean reachable)
```

Returns a [boolean](/types#boolean).

## ContainsReachable
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ContainsReachable (object parent, object child)
```

Returns a [boolean](/types#boolean).

## ContainsVisible
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ContainsVisible (object parent, object child)
```

Returns a [boolean](/types#boolean).

## CreateGiveMenuList
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
CreateGiveMenuList(object)
```

## CreateUseMenuList
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
CreateUseMenuList(object)
```

## DisableMenu
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
DisableMenu()
```

## DisableMenuOutputSection
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
DisableMenuOutputSection(section)
```

## DisplayHttpLink
```
DisplayHttpLink(string displaylink,string url, boolean https)
```

Displays a weblink in the gamewindow. Clicking on **displaylink** will open the **url** in an external webbrowser. You can specify to use HTTPS or not.

## DoAskTell
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
DoAskTell(object, text, property, defaultscript, defaulttemplate)
```

## DoDrop
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
DoDrop(object, ismultiple)
```

## DoTake
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
DoTake(object, ismultiple)
```

## EndOutputSection
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
EndOutputSection(name)
```

## EscapeQuotes
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
EscapeQuotes(s)
```

## FormatStatusAttribute
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
FormatStatusAttribute(attr, value, format)
```

## GenerateMenuChoices
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GenerateMenuChoices(dictionary, objects)
```

## GetCommandBarFormat
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetCommandBarFormat()
```

## GetCoordinateOwner
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetCoordinateOwner(playerobject)
```

## GetCurrentLinkTextFormat
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetCurrentLinkTextFormat()
```

## GetCurrentTextFormat
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetCurrentTextFormat(colour)
```

## GetDefaultPrefix
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetDefaultPrefix(object)
```

Should be defined by the language library (but is only used within the language library, so it is safe to not define this if it is not used). When [usedefaultprefix](/attributes#usedefaultprefix) is in use for an object, this should generate the relevant default.

## GetDescriptor
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetDescriptor(object)
```

## GetKeywordsMatchStrength
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetKeywordsMatchStrength(keywords, input)
```

## GetLinkTextColour
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetLinkTextColour()
```

## GetPlacesObjectsList
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetPlacesObjectsList()
```

Required by Quest so it can display the "Places and Objects" list. Returns [ScopeVisibleNotHeldNotScenery](/functions/scope#scopevisiblenotheldnotscenery) with the player object excluded.

## GetScope
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetScope(variable, value, objtype)
```

## GetScoping
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetScoping(scopestring, variable)
```

## GetTaggedName
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
GetTaggedName (object, string type, stringlist verbs)
```

Returns a [string](/types#string).

**This function was removed in Quest 5.4**

## GetUniqueElementName
```
GetUniqueElementName (string element name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [string](/types#string) containing the specified name - if necessary with a number added to ensure it is an unused element name.

## Grid_AddNewShapePoint
```
Grid_AddNewShapePoint (int x, int y)
```

On the custom grid drawing layer, adds a new point to a shape. To draw a custom shape, call this function for each point on the shape, then call [Grid\_DrawShape](#grid_drawshape) to complete the drawing.

## Grid_CalculateMapCoordinates

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_CalculateMapCoordinates(room)
```

## Grid_ClearCustomLayer
```
Grid_ClearCustomLayer ()
```

Clears the custom grid drawing layer.

## Grid_DrawArrow
```
Grid_DrawArrow (string id, int x1, int y1, int x2, int y2, string border, int borderwidth)
```

On the custom grid drawing layer, draws an arrow of the specified border colour and thickness from (x1,y1) to (x2,y2). The id is arbitrary - if reused, the existing arrow with the same id will be removed.

## Grid_DrawGridLines
```
Grid_DrawGridLines (int x1, int y1, int x2, int y2, string border)
```

On the custom grid drawing layer, draws a grid with line of the specified border from (x1,y1) to (x2,y2).

## Grid_DrawImage
```
Grid_DrawImage (string id, string url, int x, int y, int width, int height)
```

On the custom grid drawing layer, draws the image from the specified URL. The id is arbitrary - if you re-use the same id, the existing image will be removed.

## Grid_DrawLine
```
Grid_DrawLine (int x1, int y1, int x2, int y2, string border, int borderwidth)
```

On the custom grid drawing layer, draws a line of the specified border colour and thickness from (x1,y1) to (x2,y2).

## Grid_DrawPlayerInRoom

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_DrawPlayerInRoom(room)
```

## Grid_DrawRoom
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_DrawRoom(room, redraw)
```

## Grid_DrawShape
```
Grid_DrawShape (string id, string border, string fill, double opacity)
```

On the custom grid drawing layer, draws an arbitrary shape. First, specify all points using [Grid\_AddNewShapePoint](#grid_addnewshapepoint). Then call this function to place the drawing on the grid, with the specified border and fill colour and opacity between 0 and 1. The id is arbitrary - if reused, an existing shape will be replaced with this one.

## Grid_DrawSquare
```
Grid_DrawSquare (string id, int x, int y, int width, int height, string text, string fill)
```

On the custom grid drawing layer, draws a square at (x,y) with the specified width and height. A fill colour and some text to display in the centre of the square can also be specified.

## Grid_DrawSvg
```
Grid_DrawSvg (string instance id, string symbol id, int x, int y, int width, int height)
```

On the custom grid drawing layer, draws the specified SVG file (the symbol id must have been previously loaded using [Grid\_LoadSvg](#grid_loadsvg). The instance id is arbitrary - if you re-use the same instance id, the existing symbol will be removed.

## Grid_GetGridCoordinateForPlayer
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_GetGridCoordinateForPlayer(playerobject, room, coordinate)
```

## Grid_GetPlayerCoordinateDictionary
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_GetPlayerCoordinateDictionary(playerobject)
```

## Grid_GetPlayerCoordinatesForRoom
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_GetPlayerCoordinatesForRoom(playerobject, room)
```

## Grid_GetRoomBooleanForPlayer
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_GetRoomBooleanForPlayer(playerobject, room, attribute)
```

## Grid_LoadSvg
```
Grid_LoadSvg (string data, string id)
```

Loads and SVG file and associates it with an id, so it can subsequently be drawn on the custom grid drawing layer using [Grid\_DrawSvg](#grid_drawsvg).

The data parameter is the raw file data for the SVG file - you can load a string with file data using the [GetFileData](/functions/general#getfiledata) function.

## Grid_Redraw

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_Redraw
```

## Grid_SetCentre
```
Grid_SetCentre (int x, int y)
```

Centres the grid on the specified co-ordinates.

## Grid_SetGridCoordinateForPlayer
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_SetGridCoordinateForPlayer(playerobject, room, coordinate, value)
```

## Grid_SetRoomBooleanForPlayer
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_SetRoomBooleanForPlayer(playerobject, room, coordinate, value)
```

## Grid_SetScale

<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Grid_SetScale(scale)
```

## Grid_ShowCustomLayer
```
Grid_ShowCustomLayer (boolean visible)
```

Turn the custom grid drawing layer on or off.

## HandleCommand
```
HandleCommand (command)
```

Parses the specified command.

## HandleGiveTo
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HandleGiveTo(object1, object2)
```

## HandleMenuTextResponse
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HandleMenuTextResponse(input)
```

## HandleMultiVerb
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HandleMultiVerb(object, property, object2, default)
```

## HandleNextCommandQueueItem
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HandleNextCommandQueueItem()
```

## HandlePageTextResponse
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HandlePageTextResponse(input)
```

## HandleSingleCommand
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HandleSingleCommand(command)
```

## HandleSingleCommandPattern
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HandleSingleCommandPattern(command, thiscommand, varlist)
```

## HandleUseOn
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HandleUseOn(object1, object2)
```

## HideOutputSection
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HideOutputSection(name)
```

## HidePreviousTurnOutput
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
HidePreviousTurnOutput()
```

## InitConjugation
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
InitConjugation()
```

## InitInterface
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
InitInterface
```

Does not return a value.

Quest will look for a function called InitInterface in any ASLX file, and if one exists then it will be called when the game begins, and also when loading a saved game.

Core.aslx defines an implementation of a InitInterface function. It does the following:

-   sets up the default game fonts and colours
-   sets up compass direction names
-   sets titles of panes ("Inventory", "Places and Objects" etc.)
-   shows or hides panes depending on the [showpanes](/attributes#showpanes) option

## InitPOV
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
InitPOV(oldPOV, newPOV)
```

## InitStatusAttributes
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
InitStatusAttributes()
```

## InitVerbsList
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
InitVerbsList
```

## IsGameRunning
```
IsGameRunning ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) indicating whether the game is currently running (i.e. false when the game has finished).

## JS_GridSquareClick
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
JS_GridSquareClick(parameterstring)
```

## JSSafe
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
JSSafe(s)
```

## ListObjectContents
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ListObjectContents(object)
```

Lists the contents of the specified object, only if [isopen](/attributes#isopen) and [listchildren](/attributes#listchildren) are set.

## MapPOVCoordinate
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
MapPOVCoordinate(sourcedata, targetdata, name, offset)
```

## MapPOVCoordinates
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
MapPOVCoordinates(source, target)
```

## MergePOVCoordinates
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
MergePOVCoordinates()
```

## ObjectForTextProcessor
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ObjectForTextProcessor(objectname)
```

## ObjectLink
```
ObjectLink (object)
```

Returns a [string](/types#string) containing the XML required to display a hyperlink for the specified object, with its display verbs.

See also: [CommandLink](#commandlink)

## OnEnterRoom
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
OnEnterRoom ()
```

Does not return a value.

## OpenObject
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
OpenObject(object)
```

## P
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
P(s)
```

## ParamsForTextProcessor
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ParamsForTextProcessor()
```

## Populate
```
Populate (string regex, string input)
```

There is also an optional cache ID parameter:

```
Populate (string regex, string input, string cache ID)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

The input must be a match for the regular expression, or an error occurs.

Returns a [stringdictionary](/types#stringdictionary), keyed by the group names in the regular expression, with values set to the resolved regex groups.

Use a cache ID for improved performance if you repeatedly test strings against the same regular expression. The compiled regular expression will be cached and used again for subsequent calls to Populate (or [GetMatchStrength](/functions/string#getmatchstrength) or [IsRegexMatch](/functions/string#isregexmatch) ) using the same cache ID.

For example, given this regex which matches the text "put (object name) on (object name)":

    put (<object1>.*) on (<object2>.*)

Passing this to the Populate function with an input "put book on shelf" will return a [stringdictionary](/types#stringdictionary) where object1="book" and object2="shelf".

See also [GetMatchStrength](/functions/string#getmatchstrength), [IsRegexMatch](/functions/string#isregexmatch)

## ProcessTextCommand
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand(section, data)
```

## ProcessTextCommand_Colour
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Colour(section, data)
```

## ProcessTextCommand_Command
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Command(section, data)
```

## ProcessTextCommand_Counter
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Counter(section, data)
```

## ProcessTextCommand_Either
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Either(section, data)
```

## ProcessTextCommand_Eval
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Eval(section, data)
```

## ProcessTextCommand_Exit
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Exit(section, data)
```

## ProcessTextCommand_Format
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Format(section, data)
```

## ProcessTextCommand_GetNextLinkId
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_GetNextLinkId()
```

## ProcessTextCommand_Here
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Here(section, data)
```

## ProcessTextCommand_If
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_If(section, data)
```

## ProcessTextCommand_Img
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Img(section, data)
```

## ProcessTextCommand_NotFirst
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_NotFirst(section, data)
```

## ProcessTextCommand_Object
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Object(section, data)
```

## ProcessTextCommand_Once
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Once(section, data)
```

## ProcessTextCommand_Page
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Page(section, data)
```

## ProcessTextCommand_Popup
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Popup(section, data)
```

## ProcessTextCommand_Random
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Random(section, data)
```

## ProcessTextCommand_RandomAlias
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_RandomAlias(section, data)
```

## ProcessTextCommand_Select
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextCommand_Select(section, data)
```

## ProcessTextSection
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ProcessTextSection(text, data)
```

## RequestSave
<b>Internal function to Core.aslx</b> - deprecated, and games should not normally need to call internal functions.

```
RequestSave()
```

## ResetCommandBarFormat
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResetCommandBarFormat()
```

## ResolveName
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResolveName(value, objtype)
```

## ResolveNameFromList
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResolveNameFromList(variable, value, objtype, scope, secondaryscope)
```

## ResolveNameInternal
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResolveNameInternal(value, objtype)
```

## ResolveNameList
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResolveNameList(value, scope, objtype, resultdictionary)
```

## ResolveNameListItem
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResolveNameListItem(value, scope, objtype, resultdictionary)
```

## ResolveNameListItemFinished
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResolveNameListItemFinished(result)
```

## ResolveNextName
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResolveNextName()
```

## ResolveNextNameListItem
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ResolveNextNameListItem()
```

## RunTurnScripts
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
RunTurnScripts()
```

## SecondaryScopeReachableForRoom
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
SecondaryScopeReachableForRoom(room)
```

## SetLinkForegroundColour
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
SetLinkForegroundColour(colour)
```

## ShowMenuResponse
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
ShowMenuResponse(option)
```

## StartGame
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
StartGame
```

Does not return a value.

Quest will look for a function called StartGame, and if one exists then it will be called when the game begins, except if the game is being loaded from a .quest-save file.

Core.aslx defines an implementation of a StartGame function. It does the following:

-   updates status attributes
-   if the [game](/elements#game) object has a "start" script attribute, runs that
-   displays the initial room description

## StartNewOutputSection
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
StartNewOutputSection()
```

## StartTurnOutputSection
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
StartTurnOutputSection()
```

## TestDropGlobal
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
TestDropGlobal(object, prefix)
```

## TestExitGlobal
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
TestExitGlobal(exit)
```

## TestTakeGlobal
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
TestTakeGlobal(object, prefix)
```

## TextFX_Typewriter_Internal
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
TextFX_Typewriter_Internal(text, speed, font, color, size)
```

## TextFX_Unscramble_Internal
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
TextFX_Unscramble_Internal(text, speed, reveal, font, color, size)
```

## TryOpenClose
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
TryOpenClose (boolean open, object)
```

Does not return a value.
## Tsplit
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
Tsplit(splittext)
```

## UIOptionUseGameColours
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
UIOptionUseGameColours()
```

## UIOptionUseGameFont
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
UIOptionUseGameFont()
```

## UnescapeQuotes
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
UnescapeQuotes(s)
```

## UnresolvedCommand
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
UnresolvedCommand(objectname, varname)
```

## UpdateObjectLinks
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
UpdateObjectLinks()
```

## UpdateTranscriptString
<b>Internal function to Core.aslx</b> - games should not normally need to call internal functions.

```
UpdateTranscriptString(data)
```

