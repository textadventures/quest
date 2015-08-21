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
	_commandLock: Object;
	_stateLock: Object;
	_state: State;
	_waitLock: Object;
	_readyForCommand: boolean;
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN ReturnStatement
	}
	CheckSections(): boolean {
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
	}
	ConvertFriendlyIfs(): boolean {
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
	}
	// UNKNOWN SubBlock
	ErrorCheck(): boolean {
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
	}
	GetAfterParameter(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
	}
	ObliterateParameters(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
	}
	ObliterateVariableNames(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	ReportErrorLine(): string {
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	GetDefineBlock(): DefineBlock {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	DefineBlockParam(): DefineBlock {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	GetResourceLines(): any {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ReturnStatement
	}
	ParseFile(): boolean {
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
	}
	// UNKNOWN SubBlock
	GetParameter(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	DecryptString(): string {
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
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
	}
	ExpressionHandler(): ExpressionResult {
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
	}
	ListContents(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ForBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	ObscureNumericExps(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	ExecuteIfAction(): boolean {
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
	}
	ExecuteIfType(): boolean {
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
	}
	GetArrayIndex(): ArrayResult {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	Disambiguate(): number {
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
	}
	DisplayStatusVariableInfo(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	DoAction(): boolean {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ExpressionStatement
		// UNKNOWN ReturnStatement
	}
	HasAction(): boolean {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	ExecuteCondition(): boolean {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	ExecuteConditions(): boolean {
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
	}
	ExecuteIfProperty(): boolean {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	GetNextChunk(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetFileDataChars(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	GetObjectActions(): ActionType {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	GetObjectId(): number {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ForBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetPropertiesInType(): PropertiesActions {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetThingNumber(): number {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	GetThingBlock(): DefineBlock {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	MakeRestoreData(): string {
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
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	ConvertParameter(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN ReturnStatement
	}
	DoFunction(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	DoInternalFunction(): string {
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
	}
	ExecuteIfIs(): boolean {
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
	}
	GetNumericContents(): number {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	SetUpChoiceForm(): string {
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
	}
	// UNKNOWN SubBlock
	GetCommandParameters(): boolean {
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
	}
	GetGender(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN ReturnStatement
	}
	GetStringContents(): string {
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
	}
	IsAvailable(): boolean {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
	}
	IsCompatible(): boolean {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN DoLoopUntilBlock
		// UNKNOWN ReturnStatement
	}
	OpenGame(): boolean {
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
	}
	SaveGame(): any {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SingleLineIfStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	MakeRestoreDataV2(): string {
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
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	DisplayCollectableInfo(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	ExecCommand(): boolean {
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
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
	}
	PlaceExist(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	RetrLine(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	RetrLineParam(): string {
		// UNKNOWN LocalDeclarationStatement
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
	}
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	PlayerCanAccessObject(): PlayerCanAccessObjectResult {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	GetGoToExits(): string {
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ForBlock
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN SubBlock
	FindExit(): RoomExit {
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
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN SimpleAssignmentStatement
		// UNKNOWN ReturnStatement
	}
	// UNKNOWN PropertyBlock
	// UNKNOWN PropertyBlock
	GetResource(): any {
		// UNKNOWN MultiLineIfBlock
		// UNKNOWN LocalDeclarationStatement
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
		// UNKNOWN LocalDeclarationStatement
		// UNKNOWN ReturnStatement
	}
}
