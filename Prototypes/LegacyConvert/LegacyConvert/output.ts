function Left(input: string, length: number): string {
    return input.substring(0, length);
}

function Right(input: string, length: number): string {
    return input.substring(input.length - length - 1);
}

function Mid(input: string, start: number, length?: number): string {
    if (typeof length === 'undefined') {
        return input.substr(start - 1);
    }
    return input.substr(start - 1, length);
}

function UCase(input: string): string {
    return input.toUpperCase();
}

function LCase(input: string): string {
    return input.toLowerCase();
}

function InStr(arg1, arg2, arg3?): number {
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

function Split(input: string, splitChar: string): string[] {
    return input.split(splitChar);
}

function Join(input: string[], joinChar: string): string {
    return input.join(joinChar);
}

function IsNumeric(input): boolean {
    return !isNaN(parseFloat(input)) && isFinite(input);
}

function Replace(input: string, oldString: string, newString: string): string {
    return input.split(oldString).join(newString);
}

function Trim(input: string): string {
    return input.trim();
}

function LTrim(input: string): string {
    return input.replace(/^\s+/,"");
}

function Asc(input: string): number {
    return input.charCodeAt(0);
}

function Chr(input: number): string {
    return String.fromCharCode(input);
}

function Len(input: string): number {
    return input.length;
}

function UBound(array: any[]): number {
    return array.length - 1;
}enum State {Ready, Working, Waiting, Finished};
class DefineBlock {
    StartLine: number;
    EndLine: number;
}
class Context {
    CallingObjectId: number;
    NumParameters: number;
    Parameters: string[];
    FunctionReturnValue: string;
    AllowRealNamesInCommand: boolean;
    DontProcessCommand: boolean;
    CancelExec: boolean;
    StackCounter: number;
}
enum LogType {Misc, FatalError, WarningError, Init, LibraryWarningError, Warning, UserError, InternalError};
enum Direction {None = -1, Out = 0, North = 1, South = 2, East = 3, West = 4, NorthWest = 5, NorthEast = 6, SouthWest = 7, SouthEast = 8, Up = 9, Down = 10};
class ItemType {
    Name: string;
    Got: boolean;
}
class Collectable {
    Name: string;
    Type: string;
    Value: number;
    Display: string;
    DisplayWhenZero: boolean;
}
class PropertyType {
    PropertyName: string;
    PropertyValue: string;
}
class ActionType {
    ActionName: string;
    Script: string;
}
class UseDataType {
    UseObject: string;
    UseType: UseType;
    UseScript: string;
}
class GiveDataType {
    GiveObject: string;
    GiveType: GiveType;
    GiveScript: string;
}
class PropertiesActions {
    Properties: string;
    NumberActions: number;
    Actions: ActionType[];
    NumberTypesIncluded: number;
    TypesIncluded: string[];
}
class VariableType {
    VariableName: string;
    VariableContents: string[];
    VariableUBound: number;
    DisplayString: string;
    OnChangeScript: string;
    NoZeroDisplay: boolean;
}
class SynonymType {
    OriginalWord: string;
    ConvertTo: string;
}
class TimerType {
    TimerName: string;
    TimerInterval: number;
    TimerActive: boolean;
    TimerAction: string;
    TimerTicks: number;
    BypassThisTurn: boolean;
}
class UserDefinedCommandType {
    CommandText: string;
    CommandScript: string;
}
class TextAction {
    Data: string;
    Type: TextActionType;
}
enum TextActionType {Text, Script, Nothing, Default};
class ScriptText {
    Text: string;
    Script: string;
}
class PlaceType {
    PlaceName: string;
    Prefix: string;
    PlaceAlias: string;
    Script: string;
}
class RoomType {
    RoomName: string;
    RoomAlias: string;
    Commands: UserDefinedCommandType[];
    NumberCommands: number;
    Description: TextAction = new TextAction();
    Out: ScriptText = new ScriptText();
    East: TextAction = new TextAction();
    West: TextAction = new TextAction();
    North: TextAction = new TextAction();
    South: TextAction = new TextAction();
    NorthEast: TextAction = new TextAction();
    NorthWest: TextAction = new TextAction();
    SouthEast: TextAction = new TextAction();
    SouthWest: TextAction = new TextAction();
    Up: TextAction = new TextAction();
    Down: TextAction = new TextAction();
    InDescription: string;
    Look: string;
    Places: PlaceType[];
    NumberPlaces: number;
    Prefix: string;
    Script: string;
    Use: ScriptText[];
    NumberUse: number;
    ObjId: number;
    BeforeTurnScript: string;
    AfterTurnScript: string;
    Exits: RoomExits;
}
class ObjectType {
    ObjectName: string;
    ObjectAlias: string;
    Detail: string;
    ContainerRoom: string;
    Exists: boolean;
    IsGlobal: boolean;
    Prefix: string;
    Suffix: string;
    Gender: string;
    Article: string;
    DefinitionSectionStart: number;
    DefinitionSectionEnd: number;
    Visible: boolean;
    GainScript: string;
    LoseScript: string;
    NumberProperties: number;
    Properties: PropertyType[];
    Speak: TextAction = new TextAction();
    Take: TextAction = new TextAction();
    IsRoom: boolean;
    IsExit: boolean;
    CorresRoom: string;
    CorresRoomId: number;
    Loaded: boolean;
    NumberActions: number;
    Actions: ActionType[];
    NumberUseData: number;
    UseData: UseDataType[];
    UseAnything: string;
    UseOnAnything: string;
    Use: string;
    NumberGiveData: number;
    GiveData: GiveDataType[];
    GiveAnything: string;
    GiveToAnything: string;
    DisplayType: string;
    NumberTypesIncluded: number;
    TypesIncluded: string[];
    NumberAltNames: number;
    AltNames: string[];
    AddScript: TextAction = new TextAction();
    RemoveScript: TextAction = new TextAction();
    OpenScript: TextAction = new TextAction();
    CloseScript: TextAction = new TextAction();
}
class ChangeType {
    AppliesTo: string;
    Change: string;
}
class GameChangeDataType {
    NumberChanges: number;
    ChangeData: ChangeType[];
}
class ResourceType {
    ResourceName: string;
    ResourceStart: number;
    ResourceLength: number;
    Extracted: boolean;
}
class ExpressionResult {
    Result: string;
    Success: ExpressionSuccess;
    Message: string;
}
enum PlayerError {BadCommand, BadGo, BadGive, BadCharacter, NoItem, ItemUnwanted, BadLook, BadThing, DefaultLook, DefaultSpeak, BadItem, DefaultTake, BadUse, DefaultUse, DefaultOut, BadPlace, BadExamine, DefaultExamine, BadTake, CantDrop, DefaultDrop, BadDrop, BadPronoun, AlreadyOpen, AlreadyClosed, CantOpen, CantClose, DefaultOpen, DefaultClose, BadPut, CantPut, DefaultPut, CantRemove, AlreadyPut, DefaultRemove, Locked, DefaultWait, AlreadyTaken};
enum ItType {Inanimate, Male, Female};
enum SetResult {Error, Found, Unfound};
enum Thing {Character, Object, Room};
enum ConvertType {Strings, Functions, Numeric, Collectables};
enum UseType {UseOnSomething, UseSomethingOn};
enum GiveType {GiveToSomething, GiveSomethingTo};
enum VarType {String, Numeric};
enum StopType {Win, Lose, Null};
enum ExpressionSuccess {OK, Fail};
class InitGameData {
    Data: number[];
    SourceFile: string;
}
class ArrayResult {
    Name: string;
    Index: number;
}
class PlayerCanAccessObjectResult {
    CanAccessObject: boolean;
    ErrorMsg: string;
}
enum AppliesTo {Object, Room};
class LegacyGame {
    CopyContext(ctx: Context): Context {
        var result: Context = new Context();
        result.CallingObjectId = ctx.CallingObjectId;
        result.NumParameters = ctx.NumParameters;
        result.Parameters = ctx.Parameters;
        result.FunctionReturnValue = ctx.FunctionReturnValue;
        result.AllowRealNamesInCommand = ctx.AllowRealNamesInCommand;
        result.DontProcessCommand = ctx.DontProcessCommand;
        result.CancelExec = ctx.CancelExec;
        result.StackCounter = ctx.StackCounter;
        return result;
    }
    _defineBlockParams: any;
    _openErrorReport: string;
    _casKeywords: string[] = [];
    _lines: string[];
    _defineBlocks: DefineBlock[];
    _numberSections: number;
    _gameName: string;
    _nullContext: Context = new Context();
    _changeLogRooms: ChangeLog;
    _changeLogObjects: ChangeLog;
    _defaultProperties: PropertiesActions;
    _defaultRoomProperties: PropertiesActions;
    _rooms: RoomType[];
    _numberRooms: number;
    _numericVariable: VariableType[];
    _numberNumericVariables: number;
    _stringVariable: VariableType[];
    _numberStringVariables: number;
    _synonyms: SynonymType[];
    _numberSynonyms: number;
    _items: ItemType[];
    _chars: ObjectType[];
    _objs: ObjectType[];
    _numberChars: number;
    _numberObjs: number;
    _numberItems: number;
    _currentRoom: string;
    _collectables: Collectable[];
    _numCollectables: number;
    _gamePath: string;
    _gameFileName: string;
    _saveGameFile: string;
    _defaultFontName: string;
    _defaultFontSize: number;
    _autoIntro: boolean;
    _commandOverrideModeOn: boolean;
    _commandOverrideVariable: string;
    _afterTurnScript: string;
    _beforeTurnScript: string;
    _outPutOn: boolean;
    _gameAslVersion: number;
    _choiceNumber: number;
    _gameLoadMethod: string;
    _timers: TimerType[];
    _numberTimers: number;
    _numDisplayStrings: number;
    _numDisplayNumerics: number;
    _gameFullyLoaded: boolean;
    _gameChangeData: GameChangeDataType = new GameChangeDataType();
    _lastIt: number;
    _lastItMode: ItType;
    _thisTurnIt: number;
    _thisTurnItMode: ItType;
    _badCmdBefore: string;
    _badCmdAfter: string;
    _numResources: number;
    _resources: ResourceType[];
    _resourceFile: string;
    _resourceOffset: number;
    _startCatPos: number;
    _useAbbreviations: boolean;
    _loadedFromQsg: boolean;
    _beforeSaveScript: string;
    _onLoadScript: string;
    _numSkipCheckFiles: number;
    _skipCheckFile: string[];
    _compassExits: any = {};
    _gotoExits: any = {};
    _textFormatter: TextFormatter = new TextFormatter();
    _log: any = {};
    _casFileData: string;
    _commandLock: Object = new Object();
    _stateLock: Object = new Object();
    _state: State = State.Ready;
    _waitLock: Object = new Object();
    _readyForCommand: boolean = true;
    _gameLoading: boolean;
    _random: Random = new Random();
    _tempFolder: string;
    _playerErrorMessageString: string;
    _listVerbs: any = {};
    _filename: string;
    _originalFilename: string;
    _data: InitGameData;
    _player: IPlayer;
    _gameFinished: boolean;
    _gameIsRestoring: boolean;
    _useStaticFrameForPictures: boolean;
    _fileData: string;
    _fileDataPos: number;
    _questionResponse: boolean;
    constructor(filename: string, originalFilename: string) {
        this._tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath, "Quest", Guid.NewGuid().ToString());
        this.LoadCASKeywords();
        this._gameLoadMethod = "normal";
        this._filename = filename;
        this._originalFilename = originalFilename;
        this._numSkipCheckFiles = 3;
        this._skipCheckFile = [];
        this._skipCheckFile[1] = "bargain.cas";
        this._skipCheckFile[2] = "easymoney.asl";
        this._skipCheckFile[3] = "musicvf1.cas";
    }
    constructor(data: InitGameData) {
        this.New(null, null);
        this._data = data;
    }
    RemoveFormatting(s: string): string {
        var code: string;
        var pos: number;
        var len: number;
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
                    len = 1;
                }
                s = Left(s, pos - 1) + Mid(s, pos + len + 1);
            }
        } while (!(pos == 0));
        return s;
    }
    CheckSections(): boolean {
        var defines: number;
        var braces: number;
        var checkLine: string = "";
        var bracePos: number;
        var pos: number;
        var section: string = "";
        var hasErrors: boolean;
        var skipBlock: boolean;
        this._openErrorReport = "";
        hasErrors = false;
        defines = 0;
        braces = 0;
        // UNKNOWN ForBlock
        if (defines > 0) {
            this.LogASLError("Missing 'end define'", LogType.FatalError);
            this._openErrorReport = this._openErrorReport + "Missing 'end define'." + vbCrLf;
            hasErrors = true;
        }
        return !hasErrors;
    }
    ConvertFriendlyIfs(): boolean {
        var convPos: number;
        var symbPos: number;
        var symbol: string;
        var endParamPos: number;
        var paramData: string;
        var startParamPos: number;
        var firstData: string;
        var secondData: string;
        var obscureLine: string;
        var newParam: string;
        var varObscureLine: string;
        var bracketCount: number;
        // UNKNOWN ForBlock
        return false;
    }
    ConvertMultiLineSections(): void {
        var startLine: number;
        var braceCount: number;
        var thisLine: string;
        var lineToAdd: string;
        var lastBrace: number;
        var i: number;
        var restOfLine: string;
        var procName: string;
        var endLineNum: number;
        var afterLastBrace: string;
        var z: string;
        var startOfOrig: string;
        var testLine: string;
        var testBraceCount: number;
        var obp: number;
        var cbp: number;
        var curProc: number;
        i = 1;
        do {
            z = this._lines[this._defineBlocks[i].StartLine];
            if (((!this.BeginsWith(z, "define text ")) && (!this.BeginsWith(z, "define menu ")) && z != "define synonyms")) {
                // UNKNOWN ForBlock
            }
            i = i + 1;
        } while (!(i > this._numberSections));
        // UNKNOWN ForBlock
    }
    ErrorCheck(): boolean {
        var curBegin: number;
        var curEnd: number;
        var hasErrors: boolean;
        var curPos: number;
        var numParamStart: number;
        var numParamEnd: number;
        var finLoop: boolean;
        var inText: boolean;
        hasErrors = false;
        inText = false;
        // UNKNOWN ForBlock
        if (hasErrors == true) {
            return true;
        }
        // UNKNOWN ForBlock
        return hasErrors;
    }
    GetAfterParameter(s: string): string {
        var eop: number;
        eop = InStr(s, ">");
        if (eop == 0 || eop + 1 > Len(s)) {
            return "";
        } else {
            return Trim(Mid(s, eop + 1));
        }
    }
    ObliterateParameters(s: string): string {
        var inParameter: boolean;
        var exitCharacter: string = "";
        var curChar: string;
        var outputLine: string = "";
        var obscuringFunctionName: boolean;
        inParameter = false;
        // UNKNOWN ForBlock
        if (inParameter) {
            return "'<ERROR;" + exitCharacter + ";" + outputLine;
        } else {
            return outputLine;
        }
    }
    ObliterateVariableNames(s: string): string {
        var inParameter: boolean;
        var exitCharacter: string = "";
        var outputLine: string = "";
        var curChar: string;
        inParameter = false;
        // UNKNOWN ForBlock
        if (inParameter) {
            outputLine = "'<ERROR;" + exitCharacter + ";" + outputLine;
        }
        return outputLine;
    }
    RemoveComments(): void {
        var aposPos: number;
        var inTextBlock: boolean;
        var inSynonymsBlock: boolean;
        var oblitLine: string;
        // UNKNOWN ForBlock
    }
    ReportErrorLine(s: string): string {
        var replaceFrom: number;
        replaceFrom = InStr(s, "do <!intproc");
        if (replaceFrom != 0) {
            return Left(s, replaceFrom - 1) + "...";
        } else {
            return s;
        }
    }
    YesNo(yn: boolean): string {
        // UNKNOWN SingleLineIfStatement
    }
    IsYes(yn: string): boolean {
        // UNKNOWN SingleLineIfStatement
    }
    BeginsWith(s: string, text: string): boolean {
        return Left(LTrim(LCase(s)), Len(text)) == LCase(text);
    }
    ConvertCasKeyword(casChar: string): string {
        var c: number = System.Text.Encoding.GetEncoding(1252).GetBytes(casChar)(0);
        var keyword: string = this._casKeywords[c];
        if (keyword == "!cr") {
            keyword = vbCrLf;
        }
        return keyword;
    }
    ConvertMultiLines(): void {
        // UNKNOWN ForBlock
        this.RemoveComments();
    }
    GetDefineBlock(blockname: string): DefineBlock {
        var l: string;
        var blockType: string;
        var result = new DefineBlock();
        result.StartLine = 0;
        result.EndLine = 0;
        // UNKNOWN ForBlock
        return result;
    }
    DefineBlockParam(blockname: string, param: string): DefineBlock {
        var cache: any;
        var result = new DefineBlock();
        param = "k" + param;
        if (!this._defineBlockParams.ContainsKey(blockname)) {
            cache = new any();
            this._defineBlockParams.Add(blockname, cache);
            // UNKNOWN ForBlock
        } else {
            cache = this._defineBlockParams.Item(blockname);
        }
        if (cache.ContainsKey(param)) {
            var blocks = Split(cache.Item(param), ",");
            result.StartLine = parseInt(blocks[0]);
            result.EndLine = parseInt(blocks[1]);
        }
        return result;
    }
    GetEverythingAfter(s: string, text: string): string {
        if (Len(text) > Len(s)) {
            return "";
        }
        return Right(s, Len(s) - Len(text));
    }
    Keyword2CAS(KWord: string): string {
        var k = "";
        if (KWord == "") {
            return "";
        }
        // UNKNOWN ForBlock
        return this.Keyword2CAS("!unknown") + KWord + this.Keyword2CAS("!unknown");
    }
    LoadCASKeywords(): void {
        var questDatLines: string[] = this.GetResourceLines(My.Resources.QuestDAT);
        // UNKNOWN ForEachBlock
    }
    GetResourceLines(res: number[]): string[] {
        var enc: any = {};
        var resFile: string = enc.GetString(res);
        return Split(resFile, Chr(13) + Chr(10));
    }
    ParseFile(filename: string): boolean {
        var hasErrors: boolean;
        var result: boolean;
        var libCode: string[] = [];
        var libLines: number;
        var ignoreMode: boolean;
        var skipCheck: boolean;
        var c: number;
        var d: number;
        var l: number;
        var libFileHandle: number;
        var libResourceLines: string[];
        var libFile: string;
        var libLine: string;
        var inDefGameBlock: number;
        var gameLine: number;
        var inDefSynBlock: number;
        var synLine: number;
        var libFoundThisSweep: boolean;
        var libFileName: string;
        var libraryList: string[] = [];
        var numLibraries: number;
        var libraryAlreadyIncluded: boolean;
        var inDefTypeBlock: number;
        var typeBlockName: string;
        var typeLine: number;
        var defineCount: number;
        var curLine: number;
        this._defineBlockParams = new any();
        result = true;
        if (LCase(Right(filename, 4)) == ".zip") {
            this._originalFilename = filename;
            filename = this.GetUnzippedFile(filename);
            this._gamePath = System.IO.Path.GetDirectoryName(filename);
        }
        if (LCase(Right(filename, 4)) == ".asl" || LCase(Right(filename, 4)) == ".txt") {
            var fileData: string;
            if (Config.ReadGameFileFromAzureBlob) {
                // UNKNOWN UsingBlock
            } else {
                fileData = System.IO.File.ReadAllText(filename);
            }
            var aslLines: string[] = fileData.Split(Chr(13));
            this._lines = [];
            this._lines[0] = "";
            // UNKNOWN ForBlock
            l = aslLines.Length;
        } else if (LCase(Right(filename, 4)) == ".cas") {
            this.LogASLError("Loading CAS");
            this.LoadCASFile(filename);
            l = UBound(this._lines);
        } else {
            // UNKNOWN ThrowStatement
        }
        numLibraries = 0;
        do {
            libFoundThisSweep = false;
            // UNKNOWN ForBlock
        } while (!(libFoundThisSweep == false));
        skipCheck = false;
        var lastSlashPos: number;
        var slashPos: number;
        var curPos = 1;
        do {
            slashPos = InStr(curPos, filename, "\\");
            // UNKNOWN SingleLineIfStatement
            curPos = slashPos + 1;
        } while (!(slashPos == 0));
        var filenameNoPath = LCase(Mid(filename, lastSlashPos + 1));
        // UNKNOWN ForBlock
        if (filenameNoPath == "musicvf1.cas") {
            this._useStaticFrameForPictures = true;
        }
        this.ConvertMultiLines();
        if (!skipCheck) {
            if (!this.CheckSections()) {
                return false;
            }
        }
        this._numberSections = 1;
        // UNKNOWN ForBlock
        this._numberSections = this._numberSections - 1;
        var gotGameBlock = false;
        // UNKNOWN ForBlock
        if (!gotGameBlock) {
            this._openErrorReport = this._openErrorReport + "No 'define game' block." + vbCrLf;
            return false;
        }
        this.ConvertMultiLineSections();
        hasErrors = this.ConvertFriendlyIfs();
        // UNKNOWN SingleLineIfStatement
        if (hasErrors) {
            // UNKNOWN ThrowStatement
        }
        this._saveGameFile = "";
        return result;
    }
    LogASLError(err: string, type: LogType = LogType.Misc): void {
        if (type == LogType.FatalError) {
            err = "FATAL ERROR: " + err;
        } else if (type == LogType.WarningError) {
            err = "ERROR: " + err;
        } else if (type == LogType.LibraryWarningError) {
            err = "WARNING ERROR (LIBRARY): " + err;
        } else if (type == LogType.Init) {
            err = "INIT: " + err;
        } else if (type == LogType.Warning) {
            err = "WARNING: " + err;
        } else if (type == LogType.UserError) {
            err = "ERROR (REQUESTED): " + err;
        } else if (type == LogType.InternalError) {
            err = "INTERNAL ERROR: " + err;
        }
        this._log.Add(err);
    }
    GetParameter(s: string, ctx: Context, convertStringVariables: boolean = true): string {
        var newParam: string;
        var startPos: number;
        var endPos: number;
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
            } else {
                if (!Left(retrParam, 9) == "~Internal") {
                    newParam = this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%", ConvertType.Numeric, ctx), "~", ConvertType.Collectables, ctx), "$", ConvertType.Functions, ctx);
                } else {
                    newParam = retrParam;
                }
            }
        } else {
            newParam = retrParam;
        }
        return this.EvaluateInlineExpressions(newParam);
    }
    AddLine(line: string): void {
        var numLines: number;
        numLines = UBound(this._lines) + 1;
        // UNKNOWN ReDimPreserveStatement
        this._lines[numLines] = line;
    }
    LoadCASFile(filename: string): void {
        var endLineReached: boolean;
        var exitTheLoop: boolean;
        var textMode: boolean;
        var casVersion: number;
        var startCat: string = "";
        var endCatPos: number;
        var fileData: string = "";
        var chkVer: string;
        var j: number;
        var curLin: string;
        var textData: string;
        var cpos: number;
        var nextLinePos: number;
        var c: string;
        var tl: string;
        var ckw: string;
        var d: string;
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
            // UNKNOWN ThrowStatement
        }
        if (casVersion == 3) {
            startCat = this.Keyword2CAS("!startcat");
        }
        // UNKNOWN ForBlock
    }
    DecryptString(s: string): string {
        var output = "";
        // UNKNOWN ForBlock
        return output;
    }
    RemoveTabs(s: string): string {
        if (InStr(s, Chr(9)) > 0) {
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
            } while (!(finished));
        }
        return s;
    }
    DoAddRemove(childId: number, parentId: number, add: boolean, ctx: Context): void {
        if (add) {
            this.AddToObjectProperties("parent=" + this._objs[parentId].ObjectName, childId, ctx);
            this._objs[childId].ContainerRoom = this._objs[parentId].ContainerRoom;
        } else {
            this.AddToObjectProperties("not parent", childId, ctx);
        }
        if (this._gameAslVersion >= 410) {
            this.AddToObjectProperties("seen", parentId, ctx);
        }
        this.UpdateVisibilityInContainers(ctx, this._objs[parentId].ObjectName);
    }
    DoLook(id: number, ctx: Context, showExamineError: boolean = false, showDefaultDescription: boolean = true): void {
        var objectContents: string;
        var foundLook = false;
        if (this._gameAslVersion >= 391) {
            this.AddToObjectProperties("seen", id, ctx);
            this.UpdateVisibilityInContainers(ctx, this._objs[id].ObjectName);
        }
        var lookLine: string;
        var o = this._objs[id];
        // UNKNOWN ForBlock
        if (!foundLook) {
            // UNKNOWN ForBlock
        }
        if (!foundLook) {
            // UNKNOWN ForBlock
        }
        if (this._gameAslVersion >= 391) {
            objectContents = this.ListContents(id, ctx);
        } else {
            objectContents = "";
        }
        if (!foundLook && showDefaultDescription) {
            var err: PlayerError;
            if (showExamineError) {
                err = PlayerError.DefaultExamine;
            } else {
                err = PlayerError.DefaultLook;
            }
            // UNKNOWN SingleLineIfStatement
        }
        // UNKNOWN SingleLineIfStatement
    }
    DoOpenClose(id: number, open: boolean, showLook: boolean, ctx: Context): void {
        if (open) {
            this.AddToObjectProperties("opened", id, ctx);
            // UNKNOWN SingleLineIfStatement
        } else {
            this.AddToObjectProperties("not opened", id, ctx);
        }
        this.UpdateVisibilityInContainers(ctx, this._objs[id].ObjectName);
    }
    EvaluateInlineExpressions(s: string): string {
        if (this._gameAslVersion < 391) {
            return s;
        }
        var bracePos: number;
        var curPos = 1;
        var resultLine = "";
        do {
            bracePos = InStr(curPos, s, "{");
            if (bracePos != 0) {
                resultLine = resultLine + Mid(s, curPos, bracePos - curPos);
                if (Mid(s, bracePos, 2) == "{{") {
                    curPos = bracePos + 2;
                    resultLine = resultLine + "{";
                } else {
                    var EndBracePos = InStr(bracePos + 1, s, "}");
                    if (EndBracePos == 0) {
                        this.LogASLError("Expected } in '" + s + "'", LogType.WarningError);
                        return "<ERROR>";
                    } else {
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
            } else {
                resultLine = resultLine + Mid(s, curPos);
            }
        } while (!(bracePos == 0 || curPos > Len(s)));
        curPos = 1;
        do {
            bracePos = InStr(curPos, resultLine, "}}");
            if (bracePos != 0) {
                resultLine = Left(resultLine, bracePos) + Mid(resultLine, bracePos + 2);
                curPos = bracePos + 1;
            }
        } while (!(bracePos == 0 || curPos > Len(resultLine)));
        return resultLine;
    }
    ExecAddRemove(cmd: string, ctx: Context): void {
        var childId: number;
        var childName: string;
        var doAdd: boolean;
        var sepPos: number;
        var parentId: number;
        var sepLen: number;
        var parentName: string;
        var verb: string = "";
        var action: string;
        var foundAction: boolean;
        var actionScript: string = "";
        var propertyExists: boolean;
        var textToPrint: string;
        var isContainer: boolean;
        var gotObject: boolean;
        var childLength: number;
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
            this.PlayerErrorMessage(PlayerError.BadCommand, ctx);
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
                    this.Print("(first taking " + this._objs[childId].Article + ")", ctx);
                    ctx.AllowRealNamesInCommand = true;
                    this.ExecCommand("take " + this._objs[childId].ObjectName, ctx, false, null, true);
                    // UNKNOWN SingleLineIfStatement
                }
                if (!gotObject) {
                    this._badCmdBefore = verb;
                    // UNKNOWN ExitSubStatement
                }
            } else {
                // UNKNOWN SingleLineIfStatement
                this._badCmdBefore = verb;
                // UNKNOWN ExitSubStatement
            }
        } else {
            childId = this.Disambiguate(childName, "inventory;" + this._currentRoom, ctx);
            if (childId <= 0) {
                // UNKNOWN SingleLineIfStatement
                this._badCmdBefore = verb;
                // UNKNOWN ExitSubStatement
            }
        }
        if (noParentSpecified && doAdd) {
            this.SetStringContents("quest.error.article", this._objs[childId].Article, ctx);
            this.PlayerErrorMessage(PlayerError.BadPut, ctx);
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
                // UNKNOWN SingleLineIfStatement
                this._badCmdBefore = Left(cmd, sepPos + sepLen);
                // UNKNOWN ExitSubStatement
            }
        } else {
            if (!this.IsYes(this.GetObjectProperty("parent", childId, true, false))) {
                this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
                // UNKNOWN ExitSubStatement
            }
            parentId = this.GetObjectIdNoAlias(this.GetObjectProperty("parent", childId, false, true));
        }
        isContainer = this.IsYes(this.GetObjectProperty("container", parentId, true, false));
        if (!isContainer) {
            if (doAdd) {
                this.PlayerErrorMessage(PlayerError.CantPut, ctx);
            } else {
                this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }
            // UNKNOWN ExitSubStatement
        }
        if (this.IsYes(this.GetObjectProperty("parent", childId, true, false))) {
            if (doAdd && LCase(this.GetObjectProperty("parent", childId, false, false)) == LCase(this._objs[parentId].ObjectName)) {
                this.PlayerErrorMessage(PlayerError.AlreadyPut, ctx);
            }
        }
        var canAccessObject = this.PlayerCanAccessObject(childId);
        if (!canAccessObject.CanAccessObject) {
            if (doAdd) {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessObject.ErrorMsg);
            } else {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessObject.ErrorMsg);
            }
            // UNKNOWN ExitSubStatement
        }
        var canAccessParent = this.PlayerCanAccessObject(parentId);
        if (!canAccessParent.CanAccessObject) {
            if (doAdd) {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessParent.ErrorMsg);
            } else {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessParent.ErrorMsg);
            }
            // UNKNOWN ExitSubStatement
        }
        if (!this.IsYes(this.GetObjectProperty("surface", parentId, true, false)) && !this.IsYes(this.GetObjectProperty("opened", parentId, true, false))) {
            if (doAdd) {
                this.PlayerErrorMessage(PlayerError.CantPut, ctx);
            } else {
                this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }
            // UNKNOWN ExitSubStatement
        }
        var o = this._objs[parentId];
        // UNKNOWN ForBlock
        if (foundAction) {
            this.SetStringContents("quest." + LCase(action) + ".object.name", this._objs[childId].ObjectName, ctx);
            this.ExecuteScript(actionScript, ctx, parentId);
        } else {
            propertyExists = this.IsYes(this.GetObjectProperty(action, parentId, true, false));
            if (!propertyExists) {
                if (doAdd) {
                    this.PlayerErrorMessage(PlayerError.CantPut, ctx);
                } else {
                    this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
                }
            } else {
                textToPrint = this.GetObjectProperty(action, parentId, false, false);
                if (textToPrint == "") {
                    if (doAdd) {
                        this.PlayerErrorMessage(PlayerError.DefaultPut, ctx);
                    } else {
                        this.PlayerErrorMessage(PlayerError.DefaultRemove, ctx);
                    }
                } else {
                    this.Print(textToPrint, ctx);
                }
                this.DoAddRemove(childId, parentId, doAdd, ctx);
            }
        }
    }
    ExecAddRemoveScript(parameter: string, add: boolean, ctx: Context): void {
        var childId: number;
        var parentId: number;
        var commandName: string;
        var childName: string;
        var parentName: string = "";
        var scp: number;
        if (add) {
            commandName = "add";
        } else {
            commandName = "remove";
        }
        scp = InStr(parameter, ";");
        if (scp == 0 && add) {
            this.LogASLError("No parent specified in '" + commandName + " <" + parameter + ">", LogType.WarningError);
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
            this.LogASLError("Invalid child object name specified in '" + commandName + " <" + parameter + ">", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        if (scp != 0) {
            parentId = this.GetObjectIdNoAlias(parentName);
            if (parentId == 0) {
                this.LogASLError("Invalid parent object name specified in '" + commandName + " <" + parameter + ">", LogType.WarningError);
                // UNKNOWN ExitSubStatement
            }
            this.DoAddRemove(childId, parentId, add, ctx);
        } else {
            this.AddToObjectProperties("not parent", childId, ctx);
            this.UpdateVisibilityInContainers(ctx, this._objs[parentId].ObjectName);
        }
    }
    ExecOpenClose(cmd: string, ctx: Context): void {
        var id: number;
        var name: string;
        var doOpen: boolean;
        var isOpen: boolean;
        var foundAction: boolean;
        var action: string = "";
        var actionScript: string = "";
        var propertyExists: boolean;
        var textToPrint: string;
        var isContainer: boolean;
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
            // UNKNOWN SingleLineIfStatement
            this._badCmdBefore = action;
            // UNKNOWN ExitSubStatement
        }
        isContainer = this.IsYes(this.GetObjectProperty("container", id, true, false));
        if (!isContainer) {
            if (doOpen) {
                this.PlayerErrorMessage(PlayerError.CantOpen, ctx);
            } else {
                this.PlayerErrorMessage(PlayerError.CantClose, ctx);
            }
            // UNKNOWN ExitSubStatement
        }
        isOpen = this.IsYes(this.GetObjectProperty("opened", id, true, false));
        if (doOpen && isOpen) {
            this.PlayerErrorMessage(PlayerError.AlreadyOpen, ctx);
            // UNKNOWN ExitSubStatement
        } else if (!doOpen && !isOpen) {
            this.PlayerErrorMessage(PlayerError.AlreadyClosed, ctx);
            // UNKNOWN ExitSubStatement
        }
        var canAccessObject = this.PlayerCanAccessObject(id);
        if (!canAccessObject.CanAccessObject) {
            if (doOpen) {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantOpen, ctx, canAccessObject.ErrorMsg);
            } else {
                this.PlayerErrorMessage_ExtendInfo(PlayerError.CantClose, ctx, canAccessObject.ErrorMsg);
            }
            // UNKNOWN ExitSubStatement
        }
        var o = this._objs[id];
        // UNKNOWN ForBlock
        if (foundAction) {
            this.ExecuteScript(actionScript, ctx, id);
        } else {
            propertyExists = this.IsYes(this.GetObjectProperty(action, id, true, false));
            if (!propertyExists) {
                if (doOpen) {
                    this.PlayerErrorMessage(PlayerError.CantOpen, ctx);
                } else {
                    this.PlayerErrorMessage(PlayerError.CantClose, ctx);
                }
            } else {
                textToPrint = this.GetObjectProperty(action, id, false, false);
                if (textToPrint == "") {
                    if (doOpen) {
                        this.PlayerErrorMessage(PlayerError.DefaultOpen, ctx);
                    } else {
                        this.PlayerErrorMessage(PlayerError.DefaultClose, ctx);
                    }
                } else {
                    this.Print(textToPrint, ctx);
                }
                this.DoOpenClose(id, doOpen, true, ctx);
            }
        }
    }
    ExecuteSelectCase(script: string, ctx: Context): void {
        var afterLine = this.GetAfterParameter(script);
        if (!this.BeginsWith(afterLine, "do <!intproc")) {
            this.LogASLError("No case block specified for '" + script + "'", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        var blockName = this.GetParameter(afterLine, ctx);
        var block = this.DefineBlockParam("procedure", blockName);
        var checkValue = this.GetParameter(script, ctx);
        var caseMatch = false;
        // UNKNOWN ForBlock
    }
    ExecVerb(cmd: string, ctx: Context, libCommands: boolean = false): boolean {
        var gameBlock: DefineBlock;
        var foundVerb = false;
        var verbProperty: string = "";
        var script: string = "";
        var verbsList: string;
        var thisVerb: string = "";
        var scp: number;
        var id: number;
        var verbObject: string = "";
        var verbTag: string;
        var thisScript: string = "";
        if (!libCommands) {
            verbTag = "verb ";
        } else {
            verbTag = "lib verb ";
        }
        gameBlock = this.GetDefineBlock("game");
        // UNKNOWN ForBlock
        if (foundVerb) {
            id = this.Disambiguate(verbObject, "inventory;" + this._currentRoom, ctx);
            if (id < 0) {
                // UNKNOWN SingleLineIfStatement
                this._badCmdBefore = thisVerb;
            } else {
                this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
                var foundAction = false;
                var o = this._objs[id];
                // UNKNOWN ForBlock
                if (thisScript != "") {
                    this.ExecuteScript(thisScript, ctx, id);
                }
                if (!foundAction) {
                    // UNKNOWN ForBlock
                }
                if (!foundAction) {
                    this.ExecuteScript(script, ctx);
                }
            }
        }
        return foundVerb;
    }
    ExpressionHandler(expr: string): ExpressionResult {
        var openBracketPos: number;
        var endBracketPos: number;
        var res: ExpressionResult = new ExpressionResult();
        do {
            openBracketPos = InStr(expr, "(");
            if (openBracketPos != 0) {
                var BracketCount = 1;
                endBracketPos = 0;
                // UNKNOWN ForBlock
                if (endBracketPos != 0) {
                    var NestedResult = this.ExpressionHandler(Mid(expr, openBracketPos + 1, endBracketPos - openBracketPos - 1));
                    if (NestedResult.Success != ExpressionSuccess.OK) {
                        res.Success = NestedResult.Success;
                        res.Message = NestedResult.Message;
                        return res;
                    }
                    expr = Left(expr, openBracketPos - 1) + " " + NestedResult.Result + " " + Mid(expr, endBracketPos + 1);
                } else {
                    res.Message = "Missing closing bracket";
                    res.Success = ExpressionSuccess.Fail;
                    return res;
                }
            }
        } while (!(openBracketPos == 0));
        var numElements = 1;
        var elements: string[];
        elements = [];
        var numOperators = 0;
        var operators: string[] = [];
        var newElement: boolean;
        var obscuredExpr = this.ObscureNumericExps(expr);
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        var opNum = 0;
        do {
            // UNKNOWN ForBlock
            if (opNum == 0) {
                // UNKNOWN ForBlock
            }
            if (opNum != 0) {
                var val1 = parseFloat(elements[opNum]);
                var val2 = parseFloat(elements[opNum + 1]);
                var result: number;
                // UNKNOWN SelectBlock
                elements[opNum] = (result).toString();
                // UNKNOWN ForBlock
                // UNKNOWN ForBlock
                numOperators = numOperators - 1;
                numElements = numElements - 1;
                // UNKNOWN ReDimPreserveStatement
                // UNKNOWN ReDimPreserveStatement
            }
        } while (!(opNum == 0 || numOperators == 0));
        res.Success = ExpressionSuccess.OK;
        res.Result = elements[1];
        return res;
    }
    ListContents(id: number, ctx: Context): string {
        var contentsIDs: number[] = [];
        if (!this.IsYes(this.GetObjectProperty("container", id, true, false))) {
            return "";
        }
        if (!this.IsYes(this.GetObjectProperty("opened", id, true, false)) && !this.IsYes(this.GetObjectProperty("transparent", id, true, false)) && !this.IsYes(this.GetObjectProperty("surface", id, true, false))) {
            if (this.DoAction(id, "list closed", ctx, false)) {
                return "<script>";
            } else {
                return this.GetObjectProperty("list closed", id, false, false);
            }
        }
        var numContents = 0;
        // UNKNOWN ForBlock
        var contents = "";
        if (numContents > 0) {
            if (this.DoAction(id, "list", ctx, false)) {
                return "<script>";
            }
            if (this.IsYes(this.GetObjectProperty("list", id, true, false))) {
                var listString = this.GetObjectProperty("list", id, false, false);
                var displayList = true;
                if (listString != "") {
                    if (Right(listString, 1) == ":") {
                        contents = Left(listString, Len(listString) - 1) + " ";
                    } else {
                        contents = listString;
                        displayList = false;
                    }
                } else {
                    contents = UCase(Left(this._objs[id].Article, 1)) + Mid(this._objs[id].Article, 2) + " contains ";
                }
                if (displayList) {
                    // UNKNOWN ForBlock
                }
                return contents + ".";
            }
            return "";
        }
        if (this.DoAction(id, "list empty", ctx, false)) {
            return "<script>";
        } else {
            return this.GetObjectProperty("list empty", id, false, false);
        }
    }
    ObscureNumericExps(s: string): string {
        var ep: number;
        var result = s;
        var pos = 1;
        do {
            ep = InStr(pos, result, "E");
            if (ep != 0) {
                result = Left(result, ep) + "X" + Mid(result, ep + 2);
                pos = ep + 2;
            }
        } while (!(ep == 0));
        return result;
    }
    ProcessListInfo(line: string, id: number): void {
        var listInfo: TextAction = new TextAction();
        var propName: string = "";
        if (this.BeginsWith(line, "list closed <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list closed";
        } else if (Trim(line) == "list closed off") {
            // UNKNOWN ExitSubStatement
        } else if (this.BeginsWith(line, "list closed")) {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = this.GetEverythingAfter(line, "list closed");
            propName = "list closed";
        } else if (this.BeginsWith(line, "list empty <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list empty";
        } else if (Trim(line) == "list empty off") {
            // UNKNOWN ExitSubStatement
        } else if (this.BeginsWith(line, "list empty")) {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = this.GetEverythingAfter(line, "list empty");
            propName = "list empty";
        } else if (Trim(line) == "list off") {
            this.AddToObjectProperties("not list", id, this._nullContext);
            // UNKNOWN ExitSubStatement
        } else if (this.BeginsWith(line, "list <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetParameter(line, this._nullContext);
            propName = "list";
        } else if (this.BeginsWith(line, "list ")) {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = this.GetEverythingAfter(line, "list ");
            propName = "list";
        }
        if (propName != "") {
            if (listInfo.Type == TextActionType.Text) {
                this.AddToObjectProperties(propName + "=" + listInfo.Data, id, this._nullContext);
            } else {
                this.AddToObjectActions("<" + propName + "> " + listInfo.Data, id, this._nullContext);
            }
        }
    }
    GetHTMLColour(colour: string, defaultColour: string): string {
        colour = LCase(colour);
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SelectBlock
    }
    DoPrint(text: string): void {
        // UNKNOWN RaiseEventStatement
    }
    DestroyExit(exitData: string, ctx: Context): void {
        var fromRoom: string = "";
        var toRoom: string = "";
        var roomId: number;
        var exitId: number;
        var scp = InStr(exitData, ";");
        if (scp == 0) {
            this.LogASLError("No exit name specified in 'destroy exit <" + exitData + ">'");
            // UNKNOWN ExitSubStatement
        }
        var roomExit: RoomExit;
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
            var found = false;
            // UNKNOWN ForBlock
            if (!found) {
                this.LogASLError("No such room '" + fromRoom + "'");
                // UNKNOWN ExitSubStatement
            }
            found = false;
            var r = this._rooms[roomId];
            // UNKNOWN ForBlock
            if (found) {
                // UNKNOWN ForBlock
                // UNKNOWN ReDimPreserveStatement
                r.NumberPlaces = r.NumberPlaces - 1;
            }
        }
        this.ShowRoomInfo(this._currentRoom, ctx, true);
        this.UpdateObjectList(ctx);
        this.AddToChangeLog("room " + fromRoom, "destroy exit " + toRoom);
    }
    DoClear(): void {
        this._player.ClearScreen();
    }
    DoWait(): void {
        this._player.DoWait();
        this.ChangeState(State.Waiting);
        // UNKNOWN SyncLockBlock
    }
    ExecuteFlag(line: string, ctx: Context): void {
        var propertyString: string = "";
        if (this.BeginsWith(line, "on ")) {
            propertyString = this.GetParameter(line, ctx);
        } else if (this.BeginsWith(line, "off ")) {
            propertyString = "not " + this.GetParameter(line, ctx);
        }
        this.AddToObjectProperties(propertyString, 1, ctx);
    }
    ExecuteIfFlag(flag: string): boolean {
        return this.GetObjectProperty(flag, 1, true) == "yes";
    }
    ExecuteIncDec(line: string, ctx: Context): void {
        var variable: string;
        var change: number;
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
        // UNKNOWN SingleLineIfStatement
        if (this.BeginsWith(line, "inc ")) {
            value = value + change;
        } else if (this.BeginsWith(line, "dec ")) {
            value = value - change;
        }
        var arrayIndex = this.GetArrayIndex(variable, ctx);
        this.SetNumericVariableContents(arrayIndex.Name, value, ctx, arrayIndex.Index);
    }
    ExtractFile(file: string): string {
        var length: number;
        var startPos: number;
        var extracted: boolean;
        var resId: number;
        // UNKNOWN SingleLineIfStatement
        var found = false;
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("Unable to extract '" + file + "' - not present in resources.", LogType.WarningError);
            return null;
        }
        var fileName = System.IO.Path.Combine(this._tempFolder, file);
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
        if (!extracted) {
            var fileData = Mid(this._casFileData, startPos, length);
            System.IO.File.WriteAllText(fileName, fileData, System.Text.Encoding.GetEncoding(1252));
            this._resources[resId].Extracted = true;
        }
        return fileName;
    }
    AddObjectAction(id: number, name: string, script: string, noUpdate: boolean = false): void {
        var actionNum: number;
        var foundExisting = false;
        var o = this._objs[id];
        // UNKNOWN ForBlock
        if (!foundExisting) {
            o.NumberActions = o.NumberActions + 1;
            // UNKNOWN ReDimPreserveStatement
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }
        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;
        this.ObjectActionUpdate(id, name, script, noUpdate);
    }
    AddToChangeLog(appliesTo: string, changeData: string): void {
        this._gameChangeData.NumberChanges = this._gameChangeData.NumberChanges + 1;
        // UNKNOWN ReDimPreserveStatement
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges] = new ChangeType();
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges].AppliesTo = appliesTo;
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges].Change = changeData;
    }
    AddToObjectChangeLog(appliesToType: any, appliesTo: string, element: string, changeData: string): void {
        var changeLog: ChangeLog;
        // UNKNOWN SelectBlock
        changeLog.AddItem(appliesTo, element, changeData);
    }
    AddToGiveInfo(id: number, giveData: string): void {
        var giveType: GiveType;
        var actionName: string;
        var actionScript: string;
        var o = this._objs[id];
        if (this.BeginsWith(giveData, "to ")) {
            giveData = this.GetEverythingAfter(giveData, "to ");
            if (this.BeginsWith(giveData, "anything ")) {
                o.GiveToAnything = this.GetEverythingAfter(giveData, "anything ");
                this.AddObjectAction(id, "give to anything", o.GiveToAnything, true);
                // UNKNOWN ExitSubStatement
            } else {
                giveType = GiveType.GiveToSomething;
                actionName = "give to ";
            }
        } else {
            if (this.BeginsWith(giveData, "anything ")) {
                o.GiveAnything = this.GetEverythingAfter(giveData, "anything ");
                this.AddObjectAction(id, "give anything", o.GiveAnything, true);
                // UNKNOWN ExitSubStatement
            } else {
                giveType = GiveType.GiveSomethingTo;
                actionName = "give ";
            }
        }
        if (Left(Trim(giveData), 1) == "<") {
            var name = this.GetParameter(giveData, this._nullContext);
            var dataId: number;
            actionName = actionName + "'" + name + "'";
            var found = false;
            // UNKNOWN ForBlock
            if (!found) {
                o.NumberGiveData = o.NumberGiveData + 1;
                // UNKNOWN ReDimPreserveStatement
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
    }
    AddToObjectActions(actionInfo: string, id: number, ctx: Context): void {
        var actionNum: number;
        var foundExisting = false;
        var name = LCase(this.GetParameter(actionInfo, ctx));
        var ep = InStr(actionInfo, ">");
        if (ep == Len(actionInfo)) {
            this.LogASLError("No script given for '" + name + "' action data", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        var script = Trim(Mid(actionInfo, ep + 1));
        var o = this._objs[id];
        // UNKNOWN ForBlock
        if (!foundExisting) {
            o.NumberActions = o.NumberActions + 1;
            // UNKNOWN ReDimPreserveStatement
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }
        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;
        this.ObjectActionUpdate(id, name, script);
    }
    AddToObjectAltNames(altNames: string, id: number): void {
        var o = this._objs[id];
        do {
            var endPos = InStr(altNames, ";");
            // UNKNOWN SingleLineIfStatement
            var curName = Trim(Left(altNames, endPos - 1));
            if (curName != "") {
                o.NumberAltNames = o.NumberAltNames + 1;
                // UNKNOWN ReDimPreserveStatement
                o.AltNames[o.NumberAltNames] = curName;
            }
            altNames = Mid(altNames, endPos + 1);
        } while (!(Trim(altNames) == ""));
    }
    AddToObjectProperties(propertyInfo: string, id: number, ctx: Context): void {
        // UNKNOWN SingleLineIfStatement
        if (Right(propertyInfo, 1) != ";") {
            propertyInfo = propertyInfo + ";";
        }
        do {
            var scp = InStr(propertyInfo, ";");
            var info = Left(propertyInfo, scp - 1);
            propertyInfo = Trim(Mid(propertyInfo, scp + 1));
            var name: string;
            var value: string;
            var num: number;
            // UNKNOWN SingleLineIfStatement
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
            // UNKNOWN ForBlock
            if (!found) {
                o.NumberProperties = o.NumberProperties + 1;
                // UNKNOWN ReDimPreserveStatement
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
        } while (!(Len(Trim(propertyInfo)) == 0));
    }
    AddToUseInfo(id: number, useData: string): void {
        var useType: UseType;
        var o = this._objs[id];
        if (this.BeginsWith(useData, "on ")) {
            useData = this.GetEverythingAfter(useData, "on ");
            if (this.BeginsWith(useData, "anything ")) {
                o.UseOnAnything = this.GetEverythingAfter(useData, "anything ");
                // UNKNOWN ExitSubStatement
            } else {
                useType = UseType.UseOnSomething;
            }
        } else {
            if (this.BeginsWith(useData, "anything ")) {
                o.UseAnything = this.GetEverythingAfter(useData, "anything ");
                // UNKNOWN ExitSubStatement
            } else {
                useType = UseType.UseSomethingOn;
            }
        }
        if (Left(Trim(useData), 1) == "<") {
            var objectName = this.GetParameter(useData, this._nullContext);
            var dataId: number;
            var found = false;
            // UNKNOWN ForBlock
            if (!found) {
                o.NumberUseData = o.NumberUseData + 1;
                // UNKNOWN ReDimPreserveStatement
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
    }
    CapFirst(s: string): string {
        return UCase(Left(s, 1)) + Mid(s, 2);
    }
    ConvertVarsIn(s: string, ctx: Context): string {
        return this.GetParameter("<" + s + ">", ctx);
    }
    DisambObjHere(ctx: Context, id: number, firstPlace: string, twoPlaces: boolean = false, secondPlace: string = "", isExit: boolean = false): boolean {
        var isSeen: boolean;
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
    }
    ExecClone(cloneString: string, ctx: Context): void {
        var id: number;
        var newName: string;
        var cloneTo: string;
        var scp = InStr(cloneString, ";");
        if (scp == 0) {
            this.LogASLError("No new object name specified in 'clone <" + cloneString + ">", LogType.WarningError);
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
        // UNKNOWN ReDimPreserveStatement
        this._objs[this._numberObjs] = new ObjectType();
        this._objs[this._numberObjs] = this._objs[id];
        this._objs[this._numberObjs].ContainerRoom = cloneTo;
        this._objs[this._numberObjs].ObjectName = newName;
        if (this._objs[id].IsRoom) {
            this._numberRooms = this._numberRooms + 1;
            // UNKNOWN ReDimPreserveStatement
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
    }
    ExecOops(correction: string, ctx: Context): void {
        if (this._badCmdBefore != "") {
            if (this._badCmdAfter == "") {
                this.ExecCommand(this._badCmdBefore + " " + correction, ctx, false);
            } else {
                this.ExecCommand(this._badCmdBefore + " " + correction + " " + this._badCmdAfter, ctx, false);
            }
        }
    }
    ExecType(typeData: string, ctx: Context): void {
        var id: number;
        var found: boolean;
        var scp = InStr(typeData, ";");
        if (scp == 0) {
            this.LogASLError("No type name given in 'type <" + typeData + ">'");
            // UNKNOWN ExitSubStatement
        }
        var objName = Trim(Left(typeData, scp - 1));
        var typeName = Trim(Mid(typeData, scp + 1));
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("No such object in 'type <" + typeData + ">'");
            // UNKNOWN ExitSubStatement
        }
        var o = this._objs[id];
        o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
        // UNKNOWN ReDimPreserveStatement
        o.TypesIncluded[o.NumberTypesIncluded] = typeName;
        var propertyData = this.GetPropertiesInType(typeName);
        this.AddToObjectProperties(propertyData.Properties, id, ctx);
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
    }
    ExecuteIfAction(actionData: string): boolean {
        var id: number;
        var scp = InStr(actionData, ";");
        if (scp == 0) {
            this.LogASLError("No action name given in condition 'action <" + actionData + ">' ...", LogType.WarningError);
            return false;
        }
        var objName = Trim(Left(actionData, scp - 1));
        var actionName = Trim(Mid(actionData, scp + 1));
        var found = false;
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("No such object '" + objName + "' in condition 'action <" + actionData + ">' ...", LogType.WarningError);
            return false;
        }
        var o = this._objs[id];
        // UNKNOWN ForBlock
        return false;
    }
    ExecuteIfType(typeData: string): boolean {
        var id: number;
        var scp = InStr(typeData, ";");
        if (scp == 0) {
            this.LogASLError("No type name given in condition 'type <" + typeData + ">' ...", LogType.WarningError);
            return false;
        }
        var objName = Trim(Left(typeData, scp - 1));
        var typeName = Trim(Mid(typeData, scp + 1));
        var found = false;
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("No such object '" + objName + "' in condition 'type <" + typeData + ">' ...", LogType.WarningError);
            return false;
        }
        var o = this._objs[id];
        // UNKNOWN ForBlock
        return false;
    }
    GetArrayIndex(varName: string, ctx: Context): ArrayResult {
        var result: ArrayResult = new ArrayResult();
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
    }
    Disambiguate(name: string, containedIn: string, ctx: Context, isExit: boolean = false): number {
        var numberCorresIds = 0;
        var idNumbers: number[] = [];
        var firstPlace: string;
        var secondPlace: string = "";
        var twoPlaces: boolean;
        var descriptionText: string[];
        var validNames: string[];
        var numValidNames: number;
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
            // UNKNOWN ForBlock
        }
        if (name == "it" || name == "them" || name == "this" || name == "those" || name == "these" || name == "that") {
            this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Inanimate && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
                return -2;
            }
        } else if (name == "him") {
            this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Male && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
                return -2;
            }
        } else if (name == "her") {
            this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Female && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
                return -2;
            }
        }
        this._thisTurnIt = 0;
        if (this.BeginsWith(name, "the ")) {
            name = this.GetEverythingAfter(name, "the ");
        }
        // UNKNOWN ForBlock
        if (this._gameAslVersion >= 391 && numberCorresIds == 0 && this._useAbbreviations && Len(name) > 0) {
            // UNKNOWN ForBlock
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
            var menuItems: any = {};
            // UNKNOWN ForBlock
            var mnu: MenuData = new MenuData();
            var response: string = this.ShowMenu(mnu);
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
    }
    DisplayStatusVariableInfo(id: number, type: VarType, ctx: Context): string {
        var displayData: string = "";
        var ep: number;
        if (type == VarType.String) {
            displayData = this.ConvertVarsIn(this._stringVariable[id].DisplayString, ctx);
            ep = InStr(displayData, "!");
            if (ep != 0) {
                displayData = Left(displayData, ep - 1) + this._stringVariable[id].VariableContents[0] + Mid(displayData, ep + 1);
            }
        } else if (type == VarType.Numeric) {
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
    }
    DoAction(id: number, action: string, ctx: Context, logError: boolean = true): boolean {
        var found: boolean;
        var script: string = "";
        var o = this._objs[id];
        // UNKNOWN ForBlock
        if (!found) {
            // UNKNOWN SingleLineIfStatement
            return false;
        }
        var newCtx: Context = this.CopyContext(ctx);
        newCtx.CallingObjectId = id;
        this.ExecuteScript(script, newCtx, id);
        return true;
    }
    HasAction(id: number, action: string): boolean {
        var o = this._objs[id];
        // UNKNOWN ForBlock
        return false;
    }
    ExecForEach(scriptLine: string, ctx: Context): void {
        var inLocation: string;
        var scriptToRun: string;
        var isExit: boolean;
        var isRoom: boolean;
        if (this.BeginsWith(scriptLine, "object ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "object ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each object " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                // UNKNOWN ExitSubStatement
            }
        } else if (this.BeginsWith(scriptLine, "exit ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "exit ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each exit " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                // UNKNOWN ExitSubStatement
            }
            isExit = true;
        } else if (this.BeginsWith(scriptLine, "room ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "room ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each room " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                // UNKNOWN ExitSubStatement
            }
            isRoom = true;
        } else {
            this.LogASLError("Unknown type in 'for each " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
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
        // UNKNOWN ForBlock
    }
    ExecuteAction(data: string, ctx: Context): void {
        var actionName: string;
        var script: string;
        var actionNum: number;
        var id: number;
        var foundExisting = false;
        var foundObject = false;
        var param = this.GetParameter(data, ctx);
        var scp = InStr(param, ";");
        if (scp == 0) {
            this.LogASLError("No action name specified in 'action " + data + "'", LogType.WarningError);
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
        // UNKNOWN ForBlock
        if (!foundObject) {
            this.LogASLError("No such object '" + objName + "' in 'action " + data + "'", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        var o = this._objs[id];
        // UNKNOWN ForBlock
        if (!foundExisting) {
            o.NumberActions = o.NumberActions + 1;
            // UNKNOWN ReDimPreserveStatement
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }
        o.Actions[actionNum].ActionName = actionName;
        o.Actions[actionNum].Script = script;
        this.ObjectActionUpdate(id, actionName, script);
    }
    ExecuteCondition(condition: string, ctx: Context): boolean {
        var result: boolean;
        var thisNot: boolean;
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
        // UNKNOWN SingleLineIfStatement
        return result;
    }
    ExecuteConditions(list: string, ctx: Context): boolean {
        var conditions: string[];
        var numConditions = 0;
        var operations: string[];
        var obscuredConditionList = this.ObliterateParameters(list);
        var pos = 1;
        var isFinalCondition = false;
        do {
            numConditions = numConditions + 1;
            // UNKNOWN ReDimPreserveStatement
            // UNKNOWN ReDimPreserveStatement
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
            pos = InStr(nextConditionPos, obscuredConditionList, " ");
        } while (!(isFinalCondition));
        operations[0] = "AND";
        var result = true;
        // UNKNOWN ForBlock
        return result;
    }
    ExecuteCreate(data: string, ctx: Context): void {
        var newName: string;
        if (this.BeginsWith(data, "room ")) {
            newName = this.GetParameter(data, ctx);
            this._numberRooms = this._numberRooms + 1;
            // UNKNOWN ReDimPreserveStatement
            this._rooms[this._numberRooms] = new RoomType();
            this._rooms[this._numberRooms].RoomName = newName;
            this._numberObjs = this._numberObjs + 1;
            // UNKNOWN ReDimPreserveStatement
            this._objs[this._numberObjs] = new ObjectType();
            this._objs[this._numberObjs].ObjectName = newName;
            this._objs[this._numberObjs].IsRoom = true;
            this._objs[this._numberObjs].CorresRoom = newName;
            this._objs[this._numberObjs].CorresRoomId = this._numberRooms;
            this._rooms[this._numberRooms].ObjId = this._numberObjs;
            this.AddToChangeLog("room " + newName, "create");
            if (this._gameAslVersion >= 410) {
                this.AddToObjectProperties(this._defaultRoomProperties.Properties, this._numberObjs, ctx);
                // UNKNOWN ForBlock
                this._rooms[this._numberRooms].Exits = new RoomExits();
                this._rooms[this._numberRooms].Exits.ObjId = this._rooms[this._numberRooms].ObjId;
            }
        } else if (this.BeginsWith(data, "object ")) {
            var paramData = this.GetParameter(data, ctx);
            var scp = InStr(paramData, ";");
            var containerRoom: string;
            if (scp == 0) {
                newName = paramData;
                containerRoom = "";
            } else {
                newName = Trim(Left(paramData, scp - 1));
                containerRoom = Trim(Mid(paramData, scp + 1));
            }
            this._numberObjs = this._numberObjs + 1;
            // UNKNOWN ReDimPreserveStatement
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
                // UNKNOWN ForBlock
            }
            // UNKNOWN SingleLineIfStatement
        } else if (this.BeginsWith(data, "exit ")) {
            this.ExecuteCreateExit(data, ctx);
        }
    }
    ExecuteCreateExit(data: string, ctx: Context): void {
        var scrRoom: string;
        var destRoom: string = "";
        var destId: number;
        var exitData = this.GetEverythingAfter(data, "exit ");
        var newName = this.GetParameter(data, ctx);
        var scp = InStr(newName, ";");
        if (this._gameAslVersion < 410) {
            if (scp == 0) {
                this.LogASLError("No exit destination given in 'create exit " + exitData + "'", LogType.WarningError);
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
            this.LogASLError("No such room '" + scrRoom + "'", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        if (this._gameAslVersion < 410) {
            destRoom = Trim(Mid(newName, scp + 1));
            if (destRoom != "") {
                destId = this.GetRoomID(destRoom, ctx);
                if (destId == 0) {
                    this.LogASLError("No such room '" + destRoom + "'", LogType.WarningError);
                    // UNKNOWN ExitSubStatement
                }
            }
        }
        var exists = false;
        if (this.BeginsWith(exitData, "<")) {
            if (this._gameAslVersion >= 410) {
                exists = this._rooms[srcId].Exits.Places.ContainsKey(destRoom);
            } else {
                // UNKNOWN ForBlock
            }
            if (exists) {
                this.LogASLError("Exit from '" + scrRoom + "' to '" + destRoom + "' already exists", LogType.WarningError);
                // UNKNOWN ExitSubStatement
            }
        }
        var paramPos = InStr(exitData, "<");
        var saveData: string;
        if (paramPos == 0) {
            saveData = exitData;
        } else {
            saveData = Left(exitData, paramPos - 1);
            saveData = saveData + "<" + this.GetParameter(exitData, ctx) + ">";
        }
        this.AddToChangeLog("room " + this._rooms[srcId].RoomName, "exit " + saveData);
        var r = this._rooms[srcId];
        if (this._gameAslVersion >= 410) {
            r.Exits.AddExitFromCreateScript(exitData, ctx);
        } else {
            if (this.BeginsWith(exitData, "north ")) {
                r.North.Data = destRoom;
                r.North.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "south ")) {
                r.South.Data = destRoom;
                r.South.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "east ")) {
                r.East.Data = destRoom;
                r.East.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "west ")) {
                r.West.Data = destRoom;
                r.West.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "northeast ")) {
                r.NorthEast.Data = destRoom;
                r.NorthEast.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "northwest ")) {
                r.NorthWest.Data = destRoom;
                r.NorthWest.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "southeast ")) {
                r.SouthEast.Data = destRoom;
                r.SouthEast.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "southwest ")) {
                r.SouthWest.Data = destRoom;
                r.SouthWest.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "up ")) {
                r.Up.Data = destRoom;
                r.Up.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "down ")) {
                r.Down.Data = destRoom;
                r.Down.Type = TextActionType.Text;
            } else if (this.BeginsWith(exitData, "out ")) {
                r.Out.Text = destRoom;
            } else if (this.BeginsWith(exitData, "<")) {
                r.NumberPlaces = r.NumberPlaces + 1;
                // UNKNOWN ReDimPreserveStatement
                r.Places[r.NumberPlaces] = new PlaceType();
                r.Places[r.NumberPlaces].PlaceName = destRoom;
            } else {
                this.LogASLError("Invalid direction in 'create exit " + exitData + "'", LogType.WarningError);
            }
        }
        if (!this._gameLoading) {
            this.ShowRoomInfo(this._currentRoom, ctx, true);
            this.UpdateObjectList(ctx);
            if (this._gameAslVersion < 410) {
                if (this._currentRoom == this._rooms[srcId].RoomName) {
                    this.UpdateDoorways(srcId, ctx);
                } else if (this._currentRoom == this._rooms[destId].RoomName) {
                    this.UpdateDoorways(destId, ctx);
                }
            } else {
                this.UpdateDoorways(this.GetRoomID(this._currentRoom, ctx), ctx);
            }
        }
    }
    ExecDrop(obj: string, ctx: Context): void {
        var found: boolean;
        var parentId: number;
        var id: number;
        id = this.Disambiguate(obj, "inventory", ctx);
        if (id > 0) {
            found = true;
        } else {
            found = false;
        }
        if (!found) {
            if (id != -2) {
                if (this._gameAslVersion >= 391) {
                    this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                } else {
                    this.PlayerErrorMessage(PlayerError.BadDrop, ctx);
                }
            }
            this._badCmdBefore = "drop";
            // UNKNOWN ExitSubStatement
        }
        var isInContainer = false;
        if (this._gameAslVersion >= 391) {
            if (this.IsYes(this.GetObjectProperty("parent", id, true, false))) {
                isInContainer = true;
                var parent = this.GetObjectProperty("parent", id, false, false);
                parentId = this.GetObjectIdNoAlias(parent);
            }
        }
        var dropFound = false;
        var dropStatement: string = "";
        // UNKNOWN ForBlock
        this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
        if (!dropFound || this.BeginsWith(dropStatement, "everywhere")) {
            if (isInContainer) {
                var parentDisplayName: string;
                if (this._objs[parentId].ObjectAlias != "") {
                    parentDisplayName = this._objs[parentId].ObjectAlias;
                } else {
                    parentDisplayName = this._objs[parentId].ObjectName;
                }
                this.Print("(first removing " + this._objs[id].Article + " from " + parentDisplayName + ")", ctx);
                ctx.AllowRealNamesInCommand = true;
                this.ExecCommand("remove " + this._objs[id].ObjectName, ctx, false, null, true);
                if (this.GetObjectProperty("parent", id, false, false) != "") {
                    // UNKNOWN ExitSubStatement
                }
            }
        }
        if (!dropFound) {
            this.PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
            this.PlayerItem(this._objs[id].ObjectName, false, ctx);
        } else {
            if (this.BeginsWith(dropStatement, "everywhere")) {
                this.PlayerItem(this._objs[id].ObjectName, false, ctx);
                if (InStr(dropStatement, "<") != 0) {
                    this.Print(this.GetParameter(dropStatement, ctx), ctx);
                } else {
                    this.PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
                }
            } else if (this.BeginsWith(dropStatement, "nowhere")) {
                if (InStr(dropStatement, "<") != 0) {
                    this.Print(this.GetParameter(dropStatement, ctx), ctx);
                } else {
                    this.PlayerErrorMessage(PlayerError.CantDrop, ctx);
                }
            } else {
                this.ExecuteScript(dropStatement, ctx);
            }
        }
    }
    ExecExamine(command: string, ctx: Context): void {
        var item = LCase(Trim(this.GetEverythingAfter(command, "examine ")));
        if (item == "") {
            this.PlayerErrorMessage(PlayerError.BadExamine, ctx);
            this._badCmdBefore = "examine";
            // UNKNOWN ExitSubStatement
        }
        var id = this.Disambiguate(item, this._currentRoom + ";inventory", ctx);
        if (id <= 0) {
            // UNKNOWN SingleLineIfStatement
            this._badCmdBefore = "examine";
            // UNKNOWN ExitSubStatement
        }
        var o = this._objs[id];
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        this.DoLook(id, ctx, true);
    }
    ExecMoveThing(data: string, type: Thing, ctx: Context): void {
        var scp = InStr(data, ";");
        var name = Trim(Left(data, scp - 1));
        var place = Trim(Mid(data, scp + 1));
        this.MoveThing(name, place, type, ctx);
    }
    ExecProperty(data: string, ctx: Context): void {
        var id: number;
        var found: boolean;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No property data given in 'property <" + data + ">'", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        var name = Trim(Left(data, scp - 1));
        var properties = Trim(Mid(data, scp + 1));
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("No such object in 'property <" + data + ">'", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        this.AddToObjectProperties(properties, id, ctx);
    }
    ExecuteDo(procedureName: string, ctx: Context): void {
        var newCtx: Context = this.CopyContext(ctx);
        var numParameters = 0;
        var useNewCtx: boolean;
        if (this._gameAslVersion >= 392 && Left(procedureName, 8) == "!intproc") {
            useNewCtx = false;
        } else {
            useNewCtx = true;
        }
        if (this._gameAslVersion >= 284) {
            var obp = InStr(procedureName, "(");
            var cbp: number;
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
                    // UNKNOWN ReDimPreserveStatement
                    newCtx.Parameters[numParameters] = Trim(Mid(parameters, pos, scp - pos));
                    pos = scp + 1;
                } while (!(pos >= Len(parameters)));
            }
        }
        var block = this.DefineBlockParam("procedure", procedureName);
        if (block.StartLine == 0 && block.EndLine == 0) {
            this.LogASLError("No such procedure " + procedureName, LogType.WarningError);
        } else {
            // UNKNOWN ForBlock
        }
    }
    ExecuteDoAction(data: string, ctx: Context): void {
        var id: number;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No action name specified in 'doaction <" + data + ">'");
            // UNKNOWN ExitSubStatement
        }
        var objName = LCase(Trim(Left(data, scp - 1)));
        var actionName = Trim(Mid(data, scp + 1));
        var found = false;
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("No such object '" + objName + "'");
            // UNKNOWN ExitSubStatement
        }
        this.DoAction(id, actionName, ctx);
    }
    ExecuteIfHere(obj: string, ctx: Context): boolean {
        if (this._gameAslVersion <= 281) {
            // UNKNOWN ForBlock
        }
        // UNKNOWN ForBlock
        return false;
    }
    ExecuteIfExists(obj: string, realOnly: boolean): boolean {
        var result: boolean;
        var errorReport = false;
        var scp: number;
        if (InStr(obj, ";") != 0) {
            scp = InStr(obj, ";");
            if (LCase(Trim(Mid(obj, scp + 1))) == "report") {
                errorReport = true;
            }
            obj = Left(obj, scp - 1);
        }
        var found = false;
        if (this._gameAslVersion < 281) {
            // UNKNOWN ForBlock
        }
        if (!found) {
            // UNKNOWN ForBlock
        }
        if (found == false && errorReport) {
            this.LogASLError("No such character/object '" + obj + "'.", LogType.UserError);
        }
        // UNKNOWN SingleLineIfStatement
        if (realOnly) {
            return found;
        }
        return result;
    }
    ExecuteIfProperty(data: string): boolean {
        var id: number;
        var scp = InStr(data, ";");
        if (scp == 0) {
            this.LogASLError("No property name given in condition 'property <" + data + ">' ...", LogType.WarningError);
            return false;
        }
        var objName = Trim(Left(data, scp - 1));
        var propertyName = Trim(Mid(data, scp + 1));
        var found = false;
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("No such object '" + objName + "' in condition 'property <" + data + ">' ...", LogType.WarningError);
            return false;
        }
        return this.GetObjectProperty(propertyName, id, true) == "yes";
    }
    ExecuteRepeat(data: string, ctx: Context): void {
        var repeatWhileTrue: boolean;
        var repeatScript: string = "";
        var bracketPos: number;
        var afterBracket: string;
        var foundScript = false;
        if (this.BeginsWith(data, "while ")) {
            repeatWhileTrue = true;
            data = this.GetEverythingAfter(data, "while ");
        } else if (this.BeginsWith(data, "until ")) {
            repeatWhileTrue = false;
            data = this.GetEverythingAfter(data, "until ");
        } else {
            this.LogASLError("Expected 'until' or 'while' in 'repeat " + this.ReportErrorLine(data) + "'", LogType.WarningError);
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
        } while (!(foundScript || afterBracket == ""));
        var conditions = Trim(Left(data, bracketPos));
        var finished = false;
        do {
            if (this.ExecuteConditions(conditions, ctx) == repeatWhileTrue) {
                this.ExecuteScript(repeatScript, ctx);
            } else {
                finished = true;
            }
        } while (!(finished || this._gameFinished));
    }
    ExecuteSetCollectable(param: string, ctx: Context): void {
        var val: number;
        var id: number;
        var scp = InStr(param, ";");
        var name = Trim(Left(param, scp - 1));
        var newVal = Trim(Right(param, Len(param) - scp));
        var found = false;
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("No such collectable '" + param + "'", LogType.WarningError);
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
    }
    ExecuteWait(waitLine: string, ctx: Context): void {
        if (waitLine != "") {
            this.Print(this.GetParameter(waitLine, ctx), ctx);
        } else {
            if (this._gameAslVersion >= 410) {
                this.PlayerErrorMessage(PlayerError.DefaultWait, ctx);
            } else {
                this.Print("|nPress a key to continue...", ctx);
            }
        }
        this.DoWait();
    }
    InitFileData(fileData: string): void {
        this._fileData = fileData;
        this._fileDataPos = 1;
    }
    GetNextChunk(): string {
        var nullPos = InStr(this._fileDataPos, this._fileData, Chr(0));
        var result = Mid(this._fileData, this._fileDataPos, nullPos - this._fileDataPos);
        if (nullPos < Len(this._fileData)) {
            this._fileDataPos = nullPos + 1;
        }
        return result;
    }
    GetFileDataChars(count: number): string {
        var result = Mid(this._fileData, this._fileDataPos, count);
        this._fileDataPos = this._fileDataPos + count;
        return result;
    }
    GetObjectActions(actionInfo: string): ActionType {
        var name = LCase(this.GetParameter(actionInfo, this._nullContext));
        var ep = InStr(actionInfo, ">");
        if (ep == Len(actionInfo)) {
            this.LogASLError("No script given for '" + name + "' action data", LogType.WarningError);
            return new ActionType();
        }
        var script = Trim(Mid(actionInfo, ep + 1));
        var result: ActionType = new ActionType();
        result.ActionName = name;
        result.Script = script;
        return result;
    }
    GetObjectId(name: string, ctx: Context, room: string = ""): number {
        var id: number;
        var found = false;
        if (this.BeginsWith(name, "the ")) {
            name = this.GetEverythingAfter(name, "the ");
        }
        // UNKNOWN ForBlock
        if (!found && this._gameAslVersion >= 280) {
            id = this.Disambiguate(name, room, ctx);
            // UNKNOWN SingleLineIfStatement
        }
        if (found) {
            return id;
        }
        return -1;
    }
    GetObjectIdNoAlias(name: string): number {
        // UNKNOWN ForBlock
        return 0;
    }
    GetObjectProperty(name: string, id: number, existsOnly: boolean = false, logError: boolean = true): string {
        var result: string = "";
        var found = false;
        var o = this._objs[id];
        // UNKNOWN ForBlock
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
    }
    GetPropertiesInType(type: string, err: boolean = true): PropertiesActions {
        var blockId: number;
        var propertyList: PropertiesActions = new PropertiesActions();
        var found = false;
        // UNKNOWN ForBlock
        if (!found) {
            if (err) {
                this.LogASLError("No such type '" + type + "'", LogType.WarningError);
            }
            return new PropertiesActions();
        }
        // UNKNOWN ForBlock
        return propertyList;
    }
    GetRoomID(name: string, ctx: Context): number {
        if (InStr(name, "[") > 0) {
            var idx = this.GetArrayIndex(name, ctx);
            name = name + Trim(Str(idx));
        }
        // UNKNOWN ForBlock
        return 0;
    }
    GetTextOrScript(textScript: string): TextAction {
        var result = new TextAction();
        textScript = Trim(textScript);
        if (Left(textScript, 1) == "<") {
            result.Type = TextActionType.Text;
            result.Data = this.GetParameter(textScript, this._nullContext);
        } else {
            result.Type = TextActionType.Script;
            result.Data = textScript;
        }
        return result;
    }
    GetThingNumber(name: string, room: string, type: Thing): number {
        if (type == Thing.Character) {
            // UNKNOWN ForBlock
        } else if (type == Thing.Object) {
            // UNKNOWN ForBlock
        }
        return -1;
    }
    GetThingBlock(name: string, room: string, type: Thing): DefineBlock {
        var result = new DefineBlock();
        if (type == Thing.Character) {
            // UNKNOWN ForBlock
        } else if (type == Thing.Object) {
            // UNKNOWN ForBlock
        }
        result.StartLine = 0;
        result.EndLine = 0;
        return result;
    }
    MakeRestoreData(): string {
        var data: any = {};
        var objectData: ChangeType[] = [];
        var roomData: ChangeType[] = [];
        var numObjectData: number;
        var numRoomData: number;
        data.Append("QUEST300" + Chr(0) + this.GetOriginalFilenameForQSG() + Chr(0));
        var start = data.Length + 1;
        data.Append(this._currentRoom + Chr(0));
        // UNKNOWN ForBlock
        data.Append(Trim(Str(numObjectData + this._changeLogObjects.Changes.Count)) + Chr(0));
        // UNKNOWN ForBlock
        // UNKNOWN ForEachBlock
        data.Append(Trim(Str(this._numberObjs)) + Chr(0));
        // UNKNOWN ForBlock
        data.Append(Trim(Str(numRoomData)) + Chr(0));
        // UNKNOWN ForBlock
        data.Append(Trim(Str(this._numberTimers)) + Chr(0));
        // UNKNOWN ForBlock
        data.Append(Trim(Str(this._numberStringVariables)) + Chr(0));
        // UNKNOWN ForBlock
        data.Append(Trim(Str(this._numberNumericVariables)) + Chr(0));
        // UNKNOWN ForBlock
        var dataString: string;
        var newFileData: any = {};
        dataString = data.ToString();
        newFileData.Append(Left(dataString, start - 1));
        // UNKNOWN ForBlock
        return newFileData.ToString();
    }
    MoveThing(name: string, room: string, type: Thing, ctx: Context): void {
        var oldRoom: string = "";
        var id = this.GetThingNumber(name, "", type);
        if (InStr(room, "[") > 0) {
            var idx = this.GetArrayIndex(room, ctx);
            room = room + Trim(Str(idx));
        }
        if (type == Thing.Character) {
            this._chars[id].ContainerRoom = room;
        } else if (type == Thing.Object) {
            oldRoom = this._objs[id].ContainerRoom;
            this._objs[id].ContainerRoom = room;
        }
        if (this._gameAslVersion >= 391) {
            // UNKNOWN ForBlock
        }
        this.UpdateObjectList(ctx);
        if (this.BeginsWith(LCase(room), "inventory") || this.BeginsWith(LCase(oldRoom), "inventory")) {
            this.UpdateItems(ctx);
        }
    }
    Pause(duration: number): void {
        this._player.DoPause(duration);
        this.ChangeState(State.Waiting);
        // UNKNOWN SyncLockBlock
    }
    ConvertParameter(parameter: string, convertChar: string, action: ConvertType, ctx: Context): string {
        var result: string = "";
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
                    this.LogASLError("Line parameter <" + parameter + "> has missing " + convertChar, LogType.WarningError);
                    return "<ERROR>";
                }
                var varName = Mid(parameter, varPos + 1, (nextPos - 1) - varPos);
                if (varName == "") {
                    result = result + convertChar;
                } else {
                    if (action == ConvertType.Strings) {
                        result = result + this.GetStringContents(varName, ctx);
                    } else if (action == ConvertType.Functions) {
                        varName = this.EvaluateInlineExpressions(varName);
                        result = result + this.DoFunction(varName, ctx);
                    } else if (action == ConvertType.Numeric) {
                        result = result + Trim(Str(this.GetNumericContents(varName, ctx)));
                    } else if (action == ConvertType.Collectables) {
                        result = result + Trim(Str(this.GetCollectableAmount(varName)));
                    }
                }
                pos = nextPos + 1;
            }
        } while (!(finished));
        return result;
    }
    DoFunction(data: string, ctx: Context): string {
        var name: string;
        var parameter: string;
        var intFuncResult: string = "";
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
        } else {
            name = data;
            parameter = "";
        }
        var block: DefineBlock;
        block = this.DefineBlockParam("function", name);
        if (block.StartLine == 0 && block.EndLine == 0) {
            intFuncResult = this.DoInternalFunction(name, parameter, ctx);
            if (intFuncResult == "__NOTDEFINED") {
                this.LogASLError("No such function '" + name + "'", LogType.WarningError);
                return "[ERROR]";
            } else {
                intFuncExecuted = true;
            }
        }
        if (intFuncExecuted) {
            return intFuncResult;
        } else {
            var newCtx: Context = this.CopyContext(ctx);
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
                    // UNKNOWN ReDimPreserveStatement
                    newCtx.Parameters[numParameters] = parameterData;
                    pos = scp + 1;
                } while (!(pos >= Len(parameter)));
                this.SetStringContents("quest.function.numparameters", Trim(Str(numParameters)), ctx);
            } else {
                this.SetStringContents("quest.function.numparameters", "0", ctx);
                newCtx.NumParameters = 0;
            }
            var result: string = "";
            // UNKNOWN ForBlock
            return result;
        }
    }
    DoInternalFunction(name: string, parameter: string, ctx: Context): string {
        var parameters: string[];
        var untrimmedParameters: string[];
        var objId: number;
        var numParameters = 0;
        var pos = 1;
        if (parameter != "") {
            parameter = parameter + ";";
            do {
                numParameters = numParameters + 1;
                var scp = InStr(pos, parameter, ";");
                // UNKNOWN ReDimPreserveStatement
                // UNKNOWN ReDimPreserveStatement
                untrimmedParameters[numParameters] = Mid(parameter, pos, scp - pos);
                parameters[numParameters] = Trim(untrimmedParameters[numParameters]);
                pos = scp + 1;
            } while (!(pos >= Len(parameter)));
            parameter = Left(parameter, Len(parameter) - 1);
        } else {
            numParameters = 1;
            parameters = [];
            untrimmedParameters = [];
            parameters[1] = "";
            untrimmedParameters[1] = "";
        }
        var param2: string;
        var param3: string;
        if (name == "displayname") {
            objId = this.GetObjectId(parameters[1], ctx);
            if (objId == -1) {
                this.LogASLError("Object '" + parameters[1] + "' does not exist", LogType.WarningError);
                return "!";
            } else {
                return this._objs[this.GetObjectId(parameters[1], ctx)].ObjectAlias;
            }
        } else if (name == "numberparameters") {
            return Trim(Str(ctx.NumParameters));
        } else if (name == "parameter") {
            if (numParameters == 0) {
                this.LogASLError("No parameter number specified for $parameter$ function", LogType.WarningError);
                return "";
            } else {
                if (Val(parameters[1]) > ctx.NumParameters) {
                    this.LogASLError("No parameter number " + parameters[1] + " sent to this function", LogType.WarningError);
                    return "";
                } else {
                    return Trim(ctx.Parameters[parseInt(parameters[1])]);
                }
            }
        } else if (name == "gettag") {
            return this.FindStatement(this.DefineBlockParam("room", parameters[1]), parameters[2]);
        } else if (name == "objectname") {
            return this._objs[ctx.CallingObjectId].ObjectName;
        } else if (name == "locationof") {
            // UNKNOWN ForBlock
            // UNKNOWN ForBlock
        } else if (name == "lengthof") {
            return Str(Len(untrimmedParameters[1]));
        } else if (name == "left") {
            if (Val(parameters[2]) < 0) {
                this.LogASLError("Invalid function call in '$Left$(" + parameters[1] + "; " + parameters[2] + ")$'", LogType.WarningError);
                return "!";
            } else {
                return Left(parameters[1], parseInt(parameters[2]));
            }
        } else if (name == "right") {
            if (Val(parameters[2]) < 0) {
                this.LogASLError("Invalid function call in '$Right$(" + parameters[1] + "; " + parameters[2] + ")$'", LogType.WarningError);
                return "!";
            } else {
                return Right(parameters[1], parseInt(parameters[2]));
            }
        } else if (name == "mid") {
            if (numParameters == 3) {
                if (Val(parameters[2]) < 0) {
                    this.LogASLError("Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + "; " + parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                } else {
                    return Mid(parameters[1], parseInt(parameters[2]), parseInt(parameters[3]));
                }
            } else if (numParameters == 2) {
                if (Val(parameters[2]) < 0) {
                    this.LogASLError("Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + ")$'", LogType.WarningError);
                    return "!";
                } else {
                    return Mid(parameters[1], parseInt(parameters[2]));
                }
            }
            this.LogASLError("Invalid function call to '$Mid$(...)$'", LogType.WarningError);
            return "";
        } else if (name == "rand") {
            return Str(Int(this._random.NextDouble() * (parseFloat(parameters[2]) - parseFloat(parameters[1]) + 1)) + parseFloat(parameters[1]));
        } else if (name == "instr") {
            if (numParameters == 3) {
                param3 = "";
                if (InStr(parameters[3], "_") != 0) {
                    // UNKNOWN ForBlock
                } else {
                    param3 = parameters[3];
                }
                if (Val(parameters[1]) <= 0) {
                    this.LogASLError("Invalid function call in '$instr(" + parameters[1] + "; " + parameters[2] + "; " + parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                } else {
                    return Trim(Str(InStr(parseInt(parameters[1]), parameters[2], param3)));
                }
            } else if (numParameters == 2) {
                param2 = "";
                if (InStr(parameters[2], "_") != 0) {
                    // UNKNOWN ForBlock
                } else {
                    param2 = parameters[2];
                }
                return Trim(Str(InStr(parameters[1], param2)));
            }
            this.LogASLError("Invalid function call to '$Instr$(...)$'", LogType.WarningError);
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
            // UNKNOWN ForBlock
            this.LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        } else if (name == "timerinterval") {
            // UNKNOWN ForBlock
            this.LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        } else if (name == "ubound") {
            // UNKNOWN ForBlock
            // UNKNOWN ForBlock
            this.LogASLError("No such variable '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        } else if (name == "objectproperty") {
            var FoundObj = false;
            // UNKNOWN ForBlock
            if (!FoundObj) {
                this.LogASLError("No such object '" + parameters[1] + "'", LogType.WarningError);
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
                this.LogASLError("No object found with display name '" + parameters[1] + "'", LogType.WarningError);
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
    }
    ExecFor(line: string, ctx: Context): void {
        if (this.BeginsWith(line, "each ")) {
            this.ExecForEach(this.GetEverythingAfter(line, "each "), ctx);
            // UNKNOWN ExitSubStatement
        }
        var endValue: number;
        var stepValue: number;
        var forData = this.GetParameter(line, ctx);
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
        // UNKNOWN ForBlock
    }
    ExecSetVar(varInfo: string, ctx: Context): void {
        var scp = InStr(varInfo, ";");
        var varName = Trim(Left(varInfo, scp - 1));
        var varCont = Trim(Mid(varInfo, scp + 1));
        var idx = this.GetArrayIndex(varName, ctx);
        if (IsNumeric(idx.Name)) {
            this.LogASLError("Invalid numeric variable name '" + idx.Name + "' - variable names cannot be numeric", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        // UNKNOWN TryBlock
    }
    ExecuteIfAsk(question: string): boolean {
        this._player.ShowQuestion(question);
        this.ChangeState(State.Waiting);
        // UNKNOWN SyncLockBlock
        return this._questionResponse;
    }
    SetQuestionResponse(response: boolean): void {
        var runnerThread: any = {};
        this.ChangeState(State.Working);
        runnerThread.Start(response);
        this.WaitForStateChange(State.Working);
    }
    SetQuestionResponseInNewThread(response: Object): void {
        this._questionResponse = response;
        // UNKNOWN SyncLockBlock
    }
    ExecuteIfGot(item: string): boolean {
        if (this._gameAslVersion >= 280) {
            // UNKNOWN ForBlock
            this.LogASLError("No object '" + item + "' defined.", LogType.WarningError);
            return false;
        }
        // UNKNOWN ForBlock
        this.LogASLError("Item '" + item + "' not defined.", LogType.WarningError);
        return false;
    }
    ExecuteIfHas(condition: string): boolean {
        var checkValue: number;
        var colNum: number;
        var scp = InStr(condition, ";");
        var name = Trim(Left(condition, scp - 1));
        var newVal = Trim(Right(condition, Len(condition) - scp));
        var found = false;
        // UNKNOWN ForBlock
        if (!found) {
            this.LogASLError("No such collectable in " + condition, LogType.WarningError);
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
    }
    ExecuteIfIs(condition: string): boolean {
        var value1: string;
        var value2: string;
        var op: string;
        var expectNumerics: boolean;
        var expResult: ExpressionResult;
        var scp = InStr(condition, ";");
        if (scp == 0) {
            this.LogASLError("Expected second parameter in 'is " + condition + "'", LogType.WarningError);
            return false;
        }
        var scp2 = InStr(scp + 1, condition, ";");
        if (scp2 == 0) {
            op = "=";
            value1 = Trim(Left(condition, scp - 1));
            value2 = Trim(Mid(condition, scp + 1));
        } else {
            value1 = Trim(Left(condition, scp - 1));
            op = Trim(Mid(condition, scp + 1, (scp2 - scp) - 1));
            value2 = Trim(Mid(condition, scp2 + 1));
        }
        if (this._gameAslVersion >= 391) {
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
        // UNKNOWN SelectBlock
        if (expectNumerics) {
            if (!(IsNumeric(value1) && IsNumeric(value2))) {
                this.LogASLError("Expected numeric comparison comparing '" + value1 + "' and '" + value2 + "'", LogType.WarningError);
            }
        }
        return result;
    }
    GetNumericContents(name: string, ctx: Context, noError: boolean = false): number {
        var numNumber: number;
        var arrayIndex: number;
        var exists = false;
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
            // UNKNOWN ForBlock
        }
        if (!exists) {
            // UNKNOWN SingleLineIfStatement
            return -32767;
        }
        if (arrayIndex > this._numericVariable[numNumber].VariableUBound) {
            // UNKNOWN SingleLineIfStatement
            return -32766;
        }
        return Val(this._numericVariable[numNumber].VariableContents[arrayIndex]);
    }
    PlayerErrorMessage(e: PlayerError, ctx: Context): void {
        this.Print(this.GetErrorMessage(e, ctx), ctx);
    }
    PlayerErrorMessage_ExtendInfo(e: PlayerError, ctx: Context, extraInfo: string): void {
        var message = this.GetErrorMessage(e, ctx);
        if (extraInfo != "") {
            if (Right(message, 1) == ".") {
                message = Left(message, Len(message) - 1);
            }
            message = message + " - " + extraInfo + ".";
        }
        this.Print(message, ctx);
    }
    GetErrorMessage(e: PlayerError, ctx: Context): string {
        return this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(this._playerErrorMessageString[e], "%", ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx), "#", ConvertType.Strings, ctx);
    }
    PlayMedia(filename: string): void {
        this.PlayMedia(filename, false, false);
    }
    PlayMedia(filename: string, sync: boolean, looped: boolean): void {
        if (filename.Length == 0) {
            this._player.StopSound();
        } else {
            // UNKNOWN SingleLineIfStatement
            this._player.PlaySound(filename, sync, looped);
            if (sync) {
                this.ChangeState(State.Waiting);
            }
            if (sync) {
                // UNKNOWN SyncLockBlock
            }
        }
    }
    PlayWav(parameter: string): void {
        var sync: boolean = false;
        var looped: boolean = false;
        var params: any = {};
        params = new any();
        var filename = params[0];
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        if (filename.Length > 0 && InStr(filename, ".") == 0) {
            filename = filename + ".wav";
        }
        this.PlayMedia(filename, sync, looped);
    }
    RestoreGameData(fileData: string): void {
        var appliesTo: string;
        var data: string = "";
        var objId: number;
        var timerNum: number;
        var varUbound: number;
        var found: boolean;
        var numStoredData: number;
        var storedData: ChangeType[] = [];
        var decryptedFile: any = {};
        // UNKNOWN ForBlock
        this._fileData = decryptedFile.ToString();
        this._currentRoom = this.GetNextChunk();
        var numData = parseInt(this.GetNextChunk());
        var createdObjects: any = {};
        // UNKNOWN ForBlock
        numData = parseInt(this.GetNextChunk());
        // UNKNOWN ForBlock
        numData = parseInt(this.GetNextChunk());
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        numData = parseInt(this.GetNextChunk());
        // UNKNOWN ForBlock
        this._gameIsRestoring = true;
        numData = parseInt(this.GetNextChunk());
        // UNKNOWN ForBlock
        numData = parseInt(this.GetNextChunk());
        // UNKNOWN ForBlock
        this._gameIsRestoring = false;
    }
    SetBackground(col: string): void {
        this._player.SetBackground("#" + this.GetHTMLColour(col, "white"));
    }
    SetForeground(col: string): void {
        this._player.SetForeground("#" + this.GetHTMLColour(col, "black"));
    }
    SetDefaultPlayerErrorMessages(): void {
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
    }
    SetFont(name: string): void {
        // UNKNOWN SingleLineIfStatement
        this._player.SetFont(name);
    }
    SetFontSize(size: number): void {
        // UNKNOWN SingleLineIfStatement
        this._player.SetFontSize((size).toString());
    }
    SetNumericVariableContents(name: string, content: number, ctx: Context, arrayIndex: number = 0): void {
        var numNumber: number;
        var exists = false;
        if (IsNumeric(name)) {
            this.LogASLError("Illegal numeric variable name '" + name + "' - check you didn't put % around the variable name in the ASL code", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        if (this._numberNumericVariables > 0) {
            // UNKNOWN ForBlock
        }
        if (exists == false) {
            this._numberNumericVariables = this._numberNumericVariables + 1;
            numNumber = this._numberNumericVariables;
            // UNKNOWN ReDimPreserveStatement
            this._numericVariable[numNumber] = new VariableType();
            this._numericVariable[numNumber].VariableUBound = arrayIndex;
        }
        if (arrayIndex > this._numericVariable[numNumber].VariableUBound) {
            // UNKNOWN ReDimPreserveStatement
            this._numericVariable[numNumber].VariableUBound = arrayIndex;
        }
        this._numericVariable[numNumber].VariableName = name;
        // UNKNOWN ReDimPreserveStatement
        this._numericVariable[numNumber].VariableContents[arrayIndex] = (content).toString();
        if (this._numericVariable[numNumber].OnChangeScript != "" && !this._gameIsRestoring) {
            var script = this._numericVariable[numNumber].OnChangeScript;
            this.ExecuteScript(script, ctx);
        }
        if (this._numericVariable[numNumber].DisplayString != "") {
            this.UpdateStatusVars(ctx);
        }
    }
    SetOpenClose(name: string, open: boolean, ctx: Context): void {
        var cmd: string;
        if (open) {
            cmd = "open";
        } else {
            cmd = "close";
        }
        var id = this.GetObjectIdNoAlias(name);
        if (id == 0) {
            this.LogASLError("Invalid object name specified in '" + cmd + " <" + name + ">", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        this.DoOpenClose(id, open, false, ctx);
    }
    SetTimerState(name: string, state: boolean): void {
        // UNKNOWN ForBlock
        this.LogASLError("No such timer '" + name + "'", LogType.WarningError);
    }
    SetUnknownVariableType(variableData: string, ctx: Context): SetResult {
        var scp = InStr(variableData, ";");
        if (scp == 0) {
            return SetResult.Error;
        }
        var name = Trim(Left(variableData, scp - 1));
        if (InStr(name, "[") != 0 && InStr(name, "]") != 0) {
            var pos = InStr(name, "[");
            name = Left(name, pos - 1);
        }
        var contents = Trim(Mid(variableData, scp + 1));
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        return SetResult.Unfound;
    }
    SetUpChoiceForm(blockName: string, ctx: Context): string {
        var block = this.DefineBlockParam("selection", blockName);
        var prompt = this.FindStatement(block, "info");
        var menuOptions: any = {};
        var menuScript: any = {};
        // UNKNOWN ForBlock
        this.Print("- |i" + prompt + "|xi", ctx);
        var mnu: MenuData = new MenuData();
        var choice: string = this.ShowMenu(mnu);
        this.Print("- " + menuOptions[choice] + "|n", ctx);
        return menuScript[choice];
    }
    SetUpDefaultFonts(): void {
        var gameblock = this.GetDefineBlock("game");
        this._defaultFontName = "Arial";
        this._defaultFontSize = 9;
        // UNKNOWN ForBlock
    }
    SetUpDisplayVariables(): void {
        // UNKNOWN ForBlock
    }
    SetUpGameObject(): void {
        this._numberObjs = 1;
        this._objs = [];
        this._objs[1] = new ObjectType();
        var o = this._objs[1];
        o.ObjectName = "game";
        o.ObjectAlias = "";
        o.Visible = false;
        o.Exists = true;
        var nestBlock = 0;
        // UNKNOWN ForBlock
    }
    SetUpMenus(): void {
        var exists: boolean = false;
        var menuTitle: string = "";
        var menuOptions: any = {};
        // UNKNOWN ForBlock
        if (exists) {
            var windowMenu: MenuData = new MenuData();
            this._player.SetWindowMenu(windowMenu);
        }
    }
    SetUpOptions(): void {
        var opt: string;
        // UNKNOWN ForBlock
    }
    SetUpRoomData(): void {
        var defaultProperties: PropertiesActions = new PropertiesActions();
        var defaultExists = false;
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
    }
    SetUpSynonyms(): void {
        var block = this.GetDefineBlock("synonyms");
        this._numberSynonyms = 0;
        if (block.StartLine == 0 && block.EndLine == 0) {
            // UNKNOWN ExitSubStatement
        }
        // UNKNOWN ForBlock
    }
    SetUpTimers(): void {
        // UNKNOWN ForBlock
    }
    SetUpTurnScript(): void {
        var block = this.GetDefineBlock("game");
        this._beforeTurnScript = "";
        this._afterTurnScript = "";
        // UNKNOWN ForBlock
    }
    SetUpUserDefinedPlayerErrors(): void {
        var block = this.GetDefineBlock("game");
        var examineIsCustomised = false;
        // UNKNOWN ForBlock
    }
    SetVisibility(thing: string, type: Thing, visible: boolean, ctx: Context): void {
        if (this._gameAslVersion >= 280) {
            var found = false;
            // UNKNOWN ForBlock
            if (!found) {
                this.LogASLError("Not found object '" + thing + "'", LogType.WarningError);
            }
        } else {
            var atPos = InStr(thing, "@");
            var room: string;
            var name: string;
            if (atPos == 0) {
                room = this._currentRoom;
                name = thing;
            } else {
                name = Trim(Left(thing, atPos - 1));
                room = Trim(Right(thing, Len(thing) - atPos));
            }
            if (type == LegacyGame.Thing.Character) {
                // UNKNOWN ForBlock
            } else if (type == LegacyGame.Thing.Object) {
                // UNKNOWN ForBlock
            }
        }
        this.UpdateObjectList(ctx);
    }
    ShowPictureInText(filename: string): void {
        if (!this._useStaticFrameForPictures) {
            this._player.ShowPicture(filename);
        } else {
            this._player.SetPanelContents("<img src=\"" + this._player.GetURL(filename) + "\" onload=\"setPanelHeight()\"/>");
        }
    }
    ShowRoomInfoV2(room: string): void {
        var roomDisplayText: string = "";
        var descTagExist: boolean;
        var gameBlock: DefineBlock;
        var charsViewable: string;
        var charsFound: number;
        var prefixAliasNoFormat: string;
        var prefix: string;
        var prefixAlias: string;
        var inDesc: string;
        var aliasName: string = "";
        var charList: string;
        var foundLastComma: number;
        var cp: number;
        var ncp: number;
        var noFormatObjsViewable: string;
        var objsViewable: string = "";
        var objsFound: number;
        var objListString: string;
        var noFormatObjListString: string;
        var possDir: string;
        var nsew: string;
        var doorways: string;
        var places: string;
        var place: string;
        var aliasOut: string = "";
        var placeNoFormat: string;
        var descLine: string = "";
        var lastComma: number;
        var oldLastComma: number;
        var defineBlock: number;
        var lookString: string = "";
        gameBlock = this.GetDefineBlock("game");
        this._currentRoom = room;
        var roomBlock: DefineBlock;
        roomBlock = this.DefineBlockParam("room", room);
        var finishedFindingCommas: boolean;
        charsViewable = "";
        charsFound = 0;
        // UNKNOWN ForBlock
        // UNKNOWN SingleLineIfStatement
        prefix = this.FindStatement(roomBlock, "prefix");
        if (prefix == "") {
            prefixAlias = "|cr" + aliasName + "|cb";
            prefixAliasNoFormat = aliasName;
        } else {
            prefixAlias = prefix + " |cr" + aliasName + "|cb";
            prefixAliasNoFormat = prefix + " " + aliasName;
        }
        inDesc = "unfound";
        // UNKNOWN ForBlock
        if (inDesc != "unfound") {
            if (Right(inDesc, 1) == ":") {
                roomDisplayText = roomDisplayText + Left(inDesc, Len(inDesc) - 1) + " " + prefixAlias + "." + vbCrLf;
            } else {
                roomDisplayText = roomDisplayText + inDesc + vbCrLf;
            }
        } else {
            roomDisplayText = roomDisplayText + "You are in " + prefixAlias + "." + vbCrLf;
        }
        this._player.LocationUpdated(prefixAliasNoFormat);
        this.SetStringContents("quest.formatroom", prefixAliasNoFormat, this._nullContext);
        // UNKNOWN ForBlock
        if (charsFound == 0) {
            charsViewable = "There is nobody here.";
            this.SetStringContents("quest.characters", "", this._nullContext);
        } else {
            charList = Left(charsViewable, Len(charsViewable) - 2);
            this.SetStringContents("quest.characters", charList, this._nullContext);
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
                } while (!(foundLastComma == 1));
                charList = Trim(Left(charList, cp - 1)) + " and " + Trim(Mid(charList, cp + 1));
            }
            charsViewable = "You can see " + charList + " here.";
        }
        roomDisplayText = roomDisplayText + charsViewable + vbCrLf;
        noFormatObjsViewable = "";
        // UNKNOWN ForBlock
        var finishedLoop: boolean;
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
                } while (!(finishedLoop));
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
        doorways = "";
        nsew = "";
        places = "";
        possDir = "";
        // UNKNOWN ForBlock
        var outside: DefineBlock;
        if (doorways != "") {
            outside = this.DefineBlockParam("room", doorways);
            // UNKNOWN ForBlock
            // UNKNOWN SingleLineIfStatement
            roomDisplayText = roomDisplayText + "You can go out to " + aliasOut + "." + vbCrLf;
            possDir = possDir + "o";
            this.SetStringContents("quest.doorways.out", aliasOut, this._nullContext);
        } else {
            this.SetStringContents("quest.doorways.out", "", this._nullContext);
        }
        var finished: boolean;
        if (nsew != "") {
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
                } while (!(finished));
                nsew = Trim(Left(nsew, cp - 1)) + " or " + Trim(Mid(nsew, cp + 1));
            }
            roomDisplayText = roomDisplayText + "You can go " + nsew + "." + vbCrLf;
            this.SetStringContents("quest.doorways.dirs", nsew, this._nullContext);
        } else {
            this.SetStringContents("quest.doorways.dirs", "", this._nullContext);
        }
        this.UpdateDirButtons(possDir, this._nullContext);
        if (places != "") {
            places = Left(places, Len(places) - 2);
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
                } while (!(finishedFindingCommas));
                places = Left(places, lastComma) + " or" + Right(places, Len(places) - lastComma);
            }
            roomDisplayText = roomDisplayText + "You can go to " + places + "." + vbCrLf;
            this.SetStringContents("quest.doorways.places", places, this._nullContext);
        } else {
            this.SetStringContents("quest.doorways.places", "", this._nullContext);
        }
        descTagExist = false;
        // UNKNOWN ForBlock
        if (descTagExist == false) {
            // UNKNOWN ForBlock
        }
        if (descTagExist == false) {
            roomDisplayText = Left(roomDisplayText, Len(roomDisplayText) - 2);
            this.Print(roomDisplayText, this._nullContext);
        } else {
            descLine = this.GetEverythingAfter(Trim(descLine), "description ");
            if (Left(descLine, 1) == "<") {
                this.Print(this.GetParameter(descLine, this._nullContext), this._nullContext);
            } else {
                this.ExecuteScript(descLine, this._nullContext);
            }
        }
        this.UpdateObjectList(this._nullContext);
        defineBlock = 0;
        // UNKNOWN ForBlock
        // UNKNOWN SingleLineIfStatement
    }
    Speak(text: string): void {
        this._player.Speak(text);
    }
    AddToObjectList(objList: any, exitList: any, name: string, type: Thing): void {
        name = this.CapFirst(name);
        if (type == Thing.Room) {
            objList.Add(new ListData());
            exitList.Add(new ListData());
        } else {
            objList.Add(new ListData());
        }
    }
    ExecExec(scriptLine: string, ctx: Context): void {
        // UNKNOWN SingleLineIfStatement
        var execLine = this.GetParameter(scriptLine, ctx);
        var newCtx: Context = this.CopyContext(ctx);
        newCtx.StackCounter = newCtx.StackCounter + 1;
        if (newCtx.StackCounter > 500) {
            this.LogASLError("Out of stack space running '" + scriptLine + "' - infinite loop?", LogType.WarningError);
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
                this.LogASLError("Unrecognised post-command parameter in " + Trim(scriptLine), LogType.WarningError);
            }
        }
    }
    ExecSetString(info: string, ctx: Context): void {
        var scp = InStr(info, ";");
        var name = Trim(Left(info, scp - 1));
        var value = Mid(info, scp + 1);
        if (IsNumeric(name)) {
            this.LogASLError("Invalid string name '" + name + "' - string names cannot be numeric", LogType.WarningError);
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
    }
    ExecUserCommand(cmd: string, ctx: Context, libCommands: boolean = false): boolean {
        var curCmd: string;
        var commandList: string;
        var script: string = "";
        var commandTag: string;
        var commandLine: string = "";
        var foundCommand = false;
        var roomId = this.GetRoomID(this._currentRoom, ctx);
        if (roomId != 0) {
            var r = this._rooms[roomId];
            // UNKNOWN ForBlock
        }
        if (!libCommands) {
            commandTag = "command";
        } else {
            commandTag = "lib command";
        }
        if (!foundCommand) {
            var block = this.GetDefineBlock("game");
            // UNKNOWN ForBlock
        }
        if (foundCommand) {
            if (this.GetCommandParameters(cmd, commandLine, ctx)) {
                this.ExecuteScript(script, ctx);
            }
        }
        return foundCommand;
    }
    ExecuteChoose(section: string, ctx: Context): void {
        this.ExecuteScript(this.SetUpChoiceForm(section, ctx), ctx);
    }
    GetCommandParameters(test: string, required: string, ctx: Context): boolean {
        var chunksBegin: number[];
        var chunksEnd: number[];
        var varName: string[];
        var var2Pos: number;
        test = "�" + Trim(test) + "�";
        required = "�" + required + "�";
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
            // UNKNOWN ReDimPreserveStatement
            // UNKNOWN ReDimPreserveStatement
            // UNKNOWN ReDimPreserveStatement
            chunksBegin[numberChunks] = chunkBegin;
            chunksEnd[numberChunks] = chunkEnd;
            varName[numberChunks] = currentVariable;
            currentReqLinePos = var2Pos + 1;
            currentTestLinePos = chunkEnd;
        } while (!(finished));
        var success = true;
        // UNKNOWN ForBlock
        return success;
    }
    GetGender(character: string, capitalise: boolean, ctx: Context): string {
        var result: string;
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
        // UNKNOWN SingleLineIfStatement
        return result;
    }
    GetStringContents(name: string, ctx: Context): string {
        var returnAlias = false;
        var arrayIndex = 0;
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
                    this.LogASLError("Array index in '" + name + "' is not valid. An array index must be either a number or a numeric variable (without surrounding '%' characters)", LogType.WarningError);
                    return "";
                }
            }
            name = Left(name, bp - 1);
        }
        var exists = false;
        var id: number;
        if (this._numberStringVariables > 0) {
            // UNKNOWN ForBlock
        }
        if (!exists) {
            this.LogASLError("No string variable '" + name + "' defined.", LogType.WarningError);
            return "";
        }
        if (arrayIndex > this._stringVariable[id].VariableUBound) {
            this.LogASLError("Array index of '" + name + "[" + Trim(Str(arrayIndex)) + "]' too big.", LogType.WarningError);
            return "";
        }
        if (!returnAlias) {
            return this._stringVariable[id].VariableContents[arrayIndex];
        } else {
            return this._objs[this.GetObjectIdNoAlias(this._stringVariable[id].VariableContents[arrayIndex])].ObjectAlias;
        }
    }
    IsAvailable(thingName: string, type: Thing, ctx: Context): boolean {
        var room: string;
        var name: string;
        var atPos = InStr(thingName, "@");
        if (atPos == 0) {
            room = this._currentRoom;
            name = thingName;
        } else {
            name = Trim(Left(thingName, atPos - 1));
            room = Trim(Right(thingName, Len(thingName) - atPos));
        }
        if (type == Thing.Character) {
            // UNKNOWN ForBlock
        } else if (type == Thing.Object) {
            // UNKNOWN ForBlock
        }
    }
    IsCompatible(test: string, required: string): boolean {
        var var2Pos: number;
        test = "^" + Trim(test) + "^";
        required = "^" + required + "^";
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
            currentReqLinePos = var2Pos + 1;
        } while (!(finished));
        return true;
    }
    OpenGame(filename: string): boolean {
        var cdatb: boolean;
        var result: boolean;
        var visible: boolean;
        var room: string;
        var fileData: string = "";
        var savedQsgVersion: string;
        var data: string = "";
        var name: string;
        var scp: number;
        var cdat: number;
        var scp2: number;
        var scp3: number;
        var lines: string[] = null;
        this._gameLoadMethod = "loaded";
        var prevQsgVersion = false;
        if (this._data == null) {
            fileData = System.IO.File.ReadAllText(filename, System.Text.Encoding.GetEncoding(1252));
        } else {
            fileData = System.Text.Encoding.GetEncoding(1252).GetString(this._data.Data);
        }
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
            // UNKNOWN SingleLineIfStatement
        }
        result = this.InitialiseGame(this._gameFileName, true);
        if (result == false) {
            return false;
        }
        if (!prevQsgVersion) {
            this._gameLoading = true;
            this.RestoreGameData(fileData);
            this._gameLoading = false;
        } else {
            this._currentRoom = lines[3];
            var lineNumber: number = 5;
            do {
                data = lines[lineNumber];
                // UNKNOWN AddAssignmentStatement
                if (data != "!i") {
                    scp = InStr(data, ";");
                    name = Trim(Left(data, scp - 1));
                    cdat = parseInt(Right(data, Len(data) - scp));
                    // UNKNOWN ForBlock
                }
            } while (!(data == "!i"));
            do {
                data = lines[lineNumber];
                // UNKNOWN AddAssignmentStatement
                if (data != "!o") {
                    scp = InStr(data, ";");
                    name = Trim(Left(data, scp - 1));
                    cdatb = this.IsYes(Right(data, Len(data) - scp));
                    // UNKNOWN ForBlock
                }
            } while (!(data == "!o"));
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
                    // UNKNOWN ForBlock
                }
            } while (!(data == "!p"));
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
                    // UNKNOWN ForBlock
                }
            } while (!(data == "!s"));
            do {
                data = lines[lineNumber];
                // UNKNOWN AddAssignmentStatement
                if (data != "!n") {
                    scp = InStr(data, ";");
                    name = Trim(Left(data, scp - 1));
                    data = Right(data, Len(data) - scp);
                    this.SetStringContents(name, data, this._nullContext);
                }
            } while (!(data == "!n"));
            do {
                data = lines[lineNumber];
                // UNKNOWN AddAssignmentStatement
                if (data != "!e") {
                    scp = InStr(data, ";");
                    name = Trim(Left(data, scp - 1));
                    data = Right(data, Len(data) - scp);
                    this.SetNumericVariableContents(name, Val(data), this._nullContext);
                }
            } while (!(data == "!e"));
        }
        this._saveGameFile = filename;
        return true;
    }
    SaveGame(filename: string, saveFile: boolean = true): number[] {
        var ctx: Context = new Context();
        var saveData: string;
        // UNKNOWN SingleLineIfStatement
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
    }
    MakeRestoreDataV2(): string {
        var lines: any = {};
        var i: number;
        lines.Add("QUEST200.1");
        lines.Add(this.GetOriginalFilenameForQSG);
        lines.Add(this._gameName);
        lines.Add(this._currentRoom);
        lines.Add("!c");
        // UNKNOWN ForBlock
        lines.Add("!i");
        // UNKNOWN ForBlock
        lines.Add("!o");
        // UNKNOWN ForBlock
        lines.Add("!p");
        // UNKNOWN ForBlock
        lines.Add("!s");
        // UNKNOWN ForBlock
        lines.Add("!n");
        // UNKNOWN ForBlock
        lines.Add("!e");
        return String.Join(vbCrLf, lines);
    }
    SetAvailability(thingString: string, exists: boolean, ctx: Context, type: Thing = Thing.Object): void {
        if (this._gameAslVersion >= 281) {
            var found = false;
            // UNKNOWN ForBlock
            if (!found) {
                this.LogASLError("Not found object '" + thingString + "'", LogType.WarningError);
            }
        } else {
            var room: string;
            var name: string;
            var atPos = InStr(thingString, "@");
            if (atPos == 0) {
                room = this._currentRoom;
                name = thingString;
            } else {
                name = Trim(Left(thingString, atPos - 1));
                room = Trim(Right(thingString, Len(thingString) - atPos));
            }
            if (type == Thing.Character) {
                // UNKNOWN ForBlock
            } else if (type == Thing.Object) {
                // UNKNOWN ForBlock
            }
        }
        this.UpdateItems(ctx);
        this.UpdateObjectList(ctx);
    }
    SetStringContents(name: string, value: string, ctx: Context, arrayIndex: number = 0): void {
        var id: number;
        var exists = false;
        if (name == "") {
            this.LogASLError("Internal error - tried to set empty string name to '" + value + "'", LogType.WarningError);
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
            this.LogASLError("'" + name + "[" + Trim(Str(arrayIndex)) + "]' is invalid - did not assign to array", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        if (this._numberStringVariables > 0) {
            // UNKNOWN ForBlock
        }
        if (!exists) {
            this._numberStringVariables = this._numberStringVariables + 1;
            id = this._numberStringVariables;
            // UNKNOWN ReDimPreserveStatement
            this._stringVariable[id] = new VariableType();
            this._stringVariable[id].VariableUBound = arrayIndex;
        }
        if (arrayIndex > this._stringVariable[id].VariableUBound) {
            // UNKNOWN ReDimPreserveStatement
            this._stringVariable[id].VariableUBound = arrayIndex;
        }
        this._stringVariable[id].VariableName = name;
        // UNKNOWN ReDimPreserveStatement
        this._stringVariable[id].VariableContents[arrayIndex] = value;
        if (this._stringVariable[id].OnChangeScript != "") {
            var script = this._stringVariable[id].OnChangeScript;
            this.ExecuteScript(script, ctx);
        }
        if (this._stringVariable[id].DisplayString != "") {
            this.UpdateStatusVars(ctx);
        }
    }
    SetUpCharObjectInfo(): void {
        var defaultProperties: PropertiesActions = new PropertiesActions();
        this._numberChars = 0;
        var defaultExists = false;
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        this.UpdateVisibilityInContainers(this._nullContext);
    }
    ShowGameAbout(ctx: Context): void {
        var version = this.FindStatement(this.GetDefineBlock("game"), "game version");
        var author = this.FindStatement(this.GetDefineBlock("game"), "game author");
        var copyright = this.FindStatement(this.GetDefineBlock("game"), "game copyright");
        var info = this.FindStatement(this.GetDefineBlock("game"), "game info");
        this.Print("|bGame name:|cl  " + this._gameName + "|cb|xb", ctx);
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        if (info != "") {
            this.Print("", ctx);
            this.Print(info, ctx);
        }
    }
    ShowPicture(filename: string): void {
        var caption: string = "";
        if (InStr(filename, ";") != 0) {
            caption = Trim(Mid(filename, InStr(filename, ";") + 1));
            filename = Trim(Left(filename, InStr(filename, ";") - 1));
        }
        if (InStr(filename, "@") != 0) {
            filename = Trim(Left(filename, InStr(filename, "@") - 1));
        }
        // UNKNOWN SingleLineIfStatement
        this.ShowPictureInText(filename);
    }
    ShowRoomInfo(room: string, ctx: Context, noPrint: boolean = false): void {
        if (this._gameAslVersion < 280) {
            this.ShowRoomInfoV2(room);
            // UNKNOWN ExitSubStatement
        }
        var roomDisplayText: string = "";
        var descTagExist: boolean;
        var doorwayString: string;
        var roomAlias: string;
        var finishedFindingCommas: boolean;
        var prefix: string;
        var roomDisplayName: string;
        var roomDisplayNameNoFormat: string;
        var inDescription: string;
        var visibleObjects: string = "";
        var visibleObjectsNoFormat: string;
        var placeList: string;
        var lastComma: number;
        var oldLastComma: number;
        var descType: number;
        var descLine: string = "";
        var showLookText: boolean;
        var lookDesc: string = "";
        var objLook: string;
        var objSuffix: string;
        var gameBlock = this.GetDefineBlock("game");
        this._currentRoom = room;
        var id = this.GetRoomID(this._currentRoom, ctx);
        // UNKNOWN SingleLineIfStatement
        roomAlias = this._rooms[id].RoomAlias;
        // UNKNOWN SingleLineIfStatement
        prefix = this._rooms[id].Prefix;
        if (prefix == "") {
            roomDisplayName = "|cr" + roomAlias + "|cb";
            roomDisplayNameNoFormat = roomAlias;
        } else {
            roomDisplayName = prefix + " |cr" + roomAlias + "|cb";
            roomDisplayNameNoFormat = prefix + " " + roomAlias;
        }
        inDescription = this._rooms[id].InDescription;
        if (inDescription != "") {
            if (Right(inDescription, 1) == ":") {
                roomDisplayText = roomDisplayText + Left(inDescription, Len(inDescription) - 1) + " " + roomDisplayName + "." + vbCrLf;
            } else {
                roomDisplayText = roomDisplayText + inDescription + vbCrLf;
            }
        } else {
            roomDisplayText = roomDisplayText + "You are in " + roomDisplayName + "." + vbCrLf;
        }
        this._player.LocationUpdated(UCase(Left(roomAlias, 1)) + Mid(roomAlias, 2));
        this.SetStringContents("quest.formatroom", roomDisplayNameNoFormat, ctx);
        visibleObjectsNoFormat = "";
        var visibleObjectsList: any = {};
        var count: number;
        // UNKNOWN ForBlock
        // UNKNOWN ForEachBlock
        if (visibleObjectsList.Count() > 0) {
            this.SetStringContents("quest.formatobjects", visibleObjects, ctx);
            visibleObjects = "There is " + visibleObjects + " here.";
            this.SetStringContents("quest.objects", visibleObjectsNoFormat, ctx);
            roomDisplayText = roomDisplayText + visibleObjects + vbCrLf;
        } else {
            this.SetStringContents("quest.objects", "", ctx);
            this.SetStringContents("quest.formatobjects", "", ctx);
        }
        doorwayString = this.UpdateDoorways(id, ctx);
        if (this._gameAslVersion < 410) {
            placeList = this.GetGoToExits(id, ctx);
            if (placeList != "") {
                placeList = Left(placeList, Len(placeList) - 2);
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
                    } while (!(finishedFindingCommas));
                    placeList = Left(placeList, lastComma - 1) + " or" + Right(placeList, Len(placeList) - lastComma);
                }
                roomDisplayText = roomDisplayText + "You can go to " + placeList + "." + vbCrLf;
                this.SetStringContents("quest.doorways.places", placeList, ctx);
            } else {
                this.SetStringContents("quest.doorways.places", "", ctx);
            }
        }
        objLook = this.GetObjectProperty("look", this._rooms[id].ObjId, null, false);
        if (objLook == "") {
            if (this._rooms[id].Look != "") {
                lookDesc = this._rooms[id].Look;
            }
        } else {
            lookDesc = objLook;
        }
        this.SetStringContents("quest.lookdesc", lookDesc, ctx);
        showLookText = true;
        if (this._rooms[id].Description.Data != "") {
            descLine = this._rooms[id].Description.Data;
            descType = this._rooms[id].Description.Type;
            descTagExist = true;
        } else {
            descTagExist = false;
        }
        if (descTagExist == false) {
            // UNKNOWN ForBlock
        }
        if (descTagExist && this._gameAslVersion >= 310) {
            showLookText = false;
        }
        if (!noPrint) {
            if (descTagExist == false) {
                roomDisplayText = Left(roomDisplayText, Len(roomDisplayText) - 2);
                this.Print(roomDisplayText, ctx);
                // UNKNOWN SingleLineIfStatement
            } else {
                if (descType == TextActionType.Text) {
                    this.Print(descLine, ctx);
                } else {
                    this.ExecuteScript(descLine, ctx);
                }
            }
            this.UpdateObjectList(ctx);
            if (showLookText) {
                if (lookDesc != "") {
                    this.Print(lookDesc, ctx);
                }
            }
        }
    }
    CheckCollectable(id: number): void {
        var max: number;
        var value: number;
        var min: number;
        var m: number;
        var type = this._collectables[id].Type;
        value = this._collectables[id].Value;
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
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
            // UNKNOWN SingleLineIfStatement
            // UNKNOWN SingleLineIfStatement
        }
        this._collectables[id].Value = value;
    }
    DisplayCollectableInfo(id: number): string {
        var display: string;
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
    }
    DisplayTextSection(section: string, ctx: Context): void {
        var block: DefineBlock;
        block = this.DefineBlockParam("text", section);
        if (block.StartLine == 0) {
            // UNKNOWN ExitSubStatement
        }
        // UNKNOWN ForBlock
        this.Print("", ctx);
    }
    ExecCommand(input: string, ctx: Context, echo: boolean = true, runUserCommand: boolean = true, dontSetIt: boolean = false): boolean {
        var parameter: string;
        var skipAfterTurn = false;
        input = this.RemoveFormatting(input);
        var oldBadCmdBefore = this._badCmdBefore;
        var roomID = this.GetRoomID(this._currentRoom, ctx);
        var enteredHelpCommand = false;
        // UNKNOWN SingleLineIfStatement
        var cmd = LCase(input);
        // UNKNOWN SyncLockBlock
        var userCommandReturn: boolean;
        if (echo) {
            this.Print("> " + input, ctx);
        }
        input = LCase(input);
        this.SetStringContents("quest.originalcommand", input, ctx);
        var newCommand = " " + input + " ";
        // UNKNOWN ForBlock
        input = Mid(newCommand, 2, Len(newCommand) - 2);
        this.SetStringContents("quest.command", input, ctx);
        var newCtx: Context = this.CopyContext(ctx);
        var globalOverride = false;
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
        // UNKNOWN SingleLineIfStatement
        if (!newCtx.DontProcessCommand) {
            userCommandReturn = false;
            if (runUserCommand == true) {
                userCommandReturn = this.ExecUserCommand(input, ctx);
                if (!userCommandReturn) {
                    userCommandReturn = this.ExecVerb(input, ctx);
                }
                if (!userCommandReturn) {
                    userCommandReturn = this.ExecUserCommand(input, ctx, true);
                }
                if (!userCommandReturn) {
                    userCommandReturn = this.ExecVerb(input, ctx, true);
                }
            }
            input = LCase(input);
        } else {
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
                        this.PlayerErrorMessage(PlayerError.BadGo, ctx);
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
                // UNKNOWN ForEachBlock
            } else if (cmd == "inventory" || cmd == "inv" || cmd == "i") {
                if (this._gameAslVersion >= 280) {
                    // UNKNOWN ForBlock
                } else {
                    // UNKNOWN ForBlock
                }
                if (invList != "") {
                    invList = Left(invList, Len(invList) - 2);
                    invList = UCase(Left(invList, 1)) + Mid(invList, 2);
                    var pos = 1;
                    var lastComma: number;
                    var thisComma: number;
                    do {
                        thisComma = InStr(pos, invList, ",");
                        if (thisComma != 0) {
                            lastComma = thisComma;
                            pos = thisComma + 1;
                        }
                    } while (!(thisComma == 0));
                    // UNKNOWN SingleLineIfStatement
                    this.Print("You are carrying:|n" + invList + ".", ctx);
                } else {
                    this.Print("You are not carrying anything.", ctx);
                }
            } else if (this.CmdStartsWith(input, "oops ")) {
                this.ExecOops(this.GetEverythingAfter(input, "oops "), ctx);
            } else if (this.CmdStartsWith(input, "the ")) {
                this.ExecOops(this.GetEverythingAfter(input, "the "), ctx);
            } else {
                this.PlayerErrorMessage(PlayerError.BadCommand, ctx);
            }
        }
        if (!skipAfterTurn) {
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
            // UNKNOWN SingleLineIfStatement
        }
        this.Print("", ctx);
        if (!dontSetIt) {
            this._lastIt = this._thisTurnIt;
            this._lastItMode = this._thisTurnItMode;
        }
        // UNKNOWN SingleLineIfStatement
        return true;
    }
    CmdStartsWith(cmd: string, startsWith: string): boolean {
        return this.BeginsWith(Trim(cmd), startsWith);
    }
    ExecGive(giveString: string, ctx: Context): void {
        var article: string;
        var item: string;
        var character: string;
        var type: Thing;
        var id: number;
        var script: string = "";
        var toPos = InStr(giveString, " to ");
        if (toPos == 0) {
            toPos = InStr(giveString, " the ");
            if (toPos == 0) {
                this.PlayerErrorMessage(PlayerError.BadGive, ctx);
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
            type = Thing.Object;
        } else {
            type = Thing.Character;
        }
        if (this._gameAslVersion >= 280) {
            id = this.Disambiguate(item, "inventory", ctx);
            if (id == -1) {
                this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                this._badCmdBefore = "give";
                this._badCmdAfter = "to " + character;
                // UNKNOWN ExitSubStatement
            } else if (id == -2) {
                // UNKNOWN ExitSubStatement
            } else {
                article = this._objs[id].Article;
            }
        } else {
            var notGot = true;
            // UNKNOWN ForBlock
            if (notGot == true) {
                this.PlayerErrorMessage(PlayerError.NoItem, ctx);
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
                // UNKNOWN SingleLineIfStatement
                this._badCmdBefore = "give " + item + " to";
                // UNKNOWN ExitSubStatement
            }
            var o = this._objs[giveToId];
            // UNKNOWN ForBlock
            if (!foundScript) {
                var g = this._objs[id];
                // UNKNOWN ForBlock
            }
            if (!foundScript) {
                script = this._objs[giveToId].GiveAnything;
                if (script != "") {
                    foundScript = true;
                    this.SetStringContents("quest.give.object.name", this._objs[id].ObjectName, ctx);
                }
            }
            if (!foundScript) {
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
                this.PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
            }
        } else {
            var block = this.GetThingBlock(character, this._currentRoom, type);
            if ((block.StartLine == 0 && block.EndLine == 0) || this.IsAvailable(character + "@" + this._currentRoom, type, ctx) == false) {
                this.PlayerErrorMessage(PlayerError.BadCharacter, ctx);
                // UNKNOWN ExitSubStatement
            }
            var realName = this._chars[this.GetThingNumber(character, this._currentRoom, type)].ObjectName;
            var giveLine = 0;
            // UNKNOWN ForBlock
            if (giveLine == 0) {
                // UNKNOWN SingleLineIfStatement
                this.SetStringContents("quest.error.charactername", realName, ctx);
                this.SetStringContents("quest.error.gender", Trim(this.GetGender(character, true, ctx)), ctx);
                this.SetStringContents("quest.error.article", article, ctx);
                this.PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
                // UNKNOWN ExitSubStatement
            }
            this.ExecuteScript(this.GetSecondChunk(this._lines[giveLine]), ctx);
        }
    }
    ExecLook(lookLine: string, ctx: Context): void {
        var item: string;
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
                    // UNKNOWN SingleLineIfStatement
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
                    if (this.IsAvailable(item, Thing.Object, ctx) == false) {
                        lookLine = "<unfound>";
                    }
                }
                if (lookLine == "<unfound>") {
                    lookLine = this.RetrLine("character", item, "look", ctx);
                    if (lookLine != "<unfound>") {
                        if (this.IsAvailable(item, Thing.Character, ctx) == false) {
                            lookLine = "<unfound>";
                        }
                    }
                    if (lookLine == "<unfound>") {
                        this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                        // UNKNOWN ExitSubStatement
                    } else if (lookLine == "<undefined>") {
                        this.PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                        // UNKNOWN ExitSubStatement
                    }
                } else if (lookLine == "<undefined>") {
                    this.PlayerErrorMessage(PlayerError.DefaultLook, ctx);
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
    }
    ExecSpeak(cmd: string, ctx: Context): void {
        if (this.BeginsWith(cmd, "the ")) {
            cmd = this.GetEverythingAfter(cmd, "the ");
        }
        var name = cmd;
        if (this._gameAslVersion >= 281) {
            var speakLine: string = "";
            var ObjID = this.Disambiguate(name, "inventory;" + this._currentRoom, ctx);
            if (ObjID <= 0) {
                // UNKNOWN SingleLineIfStatement
                this._badCmdBefore = "speak to";
                // UNKNOWN ExitSubStatement
            }
            var foundSpeak = false;
            var o = this._objs[ObjID];
            // UNKNOWN ForBlock
            if (!foundSpeak) {
                // UNKNOWN ForBlock
            }
            if (this._gameAslVersion < 311 && !foundSpeak) {
                // UNKNOWN ForBlock
            }
            if (!foundSpeak) {
                this.SetStringContents("quest.error.gender", UCase(Left(this._objs[ObjID].Gender, 1)) + Mid(this._objs[ObjID].Gender, 2), ctx);
                this.PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
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
            var type = Thing.Character;
            var data = Trim(this.GetEverythingAfter(Trim(line), "speak "));
            if (line != "<unfound>" && line != "<undefined>") {
                if (this.IsAvailable(cmd + "@" + this._currentRoom, type, ctx) == false) {
                    line = "<undefined>";
                }
            }
            if (line == "<undefined>") {
                this.PlayerErrorMessage(PlayerError.BadCharacter, ctx);
            } else if (line == "<unfound>") {
                this.SetStringContents("quest.error.gender", Trim(this.GetGender(cmd, true, ctx)), ctx);
                this.SetStringContents("quest.error.charactername", cmd, ctx);
                this.PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
            } else if (this.BeginsWith(data, "<")) {
                data = this.GetParameter(line, ctx);
                this.Print(Chr(34) + data + Chr(34), ctx);
            } else {
                this.ExecuteScript(data, ctx);
            }
        }
    }
    ExecTake(item: string, ctx: Context): void {
        var parentID: number;
        var parentDisplayName: string;
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
                        this.PlayerErrorMessage(PlayerError.AlreadyTaken, ctx);
                    } else {
                        this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                } else if (this._gameAslVersion >= 391) {
                    this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                } else {
                    this.PlayerErrorMessage(PlayerError.BadItem, ctx);
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
                this.PlayerErrorMessage_ExtendInfo(PlayerError.BadTake, ctx, canAccessObject.ErrorMsg);
                // UNKNOWN ExitSubStatement
            }
            var parent = this.GetObjectProperty("parent", id, false, false);
            parentID = this.GetObjectIdNoAlias(parent);
        }
        if (this._gameAslVersion >= 280) {
            var t = this._objs[id].Take;
            if (isInContainer && (t.Type == TextActionType.Default || t.Type == TextActionType.Text)) {
                if (this._objs[parentID].ObjectAlias != "") {
                    parentDisplayName = this._objs[parentID].ObjectAlias;
                } else {
                    parentDisplayName = this._objs[parentID].ObjectName;
                }
                this.Print("(first removing " + this._objs[id].Article + " from " + parentDisplayName + ")", ctx);
                ctx.AllowRealNamesInCommand = true;
                this.ExecCommand("remove " + this._objs[id].ObjectName, ctx, false, null, true);
                if (this.GetObjectProperty("parent", id, false, false) != "") {
                    // UNKNOWN ExitSubStatement
                }
            }
            if (t.Type == TextActionType.Default) {
                this.PlayerErrorMessage(PlayerError.DefaultTake, ctx);
                this.PlayerItem(item, true, ctx, id);
            } else if (t.Type == TextActionType.Text) {
                this.Print(t.Data, ctx);
                this.PlayerItem(item, true, ctx, id);
            } else if (t.Type == TextActionType.Script) {
                this.ExecuteScript(t.Data, ctx, id);
            } else {
                this.PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        } else {
            // UNKNOWN ForBlock
            if (!foundTake) {
                this.PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        }
    }
    ExecUse(useLine: string, ctx: Context): void {
        var endOnWith: number;
        var useDeclareLine = "";
        var useOn: string;
        var useItem: string;
        useLine = Trim(this.GetEverythingAfter(useLine, "use "));
        var roomId: number;
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
        var id: number;
        var notGotItem: boolean;
        if (this._gameAslVersion >= 280) {
            var foundItem = false;
            id = this.Disambiguate(useItem, "inventory", ctx);
            // UNKNOWN SingleLineIfStatement
            if (!foundItem) {
                // UNKNOWN SingleLineIfStatement
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
            // UNKNOWN ForBlock
            if (notGotItem == true) {
                this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                // UNKNOWN ExitSubStatement
            }
        }
        var useScript: string = "";
        var foundUseScript: boolean;
        var foundUseOnObject: boolean;
        var useOnObjectId: number;
        var found: boolean;
        if (this._gameAslVersion >= 280) {
            foundUseScript = false;
            if (useOn == "") {
                if (this._gameAslVersion < 410) {
                    var r = this._rooms[roomId];
                    // UNKNOWN ForBlock
                }
                if (!foundUseScript) {
                    useScript = this._objs[id].Use;
                    // UNKNOWN SingleLineIfStatement
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
                    // UNKNOWN SingleLineIfStatement
                    this._badCmdBefore = "use " + useItem + " on";
                    // UNKNOWN ExitSubStatement
                }
                var o = this._objs[useOnObjectId];
                // UNKNOWN ForBlock
                if (!foundUseScript) {
                    var u = this._objs[id];
                    // UNKNOWN ForBlock
                }
                if (!foundUseScript) {
                    useScript = this._objs[useOnObjectId].UseAnything;
                    if (useScript != "") {
                        foundUseScript = true;
                        this.SetStringContents("quest.use.object.name", this._objs[id].ObjectName, ctx);
                    }
                }
                if (!foundUseScript) {
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
                this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
            }
        } else {
            if (useOn != "") {
                useDeclareLine = this.RetrLineParam("object", useOn, "use", useItem, ctx);
            } else {
                found = false;
                // UNKNOWN ForBlock
                if (!found) {
                    useDeclareLine = this.FindLine(this.GetDefineBlock("game"), "use", useItem);
                }
                if (!found && useDeclareLine == "") {
                    this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    // UNKNOWN ExitSubStatement
                }
            }
            if (useDeclareLine != "<unfound>" && useDeclareLine != "<undefined>" && useOn != "") {
                if (this.IsAvailable(useOn, Thing.Object, ctx) == false) {
                    useDeclareLine = "<undefined>";
                }
            }
            if (useDeclareLine == "<undefined>") {
                useDeclareLine = this.RetrLineParam("character", useOn, "use", useItem, ctx);
                if (useDeclareLine != "<undefined>") {
                    if (this.IsAvailable(useOn, Thing.Character, ctx) == false) {
                        useDeclareLine = "<undefined>";
                    }
                }
                if (useDeclareLine == "<undefined>") {
                    this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    // UNKNOWN ExitSubStatement
                } else if (useDeclareLine == "<unfound>") {
                    this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    // UNKNOWN ExitSubStatement
                }
            } else if (useDeclareLine == "<unfound>") {
                this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                // UNKNOWN ExitSubStatement
            }
            var script = Right(useDeclareLine, Len(useDeclareLine) - InStr(useDeclareLine, ">"));
            this.ExecuteScript(script, ctx);
        }
    }
    ObjectActionUpdate(id: number, name: string, script: string, noUpdate: boolean = false): void {
        var objectName: string;
        var sp: number;
        var ep: number;
        name = LCase(name);
        if (!noUpdate) {
            if (name == "take") {
                this._objs[id].Take.Data = script;
                this._objs[id].Take.Type = TextActionType.Script;
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
    }
    ExecuteIf(scriptLine: string, ctx: Context): void {
        var ifLine = Trim(this.GetEverythingAfter(Trim(scriptLine), "if "));
        var obscuredLine = this.ObliterateParameters(ifLine);
        var thenPos = InStr(obscuredLine, "then");
        if (thenPos == 0) {
            var errMsg = "Expected 'then' missing from script statement '" + this.ReportErrorLine(scriptLine) + "' - statement bypassed.";
            this.LogASLError(errMsg, LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        var conditions = Trim(Left(ifLine, thenPos - 1));
        thenPos = thenPos + 4;
        var elsePos = InStr(obscuredLine, "else");
        var thenEndPos: number;
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
        if (Left(thenScript, 1) == "{" && Right(thenScript, 1) == "}") {
            thenScript = Mid(thenScript, 2, Len(thenScript) - 2);
        }
        if (Left(elseScript, 1) == "{" && Right(elseScript, 1) == "}") {
            elseScript = Mid(elseScript, 2, Len(elseScript) - 2);
        }
        if (this.ExecuteConditions(conditions, ctx)) {
            this.ExecuteScript((thenScript), ctx);
        } else {
            // UNKNOWN SingleLineIfStatement
        }
    }
    ExecuteScript(scriptLine: string, ctx: Context, newCallingObjectId: number = 0): void {
        // UNKNOWN TryBlock
    }
    ExecuteEnter(scriptLine: string, ctx: Context): void {
        this._commandOverrideModeOn = true;
        this._commandOverrideVariable = this.GetParameter(scriptLine, ctx);
        this.ChangeState(State.Waiting, true);
        // UNKNOWN SyncLockBlock
        this._commandOverrideModeOn = false;
    }
    ExecuteSet(setInstruction: string, ctx: Context): void {
        if (this._gameAslVersion >= 280) {
            if (this.BeginsWith(setInstruction, "interval ")) {
                var interval = this.GetParameter(setInstruction, ctx);
                var scp = InStr(interval, ";");
                if (scp == 0) {
                    this.LogASLError("Too few parameters in 'set " + setInstruction + "'", LogType.WarningError);
                    // UNKNOWN ExitSubStatement
                }
                var name = Trim(Left(interval, scp - 1));
                interval = (Val(Trim(Mid(interval, scp + 1)))).toString();
                var found = false;
                // UNKNOWN ForBlock
                if (!found) {
                    this.LogASLError("No such timer '" + name + "'", LogType.WarningError);
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
                if (result == SetResult.Error) {
                    this.LogASLError("Error on setting 'set " + setInstruction + "'", LogType.WarningError);
                } else if (result == SetResult.Unfound) {
                    this.LogASLError("Variable type not specified in 'set " + setInstruction + "'", LogType.WarningError);
                }
            }
        } else {
            this.ExecuteSetCollectable(this.GetParameter(setInstruction, ctx), ctx);
        }
    }
    FindStatement(block: DefineBlock, statement: string): string {
        // UNKNOWN ForBlock
        return "";
    }
    FindLine(block: DefineBlock, statement: string, statementParam: string): string {
        // UNKNOWN ForBlock
        return "";
    }
    GetCollectableAmount(name: string): number {
        // UNKNOWN ForBlock
        return 0;
    }
    GetSecondChunk(line: string): string {
        var endOfFirstBit = InStr(line, ">") + 1;
        var lengthOfKeyword = (Len(line) - endOfFirstBit) + 1;
        return Trim(Mid(line, endOfFirstBit, lengthOfKeyword));
    }
    GoDirection(direction: string, ctx: Context): void {
        var dirData: TextAction = new TextAction();
        var id = this.GetRoomID(this._currentRoom, ctx);
        // UNKNOWN SingleLineIfStatement
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
                dirData.Type = TextActionType.Text;
            } else {
                dirData.Data = r.Out.Script;
                dirData.Type = TextActionType.Script;
            }
        }
        if (dirData.Type == TextActionType.Script && dirData.Data != "") {
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
                this.PlayerErrorMessage(PlayerError.DefaultOut, ctx);
            } else {
                this.PlayerErrorMessage(PlayerError.BadPlace, ctx);
            }
        }
    }
    GoToPlace(place: string, ctx: Context): void {
        var destination = "";
        var placeData: string;
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
            this.PlayerErrorMessage(PlayerError.BadPlace, ctx);
        }
    }
    InitialiseGame(filename: string, fromQsg: boolean = false): boolean {
        this._loadedFromQsg = fromQsg;
        this._changeLogRooms = new ChangeLog();
        this._changeLogObjects = new ChangeLog();
        this._changeLogRooms.AppliesToType = ChangeLog.AppliesTo.Room;
        this._changeLogObjects.AppliesToType = ChangeLog.AppliesTo.Object;
        this._outPutOn = true;
        this._useAbbreviations = true;
        this._gamePath = System.IO.Path.GetDirectoryName(filename) + "\\";
        this.LogASLError("Opening file " + filename + " on " + Date.Now.ToString(), LogType.Init);
        if (this.ParseFile(filename) == false) {
            this.LogASLError("Unable to open file", LogType.Init);
            var err = "Unable to open " + filename;
            if (this._openErrorReport != "") {
                this._openErrorReport = Left(this._openErrorReport, Len(this._openErrorReport) - 2);
                err = err + ":" + vbCrLf + vbCrLf + this._openErrorReport;
            }
            this.Print("Error: " + err, this._nullContext);
            return false;
        }
        var gameBlock: DefineBlock;
        gameBlock = this.GetDefineBlock("game");
        var aslVersion = "//";
        // UNKNOWN ForBlock
        if (aslVersion == "//") {
            this.LogASLError("File contains no version header.", LogType.WarningError);
        } else {
            this._gameAslVersion = parseInt(aslVersion);
            var recognisedVersions = "/100/200/210/217/280/281/282/283/284/285/300/310/311/320/350/390/391/392/400/410/";
            if (InStr(recognisedVersions, "/" + aslVersion + "/") == 0) {
                this.LogASLError("Unrecognised ASL version number.", LogType.WarningError);
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
        this._gameName = this.GetParameter(this._lines[this.GetDefineBlock("game").StartLine], this._nullContext);
        this._player.UpdateGameName(this._gameName);
        this._player.Show("Panes");
        this._player.Show("Location");
        this._player.Show("Command");
        this.SetUpGameObject();
        this.SetUpOptions();
        // UNKNOWN ForBlock
        this.SetDefaultPlayerErrorMessages();
        this.SetUpSynonyms();
        this.SetUpRoomData();
        if (this._gameAslVersion >= 410) {
            this.SetUpExits();
        }
        if (this._gameAslVersion < 280) {
            this.SetUpItemArrays();
        }
        if (this._gameAslVersion < 280) {
            this.SetUpStartItems();
        }
        this.SetUpCollectables();
        this.SetUpDisplayVariables();
        this.SetUpCharObjectInfo();
        this.SetUpUserDefinedPlayerErrors();
        this.SetUpDefaultFonts();
        this.SetUpTurnScript();
        this.SetUpTimers();
        this.SetUpMenus();
        this._gameFileName = filename;
        this.LogASLError("Finished loading file.", LogType.Init);
        this._defaultRoomProperties = this.GetPropertiesInType("defaultroom", false);
        this._defaultProperties = this.GetPropertiesInType("default", false);
        return true;
    }
    PlaceExist(placeName: string, ctx: Context): string {
        var roomId = this.GetRoomID(this._currentRoom, ctx);
        var foundPlace = false;
        var scriptPresent = false;
        var r = this._rooms[roomId];
        // UNKNOWN ForBlock
        return "";
    }
    PlayerItem(item: string, got: boolean, ctx: Context, objId: number = 0): void {
        var foundObjectName = false;
        if (this._gameAslVersion >= 280) {
            if (objId == 0) {
                // UNKNOWN ForBlock
            }
            if (objId != 0) {
                if (got) {
                    if (this._gameAslVersion >= 391) {
                        this.AddToObjectProperties("not parent", objId, ctx);
                    }
                    this.MoveThing(this._objs[objId].ObjectName, "inventory", Thing.Object, ctx);
                    if (this._objs[objId].GainScript != "") {
                        this.ExecuteScript(this._objs[objId].GainScript, ctx);
                    }
                } else {
                    this.MoveThing(this._objs[objId].ObjectName, this._currentRoom, Thing.Object, ctx);
                    if (this._objs[objId].LoseScript != "") {
                        this.ExecuteScript(this._objs[objId].LoseScript, ctx);
                    }
                }
                foundObjectName = true;
            }
            if (!foundObjectName) {
                this.LogASLError("No such object '" + item + "'", LogType.WarningError);
            } else {
                this.UpdateItems(ctx);
                this.UpdateObjectList(ctx);
            }
        } else {
            // UNKNOWN ForBlock
            this.UpdateItems(ctx);
        }
    }
    PlayGame(room: string, ctx: Context): void {
        var id = this.GetRoomID(room, ctx);
        if (id == 0) {
            this.LogASLError("No such room '" + room + "'", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        this._currentRoom = room;
        this.SetStringContents("quest.currentroom", room, ctx);
        if (this._gameAslVersion >= 391 && this._gameAslVersion < 410) {
            this.AddToObjectProperties("visited", this._rooms[id].ObjId, ctx);
        }
        this.ShowRoomInfo(room, ctx);
        this.UpdateItems(ctx);
        if (this._rooms[id].Script != "") {
            var script = this._rooms[id].Script;
            this.ExecuteScript(script, ctx);
        }
        if (this._gameAslVersion >= 410) {
            this.AddToObjectProperties("visited", this._rooms[id].ObjId, ctx);
        }
    }
    Print(txt: string, ctx: Context): void {
        var printString = "";
        if (txt == "") {
            this.DoPrint(printString);
        } else {
            // UNKNOWN ForBlock
            // UNKNOWN SingleLineIfStatement
        }
    }
    RetrLine(blockType: string, param: string, line: string, ctx: Context): string {
        var searchblock: DefineBlock;
        if (blockType == "object") {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Object);
        } else {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Character);
        }
        if (searchblock.StartLine == 0 && searchblock.EndLine == 0) {
            return "<undefined>";
        }
        // UNKNOWN ForBlock
        return "<unfound>";
    }
    RetrLineParam(blockType: string, param: string, line: string, lineParam: string, ctx: Context): string {
        var searchblock: DefineBlock;
        if (blockType == "object") {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Object);
        } else {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Character);
        }
        if (searchblock.StartLine == 0 && searchblock.EndLine == 0) {
            return "<undefined>";
        }
        // UNKNOWN ForBlock
        return "<unfound>";
    }
    SetUpCollectables(): void {
        var lastItem = false;
        this._numCollectables = 0;
        // UNKNOWN ForBlock
    }
    SetUpItemArrays(): void {
        var lastItem = false;
        this._numberItems = 0;
        // UNKNOWN ForBlock
    }
    SetUpStartItems(): void {
        var lastItem = false;
        // UNKNOWN ForBlock
    }
    ShowHelp(ctx: Context): void {
        this.Print("|b|cl|s14Quest Quick Help|xb|cb|s00", ctx);
        this.Print("", ctx);
        this.Print("|cl|bMoving|xb|cb Press the direction buttons in the 'Compass' pane, or type |bGO NORTH|xb, |bSOUTH|xb, |bE|xb, etc. |xn", ctx);
        this.Print("To go into a place, type |bGO TO ...|xb . To leave a place, type |bOUT, EXIT|xb or |bLEAVE|xb, or press the '|crOUT|cb' button.|n", ctx);
        this.Print("|cl|bObjects and Characters|xb|cb Use |bTAKE ...|xb, |bGIVE ... TO ...|xb, |bTALK|xb/|bSPEAK TO ...|xb, |bUSE ... ON|xb/|bWITH ...|xb, |bLOOK AT ...|xb, etc.|n", ctx);
        this.Print("|cl|bExit Quest|xb|cb Type |bQUIT|xb to leave Quest.|n", ctx);
        this.Print("|cl|bMisc|xb|cb Type |bABOUT|xb to get information on the current game. The next turn after referring to an object or character, you can use |bIT|xb, |bHIM|xb etc. as appropriate to refer to it/him/etc. again. If you make a mistake when typing an object's name, type |bOOPS|xb followed by your correction.|n", ctx);
        this.Print("|cl|bKeyboard shortcuts|xb|cb Press the |crup arrow|cb and |crdown arrow|cb to scroll through commands you have already typed in. Press |crEsc|cb to clear the command box.|n|n", ctx);
        this.Print("Further information is available by selecting |iQuest Documentation|xi from the |iHelp|xi menu.", ctx);
    }
    ReadCatalog(data: string): void {
        var nullPos = InStr(data, Chr(0));
        this._numResources = parseInt(this.DecryptString(Left(data, nullPos - 1)));
        // UNKNOWN ReDimPreserveStatement
        this._resources[this._numResources] = new ResourceType();
        data = Mid(data, nullPos + 1);
        var resourceStart = 0;
        // UNKNOWN ForBlock
    }
    UpdateDirButtons(dirs: string, ctx: Context): void {
        var compassExits: any = {};
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
    }
    AddCompassExit(exitList: any, name: string): void {
        exitList.Add(new ListData());
    }
    UpdateDoorways(roomId: number, ctx: Context): string {
        var roomDisplayText: string = "";
        var directions: string = "";
        var outPlacePrefix: string = "";
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
                // UNKNOWN SingleLineIfStatement
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
                    } while (!(finished));
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
    }
    UpdateItems(ctx: Context): void {
        var invList: any = {};
        // UNKNOWN SingleLineIfStatement
        var name: string;
        if (this._gameAslVersion >= 280) {
            // UNKNOWN ForBlock
        } else {
            // UNKNOWN ForBlock
        }
        // UNKNOWN RaiseEventStatement
        if (this._gameAslVersion >= 284) {
            this.UpdateStatusVars(ctx);
        } else {
            if (this._numCollectables > 0) {
                var status: string = "";
                // UNKNOWN ForBlock
                this._player.SetStatusText(status);
            }
        }
    }
    FinishGame(stopType: StopType, ctx: Context): void {
        if (stopType == StopType.Win) {
            this.DisplayTextSection("win", ctx);
        } else if (stopType == StopType.Lose) {
            this.DisplayTextSection("lose", ctx);
        }
        this.GameFinished();
    }
    UpdateObjectList(ctx: Context): void {
        var shownPlaceName: string;
        var objSuffix: string;
        var charsFound: number;
        var noFormatObjsViewable: string;
        var charList: string;
        var objsFound: number;
        var objListString: string;
        var noFormatObjListString: string;
        // UNKNOWN SingleLineIfStatement
        var objList: any = {};
        var exitList: any = {};
        var roomBlock: DefineBlock;
        roomBlock = this.DefineBlockParam("room", this._currentRoom);
        if (this._gameAslVersion < 281) {
            // UNKNOWN ForBlock
            if (charsFound == 0) {
                this.SetStringContents("quest.characters", "", ctx);
            } else {
                charList = Left(charsViewable, Len(charsViewable) - 2);
                this.SetStringContents("quest.characters", charList, ctx);
            }
        }
        noFormatObjsViewable = "";
        // UNKNOWN ForBlock
        if (objsFound != 0) {
            objListString = Left(objsViewable, Len(objsViewable) - 2);
            noFormatObjListString = Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2);
            this.SetStringContents("quest.objects", Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2), ctx);
            this.SetStringContents("quest.formatobjects", objListString, ctx);
        } else {
            this.SetStringContents("quest.objects", "", ctx);
            this.SetStringContents("quest.formatobjects", "", ctx);
        }
        var roomId: number;
        roomId = this.GetRoomID(this._currentRoom, ctx);
        var r = this._rooms[roomId];
        if (this._gameAslVersion >= 410) {
            if (roomId > 0) {
                // UNKNOWN ForEachBlock
            }
        } else {
            // UNKNOWN ForBlock
        }
        // UNKNOWN RaiseEventStatement
        this._gotoExits = exitList;
        this.UpdateExitsList();
    }
    UpdateExitsList(): void {
        var mergedList: any = {};
        // UNKNOWN ForEachBlock
        // UNKNOWN ForEachBlock
        // UNKNOWN RaiseEventStatement
    }
    UpdateStatusVars(ctx: Context): void {
        var displayData: string;
        var status: string = "";
        if (this._numDisplayStrings > 0) {
            // UNKNOWN ForBlock
        }
        if (this._numDisplayNumerics > 0) {
            // UNKNOWN ForBlock
        }
        this._player.SetStatusText(status);
    }
    UpdateVisibilityInContainers(ctx: Context, onlyParent: string = ""): void {
        var parentId: number;
        var parent: string;
        var parentIsTransparent: boolean;
        var parentIsOpen: boolean;
        var parentIsSeen: boolean;
        var parentIsSurface: boolean;
        // UNKNOWN SingleLineIfStatement
        if (onlyParent != "") {
            onlyParent = LCase(onlyParent);
            parentId = this.GetObjectIdNoAlias(onlyParent);
            parentIsOpen = this.IsYes(this.GetObjectProperty("opened", parentId, true, false));
            parentIsTransparent = this.IsYes(this.GetObjectProperty("transparent", parentId, true, false));
            parentIsSeen = this.IsYes(this.GetObjectProperty("seen", parentId, true, false));
            parentIsSurface = this.IsYes(this.GetObjectProperty("surface", parentId, true, false));
        }
        // UNKNOWN ForBlock
    }
    PlayerCanAccessObject(id: number, colObjects: any = null): PlayerCanAccessObjectResult {
        var parent: string;
        var parentId: number;
        var parentDisplayName: string;
        var result: PlayerCanAccessObjectResult = new PlayerCanAccessObjectResult();
        var hierarchy: string = "";
        if (this.IsYes(this.GetObjectProperty("parent", id, true, false))) {
            parent = this.GetObjectProperty("parent", id, false, false);
            parentId = this.GetObjectIdNoAlias(parent);
            if (!this.IsYes(this.GetObjectProperty("surface", parentId, true, false)) && !this.IsYes(this.GetObjectProperty("opened", parentId, true, false))) {
                if (this._objs[parentId].ObjectAlias != "") {
                    parentDisplayName = this._objs[parentId].ObjectAlias;
                } else {
                    parentDisplayName = this._objs[parentId].ObjectName;
                }
                result.CanAccessObject = false;
                result.ErrorMsg = "inside closed " + parentDisplayName;
                return result;
            }
            if (colObjects == null) {
                colObjects = new any();
            }
            if (colObjects.Contains(parentId)) {
                // UNKNOWN ForEachBlock
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
    }
    GetGoToExits(roomId: number, ctx: Context): string {
        var placeList: string = "";
        var shownPlaceName: string;
        // UNKNOWN ForBlock
        return placeList;
    }
    SetUpExits(): void {
        // UNKNOWN ForBlock
        // UNKNOWN ExitSubStatement
    }
    FindExit(tag: string): RoomExit {
        var params = Split(tag, ";");
        if (UBound(params) < 1) {
            this.LogASLError("No exit specified in '" + tag + "'", LogType.WarningError);
            return new RoomExit();
        }
        var room = Trim(params[0]);
        var exitName = Trim(params[1]);
        var roomId = this.GetRoomID(room, this._nullContext);
        if (roomId == 0) {
            this.LogASLError("Can't find room '" + room + "'", LogType.WarningError);
            return null;
        }
        var exits = this._rooms[roomId].Exits;
        var dir = exits.GetDirectionEnum(exitName);
        if (dir == Direction.None) {
            if (exits.Places.ContainsKey(exitName)) {
                return exits.Places.Item(exitName);
            }
        } else {
            return exits.GetDirectionExit(dir);
        }
        return null;
    }
    ExecuteLock(tag: string, lock: boolean): void {
        var roomExit: RoomExit;
        roomExit = this.FindExit(tag);
        if (roomExit == null) {
            this.LogASLError("Can't find exit '" + tag + "'", LogType.WarningError);
            // UNKNOWN ExitSubStatement
        }
        roomExit.IsLocked = lock;
    }
    Begin(): void {
        var runnerThread: any = {};
        this.ChangeState(State.Working);
        runnerThread.Start();
        // UNKNOWN SyncLockBlock
    }
    DoBegin(): void {
        var gameBlock: DefineBlock = this.GetDefineBlock("game");
        var ctx: Context = new Context();
        this.SetFont("");
        this.SetFontSize(0);
        // UNKNOWN ForBlock
        // UNKNOWN ForBlock
        this._autoIntro = true;
        if (this._gameAslVersion < 391 || (this._gameAslVersion >= 391 && this._gameLoadMethod == "normal")) {
            if (this._gameAslVersion >= 311) {
                // UNKNOWN ForBlock
            }
            // UNKNOWN ForBlock
        }
        this._gameFullyLoaded = true;
        // UNKNOWN SingleLineIfStatement
        var startRoom: string = "";
        // UNKNOWN ForBlock
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
                ctx = this._nullContext;
                this.ExecuteScript(this._onLoadScript, ctx);
            }
        }
        this.RaiseNextTimerTickRequest();
        this.ChangeState(State.Ready);
    }
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    Finish(): void {
        this.GameFinished();
    }
    // UNKNOWN EventStatement
    // UNKNOWN EventStatement
    // UNKNOWN EventStatement
    Save(filename: string, html: string): void {
        this.SaveGame(filename);
    }
    Save(html: string): number[] {
        return this.SaveGame(Filename, false);
    }
    // UNKNOWN PropertyBlock
    SendCommand(command: string): void {
        this.SendCommand(command, 0, null);
    }
    SendCommand(command: string, metadata: any): void {
        this.SendCommand(command, 0, metadata);
    }
    SendCommand(command: string, elapsedTime: number, metadata: any): void {
        // UNKNOWN SingleLineIfStatement
        var runnerThread: any = {};
        this.ChangeState(State.Working);
        runnerThread.Start(command);
        this.WaitForStateChange(State.Working);
        if (elapsedTime > 0) {
            this.Tick(elapsedTime);
        } else {
            this.RaiseNextTimerTickRequest();
        }
    }
    WaitForStateChange(changedFromState: State): void {
        // UNKNOWN SyncLockBlock
    }
    ProcessCommandInNewThread(command: Object): void {
        // UNKNOWN TryBlock
    }
    SendEvent(eventName: string, param: string): void {
    }
    // UNKNOWN EventStatement
    Initialise(player: IPlayer): boolean {
        this._player = player;
        if (LCase(Right(this._filename, 4)) == ".qsg" || this._data != null) {
            return this.OpenGame(this._filename);
        } else {
            return this.InitialiseGame(this._filename);
        }
    }
    GameFinished(): void {
        this._gameFinished = true;
        // UNKNOWN RaiseEventStatement
        this.ChangeState(State.Finished);
        // UNKNOWN SyncLockBlock
        // UNKNOWN SyncLockBlock
        // UNKNOWN SyncLockBlock
        this.Cleanup();
    }
    GetResourcePath(filename: string): string {
        if (!this._resourceFile == null && this._resourceFile.Length > 0) {
            var extractResult: string = this.ExtractFile(filename);
            return extractResult;
        }
        return System.IO.Path.Combine(this._gamePath, filename);
    }
    Cleanup(): void {
        this.DeleteDirectory(this._tempFolder);
    }
    DeleteDirectory(dir: string): void {
        if (System.IO.Directory.Exists(dir)) {
            // UNKNOWN TryBlock
        }
    }
    GetLibraryLines(libName: string): string[] {
        var libCode: number[] = null;
        libName = LCase(libName);
        // UNKNOWN SelectBlock
        // UNKNOWN SingleLineIfStatement
        return this.GetResourceLines(libCode);
    }
    // UNKNOWN PropertyBlock
    Tick(elapsedTime: number): void {
        var i: number;
        var timerScripts: any = {};
        Debug.Print("Tick: " + elapsedTime.ToString);
        // UNKNOWN ForBlock
        if (timerScripts.Count > 0) {
            var runnerThread: any = {};
            this.ChangeState(State.Working);
            runnerThread.Start(timerScripts);
            this.WaitForStateChange(State.Working);
        }
        this.RaiseNextTimerTickRequest();
    }
    RunTimersInNewThread(scripts: Object): void {
        var scriptList: any = scripts;
        // UNKNOWN ForEachBlock
        this.ChangeState(State.Ready);
    }
    RaiseNextTimerTickRequest(): void {
        var anyTimerActive: boolean = false;
        var nextTrigger: number = 60;
        // UNKNOWN ForBlock
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        Debug.Print("RaiseNextTimerTickRequest " + nextTrigger.ToString);
        // UNKNOWN RaiseEventStatement
    }
    ChangeState(newState: State): void {
        var acceptCommands: boolean = (newState == State.Ready);
        this.ChangeState(newState, acceptCommands);
    }
    ChangeState(newState: State, acceptCommands: boolean): void {
        this._readyForCommand = acceptCommands;
        // UNKNOWN SyncLockBlock
    }
    FinishWait(): void {
        // UNKNOWN SingleLineIfStatement
        var runnerThread: any = {};
        this.ChangeState(State.Working);
        runnerThread.Start();
        this.WaitForStateChange(State.Working);
    }
    FinishWaitInNewThread(): void {
        // UNKNOWN SyncLockBlock
    }
    FinishPause(): void {
        this.FinishWait();
    }
    m_menuResponse: string;
    ShowMenu(menuData: MenuData): string {
        this._player.ShowMenu(menuData);
        this.ChangeState(State.Waiting);
        // UNKNOWN SyncLockBlock
        return this.m_menuResponse;
    }
    SetMenuResponse(response: string): void {
        var runnerThread: any = {};
        this.ChangeState(State.Working);
        runnerThread.Start(response);
        this.WaitForStateChange(State.Working);
    }
    SetMenuResponseInNewThread(response: Object): void {
        this.m_menuResponse = response;
        // UNKNOWN SyncLockBlock
    }
    LogException(ex: Exception): void {
        // UNKNOWN RaiseEventStatement
    }
    GetExternalScripts(): any {
        return null;
    }
    GetExternalStylesheets(): any {
        return null;
    }
    // UNKNOWN EventStatement
    // UNKNOWN PropertyBlock
    GetOriginalFilenameForQSG(): string {
        // UNKNOWN SingleLineIfStatement
        return this._gameFileName;
    }
    // UNKNOWN DelegateFunctionStatement
    m_unzipFunction: UnzipFunctionDelegate;
    SetUnzipFunction(unzipFunction: UnzipFunctionDelegate): void {
        this.m_unzipFunction = unzipFunction;
    }
    GetUnzippedFile(filename: string): string {
        var tempDir: string = null;
        var result: string = this.m_unzipFunction.Invoke(filename, tempDir);
        this._tempFolder = tempDir;
        return result;
    }
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    GetResource(file: string): any {
        if (file == "_game.cas") {
            return new any();
        }
        var path: string = this.GetResourcePath(file);
        // UNKNOWN SingleLineIfStatement
        return new any();
    }
    m_gameId: string;
    // UNKNOWN PropertyBlock
    GetResources(): any {
        // UNKNOWN ForBlock
        if (this._numResources > 0) {
            // UNKNOWN YieldStatement
        }
    }
    GetResourcelessCAS(): number[] {
        var fileData: string = System.IO.File.ReadAllText(this._resourceFile, System.Text.Encoding.GetEncoding(1252));
        return System.Text.Encoding.GetEncoding(1252).GetBytes(Left(fileData, this._startCatPos - 1));
    }
}
class ChangeLog {
    _appliesToType: AppliesTo;
    _changes: any = {};
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    AddItem(appliesTo: string, element: string, changeData: string): void {
        var key = appliesTo + "#" + Left(changeData, 4) + "~" + element;
        if (this._changes.ContainsKey(key)) {
            this._changes.Remove(key);
        }
        this._changes.Add(key, changeData);
    }
}
class Config {
    // UNKNOWN PropertyBlock
}
class RoomExit {
    Id: string;
    _objId: number;
    _roomId: number;
    _direction: any;
    _parent: RoomExits;
    _objName: string;
    _displayName: string;
    _game: LegacyGame;
    constructor(game: LegacyGame) {
        this._game = game;
        game._numberObjs = game._numberObjs + 1;
        // UNKNOWN ReDimPreserveStatement
        game._objs[game._numberObjs] = new ObjectType();
        this._objId = game._numberObjs;
        // UNKNOWN WithBlock
    }
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
    RunAction(actionName: string, ctx: Context): void {
        this._game.DoAction(this._objId, actionName, ctx);
    }
    RunScript(ctx: Context): void {
        this.RunAction("script", ctx);
    }
    UpdateObjectName(): void {
        var objName: string;
        var lastExitId: number;
        var parentRoom: string;
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        parentRoom = this._game._objs[this._parent.ObjId].ObjectName;
        objName = parentRoom;
        if (this._direction != LegacyGame.Direction.None) {
            objName = objName + "." + this._parent.GetDirectionName(this._direction);
            this._game._objs[this._objId].ObjectAlias = this._parent.GetDirectionName(this._direction);
        } else {
            var lastExitIdString: string = this._game.GetObjectProperty("quest.lastexitid", (this._parent.ObjId), null, false);
            if (lastExitIdString.Length == 0) {
                lastExitId = 0;
            } else {
                lastExitId = parseInt(lastExitId);
            }
            lastExitId = lastExitId + 1;
            this._game.AddToObjectProperties("quest.lastexitid=" + (lastExitId).toString(), (this._parent.ObjId), this._game._nullContext);
            objName = objName + ".exit" + (lastExitId).toString();
            if (RoomId == 0) {
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
    }
    Go(ctx: Context): void {
        if (IsLocked) {
            if (ExitPropertyBool("lockmessage")) {
                this._game.Print(ExitProperty("lockmessage"), ctx);
            } else {
                this._game.PlayerErrorMessage(PlayerError.Locked, ctx);
            }
        } else {
            if (IsScript) {
                this.RunScript(ctx);
            } else {
                this._game.PlayGame(ToRoom, ctx);
            }
        }
    }
}
class RoomExits {
    _directions: any = {};
    _places: any = {};
    _objId: number;
    _allExits: any;
    _regenerateAllExits: boolean;
    _game: LegacyGame;
    constructor(game: LegacyGame) {
        this._game = game;
        this._regenerateAllExits = true;
    }
    SetDirection(direction: Direction, roomExit: RoomExit): void {
        if (this._directions.ContainsKey(direction)) {
            roomExit = this._directions.Item(direction);
            this._game._objs[roomExit.ObjId].Exists = true;
        } else {
            roomExit = new RoomExit();
            this._directions.Add(direction, roomExit);
        }
        this._regenerateAllExits = true;
    }
    GetDirectionExit(direction: Direction): RoomExit {
        if (this._directions.ContainsKey(direction)) {
            return this._directions.Item(direction);
        }
        return null;
    }
    AddPlaceExit(roomExit: RoomExit): void {
        if (this._places.ContainsKey(roomExit.ToRoom)) {
            var removeItem: RoomExit = this._places.Item(roomExit.ToRoom);
            this.RemoveExit(removeItem);
        }
        this._places.Add(roomExit.ToRoom, roomExit);
        this._regenerateAllExits = true;
    }
    AddExitFromTag(tag: string): void {
        var thisDir: Direction;
        var roomExit: RoomExit = null;
        var params: string[] = [];
        var afterParam: string;
        var param: boolean;
        if (this._game.BeginsWith(tag, "out ")) {
            tag = this._game.GetEverythingAfter(tag, "out ");
            thisDir = Direction.Out;
        } else if (this._game.BeginsWith(tag, "east ")) {
            tag = this._game.GetEverythingAfter(tag, "east ");
            thisDir = Direction.East;
        } else if (this._game.BeginsWith(tag, "west ")) {
            tag = this._game.GetEverythingAfter(tag, "west ");
            thisDir = Direction.West;
        } else if (this._game.BeginsWith(tag, "north ")) {
            tag = this._game.GetEverythingAfter(tag, "north ");
            thisDir = Direction.North;
        } else if (this._game.BeginsWith(tag, "south ")) {
            tag = this._game.GetEverythingAfter(tag, "south ");
            thisDir = Direction.South;
        } else if (this._game.BeginsWith(tag, "northeast ")) {
            tag = this._game.GetEverythingAfter(tag, "northeast ");
            thisDir = Direction.NorthEast;
        } else if (this._game.BeginsWith(tag, "northwest ")) {
            tag = this._game.GetEverythingAfter(tag, "northwest ");
            thisDir = Direction.NorthWest;
        } else if (this._game.BeginsWith(tag, "southeast ")) {
            tag = this._game.GetEverythingAfter(tag, "southeast ");
            thisDir = Direction.SouthEast;
        } else if (this._game.BeginsWith(tag, "southwest ")) {
            tag = this._game.GetEverythingAfter(tag, "southwest ");
            thisDir = Direction.SouthWest;
        } else if (this._game.BeginsWith(tag, "up ")) {
            tag = this._game.GetEverythingAfter(tag, "up ");
            thisDir = Direction.Up;
        } else if (this._game.BeginsWith(tag, "down ")) {
            tag = this._game.GetEverythingAfter(tag, "down ");
            thisDir = Direction.Down;
        } else if (this._game.BeginsWith(tag, "place ")) {
            tag = this._game.GetEverythingAfter(tag, "place ");
            thisDir = Direction.None;
        } else {
            // UNKNOWN ExitSubStatement
        }
        if (thisDir != Direction.None) {
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
            roomExit.Script = afterParam;
            if (thisDir == Direction.None) {
                roomExit.ToRoom = params[0];
                if (UBound(params) > 0) {
                    roomExit.LockMessage = params[1];
                }
            } else {
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
        if (thisDir == Direction.None) {
            this.AddPlaceExit(roomExit);
        }
    }
    AddExitFromCreateScript(script: string, ctx: Context): void {
        var param: string;
        var params: string[];
        var paramStart: number;
        var paramEnd: number;
        param = this._game.GetParameter(script, ctx);
        params = Split(param, ";");
        paramStart = InStr(script, "<");
        paramEnd = InStr(paramStart, script, ">");
        if (paramStart > 1) {
            if (UBound(params) == 0) {
                this.AddExitFromTag(Trim(Left(script, paramStart - 1)) + " " + Trim(Mid(script, paramEnd + 1)));
            } else {
                this.AddExitFromTag(Trim(Left(script, paramStart - 1)) + " <" + Trim(params[1]) + ">");
            }
        } else {
            if (UBound(params) < 1) {
                this._game.LogASLError("No exit destination given in 'create exit " + script + "'", LogType.WarningError);
                // UNKNOWN ExitSubStatement
            }
            this.AddExitFromTag("place <" + Trim(params[1]) + Mid(script, paramEnd));
        }
    }
    // UNKNOWN PropertyBlock
    // UNKNOWN PropertyBlock
    ExecuteGo(cmd: string, ctx: Context): void {
        var lExitID: number;
        var oExit: RoomExit;
        if (this._game.BeginsWith(cmd, "go to ")) {
            cmd = this._game.GetEverythingAfter(cmd, "go to ");
        } else if (this._game.BeginsWith(cmd, "go ")) {
            cmd = this._game.GetEverythingAfter(cmd, "go ");
        }
        lExitID = this._game.Disambiguate(cmd, this._game._currentRoom, ctx, true);
        if (lExitID == -1) {
            this._game.PlayerErrorMessage(PlayerError.BadPlace, ctx);
        } else {
            oExit = this.GetExitByObjectId(lExitID);
            oExit.Go(ctx);
        }
    }
    GetAvailableDirectionsDescription(description: string, list: string): void {
        var roomExit: RoomExit;
        var count: number;
        var descPrefix: string;
        var orString: string;
        descPrefix = "You can go";
        orString = "or";
        list = "";
        count = 0;
        // UNKNOWN ForEachBlock
        this._game.SetStringContents("quest.doorways", description, this._game._nullContext);
        if (count > 0) {
            description = descPrefix + " " + description + ".";
        }
    }
    GetDirectionName(dir: Direction): string {
        // UNKNOWN SelectBlock
        return null;
    }
    GetDirectionEnum(dir: string): Direction {
        // UNKNOWN SelectBlock
        return Direction.None;
    }
    GetDirectionToken(dir: Direction): string {
        // UNKNOWN SelectBlock
        return null;
    }
    GetDirectionNameDisplay(roomExit: RoomExit): string {
        if (roomExit.Direction != Direction.None) {
            var dir = this.GetDirectionName((roomExit.Direction));
            return "|b" + dir + "|xb";
        }
        var sDisplay = "|b" + roomExit.DisplayName + "|xb";
        if (Len(roomExit.Prefix) > 0) {
            sDisplay = roomExit.Prefix + " " + sDisplay;
        }
        return "to " + sDisplay;
    }
    GetExitByObjectId(id: number): RoomExit {
        // UNKNOWN ForEachBlock
        return null;
    }
    AllExits(): any {
        if (!this._regenerateAllExits) {
            this.AllExits = this._allExits;
            // UNKNOWN ExitFunctionStatement
        }
        this._allExits = new any();
        // UNKNOWN ForEachBlock
        // UNKNOWN ForEachBlock
        return this._allExits;
    }
    RemoveExit(roomExit: RoomExit): void {
        if (roomExit.Direction == Direction.None) {
            if (this._places.ContainsKey(roomExit.ToRoom)) {
                this._places.Remove(roomExit.ToRoom);
            }
        }
        this._game._objs[roomExit.ObjId].Exists = false;
        this._regenerateAllExits = true;
    }
}
class TextFormatter {
    bold: boolean;
    italic: boolean;
    underline: boolean;
    colour: string = "";
    fontSize: number = 0;
    align: string = "";
    OutputHTML(input: string): string {
        var output: string = "";
        var position: number = 0;
        var codePosition: number;
        var finished: boolean = false;
        var nobr: boolean = false;
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
            } else {
                // UNKNOWN AddAssignmentStatement
                position = codePosition + 1;
                var oneCharCode: string = "";
                var twoCharCode: string = "";
                if (position < input.Length) {
                    oneCharCode = input.Substring(position, 1);
                }
                if (position < (input.Length - 1)) {
                    twoCharCode = input.Substring(position, 2);
                }
                var foundCode: boolean = true;
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
                        if (position < (input.Length - 2)) {
                            var sizeCode: string = input.Substring(position + 1, 2);
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
        } while (!(finished || position >= input.Length));
        return String.Format("<output{0}>{1}</output>", nobr ? " nobr=\"true\"" : "", output);
    }
    FormatText(input: string): string {
        // UNKNOWN SingleLineIfStatement
        var output: string = "";
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN AddAssignmentStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        // UNKNOWN SingleLineIfStatement
        return output;
    }
}
