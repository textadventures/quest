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
    }
    LegacyGame.prototype.CopyContext = function () {
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.CheckSections = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ConvertFriendlyIfs = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.ErrorCheck = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetAfterParameter = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObliterateParameters = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObliterateVariableNames = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.ReportErrorLine = function () {
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetDefineBlock = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DefineBlockParam = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetResourceLines = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ParseFile = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.DecryptString = function () {
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExpressionHandler = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ReDimStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ListContents = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.ObscureNumericExps = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecuteIfAction = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfType = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetArrayIndex = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.Disambiguate = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DoAction = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.HasAction = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecuteCondition = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteConditions = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfProperty = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.GetNextChunk = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetFileDataChars = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectActions = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetObjectId = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetPropertiesInType = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetThingNumber = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetThingBlock = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.MakeRestoreData = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.ConvertParameter = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.DoFunction = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.DoInternalFunction = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.ExecuteIfIs = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SelectBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetNumericContents = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.SetUpChoiceForm = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ExpressionStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.GetCommandParameters = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetGender = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetStringContents = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.IsAvailable = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.IsCompatible = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN DoLoopUntilBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.OpenGame = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.MakeRestoreDataV2 = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.ExecCommand = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SyncLockBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ExpressionStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    // UNKNOWN SubBlock
    LegacyGame.prototype.RetrLine = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.RetrLineParam = function () {
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };
    LegacyGame.prototype.GetGoToExits = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ForBlock
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN SubBlock
    LegacyGame.prototype.FindExit = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
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
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SimpleAssignmentStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetResource = function () {
        // UNKNOWN MultiLineIfBlock
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN ReturnStatement
    };

    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetResources = function () {
        // UNKNOWN ForBlock
        // UNKNOWN MultiLineIfBlock
    };
    LegacyGame.prototype.GetResourcelessCAS = function () {
        // UNKNOWN LocalDeclarationStatement
        // UNKNOWN ReturnStatement
    };
    return LegacyGame;
})();
