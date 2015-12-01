"use strict";

interface Window {
    quest: any;
}

var quest = window.quest || {};

function Left(input: string, length: number): string {
    return input.substring(0, length);
}

function Right(input: string, length: number): string {
    return input.substring(input.length - length);
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

function InStr(input: string, search: string): number {
    return input.indexOf(search) + 1;
}

function InStrFrom(start: number, input: string, search: string): number {
    return input.indexOf(search, start - 1) + 1;
}

function InStrRev(input: string, search: string): number {
    return input.lastIndexOf(search) + 1;
}

function Split(input: string, splitChar: string): string[] {
    return input.split(splitChar);
}

function IsNumeric(input: any): boolean {
    return !isNaN(parseFloat(input)) && isFinite(input);
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

var windows1252mapping = ['\u20AC','','\u201A','\u0192','\u201E','\u2026',
    '\u2020','\u2021','\u02C6','\u2030','\u0160','\u2039','\u0152','',
    '\u017D','','','\u2018','\u2019','\u201C','\u201D','\u2022','\u2013',
    '\u2014','\u02DC','\u2122','\u0161','\u203A','\u0153','','\u017E',
    '\u0178'];

function Chr(input: number): string {
    if (input < 128 || input > 159) {
        return String.fromCharCode(input);
    }
    return windows1252mapping[input - 128];
}

function Len(input: string): number {
    if (input === null || input === undefined) return 0;
    return input.length;
}

function UBound(array: any[]): number {
    return array.length - 1;
}

function Str(input: number): string {
    return input.toString();
}

function Val(input: string): number {
    return parseInt(input);
}

interface StringDictionary {
    [index: string]: string;
}

class MenuData {
    Caption: string;
    Options: StringDictionary;
    AllowCancel: boolean;
    
    constructor(caption: string, options: StringDictionary, allowCancel: boolean) {
        this.Caption = caption;
        this.Options = options;
        this.AllowCancel = allowCancel;
    }
}

class Player {
    TextFormatter: TextFormatter;
    ResourceRoot: string;
    constructor(textFormatter: TextFormatter) {
        this.TextFormatter = textFormatter;
    }
    SetResourceRoot(resourceRoot: string) {
        this.ResourceRoot = resourceRoot;
    }
    ShowMenu(menuData: MenuData) {
        quest.ui.showMenu(menuData.Caption, menuData.Options, menuData.AllowCancel);
    }
    DoWait() {
        quest.ui.beginWait();
    }
    ShowQuestion(caption: string) {
        quest.ui.showQuestion(caption);
    }
    PlaySound(filename: string, synchronous: boolean, looped: boolean) {
        quest.ui.playSound(this.ResourceRoot + filename, synchronous, looped);
    }
    StopSound() {
        quest.ui.stopSound;
    }
    ClearScreen() {
        quest.ui.clearScreen();
    }
    SetBackground(colour: string) {
        quest.ui.setBackground(colour);
    }
    SetForeground(colour: string) {
        this.TextFormatter.foreground = colour;
    }
    SetFont(fontName: string) {
        this.TextFormatter.fontFamily = fontName;
    }
    SetFontSize(fontSize: number) {
        this.TextFormatter.defaultFontSize = fontSize;
    }
    SetPanelContents(html: string) {
        quest.ui.setPanelContents(html);
    }
    SetPanesVisible(data: string) {
        quest.ui.panesVisible(data == "on");
    }
    ShowPicture(filename: string) {
        var url = this.ResourceRoot + filename;
        var html = `<img src="${url}" onload="scrollToEnd();" /><br />`
        quest.print(html);
    }
    GetURL(file: string) {
        return this.ResourceRoot + file;
    }
    LocationUpdated(location: string) {
        quest.ui.locationUpdated(location);
    }
    GetNewGameFile(originalFilename: string, extensions: string) {
        
    }
    RequestSave(html: string) {
        
    }
    UpdateGameName(name: string) {
        if (name.length == 0) name = "Untitled Game";
        quest.ui.setGameName(name);
    }
    Show(element: string) {
        quest.ui.show(element);
    }
    SetStatusText(text: string) {
        quest.ui.updateStatus(text.replace("\n", "<br />"));
    }
    UpdateInventoryList(items: ListData[]) {
        quest.ui.updateList("inventory", items);
    }
    UpdateObjectsList(items: ListData[]) {
        quest.ui.updateList("placesobjects", items);
    }
    UpdateExitsList(items: ListData[]) {
        quest.ui.updateCompass(items);
    }
    RequestNextTimerTick(seconds: number) {
        quest.ui.requestNextTimerTick(seconds);
    }
}

class ListData {
    Text: string;
    Verbs: string[];
    ElementId: string;
    ElementName: string;
    constructor(text: string, verbs: string[]) {
        this.Text = text;
        this.Verbs = verbs;
        this.ElementId = text;
        this.ElementName = text;
    }
}

enum ListType {InventoryList, ExitsList, ObjectsList};

interface Callback {
    (): void;
}

interface BooleanCallback {
    (data: boolean): void;
}

interface StringCallback {
    (data: string): void;
}

interface FileFetcherCallback {
    (data: string): void;
}

interface FileFetcher {
    (filename: string, onSuccess: FileFetcherCallback, onFailure: FileFetcherCallback): void;
}

interface BinaryFileFetcherCallback {
    (data: Uint8Array): void;
}

interface BinaryFileFetcher {
    (filename: string, onSuccess: BinaryFileFetcherCallback, onFailure: FileFetcherCallback): void;
}

enum State {Ready, Working, Waiting, Finished};
class DefineBlock {
    StartLine: number = 0;
    EndLine: number = 0;
}
class Context {
    CallingObjectId: number = 0;
    NumParameters: number = 0;
    Parameters: string[];
    FunctionReturnValue: string = "";
    AllowRealNamesInCommand: boolean = false;
    DontProcessCommand: boolean = false;
    CancelExec: boolean = false;
    StackCounter: number = 0;
}
enum LogType {Misc, FatalError, WarningError, Init, LibraryWarningError, Warning, UserError, InternalError};
enum Direction {None = -1, Out = 0, North = 1, South = 2, East = 3, West = 4, NorthWest = 5, NorthEast = 6, SouthWest = 7, SouthEast = 8, Up = 9, Down = 10};
class ItemType {
    Name: string = "";
    Got: boolean = false;
}
class Collectable {
    Name: string = "";
    Type: string = "";
    Value: number = 0;
    Display: string = "";
    DisplayWhenZero: boolean = false;
}
class PropertyType {
    PropertyName: string = "";
    PropertyValue: string = "";
}
class ActionType {
    ActionName: string = "";
    Script: string = "";
}
class UseDataType {
    UseObject: string = "";
    UseType: UseType;
    UseScript: string = "";
}
class GiveDataType {
    GiveObject: string = "";
    GiveType: GiveType;
    GiveScript: string = "";
}
class PropertiesActions {
    Properties: string = "";
    NumberActions: number = 0;
    Actions: ActionType[];
    NumberTypesIncluded: number = 0;
    TypesIncluded: string[];
}
class VariableType {
    VariableName: string = "";
    VariableContents: string[];
    VariableUBound: number = 0;
    DisplayString: string = "";
    OnChangeScript: string = "";
    NoZeroDisplay: boolean = false;
}
class SynonymType {
    OriginalWord: string = "";
    ConvertTo: string = "";
}
class TimerType {
    TimerName: string = "";
    TimerInterval: number = 0;
    TimerActive: boolean = false;
    TimerAction: string = "";
    TimerTicks: number = 0;
    BypassThisTurn: boolean = false;
}
class UserDefinedCommandType {
    CommandText: string = "";
    CommandScript: string = "";
}
class TextAction {
    Data: string = "";
    Type: TextActionType;
}
enum TextActionType {Text, Script, Nothing, Default};
class ScriptText {
    Text: string = "";
    Script: string = "";
}
class PlaceType {
    PlaceName: string = "";
    Prefix: string = "";
    PlaceAlias: string = "";
    Script: string = "";
}
class RoomType {
    RoomName: string = "";
    RoomAlias: string = "";
    Commands: UserDefinedCommandType[];
    NumberCommands: number = 0;
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
    InDescription: string = "";
    Look: string = "";
    Places: PlaceType[];
    NumberPlaces: number = 0;
    Prefix: string = "";
    Script: string = "";
    Use: ScriptText[];
    NumberUse: number = 0;
    ObjId: number = 0;
    BeforeTurnScript: string = "";
    AfterTurnScript: string = "";
    Exits: RoomExits;
}
class ObjectType {
    ObjectName: string = "";
    ObjectAlias: string = "";
    Detail: string = "";
    ContainerRoom: string = "";
    Exists: boolean = false;
    IsGlobal: boolean = false;
    Prefix: string = "";
    Suffix: string = "";
    Gender: string = "";
    Article: string = "";
    DefinitionSectionStart: number = 0;
    DefinitionSectionEnd: number = 0;
    Visible: boolean = false;
    GainScript: string = "";
    LoseScript: string = "";
    NumberProperties: number = 0;
    Properties: PropertyType[];
    Speak: TextAction = new TextAction();
    Take: TextAction = new TextAction();
    IsRoom: boolean = false;
    IsExit: boolean = false;
    CorresRoom: string = "";
    CorresRoomId: number = 0;
    Loaded: boolean = false;
    NumberActions: number = 0;
    Actions: ActionType[];
    NumberUseData: number = 0;
    UseData: UseDataType[];
    UseAnything: string = "";
    UseOnAnything: string = "";
    Use: string = "";
    NumberGiveData: number = 0;
    GiveData: GiveDataType[];
    GiveAnything: string = "";
    GiveToAnything: string = "";
    DisplayType: string = "";
    NumberTypesIncluded: number = 0;
    TypesIncluded: string[];
    NumberAltNames: number = 0;
    AltNames: string[];
    AddScript: TextAction = new TextAction();
    RemoveScript: TextAction = new TextAction();
    OpenScript: TextAction = new TextAction();
    CloseScript: TextAction = new TextAction();
}
class ChangeType {
    AppliesTo: string = "";
    Change: string = "";
}
class GameChangeDataType {
    NumberChanges: number = 0;
    ChangeData: ChangeType[];
}
class ResourceType {
    ResourceName: string = "";
    ResourceStart: number = 0;
    ResourceLength: number = 0;
    Extracted: boolean = false;
}
class ExpressionResult {
    Result: string = "";
    Success: ExpressionSuccess;
    Message: string = "";
}
class GetAvailableDirectionsResult {
    Description: string = "";
    List: string = "";
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
    Index: number = 0;
}
class PlayerCanAccessObjectResult {
    CanAccessObject: boolean = false;
    ErrorMsg: string;
}
enum AppliesTo {Object, Room};

interface DefineBlockParams {
    [index: string]: StringDictionary;
}

interface ListVerbs {
    [index: number]: string[];
}

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
    _defineBlockParams: DefineBlockParams;
    _openErrorReport: string = "";
    _casKeywords: string[] = [];
    //Tokenised CAS keywords
    _lines: string[];
    //Stores the lines of the ASL script/definitions
    _defineBlocks: DefineBlock[];
    //Stores the start and end lines of each 'define' section
    _numberSections: number = 0;
    //Number of define sections
    _gameName: string = "";
    //The name of the game
    _nullContext: Context = new Context();
    _changeLogRooms: ChangeLog;
    _changeLogObjects: ChangeLog;
    _defaultProperties: PropertiesActions;
    _defaultRoomProperties: PropertiesActions;
    _rooms: RoomType[];
    _numberRooms: number = 0;
    _numericVariable: VariableType[];
    _numberNumericVariables: number = 0;
    _stringVariable: VariableType[];
    _numberStringVariables: number = 0;
    _synonyms: SynonymType[];
    _numberSynonyms: number = 0;
    _items: ItemType[];
    _chars: ObjectType[];
    _objs: ObjectType[];
    _numberChars: number = 0;
    _numberObjs: number = 0;
    _numberItems: number = 0;
    _currentRoom: string = "";
    _collectables: Collectable[];
    _numCollectables: number = 0;
    _gameFileName: string = "";
    _defaultFontName: string = "";
    _defaultFontSize: number = 0;
    _autoIntro: boolean = false;
    _commandOverrideModeOn: boolean = false;
    _commandOverrideResolve: Callback;
    _commandOverrideVariable: string = "";
    _waitResolve: Callback;
    _askResolve: BooleanCallback;
    _menuResolve: StringCallback;
    _afterTurnScript: string = "";
    _beforeTurnScript: string = "";
    _outPutOn: boolean = false;
    _gameAslVersion: number = 0;
    _choiceNumber: number = 0;
    _gameLoadMethod: string = "";
    _timers: TimerType[];
    _numberTimers: number = 0;
    _numDisplayStrings: number = 0;
    _numDisplayNumerics: number = 0;
    _gameFullyLoaded: boolean = false;
    _gameChangeData: GameChangeDataType = new GameChangeDataType();
    _lastIt: number = 0;
    _lastItMode: ItType;
    _thisTurnIt: number = 0;
    _thisTurnItMode: ItType;
    _badCmdBefore: string = "";
    _badCmdAfter: string = "";
    _numResources: number = 0;
    _resources: ResourceType[];
    _resourceFile: string = "";
    _resourceOffset: number = 0;
    _startCatPos: number = 0;
    _useAbbreviations: boolean = false;
    _loadedFromQsg: boolean = false;
    _beforeSaveScript: string = "";
    _onLoadScript: string = "";
    _skipCheckFile = ["bargain.cas", "easymoney.asl", "musicvf1.cas"];
    _compassExits: ListData[] = [];
    _gotoExits: ListData[] = [];
    _textFormatter: TextFormatter = new TextFormatter();
    _casFileData: Uint8Array;
    _commandLock: Object = new Object();
    _stateLock: Object = new Object();
    _state: State = State.Ready;
    _waitLock: Object = new Object();
    _readyForCommand: boolean = true;
    _gameLoading: boolean = false;
    _playerErrorMessageString: string[] = [];
    _listVerbs: ListVerbs = {};
    _filename: string = "";
    _originalFilename: string = "";
    _data: string;
    _player: Player = new Player(this._textFormatter);
    _gameFinished: boolean = false;
    _gameIsRestoring: boolean = false;
    _useStaticFrameForPictures: boolean = false;
    _fileData: string = "";
    _fileDataPos: number = 0;
    _questionResponse: boolean = false;
    _fileFetcher: FileFetcher;
    _binaryFileFetcher: BinaryFileFetcher;
    
    constructor(filename: string,
        originalFilename: string,
        data: string,
        fileFetcher: FileFetcher,
        binaryFileFetcher: BinaryFileFetcher,
        resourceRoot: string) {
        this.LoadCASKeywords();
        this._gameLoadMethod = "normal";
        this._filename = filename;
        this._originalFilename = originalFilename;
        this._data = data;
        this._fileFetcher = fileFetcher;
        this._binaryFileFetcher = binaryFileFetcher;
        this._player.SetResourceRoot(resourceRoot);
    }
    RemoveFormatting(s: string): string {
        var code: string;
        var pos: number = 0;
        var len: number = 0;
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
        } while (!(pos == 0));
        return s;
    }
    CheckSections(): boolean {
        var defines: number = 0;
        var braces: number = 0;
        var checkLine: string = "";
        var bracePos: number = 0;
        var pos: number = 0;
        var section: string = "";
        var hasErrors: boolean = false;
        var skipBlock: boolean = false;
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
                        this.LogASLError("Extra 'end define' after block '" + section + "'", LogType.FatalError);
                        this._openErrorReport = this._openErrorReport + "Extra 'end define' after block '" + section + "'\n";
                        hasErrors = true;
                        defines = 0;
                    }
                    if (braces > 0) {
                        this.LogASLError("Missing } in block '" + section + "'", LogType.FatalError);
                        this._openErrorReport = this._openErrorReport + "Missing } in block '" + section + "'\n";
                        hasErrors = true;
                    } else if (braces < 0) {
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
    }
    ConvertFriendlyIfs(): boolean {
        // Converts
        //   if (%something% < 3) then ...
        // to
        //   if is <%something%;lt;3> then ...
        // and also repeat until ...
        // Returns False if successful
        var convPos: number = 0;
        var symbPos: number = 0;
        var symbol: string;
        var endParamPos: number = 0;
        var paramData: string;
        var startParamPos: number = 0;
        var firstData: string;
        var secondData: string;
        var obscureLine: string;
        var newParam: string;
        var varObscureLine: string;
        var bracketCount: number = 0;
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
                    } else if (Mid(this._lines[i], j, 1) == ")") {
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
    }
    ConvertMultiLineSections(): void {
        var startLine: number = 0;
        var braceCount: number = 0;
        var thisLine: string;
        var lineToAdd: string;
        var lastBrace: number = 0;
        var i: number = 0;
        var restOfLine: string;
        var procName: string;
        var endLineNum: number = 0;
        var afterLastBrace: string;
        var z: string;
        var startOfOrig: string;
        var testLine: string;
        var testBraceCount: number = 0;
        var obp: number = 0;
        var cbp: number = 0;
        var curProc: number = 0;
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
                                } while (braceCount != 0);
                            }
                            this.AddLine("end define");
                            endLineNum = UBound(this._lines);
                            this._numberSections = this._numberSections + 1;
                            if (!this._defineBlocks) this._defineBlocks = [];
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
    }
    ErrorCheck(): boolean {
        // Parses ASL script for errors. Returns TRUE if OK;
        // False if a critical error is encountered.
        var curBegin: number = 0;
        var curEnd: number = 0;
        var hasErrors: boolean = false;
        var curPos: number = 0;
        var numParamStart: number = 0;
        var numParamEnd: number = 0;
        var finLoop: boolean = false;
        var inText: boolean = false;
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
                    } else {
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
                    } else {
                        finLoop = true;
                    }
                } while (!(finLoop));
                if (numParamStart > numParamEnd) {
                    this.LogASLError("Expected > in " + this.ReportErrorLine(this._lines[i]), LogType.FatalError);
                    hasErrors = true;
                } else if (numParamStart < numParamEnd) {
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
            } else {
                if (!this.BeginsWith(this._lines[curBegin], "define synonyms") && !this.BeginsWith(this._lines[curBegin], "define options")) {
                    if (InStr(this._lines[curBegin], "<") == 0) {
                        this.LogASLError(this._lines[curBegin] + " has no parameter", LogType.FatalError);
                        hasErrors = true;
                    }
                }
            }
        }
        return hasErrors;
    }
    GetAfterParameter(s: string): string {
        // Returns everything after the end of the first parameter
        // in a string, i.e. for "use <thing> do <myproc>" it
        // returns "do <myproc>"
        var eop: number = 0;
        eop = InStr(s, ">");
        if (eop == 0 || eop + 1 > Len(s)) {
            return "";
        } else {
            return Trim(Mid(s, eop + 1));
        }
    }
    ObliterateParameters(s: string): string {
        var inParameter: boolean = false;
        var exitCharacter: string = "";
        var curChar: string;
        var outputLine: string = "";
        var obscuringFunctionName: boolean = false;
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
    }
    ObliterateVariableNames(s: string): string {
        var inParameter: boolean = false;
        var exitCharacter: string = "";
        var outputLine: string = "";
        var curChar: string;
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
    }
    RemoveComments(): void {
        var aposPos: number = 0;
        var inTextBlock: boolean = false;
        var inSynonymsBlock: boolean = false;
        var oblitLine: string;
        // If in a synonyms block, we want to remove lines which are comments, but
        // we don't want to remove synonyms that contain apostrophes, so we only
        // get rid of lines with an "'" at the beginning or with " '" in them
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
    }
    ReportErrorLine(s: string): string {
        // We don't want to see the "!intproc" in logged error reports lines.
        // This function replaces these "do" lines with a nicer-looking "..." for error reporting.
        var replaceFrom: number = 0;
        replaceFrom = InStr(s, "do <!intproc");
        if (replaceFrom != 0) {
            return Left(s, replaceFrom - 1) + "...";
        } else {
            return s;
        }
    }
    YesNo(yn: boolean): string {
        return yn ? "Yes" : "No";
    }
    IsYes(yn: string): boolean {
        if (LCase(yn) == "yes") {
            return true;
        } else {
            return false;
        }
    }
    BeginsWith(s: string, text: string): boolean {
        // Compares the beginning of the line with a given
        // string. Case insensitive.
        // Example: beginswith("Hello there","HeLlO")=TRUE
        return Left(LTrim(LCase(s)), Len(text)) == LCase(text);
    }
    ConvertCasKeyword(casChar: number): string {
        var keyword: string = this._casKeywords[casChar];
        if (keyword == "!cr") {
            keyword = "\n";
        }
        return keyword;
    }
    ConvertMultiLines(): void {
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
            } else if (Right(this._lines[i], 1) == "_") {
                this._lines[i] = Left(this._lines[i], Len(this._lines[i]) - 1) + LTrim(this._lines[i + 1]);
                this._lines[i + 1] = "";
                //Recalculate this line again
                i = i + 1;
            }
        }
        this.RemoveComments();
    }
    GetDefineBlock(blockname: string): DefineBlock {
        // Returns the start and end points of a named block.
        // Returns 0 if block not found.
        var l: string;
        var blockType: string;
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
    }
    DefineBlockParam(blockname: string, param: string): DefineBlock {
        // Returns the start and end points of a named block
        var cache: StringDictionary;
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
                    var blockKey = this.GetSimpleParameter(this._lines[this._defineBlocks[i].StartLine]);
                    blockKey = "k" + blockKey;
                    if (!cache[blockKey]) {
                        cache[blockKey] = this._defineBlocks[i].StartLine + "," + this._defineBlocks[i].EndLine;
                    }
                }
            }
        } else {
            cache = this._defineBlockParams[blockname];
        }
        if (cache[param]) {
            var blocks = Split(cache[param], ",");
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
    LoadCASKeywords(): void {
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
    }
    
    LoadLibraries(onSuccess: Callback, onFailure: Callback, start: number = this._lines.length - 1) {
        var libFoundThisSweep = false;
        var libFileName: string;
        var libraryList: string[] = [];
        var numLibraries: number = 0;
        var libraryAlreadyIncluded: boolean = false;

        var self = this;
        for (var i = start; i >= 1; i--) {
            // We search for includes backwards as a game might include
            // some-general.lib and then something-specific.lib which needs
            // something-general; if we include something-specific first,
            // then something-general afterwards, something-general's startscript
            // gets executed before something-specific's, as we execute the
            // lib startscripts backwards as well
            if (self.BeginsWith(self._lines[i], "!include ")) {
                libFileName = self.GetSimpleParameter(self._lines[i]);
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
                } else {
                    numLibraries = numLibraries + 1;
                    if (!libraryList) libraryList = [];
                    libraryList[numLibraries] = libFileName;
                    libFoundThisSweep = true;
                    self.LogASLError(" - Searching for " + libFileName + " (game path)", LogType.Init);
                    
                    var loadLibrary = function (libCode: string[]) {
                        self.LogASLError("     - Found library, opening...", LogType.Init);
                        libCode = libCode.map(function (line: string) {
                            return self.RemoveTabs(line).trim();
                        });
                        
                        self.LoadLibrary(libCode);
                        
                        // continue scanning for libraries
                        self.LoadLibraries(onSuccess, onFailure, i-1);
                    };
                    
                    var onLibSuccess = function (data: string) {
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
    }
    
    LoadLibrary(libCode: string[]) {
        var self = this;
        var libLines: number = libCode.length - 1;
        var ignoreMode: boolean = false;
        var inDefGameBlock: number = 0;
        var gameLine: number = 0;
        var inDefSynBlock: number = 0;
        var synLine: number = 0;
        var inDefTypeBlock: number = 0;
        var typeBlockName: string;
        var typeLine: number = 0;

        var libVer = -1;
        
        if (libCode[1] == "!library") {
            for (var c = 1; c <= libLines; c++) {
                if (self.BeginsWith(libCode[c], "!asl-version ")) {
                    libVer = parseInt(self.GetSimpleParameter(libCode[c]));
                    break;
                }
            }
        } else {
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
            } else if (Left(libCode[c], 1) != "!" && Left(libCode[c], 1) != "'" && !ignoreMode) {
                self.AddLine(libCode[c]);
            } else {
                if (libCode[c] == "!addto game") {
                    inDefGameBlock = 0;
                    for (var d = 1; d <= UBound(self._lines); d++) {
                        if (self.BeginsWith(self._lines[d], "define game ")) {
                            inDefGameBlock = 1;
                        } else if (self.BeginsWith(self._lines[d], "define ")) {
                            if (inDefGameBlock != 0) {
                                inDefGameBlock = inDefGameBlock + 1;
                            }
                        } else if (self._lines[d] == "end define" && inDefGameBlock == 1) {
                            gameLine = d;
                            break;
                        } else if (self._lines[d] == "end define") {
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
                            var lineToAdd: string;
                            if (libVer >= 311 && self.BeginsWith(libCode[c], "startscript ")) {
                                lineToAdd = "lib " + libCode[c];
                            } else if (libVer >= 392 && (self.BeginsWith(libCode[c], "command ") || self.BeginsWith(libCode[c], "verb "))) {
                                lineToAdd = "lib " + libCode[c];
                            } else {
                                lineToAdd = libCode[c];
                            }
                            self._lines.splice(gameLine, 0, lineToAdd);
                            gameLine = gameLine + 1;
                        }
                    } while (!(self.BeginsWith(libCode[c], "!end")));
                } else if (libCode[c] == "!addto synonyms") {
                    inDefSynBlock = 0;
                    for (var d = 1; d <= UBound(self._lines); d++) {
                        if (self._lines[d] == "define synonyms") {
                            inDefSynBlock = 1;
                        } else if (self._lines[d] == "end define" && inDefSynBlock == 1) {
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
                            if (!self._lines) self._lines = [];
                            for (var d = UBound(self._lines); d >= synLine + 1; d--) {
                                self._lines[d] = self._lines[d - 1];
                            }
                            self._lines[synLine] = libCode[c];
                            synLine = synLine + 1;
                        }
                    } while (!(self.BeginsWith(libCode[c], "!end")));
                } else if (self.BeginsWith(libCode[c], "!addto type ")) {
                    inDefTypeBlock = 0;
                    typeBlockName = LCase(self.GetSimpleParameter(libCode[c]));
                    for (var d = 1; d <= UBound(self._lines); d++) {
                        if (LCase(self._lines[d]) == "define type <" + typeBlockName + ">") {
                            inDefTypeBlock = 1;
                        } else if (self._lines[d] == "end define" && inDefTypeBlock == 1) {
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
                            if (!self._lines) self._lines = [];
                            for (var d = UBound(self._lines); d >= typeLine + 1; d--) {
                                self._lines[d] = self._lines[d - 1];
                            }
                            self._lines[typeLine] = libCode[c];
                            typeLine = typeLine + 1;
                        }
                    } while (!(self.BeginsWith(libCode[c], "!end")));
                } else if (libCode[c] == "!library") {
                    //ignore
                } else if (self.BeginsWith(libCode[c], "!asl-version ")) {
                    //ignore
                } else if (self.BeginsWith(libCode[c], "'")) {
                    //ignore
                } else if (self.BeginsWith(libCode[c], "!QDK")) {
                    ignoreMode = true;
                } else if (self.BeginsWith(libCode[c], "!end")) {
                    ignoreMode = false;
                }
            }
        }
    };
    
    async ParseFile(filename: string, onSuccess: Callback, onFailure: Callback): Promise<void> {
        var hasErrors: boolean = false;
        var skipCheck: boolean = false;
        var defineCount: number = 0;
        var curLine: number = 0;
        this._defineBlockParams = {};
        var self = this;
        
        var doParse = function () {
            skipCheck = false;
            var lastSlashPos: number = 0;
            var slashPos: number = 0;
            var curPos = 1;
            do {
                slashPos = InStrFrom(curPos, filename, "\\");
                if (slashPos == 0) slashPos = InStrFrom(curPos, filename, "/");
                if (slashPos != 0) {
                    lastSlashPos = slashPos;
                }
                curPos = slashPos + 1;
            } while (!(slashPos == 0));
            var filenameNoPath = LCase(Mid(filename, lastSlashPos + 1));
            // Very early versions of Quest didn't perform very good syntax checking of ASL files, so this is
            // for compatibility with games which have non-fatal errors in them.
            skipCheck = (self._skipCheckFile.indexOf(filenameNoPath) !== -1)
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
                        } else if (self.BeginsWith(self._lines[curLine], "end define")) {
                            defineCount = defineCount - 1;
                        }
                        curLine = curLine + 1;
                    } while (!(defineCount == 0));
                    curLine = curLine - 1;
                    // Now, we know that the define section begins at i and ends at
                    // curline. Remember where the section begins and ends:
                    if (!self._defineBlocks) self._defineBlocks = [];
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
            this.GetFileData(filename, function (fileData: string) {
                var aslLines: string[] = fileData.replace(/\r\n/g, "\n").split("\n");
                self._lines = [];
                self._lines[0] = "";
                for (var l = 1; l <= aslLines.length; l++) {
                    self._lines[l] = self.RemoveTabs(aslLines[l - 1]).trim();
                }
                loadLibrariesAndParseFile();
            }, onFailure);
        } else if (LCase(Right(filename, 4)) == ".cas") {
            this.LogASLError("Loading CAS");
            await this.LoadCASFile(filename);
            loadLibrariesAndParseFile();
        } else {
            throw "Unrecognized file extension";
        }
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
        console.log(err);
    }
    async GetParameter(s: string, ctx: Context, convertStringVariables: boolean = true): Promise<string> {
        // Returns the parameters between < and > in a string
        var newParam: string;
        var startPos: number = 0;
        var endPos: number = 0;
        startPos = InStr(s, "<");
        endPos = InStr(s, ">");
        if (startPos == 0 || endPos == 0) {
            this.LogASLError("Expected parameter in '" + this.ReportErrorLine(s) + "'", LogType.WarningError);
            return "";
        }
        var retrParam = Mid(s, startPos + 1, (endPos - startPos) - 1);
        if (convertStringVariables) {
            if (this._gameAslVersion >= 320) {
                newParam = await this.ConvertParameter(
                    await this.ConvertParameter(
                        await this.ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%", ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx);
            } else {
                if (Left(retrParam, 9) != "~Internal") {
                    newParam = await this.ConvertParameter(
                        await this.ConvertParameter(
                            await this.ConvertParameter(
                                await this.ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%", ConvertType.Numeric, ctx), "~", ConvertType.Collectables, ctx), "$", ConvertType.Functions, ctx);
                } else {
                    newParam = retrParam;
                }
            }
        } else {
            newParam = retrParam;
        }
        return this.EvaluateInlineExpressions(newParam);
    }
    GetSimpleParameter(s: string): string {
        // Returns the parameters between < and > in a string
        var newParam: string;
        var startPos: number = 0;
        var endPos: number = 0;
        startPos = InStr(s, "<");
        endPos = InStr(s, ">");
        if (startPos == 0 || endPos == 0) {
            this.LogASLError("Expected parameter in '" + this.ReportErrorLine(s) + "'", LogType.WarningError);
            return "";
        }
        return Mid(s, startPos + 1, (endPos - startPos) - 1);
    }
    AddLine(line: string): void {
        //Adds a line to the game script
        if (!this._lines || this._lines.length == 0) this._lines = [""];
        this._lines.push(line);
    }
    async LoadCASFile(filename: string): Promise<void> {
        var endLineReached: boolean = false;
        var exitTheLoop: boolean = false;
        var textMode: boolean = false;
        var casVersion: number = 0;
        var startCat: number;
        var endCatPos: number = 0;
        var chkVer: string;
        var j: number = 0;
        var curLin: string;
        var textData: Uint8Array;
        var cpos: number = 0;
        var nextLinePos: number = 0;
        var c: string;
        var tl: string;
        var ckw: number;
        var d: number;
        this._lines = [];
        var fileData = await this.GetCASFileData(filename);
        chkVer = String.fromCharCode.apply(null, fileData.slice(0, 7));
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
            startCat = 252; // !startcat
        }
        for (var i = 8; i < fileData.length; i++) {
            if (casVersion == 3 && fileData[i] == startCat) {
                // Read catalog
                this._startCatPos = i;
                endCatPos = fileData.indexOf(253 /* !endcat */, j);                
                this.ReadCatalog(fileData.slice(j + 1, endCatPos));
                this._resourceFile = filename;
                this._resourceOffset = endCatPos + 1;
                i = fileData.length;
                this._casFileData = fileData;
            } else {
                curLin = "";
                endLineReached = false;
                if (textMode) {
                    textData = fileData.slice(i - 1, fileData.indexOf(253, i));
                    cpos = 1;
                    var finished = false;
                    if (textData.length > 0) {
                        do {
                            nextLinePos = textData.indexOf(0, cpos);
                            if (nextLinePos == -1) {
                                nextLinePos = textData.length + 1;
                                finished = true;
                            }
                            tl = this.DecryptString(textData.slice(cpos, nextLinePos));
                            this.AddLine(tl);
                            cpos = nextLinePos + 1;
                        } while (!(finished));
                    }
                    textMode = false;
                    i = fileData.indexOf(253, i);
                }
                j = i;
                do {
                    ckw = fileData[j];
                    c = this.ConvertCasKeyword(ckw);
                    if (c == "\n") {
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
                                    d = fileData[j];
                                    if (d != 0) {
                                        curLin = curLin + this.DecryptItem(d);
                                    } else {
                                        curLin = curLin + "> ";
                                        exitTheLoop = true;
                                    }
                                } while (!(exitTheLoop));
                            } else if (c == "!unknown") {
                                exitTheLoop = false;
                                do {
                                    j = j + 1;
                                    d = fileData[j];
                                    if (d != 254) {
                                        curLin = curLin + Chr(d);
                                    } else {
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
    }
    RemoveTabs(s: string): string {
        return s.replace(/\t/g, "    ");
    }
    DoAddRemove(childId: number, parentId: number, add: boolean, ctx: Context): void {
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
    }
    async DoLook(id: number, ctx: Context, showExamineError: boolean = false, showDefaultDescription: boolean = true): Promise<void> {
        var objectContents: string;
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
        var lookLine: string;
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == "look") {
                foundLook = true;
                await this.ExecuteScript(o.Actions[i].Script, ctx);
                break;
            }
        }
        if (!foundLook) {
            for (var i = 1; i <= o.NumberProperties; i++) {
                if (o.Properties[i].PropertyName == "look") {
                    // do this odd RetrieveParameter stuff to convert any variables
                    await this.Print(await this.GetParameter("<" + o.Properties[i].PropertyValue + ">", ctx), ctx);
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
                        await this.Print(await this.GetParameter(this._lines[i], ctx), ctx);
                    } else {
                        await this.ExecuteScript(lookLine, ctx, id);
                    }
                    foundLook = true;
                }
            }
        }
        if (this._gameAslVersion >= 391) {
            objectContents = await this.ListContents(id, ctx);
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
            // Print "Nothing out of the ordinary" or whatever, but only if we're not going to list
            // any contents.
            if (objectContents == "") {
                await this.PlayerErrorMessage(err, ctx);
            }
        }
        if (objectContents != "" && objectContents != "<script>") {
            await this.Print(objectContents, ctx);
        }
    }
    async DoOpenClose(id: number, open: boolean, showLook: boolean, ctx: Context): Promise<void> {
        if (open) {
            this.AddToObjectProperties("opened", id, ctx);
            if (showLook) {
                await this.DoLook(id, ctx, null, false);
            }
        } else {
            this.AddToObjectProperties("not opened", id, ctx);
        }
        this.UpdateVisibilityInContainers(ctx, this._objs[id].ObjectName);
    }
    EvaluateInlineExpressions(s: string): string {
        // Evaluates in-line expressions e.g. msg <Hello, did you know that 2 + 2 = {2+2}?>
        if (this._gameAslVersion < 391) {
            return s;
        }
        var bracePos: number = 0;
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
                } else {
                    var EndBracePos = InStrFrom(bracePos + 1, s, "}");
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
    }
    async ExecAddRemove(cmd: string, ctx: Context): Promise<void> {
        var childId: number = 0;
        var childName: string;
        var doAdd: boolean = false;
        var sepPos: number = 0;
        var parentId: number = 0;
        var sepLen: number = 0;
        var parentName: string;
        var verb: string = "";
        var action: string;
        var foundAction: boolean = false;
        var actionScript: string = "";
        var propertyExists: boolean = false;
        var textToPrint: string;
        var isContainer: boolean = false;
        var gotObject: boolean = false;
        var childLength: number = 0;
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
            await this.PlayerErrorMessage(PlayerError.BadCommand, ctx);
            this._badCmdBefore = verb;
            return;
        }
        childName = Trim(Mid(cmd, Len(verb) + 2, childLength));
        gotObject = false;
        if (this._gameAslVersion >= 392 && doAdd) {
            childId = await this.Disambiguate(childName, this._currentRoom + ";inventory", ctx);
            if (childId > 0) {
                if (this._objs[childId].ContainerRoom == "inventory") {
                    gotObject = true;
                } else {
                    // Player is not carrying the object they referred to. So, first take the object.
                    await this.Print("(first taking " + this._objs[childId].Article + ")", ctx);
                    // Try to take the object
                    ctx.AllowRealNamesInCommand = true;
                    await this.ExecCommand("take " + this._objs[childId].ObjectName, ctx, false, null, true);
                    if (this._objs[childId].ContainerRoom == "inventory") {
                        gotObject = true;
                    }
                }
                if (!gotObject) {
                    this._badCmdBefore = verb;
                    return;
                }
            } else {
                if (childId != -2) {
                    await this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                }
                this._badCmdBefore = verb;
                return;
            }
        } else {
            childId = await this.Disambiguate(childName, "inventory;" + this._currentRoom, ctx);
            if (childId <= 0) {
                if (childId != -2) {
                    await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                this._badCmdBefore = verb;
                return;
            }
        }
        if (noParentSpecified && doAdd) {
            await this.SetStringContents("quest.error.article", this._objs[childId].Article, ctx);
            await this.PlayerErrorMessage(PlayerError.BadPut, ctx);
            return;
        }
        if (doAdd) {
            action = "add";
        } else {
            action = "remove";
        }
        if (!noParentSpecified) {
            parentName = Trim(Mid(cmd, sepPos + sepLen));
            parentId = await this.Disambiguate(parentName, this._currentRoom + ";inventory", ctx);
            if (parentId <= 0) {
                if (parentId != -2) {
                    await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                this._badCmdBefore = Left(cmd, sepPos + sepLen);
                return;
            }
        } else {
            // Assume the player was referring to the parent that the object is already in,
            // if it is even in an object already
            if (!this.IsYes(this.GetObjectProperty("parent", childId, true, false))) {
                await this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
                return;
            }
            parentId = this.GetObjectIdNoAlias(this.GetObjectProperty("parent", childId, false, true));
        }
        // Check if parent is a container
        isContainer = this.IsYes(this.GetObjectProperty("container", parentId, true, false));
        if (!isContainer) {
            if (doAdd) {
                await this.PlayerErrorMessage(PlayerError.CantPut, ctx);
            } else {
                await this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }
            return;
        }
        // Check object is already held by that parent
        if (this.IsYes(this.GetObjectProperty("parent", childId, true, false))) {
            if (doAdd && LCase(this.GetObjectProperty("parent", childId, false, false)) == LCase(this._objs[parentId].ObjectName)) {
                await this.PlayerErrorMessage(PlayerError.AlreadyPut, ctx);
            }
        }
        // Check parent and child are accessible to player
        var canAccessObject = this.PlayerCanAccessObject(childId);
        if (!canAccessObject.CanAccessObject) {
            if (doAdd) {
                await this.PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessObject.ErrorMsg);
            } else {
                await this.PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessObject.ErrorMsg);
            }
            return;
        }
        var canAccessParent = this.PlayerCanAccessObject(parentId);
        if (!canAccessParent.CanAccessObject) {
            if (doAdd) {
                await this.PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessParent.ErrorMsg);
            } else {
                await this.PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessParent.ErrorMsg);
            }
            return;
        }
        // Check if parent is a closed container
        if (!this.IsYes(this.GetObjectProperty("surface", parentId, true, false)) && !this.IsYes(this.GetObjectProperty("opened", parentId, true, false))) {
            // Not a surface and not open, so can't add to this closed container.
            if (doAdd) {
                await this.PlayerErrorMessage(PlayerError.CantPut, ctx);
            } else {
                await this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
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
            await this.SetStringContents("quest." + LCase(action) + ".object.name", this._objs[childId].ObjectName, ctx);
            await this.ExecuteScript(actionScript, ctx, parentId);
        } else {
            // Now check for a property
            propertyExists = this.IsYes(this.GetObjectProperty(action, parentId, true, false));
            if (!propertyExists) {
                // Show error message
                if (doAdd) {
                    await this.PlayerErrorMessage(PlayerError.CantPut, ctx);
                } else {
                    await this.PlayerErrorMessage(PlayerError.CantRemove, ctx);
                }
            } else {
                textToPrint = this.GetObjectProperty(action, parentId, false, false);
                if (textToPrint == "") {
                    // Show default message
                    if (doAdd) {
                        await this.PlayerErrorMessage(PlayerError.DefaultPut, ctx);
                    } else {
                        await this.PlayerErrorMessage(PlayerError.DefaultRemove, ctx);
                    }
                } else {
                    await this.Print(textToPrint, ctx);
                }
                this.DoAddRemove(childId, parentId, doAdd, ctx);
            }
        }
    }
    ExecAddRemoveScript(parameter: string, add: boolean, ctx: Context): void {
        var childId: number = 0;
        var parentId: number = 0;
        var commandName: string;
        var childName: string;
        var parentName: string = "";
        var scp: number = 0;
        if (add) {
            commandName = "add";
        } else {
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
        } else {
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
        } else {
            this.AddToObjectProperties("not parent", childId, ctx);
            this.UpdateVisibilityInContainers(ctx, this._objs[parentId].ObjectName);
        }
    }
    async ExecOpenClose(cmd: string, ctx: Context): Promise<void> {
        var id: number = 0;
        var name: string;
        var doOpen: boolean = false;
        var isOpen: boolean = false;
        var foundAction: boolean = false;
        var action: string = "";
        var actionScript: string = "";
        var propertyExists: boolean = false;
        var textToPrint: string;
        var isContainer: boolean = false;
        if (this.BeginsWith(cmd, "open ")) {
            action = "open";
            doOpen = true;
        } else if (this.BeginsWith(cmd, "close ")) {
            action = "close";
            doOpen = false;
        }
        name = this.GetEverythingAfter(cmd, action + " ");
        id = await this.Disambiguate(name, this._currentRoom + ";inventory", ctx);
        if (id <= 0) {
            if (id != -2) {
                await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
            }
            this._badCmdBefore = action;
            return;
        }
        // Check if it's even a container
        isContainer = this.IsYes(this.GetObjectProperty("container", id, true, false));
        if (!isContainer) {
            if (doOpen) {
                await this.PlayerErrorMessage(PlayerError.CantOpen, ctx);
            } else {
                await this.PlayerErrorMessage(PlayerError.CantClose, ctx);
            }
            return;
        }
        // Check if it's already open (or closed)
        isOpen = this.IsYes(this.GetObjectProperty("opened", id, true, false));
        if (doOpen && isOpen) {
            // Object is already open
            await this.PlayerErrorMessage(PlayerError.AlreadyOpen, ctx);
            return;
        } else if (!doOpen && !isOpen) {
            // Object is already closed
            await this.PlayerErrorMessage(PlayerError.AlreadyClosed, ctx);
            return;
        }
        // Check if it's accessible, i.e. check it's not itself inside another closed container
        var canAccessObject = this.PlayerCanAccessObject(id);
        if (!canAccessObject.CanAccessObject) {
            if (doOpen) {
                await this.PlayerErrorMessage_ExtendInfo(PlayerError.CantOpen, ctx, canAccessObject.ErrorMsg);
            } else {
                await this.PlayerErrorMessage_ExtendInfo(PlayerError.CantClose, ctx, canAccessObject.ErrorMsg);
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
            await this.ExecuteScript(actionScript, ctx, id);
        } else {
            // Now check for a property
            propertyExists = this.IsYes(this.GetObjectProperty(action, id, true, false));
            if (!propertyExists) {
                // Show error message
                if (doOpen) {
                    await this.PlayerErrorMessage(PlayerError.CantOpen, ctx);
                } else {
                    await this.PlayerErrorMessage(PlayerError.CantClose, ctx);
                }
            } else {
                textToPrint = this.GetObjectProperty(action, id, false, false);
                if (textToPrint == "") {
                    // Show default message
                    if (doOpen) {
                        await this.PlayerErrorMessage(PlayerError.DefaultOpen, ctx);
                    } else {
                        await this.PlayerErrorMessage(PlayerError.DefaultClose, ctx);
                    }
                } else {
                    await this.Print(textToPrint, ctx);
                }
                await this.DoOpenClose(id, doOpen, true, ctx);
            }
        }
    }
    async ExecuteSelectCase(script: string, ctx: Context): Promise<void> {
        // ScriptLine passed will look like this:
        //   select case <whatever> do <!intprocX>
        // with all the case statements in the intproc.
        var afterLine = this.GetAfterParameter(script);
        if (!this.BeginsWith(afterLine, "do <!intproc")) {
            this.LogASLError("No case block specified for '" + script + "'", LogType.WarningError);
            return;
        }
        var blockName = await this.GetParameter(afterLine, ctx);
        var block = this.DefineBlockParam("procedure", blockName);
        var checkValue = await this.GetParameter(script, ctx);
        var caseMatch = false;
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            // Go through all the cases until we find the one that matches
            if (this._lines[i] != "") {
                if (!this.BeginsWith(this._lines[i], "case ")) {
                    this.LogASLError("Invalid line in 'select case' block: '" + this._lines[i] + "'", LogType.WarningError);
                } else {
                    var caseScript = "";
                    if (this.BeginsWith(this._lines[i], "case else ")) {
                        caseMatch = true;
                        caseScript = this.GetEverythingAfter(this._lines[i], "case else ");
                    } else {
                        var thisCase = await this.GetParameter(this._lines[i], ctx);
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
                        } while (!(finished));
                    }
                    if (caseMatch) {
                        await this.ExecuteScript(caseScript, ctx);
                        return;
                    }
                }
            }
        }
    }
    async ExecVerb(cmd: string, ctx: Context, libCommands: boolean = false): Promise<boolean> {
        var gameBlock: DefineBlock;
        var foundVerb = false;
        var verbProperty: string = "";
        var script: string = "";
        var verbsList: string;
        var thisVerb: string = "";
        var scp: number = 0;
        var id: number = 0;
        var verbObject: string = "";
        var verbTag: string;
        var thisScript: string = "";
        if (!libCommands) {
            verbTag = "verb ";
        } else {
            verbTag = "lib verb ";
        }
        gameBlock = this.GetDefineBlock("game");
        for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], verbTag)) {
                verbsList = await this.GetParameter(this._lines[i], ctx);
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
                // Now let's see if this matches:
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
                } while (!(scp == 0 || Trim(verbsList) == "" || foundVerb));
                if (foundVerb) {
                    break;
                }
            }
        }
        if (foundVerb) {
            id = await this.Disambiguate(verbObject, "inventory;" + this._currentRoom, ctx);
            if (id < 0) {
                if (id != -2) {
                    await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                this._badCmdBefore = thisVerb;
            } else {
                await this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
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
                    await this.ExecuteScript(thisScript, ctx, id);
                }
                if (!foundAction) {
                    // Check properties for a message
                    for (var i = 1; i <= o.NumberProperties; i++) {
                        if (LCase(o.Properties[i].PropertyName) == verbProperty) {
                            foundAction = true;
                            await this.Print(o.Properties[i].PropertyValue, ctx);
                            break;
                        }
                    }
                }
                if (!foundAction) {
                    // Execute the default script from the verb definition
                    await this.ExecuteScript(script, ctx);
                }
            }
        }
        return foundVerb;
    }
    ExpressionHandler(expr: string): ExpressionResult {
        var openBracketPos: number = 0;
        var endBracketPos: number = 0;
        var res: ExpressionResult = new ExpressionResult();
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
        // Split expression into elements, e.g.:
        //       2 + 3 * 578.2 / 36
        //       E O E O EEEEE O EE      where E=Element, O=Operator
        var numElements = 1;
        var elements: string[];
        elements = ["", ""];
        var numOperators = 0;
        var operators: string[] = [];
        var newElement: boolean = false;
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
                    } else {
                        newElement = true;
                    }
                    break;
                default:
                    newElement = false;
                    break;
            }
            if (newElement) {
                numElements = numElements + 1;
                elements[numElements] = "";
                numOperators = numOperators + 1;
                operators[numOperators] = Mid(expr, i, 1);
            } else {
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
                var result: number = 0;
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
                if (!operators) operators = [];
                if (!elements) elements = [];
            }
        } while (!(opNum == 0 || numOperators == 0));
        res.Success = ExpressionSuccess.OK;
        res.Result = elements[1];
        return res;
    }
    async ListContents(id: number, ctx: Context): Promise<string> {
        // Returns a formatted list of the contents of a container.
        // If the list action causes a script to be run instead, ListContents
        // returns "<script>"
        var contentsIDs: number[] = [];
        if (!this.IsYes(this.GetObjectProperty("container", id, true, false))) {
            return "";
        }
        if (!this.IsYes(this.GetObjectProperty("opened", id, true, false)) && !this.IsYes(this.GetObjectProperty("transparent", id, true, false)) && !this.IsYes(this.GetObjectProperty("surface", id, true, false))) {
            // Container is closed, so return "list closed" property if there is one.
            if (await this.DoAction(id, "list closed", ctx, false)) {
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
                    if (!contentsIDs) contentsIDs = [];
                    contentsIDs[numContents] = i;
                }
            }
        }
        var contents = "";
        if (numContents > 0) {
            // Check if list property is set.
            if (await this.DoAction(id, "list", ctx, false)) {
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
        if (await this.DoAction(id, "list empty", ctx, false)) {
            return "<script>";
        } else {
            return this.GetObjectProperty("list empty", id, false, false);
        }
    }
    ObscureNumericExps(s: string): string {
        // Obscures + or - next to E in Double-type variables with exponents
        //   e.g. 2.345E+20 becomes 2.345EX20
        // This stops huge numbers breaking parsing of maths functions
        var ep: number = 0;
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
    }
    ProcessListInfo(line: string, id: number): void {
        var listInfo: TextAction = new TextAction();
        var propName: string = "";
        if (this.BeginsWith(line, "list closed <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetSimpleParameter(line);
            propName = "list closed";
        } else if (Trim(line) == "list closed off") {
            // default for list closed is off anyway
            return;
        } else if (this.BeginsWith(line, "list closed")) {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = this.GetEverythingAfter(line, "list closed");
            propName = "list closed";
        } else if (this.BeginsWith(line, "list empty <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetSimpleParameter(line);
            propName = "list empty";
        } else if (Trim(line) == "list empty off") {
            // default for list empty is off anyway
            return;
        } else if (this.BeginsWith(line, "list empty")) {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = this.GetEverythingAfter(line, "list empty");
            propName = "list empty";
        } else if (Trim(line) == "list off") {
            this.AddToObjectProperties("not list", id, this._nullContext);
            return;
        } else if (this.BeginsWith(line, "list <")) {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = this.GetSimpleParameter(line);
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
                this.AddToObjectActions("<" + propName + "> " + listInfo.Data, id);
            }
        }
    }
    GetHTMLColour(colour: string, defaultColour: string): string {
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
    }
    async DestroyExit(exitData: string, ctx: Context): Promise<void> {
        var fromRoom: string = "";
        var toRoom: string = "";
        var roomId: number = 0;
        var exitId: number = 0;
        var scp = InStr(exitData, ";");
        if (scp == 0) {
            this.LogASLError("No exit name specified in 'destroy exit <" + exitData + ">'");
            return;
        }
        var roomExit: RoomExit;
        if (this._gameAslVersion >= 410) {
            roomExit = this.FindExit(exitData);
            if (roomExit == null) {
                this.LogASLError("Can't find exit in 'destroy exit <" + exitData + ">'");
                return;
            }
            roomExit.GetParent().RemoveExit(roomExit);
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
                if (!r.Places) r.Places = [];
                r.NumberPlaces = r.NumberPlaces - 1;
            }
        }
        // Update quest.* vars and obj list
        await this.ShowRoomInfo(this._currentRoom, ctx, true);
        this.UpdateObjectList(ctx);
        this.AddToChangeLog("room " + fromRoom, "destroy exit " + toRoom);
    }
    DoClear(): void {
        this._player.ClearScreen();
    }
    async ExecuteFlag(line: string, ctx: Context): Promise<void> {
        var propertyString: string = "";
        if (this.BeginsWith(line, "on ")) {
            propertyString = await this.GetParameter(line, ctx);
        } else if (this.BeginsWith(line, "off ")) {
            propertyString = "not " + await this.GetParameter(line, ctx);
        }
        // Game object always has ObjID 1
        this.AddToObjectProperties(propertyString, 1, ctx);
    }
    ExecuteIfFlag(flag: string): boolean {
        // Game ObjID is 1
        return this.GetObjectProperty(flag, 1, true) == "yes";
    }
    async ExecuteIncDec(line: string, ctx: Context): Promise<void> {
        var variable: string;
        var change: number = 0;
        var param = await this.GetParameter(line, ctx);
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
        await this.SetNumericVariableContents(arrayIndex.Name, value, ctx, arrayIndex.Index);
    }
    ExtractFile(file: string): string {
        var length: number = 0;
        var startPos: number = 0;
        var extracted: boolean = false;
        var resId: number = 0;
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
            //var fileData = Mid(this._casFileData, startPos, length);
            // Write file to temp dir
            //System.IO.File.WriteAllText(fileName, fileData, System.Text.Encoding.GetEncoding(1252));
            this._resources[resId].Extracted = true;
        }
        //return fileName;
        return null;
    }
    AddObjectAction(id: number, name: string, script: string, noUpdate: boolean = false): void {
        // Use NoUpdate in e.g. AddToGiveInfo, otherwise ObjectActionUpdate will call
        // AddToGiveInfo again leading to a big loop
        var actionNum: number = 0;
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
            if (!o.Actions) o.Actions = [];
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }
        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;
        this.ObjectActionUpdate(id, name, script, noUpdate);
    }
    AddToChangeLog(appliesTo: string, changeData: string): void {
        this._gameChangeData.NumberChanges = this._gameChangeData.NumberChanges + 1;
        if (!this._gameChangeData.ChangeData) this._gameChangeData.ChangeData = [];
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges] = new ChangeType();
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges].AppliesTo = appliesTo;
        this._gameChangeData.ChangeData[this._gameChangeData.NumberChanges].Change = changeData;
    }
    AddToObjectChangeLog(appliesToType: AppliesTo, appliesTo: string, element: string, changeData: string): void {
        var changeLog: ChangeLog;
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
                return;
            } else {
                giveType = GiveType.GiveToSomething;
                actionName = "give to ";
            }
        } else {
            if (this.BeginsWith(giveData, "anything ")) {
                o.GiveAnything = this.GetEverythingAfter(giveData, "anything ");
                this.AddObjectAction(id, "give anything", o.GiveAnything, true);
                return;
            } else {
                giveType = GiveType.GiveSomethingTo;
                actionName = "give ";
            }
        }
        if (Left(Trim(giveData), 1) == "<") {
            var name = this.GetSimpleParameter(giveData);
            var dataId: number = 0;
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
                if (!o.GiveData) o.GiveData = [];
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
    AddToObjectActions(actionInfo: string, id: number) {
        var actionNum: number = 0;
        var foundExisting = false;
        var name = LCase(this.GetSimpleParameter(actionInfo));
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
            if (!o.Actions) o.Actions = [];
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
            if (endPos == 0) {
                endPos = Len(altNames) + 1;
            }
            var curName = Trim(Left(altNames, endPos - 1));
            if (curName != "") {
                o.NumberAltNames = o.NumberAltNames + 1;
                if (!o.AltNames) o.AltNames = [];
                o.AltNames[o.NumberAltNames] = curName;
            }
            altNames = Mid(altNames, endPos + 1);
        } while (!(Trim(altNames) == ""));
    }
    AddToObjectProperties(propertyInfo: string, id: number, ctx: Context): void {
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
            var name: string;
            var value: string;
            var num: number = 0;
            if (info == "") {
                break;
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
                if (!o.Properties) o.Properties = [];
                o.Properties[o.NumberProperties] = new PropertyType();
                num = o.NumberProperties;
            }
            if (falseProperty) {
                o.Properties[num].PropertyName = "";
            } else {
                o.Properties[num].PropertyName = name;
                o.Properties[num].PropertyValue = value;
            }
            this.AddToObjectChangeLog(AppliesTo.Object, this._objs[id].ObjectName, name, "properties " + info);
            switch (name) {
                case "alias":
                    if (o.IsRoom) {
                        this._rooms[o.CorresRoomId].RoomAlias = value;
                    } else {
                        o.ObjectAlias = value;
                    }
                    if (this._gameFullyLoaded) {
                        this.UpdateObjectList(ctx);
                        this.UpdateInventory(ctx);
                    }
                    break;
                case "prefix":
                    if (o.IsRoom) {
                        this._rooms[o.CorresRoomId].Prefix = value;
                    } else {
                        if (value != "") {
                            o.Prefix = value + " ";
                        } else {
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
                    } else {
                        o.Exists = false;
                    }
                    if (this._gameFullyLoaded) {
                        this.UpdateObjectList(ctx);
                    }
                    break;
                case "invisible":
                    if (falseProperty) {
                        o.Visible = true;
                    } else {
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
                        } else {
                            if (value == "") {
                                o.Take.Type = TextActionType.Default;
                            } else {
                                o.Take.Type = TextActionType.Text;
                                o.Take.Data = value;
                            }
                        }
                    }
                    break;
            }
        } while (Len(Trim(propertyInfo)) != 0);
    }
    AddToUseInfo(id: number, useData: string): void {
        var useType: UseType;
        var o = this._objs[id];
        if (this.BeginsWith(useData, "on ")) {
            useData = this.GetEverythingAfter(useData, "on ");
            if (this.BeginsWith(useData, "anything ")) {
                o.UseOnAnything = this.GetEverythingAfter(useData, "anything ");
                return;
            } else {
                useType = UseType.UseOnSomething;
            }
        } else {
            if (this.BeginsWith(useData, "anything ")) {
                o.UseAnything = this.GetEverythingAfter(useData, "anything ");
                return;
            } else {
                useType = UseType.UseSomethingOn;
            }
        }
        if (Left(Trim(useData), 1) == "<") {
            var objectName = this.GetSimpleParameter(useData);
            var dataId: number = 0;
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
                if (!o.UseData) o.UseData = [];
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
    async ConvertVarsIn(s: string, ctx: Context): Promise<string> {
        return await this.GetParameter("<" + s + ">", ctx);
    }
    DisambObjHere(ctx: Context, id: number, firstPlace: string, twoPlaces: boolean = false, secondPlace: string = "", isExit: boolean = false): boolean {
        var isSeen: boolean = false;
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
        
        if (((!twoPlaces && (LCase(this._objs[id].ContainerRoom) == LCase(firstPlace) || firstPlace == "")) ||
            (twoPlaces && (LCase(this._objs[id].ContainerRoom) == LCase(firstPlace) || LCase(this._objs[id].ContainerRoom) == LCase(secondPlace)))) &&
            this._objs[id].Exists && this._objs[id].IsExit == isExit) {

            if (!onlySeen) {
                return true;
            }
            return isSeen;
        }
        return false;
    }
    ExecClone(cloneString: string, ctx: Context): void {
        var id: number = 0;
        var newName: string;
        var cloneTo: string;
        var scp = InStr(cloneString, ";");
        if (scp == 0) {
            this.LogASLError("No new object name specified in 'clone <" + cloneString + ">", LogType.WarningError);
            return;
        } else {
            var objectToClone = Trim(Left(cloneString, scp - 1));
            id = this.GetObjectIdNoAlias(objectToClone);
            var SC2 = InStrFrom(scp + 1, cloneString, ";");
            if (SC2 == 0) {
                cloneTo = this._objs[id].ContainerRoom;
                newName = Trim(Mid(cloneString, scp + 1));
            } else {
                cloneTo = Trim(Mid(cloneString, SC2 + 1));
                newName = Trim(Mid(cloneString, scp + 1, (SC2 - scp) - 1));
            }
        }
        this._numberObjs = this._numberObjs + 1;
        if (!this._objs) this._objs = [];
        this._objs[this._numberObjs] = new ObjectType();
        this._objs[this._numberObjs] = this._objs[id];
        this._objs[this._numberObjs].ContainerRoom = cloneTo;
        this._objs[this._numberObjs].ObjectName = newName;
        if (this._objs[id].IsRoom) {
            // This is a room so create the corresponding room as well
            this._numberRooms = this._numberRooms + 1;
            if (!this._rooms) this._rooms = [];
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
    async ExecOops(correction: string, ctx: Context): Promise<void> {
        if (this._badCmdBefore != "") {
            if (this._badCmdAfter == "") {
                await this.ExecCommand(this._badCmdBefore + " " + correction, ctx, false);
            } else {
                await this.ExecCommand(this._badCmdBefore + " " + correction + " " + this._badCmdAfter, ctx, false);
            }
        }
    }
    ExecType(typeData: string, ctx: Context): void {
        var id: number = 0;
        var found: boolean = false;
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
        if (!o.TypesIncluded) o.TypesIncluded = [];
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
            if (!o.TypesIncluded) o.TypesIncluded = [];
            o.TypesIncluded[o.NumberTypesIncluded] = propertyData.TypesIncluded[i];
        }
    }
    ExecuteIfAction(actionData: string): boolean {
        var id: number = 0;
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
    }
    ExecuteIfType(typeData: string): boolean {
        var id: number = 0;
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
            result.Index = this.GetNumericContents(data, ctx);
        }
        result.Name = Left(varName, beginPos - 1);
        return result;
    }
    async Disambiguate(name: string, containedIn: string, ctx: Context, isExit: boolean = false): Promise<number> {
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
        var idNumbers: number[] = [];
        var firstPlace: string;
        var secondPlace: string = "";
        var twoPlaces: boolean = false;
        var descriptionText: string[];
        var validNames: string[];
        var numValidNames: number = 0;
        name = Trim(name);
        await this.SetStringContents("quest.lastobject", "", ctx);
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
                        await this.SetStringContents("quest.lastobject", this._objs[i].ObjectName, ctx);
                        return i;
                    }
                }
            }
        }
        // If player uses "it", "them" etc. as name:
        if (name == "it" || name == "them" || name == "this" || name == "those" || name == "these" || name == "that") {
            await this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Inanimate && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                await this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                await this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
                return -2;
            }
        } else if (name == "him") {
            await this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Male && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                await this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                await this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
                return -2;
            }
        } else if (name == "her") {
            await this.SetStringContents("quest.error.pronoun", name, ctx);
            if (this._lastIt != 0 && this._lastItMode == ItType.Female && this.DisambObjHere(ctx, this._lastIt, firstPlace, twoPlaces, secondPlace)) {
                await this.SetStringContents("quest.lastobject", this._objs[this._lastIt].ObjectName, ctx);
                return this._lastIt;
            } else {
                await this.PlayerErrorMessage(PlayerError.BadPronoun, ctx);
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
                        if (!idNumbers) idNumbers = [];
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
                    var thisName: string;
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
                        if (!idNumbers) idNumbers = [];
                        idNumbers[numberCorresIds] = i;
                    }
                }
            }
        }
        if (numberCorresIds == 1) {
            await this.SetStringContents("quest.lastobject", this._objs[idNumbers[1]].ObjectName, ctx);
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
        } else if (numberCorresIds > 1) {
            descriptionText = [];
            var question = "Please select which " + name + " you mean:";
            await this.Print("- |i" + question + "|xi", ctx);
            var menuItems: StringDictionary = {};
            for (var i = 1; i <= numberCorresIds; i++) {
                descriptionText[i] = this._objs[idNumbers[i]].Detail;
                if (descriptionText[i] == "") {
                    if (this._objs[idNumbers[i]].Prefix == "") {
                        descriptionText[i] = this._objs[idNumbers[i]].ObjectAlias;
                    } else {
                        descriptionText[i] = this._objs[idNumbers[i]].Prefix + this._objs[idNumbers[i]].ObjectAlias;
                    }
                }
                menuItems[i.toString()] = descriptionText[i];
            }
            var mnu: MenuData = new MenuData(question, menuItems, false);
            var response: string = await this.ShowMenu(mnu);
            this._choiceNumber = parseInt(response);
            await this.SetStringContents("quest.lastobject", this._objs[idNumbers[this._choiceNumber]].ObjectName, ctx);
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
            await this.Print("- " + descriptionText[this._choiceNumber] + "|n", ctx);
            return idNumbers[this._choiceNumber];
        }
        this._thisTurnIt = this._lastIt;
        await this.SetStringContents("quest.error.object", name, ctx);
        return -1;
    }
    async DisplayStatusVariableInfo(id: number, type: VarType, ctx: Context): Promise<string> {
        var displayData: string = "";
        var ep: number = 0;
        if (type == VarType.String) {
            displayData = await this.ConvertVarsIn(this._stringVariable[id].DisplayString, ctx);
            ep = InStr(displayData, "!");
            if (ep != 0) {
                displayData = Left(displayData, ep - 1) + this._stringVariable[id].VariableContents[0] + Mid(displayData, ep + 1);
            }
        } else if (type == VarType.Numeric) {
            if (this._numericVariable[id].NoZeroDisplay && Val(this._numericVariable[id].VariableContents[0]) == 0) {
                return "";
            }
            displayData = await this.ConvertVarsIn(this._numericVariable[id].DisplayString, ctx);
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
                } else {
                    displayData = beforeStar + afterStar;
                }
            }
        }
        return displayData;
    }
    async DoAction(id: number, action: string, ctx: Context, logError: boolean = true): Promise<boolean> {
        var found: boolean = false;
        var script: string = "";
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
        var newCtx: Context = this.CopyContext(ctx);
        newCtx.CallingObjectId = id;
        await this.ExecuteScript(script, newCtx, id);
        return true;
    }
    HasAction(id: number, action: string): boolean {
        var o = this._objs[id];
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == LCase(action)) {
                return true;
            }
        }
        return false;
    }
    async ExecForEach(scriptLine: string, ctx: Context): Promise<void> {
        var inLocation: string;
        var scriptToRun: string;
        var isExit: boolean = false;
        var isRoom: boolean = false;
        if (this.BeginsWith(scriptLine, "object ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "object ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each object " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                return;
            }
        } else if (this.BeginsWith(scriptLine, "exit ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "exit ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each exit " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                return;
            }
            isExit = true;
        } else if (this.BeginsWith(scriptLine, "room ")) {
            scriptLine = this.GetEverythingAfter(scriptLine, "room ");
            if (!this.BeginsWith(scriptLine, "in ")) {
                this.LogASLError("Expected 'in' in 'for each room " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
                return;
            }
            isRoom = true;
        } else {
            this.LogASLError("Unknown type in 'for each " + this.ReportErrorLine(scriptLine) + "'", LogType.WarningError);
            return;
        }
        scriptLine = this.GetEverythingAfter(scriptLine, "in ");
        if (this.BeginsWith(scriptLine, "game ")) {
            inLocation = "";
            scriptToRun = this.GetEverythingAfter(scriptLine, "game ");
        } else {
            inLocation = LCase(await this.GetParameter(scriptLine, ctx));
            var bracketPos = InStr(scriptLine, ">");
            scriptToRun = Trim(Mid(scriptLine, bracketPos + 1));
        }
        for (var i = 1; i <= this._numberObjs; i++) {
            if (inLocation == "" || LCase(this._objs[i].ContainerRoom) == inLocation) {
                if (this._objs[i].IsRoom == isRoom && this._objs[i].IsExit == isExit) {
                    await this.SetStringContents("quest.thing", this._objs[i].ObjectName, ctx);
                    await this.ExecuteScript(scriptToRun, ctx);
                }
            }
        }
    }
    async ExecuteAction(data: string, ctx: Context): Promise<void> {
        var actionName: string;
        var script: string;
        var actionNum: number = 0;
        var id: number = 0;
        var foundExisting = false;
        var foundObject = false;
        var param = await this.GetParameter(data, ctx);
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
            if (!o.Actions) o.Actions = [];
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }
        o.Actions[actionNum].ActionName = actionName;
        o.Actions[actionNum].Script = script;
        this.ObjectActionUpdate(id, actionName, script);
    }
    async ExecuteCondition(condition: string, ctx: Context): Promise<boolean> {
        var result: boolean = false;
        var thisNot: boolean = false;
        if (this.BeginsWith(condition, "not ")) {
            thisNot = true;
            condition = this.GetEverythingAfter(condition, "not ");
        } else {
            thisNot = false;
        }
        if (this.BeginsWith(condition, "got ")) {
            result = this.ExecuteIfGot(await this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "has ")) {
            result = this.ExecuteIfHas(await this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "ask ")) {
            result = await this.ExecuteIfAsk(await this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "is ")) {
            result = this.ExecuteIfIs(await this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "here ")) {
            result = this.ExecuteIfHere(await this.GetParameter(condition, ctx), ctx);
        } else if (this.BeginsWith(condition, "exists ")) {
            result = this.ExecuteIfExists(await this.GetParameter(condition, ctx), false);
        } else if (this.BeginsWith(condition, "real ")) {
            result = this.ExecuteIfExists(await this.GetParameter(condition, ctx), true);
        } else if (this.BeginsWith(condition, "property ")) {
            result = this.ExecuteIfProperty(await this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "action ")) {
            result = this.ExecuteIfAction(await this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "type ")) {
            result = this.ExecuteIfType(await this.GetParameter(condition, ctx));
        } else if (this.BeginsWith(condition, "flag ")) {
            result = this.ExecuteIfFlag(await this.GetParameter(condition, ctx));
        }
        if (thisNot) {
            result = !result;
        }
        return result;
    }
    async ExecuteConditions(list: string, ctx: Context): Promise<boolean> {
        var conditions: string[];
        var numConditions = 0;
        var operations: string[];
        var obscuredConditionList = this.ObliterateParameters(list);
        var pos = 1;
        var isFinalCondition = false;
        do {
            numConditions = numConditions + 1;
            if (!conditions) conditions = [];
            if (!operations) operations = [];
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
            var thisResult = await this.ExecuteCondition(conditions[i], ctx);
            if (operations[i - 1] == "AND") {
                result = thisResult && result;
            } else if (operations[i - 1] == "OR") {
                result = thisResult || result;
            }
        }
        return result;
    }
    async ExecuteCreate(data: string, ctx: Context): Promise<void> {
        var newName: string;
        if (this.BeginsWith(data, "room ")) {
            newName = await this.GetParameter(data, ctx);
            this._numberRooms = this._numberRooms + 1;
            if (!this._rooms) this._rooms = [];
            this._rooms[this._numberRooms] = new RoomType();
            this._rooms[this._numberRooms].RoomName = newName;
            this._numberObjs = this._numberObjs + 1;
            if (!this._objs) this._objs = [];
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
        } else if (this.BeginsWith(data, "object ")) {
            var paramData = await this.GetParameter(data, ctx);
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
            if (!this._objs) this._objs = [];
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
            await this.ExecuteCreateExit(data, ctx);
        }
    }
    async ExecuteCreateExit(data: string, ctx: Context): Promise<void> {
        var scrRoom: string;
        var destRoom: string = "";
        var destId: number = 0;
        var exitData = this.GetEverythingAfter(data, "exit ");
        var newName = await this.GetParameter(data, ctx);
        var scp = InStr(newName, ";");
        if (this._gameAslVersion < 410) {
            if (scp == 0) {
                this.LogASLError("No exit destination given in 'create exit " + exitData + "'", LogType.WarningError);
                return;
            }
        }
        if (scp == 0) {
            scrRoom = Trim(newName);
        } else {
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
            } else {
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
        var saveData: string;
        if (paramPos == 0) {
            saveData = exitData;
        } else {
            saveData = Left(exitData, paramPos - 1);
            // We do this so the changelog doesn't contain unconverted variable names
            saveData = saveData + "<" + await this.GetParameter(exitData, ctx) + ">";
        }
        this.AddToChangeLog("room " + this._rooms[srcId].RoomName, "exit " + saveData);
        var r = this._rooms[srcId];
        if (this._gameAslVersion >= 410) {
            await r.Exits.AddExitFromCreateScript(exitData, ctx);
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
                if (!r.Places) r.Places = [];
                r.Places[r.NumberPlaces] = new PlaceType();
                r.Places[r.NumberPlaces].PlaceName = destRoom;
            } else {
                this.LogASLError("Invalid direction in 'create exit " + exitData + "'", LogType.WarningError);
            }
        }
        if (!this._gameLoading) {
            // Update quest.doorways variables
            await this.ShowRoomInfo(this._currentRoom, ctx, true);
            this.UpdateObjectList(ctx);
            if (this._gameAslVersion < 410) {
                if (this._currentRoom == this._rooms[srcId].RoomName) {
                    await this.UpdateDoorways(srcId, ctx);
                } else if (this._currentRoom == this._rooms[destId].RoomName) {
                    await this.UpdateDoorways(destId, ctx);
                }
            } else {
                // Don't have DestID in ASL410 CreateExit code, so just UpdateDoorways
                // for current room anyway.
                await this.UpdateDoorways(this.GetRoomId(this._currentRoom, ctx), ctx);
            }
        }
    }
    async ExecDrop(obj: string, ctx: Context): Promise<void> {
        var found: boolean = false;
        var parentId: number = 0;
        var id: number = 0;
        id = await this.Disambiguate(obj, "inventory", ctx);
        if (id > 0) {
            found = true;
        } else {
            found = false;
        }
        if (!found) {
            if (id != -2) {
                if (this._gameAslVersion >= 391) {
                    await this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                } else {
                    await this.PlayerErrorMessage(PlayerError.BadDrop, ctx);
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
        var dropStatement: string = "";
        for (var i = this._objs[id].DefinitionSectionStart; i <= this._objs[id].DefinitionSectionEnd; i++) {
            if (this.BeginsWith(this._lines[i], "drop ")) {
                dropStatement = this.GetEverythingAfter(this._lines[i], "drop ");
                dropFound = true;
                break;
            }
        }
        await this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
        if (!dropFound || this.BeginsWith(dropStatement, "everywhere")) {
            if (isInContainer) {
                // So, we want to drop an object that's in a container or surface. So first
                // we have to remove the object from that container.
                var parentDisplayName: string;
                if (this._objs[parentId].ObjectAlias != "") {
                    parentDisplayName = this._objs[parentId].ObjectAlias;
                } else {
                    parentDisplayName = this._objs[parentId].ObjectName;
                }
                await this.Print("(first removing " + this._objs[id].Article + " from " + parentDisplayName + ")", ctx);
                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                await this.ExecCommand("remove " + this._objs[id].ObjectName, ctx, false, null, true);
                if (this.GetObjectProperty("parent", id, false, false) != "") {
                    // removing the object failed
                    return;
                }
            }
        }
        if (!dropFound) {
            await this.PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
            await this.PlayerItem(this._objs[id].ObjectName, false, ctx);
        } else {
            if (this.BeginsWith(dropStatement, "everywhere")) {
                await this.PlayerItem(this._objs[id].ObjectName, false, ctx);
                if (InStr(dropStatement, "<") != 0) {
                    await this.Print(await this.GetParameter(dropStatement, ctx), ctx);
                } else {
                    await this.PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
                }
            } else if (this.BeginsWith(dropStatement, "nowhere")) {
                if (InStr(dropStatement, "<") != 0) {
                    await this.Print(await this.GetParameter(dropStatement, ctx), ctx);
                } else {
                    await this.PlayerErrorMessage(PlayerError.CantDrop, ctx);
                }
            } else {
                await this.ExecuteScript(dropStatement, ctx);
            }
        }
    }
    async ExecExamine(command: string, ctx: Context): Promise<void> {
        var item = LCase(Trim(this.GetEverythingAfter(command, "examine ")));
        if (item == "") {
            await this.PlayerErrorMessage(PlayerError.BadExamine, ctx);
            this._badCmdBefore = "examine";
            return;
        }
        var id = await this.Disambiguate(item, this._currentRoom + ";inventory", ctx);
        if (id <= 0) {
            if (id != -2) {
                await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
            }
            this._badCmdBefore = "examine";
            return;
        }
        var o = this._objs[id];
        // Find "examine" action:
        for (var i = 1; i <= o.NumberActions; i++) {
            if (o.Actions[i].ActionName == "examine") {
                await this.ExecuteScript(o.Actions[i].Script, ctx, id);
                return;
            }
        }
        // Find "examine" property:
        for (var i = 1; i <= o.NumberProperties; i++) {
            if (o.Properties[i].PropertyName == "examine") {
                await this.Print(o.Properties[i].PropertyValue, ctx);
                return;
            }
        }
        // Find "examine" tag:
        for (var i = o.DefinitionSectionStart + 1; i <= this._objs[id].DefinitionSectionEnd - 1; i++) {
            if (this.BeginsWith(this._lines[i], "examine ")) {
                var action = Trim(this.GetEverythingAfter(this._lines[i], "examine "));
                if (Left(action, 1) == "<") {
                    await this.Print(await this.GetParameter(this._lines[i], ctx), ctx);
                } else {
                    await this.ExecuteScript(action, ctx, id);
                }
                return;
            }
        }
        await this.DoLook(id, ctx, true);
    }
    ExecMoveThing(data: string, type: Thing, ctx: Context): void {
        var scp = InStr(data, ";");
        var name = Trim(Left(data, scp - 1));
        var place = Trim(Mid(data, scp + 1));
        this.MoveThing(name, place, type, ctx);
    }
    ExecProperty(data: string, ctx: Context): void {
        var id: number = 0;
        var found: boolean = false;
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
    }
    async ExecuteDo(procedureName: string, ctx: Context): Promise<void> {
        var newCtx: Context = this.CopyContext(ctx);
        var numParameters = 0;
        var useNewCtx: boolean = false;
        if (this._gameAslVersion >= 392 && Left(procedureName, 8) == "!intproc") {
            // If "do" procedure is run in a new context, context info is not passed to any nested
            // script blocks in braces.
            useNewCtx = false;
        } else {
            useNewCtx = true;
        }
        if (this._gameAslVersion >= 284) {
            var obp = InStr(procedureName, "(");
            var cbp: number = 0;
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
                    if (!newCtx.Parameters) newCtx.Parameters = [];
                    newCtx.Parameters[numParameters] = Trim(Mid(parameters, pos, scp - pos));
                    pos = scp + 1;
                } while (!(pos >= Len(parameters)));
            }
        }
        var block = this.DefineBlockParam("procedure", procedureName);
        if (block.StartLine == 0 && block.EndLine == 0) {
            this.LogASLError("No such procedure " + procedureName, LogType.WarningError);
        } else {
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                if (!useNewCtx) {
                    await this.ExecuteScript(this._lines[i], ctx);
                } else {
                    await this.ExecuteScript(this._lines[i], newCtx);
                    ctx.DontProcessCommand = newCtx.DontProcessCommand;
                }
            }
        }
    }
    async ExecuteDoAction(data: string, ctx: Context): Promise<void> {
        var id: number = 0;
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
        await this.DoAction(id, actionName, ctx);
    }
    ExecuteIfHere(obj: string, ctx: Context): boolean {
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
    }
    ExecuteIfExists(obj: string, realOnly: boolean): boolean {
        var result: boolean = false;
        var errorReport = false;
        var scp: number = 0;
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
    }
    ExecuteIfProperty(data: string): boolean {
        var id: number = 0;
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
    }
    async ExecuteRepeat(data: string, ctx: Context): Promise<void> {
        var repeatWhileTrue: boolean = false;
        var repeatScript: string = "";
        var bracketPos: number = 0;
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
            return;
        }
        var pos = 1;
        do {
            bracketPos = InStrFrom(pos, data, ">");
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
            if (await this.ExecuteConditions(conditions, ctx) == repeatWhileTrue) {
                await this.ExecuteScript(repeatScript, ctx);
            } else {
                finished = true;
            }
        } while (!(finished || this._gameFinished));
    }
    ExecuteSetCollectable(param: string, ctx: Context): void {
        var val: number = 0;
        var id: number = 0;
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
        this.UpdateCollectables();
    }
    async ExecuteWait(waitLine: string, ctx: Context): Promise<void> {
        if (waitLine != "") {
            await this.Print(await this.GetParameter(waitLine, ctx), ctx);
        } else {
            if (this._gameAslVersion >= 410) {
                await this.PlayerErrorMessage(PlayerError.DefaultWait, ctx);
            } else {
                await this.Print("|nPress a key to continue...", ctx);
            }
        }
        await this.DoWait();
    }
    InitFileData(fileData: string): void {
        this._fileData = fileData;
        this._fileDataPos = 1;
    }
    GetNextChunk(): string {
        var nullPos = InStrFrom(this._fileDataPos, this._fileData, Chr(0));
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
        var name = LCase(this.GetSimpleParameter(actionInfo));
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
    async GetObjectId(name: string, ctx: Context, room: string = ""): Promise<number> {
        var id: number = 0;
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
            id = await this.Disambiguate(name, room, ctx);
            if (id > 0) {
                found = true;
            }
        }
        if (found) {
            return id;
        }
        return -1;
    }
    GetObjectIdNoAlias(name: string): number {
        for (var i = 1; i <= this._numberObjs; i++) {
            if (LCase(this._objs[i].ObjectName) == LCase(name)) {
                return i;
            }
        }
        return 0;
    }
    GetObjectProperty(name: string, id: number, existsOnly: boolean = false, logError: boolean = true): string {
        var result: string = "";
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
    }
    GetPropertiesInType(type: string, err: boolean = true): PropertiesActions {
        var blockId: number = 0;
        var propertyList: PropertiesActions = new PropertiesActions();
        var found = false;
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define type")) {
                if (LCase(this.GetSimpleParameter(this._lines[this._defineBlocks[i].StartLine])) == LCase(type)) {
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
                var typeName = LCase(this.GetSimpleParameter(this._lines[i]));
                var newProperties = this.GetPropertiesInType(typeName);
                propertyList.Properties = propertyList.Properties + newProperties.Properties;
                if (!propertyList.Actions) propertyList.Actions = [];
                for (var j = propertyList.NumberActions + 1; j <= propertyList.NumberActions + newProperties.NumberActions; j++) {
                    propertyList.Actions[j] = new ActionType();
                    propertyList.Actions[j].ActionName = newProperties.Actions[j - propertyList.NumberActions].ActionName;
                    propertyList.Actions[j].Script = newProperties.Actions[j - propertyList.NumberActions].Script;
                }
                propertyList.NumberActions = propertyList.NumberActions + newProperties.NumberActions;
                // Add this type name to the TypesIncluded list...
                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + 1;
                if (!propertyList.TypesIncluded) propertyList.TypesIncluded = [];
                propertyList.TypesIncluded[propertyList.NumberTypesIncluded] = typeName;
                // and add the names of the types included by it...
                if (!propertyList.TypesIncluded) propertyList.TypesIncluded = [];
                for (var j = propertyList.NumberTypesIncluded + 1; j <= propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded; j++) {
                    propertyList.TypesIncluded[j] = newProperties.TypesIncluded[j - propertyList.NumberTypesIncluded];
                }
                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded;
            } else if (this.BeginsWith(this._lines[i], "action ")) {
                propertyList.NumberActions = propertyList.NumberActions + 1;
                if (!propertyList.Actions) propertyList.Actions = [];
                propertyList.Actions[propertyList.NumberActions] = this.GetObjectActions(this.GetEverythingAfter(this._lines[i], "action "));
            } else if (this.BeginsWith(this._lines[i], "properties ")) {
                propertyList.Properties = propertyList.Properties + this.GetSimpleParameter(this._lines[i]) + ";";
            } else if (Trim(this._lines[i]) != "") {
                propertyList.Properties = propertyList.Properties + this._lines[i] + ";";
            }
        }
        return propertyList;
    }
    GetRoomId(name: string, ctx: Context): number {
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
    }
    GetTextOrScript(textScript: string): TextAction {
        var result = new TextAction();
        textScript = Trim(textScript);
        if (Left(textScript, 1) == "<") {
            result.Type = TextActionType.Text;
            result.Data = this.GetSimpleParameter(textScript);
        } else {
            result.Type = TextActionType.Script;
            result.Data = textScript;
        }
        return result;
    }
    GetThingNumber(name: string, room: string, type: Thing): number {
        // Returns the number in the Chars() or _objs() array of the specified char/obj
        if (type == Thing.Character) {
            for (var i = 1; i <= this._numberChars; i++) {
                if ((room != "" && LCase(this._chars[i].ObjectName) == LCase(name) && LCase(this._chars[i].ContainerRoom) == LCase(room)) || (room == "" && LCase(this._chars[i].ObjectName) == LCase(name))) {
                    return i;
                }
            }
        } else if (type == Thing.Object) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if ((room != "" && LCase(this._objs[i].ObjectName) == LCase(name) && LCase(this._objs[i].ContainerRoom) == LCase(room)) || (room == "" && LCase(this._objs[i].ObjectName) == LCase(name))) {
                    return i;
                }
            }
        }
        return -1;
    }
    GetThingBlock(name: string, room: string, type: Thing): DefineBlock {
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
        } else if (type == Thing.Object) {
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
    }
    MakeRestoreData(): string {
        var data: string[] = [];
        var objectData: ChangeType[] = [];
        var roomData: ChangeType[] = [];
        var numObjectData: number = 0;
        var numRoomData: number = 0;
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
                if (!objectData) objectData = [];
                objectData[numObjectData] = new ChangeType();
                objectData[numObjectData] = this._gameChangeData.ChangeData[i];
            } else if (this.BeginsWith(this._gameChangeData.ChangeData[i].AppliesTo, "room ")) {
                numRoomData = numRoomData + 1;
                if (!roomData) roomData = [];
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
            var exists: string;
            var visible: string;
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
            } else {
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
        var dataString: string;
        var newFileData: string[] = [];
        dataString = data.join("");
        newFileData.push(Left(dataString, start - 1));
        for (var i = start; i <= Len(dataString); i++) {
            newFileData.push(Chr(255 - Asc(Mid(dataString, i, 1))));
        }
        return newFileData.join("");
    }
    MoveThing(name: string, room: string, type: Thing, ctx: Context): void {
        var oldRoom: string = "";
        var id = this.GetThingNumber(name, "", type);
        if (InStr(room, "[") > 0) {
            var idx = this.GetArrayIndex(room, ctx);
            room = room + Trim(Str(idx.Index));
        }
        if (type == Thing.Character) {
            this._chars[id].ContainerRoom = room;
        } else if (type == Thing.Object) {
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
            this.UpdateInventory(ctx);
        }
    }
    async ConvertParameter(parameter: string, convertChar: string, action: ConvertType, ctx: Context): Promise<string> {
        // Returns a string with functions, string and
        // numeric variables executed or converted as
        // appropriate, read for display/etc.
        var result: string = "";
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
                } else {
                    if (action == ConvertType.Strings) {
                        result = result + this.GetStringContents(varName, ctx);
                    } else if (action == ConvertType.Functions) {
                        varName = this.EvaluateInlineExpressions(varName);
                        result = result + await this.DoFunction(varName, ctx);
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
    async DoFunction(data: string, ctx: Context): Promise<string> {
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
            //Function does not exist; try an internal function.
            intFuncResult = await this.DoInternalFunction(name, parameter, ctx);
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
                    var scp = InStrFrom(pos, parameter, ";");
                    var parameterData = Trim(Mid(parameter, pos, scp - pos));
                    await this.SetStringContents("quest.function.parameter." + Trim(Str(numParameters)), parameterData, ctx);
                    newCtx.NumParameters = numParameters;
                    if (!newCtx.Parameters) newCtx.Parameters = [];
                    newCtx.Parameters[numParameters] = parameterData;
                    pos = scp + 1;
                } while (!(pos >= Len(parameter)));
                await this.SetStringContents("quest.function.numparameters", Trim(Str(numParameters)), ctx);
            } else {
                await this.SetStringContents("quest.function.numparameters", "0", ctx);
                newCtx.NumParameters = 0;
            }
            var result: string = "";
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                await this.ExecuteScript(this._lines[i], newCtx);
                result = newCtx.FunctionReturnValue;
            }
            return result;
        }
    }
    async DoInternalFunction(name: string, parameter: string, ctx: Context): Promise<string> {
        var parameters: string[];
        var untrimmedParameters: string[];
        var objId: number = 0;
        var numParameters = 0;
        var pos = 1;
        if (parameter != "") {
            parameter = parameter + ";";
            do {
                numParameters = numParameters + 1;
                var scp = InStrFrom(pos, parameter, ";");
                if (!parameters) parameters = [];
                if (!untrimmedParameters) untrimmedParameters = [];
                untrimmedParameters[numParameters] = Mid(parameter, pos, scp - pos);
                parameters[numParameters] = Trim(untrimmedParameters[numParameters]);
                pos = scp + 1;
            } while (!(pos >= Len(parameter)));
            // Remove final ";"
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
            objId = await this.GetObjectId(parameters[1], ctx);
            if (objId == -1) {
                this.LogASLError("Object '" + parameters[1] + "' does not exist", LogType.WarningError);
                return "!";
            } else {
                return this._objs[await this.GetObjectId(parameters[1], ctx)].ObjectAlias;
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
            // Deprecated
            return await this.FindStatement(this.DefineBlockParam("room", parameters[1]), parameters[2]);
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
            return Str(Math.random() * (parseFloat(parameters[2]) - parseFloat(parameters[1]) + 1) + parseFloat(parameters[1]));
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
                    this.LogASLError("Invalid function call in '$instr(" + parameters[1] + "; " + parameters[2] + "; " + parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                } else {
                    return Trim(Str(InStrFrom(parseInt(parameters[1]), parameters[2], param3)));
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
            for (var i = 1; i <= this._numberTimers; i++) {
                if (LCase(this._timers[i].TimerName) == LCase(parameters[1])) {
                    if (this._timers[i].TimerActive) {
                        return "1";
                    } else {
                        return "0";
                    }
                }
            }
            this.LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        } else if (name == "timerinterval") {
            for (var i = 1; i <= this._numberTimers; i++) {
                if (LCase(this._timers[i].TimerName) == LCase(parameters[1])) {
                    return Str(this._timers[i].TimerInterval);
                }
            }
            this.LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
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
            this.LogASLError("No such variable '" + parameters[1] + "'", LogType.WarningError);
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
                this.LogASLError("No such object '" + parameters[1] + "'", LogType.WarningError);
                return "!";
            } else {
                return this.GetObjectProperty(parameters[2], objId);
            }
        } else if (name == "getobjectname") {
            if (numParameters == 3) {
                objId = await this.Disambiguate(parameters[1], parameters[2] + ";" + parameters[3], ctx);
            } else if (numParameters == 2) {
                objId = await this.Disambiguate(parameters[1], parameters[2], ctx);
            } else {
                objId = await this.Disambiguate(parameters[1], this._currentRoom + ";inventory", ctx);
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
                return this._objs[e.GetObjId()].ObjectName;
            }
        }
        return "__NOTDEFINED";
    }
    async ExecFor(line: string, ctx: Context): Promise<void> {
        // See if this is a "for each" loop:
        if (this.BeginsWith(line, "each ")) {
            await this.ExecForEach(this.GetEverythingAfter(line, "each "), ctx);
            return;
        }
        // Executes a for loop, of form:
        //   for <variable; startvalue; endvalue> script
        var endValue: number = 0;
        var stepValue: number = 0;
        var forData = await this.GetParameter(line, ctx);
        // Extract individual components:
        var scp1 = InStr(forData, ";");
        var scp2 = InStrFrom(scp1 + 1, forData, ";");
        var scp3 = InStrFrom(scp2 + 1, forData, ";");
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
            await this.SetNumericVariableContents(counterVariable, i, ctx);
            await this.ExecuteScript(loopScript, ctx);
            i = this.GetNumericContents(counterVariable, ctx);
        }
    }
    async ExecSetVar(varInfo: string, ctx: Context): Promise<void> {
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
                } else {
                    varCont = "0";
                    this.LogASLError("Error setting numeric variable <" + varInfo + "> : " + expResult.Message, LogType.WarningError);
                }
            } else {
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
                            } else {
                                this.LogASLError("Division by zero - The result of this operation has been set to zero.", LogType.WarningError);
                                varCont = "0";
                            }
                            break;
                    }
                }
            }
            await this.SetNumericVariableContents(idx.Name, Val(varCont), ctx, idx.Index);
        }
        catch (e) {
            this.LogASLError("Error setting variable '" + idx.Name + "' to '" + varCont + "'", LogType.WarningError);
        }
    }
    ExecuteIfGot(item: string): boolean {
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
    }
    ExecuteIfHas(condition: string): boolean {
        var checkValue: number = 0;
        var colNum: number = 0;
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
        var expectNumerics: boolean = false;
        var expResult: ExpressionResult;
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
        } else {
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
    }
    GetNumericContents(name: string, ctx: Context, noError: boolean = false): number {
        var numNumber: number = 0;
        var arrayIndex: number = 0;
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
                arrayIndex = this.GetNumericContents(arrayIndexData, ctx);
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
    }
    async PlayerErrorMessage(e: PlayerError, ctx: Context): Promise<void> {
        await this.Print(await this.GetErrorMessage(e, ctx), ctx);
    }
    async PlayerErrorMessage_ExtendInfo(e: PlayerError, ctx: Context, extraInfo: string): Promise<void> {
        var message = await this.GetErrorMessage(e, ctx);
        if (extraInfo != "") {
            if (Right(message, 1) == ".") {
                message = Left(message, Len(message) - 1);
            }
            message = message + " - " + extraInfo + ".";
        }
        await this.Print(message, ctx);
    }
    async GetErrorMessage(e: PlayerError, ctx: Context): Promise<string> {
        return await this.ConvertParameter(
            await this.ConvertParameter(
                await this.ConvertParameter(this._playerErrorMessageString[e], "%", ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx), "#", ConvertType.Strings, ctx);
    }
    PlayMedia(filename: string, sync: boolean = false, looped: boolean = false): void {
        if (filename.length == 0) {
            this._player.StopSound();
            //TODO: Handle sync parameter
        } else {
            if (looped && sync) {
                sync = false;
            }
            // Can't loop and sync at the same time, that would just hang!
            this._player.PlaySound(filename, sync, looped);
        }
    }
    PlayWav(parameter: string): void {
        var params: string[] = parameter.split(";").map(function (p) {
            return p.trim();
        });
        var filename = params[0];
        var looped = (params.indexOf("loop") != -1);
        var sync = (params.indexOf("sync") != -1);
        if (filename.length > 0 && InStr(filename, ".") == 0) {
            filename = filename + ".wav";
        }
        this.PlayMedia(filename, sync, looped);
    }
    async RestoreGameData(fileData: string): Promise<void> {
        var appliesTo: string;
        var data: string = "";
        var objId: number = 0;
        var timerNum: number = 0;
        var varUbound: number = 0;
        var found: boolean = false;
        var numStoredData: number = 0;
        var storedData: ChangeType[] = [];
        var decryptedFile: string[] = [];
        // Decrypt file
        for (var i = 1; i <= Len(fileData); i++) {
            decryptedFile.push(Chr(255 - Asc(Mid(fileData, i, 1))));
        }
        this._fileData = decryptedFile.join("");
        this._currentRoom = this.GetNextChunk();
        // OBJECTS
        var numData = parseInt(this.GetNextChunk());
        var createdObjects: string[] = [];
        for (var i = 1; i <= numData; i++) {
            appliesTo = this.GetNextChunk();
            data = this.GetNextChunk();
            // As of Quest 4.0, properties and actions are put into StoredData while we load the file,
            // and then processed later. This is so any created rooms pick up their properties - otherwise
            // we try to set them before they've been created.
            if (this.BeginsWith(data, "properties ") || this.BeginsWith(data, "action ")) {
                numStoredData = numStoredData + 1;
                if (!storedData) storedData = [];
                storedData[numStoredData] = new ChangeType();
                storedData[numStoredData].AppliesTo = appliesTo;
                storedData[numStoredData].Change = data;
            } else if (this.BeginsWith(data, "create ")) {
                var createData: string = appliesTo + ";" + this.GetEverythingAfter(data, "create ");
                // workaround bug where duplicate "create" entries appear in the restore data
                if (createdObjects.indexOf(createData) == -1) {
                    await this.ExecuteCreate("object <" + createData + ">", this._nullContext);
                    createdObjects.push(createData);
                }
            } else {
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
                await this.ExecuteCreate(data, this._nullContext);
            } else if (data == "create") {
                await this.ExecuteCreate("room <" + appliesTo + ">", this._nullContext);
            } else if (this.BeginsWith(data, "destroy exit ")) {
                await this.DestroyExit(appliesTo + "; " + this.GetEverythingAfter(data, "destroy exit "), this._nullContext);
            }
        }
        // Now go through and apply object properties and actions
        for (var i = 1; i <= numStoredData; i++) {
            var d = storedData[i];
            if (this.BeginsWith(d.Change, "properties ")) {
                this.AddToObjectProperties(this.GetEverythingAfter(d.Change, "properties "), this.GetObjectIdNoAlias(d.AppliesTo), this._nullContext);
            } else if (this.BeginsWith(d.Change, "action ")) {
                this.AddToObjectActions(this.GetEverythingAfter(d.Change, "action "), this.GetObjectIdNoAlias(d.AppliesTo));
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
                var thisChar: string = this.GetFileDataChars(1);
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
                await this.SetStringContents(appliesTo, data, this._nullContext);
            } else {
                for (var j = 0; j <= varUbound; j++) {
                    data = this.GetNextChunk();
                    await this.SetStringContents(appliesTo, data, this._nullContext, j);
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
                await this.SetNumericVariableContents(appliesTo, Val(data), this._nullContext);
            } else {
                for (var j = 0; j <= varUbound; j++) {
                    data = this.GetNextChunk();
                    await this.SetNumericVariableContents(appliesTo, Val(data), this._nullContext, j);
                }
            }
        }
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
        if (name == "") {
            name = this._defaultFontName;
        }
        this._player.SetFont(name);
    }
    async SetNumericVariableContents(name: string, content: number, ctx: Context, arrayIndex: number = 0): Promise<void> {
        var numNumber: number = 0;
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
            if (!this._numericVariable) this._numericVariable = [];
            this._numericVariable[numNumber] = new VariableType();
            this._numericVariable[numNumber].VariableUBound = arrayIndex;
        }
        if (arrayIndex > this._numericVariable[numNumber].VariableUBound) {
            if (!this._numericVariable[numNumber].VariableContents) this._numericVariable[numNumber].VariableContents = [];
            this._numericVariable[numNumber].VariableUBound = arrayIndex;
        }
        // Now, set the contents
        this._numericVariable[numNumber].VariableName = name;
        if (!this._numericVariable[numNumber].VariableContents) this._numericVariable[numNumber].VariableContents = [];
        this._numericVariable[numNumber].VariableContents[arrayIndex] = (content).toString();
        if (this._numericVariable[numNumber].OnChangeScript != "" && !this._gameIsRestoring) {
            var script = this._numericVariable[numNumber].OnChangeScript;
            await this.ExecuteScript(script, ctx);
        }
        if (this._numericVariable[numNumber].DisplayString != "") {
            await this.UpdateStatusVars(ctx);
        }
    }
    async SetOpenClose(name: string, open: boolean, ctx: Context): Promise<void> {
        var cmd: string;
        if (open) {
            cmd = "open";
        } else {
            cmd = "close";
        }
        var id = this.GetObjectIdNoAlias(name);
        if (id == 0) {
            this.LogASLError("Invalid object name specified in '" + cmd + " <" + name + ">", LogType.WarningError);
            return;
        }
        await this.DoOpenClose(id, open, false, ctx);
    }
    SetTimerState(name: string, state: boolean): void {
        for (var i = 1; i <= this._numberTimers; i++) {
            if (LCase(name) == LCase(this._timers[i].TimerName)) {
                this._timers[i].TimerActive = state;
                this._timers[i].BypassThisTurn = true;
                // don't trigger timer during the turn it was first enabled
                return null;
            }
        }
        this.LogASLError("No such timer '" + name + "'", LogType.WarningError);
    }
    async SetUnknownVariableType(variableData: string, ctx: Context): Promise<SetResult> {
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
                await this.ExecSetString(variableData, ctx);
                return SetResult.Found;
            }
        }
        for (var i = 1; i <= this._numberNumericVariables; i++) {
            if (LCase(this._numericVariable[i].VariableName) == LCase(name)) {
                await this.ExecSetVar(variableData, ctx);
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
    }
    async SetUpChoiceForm(blockName: string, ctx: Context): Promise<string> {
        // Returns script to execute from choice block
        var block = this.DefineBlockParam("selection", blockName);
        var prompt = this.FindStatement(block, "info");
        var menuOptions: StringDictionary = {};
        var menuScript: StringDictionary = {};
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "choice ")) {
                menuOptions[i.toString()] = await this.GetParameter(this._lines[i], ctx);
                menuScript[i.toString()] = Trim(Right(this._lines[i], Len(this._lines[i]) - InStr(this._lines[i], ">")));
            }
        }
        await this.Print("- |i" + prompt + "|xi", ctx);
        var mnu: MenuData = new MenuData(prompt, menuOptions, false);
        var choice: string = await this.ShowMenu(mnu);
        await this.Print("- " + menuOptions[choice] + "|n", ctx);
        return menuScript[choice];
    }
    SetUpDefaultFonts(): void {
        // Sets up default fonts
        var gameblock = this.GetDefineBlock("game");
        this._defaultFontName = "Arial";
        this._defaultFontSize = 9;
        for (var i = gameblock.StartLine + 1; i <= gameblock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "default fontname ")) {
                var name = this.GetSimpleParameter(this._lines[i]);
                if (name != "") {
                    this._defaultFontName = name;
                }
            } else if (this.BeginsWith(this._lines[i], "default fontsize ")) {
                var size = parseInt(this.GetSimpleParameter(this._lines[i]));
                if (size != 0) {
                    this._defaultFontSize = size;
                }
            }
        }
    }
    SetUpDisplayVariables() {
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "define variable ")) {
                var variable = new VariableType();
                variable.VariableContents = [];
                variable.VariableName = this.GetSimpleParameter(this._lines[i]);
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
                    } else if (this.BeginsWith(this._lines[i], "onchange ")) {
                        variable.OnChangeScript = this.GetEverythingAfter(this._lines[i], "onchange ");
                    } else if (this.BeginsWith(this._lines[i], "display ")) {
                        var displayString = this.GetEverythingAfter(this._lines[i], "display ");
                        if (this.BeginsWith(displayString, "nozero ")) {
                            variable.NoZeroDisplay = true;
                        }
                        variable.DisplayString = this.GetSimpleParameter(this._lines[i]);
                    } else if (this.BeginsWith(this._lines[i], "value ")) {
                        variable.VariableContents[0] = this.GetSimpleParameter(this._lines[i]);
                    }
                } while (!(Trim(this._lines[i]) == "end define"));
                if (type == "string") {
                    // Create string variable
                    this._numberStringVariables = this._numberStringVariables + 1;
                    var id = this._numberStringVariables;
                    if (!this._stringVariable) this._stringVariable = [];
                    this._stringVariable[id] = variable;
                    this._numDisplayStrings = this._numDisplayStrings + 1;
                } else if (type == "numeric") {
                    if (variable.VariableContents[0] == "") {
                        variable.VariableContents[0] = (0).toString();
                    }
                    this._numberNumericVariables = this._numberNumericVariables + 1;
                    var iNumNumber = this._numberNumericVariables;
                    if (!this._numericVariable) this._numericVariable = [];
                    this._numericVariable[iNumNumber] = variable;
                    this._numDisplayNumerics = this._numDisplayNumerics + 1;
                }
            }
        }
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
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (nestBlock == 0) {
                if (this.BeginsWith(this._lines[i], "define ")) {
                    nestBlock = nestBlock + 1;
                } else if (this.BeginsWith(this._lines[i], "properties ")) {
                    this.AddToObjectProperties(this.GetSimpleParameter(this._lines[i]), this._numberObjs, this._nullContext);
                } else if (this.BeginsWith(this._lines[i], "type ")) {
                    o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                    if (!o.TypesIncluded) o.TypesIncluded = [];
                    o.TypesIncluded[o.NumberTypesIncluded] = this.GetSimpleParameter(this._lines[i]);
                    var propertyData = this.GetPropertiesInType(this.GetSimpleParameter(this._lines[i]));
                    this.AddToObjectProperties(propertyData.Properties, this._numberObjs, this._nullContext);
                    for (var k = 1; k <= propertyData.NumberActions; k++) {
                        this.AddObjectAction(this._numberObjs, propertyData.Actions[k].ActionName, propertyData.Actions[k].Script);
                    }
                } else if (this.BeginsWith(this._lines[i], "action ")) {
                    this.AddToObjectActions(this.GetEverythingAfter(this._lines[i], "action "), this._numberObjs);
                }
            } else {
                if (Trim(this._lines[i]) == "end define") {
                    nestBlock = nestBlock - 1;
                }
            }
        }
    }
    SetUpOptions(): void {
        var opt: string;
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
    }
    SetUpRoomData(): void {
        var defaultProperties: PropertiesActions = new PropertiesActions();
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
                if (!this._rooms) this._rooms = [];
                this._rooms[this._numberRooms] = new RoomType();
                this._numberObjs = this._numberObjs + 1;
                if (!this._objs) this._objs = [];
                this._objs[this._numberObjs] = new ObjectType();
                var r = this._rooms[this._numberRooms];
                r.RoomName = this.GetSimpleParameter(this._lines[this._defineBlocks[i].StartLine]);
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
                            } else if (Trim(this._lines[j]) == "end define") {
                                nestedBlock = nestedBlock - 1;
                            }
                        } while (!(nestedBlock == 0));
                    }
                    if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "alias ")) {
                        r.RoomAlias = this.GetSimpleParameter(this._lines[j]);
                        this._objs[this._numberObjs].ObjectAlias = r.RoomAlias;
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("alias=" + r.RoomAlias, this._numberObjs, this._nullContext);
                        }
                    } else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "description ")) {
                        r.Description = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "description "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Description.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "description", r.Description.Data);
                            } else {
                                this.AddToObjectProperties("description=" + r.Description.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "out ")) {
                        r.Out.Text = this.GetSimpleParameter(this._lines[j]);
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
                            if (r.East.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "east", r.East.Data);
                            } else {
                                this.AddToObjectProperties("east=" + r.East.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "west ")) {
                        r.West = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "west "));
                        if (this._gameAslVersion >= 350) {
                            if (r.West.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "west", r.West.Data);
                            } else {
                                this.AddToObjectProperties("west=" + r.West.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "north ")) {
                        r.North = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "north "));
                        if (this._gameAslVersion >= 350) {
                            if (r.North.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "north", r.North.Data);
                            } else {
                                this.AddToObjectProperties("north=" + r.North.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "south ")) {
                        r.South = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "south "));
                        if (this._gameAslVersion >= 350) {
                            if (r.South.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "south", r.South.Data);
                            } else {
                                this.AddToObjectProperties("south=" + r.South.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "northeast ")) {
                        r.NorthEast = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "northeast "));
                        if (this._gameAslVersion >= 350) {
                            if (r.NorthEast.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "northeast", r.NorthEast.Data);
                            } else {
                                this.AddToObjectProperties("northeast=" + r.NorthEast.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "northwest ")) {
                        r.NorthWest = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "northwest "));
                        if (this._gameAslVersion >= 350) {
                            if (r.NorthWest.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "northwest", r.NorthWest.Data);
                            } else {
                                this.AddToObjectProperties("northwest=" + r.NorthWest.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "southeast ")) {
                        r.SouthEast = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "southeast "));
                        if (this._gameAslVersion >= 350) {
                            if (r.SouthEast.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "southeast", r.SouthEast.Data);
                            } else {
                                this.AddToObjectProperties("southeast=" + r.SouthEast.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "southwest ")) {
                        r.SouthWest = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "southwest "));
                        if (this._gameAslVersion >= 350) {
                            if (r.SouthWest.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "southwest", r.SouthWest.Data);
                            } else {
                                this.AddToObjectProperties("southwest=" + r.SouthWest.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "up ")) {
                        r.Up = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "up "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Up.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "up", r.Up.Data);
                            } else {
                                this.AddToObjectProperties("up=" + r.Up.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this.BeginsWith(this._lines[j], "down ")) {
                        r.Down = this.GetTextOrScript(this.GetEverythingAfter(this._lines[j], "down "));
                        if (this._gameAslVersion >= 350) {
                            if (r.Down.Type == TextActionType.Script) {
                                this.AddObjectAction(this._numberObjs, "down", r.Down.Data);
                            } else {
                                this.AddToObjectProperties("down=" + r.Down.Data, this._numberObjs, this._nullContext);
                            }
                        }
                    } else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "indescription ")) {
                        r.InDescription = this.GetSimpleParameter(this._lines[j]);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("indescription=" + r.InDescription, this._numberObjs, this._nullContext);
                        }
                    } else if (this._gameAslVersion >= 280 && this.BeginsWith(this._lines[j], "look ")) {
                        r.Look = this.GetSimpleParameter(this._lines[j]);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("look=" + r.Look, this._numberObjs, this._nullContext);
                        }
                    } else if (this.BeginsWith(this._lines[j], "prefix ")) {
                        r.Prefix = this.GetSimpleParameter(this._lines[j]);
                        if (this._gameAslVersion >= 350) {
                            this.AddToObjectProperties("prefix=" + r.Prefix, this._numberObjs, this._nullContext);
                        }
                    } else if (this.BeginsWith(this._lines[j], "script ")) {
                        r.Script = this.GetEverythingAfter(this._lines[j], "script ");
                        this.AddObjectAction(this._numberObjs, "script", r.Script);
                    } else if (this.BeginsWith(this._lines[j], "command ")) {
                        r.NumberCommands = r.NumberCommands + 1;
                        if (!r.Commands) r.Commands = [];
                        r.Commands[r.NumberCommands] = new UserDefinedCommandType();
                        r.Commands[r.NumberCommands].CommandText = this.GetSimpleParameter(this._lines[j]);
                        r.Commands[r.NumberCommands].CommandScript = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                    } else if (this.BeginsWith(this._lines[j], "place ")) {
                        r.NumberPlaces = r.NumberPlaces + 1;
                        if (!r.Places) r.Places = [];
                        r.Places[r.NumberPlaces] = new PlaceType();
                        var placeData = this.GetSimpleParameter(this._lines[j]);
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
                        if (!r.Use) r.Use = [];
                        r.Use[r.NumberUse] = new ScriptText();
                        r.Use[r.NumberUse].Text = this.GetSimpleParameter(this._lines[j]);
                        r.Use[r.NumberUse].Script = Trim(Mid(this._lines[j], InStr(this._lines[j], ">") + 1));
                    } else if (this.BeginsWith(this._lines[j], "properties ")) {
                        this.AddToObjectProperties(this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                    } else if (this.BeginsWith(this._lines[j], "type ")) {
                        this._objs[this._numberObjs].NumberTypesIncluded = this._objs[this._numberObjs].NumberTypesIncluded + 1;
                        if (!this._objs[this._numberObjs].TypesIncluded) this._objs[this._numberObjs].TypesIncluded = [];
                        this._objs[this._numberObjs].TypesIncluded[this._objs[this._numberObjs].NumberTypesIncluded] = this.GetSimpleParameter(this._lines[j]);
                        var propertyData = this.GetPropertiesInType(this.GetSimpleParameter(this._lines[j]));
                        this.AddToObjectProperties(propertyData.Properties, this._numberObjs, this._nullContext);
                        for (var k = 1; k <= propertyData.NumberActions; k++) {
                            this.AddObjectAction(this._numberObjs, propertyData.Actions[k].ActionName, propertyData.Actions[k].Script);
                        }
                    } else if (this.BeginsWith(this._lines[j], "action ")) {
                        this.AddToObjectActions(this.GetEverythingAfter(this._lines[j], "action "), this._numberObjs);
                    } else if (this.BeginsWith(this._lines[j], "beforeturn ")) {
                        r.BeforeTurnScript = r.BeforeTurnScript + this.GetEverythingAfter(this._lines[j], "beforeturn ") + "\n";
                    } else if (this.BeginsWith(this._lines[j], "afterturn ")) {
                        r.AfterTurnScript = r.AfterTurnScript + this.GetEverythingAfter(this._lines[j], "afterturn ") + "\n";
                    }
                }
            }
        }
    }
    SetUpSynonyms(): void {
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
                    } else {
                        this._numberSynonyms = this._numberSynonyms + 1;
                        if (!this._synonyms) this._synonyms = [];
                        this._synonyms[this._numberSynonyms] = new SynonymType();
                        this._synonyms[this._numberSynonyms].OriginalWord = thisWord;
                        this._synonyms[this._numberSynonyms].ConvertTo = convertWord;
                    }
                    pos = endOfWord + 1;
                } while (!(pos >= Len(originalWordsList)));
            }
        }
    }
    SetUpTimers(): void {
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define timer ")) {
                this._numberTimers = this._numberTimers + 1;
                if (!this._timers) this._timers = [];
                this._timers[this._numberTimers] = new TimerType();
                this._timers[this._numberTimers].TimerName = this.GetSimpleParameter(this._lines[this._defineBlocks[i].StartLine]);
                this._timers[this._numberTimers].TimerActive = false;
                for (var j = this._defineBlocks[i].StartLine + 1; j <= this._defineBlocks[i].EndLine - 1; j++) {
                    if (this.BeginsWith(this._lines[j], "interval ")) {
                        this._timers[this._numberTimers].TimerInterval = parseInt(this.GetSimpleParameter(this._lines[j]));
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
    }
    SetUpTurnScript(): void {
        var block = this.GetDefineBlock("game");
        this._beforeTurnScript = "";
        this._afterTurnScript = "";
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "beforeturn ")) {
                this._beforeTurnScript = this._beforeTurnScript + this.GetEverythingAfter(Trim(this._lines[i]), "beforeturn ") + "\n";
            } else if (this.BeginsWith(this._lines[i], "afterturn ")) {
                this._afterTurnScript = this._afterTurnScript + this.GetEverythingAfter(Trim(this._lines[i]), "afterturn ") + "\n";
            }
        }
    }
    SetUpUserDefinedPlayerErrors(): void {
        // goes through "define game" block and sets stored error
        // messages accordingly
        var block = this.GetDefineBlock("game");
        var examineIsCustomised = false;
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "error ")) {
                var errorInfo = this.GetSimpleParameter(this._lines[i]);
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
                        } else {
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
    }
    SetVisibility(thing: string, type: Thing, visible: boolean, ctx: Context): void {
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
                this.LogASLError("Not found object '" + thing + "'", LogType.WarningError);
            }
        } else {
            // split ThingString into character name and room
            // (thingstring of form name@room)
            var atPos = InStr(thing, "@");
            var room: string;
            var name: string;
            // If no room specified, current room presumed
            if (atPos == 0) {
                room = this._currentRoom;
                name = thing;
            } else {
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
            } else if (type == Thing.Object) {
                for (var i = 1; i <= this._numberObjs; i++) {
                    if (LCase(this._objs[i].ContainerRoom) == LCase(room) && LCase(this._objs[i].ObjectName) == LCase(name)) {
                        this._objs[i].Visible = visible;
                        break;
                    }
                }
            }
        }
        this.UpdateObjectList(ctx);
    }
    ShowPictureInText(filename: string): void {
        if (!this._useStaticFrameForPictures) {
            this._player.ShowPicture(filename);
        } else {
            // Workaround for a particular game which expects pictures to be in a popup window -
            // use the static picture frame feature so that image is not cleared
            this._player.SetPanelContents("<img src=\"" + this._player.GetURL(filename) + "\" onload=\"setPanelHeight()\"/>");
        }
    }
    async ShowRoomInfoV2(room: string): Promise<void> {
        // ShowRoomInfo for Quest 2.x games
        var roomDisplayText: string = "";
        var descTagExist: boolean = false;
        var gameBlock: DefineBlock;
        var charsViewable: string;
        var charsFound: number = 0;
        var prefixAliasNoFormat: string;
        var prefix: string;
        var prefixAlias: string;
        var inDesc: string;
        var aliasName: string = "";
        var charList: string;
        var foundLastComma: number = 0;
        var cp: number = 0;
        var ncp: number = 0;
        var noFormatObjsViewable: string;
        var objsViewable: string = "";
        var objsFound: number = 0;
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
        var lastComma: number = 0;
        var oldLastComma: number = 0;
        var defineBlock: number = 0;
        var lookString: string = "";
        gameBlock = this.GetDefineBlock("game");
        this._currentRoom = room;
        //find the room
        var roomBlock: DefineBlock;
        roomBlock = this.DefineBlockParam("room", room);
        var finishedFindingCommas: boolean = false;
        charsViewable = "";
        charsFound = 0;
        //see if room has an alias
        for (var i = roomBlock.StartLine + 1; i <= roomBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "alias")) {
                aliasName = await this.GetParameter(this._lines[i], this._nullContext);
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
                inDesc = Trim(await this.GetParameter(this._lines[i], this._nullContext));
                i = roomBlock.EndLine;
            }
        }
        if (inDesc != "unfound") {
            // Print player's location according to indescription:
            if (Right(inDesc, 1) == ":") {
                // if line ends with a colon, add place name:
                roomDisplayText = roomDisplayText + Left(inDesc, Len(inDesc) - 1) + " " + prefixAlias + ".\n";
            } else {
                // otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText + inDesc + "\n";
            }
        } else {
            // if no indescription line, print the default.
            roomDisplayText = roomDisplayText + "You are in " + prefixAlias + ".\n";
        }
        this._player.LocationUpdated(prefixAliasNoFormat);
        await this.SetStringContents("quest.formatroom", prefixAliasNoFormat, this._nullContext);
        //FIND CHARACTERS ===
        for (var i = 1; i <= this._numberChars; i++) {
            if (this._chars[i].ContainerRoom == room && this._chars[i].Exists && this._chars[i].Visible) {
                charsViewable = charsViewable + this._chars[i].Prefix + "|b" + this._chars[i].ObjectName + "|xb" + this._chars[i].Suffix + ", ";
                charsFound = charsFound + 1;
            }
        }
        if (charsFound == 0) {
            charsViewable = "There is nobody here.";
            await this.SetStringContents("quest.characters", "", this._nullContext);
        } else {
            //chop off final comma and add full stop (.)
            charList = Left(charsViewable, Len(charsViewable) - 2);
            await this.SetStringContents("quest.characters", charList, this._nullContext);
            //if more than one character, add "and" before
            //last one:
            cp = InStr(charList, ",");
            if (cp != 0) {
                foundLastComma = 0;
                do {
                    ncp = InStrFrom(cp + 1, charList, ",");
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
        var finishedLoop: boolean = false;
        if (objsFound != 0) {
            objListString = Left(objsViewable, Len(objsViewable) - 2);
            noFormatObjListString = Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2);
            cp = InStr(objListString, ",");
            if (cp != 0) {
                do {
                    ncp = InStrFrom(cp + 1, objListString, ",");
                    if (ncp == 0) {
                        finishedLoop = true;
                    } else {
                        cp = ncp;
                    }
                } while (!(finishedLoop));
                objListString = Trim(Left(objListString, cp - 1) + " and " + Trim(Mid(objListString, cp + 1)));
            }
            objsViewable = "There is " + objListString + " here.";
            await this.SetStringContents("quest.objects", Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2), this._nullContext);
            await this.SetStringContents("quest.formatobjects", objListString, this._nullContext);
            roomDisplayText = roomDisplayText + objsViewable + "\n";
        } else {
            await this.SetStringContents("quest.objects", "", this._nullContext);
            await this.SetStringContents("quest.formatobjects", "", this._nullContext);
        }
        //FIND DOORWAYS
        doorways = "";
        nsew = "";
        places = "";
        possDir = "";
        for (var i = roomBlock.StartLine + 1; i <= roomBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "out")) {
                doorways = await this.GetParameter(this._lines[i], this._nullContext);
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
                place = await this.GetParameter(this._lines[i], this._nullContext);
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
        var outside: DefineBlock;
        if (doorways != "") {
            //see if outside has an alias
            outside = this.DefineBlockParam("room", doorways);
            for (var i = outside.StartLine + 1; i <= outside.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "alias")) {
                    aliasOut = await this.GetParameter(this._lines[i], this._nullContext);
                    i = outside.EndLine;
                }
            }
            if (aliasOut == "") {
                aliasOut = doorways;
            }
            roomDisplayText = roomDisplayText + "You can go out to " + aliasOut + ".\n";
            possDir = possDir + "o";
            await this.SetStringContents("quest.doorways.out", aliasOut, this._nullContext);
        } else {
            await this.SetStringContents("quest.doorways.out", "", this._nullContext);
        }
        var finished: boolean = false;
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
                    } else {
                        cp = ncp;
                    }
                } while (!(finished));
                nsew = Trim(Left(nsew, cp - 1)) + " or " + Trim(Mid(nsew, cp + 1));
            }
            roomDisplayText = roomDisplayText + "You can go " + nsew + ".\n";
            await this.SetStringContents("quest.doorways.dirs", nsew, this._nullContext);
        } else {
            await this.SetStringContents("quest.doorways.dirs", "", this._nullContext);
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
            await this.SetStringContents("quest.doorways.places", places, this._nullContext);
        } else {
            await this.SetStringContents("quest.doorways.places", "", this._nullContext);
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
            await this.Print(roomDisplayText, this._nullContext);
        } else {
            //execute description tag:
            //If no script, just print the tag's parameter.
            //Otherwise, execute it as ASL script:
            descLine = this.GetEverythingAfter(Trim(descLine), "description ");
            if (Left(descLine, 1) == "<") {
                await this.Print(await this.GetParameter(descLine, this._nullContext), this._nullContext);
            } else {
                await this.ExecuteScript(descLine, this._nullContext);
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
                lookString = await this.GetParameter(this._lines[i], this._nullContext);
                i = roomBlock.EndLine;
            }
        }
        if (lookString != "") {
            await this.Print(lookString, this._nullContext);
        }
    }
    AddToObjectList(objList: ListData[], exitList: ListData[], name: string, type: Thing): void {
        name = this.CapFirst(name);
        if (type == Thing.Room) {
            objList.push(new ListData(name, this._listVerbs[ListType.ExitsList]));
            exitList.push(new ListData(name, this._listVerbs[ListType.ExitsList]));
        } else {
            objList.push(new ListData(name, this._listVerbs[ListType.ObjectsList]));
        }
    }
    async ExecExec(scriptLine: string, ctx: Context): Promise<void> {
        if (ctx.CancelExec) {
            return;
        }
        var execLine = await this.GetParameter(scriptLine, ctx);
        var newCtx: Context = this.CopyContext(ctx);
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
                await this.ExecCommand(execLine, newCtx, false);
            }
            catch (e) {
                this.LogASLError("Internal error running '" + scriptLine + "'", LogType.WarningError);
                ctx.CancelExec = true;
            }
        } else {
            var scp = InStr(execLine, ";");
            var ex = Trim(Left(execLine, scp - 1));
            var r = Trim(Mid(execLine, scp + 1));
            if (r == "normal") {
                await this.ExecCommand(ex, newCtx, false, false);
            } else {
                this.LogASLError("Unrecognised post-command parameter in " + Trim(scriptLine), LogType.WarningError);
            }
        }
    }
    async ExecSetString(info: string, ctx: Context): Promise<void> {
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
        await this.SetStringContents(idx.Name, value, ctx, idx.Index);
    }
    async ExecUserCommand(cmd: string, ctx: Context, libCommands: boolean = false): Promise<boolean> {
        //Executes a user-defined command. If unavailable, returns
        //false.
        var curCmd: string;
        var commandList: string;
        var script: string = "";
        var commandTag: string;
        var commandLine: string = "";
        var foundCommand = false;
        //First, check for a command in the current room block
        var roomId = this.GetRoomId(this._currentRoom, ctx);
        // RoomID is 0 if we have no rooms in the game. Unlikely, but we get an RTE otherwise.
        if (roomId != 0) {
            var r = this._rooms[roomId];
            for (var i = 1; i <= r.NumberCommands; i++) {
                commandList = r.Commands[i].CommandText;
                var ep: number = 0;
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
                } while (!(ep == 0));
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
                    commandList = await this.GetParameter(this._lines[i], ctx, false);
                    var ep: number = 0;
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
                    } while (!(ep == 0));
                }
            }
        }
        if (foundCommand) {
            if (await this.GetCommandParameters(cmd, commandLine, ctx)) {
                await this.ExecuteScript(script, ctx);
            }
        }
        return foundCommand;
    }
    async ExecuteChoose(section: string, ctx: Context): Promise<void> {
        await this.ExecuteScript(await this.SetUpChoiceForm(section, ctx), ctx);
    }
    async GetCommandParameters(test: string, required: string, ctx: Context): Promise<boolean> {
        //Gets parameters from line. For example, if 'required'
        //is "read #1#" and 'test' is "read sign", #1# returns
        //"sign".
        // Returns FALSE if #@object# form used and object doesn't
        // exist.
        var chunksBegin: number[];
        var chunksEnd: number[];
        var varName: string[];
        var var2Pos: number = 0;
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
            } else {
                var2Pos = InStrFrom(nextVarPos + 1, required, "#");
                currentVariable = Mid(required, nextVarPos + 1, (var2Pos - 1) - nextVarPos);
            }
            var checkChunk = Mid(required, currentReqLinePos, (nextVarPos - 1) - (currentReqLinePos - 1));
            var chunkBegin = InStrFrom(currentTestLinePos, LCase(test), LCase(checkChunk));
            var chunkEnd = chunkBegin + Len(checkChunk);
            numberChunks = numberChunks + 1;
            if (!chunksBegin) chunksBegin = [];
            if (!chunksEnd) chunksEnd = [];
            if (!varName) varName = [];
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
            var arrayIndex: number = 0;
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
                var id = await this.Disambiguate(curChunk, this._currentRoom + ";" + "inventory", ctx);
                if (id == -1) {
                    if (this._gameAslVersion >= 391) {
                        await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    } else {
                        await this.PlayerErrorMessage(PlayerError.BadItem, ctx);
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
                    await this.SetStringContents(varName[i], this._objs[id].ObjectName, ctx, arrayIndex);
                }
            } else {
                await this.SetStringContents(varName[i], curChunk, ctx, arrayIndex);
            }
        }
        return success;
    }
    async GetGender(character: string, capitalise: boolean, ctx: Context): Promise<string> {
        var result: string;
        if (this._gameAslVersion >= 281) {
            result = this._objs[this.GetObjectIdNoAlias(character)].Gender;
        } else {
            var resultLine = this.RetrLine("character", character, "gender", ctx);
            if (resultLine == "<unfound>") {
                result = "it ";
            } else {
                result = await this.GetParameter(resultLine, ctx) + " ";
            }
        }
        if (capitalise) {
            result = UCase(Left(result, 1)) + Right(result, Len(result) - 1);
        }
        return result;
    }
    GetStringContents(name: string, ctx: Context): string {
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
            } else {
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
        var id: number = 0;
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
        } else {
            return this._objs[this.GetObjectIdNoAlias(this._stringVariable[id].VariableContents[arrayIndex])].ObjectAlias;
        }
    }
    IsAvailable(thingName: string, type: Thing, ctx: Context): boolean {
        // Returns availability of object/character
        // split ThingString into character name and room
        // (thingstring of form name@room)
        var room: string;
        var name: string;
        var atPos = InStr(thingName, "@");
        // If no room specified, current room presumed
        if (atPos == 0) {
            room = this._currentRoom;
            name = thingName;
        } else {
            name = Trim(Left(thingName, atPos - 1));
            room = Trim(Right(thingName, Len(thingName) - atPos));
        }
        if (type == Thing.Character) {
            for (var i = 1; i <= this._numberChars; i++) {
                if (LCase(this._chars[i].ContainerRoom) == LCase(room) && LCase(this._chars[i].ObjectName) == LCase(name)) {
                    return this._chars[i].Exists;
                }
            }
        } else if (type == Thing.Object) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (LCase(this._objs[i].ContainerRoom) == LCase(room) && LCase(this._objs[i].ObjectName) == LCase(name)) {
                    return this._objs[i].Exists;
                }
            }
        }
    }
    IsCompatible(test: string, required: string): boolean {
        //Tests to see if 'test' "works" with 'required'.
        //For example, if 'required' = "read #text#", then the
        //tests of "read book" and "read sign" are compatible.
        var var2Pos: number = 0;
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
            } else {
                var2Pos = InStrFrom(nextVarPos + 1, required, "#");
            }
            var checkChunk = Mid(required, currentReqLinePos, (nextVarPos - 1) - (currentReqLinePos - 1));
            if (InStrFrom(currentTestLinePos, test, checkChunk) != 0) {
                currentTestLinePos = InStrFrom(currentTestLinePos, test, checkChunk) + Len(checkChunk);
            } else {
                return false;
            }
            //Skip to end of variable
            currentReqLinePos = var2Pos + 1;
        } while (!(finished));
        return true;
    }
    async OpenGame(filename: string, onSuccess: Callback, onFailure: Callback): Promise<void> {
        var cdatb: boolean = false;
        var result: boolean = false;
        var visible: boolean = false;
        var room: string;
        var fileData: string = "";
        var savedQsgVersion: string;
        var data: string = "";
        var name: string;
        var scp: number = 0;
        var cdat: number = 0;
        var scp2: number = 0;
        var scp3: number = 0;
        var lines: string[] = null;
        this._gameLoadMethod = "loaded";
        var prevQsgVersion = false;
        var fileData = this._data;
        // Check version
        savedQsgVersion = Left(fileData, 10);
        if (this.BeginsWith(savedQsgVersion, "QUEST200.1")) {
            prevQsgVersion = true;
        } else if (!this.BeginsWith(savedQsgVersion, "QUEST300")) {
            onFailure();
            return;
        }
        if (prevQsgVersion) {
            lines = fileData.split("\n");
            this._gameFileName = lines[1];
        } else {
            this.InitFileData(fileData);
            this.GetNextChunk();
            // TODO: For desktop version, game file to load is stored in next chunk
            this.GetNextChunk();   //this._gameFileName = this.GetNextChunk();
            this._gameFileName = filename;
        }
        // TODO
        //if (this._data == null && !System.IO.File.Exists(this._gameFileName)) {
        //    this._gameFileName = this._player.GetNewGameFile(this._gameFileName, "*.asl;*.cas;*.zip");
        //    if (this._gameFileName == "") {
        //        return false;
        //    }
        //}
        var self = this;
        await this.InitialiseGame(this._gameFileName, true, async function () : Promise<void> {
            if (!prevQsgVersion) {
                // Open Quest 3.0 saved game file
                self._gameLoading = true;
                await self.RestoreGameData(fileData);
                self._gameLoading = false;
            } else {
                // Open Quest 2.x saved game file
                self._currentRoom = lines[3];
                // Start at line 5 as line 4 is always "!c"
                var lineNumber: number = 5;
                do {
                    data = lines[lineNumber];
                    lineNumber += 1;
                    if (data != "!i") {
                        scp = InStr(data, ";");
                        name = Trim(Left(data, scp - 1));
                        cdat = parseInt(Right(data, Len(data) - scp));
                        for (var i = 1; i <= self._numCollectables; i++) {
                            if (self._collectables[i].Name == name) {
                                self._collectables[i].Value = cdat;
                                i = self._numCollectables;
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
                        cdatb = self.IsYes(Right(data, Len(data) - scp));
                        for (var i = 1; i <= self._numberItems; i++) {
                            if (self._items[i].Name == name) {
                                self._items[i].Got = cdatb;
                                i = self._numberItems;
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
                        cdatb = self.IsYes(Mid(data, scp + 1, (scp2 - scp) - 1));
                        visible = self.IsYes(Mid(data, scp2 + 1, (scp3 - scp2) - 1));
                        room = Trim(Mid(data, scp3 + 1));
                        for (var i = 1; i <= self._numberObjs; i++) {
                            if (self._objs[i].ObjectName == name && !self._objs[i].Loaded) {
                                self._objs[i].Exists = cdatb;
                                self._objs[i].Visible = visible;
                                self._objs[i].ContainerRoom = room;
                                self._objs[i].Loaded = true;
                                i = self._numberObjs;
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
                        cdatb = self.IsYes(Mid(data, scp + 1, (scp2 - scp) - 1));
                        visible = self.IsYes(Mid(data, scp2 + 1, (scp3 - scp2) - 1));
                        room = Trim(Mid(data, scp3 + 1));
                        for (var i = 1; i <= self._numberChars; i++) {
                            if (self._chars[i].ObjectName == name) {
                                self._chars[i].Exists = cdatb;
                                self._chars[i].Visible = visible;
                                self._chars[i].ContainerRoom = room;
                                i = self._numberChars;
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
                        await self.SetStringContents(name, data, self._nullContext);
                    }
                } while (!(data == "!n"));
                do {
                    data = lines[lineNumber];
                    lineNumber += 1;
                    if (data != "!e") {
                        scp = InStr(data, ";");
                        name = Trim(Left(data, scp - 1));
                        data = Right(data, Len(data) - scp);
                        await self.SetNumericVariableContents(name, Val(data), self._nullContext);
                    }
                } while (!(data == "!e"));
            }
            onSuccess();
        }, onFailure);
    }
    async SaveGame(html: string, callback: StringCallback): Promise<void> {
        // html parameter is ignored for ASL4 
        var saveData: string;
        if (this._gameAslVersion >= 391) {
            var ctx: Context = new Context();
            await this.ExecuteScript(this._beforeSaveScript, ctx);
        }
        if (this._gameAslVersion >= 280) {
            saveData = this.MakeRestoreData();
        } else {
            saveData = this.MakeRestoreDataV2();
        }
        
        callback(saveData);
    }
    MakeRestoreDataV2(): string {
        var lines: string[] = [];
        var i: number = 0;
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
    }
    SetAvailability(thingString: string, exists: boolean, ctx: Context, type: Thing = Thing.Object): void {
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
                this.LogASLError("Not found object '" + thingString + "'", LogType.WarningError);
            }
        } else {
            // split ThingString into character name and room
            // (thingstring of form name@room)
            var room: string;
            var name: string;
            var atPos = InStr(thingString, "@");
            // If no room specified, currentroom presumed
            if (atPos == 0) {
                room = this._currentRoom;
                name = thingString;
            } else {
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
            } else if (type == Thing.Object) {
                for (var i = 1; i <= this._numberObjs; i++) {
                    if (LCase(this._objs[i].ContainerRoom) == LCase(room) && LCase(this._objs[i].ObjectName) == LCase(name)) {
                        this._objs[i].Exists = exists;
                        break;
                    }
                }
            }
        }
        this.UpdateInventory(ctx);
        this.UpdateObjectList(ctx);
    }
    SetStringContentsSimple(name: string, value: string, ctx: Context, arrayIndex: number = 0): number {
        var id: number = 0;
        var exists = false;
        if (name == "") {
            this.LogASLError("Internal error - tried to set empty string name to '" + value + "'", LogType.WarningError);
            return -1;
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
            return -1;
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
            if (!this._stringVariable) this._stringVariable = [];
            this._stringVariable[id] = new VariableType();
            this._stringVariable[id].VariableUBound = arrayIndex;
        }
        if (arrayIndex > this._stringVariable[id].VariableUBound) {
            if (!this._stringVariable[id].VariableContents) this._stringVariable[id].VariableContents = [];
            this._stringVariable[id].VariableUBound = arrayIndex;
        }
        // Now, set the contents
        this._stringVariable[id].VariableName = name;
        if (!this._stringVariable[id].VariableContents) this._stringVariable[id].VariableContents = [];
        this._stringVariable[id].VariableContents[arrayIndex] = value;
        return id;
    }
    async SetStringContents(name: string, value: string, ctx: Context, arrayIndex: number = 0): Promise<void> {
        var id = this.SetStringContentsSimple(name, value, ctx, arrayIndex);
        if (id == -1) return;
        if (this._stringVariable[id].OnChangeScript != "") {
            var script = this._stringVariable[id].OnChangeScript;
            await this.ExecuteScript(script, ctx);
        }
        if (this._stringVariable[id].DisplayString != "") {
            await this.UpdateStatusVars(ctx);
        }
    }
    SetUpCharObjectInfo(): void {
        var defaultProperties: PropertiesActions = new PropertiesActions();
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
            var restOfLine: string;
            var origContainerRoomName: string;
            var containerRoomName: string;
            if (this.BeginsWith(this._lines[block.StartLine], "define room")) {
                origContainerRoomName = this.GetSimpleParameter(this._lines[block.StartLine]);
            } else {
                origContainerRoomName = "";
            }
            var startLine: number = block.StartLine;
            var endLine: number = block.EndLine;
            if (this.BeginsWith(this._lines[block.StartLine], "define object ")) {
                startLine = startLine - 1;
                endLine = endLine + 1;
            }
            for (var j = startLine + 1; j <= endLine - 1; j++) {
                if (this.BeginsWith(this._lines[j], "define object")) {
                    containerRoomName = origContainerRoomName;
                    this._numberObjs = this._numberObjs + 1;
                    if (!this._objs) this._objs = [];
                    this._objs[this._numberObjs] = new ObjectType();
                    var o = this._objs[this._numberObjs];
                    o.ObjectName = this.GetSimpleParameter(this._lines[j]);
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
                        } else if (this.BeginsWith(this._lines[j], "startin ") && containerRoomName == "__UNKNOWN") {
                            containerRoomName = this.GetSimpleParameter(this._lines[j]);
                        } else if (this.BeginsWith(this._lines[j], "prefix ")) {
                            o.Prefix = this.GetSimpleParameter(this._lines[j]) + " ";
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("prefix=" + o.Prefix, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "suffix ")) {
                            o.Suffix = this.GetSimpleParameter(this._lines[j]);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("suffix=" + o.Suffix, this._numberObjs, this._nullContext);
                            }
                        } else if (Trim(this._lines[j]) == "invisible") {
                            o.Visible = false;
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("invisible", this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "alias ")) {
                            o.ObjectAlias = this.GetSimpleParameter(this._lines[j]);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("alias=" + o.ObjectAlias, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "alt ")) {
                            this.AddToObjectAltNames(this.GetSimpleParameter(this._lines[j]), this._numberObjs);
                        } else if (this.BeginsWith(this._lines[j], "detail ")) {
                            o.Detail = this.GetSimpleParameter(this._lines[j]);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("detail=" + o.Detail, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "gender ")) {
                            o.Gender = this.GetSimpleParameter(this._lines[j]);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("gender=" + o.Gender, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "article ")) {
                            o.Article = this.GetSimpleParameter(this._lines[j]);
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
                            o.DisplayType = this.GetSimpleParameter(this._lines[j]);
                            if (this._gameAslVersion >= 311) {
                                this.AddToObjectProperties("displaytype=" + o.DisplayType, this._numberObjs, this._nullContext);
                            }
                        } else if (this.BeginsWith(this._lines[j], "look ")) {
                            if (this._gameAslVersion >= 311) {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "look ");
                                if (Left(restOfLine, 1) == "<") {
                                    this.AddToObjectProperties("look=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                                } else {
                                    this.AddObjectAction(this._numberObjs, "look", restOfLine);
                                }
                            }
                        } else if (this.BeginsWith(this._lines[j], "examine ")) {
                            if (this._gameAslVersion >= 311) {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "examine ");
                                if (Left(restOfLine, 1) == "<") {
                                    this.AddToObjectProperties("examine=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                                } else {
                                    this.AddObjectAction(this._numberObjs, "examine", restOfLine);
                                }
                            }
                        } else if (this._gameAslVersion >= 311 && this.BeginsWith(this._lines[j], "speak ")) {
                            restOfLine = this.GetEverythingAfter(this._lines[j], "speak ");
                            if (Left(restOfLine, 1) == "<") {
                                this.AddToObjectProperties("speak=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                            } else {
                                this.AddObjectAction(this._numberObjs, "speak", restOfLine);
                            }
                        } else if (this.BeginsWith(this._lines[j], "properties ")) {
                            this.AddToObjectProperties(this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "type ")) {
                            o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                            if (!o.TypesIncluded) o.TypesIncluded = [];
                            o.TypesIncluded[o.NumberTypesIncluded] = this.GetSimpleParameter(this._lines[j]);
                            var PropertyData = this.GetPropertiesInType(this.GetSimpleParameter(this._lines[j]));
                            this.AddToObjectProperties(PropertyData.Properties, this._numberObjs, this._nullContext);
                            for (var k = 1; k <= PropertyData.NumberActions; k++) {
                                this.AddObjectAction(this._numberObjs, PropertyData.Actions[k].ActionName, PropertyData.Actions[k].Script);
                            }
                            if (!o.TypesIncluded) o.TypesIncluded = [];
                            for (var k = 1; k <= PropertyData.NumberTypesIncluded; k++) {
                                o.TypesIncluded[k + o.NumberTypesIncluded] = PropertyData.TypesIncluded[k];
                            }
                            o.NumberTypesIncluded = o.NumberTypesIncluded + PropertyData.NumberTypesIncluded;
                        } else if (this.BeginsWith(this._lines[j], "action ")) {
                            this.AddToObjectActions(this.GetEverythingAfter(this._lines[j], "action "), this._numberObjs);
                        } else if (this.BeginsWith(this._lines[j], "use ")) {
                            this.AddToUseInfo(this._numberObjs, this.GetEverythingAfter(this._lines[j], "use "));
                        } else if (this.BeginsWith(this._lines[j], "give ")) {
                            this.AddToGiveInfo(this._numberObjs, this.GetEverythingAfter(this._lines[j], "give "));
                        } else if (Trim(this._lines[j]) == "take") {
                            o.Take.Type = TextActionType.Default;
                            this.AddToObjectProperties("take", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "take ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "take "), 1) == "<") {
                                o.Take.Type = TextActionType.Text;
                                o.Take.Data = this.GetSimpleParameter(this._lines[j]);
                                this.AddToObjectProperties("take=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                            } else {
                                o.Take.Type = TextActionType.Script;
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
                                this.AddToObjectProperties("open=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                            } else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "open ");
                                this.AddObjectAction(this._numberObjs, "open", restOfLine);
                            }
                        } else if (Trim(this._lines[j]) == "close") {
                            this.AddToObjectProperties("close", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "close ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "close "), 1) == "<") {
                                this.AddToObjectProperties("close=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                            } else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "close ");
                                this.AddObjectAction(this._numberObjs, "close", restOfLine);
                            }
                        } else if (Trim(this._lines[j]) == "add") {
                            this.AddToObjectProperties("add", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "add ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "add "), 1) == "<") {
                                this.AddToObjectProperties("add=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                            } else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "add ");
                                this.AddObjectAction(this._numberObjs, "add", restOfLine);
                            }
                        } else if (Trim(this._lines[j]) == "remove") {
                            this.AddToObjectProperties("remove", this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "remove ")) {
                            if (Left(this.GetEverythingAfter(this._lines[j], "remove "), 1) == "<") {
                                this.AddToObjectProperties("remove=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                            } else {
                                restOfLine = this.GetEverythingAfter(this._lines[j], "remove ");
                                this.AddObjectAction(this._numberObjs, "remove", restOfLine);
                            }
                        } else if (this.BeginsWith(this._lines[j], "parent ")) {
                            this.AddToObjectProperties("parent=" + this.GetSimpleParameter(this._lines[j]), this._numberObjs, this._nullContext);
                        } else if (this.BeginsWith(this._lines[j], "list")) {
                            this.ProcessListInfo(this._lines[j], this._numberObjs);
                        }
                    } while (!(Trim(this._lines[j]) == "end define"));
                    o.DefinitionSectionEnd = j;
                    if (!hidden) {
                        o.Exists = true;
                    }
                } else if (this._gameAslVersion <= 280 && this.BeginsWith(this._lines[j], "define character")) {
                    containerRoomName = origContainerRoomName;
                    this._numberChars = this._numberChars + 1;
                    if (!this._chars) this._chars = [];
                    this._chars[this._numberChars] = new ObjectType();
                    this._chars[this._numberChars].ObjectName = this.GetSimpleParameter(this._lines[j]);
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
                            containerRoomName = this.GetSimpleParameter(this._lines[j]);
                        } else if (this.BeginsWith(this._lines[j], "prefix ")) {
                            this._chars[this._numberChars].Prefix = this.GetSimpleParameter(this._lines[j]) + " ";
                        } else if (this.BeginsWith(this._lines[j], "suffix ")) {
                            this._chars[this._numberChars].Suffix = " " + this.GetSimpleParameter(this._lines[j]);
                        } else if (Trim(this._lines[j]) == "invisible") {
                            this._chars[this._numberChars].Visible = false;
                        } else if (this.BeginsWith(this._lines[j], "alias ")) {
                            this._chars[this._numberChars].ObjectAlias = this.GetSimpleParameter(this._lines[j]);
                        } else if (this.BeginsWith(this._lines[j], "detail ")) {
                            this._chars[this._numberChars].Detail = this.GetSimpleParameter(this._lines[j]);
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
    }
    async ShowGameAbout(ctx: Context): Promise<void> {
        var version = this.FindStatement(this.GetDefineBlock("game"), "game version");
        var author = this.FindStatement(this.GetDefineBlock("game"), "game author");
        var copyright = this.FindStatement(this.GetDefineBlock("game"), "game copyright");
        var info = this.FindStatement(this.GetDefineBlock("game"), "game info");
        await this.Print("|bGame name:|cl  " + this._gameName + "|cb|xb", ctx);
        if (version != "") {
            await this.Print("|bVersion:|xb    " + version, ctx);
        }
        if (author != "") {
            await this.Print("|bAuthor:|xb     " + author, ctx);
        }
        if (copyright != "") {
            await this.Print("|bCopyright:|xb  " + copyright, ctx);
        }
        if (info != "") {
            await this.Print("", ctx);
            await this.Print(info, ctx);
        }
    }
    async ShowPicture(filename: string): Promise<void> {
        // In Quest 4.x this function would be used for showing a picture in a popup window, but
        // this is no longer supported - ALL images are displayed in-line with the game text. Any
        // image caption is displayed as text, and any image size specified is ignored.
        var caption: string = "";
        if (InStr(filename, ";") != 0) {
            caption = Trim(Mid(filename, InStr(filename, ";") + 1));
            filename = Trim(Left(filename, InStr(filename, ";") - 1));
        }
        if (InStr(filename, "@") != 0) {
            // size is ignored
            filename = Trim(Left(filename, InStr(filename, "@") - 1));
        }
        if (caption.length > 0) {
            await this.Print(caption, this._nullContext);
        }
        this.ShowPictureInText(filename);
    }
    async ShowRoomInfo(room: string, ctx: Context, noPrint: boolean = false): Promise<void> {
        if (this._gameAslVersion < 280) {
            await this.ShowRoomInfoV2(room);
            return;
        }
        var roomDisplayText: string = "";
        var descTagExist: boolean = false;
        var doorwayString: string;
        var roomAlias: string;
        var finishedFindingCommas: boolean = false;
        var prefix: string;
        var roomDisplayName: string;
        var roomDisplayNameNoFormat: string;
        var inDescription: string;
        var visibleObjects: string = "";
        var visibleObjectsNoFormat: string;
        var placeList: string;
        var lastComma: number = 0;
        var oldLastComma: number = 0;
        var descType: number = 0;
        var descLine: string = "";
        var showLookText: boolean = false;
        var lookDesc: string = "";
        var objLook: string;
        var objSuffix: string;
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
                roomDisplayText = roomDisplayText + Left(inDescription, Len(inDescription) - 1) + " " + roomDisplayName + ".\n";
            } else {
                // otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText + inDescription + "\n";
            }
        } else {
            // if no indescription line, print the default.
            roomDisplayText = roomDisplayText + "You are in " + roomDisplayName + ".\n";
        }
        this._player.LocationUpdated(UCase(Left(roomAlias, 1)) + Mid(roomAlias, 2));
        await this.SetStringContents("quest.formatroom", roomDisplayNameNoFormat, ctx);
        
        // SHOW OBJECTS *************************************************************
        visibleObjectsNoFormat = "";
        var visibleObjectsList: number[] = [];
        // of object IDs
        var count: number = 0;
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
            } else {
                visibleObjects = visibleObjects + this._objs[objId].Prefix + "|b" + this._objs[objId].ObjectAlias + "|xb" + objSuffix;
                visibleObjectsNoFormat = visibleObjectsNoFormat + this._objs[objId].Prefix + this._objs[objId].ObjectAlias;
            }
            count = count + 1;
            if (count < visibleObjectsList.length - 1) {
                visibleObjects = visibleObjects + ", ";
                visibleObjectsNoFormat = visibleObjectsNoFormat + ", ";
            } else if (count == visibleObjectsList.length - 1) {
                visibleObjects = visibleObjects + " and ";
                visibleObjectsNoFormat = visibleObjectsNoFormat + ", ";
            }
        }, this);
        if (visibleObjectsList.length > 0) {
            await this.SetStringContents("quest.formatobjects", visibleObjects, ctx);
            visibleObjects = "There is " + visibleObjects + " here.";
            await this.SetStringContents("quest.objects", visibleObjectsNoFormat, ctx);
            roomDisplayText = roomDisplayText + visibleObjects + "\n";
        } else {
            await this.SetStringContents("quest.objects", "", ctx);
            await this.SetStringContents("quest.formatobjects", "", ctx);
        }
        
        // SHOW EXITS ***************************************************************
        doorwayString = await this.UpdateDoorways(id, ctx);
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
                await this.SetStringContents("quest.doorways.places", placeList, ctx);
            } else {
                await this.SetStringContents("quest.doorways.places", "", ctx);
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
        await this.SetStringContents("quest.lookdesc", lookDesc, ctx);
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
        if (!descTagExist) {
            //Look in the "define game" block:
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "description ")) {
                    descLine = this.GetEverythingAfter(this._lines[i], "description ");
                    descTagExist = true;
                    if (Left(descLine, 1) == "<") {
                        descLine = await this.GetParameter(descLine, ctx);
                        descType = TextActionType.Text;
                    } else {
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
                await this.Print(roomDisplayText, ctx);
                if (doorwayString != "") {
                    await this.Print(doorwayString, ctx);
                }
            } else {
                //execute description tag:
                //If no script, just print the tag's parameter.
                //Otherwise, execute it as ASL script:
                if (descType == TextActionType.Text) {
                    await this.Print(descLine, ctx);
                } else {
                    await this.ExecuteScript(descLine, ctx);
                }
            }
            this.UpdateObjectList(ctx);
            // SHOW "LOOK" DESCRIPTION **************************************************
            if (showLookText) {
                if (lookDesc != "") {
                    await this.Print(lookDesc, ctx);
                }
            }
        }
    }
    CheckCollectable(id: number): void {
        // Checks to see whether a collectable item has exceeded
        // its range - if so, it resets the number to the nearest
        // valid number. It's a handy quick way of making sure that
        // a player's health doesn't reach 101%, for example.
        var max: number = 0;
        var value: number = 0;
        var min: number = 0;
        var m: number = 0;
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
                var secondStarPos = InStrFrom(firstStarPos + 1, display, "*");
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
        if (this._collectables[id].Value == 0 && !this._collectables[id].DisplayWhenZero) {
            display = "<null>";
        }
        return display;
    }
    async DisplayTextSection(section: string, ctx: Context): Promise<void> {
        var block: DefineBlock;
        block = this.DefineBlockParam("text", section);
        if (block.StartLine == 0) {
            return;
        }
        for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
            if (this._gameAslVersion >= 392) {
                // Convert string variables etc.
                await this.Print(await this.GetParameter("<" + this._lines[i] + ">", ctx), ctx);
            } else {
                await this.Print(this._lines[i], ctx);
            }
        }
        await this.Print("", ctx);
    }
    // Returns true if the system is ready to process a new command after completion - so it will be
    // in most cases, except when ExecCommand just caused an "enter" script command to complete
    async ExecCommand(input: string, ctx: Context, echo: boolean = true, runUserCommand: boolean = true, dontSetIt: boolean = false): Promise<boolean> {
        var parameter: string;
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
            this._commandOverrideModeOn = false;
            // Commands have been overridden for this command,
            // so put input into previously specified variable
            // and exit:
            await this.SetStringContents(this._commandOverrideVariable, input, ctx);
            this._commandOverrideResolve();
            return false;
        }
        var userCommandReturn: boolean = false;
        if (echo) {
            await this.Print("> " + input, ctx);
        }
        input = LCase(input);
        await this.SetStringContents("quest.originalcommand", input, ctx);
        var newCommand = " " + input + " ";
        // Convert synonyms:
        for (var i = 1; i <= this._numberSynonyms; i++) {
            var cp = 1;
            var n: number = 0;
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
        await this.SetStringContents("quest.command", input, ctx);
        // Execute any "beforeturn" script:
        var newCtx: Context = this.CopyContext(ctx);
        var globalOverride = false;
        // RoomID is 0 if there are no rooms in the game. Unlikely, but we get an RTE otherwise.
        if (roomID != 0) {
            if (this._rooms[roomID].BeforeTurnScript != "") {
                if (this.BeginsWith(this._rooms[roomID].BeforeTurnScript, "override")) {
                    await this.ExecuteScript(this.GetEverythingAfter(this._rooms[roomID].BeforeTurnScript, "override"), newCtx);
                    globalOverride = true;
                } else {
                    await this.ExecuteScript(this._rooms[roomID].BeforeTurnScript, newCtx);
                }
            }
        }
        if (this._beforeTurnScript != "" && !globalOverride) {
            await this.ExecuteScript(this._beforeTurnScript, newCtx);
        }
        // In executing BeforeTurn script, "dontprocess" sets ctx.DontProcessCommand,
        // in which case we don't process the command.
        if (!newCtx.DontProcessCommand) {
            //Try to execute user defined command, if allowed:
            userCommandReturn = false;
            if (runUserCommand) {
                userCommandReturn = await this.ExecUserCommand(input, ctx);
                if (!userCommandReturn) {
                    userCommandReturn = await this.ExecVerb(input, ctx);
                }
                if (!userCommandReturn) {
                    // Try command defined by a library
                    userCommandReturn = await this.ExecUserCommand(input, ctx, true);
                }
                if (!userCommandReturn) {
                    // Try verb defined by a library
                    userCommandReturn = await this.ExecVerb(input, ctx, true);
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
                await this.ExecSpeak(parameter, ctx);
            } else if (this.CmdStartsWith(input, "talk to ")) {
                parameter = this.GetEverythingAfter(input, "talk to ");
                await this.ExecSpeak(parameter, ctx);
            } else if (cmd == "exit" || cmd == "out" || cmd == "leave") {
                await this.GoDirection("out", ctx);
                this._lastIt = 0;
            } else if (cmd == "north" || cmd == "south" || cmd == "east" || cmd == "west") {
                await this.GoDirection(input, ctx);
                this._lastIt = 0;
            } else if (cmd == "n" || cmd == "s" || cmd == "w" || cmd == "e") {
                switch (InStr("nswe", cmd)) {
                    case 1:
                        await this.GoDirection("north", ctx);
                        break;
                    case 2:
                        await this.GoDirection("south", ctx);
                        break;
                    case 3:
                        await this.GoDirection("west", ctx);
                        break;
                    case 4:
                        await this.GoDirection("east", ctx);
                        break;
                }
                this._lastIt = 0;
            } else if (cmd == "ne" || cmd == "northeast" || cmd == "north-east" || cmd == "north east" || cmd == "go ne" || cmd == "go northeast" || cmd == "go north-east" || cmd == "go north east") {
                await this.GoDirection("northeast", ctx);
                this._lastIt = 0;
            } else if (cmd == "nw" || cmd == "northwest" || cmd == "north-west" || cmd == "north west" || cmd == "go nw" || cmd == "go northwest" || cmd == "go north-west" || cmd == "go north west") {
                await this.GoDirection("northwest", ctx);
                this._lastIt = 0;
            } else if (cmd == "se" || cmd == "southeast" || cmd == "south-east" || cmd == "south east" || cmd == "go se" || cmd == "go southeast" || cmd == "go south-east" || cmd == "go south east") {
                await this.GoDirection("southeast", ctx);
                this._lastIt = 0;
            } else if (cmd == "sw" || cmd == "southwest" || cmd == "south-west" || cmd == "south west" || cmd == "go sw" || cmd == "go southwest" || cmd == "go south-west" || cmd == "go south west") {
                await this.GoDirection("southwest", ctx);
                this._lastIt = 0;
            } else if (cmd == "up" || cmd == "u") {
                await this.GoDirection("up", ctx);
                this._lastIt = 0;
            } else if (cmd == "down" || cmd == "d") {
                await this.GoDirection("down", ctx);
                this._lastIt = 0;
            } else if (this.CmdStartsWith(input, "go ")) {
                if (this._gameAslVersion >= 410) {
                    await this._rooms[this.GetRoomId(this._currentRoom, ctx)].Exits.ExecuteGo(input, ctx);
                } else {
                    parameter = this.GetEverythingAfter(input, "go ");
                    if (parameter == "out") {
                        await this.GoDirection("out", ctx);
                    } else if (parameter == "north" || parameter == "south" || parameter == "east" || parameter == "west" || parameter == "up" || parameter == "down") {
                        await this.GoDirection(parameter, ctx);
                    } else if (this.BeginsWith(parameter, "to ")) {
                        parameter = this.GetEverythingAfter(parameter, "to ");
                        await this.GoToPlace(parameter, ctx);
                    } else {
                        await this.PlayerErrorMessage(PlayerError.BadGo, ctx);
                    }
                }
                this._lastIt = 0;
            } else if (this.CmdStartsWith(input, "give ")) {
                parameter = this.GetEverythingAfter(input, "give ");
                await this.ExecGive(parameter, ctx);
            } else if (this.CmdStartsWith(input, "take ")) {
                parameter = this.GetEverythingAfter(input, "take ");
                await this.ExecTake(parameter, ctx);
            } else if (this.CmdStartsWith(input, "drop ") && this._gameAslVersion >= 280) {
                parameter = this.GetEverythingAfter(input, "drop ");
                await this.ExecDrop(parameter, ctx);
            } else if (this.CmdStartsWith(input, "get ")) {
                parameter = this.GetEverythingAfter(input, "get ");
                await this.ExecTake(parameter, ctx);
            } else if (this.CmdStartsWith(input, "pick up ")) {
                parameter = this.GetEverythingAfter(input, "pick up ");
                await this.ExecTake(parameter, ctx);
            } else if (cmd == "pick it up" || cmd == "pick them up" || cmd == "pick this up" || cmd == "pick that up" || cmd == "pick these up" || cmd == "pick those up" || cmd == "pick him up" || cmd == "pick her up") {
                await this.ExecTake(Mid(cmd, 6, InStrFrom(7, cmd, " ") - 6), ctx);
            } else if (this.CmdStartsWith(input, "look ")) {
                await this.ExecLook(input, ctx);
            } else if (this.CmdStartsWith(input, "l ")) {
                await this.ExecLook("look " + this.GetEverythingAfter(input, "l "), ctx);
            } else if (this.CmdStartsWith(input, "examine ") && this._gameAslVersion >= 280) {
                await this.ExecExamine(input, ctx);
            } else if (this.CmdStartsWith(input, "x ") && this._gameAslVersion >= 280) {
                await this.ExecExamine("examine " + this.GetEverythingAfter(input, "x "), ctx);
            } else if (cmd == "l" || cmd == "look") {
                await this.ExecLook("look", ctx);
            } else if (cmd == "x" || cmd == "examine") {
                await this.ExecExamine("examine", ctx);
            } else if (this.CmdStartsWith(input, "use ")) {
                await this.ExecUse(input, ctx);
            } else if (this.CmdStartsWith(input, "open ") && this._gameAslVersion >= 391) {
                await this.ExecOpenClose(input, ctx);
            } else if (this.CmdStartsWith(input, "close ") && this._gameAslVersion >= 391) {
                await this.ExecOpenClose(input, ctx);
            } else if (this.CmdStartsWith(input, "put ") && this._gameAslVersion >= 391) {
                await this.ExecAddRemove(input, ctx);
            } else if (this.CmdStartsWith(input, "add ") && this._gameAslVersion >= 391) {
                await this.ExecAddRemove(input, ctx);
            } else if (this.CmdStartsWith(input, "remove ") && this._gameAslVersion >= 391) {
                await this.ExecAddRemove(input, ctx);
            } else if (cmd == "save") {
                this._player.RequestSave(null);
            } else if (cmd == "quit") {
                this.GameFinished();
            } else if (this.BeginsWith(cmd, "help")) {
                await this.ShowHelp(ctx);
                enteredHelpCommand = true;
            } else if (cmd == "about") {
                await this.ShowGameAbout(ctx);
            } else if (cmd == "clear") {
                this.DoClear();
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
                        if (this._items[j].Got) {
                            invList = invList + this._items[j].Name + ", ";
                        }
                    }
                }
                if (invList != "") {
                    invList = Left(invList, Len(invList) - 2);
                    invList = UCase(Left(invList, 1)) + Mid(invList, 2);
                    var pos = 1;
                    var lastComma: number = 0;
                    var thisComma: number = 0;
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
                    await this.Print("You are carrying:|n" + invList + ".", ctx);
                } else {
                    await this.Print("You are not carrying anything.", ctx);
                }
            } else if (this.CmdStartsWith(input, "oops ")) {
                await this.ExecOops(this.GetEverythingAfter(input, "oops "), ctx);
            } else if (this.CmdStartsWith(input, "the ")) {
                await this.ExecOops(this.GetEverythingAfter(input, "the "), ctx);
            } else {
                await this.PlayerErrorMessage(PlayerError.BadCommand, ctx);
            }
        }
        if (!skipAfterTurn) {
            // Execute any "afterturn" script:
            globalOverride = false;
            if (roomID != 0) {
                if (this._rooms[roomID].AfterTurnScript != "") {
                    if (this.BeginsWith(this._rooms[roomID].AfterTurnScript, "override")) {
                        await this.ExecuteScript(this.GetEverythingAfter(this._rooms[roomID].AfterTurnScript, "override"), ctx);
                        globalOverride = true;
                    } else {
                        await this.ExecuteScript(this._rooms[roomID].AfterTurnScript, ctx);
                    }
                }
            }
            if (this._afterTurnScript != "" && !globalOverride) {
                await this.ExecuteScript(this._afterTurnScript, ctx);
            }
        }
        await this.Print("", ctx);
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
    }
    CmdStartsWith(cmd: string, startsWith: string): boolean {
        // When we are checking user input in ExecCommand, we check things like whether
        // the player entered something beginning with "put ". We need to trim what the player entered
        // though, otherwise they might just type "put " and then we would try disambiguating an object
        // called "".
        return this.BeginsWith(Trim(cmd), startsWith);
    }
    async ExecGive(giveString: string, ctx: Context): Promise<void> {
        var article: string;
        var item: string;
        var character: string;
        var type: Thing;
        var id: number = 0;
        var script: string = "";
        var toPos = InStr(giveString, " to ");
        if (toPos == 0) {
            toPos = InStr(giveString, " the ");
            if (toPos == 0) {
                await this.PlayerErrorMessage(PlayerError.BadGive, ctx);
                return;
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
        // First see if player has "ItemToGive":
        if (this._gameAslVersion >= 280) {
            id = await this.Disambiguate(item, "inventory", ctx);
            if (id == -1) {
                await this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                this._badCmdBefore = "give";
                this._badCmdAfter = "to " + character;
                return;
            } else if (id == -2) {
                return;
            } else {
                article = this._objs[id].Article;
            }
        } else {
            // ASL2:
            var notGot = true;
            for (var i = 1; i <= this._numberItems; i++) {
                if (LCase(this._items[i].Name) == LCase(item)) {
                    if (!this._items[i].Got) {
                        notGot = true;
                        break;
                    } else {
                        notGot = false;
                    }
                }
            }
            if (notGot) {
                await this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                return;
            } else {
                article = this._objs[id].Article;
            }
        }
        if (this._gameAslVersion >= 281) {
            var foundScript = false;
            var foundObject = false;
            var giveToId = await this.Disambiguate(character, this._currentRoom, ctx);
            if (giveToId > 0) {
                foundObject = true;
            }
            if (!foundObject) {
                if (giveToId != -2) {
                    await this.PlayerErrorMessage(PlayerError.BadCharacter, ctx);
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
                    await this.SetStringContents("quest.give.object.name", this._objs[id].ObjectName, ctx);
                }
            }
            if (!foundScript) {
                //check a for give to anything:
                script = this._objs[id].GiveToAnything;
                if (script != "") {
                    foundScript = true;
                    await this.SetStringContents("quest.give.object.name", this._objs[giveToId].ObjectName, ctx);
                }
            }
            if (foundScript) {
                await this.ExecuteScript(script, ctx, id);
            } else {
                await this.SetStringContents("quest.error.charactername", this._objs[giveToId].ObjectName, ctx);
                var gender = Trim(this._objs[giveToId].Gender);
                gender = UCase(Left(gender, 1)) + Mid(gender, 2);
                await this.SetStringContents("quest.error.gender", gender, ctx);
                await this.SetStringContents("quest.error.article", article, ctx);
                await this.PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
            }
        } else {
            // ASL2:
            var block = this.GetThingBlock(character, this._currentRoom, type);
            if ((block.StartLine == 0 && block.EndLine == 0) || !this.IsAvailable(character + "@" + this._currentRoom, type, ctx)) {
                await this.PlayerErrorMessage(PlayerError.BadCharacter, ctx);
                return;
            }
            var realName = this._chars[this.GetThingNumber(character, this._currentRoom, type)].ObjectName;
            // now, see if there's a give statement for this item in
            // this characters definition:
            var giveLine = 0;
            for (var i = block.StartLine + 1; i <= block.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "give")) {
                    var ItemCheck = await this.GetParameter(this._lines[i], ctx);
                    if (LCase(ItemCheck) == LCase(item)) {
                        giveLine = i;
                    }
                }
            }
            if (giveLine == 0) {
                if (article == "") {
                    article = "it";
                }
                await this.SetStringContents("quest.error.charactername", realName, ctx);
                await this.SetStringContents("quest.error.gender", Trim(await this.GetGender(character, true, ctx)), ctx);
                await this.SetStringContents("quest.error.article", article, ctx);
                await this.PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
                return;
            }
            // now, execute the statement on GiveLine
            await this.ExecuteScript(this.GetSecondChunk(this._lines[giveLine]), ctx);
        }
    }
    async ExecLook(lookLine: string, ctx: Context): Promise<void> {
        var item: string;
        if (Trim(lookLine) == "look") {
            await this.ShowRoomInfo((this._currentRoom), ctx);
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
                var id = await this.Disambiguate(item, "inventory;" + this._currentRoom, ctx);
                if (id <= 0) {
                    if (id != -2) {
                        await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                    this._badCmdBefore = "look at";
                    return;
                }
                await this.DoLook(id, ctx);
            } else {
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
                        await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                        return;
                    } else if (lookLine == "<undefined>") {
                        await this.PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                        return;
                    }
                } else if (lookLine == "<undefined>") {
                    await this.PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                    return;
                }
                var lookData = Trim(this.GetEverythingAfter(Trim(lookLine), "look "));
                if (Left(lookData, 1) == "<") {
                    var lookText = await this.GetParameter(lookLine, ctx);
                    await this.Print(lookText, ctx);
                } else {
                    await this.ExecuteScript(lookData, ctx);
                }
            }
        }
    }
    async ExecSpeak(cmd: string, ctx: Context): Promise<void> {
        if (this.BeginsWith(cmd, "the ")) {
            cmd = this.GetEverythingAfter(cmd, "the ");
        }
        var name = cmd;
        // if the "speak" parameter of the character c$'s definition
        // is just a parameter, say it - otherwise execute it as
        // a script.
        if (this._gameAslVersion >= 281) {
            var speakLine: string = "";
            var ObjID = await this.Disambiguate(name, "inventory;" + this._currentRoom, ctx);
            if (ObjID <= 0) {
                if (ObjID != -2) {
                    await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
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
                await this.SetStringContents("quest.error.gender", UCase(Left(this._objs[ObjID].Gender, 1)) + Mid(this._objs[ObjID].Gender, 2), ctx);
                await this.PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
                return;
            }
            speakLine = this.GetEverythingAfter(speakLine, "speak ");
            if (this.BeginsWith(speakLine, "<")) {
                var text = await this.GetParameter(speakLine, ctx);
                if (this._gameAslVersion >= 350) {
                    await this.Print(text, ctx);
                } else {
                    await this.Print(Chr(34) + text + Chr(34), ctx);
                }
            } else {
                await this.ExecuteScript(speakLine, ctx, ObjID);
            }
        } else {
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
                await this.PlayerErrorMessage(PlayerError.BadCharacter, ctx);
            } else if (line == "<unfound>") {
                await this.SetStringContents("quest.error.gender", Trim(await this.GetGender(cmd, true, ctx)), ctx);
                await this.SetStringContents("quest.error.charactername", cmd, ctx);
                await this.PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
            } else if (this.BeginsWith(data, "<")) {
                data = await this.GetParameter(line, ctx);
                await this.Print(Chr(34) + data + Chr(34), ctx);
            } else {
                await this.ExecuteScript(data, ctx);
            }
        }
    }
    async ExecTake(item: string, ctx: Context): Promise<void> {
        var parentID: number = 0;
        var parentDisplayName: string;
        var foundItem = true;
        var foundTake = false;
        var id = await this.Disambiguate(item, this._currentRoom, ctx);
        if (id < 0) {
            foundItem = false;
        } else {
            foundItem = true;
        }
        if (!foundItem) {
            if (id != -2) {
                if (this._gameAslVersion >= 410) {
                    id = await this.Disambiguate(item, "inventory", ctx);
                    if (id >= 0) {
                        // Player already has this item
                        await this.PlayerErrorMessage(PlayerError.AlreadyTaken, ctx);
                    } else {
                        await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                } else if (this._gameAslVersion >= 391) {
                    await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                } else {
                    await this.PlayerErrorMessage(PlayerError.BadItem, ctx);
                }
            }
            this._badCmdBefore = "take";
            return;
        } else {
            await this.SetStringContents("quest.error.article", this._objs[id].Article, ctx);
        }
        var isInContainer = false;
        if (this._gameAslVersion >= 391) {
            var canAccessObject = this.PlayerCanAccessObject(id);
            if (!canAccessObject.CanAccessObject) {
                await this.PlayerErrorMessage_ExtendInfo(PlayerError.BadTake, ctx, canAccessObject.ErrorMsg);
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
                } else {
                    parentDisplayName = this._objs[parentID].ObjectName;
                }
                await this.Print("(first removing " + this._objs[id].Article + " from " + parentDisplayName + ")", ctx);
                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                await this.ExecCommand("remove " + this._objs[id].ObjectName, ctx, false, null, true);
                if (this.GetObjectProperty("parent", id, false, false) != "") {
                    // removing the object failed
                    return;
                }
            }
            if (t.Type == TextActionType.Default) {
                await this.PlayerErrorMessage(PlayerError.DefaultTake, ctx);
                await this.PlayerItem(item, true, ctx, id);
            } else if (t.Type == TextActionType.Text) {
                await this.Print(t.Data, ctx);
                await this.PlayerItem(item, true, ctx, id);
            } else if (t.Type == TextActionType.Script) {
                await this.ExecuteScript(t.Data, ctx, id);
            } else {
                await this.PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        } else {
            // find 'take' line
            for (var i = this._objs[id].DefinitionSectionStart + 1; i <= this._objs[id].DefinitionSectionEnd - 1; i++) {
                if (this.BeginsWith(this._lines[i], "take")) {
                    var script = Trim(this.GetEverythingAfter(Trim(this._lines[i]), "take"));
                    await this.ExecuteScript(script, ctx, id);
                    foundTake = true;
                    i = this._objs[id].DefinitionSectionEnd;
                }
            }
            if (!foundTake) {
                await this.PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        }
    }
    async ExecUse(useLine: string, ctx: Context): Promise<void> {
        var endOnWith: number = 0;
        var useDeclareLine = "";
        var useOn: string;
        var useItem: string;
        useLine = Trim(this.GetEverythingAfter(useLine, "use "));
        var roomId: number = 0;
        roomId = this.GetRoomId(this._currentRoom, ctx);
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
        var id: number = 0;
        var notGotItem: boolean = false;
        if (this._gameAslVersion >= 280) {
            var foundItem = false;
            id = await this.Disambiguate(useItem, "inventory", ctx);
            if (id > 0) {
                foundItem = true;
            }
            if (!foundItem) {
                if (id != -2) {
                    await this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                }
                if (useOn == "") {
                    this._badCmdBefore = "use";
                } else {
                    this._badCmdBefore = "use";
                    this._badCmdAfter = "on " + useOn;
                }
                return;
            }
        } else {
            notGotItem = true;
            for (var i = 1; i <= this._numberItems; i++) {
                if (LCase(this._items[i].Name) == LCase(useItem)) {
                    if (!this._items[i].Got) {
                        notGotItem = true;
                        i = this._numberItems;
                    } else {
                        notGotItem = false;
                    }
                }
            }
            if (notGotItem) {
                await this.PlayerErrorMessage(PlayerError.NoItem, ctx);
                return;
            }
        }
        var useScript: string = "";
        var foundUseScript: boolean = false;
        var foundUseOnObject: boolean = false;
        var useOnObjectId: number = 0;
        var found: boolean = false;
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
                useOnObjectId = await this.Disambiguate(useOn, this._currentRoom, ctx);
                if (useOnObjectId > 0) {
                    foundUseOnObject = true;
                } else {
                    useOnObjectId = await this.Disambiguate(useOn, "inventory", ctx);
                    if (useOnObjectId > 0) {
                        foundUseOnObject = true;
                    }
                }
                if (!foundUseOnObject) {
                    if (useOnObjectId != -2) {
                        await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
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
                        await this.SetStringContents("quest.use.object.name", this._objs[id].ObjectName, ctx);
                    }
                }
                if (!foundUseScript) {
                    //check a for use on anything:
                    useScript = this._objs[id].UseOnAnything;
                    if (useScript != "") {
                        foundUseScript = true;
                        await this.SetStringContents("quest.use.object.name", this._objs[useOnObjectId].ObjectName, ctx);
                    }
                }
            }
            if (foundUseScript) {
                await this.ExecuteScript(useScript, ctx, id);
            } else {
                await this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
            }
        } else {
            if (useOn != "") {
                useDeclareLine = await this.RetrLineParam("object", useOn, "use", useItem, ctx);
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
                    await this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
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
                useDeclareLine = await this.RetrLineParam("character", useOn, "use", useItem, ctx);
                if (useDeclareLine != "<undefined>") {
                    //Check for character availability
                    if (!this.IsAvailable(useOn, Thing.Character, ctx)) {
                        useDeclareLine = "<undefined>";
                    }
                }
                if (useDeclareLine == "<undefined>") {
                    await this.PlayerErrorMessage(PlayerError.BadThing, ctx);
                    return;
                } else if (useDeclareLine == "<unfound>") {
                    await this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    return;
                }
            } else if (useDeclareLine == "<unfound>") {
                await this.PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                return;
            }
            var script = Right(useDeclareLine, Len(useDeclareLine) - InStr(useDeclareLine, ">"));
            await this.ExecuteScript(script, ctx);
        }
    }
    ObjectActionUpdate(id: number, name: string, script: string, noUpdate: boolean = false): void {
        var objectName: string;
        var sp: number = 0;
        var ep: number = 0;
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
                    ep = InStrFrom(sp + 1, name, "'");
                    if (ep == 0) {
                        this.LogASLError("Missing ' in 'action <use " + name + "> " + this.ReportErrorLine(script));
                        return;
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
                    ep = InStrFrom(sp + 1, name, "'");
                    if (ep == 0) {
                        this.LogASLError("Missing ' in 'action <give " + name + "> " + this.ReportErrorLine(script));
                        return;
                    }
                    objectName = Mid(name, sp + 1, ep - sp - 1);
                    this.AddToGiveInfo(id, Trim(Left(name, sp - 1)) + " <" + objectName + "> " + script);
                } else {
                    this.AddToGiveInfo(id, name + " " + script);
                }
            }
        }
        if (this._gameFullyLoaded) {
            this.AddToObjectChangeLog(AppliesTo.Object, this._objs[id].ObjectName, name, "action <" + name + "> " + script);
        }
    }
    async ExecuteIf(scriptLine: string, ctx: Context): Promise<void> {
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
        var thenEndPos: number = 0;
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
        if (await this.ExecuteConditions(conditions, ctx)) {
            await this.ExecuteScript((thenScript), ctx);
        } else {
            if (elsePos != 0) {
                await this.ExecuteScript((elseScript), ctx);
            }
        }
    }
    async ExecuteScript(scriptLine: string, ctx: Context, newCallingObjectId: number = 0): Promise<void> {
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
                        await this.ExecuteScript(curScriptLine, ctx);
                    }
                    curPos = crLfPos + 1;
                } while (!(finished));
                return;
            }
            if (newCallingObjectId != 0) {
                ctx.CallingObjectId = newCallingObjectId;
            }
            if (this.BeginsWith(scriptLine, "if ")) {
                await this.ExecuteIf(scriptLine, ctx);
            } else if (this.BeginsWith(scriptLine, "select case ")) {
                await this.ExecuteSelectCase(scriptLine, ctx);
            } else if (this.BeginsWith(scriptLine, "choose ")) {
                await this.ExecuteChoose(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "set ")) {
                await this.ExecuteSet(this.GetEverythingAfter(scriptLine, "set "), ctx);
            } else if (this.BeginsWith(scriptLine, "inc ") || this.BeginsWith(scriptLine, "dec ")) {
                await this.ExecuteIncDec(scriptLine, ctx);
            } else if (this.BeginsWith(scriptLine, "say ")) {
                await this.Print(Chr(34) + await this.GetParameter(scriptLine, ctx) + Chr(34), ctx);
            } else if (this.BeginsWith(scriptLine, "do ")) {
                await this.ExecuteDo(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "doaction ")) {
                await this.ExecuteDoAction(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "give ")) {
                await this.PlayerItem(await this.GetParameter(scriptLine, ctx), true, ctx);
            } else if (this.BeginsWith(scriptLine, "lose ") || this.BeginsWith(scriptLine, "drop ")) {
                await this.PlayerItem(await this.GetParameter(scriptLine, ctx), false, ctx);
            } else if (this.BeginsWith(scriptLine, "msg ")) {
                await this.Print(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "speak ")) {
                this.LogASLError("'speak' is not supported in this version of Quest", LogType.WarningError);
            } else if (this.BeginsWith(scriptLine, "helpmsg ")) {
                await this.Print(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (Trim(LCase(scriptLine)) == "helpclose") {
                // This command does nothing in the Quest 5 player, as there is no separate help window
            } else if (this.BeginsWith(scriptLine, "goto ")) {
                await this.PlayGame(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "playerwin")) {
                await this.FinishGame(StopType.Win, ctx);
            } else if (this.BeginsWith(scriptLine, "playerlose")) {
                await this.FinishGame(StopType.Lose, ctx);
            } else if (Trim(LCase(scriptLine)) == "stop") {
                await this.FinishGame(StopType.Null, ctx);
            } else if (this.BeginsWith(scriptLine, "playwav ")) {
                this.PlayWav(await this.GetParameter(scriptLine, ctx));
            } else if (this.BeginsWith(scriptLine, "playmidi ")) {
                this.PlayMedia(await this.GetParameter(scriptLine, ctx));
            } else if (this.BeginsWith(scriptLine, "playmp3 ")) {
                this.PlayMedia(await this.GetParameter(scriptLine, ctx));
            } else if (Trim(LCase(scriptLine)) == "picture close") {
            } else if ((this._gameAslVersion >= 390 && this.BeginsWith(scriptLine, "picture popup ")) || (this._gameAslVersion >= 282 && this._gameAslVersion < 390 && this.BeginsWith(scriptLine, "picture ")) || (this._gameAslVersion < 282 && this.BeginsWith(scriptLine, "show "))) {
                // This command does nothing in the Quest 5 player, as there is no separate picture window
                await this.ShowPicture(await this.GetParameter(scriptLine, ctx));
            } else if ((this._gameAslVersion >= 390 && this.BeginsWith(scriptLine, "picture "))) {
                this.ShowPictureInText(await this.GetParameter(scriptLine, ctx));
            } else if (this.BeginsWith(scriptLine, "animate persist ")) {
                await this.ShowPicture(await this.GetParameter(scriptLine, ctx));
            } else if (this.BeginsWith(scriptLine, "animate ")) {
                await this.ShowPicture(await this.GetParameter(scriptLine, ctx));
            } else if (this.BeginsWith(scriptLine, "extract ")) {
                this.ExtractFile(await this.GetParameter(scriptLine, ctx));
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "hideobject ")) {
                this.SetAvailability(await this.GetParameter(scriptLine, ctx), false, ctx);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "showobject ")) {
                this.SetAvailability(await this.GetParameter(scriptLine, ctx), true, ctx);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "moveobject ")) {
                this.ExecMoveThing(await this.GetParameter(scriptLine, ctx), Thing.Object, ctx);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "hidechar ")) {
                this.SetAvailability(await this.GetParameter(scriptLine, ctx), false, ctx, Thing.Character);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "showchar ")) {
                this.SetAvailability(await this.GetParameter(scriptLine, ctx), true, ctx, Thing.Character);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "movechar ")) {
                this.ExecMoveThing(await this.GetParameter(scriptLine, ctx), Thing.Character, ctx);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "revealobject ")) {
                this.SetVisibility(await this.GetParameter(scriptLine, ctx), Thing.Object, true, ctx);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "concealobject ")) {
                this.SetVisibility(await this.GetParameter(scriptLine, ctx), Thing.Object, false, ctx);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "revealchar ")) {
                this.SetVisibility(await this.GetParameter(scriptLine, ctx), Thing.Character, true, ctx);
            } else if (this._gameAslVersion < 281 && this.BeginsWith(scriptLine, "concealchar ")) {
                this.SetVisibility(await this.GetParameter(scriptLine, ctx), Thing.Character, false, ctx);
            } else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "hide ")) {
                this.SetAvailability(await this.GetParameter(scriptLine, ctx), false, ctx);
            } else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "show ")) {
                this.SetAvailability(await this.GetParameter(scriptLine, ctx), true, ctx);
            } else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "move ")) {
                this.ExecMoveThing(await this.GetParameter(scriptLine, ctx), Thing.Object, ctx);
            } else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "reveal ")) {
                this.SetVisibility(await this.GetParameter(scriptLine, ctx), Thing.Object, true, ctx);
            } else if (this._gameAslVersion >= 281 && this.BeginsWith(scriptLine, "conceal ")) {
                this.SetVisibility(await this.GetParameter(scriptLine, ctx), Thing.Object, false, ctx);
            } else if (this._gameAslVersion >= 391 && this.BeginsWith(scriptLine, "open ")) {
                await this.SetOpenClose(await this.GetParameter(scriptLine, ctx), true, ctx);
            } else if (this._gameAslVersion >= 391 && this.BeginsWith(scriptLine, "close ")) {
                await this.SetOpenClose(await this.GetParameter(scriptLine, ctx), false, ctx);
            } else if (this._gameAslVersion >= 391 && this.BeginsWith(scriptLine, "add ")) {
                this.ExecAddRemoveScript(await this.GetParameter(scriptLine, ctx), true, ctx);
            } else if (this._gameAslVersion >= 391 && this.BeginsWith(scriptLine, "remove ")) {
                this.ExecAddRemoveScript(await this.GetParameter(scriptLine, ctx), false, ctx);
            } else if (this.BeginsWith(scriptLine, "clone ")) {
                this.ExecClone(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "exec ")) {
                await this.ExecExec(scriptLine, ctx);
            } else if (this.BeginsWith(scriptLine, "setstring ")) {
                await this.ExecSetString(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "setvar ")) {
                await this.ExecSetVar(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "for ")) {
                await this.ExecFor(this.GetEverythingAfter(scriptLine, "for "), ctx);
            } else if (this.BeginsWith(scriptLine, "property ")) {
                this.ExecProperty(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "type ")) {
                this.ExecType(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "action ")) {
                await this.ExecuteAction(this.GetEverythingAfter(scriptLine, "action "), ctx);
            } else if (this.BeginsWith(scriptLine, "flag ")) {
                await this.ExecuteFlag(this.GetEverythingAfter(scriptLine, "flag "), ctx);
            } else if (this.BeginsWith(scriptLine, "create ")) {
                await this.ExecuteCreate(this.GetEverythingAfter(scriptLine, "create "), ctx);
            } else if (this.BeginsWith(scriptLine, "destroy exit ")) {
                await this.DestroyExit(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "repeat ")) {
                await this.ExecuteRepeat(this.GetEverythingAfter(scriptLine, "repeat "), ctx);
            } else if (this.BeginsWith(scriptLine, "enter ")) {
                await this.ExecuteEnter(scriptLine, ctx);
            } else if (this.BeginsWith(scriptLine, "displaytext ")) {
                await this.DisplayTextSection(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "helpdisplaytext ")) {
                await this.DisplayTextSection(await this.GetParameter(scriptLine, ctx), ctx);
            } else if (this.BeginsWith(scriptLine, "font ")) {
                this.SetFont(await this.GetParameter(scriptLine, ctx));
            } else if (this.BeginsWith(scriptLine, "pause ")) {
                await this.Pause(parseInt(await this.GetParameter(scriptLine, ctx)));
            } else if (Trim(LCase(scriptLine)) == "clear") {
                this.DoClear();
            } else if (Trim(LCase(scriptLine)) == "helpclear") {
                // This command does nothing in the Quest 5 player, as there is no separate help window
            } else if (this.BeginsWith(scriptLine, "background ")) {
                this.SetBackground(await this.GetParameter(scriptLine, ctx));
            } else if (this.BeginsWith(scriptLine, "foreground ")) {
                this.SetForeground(await this.GetParameter(scriptLine, ctx));
            } else if (Trim(LCase(scriptLine)) == "nointro") {
                this._autoIntro = false;
            } else if (this.BeginsWith(scriptLine, "debug ")) {
                this.LogASLError(await this.GetParameter(scriptLine, ctx), LogType.Misc);
            } else if (this.BeginsWith(scriptLine, "mailto ")) {
                // TODO: Just write HTML directly
                var emailAddress: string = await this.GetParameter(scriptLine, ctx);
            } else if (this.BeginsWith(scriptLine, "shell ") && this._gameAslVersion < 410) {               
                this.LogASLError("'shell' is not supported in this version of Quest", LogType.WarningError);
            } else if (this.BeginsWith(scriptLine, "shellexe ") && this._gameAslVersion < 410) {
                this.LogASLError("'shellexe' is not supported in this version of Quest", LogType.WarningError);
            } else if (this.BeginsWith(scriptLine, "wait")) {
                await this.ExecuteWait(Trim(this.GetEverythingAfter(Trim(scriptLine), "wait")), ctx);
            } else if (this.BeginsWith(scriptLine, "timeron ")) {
                this.SetTimerState(await this.GetParameter(scriptLine, ctx), true);
            } else if (this.BeginsWith(scriptLine, "timeroff ")) {
                this.SetTimerState(await this.GetParameter(scriptLine, ctx), false);
            } else if (Trim(LCase(scriptLine)) == "outputon") {
                this._outPutOn = true;
                this.UpdateObjectList(ctx);
                await this.UpdateItems(ctx);
            } else if (Trim(LCase(scriptLine)) == "outputoff") {
                this._outPutOn = false;
            } else if (Trim(LCase(scriptLine)) == "panes off") {
                this._player.SetPanesVisible("off");
            } else if (Trim(LCase(scriptLine)) == "panes on") {
                this._player.SetPanesVisible("on");
            } else if (this.BeginsWith(scriptLine, "lock ")) {
                this.ExecuteLock(await this.GetParameter(scriptLine, ctx), true);
            } else if (this.BeginsWith(scriptLine, "unlock ")) {
                this.ExecuteLock(await this.GetParameter(scriptLine, ctx), false);
            } else if (this.BeginsWith(scriptLine, "playmod ") && this._gameAslVersion < 410) {
                this.LogASLError("'playmod' is not supported in this version of Quest", LogType.WarningError);
            } else if (this.BeginsWith(scriptLine, "modvolume") && this._gameAslVersion < 410) {
                this.LogASLError("'modvolume' is not supported in this version of Quest", LogType.WarningError);
            } else if (Trim(LCase(scriptLine)) == "dontprocess") {
                ctx.DontProcessCommand = true;
            } else if (this.BeginsWith(scriptLine, "return ")) {
                ctx.FunctionReturnValue = await this.GetParameter(scriptLine, ctx);
            } else {
                if (!this.BeginsWith(scriptLine, "'")) {
                    this.LogASLError("Unrecognized keyword. Line reads: '" + Trim(this.ReportErrorLine(scriptLine)) + "'", LogType.WarningError);
                }
            }
        }
        catch (e) {
            await this.Print("[An internal error occurred]", ctx);
            this.LogASLError("Error - '" + e + "' occurred processing script line '" + scriptLine + "'", LogType.InternalError);
        }
    }
    async ExecuteEnter(scriptLine: string, ctx: Context): Promise<void> {
        this._commandOverrideModeOn = true;
        this._commandOverrideVariable = await this.GetParameter(scriptLine, ctx);
        // Now, wait for CommandOverrideModeOn to be set
        // to False by ExecCommand. Execution can then resume.        
        var self = this;        
        return new Promise<void>((resolve) => self._commandOverrideResolve = resolve);
    }
    async ExecuteSet(setInstruction: string, ctx: Context): Promise<void> {
        if (this._gameAslVersion >= 280) {
            if (this.BeginsWith(setInstruction, "interval ")) {
                var interval = await this.GetParameter(setInstruction, ctx);
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
            } else if (this.BeginsWith(setInstruction, "string ")) {
                await this.ExecSetString(await this.GetParameter(setInstruction, ctx), ctx);
            } else if (this.BeginsWith(setInstruction, "numeric ")) {
                await this.ExecSetVar(await this.GetParameter(setInstruction, ctx), ctx);
            } else if (this.BeginsWith(setInstruction, "collectable ")) {
                this.ExecuteSetCollectable(await this.GetParameter(setInstruction, ctx), ctx);
            } else {
                var result = await this.SetUnknownVariableType(await this.GetParameter(setInstruction, ctx), ctx);
                if (result == SetResult.Error) {
                    this.LogASLError("Error on setting 'set " + setInstruction + "'", LogType.WarningError);
                } else if (result == SetResult.Unfound) {
                    this.LogASLError("Variable type not specified in 'set " + setInstruction + "'", LogType.WarningError);
                }
            }
        } else {
            this.ExecuteSetCollectable(await this.GetParameter(setInstruction, ctx), ctx);
        }
    }
    FindStatement(block: DefineBlock, statement: string): string {
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
                return this.GetSimpleParameter(this._lines[i]);
            }
        }
        return "";
    }
    FindLine(block: DefineBlock, statement: string, statementParam: string): string {
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
                if (UCase(Trim(this.GetSimpleParameter(this._lines[i]))) == UCase(Trim(statementParam))) {
                    return Trim(this._lines[i]);
                }
            }
        }
        return "";
    }
    GetCollectableAmount(name: string): number {
        for (var i = 1; i <= this._numCollectables; i++) {
            if (this._collectables[i].Name == name) {
                return this._collectables[i].Value;
            }
        }
        return 0;
    }
    GetSecondChunk(line: string): string {
        var endOfFirstBit = InStr(line, ">") + 1;
        var lengthOfKeyword = (Len(line) - endOfFirstBit) + 1;
        return Trim(Mid(line, endOfFirstBit, lengthOfKeyword));
    }
    async GoDirection(direction: string, ctx: Context): Promise<void> {
        // leaves the current room in direction specified by
        // 'direction'
        var dirData: TextAction = new TextAction();
        var id = this.GetRoomId(this._currentRoom, ctx);
        if (id == 0) {
            return;
        }
        if (this._gameAslVersion >= 410) {
            await this._rooms[id].Exits.ExecuteGo(direction, ctx);
            return;
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
            await this.ExecuteScript(dirData.Data, ctx);
        } else if (dirData.Data != "") {
            var newRoom = dirData.Data;
            var scp = InStr(newRoom, ";");
            if (scp != 0) {
                newRoom = Trim(Mid(newRoom, scp + 1));
            }
            await this.PlayGame(newRoom, ctx);
        } else {
            if (direction == "out") {
                await this.PlayerErrorMessage(PlayerError.DefaultOut, ctx);
            } else {
                await this.PlayerErrorMessage(PlayerError.BadPlace, ctx);
            }
        }
    }
    async GoToPlace(place: string, ctx: Context): Promise<void> {
        // leaves the current room in direction specified by
        // 'direction'
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
                await this.ExecuteScript(s, ctx);
            } else {
                await this.PlayGame(destination, ctx);
            }
        }
        if (disallowed) {
            await this.PlayerErrorMessage(PlayerError.BadPlace, ctx);
        }
    }
    async InitialiseGame(filename: string, fromQsg: boolean, onSuccess: Callback, onFailure: Callback): Promise<void> {
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
            var gameBlock: DefineBlock;
            gameBlock = self.GetDefineBlock("game");
            var aslVersion = "//";
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (self.BeginsWith(self._lines[i], "asl-version ")) {
                    aslVersion = self.GetSimpleParameter(self._lines[i]);
                }
            }
            if (aslVersion == "//") {
                self.LogASLError("File contains no version header.", LogType.WarningError);
            } else {
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
            } else {
                self._listVerbs[ListType.ObjectsList] = ["Look at", "Take", "Speak to"];
                self._listVerbs[ListType.InventoryList] = ["Look at", "Use", "Drop"];
            }
            // Get the name of the game:
            self._gameName = self.GetSimpleParameter(self._lines[self.GetDefineBlock("game").StartLine]);
            self._player.UpdateGameName(self._gameName);
            self._player.Show("Panes");
            self._player.Show("Location");
            self._player.Show("Command");
            self.SetUpGameObject();
            self.SetUpOptions();
            for (var i = self.GetDefineBlock("game").StartLine + 1; i <= self.GetDefineBlock("game").EndLine - 1; i++) {
                if (self.BeginsWith(self._lines[i], "beforesave ")) {
                    self._beforeSaveScript = self.GetEverythingAfter(self._lines[i], "beforesave ");
                } else if (self.BeginsWith(self._lines[i], "onload ")) {
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
            self.DoPrint("Error: " + err);
            onFailure();
        };
        
        this.LogASLError("Opening file " + filename, LogType.Init);
        await this.ParseFile(filename, doInitialise, onParseFailure);
        await this.UpdateItems(this._nullContext);
    }
    PlaceExist(placeName: string, ctx: Context): string {
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
                } else {
                    return checkPlace;
                }
            }
        }
        return "";
    }
    async PlayerItem(item: string, got: boolean, ctx: Context, objId: number = 0): Promise<void> {
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
                        await this.ExecuteScript(this._objs[objId].GainScript, ctx);
                    }
                } else {
                    this.MoveThing(this._objs[objId].ObjectName, this._currentRoom, Thing.Object, ctx);
                    if (this._objs[objId].LoseScript != "") {
                        await this.ExecuteScript(this._objs[objId].LoseScript, ctx);
                    }
                }
                foundObjectName = true;
            }
            if (!foundObjectName) {
                this.LogASLError("No such object '" + item + "'", LogType.WarningError);
            } else {
                await this.UpdateItems(ctx);
                this.UpdateObjectList(ctx);
            }
        } else {
            for (var i = 1; i <= this._numberItems; i++) {
                if (this._items[i].Name == item) {
                    this._items[i].Got = got;
                    i = this._numberItems;
                }
            }
            await this.UpdateItems(ctx);
        }
    }
    async PlayGame(room: string, ctx: Context): Promise<void> {
        //plays the specified room
        var id = this.GetRoomId(room, ctx);
        if (id == 0) {
            this.LogASLError("No such room '" + room + "'", LogType.WarningError);
            return;
        }
        this._currentRoom = room;
        await this.SetStringContents("quest.currentroom", room, ctx);
        if (this._gameAslVersion >= 391 && this._gameAslVersion < 410) {
            this.AddToObjectProperties("visited", this._rooms[id].ObjId, ctx);
        }
        await this.ShowRoomInfo(room, ctx);
        await this.UpdateItems(ctx);
        // Find script lines and execute them.
        if (this._rooms[id].Script != "") {
            var script = this._rooms[id].Script;
            await this.ExecuteScript(script, ctx);
        }
        if (this._gameAslVersion >= 410) {
            this.AddToObjectProperties("visited", this._rooms[id].ObjId, ctx);
        }
    }
    async Print(txt: string, ctx: Context): Promise<void> {
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
                    await this.ExecuteScript("wait <>", ctx);
                } else if (Mid(txt, i, 2) == "|c") {
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
                            await this.ExecuteScript("clear", ctx);
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
        for (var i = searchblock.StartLine + 1; i <= searchblock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], line)) {
                return Trim(this._lines[i]);
            }
        }
        return "<unfound>";
    }
    async RetrLineParam(blockType: string, param: string, line: string, lineParam: string, ctx: Context): Promise<string> {
        var searchblock: DefineBlock;
        if (blockType == "object") {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Object);
        } else {
            searchblock = this.GetThingBlock(param, this._currentRoom, Thing.Character);
        }
        if (searchblock.StartLine == 0 && searchblock.EndLine == 0) {
            return "<undefined>";
        }
        for (var i = searchblock.StartLine + 1; i <= searchblock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], line) && LCase(await this.GetParameter(this._lines[i], ctx)) == LCase(lineParam)) {
                return Trim(this._lines[i]);
            }
        }
        return "<unfound>";
    }
    SetUpCollectables(): void {
        var lastItem = false;
        this._numCollectables = 0;
        // Initialise collectables:
        // First, find the collectables section within the define
        // game block, and get its parameters:
        for (var a = this.GetDefineBlock("game").StartLine + 1; a <= this.GetDefineBlock("game").EndLine - 1; a++) {
            if (this.BeginsWith(this._lines[a], "collectables ")) {
                var collectables = Trim(this.GetSimpleParameter(this._lines[a]));
                // if collectables is a null string, there are no
                // collectables. Otherwise, there is one more object than
                // the number of commas. So, first check to see if we have
                // no objects:
                if (collectables != "") {
                    this._numCollectables = 1;
                    var pos = 1;
                    do {
                        if (!this._collectables) this._collectables = [];
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
                    } while (!lastItem);
                    this._numCollectables = this._numCollectables - 1;
                }
            }
        }
    }
    SetUpItemArrays(): void {
        var lastItem = false;
        this._numberItems = 0;
        // Initialise items:
        // First, find the possitems section within the define game
        // block, and get its parameters:
        for (var a = this.GetDefineBlock("game").StartLine + 1; a <= this.GetDefineBlock("game").EndLine - 1; a++) {
            if (this.BeginsWith(this._lines[a], "possitems ") || this.BeginsWith(this._lines[a], "items ")) {
                var possItems = this.GetSimpleParameter(this._lines[a]);
                if (possItems != "") {
                    this._numberItems = this._numberItems + 1;
                    var pos = 1;
                    do {
                        if (!this._items) this._items = [];
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
                        //lastitem set when nextcomma=0, above.
                    } while (!lastItem);
                    this._numberItems = this._numberItems - 1;
                }
            }
        }
    }
    SetUpStartItems(): void {
        var lastItem = false;
        for (var a = this.GetDefineBlock("game").StartLine + 1; a <= this.GetDefineBlock("game").EndLine - 1; a++) {
            if (this.BeginsWith(this._lines[a], "startitems ")) {
                var startItems = this.GetSimpleParameter(this._lines[a]);
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
                        //lastitem set when nextcomma=0, above.
                    } while (!lastItem);
                }
            }
        }
    }
    async ShowHelp(ctx: Context): Promise<void> {
        await this.Print("|b|cl|s14Quest Quick Help|xb|cb|s00", ctx);
        await this.Print("", ctx);
        await this.Print("|cl|bMoving|xb|cb Press the direction buttons in the 'Compass' pane, or type |bGO NORTH|xb, |bSOUTH|xb, |bE|xb, etc. |xn", ctx);
        await this.Print("To go into a place, type |bGO TO ...|xb . To leave a place, type |bOUT, EXIT|xb or |bLEAVE|xb, or press the '|crOUT|cb' button.|n", ctx);
        await this.Print("|cl|bObjects and Characters|xb|cb Use |bTAKE ...|xb, |bGIVE ... TO ...|xb, |bTALK|xb/|bSPEAK TO ...|xb, |bUSE ... ON|xb/|bWITH ...|xb, |bLOOK AT ...|xb, etc.|n", ctx);
        await this.Print("|cl|bMisc|xb|cb Type |bABOUT|xb to get information on the current game. The next turn after referring to an object or character, you can use |bIT|xb, |bHIM|xb etc. as appropriate to refer to it/him/etc. again. If you make a mistake when typing an object's name, type |bOOPS|xb followed by your correction.|n", ctx);
        await this.Print("|cl|bKeyboard shortcuts|xb|cb Press the |crup arrow|cb and |crdown arrow|cb to scroll through commands you have already typed in. Press |crEsc|cb to clear the command box.", ctx);
    }
    ReadCatalog(data: Uint8Array): void {
        var nullPos = data.indexOf(0);
        this._numResources = parseInt(this.DecryptString(data.slice(0, nullPos)));
        if (!this._resources) this._resources = [];
        data = data.slice(nullPos + 1);
        var resourceStart = 0;
        for (var i = 1; i <= this._numResources; i++) {
            this._resources[i] = new ResourceType(); 
            var r = this._resources[i];
            var nullPos = data.indexOf(0);
            r.ResourceName = this.DecryptString(data.slice(0, nullPos));
            data = data.slice(nullPos + 1);
            nullPos = data.indexOf(0);
            r.ResourceLength = parseInt(this.DecryptString(data.slice(0, nullPos)));
            data = data.slice(nullPos + 1);
            r.ResourceStart = resourceStart;
            resourceStart = resourceStart + r.ResourceLength;
            r.Extracted = false;
        }
    }
    UpdateDirButtons(dirs: string, ctx: Context): void {
        var compassExits: ListData[] = [];
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
    AddCompassExit(exitList: ListData[], name: string): void {
        exitList.push(new ListData(name, this._listVerbs[ListType.ExitsList]));
    }
    async UpdateDoorways(roomId: number, ctx: Context): Promise<string> {
        var roomDisplayText: string = "";
        var outPlace: string = "";
        var directions: string = "";
        var nsew: string = "";
        var outPlaceName: string = "";
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
            var result = await this._rooms[roomId].Exits.GetAvailableDirectionsDescription();
            roomDisplayText = result.Description;
            directions = result.List;
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
                var outPlaceAlias = this._rooms[this.GetRoomId(outPlaceName, ctx)].RoomAlias;
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
                    await this.SetStringContents("quest.doorways.out", outPlaceName, ctx);
                } else {
                    await this.SetStringContents("quest.doorways.out", outPlaceAlias, ctx);
                }
                await this.SetStringContents("quest.doorways.out.display", outPlaceAlias, ctx);
            } else {
                await this.SetStringContents("quest.doorways.out", "", ctx);
                await this.SetStringContents("quest.doorways.out.display", "", ctx);
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
                        } else {
                            cp = ncp;
                        }
                    } while (!(finished));
                    nsew = Trim(Left(nsew, cp - 1)) + " or " + Trim(Mid(nsew, cp + 1));
                }
                roomDisplayText = roomDisplayText + "You can go " + nsew + ".";
                await this.SetStringContents("quest.doorways.dirs", nsew, ctx);
            } else {
                await this.SetStringContents("quest.doorways.dirs", "", ctx);
            }
        }
        this.UpdateDirButtons(directions, ctx);
        return roomDisplayText;
    }
    UpdateInventory(ctx: Context) {
        var invList: ListData[] = [];
        if (!this._outPutOn) {
            return;
        }
        var name: string;
        if (this._gameAslVersion >= 280) {
            for (var i = 1; i <= this._numberObjs; i++) {
                if (this._objs[i].ContainerRoom == "inventory" && this._objs[i].Exists && this._objs[i].Visible) {
                    if (this._objs[i].ObjectAlias == "") {
                        name = this._objs[i].ObjectName;
                    } else {
                        name = this._objs[i].ObjectAlias;
                    }
                    invList.push(new ListData(this.CapFirst(name), this._listVerbs[ListType.InventoryList]));
                }
            }
        } else {
            for (var j = 1; j <= this._numberItems; j++) {
                if (this._items[j].Got) {
                    invList.push(new ListData(this.CapFirst(this._items[j].Name), this._listVerbs[ListType.InventoryList]));
                }
            }
        }
        this._player.UpdateInventoryList(invList);
    }
    UpdateCollectables(): void {
        if (this._numCollectables > 0) {
            var status: string = "";
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
    async UpdateItems(ctx: Context): Promise<void> {
        // displays the items a player has
        this.UpdateInventory(ctx);
        if (this._gameAslVersion >= 284) {
            await this.UpdateStatusVars(ctx);
        } else {
            this.UpdateCollectables();
        }
    }
    async FinishGame(stopType: StopType, ctx: Context): Promise<void> {
        if (stopType == StopType.Win) {
            await this.DisplayTextSection("win", ctx);
        } else if (stopType == StopType.Lose) {
            await this.DisplayTextSection("lose", ctx);
        }
        this.GameFinished();
    }
    UpdateObjectList(ctx: Context) {
        // Updates object list
        var shownPlaceName: string;
        var objSuffix: string;
        var charsViewable: string = "";
        var charsFound: number = 0;
        var noFormatObjsViewable: string;
        var charList: string;
        var objsViewable: string = "";
        var objsFound: number = 0;
        var objListString: string;
        var noFormatObjListString: string;
        if (!this._outPutOn) {
            return;
        }
        var objList: ListData[] = [];
        var exitList: ListData[] = [];
        //find the room
        var roomBlock: DefineBlock;
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
                this.SetStringContentsSimple("quest.characters", "", ctx);
            } else {
                //chop off final comma and add full stop (.)
                charList = Left(charsViewable, Len(charsViewable) - 2);
                this.SetStringContentsSimple("quest.characters", charList, ctx);
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
                } else {
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
            this.SetStringContentsSimple("quest.objects", Left(noFormatObjsViewable, Len(noFormatObjsViewable) - 2), ctx);
            this.SetStringContentsSimple("quest.formatobjects", objListString, ctx);
        } else {
            this.SetStringContentsSimple("quest.objects", "", ctx);
            this.SetStringContentsSimple("quest.formatobjects", "", ctx);
        }
        //FIND DOORWAYS
        var roomId: number = 0;
        roomId = this.GetRoomId(this._currentRoom, ctx);
        if (roomId > 0) {
            if (this._gameAslVersion >= 410) {
                var places = this._rooms[roomId].Exits.GetPlaces();
                for (var key in places) {
                    var roomExit = places[key];
                    this.AddToObjectList(objList, exitList, roomExit.GetDisplayName(), Thing.Room);
                };
            } else {
                var r = this._rooms[roomId];
                for (var i = 1; i <= r.NumberPlaces; i++) {
                    if (this._gameAslVersion >= 311 && this._rooms[roomId].Places[i].Script == "") {
                        var placeId = this.GetRoomId(this._rooms[roomId].Places[i].PlaceName, ctx);
                        if (placeId == 0) {
                            shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                        } else {
                            if (this._rooms[placeId].RoomAlias != "") {
                                shownPlaceName = this._rooms[placeId].RoomAlias;
                            } else {
                                shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                            }
                        }
                    } else {
                        shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                    }
                    this.AddToObjectList(objList, exitList, shownPlaceName, Thing.Room);
                }
            }
        }
        this._player.UpdateObjectsList(objList);
        this._gotoExits = exitList;
        this.UpdateExitsList();
    }
    async UpdateStatusVars(ctx: Context): Promise<void> {
        var displayData: string;
        var status: string = "";
        if (this._numDisplayStrings > 0) {
            for (var i = 1; i <= this._numDisplayStrings; i++) {
                displayData = await this.DisplayStatusVariableInfo(i, VarType.String, ctx);
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
                displayData = await this.DisplayStatusVariableInfo(i, VarType.Numeric, ctx);
                if (displayData != "") {
                    if (status.length > 0) {
                        status += "\n";
                    }
                    status += displayData;
                }
            }
        }
        this._player.SetStatusText(status);
    }
    UpdateVisibilityInContainers(ctx: Context, onlyParent: string = ""): void {
        // Use OnlyParent to only update objects that are contained by a specific parent
        var parentId: number = 0;
        var parent: string;
        var parentIsTransparent: boolean = false;
        var parentIsOpen: boolean = false;
        var parentIsSeen: boolean = false;
        var parentIsSurface: boolean = false;
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
                    } else {
                        this.SetAvailability(this._objs[i].ObjectName, false, ctx);
                    }
                }
            }
        }
    }
    PlayerCanAccessObject(id: number, colObjects: number[] = null): PlayerCanAccessObjectResult {
        // Called to see if a player can interact with an object (take it, open it etc.).
        // For example, if the object is on a surface which is inside a closed container,
        // the object cannot be accessed.
        var parent: string;
        var parentId: number = 0;
        var parentDisplayName: string;
        var result: PlayerCanAccessObjectResult = new PlayerCanAccessObjectResult();
        var hierarchy: string = "";
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
    }
    GetGoToExits(roomId: number, ctx: Context): string {
        var placeList: string = "";
        var shownPlaceName: string;
        for (var i = 1; i <= this._rooms[roomId].NumberPlaces; i++) {
            if (this._gameAslVersion >= 311 && this._rooms[roomId].Places[i].Script == "") {
                var placeId = this.GetRoomId(this._rooms[roomId].Places[i].PlaceName, ctx);
                if (placeId == 0) {
                    this.LogASLError("No such room '" + this._rooms[roomId].Places[i].PlaceName + "'", LogType.WarningError);
                    shownPlaceName = this._rooms[roomId].Places[i].PlaceName;
                } else {
                    if (this._rooms[placeId].RoomAlias != "") {
                        shownPlaceName = this._rooms[placeId].RoomAlias;
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
    }
    SetUpExits(): void {
        // Exits have to be set up after all the rooms have been initialised
        for (var i = 1; i <= this._numberSections; i++) {
            if (this.BeginsWith(this._lines[this._defineBlocks[i].StartLine], "define room ")) {
                var roomName = this.GetSimpleParameter(this._lines[this._defineBlocks[i].StartLine]);
                var roomId = this.GetRoomId(roomName, this._nullContext);
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
                        } while (!(nestedBlock == 0));
                    }
                    this._rooms[roomId].Exits.AddExitFromTag(this._lines[j]);
                }
            }
        }
        return;
    }
    FindExit(tag: string): RoomExit {
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
            return;
        }
        roomExit.SetIsLocked(lock);
    }
    async DoBegin(): Promise<void> {
        var gameBlock: DefineBlock = this.GetDefineBlock("game");
        var ctx: Context = new Context();
        this.SetFont("");
        this._player.SetFontSize(this._defaultFontSize);
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "background ")) {
                this.SetBackground(await this.GetParameter(this._lines[i], this._nullContext));
            }
        }
        for (var i = this.GetDefineBlock("game").StartLine + 1; i <= this.GetDefineBlock("game").EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "foreground ")) {
                this.SetForeground(await this.GetParameter(this._lines[i], this._nullContext));
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
                        await this.ExecuteScript(Trim(this.GetEverythingAfter(Trim(this._lines[i]), "lib startscript ")), ctx);
                    }
                }
            }
            for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
                if (this.BeginsWith(this._lines[i], "startscript ")) {
                    ctx = this._nullContext;
                    await this.ExecuteScript(Trim(this.GetEverythingAfter(Trim(this._lines[i]), "startscript")), ctx);
                } else if (this.BeginsWith(this._lines[i], "lib startscript ") && this._gameAslVersion < 311) {
                    ctx = this._nullContext;
                    await this.ExecuteScript(Trim(this.GetEverythingAfter(Trim(this._lines[i]), "lib startscript ")), ctx);
                }
            }
        }
        this._gameFullyLoaded = true;
        // Display intro text
        if (this._autoIntro && this._gameLoadMethod == "normal") {
            await this.DisplayTextSection("intro", this._nullContext);
        }
        // Start game from room specified by "start" statement
        var startRoom: string = "";
        for (var i = gameBlock.StartLine + 1; i <= gameBlock.EndLine - 1; i++) {
            if (this.BeginsWith(this._lines[i], "start ")) {
                startRoom = await this.GetParameter(this._lines[i], this._nullContext);
            }
        }
        if (!this._loadedFromQsg) {
            ctx = this._nullContext;
            await this.PlayGame(startRoom, ctx);
            await this.Print("", this._nullContext);
        } else {
            await this.UpdateItems(this._nullContext);
            await this.Print("Restored saved game", this._nullContext);
            await this.Print("", this._nullContext);
            await this.PlayGame(this._currentRoom, this._nullContext);
            await this.Print("", this._nullContext);
            if (this._gameAslVersion >= 391) {
                // For ASL>=391, OnLoad is now run for all games.
                ctx = this._nullContext;
                await this.ExecuteScript(this._onLoadScript, ctx);
            }
        }
        this.RaiseNextTimerTickRequest();
    }
    Finish(): void {
        this.GameFinished();
    }
    async SendCommand(command: string, elapsedTime?: number): Promise<void> {
        await this.ExecCommand(command, new Context());
        
        if (elapsedTime > 0) {
            await this.Tick(elapsedTime);
        } else {
            this.RaiseNextTimerTickRequest();
        }
    }
    async Initialise(onSuccess: Callback, onFailure: Callback): Promise<void> {
        if (LCase(Right(this._filename, 4)) == ".qsg" || this._data != null) {
            await this.OpenGame(this._filename, onSuccess, onFailure);
        } else {
            await this.InitialiseGame(this._filename, false, onSuccess, onFailure);
        }
    }
    GameFinished(): void {
        this._gameFinished = true;
    }
    GetResourcePath(filename: string): string {
        if (!this._resourceFile == null && this._resourceFile.length > 0) {
            var extractResult: string = this.ExtractFile(filename);
            return extractResult;
        }
        // TODO
        //return System.IO.Path.Combine(this._gamePath, filename);
        return null;
    }
    GetLibraryLines(libName: string): string[] {
        var lib = libraries[libName.toLowerCase()];
        if (!lib) return null;
        return lib.split("\n");
    }
    async Tick(elapsedTime: number): Promise<void> {
        var i: number = 0;
        var timerScripts: string[] = [];
        for (var i = 1; i <= this._numberTimers; i++) {
            if (this._timers[i].TimerActive) {
                if (this._timers[i].BypassThisTurn) {
                    // don't trigger timer during the turn it was first enabled
                    this._timers[i].BypassThisTurn = false;
                } else {
                    this._timers[i].TimerTicks = this._timers[i].TimerTicks + elapsedTime;
                    if (this._timers[i].TimerTicks >= this._timers[i].TimerInterval) {
                        this._timers[i].TimerTicks = 0;
                        timerScripts.push(this._timers[i].TimerAction);
                    }
                }
            }
        }
        if (timerScripts.length > 0) {
            await this.RunTimer(timerScripts);
        }
        this.RaiseNextTimerTickRequest();
    }
    async RunTimer(scripts: string[]): Promise<void> {
        await scripts.forEach(async function (script) {
            try {
                await this.ExecuteScript(script, this._nullContext);
            }
            catch (e) {
            }
        }, this);
    }
    RaiseNextTimerTickRequest(): void {
        var anyTimerActive: boolean = false;
        var nextTrigger: number = 60;
        for (var i = 1; i <= this._numberTimers; i++) {
            if (this._timers[i].TimerActive) {
                anyTimerActive = true;
                var thisNextTrigger: number = this._timers[i].TimerInterval - this._timers[i].TimerTicks;
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
        this._player.RequestNextTimerTick(nextTrigger);
    }

    GetOriginalFilenameForQSG(): string {
        if (this._originalFilename != null) {
            return this._originalFilename;
        }
        return this._gameFileName;
    }
    GetFileData(filename: string, onSuccess: FileFetcherCallback, onFailure: FileFetcherCallback): void {
        this._fileFetcher(filename, onSuccess, onFailure);
    }
    
    async GetCASFileData(filename: string) : Promise<Uint8Array> {
        var self = this;
        return new Promise<Uint8Array>(function (resolve) {
            var onSuccess = function (data: Uint8Array) {
                resolve(data);
            };
            
            var onFailure = function () {
                resolve(null);
            };
        
            self._binaryFileFetcher(filename, onSuccess, onFailure);
        });
    }
    
    DoPrint(text: string) {
        var output = this._textFormatter.OutputHTML(text);
        quest.print(output.text, !output.nobr);
    }
    
    async DoWait(): Promise<void> {
        this._player.DoWait();
        return new Promise<void>(resolve => this._waitResolve = resolve);
    }
    
    EndWait() {
        this._waitResolve();
    }
    
    Pause(duration: number): Promise<void> {
        return new Promise<void>(function (resolve) {
            setTimeout(function () {
                resolve();
            }, duration);
        });
    }
    
    async ExecuteIfAsk(question: string): Promise<boolean> {
        this._player.ShowQuestion(question);
        return new Promise<boolean>(resolve => this._askResolve = resolve);
    }
    
    SetQuestionResponse(response: string) {
        this._askResolve(response == "yes");
    }
    
    UpdateExitsList() {
        // The Quest 5.0 Player takes a combined list of compass and "go to" exits, whereas the
        // ASL4 code produces these separately. So we keep track of them separately and then
        // merge to send to the Player.
        var mergedList: ListData[] = this._compassExits.concat(this._gotoExits);
        this._player.UpdateExitsList(mergedList);
    }
    
    async Begin(): Promise<void> {
        await this.DoBegin();
    }
    
    async ShowMenu(menuData: MenuData): Promise<string> {
        this._player.ShowMenu(menuData);
        return new Promise<string>(resolve => this._menuResolve = resolve);
    }
    
    SetMenuResponse(response: string) {
        this._menuResolve(response);
    }
    
    DecryptString(input: Uint8Array): string {
        var result: string[] = [];
        for (var i = 0; i < input.length; i++) {           
            result.push(Chr(input[i] ^ 255));
        }
        return result.join("");        
    }
    
    DecryptItem(input: number): string {
        return Chr(input ^ 255);
    }
}
class ChangeLog {
    // NOTE: We only actually use the Object change log at the moment, as that is the only
    // one that has properties and actions.
    AppliesToType: AppliesTo;
    Changes: StringDictionary = {};
    // appliesTo = room or object name
    // element = the thing that's changed, e.g. an action or property name
    // changeData = the actual change info
    AddItem(appliesTo: string, element: string, changeData: string): void {
        // the first four characters of the changeData will be "prop" or "acti", so we add this to the
        // key so that actions and properties don't collide.
        var key = appliesTo + "#" + Left(changeData, 4) + "~" + element;
        this.Changes[key] = changeData;
    }
}
class RoomExit {
    Id: string;
    _objId: number = 0;
    _roomId: number = 0;
    _direction: Direction;
    _parent: RoomExits;
    _objName: string;
    _displayName: string;
    // this could be a place exit's alias
    _game: LegacyGame;
    constructor(game: LegacyGame) {
        this._game = game;
        game._numberObjs = game._numberObjs + 1;
        if (!game._objs) game._objs = [];
        game._objs[game._numberObjs] = new ObjectType();
        this._objId = game._numberObjs;
        var o = game._objs[this._objId];
        o.IsExit = true;
        o.Visible = true;
        o.Exists = true;
    }
    SetExitProperty(propertyName: string, value: string): void {
        this._game.AddToObjectProperties(propertyName + "=" + value, this._objId, this._game._nullContext);
    }
    GetExitProperty(propertyName: string): string {
        return this._game.GetObjectProperty(propertyName, this._objId, false, false);
    }
    SetExitPropertyBool(propertyName: string, value: boolean): void {
        var sPropertyString: string;
        sPropertyString = propertyName;
        if (!value) {
            sPropertyString = "not " + sPropertyString;
        }
        this._game.AddToObjectProperties(sPropertyString, this._objId, this._game._nullContext);
    }
    GetExitPropertyBool(propertyName: string): boolean {
        return (this._game.GetObjectProperty(propertyName, this._objId, true, false) == "yes");
    }
    SetAction(actionName: string, value: string): void {
        this._game.AddToObjectActions("<" + actionName + "> " + value, this._objId);
    }
    SetToRoom(value: string): void {
        this.SetExitProperty("to", value);
        this.UpdateObjectName();
    }
    GetToRoom(): string {
        return this.GetExitProperty("to");
    }
    SetPrefix(value: string): void {
        this.SetExitProperty("prefix", value);
    }
    GetPrefix(): string {
        return this.GetExitProperty("prefix");
    }
    SetScript(value: string): void {
        if (Len(value) > 0) {
            this.SetAction("script", value);
        }
    }
    IsScript(): boolean {
        return this._game.HasAction(this._objId, "script");
    }
    SetDirection(value: Direction): void {
        this._direction = value;
        if (value != Direction.None) {
            this.UpdateObjectName();
        }
    }
    GetDirection(): Direction {
        return this._direction;
    }
    SetParent(value: RoomExits): void {
        this._parent = value;
    }
    GetParent(): RoomExits {
        return this._parent;
    }
    GetObjId(): number {
        return this._objId;
    }
    GetRoomId(): number {
        if (this._roomId == 0) {
            this._roomId = this._game.GetRoomId(this.GetToRoom(), this._game._nullContext);
        }
        return this._roomId;
    }
    GetDisplayName(): string {
        return this._displayName;
    }
    GetDisplayText(): string {
        return this._displayName;
    }
    SetIsLocked(value: boolean): void {
        this.SetExitPropertyBool("locked", value);
    }
    GetIsLocked(): boolean {
        return this.GetExitPropertyBool("locked");
    }
    SetLockMessage(value: string): void {
        this.SetExitProperty("lockmessage", value);
    }
    GetLockMessage(): string {
        return this.GetExitProperty("lockmessage");
    }
    async RunAction(actionName: string, ctx: Context): Promise<void> {
        await this._game.DoAction(this._objId, actionName, ctx);
    }
    async RunScript(ctx: Context): Promise<void> {
        await this.RunAction("script", ctx);
    }
    UpdateObjectName(): void {
        var objName: string;
        var lastExitId: number = 0;
        var parentRoom: string;
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
        } else {
            var lastExitIdString: string = this._game.GetObjectProperty("quest.lastexitid", (this._parent.GetObjId()), null, false);
            if (lastExitIdString.length == 0) {
                lastExitId = 0;
            }
            lastExitId = lastExitId + 1;
            this._game.AddToObjectProperties("quest.lastexitid=" + (lastExitId).toString(), (this._parent.GetObjId()), this._game._nullContext);
            objName = objName + ".exit" + (lastExitId).toString();
            if (this.GetRoomId() == 0) {
                // the room we're pointing at might not exist, especially if this is a script exit
                this._displayName = this.GetToRoom();
            } else {
                if (Len(this._game._rooms[this.GetRoomId()].RoomAlias) > 0) {
                    this._displayName = this._game._rooms[this.GetRoomId()].RoomAlias;
                } else {
                    this._displayName = this.GetToRoom();
                }
            }
            this._game._objs[this._objId].ObjectAlias = this._displayName;
            this.SetPrefix(this._game._rooms[this.GetRoomId()].Prefix);
        }
        this._game._objs[this._objId].ObjectName = objName;
        this._game._objs[this._objId].ContainerRoom = parentRoom;
        this._objName = objName;
    }
    async Go(ctx: Context): Promise<void> {
        if (this.GetIsLocked()) {
            if (this.GetExitPropertyBool("lockmessage")) {
                await this._game.Print(this.GetExitProperty("lockmessage"), ctx);
            } else {
                await this._game.PlayerErrorMessage(PlayerError.Locked, ctx);
            }
        } else {
            if (this.IsScript()) {
                await this.RunScript(ctx);
            } else {
                await this._game.PlayGame(this.GetToRoom(), ctx);
            }
        }
    }
}

interface NumberRoomExitDictionary {
    [index: number]: RoomExit;
}

interface StringRoomExitDictionary {
    [index: string]: RoomExit;
}

class RoomExits {
    _directions: NumberRoomExitDictionary = {};
    _places: StringRoomExitDictionary = {};
    _objId: number = 0;
    _allExits: RoomExit[];
    _regenerateAllExits: boolean;
    _game: LegacyGame;
    constructor(game: LegacyGame) {
        this._game = game;
        this._regenerateAllExits = true;
    }
    SetDirection(direction: Direction): RoomExit {
        var roomExit: RoomExit;
        if (this._directions[direction]) {
            roomExit = this._directions[direction];
            this._game._objs[roomExit.GetObjId()].Exists = true;
        } else {
            roomExit = new RoomExit(this._game);
            this._directions[direction] = roomExit;
        }
        this._regenerateAllExits = true;
        return roomExit;
    }
    GetDirectionExit(direction: Direction): RoomExit {
        if (this._directions[direction]) {
            return this._directions[direction];
        }
        return null;
    }
    AddPlaceExit(roomExit: RoomExit): void {
        this._places[roomExit.GetToRoom()] = roomExit;
        this._regenerateAllExits = true;
    }
    AddExitFromTag(tag: string): void {
        var thisDir: Direction;
        var roomExit: RoomExit = null;
        var params: string[] = [];
        var afterParam: string;
        var param: boolean = false;
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
            return;
        }
        if (thisDir != Direction.None) {
            // This will reuse an existing Exit object if we're resetting
            // the destination of an existing directional exit.
            roomExit = this.SetDirection(thisDir);
        } else {
            roomExit = new RoomExit(this._game);
        }
        roomExit.SetParent(this);
        roomExit.SetDirection(thisDir);
        if (this._game.BeginsWith(tag, "locked ")) {
            roomExit.SetIsLocked(true);
            tag = this._game.GetEverythingAfter(tag, "locked ");
        }
        if (Left(Trim(tag), 1) == "<") {
            params = Split(this._game.GetSimpleParameter(tag), ";");
            afterParam = Trim(Mid(tag, InStr(tag, ">") + 1));
            param = true;
        } else {
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
            } else {
                // A directional exit with a script may have no parameter.
                // If it does have a parameter it will be a lock message.
                if (param) {
                    roomExit.SetLockMessage(params[0]);
                }
            }
        } else {
            roomExit.SetToRoom(params[0]);
            if (UBound(params) > 0) {
                roomExit.SetLockMessage(params[1]);
            }
        }
        if (thisDir == Direction.None) {
            this.AddPlaceExit(roomExit);
        }
    }
    async AddExitFromCreateScript(script: string, ctx: Context): Promise<void> {
        // sScript is the "create exit ..." script, but without the "create exit" at the beginning.
        // So it's very similar to creating an exit from a tag, except we have the source room
        // name before the semicolon (which we don't even care about as we ARE the source room).
        var param: string;
        var params: string[];
        var paramStart: number = 0;
        var paramEnd: number = 0;
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
        param = await this._game.GetParameter(script, ctx);
        params = Split(param, ";");
        paramStart = InStr(script, "<");
        paramEnd = InStrFrom(paramStart, script, ">");
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
                this._game.LogASLError("No exit destination given in 'create exit " + script + "'", LogType.WarningError);
                return;
            }
            // Place exit so add "place" tag at the beginning
            this.AddExitFromTag("place <" + Trim(params[1]) + Mid(script, paramEnd));
        }
    }
    SetObjId(value: number): void {
        this._objId = value;
    }
    GetObjId(): number {
        return this._objId;
    }
    GetPlaces(): StringRoomExitDictionary {
        return this._places;
    }
    async ExecuteGo(cmd: string, ctx: Context): Promise<void> {
        // This will handle "n", "go east", "go [to] library" etc.
        var exitId: number = 0;
        var roomExit: RoomExit;
        if (this._game.BeginsWith(cmd, "go to ")) {
            cmd = this._game.GetEverythingAfter(cmd, "go to ");
        } else if (this._game.BeginsWith(cmd, "go ")) {
            cmd = this._game.GetEverythingAfter(cmd, "go ");
        }
        exitId = await this._game.Disambiguate(cmd, this._game._currentRoom, ctx, true);
        if (exitId == -1) {
            await this._game.PlayerErrorMessage(PlayerError.BadPlace, ctx);
        } else {
            roomExit = this.GetExitByObjectId(exitId);
            await roomExit.Go(ctx);
        }
    }
    async GetAvailableDirectionsDescription(): Promise<GetAvailableDirectionsResult> {
        var roomExit: RoomExit;
        var count: number = 0;
        var descPrefix: string;
        var orString: string;
        descPrefix = "You can go";
        orString = "or";
        var list = "";
        var description = "";
        count = 0;
        var allExits = this.AllExits();
        allExits.forEach(function (roomExit) {
            count = count + 1;
            list = list + this.GetDirectionToken(roomExit.GetDirection());
            description = description + this.GetDirectionNameDisplay(roomExit);
            if (count < allExits.length - 1) {
                description = description + ", ";
            } else if (count == allExits.length - 1) {
                description = description + " " + orString + " ";
            }
        }, this);
        await this._game.SetStringContents("quest.doorways", description, this._game._nullContext);
        if (count > 0) {
            description = descPrefix + " " + description + ".";
        }
        var result = new GetAvailableDirectionsResult();
        result.Description = description;
        result.List = list;
        return result;
    }
    GetDirectionName(dir: Direction): string {
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
    }
    GetDirectionEnum(dir: string): Direction {
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
    }
    GetDirectionToken(dir: Direction): string {
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
    }
    GetDirectionNameDisplay(roomExit: RoomExit): string {
        if (roomExit.GetDirection() != Direction.None) {
            var dir = this.GetDirectionName(roomExit.GetDirection());
            return "|b" + dir + "|xb";
        }
        var display = "|b" + roomExit.GetDisplayName() + "|xb";
        if (Len(roomExit.GetPrefix()) > 0) {
            display = roomExit.GetPrefix() + " " + display;
        }
        return "to " + display;
    }
    GetExitByObjectId(id: number): RoomExit {
        var result: RoomExit;
        this.AllExits().forEach(function (roomExit) {
            if (roomExit.GetObjId() == id) {
                result = roomExit;
                return;
            }
        }, this);
        return result;
    }
    AllExits(): RoomExit[] {
        if (!this._regenerateAllExits) {
            return this._allExits;
        }
        this._allExits = [];
        for (var dir in this._directions) {
            var roomExit = this._directions[dir];
            if (this._game._objs[roomExit.GetObjId()].Exists) {
                this._allExits.push(this._directions[dir]);
            }
        };
        for (var dir in this._places) {
            var roomExit = this._places[dir];
            if (this._game._objs[roomExit.GetObjId()].Exists) {
                this._allExits.push(this._places[dir]);
            }
        };
        return this._allExits;
    }
    RemoveExit(roomExit: RoomExit): void {
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
    }
}
class TextFormatterResult {
    text: string;
    nobr: boolean;
}
class TextFormatter {
    // the Player generates style tags for us
    // so all we need to do is have some kind of <color> <fontsize> <justify> tags etc.
    // it would actually be a really good idea for the player to handle the <wait> and <clear> tags too...?
    bold: boolean;
    italic: boolean;
    underline: boolean;
    colour: string = "";
    fontSize: number = 0;
    defaultFontSize: number = 0;
    align: string = "";
    fontFamily: string = "";
    foreground: string = "";
    OutputHTML(input: string): TextFormatterResult {
        var output: string = "";
        var position: number = 0;
        var codePosition: number = 0;
        var finished: boolean = false;
        var nobr: boolean = false;
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
                // can also have size codes
            } else {
                output += this.FormatText(input.substr(position, codePosition - position));
                position = codePosition + 1;
                var oneCharCode: string = "";
                var twoCharCode: string = "";
                if (position < input.length) {
                    oneCharCode = input.substr(position, 1);
                }
                if (position < (input.length - 1)) {
                    twoCharCode = input.substr(position, 2);
                }
                var foundCode: boolean = true;
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
                } else {
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
                            var sizeCode: string = input.substr(position + 1, 2);
                            var fontSize: number = parseInt(sizeCode);
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
    }
    FormatText(input: string): string {
        if (input.length == 0) {
            return input;
        }
        var output: string = "";
        if (this.align.length > 0) {
            output += '<div style="text-align:' + this.align + '">';
        }
        var fontSize = this.fontSize;
        if (fontSize == 0) fontSize = this.defaultFontSize;
        output += '<span style="font-family:' + this.fontFamily +  ';font-size:' + fontSize.toString() + 'pt">';
        if (this.colour.length > 0) {
            output += '<span style="color:' + this.colour + '">';
        }
        else if (this.foreground.length > 0) {
            output += '<span style="color:' + this.foreground + '">';
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
    }
}
var libraries: StringDictionary = {};
libraries["stdverbs.lib"] = `!library
!asl-version <400>
!name <Additional verbs>
!version <1.0>
!author <Alex Warren>
! This library adds default responses for a number of common commands that players might use in a game. It also allows players to type multiple commands on the same command line.

' STDVERBS.LIB v1.0
' for Quest 4.0
' Copyright © 2007 Axe Software. Please do not modify this library.

!addto game
	verb <buy> msg <You can't buy #(quest.lastobject):article#.>
	verb <climb> msg <You can't climb #(quest.lastobject):article#.>
	verb <drink> msg <You can't drink #(quest.lastobject):article#.>
	verb <eat> msg <You can't eat #(quest.lastobject):article#.>
	verb <hit> msg <You can't hit #(quest.lastobject):article#.>
	verb <kill> msg <You can't kill #(quest.lastobject):article#.>
	verb <kiss> msg <I don't think #(quest.lastobject):article# would like that.>
	verb <knock> msg <You can't knock #(quest.lastobject):article#.>
	verb <lick> msg <You can't lick #(quest.lastobject):article#.>
	verb <listen to> msg <You listen, but #(quest.lastobject):article# makes no sound.>
	verb <lock> msg <You can't lock #(quest.lastobject):article#.>
	verb <move> msg <You can't move #(quest.lastobject):article#.>
	verb <pull> msg <You can't pull #(quest.lastobject):article#.>
	verb <push> msg <You can't push #(quest.lastobject):article#.>
	verb <read> msg <You can't read #(quest.lastobject):article#.>
	verb <search> msg <You can't search #(quest.lastobject):article#.>
	verb <show> msg <You can't show #(quest.lastobject):article#.>
	verb <sit on> msg <You can't sit on #(quest.lastobject):article#.>
	verb <smell; sniff> msg <You sniff, but #(quest.lastobject):article# doesn't smell of much.>
	verb <taste> msg <You can't taste #(quest.lastobject):article#.>
	verb <throw> msg <You can't throw #(quest.lastobject):article#.>
	verb <tie> msg <You can't tie #(quest.lastobject):article#.>
	verb <touch> msg <You can't touch #(quest.lastobject):article#.>
	verb <turn on> msg <You can't turn #(quest.lastobject):article# on.>
	verb <turn off> msg <You can't turn #(quest.lastobject):article# off.>
	verb <turn> msg <You can't turn #(quest.lastobject):article#.>
	verb <unlock> msg <You can't unlock #(quest.lastobject):article#.>
	verb <untie> msg <You can't untie #(quest.lastobject):article#.>
	verb <wear> msg <You can't wear #(quest.lastobject):article#.>
	
	command <ask #stdverbs.command#> msg <You get no reply.>
	command <listen> msg <You can't hear much.>
	command <jump> msg <You jump, but nothing happens.>
	command <tell #stdverbs.command#> msg <You get no reply.>
	command <sit down; sleep> msg <No time for lounging about now.>
	command <wait> msg <Time passes.>
	command <xyzzy> msg <Surprisingly, absolutely nothing happens.>
	
	command <#stdverbs.command#. #stdverbs.command2#; _
		#stdverbs.command#, then #stdverbs.command2#; _
		#stdverbs.command#, and then #stdverbs.command2#; _
		#stdverbs.command#, #stdverbs.command2#; _
		#stdverbs.command# and then #stdverbs.command2#; _
		#stdverbs.command# then #stdverbs.command2#> {
		exec <#stdverbs.command#>
		exec <#stdverbs.command2#>
	}
	
	command <#stdverbs.command#.> exec <#stdverbs.command#>
!end`;
libraries["standard.lib"] = `!library

'----------------------------------------------------------------------

'Filename:	standard.lib

'Version:		beta 3a ( minor bug fix of beta 3 )

'Type:		ASL Library ( for Quest 2.0/2.1 only )

'By:			A.G. Bampton (originally 10/08/1999)   

'Revision:	21/12/1999

'Purpose/ 	See STDLIB.RTF for details of what this library does
'Usage:		and how to use it.
'
'WARNING:	I STRONGLY ADVISE YOU NEVER CHANGE ANY CODE IN THIS
'	    	LIBRARY! It's complete and functional 'as is' and if
'	    	it has been altered I will not be able to offer any
'	    	support for it's use. Tailor/modify the way it works
'	    	in external code - either within your 'ASL' game code
'	    	or, IF YOU ARE USING QUEST PRO AND CAN COMPILE YOUR
'	    	GAME TO A 'CAS' FILE ONLY, by including a customising
'	    	library as per 'library_demo.asl' my standard library
'	    	demo. 
'----------------------------------------------------------------------

!asl-version <200>

!addto game
	command <look at #object#> do <Look_Proc>
	command <x #object#> do <Look_Proc>
	command <drop #object# in #container#> do <Put_In_Container_Proc>
	command <drop #object# down> do <Drop_Proc>
	command <drop #object#> do <Drop_Proc>
	command <give #give_com_string#> do <Alternate_Give_Proc>
	command <take #object# from #character#> do <Take_Back_Proc>
	command <take #character#'s #object#> do <Take_Back_Proc>
	command <take #character#s #object#> do <Take_Back_Proc>
	command <take #character#' #object#> do <Take_Back_Proc>
	command <take #object#> do <Special_Take_Proc>
	command <customdrop #object# in #container#> do <Put_In_Container_Proc>
	command <customdrop #object#> do <Drop_Proc>
	command <customtake #object# from #character#> do <Take_Back_Proc>
	command <customtake #object#> do <Special_Take_Proc>
!end


!addto synonyms
	examine; inspect = look at
	drop the; put the; put down the; put down; put = drop
	give back the; give back; give the = give
	get the; get; take back the; take back; take the = take
	out of the; out of; back from the; back from; from the = from
	in to; in to the; into the; into; inside the; inside = in
	back to the; back to; to the = to 
!end

'-------------------------------------------------------------------

'The rest of this library is appended to the calling ASL file


define procedure <Alternate_Give_Proc>
	setvar <found_to;0>
	for <to_true;1;$lengthof(#give_com_string#)$> do <to_test>
		if not is <%found_to%;0> then {
		exec <give #give_com_string#;normal>
		setvar <found_to;0>
		}
		else {
		for <space_true;1;$lengthof(#give_com_string#)$> do <space_test>
		setstring <give_char;$left(#give_com_string#;%found_space%)$>
		setstring <give_obj;$mid(#give_com_string#;%found_space%)$>
		exec <give #give_obj# to #give_char#;normal>
		}
end define
	

define procedure <to_test>
	setstring <to_test_for;$mid(o to c;2;4)$>
	setstring <test_part;$mid(#give_com_string#;%to_true%;4)$>
	if is <#to_test_for#;#test_part#> then setvar <found_to;%to_true%>
end define
	

define procedure <space_test>
	setstring <to_test_for;$mid(c o;2;1)$>
	setstring <test_part;$mid(#give_com_string#;%space_true%;1)$>
	if is <#to_test_for#;#test_part#> then setvar <found_space;%space_true%>
end define


define procedure <Look_Proc>

if not is <#standard.lib.version#;> then {

	if is <$instr(#standard.lib.characters#;#object#)$;0> then {
	setstring <where_it_is;$locationof(#object#)$>

		if not is <$instr(#standard.lib.containers#;#where_it_is#)$;0> then {

			if not is <$instr(#standard.lib.characters#;#where_it_is#)$;0> then {

				if here <#where_it_is#> then {
				moveobject <#object#;#quest.currentroom#>
				showobject <#object#>
				exec <look at #object#;normal>
				moveobject <#object#;#where_it_is#>
				}
				else {
				exec <look at #object#;normal>
				}

			}
			else {

				if here <#where_it_is#> or got <#where_it_is#> then {
				moveobject <#object#;#quest.currentroom#>
				showobject <#object#>
				exec <look at #object#;normal>
				moveobject <#object#;#where_it_is#>
				}
				else {
				exec <look at #object#;normal>
				}

			}

		}
		else {

			if got <#object#> then {
			moveobject <#object#;#quest.currentroom#>
			showobject <#object#>
			exec <look at #object#;normal>
			do <Check_Contents_Proc>
			hideobject <#object#>
			moveobject <#object#;#where_it_is#>
			}
			else {
			exec <look at #object#;normal>

				if here <#object#> then {
				do <Check_Contents_Proc>
				}

			}	

		}

	}
	else {
	exec <look at #object#;normal>

		if here <#object#> then {
		do <Check_Contents_Proc>
		}
	}

}
else {
do <Old_Look_Proc>
}

end define

define procedure <Check_Contents_Proc>

    	if not is <$instr(#standard.lib.containers#;#object#)$;0> then {
	    setstring <where_we_were;#quest.currentroom#>
    	outputoff
	    goto <#object#>
		    if is <$lengthof(#quest.objects#)$;0> then {
    		goto <#where_we_were#>
	    	outputon
		    }
    		else {
	    	setstring <parsed_list;$gettag(#object#;prefix)$ __
		    $parse_object_list$>
    		goto <#where_we_were#>
	    	outputon
		    msg <#parsed_list#>
		    }
    	}
end define

define function <parse_object_list>
	setvar <found_comma;0>
	for <last_comma;1;$lengthof(#quest.formatobjects#)$> {
	do <Last_Comma_Proc>
	}
		if not is <%found_comma%;0> then { 
		setvar <remaining;%last_comma%-%found_comma%>
		setvar <remaining;%remaining%-1>
		setvar <found_comma;%found_comma%-1>
		setstring <left_part;$left(#quest.formatobjects#;__
		%found_comma%)$>
		setstring <right_part;$right(#quest.formatobjects#;__
		%remaining%)$>
		setstring <parsed_object_list;#left_part# and #right_part#.>
		}
		if is <%found_comma%;0> then {
		setstring <parsed_object_list;#quest.formatobjects#.>
		}
		return <#parsed_object_list#>
end define

define procedure <Last_Comma_Proc>
	setstring <test_part;$mid(#quest.formatobjects#;%last_comma%;1)$>
	if is <#test_part#;,> then setvar <found_comma;%last_comma%>
end define


define procedure <Drop_Proc>

	if is <$instr(#standard.lib.characters#;#object#)$;0> then {
	do <override_permitted>  
	if is <#override#;yes> then {

		if got <#object#> then {
		lose <#object#>
		moveobject <#object#;#quest.currentroom#>
		showobject <#object#>
		setvar <custom_message; $lengthof(#std.lib.message.drop#)$>
			if is <%custom_message%; 0> then msg <You drop the #object#.>
			else msg <#std.lib.message.drop#.>
		}
		else {
		setvar <custom_message; $lengthof(#std.lib.message.notcarried#)$>
			if is <%custom_message%; 0> then msg <You're not holding the #object#.>
			else msg <#std.lib.message.notcarried#.>
		}

	}
	}
	else msg <Sorry, I don't understand '#quest.originalcommand#'.>
end define


define procedure <Special_Take_Proc>

if not is <#standard.lib.version#;> then {

	if is <$instr(#standard.lib.characters#;#object#)$;0> then {
	do <override_permitted>  
		if is <#override#;yes> then {
		setstring <where_it_is;$locationof(#object#)$>
			if not is <$instr(#standard.lib.containers#;#where_it_is#)$;0> then {
				if not is <$instr(#standard.lib.characters#;#where_it_is#)$;0> then {
					if here <#where_it_is#> then {
					exec <take #object# from #where_it_is#>
					}
					else exec <take #object#;normal>
				}
				else {
					if here <#where_it_is#> or got <#where_it_is#> then {
					exec <take #object# from #where_it_is#>
					}
					else exec <take #object#;normal>
				}
			}
			else {
				if got <#object#> then {
				setvar <custom_message; $lengthof(#std.lib.message.alreadygot#)$>
					if is <%custom_message%; 0> then {
					msg <You already have it.>
					}
					else {
					msg <#std.lib.message.alreadygot#.>
					}
				}
				else {
					if here <#object#> then {
					exec <take #object#;normal>
					}
					else {
					exec <take #object#;normal>
					}
				}
			}
		}
	}
	else msg <Sorry, I don't understand '#quest.originalcommand#'.>
}
else {
do <Old_Special_Take_Proc>
}
end define


define procedure <Take_Back_Proc>

if not is <#standard.lib.version#;> then {

	if not is <$instr(#standard.lib.containers#;#character#)$;0> then {
	if is <$instr(#standard.lib.characters#;#character#)$;0> then {
	setstring <container;#character#>
	}
	else {
	setstring <container;#object#>
	}
	do <override_permitted>  
	if is <#override#;yes> then {

	if here <#character#> or got <#container#> then {
		if is <$locationof(#object#)$;#character#> then {
		moveobject <#object#;#quest.currentroom#>
		give <#object#>
		hideobject <#object#>
		setvar <custom_message; $lengthof(#std.lib.message.takefrom#)$>
			if is <%custom_message%; 0> then {
				if is <$gettag(#character#;look)$;character> then {
				msg <You reach out and take the #object# from $capfirst(#character#)$.> }
				else msg <You take the #object# out of the #character#.> 
			}
			else msg <#std.lib.message.takefrom#.>
		}
		else {
			setvar <custom_message; $lengthof(#std.lib.message.objnotheld#)$>
			if is <%custom_message%;0> then {
				if is <$gettag(#character#;look)$;character> then {
				msg <You can't do that, $capfirst(#character#)$ doesn't have the #object#.> }
				else msg <You can't do that, the #object# isn't in the #character#.>
			}
			else msg <#std.lib.message.objnotheld#.>
		}
	}
	else {
	setvar <custom_message; $lengthof(#std.lib.message.charnothere#)$>
		if is <%custom_message%; 0> then {
			if is <$gettag(#character#;look)$;character> then {
			msg <You can't do that, $capfirst(#character#)$ isn't here.> }
			else msg <You can't do that, the #character# isn't here.>
		}
		else msg <#std.lib.message.charnothere#.>
	}

	}
	}
	else msg <Sorry, I don't understand '#quest.originalcommand#'.>
}
else {
do <Old_Take_Back_Proc>
}
end define


define procedure <Put_In_Container_Proc>
	do <override_permitted>  
	if is <#override#;yes> then {

	if is <#object#;#container#> then do <Put_In_Self_Proc>
	else {
	setstring <test_prefix;$gettag(#container#;prefix)$>
		if is <$lengthof(#test_prefix#)$;0> then do <Not_A_Container_Proc>
		else do <Put_In_Container_Verified_Proc>
	}

	}
	else msg <Sorry, I don't understand '#quest.originalcommand#'.>
end define


define procedure <Not_A_Container_Proc>
	setvar <custom_message; $lengthof(#std.lib.message.notacontainer#)$>
		if is <%custom_message%; 0> then msg <It's not possible to do that.>
		else msg <#std.library.message.notacontainer#.>					
end define


define procedure <Put_In_Self_Proc>
	setvar <custom_message; $lengthof(#std.lib.message.putinself#)$>
		if is <%custom_message%; 0> then msg <That would be some feat if you could manage it!>
		else msg <#std.library.message.putinself#.>					
end define

define procedure <Put_In_Container_Verified_Proc>
	if is <$gettag(#container#;look)$;character> then {
	msg <( assuming you meant "|iGive #object# to $capfirst(#container#)$"|xi. )>
	exec <give #object# to #container#>
	}
	else if is <$gettag(#object#;look)$;character> and here <#object#> then do <Put_Char_In_Obj_Proc>
	else if is <$gettag(#object#;look)$;character> then do <Character_Not_Here_Proc>
	else {
	if got <#object#> and got <#container#> then do <Put_It_In_Proc>
		else {
			if got <#object#> and here <#container#> then do <Put_It_In_Proc>
			else {
				if got <#object#> then do <No_Container_Here_Proc>
				else {
				do <Nothing_To_Put_In_Proc>
				}
			}
		}
	}
end define


define procedure <Put_Char_In_Obj_Proc>
	setvar <custom_message; $lengthof(#std.lib.message.putcharinobj#)$>
		if is <%custom_message%; 0> then msg <You can't do that with $capfirst(#object#)$.>
		else msg <#std.lib.message.putcharinobj#.> 
end define


define procedure <No_Container_Here_Proc>
	setvar <custom_message; $lengthof(#std.lib.message.nocontainerhere#)$>
		if is <%custom_message%; 0> then msg <The #container# isn't available for you __
		to put things in at the moment.>
		else msg <#std.library.message.nocontainerhere#.>					
end define


define procedure <Character_Not_Here_Proc>
	setvar <custom_message; $lengthof(#std.lib.message.charnothere#)$>
		if is <%custom_message%; 0> then msg <$capfirst(#object#)$ isn't here.>
		else msg <#std.lib.message.charnothere#.>
end define


define procedure <Nothing_To_Put_In_Proc>
	setvar <custom_message; $lengthof(#std.lib.message.nothingtoputin#)$>
		if is <%custom_message%; 0> then msg <You don't appear to be holding the #object#.>
		else msg <#std.lib.message.nothingtoputin#.>
end define


define procedure <Put_It_In_Proc>
	lose <#object#>
	moveobject <#object#;#container#>
	showobject <#object#@#container#>
	setvar <custom_message; $lengthof(#std.lib.message.putincontainer#)$>
		if is <%custom_message%; 0> then msg <You put the #object# into the #container#.> 
		else msg <#std.lib.message.putincontainer#.>
end define


define procedure <override_permitted>
	setstring <override;#reset#>
	if is <$left(#quest.command#;6)$;custom> then {
		if is <#std.lib.override#;yes> then {
		setstring <std.lib.override;#reset#>
		setstring <override;yes>
		}
		else {
		setstring <override;#reset#>	
		}
	}
	else setstring <override;yes>
end define

define procedure <standard_lib_setup>
	setstring <reset;>
	setstring <std.lib.message.override;>
	setstring <std.lib.message.drop;>
	setstring <std.lib.message.notcarried;>
	setstring <std.lib.message.alreadygot;>
	setstring <std.lib.message.takefrom;>
	setstring <std.lib.message.objnotheld;>
	setstring <std.lib.message.charnothere;>
	setstring <std.lib.message.nocontainerhere;>
	setstring <std.lib.message.notacontainer;>
	setstring <std.lib.message.putincontainer;>
	setstring <std.lib.message.nothingtoputin;>
	setstring <std.lib.message.putcharinobj;>
	setstring <std.lib.message.putinself;>
end define


'==== Following code included for backward compatibility only ====

define procedure <Old_Look_Proc>
	if got <#object#> then {
	moveobject <#object#;#quest.currentroom#>
	showobject <#object#>
	exec <look at #object#;normal>
	do <Old_Contents_Proc>
	hideobject <#object#>
	}
	else {
	setstring <where_it_is;$locationof(#object#)$>
		
	if here <#where_it_is#> or got <#where_it_is#> then {
		moveobject <#object#;#quest.currentroom#>
		showobject <#object#>
		exec <look at #object#;normal>
		moveobject <#object#;#where_it_is#>
		do <Old_Contents_Proc>
		}
		else {
		exec <look at #object#;normal>
		do <Old_Contents_Proc>
		}
	}
end define


define procedure <Old_Contents_Proc>
	if is <$locationof(#object#)$;#quest.currentroom#> or got <#object#> then {
	setstring <Where_We_Were;#quest.currentroom#>
	outputoff
	goto <#object#>
		if is <$lengthof(#quest.objects#)$;0> then {
		goto <#Where_We_Were#>
		outputon
		}
		else {
		outputon
		msg <$gettag(#object#;prefix)$ #quest.formatobjects#.>
		outputoff
		goto <#Where_We_Were#>
		outputon
		}
	}
end define

define procedure <Old_Special_Take_Proc>
	do <override_permitted>  
	if is <#override#;yes> then {

	setstring <where_it_is;$locationof(#object#)$>
	if here <#where_it_is#> or got <#where_it_is#> then exec <take #object# from #where_it_is#>
	else {
		if got <#object#> then {
		setvar <custom_message; $lengthof(#std.lib.message.alreadygot#)$>
			if is <%custom_message%; 0> then msg <You already have it.>
			else msg <#std.lib.message.alreadygot#.>
		}
		else exec <take #object#;normal>
	}

	}
	else msg <Sorry, I don't understand '#quest.originalcommand#'.>
end define


define procedure <Old_Take_Back_Proc>
	do <override_permitted>  
	if is <#override#;yes> then {

	if here <#character#> or got <#character#> then {
		if is <$locationof(#object#)$;#character#> then {
		moveobject <#object#;#quest.currentroom#>
		give <#object#>
		hideobject <#object#>
		setvar <custom_message; $lengthof(#std.lib.message.takefrom#)$>
			if is <%custom_message%; 0> then {
				if is <$gettag(#character#;look)$;character> then {
				msg <You reach out and take the #object# from $capfirst(#character#)$.> }
				else msg <You take the #object# out of the #character#.> 
			}
			else msg <#std.lib.message.takefrom#.>
		}
		else {
			setvar <custom_message; $lengthof(#std.lib.message.objnotheld#)$>
			if is <%custom_message%;0> then {
				if is <$gettag(#character#;look)$;character> then {
				msg <You can't do that, $capfirst(#character#)$ doesn't have the #object#.> }
				else msg <You can't do that, the #object# isn't in the #character#.>
			}
			else msg <#std.lib.message.objnotheld#.>
		}
	}
	else {
	setvar <custom_message; $lengthof(#std.lib.message.charnothere#)$>
		if is <%custom_message%; 0> then {
			if is <$gettag(#character#;look)$;character> then {
			msg <You can't do that, $capfirst(#character#)$ isn't here.> }
			else msg <You can't do that, the #character# isn't here.>
		}
		else msg <#std.lib.message.charnothere#.>
	}

	}
	else msg <Sorry, I don't understand '#quest.originalcommand#'.>
end define
`;
libraries["q3ext.qlb"] = `!library
!deprecated
!asl-version <300>

' N.B. THIS IS RELEASE 6 FOR QUEST 3.02 OR LATER

!addto game
   command <examine in #q3ext.qlb.object#> do <q3ext.qlb.ExamProc>
   command <examine #q3ext.qlb.object#> do <q3ext.qlb.ExamProc>
   command <look in #q3ext.qlb.object#> do <q3ext.qlb.ExamProc>   
   command <look at #q3ext.qlb.object#> do <q3ext.qlb.LookProc>
   command <look #q3ext.qlb.object#> do <q3ext.qlb.LookProc>  
   command <give #q3ext.qlb.give#> do <q3ext.qlb.GiveProc>
   command <drop #q3ext.qlb.object# in #q3ext.qlb.container#> do <q3ext.qlb.PutInProc>
   command <drop #q3ext.qlb.object#> exec <drop $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>
   command <take #q3ext.qlb.object# from #q3ext.qlb.container#> do <q3ext.qlb.TakeBackProc>
   command <take #q3ext.qlc.container#'s #q3ext.qlb.object#> do <q3ext.qlb.TakeBackProc>
   command <take #q3ext.qlb.container#s #q3ext.qlb.object#> do <q3ext.qlb.TakeBackProc>
   command <take #q3ext.qlb.container#' #q3ext.qlb.object#> do <q3ext.qlb.TakeBackProc>
   command <take #q3ext.qlb.object#> do <q3ext.qlb.SpecialTakeProc>
   command <read #q3ext.qlb.object#> do <q3ext.qlb.ReadProc>
   command <wear #q3ext.qlb.object#> do <q3ext.qlb.WearProc>
   command <unwear #q3ext.qlb.object#> do <q3ext.qlb.UnWearProc>
   command <open #q3ext.qlb.object#> do <q3ext.qlb.OpenProc>
   command <close #q3ext.qlb.object#> do <q3ext.qlb.CloseProc>
!end

!addto synonyms
   of; the =
   x = examine
   put; put down = drop
   give back = give
   pick up; pick; get; take back = take
   out of; back from = from
   in to; into; inside = in
   back to = to
   don; drop on = wear
   take off; remove = unwear
   shut = close
   talk = speak
   look at in = examine
   go = go to
   to to = to
!end

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'* This is a dummy room - only needed for the force_refresh procedure to function  *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define room <q3ext.qlb.limbo>
   look <Just a dummy!>
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*    This procedure implements an alternative room description - optional usage   * 
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.DescribeRoomProc>
   set string <q3ext.qlb.indescription;$gettag(#quest.currentroom#;indescription)$>
   if is <#q3ext.qlb.indescription#;> then {
      msg <|nYou are in $gettag(#quest.currentroom#;prefix)$ |b|cl#quest.currentroom#.|cb|xb|xn>
   }
   else {
      msg <|n#q3ext.qlb.indescription#$gettag(#quest.currentroom#;prefix)$ |b|cl#quest.currentroom#.|cb|xb|xn>                
   }
   set string <q3ext.qlb.ObjList;>
   set string <q3ext.qlb.CharList;>
   for each object in <#quest.currentroom#> {
      if property <#quest.thing#;invisible> or property <#quest.thing#;hidden> then {
      }
      else {
         if property <#quest.thing#;human> then {
            set <q3ext.qlb.CharList;#q3ext.qlb.CharList# |b#quest.thing#|xb, >
         }
         else{
            set <q3ext.qlb.ObjList;#q3ext.qlb.ObjList# $objectproperty(#quest.thing#;prefix)$>
            set <q3ext.qlb.ObjList;#q3ext.qlb.ObjList# |b#quest.thing#|xb, >
         }
      }
   }
   if ($lengthof(#q3ext.qlb.CharList#)$ >0) then {
      set numeric <q3ext.qlb.LengthOf;$lengthof(#q3ext.qlb.CharList#)$ - 1>
      set <q3ext.qlb.CharList;$left(#q3ext.qlb.CharList#;%q3ext.qlb.LengthOf%)$>
      set <q3ext.qlb.CharList;$q3ext.qlb.parsed(#q3ext.qlb.CharList#)$>
         if ($instr(#q3ext.qlb.CharList#;_and_)$ >0) then { 
            msg < (#q3ext.qlb.CharList# are also here.)|xn>    
         }
         else {
            msg < (#q3ext.qlb.CharList# is also here.)|xn>        
         }
      }
      msg <|n|n$parameter(1)$|n>
      if ($lengthof(#q3ext.qlb.ObjList#)$ >0) then {     
         set <q3ext.qlb.LengthOf;$lengthof(#q3ext.qlb.ObjList#)$ - 1>
         set <q3ext.qlb.ObjList;$left(#q3ext.qlb.ObjList#;%q3ext.qlb.LengthOf%)$>      
         set <q3ext.qlb.ObjList;$q3ext.qlb.parsed(#q3ext.qlb.ObjList#)$>
         if ($instr(#q3ext.qlb.ObjList#;_and_)$ >0) then { 
            msg <There are #q3ext.qlb.ObjList# here.|n>     
         }
         else {
            msg <There is #q3ext.qlb.ObjList# here.|n>         
         }        
      }
      if not is <#quest.doorways.dirs#;> then {
         msg <You can move #quest.doorways.dirs#.>
      }
      if not is <#quest.doorways.places#;> then {
         msg <You can go to #quest.doorways.places#.>
      }
      if not is <#quest.doorways.out#;> then {
         msg <You can go out to |b#quest.doorways.out.display#|xb.>
      }     
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*  All regular 'take' commands are directed here to be tested for special needs   *
'*  The code redirects to other procedures if object is in a container, and also   *
'*  checks the player doesn't already have the item & prints message if he does.   *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.SpecialTakeProc>
   set string <q3ext.qlb.containedby;$q3ext.qlb.containedby$>  
   if (#q3ext.qlb.containedby# = nil) then {
      if got <#q3ext.qlb.object#> then {
         msg <You already have the #q3ext.qlb.object#.>           
      } 
      else {
         exec <take #q3ext.qlb.object#;normal>
      }
   }
   else {
      do <q3ext.qlb.TakeBackProc>
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*       This code is called if an object is taken that is in a container          * 
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.TakeBackProc>
   set string <q3ext.qlb.containedby;$q3ext.qlb.containedby$>
   if (#q3ext.qlb.containedby# = nil) then {
      msg <(assuming you meant to take the #q3ext.qlb.object#).>
      exec <take #q3ext.qlb.object#>
   }
   else {
      if here <#q3ext.qlb.containedby#> or got <#q3ext.qlb.containedby#> then {
         if not property <#q3ext.qlb.containedby#; closed> then {
            move <#q3ext.qlb.object#;#quest.currentroom#>
         }
      }
         outputoff
         exec <take #q3ext.qlb.object#;normal>
         outputoff
         set string <q3ext.qlb.wherenow;#quest.currentroom#>
         goto <q3ext.qlb.limbo>
         goto <#q3ext.qlb.wherenow#>
         outputon
         if got <#q3ext.qlb.object#> then {
            if property <#q3ext.qlb.containedby#;human> then {
               msg <You take the #q3ext.qlb.object# from $capfirst(#q3ext.qlb.containedby#)$.>
            }
            else {
               msg <You take the #q3ext.qlb.object# out of the #q3ext.qlb.containedby#.>
            }
         }
      else {
         exec <take #q3ext.qlb.object#;normal>
      }
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*       Code called when an object is 'given to' or 'put into' a container        *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.GiveProc>
   set numeric <q3ext.qlb.ToPos;$instr(#q3ext.qlb.give#;_to_)$>
   if (%q3ext.qlb.ToPos% <> 0) then {
      set string <q3ext.qlb.GiveObj;$left(#q3ext.qlb.give#;%q3ext.qlb.ToPos%)$>
      set string <q3ext.qlb.GiveObj;$q3ext.qlb.realname(#q3ext.qlb.Giveobj#)$>
      set <q3ext.qlb.ToPos;%q3ext.qlb.ToPos% + 4>
      set string <q3ext.qlb.GiveTo;$mid(#q3ext.qlb.give#;%q3ext.qlb.ToPos%)$>
      set string <q3ext.qlb.GiveTo;$q3ext.qlb.realname(#q3ext.qlb.GiveTo#)$>      
      if not property <#q3ext.qlb.GiveObj#; unworn> and action <#q3ext.qlb.GiveObj#;wear> then {
         msg <You'll have to take $objectproperty(#q3ext.qlb.GiveObj#;pronoun)$ off first.>
      }
      else {
         if here <#q3ext.qlb.GiveObj#> or got <#q3ext.qlb.GiveObj#> then {
            if here <#q3ext.qlb.GiveTo#> or got <#q3ext.qlb.GiveTo#> then {
               if property <#q3ext.qlb.GiveTo#;container> and not property <#q3ext.qlb.GiveTo#; closed> then {
                  if got <#q3ext.qlb.GiveTo#> then {
                     outputoff
                     exec <drop #q3ext.qlb.GiveTo#>
                     outputon
                     exec <give #q3ext.qlb.GiveObj# to $q3ext.qlb.objname(#q3ext.qlb.GiveTo#)$; normal>
                     outputoff
                     exec <take #q3ext.qlb.GiveTo#>
                     do <force_refresh>
                     outputon
                  }
                  else {
                     exec <give #q3ext.qlb.GiveObj# to $q3ext.qlb.objname(#q3ext.qlb.GiveTo#)$; normal>
                     do <force_refresh>                  
                  }
               }
               else {
                  if property <#q3ext.qlb.GiveObj#;human > then {
                     msg <You try, but $capfirst(#q3ext.qlb.GiveObj#)$ isn't interested.>
                  }
                  else {
                     msg <Hard as you try, you find you cannot do that|xn>
                     if property <#q3ext.qlb.GiveTo#; closed> then {
                        msg < while the #q3ext.qlb.GiveTo# is closed.>
                     }
                     else {
                        msg <.|n>
                     }
                  }
               }
            }
            else {
               if property <#q3ext.qlb.GiveTo#;human> then {
                  msg <$capfirst(#q3ext.qlb.GiveObj#)$ isn't here.>
               }
               else {
                  msg <The #q3ext.qlb.GiveTo# isn't available.>
               }          
            }
         }
         else {
            if property <#q3ext.qlb.GiveObj#;human > then {
               msg <$capfirst(#q3ext.qlb.GiveObj#)$ isn't here.>
            }
            else {
               msg <The #q3ext.qlb.GiveObj# isn't available.>
            }
         }
      }
   }
   else {
'***  Deals with 'Give character the object' style. 
      for <q3ext.qlb.SpaceTrue;1;$lengthof(#q3ext.qlb.give#)$> do <q3ext.qlb.SpaceTest>
      set string <q3ext.qlb.GiveTo;$left(#q3ext.qlb.give#;%q3ext.qlb.FoundSpace%)$>
      set string <q3ext.qlb.GiveObj;$mid(#q3ext.qlb.give#;%q3ext.qlb.FoundSpace%)$>
      exec <give #q3ext.qlb.GiveObj# to #q3ext.qlb.GiveTo#>
    }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*   A utility procedure, called to find the positions of spaces in player input   *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.SpaceTest>
   set string <q3ext.qlb.Space;$mid(c o;2;1)$>
   set string <q3ext.qlb.TestPart;$mid(#q3ext.qlb.give#;%q3ext.qlb.SpaceTrue%;1)$>
   if is <#q3ext.qlb.space#;#q3ext.qlb.TestPart#> then set numeric <q3ext.qlb.FoundSpace;%q3ext.qlb.SpaceTrue%>
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*      This procedure deals with examining containers & objects in containers.    *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.ExamProc>
   set <q3ext.qlb.object; $q3ext.qlb.objname(#q3ext.qlb.object#)$>
   set <q3ext.qlb.object; $q3ext.qlb.wornfix()$>
   set string <q3ext.qlb.containedby;$q3ext.qlb.containedby$>
   set string <q3ext.qlb.whereat;#quest.currentroom#>
   if (#q3ext.qlb.containedby# = nil) then {
      if action <#q3ext.qlb.object#; open> then {
         if property <#q3ext.qlb.object#; closed> then {
            if ($objectproperty(#q3ext.qlb.object#; closedexam)$ = null) then {
               msg <Nothing out of the ordinary.>               
            }
            else {
               msg <$objectproperty(#q3ext.qlb.object#; closedexam)$.>
            }
         }
         else {
            if ($objectproperty(#q3ext.qlb.object#; openexam)$ = null) then {
               msg <Nothing out of the ordinary.>               
            }
            else {
               msg <$objectproperty(#q3ext.qlb.object#; openexam)$.>
            }
         }
      }
      else {
         exec <examine #q3ext.qlb.object#;normal>
      }
      if property <#q3ext.qlb.object#;container> then {
         if here <#q3ext.qlb.object#> or got <#q3ext.qlb.object#> then {
            doaction <#q3ext.qlb.object#;contents>
         }
      }
   }
   else {
      if here <#q3ext.qlb.containedby#> or got <#q3ext.qlb.containedby#> then {
         if not property <#q3ext.qlb.containedby#; closed> then {
            outputoff
            goto <#q3ext.qlb.containedby#_inventory>
            outputon
               if action <#q3ext.qlb.object#; open> then {
                  if property <#q3ext.qlb.object#; closed> then {
                     if ($objectproperty(#q3ext.qlb.object#; closedexam)$ = null) then {
                        msg <Nothing out of the ordinary.>               
                     }
                     else {
                        msg <$objectproperty(#q3ext.qlb.object#; closedexam)$.>
                     }
                  }
                  else {
                     if ($objectproperty(#q3ext.qlb.object#; openexam)$ = null) then {
                        msg <Nothing out of the ordinary.>               
                     }
                     else {
                        msg <$objectproperty(#q3ext.qlb.object#; openexam)$.>
                     }
                  }
               }
               else {
                  exec <examine #q3ext.qlb.object#;normal>
               }
            outputoff
            goto <#q3ext.qlb.whereat#>
            outputon
         }
         else {
            exec <examine #q3ext.qlb.object#;normal>
         }
      }
      else {
         exec <examine #q3ext.qlb.object#;normal>         
      }
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*      This procedure deals with 'looking at' objects held in containers.         *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.LookProc>
   set <q3ext.qlb.object; $q3ext.qlb.realname(#q3ext.qlb.object#)$>
   set <q3ext.qlb.object; $q3ext.qlb.wornfix()$>
   set string <q3ext.qlb.containedby;$q3ext.qlb.containedby$>
   set string <q3ext.qlb.whereat;#quest.currentroom#>
   if (#q3ext.qlb.containedby# = nil) then {
      exec <look at $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>
   }
   else {
      if here <#q3ext.qlb.containedby#> or got <#q3ext.qlb.containedby#> then {
         if not property <#q3ext.qlb.containedby#; closed> then {
            outputoff
            goto <#q3ext.qlb.containedby#_inventory>
            outputon
            exec <look at $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>   
            outputoff
            goto <#q3ext.qlb.whereat#>
            outputon
         }
         else {
            exec <look at $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>
         }
      }
      else {
         exec <look at $q3ext.qlb.objname(#q3ext.qlb.object#)$;normal>
      }
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*   Redirects 'put into' so that it is processed by the 'give to' routine, these  *
'*   are functionally identical so no need to have duplicate code                  *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.PutInProc>
   exec <give #q3ext.qlb.object# to #q3ext.qlb.container#>
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*  A utility procedure, returns the position of the last comma in a passed string *
'*  Used to replace the last comma with 'and' when parsing output strings          *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.LastCommaProc>
   set string <q3ext.qlb.TestPart;$mid(#q3ext.qlb.Param#;%q3ext.qlb.LastComma%;1)$>
   if is <#q3ext.qlb.TestPart#;,> then set <q3ext.qlb.FoundComma;%q3ext.qlb.LastComma%>
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*  The 'read' command procedure.                                                  *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.ReadProc>
   set <q3ext.qlb.object;$q3ext.qlb.realname(#q3ext.qlb.object#)$>
   if property <#q3ext.qlb.object#;readable> then {
      doaction <#q3ext.qlb.object#;read>
      if not is <$objectproperty(#q3ext.qlb.object#;readaction)$; nil> then {
         doaction <#q3ext.qlb.object#;readaction>
      }
   }
   else {
      msg <There's nothing to read!>
   }
end define


'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*  The 'wear' command procedure.                                                  *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.WearProc>
   set <q3ext.qlb.object;$q3ext.qlb.realname(#q3ext.qlb.object#)$>
   msg <#q3ext.qlb.object#.....>
   if got <#q3ext.qlb.object#> then {
      if action <#q3ext.qlb.object#;wear> then {
         if ($objectproperty(#q3ext.qlb.object#;sex)$ = %q3ext.qlb.playersex%) then {
            do <q3ext.qlb.CheckWearProc>
         }
         else {
            msg <On second thoughts you decide that the |xn>
            msg <#q3ext.qlb.object# |xn>
            if (%q3ext.qlb.playersex% = 1) then {
               msg <won't suit a man like you at all.>
            }
            else {
               msg <won't suit a woman like you at all.>
            }
         }
         }
         else {
         msg <You cannot wear the #q3ext.qlb.object#.>
         }
   }
   else {
      msg <You are not carrying the #q3ext.qlb.object#.>
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*  The 'CheckWear' procedure - this checks the sense of wear commands             *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.CheckWearProc>
   set numeric <q3ext.qlb.wearable;0>
   if ($objectproperty(#q3ext.qlb.object#;topcover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;topcover)$ <= %q3ext.qlb.topcovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>
   }
   if ($objectproperty(#q3ext.qlb.object#;headcover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;headcover)$ <= %q3ext.qlb.headcovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>
   }
   if ($objectproperty(#q3ext.qlb.object#;feetcover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;feetcover)$ <= %q3ext.qlb.feetcovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>
   }
   if ($objectproperty(#q3ext.qlb.object#;handscover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;handscover)$ <= %q3ext.qlb.handscovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>
   }
' - disallow for coats (don't affect ability to put on lower torso clothes)
   set <q3ext.qlb.tempcovered;%q3ext.qlb.botcovered%>
   if (%q3ext.qlb.tempcovered% > 63) and __
   ($objectproperty(#q3ext.qlb.object#;botcover)$ < 33) then {
      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% - 64>
   }
' - disallow for skirts and dresses (like coats!)
   if (%q3ext.qlb.tempcovered% > 31) and __
   ($objectproperty(#q3ext.qlb.object#;botcover)$ < 16) and __
   ($objectproperty(#q3ext.qlb.object#;botcover)$ <> 4) then {
      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% - 32>
   }
' - disallow wearing of skirts/dresses and trousers simultaneously
   if (%q3ext.qlb.tempcovered% > 15) then {
      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% + 16> 
   }
   if ($objectproperty(#q3ext.qlb.object#;botcover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;botcover)$ <= %q3ext.qlb.tempcovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>
   }
   if (%q3ext.qlb.wearable% =0) then {
      doaction <#q3ext.qlb.object#;wear>
      property <#q3ext.qlb.object#; not unworn>
      set <q3ext.qlb.topcovered;%q3ext.qlb.topcovered% + $objectproperty(#q3ext.qlb.object#;topcover)$>
      set <q3ext.qlb.headcovered;%q3ext.qlb.headcovered% + $objectproperty(#q3ext.qlb.object#;headcover)$>
      set <q3ext.qlb.handscovered;%q3ext.qlb.handscovered% + $objectproperty(#q3ext.qlb.object#;handscover)$>
      set <q3ext.qlb.feetcovered;%q3ext.qlb.feetcovered% + $objectproperty(#q3ext.qlb.object#;feetcover)$>
      set <q3ext.qlb.botcovered;%q3ext.qlb.botcovered% + $objectproperty(#q3ext.qlb.object#;botcover)$>
   }
   else {
      msg <Given what you are already wearing - that makes no sense at all.>
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*  The 'unwear' command procedure.                                                *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.UnWearProc>
   set <q3ext.qlb.object;$q3ext.qlb.realname(#q3ext.qlb.object#)$>
   if not property <#q3ext.qlb.object#; unworn> then {
      if action <#q3ext.qlb.object#;unwear> then {
         do <q3ext.qlb.CheckUnwearProc>
      }
      else {
      msg <You cannot do that.>
      }
   }
   else {
      msg <You aren't wearing the #q3ext.qlb.object#.>
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*  The 'CheckUnwear' procedure.                                                   *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.CheckUnwearProc>
   set numeric <q3ext.qlb.wearable;0>
   set <q3ext.qlb.tempcovered; %q3ext.qlb.topcovered% /2>
   if ($objectproperty(#q3ext.qlb.object#;topcover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;topcover)$ <= %q3ext.qlb.tempcovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 1>
   }
   set <q3ext.qlb.tempcovered; %q3ext.qlb.headcovered% /2>
   if ($objectproperty(#q3ext.qlb.object#;headcover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;headcover)$ <= %q3ext.qlb.tempcovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 2>
   }
   set <q3ext.qlb.tempcovered; %q3ext.qlb.feetcovered% /2>
   if ($objectproperty(#q3ext.qlb.object#;feetcover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;feetcover)$ <= %q3ext.qlb.tempcovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 4>
   }
   set <q3ext.qlb.tempcovered; %q3ext.qlb.handscovered% /2>   
      if ($objectproperty(#q3ext.qlb.object#;handscover)$ <> 0) and __
      ($objectproperty(#q3ext.qlb.object#;handscover)$ <= %q3ext.qlb.tempcovered%) then {
         set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 8>
   }
' - disallow for coats (don't affect ability to take off lower torso clothes)
   set <q3ext.qlb.tempcovered;%q3ext.qlb.botcovered%>
   if (%q3ext.qlb.tempcovered% > 63) then {
      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% - 64>
   }
' - disallow for skirts and dresses (like coats!)
   if (%q3ext.qlb.tempcovered% > 31) and __
   ($objectproperty(#q3ext.qlb.object#;botcover)$ <> 4) then {
      set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% - 32>
   }
   set <q3ext.qlb.tempcovered;%q3ext.qlb.tempcovered% /2>
   if ($objectproperty(#q3ext.qlb.object#;botcover)$ <> 0) and __
   ($objectproperty(#q3ext.qlb.object#;botcover)$ <= %q3ext.qlb.tempcovered%) then {
      set <q3ext.qlb.wearable;%q3ext.qlb.wearable% + 16>
   }
   if (%q3ext.qlb.wearable% =0) then {
      doaction <#q3ext.qlb.object#;unwear>
      property <#q3ext.qlb.object#; unworn>
      set <q3ext.qlb.topcovered;%q3ext.qlb.topcovered% - $objectproperty(#q3ext.qlb.object#;topcover)$>
      set <q3ext.qlb.headcovered;%q3ext.qlb.headcovered% - $objectproperty(#q3ext.qlb.object#;headcover)$>
      set <q3ext.qlb.handscovered;%q3ext.qlb.handscovered% - $objectproperty(#q3ext.qlb.object#;handscover)$>
      set <q3ext.qlb.feetcovered;%q3ext.qlb.feetcovered% - $objectproperty(#q3ext.qlb.object#;feetcover)$>
      set <q3ext.qlb.botcovered;%q3ext.qlb.botcovered% - $objectproperty(#q3ext.qlb.object#;botcover)$>
   }
   else {
      msg <Given what you are wearing, that isn't possible.>
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'* Open and close commands for 'openable' type                                     *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.OpenProc>
   set <q3ext.qlb.object; $q3ext.qlb.realname(#q3ext.qlb.object#)$>
   if action <#q3ext.qlb.object#; open> then {
      doaction <#q3ext.qlb.object#; open> 
   }
   else msg <You can't do that.>
end define

define procedure <q3ext.qlb.CloseProc>
   set <q3ext.qlb.object; $q3ext.qlb.realname(#q3ext.qlb.object#)$>
   if action <#q3ext.qlb.object#; close> then {
      doaction <#q3ext.qlb.object#; close>
   }
   else msg <You can't do that.>
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'* This forces the inventory window to be refreshed - Quest doesn't do it itself!  *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <force_refresh>
      outputoff
      set string <q3ext.qlb.wherenow;#quest.currentroom#>
      goto <q3ext.qlb.limbo>
      goto <#q3ext.qlb.wherenow#>      
      outputon
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'* This is called to initialise the library                                        *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define procedure <q3ext.qlb.setup>
'  These are defaults - making the player male and naked! - it will probably be 
'  required to set these variables differently in the startscript AFTER calling
'  this setup routine
'  'q3ext.qlb.playersex' should be set to 1 for a male player, 2 for a female.
   set numeric <q3ext.qlb.headcovered;0>
   set numeric <q3ext.qlb.handscovered;0>
   set numeric <q3ext.qlb.feetcovered;0>
   set numeric <q3ext.qlb.topcovered;0>
   set numeric <q3ext.qlb.botcovered;0>
   set numeric <q3ext.qlb.tempcovered;0>
   set numeric <q3ext.qlb.playersex;1>
   set numeric <q3ext.qlb.version;6>
   if (%q3ext.qlb.version% < $parameter(1)$) then {
      msg <WARNING!|n|nThis game requires Version $parameter(1)$ (or later) of _
      the Q3EXT.QLB library, you appear to have Version %q3ext.qlb.version%. _
      |n|nPlease obtain the latest release of the library before trying to run this _
      game.|n|n|w>
   }
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*                         FUNCTION DEFINITIONS FOLLOW                             *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'* Returns passed string that was comma separated list 'parsed' to read naturally. *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define function <q3ext.qlb.parsed>
   set numeric <q3ext.qlb.FoundComma;0>
   set string <q3ext.qlb.Param;$parameter(1)$>
   for <q3ext.qlb.LastComma;1;$lengthof(#q3ext.qlb.Param#)$> {
   do <q3ext.qlb.LastCommaProc>
   }
      if not is <%q3ext.qlb.FoundComma%;0> then { 
      set numeric <q3ext.qlb.remaining;%q3ext.qlb.LastComma%-%q3ext.qlb.FoundComma%>
      set <q3ext.qlb.remaining;%q3ext.qlb.remaining%-1>
      set <q3ext.qlb.FoundComma;%q3ext.qlb.FoundComma%-1>
      set string <q3ext.qlb.LeftPart;$left(#q3ext.qlb.Param#;__
      %q3ext.qlb.FoundComma%)$>
      set string <q3ext.qlb.RightPart;$right(#q3ext.qlb.Param#;__
      %q3ext.qlb.remaining%)$>
      set string <q3ext.qlb.parsed;#q3ext.qlb.LeftPart# and #q3ext.qlb.RightPart#>
      }
      if is <%q3ext.qlb.FoundComma%;0> then {
      set string <q3ext.qlb.parsed;#q3ext.qlb.Param#>
      }
      return <#q3ext.qlb.parsed#>
end define

'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'* The following function returns the name of the object in who's container the    *
'* looked at object is held, or 'nil' if the object isn't in a container.          *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define function <q3ext.qlb.containedby>
   set <q3ext.qlb.object; $q3ext.qlb.realname(#q3ext.qlb.object#)$>
   set string <q3ext.qlb.containedby;$locationof(#q3ext.qlb.object#)$>
   set numeric <q3ext.qlb.LengthOf;$lengthof(#q3ext.qlb.containedby#)$>
   if (%q3ext.qlb.LengthOf% < 11) then {
      set <q3ext.qlb.containedby;nil>
   }
   if (%q3ext.qlb.LengthOf% > 11) then {
      set <q3ext.qlb.containedby;$right(#q3ext.qlb.containedby#;10)$>
   }
   if (#q3ext.qlb.containedby# <> _inventory) then {
      set <q3ext.qlb.containedby;nil>
   }
   if (#q3ext.qlb.containedby# = _inventory) then {
      set <q3ext.qlb.containedby;$locationof(#q3ext.qlb.object#)$>
      set <q3ext.qlb.LengthOf;%q3ext.qlb.LengthOf%-10>
      set <q3ext.qlb.containedby;$left(#q3ext.qlb.containedby#;%q3ext.qlb.LengthOf%)$>    
   }
   return <#q3ext.qlb.containedby#>
end define

define function <q3ext.qlb.wornfix>
   set string <q3ext.qlb.wornfix;#q3ext.qlb.object#>
   if action <#q3ext.qlb.object#; wear> and not property <#q3ext.qlb.object#; unworn> then {
      set <q3ext.qlb.wornfix;#q3ext.qlb.object# [worn]>
   }
   return <#q3ext.qlb.wornfix#>
end define

define function <q3ext.qlb.objname>
      set string <q3ext.qlb.objname;$parameter(1)$>
      set <q3ext.qlb.objname;$q3ext.qlb.realname(#q3ext.qlb.objname#)$>      
      set <q3ext.qlb.objname;$displayname(#q3ext.qlb.objname#)$>
      return <#q3ext.qlb.objname#>
end define

define function <q3ext.qlb.realname>
      set numeric <q3ext.qlb.found;0>
      set string <q3ext.qlb.objname;$parameter(1)$>
      set string <q3ext.qlb.lobjname;$lcase(#q3ext.qlb.objname#)$>

      for each object in <#quest.currentroom#> {
         set string <q3ext.qlb.display;$displayname(#quest.thing#)$>
         set string <q3ext.qlb.display;$lcase(#q3ext.qlb.display#)$>
         set string <q3ext.qlb.realname;#quest.thing#>
         set string <q3ext.qlb.realname;$lcase(#q3ext.qlb.realname#)$>
         if ( $instr(#q3ext.qlb.display#;#q3ext.qlb.lobjname#)$ > 0 ) or _
            ( $instr(#q3ext.qlb.lobjname#;#q3ext.qlb.realname#)$ > 0 ) then {  
             set <q3ext.qlb.objname;#quest.thing#>
             set <q3ext.qlb.found;1>
         }    
      }
      if (%q3ext.qlb.found% = 0) then {
         for each object in inventory {
         set string <q3ext.qlb.display;$displayname(#quest.thing#)$>
         set string <q3ext.qlb.display;$lcase(#q3ext.qlb.display#)$>
         set string <q3ext.qlb.realname;#quest.thing#>
         set string <q3ext.qlb.realname;$lcase(#q3ext.qlb.realname#)$>
            if ( $instr(#q3ext.qlb.display#;#q3ext.qlb.lobjname#)$ > 0 ) or _
               ( $instr(#q3ext.qlb.lobjname#;#q3ext.qlb.realname#)$ > 0 ) then {  
               set <q3ext.qlb.objname;#quest.thing#>      
               set <q3ext.qlb.found;1>
            }    
         }     
      }
       if (%q3ext.qlb.found% = 0) then {
         for each object in game {
         set string <q3ext.qlb.display;$displayname(#quest.thing#)$>
         set string <q3ext.qlb.display;$lcase(#q3ext.qlb.display#)$>
         set string <q3ext.qlb.realname;#quest.thing#>
         set string <q3ext.qlb.realname;$lcase(#q3ext.qlb.realname#)$>
            if ( $instr(#q3ext.qlb.display#;#q3ext.qlb.lobjname#)$ > 0 ) or _
               ( $instr(#q3ext.qlb.lobjname#;#q3ext.qlb.realname#)$ > 0 ) then {  
                set <q3ext.qlb.objname;#quest.thing#>      
            }    
         }     
      }     
   return <#q3ext.qlb.objname#>
end define


'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
'*                           TYPE DEFINITIONS FOLLOW                               *
'* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define type <human>
   human
   type <container>
   displaytype=Person
   action <take> {
      set string <q3ext.qlb.object; $mid(#quest.command#;6)$>
      msg <You can't take $capfirst(#q3ext.qlb.object#)$!>
   }
   action <speak> {
      set string <q3ext.qlb.object; $mid(#quest.command#;10)$>
      msg <$capfirst(#q3ext.qlb.object#)$ doesn't reply to you.>
   }
      action <give anything> {
         if property <#q3ext.qlb.GiveTo#;container> then {
         msg <|n$capfirst(#q3ext.qlb.GiveTo#)$ accepts the #q3ext.qlb.GiveObj#.>
         move <#q3ext.qlb.GiveObj#;#q3ext.qlb.GiveTo#_inventory>
         }
         else {
         msg <$capfirst(#q3ext.qlb.GiveTo#)$ doesn't seem to want the #q3ext.qlb.GiveObj#.>
         }
   }
end define

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define type <container>
   container
   action <contents> {
      if not property <#q3ext.qlb.object#; closed> then {
         outputoff
         goto <#q3ext.qlb.object#_inventory>
         set string <q3ext.qlb.whatsheld;$gettag(#q3ext.qlb.object#_inventory;prefix)$$q3ext.qlb.parsed(#quest.formatobjects#)$.>
         if ($lengthof(#quest.objects#)$ = 0) then set <q3ext.qlb.whatsheld;>
         outputon
         msg <#q3ext.qlb.whatsheld#>         
         outputoff
         goto <#q3ext.qlb.whereat#>
         outputon
      }
   }

   action <give anything> {
      msg <You put the #q3ext.qlb.GiveObj# in the #q3ext.qlb.GiveTo#.>
      move <#q3ext.qlb.GiveObj#; #q3ext.qlb.GiveTo#_inventory>
   }
end define

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define type <object>
      prefix = a
      take = nil
      action <take> {
         if ($objectproperty(#q3ext.qlb.object#;take)$=nil) then {
            msg <You pick up the #q3ext.qlb.object#.>
         }
         else {
         msg <$objectproperty(#q3ext.qlb.object#;take)$.>      
         }     
      move <#q3ext.qlb.object#;inventory>
      do <force_refresh>
      }
end define

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define type <scenery>
   invisible
   prefix = a
   pronoun = that
   notake = nil
   action <take> {
      if ($objectproperty(#q3ext.qlb.object#;notake)$=nil) then {
      msg <Taking $objectproperty(#q3ext.qlb.object#;pronoun)$ would serve no useful purpose.>
      }
      else {
      msg <$objectproperty(#q3ext.qlb.object#;notake)$.>    
      }
   }
end define

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define type <readable>
   readable
   readmessage = You start to read but it is so incredibly dull you decide not to bother.
   readaction = nil
   action <read> {
   set string <q3ext.qlb.objname;$thisobject$>
   msg <$objectproperty(#q3ext.qlb.objname#;readmessage)$>
   }
end define

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
' *  The openable type                                                              *
' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define type <openable>
   closed
   properties <closeddesc = null>
   properties <opendesc = null>
   properties <closingdesc = null>
   properties <openingdesc = null>
   properties <closedexam = null>
   properties <openexam = null>  
   
   prefix = a
     
   action <open> {
   set string <q3ext.qlb.opened; $thisobject$>   
   if property <$thisobject$; closed> then {
      property <$thisobject$; not closed>
      if ($objectproperty(#q3ext.qlb.opened#; openingdesc)$ = null) then {
         msg <You open the #q3ext.qlb.object#.>
      }
      else {
         msg <$objectproperty(#q3ext.qlb.opened#; openingdesc)$>
      }
   }
   else msg <The #q3ext.qlb.object# is already open.>
   }
    
   action <close> {
   set string <q3ext.qlb.closed; $thisobject$>    
   if not property <$thisobject$;closed> then {
      property <$thisobject$; closed>
      if ($objectproperty(#q3ext.qlb.closed#; closingdesc)$ = null) then {
         msg <You close the #q3ext.qlb.object#.>
      }
      else {
         msg <$objectproperty(#q3ext.qlb.closed#; closingdesc)$.>      
      }
   }
   else msg <The #q3ext.qlb.object# is already closed.>
   }
    
   action <look> {
   set string <q3ext.qlb.lookedat; $thisobject$>
   if property <$thisobject$; closed> then {
      if ($objectproperty(#q3ext.qlb.lookedat#; closeddesc)$ = null) then {
         set string <q3ext.qlb.lookmessage ;The #q3ext.qlb.lookedat# is closed>
      }
      else {
         set string <q3ext.qlb.lookmessage ;$objectproperty(#q3ext.qlb.lookedat#; closeddesc)$.>
      }
   }
   else {
      if ($objectproperty(#q3ext.qlb.lookedat#; opendesc)$ = null) then {
         set string <q3ext.qlb.lookmessage ;The #q3ext.qlb.lookedat# is open>
      }
      else {
         set string <q3ext.qlb.lookmessage ;$objectproperty(#q3ext.qlb.lookedat#; opendesc)$.>
      }
   }
   msg <#q3ext.qlb.lookmessage#.>
   }
   
end define

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
' * This clothing type is not used directly but inherited by specific clothes.      *
' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 
define type <clothing>
   type <object>
   unworn
   headcover = 0
   handscover = 0
   feetcover = 0
   topcover = 0
   botcover = 0
   sex = 1
   pronoun = it
   wearmessage = You put it on.
   unwearmessage = You take it off.
   properties <alias=$thisobject$>
      action <wear> {
      set string <q3ext.qlb.objname;$thisobject$>
      msg <$objectproperty(#q3ext.qlb.objname#;wearmessage)$>
      property <#q3ext.qlb.objname#;alias=#q3ext.qlb.objname# [worn]>
      move <#q3ext.qlb.objname#;inventory>
      do <force_refresh>
      }
      action <unwear> {
      set string <q3ext.qlb.objname;$thisobject$>
      msg <$objectproperty(#q3ext.qlb.objname#;unwearmessage)$>
      property <#q3ext.qlb.objname#;alias=$thisobject$>
      do <force_refresh>
      }   
end define  

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
' * This lib defines 15 specific clothing objects, and most of these can be used    *
' * to good effect for other garments - the defined objects are listed together     *
' * with other garments that work similarly for game purposes.                      *
' *                                                                                 *
' * DEFINED GARMENT : ALSO USABLE FOR THESE GARMENTS                                *
' * hat             : any headwear                                                  *
' * gloves          : any handwear                                                  *
' * shoes           : boots, outer footwear generally                               *
' * socks           : stockings                                                     *
' * tights          : pantie hose                                                   *
' * undies          : panties, briefs - lower portion underwear generally           *
' * teddy           : uhm.. any underthing that covers like a teddy!                *
' * trousers        : jeans, shorts (not the underwear variety)                     *
' * dress           : coverall                                                      *
' * skirt           : kilt maybe?                                                   *
' * vest            : bra, other 'top only' undergarment                            *
' * shirt           : blouse, T-Shirt etc.                                          *
' * sweater         : pullover, sweatshirt - '2nd layer' top garment                *
' * jacket          : fleece, parka, anorak, short coat of whatever type            *
' * coat            : any long length outermost garment like a coat                 *
' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define type <shoes>
      type <clothing>
      feetcover = 4
      wearmessage = You put them on.
      unwearmessage = You take them off.
      pronoun = them
   properties <prefix=a pair of>
end define

define type <socks>
      type <clothing>
      feetcover = 2
      wearmessage = You put them on.
      unwearmessage = You take them off.
      pronoun = them
   properties <prefix=a pair of>
end define

define type <tights>
      type <clothing>
      feetcover = 2
      botcover = 8
      pronoun = them
      wearmessage = You put them on.
      unwearmessage = You take them off.
   properties <prefix=a pair of>
end define

define type <hat>
      type <clothing>
      headcover = 2
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

define type <gloves>
      type <clothing>
      handscover = 2
      wearmessage = You put them on.
      unwearmessage = You take them off.
      pronoun = them
 properties <prefix=a pair of>
end define

define type <vest>
      type <clothing>
      topcover = 2
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

define type <shirt>
      type <clothing>
      topcover = 8
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

define type <teddy>
      type <clothing>
      topcover = 4
      botcover = 4
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

define type <undies>
      type <clothing>
      botcover = 2
      wearmessage = You put them on.
      unwearmessage = You take them off.
      pronoun = them
   properties <prefix=a pair of>
end define

define type <dress>
      type <clothing>
      topcover = 8
      botcover = 32
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

define type <skirt>
      type <clothing>
      botcover = 32
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

define type <trousers>
      type <clothing>
      botcover = 16
      wearmessage = You put them on.
      unwearmessage = You take them off.
      pronoun = them
   properties <prefix=a pair of>
end define

define type <sweater>
      type <clothing>
      topcover = 16
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

define type <jacket>
      type <clothing>
      topcover = 32
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

define type <coat>
      type <clothing>
      topcover = 64
      botcover = 64
      wearmessage = You put it on.
      unwearmessage = You take it off.
   properties <prefix=a>
end define

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *`;
libraries["typelib.qlb"] = `!library
!deprecated
!asl-version <311>
!name <MaDbRiT's Type Library>
!version <1.009 06-Sep-2003>
!author <MaDbRiT (Al Bampton)>
!help <typelib.pdf>
! This library adds a whole set of types, with related actions and properties to Quest.
! It is easy to use from Q.D.K. but be sure to read the manual before you start. 


'=====================================================
'== This is MaDbRiT's QUEST Types Library @06-09-03 ==
'== It will only work with Quest 3.11 build 137 or  == 
'== later This version is specifically designed to  ==
'== be easily usable from Q.D.K.                    ==
'=====================================================
'==        !!!! PLEASE READ THE MANUAL !!!!         ==
'==        !!!! NEVER CHANGE THIS FILE !!!!         ==
'=====================================================

'======================================
'== Add these lines to the gameblock ==
'======================================

!addto game
   command <look at #TLSdObj#; look #TLSdObj#; l #TLSdObj#> do <TLPlook>
   command <examine #TLSdObj#; inspect #TLSdObj#; x #TLSdObj#> do <TLPexamine>
   command <open #TLSdObj#> do <TLPopen>
   command <close #TLSdObj#; shut #TLSdObj#> do <TLPclose>   
   command <take #TLSdObj# from #TLSiObj#> do <TLPtake>
   command <take #TLSdObj# off> do <TLPunWear>
   command <take #TLSdObj#> do <TLPtake>
   command <drop #TLSdObj# into #TLSiObj#> do <TLPputIn>
   command <drop #TLSdObj# on> do <TLPwear>   
   command <drop #TLSdObj# down; drop down #TLSdObj#; drop #TLSdObj#> do <TLPdrop>   
   command <give #TLSdObj# to #TLSiObj#;give #TLSiObj# the #TLSdObj#> do <TLPgive>
   command <read #TLSdObj#> do <TLPread>   
   command <wear #TLSdObj#> do <TLPwear>
   command <unwear #TLSdObj#> do <TLPunWear>
   description {
      do <TLProomDescription>
   }
   startscript do <TLPstartup>
'   nodebug
!end

'===========================================
'== Add some synonyms used in the library ==
'===========================================

!addto synonyms
   back; back from; out of =from
   inside; in to; in =into
   look into = examine   
   put =drop
   don; drop on = wear
   take off; remove = unwear
   shut = close
   talk = speak
!end

'==================================================
'== A couple of dummy rooms, used by the library ==
'==================================================

define room <TLRcontainers>

end define

define room <TLRcontents>

end define

'================
'== PROCEDURES ==
'================

'===========================================================
'== Configure the library for use - called in startscript ==
'===========================================================

define procedure <TLPstartup>

   set string <TLSplayerRoom;>
   set string <TLScontentList;>
   set string <TLStemp;>
   set string <TLSrealName;>
   set string <TLSlist;>
   set string <TLStempName;>
   set string <TLMnotOpenable;Try as you might, you find that __
              is just not possible.>
   set string <TLMnotClosable;Try as you might, you find that __
              is just not possible.>
   set string <TLMtaken;Taken.>
   set string <TLMalreadyTaken;You have already got>
   set string <TLMdontHaveDObj;You don't seem to have that.>
   set string <TLMnoNeed;You realise there is no need __
               to do that and change your mind.>
   set string <TLSthisObj;>
   set string <TLSobjAlias;>
   set string <TLSindescription;>
   set string <TLSalias;>
   set string <TLSobjList;>
   set string <TLScharList;>
   set numeric <TLNcomma;0>
   set numeric <TLNcount;0>
   set numeric <TLNlength;0>
   set numeric <TLNsizeLimit;0>
   set numeric <TLNweightLimit;0>
   set numeric <TLNheadCovered;0>
   set numeric <TLNhandsCovered;0>
   set numeric <TLNfeetCovered;0>
   set numeric <TLNtopCovered;0>
   set numeric <TLNbotCovered;0>
   set numeric <TLNtempCovered;0>
   set numeric <TLNwearable;0>
   set numeric <TLNlengthOf;0>
'***
' TLNplayerSex' should be set to 1 for a male player, 2 for a female.
'***
   set numeric <TLNplayerSex;1>

   for each object in game {
      if type <#quest.thing#;TLTcontainable> then {
      if (#(quest.thing):isIn#<>nil) then {
         move <#quest.thing#;TLRcontainers>
        }
    }
    if type <#quest.thing#;TLTclothing> then {
    property <#quest.thing#;displayname=$displayname(#quest.thing#)$>
      if property <#quest.thing#;worn> then {
          move <#quest.thing#;inventory>
         property <#quest.thing#;alias=#(quest.thing):displayname# [worn]>
         set <TLNtopCovered;%TLNtopCovered% + #(quest.thing):topcover#>
         set <TLNheadCovered;%TLNheadCovered% + #(quest.thing):headcover#>
         set <TLNhandsCovered;%TLNhandsCovered% + #(quest.thing):handscover#>
         set <TLNfeetCovered;%TLNfeetCovered% + #(quest.thing):feetcover#>
            set <TLNbotCovered;%TLNbotCovered% + #(quest.thing):botcover#>
          }
   }
   }
   
end define

'==================================
'== Alternative room description ==
'==================================

define procedure <TLProomDescription>
   set <TLSindescription;$gettag(#quest.currentroom#;indescription)$>
   set <TLSalias;$gettag(#quest.currentroom#;alias)$>
   if is <#TLSindescription#;> then {
     if is <#TLSalias#;> then {
         msg <You are in $gettag(#quest.currentroom#;prefix)$ |b|cl#quest.currentroom#.|cb|xb|xn>
     }
     else {
         msg <You are in $gettag(#quest.currentroom#;prefix)$ |b|cl#TLSalias#.|cb|xb|xn>
     }
   }
   else {
     if is <#TLSalias#;> then {
         msg <#TLSindescription# $gettag(#quest.currentroom#;prefix)$ |b|cl#quest.currentroom#.|cb|xb|xn>
     }
     else {
         msg <#TLSindescription# $gettag(#quest.currentroom#;prefix)$ |b|cl#TLSalias#.|cb|xb|xn>
     }
   }
   msg <|n#quest.lookdesc#|n>
   set <TLSobjList;>
   set <TLScharList;>
   for each object in <#quest.currentroom#> {
      set <TLSthisObj;#quest.thing#>
      if property <#quest.thing#;invisible> or property <#quest.thing#;hidden> then {
      }
      else {
         if type <#quest.thing#;TLTactor> then {
			if property <#quest.thing#;named> then {
            	set <TLScharList;#TLScharList# |b$capfirst(#@quest.thing#)$|xb, >
			}
			else set <TLScharList;#TLScharList# #(quest.thing):prefix# |b#@quest.thing#|xb, >
         }
         else{
            set <TLSobjList;#TLSobjList# #(quest.thing):prefix#>
            set <TLSobjList;#TLSobjList# |b#@quest.thing#|xb, >
         }
      }
   }
   if ($lengthof(#TLScharList#)$ >0) then {
      set <TLNlengthOf;$lengthof(#TLScharList#)$ - 1>
      set <TLScharList;$left(#TLScharList#;%TLNlengthOf%)$>
      set <TLScharList;$TLFcontentFormat(#TLScharList#)$>
         if ($instr(#TLScharList#;_and_)$ >0) then { 
            msg <#TLScharList# are here.|n>    
         }
         else {
            msg <#TLScharList# is here.|n>        
         }
      }
      if ($lengthof(#TLSobjList#)$ >0) then {     
         set <TLNlengthOf;$lengthof(#TLSobjList#)$ - 1>
         set <TLSobjList;$left(#TLSobjList#;%TLNlengthOf%)$>
         set <TLSobjList;$TLFcontentFormat(#TLSobjList#)$>
         if ($instr(#TLSobjList#;_and_)$ >0) then { 
            msg <You can see #TLSobjList# here.|n>     
         }
         else {
            msg <You can see #TLSobjList# here.|n>         
         }        
      }
      if not is <#quest.doorways.dirs#;> then {
         msg <You can move #quest.doorways.dirs#.>
      }
      if not is <#quest.doorways.places#;> then {
         msg <You can go to #quest.doorways.places#.>
      }
      if not is <#quest.doorways.out#;> then {
         msg <You can go out to |b#quest.doorways.out.display#|xb.>
      }     
end define


'================================================================
'== Override the inbuilt LOOK function - need extra capability ==
'================================================================

define procedure <TLPlook>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
         if ($locationof(#TLSdObj#)$=TLRcontents) then {
            move <#TLSdObj#;#quest.currentroom#>
         }
         exec <look at #TLSdObj#;normal>
         if type <#TLSdObj#;TLTcontainable> then {
            if (#(TLSdObj):isIn#<>nil) then {
               move <#TLSdObj#;TLRContents>
            }
         }
      }
      else exec <look #TLSdObj#;normal>
   do <TLPemptyContents>
end define

'===================================================================
'== Override the inbuilt EXAMINE function - need extra capability ==
'===================================================================

define procedure <TLPexamine>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
         if ($locationof(#TLSdObj#)$=TLRcontents) then {
            move <#TLSdObj#;#quest.currentroom#>
         }         
         do <TLPexamineContainer>
         if type <#TLSdObj#;TLTcontainable> then {
            if (#(TLSdObj):isIn#<>nil) then {
               move <#TLSdObj#;TLRContents>
            }
         }         
      }
   else do <TLPexamineContainer>
   do <TLPemptyContents>
end define

'================================================================
'== Tests examined object & calls contents action where needed ==
'================================================================

define procedure <TLPexamineContainer>
   exec <x #TLSdObj#;normal>
   if type <#TLSdObj#;TLTcontainer> then {
     doaction <#TLSdObj#;contents>
   }
end define

'================================================================
'== Override the inbuilt TAKE function - need extra capability ==
'================================================================

define procedure <TLPtake>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
         if ($locationof(#TLSdObj#)$<>inventory) then  {
            move <#TLSdObj#;#quest.currentroom#>
            if property <#TLSdObj#;takeable> then {
               doaction <#TLSdObj#;take>
               if property <#TLSdObj#;isIn> then {
                  property <#TLSdObj#;isIn=nil>
               }
            }
            else {
            	msg <#(TLSdObj):noTake#>
                if property <#TLSdObj#;isIn> then {
                  	if (#(TLSdObj):isIn#<> nil) then {
 						move <#TLSdObj#;TLRcontents>
					}
                }
			}
         }
         else msg <#TLMalreadyTaken# #(TLSdObj):article#.>
      }
      else doaction <#TLSdObj#;take>
   do <TLPemptyContents>
end define

'================================================================
'== Override the inbuilt DROP function - need extra capability ==
'================================================================

define procedure <TLPdrop>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
         if property <#TLSdObj#;worn> then {
            msg <You'll have to take #(TLSdObj):article# off first.>
         }
         else exec <drop #TLSTemp#;normal>
      }
      else exec <drop #TLStemp#;normal>
end define


'================================================================
'== Override the inbuilt GIVE function - need extra capability ==
'================================================================

define procedure <TLPgive>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
   set <TLSiObj;$TLFgetObjectName(#TLSiObj#)$>
   if (#TLSdObj#=!) then {
      exec <look at #TLSdObj#;normal>
   }
   else {
      if (#TLSIObj#=!) then {
         exec <look at #TLSIObj#;normal>
      }
      else {
       if property <#TLSdObj#;worn> then {
         msg <You'll have to take #(TLSdObj):article# off first.>     
       }
       else {
         do <TLPcheckGive>
      }
      }
   }
   do <TLPemptyContents>
end define

'=============================================================
'== Tests objects & redirects or runs standard give routine == 
'=============================================================

define procedure <TLPcheckGive>
   if type <#TLSdObj#;TLTcontainable> or type <#TLSiObj#;TLTcontainer> then {
      do <TLPcheckIObj>
   }
   else {
      exec <give #TLSdObj# to #TLSiObj#;normal>
   }
end define


'=========================================================
'== 3 utility procedures - manipulate contained objects ==
'=========================================================

define procedure <TLPfillContents>
   for each object in <TLRcontainers> {
      if got <#(quest.thing):isIn#> or here <#(quest.thing):isIn#> then {
         if not property <#(quest.thing):isIn#;closed> then {
            move <#quest.thing#;TLRcontents>
         }
      }
   }   
end define

define procedure <TLPvalidContents>
   for each object in <TLRcontents> {
      if (#(quest.thing):isIn# <> #TLSdobj# ) then {
         move <#quest.thing#;TLRcontainers>
      }
   } 
end define

define procedure <TLPemptyContents>
   for each object in <TLRcontents> {
      move <#quest.thing#;TLRcontainers>
   } 
end define

'===================================
'== Handle the added OPEN command ==
'===================================

define procedure <TLPopen>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
       do <TLPopenObj>
      }
      else exec <look #TLSdObj#;normal>
   do <TLPemptyContents>   
end define

'==================================================================
'== Tests open-ed object & calls open action or denial as needed ==
'==================================================================

define procedure <TLPopenObj>
   if type <#TLSdObj#;TLTclosable> then {
     doaction <#TLSdObj#;open>
   }
   else msg <#TLMnotOpenable#>
end define

'====================================
'== Handle the added CLOSE command ==
'====================================

define procedure <TLPclose>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
       do <TLPcloseObj>
      }
      else exec <look #TLSdObj#;normal>
   do <TLPemptyContents>   
end define

'===================================================================
'== Tests close-d object & calls close action or denial as needed ==
'===================================================================

define procedure <TLPcloseObj>
   if type <#TLSdObj#;TLTclosable> then {
     doaction <#TLSdObj#;close>
   }
   else msg <#TLMnotClosable#>
end define

'=====================================
'== Handle the added PUT IN command ==
'=====================================

define procedure <TLPputIn>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
'   set <TLSiObj;$TLFgetObjectName(#TLSiObj#)$>
   set <TLSiObj;$TLFgetObjectName(#TLSiObj#)$>   
      if (#TLSdObj# <> !) then {
         do <TLPcheckIObj>
      }
      else msg <#TLMdontHaveDObj#>
   do <TLPemptyContents>
end define

'===========================================
'== Test the indirect object is available ==
'===========================================

define procedure <TLPcheckIObj>
   if (#TLSiObj# <> !) then {
      do <TLPcheckContainer>
   }
   else {
   	if property <#TLSiObj#;named> then msg <$displayname(TLSiObj)$ isn't here.>
	else msg <The $displayname(TLSiObj)$ isn't here.>
   }
end define

'======================================
'== Test for container & containable ==
'======================================

define procedure <TLPcheckContainer>
   if not type <#TLSiObj#;TLTcontainer> then {
      msg <You can't put things in $TLFnamed(#TLSiObj#)$.>
   }
   else {
      if property <#TLSiObj#;closed> then {
         msg <You'll have to open $TLFnamed(#TLSiObj#)$...>
      }
      else {
         if not type <#TLSdObj#;TLTcontainable> then {
            msg <You can't put $TLFnamed(#TLSdObj#)$ in anything.>
         }
         else do <TLPcontainerLimits>
      }
   }
end define

'======================================
'== Both legal objects, check limits ==
'======================================
define procedure <TLPcontainerLimits>
   if (#(TLSdObj):isIn#<>#TLSiObj#) then { 
      set <TLNsizeLimit;$TLFsizeHeld(#TLSiObj#)$>
      set <TLNweightLimit;$TLFweightHeld(#TLSiObj#)$>
      set <TLNsizeLimit;#(TLSiObj):sizeLimit# - %TLNsizeLimit%>
      set <TLNweightLimit;#(TLSiObj):weightLimit# - %TLNweightLimit%>
      if (%TLNsizeLimit% < #(TLSdObj):size#) then {
         msg <#(TLSdObj):tooBig#>
      }
      else {
         if (%TLNweightLimit% < #(TLSdObj):weight#) then {
            msg <#(TLSdObj):tooHeavy#>
         }
         else {
            move <#TLSdObj#;TLRcontainers>
            property <#TLSdOBJ#;isIn=#TLSiObj#>
				doaction <#TLSdObj#;contained>
         }
      }  
   }
   else msg <#TLMnoNeed#>
end define

'===================================
'== The 'read' command procedure. ==
'===================================

define procedure <TLPread>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
         if type <#TLSdObj#;TLTreadable> then {
            doaction <#TLSdObj#;read>
         }
         else msg <There's nothing to read!>
      }
      else exec <look #TLSdObj#;normal>
   do <TLPemptyContents> 
end define

'===================================
'== The 'wear' command procedure. ==
'===================================

define procedure <TLPwear>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
         if action <#TLSdObj#;wear> then {
            if (#(TLSdObj):sex# = %TLNplayerSex%) then {
               do <TLPcheckWear>
            }
            else {
               msg <On second thoughts you decide that the |xn>
               msg <#TLSdObj# |xn>
               if (%TLNplayerSex% = 1) then {
                  msg <won't suit a man like you at all.>
               }
               else msg <won't suit a woman like you at all.>
            }
         }
         else {
            if (#(TLSdObj):noWear#= default) then {
            	msg <You cannot wear $TLFnamed(#TLSdObj#)$!>
			}
			else {
				msg <#(TLSdObj):noWear#>
			}
         }
      }
      else msg <$TLFnamed(#TLStemp#)$ isn't here.>
   do <TLPemptyContents>
end define

'========================================================================
'== The 'checkWear' procedure - this checks the sense of wear commands ==
'========================================================================

define procedure <TLPcheckWear>
   set <TLNwearable;0>
   if (#(TLSdObj):topcover# <> 0) and (#(TLSdObj):topcover# <= %TLNtopCovered%) then {
      set <TLNwearable;%TLNwearable% + 1>
   }
   if (#(TLSdObj):headcover# <> 0) and (#(TLSdObj):headcover# <= %TLNheadCovered%) then {
      set <TLNwearable;%TLNwearable% + 1>
   }
   if (#(TLSdObj):feetcover# <> 0) and (#(TLSdObj):feetcover# <= %TLNfeetCovered%) then {
      set <TLNwearable;%TLNwearable% + 1>
   }
   if (#(TLSdObj):handscover# <> 0) and (#(TLSdObj):handscover# <= %TLNhandsCovered%) then {
      set <TLNwearable;%TLNwearable% + 1>
   }
' - disallow for coats (don't affect ability to put on lower torso clothes)
   set <TLNtempCovered;%TLNbotCovered%>
   if (%TLNtempCovered% > 63) and (#(TLSdObj):botcover# < 33) then {
      set <TLNtempCovered;%TLNtempCovered% - 64>
   }
' - disallow for skirts and dresses (like coats!)
   if (%TLNtempCovered% > 31) and (#(TLSdObj):botcover# < 16) and __
   (#(TLSdObj):botcover# <> 4) then {
      set <TLNtempCovered;%TLNtempCovered% - 32>
   }
' - disallow wearing of skirts/dresses and trousers simultaneously
   if (%TLNtempCovered% > 15) then {
      set <TLNtempCovered;%TLNtempCovered% + 16> 
   }
   if (#(TLSdObj):botcover# <> 0) and (#(TLSdObj):botcover# <= %TLNtempCovered%) then {
      set <TLNwearable;%TLNwearable% + 1>
   }
   if (%TLNwearable% =0) then {
      doaction <#TLSdObj#;wear>
      property <#TLSdObj#; worn>
      set <TLNtopCovered;%TLNtopCovered% + #(TLSdObj):topcover#>
      set <TLNheadCovered;%TLNheadCovered% + #(TLSdObj):headcover#>
      set <TLNhandsCovered;%TLNhandsCovered% + #(TLSdObj):handscover#>
      set <TLNfeetCovered;%TLNfeetCovered% + #(TLSdObj):feetcover#>
      set <TLNbotCovered;%TLNbotCovered% + #(TLSdObj):botcover#>
   }
   else {
      msg <Given what you are already wearing - that makes no sense at all.>
   }
end define

'=====================================
'== The 'unwear' command procedure. ==
'=====================================

define procedure <TLPunWear>
   do <TLPfillContents>
   set <TLSdObj;$TLFgetObjectName(#TLSdObj#)$>
      if (#TLSdObj# <> !) then {
         if property <#TLSdObj#; worn> then {
            if action <#TLSdObj#;unwear> then {
               do <TLPcheckUnWear>
            }
            else msg <You can't do that.>
         }
         else msg <You can't do that.>
      }
      else msg <You aren't wearing $TLFnamed(#TLStemp#)$.>
   do <TLPemptyContents>
end define

'==================================
'== The 'CheckUnwear' procedure. ==
'==================================

define procedure <TLPcheckUnWear>
   set <TLNwearable;0>
   set <TLNtempCovered; %TLNtopCovered% /2>
   if (#(TLSdObj):topcover# <> 0) and (#(TLSdObj):topcover# <= %TLNtempCovered%) then {
      set <TLNwearable;%TLNwearable% + 1>
   }
   set <TLNtempCovered; %TLNheadCovered% /2>
   if (#(TLSdObj):headcover# <> 0) and (#(TLSdObj):headcover# <= %TLNtempCovered%) then {
      set <TLNwearable;%TLNwearable% + 2>
   }
   set <TLNtempCovered; %TLNfeetCovered% /2>
   if (#(TLSdObj):feetcover# <> 0) and (#(TLSdObj):feetcover# <= %TLNtempCovered%) then {
      set <TLNwearable;%TLNwearable% + 4>
   }
   set <TLNtempCovered; %TLNhandsCovered% /2>   
      if (#(TLSdObj):handscover# <> 0) and (#(TLSdObj):handscover# <= %TLNtempCovered%) then {
         set <TLNwearable;%TLNwearable% + 8>
   }
' - disallow for coats (don't affect ability to take off lower torso clothes)
   set <TLNtempCovered;%TLNbotCovered%>
   if (%TLNtempCovered% > 63) then {
      set <TLNtempCovered;%TLNtempCovered% - 64>
   }
' - disallow for skirts and dresses (like coats!)
   if (%TLNtempCovered% > 31) and (#(TLSdObj):botcover# <> 4) then {
      set <TLNtempCovered;%TLNtempCcovered% - 32>
   }
   set <TLNtempCovered;%TLNtempCovered% /2>
   if (#(TLSdObj):botcover# <> 0) and (#(TLSdObj):botcover# <= %TLNtempCovered%) then {
      set <TLNwearable;%TLNwearable% + 16>
   }
   if (%TLNwearable% =0) then {
      doaction <#TLSdObj#;unwear>
      property <#TLSdObj#; not worn>
      set <TLNtopCovered;%TLNtopCovered% - #(TLSdObj):topcover#>
      set <TLNheadCovered;%TLNheadCovered% - #(TLSdObj):headcover#>
      set <TLNhandsCovered;%TLNhandsCovered% - #(TLSdObj):handscover#>
      set <TLNfeetCovered;%TLNfeetCovered% - #(TLSdObj):feetcover#>
      set <TLNbotCovered;%TLNbotCovered% - #(TLSdObj):botcover#>
   }
   else {
      msg <Given what you are wearing, that isn't possible.>
   }
end define

'===============
'== FUNCTIONS ==
'===============

'========================================================
'== Returns object real name, checking three locations ==
'========================================================

define function <TLFgetObjectName>
   set <TLStemp;$parameter(1)$>
   set <TLSrealName;$getobjectname(#TLStemp#)$>
'msg <DEBUG #TLStemp# #TLSrealName#>
   if (#TLSrealName#=!) then {
      set <TLSrealName;$getobjectname(#TLStemp#;TLRcontents)$>
   }
   if (#TLSrealName#=!) then {
      set <TLSrealName;$getobjectname(#TLStemp# [worn])$>
        if (#TLSrealName#=!) then {
         set <TLSrealName;$getobjectname(#TLStemp# [worn];TLRcontents)$>
      }
   }
   return <#TLSrealName#>
end define

'=====================================================
'== Replaces last comma in parsed string with "and" ==
'=====================================================

define function <TLFcontentFormat>
   set <TLStemp;$parameter(1)$>
   set <TLNlength;$lengthof(#TLStemp#)$>
   set <TLNcomma;0>
   for <TLNcount; 1; %TLNlength%; 1> {
      if ($mid(#TLStemp#;%TLNcount%;1)$ =,) then {
         set <TLNcomma;%TLNcount%>
      }
   }
   if (%TLNcomma% <>0) then {
      set <TLNcomma;%TLNcomma%-1>
      set <TLSlist;$left(#TLStemp#;%TLNcomma%)$ and>
      set <TLNcomma;%TLNcomma%+2>
      set <TLSlist;#TLSlist#$mid(#TLStemp#;%TLNcomma%)$>     
   }
   else set <TLSlist;#TLStemp#>
   return <#TLSlist#>
end define

'=================================================
'== Returns sizes / weights held in a container ==
'=================================================

define function <TLFsizeHeld>
   set <TLNsizeLimit;0>
   for each object in <TLRcontents> {
      if (#(quest.thing):isIn#=#TLSiObj#) then {
         set <TLNsizeLimit;%TLNsizeLimit% + #(quest.thing):size#>
      }     
   }
   return <%TLNsizeLimit%>
end define

define function <TLFweightHeld>
   set <TLNweightLimit;0>
   for each object in <TLRcontents> {
      if (#(quest.thing):isIn#=#TLSiObj#) then {
         set <TLNweightLimit;%TLNweightLimit% + #(quest.thing):weight#>
      }     
   }
   return <%TLNweightLimit%>
end define

'========================================================
'== Returns proper name or 'the object' as appropriate ==
'========================================================

define function <TLFnamed>
   set <TLStempName;$parameter(1)$>
   if property <#TLStempName#;named> then {
     set <TLStempName;$capfirst(#TLStempName#)$>
   }
   else set <TLStempName;the #TLStempName#>
   return <#TLStempName#>
end define

'======================
'== TYPE DEFINITIONS ==
'======================

define type <TLTactor>
   listHeader = He is carrying
   noSpeak = He says nothing.
   article = he
   displaytype = person
   named
   action <speak> {
      set <TLSthisObj;$thisobject$> 
      msg <#(TLSthisObj):noSpeak#>
   }
   
end define

'=====================================================================
'== The TLTcontainer type: object that you can put other objects in ==
'=====================================================================

define type <TLTcontainer>
   listHeader=It contains
   sizeLimit=100
   weightLimit=100
      
   action <contents> {
   do <TLPvalidContents>
      outputoff
      set <TLSplayerRoom;#quest.currentroom#>
      goto <TLRcontents>
      set <TLScontentList;#quest.formatobjects#>         
      goto <#TLSplayerRoom#>
      outputon
      set <TLSthisObj;$thisobject$>
      if type <#TLSthisObj#;TLTClosable> then {
         if property <#TLSthisObj#;closed> then {
            set <TLScontentList;>
            msg <#(TLSthisObj):closedDesc#>
         }
      }
      if ($lengthof(#TLScontentList#)$ > 0) then {
         msg <#(TLSdObj):listHeader# #TLScontentList#.>
      }
   }
end define

'=========================================================================
'== The TLTcontainable type: object that can be put into a TLTcontainer == 
'=========================================================================

define type <TLTcontainable>
   isIn=nil
   size=25
   weight=25
   tooBig=It is too big to fit.
   tooHeavy=It is too heavy for that.
   action <contained> msg <O.K.>
end define

'=================================================================
'== The TLTclosable type: object that can be opened and closed == 
'=================================================================

define type <TLTclosable>
   closed
   closedDesc=(It is closed, you cannot see inside.)
   isClosedDesc=It is already closed.
   isOpenedDesc=It is already open.
   closingDesc=You close it.
   openingDesc=You open it.
   article=it

   action <open> {
      set <TLSthisObj;$thisobject$>
      if property <#TLSthisObj#;closed> then {
         doaction <#TLSthisObj#;opened>
         property <#TLSthisObj#;not closed>
      }
      else {
         msg <#(TLSthisObj):isOpenedDesc#>
      }
   }

   action <close> {
      set <TLSthisObj;$thisobject$>
      if property <#TLSthisObj#;closed> then {
         msg <#(TLSthisObj):isClosedDesc#>
      }
      else {
         doaction <#TLSthisObj#;closed>
         property <#TLSthisObj#;closed>
      }
   }

   	action <opened> {
    	msg <#(TLSthisObj):openingDesc#>
   	}

   	action <closed> {
   		msg <#(TLSthisObj):closingDesc#>
   	}

end define

'=============================================
'== The TLTreadable type: a readable object ==
'=============================================

define type <TLTreadable>
   readmessage = You start to read but it is so incredibly dull you decide not to bother.
   action <read> {
   set <TLSdObj;$thisobject$>
   msg <#(TLSdObj):readmessage#>
   }
end define


'====================================================
'== The TLTobject type: a visible, takeable object ==
'====================================================

define type <TLTobject>
   takeable
   action <take> {
      set <TLSthisObj;$thisobject$>
      move <#TLSthisObj#;inventory>
      msg <#TLMtaken#>
      if property <#TLSthisObj#;TLTcontainable> then {
         property <#TLSthisObj#;isIn=nil>
      }
   }
end define

'=================================================
'== The TLTscenery type: fixed, unlisted object ==
'=================================================

define type <TLTscenery>
   invisible
end define

'============================================================
'== The default type: common functionality for all objects ==
'============================================================

!addto type <default>
'define type <default>
   prefix=a
      article=it
      displaytype=object
      noTake = Taking that would serve no useful purpose.
	  noWear = default
'end define
!end


'================================================================================
'== This clothing type is not used directly but inherited by specific clothes. ==
'================================================================================
 
define type <TLTclothing>
   type <TLTobject>
   headcover = 0
   handscover = 0
   feetcover = 0
   topcover = 0
   botcover = 0
   sex = 1
   article = it
   wearmessage = You put it on.
   unwearmessage = You take it off.
   properties <displayname=$thisobject$>
      action <wear> {
         set <TLSdObj;$thisobject$>
         msg <#(TLSdObj):wearmessage#>
         property <#TLSdObj#;alias=#(TLSdObj):displayname# [worn]>
         move <#TLSdObj#;inventory>
      }
      action <unwear> {
         set <TLSdObj;$thisobject$>
         msg <#(TLSdObj):unwearmessage#>
         property <#TLSdObj#;alias=#(TLSdObj):displayname#>
      }   
end define  

' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
' * This lib defines 15 specific clothing objects, and most of these can be used    *
' * to good effect for other garments - the defined objects are listed together     *
' * with other garments that work similarly for game purposes.                      *
' *                                                                                 *
' * DEFINED GARMENT : ALSO USABLE FOR THESE GARMENTS                                *
' * hat             : any headwear                                                  *
' * gloves          : any handwear                                                  *
' * shoes           : boots, outer footwear generally                               *
' * socks           : stockings                                                     *
' * tights          : pantie hose                                                   *
' * undies          : panties, briefs - lower portion underwear generally           *
' * teddy           : uhm.. any underthing that covers like a teddy!                *
' * trousers        : jeans, shorts (not the underwear variety)                     *
' * dress           : coverall                                                      *
' * skirt           : kilt maybe?                                                   *
' * vest            : bra, other 'top only' undergarment                            *
' * shirt           : blouse, T-Shirt etc.                                          *
' * sweater         : pullover, sweatshirt - '2nd layer' top garment                *
' * jacket          : fleece, parka, anorak, short coat of whatever type            *
' * coat            : any long length outermost garment like a coat                 *
' * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

define type <TLTshoes>
   type <TLTclothing>
   feetcover = 4
   wearmessage = You put them on.
   unwearmessage = You take them off.
   article = them
   prefix=a pair of
end define

define type <TLTsocks>
   type <TLTclothing>
   feetcover = 2
   wearmessage = You put them on.
   unwearmessage = You take them off.
   article = them
   prefix=a pair of
end define

define type <TLTtights>
   type <TLTclothing>
   feetcover = 2
   botcover = 8
   article = them
   wearmessage = You put them on.
   unwearmessage = You take them off.
   prefix=a pair of
end define

define type <TLThat>
   type <TLTclothing>
   headcover = 2
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
end define

define type <TLTgloves>
   type <TLTclothing>
   handscover = 2
   wearmessage = You put them on.
   unwearmessage = You take them off.
   article = them
   prefix=a pair of
end define

define type <TLTvest>
   type <TLTclothing>
   topcover = 2
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
end define

define type <TLTshirt>
   type <TLTclothing>
   topcover = 8
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
end define

define type <TLTteddy>
   type <TLTclothing>
   topcover = 4
   botcover = 4
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
 end define

define type <TLTundies>
   type <TLTclothing>
   botcover = 2
   wearmessage = You put them on.
   unwearmessage = You take them off.
   article = them
   prefix=a pair of
end define

define type <TLTdress>
   type <TLTclothing>
   topcover = 8
   botcover = 32
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
end define

define type <TLTskirt>
   type <TLTclothing>
   botcover = 32
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
end define

define type <TLTtrousers>
   type <TLTclothing>
   botcover = 16
   wearmessage = You put them on.
   unwearmessage = You take them off.
   article = them
   prefix=a pair of
end define

define type <TLTsweater>
   type <TLTclothing>
   topcover = 16
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
end define

define type <TLTjacket>
   type <TLTclothing>
   topcover = 32
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
end define

define type <TLTcoat>
   type <TLTclothing>
   topcover = 64
   botcover = 64
   wearmessage = You put it on.
   unwearmessage = You take it off.
   prefix=a
end define


'==========================
'== INTERFACE FOR Q.D.K. ==
'==========================

!QDK

'script <MaDbRiTs Types Library>
'   command <Initialise the library (use in startscript); TLPstartup>
'      display <Initialise MaDbRiTs Types Library.>

object <Basics>
   type <Regular object; TLTobject>
   type <Scenery (not takeable or described) object; TLTscenery> 
   property <Make regular object not takeable;not takeable>
   property <Not takeable msg;noTake;text>
   property <Not wearable msg;noWear;text>
   
object <Readable>
   type <Readable object;TLTreadable>
   property <Message if read;readMessage;text>
   action <Action if read;read>

object <Containers>
   type <Container object (can have things put in it); TLTcontainer>
   property <Header for listing;listHeader;text> 
   property <Size Limit;sizeLimit;text>
   property <Weight Limit;weightLimit;text>
   
   type <Containable object (can be put in things); TLTcontainable>  
   property <Size;size;text>
   property <Weight;weight;text>
   property <Start game inside;isIn;objects>
   property <Msg if Too Big;tooBig;text>
   property <Msg if Too Heavy;tooHeavy;text>
	action <Action when contained;contained>

object <Actor>
   type <Actor can carry things;TLTcontainer>
   property <Header for listing;listHeader;text> 
   property <Size Limit;sizeLimit;text>
   property <Weight Limit;weightLimit;text>
   type <Actor; TLTactor>
   property <Actor not named. (e.g. 'the sailor' not 'Jack');not named>
   property <Default speak reply;noSpeak;text>
   action <Script if spoken to;speak>
   
object <Closables>   
   type <Closable object     NOTE Following properties all have useful defaults;TLTclosable>
   property <Start open?;not closed>
   property <Closed text (container);closedDesc;text>
   property <Is closed description;isClosedDesc;text>   
   property <Closing description;closingDesc;text>   
   property <Is open description;isOpenDesc;text>   
   property <Opening description;openingDesc;text>
   action <Script when opened;opened>
   action <Script when closed;closed>

object <Clothing 1>  
   type <Sweater;TLTsweater>
   type <Shirt;TLTshirt>
   type <Vest (top half underwear / bra etc.);TLTvest>
   type <Teddy (or 1 piece swimsuit etc.);TLTteddy>
   type <Underwear (shorts or briefs);TLTundies>
   type <Socks;TLTsocks>
   type <Tights (a.k.a. Panty Hose);TLTtights>
   type <Dress;TLTdress>
   type <Skirt;TLTskirt>
   type <Trousers;TLTtrousers>
   type <Shoes;TLTshoes>
   type <Hat           NOTE Clothing continues on next tab!;TLThat>   
   
   
object <Clothing 2>
   type <Gloves;TLTgloves>
   type <Jacket (short coat);TLTjacket>
   type <Coat (long coat)       NOTE Properties below have defaults! See the manual;TLTcoat>  
   property <Msg when put on;wearmessage;text>
   property <Msg when taken off;unwearmessage;text>   
   property <Head cover value;headcover;text>
   property <Hands cover value;handscover;text>
   property <Feet cover value;feetcover;text>
   property <Top cover value;topcover;text>
   property <Bottom cover value;botcover;text>
   property <Sex;sex;text>
   property <Start game worn?;worn>
!end
`;
libraries["net.lib"] = `!library
!asl-version <350>
!name <QuestNet Standard Library>
!version <1.0>
!author <Alex Warren>
! This library adds useful additional QuestNet functions. We recommend you include this library in all QuestNet (multiplayer) games.

' NET.LIB v1.0
' for QuestNet Server 3.5
' Copyright © 2004 Axe Software. Please do not modify this library.

!QDK

	object <QuestNet>
		type <This object can be &given to other players; giveable>

!end


define type <giveable>
        action <give to anything> {
                if ( #quest.give.object.name# = player%userid% ) then {
                        msg <It is silly to give things to yourself!>
                }
                else {
                        if property <#quest.give.object.name#; netplayer> then {
                                move <$thisobject$; #quest.give.object.name#>
                                msg <You give $name(#quest.give.object.name#)$ the $thisobjectname$.>
                                msgto <#quest.give.object.name#; |b$name(%userid%)$|xb has given you a |b$thisobjectname$|xb.>
                                if action <$thisobject$; gain> then {
                                	with <#quest.give.object.name#> {
                                		doaction <$thisobject$; gain>
                                	}
                                }
                                if action <$thisobject$; lose> then {
                                	doaction <$thisobject$; lose>
                                }
                        }
                        else {
                                msg <The $displayname(#quest.give.object.name#)$ doesn't want that.>
                        }
                }
        }
end define
`;