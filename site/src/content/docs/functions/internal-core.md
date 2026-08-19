---
title: "Internal Core.aslx functions"
sidebar:
  order: 14
---

Most games shouldn't need to call these directly.

## _DoRemove
```quest
_DoRemove(object)
```

## _DoWear
```quest
_DoWear(object)
```

## _GetList
```quest
_GetList(char, slot)
```

## _GetOuterForGarment
```quest
_GetOuterForGarment(char, garment)
```

## _GetSlotArmour
```quest
_GetSlotArmour(char, slot)
```

## _GetWornClothingLocation
```quest
_GetWornClothingLocation()
```

## _SetGarmentAlias
```quest
_SetGarmentAlias(object)
```

## _SetMultistate
```quest
_SetMultistate(object)
```

## _SetVerbList
```quest
_SetVerbList(garment, list, s)
```

## _SetVerbsForGarment
```quest
_SetVerbsForGarment(garment)
```

## AddExternalStylesheet
```quest
AddExternalStylesheet(stylesheet)
```

## AddStatusAttributesForElement
```quest
AddStatusAttributesForElement(status, element)
```

## AddToResolvedNames
```quest
AddToResolvedNames(var, result)
```

## BlockingMessage
```quest
BlockingMessage(blocked, prefix)
```

## CheckLimits
```quest
CheckLimits(object, prefix)
```

## ClearMenu
```quest
ClearMenu()
```

## ClearPageDialogueState
```quest
ClearPageDialogueState()
```

## CloseObject
```quest
CloseObject(object, ismultiple)
```

## CommandLink
```quest
CommandLink (string command, string link text)
```

Returns a [string](/types#string) containing the XML required to display a hyperlink. When the hyperlink is clicked, the specified player command will be run. For example:

```quest
 msg (CommandLink("undo", "Click here to undo the previous turn"))
```

outputs a link titled "Click here to undo the previous turn" - when clicked, the "undo" player command is run.

See also: [ObjectLink](#objectlink)

## CompareNames
```quest
CompareNames(name, value, obj, fullmatches, partialmatches)
```

## ContainsAccessible
```quest
ContainsAccessible (object parent, object child, boolean reachable)
```

Returns a [boolean](/types#boolean).

## ContainsReachable
```quest
ContainsReachable (object parent, object child)
```

Returns a [boolean](/types#boolean).

## ContainsVisible
```quest
ContainsVisible (object parent, object child)
```

Returns a [boolean](/types#boolean).

## CreateGiveMenuList
```quest
CreateGiveMenuList(object)
```

## CreateUseMenuList
```quest
CreateUseMenuList(object)
```

## DisableMenu
```quest
DisableMenu()
```

## DisableMenuOutputSection
```quest
DisableMenuOutputSection(section)
```

## DisplayHttpLink
```quest
DisplayHttpLink(string displaylink,string url, boolean https)
```

Displays a weblink in the gamewindow. Clicking on **displaylink** will open the **url** in an external webbrowser. You can specify to use HTTPS or not.

## DoAskTell
```quest
DoAskTell(object, text, property, defaultscript, defaulttemplate)
```

## DoDrop
```quest
DoDrop(object, ismultiple)
```

## DoTake
```quest
DoTake(object, ismultiple)
```

## EndOutputSection
```quest
EndOutputSection(name)
```

## EscapeQuotes
```quest
EscapeQuotes(s)
```

## FormatStatusAttribute
```quest
FormatStatusAttribute(attr, value, format)
```

## GenerateMenuChoices
```quest
GenerateMenuChoices(dictionary, objects)
```

## GetCommandBarFormat
```quest
GetCommandBarFormat()
```

## GetCoordinateOwner
```quest
GetCoordinateOwner(playerobject)
```

## GetCurrentLinkTextFormat
```quest
GetCurrentLinkTextFormat()
```

## GetCurrentTextFormat
```quest
GetCurrentTextFormat(colour)
```

## GetDefaultPrefix
```quest
GetDefaultPrefix(object)
```

Should be defined by the language library (but is only used within the language library, so it is safe to not define this if it is not used). When [usedefaultprefix](/attributes#usedefaultprefix) is in use for an object, this should generate the relevant default.

## GetDescriptor
```quest
GetDescriptor(object)
```

## GetKeywordsMatchStrength
```quest
GetKeywordsMatchStrength(keywords, input)
```

## GetLinkTextColour
```quest
GetLinkTextColour()
```

## GetPlacesObjectsList
```quest
GetPlacesObjectsList()
```

Required by Quest so it can display the "Places and Objects" list. Returns [ScopeVisibleNotHeldNotScenery](/functions/scope#scopevisiblenotheldnotscenery) with the player object excluded.

## GetScope
```quest
GetScope(variable, value, objtype)
```

## GetScoping
```quest
GetScoping(scopestring, variable)
```

## GetTaggedName
```quest
GetTaggedName (object, string type, stringlist verbs)
```

Returns a [string](/types#string).

**This function was removed in Quest 5.4**

## GetUniqueElementName
```quest
GetUniqueElementName (string element name)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [string](/types#string) containing the specified name - if necessary with a number added to ensure it is an unused element name.

## Grid_AddNewShapePoint
```quest
Grid_AddNewShapePoint (int x, int y)
```

On the custom grid drawing layer, adds a new point to a shape. To draw a custom shape, call this function for each point on the shape, then call [Grid\_DrawShape](#grid_drawshape) to complete the drawing.

## Grid_CalculateMapCoordinates

```quest
Grid_CalculateMapCoordinates(room)
```

## Grid_ClearCustomLayer
```quest
Grid_ClearCustomLayer ()
```

Clears the custom grid drawing layer.

## Grid_DrawArrow
```quest
Grid_DrawArrow (string id, int x1, int y1, int x2, int y2, string border, int borderwidth)
```

On the custom grid drawing layer, draws an arrow of the specified border colour and thickness from (x1,y1) to (x2,y2). The id is arbitrary - if reused, the existing arrow with the same id will be removed.

## Grid_DrawGridLines
```quest
Grid_DrawGridLines (int x1, int y1, int x2, int y2, string border)
```

On the custom grid drawing layer, draws a grid with line of the specified border from (x1,y1) to (x2,y2).

## Grid_DrawImage
```quest
Grid_DrawImage (string id, string url, int x, int y, int width, int height)
```

On the custom grid drawing layer, draws the image from the specified URL. The id is arbitrary - if you re-use the same id, the existing image will be removed.

## Grid_DrawLine
```quest
Grid_DrawLine (int x1, int y1, int x2, int y2, string border, int borderwidth)
```

On the custom grid drawing layer, draws a line of the specified border colour and thickness from (x1,y1) to (x2,y2).

## Grid_DrawPlayerInRoom

```quest
Grid_DrawPlayerInRoom(room)
```

## Grid_DrawRoom
```quest
Grid_DrawRoom(room, redraw)
```

## Grid_DrawShape
```quest
Grid_DrawShape (string id, string border, string fill, double opacity)
```

On the custom grid drawing layer, draws an arbitrary shape. First, specify all points using [Grid\_AddNewShapePoint](#grid_addnewshapepoint). Then call this function to place the drawing on the grid, with the specified border and fill colour and opacity between 0 and 1. The id is arbitrary - if reused, an existing shape will be replaced with this one.

## Grid_DrawSquare
```quest
Grid_DrawSquare (string id, int x, int y, int width, int height, string text, string fill)
```

On the custom grid drawing layer, draws a square at (x,y) with the specified width and height. A fill colour and some text to display in the centre of the square can also be specified.

## Grid_DrawSvg
```quest
Grid_DrawSvg (string instance id, string symbol id, int x, int y, int width, int height)
```

On the custom grid drawing layer, draws the specified SVG file (the symbol id must have been previously loaded using [Grid\_LoadSvg](#grid_loadsvg). The instance id is arbitrary - if you re-use the same instance id, the existing symbol will be removed.

## Grid_GetGridCoordinateForPlayer
```quest
Grid_GetGridCoordinateForPlayer(playerobject, room, coordinate)
```

## Grid_GetPlayerCoordinateDictionary
```quest
Grid_GetPlayerCoordinateDictionary(playerobject)
```

## Grid_GetPlayerCoordinatesForRoom
```quest
Grid_GetPlayerCoordinatesForRoom(playerobject, room)
```

## Grid_GetRoomBooleanForPlayer
```quest
Grid_GetRoomBooleanForPlayer(playerobject, room, attribute)
```

## Grid_LoadSvg
```quest
Grid_LoadSvg (string data, string id)
```

Loads and SVG file and associates it with an id, so it can subsequently be drawn on the custom grid drawing layer using [Grid\_DrawSvg](#grid_drawsvg).

The data parameter is the raw file data for the SVG file - you can load a string with file data using the [GetFileData](/functions/general#getfiledata) function.

## Grid_Redraw

```quest
Grid_Redraw
```

## Grid_SetCentre
```quest
Grid_SetCentre (int x, int y)
```

Centres the grid on the specified co-ordinates.

## Grid_SetGridCoordinateForPlayer
```quest
Grid_SetGridCoordinateForPlayer(playerobject, room, coordinate, value)
```

## Grid_SetRoomBooleanForPlayer
```quest
Grid_SetRoomBooleanForPlayer(playerobject, room, coordinate, value)
```

## Grid_SetScale

```quest
Grid_SetScale(scale)
```

## Grid_ShowCustomLayer
```quest
Grid_ShowCustomLayer (boolean visible)
```

Turn the custom grid drawing layer on or off.

## HandleCommand
```quest
HandleCommand (command)
```

Parses the specified command.

## HandleGiveTo
```quest
HandleGiveTo(object1, object2)
```

## HandleMenuTextResponse
```quest
HandleMenuTextResponse(input)
```

## HandleMultiVerb
```quest
HandleMultiVerb(object, property, object2, default)
```

## HandleNextCommandQueueItem
```quest
HandleNextCommandQueueItem()
```

## HandlePageTextResponse
```quest
HandlePageTextResponse(input)
```

## HandleSingleCommand
```quest
HandleSingleCommand(command)
```

## HandleSingleCommandPattern
```quest
HandleSingleCommandPattern(command, thiscommand, varlist)
```

## HandleUseOn
```quest
HandleUseOn(object1, object2)
```

## HideOutputSection
```quest
HideOutputSection(name)
```

## HidePreviousTurnOutput
```quest
HidePreviousTurnOutput()
```

## InitConjugation
```quest
InitConjugation()
```

## InitInterface
```quest
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
```quest
InitPOV(oldPOV, newPOV)
```

## InitStatusAttributes
```quest
InitStatusAttributes()
```

## InitVerbsList
```quest
InitVerbsList
```

## IsGameRunning
```quest
IsGameRunning ()
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

Returns a [boolean](/types#boolean) indicating whether the game is currently running (i.e. false when the game has finished).

## JS_GridSquareClick
```quest
JS_GridSquareClick(parameterstring)
```

## JSSafe
```quest
JSSafe(s)
```

## ListObjectContents
```quest
ListObjectContents(object)
```

Lists the contents of the specified object, only if [isopen](/attributes#isopen) and [listchildren](/attributes#listchildren) are set.

## MapPOVCoordinate
```quest
MapPOVCoordinate(sourcedata, targetdata, name, offset)
```

## MapPOVCoordinates
```quest
MapPOVCoordinates(source, target)
```

## MergePOVCoordinates
```quest
MergePOVCoordinates()
```

## ObjectForTextProcessor
```quest
ObjectForTextProcessor(objectname)
```

## ObjectLink
```quest
ObjectLink (object)
```

Returns a [string](/types#string) containing the XML required to display a hyperlink for the specified object, with its display verbs.

See also: [CommandLink](#commandlink)

## OnEnterRoom
```quest
OnEnterRoom ()
```

Does not return a value.

## OpenObject
```quest
OpenObject(object)
```

## P
```quest
P(s)
```

## ParamsForTextProcessor
```quest
ParamsForTextProcessor()
```

## Populate
```quest
Populate (string regex, string input)
```

There is also an optional cache ID parameter:

```quest
Populate (string regex, string input, string cache ID)
```

<a href="/functions/hardcoded" class="qv-badge">hard-coded</a>

The input must be a match for the regular expression, or an error occurs.

Returns a [stringdictionary](/types#stringdictionary), keyed by the group names in the regular expression, with values set to the resolved regex groups.

Use a cache ID for improved performance if you repeatedly test strings against the same regular expression. The compiled regular expression will be cached and used again for subsequent calls to Populate (or [GetMatchStrength](/functions/string#getmatchstrength) or [IsRegexMatch](/functions/string#isregexmatch) ) using the same cache ID.

For example, given this regex which matches the text "put (object name) on (object name)":

```xml
put (<object1>.*) on (<object2>.*)
```

Passing this to the Populate function with an input "put book on shelf" will return a [stringdictionary](/types#stringdictionary) where object1="book" and object2="shelf".

See also [GetMatchStrength](/functions/string#getmatchstrength), [IsRegexMatch](/functions/string#isregexmatch)

## ProcessTextCommand
```quest
ProcessTextCommand(section, data)
```

## ProcessTextCommand_Colour
```quest
ProcessTextCommand_Colour(section, data)
```

## ProcessTextCommand_Command
```quest
ProcessTextCommand_Command(section, data)
```

## ProcessTextCommand_Counter
```quest
ProcessTextCommand_Counter(section, data)
```

## ProcessTextCommand_Either
```quest
ProcessTextCommand_Either(section, data)
```

## ProcessTextCommand_Eval
```quest
ProcessTextCommand_Eval(section, data)
```

## ProcessTextCommand_Exit
```quest
ProcessTextCommand_Exit(section, data)
```

## ProcessTextCommand_Format
```quest
ProcessTextCommand_Format(section, data)
```

## ProcessTextCommand_GetNextLinkId
```quest
ProcessTextCommand_GetNextLinkId()
```

## ProcessTextCommand_Here
```quest
ProcessTextCommand_Here(section, data)
```

## ProcessTextCommand_If
```quest
ProcessTextCommand_If(section, data)
```

## ProcessTextCommand_Img
```quest
ProcessTextCommand_Img(section, data)
```

## ProcessTextCommand_NotFirst
```quest
ProcessTextCommand_NotFirst(section, data)
```

## ProcessTextCommand_Object
```quest
ProcessTextCommand_Object(section, data)
```

## ProcessTextCommand_Once
```quest
ProcessTextCommand_Once(section, data)
```

## ProcessTextCommand_Page
```quest
ProcessTextCommand_Page(section, data)
```

## ProcessTextCommand_Popup
```quest
ProcessTextCommand_Popup(section, data)
```

## ProcessTextCommand_Random
```quest
ProcessTextCommand_Random(section, data)
```

## ProcessTextCommand_RandomAlias
```quest
ProcessTextCommand_RandomAlias(section, data)
```

## ProcessTextCommand_Select
```quest
ProcessTextCommand_Select(section, data)
```

## ProcessTextSection
```quest
ProcessTextSection(text, data)
```

## RequestSave
```quest
RequestSave()
```

Deprecated.

## ResetCommandBarFormat
```quest
ResetCommandBarFormat()
```

## ResolveName
```quest
ResolveName(value, objtype)
```

## ResolveNameFromList
```quest
ResolveNameFromList(variable, value, objtype, scope, secondaryscope)
```

## ResolveNameInternal
```quest
ResolveNameInternal(value, objtype)
```

## ResolveNameList
```quest
ResolveNameList(value, scope, objtype, resultdictionary)
```

## ResolveNameListItem
```quest
ResolveNameListItem(value, scope, objtype, resultdictionary)
```

## ResolveNameListItemFinished
```quest
ResolveNameListItemFinished(result)
```

## ResolveNextName
```quest
ResolveNextName()
```

## ResolveNextNameListItem
```quest
ResolveNextNameListItem()
```

## RunTurnScripts
```quest
RunTurnScripts()
```

## SecondaryScopeReachableForRoom
```quest
SecondaryScopeReachableForRoom(room)
```

## SetLinkForegroundColour
```quest
SetLinkForegroundColour(colour)
```

## ShowMenuResponse
```quest
ShowMenuResponse(option)
```

## StartGame
```quest
StartGame
```

Does not return a value.

Quest will look for a function called StartGame, and if one exists then it will be called when the game begins, except if the game is being loaded from a .quest-save file.

Core.aslx defines an implementation of a StartGame function. It does the following:

-   updates status attributes
-   if the [game](/elements#game) object has a "start" script attribute, runs that
-   displays the initial room description

## StartNewOutputSection
```quest
StartNewOutputSection()
```

## StartTurnOutputSection
```quest
StartTurnOutputSection()
```

## TestDropGlobal
```quest
TestDropGlobal(object, prefix)
```

## TestExitGlobal
```quest
TestExitGlobal(exit)
```

## TestTakeGlobal
```quest
TestTakeGlobal(object, prefix)
```

## TextFX_Typewriter_Internal
```quest
TextFX_Typewriter_Internal(text, speed, font, color, size)
```

## TextFX_Unscramble_Internal
```quest
TextFX_Unscramble_Internal(text, speed, reveal, font, color, size)
```

## TryOpenClose
```quest
TryOpenClose (boolean open, object)
```

Does not return a value.
## Tsplit
```quest
Tsplit(splittext)
```

## UIOptionUseGameColours
```quest
UIOptionUseGameColours()
```

## UIOptionUseGameFont
```quest
UIOptionUseGameFont()
```

## UnescapeQuotes
```quest
UnescapeQuotes(s)
```

## UnresolvedCommand
```quest
UnresolvedCommand(objectname, varname)
```

## UpdateObjectLinks
```quest
UpdateObjectLinks()
```

## UpdateTranscriptString
```quest
UpdateTranscriptString(data)
```

