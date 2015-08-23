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
	// UNKNOWN ConstructorBlock
	// UNKNOWN ConstructorBlock
	RemoveFormatting(s: string): string {
		var code: string;
		var pos: number;
		var len: number;
		// UNKNOWN DoLoopUntilBlock
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
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN DoLoopUntilBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		return hasErrors;
	}
	GetAfterParameter(s: string): string {
		var eop: number;
		eop = InStr(s, ">");
		// UNKNOWN MultiLineIfBlock
	}
	ObliterateParameters(s: string): string {
		var inParameter: boolean;
		var exitCharacter: string = "";
		var curChar: string;
		var outputLine: string = "";
		var obscuringFunctionName: boolean;
		inParameter = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
	}
	ObliterateVariableNames(s: string): string {
		var inParameter: boolean;
		var exitCharacter: string = "";
		var outputLine: string = "";
		var curChar: string;
		inParameter = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		return keyword;
	}
	ConvertMultiLines(): void {
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		return result;
	}
	GetEverythingAfter(s: string, text: string): string {
		// UNKNOWN MultiLineIfBlock
		return Right(s, Len(s) - Len(text));
	}
	Keyword2CAS(KWord: string): string {
		var k = "";
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		numLibraries = 0;
		// UNKNOWN DoLoopUntilBlock
		skipCheck = false;
		var lastSlashPos: number;
		var slashPos: number;
		var curPos = 1;
		// UNKNOWN DoLoopUntilBlock
		var filenameNoPath = LCase(Mid(filename, lastSlashPos + 1));
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		this._numberSections = 1;
		// UNKNOWN ForBlock
		this._numberSections = this._numberSections - 1;
		var gotGameBlock = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		hasErrors = this.ConvertFriendlyIfs();
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		this._saveGameFile = "";
		return result;
	}
	LogASLError(err: string, type: LogType = LogType.Misc): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	GetParameter(s: string, ctx: Context, convertStringVariables: boolean = true): string {
		var newParam: string;
		var startPos: number;
		var endPos: number;
		startPos = InStr(s, "<");
		endPos = InStr(s, ">");
		// UNKNOWN MultiLineIfBlock
		var retrParam = Mid(s, startPos + 1, (endPos - startPos) - 1);
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN ReDimStatement
		// UNKNOWN MultiLineIfBlock
		chkVer = Left(fileData, 7);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
	}
	DecryptString(s: string): string {
		var output = "";
		// UNKNOWN ForBlock
		return output;
	}
	RemoveTabs(s: string): string {
		// UNKNOWN MultiLineIfBlock
		return s;
	}
	DoAddRemove(childId: number, parentId: number, add: boolean, ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	DoLook(id: number, ctx: Context, showExamineError: boolean = false, showDefaultDescription: boolean = true): void {
		var objectContents: string;
		var foundLook = false;
		// UNKNOWN MultiLineIfBlock
		var lookLine: string;
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
	}
	DoOpenClose(id: number, open: boolean, showLook: boolean, ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	EvaluateInlineExpressions(s: string): string {
		// UNKNOWN MultiLineIfBlock
		var bracePos: number;
		var curPos = 1;
		var resultLine = "";
		// UNKNOWN DoLoopUntilBlock
		curPos = 1;
		// UNKNOWN DoLoopUntilBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		childLength = sepPos - (Len(verb) + 2);
		// UNKNOWN MultiLineIfBlock
		childName = Trim(Mid(cmd, Len(verb) + 2, childLength));
		gotObject = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		isContainer = this.IsYes(this.GetObjectProperty("container", parentId, true, false));
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var canAccessObject = this.PlayerCanAccessObject(childId);
		// UNKNOWN MultiLineIfBlock
		var canAccessParent = this.PlayerCanAccessObject(parentId);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var o = this._objs[parentId];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecAddRemoveScript(parameter: string, add: boolean, ctx: Context): void {
		var childId: number;
		var parentId: number;
		var commandName: string;
		var childName: string;
		var parentName: string = "";
		var scp: number;
		// UNKNOWN MultiLineIfBlock
		scp = InStr(parameter, ";");
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		childId = this.GetObjectIdNoAlias(childName);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		name = this.GetEverythingAfter(cmd, action + " ");
		id = this.Disambiguate(name, this._currentRoom + ";inventory", ctx);
		// UNKNOWN MultiLineIfBlock
		isContainer = this.IsYes(this.GetObjectProperty("container", id, true, false));
		// UNKNOWN MultiLineIfBlock
		isOpen = this.IsYes(this.GetObjectProperty("opened", id, true, false));
		// UNKNOWN MultiLineIfBlock
		var canAccessObject = this.PlayerCanAccessObject(id);
		// UNKNOWN MultiLineIfBlock
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteSelectCase(script: string, ctx: Context): void {
		var afterLine = this.GetAfterParameter(script);
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		gameBlock = this.GetDefineBlock("game");
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		return foundVerb;
	}
	ExpressionHandler(expr: string): ExpressionResult {
		var openBracketPos: number;
		var endBracketPos: number;
		var res: ExpressionResult = new ExpressionResult();
		// UNKNOWN DoLoopUntilBlock
		var numElements = 1;
		var elements: string[];
		// UNKNOWN ReDimStatement
		var numOperators = 0;
		var operators: string[] = [];
		var newElement: boolean;
		var obscuredExpr = this.ObscureNumericExps(expr);
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		var opNum = 0;
		// UNKNOWN DoLoopUntilBlock
		res.Success = ExpressionSuccess.OK;
		res.Result = elements[1];
		return res;
	}
	ListContents(id: number, ctx: Context): string {
		var contentsIDs: number[] = [];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var numContents = 0;
		// UNKNOWN ForBlock
		var contents = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ObscureNumericExps(s: string): string {
		var EPos: number;
		var CurPos: number;
		var OutputString: string;
		OutputString = s;
		CurPos = 1;
		// UNKNOWN DoLoopUntilBlock
		return OutputString;
	}
	ProcessListInfo(line: string, id: number): void {
		var listInfo: TextAction = new TextAction();
		var propName: string = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		var roomExit: RoomExit;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	DoClear(): void {
		// UNKNOWN ExpressionStatement
	}
	DoWait(): void {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
	}
	ExecuteFlag(line: string, ctx: Context): void {
		var propertyString: string = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ExecuteIfFlag(flag: string): boolean {
		return this.GetObjectProperty(flag, 1, true) == "yes";
	}
	ExecuteIncDec(line: string, ctx: Context): void {
		var variable: string;
		var change: number;
		var param = this.GetParameter(line, ctx);
		var sc = InStr(param, ";");
		// UNKNOWN MultiLineIfBlock
		var value = this.GetNumericContents(variable, ctx, true);
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		var arrayIndex = this.GetArrayIndex(variable, ctx);
		// UNKNOWN ExpressionStatement
	}
	ExtractFile(file: string): string {
		var length: number;
		var startPos: number;
		var extracted: boolean;
		var resId: number;
		// UNKNOWN SingleLineIfStatement
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var fileName = System.IO.Path.Combine(this._tempFolder, file);
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		return fileName;
	}
	AddObjectAction(id: number, name: string, script: string, noUpdate: boolean = false): void {
		var actionNum: number;
		var foundExisting = false;
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		o.Actions[actionNum].ActionName = name;
		o.Actions[actionNum].Script = script;
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN ExpressionStatement
	}
	AddToGiveInfo(id: number, giveData: string): void {
		var giveType: GiveType;
		var actionName: string;
		var actionScript: string;
		var o = this._objs[id];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	AddToObjectActions(actionInfo: string, id: number, ctx: Context): void {
		var actionNum: number;
		var foundExisting = false;
		var name = LCase(this.GetParameter(actionInfo, ctx));
		var ep = InStr(actionInfo, ">");
		// UNKNOWN MultiLineIfBlock
		var script = Trim(Mid(actionInfo, ep + 1));
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		o.Actions[actionNum].ActionName = name;
		o.Actions[actionNum].Script = script;
		// UNKNOWN ExpressionStatement
	}
	AddToObjectAltNames(altNames: string, id: number): void {
		var o = this._objs[id];
		// UNKNOWN DoLoopUntilBlock
	}
	AddToObjectProperties(propertyInfo: string, id: number, ctx: Context): void {
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN DoLoopUntilBlock
	}
	AddToUseInfo(id: number, useData: string): void {
		var useType: UseType;
		var o = this._objs[id];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		return false;
	}
	ExecClone(cloneString: string, ctx: Context): void {
		var id: number;
		var newName: string;
		var cloneTo: string;
		var SC = InStr(cloneString, ";");
		// UNKNOWN MultiLineIfBlock
		this._numberObjs = this._numberObjs + 1;
		// UNKNOWN ReDimPreserveStatement
		this._objs[this._numberObjs] = new ObjectType();
		this._objs[this._numberObjs] = this._objs[id];
		this._objs[this._numberObjs].ContainerRoom = cloneTo;
		this._objs[this._numberObjs].ObjectName = newName;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ExecOops(correction: string, ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
	}
	ExecType(typeData: string, ctx: Context): void {
		var id: number;
		var found: boolean;
		var scp = InStr(typeData, ";");
		// UNKNOWN MultiLineIfBlock
		var objName = Trim(Left(typeData, scp - 1));
		var typeName = Trim(Mid(typeData, scp + 1));
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var o = this._objs[id];
		o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
		// UNKNOWN ReDimPreserveStatement
		o.TypesIncluded[o.NumberTypesIncluded] = typeName;
		var propertyData = this.GetPropertiesInType(typeName);
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
	}
	ExecuteIfAction(actionData: string): boolean {
		var id: number;
		var scp = InStr(actionData, ";");
		// UNKNOWN MultiLineIfBlock
		var objName = Trim(Left(actionData, scp - 1));
		var actionName = Trim(Mid(actionData, scp + 1));
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var o = this._objs[id];
		// UNKNOWN ForBlock
		return false;
	}
	ExecuteIfType(typeData: string): boolean {
		var id: number;
		var scp = InStr(typeData, ";");
		// UNKNOWN MultiLineIfBlock
		var objName = Trim(Left(typeData, scp - 1));
		var typeName = Trim(Mid(typeData, scp + 1));
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var o = this._objs[id];
		// UNKNOWN ForBlock
		return false;
	}
	GetArrayIndex(varName: string, ctx: Context): ArrayResult {
		var result: ArrayResult = new ArrayResult();
		// UNKNOWN MultiLineIfBlock
		var beginPos = InStr(varName, "[");
		var endPos = InStr(varName, "]");
		var data = Mid(varName, beginPos + 1, (endPos - beginPos) - 1);
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		this._thisTurnIt = 0;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		this._thisTurnIt = this._lastIt;
		// UNKNOWN ExpressionStatement
		return -1;
	}
	DisplayStatusVariableInfo(id: number, type: VarType, ctx: Context): string {
		var displayData: string = "";
		var ep: number;
		// UNKNOWN MultiLineIfBlock
		return displayData;
	}
	DoAction(ObjID: number, ActionName: string, ctx: Context, LogError: boolean = true): boolean {
		var FoundAction: boolean;
		var ActionScript: string = "";
		var o = this._objs[ObjID];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var NewThread: Context = this.CopyContext(ctx);
		NewThread.CallingObjectId = ObjID;
		// UNKNOWN ExpressionStatement
		return true;
	}
	HasAction(ObjID: number, ActionName: string): boolean {
		var o = this._objs[ObjID];
		// UNKNOWN ForBlock
		return false;
	}
	ExecForEach(scriptLine: string, ctx: Context): void {
		var inLocation: string;
		var scriptToRun: string;
		var isExit: boolean;
		var isRoom: boolean;
		// UNKNOWN MultiLineIfBlock
		scriptLine = this.GetEverythingAfter(scriptLine, "in ");
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		var objName = Trim(Left(param, scp - 1));
		actionName = Trim(Mid(param, scp + 1));
		var ep = InStr(data, ">");
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		o.Actions[actionNum].ActionName = actionName;
		o.Actions[actionNum].Script = script;
		// UNKNOWN ExpressionStatement
	}
	ExecuteCondition(condition: string, ctx: Context): boolean {
		var result: boolean;
		var thisNot: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN DoLoopUntilBlock
		operations[0] = "AND";
		var result = true;
		// UNKNOWN ForBlock
		return result;
	}
	ExecuteCreate(data: string, ctx: Context): void {
		var newName: string;
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteCreateExit(data: string, ctx: Context): void {
		var scrRoom: string;
		var destRoom: string = "";
		var destId: number;
		var exitData = this.GetEverythingAfter(data, "exit ");
		var newName = this.GetParameter(data, ctx);
		var scp = InStr(newName, ";");
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var srcId = this.GetRoomID(scrRoom, ctx);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var exists = false;
		// UNKNOWN MultiLineIfBlock
		var paramPos = InStr(exitData, "<");
		var saveData: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		var r = this._rooms[srcId];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecDrop(obj: string, ctx: Context): void {
		var found: boolean;
		var parentId: number;
		var id: number;
		id = this.Disambiguate(obj, "inventory", ctx);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var isInContainer = false;
		// UNKNOWN MultiLineIfBlock
		var dropFound = false;
		var dropStatement: string = "";
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecExamine(command: string, ctx: Context): void {
		var item = LCase(Trim(this.GetEverythingAfter(command, "examine ")));
		// UNKNOWN MultiLineIfBlock
		var id = this.Disambiguate(item, this._currentRoom + ";inventory", ctx);
		// UNKNOWN MultiLineIfBlock
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
	}
	ExecMoveThing(data: string, type: Thing, ctx: Context): void {
		var scp = InStr(data, ";");
		var name = Trim(Left(data, scp - 1));
		var place = Trim(Mid(data, scp + 1));
		// UNKNOWN ExpressionStatement
	}
	ExecProperty(data: string, ctx: Context): void {
		var id: number;
		var found: boolean;
		var scp = InStr(data, ";");
		// UNKNOWN MultiLineIfBlock
		var name = Trim(Left(data, scp - 1));
		var properties = Trim(Mid(data, scp + 1));
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ExecuteDo(procedureName: string, ctx: Context): void {
		var newCtx: Context = this.CopyContext(ctx);
		var numParameters = 0;
		var useNewCtx: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var block = this.DefineBlockParam("procedure", procedureName);
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteDoAction(data: string, ctx: Context): void {
		var id: number;
		var scp = InStr(data, ";");
		// UNKNOWN MultiLineIfBlock
		var objName = LCase(Trim(Left(data, scp - 1)));
		var actionName = Trim(Mid(data, scp + 1));
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ExecuteIfHere(obj: string, ctx: Context): boolean {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		return false;
	}
	ExecuteIfExists(obj: string, realOnly: boolean): boolean {
		var result: boolean;
		var errorReport = false;
		var scp: number;
		// UNKNOWN MultiLineIfBlock
		var found = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		return result;
	}
	ExecuteIfProperty(data: string): boolean {
		var id: number;
		var scp = InStr(data, ";");
		// UNKNOWN MultiLineIfBlock
		var objName = Trim(Left(data, scp - 1));
		var propertyName = Trim(Mid(data, scp + 1));
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		return this.GetObjectProperty(propertyName, id, true) == "yes";
	}
	ExecuteRepeat(data: string, ctx: Context): void {
		var repeatWhileTrue: boolean;
		var repeatScript: string = "";
		var bracketPos: number;
		var afterBracket: string;
		var foundScript = false;
		// UNKNOWN MultiLineIfBlock
		var pos = 1;
		// UNKNOWN DoLoopUntilBlock
		var conditions = Trim(Left(data, bracketPos));
		var finished = false;
		// UNKNOWN DoLoopUntilBlock
	}
	ExecuteSetCollectable(param: string, ctx: Context): void {
		var val: number;
		var id: number;
		var scp = InStr(param, ";");
		var name = Trim(Left(param, scp - 1));
		var newVal = Trim(Right(param, Len(param) - scp));
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var op = Left(newVal, 1);
		var newValue = Trim(Right(newVal, Len(newVal) - 1));
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	ExecuteWait(waitLine: string, ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	InitFileData(fileData: string): void {
		this._fileData = fileData;
		this._fileDataPos = 1;
	}
	GetNextChunk(): string {
		var nullPos = InStr(this._fileDataPos, this._fileData, Chr(0));
		var result = Mid(this._fileData, this._fileDataPos, nullPos - this._fileDataPos);
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		var script = Trim(Mid(actionInfo, ep + 1));
		var result: ActionType = new ActionType();
		result.ActionName = name;
		result.Script = script;
		return result;
	}
	GetObjectId(name: string, ctx: Context, room: string = ""): number {
		var id: number;
		var found = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		return "";
	}
	GetPropertiesInType(type: string, err: boolean = true): PropertiesActions {
		var blockId: number;
		var propertyList: PropertiesActions = new PropertiesActions();
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		return propertyList;
	}
	GetRoomID(name: string, ctx: Context): number {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		return 0;
	}
	GetTextOrScript(textScript: string): TextAction {
		var result = new TextAction();
		textScript = Trim(textScript);
		// UNKNOWN MultiLineIfBlock
		return result;
	}
	GetThingNumber(name: string, room: string, type: Thing): number {
		// UNKNOWN MultiLineIfBlock
		return -1;
	}
	GetThingBlock(name: string, room: string, type: Thing): DefineBlock {
		var result = new DefineBlock();
		// UNKNOWN MultiLineIfBlock
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
		var dataString: string;
		var newFileData: any = {};
		dataString = data.ToString();
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		return newFileData.ToString();
	}
	MoveThing(name: string, room: string, type: Thing, ctx: Context): void {
		var oldRoom: string = "";
		var id = this.GetThingNumber(name, "", type);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
	}
	Pause(duration: number): void {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
	}
	ConvertParameter(parameter: string, convertChar: string, action: ConvertType, ctx: Context): string {
		var result: string = "";
		var pos = 1;
		var finished = false;
		// UNKNOWN DoLoopUntilBlock
		return result;
	}
	DoFunction(data: string, ctx: Context): string {
		var name: string;
		var parameter: string;
		var intFuncResult: string = "";
		var intFuncExecuted = false;
		var paramPos = InStr(data, "(");
		// UNKNOWN MultiLineIfBlock
		var block: DefineBlock;
		block = this.DefineBlockParam("function", name);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	DoInternalFunction(name: string, parameter: string, ctx: Context): string {
		var parameters: string[];
		var untrimmedParameters: string[];
		var objId: number;
		var numParameters = 0;
		var pos = 1;
		// UNKNOWN MultiLineIfBlock
		var param2: string;
		var param3: string;
		// UNKNOWN MultiLineIfBlock
		return "__NOTDEFINED";
	}
	ExecFor(line: string, ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
		var endValue: number;
		var stepValue: number;
		var forData = this.GetParameter(line, ctx);
		var scp1 = InStr(forData, ";");
		var scp2 = InStr(scp1 + 1, forData, ";");
		var scp3 = InStr(scp2 + 1, forData, ";");
		var counterVariable = Trim(Left(forData, scp1 - 1));
		var startValue = parseInt(Mid(forData, scp1 + 1, (scp2 - 1) - scp1));
		// UNKNOWN MultiLineIfBlock
		var loopScript = Trim(Mid(line, InStr(line, ">") + 1));
		// UNKNOWN ForBlock
	}
	ExecSetVar(varInfo: string, ctx: Context): void {
		var scp = InStr(varInfo, ";");
		var varName = Trim(Left(varInfo, scp - 1));
		var varCont = Trim(Mid(varInfo, scp + 1));
		var idx = this.GetArrayIndex(varName, ctx);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN TryBlock
	}
	ExecuteIfAsk(question: string): boolean {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		return this._questionResponse;
	}
	SetQuestionResponse(response: boolean): void {
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	SetQuestionResponseInNewThread(response: Object): void {
		this._questionResponse = response;
		// UNKNOWN SyncLockBlock
	}
	ExecuteIfGot(item: string): boolean {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN MultiLineIfBlock
		var op = Left(newVal, 1);
		var newValue = Trim(Right(newVal, Len(newVal) - 1));
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		return false;
	}
	ExecuteIfIs(condition: string): boolean {
		var value1: string;
		var value2: string;
		var op: string;
		var expectNumerics: boolean;
		var expResult: ExpressionResult;
		var scp = InStr(condition, ";");
		// UNKNOWN MultiLineIfBlock
		var scp2 = InStr(scp + 1, condition, ";");
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var result = false;
		// UNKNOWN SelectBlock
		// UNKNOWN MultiLineIfBlock
		return result;
	}
	GetNumericContents(name: string, ctx: Context, noError: boolean = false): number {
		var numNumber: number;
		var arrayIndex: number;
		var exists = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		return Val(this._numericVariable[numNumber].VariableContents[arrayIndex]);
	}
	PlayerErrorMessage(e: PlayerError, ctx: Context): void {
		// UNKNOWN ExpressionStatement
	}
	PlayerErrorMessage_ExtendInfo(e: PlayerError, ctx: Context, extraInfo: string): void {
		var message = this.GetErrorMessage(e, ctx);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	GetErrorMessage(e: PlayerError, ctx: Context): string {
		return this.ConvertParameter(this.ConvertParameter(this.ConvertParameter(this._playerErrorMessageString[e], "%", ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx), "#", ConvertType.Strings, ctx);
	}
	PlayMedia(filename: string): void {
		// UNKNOWN ExpressionStatement
	}
	PlayMedia(filename: string, sync: boolean, looped: boolean): void {
		// UNKNOWN MultiLineIfBlock
	}
	PlayWav(parameter: string): void {
		var sync: boolean = false;
		var looped: boolean = false;
		var params: any = {};
		params = new any();
		var filename = params[0];
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN ExpressionStatement
	}
	SetForeground(col: string): void {
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN ExpressionStatement
	}
	SetFontSize(size: number): void {
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ExpressionStatement
	}
	SetNumericVariableContents(name: string, content: number, ctx: Context, arrayIndex: number = 0): void {
		var numNumber: number;
		var exists = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		this._numericVariable[numNumber].VariableName = name;
		// UNKNOWN ReDimPreserveStatement
		this._numericVariable[numNumber].VariableContents[arrayIndex] = (content).toString();
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	SetOpenClose(name: string, open: boolean, ctx: Context): void {
		var cmd: string;
		// UNKNOWN MultiLineIfBlock
		var id = this.GetObjectIdNoAlias(name);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	SetTimerState(name: string, state: boolean): void {
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
	}
	SetUnknownVariableType(variableData: string, ctx: Context): SetResult {
		var scp = InStr(variableData, ";");
		// UNKNOWN MultiLineIfBlock
		var name = Trim(Left(variableData, scp - 1));
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN ExpressionStatement
		var mnu: MenuData = new MenuData();
		var choice: string = this.ShowMenu(mnu);
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN ReDimStatement
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
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ShowPictureInText(filename: string): void {
		// UNKNOWN MultiLineIfBlock
	}
	ShowRoomInfoV2(Room: string): void {
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
		this._currentRoom = Room;
		var roomBlock: DefineBlock;
		roomBlock = this.DefineBlockParam("room", Room);
		var finishedFindingCommas: boolean;
		charsViewable = "";
		charsFound = 0;
		// UNKNOWN ForBlock
		// UNKNOWN SingleLineIfStatement
		prefix = this.FindStatement(roomBlock, "prefix");
		// UNKNOWN MultiLineIfBlock
		inDesc = "unfound";
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		roomDisplayText = roomDisplayText + charsViewable + vbCrLf;
		noFormatObjsViewable = "";
		// UNKNOWN ForBlock
		var finishedLoop: boolean;
		// UNKNOWN MultiLineIfBlock
		doorways = "";
		nsew = "";
		places = "";
		possDir = "";
		// UNKNOWN ForBlock
		var outside: DefineBlock;
		// UNKNOWN MultiLineIfBlock
		var finished: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		descTagExist = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		defineBlock = 0;
		// UNKNOWN ForBlock
		// UNKNOWN SingleLineIfStatement
	}
	Speak(text: string): void {
		// UNKNOWN ExpressionStatement
	}
	AddToObjectList(objList: any, exitList: any, name: string, type: Thing): void {
		name = this.CapFirst(name);
		// UNKNOWN MultiLineIfBlock
	}
	ExecExec(scriptLine: string, ctx: Context): void {
		// UNKNOWN SingleLineIfStatement
		var execLine = this.GetParameter(scriptLine, ctx);
		var newCtx: Context = this.CopyContext(ctx);
		newCtx.StackCounter = newCtx.StackCounter + 1;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecSetString(info: string, ctx: Context): void {
		var scp = InStr(info, ";");
		var name = Trim(Left(info, scp - 1));
		var value = Mid(info, scp + 1);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var idx = this.GetArrayIndex(name, ctx);
		// UNKNOWN ExpressionStatement
	}
	ExecUserCommand(cmd: string, ctx: Context, libCommands: boolean = false): boolean {
		var curCmd: string;
		var commandList: string;
		var script: string = "";
		var commandTag: string;
		var commandLine: string = "";
		var foundCommand = false;
		var roomId = this.GetRoomID(this._currentRoom, ctx);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		return foundCommand;
	}
	ExecuteChoose(section: string, ctx: Context): void {
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN DoLoopUntilBlock
		var success = true;
		// UNKNOWN ForBlock
		return success;
	}
	GetGender(character: string, capitalise: boolean, ctx: Context): string {
		var result: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		return result;
	}
	GetStringContents(name: string, ctx: Context): string {
		var returnAlias = false;
		var arrayIndex = 0;
		var cp = InStr(name, ":");
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var exists = false;
		var id: number;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	IsAvailable(thingName: string, type: Thing, ctx: Context): boolean {
		var room: string;
		var name: string;
		var atPos = InStr(thingName, "@");
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	IsCompatible(test: string, required: string): boolean {
		var var2Pos: number;
		test = "�" + Trim(test) + "�";
		required = "�" + required + "�";
		var currentReqLinePos = 1;
		var currentTestLinePos = 1;
		var finished = false;
		// UNKNOWN DoLoopUntilBlock
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
		// UNKNOWN MultiLineIfBlock
		savedQsgVersion = Left(fileData, 10);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		result = this.InitialiseGame(this._gameFileName, true);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		this._saveGameFile = filename;
		return true;
	}
	SaveGame(filename: string, saveFile: boolean = true): number[] {
		var ctx: Context = new Context();
		var saveData: string;
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		this._saveGameFile = filename;
		return System.Text.Encoding.GetEncoding(1252).GetBytes(saveData);
	}
	MakeRestoreDataV2(): string {
		var lines: any = {};
		var i: number;
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
		return String.Join(vbCrLf, lines);
	}
	SetAvailability(thingString: string, exists: boolean, ctx: Context, type: Thing = Thing.Object): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	SetStringContents(name: string, value: string, ctx: Context, arrayIndex: number = 0): void {
		var id: number;
		var exists = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		this._stringVariable[id].VariableName = name;
		// UNKNOWN ReDimPreserveStatement
		this._stringVariable[id].VariableContents[arrayIndex] = value;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	SetUpCharObjectInfo(): void {
		var defaultProperties: PropertiesActions = new PropertiesActions();
		this._numberChars = 0;
		var defaultExists = false;
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
	}
	ShowGameAbout(ctx: Context): void {
		var version = this.FindStatement(this.GetDefineBlock("game"), "game version");
		var author = this.FindStatement(this.GetDefineBlock("game"), "game author");
		var copyright = this.FindStatement(this.GetDefineBlock("game"), "game copyright");
		var info = this.FindStatement(this.GetDefineBlock("game"), "game info");
		// UNKNOWN ExpressionStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
	}
	ShowPicture(filename: string): void {
		var caption: string = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ExpressionStatement
	}
	ShowRoomInfo(room: string, ctx: Context, noPrint: boolean = false): void {
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		inDescription = this._rooms[id].InDescription;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		visibleObjectsNoFormat = "";
		var visibleObjectsList: any = {};
		var count: number;
		// UNKNOWN ForBlock
		// UNKNOWN ForEachBlock
		// UNKNOWN MultiLineIfBlock
		doorwayString = this.UpdateDoorways(id, ctx);
		// UNKNOWN MultiLineIfBlock
		objLook = this.GetObjectProperty("look", this._rooms[id].ObjId, null, false);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		showLookText = true;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		this._collectables[id].Value = value;
	}
	DisplayCollectableInfo(id: number): string {
		var display: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		return display;
	}
	DisplayTextSection(section: string, ctx: Context): void {
		var block: DefineBlock;
		block = this.DefineBlockParam("text", section);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN MultiLineIfBlock
		input = LCase(input);
		// UNKNOWN ExpressionStatement
		var newCommand = " " + input + " ";
		// UNKNOWN ForBlock
		input = Mid(newCommand, 2, Len(newCommand) - 2);
		// UNKNOWN ExpressionStatement
		var newCtx: Context = this.CopyContext(ctx);
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecLook(lookLine: string, ctx: Context): void {
		var item: string;
		// UNKNOWN MultiLineIfBlock
	}
	ExecSpeak(cmd: string, ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
		var name = cmd;
		// UNKNOWN MultiLineIfBlock
	}
	ExecTake(item: string, ctx: Context): void {
		var parentID: number;
		var parentDisplayName: string;
		var foundItem = true;
		var foundTake = false;
		var id = this.Disambiguate(item, this._currentRoom, ctx);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var isInContainer = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var id: number;
		var notGotItem: boolean;
		// UNKNOWN MultiLineIfBlock
		var useScript: string = "";
		var foundUseScript: boolean;
		var foundUseOnObject: boolean;
		var useOnObjectId: number;
		var found: boolean;
		// UNKNOWN MultiLineIfBlock
	}
	ObjectActionUpdate(id: number, name: string, script: string, noUpdate: boolean = false): void {
		var objectName: string;
		var sp: number;
		var ep: number;
		name = LCase(name);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteIf(scriptLine: string, ctx: Context): void {
		var ifLine = Trim(this.GetEverythingAfter(Trim(scriptLine), "if "));
		var obscuredLine = this.ObliterateParameters(ifLine);
		var thenPos = InStr(obscuredLine, "then");
		// UNKNOWN MultiLineIfBlock
		var conditions = Trim(Left(ifLine, thenPos - 1));
		thenPos = thenPos + 4;
		var elsePos = InStr(obscuredLine, "else");
		var thenEndPos: number;
		// UNKNOWN MultiLineIfBlock
		var thenScript = Trim(Mid(ifLine, thenPos, thenEndPos - thenPos));
		var elseScript = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteScript(scriptLine: string, ctx: Context, newCallingObjectId: number = 0): void {
		// UNKNOWN TryBlock
	}
	ExecuteEnter(scriptLine: string, ctx: Context): void {
		this._commandOverrideModeOn = true;
		this._commandOverrideVariable = this.GetParameter(scriptLine, ctx);
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		this._commandOverrideModeOn = false;
	}
	ExecuteSet(setInstruction: string, ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		var r = this._rooms[id];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	GoToPlace(place: string, ctx: Context): void {
		var destination = "";
		var placeData: string;
		var disallowed = false;
		placeData = this.PlaceExist(place, ctx);
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		var gameBlock: DefineBlock;
		gameBlock = this.GetDefineBlock("game");
		var aslVersion = "//";
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		this._gameName = this.GetParameter(this._lines[this.GetDefineBlock("game").StartLine], this._nullContext);
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
		this._gameFileName = filename;
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN MultiLineIfBlock
	}
	PlayGame(room: string, ctx: Context): void {
		var id = this.GetRoomID(room, ctx);
		// UNKNOWN MultiLineIfBlock
		this._currentRoom = room;
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	Print(txt: string, ctx: Context): void {
		var printString = "";
		// UNKNOWN MultiLineIfBlock
	}
	RetrLine(blockType: string, param: string, line: string, ctx: Context): string {
		var searchblock: DefineBlock;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		return "<unfound>";
	}
	RetrLineParam(blockType: string, param: string, line: string, lineParam: string, ctx: Context): string {
		var searchblock: DefineBlock;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
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
		this._compassExits = compassExits;
		// UNKNOWN ExpressionStatement
	}
	AddCompassExit(exitList: any, name: string): void {
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		return roomDisplayText;
	}
	UpdateItems(ctx: Context): void {
		var invList: any = {};
		// UNKNOWN SingleLineIfStatement
		var name: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN RaiseEventStatement
		// UNKNOWN MultiLineIfBlock
	}
	FinishGame(stopType: StopType, ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN MultiLineIfBlock
		noFormatObjsViewable = "";
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var roomId: number;
		roomId = this.GetRoomID(this._currentRoom, ctx);
		var r = this._rooms[roomId];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN RaiseEventStatement
		this._gotoExits = exitList;
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	UpdateVisibilityInContainers(ctx: Context, onlyParent: string = ""): void {
		var parentId: number;
		var parent: string;
		var parentIsTransparent: boolean;
		var parentIsOpen: boolean;
		var parentIsSeen: boolean;
		var parentIsSurface: boolean;
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
	}
	PlayerCanAccessObject(id: number, colObjects: any = null): PlayerCanAccessObjectResult {
		var parent: string;
		var parentId: number;
		var parentDisplayName: string;
		var result: PlayerCanAccessObjectResult = new PlayerCanAccessObjectResult();
		var hierarchy: string = "";
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		var room = Trim(params[0]);
		var exitName = Trim(params[1]);
		var roomId = this.GetRoomID(room, this._nullContext);
		// UNKNOWN MultiLineIfBlock
		var exits = this._rooms[roomId].Exits;
		var dir = exits.GetDirectionEnum(exitName);
		// UNKNOWN MultiLineIfBlock
		return null;
	}
	ExecuteLock(tag: string, lock: boolean): void {
		var roomExit: RoomExit;
		roomExit = this.FindExit(tag);
		// UNKNOWN MultiLineIfBlock
		roomExit.IsLocked = lock;
	}
	Begin(): void {
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
	}
	DoBegin(): void {
		var gameBlock: DefineBlock = this.GetDefineBlock("game");
		var ctx: Context = new Context();
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		this._autoIntro = true;
		// UNKNOWN MultiLineIfBlock
		this._gameFullyLoaded = true;
		// UNKNOWN SingleLineIfStatement
		var startRoom: string = "";
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	// UNKNOWN PropertyBlock
	// UNKNOWN PropertyBlock
	Finish(): void {
		// UNKNOWN ExpressionStatement
	}
	// UNKNOWN EventStatement
	// UNKNOWN EventStatement
	// UNKNOWN EventStatement
	Save(filename: string, html: string): void {
		// UNKNOWN ExpressionStatement
	}
	Save(html: string): number[] {
		return this.SaveGame(Filename, false);
	}
	// UNKNOWN PropertyBlock
	SendCommand(command: string): void {
		// UNKNOWN ExpressionStatement
	}
	SendCommand(command: string, metadata: any): void {
		// UNKNOWN ExpressionStatement
	}
	SendCommand(command: string, elapsedTime: number, metadata: any): void {
		// UNKNOWN SingleLineIfStatement
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
	}
	GameFinished(): void {
		this._gameFinished = true;
		// UNKNOWN RaiseEventStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		// UNKNOWN SyncLockBlock
		// UNKNOWN SyncLockBlock
		// UNKNOWN ExpressionStatement
	}
	GetResourcePath(filename: string): string {
		// UNKNOWN MultiLineIfBlock
		return System.IO.Path.Combine(this._gamePath, filename);
	}
	Cleanup(): void {
		// UNKNOWN ExpressionStatement
	}
	DeleteDirectory(dir: string): void {
		// UNKNOWN MultiLineIfBlock
	}
	Finalize(): void {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	RunTimersInNewThread(scripts: Object): void {
		var scriptList: any = scripts;
		// UNKNOWN ForEachBlock
		// UNKNOWN ExpressionStatement
	}
	RaiseNextTimerTickRequest(): void {
		var anyTimerActive: boolean = false;
		var nextTrigger: number = 60;
		// UNKNOWN ForBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN RaiseEventStatement
	}
	ChangeState(newState: State): void {
		var acceptCommands: boolean = (newState == State.Ready);
		// UNKNOWN ExpressionStatement
	}
	ChangeState(newState: State, acceptCommands: boolean): void {
		this._readyForCommand = acceptCommands;
		// UNKNOWN SyncLockBlock
	}
	FinishWait(): void {
		// UNKNOWN SingleLineIfStatement
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	FinishWaitInNewThread(): void {
		// UNKNOWN SyncLockBlock
	}
	FinishPause(): void {
		// UNKNOWN ExpressionStatement
	}
	m_menuResponse: string;
	ShowMenu(menuData: MenuData): string {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		return this.m_menuResponse;
	}
	SetMenuResponse(response: string): void {
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
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
		// UNKNOWN MultiLineIfBlock
		var path: string = this.GetResourcePath(file);
		// UNKNOWN SingleLineIfStatement
		return new any();
	}
	m_gameId: string;
	// UNKNOWN PropertyBlock
	GetResources(): any {
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
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
	// UNKNOWN ConstructorBlock
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
		// UNKNOWN ExpressionStatement
	}
	RunScript(ctx: Context): void {
		// UNKNOWN ExpressionStatement
	}
	UpdateObjectName(): void {
		var objName: string;
		var lastExitId: number;
		var parentRoom: string;
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		parentRoom = this._game._objs[this._parent.ObjId].ObjectName;
		objName = parentRoom;
		// UNKNOWN MultiLineIfBlock
		this._game._objs[this._objId].ObjectName = objName;
		this._game._objs[this._objId].ContainerRoom = parentRoom;
		this._objName = objName;
	}
	Go(ctx: Context): void {
		// UNKNOWN MultiLineIfBlock
	}
}
class RoomExits {
	_directions: any = {};
	_places: any = {};
	_objId: number;
	_allExits: any;
	_regenerateAllExits: boolean;
	_game: LegacyGame;
	// UNKNOWN ConstructorBlock
	SetDirection(direction: Direction, roomExit: RoomExit): void {
		// UNKNOWN MultiLineIfBlock
		this._regenerateAllExits = true;
	}
	GetDirectionExit(direction: Direction): RoomExit {
		// UNKNOWN MultiLineIfBlock
		return null;
	}
	AddPlaceExit(roomExit: RoomExit): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		this._regenerateAllExits = true;
	}
	AddExitFromTag(tag: string): void {
		var thisDir: Direction;
		var roomExit: RoomExit = null;
		var params: string[] = [];
		var afterParam: string;
		var param: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		roomExit.Parent = this;
		roomExit.Direction = thisDir;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
	}
	// UNKNOWN PropertyBlock
	// UNKNOWN PropertyBlock
	ExecuteGo(cmd: string, ctx: Context): void {
		var lExitID: number;
		var oExit: RoomExit;
		// UNKNOWN MultiLineIfBlock
		lExitID = this._game.Disambiguate(cmd, this._game._currentRoom, ctx, true);
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		var sDisplay = "|b" + roomExit.DisplayName + "|xb";
		// UNKNOWN MultiLineIfBlock
		return "to " + sDisplay;
	}
	GetExitByObjectId(id: number): RoomExit {
		// UNKNOWN ForEachBlock
		return null;
	}
	AllExits(): any {
		// UNKNOWN MultiLineIfBlock
		this._allExits = new any();
		// UNKNOWN ForEachBlock
		// UNKNOWN ForEachBlock
		return this._allExits;
	}
	RemoveExit(roomExit: RoomExit): void {
		// UNKNOWN MultiLineIfBlock
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
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN DoLoopUntilBlock
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
