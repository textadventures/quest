enum State {Ready, Working, Waiting, Finished};
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
		Data: Byte[];
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
class LegacyGame {
	CopyContext(): Context {
		var result: Context = new Context();
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
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
	RemoveFormatting(): string {
		var code: string;
		var pos: number;
		var len: number;
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN ReturnStatement
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
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
		// UNKNOWN ReturnStatement
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
		// UNKNOWN SimpleAssignmentStatement
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	GetAfterParameter(): string {
		var eop: number;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
	}
	ObliterateParameters(): string {
		var inParameter: boolean;
		var exitCharacter: string = "";
		var curChar: string;
		var outputLine: string = "";
		var obscuringFunctionName: boolean;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
	}
	ObliterateVariableNames(): string {
		var inParameter: boolean;
		var exitCharacter: string = "";
		var outputLine: string = "";
		var curChar: string;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	RemoveComments(): void {
		var aposPos: number;
		var inTextBlock: boolean;
		var inSynonymsBlock: boolean;
		var oblitLine: string;
		// UNKNOWN ForBlock
	}
	ReportErrorLine(): string {
		var replaceFrom: number;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
	}
	YesNo(): string {
		// UNKNOWN SingleLineIfStatement
	}
	IsYes(): boolean {
		// UNKNOWN SingleLineIfStatement
	}
	BeginsWith(): boolean {
		// UNKNOWN ReturnStatement
	}
	ConvertCasKeyword(): string {
		var c: Byte = 'expr';
		var keyword: string = this._casKeywords[c];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	ConvertMultiLines(): void {
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
	}
	GetDefineBlock(): DefineBlock {
		var l: string;
		var blockType: string;
		var result = new DefineBlock();
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	DefineBlockParam(): DefineBlock {
		var cache: any;
		var result = new DefineBlock();
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetEverythingAfter(): string {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	Keyword2CAS(): string {
		var k = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	LoadCASKeywords(): void {
		var questDatLines: string[] = 'expr';
		// UNKNOWN ForEachBlock
	}
	GetResourceLines(): string[] {
		var enc: any = {};
		var resFile: string = 'expr';
		// UNKNOWN ReturnStatement
	}
	ParseFile(): boolean {
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN SimpleAssignmentStatement
		var lastSlashPos: number;
		var slashPos: number;
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
	}
	LogASLError(): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	GetParameter(): string {
		var newParam: string;
		var startPos: number;
		var endPos: number;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		var retrParam = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	AddLine(): void {
		var numLines: number;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReDimPreserveStatement
		// UNKNOWN SimpleAssignmentStatement
	}
	LoadCASFile(): void {
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
	}
	DecryptString(): string {
		var output = "";
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	RemoveTabs(): string {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	DoAddRemove(): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	DoLook(): void {
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
	DoOpenClose(): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	EvaluateInlineExpressions(): string {
		// UNKNOWN MultiLineIfBlock
		var bracePos: number;
		var curPos = 1;
		var resultLine = "";
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN ReturnStatement
	}
	ExecAddRemove(): void {
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
	}
	ExecAddRemoveScript(): void {
		var childId: number;
		var parentId: number;
		var commandName: string;
		var childName: string;
		var parentName: string = "";
		var scp: number;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecOpenClose(): void {
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
	}
	ExecuteSelectCase(): void {
		var afterLine = 'expr';
		// UNKNOWN MultiLineIfBlock
		var blockName = 'expr';
		var block = 'expr';
		var checkValue = 'expr';
		var caseMatch = false;
		// UNKNOWN ForBlock
	}
	ExecVerb(): boolean {
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	ExpressionHandler(): ExpressionResult {
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
		var obscuredExpr = 'expr';
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		var opNum = 0;
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	ListContents(): string {
		var contentsIDs: number[] = [];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var numContents = 0;
		// UNKNOWN ForBlock
		var contents = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ObscureNumericExps(): string {
		var EPos: number;
		var CurPos: number;
		var OutputString: string;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN ReturnStatement
	}
	ProcessListInfo(): void {
		var listInfo: TextAction = new TextAction();
		var propName: string = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	GetHTMLColour(): string {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SelectBlock
	}
	DoPrint(): void {
		// UNKNOWN RaiseEventStatement
	}
	DestroyExit(): void {
		var fromRoom: string = "";
		var toRoom: string = "";
		var roomId: number;
		var exitId: number;
		var scp = 'expr';
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
	ExecuteFlag(): void {
		var propertyString: string = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ExecuteIfFlag(): boolean {
		// UNKNOWN ReturnStatement
	}
	ExecuteIncDec(): void {
		var variable: string;
		var change: number;
		var param = 'expr';
		var sc = 'expr';
		// UNKNOWN MultiLineIfBlock
		var value = 'expr';
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		var arrayIndex = 'expr';
		// UNKNOWN ExpressionStatement
	}
	ExtractFile(): string {
		var length: number;
		var startPos: number;
		var extracted: boolean;
		var resId: number;
		// UNKNOWN SingleLineIfStatement
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var fileName = 'expr';
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	AddObjectAction(): void {
		var actionNum: number;
		var foundExisting = false;
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
	}
	AddToChangeLog(): void {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReDimPreserveStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
	}
	AddToObjectChangeLog(): void {
		var changeLog: ChangeLog;
		// UNKNOWN SelectBlock
		// UNKNOWN ExpressionStatement
	}
	AddToGiveInfo(): void {
		var giveType: GiveType;
		var actionName: string;
		var actionScript: string;
		var o = this._objs[id];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	AddToObjectActions(): void {
		var actionNum: number;
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
	}
	AddToObjectAltNames(): void {
		var o = this._objs[id];
		// UNKNOWN DoLoopUntilBlock
	}
	AddToObjectProperties(): void {
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN DoLoopUntilBlock
	}
	AddToUseInfo(): void {
		var useType: UseType;
		var o = this._objs[id];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	CapFirst(): string {
		// UNKNOWN ReturnStatement
	}
	ConvertVarsIn(): string {
		// UNKNOWN ReturnStatement
	}
	DisambObjHere(): boolean {
		var isSeen: boolean;
		var onlySeen = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	ExecClone(): void {
		var id: number;
		var newName: string;
		var cloneTo: string;
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
	}
	ExecOops(): void {
		// UNKNOWN MultiLineIfBlock
	}
	ExecType(): void {
		var id: number;
		var found: boolean;
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
	}
	ExecuteIfAction(): boolean {
		var id: number;
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
	}
	ExecuteIfType(): boolean {
		var id: number;
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
	}
	GetArrayIndex(): ArrayResult {
		var result: ArrayResult = new ArrayResult();
		// UNKNOWN MultiLineIfBlock
		var beginPos = 'expr';
		var endPos = 'expr';
		var data = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	Disambiguate(): number {
		var numberCorresIds = 0;
		var idNumbers: number[] = [];
		var firstPlace: string;
		var secondPlace: string = "";
		var twoPlaces: boolean;
		var descriptionText: string[];
		var validNames: string[];
		var numValidNames: number;
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
	}
	DisplayStatusVariableInfo(): string {
		var displayData: string = "";
		var ep: number;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	DoAction(): boolean {
		var FoundAction: boolean;
		var ActionScript: string = "";
		var o = this._objs[ObjID];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var NewThread: Context = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ReturnStatement
	}
	HasAction(): boolean {
		var o = this._objs[ObjID];
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	ExecForEach(): void {
		var inLocation: string;
		var scriptToRun: string;
		var isExit: boolean;
		var isRoom: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
	}
	ExecuteAction(): void {
		var actionName: string;
		var script: string;
		var actionNum: number;
		var id: number;
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
	}
	ExecuteCondition(): boolean {
		var result: boolean;
		var thisNot: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	ExecuteConditions(): boolean {
		var conditions: string[];
		var numConditions = 0;
		var operations: string[];
		var obscuredConditionList = 'expr';
		var pos = 1;
		var isFinalCondition = false;
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN SimpleAssignmentStatement
		var result = true;
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	ExecuteCreate(): void {
		var newName: string;
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteCreateExit(): void {
		var scrRoom: string;
		var destRoom: string = "";
		var destId: number;
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
		var saveData: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		var r = this._rooms[srcId];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecDrop(): void {
		var found: boolean;
		var parentId: number;
		var id: number;
		// UNKNOWN SimpleAssignmentStatement
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
	ExecExamine(): void {
		var item = 'expr';
		// UNKNOWN MultiLineIfBlock
		var id = 'expr';
		// UNKNOWN MultiLineIfBlock
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
	}
	ExecMoveThing(): void {
		var scp = 'expr';
		var name = 'expr';
		var place = 'expr';
		// UNKNOWN ExpressionStatement
	}
	ExecProperty(): void {
		var id: number;
		var found: boolean;
		var scp = 'expr';
		// UNKNOWN MultiLineIfBlock
		var name = 'expr';
		var properties = 'expr';
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ExecuteDo(): void {
		var newCtx: Context = 'expr';
		var numParameters = 0;
		var useNewCtx: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var block = 'expr';
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteDoAction(): void {
		var id: number;
		var scp = 'expr';
		// UNKNOWN MultiLineIfBlock
		var objName = 'expr';
		var actionName = 'expr';
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ExecuteIfHere(): boolean {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	ExecuteIfExists(): boolean {
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
		// UNKNOWN ReturnStatement
	}
	ExecuteIfProperty(): boolean {
		var id: number;
		var scp = 'expr';
		// UNKNOWN MultiLineIfBlock
		var objName = 'expr';
		var propertyName = 'expr';
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	ExecuteRepeat(): void {
		var repeatWhileTrue: boolean;
		var repeatScript: string = "";
		var bracketPos: number;
		var afterBracket: string;
		var foundScript = false;
		// UNKNOWN MultiLineIfBlock
		var pos = 1;
		// UNKNOWN DoLoopUntilBlock
		var conditions = 'expr';
		var finished = false;
		// UNKNOWN DoLoopUntilBlock
	}
	ExecuteSetCollectable(): void {
		var val: number;
		var id: number;
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
	}
	ExecuteWait(): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	InitFileData(): void {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
	}
	GetNextChunk(): string {
		var nullPos = 'expr';
		var result = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetFileDataChars(): string {
		var result = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	GetObjectActions(): ActionType {
		var name = 'expr';
		var ep = 'expr';
		// UNKNOWN MultiLineIfBlock
		var script = 'expr';
		var result: ActionType = new ActionType();
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	GetObjectId(): number {
		var id: number;
		var found = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetObjectIdNoAlias(): number {
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	GetObjectProperty(): string {
		var result: string = "";
		var found = false;
		var o = this._objs[id];
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetPropertiesInType(): PropertiesActions {
		var blockId: number;
		var propertyList: PropertiesActions = new PropertiesActions();
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	GetRoomID(): number {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	GetTextOrScript(): TextAction {
		var result = new TextAction();
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetThingNumber(): number {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetThingBlock(): DefineBlock {
		var result = new DefineBlock();
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	MoveThing(): void {
		var oldRoom: string = "";
		var id = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
	}
	Pause(): void {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
	}
	ConvertParameter(): string {
		var result: string = "";
		var pos = 1;
		var finished = false;
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN ReturnStatement
	}
	DoFunction(): string {
		var name: string;
		var parameter: string;
		var intFuncResult: string = "";
		var intFuncExecuted = false;
		var paramPos = 'expr';
		// UNKNOWN MultiLineIfBlock
		var block: DefineBlock;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	DoInternalFunction(): string {
		var parameters: string[];
		var untrimmedParameters: string[];
		var objId: number;
		var numParameters = 0;
		var pos = 1;
		// UNKNOWN MultiLineIfBlock
		var param2: string;
		var param3: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	ExecFor(): void {
		// UNKNOWN MultiLineIfBlock
		var endValue: number;
		var stepValue: number;
		var forData = 'expr';
		var scp1 = 'expr';
		var scp2 = 'expr';
		var scp3 = 'expr';
		var counterVariable = 'expr';
		var startValue = 'expr';
		// UNKNOWN MultiLineIfBlock
		var loopScript = 'expr';
		// UNKNOWN ForBlock
	}
	ExecSetVar(): void {
		var scp = 'expr';
		var varName = 'expr';
		var varCont = 'expr';
		var idx = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN TryBlock
	}
	ExecuteIfAsk(): boolean {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		// UNKNOWN ReturnStatement
	}
	SetQuestionResponse(): void {
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	SetQuestionResponseInNewThread(): void {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SyncLockBlock
	}
	ExecuteIfGot(): boolean {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ReturnStatement
	}
	ExecuteIfHas(): boolean {
		var checkValue: number;
		var colNum: number;
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
	}
	ExecuteIfIs(): boolean {
		var value1: string;
		var value2: string;
		var op: string;
		var expectNumerics: boolean;
		var expResult: ExpressionResult;
		var scp = 'expr';
		// UNKNOWN MultiLineIfBlock
		var scp2 = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var result = false;
		// UNKNOWN SelectBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetNumericContents(): number {
		var numNumber: number;
		var arrayIndex: number;
		var exists = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	PlayerErrorMessage(): void {
		// UNKNOWN ExpressionStatement
	}
	PlayerErrorMessage_ExtendInfo(): void {
		var message = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	GetErrorMessage(): string {
		// UNKNOWN ReturnStatement
	}
	PlayMedia(): void {
		// UNKNOWN ExpressionStatement
	}
	PlayMedia(): void {
		// UNKNOWN MultiLineIfBlock
	}
	PlayWav(): void {
		var sync: boolean = false;
		var looped: boolean = false;
		var params: any = {};
		// UNKNOWN SimpleAssignmentStatement
		var filename = 'expr';
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	RestoreGameData(): void {
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		var numData = 'expr';
		var createdObjects: any = {};
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
	}
	SetBackground(): void {
		// UNKNOWN ExpressionStatement
	}
	SetForeground(): void {
		// UNKNOWN ExpressionStatement
	}
	SetDefaultPlayerErrorMessages(): void {
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
	}
	SetFont(): void {
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ExpressionStatement
	}
	SetFontSize(): void {
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ExpressionStatement
	}
	SetNumericVariableContents(): void {
		var numNumber: number;
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
	}
	SetOpenClose(): void {
		var cmd: string;
		// UNKNOWN MultiLineIfBlock
		var id = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	SetTimerState(): void {
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
	}
	SetUnknownVariableType(): SetResult {
		var scp = 'expr';
		// UNKNOWN MultiLineIfBlock
		var name = 'expr';
		// UNKNOWN MultiLineIfBlock
		var contents = 'expr';
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	SetUpChoiceForm(): string {
		var block = 'expr';
		var prompt = 'expr';
		var menuOptions: any = {};
		var menuScript: any = {};
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
		var mnu: MenuData = new MenuData();
		var choice: string = 'expr';
		// UNKNOWN ExpressionStatement
		// UNKNOWN ReturnStatement
	}
	SetUpDefaultFonts(): void {
		var gameblock = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
	}
	SetUpDisplayVariables(): void {
		// UNKNOWN ForBlock
	}
	SetUpGameObject(): void {
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
		var block = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
	}
	SetUpTimers(): void {
		// UNKNOWN ForBlock
	}
	SetUpTurnScript(): void {
		var block = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
	}
	SetUpUserDefinedPlayerErrors(): void {
		var block = 'expr';
		var examineIsCustomised = false;
		// UNKNOWN ForBlock
	}
	SetVisibility(): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	ShowPictureInText(): void {
		// UNKNOWN MultiLineIfBlock
	}
	ShowRoomInfoV2(): void {
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		var roomBlock: DefineBlock;
		// UNKNOWN SimpleAssignmentStatement
		var finishedFindingCommas: boolean;
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
		var finishedLoop: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		var outside: DefineBlock;
		// UNKNOWN MultiLineIfBlock
		var finished: boolean;
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
	}
	Speak(): void {
		// UNKNOWN ExpressionStatement
	}
	AddToObjectList(): void {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
	}
	ExecExec(): void {
		// UNKNOWN SingleLineIfStatement
		var execLine = 'expr';
		var newCtx: Context = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecSetString(): void {
		var scp = 'expr';
		var name = 'expr';
		var value = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var idx = 'expr';
		// UNKNOWN ExpressionStatement
	}
	ExecUserCommand(): boolean {
		var curCmd: string;
		var commandList: string;
		var script: string = "";
		var commandTag: string;
		var commandLine: string = "";
		var foundCommand = false;
		var roomId = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	ExecuteChoose(): void {
		// UNKNOWN ExpressionStatement
	}
	GetCommandParameters(): boolean {
		var chunksBegin: number[];
		var chunksEnd: number[];
		var varName: string[];
		var var2Pos: number;
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
	}
	GetGender(): string {
		var result: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	GetStringContents(): string {
		var returnAlias = false;
		var arrayIndex = 0;
		var cp = 'expr';
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
	IsAvailable(): boolean {
		var room: string;
		var name: string;
		var atPos = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	IsCompatible(): boolean {
		var var2Pos: number;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		var currentReqLinePos = 1;
		var currentTestLinePos = 1;
		var finished = false;
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN ReturnStatement
	}
	OpenGame(): boolean {
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
		var lines: string[] = 'expr';
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
	}
	SaveGame(): Byte[] {
		var ctx: Context = new Context();
		var saveData: string;
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
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
		// UNKNOWN ReturnStatement
	}
	SetAvailability(): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	SetStringContents(): void {
		var id: number;
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
	}
	SetUpCharObjectInfo(): void {
		var defaultProperties: PropertiesActions = new PropertiesActions();
		// UNKNOWN SimpleAssignmentStatement
		var defaultExists = false;
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
	}
	ShowGameAbout(): void {
		var version = 'expr';
		var author = 'expr';
		var copyright = 'expr';
		var info = 'expr';
		// UNKNOWN ExpressionStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
	}
	ShowPicture(): void {
		var caption: string = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ExpressionStatement
	}
	ShowRoomInfo(): void {
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
		var visibleObjectsList: any = {};
		var count: number;
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
	}
	CheckCollectable(): void {
		var max: number;
		var value: number;
		var min: number;
		var m: number;
		var type = this._collectables[id].Type;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
	}
	DisplayCollectableInfo(): string {
		var display: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	DisplayTextSection(): void {
		var block: DefineBlock;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
	}
	ExecCommand(): boolean {
		var parameter: string;
		var skipAfterTurn = false;
		// UNKNOWN SimpleAssignmentStatement
		var oldBadCmdBefore = this._badCmdBefore;
		var roomID = 'expr';
		var enteredHelpCommand = false;
		// UNKNOWN SingleLineIfStatement
		var cmd = 'expr';
		// UNKNOWN SyncLockBlock
		var userCommandReturn: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		var newCommand = " " + input + " ";
		// UNKNOWN ForBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		var newCtx: Context = 'expr';
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
	}
	CmdStartsWith(): boolean {
		// UNKNOWN ReturnStatement
	}
	ExecGive(): void {
		var article: string;
		var item: string;
		var character: string;
		var type: Thing;
		var id: number;
		var script: string = "";
		var toPos = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecLook(): void {
		var item: string;
		// UNKNOWN MultiLineIfBlock
	}
	ExecSpeak(): void {
		// UNKNOWN MultiLineIfBlock
		var name = cmd;
		// UNKNOWN MultiLineIfBlock
	}
	ExecTake(): void {
		var parentID: number;
		var parentDisplayName: string;
		var foundItem = true;
		var foundTake = false;
		var id = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		var isInContainer = false;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecUse(): void {
		var endOnWith: number;
		var useDeclareLine = "";
		var useOn: string;
		var useItem: string;
		// UNKNOWN SimpleAssignmentStatement
		var roomId: number;
		// UNKNOWN SimpleAssignmentStatement
		var onWithPos = 'expr';
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
	ObjectActionUpdate(): void {
		var objectName: string;
		var sp: number;
		var ep: number;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteIf(): void {
		var ifLine = 'expr';
		var obscuredLine = 'expr';
		var thenPos = 'expr';
		// UNKNOWN MultiLineIfBlock
		var conditions = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		var elsePos = 'expr';
		var thenEndPos: number;
		// UNKNOWN MultiLineIfBlock
		var thenScript = 'expr';
		var elseScript = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ExecuteScript(): void {
		// UNKNOWN TryBlock
	}
	ExecuteEnter(): void {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		// UNKNOWN SimpleAssignmentStatement
	}
	ExecuteSet(): void {
		// UNKNOWN MultiLineIfBlock
	}
	FindStatement(): string {
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	FindLine(): string {
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	GetCollectableAmount(): number {
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	GetSecondChunk(): string {
		var endOfFirstBit = 'expr' + 1;
		var lengthOfKeyword = 'expr' + 1;
		// UNKNOWN ReturnStatement
	}
	GoDirection(): void {
		var dirData: TextAction = new TextAction();
		var id = 'expr';
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		var r = this._rooms[id];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	GoToPlace(): void {
		var destination = "";
		var placeData: string;
		var disallowed = false;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	InitialiseGame(): boolean {
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
		var gameBlock: DefineBlock;
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
	}
	PlaceExist(): string {
		var roomId = 'expr';
		var foundPlace = false;
		var scriptPresent = false;
		var r = this._rooms[roomId];
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	PlayerItem(): void {
		var foundObjectName = false;
		// UNKNOWN MultiLineIfBlock
	}
	PlayGame(): void {
		var id = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	Print(): void {
		var printString = "";
		// UNKNOWN MultiLineIfBlock
	}
	RetrLine(): string {
		var searchblock: DefineBlock;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	RetrLineParam(): string {
		var searchblock: DefineBlock;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	SetUpCollectables(): void {
		var lastItem = false;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
	}
	SetUpItemArrays(): void {
		var lastItem = false;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
	}
	SetUpStartItems(): void {
		var lastItem = false;
		// UNKNOWN ForBlock
	}
	ShowHelp(): void {
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
	ReadCatalog(): void {
		var nullPos = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReDimPreserveStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		var resourceStart = 0;
		// UNKNOWN ForBlock
	}
	UpdateDirButtons(): void {
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
	}
	AddCompassExit(): void {
		// UNKNOWN ExpressionStatement
	}
	UpdateDoorways(): string {
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
		// UNKNOWN ReturnStatement
	}
	UpdateItems(): void {
		var invList: any = {};
		// UNKNOWN SingleLineIfStatement
		var name: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN RaiseEventStatement
		// UNKNOWN MultiLineIfBlock
	}
	FinishGame(): void {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	UpdateObjectList(): void {
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
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var roomId: number;
		// UNKNOWN SimpleAssignmentStatement
		var r = this._rooms[roomId];
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN RaiseEventStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
	}
	UpdateExitsList(): void {
		var mergedList: any = {};
		// UNKNOWN ForEachBlock
		// UNKNOWN ForEachBlock
		// UNKNOWN RaiseEventStatement
	}
	UpdateStatusVars(): void {
		var displayData: string;
		var status: string = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	UpdateVisibilityInContainers(): void {
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
	PlayerCanAccessObject(): PlayerCanAccessObjectResult {
		var parent: string;
		var parentId: number;
		var parentDisplayName: string;
		var result: PlayerCanAccessObjectResult = new PlayerCanAccessObjectResult();
		var hierarchy: string = "";
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	GetGoToExits(): string {
		var placeList: string = "";
		var shownPlaceName: string;
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	SetUpExits(): void {
		// UNKNOWN ForBlock
		// UNKNOWN ExitSubStatement
	}
	FindExit(): RoomExit {
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
	}
	ExecuteLock(): void {
		var roomExit: RoomExit;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
	}
	Begin(): void {
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
	}
	DoBegin(): void {
		var gameBlock: DefineBlock = 'expr';
		var ctx: Context = new Context();
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
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
	Save(): void {
		// UNKNOWN ExpressionStatement
	}
	Save(): Byte[] {
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN PropertyBlock
	SendCommand(): void {
		// UNKNOWN ExpressionStatement
	}
	SendCommand(): void {
		// UNKNOWN ExpressionStatement
	}
	SendCommand(): void {
		// UNKNOWN SingleLineIfStatement
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN MultiLineIfBlock
	}
	WaitForStateChange(): void {
		// UNKNOWN SyncLockBlock
	}
	ProcessCommandInNewThread(): void {
		// UNKNOWN TryBlock
	}
	SendEvent(): void {
	}
	// UNKNOWN EventStatement
	Initialise(): boolean {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
	}
	GameFinished(): void {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN RaiseEventStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		// UNKNOWN SyncLockBlock
		// UNKNOWN SyncLockBlock
		// UNKNOWN ExpressionStatement
	}
	GetResourcePath(): string {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	Cleanup(): void {
		// UNKNOWN ExpressionStatement
	}
	DeleteDirectory(): void {
		// UNKNOWN MultiLineIfBlock
	}
	Finalize(): void {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	GetLibraryLines(): string[] {
		var libCode: Byte[] = null;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SelectBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN PropertyBlock
	Tick(): void {
		var i: number;
		var timerScripts: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ExpressionStatement
	}
	RunTimersInNewThread(): void {
		var scriptList: any = 'expr';
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
	ChangeState(): void {
		var acceptCommands: boolean = 'expr';
		// UNKNOWN ExpressionStatement
	}
	ChangeState(): void {
		// UNKNOWN SimpleAssignmentStatement
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
	ShowMenu(): string {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		// UNKNOWN ReturnStatement
	}
	SetMenuResponse(): void {
		var runnerThread: any = {};
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
	}
	SetMenuResponseInNewThread(): void {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SyncLockBlock
	}
	LogException(): void {
		// UNKNOWN RaiseEventStatement
	}
	GetExternalScripts(): any {
		// UNKNOWN ReturnStatement
	}
	GetExternalStylesheets(): any {
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN EventStatement
	// UNKNOWN PropertyBlock
	GetOriginalFilenameForQSG(): string {
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN DelegateFunctionStatement
	m_unzipFunction: UnzipFunctionDelegate;
	SetUnzipFunction(): void {
		// UNKNOWN SimpleAssignmentStatement
	}
	GetUnzippedFile(): string {
		var tempDir: string = null;
		var result: string = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN PropertyBlock
	// UNKNOWN PropertyBlock
	GetResource(): any {
		// UNKNOWN MultiLineIfBlock
		var path: string = 'expr';
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	m_gameId: string;
	// UNKNOWN PropertyBlock
	GetResources(): any {
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
	}
	GetResourcelessCAS(): Byte[] {
		var fileData: string = 'expr';
		// UNKNOWN ReturnStatement
	}
}
