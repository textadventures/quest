---
title: "Internal Core.aslx functions"
sidebar:
  order: 14
---

Most games shouldn't need to call these directly.

## _DoRemove
```
_DoRemove(object)
```

## _DoWear
```
_DoWear(object)
```

## _GetList
```
_GetList(char, slot)
```

## _GetOuterForGarment
```
_GetOuterForGarment(char, garment)
```

## _GetSlotArmour
```
_GetSlotArmour(char, slot)
```

## _GetWornClothingLocation
```
_GetWornClothingLocation()
```

## _SetGarmentAlias
```
_SetGarmentAlias(object)
```

## _SetMultistate
```
_SetMultistate(object)
```

## _SetVerbList
```
_SetVerbList(garment, list, s)
```

## _SetVerbsForGarment
```
_SetVerbsForGarment(garment)
```

## AddExternalStylesheet
```
AddExternalStylesheet(stylesheet)
```

## AddStatusAttributesForElement
```
AddStatusAttributesForElement(status, element)
```

## AddToResolvedNames
```
AddToResolvedNames(var, result)
```

## BlockingMessage
```
BlockingMessage(blocked, prefix)
```

## CheckLimits
```
CheckLimits(object, prefix)
```

## ClearMenu
```
ClearMenu()
```

## ClearPageDialogueState
```
ClearPageDialogueState()
```

## CloseObject
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
```
CompareNames(name, value, obj, fullmatches, partialmatches)
```

## ContainsAccessible
```
ContainsAccessible (object parent, object child, boolean reachable)
```

Returns a [boolean](/types#boolean).

## ContainsReachable
```
ContainsReachable (object parent, object child)
```

Returns a [boolean](/types#boolean).

## ContainsVisible
```
ContainsVisible (object parent, object child)
```

Returns a [boolean](/types#boolean).

## CreateGiveMenuList
```
CreateGiveMenuList(object)
```

## CreateUseMenuList
```
CreateUseMenuList(object)
```

## DisableMenu
```
DisableMenu()
```

## DisableMenuOutputSection
```
DisableMenuOutputSection(section)
```

## DisplayHttpLink
```
DisplayHttpLink(string displaylink,string url, boolean https)
```

Displays a weblink in the gamewindow. Clicking on **displaylink** will open the **url** in an external webbrowser. You can specify to use HTTPS or not.

## DoAskTell
```
DoAskTell(object, text, property, defaultscript, defaulttemplate)
```

## DoDrop
```
DoDrop(object, ismultiple)
```

## DoTake
```
DoTake(object, ismultiple)
```

## EndOutputSection
```
EndOutputSection(name)
```

## EscapeQuotes
```
EscapeQuotes(s)
```

## FormatStatusAttribute
```
FormatStatusAttribute(attr, value, format)
```

## GenerateMenuChoices
```
GenerateMenuChoices(dictionary, objects)
```

## GetCommandBarFormat
```
GetCommandBarFormat()
```

## GetCoordinateOwner
```
GetCoordinateOwner(playerobject)
```

## GetCurrentLinkTextFormat
```
GetCurrentLinkTextFormat()
```

## GetCurrentTextFormat
```
GetCurrentTextFormat(colour)
```

## GetDefaultPrefix
```
GetDefaultPrefix(object)
```

Should be defined by the language library (but is only used within the language library, so it is safe to not define this if it is not used). When [usedefaultprefix](/attributes#usedefaultprefix) is in use for an object, this should generate the relevant default.

## GetDescriptor
```
GetDescriptor(object)
```

## GetKeywordsMatchStrength
```
GetKeywordsMatchStrength(keywords, input)
```

## GetLinkTextColour
```
GetLinkTextColour()
```

## GetPlacesObjectsList
```
GetPlacesObjectsList()
```

Required by Quest so it can display the "Places and Objects" list. Returns [ScopeVisibleNotHeldNotScenery](/functions/scope#scopevisiblenotheldnotscenery) with the player object excluded.

## GetScope
```
GetScope(variable, value, objtype)
```

## GetScoping
```
GetScoping(scopestring, variable)
```

## GetTaggedName
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

```
Grid_DrawPlayerInRoom(room)
```

## Grid_DrawRoom
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
```
Grid_GetGridCoordinateForPlayer(playerobject, room, coordinate)
```

## Grid_GetPlayerCoordinateDictionary
```
Grid_GetPlayerCoordinateDictionary(playerobject)
```

## Grid_GetPlayerCoordinatesForRoom
```
Grid_GetPlayerCoordinatesForRoom(playerobject, room)
```

## Grid_GetRoomBooleanForPlayer
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

```
Grid_Redraw
```

## Grid_SetCentre
```
Grid_SetCentre (int x, int y)
```

Centres the grid on the specified co-ordinates.

## Grid_SetGridCoordinateForPlayer
```
Grid_SetGridCoordinateForPlayer(playerobject, room, coordinate, value)
```

## Grid_SetRoomBooleanForPlayer
```
Grid_SetRoomBooleanForPlayer(playerobject, room, coordinate, value)
```

## Grid_SetScale

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
```
HandleGiveTo(object1, object2)
```

## HandleMenuTextResponse
```
HandleMenuTextResponse(input)
```

## HandleMultiVerb
```
HandleMultiVerb(object, property, object2, default)
```

## HandleNextCommandQueueItem
```
HandleNextCommandQueueItem()
```

## HandlePageTextResponse
```
HandlePageTextResponse(input)
```

## HandleSingleCommand
```
HandleSingleCommand(command)
```

## HandleSingleCommandPattern
```
HandleSingleCommandPattern(command, thiscommand, varlist)
```

## HandleUseOn
```
HandleUseOn(object1, object2)
```

## HideOutputSection
```
HideOutputSection(name)
```

## HidePreviousTurnOutput
```
HidePreviousTurnOutput()
```

## InitConjugation
```
InitConjugation()
```

## InitInterface
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
```
InitPOV(oldPOV, newPOV)
```

## InitStatusAttributes
```
InitStatusAttributes()
```

## InitVerbsList
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
```
JS_GridSquareClick(parameterstring)
```

## JSSafe
```
JSSafe(s)
```

## ListObjectContents
```
ListObjectContents(object)
```

Lists the contents of the specified object, only if [isopen](/attributes#isopen) and [listchildren](/attributes#listchildren) are set.

## MapPOVCoordinate
```
MapPOVCoordinate(sourcedata, targetdata, name, offset)
```

## MapPOVCoordinates
```
MapPOVCoordinates(source, target)
```

## MergePOVCoordinates
```
MergePOVCoordinates()
```

## ObjectForTextProcessor
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
```
OnEnterRoom ()
```

Does not return a value.

## OpenObject
```
OpenObject(object)
```

## P
```
P(s)
```

## ParamsForTextProcessor
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
```
ProcessTextCommand(section, data)
```

## ProcessTextCommand_Colour
```
ProcessTextCommand_Colour(section, data)
```

## ProcessTextCommand_Command
```
ProcessTextCommand_Command(section, data)
```

## ProcessTextCommand_Counter
```
ProcessTextCommand_Counter(section, data)
```

## ProcessTextCommand_Either
```
ProcessTextCommand_Either(section, data)
```

## ProcessTextCommand_Eval
```
ProcessTextCommand_Eval(section, data)
```

## ProcessTextCommand_Exit
```
ProcessTextCommand_Exit(section, data)
```

## ProcessTextCommand_Format
```
ProcessTextCommand_Format(section, data)
```

## ProcessTextCommand_GetNextLinkId
```
ProcessTextCommand_GetNextLinkId()
```

## ProcessTextCommand_Here
```
ProcessTextCommand_Here(section, data)
```

## ProcessTextCommand_If
```
ProcessTextCommand_If(section, data)
```

## ProcessTextCommand_Img
```
ProcessTextCommand_Img(section, data)
```

## ProcessTextCommand_NotFirst
```
ProcessTextCommand_NotFirst(section, data)
```

## ProcessTextCommand_Object
```
ProcessTextCommand_Object(section, data)
```

## ProcessTextCommand_Once
```
ProcessTextCommand_Once(section, data)
```

## ProcessTextCommand_Page
```
ProcessTextCommand_Page(section, data)
```

## ProcessTextCommand_Popup
```
ProcessTextCommand_Popup(section, data)
```

## ProcessTextCommand_Random
```
ProcessTextCommand_Random(section, data)
```

## ProcessTextCommand_RandomAlias
```
ProcessTextCommand_RandomAlias(section, data)
```

## ProcessTextCommand_Select
```
ProcessTextCommand_Select(section, data)
```

## ProcessTextSection
```
ProcessTextSection(text, data)
```

## RequestSave
```
RequestSave()
```

Deprecated.

## ResetCommandBarFormat
```
ResetCommandBarFormat()
```

## ResolveName
```
ResolveName(value, objtype)
```

## ResolveNameFromList
```
ResolveNameFromList(variable, value, objtype, scope, secondaryscope)
```

## ResolveNameInternal
```
ResolveNameInternal(value, objtype)
```

## ResolveNameList
```
ResolveNameList(value, scope, objtype, resultdictionary)
```

## ResolveNameListItem
```
ResolveNameListItem(value, scope, objtype, resultdictionary)
```

## ResolveNameListItemFinished
```
ResolveNameListItemFinished(result)
```

## ResolveNextName
```
ResolveNextName()
```

## ResolveNextNameListItem
```
ResolveNextNameListItem()
```

## RunTurnScripts
```
RunTurnScripts()
```

## SecondaryScopeReachableForRoom
```
SecondaryScopeReachableForRoom(room)
```

## SetLinkForegroundColour
```
SetLinkForegroundColour(colour)
```

## ShowMenuResponse
```
ShowMenuResponse(option)
```

## StartGame
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
```
StartNewOutputSection()
```

## StartTurnOutputSection
```
StartTurnOutputSection()
```

## TestDropGlobal
```
TestDropGlobal(object, prefix)
```

## TestExitGlobal
```
TestExitGlobal(exit)
```

## TestTakeGlobal
```
TestTakeGlobal(object, prefix)
```

## TextFX_Typewriter_Internal
```
TextFX_Typewriter_Internal(text, speed, font, color, size)
```

## TextFX_Unscramble_Internal
```
TextFX_Unscramble_Internal(text, speed, reveal, font, color, size)
```

## TryOpenClose
```
TryOpenClose (boolean open, object)
```

Does not return a value.
## Tsplit
```
Tsplit(splittext)
```

## UIOptionUseGameColours
```
UIOptionUseGameColours()
```

## UIOptionUseGameFont
```
UIOptionUseGameFont()
```

## UnescapeQuotes
```
UnescapeQuotes(s)
```

## UnresolvedCommand
```
UnresolvedCommand(objectname, varname)
```

## UpdateObjectLinks
```
UpdateObjectLinks()
```

## UpdateTranscriptString
```
UpdateTranscriptString(data)
```

