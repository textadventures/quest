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

    private class DefineBlock
    {
        public int EndLine;
        public int StartLine;
    }

    internal class Context
    {
        public bool AllowRealNamesInCommand;
        public int CallingObjectId;
        public bool CancelExec;
        public bool DontProcessCommand;
        public string FunctionReturnValue;
        public int NumParameters;
        public string[] Parameters;
        public int StackCounter;
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
        public bool Got;
        public string Name;
    }

    private class Collectable
    {
        public string Display;
        public bool DisplayWhenZero;
        public string Name;
        public string Type;
        public double Value;
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
        public string UseScript;
        public UseType UseType;
    }

    internal class GiveDataType
    {
        public string GiveObject;
        public string GiveScript;
        public GiveType GiveType;
    }

    private class PropertiesActions
    {
        public ActionType[] Actions;
        public int NumberActions;
        public int NumberTypesIncluded;
        public string Properties;
        public string[] TypesIncluded;
    }

    private class VariableType
    {
        public string DisplayString;
        public bool NoZeroDisplay;
        public string OnChangeScript;
        public string[] VariableContents;
        public string VariableName;
        public int VariableUBound;
    }

    private class SynonymType
    {
        public string ConvertTo;
        public string OriginalWord;
    }

    private class TimerType
    {
        public bool BypassThisTurn;
        public string TimerAction;
        public bool TimerActive;
        public int TimerInterval;
        public string TimerName;
        public int TimerTicks;
    }

    internal class UserDefinedCommandType
    {
        public string CommandScript;
        public string CommandText;
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
        public string Script;
        public string Text;
    }

    internal class PlaceType
    {
        public string PlaceName;
        public string Prefix;
        public string Script;
    }

    internal class RoomType
    {
        public string AfterTurnScript;
        public string BeforeTurnScript;
        public UserDefinedCommandType[] Commands;
        public TextAction Description = new();
        public TextAction Down = new();
        public TextAction East = new();
        public RoomExits Exits;
        public string InDescription;
        public string Look;
        public TextAction North = new();
        public TextAction NorthEast = new();
        public TextAction NorthWest = new();
        public int NumberCommands;
        public int NumberPlaces;
        public int NumberUse;
        public int ObjId;
        public ScriptText Out = new();
        public PlaceType[] Places;
        public string Prefix;
        public string RoomAlias;
        public string RoomName;
        public string Script;
        public TextAction South = new();
        public TextAction SouthEast = new();
        public TextAction SouthWest = new();
        public TextAction Up = new();
        public ScriptText[] Use;
        public TextAction West = new();
    }

    internal class ObjectType
    {
        public ActionType[] Actions;
        public TextAction AddScript = new();
        public string[] AltNames;
        public string Article;
        public TextAction CloseScript = new();
        public string ContainerRoom;
        public string CorresRoom;
        public int CorresRoomId;
        public int DefinitionSectionEnd;
        public int DefinitionSectionStart;
        public string Detail;
        public string DisplayType;
        public bool Exists;
        public string GainScript;
        public string Gender;
        public string GiveAnything;
        public GiveDataType[] GiveData;
        public string GiveToAnything;
        public bool IsExit;
        public bool IsRoom;
        public bool Loaded;
        public string LoseScript;
        public int NumberActions;
        public int NumberAltNames;
        public int NumberGiveData;
        public int NumberProperties;
        public int NumberTypesIncluded;
        public int NumberUseData;
        public string ObjectAlias;
        public string ObjectName;
        public TextAction OpenScript = new();
        public string Prefix;
        public PropertyType[] Properties;
        public TextAction RemoveScript = new();
        public TextAction Speak = new();
        public string Suffix;
        public TextAction Take = new();
        public string[] TypesIncluded;
        public string Use;
        public string UseAnything;
        public UseDataType[] UseData;
        public string UseOnAnything;
        public bool Visible;
    }

    private class ChangeType
    {
        public string AppliesTo;
        public string Change;
    }

    private class GameChangeDataType
    {
        public ChangeType[] ChangeData;
        public int NumberChanges;
    }

    private class ResourceType
    {
        public int ResourceLength;
        public string ResourceName;
        public int ResourceStart;
    }

    private class ExpressionResult
    {
        public string Message;
        public string Result;
        public ExpressionSuccess Success;
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