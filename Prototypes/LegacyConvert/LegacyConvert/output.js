function Left(input, length) {
    return input.substring(0, length);
}

function Right(input, length) {
    return input.substring(input.length - length - 1);
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

function InStr(arg1, arg2, arg3) {
    var input, search;
    if (typeof arg3 === 'undefined') {
        input = arg1;
        search = arg2;
        return input.indexOf(search) + 1;
    }

    var start = arg1;
    input = arg2;
    search = arg3;
    return input.indexOf(search, start - 1) + 1;
}

function Split(input, splitChar) {
    return input.split(splitChar);
}

function Join(input, joinChar) {
    return input.join(joinChar);
}

function IsNumeric(input) {
    return !isNaN(parseFloat(input)) && isFinite(input);
}

function Replace(input, oldString, newString) {
    return input.split(oldString).join(newString);
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
    return input.length;
}

function UBound(array) {
    return array.length - 1;
}
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
var AppliesTo;
(function (AppliesTo) {
    AppliesTo[AppliesTo["Object"] = 0] = "Object";
    AppliesTo[AppliesTo["Room"] = 1] = "Room";
})(AppliesTo || (AppliesTo = {}));
;
var LegacyGame = (function () {
    function LegacyGame(data) {
        this._casKeywords = [];
        //The name of the game
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
        this.New(null, null);
        this._data = data;
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
        var pos;
        var len;
        do {
            pos = InStr(s, "|");
            if (pos != 0) {
                code = Mid(s, pos + 1, 3);
                if (Left(code, 1) == "b") {
                    len = 1;
                } else if (Left(code, 2) == "xb") {
                    len = 2;
                } else if (Left(code, 1) == "u") {
                    len = 1;
                } else if (Left(code, 2) == "xu") {
                    len = 2;
                } else if (Left(code, 1) == "i") {
                    len = 1;
                } else if (Left(code, 2) == "xi") {
                    len = 2;
                } else if (Left(code, 2) == "cr") {
                    len = 2;
                } else if (Left(code, 2) == "cb") {
                    len = 2;
                } else if (Left(code, 2) == "cl") {
                    len = 2;
                } else if (Left(code, 2) == "cy") {
                    len = 2;
                } else if (Left(code, 2) == "cg") {
                    len = 2;
                } else if (Left(code, 1) == "n") {
                    len = 1;
                } else if (Left(code, 2) == "xn") {
                    len = 2;
                } else if (Left(code, 1) == "s") {
                    len = 3;
                } else if (Left(code, 2) == "jc") {
                    len = 2;
                } else if (Left(code, 2) == "jl") {
                    len = 2;
                } else if (Left(code, 2) == "jr") {
                    len = 2;
                } else if (Left(code, 1) == "w") {
                    len = 1;
                } else if (Left(code, 1) == "c") {
                    len = 1;
                }
                if (len == 0) {
                    // unknown code
                    len = 1;
                }
                s = Left(s, pos - 1) + Mid(s, pos + len + 1);
            }
        } while(!(pos == 0));
        return s;
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
                } else if (Trim(this._lines[i]) == "end define") {
                    defines = defines - 1;
                    if (defines < 0) {
                        this.LogASLError("Extra 'end define' after block '" + section + "'", 1 /* FatalError */);
                        this._openErrorReport = this._openErrorReport + "Extra 'end define' after block '" + section + "'" + vbCrLf;
                        hasErrors = true;
                        defines = 0;
                    }
                    if (braces > 0) {
                        this.LogASLError("Missing } in block '" + section + "'", 1 /* FatalError */);
                        this._openErrorReport = this._openErrorReport + "Missing } in block '" + section + "'" + vbCrLf;
                        hasErrors = true;
                    } else if (braces < 0) {
                        this.LogASLError("Too many } in block '" + section + "'", 1 /* FatalError */);
                        this._openErrorReport = this._openErrorReport + "Too many } in block '" + section + "'" + vbCrLf;
                        hasErrors = true;
                    }
                }
                if (Left(this._lines[i], 1) != "'" && !skipBlock) {
                    checkLine = this.ObliterateParameters(this._lines[i]);
                    if (this.BeginsWith(checkLine, "'<ERROR;")) {
                        // ObliterateParameters denotes a mismatched $, ( etc.
                        // by prefixing line with '<ERROR;*; where * is the mismatched
                        // character
                        this.LogASLError("Expected closing " + Mid(checkLine, 9, 1) + " character in '" + this.ReportErrorLine(this._lines[i]) + "'", 1 /* FatalError */);
                        this._openErrorReport = this._openErrorReport + "Expected closing " + Mid(checkLine, 9, 1) + " character in '" + this.ReportErrorLine(this._lines[i]) + "'." + vbCrLf;
                        return false;
                    }
                }
                if (Left(Trim(checkLine), 1) != "'") {
                    // Now check {
                    pos = 1;
                    do {
                        bracePos = InStr(pos, checkLine, "{");
                        if (bracePos != 0) {
                            pos = bracePos + 1;
                            braces = braces + 1;
                        }
                    } while(!(bracePos == 0 || pos > Len(checkLine)));

                    // Now check }
                    pos = 1;
                    do {
                        bracePos = InStr(pos, checkLine, "}");
                        if (bracePos != 0) {
                            pos = bracePos + 1;
                            braces = braces - 1;
                        }
                    } while(!(bracePos == 0 || pos > Len(checkLine)));
                }
            }
        }
        if (defines > 0) {
            this.LogASLError("Missing 'end define'", 1 /* FatalError */);
            this._openErrorReport = this._openErrorReport + "Missing 'end define'." + vbCrLf;
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
                    this.LogASLError("Expected closing " + Mid(varObscureLine, 9, 1) + " character in '" + this.ReportErrorLine(this._lines[i]) + "'", 1 /* FatalError */);
                    return true;
                }
                startParamPos = InStr(convPos, this._lines[i], "(");
                endParamPos = 0;
                bracketCount = 1;
                for (var j = startParamPos + 1; j <= Len(this._lines[i]); j++) {
                    if (Mid(this._lines[i], j, 1) == "(") {
                        bracketCount = bracketCount + 1;
                    } else if (Mid(this._lines[i], j, 1) == ")") {
                        bracketCount = bracketCount - 1;
                    }
                    if (bracketCount == 0) {
                        endParamPos = j;
                        break;
                    }
                }
                if (endParamPos == 0) {
                    this.LogASLError("Expected ) in '" + this.ReportErrorLine(this._lines[i]) + "'", 1 /* FatalError */);
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
                                            this.LogASLError("Unrecognised 'if' condition in '" + this.ReportErrorLine(this._lines[i]) + "'", 1 /* FatalError */);
                                            return true;
                                        } else {
                                            symbol = "=";
                                        }
                                    } else {
                                        symbol = ">";
                                    }
                                } else {
                                    symbol = "<";
                                }
                            } else {
                                symbol = ">=";
                            }
                        } else {
                            symbol = "<=";
                        }
                    } else {
                        symbol = "<>";
                    }
                } else {
                    symbol = "<>";
                }
                firstData = Trim(Left(paramData, symbPos - 1));
                secondData = Trim(Mid(paramData, symbPos + Len(symbol)));
                if (symbol == "=") {
                    newParam = "is <" + firstData + ";" + secondData + ">";
                } else {
                    newParam = "is <" + firstData + ";";
                    if (symbol == "<") {
                        newParam = newParam + "lt";
                    } else if (symbol == ">") {
                        newParam = newParam + "gt";
                    } else if (symbol == ">=") {
                        newParam = newParam + "gt=";
                    } else if (symbol == "<=") {
                        newParam = newParam + "lt=";
                    } else if (symbol == "<>") {
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
                            } else if (cbp < obp) {
                                testBraceCount = testBraceCount - 1;
                                testLine = Mid(testLine, cbp + 1);
                            }
                        } while(!(obp == cbp || testBraceCount == 0));
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
                                } else if (Mid(restOfLine, m, 1) == "}") {
                                    braceCount = braceCount - 1;
                                }
                            }
                            if (braceCount != 0) {
                                var k = j + 1;
                                do {
                                    for (var m = 1; m <= Len(this._lines[k]); m++) {
                                        if (Mid(this._lines[k], m, 1) == "{") {
                                            braceCount = braceCount + 1;
                                        } else if (Mid(this._lines[k], m, 1) == "}") {
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
                                    } else {
                                        this.AddLine(Left(this._lines[k], lastBrace - 1));
                                        afterLastBrace = Trim(Mid(this._lines[k], lastBrace + 1));
                                    }

                                    //Clear original line
                                    this._lines[k] = "";
                                    k = k + 1;
                                } while(braceCount != 0);
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
        } while(!(i > this._numberSections));

        for (var i = 1; i <= this._numberSections; i++) {
            z = this._lines[this._defineBlocks[i].StartLine];
            if (((!this.BeginsWith(z, "define text ")) && (!this.BeginsWith(z, "define menu ")) && z != "define synonyms")) {
                for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                    if (this.BeginsWith(this._lines[j], "else ")) {
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
        var curBegin;
        var curEnd;
        var hasErrors;
        var curPos;
        var numParamStart;
        var numParamEnd;
        var finLoop;
        var inText;
        hasErrors = false;
        inText = false;

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
                    if (InStr(curPos, this._lines[i], "<") != 0) {
                        numParamStart = numParamStart + 1;
                        curPos = InStr(curPos, this._lines[i], "<") + 1;
                    } else {
                        finLoop = true;
                    }
                } while(!(finLoop));

                //Find number of >'s:
                curPos = 1;
                finLoop = false;
                do {
                    if (InStr(curPos, this._lines[i], ">") != 0) {
                        numParamEnd = numParamEnd + 1;
                        curPos = InStr(curPos, this._lines[i], ">") + 1;
                    } else {
                        finLoop = true;
                    }
                } while(!(finLoop));
                if (numParamStart > numParamEnd) {
                    this.LogASLError("Expected > in " + this.ReportErrorLine(this._lines[i]), 1 /* FatalError */);
                    hasErrors = true;
                } else if (numParamStart < numParamEnd) {
                    this.LogASLError("Too many > in " + this.ReportErrorLine(this._lines[i]), 1 /* FatalError */);
                    hasErrors = true;
                }
            }
        }

        //Exit if errors found
        if (hasErrors == true) {
            return true;
        }

        for (var i = 1; i <= this._numberSections; i++) {
            curBegin = this._defineBlocks[i].StartLine;
            curEnd = this._defineBlocks[i].EndLine;
            if (this.BeginsWith(this._lines[curBegin], "define game")) {
                if (InStr(this._lines[curBegin], "<") == 0) {
                    this.LogASLError("'define game' has no parameter - game has no name", 1 /* FatalError */);
                    return true;
                }
            } else {
                if (!this.BeginsWith(this._lines[curBegin], "define synonyms") && !this.BeginsWith(this._lines[curBegin], "define options")) {
                    if (InStr(this._lines[curBegin], "<") == 0) {
                        this.LogASLError(this._lines[curBegin] + " has no parameter", 1 /* FatalError */);
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
        var eop;
        eop = InStr(s, ">");
        if (eop == 0 || eop + 1 > Len(s)) {
            return "";
        } else {
            return Trim(Mid(s, eop + 1));
        }
    };
    LegacyGame.prototype.ObliterateParameters = function (s) {
        var inParameter;
        var exitCharacter = "";
        var curChar;
        var outputLine = "";
        var obscuringFunctionName;
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
            } else {
                if (curChar == exitCharacter) {
                    if (!obscuringFunctionName) {
                        inParameter = false;
                        outputLine = outputLine + curChar;
                    } else {
                        // We've finished obscuring the function name,
                        // now let's find the next ) as we were before
                        // we found this dastardly function
                        obscuringFunctionName = false;
                        exitCharacter = ")";
                        outputLine = outputLine + "~";
                    }
                } else {
                    outputLine = outputLine + "~";
                }
            }
        }
        if (inParameter) {
            return "'<ERROR;" + exitCharacter + ";" + outputLine;
        } else {
            return outputLine;
        }
    };
    LegacyGame.prototype.ObliterateVariableNames = function (s) {
        var inParameter;
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
            } else {
                if (curChar == exitCharacter) {
                    inParameter = false;
                    outputLine = outputLine + curChar;
                } else {
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
        var aposPos;
        var inTextBlock;
        var inSynonymsBlock;
        var oblitLine;

        for (var i = 1; i <= UBound(this._lines); i++) {
            if (this.BeginsWith(this._lines[i], "'!qdk-note:")) {
                this._lines[i] = "#!qdk-note:" + this.GetEverythingAfter(this._lines[i], "'!qdk-note:");
            } else {
                if (this.BeginsWith(this._lines[i], "define text ")) {
                    inTextBlock = true;
                } else if (Trim(this._lines[i]) == "define synonyms") {
                    inSynonymsBlock = true;
                } else if (this.BeginsWith(this._lines[i], "define type ")) {
                    inSynonymsBlock = true;
                } else if (Trim(this._lines[i]) == "end define") {
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
                } else if (inSynonymsBlock) {
                    if (Left(Trim(this._lines[i]), 1) == "'") {
                        this._lines[i] = "";
                    } else {
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
        var replaceFrom;
        replaceFrom = InStr(s, "do <!intproc");
        if (replaceFrom != 0) {
            return Left(s, replaceFrom - 1) + "...";
        } else {
            return s;
        }
    };
    LegacyGame.prototype.YesNo = function (yn) {
        if (yn == true) {
            return "Yes";
        } else {
            return "No";
        }
    };
    LegacyGame.prototype.IsYes = function (yn) {
        if (LCase(yn) == "yes") {
            return true;
        } else {
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
        var c = System.Text.Encoding.GetEncoding(1252).GetBytes(casChar)(0);
        var keyword = this._casKeywords[c];
        if (keyword == "!cr") {
            keyword = vbCrLf;
        }
        return keyword;
    };
    LegacyGame.prototype.ConvertMultiLines = function () {
        for (var i = UBound(this._lines); i >= 1; i--) {
            if (Right(this._lines[i], 2) == "__") {
                this._lines[i] = Left(this._lines[i], Len(this._lines[i]) - 2) + LTrim(this._lines[i + 1]);
                this._lines[i + 1] = "";

                //Recalculate this line again
                i = i + 1;
            } else if (Right(this._lines[i], 1) == "_") {
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
            if (InStr(8, l, " ") == 0) {
                l = l + " ";
            }
            blockType = Mid(l, 8, InStr(8, l, " ") - 8);
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
        param = "k" + param;

        // protect against numeric block names
        if (!this._defineBlockParams.ContainsKey(blockname)) {
            // Lazily create cache of define block params
            cache = new any();
            this._defineBlockParams.Add(blockname, cache);
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
                    if (!cache.ContainsKey(blockKey)) {
                        cache.Add(blockKey, this._defineBlocks[i].StartLine + "," + this._defineBlocks[i].EndLine);
                        // silently ignore duplicates
                    } else {
                    }
                }
            }
        } else {
            cache = this._defineBlockParams.Item(blockname);
        }
        if (cache.ContainsKey(param)) {
            var blocks = Split(cache.Item(param), ",");
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
        var k = "";
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
        //Loads data required for conversion of CAS files
        var questDatLines = this.GetResourceLines(My.Resources.QuestDAT);
        questDatLines.forEach(function (line) {
            if (Left(line, 1) != "#") {
                //Lines isn't a comment - so parse it.
                var scp = InStr(line, ";");
                var keyword = Trim(Left(line, scp - 1));
                var num = parseInt(Right(line, Len(line) - scp));
                this._casKeywords[num] = keyword;
            }
        }, this);
    };
    LegacyGame.prototype.GetResourceLines = function (res) {
        var enc = {};
        var resFile = enc.GetString(res);
        return Split(resFile, Chr(13) + Chr(10));
    };
    LegacyGame.prototype.ParseFile = function (filename) {
        //Returns FALSE if failed.
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
        this._defineBlockParams = new any();
        result = true;

        // Parses file and returns the positions of each main
        // 'define' block. Supports nested defines.
        if (LCase(Right(filename, 4)) == ".zip") {
            this._originalFilename = filename;
            filename = this.GetUnzippedFile(filename);
            this._gamePath = System.IO.Path.GetDirectoryName(filename);
        }
        if (LCase(Right(filename, 4)) == ".asl" || LCase(Right(filename, 4)) == ".txt") {
            //Read file into Lines array
            var fileData = this.GetFileData(filename);
            var aslLines = fileData.Split(Chr(13));
            this._lines = [];
            this._lines[0] = "";
            for (var l = 1; l <= aslLines.Length; l++) {
                this._lines[l] = aslLines[l - 1];
                this._lines[l] = this.RemoveTabs(this._lines[l]);
                this._lines[l] = this._lines[l].Trim(" ", Chr(10), Chr(13));
            }
            l = aslLines.Length;
        } else if (LCase(Right(filename, 4)) == ".cas") {
            this.LogASLError("Loading CAS");
            this.LoadCASFile(filename);
            l = UBound(this._lines);
        } else {
            throw "Unrecognized file extension";
        }

        // Add libraries to end of code:
        numLibraries = 0;
        do {
            libFoundThisSweep = false;
            for (var i = l; i >= 1; i--) {
                // We search for includes backwards as a game might include
                // some-general.lib and then something-specific.lib which needs
                // something-general; if we include something-specific first,
                // then something-general afterwards, something-general's startscript
                // gets executed before something-specific's, as we execute the
                // lib startscripts backwards as well
                if (this.BeginsWith(this._lines[i], "!include ")) {
                    libFileName = this.GetParameter(this._lines[i], this._nullContext);

                    //Clear !include statement
                    this._lines[i] = "";
                    libraryAlreadyIncluded = false;
                    this.LogASLError("Including library '" + libFileName + "'...", 3 /* Init */);
                    for (var j = 1; j <= numLibraries; j++) {
                        if (LCase(libFileName) == LCase(libraryList[j])) {
                            libraryAlreadyIncluded = true;
                            break;
                        }
                    }
                    if (libraryAlreadyIncluded) {
                        this.LogASLError("     - Library already included.", 3 /* Init */);
                    } else {
                        numLibraries = numLibraries + 1;
                        if (!libraryList)
                            libraryList = [];
                        libraryList[numLibraries] = libFileName;
                        libFoundThisSweep = true;
                        libResourceLines = null;
                        libFile = this._gamePath + libFileName;
                        this.LogASLError(" - Searching for " + libFile + " (game path)", 3 /* Init */);
                        libFileHandle = FreeFile();
                        if (System.IO.File.Exists(libFile)) {
                            FileOpen(libFileHandle, libFile, OpenMode.Input);
                        } else {
                            // File was not found; try standard Quest libraries (stored here as resources)
                            this.LogASLError("     - Library not found in game path.", 3 /* Init */);
                            this.LogASLError(" - Searching for " + libFile + " (standard libraries)", 3 /* Init */);
                            libResourceLines = this.GetLibraryLines(libFileName);
                            if (libResourceLines == null) {
                                this.LogASLError("Library not found.", 1 /* FatalError */);
                                this._openErrorReport = this._openErrorReport + "Library '" + libraryList[numLibraries] + "' not found." + vbCrLf;
                                return false;
                            }
                        }
                        this.LogASLError("     - Found library, opening...", 3 /* Init */);
                        libLines = 0;
                        if (libResourceLines == null) {
                            // UNKNOWN DoUntilLoopBlock
                            FileClose(libFileHandle);
                        } else {
                            libResourceLines.forEach(function (resLibLine) {
                                libLines = libLines + 1;
                                if (!libCode)
                                    libCode = [];
                                libLine = resLibLine;
                                libLine = this.RemoveTabs(libLine);
                                libCode[libLines] = Trim(libLine);
                            }, this);
                        }
                        var libVer = -1;
                        if (libCode[1] == "!library") {
                            for (var c = 1; c <= libLines; c++) {
                                if (this.BeginsWith(libCode[c], "!asl-version ")) {
                                    libVer = parseInt(this.GetParameter(libCode[c], this._nullContext));
                                    break;
                                }
                            }
                        } else {
                            //Old library
                            libVer = 100;
                        }
                        if (libVer == -1) {
                            this.LogASLError(" - Library has no asl-version information.", 4 /* LibraryWarningError */);
                            libVer = 200;
                        }
                        ignoreMode = false;
                        for (var c = 1; c <= libLines; c++) {
                            if (this.BeginsWith(libCode[c], "!include ")) {
                                // Quest only honours !include in a library for asl-version
                                // 311 or later, as it ignored them in versions < 3.11
                                if (libVer >= 311) {
                                    this.AddLine(libCode[c]);
                                    l = l + 1;
                                }
                            } else if (Left(libCode[c], 1) != "!" && Left(libCode[c], 1) != "'" && !ignoreMode) {
                                this.AddLine(libCode[c]);
                                l = l + 1;
                            } else {
                                if (libCode[c] == "!addto game") {
                                    inDefGameBlock = 0;
                                    for (var d = 1; d <= UBound(this._lines); d++) {
                                        if (this.BeginsWith(this._lines[d], "define game ")) {
                                            inDefGameBlock = 1;
                                        } else if (this.BeginsWith(this._lines[d], "define ")) {
                                            if (inDefGameBlock != 0) {
                                                inDefGameBlock = inDefGameBlock + 1;
                                            }
                                        } else if (this._lines[d] == "end define" && inDefGameBlock == 1) {
                                            gameLine = d;
                                            d = UBound(this._lines);
                                        } else if (this._lines[d] == "end define") {
                                            if (inDefGameBlock != 0) {
                                                inDefGameBlock = inDefGameBlock - 1;
                                            }
                                        }
                                    }
                                    do {
                                        c = c + 1;
                                        if (!this.BeginsWith(libCode[c], "!end")) {
                                            if (!this._lines)
                                                this._lines = [];
                                            for (var d = UBound(this._lines); d >= gameLine + 1; d--) {
                                                this._lines[d] = this._lines[d - 1];
                                            }

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
                                            if (libVer >= 311 && this.BeginsWith(libCode[c], "startscript ")) {
                                                this._lines[gameLine] = "lib " + libCode[c];
                                            } else if (libVer >= 392 && (this.BeginsWith(libCode[c], "command ") || this.BeginsWith(libCode[c], "verb "))) {
                                                this._lines[gameLine] = "lib " + libCode[c];
                                            } else {
                                                this._lines[gameLine] = libCode[c];
                                            }
                                            l = l + 1;
                                            gameLine = gameLine + 1;
                                        }
                                    } while(!(this.BeginsWith(libCode[c], "!end")));
                                    //ignore
                                    //ignore
                                    //ignore
                                } else if (libCode[c] == "!addto synonyms") {
                                    inDefSynBlock = 0;
                                    for (var d = 1; d <= UBound(this._lines); d++) {
                                        if (this._lines[d] == "define synonyms") {
                                            inDefSynBlock = 1;
                                        } else if (this._lines[d] == "end define" && inDefSynBlock == 1) {
                                            synLine = d;
                                            d = UBound(this._lines);
                                        }
                                    }
                                    if (inDefSynBlock == 0) {
                                        //No "define synonyms" block in game - so add it
                                        this.AddLine("define synonyms");
                                        this.AddLine("end define");
                                        synLine = UBound(this._lines);
                                    }
                                    do {
                                        c = c + 1;
                                        if (!this.BeginsWith(libCode[c], "!end")) {
                                            if (!this._lines)
                                                this._lines = [];
                                            for (var d = UBound(this._lines); d >= synLine + 1; d--) {
                                                this._lines[d] = this._lines[d - 1];
                                            }
                                            this._lines[synLine] = libCode[c];
                                            l = l + 1;
                                            synLine = synLine + 1;
                                        }
                                    } while(!(this.BeginsWith(libCode[c], "!end")));
                                } else if (this.BeginsWith(libCode[c], "!addto type ")) {
                                    inDefTypeBlock = 0;
                                    typeBlockName = LCase(this.GetParameter(libCode[c], this._nullContext));
                                    for (var d = 1; d <= UBound(this._lines); d++) {
                                        if (LCase(this._lines[d]) == "define type <" + typeBlockName + ">") {
                                            inDefTypeBlock = 1;
                                        } else if (this._lines[d] == "end define" && inDefTypeBlock == 1) {
                                            typeLine = d;
                                            d = UBound(this._lines);
                                        }
                                    }
                                    if (inDefTypeBlock == 0) {
                                        //No "define type (whatever)" block in game - so add it
                                        this.AddLine("define type <" + typeBlockName + ">");
                                        this.AddLine("end define");
                                        typeLine = UBound(this._lines);
                                    }
                                    do {
                                        c = c + 1;
                                        if (c > libLines) {
                                            // UNKNOWN ExitDoStatement
                                        }
                                        if (!this.BeginsWith(libCode[c], "!end")) {
                                            if (!this._lines)
                                                this._lines = [];
                                            for (var d = UBound(this._lines); d >= typeLine + 1; d--) {
                                                this._lines[d] = this._lines[d - 1];
                                            }
                                            this._lines[typeLine] = libCode[c];
                                            l = l + 1;
                                            typeLine = typeLine + 1;
                                        }
                                    } while(!(this.BeginsWith(libCode[c], "!end")));
                                } else if (libCode[c] == "!library") {
                                } else if (this.BeginsWith(libCode[c], "!asl-version ")) {
                                    //ignore
                                } else if (this.BeginsWith(libCode[c], "'")) {
                                    //ignore
                                } else if (this.BeginsWith(libCode[c], "!QDK")) {
                                    //ignore
                                    ignoreMode = true;
                                } else if (this.BeginsWith(libCode[c], "!end")) {
                                    ignoreMode = false;
                                }
                            }
                        }
                    }
                }
            }
        } while(!(libFoundThisSweep == false));
        skipCheck = false;
        var lastSlashPos;
        var slashPos;
        var curPos = 1;
        do {
            slashPos = InStr(curPos, filename, "\\");
            if (slashPos != 0) {
                lastSlashPos = slashPos;
            }
            curPos = slashPos + 1;
        } while(!(slashPos == 0));
        var filenameNoPath = LCase(Mid(filename, lastSlashPos + 1));
        for (var i = 1; i <= this._numSkipCheckFiles; i++) {
            if (filenameNoPath == this._skipCheckFile[i]) {
                skipCheck = true;
                break;
            }
        }
        if (filenameNoPath == "musicvf1.cas") {
            this._useStaticFrameForPictures = true;
        }

        //RemoveComments called within ConvertMultiLines
        this.ConvertMultiLines();
        if (!skipCheck) {
            if (!this.CheckSections()) {
                return false;
            }
        }
        this._numberSections = 1;
        for (var i = 1; i <= l; i++) {
            // find section beginning with 'define'
            if (this.BeginsWith(this._lines[i], "define") == true) {
                // Now, go through until we reach an 'end define'. However, if we
                // encounter another 'define' there is a nested define. So, if we
                // encounter 'define' we increment the definecount. When we find an
                // 'end define' we decrement it. When definecount is zero, we have
                // found the end of the section.
                defineCount = 1;

                // Don't count the current line - we know it begins with 'define'...
                curLine = i + 1;
                do {
                    if (this.BeginsWith(this._lines[curLine], "define") == true) {
                        defineCount = defineCount + 1;
                    } else if (this.BeginsWith(this._lines[curLine], "end define") == true) {
                        defineCount = defineCount - 1;
                    }
                    curLine = curLine + 1;
                } while(!(defineCount == 0));
                curLine = curLine - 1;

                // Now, we know that the define section begins at i and ends at
                // curline. Remember where the section begins and ends:
                if (!this._defineBlocks)
                    this._defineBlocks = [];
                this._defineBlocks[this._numberSections] = new DefineBlock();
                this._defineBlocks[this._numberSections].StartLine = i;
                this._defineBlocks[this._numberSections].EndLine = curLine;
                this._numberSections = this._numberSections + 1;
                i = curLine;
            }
        }
        this._numberSections = this._numberSections - 1;
        var gotGameBlock = false;
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define game ")) {
                gotGameBlock = true;
                break;
            }
        }
        if (!gotGameBlock) {
            this._openErrorReport = this._openErrorReport + "No 'define game' block." + vbCrLf;
            return false;
        }
        this.ConvertMultiLineSections();
        hasErrors = this.ConvertFriendlyIfs();
        if (!hasErrors) {
            hasErrors = this.ErrorCheck();
        }
        if (hasErrors) {
            throw "Errors found in game file.";
        }
        this._saveGameFile = "";
        return result;
    };
    LegacyGame.prototype.LogASLError = function (err, type) {
        if (typeof type === "undefined") { type = 0 /* Misc */; }
        if (type == 1 /* FatalError */) {
            err = "FATAL ERROR: " + err;
        } else if (type == 2 /* WarningError */) {
            err = "ERROR: " + err;
        } else if (type == 4 /* LibraryWarningError */) {
            err = "WARNING ERROR (LIBRARY): " + err;
        } else if (type == 3 /* Init */) {
            err = "INIT: " + err;
        } else if (type == 5 /* Warning */) {
            err = "WARNING: " + err;
        } else if (type == 6 /* UserError */) {
            err = "ERROR (REQUESTED): " + err;
        } else if (type == 7 /* InternalError */) {
            err = "INTERNAL ERROR: " + err;
        }
        this._log.Add(err);
    };
    LegacyGame.prototype.GetParameter = function (s, ctx, convertStringVariables) {
        if (typeof convertStringVariables === "undefined") { convertStringVariables = true; }
        // Returns the parameters between < and > in a string
        var newParam;
        var startPos;
        var endPos;
        startPos = InStr(s, "<");
        endPos = InStr(s, ">");
        if (startPos == 0 || endPos == 0) {
            this.LogASLError("Expected parameter in '" + this.ReportErrorLine(s) + "'", 2 /* WarningError */);
            return "";
        }
        var retrParam = Mid(s, startPos + 1, (endPos - startPos) - 1);
        if (convertStringVariables) {
            if (this._gameAslVersion >= 320) {
                newParam = this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(retrParam, "#", 0 /* Strings */, ctx), "%", 2 /* Numeric */, ctx), "$", 1 /* Functions */, ctx);
            } else {
                if (!Left(retrParam, 9) == "~Internal") {
                    newParam = this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(retrParam, "#", 0 /* Strings */, ctx), "%", 2 /* Numeric */, ctx), "~", 3 /* Collectables */, ctx), "$", 1 /* Functions */, ctx);
                } else {
                    newParam = retrParam;
                }
            }
        } else {
            newParam = retrParam;
        }
        return this.EvaluateInlineExpressions(newParam);
    };
    LegacyGame.prototype.AddLine = function (line) {
        //Adds a line to the game script
        var numLines;
        numLines = UBound(this._lines) + 1;
        if (!this._lines)
            this._lines = [];
        this._lines[numLines] = line;
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
        this._lines = [];
        if (Config.ReadGameFileFromAzureBlob) {
            // UNKNOWN UsingBlock
        } else {
            fileData = System.IO.File.ReadAllText(filename, System.Text.Encoding.GetEncoding(1252));
        }
        chkVer = Left(fileData, 7);
        if (chkVer == "QCGF001") {
            casVersion = 1;
        } else if (chkVer == "QCGF002") {
            casVersion = 2;
        } else if (chkVer == "QCGF003") {
            casVersion = 3;
        } else {
            throw "Invalid or corrupted CAS file.";
        }
        if (casVersion == 3) {
            startCat = this.Keyword2CAS("!startcat");
        }
        for (var i = 9; i <= Len(fileData); i++) {
            if (casVersion == 3 && Mid(fileData, i, 1) == startCat) {
                // Read catalog
                this._startCatPos = i;
                endCatPos = InStr(j, fileData, this.Keyword2CAS("!endcat"));
                this.ReadCatalog(Mid(fileData, j + 1, endCatPos - j - 1));
                this._resourceFile = filename;
                this._resourceOffset = endCatPos + 1;
                i = Len(fileData);
                this._casFileData = fileData;
            } else {
                curLin = "";
                endLineReached = false;
                if (textMode == true) {
                    textData = Mid(fileData, i, InStr(i, fileData, Chr(253)) - (i - 1));
                    textData = Left(textData, Len(textData) - 1);
                    cpos = 1;
                    var finished = false;
                    if (textData != "") {
                        do {
                            nextLinePos = InStr(cpos, textData, Chr(0));
                            if (nextLinePos == 0) {
                                nextLinePos = Len(textData) + 1;
                                finished = true;
                            }
                            tl = DecryptString(Mid(textData, cpos, nextLinePos - cpos));
                            this.AddLine(tl);
                            cpos = nextLinePos + 1;
                        } while(!(finished));
                    }
                    textMode = false;
                    i = InStr(i, fileData, Chr(253));
                }
                j = i;
                do {
                    ckw = Mid(fileData, j, 1);
                    c = this.ConvertCasKeyword(ckw);
                    if (c == vbCrLf) {
                        endLineReached = true;
                    } else {
                        if (Left(c, 1) != "!") {
                            curLin = curLin + c + " ";
                        } else {
                            if (c == "!quote") {
                                exitTheLoop = false;
                                curLin = curLin + "<";
                                do {
                                    j = j + 1;
                                    d = Mid(fileData, j, 1);
                                    if (d != Chr(0)) {
                                        curLin = curLin + DecryptString(d);
                                    } else {
                                        curLin = curLin + "> ";
                                        exitTheLoop = true;
                                    }
                                } while(!(exitTheLoop));
                            } else if (c == "!unknown") {
                                exitTheLoop = false;
                                do {
                                    j = j + 1;
                                    d = Mid(fileData, j, 1);
                                    if (d != Chr(254)) {
                                        curLin = curLin + d;
                                    } else {
                                        exitTheLoop = true;
                                    }
                                } while(!(exitTheLoop));
                                curLin = curLin + " ";
                            }
                        }
                    }
                    j = j + 1;
                } while(!(endLineReached));
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
        if (InStr(s, Chr(9)) > 0) {
            //Remove tab characters and change them into
            //spaces; otherwise they bugger up the Trim
            //commands.
            var cpos = 1;
            var finished = false;
            do {
                var tabChar = InStr(cpos, s, Chr(9));
                if (tabChar != 0) {
                    s = Left(s, tabChar - 1) + Space(4) + Mid(s, tabChar + 1);
                    cpos = tabChar + 1;
                } else {
                    finished = true;
                }
            } while(!(finished));
        }
        return s;
    };
    LegacyGame.prototype.DoAddRemove = function (childId, parentId, add, ctx) {
        if (add) {
            this.AddToObjectProperties("parent=" + this._objs[parentId].ObjectName, childId, ctx);
            this._objs[childId].ContainerRoom = this._objs[parentId].ContainerRoom;
        } else {
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
        if (typeof showExamineError === "undefined") { showExamineError = false; }
        if (typeof showDefaultDescription === "undefined") { showDefaultDescription = true; }
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
                    } else {
                        this.ExecuteScript(lookLine, ctx, id);
                    }
                    foundLook = true;
                }
            }
        }
        if (this._gameAslVersion >= 391) {
            objectContents = this.ListContents(id, ctx);
        } else {
            objectContents = "";
        }
        if (!foundLook && showDefaultDescription) {
            var err;
            if (showExamineError) {
                err = 17 /* DefaultExamine */;
            } else {
                err = 8 /* DefaultLook */;
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
        } else {
            this.AddToObjectProperties("not opened", id, ctx);
        }
        this.UpdateVisibilityInContainers(ctx, this._objs[id].ObjectName);
    };
    LegacyGame.prototype.EvaluateInlineExpressions = function (s) {
        // Evaluates in-line expressions e.g. msg <Hello, did you know that 2 + 2 = {2+2}?>
        if (this._gameAslVersion < 391) {
            return s;
        }
        var bracePos;
        var curPos = 1;
        var resultLine = "";
        do {
            bracePos = InStr(curPos, s, "{");
            if (bracePos != 0) {
                resultLine = resultLine + Mid(s, curPos, bracePos - curPos);
                if (Mid(s, bracePos, 2) == "{{") {
                    // {{ = {
                    curPos = bracePos + 2;
                    resultLine = resultLine + "{";
                } else {
                    var EndBracePos = InStr(bracePos + 1, s, "}");
                    if (EndBracePos == 0) {
                        this.LogASLError("Expected } in '" + s + "'", 2 /* WarningError */);
                        return "<ERROR>";
                    } else {
                        var expression = Mid(s, bracePos + 1, EndBracePos - bracePos - 1);
                        var expResult = this.ExpressionHandler(expression);
                        if (expResult.Success != 0 /* OK */) {
                            this.LogASLError("Error evaluating expression in <" + s + "> - " + expResult.Message);
                            return "<ERROR>";
                        }
                        resultLine = resultLine + expResult.Result;
                        curPos = EndBracePos + 1;
                    }
                }
            } else {
                resultLine = resultLine + Mid(s, curPos);
            }
        } while(!(bracePos == 0 || curPos > Len(s)));

        // Above, we only bothered checking for {{. But for consistency, also }} = }. So let's do that:
        curPos = 1;
        do {
            bracePos = InStr(curPos, resultLine, "}}");
            if (bracePos != 0) {
                resultLine = Left(resultLine, bracePos) + Mid(resultLine, bracePos + 2);
                curPos = bracePos + 1;
            }
        } while(!(bracePos == 0 || curPos > Len(resultLine)));
        return resultLine;
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
        } else if (this.BeginsWith(cmd, "add ")) {
            verb = "add";
            doAdd = true;
            sepPos = InStr(cmd, " to ");
            sepLen = 4;
        } else if (this.BeginsWith(cmd, "remove ")) {
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
            this.PlayerErrorMessage(0 /* BadCommand */, ctx);
            this._badCmdBefore = verb;
            // UNKNOWN ExitSubStatement
        }
        childName = Trim(Mid(cmd, Len(verb) + 2, childLength));
        gotObject = false;
        if (this._gameAslVersion >= 392 && doAdd) {
            childId = this.Disambiguate(childName, this._currentRoom + ";inventory", ctx);
            if (childId > 0) {
                if (this._objs[childId].ContainerRoom == "inventory") {
                    gotObject = true;
                } else {
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
                    // UNKNOWN ExitSubStatement
                }
            } else {
                if (childId != -2) {
                    this.PlayerErrorMessage(4 /* NoItem */, ctx);
                }
                this._badCmdBefore = verb;
                // UNKNOWN ExitSubStatement
            }
        } else {
            childId = this.Disambiguate(childName, "inventory;" + this._currentRoom, ctx);
            if (childId <= 0) {
                if (childId != -2) {
                    this.PlayerErrorMessage(7 /* BadThing */, ctx);
                }
                this._badCmdBefore = verb;
                // UNKNOWN ExitSubStatement
            }
        }
        if (noParentSpecified && doAdd) {
            this.SetStringContents("quest.error.article", this._objs[childId].Article, ctx);
            this.PlayerErrorMessage(29 /* BadPut */, ctx);
            // UNKNOWN ExitSubStatement
        }
        if (doAdd) {
            action = "add";
        } else {
            action = "remove";
        }
        if (!noParentSpecified) {
            parentName = Trim(Mid(cmd, sepPos + sepLen));
            parentId = this.Disambiguate(parentName, this._currentRoom + ";inventory", ctx);
            if (parentId <= 0) {
                if (parentId != -2) {
                    this.PlayerErrorMessage(7 /* BadThing */, ctx);
                }
                this._badCmdBefore = Left(cmd, sepPos + sepLen);
                // UNKNOWN ExitSubStatement
            }
        } else {
            // Assume the player was referring to the parent that the object is already in,
            // if it is even in an object already
            if (!this.IsYes(this.GetObjectProperty("parent", childId, true, false))) {
                this.PlayerErrorMessage(32 /* CantRemove */, ctx);
                // UNKNOWN ExitSubStatement
            }
            parentId = this.GetObjectIdNoAlias(this.GetObjectProperty("parent", childId, false, true));
        }

        // Check if parent is a container
        isContainer = this.IsYes(this.GetObjectProperty("container", parentId, true, false));
        if (!isContainer) {
            if (doAdd) {
                this.PlayerErrorMessage(30 /* CantPut */, ctx);
            } else {
                this.PlayerErrorMessage(32 /* CantRemove */, ctx);
            }
            // UNKNOWN ExitSubStatement
        }

        // Check object is already held by that parent
        if (this.IsYes(this.GetObjectProperty("parent", childId, true, false))) {
            if (doAdd && LCase(this.GetObjectProperty("parent", childId, false, false)) == LCase(this._objs[parentId].ObjectName)) {
                this.PlayerErrorMessage(33 /* AlreadyPut */, ctx);
            }
        }

        // Check parent and child are accessible to player
        var canAccessObject = this.PlayerCanAccessObject(childId);
        if (!canAccessObject.CanAccessObject) {
            if (doAdd) {
                this.PlayerErrorMessage_ExtendInfo(30 /* CantPut */, ctx, canAccessObject.ErrorMsg);
            } else {
                this.PlayerErrorMessage_ExtendInfo(32 /* CantRemove */, ctx, canAccessObject.ErrorMsg);
            }
            // UNKNOWN ExitSubStatement
        }
        var canAccessParent = this.PlayerCanAccessObject(parentId);
        if (!canAccessParent.CanAccessObject) {
            if (doAdd) {
                this.PlayerErrorMessage_ExtendInfo(30 /* CantPut */, ctx, canAccessParent.ErrorMsg);
            } else {
                this.PlayerErrorMessage_ExtendInfo(32 /* CantRemove */, ctx, canAccessParent.ErrorMsg);
            }
            // UNKNOWN ExitSubStatement
        }

        // Check if parent is a closed container
        if (!this.IsYes(this.GetObjectProperty("surface", parentId, true, false)) && !this.IsYes(this.GetObjectProperty("opened", parentId, true, false))) {
            // Not a surface and not open, so can't add to this closed container.
            if (doAdd) {
                this.PlayerErrorMessage(30 /* CantPut */, ctx);
            } else {
                this.PlayerErrorMessage(32 /* CantRemove */, ctx);
            }
            // UNKNOWN ExitSubStatement
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
        } else {
            // Now check for a property
            propertyExists = this.IsYes(this.GetObjectProperty(action, parentId, true, false));
            if (!propertyExists) {
                // Show error message
                if (doAdd) {
                    this.PlayerErrorMessage(30 /* CantPut */, ctx);
                } else {
                    this.PlayerErrorMessage(32 /* CantRemove */, ctx);
                }
            } else {
                textToPrint = this.GetObjectProperty(action, parentId, false, false);
                if (textToPrint == "") {
                    // Show default message
                    if (doAdd) {
                        this.PlayerErrorMessage(31 /* DefaultPut */, ctx);
                    } else {
                        this.PlayerErrorMessage(34 /* DefaultRemove */, ctx);
                    }
                } else {
                    this.Print(textToPrint, ctx);
                }
                this.DoAddRemove(childId, parentId, doAdd, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecAddRemoveScript = function (parameter, add, ctx) {
        var childId;
        var parentId;
        var commandName;
        var childName;
        var parentName = "";
        var scp;
        if (add) {
            commandName = "add";
        } else {
            commandName = "remove";
        }
        scp = InStr(parameter, ";");
        if (scp == 0 && add) {
            this.LogASLError("No parent specified in '" + commandName + " <" + parameter + ">", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        if (scp != 0) {
            childName = LCase(Trim(Left(parameter, scp - 1)));
            parentName = LCase(Trim(Mid(parameter, scp + 1)));
        } else {
            childName = LCase(Trim(parameter));
        }
        childId = this.GetObjectIdNoAlias(childName);
        if (childId == 0) {
            this.LogASLError("Invalid child object name specified in '" + commandName + " <" + parameter + ">", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        if (scp != 0) {
            parentId = this.GetObjectIdNoAlias(parentName);
            if (parentId == 0) {
                this.LogASLError("Invalid parent object name specified in '" + commandName + " <" + parameter + ">", 2 /* WarningError */);
                // UNKNOWN ExitSubStatement
            }
            this.DoAddRemove(childId, parentId, add, ctx);
        } else {
            this.AddToObjectProperties("not parent", childId, ctx);
            this.UpdateVisibilityInContainers(ctx, this._objs[parentId].ObjectName);
        }
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
        if (this.BeginsWith(cmd, "open ")) {
            action = "open";
            doOpen = true;
        } else if (this.BeginsWith(cmd, "close ")) {
            action = "close";
            doOpen = false;
        }
        name = this.GetEverythingAfter(cmd, action + " ");
        id = this.Disambiguate(name, this._currentRoom + ";inventory", ctx);
        if (id <= 0) {
            if (id != -2) {
                this.PlayerErrorMessage(7 /* BadThing */, ctx);
            }
            this._badCmdBefore = action;
            // UNKNOWN ExitSubStatement
        }

        // Check if it's even a container
        isContainer = this.IsYes(this.GetObjectProperty("container", id, true, false));
        if (!isContainer) {
            if (doOpen) {
                this.PlayerErrorMessage(25 /* CantOpen */, ctx);
            } else {
                this.PlayerErrorMessage(26 /* CantClose */, ctx);
            }
            // UNKNOWN ExitSubStatement
        }

        // Check if it's already open (or closed)
        isOpen = this.IsYes(this.GetObjectProperty("opened", id, true, false));
        if (doOpen && isOpen) {
            // Object is already open
            this.PlayerErrorMessage(23 /* AlreadyOpen */, ctx);
            // UNKNOWN ExitSubStatement
        } else if (!doOpen && !isOpen) {
            // Object is already closed
            this.PlayerErrorMessage(24 /* AlreadyClosed */, ctx);
            // UNKNOWN ExitSubStatement
        }

        // Check if it's accessible, i.e. check it's not itself inside another closed container
        var canAccessObject = this.PlayerCanAccessObject(id);
        if (!canAccessObject.CanAccessObject) {
            if (doOpen) {
                this.PlayerErrorMessage_ExtendInfo(25 /* CantOpen */, ctx, canAccessObject.ErrorMsg);
            } else {
                this.PlayerErrorMessage_ExtendInfo(26 /* CantClose */, ctx, canAccessObject.ErrorMsg);
            }
            // UNKNOWN ExitSubStatement
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
        } else {
            // Now check for a property
            propertyExists = this.IsYes(this.GetObjectProperty(action, id, true, false));
            if (!propertyExists) {
                // Show error message
                if (doOpen) {
                    this.PlayerErrorMessage(25 /* CantOpen */, ctx);
                } else {
                    this.PlayerErrorMessage(26 /* CantClose */, ctx);
                }
            } else {
                textToPrint = this.GetObjectProperty(action, id, false, false);
                if (textToPrint == "") {
                    // Show default message
                    if (doOpen) {
                        this.PlayerErrorMessage(27 /* DefaultOpen */, ctx);
                    } else {
                        this.PlayerErrorMessage(28 /* DefaultClose */, ctx);
                    }
                } else {
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
            this.LogASLError("No case block specified for '" + script + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        var blockName = this.GetParameter(afterLine, ctx);
        var block = this.DefineBlockParam("procedure", blockName);
        var checkValue = this.GetParameter(script, ctx);
        var caseMatch = false;
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            // Go through all the cases until we find the one that matches
            if (this._lines[i] != "") {
                if (!this.BeginsWith(this._lines[i], "case ")) {
                    this.LogASLError("Invalid line in 'select case' block: '" + this._lines[i] + "'", 2 /* WarningError */);
                } else {
                    var caseScript = "";
                    if (this.BeginsWith(this._lines[i], "case else ")) {
                        caseMatch = true;
                        caseScript = this.GetEverythingAfter(this._lines[i], "case else ");
                    } else {
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
                            } else {
                                thisCase = Mid(thisCase, SCP + 1);
                            }
                        } while(!(finished));
                    }
                    if (caseMatch) {
                        this.ExecuteScript(caseScript, ctx);
                        // UNKNOWN ExitSubStatement
                    }
                }
            }
        }
    };
    LegacyGame.prototype.ExecVerb = function (cmd, ctx, libCommands) {
        if (typeof libCommands === "undefined") { libCommands = false; }
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
        if (!libCommands) {
            verbTag = "verb ";
        } else {
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
                } else {
                    scp = InStr(verbsList, ";");
                    if (scp == 0) {
                        verbProperty = LCase(verbsList);
                    } else {
                        verbProperty = LCase(Trim(Left(verbsList, scp - 1)));
                    }
                }

                do {
                    scp = InStr(verbsList, ";");
                    if (scp == 0) {
                        thisVerb = LCase(verbsList);
                    } else {
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
                } while(!(scp == 0 || Trim(verbsList) == "" || foundVerb));
                if (foundVerb) {
                    break;
                }
            }
        }
        if (foundVerb) {
            id = this.Disambiguate(verbObject, "inventory;" + this._currentRoom, ctx);
            if (id < 0) {
                if (id != -2) {
                    this.PlayerErrorMessage(7 /* BadThing */, ctx);
                }
                this._badCmdBefore = thisVerb;
            } else {
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
        var openBracketPos;
        var endBracketPos;
        var res = new ExpressionResult();

        do {
            openBracketPos = InStr(expr, "(");
            if (openBracketPos != 0) {
                // Find equivalent closing bracket
                var BracketCount = 1;
                endBracketPos = 0;
                for (var i = openBracketPos + 1; i <= Len(expr); i++) {
                    if (Mid(expr, i, 1) == "(") {
                        BracketCount = BracketCount + 1;
                    } else if (Mid(expr, i, 1) == ")") {
                        BracketCount = BracketCount - 1;
                    }
                    if (BracketCount == 0) {
                        endBracketPos = i;
                        break;
                    }
                }
                if (endBracketPos != 0) {
                    var NestedResult = this.ExpressionHandler(Mid(expr, openBracketPos + 1, endBracketPos - openBracketPos - 1));
                    if (NestedResult.Success != 0 /* OK */) {
                        res.Success = NestedResult.Success;
                        res.Message = NestedResult.Message;
                        return res;
                    }
                    expr = Left(expr, openBracketPos - 1) + " " + NestedResult.Result + " " + Mid(expr, endBracketPos + 1);
                } else {
                    res.Message = "Missing closing bracket";
                    res.Success = 1 /* Fail */;
                    return res;
                }
            }
        } while(!(openBracketPos == 0));

        // Split expression into elements, e.g.:
        //       2 + 3 * 578.2 / 36
        //       E O E O EEEEE O EE      where E=Element, O=Operator
        var numElements = 1;
        var elements;
        elements = [];
        var numOperators = 0;
        var operators = [];
        var newElement;
        var obscuredExpr = this.ObscureNumericExps(expr);
        for (var i = 1; i <= Len(expr); i++) {
            // UNKNOWN SelectBlock
            if (newElement) {
                numElements = numElements + 1;
                if (!elements)
                    elements = [];
                numOperators = numOperators + 1;
                if (!operators)
                    operators = [];
                operators[numOperators] = Mid(expr, i, 1);
            } else {
                elements[numElements] = elements[numElements] + Mid(expr, i, 1);
            }
        }

        for (var i = 1; i <= numElements; i++) {
            elements[i] = Trim(elements[i]);
            if (!IsNumeric(elements[i])) {
                res.Message = "Syntax error evaluating expression - non-numeric element '" + elements[i] + "'";
                res.Success = 1 /* Fail */;
                return res;
            }
        }
        var opNum = 0;
        do {
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
                var result;

                // UNKNOWN SelectBlock
                elements[opNum] = (result).toString();

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
        } while(!(opNum == 0 || numOperators == 0));
        res.Success = 0 /* OK */;
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
            } else {
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
                    } else {
                        // If header doesn't end in a colon, then the header is the only text to print
                        contents = listString;
                        displayList = false;
                    }
                } else {
                    contents = UCase(Left(this._objs[id].Article, 1)) + Mid(this._objs[id].Article, 2) + " contains ";
                }
                if (displayList) {
                    for (var i = 1; i <= numContents; i++) {
                        if (i > 1) {
                            if (i < numContents) {
                                contents = contents + ", ";
                            } else {
                                contents = contents + " and ";
                            }
                        }
                        var o = this._objs[contentsIDs[i]];
                        if (o.Prefix != "") {
                            contents = contents + o.Prefix;
                        }
                        if (o.ObjectAlias != "") {
                            contents = contents + "|b" + o.ObjectAlias + "|xb";
                        } else {
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
        } else {
            return this.GetObjectProperty("list empty", id, false, false);
        }
    };
    LegacyGame.prototype.ObscureNumericExps = function (s) {
        // Obscures + or - next to E in Double-type variables with exponents
        //   e.g. 2.345E+20 becomes 2.345EX20
        // This stops huge numbers breaking parsing of maths functions
        var ep;
        var result = s;
        var pos = 1;
        do {
            ep = InStr(pos, result, "E");
            if (ep != 0) {
                result = Left(result, ep) + "X" + Mid(result, ep + 2);
                pos = ep + 2;
            }
        } while(!(ep == 0));
        return result;
    };
    LegacyGame.prototype.ProcessListInfo = function (line, id) {
        var listInfo = new TextAction();
        var propName = "";
        if (this.BeginsWith(line, "list closed <")) {
            listInfo.Type = 0 /* Text */;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list closed";
        } else if (Trim(line) == "list closed off") {
            // default for list closed is off anyway
            // UNKNOWN ExitSubStatement
        } else if (this.BeginsWith(line, "list closed")) {
            listInfo.Type = 1 /* Script */;
            listInfo.Data = this.GetEverythingAfter(line, "list closed");
            propName = "list closed";
        } else if (this.BeginsWith(line, "list empty <")) {
            listInfo.Type = 0 /* Text */;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list empty";
        } else if (Trim(line) == "list empty off") {
            // default for list empty is off anyway
            // UNKNOWN ExitSubStatement
        } else if (this.BeginsWith(line, "list empty")) {
            listInfo.Type = 1 /* Script */;
            listInfo.Data = this.GetEverythingAfter(line, "list empty");
            propName = "list empty";
        } else if (Trim(line) == "list off") {
            this.AddToObjectProperties("not list", id, this._nullContext);
            // UNKNOWN ExitSubStatement
        } else if (this.BeginsWith(line, "list <")) {
            listInfo.Type = 0 /* Text */;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list";
        } else if (this.BeginsWith(line, "list ")) {
            listInfo.Type = 1 /* Script */;
            listInfo.Data = this.GetEverythingAfter(line, "list ");
            propName = "list";
        }
        if (propName != "") {
            if (listInfo.Type == 0 /* Text */) {
                this.AddToObjectProperties(propName + "=" + listInfo.Data, id, this._nullContext);
            } else {
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
        var scp = InStr(exitData, ";");
        if (scp == 0) {
            this.LogASLError("No exit name specified in 'destroy exit <" + exitData + ">'");
            // UNKNOWN ExitSubStatement
        }
        var roomExit;
        if (this._gameAslVersion >= 410) {
            roomExit = this.FindExit(exitData);
            if (roomExit == null) {
                this.LogASLError("Can't find exit in 'destroy exit <" + exitData + ">'");
                // UNKNOWN ExitSubStatement
            }
            roomExit.Parent.RemoveExit(roomExit);
        } else {
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
                // UNKNOWN ExitSubStatement
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
    LegacyGame.prototype.DoWait = function () {
        this._player.DoWait();
        this.ChangeState(2 /* Waiting */);
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.ExecuteFlag = function (line, ctx) {
        var propertyString = "";
        if (this.BeginsWith(line, "on ")) {
            propertyString = this.GetParameter(line, ctx);
        } else if (this.BeginsWith(line, "off ")) {
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
        var change;
        var param = this.GetParameter(line, ctx);
        var sc = InStr(param, ";");
        if (sc == 0) {
            change = 1;
            variable = param;
        } else {
            change = Val(Mid(param, sc + 1));
            variable = Trim(Left(param, sc - 1));
        }
        var value = this.GetNumericContents(variable, ctx, true);
        if (value <= -32766) {
            value = 0;
        }
        if (this.BeginsWith(line, "inc ")) {
            value = value + change;
        } else if (this.BeginsWith(line, "dec ")) {
            value = value - change;
        }
        var arrayIndex = this.GetArrayIndex(variable, ctx);
        this.SetNumericVariableContents(arrayIndex.Name, value, ctx, arrayIndex.Index);
    };
    LegacyGame.prototype.ExtractFile = function (file) {
        var length;
        var startPos;
        var extracted;
        var resId;
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
            this.LogASLError("Unable to extract '" + file + "' - not present in resources.", 2 /* WarningError */);
            return null;
        }
        var fileName = System.IO.Path.Combine(this._tempFolder, file);
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
        if (!extracted) {
            // Extract file from cached CAS data
            var fileData = Mid(this._casFileData, startPos, length);

            // Write file to temp dir
            System.IO.File.WriteAllText(fileName, fileData, System.Text.Encoding.GetEncoding(1252));
            this._resources[resId].Extracted = true;
        }
        return fileName;
    };
    LegacyGame.prototype.AddObjectAction = function (id, name, script, noUpdate) {
        if (typeof noUpdate === "undefined") { noUpdate = false; }
        // Use NoUpdate in e.g. AddToGiveInfo, otherwise ObjectActionUpdate will call
        // AddToGiveInfo again leading to a big loop
        var actionNum;
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
        // UNKNOWN SelectBlock
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
                // UNKNOWN ExitSubStatement
            } else {
                giveType = 0 /* GiveToSomething */;
                actionName = "give to ";
            }
        } else {
            if (this.BeginsWith(giveData, "anything ")) {
                o.GiveAnything = this.GetEverythingAfter(giveData, "anything ");
                this.AddObjectAction(id, "give anything", o.GiveAnything, true);
                // UNKNOWN ExitSubStatement
            } else {
                giveType = 1 /* GiveSomethingTo */;
                actionName = "give ";
            }
        }
        if (Left(Trim(giveData), 1) == "<") {
            var name = this.GetParameter(giveData, this._nullContext);
            var dataId;
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
        var actionNum;
        var foundExisting = false;
        var name = LCase(this.GetParameter(actionInfo, ctx));
        var ep = InStr(actionInfo, ">");
        if (ep == Len(actionInfo)) {
            this.LogASLError("No script given for '" + name + "' action data", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
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
        } while(!(Trim(altNames) == ""));
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
            var num;
            if (info == "") {
                // UNKNOWN ExitDoStatement
            }
            var ep = InStr(info, "=");
            if (ep != 0) {
                name = Trim(Left(info, ep - 1));
                value = Trim(Mid(info, ep + 1));
            } else {
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
            } else {
                o.Properties[num].PropertyName = name;
                o.Properties[num].PropertyValue = value;
            }
            this.AddToObjectChangeLog(ChangeLog.AppliesTo.Object, this._objs[id].ObjectName, name, "properties " + info);
            // UNKNOWN SelectBlock
        } while(!(Len(Trim(propertyInfo)) == 0));
    };
    LegacyGame.prototype.AddToUseInfo = function (id, useData) {
        var useType;
        var o = this._objs[id];
        if (this.BeginsWith(useData, "on ")) {
            useData = this.GetEverythingAfter(useData, "on ");
            if (this.BeginsWith(useData, "anything ")) {
                o.UseOnAnything = this.GetEverythingAfter(useData, "anything ");
                // UNKNOWN ExitSubStatement
            } else {
                useType = 0 /* UseOnSomething */;
            }
        } else {
            if (this.BeginsWith(useData, "anything ")) {
                o.UseAnything = this.GetEverythingAfter(useData, "anything ");
                // UNKNOWN ExitSubStatement
            } else {
                useType = 1 /* UseSomethingOn */;
            }
        }
        if (Left(Trim(useData), 1) == "<") {
            var objectName = this.GetParameter(useData, this._nullContext);
            var dataId;
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
        } else {
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
        if (typeof twoPlaces === "undefined") { twoPlaces = false; }
        if (typeof secondPlace === "undefined") { secondPlace = ""; }
        if (typeof isExit === "undefined") { isExit = false; }
        var isSeen;
        var onlySeen = false;
        if (firstPlace == "game") {
            firstPlace = "";
            if (secondPlace == "seen") {
                twoPlaces = false;
                secondPlace = "";
                onlySeen = true;
                var roomObjId = this._rooms[this.GetRoomID(this._objs[id].ContainerRoom, ctx)].ObjId;
                if (this._objs[id].ContainerRoom == "inventory") {
                    isSeen = true;
                } else {
                    if (this.IsYes(this.GetObjectProperty("visited", roomObjId, true, false))) {
                        isSeen = true;
                    } else {
                        if (this.IsYes(this.GetObjectProperty("seen", id, true, false))) {
                            isSeen = true;
                        }
                    }
                }
            }
        }
        if (((twoPlaces == false && (LCase(this._objs[id].ContainerRoom) == LCase(firstPlace) || firstPlace == "")) || (twoPlaces == true && (LCase(this._objs[id].ContainerRoom) == LCase(firstPlace) || LCase(this._objs[id].ContainerRoom) == LCase(secondPlace)))) && this._objs[id].Exists == true && this._objs[id].IsExit == isExit) {
            if (!onlySeen) {
                return true;
            }
            return isSeen;
        }
        return false;
    };
    LegacyGame.prototype.ExecClone = function (cloneString, ctx) {
        var id;
        var newName;
        var cloneTo;
        var scp = InStr(cloneString, ";");
        if (scp == 0) {
            this.LogASLError("No new object name specified in 'clone <" + cloneString + ">", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        } else {
            var objectToClone = Trim(Left(cloneString, scp - 1));
            id = this.GetObjectIdNoAlias(objectToClone);
            var SC2 = InStr(scp + 1, cloneString, ";");
            if (SC2 == 0) {
                cloneTo = this._objs[id].ContainerRoom;
                newName = Trim(Mid(cloneString, scp + 1));
            } else {
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
        } else {
            this.AddToChangeLog("object " + newName, "create " + this._objs[this._numberObjs].ContainerRoom);
        }
        this.UpdateObjectList(ctx);
    };
    LegacyGame.prototype.ExecOops = function (correction, ctx) {
        if (this._badCmdBefore != "") {
            if (this._badCmdAfter == "") {
                this.ExecCommand(this._badCmdBefore + " " + correction, ctx, false);
            } else {
                this.ExecCommand(this._badCmdBefore + " " + correction + " " + this._badCmdAfter, ctx, false);
            }
        }
    };
    LegacyGame.prototype.ExecType = function (typeData, ctx) {
        var id;
        var found;
        var scp = InStr(typeData, ";");
        if (scp == 0) {
            this.LogASLError("No type name given in 'type <" + typeData + ">'");
            // UNKNOWN ExitSubStatement
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
            // UNKNOWN ExitSubStatement
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

        for (var i = 1; i <= propertyData.NumberTypesIncluded; i++) {
            o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
            if (!o.TypesIncluded)
                o.TypesIncluded = [];
            o.TypesIncluded[o.NumberTypesIncluded] = propertyData.TypesIncluded[i];
        }
    };
    LegacyGame.prototype.ExecuteIfAction = function (actionData) {
        var id;
        var scp = InStr(actionData, ";");
        if (scp == 0) {
            this.LogASLError("No action name given in condition 'action <" + actionData + ">' ...", 2 /* WarningError */);
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
            this.LogASLError("No such object '" + objName + "' in condition 'action <" + actionData + ">' ...", 2 /* WarningError */);
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
        var id;
        var scp = InStr(typeData, ";");
        if (scp == 0) {
            this.LogASLError("No type name given in condition 'type <" + typeData + ">' ...", 2 /* WarningError */);
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
            this.LogASLError("No such object '" + objName + "' in condition 'type <" + typeData + ">' ...", 2 /* WarningError */);
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
        } else {
            result.Index = parseInt(this.GetNumericContents(data, ctx));
        }
        result.Name = Left(varName, beginPos - 1);
        return result;
    };
    LegacyGame.prototype.Disambiguate = function (name, containedIn, ctx, isExit) {
        if (typeof isExit === "undefined") { isExit = false; }
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
        var twoPlaces;
        var descriptionText;
        var validNames;
        var numValidNames;
        name = Trim(name);
        this.SetStringContents("quest.lastobject", "", ctx);
        if (InStr(containedIn, ";") != 0) {
            var scp = InStr(containedIn, ";");
            twoPlaces = true;
            firstPlace = Trim(Left(containedIn, scp - 1));
            secondPlace = Trim(Mid(containedIn, scp + 1));
        } else {
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
            if (this._lastIt != 0 && this._lastItMode == 0 /* Inanimate */ && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                this.PlayerErrorMessage(22 /* BadPronoun */, ctx);
                return -2;
            }
        } else if (name == "him") {
            this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == 1 /* Male */ && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                this.PlayerErrorMessage(22 /* BadPronoun */, ctx);
                return -2;
            }
        } else if (name == "her") {
            this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == 2 /* Female */ && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                this.PlayerErrorMessage(22 /* BadPronoun */, ctx);
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
            for (var i = 1; i <= this._numberObjs; i++) {
                if (this.DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit)) {
                    var thisName;
                    if (this._objs[i].ObjectAlias != "") {
                        thisName = LCase(this._objs[i].ObjectAlias);
                    } else {
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

            // UNKNOWN SelectBlock
            return idNumbers[1];
        } else if (numberCorresIds > 1) {
            descriptionText = [];
            var question = "Please select which " + name + " you mean:";
            this.Print("- |i" + question + "|xi", ctx);
            var menuItems = {};
            for (var i = 1; i <= numberCorresIds; i++) {
                descriptionText[i] = this._objs[idNumbers[i]].Detail;
                if (descriptionText[i] == "") {
                    if (this._objs[idNumbers[i]].Prefix == "") {
                        descriptionText[i] = this._objs[idNumbers[i]].ObjectAlias;
                    } else {
                        descriptionText[i] = this._objs[idNumbers[i]].Prefix + this._objs[idNumbers[i]].ObjectAlias;
                    }
                }
                menuItems.Add((i).toString(), descriptionText[i]);
            }
            var mnu = new MenuData();
            var response = this.ShowMenu(mnu);
            this._choiceNumber = parseInt(response);
            this.SetStringContents("quest.lastobject", this._objs[idNumbers[this._choiceNumber]].ObjectName, ctx);
            this._thisTurnIt = idNumbers[this._choiceNumber];

            // UNKNOWN SelectBlock
            this.Print("- " + descriptionText[this._choiceNumber] + "|n", ctx);
            return idNumbers[this._choiceNumber];
        }
        this._thisTurnIt = this._lastIt;
        this.SetStringContents("quest.error.object", name, ctx);
        return -1;
    };
    LegacyGame.prototype.DisplayStatusVariableInfo = function (id, type, ctx) {
        var displayData = "";
        var ep;
        if (type == 0 /* String */) {
            displayData = this.ConvertVarsIn(this._stringVariable[id].DisplayString, ctx);
            ep = InStr(displayData, "!");
            if (ep != 0) {
                displayData = Left(displayData, ep - 1) + this._stringVariable[id].VariableContents[0] + Mid(displayData, ep + 1);
            }
        } else if (type == 1 /* Numeric */) {
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
                var secondStar = InStr(firstStar + 1, displayData, "*");
                var beforeStar = Left(displayData, firstStar - 1);
                var afterStar = Mid(displayData, secondStar + 1);
                var betweenStar = Mid(displayData, firstStar + 1, (secondStar - firstStar) - 1);
                if (parseFloat(this._numericVariable[id].VariableContents[0]) != 1) {
                    displayData = beforeStar + betweenStar + afterStar;
                } else {
                    displayData = beforeStar + afterStar;
                }
            }
        }
        return displayData;
    };
    LegacyGame.prototype.DoAction = function (id, action, ctx, logError) {
        if (typeof logError === "undefined") { logError = true; }
        var found;
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
        var isExit;
        var isRoom;
        if (this.BeginsWith(scriptLine, "object ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "object ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each object " + this.ReportErrorLine(scriptLine) + "'", 2 /* WarningError */);
                // UNKNOWN ExitSubStatement
            }
        } else if (this.BeginsWith(scriptLine, "exit ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "exit ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each exit " + this.ReportErrorLine(scriptLine) + "'", 2 /* WarningError */);
                // UNKNOWN ExitSubStatement
            }
            isExit = true;
        } else if (this.BeginsWith(scriptLine, "room ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "room ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each room " + this.ReportErrorLine(scriptLine) + "'", 2 /* WarningError */);
                // UNKNOWN ExitSubStatement
            }
            isRoom = true;
        } else {
            this.LogASLError("Unknown type in 'for each " + this.ReportErrorLine(scriptLine) + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        scriptLine = this.GetEverythingAfter(scriptLine, "in ");
        if (this.BeginsWith(scriptLine, "game ")) {
            inLocation = "";
            scriptToRun = this.GetEverythingAfter(scriptLine, "game ");
        } else {
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
        var actionNum;
        var id;
        var foundExisting = false;
        var foundObject = false;
        var param = this.GetParameter(data, ctx);
        var scp = InStr(param, ";");
        if (scp == 0) {
            this.LogASLError("No action name specified in 'action " + data + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        var objName = Trim(Left(param, scp - 1));
        actionName = Trim(Mid(param, scp + 1));
        var ep = InStr(data, ">");
        if (ep == Len(Trim(data))) {
            script = "";
        } else {
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
            this.LogASLError("No such object '" + objName + "' in 'action " + data + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
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
        var result;
        var thisNot;
        if (this.BeginsWith(condition, "not ")) {
            thisNot = true;
            condition = this.GetEverythingAfter(condition, "not ");
        } else {
            thisNot = false;
        }
        if (this.BeginsWith(condition, "got ")) {
            result = this.ExecuteIfGot(this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "has ")) {
            result = this.ExecuteIfHas(this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "ask ")) {
            result = this.ExecuteIfAsk(this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "is ")) {
            result = this.ExecuteIfIs(this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "here ")) {
            result = this.ExecuteIfHere(this.GetParameter(condition, ctx), ctx);
        } else if (this.BeginsWith(condition, "exists ")) {
            result = this.ExecuteIfExists(this.GetParameter(condition, ctx), false);
        } else if (this.BeginsWith(condition, "real ")) {
            result = this.ExecuteIfExists(this.GetParameter(condition, ctx), true);
        } else if (this.BeginsWith(condition, "property ")) {
            result = this.ExecuteIfProperty(this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "action ")) {
            result = this.ExecuteIfAction(this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "type ")) {
            result = this.ExecuteIfType(this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "flag ")) {
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
            var nextConditionPos = InStr(pos, obscuredConditionList, "and ");
            if (nextConditionPos == 0) {
                nextConditionPos = InStr(pos, obscuredConditionList, "or ");
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
            pos = InStr(nextConditionPos, obscuredConditionList, " ");
        } while(!(isFinalCondition));
        operations[0] = "AND";
        var result = true;
        for (var i = 1; i <= numConditions; i++) {
            var thisResult = this.ExecuteCondition(conditions[i], ctx);
            if (operations[i - 1] == "AND") {
                result = thisResult && result;
            } else if (operations[i - 1] == "OR") {
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
                this._rooms[this._numberRooms].Exits = new RoomExits();
                this._rooms[this._numberRooms].Exits.ObjId = this._rooms[this._numberRooms].ObjId;
            }
        } else if (this.BeginsWith(data, "object ")) {
            var paramData = this.GetParameter(data, ctx);
            var scp = InStr(paramData, ";");
            var containerRoom;
            if (scp == 0) {
                newName = paramData;
                containerRoom = "";
            } else {
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
        } else if (this.BeginsWith(data, "exit ")) {
            this.ExecuteCreateExit(data, ctx);
        }
    };
    LegacyGame.prototype.ExecuteCreateExit = function (data, ctx) {
        var scrRoom;
        var destRoom = "";
        var destId;
        var exitData = this.GetEverythingAfter(data, "exit ");
        var newName = this.GetParameter(data, ctx);
        var scp = InStr(newName, ";");
        if (this._gameAslVersion < 410) {
            if (scp == 0) {
                this.LogASLError("No exit destination given in 'create exit " + exitData + "'", 2 /* WarningError */);
                // UNKNOWN ExitSubStatement
            }
        }
        if (scp == 0) {
            scrRoom = Trim(newName);
        } else {
            scrRoom = Trim(Left(newName, scp - 1));
        }
        var srcId = this.GetRoomID(scrRoom, ctx);
        if (srcId == 0) {
            this.LogASLError("No such room '" + scrRoom + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        if (this._gameAslVersion < 410) {
            // only do destination room check for ASL <410, as can now have scripts on dynamically
            // created exits, so the destination doesn't necessarily have to exist.
            destRoom = Trim(Mid(newName, scp + 1));
            if (destRoom != "") {
                destId = this.GetRoomID(destRoom, ctx);
                if (destId == 0) {
                    this.LogASLError("No such room '" + destRoom + "'", 2 /* WarningError */);
                    // UNKNOWN ExitSubStatement
                }
            }
        }

        // If it's a "go to" exit, check if it already exists:
        var exists = false;
        if (this.BeginsWith(exitData, "<")) {
            if (this._gameAslVersion >= 410) {
                exists = this._rooms[srcId].Exits.Places.ContainsKey(destRoom);
            } else {
                for (var i = 1; i <= this._rooms[srcId].NumberPlaces; i++) {
                    if (LCase(this._rooms[srcId].Places[i].PlaceName) == LCase(destRoom)) {
                        exists = true;
                        break;
                    }
                }
            }
            if (exists) {
                this.LogASLError("Exit from '" + scrRoom + "' to '" + destRoom + "' already exists", 2 /* WarningError */);
                // UNKNOWN ExitSubStatement
            }
        }
        var paramPos = InStr(exitData, "<");
        var saveData;
        if (paramPos == 0) {
            saveData = exitData;
        } else {
            saveData = Left(exitData, paramPos - 1);

            // We do this so the changelog doesn't contain unconverted variable names
            saveData = saveData + "<" + this.GetParameter(exitData, ctx) + ">";
        }
        this.AddToChangeLog("room " + this._rooms[srcId].RoomName, "exit " + saveData);
        var r = this._rooms[srcId];
        if (this._gameAslVersion >= 410) {
            r.Exits.AddExitFromCreateScript(exitData, ctx);
        } else {
            if (this.BeginsWith(exitData, "north ")) {
                r.North.Data = destRoom;
                r.North.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "south ")) {
                r.South.Data = destRoom;
                r.South.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "east ")) {
                r.East.Data = destRoom;
                r.East.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "west ")) {
                r.West.Data = destRoom;
                r.West.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "northeast ")) {
                r.NorthEast.Data = destRoom;
                r.NorthEast.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "northwest ")) {
                r.NorthWest.Data = destRoom;
                r.NorthWest.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "southeast ")) {
                r.SouthEast.Data = destRoom;
                r.SouthEast.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "southwest ")) {
                r.SouthWest.Data = destRoom;
                r.SouthWest.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "up ")) {
                r.Up.Data = destRoom;
                r.Up.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "down ")) {
                r.Down.Data = destRoom;
                r.Down.Type = 0 /* Text */;
            } else if (this.BeginsWith(exitData, "out ")) {
                r.Out.Text = destRoom;
            } else if (this.BeginsWith(exitData, "<")) {
                r.NumberPlaces = r.NumberPlaces + 1;
                if (!r.Places)
                    r.Places = [];
                r.Places[r.NumberPlaces] = new PlaceType();
                r.Places[r.NumberPlaces].PlaceName = destRoom;
            } else {
                this.LogASLError("Invalid direction in 'create exit " + exitData + "'", 2 /* WarningError */);
            }
        }
        if (!this._gameLoading) {
            // Update quest.doorways variables
            this.ShowRoomInfo(this._currentRoom, ctx, true);
            this.UpdateObjectList(ctx);
            if (this._gameAslVersion < 410) {
                if (this._currentRoom == this._rooms[srcId].RoomName) {
                    this.UpdateDoorways(srcId, ctx);
                } else if (this._currentRoom == this._rooms[destId].RoomName) {
                    this.UpdateDoorways(destId, ctx);
                }
            } else {
                // Don't have DestID in ASL410 CreateExit code, so just UpdateDoorways
                // for current room anyway.
                this.UpdateDoorways(this.GetRoomID(this._currentRoom, ctx), ctx);
            }
        }
    };
    LegacyGame.prototype.ExecDrop = function (obj, ctx) {
        var found;
        var parentId;
        var id;
        id = this.Disambiguate(obj, "inventory", ctx);
        if (id > 0) {
            found = true;
        } else {
            found = false;
        }
        if (!found) {
            if (id != -2) {
                if (this._gameAslVersion >= 391) {
                    this.PlayerErrorMessage(4 /* NoItem */, ctx);
                } else {
                    this.PlayerErrorMessage(21 /* BadDrop */, ctx);
                }
            }
            this._badCmdBefore = "drop";
            // UNKNOWN ExitSubStatement
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
                } else {
                    parentDisplayName = this._objs[parentId].ObjectName;
                }
                this.Print("(first removing " + this._objs[id].Article + " from " + parentDisplayName + ")", ctx);

                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                this.ExecCommand("remove " + this._objs[id].ObjectName, ctx, false, null, true);
                if (this.GetObjectProperty("parent", id, false, false) != "") {
                    // removing the object failed
                    // UNKNOWN ExitSubStatement
                }
            }
        }
        if (!dropFound) {
            this.PlayerErrorMessage(20 /* DefaultDrop */, ctx);
            this.PlayerItem(this._objs[id].ObjectName, false, ctx);
        } else {
            if (this.BeginsWith(dropStatement, "everywhere")) {
                this.PlayerItem(this._objs[id].ObjectName, false, ctx);
                if (InStr(dropStatement, "<") != 0) {
                    this.Print(this.GetParameter(dropStatement, ctx), ctx);
                } else {
                    this.PlayerErrorMessage(20 /* DefaultDrop */, ctx);
                }
            } else if (this.BeginsWith(dropStatement, "nowhere")) {
                if (InStr(dropStatement, "<") != 0) {
                    this.Print(this.GetParameter(dropStatement, ctx), ctx);
                } else {
                    this.PlayerErrorMessage(19 /* CantDrop */, ctx);
                }
            } else {
                this.ExecuteScript(dropStatement, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecExamine = function (command, ctx) {
        var item = LCase(Trim(this.GetEverythingAfter(command, "examine ")));
        if (item == "") {
            this.PlayerErrorMessage(16 /* BadExamine */, ctx);
            this._badCmdBefore = "examine";
            // UNKNOWN ExitSubStatement
        }
        var id = this.Disambiguate(item, this._currentRoom + ";inventory", ctx);
        if (id <= 0) {
            if (id != -2) {
                this.PlayerErrorMessage(7 /* BadThing */, ctx);
            }
            this._badCmdBefore = "examine";
            // UNKNOWN ExitSubStatement
        }
        var o = this._objs[id];

        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == "examine") {
                this.ExecuteScript(o.Actions[i].Script, ctx, id);
                // UNKNOWN ExitSubStatement
            }
        }

        for (var i = 1; i <= o.NumberProperties; i++) {
            if (o.Properties[i].PropertyName == "examine") {
                this.Print(o.Properties[i].PropertyValue, ctx);
                // UNKNOWN ExitSubStatement
            }
        }

        for (var i = o.DefinitionSectionStart + 1; i <= this._objs[id].DefinitionSectionEnd - 1; i++) {
            if (this.BeginsWith(this._lines[i], "examine ")) {
                var action = Trim(this.GetEverythingAfter(this._lines[i], "examine "));
                if (Left(action, 1) == "<") {
                    this.Print(this.GetParameter(this._lines[i], ctx), ctx);
                } else {
                    this.ExecuteScript(action, ctx, id);
                }
                // UNKNOWN ExitSubStatement
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
        var id;
        var found;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No property data given in 'property <" + data + ">'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
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
            this.LogASLError("No such object in 'property <" + data + ">'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        this.AddToObjectProperties(properties, id, ctx);
    };
    LegacyGame.prototype.ExecuteDo = function (procedureName, ctx) {
        var newCtx = this.CopyContext(ctx);
        var numParameters = 0;
        var useNewCtx;
        if (this._gameAslVersion >= 392 && Left(procedureName, 8) == "!intproc") {
            // If "do" procedure is run in a new context, context info is not passed to any nested
            // script blocks in braces.
            useNewCtx = false;
        } else {
            useNewCtx = true;
        }
        if (this._gameAslVersion >= 284) {
            var obp = InStr(procedureName, "(");
            var cbp;
            if (obp != 0) {
                cbp = InStr(obp + 1, procedureName, ")");
            }
            if (obp != 0 && cbp != 0) {
                var parameters = Mid(procedureName, obp + 1, (cbp - obp) - 1);
                procedureName = Left(procedureName, obp - 1);
                parameters = parameters + ";";
                var pos = 1;
                do {
                    numParameters = numParameters + 1;
                    var scp = InStr(pos, parameters, ";");
                    newCtx.NumParameters = numParameters;
                    if (!newCtx.Parameters)
                        newCtx.Parameters = [];
                    newCtx.Parameters[numParameters] = Trim(Mid(parameters, pos, scp - pos));
                    pos = scp + 1;
                } while(!(pos >= Len(parameters)));
            }
        }
        var block = this.DefineBlockParam("procedure", procedureName);
        if (block.StartLine == 0 && block.EndLine == 0) {
            this.LogASLError("No such procedure " + procedureName, 2 /* WarningError */);
        } else {
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                if (!useNewCtx) {
                    this.ExecuteScript(this._lines[i], ctx);
                } else {
                    this.ExecuteScript(this._lines[i], newCtx);
                    ctx.DontProcessCommand = newCtx.DontProcessCommand;
                }
            }
        }
    };
    LegacyGame.prototype.ExecuteDoAction = function (data, ctx) {
        var id;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No action name specified in 'doaction <" + data + ">'");
            // UNKNOWN ExitSubStatement
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
            // UNKNOWN ExitSubStatement
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
        var result;
        var errorReport = false;
        var scp;
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
        if (found == false && errorReport) {
            this.LogASLError("No such character/object '" + obj + "'.", 6 /* UserError */);
        }
        if (found == false) {
            result = false;
        }
        if (realOnly) {
            return found;
        }
        return result;
    };
    LegacyGame.prototype.ExecuteIfProperty = function (data) {
        var id;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No property name given in condition 'property <" + data + ">' ...", 2 /* WarningError */);
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
            this.LogASLError("No such object '" + objName + "' in condition 'property <" + data + ">' ...", 2 /* WarningError */);
            return false;
        }
        return this.GetObjectProperty(propertyName, id, true) == "yes";
    };
    LegacyGame.prototype.ExecuteRepeat = function (data, ctx) {
        var repeatWhileTrue;
        var repeatScript = "";
        var bracketPos;
        var afterBracket;
        var foundScript = false;
        if (this.BeginsWith(data, "while ")) {
            repeatWhileTrue = true;
            data = this.GetEverythingAfter(data, "while ");
        } else if (this.BeginsWith(data, "until ")) {
            repeatWhileTrue = false;
            data = this.GetEverythingAfter(data, "until ");
        } else {
            this.LogASLError("Expected 'until' or 'while' in 'repeat " + this.ReportErrorLine(data) + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        var pos = 1;
        do {
            bracketPos = InStr(pos, data, ">");
            afterBracket = Trim(Mid(data, bracketPos + 1));
            if ((!this.BeginsWith(afterBracket, "and ")) && (!this.BeginsWith(afterBracket, "or "))) {
                repeatScript = afterBracket;
                foundScript = true;
            } else {
                pos = bracketPos + 1;
            }
        } while(!(foundScript || afterBracket == ""));
        var conditions = Trim(Left(data, bracketPos));
        var finished = false;
        do {
            if (this.ExecuteConditions(conditions, ctx) == repeatWhileTrue) {
                this.ExecuteScript(repeatScript, ctx);
            } else {
                finished = true;
            }
        } while(!(finished || this._gameFinished));
    };
    LegacyGame.prototype.ExecuteSetCollectable = function (param, ctx) {
        var val;
        var id;
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
            this.LogASLError("No such collectable '" + param + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        var op = Left(newVal, 1);
        var newValue = Trim(Right(newVal, Len(newVal) - 1));
        if (IsNumeric(newValue)) {
            val = Conversion.Val(newValue);
        } else {
            val = this.GetCollectableAmount(newValue);
        }
        if (op == "+") {
            this._collectables[id].Value = this._collectables[id].Value + val;
        } else if (op == "-") {
            this._collectables[id].Value = this._collectables[id].Value - val;
        } else if (op == "=") {
            this._collectables[id].Value = val;
        }
        this.CheckCollectable(id);
        this.UpdateItems(ctx);
    };
    LegacyGame.prototype.ExecuteWait = function (waitLine, ctx) {
        if (waitLine != "") {
            this.Print(this.GetParameter(waitLine, ctx), ctx);
        } else {
            if (this._gameAslVersion >= 410) {
                this.PlayerErrorMessage(36 /* DefaultWait */, ctx);
            } else {
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
        var nullPos = InStr(this._fileDataPos, this._fileData, Chr(0));
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
            this.LogASLError("No script given for '" + name + "' action data", 2 /* WarningError */);
            return new ActionType();
        }
        var script = Trim(Mid(actionInfo, ep + 1));
        var result = new ActionType();
        result.ActionName = name;
        result.Script = script;
        return result;
    };
    LegacyGame.prototype.GetObjectId = function (name, ctx, room) {
        if (typeof room === "undefined") { room = ""; }
        var id;
        var found = false;
        if (this.BeginsWith(name, "the ")) {
            name = this.GetEverythingAfter(name, "the ");
        }
        for (var i = 1; i <= this._numberObjs; i++) {
            if ((LCase(this._objs[i].ObjectName) == LCase(name) || LCase(this._objs[i].ObjectName) == "the " + LCase(name)) && (LCase(this._objs[i].ContainerRoom) == LCase(room) || room == "") && this._objs[i].Exists == true) {
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
        if (typeof existsOnly === "undefined") { existsOnly = false; }
        if (typeof logError === "undefined") { logError = true; }
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
            this.LogASLError("Object '" + this._objs[id].ObjectName + "' has no property '" + name + "'", 2 /* WarningError */);
            return "!";
        }
        return "";
    };
    LegacyGame.prototype.GetPropertiesInType = function (type, err) {
        if (typeof err === "undefined") { err = true; }
        var blockId;
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
                this.LogASLError("No such type '" + type + "'", 2 /* WarningError */);
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
            } else if (this.BeginsWith(this._lines[i], "action ")) {
                propertyList.NumberActions = propertyList.NumberActions + 1;
                if (!propertyList.Actions)
                    propertyList.Actions = [];
                propertyList.Actions[propertyList.NumberActions] = this.GetObjectActions(this.GetEverythingAfter(this._lines[i], "action "));
            } else if (this.BeginsWith(this._lines[i], "properties ")) {
                propertyList.Properties = propertyList.Properties + this.GetParameter(this._lines[i], this._nullContext) + ";";
            } else if (Trim(this._lines[i]) != "") {
                propertyList.Properties = propertyList.Properties + this._lines[i] + ";";
            }
        }
        return propertyList;
    };
    LegacyGame.prototype.GetRoomID = function (name, ctx) {
        if (InStr(name, "[") > 0) {
            var idx = this.GetArrayIndex(name, ctx);
            name = name + Trim(Str(idx));
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
            result.Type = 0 /* Text */;
            result.Data = this.GetParameter(textScript, this._nullContext);
        } else {
            result.Type = 1 /* Script */;
            result.Data = textScript;
        }
        return result;
    };
    LegacyGame.prototype.GetThingNumber = function (name, room, type) {
        // Returns the number in the Chars() or _objs() array of the specified char/obj
        if (type == 0 /* Character */) {
            for (var i = 1; i <= this._numberChars; i++) {
                if ((room != "" && LCase(this._chars[i].ObjectName) == LCase(name) && LCase(this._chars[i].ContainerRoom) == LCase(room)) || (room == "" && LCase(this._chars[i].ObjectName) == LCase(name))) {
                    return i;
                }
            }
        } else if (type == 1 /* Object */) {
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
        if (type == 0 /* Character */) {
            for (var i = 1; i <= this._numberChars; i++) {
                if (LCase(this._chars[i].ObjectName) == LCase(name) && LCase(this._chars[i].ContainerRoom) == LCase(room)) {
                    result.StartLine = this._chars[i].DefinitionSectionStart;
                    result.EndLine = this._chars[i].DefinitionSectionEnd;
                    return result;
                }
            }
        } else if (type == 1 /* Object */) {
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
        var data = {};
        var objectData = [];
        var roomData = [];
        var numObjectData;
        var numRoomData;

        // <<< FILE HEADER DATA >>>
        data.Append("QUEST300" + Chr(0) + this.GetOriginalFilenameForQSG() + Chr(0));

        // The start point for encrypted data is after the filename
        var start = data.Length + 1;
        data.Append(this._currentRoom + Chr(0));

        for (var i = 1; i <= this._gameChangeData.NumberChanges; i++) {
            if (this.BeginsWith(this._gameChangeData.ChangeData[i].AppliesTo, "object ")) {
                numObjectData = numObjectData + 1;
                if (!objectData)
                    objectData = [];
                objectData[numObjectData] = new ChangeType();
                objectData[numObjectData] = this._gameChangeData.ChangeData[i];
            } else if (this.BeginsWith(this._gameChangeData.ChangeData[i].AppliesTo, "room ")) {
                numRoomData = numRoomData + 1;
                if (!roomData)
                    roomData = [];
                roomData[numRoomData] = new ChangeType();
                roomData[numRoomData] = this._gameChangeData.ChangeData[i];
            }
        }

        // <<< OBJECT CREATE/CHANGE DATA >>>
        data.Append(Trim(Str(numObjectData + this._changeLogObjects.Changes.Count)) + Chr(0));
        for (var i = 1; i <= numObjectData; i++) {
            data.Append(this.GetEverythingAfter(objectData[i].AppliesTo, "object ") + Chr(0) + objectData[i].Change + Chr(0));
        }
        this._changeLogObjects.Changes.Keys.forEach(function (key) {
            var appliesTo = Split(key, "#")(0);
            var changeData = this._changeLogObjects.Changes.Item(key);
            data.Append(appliesTo + Chr(0) + changeData + Chr(0));
        }, this);

        // <<< OBJECT EXIST/VISIBLE/ROOM DATA >>>
        data.Append(Trim(Str(this._numberObjs)) + Chr(0));
        for (var i = 1; i <= this._numberObjs; i++) {
            var exists;
            var visible;
            if (this._objs[i].Exists) {
                exists = Chr(1);
            } else {
                exists = Chr(0);
            }
            if (this._objs[i].Visible) {
                visible = Chr(1);
            } else {
                visible = Chr(0);
            }
            data.Append(this._objs[i].ObjectName + Chr(0) + exists + visible + this._objs[i].ContainerRoom + Chr(0));
        }

        // <<< ROOM CREATE/CHANGE DATA >>>
        data.Append(Trim(Str(numRoomData)) + Chr(0));
        for (var i = 1; i <= numRoomData; i++) {
            data.Append(this.GetEverythingAfter(roomData[i].AppliesTo, "room ") + Chr(0) + roomData[i].Change + Chr(0));
        }

        // <<< TIMER STATE DATA >>>
        data.Append(Trim(Str(this._numberTimers)) + Chr(0));
        for (var i = 1; i <= this._numberTimers; i++) {
            var t = this._timers[i];
            data.Append(t.TimerName + Chr(0));
            if (t.TimerActive) {
                data.Append(Chr(1));
            } else {
                data.Append(Chr(0));
            }
            data.Append(Trim(Str(t.TimerInterval)) + Chr(0));
            data.Append(Trim(Str(t.TimerTicks)) + Chr(0));
        }

        // <<< STRING VARIABLE DATA >>>
        data.Append(Trim(Str(this._numberStringVariables)) + Chr(0));
        for (var i = 1; i <= this._numberStringVariables; i++) {
            var s = this._stringVariable[i];
            data.Append(s.VariableName + Chr(0) + Trim(Str(s.VariableUBound)) + Chr(0));
            for (var j = 0; j <= s.VariableUBound; j++) {
                data.Append(s.VariableContents[j] + Chr(0));
            }
        }

        // <<< NUMERIC VARIABLE DATA >>>
        data.Append(Trim(Str(this._numberNumericVariables)) + Chr(0));
        for (var i = 1; i <= this._numberNumericVariables; i++) {
            var n = this._numericVariable[i];
            data.Append(n.VariableName + Chr(0) + Trim(Str(n.VariableUBound)) + Chr(0));
            for (var j = 0; j <= n.VariableUBound; j++) {
                data.Append(n.VariableContents[j] + Chr(0));
            }
        }

        // Now encrypt data
        var dataString;
        var newFileData = {};
        dataString = data.ToString();
        newFileData.Append(Left(dataString, start - 1));
        for (var i = start; i <= Len(dataString); i++) {
            newFileData.Append(Chr(255 - Asc(Mid(dataString, i, 1))));
        }
        return newFileData.ToString();
    };
    LegacyGame.prototype.MoveThing = function (name, room, type, ctx) {
        var oldRoom = "";
        var id = this.GetThingNumber(name, "", type);
        if (InStr(room, "[") > 0) {
            var idx = this.GetArrayIndex(room, ctx);
            room = room + Trim(Str(idx));
        }
        if (type == 0 /* Character */) {
            this._chars[id].ContainerRoom = room;
        } else if (type == 1 /* Object */) {
            oldRoom = this._objs[id].ContainerRoom;
            this._objs[id].ContainerRoom = room;
        }
        if (this._gameAslVersion >= 391) {
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
    LegacyGame.prototype.Pause = function (duration) {
        this._player.DoPause(duration);
        this.ChangeState(2 /* Waiting */);
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.ConvertParameter = function (parameter, convertChar, action, ctx) {
        // Returns a string with functions, string and
        // numeric variables executed or converted as
        // appropriate, read for display/etc.
        var result = "";
        var pos = 1;
        var finished = false;
        do {
            var varPos = InStr(pos, parameter, convertChar);
            if (varPos == 0) {
                varPos = Len(parameter) + 1;
                finished = true;
            }
            var currentBit = Mid(parameter, pos, varPos - pos);
            result = result + currentBit;
            if (finished == false) {
                var nextPos = InStr(varPos + 1, parameter, convertChar);
                if (nextPos == 0) {
                    this.LogASLError("Line parameter <" + parameter + "> has missing " + convertChar, 2 /* WarningError */);
                    return "<ERROR>";
                }
                var varName = Mid(parameter, varPos + 1, (nextPos - 1) - varPos);
                if (varName == "") {
                    result = result + convertChar;
                } else {
                    if (action == 0 /* Strings */) {
                        result = result + this.GetStringContents(varName, ctx);
                    } else if (action == 1 /* Functions */) {
                        varName = this.EvaluateInlineExpressions(varName);
                        result = result + this.DoFunction(varName, ctx);
                    } else if (action == 2 /* Numeric */) {
                        result = result + Trim(Str(this.GetNumericContents(varName, ctx)));
                    } else if (action == 3 /* Collectables */) {
                        result = result + Trim(Str(this.GetCollectableAmount(varName)));
                    }
                }
                pos = nextPos + 1;
            }
        } while(!(finished));
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
                this.LogASLError("Expected ) in $" + data + "$", 2 /* WarningError */);
                return "";
            }
            parameter = Mid(data, paramPos + 1, (endParamPos - paramPos) - 1);
        } else {
            name = data;
            parameter = "";
        }
        var block;
        block = this.DefineBlockParam("function", name);
        if (block.StartLine == 0 && block.EndLine == 0) {
            //Function does not exist; try an internal function.
            intFuncResult = this.DoInternalFunction(name, parameter, ctx);
            if (intFuncResult == "__NOTDEFINED") {
                this.LogASLError("No such function '" + name + "'", 2 /* WarningError */);
                return "[ERROR]";
            } else {
                intFuncExecuted = true;
            }
        }
        if (intFuncExecuted) {
            return intFuncResult;
        } else {
            var newCtx = this.CopyContext(ctx);
            var numParameters = 0;
            var pos = 1;
            if (parameter != "") {
                parameter = parameter + ";";
                do {
                    numParameters = numParameters + 1;
                    var scp = InStr(pos, parameter, ";");
                    var parameterData = Trim(Mid(parameter, pos, scp - pos));
                    this.SetStringContents("quest.function.parameter." + Trim(Str(numParameters)), parameterData, ctx);
                    newCtx.NumParameters = numParameters;
                    if (!newCtx.Parameters)
                        newCtx.Parameters = [];
                    newCtx.Parameters[numParameters] = parameterData;
                    pos = scp + 1;
                } while(!(pos >= Len(parameter)));
                this.SetStringContents("quest.function.numparameters", Trim(Str(numParameters)), ctx);
            } else {
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
        var objId;
        var numParameters = 0;
        var pos = 1;
        if (parameter != "") {
            parameter = parameter + ";";
            do {
                numParameters = numParameters + 1;
                var scp = InStr(pos, parameter, ";");
                if (!parameters)
                    parameters = [];
                if (!untrimmedParameters)
                    untrimmedParameters = [];
                untrimmedParameters[numParameters] = Mid(parameter, pos, scp - pos);
                parameters[numParameters] = Trim(untrimmedParameters[numParameters]);
                pos = scp + 1;
            } while(!(pos >= Len(parameter)));

            // Remove final ";"
            parameter = Left(parameter, Len(parameter) - 1);
        } else {
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
                this.LogASLError("Object '" + parameters[1] + "' does not exist", 2 /* WarningError */);
                return "!";
            } else {
                return this._objs[this.GetObjectId(parameters[1], ctx)].ObjectAlias;
            }
        } else if (name == "numberparameters") {
            return Trim(Str(ctx.NumParameters));
        } else if (name == "parameter") {
            if (numParameters == 0) {
                this.LogASLError("No parameter number specified for $parameter$ function", 2 /* WarningError */);
                return "";
            } else {
                if (Val(parameters[1]) > ctx.NumParameters) {
                    this.LogASLError("No parameter number " + parameters[1] + " sent to this function", 2 /* WarningError */);
                    return "";
                } else {
                    return Trim(ctx.Parameters[parseInt(parameters[1])]);
                }
            }
        } else if (name == "gettag") {
            // Deprecated
            return this.FindStatement(this.DefineBlockParam("room", parameters[1]), parameters[2]);
        } else if (name == "objectname") {
            return this._objs[ctx.CallingObjectId].ObjectName;
        } else if (name == "locationof") {
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
        } else if (name == "lengthof") {
            return Str(Len(untrimmedParameters[1]));
        } else if (name == "left") {
            if (Val(parameters[2]) < 0) {
                this.LogASLError("Invalid function call in '$Left$(" + parameters[1] + "; " + parameters[2] + ")$'", 2 /* WarningError */);
                return "!";
            } else {
                return Left(parameters[1], parseInt(parameters[2]));
            }
        } else if (name == "right") {
            if (Val(parameters[2]) < 0) {
                this.LogASLError("Invalid function call in '$Right$(" + parameters[1] + "; " + parameters[2] + ")$'", 2 /* WarningError */);
                return "!";
            } else {
                return Right(parameters[1], parseInt(parameters[2]));
            }
        } else if (name == "mid") {
            if (numParameters == 3) {
                if (Val(parameters[2]) < 0) {
                    this.LogASLError("Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + "; " + parameters[3] + ")$'", 2 /* WarningError */);
                    return "!";
                } else {
                    return Mid(parameters[1], parseInt(parameters[2]), parseInt(parameters[3]));
                }
            } else if (numParameters == 2) {
                if (Val(parameters[2]) < 0) {
                    this.LogASLError("Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + ")$'", 2 /* WarningError */);
                    return "!";
                } else {
                    return Mid(parameters[1], parseInt(parameters[2]));
                }
            }
            this.LogASLError("Invalid function call to '$Mid$(...)$'", 2 /* WarningError */);
            return "";
        } else if (name == "rand") {
            return Str(Int(this._random.NextDouble() * (parseFloat(parameters[2]) - parseFloat(parameters[1]) + 1)) + parseFloat(parameters[1]));
        } else if (name == "instr") {
            if (numParameters == 3) {
                param3 = "";
                if (InStr(parameters[3], "_") != 0) {
                    for (var i = 1; i <= Len(parameters[3]); i++) {
                        if (Mid(parameters[3], i, 1) == "_") {
                            param3 = param3 + " ";
                        } else {
                            param3 = param3 + Mid(parameters[3], i, 1);
                        }
                    }
                } else {
                    param3 = parameters[3];
                }
                if (Val(parameters[1]) <= 0) {
                    this.LogASLError("Invalid function call in '$instr(" + parameters[1] + "; " + parameters[2] + "; " + parameters[3] + ")$'", 2 /* WarningError */);
                    return "!";
                } else {
                    return Trim(Str(InStr(parseInt(parameters[1]), parameters[2], param3)));
                }
            } else if (numParameters == 2) {
                param2 = "";
                if (InStr(parameters[2], "_") != 0) {
                    for (var i = 1; i <= Len(parameters[2]); i++) {
                        if (Mid(parameters[2], i, 1) == "_") {
                            param2 = param2 + " ";
                        } else {
                            param2 = param2 + Mid(parameters[2], i, 1);
                        }
                    }
                } else {
                    param2 = parameters[2];
                }
                return Trim(Str(InStr(parameters[1], param2)));
            }
            this.LogASLError("Invalid function call to '$Instr$(...)$'", 2 /* WarningError */);
            return "";
        } else if (name == "ucase") {
            return UCase(parameters[1]);
        } else if (name == "lcase") {
            return LCase(parameters[1]);
        } else if (name == "capfirst") {
            return UCase(Left(parameters[1], 1)) + Mid(parameters[1], 2);
        } else if (name == "symbol") {
            if (parameters[1] == "lt") {
                return "<";
            } else if (parameters[1] == "gt") {
                return ">";
            } else {
                return "!";
            }
        } else if (name == "loadmethod") {
            return this._gameLoadMethod;
        } else if (name == "timerstate") {
            for (var i = 1; i <= this._numberTimers; i++) {
                if (LCase(this._timers[i].TimerName) == LCase(parameters[1])) {
                    if (this._timers[i].TimerActive) {
                        return "1";
                    } else {
                        return "0";
                    }
                }
            }
            this.LogASLError("No such timer '" + parameters[1] + "'", 2 /* WarningError */);
            return "!";
        } else if (name == "timerinterval") {
            for (var i = 1; i <= this._numberTimers; i++) {
                if (LCase(this._timers[i].TimerName) == LCase(parameters[1])) {
                    return Str(this._timers[i].TimerInterval);
                }
            }
            this.LogASLError("No such timer '" + parameters[1] + "'", 2 /* WarningError */);
            return "!";
        } else if (name == "ubound") {
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
            this.LogASLError("No such variable '" + parameters[1] + "'", 2 /* WarningError */);
            return "!";
        } else if (name == "objectproperty") {
            var FoundObj = false;
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(parameters[1])) {
                    FoundObj = true;
                    objId = i;
                    i = this._numberObjs;
                }
            }
            if (!FoundObj) {
                this.LogASLError("No such object '" + parameters[1] + "'", 2 /* WarningError */);
                return "!";
            } else {
                return this.GetObjectProperty(parameters[2], objId);
            }
        } else if (name == "getobjectname") {
            if (numParameters == 3) {
                objId = this.Disambiguate(parameters[1], parameters[2] + ";" + parameters[3], ctx);
            } else if (numParameters == 2) {
                objId = this.Disambiguate(parameters[1], parameters[2], ctx);
            } else {
                objId = this.Disambiguate(parameters[1], this._currentRoom + ";inventory", ctx);
            }
            if (objId <= -1) {
                this.LogASLError("No object found with display name '" + parameters[1] + "'", 2 /* WarningError */);
                return "!";
            } else {
                return this._objs[objId].ObjectName;
            }
        } else if (name == "thisobject") {
            return this._objs[ctx.CallingObjectId].ObjectName;
        } else if (name == "thisobjectname") {
            return this._objs[ctx.CallingObjectId].ObjectAlias;
        } else if (name == "speechenabled") {
            return "1";
        } else if (name == "removeformatting") {
            return this.RemoveFormatting(parameter);
        } else if (name == "findexit" && this._gameAslVersion >= 410) {
            var e = this.FindExit(parameter);
            if (e == null) {
                return "";
            } else {
                return this._objs[e.ObjId].ObjectName;
            }
        }
        return "__NOTDEFINED";
    };
    LegacyGame.prototype.ExecFor = function (line, ctx) {
        // See if this is a "for each" loop:
        if (this.BeginsWith(line, "each ")) {
            this.ExecForEach(this.GetEverythingAfter(line, "each "), ctx);
            // UNKNOWN ExitSubStatement
        }

        // Executes a for loop, of form:
        //   for <variable; startvalue; endvalue> script
        var endValue;
        var stepValue;
        var forData = this.GetParameter(line, ctx);

        // Extract individual components:
        var scp1 = InStr(forData, ";");
        var scp2 = InStr(scp1 + 1, forData, ";");
        var scp3 = InStr(scp2 + 1, forData, ";");
        var counterVariable = Trim(Left(forData, scp1 - 1));
        var startValue = parseInt(Mid(forData, scp1 + 1, (scp2 - 1) - scp1));
        if (scp3 != 0) {
            endValue = parseInt(Mid(forData, scp2 + 1, (scp3 - 1) - scp2));
            stepValue = parseInt(Mid(forData, scp3 + 1));
        } else {
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
            this.LogASLError("Invalid numeric variable name '" + idx.Name + "' - variable names cannot be numeric", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        // UNKNOWN TryBlock
    };
    LegacyGame.prototype.ExecuteIfAsk = function (question) {
        this._player.ShowQuestion(question);
        this.ChangeState(2 /* Waiting */);

        // UNKNOWN SyncLockBlock
        return this._questionResponse;
    };
    LegacyGame.prototype.SetQuestionResponse = function (response) {
        var runnerThread = {};
        this.ChangeState(1 /* Working */);
        runnerThread.Start(response);
        this.WaitForStateChange(1 /* Working */);
    };
    LegacyGame.prototype.SetQuestionResponseInNewThread = function (response) {
        this._questionResponse = response;
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.ExecuteIfGot = function (item) {
        if (this._gameAslVersion >= 280) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(item)) {
                    return this._objs[i].ContainerRoom == "inventory" && this._objs[i].Exists;
                }
            }
            this.LogASLError("No object '" + item + "' defined.", 2 /* WarningError */);
            return false;
        }
        for (var i = 1; i <= this._numberItems; i++) {
            if (LCase(this._items[i].Name) == LCase(item)) {
                return this._items[i].Got;
            }
        }
        this.LogASLError("Item '" + item + "' not defined.", 2 /* WarningError */);
        return false;
    };
    LegacyGame.prototype.ExecuteIfHas = function (condition) {
        var checkValue;
        var colNum;
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
            this.LogASLError("No such collectable in " + condition, 2 /* WarningError */);
            // UNKNOWN ExitFunctionStatement
        }
        var op = Left(newVal, 1);
        var newValue = Trim(Right(newVal, Len(newVal) - 1));
        if (IsNumeric(newValue)) {
            checkValue = Val(newValue);
        } else {
            checkValue = this.GetCollectableAmount(newValue);
        }
        if (op == "+") {
            return this._collectables[colNum].Value > checkValue;
        } else if (op == "-") {
            return this._collectables[colNum].Value < checkValue;
        } else if (op == "=") {
            return this._collectables[colNum].Value == checkValue;
        }
        return false;
    };
    LegacyGame.prototype.ExecuteIfIs = function (condition) {
        var value1;
        var value2;
        var op;
        var expectNumerics;
        var expResult;
        var scp = InStr(condition, ";");
        if (scp == 0) {
            this.LogASLError("Expected second parameter in 'is " + condition + "'", 2 /* WarningError */);
            return false;
        }
        var scp2 = InStr(scp + 1, condition, ";");
        if (scp2 == 0) {
            // Only two parameters => standard "="
            op = "=";
            value1 = Trim(Left(condition, scp - 1));
            value2 = Trim(Mid(condition, scp + 1));
        } else {
            value1 = Trim(Left(condition, scp - 1));
            op = Trim(Mid(condition, scp + 1, (scp2 - scp) - 1));
            value2 = Trim(Mid(condition, scp2 + 1));
        }
        if (this._gameAslVersion >= 391) {
            // Evaluate expressions in Value1 and Value2
            expResult = this.ExpressionHandler(value1);
            if (expResult.Success == 0 /* OK */) {
                value1 = expResult.Result;
            }
            expResult = this.ExpressionHandler(value2);
            if (expResult.Success == 0 /* OK */) {
                value2 = expResult.Result;
            }
        }
        var result = false;

        // UNKNOWN SelectBlock
        if (expectNumerics) {
            if (!(IsNumeric(value1) && IsNumeric(value2))) {
                this.LogASLError("Expected numeric comparison comparing '" + value1 + "' and '" + value2 + "'", 2 /* WarningError */);
            }
        }
        return result;
    };
    LegacyGame.prototype.GetNumericContents = function (name, ctx, noError) {
        if (typeof noError === "undefined") { noError = false; }
        var numNumber;
        var arrayIndex;
        var exists = false;

        // First, see if the variable already exists. If it
        // does, get its contents. If not, generate an error.
        if (InStr(name, "[") != 0 && InStr(name, "]") != 0) {
            var op = InStr(name, "[");
            var cp = InStr(name, "]");
            var arrayIndexData = Mid(name, op + 1, (cp - op) - 1);
            if (IsNumeric(arrayIndexData)) {
                arrayIndex = parseInt(arrayIndexData);
            } else {
                arrayIndex = parseInt(this.GetNumericContents(arrayIndexData, ctx));
            }
            name = Left(name, op - 1);
        } else {
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
                this.LogASLError("No numeric variable '" + name + "' defined.", 2 /* WarningError */);
            }
            return -32767;
        }
        if (arrayIndex > this._numericVariable[numNumber].VariableUBound) {
            if (!noError) {
                this.LogASLError("Array index of '" + name + "[" + Trim(Str(arrayIndex)) + "]' too big.", 2 /* WarningError */);
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
        return this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(this._playerErrorMessageString[e], "%", 2 /* Numeric */, ctx), "$", 1 /* Functions */, ctx), "#", 0 /* Strings */, ctx);
    };
    LegacyGame.prototype.PlayMedia = function (filename) {
        this.PlayMedia(filename, false, false);
    };
    LegacyGame.prototype.PlayMedia = function (filename, sync, looped) {
        if (filename.Length == 0) {
            this._player.StopSound();
        } else {
            if (looped && sync) {
                sync = false;
            }

            // Can't loop and sync at the same time, that would just hang!
            this._player.PlaySound(filename, sync, looped);
            if (sync) {
                this.ChangeState(2 /* Waiting */);
            }
            if (sync) {
                // UNKNOWN SyncLockBlock
            }
        }
    };
    LegacyGame.prototype.PlayWav = function (parameter) {
        var sync = false;
        var looped = false;
        var params = {};
        params = new any();
        var filename = params[0];
        if (params.Contains("loop")) {
            looped = true;
        }
        if (params.Contains("sync")) {
            sync = true;
        }
        if (filename.Length > 0 && InStr(filename, ".") == 0) {
            filename = filename + ".wav";
        }
        this.PlayMedia(filename, sync, looped);
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

        for (var i = 1; i <= Len(fileData); i++) {
            decryptedFile.Append(Chr(255 - Asc(Mid(fileData, i, 1))));
        }
        this._fileData = decryptedFile.ToString();
        this._currentRoom = this.GetNextChunk();

        // OBJECTS
        var numData = parseInt(this.GetNextChunk());
        var createdObjects = {};
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
            } else if (this.BeginsWith(data, "create ")) {
                var createData = appliesTo + ";" + this.GetEverythingAfter(data, "create ");

                // workaround bug where duplicate "create" entries appear in the restore data
                if (!createdObjects.Contains(createData)) {
                    this.ExecuteCreate("object <" + createData + ">", this._nullContext);
                    createdObjects.Add(createData);
                }
            } else {
                this.LogASLError("QSG Error: Unrecognised item '" + appliesTo + "; " + data + "'", 7 /* InternalError */);
            }
        }
        numData = parseInt(this.GetNextChunk());
        for (var i = 1; i <= numData; i++) {
            appliesTo = this.GetNextChunk();
            data = this.GetFileDataChars(2);
            objId = this.GetObjectIdNoAlias(appliesTo);
            if (Left(data, 1) == Chr(1)) {
                this._objs[objId].Exists = true;
            } else {
                this._objs[objId].Exists = false;
            }
            if (Right(data, 1) == Chr(1)) {
                this._objs[objId].Visible = true;
            } else {
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
            } else if (data == "create") {
                this.ExecuteCreate("room <" + appliesTo + ">", this._nullContext);
            } else if (this.BeginsWith(data, "destroy exit ")) {
                this.DestroyExit(appliesTo + "; " + this.GetEverythingAfter(data, "destroy exit "), this._nullContext);
            }
        }

        for (var i = 1; i <= numStoredData; i++) {
            var d = storedData[i];
            if (this.BeginsWith(d.Change, "properties ")) {
                this.AddToObjectProperties(this.GetEverythingAfter(d.Change, "properties "), this.GetObjectIdNoAlias(d.AppliesTo), this._nullContext);
            } else if (this.BeginsWith(d.Change, "action ")) {
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
                } else {
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
            } else {
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
            } else {
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
        this._playerErrorMessageString[0 /* BadCommand */] = "I don't understand your command. Type HELP for a list of valid commands.";
        this._playerErrorMessageString[1 /* BadGo */] = "I don't understand your use of 'GO' - you must either GO in some direction, or GO TO a place.";
        this._playerErrorMessageString[2 /* BadGive */] = "You didn't say who you wanted to give that to.";
        this._playerErrorMessageString[3 /* BadCharacter */] = "I can't see anybody of that name here.";
        this._playerErrorMessageString[4 /* NoItem */] = "You don't have that.";
        this._playerErrorMessageString[5 /* ItemUnwanted */] = "#quest.error.gender# doesn't want #quest.error.article#.";
        this._playerErrorMessageString[6 /* BadLook */] = "You didn't say what you wanted to look at.";
        this._playerErrorMessageString[7 /* BadThing */] = "I can't see that here.";
        this._playerErrorMessageString[8 /* DefaultLook */] = "Nothing out of the ordinary.";
        this._playerErrorMessageString[9 /* DefaultSpeak */] = "#quest.error.gender# says nothing.";
        this._playerErrorMessageString[10 /* BadItem */] = "I can't see that anywhere.";
        this._playerErrorMessageString[11 /* DefaultTake */] = "You pick #quest.error.article# up.";
        this._playerErrorMessageString[12 /* BadUse */] = "You didn't say what you wanted to use that on.";
        this._playerErrorMessageString[13 /* DefaultUse */] = "You can't use that here.";
        this._playerErrorMessageString[14 /* DefaultOut */] = "There's nowhere you can go out to around here.";
        this._playerErrorMessageString[15 /* BadPlace */] = "You can't go there.";
        this._playerErrorMessageString[17 /* DefaultExamine */] = "Nothing out of the ordinary.";
        this._playerErrorMessageString[18 /* BadTake */] = "You can't take #quest.error.article#.";
        this._playerErrorMessageString[19 /* CantDrop */] = "You can't drop that here.";
        this._playerErrorMessageString[20 /* DefaultDrop */] = "You drop #quest.error.article#.";
        this._playerErrorMessageString[21 /* BadDrop */] = "You are not carrying such a thing.";
        this._playerErrorMessageString[22 /* BadPronoun */] = "I don't know what '#quest.error.pronoun#' you are referring to.";
        this._playerErrorMessageString[16 /* BadExamine */] = "You didn't say what you wanted to examine.";
        this._playerErrorMessageString[23 /* AlreadyOpen */] = "It is already open.";
        this._playerErrorMessageString[24 /* AlreadyClosed */] = "It is already closed.";
        this._playerErrorMessageString[25 /* CantOpen */] = "You can't open that.";
        this._playerErrorMessageString[26 /* CantClose */] = "You can't close that.";
        this._playerErrorMessageString[27 /* DefaultOpen */] = "You open it.";
        this._playerErrorMessageString[28 /* DefaultClose */] = "You close it.";
        this._playerErrorMessageString[29 /* BadPut */] = "You didn't specify what you wanted to put #quest.error.article# on or in.";
        this._playerErrorMessageString[30 /* CantPut */] = "You can't put that there.";
        this._playerErrorMessageString[31 /* DefaultPut */] = "Done.";
        this._playerErrorMessageString[32 /* CantRemove */] = "You can't remove that.";
        this._playerErrorMessageString[33 /* AlreadyPut */] = "It is already there.";
        this._playerErrorMessageString[34 /* DefaultRemove */] = "Done.";
        this._playerErrorMessageString[35 /* Locked */] = "The exit is locked.";
        this._playerErrorMessageString[36 /* DefaultWait */] = "Press a key to continue...";
        this._playerErrorMessageString[37 /* AlreadyTaken */] = "You already have that.";
    };
    LegacyGame.prototype.SetFont = function (name) {
        if (name == "") {
            name = this._defaultFontName;
        }
        this._player.SetFont(name);
    };
    LegacyGame.prototype.SetFontSize = function (size) {
        if (size == 0) {
            size = this._defaultFontSize;
        }
        this._player.SetFontSize((size).toString());
    };
    LegacyGame.prototype.SetNumericVariableContents = function (name, content, ctx, arrayIndex) {
        if (typeof arrayIndex === "undefined") { arrayIndex = 0; }
        var numNumber;
        var exists = false;
        if (IsNumeric(name)) {
            this.LogASLError("Illegal numeric variable name '" + name + "' - check you didn't put % around the variable name in the ASL code", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
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
        if (exists == false) {
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
        } else {
            cmd = "close";
        }
        var id = this.GetObjectIdNoAlias(name);
        if (id == 0) {
            this.LogASLError("Invalid object name specified in '" + cmd + " <" + name + ">", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
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
        this.LogASLError("No such timer '" + name + "'", 2 /* WarningError */);
    };
    LegacyGame.prototype.SetUnknownVariableType = function (variableData, ctx) {
        var scp = InStr(variableData, ";");
        if (scp == 0) {
            return 0 /* Error */;
        }
        var name = Trim(Left(variableData, scp - 1));
        if (InStr(name, "[") != 0 && InStr(name, "]") != 0) {
            var pos = InStr(name, "[");
            name = Left(name, pos - 1);
        }
        var contents = Trim(Mid(variableData, scp + 1));
        for (var i = 1; i <= this._numberStringVariables; i++) {
            if (LCase(this._stringVariable[i].VariableName) == LCase(name)) {
                this.ExecSetString(variableData, ctx);
                return 1 /* Found */;
            }
        }
        for (var i = 1; i <= this._numberNumericVariables; i++) {
            if (LCase(this._numericVariable[i].VariableName) == LCase(name)) {
                this.ExecSetVar(variableData, ctx);
                return 1 /* Found */;
            }
        }
        for (var i = 1; i <= this._numCollectables; i++) {
            if (LCase(this._collectables[i].Name) == LCase(name)) {
                this.ExecuteSetCollectable(variableData, ctx);
                return 1 /* Found */;
            }
        }
        return 2 /* Unfound */;
    };
    LegacyGame.prototype.SetUpChoiceForm = function (blockName, ctx) {
        // Returns script to execute from choice block
        var block = this.DefineBlockParam("selection", blockName);
        var prompt = this.FindStatement(block, "info");
        var menuOptions = {};
        var menuScript = {};
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "choice ")) {
                menuOptions.Add((i).toString(), this.GetParameter(this._lines[i], ctx));
                menuScript.Add((i).toString(), Trim(Right(this._lines[i], Len(this._lines[i]) - InStr(this._lines[i], ">"))));
            }
        }
        this.Print("- |i" + prompt + "|xi", ctx);
        var mnu = new MenuData();
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
            } else if (this.BeginsWith(this._lines[i], "default fontsize ")) {
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
                            this.LogASLError("Unrecognised variable type in variable '" + variable.VariableName + "' - type '" + type + "'", 2 /* WarningError */);
                            // UNKNOWN ExitDoStatement
                        }
                    } else if (this.BeginsWith(this._lines[i], "onchange ")) {
                        variable.OnChangeScript = this.GetEverythingAfter(this._lines[i], "onchange ");
                    } else if (this.BeginsWith(this._lines[i], "display ")) {
                        var displayString = this.GetEverythingAfter(this._lines[i], "display ");
                        if (this.BeginsWith(displayString, "nozero ")) {
                            variable.NoZeroDisplay = true;
                        }
                        variable.DisplayString = this.GetParameter(this._lines[i], this._nullContext, false);
                    } else if (this.BeginsWith(this._lines[i], "value ")) {
                        variable.VariableContents[0] = this.GetParameter(this._lines[i], this._nullContext);
                    }
                } while(!(Trim(this._lines[i]) == "end define"));
                if (type == "string") {
                    // Create string variable
                    this._numberStringVariables = this._numberStringVariables + 1;
                    var id = this._numberStringVariables;
                    if (!this._stringVariable)
                        this._stringVariable = [];
                    this._stringVariable[id] = variable;
                    this._numDisplayStrings = this._numDisplayStrings + 1;
                } else if (type == "numeric") {
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
                } else if (this.BeginsWith(this._lines[i], "properties ")) {
                    this.AddToObjectProperties(this.GetParameter(this._lines[i], this._nullContext), this._numberObjs, this._nullContext);
                } else if (this.BeginsWith(this._lines[i], "type ")) {
                    o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                    if (!o.TypesIncluded)
                        o.TypesIncluded = [];
                    o.TypesIncluded[o.NumberTypesIncluded] = this.GetParameter(this._lines[i], this._nullContext);
                    var propertyData = this.GetPropertiesInType(this.GetParameter(this._lines[i], this._nullContext));
                    this.AddToObjectProperties(propertyData.Properties, this._numberObjs, this._nullContext);
                    for (var k = 1; k <= propertyData.NumberActions; k++) {
                        this.AddObjectAction(this._numberObjs, propertyData.Actions[k].ActionName, propertyData.Actions[k].Script);
                    }
                } else if (this.BeginsWith(this._lines[i], "action ")) {
                    this.AddToObjectActions(this.GetEverythingAfter(this._lines[i], "action "), this._numberObjs, this._nullContext);
                }
            } else {
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
                    this.LogASLError("Can't load menu '" + this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext) + "' - only one menu can be added.", 2 /* WarningError */);
                } else {
                    menuTitle = this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext);
                    for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                        if (Trim(this._lines[j]) != "") {
                            var scp = InStr(this._lines[j], ":");
                            if (scp == 0 && this._lines[j] != "-") {
                                this.LogASLError("No menu command specified in menu '" + menuTitle + "', item '" + this._lines[j], 2 /* WarningError */);
                            } else {
                                if (this._lines[j] == "-") {
                                    menuOptions.Add("k" + (j).toString(), "-");
                                } else {
                                    menuOptions.Add(Trim(Mid(this._lines[j], scp + 1)), Trim(Left(this._lines[j], scp - 1)));
                                }
                            }
                        }
                    }
                    if (menuOptions.Count > 0) {
                        exists = true;
                    }
                }
            }
        }
        if (exists) {
            var windowMenu = new MenuData();
            this._player.SetWindowMenu(windowMenu);
        }
    };
    LegacyGame.prototype.SetUpOptions = function () {
        var opt;
        for (var i = this.GetDefineBlock("options").StartLine + 1; i <= this.GetDefineBlock("options").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "panes ")) {
                opt = LCase(Trim(this.GetEverythingAfter(this._lines[i], "panes ")));
                this._player.SetPanesVisible(opt);
            } else if (this.BeginsWith(this._lines[i], "abbreviations ")) {
                opt = LCase(Trim(this.GetEverythingAfter(this._lines[i], "abbreviations ")));
                if (opt == "off") {
                    this._useAbbreviations = false;
                } else {
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
                    r.Exits = new RoomExits();
                    r.Exits.ObjId = r.ObjId;
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
                            } else if (Trim(this._lines[j]) == "end define") {
                                nestedBlock = nestedBlock - 1;
                            }
                        } while(!(nestedBlock == 0));
                    }
                    if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "alias ")) {
                        r.RoomAlias = this.GetParameter(this._lines[j], this._nullContext);
                        this._objs[this._numberObjs].ObjectAlias = r.RoomAlias;
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("alias=" + r.RoomAlias, this._numberObjs, this._nullContext);
                        }
                    } else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "description ")) {
                        r.Description = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "description "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Description.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "description", r.Description.Data);
                            } else {
                                this.AddToObjectProperties("description=" + r.Description.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "out ")) {
                        r.Out.Text = this.GetParameter(this._lines[j], this._nullContext);
                        r.Out.Script = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                        if (this._gameAslVersion >= 350) {
                            if (r.Out.Script != "") {
                                this.AddObjectAction(this._numberObjs, "out", r.Out.Script);
                            }
                            this.AddToObjectProperties("out=" + r.Out.Text, this._numberObjs, this._nullContext);
                        }
                    } else if (this.BeginsWith(this._lines[j], "east ")) {
                        r.East = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "east "));
                        if (this._gameAslVersion >= 350) {
                            if (r.East.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "east", r.East.Data);
                            } else {
                                this.AddToObjectProperties("east=" + r.East.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "west ")) {
                        r.West = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "west "));
                        if (this._gameAslVersion >= 350) {
                            if (r.West.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "west", r.West.Data);
                            } else {
                                this.AddToObjectProperties("west=" + r.West.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "north ")) {
                        r.North = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "north "));
                        if (this._gameAslVersion >= 350) {
                            if (r.North.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "north", r.North.Data);
                            } else {
                                this.AddToObjectProperties("north=" + r.North.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "south ")) {
                        r.South = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "south "));
                        if (this._gameAslVersion >= 350) {
                            if (r.South.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "south", r.South.Data);
                            } else {
                                this.AddToObjectProperties("south=" + r.South.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "northeast ")) {
                        r.NorthEast = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "northeast "));
                        if (this._gameAslVersion >= 350) {
                            if (r.NorthEast.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "northeast", r.NorthEast.Data);
                            } else {
                                this.AddToObjectProperties("northeast=" + r.NorthEast.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "northwest ")) {
                        r.NorthWest = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "northwest "));
                        if (this._gameAslVersion >= 350) {
                            if (r.NorthWest.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "northwest", r.NorthWest.Data);
                            } else {
                                this.AddToObjectProperties("northwest=" + r.NorthWest.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "southeast ")) {
                        r.SouthEast = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "southeast "));
                        if (this._gameAslVersion >= 350) {
                            if (r.SouthEast.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "southeast", r.SouthEast.Data);
                            } else {
                                this.AddToObjectProperties("southeast=" + r.SouthEast.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "southwest ")) {
                        r.SouthWest = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "southwest "));
                        if (this._gameAslVersion >= 350) {
                            if (r.SouthWest.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "southwest", r.SouthWest.Data);
                            } else {
                                this.AddToObjectProperties("southwest=" + r.SouthWest.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "up ")) {
                        r.Up = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "up "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Up.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "up", r.Up.Data);
                            } else {
                                this.AddToObjectProperties("up=" + r.Up.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "down ")) {
                        r.Down = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "down "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Down.Type == 1 /* Script */) {
                                this.AddObjectAction(this._numberObjs, "down", r.Down.Data);
                            } else {
                                this.AddToObjectProperties("down=" + r.Down.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "indescription ")) {
                        r.InDescription = this.GetParameter(this._lines[j], this._nullContext);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("indescription=" + r.InDescription, this._numberObjs, this._nullContext);
                        }
                    } else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "look ")) {
                        r.Look = this.GetParameter(this._lines[j], this._nullContext);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("look=" + r.Look, this._numberObjs, this._nullContext);
                        }
                    } else if (this.BeginsWith(this._lines[j], "prefix ")) {
                        r.Prefix = this.GetParameter(this._lines[j], this._nullContext);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("prefix=" + r.Prefix, this._numberObjs, this._nullContext);
                        }
                    } else if (this.BeginsWith(this._lines[j], "script ")) {
                        r.Script = this.GetEverythingAfter(this._lines[j], "script ");
                        this.AddObjectAction(this._numberObjs, "script", r.Script);
                    } else if (this.BeginsWith(this._lines[j], "command ")) {
                        r.NumberCommands = r.NumberCommands + 1;
                        if (!r.Commands)
                            r.Commands = [];
                        r.Commands[r.NumberCommands] = new UserDefinedCommandType();
                        r.Commands[r.NumberCommands].CommandText = this.GetParameter(this._lines[j], this._nullContext, false);
                        r.Commands[r.NumberCommands].CommandScript = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                    } else if (this.BeginsWith(this._lines[j], "place ")) {
                        r.NumberPlaces = r.NumberPlaces + 1;
                        if (!r.Places)
                            r.Places = [];
                        r.Places[r.NumberPlaces] = new PlaceType();
                        var placeData = this.GetParameter(this._lines[j], this._nullContext);
                        var scp = InStr(placeData, ";");
                        if (scp == 0) {
                            r.Places[r.NumberPlaces].PlaceName = placeData;
                        } else {
                            r.Places[r.NumberPlaces].PlaceName = Trim(Mid(placeData, scp + 1));
                            r.Places[r.NumberPlaces].Prefix = Trim(Left(placeData, scp - 1));
                        }
                        r.Places[r.NumberPlaces].Script = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                    } else if (this.BeginsWith(this._lines[j], "use ")) {
                        r.NumberUse = r.NumberUse + 1;
                        if (!r.Use)
                            r.Use = [];
                        r.Use[r.NumberUse] = new ScriptText();
                        r.Use[r.NumberUse].Text = this.GetParameter(this._lines[j], this._nullContext);
                        r.Use[r.NumberUse].Script = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                    } else if (this.BeginsWith(this._lines[j], "properties ")) {
                        this.AddToObjectProperties(this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                    } else if (this.BeginsWith(this._lines[j], "type ")) {
                        this._objs[this._numberObjs].NumberTypesIncluded = this._objs[this._numberObjs].NumberTypesIncluded + 1;
                        if (!this._objs[this._numberObjs].TypesIncluded)
                            this._objs[this._numberObjs].TypesIncluded = [];
                        this._objs[this._numberObjs].TypesIncluded[this._objs[this._numberObjs].NumberTypesIncluded] = this.GetParameter(this._lines[j], this._nullContext);
                        var propertyData = this.GetPropertiesInType(this.GetParameter(this._lines[j], this._nullContext));
                        this.AddToObjectProperties(propertyData.Properties, this._numberObjs, this._nullContext);
                        for (var k = 1; k <= propertyData.NumberActions; k++) {
                            this.AddObjectAction(this._numberObjs, propertyData.Actions[k].ActionName, propertyData.Actions[k].Script);
                        }
                    } else if (this.BeginsWith(this._lines[j], "action ")) {
                        this.AddToObjectActions(this.GetEverythingAfter(this._lines[j], "action "), this._numberObjs, this._nullContext);
                    } else if (this.BeginsWith(this._lines[j], "beforeturn ")) {
                        r.BeforeTurnScript = r.BeforeTurnScript + this.GetEverythingAfter(this._lines[j], "beforeturn ") + vbCrLf;
                    } else if (this.BeginsWith(this._lines[j], "afterturn ")) {
                        r.AfterTurnScript = r.AfterTurnScript + this.GetEverythingAfter(this._lines[j], "afterturn ") + vbCrLf;
                    }
                }
            }
        }
    };
    LegacyGame.prototype.SetUpSynonyms = function () {
        var block = this.GetDefineBlock("synonyms");
        this._numberSynonyms = 0;
        if (block.StartLine == 0 && block.EndLine == 0) {
            // UNKNOWN ExitSubStatement
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
                    var endOfWord = InStr(pos, originalWordsList, ";");
                    var thisWord = Trim(Mid(originalWordsList, pos, endOfWord - pos));
                    if (InStr(" " + convertWord + " ", " " + thisWord + " ") > 0) {
                        // Recursive synonym
                        this.LogASLError("Recursive synonym detected: '" + thisWord + "' converting to '" + convertWord + "'", 2 /* WarningError */);
                    } else {
                        this._numberSynonyms = this._numberSynonyms + 1;
                        if (!this._synonyms)
                            this._synonyms = [];
                        this._synonyms[this._numberSynonyms] = new SynonymType();
                        this._synonyms[this._numberSynonyms].OriginalWord = thisWord;
                        this._synonyms[this._numberSynonyms].ConvertTo = convertWord;
                    }
                    pos = endOfWord + 1;
                } while(!(pos >= Len(originalWordsList)));
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
                    } else if (this.BeginsWith(this._lines[j], "action ")) {
                        this._timers[this._numberTimers].TimerAction = this.GetEverythingAfter(this._lines[j], "action ");
                    } else if (Trim(LCase(this._lines[j])) == "enabled") {
                        this._timers[this._numberTimers].TimerActive = true;
                    } else if (Trim(LCase(this._lines[j])) == "disabled") {
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
                this._beforeTurnScript = this._beforeTurnScript + this.GetEverythingAfter(Trim(this._lines[i]), "beforeturn ") + vbCrLf;
            } else if (this.BeginsWith(this._lines[i], "afterturn ")) {
                this._afterTurnScript = this._afterTurnScript + this.GetEverythingAfter(Trim(this._lines[i]), "afterturn ") + vbCrLf;
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

                // UNKNOWN SelectBlock
                this._playerErrorMessageString[currentError] = errorMsg;
                if (currentError == 8 /* DefaultLook */ && !examineIsCustomised) {
                    // If we're setting the default look message, and we've not already customised the
                    // default examine message, then set the default examine message to the same thing.
                    this._playerErrorMessageString[17 /* DefaultExamine */] = errorMsg;
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
                    } else {
                        this.AddToObjectProperties("invisible", i, ctx);
                    }
                    found = true;
                    break;
                }
            }
            if (!found) {
                this.LogASLError("Not found object '" + thing + "'", 2 /* WarningError */);
            }
        } else {
            // split ThingString into character name and room
            // (thingstring of form name@room)
            var atPos = InStr(thing, "@");
            var room;
            var name;

            // If no room specified, current room presumed
            if (atPos == 0) {
                room = this._currentRoom;
                name = thing;
            } else {
                name = Trim(Left(thing, atPos - 1));
                room = Trim(Right(thing, Len(thing) - atPos));
            }
            if (type == LegacyGame.Thing.Character) {
                for (var i = 1; i <= this._numberChars; i++) {
                    if (LCase(this._chars[i].ContainerRoom) == LCase(room) && LCase(this._chars[i].ObjectName) == LCase(name)) {
                        this._chars[i].Visible = visible;
                        break;
                    }
                }
            } else if (type == LegacyGame.Thing.Object) {
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
        } else {
            // Workaround for a particular game which expects pictures to be in a popup window -
            // use the static picture frame feature so that image is not cleared
            this._player.SetPanelContents("<img src=\"" + this._player.GetURL(filename) + "\" onload=\"setPanelHeight()\"/>");
        }
    };
    LegacyGame.prototype.ShowRoomInfoV2 = function (room) {
        // ShowRoomInfo for Quest 2.x games
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
        gameBlock = this.GetDefineBlock("game");
        this._currentRoom = room;

        //find the room
        var roomBlock;
        roomBlock = this.DefineBlockParam("room", room);
        var finishedFindingCommas;
        charsViewable = "";
        charsFound = 0;

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
            // No formatting version, for label
        } else {
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
                roomDisplayText = roomDisplayText + Left(inDesc, Len(inDesc) - 1) + " " + prefixAlias + "." + vbCrLf;
            } else {
                // otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText + inDesc + vbCrLf;
            }
        } else {
            // if no indescription line, print the default.
            roomDisplayText = roomDisplayText + "You are in " + prefixAlias + "." + vbCrLf;
        }
        this._player.LocationUpdated(prefixAliasNoFormat);
        this.SetStringContents("quest.formatroom", prefixAliasNoFormat, this._nullContext);

        for (var i = 1; i <= this._numberChars; i++) {
            if (this._chars[i].ContainerRoom == room && this._chars[i].Exists && this._chars[i].Visible) {
                charsViewable = charsViewable + this._chars[i].Prefix + "|b" + this._chars[i].ObjectName + "|xb" + this._chars[i].Suffix + ", ";
                charsFound = charsFound + 1;
            }
        }
        if (charsFound == 0) {
            charsViewable = "There is nobody here.";
            this.SetStringContents("quest.characters", "", this._nullContext);
        } else {
            //chop off final comma and add full stop (.)
            charList = Left(charsViewable, Len(charsViewable) - 2);
            this.SetStringContents("quest.characters", charList, this._nullContext);

            //if more than one character, add "and" before
            //last one:
            cp = InStr(charList, ",");
            if (cp != 0) {
                foundLastComma = 0;
                do {
                    ncp = InStr(cp + 1, charList, ",");
                    if (ncp == 0) {
                        foundLastComma = 1;
                    } else {
                        cp = ncp;
                    }
                } while(!(foundLastComma == 1));
                charList = Trim(Left(charList, cp - 1)) + " and " + Trim(Mid(charList, cp + 1));
            }
            charsViewable = "You can see " + charList + " here.";
        }
        roomDisplayText = roomDisplayText + charsViewable + vbCrLf;

        //FIND OBJECTS
        noFormatObjsViewable = "";
        for (var i = 1; i <= this._numberObjs; i++) {
            if (this._objs[i].ContainerRoom == room && this._objs[i].Exists && this._objs[i].Visible) {
                objsViewable = objsViewable + this._objs[i].Prefix + "|b" + this._objs[i].ObjectName + "|xb" + this._objs[i].Suffix + ", ";
                noFormatObjsViewable = noFormatObjsViewable + this._objs[i].Prefix + this._objs[i].ObjectName + ", ";
                objsFound = objsFound + 1;
            }
        }
        var finishedLoop;
        if (objsFound != 0) {
            objListString = Left(objsViewable, Len(objsViewable) - 2);
            noFormatObjListString = Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2);
            cp = InStr(objListString, ",");
            if (cp != 0) {
                do {
                    ncp = InStr(cp + 1, objListString, ",");
                    if (ncp == 0) {
                        finishedLoop = true;
                    } else {
                        cp = ncp;
                    }
                } while(!(finishedLoop));
                objListString = Trim(Left(objListString, cp - 1) + " and " + Trim(Mid(objListString, cp + 1)));
            }
            objsViewable = "There is " + objListString + " here.";
            this.SetStringContents("quest.objects", Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2), this._nullContext);
            this.SetStringContents("quest.formatobjects", objListString, this._nullContext);
            roomDisplayText = roomDisplayText + objsViewable + vbCrLf;
        } else {
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
            } else if (this.BeginsWith(this._lines[i], "south ")) {
                nsew = nsew + "|bsouth|xb, ";
                possDir = possDir + "s";
            } else if (this.BeginsWith(this._lines[i], "east ")) {
                nsew = nsew + "|beast|xb, ";
                possDir = possDir + "e";
            } else if (this.BeginsWith(this._lines[i], "west ")) {
                nsew = nsew + "|bwest|xb, ";
                possDir = possDir + "w";
            } else if (this.BeginsWith(this._lines[i], "northeast ")) {
                nsew = nsew + "|bnortheast|xb, ";
                possDir = possDir + "a";
            } else if (this.BeginsWith(this._lines[i], "northwest ")) {
                nsew = nsew + "|bnorthwest|xb, ";
                possDir = possDir + "b";
            } else if (this.BeginsWith(this._lines[i], "southeast ")) {
                nsew = nsew + "|bsoutheast|xb, ";
                possDir = possDir + "c";
            } else if (this.BeginsWith(this._lines[i], "southwest ")) {
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
                } else {
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
            roomDisplayText = roomDisplayText + "You can go out to " + aliasOut + "." + vbCrLf;
            possDir = possDir + "o";
            this.SetStringContents("quest.doorways.out", aliasOut, this._nullContext);
        } else {
            this.SetStringContents("quest.doorways.out", "", this._nullContext);
        }
        var finished;
        if (nsew != "") {
            //strip final comma
            nsew = Left(nsew, Len(nsew) - 2);
            cp = InStr(nsew, ",");
            if (cp != 0) {
                finished = false;
                do {
                    ncp = InStr(cp + 1, nsew, ",");
                    if (ncp == 0) {
                        finished = true;
                    } else {
                        cp = ncp;
                    }
                } while(!(finished));
                nsew = Trim(Left(nsew, cp - 1)) + " or " + Trim(Mid(nsew, cp + 1));
            }
            roomDisplayText = roomDisplayText + "You can go " + nsew + "." + vbCrLf;
            this.SetStringContents("quest.doorways.dirs", nsew, this._nullContext);
        } else {
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
                    lastComma = InStr(lastComma + 1, places, ",");
                    if (lastComma == 0) {
                        finishedFindingCommas = true;
                        lastComma = oldLastComma;
                    }
                } while(!(finishedFindingCommas));
                places = Left(places, lastComma) + " or" + Right(places, Len(places) - lastComma);
            }
            roomDisplayText = roomDisplayText + "You can go to " + places + "." + vbCrLf;
            this.SetStringContents("quest.doorways.places", places, this._nullContext);
        } else {
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
        if (descTagExist == false) {
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "description ")) {
                    descLine = this._lines[i];
                    descTagExist = true;
                    break;
                }
            }
        }
        if (descTagExist == false) {
            //Remove final newline:
            roomDisplayText = Left(roomDisplayText, Len(roomDisplayText) - 2);
            this.Print(roomDisplayText, this._nullContext);
        } else {
            //execute description tag:
            //If no script, just print the tag's parameter.
            //Otherwise, execute it as ASL script:
            descLine = this.GetEverythingAfter(Trim(descLine), "description ");
            if (Left(descLine, 1) == "<") {
                this.Print(this.GetParameter(descLine, this._nullContext), this._nullContext);
            } else {
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
        if (type == 2 /* Room */) {
            objList.Add(new ListData());
            exitList.Add(new ListData());
        } else {
            objList.Add(new ListData());
        }
    };
    LegacyGame.prototype.ExecExec = function (scriptLine, ctx) {
        if (ctx.CancelExec) {
            // UNKNOWN ExitSubStatement
        }
        var execLine = this.GetParameter(scriptLine, ctx);
        var newCtx = this.CopyContext(ctx);
        newCtx.StackCounter = newCtx.StackCounter + 1;
        if (newCtx.StackCounter > 500) {
            this.LogASLError("Out of stack space running '" + scriptLine + "' - infinite loop?", 2 /* WarningError */);
            ctx.CancelExec = true;
            // UNKNOWN ExitSubStatement
        }
        if (this._gameAslVersion >= 310) {
            newCtx.AllowRealNamesInCommand = true;
        }
        if (InStr(execLine, ";") == 0) {
            // UNKNOWN TryBlock
        } else {
            var scp = InStr(execLine, ";");
            var ex = Trim(Left(execLine, scp - 1));
            var r = Trim(Mid(execLine, scp + 1));
            if (r == "normal") {
                this.ExecCommand(ex, newCtx, false, false);
            } else {
                this.LogASLError("Unrecognised post-command parameter in " + Trim(scriptLine), 2 /* WarningError */);
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
            this.LogASLError("Invalid string name '" + name + "' - string names cannot be numeric", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
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
        if (typeof libCommands === "undefined") { libCommands = false; }
        //Executes a user-defined command. If unavailable, returns
        //false.
        var curCmd;
        var commandList;
        var script = "";
        var commandTag;
        var commandLine = "";
        var foundCommand = false;

        //First, check for a command in the current room block
        var roomId = this.GetRoomID(this._currentRoom, ctx);

        // RoomID is 0 if we have no rooms in the game. Unlikely, but we get an RTE otherwise.
        if (roomId != 0) {
            var r = this._rooms[roomId];
            for (var i = 1; i <= r.NumberCommands; i++) {
                commandList = r.Commands[i].CommandText;
                var ep;
                do {
                    ep = InStr(commandList, ";");
                    if (ep == 0) {
                        curCmd = commandList;
                    } else {
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
                } while(!(ep == 0));
            }
        }
        if (!libCommands) {
            commandTag = "command";
        } else {
            commandTag = "lib command";
        }
        if (!foundCommand) {
            // Check "define game" block
            var block = this.GetDefineBlock("game");
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], commandTag)) {
                    commandList = this.GetParameter(this._lines[i], ctx, false);
                    var ep;
                    do {
                        ep = InStr(commandList, ";");
                        if (ep == 0) {
                            curCmd = commandList;
                        } else {
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
                    } while(!(ep == 0));
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
        var var2Pos;

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
            var nextVarPos = InStr(currentReqLinePos, required, "#");
            var currentVariable = "";
            if (nextVarPos == 0) {
                finished = true;
                nextVarPos = Len(required) + 1;
            } else {
                var2Pos = InStr(nextVarPos + 1, required, "#");
                currentVariable = Mid(required, nextVarPos + 1, (var2Pos - 1) - nextVarPos);
            }
            var checkChunk = Mid(required, currentReqLinePos, (nextVarPos - 1) - (currentReqLinePos - 1));
            var chunkBegin = InStr(currentTestLinePos, LCase(test), LCase(checkChunk));
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
        } while(!(finished));
        var success = true;

        for (var i = 1; i <= numberChunks - 1; i++) {
            var arrayIndex;

            // If VarName contains array name, change to index number
            if (InStr(varName[i], "[") > 0) {
                var indexResult = this.GetArrayIndex(varName[i], ctx);
                varName[i] = indexResult.Name;
                arrayIndex = indexResult.Index;
            } else {
                arrayIndex = 0;
            }
            var curChunk = Mid(test, chunksEnd[i], chunksBegin[i + 1] - chunksEnd[i]);
            if (this.BeginsWith(varName[i], "@")) {
                varName[i] = this.GetEverythingAfter(varName[i], "@");
                var id = this.Disambiguate(curChunk, this._currentRoom + ";" + "inventory", ctx);
                if (id == -1) {
                    if (this._gameAslVersion >= 391) {
                        this.PlayerErrorMessage(7 /* BadThing */, ctx);
                    } else {
                        this.PlayerErrorMessage(10 /* BadItem */, ctx);
                    }

                    // The Mid$(...,2) and Left$(...,2) removes the initial/final "."
                    this._badCmdBefore = Mid(Trim(Left(test, chunksEnd[i] - 1)), 2);
                    this._badCmdAfter = Trim(Mid(test, chunksBegin[i + 1]));
                    this._badCmdAfter = Left(this._badCmdAfter, Len(this._badCmdAfter) - 1);
                    success = false;
                } else if (id == -2) {
                    this._badCmdBefore = Trim(Left(test, chunksEnd[i] - 1));
                    this._badCmdAfter = Trim(Mid(test, chunksBegin[i + 1]));
                    success = false;
                } else {
                    this.SetStringContents(varName[i], this._objs[id].ObjectName, ctx, arrayIndex);
                }
            } else {
                this.SetStringContents(varName[i], curChunk, ctx, arrayIndex);
            }
        }
        return success;
    };
    LegacyGame.prototype.GetGender = function (character, capitalise, ctx) {
        var result;
        if (this._gameAslVersion >= 281) {
            result = this._objs[this.GetObjectIdNoAlias(character)].Gender;
        } else {
            var resultLine = this.RetrLine("character", character, "gender", ctx);
            if (resultLine == "<unfound>") {
                result = "it ";
            } else {
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
                var cbp = InStr(obp, objName, ")");
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
            } else {
                arrayIndex = parseInt(this.GetNumericContents(arrayIndexData, ctx));
                if (arrayIndex == -32767) {
                    this.LogASLError("Array index in '" + name + "' is not valid. An array index must be either a number or a numeric variable (without surrounding '%' characters)", 2 /* WarningError */);
                    return "";
                }
            }
            name = Left(name, bp - 1);
        }

        // First, see if the string already exists. If it does,
        // get its contents. If not, generate an error.
        var exists = false;
        var id;
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
            this.LogASLError("No string variable '" + name + "' defined.", 2 /* WarningError */);
            return "";
        }
        if (arrayIndex > this._stringVariable[id].VariableUBound) {
            this.LogASLError("Array index of '" + name + "[" + Trim(Str(arrayIndex)) + "]' too big.", 2 /* WarningError */);
            return "";
        }

        // Now, set the contents
        if (!returnAlias) {
            return this._stringVariable[id].VariableContents[arrayIndex];
        } else {
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
        } else {
            name = Trim(Left(thingName, atPos - 1));
            room = Trim(Right(thingName, Len(thingName) - atPos));
        }
        if (type == 0 /* Character */) {
            for (var i = 1; i <= this._numberChars; i++) {
                if (LCase(this._chars[i].ContainerRoom) == LCase(room) && LCase(this._chars[i].ObjectName) == LCase(name)) {
                    return this._chars[i].Exists;
                }
            }
        } else if (type == 1 /* Object */) {
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
        var var2Pos;

        // This avoids "xxx123" being compatible with "xxx".
        test = "^" + Trim(test) + "^";
        required = "^" + required + "^";

        //Go through RequiredLine in chunks going up to variables.
        var currentReqLinePos = 1;
        var currentTestLinePos = 1;
        var finished = false;
        do {
            var nextVarPos = InStr(currentReqLinePos, required, "#");
            if (nextVarPos == 0) {
                nextVarPos = Len(required) + 1;
                finished = true;
            } else {
                var2Pos = InStr(nextVarPos + 1, required, "#");
            }
            var checkChunk = Mid(required, currentReqLinePos, (nextVarPos - 1) - (currentReqLinePos - 1));
            if (InStr(currentTestLinePos, test, checkChunk) != 0) {
                currentTestLinePos = InStr(currentTestLinePos, test, checkChunk) + Len(checkChunk);
            } else {
                return false;
            }

            //Skip to end of variable
            currentReqLinePos = var2Pos + 1;
        } while(!(finished));
        return true;
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
        var lines = null;
        this._gameLoadMethod = "loaded";
        var prevQsgVersion = false;
        if (this._data == null) {
            fileData = System.IO.File.ReadAllText(filename, System.Text.Encoding.GetEncoding(1252));
        } else {
            fileData = System.Text.Encoding.GetEncoding(1252).GetString(this._data.Data);
        }

        // Check version
        savedQsgVersion = Left(fileData, 10);
        if (this.BeginsWith(savedQsgVersion, "QUEST200.1")) {
            prevQsgVersion = true;
        } else if (!this.BeginsWith(savedQsgVersion, "QUEST300")) {
            return false;
        }
        if (prevQsgVersion) {
            lines = fileData.Split('expr', StringSplitOptions.None);
            this._gameFileName = lines[1];
        } else {
            this.InitFileData(fileData);
            this.GetNextChunk();
            if (this._data == null) {
                this._gameFileName = this.GetNextChunk();
            } else {
                this.GetNextChunk();
                this._gameFileName = this._data.SourceFile;
            }
        }
        if (this._data == null && !System.IO.File.Exists(this._gameFileName)) {
            this._gameFileName = this._player.GetNewGameFile(this._gameFileName, "*.asl;*.cas;*.zip");
            if (this._gameFileName == "") {
                // UNKNOWN ExitFunctionStatement
            }
        }
        result = this.InitialiseGame(this._gameFileName, true);
        if (result == false) {
            return false;
        }
        if (!prevQsgVersion) {
            // Open Quest 3.0 saved game file
            this._gameLoading = true;
            this.RestoreGameData(fileData);
            this._gameLoading = false;
        } else {
            // Open Quest 2.x saved game file
            this._currentRoom = lines[3];

            // Start at line 5 as line 4 is always "!c"
            var lineNumber = 5;
            do {
                data = lines[lineNumber];

                // UNKNOWN AddAssignmentStatement
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
            } while(!(data == "!i"));
            do {
                data = lines[lineNumber];

                // UNKNOWN AddAssignmentStatement
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
            } while(!(data == "!o"));
            do {
                data = lines[lineNumber];

                // UNKNOWN AddAssignmentStatement
                if (data != "!p") {
                    scp = InStr(data, ";");
                    scp2 = InStr(scp + 1, data, ";");
                    scp3 = InStr(scp2 + 1, data, ";");
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
            } while(!(data == "!p"));
            do {
                data = lines[lineNumber];

                // UNKNOWN AddAssignmentStatement
                if (data != "!s") {
                    scp = InStr(data, ";");
                    scp2 = InStr(scp + 1, data, ";");
                    scp3 = InStr(scp2 + 1, data, ";");
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
            } while(!(data == "!s"));
            do {
                data = lines[lineNumber];

                // UNKNOWN AddAssignmentStatement
                if (data != "!n") {
                    scp = InStr(data, ";");
                    name = Trim(Left(data, scp - 1));
                    data = Right(data, Len(data) - scp);
                    this.SetStringContents(name, data, this._nullContext);
                }
            } while(!(data == "!n"));
            do {
                data = lines[lineNumber];

                // UNKNOWN AddAssignmentStatement
                if (data != "!e") {
                    scp = InStr(data, ";");
                    name = Trim(Left(data, scp - 1));
                    data = Right(data, Len(data) - scp);
                    this.SetNumericVariableContents(name, Val(data), this._nullContext);
                }
            } while(!(data == "!e"));
        }
        this._saveGameFile = filename;
        return true;
    };
    LegacyGame.prototype.SaveGame = function (filename, saveFile) {
        if (typeof saveFile === "undefined") { saveFile = true; }
        var ctx = new Context();
        var saveData;
        if (this._gameAslVersion >= 391) {
            this.ExecuteScript(this._beforeSaveScript, ctx);
        }
        if (this._gameAslVersion >= 280) {
            saveData = this.MakeRestoreData();
        } else {
            saveData = this.MakeRestoreDataV2();
        }
        if (saveFile) {
            System.IO.File.WriteAllText(filename, saveData, System.Text.Encoding.GetEncoding(1252));
        }
        this._saveGameFile = filename;
        return System.Text.Encoding.GetEncoding(1252).GetBytes(saveData);
    };
    LegacyGame.prototype.MakeRestoreDataV2 = function () {
        var lines = {};
        var i;
        lines.Add("QUEST200.1");
        lines.Add(this.GetOriginalFilenameForQSG);
        lines.Add(this._gameName);
        lines.Add(this._currentRoom);
        lines.Add("!c");
        for (var i = 1; i <= this._numCollectables; i++) {
            lines.Add(this._collectables[i].Name + ";" + Str(this._collectables[i].Value));
        }
        lines.Add("!i");
        for (var i = 1; i <= this._numberItems; i++) {
            lines.Add(this._items[i].Name + ";" + this.YesNo(this._items[i].Got));
        }
        lines.Add("!o");
        for (var i = 1; i <= this._numberObjs; i++) {
            lines.Add(this._objs[i].ObjectName + ";" + this.YesNo(this._objs[i].Exists) + ";" + this.YesNo(this._objs[i].Visible) + ";" + this._objs[i].ContainerRoom);
        }
        lines.Add("!p");
        for (var i = 1; i <= this._numberChars; i++) {
            lines.Add(this._chars[i].ObjectName + ";" + this.YesNo(this._chars[i].Exists) + ";" + this.YesNo(this._chars[i].Visible) + ";" + this._chars[i].ContainerRoom);
        }
        lines.Add("!s");
        for (var i = 1; i <= this._numberStringVariables; i++) {
            lines.Add(this._stringVariable[i].VariableName + ";" + this._stringVariable[i].VariableContents[0]);
        }
        lines.Add("!n");
        for (var i = 1; i <= this._numberNumericVariables; i++) {
            lines.Add(this._numericVariable[i].VariableName + ";" + Str(parseFloat(this._numericVariable[i].VariableContents[0])));
        }
        lines.Add("!e");
        return String.Join(vbCrLf, lines);
    };
    LegacyGame.prototype.SetAvailability = function (thingString, exists, ctx, type) {
        if (typeof type === "undefined") { type = 1 /* Object */; }
        // Sets availability of objects (and characters in ASL<281)
        if (this._gameAslVersion >= 281) {
            var found = false;
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ObjectName) == LCase(thingString)) {
                    this._objs[i].Exists = exists;
                    if (exists) {
                        this.AddToObjectProperties("not hidden", i, ctx);
                    } else {
                        this.AddToObjectProperties("hidden", i, ctx);
                    }
                    found = true;
                    break;
                }
            }
            if (!found) {
                this.LogASLError("Not found object '" + thingString + "'", 2 /* WarningError */);
            }
        } else {
            // split ThingString into character name and room
            // (thingstring of form name@room)
            var room;
            var name;
            var atPos = InStr(thingString, "@");

            // If no room specified, currentroom presumed
            if (atPos == 0) {
                room = this._currentRoom;
                name = thingString;
            } else {
                name = Trim(Left(thingString, atPos - 1));
                room = Trim(Right(thingString, Len(thingString) - atPos));
            }
            if (type == 0 /* Character */) {
                for (var i = 1; i <= this._numberChars; i++) {
                    if (LCase(this._chars[i].ContainerRoom) == LCase(room) && LCase(this._chars[i].ObjectName) == LCase(name)) {
                        this._chars[i].Exists = exists;
                        break;
                    }
                }
            } else if (type == 1 /* Object */) {
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
        if (typeof arrayIndex === "undefined") { arrayIndex = 0; }
        var id;
        var exists = false;
        if (name == "") {
            this.LogASLError("Internal error - tried to set empty string name to '" + value + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        if (this._gameAslVersion >= 281) {
            var bp = InStr(name, "[");
            if (bp != 0) {
                arrayIndex = this.GetArrayIndex(name, ctx).Index;
                name = Left(name, bp - 1);
            }
        }
        if (arrayIndex < 0) {
            this.LogASLError("'" + name + "[" + Trim(Str(arrayIndex)) + "]' is invalid - did not assign to array", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
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
                // UNKNOWN ContinueForStatement
            }
            var restOfLine;
            var origContainerRoomName;
            var containerRoomName;
            if (this.BeginsWith(this._lines[block.StartLine], "define room")) {
                origContainerRoomName = this.GetParameter(this._lines[block.StartLine], this._nullContext);
            } else {
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
                    o.Take.Type = 2 /* Nothing */;
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
                        } else if (this.BeginsWith(this._lines[j], "startin ") && containerRoomName == "__UNKNOWN") {
                            containerRoomName = this.GetParameter(this._lines[j], this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "prefix ")) {
                            o.Prefix = this.GetParameter(this._lines[j], this._nullContext) + " ";
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("prefix=" + o.Prefix, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "suffix ")) {
                            o.Suffix = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("suffix=" + o.Suffix, this._numberObjs, this._nullContext);
                            }
                        } else if (Trim(this._lines[j]) == "invisible") {
                            o.Visible = false;
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("invisible", this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "alias ")) {
                            o.ObjectAlias = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("alias=" + o.ObjectAlias, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "alt ")) {
                            this.AddToObjectAltNames(this.GetParameter(this._lines[j], this._nullContext), this._numberObjs);
                        } else if (this.BeginsWith(this._lines[j], "detail ")) {
                            o.Detail = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("detail=" + o.Detail, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "gender ")) {
                            o.Gender = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("gender=" + o.Gender, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "article ")) {
                            o.Article = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("article=" + o.Article, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "gain ")) {
                            o.GainScript = this.GetEverythingAfter(this._lines[j], "gain ");
                            this.AddObjectAction(this._numberObjs, "gain", o.GainScript);
                        } else if (this.BeginsWith(this._lines[j], "lose ")) {
                            o.LoseScript = this.GetEverythingAfter(this._lines[j], "lose ");
                            this.AddObjectAction(this._numberObjs, "lose", o.LoseScript);
                        } else if (this.BeginsWith(this._lines[j], "displaytype ")) {
                            o.DisplayType = this.GetParameter(this._lines[j], this._nullContext);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("displaytype=" + o.DisplayType, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "look ")) {
                            if (this._gameAslVersion >= 311) {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "look ");
                                if (Left(restOfLine, 1) == "<") {
                                    this.AddToObjectProperties("look=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                                } else {
                                    this.AddObjectAction(this._numberObjs, "look", restOfLine);
                                }
                            }
                        } else if (this.BeginsWith(this._lines[j], "examine ")) {
                            if (this._gameAslVersion >= 311) {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "examine ");
                                if (Left(restOfLine, 1) == "<") {
                                    this.AddToObjectProperties("examine=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                                } else {
                                    this.AddObjectAction(this._numberObjs, "examine", restOfLine);
                                }
                            }
                        } else if (this._gameAslVersion >= 311 && this.BeginsWith(this._lines[j], "speak ")) {
                            restOfLine = this.GetEverythingAfter(this._lines[j], "speak ");
                            if (Left(restOfLine, 1) == "<") {
                                this.AddToObjectProperties("speak=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            } else {
                                this.AddObjectAction(this._numberObjs, "speak", restOfLine);
                            }
                        } else if (this.BeginsWith(this._lines[j], "properties ")) {
                            this.AddToObjectProperties(this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "type ")) {
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
                        } else if (this.BeginsWith(this._lines[j], "action ")) {
                            this.AddToObjectActions(this.GetEverythingAfter(this._lines[j], "action "), this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "use ")) {
                            this.AddToUseInfo(this._numberObjs, this.GetEverythingAfter(this._lines[j], "use "));
                        } else if (this.BeginsWith(this._lines[j], "give ")) {
                            this.AddToGiveInfo(this._numberObjs, this.GetEverythingAfter(this._lines[j], "give "));
                        } else if (Trim(this._lines[j]) == "take") {
                            o.Take.Type = 3 /* Default */;
                            this.AddToObjectProperties("take", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "take ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "take "), 1) == "<") {
                                o.Take.Type = 0 /* Text */;
                                o.Take.Data = this.GetParameter(this._lines[j], this._nullContext);
                                this.AddToObjectProperties("take=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            } else {
                                o.Take.Type = 1 /* Script */;
                                restOfLine = this.GetEverythingAfter(this._lines[j], "take ");
                                o.Take.Data = restOfLine;
                                this.AddObjectAction(this._numberObjs, "take", restOfLine);
                            }
                        } else if (Trim(this._lines[j]) == "container") {
                            if (this._gameAslVersion >= 391) {
                                this.AddToObjectProperties("container", this._numberObjs, this._nullContext);
                            }
                        } else if (Trim(this._lines[j]) == "surface") {
                            if (this._gameAslVersion >= 391) {
                                this.AddToObjectProperties("container", this._numberObjs, this._nullContext);
                                this.AddToObjectProperties("surface", this._numberObjs, this._nullContext);
                            }
                        } else if (Trim(this._lines[j]) == "opened") {
                            if (this._gameAslVersion >= 391) {
                                this.AddToObjectProperties("opened", this._numberObjs, this._nullContext);
                            }
                        } else if (Trim(this._lines[j]) == "transparent") {
                            if (this._gameAslVersion >= 391) {
                                this.AddToObjectProperties("transparent", this._numberObjs, this._nullContext);
                            }
                        } else if (Trim(this._lines[j]) == "open") {
                            this.AddToObjectProperties("open", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "open ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "open "), 1) == "<") {
                                this.AddToObjectProperties("open=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            } else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "open ");
                                this.AddObjectAction(this._numberObjs, "open", restOfLine);
                            }
                        } else if (Trim(this._lines[j]) == "close") {
                            this.AddToObjectProperties("close", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "close ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "close "), 1) == "<") {
                                this.AddToObjectProperties("close=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            } else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "close ");
                                this.AddObjectAction(this._numberObjs, "close", restOfLine);
                            }
                        } else if (Trim(this._lines[j]) == "add") {
                            this.AddToObjectProperties("add", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "add ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "add "), 1) == "<") {
                                this.AddToObjectProperties("add=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            } else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "add ");
                                this.AddObjectAction(this._numberObjs, "add", restOfLine);
                            }
                        } else if (Trim(this._lines[j]) == "remove") {
                            this.AddToObjectProperties("remove", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "remove ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "remove "), 1) == "<") {
                                this.AddToObjectProperties("remove=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                            } else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "remove ");
                                this.AddObjectAction(this._numberObjs, "remove", restOfLine);
                            }
                        } else if (this.BeginsWith(this._lines[j], "parent ")) {
                            this.AddToObjectProperties("parent=" + this.GetParameter(this._lines[j], this._nullContext), this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "list")) {
                            this.ProcessListInfo(this._lines[j], this._numberObjs);
                        }
                    } while(!(Trim(this._lines[j]) == "end define"));
                    o.DefinitionSectionEnd = j;
                    if (!hidden) {
                        o.Exists = true;
                    }
                } else if (this._gameAslVersion <= 280 && this.BeginsWith(this._lines[j], "define character")) {
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
                        } else if (this.BeginsWith(this._lines[j], "startin ") && containerRoomName == "__UNKNOWN") {
                            containerRoomName = this.GetParameter(this._lines[j], this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "prefix ")) {
                            this._chars[this._numberChars].Prefix = this.GetParameter(this._lines[j], this._nullContext) + " ";
                        } else if (this.BeginsWith(this._lines[j], "suffix ")) {
                            this._chars[this._numberChars].Suffix = " " + this.GetParameter(this._lines[j], this._nullContext);
                        } else if (Trim(this._lines[j]) == "invisible") {
                            this._chars[this._numberChars].Visible = false;
                        } else if (this.BeginsWith(this._lines[j], "alias ")) {
                            this._chars[this._numberChars].ObjectAlias = this.GetParameter(this._lines[j], this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "detail ")) {
                            this._chars[this._numberChars].Detail = this.GetParameter(this._lines[j], this._nullContext);
                        }
                        this._chars[this._numberChars].ContainerRoom = containerRoomName;
                    } while(!(Trim(this._lines[j]) == "end define"));
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
        if (caption.Length > 0) {
            this.Print(caption, this._nullContext);
        }
        this.ShowPictureInText(filename);
    };
    LegacyGame.prototype.ShowRoomInfo = function (room, ctx, noPrint) {
        if (typeof noPrint === "undefined") { noPrint = false; }
        if (this._gameAslVersion < 280) {
            this.ShowRoomInfoV2(room);
            // UNKNOWN ExitSubStatement
        }
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
        var gameBlock = this.GetDefineBlock("game");
        this._currentRoom = room;
        var id = this.GetRoomID(this._currentRoom, ctx);
        if (id == 0) {
            // UNKNOWN ExitSubStatement
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
            // No formatting version, for label
        } else {
            roomDisplayName = prefix + " |cr" + roomAlias + "|cb";
            roomDisplayNameNoFormat = prefix + " " + roomAlias;
        }
        inDescription = this._rooms[id].InDescription;
        if (inDescription != "") {
            // Print player's location according to indescription:
            if (Right(inDescription, 1) == ":") {
                // if line ends with a colon, add place name:
                roomDisplayText = roomDisplayText + Left(inDescription, Len(inDescription) - 1) + " " + roomDisplayName + "." + vbCrLf;
            } else {
                // otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText + inDescription + vbCrLf;
            }
        } else {
            // if no indescription line, print the default.
            roomDisplayText = roomDisplayText + "You are in " + roomDisplayName + "." + vbCrLf;
        }
        this._player.LocationUpdated(UCase(Left(roomAlias, 1)) + Mid(roomAlias, 2));
        this.SetStringContents("quest.formatroom", roomDisplayNameNoFormat, ctx);

        // SHOW OBJECTS *************************************************************
        visibleObjectsNoFormat = "";
        var visibleObjectsList = {};

        // of object IDs
        var count;
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ContainerRoom) == LCase(room) && this._objs[i].Exists && this._objs[i].Visible && !this._objs[i].IsExit) {
                visibleObjectsList.Add(i);
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
            } else {
                visibleObjects = visibleObjects + this._objs[objId].Prefix + "|b" + this._objs[objId].ObjectAlias + "|xb" + objSuffix;
                visibleObjectsNoFormat = visibleObjectsNoFormat + this._objs[objId].Prefix + this._objs[objId].ObjectAlias;
            }
            count = count + 1;
            if (count < visibleObjectsList.Count() - 1) {
                visibleObjects = visibleObjects + ", ";
                visibleObjectsNoFormat = visibleObjectsNoFormat + ", ";
            } else if (count == visibleObjectsList.Count() - 1) {
                visibleObjects = visibleObjects + " and ";
                visibleObjectsNoFormat = visibleObjectsNoFormat + ", ";
            }
        }, this);
        if (visibleObjectsList.Count() > 0) {
            this.SetStringContents("quest.formatobjects", visibleObjects, ctx);
            visibleObjects = "There is " + visibleObjects + " here.";
            this.SetStringContents("quest.objects", visibleObjectsNoFormat, ctx);
            roomDisplayText = roomDisplayText + visibleObjects + vbCrLf;
        } else {
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
                        lastComma = InStr(lastComma + 1, placeList, ",");
                        if (lastComma == 0) {
                            finishedFindingCommas = true;
                            lastComma = oldLastComma;
                        }
                    } while(!(finishedFindingCommas));
                    placeList = Left(placeList, lastComma - 1) + " or" + Right(placeList, Len(placeList) - lastComma);
                }
                roomDisplayText = roomDisplayText + "You can go to " + placeList + "." + vbCrLf;
                this.SetStringContents("quest.doorways.places", placeList, ctx);
            } else {
                this.SetStringContents("quest.doorways.places", "", ctx);
            }
        }

        // GET "LOOK" DESCRIPTION (but don't print it yet) **************************
        objLook = this.GetObjectProperty("look", this._rooms[id].ObjId, null, false);
        if (objLook == "") {
            if (this._rooms[id].Look != "") {
                lookDesc = this._rooms[id].Look;
            }
        } else {
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
        } else {
            descTagExist = false;
        }
        if (descTagExist == false) {
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "description ")) {
                    descLine = this.GetEverythingAfter(this._lines[i], "description ");
                    descTagExist = true;
                    if (Left(descLine, 1) == "<") {
                        descLine = this.GetParameter(descLine, ctx);
                        descType = 0 /* Text */;
                    } else {
                        descType = 1 /* Script */;
                    }
                    i = gameBlock.EndLine;
                }
            }
        }
        if (descTagExist && this._gameAslVersion >= 310) {
            showLookText = false;
        }
        if (!noPrint) {
            if (descTagExist == false) {
                //Remove final vbCrLf:
                roomDisplayText = Left(roomDisplayText, Len(roomDisplayText) - 2);
                this.Print(roomDisplayText, ctx);
                if (doorwayString != "") {
                    this.Print(doorwayString, ctx);
                }
            } else {
                //execute description tag:
                //If no script, just print the tag's parameter.
                //Otherwise, execute it as ASL script:
                if (descType == 0 /* Text */) {
                    this.Print(descLine, ctx);
                } else {
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
        var max;
        var value;
        var min;
        var m;
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
            } else if (InStr(type, "r") == Len(type)) {
                min = Val(Left(type, Len(type) - 1));
                m = 2;
            } else {
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
        } else if (this._collectables[id].Display == "") {
            display = "<null>";
        } else {
            var ep = InStr(this._collectables[id].Display, "!");
            if (ep == 0) {
                display = this._collectables[id].Display;
            } else {
                var firstBit = Left(this._collectables[id].Display, ep - 1);
                var nextBit = Right(this._collectables[id].Display, Len(this._collectables[id].Display) - ep);
                display = firstBit + Trim(Str(this._collectables[id].Value)) + nextBit;
            }
            if (InStr(display, "*") > 0) {
                var firstStarPos = InStr(display, "*");
                var secondStarPos = InStr(firstStarPos + 1, display, "*");
                var beforeStar = Left(display, firstStarPos - 1);
                var afterStar = Mid(display, secondStarPos + 1);
                var betweenStar = Mid(display, firstStarPos + 1, (secondStarPos - firstStarPos) - 1);
                if (this._collectables[id].Value != 1) {
                    display = beforeStar + betweenStar + afterStar;
                } else {
                    display = beforeStar + afterStar;
                }
            }
        }
        if (this._collectables[id].Value == 0 && this._collectables[id].DisplayWhenZero == false) {
            display = "<null>";
        }
        return display;
    };
    LegacyGame.prototype.DisplayTextSection = function (section, ctx) {
        var block;
        block = this.DefineBlockParam("text", section);
        if (block.StartLine == 0) {
            // UNKNOWN ExitSubStatement
        }
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this._gameAslVersion >= 392) {
                // Convert string variables etc.
                this.Print(this.GetParameter("<" + this._lines[i] + ">", ctx), ctx);
            } else {
                this.Print(this._lines[i], ctx);
            }
        }
        this.Print("", ctx);
    };

    // Returns true if the system is ready to process a new command after completion - so it will be
    // in most cases, except when ExecCommand just caused an "enter" script command to complete
    LegacyGame.prototype.ExecCommand = function (input, ctx, echo, runUserCommand, dontSetIt) {
        if (typeof echo === "undefined") { echo = true; }
        if (typeof runUserCommand === "undefined") { runUserCommand = true; }
        if (typeof dontSetIt === "undefined") { dontSetIt = false; }
        var parameter;
        var skipAfterTurn = false;
        input = this.RemoveFormatting(input);
        var oldBadCmdBefore = this._badCmdBefore;
        var roomID = this.GetRoomID(this._currentRoom, ctx);
        var enteredHelpCommand = false;
        if (input == "") {
            return true;
        }
        var cmd = LCase(input);

        // UNKNOWN SyncLockBlock
        var userCommandReturn;
        if (echo) {
            this.Print("> " + input, ctx);
        }
        input = LCase(input);
        this.SetStringContents("quest.originalcommand", input, ctx);
        var newCommand = " " + input + " ";

        for (var i = 1; i <= this._numberSynonyms; i++) {
            var cp = 1;
            var n;
            do {
                n = InStr(cp, newCommand, " " + this._synonyms[i].OriginalWord + " ");
                if (n != 0) {
                    newCommand = Left(newCommand, n - 1) + " " + this._synonyms[i].ConvertTo + " " + Mid(newCommand, n + Len(this._synonyms[i].OriginalWord) + 2);
                    cp = n + 1;
                }
            } while(!(n == 0));
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
                } else {
                    this.ExecuteScript(this._rooms[roomID].BeforeTurnScript, newCtx);
                }
            }
        }
        if (this._beforeTurnScript != "" && globalOverride == false) {
            this.ExecuteScript(this._beforeTurnScript, newCtx);
        }

        // In executing BeforeTurn script, "dontprocess" sets ctx.DontProcessCommand,
        // in which case we don't process the command.
        if (!newCtx.DontProcessCommand) {
            //Try to execute user defined command, if allowed:
            userCommandReturn = false;
            if (runUserCommand == true) {
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
        } else {
            // Set the UserCommand flag to fudge not processing any more commands
            userCommandReturn = true;
        }
        var invList = "";
        if (!userCommandReturn) {
            if (this.CmdStartsWith(input, "speak to ")) {
                parameter = this.GetEverythingAfter(input, "speak to ");
                this.ExecSpeak(parameter, ctx);
            } else if (this.CmdStartsWith(input, "talk to ")) {
                parameter = this.GetEverythingAfter(input, "talk to ");
                this.ExecSpeak(parameter, ctx);
            } else if (cmd == "exit" || cmd == "out" || cmd == "leave") {
                this.GoDirection("out", ctx);
                this._lastIt = 0;
            } else if (cmd == "north" || cmd == "south" || cmd == "east" || cmd == "west") {
                this.GoDirection(input, ctx);
                this._lastIt = 0;
            } else if (cmd == "n" || cmd == "s" || cmd == "w" || cmd == "e") {
                // UNKNOWN SelectBlock
                this._lastIt = 0;
            } else if (cmd == "ne" || cmd == "northeast" || cmd == "north-east" || cmd == "north east" || cmd == "go ne" || cmd == "go northeast" || cmd == "go north-east" || cmd == "go north east") {
                this.GoDirection("northeast", ctx);
                this._lastIt = 0;
            } else if (cmd == "nw" || cmd == "northwest" || cmd == "north-west" || cmd == "north west" || cmd == "go nw" || cmd == "go northwest" || cmd == "go north-west" || cmd == "go north west") {
                this.GoDirection("northwest", ctx);
                this._lastIt = 0;
            } else if (cmd == "se" || cmd == "southeast" || cmd == "south-east" || cmd == "south east" || cmd == "go se" || cmd == "go southeast" || cmd == "go south-east" || cmd == "go south east") {
                this.GoDirection("southeast", ctx);
                this._lastIt = 0;
            } else if (cmd == "sw" || cmd == "southwest" || cmd == "south-west" || cmd == "south west" || cmd == "go sw" || cmd == "go southwest" || cmd == "go south-west" || cmd == "go south west") {
                this.GoDirection("southwest", ctx);
                this._lastIt = 0;
            } else if (cmd == "up" || cmd == "u") {
                this.GoDirection("up", ctx);
                this._lastIt = 0;
            } else if (cmd == "down" || cmd == "d") {
                this.GoDirection("down", ctx);
                this._lastIt = 0;
            } else if (this.CmdStartsWith(input, "go ")) {
                if (this._gameAslVersion >= 410) {
                    this._rooms[this.GetRoomID(this._currentRoom, ctx)].Exits.ExecuteGo(input, ctx);
                } else {
                    parameter = this.GetEverythingAfter(input, "go ");
                    if (parameter == "out") {
                        this.GoDirection("out", ctx);
                    } else if (parameter == "north" || parameter == "south" || parameter == "east" || parameter == "west" || parameter == "up" || parameter == "down") {
                        this.GoDirection(parameter, ctx);
                    } else if (this.BeginsWith(parameter, "to ")) {
                        parameter = this.GetEverythingAfter(parameter, "to ");
                        this.GoToPlace(parameter, ctx);
                    } else {
                        this.PlayerErrorMessage(1 /* BadGo */, ctx);
                    }
                }
                this._lastIt = 0;
            } else if (this.CmdStartsWith(input, "give ")) {
                parameter = this.GetEverythingAfter(input, "give ");
                this.ExecGive(parameter, ctx);
            } else if (this.CmdStartsWith(input, "take ")) {
                parameter = this.GetEverythingAfter(input, "take ");
                this.ExecTake(parameter, ctx);
            } else if (this.CmdStartsWith(input, "drop ") && this._gameAslVersion >= 280) {
                parameter = this.GetEverythingAfter(input, "drop ");
                this.ExecDrop(parameter, ctx);
            } else if (this.CmdStartsWith(input, "get ")) {
                parameter = this.GetEverythingAfter(input, "get ");
                this.ExecTake(parameter, ctx);
            } else if (this.CmdStartsWith(input, "pick up ")) {
                parameter = this.GetEverythingAfter(input, "pick up ");
                this.ExecTake(parameter, ctx);
            } else if (cmd == "pick it up" || cmd == "pick them up" || cmd == "pick this up" || cmd == "pick that up" || cmd == "pick these up" || cmd == "pick those up" || cmd == "pick him up" || cmd == "pick her up") {
                this.ExecTake(Mid(cmd, 6, InStr(7, cmd, " ") - 6), ctx);
            } else if (this.CmdStartsWith(input, "look ")) {
                this.ExecLook(input, ctx);
            } else if (this.CmdStartsWith(input, "l ")) {
                this.ExecLook("look " + this.GetEverythingAfter(input, "l "), ctx);
            } else if (this.CmdStartsWith(input, "examine ") && this._gameAslVersion >= 280) {
                this.ExecExamine(input, ctx);
            } else if (this.CmdStartsWith(input, "x ") && this._gameAslVersion >= 280) {
                this.ExecExamine("examine " + this.GetEverythingAfter(input, "x "), ctx);
            } else if (cmd == "l" || cmd == "look") {
                this.ExecLook("look", ctx);
            } else if (cmd == "x" || cmd == "examine") {
                this.ExecExamine("examine", ctx);
            } else if (this.CmdStartsWith(input, "use ")) {
                this.ExecUse(input, ctx);
            } else if (this.CmdStartsWith(input, "open ") && this._gameAslVersion >= 391) {
                this.ExecOpenClose(input, ctx);
            } else if (this.CmdStartsWith(input, "close ") && this._gameAslVersion >= 391) {
                this.ExecOpenClose(input, ctx);
            } else if (this.CmdStartsWith(input, "put ") && this._gameAslVersion >= 391) {
                this.ExecAddRemove(input, ctx);
            } else if (this.CmdStartsWith(input, "add ") && this._gameAslVersion >= 391) {
                this.ExecAddRemove(input, ctx);
            } else if (this.CmdStartsWith(input, "remove ") && this._gameAslVersion >= 391) {
                this.ExecAddRemove(input, ctx);
            } else if (cmd == "save") {
                this._player.RequestSave(null);
            } else if (cmd == "quit") {
                this.GameFinished();
            } else if (this.BeginsWith(cmd, "help")) {
                this.ShowHelp(ctx);
                enteredHelpCommand = true;
            } else if (cmd == "about") {
                this.ShowGameAbout(ctx);
            } else if (cmd == "clear") {
                this.DoClear();
            } else if (cmd == "debug") {
                // TO DO: This is temporary, would be better to have a log viewer built in to Player
                this._log.forEach(function (logEntry) {
                    this.Print(logEntry, ctx);
                }, this);
            } else if (cmd == "inventory" || cmd == "inv" || cmd == "i") {
                if (this._gameAslVersion >= 280) {
                    for (var i = 1; i <= this._numberObjs; i++) {
                        if (this._objs[i].ContainerRoom == "inventory" && this._objs[i].Exists && this._objs[i].Visible) {
                            invList = invList + this._objs[i].Prefix;
                            if (this._objs[i].ObjectAlias == "") {
                                invList = invList + "|b" + this._objs[i].ObjectName + "|xb";
                            } else {
                                invList = invList + "|b" + this._objs[i].ObjectAlias + "|xb";
                            }
                            if (this._objs[i].Suffix != "") {
                                invList = invList + " " + this._objs[i].Suffix;
                            }
                            invList = invList + ", ";
                        }
                    }
                } else {
                    for (var j = 1; j <= this._numberItems; j++) {
                        if (this._items[j].Got == true) {
                            invList = invList + this._items[j].Name + ", ";
                        }
                    }
                }
                if (invList != "") {
                    invList = Left(invList, Len(invList) - 2);
                    invList = UCase(Left(invList, 1)) + Mid(invList, 2);
                    var pos = 1;
                    var lastComma;
                    var thisComma;
                    do {
                        thisComma = InStr(pos, invList, ",");
                        if (thisComma != 0) {
                            lastComma = thisComma;
                            pos = thisComma + 1;
                        }
                    } while(!(thisComma == 0));
                    if (lastComma != 0) {
                        invList = Left(invList, lastComma - 1) + " and" + Mid(invList, lastComma + 1);
                    }
                    this.Print("You are carrying:|n" + invList + ".", ctx);
                } else {
                    this.Print("You are not carrying anything.", ctx);
                }
            } else if (this.CmdStartsWith(input, "oops ")) {
                this.ExecOops(this.GetEverythingAfter(input, "oops "), ctx);
            } else if (this.CmdStartsWith(input, "the ")) {
                this.ExecOops(this.GetEverythingAfter(input, "the "), ctx);
            } else {
                this.PlayerErrorMessage(0 /* BadCommand */, ctx);
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
                    } else {
                        this.ExecuteScript(this._rooms[roomID].AfterTurnScript, ctx);
                    }
                }
            }

            // was set to NullThread here for some reason
            if (this._afterTurnScript != "" && globalOverride == false) {
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
        var id;
        var script = "";
        var toPos = InStr(giveString, " to ");
        if (toPos == 0) {
            toPos = InStr(giveString, " the ");
            if (toPos == 0) {
                this.PlayerErrorMessage(2 /* BadGive */, ctx);
                // UNKNOWN ExitSubStatement
            } else {
                item = Trim(Mid(giveString, toPos + 4, Len(giveString) - (toPos + 2)));
                character = Trim(Mid(giveString, 1, toPos));
            }
        } else {
            item = Trim(Mid(giveString, 1, toPos));
            character = Trim(Mid(giveString, toPos + 3, Len(giveString) - (toPos + 2)));
        }
        if (this._gameAslVersion >= 281) {
            type = 1 /* Object */;
        } else {
            type = 0 /* Character */;
        }

        // First see if player has "ItemToGive":
        if (this._gameAslVersion >= 280) {
            id = this.Disambiguate(item, "inventory", ctx);
            if (id == -1) {
                this.PlayerErrorMessage(4 /* NoItem */, ctx);
                this._badCmdBefore = "give";
                this._badCmdAfter = "to " + character;
                // UNKNOWN ExitSubStatement
            } else if (id == -2) {
                // UNKNOWN ExitSubStatement
            } else {
                article = this._objs[id].Article;
            }
        } else {
            // ASL2:
            var notGot = true;
            for (var i = 1; i <= this._numberItems; i++) {
                if (LCase(this._items[i].Name) == LCase(item)) {
                    if (this._items[i].Got == false) {
                        notGot = true;
                        i = this._numberItems;
                    } else {
                        notGot = false;
                    }
                }
            }
            if (notGot == true) {
                this.PlayerErrorMessage(4 /* NoItem */, ctx);
                // UNKNOWN ExitSubStatement
            } else {
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
                    this.PlayerErrorMessage(3 /* BadCharacter */, ctx);
                }
                this._badCmdBefore = "give " + item + " to";
                // UNKNOWN ExitSubStatement
            }

            //Find appropriate give script ****
            //now, for "give a to b", we have
            //ItemID=a and GiveToObjectID=b
            var o = this._objs[giveToId];
            for (var i = 1; i <= o.NumberGiveData; i++) {
                if (o.GiveData[i].GiveType == 1 /* GiveSomethingTo */ && LCase(o.GiveData[i].GiveObject) == LCase(this._objs[id].ObjectName)) {
                    foundScript = true;
                    script = o.GiveData[i].GiveScript;
                    break;
                }
            }
            if (!foundScript) {
                //check a for give to <b>:
                var g = this._objs[id];
                for (var i = 1; i <= g.NumberGiveData; i++) {
                    if (g.GiveData[i].GiveType == 0 /* GiveToSomething */ && LCase(g.GiveData[i].GiveObject) == LCase(this._objs[giveToId].ObjectName)) {
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
            } else {
                this.SetStringContents("quest.error.charactername", this._objs[giveToId].ObjectName, ctx);
                var gender = Trim(this._objs[giveToId].Gender);
                gender = UCase(Left(gender, 1)) + Mid(gender, 2);
                this.SetStringContents("quest.error.gender", gender, ctx);
                this.SetStringContents("quest.error.article", article, ctx);
                this.PlayerErrorMessage(5 /* ItemUnwanted */, ctx);
            }
        } else {
            // ASL2:
            var block = this.GetThingBlock(character, this._currentRoom, type);
            if ((block.StartLine == 0 && block.EndLine == 0) || this.IsAvailable(character + "@" + this._currentRoom, type, ctx) == false) {
                this.PlayerErrorMessage(3 /* BadCharacter */, ctx);
                // UNKNOWN ExitSubStatement
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
                this.PlayerErrorMessage(5 /* ItemUnwanted */, ctx);
                // UNKNOWN ExitSubStatement
            }

            // now, execute the statement on GiveLine
            this.ExecuteScript(this.GetSecondChunk(this._lines[giveLine]), ctx);
        }
    };
    LegacyGame.prototype.ExecLook = function (lookLine, ctx) {
        var item;
        if (Trim(lookLine) == "look") {
            this.ShowRoomInfo((this._currentRoom), ctx);
        } else {
            if (this._gameAslVersion < 391) {
                var atPos = InStr(lookLine, " at ");
                if (atPos == 0) {
                    item = this.GetEverythingAfter(lookLine, "look ");
                } else {
                    item = Trim(Mid(lookLine, atPos + 4));
                }
            } else {
                if (this.BeginsWith(lookLine, "look at ")) {
                    item = this.GetEverythingAfter(lookLine, "look at ");
                } else if (this.BeginsWith(lookLine, "look in ")) {
                    item = this.GetEverythingAfter(lookLine, "look in ");
                } else if (this.BeginsWith(lookLine, "look on ")) {
                    item = this.GetEverythingAfter(lookLine, "look on ");
                } else if (this.BeginsWith(lookLine, "look inside ")) {
                    item = this.GetEverythingAfter(lookLine, "look inside ");
                } else {
                    item = this.GetEverythingAfter(lookLine, "look ");
                }
            }
            if (this._gameAslVersion >= 280) {
                var id = this.Disambiguate(item, "inventory;" + this._currentRoom, ctx);
                if (id <= 0) {
                    if (id != -2) {
                        this.PlayerErrorMessage(7 /* BadThing */, ctx);
                    }
                    this._badCmdBefore = "look at";
                    // UNKNOWN ExitSubStatement
                }
                this.DoLook(id, ctx);
            } else {
                if (this.BeginsWith(item, "the ")) {
                    item = this.GetEverythingAfter(item, "the ");
                }
                lookLine = this.RetrLine("object", item, "look", ctx);
                if (lookLine != "<unfound>") {
                    //Check for availability
                    if (this.IsAvailable(item, 1 /* Object */, ctx) == false) {
                        lookLine = "<unfound>";
                    }
                }
                if (lookLine == "<unfound>") {
                    lookLine = this.RetrLine("character", item, "look", ctx);
                    if (lookLine != "<unfound>") {
                        if (this.IsAvailable(item, 0 /* Character */, ctx) == false) {
                            lookLine = "<unfound>";
                        }
                    }
                    if (lookLine == "<unfound>") {
                        this.PlayerErrorMessage(7 /* BadThing */, ctx);
                        // UNKNOWN ExitSubStatement
                    } else if (lookLine == "<undefined>") {
                        this.PlayerErrorMessage(8 /* DefaultLook */, ctx);
                        // UNKNOWN ExitSubStatement
                    }
                } else if (lookLine == "<undefined>") {
                    this.PlayerErrorMessage(8 /* DefaultLook */, ctx);
                    // UNKNOWN ExitSubStatement
                }
                var lookData = Trim(this.GetEverythingAfter(Trim(lookLine), "look "));
                if (Left(lookData, 1) == "<") {
                    var LookText = this.GetParameter(lookLine, ctx);
                    this.Print(LookText, ctx);
                } else {
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
                    this.PlayerErrorMessage(7 /* BadThing */, ctx);
                }
                this._badCmdBefore = "speak to";
                // UNKNOWN ExitSubStatement
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
                this.PlayerErrorMessage(9 /* DefaultSpeak */, ctx);
                // UNKNOWN ExitSubStatement
            }
            speakLine = this.GetEverythingAfter(speakLine, "speak ");
            if (this.BeginsWith(speakLine, "<")) {
                var text = this.GetParameter(speakLine, ctx);
                if (this._gameAslVersion >= 350) {
                    this.Print(text, ctx);
                } else {
                    this.Print(Chr(34) + text + Chr(34), ctx);
                }
            } else {
                this.ExecuteScript(speakLine, ctx, ObjID);
            }
        } else {
            var line = this.RetrLine("character", cmd, "speak", ctx);
            var type = 0 /* Character */;
            var data = Trim(this.GetEverythingAfter(Trim(line), "speak "));
            if (line != "<unfound>" && line != "<undefined>") {
                // Character exists; but is it available??
                if (this.IsAvailable(cmd + "@" + this._currentRoom, type, ctx) == false) {
                    line = "<undefined>";
                }
            }
            if (line == "<undefined>") {
                this.PlayerErrorMessage(3 /* BadCharacter */, ctx);
            } else if (line == "<unfound>") {
                this.SetStringContents("quest.error.gender", Trim(this.GetGender(cmd, true, ctx)), ctx);
                this.SetStringContents("quest.error.charactername", cmd, ctx);
                this.PlayerErrorMessage(9 /* DefaultSpeak */, ctx);
            } else if (this.BeginsWith(data, "<")) {
                data = this.GetParameter(line, ctx);
                this.Print(Chr(34) + data + Chr(34), ctx);
            } else {
                this.ExecuteScript(data, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecTake = function (item, ctx) {
        var parentID;
        var parentDisplayName;
        var foundItem = true;
        var foundTake = false;
        var id = this.Disambiguate(item, this._currentRoom, ctx);
        if (id < 0) {
            foundItem = false;
        } else {
            foundItem = true;
        }
        if (!foundItem) {
            if (id != -2) {
                if (this._gameAslVersion >= 410) {
                    id = this.Disambiguate(item, "inventory", ctx);
                    if (id >= 0) {
                        // Player already has this item
                        this.PlayerErrorMessage(37 /* AlreadyTaken */, ctx);
                    } else {
                        this.PlayerErrorMessage(7 /* BadThing */, ctx);
                    }
                } else if (this._gameAslVersion >= 391) {
                    this.PlayerErrorMessage(7 /* BadThing */, ctx);
                } else {
                    this.PlayerErrorMessage(10 /* BadItem */, ctx);
                }
            }
            this._badCmdBefore = "take";
            // UNKNOWN ExitSubStatement
        } else {
            this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
        }
        var isInContainer = false;
        if (this._gameAslVersion >= 391) {
            var canAccessObject = this.PlayerCanAccessObject(id);
            if (!canAccessObject.CanAccessObject) {
                this.PlayerErrorMessage_ExtendInfo(18 /* BadTake */, ctx, canAccessObject.ErrorMsg);
                // UNKNOWN ExitSubStatement
            }
            var parent = this.GetObjectProperty("parent", id, false, false);
            parentID = this.GetObjectIdNoAlias(parent);
        }
        if (this._gameAslVersion >= 280) {
            var t = this._objs[id].Take;
            if (isInContainer && (t.Type == 3 /* Default */ || t.Type == 0 /* Text */)) {
                // So, we want to take an object that's in a container or surface. So first
                // we have to remove the object from that container.
                if (this._objs[parentID].ObjectAlias != "") {
                    parentDisplayName = this._objs[parentID].ObjectAlias;
                } else {
                    parentDisplayName = this._objs[parentID].ObjectName;
                }
                this.Print("(first removing " + this._objs[id].Article + " from " + parentDisplayName + ")", ctx);

                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                this.ExecCommand("remove " + this._objs[id].ObjectName, ctx, false, null, true);
                if (this.GetObjectProperty("parent", id, false, false) != "") {
                    // removing the object failed
                    // UNKNOWN ExitSubStatement
                }
            }
            if (t.Type == 3 /* Default */) {
                this.PlayerErrorMessage(11 /* DefaultTake */, ctx);
                this.PlayerItem(item, true, ctx, id);
            } else if (t.Type == 0 /* Text */) {
                this.Print(t.Data, ctx);
                this.PlayerItem(item, true, ctx, id);
            } else if (t.Type == 1 /* Script */) {
                this.ExecuteScript(t.Data, ctx, id);
            } else {
                this.PlayerErrorMessage(18 /* BadTake */, ctx);
            }
        } else {
            for (var i = this._objs[id].DefinitionSectionStart + 1; i <= this._objs[id].DefinitionSectionEnd - 1; i++) {
                if (this.BeginsWith(this._lines[i], "take")) {
                    var script = Trim(this.GetEverythingAfter(Trim(this._lines[i]), "take"));
                    this.ExecuteScript(script, ctx, id);
                    foundTake = true;
                    i = this._objs[id].DefinitionSectionEnd;
                }
            }
            if (!foundTake) {
                this.PlayerErrorMessage(18 /* BadTake */, ctx);
            }
        }
    };
    LegacyGame.prototype.ExecUse = function (useLine, ctx) {
        var endOnWith;
        var useDeclareLine = "";
        var useOn;
        var useItem;
        useLine = Trim(this.GetEverythingAfter(useLine, "use "));
        var roomId;
        roomId = this.GetRoomID(this._currentRoom, ctx);
        var onWithPos = InStr(useLine, " on ");
        if (onWithPos == 0) {
            onWithPos = InStr(useLine, " with ");
            endOnWith = onWithPos + 4;
        } else {
            endOnWith = onWithPos + 2;
        }
        if (onWithPos != 0) {
            useOn = Trim(Right(useLine, Len(useLine) - endOnWith));
            useItem = Trim(Left(useLine, onWithPos - 1));
        } else {
            useOn = "";
            useItem = useLine;
        }

        // see if player has this item:
        var id;
        var notGotItem;
        if (this._gameAslVersion >= 280) {
            var foundItem = false;
            id = this.Disambiguate(useItem, "inventory", ctx);
            if (id > 0) {
                foundItem = true;
            }
            if (!foundItem) {
                if (id != -2) {
                    this.PlayerErrorMessage(4 /* NoItem */, ctx);
                }
                if (useOn == "") {
                    this._badCmdBefore = "use";
                } else {
                    this._badCmdBefore = "use";
                    this._badCmdAfter = "on " + useOn;
                }
                // UNKNOWN ExitSubStatement
            }
        } else {
            notGotItem = true;
            for (var i = 1; i <= this._numberItems; i++) {
                if (LCase(this._items[i].Name) == LCase(useItem)) {
                    if (this._items[i].Got == false) {
                        notGotItem = true;
                        i = this._numberItems;
                    } else {
                        notGotItem = false;
                    }
                }
            }
            if (notGotItem == true) {
                this.PlayerErrorMessage(4 /* NoItem */, ctx);
                // UNKNOWN ExitSubStatement
            }
        }
        var useScript = "";
        var foundUseScript;
        var foundUseOnObject;
        var useOnObjectId;
        var found;
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
            } else {
                foundUseOnObject = false;
                useOnObjectId = this.Disambiguate(useOn, this._currentRoom, ctx);
                if (useOnObjectId > 0) {
                    foundUseOnObject = true;
                } else {
                    useOnObjectId = this.Disambiguate(useOn, "inventory", ctx);
                    if (useOnObjectId > 0) {
                        foundUseOnObject = true;
                    }
                }
                if (!foundUseOnObject) {
                    if (useOnObjectId != -2) {
                        this.PlayerErrorMessage(7 /* BadThing */, ctx);
                    }
                    this._badCmdBefore = "use " + useItem + " on";
                    // UNKNOWN ExitSubStatement
                }

                //now, for "use a on b", we have
                //ItemID=a and UseOnObjectID=b
                //first check b for use <a>:
                var o = this._objs[useOnObjectId];
                for (var i = 1; i <= o.NumberUseData; i++) {
                    if (o.UseData[i].UseType == 1 /* UseSomethingOn */ && LCase(o.UseData[i].UseObject) == LCase(this._objs[id].ObjectName)) {
                        foundUseScript = true;
                        useScript = o.UseData[i].UseScript;
                        break;
                    }
                }
                if (!foundUseScript) {
                    //check a for use on <b>:
                    var u = this._objs[id];
                    for (var i = 1; i <= u.NumberUseData; i++) {
                        if (u.UseData[i].UseType == 0 /* UseOnSomething */ && LCase(u.UseData[i].UseObject) == LCase(this._objs[useOnObjectId].ObjectName)) {
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
            } else {
                this.PlayerErrorMessage(13 /* DefaultUse */, ctx);
            }
        } else {
            if (useOn != "") {
                useDeclareLine = this.RetrLineParam("object", useOn, "use", useItem, ctx);
            } else {
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
                    this.PlayerErrorMessage(13 /* DefaultUse */, ctx);
                    // UNKNOWN ExitSubStatement
                }
            }
            if (useDeclareLine != "<unfound>" && useDeclareLine != "<undefined>" && useOn != "") {
                //Check for object availablity
                if (this.IsAvailable(useOn, 1 /* Object */, ctx) == false) {
                    useDeclareLine = "<undefined>";
                }
            }
            if (useDeclareLine == "<undefined>") {
                useDeclareLine = this.RetrLineParam("character", useOn, "use", useItem, ctx);
                if (useDeclareLine != "<undefined>") {
                    //Check for character availability
                    if (this.IsAvailable(useOn, 0 /* Character */, ctx) == false) {
                        useDeclareLine = "<undefined>";
                    }
                }
                if (useDeclareLine == "<undefined>") {
                    this.PlayerErrorMessage(7 /* BadThing */, ctx);
                    // UNKNOWN ExitSubStatement
                } else if (useDeclareLine == "<unfound>") {
                    this.PlayerErrorMessage(13 /* DefaultUse */, ctx);
                    // UNKNOWN ExitSubStatement
                }
            } else if (useDeclareLine == "<unfound>") {
                this.PlayerErrorMessage(13 /* DefaultUse */, ctx);
                // UNKNOWN ExitSubStatement
            }
            var script = Right(useDeclareLine, Len(useDeclareLine) - InStr(useDeclareLine, ">"));
            this.ExecuteScript(script, ctx);
        }
    };
    LegacyGame.prototype.ObjectActionUpdate = function (id, name, script, noUpdate) {
        if (typeof noUpdate === "undefined") { noUpdate = false; }
        var objectName;
        var sp;
        var ep;
        name = LCase(name);
        if (!noUpdate) {
            if (name == "take") {
                this._objs[id].Take.Data = script;
                this._objs[id].Take.Type = 1 /* Script */;
            } else if (name == "use") {
                this.AddToUseInfo(id, script);
            } else if (name == "gain") {
                this._objs[id].GainScript = script;
            } else if (name == "lose") {
                this._objs[id].LoseScript = script;
            } else if (this.BeginsWith(name, "use ")) {
                name = this.GetEverythingAfter(name, "use ");
                if (InStr(name, "'") > 0) {
                    sp = InStr(name, "'");
                    ep = InStr(sp + 1, name, "'");
                    if (ep == 0) {
                        this.LogASLError("Missing ' in 'action <use " + name + "> " + this.ReportErrorLine(script));
                        // UNKNOWN ExitSubStatement
                    }
                    objectName = Mid(name, sp + 1, ep - sp - 1);
                    this.AddToUseInfo(id, Trim(Left(name, sp - 1)) + " <" + objectName + "> " + script);
                } else {
                    this.AddToUseInfo(id, name + " " + script);
                }
            } else if (this.BeginsWith(name, "give ")) {
                name = this.GetEverythingAfter(name, "give ");
                if (InStr(name, "'") > 0) {
                    sp = InStr(name, "'");
                    ep = InStr(sp + 1, name, "'");
                    if (ep == 0) {
                        this.LogASLError("Missing ' in 'action <give " + name + "> " + this.ReportErrorLine(script));
                        // UNKNOWN ExitSubStatement
                    }
                    objectName = Mid(name, sp + 1, ep - sp - 1);
                    this.AddToGiveInfo(id, Trim(Left(name, sp - 1)) + " <" + objectName + "> " + script);
                } else {
                    this.AddToGiveInfo(id, name + " " + script);
                }
            }
        }
        if (this._gameFullyLoaded) {
            this.AddToObjectChangeLog(ChangeLog.AppliesTo.Object, this._objs[id].ObjectName, name, "action <" + name + "> " + script);
        }
    };
    LegacyGame.prototype.ExecuteIf = function (scriptLine, ctx) {
        var ifLine = Trim(this.GetEverythingAfter(Trim(scriptLine), "if "));
        var obscuredLine = this.ObliterateParameters(ifLine);
        var thenPos = InStr(obscuredLine, "then");
        if (thenPos == 0) {
            var errMsg = "Expected 'then' missing from script statement '" + this.ReportErrorLine(scriptLine) + "' - statement bypassed.";
            this.LogASLError(errMsg, 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        var conditions = Trim(Left(ifLine, thenPos - 1));
        thenPos = thenPos + 4;
        var elsePos = InStr(obscuredLine, "else");
        var thenEndPos;
        if (elsePos == 0) {
            thenEndPos = Len(obscuredLine) + 1;
        } else {
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
        } else {
            if (elsePos != 0) {
                this.ExecuteScript((elseScript), ctx);
            }
        }
    };
    LegacyGame.prototype.ExecuteScript = function (scriptLine, ctx, newCallingObjectId) {
        if (typeof newCallingObjectId === "undefined") { newCallingObjectId = 0; }
        // UNKNOWN TryBlock
    };
    LegacyGame.prototype.ExecuteEnter = function (scriptLine, ctx) {
        this._commandOverrideModeOn = true;
        this._commandOverrideVariable = this.GetParameter(scriptLine, ctx);

        // Now, wait for CommandOverrideModeOn to be set
        // to False by ExecCommand. Execution can then resume.
        this.ChangeState(2 /* Waiting */, true);

        // UNKNOWN SyncLockBlock
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
                    this.LogASLError("Too few parameters in 'set " + setInstruction + "'", 2 /* WarningError */);
                    // UNKNOWN ExitSubStatement
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
                    this.LogASLError("No such timer '" + name + "'", 2 /* WarningError */);
                    // UNKNOWN ExitSubStatement
                }
            } else if (this.BeginsWith(setInstruction, "string ")) {
                this.ExecSetString(this.GetParameter(setInstruction, ctx), ctx);
            } else if (this.BeginsWith(setInstruction, "numeric ")) {
                this.ExecSetVar(this.GetParameter(setInstruction, ctx), ctx);
            } else if (this.BeginsWith(setInstruction, "collectable ")) {
                this.ExecuteSetCollectable(this.GetParameter(setInstruction, ctx), ctx);
            } else {
                var result = this.SetUnknownVariableType(this.GetParameter(setInstruction, ctx), ctx);
                if (result == 0 /* Error */) {
                    this.LogASLError("Error on setting 'set " + setInstruction + "'", 2 /* WarningError */);
                } else if (result == 2 /* Unfound */) {
                    this.LogASLError("Variable type not specified in 'set " + setInstruction + "'", 2 /* WarningError */);
                }
            }
        } else {
            this.ExecuteSetCollectable(this.GetParameter(setInstruction, ctx), ctx);
        }
    };
    LegacyGame.prototype.FindStatement = function (block, statement) {
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            // Ignore sub-define blocks
            if (this.BeginsWith(this._lines[i], "define ")) {
                do {
                    i = i + 1;
                } while(!(Trim(this._lines[i]) == "end define"));
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
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            // Ignore sub-define blocks
            if (this.BeginsWith(this._lines[i], "define ")) {
                do {
                    i = i + 1;
                } while(!(Trim(this._lines[i]) == "end define"));
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
        var id = this.GetRoomID(this._currentRoom, ctx);
        if (id == 0) {
            // UNKNOWN ExitSubStatement
        }
        if (this._gameAslVersion >= 410) {
            this._rooms[id].Exits.ExecuteGo(direction, ctx);
            // UNKNOWN ExitSubStatement
        }
        var r = this._rooms[id];
        if (direction == "north") {
            dirData = r.North;
        } else if (direction == "south") {
            dirData = r.South;
        } else if (direction == "west") {
            dirData = r.West;
        } else if (direction == "east") {
            dirData = r.East;
        } else if (direction == "northeast") {
            dirData = r.NorthEast;
        } else if (direction == "northwest") {
            dirData = r.NorthWest;
        } else if (direction == "southeast") {
            dirData = r.SouthEast;
        } else if (direction == "southwest") {
            dirData = r.SouthWest;
        } else if (direction == "up") {
            dirData = r.Up;
        } else if (direction == "down") {
            dirData = r.Down;
        } else if (direction == "out") {
            if (r.Out.Script == "") {
                dirData.Data = r.Out.Text;
                dirData.Type = 0 /* Text */;
            } else {
                dirData.Data = r.Out.Script;
                dirData.Type = 1 /* Script */;
            }
        }
        if (dirData.Type == 1 /* Script */ && dirData.Data != "") {
            this.ExecuteScript(dirData.Data, ctx);
        } else if (dirData.Data != "") {
            var newRoom = dirData.Data;
            var scp = InStr(newRoom, ";");
            if (scp != 0) {
                newRoom = Trim(Mid(newRoom, scp + 1));
            }
            this.PlayGame(newRoom, ctx);
        } else {
            if (direction == "out") {
                this.PlayerErrorMessage(14 /* DefaultOut */, ctx);
            } else {
                this.PlayerErrorMessage(15 /* BadPlace */, ctx);
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
        } else if (this.BeginsWith(place, "the ")) {
            var np = this.GetEverythingAfter(place, "the ");
            placeData = this.PlaceExist(np, ctx);
            if (placeData != "") {
                destination = placeData;
            } else {
                disallowed = true;
            }
        } else {
            disallowed = true;
        }
        if (destination != "") {
            if (InStr(destination, ";") > 0) {
                var s = Trim(Right(destination, Len(destination) - InStr(destination, ";")));
                this.ExecuteScript(s, ctx);
            } else {
                this.PlayGame(destination, ctx);
            }
        }
        if (disallowed == true) {
            this.PlayerErrorMessage(15 /* BadPlace */, ctx);
        }
    };
    LegacyGame.prototype.InitialiseGame = function (filename, fromQsg) {
        if (typeof fromQsg === "undefined") { fromQsg = false; }
        this._loadedFromQsg = fromQsg;
        this._changeLogRooms = new ChangeLog();
        this._changeLogObjects = new ChangeLog();
        this._changeLogRooms.AppliesToType = ChangeLog.AppliesTo.Room;
        this._changeLogObjects.AppliesToType = ChangeLog.AppliesTo.Object;
        this._outPutOn = true;
        this._useAbbreviations = true;
        this._gamePath = System.IO.Path.GetDirectoryName(filename) + "\\";
        this.LogASLError("Opening file " + filename + " on " + Date.Now.ToString(), 3 /* Init */);

        // Parse file and find where the 'define' blocks are:
        if (this.ParseFile(filename) == false) {
            this.LogASLError("Unable to open file", 3 /* Init */);
            var err = "Unable to open " + filename;
            if (this._openErrorReport != "") {
                // Strip last vbcrlf
                this._openErrorReport = Left(this._openErrorReport, Len(this._openErrorReport) - 2);
                err = err + ":" + vbCrLf + vbCrLf + this._openErrorReport;
            }
            this.Print("Error: " + err, this._nullContext);
            return false;
        }

        // Check version
        var gameBlock;
        gameBlock = this.GetDefineBlock("game");
        var aslVersion = "//";
        for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "asl-version ")) {
                aslVersion = this.GetParameter(this._lines[i], this._nullContext);
            }
        }
        if (aslVersion == "//") {
            this.LogASLError("File contains no version header.", 2 /* WarningError */);
        } else {
            this._gameAslVersion = parseInt(aslVersion);
            var recognisedVersions = "/100/200/210/217/280/281/282/283/284/285/300/310/311/320/350/390/391/392/400/410/";
            if (InStr(recognisedVersions, "/" + aslVersion + "/") == 0) {
                this.LogASLError("Unrecognised ASL version number.", 2 /* WarningError */);
            }
        }
        this._listVerbs.Add(ListType.ExitsList, new any());
        if (this._gameAslVersion >= 280 && this._gameAslVersion < 390) {
            this._listVerbs.Add(ListType.ObjectsList, new any());
            this._listVerbs.Add(ListType.InventoryList, new any());
        } else {
            this._listVerbs.Add(ListType.ObjectsList, new any());
            this._listVerbs.Add(ListType.InventoryList, new any());
        }

        // Get the name of the game:
        this._gameName = this.GetParameter(this._lines[this.GetDefineBlock("game").StartLine], this._nullContext);
        this._player.UpdateGameName(this._gameName);
        this._player.Show("Panes");
        this._player.Show("Location");
        this._player.Show("Command");
        this.SetUpGameObject();
        this.SetUpOptions();
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "beforesave ")) {
                this._beforeSaveScript = this.GetEverythingAfter(this._lines[i], "beforesave ");
            } else if (this.BeginsWith(this._lines[i], "onload ")) {
                this._onLoadScript = this.GetEverythingAfter(this._lines[i], "onload ");
            }
        }
        this.SetDefaultPlayerErrorMessages();
        this.SetUpSynonyms();
        this.SetUpRoomData();
        if (this._gameAslVersion >= 410) {
            this.SetUpExits();
        }
        if (this._gameAslVersion < 280) {
            // Set up an array containing the names of all the items
            // used in the game, based on the possitems statement
            // of the 'define game' block.
            this.SetUpItemArrays();
        }
        if (this._gameAslVersion < 280) {
            // Now, go through the 'startitems' statement and set up
            // the items array so we start with those items mentioned.
            this.SetUpStartItems();
        }

        // Set up collectables.
        this.SetUpCollectables();
        this.SetUpDisplayVariables();

        // Set up characters and objects.
        this.SetUpCharObjectInfo();
        this.SetUpUserDefinedPlayerErrors();
        this.SetUpDefaultFonts();
        this.SetUpTurnScript();
        this.SetUpTimers();
        this.SetUpMenus();
        this._gameFileName = filename;
        this.LogASLError("Finished loading file.", 3 /* Init */);
        this._defaultRoomProperties = this.GetPropertiesInType("defaultroom", false);
        this._defaultProperties = this.GetPropertiesInType("default", false);
        return true;
    };
    LegacyGame.prototype.PlaceExist = function (placeName, ctx) {
        // Returns actual name of an available "place" exit, and if
        // script is executed on going in that direction, that script
        // is returned after a ";"
        var roomId = this.GetRoomID(this._currentRoom, ctx);
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
                var destRoomId = this.GetRoomID(checkPlace, ctx);
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
                } else {
                    return checkPlace;
                }
            }
        }
        return "";
    };
    LegacyGame.prototype.PlayerItem = function (item, got, ctx, objId) {
        if (typeof objId === "undefined") { objId = 0; }
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
                    this.MoveThing(this._objs[objId].ObjectName, "inventory", 1 /* Object */, ctx);
                    if (this._objs[objId].GainScript != "") {
                        this.ExecuteScript(this._objs[objId].GainScript, ctx);
                    }
                } else {
                    this.MoveThing(this._objs[objId].ObjectName, this._currentRoom, 1 /* Object */, ctx);
                    if (this._objs[objId].LoseScript != "") {
                        this.ExecuteScript(this._objs[objId].LoseScript, ctx);
                    }
                }
                foundObjectName = true;
            }
            if (!foundObjectName) {
                this.LogASLError("No such object '" + item + "'", 2 /* WarningError */);
            } else {
                this.UpdateItems(ctx);
                this.UpdateObjectList(ctx);
            }
        } else {
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
        var id = this.GetRoomID(room, ctx);
        if (id == 0) {
            this.LogASLError("No such room '" + room + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
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
        } else {
            for (var i = 1; i <= Len(txt); i++) {
                var printThis = true;
                if (Mid(txt, i, 2) == "|w") {
                    this.DoPrint(printString);
                    printString = "";
                    printThis = false;
                    i = i + 1;
                    this.ExecuteScript("wait <>", ctx);
                } else if (Mid(txt, i, 2) == "|c") {
                    // UNKNOWN SelectBlock
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
            searchblock = this.GetThingBlock(param, this._currentRoom, 1 /* Object */);
        } else {
            searchblock = this.GetThingBlock(param, this._currentRoom, 0 /* Character */);
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
            searchblock = this.GetThingBlock(param, this._currentRoom, 1 /* Object */);
        } else {
            searchblock = this.GetThingBlock(param, this._currentRoom, 0 /* Character */);
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
                        var nextComma = InStr(pos + 1, collectables, ",");
                        if (nextComma == 0) {
                            nextComma = InStr(pos + 1, collectables, ";");
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
                        var sp2 = InStr(ep, info, " ");
                        if (sp2 == 0) {
                            sp2 = Len(info) + 1;
                        }
                        var t = Trim(Mid(info, sp1 + 1, ep - sp1 - 1));
                        var i = Trim(Mid(info, ep + 1, sp2 - ep - 1));
                        if (Left(t, 1) == "d") {
                            t = Mid(t, 2);
                            this._collectables[this._numCollectables].DisplayWhenZero = false;
                        } else {
                            this._collectables[this._numCollectables].DisplayWhenZero = true;
                        }
                        this._collectables[this._numCollectables].Type = t;
                        this._collectables[this._numCollectables].Value = Val(i);

                        // Get display string between square brackets
                        var obp = InStr(info, "[");
                        var cbp = InStr(info, "]");
                        if (obp == 0) {
                            this._collectables[this._numCollectables].Display = "<def>";
                        } else {
                            var b = Mid(info, obp + 1, (cbp - 1) - obp);
                            this._collectables[this._numCollectables].Display = Trim(b);
                        }
                        pos = nextComma + 1;
                        this._numCollectables = this._numCollectables + 1;
                        //lastitem set when nextcomma=0, above.
                    } while(!(lastItem == true));
                    this._numCollectables = this._numCollectables - 1;
                }
            }
        }
    };
    LegacyGame.prototype.SetUpItemArrays = function () {
        var lastItem = false;
        this._numberItems = 0;

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
                        var nextComma = InStr(pos + 1, possItems, ",");
                        if (nextComma == 0) {
                            nextComma = InStr(pos + 1, possItems, ";");
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
                        //lastitem set when nextcomma=0, above.
                    } while(!(lastItem == true));
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
                        var nextComma = InStr(pos + 1, startItems, ",");
                        if (nextComma == 0) {
                            nextComma = InStr(pos + 1, startItems, ";");
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

                        for (var i = 1; i <= this._numberItems; i++) {
                            if (this._items[i].Name == name) {
                                this._items[i].Got = true;
                                break;
                            }
                        }
                        pos = nextComma + 1;
                        //lastitem set when nextcomma=0, above.
                    } while(!(lastItem == true));
                }
            }
        }
    };
    LegacyGame.prototype.ShowHelp = function (ctx) {
        // In Quest 4 and below, the help text displays in a separate window. In Quest 5, it displays
        // in the same window as the game text.
        this.Print("|b|cl|s14Quest Quick Help|xb|cb|s00", ctx);
        this.Print("", ctx);
        this.Print("|cl|bMoving|xb|cb Press the direction buttons in the 'Compass' pane, or type |bGO NORTH|xb, |bSOUTH|xb, |bE|xb, etc. |xn", ctx);
        this.Print("To go into a place, type |bGO TO ...|xb . To leave a place, type |bOUT, EXIT|xb or |bLEAVE|xb, or press the '|crOUT|cb' button.|n", ctx);
        this.Print("|cl|bObjects and Characters|xb|cb Use |bTAKE ...|xb, |bGIVE ... TO ...|xb, |bTALK|xb/|bSPEAK TO ...|xb, |bUSE ... ON|xb/|bWITH ...|xb, |bLOOK AT ...|xb, etc.|n", ctx);
        this.Print("|cl|bExit Quest|xb|cb Type |bQUIT|xb to leave Quest.|n", ctx);
        this.Print("|cl|bMisc|xb|cb Type |bABOUT|xb to get information on the current game. The next turn after referring to an object or character, you can use |bIT|xb, |bHIM|xb etc. as appropriate to refer to it/him/etc. again. If you make a mistake when typing an object's name, type |bOOPS|xb followed by your correction.|n", ctx);
        this.Print("|cl|bKeyboard shortcuts|xb|cb Press the |crup arrow|cb and |crdown arrow|cb to scroll through commands you have already typed in. Press |crEsc|cb to clear the command box.|n|n", ctx);
        this.Print("Further information is available by selecting |iQuest Documentation|xi from the |iHelp|xi menu.", ctx);
    };
    LegacyGame.prototype.ReadCatalog = function (data) {
        var nullPos = InStr(data, Chr(0));
        this._numResources = parseInt(DecryptString(Left(data, nullPos - 1)));
        if (!this._resources)
            this._resources = [];
        this._resources[this._numResources] = new ResourceType();
        data = Mid(data, nullPos + 1);
        var resourceStart = 0;
        for (var i = 1; i <= this._numResources; i++) {
            var r = this._resources[i];
            nullPos = InStr(data, Chr(0));
            r.ResourceName = DecryptString(Left(data, nullPos - 1));
            data = Mid(data, nullPos + 1);
            nullPos = InStr(data, Chr(0));
            r.ResourceLength = parseInt(DecryptString(Left(data, nullPos - 1)));
            data = Mid(data, nullPos + 1);
            r.ResourceStart = resourceStart;
            resourceStart = resourceStart + r.ResourceLength;
            r.Extracted = false;
        }
    };
    LegacyGame.prototype.UpdateDirButtons = function (dirs, ctx) {
        var compassExits = {};
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
        exitList.Add(new ListData());
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
        if (this._gameAslVersion >= 410) {
            this._rooms[roomId].Exits.GetAvailableDirectionsDescription(roomDisplayText, directions);
        } else {
            if (this._rooms[roomId].Out.Text != "") {
                outPlace = this._rooms[roomId].Out.Text;

                //remove any prefix semicolon from printed text
                var scp = InStr(outPlace, ";");
                if (scp == 0) {
                    outPlaceName = outPlace;
                } else {
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
                var outPlaceAlias = this._rooms[this.GetRoomID(outPlaceName, ctx)].RoomAlias;
                if (outPlaceAlias == "") {
                    outPlaceAlias = outPlace;
                } else {
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
                } else {
                    this.SetStringContents("quest.doorways.out", outPlaceAlias, ctx);
                }
                this.SetStringContents("quest.doorways.out.display", outPlaceAlias, ctx);
            } else {
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
                        var ncp = InStr(cp + 1, nsew, ",");
                        if (ncp == 0) {
                            finished = true;
                        } else {
                            cp = ncp;
                        }
                    } while(!(finished));
                    nsew = Trim(Left(nsew, cp - 1)) + " or " + Trim(Mid(nsew, cp + 1));
                }
                roomDisplayText = roomDisplayText + "You can go " + nsew + ".";
                this.SetStringContents("quest.doorways.dirs", nsew, ctx);
            } else {
                this.SetStringContents("quest.doorways.dirs", "", ctx);
            }
        }
        this.UpdateDirButtons(directions, ctx);
        return roomDisplayText;
    };
    LegacyGame.prototype.UpdateItems = function (ctx) {
        // displays the items a player has
        var invList = {};
        if (!this._outPutOn) {
            // UNKNOWN ExitSubStatement
        }
        var name;
        if (this._gameAslVersion >= 280) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (this._objs[i].ContainerRoom == "inventory" && this._objs[i].Exists && this._objs[i].Visible) {
                    if (this._objs[i].ObjectAlias == "") {
                        name = this._objs[i].ObjectName;
                    } else {
                        name = this._objs[i].ObjectAlias;
                    }
                    invList.Add(new ListData());
                }
            }
        } else {
            for (var j = 1; j <= this._numberItems; j++) {
                if (this._items[j].Got == true) {
                    invList.Add(new ListData());
                }
            }
        }

        // UNKNOWN RaiseEventStatement
        if (this._gameAslVersion >= 284) {
            this.UpdateStatusVars(ctx);
        } else {
            if (this._numCollectables > 0) {
                var status = "";
                for (var j = 1; j <= this._numCollectables; j++) {
                    var k = this.DisplayCollectableInfo(j);
                    if (k != "<null>") {
                        if (status.Length > 0) {
                            // UNKNOWN AddAssignmentStatement
                        }
                        // UNKNOWN AddAssignmentStatement
                    }
                }
                this._player.SetStatusText(status);
            }
        }
    };
    LegacyGame.prototype.FinishGame = function (stopType, ctx) {
        if (stopType == 0 /* Win */) {
            this.DisplayTextSection("win", ctx);
        } else if (stopType == 1 /* Lose */) {
            this.DisplayTextSection("lose", ctx);
        }
        this.GameFinished();
    };
    LegacyGame.prototype.UpdateObjectList = function (ctx) {
        // Updates object list
        var shownPlaceName;
        var objSuffix;
        var charsFound;
        var noFormatObjsViewable;
        var charList;
        var objsFound;
        var objListString;
        var noFormatObjListString;
        if (!this._outPutOn) {
            // UNKNOWN ExitSubStatement
        }
        var objList = {};
        var exitList = {};

        //find the room
        var roomBlock;
        roomBlock = this.DefineBlockParam("room", this._currentRoom);

        //FIND CHARACTERS ===
        if (this._gameAslVersion < 281) {
            for (var i = 1; i <= this._numberChars; i++) {
                if (this._chars[i].ContainerRoom == this._currentRoom && this._chars[i].Exists && this._chars[i].Visible) {
                    this.AddToObjectList(objList, exitList, this._chars[i].ObjectName, 0 /* Character */);
                    charsViewable = charsViewable + this._chars[i].Prefix + "|b" + this._chars[i].ObjectName + "|xb" + this._chars[i].Suffix + ", ";
                    charsFound = charsFound + 1;
                }
            }
            if (charsFound == 0) {
                this.SetStringContents("quest.characters", "", ctx);
            } else {
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
                    this.AddToObjectList(objList, exitList, this._objs[i].ObjectName, 1 /* Object */);
                    objsViewable = objsViewable + this._objs[i].Prefix + "|b" + this._objs[i].ObjectName + "|xb" + objSuffix + ", ";
                    noFormatObjsViewable = noFormatObjsViewable + this._objs[i].Prefix + this._objs[i].ObjectName + ", ";
                } else {
                    this.AddToObjectList(objList, exitList, this._objs[i].ObjectAlias, 1 /* Object */);
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
        } else {
            this.SetStringContents("quest.objects", "", ctx);
            this.SetStringContents("quest.formatobjects", "", ctx);
        }

        //FIND DOORWAYS
        var roomId;
        roomId = this.GetRoomID(this._currentRoom, ctx);
        if (roomId > 0) {
            if (this._gameAslVersion >= 410) {
                this._rooms[roomId].Exits.Places.Values.forEach(function (roomExit) {
                    this.AddToObjectList(objList, exitList, roomExit.DisplayName, 2 /* Room */);
                }, this);
            } else {
                var r = this._rooms[roomId];
                for (var i = 1; i <= r.NumberPlaces; i++) {
                    if (this._gameAslVersion >= 311 && this._rooms[roomId].Places[i].Script == "") {
                        var PlaceID = this.GetRoomID(this._rooms[roomId].Places[i].PlaceName, ctx);
                        if (PlaceID == 0) {
                            shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                        } else {
                            if (this._rooms[PlaceID].RoomAlias != "") {
                                shownPlaceName = this._rooms[PlaceID].RoomAlias;
                            } else {
                                shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                            }
                        }
                    } else {
                        shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                    }
                    this.AddToObjectList(objList, exitList, shownPlaceName, 2 /* Room */);
                }
            }
        }

        // UNKNOWN RaiseEventStatement
        this._gotoExits = exitList;
        this.UpdateExitsList();
    };
    LegacyGame.prototype.UpdateExitsList = function () {
        // The Quest 5.0 Player takes a combined list of compass and "go to" exits, whereas the
        // ASL4 code produces these separately. So we keep track of them separately and then
        // merge to send to the Player.
        var mergedList = {};
        this._compassExits.forEach(function (listItem) {
            mergedList.Add(listItem);
        }, this);
        this._gotoExits.forEach(function (listItem) {
            mergedList.Add(listItem);
        }, this);
        // UNKNOWN RaiseEventStatement
    };
    LegacyGame.prototype.UpdateStatusVars = function (ctx) {
        var displayData;
        var status = "";
        if (this._numDisplayStrings > 0) {
            for (var i = 1; i <= this._numDisplayStrings; i++) {
                displayData = this.DisplayStatusVariableInfo(i, 0 /* String */, ctx);
                if (displayData != "") {
                    if (status.Length > 0) {
                        // UNKNOWN AddAssignmentStatement
                    }
                    // UNKNOWN AddAssignmentStatement
                }
            }
        }
        if (this._numDisplayNumerics > 0) {
            for (var i = 1; i <= this._numDisplayNumerics; i++) {
                displayData = this.DisplayStatusVariableInfo(i, 1 /* Numeric */, ctx);
                if (displayData != "") {
                    if (status.Length > 0) {
                        // UNKNOWN AddAssignmentStatement
                    }
                    // UNKNOWN AddAssignmentStatement
                }
            }
        }
        this._player.SetStatusText(status);
    };
    LegacyGame.prototype.UpdateVisibilityInContainers = function (ctx, onlyParent) {
        if (typeof onlyParent === "undefined") { onlyParent = ""; }
        // Use OnlyParent to only update objects that are contained by a specific parent
        var parentId;
        var parent;
        var parentIsTransparent;
        var parentIsOpen;
        var parentIsSeen;
        var parentIsSurface;
        if (this._gameAslVersion < 391) {
            // UNKNOWN ExitSubStatement
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
                    } else {
                        this.SetAvailability(this._objs[i].ObjectName, false, ctx);
                    }
                }
            }
        }
    };
    LegacyGame.prototype.PlayerCanAccessObject = function (id, colObjects) {
        if (typeof colObjects === "undefined") { colObjects = null; }
        // Called to see if a player can interact with an object (take it, open it etc.).
        // For example, if the object is on a surface which is inside a closed container,
        // the object cannot be accessed.
        var parent;
        var parentId;
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
                } else {
                    parentDisplayName = this._objs[parentId].ObjectName;
                }
                result.CanAccessObject = false;
                result.ErrorMsg = "inside closed " + parentDisplayName;
                return result;
            }

            // Is the parent itself accessible?
            if (colObjects == null) {
                colObjects = new any();
            }
            if (colObjects.Contains(parentId)) {
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
            colObjects.Add(parentId);
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
                var PlaceID = this.GetRoomID(this._rooms[roomId].Places[i].PlaceName, ctx);
                if (PlaceID == 0) {
                    this.LogASLError("No such room '" + this._rooms[roomId].Places[i].PlaceName + "'", 2 /* WarningError */);
                    shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                } else {
                    if (this._rooms[PlaceID].RoomAlias != "") {
                        shownPlaceName = this._rooms[PlaceID].RoomAlias;
                    } else {
                        shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                    }
                }
            } else {
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
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define room ")) {
                var roomName = this.GetParameter(this._lines[this._defineBlocks[i].StartLine], this._nullContext);
                var roomId = this.GetRoomID(roomName, this._nullContext);
                for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                    if (this.BeginsWith(this._lines[j], "define ")) {
                        //skip nested blocks
                        var nestedBlock = 1;
                        do {
                            j = j + 1;
                            if (this.BeginsWith(this._lines[j], "define ")) {
                                nestedBlock = nestedBlock + 1;
                            } else if (Trim(this._lines[j]) == "end define") {
                                nestedBlock = nestedBlock - 1;
                            }
                        } while(!(nestedBlock == 0));
                    }
                    this._rooms[roomId].Exits.AddExitFromTag(this._lines[j]);
                }
            }
        }
        // UNKNOWN ExitSubStatement
    };
    LegacyGame.prototype.FindExit = function (tag) {
        // e.g. Takes a tag of the form "room; north" and return's the north exit of room.
        var params = Split(tag, ";");
        if (UBound(params) < 1) {
            this.LogASLError("No exit specified in '" + tag + "'", 2 /* WarningError */);
            return new RoomExit();
        }
        var room = Trim(params[0]);
        var exitName = Trim(params[1]);
        var roomId = this.GetRoomID(room, this._nullContext);
        if (roomId == 0) {
            this.LogASLError("Can't find room '" + room + "'", 2 /* WarningError */);
            return null;
        }
        var exits = this._rooms[roomId].Exits;
        var dir = exits.GetDirectionEnum(exitName);
        if (dir == -1 /* None */) {
            if (exits.Places.ContainsKey(exitName)) {
                return exits.Places.Item(exitName);
            }
        } else {
            return exits.GetDirectionExit(dir);
        }
        return null;
    };
    LegacyGame.prototype.ExecuteLock = function (tag, lock) {
        var roomExit;
        roomExit = this.FindExit(tag);
        if (roomExit == null) {
            this.LogASLError("Can't find exit '" + tag + "'", 2 /* WarningError */);
            // UNKNOWN ExitSubStatement
        }
        roomExit.IsLocked = lock;
    };
    LegacyGame.prototype.Begin = function () {
        var runnerThread = {};
        this.ChangeState(1 /* Working */);
        runnerThread.Start();
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.DoBegin = function () {
        var gameBlock = this.GetDefineBlock("game");
        var ctx = new Context();
        this.SetFont("");
        this.SetFontSize(0);
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
                } else if (this.BeginsWith(this._lines[i], "lib startscript ") && this._gameAslVersion < 311) {
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
        } else {
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
        this.ChangeState(0 /* Ready */);
    };

    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    LegacyGame.prototype.Finish = function () {
        this.GameFinished();
    };

    // UNKNOWN EventStatement
    // UNKNOWN EventStatement
    // UNKNOWN EventStatement
    LegacyGame.prototype.Save = function (filename, html) {
        this.SaveGame(filename);
    };
    LegacyGame.prototype.Save = function (html) {
        return this.SaveGame(Filename, false);
    };

    // UNKNOWN PropertyBlock
    LegacyGame.prototype.SendCommand = function (command) {
        this.SendCommand(command, 0, null);
    };
    LegacyGame.prototype.SendCommand = function (command, metadata) {
        this.SendCommand(command, 0, metadata);
    };
    LegacyGame.prototype.SendCommand = function (command, elapsedTime, metadata) {
        // The processing of commands is done in a separate thread, so things like the "enter" command can
        // lock the thread while waiting for further input. After starting to process the command, we wait
        // for something to happen before returning from the SendCommand call - either the command will have
        // finished processing, or perhaps a prompt has been printed and now the game is waiting for further
        // user input after hitting an "enter" script command.
        if (!this._readyForCommand) {
            // UNKNOWN ExitSubStatement
        }
        var runnerThread = {};
        this.ChangeState(1 /* Working */);
        runnerThread.Start(command);
        this.WaitForStateChange(1 /* Working */);
        if (elapsedTime > 0) {
            this.Tick(elapsedTime);
        } else {
            this.RaiseNextTimerTickRequest();
        }
    };
    LegacyGame.prototype.WaitForStateChange = function (changedFromState) {
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.ProcessCommandInNewThread = function (command) {
        // Process command, and change state to Ready if the command finished processing
        // UNKNOWN TryBlock
    };
    LegacyGame.prototype.SendEvent = function (eventName, param) {
    };

    // UNKNOWN EventStatement
    LegacyGame.prototype.Initialise = function (player) {
        this._player = player;
        if (LCase(Right(this._filename, 4)) == ".qsg" || this._data != null) {
            return this.OpenGame(this._filename);
        } else {
            return this.InitialiseGame(this._filename);
        }
    };
    LegacyGame.prototype.GameFinished = function () {
        this._gameFinished = true;

        // UNKNOWN RaiseEventStatement
        this.ChangeState(3 /* Finished */);

        // In case we're in the middle of processing an "enter" command, nudge the thread along
        // UNKNOWN SyncLockBlock
        // UNKNOWN SyncLockBlock
        // UNKNOWN SyncLockBlock
        this.Cleanup();
    };
    LegacyGame.prototype.GetResourcePath = function (filename) {
        if (!this._resourceFile == null && this._resourceFile.Length > 0) {
            var extractResult = this.ExtractFile(filename);
            return extractResult;
        }
        return System.IO.Path.Combine(this._gamePath, filename);
    };
    LegacyGame.prototype.Cleanup = function () {
        this.DeleteDirectory(this._tempFolder);
    };
    LegacyGame.prototype.DeleteDirectory = function (dir) {
        if (System.IO.Directory.Exists(dir)) {
            // UNKNOWN TryBlock
        }
    };
    LegacyGame.prototype.GetLibraryLines = function (libName) {
        var libCode = null;
        libName = LCase(libName);

        // UNKNOWN SelectBlock
        if (libCode == null) {
            return null;
        }
        return this.GetResourceLines(libCode);
    };

    // UNKNOWN PropertyBlock
    LegacyGame.prototype.Tick = function (elapsedTime) {
        var i;
        var timerScripts = {};
        Debug.Print("Tick: " + elapsedTime.ToString);
        for (var i = 1; i <= this._numberTimers; i++) {
            if (this._timers[i].TimerActive) {
                if (this._timers[i].BypassThisTurn) {
                    // don't trigger timer during the turn it was first enabled
                    this._timers[i].BypassThisTurn = false;
                } else {
                    this._timers[i].TimerTicks = this._timers[i].TimerTicks + elapsedTime;
                    if (this._timers[i].TimerTicks >= this._timers[i].TimerInterval) {
                        this._timers[i].TimerTicks = 0;
                        timerScripts.Add(this._timers[i].TimerAction);
                    }
                }
            }
        }
        if (timerScripts.Count > 0) {
            var runnerThread = {};
            this.ChangeState(1 /* Working */);
            runnerThread.Start(timerScripts);
            this.WaitForStateChange(1 /* Working */);
        }
        this.RaiseNextTimerTickRequest();
    };
    LegacyGame.prototype.RunTimersInNewThread = function (scripts) {
        var scriptList = scripts;
        scriptList.forEach(function (script) {
            // UNKNOWN TryBlock
        }, this);
        this.ChangeState(0 /* Ready */);
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
        Debug.Print("RaiseNextTimerTickRequest " + nextTrigger.ToString);
        // UNKNOWN RaiseEventStatement
    };
    LegacyGame.prototype.ChangeState = function (newState) {
        var acceptCommands = (newState == 0 /* Ready */);
        this.ChangeState(newState, acceptCommands);
    };
    LegacyGame.prototype.ChangeState = function (newState, acceptCommands) {
        this._readyForCommand = acceptCommands;
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.FinishWait = function () {
        if ((this._state != 2 /* Waiting */)) {
            // UNKNOWN ExitSubStatement
        }
        var runnerThread = {};
        this.ChangeState(1 /* Working */);
        runnerThread.Start();
        this.WaitForStateChange(1 /* Working */);
    };
    LegacyGame.prototype.FinishWaitInNewThread = function () {
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.FinishPause = function () {
        this.FinishWait();
    };

    LegacyGame.prototype.ShowMenu = function (menuData) {
        this._player.ShowMenu(menuData);
        this.ChangeState(2 /* Waiting */);

        // UNKNOWN SyncLockBlock
        return this.m_menuResponse;
    };
    LegacyGame.prototype.SetMenuResponse = function (response) {
        var runnerThread = {};
        this.ChangeState(1 /* Working */);
        runnerThread.Start(response);
        this.WaitForStateChange(1 /* Working */);
    };
    LegacyGame.prototype.SetMenuResponseInNewThread = function (response) {
        this.m_menuResponse = response;
        // UNKNOWN SyncLockBlock
    };
    LegacyGame.prototype.LogException = function (ex) {
        // UNKNOWN RaiseEventStatement
    };
    LegacyGame.prototype.GetExternalScripts = function () {
        return null;
    };
    LegacyGame.prototype.GetExternalStylesheets = function () {
        return null;
    };

    // UNKNOWN EventStatement
    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetOriginalFilenameForQSG = function () {
        if (this._originalFilename != null) {
            return this._originalFilename;
        }
        return this._gameFileName;
    };

    LegacyGame.prototype.SetUnzipFunction = function (unzipFunction) {
        this.m_unzipFunction = unzipFunction;
    };
    LegacyGame.prototype.GetUnzippedFile = function (filename) {
        var tempDir = null;
        var result = this.m_unzipFunction.Invoke(filename, tempDir);
        this._tempFolder = tempDir;
        return result;
    };

    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetResource = function (file) {
        if (file == "_game.cas") {
            return new any();
        }
        var path = this.GetResourcePath(file);
        if (!System.IO.File.Exists(path)) {
            return null;
        }
        return new any();
    };

    // UNKNOWN PropertyBlock
    LegacyGame.prototype.GetResources = function () {
        for (var i = 1; i <= this._numResources; i++) {
            // UNKNOWN YieldStatement
        }
        if (this._numResources > 0) {
            // UNKNOWN YieldStatement
        }
    };
    LegacyGame.prototype.GetResourcelessCAS = function () {
        var fileData = System.IO.File.ReadAllText(this._resourceFile, System.Text.Encoding.GetEncoding(1252));
        return System.Text.Encoding.GetEncoding(1252).GetBytes(Left(fileData, this._startCatPos - 1));
    };
    LegacyGame.prototype.GetFileData = function (filename) {
        // TODO
        return "";
    };
    return LegacyGame;
})();
var ChangeLog = (function () {
    function ChangeLog() {
        this._changes = {};
    }
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // appliesTo = room or object name
    // element = the thing that's changed, e.g. an action or property name
    // changeData = the actual change info
    ChangeLog.prototype.AddItem = function (appliesTo, element, changeData) {
        // the first four characters of the changeData will be "prop" or "acti", so we add this to the
        // key so that actions and properties don't collide.
        var key = appliesTo + "#" + Left(changeData, 4) + "~" + element;
        if (this._changes.ContainsKey(key)) {
            this._changes.Remove(key);
        }
        this._changes.Add(key, changeData);
    };
    return ChangeLog;
})();
var Config = (function () {
    function Config() {
    }
    return Config;
})();
var RoomExit = (function () {
    function RoomExit(game) {
        this._game = game;
        game._numberObjs = game._numberObjs + 1;
        if (!game._objs)
            game._objs = [];
        game._objs[game._numberObjs] = new ObjectType();
        this._objId = game._numberObjs;
        // UNKNOWN WithBlock
    }
    // If this code was properly object oriented, we could set up properties properly
    // on the "object" object.
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    RoomExit.prototype.RunAction = function (actionName, ctx) {
        this._game.DoAction(this._objId, actionName, ctx);
    };
    RoomExit.prototype.RunScript = function (ctx) {
        this.RunAction("script", ctx);
    };
    RoomExit.prototype.UpdateObjectName = function () {
        var objName;
        var lastExitId;
        var parentRoom;
        if (Len(this._objName) > 0) {
            // UNKNOWN ExitSubStatement
        }
        if (this._parent == null) {
            // UNKNOWN ExitSubStatement
        }
        parentRoom = this._game._objs[this._parent.ObjId].ObjectName;
        objName = parentRoom;
        if (this._direction != LegacyGame.Direction.None) {
            objName = objName + "." + this._parent.GetDirectionName(this._direction);
            this._game._objs[this._objId].ObjectAlias = this._parent.GetDirectionName(this._direction);
        } else {
            var lastExitIdString = this._game.GetObjectProperty("quest.lastexitid", (this._parent.ObjId), null, false);
            if (lastExitIdString.Length == 0) {
                lastExitId = 0;
            } else {
                lastExitId = parseInt(lastExitId);
            }
            lastExitId = lastExitId + 1;
            this._game.AddToObjectProperties("quest.lastexitid=" + (lastExitId).toString(), (this._parent.ObjId), this._game._nullContext);
            objName = objName + ".exit" + (lastExitId).toString();
            if (RoomId == 0) {
                // the room we're pointing at might not exist, especially if this is a script exit
                this._displayName = ToRoom;
            } else {
                if (Len(this._game._rooms[RoomId].RoomAlias) > 0) {
                    this._displayName = this._game._rooms[RoomId].RoomAlias;
                } else {
                    this._displayName = ToRoom;
                }
            }
            this._game._objs[this._objId].ObjectAlias = this._displayName;
            Prefix = this._game._rooms[RoomId].Prefix;
        }
        this._game._objs[this._objId].ObjectName = objName;
        this._game._objs[this._objId].ContainerRoom = parentRoom;
        this._objName = objName;
    };
    RoomExit.prototype.Go = function (ctx) {
        if (IsLocked) {
            if (ExitPropertyBool("lockmessage")) {
                this._game.Print(ExitProperty("lockmessage"), ctx);
            } else {
                this._game.PlayerErrorMessage(35 /* Locked */, ctx);
            }
        } else {
            if (IsScript) {
                this.RunScript(ctx);
            } else {
                this._game.PlayGame(ToRoom, ctx);
            }
        }
    };
    return RoomExit;
})();
var RoomExits = (function () {
    function RoomExits(game) {
        this._directions = {};
        this._places = {};
        this._game = game;
        this._regenerateAllExits = true;
    }
    RoomExits.prototype.SetDirection = function (direction, roomExit) {
        if (this._directions.ContainsKey(direction)) {
            roomExit = this._directions.Item(direction);
            this._game._objs[roomExit.ObjId].Exists = true;
        } else {
            roomExit = new RoomExit();
            this._directions.Add(direction, roomExit);
        }
        this._regenerateAllExits = true;
    };
    RoomExits.prototype.GetDirectionExit = function (direction) {
        if (this._directions.ContainsKey(direction)) {
            return this._directions.Item(direction);
        }
        return null;
    };
    RoomExits.prototype.AddPlaceExit = function (roomExit) {
        if (this._places.ContainsKey(roomExit.ToRoom)) {
            var removeItem = this._places.Item(roomExit.ToRoom);
            this.RemoveExit(removeItem);
        }
        this._places.Add(roomExit.ToRoom, roomExit);
        this._regenerateAllExits = true;
    };
    RoomExits.prototype.AddExitFromTag = function (tag) {
        var thisDir;
        var roomExit = null;
        var params = [];
        var afterParam;
        var param;
        if (this._game.BeginsWith(tag, "out ")) {
            tag = this._game.GetEverythingAfter(tag, "out ");
            thisDir = 0 /* Out */;
        } else if (this._game.BeginsWith(tag, "east ")) {
            tag = this._game.GetEverythingAfter(tag, "east ");
            thisDir = 3 /* East */;
        } else if (this._game.BeginsWith(tag, "west ")) {
            tag = this._game.GetEverythingAfter(tag, "west ");
            thisDir = 4 /* West */;
        } else if (this._game.BeginsWith(tag, "north ")) {
            tag = this._game.GetEverythingAfter(tag, "north ");
            thisDir = 1 /* North */;
        } else if (this._game.BeginsWith(tag, "south ")) {
            tag = this._game.GetEverythingAfter(tag, "south ");
            thisDir = 2 /* South */;
        } else if (this._game.BeginsWith(tag, "northeast ")) {
            tag = this._game.GetEverythingAfter(tag, "northeast ");
            thisDir = 6 /* NorthEast */;
        } else if (this._game.BeginsWith(tag, "northwest ")) {
            tag = this._game.GetEverythingAfter(tag, "northwest ");
            thisDir = 5 /* NorthWest */;
        } else if (this._game.BeginsWith(tag, "southeast ")) {
            tag = this._game.GetEverythingAfter(tag, "southeast ");
            thisDir = 8 /* SouthEast */;
        } else if (this._game.BeginsWith(tag, "southwest ")) {
            tag = this._game.GetEverythingAfter(tag, "southwest ");
            thisDir = 7 /* SouthWest */;
        } else if (this._game.BeginsWith(tag, "up ")) {
            tag = this._game.GetEverythingAfter(tag, "up ");
            thisDir = 9 /* Up */;
        } else if (this._game.BeginsWith(tag, "down ")) {
            tag = this._game.GetEverythingAfter(tag, "down ");
            thisDir = 10 /* Down */;
        } else if (this._game.BeginsWith(tag, "place ")) {
            tag = this._game.GetEverythingAfter(tag, "place ");
            thisDir = -1 /* None */;
        } else {
            // UNKNOWN ExitSubStatement
        }
        if (thisDir != -1 /* None */) {
            // This will reuse an existing Exit object if we're resetting
            // the destination of an existing directional exit.
            this.SetDirection(thisDir, roomExit);
        } else {
            roomExit = new RoomExit();
        }
        roomExit.Parent = this;
        roomExit.Direction = thisDir;
        if (this._game.BeginsWith(tag, "locked ")) {
            roomExit.IsLocked = true;
            tag = this._game.GetEverythingAfter(tag, "locked ");
        }
        if (Left(Trim(tag), 1) == "<") {
            params = Split(this._game.GetParameter(tag, this._game._nullContext), ";");
            afterParam = Trim(Mid(tag, InStr(tag, ">") + 1));
            param = true;
        } else {
            afterParam = tag;
        }
        if (Len(afterParam) > 0) {
            // Script exit
            roomExit.Script = afterParam;
            if (thisDir == -1 /* None */) {
                // A place exit with a script still has a ToRoom
                roomExit.ToRoom = params[0];

                // and may have a lock message
                if (UBound(params) > 0) {
                    roomExit.LockMessage = params[1];
                }
            } else {
                // A directional exit with a script may have no parameter.
                // If it does have a parameter it will be a lock message.
                if (param) {
                    roomExit.LockMessage = params[0];
                }
            }
        } else {
            roomExit.ToRoom = params[0];
            if (UBound(params) > 0) {
                roomExit.LockMessage = params[1];
            }
        }
        if (thisDir == -1 /* None */) {
            this.AddPlaceExit(roomExit);
        }
    };
    RoomExits.prototype.AddExitFromCreateScript = function (script, ctx) {
        // sScript is the "create exit ..." script, but without the "create exit" at the beginning.
        // So it's very similar to creating an exit from a tag, except we have the source room
        // name before the semicolon (which we don't even care about as we ARE the source room).
        var param;
        var params;
        var paramStart;
        var paramEnd;

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
        paramEnd = InStr(paramStart, script, ">");
        if (paramStart > 1) {
            // Directional exit
            if (UBound(params) == 0) {
                // Script directional exit
                this.AddExitFromTag(Trim(Left(script, paramStart - 1)) + " " + Trim(Mid(script, paramEnd + 1)));
            } else {
                // "Normal" directional exit
                this.AddExitFromTag(Trim(Left(script, paramStart - 1)) + " <" + Trim(params[1]) + ">");
            }
        } else {
            if (UBound(params) < 1) {
                this._game.LogASLError("No exit destination given in 'create exit " + script + "'", 2 /* WarningError */);
                // UNKNOWN ExitSubStatement
            }

            // Place exit so add "place" tag at the beginning
            this.AddExitFromTag("place <" + Trim(params[1]) + Mid(script, paramEnd));
        }
    };

    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    RoomExits.prototype.ExecuteGo = function (cmd, ctx) {
        // This will handle "n", "go east", "go [to] library" etc.
        var lExitID;
        var oExit;
        if (this._game.BeginsWith(cmd, "go to ")) {
            cmd = this._game.GetEverythingAfter(cmd, "go to ");
        } else if (this._game.BeginsWith(cmd, "go ")) {
            cmd = this._game.GetEverythingAfter(cmd, "go ");
        }
        lExitID = this._game.Disambiguate(cmd, this._game._currentRoom, ctx, true);
        if (lExitID == -1) {
            this._game.PlayerErrorMessage(15 /* BadPlace */, ctx);
        } else {
            oExit = this.GetExitByObjectId(lExitID);
            oExit.Go(ctx);
        }
    };
    RoomExits.prototype.GetAvailableDirectionsDescription = function (description, list) {
        var roomExit;
        var count;
        var descPrefix;
        var orString;
        descPrefix = "You can go";
        orString = "or";
        list = "";
        count = 0;
        this.AllExits().forEach(function (kvp) {
            count = count + 1;
            roomExit = kvp.Value;
            list = list + this.GetDirectionToken((roomExit.Direction));
            description = description + this.GetDirectionNameDisplay(roomExit);
            if (count < this.AllExits.Count - 1) {
                description = description + ", ";
            } else if (count == this.AllExits.Count - 1) {
                description = description + " " + orString + " ";
            }
        }, this);
        this._game.SetStringContents("quest.doorways", description, this._game._nullContext);
        if (count > 0) {
            description = descPrefix + " " + description + ".";
        }
    };
    RoomExits.prototype.GetDirectionName = function (dir) {
        // UNKNOWN SelectBlock
        return null;
    };
    RoomExits.prototype.GetDirectionEnum = function (dir) {
        // UNKNOWN SelectBlock
        return -1 /* None */;
    };
    RoomExits.prototype.GetDirectionToken = function (dir) {
        // UNKNOWN SelectBlock
        return null;
    };
    RoomExits.prototype.GetDirectionNameDisplay = function (roomExit) {
        if (roomExit.Direction != -1 /* None */) {
            var dir = this.GetDirectionName((roomExit.Direction));
            return "|b" + dir + "|xb";
        }
        var sDisplay = "|b" + roomExit.DisplayName + "|xb";
        if (Len(roomExit.Prefix) > 0) {
            sDisplay = roomExit.Prefix + " " + sDisplay;
        }
        return "to " + sDisplay;
    };
    RoomExits.prototype.GetExitByObjectId = function (id) {
        this.AllExits().forEach(function (kvp) {
            if (kvp.Value.ObjId == id) {
                return kvp.Value;
            }
        }, this);
        return null;
    };
    RoomExits.prototype.AllExits = function () {
        if (!this._regenerateAllExits) {
            this.AllExits = this._allExits;
            // UNKNOWN ExitFunctionStatement
        }
        this._allExits = new any();
        this._directions.Keys.forEach(function (dir) {
            var roomExit = this._directions.Item(dir);
            if (this._game._objs[roomExit.ObjId].Exists) {
                this._allExits.Add(dir, this._directions.Item(dir));
            }
        }, this);
        this._places.Keys.forEach(function (dir) {
            var roomExit = this._places.Item(dir);
            if (this._game._objs[roomExit.ObjId].Exists) {
                this._allExits.Add(dir, this._places.Item(dir));
            }
        }, this);
        return this._allExits;
    };
    RoomExits.prototype.RemoveExit = function (roomExit) {
        // Don't remove directional exits, as if they're recreated
        // a new object will be created which will have the same name
        // as the old one. This is because we can't delete objects yet...
        if (roomExit.Direction == -1 /* None */) {
            if (this._places.ContainsKey(roomExit.ToRoom)) {
                this._places.Remove(roomExit.ToRoom);
            }
        }
        this._game._objs[roomExit.ObjId].Exists = false;
        this._regenerateAllExits = true;
    };
    return RoomExits;
})();
var TextFormatter = (function () {
    function TextFormatter() {
        this.colour = "";
        this.fontSize = 0;
        this.align = "";
    }
    TextFormatter.prototype.OutputHTML = function (input) {
        var output = "";
        var position = 0;
        var codePosition;
        var finished = false;
        var nobr = false;
        input = input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace(vbCrLf, "<br />");
        if (Right(input, 3) == "|xn") {
            nobr = true;
            input = Left(input, Len(input) - 3);
        }
        do {
            codePosition = input.IndexOf("|", position);
            if (codePosition == -1) {
                // UNKNOWN AddAssignmentStatement
                finished = true;
                // can also have size codes
            } else {
                // UNKNOWN AddAssignmentStatement
                position = codePosition + 1;
                var oneCharCode = "";
                var twoCharCode = "";
                if (position < input.Length) {
                    oneCharCode = input.Substring(position, 1);
                }
                if (position < (input.Length - 1)) {
                    twoCharCode = input.Substring(position, 2);
                }
                var foundCode = true;

                // UNKNOWN SelectBlock
                if (foundCode) {
                    // UNKNOWN AddAssignmentStatement
                } else {
                    foundCode = true;

                    // UNKNOWN SelectBlock
                    if (foundCode) {
                        // UNKNOWN AddAssignmentStatement
                    }
                }
                if (!foundCode) {
                    if (oneCharCode == "s") {
                        // |s00 |s10 etc.
                        if (position < (input.Length - 2)) {
                            var sizeCode = input.Substring(position + 1, 2);
                            if (Integer.TryParse(sizeCode, this.fontSize)) {
                                foundCode = true;
                                // UNKNOWN AddAssignmentStatement
                            }
                        }
                    }
                }
                if (!foundCode) {
                    // UNKNOWN AddAssignmentStatement
                }
            }
        } while(!(finished || position >= input.Length));
        return String.Format("<output{0}>{1}</output>", nobr ? " nobr=\"true\"" : "", output);
    };
    TextFormatter.prototype.FormatText = function (input) {
        if (input.Length == 0) {
            return input;
        }
        var output = "";
        if (this.align.Length > 0) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.fontSize > 0) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.colour.Length > 0) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.bold) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.italic) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.underline) {
            // UNKNOWN AddAssignmentStatement
        }

        // UNKNOWN AddAssignmentStatement
        if (this.underline) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.italic) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.bold) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.colour.Length > 0) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.fontSize > 0) {
            // UNKNOWN AddAssignmentStatement
        }
        if (this.align.Length > 0) {
            // UNKNOWN AddAssignmentStatement
        }
        return output;
    };
    return TextFormatter;
})();
