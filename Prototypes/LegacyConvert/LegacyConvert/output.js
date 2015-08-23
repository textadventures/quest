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
    }
    return RoomType;
})();
var ObjectType = (function () {
    function ObjectType() {
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
        this._commandLock = 'expr';
        this._stateLock = 'expr';
        this._state = 'expr';
        this._waitLock = 'expr';
        this._readyForCommand = 'expr';
    }
    LegacyGame.prototype.CopyContext = function () {
        var result = 'expr';
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
    LegacyGame.prototype.RemoveFormatting = function () {
        var code;
        var pos;
        var len;
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.CheckSections = function () {
        var defines;
        var braces;
        var checkLine = 'expr';
        var bracePos;
        var pos;
        var section = 'expr';
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

    // UNKNOWN SubBlock
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
    LegacyGame.prototype.GetAfterParameter = function () {
        var eop;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObliterateParameters = function () {
        var inParameter;
        var exitCharacter = 'expr';
        var curChar;
        var outputLine = 'expr';
        var obscuringFunctionName;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObliterateVariableNames = function () {
        var inParameter;
        var exitCharacter = 'expr';
        var outputLine = 'expr';
        var curChar;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.ReportErrorLine = function () {
        var replaceFrom;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.YesNo = function () {
        // UNKNOWN SingleLineIfStatement
    };
    LegacyGame.prototype.IsYes = function () {
        // UNKNOWN SingleLineIfStatement
    };
    LegacyGame.prototype.BeginsWith = function () {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ConvertCasKeyword = function () {
        var c = 'expr';
        var keyword = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetDefineBlock = function () {
        var l;
        var blockType;
        var result = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DefineBlockParam = function () {
        var cache;
        var result = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetEverythingAfter = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.Keyword2CAS = function () {
        var k = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetResourceLines = function () {
        var enc;
        var resFile = 'expr';
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ParseFile = function () {
        var hasErrors;
        var result;
        var libCode;
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
        var libraryList;
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
        var curPos = 'expr';

        // UNKNOWN DoLoopUntilBlock
        var filenameNoPath = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        var gotGameBlock = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetParameter = function () {
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

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.DecryptString = function () {
        var output = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.RemoveTabs = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.EvaluateInlineExpressions = function () {
        // UNKNOWN MultiLineIfBlock
        var bracePos;
        var curPos = 'expr';
        var resultLine = 'expr';
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecVerb = function () {
        var gameBlock;
        var foundVerb = 'expr';
        var verbProperty = 'expr';
        var script = 'expr';
        var verbsList;
        var thisVerb = 'expr';
        var scp;
        var id;
        var verbObject = 'expr';
        var verbTag;
        var thisScript = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExpressionHandler = function () {
        var openBracketPos;
        var endBracketPos;
        var res;

        // UNKNOWN DoLoopUntilBlock
        var numElements = 'expr';
        var elements;

        // UNKNOWN ReDimStatement
        var numOperators = 'expr';
        var operators;
        var newElement;
        var obscuredExpr = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        var opNum = 'expr';
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ListContents = function () {
        var contentsIDs;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var numContents = 'expr';

        // UNKNOWN ForBlock
        var contents = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObscureNumericExps = function () {
        var EPos;
        var CurPos;
        var OutputString;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetHTMLColour = function () {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SelectBlock
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecuteIfFlag = function () {
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.ExtractFile = function () {
        var length;
        var startPos;
        var extracted;
        var resId;

        // UNKNOWN SingleLineIfStatement
        var found = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var fileName = 'expr';
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.CapFirst = function () {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ConvertVarsIn = function () {
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DisambObjHere = function () {
        var isSeen;
        var onlySeen = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecuteIfAction = function () {
        var id;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';
        var actionName = 'expr';
        var found = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var o = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfType = function () {
        var id;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';
        var typeName = 'expr';
        var found = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var o = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetArrayIndex = function () {
        var result;

        // UNKNOWN MultiLineIfBlock
        var beginPos = 'expr';
        var endPos = 'expr';
        var data = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.Disambiguate = function () {
        var numberCorresIds = 'expr';
        var idNumbers;
        var firstPlace;
        var secondPlace = 'expr';
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
    LegacyGame.prototype.DisplayStatusVariableInfo = function () {
        var displayData = 'expr';
        var ep;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DoAction = function () {
        var FoundAction;
        var ActionScript = 'expr';
        var o = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var NewThread = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.HasAction = function () {
        var o = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecuteCondition = function () {
        var result;
        var thisNot;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteConditions = function () {
        var conditions;
        var numConditions = 'expr';
        var operations;
        var obscuredConditionList = 'expr';
        var pos = 'expr';
        var isFinalCondition = 'expr';

        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        var result = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecuteIfHere = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfExists = function () {
        var result;
        var errorReport = 'expr';
        var scp;

        // UNKNOWN MultiLineIfBlock
        var found = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfProperty = function () {
        var id;
        var scp = 'expr';

        // UNKNOWN MultiLineIfBlock
        var objName = 'expr';
        var propertyName = 'expr';
        var found = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.GetNextChunk = function () {
        var nullPos = 'expr';
        var result = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetFileDataChars = function () {
        var result = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectActions = function () {
        var name = 'expr';
        var ep = 'expr';

        // UNKNOWN MultiLineIfBlock
        var script = 'expr';
        var result = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectId = function () {
        var id;
        var found = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectIdNoAlias = function () {
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectProperty = function () {
        var result = 'expr';
        var found = 'expr';
        var o = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetPropertiesInType = function () {
        var blockId;
        var propertyList;
        var found = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetRoomID = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetTextOrScript = function () {
        var result = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetThingNumber = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetThingBlock = function () {
        var result = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.MakeRestoreData = function () {
        var data;
        var objectData;
        var roomData;
        var numObjectData;
        var numRoomData;

        // UNKNOWN ExpressionStatement
        var start = 'expr';

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
        var newFileData;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ConvertParameter = function () {
        var result = 'expr';
        var pos = 'expr';
        var finished = 'expr';
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DoFunction = function () {
        var name;
        var parameter;
        var intFuncResult = 'expr';
        var intFuncExecuted = 'expr';
        var paramPos = 'expr';

        // UNKNOWN MultiLineIfBlock
        var block;
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.DoInternalFunction = function () {
        var parameters;
        var untrimmedParameters;
        var objId;
        var numParameters = 'expr';
        var pos = 'expr';

        // UNKNOWN MultiLineIfBlock
        var param2;
        var param3;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecuteIfAsk = function () {
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecuteIfGot = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfHas = function () {
        var checkValue;
        var colNum;
        var scp = 'expr';
        var name = 'expr';
        var newVal = 'expr';
        var found = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        var op = 'expr';
        var newValue = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfIs = function () {
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
        var result = 'expr';
        // UNKNOWN SelectBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetNumericContents = function () {
        var numNumber;
        var arrayIndex;
        var exists = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.GetErrorMessage = function () {
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.SetUnknownVariableType = function () {
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
    LegacyGame.prototype.SetUpChoiceForm = function () {
        var block = 'expr';
        var prompt = 'expr';
        var menuOptions;
        var menuScript;

        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        var mnu;
        var choice = 'expr';
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecUserCommand = function () {
        var curCmd;
        var commandList;
        var script = 'expr';
        var commandTag;
        var commandLine = 'expr';
        var foundCommand = 'expr';
        var roomId = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetCommandParameters = function () {
        var chunksBegin;
        var chunksEnd;
        var varName;
        var var2Pos;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        var currentReqLinePos = 'expr';
        var currentTestLinePos = 'expr';
        var finished = 'expr';
        var numberChunks = 'expr';

        // UNKNOWN DoLoopUntilBlock
        var success = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetGender = function () {
        var result;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetStringContents = function () {
        var returnAlias = 'expr';
        var arrayIndex = 'expr';
        var cp = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        var exists = 'expr';
        var id;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.IsAvailable = function () {
        var room;
        var name;
        var atPos = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.IsCompatible = function () {
        var var2Pos;

        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        var currentReqLinePos = 'expr';
        var currentTestLinePos = 'expr';
        var finished = 'expr';
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.OpenGame = function () {
        var cdatb;
        var result;
        var visible;
        var room;
        var fileData = 'expr';
        var savedQsgVersion;
        var data = 'expr';
        var name;
        var scp;
        var cdat;
        var scp2;
        var scp3;
        var lines = 'expr';

        // UNKNOWN SimpleAssignmentStatement
        var prevQsgVersion = 'expr';
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
    LegacyGame.prototype.SaveGame = function () {
        var ctx = 'expr';
        var saveData;
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.MakeRestoreDataV2 = function () {
        var lines;
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

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.DisplayCollectableInfo = function () {
        var display;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecCommand = function () {
        var parameter;
        var skipAfterTurn = 'expr';

        // UNKNOWN SimpleAssignmentStatement
        var oldBadCmdBefore = 'expr';
        var roomID = 'expr';
        var enteredHelpCommand = 'expr';

        // UNKNOWN SingleLineIfStatement
        var cmd = 'expr';

        // UNKNOWN SyncLockBlock
        var userCommandReturn;

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        var newCommand = 'expr';

        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        var newCtx = 'expr';
        var globalOverride = 'expr';

        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        var invList = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.CmdStartsWith = function () {
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.FindStatement = function () {
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.FindLine = function () {
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetCollectableAmount = function () {
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetSecondChunk = function () {
        var endOfFirstBit = 'expr';
        var lengthOfKeyword = 'expr';
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.InitialiseGame = function () {
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
        var aslVersion = 'expr';
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
    LegacyGame.prototype.PlaceExist = function () {
        var roomId = 'expr';
        var foundPlace = 'expr';
        var scriptPresent = 'expr';
        var r = 'expr';
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.RetrLine = function () {
        var searchblock;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.RetrLineParam = function () {
        var searchblock;
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.UpdateDoorways = function () {
        var roomDisplayText = 'expr';
        var directions = 'expr';
        var outPlacePrefix = 'expr';
        var n = 'expr';
        var s = 'expr';
        var e = 'expr';
        var w = 'expr';
        var ne = 'expr';
        var nw = 'expr';
        var se = 'expr';
        var sw = 'expr';
        var u = 'expr';
        var d = 'expr';
        var o = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.PlayerCanAccessObject = function () {
        var parent;
        var parentId;
        var parentDisplayName;
        var result;
        var hierarchy = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetGoToExits = function () {
        var placeList = 'expr';
        var shownPlaceName;
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.FindExit = function () {
        var params = 'expr';

        // UNKNOWN MultiLineIfBlock
        var room = 'expr';
        var exitName = 'expr';
        var roomId = 'expr';

        // UNKNOWN MultiLineIfBlock
        var exits = 'expr';
        var dir = 'expr';
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN SubBlock
    // UNKNOWN EventStatement
    // UNKNOWN EventStatement
    // UNKNOWN EventStatement
    // UNKNOWN SubBlock
    LegacyGame.prototype.Save = function () {
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN PropertyBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN EventStatement
    LegacyGame.prototype.Initialise = function () {
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetResourcePath = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.GetLibraryLines = function () {
        var libCode = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SelectBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };

    LegacyGame.prototype.ShowMenu = function () {
        // UNKNOWN ExpressionStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN SyncLockBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
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

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetUnzippedFile = function () {
        var tempDir = 'expr';
        var result = 'expr';
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetResource = function () {
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
