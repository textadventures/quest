namespace QuestViva.Legacy;

public partial class V4Game
{
    public enum State
    {
        Ready, // game is not doing any processing, and is ready for a command
        Working, // game is processing a command
        Waiting, // while processing a command, game has encountered e.g. an "enter" script, and is awaiting further input
        Finished // game is over
    }

    private class DefineBlock
    {
        public int StartLine;
        public int EndLine;
    }

    internal class Context
    {
        public int CallingObjectId;
        public int NumParameters;
        public string[] Parameters;
        public string FunctionReturnValue;
        public bool AllowRealNamesInCommand;
        public bool DontProcessCommand;
        public bool CancelExec;
        public int StackCounter;
    }

    private Context CopyContext(Context ctx)
    {
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
    }

    internal enum LogType
    {
        Misc,
        FatalError,
        WarningError,
        Init,
        LibraryWarningError,
        Warning,
        UserError,
        InternalError
    }
    
        internal enum Direction
    {
        None = -1,
        Out = 0,
        North = 1,
        South = 2,
        East = 3,
        West = 4,
        NorthWest = 5,
        NorthEast = 6,
        SouthWest = 7,
        SouthEast = 8,
        Up = 9,
        Down = 10
    }

    private class ItemType
    {
        public string Name;
        public bool Got;
    }

    private class Collectable
    {
        public string Name;
        public string Type;
        public double Value;
        public string Display;
        public bool DisplayWhenZero;
    }

    internal class PropertyType
    {
        public string PropertyName;
        public string PropertyValue;
    }

    internal class ActionType
    {
        public string ActionName;
        public string Script;
    }

    internal class UseDataType
    {
        public string UseObject;
        public UseType UseType;
        public string UseScript;
    }

    internal class GiveDataType
    {
        public string GiveObject;
        public GiveType GiveType;
        public string GiveScript;
    }

    private class PropertiesActions
    {
        public string Properties;
        public int NumberActions;
        public ActionType[] Actions;
        public int NumberTypesIncluded;
        public string[] TypesIncluded;
    }

    private class VariableType
    {
        public string VariableName;
        public string[] VariableContents;
        public int VariableUBound;
        public string DisplayString;
        public string OnChangeScript;
        public bool NoZeroDisplay;
    }

    private class SynonymType
    {
        public string OriginalWord;
        public string ConvertTo;
    }

    private class TimerType
    {
        public string TimerName;
        public int TimerInterval;
        public bool TimerActive;
        public string TimerAction;
        public int TimerTicks;
        public bool BypassThisTurn;
    }

    internal class UserDefinedCommandType
    {
        public string CommandText;
        public string CommandScript;
    }

    internal class TextAction
    {
        public string Data;
        public TextActionType Type;
    }

    internal enum TextActionType
    {
        Text,
        Script,
        Nothing,
        Default
    }

    internal class ScriptText
    {
        public string Text;
        public string Script;
    }

    internal class PlaceType
    {
        public string PlaceName;
        public string Prefix;
        public string Script;
    }

    internal class RoomType
    {
        public string RoomName;
        public string RoomAlias;
        public UserDefinedCommandType[] Commands;
        public int NumberCommands;
        public TextAction Description = new();
        public ScriptText Out = new();
        public TextAction East = new();
        public TextAction West = new();
        public TextAction North = new();
        public TextAction South = new();
        public TextAction NorthEast = new();
        public TextAction NorthWest = new();
        public TextAction SouthEast = new();
        public TextAction SouthWest = new();
        public TextAction Up = new();
        public TextAction Down = new();
        public string InDescription;
        public string Look;
        public PlaceType[] Places;
        public int NumberPlaces;
        public string Prefix;
        public string Script;
        public ScriptText[] Use;
        public int NumberUse;
        public int ObjId;
        public string BeforeTurnScript;
        public string AfterTurnScript;
        public RoomExits Exits;
    }

    internal class ObjectType
    {
        public string ObjectName;
        public string ObjectAlias;
        public string Detail;
        public string ContainerRoom;
        public bool Exists;
        public string Prefix;
        public string Suffix;
        public string Gender;
        public string Article;
        public int DefinitionSectionStart;
        public int DefinitionSectionEnd;
        public bool Visible;
        public string GainScript;
        public string LoseScript;
        public int NumberProperties;
        public PropertyType[] Properties;
        public TextAction Speak = new();
        public TextAction Take = new();
        public bool IsRoom;
        public bool IsExit;
        public string CorresRoom;
        public int CorresRoomId;
        public bool Loaded;
        public int NumberActions;
        public ActionType[] Actions;
        public int NumberUseData;
        public UseDataType[] UseData;
        public string UseAnything;
        public string UseOnAnything;
        public string Use;
        public int NumberGiveData;
        public GiveDataType[] GiveData;
        public string GiveAnything;
        public string GiveToAnything;
        public string DisplayType;
        public int NumberTypesIncluded;
        public string[] TypesIncluded;
        public int NumberAltNames;
        public string[] AltNames;
        public TextAction AddScript = new();
        public TextAction RemoveScript = new();
        public TextAction OpenScript = new();
        public TextAction CloseScript = new();
    }

    private class ChangeType
    {
        public string AppliesTo;
        public string Change;
    }

    private class GameChangeDataType
    {
        public int NumberChanges;
        public ChangeType[] ChangeData;
    }

    private class ResourceType
    {
        public string ResourceName;
        public int ResourceStart;
        public int ResourceLength;
        public bool Extracted;
    }

    private class ExpressionResult
    {
        public string Result;
        public ExpressionSuccess Success;
        public string Message;
    }

    internal enum PlayerError
    {
        BadCommand,
        BadGo,
        BadGive,
        BadCharacter,
        NoItem,
        ItemUnwanted,
        BadLook,
        BadThing,
        DefaultLook,
        DefaultSpeak,
        BadItem,
        DefaultTake,
        BadUse,
        DefaultUse,
        DefaultOut,
        BadPlace,
        BadExamine,
        DefaultExamine,
        BadTake,
        CantDrop,
        DefaultDrop,
        BadDrop,
        BadPronoun,
        AlreadyOpen,
        AlreadyClosed,
        CantOpen,
        CantClose,
        DefaultOpen,
        DefaultClose,
        BadPut,
        CantPut,
        DefaultPut,
        CantRemove,
        AlreadyPut,
        DefaultRemove,
        Locked,
        DefaultWait,
        AlreadyTaken
    }

    private enum ItType
    {
        Inanimate,
        Male,
        Female
    }

    private enum SetResult
    {
        Error,
        Found,
        Unfound
    }

    private enum Thing
    {
        Character,
        Object,
        Room
    }

    private enum ConvertType
    {
        Strings,
        Functions,
        Numeric,
        Collectables
    }

    internal enum UseType
    {
        UseOnSomething,
        UseSomethingOn
    }

    internal enum GiveType
    {
        GiveToSomething,
        GiveSomethingTo
    }

    private enum VarType
    {
        String,
        Numeric
    }

    private enum StopType
    {
        Win,
        Lose,
        Null
    }

    private enum ExpressionSuccess
    {
        OK,
        Fail
    }
}