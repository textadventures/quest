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
    }
    return DefineBlock;
})();
var Context = (function () {
    function Context() {
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
    }
    return ItemType;
})();
var Collectable = (function () {
    function Collectable() {
    }
    return Collectable;
})();
var PropertyType = (function () {
    function PropertyType() {
    }
    return PropertyType;
})();
var ActionType = (function () {
    function ActionType() {
    }
    return ActionType;
})();
var UseDataType = (function () {
    function UseDataType() {
    }
    return UseDataType;
})();
var GiveDataType = (function () {
    function GiveDataType() {
    }
    return GiveDataType;
})();
var PropertiesActions = (function () {
    function PropertiesActions() {
    }
    return PropertiesActions;
})();
var VariableType = (function () {
    function VariableType() {
    }
    return VariableType;
})();
var SynonymType = (function () {
    function SynonymType() {
    }
    return SynonymType;
})();
var TimerType = (function () {
    function TimerType() {
    }
    return TimerType;
})();
var UserDefinedCommandType = (function () {
    function UserDefinedCommandType() {
    }
    return UserDefinedCommandType;
})();
var TextAction = (function () {
    function TextAction() {
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
    }
    return ScriptText;
})();
var PlaceType = (function () {
    function PlaceType() {
    }
    return PlaceType;
})();
var RoomType = (function () {
    function RoomType() {
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
    }
    return RoomType;
})();
var ObjectType = (function () {
    function ObjectType() {
        this.Speak = new TextAction();
        this.Take = new TextAction();
        this.AddScript = new TextAction();
        this.RemoveScript = new TextAction();
        this.OpenScript = new TextAction();
        this.CloseScript = new TextAction();
    }
    return ObjectType;
})();
var ChangeType = (function () {
    function ChangeType() {
    }
    return ChangeType;
})();
var GameChangeDataType = (function () {
    function GameChangeDataType() {
    }
    return GameChangeDataType;
})();
var ResourceType = (function () {
    function ResourceType() {
    }
    return ResourceType;
})();
var ExpressionResult = (function () {
    function ExpressionResult() {
    }
    return ExpressionResult;
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
    }
    return ArrayResult;
})();
var PlayerCanAccessObjectResult = (function () {
    function PlayerCanAccessObjectResult() {
    }
    return PlayerCanAccessObjectResult;
})();
var LegacyGame = (function () {
    function LegacyGame() {
        this._casKeywords = [];
        this._nullContext = new Context();
        this._gameChangeData = new GameChangeDataType();
        this._compassExits = {};
        this._gotoExits = {};
        this._textFormatter = new TextFormatter();
        this._log = {};
        this._commandLock = new Object();
        this._stateLock = new Object();
        this._state = 0 /* Ready */;
        this._waitLock = new Object();
        this._readyForCommand = true;
        this._random = new Random();
        this._listVerbs = {};
    }
    LegacyGame.prototype.CopyContext = function (ctx) {
        var result = new Context();
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN ConstructorBlock
    // UNKNOWN ConstructorBlock
    LegacyGame.prototype.RemoveFormatting = function (s) {
        var code;
        var pos;
        var len;
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.CheckSections = function () {
        var defines;
        var braces;
        var checkLine = "";
        var bracePos;
        var pos;
        var section = "";
        var hasErrors;
        var skipBlock;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ConvertFriendlyIfs = function () {
        var convPos;
        var symbPos;
        var symbol;
        var endParamPos;
        var paramData;
        var startParamPos;
        var firstData;
        var secondData;
        var obscureLine;
        var newParam;
        var varObscureLine;
        var bracketCount;
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ConvertMultiLineSections = function () {
        var startLine;
        var braceCount;
        var thisLine;
        var lineToAdd;
        var lastBrace;
        var i;
        var restOfLine;
        var procName;
        var endLineNum;
        var afterLastBrace;
        var z;
        var startOfOrig;
        var testLine;
        var testBraceCount;
        var obp;
        var cbp;
        var curProc;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.ErrorCheck = function () {
        var curBegin;
        var curEnd;
        var hasErrors;
        var curPos;
        var numParamStart;
        var numParamEnd;
        var finLoop;
        var inText;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetAfterParameter = function (s) {
        var eop;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObliterateParameters = function (s) {
        var inParameter;
        var exitCharacter = "";
        var curChar;
        var outputLine = "";
        var obscuringFunctionName;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObliterateVariableNames = function (s) {
        var inParameter;
        var exitCharacter = "";
        var outputLine = "";
        var curChar;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.RemoveComments = function () {
        var aposPos;
        var inTextBlock;
        var inSynonymsBlock;
        var oblitLine;
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.ReportErrorLine = function (s) {
        var replaceFrom;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.YesNo = function (yn) {
        // UNKNOWN SingleLineIfStatement
    };
    LegacyGame.prototype.IsYes = function (yn) {
        // UNKNOWN SingleLineIfStatement
    };
    LegacyGame.prototype.BeginsWith = function (s, text) {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ConvertCasKeyword = function (casChar) {
        var c = 'expr';
        var keyword = this._casKeywords[c];
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ConvertMultiLines = function () {
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.GetDefineBlock = function (blockname) {
        var l;
        var blockType;
        var result = new DefineBlock();
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DefineBlockParam = function (blockname, param) {
        var cache;
        var result = new DefineBlock();
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetEverythingAfter = function (s, text) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.Keyword2CAS = function (KWord) {
        var k = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.LoadCASKeywords = function () {
        var questDatLines = 'expr';
        // UNKNOWN ForEachBlock
    };
    LegacyGame.prototype.GetResourceLines = function (res) {
        var enc = {};
        var resFile = 'expr';
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ParseFile = function (filename) {
        var hasErrors;
        var result;
        var libCode = [];
        var libLines;
        var ignoreMode;
        var skipCheck;
        var c;
        var d;
        var l;
        var libFileHandle;
        var libResourceLines;
        var libFile;
        var libLine;
        var inDefGameBlock;
        var gameLine;
        var inDefSynBlock;
        var synLine;
        var libFoundThisSweep;
        var libFileName;
        var libraryList = [];
        var numLibraries;
        var libraryAlreadyIncluded;
        var inDefTypeBlock;
        var typeBlockName;
        var typeLine;
        var defineCount;
        var curLine;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        var lastSlashPos;
        var slashPos;
        var curPos = 1;

        // UNKNOWN DoLoopUntilBlock
        var filenameNoPath = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        var gotGameBlock = false;
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.LogASLError = function (err, type) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.GetParameter = function (s, ctx, convertStringVariables) {
        var newParam;
        var startPos;
        var endPos;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        var retrParam = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.AddLine = function (line) {
        var numLines;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReDimPreserveStatement
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.LoadCASFile = function (filename) {
        var endLineReached;
        var exitTheLoop;
        var textMode;
        var casVersion;
        var startCat = "";
        var endCatPos;
        var fileData = "";
        var chkVer;
        var j;
        var curLin;
        var textData;
        var cpos;
        var nextLinePos;
        var c;
        var tl;
        var ckw;
        var d;
        // UNKNOWN ReDimStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.DecryptString = function (s) {
        var output = "";
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.RemoveTabs = function (s) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DoAddRemove = function (childId, parentId, add, ctx) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.DoLook = function (id, ctx, showExamineError, showDefaultDescription) {
        var objectContents;
        var foundLook = false;

        // UNKNOWN MultiLineIfBlock
        var lookLine;
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
    };
    LegacyGame.prototype.DoOpenClose = function (id, open, showLook, ctx) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.EvaluateInlineExpressions = function (s) {
        // UNKNOWN MultiLineIfBlock
        var bracePos;
        var curPos = 1;
        var resultLine = "";
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecAddRemove = function (cmd, ctx) {
        var childId;
        var childName;
        var doAdd;
        var sepPos;
        var parentId;
        var sepLen;
        var parentName;
        var verb = "";
        var action;
        var foundAction;
        var actionScript = "";
        var propertyExists;
        var textToPrint;
        var isContainer;
        var gotObject;
        var childLength;
        var noParentSpecified = false;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var canAccessObject = 'expr';

        // UNKNOWN MultiLineIfBlock
        var canAccessParent = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var o = this._objs[parentId];
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecAddRemoveScript = function (parameter, add, ctx) {
        var childId;
        var parentId;
        var commandName;
        var childName;
        var parentName = "";
        var scp;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecOpenClose = function (cmd, ctx) {
        var id;
        var name;
        var doOpen;
        var isOpen;
        var foundAction;
        var action = "";
        var actionScript = "";
        var propertyExists;
        var textToPrint;
        var isContainer;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        var canAccessObject = 'expr';

        // UNKNOWN MultiLineIfBlock
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecuteSelectCase = function (script, ctx) {
        var afterLine = 'expr';

        // UNKNOWN MultiLineIfBlock
        var blockName = 'expr';
        var block = 'expr';
        var checkValue = 'expr';
        var caseMatch = false;
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.ExecVerb = function (cmd, ctx, libCommands) {
        var gameBlock;
        var foundVerb = false;
        var verbProperty = "";
        var script = "";
        var verbsList;
        var thisVerb = "";
        var scp;
        var id;
        var verbObject = "";
        var verbTag;
        var thisScript = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExpressionHandler = function (expr) {
        var openBracketPos;
        var endBracketPos;
        var res = new ExpressionResult();

        // UNKNOWN DoLoopUntilBlock
        var numElements = 1;
        var elements;

        // UNKNOWN ReDimStatement
        var numOperators = 0;
        var operators = [];
        var newElement;
        var obscuredExpr = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        var opNum = 0;
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ListContents = function (id, ctx) {
        var contentsIDs = [];

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var numContents = 0;

        // UNKNOWN ForBlock
        var contents = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObscureNumericExps = function (s) {
        var EPos;
        var CurPos;
        var OutputString;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ProcessListInfo = function (line, id) {
        var listInfo = new TextAction();
        var propName = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.GetHTMLColour = function (colour, defaultColour) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SelectBlock
    };
    LegacyGame.prototype.DoPrint = function (text) {
        // UNKNOWN RaiseEventStatement
    };
    LegacyGame.prototype.DestroyExit = function (exitData, ctx) {
        var fromRoom = "";
        var toRoom = "";
        var roomId;
        var exitId;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var roomExit;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.DoClear = function () {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.DoWait = function () {
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.ExecuteFlag = function (line, ctx) {
        var propertyString = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecuteIfFlag = function (flag) {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIncDec = function (line, ctx) {
        var variable;
        var change;
        var param = 'expr';
        var sc = 'expr';

        // UNKNOWN MultiLineIfBlock
        var value = 'expr';

        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        var arrayIndex = 'expr';
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExtractFile = function (file) {
        var length;
        var startPos;
        var extracted;
        var resId;

        // UNKNOWN SingleLineIfStatement
        var found = false;

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var fileName = 'expr';
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.AddObjectAction = function (id, name, script, noUpdate) {
        var actionNum;
        var foundExisting = false;
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.AddToChangeLog = function (appliesTo, changeData) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReDimPreserveStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.AddToObjectChangeLog = function (appliesToType, appliesTo, element, changeData) {
        var changeLog;
        // UNKNOWN SelectBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.AddToGiveInfo = function (id, giveData) {
        var giveType;
        var actionName;
        var actionScript;
        var o = this._objs[id];
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.AddToObjectActions = function (actionInfo, id, ctx) {
        var actionNum;
        var foundExisting = false;
        var name = 'expr';
        var ep = 'expr';

        // UNKNOWN MultiLineIfBlock
        var script = 'expr';
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.AddToObjectAltNames = function (altNames, id) {
        var o = this._objs[id];
        // UNKNOWN DoLoopUntilBlock
    };
    LegacyGame.prototype.AddToObjectProperties = function (propertyInfo, id, ctx) {
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN DoLoopUntilBlock
    };
    LegacyGame.prototype.AddToUseInfo = function (id, useData) {
        var useType;
        var o = this._objs[id];
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.CapFirst = function (s) {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ConvertVarsIn = function (s, ctx) {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DisambObjHere = function (ctx, id, firstPlace, twoPlaces, secondPlace, isExit) {
        var isSeen;
        var onlySeen = false;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecClone = function (cloneString, ctx) {
        var id;
        var newName;
        var cloneTo;
        var SC = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReDimPreserveStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecOops = function (correction, ctx) {
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecType = function (typeData, ctx) {
        var id;
        var found;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';
        var typeName = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var o = this._objs[id];

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReDimPreserveStatement
        // UNKNOWN SimpleAssignmentStatement
        var propertyData = 'expr';
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.ExecuteIfAction = function (actionData) {
        var id;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';
        var actionName = 'expr';
        var found = false;

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfType = function (typeData) {
        var id;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';
        var typeName = 'expr';
        var found = false;

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetArrayIndex = function (varName, ctx) {
        var result = new ArrayResult();

        // UNKNOWN MultiLineIfBlock
        var beginPos = 'expr';
        var endPos = 'expr';
        var data = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.Disambiguate = function (name, containedIn, ctx, isExit) {
        var numberCorresIds = 0;
        var idNumbers = [];
        var firstPlace;
        var secondPlace = "";
        var twoPlaces;
        var descriptionText;
        var validNames;
        var numValidNames;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DisplayStatusVariableInfo = function (id, type, ctx) {
        var displayData = "";
        var ep;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DoAction = function (ObjID, ActionName, ctx, LogError) {
        var FoundAction;
        var ActionScript = "";
        var o = this._objs[ObjID];

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var NewThread = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.HasAction = function (ObjID, ActionName) {
        var o = this._objs[ObjID];
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecForEach = function (scriptLine, ctx) {
        var inLocation;
        var scriptToRun;
        var isExit;
        var isRoom;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.ExecuteAction = function (data, ctx) {
        var actionName;
        var script;
        var actionNum;
        var id;
        var foundExisting = false;
        var foundObject = false;
        var param = 'expr';
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';

        // UNKNOWN SimpleAssignmentStatement
        var ep = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecuteCondition = function (condition, ctx) {
        var result;
        var thisNot;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteConditions = function (list, ctx) {
        var conditions;
        var numConditions = 0;
        var operations;
        var obscuredConditionList = 'expr';
        var pos = 1;
        var isFinalCondition = false;

        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        var result = true;
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteCreate = function (data, ctx) {
        var newName;
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecuteCreateExit = function (data, ctx) {
        var scrRoom;
        var destRoom = "";
        var destId;
        var exitData = 'expr';
        var newName = 'expr';
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var srcId = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var exists = false;

        // UNKNOWN MultiLineIfBlock
        var paramPos = 'expr';
        var saveData;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        var r = this._rooms[srcId];
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecDrop = function (obj, ctx) {
        var found;
        var parentId;
        var id;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var isInContainer = false;

        // UNKNOWN MultiLineIfBlock
        var dropFound = false;
        var dropStatement = "";
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecExamine = function (command, ctx) {
        var item = 'expr';

        // UNKNOWN MultiLineIfBlock
        var id = 'expr';

        // UNKNOWN MultiLineIfBlock
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecMoveThing = function (data, type, ctx) {
        var scp = 'expr';
        var name = 'expr';
        var place = 'expr';
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecProperty = function (data, ctx) {
        var id;
        var found;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var name = 'expr';
        var properties = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecuteDo = function (procedureName, ctx) {
        var newCtx = 'expr';
        var numParameters = 0;
        var useNewCtx;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var block = 'expr';
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecuteDoAction = function (data, ctx) {
        var id;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';
        var actionName = 'expr';
        var found = false;
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecuteIfHere = function (obj, ctx) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfExists = function (obj, realOnly) {
        var result;
        var errorReport = false;
        var scp;

        // UNKNOWN MultiLineIfBlock
        var found = false;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfProperty = function (data) {
        var id;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';
        var propertyName = 'expr';
        var found = false;
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteRepeat = function (data, ctx) {
        var repeatWhileTrue;
        var repeatScript = "";
        var bracketPos;
        var afterBracket;
        var foundScript = false;

        // UNKNOWN MultiLineIfBlock
        var pos = 1;

        // UNKNOWN DoLoopUntilBlock
        var conditions = 'expr';
        var finished = false;
        // UNKNOWN DoLoopUntilBlock
    };
    LegacyGame.prototype.ExecuteSetCollectable = function (param, ctx) {
        var val;
        var id;
        var scp = 'expr';
        var name = 'expr';
        var newVal = 'expr';
        var found = false;

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var op = 'expr';
        var newValue = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecuteWait = function (waitLine, ctx) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.InitFileData = function (fileData) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.GetNextChunk = function () {
        var nullPos = 'expr';
        var result = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetFileDataChars = function (count) {
        var result = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectActions = function (actionInfo) {
        var name = 'expr';
        var ep = 'expr';

        // UNKNOWN MultiLineIfBlock
        var script = 'expr';
        var result = new ActionType();
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectId = function (name, ctx, room) {
        var id;
        var found = false;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectIdNoAlias = function (name) {
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectProperty = function (name, id, existsOnly, logError) {
        var result = "";
        var found = false;
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetPropertiesInType = function (type, err) {
        var blockId;
        var propertyList = new PropertiesActions();
        var found = false;
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetRoomID = function (name, ctx) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetTextOrScript = function (textScript) {
        var result = new TextAction();
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetThingNumber = function (name, room, type) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetThingBlock = function (name, room, type) {
        var result = new DefineBlock();
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.MakeRestoreData = function () {
        var data = {};
        var objectData = [];
        var roomData = [];
        var numObjectData;
        var numRoomData;

        // UNKNOWN ExpressionStatement
        var start = data.Length + 1;

        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ForEachBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        var dataString;
        var newFileData = {};
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.MoveThing = function (name, room, type, ctx) {
        var oldRoom = "";
        var id = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.Pause = function (duration) {
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.ConvertParameter = function (parameter, convertChar, action, ctx) {
        var result = "";
        var pos = 1;
        var finished = false;
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DoFunction = function (data, ctx) {
        var name;
        var parameter;
        var intFuncResult = "";
        var intFuncExecuted = false;
        var paramPos = 'expr';

        // UNKNOWN MultiLineIfBlock
        var block;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.DoInternalFunction = function (name, parameter, ctx) {
        var parameters;
        var untrimmedParameters;
        var objId;
        var numParameters = 0;
        var pos = 1;

        // UNKNOWN MultiLineIfBlock
        var param2;
        var param3;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecFor = function (line, ctx) {
        // UNKNOWN MultiLineIfBlock
        var endValue;
        var stepValue;
        var forData = 'expr';
        var scp1 = 'expr';
        var scp2 = 'expr';
        var scp3 = 'expr';
        var counterVariable = 'expr';
        var startValue = 'expr';

        // UNKNOWN MultiLineIfBlock
        var loopScript = 'expr';
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.ExecSetVar = function (varInfo, ctx) {
        var scp = 'expr';
        var varName = 'expr';
        var varCont = 'expr';
        var idx = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN TryBlock
    };
    LegacyGame.prototype.ExecuteIfAsk = function (question) {
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SetQuestionResponse = function (response) {
        var runnerThread = {};
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetQuestionResponseInNewThread = function (response) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.ExecuteIfGot = function (item) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfHas = function (condition) {
        var checkValue;
        var colNum;
        var scp = 'expr';
        var name = 'expr';
        var newVal = 'expr';
        var found = false;

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var op = 'expr';
        var newValue = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfIs = function (condition) {
        var value1;
        var value2;
        var op;
        var expectNumerics;
        var expResult;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var scp2 = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var result = false;
        // UNKNOWN SelectBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetNumericContents = function (name, ctx, noError) {
        var numNumber;
        var arrayIndex;
        var exists = false;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.PlayerErrorMessage = function (e, ctx) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.PlayerErrorMessage_ExtendInfo = function (e, ctx, extraInfo) {
        var message = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.GetErrorMessage = function (e, ctx) {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.PlayMedia = function (filename) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.PlayMedia = function (filename, sync, looped) {
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.PlayWav = function (parameter) {
        var sync = false;
        var looped = false;
        var params = {};

        // UNKNOWN SimpleAssignmentStatement
        var filename = 'expr';
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.RestoreGameData = function (fileData) {
        var appliesTo;
        var data = "";
        var objId;
        var timerNum;
        var varUbound;
        var found;
        var numStoredData;
        var storedData = [];
        var decryptedFile = {};

        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        var numData = 'expr';
        var createdObjects = {};
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.SetBackground = function (col) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetForeground = function (col) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetDefaultPlayerErrorMessages = function () {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.SetFont = function (name) {
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetFontSize = function (size) {
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetNumericVariableContents = function (name, content, ctx, arrayIndex) {
        var numNumber;
        var exists = false;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReDimPreserveStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.SetOpenClose = function (name, open, ctx) {
        var cmd;

        // UNKNOWN MultiLineIfBlock
        var id = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetTimerState = function (name, state) {
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetUnknownVariableType = function (variableData, ctx) {
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var name = 'expr';

        // UNKNOWN MultiLineIfBlock
        var contents = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SetUpChoiceForm = function (blockName, ctx) {
        var block = 'expr';
        var prompt = 'expr';
        var menuOptions = {};
        var menuScript = {};

        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        var mnu = new MenuData();
        var choice = 'expr';
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SetUpDefaultFonts = function () {
        var gameblock = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpDisplayVariables = function () {
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpGameObject = function () {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReDimStatement
        // UNKNOWN SimpleAssignmentStatement
        var o = this._objs[1];

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        var nestBlock = 0;
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpMenus = function () {
        var exists = false;
        var menuTitle = "";
        var menuOptions = {};
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.SetUpOptions = function () {
        var opt;
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpRoomData = function () {
        var defaultProperties = new PropertiesActions();
        var defaultExists = false;
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpSynonyms = function () {
        var block = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpTimers = function () {
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpTurnScript = function () {
        var block = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpUserDefinedPlayerErrors = function () {
        var block = 'expr';
        var examineIsCustomised = false;
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetVisibility = function (thing, type, visible, ctx) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ShowPictureInText = function (filename) {
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ShowRoomInfoV2 = function (Room) {
        var roomDisplayText = "";
        var descTagExist;
        var gameBlock;
        var charsViewable;
        var charsFound;
        var prefixAliasNoFormat;
        var prefix;
        var prefixAlias;
        var inDesc;
        var aliasName = "";
        var charList;
        var foundLastComma;
        var cp;
        var ncp;
        var noFormatObjsViewable;
        var objsViewable = "";
        var objsFound;
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
        var lastComma;
        var oldLastComma;
        var defineBlock;
        var lookString = "";

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        var roomBlock;

        // UNKNOWN SimpleAssignmentStatement
        var finishedFindingCommas;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        var finishedLoop;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        var outside;

        // UNKNOWN MultiLineIfBlock
        var finished;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SingleLineIfStatement
    };
    LegacyGame.prototype.Speak = function (text) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.AddToObjectList = function (objList, exitList, name, type) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecExec = function (scriptLine, ctx) {
        // UNKNOWN SingleLineIfStatement
        var execLine = 'expr';
        var newCtx = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecSetString = function (info, ctx) {
        var scp = 'expr';
        var name = 'expr';
        var value = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var idx = 'expr';
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecUserCommand = function (cmd, ctx, libCommands) {
        var curCmd;
        var commandList;
        var script = "";
        var commandTag;
        var commandLine = "";
        var foundCommand = false;
        var roomId = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteChoose = function (section, ctx) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.GetCommandParameters = function (test, required, ctx) {
        var chunksBegin;
        var chunksEnd;
        var varName;
        var var2Pos;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        var currentReqLinePos = 1;
        var currentTestLinePos = 1;
        var finished = false;
        var numberChunks = 0;

        // UNKNOWN DoLoopUntilBlock
        var success = true;
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetGender = function (character, capitalise, ctx) {
        var result;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetStringContents = function (name, ctx) {
        var returnAlias = false;
        var arrayIndex = 0;
        var cp = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var exists = false;
        var id;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.IsAvailable = function (thingName, type, ctx) {
        var room;
        var name;
        var atPos = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.IsCompatible = function (test, required) {
        var var2Pos;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        var currentReqLinePos = 1;
        var currentTestLinePos = 1;
        var finished = false;
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.OpenGame = function (filename) {
        var cdatb;
        var result;
        var visible;
        var room;
        var fileData = "";
        var savedQsgVersion;
        var data = "";
        var name;
        var scp;
        var cdat;
        var scp2;
        var scp3;
        var lines = 'expr';

        // UNKNOWN SimpleAssignmentStatement
        var prevQsgVersion = false;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SaveGame = function (filename, saveFile) {
        var ctx = new Context();
        var saveData;
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.MakeRestoreDataV2 = function () {
        var lines = {};
        var i;
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SetAvailability = function (thingString, exists, ctx, type) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetStringContents = function (name, value, ctx, arrayIndex) {
        var id;
        var exists = false;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReDimPreserveStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.SetUpCharObjectInfo = function () {
        var defaultProperties = new PropertiesActions();

        // UNKNOWN SimpleAssignmentStatement
        var defaultExists = false;
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ShowGameAbout = function (ctx) {
        var version = 'expr';
        var author = 'expr';
        var copyright = 'expr';
        var info = 'expr';
        // UNKNOWN ExpressionStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ShowPicture = function (filename) {
        var caption = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ShowRoomInfo = function (room, ctx, noPrint) {
        // UNKNOWN MultiLineIfBlock
        var roomDisplayText = "";
        var descTagExist;
        var doorwayString;
        var roomAlias;
        var finishedFindingCommas;
        var prefix;
        var roomDisplayName;
        var roomDisplayNameNoFormat;
        var inDescription;
        var visibleObjects = "";
        var visibleObjectsNoFormat;
        var placeList;
        var lastComma;
        var oldLastComma;
        var descType;
        var descLine = "";
        var showLookText;
        var lookDesc = "";
        var objLook;
        var objSuffix;
        var gameBlock = 'expr';

        // UNKNOWN SimpleAssignmentStatement
        var id = 'expr';

        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SimpleAssignmentStatement
        var visibleObjectsList = {};
        var count;
        // UNKNOWN ForBlock
        // UNKNOWN ForEachBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.CheckCollectable = function (id) {
        var max;
        var value;
        var min;
        var m;
        var type = this._collectables[id].Type;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.DisplayCollectableInfo = function (id) {
        var display;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DisplayTextSection = function (section, ctx, OutputTo) {
        var block;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ExecCommand = function (input, ctx, echo, runUserCommand, dontSetIt) {
        var parameter;
        var skipAfterTurn = false;

        // UNKNOWN SimpleAssignmentStatement
        var oldBadCmdBefore = this._badCmdBefore;
        var roomID = 'expr';
        var enteredHelpCommand = false;

        // UNKNOWN SingleLineIfStatement
        var cmd = 'expr';

        // UNKNOWN SyncLockBlock
        var userCommandReturn;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        var newCommand = " " + input + " ";

        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        var newCtx = 'expr';
        var globalOverride = false;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        var invList = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.CmdStartsWith = function (cmd, startsWith) {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecGive = function (giveString, ctx) {
        var article;
        var item;
        var character;
        var type;
        var id;
        var script = "";
        var toPos = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecLook = function (lookLine, ctx) {
        var item;
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecSpeak = function (cmd, ctx) {
        // UNKNOWN MultiLineIfBlock
        var name = cmd;
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecTake = function (item, ctx) {
        var parentID;
        var parentDisplayName;
        var foundItem = true;
        var foundTake = false;
        var id = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var isInContainer = false;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecUse = function (useLine, ctx) {
        var endOnWith;
        var useDeclareLine = "";
        var useOn;
        var useItem;

        // UNKNOWN SimpleAssignmentStatement
        var roomId;

        // UNKNOWN SimpleAssignmentStatement
        var onWithPos = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var id;
        var notGotItem;

        // UNKNOWN MultiLineIfBlock
        var useScript = "";
        var foundUseScript;
        var foundUseOnObject;
        var useOnObjectId;
        var found;
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObjectActionUpdate = function (id, name, script, noUpdate) {
        var objectName;
        var sp;
        var ep;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecuteIf = function (scriptLine, ctx) {
        var ifLine = 'expr';
        var obscuredLine = 'expr';
        var thenPos = 'expr';

        // UNKNOWN MultiLineIfBlock
        var conditions = 'expr';

        // UNKNOWN SimpleAssignmentStatement
        var elsePos = 'expr';
        var thenEndPos;

        // UNKNOWN MultiLineIfBlock
        var thenScript = 'expr';
        var elseScript = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ExecuteScript = function (scriptLine, ctx, newCallingObjectId) {
        // UNKNOWN TryBlock
    };
    LegacyGame.prototype.ExecuteEnter = function (scriptLine, ctx) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.ExecuteSet = function (setInstruction, ctx) {
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.FindStatement = function (block, statement) {
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.FindLine = function (block, statement, statementParam) {
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetCollectableAmount = function (name) {
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetSecondChunk = function (line) {
        var endOfFirstBit = 'expr' + 1;
        var lengthOfKeyword = 'expr' + 1;
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GoDirection = function (direction, ctx) {
        var dirData = new TextAction();
        var id = 'expr';

        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        var r = this._rooms[id];
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.GoToPlace = function (place, ctx) {
        var destination = "";
        var placeData;
        var disallowed = false;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.InitialiseGame = function (filename, fromQsg) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        var gameBlock;

        // UNKNOWN SimpleAssignmentStatement
        var aslVersion = "//";
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.PlaceExist = function (placeName, ctx) {
        var roomId = 'expr';
        var foundPlace = false;
        var scriptPresent = false;
        var r = this._rooms[roomId];
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.PlayerItem = function (item, got, ctx, objId) {
        var foundObjectName = false;
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.PlayGame = function (room, ctx) {
        var id = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.Print = function (txt, ctx, OutputTo) {
        var printString = "";
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.RetrLine = function (blockType, param, line, ctx) {
        var searchblock;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.RetrLineParam = function (blockType, param, line, lineParam, ctx) {
        var searchblock;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SetUpCollectables = function () {
        var lastItem = false;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpItemArrays = function () {
        var lastItem = false;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.SetUpStartItems = function () {
        var lastItem = false;
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.ShowHelp = function (ctx) {
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ReadCatalog = function (data) {
        var nullPos = 'expr';

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReDimPreserveStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        var resourceStart = 0;
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.UpdateDirButtons = function (dirs, ctx) {
        var compassExits = {};
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.AddCompassExit = function (exitList, name) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.UpdateDoorways = function (roomId, ctx) {
        var roomDisplayText = "";
        var directions = "";
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
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.UpdateItems = function (ctx) {
        var invList = {};

        // UNKNOWN SingleLineIfStatement
        var name;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN RaiseEventStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.FinishGame = function (stopType, ctx) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.UpdateObjectList = function (ctx) {
        var shownPlaceName;
        var objSuffix;
        var charsFound;
        var noFormatObjsViewable;
        var charList;
        var objsFound;
        var objListString;
        var noFormatObjListString;

        // UNKNOWN SingleLineIfStatement
        var objList = {};
        var exitList = {};
        var roomBlock;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var roomId;

        // UNKNOWN SimpleAssignmentStatement
        var r = this._rooms[roomId];
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN RaiseEventStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.UpdateExitsList = function () {
        var mergedList = {};
        // UNKNOWN ForEachBlock
        // UNKNOWN ForEachBlock
        // UNKNOWN RaiseEventStatement
    };
    LegacyGame.prototype.UpdateStatusVars = function (ctx) {
        var displayData;
        var status = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.UpdateVisibilityInContainers = function (ctx, onlyParent) {
        var parentId;
        var parent;
        var parentIsTransparent;
        var parentIsOpen;
        var parentIsSeen;
        var parentIsSurface;
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
    };
    LegacyGame.prototype.PlayerCanAccessObject = function (id, colObjects) {
        var parent;
        var parentId;
        var parentDisplayName;
        var result = new PlayerCanAccessObjectResult();
        var hierarchy = "";
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetGoToExits = function (roomId, ctx) {
        var placeList = "";
        var shownPlaceName;
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SetUpExits = function () {
        // UNKNOWN ForBlock
        // UNKNOWN ExitSubStatement
    };
    LegacyGame.prototype.FindExit = function (tag) {
        var params = 'expr';

        // UNKNOWN MultiLineIfBlock
        var room = 'expr';
        var exitName = 'expr';
        var roomId = 'expr';

        // UNKNOWN MultiLineIfBlock
        var exits = this._rooms[roomId].Exits;
        var dir = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteLock = function (tag, lock) {
        var roomExit;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.Begin = function () {
        var runnerThread = {};
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.DoBegin = function () {
        var gameBlock = 'expr';
        var ctx = new Context();

        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SingleLineIfStatement
        var startRoom = "";
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };

    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    LegacyGame.prototype.Finish = function () {
        // UNKNOWN ExpressionStatement
    };

    // UNKNOWN EventStatement
    // UNKNOWN EventStatement
    // UNKNOWN EventStatement
    LegacyGame.prototype.Save = function (filename, html) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.Save = function (html) {
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN PropertyBlock
    LegacyGame.prototype.SendCommand = function (command) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SendCommand = function (command, metadata) {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SendCommand = function (command, elapsedTime, metadata) {
        // UNKNOWN SingleLineIfStatement
        var runnerThread = {};
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.WaitForStateChange = function (changedFromState) {
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.ProcessCommandInNewThread = function (command) {
        // UNKNOWN TryBlock
    };
    LegacyGame.prototype.SendEvent = function (eventName, param) {
    };

    // UNKNOWN EventStatement
    LegacyGame.prototype.Initialise = function (player) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.GameFinished = function () {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN RaiseEventStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
        // UNKNOWN SyncLockBlock
        // UNKNOWN SyncLockBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.GetResourcePath = function (filename) {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.Cleanup = function () {
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.DeleteDirectory = function (dir) {
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.Finalize = function () {
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.GetLibraryLines = function (libName) {
        var libCode = null;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SelectBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN PropertyBlock
    LegacyGame.prototype.Tick = function (elapsedTime) {
        var i;
        var timerScripts = {};
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.RunTimersInNewThread = function (scripts) {
        var scriptList = 'expr';
        // UNKNOWN ForEachBlock
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.RaiseNextTimerTickRequest = function () {
        var anyTimerActive = false;
        var nextTrigger = 60;
        // UNKNOWN ForBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN RaiseEventStatement
    };
    LegacyGame.prototype.ChangeState = function (newState) {
        var acceptCommands = 'expr';
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.ChangeState = function (newState, acceptCommands) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.FinishWait = function () {
        // UNKNOWN SingleLineIfStatement
        var runnerThread = {};
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.FinishWaitInNewThread = function () {
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.FinishPause = function () {
        // UNKNOWN ExpressionStatement
    };

    LegacyGame.prototype.ShowMenu = function (menuData) {
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SetMenuResponse = function (response) {
        var runnerThread = {};
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
    };
    LegacyGame.prototype.SetMenuResponseInNewThread = function (response) {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.LogException = function (ex) {
        // UNKNOWN RaiseEventStatement
    };
    LegacyGame.prototype.GetExternalScripts = function () {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetExternalStylesheets = function () {
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN EventStatement
    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetOriginalFilenameForQSG = function () {
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };

    LegacyGame.prototype.SetUnzipFunction = function (unzipFunction) {
        // UNKNOWN SimpleAssignmentStatement
    };
    LegacyGame.prototype.GetUnzippedFile = function (filename) {
        var tempDir = null;
        var result = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetResource = function (file) {
        // UNKNOWN MultiLineIfBlock
        var path = 'expr';
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetResources = function () {
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.GetResourcelessCAS = function () {
        var fileData = 'expr';
        // UNKNOWN ReturnStatement
    };
    return LegacyGame;
})();
