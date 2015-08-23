enum State {Ready, Working, Waiting, Finished};
class DefineBlock {
		StartLine: number;
		EndLine: number;
}
class Context {
		CallingObjectId: number;
		NumParameters: number;
		Parameters: string;
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
		Actions: ActionType;
		NumberTypesIncluded: number;
		TypesIncluded: string;
}
class VariableType {
		VariableName: string;
		VariableContents: string;
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
		Commands: UserDefinedCommandType;
		NumberCommands: number;
		Description: any;
		Out: any;
		East: any;
		West: any;
		North: any;
		South: any;
		NorthEast: any;
		NorthWest: any;
		SouthEast: any;
		SouthWest: any;
		Up: any;
		Down: any;
		InDescription: string;
		Look: string;
		Places: PlaceType;
		NumberPlaces: number;
		Prefix: string;
		Script: string;
		Use: ScriptText;
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
		Properties: PropertyType;
		Speak: any;
		Take: any;
		IsRoom: boolean;
		IsExit: boolean;
		CorresRoom: string;
		CorresRoomId: number;
		Loaded: boolean;
		NumberActions: number;
		Actions: ActionType;
		NumberUseData: number;
		UseData: UseDataType;
		UseAnything: string;
		UseOnAnything: string;
		Use: string;
		NumberGiveData: number;
		GiveData: GiveDataType;
		GiveAnything: string;
		GiveToAnything: string;
		DisplayType: string;
		NumberTypesIncluded: number;
		TypesIncluded: string;
		NumberAltNames: number;
		AltNames: string;
		AddScript: any;
		RemoveScript: any;
		OpenScript: any;
		CloseScript: any;
}
class ChangeType {
		AppliesTo: string;
		Change: string;
}
class GameChangeDataType {
		NumberChanges: number;
		ChangeData: ChangeType;
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
		Data: any;
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
	_casKeywords: string;
	_lines: string;
	_defineBlocks: DefineBlock;
	_numberSections: number;
	_gameName: string;
	_nullContext: any;
	_changeLogRooms: ChangeLog;
	_changeLogObjects: ChangeLog;
	_defaultProperties: PropertiesActions;
	_defaultRoomProperties: PropertiesActions;
	_rooms: RoomType;
	_numberRooms: number;
	_numericVariable: VariableType;
	_numberNumericVariables: number;
	_stringVariable: VariableType;
	_numberStringVariables: number;
	_synonyms: SynonymType;
	_numberSynonyms: number;
	_items: ItemType;
	_chars: ObjectType;
	_objs: ObjectType;
	_numberChars: number;
	_numberObjs: number;
	_numberItems: number;
	_currentRoom: string;
	_collectables: Collectable;
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
	_timers: TimerType;
	_numberTimers: number;
	_numDisplayStrings: number;
	_numDisplayNumerics: number;
	_gameFullyLoaded: boolean;
	_gameChangeData: any;
	_lastIt: number;
	_lastItMode: ItType;
	_thisTurnIt: number;
	_thisTurnItMode: ItType;
	_badCmdBefore: string;
	_badCmdAfter: string;
	_numResources: number;
	_resources: ResourceType;
	_resourceFile: string;
	_resourceOffset: number;
	_startCatPos: number;
	_useAbbreviations: boolean;
	_loadedFromQsg: boolean;
	_beforeSaveScript: string;
	_onLoadScript: string;
	_numSkipCheckFiles: number;
	_skipCheckFile: string;
	_compassExits: any;
	_gotoExits: any;
	_textFormatter: any;
	_log: any;
	_casFileData: string;
	_commandLock: Object = new Object();
	_stateLock: Object = new Object();
	_state: State = State.Ready;
	_waitLock: Object = new Object();
	_readyForCommand: boolean = true;
	_gameLoading: boolean;
	_random: any;
	_tempFolder: string;
	_playerErrorMessageString: string;
	_listVerbs: any;
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
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
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
		var keyword: string = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	GetResourceLines(): any {
		var enc: any;
		var resFile: string = 'expr';
		// UNKNOWN ReturnStatement
	}
	ParseFile(): boolean {
		var hasErrors: boolean;
		var result: boolean;
		var libCode: string;
		var libLines: number;
		var ignoreMode: boolean;
		var skipCheck: boolean;
		var c: number;
		var d: number;
		var l: number;
		var libFileHandle: number;
		var libResourceLines: any;
		var libFile: string;
		var libLine: string;
		var inDefGameBlock: number;
		var gameLine: number;
		var inDefSynBlock: number;
		var synLine: number;
		var libFoundThisSweep: boolean;
		var libFileName: string;
		var libraryList: string;
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
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	DecryptString(): string {
		var output = "";
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	RemoveTabs(): string {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
		var res: any;
		// UNKNOWN DoLoopUntilBlock
		var numElements = 1;
		var elements: string;
		// UNKNOWN ReDimStatement
		var numOperators = 0;
		var operators: string;
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
		var contentsIDs: number;
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
	// UNKNOWN SubBlock
	GetHTMLColour(): string {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN SelectBlock
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	ExecuteIfFlag(): boolean {
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	ExecuteIfAction(): boolean {
		var id: number;
		var scp = 'expr';
		// UNKNOWN MultiLineIfBlock
		var objName = 'expr';
		var actionName = 'expr';
		var found = false;
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var o = 'expr';
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
		var o = 'expr';
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	GetArrayIndex(): ArrayResult {
		var result: any;
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
		var idNumbers: number;
		var firstPlace: string;
		var secondPlace: string = "";
		var twoPlaces: boolean;
		var descriptionText: string;
		var validNames: string;
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
		var o = 'expr';
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		var NewThread: Context = 'expr';
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ReturnStatement
	}
	HasAction(): boolean {
		var o = 'expr';
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	ExecuteCondition(): boolean {
		var result: boolean;
		var thisNot: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	ExecuteConditions(): boolean {
		var conditions: string;
		var numConditions = 0;
		var operations: string;
		var obscuredConditionList = 'expr';
		var pos = 1;
		var isFinalCondition = false;
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN SimpleAssignmentStatement
		var result = true;
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
		var o = 'expr';
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetPropertiesInType(): PropertiesActions {
		var blockId: number;
		var propertyList: any;
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
		var data: any;
		var objectData: ChangeType;
		var roomData: ChangeType;
		var numObjectData: number;
		var numRoomData: number;
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
		var dataString: string;
		var newFileData: any;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
		var parameters: string;
		var untrimmedParameters: string;
		var objId: number;
		var numParameters = 0;
		var pos = 1;
		// UNKNOWN MultiLineIfBlock
		var param2: string;
		var param3: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	ExecuteIfAsk(): boolean {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	GetErrorMessage(): string {
		// UNKNOWN ReturnStatement
	}
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
		var menuOptions: any;
		var menuScript: any;
		// UNKNOWN ForBlock
		// UNKNOWN ExpressionStatement
		var mnu: any;
		var choice: string = 'expr';
		// UNKNOWN ExpressionStatement
		// UNKNOWN ReturnStatement
	}
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
	// UNKNOWN SubBlock
	GetCommandParameters(): boolean {
		var chunksBegin: number;
		var chunksEnd: number;
		var varName: string;
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
		var lines: any = 'expr';
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
	SaveGame(): any {
		var ctx: Context = new Context();
		var saveData: string;
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	MakeRestoreDataV2(): string {
		var lines: any;
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	DisplayCollectableInfo(): string {
		var display: string;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	ExecCommand(): boolean {
		var parameter: string;
		var skipAfterTurn = false;
		// UNKNOWN SimpleAssignmentStatement
		var oldBadCmdBefore = _badCmdBefore;
		var roomID = 'expr';
		var enteredHelpCommand = false;
		// UNKNOWN SingleLineIfStatement
		var cmd = 'expr';
		// UNKNOWN SyncLockBlock
		var userCommandReturn: boolean;
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		var newCommand = 'expr';
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
		var endOfFirstBit = 'expr';
		var lengthOfKeyword = 'expr';
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
		var r = 'expr';
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	PlayerCanAccessObject(): PlayerCanAccessObjectResult {
		var parent: string;
		var parentId: number;
		var parentDisplayName: string;
		var result: any;
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
	// UNKNOWN SubBlock
	FindExit(): RoomExit {
		var params = 'expr';
		// UNKNOWN MultiLineIfBlock
		var room = 'expr';
		var exitName = 'expr';
		var roomId = 'expr';
		// UNKNOWN MultiLineIfBlock
		var exits = 'expr'.Exits;
		var dir = 'expr';
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
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
	Save(): any {
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN PropertyBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN EventStatement
	Initialise(): boolean {
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
	}
	// UNKNOWN SubBlock
	GetResourcePath(): string {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	GetLibraryLines(): any {
		var libCode: any = null;
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SelectBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN PropertyBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	m_menuResponse: string;
	ShowMenu(): string {
		// UNKNOWN ExpressionStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN SyncLockBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
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
	// UNKNOWN SubBlock
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
	GetResourcelessCAS(): any {
		var fileData: string = 'expr';
		// UNKNOWN ReturnStatement
	}
}
