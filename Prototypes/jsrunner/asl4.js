var quest = window.quest || {};
function Left(input, length) {
    return input.substring(0, length);
}
function Right(input, length) {
    return input.substring(input.length - length);
}
function Mid(input, start, length) {
    if (typeof length === 'undefined') {
        return input.substr(start - 1);
    }
    return input.substr(start - 1, length);
}
function UCase(input) {
    return input.toUpperCase();
}
function LCase(input) {
    return input.toLowerCase();
}
function InStr(input, search) {
    return input.indexOf(search) + 1;
}
function InStrFrom(start, input, search) {
    return input.indexOf(search, start - 1) + 1;
}
function InStrRev(input, search) {
    return input.lastIndexOf(search) + 1;
}
function Split(input, splitChar) {
    return input.split(splitChar);
}
function IsNumeric(input) {
    return !isNaN(parseFloat(input)) && isFinite(input);
}
function Trim(input) {
    return input.trim();
}
function LTrim(input) {
    return input.replace(/^\s+/, "");
}
function Asc(input) {
    return input.charCodeAt(0);
}
function Chr(input) {
    return String.fromCharCode(input);
}
function Len(input) {
    if (input === null || input === undefined)
        return 0;
    return input.length;
}
function UBound(array) {
    return array.length - 1;
}
function Str(input) {
    return input.toString();
}
function Val(input) {
    return parseInt(input);
}
var MenuData = (function () {
    function MenuData(caption, options, allowCancel) {
        this.Caption = caption;
        this.Options = options;
        this.AllowCancel = allowCancel;
    }
    return MenuData;
})();
var Player = (function () {
    function Player(textFormatter) {
        this.TextFormatter = textFormatter;
    }
    Player.prototype.ShowMenu = function (menuData) {
    };
    Player.prototype.DoWait = function () {
    };
    Player.prototype.DoPause = function (ms) {
    };
    Player.prototype.ShowQuestion = function (caption) {
    };
    Player.prototype.PlaySound = function (filename, synchronous, looped) {
    };
    Player.prototype.StopSound = function () {
    };
    Player.prototype.ClearScreen = function () {
    };
    Player.prototype.SetBackground = function (colour) {
    };
    Player.prototype.SetForeground = function (colour) {
        this.TextFormatter.foreground = colour;
    };
    Player.prototype.SetFont = function (fontName) {
        this.TextFormatter.fontFamily = fontName;
    };
    Player.prototype.SetFontSize = function (fontSize) {
        this.TextFormatter.fontSize = fontSize;
    };
    Player.prototype.SetPanelContents = function (html) {
    };
    Player.prototype.SetWindowMenu = function (menuData) {
    };
    Player.prototype.SetPanesVisible = function (data) {
    };
    Player.prototype.ShowPicture = function (filename) {
    };
    Player.prototype.GetURL = function (file) {
    };
    Player.prototype.LocationUpdated = function (location) {
        quest.ui.locationUpdated(location);
    };
    Player.prototype.Speak = function (text) {
    };
    Player.prototype.GetNewGameFile = function (originalFilename, extensions) {
    };
    Player.prototype.RequestSave = function (html) {
    };
    Player.prototype.UpdateGameName = function (name) {
    };
    Player.prototype.Show = function (element) {
        quest.ui.show(element);
    };
    Player.prototype.SetStatusText = function (text) {
    };
    Player.prototype.UpdateInventoryList = function (items) {
        quest.ui.updateList("inventory", items);
    };
    Player.prototype.UpdateObjectsList = function (items) {
        quest.ui.updateList("placesObjects", items);
    };
    Player.prototype.UpdateExitsList = function (items) {
        quest.ui.updateCompass(items);
    };
    return Player;
})();
var ListData = (function () {
    function ListData(text, verbs) {
        this.Text = text;
        this.Verbs = verbs;
        this.ElementId = text;
        this.ElementName = text;
    }
    return ListData;
})();
var ListType;
(function (ListType) {
    ListType[ListType["InventoryList"] = 0] = "InventoryList";
    ListType[ListType["ExitsList"] = 1] = "ExitsList";
    ListType[ListType["ObjectsList"] = 2] = "ObjectsList";
})(ListType || (ListType = {}));
;
var State;
(function (State) {
    State[State["Ready"] = 0] = "Ready";
    State[State["Working"] = 1] = "Working";
    State[State["Waiting"] = 2] = "Waiting";
    State[State["Finished"] = 3] = "Finished";
})(State || (State = {}));
;
var DefineBlock = (function () {
    function DefineBlock() {
        this.StartLine = 0;
        this.EndLine = 0;
    }
    return DefineBlock;
})();
var Context = (function () {
    function Context() {
        this.CallingObjectId = 0;
        this.NumParameters = 0;
        this.FunctionReturnValue = "";
        this.AllowRealNamesInCommand = false;
        this.DontProcessCommand = false;
        this.CancelExec = false;
        this.StackCounter = 0;
    }
    return Context;
})();
var LogType;
(function (LogType) {
    LogType[LogType["Misc"] = 0] = "Misc";
    LogType[LogType["FatalError"] = 1] = "FatalError";
    LogType[LogType["WarningError"] = 2] = "WarningError";
    LogType[LogType["Init"] = 3] = "Init";
    LogType[LogType["LibraryWarningError"] = 4] = "LibraryWarningError";
    LogType[LogType["Warning"] = 5] = "Warning";
    LogType[LogType["UserError"] = 6] = "UserError";
    LogType[LogType["InternalError"] = 7] = "InternalError";
})(LogType || (LogType = {}));
;
var Direction;
(function (Direction) {
    Direction[Direction["None"] = -1] = "None";
    Direction[Direction["Out"] = 0] = "Out";
    Direction[Direction["North"] = 1] = "North";
    Direction[Direction["South"] = 2] = "South";
    Direction[Direction["East"] = 3] = "East";
    Direction[Direction["West"] = 4] = "West";
    Direction[Direction["NorthWest"] = 5] = "NorthWest";
    Direction[Direction["NorthEast"] = 6] = "NorthEast";
    Direction[Direction["SouthWest"] = 7] = "SouthWest";
    Direction[Direction["SouthEast"] = 8] = "SouthEast";
    Direction[Direction["Up"] = 9] = "Up";
    Direction[Direction["Down"] = 10] = "Down";
})(Direction || (Direction = {}));
;
var ItemType = (function () {
    function ItemType() {
        this.Name = "";
        this.Got = false;
    }
    return ItemType;
})();
var Collectable = (function () {
    function Collectable() {
        this.Name = "";
        this.Type = "";
        this.Value = 0;
        this.Display = "";
        this.DisplayWhenZero = false;
    }
    return Collectable;
})();
var PropertyType = (function () {
    function PropertyType() {
        this.PropertyName = "";
        this.PropertyValue = "";
    }
    return PropertyType;
})();
var ActionType = (function () {
    function ActionType() {
        this.ActionName = "";
        this.Script = "";
    }
    return ActionType;
})();
var UseDataType = (function () {
    function UseDataType() {
        this.UseObject = "";
        this.UseScript = "";
    }
    return UseDataType;
})();
var GiveDataType = (function () {
    function GiveDataType() {
        this.GiveObject = "";
        this.GiveScript = "";
    }
    return GiveDataType;
})();
var PropertiesActions = (function () {
    function PropertiesActions() {
        this.Properties = "";
        this.NumberActions = 0;
        this.NumberTypesIncluded = 0;
    }
    return PropertiesActions;
})();
var VariableType = (function () {
    function VariableType() {
        this.VariableName = "";
        this.VariableUBound = 0;
        this.DisplayString = "";
        this.OnChangeScript = "";
        this.NoZeroDisplay = false;
    }
    return VariableType;
})();
var SynonymType = (function () {
    function SynonymType() {
        this.OriginalWord = "";
        this.ConvertTo = "";
    }
    return SynonymType;
})();
var TimerType = (function () {
    function TimerType() {
        this.TimerName = "";
        this.TimerInterval = 0;
        this.TimerActive = false;
        this.TimerAction = "";
        this.TimerTicks = 0;
        this.BypassThisTurn = false;
    }
    return TimerType;
})();
var UserDefinedCommandType = (function () {
    function UserDefinedCommandType() {
        this.CommandText = "";
        this.CommandScript = "";
    }
    return UserDefinedCommandType;
})();
var TextAction = (function () {
    function TextAction() {
        this.Data = "";
    }
    return TextAction;
})();
var TextActionType;
(function (TextActionType) {
    TextActionType[TextActionType["Text"] = 0] = "Text";
    TextActionType[TextActionType["Script"] = 1] = "Script";
    TextActionType[TextActionType["Nothing"] = 2] = "Nothing";
    TextActionType[TextActionType["Default"] = 3] = "Default";
})(TextActionType || (TextActionType = {}));
;
var ScriptText = (function () {
    function ScriptText() {
        this.Text = "";
        this.Script = "";
    }
    return ScriptText;
})();
var PlaceType = (function () {
    function PlaceType() {
        this.PlaceName = "";
        this.Prefix = "";
        this.PlaceAlias = "";
        this.Script = "";
    }
    return PlaceType;
})();
var RoomType = (function () {
    function RoomType() {
        this.RoomName = "";
        this.RoomAlias = "";
        this.NumberCommands = 0;
        this.Description = new TextAction();
        this.Out = new ScriptText();
        this.East = new TextAction();
        this.West = new TextAction();
        this.North = new TextAction();
        this.South = new TextAction();
        this.NorthEast = new TextAction();
        this.NorthWest = new TextAction();
        this.SouthEast = new TextAction();
        this.SouthWest = new TextAction();
        this.Up = new TextAction();
        this.Down = new TextAction();
        this.InDescription = "";
        this.Look = "";
        this.NumberPlaces = 0;
        this.Prefix = "";
        this.Script = "";
        this.NumberUse = 0;
        this.ObjId = 0;
        this.BeforeTurnScript = "";
        this.AfterTurnScript = "";
    }
    return RoomType;
})();
var ObjectType = (function () {
    function ObjectType() {
        this.ObjectName = "";
        this.ObjectAlias = "";
        this.Detail = "";
        this.ContainerRoom = "";
        this.Exists = false;
        this.IsGlobal = false;
        this.Prefix = "";
        this.Suffix = "";
        this.Gender = "";
        this.Article = "";
        this.DefinitionSectionStart = 0;
        this.DefinitionSectionEnd = 0;
        this.Visible = false;
        this.GainScript = "";
        this.LoseScript = "";
        this.NumberProperties = 0;
        this.Speak = new TextAction();
        this.Take = new TextAction();
        this.IsRoom = false;
        this.IsExit = false;
        this.CorresRoom = "";
        this.CorresRoomId = 0;
        this.Loaded = false;
        this.NumberActions = 0;
        this.NumberUseData = 0;
        this.UseAnything = "";
        this.UseOnAnything = "";
        this.Use = "";
        this.NumberGiveData = 0;
        this.GiveAnything = "";
        this.GiveToAnything = "";
        this.DisplayType = "";
        this.NumberTypesIncluded = 0;
        this.NumberAltNames = 0;
        this.AddScript = new TextAction();
        this.RemoveScript = new TextAction();
        this.OpenScript = new TextAction();
        this.CloseScript = new TextAction();
    }
    return ObjectType;
})();
var ChangeType = (function () {
    function ChangeType() {
        this.AppliesTo = "";
        this.Change = "";
    }
    return ChangeType;
})();
var GameChangeDataType = (function () {
    function GameChangeDataType() {
        this.NumberChanges = 0;
    }
    return GameChangeDataType;
})();
var ResourceType = (function () {
    function ResourceType() {
        this.ResourceName = "";
        this.ResourceStart = 0;
        this.ResourceLength = 0;
        this.Extracted = false;
    }
    return ResourceType;
})();
var ExpressionResult = (function () {
    function ExpressionResult() {
        this.Result = "";
        this.Message = "";
    }
    return ExpressionResult;
})();
var GetAvailableDirectionsResult = (function () {
    function GetAvailableDirectionsResult() {
        this.Description = "";
        this.List = "";
    }
    return GetAvailableDirectionsResult;
})();
var PlayerError;
(function (PlayerError) {
    PlayerError[PlayerError["BadCommand"] = 0] = "BadCommand";
    PlayerError[PlayerError["BadGo"] = 1] = "BadGo";
    PlayerError[PlayerError["BadGive"] = 2] = "BadGive";
    PlayerError[PlayerError["BadCharacter"] = 3] = "BadCharacter";
    PlayerError[PlayerError["NoItem"] = 4] = "NoItem";
    PlayerError[PlayerError["ItemUnwanted"] = 5] = "ItemUnwanted";
    PlayerError[PlayerError["BadLook"] = 6] = "BadLook";
    PlayerError[PlayerError["BadThing"] = 7] = "BadThing";
    PlayerError[PlayerError["DefaultLook"] = 8] = "DefaultLook";
    PlayerError[PlayerError["DefaultSpeak"] = 9] = "DefaultSpeak";
    PlayerError[PlayerError["BadItem"] = 10] = "BadItem";
    PlayerError[PlayerError["DefaultTake"] = 11] = "DefaultTake";
    PlayerError[PlayerError["BadUse"] = 12] = "BadUse";
    PlayerError[PlayerError["DefaultUse"] = 13] = "DefaultUse";
    PlayerError[PlayerError["DefaultOut"] = 14] = "DefaultOut";
    PlayerError[PlayerError["BadPlace"] = 15] = "BadPlace";
    PlayerError[PlayerError["BadExamine"] = 16] = "BadExamine";
    PlayerError[PlayerError["DefaultExamine"] = 17] = "DefaultExamine";
    PlayerError[PlayerError["BadTake"] = 18] = "BadTake";
    PlayerError[PlayerError["CantDrop"] = 19] = "CantDrop";
    PlayerError[PlayerError["DefaultDrop"] = 20] = "DefaultDrop";
    PlayerError[PlayerError["BadDrop"] = 21] = "BadDrop";
    PlayerError[PlayerError["BadPronoun"] = 22] = "BadPronoun";
    PlayerError[PlayerError["AlreadyOpen"] = 23] = "AlreadyOpen";
    PlayerError[PlayerError["AlreadyClosed"] = 24] = "AlreadyClosed";
    PlayerError[PlayerError["CantOpen"] = 25] = "CantOpen";
    PlayerError[PlayerError["CantClose"] = 26] = "CantClose";
    PlayerError[PlayerError["DefaultOpen"] = 27] = "DefaultOpen";
    PlayerError[PlayerError["DefaultClose"] = 28] = "DefaultClose";
    PlayerError[PlayerError["BadPut"] = 29] = "BadPut";
    PlayerError[PlayerError["CantPut"] = 30] = "CantPut";
    PlayerError[PlayerError["DefaultPut"] = 31] = "DefaultPut";
    PlayerError[PlayerError["CantRemove"] = 32] = "CantRemove";
    PlayerError[PlayerError["AlreadyPut"] = 33] = "AlreadyPut";
    PlayerError[PlayerError["DefaultRemove"] = 34] = "DefaultRemove";
    PlayerError[PlayerError["Locked"] = 35] = "Locked";
    PlayerError[PlayerError["DefaultWait"] = 36] = "DefaultWait";
    PlayerError[PlayerError["AlreadyTaken"] = 37] = "AlreadyTaken";
})(PlayerError || (PlayerError = {}));
;
var ItType;
(function (ItType) {
    ItType[ItType["Inanimate"] = 0] = "Inanimate";
    ItType[ItType["Male"] = 1] = "Male";
    ItType[ItType["Female"] = 2] = "Female";
})(ItType || (ItType = {}));
;
var SetResult;
(function (SetResult) {
    SetResult[SetResult["Error"] = 0] = "Error";
    SetResult[SetResult["Found"] = 1] = "Found";
    SetResult[SetResult["Unfound"] = 2] = "Unfound";
})(SetResult || (SetResult = {}));
;
var Thing;
(function (Thing) {
    Thing[Thing["Character"] = 0] = "Character";
    Thing[Thing["Object"] = 1] = "Object";
    Thing[Thing["Room"] = 2] = "Room";
})(Thing || (Thing = {}));
;
var ConvertType;
(function (ConvertType) {
    ConvertType[ConvertType["Strings"] = 0] = "Strings";
    ConvertType[ConvertType["Functions"] = 1] = "Functions";
    ConvertType[ConvertType["Numeric"] = 2] = "Numeric";
    ConvertType[ConvertType["Collectables"] = 3] = "Collectables";
})(ConvertType || (ConvertType = {}));
;
var UseType;
(function (UseType) {
    UseType[UseType["UseOnSomething"] = 0] = "UseOnSomething";
    UseType[UseType["UseSomethingOn"] = 1] = "UseSomethingOn";
})(UseType || (UseType = {}));
;
var GiveType;
(function (GiveType) {
    GiveType[GiveType["GiveToSomething"] = 0] = "GiveToSomething";
    GiveType[GiveType["GiveSomethingTo"] = 1] = "GiveSomethingTo";
})(GiveType || (GiveType = {}));
;
var VarType;
(function (VarType) {
    VarType[VarType["String"] = 0] = "String";
    VarType[VarType["Numeric"] = 1] = "Numeric";
})(VarType || (VarType = {}));
;
var StopType;
(function (StopType) {
    StopType[StopType["Win"] = 0] = "Win";
    StopType[StopType["Lose"] = 1] = "Lose";
    StopType[StopType["Null"] = 2] = "Null";
})(StopType || (StopType = {}));
;
var ExpressionSuccess;
(function (ExpressionSuccess) {
    ExpressionSuccess[ExpressionSuccess["OK"] = 0] = "OK";
    ExpressionSuccess[ExpressionSuccess["Fail"] = 1] = "Fail";
})(ExpressionSuccess || (ExpressionSuccess = {}));
;
var InitGameData = (function () {
    function InitGameData() {
    }
    return InitGameData;
})();
var ArrayResult = (function () {
    function ArrayResult() {
        this.Index = 0;
    }
    return ArrayResult;
})();
var PlayerCanAccessObjectResult = (function () {
    function PlayerCanAccessObjectResult() {
        this.CanAccessObject = false;
    }
    return PlayerCanAccessObjectResult;
})();
var AppliesTo;
(function (AppliesTo) {
    AppliesTo[AppliesTo["Object"] = 0] = "Object";
    AppliesTo[AppliesTo["Room"] = 1] = "Room";
})(AppliesTo || (AppliesTo = {}));
;
var LegacyGame = (function () {
    function LegacyGame(filename, originalFilename, data, fileFetcher) {
        this._casKeywords = [];
        //Stores the start and end lines of each 'define' section
        this._numberSections = 0;
        //The name of the game
        this._nullContext = new Context();
        this._numberRooms = 0;
        this._numberNumericVariables = 0;
        this._numberStringVariables = 0;
        this._numberSynonyms = 0;
        this._numberChars = 0;
        this._numberObjs = 0;
        this._numberItems = 0;
        this._numCollectables = 0;
        this._defaultFontSize = 0;
        this._autoIntro = false;
        this._commandOverrideModeOn = false;
        this._outPutOn = false;
        this._gameAslVersion = 0;
        this._choiceNumber = 0;
        this._numberTimers = 0;
        this._numDisplayStrings = 0;
        this._numDisplayNumerics = 0;
        this._gameFullyLoaded = false;
        this._gameChangeData = new GameChangeDataType();
        this._lastIt = 0;
        this._thisTurnIt = 0;
        this._numResources = 0;
        this._resourceOffset = 0;
        this._startCatPos = 0;
        this._useAbbreviations = false;
        this._loadedFromQsg = false;
        this._skipCheckFile = ["bargain.cas", "easymoney.asl", "musicvf1.cas"];
        this._textFormatter = new TextFormatter();
        this._commandLock = new Object();
        this._stateLock = new Object();
        this._state = State.Ready;
        this._waitLock = new Object();
        this._readyForCommand = true;
        this._gameLoading = false;
        this._playerErrorMessageString = [];
        this._listVerbs = {};
        this._player = new Player(this._textFormatter);
        this._gameFinished = false;
        this._gameIsRestoring = false;
        this._useStaticFrameForPictures = false;
        this._fileDataPos = 0;
        this._questionResponse = false;
        this.LoadCASKeywords();
        this._gameLoadMethod = "normal";
        this._filename = filename;
        this._originalFilename = originalFilename;
        this._data = data;
        this._fileFetcher = fileFetcher;
    }
    LegacyGame.prototype.CopyContext = function (ctx) {
        var result = new Context();
        result.CallingObjectId = ctx.CallingObjectId;
        result.NumParameters = ctx.NumParameters;
        result.Parameters = ctx.Parameters;
        result.FunctionReturnValue = ctx.FunctionReturnValue;
        result.AllowRealNamesInCommand = ctx.AllowRealNamesInCommand;
        result.DontProcessCommand = ctx.DontProcessCommand;
        result.CancelExec = ctx.CancelExec;
        result.StackCounter = ctx.StackCounter;
        return result;
    };
    LegacyGame.prototype.RemoveFormatting = function (s) {
        var code;
        var pos = 0;
        var len = 0;
        do {
            pos = InStr(s, "|");
            if (pos != 0) {
                code = Mid(s, pos + 1, 3);
                if (Left(code, 1) == "b") {
                    len = 1;
                }
                else if (Left(code, 2) == "xb") {
                    len = 2;
                }
                else if (Left(code, 1) == "u") {
                    len = 1;
                }
                else if (Left(code, 2) == "xu") {
                    len = 2;
                }
                else if (Left(code, 1) == "i") {
                    len = 1;
                }
                else if (Left(code, 2) == "xi") {
                    len = 2;
                }
                else if (Left(code, 2) == "cr") {
                    len = 2;
                }
                else if (Left(code, 2) == "cb") {
                    len = 2;
                }
                else if (Left(code, 2) == "cl") {
                    len = 2;
                }
                else if (Left(code, 2) == "cy") {
                    len = 2;
                }
                else if (Left(code, 2) == "cg") {
                    len = 2;
                }
                else if (Left(code, 1) == "n") {
                    len = 1;
                }
                else if (Left(code, 2) == "xn") {
                    len = 2;
                }
                else if (Left(code, 1) == "s") {
                    len = 3;
                }
                else if (Left(code, 2) == "jc") {
                    len = 2;
                }
                else if (Left(code, 2) == "jl") {
                    len = 2;
                }
                else if (Left(code, 2) == "jr") {
                    len = 2;
                }
                else if (Left(code, 1) == "w") {
                    len = 1;
                }
                else if (Left(code, 1) == "c") {
                    len = 1;
                }
                if (len == 0) {
                    // unknown code
                    len = 1;
                }
                s = Left(s, pos - 1) + Mid(s, pos + len + 1);
            }
        } while (!(pos == 0));
        return s;
    };
    LegacyGame.prototype.CheckSections = function () {
        var defines = 0;
        var braces = 0;
        var checkLine = "";
        var bracePos = 0;
        var pos = 0;
        var section = "";
        var hasErrors = false;
        var skipBlock = false;
        this._openErrorReport = "";
        hasErrors = false;
        defines = 0;
        braces = 0;
        for (var i = 1; i <= UBound(this._lines); i++) {
            if (!this.BeginsWith(this._lines[i], "#!qdk-note: ")) {
                if (this.BeginsWith(this._lines[i], "define ")) {
                    section = this._lines[i];
                    braces = 0;
                    defines = defines + 1;
                    skipBlock = this.BeginsWith(this._lines[i], "define text") || this.BeginsWith(this._lines[i], "define synonyms");
                }
                else if (Trim(this._lines[i]) == "end define") {
                    defines = defines - 1;
                    if (defines < 0) {
                        this.LogASLError("Extra 'end define' after block '" + section + "'", LogType.FatalError);
                        this._openErrorReport = this._openErrorReport + "Extra 'end define' after block '" + section + "'\n";
                        hasErrors = true;
                        defines = 0;
                    }
                    if (braces > 0) {
                        this.LogASLError("Missing } in block '" + section + "'", LogType.FatalError);
                        this._openErrorReport = this._openErrorReport + "Missing } in block '" + section + "'\n";
                        hasErrors = true;
                    }
                    else if (braces < 0) {
                        this.LogASLError("Too many } in block '" + section + "'", LogType.FatalError);
                        this._openErrorReport = this._openErrorReport + "Too many } in block '" + section + "'\n";
                        hasErrors = true;
                    }
                }
                if (Left(this._lines[i], 1) != "'" && !skipBlock) {
                    checkLine = this.ObliterateParameters(this._lines[i]);
                    if (this.BeginsWith(checkLine, "'<ERROR;")) {
                        // ObliterateParameters denotes a mismatched $, ( etc.
                        // by prefixing line with '<ERROR;*; where * is the mismatched
                        // character
                        this.LogASLError("Expected closing " + Mid(checkLine, 9, 1) + " character in '" + this.ReportErrorLine(this._lines[i]) + "'", LogType.FatalError);
                        this._openErrorReport = this._openErrorReport + "Expected closing " + Mid(checkLine, 9, 1) + " character in '" + this.ReportErrorLine(this._lines[i]) + "'.\n";
                        return false;
                    }
                }
                if (Left(Trim(checkLine), 1) != "'") {
                    // Now check {
                    pos = 1;
                    do {
                        bracePos = InStrFrom(pos, checkLine, "{");
                        if (bracePos != 0) {
                            pos = bracePos + 1;
                            braces = braces + 1;
                        }
                    } while (!(bracePos == 0 || pos > Len(checkLine)));
                    // Now check }
                    pos = 1;
                    do {
                        bracePos = InStrFrom(pos, checkLine, "}");
                        if (bracePos != 0) {
                            pos = bracePos + 1;
                            braces = braces - 1;
                        }
                    } while (!(bracePos == 0 || pos > Len(checkLine)));
                }
            }
        }
        if (defines > 0) {
            this.LogASLError("Missing 'end define'", LogType.FatalError);
            this._openErrorReport = this._openErrorReport + "Missing 'end define'.\n";
            hasErrors = true;
        }
        return !hasErrors;
    };
    LegacyGame.prototype.ConvertFriendlyIfs = function () {
        // Converts
        //   if (%something% < 3) then ...
        // to
        //   if is <%something%;lt;3> then ...
        // and also repeat until ...
        // Returns False if successful
        var convPos = 0;
        var symbPos = 0;
        var symbol;
        var endParamPos = 0;
        var paramData;
        var startParamPos = 0;
        var firstData;
        var secondData;
        var obscureLine;
        var newParam;
        var varObscureLine;
        var bracketCount = 0;
        for (var i = 1; i <= UBound(this._lines); i++) {
            obscureLine = this.ObliterateParameters(this._lines[i]);
            convPos = InStr(obscureLine, "if (");
            if (convPos == 0) {
                convPos = InStr(obscureLine, "until (");
            }
            if (convPos == 0) {
                convPos = InStr(obscureLine, "while (");
            }
            if (convPos == 0) {
                convPos = InStr(obscureLine, "not (");
            }
            if (convPos == 0) {
                convPos = InStr(obscureLine, "and (");
            }
            if (convPos == 0) {
                convPos = InStr(obscureLine, "or (");
            }
            if (convPos != 0) {
                varObscureLine = this.ObliterateVariableNames(this._lines[i]);
                if (this.BeginsWith(varObscureLine, "'<ERROR;")) {
                    // ObliterateVariableNames denotes a mismatched #, % or $
                    // by prefixing line with '<ERROR;*; where * is the mismatched
                    // character
                    this.LogASLError("Expected closing " + Mid(varObscureLine, 9, 1) + " character in '" + this.ReportErrorLine(this._lines[i]) + "'", LogType.FatalError);
                    return true;
                }
                startParamPos = InStrFrom(convPos, this._lines[i], "(");
                endParamPos = 0;
                bracketCount = 1;
                for (var j = startParamPos + 1; j <= Len(this._lines[i]); j++) {
                    if (Mid(this._lines[i], j, 1) == "(") {
                        bracketCount = bracketCount + 1;
                    }
                    else if (Mid(this._lines[i], j, 1) == ")") {
                        bracketCount = bracketCount - 1;
                    }
                    if (bracketCount == 0) {
                        endParamPos = j;
                        break;
                    }
                }
                if (endParamPos == 0) {
                    this.LogASLError("Expected ) in '" + this.ReportErrorLine(this._lines[i]) + "'", LogType.FatalError);
                    return true;
                }
                paramData = Mid(this._lines[i], startParamPos + 1, (endParamPos - startParamPos) - 1);
                symbPos = InStr(paramData, "!=");
                if (symbPos == 0) {
                    symbPos = InStr(paramData, "<>");
                    if (symbPos == 0) {
                        symbPos = InStr(paramData, "<=");
                        if (symbPos == 0) {
                            symbPos = InStr(paramData, ">=");
                            if (symbPos == 0) {
                                symbPos = InStr(paramData, "<");
                                if (symbPos == 0) {
                                    symbPos = InStr(paramData, ">");
                                    if (symbPos == 0) {
                                        symbPos = InStr(paramData, "=");
                                        if (symbPos == 0) {
                                            this.LogASLError("Unrecognised 'if' condition in '" + this.ReportErrorLine(this._lines[i]) + "'", LogType.FatalError);
                                            return true;
                                        }
                                        else {
                                            symbol = "=";
                                        }
                                    }
                                    else {
                                        symbol = ">";
                                    }
                                }
                                else {
                                    symbol = "<";
                                }
                            }
                            else {
                                symbol = ">=";
                            }
                        }
                        else {
                            symbol = "<=";
                        }
                    }
                    else {
                        symbol = "<>";
                    }
                }
                else {
                    symbol = "<>";
                }
                firstData = Trim(Left(paramData, symbPos - 1));
                secondData = Trim(Mid(paramData, symbPos + Len(symbol)));
                if (symbol == "=") {
                    newParam = "is <" + firstData + ";" + secondData + ">";
                }
                else {
                    newParam = "is <" + firstData + ";";
                    if (symbol == "<") {
                        newParam = newParam + "lt";
                    }
                    else if (symbol == ">") {
                        newParam = newParam + "gt";
                    }
                    else if (symbol == ">=") {
                        newParam = newParam + "gt=";
                    }
                    else if (symbol == "<=") {
                        newParam = newParam + "lt=";
                    }
                    else if (symbol == "<>") {
                        newParam = newParam + "!=";
                    }
                    newParam = newParam + ";" + secondData + ">";
                }
                this._lines[i] = Left(this._lines[i], startParamPos - 1) + newParam + Mid(this._lines[i], endParamPos + 1);
                // Repeat processing this line, in case there are
                // further changes to be made.
                i = i - 1;
            }
        }
        return false;
    };
    LegacyGame.prototype.ConvertMultiLineSections = function () {
        var startLine = 0;
        var braceCount = 0;
        var thisLine;
        var lineToAdd;
        var lastBrace = 0;
        var i = 0;
        var restOfLine;
        var procName;
        var endLineNum = 0;
        var afterLastBrace;
        var z;
        var startOfOrig;
        var testLine;
        var testBraceCount = 0;
        var obp = 0;
        var cbp = 0;
        var curProc = 0;
        i = 1;
        do {
            z = this._lines[this._defineBlocks[i].StartLine];
            if (((!this.BeginsWith(z, "define text ")) && (!this.BeginsWith(z, "define menu ")) && z != "define synonyms")) {
                for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                    if (InStr(this._lines[j], "{") > 0) {
                        afterLastBrace = "";
                        thisLine = Trim(this._lines[j]);
                        procName = "<!intproc" + curProc + ">";
                        // see if this brace's corresponding closing
                        // brace is on same line:
                        testLine = Mid(this._lines[j], InStr(this._lines[j], "{") + 1);
                        testBraceCount = 1;
                        do {
                            obp = InStr(testLine, "{");
                            cbp = InStr(testLine, "}");
                            if (obp == 0) {
                                obp = Len(testLine) + 1;
                            }
                            if (cbp == 0) {
                                cbp = Len(testLine) + 1;
                            }
                            if (obp < cbp) {
                                testBraceCount = testBraceCount + 1;
                                testLine = Mid(testLine, obp + 1);
                            }
                            else if (cbp < obp) {
                                testBraceCount = testBraceCount - 1;
                                testLine = Mid(testLine, cbp + 1);
                            }
                        } while (!(obp == cbp || testBraceCount == 0));
                        if (testBraceCount != 0) {
                            this.AddLine("define procedure " + procName);
                            startLine = UBound(this._lines);
                            restOfLine = Trim(Right(thisLine, Len(thisLine) - InStr(thisLine, "{")));
                            braceCount = 1;
                            if (restOfLine != "") {
                                this.AddLine(restOfLine);
                            }
                            for (var m = 1; m <= Len(restOfLine); m++) {
                                if (Mid(restOfLine, m, 1) == "{") {
                                    braceCount = braceCount + 1;
                                }
                                else if (Mid(restOfLine, m, 1) == "}") {
                                    braceCount = braceCount - 1;
                                }
                            }
                            if (braceCount != 0) {
                                var k = j + 1;
                                do {
                                    for (var m = 1; m <= Len(this._lines[k]); m++) {
                                        if (Mid(this._lines[k], m, 1) == "{") {
                                            braceCount = braceCount + 1;
                                        }
                                        else if (Mid(this._lines[k], m, 1) == "}") {
                                            braceCount = braceCount - 1;
                                        }
                                        if (braceCount == 0) {
                                            lastBrace = m;
                                            break;
                                        }
                                    }
                                    if (braceCount != 0) {
                                        //put Lines(k) into another variable, as
                                        //AddLine ReDims Lines, which it can't do if
                                        //passed Lines(x) as a parameter.
                                        lineToAdd = this._lines[k];
                                        this.AddLine(lineToAdd);
                                    }
                                    else {
                                        this.AddLine(Left(this._lines[k], lastBrace - 1));
                                        afterLastBrace = Trim(Mid(this._lines[k], lastBrace + 1));
                                    }
                                    //Clear original line
                                    this._lines[k] = "";
                                    k = k + 1;
                                } while (braceCount != 0);
                            }
                            this.AddLine("end define");
                            endLineNum = UBound(this._lines);
                            this._numberSections = this._numberSections + 1;
                            if (!this._defineBlocks)
                                this._defineBlocks = [];
                            this._defineBlocks[this._numberSections] = new DefineBlock();
                            this._defineBlocks[this._numberSections].StartLine = startLine;
                            this._defineBlocks[this._numberSections].EndLine = endLineNum;
                            //Change original line where the { section
                            //started to call the new procedure.
                            startOfOrig = Trim(Left(thisLine, InStr(thisLine, "{") - 1));
                            this._lines[j] = startOfOrig + " do " + procName + " " + afterLastBrace;
                            curProc = curProc + 1;
                            // Process this line again in case there was stuff after the last brace that included
                            // more braces. e.g. } else {
                            j = j - 1;
                        }
                    }
                }
            }
            i = i + 1;
        } while (!(i > this._numberSections));
        // Join next-line "else"s to corresponding "if"s
        for (var i = 1; i <= this._numberSections; i++) {
            z = this._lines[this._defineBlocks[i].StartLine];
            if (((!this.BeginsWith(z, "define text ")) && (!this.BeginsWith(z, "define menu ")) && z != "define synonyms")) {
                for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                    if (this.BeginsWith(this._lines[j], "else ")) {
                        //Go upwards to find "if" statement that this
                        //belongs to
                        for (var k = j; k >= this._defineBlocks[i].StartLine + 1; k--) {
                            if (this.BeginsWith(this._lines[k], "if ") || InStr(this.ObliterateParameters(this._lines[k]), " if ") != 0) {
                                this._lines[k] = this._lines[k] + " " + Trim(this._lines[j]);
                                this._lines[j] = "";
                                k = this._defineBlocks[i].StartLine;
                            }
                        }
                    }
                }
            }
        }
    };
    LegacyGame.prototype.ErrorCheck = function () {
        // Parses ASL script for errors. Returns TRUE if OK;
        // False if a critical error is encountered.
        var curBegin = 0;
        var curEnd = 0;
        var hasErrors = false;
        var curPos = 0;
        var numParamStart = 0;
        var numParamEnd = 0;
        var finLoop = false;
        var inText = false;
        hasErrors = false;
        inText = false;
        // Checks for incorrect number of < and > :
        for (var i = 1; i <= UBound(this._lines); i++) {
            numParamStart = 0;
            numParamEnd = 0;
            if (this.BeginsWith(this._lines[i], "define text ")) {
                inText = true;
            }
            if (inText && Trim(LCase(this._lines[i])) == "end define") {
                inText = false;
            }
            if (!inText) {
                //Find number of <'s:
                curPos = 1;
                finLoop = false;
                do {
                    if (InStrFrom(curPos, this._lines[i], "<") != 0) {
                        numParamStart = numParamStart + 1;
                        curPos = InStrFrom(curPos, this._lines[i], "<") + 1;
                    }
                    else {
                        finLoop = true;
                    }
                } while (!(finLoop));
                //Find number of >'s:
                curPos = 1;
                finLoop = false;
                do {
                    if (InStrFrom(curPos, this._lines[i], ">") != 0) {
                        numParamEnd = numParamEnd + 1;
                        curPos = InStrFrom(curPos, this._lines[i], ">") + 1;
                    }
                    else {
                        finLoop = true;
                    }
                } while (!(finLoop));
                if (numParamStart > numParamEnd) {
                    this.LogASLError("Expected > in " + this.ReportErrorLine(this._lines[i]), LogType.FatalError);
                    hasErrors = true;
                }
                else if (numParamStart < numParamEnd) {
                    this.LogASLError("Too many > in " + this.ReportErrorLine(this._lines[i]), LogType.FatalError);
                    hasErrors = true;
                }
            }
        }
        //Exit if errors found
        if (hasErrors) {
            return true;
        }
        // Checks that define sections have parameters:
        for (var i = 1; i <= this._numberSections; i++) {
            curBegin = this._defineBlocks[i].StartLine;
            curEnd = this._defineBlocks[i].EndLine;
            if (this.BeginsWith(this._lines[curBegin], "define game")) {
                if (InStr(this._lines[curBegin], "<") == 0) {
                    this.LogASLError("'define game' has no parameter - game has no name", LogType.FatalError);
                    return true;
                }
            }
            else {
                if (!this.BeginsWith(this._lines[curBegin], "define synonyms") && !this.BeginsWith(this._lines[curBegin], "define options")) {
                    if (InStr(this._lines[curBegin], "<") == 0) {
                        this.LogASLError(this._lines[curBegin] + " has no parameter", LogType.FatalError);
                        hasErrors = true;
                    }
                }
            }
        }
        return hasErrors;
    };
    LegacyGame.prototype.GetAfterParameter = function (s) {
        // Returns everything after the end of the first parameter
        // in a string, i.e. for "use <thing> do <myproc>" it
        // returns "do <myproc>"
        var eop = 0;
        eop = InStr(s, ">");
        if (eop == 0 || eop + 1 > Len(s)) {
            return "";
        }
        else {
            return Trim(Mid(s, eop + 1));
        }
    };
    LegacyGame.prototype.ObliterateParameters = function (s) {
        var inParameter = false;
        var exitCharacter = "";
        var curChar;
        var outputLine = "";
        var obscuringFunctionName = false;
        inParameter = false;
        for (var i = 1; i <= Len(s); i++) {
            curChar = Mid(s, i, 1);
            if (inParameter) {
                if (exitCharacter == ")") {
                    if (InStr("$#%", curChar) > 0) {
                        // We might be converting a line like:
                        //   if ( $rand(1;10)$ < 3 ) then {
                        // and we don't want it to end up like this:
                        //   if (~~~~~~~~~~~)$ <~~~~~~~~~~~
                        // which will cause all sorts of confustion. So,
                        // we get rid of everything between the $ characters
                        // in this case, and set a flag so we know what we're
                        // doing.
                        obscuringFunctionName = true;
                        exitCharacter = curChar;
                        // Move along please
                        outputLine = outputLine + "~";
                        i = i + 1;
                        curChar = Mid(s, i, 1);
                    }
                }
            }
            if (!inParameter) {
                outputLine = outputLine + curChar;
                if (curChar == "<") {
                    inParameter = true;
                    exitCharacter = ">";
                }
                if (curChar == "(") {
                    inParameter = true;
                    exitCharacter = ")";
                }
            }
            else {
                if (curChar == exitCharacter) {
                    if (!obscuringFunctionName) {
                        inParameter = false;
                        outputLine = outputLine + curChar;
                    }
                    else {
                        // We've finished obscuring the function name,
                        // now let's find the next ) as we were before
                        // we found this dastardly function
                        obscuringFunctionName = false;
                        exitCharacter = ")";
                        outputLine = outputLine + "~";
                    }
                }
                else {
                    outputLine = outputLine + "~";
                }
            }
        }
        if (inParameter) {
            return "'<ERROR;" + exitCharacter + ";" + outputLine;
        }
        else {
            return outputLine;
        }
    };
    LegacyGame.prototype.ObliterateVariableNames = function (s) {
        var inParameter = false;
        var exitCharacter = "";
        var outputLine = "";
        var curChar;
        inParameter = false;
        for (var i = 1; i <= Len(s); i++) {
            curChar = Mid(s, i, 1);
            if (!inParameter) {
                outputLine = outputLine + curChar;
                if (curChar == "$") {
                    inParameter = true;
                    exitCharacter = "$";
                }
                if (curChar == "#") {
                    inParameter = true;
                    exitCharacter = "#";
                }
                if (curChar == "%") {
                    inParameter = true;
                    exitCharacter = "%";
                }
                // The ~ was for collectables, and this syntax only
                // exists in Quest 2.x. The ~ was only finally
                // allowed to be present on its own in ASL 320.
                if (curChar == "~" && this._gameAslVersion < 320) {
                    inParameter = true;
                    exitCharacter = "~";
                }
            }
            else {
                if (curChar == exitCharacter) {
                    inParameter = false;
                    outputLine = outputLine + curChar;
                }
                else {
                    outputLine = outputLine + "X";
                }
            }
        }
        if (inParameter) {
            outputLine = "'<ERROR;" + exitCharacter + ";" + outputLine;
        }
        return outputLine;
    };
    LegacyGame.prototype.RemoveComments = function () {
        var aposPos = 0;
        var inTextBlock = false;
        var inSynonymsBlock = false;
        var oblitLine;
        // If in a synonyms block, we want to remove lines which are comments, but
        // we don't want to remove synonyms that contain apostrophes, so we only
        // get rid of lines with an "'" at the beginning or with " '" in them
        for (var i = 1; i <= UBound(this._lines); i++) {
            if (this.BeginsWith(this._lines[i], "'!qdk-note:")) {
                this._lines[i] = "#!qdk-note:" + this.GetEverythingAfter(this._lines[i], "'!qdk-note:");
            }
            else {
                if (this.BeginsWith(this._lines[i], "define text ")) {
                    inTextBlock = true;
                }
                else if (Trim(this._lines[i]) == "define synonyms") {
                    inSynonymsBlock = true;
                }
                else if (this.BeginsWith(this._lines[i], "define type ")) {
                    inSynonymsBlock = true;
                }
                else if (Trim(this._lines[i]) == "end define") {
                    inTextBlock = false;
                    inSynonymsBlock = false;
                }
                if (!inTextBlock && !inSynonymsBlock) {
                    if (InStr(this._lines[i], "'") > 0) {
                        oblitLine = this.ObliterateParameters(this._lines[i]);
                        if (!this.BeginsWith(oblitLine, "'<ERROR;")) {
                            aposPos = InStr(oblitLine, "'");
                            if (aposPos != 0) {
                                this._lines[i] = Trim(Left(this._lines[i], aposPos - 1));
                            }
                        }
                    }
                }
                else if (inSynonymsBlock) {
                    if (Left(Trim(this._lines[i]), 1) == "'") {
                        this._lines[i] = "";
                    }
                    else {
                        // we look for " '", not "'" in synonyms lines
                        aposPos = InStr(this.ObliterateParameters(this._lines[i]), " '");
                        if (aposPos != 0) {
                            this._lines[i] = Trim(Left(this._lines[i], aposPos - 1));
                        }
                    }
                }
            }
        }
    };
    LegacyGame.prototype.ReportErrorLine = function (s) {
        // We don't want to see the "!intproc" in logged error reports lines.
        // This function replaces these "do" lines with a nicer-looking "..." for error reporting.
        var replaceFrom = 0;
        replaceFrom = InStr(s, "do <!intproc");
        if (replaceFrom != 0) {
            return Left(s, replaceFrom - 1) + "...";
        }
        else {
            return s;
        }
    };
    LegacyGame.prototype.YesNo = function (yn) {
        return yn ? "Yes" : "No";
    };
    LegacyGame.prototype.IsYes = function (yn) {
        if (LCase(yn) == "yes") {
            return true;
        }
        else {
            return false;
        }
    };
    LegacyGame.prototype.BeginsWith = function (s, text) {
        // Compares the beginning of the line with a given
        // string. Case insensitive.
        // Example: beginswith("Hello there","HeLlO")=TRUE
        return Left(LTrim(LCase(s)), Len(text)) == LCase(text);
    };
    LegacyGame.prototype.ConvertCasKeyword = function (casChar) {
        var keyword = this._casKeywords[casChar];
        if (keyword == "!cr") {
            keyword = "\n";
        }
        return keyword;
    };
    LegacyGame.prototype.ConvertMultiLines = function () {
        //Goes through each section capable of containing
        //script commands and puts any multiple-line script commands
        //into separate procedures. Also joins multiple-line "if"
        //statements.
        //This calls RemoveComments after joining lines, so that lines
        //with "'" as part of a multi-line parameter are not destroyed,
        //before looking for braces.
        for (var i = UBound(this._lines); i >= 1; i--) {
            if (Right(this._lines[i], 2) == "__") {
                this._lines[i] = Left(this._lines[i], Len(this._lines[i]) - 2) + LTrim(this._lines[i + 1]);
                this._lines[i + 1] = "";
                //Recalculate this line again
                i = i + 1;
            }
            else if (Right(this._lines[i], 1) == "_") {
                this._lines[i] = Left(this._lines[i], Len(this._lines[i]) - 1) + LTrim(this._lines[i + 1]);
                this._lines[i + 1] = "";
                //Recalculate this line again
                i = i + 1;
            }
        }
        this.RemoveComments();
    };
    LegacyGame.prototype.GetDefineBlock = function (blockname) {
        // Returns the start and end points of a named block.
        // Returns 0 if block not found.
        var l;
        var blockType;
        var result = new DefineBlock();
        result.StartLine = 0;
        result.EndLine = 0;
        for (var i = 1; i <= this._numberSections; i++) {
            // Get the first line of the define section:
            l = this._lines[this._defineBlocks[i].StartLine];
            // Now, starting from the first word after 'define',
            // retrieve the next word and compare it to blockname:
            // Add a space for define blocks with no parameter
            if (InStrFrom(8, l, " ") == 0) {
                l = l + " ";
            }
            blockType = Mid(l, 8, InStrFrom(8, l, " ") - 8);
            if (blockType == blockname) {
                // Return the start and end points
                result.StartLine = this._defineBlocks[i].StartLine;
                result.EndLine = this._defineBlocks[i].EndLine;
                return result;
            }
        }
        return result;
    };
    LegacyGame.prototype.DefineBlockParam = function (blockname, param) {
        // Returns the start and end points of a named block
        var cache;
        var result = new DefineBlock();
        param = "k" + param; // protect against numeric block names
        if (!this._defineBlockParams[blockname]) {
            // Lazily create cache of define block params
            cache = {};
            this._defineBlockParams[blockname] = cache;
            for (var i = 1; i <= this._numberSections; i++) {
                // get the word after "define", e.g. "procedure"
                var blockType = this.GetEverythingAfter(this._lines[this._defineBlocks[i].StartLine], "define ");
                var sp = InStr(blockType, " ");
                if (sp != 0) {
                    blockType = Trim(Left(blockType, sp - 1));
                }
                if (blockType == blockname) {
                    var blockKey = this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext, false);
                    blockKey = "k" + blockKey;
                    if (!cache[blockKey]) {
                        cache[blockKey] = this._defineBlocks[i].StartLine + "," + this._defineBlocks[i].EndLine;
                    }
                }
            }
        }
        else {
            cache = this._defineBlockParams[blockname];
        }
        if (cache[param]) {
            var blocks = Split(cache[param], ",");
            result.StartLine = parseInt(blocks[0]);
            result.EndLine = parseInt(blocks[1]);
        }
        return result;
    };
    LegacyGame.prototype.GetEverythingAfter = function (s, text) {
        if (Len(text) > Len(s)) {
            return "";
        }
        return Right(s, Len(s) - Len(text));
    };
    LegacyGame.prototype.Keyword2CAS = function (KWord) {
        if (KWord == "") {
            return "";
        }
        for (var i = 0; i <= 255; i++) {
            if (LCase(KWord) == LCase(this._casKeywords[i])) {
                return Chr(i);
            }
        }
        return this.Keyword2CAS("!unknown") + KWord + this.Keyword2CAS("!unknown");
    };
    LegacyGame.prototype.LoadCASKeywords = function () {
        this._casKeywords[0] = "!null";
        this._casKeywords[1] = "game";
        this._casKeywords[2] = "procedure";
        this._casKeywords[3] = "room";
        this._casKeywords[4] = "object";
        this._casKeywords[5] = "character";
        this._casKeywords[6] = "text";
        this._casKeywords[7] = "selection";
        this._casKeywords[8] = "define";
        this._casKeywords[9] = "end";
        this._casKeywords[10] = "!quote";
        this._casKeywords[11] = "asl-version";
        this._casKeywords[12] = "game";
        this._casKeywords[13] = "version";
        this._casKeywords[14] = "author";
        this._casKeywords[15] = "copyright";
        this._casKeywords[16] = "info";
        this._casKeywords[17] = "start";
        this._casKeywords[18] = "possitems";
        this._casKeywords[19] = "startitems";
        this._casKeywords[20] = "prefix";
        this._casKeywords[21] = "look";
        this._casKeywords[22] = "out";
        this._casKeywords[23] = "gender";
        this._casKeywords[24] = "speak";
        this._casKeywords[25] = "take";
        this._casKeywords[26] = "alias";
        this._casKeywords[27] = "place";
        this._casKeywords[28] = "east";
        this._casKeywords[29] = "north";
        this._casKeywords[30] = "west";
        this._casKeywords[31] = "south";
        this._casKeywords[32] = "give";
        this._casKeywords[33] = "hideobject";
        this._casKeywords[34] = "hidechar";
        this._casKeywords[35] = "showobject";
        this._casKeywords[36] = "showchar";
        this._casKeywords[37] = "collectable";
        this._casKeywords[38] = "collecatbles";
        this._casKeywords[39] = "command";
        this._casKeywords[40] = "use";
        this._casKeywords[41] = "hidden";
        this._casKeywords[42] = "script";
        this._casKeywords[43] = "font";
        this._casKeywords[44] = "default";
        this._casKeywords[45] = "fontname";
        this._casKeywords[46] = "fontsize";
        this._casKeywords[47] = "startscript";
        this._casKeywords[48] = "nointro";
        this._casKeywords[49] = "indescription";
        this._casKeywords[50] = "description";
        this._casKeywords[51] = "function";
        this._casKeywords[52] = "setvar";
        this._casKeywords[53] = "for";
        this._casKeywords[54] = "error";
        this._casKeywords[55] = "synonyms";
        this._casKeywords[56] = "beforeturn";
        this._casKeywords[57] = "afterturn";
        this._casKeywords[58] = "invisible";
        this._casKeywords[59] = "nodebug";
        this._casKeywords[60] = "suffix";
        this._casKeywords[61] = "startin";
        this._casKeywords[62] = "northeast";
        this._casKeywords[63] = "northwest";
        this._casKeywords[64] = "southeast";
        this._casKeywords[65] = "southwest";
        this._casKeywords[66] = "items";
        this._casKeywords[67] = "examine";
        this._casKeywords[68] = "detail";
        this._casKeywords[69] = "drop";
        this._casKeywords[70] = "everywhere";
        this._casKeywords[71] = "nowhere";
        this._casKeywords[72] = "on";
        this._casKeywords[73] = "anything";
        this._casKeywords[74] = "article";
        this._casKeywords[75] = "gain";
        this._casKeywords[76] = "properties";
        this._casKeywords[77] = "type";
        this._casKeywords[78] = "action";
        this._casKeywords[79] = "displaytype";
        this._casKeywords[80] = "override";
        this._casKeywords[81] = "enabled";
        this._casKeywords[82] = "disabled";
        this._casKeywords[83] = "variable";
        this._casKeywords[84] = "value";
        this._casKeywords[85] = "display";
        this._casKeywords[86] = "nozero";
        this._casKeywords[87] = "onchange";
        this._casKeywords[88] = "timer";
        this._casKeywords[89] = "alt";
        this._casKeywords[90] = "lib";
        this._casKeywords[91] = "up";
        this._casKeywords[92] = "down";
        this._casKeywords[93] = "gametype";
        this._casKeywords[94] = "singleplayer";
        this._casKeywords[95] = "multiplayer";
        this._casKeywords[96] = "verb";
        this._casKeywords[97] = "menu";
        this._casKeywords[98] = "container";
        this._casKeywords[99] = "surface";
        this._casKeywords[100] = "transparent";
        this._casKeywords[101] = "opened";
        this._casKeywords[102] = "parent";
        this._casKeywords[103] = "open";
        this._casKeywords[104] = "close";
        this._casKeywords[105] = "add";
        this._casKeywords[106] = "remove";
        this._casKeywords[107] = "list";
        this._casKeywords[108] = "empty";
        this._casKeywords[109] = "closed";
        this._casKeywords[110] = "options";
        this._casKeywords[111] = "abbreviations";
        this._casKeywords[112] = "locked";
        this._casKeywords[150] = "do";
        this._casKeywords[151] = "if";
        this._casKeywords[152] = "got";
        this._casKeywords[153] = "then";
        this._casKeywords[154] = "else";
        this._casKeywords[155] = "has";
        this._casKeywords[156] = "say";
        this._casKeywords[157] = "playwav";
        this._casKeywords[158] = "lose";
        this._casKeywords[159] = "msg";
        this._casKeywords[160] = "not";
        this._casKeywords[161] = "playerlose";
        this._casKeywords[162] = "playerwin";
        this._casKeywords[163] = "ask";
        this._casKeywords[164] = "goto";
        this._casKeywords[165] = "set";
        this._casKeywords[166] = "show";
        this._casKeywords[167] = "choice";
        this._casKeywords[168] = "choose";
        this._casKeywords[169] = "is";
        this._casKeywords[170] = "setstring";
        this._casKeywords[171] = "displaytext";
        this._casKeywords[172] = "exec";
        this._casKeywords[173] = "pause";
        this._casKeywords[174] = "clear";
        this._casKeywords[175] = "debug";
        this._casKeywords[176] = "enter";
        this._casKeywords[177] = "movechar";
        this._casKeywords[178] = "moveobject";
        this._casKeywords[179] = "revealchar";
        this._casKeywords[180] = "revealobject";
        this._casKeywords[181] = "concealchar";
        this._casKeywords[182] = "concealobject";
        this._casKeywords[183] = "mailto";
        this._casKeywords[184] = "and";
        this._casKeywords[185] = "or";
        this._casKeywords[186] = "outputoff";
        this._casKeywords[187] = "outputon";
        this._casKeywords[188] = "here";
        this._casKeywords[189] = "playmidi";
        this._casKeywords[190] = "drop";
        this._casKeywords[191] = "helpmsg";
        this._casKeywords[192] = "helpdisplaytext";
        this._casKeywords[193] = "helpclear";
        this._casKeywords[194] = "helpclose";
        this._casKeywords[195] = "hide";
        this._casKeywords[196] = "show";
        this._casKeywords[197] = "move";
        this._casKeywords[198] = "conceal";
        this._casKeywords[199] = "reveal";
        this._casKeywords[200] = "numeric";
        this._casKeywords[201] = "string";
        this._casKeywords[202] = "collectable";
        this._casKeywords[203] = "property";
        this._casKeywords[204] = "create";
        this._casKeywords[205] = "exit";
        this._casKeywords[206] = "doaction";
        this._casKeywords[207] = "close";
        this._casKeywords[208] = "each";
        this._casKeywords[209] = "in";
        this._casKeywords[210] = "repeat";
        this._casKeywords[211] = "while";
        this._casKeywords[212] = "until";
        this._casKeywords[213] = "timeron";
        this._casKeywords[214] = "timeroff";
        this._casKeywords[215] = "stop";
        this._casKeywords[216] = "panes";
        this._casKeywords[217] = "on";
        this._casKeywords[218] = "off";
        this._casKeywords[219] = "return";
        this._casKeywords[220] = "playmod";
        this._casKeywords[221] = "modvolume";
        this._casKeywords[222] = "clone";
        this._casKeywords[223] = "shellexe";
        this._casKeywords[224] = "background";
        this._casKeywords[225] = "foreground";
        this._casKeywords[226] = "wait";
        this._casKeywords[227] = "picture";
        this._casKeywords[228] = "nospeak";
        this._casKeywords[229] = "animate";
        this._casKeywords[230] = "persist";
        this._casKeywords[231] = "inc";
        this._casKeywords[232] = "dec";
        this._casKeywords[233] = "flag";
        this._casKeywords[234] = "dontprocess";
        this._casKeywords[235] = "destroy";
        this._casKeywords[236] = "beforesave";
        this._casKeywords[237] = "onload";
        this._casKeywords[238] = "playmp3";
        this._casKeywords[239] = "extract";
        this._casKeywords[240] = "shell";
        this._casKeywords[241] = "popup";
        this._casKeywords[242] = "select";
        this._casKeywords[243] = "case";
        this._casKeywords[244] = "lock";
        this._casKeywords[245] = "unlock";
        this._casKeywords[252] = "!startcat";
        this._casKeywords[253] = "!endcat";
        this._casKeywords[254] = "!unknown";
        this._casKeywords[255] = "!cr";
    };
    LegacyGame.prototype.LoadLibraries = function (onSuccess, onFailure, start) {
        if (start === void 0) { start = this._lines.length - 1; }
        var libFoundThisSweep = false;
        var libFileName;
        var libraryList = [];
        var numLibraries = 0;
        var libraryAlreadyIncluded = false;
        var self = this;
        for (var i = start; i >= 1; i--) {
            // We search for includes backwards as a game might include
            // some-general.lib and then something-specific.lib which needs
            // something-general; if we include something-specific first,
            // then something-general afterwards, something-general's startscript
            // gets executed before something-specific's, as we execute the
            // lib startscripts backwards as well
            if (self.BeginsWith(self._lines[i], "!include ")) {
                libFileName = self.GetParameter(self._lines[i], self._nullContext);
                //Clear !include statement
                self._lines[i] = "";
                libraryAlreadyIncluded = false;
                self.LogASLError("Including library '" + libFileName + "'...", LogType.Init);
                for (var j = 1; j <= numLibraries; j++) {
                    if (LCase(libFileName) == LCase(libraryList[j])) {
                        libraryAlreadyIncluded = true;
                        break;
                    }
                }
                if (libraryAlreadyIncluded) {
                    self.LogASLError("     - Library already included.", LogType.Init);
                }
                else {
                    numLibraries = numLibraries + 1;
                    if (!libraryList)
                        libraryList = [];
                    libraryList[numLibraries] = libFileName;
                    libFoundThisSweep = true;
                    self.LogASLError(" - Searching for " + libFileName + " (game path)", LogType.Init);
                    var loadLibrary = function (libCode) {
                        self.LogASLError("     - Found library, opening...", LogType.Init);
                        libCode = libCode.map(function (line) {
                            return self.RemoveTabs(line).trim();
                        });
                        self.LoadLibrary(libCode);
                        // continue scanning for libraries
                        self.LoadLibraries(onSuccess, onFailure, i - 1);
                    };
                    var onLibSuccess = function (data) {
                        var libCode = data.split("\n");
                        loadLibrary(libCode);
                    };
                    var onLibFailure = function () {
                        // File was not found; try standard Quest libraries (stored at the end of asl4.ts)
                        self.LogASLError("     - Library not found in game path.", LogType.Init);
                        self.LogASLError(" - Searching for " + libFileName + " (standard libraries)", LogType.Init);
                        var libCode = self.GetLibraryLines(libFileName);
                        if (libCode) {
                            loadLibrary(libCode);
                        }
                        else {
                            self.LogASLError("Library not found.", LogType.FatalError);
                            onFailure();
                        }
                    };
                    this.GetFileData(libFileName, onLibSuccess, onLibFailure);
                    break;
                }
            }
        }
        if (!libFoundThisSweep) {
            if (start == this._lines.length - 1) {
                onSuccess();
            }
            else {
                this.LoadLibraries(onSuccess, onFailure);
            }
        }
    };
    LegacyGame.prototype.LoadLibrary = function (libCode) {
        var self = this;
        var libLines = libCode.length - 1;
        var ignoreMode = false;
        var inDefGameBlock = 0;
        var gameLine = 0;
        var inDefSynBlock = 0;
        var synLine = 0;
        var inDefTypeBlock = 0;
        var typeBlockName;
        var typeLine = 0;
        var libVer = -1;
        if (libCode[1] == "!library") {
            for (var c = 1; c <= libLines; c++) {
                if (self.BeginsWith(libCode[c], "!asl-version ")) {
                    libVer = parseInt(self.GetParameter(libCode[c], self._nullContext));
                    break;
                }
            }
        }
        else {
            //Old library
            libVer = 100;
        }
        if (libVer == -1) {
            self.LogASLError(" - Library has no asl-version information.", LogType.LibraryWarningError);
            libVer = 200;
        }
        ignoreMode = false;
        for (var c = 1; c <= libLines; c++) {
            if (self.BeginsWith(libCode[c], "!include ")) {
                // Quest only honours !include in a library for asl-version
                // 311 or later, as it ignored them in versions < 3.11
                if (libVer >= 311) {
                    self.AddLine(libCode[c]);
                }
            }
            else if (Left(libCode[c], 1) != "!" && Left(libCode[c], 1) != "'" && !ignoreMode) {
                self.AddLine(libCode[c]);
            }
            else {
                if (libCode[c] == "!addto game") {
                    inDefGameBlock = 0;
                    for (var d = 1; d <= UBound(self._lines); d++) {
                        if (self.BeginsWith(self._lines[d], "define game ")) {
                            inDefGameBlock = 1;
                        }
                        else if (self.BeginsWith(self._lines[d], "define ")) {
                            if (inDefGameBlock != 0) {
                                inDefGameBlock = inDefGameBlock + 1;
                            }
                        }
                        else if (self._lines[d] == "end define" && inDefGameBlock == 1) {
                            gameLine = d;
                            break;
                        }
                        else if (self._lines[d] == "end define") {
                            if (inDefGameBlock != 0) {
                                inDefGameBlock = inDefGameBlock - 1;
                            }
                        }
                    }
                    do {
                        c = c + 1;
                        if (!self.BeginsWith(libCode[c], "!end")) {
                            // startscript lines in a library are prepended
                            // with "lib" internally so they are executed
                            // before any startscript specified by the
                            // calling ASL file, for asl-versions 311 and
                            // later.
                            // similarly, commands in a library. NB: without this, lib
                            // verbs have lower precedence than game verbs anyway. Also
                            // lib commands have lower precedence than game commands. We
                            // only need this code so that game verbs have a higher
                            // precedence than lib commands.
                            // we also need it so that lib verbs have a higher
                            // precedence than lib commands.
                            var lineToAdd;
                            if (libVer >= 311 && self.BeginsWith(libCode[c], "startscript ")) {
                                lineToAdd = "lib " + libCode[c];
                            }
                            else if (libVer >= 392 && (self.BeginsWith(libCode[c], "command ") || self.BeginsWith(libCode[c], "verb "))) {
                                lineToAdd = "lib " + libCode[c];
                            }
                            else {
                                lineToAdd = libCode[c];
                            }
                            self._lines.splice(gameLine, 0, lineToAdd);
                            gameLine = gameLine + 1;
                        }
                    } while (!(self.BeginsWith(libCode[c], "!end")));
                }
                else if (libCode[c] == "!addto synonyms") {
                    inDefSynBlock = 0;
                    for (var d = 1; d <= UBound(self._lines); d++) {
                        if (self._lines[d] == "define synonyms") {
                            inDefSynBlock = 1;
                        }
                        else if (self._lines[d] == "end define" && inDefSynBlock == 1) {
                            synLine = d;
                            d = UBound(self._lines);
                        }
                    }
                    if (inDefSynBlock == 0) {
                        //No "define synonyms" block in game - so add it
                        self.AddLine("define synonyms");
                        self.AddLine("end define");
                        synLine = UBound(self._lines);
                    }
                    do {
                        c = c + 1;
                        if (!self.BeginsWith(libCode[c], "!end")) {
                            if (!self._lines)
                                self._lines = [];
                            for (var d = UBound(self._lines); d >= synLine + 1; d--) {
                                self._lines[d] = self._lines[d - 1];
                            }
                            self._lines[synLine] = libCode[c];
                            synLine = synLine + 1;
                        }
                    } while (!(self.BeginsWith(libCode[c], "!end")));
                }
                else if (self.BeginsWith(libCode[c], "!addto type ")) {
                    inDefTypeBlock = 0;
                    typeBlockName = LCase(self.GetParameter(libCode[c], self._nullContext));
                    for (var d = 1; d <= UBound(self._lines); d++) {
                        if (LCase(self._lines[d]) == "define type <" + typeBlockName + ">") {
                            inDefTypeBlock = 1;
                        }
                        else if (self._lines[d] == "end define" && inDefTypeBlock == 1) {
                            typeLine = d;
                            d = UBound(self._lines);
                        }
                    }
                    if (inDefTypeBlock == 0) {
                        //No "define type (whatever)" block in game - so add it
                        self.AddLine("define type <" + typeBlockName + ">");
                        self.AddLine("end define");
                        typeLine = UBound(self._lines);
                    }
                    do {
                        c = c + 1;
                        if (c > libLines) {
                            break;
                        }
                        if (!self.BeginsWith(libCode[c], "!end")) {
                            if (!self._lines)
                                self._lines = [];
                            for (var d = UBound(self._lines); d >= typeLine + 1; d--) {
                                self._lines[d] = self._lines[d - 1];
                            }
                            self._lines[typeLine] = libCode[c];
                            typeLine = typeLine + 1;
                        }
                    } while (!(self.BeginsWith(libCode[c], "!end")));
                }
                else if (libCode[c] == "!library") {
                }
                else if (self.BeginsWith(libCode[c], "!asl-version ")) {
                }
                else if (self.BeginsWith(libCode[c], "'")) {
                }
                else if (self.BeginsWith(libCode[c], "!QDK")) {
                    ignoreMode = true;
                }
                else if (self.BeginsWith(libCode[c], "!end")) {
                    ignoreMode = false;
                }
            }
        }
    };
    ;
    LegacyGame.prototype.ParseFile = function (filename, onSuccess, onFailure) {
        var hasErrors = false;
        var skipCheck = false;
        var defineCount = 0;
        var curLine = 0;
        this._defineBlockParams = {};
        var self = this;
        var doParse = function () {
            skipCheck = false;
            var lastSlashPos = 0;
            var slashPos = 0;
            var curPos = 1;
            do {
                slashPos = InStrFrom(curPos, filename, "\\");
                if (slashPos != 0) {
                    lastSlashPos = slashPos;
                }
                curPos = slashPos + 1;
            } while (!(slashPos == 0));
            var filenameNoPath = LCase(Mid(filename, lastSlashPos + 1));
            // Very early versions of Quest didn't perform very good syntax checking of ASL files, so this is
            // for compatibility with games which have non-fatal errors in them.
            skipCheck = (self._skipCheckFile.indexOf(filenameNoPath) !== -1);
            if (filenameNoPath == "musicvf1.cas") {
                self._useStaticFrameForPictures = true;
            }
            //RemoveComments called within ConvertMultiLines
            self.ConvertMultiLines();
            if (!skipCheck) {
                if (!self.CheckSections()) {
                    onFailure();
                    return;
                }
            }
            self._numberSections = 1;
            for (var i = 1; i <= self._lines.length - 1; i++) {
                // find section beginning with 'define'
                if (self.BeginsWith(self._lines[i], "define")) {
                    // Now, go through until we reach an 'end define'. However, if we
                    // encounter another 'define' there is a nested define. So, if we
                    // encounter 'define' we increment the definecount. When we find an
                    // 'end define' we decrement it. When definecount is zero, we have
                    // found the end of the section.
                    defineCount = 1;
                    // Don't count the current line - we know it begins with 'define'...
                    curLine = i + 1;
                    do {
                        if (self.BeginsWith(self._lines[curLine], "define")) {
                            defineCount = defineCount + 1;
                        }
                        else if (self.BeginsWith(self._lines[curLine], "end define")) {
                            defineCount = defineCount - 1;
                        }
                        curLine = curLine + 1;
                    } while (!(defineCount == 0));
                    curLine = curLine - 1;
                    // Now, we know that the define section begins at i and ends at
                    // curline. Remember where the section begins and ends:
                    if (!self._defineBlocks)
                        self._defineBlocks = [];
                    self._defineBlocks[self._numberSections] = new DefineBlock();
                    self._defineBlocks[self._numberSections].StartLine = i;
                    self._defineBlocks[self._numberSections].EndLine = curLine;
                    self._numberSections = self._numberSections + 1;
                    i = curLine;
                }
            }
            self._numberSections = self._numberSections - 1;
            var gotGameBlock = false;
            for (var i = 1; i <= self._numberSections; i++) {
                if (self.BeginsWith(self._lines[self._defineBlocks[i].StartLine], "define game ")) {
                    gotGameBlock = true;
                    break;
                }
            }
            if (!gotGameBlock) {
                self._openErrorReport = self._openErrorReport + "No 'define game' block.\n";
                onFailure();
                return;
            }
            self.ConvertMultiLineSections();
            hasErrors = self.ConvertFriendlyIfs();
            if (!hasErrors) {
                hasErrors = self.ErrorCheck();
            }
            if (hasErrors) {
                throw "Errors found in game file.";
            }
            self._saveGameFile = "";
            onSuccess();
        };
        var loadLibrariesAndParseFile = function () {
            // Add libraries to end of _lines, then parse the result
            self.LoadLibraries(doParse, onFailure);
        };
        // Parses file and returns the positions of each main
        // 'define' block. Supports nested defines.
        //  TODO: Handle zip files
        //if (LCase(Right(filename, 4)) == ".zip") {
        //    this._originalFilename = filename;
        //    filename = GetUnzippedFile(filename);
        //    this._gamePath = System.IO.Path.GetDirectoryName(filename);
        //}
        if (LCase(Right(filename, 4)) == ".asl" || LCase(Right(filename, 4)) == ".txt") {
            var self = this;
            this.GetFileData(filename, function (fileData) {
                var aslLines = fileData.replace(/\r\n/g, "\n").split("\n");
                self._lines = [];
                self._lines[0] = "";
                for (var l = 1; l <= aslLines.length; l++) {
                    self._lines[l] = self.RemoveTabs(aslLines[l - 1]).trim();
                }
                loadLibrariesAndParseFile();
            }, onFailure);
        }
        else if (LCase(Right(filename, 4)) == ".cas") {
            this.LogASLError("Loading CAS");
            this.LoadCASFile(filename);
            loadLibrariesAndParseFile();
        }
        else {
            throw "Unrecognized file extension";
        }
    };
    LegacyGame.prototype.LogASLError = function (err, type) {
        if (type === void 0) { type = LogType.Misc; }
        if (type == LogType.FatalError) {
            err = "FATAL ERROR: " + err;
        }
        else if (type == LogType.WarningError) {
            err = "ERROR: " + err;
        }
        else if (type == LogType.LibraryWarningError) {
            err = "WARNING ERROR (LIBRARY): " + err;
        }
        else if (type == LogType.Init) {
            err = "INIT: " + err;
        }
        else if (type == LogType.Warning) {
            err = "WARNING: " + err;
        }
        else if (type == LogType.UserError) {
            err = "ERROR (REQUESTED): " + err;
        }
        else if (type == LogType.InternalError) {
            err = "INTERNAL ERROR: " + err;
        }
        console.log(err);
    };
    LegacyGame.prototype.GetParameter = function (s, ctx, convertStringVariables) {
        if (convertStringVariables === void 0) { convertStringVariables = true; }
        // Returns the parameters between < and > in a string
        var newParam;
        var startPos = 0;
        var endPos = 0;
        startPos = InStr(s, "<");
        endPos = InStr(s, ">");
        if (startPos == 0 || endPos == 0) {
            this.LogASLError("Expected parameter in '" + this.ReportErrorLine(s) + "'", LogType.WarningError);
            return "";
        }
        var retrParam = Mid(s, startPos + 1, (endPos - startPos) - 1);
        if (convertStringVariables) {
            if (this._gameAslVersion >= 320) {
                newParam = this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%", ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx);
            }
            else {
                if (Left(retrParam, 9) != "~Internal") {
                    newParam = this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%", ConvertType.Numeric, ctx), "~", ConvertType.Collectables, ctx), "$", ConvertType.Functions, ctx);
                }
                else {
                    newParam = retrParam;
                }
            }
        }
        else {
            newParam = retrParam;
        }
        return this.EvaluateInlineExpressions(newParam);
    };
    LegacyGame.prototype.AddLine = function (line) {
        //Adds a line to the game script
        var numLines = 0;
        numLines = UBound(this._lines) + 1;
        if (!this._lines)
            this._lines = [];
        this._lines[numLines] = line;
    };
    LegacyGame.prototype.LoadCASFile = function (filename) {
        var endLineReached = false;
        var exitTheLoop = false;
        var textMode = false;
        var casVersion = 0;
        var startCat = "";
        var endCatPos = 0;
        var chkVer;
        var j = 0;
        var curLin;
        var textData;
        var cpos = 0;
        var nextLinePos = 0;
        var c;
        var tl;
        var ckw;
        var d;
        this._lines = [];
        var fileData = this.GetCASFileData(filename);
        chkVer = Left(fileData, 7);
        if (chkVer == "QCGF001") {
            casVersion = 1;
        }
        else if (chkVer == "QCGF002") {
            casVersion = 2;
        }
        else if (chkVer == "QCGF003") {
            casVersion = 3;
        }
        else {
            throw "Invalid or corrupted CAS file.";
        }
        if (casVersion == 3) {
            startCat = this.Keyword2CAS("!startcat");
        }
        for (var i = 9; i <= Len(fileData); i++) {
            if (casVersion == 3 && Mid(fileData, i, 1) == startCat) {
                // Read catalog
                this._startCatPos = i;
                endCatPos = InStrFrom(j, fileData, this.Keyword2CAS("!endcat"));
                this.ReadCatalog(Mid(fileData, j + 1, endCatPos - j - 1));
                this._resourceFile = filename;
                this._resourceOffset = endCatPos + 1;
                i = Len(fileData);
                this._casFileData = fileData;
            }
            else {
                curLin = "";
                endLineReached = false;
                if (textMode) {
                    textData = Mid(fileData, i, InStrFrom(i, fileData, Chr(253)) - (i - 1));
                    textData = Left(textData, Len(textData) - 1);
                    cpos = 1;
                    var finished = false;
                    if (textData != "") {
                        do {
                            nextLinePos = InStrFrom(cpos, textData, Chr(0));
                            if (nextLinePos == 0) {
                                nextLinePos = Len(textData) + 1;
                                finished = true;
                            }
                            tl = this.DecryptString(Mid(textData, cpos, nextLinePos - cpos));
                            this.AddLine(tl);
                            cpos = nextLinePos + 1;
                        } while (!(finished));
                    }
                    textMode = false;
                    i = InStrFrom(i, fileData, Chr(253));
                }
                j = i;
                do {
                    ckw = Asc(Mid(fileData, j, 1));
                    c = this.ConvertCasKeyword(ckw);
                    if (c == "\n") {
                        endLineReached = true;
                    }
                    else {
                        if (Left(c, 1) != "!") {
                            curLin = curLin + c + " ";
                        }
                        else {
                            if (c == "!quote") {
                                exitTheLoop = false;
                                curLin = curLin + "<";
                                do {
                                    j = j + 1;
                                    d = Mid(fileData, j, 1);
                                    if (d != Chr(0)) {
                                        curLin = curLin + this.DecryptString(d);
                                    }
                                    else {
                                        curLin = curLin + "> ";
                                        exitTheLoop = true;
                                    }
                                } while (!(exitTheLoop));
                            }
                            else if (c == "!unknown") {
                                exitTheLoop = false;
                                do {
                                    j = j + 1;
                                    d = Mid(fileData, j, 1);
                                    if (d != Chr(254)) {
                                        curLin = curLin + d;
                                    }
                                    else {
                                        exitTheLoop = true;
                                    }
                                } while (!(exitTheLoop));
                                curLin = curLin + " ";
                            }
                        }
                    }
                    j = j + 1;
                } while (!(endLineReached));
                this.AddLine(Trim(curLin));
                if (this.BeginsWith(curLin, "define text") || (casVersion >= 2 && (this.BeginsWith(curLin, "define synonyms") || this.BeginsWith(curLin, "define type") || this.BeginsWith(curLin, "define menu")))) {
                    textMode = true;
                }
                //j is already at correct place, but i will be
                //incremented - so put j back one or we will miss a
                //character.
                i = j - 1;
            }
        }
    };
    LegacyGame.prototype.RemoveTabs = function (s) {
        return s.replace(/\t/g, "    ");
    };
    LegacyGame.prototype.DoAddRemove = function (childId, parentId, add, ctx) {
        if (add) {
            this.AddToObjectProperties("parent=" + this._objs[parentId].ObjectName, childId, ctx);
            this._objs[childId].ContainerRoom = this._objs[parentId].ContainerRoom;
        }
        else {
            this.AddToObjectProperties("not parent", childId, ctx);
        }
        if (this._gameAslVersion >= 410) {
            // Putting something in a container implicitly makes that
            // container "seen". Otherwise we could try to "look at" the
            // object we just put in the container and have disambigution fail!
            this.AddToObjectProperties("seen", parentId, ctx);
        }
        this.UpdateVisibilityInContainers(ctx, this._objs[parentId].ObjectName);
    };
    LegacyGame.prototype.DoLook = function (id, ctx, showExamineError, showDefaultDescription) {
        if (showExamineError === void 0) { showExamineError = false; }
        if (showDefaultDescription === void 0) { showDefaultDescription = true; }
        var objectContents;
        var foundLook = false;
        // First, set the "seen" property, and for ASL >= 391, update visibility for any
        // object that is contained by this object.
        if (this._gameAslVersion >= 391) {
            this.AddToObjectProperties("seen", id, ctx);
            this.UpdateVisibilityInContainers(ctx, this._objs[id].ObjectName);
        }
        // First look for action, then look
        // for property, then check define
        // section:
        var lookLine;
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == "look") {
                foundLook = true;
                this.ExecuteScript(o.Actions[i].Script, ctx);
                break;
            }
        }
        if (!foundLook) {
            for (var i = 1; i <= o.NumberProperties; i++) {
                if (o.Properties[i].PropertyName == "look") {
                    // do this odd RetrieveParameter stuff to convert any variables
                    this.Print(this.GetParameter("<" + o.Properties[i].PropertyValue + ">", ctx), ctx);
                    foundLook = true;
                    break;
                }
            }
        }
        if (!foundLook) {
            for (var i = o.DefinitionSectionStart; i <= o.DefinitionSectionEnd; i++) {
                if (this.BeginsWith(this._lines[i], "look ")) {
                    lookLine = Trim(this.GetEverythingAfter(this._lines[i], "look "));
                    if (Left(lookLine, 1) == "<") {
                        this.Print(this.GetParameter(this._lines[i], ctx), ctx);
                    }
                    else {
                        this.ExecuteScript(lookLine, ctx, id);
                    }
                    foundLook = true;
                }
            }
        }
        if (this._gameAslVersion >= 391) {
            objectContents = this.ListContents(id, ctx);
        }
        else {
            objectContents = "";
        }
        if (!foundLook && showDefaultDescription) {
            var err;
            if (showExamineError) {
                err = PlayerError.DefaultExamine;
            }
            else {
                err = PlayerError.DefaultLook;
            }
            // Print "Nothing out of the ordinary" or whatever, but only if we're not going to list
            // any contents.
            if (objectContents == "") {
                this.PlayerErrorMessage(err, ctx);
            }
        }
        if (objectContents != "" && objectContents != "<script>") {
            this.Print(objectContents, ctx);
        }
    };
    LegacyGame.prototype.DoOpenClose = function (id, open, showLook, ctx) {
        if (open) {
            this.AddToObjectProperties("opened", id, ctx);
            if (showLook) {
                this.DoLook(id, ctx, null, false);
            }
        }
        else {
            this.AddToObjectProperties("not opened", id, ctx);
        }
        this.UpdateVisibilityInContainers(ctx, this._objs[id].ObjectName);
    };
    LegacyGame.prototype.EvaluateInlineExpressions = function (s) {
        // Evaluates in-line expressions e.g. msg <Hello, did you know that 2 + 2 = {2+2}?>
        if (this._gameAslVersion < 391) {
            return s;
        }
        var bracePos = 0;
        var curPos = 1;
        var resultLine = "";
        do {
            bracePos = InStrFrom(curPos, s, "{");
            if (bracePos != 0) {
                resultLine = resultLine + Mid(s, curPos, bracePos - curPos);
                if (Mid(s, bracePos, 2) == "{{") {
                    // {{ = {
                    curPos = bracePos + 2;
                    resultLine = resultLine + "{";
                }
                else {
                    var EndBracePos = InStrFrom(bracePos + 1, s, "}");
                    if (EndBracePos == 0) {
                        this.LogASLError("Expected } in '" + s + "'", LogType.WarningError);
                        return "<ERROR>";
                    }
                    else {
                        var expression = Mid(s, bracePos + 1, EndBracePos - bracePos - 1);
                        var expResult = this.ExpressionHandler(expression);
                        if (expResult.Success != ExpressionSuccess.OK) {
                            this.LogASLError("Error evaluating expression in <" + s + "> - " + expResult.Message);
                            return "<ERROR>";
                        }
                        resultLine = resultLine + expResult.Result;
                        curPos = EndBracePos + 1;
                    }
                }
            }
            else {
                resultLine = resultLine + Mid(s, curPos);
            }
        } while (!(bracePos == 0 || curPos > Len(s)));
        // Above, we only bothered checking for {{. But for consistency, also }} = }. So let's do that:
        curPos = 1;
        do {
            bracePos = InStrFrom(curPos, resultLine, "}}");
            if (bracePos != 0) {
                resultLine = Left(resultLine, bracePos) + Mid(resultLine, bracePos + 2);
                curPos = bracePos + 1;
            }
        } while (!(bracePos == 0 || curPos > Len(resultLine)));
        return resultLine;
    };
    LegacyGame.prototype.ExecAddRemove = function (cmd, ctx) {
        var childId = 0;
        var childName;
        var doAdd = false;
        var sepPos = 0;
        var parentId = 0;
        var sepLen = 0;
        var parentName;
        var verb = "";
        var action;
        var foundAction = false;
        var actionScript = "";
        var propertyExists = false;
        var textToPrint;
        var isContainer = false;
        var gotObject = false;
        var childLength = 0;
        var noParentSpecified = false;
        if (this.BeginsWith(cmd, "put ")) {
            verb = "put";
            doAdd = true;
            sepPos = InStr(cmd, " on ");
            sepLen = 4;
            if (sepPos == 0) {
                sepPos = InStr(cmd, " in ");
                sepLen = 4;
            }
            if (sepPos == 0) {
                sepPos = InStr(cmd, " onto ");
                sepLen = 6;
            }
        }
        else if (this.BeginsWith(cmd, "add ")) {
            verb = "add";
            doAdd = true;
            sepPos = InStr(cmd, " to ");
            sepLen = 4;
        }
        else if (this.BeginsWith(cmd, "remove ")) {
            verb = "remove";
            doAdd = false;
            sepPos = InStr(cmd, " from ");
            sepLen = 6;
        }
        if (sepPos == 0) {
            noParentSpecified = true;
            sepPos = Len(cmd) + 1;
        }
        childLength = sepPos - (Len(verb) + 2);
        if (childLength < 0) {
            this.PlayerErrorMessage(PlayerError.BadCommand, ctx);
            this._badCmdBefore = verb;
            return;
        }
        childName = Trim(Mid(cmd, Len(verb) + 2, childLength));
        gotObject = false;
        if (this._gameAslVersion >= 392 && doAdd) {
            childId = this.Disambiguate(childName, this._currentRoom + ";inventory", ctx);
            if (childId > 0) {
                if (this._objs[childId].ContainerRoom == "inventory") {
                    gotObject = true;
                }
                else {
                    // Player is not carrying the object they referred to. So, first take the object.
                    this.Print("(first taking " + this._objs[childId].Article + ")", ctx);
                    // Try to take the object
                    ctx.AllowRealNamesInCommand = true;
                    this.ExecCommand("take " + this._objs[childId].ObjectName, ctx, false, null, true);
                    if (this._objs[childId].ContainerRoom == "inventory") {
                        gotObject = true;
                    }
                }
                if (!gotObject) {
                    this._badCmdBefore = verb;
                    return;
                }
            }
            else {
                if (childId != -2) {
                    this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                }
                this._badCmdBefore = verb;
                return;
            }
        }
        else {
            childId = this.Disambiguate(childName, "inventory;" + this._currentRoom, ctx);
            if (childId <= 0) {
                if (childId != -2) {
                    this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                this._badCmdBefore = verb;
                return;
            }
        }
        if (noParentSpecified && doAdd) {
            this.SetStringContents("quest.error.article", this._objs[childId].Article, ctx);
            this.PlayerErrorMessage(PlayerError.BadPut, ctx);
            return;
        }
        if (doAdd) {
            action = "add";
        }
        else {
            action = "remove";
        }
        if (!noParentSpecified) {
            parentName = Trim(Mid(cmd, sepPos + sepLen));
            parentId = this.Disambiguate(parentName, this._currentRoom + ";inventory", ctx);
            if (parentId <= 0) {
                if (parentId != -2) {
                    this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                this._badCmdBefore = Left(cmd, sepPos + sepLen);
                return;
            }
        }
        else {
            // Assume the player was referring to the parent that the object is already in,
            // if it is even in an object already
            if (!this.IsYes(this.GetObjectProperty("parent", childId, true, false))) {
                this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
                return;
            }
            parentId = this.GetObjectIdNoAlias(this.GetObjectProperty("parent", childId, false, true));
        }
        // Check if parent is a container
        isContainer = this.IsYes(this.GetObjectProperty("container", parentId, true, false));
        if (!isContainer) {
            if (doAdd) {
                this.PlayerErrorMessage(PlayerError.CantPut, ctx);
            }
            else {
                this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }
            return;
        }
        // Check object is already held by that parent
        if (this.IsYes(this.GetObjectProperty("parent", childId, true, false))) {
            if (doAdd && LCase(this.GetObjectProperty("parent", childId, false, false)) == LCase(this._objs[parentId].ObjectName)) {
                this.PlayerErrorMessage(PlayerError.AlreadyPut, ctx);
            }
        }
        // Check parent and child are accessible to player
        var canAccessObject = this.PlayerCanAccessObject(childId);
        if (!canAccessObject.CanAccessObject) {
            if (doAdd) {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessObject.ErrorMsg);
            }
            else {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessObject.ErrorMsg);
            }
            return;
        }
        var canAccessParent = this.PlayerCanAccessObject(parentId);
        if (!canAccessParent.CanAccessObject) {
            if (doAdd) {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessParent.ErrorMsg);
            }
            else {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessParent.ErrorMsg);
            }
            return;
        }
        // Check if parent is a closed container
        if (!this.IsYes(this.GetObjectProperty("surface", parentId, true, false)) && !this.IsYes(this.GetObjectProperty("opened", parentId, true, false))) {
            // Not a surface and not open, so can't add to this closed container.
            if (doAdd) {
                this.PlayerErrorMessage(PlayerError.CantPut, ctx);
            }
            else {
                this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }
            return;
        }
        // Now check if it can be added to (or removed from)
        // First check for an action
        var o = this._objs[parentId];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (LCase(o.Actions[i].ActionName) == action) {
                foundAction = true;
                actionScript = o.Actions[i].Script;
                break;
            }
        }
        if (foundAction) {
            this.SetStringContents("quest." + LCase(action) + ".object.name", this._objs[childId].ObjectName, ctx);
            this.ExecuteScript(actionScript, ctx, parentId);
        }
        else {
            // Now check for a property
            propertyExists = this.IsYes(this.GetObjectProperty(action, parentId, true, false));
            if (!propertyExists) {
                // Show error message
                if (doAdd) {
                    this.PlayerErrorMessage(PlayerError.CantPut, ctx);
                }
                else {
                    this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
                }
            }
            else {
                textToPrint = this.GetObjectProperty(action, parentId, false, false);
                if (textToPrint == "") {
                    // Show default message
                    if (doAdd) {
                        this.PlayerErrorMessage(PlayerError.DefaultPut, ctx);
                    }
                    else {
                        this.PlayerErrorMessage(PlayerError.DefaultRemove, ctx);
                    }
                }
                else {
                    this.Print(textToPrint, ctx);
                }
                this.DoAddRemove(childId, parentId, doAdd, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecAddRemoveScript = function (parameter, add, ctx) {
        var childId = 0;
        var parentId = 0;
        var commandName;
        var childName;
        var parentName = "";
        var scp = 0;
        if (add) {
            commandName = "add";
        }
        else {
            commandName = "remove";
        }
        scp = InStr(parameter, ";");
        if (scp == 0 && add) {
            this.LogASLError("No parent specified in '" + commandName + " <" + parameter + ">", LogType.WarningError);
            return;
        }
        if (scp != 0) {
            childName = LCase(Trim(Left(parameter, scp - 1)));
            parentName = LCase(Trim(Mid(parameter, scp + 1)));
        }
        else {
            childName = LCase(Trim(parameter));
        }
        childId = this.GetObjectIdNoAlias(childName);
        if (childId == 0) {
            this.LogASLError("Invalid child object name specified in '" + commandName + " <" + parameter + ">", LogType.WarningError);
            return;
        }
        if (scp != 0) {
            parentId = this.GetObjectIdNoAlias(parentName);
            if (parentId == 0) {
                this.LogASLError("Invalid parent object name specified in '" + commandName + " <" + parameter + ">", LogType.WarningError);
                return;
            }
            this.DoAddRemove(childId, parentId, add, ctx);
        }
        else {
            this.AddToObjectProperties("not parent", childId, ctx);
            this.UpdateVisibilityInContainers(ctx, this._objs[parentId].ObjectName);
        }
    };
    LegacyGame.prototype.ExecOpenClose = function (cmd, ctx) {
        var id = 0;
        var name;
        var doOpen = false;
        var isOpen = false;
        var foundAction = false;
        var action = "";
        var actionScript = "";
        var propertyExists = false;
        var textToPrint;
        var isContainer = false;
        if (this.BeginsWith(cmd, "open ")) {
            action = "open";
            doOpen = true;
        }
        else if (this.BeginsWith(cmd, "close ")) {
            action = "close";
            doOpen = false;
        }
        name = this.GetEverythingAfter(cmd, action + " ");
        id = this.Disambiguate(name, this._currentRoom + ";inventory", ctx);
        if (id <= 0) {
            if (id != -2) {
                this.PlayerErrorMessage(PlayerError.BadThing, ctx);
            }
            this._badCmdBefore = action;
            return;
        }
        // Check if it's even a container
        isContainer = this.IsYes(this.GetObjectProperty("container", id, true, false));
        if (!isContainer) {
            if (doOpen) {
                this.PlayerErrorMessage(PlayerError.CantOpen, ctx);
            }
            else {
                this.PlayerErrorMessage(PlayerError.CantClose, ctx);
            }
            return;
        }
        // Check if it's already open (or closed)
        isOpen = this.IsYes(this.GetObjectProperty("opened", id, true, false));
        if (doOpen && isOpen) {
            // Object is already open
            this.PlayerErrorMessage(PlayerError.AlreadyOpen, ctx);
            return;
        }
        else if (!doOpen && !isOpen) {
            // Object is already closed
            this.PlayerErrorMessage(PlayerError.AlreadyClosed, ctx);
            return;
        }
        // Check if it's accessible, i.e. check it's not itself inside another closed container
        var canAccessObject = this.PlayerCanAccessObject(id);
        if (!canAccessObject.CanAccessObject) {
            if (doOpen) {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantOpen, ctx, canAccessObject.ErrorMsg);
            }
            else {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantClose, ctx, canAccessObject.ErrorMsg);
            }
            return;
        }
        // Now check if it can be opened (or closed)
        // First check for an action
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (LCase(o.Actions[i].ActionName) == action) {
                foundAction = true;
                actionScript = o.Actions[i].Script;
                break;
            }
        }
        if (foundAction) {
            this.ExecuteScript(actionScript, ctx, id);
        }
        else {
            // Now check for a property
            propertyExists = this.IsYes(this.GetObjectProperty(action, id, true, false));
            if (!propertyExists) {
                // Show error message
                if (doOpen) {
                    this.PlayerErrorMessage(PlayerError.CantOpen, ctx);
                }
                else {
                    this.PlayerErrorMessage(PlayerError.CantClose, ctx);
                }
            }
            else {
                textToPrint = this.GetObjectProperty(action, id, false, false);
                if (textToPrint == "") {
                    // Show default message
                    if (doOpen) {
                        this.PlayerErrorMessage(PlayerError.DefaultOpen, ctx);
                    }
                    else {
                        this.PlayerErrorMessage(PlayerError.DefaultClose, ctx);
                    }
                }
                else {
                    this.Print(textToPrint, ctx);
                }
                this.DoOpenClose(id, doOpen, true, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecuteSelectCase = function (script, ctx) {
        // ScriptLine passed will look like this:
        //   select case <whatever> do <!intprocX>
        // with all the case statements in the intproc.
        var afterLine = this.GetAfterParameter(script);
        if (!this.BeginsWith(afterLine, "do <!intproc")) {
            this.LogASLError("No case block specified for '" + script + "'", LogType.WarningError);
            return;
        }
        var blockName = this.GetParameter(afterLine, ctx);
        var block = this.DefineBlockParam("procedure", blockName);
        var checkValue = this.GetParameter(script, ctx);
        var caseMatch = false;
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            // Go through all the cases until we find the one that matches
            if (this._lines[i] != "") {
                if (!this.BeginsWith(this._lines[i], "case ")) {
                    this.LogASLError("Invalid line in 'select case' block: '" + this._lines[i] + "'", LogType.WarningError);
                }
                else {
                    var caseScript = "";
                    if (this.BeginsWith(this._lines[i], "case else ")) {
                        caseMatch = true;
                        caseScript = this.GetEverythingAfter(this._lines[i], "case else ");
                    }
                    else {
                        var thisCase = this.GetParameter(this._lines[i], ctx);
                        var finished = false;
                        do {
                            var SCP = InStr(thisCase, ";");
                            if (SCP == 0) {
                                SCP = Len(thisCase) + 1;
                                finished = true;
                            }
                            var condition = Trim(Left(thisCase, SCP - 1));
                            if (condition == checkValue) {
                                caseScript = this.GetAfterParameter(this._lines[i]);
                                caseMatch = true;
                                finished = true;
                            }
                            else {
                                thisCase = Mid(thisCase, SCP + 1);
                            }
                        } while (!(finished));
                    }
                    if (caseMatch) {
                        this.ExecuteScript(caseScript, ctx);
                        return;
                    }
                }
            }
        }
    };
    LegacyGame.prototype.ExecVerb = function (cmd, ctx, libCommands) {
        if (libCommands === void 0) { libCommands = false; }
        var gameBlock;
        var foundVerb = false;
        var verbProperty = "";
        var script = "";
        var verbsList;
        var thisVerb = "";
        var scp = 0;
        var id = 0;
        var verbObject = "";
        var verbTag;
        var thisScript = "";
        if (!libCommands) {
            verbTag = "verb ";
        }
        else {
            verbTag = "lib verb ";
        }
        gameBlock = this.GetDefineBlock("game");
        for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], verbTag)) {
                verbsList = this.GetParameter(this._lines[i], ctx);
                // The property or action the verb uses is either after a colon,
                // or it's the first (or only) verb on the line.
                var colonPos = InStr(verbsList, ":");
                if (colonPos != 0) {
                    verbProperty = LCase(Trim(Mid(verbsList, colonPos + 1)));
                    verbsList = Trim(Left(verbsList, colonPos - 1));
                }
                else {
                    scp = InStr(verbsList, ";");
                    if (scp == 0) {
                        verbProperty = LCase(verbsList);
                    }
                    else {
                        verbProperty = LCase(Trim(Left(verbsList, scp - 1)));
                    }
                }
                // Now let's see if this matches:
                do {
                    scp = InStr(verbsList, ";");
                    if (scp == 0) {
                        thisVerb = LCase(verbsList);
                    }
                    else {
                        thisVerb = LCase(Trim(Left(verbsList, scp - 1)));
                    }
                    if (this.BeginsWith(cmd, thisVerb + " ")) {
                        foundVerb = true;
                        verbObject = this.GetEverythingAfter(cmd, thisVerb + " ");
                        script = Trim(Mid(this._lines[i], InStr(this._lines[i], ">") + 1));
                    }
                    if (scp != 0) {
                        verbsList = Trim(Mid(verbsList, scp + 1));
                    }
                } while (!(scp == 0 || Trim(verbsList) == "" || foundVerb));
                if (foundVerb) {
                    break;
                }
            }
        }
        if (foundVerb) {
            id = this.Disambiguate(verbObject, "inventory;" + this._currentRoom, ctx);
            if (id < 0) {
                if (id != -2) {
                    this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                this._badCmdBefore = thisVerb;
            }
            else {
                this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
                var foundAction = false;
                // Now see if this object has the relevant action or property
                var o = this._objs[id];
                for (var i = 1; i <= o.NumberActions; i++) {
                    if (LCase(o.Actions[i].ActionName) == verbProperty) {
                        foundAction = true;
                        thisScript = o.Actions[i].Script;
                        break;
                    }
                }
                if (thisScript != "") {
                    // Avoid an RTE "this array is fixed or temporarily locked"
                    this.ExecuteScript(thisScript, ctx, id);
                }
                if (!foundAction) {
                    // Check properties for a message
                    for (var i = 1; i <= o.NumberProperties; i++) {
                        if (LCase(o.Properties[i].PropertyName) == verbProperty) {
                            foundAction = true;
                            this.Print(o.Properties[i].PropertyValue, ctx);
                            break;
                        }
                    }
                }
                if (!foundAction) {
                    // Execute the default script from the verb definition
                    this.ExecuteScript(script, ctx);
                }
            }
        }
        return foundVerb;
    };
    LegacyGame.prototype.ExpressionHandler = function (expr) {
        var openBracketPos = 0;
        var endBracketPos = 0;
        var res = new ExpressionResult();
        // Find brackets, recursively call ExpressionHandler
        do {
            openBracketPos = InStr(expr, "(");
            if (openBracketPos != 0) {
                // Find equivalent closing bracket
                var BracketCount = 1;
                endBracketPos = 0;
                for (var i = openBracketPos + 1; i <= Len(expr); i++) {
                    if (Mid(expr, i, 1) == "(") {
                        BracketCount = BracketCount + 1;
                    }
                    else if (Mid(expr, i, 1) == ")") {
                        BracketCount = BracketCount - 1;
                    }
                    if (BracketCount == 0) {
                        endBracketPos = i;
                        break;
                    }
                }
                if (endBracketPos != 0) {
                    var NestedResult = this.ExpressionHandler(Mid(expr, openBracketPos + 1, endBracketPos - openBracketPos - 1));
                    if (NestedResult.Success != ExpressionSuccess.OK) {
                        res.Success = NestedResult.Success;
                        res.Message = NestedResult.Message;
                        return res;
                    }
                    expr = Left(expr, openBracketPos - 1) + " " + NestedResult.Result + " " + Mid(expr, endBracketPos + 1);
                }
                else {
                    res.Message = "Missing closing bracket";
                    res.Success = ExpressionSuccess.Fail;
                    return res;
                }
            }
        } while (!(openBracketPos == 0));
        // Split expression into elements, e.g.:
        //       2 + 3 * 578.2 / 36
        //       E O E O EEEEE O EE      where E=Element, O=Operator
        var numElements = 1;
        var elements;
        elements = [];
        var numOperators = 0;
        var operators = [];
        var newElement = false;
        var obscuredExpr = this.ObscureNumericExps(expr);
        for (var i = 1; i <= Len(expr); i++) {
            switch (Mid(obscuredExpr, i, 1)) {
                case "+":
                case "*":
                case "/":
                    newElement = true;
                    break;
                case "-":
                    // A minus often means subtraction, so it's a new element. But sometimes
                    // it just denotes a negative number. In this case, the current element will
                    // be empty.
                    if (Trim(elements[numElements]) == "") {
                        newElement = false;
                    }
                    else {
                        newElement = true;
                    }
                    break;
                default:
                    newElement = false;
                    break;
            }
            if (newElement) {
                numElements = numElements + 1;
                if (!elements)
                    elements = [];
                numOperators = numOperators + 1;
                if (!operators)
                    operators = [];
                operators[numOperators] = Mid(expr, i, 1);
            }
            else {
                elements[numElements] = elements[numElements] + Mid(expr, i, 1);
            }
        }
        // Check Elements are numeric, and trim spaces
        for (var i = 1; i <= numElements; i++) {
            elements[i] = Trim(elements[i]);
            if (!IsNumeric(elements[i])) {
                res.Message = "Syntax error evaluating expression - non-numeric element '" + elements[i] + "'";
                res.Success = ExpressionSuccess.Fail;
                return res;
            }
        }
        var opNum = 0;
        do {
            // Go through the Operators array to find next calculation to perform
            for (var i = 1; i <= numOperators; i++) {
                if (operators[i] == "/" || operators[i] == "*") {
                    opNum = i;
                    break;
                }
            }
            if (opNum == 0) {
                for (var i = 1; i <= numOperators; i++) {
                    if (operators[i] == "+" || operators[i] == "-") {
                        opNum = i;
                        break;
                    }
                }
            }
            // If OpNum is still 0, there are no calculations left to do.
            if (opNum != 0) {
                var val1 = parseFloat(elements[opNum]);
                var val2 = parseFloat(elements[opNum + 1]);
                var result = 0;
                switch (operators[opNum]) {
                    case "/":
                        if (val2 == 0) {
                            res.Message = "Division by zero";
                            res.Success = ExpressionSuccess.Fail;
                            return res;
                        }
                        result = val1 / val2;
                        break;
                    case "*":
                        result = val1 * val2;
                        break;
                    case "+":
                        result = val1 + val2;
                        break;
                    case "-":
                        result = val1 - val2;
                        break;
                }
                elements[opNum] = (result).toString();
                // Remove this operator, and Elements(OpNum+1) from the arrays
                for (var i = opNum; i <= numOperators - 1; i++) {
                    operators[i] = operators[i + 1];
                }
                for (var i = opNum + 1; i <= numElements - 1; i++) {
                    elements[i] = elements[i + 1];
                }
                numOperators = numOperators - 1;
                numElements = numElements - 1;
                if (!operators)
                    operators = [];
                if (!elements)
                    elements = [];
            }
        } while (!(opNum == 0 || numOperators == 0));
        res.Success = ExpressionSuccess.OK;
        res.Result = elements[1];
        return res;
    };
    LegacyGame.prototype.ListContents = function (id, ctx) {
        // Returns a formatted list of the contents of a container.
        // If the list action causes a script to be run instead, ListContents
        // returns "<script>"
        var contentsIDs = [];
        if (!this.IsYes(this.GetObjectProperty("container", id, true, false))) {
            return "";
        }
        if (!this.IsYes(this.GetObjectProperty("opened", id, true, false)) && !this.IsYes(this.GetObjectProperty("transparent", id, true, false)) && !this.IsYes(this.GetObjectProperty("surface", id, true, false))) {
            // Container is closed, so return "list closed" property if there is one.
            if (this.DoAction(id, "list closed", ctx, false)) {
                return "<script>";
            }
            else {
                return this.GetObjectProperty("list closed", id, false, false);
            }
        }
        // populate contents string
        var numContents = 0;
        for (var i = 1; i <= this._numberObjs; i++) {
            if (this._objs[i].Exists && this._objs[i].Visible) {
                if (LCase(this.GetObjectProperty("parent", i, false, false)) == LCase(this._objs[id].ObjectName)) {
                    numContents = numContents + 1;
                    if (!contentsIDs)
                        contentsIDs = [];
                    contentsIDs[numContents] = i;
                }
            }
        }
        var contents = "";
        if (numContents > 0) {
            // Check if list property is set.
            if (this.DoAction(id, "list", ctx, false)) {
                return "<script>";
            }
            if (this.IsYes(this.GetObjectProperty("list", id, true, false))) {
                // Read header, if any
                var listString = this.GetObjectProperty("list", id, false, false);
                var displayList = true;
                if (listString != "") {
                    if (Right(listString, 1) == ":") {
                        contents = Left(listString, Len(listString) - 1) + " ";
                    }
                    else {
                        // If header doesn't end in a colon, then the header is the only text to print
                        contents = listString;
                        displayList = false;
                    }
                }
                else {
                    contents = UCase(Left(this._objs[id].Article, 1)) + Mid(this._objs[id].Article, 2) + " contains ";
                }
                if (displayList) {
                    for (var i = 1; i <= numContents; i++) {
                        if (i > 1) {
                            if (i < numContents) {
                                contents = contents + ", ";
                            }
                            else {
                                contents = contents + " and ";
                            }
                        }
                        var o = this._objs[contentsIDs[i]];
                        if (o.Prefix != "") {
                            contents = contents + o.Prefix;
                        }
                        if (o.ObjectAlias != "") {
                            contents = contents + "|b" + o.ObjectAlias + "|xb";
                        }
                        else {
                            contents = contents + "|b" + o.ObjectName + "|xb";
                        }
                        if (o.Suffix != "") {
                            contents = contents + " " + o.Suffix;
                        }
                    }
                }
                return contents + ".";
            }
            // The "list" property is not set, so do not list contents.
            return "";
        }
        // Container is empty, so return "list empty" property if there is one.
        if (this.DoAction(id, "list empty", ctx, false)) {
            return "<script>";
        }
        else {
            return this.GetObjectProperty("list empty", id, false, false);
        }
    };
    LegacyGame.prototype.ObscureNumericExps = function (s) {
        // Obscures + or - next to E in Double-type variables with exponents
        //   e.g. 2.345E+20 becomes 2.345EX20
        // This stops huge numbers breaking parsing of maths functions
        var ep = 0;
        var result = s;
        var pos = 1;
        do {
            ep = InStrFrom(pos, result, "E");
            if (ep != 0) {
                result = Left(result, ep) + "X" + Mid(result, ep + 2);
                pos = ep + 2;
            }
        } while (!(ep == 0));
        return result;
    };
    LegacyGame.prototype.ProcessListInfo = function (line, id) {
        var listInfo = new TextAction();
        var propName = "";
        if (this.BeginsWith(line, "list closed <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list closed";
        }
        else if (Trim(line) == "list closed off") {
            // default for list closed is off anyway
            return;
        }
        else if (this.BeginsWith(line, "list closed")) {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = this.GetEverythingAfter(line, "list closed");
            propName = "list closed";
        }
        else if (this.BeginsWith(line, "list empty <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list empty";
        }
        else if (Trim(line) == "list empty off") {
            // default for list empty is off anyway
            return;
        }
        else if (this.BeginsWith(line, "list empty")) {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = this.GetEverythingAfter(line, "list empty");
            propName = "list empty";
        }
        else if (Trim(line) == "list off") {
            this.AddToObjectProperties("not list", id, this._nullContext);
            return;
        }
        else if (this.BeginsWith(line, "list <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list";
        }
        else if (this.BeginsWith(line, "list ")) {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = this.GetEverythingAfter(line, "list ");
            propName = "list";
        }
        if (propName != "") {
            if (listInfo.Type == TextActionType.Text) {
                this.AddToObjectProperties(propName + "=" + listInfo.Data, id, this._nullContext);
            }
            else {
                this.AddToObjectActions("<" + propName + "> " + listInfo.Data, id, this._nullContext);
            }
        }
    };
    LegacyGame.prototype.GetHTMLColour = function (colour, defaultColour) {
        // Converts a Quest foreground or background colour setting into an HTML colour
        colour = LCase(colour);
        if (colour == "" || colour == "0") {
            colour = defaultColour;
        }
        switch (colour) {
            case "white":
                return "FFFFFF";
            case "black":
                return "000000";
            case "blue":
                return "0000FF";
            case "yellow":
                return "FFFF00";
            case "red":
                return "FF0000";
            case "green":
                return "00FF00";
            default:
                return colour;
        }
    };
    LegacyGame.prototype.DestroyExit = function (exitData, ctx) {
        var fromRoom = "";
        var toRoom = "";
        var roomId = 0;
        var exitId = 0;
        var scp = InStr(exitData, ";");
        if (scp == 0) {
            this.LogASLError("No exit name specified in 'destroy exit <" + exitData + ">'");
            return;
        }
        var roomExit;
        if (this._gameAslVersion >= 410) {
            roomExit = this.FindExit(exitData);
            if (roomExit == null) {
                this.LogASLError("Can't find exit in 'destroy exit <" + exitData + ">'");
                return;
            }
            roomExit.GetParent().RemoveExit(roomExit);
        }
        else {
            fromRoom = LCase(Trim(Left(exitData, scp - 1)));
            toRoom = Trim(Mid(exitData, scp + 1));
            // Find From Room:
            var found = false;
            for (var i = 1; i <= this._numberRooms; i++) {
                if (LCase(this._rooms[i].RoomName) == fromRoom) {
                    found = true;
                    roomId = i;
                    break;
                }
            }
            if (!found) {
                this.LogASLError("No such room '" + fromRoom + "'");
                return;
            }
            found = false;
            var r = this._rooms[roomId];
            for (var i = 1; i <= r.NumberPlaces; i++) {
                if (r.Places[i].PlaceName == toRoom) {
                    exitId = i;
                    found = true;
                    break;
                }
            }
            if (found) {
                for (var i = exitId; i <= r.NumberPlaces - 1; i++) {
                    r.Places[i] = r.Places[i + 1];
                }
                if (!r.Places)
                    r.Places = [];
                r.NumberPlaces = r.NumberPlaces - 1;
            }
        }
        // Update quest.* vars and obj list
        this.ShowRoomInfo(this._currentRoom, ctx, true);
        this.UpdateObjectList(ctx);
        this.AddToChangeLog("room " + fromRoom, "destroy exit " + toRoom);
    };
    LegacyGame.prototype.DoClear = function () {
        this._player.ClearScreen();
    };
    LegacyGame.prototype.ExecuteFlag = function (line, ctx) {
        var propertyString = "";
        if (this.BeginsWith(line, "on ")) {
            propertyString = this.GetParameter(line, ctx);
        }
        else if (this.BeginsWith(line, "off ")) {
            propertyString = "not " + this.GetParameter(line, ctx);
        }
        // Game object always has ObjID 1
        this.AddToObjectProperties(propertyString, 1, ctx);
    };
    LegacyGame.prototype.ExecuteIfFlag = function (flag) {
        // Game ObjID is 1
        return this.GetObjectProperty(flag, 1, true) == "yes";
    };
    LegacyGame.prototype.ExecuteIncDec = function (line, ctx) {
        var variable;
        var change = 0;
        var param = this.GetParameter(line, ctx);
        var sc = InStr(param, ";");
        if (sc == 0) {
            change = 1;
            variable = param;
        }
        else {
            change = Val(Mid(param, sc + 1));
            variable = Trim(Left(param, sc - 1));
        }
        var value = this.GetNumericContents(variable, ctx, true);
        if (value <= -32766) {
            value = 0;
        }
        if (this.BeginsWith(line, "inc ")) {
            value = value + change;
        }
        else if (this.BeginsWith(line, "dec ")) {
            value = value - change;
        }
        var arrayIndex = this.GetArrayIndex(variable, ctx);
        this.SetNumericVariableContents(arrayIndex.Name, value, ctx, arrayIndex.Index);
    };
    LegacyGame.prototype.ExtractFile = function (file) {
        var length = 0;
        var startPos = 0;
        var extracted = false;
        var resId = 0;
        if (this._resourceFile == "") {
            return "";
        }
        // Find file in catalog
        var found = false;
        for (var i = 1; i <= this._numResources; i++) {
            if (LCase(file) == LCase(this._resources[i].ResourceName)) {
                found = true;
                startPos = this._resources[i].ResourceStart + this._resourceOffset;
                length = this._resources[i].ResourceLength;
                extracted = this._resources[i].Extracted;
                resId = i;
                break;
            }
        }
        if (!found) {
            this.LogASLError("Unable to extract '" + file + "' - not present in resources.", LogType.WarningError);
            return null;
        }
        // TODO: Extract file from CAS
        //var fileName = System.IO.Path.Combine(this._tempFolder, file);
        //System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
        if (!extracted) {
            // Extract file from cached CAS data
            var fileData = Mid(this._casFileData, startPos, length);
            // Write file to temp dir
            //System.IO.File.WriteAllText(fileName, fileData, System.Text.Encoding.GetEncoding(1252));
            this._resources[resId].Extracted = true;
        }
        //return fileName;
        return null;
    };
    LegacyGame.prototype.AddObjectAction = function (id, name, script, noUpdate) {
        if (noUpdate === void 0) { noUpdate = false; }
        // Use NoUpdate in e.g. AddToGiveInfo, otherwise ObjectActionUpdate will call
        // AddToGiveInfo again leading to a big loop
        var actionNum = 0;
        var foundExisting = false;
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == name) {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }
        if (!foundExisting) {
            o.NumberActions = o.NumberActions + 1;
            if (!o.Actions)
                o.Actions = [];
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }
        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;
        this.ObjectActionUpdate(id, name, script, noUpdate);
    };
    LegacyGame.prototype.AddToChangeLog = function (appliesTo, changeData) {
        this._gameChangeData.NumberChanges = this._gameChangeData.NumberChanges + 1;
        if (!this._gameChangeData.ChangeData)
            this._gameChangeData.ChangeData = [];
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges] = new ChangeType();
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges].AppliesTo = appliesTo;
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges].Change = changeData;
    };
    LegacyGame.prototype.AddToObjectChangeLog = function (appliesToType, appliesTo, element, changeData) {
        var changeLog;
        // NOTE: We're only actually ever using the object changelog.
        // Rooms only get logged for creating rooms and creating/destroying exits, so we don't
        // need the refactored ChangeLog component for those.
        switch (appliesToType) {
            case AppliesTo.Object:
                changeLog = this._changeLogObjects;
                break;
            case AppliesTo.Room:
                changeLog = this._changeLogRooms;
                break;
            default:
                throw "New ArgumentOutOfRangeException()";
        }
        changeLog.AddItem(appliesTo, element, changeData);
    };
    LegacyGame.prototype.AddToGiveInfo = function (id, giveData) {
        var giveType;
        var actionName;
        var actionScript;
        var o = this._objs[id];
        if (this.BeginsWith(giveData, "to ")) {
            giveData = this.GetEverythingAfter(giveData, "to ");
            if (this.BeginsWith(giveData, "anything ")) {
                o.GiveToAnything = this.GetEverythingAfter(giveData, "anything ");
                this.AddObjectAction(id, "give to anything", o.GiveToAnything, true);
                return;
            }
            else {
                giveType = GiveType.GiveToSomething;
                actionName = "give to ";
            }
        }
        else {
            if (this.BeginsWith(giveData, "anything ")) {
                o.GiveAnything = this.GetEverythingAfter(giveData, "anything ");
                this.AddObjectAction(id, "give anything", o.GiveAnything, true);
                return;
            }
            else {
                giveType = GiveType.GiveSomethingTo;
                actionName = "give ";
            }
        }
        if (Left(Trim(giveData), 1) == "<") {
            var name = this.GetParameter(giveData, this._nullContext);
            var dataId = 0;
            actionName = actionName + "'" + name + "'";
            var found = false;
            for (var i = 1; i <= o.NumberGiveData; i++) {
                if (o.GiveData[i].GiveType == giveType && LCase(o.GiveData[i].GiveObject) == LCase(name)) {
                    dataId = i;
                    found = true;
                    break;
                }
            }
            if (!found) {
                o.NumberGiveData = o.NumberGiveData + 1;
                if (!o.GiveData)
                    o.GiveData = [];
                o.GiveData[o.NumberGiveData] = new GiveDataType();
                dataId = o.NumberGiveData;
            }
            var EP = InStr(giveData, ">");
            o.GiveData[dataId].GiveType = giveType;
            o.GiveData[dataId].GiveObject = name;
            o.GiveData[dataId].GiveScript = Mid(giveData, EP + 2);
            actionScript = o.GiveData[dataId].GiveScript;
            this.AddObjectAction(id, actionName, actionScript, true);
        }
    };
    LegacyGame.prototype.AddToObjectActions = function (actionInfo, id, ctx) {
        var actionNum = 0;
        var foundExisting = false;
        var name = LCase(this.GetParameter(actionInfo, ctx));
        var ep = InStr(actionInfo, ">");
        if (ep == Len(actionInfo)) {
            this.LogASLError("No script given for '" + name + "' action data", LogType.WarningError);
            return;
        }
        var script = Trim(Mid(actionInfo, ep + 1));
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == name) {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }
        if (!foundExisting) {
            o.NumberActions = o.NumberActions + 1;
            if (!o.Actions)
                o.Actions = [];
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }
        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;
        this.ObjectActionUpdate(id, name, script);
    };
    LegacyGame.prototype.AddToObjectAltNames = function (altNames, id) {
        var o = this._objs[id];
        do {
            var endPos = InStr(altNames, ";");
            if (endPos == 0) {
                endPos = Len(altNames) + 1;
            }
            var curName = Trim(Left(altNames, endPos - 1));
            if (curName != "") {
                o.NumberAltNames = o.NumberAltNames + 1;
                if (!o.AltNames)
                    o.AltNames = [];
                o.AltNames[o.NumberAltNames] = curName;
            }
            altNames = Mid(altNames, endPos + 1);
        } while (!(Trim(altNames) == ""));
    };
    LegacyGame.prototype.AddToObjectProperties = function (propertyInfo, id, ctx) {
        if (id == 0) {
            return null;
        }
        if (Right(propertyInfo, 1) != ";") {
            propertyInfo = propertyInfo + ";";
        }
        do {
            var scp = InStr(propertyInfo, ";");
            var info = Left(propertyInfo, scp - 1);
            propertyInfo = Trim(Mid(propertyInfo, scp + 1));
            var name;
            var value;
            var num = 0;
            if (info == "") {
                break;
            }
            var ep = InStr(info, "=");
            if (ep != 0) {
                name = Trim(Left(info, ep - 1));
                value = Trim(Mid(info, ep + 1));
            }
            else {
                name = info;
                value = "";
            }
            var falseProperty = false;
            if (this.BeginsWith(name, "not ") && value == "") {
                falseProperty = true;
                name = this.GetEverythingAfter(name, "not ");
            }
            var o = this._objs[id];
            var found = false;
            for (var i = 1; i <= o.NumberProperties; i++) {
                if (LCase(o.Properties[i].PropertyName) == LCase(name)) {
                    found = true;
                    num = i;
                    i = o.NumberProperties;
                }
            }
            if (!found) {
                o.NumberProperties = o.NumberProperties + 1;
                if (!o.Properties)
                    o.Properties = [];
                o.Properties[o.NumberProperties] = new PropertyType();
                num = o.NumberProperties;
            }
            if (falseProperty) {
                o.Properties[num].PropertyName = "";
            }
            else {
                o.Properties[num].PropertyName = name;
                o.Properties[num].PropertyValue = value;
            }
            this.AddToObjectChangeLog(AppliesTo.Object, this._objs[id].ObjectName, name, "properties " + info);
            switch (name) {
                case "alias":
                    if (o.IsRoom) {
                        this._rooms[o.CorresRoomId].RoomAlias = value;
                    }
                    else {
                        o.ObjectAlias = value;
                    }
                    if (this._gameFullyLoaded) {
                        this.UpdateObjectList(ctx);
                        this.UpdateItems(ctx);
                    }
                    break;
                case "prefix":
                    if (o.IsRoom) {
                        this._rooms[o.CorresRoomId].Prefix = value;
                    }
                    else {
                        if (value != "") {
                            o.Prefix = value + " ";
                        }
                        else {
                            o.Prefix = "";
                        }
                    }
                    break;
                case "indescription":
                    if (o.IsRoom) {
                        this._rooms[o.CorresRoomId].InDescription = value;
                    }
                    break;
                case "description":
                    if (o.IsRoom) {
                        this._rooms[o.CorresRoomId].Description.Data = value;
                        this._rooms[o.CorresRoomId].Description.Type = TextActionType.Text;
                    }
                    break;
                case "look":
                    if (o.IsRoom) {
                        this._rooms[o.CorresRoomId].Look = value;
                    }
                    break;
                case "suffix":
                    o.Suffix = value;
                    break;
                case "displaytype":
                    o.DisplayType = value;
                    if (this._gameFullyLoaded) {
                        this.UpdateObjectList(ctx);
                    }
                    break;
                case "gender":
                    o.Gender = value;
                    break;
                case "article":
                    o.Article = value;
                    break;
                case "detail":
                    o.Detail = value;
                    break;
                case "hidden":
                    if (falseProperty) {
                        o.Exists = true;
                    }
                    else {
                        o.Exists = false;
                    }
                    if (this._gameFullyLoaded) {
                        this.UpdateObjectList(ctx);
                    }
                    break;
                case "invisible":
                    if (falseProperty) {
                        o.Visible = true;
                    }
                    else {
                        o.Visible = false;
                    }
                    if (this._gameFullyLoaded) {
                        this.UpdateObjectList(ctx);
                    }
                    break;
                case "take":
                    if (this._gameAslVersion >= 392) {
                        if (falseProperty) {
                            o.Take.Type = TextActionType.Nothing;
                        }
                        else {
                            if (value == "") {
                                o.Take.Type = TextActionType.Default;
                            }
                            else {
                                o.Take.Type = TextActionType.Text;
                                o.Take.Data = value;
                            }
                        }
                    }
                    break;
            }
        } while (Len(Trim(propertyInfo)) != 0);
    };
    LegacyGame.prototype.AddToUseInfo = function (id, useData) {
        var useType;
        var o = this._objs[id];
        if (this.BeginsWith(useData, "on ")) {
            useData = this.GetEverythingAfter(useData, "on ");
            if (this.BeginsWith(useData, "anything ")) {
                o.UseOnAnything = this.GetEverythingAfter(useData, "anything ");
                return;
            }
            else {
                useType = UseType.UseOnSomething;
            }
        }
        else {
            if (this.BeginsWith(useData, "anything ")) {
                o.UseAnything = this.GetEverythingAfter(useData, "anything ");
                return;
            }
            else {
                useType = UseType.UseSomethingOn;
            }
        }
        if (Left(Trim(useData), 1) == "<") {
            var objectName = this.GetParameter(useData, this._nullContext);
            var dataId = 0;
            var found = false;
            for (var i = 1; i <= o.NumberUseData; i++) {
                if (o.UseData[i].UseType == useType && LCase(o.UseData[i].UseObject) == LCase(objectName)) {
                    dataId = i;
                    found = true;
                    break;
                }
            }
            if (!found) {
                o.NumberUseData = o.NumberUseData + 1;
                if (!o.UseData)
                    o.UseData = [];
                o.UseData[o.NumberUseData] = new UseDataType();
                dataId = o.NumberUseData;
            }
            var ep = InStr(useData, ">");
            o.UseData[dataId].UseType = useType;
            o.UseData[dataId].UseObject = objectName;
            o.UseData[dataId].UseScript = Mid(useData, ep + 2);
        }
        else {
            o.Use = Trim(useData);
        }
    };
    LegacyGame.prototype.CapFirst = function (s) {
        return UCase(Left(s, 1)) + Mid(s, 2);
    };
    LegacyGame.prototype.ConvertVarsIn = function (s, ctx) {
        return this.GetParameter("<" + s + ">", ctx);
    };
    LegacyGame.prototype.DisambObjHere = function (ctx, id, firstPlace, twoPlaces, secondPlace, isExit) {
        if (twoPlaces === void 0) { twoPlaces = false; }
        if (secondPlace === void 0) { secondPlace = ""; }
        if (isExit === void 0) { isExit = false; }
        var isSeen = false;
        var onlySeen = false;
        if (firstPlace == "game") {
            firstPlace = "";
            if (secondPlace == "seen") {
                twoPlaces = false;
                secondPlace = "";
                onlySeen = true;
                var roomObjId = this._rooms[this.GetRoomId(this._objs[id].ContainerRoom, ctx)].ObjId;
                if (this._objs[id].ContainerRoom == "inventory") {
                    isSeen = true;
                }
                else {
                    if (this.IsYes(this.GetObjectProperty("visited", roomObjId, true, false))) {
                        isSeen = true;
                    }
                    else {
                        if (this.IsYes(this.GetObjectProperty("seen", id, true, false))) {
                            isSeen = true;
                        }
                    }
                }
            }
        }
        if (((!twoPlaces && (LCase(this._objs[id].ContainerRoom) == LCase(firstPlace) || firstPlace == "")) ||
            (twoPlaces && (LCase(this._objs[id].ContainerRoom) == LCase(firstPlace) || LCase(this._objs[id].ContainerRoom) == LCase(secondPlace)))) &&
            this._objs[id].Exists && this._objs[id].IsExit == isExit) {
            if (!onlySeen) {
                return true;
            }
            return isSeen;
        }
        return false;
    };
    LegacyGame.prototype.ExecClone = function (cloneString, ctx) {
        var id = 0;
        var newName;
        var cloneTo;
        var scp = InStr(cloneString, ";");
        if (scp == 0) {
            this.LogASLError("No new object name specified in 'clone <" + cloneString + ">", LogType.WarningError);
            return;
        }
        else {
            var objectToClone = Trim(Left(cloneString, scp - 1));
            id = this.GetObjectIdNoAlias(objectToClone);
            var SC2 = InStrFrom(scp + 1, cloneString, ";");
            if (SC2 == 0) {
                cloneTo = this._objs[id].ContainerRoom;
                newName = Trim(Mid(cloneString, scp + 1));
            }
            else {
                cloneTo = Trim(Mid(cloneString, SC2 + 1));
                newName = Trim(Mid(cloneString, scp + 1, (SC2 - scp) - 1));
            }
        }
        this._numberObjs = this._numberObjs + 1;
        if (!this._objs)
            this._objs = [];
        this._objs[this._numberObjs] = new ObjectType();
        this._objs[this._numberObjs] = this._objs[id];
        this._objs[this._numberObjs].ContainerRoom = cloneTo;
        this._objs[this._numberObjs].ObjectName = newName;
        if (this._objs[id].IsRoom) {
            // This is a room so create the corresponding room as well
            this._numberRooms = this._numberRooms + 1;
            if (!this._rooms)
                this._rooms = [];
            this._rooms[this._numberRooms] = new RoomType();
            this._rooms[this._numberRooms] = this._rooms[this._objs[id].CorresRoomId];
            this._rooms[this._numberRooms].RoomName = newName;
            this._rooms[this._numberRooms].ObjId = this._numberObjs;
            this._objs[this._numberObjs].CorresRoom = newName;
            this._objs[this._numberObjs].CorresRoomId = this._numberRooms;
            this.AddToChangeLog("room " + newName, "create");
        }
        else {
            this.AddToChangeLog("object " + newName, "create " + this._objs[this._numberObjs].ContainerRoom);
        }
        this.UpdateObjectList(ctx);
    };
    LegacyGame.prototype.ExecOops = function (correction, ctx) {
        if (this._badCmdBefore != "") {
            if (this._badCmdAfter == "") {
                this.ExecCommand(this._badCmdBefore + " " + correction, ctx, false);
            }
            else {
                this.ExecCommand(this._badCmdBefore + " " + correction + " " + this._badCmdAfter, ctx, false);
            }
        }
    };
    LegacyGame.prototype.ExecType = function (typeData, ctx) {
        var id = 0;
        var found = false;
        var scp = InStr(typeData, ";");
        if (scp == 0) {
            this.LogASLError("No type name given in 'type <" + typeData + ">'");
            return;
        }
        var objName = Trim(Left(typeData, scp - 1));
        var typeName = Trim(Mid(typeData, scp + 1));
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == LCase(objName)) {
                found = true;
                id = i;
                break;
            }
        }
        if (!found) {
            this.LogASLError("No such object in 'type <" + typeData + ">'");
            return;
        }
        var o = this._objs[id];
        o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
        if (!o.TypesIncluded)
            o.TypesIncluded = [];
        o.TypesIncluded[o.NumberTypesIncluded] = typeName;
        var propertyData = this.GetPropertiesInType(typeName);
        this.AddToObjectProperties(propertyData.Properties, id, ctx);
        for (var i = 1; i <= propertyData.NumberActions; i++) {
            this.AddObjectAction(id, propertyData.Actions[i].ActionName, propertyData.Actions[i].Script);
        }
        // New as of Quest 4.0. Fixes bug that "if type" would fail for any
        // parent types included by the "type" command.
        for (var i = 1; i <= propertyData.NumberTypesIncluded; i++) {
            o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
            if (!o.TypesIncluded)
                o.TypesIncluded = [];
            o.TypesIncluded[o.NumberTypesIncluded] = propertyData.TypesIncluded[i];
        }
    };
    LegacyGame.prototype.ExecuteIfAction = function (actionData) {
        var id = 0;
        var scp = InStr(actionData, ";");
        if (scp == 0) {
            this.LogASLError("No action name given in condition 'action <" + actionData + ">' ...", LogType.WarningError);
            return false;
        }
        var objName = Trim(Left(actionData, scp - 1));
        var actionName = Trim(Mid(actionData, scp + 1));
        var found = false;
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == LCase(objName)) {
                found = true;
                id = i;
                break;
            }
        }
        if (!found) {
            this.LogASLError("No such object '" + objName + "' in condition 'action <" + actionData + ">' ...", LogType.WarningError);
            return false;
        }
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (LCase(o.Actions[i].ActionName) == LCase(actionName)) {
                return true;
            }
        }
        return false;
    };
    LegacyGame.prototype.ExecuteIfType = function (typeData) {
        var id = 0;
        var scp = InStr(typeData, ";");
        if (scp == 0) {
            this.LogASLError("No type name given in condition 'type <" + typeData + ">' ...", LogType.WarningError);
            return false;
        }
        var objName = Trim(Left(typeData, scp - 1));
        var typeName = Trim(Mid(typeData, scp + 1));
        var found = false;
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == LCase(objName)) {
                found = true;
                id = i;
                break;
            }
        }
        if (!found) {
            this.LogASLError("No such object '" + objName + "' in condition 'type <" + typeData + ">' ...", LogType.WarningError);
            return false;
        }
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberTypesIncluded; i++) {
            if (LCase(o.TypesIncluded[i]) == LCase(typeName)) {
                return true;
            }
        }
        return false;
    };
    LegacyGame.prototype.GetArrayIndex = function (varName, ctx) {
        var result = new ArrayResult();
        if (InStr(varName, "[") == 0 || InStr(varName, "]") == 0) {
            result.Name = varName;
            return result;
        }
        var beginPos = InStr(varName, "[");
        var endPos = InStr(varName, "]");
        var data = Mid(varName, beginPos + 1, (endPos - beginPos) - 1);
        if (IsNumeric(data)) {
            result.Index = parseInt(data);
        }
        else {
            result.Index = this.GetNumericContents(data, ctx);
        }
        result.Name = Left(varName, beginPos - 1);
        return result;
    };
    LegacyGame.prototype.Disambiguate = function (name, containedIn, ctx, isExit) {
        if (isExit === void 0) { isExit = false; }
        // Returns object ID being referred to by player.
        // Returns -1 if object doesn't exist, calling function
        //   then expected to print relevant error.
        // Returns -2 if "it" meaningless, prints own error.
        // If it returns an object ID, it also sets quest.lastobject to the name
        // of the object referred to.
        // If ctx.AllowRealNamesInCommand is True, will allow an object's real
        // name to be used even when the object has an alias - this is used when
        // Disambiguate has been called after an "exec" command to prevent the
        // player having to choose an object from the disambiguation menu twice
        var numberCorresIds = 0;
        var idNumbers = [];
        var firstPlace;
        var secondPlace = "";
        var twoPlaces = false;
        var descriptionText;
        var validNames;
        var numValidNames = 0;
        name = Trim(name);
        this.SetStringContents("quest.lastobject", "", ctx);
        if (InStr(containedIn, ";") != 0) {
            var scp = InStr(containedIn, ";");
            twoPlaces = true;
            firstPlace = Trim(Left(containedIn, scp - 1));
            secondPlace = Trim(Mid(containedIn, scp + 1));
        }
        else {
            twoPlaces = false;
            firstPlace = containedIn;
        }
        if (ctx.AllowRealNamesInCommand) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (this.DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace)) {
                    if (LCase(this._objs[i].ObjectName) == LCase(name)) {
                        this.SetStringContents("quest.lastobject", this._objs[i].ObjectName, ctx);
                        return i;
                    }
                }
            }
        }
        // If player uses "it", "them" etc. as name:
        if (name == "it" || name == "them" || name == "this" || name == "those" || name == "these" || name == "that") {
            this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Inanimate && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            }
            else {
                this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
                return -2;
            }
        }
        else if (name == "him") {
            this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Male && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            }
            else {
                this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
                return -2;
            }
        }
        else if (name == "her") {
            this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Female && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            }
            else {
                this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
                return -2;
            }
        }
        this._thisTurnIt = 0;
        if (this.BeginsWith(name, "the ")) {
            name = this.GetEverythingAfter(name, "the ");
        }
        for (var i = 1; i <= this._numberObjs; i++) {
            if (this.DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit)) {
                numValidNames = this._objs[i].NumberAltNames + 1;
                validNames = [];
                validNames[1] = this._objs[i].ObjectAlias;
                for (var j = 1; j <= this._objs[i].NumberAltNames; j++) {
                    validNames[j + 1] = this._objs[i].AltNames[j];
                }
                for (var j = 1; j <= numValidNames; j++) {
                    if (((LCase(validNames[j]) == LCase(name)) || ("the " + LCase(name) == LCase(validNames[j])))) {
                        numberCorresIds = numberCorresIds + 1;
                        if (!idNumbers)
                            idNumbers = [];
                        idNumbers[numberCorresIds] = i;
                        j = numValidNames;
                    }
                }
            }
        }
        if (this._gameAslVersion >= 391 && numberCorresIds == 0 && this._useAbbreviations && Len(name) > 0) {
            // Check for abbreviated object names
            for (var i = 1; i <= this._numberObjs; i++) {
                if (this.DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit)) {
                    var thisName;
                    if (this._objs[i].ObjectAlias != "") {
                        thisName = LCase(this._objs[i].ObjectAlias);
                    }
                    else {
                        thisName = LCase(this._objs[i].ObjectName);
                    }
                    if (this._gameAslVersion >= 410) {
                        if (this._objs[i].Prefix != "") {
                            thisName = Trim(LCase(this._objs[i].Prefix)) + " " + thisName;
                        }
                        if (this._objs[i].Suffix != "") {
                            thisName = thisName + " " + Trim(LCase(this._objs[i].Suffix));
                        }
                    }
                    if (InStr(" " + thisName, " " + LCase(name)) != 0) {
                        numberCorresIds = numberCorresIds + 1;
                        if (!idNumbers)
                            idNumbers = [];
                        idNumbers[numberCorresIds] = i;
                    }
                }
            }
        }
        if (numberCorresIds == 1) {
            this.SetStringContents("quest.lastobject", this._objs[idNumbers[1]].ObjectName, ctx);
            this._thisTurnIt = idNumbers[1];
            switch (this._objs[idNumbers[1]].Article) {
                case "him":
                    this._thisTurnItMode = ItType.Male;
                    break;
                case "her":
                    this._thisTurnItMode = ItType.Female;
                    break;
                default:
                    this._thisTurnItMode = ItType.Inanimate;
                    break;
            }
            return idNumbers[1];
        }
        else if (numberCorresIds > 1) {
            descriptionText = [];
            var question = "Please select which " + name + " you mean:";
            this.Print("- |i" + question + "|xi", ctx);
            var menuItems = {};
            for (var i = 1; i <= numberCorresIds; i++) {
                descriptionText[i] = this._objs[idNumbers[i]].Detail;
                if (descriptionText[i] == "") {
                    if (this._objs[idNumbers[i]].Prefix == "") {
                        descriptionText[i] = this._objs[idNumbers[i]].ObjectAlias;
                    }
                    else {
                        descriptionText[i] = this._objs[idNumbers[i]].Prefix + this._objs[idNumbers[i]].ObjectAlias;
                    }
                }
                menuItems[i.toString()] = descriptionText[i];
            }
            var mnu = new MenuData(question, menuItems, false);
            var response = this.ShowMenu(mnu);
            this._choiceNumber = parseInt(response);
            this.SetStringContents("quest.lastobject", this._objs[idNumbers[this._choiceNumber]].ObjectName, ctx);
            this._thisTurnIt = idNumbers[this._choiceNumber];
            switch (this._objs[idNumbers[this._choiceNumber]].Article) {
                case "him":
                    this._thisTurnItMode = ItType.Male;
                    break;
                case "her":
                    this._thisTurnItMode = ItType.Female;
                    break;
                default:
                    this._thisTurnItMode = ItType.Inanimate;
                    break;
            }
            this.Print("- " + descriptionText[this._choiceNumber] + "|n", ctx);
            return idNumbers[this._choiceNumber];
        }
        this._thisTurnIt = this._lastIt;
        this.SetStringContents("quest.error.object", name, ctx);
        return -1;
    };
    LegacyGame.prototype.DisplayStatusVariableInfo = function (id, type, ctx) {
        var displayData = "";
        var ep = 0;
        if (type == VarType.String) {
            displayData = this.ConvertVarsIn(this._stringVariable[id].DisplayString, ctx);
            ep = InStr(displayData, "!");
            if (ep != 0) {
                displayData = Left(displayData, ep - 1) + this._stringVariable[id].VariableContents[0] + Mid(displayData, ep + 1);
            }
        }
        else if (type == VarType.Numeric) {
            if (this._numericVariable[id].NoZeroDisplay && Val(this._numericVariable[id].VariableContents[0]) == 0) {
                return "";
            }
            displayData = this.ConvertVarsIn(this._numericVariable[id].DisplayString, ctx);
            ep = InStr(displayData, "!");
            if (ep != 0) {
                displayData = Left(displayData, ep - 1) + this._numericVariable[id].VariableContents[0] + Mid(displayData, ep + 1);
            }
            if (InStr(displayData, "*") > 0) {
                var firstStar = InStr(displayData, "*");
                var secondStar = InStrFrom(firstStar + 1, displayData, "*");
                var beforeStar = Left(displayData, firstStar - 1);
                var afterStar = Mid(displayData, secondStar + 1);
                var betweenStar = Mid(displayData, firstStar + 1, (secondStar - firstStar) - 1);
                if (parseFloat(this._numericVariable[id].VariableContents[0]) != 1) {
                    displayData = beforeStar + betweenStar + afterStar;
                }
                else {
                    displayData = beforeStar + afterStar;
                }
            }
        }
        return displayData;
    };
    LegacyGame.prototype.DoAction = function (id, action, ctx, logError) {
        if (logError === void 0) { logError = true; }
        var found = false;
        var script = "";
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == LCase(action)) {
                found = true;
                script = o.Actions[i].Script;
                break;
            }
        }
        if (!found) {
            if (logError) {
                this.LogASLError("No such action '" + action + "' defined for object '" + o.ObjectName + "'");
            }
            return false;
        }
        var newCtx = this.CopyContext(ctx);
        newCtx.CallingObjectId = id;
        this.ExecuteScript(script, newCtx, id);
        return true;
    };
    LegacyGame.prototype.HasAction = function (id, action) {
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == LCase(action)) {
                return true;
            }
        }
        return false;
    };
    LegacyGame.prototype.ExecForEach = function (scriptLine, ctx) {
        var inLocation;
        var scriptToRun;
        var isExit = false;
        var isRoom = false;
        if (this.BeginsWith(scriptLine, "object ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "object ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each object " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                return;
            }
        }
        else if (this.BeginsWith(scriptLine, "exit ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "exit ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each exit " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                return;
            }
            isExit = true;
        }
        else if (this.BeginsWith(scriptLine, "room ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "room ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each room " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                return;
            }
            isRoom = true;
        }
        else {
            this.LogASLError("Unknown type in 'for each " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
            return;
        }
        scriptLine = this.GetEverythingAfter(scriptLine, "in ");
        if (this.BeginsWith(scriptLine, "game ")) {
            inLocation = "";
            scriptToRun = this.GetEverythingAfter(scriptLine, "game ");
        }
        else {
            inLocation = LCase(this.GetParameter(scriptLine, ctx));
            var bracketPos = InStr(scriptLine, ">");
            scriptToRun = Trim(Mid(scriptLine, bracketPos + 1));
        }
        for (var i = 1; i <= this._numberObjs; i++) {
            if (inLocation == "" || LCase(this._objs[i].ContainerRoom) == inLocation) {
                if (this._objs[i].IsRoom == isRoom && this._objs[i].IsExit == isExit) {
                    this.SetStringContents("quest.thing", this._objs[i].ObjectName, ctx);
                    this.ExecuteScript(scriptToRun, ctx);
                }
            }
        }
    };
    LegacyGame.prototype.ExecuteAction = function (data, ctx) {
        var actionName;
        var script;
        var actionNum = 0;
        var id = 0;
        var foundExisting = false;
        var foundObject = false;
        var param = this.GetParameter(data, ctx);
        var scp = InStr(param, ";");
        if (scp == 0) {
            this.LogASLError("No action name specified in 'action " + data + "'", LogType.WarningError);
            return;
        }
        var objName = Trim(Left(param, scp - 1));
        actionName = Trim(Mid(param, scp + 1));
        var ep = InStr(data, ">");
        if (ep == Len(Trim(data))) {
            script = "";
        }
        else {
            script = Trim(Mid(data, ep + 1));
        }
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == LCase(objName)) {
                foundObject = true;
                id = i;
                break;
            }
        }
        if (!foundObject) {
            this.LogASLError("No such object '" + objName + "' in 'action " + data + "'", LogType.WarningError);
            return;
        }
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == actionName) {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }
        if (!foundExisting) {
            o.NumberActions = o.NumberActions + 1;
            if (!o.Actions)
                o.Actions = [];
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }
        o.Actions[actionNum].ActionName = actionName;
        o.Actions[actionNum].Script = script;
        this.ObjectActionUpdate(id, actionName, script);
    };
    LegacyGame.prototype.ExecuteCondition = function (condition, ctx) {
        var result = false;
        var thisNot = false;
        if (this.BeginsWith(condition, "not ")) {
            thisNot = true;
            condition = this.GetEverythingAfter(condition, "not ");
        }
        else {
            thisNot = false;
        }
        if (this.BeginsWith(condition, "got ")) {
            result = this.ExecuteIfGot(this.GetParameter(condition, ctx));
        }
        else if (this.BeginsWith(condition, "has ")) {
            result = this.ExecuteIfHas(this.GetParameter(condition, ctx));
        }
        else if (this.BeginsWith(condition, "ask ")) {
            result = this.ExecuteIfAsk(this.GetParameter(condition, ctx));
        }
        else if (this.BeginsWith(condition, "is ")) {
            result = this.ExecuteIfIs(this.GetParameter(condition, ctx));
        }
        else if (this.BeginsWith(condition, "here ")) {
            result = this.ExecuteIfHere(this.GetParameter(condition, ctx), ctx);
        }
        else if (this.BeginsWith(condition, "exists ")) {
            result = this.ExecuteIfExists(this.GetParameter(condition, ctx), false);
        }
        else if (this.BeginsWith(condition, "real ")) {
            result = this.ExecuteIfExists(this.GetParameter(condition, ctx), true);
        }
        else if (this.BeginsWith(condition, "property ")) {
            result = this.ExecuteIfProperty(this.GetParameter(condition, ctx));
        }
        else if (this.BeginsWith(condition, "action ")) {
            result = this.ExecuteIfAction(this.GetParameter(condition, ctx));
        }
        else if (this.BeginsWith(condition, "type ")) {
            result = this.ExecuteIfType(this.GetParameter(condition, ctx));
        }
        else if (this.BeginsWith(condition, "flag ")) {
            result = this.ExecuteIfFlag(this.GetParameter(condition, ctx));
        }
        if (thisNot) {
            result = !result;
        }
        return result;
    };
    LegacyGame.prototype.ExecuteConditions = function (list, ctx) {
        var conditions;
        var numConditions = 0;
        var operations;
        var obscuredConditionList = this.ObliterateParameters(list);
        var pos = 1;
        var isFinalCondition = false;
        do {
            numConditions = numConditions + 1;
            if (!conditions)
                conditions = [];
            if (!operations)
                operations = [];
            var nextCondition = "AND";
            var nextConditionPos = InStrFrom(pos, obscuredConditionList, "and ");
            if (nextConditionPos == 0) {
                nextConditionPos = InStrFrom(pos, obscuredConditionList, "or ");
                nextCondition = "OR";
            }
            if (nextConditionPos == 0) {
                nextConditionPos = Len(obscuredConditionList) + 2;
                isFinalCondition = true;
                nextCondition = "FINAL";
            }
            var thisCondition = Trim(Mid(list, pos, nextConditionPos - pos - 1));
            conditions[numConditions] = thisCondition;
            operations[numConditions] = nextCondition;
            // next condition starts from space after and/or
            pos = InStrFrom(nextConditionPos, obscuredConditionList, " ");
        } while (!(isFinalCondition));
        operations[0] = "AND";
        var result = true;
        for (var i = 1; i <= numConditions; i++) {
            var thisResult = this.ExecuteCondition(conditions[i], ctx);
            if (operations[i - 1] == "AND") {
                result = thisResult && result;
            }
            else if (operations[i - 1] == "OR") {
                result = thisResult || result;
            }
        }
        return result;
    };
    LegacyGame.prototype.ExecuteCreate = function (data, ctx) {
        var newName;
        if (this.BeginsWith(data, "room ")) {
            newName = this.GetParameter(data, ctx);
            this._numberRooms = this._numberRooms + 1;
            if (!this._rooms)
                this._rooms = [];
            this._rooms[this._numberRooms] = new RoomType();
            this._rooms[this._numberRooms].RoomName = newName;
            this._numberObjs = this._numberObjs + 1;
            if (!this._objs)
                this._objs = [];
            this._objs[this._numberObjs] = new ObjectType();
            this._objs[this._numberObjs].ObjectName = newName;
            this._objs[this._numberObjs].IsRoom = true;
            this._objs[this._numberObjs].CorresRoom = newName;
            this._objs[this._numberObjs].CorresRoomId = this._numberRooms;
            this._rooms[this._numberRooms].ObjId = this._numberObjs;
            this.AddToChangeLog("room " + newName, "create");
            if (this._gameAslVersion >= 410) {
                this.AddToObjectProperties(this._defaultRoomProperties.Properties, this._numberObjs, ctx);
                for (var j = 1; j <= this._defaultRoomProperties.NumberActions; j++) {
                    this.AddObjectAction(this._numberObjs, this._defaultRoomProperties.Actions[j].ActionName, this._defaultRoomProperties.Actions[j].Script);
                }
                this._rooms[this._numberRooms].Exits = new RoomExits(this);
                this._rooms[this._numberRooms].Exits.SetObjId(this._rooms[this._numberRooms].ObjId);
            }
        }
        else if (this.BeginsWith(data, "object ")) {
            var paramData = this.GetParameter(data, ctx);
            var scp = InStr(paramData, ";");
            var containerRoom;
            if (scp == 0) {
                newName = paramData;
                containerRoom = "";
            }
            else {
                newName = Trim(Left(paramData, scp - 1));
                containerRoom = Trim(Mid(paramData, scp + 1));
            }
            this._numberObjs = this._numberObjs + 1;
            if (!this._objs)
                this._objs = [];
            this._objs[this._numberObjs] = new ObjectType();
            var o = this._objs[this._numberObjs];
            o.ObjectName = newName;
            o.ObjectAlias = newName;
            o.ContainerRoom = containerRoom;
            o.Exists = true;
            o.Visible = true;
            o.Gender = "it";
            o.Article = "it";
            this.AddToChangeLog("object " + newName, "create " + this._objs[this._numberObjs].ContainerRoom);
            if (this._gameAslVersion >= 410) {
                this.AddToObjectProperties(this._defaultProperties.Properties, this._numberObjs, ctx);
                for (var j = 1; j <= this._defaultProperties.NumberActions; j++) {
                    this.AddObjectAction(this._numberObjs, this._defaultProperties.Actions[j].ActionName, this._defaultProperties.Actions[j].Script);
                }
            }
            if (!this._gameLoading) {
                this.UpdateObjectList(ctx);
            }
        }
        else if (this.BeginsWith(data, "exit ")) {
            this.ExecuteCreateExit(data, ctx);
        }
    };
    LegacyGame.prototype.ExecuteCreateExit = function (data, ctx) {
        var scrRoom;
        var destRoom = "";
        var destId = 0;
        var exitData = this.GetEverythingAfter(data, "exit ");
        var newName = this.GetParameter(data, ctx);
        var scp = InStr(newName, ";");
        if (this._gameAslVersion < 410) {
            if (scp == 0) {
                this.LogASLError("No exit destination given in 'create exit " + exitData + "'", LogType.WarningError);
                return;
            }
        }
        if (scp == 0) {
            scrRoom = Trim(newName);
        }
        else {
            scrRoom = Trim(Left(newName, scp - 1));
        }
        var srcId = this.GetRoomId(scrRoom, ctx);
        if (srcId == 0) {
            this.LogASLError("No such room '" + scrRoom + "'", LogType.WarningError);
            return;
        }
        if (this._gameAslVersion < 410) {
            // only do destination room check for ASL <410, as can now have scripts on dynamically
            // created exits, so the destination doesn't necessarily have to exist.
            destRoom = Trim(Mid(newName, scp + 1));
            if (destRoom != "") {
                destId = this.GetRoomId(destRoom, ctx);
                if (destId == 0) {
                    this.LogASLError("No such room '" + destRoom + "'", LogType.WarningError);
                    return;
                }
            }
        }
        // If it's a "go to" exit, check if it already exists:
        var exists = false;
        if (this.BeginsWith(exitData, "<")) {
            if (this._gameAslVersion >= 410) {
                exists = !!this._rooms[srcId].Exits.GetPlaces()[destRoom];
            }
            else {
                for (var i = 1; i <= this._rooms[srcId].NumberPlaces; i++) {
                    if (LCase(this._rooms[srcId].Places[i].PlaceName) == LCase(destRoom)) {
                        exists = true;
                        break;
                    }
                }
            }
            if (exists) {
                this.LogASLError("Exit from '" + scrRoom + "' to '" + destRoom + "' already exists", LogType.WarningError);
                return;
            }
        }
        var paramPos = InStr(exitData, "<");
        var saveData;
        if (paramPos == 0) {
            saveData = exitData;
        }
        else {
            saveData = Left(exitData, paramPos - 1);
            // We do this so the changelog doesn't contain unconverted variable names
            saveData = saveData + "<" + this.GetParameter(exitData, ctx) + ">";
        }
        this.AddToChangeLog("room " + this._rooms[srcId].RoomName, "exit " + saveData);
        var r = this._rooms[srcId];
        if (this._gameAslVersion >= 410) {
            r.Exits.AddExitFromCreateScript(exitData, ctx);
        }
        else {
            if (this.BeginsWith(exitData, "north ")) {
                r.North.Data = destRoom;
                r.North.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "south ")) {
                r.South.Data = destRoom;
                r.South.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "east ")) {
                r.East.Data = destRoom;
                r.East.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "west ")) {
                r.West.Data = destRoom;
                r.West.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "northeast ")) {
                r.NorthEast.Data = destRoom;
                r.NorthEast.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "northwest ")) {
                r.NorthWest.Data = destRoom;
                r.NorthWest.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "southeast ")) {
                r.SouthEast.Data = destRoom;
                r.SouthEast.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "southwest ")) {
                r.SouthWest.Data = destRoom;
                r.SouthWest.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "up ")) {
                r.Up.Data = destRoom;
                r.Up.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "down ")) {
                r.Down.Data = destRoom;
                r.Down.Type = TextActionType.Text;
            }
            else if (this.BeginsWith(exitData, "out ")) {
                r.Out.Text = destRoom;
            }
            else if (this.BeginsWith(exitData, "<")) {
                r.NumberPlaces = r.NumberPlaces + 1;
                if (!r.Places)
                    r.Places = [];
                r.Places[r.NumberPlaces] = new PlaceType();
                r.Places[r.NumberPlaces].PlaceName = destRoom;
            }
            else {
                this.LogASLError("Invalid direction in 'create exit " + exitData + "'", LogType.WarningError);
            }
        }
        if (!this._gameLoading) {
            // Update quest.doorways variables
            this.ShowRoomInfo(this._currentRoom, ctx, true);
            this.UpdateObjectList(ctx);
            if (this._gameAslVersion < 410) {
                if (this._currentRoom == this._rooms[srcId].RoomName) {
                    this.UpdateDoorways(srcId, ctx);
                }
                else if (this._currentRoom == this._rooms[destId].RoomName) {
                    this.UpdateDoorways(destId, ctx);
                }
            }
            else {
                // Don't have DestID in ASL410 CreateExit code, so just UpdateDoorways
                // for current room anyway.
                this.UpdateDoorways(this.GetRoomId(this._currentRoom, ctx), ctx);
            }
        }
    };
    LegacyGame.prototype.ExecDrop = function (obj, ctx) {
        var found = false;
        var parentId = 0;
        var id = 0;
        id = this.Disambiguate(obj, "inventory", ctx);
        if (id > 0) {
            found = true;
        }
        else {
            found = false;
        }
        if (!found) {
            if (id != -2) {
                if (this._gameAslVersion >= 391) {
                    this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                }
                else {
                    this.PlayerErrorMessage(PlayerError.BadDrop, ctx);
                }
            }
            this._badCmdBefore = "drop";
            return;
        }
        // If object is inside a container, it must be removed before it can be dropped.
        var isInContainer = false;
        if (this._gameAslVersion >= 391) {
            if (this.IsYes(this.GetObjectProperty("parent", id, true, false))) {
                isInContainer = true;
                var parent = this.GetObjectProperty("parent", id, false, false);
                parentId = this.GetObjectIdNoAlias(parent);
            }
        }
        var dropFound = false;
        var dropStatement = "";
        for (var i = this._objs[id].DefinitionSectionStart; i <= this._objs[id].DefinitionSectionEnd; i++) {
            if (this.BeginsWith(this._lines[i], "drop ")) {
                dropStatement = this.GetEverythingAfter(this._lines[i], "drop ");
                dropFound = true;
                break;
            }
        }
        this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
        if (!dropFound || this.BeginsWith(dropStatement, "everywhere")) {
            if (isInContainer) {
                // So, we want to drop an object that's in a container or surface. So first
                // we have to remove the object from that container.
                var parentDisplayName;
                if (this._objs[parentId].ObjectAlias != "") {
                    parentDisplayName = this._objs[parentId].ObjectAlias;
                }
                else {
                    parentDisplayName = this._objs[parentId].ObjectName;
                }
                this.Print("(first removing " + this._objs[id].Article + " from " + parentDisplayName + ")", ctx);
                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                this.ExecCommand("remove " + this._objs[id].ObjectName, ctx, false, null, true);
                if (this.GetObjectProperty("parent", id, false, false) != "") {
                    // removing the object failed
                    return;
                }
            }
        }
        if (!dropFound) {
            this.PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
            this.PlayerItem(this._objs[id].ObjectName, false, ctx);
        }
        else {
            if (this.BeginsWith(dropStatement, "everywhere")) {
                this.PlayerItem(this._objs[id].ObjectName, false, ctx);
                if (InStr(dropStatement, "<") != 0) {
                    this.Print(this.GetParameter(dropStatement, ctx), ctx);
                }
                else {
                    this.PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
                }
            }
            else if (this.BeginsWith(dropStatement, "nowhere")) {
                if (InStr(dropStatement, "<") != 0) {
                    this.Print(this.GetParameter(dropStatement, ctx), ctx);
                }
                else {
                    this.PlayerErrorMessage(PlayerError.CantDrop, ctx);
                }
            }
            else {
                this.ExecuteScript(dropStatement, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecExamine = function (command, ctx) {
        var item = LCase(Trim(this.GetEverythingAfter(command, "examine ")));
        if (item == "") {
            this.PlayerErrorMessage(PlayerError.BadExamine, ctx);
            this._badCmdBefore = "examine";
            return;
        }
        var id = this.Disambiguate(item, this._currentRoom + ";inventory", ctx);
        if (id <= 0) {
            if (id != -2) {
                this.PlayerErrorMessage(PlayerError.BadThing, ctx);
            }
            this._badCmdBefore = "examine";
            return;
        }
        var o = this._objs[id];
        // Find "examine" action:
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == "examine") {
                this.ExecuteScript(o.Actions[i].Script, ctx, id);
                return;
            }
        }
        // Find "examine" property:
        for (var i = 1; i <= o.NumberProperties; i++) {
            if (o.Properties[i].PropertyName == "examine") {
                this.Print(o.Properties[i].PropertyValue, ctx);
                return;
            }
        }
        // Find "examine" tag:
        for (var i = o.DefinitionSectionStart + 1; i <= this._objs[id].DefinitionSectionEnd - 1; i++) {
            if (this.BeginsWith(this._lines[i], "examine ")) {
                var action = Trim(this.GetEverythingAfter(this._lines[i], "examine "));
                if (Left(action, 1) == "<") {
                    this.Print(this.GetParameter(this._lines[i], ctx), ctx);
                }
                else {
                    this.ExecuteScript(action, ctx, id);
                }
                return;
            }
        }
        this.DoLook(id, ctx, true);
    };
    LegacyGame.prototype.ExecMoveThing = function (data, type, ctx) {
        var scp = InStr(data, ";");
        var name = Trim(Left(data, scp - 1));
        var place = Trim(Mid(data, scp + 1));
        this.MoveThing(name, place, type, ctx);
    };
    LegacyGame.prototype.ExecProperty = function (data, ctx) {
        var id = 0;
        var found = false;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No property data given in 'property <" + data + ">'", LogType.WarningError);
            return;
        }
        var name = Trim(Left(data, scp - 1));
        var properties = Trim(Mid(data, scp + 1));
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == LCase(name)) {
                found = true;
                id = i;
                break;
            }
        }
        if (!found) {
            this.LogASLError("No such object in 'property <" + data + ">'", LogType.WarningError);
            return;
        }
        this.AddToObjectProperties(properties, id, ctx);
    };
    LegacyGame.prototype.ExecuteDo = function (procedureName, ctx) {
        var newCtx = this.CopyContext(ctx);
        var numParameters = 0;
        var useNewCtx = false;
        if (this._gameAslVersion >= 392 && Left(procedureName, 8) == "!intproc") {
            // If "do" procedure is run in a new context, context info is not passed to any nested
            // script blocks in braces.
            useNewCtx = false;
        }
        else {
            useNewCtx = true;
        }
        if (this._gameAslVersion >= 284) {
            var obp = InStr(procedureName, "(");
            var cbp = 0;
            if (obp != 0) {
                cbp = InStrFrom(obp + 1, procedureName, ")");
            }
            if (obp != 0 && cbp != 0) {
                var parameters = Mid(procedureName, obp + 1, (cbp - obp) - 1);
                procedureName = Left(procedureName, obp - 1);
                parameters = parameters + ";";
                var pos = 1;
                do {
                    numParameters = numParameters + 1;
                    var scp = InStrFrom(pos, parameters, ";");
                    newCtx.NumParameters = numParameters;
                    if (!newCtx.Parameters)
                        newCtx.Parameters = [];
                    newCtx.Parameters[numParameters] = Trim(Mid(parameters, pos, scp - pos));
                    pos = scp + 1;
                } while (!(pos >= Len(parameters)));
            }
        }
        var block = this.DefineBlockParam("procedure", procedureName);
        if (block.StartLine == 0 && block.EndLine == 0) {
            this.LogASLError("No such procedure " + procedureName, LogType.WarningError);
        }
        else {
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                if (!useNewCtx) {
                    this.ExecuteScript(this._lines[i], ctx);
                }
                else {
                    this.ExecuteScript(this._lines[i], newCtx);
                    ctx.DontProcessCommand = newCtx.DontProcessCommand;
                }
            }
        }
    };
    LegacyGame.prototype.ExecuteDoAction = function (data, ctx) {
        var id = 0;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No action name specified in 'doaction <" + data + ">'");
            return;
        }
        var objName = LCase(Trim(Left(data, scp - 1)));
        var actionName = Trim(Mid(data, scp + 1));
        var found = false;
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == objName) {
                found = true;
                id = i;
                break;
            }
        }
        if (!found) {
            this.LogASLError("No such object '" + objName + "'");
            return;
        }
        this.DoAction(id, actionName, ctx);
    };
    LegacyGame.prototype.ExecuteIfHere = function (obj, ctx) {
        if (this._gameAslVersion <= 281) {
            for (var i = 1; i <= this._numberChars; i++) {
                if (this._chars[i].ContainerRoom == this._currentRoom && this._chars[i].Exists) {
                    if (LCase(obj) == LCase(this._chars[i].ObjectName)) {
                        return true;
                    }
                }
            }
        }
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ContainerRoom) == LCase(this._currentRoom) && this._objs[i].Exists) {
                if (LCase(obj) == LCase(this._objs[i].ObjectName)) {
                    return true;
                }
            }
        }
        return false;
    };
    LegacyGame.prototype.ExecuteIfExists = function (obj, realOnly) {
        var result = false;
        var errorReport = false;
        var scp = 0;
        if (InStr(obj, ";") != 0) {
            scp = InStr(obj, ";");
            if (LCase(Trim(Mid(obj, scp + 1))) == "report") {
                errorReport = true;
            }
            obj = Left(obj, scp - 1);
        }
        var found = false;
        if (this._gameAslVersion < 281) {
            for (var i = 1; i <= this._numberChars; i++) {
                if (LCase(obj) == LCase(this._chars[i].ObjectName)) {
                    result = this._chars[i].Exists;
                    found = true;
                    break;
                }
            }
        }
        if (!found) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(obj) == LCase(this._objs[i].ObjectName)) {
                    result = this._objs[i].Exists;
                    found = true;
                    break;
                }
            }
        }
        if (!found && errorReport) {
            this.LogASLError("No such character/object '" + obj + "'.", LogType.UserError);
        }
        if (!found) {
            result = false;
        }
        if (realOnly) {
            return found;
        }
        return result;
    };
    LegacyGame.prototype.ExecuteIfProperty = function (data) {
        var id = 0;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No property name given in condition 'property <" + data + ">' ...", LogType.WarningError);
            return false;
        }
        var objName = Trim(Left(data, scp - 1));
        var propertyName = Trim(Mid(data, scp + 1));
        var found = false;
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == LCase(objName)) {
                found = true;
                id = i;
                break;
            }
        }
        if (!found) {
            this.LogASLError("No such object '" + objName + "' in condition 'property <" + data + ">' ...", LogType.WarningError);
            return false;
        }
        return this.GetObjectProperty(propertyName, id, true) == "yes";
    };
    LegacyGame.prototype.ExecuteRepeat = function (data, ctx) {
        var repeatWhileTrue = false;
        var repeatScript = "";
        var bracketPos = 0;
        var afterBracket;
        var foundScript = false;
        if (this.BeginsWith(data, "while ")) {
            repeatWhileTrue = true;
            data = this.GetEverythingAfter(data, "while ");
        }
        else if (this.BeginsWith(data, "until ")) {
            repeatWhileTrue = false;
            data = this.GetEverythingAfter(data, "until ");
        }
        else {
            this.LogASLError("Expected 'until' or 'while' in 'repeat " + this.ReportErrorLine(data) + "'", LogType.WarningError);
            return;
        }
        var pos = 1;
        do {
            bracketPos = InStrFrom(pos, data, ">");
            afterBracket = Trim(Mid(data, bracketPos + 1));
            if ((!this.BeginsWith(afterBracket, "and ")) && (!this.BeginsWith(afterBracket, "or "))) {
                repeatScript = afterBracket;
                foundScript = true;
            }
            else {
                pos = bracketPos + 1;
            }
        } while (!(foundScript || afterBracket == ""));
        var conditions = Trim(Left(data, bracketPos));
        var finished = false;
        do {
            if (this.ExecuteConditions(conditions, ctx) == repeatWhileTrue) {
                this.ExecuteScript(repeatScript, ctx);
            }
            else {
                finished = true;
            }
        } while (!(finished || this._gameFinished));
    };
    LegacyGame.prototype.ExecuteSetCollectable = function (param, ctx) {
        var val = 0;
        var id = 0;
        var scp = InStr(param, ";");
        var name = Trim(Left(param, scp - 1));
        var newVal = Trim(Right(param, Len(param) - scp));
        var found = false;
        for (var i = 1; i <= this._numCollectables; i++) {
            if (this._collectables[i].Name == name) {
                id = i;
                found = true;
                break;
            }
        }
        if (!found) {
            this.LogASLError("No such collectable '" + param + "'", LogType.WarningError);
            return;
        }
        var op = Left(newVal, 1);
        var newValue = Trim(Right(newVal, Len(newVal) - 1));
        if (IsNumeric(newValue)) {
            val = Val(newValue);
        }
        else {
            val = this.GetCollectableAmount(newValue);
        }
        if (op == "+") {
            this._collectables[id].Value = this._collectables[id].Value + val;
        }
        else if (op == "-") {
            this._collectables[id].Value = this._collectables[id].Value - val;
        }
        else if (op == "=") {
            this._collectables[id].Value = val;
        }
        this.CheckCollectable(id);
        this.UpdateItems(ctx);
    };
    LegacyGame.prototype.ExecuteWait = function (waitLine, ctx) {
        if (waitLine != "") {
            this.Print(this.GetParameter(waitLine, ctx), ctx);
        }
        else {
            if (this._gameAslVersion >= 410) {
                this.PlayerErrorMessage(PlayerError.DefaultWait, ctx);
            }
            else {
                this.Print("|nPress a key to continue...", ctx);
            }
        }
        this.DoWait();
    };
    LegacyGame.prototype.InitFileData = function (fileData) {
        this._fileData = fileData;
        this._fileDataPos = 1;
    };
    LegacyGame.prototype.GetNextChunk = function () {
        var nullPos = InStrFrom(this._fileDataPos, this._fileData, Chr(0));
        var result = Mid(this._fileData, this._fileDataPos, nullPos - this._fileDataPos);
        if (nullPos < Len(this._fileData)) {
            this._fileDataPos = nullPos + 1;
        }
        return result;
    };
    LegacyGame.prototype.GetFileDataChars = function (count) {
        var result = Mid(this._fileData, this._fileDataPos, count);
        this._fileDataPos = this._fileDataPos + count;
        return result;
    };
    LegacyGame.prototype.GetObjectActions = function (actionInfo) {
        var name = LCase(this.GetParameter(actionInfo, this._nullContext));
        var ep = InStr(actionInfo, ">");
        if (ep == Len(actionInfo)) {
            this.LogASLError("No script given for '" + name + "' action data", LogType.WarningError);
            return new ActionType();
        }
        var script = Trim(Mid(actionInfo, ep + 1));
        var result = new ActionType();
        result.ActionName = name;
        result.Script = script;
        return result;
    };
    LegacyGame.prototype.GetObjectId = function (name, ctx, room) {
        if (room === void 0) { room = ""; }
        var id = 0;
        var found = false;
        if (this.BeginsWith(name, "the ")) {
            name = this.GetEverythingAfter(name, "the ");
        }
        for (var i = 1; i <= this._numberObjs; i++) {
            if ((LCase(this._objs[i].ObjectName) == LCase(name) || LCase(this._objs[i].ObjectName) == "the " + LCase(name)) && (LCase(this._objs[i].ContainerRoom) == LCase(room) || room == "") && this._objs[i].Exists) {
                id = i;
                found = true;
                break;
            }
        }
        if (!found && this._gameAslVersion >= 280) {
            id = this.Disambiguate(name, room, ctx);
            if (id > 0) {
                found = true;
            }
        }
        if (found) {
            return id;
        }
        return -1;
    };
    LegacyGame.prototype.GetObjectIdNoAlias = function (name) {
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == LCase(name)) {
                return i;
            }
        }
        return 0;
    };
    LegacyGame.prototype.GetObjectProperty = function (name, id, existsOnly, logError) {
        if (existsOnly === void 0) { existsOnly = false; }
        if (logError === void 0) { logError = true; }
        var result = "";
        var found = false;
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberProperties; i++) {
            if (LCase(o.Properties[i].PropertyName) == LCase(name)) {
                found = true;
                result = o.Properties[i].PropertyValue;
                break;
            }
        }
        if (existsOnly) {
            if (found) {
                return "yes";
            }
            return "no";
        }
        if (found) {
            return result;
        }
        if (logError) {
            this.LogASLError("Object '" + this._objs[id].ObjectName + "' has no property '" + name + "'", LogType.WarningError);
            return "!";
        }
        return "";
    };
    LegacyGame.prototype.GetPropertiesInType = function (type, err) {
        if (err === void 0) { err = true; }
        var blockId = 0;
        var propertyList = new PropertiesActions();
        var found = false;
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define type")) {
                if (LCase(this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext)) == LCase(type)) {
                    blockId = i;
                    found = true;
                    break;
                }
            }
        }
        if (!found) {
            if (err) {
                this.LogASLError("No such type '" + type + "'", LogType.WarningError);
            }
            return new PropertiesActions();
        }
        for (var i = this._defineBlocks[blockId].StartLine + 1; i <= this._defineBlocks[blockId].EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "type ")) {
                var typeName = LCase(this.GetParameter(this._lines[i], this._nullContext));
                var newProperties = this.GetPropertiesInType(typeName);
                propertyList.Properties = propertyList.Properties + newProperties.Properties;
                if (!propertyList.Actions)
                    propertyList.Actions = [];
                for (var j = propertyList.NumberActions + 1; j <= propertyList.NumberActions + newProperties.NumberActions; j++) {
                    propertyList.Actions[j] = new ActionType();
                    propertyList.Actions[j].ActionName = newProperties.Actions[j - propertyList.NumberActions].ActionName;
                    propertyList.Actions[j].Script = newProperties.Actions[j - propertyList.NumberActions].Script;
                }
                propertyList.NumberActions = propertyList.NumberActions + newProperties.NumberActions;
                // Add this type name to the TypesIncluded list...
                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + 1;
                if (!propertyList.TypesIncluded)
                    propertyList.TypesIncluded = [];
                propertyList.TypesIncluded[propertyList.NumberTypesIncluded] = typeName;
                // and add the names of the types included by it...
                if (!propertyList.TypesIncluded)
                    propertyList.TypesIncluded = [];
                for (var j = propertyList.NumberTypesIncluded + 1; j <= propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded; j++) {
                    propertyList.TypesIncluded[j] = newProperties.TypesIncluded[j - propertyList.NumberTypesIncluded];
                }
                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded;
            }
            else if (this.BeginsWith(this._lines[i], "action ")) {
                propertyList.NumberActions = propertyList.NumberActions + 1;
                if (!propertyList.Actions)
                    propertyList.Actions = [];
                propertyList.Actions[propertyList.NumberActions] = this.GetObjectActions(this.GetEverythingAfter(this._lines[i], "action "));
            }
            else if (this.BeginsWith(this._lines[i], "properties ")) {
                propertyList.Properties = propertyList.Properties + this.GetParameter(this._lines[i], this._nullContext) + ";";
            }
            else if (Trim(this._lines[i]) != "") {
                propertyList.Properties = propertyList.Properties + this._lines[i] + ";";
            }
        }
        return propertyList;
    };
    LegacyGame.prototype.GetRoomId = function (name, ctx) {
        if (InStr(name, "[") > 0) {
            var idx = this.GetArrayIndex(name, ctx);
            name = name + Trim(Str(idx.Index));
        }
        for (var i = 1; i <= this._numberRooms; i++) {
            if (LCase(this._rooms[i].RoomName) == LCase(name)) {
                return i;
            }
        }
        return 0;
    };
    LegacyGame.prototype.GetTextOrScript = function (textScript) {
        var result = new TextAction();
        textScript = Trim(textScript);
        if (Left(textScript, 1) == "<") {
            result.Type = TextActionType.Text;
            result.Data = this.GetParameter(textScript, this._nullContext);
        }
        else {
            result.Type = TextActionType.Script;
            result.Data = textScript;
        }
        return result;
    };
    LegacyGame.prototype.GetThingNumber = function (name, room, type) {
        // Returns the number in the Chars() or _objs() array of the specified char/obj
        if (type == Thing.Character) {
            for (var i = 1; i <= this._numberChars; i++) {
                if ((room != "" && LCase(this._chars[i].ObjectName) == LCase(name) && LCase(this._chars[i].ContainerRoom) == LCase(room)) || (room == "" && LCase(this._chars[i].ObjectName) == LCase(name))) {
                    return i;
                }
            }
        }
        else if (type == Thing.Object) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if ((room != "" && LCase(this._objs[i].ObjectName) == LCase(name) && LCase(this._objs[i].ContainerRoom) == LCase(room)) || (room == "" && LCase(this._objs[i].ObjectName) == LCase(name))) {
                    return i;
                }
            }
        }
        return -1;
    };
    LegacyGame.prototype.GetThingBlock = function (name, room, type) {
        // Returns position where specified char/obj is defined in ASL code
        var result = new DefineBlock();
        if (type == Thing.Character) {
            for (var i = 1; i <= this._numberChars; i++) {
                if (LCase(this._chars[i].ObjectName) == LCase(name) && LCase(this._chars[i].ContainerRoom) == LCase(room)) {
                    result.StartLine = this._chars[i].DefinitionSectionStart;
                    result.EndLine = this._chars[i].DefinitionSectionEnd;
                    return result;
                }
            }
        }
        else if (type == Thing.Object) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(name) && LCase(this._objs[i].ContainerRoom) == LCase(room)) {
                    result.StartLine = this._objs[i].DefinitionSectionStart;
                    result.EndLine = this._objs[i].DefinitionSectionEnd;
                    return result;
                }
            }
        }
        result.StartLine = 0;
        result.EndLine = 0;
        return result;
    };
    LegacyGame.prototype.MakeRestoreData = function () {
        var data = [];
        var objectData = [];
        var roomData = [];
        var numObjectData = 0;
        var numRoomData = 0;
        // <<< FILE HEADER DATA >>>
        var header = "QUEST300" + Chr(0) + this.GetOriginalFilenameForQSG() + Chr(0);
        data.push(header);
        // The start point for encrypted data is after the filename
        var start = header.length + 1;
        data.push(this._currentRoom + Chr(0));
        // Organise Change Log
        for (var i = 1; i <= this._gameChangeData.NumberChanges; i++) {
            if (this.BeginsWith(this._gameChangeData.ChangeData[i].AppliesTo, "object ")) {
                numObjectData = numObjectData + 1;
                if (!objectData)
                    objectData = [];
                objectData[numObjectData] = new ChangeType();
                objectData[numObjectData] = this._gameChangeData.ChangeData[i];
            }
            else if (this.BeginsWith(this._gameChangeData.ChangeData[i].AppliesTo, "room ")) {
                numRoomData = numRoomData + 1;
                if (!roomData)
                    roomData = [];
                roomData[numRoomData] = new ChangeType();
                roomData[numRoomData] = this._gameChangeData.ChangeData[i];
            }
        }
        // <<< OBJECT CREATE/CHANGE DATA >>>
        data.push(Trim(Str(numObjectData + Object.keys(this._changeLogObjects.Changes).length)) + Chr(0));
        for (var i = 1; i <= numObjectData; i++) {
            data.push(this.GetEverythingAfter(objectData[i].AppliesTo, "object ") + Chr(0) + objectData[i].Change + Chr(0));
        }
        Object.keys(this._changeLogObjects.Changes).forEach(function (key) {
            var appliesTo = Split(key, "#")[0];
            var changeData = this._changeLogObjects.Changes[key];
            data.push(appliesTo + Chr(0) + changeData + Chr(0));
        }, this);
        // <<< OBJECT EXIST/VISIBLE/ROOM DATA >>>
        data.push(Trim(Str(this._numberObjs)) + Chr(0));
        for (var i = 1; i <= this._numberObjs; i++) {
            var exists;
            var visible;
            if (this._objs[i].Exists) {
                exists = Chr(1);
            }
            else {
                exists = Chr(0);
            }
            if (this._objs[i].Visible) {
                visible = Chr(1);
            }
            else {
                visible = Chr(0);
            }
            data.push(this._objs[i].ObjectName + Chr(0) + exists + visible + this._objs[i].ContainerRoom + Chr(0));
        }
        // <<< ROOM CREATE/CHANGE DATA >>>
        data.push(Trim(Str(numRoomData)) + Chr(0));
        for (var i = 1; i <= numRoomData; i++) {
            data.push(this.GetEverythingAfter(roomData[i].AppliesTo, "room ") + Chr(0) + roomData[i].Change + Chr(0));
        }
        // <<< TIMER STATE DATA >>>
        data.push(Trim(Str(this._numberTimers)) + Chr(0));
        for (var i = 1; i <= this._numberTimers; i++) {
            var t = this._timers[i];
            data.push(t.TimerName + Chr(0));
            if (t.TimerActive) {
                data.push(Chr(1));
            }
            else {
                data.push(Chr(0));
            }
            data.push(Trim(Str(t.TimerInterval)) + Chr(0));
            data.push(Trim(Str(t.TimerTicks)) + Chr(0));
        }
        // <<< STRING VARIABLE DATA >>>
        data.push(Trim(Str(this._numberStringVariables)) + Chr(0));
        for (var i = 1; i <= this._numberStringVariables; i++) {
            var s = this._stringVariable[i];
            data.push(s.VariableName + Chr(0) + Trim(Str(s.VariableUBound)) + Chr(0));
            for (var j = 0; j <= s.VariableUBound; j++) {
                data.push(s.VariableContents[j] + Chr(0));
            }
        }
        // <<< NUMERIC VARIABLE DATA >>>
        data.push(Trim(Str(this._numberNumericVariables)) + Chr(0));
        for (var i = 1; i <= this._numberNumericVariables; i++) {
            var n = this._numericVariable[i];
            data.push(n.VariableName + Chr(0) + Trim(Str(n.VariableUBound)) + Chr(0));
            for (var j = 0; j <= n.VariableUBound; j++) {
                data.push(n.VariableContents[j] + Chr(0));
            }
        }
        // Now encrypt data
        var dataString;
        var newFileData = [];
        dataString = data.join("");
        newFileData.push(Left(dataString, start - 1));
        for (var i = start; i <= Len(dataString); i++) {
            newFileData.push(Chr(255 - Asc(Mid(dataString, i, 1))));
        }
        return newFileData.join("");
    };
    LegacyGame.prototype.MoveThing = function (name, room, type, ctx) {
        var oldRoom = "";
        var id = this.GetThingNumber(name, "", type);
        if (InStr(room, "[") > 0) {
            var idx = this.GetArrayIndex(room, ctx);
            room = room + Trim(Str(idx.Index));
        }
        if (type == Thing.Character) {
            this._chars[id].ContainerRoom = room;
        }
        else if (type == Thing.Object) {
            oldRoom = this._objs[id].ContainerRoom;
            this._objs[id].ContainerRoom = room;
        }
        if (this._gameAslVersion >= 391) {
            // If this object contains other objects, move them too
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this.GetObjectProperty("parent", i, false, false)) == LCase(name)) {
                    this.MoveThing(this._objs[i].ObjectName, room, type, ctx);
                }
            }
        }
        this.UpdateObjectList(ctx);
        if (this.BeginsWith(LCase(room), "inventory") || this.BeginsWith(LCase(oldRoom), "inventory")) {
            this.UpdateItems(ctx);
        }
    };
    LegacyGame.prototype.ConvertParameter = function (parameter, convertChar, action, ctx) {
        // Returns a string with functions, string and
        // numeric variables executed or converted as
        // appropriate, read for display/etc.
        var result = "";
        var pos = 1;
        var finished = false;
        do {
            var varPos = InStrFrom(pos, parameter, convertChar);
            if (varPos == 0) {
                varPos = Len(parameter) + 1;
                finished = true;
            }
            var currentBit = Mid(parameter, pos, varPos - pos);
            result = result + currentBit;
            if (!finished) {
                var nextPos = InStrFrom(varPos + 1, parameter, convertChar);
                if (nextPos == 0) {
                    this.LogASLError("Line parameter <" + parameter + "> has missing " + convertChar, LogType.WarningError);
                    return "<ERROR>";
                }
                var varName = Mid(parameter, varPos + 1, (nextPos - 1) - varPos);
                if (varName == "") {
                    result = result + convertChar;
                }
                else {
                    if (action == ConvertType.Strings) {
                        result = result + this.GetStringContents(varName, ctx);
                    }
                    else if (action == ConvertType.Functions) {
                        varName = this.EvaluateInlineExpressions(varName);
                        result = result + this.DoFunction(varName, ctx);
                    }
                    else if (action == ConvertType.Numeric) {
                        result = result + Trim(Str(this.GetNumericContents(varName, ctx)));
                    }
                    else if (action == ConvertType.Collectables) {
                        result = result + Trim(Str(this.GetCollectableAmount(varName)));
                    }
                }
                pos = nextPos + 1;
            }
        } while (!(finished));
        return result;
    };
    LegacyGame.prototype.DoFunction = function (data, ctx) {
        var name;
        var parameter;
        var intFuncResult = "";
        var intFuncExecuted = false;
        var paramPos = InStr(data, "(");
        if (paramPos != 0) {
            name = Trim(Left(data, paramPos - 1));
            var endParamPos = InStrRev(data, ")");
            if (endParamPos == 0) {
                this.LogASLError("Expected ) in $" + data + "$", LogType.WarningError);
                return "";
            }
            parameter = Mid(data, paramPos + 1, (endParamPos - paramPos) - 1);
        }
        else {
            name = data;
            parameter = "";
        }
        var block;
        block = this.DefineBlockParam("function", name);
        if (block.StartLine == 0 && block.EndLine == 0) {
            //Function does not exist; try an internal function.
            intFuncResult = this.DoInternalFunction(name, parameter, ctx);
            if (intFuncResult == "__NOTDEFINED") {
                this.LogASLError("No such function '" + name + "'", LogType.WarningError);
                return "[ERROR]";
            }
            else {
                intFuncExecuted = true;
            }
        }
        if (intFuncExecuted) {
            return intFuncResult;
        }
        else {
            var newCtx = this.CopyContext(ctx);
            var numParameters = 0;
            var pos = 1;
            if (parameter != "") {
                parameter = parameter + ";";
                do {
                    numParameters = numParameters + 1;
                    var scp = InStrFrom(pos, parameter, ";");
                    var parameterData = Trim(Mid(parameter, pos, scp - pos));
                    this.SetStringContents("quest.function.parameter." + Trim(Str(numParameters)), parameterData, ctx);
                    newCtx.NumParameters = numParameters;
                    if (!newCtx.Parameters)
                        newCtx.Parameters = [];
                    newCtx.Parameters[numParameters] = parameterData;
                    pos = scp + 1;
                } while (!(pos >= Len(parameter)));
                this.SetStringContents("quest.function.numparameters", Trim(Str(numParameters)), ctx);
            }
            else {
                this.SetStringContents("quest.function.numparameters", "0", ctx);
                newCtx.NumParameters = 0;
            }
            var result = "";
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                this.ExecuteScript(this._lines[i], newCtx);
                result = newCtx.FunctionReturnValue;
            }
            return result;
        }
    };
    LegacyGame.prototype.DoInternalFunction = function (name, parameter, ctx) {
        var parameters;
        var untrimmedParameters;
        var objId = 0;
        var numParameters = 0;
        var pos = 1;
        if (parameter != "") {
            parameter = parameter + ";";
            do {
                numParameters = numParameters + 1;
                var scp = InStrFrom(pos, parameter, ";");
                if (!parameters)
                    parameters = [];
                if (!untrimmedParameters)
                    untrimmedParameters = [];
                untrimmedParameters[numParameters] = Mid(parameter, pos, scp - pos);
                parameters[numParameters] = Trim(untrimmedParameters[numParameters]);
                pos = scp + 1;
            } while (!(pos >= Len(parameter)));
            // Remove final ";"
            parameter = Left(parameter, Len(parameter) - 1);
        }
        else {
            numParameters = 1;
            parameters = [];
            untrimmedParameters = [];
            parameters[1] = "";
            untrimmedParameters[1] = "";
        }
        var param2;
        var param3;
        if (name == "displayname") {
            objId = this.GetObjectId(parameters[1], ctx);
            if (objId == -1) {
                this.LogASLError("Object '" + parameters[1] + "' does not exist", LogType.WarningError);
                return "!";
            }
            else {
                return this._objs[this.GetObjectId(parameters[1], ctx)].ObjectAlias;
            }
        }
        else if (name == "numberparameters") {
            return Trim(Str(ctx.NumParameters));
        }
        else if (name == "parameter") {
            if (numParameters == 0) {
                this.LogASLError("No parameter number specified for $parameter$ function", LogType.WarningError);
                return "";
            }
            else {
                if (Val(parameters[1]) > ctx.NumParameters) {
                    this.LogASLError("No parameter number " + parameters[1] + " sent to this function", LogType.WarningError);
                    return "";
                }
                else {
                    return Trim(ctx.Parameters[parseInt(parameters[1])]);
                }
            }
        }
        else if (name == "gettag") {
            // Deprecated
            return this.FindStatement(this.DefineBlockParam("room", parameters[1]), parameters[2]);
        }
        else if (name == "objectname") {
            return this._objs[ctx.CallingObjectId].ObjectName;
        }
        else if (name == "locationof") {
            for (var i = 1; i <= this._numberChars; i++) {
                if (LCase(this._chars[i].ObjectName) == LCase(parameters[1])) {
                    return this._chars[i].ContainerRoom;
                }
            }
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(parameters[1])) {
                    return this._objs[i].ContainerRoom;
                }
            }
        }
        else if (name == "lengthof") {
            return Str(Len(untrimmedParameters[1]));
        }
        else if (name == "left") {
            if (Val(parameters[2]) < 0) {
                this.LogASLError("Invalid function call in '$Left$(" + parameters[1] + "; " + parameters[2] + ")$'", LogType.WarningError);
                return "!";
            }
            else {
                return Left(parameters[1], parseInt(parameters[2]));
            }
        }
        else if (name == "right") {
            if (Val(parameters[2]) < 0) {
                this.LogASLError("Invalid function call in '$Right$(" + parameters[1] + "; " + parameters[2] + ")$'", LogType.WarningError);
                return "!";
            }
            else {
                return Right(parameters[1], parseInt(parameters[2]));
            }
        }
        else if (name == "mid") {
            if (numParameters == 3) {
                if (Val(parameters[2]) < 0) {
                    this.LogASLError("Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + "; " + parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                }
                else {
                    return Mid(parameters[1], parseInt(parameters[2]), parseInt(parameters[3]));
                }
            }
            else if (numParameters == 2) {
                if (Val(parameters[2]) < 0) {
                    this.LogASLError("Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + ")$'", LogType.WarningError);
                    return "!";
                }
                else {
                    return Mid(parameters[1], parseInt(parameters[2]));
                }
            }
            this.LogASLError("Invalid function call to '$Mid$(...)$'", LogType.WarningError);
            return "";
        }
        else if (name == "rand") {
            return Str(Math.random() * (parseFloat(parameters[2]) - parseFloat(parameters[1]) + 1) + parseFloat(parameters[1]));
        }
        else if (name == "instr") {
            if (numParameters == 3) {
                param3 = "";
                if (InStr(parameters[3], "_") != 0) {
                    for (var i = 1; i <= Len(parameters[3]); i++) {
                        if (Mid(parameters[3], i, 1) == "_") {
                            param3 = param3 + " ";
                        }
                        else {
                            param3 = param3 + Mid(parameters[3], i, 1);
                        }
                    }
                }
                else {
                    param3 = parameters[3];
                }
                if (Val(parameters[1]) <= 0) {
                    this.LogASLError("Invalid function call in '$instr(" + parameters[1] + "; " + parameters[2] + "; " + parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                }
                else {
                    return Trim(Str(InStrFrom(parseInt(parameters[1]), parameters[2], param3)));
                }
            }
            else if (numParameters == 2) {
                param2 = "";
                if (InStr(parameters[2], "_") != 0) {
                    for (var i = 1; i <= Len(parameters[2]); i++) {
                        if (Mid(parameters[2], i, 1) == "_") {
                            param2 = param2 + " ";
                        }
                        else {
                            param2 = param2 + Mid(parameters[2], i, 1);
                        }
                    }
                }
                else {
                    param2 = parameters[2];
                }
                return Trim(Str(InStr(parameters[1], param2)));
            }
            this.LogASLError("Invalid function call to '$Instr$(...)$'", LogType.WarningError);
            return "";
        }
        else if (name == "ucase") {
            return UCase(parameters[1]);
        }
        else if (name == "lcase") {
            return LCase(parameters[1]);
        }
        else if (name == "capfirst") {
            return UCase(Left(parameters[1], 1)) + Mid(parameters[1], 2);
        }
        else if (name == "symbol") {
            if (parameters[1] == "lt") {
                return "<";
            }
            else if (parameters[1] == "gt") {
                return ">";
            }
            else {
                return "!";
            }
        }
        else if (name == "loadmethod") {
            return this._gameLoadMethod;
        }
        else if (name == "timerstate") {
            for (var i = 1; i <= this._numberTimers; i++) {
                if (LCase(this._timers[i].TimerName) == LCase(parameters[1])) {
                    if (this._timers[i].TimerActive) {
                        return "1";
                    }
                    else {
                        return "0";
                    }
                }
            }
            this.LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "timerinterval") {
            for (var i = 1; i <= this._numberTimers; i++) {
                if (LCase(this._timers[i].TimerName) == LCase(parameters[1])) {
                    return Str(this._timers[i].TimerInterval);
                }
            }
            this.LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "ubound") {
            for (var i = 1; i <= this._numberNumericVariables; i++) {
                if (LCase(this._numericVariable[i].VariableName) == LCase(parameters[1])) {
                    return Trim(Str(this._numericVariable[i].VariableUBound));
                }
            }
            for (var i = 1; i <= this._numberStringVariables; i++) {
                if (LCase(this._stringVariable[i].VariableName) == LCase(parameters[1])) {
                    return Trim(Str(this._stringVariable[i].VariableUBound));
                }
            }
            this.LogASLError("No such variable '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "objectproperty") {
            var FoundObj = false;
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(parameters[1])) {
                    FoundObj = true;
                    objId = i;
                    i = this._numberObjs;
                }
            }
            if (!FoundObj) {
                this.LogASLError("No such object '" + parameters[1] + "'", LogType.WarningError);
                return "!";
            }
            else {
                return this.GetObjectProperty(parameters[2], objId);
            }
        }
        else if (name == "getobjectname") {
            if (numParameters == 3) {
                objId = this.Disambiguate(parameters[1], parameters[2] + ";" + parameters[3], ctx);
            }
            else if (numParameters == 2) {
                objId = this.Disambiguate(parameters[1], parameters[2], ctx);
            }
            else {
                objId = this.Disambiguate(parameters[1], this._currentRoom + ";inventory", ctx);
            }
            if (objId <= -1) {
                this.LogASLError("No object found with display name '" + parameters[1] + "'", LogType.WarningError);
                return "!";
            }
            else {
                return this._objs[objId].ObjectName;
            }
        }
        else if (name == "thisobject") {
            return this._objs[ctx.CallingObjectId].ObjectName;
        }
        else if (name == "thisobjectname") {
            return this._objs[ctx.CallingObjectId].ObjectAlias;
        }
        else if (name == "speechenabled") {
            return "1";
        }
        else if (name == "removeformatting") {
            return this.RemoveFormatting(parameter);
        }
        else if (name == "findexit" && this._gameAslVersion >= 410) {
            var e = this.FindExit(parameter);
            if (e == null) {
                return "";
            }
            else {
                return this._objs[e.GetObjId()].ObjectName;
            }
        }
        return "__NOTDEFINED";
    };
    LegacyGame.prototype.ExecFor = function (line, ctx) {
        // See if this is a "for each" loop:
        if (this.BeginsWith(line, "each ")) {
            this.ExecForEach(this.GetEverythingAfter(line, "each "), ctx);
            return;
        }
        // Executes a for loop, of form:
        //   for <variable; startvalue; endvalue> script
        var endValue = 0;
        var stepValue = 0;
        var forData = this.GetParameter(line, ctx);
        // Extract individual components:
        var scp1 = InStr(forData, ";");
        var scp2 = InStrFrom(scp1 + 1, forData, ";");
        var scp3 = InStrFrom(scp2 + 1, forData, ";");
        var counterVariable = Trim(Left(forData, scp1 - 1));
        var startValue = parseInt(Mid(forData, scp1 + 1, (scp2 - 1) - scp1));
        if (scp3 != 0) {
            endValue = parseInt(Mid(forData, scp2 + 1, (scp3 - 1) - scp2));
            stepValue = parseInt(Mid(forData, scp3 + 1));
        }
        else {
            endValue = parseInt(Mid(forData, scp2 + 1));
            stepValue = 1;
        }
        var loopScript = Trim(Mid(line, InStr(line, ">") + 1));
        for (var i = startValue; stepValue > 0 ? i <= endValue : i >= endValue; i = i + stepValue) {
            this.SetNumericVariableContents(counterVariable, i, ctx);
            this.ExecuteScript(loopScript, ctx);
            i = this.GetNumericContents(counterVariable, ctx);
        }
    };
    LegacyGame.prototype.ExecSetVar = function (varInfo, ctx) {
        // Sets variable contents from a script parameter.
        // Eg <var1;7> sets numeric variable var1
        // to 7
        var scp = InStr(varInfo, ";");
        var varName = Trim(Left(varInfo, scp - 1));
        var varCont = Trim(Mid(varInfo, scp + 1));
        var idx = this.GetArrayIndex(varName, ctx);
        if (IsNumeric(idx.Name)) {
            this.LogASLError("Invalid numeric variable name '" + idx.Name + "' - variable names cannot be numeric", LogType.WarningError);
            return;
        }
        try {
            if (this._gameAslVersion >= 391) {
                var expResult = this.ExpressionHandler(varCont);
                if (expResult.Success == ExpressionSuccess.OK) {
                    varCont = expResult.Result;
                }
                else {
                    varCont = "0";
                    this.LogASLError("Error setting numeric variable <" + varInfo + "> : " + expResult.Message, LogType.WarningError);
                }
            }
            else {
                var obscuredVarInfo = this.ObscureNumericExps(varCont);
                var opPos = InStr(obscuredVarInfo, "+");
                if (opPos == 0) {
                    opPos = InStr(obscuredVarInfo, "*");
                }
                if (opPos == 0) {
                    opPos = InStr(obscuredVarInfo, "/");
                }
                if (opPos == 0) {
                    opPos = InStrFrom(2, obscuredVarInfo, "-");
                }
                if (opPos != 0) {
                    var op = Mid(varCont, opPos, 1);
                    var num1 = Val(Left(varCont, opPos - 1));
                    var num2 = Val(Mid(varCont, opPos + 1));
                    switch (op) {
                        case "+":
                            varCont = Str(num1 + num2);
                            break;
                        case "-":
                            varCont = Str(num1 - num2);
                            break;
                        case "*":
                            varCont = Str(num1 * num2);
                            break;
                        case "/":
                            if (num2 != 0) {
                                varCont = Str(num1 / num2);
                            }
                            else {
                                this.LogASLError("Division by zero - The result of this operation has been set to zero.", LogType.WarningError);
                                varCont = "0";
                            }
                            break;
                    }
                }
            }
            this.SetNumericVariableContents(idx.Name, Val(varCont), ctx, idx.Index);
        }
        catch (e) {
            this.LogASLError("Error setting variable '" + idx.Name + "' to '" + varCont + "'", LogType.WarningError);
        }
    };
    LegacyGame.prototype.ExecuteIfGot = function (item) {
        if (this._gameAslVersion >= 280) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(item)) {
                    return this._objs[i].ContainerRoom == "inventory" && this._objs[i].Exists;
                }
            }
            this.LogASLError("No object '" + item + "' defined.", LogType.WarningError);
            return false;
        }
        for (var i = 1; i <= this._numberItems; i++) {
            if (LCase(this._items[i].Name) == LCase(item)) {
                return this._items[i].Got;
            }
        }
        this.LogASLError("Item '" + item + "' not defined.", LogType.WarningError);
        return false;
    };
    LegacyGame.prototype.ExecuteIfHas = function (condition) {
        var checkValue = 0;
        var colNum = 0;
        var scp = InStr(condition, ";");
        var name = Trim(Left(condition, scp - 1));
        var newVal = Trim(Right(condition, Len(condition) - scp));
        var found = false;
        for (var i = 1; i <= this._numCollectables; i++) {
            if (this._collectables[i].Name == name) {
                colNum = i;
                found = true;
                break;
            }
        }
        if (!found) {
            this.LogASLError("No such collectable in " + condition, LogType.WarningError);
            return false;
        }
        var op = Left(newVal, 1);
        var newValue = Trim(Right(newVal, Len(newVal) - 1));
        if (IsNumeric(newValue)) {
            checkValue = Val(newValue);
        }
        else {
            checkValue = this.GetCollectableAmount(newValue);
        }
        if (op == "+") {
            return this._collectables[colNum].Value > checkValue;
        }
        else if (op == "-") {
            return this._collectables[colNum].Value < checkValue;
        }
        else if (op == "=") {
            return this._collectables[colNum].Value == checkValue;
        }
        return false;
    };
    LegacyGame.prototype.ExecuteIfIs = function (condition) {
        var value1;
        var value2;
        var op;
        var expectNumerics = false;
        var expResult;
        var scp = InStr(condition, ";");
        if (scp == 0) {
            this.LogASLError("Expected second parameter in 'is " + condition + "'", LogType.WarningError);
            return false;
        }
        var scp2 = InStrFrom(scp + 1, condition, ";");
        if (scp2 == 0) {
            // Only two parameters => standard "="
            op = "=";
            value1 = Trim(Left(condition, scp - 1));
            value2 = Trim(Mid(condition, scp + 1));
        }
        else {
            value1 = Trim(Left(condition, scp - 1));
            op = Trim(Mid(condition, scp + 1, (scp2 - scp) - 1));
            value2 = Trim(Mid(condition, scp2 + 1));
        }
        if (this._gameAslVersion >= 391) {
            // Evaluate expressions in Value1 and Value2
            expResult = this.ExpressionHandler(value1);
            if (expResult.Success == ExpressionSuccess.OK) {
                value1 = expResult.Result;
            }
            expResult = this.ExpressionHandler(value2);
            if (expResult.Success == ExpressionSuccess.OK) {
                value2 = expResult.Result;
            }
        }
        var result = false;
        switch (op) {
            case "=":
                if (LCase(value1) == LCase(value2)) {
                    result = true;
                }
                expectNumerics = false;
                break;
            case "!=":
                if (LCase(value1) != LCase(value2)) {
                    result = true;
                }
                expectNumerics = false;
                break;
            case "gt":
                if (Val(value1) > Val(value2)) {
                    result = true;
                }
                expectNumerics = true;
                break;
            case "lt":
                if (Val(value1) < Val(value2)) {
                    result = true;
                }
                expectNumerics = true;
                break;
            case "gt=":
                if (Val(value1) >= Val(value2)) {
                    result = true;
                }
                expectNumerics = true;
                break;
            case "lt=":
                if (Val(value1) <= Val(value2)) {
                    result = true;
                }
                expectNumerics = true;
                break;
            default:
                this.LogASLError("Unrecognised comparison condition in 'is " + condition + "'", LogType.WarningError);
                break;
        }
        if (expectNumerics) {
            if (!(IsNumeric(value1) && IsNumeric(value2))) {
                this.LogASLError("Expected numeric comparison comparing '" + value1 + "' and '" + value2 + "'", LogType.WarningError);
            }
        }
        return result;
    };
    LegacyGame.prototype.GetNumericContents = function (name, ctx, noError) {
        if (noError === void 0) { noError = false; }
        var numNumber = 0;
        var arrayIndex = 0;
        var exists = false;
        // First, see if the variable already exists. If it
        // does, get its contents. If not, generate an error.
        if (InStr(name, "[") != 0 && InStr(name, "]") != 0) {
            var op = InStr(name, "[");
            var cp = InStr(name, "]");
            var arrayIndexData = Mid(name, op + 1, (cp - op) - 1);
            if (IsNumeric(arrayIndexData)) {
                arrayIndex = parseInt(arrayIndexData);
            }
            else {
                arrayIndex = this.GetNumericContents(arrayIndexData, ctx);
            }
            name = Left(name, op - 1);
        }
        else {
            arrayIndex = 0;
        }
        if (this._numberNumericVariables > 0) {
            for (var i = 1; i <= this._numberNumericVariables; i++) {
                if (LCase(this._numericVariable[i].VariableName) == LCase(name)) {
                    numNumber = i;
                    exists = true;
                    break;
                }
            }
        }
        if (!exists) {
            if (!noError) {
                this.LogASLError("No numeric variable '" + name + "' defined.", LogType.WarningError);
            }
            return -32767;
        }
        if (arrayIndex > this._numericVariable[numNumber].VariableUBound) {
            if (!noError) {
                this.LogASLError("Array index of '" + name + "[" + Trim(Str(arrayIndex)) + "]' too big.", LogType.WarningError);
            }
            return -32766;
        }
        // Now, set the contents
        return Val(this._numericVariable[numNumber].VariableContents[arrayIndex]);
    };
    LegacyGame.prototype.PlayerErrorMessage = function (e, ctx) {
        this.Print(this.GetErrorMessage(e, ctx), ctx);
    };
    LegacyGame.prototype.PlayerErrorMessage_ExtendInfo = function (e, ctx, extraInfo) {
        var message = this.GetErrorMessage(e, ctx);
        if (extraInfo != "") {
            if (Right(message, 1) == ".") {
                message = Left(message, Len(message) - 1);
            }
            message = message + " - " + extraInfo + ".";
        }
        this.Print(message, ctx);
    };
    LegacyGame.prototype.GetErrorMessage = function (e, ctx) {
        return this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(this._playerErrorMessageString[e], "%", ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx), "#", ConvertType.Strings, ctx);
    };
    LegacyGame.prototype.PlayMedia = function (filename, sync, looped) {
        if (sync === void 0) { sync = false; }
        if (looped === void 0) { looped = false; }
        if (filename.length == 0) {
            this._player.StopSound();
        }
        else {
            if (looped && sync) {
                sync = false;
            }
            // Can't loop and sync at the same time, that would just hang!
            this._player.PlaySound(filename, sync, looped);
        }
    };
    LegacyGame.prototype.PlayWav = function (parameter) {
        var params = parameter.split(";").map(function (p) {
            return p.trim();
        });
        var filename = params[0];
        var looped = (params.indexOf("loop") != -1);
        var sync = (params.indexOf("sync") != -1);
        if (filename.length > 0 && InStr(filename, ".") == 0) {
            filename = filename + ".wav";
        }
        this.PlayMedia(filename, sync, looped);
    };
    LegacyGame.prototype.RestoreGameData = function (fileData) {
        var appliesTo;
        var data = "";
        var objId = 0;
        var timerNum = 0;
        var varUbound = 0;
        var found = false;
        var numStoredData = 0;
        var storedData = [];
        var decryptedFile = [];
        // Decrypt file
        for (var i = 1; i <= Len(fileData); i++) {
            decryptedFile.push(Chr(255 - Asc(Mid(fileData, i, 1))));
        }
        this._fileData = decryptedFile.join("");
        this._currentRoom = this.GetNextChunk();
        // OBJECTS
        var numData = parseInt(this.GetNextChunk());
        var createdObjects = [];
        for (var i = 1; i <= numData; i++) {
            appliesTo = this.GetNextChunk();
            data = this.GetNextChunk();
            // As of Quest 4.0, properties and actions are put into StoredData while we load the file,
            // and then processed later. This is so any created rooms pick up their properties - otherwise
            // we try to set them before they've been created.
            if (this.BeginsWith(data, "properties ") || this.BeginsWith(data, "action ")) {
                numStoredData = numStoredData + 1;
                if (!storedData)
                    storedData = [];
                storedData[numStoredData] = new ChangeType();
                storedData[numStoredData].AppliesTo = appliesTo;
                storedData[numStoredData].Change = data;
            }
            else if (this.BeginsWith(data, "create ")) {
                var createData = appliesTo + ";" + this.GetEverythingAfter(data, "create ");
                // workaround bug where duplicate "create" entries appear in the restore data
                if (createdObjects.indexOf(createData) == -1) {
                    this.ExecuteCreate("object <" + createData + ">", this._nullContext);
                    createdObjects.push(createData);
                }
            }
            else {
                this.LogASLError("QSG Error: Unrecognised item '" + appliesTo + "; " + data + "'", LogType.InternalError);
            }
        }
        numData = parseInt(this.GetNextChunk());
        for (var i = 1; i <= numData; i++) {
            appliesTo = this.GetNextChunk();
            data = this.GetFileDataChars(2);
            objId = this.GetObjectIdNoAlias(appliesTo);
            if (Left(data, 1) == Chr(1)) {
                this._objs[objId].Exists = true;
            }
            else {
                this._objs[objId].Exists = false;
            }
            if (Right(data, 1) == Chr(1)) {
                this._objs[objId].Visible = true;
            }
            else {
                this._objs[objId].Visible = false;
            }
            this._objs[objId].ContainerRoom = this.GetNextChunk();
        }
        // ROOMS
        numData = parseInt(this.GetNextChunk());
        for (var i = 1; i <= numData; i++) {
            appliesTo = this.GetNextChunk();
            data = this.GetNextChunk();
            if (this.BeginsWith(data, "exit ")) {
                this.ExecuteCreate(data, this._nullContext);
            }
            else if (data == "create") {
                this.ExecuteCreate("room <" + appliesTo + ">", this._nullContext);
            }
            else if (this.BeginsWith(data, "destroy exit ")) {
                this.DestroyExit(appliesTo + "; " + this.GetEverythingAfter(data, "destroy exit "), this._nullContext);
            }
        }
        // Now go through and apply object properties and actions
        for (var i = 1; i <= numStoredData; i++) {
            var d = storedData[i];
            if (this.BeginsWith(d.Change, "properties ")) {
                this.AddToObjectProperties(this.GetEverythingAfter(d.Change, "properties "), this.GetObjectIdNoAlias(d.AppliesTo), this._nullContext);
            }
            else if (this.BeginsWith(d.Change, "action ")) {
                this.AddToObjectActions(this.GetEverythingAfter(d.Change, "action "), this.GetObjectIdNoAlias(d.AppliesTo), this._nullContext);
            }
        }
        // TIMERS
        numData = parseInt(this.GetNextChunk());
        for (var i = 1; i <= numData; i++) {
            found = false;
            appliesTo = this.GetNextChunk();
            for (var j = 1; j <= this._numberTimers; j++) {
                if (this._timers[j].TimerName == appliesTo) {
                    timerNum = j;
                    found = true;
                    break;
                }
            }
            if (found) {
                var t = this._timers[timerNum];
                var thisChar = this.GetFileDataChars(1);
                if (thisChar == Chr(1)) {
                    t.TimerActive = true;
                }
                else {
                    t.TimerActive = false;
                }
                t.TimerInterval = parseInt(this.GetNextChunk());
                t.TimerTicks = parseInt(this.GetNextChunk());
            }
        }
        // STRING VARIABLES
        // Set this flag so we don't run any status variable onchange scripts while restoring
        this._gameIsRestoring = true;
        numData = parseInt(this.GetNextChunk());
        for (var i = 1; i <= numData; i++) {
            appliesTo = this.GetNextChunk();
            varUbound = parseInt(this.GetNextChunk());
            if (varUbound == 0) {
                data = this.GetNextChunk();
                this.SetStringContents(appliesTo, data, this._nullContext);
            }
            else {
                for (var j = 0; j <= varUbound; j++) {
                    data = this.GetNextChunk();
                    this.SetStringContents(appliesTo, data, this._nullContext, j);
                }
            }
        }
        // NUMERIC VARIABLES
        numData = parseInt(this.GetNextChunk());
        for (var i = 1; i <= numData; i++) {
            appliesTo = this.GetNextChunk();
            varUbound = parseInt(this.GetNextChunk());
            if (varUbound == 0) {
                data = this.GetNextChunk();
                this.SetNumericVariableContents(appliesTo, Val(data), this._nullContext);
            }
            else {
                for (var j = 0; j <= varUbound; j++) {
                    data = this.GetNextChunk();
                    this.SetNumericVariableContents(appliesTo, Val(data), this._nullContext, j);
                }
            }
        }
        this._gameIsRestoring = false;
    };
    LegacyGame.prototype.SetBackground = function (col) {
        this._player.SetBackground("#" + this.GetHTMLColour(col, "white"));
    };
    LegacyGame.prototype.SetForeground = function (col) {
        this._player.SetForeground("#" + this.GetHTMLColour(col, "black"));
    };
    LegacyGame.prototype.SetDefaultPlayerErrorMessages = function () {
        this._playerErrorMessageString[PlayerError.BadCommand] = "I don't understand your command. Type HELP for a list of valid commands.";
        this._playerErrorMessageString[PlayerError.BadGo] = "I don't understand your use of 'GO' - you must either GO in some direction, or GO TO a place.";
        this._playerErrorMessageString[PlayerError.BadGive] = "You didn't say who you wanted to give that to.";
        this._playerErrorMessageString[PlayerError.BadCharacter] = "I can't see anybody of that name here.";
        this._playerErrorMessageString[PlayerError.NoItem] = "You don't have that.";
        this._playerErrorMessageString[PlayerError.ItemUnwanted] = "#quest.error.gender# doesn't want #quest.error.article#.";
        this._playerErrorMessageString[PlayerError.BadLook] = "You didn't say what you wanted to look at.";
        this._playerErrorMessageString[PlayerError.BadThing] = "I can't see that here.";
        this._playerErrorMessageString[PlayerError.DefaultLook] = "Nothing out of the ordinary.";
        this._playerErrorMessageString[PlayerError.DefaultSpeak] = "#quest.error.gender# says nothing.";
        this._playerErrorMessageString[PlayerError.BadItem] = "I can't see that anywhere.";
        this._playerErrorMessageString[PlayerError.DefaultTake] = "You pick #quest.error.article# up.";
        this._playerErrorMessageString[PlayerError.BadUse] = "You didn't say what you wanted to use that on.";
        this._playerErrorMessageString[PlayerError.DefaultUse] = "You can't use that here.";
        this._playerErrorMessageString[PlayerError.DefaultOut] = "There's nowhere you can go out to around here.";
        this._playerErrorMessageString[PlayerError.BadPlace] = "You can't go there.";
        this._playerErrorMessageString[PlayerError.DefaultExamine] = "Nothing out of the ordinary.";
        this._playerErrorMessageString[PlayerError.BadTake] = "You can't take #quest.error.article#.";
        this._playerErrorMessageString[PlayerError.CantDrop] = "You can't drop that here.";
        this._playerErrorMessageString[PlayerError.DefaultDrop] = "You drop #quest.error.article#.";
        this._playerErrorMessageString[PlayerError.BadDrop] = "You are not carrying such a thing.";
        this._playerErrorMessageString[PlayerError.BadPronoun] = "I don't know what '#quest.error.pronoun#' you are referring to.";
        this._playerErrorMessageString[PlayerError.BadExamine] = "You didn't say what you wanted to examine.";
        this._playerErrorMessageString[PlayerError.AlreadyOpen] = "It is already open.";
        this._playerErrorMessageString[PlayerError.AlreadyClosed] = "It is already closed.";
        this._playerErrorMessageString[PlayerError.CantOpen] = "You can't open that.";
        this._playerErrorMessageString[PlayerError.CantClose] = "You can't close that.";
        this._playerErrorMessageString[PlayerError.DefaultOpen] = "You open it.";
        this._playerErrorMessageString[PlayerError.DefaultClose] = "You close it.";
        this._playerErrorMessageString[PlayerError.BadPut] = "You didn't specify what you wanted to put #quest.error.article# on or in.";
        this._playerErrorMessageString[PlayerError.CantPut] = "You can't put that there.";
        this._playerErrorMessageString[PlayerError.DefaultPut] = "Done.";
        this._playerErrorMessageString[PlayerError.CantRemove] = "You can't remove that.";
        this._playerErrorMessageString[PlayerError.AlreadyPut] = "It is already there.";
        this._playerErrorMessageString[PlayerError.DefaultRemove] = "Done.";
        this._playerErrorMessageString[PlayerError.Locked] = "The exit is locked.";
        this._playerErrorMessageString[PlayerError.DefaultWait] = "Press a key to continue...";
        this._playerErrorMessageString[PlayerError.AlreadyTaken] = "You already have that.";
    };
    LegacyGame.prototype.SetFont = function (name) {
        if (name == "") {
            name = this._defaultFontName;
        }
        this._player.SetFont(name);
    };
    LegacyGame.prototype.SetNumericVariableContents = function (name, content, ctx, arrayIndex) {
        if (arrayIndex === void 0) { arrayIndex = 0; }
        var numNumber = 0;
        var exists = false;
        if (IsNumeric(name)) {
            this.LogASLError("Illegal numeric variable name '" + name + "' - check you didn't put % around the variable name in the ASL code", LogType.WarningError);
            return;
        }
        // First, see if variable already exists. If it does,
        // modify it. If not, create it.
        if (this._numberNumericVariables > 0) {
            for (var i = 1; i <= this._numberNumericVariables; i++) {
                if (LCase(this._numericVariable[i].VariableName) == LCase(name)) {
                    numNumber = i;
                    exists = true;
                    break;
                }
            }
        }
        if (!exists) {
            this._numberNumericVariables = this._numberNumericVariables + 1;
            numNumber = this._numberNumericVariables;
            if (!this._numericVariable)
                this._numericVariable = [];
            this._numericVariable[numNumber] = new VariableType();
            this._numericVariable[numNumber].VariableUBound = arrayIndex;
        }
        if (arrayIndex > this._numericVariable[numNumber].VariableUBound) {
            if (!this._numericVariable[numNumber].VariableContents)
                this._numericVariable[numNumber].VariableContents = [];
            this._numericVariable[numNumber].VariableUBound = arrayIndex;
        }
        // Now, set the contents
        this._numericVariable[numNumber].VariableName = name;
        if (!this._numericVariable[numNumber].VariableContents)
            this._numericVariable[numNumber].VariableContents = [];
        this._numericVariable[numNumber].VariableContents[arrayIndex] = (content).toString();
        if (this._numericVariable[numNumber].OnChangeScript != "" && !this._gameIsRestoring) {
            var script = this._numericVariable[numNumber].OnChangeScript;
            this.ExecuteScript(script, ctx);
        }
        if (this._numericVariable[numNumber].DisplayString != "") {
            this.UpdateStatusVars(ctx);
        }
    };
    LegacyGame.prototype.SetOpenClose = function (name, open, ctx) {
        var cmd;
        if (open) {
            cmd = "open";
        }
        else {
            cmd = "close";
        }
        var id = this.GetObjectIdNoAlias(name);
        if (id == 0) {
            this.LogASLError("Invalid object name specified in '" + cmd + " <" + name + ">", LogType.WarningError);
            return;
        }
        this.DoOpenClose(id, open, false, ctx);
    };
    LegacyGame.prototype.SetTimerState = function (name, state) {
        for (var i = 1; i <= this._numberTimers; i++) {
            if (LCase(name) == LCase(this._timers[i].TimerName)) {
                this._timers[i].TimerActive = state;
                this._timers[i].BypassThisTurn = true;
                // don't trigger timer during the turn it was first enabled
                return null;
            }
        }
        this.LogASLError("No such timer '" + name + "'", LogType.WarningError);
    };
    LegacyGame.prototype.SetUnknownVariableType = function (variableData, ctx) {
        var scp = InStr(variableData, ";");
        if (scp == 0) {
            return SetResult.Error;
        }
        var name = Trim(Left(variableData, scp - 1));
        if (InStr(name, "[") != 0 && InStr(name, "]") != 0) {
            var pos = InStr(name, "[");
            name = Left(name, pos - 1);
        }
        for (var i = 1; i <= this._numberStringVariables; i++) {
            if (LCase(this._stringVariable[i].VariableName) == LCase(name)) {
                this.ExecSetString(variableData, ctx);
                return SetResult.Found;
            }
        }
        for (var i = 1; i <= this._numberNumericVariables; i++) {
            if (LCase(this._numericVariable[i].VariableName) == LCase(name)) {
                this.ExecSetVar(variableData, ctx);
                return SetResult.Found;
            }
        }
        for (var i = 1; i <= this._numCollectables; i++) {
            if (LCase(this._collectables[i].Name) == LCase(name)) {
                this.ExecuteSetCollectable(variableData, ctx);
                return SetResult.Found;
            }
        }
        return SetResult.Unfound;
    };
    LegacyGame.prototype.SetUpChoiceForm = function (blockName, ctx) {
        // Returns script to execute from choice block
        var block = this.DefineBlockParam("selection", blockName);
        var prompt = this.FindStatement(block, "info");
        var menuOptions = {};
        var menuScript = {};
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "choice ")) {
                menuOptions[i.toString()] = this.GetParameter(this._lines[i], ctx);
                menuScript[i.toString()] = Trim(Right(this._lines[i], Len(this._lines[i]) - InStr(this._lines[i], ">")));
            }
        }
        this.Print("- |i" + prompt + "|xi", ctx);
        var mnu = new MenuData(prompt, menuOptions, false);
        var choice = this.ShowMenu(mnu);
        this.Print("- " + menuOptions[choice] + "|n", ctx);
        return menuScript[choice];
    };
    LegacyGame.prototype.SetUpDefaultFonts = function () {
        // Sets up default fonts
        var gameblock = this.GetDefineBlock("game");
        this._defaultFontName = "Arial";
        this._defaultFontSize = 9;
        for (var i = gameblock.StartLine + 1; i <= gameblock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "default fontname ")) {
                var name = this.GetParameter(this._lines[i], this._nullContext);
                if (name != "") {
                    this._defaultFontName = name;
                }
            }
            else if (this.BeginsWith(this._lines[i], "default fontsize ")) {
                var size = parseInt(this.GetParameter(this._lines[i], this._nullContext));
                if (size != 0) {
                    this._defaultFontSize = size;
                }
            }
        }
    };
    LegacyGame.prototype.SetUpDisplayVariables = function () {
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "define variable ")) {
                var variable = new VariableType();
                variable.VariableContents = [];
                variable.VariableName = this.GetParameter(this._lines[i], this._nullContext);
                variable.DisplayString = "";
                variable.NoZeroDisplay = false;
                variable.OnChangeScript = "";
                variable.VariableContents[0] = "";
                variable.VariableUBound = 0;
                var type = "numeric";
                do {
                    i = i + 1;
                    if (this.BeginsWith(this._lines[i], "type ")) {
                        type = this.GetEverythingAfter(this._lines[i], "type ");
                        if (type != "string" && type != "numeric") {
                            this.LogASLError("Unrecognised variable type in variable '" + variable.VariableName + "' - type '" + type + "'", LogType.WarningError);
                            break;
                        }
                    }
                    else if (this.BeginsWith(this._lines[i], "onchange ")) {
                        variable.OnChangeScript = this.GetEverythingAfter(this._lines[i], "onchange ");
                    }
                    else if (this.BeginsWith(this._lines[i], "display ")) {
                        var displayString = this.GetEverythingAfter(this._lines[i], "display ");
                        if (this.BeginsWith(displayString, "nozero ")) {
                            variable.NoZeroDisplay = true;
                        }
                        variable.DisplayString = this.GetParameter(this._lines[i], this._nullContext, false);
                    }
                    else if (this.BeginsWith(this._lines[i], "value ")) {
                        variable.VariableContents[0] = this.GetParameter(this._lines[i], this._nullContext);
                    }
                } while (!(Trim(this._lines[i]) == "end define"));
                if (type == "string") {
                    // Create string variable
                    this._numberStringVariables = this._numberStringVariables + 1;
                    var id = this._numberStringVariables;
                    if (!this._stringVariable)
                        this._stringVariable = [];
                    this._stringVariable[id] = variable;
                    this._numDisplayStrings = this._numDisplayStrings + 1;
                }
                else if (type == "numeric") {
                    if (variable.VariableContents[0] == "") {
                        variable.VariableContents[0] = (0).toString();
                    }
                    this._numberNumericVariables = this._numberNumericVariables + 1;
                    var iNumNumber = this._numberNumericVariables;
                    if (!this._numericVariable)
                        this._numericVariable = [];
                    this._numericVariable[iNumNumber] = variable;
                    this._numDisplayNumerics = this._numDisplayNumerics + 1;
                }
            }
        }
    };
    LegacyGame.prototype.SetUpGameObject = function () {
        this._numberObjs = 1;
        this._objs = [];
        this._objs[1] = new ObjectType();
        var o = this._objs[1];
        o.ObjectName = "game";
        o.ObjectAlias = "";
        o.Visible = false;
        o.Exists = true;
        var nestBlock = 0;
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (nestBlock == 0) {
                if (this.BeginsWith(this._lines[i], "define ")) {
                    nestBlock = nestBlock + 1;
                }
                else if (this.BeginsWith(this._lines[i], "properties ")) {
                    this.AddToObjectProperties(this.GetParameter(this._lines[i], this._nullContext), this._numberObjs, this._nullContext);
                }
                else if (this.BeginsWith(this._lines[i], "type ")) {
                    o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                    if (!o.TypesIncluded)
                        o.TypesIncluded = [];
                    o.TypesIncluded[o.NumberTypesIncluded] = this.GetParameter(this._lines[i], this._nullContext);
                    var propertyData = this.GetPropertiesInType(this.GetParameter(this._lines[i], this._nullContext));
                    this.AddToObjectProperties(propertyData.Properties, this._numberObjs, this._nullContext);
                    for (var k = 1; k <= propertyData.NumberActions; k++) {
                        this.AddObjectAction(this._numberObjs, propertyData.Actions[k].ActionName, propertyData.Actions[k].Script);
                    }
                }
                else if (this.BeginsWith(this._lines[i], "action ")) {
                    this.AddToObjectActions(this.GetEverythingAfter(this._lines[i], "action "), this._numberObjs, this._nullContext);
                }
            }
            else {
                if (Trim(this._lines[i]) == "end define") {
                    nestBlock = nestBlock - 1;
                }
            }
        }
    };
    LegacyGame.prototype.SetUpMenus = function () {
        var exists = false;
        var menuTitle = "";
        var menuOptions = {};
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define menu ")) {
                if (exists) {
                    this.LogASLError("Can't load menu '" + this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext) + "' - only one menu can be added.", LogType.WarningError);
                }
                else {
                    menuTitle = this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext);
                    for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                        if (Trim(this._lines[j]) != "") {
                            var scp = InStr(this._lines[j], ":");
                            if (scp == 0 && this._lines[j] != "-") {
                                this.LogASLError("No menu command specified in menu '" + menuTitle + "', item '" + this._lines[j], LogType.WarningError);
                            }
                            else {
                                if (this._lines[j] == "-") {
                                    menuOptions["k" + j] = "-";
                                }
                                else {
                                    menuOptions[Trim(Mid(this._lines[j], scp + 1))] = Trim(Left(this._lines[j], scp - 1));
                                }
                            }
                        }
                    }
                    if (Object.keys(menuOptions).length > 0) {
                        exists = true;
                    }
                }
            }
        }
        if (exists) {
            var windowMenu = new MenuData(menuTitle, menuOptions, false);
            this._player.SetWindowMenu(windowMenu);
        }
    };
    LegacyGame.prototype.SetUpOptions = function () {
        var opt;
        for (var i = this.GetDefineBlock("options").StartLine + 1; i <= this.GetDefineBlock("options").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "panes ")) {
                opt = LCase(Trim(this.GetEverythingAfter(this._lines[i], "panes ")));
                this._player.SetPanesVisible(opt);
            }
            else if (this.BeginsWith(this._lines[i], "abbreviations ")) {
                opt = LCase(Trim(this.GetEverythingAfter(this._lines[i], "abbreviations ")));
                if (opt == "off") {
                    this._useAbbreviations = false;
                }
                else {
                    this._useAbbreviations = true;
                }
            }
        }
    };
    LegacyGame.prototype.SetUpRoomData = function () {
        var defaultProperties = new PropertiesActions();
        // see if define type <defaultroom> exists:
        var defaultExists = false;
        for (var i = 1; i <= this._numberSections; i++) {
            if (Trim(this._lines[this._defineBlocks[i].StartLine]) == "define type <defaultroom>") {
                defaultExists = true;
                defaultProperties = this.GetPropertiesInType("defaultroom");
                break;
            }
        }
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define room ")) {
                this._numberRooms = this._numberRooms + 1;
                if (!this._rooms)
                    this._rooms = [];
                this._rooms[this._numberRooms] = new RoomType();
                this._numberObjs = this._numberObjs + 1;
                if (!this._objs)
                    this._objs = [];
                this._objs[this._numberObjs] = new ObjectType();
                var r = this._rooms[this._numberRooms];
                r.RoomName = this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext);
                this._objs[this._numberObjs].ObjectName = r.RoomName;
                this._objs[this._numberObjs].IsRoom = true;
                this._objs[this._numberObjs].CorresRoom = r.RoomName;
                this._objs[this._numberObjs].CorresRoomId = this._numberRooms;
                r.ObjId = this._numberObjs;
                if (this._gameAslVersion >= 410) {
                    r.Exits = new RoomExits(this);
                    r.Exits.SetObjId(r.ObjId);
                }
                if (defaultExists) {
                    this.AddToObjectProperties(defaultProperties.Properties, this._numberObjs, this._nullContext);
                    for (var k = 1; k <= defaultProperties.NumberActions; k++) {
                        this.AddObjectAction(this._numberObjs, defaultProperties.Actions[k].ActionName, defaultProperties.Actions[k].Script);
                    }
                }
                for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                    if (this.BeginsWith(this._lines[j], "define ")) {
                        //skip nested blocks
                        var nestedBlock = 1;
                        do {
                            j = j + 1;
                            if (this.BeginsWith(this._lines[j], "define ")) {
                                nestedBlock = nestedBlock + 1;
                            }
                            else if (Trim(this._lines[j]) == "end define") {
                                nestedBlock = nestedBlock - 1;
                            }
                        } while (!(nestedBlock == 0));
                    }
                    if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "alias ")) {
                        r.RoomAlias = this.GetParameter(this._lines[j], this._nullContext);
                        this._objs[this._numberObjs].ObjectAlias = r.RoomAlias;
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("alias=" + r.RoomAlias, this._numberObjs, this._nullContext);
                        }
                    }
                    else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "description ")) {
                        r.Description = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "description "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Description.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "description", r.Description.Data);
                            }
                            else {
                                this.AddToObjectProperties("description=" + r.Description.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "out ")) {
                        r.Out.Text = this.GetParameter(this._lines[j], this._nullContext);
                        r.Out.Script = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                        if (this._gameAslVersion >= 350) {
                            if (r.Out.Script != "") {
                                this.AddObjectAction(this._numberObjs, "out", r.Out.Script);
                            }
                            this.AddToObjectProperties("out=" + r.Out.Text, this._numberObjs, this._nullContext);
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "east ")) {
                        r.East = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "east "));
                        if (this._gameAslVersion >= 350) {
                            if (r.East.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "east", r.East.Data);
                            }
                            else {
                                this.AddToObjectProperties("east=" + r.East.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "west ")) {
                        r.West = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "west "));
                        if (this._gameAslVersion >= 350) {
                            if (r.West.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "west", r.West.Data);
                            }
                            else {
                                this.AddToObjectProperties("west=" + r.West.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "north ")) {
                        r.North = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "north "));
                        if (this._gameAslVersion >= 350) {
                            if (r.North.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "north", r.North.Data);
                            }
                            else {
                                this.AddToObjectProperties("north=" + r.North.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "south ")) {
                        r.South = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "south "));
                        if (this._gameAslVersion >= 350) {
                            if (r.South.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "south", r.South.Data);
                            }
                            else {
                                this.AddToObjectProperties("south=" + r.South.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "northeast ")) {
                        r.NorthEast = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "northeast "));
                        if (this._gameAslVersion >= 350) {
                            if (r.NorthEast.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "northeast", r.NorthEast.Data);
                            }
                            else {
                                this.AddToObjectProperties("northeast=" + r.NorthEast.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "northwest ")) {
                        r.NorthWest = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "northwest "));
                        if (this._gameAslVersion >= 350) {
                            if (r.NorthWest.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "northwest", r.NorthWest.Data);
                            }
                            else {
                                this.AddToObjectProperties("northwest=" + r.NorthWest.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "southeast ")) {
                        r.SouthEast = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "southeast "));
                        if (this._gameAslVersion >= 350) {
                            if (r.SouthEast.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "southeast", r.SouthEast.Data);
                            }
                            else {
                                this.AddToObjectProperties("southeast=" + r.SouthEast.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "southwest ")) {
                        r.SouthWest = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "southwest "));
                        if (this._gameAslVersion >= 350) {
                            if (r.SouthWest.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "southwest", r.SouthWest.Data);
                            }
                            else {
                                this.AddToObjectProperties("southwest=" + r.SouthWest.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "up ")) {
                        r.Up = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "up "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Up.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "up", r.Up.Data);
                            }
                            else {
                                this.AddToObjectProperties("up=" + r.Up.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "down ")) {
                        r.Down = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "down "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Down.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "down", r.Down.Data);
                            }
                            else {
                                this.AddToObjectProperties("down=" + r.Down.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    }
                    else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "indescription ")) {
                        r.InDescription = this.GetParameter(this._lines[j], this._nullContext);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("indescription=" + r.InDescription, this._numberObjs, this._nullContext);
                        }
                    }
                    else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "look ")) {
                        r.Look = this.GetParameter(this._lines[j], this._nullContext);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("look=" + r.Look, this._numberObjs, this._nullContext);
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "prefix ")) {
                        r.Prefix = this.GetParameter(this._lines[j], this._nullContext);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("prefix=" + r.Prefix, this._numberObjs, this._nullContext);
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "script ")) {
                        r.Script = this.GetEverythingAfter(this._lines[j], "script ");
                        this.AddObjectAction(this._numberObjs, "script", r.Script);
                    }
                    else if (this.BeginsWith(this._lines[j], "command ")) {
                        r.NumberCommands = r.NumberCommands + 1;
                        if (!r.Commands)
                            r.Commands = [];
                        r.Commands[r.NumberCommands] = new UserDefinedCommandType();
                        r.Commands[r.NumberCommands].CommandText = this.GetParameter(this._lines[j], this._nullContext, false);
                        r.Commands[r.NumberCommands].CommandScript = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                    }
                    else if (this.BeginsWith(this._lines[j], "place ")) {
                        r.NumberPlaces = r.NumberPlaces + 1;
                        if (!r.Places)
                            r.Places = [];
                        r.Places[r.NumberPlaces] = new PlaceType();
                        var placeData = this.GetParameter(this._lines[j], this._nullContext);
                        var scp = InStr(placeData, ";");
                        if (scp == 0) {
                            r.Places[r.NumberPlaces].PlaceName = placeData;
                        }
                        else {
                            r.Places[r.NumberPlaces].PlaceName = Trim(Mid(placeData, scp + 1));
                            r.Places[r.NumberPlaces].Prefix = Trim(Left(placeData, scp - 1));
                        }
                        r.Places[r.NumberPlaces].Script = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                    }
                    else if (this.BeginsWith(this._lines[j], "use ")) {
                        r.NumberUse = r.NumberUse + 1;
                        if (!r.Use)
                            r.Use = [];
                        r.Use[r.NumberUse] = new ScriptText();
                        r.Use[r.NumberUse].Text = this.GetParameter(this._lines[j], this._nullContext);
                        r.Use[r.NumberUse].Script = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                    }
                    else if (this.BeginsWith(this._lines[j], "properties ")) {
                        this.AddToObjectProperties(this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                    }
                    else if (this.BeginsWith(this._lines[j], "type ")) {
                        this._objs[this._numberObjs].NumberTypesIncluded = this._objs[this._numberObjs].NumberTypesIncluded + 1;
                        if (!this._objs[this._numberObjs].TypesIncluded)
                            this._objs[this._numberObjs].TypesIncluded = [];
                        this._objs[this._numberObjs].TypesIncluded[this._objs[this._numberObjs].NumberTypesIncluded] = this.GetParameter(this._lines[j], this._nullContext);
                        var propertyData = this.GetPropertiesInType(this.GetParameter(this._lines[j], this._nullContext));
                        this.AddToObjectProperties(propertyData.Properties, this._numberObjs, this._nullContext);
                        for (var k = 1; k <= propertyData.NumberActions; k++) {
                            this.AddObjectAction(this._numberObjs, propertyData.Actions[k].ActionName, propertyData.Actions[k].Script);
                        }
                    }
                    else if (this.BeginsWith(this._lines[j], "action ")) {
                        this.AddToObjectActions(this.GetEverythingAfter(this._lines[j], "action "), this._numberObjs, this._nullContext);
                    }
                    else if (this.BeginsWith(this._lines[j], "beforeturn ")) {
                        r.BeforeTurnScript = r.BeforeTurnScript + this.GetEverythingAfter(this._lines[j], "beforeturn ") + "\n";
                    }
                    else if (this.BeginsWith(this._lines[j], "afterturn ")) {
                        r.AfterTurnScript = r.AfterTurnScript + this.GetEverythingAfter(this._lines[j], "afterturn ") + "\n";
                    }
                }
            }
        }
    };
    LegacyGame.prototype.SetUpSynonyms = function () {
        var block = this.GetDefineBlock("synonyms");
        this._numberSynonyms = 0;
        if (block.StartLine == 0 && block.EndLine == 0) {
            return;
        }
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            var eqp = InStr(this._lines[i], "=");
            if (eqp != 0) {
                var originalWordsList = Trim(Left(this._lines[i], eqp - 1));
                var convertWord = Trim(Mid(this._lines[i], eqp + 1));
                //Go through each word in OriginalWordsList (sep.
                //by ";"):
                originalWordsList = originalWordsList + ";";
                var pos = 1;
                do {
                    var endOfWord = InStrFrom(pos, originalWordsList, ";");
                    var thisWord = Trim(Mid(originalWordsList, pos, endOfWord - pos));
                    if (InStr(" " + convertWord + " ", " " + thisWord + " ") > 0) {
                        // Recursive synonym
                        this.LogASLError("Recursive synonym detected: '" + thisWord + "' converting to '" + convertWord + "'", LogType.WarningError);
                    }
                    else {
                        this._numberSynonyms = this._numberSynonyms + 1;
                        if (!this._synonyms)
                            this._synonyms = [];
                        this._synonyms[this._numberSynonyms] = new SynonymType();
                        this._synonyms[this._numberSynonyms].OriginalWord = thisWord;
                        this._synonyms[this._numberSynonyms].ConvertTo = convertWord;
                    }
                    pos = endOfWord + 1;
                } while (!(pos >= Len(originalWordsList)));
            }
        }
    };
    LegacyGame.prototype.SetUpTimers = function () {
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define timer ")) {
                this._numberTimers = this._numberTimers + 1;
                if (!this._timers)
                    this._timers = [];
                this._timers[this._numberTimers] = new TimerType();
                this._timers[this._numberTimers].TimerName = this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext);
                this._timers[this._numberTimers].TimerActive = false;
                for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                    if (this.BeginsWith(this._lines[j], "interval ")) {
                        this._timers[this._numberTimers].TimerInterval = parseInt(this.GetParameter(this._lines[j], this._nullContext));
                    }
                    else if (this.BeginsWith(this._lines[j], "action ")) {
                        this._timers[this._numberTimers].TimerAction = this.GetEverythingAfter(this._lines[j], "action ");
                    }
                    else if (Trim(LCase(this._lines[j])) == "enabled") {
                        this._timers[this._numberTimers].TimerActive = true;
                    }
                    else if (Trim(LCase(this._lines[j])) == "disabled") {
                        this._timers[this._numberTimers].TimerActive = false;
                    }
                }
            }
        }
    };
    LegacyGame.prototype.SetUpTurnScript = function () {
        var block = this.GetDefineBlock("game");
        this._beforeTurnScript = "";
        this._afterTurnScript = "";
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "beforeturn ")) {
                this._beforeTurnScript = this._beforeTurnScript + this.GetEverythingAfter(Trim(this._lines[i]), "beforeturn ") + "\n";
            }
            else if (this.BeginsWith(this._lines[i], "afterturn ")) {
                this._afterTurnScript = this._afterTurnScript + this.GetEverythingAfter(Trim(this._lines[i]), "afterturn ") + "\n";
            }
        }
    };
    LegacyGame.prototype.SetUpUserDefinedPlayerErrors = function () {
        // goes through "define game" block and sets stored error
        // messages accordingly
        var block = this.GetDefineBlock("game");
        var examineIsCustomised = false;
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "error ")) {
                var errorInfo = this.GetParameter(this._lines[i], this._nullContext, false);
                var scp = InStr(errorInfo, ";");
                var errorName = Left(errorInfo, scp - 1);
                var errorMsg = Trim(Mid(errorInfo, scp + 1));
                var currentError = 0;
                switch (errorName) {
                    case "badcommand":
                        currentError = PlayerError.BadCommand;
                        break;
                    case "badgo":
                        currentError = PlayerError.BadGo;
                        break;
                    case "badgive":
                        currentError = PlayerError.BadGive;
                        break;
                    case "badcharacter":
                        currentError = PlayerError.BadCharacter;
                        break;
                    case "noitem":
                        currentError = PlayerError.NoItem;
                        break;
                    case "itemunwanted":
                        currentError = PlayerError.ItemUnwanted;
                        break;
                    case "badlook":
                        currentError = PlayerError.BadLook;
                        break;
                    case "badthing":
                        currentError = PlayerError.BadThing;
                        break;
                    case "defaultlook":
                        currentError = PlayerError.DefaultLook;
                        break;
                    case "defaultspeak":
                        currentError = PlayerError.DefaultSpeak;
                        break;
                    case "baditem":
                        currentError = PlayerError.BadItem;
                        break;
                    case "baddrop":
                        currentError = PlayerError.BadDrop;
                        break;
                    case "defaultake":
                        if (this._gameAslVersion <= 280) {
                            currentError = PlayerError.BadTake;
                        }
                        else {
                            currentError = PlayerError.DefaultTake;
                        }
                        break;
                    case "baduse":
                        currentError = PlayerError.BadUse;
                        break;
                    case "defaultuse":
                        currentError = PlayerError.DefaultUse;
                        break;
                    case "defaultout":
                        currentError = PlayerError.DefaultOut;
                        break;
                    case "badplace":
                        currentError = PlayerError.BadPlace;
                        break;
                    case "badexamine":
                        if (this._gameAslVersion >= 310) {
                            currentError = PlayerError.BadExamine;
                        }
                        break;
                    case "defaultexamine":
                        currentError = PlayerError.DefaultExamine;
                        examineIsCustomised = true;
                        break;
                    case "badtake":
                        currentError = PlayerError.BadTake;
                        break;
                    case "cantdrop":
                        currentError = PlayerError.CantDrop;
                        break;
                    case "defaultdrop":
                        currentError = PlayerError.DefaultDrop;
                        break;
                    case "badpronoun":
                        currentError = PlayerError.BadPronoun;
                        break;
                    case "alreadyopen":
                        currentError = PlayerError.AlreadyOpen;
                        break;
                    case "alreadyclosed":
                        currentError = PlayerError.AlreadyClosed;
                        break;
                    case "cantopen":
                        currentError = PlayerError.CantOpen;
                        break;
                    case "cantclose":
                        currentError = PlayerError.CantClose;
                        break;
                    case "defaultopen":
                        currentError = PlayerError.DefaultOpen;
                        break;
                    case "defaultclose":
                        currentError = PlayerError.DefaultClose;
                        break;
                    case "badput":
                        currentError = PlayerError.BadPut;
                        break;
                    case "cantput":
                        currentError = PlayerError.CantPut;
                        break;
                    case "defaultput":
                        currentError = PlayerError.DefaultPut;
                        break;
                    case "cantremove":
                        currentError = PlayerError.CantRemove;
                        break;
                    case "alreadyput":
                        currentError = PlayerError.AlreadyPut;
                        break;
                    case "defaultremove":
                        currentError = PlayerError.DefaultRemove;
                        break;
                    case "locked":
                        currentError = PlayerError.Locked;
                        break;
                    case "defaultwait":
                        currentError = PlayerError.DefaultWait;
                        break;
                    case "alreadytaken":
                        currentError = PlayerError.AlreadyTaken;
                        break;
                }
                this._playerErrorMessageString[currentError] = errorMsg;
                if (currentError == PlayerError.DefaultLook && !examineIsCustomised) {
                    // If we're setting the default look message, and we've not already customised the
                    // default examine message, then set the default examine message to the same thing.
                    this._playerErrorMessageString[PlayerError.DefaultExamine] = errorMsg;
                }
            }
        }
    };
    LegacyGame.prototype.SetVisibility = function (thing, type, visible, ctx) {
        // Sets visibilty of objects and characters        
        if (this._gameAslVersion >= 280) {
            var found = false;
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(thing)) {
                    this._objs[i].Visible = visible;
                    if (visible) {
                        this.AddToObjectProperties("not invisible", i, ctx);
                    }
                    else {
                        this.AddToObjectProperties("invisible", i, ctx);
                    }
                    found = true;
                    break;
                }
            }
            if (!found) {
                this.LogASLError("Not found object '" + thing + "'", LogType.WarningError);
            }
        }
        else {
            // split ThingString into character name and room
            // (thingstring of form name@room)
            var atPos = InStr(thing, "@");
            var room;
            var name;
            // If no room specified, current room presumed
            if (atPos == 0) {
                room = this._currentRoom;
                name = thing;
            }
            else {
                name = Trim(Left(thing, atPos - 1));
                room = Trim(Right(thing, Len(thing) - atPos));
            }
            if (type == Thing.Character) {
                for (var i = 1; i <= this._numberChars; i++) {
                    if (LCase(this._chars[i].ContainerRoom) == LCase(room) && LCase(this._chars[i].ObjectName) == LCase(name)) {
                        this._chars[i].Visible = visible;
                        break;
                    }
                }
            }
            else if (type == Thing.Object) {
                for (var i = 1; i <= this._numberObjs; i++) {
                    if (LCase(this._objs[i].ContainerRoom) == LCase(room) && LCase(this._objs[i].ObjectName) == LCase(name)) {
                        this._objs[i].Visible = visible;
                        break;
                    }
                }
            }
        }
        this.UpdateObjectList(ctx);
    };
    LegacyGame.prototype.ShowPictureInText = function (filename) {
        if (!this._useStaticFrameForPictures) {
            this._player.ShowPicture(filename);
        }
        else {
            // Workaround for a particular game which expects pictures to be in a popup window -
            // use the static picture frame feature so that image is not cleared
            this._player.SetPanelContents("<img src=\"" + this._player.GetURL(filename) + "\" onload=\"setPanelHeight()\"/>");
        }
    };
    LegacyGame.prototype.ShowRoomInfoV2 = function (room) {
        // ShowRoomInfo for Quest 2.x games
        var roomDisplayText = "";
        var descTagExist = false;
        var gameBlock;
        var charsViewable;
        var charsFound = 0;
        var prefixAliasNoFormat;
        var prefix;
        var prefixAlias;
        var inDesc;
        var aliasName = "";
        var charList;
        var foundLastComma = 0;
        var cp = 0;
        var ncp = 0;
        var noFormatObjsViewable;
        var objsViewable = "";
        var objsFound = 0;
        var objListString;
        var noFormatObjListString;
        var possDir;
        var nsew;
        var doorways;
        var places;
        var place;
        var aliasOut = "";
        var placeNoFormat;
        var descLine = "";
        var lastComma = 0;
        var oldLastComma = 0;
        var defineBlock = 0;
        var lookString = "";
        gameBlock = this.GetDefineBlock("game");
        this._currentRoom = room;
        //find the room
        var roomBlock;
        roomBlock = this.DefineBlockParam("room", room);
        var finishedFindingCommas = false;
        charsViewable = "";
        charsFound = 0;
        //see if room has an alias
        for (var i = roomBlock.StartLine + 1; i <= roomBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "alias")) {
                aliasName = this.GetParameter(this._lines[i], this._nullContext);
                i = roomBlock.EndLine;
            }
        }
        if (aliasName == "") {
            aliasName = room;
        }
        //see if room has a prefix
        prefix = this.FindStatement(roomBlock, "prefix");
        if (prefix == "") {
            prefixAlias = "|cr" + aliasName + "|cb";
            prefixAliasNoFormat = aliasName;
        }
        else {
            prefixAlias = prefix + " |cr" + aliasName + "|cb";
            prefixAliasNoFormat = prefix + " " + aliasName;
        }
        //print player's location
        //find indescription line:
        inDesc = "unfound";
        for (var i = roomBlock.StartLine + 1; i <= roomBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "indescription")) {
                inDesc = Trim(this.GetParameter(this._lines[i], this._nullContext));
                i = roomBlock.EndLine;
            }
        }
        if (inDesc != "unfound") {
            // Print player's location according to indescription:
            if (Right(inDesc, 1) == ":") {
                // if line ends with a colon, add place name:
                roomDisplayText = roomDisplayText + Left(inDesc, Len(inDesc) - 1) + " " + prefixAlias + ".\n";
            }
            else {
                // otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText + inDesc + "\n";
            }
        }
        else {
            // if no indescription line, print the default.
            roomDisplayText = roomDisplayText + "You are in " + prefixAlias + ".\n";
        }
        this._player.LocationUpdated(prefixAliasNoFormat);
        this.SetStringContents("quest.formatroom", prefixAliasNoFormat, this._nullContext);
        //FIND CHARACTERS ===
        for (var i = 1; i <= this._numberChars; i++) {
            if (this._chars[i].ContainerRoom == room && this._chars[i].Exists && this._chars[i].Visible) {
                charsViewable = charsViewable + this._chars[i].Prefix + "|b" + this._chars[i].ObjectName + "|xb" + this._chars[i].Suffix + ", ";
                charsFound = charsFound + 1;
            }
        }
        if (charsFound == 0) {
            charsViewable = "There is nobody here.";
            this.SetStringContents("quest.characters", "", this._nullContext);
        }
        else {
            //chop off final comma and add full stop (.)
            charList = Left(charsViewable, Len(charsViewable) - 2);
            this.SetStringContents("quest.characters", charList, this._nullContext);
            //if more than one character, add "and" before
            //last one:
            cp = InStr(charList, ",");
            if (cp != 0) {
                foundLastComma = 0;
                do {
                    ncp = InStrFrom(cp + 1, charList, ",");
                    if (ncp == 0) {
                        foundLastComma = 1;
                    }
                    else {
                        cp = ncp;
                    }
                } while (!(foundLastComma == 1));
                charList = Trim(Left(charList, cp - 1)) + " and " + Trim(Mid(charList, cp + 1));
            }
            charsViewable = "You can see " + charList + " here.";
        }
        roomDisplayText = roomDisplayText + charsViewable + "\n";
        //FIND OBJECTS
        noFormatObjsViewable = "";
        for (var i = 1; i <= this._numberObjs; i++) {
            if (this._objs[i].ContainerRoom == room && this._objs[i].Exists && this._objs[i].Visible) {
                objsViewable = objsViewable + this._objs[i].Prefix + "|b" + this._objs[i].ObjectName + "|xb" + this._objs[i].Suffix + ", ";
                noFormatObjsViewable = noFormatObjsViewable + this._objs[i].Prefix + this._objs[i].ObjectName + ", ";
                objsFound = objsFound + 1;
            }
        }
        var finishedLoop = false;
        if (objsFound != 0) {
            objListString = Left(objsViewable, Len(objsViewable) - 2);
            noFormatObjListString = Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2);
            cp = InStr(objListString, ",");
            if (cp != 0) {
                do {
                    ncp = InStrFrom(cp + 1, objListString, ",");
                    if (ncp == 0) {
                        finishedLoop = true;
                    }
                    else {
                        cp = ncp;
                    }
                } while (!(finishedLoop));
                objListString = Trim(Left(objListString, cp - 1) + " and " + Trim(Mid(objListString, cp + 1)));
            }
            objsViewable = "There is " + objListString + " here.";
            this.SetStringContents("quest.objects", Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2), this._nullContext);
            this.SetStringContents("quest.formatobjects", objListString, this._nullContext);
            roomDisplayText = roomDisplayText + objsViewable + "\n";
        }
        else {
            this.SetStringContents("quest.objects", "", this._nullContext);
            this.SetStringContents("quest.formatobjects", "", this._nullContext);
        }
        //FIND DOORWAYS
        doorways = "";
        nsew = "";
        places = "";
        possDir = "";
        for (var i = roomBlock.StartLine + 1; i <= roomBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "out")) {
                doorways = this.GetParameter(this._lines[i], this._nullContext);
            }
            if (this.BeginsWith(this._lines[i], "north ")) {
                nsew = nsew + "|bnorth|xb, ";
                possDir = possDir + "n";
            }
            else if (this.BeginsWith(this._lines[i], "south ")) {
                nsew = nsew + "|bsouth|xb, ";
                possDir = possDir + "s";
            }
            else if (this.BeginsWith(this._lines[i], "east ")) {
                nsew = nsew + "|beast|xb, ";
                possDir = possDir + "e";
            }
            else if (this.BeginsWith(this._lines[i], "west ")) {
                nsew = nsew + "|bwest|xb, ";
                possDir = possDir + "w";
            }
            else if (this.BeginsWith(this._lines[i], "northeast ")) {
                nsew = nsew + "|bnortheast|xb, ";
                possDir = possDir + "a";
            }
            else if (this.BeginsWith(this._lines[i], "northwest ")) {
                nsew = nsew + "|bnorthwest|xb, ";
                possDir = possDir + "b";
            }
            else if (this.BeginsWith(this._lines[i], "southeast ")) {
                nsew = nsew + "|bsoutheast|xb, ";
                possDir = possDir + "c";
            }
            else if (this.BeginsWith(this._lines[i], "southwest ")) {
                nsew = nsew + "|bsouthwest|xb, ";
                possDir = possDir + "d";
            }
            if (this.BeginsWith(this._lines[i], "place")) {
                //remove any prefix semicolon from printed text
                place = this.GetParameter(this._lines[i], this._nullContext);
                placeNoFormat = place;
                //Used in object list - no formatting or prefix
                if (InStr(place, ";") > 0) {
                    placeNoFormat = Right(place, Len(place) - (InStr(place, ";") + 1));
                    place = Trim(Left(place, InStr(place, ";") - 1)) + " |b" + Right(place, Len(place) - (InStr(place, ";") + 1)) + "|xb";
                }
                else {
                    place = "|b" + place + "|xb";
                }
                places = places + place + ", ";
            }
        }
        var outside;
        if (doorways != "") {
            //see if outside has an alias
            outside = this.DefineBlockParam("room", doorways);
            for (var i = outside.StartLine + 1; i <= outside.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "alias")) {
                    aliasOut = this.GetParameter(this._lines[i], this._nullContext);
                    i = outside.EndLine;
                }
            }
            if (aliasOut == "") {
                aliasOut = doorways;
            }
            roomDisplayText = roomDisplayText + "You can go out to " + aliasOut + ".\n";
            possDir = possDir + "o";
            this.SetStringContents("quest.doorways.out", aliasOut, this._nullContext);
        }
        else {
            this.SetStringContents("quest.doorways.out", "", this._nullContext);
        }
        var finished = false;
        if (nsew != "") {
            //strip final comma
            nsew = Left(nsew, Len(nsew) - 2);
            cp = InStr(nsew, ",");
            if (cp != 0) {
                finished = false;
                do {
                    ncp = InStrFrom(cp + 1, nsew, ",");
                    if (ncp == 0) {
                        finished = true;
                    }
                    else {
                        cp = ncp;
                    }
                } while (!(finished));
                nsew = Trim(Left(nsew, cp - 1)) + " or " + Trim(Mid(nsew, cp + 1));
            }
            roomDisplayText = roomDisplayText + "You can go " + nsew + ".\n";
            this.SetStringContents("quest.doorways.dirs", nsew, this._nullContext);
        }
        else {
            this.SetStringContents("quest.doorways.dirs", "", this._nullContext);
        }
        this.UpdateDirButtons(possDir, this._nullContext);
        if (places != "") {
            //strip final comma
            places = Left(places, Len(places) - 2);
            //if there is still a comma here, there is more than
            //one place, so add the word "or" before the last one.
            if (InStr(places, ",") > 0) {
                lastComma = 0;
                finishedFindingCommas = false;
                do {
                    oldLastComma = lastComma;
                    lastComma = InStrFrom(lastComma + 1, places, ",");
                    if (lastComma == 0) {
                        finishedFindingCommas = true;
                        lastComma = oldLastComma;
                    }
                } while (!(finishedFindingCommas));
                places = Left(places, lastComma) + " or" + Right(places, Len(places) - lastComma);
            }
            roomDisplayText = roomDisplayText + "You can go to " + places + ".\n";
            this.SetStringContents("quest.doorways.places", places, this._nullContext);
        }
        else {
            this.SetStringContents("quest.doorways.places", "", this._nullContext);
        }
        //Print RoomDisplayText if there is no "description" tag,
        //otherwise execute the description tag information:
        // First, look in the "define room" block:
        descTagExist = false;
        for (var i = roomBlock.StartLine + 1; i <= roomBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "description ")) {
                descLine = this._lines[i];
                descTagExist = true;
                break;
            }
        }
        if (!descTagExist) {
            //Look in the "define game" block:
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "description ")) {
                    descLine = this._lines[i];
                    descTagExist = true;
                    break;
                }
            }
        }
        if (!descTagExist) {
            //Remove final newline:
            roomDisplayText = Left(roomDisplayText, Len(roomDisplayText) - 2);
            this.Print(roomDisplayText, this._nullContext);
        }
        else {
            //execute description tag:
            //If no script, just print the tag's parameter.
            //Otherwise, execute it as ASL script:
            descLine = this.GetEverythingAfter(Trim(descLine), "description ");
            if (Left(descLine, 1) == "<") {
                this.Print(this.GetParameter(descLine, this._nullContext), this._nullContext);
            }
            else {
                this.ExecuteScript(descLine, this._nullContext);
            }
        }
        this.UpdateObjectList(this._nullContext);
        defineBlock = 0;
        for (var i = roomBlock.StartLine + 1; i <= roomBlock.EndLine - 1; i++) {
            // don't get the 'look' statements in nested define blocks
            if (this.BeginsWith(this._lines[i], "define")) {
                defineBlock = defineBlock + 1;
            }
            if (this.BeginsWith(this._lines[i], "end define")) {
                defineBlock = defineBlock - 1;
            }
            if (this.BeginsWith(this._lines[i], "look") && defineBlock == 0) {
                lookString = this.GetParameter(this._lines[i], this._nullContext);
                i = roomBlock.EndLine;
            }
        }
        if (lookString != "") {
            this.Print(lookString, this._nullContext);
        }
    };
    LegacyGame.prototype.Speak = function (text) {
        this._player.Speak(text);
    };
    LegacyGame.prototype.AddToObjectList = function (objList, exitList, name, type) {
        name = this.CapFirst(name);
        if (type == Thing.Room) {
            objList.push(new ListData(name, this._listVerbs[ListType.ExitsList]));
            exitList.push(new ListData(name, this._listVerbs[ListType.ExitsList]));
        }
        else {
            objList.push(new ListData(name, this._listVerbs[ListType.ObjectsList]));
        }
    };
    LegacyGame.prototype.ExecExec = function (scriptLine, ctx) {
        if (ctx.CancelExec) {
            return;
        }
        var execLine = this.GetParameter(scriptLine, ctx);
        var newCtx = this.CopyContext(ctx);
        newCtx.StackCounter = newCtx.StackCounter + 1;
        if (newCtx.StackCounter > 500) {
            this.LogASLError("Out of stack space running '" + scriptLine + "' - infinite loop?", LogType.WarningError);
            ctx.CancelExec = true;
            return;
        }
        if (this._gameAslVersion >= 310) {
            newCtx.AllowRealNamesInCommand = true;
        }
        if (InStr(execLine, ";") == 0) {
            try {
                this.ExecCommand(execLine, newCtx, false);
            }
            catch (e) {
                this.LogASLError("Internal error running '" + scriptLine + "'", LogType.WarningError);
                ctx.CancelExec = true;
            }
        }
        else {
            var scp = InStr(execLine, ";");
            var ex = Trim(Left(execLine, scp - 1));
            var r = Trim(Mid(execLine, scp + 1));
            if (r == "normal") {
                this.ExecCommand(ex, newCtx, false, false);
            }
            else {
                this.LogASLError("Unrecognised post-command parameter in " + Trim(scriptLine), LogType.WarningError);
            }
        }
    };
    LegacyGame.prototype.ExecSetString = function (info, ctx) {
        // Sets string contents from a script parameter.
        // Eg <string1;contents> sets string variable string1
        // to "contents"
        var scp = InStr(info, ";");
        var name = Trim(Left(info, scp - 1));
        var value = Mid(info, scp + 1);
        if (IsNumeric(name)) {
            this.LogASLError("Invalid string name '" + name + "' - string names cannot be numeric", LogType.WarningError);
            return;
        }
        if (this._gameAslVersion >= 281) {
            value = Trim(value);
            if (Left(value, 1) == "[" && Right(value, 1) == "]") {
                value = Mid(value, 2, Len(value) - 2);
            }
        }
        var idx = this.GetArrayIndex(name, ctx);
        this.SetStringContents(idx.Name, value, ctx, idx.Index);
    };
    LegacyGame.prototype.ExecUserCommand = function (cmd, ctx, libCommands) {
        if (libCommands === void 0) { libCommands = false; }
        //Executes a user-defined command. If unavailable, returns
        //false.
        var curCmd;
        var commandList;
        var script = "";
        var commandTag;
        var commandLine = "";
        var foundCommand = false;
        //First, check for a command in the current room block
        var roomId = this.GetRoomId(this._currentRoom, ctx);
        // RoomID is 0 if we have no rooms in the game. Unlikely, but we get an RTE otherwise.
        if (roomId != 0) {
            var r = this._rooms[roomId];
            for (var i = 1; i <= r.NumberCommands; i++) {
                commandList = r.Commands[i].CommandText;
                var ep = 0;
                do {
                    ep = InStr(commandList, ";");
                    if (ep == 0) {
                        curCmd = commandList;
                    }
                    else {
                        curCmd = Trim(Left(commandList, ep - 1));
                        commandList = Trim(Mid(commandList, ep + 1));
                    }
                    if (this.IsCompatible(LCase(cmd), LCase(curCmd))) {
                        commandLine = curCmd;
                        script = r.Commands[i].CommandScript;
                        foundCommand = true;
                        ep = 0;
                        break;
                    }
                } while (!(ep == 0));
            }
        }
        if (!libCommands) {
            commandTag = "command";
        }
        else {
            commandTag = "lib command";
        }
        if (!foundCommand) {
            // Check "define game" block
            var block = this.GetDefineBlock("game");
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], commandTag)) {
                    commandList = this.GetParameter(this._lines[i], ctx, false);
                    var ep = 0;
                    do {
                        ep = InStr(commandList, ";");
                        if (ep == 0) {
                            curCmd = commandList;
                        }
                        else {
                            curCmd = Trim(Left(commandList, ep - 1));
                            commandList = Trim(Mid(commandList, ep + 1));
                        }
                        if (this.IsCompatible(LCase(cmd), LCase(curCmd))) {
                            commandLine = curCmd;
                            var ScriptPos = InStr(this._lines[i], ">") + 1;
                            script = Trim(Mid(this._lines[i], ScriptPos));
                            foundCommand = true;
                            ep = 0;
                            break;
                        }
                    } while (!(ep == 0));
                }
            }
        }
        if (foundCommand) {
            if (this.GetCommandParameters(cmd, commandLine, ctx)) {
                this.ExecuteScript(script, ctx);
            }
        }
        return foundCommand;
    };
    LegacyGame.prototype.ExecuteChoose = function (section, ctx) {
        this.ExecuteScript(this.SetUpChoiceForm(section, ctx), ctx);
    };
    LegacyGame.prototype.GetCommandParameters = function (test, required, ctx) {
        //Gets parameters from line. For example, if 'required'
        //is "read #1#" and 'test' is "read sign", #1# returns
        //"sign".
        // Returns FALSE if #@object# form used and object doesn't
        // exist.
        var chunksBegin;
        var chunksEnd;
        var varName;
        var var2Pos = 0;
        // Add dots before and after both strings. This fudge
        // stops problems caused when variables are right at the
        // beginning or end of a line.
        // PostScript: well, it used to, I'm not sure if it's really
        // required now though....
        // As of Quest 4.0 we use the � character rather than a dot.
        test = "�" + Trim(test) + "�";
        required = "�" + required + "�";
        //Go through RequiredLine in chunks going up to variables.
        var currentReqLinePos = 1;
        var currentTestLinePos = 1;
        var finished = false;
        var numberChunks = 0;
        do {
            var nextVarPos = InStrFrom(currentReqLinePos, required, "#");
            var currentVariable = "";
            if (nextVarPos == 0) {
                finished = true;
                nextVarPos = Len(required) + 1;
            }
            else {
                var2Pos = InStrFrom(nextVarPos + 1, required, "#");
                currentVariable = Mid(required, nextVarPos + 1, (var2Pos - 1) - nextVarPos);
            }
            var checkChunk = Mid(required, currentReqLinePos, (nextVarPos - 1) - (currentReqLinePos - 1));
            var chunkBegin = InStrFrom(currentTestLinePos, LCase(test), LCase(checkChunk));
            var chunkEnd = chunkBegin + Len(checkChunk);
            numberChunks = numberChunks + 1;
            if (!chunksBegin)
                chunksBegin = [];
            if (!chunksEnd)
                chunksEnd = [];
            if (!varName)
                varName = [];
            chunksBegin[numberChunks] = chunkBegin;
            chunksEnd[numberChunks] = chunkEnd;
            varName[numberChunks] = currentVariable;
            //Get to end of variable name
            currentReqLinePos = var2Pos + 1;
            currentTestLinePos = chunkEnd;
        } while (!(finished));
        var success = true;
        //Return values to string variable
        for (var i = 1; i <= numberChunks - 1; i++) {
            var arrayIndex = 0;
            // If VarName contains array name, change to index number
            if (InStr(varName[i], "[") > 0) {
                var indexResult = this.GetArrayIndex(varName[i], ctx);
                varName[i] = indexResult.Name;
                arrayIndex = indexResult.Index;
            }
            else {
                arrayIndex = 0;
            }
            var curChunk = Mid(test, chunksEnd[i], chunksBegin[i + 1] - chunksEnd[i]);
            if (this.BeginsWith(varName[i], "@")) {
                varName[i] = this.GetEverythingAfter(varName[i], "@");
                var id = this.Disambiguate(curChunk, this._currentRoom + ";" + "inventory", ctx);
                if (id == -1) {
                    if (this._gameAslVersion >= 391) {
                        this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                    else {
                        this.PlayerErrorMessage(PlayerError.BadItem, ctx);
                    }
                    // The Mid$(...,2) and Left$(...,2) removes the initial/final "."
                    this._badCmdBefore = Mid(Trim(Left(test, chunksEnd[i] - 1)), 2);
                    this._badCmdAfter = Trim(Mid(test, chunksBegin[i + 1]));
                    this._badCmdAfter = Left(this._badCmdAfter, Len(this._badCmdAfter) - 1);
                    success = false;
                }
                else if (id == -2) {
                    this._badCmdBefore = Trim(Left(test, chunksEnd[i] - 1));
                    this._badCmdAfter = Trim(Mid(test, chunksBegin[i + 1]));
                    success = false;
                }
                else {
                    this.SetStringContents(varName[i], this._objs[id].ObjectName, ctx, arrayIndex);
                }
            }
            else {
                this.SetStringContents(varName[i], curChunk, ctx, arrayIndex);
            }
        }
        return success;
    };
    LegacyGame.prototype.GetGender = function (character, capitalise, ctx) {
        var result;
        if (this._gameAslVersion >= 281) {
            result = this._objs[this.GetObjectIdNoAlias(character)].Gender;
        }
        else {
            var resultLine = this.RetrLine("character", character, "gender", ctx);
            if (resultLine == "<unfound>") {
                result = "it ";
            }
            else {
                result = this.GetParameter(resultLine, ctx) + " ";
            }
        }
        if (capitalise) {
            result = UCase(Left(result, 1)) + Right(result, Len(result) - 1);
        }
        return result;
    };
    LegacyGame.prototype.GetStringContents = function (name, ctx) {
        var returnAlias = false;
        var arrayIndex = 0;
        // Check for property shortcut
        var cp = InStr(name, ":");
        if (cp != 0) {
            var objName = Trim(Left(name, cp - 1));
            var propName = Trim(Mid(name, cp + 1));
            var obp = InStr(objName, "(");
            if (obp != 0) {
                var cbp = InStrFrom(obp, objName, ")");
                if (cbp != 0) {
                    objName = this.GetStringContents(Mid(objName, obp + 1, (cbp - obp) - 1), ctx);
                }
            }
            return this.GetObjectProperty(propName, this.GetObjectIdNoAlias(objName));
        }
        if (Left(name, 1) == "@") {
            returnAlias = true;
            name = Mid(name, 2);
        }
        if (InStr(name, "[") != 0 && InStr(name, "]") != 0) {
            var bp = InStr(name, "[");
            var ep = InStr(name, "]");
            var arrayIndexData = Mid(name, bp + 1, (ep - bp) - 1);
            if (IsNumeric(arrayIndexData)) {
                arrayIndex = parseInt(arrayIndexData);
            }
            else {
                arrayIndex = this.GetNumericContents(arrayIndexData, ctx);
                if (arrayIndex == -32767) {
                    this.LogASLError("Array index in '" + name + "' is not valid. An array index must be either a number or a numeric variable (without surrounding '%' characters)", LogType.WarningError);
                    return "";
                }
            }
            name = Left(name, bp - 1);
        }
        // First, see if the string already exists. If it does,
        // get its contents. If not, generate an error.
        var exists = false;
        var id = 0;
        if (this._numberStringVariables > 0) {
            for (var i = 1; i <= this._numberStringVariables; i++) {
                if (LCase(this._stringVariable[i].VariableName) == LCase(name)) {
                    id = i;
                    exists = true;
                    break;
                }
            }
        }
        if (!exists) {
            this.LogASLError("No string variable '" + name + "' defined.", LogType.WarningError);
            return "";
        }
        if (arrayIndex > this._stringVariable[id].VariableUBound) {
            this.LogASLError("Array index of '" + name + "[" + Trim(Str(arrayIndex)) + "]' too big.", LogType.WarningError);
            return "";
        }
        // Now, set the contents
        if (!returnAlias) {
            return this._stringVariable[id].VariableContents[arrayIndex];
        }
        else {
            return this._objs[this.GetObjectIdNoAlias(this._stringVariable[id].VariableContents[arrayIndex])].ObjectAlias;
        }
    };
    LegacyGame.prototype.IsAvailable = function (thingName, type, ctx) {
        // Returns availability of object/character
        // split ThingString into character name and room
        // (thingstring of form name@room)
        var room;
        var name;
        var atPos = InStr(thingName, "@");
        // If no room specified, current room presumed
        if (atPos == 0) {
            room = this._currentRoom;
            name = thingName;
        }
        else {
            name = Trim(Left(thingName, atPos - 1));
            room = Trim(Right(thingName, Len(thingName) - atPos));
        }
        if (type == Thing.Character) {
            for (var i = 1; i <= this._numberChars; i++) {
                if (LCase(this._chars[i].ContainerRoom) == LCase(room) && LCase(this._chars[i].ObjectName) == LCase(name)) {
                    return this._chars[i].Exists;
                }
            }
        }
        else if (type == Thing.Object) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ContainerRoom) == LCase(room) && LCase(this._objs[i].ObjectName) == LCase(name)) {
                    return this._objs[i].Exists;
                }
            }
        }
    };
    LegacyGame.prototype.IsCompatible = function (test, required) {
        //Tests to see if 'test' "works" with 'required'.
        //For example, if 'required' = "read #text#", then the
        //tests of "read book" and "read sign" are compatible.
        var var2Pos = 0;
        // This avoids "xxx123" being compatible with "xxx".
        test = "^" + Trim(test) + "^";
        required = "^" + required + "^";
        //Go through RequiredLine in chunks going up to variables.
        var currentReqLinePos = 1;
        var currentTestLinePos = 1;
        var finished = false;
        do {
            var nextVarPos = InStrFrom(currentReqLinePos, required, "#");
            if (nextVarPos == 0) {
                nextVarPos = Len(required) + 1;
                finished = true;
            }
            else {
                var2Pos = InStrFrom(nextVarPos + 1, required, "#");
            }
            var checkChunk = Mid(required, currentReqLinePos, (nextVarPos - 1) - (currentReqLinePos - 1));
            if (InStrFrom(currentTestLinePos, test, checkChunk) != 0) {
                currentTestLinePos = InStrFrom(currentTestLinePos, test, checkChunk) + Len(checkChunk);
            }
            else {
                return false;
            }
            //Skip to end of variable
            currentReqLinePos = var2Pos + 1;
        } while (!(finished));
        return true;
    };
    LegacyGame.prototype.OpenGame = function (filename, onSuccess, onFailure) {
        var cdatb = false;
        var result = false;
        var visible = false;
        var room;
        var fileData = "";
        var savedQsgVersion;
        var data = "";
        var name;
        var scp = 0;
        var cdat = 0;
        var scp2 = 0;
        var scp3 = 0;
        var lines = null;
        this._gameLoadMethod = "loaded";
        var prevQsgVersion = false;
        // TODO
        //if (this._data == null) {
        //    fileData = System.IO.File.ReadAllText(filename, System.Text.Encoding.GetEncoding(1252));
        //} else {
        //    fileData = System.Text.Encoding.GetEncoding(1252).GetString(this._data.Data);
        //}
        // Check version
        savedQsgVersion = Left(fileData, 10);
        if (this.BeginsWith(savedQsgVersion, "QUEST200.1")) {
            prevQsgVersion = true;
        }
        else if (!this.BeginsWith(savedQsgVersion, "QUEST300")) {
            onFailure();
            return;
        }
        if (prevQsgVersion) {
            lines = fileData.split("\n");
            this._gameFileName = lines[1];
        }
        else {
            this.InitFileData(fileData);
            this.GetNextChunk();
            if (this._data == null) {
                this._gameFileName = this.GetNextChunk();
            }
            else {
                this.GetNextChunk();
                this._gameFileName = this._data.SourceFile;
            }
        }
        // TODO
        //if (this._data == null && !System.IO.File.Exists(this._gameFileName)) {
        //    this._gameFileName = this._player.GetNewGameFile(this._gameFileName, "*.asl;*.cas;*.zip");
        //    if (this._gameFileName == "") {
        //        return false;
        //    }
        //}
        this.InitialiseGame(this._gameFileName, true, function () {
            if (!prevQsgVersion) {
                // Open Quest 3.0 saved game file
                this._gameLoading = true;
                this.RestoreGameData(fileData);
                this._gameLoading = false;
            }
            else {
                // Open Quest 2.x saved game file
                this._currentRoom = lines[3];
                // Start at line 5 as line 4 is always "!c"
                var lineNumber = 5;
                do {
                    data = lines[lineNumber];
                    lineNumber += 1;
                    if (data != "!i") {
                        scp = InStr(data, ";");
                        name = Trim(Left(data, scp - 1));
                        cdat = parseInt(Right(data, Len(data) - scp));
                        for (var i = 1; i <= this._numCollectables; i++) {
                            if (this._collectables[i].Name == name) {
                                this._collectables[i].Value = cdat;
                                i = this._numCollectables;
                            }
                        }
                    }
                } while (!(data == "!i"));
                do {
                    data = lines[lineNumber];
                    lineNumber += 1;
                    if (data != "!o") {
                        scp = InStr(data, ";");
                        name = Trim(Left(data, scp - 1));
                        cdatb = this.IsYes(Right(data, Len(data) - scp));
                        for (var i = 1; i <= this._numberItems; i++) {
                            if (this._items[i].Name == name) {
                                this._items[i].Got = cdatb;
                                i = this._numberItems;
                            }
                        }
                    }
                } while (!(data == "!o"));
                do {
                    data = lines[lineNumber];
                    lineNumber += 1;
                    if (data != "!p") {
                        scp = InStr(data, ";");
                        scp2 = InStrFrom(scp + 1, data, ";");
                        scp3 = InStrFrom(scp2 + 1, data, ";");
                        name = Trim(Left(data, scp - 1));
                        cdatb = this.IsYes(Mid(data, scp + 1, (scp2 - scp) - 1));
                        visible = this.IsYes(Mid(data, scp2 + 1, (scp3 - scp2) - 1));
                        room = Trim(Mid(data, scp3 + 1));
                        for (var i = 1; i <= this._numberObjs; i++) {
                            if (this._objs[i].ObjectName == name && !this._objs[i].Loaded) {
                                this._objs[i].Exists = cdatb;
                                this._objs[i].Visible = visible;
                                this._objs[i].ContainerRoom = room;
                                this._objs[i].Loaded = true;
                                i = this._numberObjs;
                            }
                        }
                    }
                } while (!(data == "!p"));
                do {
                    data = lines[lineNumber];
                    lineNumber += 1;
                    if (data != "!s") {
                        scp = InStr(data, ";");
                        scp2 = InStrFrom(scp + 1, data, ";");
                        scp3 = InStrFrom(scp2 + 1, data, ";");
                        name = Trim(Left(data, scp - 1));
                        cdatb = this.IsYes(Mid(data, scp + 1, (scp2 - scp) - 1));
                        visible = this.IsYes(Mid(data, scp2 + 1, (scp3 - scp2) - 1));
                        room = Trim(Mid(data, scp3 + 1));
                        for (var i = 1; i <= this._numberChars; i++) {
                            if (this._chars[i].ObjectName == name) {
                                this._chars[i].Exists = cdatb;
                                this._chars[i].Visible = visible;
                                this._chars[i].ContainerRoom = room;
                                i = this._numberChars;
                            }
                        }
                    }
                } while (!(data == "!s"));
                do {
                    data = lines[lineNumber];
                    lineNumber += 1;
                    if (data != "!n") {
                        scp = InStr(data, ";");
                        name = Trim(Left(data, scp - 1));
                        data = Right(data, Len(data) - scp);
                        this.SetStringContents(name, data, this._nullContext);
                    }
                } while (!(data == "!n"));
                do {
                    data = lines[lineNumber];
                    lineNumber += 1;
                    if (data != "!e") {
                        scp = InStr(data, ";");
                        name = Trim(Left(data, scp - 1));
                        data = Right(data, Len(data) - scp);
                        this.SetNumericVariableContents(name, Val(data), this._nullContext);
                    }
                } while (!(data == "!e"));
            }
            this._saveGameFile = filename;
            onSuccess();
        }, onFailure);
    };
    LegacyGame.prototype.SaveGame = function (filename, saveFile) {
        if (saveFile === void 0) { saveFile = true; }
        var ctx = new Context();
        var saveData;
        if (this._gameAslVersion >= 391) {
            this.ExecuteScript(this._beforeSaveScript, ctx);
        }
        if (this._gameAslVersion >= 280) {
            saveData = this.MakeRestoreData();
        }
        else {
            saveData = this.MakeRestoreDataV2();
        }
        // TODO
        //if (saveFile) {
        //    System.IO.File.WriteAllText(filename, saveData, System.Text.Encoding.GetEncoding(1252));
        //}
        this._saveGameFile = filename;
        //return System.Text.Encoding.GetEncoding(1252).GetBytes(saveData);
        return null;
    };
    LegacyGame.prototype.MakeRestoreDataV2 = function () {
        var lines = [];
        var i = 0;
        lines.push("QUEST200.1");
        lines.push(this.GetOriginalFilenameForQSG());
        lines.push(this._gameName);
        lines.push(this._currentRoom);
        lines.push("!c");
        for (var i = 1; i <= this._numCollectables; i++) {
            lines.push(this._collectables[i].Name + ";" + Str(this._collectables[i].Value));
        }
        lines.push("!i");
        for (var i = 1; i <= this._numberItems; i++) {
            lines.push(this._items[i].Name + ";" + this.YesNo(this._items[i].Got));
        }
        lines.push("!o");
        for (var i = 1; i <= this._numberObjs; i++) {
            lines.push(this._objs[i].ObjectName + ";" + this.YesNo(this._objs[i].Exists) + ";" + this.YesNo(this._objs[i].Visible) + ";" + this._objs[i].ContainerRoom);
        }
        lines.push("!p");
        for (var i = 1; i <= this._numberChars; i++) {
            lines.push(this._chars[i].ObjectName + ";" + this.YesNo(this._chars[i].Exists) + ";" + this.YesNo(this._chars[i].Visible) + ";" + this._chars[i].ContainerRoom);
        }
        lines.push("!s");
        for (var i = 1; i <= this._numberStringVariables; i++) {
            lines.push(this._stringVariable[i].VariableName + ";" + this._stringVariable[i].VariableContents[0]);
        }
        lines.push("!n");
        for (var i = 1; i <= this._numberNumericVariables; i++) {
            lines.push(this._numericVariable[i].VariableName + ";" + Str(parseFloat(this._numericVariable[i].VariableContents[0])));
        }
        lines.push("!e");
        return lines.join("\n");
    };
    LegacyGame.prototype.SetAvailability = function (thingString, exists, ctx, type) {
        if (type === void 0) { type = Thing.Object; }
        // Sets availability of objects (and characters in ASL<281)
        if (this._gameAslVersion >= 281) {
            var found = false;
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(thingString)) {
                    this._objs[i].Exists = exists;
                    if (exists) {
                        this.AddToObjectProperties("not hidden", i, ctx);
                    }
                    else {
                        this.AddToObjectProperties("hidden", i, ctx);
                    }
                    found = true;
                    break;
                }
            }
            if (!found) {
                this.LogASLError("Not found object '" + thingString + "'", LogType.WarningError);
            }
        }
        else {
            // split ThingString into character name and room
            // (thingstring of form name@room)
            var room;
            var name;
            var atPos = InStr(thingString, "@");
            // If no room specified, currentroom presumed
            if (atPos == 0) {
                room = this._currentRoom;
                name = thingString;
            }
            else {
                name = Trim(Left(thingString, atPos - 1));
                room = Trim(Right(thingString, Len(thingString) - atPos));
            }
            if (type == Thing.Character) {
                for (var i = 1; i <= this._numberChars; i++) {
                    if (LCase(this._chars[i].ContainerRoom) == LCase(room) && LCase(this._chars[i].ObjectName) == LCase(name)) {
                        this._chars[i].Exists = exists;
                        break;
                    }
                }
            }
            else if (type == Thing.Object) {
                for (var i = 1; i <= this._numberObjs; i++) {
                    if (LCase(this._objs[i].ContainerRoom) == LCase(room) && LCase(this._objs[i].ObjectName) == LCase(name)) {
                        this._objs[i].Exists = exists;
                        break;
                    }
                }
            }
        }
        this.UpdateItems(ctx);
        this.UpdateObjectList(ctx);
    };
    LegacyGame.prototype.SetStringContents = function (name, value, ctx, arrayIndex) {
        if (arrayIndex === void 0) { arrayIndex = 0; }
        var id = 0;
        var exists = false;
        if (name == "") {
            this.LogASLError("Internal error - tried to set empty string name to '" + value + "'", LogType.WarningError);
            return;
        }
        if (this._gameAslVersion >= 281) {
            var bp = InStr(name, "[");
            if (bp != 0) {
                arrayIndex = this.GetArrayIndex(name, ctx).Index;
                name = Left(name, bp - 1);
            }
        }
        if (arrayIndex < 0) {
            this.LogASLError("'" + name + "[" + Trim(Str(arrayIndex)) + "]' is invalid - did not assign to array", LogType.WarningError);
            return;
        }
        // First, see if the string already exists. If it does,
        // modify it. If not, create it.
        if (this._numberStringVariables > 0) {
            for (var i = 1; i <= this._numberStringVariables; i++) {
                if (LCase(this._stringVariable[i].VariableName) == LCase(name)) {
                    id = i;
                    exists = true;
                    break;
                }
            }
        }
        if (!exists) {
            this._numberStringVariables = this._numberStringVariables + 1;
            id = this._numberStringVariables;
            if (!this._stringVariable)
                this._stringVariable = [];
            this._stringVariable[id] = new VariableType();
            this._stringVariable[id].VariableUBound = arrayIndex;
        }
        if (arrayIndex > this._stringVariable[id].VariableUBound) {
            if (!this._stringVariable[id].VariableContents)
                this._stringVariable[id].VariableContents = [];
            this._stringVariable[id].VariableUBound = arrayIndex;
        }
        // Now, set the contents
        this._stringVariable[id].VariableName = name;
        if (!this._stringVariable[id].VariableContents)
            this._stringVariable[id].VariableContents = [];
        this._stringVariable[id].VariableContents[arrayIndex] = value;
        if (this._stringVariable[id].OnChangeScript != "") {
            var script = this._stringVariable[id].OnChangeScript;
            this.ExecuteScript(script, ctx);
        }
        if (this._stringVariable[id].DisplayString != "") {
            this.UpdateStatusVars(ctx);
        }
    };
    LegacyGame.prototype.SetUpCharObjectInfo = function () {
        var defaultProperties = new PropertiesActions();
        this._numberChars = 0;
        // see if define type <default> exists:
        var defaultExists = false;
        for (var i = 1; i <= this._numberSections; i++) {
            if (Trim(this._lines[this._defineBlocks[i].StartLine]) == "define type <default>") {
                defaultExists = true;
                defaultProperties = this.GetPropertiesInType("default");
                break;
            }
        }
        for (var i = 1; i <= this._numberSections; i++) {
            var block = this._defineBlocks[i];
            if (!(this.BeginsWith(this._lines[block.StartLine], "define room") || this.BeginsWith(this._lines[block.StartLine], "define game") || this.BeginsWith(this._lines[block.StartLine], "define object "))) {
                continue;
            }
            var restOfLine;
            var origContainerRoomName;
            var containerRoomName;
            if (this.BeginsWith(this._lines[block.StartLine], "define room")) {
                origContainerRoomName = this.GetParameter(this._lines[block.StartLine], this._nullContext);
            }
            else {
                origContainerRoomName = "";
            }
            var startLine = block.StartLine;
            var endLine = block.EndLine;
            if (this.BeginsWith(this._lines[block.StartLine], "define object ")) {
                startLine = startLine - 1;
                endLine = endLine + 1;
            }
            for (var j = startLine + 1; j <= endLine - 1; j++) {
                if (this.BeginsWith(this._lines[j], "define object")) {
                    containerRoomName = origContainerRoomName;
                    this._numberObjs = this._numberObjs + 1;
                    if (!this._objs)
                        this._objs = [];
                    this._objs[this._numberObjs] = new ObjectType();
                    var o = this._objs[this._numberObjs];
                    o.ObjectName = this.GetParameter(this._lines[j], this._nullContext);
                    o.ObjectAlias = o.ObjectName;
                    o.DefinitionSectionStart = j;
                    o.ContainerRoom = containerRoomName;
                    o.Visible = true;
                    o.Gender = "it";
                    o.Article = "it";
                    o.Take.Type = TextActionType.Nothing;
                    if (defaultExists) {
                        this.AddToObjectProperties(defaultProperties.Properties, this._numberObjs, this._nullContext);
                        for (var k = 1; k <= defaultProperties.NumberActions; k++) {
                            this.AddObjectAction(this._numberObjs, defaultProperties.Actions[k].ActionName, defaultProperties.Actions[k].Script);
                        }
                    }
                    if (this._gameAslVersion >= 391) {
                        this.AddToObjectProperties("list", this._numberObjs, this._nullContext);
                    }
                    var hidden = false;
                    do {
                        j = j + 1;
                        if (Trim(this._lines[j]) == "hidden") {
                            o.Exists = false;
                            hidden = true;
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("hidden", this._numberObjs, this._nullContext);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "startin ") && containerRoomName == "__UNKNOWN") {
                            containerRoomName = this.GetParameter(this._lines[j], this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "prefix ")) {
                            o.Prefix = this.GetParameter(this._lines[j], this._nullContext) + " ";
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("prefix=" + o.Prefix, this._numberObjs, this._nullContext);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "suffix ")) {
                            o.Suffix = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("suffix=" + o.Suffix, this._numberObjs, this._nullContext);
                            }
                        }
                        else if (Trim(this._lines[j]) == "invisible") {
                            o.Visible = false;
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("invisible", this._numberObjs, this._nullContext);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "alias ")) {
                            o.ObjectAlias = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("alias=" + o.ObjectAlias, this._numberObjs, this._nullContext);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "alt ")) {
                            this.AddToObjectAltNames(this.GetParameter(this._lines[j], this._nullContext), this._numberObjs);
                        }
                        else if (this.BeginsWith(this._lines[j], "detail ")) {
                            o.Detail = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("detail=" + o.Detail, this._numberObjs, this._nullContext);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "gender ")) {
                            o.Gender = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("gender=" + o.Gender, this._numberObjs, this._nullContext);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "article ")) {
                            o.Article = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("article=" + o.Article, this._numberObjs, this._nullContext);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "gain ")) {
                            o.GainScript = this.GetEverythingAfter(this._lines[j], "gain ");
                            this.AddObjectAction(this._numberObjs, "gain", o.GainScript);
                        }
                        else if (this.BeginsWith(this._lines[j], "lose ")) {
                            o.LoseScript = this.GetEverythingAfter(this._lines[j], "lose ");
                            this.AddObjectAction(this._numberObjs, "lose", o.LoseScript);
                        }
                        else if (this.BeginsWith(this._lines[j], "displaytype ")) {
                            o.DisplayType = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("displaytype=" + o.DisplayType, this._numberObjs, this._nullContext);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "look ")) {
                            if (this._gameAslVersion >= 311) {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "look ");
                                if (Left(restOfLine, 1) == "<") {
                                    this.AddToObjectProperties("look=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                                }
                                else {
                                    this.AddObjectAction(this._numberObjs, "look", restOfLine);
                                }
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "examine ")) {
                            if (this._gameAslVersion >= 311) {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "examine ");
                                if (Left(restOfLine, 1) == "<") {
                                    this.AddToObjectProperties("examine=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                                }
                                else {
                                    this.AddObjectAction(this._numberObjs, "examine", restOfLine);
                                }
                            }
                        }
                        else if (this._gameAslVersion >= 311 && this.BeginsWith(this._lines[j], "speak ")) {
                            restOfLine = this.GetEverythingAfter(this._lines[j], "speak ");
                            if (Left(restOfLine, 1) == "<") {
                                this.AddToObjectProperties("speak=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            }
                            else {
                                this.AddObjectAction(this._numberObjs, "speak", restOfLine);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "properties ")) {
                            this.AddToObjectProperties(this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "type ")) {
                            o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                            if (!o.TypesIncluded)
                                o.TypesIncluded = [];
                            o.TypesIncluded[o.NumberTypesIncluded] = this.GetParameter(this._lines[j], this._nullContext);
                            var PropertyData = this.GetPropertiesInType(this.GetParameter(this._lines[j], this._nullContext));
                            this.AddToObjectProperties(PropertyData.Properties, this._numberObjs, this._nullContext);
                            for (var k = 1; k <= PropertyData.NumberActions; k++) {
                                this.AddObjectAction(this._numberObjs, PropertyData.Actions[k].ActionName, PropertyData.Actions[k].Script);
                            }
                            if (!o.TypesIncluded)
                                o.TypesIncluded = [];
                            for (var k = 1; k <= PropertyData.NumberTypesIncluded; k++) {
                                o.TypesIncluded[k + o.NumberTypesIncluded] = PropertyData.TypesIncluded[k];
                            }
                            o.NumberTypesIncluded = o.NumberTypesIncluded + PropertyData.NumberTypesIncluded;
                        }
                        else if (this.BeginsWith(this._lines[j], "action ")) {
                            this.AddToObjectActions(this.GetEverythingAfter(this._lines[j], "action "), this._numberObjs, this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "use ")) {
                            this.AddToUseInfo(this._numberObjs, this.GetEverythingAfter(this._lines[j], "use "));
                        }
                        else if (this.BeginsWith(this._lines[j], "give ")) {
                            this.AddToGiveInfo(this._numberObjs, this.GetEverythingAfter(this._lines[j], "give "));
                        }
                        else if (Trim(this._lines[j]) == "take") {
                            o.Take.Type = TextActionType.Default;
                            this.AddToObjectProperties("take", this._numberObjs, this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "take ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "take "), 1) == "<") {
                                o.Take.Type = TextActionType.Text;
                                o.Take.Data = this.GetParameter(this._lines[j], this._nullContext);
                                this.AddToObjectProperties("take=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            }
                            else {
                                o.Take.Type = TextActionType.Script;
                                restOfLine = this.GetEverythingAfter(this._lines[j], "take ");
                                o.Take.Data = restOfLine;
                                this.AddObjectAction(this._numberObjs, "take", restOfLine);
                            }
                        }
                        else if (Trim(this._lines[j]) == "container") {
                            if (this._gameAslVersion >= 391) {
                                this.AddToObjectProperties("container", this._numberObjs, this._nullContext);
                            }
                        }
                        else if (Trim(this._lines[j]) == "surface") {
                            if (this._gameAslVersion >= 391) {
                                this.AddToObjectProperties("container", this._numberObjs, this._nullContext);
                                this.AddToObjectProperties("surface", this._numberObjs, this._nullContext);
                            }
                        }
                        else if (Trim(this._lines[j]) == "opened") {
                            if (this._gameAslVersion >= 391) {
                                this.AddToObjectProperties("opened", this._numberObjs, this._nullContext);
                            }
                        }
                        else if (Trim(this._lines[j]) == "transparent") {
                            if (this._gameAslVersion >= 391) {
                                this.AddToObjectProperties("transparent", this._numberObjs, this._nullContext);
                            }
                        }
                        else if (Trim(this._lines[j]) == "open") {
                            this.AddToObjectProperties("open", this._numberObjs, this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "open ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "open "), 1) == "<") {
                                this.AddToObjectProperties("open=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            }
                            else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "open ");
                                this.AddObjectAction(this._numberObjs, "open", restOfLine);
                            }
                        }
                        else if (Trim(this._lines[j]) == "close") {
                            this.AddToObjectProperties("close", this._numberObjs, this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "close ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "close "), 1) == "<") {
                                this.AddToObjectProperties("close=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            }
                            else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "close ");
                                this.AddObjectAction(this._numberObjs, "close", restOfLine);
                            }
                        }
                        else if (Trim(this._lines[j]) == "add") {
                            this.AddToObjectProperties("add", this._numberObjs, this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "add ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "add "), 1) == "<") {
                                this.AddToObjectProperties("add=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            }
                            else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "add ");
                                this.AddObjectAction(this._numberObjs, "add", restOfLine);
                            }
                        }
                        else if (Trim(this._lines[j]) == "remove") {
                            this.AddToObjectProperties("remove", this._numberObjs, this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "remove ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "remove "), 1) == "<") {
                                this.AddToObjectProperties("remove=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            }
                            else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "remove ");
                                this.AddObjectAction(this._numberObjs, "remove", restOfLine);
                            }
                        }
                        else if (this.BeginsWith(this._lines[j], "parent ")) {
                            this.AddToObjectProperties("parent=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "list")) {
                            this.ProcessListInfo(this._lines[j], this._numberObjs);
                        }
                    } while (!(Trim(this._lines[j]) == "end define"));
                    o.DefinitionSectionEnd = j;
                    if (!hidden) {
                        o.Exists = true;
                    }
                }
                else if (this._gameAslVersion <= 280 && this.BeginsWith(this._lines[j], "define character")) {
                    containerRoomName = origContainerRoomName;
                    this._numberChars = this._numberChars + 1;
                    if (!this._chars)
                        this._chars = [];
                    this._chars[this._numberChars] = new ObjectType();
                    this._chars[this._numberChars].ObjectName = this.GetParameter(this._lines[j], this._nullContext);
                    this._chars[this._numberChars].DefinitionSectionStart = j;
                    this._chars[this._numberChars].ContainerRoom = "";
                    this._chars[this._numberChars].Visible = true;
                    var hidden = false;
                    do {
                        j = j + 1;
                        if (Trim(this._lines[j]) == "hidden") {
                            this._chars[this._numberChars].Exists = false;
                            hidden = true;
                        }
                        else if (this.BeginsWith(this._lines[j], "startin ") && containerRoomName == "__UNKNOWN") {
                            containerRoomName = this.GetParameter(this._lines[j], this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "prefix ")) {
                            this._chars[this._numberChars].Prefix = this.GetParameter(this._lines[j], this._nullContext) + " ";
                        }
                        else if (this.BeginsWith(this._lines[j], "suffix ")) {
                            this._chars[this._numberChars].Suffix = " " + this.GetParameter(this._lines[j], this._nullContext);
                        }
                        else if (Trim(this._lines[j]) == "invisible") {
                            this._chars[this._numberChars].Visible = false;
                        }
                        else if (this.BeginsWith(this._lines[j], "alias ")) {
                            this._chars[this._numberChars].ObjectAlias = this.GetParameter(this._lines[j], this._nullContext);
                        }
                        else if (this.BeginsWith(this._lines[j], "detail ")) {
                            this._chars[this._numberChars].Detail = this.GetParameter(this._lines[j], this._nullContext);
                        }
                        this._chars[this._numberChars].ContainerRoom = containerRoomName;
                    } while (!(Trim(this._lines[j]) == "end define"));
                    this._chars[this._numberChars].DefinitionSectionEnd = j;
                    if (!hidden) {
                        this._chars[this._numberChars].Exists = true;
                    }
                }
            }
        }
        this.UpdateVisibilityInContainers(this._nullContext);
    };
    LegacyGame.prototype.ShowGameAbout = function (ctx) {
        var version = this.FindStatement(this.GetDefineBlock("game"), "game version");
        var author = this.FindStatement(this.GetDefineBlock("game"), "game author");
        var copyright = this.FindStatement(this.GetDefineBlock("game"), "game copyright");
        var info = this.FindStatement(this.GetDefineBlock("game"), "game info");
        this.Print("|bGame name:|cl  " + this._gameName + "|cb|xb", ctx);
        if (version != "") {
            this.Print("|bVersion:|xb    " + version, ctx);
        }
        if (author != "") {
            this.Print("|bAuthor:|xb     " + author, ctx);
        }
        if (copyright != "") {
            this.Print("|bCopyright:|xb  " + copyright, ctx);
        }
        if (info != "") {
            this.Print("", ctx);
            this.Print(info, ctx);
        }
    };
    LegacyGame.prototype.ShowPicture = function (filename) {
        // In Quest 4.x this function would be used for showing a picture in a popup window, but
        // this is no longer supported - ALL images are displayed in-line with the game text. Any
        // image caption is displayed as text, and any image size specified is ignored.
        var caption = "";
        if (InStr(filename, ";") != 0) {
            caption = Trim(Mid(filename, InStr(filename, ";") + 1));
            filename = Trim(Left(filename, InStr(filename, ";") - 1));
        }
        if (InStr(filename, "@") != 0) {
            // size is ignored
            filename = Trim(Left(filename, InStr(filename, "@") - 1));
        }
        if (caption.length > 0) {
            this.Print(caption, this._nullContext);
        }
        this.ShowPictureInText(filename);
    };
    LegacyGame.prototype.ShowRoomInfo = function (room, ctx, noPrint) {
        if (noPrint === void 0) { noPrint = false; }
        if (this._gameAslVersion < 280) {
            this.ShowRoomInfoV2(room);
            return;
        }
        var roomDisplayText = "";
        var descTagExist = false;
        var doorwayString;
        var roomAlias;
        var finishedFindingCommas = false;
        var prefix;
        var roomDisplayName;
        var roomDisplayNameNoFormat;
        var inDescription;
        var visibleObjects = "";
        var visibleObjectsNoFormat;
        var placeList;
        var lastComma = 0;
        var oldLastComma = 0;
        var descType = 0;
        var descLine = "";
        var showLookText = false;
        var lookDesc = "";
        var objLook;
        var objSuffix;
        var gameBlock = this.GetDefineBlock("game");
        this._currentRoom = room;
        var id = this.GetRoomId(this._currentRoom, ctx);
        if (id == 0) {
            return;
        }
        // FIRST LINE - YOU ARE IN... ***********************************************
        roomAlias = this._rooms[id].RoomAlias;
        if (roomAlias == "") {
            roomAlias = this._rooms[id].RoomName;
        }
        prefix = this._rooms[id].Prefix;
        if (prefix == "") {
            roomDisplayName = "|cr" + roomAlias + "|cb";
            roomDisplayNameNoFormat = roomAlias;
        }
        else {
            roomDisplayName = prefix + " |cr" + roomAlias + "|cb";
            roomDisplayNameNoFormat = prefix + " " + roomAlias;
        }
        inDescription = this._rooms[id].InDescription;
        if (inDescription != "") {
            // Print player's location according to indescription:
            if (Right(inDescription, 1) == ":") {
                // if line ends with a colon, add place name:
                roomDisplayText = roomDisplayText + Left(inDescription, Len(inDescription) - 1) + " " + roomDisplayName + ".\n";
            }
            else {
                // otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText + inDescription + "\n";
            }
        }
        else {
            // if no indescription line, print the default.
            roomDisplayText = roomDisplayText + "You are in " + roomDisplayName + ".\n";
        }
        this._player.LocationUpdated(UCase(Left(roomAlias, 1)) + Mid(roomAlias, 2));
        this.SetStringContents("quest.formatroom", roomDisplayNameNoFormat, ctx);
        // SHOW OBJECTS *************************************************************
        visibleObjectsNoFormat = "";
        var visibleObjectsList = [];
        // of object IDs
        var count = 0;
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ContainerRoom) == LCase(room) && this._objs[i].Exists && this._objs[i].Visible && !this._objs[i].IsExit) {
                visibleObjectsList.push(i);
            }
        }
        visibleObjectsList.forEach(function (objId) {
            objSuffix = this._objs[objId].Suffix;
            if (objSuffix != "") {
                objSuffix = " " + objSuffix;
            }
            if (this._objs[objId].ObjectAlias == "") {
                visibleObjects = visibleObjects + this._objs[objId].Prefix + "|b" + this._objs[objId].ObjectName + "|xb" + objSuffix;
                visibleObjectsNoFormat = visibleObjectsNoFormat + this._objs[objId].Prefix + this._objs[objId].ObjectName;
            }
            else {
                visibleObjects = visibleObjects + this._objs[objId].Prefix + "|b" + this._objs[objId].ObjectAlias + "|xb" + objSuffix;
                visibleObjectsNoFormat = visibleObjectsNoFormat + this._objs[objId].Prefix + this._objs[objId].ObjectAlias;
            }
            count = count + 1;
            if (count < visibleObjectsList.length - 1) {
                visibleObjects = visibleObjects + ", ";
                visibleObjectsNoFormat = visibleObjectsNoFormat + ", ";
            }
            else if (count == visibleObjectsList.length - 1) {
                visibleObjects = visibleObjects + " and ";
                visibleObjectsNoFormat = visibleObjectsNoFormat + ", ";
            }
        }, this);
        if (visibleObjectsList.length > 0) {
            this.SetStringContents("quest.formatobjects", visibleObjects, ctx);
            visibleObjects = "There is " + visibleObjects + " here.";
            this.SetStringContents("quest.objects", visibleObjectsNoFormat, ctx);
            roomDisplayText = roomDisplayText + visibleObjects + "\n";
        }
        else {
            this.SetStringContents("quest.objects", "", ctx);
            this.SetStringContents("quest.formatobjects", "", ctx);
        }
        // SHOW EXITS ***************************************************************
        doorwayString = this.UpdateDoorways(id, ctx);
        if (this._gameAslVersion < 410) {
            placeList = this.GetGoToExits(id, ctx);
            if (placeList != "") {
                //strip final comma
                placeList = Left(placeList, Len(placeList) - 2);
                //if there is still a comma here, there is more than
                //one place, so add the word "or" before the last one.
                if (InStr(placeList, ",") > 0) {
                    lastComma = 0;
                    finishedFindingCommas = false;
                    do {
                        oldLastComma = lastComma;
                        lastComma = InStrFrom(lastComma + 1, placeList, ",");
                        if (lastComma == 0) {
                            finishedFindingCommas = true;
                            lastComma = oldLastComma;
                        }
                    } while (!(finishedFindingCommas));
                    placeList = Left(placeList, lastComma - 1) + " or" + Right(placeList, Len(placeList) - lastComma);
                }
                roomDisplayText = roomDisplayText + "You can go to " + placeList + ".\n";
                this.SetStringContents("quest.doorways.places", placeList, ctx);
            }
            else {
                this.SetStringContents("quest.doorways.places", "", ctx);
            }
        }
        // GET "LOOK" DESCRIPTION (but don't print it yet) **************************
        objLook = this.GetObjectProperty("look", this._rooms[id].ObjId, null, false);
        if (objLook == "") {
            if (this._rooms[id].Look != "") {
                lookDesc = this._rooms[id].Look;
            }
        }
        else {
            lookDesc = objLook;
        }
        this.SetStringContents("quest.lookdesc", lookDesc, ctx);
        // FIND DESCRIPTION TAG, OR ACTION ******************************************
        // In Quest versions prior to 3.1, with any custom description, the "look"
        // text was always displayed after the "description" tag was printed/executed.
        // In Quest 3.1 and later, it isn't - descriptions should print the look
        // tag themselves when and where necessary.
        showLookText = true;
        if (this._rooms[id].Description.Data != "") {
            descLine = this._rooms[id].Description.Data;
            descType = this._rooms[id].Description.Type;
            descTagExist = true;
        }
        else {
            descTagExist = false;
        }
        if (!descTagExist) {
            //Look in the "define game" block:
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "description ")) {
                    descLine = this.GetEverythingAfter(this._lines[i], "description ");
                    descTagExist = true;
                    if (Left(descLine, 1) == "<") {
                        descLine = this.GetParameter(descLine, ctx);
                        descType = TextActionType.Text;
                    }
                    else {
                        descType = TextActionType.Script;
                    }
                    i = gameBlock.EndLine;
                }
            }
        }
        if (descTagExist && this._gameAslVersion >= 310) {
            showLookText = false;
        }
        if (!noPrint) {
            if (!descTagExist) {
                //Remove final \n
                roomDisplayText = Left(roomDisplayText, Len(roomDisplayText) - 1);
                this.Print(roomDisplayText, ctx);
                if (doorwayString != "") {
                    this.Print(doorwayString, ctx);
                }
            }
            else {
                //execute description tag:
                //If no script, just print the tag's parameter.
                //Otherwise, execute it as ASL script:
                if (descType == TextActionType.Text) {
                    this.Print(descLine, ctx);
                }
                else {
                    this.ExecuteScript(descLine, ctx);
                }
            }
            this.UpdateObjectList(ctx);
            // SHOW "LOOK" DESCRIPTION **************************************************
            if (showLookText) {
                if (lookDesc != "") {
                    this.Print(lookDesc, ctx);
                }
            }
        }
    };
    LegacyGame.prototype.CheckCollectable = function (id) {
        // Checks to see whether a collectable item has exceeded
        // its range - if so, it resets the number to the nearest
        // valid number. It's a handy quick way of making sure that
        // a player's health doesn't reach 101%, for example.
        var max = 0;
        var value = 0;
        var min = 0;
        var m = 0;
        var type = this._collectables[id].Type;
        value = this._collectables[id].Value;
        if (type == "%" && value > 100) {
            value = 100;
        }
        if ((type == "%" || type == "p") && value < 0) {
            value = 0;
        }
        if (InStr(type, "r") > 0) {
            if (InStr(type, "r") == 1) {
                max = Val(Mid(type, Len(type) - 1));
                m = 1;
            }
            else if (InStr(type, "r") == Len(type)) {
                min = Val(Left(type, Len(type) - 1));
                m = 2;
            }
            else {
                min = Val(Left(type, InStr(type, "r") - 1));
                max = Val(Mid(type, InStr(type, "r") + 1));
                m = 3;
            }
            if ((m == 1 || m == 3) && value > max) {
                value = max;
            }
            if ((m == 2 || m == 3) && value < min) {
                value = min;
            }
        }
        this._collectables[id].Value = value;
    };
    LegacyGame.prototype.DisplayCollectableInfo = function (id) {
        var display;
        if (this._collectables[id].Display == "<def>") {
            display = "You have " + Trim(Str(this._collectables[id].Value)) + " " + this._collectables[id].Name;
        }
        else if (this._collectables[id].Display == "") {
            display = "<null>";
        }
        else {
            var ep = InStr(this._collectables[id].Display, "!");
            if (ep == 0) {
                display = this._collectables[id].Display;
            }
            else {
                var firstBit = Left(this._collectables[id].Display, ep - 1);
                var nextBit = Right(this._collectables[id].Display, Len(this._collectables[id].Display) - ep);
                display = firstBit + Trim(Str(this._collectables[id].Value)) + nextBit;
            }
            if (InStr(display, "*") > 0) {
                var firstStarPos = InStr(display, "*");
                var secondStarPos = InStrFrom(firstStarPos + 1, display, "*");
                var beforeStar = Left(display, firstStarPos - 1);
                var afterStar = Mid(display, secondStarPos + 1);
                var betweenStar = Mid(display, firstStarPos + 1, (secondStarPos - firstStarPos) - 1);
                if (this._collectables[id].Value != 1) {
                    display = beforeStar + betweenStar + afterStar;
                }
                else {
                    display = beforeStar + afterStar;
                }
            }
        }
        if (this._collectables[id].Value == 0 && !this._collectables[id].DisplayWhenZero) {
            display = "<null>";
        }
        return display;
    };
    LegacyGame.prototype.DisplayTextSection = function (section, ctx) {
        var block;
        block = this.DefineBlockParam("text", section);
        if (block.StartLine == 0) {
            return;
        }
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this._gameAslVersion >= 392) {
                // Convert string variables etc.
                this.Print(this.GetParameter("<" + this._lines[i] + ">", ctx), ctx);
            }
            else {
                this.Print(this._lines[i], ctx);
            }
        }
        this.Print("", ctx);
    };
    // Returns true if the system is ready to process a new command after completion - so it will be
    // in most cases, except when ExecCommand just caused an "enter" script command to complete
    LegacyGame.prototype.ExecCommand = function (input, ctx, echo, runUserCommand, dontSetIt) {
        if (echo === void 0) { echo = true; }
        if (runUserCommand === void 0) { runUserCommand = true; }
        if (dontSetIt === void 0) { dontSetIt = false; }
        var parameter;
        var skipAfterTurn = false;
        input = this.RemoveFormatting(input);
        var oldBadCmdBefore = this._badCmdBefore;
        var roomID = this.GetRoomId(this._currentRoom, ctx);
        var enteredHelpCommand = false;
        if (input == "") {
            return true;
        }
        var cmd = LCase(input);
        if (this._commandOverrideModeOn) {
            // Commands have been overridden for this command,
            // so put input into previously specified variable
            // and exit:
            this.SetStringContents(this._commandOverrideVariable, input, ctx);
            return false;
        }
        var userCommandReturn = false;
        if (echo) {
            this.Print("> " + input, ctx);
        }
        input = LCase(input);
        this.SetStringContents("quest.originalcommand", input, ctx);
        var newCommand = " " + input + " ";
        // Convert synonyms:
        for (var i = 1; i <= this._numberSynonyms; i++) {
            var cp = 1;
            var n = 0;
            do {
                n = InStrFrom(cp, newCommand, " " + this._synonyms[i].OriginalWord + " ");
                if (n != 0) {
                    newCommand = Left(newCommand, n - 1) + " " + this._synonyms[i].ConvertTo + " " + Mid(newCommand, n + Len(this._synonyms[i].OriginalWord) + 2);
                    cp = n + 1;
                }
            } while (!(n == 0));
        }
        //strip starting and ending spaces
        input = Mid(newCommand, 2, Len(newCommand) - 2);
        this.SetStringContents("quest.command", input, ctx);
        // Execute any "beforeturn" script:
        var newCtx = this.CopyContext(ctx);
        var globalOverride = false;
        // RoomID is 0 if there are no rooms in the game. Unlikely, but we get an RTE otherwise.
        if (roomID != 0) {
            if (this._rooms[roomID].BeforeTurnScript != "") {
                if (this.BeginsWith(this._rooms[roomID].BeforeTurnScript, "override")) {
                    this.ExecuteScript(this.GetEverythingAfter(this._rooms[roomID].BeforeTurnScript, "override"), newCtx);
                    globalOverride = true;
                }
                else {
                    this.ExecuteScript(this._rooms[roomID].BeforeTurnScript, newCtx);
                }
            }
        }
        if (this._beforeTurnScript != "" && !globalOverride) {
            this.ExecuteScript(this._beforeTurnScript, newCtx);
        }
        // In executing BeforeTurn script, "dontprocess" sets ctx.DontProcessCommand,
        // in which case we don't process the command.
        if (!newCtx.DontProcessCommand) {
            //Try to execute user defined command, if allowed:
            userCommandReturn = false;
            if (runUserCommand) {
                userCommandReturn = this.ExecUserCommand(input, ctx);
                if (!userCommandReturn) {
                    userCommandReturn = this.ExecVerb(input, ctx);
                }
                if (!userCommandReturn) {
                    // Try command defined by a library
                    userCommandReturn = this.ExecUserCommand(input, ctx, true);
                }
                if (!userCommandReturn) {
                    // Try verb defined by a library
                    userCommandReturn = this.ExecVerb(input, ctx, true);
                }
            }
            input = LCase(input);
        }
        else {
            // Set the UserCommand flag to fudge not processing any more commands
            userCommandReturn = true;
        }
        var invList = "";
        if (!userCommandReturn) {
            if (this.CmdStartsWith(input, "speak to ")) {
                parameter = this.GetEverythingAfter(input, "speak to ");
                this.ExecSpeak(parameter, ctx);
            }
            else if (this.CmdStartsWith(input, "talk to ")) {
                parameter = this.GetEverythingAfter(input, "talk to ");
                this.ExecSpeak(parameter, ctx);
            }
            else if (cmd == "exit" || cmd == "out" || cmd == "leave") {
                this.GoDirection("out", ctx);
                this._lastIt = 0;
            }
            else if (cmd == "north" || cmd == "south" || cmd == "east" || cmd == "west") {
                this.GoDirection(input, ctx);
                this._lastIt = 0;
            }
            else if (cmd == "n" || cmd == "s" || cmd == "w" || cmd == "e") {
                switch (InStr("nswe", cmd)) {
                    case 1:
                        this.GoDirection("north", ctx);
                        break;
                    case 2:
                        this.GoDirection("south", ctx);
                        break;
                    case 3:
                        this.GoDirection("west", ctx);
                        break;
                    case 4:
                        this.GoDirection("east", ctx);
                        break;
                }
                this._lastIt = 0;
            }
            else if (cmd == "ne" || cmd == "northeast" || cmd == "north-east" || cmd == "north east" || cmd == "go ne" || cmd == "go northeast" || cmd == "go north-east" || cmd == "go north east") {
                this.GoDirection("northeast", ctx);
                this._lastIt = 0;
            }
            else if (cmd == "nw" || cmd == "northwest" || cmd == "north-west" || cmd == "north west" || cmd == "go nw" || cmd == "go northwest" || cmd == "go north-west" || cmd == "go north west") {
                this.GoDirection("northwest", ctx);
                this._lastIt = 0;
            }
            else if (cmd == "se" || cmd == "southeast" || cmd == "south-east" || cmd == "south east" || cmd == "go se" || cmd == "go southeast" || cmd == "go south-east" || cmd == "go south east") {
                this.GoDirection("southeast", ctx);
                this._lastIt = 0;
            }
            else if (cmd == "sw" || cmd == "southwest" || cmd == "south-west" || cmd == "south west" || cmd == "go sw" || cmd == "go southwest" || cmd == "go south-west" || cmd == "go south west") {
                this.GoDirection("southwest", ctx);
                this._lastIt = 0;
            }
            else if (cmd == "up" || cmd == "u") {
                this.GoDirection("up", ctx);
                this._lastIt = 0;
            }
            else if (cmd == "down" || cmd == "d") {
                this.GoDirection("down", ctx);
                this._lastIt = 0;
            }
            else if (this.CmdStartsWith(input, "go ")) {
                if (this._gameAslVersion >= 410) {
                    this._rooms[this.GetRoomId(this._currentRoom, ctx)].Exits.ExecuteGo(input, ctx);
                }
                else {
                    parameter = this.GetEverythingAfter(input, "go ");
                    if (parameter == "out") {
                        this.GoDirection("out", ctx);
                    }
                    else if (parameter == "north" || parameter == "south" || parameter == "east" || parameter == "west" || parameter == "up" || parameter == "down") {
                        this.GoDirection(parameter, ctx);
                    }
                    else if (this.BeginsWith(parameter, "to ")) {
                        parameter = this.GetEverythingAfter(parameter, "to ");
                        this.GoToPlace(parameter, ctx);
                    }
                    else {
                        this.PlayerErrorMessage(PlayerError.BadGo, ctx);
                    }
                }
                this._lastIt = 0;
            }
            else if (this.CmdStartsWith(input, "give ")) {
                parameter = this.GetEverythingAfter(input, "give ");
                this.ExecGive(parameter, ctx);
            }
            else if (this.CmdStartsWith(input, "take ")) {
                parameter = this.GetEverythingAfter(input, "take ");
                this.ExecTake(parameter, ctx);
            }
            else if (this.CmdStartsWith(input, "drop ") && this._gameAslVersion >= 280) {
                parameter = this.GetEverythingAfter(input, "drop ");
                this.ExecDrop(parameter, ctx);
            }
            else if (this.CmdStartsWith(input, "get ")) {
                parameter = this.GetEverythingAfter(input, "get ");
                this.ExecTake(parameter, ctx);
            }
            else if (this.CmdStartsWith(input, "pick up ")) {
                parameter = this.GetEverythingAfter(input, "pick up ");
                this.ExecTake(parameter, ctx);
            }
            else if (cmd == "pick it up" || cmd == "pick them up" || cmd == "pick this up" || cmd == "pick that up" || cmd == "pick these up" || cmd == "pick those up" || cmd == "pick him up" || cmd == "pick her up") {
                this.ExecTake(Mid(cmd, 6, InStrFrom(7, cmd, " ") - 6), ctx);
            }
            else if (this.CmdStartsWith(input, "look ")) {
                this.ExecLook(input, ctx);
            }
            else if (this.CmdStartsWith(input, "l ")) {
                this.ExecLook("look " + this.GetEverythingAfter(input, "l "), ctx);
            }
            else if (this.CmdStartsWith(input, "examine ") && this._gameAslVersion >= 280) {
                this.ExecExamine(input, ctx);
            }
            else if (this.CmdStartsWith(input, "x ") && this._gameAslVersion >= 280) {
                this.ExecExamine("examine " + this.GetEverythingAfter(input, "x "), ctx);
            }
            else if (cmd == "l" || cmd == "look") {
                this.ExecLook("look", ctx);
            }
            else if (cmd == "x" || cmd == "examine") {
                this.ExecExamine("examine", ctx);
            }
            else if (this.CmdStartsWith(input, "use ")) {
                this.ExecUse(input, ctx);
            }
            else if (this.CmdStartsWith(input, "open ") && this._gameAslVersion >= 391) {
                this.ExecOpenClose(input, ctx);
            }
            else if (this.CmdStartsWith(input, "close ") && this._gameAslVersion >= 391) {
                this.ExecOpenClose(input, ctx);
            }
            else if (this.CmdStartsWith(input, "put ") && this._gameAslVersion >= 391) {
                this.ExecAddRemove(input, ctx);
            }
            else if (this.CmdStartsWith(input, "add ") && this._gameAslVersion >= 391) {
                this.ExecAddRemove(input, ctx);
            }
            else if (this.CmdStartsWith(input, "remove ") && this._gameAslVersion >= 391) {
                this.ExecAddRemove(input, ctx);
            }
            else if (cmd == "save") {
                this._player.RequestSave(null);
            }
            else if (cmd == "quit") {
                this.GameFinished();
            }
            else if (this.BeginsWith(cmd, "help")) {
                this.ShowHelp(ctx);
                enteredHelpCommand = true;
            }
            else if (cmd == "about") {
                this.ShowGameAbout(ctx);
            }
            else if (cmd == "clear") {
                this.DoClear();
            }
            else if (cmd == "inventory" || cmd == "inv" || cmd == "i") {
                if (this._gameAslVersion >= 280) {
                    for (var i = 1; i <= this._numberObjs; i++) {
                        if (this._objs[i].ContainerRoom == "inventory" && this._objs[i].Exists && this._objs[i].Visible) {
                            invList = invList + this._objs[i].Prefix;
                            if (this._objs[i].ObjectAlias == "") {
                                invList = invList + "|b" + this._objs[i].ObjectName + "|xb";
                            }
                            else {
                                invList = invList + "|b" + this._objs[i].ObjectAlias + "|xb";
                            }
                            if (this._objs[i].Suffix != "") {
                                invList = invList + " " + this._objs[i].Suffix;
                            }
                            invList = invList + ", ";
                        }
                    }
                }
                else {
                    for (var j = 1; j <= this._numberItems; j++) {
                        if (this._items[j].Got) {
                            invList = invList + this._items[j].Name + ", ";
                        }
                    }
                }
                if (invList != "") {
                    invList = Left(invList, Len(invList) - 2);
                    invList = UCase(Left(invList, 1)) + Mid(invList, 2);
                    var pos = 1;
                    var lastComma = 0;
                    var thisComma = 0;
                    do {
                        thisComma = InStrFrom(pos, invList, ",");
                        if (thisComma != 0) {
                            lastComma = thisComma;
                            pos = thisComma + 1;
                        }
                    } while (!(thisComma == 0));
                    if (lastComma != 0) {
                        invList = Left(invList, lastComma - 1) + " and" + Mid(invList, lastComma + 1);
                    }
                    this.Print("You are carrying:|n" + invList + ".", ctx);
                }
                else {
                    this.Print("You are not carrying anything.", ctx);
                }
            }
            else if (this.CmdStartsWith(input, "oops ")) {
                this.ExecOops(this.GetEverythingAfter(input, "oops "), ctx);
            }
            else if (this.CmdStartsWith(input, "the ")) {
                this.ExecOops(this.GetEverythingAfter(input, "the "), ctx);
            }
            else {
                this.PlayerErrorMessage(PlayerError.BadCommand, ctx);
            }
        }
        if (!skipAfterTurn) {
            // Execute any "afterturn" script:
            globalOverride = false;
            if (roomID != 0) {
                if (this._rooms[roomID].AfterTurnScript != "") {
                    if (this.BeginsWith(this._rooms[roomID].AfterTurnScript, "override")) {
                        this.ExecuteScript(this.GetEverythingAfter(this._rooms[roomID].AfterTurnScript, "override"), ctx);
                        globalOverride = true;
                    }
                    else {
                        this.ExecuteScript(this._rooms[roomID].AfterTurnScript, ctx);
                    }
                }
            }
            if (this._afterTurnScript != "" && !globalOverride) {
                this.ExecuteScript(this._afterTurnScript, ctx);
            }
        }
        this.Print("", ctx);
        if (!dontSetIt) {
            // Use "DontSetIt" when we don't want "it" etc. to refer to the object used in this turn.
            // This is used for e.g. auto-remove object from container when taking.
            this._lastIt = this._thisTurnIt;
            this._lastItMode = this._thisTurnItMode;
        }
        if (this._badCmdBefore == oldBadCmdBefore) {
            this._badCmdBefore = "";
        }
        return true;
    };
    LegacyGame.prototype.CmdStartsWith = function (cmd, startsWith) {
        // When we are checking user input in ExecCommand, we check things like whether
        // the player entered something beginning with "put ". We need to trim what the player entered
        // though, otherwise they might just type "put " and then we would try disambiguating an object
        // called "".
        return this.BeginsWith(Trim(cmd), startsWith);
    };
    LegacyGame.prototype.ExecGive = function (giveString, ctx) {
        var article;
        var item;
        var character;
        var type;
        var id = 0;
        var script = "";
        var toPos = InStr(giveString, " to ");
        if (toPos == 0) {
            toPos = InStr(giveString, " the ");
            if (toPos == 0) {
                this.PlayerErrorMessage(PlayerError.BadGive, ctx);
                return;
            }
            else {
                item = Trim(Mid(giveString, toPos + 4, Len(giveString) - (toPos + 2)));
                character = Trim(Mid(giveString, 1, toPos));
            }
        }
        else {
            item = Trim(Mid(giveString, 1, toPos));
            character = Trim(Mid(giveString, toPos + 3, Len(giveString) - (toPos + 2)));
        }
        if (this._gameAslVersion >= 281) {
            type = Thing.Object;
        }
        else {
            type = Thing.Character;
        }
        // First see if player has "ItemToGive":
        if (this._gameAslVersion >= 280) {
            id = this.Disambiguate(item, "inventory", ctx);
            if (id == -1) {
                this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                this._badCmdBefore = "give";
                this._badCmdAfter = "to " + character;
                return;
            }
            else if (id == -2) {
                return;
            }
            else {
                article = this._objs[id].Article;
            }
        }
        else {
            // ASL2:
            var notGot = true;
            for (var i = 1; i <= this._numberItems; i++) {
                if (LCase(this._items[i].Name) == LCase(item)) {
                    if (!this._items[i].Got) {
                        notGot = true;
                        break;
                    }
                    else {
                        notGot = false;
                    }
                }
            }
            if (notGot) {
                this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                return;
            }
            else {
                article = this._objs[id].Article;
            }
        }
        if (this._gameAslVersion >= 281) {
            var foundScript = false;
            var foundObject = false;
            var giveToId = this.Disambiguate(character, this._currentRoom, ctx);
            if (giveToId > 0) {
                foundObject = true;
            }
            if (!foundObject) {
                if (giveToId != -2) {
                    this.PlayerErrorMessage(PlayerError.BadCharacter, ctx);
                }
                this._badCmdBefore = "give " + item + " to";
                return;
            }
            //Find appropriate give script ****
            //now, for "give a to b", we have
            //ItemID=a and GiveToObjectID=b
            var o = this._objs[giveToId];
            for (var i = 1; i <= o.NumberGiveData; i++) {
                if (o.GiveData[i].GiveType == GiveType.GiveSomethingTo && LCase(o.GiveData[i].GiveObject) == LCase(this._objs[id].ObjectName)) {
                    foundScript = true;
                    script = o.GiveData[i].GiveScript;
                    break;
                }
            }
            if (!foundScript) {
                //check a for give to <b>:
                var g = this._objs[id];
                for (var i = 1; i <= g.NumberGiveData; i++) {
                    if (g.GiveData[i].GiveType == GiveType.GiveToSomething && LCase(g.GiveData[i].GiveObject) == LCase(this._objs[giveToId].ObjectName)) {
                        foundScript = true;
                        script = g.GiveData[i].GiveScript;
                        break;
                    }
                }
            }
            if (!foundScript) {
                //check b for give anything:
                script = this._objs[giveToId].GiveAnything;
                if (script != "") {
                    foundScript = true;
                    this.SetStringContents("quest.give.object.name", this._objs[id].ObjectName, ctx);
                }
            }
            if (!foundScript) {
                //check a for give to anything:
                script = this._objs[id].GiveToAnything;
                if (script != "") {
                    foundScript = true;
                    this.SetStringContents("quest.give.object.name", this._objs[giveToId].ObjectName, ctx);
                }
            }
            if (foundScript) {
                this.ExecuteScript(script, ctx, id);
            }
            else {
                this.SetStringContents("quest.error.charactername", this._objs[giveToId].ObjectName, ctx);
                var gender = Trim(this._objs[giveToId].Gender);
                gender = UCase(Left(gender, 1)) + Mid(gender, 2);
                this.SetStringContents("quest.error.gender", gender, ctx);
                this.SetStringContents("quest.error.article", article, ctx);
                this.PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
            }
        }
        else {
            // ASL2:
            var block = this.GetThingBlock(character, this._currentRoom, type);
            if ((block.StartLine == 0 && block.EndLine == 0) || !this.IsAvailable(character + "@" + this._currentRoom, type, ctx)) {
                this.PlayerErrorMessage(PlayerError.BadCharacter, ctx);
                return;
            }
            var realName = this._chars[this.GetThingNumber(character, this._currentRoom, type)].ObjectName;
            // now, see if there's a give statement for this item in
            // this characters definition:
            var giveLine = 0;
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "give")) {
                    var ItemCheck = this.GetParameter(this._lines[i], ctx);
                    if (LCase(ItemCheck) == LCase(item)) {
                        giveLine = i;
                    }
                }
            }
            if (giveLine == 0) {
                if (article == "") {
                    article = "it";
                }
                this.SetStringContents("quest.error.charactername", realName, ctx);
                this.SetStringContents("quest.error.gender", Trim(this.GetGender(character, true, ctx)), ctx);
                this.SetStringContents("quest.error.article", article, ctx);
                this.PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
                return;
            }
            // now, execute the statement on GiveLine
            this.ExecuteScript(this.GetSecondChunk(this._lines[giveLine]), ctx);
        }
    };
    LegacyGame.prototype.ExecLook = function (lookLine, ctx) {
        var item;
        if (Trim(lookLine) == "look") {
            this.ShowRoomInfo((this._currentRoom), ctx);
        }
        else {
            if (this._gameAslVersion < 391) {
                var atPos = InStr(lookLine, " at ");
                if (atPos == 0) {
                    item = this.GetEverythingAfter(lookLine, "look ");
                }
                else {
                    item = Trim(Mid(lookLine, atPos + 4));
                }
            }
            else {
                if (this.BeginsWith(lookLine, "look at ")) {
                    item = this.GetEverythingAfter(lookLine, "look at ");
                }
                else if (this.BeginsWith(lookLine, "look in ")) {
                    item = this.GetEverythingAfter(lookLine, "look in ");
                }
                else if (this.BeginsWith(lookLine, "look on ")) {
                    item = this.GetEverythingAfter(lookLine, "look on ");
                }
                else if (this.BeginsWith(lookLine, "look inside ")) {
                    item = this.GetEverythingAfter(lookLine, "look inside ");
                }
                else {
                    item = this.GetEverythingAfter(lookLine, "look ");
                }
            }
            if (this._gameAslVersion >= 280) {
                var id = this.Disambiguate(item, "inventory;" + this._currentRoom, ctx);
                if (id <= 0) {
                    if (id != -2) {
                        this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                    this._badCmdBefore = "look at";
                    return;
                }
                this.DoLook(id, ctx);
            }
            else {
                if (this.BeginsWith(item, "the ")) {
                    item = this.GetEverythingAfter(item, "the ");
                }
                lookLine = this.RetrLine("object", item, "look", ctx);
                if (lookLine != "<unfound>") {
                    //Check for availability
                    if (!this.IsAvailable(item, Thing.Object, ctx)) {
                        lookLine = "<unfound>";
                    }
                }
                if (lookLine == "<unfound>") {
                    lookLine = this.RetrLine("character", item, "look", ctx);
                    if (lookLine != "<unfound>") {
                        if (!this.IsAvailable(item, Thing.Character, ctx)) {
                            lookLine = "<unfound>";
                        }
                    }
                    if (lookLine == "<unfound>") {
                        this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                        return;
                    }
                    else if (lookLine == "<undefined>") {
                        this.PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                        return;
                    }
                }
                else if (lookLine == "<undefined>") {
                    this.PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                    return;
                }
                var lookData = Trim(this.GetEverythingAfter(Trim(lookLine), "look "));
                if (Left(lookData, 1) == "<") {
                    var LookText = this.GetParameter(lookLine, ctx);
                    this.Print(LookText, ctx);
                }
                else {
                    this.ExecuteScript(lookData, ctx);
                }
            }
        }
    };
    LegacyGame.prototype.ExecSpeak = function (cmd, ctx) {
        if (this.BeginsWith(cmd, "the ")) {
            cmd = this.GetEverythingAfter(cmd, "the ");
        }
        var name = cmd;
        // if the "speak" parameter of the character c$'s definition
        // is just a parameter, say it - otherwise execute it as
        // a script.
        if (this._gameAslVersion >= 281) {
            var speakLine = "";
            var ObjID = this.Disambiguate(name, "inventory;" + this._currentRoom, ctx);
            if (ObjID <= 0) {
                if (ObjID != -2) {
                    this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                this._badCmdBefore = "speak to";
                return;
            }
            var foundSpeak = false;
            // First look for action, then look
            // for property, then check define
            // section:
            var o = this._objs[ObjID];
            for (var i = 1; i <= o.NumberActions; i++) {
                if (o.Actions[i].ActionName == "speak") {
                    speakLine = "speak " + o.Actions[i].Script;
                    foundSpeak = true;
                    break;
                }
            }
            if (!foundSpeak) {
                for (var i = 1; i <= o.NumberProperties; i++) {
                    if (o.Properties[i].PropertyName == "speak") {
                        speakLine = "speak <" + o.Properties[i].PropertyValue + ">";
                        foundSpeak = true;
                        break;
                    }
                }
            }
            // For some reason ASL3 < 311 looks for a "look" tag rather than
            // having had this set up at initialisation.
            if (this._gameAslVersion < 311 && !foundSpeak) {
                for (var i = o.DefinitionSectionStart; i <= o.DefinitionSectionEnd; i++) {
                    if (this.BeginsWith(this._lines[i], "speak ")) {
                        speakLine = this._lines[i];
                        foundSpeak = true;
                    }
                }
            }
            if (!foundSpeak) {
                this.SetStringContents("quest.error.gender", UCase(Left(this._objs[ObjID].Gender, 1)) + Mid(this._objs[ObjID].Gender, 2), ctx);
                this.PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
                return;
            }
            speakLine = this.GetEverythingAfter(speakLine, "speak ");
            if (this.BeginsWith(speakLine, "<")) {
                var text = this.GetParameter(speakLine, ctx);
                if (this._gameAslVersion >= 350) {
                    this.Print(text, ctx);
                }
                else {
                    this.Print(Chr(34) + text + Chr(34), ctx);
                }
            }
            else {
                this.ExecuteScript(speakLine, ctx, ObjID);
            }
        }
        else {
            var line = this.RetrLine("character", cmd, "speak", ctx);
            var type = Thing.Character;
            var data = Trim(this.GetEverythingAfter(Trim(line), "speak "));
            if (line != "<unfound>" && line != "<undefined>") {
                // Character exists; but is it available??
                if (!this.IsAvailable(cmd + "@" + this._currentRoom, type, ctx)) {
                    line = "<undefined>";
                }
            }
            if (line == "<undefined>") {
                this.PlayerErrorMessage(PlayerError.BadCharacter, ctx);
            }
            else if (line == "<unfound>") {
                this.SetStringContents("quest.error.gender", Trim(this.GetGender(cmd, true, ctx)), ctx);
                this.SetStringContents("quest.error.charactername", cmd, ctx);
                this.PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
            }
            else if (this.BeginsWith(data, "<")) {
                data = this.GetParameter(line, ctx);
                this.Print(Chr(34) + data + Chr(34), ctx);
            }
            else {
                this.ExecuteScript(data, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecTake = function (item, ctx) {
        var parentID = 0;
        var parentDisplayName;
        var foundItem = true;
        var foundTake = false;
        var id = this.Disambiguate(item, this._currentRoom, ctx);
        if (id < 0) {
            foundItem = false;
        }
        else {
            foundItem = true;
        }
        if (!foundItem) {
            if (id != -2) {
                if (this._gameAslVersion >= 410) {
                    id = this.Disambiguate(item, "inventory", ctx);
                    if (id >= 0) {
                        // Player already has this item
                        this.PlayerErrorMessage(PlayerError.AlreadyTaken, ctx);
                    }
                    else {
                        this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                }
                else if (this._gameAslVersion >= 391) {
                    this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                else {
                    this.PlayerErrorMessage(PlayerError.BadItem, ctx);
                }
            }
            this._badCmdBefore = "take";
            return;
        }
        else {
            this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
        }
        var isInContainer = false;
        if (this._gameAslVersion >= 391) {
            var canAccessObject = this.PlayerCanAccessObject(id);
            if (!canAccessObject.CanAccessObject) {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.BadTake, ctx, canAccessObject.ErrorMsg);
                return;
            }
            var parent = this.GetObjectProperty("parent", id, false, false);
            parentID = this.GetObjectIdNoAlias(parent);
        }
        if (this._gameAslVersion >= 280) {
            var t = this._objs[id].Take;
            if (isInContainer && (t.Type == TextActionType.Default || t.Type == TextActionType.Text)) {
                // So, we want to take an object that's in a container or surface. So first
                // we have to remove the object from that container.
                if (this._objs[parentID].ObjectAlias != "") {
                    parentDisplayName = this._objs[parentID].ObjectAlias;
                }
                else {
                    parentDisplayName = this._objs[parentID].ObjectName;
                }
                this.Print("(first removing " + this._objs[id].Article + " from " + parentDisplayName + ")", ctx);
                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                this.ExecCommand("remove " + this._objs[id].ObjectName, ctx, false, null, true);
                if (this.GetObjectProperty("parent", id, false, false) != "") {
                    // removing the object failed
                    return;
                }
            }
            if (t.Type == TextActionType.Default) {
                this.PlayerErrorMessage(PlayerError.DefaultTake, ctx);
                this.PlayerItem(item, true, ctx, id);
            }
            else if (t.Type == TextActionType.Text) {
                this.Print(t.Data, ctx);
                this.PlayerItem(item, true, ctx, id);
            }
            else if (t.Type == TextActionType.Script) {
                this.ExecuteScript(t.Data, ctx, id);
            }
            else {
                this.PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        }
        else {
            // find 'take' line
            for (var i = this._objs[id].DefinitionSectionStart + 1; i <= this._objs[id].DefinitionSectionEnd - 1; i++) {
                if (this.BeginsWith(this._lines[i], "take")) {
                    var script = Trim(this.GetEverythingAfter(Trim(this._lines[i]), "take"));
                    this.ExecuteScript(script, ctx, id);
                    foundTake = true;
                    i = this._objs[id].DefinitionSectionEnd;
                }
            }
            if (!foundTake) {
                this.PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecUse = function (useLine, ctx) {
        var endOnWith = 0;
        var useDeclareLine = "";
        var useOn;
        var useItem;
        useLine = Trim(this.GetEverythingAfter(useLine, "use "));
        var roomId = 0;
        roomId = this.GetRoomId(this._currentRoom, ctx);
        var onWithPos = InStr(useLine, " on ");
        if (onWithPos == 0) {
            onWithPos = InStr(useLine, " with ");
            endOnWith = onWithPos + 4;
        }
        else {
            endOnWith = onWithPos + 2;
        }
        if (onWithPos != 0) {
            useOn = Trim(Right(useLine, Len(useLine) - endOnWith));
            useItem = Trim(Left(useLine, onWithPos - 1));
        }
        else {
            useOn = "";
            useItem = useLine;
        }
        // see if player has this item:
        var id = 0;
        var notGotItem = false;
        if (this._gameAslVersion >= 280) {
            var foundItem = false;
            id = this.Disambiguate(useItem, "inventory", ctx);
            if (id > 0) {
                foundItem = true;
            }
            if (!foundItem) {
                if (id != -2) {
                    this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                }
                if (useOn == "") {
                    this._badCmdBefore = "use";
                }
                else {
                    this._badCmdBefore = "use";
                    this._badCmdAfter = "on " + useOn;
                }
                return;
            }
        }
        else {
            notGotItem = true;
            for (var i = 1; i <= this._numberItems; i++) {
                if (LCase(this._items[i].Name) == LCase(useItem)) {
                    if (!this._items[i].Got) {
                        notGotItem = true;
                        i = this._numberItems;
                    }
                    else {
                        notGotItem = false;
                    }
                }
            }
            if (notGotItem) {
                this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                return;
            }
        }
        var useScript = "";
        var foundUseScript = false;
        var foundUseOnObject = false;
        var useOnObjectId = 0;
        var found = false;
        if (this._gameAslVersion >= 280) {
            foundUseScript = false;
            if (useOn == "") {
                if (this._gameAslVersion < 410) {
                    var r = this._rooms[roomId];
                    for (var i = 1; i <= r.NumberUse; i++) {
                        if (LCase(this._objs[id].ObjectName) == LCase(r.Use[i].Text)) {
                            foundUseScript = true;
                            useScript = r.Use[i].Script;
                            break;
                        }
                    }
                }
                if (!foundUseScript) {
                    useScript = this._objs[id].Use;
                    if (useScript != "") {
                        foundUseScript = true;
                    }
                }
            }
            else {
                foundUseOnObject = false;
                useOnObjectId = this.Disambiguate(useOn, this._currentRoom, ctx);
                if (useOnObjectId > 0) {
                    foundUseOnObject = true;
                }
                else {
                    useOnObjectId = this.Disambiguate(useOn, "inventory", ctx);
                    if (useOnObjectId > 0) {
                        foundUseOnObject = true;
                    }
                }
                if (!foundUseOnObject) {
                    if (useOnObjectId != -2) {
                        this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                    this._badCmdBefore = "use " + useItem + " on";
                    return;
                }
                //now, for "use a on b", we have
                //ItemID=a and UseOnObjectID=b
                //first check b for use <a>:
                var o = this._objs[useOnObjectId];
                for (var i = 1; i <= o.NumberUseData; i++) {
                    if (o.UseData[i].UseType == UseType.UseSomethingOn && LCase(o.UseData[i].UseObject) == LCase(this._objs[id].ObjectName)) {
                        foundUseScript = true;
                        useScript = o.UseData[i].UseScript;
                        break;
                    }
                }
                if (!foundUseScript) {
                    //check a for use on <b>:
                    var u = this._objs[id];
                    for (var i = 1; i <= u.NumberUseData; i++) {
                        if (u.UseData[i].UseType == UseType.UseOnSomething && LCase(u.UseData[i].UseObject) == LCase(this._objs[useOnObjectId].ObjectName)) {
                            foundUseScript = true;
                            useScript = u.UseData[i].UseScript;
                            break;
                        }
                    }
                }
                if (!foundUseScript) {
                    //check b for use anything:
                    useScript = this._objs[useOnObjectId].UseAnything;
                    if (useScript != "") {
                        foundUseScript = true;
                        this.SetStringContents("quest.use.object.name", this._objs[id].ObjectName, ctx);
                    }
                }
                if (!foundUseScript) {
                    //check a for use on anything:
                    useScript = this._objs[id].UseOnAnything;
                    if (useScript != "") {
                        foundUseScript = true;
                        this.SetStringContents("quest.use.object.name", this._objs[useOnObjectId].ObjectName, ctx);
                    }
                }
            }
            if (foundUseScript) {
                this.ExecuteScript(useScript, ctx, id);
            }
            else {
                this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
            }
        }
        else {
            if (useOn != "") {
                useDeclareLine = this.RetrLineParam("object", useOn, "use", useItem, ctx);
            }
            else {
                found = false;
                for (var i = 1; i <= this._rooms[roomId].NumberUse; i++) {
                    if (LCase(this._rooms[roomId].Use[i].Text) == LCase(useItem)) {
                        useDeclareLine = "use <> " + this._rooms[roomId].Use[i].Script;
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    useDeclareLine = this.FindLine(this.GetDefineBlock("game"), "use", useItem);
                }
                if (!found && useDeclareLine == "") {
                    this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    return;
                }
            }
            if (useDeclareLine != "<unfound>" && useDeclareLine != "<undefined>" && useOn != "") {
                //Check for object availablity
                if (!this.IsAvailable(useOn, Thing.Object, ctx)) {
                    useDeclareLine = "<undefined>";
                }
            }
            if (useDeclareLine == "<undefined>") {
                useDeclareLine = this.RetrLineParam("character", useOn, "use", useItem, ctx);
                if (useDeclareLine != "<undefined>") {
                    //Check for character availability
                    if (!this.IsAvailable(useOn, Thing.Character, ctx)) {
                        useDeclareLine = "<undefined>";
                    }
                }
                if (useDeclareLine == "<undefined>") {
                    this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    return;
                }
                else if (useDeclareLine == "<unfound>") {
                    this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    return;
                }
            }
            else if (useDeclareLine == "<unfound>") {
                this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                return;
            }
            var script = Right(useDeclareLine, Len(useDeclareLine) - InStr(useDeclareLine, ">"));
            this.ExecuteScript(script, ctx);
        }
    };
    LegacyGame.prototype.ObjectActionUpdate = function (id, name, script, noUpdate) {
        if (noUpdate === void 0) { noUpdate = false; }
        var objectName;
        var sp = 0;
        var ep = 0;
        name = LCase(name);
        if (!noUpdate) {
            if (name == "take") {
                this._objs[id].Take.Data = script;
                this._objs[id].Take.Type = TextActionType.Script;
            }
            else if (name == "use") {
                this.AddToUseInfo(id, script);
            }
            else if (name == "gain") {
                this._objs[id].GainScript = script;
            }
            else if (name == "lose") {
                this._objs[id].LoseScript = script;
            }
            else if (this.BeginsWith(name, "use ")) {
                name = this.GetEverythingAfter(name, "use ");
                if (InStr(name, "'") > 0) {
                    sp = InStr(name, "'");
                    ep = InStrFrom(sp + 1, name, "'");
                    if (ep == 0) {
                        this.LogASLError("Missing ' in 'action <use " + name + "> " + this.ReportErrorLine(script));
                        return;
                    }
                    objectName = Mid(name, sp + 1, ep - sp - 1);
                    this.AddToUseInfo(id, Trim(Left(name, sp - 1)) + " <" + objectName + "> " + script);
                }
                else {
                    this.AddToUseInfo(id, name + " " + script);
                }
            }
            else if (this.BeginsWith(name, "give ")) {
                name = this.GetEverythingAfter(name, "give ");
                if (InStr(name, "'") > 0) {
                    sp = InStr(name, "'");
                    ep = InStrFrom(sp + 1, name, "'");
                    if (ep == 0) {
                        this.LogASLError("Missing ' in 'action <give " + name + "> " + this.ReportErrorLine(script));
                        return;
                    }
                    objectName = Mid(name, sp + 1, ep - sp - 1);
                    this.AddToGiveInfo(id, Trim(Left(name, sp - 1)) + " <" + objectName + "> " + script);
                }
                else {
                    this.AddToGiveInfo(id, name + " " + script);
                }
            }
        }
        if (this._gameFullyLoaded) {
            this.AddToObjectChangeLog(AppliesTo.Object, this._objs[id].ObjectName, name, "action <" + name + "> " + script);
        }
    };
    LegacyGame.prototype.ExecuteIf = function (scriptLine, ctx) {
        var ifLine = Trim(this.GetEverythingAfter(Trim(scriptLine), "if "));
        var obscuredLine = this.ObliterateParameters(ifLine);
        var thenPos = InStr(obscuredLine, "then");
        if (thenPos == 0) {
            var errMsg = "Expected 'then' missing from script statement '" + this.ReportErrorLine(scriptLine) + "' - statement bypassed.";
            this.LogASLError(errMsg, LogType.WarningError);
            return;
        }
        var conditions = Trim(Left(ifLine, thenPos - 1));
        thenPos = thenPos + 4;
        var elsePos = InStr(obscuredLine, "else");
        var thenEndPos = 0;
        if (elsePos == 0) {
            thenEndPos = Len(obscuredLine) + 1;
        }
        else {
            thenEndPos = elsePos - 1;
        }
        var thenScript = Trim(Mid(ifLine, thenPos, thenEndPos - thenPos));
        var elseScript = "";
        if (elsePos != 0) {
            elseScript = Trim(Right(ifLine, Len(ifLine) - (thenEndPos + 4)));
        }
        // Remove braces from around "then" and "else" script
        // commands, if present
        if (Left(thenScript, 1) == "{" && Right(thenScript, 1) == "}") {
            thenScript = Mid(thenScript, 2, Len(thenScript) - 2);
        }
        if (Left(elseScript, 1) == "{" && Right(elseScript, 1) == "}") {
            elseScript = Mid(elseScript, 2, Len(elseScript) - 2);
        }
        if (this.ExecuteConditions(conditions, ctx)) {
            this.ExecuteScript((thenScript), ctx);
        }
        else {
            if (elsePos != 0) {
                this.ExecuteScript((elseScript), ctx);
            }
        }
    };
    LegacyGame.prototype.ExecuteScript = function (scriptLine, ctx, newCallingObjectId) {
        if (newCallingObjectId === void 0) { newCallingObjectId = 0; }
        try {
            if (Trim(scriptLine) == "") {
                return;
            }
            if (this._gameFinished) {
                return;
            }
            if (InStr(scriptLine, "\n") > 0) {
                var curPos = 1;
                var finished = false;
                do {
                    var crLfPos = InStrFrom(curPos, scriptLine, "\n");
                    if (crLfPos == 0) {
                        finished = true;
                        crLfPos = Len(scriptLine) + 1;
                    }
                    var curScriptLine = Trim(Mid(scriptLine, curPos, crLfPos - curPos));
                    if (curScriptLine != "\n") {
                        this.ExecuteScript(curScriptLine, ctx);
                    }
                    curPos = crLfPos + 1;
                } while (!(finished));
                return;
            }
            if (newCallingObjectId != 0) {
                ctx.CallingObjectId = newCallingObjectId;
            }
            if (this.BeginsWith(scriptLine, "if ")) {
                this.ExecuteIf(scriptLine, ctx);
            }
            else if (this.BeginsWith(scriptLine, "select case ")) {
                this.ExecuteSelectCase(scriptLine, ctx);
            }
            else if (this.BeginsWith(scriptLine, "choose ")) {
                this.ExecuteChoose(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "set ")) {
                this.ExecuteSet(this.GetEverythingAfter(scriptLine, "set "), ctx);
            }
            else if (this.BeginsWith(scriptLine, "inc ") || this.BeginsWith(scriptLine, "dec ")) {
                this.ExecuteIncDec(scriptLine, ctx);
            }
            else if (this.BeginsWith(scriptLine, "say ")) {
                this.Print(Chr(34) + this.GetParameter(scriptLine, ctx) + Chr(34), ctx);
            }
            else if (this.BeginsWith(scriptLine, "do ")) {
                this.ExecuteDo(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "doaction ")) {
                this.ExecuteDoAction(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "give ")) {
                this.PlayerItem(this.GetParameter(scriptLine, ctx), true, ctx);
            }
            else if (this.BeginsWith(scriptLine, "lose ") || this.BeginsWith(scriptLine, "drop ")) {
                this.PlayerItem(this.GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (this.BeginsWith(scriptLine, "msg ")) {
                this.Print(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "speak ")) {
                this.Speak(this.GetParameter(scriptLine, ctx));
            }
            else if (this.BeginsWith(scriptLine, "helpmsg ")) {
                this.Print(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (Trim(LCase(scriptLine)) == "helpclose") {
            }
            else if (this.BeginsWith(scriptLine, "goto ")) {
                this.PlayGame(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "playerwin")) {
                this.FinishGame(StopType.Win, ctx);
            }
            else if (this.BeginsWith(scriptLine, "playerlose")) {
                this.FinishGame(StopType.Lose, ctx);
            }
            else if (Trim(LCase(scriptLine)) == "stop") {
                this.FinishGame(StopType.Null, ctx);
            }
            else if (this.BeginsWith(scriptLine, "playwav ")) {
                this.PlayWav(this.GetParameter(scriptLine, ctx));
            }
            else if (this.BeginsWith(scriptLine, "playmidi ")) {
                this.PlayMedia(this.GetParameter(scriptLine, ctx));
            }
            else if (this.BeginsWith(scriptLine, "playmp3 ")) {
                this.PlayMedia(this.GetParameter(scriptLine, ctx));
            }
            else if (Trim(LCase(scriptLine)) == "picture close") {
            }
            else if ((this._gameAslVersion >= 390 && this.BeginsWith(scriptLine, "picture popup ")) || (this._gameAslVersion >= 282 && this._gameAslVersion < 390 && this.BeginsWith(scriptLine, "picture ")) || (this._gameAslVersion < 282 && this.BeginsWith(scriptLine, "show "))) {
                // This command does nothing in the Quest 5 player, as there is no separate picture window
                this.ShowPicture(this.GetParameter(scriptLine, ctx));
            }
            else if ((this._gameAslVersion >= 390 && this.BeginsWith(scriptLine, "picture "))) {
                this.ShowPictureInText(this.GetParameter(scriptLine, ctx));
            }
            else if (this.BeginsWith(scriptLine, "animate persist ")) {
                this.ShowPicture(this.GetParameter(scriptLine, ctx));
            }
            else if (this.BeginsWith(scriptLine, "animate ")) {
                this.ShowPicture(this.GetParameter(scriptLine, ctx));
            }
            else if (this.BeginsWith(scriptLine, "extract ")) {
                this.ExtractFile(this.GetParameter(scriptLine, ctx));
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "hideobject ")) {
                this.SetAvailability(this.GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "showobject ")) {
                this.SetAvailability(this.GetParameter(scriptLine, ctx), true, ctx);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "moveobject ")) {
                this.ExecMoveThing(this.GetParameter(scriptLine, ctx), Thing.Object, ctx);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "hidechar ")) {
                this.SetAvailability(this.GetParameter(scriptLine, ctx), false, ctx, Thing.Character);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "showchar ")) {
                this.SetAvailability(this.GetParameter(scriptLine, ctx), true, ctx, Thing.Character);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "movechar ")) {
                this.ExecMoveThing(this.GetParameter(scriptLine, ctx), Thing.Character, ctx);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "revealobject ")) {
                this.SetVisibility(this.GetParameter(scriptLine, ctx), Thing.Object, true, ctx);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "concealobject ")) {
                this.SetVisibility(this.GetParameter(scriptLine, ctx), Thing.Object, false, ctx);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "revealchar ")) {
                this.SetVisibility(this.GetParameter(scriptLine, ctx), Thing.Character, true, ctx);
            }
            else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "concealchar ")) {
                this.SetVisibility(this.GetParameter(scriptLine, ctx), Thing.Character, false, ctx);
            }
            else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "hide ")) {
                this.SetAvailability(this.GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "show ")) {
                this.SetAvailability(this.GetParameter(scriptLine, ctx), true, ctx);
            }
            else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "move ")) {
                this.ExecMoveThing(this.GetParameter(scriptLine, ctx), Thing.Object, ctx);
            }
            else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "reveal ")) {
                this.SetVisibility(this.GetParameter(scriptLine, ctx), Thing.Object, true, ctx);
            }
            else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "conceal ")) {
                this.SetVisibility(this.GetParameter(scriptLine, ctx), Thing.Object, false, ctx);
            }
            else if (this._gameAslVersion >= 391 && this.BeginsWith(scriptLine, "open ")) {
                this.SetOpenClose(this.GetParameter(scriptLine, ctx), true, ctx);
            }
            else if (this._gameAslVersion >= 391 && this.BeginsWith(scriptLine, "close ")) {
                this.SetOpenClose(this.GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (this._gameAslVersion >= 391 && this.BeginsWith(scriptLine, "add ")) {
                this.ExecAddRemoveScript(this.GetParameter(scriptLine, ctx), true, ctx);
            }
            else if (this._gameAslVersion >= 391 && this.BeginsWith(scriptLine, "remove ")) {
                this.ExecAddRemoveScript(this.GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (this.BeginsWith(scriptLine, "clone ")) {
                this.ExecClone(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "exec ")) {
                this.ExecExec(scriptLine, ctx);
            }
            else if (this.BeginsWith(scriptLine, "setstring ")) {
                this.ExecSetString(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "setvar ")) {
                this.ExecSetVar(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "for ")) {
                this.ExecFor(this.GetEverythingAfter(scriptLine, "for "), ctx);
            }
            else if (this.BeginsWith(scriptLine, "property ")) {
                this.ExecProperty(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "type ")) {
                this.ExecType(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "action ")) {
                this.ExecuteAction(this.GetEverythingAfter(scriptLine, "action "), ctx);
            }
            else if (this.BeginsWith(scriptLine, "flag ")) {
                this.ExecuteFlag(this.GetEverythingAfter(scriptLine, "flag "), ctx);
            }
            else if (this.BeginsWith(scriptLine, "create ")) {
                this.ExecuteCreate(this.GetEverythingAfter(scriptLine, "create "), ctx);
            }
            else if (this.BeginsWith(scriptLine, "destroy exit ")) {
                this.DestroyExit(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "repeat ")) {
                this.ExecuteRepeat(this.GetEverythingAfter(scriptLine, "repeat "), ctx);
            }
            else if (this.BeginsWith(scriptLine, "enter ")) {
                this.ExecuteEnter(scriptLine, ctx);
            }
            else if (this.BeginsWith(scriptLine, "displaytext ")) {
                this.DisplayTextSection(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "helpdisplaytext ")) {
                this.DisplayTextSection(this.GetParameter(scriptLine, ctx), ctx);
            }
            else if (this.BeginsWith(scriptLine, "font ")) {
                this.SetFont(this.GetParameter(scriptLine, ctx));
            }
            else if (this.BeginsWith(scriptLine, "pause ")) {
                this.Pause(parseInt(this.GetParameter(scriptLine, ctx)));
            }
            else if (Trim(LCase(scriptLine)) == "clear") {
                this.DoClear();
            }
            else if (Trim(LCase(scriptLine)) == "helpclear") {
            }
            else if (this.BeginsWith(scriptLine, "background ")) {
                this.SetBackground(this.GetParameter(scriptLine, ctx));
            }
            else if (this.BeginsWith(scriptLine, "foreground ")) {
                this.SetForeground(this.GetParameter(scriptLine, ctx));
            }
            else if (Trim(LCase(scriptLine)) == "nointro") {
                this._autoIntro = false;
            }
            else if (this.BeginsWith(scriptLine, "debug ")) {
                this.LogASLError(this.GetParameter(scriptLine, ctx), LogType.Misc);
            }
            else if (this.BeginsWith(scriptLine, "mailto ")) {
                // TODO: Just write HTML directly
                var emailAddress = this.GetParameter(scriptLine, ctx);
            }
            else if (this.BeginsWith(scriptLine, "shell ") && this._gameAslVersion < 410) {
                this.LogASLError("'shell' is not supported in this version of Quest", LogType.WarningError);
            }
            else if (this.BeginsWith(scriptLine, "shellexe ") && this._gameAslVersion < 410) {
                this.LogASLError("'shellexe' is not supported in this version of Quest", LogType.WarningError);
            }
            else if (this.BeginsWith(scriptLine, "wait")) {
                this.ExecuteWait(Trim(this.GetEverythingAfter(Trim(scriptLine), "wait")), ctx);
            }
            else if (this.BeginsWith(scriptLine, "timeron ")) {
                this.SetTimerState(this.GetParameter(scriptLine, ctx), true);
            }
            else if (this.BeginsWith(scriptLine, "timeroff ")) {
                this.SetTimerState(this.GetParameter(scriptLine, ctx), false);
            }
            else if (Trim(LCase(scriptLine)) == "outputon") {
                this._outPutOn = true;
                this.UpdateObjectList(ctx);
                this.UpdateItems(ctx);
            }
            else if (Trim(LCase(scriptLine)) == "outputoff") {
                this._outPutOn = false;
            }
            else if (Trim(LCase(scriptLine)) == "panes off") {
                this._player.SetPanesVisible("off");
            }
            else if (Trim(LCase(scriptLine)) == "panes on") {
                this._player.SetPanesVisible("on");
            }
            else if (this.BeginsWith(scriptLine, "lock ")) {
                this.ExecuteLock(this.GetParameter(scriptLine, ctx), true);
            }
            else if (this.BeginsWith(scriptLine, "unlock ")) {
                this.ExecuteLock(this.GetParameter(scriptLine, ctx), false);
            }
            else if (this.BeginsWith(scriptLine, "playmod ") && this._gameAslVersion < 410) {
                this.LogASLError("'playmod' is not supported in this version of Quest", LogType.WarningError);
            }
            else if (this.BeginsWith(scriptLine, "modvolume") && this._gameAslVersion < 410) {
                this.LogASLError("'modvolume' is not supported in this version of Quest", LogType.WarningError);
            }
            else if (Trim(LCase(scriptLine)) == "dontprocess") {
                ctx.DontProcessCommand = true;
            }
            else if (this.BeginsWith(scriptLine, "return ")) {
                ctx.FunctionReturnValue = this.GetParameter(scriptLine, ctx);
            }
            else {
                if (!this.BeginsWith(scriptLine, "'")) {
                    this.LogASLError("Unrecognized keyword. Line reads: '" + Trim(this.ReportErrorLine(scriptLine)) + "'", LogType.WarningError);
                }
            }
        }
        catch (e) {
            this.Print("[An internal error occurred]", ctx);
            this.LogASLError("Error - '" + e + "' occurred processing script line '" + scriptLine + "'", LogType.InternalError);
        }
    };
    LegacyGame.prototype.ExecuteEnter = function (scriptLine, ctx) {
        this._commandOverrideModeOn = true;
        this._commandOverrideVariable = this.GetParameter(scriptLine, ctx);
        // Now, wait for CommandOverrideModeOn to be set
        // to False by ExecCommand. Execution can then resume.
        // TODO: Handle in TypeScript version
        this._commandOverrideModeOn = false;
        // State will have been changed to Working when the user typed their response,
        // and will be set back to Ready when the call to ExecCommand has finished
    };
    LegacyGame.prototype.ExecuteSet = function (setInstruction, ctx) {
        if (this._gameAslVersion >= 280) {
            if (this.BeginsWith(setInstruction, "interval ")) {
                var interval = this.GetParameter(setInstruction, ctx);
                var scp = InStr(interval, ";");
                if (scp == 0) {
                    this.LogASLError("Too few parameters in 'set " + setInstruction + "'", LogType.WarningError);
                    return;
                }
                var name = Trim(Left(interval, scp - 1));
                interval = (Val(Trim(Mid(interval, scp + 1)))).toString();
                var found = false;
                for (var i = 1; i <= this._numberTimers; i++) {
                    if (LCase(name) == LCase(this._timers[i].TimerName)) {
                        found = true;
                        this._timers[i].TimerInterval = parseInt(interval);
                        i = this._numberTimers;
                    }
                }
                if (!found) {
                    this.LogASLError("No such timer '" + name + "'", LogType.WarningError);
                    return;
                }
            }
            else if (this.BeginsWith(setInstruction, "string ")) {
                this.ExecSetString(this.GetParameter(setInstruction, ctx), ctx);
            }
            else if (this.BeginsWith(setInstruction, "numeric ")) {
                this.ExecSetVar(this.GetParameter(setInstruction, ctx), ctx);
            }
            else if (this.BeginsWith(setInstruction, "collectable ")) {
                this.ExecuteSetCollectable(this.GetParameter(setInstruction, ctx), ctx);
            }
            else {
                var result = this.SetUnknownVariableType(this.GetParameter(setInstruction, ctx), ctx);
                if (result == SetResult.Error) {
                    this.LogASLError("Error on setting 'set " + setInstruction + "'", LogType.WarningError);
                }
                else if (result == SetResult.Unfound) {
                    this.LogASLError("Variable type not specified in 'set " + setInstruction + "'", LogType.WarningError);
                }
            }
        }
        else {
            this.ExecuteSetCollectable(this.GetParameter(setInstruction, ctx), ctx);
        }
    };
    LegacyGame.prototype.FindStatement = function (block, statement) {
        // Finds a statement within a given block of lines
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            // Ignore sub-define blocks
            if (this.BeginsWith(this._lines[i], "define ")) {
                do {
                    i = i + 1;
                } while (!(Trim(this._lines[i]) == "end define"));
            }
            // Check to see if the line matches the statement
            // that is begin searched for
            if (this.BeginsWith(this._lines[i], statement)) {
                // Return the parameters between < and > :
                return this.GetParameter(this._lines[i], this._nullContext);
            }
        }
        return "";
    };
    LegacyGame.prototype.FindLine = function (block, statement, statementParam) {
        // Finds a statement within a given block of lines
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            // Ignore sub-define blocks
            if (this.BeginsWith(this._lines[i], "define ")) {
                do {
                    i = i + 1;
                } while (!(Trim(this._lines[i]) == "end define"));
            }
            // Check to see if the line matches the statement
            // that is begin searched for
            if (this.BeginsWith(this._lines[i], statement)) {
                if (UCase(Trim(this.GetParameter(this._lines[i], this._nullContext))) == UCase(Trim(statementParam))) {
                    return Trim(this._lines[i]);
                }
            }
        }
        return "";
    };
    LegacyGame.prototype.GetCollectableAmount = function (name) {
        for (var i = 1; i <= this._numCollectables; i++) {
            if (this._collectables[i].Name == name) {
                return this._collectables[i].Value;
            }
        }
        return 0;
    };
    LegacyGame.prototype.GetSecondChunk = function (line) {
        var endOfFirstBit = InStr(line, ">") + 1;
        var lengthOfKeyword = (Len(line) - endOfFirstBit) + 1;
        return Trim(Mid(line, endOfFirstBit, lengthOfKeyword));
    };
    LegacyGame.prototype.GoDirection = function (direction, ctx) {
        // leaves the current room in direction specified by
        // 'direction'
        var dirData = new TextAction();
        var id = this.GetRoomId(this._currentRoom, ctx);
        if (id == 0) {
            return;
        }
        if (this._gameAslVersion >= 410) {
            this._rooms[id].Exits.ExecuteGo(direction, ctx);
            return;
        }
        var r = this._rooms[id];
        if (direction == "north") {
            dirData = r.North;
        }
        else if (direction == "south") {
            dirData = r.South;
        }
        else if (direction == "west") {
            dirData = r.West;
        }
        else if (direction == "east") {
            dirData = r.East;
        }
        else if (direction == "northeast") {
            dirData = r.NorthEast;
        }
        else if (direction == "northwest") {
            dirData = r.NorthWest;
        }
        else if (direction == "southeast") {
            dirData = r.SouthEast;
        }
        else if (direction == "southwest") {
            dirData = r.SouthWest;
        }
        else if (direction == "up") {
            dirData = r.Up;
        }
        else if (direction == "down") {
            dirData = r.Down;
        }
        else if (direction == "out") {
            if (r.Out.Script == "") {
                dirData.Data = r.Out.Text;
                dirData.Type = TextActionType.Text;
            }
            else {
                dirData.Data = r.Out.Script;
                dirData.Type = TextActionType.Script;
            }
        }
        if (dirData.Type == TextActionType.Script && dirData.Data != "") {
            this.ExecuteScript(dirData.Data, ctx);
        }
        else if (dirData.Data != "") {
            var newRoom = dirData.Data;
            var scp = InStr(newRoom, ";");
            if (scp != 0) {
                newRoom = Trim(Mid(newRoom, scp + 1));
            }
            this.PlayGame(newRoom, ctx);
        }
        else {
            if (direction == "out") {
                this.PlayerErrorMessage(PlayerError.DefaultOut, ctx);
            }
            else {
                this.PlayerErrorMessage(PlayerError.BadPlace, ctx);
            }
        }
    };
    LegacyGame.prototype.GoToPlace = function (place, ctx) {
        // leaves the current room in direction specified by
        // 'direction'
        var destination = "";
        var placeData;
        var disallowed = false;
        placeData = this.PlaceExist(place, ctx);
        if (placeData != "") {
            destination = placeData;
        }
        else if (this.BeginsWith(place, "the ")) {
            var np = this.GetEverythingAfter(place, "the ");
            placeData = this.PlaceExist(np, ctx);
            if (placeData != "") {
                destination = placeData;
            }
            else {
                disallowed = true;
            }
        }
        else {
            disallowed = true;
        }
        if (destination != "") {
            if (InStr(destination, ";") > 0) {
                var s = Trim(Right(destination, Len(destination) - InStr(destination, ";")));
                this.ExecuteScript(s, ctx);
            }
            else {
                this.PlayGame(destination, ctx);
            }
        }
        if (disallowed) {
            this.PlayerErrorMessage(PlayerError.BadPlace, ctx);
        }
    };
    LegacyGame.prototype.InitialiseGame = function (filename, fromQsg, onSuccess, onFailure) {
        this._loadedFromQsg = fromQsg;
        this._changeLogRooms = new ChangeLog();
        this._changeLogObjects = new ChangeLog();
        this._changeLogRooms.AppliesToType = AppliesTo.Room;
        this._changeLogObjects.AppliesToType = AppliesTo.Object;
        this._outPutOn = true;
        this._useAbbreviations = true;
        var self = this;
        var doInitialise = function () {
            // Check version
            var gameBlock;
            gameBlock = self.GetDefineBlock("game");
            var aslVersion = "//";
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (self.BeginsWith(self._lines[i], "asl-version ")) {
                    aslVersion = self.GetParameter(self._lines[i], self._nullContext);
                }
            }
            if (aslVersion == "//") {
                self.LogASLError("File contains no version header.", LogType.WarningError);
            }
            else {
                self._gameAslVersion = parseInt(aslVersion);
                var recognisedVersions = "/100/200/210/217/280/281/282/283/284/285/300/310/311/320/350/390/391/392/400/410/";
                if (InStr(recognisedVersions, "/" + aslVersion + "/") == 0) {
                    self.LogASLError("Unrecognised ASL version number.", LogType.WarningError);
                }
            }
            self._listVerbs[ListType.ExitsList] = ["Go to"];
            if (self._gameAslVersion >= 280 && self._gameAslVersion < 390) {
                self._listVerbs[ListType.ObjectsList] = ["Look at", "Examine", "Take", "Speak to"];
                self._listVerbs[ListType.InventoryList] = ["Look at", "Examine", "Use", "Drop"];
            }
            else {
                self._listVerbs[ListType.ObjectsList] = ["Look at", "Take", "Speak to"];
                self._listVerbs[ListType.InventoryList] = ["Look at", "Use", "Drop"];
            }
            // Get the name of the game:
            self._gameName = self.GetParameter(self._lines[self.GetDefineBlock("game").StartLine], self._nullContext);
            self._player.UpdateGameName(self._gameName);
            self._player.Show("Panes");
            self._player.Show("Location");
            self._player.Show("Command");
            self.SetUpGameObject();
            self.SetUpOptions();
            for (var i = self.GetDefineBlock("game").StartLine + 1; i <= self.GetDefineBlock("game").EndLine - 1; i++) {
                if (self.BeginsWith(self._lines[i], "beforesave ")) {
                    self._beforeSaveScript = self.GetEverythingAfter(self._lines[i], "beforesave ");
                }
                else if (self.BeginsWith(self._lines[i], "onload ")) {
                    self._onLoadScript = self.GetEverythingAfter(self._lines[i], "onload ");
                }
            }
            self.SetDefaultPlayerErrorMessages();
            self.SetUpSynonyms();
            self.SetUpRoomData();
            if (self._gameAslVersion >= 410) {
                self.SetUpExits();
            }
            if (self._gameAslVersion < 280) {
                // Set up an array containing the names of all the items
                // used in the game, based on the possitems statement
                // of the 'define game' block.
                self.SetUpItemArrays();
            }
            if (self._gameAslVersion < 280) {
                // Now, go through the 'startitems' statement and set up
                // the items array so we start with those items mentioned.
                self.SetUpStartItems();
            }
            // Set up collectables.
            self.SetUpCollectables();
            self.SetUpDisplayVariables();
            // Set up characters and objects.
            self.SetUpCharObjectInfo();
            self.SetUpUserDefinedPlayerErrors();
            self.SetUpDefaultFonts();
            self.SetUpTurnScript();
            self.SetUpTimers();
            self.SetUpMenus();
            self._gameFileName = filename;
            self.LogASLError("Finished loading file.", LogType.Init);
            self._defaultRoomProperties = self.GetPropertiesInType("defaultroom", false);
            self._defaultProperties = self.GetPropertiesInType("default", false);
            onSuccess();
        };
        var onParseFailure = function () {
            self.LogASLError("Unable to open file", LogType.Init);
            var err = "Unable to open " + filename;
            if (self._openErrorReport) {
                // Strip last \n
                self._openErrorReport = Left(self._openErrorReport, Len(self._openErrorReport) - 1);
                err = err + ":\n\n" + self._openErrorReport;
            }
            self.Print("Error: " + err, self._nullContext);
            onFailure();
        };
        this.LogASLError("Opening file " + filename, LogType.Init);
        this.ParseFile(filename, doInitialise, onParseFailure);
    };
    LegacyGame.prototype.PlaceExist = function (placeName, ctx) {
        // Returns actual name of an available "place" exit, and if
        // script is executed on going in that direction, that script
        // is returned after a ";"
        var roomId = this.GetRoomId(this._currentRoom, ctx);
        var foundPlace = false;
        var scriptPresent = false;
        // check if place is available
        var r = this._rooms[roomId];
        for (var i = 1; i <= r.NumberPlaces; i++) {
            var checkPlace = r.Places[i].PlaceName;
            //remove any prefix and semicolon
            if (InStr(checkPlace, ";") > 0) {
                checkPlace = Trim(Right(checkPlace, Len(checkPlace) - (InStr(checkPlace, ";") + 1)));
            }
            var checkPlaceName = checkPlace;
            if (this._gameAslVersion >= 311 && r.Places[i].Script == "") {
                var destRoomId = this.GetRoomId(checkPlace, ctx);
                if (destRoomId != 0) {
                    if (this._rooms[destRoomId].RoomAlias != "") {
                        checkPlaceName = this._rooms[destRoomId].RoomAlias;
                    }
                }
            }
            if (LCase(checkPlaceName) == LCase(placeName)) {
                foundPlace = true;
                if (r.Places[i].Script != "") {
                    return checkPlace + ";" + r.Places[i].Script;
                }
                else {
                    return checkPlace;
                }
            }
        }
        return "";
    };
    LegacyGame.prototype.PlayerItem = function (item, got, ctx, objId) {
        if (objId === void 0) { objId = 0; }
        // Gives the player an item (if got=True) or takes an
        // item away from the player (if got=False).
        // If ASL>280, setting got=TRUE moves specified
        // *object* to room "inventory"; setting got=FALSE
        // drops object into current room.
        var foundObjectName = false;
        if (this._gameAslVersion >= 280) {
            if (objId == 0) {
                for (var i = 1; i <= this._numberObjs; i++) {
                    if (LCase(this._objs[i].ObjectName) == LCase(item)) {
                        objId = i;
                        break;
                    }
                }
            }
            if (objId != 0) {
                if (got) {
                    if (this._gameAslVersion >= 391) {
                        // Unset parent information, if any
                        this.AddToObjectProperties("not parent", objId, ctx);
                    }
                    this.MoveThing(this._objs[objId].ObjectName, "inventory", Thing.Object, ctx);
                    if (this._objs[objId].GainScript != "") {
                        this.ExecuteScript(this._objs[objId].GainScript, ctx);
                    }
                }
                else {
                    this.MoveThing(this._objs[objId].ObjectName, this._currentRoom, Thing.Object, ctx);
                    if (this._objs[objId].LoseScript != "") {
                        this.ExecuteScript(this._objs[objId].LoseScript, ctx);
                    }
                }
                foundObjectName = true;
            }
            if (!foundObjectName) {
                this.LogASLError("No such object '" + item + "'", LogType.WarningError);
            }
            else {
                this.UpdateItems(ctx);
                this.UpdateObjectList(ctx);
            }
        }
        else {
            for (var i = 1; i <= this._numberItems; i++) {
                if (this._items[i].Name == item) {
                    this._items[i].Got = got;
                    i = this._numberItems;
                }
            }
            this.UpdateItems(ctx);
        }
    };
    LegacyGame.prototype.PlayGame = function (room, ctx) {
        //plays the specified room
        var id = this.GetRoomId(room, ctx);
        if (id == 0) {
            this.LogASLError("No such room '" + room + "'", LogType.WarningError);
            return;
        }
        this._currentRoom = room;
        this.SetStringContents("quest.currentroom", room, ctx);
        if (this._gameAslVersion >= 391 && this._gameAslVersion < 410) {
            this.AddToObjectProperties("visited", this._rooms[id].ObjId, ctx);
        }
        this.ShowRoomInfo(room, ctx);
        this.UpdateItems(ctx);
        // Find script lines and execute them.
        if (this._rooms[id].Script != "") {
            var script = this._rooms[id].Script;
            this.ExecuteScript(script, ctx);
        }
        if (this._gameAslVersion >= 410) {
            this.AddToObjectProperties("visited", this._rooms[id].ObjId, ctx);
        }
    };
    LegacyGame.prototype.Print = function (txt, ctx) {
        var printString = "";
        if (txt == "") {
            this.DoPrint(printString);
        }
        else {
            for (var i = 1; i <= Len(txt); i++) {
                var printThis = true;
                if (Mid(txt, i, 2) == "|w") {
                    this.DoPrint(printString);
                    printString = "";
                    printThis = false;
                    i = i + 1;
                    this.ExecuteScript("wait <>", ctx);
                }
                else if (Mid(txt, i, 2) == "|c") {
                    switch (Mid(txt, i, 3)) {
                        case "|cb":
                        case "|cr":
                        case "|cl":
                        case "|cy":
                        case "|cg":
                            // Do nothing - we don't want to remove the colour formatting codes.
                            break;
                        default:
                            this.DoPrint(printString);
                            printString = "";
                            printThis = false;
                            i = i + 1;
                            this.ExecuteScript("clear", ctx);
                            break;
                    }
                }
                if (printThis) {
                    printString = printString + Mid(txt, i, 1);
                }
            }
            if (printString != "") {
                this.DoPrint(printString);
            }
        }
    };
    LegacyGame.prototype.RetrLine = function (blockType, param, line, ctx) {
        var searchblock;
        if (blockType == "object") {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Object);
        }
        else {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Character);
        }
        if (searchblock.StartLine == 0 && searchblock.EndLine == 0) {
            return "<undefined>";
        }
        for (var i = searchblock.StartLine + 1; i <= searchblock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], line)) {
                return Trim(this._lines[i]);
            }
        }
        return "<unfound>";
    };
    LegacyGame.prototype.RetrLineParam = function (blockType, param, line, lineParam, ctx) {
        var searchblock;
        if (blockType == "object") {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Object);
        }
        else {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Character);
        }
        if (searchblock.StartLine == 0 && searchblock.EndLine == 0) {
            return "<undefined>";
        }
        for (var i = searchblock.StartLine + 1; i <= searchblock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], line) && LCase(this.GetParameter(this._lines[i], ctx)) == LCase(lineParam)) {
                return Trim(this._lines[i]);
            }
        }
        return "<unfound>";
    };
    LegacyGame.prototype.SetUpCollectables = function () {
        var lastItem = false;
        this._numCollectables = 0;
        // Initialise collectables:
        // First, find the collectables section within the define
        // game block, and get its parameters:
        for (var a = this.GetDefineBlock("game").StartLine + 1; a <= this.GetDefineBlock("game").EndLine - 1; a++) {
            if (this.BeginsWith(this._lines[a], "collectables ")) {
                var collectables = Trim(this.GetParameter(this._lines[a], this._nullContext, false));
                // if collectables is a null string, there are no
                // collectables. Otherwise, there is one more object than
                // the number of commas. So, first check to see if we have
                // no objects:
                if (collectables != "") {
                    this._numCollectables = 1;
                    var pos = 1;
                    do {
                        if (!this._collectables)
                            this._collectables = [];
                        this._collectables[this._numCollectables] = new Collectable();
                        var nextComma = InStrFrom(pos + 1, collectables, ",");
                        if (nextComma == 0) {
                            nextComma = InStrFrom(pos + 1, collectables, ";");
                        }
                        //If there are no more commas, we want everything
                        //up to the end of the string, and then to exit
                        //the loop:
                        if (nextComma == 0) {
                            nextComma = Len(collectables) + 1;
                            lastItem = true;
                        }
                        //Get item info
                        var info = Trim(Mid(collectables, pos, nextComma - pos));
                        this._collectables[this._numCollectables].Name = Trim(Left(info, InStr(info, " ")));
                        var ep = InStr(info, "=");
                        var sp1 = InStr(info, " ");
                        var sp2 = InStrFrom(ep, info, " ");
                        if (sp2 == 0) {
                            sp2 = Len(info) + 1;
                        }
                        var t = Trim(Mid(info, sp1 + 1, ep - sp1 - 1));
                        var i = Trim(Mid(info, ep + 1, sp2 - ep - 1));
                        if (Left(t, 1) == "d") {
                            t = Mid(t, 2);
                            this._collectables[this._numCollectables].DisplayWhenZero = false;
                        }
                        else {
                            this._collectables[this._numCollectables].DisplayWhenZero = true;
                        }
                        this._collectables[this._numCollectables].Type = t;
                        this._collectables[this._numCollectables].Value = Val(i);
                        // Get display string between square brackets
                        var obp = InStr(info, "[");
                        var cbp = InStr(info, "]");
                        if (obp == 0) {
                            this._collectables[this._numCollectables].Display = "<def>";
                        }
                        else {
                            var b = Mid(info, obp + 1, (cbp - 1) - obp);
                            this._collectables[this._numCollectables].Display = Trim(b);
                        }
                        pos = nextComma + 1;
                        this._numCollectables = this._numCollectables + 1;
                    } while (!lastItem);
                    this._numCollectables = this._numCollectables - 1;
                }
            }
        }
    };
    LegacyGame.prototype.SetUpItemArrays = function () {
        var lastItem = false;
        this._numberItems = 0;
        // Initialise items:
        // First, find the possitems section within the define game
        // block, and get its parameters:
        for (var a = this.GetDefineBlock("game").StartLine + 1; a <= this.GetDefineBlock("game").EndLine - 1; a++) {
            if (this.BeginsWith(this._lines[a], "possitems ") || this.BeginsWith(this._lines[a], "items ")) {
                var possItems = this.GetParameter(this._lines[a], this._nullContext);
                if (possItems != "") {
                    this._numberItems = this._numberItems + 1;
                    var pos = 1;
                    do {
                        if (!this._items)
                            this._items = [];
                        this._items[this._numberItems] = new ItemType();
                        var nextComma = InStrFrom(pos + 1, possItems, ",");
                        if (nextComma == 0) {
                            nextComma = InStrFrom(pos + 1, possItems, ";");
                        }
                        //If there are no more commas, we want everything
                        //up to the end of the string, and then to exit
                        //the loop:
                        if (nextComma == 0) {
                            nextComma = Len(possItems) + 1;
                            lastItem = true;
                        }
                        //Get item name
                        this._items[this._numberItems].Name = Trim(Mid(possItems, pos, nextComma - pos));
                        this._items[this._numberItems].Got = false;
                        pos = nextComma + 1;
                        this._numberItems = this._numberItems + 1;
                    } while (!lastItem);
                    this._numberItems = this._numberItems - 1;
                }
            }
        }
    };
    LegacyGame.prototype.SetUpStartItems = function () {
        var lastItem = false;
        for (var a = this.GetDefineBlock("game").StartLine + 1; a <= this.GetDefineBlock("game").EndLine - 1; a++) {
            if (this.BeginsWith(this._lines[a], "startitems ")) {
                var startItems = this.GetParameter(this._lines[a], this._nullContext);
                if (startItems != "") {
                    var pos = 1;
                    do {
                        var nextComma = InStrFrom(pos + 1, startItems, ",");
                        if (nextComma == 0) {
                            nextComma = InStrFrom(pos + 1, startItems, ";");
                        }
                        //If there are no more commas, we want everything
                        //up to the end of the string, and then to exit
                        //the loop:
                        if (nextComma == 0) {
                            nextComma = Len(startItems) + 1;
                            lastItem = true;
                        }
                        //Get item name
                        var name = Trim(Mid(startItems, pos, nextComma - pos));
                        //Find which item this is, and set it
                        for (var i = 1; i <= this._numberItems; i++) {
                            if (this._items[i].Name == name) {
                                this._items[i].Got = true;
                                break;
                            }
                        }
                        pos = nextComma + 1;
                    } while (!lastItem);
                }
            }
        }
    };
    LegacyGame.prototype.ShowHelp = function (ctx) {
        this.Print("|b|cl|s14Quest Quick Help|xb|cb|s00", ctx);
        this.Print("", ctx);
        this.Print("|cl|bMoving|xb|cb Press the direction buttons in the 'Compass' pane, or type |bGO NORTH|xb, |bSOUTH|xb, |bE|xb, etc. |xn", ctx);
        this.Print("To go into a place, type |bGO TO ...|xb . To leave a place, type |bOUT, EXIT|xb or |bLEAVE|xb, or press the '|crOUT|cb' button.|n", ctx);
        this.Print("|cl|bObjects and Characters|xb|cb Use |bTAKE ...|xb, |bGIVE ... TO ...|xb, |bTALK|xb/|bSPEAK TO ...|xb, |bUSE ... ON|xb/|bWITH ...|xb, |bLOOK AT ...|xb, etc.|n", ctx);
        this.Print("|cl|bMisc|xb|cb Type |bABOUT|xb to get information on the current game. The next turn after referring to an object or character, you can use |bIT|xb, |bHIM|xb etc. as appropriate to refer to it/him/etc. again. If you make a mistake when typing an object's name, type |bOOPS|xb followed by your correction.|n", ctx);
        this.Print("|cl|bKeyboard shortcuts|xb|cb Press the |crup arrow|cb and |crdown arrow|cb to scroll through commands you have already typed in. Press |crEsc|cb to clear the command box.", ctx);
    };
    LegacyGame.prototype.ReadCatalog = function (data) {
        var nullPos = InStr(data, Chr(0));
        this._numResources = parseInt(this.DecryptString(Left(data, nullPos - 1)));
        if (!this._resources)
            this._resources = [];
        this._resources[this._numResources] = new ResourceType();
        data = Mid(data, nullPos + 1);
        var resourceStart = 0;
        for (var i = 1; i <= this._numResources; i++) {
            var r = this._resources[i];
            nullPos = InStr(data, Chr(0));
            r.ResourceName = this.DecryptString(Left(data, nullPos - 1));
            data = Mid(data, nullPos + 1);
            nullPos = InStr(data, Chr(0));
            r.ResourceLength = parseInt(this.DecryptString(Left(data, nullPos - 1)));
            data = Mid(data, nullPos + 1);
            r.ResourceStart = resourceStart;
            resourceStart = resourceStart + r.ResourceLength;
            r.Extracted = false;
        }
    };
    LegacyGame.prototype.UpdateDirButtons = function (dirs, ctx) {
        var compassExits = [];
        if (InStr(dirs, "n") > 0) {
            this.AddCompassExit(compassExits, "north");
        }
        if (InStr(dirs, "s") > 0) {
            this.AddCompassExit(compassExits, "south");
        }
        if (InStr(dirs, "w") > 0) {
            this.AddCompassExit(compassExits, "west");
        }
        if (InStr(dirs, "e") > 0) {
            this.AddCompassExit(compassExits, "east");
        }
        if (InStr(dirs, "o") > 0) {
            this.AddCompassExit(compassExits, "out");
        }
        if (InStr(dirs, "a") > 0) {
            this.AddCompassExit(compassExits, "northeast");
        }
        if (InStr(dirs, "b") > 0) {
            this.AddCompassExit(compassExits, "northwest");
        }
        if (InStr(dirs, "c") > 0) {
            this.AddCompassExit(compassExits, "southeast");
        }
        if (InStr(dirs, "d") > 0) {
            this.AddCompassExit(compassExits, "southwest");
        }
        if (InStr(dirs, "u") > 0) {
            this.AddCompassExit(compassExits, "up");
        }
        if (InStr(dirs, "f") > 0) {
            this.AddCompassExit(compassExits, "down");
        }
        this._compassExits = compassExits;
        this.UpdateExitsList();
    };
    LegacyGame.prototype.AddCompassExit = function (exitList, name) {
        exitList.push(new ListData(name, this._listVerbs[ListType.ExitsList]));
    };
    LegacyGame.prototype.UpdateDoorways = function (roomId, ctx) {
        var roomDisplayText = "";
        var outPlace = "";
        var directions = "";
        var nsew = "";
        var outPlaceName = "";
        var outPlacePrefix = "";
        var n = "north";
        var s = "south";
        var e = "east";
        var w = "west";
        var ne = "northeast";
        var nw = "northwest";
        var se = "southeast";
        var sw = "southwest";
        var u = "up";
        var d = "down";
        var o = "out";
        if (this._gameAslVersion >= 410) {
            var result = this._rooms[roomId].Exits.GetAvailableDirectionsDescription();
            roomDisplayText = result.Description;
            directions = result.List;
        }
        else {
            if (this._rooms[roomId].Out.Text != "") {
                outPlace = this._rooms[roomId].Out.Text;
                //remove any prefix semicolon from printed text
                var scp = InStr(outPlace, ";");
                if (scp == 0) {
                    outPlaceName = outPlace;
                }
                else {
                    outPlaceName = Trim(Mid(outPlace, scp + 1));
                    outPlacePrefix = Trim(Left(outPlace, scp - 1));
                    outPlace = outPlacePrefix + " " + outPlaceName;
                }
            }
            if (this._rooms[roomId].North.Data != "") {
                nsew = nsew + "|b" + n + "|xb, ";
                directions = directions + "n";
            }
            if (this._rooms[roomId].South.Data != "") {
                nsew = nsew + "|b" + s + "|xb, ";
                directions = directions + "s";
            }
            if (this._rooms[roomId].East.Data != "") {
                nsew = nsew + "|b" + e + "|xb, ";
                directions = directions + "e";
            }
            if (this._rooms[roomId].West.Data != "") {
                nsew = nsew + "|b" + w + "|xb, ";
                directions = directions + "w";
            }
            if (this._rooms[roomId].NorthEast.Data != "") {
                nsew = nsew + "|b" + ne + "|xb, ";
                directions = directions + "a";
            }
            if (this._rooms[roomId].NorthWest.Data != "") {
                nsew = nsew + "|b" + nw + "|xb, ";
                directions = directions + "b";
            }
            if (this._rooms[roomId].SouthEast.Data != "") {
                nsew = nsew + "|b" + se + "|xb, ";
                directions = directions + "c";
            }
            if (this._rooms[roomId].SouthWest.Data != "") {
                nsew = nsew + "|b" + sw + "|xb, ";
                directions = directions + "d";
            }
            if (this._rooms[roomId].Up.Data != "") {
                nsew = nsew + "|b" + u + "|xb, ";
                directions = directions + "u";
            }
            if (this._rooms[roomId].Down.Data != "") {
                nsew = nsew + "|b" + d + "|xb, ";
                directions = directions + "f";
            }
            if (outPlace != "") {
                //see if outside has an alias
                var outPlaceAlias = this._rooms[this.GetRoomId(outPlaceName, ctx)].RoomAlias;
                if (outPlaceAlias == "") {
                    outPlaceAlias = outPlace;
                }
                else {
                    if (this._gameAslVersion >= 360) {
                        if (outPlacePrefix != "") {
                            outPlaceAlias = outPlacePrefix + " " + outPlaceAlias;
                        }
                    }
                }
                roomDisplayText = roomDisplayText + "You can go |bout|xb to " + outPlaceAlias + ".";
                if (nsew != "") {
                    roomDisplayText = roomDisplayText + " ";
                }
                directions = directions + "o";
                if (this._gameAslVersion >= 280) {
                    this.SetStringContents("quest.doorways.out", outPlaceName, ctx);
                }
                else {
                    this.SetStringContents("quest.doorways.out", outPlaceAlias, ctx);
                }
                this.SetStringContents("quest.doorways.out.display", outPlaceAlias, ctx);
            }
            else {
                this.SetStringContents("quest.doorways.out", "", ctx);
                this.SetStringContents("quest.doorways.out.display", "", ctx);
            }
            if (nsew != "") {
                //strip final comma
                nsew = Left(nsew, Len(nsew) - 2);
                var cp = InStr(nsew, ",");
                if (cp != 0) {
                    var finished = false;
                    do {
                        var ncp = InStrFrom(cp + 1, nsew, ",");
                        if (ncp == 0) {
                            finished = true;
                        }
                        else {
                            cp = ncp;
                        }
                    } while (!(finished));
                    nsew = Trim(Left(nsew, cp - 1)) + " or " + Trim(Mid(nsew, cp + 1));
                }
                roomDisplayText = roomDisplayText + "You can go " + nsew + ".";
                this.SetStringContents("quest.doorways.dirs", nsew, ctx);
            }
            else {
                this.SetStringContents("quest.doorways.dirs", "", ctx);
            }
        }
        this.UpdateDirButtons(directions, ctx);
        return roomDisplayText;
    };
    LegacyGame.prototype.UpdateItems = function (ctx) {
        // displays the items a player has
        var invList = [];
        if (!this._outPutOn) {
            return;
        }
        var name;
        if (this._gameAslVersion >= 280) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (this._objs[i].ContainerRoom == "inventory" && this._objs[i].Exists && this._objs[i].Visible) {
                    if (this._objs[i].ObjectAlias == "") {
                        name = this._objs[i].ObjectName;
                    }
                    else {
                        name = this._objs[i].ObjectAlias;
                    }
                    invList.push(new ListData(this.CapFirst(name), this._listVerbs[ListType.InventoryList]));
                }
            }
        }
        else {
            for (var j = 1; j <= this._numberItems; j++) {
                if (this._items[j].Got) {
                    invList.push(new ListData(this.CapFirst(this._items[j].Name), this._listVerbs[ListType.InventoryList]));
                }
            }
        }
        this._player.UpdateInventoryList(invList);
        if (this._gameAslVersion >= 284) {
            this.UpdateStatusVars(ctx);
        }
        else {
            if (this._numCollectables > 0) {
                var status = "";
                for (var j = 1; j <= this._numCollectables; j++) {
                    var k = this.DisplayCollectableInfo(j);
                    if (k != "<null>") {
                        if (status.length > 0) {
                            status += "\n";
                        }
                        status += k;
                    }
                }
                this._player.SetStatusText(status);
            }
        }
    };
    LegacyGame.prototype.FinishGame = function (stopType, ctx) {
        if (stopType == StopType.Win) {
            this.DisplayTextSection("win", ctx);
        }
        else if (stopType == StopType.Lose) {
            this.DisplayTextSection("lose", ctx);
        }
        this.GameFinished();
    };
    LegacyGame.prototype.UpdateObjectList = function (ctx) {
        // Updates object list
        var shownPlaceName;
        var objSuffix;
        var charsViewable = "";
        var charsFound = 0;
        var noFormatObjsViewable;
        var charList;
        var objsViewable = "";
        var objsFound = 0;
        var objListString;
        var noFormatObjListString;
        if (!this._outPutOn) {
            return;
        }
        var objList = [];
        var exitList = [];
        //find the room
        var roomBlock;
        roomBlock = this.DefineBlockParam("room", this._currentRoom);
        //FIND CHARACTERS ===
        if (this._gameAslVersion < 281) {
            // go through Chars() array
            for (var i = 1; i <= this._numberChars; i++) {
                if (this._chars[i].ContainerRoom == this._currentRoom && this._chars[i].Exists && this._chars[i].Visible) {
                    this.AddToObjectList(objList, exitList, this._chars[i].ObjectName, Thing.Character);
                    charsViewable = charsViewable + this._chars[i].Prefix + "|b" + this._chars[i].ObjectName + "|xb" + this._chars[i].Suffix + ", ";
                    charsFound = charsFound + 1;
                }
            }
            if (charsFound == 0) {
                this.SetStringContents("quest.characters", "", ctx);
            }
            else {
                //chop off final comma and add full stop (.)
                charList = Left(charsViewable, Len(charsViewable) - 2);
                this.SetStringContents("quest.characters", charList, ctx);
            }
        }
        //FIND OBJECTS
        noFormatObjsViewable = "";
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ContainerRoom) == LCase(this._currentRoom) && this._objs[i].Exists && this._objs[i].Visible && !this._objs[i].IsExit) {
                objSuffix = this._objs[i].Suffix;
                if (objSuffix != "") {
                    objSuffix = " " + objSuffix;
                }
                if (this._objs[i].ObjectAlias == "") {
                    this.AddToObjectList(objList, exitList, this._objs[i].ObjectName, Thing.Object);
                    objsViewable = objsViewable + this._objs[i].Prefix + "|b" + this._objs[i].ObjectName + "|xb" + objSuffix + ", ";
                    noFormatObjsViewable = noFormatObjsViewable + this._objs[i].Prefix + this._objs[i].ObjectName + ", ";
                }
                else {
                    this.AddToObjectList(objList, exitList, this._objs[i].ObjectAlias, Thing.Object);
                    objsViewable = objsViewable + this._objs[i].Prefix + "|b" + this._objs[i].ObjectAlias + "|xb" + objSuffix + ", ";
                    noFormatObjsViewable = noFormatObjsViewable + this._objs[i].Prefix + this._objs[i].ObjectAlias + ", ";
                }
                objsFound = objsFound + 1;
            }
        }
        if (objsFound != 0) {
            objListString = Left(objsViewable, Len(objsViewable) - 2);
            noFormatObjListString = Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2);
            this.SetStringContents("quest.objects", Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2), ctx);
            this.SetStringContents("quest.formatobjects", objListString, ctx);
        }
        else {
            this.SetStringContents("quest.objects", "", ctx);
            this.SetStringContents("quest.formatobjects", "", ctx);
        }
        //FIND DOORWAYS
        var roomId = 0;
        roomId = this.GetRoomId(this._currentRoom, ctx);
        if (roomId > 0) {
            if (this._gameAslVersion >= 410) {
                var places = this._rooms[roomId].Exits.GetPlaces();
                for (var key in places) {
                    var roomExit = places[key];
                    this.AddToObjectList(objList, exitList, roomExit.GetDisplayName(), Thing.Room);
                }
                ;
            }
            else {
                var r = this._rooms[roomId];
                for (var i = 1; i <= r.NumberPlaces; i++) {
                    if (this._gameAslVersion >= 311 && this._rooms[roomId].Places[i].Script == "") {
                        var placeId = this.GetRoomId(this._rooms[roomId].Places[i].PlaceName, ctx);
                        if (placeId == 0) {
                            shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                        }
                        else {
                            if (this._rooms[placeId].RoomAlias != "") {
                                shownPlaceName = this._rooms[placeId].RoomAlias;
                            }
                            else {
                                shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                            }
                        }
                    }
                    else {
                        shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                    }
                    this.AddToObjectList(objList, exitList, shownPlaceName, Thing.Room);
                }
            }
        }
        // TODO...
        this._gotoExits = exitList;
        this.UpdateExitsList();
    };
    LegacyGame.prototype.UpdateStatusVars = function (ctx) {
        var displayData;
        var status = "";
        if (this._numDisplayStrings > 0) {
            for (var i = 1; i <= this._numDisplayStrings; i++) {
                displayData = this.DisplayStatusVariableInfo(i, VarType.String, ctx);
                if (displayData != "") {
                    if (status.length > 0) {
                        status += "\n";
                    }
                    status += displayData;
                }
            }
        }
        if (this._numDisplayNumerics > 0) {
            for (var i = 1; i <= this._numDisplayNumerics; i++) {
                displayData = this.DisplayStatusVariableInfo(i, VarType.Numeric, ctx);
                if (displayData != "") {
                    if (status.length > 0) {
                        status += "\n";
                    }
                    status += displayData;
                }
            }
        }
        this._player.SetStatusText(status);
    };
    LegacyGame.prototype.UpdateVisibilityInContainers = function (ctx, onlyParent) {
        if (onlyParent === void 0) { onlyParent = ""; }
        // Use OnlyParent to only update objects that are contained by a specific parent
        var parentId = 0;
        var parent;
        var parentIsTransparent = false;
        var parentIsOpen = false;
        var parentIsSeen = false;
        var parentIsSurface = false;
        if (this._gameAslVersion < 391) {
            return;
        }
        if (onlyParent != "") {
            onlyParent = LCase(onlyParent);
            parentId = this.GetObjectIdNoAlias(onlyParent);
            parentIsOpen = this.IsYes(this.GetObjectProperty("opened", parentId, true, false));
            parentIsTransparent = this.IsYes(this.GetObjectProperty("transparent", parentId, true, false));
            parentIsSeen = this.IsYes(this.GetObjectProperty("seen", parentId, true, false));
            parentIsSurface = this.IsYes(this.GetObjectProperty("surface", parentId, true, false));
        }
        for (var i = 1; i <= this._numberObjs; i++) {
            // If object has a parent object
            parent = this.GetObjectProperty("parent", i, false, false);
            if (parent != "") {
                // Check if that parent is open, or transparent
                if (onlyParent == "") {
                    parentId = this.GetObjectIdNoAlias(parent);
                    parentIsOpen = this.IsYes(this.GetObjectProperty("opened", parentId, true, false));
                    parentIsTransparent = this.IsYes(this.GetObjectProperty("transparent", parentId, true, false));
                    parentIsSeen = this.IsYes(this.GetObjectProperty("seen", parentId, true, false));
                    parentIsSurface = this.IsYes(this.GetObjectProperty("surface", parentId, true, false));
                }
                if (onlyParent == "" || (LCase(parent) == onlyParent)) {
                    if (parentIsSurface || ((parentIsOpen || parentIsTransparent) && parentIsSeen)) {
                        // If the parent is a surface, then the contents are always available.
                        // Otherwise, only if the parent has been seen, AND is either open or transparent,
                        // then the contents are available.
                        this.SetAvailability(this._objs[i].ObjectName, true, ctx);
                    }
                    else {
                        this.SetAvailability(this._objs[i].ObjectName, false, ctx);
                    }
                }
            }
        }
    };
    LegacyGame.prototype.PlayerCanAccessObject = function (id, colObjects) {
        if (colObjects === void 0) { colObjects = null; }
        // Called to see if a player can interact with an object (take it, open it etc.).
        // For example, if the object is on a surface which is inside a closed container,
        // the object cannot be accessed.
        var parent;
        var parentId = 0;
        var parentDisplayName;
        var result = new PlayerCanAccessObjectResult();
        var hierarchy = "";
        if (this.IsYes(this.GetObjectProperty("parent", id, true, false))) {
            // Object is in a container...
            parent = this.GetObjectProperty("parent", id, false, false);
            parentId = this.GetObjectIdNoAlias(parent);
            // But if it's a surface then it's OK
            if (!this.IsYes(this.GetObjectProperty("surface", parentId, true, false)) && !this.IsYes(this.GetObjectProperty("opened", parentId, true, false))) {
                // Parent has no "opened" property, so it's closed. Hence
                // object can't be accessed
                if (this._objs[parentId].ObjectAlias != "") {
                    parentDisplayName = this._objs[parentId].ObjectAlias;
                }
                else {
                    parentDisplayName = this._objs[parentId].ObjectName;
                }
                result.CanAccessObject = false;
                result.ErrorMsg = "inside closed " + parentDisplayName;
                return result;
            }
            // Is the parent itself accessible?
            if (colObjects == null) {
                colObjects = [];
            }
            if (colObjects.indexOf(parentId) != -1) {
                // We've already encountered this parent while recursively calling
                // this function - we're in a loop of parents!
                colObjects.forEach(function (objId) {
                    hierarchy = hierarchy + this._objs[objId].ObjectName + " -> ";
                }, this);
                hierarchy = hierarchy + this._objs[parentId].ObjectName;
                this.LogASLError("Looped object parents detected: " + hierarchy);
                result.CanAccessObject = false;
                return result;
            }
            colObjects.push(parentId);
            return this.PlayerCanAccessObject(parentId, colObjects);
        }
        result.CanAccessObject = true;
        return result;
    };
    LegacyGame.prototype.GetGoToExits = function (roomId, ctx) {
        var placeList = "";
        var shownPlaceName;
        for (var i = 1; i <= this._rooms[roomId].NumberPlaces; i++) {
            if (this._gameAslVersion >= 311 && this._rooms[roomId].Places[i].Script == "") {
                var placeId = this.GetRoomId(this._rooms[roomId].Places[i].PlaceName, ctx);
                if (placeId == 0) {
                    this.LogASLError("No such room '" + this._rooms[roomId].Places[i].PlaceName + "'", LogType.WarningError);
                    shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                }
                else {
                    if (this._rooms[placeId].RoomAlias != "") {
                        shownPlaceName = this._rooms[placeId].RoomAlias;
                    }
                    else {
                        shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                    }
                }
            }
            else {
                shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
            }
            var shownPrefix = this._rooms[roomId].Places[i].Prefix;
            if (shownPrefix != "") {
                shownPrefix = shownPrefix + " ";
            }
            placeList = placeList + shownPrefix + "|b" + shownPlaceName + "|xb, ";
        }
        return placeList;
    };
    LegacyGame.prototype.SetUpExits = function () {
        // Exits have to be set up after all the rooms have been initialised
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define room ")) {
                var roomName = this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext);
                var roomId = this.GetRoomId(roomName, this._nullContext);
                for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                    if (this.BeginsWith(this._lines[j], "define ")) {
                        //skip nested blocks
                        var nestedBlock = 1;
                        do {
                            j = j + 1;
                            if (this.BeginsWith(this._lines[j], "define ")) {
                                nestedBlock = nestedBlock + 1;
                            }
                            else if (Trim(this._lines[j]) == "end define") {
                                nestedBlock = nestedBlock - 1;
                            }
                        } while (!(nestedBlock == 0));
                    }
                    this._rooms[roomId].Exits.AddExitFromTag(this._lines[j]);
                }
            }
        }
        return;
    };
    LegacyGame.prototype.FindExit = function (tag) {
        // e.g. Takes a tag of the form "room; north" and return's the north exit of room.
        var params = Split(tag, ";");
        if (UBound(params) < 1) {
            this.LogASLError("No exit specified in '" + tag + "'", LogType.WarningError);
            return new RoomExit(this);
        }
        var room = Trim(params[0]);
        var exitName = Trim(params[1]);
        var roomId = this.GetRoomId(room, this._nullContext);
        if (roomId == 0) {
            this.LogASLError("Can't find room '" + room + "'", LogType.WarningError);
            return null;
        }
        var exits = this._rooms[roomId].Exits;
        var dir = exits.GetDirectionEnum(exitName);
        if (dir == Direction.None) {
            if (exits.GetPlaces()[exitName]) {
                return exits.GetPlaces()[exitName];
            }
        }
        else {
            return exits.GetDirectionExit(dir);
        }
        return null;
    };
    LegacyGame.prototype.ExecuteLock = function (tag, lock) {
        var roomExit;
        roomExit = this.FindExit(tag);
        if (roomExit == null) {
            this.LogASLError("Can't find exit '" + tag + "'", LogType.WarningError);
            return;
        }
        roomExit.SetIsLocked(lock);
    };
    LegacyGame.prototype.DoBegin = function () {
        var gameBlock = this.GetDefineBlock("game");
        var ctx = new Context();
        this.SetFont("");
        this._player.SetFontSize(this._defaultFontSize);
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "background ")) {
                this.SetBackground(this.GetParameter(this._lines[i], this._nullContext));
            }
        }
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "foreground ")) {
                this.SetForeground(this.GetParameter(this._lines[i], this._nullContext));
            }
        }
        // Execute any startscript command that appears in the
        // "define game" block:
        this._autoIntro = true;
        // For ASL>=391, we only run startscripts if LoadMethod is normal (i.e. we haven't started
        // from a saved QSG file)
        if (this._gameAslVersion < 391 || (this._gameAslVersion >= 391 && this._gameLoadMethod == "normal")) {
            // for GameASLVersion 311 and later, any library startscript is executed first:
            if (this._gameAslVersion >= 311) {
                // We go through the game block executing these in reverse order, as
                // the statements which are included last should be executed first.
                for (var i = gameBlock.EndLine - 1; i >= gameBlock.StartLine + 1; i--) {
                    if (this.BeginsWith(this._lines[i], "lib startscript ")) {
                        ctx = this._nullContext;
                        this.ExecuteScript(Trim(this.GetEverythingAfter(Trim(this._lines[i]), "lib startscript ")), ctx);
                    }
                }
            }
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "startscript ")) {
                    ctx = this._nullContext;
                    this.ExecuteScript(Trim(this.GetEverythingAfter(Trim(this._lines[i]), "startscript")), ctx);
                }
                else if (this.BeginsWith(this._lines[i], "lib startscript ") && this._gameAslVersion < 311) {
                    ctx = this._nullContext;
                    this.ExecuteScript(Trim(this.GetEverythingAfter(Trim(this._lines[i]), "lib startscript ")), ctx);
                }
            }
        }
        this._gameFullyLoaded = true;
        // Display intro text
        if (this._autoIntro && this._gameLoadMethod == "normal") {
            this.DisplayTextSection("intro", this._nullContext);
        }
        // Start game from room specified by "start" statement
        var startRoom = "";
        for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "start ")) {
                startRoom = this.GetParameter(this._lines[i], this._nullContext);
            }
        }
        if (!this._loadedFromQsg) {
            ctx = this._nullContext;
            this.PlayGame(startRoom, ctx);
            this.Print("", this._nullContext);
        }
        else {
            this.UpdateItems(this._nullContext);
            this.Print("Restored saved game", this._nullContext);
            this.Print("", this._nullContext);
            this.PlayGame(this._currentRoom, this._nullContext);
            this.Print("", this._nullContext);
            if (this._gameAslVersion >= 391) {
                // For ASL>=391, OnLoad is now run for all games.
                ctx = this._nullContext;
                this.ExecuteScript(this._onLoadScript, ctx);
            }
        }
        this.RaiseNextTimerTickRequest();
    };
    LegacyGame.prototype.Finish = function () {
        this.GameFinished();
    };
    LegacyGame.prototype.SendCommand = function (command, elapsedTime) {
        this.ExecCommand(command, new Context());
        if (elapsedTime > 0) {
            this.Tick(elapsedTime);
        }
        else {
            this.RaiseNextTimerTickRequest();
        }
    };
    LegacyGame.prototype.Initialise = function (onSuccess, onFailure) {
        if (LCase(Right(this._filename, 4)) == ".qsg" || this._data != null) {
            this.OpenGame(this._filename, onSuccess, onFailure);
        }
        else {
            this.InitialiseGame(this._filename, false, onSuccess, onFailure);
        }
    };
    LegacyGame.prototype.GameFinished = function () {
        this._gameFinished = true;
    };
    LegacyGame.prototype.GetResourcePath = function (filename) {
        if (!this._resourceFile == null && this._resourceFile.length > 0) {
            var extractResult = this.ExtractFile(filename);
            return extractResult;
        }
        // TODO
        //return System.IO.Path.Combine(this._gamePath, filename);
        return null;
    };
    LegacyGame.prototype.GetLibraryLines = function (libName) {
        var lib = libraries[libName.toLowerCase()];
        if (!lib)
            return null;
        return lib.split("\n");
    };
    LegacyGame.prototype.Tick = function (elapsedTime) {
        var i = 0;
        var timerScripts = [];
        for (var i = 1; i <= this._numberTimers; i++) {
            if (this._timers[i].TimerActive) {
                if (this._timers[i].BypassThisTurn) {
                    // don't trigger timer during the turn it was first enabled
                    this._timers[i].BypassThisTurn = false;
                }
                else {
                    this._timers[i].TimerTicks = this._timers[i].TimerTicks + elapsedTime;
                    if (this._timers[i].TimerTicks >= this._timers[i].TimerInterval) {
                        this._timers[i].TimerTicks = 0;
                        timerScripts.push(this._timers[i].TimerAction);
                    }
                }
            }
        }
        if (timerScripts.length > 0) {
            this.RunTimer(timerScripts);
        }
        this.RaiseNextTimerTickRequest();
    };
    LegacyGame.prototype.RunTimer = function (scripts) {
        scripts.forEach(function (script) {
            try {
                this.ExecuteScript(script, this._nullContext);
            }
            catch (e) {
            }
        }, this);
    };
    LegacyGame.prototype.RaiseNextTimerTickRequest = function () {
        var anyTimerActive = false;
        var nextTrigger = 60;
        for (var i = 1; i <= this._numberTimers; i++) {
            if (this._timers[i].TimerActive) {
                anyTimerActive = true;
                var thisNextTrigger = this._timers[i].TimerInterval - this._timers[i].TimerTicks;
                if (thisNextTrigger < nextTrigger) {
                    nextTrigger = thisNextTrigger;
                }
            }
        }
        if (!anyTimerActive) {
            nextTrigger = 0;
        }
        if (this._gameFinished) {
            nextTrigger = 0;
        }
        // TODO...
    };
    LegacyGame.prototype.GetOriginalFilenameForQSG = function () {
        if (this._originalFilename != null) {
            return this._originalFilename;
        }
        return this._gameFileName;
    };
    LegacyGame.prototype.GetFileData = function (filename, onSuccess, onFailure) {
        this._fileFetcher(filename, onSuccess, onFailure);
    };
    LegacyGame.prototype.GetCASFileData = function (filename) {
        // TODO
        return "";
    };
    LegacyGame.prototype.DoPrint = function (text) {
        var output = this._textFormatter.OutputHTML(text);
        quest.print(output.text, !output.nobr);
    };
    LegacyGame.prototype.DoWait = function () {
        // TODO
    };
    LegacyGame.prototype.Pause = function (duration) {
        // TODO
    };
    LegacyGame.prototype.ExecuteIfAsk = function (question) {
        // TODO
        return true;
    };
    LegacyGame.prototype.UpdateExitsList = function () {
        // The Quest 5.0 Player takes a combined list of compass and "go to" exits, whereas the
        // ASL4 code produces these separately. So we keep track of them separately and then
        // merge to send to the Player.
        var mergedList = this._compassExits.concat(this._gotoExits);
        // TODO: RaiseEvent UpdateList(ListType.ExitsList, mergedList)
    };
    LegacyGame.prototype.Begin = function () {
        this.DoBegin();
    };
    LegacyGame.prototype.ShowMenu = function (menuData) {
        // TODO
        return null;
    };
    LegacyGame.prototype.DecryptString = function (s) {
        // TODO
        return null;
    };
    return LegacyGame;
})();
var ChangeLog = (function () {
    function ChangeLog() {
        this.Changes = {};
    }
    // appliesTo = room or object name
    // element = the thing that's changed, e.g. an action or property name
    // changeData = the actual change info
    ChangeLog.prototype.AddItem = function (appliesTo, element, changeData) {
        // the first four characters of the changeData will be "prop" or "acti", so we add this to the
        // key so that actions and properties don't collide.
        var key = appliesTo + "#" + Left(changeData, 4) + "~" + element;
        this.Changes[key] = changeData;
    };
    return ChangeLog;
})();
var RoomExit = (function () {
    function RoomExit(game) {
        this._objId = 0;
        this._roomId = 0;
        this._game = game;
        game._numberObjs = game._numberObjs + 1;
        if (!game._objs)
            game._objs = [];
        game._objs[game._numberObjs] = new ObjectType();
        this._objId = game._numberObjs;
        var o = game._objs[this._objId];
        o.IsExit = true;
        o.Visible = true;
        o.Exists = true;
    }
    RoomExit.prototype.SetExitProperty = function (propertyName, value) {
        this._game.AddToObjectProperties(propertyName + "=" + value, this._objId, this._game._nullContext);
    };
    RoomExit.prototype.GetExitProperty = function (propertyName) {
        return this._game.GetObjectProperty(propertyName, this._objId, false, false);
    };
    RoomExit.prototype.SetExitPropertyBool = function (propertyName, value) {
        var sPropertyString;
        sPropertyString = propertyName;
        if (!value) {
            sPropertyString = "not " + sPropertyString;
        }
        this._game.AddToObjectProperties(sPropertyString, this._objId, this._game._nullContext);
    };
    RoomExit.prototype.GetExitPropertyBool = function (propertyName) {
        return (this._game.GetObjectProperty(propertyName, this._objId, true, false) == "yes");
    };
    RoomExit.prototype.SetAction = function (actionName, value) {
        this._game.AddToObjectActions("<" + actionName + "> " + value, this._objId, this._game._nullContext);
    };
    RoomExit.prototype.SetToRoom = function (value) {
        this.SetExitProperty("to", value);
        this.UpdateObjectName();
    };
    RoomExit.prototype.GetToRoom = function () {
        return this.GetExitProperty("to");
    };
    RoomExit.prototype.SetPrefix = function (value) {
        this.SetExitProperty("prefix", value);
    };
    RoomExit.prototype.GetPrefix = function () {
        return this.GetExitProperty("prefix");
    };
    RoomExit.prototype.SetScript = function (value) {
        if (Len(value) > 0) {
            this.SetAction("script", value);
        }
    };
    RoomExit.prototype.IsScript = function () {
        return this._game.HasAction(this._objId, "script");
    };
    RoomExit.prototype.SetDirection = function (value) {
        this._direction = value;
        if (value != Direction.None) {
            this.UpdateObjectName();
        }
    };
    RoomExit.prototype.GetDirection = function () {
        return this._direction;
    };
    RoomExit.prototype.SetParent = function (value) {
        this._parent = value;
    };
    RoomExit.prototype.GetParent = function () {
        return this._parent;
    };
    RoomExit.prototype.GetObjId = function () {
        return this._objId;
    };
    RoomExit.prototype.GetRoomId = function () {
        if (this._roomId == 0) {
            this._roomId = this._game.GetRoomId(this.GetToRoom(), this._game._nullContext);
        }
        return this._roomId;
    };
    RoomExit.prototype.GetDisplayName = function () {
        return this._displayName;
    };
    RoomExit.prototype.GetDisplayText = function () {
        return this._displayName;
    };
    RoomExit.prototype.SetIsLocked = function (value) {
        this.SetExitPropertyBool("locked", value);
    };
    RoomExit.prototype.GetIsLocked = function () {
        return this.GetExitPropertyBool("locked");
    };
    RoomExit.prototype.SetLockMessage = function (value) {
        this.SetExitProperty("lockmessage", value);
    };
    RoomExit.prototype.GetLockMessage = function () {
        return this.GetExitProperty("lockmessage");
    };
    RoomExit.prototype.RunAction = function (actionName, ctx) {
        this._game.DoAction(this._objId, actionName, ctx);
    };
    RoomExit.prototype.RunScript = function (ctx) {
        this.RunAction("script", ctx);
    };
    RoomExit.prototype.UpdateObjectName = function () {
        var objName;
        var lastExitId = 0;
        var parentRoom;
        if (Len(this._objName) > 0) {
            return;
        }
        if (this._parent == null) {
            return;
        }
        parentRoom = this._game._objs[this._parent.GetObjId()].ObjectName;
        objName = parentRoom;
        if (this._direction != Direction.None) {
            objName = objName + "." + this._parent.GetDirectionName(this._direction);
            this._game._objs[this._objId].ObjectAlias = this._parent.GetDirectionName(this._direction);
        }
        else {
            var lastExitIdString = this._game.GetObjectProperty("quest.lastexitid", (this._parent.GetObjId()), null, false);
            if (lastExitIdString.length == 0) {
                lastExitId = 0;
            }
            lastExitId = lastExitId + 1;
            this._game.AddToObjectProperties("quest.lastexitid=" + (lastExitId).toString(), (this._parent.GetObjId()), this._game._nullContext);
            objName = objName + ".exit" + (lastExitId).toString();
            if (this.GetRoomId() == 0) {
                // the room we're pointing at might not exist, especially if this is a script exit
                this._displayName = this.GetToRoom();
            }
            else {
                if (Len(this._game._rooms[this.GetRoomId()].RoomAlias) > 0) {
                    this._displayName = this._game._rooms[this.GetRoomId()].RoomAlias;
                }
                else {
                    this._displayName = this.GetToRoom();
                }
            }
            this._game._objs[this._objId].ObjectAlias = this._displayName;
            this.SetPrefix(this._game._rooms[this.GetRoomId()].Prefix);
        }
        this._game._objs[this._objId].ObjectName = objName;
        this._game._objs[this._objId].ContainerRoom = parentRoom;
        this._objName = objName;
    };
    RoomExit.prototype.Go = function (ctx) {
        if (this.GetIsLocked()) {
            if (this.GetExitPropertyBool("lockmessage")) {
                this._game.Print(this.GetExitProperty("lockmessage"), ctx);
            }
            else {
                this._game.PlayerErrorMessage(PlayerError.Locked, ctx);
            }
        }
        else {
            if (this.IsScript()) {
                this.RunScript(ctx);
            }
            else {
                this._game.PlayGame(this.GetToRoom(), ctx);
            }
        }
    };
    return RoomExit;
})();
var RoomExits = (function () {
    function RoomExits(game) {
        this._directions = {};
        this._places = {};
        this._objId = 0;
        this._game = game;
        this._regenerateAllExits = true;
    }
    RoomExits.prototype.SetDirection = function (direction) {
        var roomExit;
        if (this._directions[direction]) {
            roomExit = this._directions[direction];
            this._game._objs[roomExit.GetObjId()].Exists = true;
        }
        else {
            roomExit = new RoomExit(this._game);
            this._directions[direction] = roomExit;
        }
        this._regenerateAllExits = true;
        return roomExit;
    };
    RoomExits.prototype.GetDirectionExit = function (direction) {
        if (this._directions[direction]) {
            return this._directions[direction];
        }
        return null;
    };
    RoomExits.prototype.AddPlaceExit = function (roomExit) {
        this._places[roomExit.GetToRoom()] = roomExit;
        this._regenerateAllExits = true;
    };
    RoomExits.prototype.AddExitFromTag = function (tag) {
        var thisDir;
        var roomExit = null;
        var params = [];
        var afterParam;
        var param = false;
        if (this._game.BeginsWith(tag, "out ")) {
            tag = this._game.GetEverythingAfter(tag, "out ");
            thisDir = Direction.Out;
        }
        else if (this._game.BeginsWith(tag, "east ")) {
            tag = this._game.GetEverythingAfter(tag, "east ");
            thisDir = Direction.East;
        }
        else if (this._game.BeginsWith(tag, "west ")) {
            tag = this._game.GetEverythingAfter(tag, "west ");
            thisDir = Direction.West;
        }
        else if (this._game.BeginsWith(tag, "north ")) {
            tag = this._game.GetEverythingAfter(tag, "north ");
            thisDir = Direction.North;
        }
        else if (this._game.BeginsWith(tag, "south ")) {
            tag = this._game.GetEverythingAfter(tag, "south ");
            thisDir = Direction.South;
        }
        else if (this._game.BeginsWith(tag, "northeast ")) {
            tag = this._game.GetEverythingAfter(tag, "northeast ");
            thisDir = Direction.NorthEast;
        }
        else if (this._game.BeginsWith(tag, "northwest ")) {
            tag = this._game.GetEverythingAfter(tag, "northwest ");
            thisDir = Direction.NorthWest;
        }
        else if (this._game.BeginsWith(tag, "southeast ")) {
            tag = this._game.GetEverythingAfter(tag, "southeast ");
            thisDir = Direction.SouthEast;
        }
        else if (this._game.BeginsWith(tag, "southwest ")) {
            tag = this._game.GetEverythingAfter(tag, "southwest ");
            thisDir = Direction.SouthWest;
        }
        else if (this._game.BeginsWith(tag, "up ")) {
            tag = this._game.GetEverythingAfter(tag, "up ");
            thisDir = Direction.Up;
        }
        else if (this._game.BeginsWith(tag, "down ")) {
            tag = this._game.GetEverythingAfter(tag, "down ");
            thisDir = Direction.Down;
        }
        else if (this._game.BeginsWith(tag, "place ")) {
            tag = this._game.GetEverythingAfter(tag, "place ");
            thisDir = Direction.None;
        }
        else {
            return;
        }
        if (thisDir != Direction.None) {
            // This will reuse an existing Exit object if we're resetting
            // the destination of an existing directional exit.
            roomExit = this.SetDirection(thisDir);
        }
        else {
            roomExit = new RoomExit(this._game);
        }
        roomExit.SetParent(this);
        roomExit.SetDirection(thisDir);
        if (this._game.BeginsWith(tag, "locked ")) {
            roomExit.SetIsLocked(true);
            tag = this._game.GetEverythingAfter(tag, "locked ");
        }
        if (Left(Trim(tag), 1) == "<") {
            params = Split(this._game.GetParameter(tag, this._game._nullContext), ";");
            afterParam = Trim(Mid(tag, InStr(tag, ">") + 1));
            param = true;
        }
        else {
            afterParam = tag;
        }
        if (Len(afterParam) > 0) {
            // Script exit
            roomExit.SetScript(afterParam);
            if (thisDir == Direction.None) {
                // A place exit with a script still has a ToRoom
                roomExit.SetToRoom(params[0]);
                // and may have a lock message
                if (UBound(params) > 0) {
                    roomExit.SetLockMessage(params[1]);
                }
            }
            else {
                // A directional exit with a script may have no parameter.
                // If it does have a parameter it will be a lock message.
                if (param) {
                    roomExit.SetLockMessage(params[0]);
                }
            }
        }
        else {
            roomExit.SetToRoom(params[0]);
            if (UBound(params) > 0) {
                roomExit.SetLockMessage(params[1]);
            }
        }
        if (thisDir == Direction.None) {
            this.AddPlaceExit(roomExit);
        }
    };
    RoomExits.prototype.AddExitFromCreateScript = function (script, ctx) {
        // sScript is the "create exit ..." script, but without the "create exit" at the beginning.
        // So it's very similar to creating an exit from a tag, except we have the source room
        // name before the semicolon (which we don't even care about as we ARE the source room).
        var param;
        var params;
        var paramStart = 0;
        var paramEnd = 0;
        // Just need to convert e.g.
        //   create exit <src_room; dest_room> { script }
        // to
        //   place <dest_room> { script }
        // And
        //   create exit north <src_room> { script }
        // to
        //   north { script }
        // And
        //   create exit north <src_room; dest_room>
        // to
        //   north <dest_room>
        param = this._game.GetParameter(script, ctx);
        params = Split(param, ";");
        paramStart = InStr(script, "<");
        paramEnd = InStrFrom(paramStart, script, ">");
        if (paramStart > 1) {
            // Directional exit
            if (UBound(params) == 0) {
                // Script directional exit
                this.AddExitFromTag(Trim(Left(script, paramStart - 1)) + " " + Trim(Mid(script, paramEnd + 1)));
            }
            else {
                // "Normal" directional exit
                this.AddExitFromTag(Trim(Left(script, paramStart - 1)) + " <" + Trim(params[1]) + ">");
            }
        }
        else {
            if (UBound(params) < 1) {
                this._game.LogASLError("No exit destination given in 'create exit " + script + "'", LogType.WarningError);
                return;
            }
            // Place exit so add "place" tag at the beginning
            this.AddExitFromTag("place <" + Trim(params[1]) + Mid(script, paramEnd));
        }
    };
    RoomExits.prototype.SetObjId = function (value) {
        this._objId = value;
    };
    RoomExits.prototype.GetObjId = function () {
        return this._objId;
    };
    RoomExits.prototype.GetPlaces = function () {
        return this._places;
    };
    RoomExits.prototype.ExecuteGo = function (cmd, ctx) {
        // This will handle "n", "go east", "go [to] library" etc.
        var exitId = 0;
        var roomExit;
        if (this._game.BeginsWith(cmd, "go to ")) {
            cmd = this._game.GetEverythingAfter(cmd, "go to ");
        }
        else if (this._game.BeginsWith(cmd, "go ")) {
            cmd = this._game.GetEverythingAfter(cmd, "go ");
        }
        exitId = this._game.Disambiguate(cmd, this._game._currentRoom, ctx, true);
        if (exitId == -1) {
            this._game.PlayerErrorMessage(PlayerError.BadPlace, ctx);
        }
        else {
            roomExit = this.GetExitByObjectId(exitId);
            roomExit.Go(ctx);
        }
    };
    RoomExits.prototype.GetAvailableDirectionsDescription = function () {
        var roomExit;
        var count = 0;
        var descPrefix;
        var orString;
        descPrefix = "You can go";
        orString = "or";
        var list = "";
        var description = "";
        count = 0;
        this.AllExits().forEach(function (roomExit) {
            count = count + 1;
            list = list + this.GetDirectionToken(roomExit.GetDirection());
            description = description + this.GetDirectionNameDisplay(roomExit);
            if (count < this.AllExits.Count - 1) {
                description = description + ", ";
            }
            else if (count == this.AllExits.Count - 1) {
                description = description + " " + orString + " ";
            }
        }, this);
        this._game.SetStringContents("quest.doorways", description, this._game._nullContext);
        if (count > 0) {
            description = descPrefix + " " + description + ".";
        }
        var result = new GetAvailableDirectionsResult();
        result.Description = description;
        result.List = list;
        return result;
    };
    RoomExits.prototype.GetDirectionName = function (dir) {
        switch (dir) {
            case Direction.Out:
                return "out";
            case Direction.North:
                return "north";
            case Direction.South:
                return "south";
            case Direction.East:
                return "east";
            case Direction.West:
                return "west";
            case Direction.NorthWest:
                return "northwest";
            case Direction.NorthEast:
                return "northeast";
            case Direction.SouthWest:
                return "southwest";
            case Direction.SouthEast:
                return "southeast";
            case Direction.Up:
                return "up";
            case Direction.Down:
                return "down";
        }
        return null;
    };
    RoomExits.prototype.GetDirectionEnum = function (dir) {
        switch (dir) {
            case "out":
                return Direction.Out;
            case "north":
                return Direction.North;
            case "south":
                return Direction.South;
            case "east":
                return Direction.East;
            case "west":
                return Direction.West;
            case "northwest":
                return Direction.NorthWest;
            case "northeast":
                return Direction.NorthEast;
            case "southwest":
                return Direction.SouthWest;
            case "southeast":
                return Direction.SouthEast;
            case "up":
                return Direction.Up;
            case "down":
                return Direction.Down;
        }
        return Direction.None;
    };
    RoomExits.prototype.GetDirectionToken = function (dir) {
        switch (dir) {
            case Direction.Out:
                return "o";
            case Direction.North:
                return "n";
            case Direction.South:
                return "s";
            case Direction.East:
                return "e";
            case Direction.West:
                return "w";
            case Direction.NorthWest:
                return "b";
            case Direction.NorthEast:
                return "a";
            case Direction.SouthWest:
                return "d";
            case Direction.SouthEast:
                return "c";
            case Direction.Up:
                return "u";
            case Direction.Down:
                return "f";
        }
        return null;
    };
    RoomExits.prototype.GetDirectionNameDisplay = function (roomExit) {
        if (roomExit.GetDirection() != Direction.None) {
            var dir = this.GetDirectionName(roomExit.GetDirection());
            return "|b" + dir + "|xb";
        }
        var display = "|b" + roomExit.GetDisplayName() + "|xb";
        if (Len(roomExit.GetPrefix()) > 0) {
            display = roomExit.GetPrefix() + " " + display;
        }
        return "to " + display;
    };
    RoomExits.prototype.GetExitByObjectId = function (id) {
        var result;
        this.AllExits().forEach(function (roomExit) {
            if (roomExit.GetObjId() == id) {
                result = roomExit;
                return;
            }
        }, this);
        return result;
    };
    RoomExits.prototype.AllExits = function () {
        if (!this._regenerateAllExits) {
            return this._allExits;
        }
        this._allExits = [];
        for (var dir in this._directions) {
            var roomExit = this._directions[dir];
            if (this._game._objs[roomExit.GetObjId()].Exists) {
                this._allExits.push(this._directions[dir]);
            }
        }
        ;
        for (var dir in this._places) {
            var roomExit = this._places[dir];
            if (this._game._objs[roomExit.GetObjId()].Exists) {
                this._allExits.push(this._places[dir]);
            }
        }
        ;
        return this._allExits;
    };
    RoomExits.prototype.RemoveExit = function (roomExit) {
        // Don't remove directional exits, as if they're recreated
        // a new object will be created which will have the same name
        // as the old one. This is because we can't delete objects yet...
        if (roomExit.GetDirection() == Direction.None) {
            if (this._places[roomExit.GetToRoom()]) {
                delete this._places[roomExit.GetToRoom()];
            }
        }
        this._game._objs[roomExit.GetObjId()].Exists = false;
        this._regenerateAllExits = true;
    };
    return RoomExits;
})();
var TextFormatterResult = (function () {
    function TextFormatterResult() {
    }
    return TextFormatterResult;
})();
var TextFormatter = (function () {
    function TextFormatter() {
        this.colour = "";
        this.fontSize = 0;
        this.align = "";
        this.fontFamily = "";
        this.foreground = "";
    }
    TextFormatter.prototype.OutputHTML = function (input) {
        var output = "";
        var position = 0;
        var codePosition = 0;
        var finished = false;
        var nobr = false;
        input = input.replace(/&/g, "&amp;").replace(/\</g, "&lt;").replace(/\>/g, "&gt;").replace(/\n/g, "<br />");
        if (Right(input, 3) == "|xn") {
            nobr = true;
            input = Left(input, Len(input) - 3);
        }
        do {
            codePosition = input.indexOf("|", position);
            if (codePosition == -1) {
                output += this.FormatText(input.substr(position));
                finished = true;
            }
            else {
                output += this.FormatText(input.substr(position, codePosition - position));
                position = codePosition + 1;
                var oneCharCode = "";
                var twoCharCode = "";
                if (position < input.length) {
                    oneCharCode = input.substr(position, 1);
                }
                if (position < (input.length - 1)) {
                    twoCharCode = input.substr(position, 2);
                }
                var foundCode = true;
                switch (twoCharCode) {
                    case "xb":
                        this.bold = false;
                        break;
                    case "xi":
                        this.italic = false;
                        break;
                    case "xu":
                        this.underline = false;
                        break;
                    case "cb":
                        this.colour = "";
                        break;
                    case "cr":
                        this.colour = "red";
                        break;
                    case "cl":
                        this.colour = "blue";
                        break;
                    case "cy":
                        this.colour = "yellow";
                        break;
                    case "cg":
                        this.colour = "green";
                        break;
                    case "jl":
                        this.align = "";
                        break;
                    case "jc":
                        this.align = "center";
                        break;
                    case "jr":
                        this.align = "right";
                        break;
                    default:
                        foundCode = false;
                        break;
                }
                if (foundCode) {
                    position += 2;
                }
                else {
                    foundCode = true;
                    switch (oneCharCode) {
                        case "b":
                            this.bold = true;
                            break;
                        case "i":
                            this.italic = true;
                            break;
                        case "u":
                            this.underline = true;
                            break;
                        case "n":
                            output += "<br />";
                            break;
                        default:
                            foundCode = false;
                            break;
                    }
                    if (foundCode) {
                        position += 1;
                    }
                }
                if (!foundCode) {
                    if (oneCharCode == "s") {
                        // |s00 |s10 etc.
                        if (position < (input.length - 2)) {
                            var sizeCode = input.substr(position + 1, 2);
                            var fontSize = parseInt(sizeCode);
                            if (!isNaN(fontSize)) {
                                this.fontSize = fontSize;
                                foundCode = true;
                                position += 3;
                            }
                        }
                    }
                }
                if (!foundCode) {
                    output += "|";
                }
            }
        } while (!(finished || position >= input.length));
        var result = new TextFormatterResult();
        result.text = output;
        result.nobr = nobr;
        return result;
    };
    TextFormatter.prototype.FormatText = function (input) {
        if (input.length == 0) {
            return input;
        }
        var output = "";
        if (this.align.length > 0) {
            output += '<div style="text-align:' + this.align + '">';
        }
        output += '<span style="font-family:' + this.fontFamily + ';font-size:' + (this.fontSize).toString() + 'pt">';
        if (this.colour.length > 0) {
            output += '<span style="color:' + this.colour + '">';
        }
        if (this.bold) {
            output += "<b>";
        }
        if (this.italic) {
            output += "<i>";
        }
        if (this.underline) {
            output += "<u>";
        }
        output += input;
        if (this.underline) {
            output += "</u>";
        }
        if (this.italic) {
            output += "</i>";
        }
        if (this.bold) {
            output += "</b>";
        }
        if (this.colour.length > 0) {
            output += "</span>";
        }
        output += "</span>";
        if (this.align.length > 0) {
            output += "</div>";
        }
        return output;
    };
    return TextFormatter;
})();
var libraries = {};
libraries["stdverbs.lib"] = "!library\n!asl-version <400>\n!name <Additional verbs>\n!version <1.0>\n!author <Alex Warren>\n! This library adds default responses for a number of common commands that players might use in a game. It also allows players to type multiple commands on the same command line.\n\n' STDVERBS.LIB v1.0\n' for Quest 4.0\n' Copyright \u00A9 2007 Axe Software. Please do not modify this library.\n\n!addto game\n\tverb <buy> msg <You can't buy #(quest.lastobject):article#.>\n\tverb <climb> msg <You can't climb #(quest.lastobject):article#.>\n\tverb <drink> msg <You can't drink #(quest.lastobject):article#.>\n\tverb <eat> msg <You can't eat #(quest.lastobject):article#.>\n\tverb <hit> msg <You can't hit #(quest.lastobject):article#.>\n\tverb <kill> msg <You can't kill #(quest.lastobject):article#.>\n\tverb <kiss> msg <I don't think #(quest.lastobject):article# would like that.>\n\tverb <knock> msg <You can't knock #(quest.lastobject):article#.>\n\tverb <lick> msg <You can't lick #(quest.lastobject):article#.>\n\tverb <listen to> msg <You listen, but #(quest.lastobject):article# makes no sound.>\n\tverb <lock> msg <You can't lock #(quest.lastobject):article#.>\n\tverb <move> msg <You can't move #(quest.lastobject):article#.>\n\tverb <pull> msg <You can't pull #(quest.lastobject):article#.>\n\tverb <push> msg <You can't push #(quest.lastobject):article#.>\n\tverb <read> msg <You can't read #(quest.lastobject):article#.>\n\tverb <search> msg <You can't search #(quest.lastobject):article#.>\n\tverb <show> msg <You can't show #(quest.lastobject):article#.>\n\tverb <sit on> msg <You can't sit on #(quest.lastobject):article#.>\n\tverb <smell; sniff> msg <You sniff, but #(quest.lastobject):article# doesn't smell of much.>\n\tverb <taste> msg <You can't taste #(quest.lastobject):article#.>\n\tverb <throw> msg <You can't throw #(quest.lastobject):article#.>\n\tverb <tie> msg <You can't tie #(quest.lastobject):article#.>\n\tverb <touch> msg <You can't touch #(quest.lastobject):article#.>\n\tverb <turn on> msg <You can't turn #(quest.lastobject):article# on.>\n\tverb <turn off> msg <You can't turn #(quest.lastobject):article# off.>\n\tverb <turn> msg <You can't turn #(quest.lastobject):article#.>\n\tverb <unlock> msg <You can't unlock #(quest.lastobject):article#.>\n\tverb <untie> msg <You can't untie #(quest.lastobject):article#.>\n\tverb <wear> msg <You can't wear #(quest.lastobject):article#.>\n\t\n\tcommand <ask #stdverbs.command#> msg <You get no reply.>\n\tcommand <listen> msg <You can't hear much.>\n\tcommand <jump> msg <You jump, but nothing happens.>\n\tcommand <tell #stdverbs.command#> msg <You get no reply.>\n\tcommand <sit down; sleep> msg <No time for lounging about now.>\n\tcommand <wait> msg <Time passes.>\n\tcommand <xyzzy> msg <Surprisingly, absolutely nothing happens.>\n\t\n\tcommand <#stdverbs.command#. #stdverbs.command2#; _\n\t\t#stdverbs.command#, then #stdverbs.command2#; _\n\t\t#stdverbs.command#, and then #stdverbs.command2#; _\n\t\t#stdverbs.command#, #stdverbs.command2#; _\n\t\t#stdverbs.command# and then #stdverbs.command2#; _\n\t\t#stdverbs.command# then #stdverbs.command2#> {\n\t\texec <#stdverbs.command#>\n\t\texec <#stdverbs.command2#>\n\t}\n\t\n\tcommand <#stdverbs.command#.> exec <#stdverbs.command#>\n!end";
libraries["standard.lib"] = "!library\n\n'----------------------------------------------------------------------\n\n'Filename:\tstandard.lib\n\n'Version:\t\tbeta 3a ( minor bug fix of beta 3 )\n\n'Type:\t\tASL Library ( for Quest 2.0/2.1 only )\n\n'By:\t\t\tA.G. Bampton (originally 10/08/1999)   \n\n'Revision:\t21/12/1999\n\n'Purpose/ \tSee STDLIB.RTF for details of what this library does\n'Usage:\t\tand how to use it.\n'\n'WARNING:\tI STRONGLY ADVISE YOU NEVER CHANGE ANY CODE IN THIS\n'\t    \tLIBRARY! It's complete and functional 'as is' and if\n'\t    \tit has been altered I will not be able to offer any\n'\t    \tsupport for it's use. Tailor/modify the way it works\n'\t    \tin external code - either within your 'ASL' game code\n'\t    \tor, IF YOU ARE USING QUEST PRO AND CAN COMPILE YOUR\n'\t    \tGAME TO A 'CAS' FILE ONLY, by including a customising\n'\t    \tlibrary as per 'library_demo.asl' my standard library\n'\t    \tdemo. \n'----------------------------------------------------------------------\n\n!asl-version <200>\n\n!addto game\n\tcommand <look at #object#> do <Look_Proc>\n\tcommand <x #object#> do <Look_Proc>\n\tcommand <drop #object# in #container#> do <Put_In_Container_Proc>\n\tcommand <drop #object# down> do <Drop_Proc>\n\tcommand <drop #object#> do <Drop_Proc>\n\tcommand <give #give_com_string#> do <Alternate_Give_Proc>\n\tcommand <take #object# from #character#> do <Take_Back_Proc>\n\tcommand <take #character#'s #object#> do <Take_Back_Proc>\n\tcommand <take #character#s #object#> do <Take_Back_Proc>\n\tcommand <take #character#' #object#> do <Take_Back_Proc>\n\tcommand <take #object#> do <Special_Take_Proc>\n\tcommand <customdrop #object# in #container#> do <Put_In_Container_Proc>\n\tcommand <customdrop #object#> do <Drop_Proc>\n\tcommand <customtake #object# from #character#> do <Take_Back_Proc>\n\tcommand <customtake #object#> do <Special_Take_Proc>\n!end\n\n\n!addto synonyms\n\texamine; inspect = look at\n\tdrop the; put the; put down the; put down; put = drop\n\tgive back the; give back; give the = give\n\tget the; get; take back the; take back; take the = take\n\tout of the; out of; back from the; back from; from the = from\n\tin to; in to the; into the; into; inside the; inside = in\n\tback to the; back to; to the = to \n!end\n\n'-------------------------------------------------------------------\n\n'The rest of this library is appended to the calling ASL file\n\n\ndefine procedure <Alternate_Give_Proc>\n\tsetvar <found_to;0>\n\tfor <to_true;1;$lengthof(#give_com_string#)$> do <to_test>\n\t\tif not is <%found_to%;0> then {\n\t\texec <give #give_com_string#;normal>\n\t\tsetvar <found_to;0>\n\t\t}\n\t\telse {\n\t\tfor <space_true;1;$lengthof(#give_com_string#)$> do <space_test>\n\t\tsetstring <give_char;$left(#give_com_string#;%found_space%)$>\n\t\tsetstring <give_obj;$mid(#give_com_string#;%found_space%)$>\n\t\texec <give #give_obj# to #give_char#;normal>\n\t\t}\nend define\n\t\n\ndefine procedure <to_test>\n\tsetstring <to_test_for;$mid(o to c;2;4)$>\n\tsetstring <test_part;$mid(#give_com_string#;%to_true%;4)$>\n\tif is <#to_test_for#;#test_part#> then setvar <found_to;%to_true%>\nend define\n\t\n\ndefine procedure <space_test>\n\tsetstring <to_test_for;$mid(c o;2;1)$>\n\tsetstring <test_part;$mid(#give_com_string#;%space_true%;1)$>\n\tif is <#to_test_for#;#test_part#> then setvar <found_space;%space_true%>\nend define\n\n\ndefine procedure <Look_Proc>\n\nif not is <#standard.lib.version#;> then {\n\n\tif is <$instr(#standard.lib.characters#;#object#)$;0> then {\n\tsetstring <where_it_is;$locationof(#object#)$>\n\n\t\tif not is <$instr(#standard.lib.containers#;#where_it_is#)$;0> then {\n\n\t\t\tif not is <$instr(#standard.lib.characters#;#where_it_is#)$;0> then {\n\n\t\t\t\tif here <#where_it_is#> then {\n\t\t\t\tmoveobject <#object#;#quest.currentroom#>\n\t\t\t\tshowobject <#object#>\n\t\t\t\texec <look at #object#;normal>\n\t\t\t\tmoveobject <#object#;#where_it_is#>\n\t\t\t\t}\n\t\t\t\telse {\n\t\t\t\texec <look at #object#;normal>\n\t\t\t\t}\n\n\t\t\t}\n\t\t\telse {\n\n\t\t\t\tif here <#where_it_is#> or got <#where_it_is#> then {\n\t\t\t\tmoveobject <#object#;#quest.currentroom#>\n\t\t\t\tshowobject <#object#>\n\t\t\t\texec <look at #object#;normal>\n\t\t\t\tmoveobject <#object#;#where_it_is#>\n\t\t\t\t}\n\t\t\t\telse {\n\t\t\t\texec <look at #object#;normal>\n\t\t\t\t}\n\n\t\t\t}\n\n\t\t}\n\t\telse {\n\n\t\t\tif got <#object#> then {\n\t\t\tmoveobject <#object#;#quest.currentroom#>\n\t\t\tshowobject <#object#>\n\t\t\texec <look at #object#;normal>\n\t\t\tdo <Check_Contents_Proc>\n\t\t\thideobject <#object#>\n\t\t\tmoveobject <#object#;#where_it_is#>\n\t\t\t}\n\t\t\telse {\n\t\t\texec <look at #object#;normal>\n\n\t\t\t\tif here <#object#> then {\n\t\t\t\tdo <Check_Contents_Proc>\n\t\t\t\t}\n\n\t\t\t}\t\n\n\t\t}\n\n\t}\n\telse {\n\texec <look at #object#;normal>\n\n\t\tif here <#object#> then {\n\t\tdo <Check_Contents_Proc>\n\t\t}\n\t}\n\n}\nelse {\ndo <Old_Look_Proc>\n}\n\nend define\n\ndefine procedure <Check_Contents_Proc>\n\n    \tif not is <$instr(#standard.lib.containers#;#object#)$;0> then {\n\t    setstring <where_we_were;#quest.currentroom#>\n    \toutputoff\n\t    goto <#object#>\n\t\t    if is <$lengthof(#quest.objects#)$;0> then {\n    \t\tgoto <#where_we_were#>\n\t    \toutputon\n\t\t    }\n    \t\telse {\n\t    \tsetstring <parsed_list;$gettag(#object#;prefix)$ __\n\t\t    $parse_object_list$>\n    \t\tgoto <#where_we_were#>\n\t    \toutputon\n\t\t    msg <#parsed_list#>\n\t\t    }\n    \t}\nend define\n\ndefine function <parse_object_list>\n\tsetvar <found_comma;0>\n\tfor <last_comma;1;$lengthof(#quest.formatobjects#)$> {\n\tdo <Last_Comma_Proc>\n\t}\n\t\tif not is <%found_comma%;0> then { \n\t\tsetvar <remaining;%last_comma%-%found_comma%>\n\t\tsetvar <remaining;%remaining%-1>\n\t\tsetvar <found_comma;%found_comma%-1>\n\t\tsetstring <left_part;$left(#quest.formatobjects#;__\n\t\t%found_comma%)$>\n\t\tsetstring <right_part;$right(#quest.formatobjects#;__\n\t\t%remaining%)$>\n\t\tsetstring <parsed_object_list;#left_part# and #right_part#.>\n\t\t}\n\t\tif is <%found_comma%;0> then {\n\t\tsetstring <parsed_object_list;#quest.formatobjects#.>\n\t\t}\n\t\treturn <#parsed_object_list#>\nend define\n\ndefine procedure <Last_Comma_Proc>\n\tsetstring <test_part;$mid(#quest.formatobjects#;%last_comma%;1)$>\n\tif is <#test_part#;,> then setvar <found_comma;%last_comma%>\nend define\n\n\ndefine procedure <Drop_Proc>\n\n\tif is <$instr(#standard.lib.characters#;#object#)$;0> then {\n\tdo <override_permitted>  \n\tif is <#override#;yes> then {\n\n\t\tif got <#object#> then {\n\t\tlose <#object#>\n\t\tmoveobject <#object#;#quest.currentroom#>\n\t\tshowobject <#object#>\n\t\tsetvar <custom_message; $lengthof(#std.lib.message.drop#)$>\n\t\t\tif is <%custom_message%; 0> then msg <You drop the #object#.>\n\t\t\telse msg <#std.lib.message.drop#.>\n\t\t}\n\t\telse {\n\t\tsetvar <custom_message; $lengthof(#std.lib.message.notcarried#)$>\n\t\t\tif is <%custom_message%; 0> then msg <You're not holding the #object#.>\n\t\t\telse msg <#std.lib.message.notcarried#.>\n\t\t}\n\n\t}\n\t}\n\telse msg <Sorry, I don't understand '#quest.originalcommand#'.>\nend define\n\n\ndefine procedure <Special_Take_Proc>\n\nif not is <#standard.lib.version#;> then {\n\n\tif is <$instr(#standard.lib.characters#;#object#)$;0> then {\n\tdo <override_permitted>  \n\t\tif is <#override#;yes> then {\n\t\tsetstring <where_it_is;$locationof(#object#)$>\n\t\t\tif not is <$instr(#standard.lib.containers#;#where_it_is#)$;0> then {\n\t\t\t\tif not is <$instr(#standard.lib.characters#;#where_it_is#)$;0> then {\n\t\t\t\t\tif here <#where_it_is#> then {\n\t\t\t\t\texec <take #object# from #where_it_is#>\n\t\t\t\t\t}\n\t\t\t\t\telse exec <take #object#;normal>\n\t\t\t\t}\n\t\t\t\telse {\n\t\t\t\t\tif here <#where_it_is#> or got <#where_it_is#> then {\n\t\t\t\t\texec <take #object# from #where_it_is#>\n\t\t\t\t\t}\n\t\t\t\t\telse exec <take #object#;normal>\n\t\t\t\t}\n\t\t\t}\n\t\t\telse {\n\t\t\t\tif got <#object#> then {\n\t\t\t\tsetvar <custom_message; $lengthof(#std.lib.message.alreadygot#)$>\n\t\t\t\t\tif is <%custom_message%; 0> then {\n\t\t\t\t\tmsg <You already have it.>\n\t\t\t\t\t}\n\t\t\t\t\telse {\n\t\t\t\t\tmsg <#std.lib.message.alreadygot#.>\n\t\t\t\t\t}\n\t\t\t\t}\n\t\t\t\telse {\n\t\t\t\t\tif here <#object#> then {\n\t\t\t\t\texec <take #object#;normal>\n\t\t\t\t\t}\n\t\t\t\t\telse {\n\t\t\t\t\texec <take #object#;normal>\n\t\t\t\t\t}\n\t\t\t\t}\n\t\t\t}\n\t\t}\n\t}\n\telse msg <Sorry, I don't understand '#quest.originalcommand#'.>\n}\nelse {\ndo <Old_Special_Take_Proc>\n}\nend define\n\n\ndefine procedure <Take_Back_Proc>\n\nif not is <#standard.lib.version#;> then {\n\n\tif not is <$instr(#standard.lib.containers#;#character#)$;0> then {\n\tif is <$instr(#standard.lib.characters#;#character#)$;0> then {\n\tsetstring <container;#character#>\n\t}\n\telse {\n\tsetstring <container;#object#>\n\t}\n\tdo <override_permitted>  \n\tif is <#override#;yes> then {\n\n\tif here <#character#> or got <#container#> then {\n\t\tif is <$locationof(#object#)$;#character#> then {\n\t\tmoveobject <#object#;#quest.currentroom#>\n\t\tgive <#object#>\n\t\thideobject <#object#>\n\t\tsetvar <custom_message; $lengthof(#std.lib.message.takefrom#)$>\n\t\t\tif is <%custom_message%; 0> then {\n\t\t\t\tif is <$gettag(#character#;look)$;character> then {\n\t\t\t\tmsg <You reach out and take the #object# from $capfirst(#character#)$.> }\n\t\t\t\telse msg <You take the #object# out of the #character#.> \n\t\t\t}\n\t\t\telse msg <#std.lib.message.takefrom#.>\n\t\t}\n\t\telse {\n\t\t\tsetvar <custom_message; $lengthof(#std.lib.message.objnotheld#)$>\n\t\t\tif is <%custom_message%;0> then {\n\t\t\t\tif is <$gettag(#character#;look)$;character> then {\n\t\t\t\tmsg <You can't do that, $capfirst(#character#)$ doesn't have the #object#.> }\n\t\t\t\telse msg <You can't do that, the #object# isn't in the #character#.>\n\t\t\t}\n\t\t\telse msg <#std.lib.message.objnotheld#.>\n\t\t}\n\t}\n\telse {\n\tsetvar <custom_message; $lengthof(#std.lib.message.charnothere#)$>\n\t\tif is <%custom_message%; 0> then {\n\t\t\tif is <$gettag(#character#;look)$;character> then {\n\t\t\tmsg <You can't do that, $capfirst(#character#)$ isn't here.> }\n\t\t\telse msg <You can't do that, the #character# isn't here.>\n\t\t}\n\t\telse msg <#std.lib.message.charnothere#.>\n\t}\n\n\t}\n\t}\n\telse msg <Sorry, I don't understand '#quest.originalcommand#'.>\n}\nelse {\ndo <Old_Take_Back_Proc>\n}\nend define\n\n\ndefine procedure <Put_In_Container_Proc>\n\tdo <override_permitted>  \n\tif is <#override#;yes> then {\n\n\tif is <#object#;#container#> then do <Put_In_Self_Proc>\n\telse {\n\tsetstring <test_prefix;$gettag(#container#;prefix)$>\n\t\tif is <$lengthof(#test_prefix#)$;0> then do <Not_A_Container_Proc>\n\t\telse do <Put_In_Container_Verified_Proc>\n\t}\n\n\t}\n\telse msg <Sorry, I don't understand '#quest.originalcommand#'.>\nend define\n\n\ndefine procedure <Not_A_Container_Proc>\n\tsetvar <custom_message; $lengthof(#std.lib.message.notacontainer#)$>\n\t\tif is <%custom_message%; 0> then msg <It's not possible to do that.>\n\t\telse msg <#std.library.message.notacontainer#.>\t\t\t\t\t\nend define\n\n\ndefine procedure <Put_In_Self_Proc>\n\tsetvar <custom_message; $lengthof(#std.lib.message.putinself#)$>\n\t\tif is <%custom_message%; 0> then msg <That would be some feat if you could manage it!>\n\t\telse msg <#std.library.message.putinself#.>\t\t\t\t\t\nend define\n\ndefine procedure <Put_In_Container_Verified_Proc>\n\tif is <$gettag(#container#;look)$;character> then {\n\tmsg <( assuming you meant \"|iGive #object# to $capfirst(#container#)$\"|xi. )>\n\texec <give #object# to #container#>\n\t}\n\telse if is <$gettag(#object#;look)$;character> and here <#object#> then do <Put_Char_In_Obj_Proc>\n\telse if is <$gettag(#object#;look)$;character> then do <Character_Not_Here_Proc>\n\telse {\n\tif got <#object#> and got <#container#> then do <Put_It_In_Proc>\n\t\telse {\n\t\t\tif got <#object#> and here <#container#> then do <Put_It_In_Proc>\n\t\t\telse {\n\t\t\t\tif got <#object#> then do <No_Container_Here_Proc>\n\t\t\t\telse {\n\t\t\t\tdo <Nothing_To_Put_In_Proc>\n\t\t\t\t}\n\t\t\t}\n\t\t}\n\t}\nend define\n\n\ndefine procedure <Put_Char_In_Obj_Proc>\n\tsetvar <custom_message; $lengthof(#std.lib.message.putcharinobj#)$>\n\t\tif is <%custom_message%; 0> then msg <You can't do that with $capfirst(#object#)$.>\n\t\telse msg <#std.lib.message.putcharinobj#.> \nend define\n\n\ndefine procedure <No_Container_Here_Proc>\n\tsetvar <custom_message; $lengthof(#std.lib.message.nocontainerhere#)$>\n\t\tif is <%custom_message%; 0> then msg <The #container# isn't available for you __\n\t\tto put things in at the moment.>\n\t\telse msg <#std.library.message.nocontainerhere#.>\t\t\t\t\t\nend define\n\n\ndefine procedure <Character_Not_Here_Proc>\n\tsetvar <custom_message; $lengthof(#std.lib.message.charnothere#)$>\n\t\tif is <%custom_message%; 0> then msg <$capfirst(#object#)$ isn't here.>\n\t\telse msg <#std.lib.message.charnothere#.>\nend define\n\n\ndefine procedure <Nothing_To_Put_In_Proc>\n\tsetvar <custom_message; $lengthof(#std.lib.message.nothingtoputin#)$>\n\t\tif is <%custom_message%; 0> then msg <You don't appear to be holding the #object#.>\n\t\telse msg <#std.lib.message.nothingtoputin#.>\nend define\n\n\ndefine procedure <Put_It_In_Proc>\n\tlose <#object#>\n\tmoveobject <#object#;#container#>\n\tshowobject <#object#@#container#>\n\tsetvar <custom_message; $lengthof(#std.lib.message.putincontainer#)$>\n\t\tif is <%custom_message%; 0> then msg <You put the #object# into the #container#.> \n\t\telse msg <#std.lib.message.putincontainer#.>\nend define\n\n\ndefine procedure <override_permitted>\n\tsetstring <override;#reset#>\n\tif is <$left(#quest.command#;6)$;custom> then {\n\t\tif is <#std.lib.override#;yes> then {\n\t\tsetstring <std.lib.override;#reset#>\n\t\tsetstring <override;yes>\n\t\t}\n\t\telse {\n\t\tsetstring <override;#reset#>\t\n\t\t}\n\t}\n\telse setstring <override;yes>\nend define\n\ndefine procedure <standard_lib_setup>\n\tsetstring <reset;>\n\tsetstring <std.lib.message.override;>\n\tsetstring <std.lib.message.drop;>\n\tsetstring <std.lib.message.notcarried;>\n\tsetstring <std.lib.message.alreadygot;>\n\tsetstring <std.lib.message.takefrom;>\n\tsetstring <std.lib.message.objnotheld;>\n\tsetstring <std.lib.message.charnothere;>\n\tsetstring <std.lib.message.nocontainerhere;>\n\tsetstring <std.lib.message.notacontainer;>\n\tsetstring <std.lib.message.putincontainer;>\n\tsetstring <std.lib.message.nothingtoputin;>\n\tsetstring <std.lib.message.putcharinobj;>\n\tsetstring <std.lib.message.putinself;>\nend define\n\n\n'==== Following code included for backward compatibility only ====\n\ndefine procedure <Old_Look_Proc>\n\tif got <#object#> then {\n\tmoveobject <#object#;#quest.currentroom#>\n\tshowobject <#object#>\n\texec <look at #object#;normal>\n\tdo <Old_Contents_Proc>\n\thideobject <#object#>\n\t}\n\telse {\n\tsetstring <where_it_is;$locationof(#object#)$>\n\t\t\n\tif here <#where_it_is#> or got <#where_it_is#> then {\n\t\tmoveobject <#object#;#quest.currentroom#>\n\t\tshowobject <#object#>\n\t\texec <look at #object#;normal>\n\t\tmoveobject <#object#;#where_it_is#>\n\t\tdo <Old_Contents_Proc>\n\t\t}\n\t\telse {\n\t\texec <look at #object#;normal>\n\t\tdo <Old_Contents_Proc>\n\t\t}\n\t}\nend define\n\n\ndefine procedure <Old_Contents_Proc>\n\tif is <$locationof(#object#)$;#quest.currentroom#> or got <#object#> then {\n\tsetstring <Where_We_Were;#quest.currentroom#>\n\toutputoff\n\tgoto <#object#>\n\t\tif is <$lengthof(#quest.objects#)$;0> then {\n\t\tgoto <#Where_We_Were#>\n\t\toutputon\n\t\t}\n\t\telse {\n\t\toutputon\n\t\tmsg <$gettag(#object#;prefix)$ #quest.formatobjects#.>\n\t\toutputoff\n\t\tgoto <#Where_We_Were#>\n\t\toutputon\n\t\t}\n\t}\nend define\n\ndefine procedure <Old_Special_Take_Proc>\n\tdo <override_permitted>  \n\tif is <#override#;yes> then {\n\n\tsetstring <where_it_is;$locationof(#object#)$>\n\tif here <#where_it_is#> or got <#where_it_is#> then exec <take #object# from #where_it_is#>\n\telse {\n\t\tif got <#object#> then {\n\t\tsetvar <custom_message; $lengthof(#std.lib.message.alreadygot#)$>\n\t\t\tif is <%custom_message%; 0> then msg <You already have it.>\n\t\t\telse msg <#std.lib.message.alreadygot#.>\n\t\t}\n\t\telse exec <take #object#;normal>\n\t}\n\n\t}\n\telse msg <Sorry, I don't understand '#quest.originalcommand#'.>\nend define\n\n\ndefine procedure <Old_Take_Back_Proc>\n\tdo <override_permitted>  \n\tif is <#override#;yes> then {\n\n\tif here <#character#> or got <#character#> then {\n\t\tif is <$locationof(#object#)$;#character#> then {\n\t\tmoveobject <#object#;#quest.currentroom#>\n\t\tgive <#object#>\n\t\thideobject <#object#>\n\t\tsetvar <custom_message; $lengthof(#std.lib.message.takefrom#)$>\n\t\t\tif is <%custom_message%; 0> then {\n\t\t\t\tif is <$gettag(#character#;look)$;character> then {\n\t\t\t\tmsg <You reach out and take the #object# from $capfirst(#character#)$.> }\n\t\t\t\telse msg <You take the #object# out of the #character#.> \n\t\t\t}\n\t\t\telse msg <#std.lib.message.takefrom#.>\n\t\t}\n\t\telse {\n\t\t\tsetvar <custom_message; $lengthof(#std.lib.message.objnotheld#)$>\n\t\t\tif is <%custom_message%;0> then {\n\t\t\t\tif is <$gettag(#character#;look)$;character> then {\n\t\t\t\tmsg <You can't do that, $capfirst(#character#)$ doesn't have the #object#.> }\n\t\t\t\telse msg <You can't do that, the #object# isn't in the #character#.>\n\t\t\t}\n\t\t\telse msg <#std.lib.message.objnotheld#.>\n\t\t}\n\t}\n\telse {\n\tsetvar <custom_message; $lengthof(#std.lib.message.charnothere#)$>\n\t\tif is <%custom_message%; 0> then {\n\t\t\tif is <$gettag(#character#;look)$;character> then {\n\t\t\tmsg <You can't do that, $capfirst(#character#)$ isn't here.> }\n\t\t\telse msg <You can't do that, the #character# isn't here.>\n\t\t}\n\t\telse msg <#std.lib.message.charnothere#.>\n\t}\n\n\t}\n\telse msg <Sorry, I don't understand '#quest.originalcommand#'.>\nend define\n";
libraries["q3ext.qlb"] = "!library\n!deprecated\n!asl-version <300>\n\n' N.B. THIS IS RELEASE 6 FOR QUEST 3.02 OR LATER\n\n!addto game\n   command <examine in #q3ext.qlb.object#> do <q3ext.qlb.ExamProc>\n   command <examine #q3ext.qlb.object#> do <q3ext.qlb.ExamProc>\n   command <look in #q3ext.qlb.object#> do <q3ext.qlb.ExamProc>   \n   command <look at #q3ext.qlb.object#> do <q3ext.qlb.LookProc>\n   command <look #q3ext.qlb.object#> do <q3ext.qlb.LookProc>  \n   command <give #q3ext.qlb.give#> do <q3ext.qlb.GiveProc>\n   command <drop #q3ext.qlb.object# in #q3ext.qlb.container#> do <q3ext.qlb.PutInProc>\n   command <drop #q3ext.qlb.object#> exec <drop $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>\n   command <take #q3ext.qlb.object# from #q3ext.qlb.container#> do <q3ext.qlb.TakeBackProc>\n   command <take #q3ext.qlc.container#'s #q3ext.qlb.object#> do <q3ext.qlb.TakeBackProc>\n   command <take #q3ext.qlb.container#s #q3ext.qlb.object#> do <q3ext.qlb.TakeBackProc>\n   command <take #q3ext.qlb.container#' #q3ext.qlb.object#> do <q3ext.qlb.TakeBackProc>\n   command <take #q3ext.qlb.object#> do <q3ext.qlb.SpecialTakeProc>\n   command <read #q3ext.qlb.object#> do <q3ext.qlb.ReadProc>\n   command <wear #q3ext.qlb.object#> do <q3ext.qlb.WearProc>\n   command <unwear #q3ext.qlb.object#> do <q3ext.qlb.UnWearProc>\n   command <open #q3ext.qlb.object#> do <q3ext.qlb.OpenProc>\n   command <close #q3ext.qlb.object#> do <q3ext.qlb.CloseProc>\n!end\n\n!addto synonyms\n   of; the =\n   x = examine\n   put; put down = drop\n   give back = give\n   pick up; pick; get; take back = take\n   out of; back from = from\n   in to; into; inside = in\n   back to = to\n   don; drop on = wear\n   take off; remove = unwear\n   shut = close\n   talk = speak\n   look at in = examine\n   go = go to\n   to to = to\n!end\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'* This is a dummy room - only needed for the force_refresh procedure to function  *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine room <q3ext.qlb.limbo>\n   look <Just a dummy!>\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*    This procedure implements an alternative room description - optional usage   * \n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.DescribeRoomProc>\n   set string <q3ext.qlb.indescription;$gettag(#quest.currentroom#;indescription)$>\n   if is <#q3ext.qlb.indescription#;> then {\n      msg <|nYou are in $gettag(#quest.currentroom#;prefix)$ |b|cl#quest.currentroom#.|cb|xb|xn>\n   }\n   else {\n      msg <|n#q3ext.qlb.indescription#$gettag(#quest.currentroom#;prefix)$ |b|cl#quest.currentroom#.|cb|xb|xn>                \n   }\n   set string <q3ext.qlb.ObjList;>\n   set string <q3ext.qlb.CharList;>\n   for each object in <#quest.currentroom#> {\n      if property <#quest.thing#;invisible> or property <#quest.thing#;hidden> then {\n      }\n      else {\n         if property <#quest.thing#;human> then {\n            set <q3ext.qlb.CharList;#q3ext.qlb.CharList# |b#quest.thing#|xb, >\n         }\n         else{\n            set <q3ext.qlb.ObjList;#q3ext.qlb.ObjList# $objectproperty(#quest.thing#;prefix)$>\n            set <q3ext.qlb.ObjList;#q3ext.qlb.ObjList# |b#quest.thing#|xb, >\n         }\n      }\n   }\n   if ($lengthof(#q3ext.qlb.CharList#)$ >0) then {\n      set numeric <q3ext.qlb.LengthOf;$lengthof(#q3ext.qlb.CharList#)$ - 1>\n      set <q3ext.qlb.CharList;$left(#q3ext.qlb.CharList#;%q3ext.qlb.LengthOf%)$>\n      set <q3ext.qlb.CharList;$q3ext.qlb.parsed(#q3ext.qlb.CharList#)$>\n         if ($instr(#q3ext.qlb.CharList#;_and_)$ >0) then { \n            msg < (#q3ext.qlb.CharList# are also here.)|xn>    \n         }\n         else {\n            msg < (#q3ext.qlb.CharList# is also here.)|xn>        \n         }\n      }\n      msg <|n|n$parameter(1)$|n>\n      if ($lengthof(#q3ext.qlb.ObjList#)$ >0) then {     \n         set <q3ext.qlb.LengthOf;$lengthof(#q3ext.qlb.ObjList#)$ - 1>\n         set <q3ext.qlb.ObjList;$left(#q3ext.qlb.ObjList#;%q3ext.qlb.LengthOf%)$>      \n         set <q3ext.qlb.ObjList;$q3ext.qlb.parsed(#q3ext.qlb.ObjList#)$>\n         if ($instr(#q3ext.qlb.ObjList#;_and_)$ >0) then { \n            msg <There are #q3ext.qlb.ObjList# here.|n>     \n         }\n         else {\n            msg <There is #q3ext.qlb.ObjList# here.|n>         \n         }        \n      }\n      if not is <#quest.doorways.dirs#;> then {\n         msg <You can move #quest.doorways.dirs#.>\n      }\n      if not is <#quest.doorways.places#;> then {\n         msg <You can go to #quest.doorways.places#.>\n      }\n      if not is <#quest.doorways.out#;> then {\n         msg <You can go out to |b#quest.doorways.out.display#|xb.>\n      }     \nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*  All regular 'take' commands are directed here to be tested for special needs   *\n'*  The code redirects to other procedures if object is in a container, and also   *\n'*  checks the player doesn't already have the item & prints message if he does.   *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.SpecialTakeProc>\n   set string <q3ext.qlb.containedby;$q3ext.qlb.containedby$>  \n   if (#q3ext.qlb.containedby# = nil) then {\n      if got <#q3ext.qlb.object#> then {\n         msg <You already have the #q3ext.qlb.object#.>           \n      } \n      else {\n         exec <take #q3ext.qlb.object#;normal>\n      }\n   }\n   else {\n      do <q3ext.qlb.TakeBackProc>\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*       This code is called if an object is taken that is in a container          * \n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.TakeBackProc>\n   set string <q3ext.qlb.containedby;$q3ext.qlb.containedby$>\n   if (#q3ext.qlb.containedby# = nil) then {\n      msg <(assuming you meant to take the #q3ext.qlb.object#).>\n      exec <take #q3ext.qlb.object#>\n   }\n   else {\n      if here <#q3ext.qlb.containedby#> or got <#q3ext.qlb.containedby#> then {\n         if not property <#q3ext.qlb.containedby#; closed> then {\n            move <#q3ext.qlb.object#;#quest.currentroom#>\n         }\n      }\n         outputoff\n         exec <take #q3ext.qlb.object#;normal>\n         outputoff\n         set string <q3ext.qlb.wherenow;#quest.currentroom#>\n         goto <q3ext.qlb.limbo>\n         goto <#q3ext.qlb.wherenow#>\n         outputon\n         if got <#q3ext.qlb.object#> then {\n            if property <#q3ext.qlb.containedby#;human> then {\n               msg <You take the #q3ext.qlb.object# from $capfirst(#q3ext.qlb.containedby#)$.>\n            }\n            else {\n               msg <You take the #q3ext.qlb.object# out of the #q3ext.qlb.containedby#.>\n            }\n         }\n      else {\n         exec <take #q3ext.qlb.object#;normal>\n      }\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*       Code called when an object is 'given to' or 'put into' a container        *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.GiveProc>\n   set numeric <q3ext.qlb.ToPos;$instr(#q3ext.qlb.give#;_to_)$>\n   if (%q3ext.qlb.ToPos% <> 0) then {\n      set string <q3ext.qlb.GiveObj;$left(#q3ext.qlb.give#;%q3ext.qlb.ToPos%)$>\n      set string <q3ext.qlb.GiveObj;$q3ext.qlb.realname(#q3ext.qlb.Giveobj#)$>\n      set <q3ext.qlb.ToPos;%q3ext.qlb.ToPos% + 4>\n      set string <q3ext.qlb.GiveTo;$mid(#q3ext.qlb.give#;%q3ext.qlb.ToPos%)$>\n      set string <q3ext.qlb.GiveTo;$q3ext.qlb.realname(#q3ext.qlb.GiveTo#)$>      \n      if not property <#q3ext.qlb.GiveObj#; unworn> and action <#q3ext.qlb.GiveObj#;wear> then {\n         msg <You'll have to take $objectproperty(#q3ext.qlb.GiveObj#;pronoun)$ off first.>\n      }\n      else {\n         if here <#q3ext.qlb.GiveObj#> or got <#q3ext.qlb.GiveObj#> then {\n            if here <#q3ext.qlb.GiveTo#> or got <#q3ext.qlb.GiveTo#> then {\n               if property <#q3ext.qlb.GiveTo#;container> and not property <#q3ext.qlb.GiveTo#; closed> then {\n                  if got <#q3ext.qlb.GiveTo#> then {\n                     outputoff\n                     exec <drop #q3ext.qlb.GiveTo#>\n                     outputon\n                     exec <give #q3ext.qlb.GiveObj# to $q3ext.qlb.objname(#q3ext.qlb.GiveTo#)$; normal>\n                     outputoff\n                     exec <take #q3ext.qlb.GiveTo#>\n                     do <force_refresh>\n                     outputon\n                  }\n                  else {\n                     exec <give #q3ext.qlb.GiveObj# to $q3ext.qlb.objname(#q3ext.qlb.GiveTo#)$; normal>\n                     do <force_refresh>                  \n                  }\n               }\n               else {\n                  if property <#q3ext.qlb.GiveObj#;human > then {\n                     msg <You try, but $capfirst(#q3ext.qlb.GiveObj#)$ isn't interested.>\n                  }\n                  else {\n                     msg <Hard as you try, you find you cannot do that|xn>\n                     if property <#q3ext.qlb.GiveTo#; closed> then {\n                        msg < while the #q3ext.qlb.GiveTo# is closed.>\n                     }\n                     else {\n                        msg <.|n>\n                     }\n                  }\n               }\n            }\n            else {\n               if property <#q3ext.qlb.GiveTo#;human> then {\n                  msg <$capfirst(#q3ext.qlb.GiveObj#)$ isn't here.>\n               }\n               else {\n                  msg <The #q3ext.qlb.GiveTo# isn't available.>\n               }          \n            }\n         }\n         else {\n            if property <#q3ext.qlb.GiveObj#;human > then {\n               msg <$capfirst(#q3ext.qlb.GiveObj#)$ isn't here.>\n            }\n            else {\n               msg <The #q3ext.qlb.GiveObj# isn't available.>\n            }\n         }\n      }\n   }\n   else {\n'***  Deals with 'Give character the object' style. \n      for <q3ext.qlb.SpaceTrue;1;$lengthof(#q3ext.qlb.give#)$> do <q3ext.qlb.SpaceTest>\n      set string <q3ext.qlb.GiveTo;$left(#q3ext.qlb.give#;%q3ext.qlb.FoundSpace%)$>\n      set string <q3ext.qlb.GiveObj;$mid(#q3ext.qlb.give#;%q3ext.qlb.FoundSpace%)$>\n      exec <give #q3ext.qlb.GiveObj# to #q3ext.qlb.GiveTo#>\n    }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*   A utility procedure, called to find the positions of spaces in player input   *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.SpaceTest>\n   set string <q3ext.qlb.Space;$mid(c o;2;1)$>\n   set string <q3ext.qlb.TestPart;$mid(#q3ext.qlb.give#;%q3ext.qlb.SpaceTrue%;1)$>\n   if is <#q3ext.qlb.space#;#q3ext.qlb.TestPart#> then set numeric <q3ext.qlb.FoundSpace;%q3ext.qlb.SpaceTrue%>\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*      This procedure deals with examining containers & objects in containers.    *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.ExamProc>\n   set <q3ext.qlb.object; $q3ext.qlb.objname(#q3ext.qlb.object#)$>\n   set <q3ext.qlb.object; $q3ext.qlb.wornfix()$>\n   set string <q3ext.qlb.containedby;$q3ext.qlb.containedby$>\n   set string <q3ext.qlb.whereat;#quest.currentroom#>\n   if (#q3ext.qlb.containedby# = nil) then {\n      if action <#q3ext.qlb.object#; open> then {\n         if property <#q3ext.qlb.object#; closed> then {\n            if ($objectproperty(#q3ext.qlb.object#; closedexam)$ = null) then {\n               msg <Nothing out of the ordinary.>               \n            }\n            else {\n               msg <$objectproperty(#q3ext.qlb.object#; closedexam)$.>\n            }\n         }\n         else {\n            if ($objectproperty(#q3ext.qlb.object#; openexam)$ = null) then {\n               msg <Nothing out of the ordinary.>               \n            }\n            else {\n               msg <$objectproperty(#q3ext.qlb.object#; openexam)$.>\n            }\n         }\n      }\n      else {\n         exec <examine #q3ext.qlb.object#;normal>\n      }\n      if property <#q3ext.qlb.object#;container> then {\n         if here <#q3ext.qlb.object#> or got <#q3ext.qlb.object#> then {\n            doaction <#q3ext.qlb.object#;contents>\n         }\n      }\n   }\n   else {\n      if here <#q3ext.qlb.containedby#> or got <#q3ext.qlb.containedby#> then {\n         if not property <#q3ext.qlb.containedby#; closed> then {\n            outputoff\n            goto <#q3ext.qlb.containedby#_inventory>\n            outputon\n               if action <#q3ext.qlb.object#; open> then {\n                  if property <#q3ext.qlb.object#; closed> then {\n                     if ($objectproperty(#q3ext.qlb.object#; closedexam)$ = null) then {\n                        msg <Nothing out of the ordinary.>               \n                     }\n                     else {\n                        msg <$objectproperty(#q3ext.qlb.object#; closedexam)$.>\n                     }\n                  }\n                  else {\n                     if ($objectproperty(#q3ext.qlb.object#; openexam)$ = null) then {\n                        msg <Nothing out of the ordinary.>               \n                     }\n                     else {\n                        msg <$objectproperty(#q3ext.qlb.object#; openexam)$.>\n                     }\n                  }\n               }\n               else {\n                  exec <examine #q3ext.qlb.object#;normal>\n               }\n            outputoff\n            goto <#q3ext.qlb.whereat#>\n            outputon\n         }\n         else {\n            exec <examine #q3ext.qlb.object#;normal>\n         }\n      }\n      else {\n         exec <examine #q3ext.qlb.object#;normal>         \n      }\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*      This procedure deals with 'looking at' objects held in containers.         *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.LookProc>\n   set <q3ext.qlb.object; $q3ext.qlb.realname(#q3ext.qlb.object#)$>\n   set <q3ext.qlb.object; $q3ext.qlb.wornfix()$>\n   set string <q3ext.qlb.containedby;$q3ext.qlb.containedby$>\n   set string <q3ext.qlb.whereat;#quest.currentroom#>\n   if (#q3ext.qlb.containedby# = nil) then {\n      exec <look at $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>\n   }\n   else {\n      if here <#q3ext.qlb.containedby#> or got <#q3ext.qlb.containedby#> then {\n         if not property <#q3ext.qlb.containedby#; closed> then {\n            outputoff\n            goto <#q3ext.qlb.containedby#_inventory>\n            outputon\n            exec <look at $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>   \n            outputoff\n            goto <#q3ext.qlb.whereat#>\n            outputon\n         }\n         else {\n            exec <look at $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>\n         }\n      }\n      else {\n         exec <look at $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>\n      }\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*   Redirects 'put into' so that it is processed by the 'give to' routine, these  *\n'*   are functionally identical so no need to have duplicate code                  *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.PutInProc>\n   exec <give #q3ext.qlb.object# to #q3ext.qlb.container#>\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*  A utility procedure, returns the position of the last comma in a passed string *\n'*  Used to replace the last comma with 'and' when parsing output strings          *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.LastCommaProc>\n   set string <q3ext.qlb.TestPart;$mid(#q3ext.qlb.Param#;%q3ext.qlb.LastComma%;1)$>\n   if is <#q3ext.qlb.TestPart#;,> then set <q3ext.qlb.FoundComma;%q3ext.qlb.LastComma%>\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*  The 'read' command procedure.                                                  *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.ReadProc>\n   set <q3ext.qlb.object;$q3ext.qlb.realname(#q3ext.qlb.object#)$>\n   if property <#q3ext.qlb.object#;readable> then {\n      doaction <#q3ext.qlb.object#;read>\n      if not is <$objectproperty(#q3ext.qlb.object#;readaction)$; nil> then {\n         doaction <#q3ext.qlb.object#;readaction>\n      }\n   }\n   else {\n      msg <There's nothing to read!>\n   }\nend define\n\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*  The 'wear' command procedure.                                                  *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.WearProc>\n   set <q3ext.qlb.object;$q3ext.qlb.realname(#q3ext.qlb.object#)$>\n   msg <#q3ext.qlb.object#.....>\n   if got <#q3ext.qlb.object#> then {\n      if action <#q3ext.qlb.object#;wear> then {\n         if ($objectproperty(#q3ext.qlb.object#;sex)$ = %q3ext.qlb.playersex%) then {\n            do <q3ext.qlb.CheckWearProc>\n         }\n         else {\n            msg <On second thoughts you decide that the |xn>\n            msg <#q3ext.qlb.object# |xn>\n            if (%q3ext.qlb.playersex% = 1) then {\n               msg <won't suit a man like you at all.>\n            }\n            else {\n               msg <won't suit a woman like you at all.>\n            }\n         }\n         }\n         else {\n         msg <You cannot wear the #q3ext.qlb.object#.>\n         }\n   }\n   else {\n      msg <You are not carrying the #q3ext.qlb.object#.>\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*  The 'CheckWear' procedure - this checks the sense of wear commands             *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.CheckWearProc>\n   set numeric <q3ext.qlb.wearable;0>\n   if ($objectproperty(#q3ext.qlb.object#;topcover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;topcover)$ <= %q3ext.qlb.topcovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>\n   }\n   if ($objectproperty(#q3ext.qlb.object#;headcover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;headcover)$ <= %q3ext.qlb.headcovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>\n   }\n   if ($objectproperty(#q3ext.qlb.object#;feetcover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;feetcover)$ <= %q3ext.qlb.feetcovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>\n   }\n   if ($objectproperty(#q3ext.qlb.object#;handscover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;handscover)$ <= %q3ext.qlb.handscovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>\n   }\n' - disallow for coats (don't affect ability to put on lower torso clothes)\n   set <q3ext.qlb.tempcovered;%q3ext.qlb.botcovered%>\n   if (%q3ext.qlb.tempcovered% > 63) and __\n   ($objectproperty(#q3ext.qlb.object#;botcover)$ < 33) then {\n      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% - 64>\n   }\n' - disallow for skirts and dresses (like coats!)\n   if (%q3ext.qlb.tempcovered% > 31) and __\n   ($objectproperty(#q3ext.qlb.object#;botcover)$ < 16) and __\n   ($objectproperty(#q3ext.qlb.object#;botcover)$ <> 4) then {\n      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% - 32>\n   }\n' - disallow wearing of skirts/dresses and trousers simultaneously\n   if (%q3ext.qlb.tempcovered% > 15) then {\n      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% + 16> \n   }\n   if ($objectproperty(#q3ext.qlb.object#;botcover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;botcover)$ <= %q3ext.qlb.tempcovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>\n   }\n   if (%q3ext.qlb.wearable% =0) then {\n      doaction <#q3ext.qlb.object#;wear>\n      property <#q3ext.qlb.object#; not unworn>\n      set <q3ext.qlb.topcovered;%q3ext.qlb.topcovered% + $objectproperty(#q3ext.qlb.object#;topcover)$>\n      set <q3ext.qlb.headcovered;%q3ext.qlb.headcovered% + $objectproperty(#q3ext.qlb.object#;headcover)$>\n      set <q3ext.qlb.handscovered;%q3ext.qlb.handscovered% + $objectproperty(#q3ext.qlb.object#;handscover)$>\n      set <q3ext.qlb.feetcovered;%q3ext.qlb.feetcovered% + $objectproperty(#q3ext.qlb.object#;feetcover)$>\n      set <q3ext.qlb.botcovered;%q3ext.qlb.botcovered% + $objectproperty(#q3ext.qlb.object#;botcover)$>\n   }\n   else {\n      msg <Given what you are already wearing - that makes no sense at all.>\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*  The 'unwear' command procedure.                                                *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.UnWearProc>\n   set <q3ext.qlb.object;$q3ext.qlb.realname(#q3ext.qlb.object#)$>\n   if not property <#q3ext.qlb.object#; unworn> then {\n      if action <#q3ext.qlb.object#;unwear> then {\n         do <q3ext.qlb.CheckUnwearProc>\n      }\n      else {\n      msg <You cannot do that.>\n      }\n   }\n   else {\n      msg <You aren't wearing the #q3ext.qlb.object#.>\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*  The 'CheckUnwear' procedure.                                                   *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.CheckUnwearProc>\n   set numeric <q3ext.qlb.wearable;0>\n   set <q3ext.qlb.tempcovered; %q3ext.qlb.topcovered% /2>\n   if ($objectproperty(#q3ext.qlb.object#;topcover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;topcover)$ <= %q3ext.qlb.tempcovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>\n   }\n   set <q3ext.qlb.tempcovered; %q3ext.qlb.headcovered% /2>\n   if ($objectproperty(#q3ext.qlb.object#;headcover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;headcover)$ <= %q3ext.qlb.tempcovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 2>\n   }\n   set <q3ext.qlb.tempcovered; %q3ext.qlb.feetcovered% /2>\n   if ($objectproperty(#q3ext.qlb.object#;feetcover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;feetcover)$ <= %q3ext.qlb.tempcovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 4>\n   }\n   set <q3ext.qlb.tempcovered; %q3ext.qlb.handscovered% /2>   \n      if ($objectproperty(#q3ext.qlb.object#;handscover)$ <> 0) and __\n      ($objectproperty(#q3ext.qlb.object#;handscover)$ <= %q3ext.qlb.tempcovered%) then {\n         set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 8>\n   }\n' - disallow for coats (don't affect ability to take off lower torso clothes)\n   set <q3ext.qlb.tempcovered;%q3ext.qlb.botcovered%>\n   if (%q3ext.qlb.tempcovered% > 63) then {\n      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% - 64>\n   }\n' - disallow for skirts and dresses (like coats!)\n   if (%q3ext.qlb.tempcovered% > 31) and __\n   ($objectproperty(#q3ext.qlb.object#;botcover)$ <> 4) then {\n      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% - 32>\n   }\n   set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% /2>\n   if ($objectproperty(#q3ext.qlb.object#;botcover)$ <> 0) and __\n   ($objectproperty(#q3ext.qlb.object#;botcover)$ <= %q3ext.qlb.tempcovered%) then {\n      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 16>\n   }\n   if (%q3ext.qlb.wearable% =0) then {\n      doaction <#q3ext.qlb.object#;unwear>\n      property <#q3ext.qlb.object#; unworn>\n      set <q3ext.qlb.topcovered;%q3ext.qlb.topcovered% - $objectproperty(#q3ext.qlb.object#;topcover)$>\n      set <q3ext.qlb.headcovered;%q3ext.qlb.headcovered% - $objectproperty(#q3ext.qlb.object#;headcover)$>\n      set <q3ext.qlb.handscovered;%q3ext.qlb.handscovered% - $objectproperty(#q3ext.qlb.object#;handscover)$>\n      set <q3ext.qlb.feetcovered;%q3ext.qlb.feetcovered% - $objectproperty(#q3ext.qlb.object#;feetcover)$>\n      set <q3ext.qlb.botcovered;%q3ext.qlb.botcovered% - $objectproperty(#q3ext.qlb.object#;botcover)$>\n   }\n   else {\n      msg <Given what you are wearing, that isn't possible.>\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'* Open and close commands for 'openable' type                                     *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.OpenProc>\n   set <q3ext.qlb.object; $q3ext.qlb.realname(#q3ext.qlb.object#)$>\n   if action <#q3ext.qlb.object#; open> then {\n      doaction <#q3ext.qlb.object#; open> \n   }\n   else msg <You can't do that.>\nend define\n\ndefine procedure <q3ext.qlb.CloseProc>\n   set <q3ext.qlb.object; $q3ext.qlb.realname(#q3ext.qlb.object#)$>\n   if action <#q3ext.qlb.object#; close> then {\n      doaction <#q3ext.qlb.object#; close>\n   }\n   else msg <You can't do that.>\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'* This forces the inventory window to be refreshed - Quest doesn't do it itself!  *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <force_refresh>\n      outputoff\n      set string <q3ext.qlb.wherenow;#quest.currentroom#>\n      goto <q3ext.qlb.limbo>\n      goto <#q3ext.qlb.wherenow#>      \n      outputon\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'* This is called to initialise the library                                        *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine procedure <q3ext.qlb.setup>\n'  These are defaults - making the player male and naked! - it will probably be \n'  required to set these variables differently in the startscript AFTER calling\n'  this setup routine\n'  'q3ext.qlb.playersex' should be set to 1 for a male player, 2 for a female.\n   set numeric <q3ext.qlb.headcovered;0>\n   set numeric <q3ext.qlb.handscovered;0>\n   set numeric <q3ext.qlb.feetcovered;0>\n   set numeric <q3ext.qlb.topcovered;0>\n   set numeric <q3ext.qlb.botcovered;0>\n   set numeric <q3ext.qlb.tempcovered;0>\n   set numeric <q3ext.qlb.playersex;1>\n   set numeric <q3ext.qlb.version;6>\n   if (%q3ext.qlb.version% < $parameter(1)$) then {\n      msg <WARNING!|n|nThis game requires Version $parameter(1)$ (or later) of _\n      the Q3EXT.QLB library, you appear to have Version %q3ext.qlb.version%. _\n      |n|nPlease obtain the latest release of the library before trying to run this _\n      game.|n|n|w>\n   }\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*                         FUNCTION DEFINITIONS FOLLOW                             *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'* Returns passed string that was comma separated list 'parsed' to read naturally. *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine function <q3ext.qlb.parsed>\n   set numeric <q3ext.qlb.FoundComma;0>\n   set string <q3ext.qlb.Param;$parameter(1)$>\n   for <q3ext.qlb.LastComma;1;$lengthof(#q3ext.qlb.Param#)$> {\n   do <q3ext.qlb.LastCommaProc>\n   }\n      if not is <%q3ext.qlb.FoundComma%;0> then { \n      set numeric <q3ext.qlb.remaining;%q3ext.qlb.LastComma%-%q3ext.qlb.FoundComma%>\n      set <q3ext.qlb.remaining;%q3ext.qlb.remaining%-1>\n      set <q3ext.qlb.FoundComma;%q3ext.qlb.FoundComma%-1>\n      set string <q3ext.qlb.LeftPart;$left(#q3ext.qlb.Param#;__\n      %q3ext.qlb.FoundComma%)$>\n      set string <q3ext.qlb.RightPart;$right(#q3ext.qlb.Param#;__\n      %q3ext.qlb.remaining%)$>\n      set string <q3ext.qlb.parsed;#q3ext.qlb.LeftPart# and #q3ext.qlb.RightPart#>\n      }\n      if is <%q3ext.qlb.FoundComma%;0> then {\n      set string <q3ext.qlb.parsed;#q3ext.qlb.Param#>\n      }\n      return <#q3ext.qlb.parsed#>\nend define\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'* The following function returns the name of the object in who's container the    *\n'* looked at object is held, or 'nil' if the object isn't in a container.          *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine function <q3ext.qlb.containedby>\n   set <q3ext.qlb.object; $q3ext.qlb.realname(#q3ext.qlb.object#)$>\n   set string <q3ext.qlb.containedby;$locationof(#q3ext.qlb.object#)$>\n   set numeric <q3ext.qlb.LengthOf;$lengthof(#q3ext.qlb.containedby#)$>\n   if (%q3ext.qlb.LengthOf% < 11) then {\n      set <q3ext.qlb.containedby;nil>\n   }\n   if (%q3ext.qlb.LengthOf% > 11) then {\n      set <q3ext.qlb.containedby;$right(#q3ext.qlb.containedby#;10)$>\n   }\n   if (#q3ext.qlb.containedby# <> _inventory) then {\n      set <q3ext.qlb.containedby;nil>\n   }\n   if (#q3ext.qlb.containedby# = _inventory) then {\n      set <q3ext.qlb.containedby;$locationof(#q3ext.qlb.object#)$>\n      set <q3ext.qlb.LengthOf;%q3ext.qlb.LengthOf%-10>\n      set <q3ext.qlb.containedby;$left(#q3ext.qlb.containedby#;%q3ext.qlb.LengthOf%)$>    \n   }\n   return <#q3ext.qlb.containedby#>\nend define\n\ndefine function <q3ext.qlb.wornfix>\n   set string <q3ext.qlb.wornfix;#q3ext.qlb.object#>\n   if action <#q3ext.qlb.object#; wear> and not property <#q3ext.qlb.object#; unworn> then {\n      set <q3ext.qlb.wornfix;#q3ext.qlb.object# [worn]>\n   }\n   return <#q3ext.qlb.wornfix#>\nend define\n\ndefine function <q3ext.qlb.objname>\n      set string <q3ext.qlb.objname;$parameter(1)$>\n      set <q3ext.qlb.objname;$q3ext.qlb.realname(#q3ext.qlb.objname#)$>      \n      set <q3ext.qlb.objname;$displayname(#q3ext.qlb.objname#)$>\n      return <#q3ext.qlb.objname#>\nend define\n\ndefine function <q3ext.qlb.realname>\n      set numeric <q3ext.qlb.found;0>\n      set string <q3ext.qlb.objname;$parameter(1)$>\n      set string <q3ext.qlb.lobjname;$lcase(#q3ext.qlb.objname#)$>\n\n      for each object in <#quest.currentroom#> {\n         set string <q3ext.qlb.display;$displayname(#quest.thing#)$>\n         set string <q3ext.qlb.display;$lcase(#q3ext.qlb.display#)$>\n         set string <q3ext.qlb.realname;#quest.thing#>\n         set string <q3ext.qlb.realname;$lcase(#q3ext.qlb.realname#)$>\n         if ( $instr(#q3ext.qlb.display#;#q3ext.qlb.lobjname#)$ > 0 ) or _\n            ( $instr(#q3ext.qlb.lobjname#;#q3ext.qlb.realname#)$ > 0 ) then {  \n             set <q3ext.qlb.objname;#quest.thing#>\n             set <q3ext.qlb.found;1>\n         }    \n      }\n      if (%q3ext.qlb.found% = 0) then {\n         for each object in inventory {\n         set string <q3ext.qlb.display;$displayname(#quest.thing#)$>\n         set string <q3ext.qlb.display;$lcase(#q3ext.qlb.display#)$>\n         set string <q3ext.qlb.realname;#quest.thing#>\n         set string <q3ext.qlb.realname;$lcase(#q3ext.qlb.realname#)$>\n            if ( $instr(#q3ext.qlb.display#;#q3ext.qlb.lobjname#)$ > 0 ) or _\n               ( $instr(#q3ext.qlb.lobjname#;#q3ext.qlb.realname#)$ > 0 ) then {  \n               set <q3ext.qlb.objname;#quest.thing#>      \n               set <q3ext.qlb.found;1>\n            }    \n         }     \n      }\n       if (%q3ext.qlb.found% = 0) then {\n         for each object in game {\n         set string <q3ext.qlb.display;$displayname(#quest.thing#)$>\n         set string <q3ext.qlb.display;$lcase(#q3ext.qlb.display#)$>\n         set string <q3ext.qlb.realname;#quest.thing#>\n         set string <q3ext.qlb.realname;$lcase(#q3ext.qlb.realname#)$>\n            if ( $instr(#q3ext.qlb.display#;#q3ext.qlb.lobjname#)$ > 0 ) or _\n               ( $instr(#q3ext.qlb.lobjname#;#q3ext.qlb.realname#)$ > 0 ) then {  \n                set <q3ext.qlb.objname;#quest.thing#>      \n            }    \n         }     \n      }     \n   return <#q3ext.qlb.objname#>\nend define\n\n\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n'*                           TYPE DEFINITIONS FOLLOW                               *\n'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine type <human>\n   human\n   type <container>\n   displaytype=Person\n   action <take> {\n      set string <q3ext.qlb.object; $mid(#quest.command#;6)$>\n      msg <You can't take $capfirst(#q3ext.qlb.object#)$!>\n   }\n   action <speak> {\n      set string <q3ext.qlb.object; $mid(#quest.command#;10)$>\n      msg <$capfirst(#q3ext.qlb.object#)$ doesn't reply to you.>\n   }\n      action <give anything> {\n         if property <#q3ext.qlb.GiveTo#;container> then {\n         msg <|n$capfirst(#q3ext.qlb.GiveTo#)$ accepts the #q3ext.qlb.GiveObj#.>\n         move <#q3ext.qlb.GiveObj#;#q3ext.qlb.GiveTo#_inventory>\n         }\n         else {\n         msg <$capfirst(#q3ext.qlb.GiveTo#)$ doesn't seem to want the #q3ext.qlb.GiveObj#.>\n         }\n   }\nend define\n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine type <container>\n   container\n   action <contents> {\n      if not property <#q3ext.qlb.object#; closed> then {\n         outputoff\n         goto <#q3ext.qlb.object#_inventory>\n         set string <q3ext.qlb.whatsheld;$gettag(#q3ext.qlb.object#_inventory;prefix)$$q3ext.qlb.parsed(#quest.formatobjects#)$.>\n         if ($lengthof(#quest.objects#)$ = 0) then set <q3ext.qlb.whatsheld;>\n         outputon\n         msg <#q3ext.qlb.whatsheld#>         \n         outputoff\n         goto <#q3ext.qlb.whereat#>\n         outputon\n      }\n   }\n\n   action <give anything> {\n      msg <You put the #q3ext.qlb.GiveObj# in the #q3ext.qlb.GiveTo#.>\n      move <#q3ext.qlb.GiveObj#; #q3ext.qlb.GiveTo#_inventory>\n   }\nend define\n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine type <object>\n      prefix = a\n      take = nil\n      action <take> {\n         if ($objectproperty(#q3ext.qlb.object#;take)$=nil) then {\n            msg <You pick up the #q3ext.qlb.object#.>\n         }\n         else {\n         msg <$objectproperty(#q3ext.qlb.object#;take)$.>      \n         }     \n      move <#q3ext.qlb.object#;inventory>\n      do <force_refresh>\n      }\nend define\n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine type <scenery>\n   invisible\n   prefix = a\n   pronoun = that\n   notake = nil\n   action <take> {\n      if ($objectproperty(#q3ext.qlb.object#;notake)$=nil) then {\n      msg <Taking $objectproperty(#q3ext.qlb.object#;pronoun)$ would serve no useful purpose.>\n      }\n      else {\n      msg <$objectproperty(#q3ext.qlb.object#;notake)$.>    \n      }\n   }\nend define\n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine type <readable>\n   readable\n   readmessage = You start to read but it is so incredibly dull you decide not to bother.\n   readaction = nil\n   action <read> {\n   set string <q3ext.qlb.objname;$thisobject$>\n   msg <$objectproperty(#q3ext.qlb.objname#;readmessage)$>\n   }\nend define\n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n' *  The openable type                                                              *\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine type <openable>\n   closed\n   properties <closeddesc = null>\n   properties <opendesc = null>\n   properties <closingdesc = null>\n   properties <openingdesc = null>\n   properties <closedexam = null>\n   properties <openexam = null>  \n   \n   prefix = a\n     \n   action <open> {\n   set string <q3ext.qlb.opened; $thisobject$>   \n   if property <$thisobject$; closed> then {\n      property <$thisobject$; not closed>\n      if ($objectproperty(#q3ext.qlb.opened#; openingdesc)$ = null) then {\n         msg <You open the #q3ext.qlb.object#.>\n      }\n      else {\n         msg <$objectproperty(#q3ext.qlb.opened#; openingdesc)$>\n      }\n   }\n   else msg <The #q3ext.qlb.object# is already open.>\n   }\n    \n   action <close> {\n   set string <q3ext.qlb.closed; $thisobject$>    \n   if not property <$thisobject$;closed> then {\n      property <$thisobject$; closed>\n      if ($objectproperty(#q3ext.qlb.closed#; closingdesc)$ = null) then {\n         msg <You close the #q3ext.qlb.object#.>\n      }\n      else {\n         msg <$objectproperty(#q3ext.qlb.closed#; closingdesc)$.>      \n      }\n   }\n   else msg <The #q3ext.qlb.object# is already closed.>\n   }\n    \n   action <look> {\n   set string <q3ext.qlb.lookedat; $thisobject$>\n   if property <$thisobject$; closed> then {\n      if ($objectproperty(#q3ext.qlb.lookedat#; closeddesc)$ = null) then {\n         set string <q3ext.qlb.lookmessage ;The #q3ext.qlb.lookedat# is closed>\n      }\n      else {\n         set string <q3ext.qlb.lookmessage ;$objectproperty(#q3ext.qlb.lookedat#; closeddesc)$.>\n      }\n   }\n   else {\n      if ($objectproperty(#q3ext.qlb.lookedat#; opendesc)$ = null) then {\n         set string <q3ext.qlb.lookmessage ;The #q3ext.qlb.lookedat# is open>\n      }\n      else {\n         set string <q3ext.qlb.lookmessage ;$objectproperty(#q3ext.qlb.lookedat#; opendesc)$.>\n      }\n   }\n   msg <#q3ext.qlb.lookmessage#.>\n   }\n   \nend define\n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n' * This clothing type is not used directly but inherited by specific clothes.      *\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n \ndefine type <clothing>\n   type <object>\n   unworn\n   headcover = 0\n   handscover = 0\n   feetcover = 0\n   topcover = 0\n   botcover = 0\n   sex = 1\n   pronoun = it\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   properties <alias=$thisobject$>\n      action <wear> {\n      set string <q3ext.qlb.objname;$thisobject$>\n      msg <$objectproperty(#q3ext.qlb.objname#;wearmessage)$>\n      property <#q3ext.qlb.objname#;alias=#q3ext.qlb.objname# [worn]>\n      move <#q3ext.qlb.objname#;inventory>\n      do <force_refresh>\n      }\n      action <unwear> {\n      set string <q3ext.qlb.objname;$thisobject$>\n      msg <$objectproperty(#q3ext.qlb.objname#;unwearmessage)$>\n      property <#q3ext.qlb.objname#;alias=$thisobject$>\n      do <force_refresh>\n      }   \nend define  \n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n' * This lib defines 15 specific clothing objects, and most of these can be used    *\n' * to good effect for other garments - the defined objects are listed together     *\n' * with other garments that work similarly for game purposes.                      *\n' *                                                                                 *\n' * DEFINED GARMENT : ALSO USABLE FOR THESE GARMENTS                                *\n' * hat             : any headwear                                                  *\n' * gloves          : any handwear                                                  *\n' * shoes           : boots, outer footwear generally                               *\n' * socks           : stockings                                                     *\n' * tights          : pantie hose                                                   *\n' * undies          : panties, briefs - lower portion underwear generally           *\n' * teddy           : uhm.. any underthing that covers like a teddy!                *\n' * trousers        : jeans, shorts (not the underwear variety)                     *\n' * dress           : coverall                                                      *\n' * skirt           : kilt maybe?                                                   *\n' * vest            : bra, other 'top only' undergarment                            *\n' * shirt           : blouse, T-Shirt etc.                                          *\n' * sweater         : pullover, sweatshirt - '2nd layer' top garment                *\n' * jacket          : fleece, parka, anorak, short coat of whatever type            *\n' * coat            : any long length outermost garment like a coat                 *\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine type <shoes>\n      type <clothing>\n      feetcover = 4\n      wearmessage = You put them on.\n      unwearmessage = You take them off.\n      pronoun = them\n   properties <prefix=a pair of>\nend define\n\ndefine type <socks>\n      type <clothing>\n      feetcover = 2\n      wearmessage = You put them on.\n      unwearmessage = You take them off.\n      pronoun = them\n   properties <prefix=a pair of>\nend define\n\ndefine type <tights>\n      type <clothing>\n      feetcover = 2\n      botcover = 8\n      pronoun = them\n      wearmessage = You put them on.\n      unwearmessage = You take them off.\n   properties <prefix=a pair of>\nend define\n\ndefine type <hat>\n      type <clothing>\n      headcover = 2\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\ndefine type <gloves>\n      type <clothing>\n      handscover = 2\n      wearmessage = You put them on.\n      unwearmessage = You take them off.\n      pronoun = them\n properties <prefix=a pair of>\nend define\n\ndefine type <vest>\n      type <clothing>\n      topcover = 2\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\ndefine type <shirt>\n      type <clothing>\n      topcover = 8\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\ndefine type <teddy>\n      type <clothing>\n      topcover = 4\n      botcover = 4\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\ndefine type <undies>\n      type <clothing>\n      botcover = 2\n      wearmessage = You put them on.\n      unwearmessage = You take them off.\n      pronoun = them\n   properties <prefix=a pair of>\nend define\n\ndefine type <dress>\n      type <clothing>\n      topcover = 8\n      botcover = 32\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\ndefine type <skirt>\n      type <clothing>\n      botcover = 32\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\ndefine type <trousers>\n      type <clothing>\n      botcover = 16\n      wearmessage = You put them on.\n      unwearmessage = You take them off.\n      pronoun = them\n   properties <prefix=a pair of>\nend define\n\ndefine type <sweater>\n      type <clothing>\n      topcover = 16\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\ndefine type <jacket>\n      type <clothing>\n      topcover = 32\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\ndefine type <coat>\n      type <clothing>\n      topcover = 64\n      botcover = 64\n      wearmessage = You put it on.\n      unwearmessage = You take it off.\n   properties <prefix=a>\nend define\n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *";
libraries["typelib.qlb"] = "!library\n!deprecated\n!asl-version <311>\n!name <MaDbRiT's Type Library>\n!version <1.009 06-Sep-2003>\n!author <MaDbRiT (Al Bampton)>\n!help <typelib.pdf>\n! This library adds a whole set of types, with related actions and properties to Quest.\n! It is easy to use from Q.D.K. but be sure to read the manual before you start. \n\n\n'=====================================================\n'== This is MaDbRiT's QUEST Types Library @06-09-03 ==\n'== It will only work with Quest 3.11 build 137 or  == \n'== later This version is specifically designed to  ==\n'== be easily usable from Q.D.K.                    ==\n'=====================================================\n'==        !!!! PLEASE READ THE MANUAL !!!!         ==\n'==        !!!! NEVER CHANGE THIS FILE !!!!         ==\n'=====================================================\n\n'======================================\n'== Add these lines to the gameblock ==\n'======================================\n\n!addto game\n   command <look at #TLSdObj#; look #TLSdObj#; l #TLSdObj#> do <TLPlook>\n   command <examine #TLSdObj#; inspect #TLSdObj#; x #TLSdObj#> do <TLPexamine>\n   command <open #TLSdObj#> do <TLPopen>\n   command <close #TLSdObj#; shut #TLSdObj#> do <TLPclose>   \n   command <take #TLSdObj# from #TLSiObj#> do <TLPtake>\n   command <take #TLSdObj# off> do <TLPunWear>\n   command <take #TLSdObj#> do <TLPtake>\n   command <drop #TLSdObj# into #TLSiObj#> do <TLPputIn>\n   command <drop #TLSdObj# on> do <TLPwear>   \n   command <drop #TLSdObj# down; drop down #TLSdObj#; drop #TLSdObj#> do <TLPdrop>   \n   command <give #TLSdObj# to #TLSiObj#;give #TLSiObj# the #TLSdObj#> do <TLPgive>\n   command <read #TLSdObj#> do <TLPread>   \n   command <wear #TLSdObj#> do <TLPwear>\n   command <unwear #TLSdObj#> do <TLPunWear>\n   description {\n      do <TLProomDescription>\n   }\n   startscript do <TLPstartup>\n'   nodebug\n!end\n\n'===========================================\n'== Add some synonyms used in the library ==\n'===========================================\n\n!addto synonyms\n   back; back from; out of =from\n   inside; in to; in =into\n   look into = examine   \n   put =drop\n   don; drop on = wear\n   take off; remove = unwear\n   shut = close\n   talk = speak\n!end\n\n'==================================================\n'== A couple of dummy rooms, used by the library ==\n'==================================================\n\ndefine room <TLRcontainers>\n\nend define\n\ndefine room <TLRcontents>\n\nend define\n\n'================\n'== PROCEDURES ==\n'================\n\n'===========================================================\n'== Configure the library for use - called in startscript ==\n'===========================================================\n\ndefine procedure <TLPstartup>\n\n   set string <TLSplayerRoom;>\n   set string <TLScontentList;>\n   set string <TLStemp;>\n   set string <TLSrealName;>\n   set string <TLSlist;>\n   set string <TLStempName;>\n   set string <TLMnotOpenable;Try as you might, you find that __\n              is just not possible.>\n   set string <TLMnotClosable;Try as you might, you find that __\n              is just not possible.>\n   set string <TLMtaken;Taken.>\n   set string <TLMalreadyTaken;You have already got>\n   set string <TLMdontHaveDObj;You don't seem to have that.>\n   set string <TLMnoNeed;You realise there is no need __\n               to do that and change your mind.>\n   set string <TLSthisObj;>\n   set string <TLSobjAlias;>\n   set string <TLSindescription;>\n   set string <TLSalias;>\n   set string <TLSobjList;>\n   set string <TLScharList;>\n   set numeric <TLNcomma;0>\n   set numeric <TLNcount;0>\n   set numeric <TLNlength;0>\n   set numeric <TLNsizeLimit;0>\n   set numeric <TLNweightLimit;0>\n   set numeric <TLNheadCovered;0>\n   set numeric <TLNhandsCovered;0>\n   set numeric <TLNfeetCovered;0>\n   set numeric <TLNtopCovered;0>\n   set numeric <TLNbotCovered;0>\n   set numeric <TLNtempCovered;0>\n   set numeric <TLNwearable;0>\n   set numeric <TLNlengthOf;0>\n'***\n' TLNplayerSex' should be set to 1 for a male player, 2 for a female.\n'***\n   set numeric <TLNplayerSex;1>\n\n   for each object in game {\n      if type <#quest.thing#;TLTcontainable> then {\n      if (#(quest.thing):isIn#<>nil) then {\n         move <#quest.thing#;TLRcontainers>\n        }\n    }\n    if type <#quest.thing#;TLTclothing> then {\n    property <#quest.thing#;displayname=$displayname(#quest.thing#)$>\n      if property <#quest.thing#;worn> then {\n          move <#quest.thing#;inventory>\n         property <#quest.thing#;alias=#(quest.thing):displayname# [worn]>\n         set <TLNtopCovered;%TLNtopCovered% + #(quest.thing):topcover#>\n         set <TLNheadCovered;%TLNheadCovered% + #(quest.thing):headcover#>\n         set <TLNhandsCovered;%TLNhandsCovered% + #(quest.thing):handscover#>\n         set <TLNfeetCovered;%TLNfeetCovered% + #(quest.thing):feetcover#>\n            set <TLNbotCovered;%TLNbotCovered% + #(quest.thing):botcover#>\n          }\n   }\n   }\n   \nend define\n\n'==================================\n'== Alternative room description ==\n'==================================\n\ndefine procedure <TLProomDescription>\n   set <TLSindescription;$gettag(#quest.currentroom#;indescription)$>\n   set <TLSalias;$gettag(#quest.currentroom#;alias)$>\n   if is <#TLSindescription#;> then {\n     if is <#TLSalias#;> then {\n         msg <You are in $gettag(#quest.currentroom#;prefix)$ |b|cl#quest.currentroom#.|cb|xb|xn>\n     }\n     else {\n         msg <You are in $gettag(#quest.currentroom#;prefix)$ |b|cl#TLSalias#.|cb|xb|xn>\n     }\n   }\n   else {\n     if is <#TLSalias#;> then {\n         msg <#TLSindescription# $gettag(#quest.currentroom#;prefix)$ |b|cl#quest.currentroom#.|cb|xb|xn>\n     }\n     else {\n         msg <#TLSindescription# $gettag(#quest.currentroom#;prefix)$ |b|cl#TLSalias#.|cb|xb|xn>\n     }\n   }\n   msg <|n#quest.lookdesc#|n>\n   set <TLSobjList;>\n   set <TLScharList;>\n   for each object in <#quest.currentroom#> {\n      set <TLSthisObj;#quest.thing#>\n      if property <#quest.thing#;invisible> or property <#quest.thing#;hidden> then {\n      }\n      else {\n         if type <#quest.thing#;TLTactor> then {\n\t\t\tif property <#quest.thing#;named> then {\n            \tset <TLScharList;#TLScharList# |b$capfirst(#@quest.thing#)$|xb, >\n\t\t\t}\n\t\t\telse set <TLScharList;#TLScharList# #(quest.thing):prefix# |b#@quest.thing#|xb, >\n         }\n         else{\n            set <TLSobjList;#TLSobjList# #(quest.thing):prefix#>\n            set <TLSobjList;#TLSobjList# |b#@quest.thing#|xb, >\n         }\n      }\n   }\n   if ($lengthof(#TLScharList#)$ >0) then {\n      set <TLNlengthOf;$lengthof(#TLScharList#)$ - 1>\n      set <TLScharList;$left(#TLScharList#;%TLNlengthOf%)$>\n      set <TLScharList;$TLFcontentFormat(#TLScharList#)$>\n         if ($instr(#TLScharList#;_and_)$ >0) then { \n            msg <#TLScharList# are here.|n>    \n         }\n         else {\n            msg <#TLScharList# is here.|n>        \n         }\n      }\n      if ($lengthof(#TLSobjList#)$ >0) then {     \n         set <TLNlengthOf;$lengthof(#TLSobjList#)$ - 1>\n         set <TLSobjList;$left(#TLSobjList#;%TLNlengthOf%)$>\n         set <TLSobjList;$TLFcontentFormat(#TLSobjList#)$>\n         if ($instr(#TLSobjList#;_and_)$ >0) then { \n            msg <You can see #TLSobjList# here.|n>     \n         }\n         else {\n            msg <You can see #TLSobjList# here.|n>         \n         }        \n      }\n      if not is <#quest.doorways.dirs#;> then {\n         msg <You can move #quest.doorways.dirs#.>\n      }\n      if not is <#quest.doorways.places#;> then {\n         msg <You can go to #quest.doorways.places#.>\n      }\n      if not is <#quest.doorways.out#;> then {\n         msg <You can go out to |b#quest.doorways.out.display#|xb.>\n      }     \nend define\n\n\n'================================================================\n'== Override the inbuilt LOOK function - need extra capability ==\n'================================================================\n\ndefine procedure <TLPlook>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n         if ($locationof(#TLSdObj#)$=TLRcontents) then {\n            move <#TLSdObj#;#quest.currentroom#>\n         }\n         exec <look at #TLSdObj#;normal>\n         if type <#TLSdObj#;TLTcontainable> then {\n            if (#(TLSdObj):isIn#<>nil) then {\n               move <#TLSdObj#;TLRContents>\n            }\n         }\n      }\n      else exec <look #TLSdObj#;normal>\n   do <TLPemptyContents>\nend define\n\n'===================================================================\n'== Override the inbuilt EXAMINE function - need extra capability ==\n'===================================================================\n\ndefine procedure <TLPexamine>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n         if ($locationof(#TLSdObj#)$=TLRcontents) then {\n            move <#TLSdObj#;#quest.currentroom#>\n         }         \n         do <TLPexamineContainer>\n         if type <#TLSdObj#;TLTcontainable> then {\n            if (#(TLSdObj):isIn#<>nil) then {\n               move <#TLSdObj#;TLRContents>\n            }\n         }         \n      }\n   else do <TLPexamineContainer>\n   do <TLPemptyContents>\nend define\n\n'================================================================\n'== Tests examined object & calls contents action where needed ==\n'================================================================\n\ndefine procedure <TLPexamineContainer>\n   exec <x #TLSdObj#;normal>\n   if type <#TLSdObj#;TLTcontainer> then {\n     doaction <#TLSdObj#;contents>\n   }\nend define\n\n'================================================================\n'== Override the inbuilt TAKE function - need extra capability ==\n'================================================================\n\ndefine procedure <TLPtake>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n         if ($locationof(#TLSdObj#)$<>inventory) then  {\n            move <#TLSdObj#;#quest.currentroom#>\n            if property <#TLSdObj#;takeable> then {\n               doaction <#TLSdObj#;take>\n               if property <#TLSdObj#;isIn> then {\n                  property <#TLSdObj#;isIn=nil>\n               }\n            }\n            else {\n            \tmsg <#(TLSdObj):noTake#>\n                if property <#TLSdObj#;isIn> then {\n                  \tif (#(TLSdObj):isIn#<> nil) then {\n \t\t\t\t\t\tmove <#TLSdObj#;TLRcontents>\n\t\t\t\t\t}\n                }\n\t\t\t}\n         }\n         else msg <#TLMalreadyTaken# #(TLSdObj):article#.>\n      }\n      else doaction <#TLSdObj#;take>\n   do <TLPemptyContents>\nend define\n\n'================================================================\n'== Override the inbuilt DROP function - need extra capability ==\n'================================================================\n\ndefine procedure <TLPdrop>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n         if property <#TLSdObj#;worn> then {\n            msg <You'll have to take #(TLSdObj):article# off first.>\n         }\n         else exec <drop #TLSTemp#;normal>\n      }\n      else exec <drop #TLStemp#;normal>\nend define\n\n\n'================================================================\n'== Override the inbuilt GIVE function - need extra capability ==\n'================================================================\n\ndefine procedure <TLPgive>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n   set <TLSiObj;$TLFgetObjectName(#TLSiObj#)$>\n   if (#TLSdObj#=!) then {\n      exec <look at #TLSdObj#;normal>\n   }\n   else {\n      if (#TLSIObj#=!) then {\n         exec <look at #TLSIObj#;normal>\n      }\n      else {\n       if property <#TLSdObj#;worn> then {\n         msg <You'll have to take #(TLSdObj):article# off first.>     \n       }\n       else {\n         do <TLPcheckGive>\n      }\n      }\n   }\n   do <TLPemptyContents>\nend define\n\n'=============================================================\n'== Tests objects & redirects or runs standard give routine == \n'=============================================================\n\ndefine procedure <TLPcheckGive>\n   if type <#TLSdObj#;TLTcontainable> or type <#TLSiObj#;TLTcontainer> then {\n      do <TLPcheckIObj>\n   }\n   else {\n      exec <give #TLSdObj# to #TLSiObj#;normal>\n   }\nend define\n\n\n'=========================================================\n'== 3 utility procedures - manipulate contained objects ==\n'=========================================================\n\ndefine procedure <TLPfillContents>\n   for each object in <TLRcontainers> {\n      if got <#(quest.thing):isIn#> or here <#(quest.thing):isIn#> then {\n         if not property <#(quest.thing):isIn#;closed> then {\n            move <#quest.thing#;TLRcontents>\n         }\n      }\n   }   \nend define\n\ndefine procedure <TLPvalidContents>\n   for each object in <TLRcontents> {\n      if (#(quest.thing):isIn# <> #TLSdobj# ) then {\n         move <#quest.thing#;TLRcontainers>\n      }\n   } \nend define\n\ndefine procedure <TLPemptyContents>\n   for each object in <TLRcontents> {\n      move <#quest.thing#;TLRcontainers>\n   } \nend define\n\n'===================================\n'== Handle the added OPEN command ==\n'===================================\n\ndefine procedure <TLPopen>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n       do <TLPopenObj>\n      }\n      else exec <look #TLSdObj#;normal>\n   do <TLPemptyContents>   \nend define\n\n'==================================================================\n'== Tests open-ed object & calls open action or denial as needed ==\n'==================================================================\n\ndefine procedure <TLPopenObj>\n   if type <#TLSdObj#;TLTclosable> then {\n     doaction <#TLSdObj#;open>\n   }\n   else msg <#TLMnotOpenable#>\nend define\n\n'====================================\n'== Handle the added CLOSE command ==\n'====================================\n\ndefine procedure <TLPclose>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n       do <TLPcloseObj>\n      }\n      else exec <look #TLSdObj#;normal>\n   do <TLPemptyContents>   \nend define\n\n'===================================================================\n'== Tests close-d object & calls close action or denial as needed ==\n'===================================================================\n\ndefine procedure <TLPcloseObj>\n   if type <#TLSdObj#;TLTclosable> then {\n     doaction <#TLSdObj#;close>\n   }\n   else msg <#TLMnotClosable#>\nend define\n\n'=====================================\n'== Handle the added PUT IN command ==\n'=====================================\n\ndefine procedure <TLPputIn>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n'   set <TLSiObj;$TLFgetObjectName(#TLSiObj#)$>\n   set <TLSiObj;$TLFgetObjectName(#TLSiObj#)$>   \n      if (#TLSdObj# <> !) then {\n         do <TLPcheckIObj>\n      }\n      else msg <#TLMdontHaveDObj#>\n   do <TLPemptyContents>\nend define\n\n'===========================================\n'== Test the indirect object is available ==\n'===========================================\n\ndefine procedure <TLPcheckIObj>\n   if (#TLSiObj# <> !) then {\n      do <TLPcheckContainer>\n   }\n   else {\n   \tif property <#TLSiObj#;named> then msg <$displayname(TLSiObj)$ isn't here.>\n\telse msg <The $displayname(TLSiObj)$ isn't here.>\n   }\nend define\n\n'======================================\n'== Test for container & containable ==\n'======================================\n\ndefine procedure <TLPcheckContainer>\n   if not type <#TLSiObj#;TLTcontainer> then {\n      msg <You can't put things in $TLFnamed(#TLSiObj#)$.>\n   }\n   else {\n      if property <#TLSiObj#;closed> then {\n         msg <You'll have to open $TLFnamed(#TLSiObj#)$...>\n      }\n      else {\n         if not type <#TLSdObj#;TLTcontainable> then {\n            msg <You can't put $TLFnamed(#TLSdObj#)$ in anything.>\n         }\n         else do <TLPcontainerLimits>\n      }\n   }\nend define\n\n'======================================\n'== Both legal objects, check limits ==\n'======================================\ndefine procedure <TLPcontainerLimits>\n   if (#(TLSdObj):isIn#<>#TLSiObj#) then { \n      set <TLNsizeLimit;$TLFsizeHeld(#TLSiObj#)$>\n      set <TLNweightLimit;$TLFweightHeld(#TLSiObj#)$>\n      set <TLNsizeLimit;#(TLSiObj):sizeLimit# - %TLNsizeLimit%>\n      set <TLNweightLimit;#(TLSiObj):weightLimit# - %TLNweightLimit%>\n      if (%TLNsizeLimit% < #(TLSdObj):size#) then {\n         msg <#(TLSdObj):tooBig#>\n      }\n      else {\n         if (%TLNweightLimit% < #(TLSdObj):weight#) then {\n            msg <#(TLSdObj):tooHeavy#>\n         }\n         else {\n            move <#TLSdObj#;TLRcontainers>\n            property <#TLSdOBJ#;isIn=#TLSiObj#>\n\t\t\t\tdoaction <#TLSdObj#;contained>\n         }\n      }  \n   }\n   else msg <#TLMnoNeed#>\nend define\n\n'===================================\n'== The 'read' command procedure. ==\n'===================================\n\ndefine procedure <TLPread>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n         if type <#TLSdObj#;TLTreadable> then {\n            doaction <#TLSdObj#;read>\n         }\n         else msg <There's nothing to read!>\n      }\n      else exec <look #TLSdObj#;normal>\n   do <TLPemptyContents> \nend define\n\n'===================================\n'== The 'wear' command procedure. ==\n'===================================\n\ndefine procedure <TLPwear>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n         if action <#TLSdObj#;wear> then {\n            if (#(TLSdObj):sex# = %TLNplayerSex%) then {\n               do <TLPcheckWear>\n            }\n            else {\n               msg <On second thoughts you decide that the |xn>\n               msg <#TLSdObj# |xn>\n               if (%TLNplayerSex% = 1) then {\n                  msg <won't suit a man like you at all.>\n               }\n               else msg <won't suit a woman like you at all.>\n            }\n         }\n         else {\n            if (#(TLSdObj):noWear#= default) then {\n            \tmsg <You cannot wear $TLFnamed(#TLSdObj#)$!>\n\t\t\t}\n\t\t\telse {\n\t\t\t\tmsg <#(TLSdObj):noWear#>\n\t\t\t}\n         }\n      }\n      else msg <$TLFnamed(#TLStemp#)$ isn't here.>\n   do <TLPemptyContents>\nend define\n\n'========================================================================\n'== The 'checkWear' procedure - this checks the sense of wear commands ==\n'========================================================================\n\ndefine procedure <TLPcheckWear>\n   set <TLNwearable;0>\n   if (#(TLSdObj):topcover# <> 0) and (#(TLSdObj):topcover# <= %TLNtopCovered%) then {\n      set <TLNwearable;%TLNwearable% + 1>\n   }\n   if (#(TLSdObj):headcover# <> 0) and (#(TLSdObj):headcover# <= %TLNheadCovered%) then {\n      set <TLNwearable;%TLNwearable% + 1>\n   }\n   if (#(TLSdObj):feetcover# <> 0) and (#(TLSdObj):feetcover# <= %TLNfeetCovered%) then {\n      set <TLNwearable;%TLNwearable% + 1>\n   }\n   if (#(TLSdObj):handscover# <> 0) and (#(TLSdObj):handscover# <= %TLNhandsCovered%) then {\n      set <TLNwearable;%TLNwearable% + 1>\n   }\n' - disallow for coats (don't affect ability to put on lower torso clothes)\n   set <TLNtempCovered;%TLNbotCovered%>\n   if (%TLNtempCovered% > 63) and (#(TLSdObj):botcover# < 33) then {\n      set <TLNtempCovered;%TLNtempCovered% - 64>\n   }\n' - disallow for skirts and dresses (like coats!)\n   if (%TLNtempCovered% > 31) and (#(TLSdObj):botcover# < 16) and __\n   (#(TLSdObj):botcover# <> 4) then {\n      set <TLNtempCovered;%TLNtempCovered% - 32>\n   }\n' - disallow wearing of skirts/dresses and trousers simultaneously\n   if (%TLNtempCovered% > 15) then {\n      set <TLNtempCovered;%TLNtempCovered% + 16> \n   }\n   if (#(TLSdObj):botcover# <> 0) and (#(TLSdObj):botcover# <= %TLNtempCovered%) then {\n      set <TLNwearable;%TLNwearable% + 1>\n   }\n   if (%TLNwearable% =0) then {\n      doaction <#TLSdObj#;wear>\n      property <#TLSdObj#; worn>\n      set <TLNtopCovered;%TLNtopCovered% + #(TLSdObj):topcover#>\n      set <TLNheadCovered;%TLNheadCovered% + #(TLSdObj):headcover#>\n      set <TLNhandsCovered;%TLNhandsCovered% + #(TLSdObj):handscover#>\n      set <TLNfeetCovered;%TLNfeetCovered% + #(TLSdObj):feetcover#>\n      set <TLNbotCovered;%TLNbotCovered% + #(TLSdObj):botcover#>\n   }\n   else {\n      msg <Given what you are already wearing - that makes no sense at all.>\n   }\nend define\n\n'=====================================\n'== The 'unwear' command procedure. ==\n'=====================================\n\ndefine procedure <TLPunWear>\n   do <TLPfillContents>\n   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>\n      if (#TLSdObj# <> !) then {\n         if property <#TLSdObj#; worn> then {\n            if action <#TLSdObj#;unwear> then {\n               do <TLPcheckUnWear>\n            }\n            else msg <You can't do that.>\n         }\n         else msg <You can't do that.>\n      }\n      else msg <You aren't wearing $TLFnamed(#TLStemp#)$.>\n   do <TLPemptyContents>\nend define\n\n'==================================\n'== The 'CheckUnwear' procedure. ==\n'==================================\n\ndefine procedure <TLPcheckUnWear>\n   set <TLNwearable;0>\n   set <TLNtempCovered; %TLNtopCovered% /2>\n   if (#(TLSdObj):topcover# <> 0) and (#(TLSdObj):topcover# <= %TLNtempCovered%) then {\n      set <TLNwearable;%TLNwearable% + 1>\n   }\n   set <TLNtempCovered; %TLNheadCovered% /2>\n   if (#(TLSdObj):headcover# <> 0) and (#(TLSdObj):headcover# <= %TLNtempCovered%) then {\n      set <TLNwearable;%TLNwearable% + 2>\n   }\n   set <TLNtempCovered; %TLNfeetCovered% /2>\n   if (#(TLSdObj):feetcover# <> 0) and (#(TLSdObj):feetcover# <= %TLNtempCovered%) then {\n      set <TLNwearable;%TLNwearable% + 4>\n   }\n   set <TLNtempCovered; %TLNhandsCovered% /2>   \n      if (#(TLSdObj):handscover# <> 0) and (#(TLSdObj):handscover# <= %TLNtempCovered%) then {\n         set <TLNwearable;%TLNwearable% + 8>\n   }\n' - disallow for coats (don't affect ability to take off lower torso clothes)\n   set <TLNtempCovered;%TLNbotCovered%>\n   if (%TLNtempCovered% > 63) then {\n      set <TLNtempCovered;%TLNtempCovered% - 64>\n   }\n' - disallow for skirts and dresses (like coats!)\n   if (%TLNtempCovered% > 31) and (#(TLSdObj):botcover# <> 4) then {\n      set <TLNtempCovered;%TLNtempCcovered% - 32>\n   }\n   set <TLNtempCovered;%TLNtempCovered% /2>\n   if (#(TLSdObj):botcover# <> 0) and (#(TLSdObj):botcover# <= %TLNtempCovered%) then {\n      set <TLNwearable;%TLNwearable% + 16>\n   }\n   if (%TLNwearable% =0) then {\n      doaction <#TLSdObj#;unwear>\n      property <#TLSdObj#; not worn>\n      set <TLNtopCovered;%TLNtopCovered% - #(TLSdObj):topcover#>\n      set <TLNheadCovered;%TLNheadCovered% - #(TLSdObj):headcover#>\n      set <TLNhandsCovered;%TLNhandsCovered% - #(TLSdObj):handscover#>\n      set <TLNfeetCovered;%TLNfeetCovered% - #(TLSdObj):feetcover#>\n      set <TLNbotCovered;%TLNbotCovered% - #(TLSdObj):botcover#>\n   }\n   else {\n      msg <Given what you are wearing, that isn't possible.>\n   }\nend define\n\n'===============\n'== FUNCTIONS ==\n'===============\n\n'========================================================\n'== Returns object real name, checking three locations ==\n'========================================================\n\ndefine function <TLFgetObjectName>\n   set <TLStemp;$parameter(1)$>\n   set <TLSrealName;$getobjectname(#TLStemp#)$>\n'msg <DEBUG #TLStemp# #TLSrealName#>\n   if (#TLSrealName#=!) then {\n      set <TLSrealName;$getobjectname(#TLStemp#;TLRcontents)$>\n   }\n   if (#TLSrealName#=!) then {\n      set <TLSrealName;$getobjectname(#TLStemp# [worn])$>\n        if (#TLSrealName#=!) then {\n         set <TLSrealName;$getobjectname(#TLStemp# [worn];TLRcontents)$>\n      }\n   }\n   return <#TLSrealName#>\nend define\n\n'=====================================================\n'== Replaces last comma in parsed string with \"and\" ==\n'=====================================================\n\ndefine function <TLFcontentFormat>\n   set <TLStemp;$parameter(1)$>\n   set <TLNlength;$lengthof(#TLStemp#)$>\n   set <TLNcomma;0>\n   for <TLNcount; 1; %TLNlength%; 1> {\n      if ($mid(#TLStemp#;%TLNcount%;1)$ =,) then {\n         set <TLNcomma;%TLNcount%>\n      }\n   }\n   if (%TLNcomma% <>0) then {\n      set <TLNcomma;%TLNcomma%-1>\n      set <TLSlist;$left(#TLStemp#;%TLNcomma%)$ and>\n      set <TLNcomma;%TLNcomma%+2>\n      set <TLSlist;#TLSlist#$mid(#TLStemp#;%TLNcomma%)$>     \n   }\n   else set <TLSlist;#TLStemp#>\n   return <#TLSlist#>\nend define\n\n'=================================================\n'== Returns sizes / weights held in a container ==\n'=================================================\n\ndefine function <TLFsizeHeld>\n   set <TLNsizeLimit;0>\n   for each object in <TLRcontents> {\n      if (#(quest.thing):isIn#=#TLSiObj#) then {\n         set <TLNsizeLimit;%TLNsizeLimit% + #(quest.thing):size#>\n      }     \n   }\n   return <%TLNsizeLimit%>\nend define\n\ndefine function <TLFweightHeld>\n   set <TLNweightLimit;0>\n   for each object in <TLRcontents> {\n      if (#(quest.thing):isIn#=#TLSiObj#) then {\n         set <TLNweightLimit;%TLNweightLimit% + #(quest.thing):weight#>\n      }     \n   }\n   return <%TLNweightLimit%>\nend define\n\n'========================================================\n'== Returns proper name or 'the object' as appropriate ==\n'========================================================\n\ndefine function <TLFnamed>\n   set <TLStempName;$parameter(1)$>\n   if property <#TLStempName#;named> then {\n     set <TLStempName;$capfirst(#TLStempName#)$>\n   }\n   else set <TLStempName;the #TLStempName#>\n   return <#TLStempName#>\nend define\n\n'======================\n'== TYPE DEFINITIONS ==\n'======================\n\ndefine type <TLTactor>\n   listHeader = He is carrying\n   noSpeak = He says nothing.\n   article = he\n   displaytype = person\n   named\n   action <speak> {\n      set <TLSthisObj;$thisobject$> \n      msg <#(TLSthisObj):noSpeak#>\n   }\n   \nend define\n\n'=====================================================================\n'== The TLTcontainer type: object that you can put other objects in ==\n'=====================================================================\n\ndefine type <TLTcontainer>\n   listHeader=It contains\n   sizeLimit=100\n   weightLimit=100\n      \n   action <contents> {\n   do <TLPvalidContents>\n      outputoff\n      set <TLSplayerRoom;#quest.currentroom#>\n      goto <TLRcontents>\n      set <TLScontentList;#quest.formatobjects#>         \n      goto <#TLSplayerRoom#>\n      outputon\n      set <TLSthisObj;$thisobject$>\n      if type <#TLSthisObj#;TLTClosable> then {\n         if property <#TLSthisObj#;closed> then {\n            set <TLScontentList;>\n            msg <#(TLSthisObj):closedDesc#>\n         }\n      }\n      if ($lengthof(#TLScontentList#)$ > 0) then {\n         msg <#(TLSdObj):listHeader# #TLScontentList#.>\n      }\n   }\nend define\n\n'=========================================================================\n'== The TLTcontainable type: object that can be put into a TLTcontainer == \n'=========================================================================\n\ndefine type <TLTcontainable>\n   isIn=nil\n   size=25\n   weight=25\n   tooBig=It is too big to fit.\n   tooHeavy=It is too heavy for that.\n   action <contained> msg <O.K.>\nend define\n\n'=================================================================\n'== The TLTclosable type: object that can be opened and closed == \n'=================================================================\n\ndefine type <TLTclosable>\n   closed\n   closedDesc=(It is closed, you cannot see inside.)\n   isClosedDesc=It is already closed.\n   isOpenedDesc=It is already open.\n   closingDesc=You close it.\n   openingDesc=You open it.\n   article=it\n\n   action <open> {\n      set <TLSthisObj;$thisobject$>\n      if property <#TLSthisObj#;closed> then {\n         doaction <#TLSthisObj#;opened>\n         property <#TLSthisObj#;not closed>\n      }\n      else {\n         msg <#(TLSthisObj):isOpenedDesc#>\n      }\n   }\n\n   action <close> {\n      set <TLSthisObj;$thisobject$>\n      if property <#TLSthisObj#;closed> then {\n         msg <#(TLSthisObj):isClosedDesc#>\n      }\n      else {\n         doaction <#TLSthisObj#;closed>\n         property <#TLSthisObj#;closed>\n      }\n   }\n\n   \taction <opened> {\n    \tmsg <#(TLSthisObj):openingDesc#>\n   \t}\n\n   \taction <closed> {\n   \t\tmsg <#(TLSthisObj):closingDesc#>\n   \t}\n\nend define\n\n'=============================================\n'== The TLTreadable type: a readable object ==\n'=============================================\n\ndefine type <TLTreadable>\n   readmessage = You start to read but it is so incredibly dull you decide not to bother.\n   action <read> {\n   set <TLSdObj;$thisobject$>\n   msg <#(TLSdObj):readmessage#>\n   }\nend define\n\n\n'====================================================\n'== The TLTobject type: a visible, takeable object ==\n'====================================================\n\ndefine type <TLTobject>\n   takeable\n   action <take> {\n      set <TLSthisObj;$thisobject$>\n      move <#TLSthisObj#;inventory>\n      msg <#TLMtaken#>\n      if property <#TLSthisObj#;TLTcontainable> then {\n         property <#TLSthisObj#;isIn=nil>\n      }\n   }\nend define\n\n'=================================================\n'== The TLTscenery type: fixed, unlisted object ==\n'=================================================\n\ndefine type <TLTscenery>\n   invisible\nend define\n\n'============================================================\n'== The default type: common functionality for all objects ==\n'============================================================\n\n!addto type <default>\n'define type <default>\n   prefix=a\n      article=it\n      displaytype=object\n      noTake = Taking that would serve no useful purpose.\n\t  noWear = default\n'end define\n!end\n\n\n'================================================================================\n'== This clothing type is not used directly but inherited by specific clothes. ==\n'================================================================================\n \ndefine type <TLTclothing>\n   type <TLTobject>\n   headcover = 0\n   handscover = 0\n   feetcover = 0\n   topcover = 0\n   botcover = 0\n   sex = 1\n   article = it\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   properties <displayname=$thisobject$>\n      action <wear> {\n         set <TLSdObj;$thisobject$>\n         msg <#(TLSdObj):wearmessage#>\n         property <#TLSdObj#;alias=#(TLSdObj):displayname# [worn]>\n         move <#TLSdObj#;inventory>\n      }\n      action <unwear> {\n         set <TLSdObj;$thisobject$>\n         msg <#(TLSdObj):unwearmessage#>\n         property <#TLSdObj#;alias=#(TLSdObj):displayname#>\n      }   \nend define  \n\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n' * This lib defines 15 specific clothing objects, and most of these can be used    *\n' * to good effect for other garments - the defined objects are listed together     *\n' * with other garments that work similarly for game purposes.                      *\n' *                                                                                 *\n' * DEFINED GARMENT : ALSO USABLE FOR THESE GARMENTS                                *\n' * hat             : any headwear                                                  *\n' * gloves          : any handwear                                                  *\n' * shoes           : boots, outer footwear generally                               *\n' * socks           : stockings                                                     *\n' * tights          : pantie hose                                                   *\n' * undies          : panties, briefs - lower portion underwear generally           *\n' * teddy           : uhm.. any underthing that covers like a teddy!                *\n' * trousers        : jeans, shorts (not the underwear variety)                     *\n' * dress           : coverall                                                      *\n' * skirt           : kilt maybe?                                                   *\n' * vest            : bra, other 'top only' undergarment                            *\n' * shirt           : blouse, T-Shirt etc.                                          *\n' * sweater         : pullover, sweatshirt - '2nd layer' top garment                *\n' * jacket          : fleece, parka, anorak, short coat of whatever type            *\n' * coat            : any long length outermost garment like a coat                 *\n' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *\n\ndefine type <TLTshoes>\n   type <TLTclothing>\n   feetcover = 4\n   wearmessage = You put them on.\n   unwearmessage = You take them off.\n   article = them\n   prefix=a pair of\nend define\n\ndefine type <TLTsocks>\n   type <TLTclothing>\n   feetcover = 2\n   wearmessage = You put them on.\n   unwearmessage = You take them off.\n   article = them\n   prefix=a pair of\nend define\n\ndefine type <TLTtights>\n   type <TLTclothing>\n   feetcover = 2\n   botcover = 8\n   article = them\n   wearmessage = You put them on.\n   unwearmessage = You take them off.\n   prefix=a pair of\nend define\n\ndefine type <TLThat>\n   type <TLTclothing>\n   headcover = 2\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\nend define\n\ndefine type <TLTgloves>\n   type <TLTclothing>\n   handscover = 2\n   wearmessage = You put them on.\n   unwearmessage = You take them off.\n   article = them\n   prefix=a pair of\nend define\n\ndefine type <TLTvest>\n   type <TLTclothing>\n   topcover = 2\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\nend define\n\ndefine type <TLTshirt>\n   type <TLTclothing>\n   topcover = 8\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\nend define\n\ndefine type <TLTteddy>\n   type <TLTclothing>\n   topcover = 4\n   botcover = 4\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\n end define\n\ndefine type <TLTundies>\n   type <TLTclothing>\n   botcover = 2\n   wearmessage = You put them on.\n   unwearmessage = You take them off.\n   article = them\n   prefix=a pair of\nend define\n\ndefine type <TLTdress>\n   type <TLTclothing>\n   topcover = 8\n   botcover = 32\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\nend define\n\ndefine type <TLTskirt>\n   type <TLTclothing>\n   botcover = 32\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\nend define\n\ndefine type <TLTtrousers>\n   type <TLTclothing>\n   botcover = 16\n   wearmessage = You put them on.\n   unwearmessage = You take them off.\n   article = them\n   prefix=a pair of\nend define\n\ndefine type <TLTsweater>\n   type <TLTclothing>\n   topcover = 16\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\nend define\n\ndefine type <TLTjacket>\n   type <TLTclothing>\n   topcover = 32\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\nend define\n\ndefine type <TLTcoat>\n   type <TLTclothing>\n   topcover = 64\n   botcover = 64\n   wearmessage = You put it on.\n   unwearmessage = You take it off.\n   prefix=a\nend define\n\n\n'==========================\n'== INTERFACE FOR Q.D.K. ==\n'==========================\n\n!QDK\n\n'script <MaDbRiTs Types Library>\n'   command <Initialise the library (use in startscript); TLPstartup>\n'      display <Initialise MaDbRiTs Types Library.>\n\nobject <Basics>\n   type <Regular object; TLTobject>\n   type <Scenery (not takeable or described) object; TLTscenery> \n   property <Make regular object not takeable;not takeable>\n   property <Not takeable msg;noTake;text>\n   property <Not wearable msg;noWear;text>\n   \nobject <Readable>\n   type <Readable object;TLTreadable>\n   property <Message if read;readMessage;text>\n   action <Action if read;read>\n\nobject <Containers>\n   type <Container object (can have things put in it); TLTcontainer>\n   property <Header for listing;listHeader;text> \n   property <Size Limit;sizeLimit;text>\n   property <Weight Limit;weightLimit;text>\n   \n   type <Containable object (can be put in things); TLTcontainable>  \n   property <Size;size;text>\n   property <Weight;weight;text>\n   property <Start game inside;isIn;objects>\n   property <Msg if Too Big;tooBig;text>\n   property <Msg if Too Heavy;tooHeavy;text>\n\taction <Action when contained;contained>\n\nobject <Actor>\n   type <Actor can carry things;TLTcontainer>\n   property <Header for listing;listHeader;text> \n   property <Size Limit;sizeLimit;text>\n   property <Weight Limit;weightLimit;text>\n   type <Actor; TLTactor>\n   property <Actor not named. (e.g. 'the sailor' not 'Jack');not named>\n   property <Default speak reply;noSpeak;text>\n   action <Script if spoken to;speak>\n   \nobject <Closables>   \n   type <Closable object     NOTE Following properties all have useful defaults;TLTclosable>\n   property <Start open?;not closed>\n   property <Closed text (container);closedDesc;text>\n   property <Is closed description;isClosedDesc;text>   \n   property <Closing description;closingDesc;text>   \n   property <Is open description;isOpenDesc;text>   \n   property <Opening description;openingDesc;text>\n   action <Script when opened;opened>\n   action <Script when closed;closed>\n\nobject <Clothing 1>  \n   type <Sweater;TLTsweater>\n   type <Shirt;TLTshirt>\n   type <Vest (top half underwear / bra etc.);TLTvest>\n   type <Teddy (or 1 piece swimsuit etc.);TLTteddy>\n   type <Underwear (shorts or briefs);TLTundies>\n   type <Socks;TLTsocks>\n   type <Tights (a.k.a. Panty Hose);TLTtights>\n   type <Dress;TLTdress>\n   type <Skirt;TLTskirt>\n   type <Trousers;TLTtrousers>\n   type <Shoes;TLTshoes>\n   type <Hat           NOTE Clothing continues on next tab!;TLThat>   \n   \n   \nobject <Clothing 2>\n   type <Gloves;TLTgloves>\n   type <Jacket (short coat);TLTjacket>\n   type <Coat (long coat)       NOTE Properties below have defaults! See the manual;TLTcoat>  \n   property <Msg when put on;wearmessage;text>\n   property <Msg when taken off;unwearmessage;text>   \n   property <Head cover value;headcover;text>\n   property <Hands cover value;handscover;text>\n   property <Feet cover value;feetcover;text>\n   property <Top cover value;topcover;text>\n   property <Bottom cover value;botcover;text>\n   property <Sex;sex;text>\n   property <Start game worn?;worn>\n!end\n";
libraries["net.lib"] = "!library\n!asl-version <350>\n!name <QuestNet Standard Library>\n!version <1.0>\n!author <Alex Warren>\n! This library adds useful additional QuestNet functions. We recommend you include this library in all QuestNet (multiplayer) games.\n\n' NET.LIB v1.0\n' for QuestNet Server 3.5\n' Copyright \u00A9 2004 Axe Software. Please do not modify this library.\n\n!QDK\n\n\tobject <QuestNet>\n\t\ttype <This object can be &given to other players; giveable>\n\n!end\n\n\ndefine type <giveable>\n        action <give to anything> {\n                if ( #quest.give.object.name# = player%userid% ) then {\n                        msg <It is silly to give things to yourself!>\n                }\n                else {\n                        if property <#quest.give.object.name#; netplayer> then {\n                                move <$thisobject$; #quest.give.object.name#>\n                                msg <You give $name(#quest.give.object.name#)$ the $thisobjectname$.>\n                                msgto <#quest.give.object.name#; |b$name(%userid%)$|xb has given you a |b$thisobjectname$|xb.>\n                                if action <$thisobject$; gain> then {\n                                \twith <#quest.give.object.name#> {\n                                \t\tdoaction <$thisobject$; gain>\n                                \t}\n                                }\n                                if action <$thisobject$; lose> then {\n                                \tdoaction <$thisobject$; lose>\n                                }\n                        }\n                        else {\n                                msg <The $displayname(#quest.give.object.name#)$ doesn't want that.>\n                        }\n                }\n        }\nend define\n";
//# sourceMappingURL=asl4.js.map