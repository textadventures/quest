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
	// UNKNOWN FunctionBlock
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
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
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
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
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
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
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
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
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
	// UNKNOWN FunctionBlock
	// UNKNOWN PropertyBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN EventStatement
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
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
	// UNKNOWN FunctionBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN EventStatement
	// UNKNOWN PropertyBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN DelegateFunctionStatement
	m_unzipFunction: UnzipFunctionDelegate;
	// UNKNOWN SubBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN PropertyBlock
	// UNKNOWN PropertyBlock
	// UNKNOWN FunctionBlock
	m_gameId: string;
	// UNKNOWN PropertyBlock
	// UNKNOWN FunctionBlock
	// UNKNOWN FunctionBlock
}
