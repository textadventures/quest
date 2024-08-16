using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace TextAdventures.Quest.LegacyASL;

public partial class LegacyGame : IASL, IASLTimer
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

    private Dictionary<string, Dictionary<string, string>> _defineBlockParams;

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

    private string _openErrorReport;
    private readonly string[] _casKeywords = new string[256]; // Tokenised CAS keywords
    private string[] _lines; // Stores the lines of the ASL script/definitions
    private DefineBlock[] _defineBlocks; // Stores the start and end lines of each 'define' section
    private int _numberSections; // Number of define sections
    private string _gameName; // The name of the game
    internal Context _nullContext = new();
    private ChangeLog _changeLogRooms;
    private ChangeLog _changeLogObjects;
    private PropertiesActions _defaultProperties;
    private PropertiesActions _defaultRoomProperties;
    internal RoomType[] _rooms;
    internal int _numberRooms;
    private VariableType[] _numericVariable;
    private int _numberNumericVariables;
    private VariableType[] _stringVariable;
    private int _numberStringVariables;
    private SynonymType[] _synonyms;
    private int _numberSynonyms;
    private ItemType[] _items;
    private ObjectType[] _chars;
    internal ObjectType[] _objs;
    private int _numberChars;
    internal int _numberObjs;
    private int _numberItems;
    internal string _currentRoom;
    private Collectable[] _collectables;
    private int _numCollectables;
    private string _gamePath;
    private string _gameFileName;
    private string _saveGameFile;
    private string _defaultFontName;
    private double _defaultFontSize;
    private bool _autoIntro;
    private bool _commandOverrideModeOn;
    private string _commandOverrideVariable;
    private string _afterTurnScript;
    private string _beforeTurnScript;
    private bool _outPutOn;
    private int _choiceNumber;
    private string _gameLoadMethod;
    private TimerType[] _timers;
    private int _numberTimers;
    private int _numDisplayStrings;
    private int _numDisplayNumerics;
    private bool _gameFullyLoaded;
    private readonly GameChangeDataType _gameChangeData = new();
    private int _lastIt;
    private ItType _lastItMode;
    private int _thisTurnIt;
    private ItType _thisTurnItMode;
    private string _badCmdBefore;
    private string _badCmdAfter;
    private int _numResources;
    private ResourceType[] _resources;
    private string _resourceFile;
    private int _resourceOffset;
    private int _startCatPos;
    private bool _useAbbreviations;
    private bool _loadedFromQsg;
    private string _beforeSaveScript;
    private string _onLoadScript;
    private readonly int _numSkipCheckFiles;
    private readonly string[] _skipCheckFile;
    private List<ListData> _compassExits = new();
    private List<ListData> _gotoExits = new();
    private readonly TextFormatter _textFormatter = new();
    private readonly List<string> _log = new();
    private string _casFileData;
    private readonly object _commandLock = new();
    private readonly object _stateLock = new();
    private State _state = State.Ready;
    private readonly object _waitLock = new();
    private bool _readyForCommand = true;
    private bool _gameLoading;
    private readonly Random _random = new();
    private readonly string[] _playerErrorMessageString = new string[39];
    private readonly Dictionary<ListType, List<string>> _listVerbs = new();
    private readonly IGameData _gameData;
    private string _originalFilename;
    private IPlayer _player;
    private bool _gameFinished;
    private bool _gameIsRestoring;
    private bool _useStaticFrameForPictures;
    private string _fileData;
    private int _fileDataPos;
    private bool _questionResponse;

    public LegacyGame(IGameData gameData)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        TempFolder = Path.Combine(Path.GetTempPath(), "Quest", Guid.NewGuid().ToString());
        LoadCASKeywords();
        _gameLoadMethod = "normal";
        _gameData = gameData;
        _originalFilename = null;

        // Very early versions of Quest didn't perform very good syntax checking of ASL files, so this is
        // for compatibility with games which have non-fatal errors in them.
        _numSkipCheckFiles = 3;
        _skipCheckFile = new string[4];
        _skipCheckFile[1] = "bargain.cas";
        _skipCheckFile[2] = "easymoney.asl";
        _skipCheckFile[3] = "musicvf1.cas";
    }

    private string RemoveFormatting(string s)
    {
        string code;
        int pos, len = default;

        do
        {
            pos = Strings.InStr(s, "|");
            if (pos != 0)
            {
                code = Strings.Mid(s, pos + 1, 3);

                if (Strings.Left(code, 1) == "b")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 2) == "xb")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "u")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 2) == "xu")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "i")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 2) == "xi")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cr")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cb")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cl")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cy")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cg")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "n")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 2) == "xn")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "s")
                {
                    len = 3;
                }
                else if (Strings.Left(code, 2) == "jc")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "jl")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "jr")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "w")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 1) == "c")
                {
                    len = 1;
                }

                if (len == 0)
                    // unknown code
                {
                    len = 1;
                }

                s = Strings.Left(s, pos - 1) + Strings.Mid(s, pos + len + 1);
            }
        } while (pos != 0);

        return s;
    }

    private bool CheckSections()
    {
        int defines, braces;
        var checkLine = "";
        int bracePos;
        int pos;
        var section = "";
        bool hasErrors;
        var skipBlock = default(bool);
        _openErrorReport = "";
        hasErrors = false;
        defines = 0;
        braces = 0;

        for (int i = 1, loopTo = Information.UBound(_lines); i <= loopTo; i++)
        {
            if (!BeginsWith(_lines[i], "#!qdk-note: "))
            {
                if (BeginsWith(_lines[i], "define "))
                {
                    section = _lines[i];
                    braces = 0;
                    defines = defines + 1;
                    skipBlock = BeginsWith(_lines[i], "define text") | BeginsWith(_lines[i], "define synonyms");
                }
                else if (Strings.Trim(_lines[i]) == "end define")
                {
                    defines = defines - 1;

                    if (defines < 0)
                    {
                        LogASLError("Extra 'end define' after block '" + section + "'", LogType.FatalError);
                        _openErrorReport = _openErrorReport + "Extra 'end define' after block '" + section + "'" +
                                           Constants.vbCrLf;
                        hasErrors = true;
                        defines = 0;
                    }

                    if (braces > 0)
                    {
                        LogASLError("Missing } in block '" + section + "'", LogType.FatalError);
                        _openErrorReport = _openErrorReport + "Missing } in block '" + section + "'" + Constants.vbCrLf;
                        hasErrors = true;
                    }
                    else if (braces < 0)
                    {
                        LogASLError("Too many } in block '" + section + "'", LogType.FatalError);
                        _openErrorReport = _openErrorReport + "Too many } in block '" + section + "'" +
                                           Constants.vbCrLf;
                        hasErrors = true;
                    }
                }

                if ((Strings.Left(_lines[i], 1) != "'") & !skipBlock)
                {
                    checkLine = ObliterateParameters(_lines[i]);
                    if (BeginsWith(checkLine, "'<ERROR;"))
                    {
                        // ObliterateParameters denotes a mismatched $, ( etc.
                        // by prefixing line with '<ERROR;*; where * is the mismatched
                        // character
                        LogASLError(
                            "Expected closing " + Strings.Mid(checkLine, 9, 1) + " character in '" +
                            ReportErrorLine(_lines[i]) + "'", LogType.FatalError);
                        _openErrorReport = _openErrorReport + "Expected closing " + Strings.Mid(checkLine, 9, 1) +
                                           " character in '" + ReportErrorLine(_lines[i]) + "'." + Constants.vbCrLf;
                        return false;
                    }
                }

                if (Strings.Left(Strings.Trim(checkLine), 1) != "'")
                {
                    // Now check {
                    pos = 1;
                    do
                    {
                        bracePos = Strings.InStr(pos, checkLine, "{");
                        if (bracePos != 0)
                        {
                            pos = bracePos + 1;
                            braces = braces + 1;
                        }
                    } while (!((bracePos == 0) | (pos > Strings.Len(checkLine))));

                    // Now check }
                    pos = 1;
                    do
                    {
                        bracePos = Strings.InStr(pos, checkLine, "}");
                        if (bracePos != 0)
                        {
                            pos = bracePos + 1;
                            braces = braces - 1;
                        }
                    } while (!((bracePos == 0) | (pos > Strings.Len(checkLine))));
                }
            }
        }

        if (defines > 0)
        {
            LogASLError("Missing 'end define'", LogType.FatalError);
            _openErrorReport = _openErrorReport + "Missing 'end define'." + Constants.vbCrLf;
            hasErrors = true;
        }

        return !hasErrors;
    }

    private bool ConvertFriendlyIfs()
    {
        // Converts
        // if (%something% < 3) then ...
        // to
        // if is <%something%;lt;3> then ...
        // and also repeat until ...

        // Returns False if successful

        int convPos, symbPos;
        string symbol;
        int endParamPos;
        string paramData;
        int startParamPos;
        string firstData, secondData;
        string obscureLine, newParam, varObscureLine;
        int bracketCount;

        for (int i = 1, loopTo = Information.UBound(_lines); i <= loopTo; i++)
        {
            obscureLine = ObliterateParameters(_lines[i]);
            convPos = Strings.InStr(obscureLine, "if (");
            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "until (");
            }

            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "while (");
            }

            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "not (");
            }

            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "and (");
            }

            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "or (");
            }


            if (convPos != 0)
            {
                varObscureLine = ObliterateVariableNames(_lines[i]);
                if (BeginsWith(varObscureLine, "'<ERROR;"))
                {
                    // ObliterateVariableNames denotes a mismatched #, % or $
                    // by prefixing line with '<ERROR;*; where * is the mismatched
                    // character
                    LogASLError(
                        "Expected closing " + Strings.Mid(varObscureLine, 9, 1) + " character in '" +
                        ReportErrorLine(_lines[i]) + "'", LogType.FatalError);
                    return true;
                }

                startParamPos = Strings.InStr(convPos, _lines[i], "(");

                endParamPos = 0;
                bracketCount = 1;
                for (int j = startParamPos + 1, loopTo1 = Strings.Len(_lines[i]); j <= loopTo1; j++)
                {
                    if (Strings.Mid(_lines[i], j, 1) == "(")
                    {
                        bracketCount = bracketCount + 1;
                    }
                    else if (Strings.Mid(_lines[i], j, 1) == ")")
                    {
                        bracketCount = bracketCount - 1;
                    }

                    if (bracketCount == 0)
                    {
                        endParamPos = j;
                        break;
                    }
                }

                if (endParamPos == 0)
                {
                    LogASLError("Expected ) in '" + ReportErrorLine(_lines[i]) + "'", LogType.FatalError);
                    return true;
                }

                paramData = Strings.Mid(_lines[i], startParamPos + 1, endParamPos - startParamPos - 1);

                symbPos = Strings.InStr(paramData, "!=");
                if (symbPos == 0)
                {
                    symbPos = Strings.InStr(paramData, "<>");
                    if (symbPos == 0)
                    {
                        symbPos = Strings.InStr(paramData, "<=");
                        if (symbPos == 0)
                        {
                            symbPos = Strings.InStr(paramData, ">=");
                            if (symbPos == 0)
                            {
                                symbPos = Strings.InStr(paramData, "<");
                                if (symbPos == 0)
                                {
                                    symbPos = Strings.InStr(paramData, ">");
                                    if (symbPos == 0)
                                    {
                                        symbPos = Strings.InStr(paramData, "=");
                                        if (symbPos == 0)
                                        {
                                            LogASLError(
                                                "Unrecognised 'if' condition in '" + ReportErrorLine(_lines[i]) + "'",
                                                LogType.FatalError);
                                            return true;
                                        }

                                        symbol = "=";
                                    }
                                    else
                                    {
                                        symbol = ">";
                                    }
                                }
                                else
                                {
                                    symbol = "<";
                                }
                            }
                            else
                            {
                                symbol = ">=";
                            }
                        }
                        else
                        {
                            symbol = "<=";
                        }
                    }
                    else
                    {
                        symbol = "<>";
                    }
                }
                else
                {
                    symbol = "<>";
                }


                firstData = Strings.Trim(Strings.Left(paramData, symbPos - 1));
                secondData = Strings.Trim(Strings.Mid(paramData, symbPos + Strings.Len(symbol)));

                if (symbol == "=")
                {
                    newParam = "is <" + firstData + ";" + secondData + ">";
                }
                else
                {
                    newParam = "is <" + firstData + ";";
                    if (symbol == "<")
                    {
                        newParam = newParam + "lt";
                    }
                    else if (symbol == ">")
                    {
                        newParam = newParam + "gt";
                    }
                    else if (symbol == ">=")
                    {
                        newParam = newParam + "gt=";
                    }
                    else if (symbol == "<=")
                    {
                        newParam = newParam + "lt=";
                    }
                    else if (symbol == "<>")
                    {
                        newParam = newParam + "!=";
                    }

                    newParam = newParam + ";" + secondData + ">";
                }

                _lines[i] = Strings.Left(_lines[i], startParamPos - 1) + newParam +
                            Strings.Mid(_lines[i], endParamPos + 1);

                // Repeat processing this line, in case there are
                // further changes to be made.
                i = i - 1;
            }
        }

        return false;
    }

    private void ConvertMultiLineSections()
    {
        int startLine, braceCount;
        string thisLine, lineToAdd;
        var lastBrace = default(int);
        int i;
        string restOfLine, procName;
        int endLineNum;
        string afterLastBrace, z;
        string startOfOrig;
        string testLine;
        int testBraceCount;
        int obp, cbp;
        var curProc = default(int);

        i = 1;
        do
        {
            z = _lines[_defineBlocks[i].StartLine];
            if (!BeginsWith(z, "define text ") & !BeginsWith(z, "define menu ") & (z != "define synonyms"))
            {
                for (int j = _defineBlocks[i].StartLine + 1, loopTo = _defineBlocks[i].EndLine - 1; j <= loopTo; j++)
                {
                    if (Strings.InStr(_lines[j], "{") > 0)
                    {
                        afterLastBrace = "";
                        thisLine = Strings.Trim(_lines[j]);

                        procName = "<!intproc" + curProc + ">";

                        // see if this brace's corresponding closing
                        // brace is on same line:
                        testLine = Strings.Mid(_lines[j], Strings.InStr(_lines[j], "{") + 1);
                        testBraceCount = 1;
                        do
                        {
                            obp = Strings.InStr(testLine, "{");
                            cbp = Strings.InStr(testLine, "}");
                            if (obp == 0)
                            {
                                obp = Strings.Len(testLine) + 1;
                            }

                            if (cbp == 0)
                            {
                                cbp = Strings.Len(testLine) + 1;
                            }

                            if (obp < cbp)
                            {
                                testBraceCount = testBraceCount + 1;
                                testLine = Strings.Mid(testLine, obp + 1);
                            }
                            else if (cbp < obp)
                            {
                                testBraceCount = testBraceCount - 1;
                                testLine = Strings.Mid(testLine, cbp + 1);
                            }
                        } while (!((obp == cbp) | (testBraceCount == 0)));

                        if (testBraceCount != 0)
                        {
                            AddLine("define procedure " + procName);
                            startLine = Information.UBound(_lines);
                            restOfLine = Strings.Trim(Strings.Right(thisLine,
                                Strings.Len(thisLine) - Strings.InStr(thisLine, "{")));
                            braceCount = 1;
                            if (!string.IsNullOrEmpty(restOfLine))
                            {
                                AddLine(restOfLine);
                            }

                            for (int m = 1, loopTo1 = Strings.Len(restOfLine); m <= loopTo1; m++)
                            {
                                if (Strings.Mid(restOfLine, m, 1) == "{")
                                {
                                    braceCount = braceCount + 1;
                                }
                                else if (Strings.Mid(restOfLine, m, 1) == "}")
                                {
                                    braceCount = braceCount - 1;
                                }
                            }

                            if (braceCount != 0)
                            {
                                var k = j + 1;
                                do
                                {
                                    for (int m = 1, loopTo2 = Strings.Len(_lines[k]); m <= loopTo2; m++)
                                    {
                                        if (Strings.Mid(_lines[k], m, 1) == "{")
                                        {
                                            braceCount = braceCount + 1;
                                        }
                                        else if (Strings.Mid(_lines[k], m, 1) == "}")
                                        {
                                            braceCount = braceCount - 1;
                                        }

                                        if (braceCount == 0)
                                        {
                                            lastBrace = m;
                                            break;
                                        }
                                    }

                                    if (braceCount != 0)
                                    {
                                        // put Lines(k) into another variable, as
                                        // AddLine ReDims Lines, which it can't do if
                                        // passed Lines(x) as a parameter.
                                        lineToAdd = _lines[k];
                                        AddLine(lineToAdd);
                                    }
                                    else
                                    {
                                        AddLine(Strings.Left(_lines[k], lastBrace - 1));
                                        afterLastBrace = Strings.Trim(Strings.Mid(_lines[k], lastBrace + 1));
                                    }

                                    // Clear original line
                                    _lines[k] = "";
                                    k = k + 1;
                                } while (braceCount != 0);
                            }

                            AddLine("end define");
                            endLineNum = Information.UBound(_lines);

                            _numberSections = _numberSections + 1;
                            Array.Resize(ref _defineBlocks, _numberSections + 1);
                            _defineBlocks[_numberSections] = new DefineBlock();
                            _defineBlocks[_numberSections].StartLine = startLine;
                            _defineBlocks[_numberSections].EndLine = endLineNum;

                            // Change original line where the { section
                            // started to call the new procedure.
                            startOfOrig = Strings.Trim(Strings.Left(thisLine, Strings.InStr(thisLine, "{") - 1));
                            _lines[j] = startOfOrig + " do " + procName + " " + afterLastBrace;
                            curProc = curProc + 1;

                            // Process this line again in case there was stuff after the last brace that included
                            // more braces. e.g. } else {
                            j = j - 1;
                        }
                    }
                }
            }

            i = i + 1;
        } while (i <= _numberSections);

        // Join next-line "else"s to corresponding "if"s

        var loopTo3 = _numberSections;
        for (i = 1; i <= loopTo3; i++)
        {
            z = _lines[_defineBlocks[i].StartLine];
            if (!BeginsWith(z, "define text ") & !BeginsWith(z, "define menu ") & (z != "define synonyms"))
            {
                for (int j = _defineBlocks[i].StartLine + 1, loopTo4 = _defineBlocks[i].EndLine - 1; j <= loopTo4; j++)
                {
                    if (BeginsWith(_lines[j], "else "))
                        // Go upwards to find "if" statement that this
                        // belongs to
                    {
                        for (int k = j, loopTo5 = _defineBlocks[i].StartLine + 1; k >= loopTo5; k -= 1)
                        {
                            if (BeginsWith(_lines[k], "if ") |
                                (Strings.InStr(ObliterateParameters(_lines[k]), " if ") != 0))
                            {
                                _lines[k] = _lines[k] + " " + Strings.Trim(_lines[j]);
                                _lines[j] = "";
                                k = _defineBlocks[i].StartLine;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool ErrorCheck()
    {
        // Parses ASL script for errors. Returns TRUE if OK;
        // False if a critical error is encountered.
        int curBegin, curEnd;
        bool hasErrors;
        int curPos;
        int numParamStart, numParamEnd;
        bool finLoop, inText;

        hasErrors = false;
        inText = false;

        // Checks for incorrect number of < and > :
        for (int i = 1, loopTo = Information.UBound(_lines); i <= loopTo; i++)
        {
            numParamStart = 0;
            numParamEnd = 0;

            if (BeginsWith(_lines[i], "define text "))
            {
                inText = true;
            }

            if (inText & (Strings.Trim(Strings.LCase(_lines[i])) == "end define"))
            {
                inText = false;
            }

            if (!inText)
            {
                // Find number of <'s:
                curPos = 1;
                finLoop = false;
                do
                {
                    if (Strings.InStr(curPos, _lines[i], "<") != 0)
                    {
                        numParamStart = numParamStart + 1;
                        curPos = Strings.InStr(curPos, _lines[i], "<") + 1;
                    }
                    else
                    {
                        finLoop = true;
                    }
                } while (!finLoop);

                // Find number of >'s:
                curPos = 1;
                finLoop = false;
                do
                {
                    if (Strings.InStr(curPos, _lines[i], ">") != 0)
                    {
                        numParamEnd = numParamEnd + 1;
                        curPos = Strings.InStr(curPos, _lines[i], ">") + 1;
                    }
                    else
                    {
                        finLoop = true;
                    }
                } while (!finLoop);

                if (numParamStart > numParamEnd)
                {
                    LogASLError("Expected > in " + ReportErrorLine(_lines[i]), LogType.FatalError);
                    hasErrors = true;
                }
                else if (numParamStart < numParamEnd)
                {
                    LogASLError("Too many > in " + ReportErrorLine(_lines[i]), LogType.FatalError);
                    hasErrors = true;
                }
            }
        }

        // Exit if errors found
        if (hasErrors)
        {
            return true;
        }

        // Checks that define sections have parameters:
        for (int i = 1, loopTo1 = _numberSections; i <= loopTo1; i++)
        {
            curBegin = _defineBlocks[i].StartLine;
            curEnd = _defineBlocks[i].EndLine;

            if (BeginsWith(_lines[curBegin], "define game"))
            {
                if (Strings.InStr(_lines[curBegin], "<") == 0)
                {
                    LogASLError("'define game' has no parameter - game has no name", LogType.FatalError);
                    return true;
                }
            }
            else if (!BeginsWith(_lines[curBegin], "define synonyms") & !BeginsWith(_lines[curBegin], "define options"))
            {
                if (Strings.InStr(_lines[curBegin], "<") == 0)
                {
                    LogASLError(_lines[curBegin] + " has no parameter", LogType.FatalError);
                    hasErrors = true;
                }
            }
        }

        return hasErrors;
    }

    private string GetAfterParameter(string s)
    {
        // Returns everything after the end of the first parameter
        // in a string, i.e. for "use <thing> do <myproc>" it
        // returns "do <myproc>"
        int eop;
        eop = Strings.InStr(s, ">");

        if ((eop == 0) | (eop + 1 > Strings.Len(s)))
        {
            return "";
        }

        return Strings.Trim(Strings.Mid(s, eop + 1));
    }

    private string ObliterateParameters(string s)
    {
        bool inParameter;
        var exitCharacter = "";
        string curChar;
        var outputLine = "";
        var obscuringFunctionName = default(bool);

        inParameter = false;

        for (int i = 1, loopTo = Strings.Len(s); i <= loopTo; i++)
        {
            curChar = Strings.Mid(s, i, 1);

            if (inParameter)
            {
                if (exitCharacter == ")")
                {
                    if (Strings.InStr("$#%", curChar) > 0)
                    {
                        // We might be converting a line like:
                        // if ( $rand(1;10)$ < 3 ) then {
                        // and we don't want it to end up like this:
                        // if (~~~~~~~~~~~)$ <~~~~~~~~~~~
                        // which will cause all sorts of confustion. So,
                        // we get rid of everything between the $ characters
                        // in this case, and set a flag so we know what we're
                        // doing.
                        obscuringFunctionName = true;
                        exitCharacter = curChar;

                        // Move along please
                        outputLine = outputLine + "~";
                        i = i + 1;
                        curChar = Strings.Mid(s, i, 1);
                    }
                }
            }

            if (!inParameter)
            {
                outputLine = outputLine + curChar;
                if (curChar == "<")
                {
                    inParameter = true;
                    exitCharacter = ">";
                }

                if (curChar == "(")
                {
                    inParameter = true;
                    exitCharacter = ")";
                }
            }
            else if ((curChar ?? "") == (exitCharacter ?? ""))
            {
                if (!obscuringFunctionName)
                {
                    inParameter = false;
                    outputLine = outputLine + curChar;
                }
                else
                {
                    // We've finished obscuring the function name,
                    // now let's find the next ) as we were before
                    // we found this dastardly function
                    obscuringFunctionName = false;
                    exitCharacter = ")";
                    outputLine = outputLine + "~";
                }
            }
            else
            {
                outputLine = outputLine + "~";
            }
        }

        if (inParameter)
        {
            return "'<ERROR;" + exitCharacter + ";" + outputLine;
        }

        return outputLine;
    }

    private string ObliterateVariableNames(string s)
    {
        bool inParameter;
        var exitCharacter = "";
        var outputLine = "";
        string curChar;

        inParameter = false;

        for (int i = 1, loopTo = Strings.Len(s); i <= loopTo; i++)
        {
            curChar = Strings.Mid(s, i, 1);
            if (!inParameter)
            {
                outputLine = outputLine + curChar;
                if (curChar == "$")
                {
                    inParameter = true;
                    exitCharacter = "$";
                }

                if (curChar == "#")
                {
                    inParameter = true;
                    exitCharacter = "#";
                }

                if (curChar == "%")
                {
                    inParameter = true;
                    exitCharacter = "%";
                }

                // The ~ was for collectables, and this syntax only
                // exists in Quest 2.x. The ~ was only finally
                // allowed to be present on its own in ASL 320.
                if ((curChar == "~") & (ASLVersion < 320))
                {
                    inParameter = true;
                    exitCharacter = "~";
                }
            }
            else if ((curChar ?? "") == (exitCharacter ?? ""))
            {
                inParameter = false;
                outputLine = outputLine + curChar;
            }
            else
            {
                outputLine = outputLine + "X";
            }
        }

        if (inParameter)
        {
            outputLine = "'<ERROR;" + exitCharacter + ";" + outputLine;
        }

        return outputLine;
    }

    private void RemoveComments()
    {
        int aposPos;
        var inTextBlock = default(bool);
        var inSynonymsBlock = default(bool);
        string oblitLine;

        // If in a synonyms block, we want to remove lines which are comments, but
        // we don't want to remove synonyms that contain apostrophes, so we only
        // get rid of lines with an "'" at the beginning or with " '" in them

        for (int i = 1, loopTo = Information.UBound(_lines); i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "'!qdk-note:"))
            {
                _lines[i] = "#!qdk-note:" + GetEverythingAfter(_lines[i], "'!qdk-note:");
            }
            else
            {
                if (BeginsWith(_lines[i], "define text "))
                {
                    inTextBlock = true;
                }
                else if (Strings.Trim(_lines[i]) == "define synonyms")
                {
                    inSynonymsBlock = true;
                }
                else if (BeginsWith(_lines[i], "define type "))
                {
                    inSynonymsBlock = true;
                }
                else if (Strings.Trim(_lines[i]) == "end define")
                {
                    inTextBlock = false;
                    inSynonymsBlock = false;
                }

                if (!inTextBlock & !inSynonymsBlock)
                {
                    if (Strings.InStr(_lines[i], "'") > 0)
                    {
                        oblitLine = ObliterateParameters(_lines[i]);
                        if (!BeginsWith(oblitLine, "'<ERROR;"))
                        {
                            aposPos = Strings.InStr(oblitLine, "'");

                            if (aposPos != 0)
                            {
                                _lines[i] = Strings.Trim(Strings.Left(_lines[i], aposPos - 1));
                            }
                        }
                    }
                }
                else if (inSynonymsBlock)
                {
                    if (Strings.Left(Strings.Trim(_lines[i]), 1) == "'")
                    {
                        _lines[i] = "";
                    }
                    else
                    {
                        // we look for " '", not "'" in synonyms lines
                        aposPos = Strings.InStr(ObliterateParameters(_lines[i]), " '");
                        if (aposPos != 0)
                        {
                            _lines[i] = Strings.Trim(Strings.Left(_lines[i], aposPos - 1));
                        }
                    }
                }
            }
        }
    }

    private string ReportErrorLine(string s)
    {
        // We don't want to see the "!intproc" in logged error reports lines.
        // This function replaces these "do" lines with a nicer-looking "..." for error reporting.

        int replaceFrom;

        replaceFrom = Strings.InStr(s, "do <!intproc");
        if (replaceFrom != 0)
        {
            return Strings.Left(s, replaceFrom - 1) + "...";
        }

        return s;
    }

    private string YesNo(bool yn)
    {
        if (yn)
        {
            return "Yes";
        }

        return "No";
    }

    private bool IsYes(string yn)
    {
        if (Strings.LCase(yn) == "yes")
        {
            return true;
        }

        return false;
    }

    internal bool BeginsWith(string s, string text)
    {
        // Compares the beginning of the line with a given
        // string. Case insensitive.

        // Example: beginswith("Hello there","HeLlO")=TRUE

        return (Strings.Left(Strings.LTrim(Strings.LCase(s)), Strings.Len(text)) ?? "") == (Strings.LCase(text) ?? "");
    }

    private string ConvertCasKeyword(string casChar)
    {
        var c = Encoding.GetEncoding(1252).GetBytes(casChar)[0];
        var keyword = _casKeywords[c];

        if (keyword == "!cr")
        {
            keyword = Constants.vbCrLf;
        }

        return keyword;
    }

    private void ConvertMultiLines()
    {
        // Goes through each section capable of containing
        // script commands and puts any multiple-line script commands
        // into separate procedures. Also joins multiple-line "if"
        // statements.

        // This calls RemoveComments after joining lines, so that lines
        // with "'" as part of a multi-line parameter are not destroyed,
        // before looking for braces.

        for (var i = Information.UBound(_lines); i >= 1; i -= 1)
        {
            if (Strings.Right(_lines[i], 2) == "__")
            {
                _lines[i] = Strings.Left(_lines[i], Strings.Len(_lines[i]) - 2) + Strings.LTrim(_lines[i + 1]);
                _lines[i + 1] = "";
                // Recalculate this line again
                i = i + 1;
            }
            else if (Strings.Right(_lines[i], 1) == "_")
            {
                _lines[i] = Strings.Left(_lines[i], Strings.Len(_lines[i]) - 1) + Strings.LTrim(_lines[i + 1]);
                _lines[i + 1] = "";
                // Recalculate this line again
                i = i + 1;
            }
        }

        RemoveComments();
    }

    private DefineBlock GetDefineBlock(string blockname)
    {
        // Returns the start and end points of a named block.
        // Returns 0 if block not found.

        string l, blockType;

        var result = new DefineBlock();
        result.StartLine = 0;
        result.EndLine = 0;

        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            // Get the first line of the define section:
            l = _lines[_defineBlocks[i].StartLine];

            // Now, starting from the first word after 'define',
            // retrieve the next word and compare it to blockname:

            // Add a space for define blocks with no parameter
            if (Strings.InStr(8, l, " ") == 0)
            {
                l = l + " ";
            }

            blockType = Strings.Mid(l, 8, Strings.InStr(8, l, " ") - 8);

            if ((blockType ?? "") == (blockname ?? ""))
            {
                // Return the start and end points
                result.StartLine = _defineBlocks[i].StartLine;
                result.EndLine = _defineBlocks[i].EndLine;
                return result;
            }
        }

        return result;
    }

    private DefineBlock DefineBlockParam(string blockname, string param)
    {
        // Returns the start and end points of a named block

        Dictionary<string, string> cache;
        var result = new DefineBlock();

        param = "k" + param; // protect against numeric block names

        if (!_defineBlockParams.ContainsKey(blockname))
        {
            // Lazily create cache of define block params

            cache = new Dictionary<string, string>();
            _defineBlockParams.Add(blockname, cache);

            for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
            {
                // get the word after "define", e.g. "procedure"
                var blockType = GetEverythingAfter(_lines[_defineBlocks[i].StartLine], "define ");
                var sp = Strings.InStr(blockType, " ");
                if (sp != 0)
                {
                    blockType = Strings.Trim(Strings.Left(blockType, sp - 1));
                }

                if ((blockType ?? "") == (blockname ?? ""))
                {
                    var blockKey = GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext, false);

                    blockKey = "k" + blockKey;

                    if (!cache.ContainsKey(blockKey))
                    {
                        cache.Add(blockKey, _defineBlocks[i].StartLine + "," + _defineBlocks[i].EndLine);
                    }
                    // silently ignore duplicates
                }
            }
        }
        else
        {
            cache = _defineBlockParams[blockname];
        }

        if (cache.ContainsKey(param))
        {
            var blocks = Strings.Split(cache[param], ",");
            result.StartLine = Conversions.ToInteger(blocks[0]);
            result.EndLine = Conversions.ToInteger(blocks[1]);
        }

        return result;
    }

    internal string GetEverythingAfter(string s, string text)
    {
        if (Strings.Len(text) > Strings.Len(s))
        {
            return "";
        }

        return Strings.Right(s, Strings.Len(s) - Strings.Len(text));
    }

    private string Keyword2CAS(string KWord)
    {
        if (string.IsNullOrEmpty(KWord))
        {
            return "";
        }

        for (var i = 0; i <= 255; i++)
        {
            if ((Strings.LCase(KWord) ?? "") == (Strings.LCase(_casKeywords[i]) ?? ""))
            {
                return Conversions.ToString(Strings.Chr(i));
            }
        }

        return Keyword2CAS("!unknown") + KWord + Keyword2CAS("!unknown");
    }

    private void LoadCASKeywords()
    {
        // Loads data required for conversion of CAS files

        var questDatLines = GetResourceLines(Resources.GetResourceBytes(Resources.QuestDAT));

        foreach (var line in questDatLines)
        {
            if (Strings.Left(line, 1) != "#")
            {
                // Lines isn't a comment - so parse it.
                var scp = Strings.InStr(line, ";");
                var keyword = Strings.Trim(Strings.Left(line, scp - 1));
                var num = Conversions.ToInteger(Strings.Right(line, Strings.Len(line) - scp));
                _casKeywords[num] = keyword;
            }
        }
    }

    private string[] GetResourceLines(byte[] res)
    {
        var enc = new UTF8Encoding();
        var resFile = enc.GetString(res);
        return Strings.Split(resFile, "\r" + "\n");
    }

    private async Task<string> GetFileData(IGameData gameData)
    {
        var stream = gameData.Data;
        return await new StreamReader(stream).ReadToEndAsync();
    }

    private async Task<bool> ParseFile(IGameData gameData)
    {
        // Returns FALSE if failed.

        bool hasErrors;
        bool result;
        var libCode = new string[1];
        int libLines;
        bool ignoreMode, skipCheck;
        int c, d, l;
        int libFileHandle;
        string[] libResourceLines;
        string libFile;
        string libLine;
        int inDefGameBlock, gameLine = default;
        int inDefSynBlock, synLine = default;
        bool libFoundThisSweep;
        string libFileName;
        var libraryList = new string[1];
        int numLibraries;
        bool libraryAlreadyIncluded;
        int inDefTypeBlock;
        string typeBlockName;
        var typeLine = default(int);
        int defineCount, curLine;
        var filename = gameData.Filename;

        _defineBlockParams = new Dictionary<string, Dictionary<string, string>>();

        result = true;

        // Parses file and returns the positions of each main
        // 'define' block. Supports nested defines.

        if (Strings.LCase(Strings.Right(filename, 4)) == ".zip")
        {
            _originalFilename = filename;
            filename = GetUnzippedFile(filename);
            _gamePath = Path.GetDirectoryName(filename);
        }

        if ((Strings.LCase(Strings.Right(filename, 4)) == ".asl") |
            (Strings.LCase(Strings.Right(filename, 4)) == ".txt"))
        {
            // Read file into Lines array
            var fileData = await GetFileData(gameData);

            var aslLines = fileData.Split('\r');
            _lines = new string[aslLines.Length + 1];
            _lines[0] = "";

            var loopTo = aslLines.Length;
            for (l = 1; l <= loopTo; l++)
            {
                _lines[l] = aslLines[l - 1];
                _lines[l] = RemoveTabs(_lines[l]);
                _lines[l] = _lines[l].Trim(' ', '\n', '\r');
            }

            l = aslLines.Length;
        }

        else if (Strings.LCase(Strings.Right(filename, 4)) == ".cas")
        {
            LogASLError("Loading CAS");
            LoadCASFile(filename);
            l = Information.UBound(_lines);
        }

        else
        {
            throw new InvalidOperationException("Unrecognized file extension");
        }

        // Add libraries to end of code:

        numLibraries = 0;

        do
        {
            libFoundThisSweep = false;
            for (var i = l; i >= 1; i -= 1)
                // We search for includes backwards as a game might include
                // some-general.lib and then something-specific.lib which needs
                // something-general; if we include something-specific first,
                // then something-general afterwards, something-general's startscript
                // gets executed before something-specific's, as we execute the
                // lib startscripts backwards as well
            {
                if (BeginsWith(_lines[i], "!include "))
                {
                    libFileName = GetParameter(_lines[i], _nullContext);
                    // Clear !include statement
                    _lines[i] = "";
                    libraryAlreadyIncluded = false;
                    LogASLError("Including library '" + libFileName + "'...", LogType.Init);

                    for (int j = 1, loopTo1 = numLibraries; j <= loopTo1; j++)
                    {
                        if ((Strings.LCase(libFileName) ?? "") == (Strings.LCase(libraryList[j]) ?? ""))
                        {
                            libraryAlreadyIncluded = true;
                            break;
                        }
                    }

                    if (libraryAlreadyIncluded)
                    {
                        LogASLError("     - Library already included.", LogType.Init);
                    }
                    else
                    {
                        numLibraries = numLibraries + 1;
                        Array.Resize(ref libraryList, numLibraries + 1);
                        libraryList[numLibraries] = libFileName;

                        libFoundThisSweep = true;
                        libResourceLines = null;

                        libFile = _gamePath + libFileName;
                        LogASLError(" - Searching for " + libFile + " (game path)", LogType.Init);
                        libFileHandle = FileSystem.FreeFile();

                        if (File.Exists(libFile))
                        {
                            FileSystem.FileOpen(libFileHandle, libFile, OpenMode.Input);
                        }
                        else
                        {
                            // File was not found; try standard Quest libraries (stored here as resources)
                            LogASLError("     - Library not found in game path.", LogType.Init);
                            LogASLError(" - Searching for " + libFile + " (standard libraries)", LogType.Init);
                            libResourceLines = GetLibraryLines(libFileName);

                            if (libResourceLines is null)
                            {
                                LogASLError("Library not found.", LogType.FatalError);
                                _openErrorReport = _openErrorReport + "Library '" + libraryList[numLibraries] +
                                                   "' not found." + Constants.vbCrLf;
                                return false;
                            }
                        }

                        LogASLError("     - Found library, opening...", LogType.Init);

                        libLines = 0;

                        if (libResourceLines is null)
                        {
                            do
                            {
                                libLines = libLines + 1;
                                libLine = FileSystem.LineInput(libFileHandle);
                                libLine = RemoveTabs(libLine);
                                Array.Resize(ref libCode, libLines + 1);
                                libCode[libLines] = Strings.Trim(libLine);
                            } while (!FileSystem.EOF(libFileHandle));

                            FileSystem.FileClose(libFileHandle);
                        }
                        else
                        {
                            foreach (var resLibLine in libResourceLines)
                            {
                                libLines = libLines + 1;
                                Array.Resize(ref libCode, libLines + 1);
                                libLine = resLibLine;
                                libLine = RemoveTabs(libLine);
                                libCode[libLines] = Strings.Trim(libLine);
                            }
                        }

                        var libVer = -1;

                        if (libCode[1] == "!library")
                        {
                            var loopTo2 = libLines;
                            for (c = 1; c <= loopTo2; c++)
                            {
                                if (BeginsWith(libCode[c], "!asl-version "))
                                {
                                    libVer = Conversions.ToInteger(GetParameter(libCode[c], _nullContext));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Old library
                            libVer = 100;
                        }

                        if (libVer == -1)
                        {
                            LogASLError(" - Library has no asl-version information.", LogType.LibraryWarningError);
                            libVer = 200;
                        }

                        ignoreMode = false;
                        var loopTo3 = libLines;
                        for (c = 1; c <= loopTo3; c++)
                        {
                            if (BeginsWith(libCode[c], "!include "))
                            {
                                // Quest only honours !include in a library for asl-version
                                // 311 or later, as it ignored them in versions < 3.11
                                if (libVer >= 311)
                                {
                                    AddLine(libCode[c]);
                                    l = l + 1;
                                }
                            }
                            else if ((Strings.Left(libCode[c], 1) != "!") & (Strings.Left(libCode[c], 1) != "'") &
                                     !ignoreMode)
                            {
                                AddLine(libCode[c]);
                                l = l + 1;
                            }
                            else if (libCode[c] == "!addto game")
                            {
                                inDefGameBlock = 0;
                                var loopTo4 = Information.UBound(_lines);
                                for (d = 1; d <= loopTo4; d++)
                                {
                                    if (BeginsWith(_lines[d], "define game "))
                                    {
                                        inDefGameBlock = 1;
                                    }
                                    else if (BeginsWith(_lines[d], "define "))
                                    {
                                        if (inDefGameBlock != 0)
                                        {
                                            inDefGameBlock = inDefGameBlock + 1;
                                        }
                                    }
                                    else if ((_lines[d] == "end define") & (inDefGameBlock == 1))
                                    {
                                        gameLine = d;
                                        d = Information.UBound(_lines);
                                    }
                                    else if (_lines[d] == "end define")
                                    {
                                        if (inDefGameBlock != 0)
                                        {
                                            inDefGameBlock = inDefGameBlock - 1;
                                        }
                                    }
                                }

                                do
                                {
                                    c = c + 1;
                                    if (!BeginsWith(libCode[c], "!end"))
                                    {
                                        Array.Resize(ref _lines, Information.UBound(_lines) + 1 + 1);
                                        var loopTo5 = gameLine + 1;
                                        for (d = Information.UBound(_lines); d >= loopTo5; d -= 1)
                                        {
                                            _lines[d] = _lines[d - 1];
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
                                        if ((libVer >= 311) & BeginsWith(libCode[c], "startscript "))
                                        {
                                            _lines[gameLine] = "lib " + libCode[c];
                                        }
                                        else if ((libVer >= 392) & (BeginsWith(libCode[c], "command ") |
                                                                    BeginsWith(libCode[c], "verb ")))
                                        {
                                            _lines[gameLine] = "lib " + libCode[c];
                                        }
                                        else
                                        {
                                            _lines[gameLine] = libCode[c];
                                        }

                                        l = l + 1;
                                        gameLine = gameLine + 1;
                                    }
                                } while (!BeginsWith(libCode[c], "!end"));
                            }
                            else if (libCode[c] == "!addto synonyms")
                            {
                                inDefSynBlock = 0;
                                var loopTo6 = Information.UBound(_lines);
                                for (d = 1; d <= loopTo6; d++)
                                {
                                    if (_lines[d] == "define synonyms")
                                    {
                                        inDefSynBlock = 1;
                                    }
                                    else if ((_lines[d] == "end define") & (inDefSynBlock == 1))
                                    {
                                        synLine = d;
                                        d = Information.UBound(_lines);
                                    }
                                }

                                if (inDefSynBlock == 0)
                                {
                                    // No "define synonyms" block in game - so add it
                                    AddLine("define synonyms");
                                    AddLine("end define");
                                    synLine = Information.UBound(_lines);
                                }

                                do
                                {
                                    c = c + 1;
                                    if (!BeginsWith(libCode[c], "!end"))
                                    {
                                        Array.Resize(ref _lines, Information.UBound(_lines) + 1 + 1);
                                        var loopTo7 = synLine + 1;
                                        for (d = Information.UBound(_lines); d >= loopTo7; d -= 1)
                                        {
                                            _lines[d] = _lines[d - 1];
                                        }

                                        _lines[synLine] = libCode[c];
                                        l = l + 1;
                                        synLine = synLine + 1;
                                    }
                                } while (!BeginsWith(libCode[c], "!end"));
                            }
                            else if (BeginsWith(libCode[c], "!addto type "))
                            {
                                inDefTypeBlock = 0;
                                typeBlockName = Strings.LCase(GetParameter(libCode[c], _nullContext));
                                var loopTo8 = Information.UBound(_lines);
                                for (d = 1; d <= loopTo8; d++)
                                {
                                    if ((Strings.LCase(_lines[d]) ?? "") ==
                                        ("define type <" + typeBlockName + ">" ?? ""))
                                    {
                                        inDefTypeBlock = 1;
                                    }
                                    else if ((_lines[d] == "end define") & (inDefTypeBlock == 1))
                                    {
                                        typeLine = d;
                                        d = Information.UBound(_lines);
                                    }
                                }

                                if (inDefTypeBlock == 0)
                                {
                                    // No "define type (whatever)" block in game - so add it
                                    AddLine("define type <" + typeBlockName + ">");
                                    AddLine("end define");
                                    typeLine = Information.UBound(_lines);
                                }

                                do
                                {
                                    c = c + 1;
                                    if (c > libLines)
                                    {
                                        break;
                                    }

                                    if (!BeginsWith(libCode[c], "!end"))
                                    {
                                        Array.Resize(ref _lines, Information.UBound(_lines) + 1 + 1);
                                        var loopTo9 = typeLine + 1;
                                        for (d = Information.UBound(_lines); d >= loopTo9; d -= 1)
                                        {
                                            _lines[d] = _lines[d - 1];
                                        }

                                        _lines[typeLine] = libCode[c];
                                        l = l + 1;
                                        typeLine = typeLine + 1;
                                    }
                                } while (!BeginsWith(libCode[c], "!end"));
                            }


                            else if (libCode[c] == "!library")
                            {
                            }
                            // ignore
                            else if (BeginsWith(libCode[c], "!asl-version "))
                            {
                            }
                            // ignore
                            else if (BeginsWith(libCode[c], "'"))
                            {
                            }
                            // ignore
                            else if (BeginsWith(libCode[c], "!QDK"))
                            {
                                ignoreMode = true;
                            }
                            else if (BeginsWith(libCode[c], "!end"))
                            {
                                ignoreMode = false;
                            }
                        }
                    }
                }
            }
        } while (libFoundThisSweep);

        skipCheck = false;

        int lastSlashPos = default, slashPos;
        var curPos = 1;
        do
        {
            slashPos = Strings.InStr(curPos, filename, @"\");
            if (slashPos == 0)
            {
                slashPos = Strings.InStr(curPos, filename, "/");
            }

            if (slashPos != 0)
            {
                lastSlashPos = slashPos;
            }

            curPos = slashPos + 1;
        } while (slashPos != 0);

        var filenameNoPath = Strings.LCase(Strings.Mid(filename, lastSlashPos + 1));

        for (int i = 1, loopTo10 = _numSkipCheckFiles; i <= loopTo10; i++)
        {
            if ((filenameNoPath ?? "") == (_skipCheckFile[i] ?? ""))
            {
                skipCheck = true;
                break;
            }
        }

        if (filenameNoPath == "musicvf1.cas")
        {
            _useStaticFrameForPictures = true;
        }

        // RemoveComments called within ConvertMultiLines
        ConvertMultiLines();

        if (!skipCheck)
        {
            if (!CheckSections())
            {
                return false;
            }
        }

        _numberSections = 1;

        for (int i = 1, loopTo11 = l; i <= loopTo11; i++)
            // find section beginning with 'define'
        {
            if (BeginsWith(_lines[i], "define"))
            {
                // Now, go through until we reach an 'end define'. However, if we
                // encounter another 'define' there is a nested define. So, if we
                // encounter 'define' we increment the definecount. When we find an
                // 'end define' we decrement it. When definecount is zero, we have
                // found the end of the section.
                defineCount = 1;

                // Don't count the current line - we know it begins with 'define'...
                curLine = i + 1;
                do
                {
                    if (BeginsWith(_lines[curLine], "define"))
                    {
                        defineCount = defineCount + 1;
                    }
                    else if (BeginsWith(_lines[curLine], "end define"))
                    {
                        defineCount = defineCount - 1;
                    }

                    curLine = curLine + 1;
                } while (defineCount != 0);

                curLine = curLine - 1;

                // Now, we know that the define section begins at i and ends at
                // curline. Remember where the section begins and ends:
                Array.Resize(ref _defineBlocks, _numberSections + 1);
                _defineBlocks[_numberSections] = new DefineBlock();
                _defineBlocks[_numberSections].StartLine = i;
                _defineBlocks[_numberSections].EndLine = curLine;

                _numberSections = _numberSections + 1;
                i = curLine;
            }
        }

        _numberSections = _numberSections - 1;

        var gotGameBlock = false;
        for (int i = 1, loopTo12 = _numberSections; i <= loopTo12; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define game "))
            {
                gotGameBlock = true;
                break;
            }
        }

        if (!gotGameBlock)
        {
            _openErrorReport = _openErrorReport + "No 'define game' block." + Constants.vbCrLf;
            return false;
        }

        ConvertMultiLineSections();

        hasErrors = ConvertFriendlyIfs();
        if (!hasErrors)
        {
            hasErrors = ErrorCheck();
        }

        if (hasErrors)
        {
            throw new InvalidOperationException("Errors found in game file.");
        }

        _saveGameFile = "";

        return result;
    }

    internal void LogASLError(string err, LogType type = LogType.Misc)
    {
        if (type == LogType.FatalError)
        {
            err = "FATAL ERROR: " + err;
        }
        else if (type == LogType.WarningError)
        {
            err = "ERROR: " + err;
        }
        else if (type == LogType.LibraryWarningError)
        {
            err = "WARNING ERROR (LIBRARY): " + err;
        }
        else if (type == LogType.Init)
        {
            err = "INIT: " + err;
        }
        else if (type == LogType.Warning)
        {
            err = "WARNING: " + err;
        }
        else if (type == LogType.UserError)
        {
            err = "ERROR (REQUESTED): " + err;
        }
        else if (type == LogType.InternalError)
        {
            err = "INTERNAL ERROR: " + err;
        }

        _log.Add(err);
        _player.Log(err);
    }

    internal string GetParameter(string s, Context ctx, bool convertStringVariables = true)
    {
        // Returns the parameters between < and > in a string
        string newParam;
        int startPos;
        int endPos;

        startPos = Strings.InStr(s, "<");
        endPos = Strings.InStr(s, ">");

        if ((startPos == 0) | (endPos == 0))
        {
            LogASLError("Expected parameter in '" + ReportErrorLine(s) + "'", LogType.WarningError);
            return "";
        }

        var retrParam = Strings.Mid(s, startPos + 1, endPos - startPos - 1);

        if (convertStringVariables)
        {
            if (ASLVersion >= 320)
            {
                newParam = ConvertParameter(
                    ConvertParameter(ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%",
                        ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx);
            }
            else if (!(Strings.Left(retrParam, 9) == "~Internal"))
            {
                newParam = ConvertParameter(
                    ConvertParameter(
                        ConvertParameter(ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%",
                            ConvertType.Numeric, ctx), "~", ConvertType.Collectables, ctx), "$", ConvertType.Functions,
                    ctx);
            }
            else
            {
                newParam = retrParam;
            }
        }
        else
        {
            newParam = retrParam;
        }

        return EvaluateInlineExpressions(newParam);
    }

    private void AddLine(string line)
    {
        // Adds a line to the game script
        int numLines;

        numLines = Information.UBound(_lines) + 1;
        Array.Resize(ref _lines, numLines + 1);
        _lines[numLines] = line;
    }

    private string GetCASFileData(string filename)
    {
        return File.ReadAllText(filename, Encoding.GetEncoding(1252));
    }

    private void LoadCASFile(string filename)
    {
        bool endLineReached, exitTheLoop;
        var textMode = default(bool);
        int casVersion;
        var startCat = "";
        int endCatPos;
        string chkVer;
        var j = default(int);
        string curLin, textData;
        int cpos, nextLinePos;
        string c, tl, ckw, d;

        _lines = new string[1];

        var fileData = GetCASFileData(filename);

        chkVer = Strings.Left(fileData, 7);
        if (chkVer == "QCGF001")
        {
            casVersion = 1;
        }
        else if (chkVer == "QCGF002")
        {
            casVersion = 2;
        }
        else if (chkVer == "QCGF003")
        {
            casVersion = 3;
        }
        else
        {
            throw new InvalidOperationException("Invalid or corrupted CAS file.");
        }

        if (casVersion == 3)
        {
            startCat = Keyword2CAS("!startcat");
        }

        for (int i = 9, loopTo = Strings.Len(fileData); i <= loopTo; i++)
        {
            if ((casVersion == 3) & ((Strings.Mid(fileData, i, 1) ?? "") == (startCat ?? "")))
            {
                // Read catalog
                _startCatPos = i;
                endCatPos = Strings.InStr(j, fileData, Keyword2CAS("!endcat"));
                ReadCatalog(Strings.Mid(fileData, j + 1, endCatPos - j - 1));
                _resourceFile = filename;
                _resourceOffset = endCatPos + 1;
                i = Strings.Len(fileData);
                _casFileData = fileData;
            }
            else
            {
                curLin = "";
                endLineReached = false;
                if (textMode)
                {
                    textData = Strings.Mid(fileData, i,
                        Strings.InStr(i, fileData, Conversions.ToString(Strings.Chr(253))) - (i - 1));
                    textData = Strings.Left(textData, Strings.Len(textData) - 1);
                    cpos = 1;
                    var finished = false;

                    if (!string.IsNullOrEmpty(textData))
                    {
                        do
                        {
                            nextLinePos = Strings.InStr(cpos, textData, "\0");
                            if (nextLinePos == 0)
                            {
                                nextLinePos = Strings.Len(textData) + 1;
                                finished = true;
                            }

                            tl = DecryptString(Strings.Mid(textData, cpos, nextLinePos - cpos));
                            AddLine(tl);
                            cpos = nextLinePos + 1;
                        } while (!finished);
                    }

                    textMode = false;
                    i = Strings.InStr(i, fileData, Conversions.ToString(Strings.Chr(253)));
                }

                j = i;
                do
                {
                    ckw = Strings.Mid(fileData, j, 1);
                    c = ConvertCasKeyword(ckw);

                    if ((c ?? "") == Constants.vbCrLf)
                    {
                        endLineReached = true;
                    }
                    else if (Strings.Left(c, 1) != "!")
                    {
                        curLin = curLin + c + " ";
                    }
                    else if (c == "!quote")
                    {
                        exitTheLoop = false;
                        curLin = curLin + "<";
                        do
                        {
                            j = j + 1;
                            d = Strings.Mid(fileData, j, 1);
                            if (d != "\0")
                            {
                                curLin = curLin + DecryptString(d);
                            }
                            else
                            {
                                curLin = curLin + "> ";
                                exitTheLoop = true;
                            }
                        } while (!exitTheLoop);
                    }
                    else if (c == "!unknown")
                    {
                        exitTheLoop = false;
                        do
                        {
                            j = j + 1;
                            d = Strings.Mid(fileData, j, 1);
                            if (d != Conversions.ToString(Strings.Chr(254)))
                            {
                                curLin = curLin + d;
                            }
                            else
                            {
                                exitTheLoop = true;
                            }
                        } while (!exitTheLoop);

                        curLin = curLin + " ";
                    }

                    j = j + 1;
                } while (!endLineReached);

                AddLine(Strings.Trim(curLin));
                if (BeginsWith(curLin, "define text") | ((casVersion >= 2) & (BeginsWith(curLin, "define synonyms") |
                                                                              BeginsWith(curLin, "define type") |
                                                                              BeginsWith(curLin, "define menu"))))
                {
                    textMode = true;
                }

                // j is already at correct place, but i will be
                // incremented - so put j back one or we will miss a
                // character.
                i = j - 1;
            }
        }
    }

    private string DecryptString(string s)
    {
        var output = "";
        for (int i = 1, loopTo = Strings.Len(s); i <= loopTo; i++)
        {
            var v = Encoding.GetEncoding(1252).GetBytes(Strings.Mid(s, i, 1))[0];
            output = output + Strings.Chr(v ^ 255);
        }

        return output;
    }

    private string RemoveTabs(string s)
    {
        if (Strings.InStr(s, "\t") > 0)
        {
            // Remove tab characters and change them into
            // spaces; otherwise they bugger up the Trim
            // commands.
            var cpos = 1;
            var finished = false;
            do
            {
                var tabChar = Strings.InStr(cpos, s, "\t");
                if (tabChar != 0)
                {
                    s = Strings.Left(s, tabChar - 1) + Strings.Space(4) + Strings.Mid(s, tabChar + 1);
                    cpos = tabChar + 1;
                }
                else
                {
                    finished = true;
                }
            } while (!finished);
        }

        return s;
    }

    private void DoAddRemove(int childId, int parentId, bool add, Context ctx)
    {
        if (add)
        {
            AddToObjectProperties("parent=" + _objs[parentId].ObjectName, childId, ctx);
            _objs[childId].ContainerRoom = _objs[parentId].ContainerRoom;
        }
        else
        {
            AddToObjectProperties("not parent", childId, ctx);
        }

        if (ASLVersion >= 410)
            // Putting something in a container implicitly makes that
            // container "seen". Otherwise we could try to "look at" the
            // object we just put in the container and have disambigution fail!
        {
            AddToObjectProperties("seen", parentId, ctx);
        }

        UpdateVisibilityInContainers(ctx, _objs[parentId].ObjectName);
    }

    private void DoLook(int id, Context ctx, bool showExamineError = false, bool showDefaultDescription = true)
    {
        string objectContents;
        var foundLook = false;

        // First, set the "seen" property, and for ASL >= 391, update visibility for any
        // object that is contained by this object.

        if (ASLVersion >= 391)
        {
            AddToObjectProperties("seen", id, ctx);
            UpdateVisibilityInContainers(ctx, _objs[id].ObjectName);
        }

        // First look for action, then look
        // for property, then check define
        // section:

        string lookLine;
        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if (o.Actions[i].ActionName == "look")
            {
                foundLook = true;
                ExecuteScript(o.Actions[i].Script, ctx);
                break;
            }
        }

        if (!foundLook)
        {
            for (int i = 1, loopTo1 = o.NumberProperties; i <= loopTo1; i++)
            {
                if (o.Properties[i].PropertyName == "look")
                {
                    // do this odd RetrieveParameter stuff to convert any variables
                    Print(GetParameter("<" + o.Properties[i].PropertyValue + ">", ctx), ctx);
                    foundLook = true;
                    break;
                }
            }
        }

        if (!foundLook)
        {
            for (int i = o.DefinitionSectionStart, loopTo2 = o.DefinitionSectionEnd; i <= loopTo2; i++)
            {
                if (BeginsWith(_lines[i], "look "))
                {
                    lookLine = Strings.Trim(GetEverythingAfter(_lines[i], "look "));

                    if (Strings.Left(lookLine, 1) == "<")
                    {
                        Print(GetParameter(_lines[i], ctx), ctx);
                    }
                    else
                    {
                        ExecuteScript(lookLine, ctx, id);
                    }

                    foundLook = true;
                }
            }
        }

        if (ASLVersion >= 391)
        {
            objectContents = ListContents(id, ctx);
        }
        else
        {
            objectContents = "";
        }

        if (!foundLook & showDefaultDescription)
        {
            PlayerError err;

            if (showExamineError)
            {
                err = PlayerError.DefaultExamine;
            }
            else
            {
                err = PlayerError.DefaultLook;
            }

            // Print "Nothing out of the ordinary" or whatever, but only if we're not going to list
            // any contents.

            if (string.IsNullOrEmpty(objectContents))
            {
                PlayerErrorMessage(err, ctx);
            }
        }

        if (!string.IsNullOrEmpty(objectContents) & (objectContents != "<script>"))
        {
            Print(objectContents, ctx);
        }
    }

    private void DoOpenClose(int id, bool open, bool showLook, Context ctx)
    {
        if (open)
        {
            AddToObjectProperties("opened", id, ctx);
            if (showLook)
            {
                DoLook(id, ctx, showDefaultDescription: false);
            }
        }
        else
        {
            AddToObjectProperties("not opened", id, ctx);
        }

        UpdateVisibilityInContainers(ctx, _objs[id].ObjectName);
    }

    private string EvaluateInlineExpressions(string s)
    {
        // Evaluates in-line expressions e.g. msg <Hello, did you know that 2 + 2 = {2+2}?>

        if (ASLVersion < 391)
        {
            return s;
        }

        int bracePos;
        var curPos = 1;
        var resultLine = "";

        do
        {
            bracePos = Strings.InStr(curPos, s, "{");

            if (bracePos != 0)
            {
                resultLine = resultLine + Strings.Mid(s, curPos, bracePos - curPos);

                if (Strings.Mid(s, bracePos, 2) == "{{")
                {
                    // {{ = {
                    curPos = bracePos + 2;
                    resultLine = resultLine + "{";
                }
                else
                {
                    var EndBracePos = Strings.InStr(bracePos + 1, s, "}");
                    if (EndBracePos == 0)
                    {
                        LogASLError("Expected } in '" + s + "'", LogType.WarningError);
                        return "<ERROR>";
                    }

                    var expression = Strings.Mid(s, bracePos + 1, EndBracePos - bracePos - 1);
                    var expResult = ExpressionHandler(expression);
                    if (expResult.Success != ExpressionSuccess.OK)
                    {
                        LogASLError("Error evaluating expression in <" + s + "> - " + expResult.Message);
                        return "<ERROR>";
                    }

                    resultLine = resultLine + expResult.Result;
                    curPos = EndBracePos + 1;
                }
            }
            else
            {
                resultLine = resultLine + Strings.Mid(s, curPos);
            }
        } while (!((bracePos == 0) | (curPos > Strings.Len(s))));

        // Above, we only bothered checking for {{. But for consistency, also }} = }. So let's do that:
        curPos = 1;
        do
        {
            bracePos = Strings.InStr(curPos, resultLine, "}}");
            if (bracePos != 0)
            {
                resultLine = Strings.Left(resultLine, bracePos) + Strings.Mid(resultLine, bracePos + 2);
                curPos = bracePos + 1;
            }
        } while (!((bracePos == 0) | (curPos > Strings.Len(resultLine))));

        return resultLine;
    }

    private void ExecAddRemove(string cmd, Context ctx)
    {
        int childId;
        string childName;
        var doAdd = default(bool);
        int sepPos = default, parentId, sepLen = default;
        string parentName;
        var verb = "";
        string action;
        var foundAction = default(bool);
        var actionScript = "";
        bool propertyExists;
        string textToPrint;
        bool isContainer;
        bool gotObject;
        int childLength;
        var noParentSpecified = false;

        if (BeginsWith(cmd, "put "))
        {
            verb = "put";
            doAdd = true;
            sepPos = Strings.InStr(cmd, " on ");
            sepLen = 4;
            if (sepPos == 0)
            {
                sepPos = Strings.InStr(cmd, " in ");
                sepLen = 4;
            }

            if (sepPos == 0)
            {
                sepPos = Strings.InStr(cmd, " onto ");
                sepLen = 6;
            }
        }
        else if (BeginsWith(cmd, "add "))
        {
            verb = "add";
            doAdd = true;
            sepPos = Strings.InStr(cmd, " to ");
            sepLen = 4;
        }
        else if (BeginsWith(cmd, "remove "))
        {
            verb = "remove";
            doAdd = false;
            sepPos = Strings.InStr(cmd, " from ");
            sepLen = 6;
        }

        if (sepPos == 0)
        {
            noParentSpecified = true;
            sepPos = Strings.Len(cmd) + 1;
        }

        childLength = sepPos - (Strings.Len(verb) + 2);

        if (childLength < 0)
        {
            PlayerErrorMessage(PlayerError.BadCommand, ctx);
            _badCmdBefore = verb;
            return;
        }

        childName = Strings.Trim(Strings.Mid(cmd, Strings.Len(verb) + 2, childLength));

        gotObject = false;

        if ((ASLVersion >= 392) & doAdd)
        {
            childId = Disambiguate(childName, _currentRoom + ";inventory", ctx);

            if (childId > 0)
            {
                if (_objs[childId].ContainerRoom == "inventory")
                {
                    gotObject = true;
                }
                else
                {
                    // Player is not carrying the object they referred to. So, first take the object.
                    Print("(first taking " + _objs[childId].Article + ")", ctx);
                    // Try to take the object
                    ctx.AllowRealNamesInCommand = true;
                    ExecCommand("take " + _objs[childId].ObjectName, ctx, false, dontSetIt: true);

                    if (_objs[childId].ContainerRoom == "inventory")
                    {
                        gotObject = true;
                    }
                }

                if (!gotObject)
                {
                    _badCmdBefore = verb;
                    return;
                }
            }
            else
            {
                if (childId != -2)
                {
                    PlayerErrorMessage(PlayerError.NoItem, ctx);
                }

                _badCmdBefore = verb;
                return;
            }
        }

        else
        {
            childId = Disambiguate(childName, "inventory;" + _currentRoom, ctx);

            if (childId <= 0)
            {
                if (childId != -2)
                {
                    PlayerErrorMessage(PlayerError.BadThing, ctx);
                }

                _badCmdBefore = verb;
                return;
            }
        }

        if (noParentSpecified & doAdd)
        {
            SetStringContents("quest.error.article", _objs[childId].Article, ctx);
            PlayerErrorMessage(PlayerError.BadPut, ctx);
            return;
        }

        if (doAdd)
        {
            action = "add";
        }
        else
        {
            action = "remove";
        }

        if (!noParentSpecified)
        {
            parentName = Strings.Trim(Strings.Mid(cmd, sepPos + sepLen));

            parentId = Disambiguate(parentName, _currentRoom + ";inventory", ctx);

            if (parentId <= 0)
            {
                if (parentId != -2)
                {
                    PlayerErrorMessage(PlayerError.BadThing, ctx);
                }

                _badCmdBefore = Strings.Left(cmd, sepPos + sepLen);
                return;
            }
        }
        else
        {
            // Assume the player was referring to the parent that the object is already in,
            // if it is even in an object already

            if (!IsYes(GetObjectProperty("parent", childId, true, false)))
            {
                PlayerErrorMessage(PlayerError.CantRemove, ctx);
                return;
            }

            parentId = GetObjectIdNoAlias(GetObjectProperty("parent", childId));
        }

        // Check if parent is a container

        isContainer = IsYes(GetObjectProperty("container", parentId, true, false));

        if (!isContainer)
        {
            if (doAdd)
            {
                PlayerErrorMessage(PlayerError.CantPut, ctx);
            }
            else
            {
                PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }

            return;
        }

        // Check object is already held by that parent

        if (IsYes(GetObjectProperty("parent", childId, true, false)))
        {
            if (doAdd & ((Strings.LCase(GetObjectProperty("parent", childId, false, false)) ?? "") ==
                         (Strings.LCase(_objs[parentId].ObjectName) ?? "")))
            {
                PlayerErrorMessage(PlayerError.AlreadyPut, ctx);
            }
        }

        // Check parent and child are accessible to player
        var canAccessObject = PlayerCanAccessObject(childId);
        if (!canAccessObject.CanAccessObject)
        {
            if (doAdd)
            {
                PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessObject.ErrorMsg);
            }
            else
            {
                PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessObject.ErrorMsg);
            }

            return;
        }

        var canAccessParent = PlayerCanAccessObject(parentId);
        if (!canAccessParent.CanAccessObject)
        {
            if (doAdd)
            {
                PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessParent.ErrorMsg);
            }
            else
            {
                PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessParent.ErrorMsg);
            }

            return;
        }

        // Check if parent is a closed container

        if (!IsYes(GetObjectProperty("surface", parentId, true, false)) &
            !IsYes(GetObjectProperty("opened", parentId, true, false)))
        {
            // Not a surface and not open, so can't add to this closed container.
            if (doAdd)
            {
                PlayerErrorMessage(PlayerError.CantPut, ctx);
            }
            else
            {
                PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }

            return;
        }

        // Now check if it can be added to (or removed from)

        // First check for an action
        var o = _objs[parentId];
        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((Strings.LCase(o.Actions[i].ActionName) ?? "") == (action ?? ""))
            {
                foundAction = true;
                actionScript = o.Actions[i].Script;
                break;
            }
        }

        if (foundAction)
        {
            SetStringContents("quest." + Strings.LCase(action) + ".object.name", _objs[childId].ObjectName, ctx);
            ExecuteScript(actionScript, ctx, parentId);
        }
        else
        {
            // Now check for a property
            propertyExists = IsYes(GetObjectProperty(action, parentId, true, false));

            if (!propertyExists)
            {
                // Show error message
                if (doAdd)
                {
                    PlayerErrorMessage(PlayerError.CantPut, ctx);
                }
                else
                {
                    PlayerErrorMessage(PlayerError.CantRemove, ctx);
                }
            }
            else
            {
                textToPrint = GetObjectProperty(action, parentId, false, false);
                if (string.IsNullOrEmpty(textToPrint))
                {
                    // Show default message
                    if (doAdd)
                    {
                        PlayerErrorMessage(PlayerError.DefaultPut, ctx);
                    }
                    else
                    {
                        PlayerErrorMessage(PlayerError.DefaultRemove, ctx);
                    }
                }
                else
                {
                    Print(textToPrint, ctx);
                }

                DoAddRemove(childId, parentId, doAdd, ctx);
            }
        }
    }

    private void ExecAddRemoveScript(string parameter, bool add, Context ctx)
    {
        int childId, parentId = default;
        string commandName;
        string childName;
        var parentName = "";
        int scp;

        if (add)
        {
            commandName = "add";
        }
        else
        {
            commandName = "remove";
        }

        scp = Strings.InStr(parameter, ";");
        if ((scp == 0) & add)
        {
            LogASLError("No parent specified in '" + commandName + " <" + parameter + ">", LogType.WarningError);
            return;
        }

        if (scp != 0)
        {
            childName = Strings.LCase(Strings.Trim(Strings.Left(parameter, scp - 1)));
            parentName = Strings.LCase(Strings.Trim(Strings.Mid(parameter, scp + 1)));
        }
        else
        {
            childName = Strings.LCase(Strings.Trim(parameter));
        }

        childId = GetObjectIdNoAlias(childName);
        if (childId == 0)
        {
            LogASLError("Invalid child object name specified in '" + commandName + " <" + parameter + ">",
                LogType.WarningError);
            return;
        }

        if (scp != 0)
        {
            parentId = GetObjectIdNoAlias(parentName);
            if (parentId == 0)
            {
                LogASLError("Invalid parent object name specified in '" + commandName + " <" + parameter + ">",
                    LogType.WarningError);
                return;
            }

            DoAddRemove(childId, parentId, add, ctx);
        }
        else
        {
            AddToObjectProperties("not parent", childId, ctx);
            UpdateVisibilityInContainers(ctx, _objs[parentId].ObjectName);
        }
    }

    private void ExecOpenClose(string cmd, Context ctx)
    {
        int id;
        string name;
        var doOpen = default(bool);
        bool isOpen, foundAction = default;
        var action = "";
        var actionScript = "";
        bool propertyExists;
        string textToPrint;
        bool isContainer;

        if (BeginsWith(cmd, "open "))
        {
            action = "open";
            doOpen = true;
        }
        else if (BeginsWith(cmd, "close "))
        {
            action = "close";
            doOpen = false;
        }

        name = GetEverythingAfter(cmd, action + " ");

        id = Disambiguate(name, _currentRoom + ";inventory", ctx);

        if (id <= 0)
        {
            if (id != -2)
            {
                PlayerErrorMessage(PlayerError.BadThing, ctx);
            }

            _badCmdBefore = action;
            return;
        }

        // Check if it's even a container

        isContainer = IsYes(GetObjectProperty("container", id, true, false));

        if (!isContainer)
        {
            if (doOpen)
            {
                PlayerErrorMessage(PlayerError.CantOpen, ctx);
            }
            else
            {
                PlayerErrorMessage(PlayerError.CantClose, ctx);
            }

            return;
        }

        // Check if it's already open (or closed)

        isOpen = IsYes(GetObjectProperty("opened", id, true, false));

        if (doOpen & isOpen)
        {
            // Object is already open
            PlayerErrorMessage(PlayerError.AlreadyOpen, ctx);
            return;
        }

        if (!doOpen & !isOpen)
        {
            // Object is already closed
            PlayerErrorMessage(PlayerError.AlreadyClosed, ctx);
            return;
        }

        // Check if it's accessible, i.e. check it's not itself inside another closed container

        var canAccessObject = PlayerCanAccessObject(id);
        if (!canAccessObject.CanAccessObject)
        {
            if (doOpen)
            {
                PlayerErrorMessage_ExtendInfo(PlayerError.CantOpen, ctx, canAccessObject.ErrorMsg);
            }
            else
            {
                PlayerErrorMessage_ExtendInfo(PlayerError.CantClose, ctx, canAccessObject.ErrorMsg);
            }

            return;
        }

        // Now check if it can be opened (or closed)

        // First check for an action
        var o = _objs[id];
        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((Strings.LCase(o.Actions[i].ActionName) ?? "") == (action ?? ""))
            {
                foundAction = true;
                actionScript = o.Actions[i].Script;
                break;
            }
        }

        if (foundAction)
        {
            ExecuteScript(actionScript, ctx, id);
        }
        else
        {
            // Now check for a property
            propertyExists = IsYes(GetObjectProperty(action, id, true, false));

            if (!propertyExists)
            {
                // Show error message
                if (doOpen)
                {
                    PlayerErrorMessage(PlayerError.CantOpen, ctx);
                }
                else
                {
                    PlayerErrorMessage(PlayerError.CantClose, ctx);
                }
            }
            else
            {
                textToPrint = GetObjectProperty(action, id, false, false);
                if (string.IsNullOrEmpty(textToPrint))
                {
                    // Show default message
                    if (doOpen)
                    {
                        PlayerErrorMessage(PlayerError.DefaultOpen, ctx);
                    }
                    else
                    {
                        PlayerErrorMessage(PlayerError.DefaultClose, ctx);
                    }
                }
                else
                {
                    Print(textToPrint, ctx);
                }

                DoOpenClose(id, doOpen, true, ctx);
            }
        }
    }

    private void ExecuteSelectCase(string script, Context ctx)
    {
        // ScriptLine passed will look like this:
        // select case <whatever> do <!intprocX>
        // with all the case statements in the intproc.

        var afterLine = GetAfterParameter(script);

        if (!BeginsWith(afterLine, "do <!intproc"))
        {
            LogASLError("No case block specified for '" + script + "'", LogType.WarningError);
            return;
        }

        var blockName = GetParameter(afterLine, ctx);
        var block = DefineBlockParam("procedure", blockName);
        var checkValue = GetParameter(script, ctx);
        var caseMatch = false;

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
            // Go through all the cases until we find the one that matches
        {
            if (!string.IsNullOrEmpty(_lines[i]))
            {
                if (!BeginsWith(_lines[i], "case "))
                {
                    LogASLError("Invalid line in 'select case' block: '" + _lines[i] + "'", LogType.WarningError);
                }
                else
                {
                    var caseScript = "";

                    if (BeginsWith(_lines[i], "case else "))
                    {
                        caseMatch = true;
                        caseScript = GetEverythingAfter(_lines[i], "case else ");
                    }
                    else
                    {
                        var thisCase = GetParameter(_lines[i], ctx);
                        var finished = false;

                        do
                        {
                            var SCP = Strings.InStr(thisCase, ";");
                            if (SCP == 0)
                            {
                                SCP = Strings.Len(thisCase) + 1;
                                finished = true;
                            }

                            var condition = Strings.Trim(Strings.Left(thisCase, SCP - 1));
                            if ((condition ?? "") == (checkValue ?? ""))
                            {
                                caseScript = GetAfterParameter(_lines[i]);
                                caseMatch = true;
                                finished = true;
                            }
                            else
                            {
                                thisCase = Strings.Mid(thisCase, SCP + 1);
                            }
                        } while (!finished);
                    }

                    if (caseMatch)
                    {
                        ExecuteScript(caseScript, ctx);
                        return;
                    }
                }
            }
        }
    }

    private bool ExecVerb(string cmd, Context ctx, bool libCommands = false)
    {
        DefineBlock gameBlock;
        var foundVerb = false;
        var verbProperty = "";
        var script = "";
        string verbsList;
        var thisVerb = "";
        int scp;
        int id;
        var verbObject = "";
        string verbTag;
        var thisScript = "";

        if (!libCommands)
        {
            verbTag = "verb ";
        }
        else
        {
            verbTag = "lib verb ";
        }

        gameBlock = GetDefineBlock("game");
        for (int i = gameBlock.StartLine + 1, loopTo = gameBlock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], verbTag))
            {
                verbsList = GetParameter(_lines[i], ctx);

                // The property or action the verb uses is either after a colon,
                // or it's the first (or only) verb on the line.
                var colonPos = Strings.InStr(verbsList, ":");
                if (colonPos != 0)
                {
                    verbProperty = Strings.LCase(Strings.Trim(Strings.Mid(verbsList, colonPos + 1)));
                    verbsList = Strings.Trim(Strings.Left(verbsList, colonPos - 1));
                }
                else
                {
                    scp = Strings.InStr(verbsList, ";");
                    if (scp == 0)
                    {
                        verbProperty = Strings.LCase(verbsList);
                    }
                    else
                    {
                        verbProperty = Strings.LCase(Strings.Trim(Strings.Left(verbsList, scp - 1)));
                    }
                }

                // Now let's see if this matches:
                do
                {
                    scp = Strings.InStr(verbsList, ";");
                    if (scp == 0)
                    {
                        thisVerb = Strings.LCase(verbsList);
                    }
                    else
                    {
                        thisVerb = Strings.LCase(Strings.Trim(Strings.Left(verbsList, scp - 1)));
                    }

                    if (BeginsWith(cmd, thisVerb + " "))
                    {
                        foundVerb = true;
                        verbObject = GetEverythingAfter(cmd, thisVerb + " ");
                        script = Strings.Trim(Strings.Mid(_lines[i], Strings.InStr(_lines[i], ">") + 1));
                    }

                    if (scp != 0)
                    {
                        verbsList = Strings.Trim(Strings.Mid(verbsList, scp + 1));
                    }
                } while (!((scp == 0) | string.IsNullOrEmpty(Strings.Trim(verbsList)) | foundVerb));

                if (foundVerb)
                {
                    break;
                }
            }
        }

        if (foundVerb)
        {
            id = Disambiguate(verbObject, "inventory;" + _currentRoom, ctx);

            if (id < 0)
            {
                if (id != -2)
                {
                    PlayerErrorMessage(PlayerError.BadThing, ctx);
                }

                _badCmdBefore = thisVerb;
            }
            else
            {
                SetStringContents("quest.error.article", _objs[id].Article, ctx);

                var foundAction = false;

                // Now see if this object has the relevant action or property
                var o = _objs[id];
                for (int i = 1, loopTo1 = o.NumberActions; i <= loopTo1; i++)
                {
                    if ((Strings.LCase(o.Actions[i].ActionName) ?? "") == (verbProperty ?? ""))
                    {
                        foundAction = true;
                        thisScript = o.Actions[i].Script;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(thisScript))
                    // Avoid an RTE "this array is fixed or temporarily locked"
                {
                    ExecuteScript(thisScript, ctx, id);
                }

                if (!foundAction)
                    // Check properties for a message
                {
                    for (int i = 1, loopTo2 = o.NumberProperties; i <= loopTo2; i++)
                    {
                        if ((Strings.LCase(o.Properties[i].PropertyName) ?? "") == (verbProperty ?? ""))
                        {
                            foundAction = true;
                            Print(o.Properties[i].PropertyValue, ctx);
                            break;
                        }
                    }
                }

                if (!foundAction)
                    // Execute the default script from the verb definition
                {
                    ExecuteScript(script, ctx);
                }
            }
        }

        return foundVerb;
    }

    private ExpressionResult ExpressionHandler(string expr)
    {
        int openBracketPos, endBracketPos;
        var res = new ExpressionResult();

        // Find brackets, recursively call ExpressionHandler
        do
        {
            openBracketPos = Strings.InStr(expr, "(");
            if (openBracketPos != 0)
            {
                // Find equivalent closing bracket
                var BracketCount = 1;
                endBracketPos = 0;
                for (int i = openBracketPos + 1, loopTo = Strings.Len(expr); i <= loopTo; i++)
                {
                    if (Strings.Mid(expr, i, 1) == "(")
                    {
                        BracketCount = BracketCount + 1;
                    }
                    else if (Strings.Mid(expr, i, 1) == ")")
                    {
                        BracketCount = BracketCount - 1;
                    }

                    if (BracketCount == 0)
                    {
                        endBracketPos = i;
                        break;
                    }
                }

                if (endBracketPos != 0)
                {
                    var NestedResult =
                        ExpressionHandler(Strings.Mid(expr, openBracketPos + 1, endBracketPos - openBracketPos - 1));
                    if (NestedResult.Success != ExpressionSuccess.OK)
                    {
                        res.Success = NestedResult.Success;
                        res.Message = NestedResult.Message;
                        return res;
                    }

                    expr = Strings.Left(expr, openBracketPos - 1) + " " + NestedResult.Result + " " +
                           Strings.Mid(expr, endBracketPos + 1);
                }

                else
                {
                    res.Message = "Missing closing bracket";
                    res.Success = ExpressionSuccess.Fail;
                    return res;
                }
            }
        } while (openBracketPos != 0);

        // Split expression into elements, e.g.:
        // 2 + 3 * 578.2 / 36
        // E O E O EEEEE O EE      where E=Element, O=Operator

        var numElements = 1;
        string[] elements;
        elements = new string[2];
        var numOperators = 0;
        var operators = new string[1];
        bool newElement;

        var obscuredExpr = ObscureNumericExps(expr);

        for (int i = 1, loopTo1 = Strings.Len(expr); i <= loopTo1; i++)
        {
            switch (Strings.Mid(obscuredExpr, i, 1) ?? "")
            {
                case "+":
                case "*":
                case "/":
                {
                    newElement = true;
                    break;
                }
                case "-":
                {
                    // A minus often means subtraction, so it's a new element. But sometimes
                    // it just denotes a negative number. In this case, the current element will
                    // be empty.

                    if (string.IsNullOrEmpty(Strings.Trim(elements[numElements])))
                    {
                        newElement = false;
                    }
                    else
                    {
                        newElement = true;
                    }

                    break;
                }

                default:
                {
                    newElement = false;
                    break;
                }
            }

            if (newElement)
            {
                numElements = numElements + 1;
                Array.Resize(ref elements, numElements + 1);

                numOperators = numOperators + 1;
                Array.Resize(ref operators, numOperators + 1);
                operators[numOperators] = Strings.Mid(expr, i, 1);
            }
            else
            {
                elements[numElements] = elements[numElements] + Strings.Mid(expr, i, 1);
            }
        }

        // Check Elements are numeric, and trim spaces
        for (int i = 1, loopTo2 = numElements; i <= loopTo2; i++)
        {
            elements[i] = Strings.Trim(elements[i]);

            if (!Information.IsNumeric(elements[i]))
            {
                res.Message = "Syntax error evaluating expression - non-numeric element '" + elements[i] + "'";
                res.Success = ExpressionSuccess.Fail;
                return res;
            }
        }

        var opNum = 0;

        var result = default(double);
        do
        {
            // Go through the Operators array to find next calculation to perform

            for (int i = 1, loopTo3 = numOperators; i <= loopTo3; i++)
            {
                if ((operators[i] == "/") | (operators[i] == "*"))
                {
                    opNum = i;
                    break;
                }
            }

            if (opNum == 0)
            {
                for (int i = 1, loopTo4 = numOperators; i <= loopTo4; i++)
                {
                    if ((operators[i] == "+") | (operators[i] == "-"))
                    {
                        opNum = i;
                        break;
                    }
                }
            }

            // If OpNum is still 0, there are no calculations left to do.

            if (opNum != 0)
            {
                var val1 = Conversions.ToDouble(elements[opNum]);
                var val2 = Conversions.ToDouble(elements[opNum + 1]);

                switch (operators[opNum] ?? "")
                {
                    case "/":
                    {
                        if (val2 == 0d)
                        {
                            res.Message = "Division by zero";
                            res.Success = ExpressionSuccess.Fail;
                            return res;
                        }

                        result = val1 / val2;
                        break;
                    }
                    case "*":
                    {
                        result = val1 * val2;
                        break;
                    }
                    case "+":
                    {
                        result = val1 + val2;
                        break;
                    }
                    case "-":
                    {
                        result = val1 - val2;
                        break;
                    }
                }

                elements[opNum] = result.ToString();

                // Remove this operator, and Elements(OpNum+1) from the arrays
                for (int i = opNum, loopTo5 = numOperators - 1; i <= loopTo5; i++)
                {
                    operators[i] = operators[i + 1];
                }

                for (int i = opNum + 1, loopTo6 = numElements - 1; i <= loopTo6; i++)
                {
                    elements[i] = elements[i + 1];
                }

                numOperators = numOperators - 1;
                numElements = numElements - 1;
                Array.Resize(ref operators, numOperators + 1);
                Array.Resize(ref elements, numElements + 1);
            }
        } while (!((opNum == 0) | (numOperators == 0)));

        res.Success = ExpressionSuccess.OK;
        res.Result = elements[1];
        return res;
    }

    private string ListContents(int id, Context ctx)
    {
        // Returns a formatted list of the contents of a container.
        // If the list action causes a script to be run instead, ListContents
        // returns "<script>"

        var contentsIDs = new int[1];

        if (!IsYes(GetObjectProperty("container", id, true, false)))
        {
            return "";
        }

        if (!IsYes(GetObjectProperty("opened", id, true, false)) &
            !IsYes(GetObjectProperty("transparent", id, true, false)) &
            !IsYes(GetObjectProperty("surface", id, true, false)))
        {
            // Container is closed, so return "list closed" property if there is one.

            if (DoAction(id, "list closed", ctx, false))
            {
                return "<script>";
            }

            return GetObjectProperty("list closed", id, false, false);
        }

        // populate contents string

        var numContents = 0;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if (_objs[i].Exists & _objs[i].Visible)
            {
                if ((Strings.LCase(GetObjectProperty("parent", i, false, false)) ?? "") ==
                    (Strings.LCase(_objs[id].ObjectName) ?? ""))
                {
                    numContents = numContents + 1;
                    Array.Resize(ref contentsIDs, numContents + 1);
                    contentsIDs[numContents] = i;
                }
            }
        }

        var contents = "";

        if (numContents > 0)
        {
            // Check if list property is set.

            if (DoAction(id, "list", ctx, false))
            {
                return "<script>";
            }

            if (IsYes(GetObjectProperty("list", id, true, false)))
            {
                // Read header, if any
                var listString = GetObjectProperty("list", id, false, false);
                var displayList = true;

                if (!string.IsNullOrEmpty(listString))
                {
                    if (Strings.Right(listString, 1) == ":")
                    {
                        contents = Strings.Left(listString, Strings.Len(listString) - 1) + " ";
                    }
                    else
                    {
                        // If header doesn't end in a colon, then the header is the only text to print
                        contents = listString;
                        displayList = false;
                    }
                }
                else
                {
                    contents = Strings.UCase(Strings.Left(_objs[id].Article, 1)) + Strings.Mid(_objs[id].Article, 2) +
                               " contains ";
                }

                if (displayList)
                {
                    for (int i = 1, loopTo1 = numContents; i <= loopTo1; i++)
                    {
                        if (i > 1)
                        {
                            if (i < numContents)
                            {
                                contents = contents + ", ";
                            }
                            else
                            {
                                contents = contents + " and ";
                            }
                        }

                        var o = _objs[contentsIDs[i]];
                        if (!string.IsNullOrEmpty(o.Prefix))
                        {
                            contents = contents + o.Prefix;
                        }

                        if (!string.IsNullOrEmpty(o.ObjectAlias))
                        {
                            contents = contents + "|b" + o.ObjectAlias + "|xb";
                        }
                        else
                        {
                            contents = contents + "|b" + o.ObjectName + "|xb";
                        }

                        if (!string.IsNullOrEmpty(o.Suffix))
                        {
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

        if (DoAction(id, "list empty", ctx, false))
        {
            return "<script>";
        }

        return GetObjectProperty("list empty", id, false, false);
    }

    private string ObscureNumericExps(string s)
    {
        // Obscures + or - next to E in Double-type variables with exponents
        // e.g. 2.345E+20 becomes 2.345EX20
        // This stops huge numbers breaking parsing of maths functions

        int ep;
        var result = s;

        var pos = 1;
        do
        {
            ep = Strings.InStr(pos, result, "E");
            if (ep != 0)
            {
                result = Strings.Left(result, ep) + "X" + Strings.Mid(result, ep + 2);
                pos = ep + 2;
            }
        } while (ep != 0);

        return result;
    }

    private void ProcessListInfo(string line, int id)
    {
        var listInfo = new TextAction();
        var propName = "";

        if (BeginsWith(line, "list closed <"))
        {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = GetParameter(line, _nullContext);
            propName = "list closed";
        }
        else if (Strings.Trim(line) == "list closed off")
        {
            // default for list closed is off anyway
            return;
        }
        else if (BeginsWith(line, "list closed"))
        {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = GetEverythingAfter(line, "list closed");
            propName = "list closed";
        }


        else if (BeginsWith(line, "list empty <"))
        {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = GetParameter(line, _nullContext);
            propName = "list empty";
        }
        else if (Strings.Trim(line) == "list empty off")
        {
            // default for list empty is off anyway
            return;
        }
        else if (BeginsWith(line, "list empty"))
        {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = GetEverythingAfter(line, "list empty");
            propName = "list empty";
        }


        else if (Strings.Trim(line) == "list off")
        {
            AddToObjectProperties("not list", id, _nullContext);
            return;
        }
        else if (BeginsWith(line, "list <"))
        {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = GetParameter(line, _nullContext);
            propName = "list";
        }
        else if (BeginsWith(line, "list "))
        {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = GetEverythingAfter(line, "list ");
            propName = "list";
        }

        if (!string.IsNullOrEmpty(propName))
        {
            if (listInfo.Type == TextActionType.Text)
            {
                AddToObjectProperties(propName + "=" + listInfo.Data, id, _nullContext);
            }
            else
            {
                AddToObjectActions("<" + propName + "> " + listInfo.Data, id, _nullContext);
            }
        }
    }

    private string GetHTMLColour(string colour, string defaultColour)
    {
        // Converts a Quest foreground or background colour setting into an HTML colour

        colour = Strings.LCase(colour);

        if (string.IsNullOrEmpty(colour) | (colour == "0"))
        {
            colour = defaultColour;
        }

        switch (colour ?? "")
        {
            case "white":
            {
                return "FFFFFF";
            }
            case "black":
            {
                return "000000";
            }
            case "blue":
            {
                return "0000FF";
            }
            case "yellow":
            {
                return "FFFF00";
            }
            case "red":
            {
                return "FF0000";
            }
            case "green":
            {
                return "00FF00";
            }

            default:
            {
                return colour;
            }
        }
    }

    private void DoPrint(string text)
    {
        PrintText?.Invoke(_textFormatter.OutputHTML(text));
    }

    private void DestroyExit(string exitData, Context ctx)
    {
        var fromRoom = "";
        var toRoom = "";
        int roomId = default, exitId = default;

        var scp = Strings.InStr(exitData, ";");
        if (scp == 0)
        {
            LogASLError("No exit name specified in 'destroy exit <" + exitData + ">'");
            return;
        }

        RoomExit roomExit;
        if (ASLVersion >= 410)
        {
            roomExit = FindExit(exitData);
            if (roomExit is null)
            {
                LogASLError("Can't find exit in 'destroy exit <" + exitData + ">'");
                return;
            }

            roomExit.GetParent().RemoveExit(ref roomExit);
        }

        else
        {
            fromRoom = Strings.LCase(Strings.Trim(Strings.Left(exitData, scp - 1)));
            toRoom = Strings.Trim(Strings.Mid(exitData, scp + 1));

            // Find From Room:
            var found = false;

            for (int i = 1, loopTo = _numberRooms; i <= loopTo; i++)
            {
                if ((Strings.LCase(_rooms[i].RoomName) ?? "") == (fromRoom ?? ""))
                {
                    found = true;
                    roomId = i;
                    break;
                }
            }

            if (!found)
            {
                LogASLError("No such room '" + fromRoom + "'");
                return;
            }

            found = false;
            var r = _rooms[roomId];

            for (int i = 1, loopTo1 = r.NumberPlaces; i <= loopTo1; i++)
            {
                if ((r.Places[i].PlaceName ?? "") == (toRoom ?? ""))
                {
                    exitId = i;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                for (int i = exitId, loopTo2 = r.NumberPlaces - 1; i <= loopTo2; i++)
                {
                    r.Places[i] = r.Places[i + 1];
                }

                Array.Resize(ref r.Places, r.NumberPlaces);
                r.NumberPlaces = r.NumberPlaces - 1;
            }
        }

        // Update quest.* vars and obj list
        ShowRoomInfo(_currentRoom, ctx, true);
        UpdateObjectList(ctx);

        AddToChangeLog("room " + fromRoom, "destroy exit " + toRoom);
    }

    private void DoClear()
    {
        _player.ClearScreen();
    }

    private void DoWait()
    {
        _player.DoWait();
        ChangeState(State.Waiting);

        lock (_waitLock)
        {
            Monitor.Wait(_waitLock);
        }
    }

    private void ExecuteFlag(string line, Context ctx)
    {
        var propertyString = "";

        if (BeginsWith(line, "on "))
        {
            propertyString = GetParameter(line, ctx);
        }
        else if (BeginsWith(line, "off "))
        {
            propertyString = "not " + GetParameter(line, ctx);
        }

        // Game object always has ObjID 1
        AddToObjectProperties(propertyString, 1, ctx);
    }

    private bool ExecuteIfFlag(string flag)
    {
        // Game ObjID is 1
        return GetObjectProperty(flag, 1, true) == "yes";
    }

    private void ExecuteIncDec(string line, Context ctx)
    {
        string variable;
        double change;
        var param = GetParameter(line, ctx);

        var sc = Strings.InStr(param, ";");
        if (sc == 0)
        {
            change = 1d;
            variable = param;
        }
        else
        {
            change = Conversion.Val(Strings.Mid(param, sc + 1));
            variable = Strings.Trim(Strings.Left(param, sc - 1));
        }

        var value = GetNumericContents(variable, ctx, true);
        if (value <= -32766)
        {
            value = 0d;
        }

        if (BeginsWith(line, "inc "))
        {
            value = value + change;
        }
        else if (BeginsWith(line, "dec "))
        {
            value = value - change;
        }

        var arrayIndex = GetArrayIndex(variable, ctx);
        SetNumericVariableContents(arrayIndex.Name, value, ctx, arrayIndex.Index);
    }

    private string ExtractFile(string file)
    {
        int length = default, startPos = default;
        var extracted = default(bool);
        var resId = default(int);

        if (string.IsNullOrEmpty(_resourceFile))
        {
            return "";
        }

        // Find file in catalog

        var found = false;

        for (int i = 1, loopTo = _numResources; i <= loopTo; i++)
        {
            if ((Strings.LCase(file) ?? "") == (Strings.LCase(_resources[i].ResourceName) ?? ""))
            {
                found = true;
                startPos = _resources[i].ResourceStart + _resourceOffset;
                length = _resources[i].ResourceLength;
                extracted = _resources[i].Extracted;
                resId = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("Unable to extract '" + file + "' - not present in resources.", LogType.WarningError);
            return null;
        }

        var fileName = Path.Combine(TempFolder, file);
        Directory.CreateDirectory(Path.GetDirectoryName(fileName));

        if (!extracted)
        {
            // Extract file from cached CAS data
            var fileData = Strings.Mid(_casFileData, startPos, length);

            // Write file to temp dir
            File.WriteAllText(fileName, fileData, Encoding.GetEncoding(1252));

            _resources[resId].Extracted = true;
        }

        return fileName;
    }

    private void AddObjectAction(int id, string name, string script, bool noUpdate = false)
    {
        // Use NoUpdate in e.g. AddToGiveInfo, otherwise ObjectActionUpdate will call
        // AddToGiveInfo again leading to a big loop

        var actionNum = default(int);
        var foundExisting = false;

        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (name ?? ""))
            {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }

        if (!foundExisting)
        {
            o.NumberActions = o.NumberActions + 1;
            Array.Resize(ref o.Actions, o.NumberActions + 1);
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }

        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;

        ObjectActionUpdate(id, name, script, noUpdate);
    }

    private void AddToChangeLog(string appliesTo, string changeData)
    {
        _gameChangeData.NumberChanges = _gameChangeData.NumberChanges + 1;
        Array.Resize(ref _gameChangeData.ChangeData, _gameChangeData.NumberChanges + 1);
        _gameChangeData.ChangeData[_gameChangeData.NumberChanges] = new ChangeType();
        _gameChangeData.ChangeData[_gameChangeData.NumberChanges].AppliesTo = appliesTo;
        _gameChangeData.ChangeData[_gameChangeData.NumberChanges].Change = changeData;
    }

    private void AddToObjectChangeLog(ChangeLog.AppliesTo appliesToType, string appliesTo, string element,
        string changeData)
    {
        ChangeLog changeLog;

        // NOTE: We're only actually ever using the object changelog.
        // Rooms only get logged for creating rooms and creating/destroying exits, so we don't
        // need the refactored ChangeLog component for those.

        switch (appliesToType)
        {
            case ChangeLog.AppliesTo.Object:
            {
                changeLog = _changeLogObjects;
                break;
            }
            case ChangeLog.AppliesTo.Room:
            {
                changeLog = _changeLogRooms;
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        changeLog.AddItem(ref appliesTo, ref element, ref changeData);
    }

    private void AddToGiveInfo(int id, string giveData)
    {
        GiveType giveType;
        string actionName;
        string actionScript;

        var o = _objs[id];

        if (BeginsWith(giveData, "to "))
        {
            giveData = GetEverythingAfter(giveData, "to ");
            if (BeginsWith(giveData, "anything "))
            {
                o.GiveToAnything = GetEverythingAfter(giveData, "anything ");
                AddObjectAction(id, "give to anything", o.GiveToAnything, true);
                return;
            }

            giveType = GiveType.GiveToSomething;
            actionName = "give to ";
        }
        else if (BeginsWith(giveData, "anything "))
        {
            o.GiveAnything = GetEverythingAfter(giveData, "anything ");

            AddObjectAction(id, "give anything", o.GiveAnything, true);
            return;
        }
        else
        {
            giveType = GiveType.GiveSomethingTo;
            actionName = "give ";
        }

        if (Strings.Left(Strings.Trim(giveData), 1) == "<")
        {
            var name = GetParameter(giveData, _nullContext);
            var dataId = default(int);

            actionName = actionName + "'" + name + "'";

            var found = false;
            for (int i = 1, loopTo = o.NumberGiveData; i <= loopTo; i++)
            {
                if ((o.GiveData[i].GiveType == giveType) &
                    ((Strings.LCase(o.GiveData[i].GiveObject) ?? "") == (Strings.LCase(name) ?? "")))
                {
                    dataId = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                o.NumberGiveData = o.NumberGiveData + 1;
                Array.Resize(ref o.GiveData, o.NumberGiveData + 1);
                o.GiveData[o.NumberGiveData] = new GiveDataType();
                dataId = o.NumberGiveData;
            }

            var EP = Strings.InStr(giveData, ">");
            o.GiveData[dataId].GiveType = giveType;
            o.GiveData[dataId].GiveObject = name;
            o.GiveData[dataId].GiveScript = Strings.Mid(giveData, EP + 2);

            actionScript = o.GiveData[dataId].GiveScript;
            AddObjectAction(id, actionName, actionScript, true);
        }
    }

    internal void AddToObjectActions(string actionInfo, int id, Context ctx)
    {
        var actionNum = default(int);
        var foundExisting = false;

        var name = Strings.LCase(GetParameter(actionInfo, ctx));
        var ep = Strings.InStr(actionInfo, ">");
        if (ep == Strings.Len(actionInfo))
        {
            LogASLError("No script given for '" + name + "' action data", LogType.WarningError);
            return;
        }

        var script = Strings.Trim(Strings.Mid(actionInfo, ep + 1));

        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (name ?? ""))
            {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }

        if (!foundExisting)
        {
            o.NumberActions = o.NumberActions + 1;
            Array.Resize(ref o.Actions, o.NumberActions + 1);
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }

        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;

        ObjectActionUpdate(id, name, script);
    }

    private void AddToObjectAltNames(string altNames, int id)
    {
        var o = _objs[id];

        do
        {
            var endPos = Strings.InStr(altNames, ";");
            if (endPos == 0)
            {
                endPos = Strings.Len(altNames) + 1;
            }

            var curName = Strings.Trim(Strings.Left(altNames, endPos - 1));

            if (!string.IsNullOrEmpty(curName))
            {
                o.NumberAltNames = o.NumberAltNames + 1;
                Array.Resize(ref o.AltNames, o.NumberAltNames + 1);
                o.AltNames[o.NumberAltNames] = curName;
            }

            altNames = Strings.Mid(altNames, endPos + 1);
        } while (!string.IsNullOrEmpty(Strings.Trim(altNames)));
    }

    internal void AddToObjectProperties(string propertyInfo, int id, Context ctx)
    {
        if (id == 0)
        {
            return;
        }

        if (Strings.Right(propertyInfo, 1) != ";")
        {
            propertyInfo = propertyInfo + ";";
        }

        var num = default(int);
        do
        {
            var scp = Strings.InStr(propertyInfo, ";");
            var info = Strings.Left(propertyInfo, scp - 1);
            propertyInfo = Strings.Trim(Strings.Mid(propertyInfo, scp + 1));

            string name, value;

            if (string.IsNullOrEmpty(info))
            {
                break;
            }

            var ep = Strings.InStr(info, "=");
            if (ep != 0)
            {
                name = Strings.Trim(Strings.Left(info, ep - 1));
                value = Strings.Trim(Strings.Mid(info, ep + 1));
            }
            else
            {
                name = info;
                value = "";
            }

            var falseProperty = false;
            if (BeginsWith(name, "not ") & string.IsNullOrEmpty(value))
            {
                falseProperty = true;
                name = GetEverythingAfter(name, "not ");
            }

            var o = _objs[id];

            var found = false;
            for (int i = 1, loopTo = o.NumberProperties; i <= loopTo; i++)
            {
                if ((Strings.LCase(o.Properties[i].PropertyName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    found = true;
                    num = i;
                    i = o.NumberProperties;
                }
            }

            if (!found)
            {
                o.NumberProperties = o.NumberProperties + 1;
                Array.Resize(ref o.Properties, o.NumberProperties + 1);
                o.Properties[o.NumberProperties] = new PropertyType();
                num = o.NumberProperties;
            }

            if (falseProperty)
            {
                o.Properties[num].PropertyName = "";
            }
            else
            {
                o.Properties[num].PropertyName = name;
                o.Properties[num].PropertyValue = value;
            }

            AddToObjectChangeLog(ChangeLog.AppliesTo.Object, _objs[id].ObjectName, name, "properties " + info);

            switch (name ?? "")
            {
                case "alias":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].RoomAlias = value;
                    }
                    else
                    {
                        o.ObjectAlias = value;
                    }

                    if (_gameFullyLoaded)
                    {
                        UpdateObjectList(ctx);
                        UpdateItems(ctx);
                    }

                    break;
                }
                case "prefix":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].Prefix = value;
                    }
                    else if (!string.IsNullOrEmpty(value))
                    {
                        o.Prefix = value + " ";
                    }
                    else
                    {
                        o.Prefix = "";
                    }

                    break;
                }
                case "indescription":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].InDescription = value;
                    }

                    break;
                }
                case "description":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].Description.Data = value;
                        _rooms[o.CorresRoomId].Description.Type = TextActionType.Text;
                    }

                    break;
                }
                case "look":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].Look = value;
                    }

                    break;
                }
                case "suffix":
                {
                    o.Suffix = value;
                    break;
                }
                case "displaytype":
                {
                    o.DisplayType = value;
                    if (_gameFullyLoaded)
                    {
                        UpdateObjectList(ctx);
                    }

                    break;
                }
                case "gender":
                {
                    o.Gender = value;
                    break;
                }
                case "article":
                {
                    o.Article = value;
                    break;
                }
                case "detail":
                {
                    o.Detail = value;
                    break;
                }
                case "hidden":
                {
                    if (falseProperty)
                    {
                        o.Exists = true;
                    }
                    else
                    {
                        o.Exists = false;
                    }

                    if (_gameFullyLoaded)
                    {
                        UpdateObjectList(ctx);
                    }

                    break;
                }
                case "invisible":
                {
                    if (falseProperty)
                    {
                        o.Visible = true;
                    }
                    else
                    {
                        o.Visible = false;
                    }

                    if (_gameFullyLoaded)
                    {
                        UpdateObjectList(ctx);
                    }

                    break;
                }
                case "take":
                {
                    if (ASLVersion >= 392)
                    {
                        if (falseProperty)
                        {
                            o.Take.Type = TextActionType.Nothing;
                        }
                        else if (string.IsNullOrEmpty(value))
                        {
                            o.Take.Type = TextActionType.Default;
                        }
                        else
                        {
                            o.Take.Type = TextActionType.Text;
                            o.Take.Data = value;
                        }
                    }

                    break;
                }
            }
        } while (Strings.Len(Strings.Trim(propertyInfo)) != 0);
    }

    private void AddToUseInfo(int id, string useData)
    {
        UseType useType;

        var o = _objs[id];

        if (BeginsWith(useData, "on "))
        {
            useData = GetEverythingAfter(useData, "on ");
            if (BeginsWith(useData, "anything "))
            {
                o.UseOnAnything = GetEverythingAfter(useData, "anything ");
                return;
            }

            useType = UseType.UseOnSomething;
        }
        else if (BeginsWith(useData, "anything "))
        {
            o.UseAnything = GetEverythingAfter(useData, "anything ");
            return;
        }
        else
        {
            useType = UseType.UseSomethingOn;
        }

        if (Strings.Left(Strings.Trim(useData), 1) == "<")
        {
            var objectName = GetParameter(useData, _nullContext);
            var dataId = default(int);
            var found = false;

            for (int i = 1, loopTo = o.NumberUseData; i <= loopTo; i++)
            {
                if ((o.UseData[i].UseType == useType) &
                    ((Strings.LCase(o.UseData[i].UseObject) ?? "") == (Strings.LCase(objectName) ?? "")))
                {
                    dataId = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                o.NumberUseData = o.NumberUseData + 1;
                Array.Resize(ref o.UseData, o.NumberUseData + 1);
                o.UseData[o.NumberUseData] = new UseDataType();
                dataId = o.NumberUseData;
            }

            var ep = Strings.InStr(useData, ">");
            o.UseData[dataId].UseType = useType;
            o.UseData[dataId].UseObject = objectName;
            o.UseData[dataId].UseScript = Strings.Mid(useData, ep + 2);
        }
        else
        {
            o.Use = Strings.Trim(useData);
        }
    }

    private string CapFirst(string s)
    {
        return Strings.UCase(Strings.Left(s, 1)) + Strings.Mid(s, 2);
    }

    private string ConvertVarsIn(string s, Context ctx)
    {
        return GetParameter("<" + s + ">", ctx);
    }

    private bool DisambObjHere(Context ctx, int id, string firstPlace, bool twoPlaces = false, string secondPlace = "",
        bool isExit = false)
    {
        var isSeen = default(bool);
        var onlySeen = false;

        if (firstPlace == "game")
        {
            firstPlace = "";
            if (secondPlace == "seen")
            {
                twoPlaces = false;
                secondPlace = "";
                onlySeen = true;
                var roomObjId = _rooms[GetRoomID(_objs[id].ContainerRoom, ctx)].ObjId;

                if (_objs[id].ContainerRoom == "inventory")
                {
                    isSeen = true;
                }
                else if (IsYes(GetObjectProperty("visited", roomObjId, true, false)))
                {
                    isSeen = true;
                }
                else if (IsYes(GetObjectProperty("seen", id, true, false)))
                {
                    isSeen = true;
                }
            }
        }

        if ((((twoPlaces == false) &
              (((Strings.LCase(_objs[id].ContainerRoom) ?? "") == (Strings.LCase(firstPlace) ?? "")) |
               string.IsNullOrEmpty(firstPlace))) |
             (twoPlaces & (((Strings.LCase(_objs[id].ContainerRoom) ?? "") == (Strings.LCase(firstPlace) ?? "")) |
                           ((Strings.LCase(_objs[id].ContainerRoom) ?? "") == (Strings.LCase(secondPlace) ?? ""))))) &
            _objs[id].Exists & (_objs[id].IsExit == isExit))
        {
            if (!onlySeen)
            {
                return true;
            }

            return isSeen;
        }

        return false;
    }

    private void ExecClone(string cloneString, Context ctx)
    {
        int id;
        string newName, cloneTo;

        var scp = Strings.InStr(cloneString, ";");
        if (scp == 0)
        {
            LogASLError("No new object name specified in 'clone <" + cloneString + ">", LogType.WarningError);
            return;
        }

        var objectToClone = Strings.Trim(Strings.Left(cloneString, scp - 1));
        id = GetObjectIdNoAlias(objectToClone);

        var SC2 = Strings.InStr(scp + 1, cloneString, ";");
        if (SC2 == 0)
        {
            cloneTo = _objs[id].ContainerRoom;
            newName = Strings.Trim(Strings.Mid(cloneString, scp + 1));
        }
        else
        {
            cloneTo = Strings.Trim(Strings.Mid(cloneString, SC2 + 1));
            newName = Strings.Trim(Strings.Mid(cloneString, scp + 1, SC2 - scp - 1));
        }

        _numberObjs = _numberObjs + 1;
        Array.Resize(ref _objs, _numberObjs + 1);
        _objs[_numberObjs] = new ObjectType();
        _objs[_numberObjs] = _objs[id];
        _objs[_numberObjs].ContainerRoom = cloneTo;
        _objs[_numberObjs].ObjectName = newName;

        if (_objs[id].IsRoom)
        {
            // This is a room so create the corresponding room as well

            _numberRooms = _numberRooms + 1;
            Array.Resize(ref _rooms, _numberRooms + 1);
            _rooms[_numberRooms] = new RoomType();
            _rooms[_numberRooms] = _rooms[_objs[id].CorresRoomId];
            _rooms[_numberRooms].RoomName = newName;
            _rooms[_numberRooms].ObjId = _numberObjs;

            _objs[_numberObjs].CorresRoom = newName;
            _objs[_numberObjs].CorresRoomId = _numberRooms;

            AddToChangeLog("room " + newName, "create");
        }
        else
        {
            AddToChangeLog("object " + newName, "create " + _objs[_numberObjs].ContainerRoom);
        }

        UpdateObjectList(ctx);
    }

    private void ExecOops(string correction, Context ctx)
    {
        if (!string.IsNullOrEmpty(_badCmdBefore))
        {
            if (string.IsNullOrEmpty(_badCmdAfter))
            {
                ExecCommand(_badCmdBefore + " " + correction, ctx, false);
            }
            else
            {
                ExecCommand(_badCmdBefore + " " + correction + " " + _badCmdAfter, ctx, false);
            }
        }
    }

    private void ExecType(string typeData, Context ctx)
    {
        var id = default(int);
        var found = default(bool);
        var scp = Strings.InStr(typeData, ";");

        if (scp == 0)
        {
            LogASLError("No type name given in 'type <" + typeData + ">'");
            return;
        }

        var objName = Strings.Trim(Strings.Left(typeData, scp - 1));
        var typeName = Strings.Trim(Strings.Mid(typeData, scp + 1));

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object in 'type <" + typeData + ">'");
            return;
        }

        var o = _objs[id];

        o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
        Array.Resize(ref o.TypesIncluded, o.NumberTypesIncluded + 1);
        o.TypesIncluded[o.NumberTypesIncluded] = typeName;

        var propertyData = GetPropertiesInType(typeName);
        AddToObjectProperties(propertyData.Properties, id, ctx);
        for (int i = 1, loopTo1 = propertyData.NumberActions; i <= loopTo1; i++)
        {
            AddObjectAction(id, propertyData.Actions[i].ActionName, propertyData.Actions[i].Script);
        }

        // New as of Quest 4.0. Fixes bug that "if type" would fail for any
        // parent types included by the "type" command.
        for (int i = 1, loopTo2 = propertyData.NumberTypesIncluded; i <= loopTo2; i++)
        {
            o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
            Array.Resize(ref o.TypesIncluded, o.NumberTypesIncluded + 1);
            o.TypesIncluded[o.NumberTypesIncluded] = propertyData.TypesIncluded[i];
        }
    }

    private bool ExecuteIfAction(string actionData)
    {
        var id = default(int);

        var scp = Strings.InStr(actionData, ";");

        if (scp == 0)
        {
            LogASLError("No action name given in condition 'action <" + actionData + ">' ...", LogType.WarningError);
            return false;
        }

        var objName = Strings.Trim(Strings.Left(actionData, scp - 1));
        var actionName = Strings.Trim(Strings.Mid(actionData, scp + 1));
        var found = false;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object '" + objName + "' in condition 'action <" + actionData + ">' ...",
                LogType.WarningError);
            return false;
        }

        var o = _objs[id];

        for (int i = 1, loopTo1 = o.NumberActions; i <= loopTo1; i++)
        {
            if ((Strings.LCase(o.Actions[i].ActionName) ?? "") == (Strings.LCase(actionName) ?? ""))
            {
                return true;
            }
        }

        return false;
    }

    private bool ExecuteIfType(string typeData)
    {
        var id = default(int);

        var scp = Strings.InStr(typeData, ";");

        if (scp == 0)
        {
            LogASLError("No type name given in condition 'type <" + typeData + ">' ...", LogType.WarningError);
            return false;
        }

        var objName = Strings.Trim(Strings.Left(typeData, scp - 1));
        var typeName = Strings.Trim(Strings.Mid(typeData, scp + 1));

        var found = false;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object '" + objName + "' in condition 'type <" + typeData + ">' ...",
                LogType.WarningError);
            return false;
        }

        var o = _objs[id];

        for (int i = 1, loopTo1 = o.NumberTypesIncluded; i <= loopTo1; i++)
        {
            if ((Strings.LCase(o.TypesIncluded[i]) ?? "") == (Strings.LCase(typeName) ?? ""))
            {
                return true;
            }
        }

        return false;
    }

    private class ArrayResult
    {
        public string Name;
        public int Index;
    }

    private ArrayResult GetArrayIndex(string varName, Context ctx)
    {
        var result = new ArrayResult();

        if ((Strings.InStr(varName, "[") == 0) | (Strings.InStr(varName, "]") == 0))
        {
            result.Name = varName;
            return result;
        }

        var beginPos = Strings.InStr(varName, "[");
        var endPos = Strings.InStr(varName, "]");
        var data = Strings.Mid(varName, beginPos + 1, endPos - beginPos - 1);

        if (Information.IsNumeric(data))
        {
            result.Index = Conversions.ToInteger(data);
        }
        else
        {
            result.Index = (int) Math.Round(GetNumericContents(data, ctx));
        }

        result.Name = Strings.Left(varName, beginPos - 1);
        return result;
    }

    internal int Disambiguate(string name, string containedIn, Context ctx, bool isExit = false)
    {
        // Returns object ID being referred to by player.
        // Returns -1 if object doesn't exist, calling function
        // then expected to print relevant error.
        // Returns -2 if "it" meaningless, prints own error.
        // If it returns an object ID, it also sets quest.lastobject to the name
        // of the object referred to.
        // If ctx.AllowRealNamesInCommand is True, will allow an object's real
        // name to be used even when the object has an alias - this is used when
        // Disambiguate has been called after an "exec" command to prevent the
        // player having to choose an object from the disambiguation menu twice

        var numberCorresIds = 0;
        var idNumbers = new int[1];
        string firstPlace;
        var secondPlace = "";
        bool twoPlaces;
        string[] descriptionText;
        string[] validNames;
        int numValidNames;

        name = Strings.Trim(name);

        SetStringContents("quest.lastobject", "", ctx);

        if (Strings.InStr(containedIn, ";") != 0)
        {
            var scp = Strings.InStr(containedIn, ";");
            twoPlaces = true;
            firstPlace = Strings.Trim(Strings.Left(containedIn, scp - 1));
            secondPlace = Strings.Trim(Strings.Mid(containedIn, scp + 1));
        }
        else
        {
            twoPlaces = false;
            firstPlace = containedIn;
        }

        if (ctx.AllowRealNamesInCommand)
        {
            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if (DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace))
                {
                    if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))
                    {
                        SetStringContents("quest.lastobject", _objs[i].ObjectName, ctx);
                        return i;
                    }
                }
            }
        }

        // If player uses "it", "them" etc. as name:
        if ((name == "it") | (name == "them") | (name == "this") | (name == "those") | (name == "these") |
            (name == "that"))
        {
            SetStringContents("quest.error.pronoun", name, ctx);
            if ((_lastIt != 0) & (_lastItMode == ItType.Inanimate) &
                DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace))
            {
                SetStringContents("quest.lastobject", _objs[_lastIt].ObjectName, ctx);
                return _lastIt;
            }

            PlayerErrorMessage(PlayerError.BadPronoun, ctx);
            return -2;
        }

        if (name == "him")
        {
            SetStringContents("quest.error.pronoun", name, ctx);
            if ((_lastIt != 0) & (_lastItMode == ItType.Male) &
                DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace))
            {
                SetStringContents("quest.lastobject", _objs[_lastIt].ObjectName, ctx);
                return _lastIt;
            }

            PlayerErrorMessage(PlayerError.BadPronoun, ctx);
            return -2;
        }

        if (name == "her")
        {
            SetStringContents("quest.error.pronoun", name, ctx);
            if ((_lastIt != 0) & (_lastItMode == ItType.Female) &
                DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace))
            {
                SetStringContents("quest.lastobject", _objs[_lastIt].ObjectName, ctx);
                return _lastIt;
            }

            PlayerErrorMessage(PlayerError.BadPronoun, ctx);
            return -2;
        }

        _thisTurnIt = 0;

        if (BeginsWith(name, "the "))
        {
            name = GetEverythingAfter(name, "the ");
        }

        for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
        {
            if (DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit))
            {
                numValidNames = _objs[i].NumberAltNames + 1;
                validNames = new string[numValidNames + 1];
                validNames[1] = _objs[i].ObjectAlias;
                for (int j = 1, loopTo2 = _objs[i].NumberAltNames; j <= loopTo2; j++)
                {
                    validNames[j + 1] = _objs[i].AltNames[j];
                }

                for (int j = 1, loopTo3 = numValidNames; j <= loopTo3; j++)
                {
                    if (((Strings.LCase(validNames[j]) ?? "") == (Strings.LCase(name) ?? "")) |
                        (("the " + Strings.LCase(name) ?? "") == (Strings.LCase(validNames[j]) ?? "")))
                    {
                        numberCorresIds = numberCorresIds + 1;
                        Array.Resize(ref idNumbers, numberCorresIds + 1);
                        idNumbers[numberCorresIds] = i;
                        j = numValidNames;
                    }
                }
            }
        }

        if ((ASLVersion >= 391) & (numberCorresIds == 0) & _useAbbreviations & (Strings.Len(name) > 0))
            // Check for abbreviated object names
        {
            for (int i = 1, loopTo4 = _numberObjs; i <= loopTo4; i++)
            {
                if (DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit))
                {
                    string thisName;
                    if (!string.IsNullOrEmpty(_objs[i].ObjectAlias))
                    {
                        thisName = Strings.LCase(_objs[i].ObjectAlias);
                    }
                    else
                    {
                        thisName = Strings.LCase(_objs[i].ObjectName);
                    }

                    if (ASLVersion >= 410)
                    {
                        if (!string.IsNullOrEmpty(_objs[i].Prefix))
                        {
                            thisName = Strings.Trim(Strings.LCase(_objs[i].Prefix)) + " " + thisName;
                        }

                        if (!string.IsNullOrEmpty(_objs[i].Suffix))
                        {
                            thisName = thisName + " " + Strings.Trim(Strings.LCase(_objs[i].Suffix));
                        }
                    }

                    if (Strings.InStr(" " + thisName, " " + Strings.LCase(name)) != 0)
                    {
                        numberCorresIds = numberCorresIds + 1;
                        Array.Resize(ref idNumbers, numberCorresIds + 1);
                        idNumbers[numberCorresIds] = i;
                    }
                }
            }
        }

        if (numberCorresIds == 1)
        {
            SetStringContents("quest.lastobject", _objs[idNumbers[1]].ObjectName, ctx);
            _thisTurnIt = idNumbers[1];

            switch (_objs[idNumbers[1]].Article ?? "")
            {
                case "him":
                {
                    _thisTurnItMode = ItType.Male;
                    break;
                }
                case "her":
                {
                    _thisTurnItMode = ItType.Female;
                    break;
                }

                default:
                {
                    _thisTurnItMode = ItType.Inanimate;
                    break;
                }
            }

            return idNumbers[1];
        }

        if (numberCorresIds > 1)
        {
            descriptionText = new string[numberCorresIds + 1];

            var question = "Please select which " + name + " you mean:";
            Print("- |i" + question + "|xi", ctx);

            var menuItems = new Dictionary<string, string>();

            for (int i = 1, loopTo5 = numberCorresIds; i <= loopTo5; i++)
            {
                descriptionText[i] = _objs[idNumbers[i]].Detail;
                if (string.IsNullOrEmpty(descriptionText[i]))
                {
                    if (string.IsNullOrEmpty(_objs[idNumbers[i]].Prefix))
                    {
                        descriptionText[i] = _objs[idNumbers[i]].ObjectAlias;
                    }
                    else
                    {
                        descriptionText[i] = _objs[idNumbers[i]].Prefix + _objs[idNumbers[i]].ObjectAlias;
                    }
                }

                menuItems.Add(i.ToString(), descriptionText[i]);
            }

            var mnu = new MenuData(question, menuItems, false);
            var response = ShowMenu(mnu);

            _choiceNumber = Conversions.ToInteger(response);

            SetStringContents("quest.lastobject", _objs[idNumbers[_choiceNumber]].ObjectName, ctx);

            _thisTurnIt = idNumbers[_choiceNumber];

            switch (_objs[idNumbers[_choiceNumber]].Article ?? "")
            {
                case "him":
                {
                    _thisTurnItMode = ItType.Male;
                    break;
                }
                case "her":
                {
                    _thisTurnItMode = ItType.Female;
                    break;
                }

                default:
                {
                    _thisTurnItMode = ItType.Inanimate;
                    break;
                }
            }

            Print("- " + descriptionText[_choiceNumber] + "|n", ctx);

            return idNumbers[_choiceNumber];
        }

        _thisTurnIt = _lastIt;
        SetStringContents("quest.error.object", name, ctx);
        return -1;
    }

    private string DisplayStatusVariableInfo(int id, VarType type, Context ctx)
    {
        var displayData = "";
        int ep;

        if (type == VarType.String)
        {
            displayData = ConvertVarsIn(_stringVariable[id].DisplayString, ctx);
            ep = Strings.InStr(displayData, "!");

            if (ep != 0)
            {
                displayData = Strings.Left(displayData, ep - 1) + _stringVariable[id].VariableContents[0] +
                              Strings.Mid(displayData, ep + 1);
            }
        }
        else if (type == VarType.Numeric)
        {
            if (_numericVariable[id].NoZeroDisplay &
                (Conversion.Val(_numericVariable[id].VariableContents[0]) == 0d))
            {
                return "";
            }

            displayData = ConvertVarsIn(_numericVariable[id].DisplayString, ctx);
            ep = Strings.InStr(displayData, "!");

            if (ep != 0)
            {
                displayData = Strings.Left(displayData, ep - 1) + _numericVariable[id].VariableContents[0] +
                              Strings.Mid(displayData, ep + 1);
            }

            if (Strings.InStr(displayData, "*") > 0)
            {
                var firstStar = Strings.InStr(displayData, "*");
                var secondStar = Strings.InStr(firstStar + 1, displayData, "*");
                var beforeStar = Strings.Left(displayData, firstStar - 1);
                var afterStar = Strings.Mid(displayData, secondStar + 1);
                var betweenStar = Strings.Mid(displayData, firstStar + 1, secondStar - firstStar - 1);

                if (Conversions.ToDouble(_numericVariable[id].VariableContents[0]) != 1d)
                {
                    displayData = beforeStar + betweenStar + afterStar;
                }
                else
                {
                    displayData = beforeStar + afterStar;
                }
            }
        }

        return displayData;
    }

    internal bool DoAction(int id, string action, Context ctx, bool logError = true)
    {
        var found = default(bool);
        var script = "";

        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (Strings.LCase(action) ?? ""))
            {
                found = true;
                script = o.Actions[i].Script;
                break;
            }
        }

        if (!found)
        {
            if (logError)
            {
                LogASLError("No such action '" + action + "' defined for object '" + o.ObjectName + "'");
            }

            return false;
        }

        var newCtx = CopyContext(ctx);
        newCtx.CallingObjectId = id;

        ExecuteScript(script, newCtx, id);

        return true;
    }

    public bool HasAction(int id, string action)
    {
        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (Strings.LCase(action) ?? ""))
            {
                return true;
            }
        }

        return false;
    }

    private void ExecForEach(string scriptLine, Context ctx)
    {
        string inLocation, scriptToRun;
        var isExit = default(bool);
        var isRoom = default(bool);

        if (BeginsWith(scriptLine, "object "))
        {
            scriptLine = GetEverythingAfter(scriptLine, "object ");
            if (!BeginsWith(scriptLine, "in "))
            {
                LogASLError("Expected 'in' in 'for each object " + ReportErrorLine(scriptLine) + "'",
                    LogType.WarningError);
                return;
            }
        }
        else if (BeginsWith(scriptLine, "exit "))
        {
            scriptLine = GetEverythingAfter(scriptLine, "exit ");
            if (!BeginsWith(scriptLine, "in "))
            {
                LogASLError("Expected 'in' in 'for each exit " + ReportErrorLine(scriptLine) + "'",
                    LogType.WarningError);
                return;
            }

            isExit = true;
        }
        else if (BeginsWith(scriptLine, "room "))
        {
            scriptLine = GetEverythingAfter(scriptLine, "room ");
            if (!BeginsWith(scriptLine, "in "))
            {
                LogASLError("Expected 'in' in 'for each room " + ReportErrorLine(scriptLine) + "'",
                    LogType.WarningError);
                return;
            }

            isRoom = true;
        }
        else
        {
            LogASLError("Unknown type in 'for each " + ReportErrorLine(scriptLine) + "'", LogType.WarningError);
            return;
        }

        scriptLine = GetEverythingAfter(scriptLine, "in ");

        if (BeginsWith(scriptLine, "game "))
        {
            inLocation = "";
            scriptToRun = GetEverythingAfter(scriptLine, "game ");
        }
        else
        {
            inLocation = Strings.LCase(GetParameter(scriptLine, ctx));
            var bracketPos = Strings.InStr(scriptLine, ">");
            scriptToRun = Strings.Trim(Strings.Mid(scriptLine, bracketPos + 1));
        }

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if (string.IsNullOrEmpty(inLocation) |
                ((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (inLocation ?? "")))
            {
                if ((_objs[i].IsRoom == isRoom) & (_objs[i].IsExit == isExit))
                {
                    SetStringContents("quest.thing", _objs[i].ObjectName, ctx);
                    ExecuteScript(scriptToRun, ctx);
                }
            }
        }
    }

    private void ExecuteAction(string data, Context ctx)
    {
        string actionName;
        string script;
        var actionNum = default(int);
        var id = default(int);
        var foundExisting = false;
        var foundObject = false;

        var param = GetParameter(data, ctx);
        var scp = Strings.InStr(param, ";");
        if (scp == 0)
        {
            LogASLError("No action name specified in 'action " + data + "'", LogType.WarningError);
            return;
        }

        var objName = Strings.Trim(Strings.Left(param, scp - 1));
        actionName = Strings.Trim(Strings.Mid(param, scp + 1));

        var ep = Strings.InStr(data, ">");
        if (ep == Strings.Len(Strings.Trim(data)))
        {
            script = "";
        }
        else
        {
            script = Strings.Trim(Strings.Mid(data, ep + 1));
        }

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                foundObject = true;
                id = i;
                break;
            }
        }

        if (!foundObject)
        {
            LogASLError("No such object '" + objName + "' in 'action " + data + "'", LogType.WarningError);
            return;
        }

        var o = _objs[id];

        for (int i = 1, loopTo1 = o.NumberActions; i <= loopTo1; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (actionName ?? ""))
            {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }

        if (!foundExisting)
        {
            o.NumberActions = o.NumberActions + 1;
            Array.Resize(ref o.Actions, o.NumberActions + 1);
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }

        o.Actions[actionNum].ActionName = actionName;
        o.Actions[actionNum].Script = script;

        ObjectActionUpdate(id, actionName, script);
    }

    private bool ExecuteCondition(string condition, Context ctx)
    {
        bool result = default, thisNot;

        if (BeginsWith(condition, "not "))
        {
            thisNot = true;
            condition = GetEverythingAfter(condition, "not ");
        }
        else
        {
            thisNot = false;
        }

        if (BeginsWith(condition, "got "))
        {
            result = ExecuteIfGot(GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "has "))
        {
            result = ExecuteIfHas(GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "ask "))
        {
            result = ExecuteIfAsk(GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "is "))
        {
            result = ExecuteIfIs(GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "here "))
        {
            result = ExecuteIfHere(GetParameter(condition, ctx), ctx);
        }
        else if (BeginsWith(condition, "exists "))
        {
            result = ExecuteIfExists(GetParameter(condition, ctx), false);
        }
        else if (BeginsWith(condition, "real "))
        {
            result = ExecuteIfExists(GetParameter(condition, ctx), true);
        }
        else if (BeginsWith(condition, "property "))
        {
            result = ExecuteIfProperty(GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "action "))
        {
            result = ExecuteIfAction(GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "type "))
        {
            result = ExecuteIfType(GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "flag "))
        {
            result = ExecuteIfFlag(GetParameter(condition, ctx));
        }

        if (thisNot)
        {
            result = !result;
        }

        return result;
    }

    private bool ExecuteConditions(string list, Context ctx)
    {
        var conditions = default(string[]);
        var numConditions = 0;
        var operations = default(string[]);
        var obscuredConditionList = ObliterateParameters(list);
        var pos = 1;
        var isFinalCondition = false;

        do
        {
            numConditions = numConditions + 1;
            Array.Resize(ref conditions, numConditions + 1);
            Array.Resize(ref operations, numConditions + 1);

            var nextCondition = "AND";
            var nextConditionPos = Strings.InStr(pos, obscuredConditionList, "and ");
            if (nextConditionPos == 0)
            {
                nextConditionPos = Strings.InStr(pos, obscuredConditionList, "or ");
                nextCondition = "OR";
            }

            if (nextConditionPos == 0)
            {
                nextConditionPos = Strings.Len(obscuredConditionList) + 2;
                isFinalCondition = true;
                nextCondition = "FINAL";
            }

            var thisCondition = Strings.Trim(Strings.Mid(list, pos, nextConditionPos - pos - 1));
            conditions[numConditions] = thisCondition;
            operations[numConditions] = nextCondition;

            // next condition starts from space after and/or
            pos = Strings.InStr(nextConditionPos, obscuredConditionList, " ");
        } while (!isFinalCondition);

        operations[0] = "AND";
        var result = true;

        for (int i = 1, loopTo = numConditions; i <= loopTo; i++)
        {
            var thisResult = ExecuteCondition(conditions[i], ctx);

            if (operations[i - 1] == "AND")
            {
                result = thisResult & result;
            }
            else if (operations[i - 1] == "OR")
            {
                result = thisResult | result;
            }
        }

        return result;
    }

    private void ExecuteCreate(string data, Context ctx)
    {
        string newName;

        if (BeginsWith(data, "room "))
        {
            newName = GetParameter(data, ctx);
            _numberRooms = _numberRooms + 1;
            Array.Resize(ref _rooms, _numberRooms + 1);
            _rooms[_numberRooms] = new RoomType();
            _rooms[_numberRooms].RoomName = newName;

            _numberObjs = _numberObjs + 1;
            Array.Resize(ref _objs, _numberObjs + 1);
            _objs[_numberObjs] = new ObjectType();
            _objs[_numberObjs].ObjectName = newName;
            _objs[_numberObjs].IsRoom = true;
            _objs[_numberObjs].CorresRoom = newName;
            _objs[_numberObjs].CorresRoomId = _numberRooms;

            _rooms[_numberRooms].ObjId = _numberObjs;

            AddToChangeLog("room " + newName, "create");

            if (ASLVersion >= 410)
            {
                AddToObjectProperties(_defaultRoomProperties.Properties, _numberObjs, ctx);
                for (int j = 1, loopTo = _defaultRoomProperties.NumberActions; j <= loopTo; j++)
                {
                    AddObjectAction(_numberObjs, _defaultRoomProperties.Actions[j].ActionName,
                        _defaultRoomProperties.Actions[j].Script);
                }

                _rooms[_numberRooms].Exits = new RoomExits(this);
                _rooms[_numberRooms].Exits.SetObjId(_rooms[_numberRooms].ObjId);
            }
        }

        else if (BeginsWith(data, "object "))
        {
            var paramData = GetParameter(data, ctx);
            var scp = Strings.InStr(paramData, ";");
            string containerRoom;

            if (scp == 0)
            {
                newName = paramData;
                containerRoom = "";
            }
            else
            {
                newName = Strings.Trim(Strings.Left(paramData, scp - 1));
                containerRoom = Strings.Trim(Strings.Mid(paramData, scp + 1));
            }

            _numberObjs = _numberObjs + 1;
            Array.Resize(ref _objs, _numberObjs + 1);
            _objs[_numberObjs] = new ObjectType();

            var o = _objs[_numberObjs];
            o.ObjectName = newName;
            o.ObjectAlias = newName;
            o.ContainerRoom = containerRoom;
            o.Exists = true;
            o.Visible = true;
            o.Gender = "it";
            o.Article = "it";

            AddToChangeLog("object " + newName, "create " + _objs[_numberObjs].ContainerRoom);

            if (ASLVersion >= 410)
            {
                AddToObjectProperties(_defaultProperties.Properties, _numberObjs, ctx);
                for (int j = 1, loopTo1 = _defaultProperties.NumberActions; j <= loopTo1; j++)
                {
                    AddObjectAction(_numberObjs, _defaultProperties.Actions[j].ActionName,
                        _defaultProperties.Actions[j].Script);
                }
            }

            if (!_gameLoading)
            {
                UpdateObjectList(ctx);
            }
        }

        else if (BeginsWith(data, "exit "))
        {
            ExecuteCreateExit(data, ctx);
        }
    }

    private void ExecuteCreateExit(string data, Context ctx)
    {
        string scrRoom;
        var destRoom = "";
        var destId = default(int);
        var exitData = GetEverythingAfter(data, "exit ");
        var newName = GetParameter(data, ctx);
        var scp = Strings.InStr(newName, ";");

        if (ASLVersion < 410)
        {
            if (scp == 0)
            {
                LogASLError("No exit destination given in 'create exit " + exitData + "'", LogType.WarningError);
                return;
            }
        }

        if (scp == 0)
        {
            scrRoom = Strings.Trim(newName);
        }
        else
        {
            scrRoom = Strings.Trim(Strings.Left(newName, scp - 1));
        }

        var srcId = GetRoomID(scrRoom, ctx);

        if (srcId == 0)
        {
            LogASLError("No such room '" + scrRoom + "'", LogType.WarningError);
            return;
        }

        if (ASLVersion < 410)
        {
            // only do destination room check for ASL <410, as can now have scripts on dynamically
            // created exits, so the destination doesn't necessarily have to exist.

            destRoom = Strings.Trim(Strings.Mid(newName, scp + 1));
            if (!string.IsNullOrEmpty(destRoom))
            {
                destId = GetRoomID(destRoom, ctx);

                if (destId == 0)
                {
                    LogASLError("No such room '" + destRoom + "'", LogType.WarningError);
                    return;
                }
            }
        }

        // If it's a "go to" exit, check if it already exists:
        var exists = false;
        if (BeginsWith(exitData, "<"))
        {
            if (ASLVersion >= 410)
            {
                exists = _rooms[srcId].Exits.GetPlaces().ContainsKey(destRoom);
            }
            else
            {
                for (int i = 1, loopTo = _rooms[srcId].NumberPlaces; i <= loopTo; i++)
                {
                    if ((Strings.LCase(_rooms[srcId].Places[i].PlaceName) ?? "") == (Strings.LCase(destRoom) ?? ""))
                    {
                        exists = true;
                        break;
                    }
                }
            }

            if (exists)
            {
                LogASLError("Exit from '" + scrRoom + "' to '" + destRoom + "' already exists", LogType.WarningError);
                return;
            }
        }

        var paramPos = Strings.InStr(exitData, "<");
        string saveData;
        if (paramPos == 0)
        {
            saveData = exitData;
        }
        else
        {
            saveData = Strings.Left(exitData, paramPos - 1);
            // We do this so the changelog doesn't contain unconverted variable names
            saveData = saveData + "<" + GetParameter(exitData, ctx) + ">";
        }

        AddToChangeLog("room " + _rooms[srcId].RoomName, "exit " + saveData);

        var r = _rooms[srcId];

        if (ASLVersion >= 410)
        {
            r.Exits.AddExitFromCreateScript(exitData, ref ctx);
        }
        else if (BeginsWith(exitData, "north "))
        {
            r.North.Data = destRoom;
            r.North.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "south "))
        {
            r.South.Data = destRoom;
            r.South.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "east "))
        {
            r.East.Data = destRoom;
            r.East.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "west "))
        {
            r.West.Data = destRoom;
            r.West.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "northeast "))
        {
            r.NorthEast.Data = destRoom;
            r.NorthEast.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "northwest "))
        {
            r.NorthWest.Data = destRoom;
            r.NorthWest.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "southeast "))
        {
            r.SouthEast.Data = destRoom;
            r.SouthEast.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "southwest "))
        {
            r.SouthWest.Data = destRoom;
            r.SouthWest.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "up "))
        {
            r.Up.Data = destRoom;
            r.Up.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "down "))
        {
            r.Down.Data = destRoom;
            r.Down.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "out "))
        {
            r.Out.Text = destRoom;
        }
        else if (BeginsWith(exitData, "<"))
        {
            r.NumberPlaces = r.NumberPlaces + 1;
            Array.Resize(ref r.Places, r.NumberPlaces + 1);
            r.Places[r.NumberPlaces] = new PlaceType();
            r.Places[r.NumberPlaces].PlaceName = destRoom;
        }
        else
        {
            LogASLError("Invalid direction in 'create exit " + exitData + "'", LogType.WarningError);
        }

        if (!_gameLoading)
        {
            // Update quest.doorways variables
            ShowRoomInfo(_currentRoom, ctx, true);

            UpdateObjectList(ctx);

            if (ASLVersion < 410)
            {
                if ((_currentRoom ?? "") == (_rooms[srcId].RoomName ?? ""))
                {
                    UpdateDoorways(srcId, ctx);
                }
                else if ((_currentRoom ?? "") == (_rooms[destId].RoomName ?? ""))
                {
                    UpdateDoorways(destId, ctx);
                }
            }
            else
            {
                // Don't have DestID in ASL410 CreateExit code, so just UpdateDoorways
                // for current room anyway.
                UpdateDoorways(GetRoomID(_currentRoom, ctx), ctx);
            }
        }
    }

    private void ExecDrop(string obj, Context ctx)
    {
        bool found;
        int parentId = default, id;

        id = Disambiguate(obj, "inventory", ctx);

        if (id > 0)
        {
            found = true;
        }
        else
        {
            found = false;
        }

        if (!found)
        {
            if (id != -2)
            {
                if (ASLVersion >= 391)
                {
                    PlayerErrorMessage(PlayerError.NoItem, ctx);
                }
                else
                {
                    PlayerErrorMessage(PlayerError.BadDrop, ctx);
                }
            }

            _badCmdBefore = "drop";
            return;
        }

        // If object is inside a container, it must be removed before it can be dropped.
        var isInContainer = false;
        if (ASLVersion >= 391)
        {
            if (IsYes(GetObjectProperty("parent", id, true, false)))
            {
                isInContainer = true;
                var parent = GetObjectProperty("parent", id, false, false);
                parentId = GetObjectIdNoAlias(parent);
            }
        }

        var dropFound = false;
        var dropStatement = "";

        for (int i = _objs[id].DefinitionSectionStart, loopTo = _objs[id].DefinitionSectionEnd; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "drop "))
            {
                dropStatement = GetEverythingAfter(_lines[i], "drop ");
                dropFound = true;
                break;
            }
        }

        SetStringContents("quest.error.article", _objs[id].Article, ctx);

        if (!dropFound | BeginsWith(dropStatement, "everywhere"))
        {
            if (isInContainer)
            {
                // So, we want to drop an object that's in a container or surface. So first
                // we have to remove the object from that container.
                string parentDisplayName;

                if (!string.IsNullOrEmpty(_objs[parentId].ObjectAlias))
                {
                    parentDisplayName = _objs[parentId].ObjectAlias;
                }
                else
                {
                    parentDisplayName = _objs[parentId].ObjectName;
                }

                Print("(first removing " + _objs[id].Article + " from " + parentDisplayName + ")", ctx);

                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                ExecCommand("remove " + _objs[id].ObjectName, ctx, false, dontSetIt: true);

                if (!string.IsNullOrEmpty(GetObjectProperty("parent", id, false, false)))
                    // removing the object failed
                {
                    return;
                }
            }
        }

        if (!dropFound)
        {
            PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
            PlayerItem(_objs[id].ObjectName, false, ctx);
        }
        else if (BeginsWith(dropStatement, "everywhere"))
        {
            PlayerItem(_objs[id].ObjectName, false, ctx);
            if (Strings.InStr(dropStatement, "<") != 0)
            {
                Print(GetParameter(dropStatement, ctx), ctx);
            }
            else
            {
                PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
            }
        }
        else if (BeginsWith(dropStatement, "nowhere"))
        {
            if (Strings.InStr(dropStatement, "<") != 0)
            {
                Print(GetParameter(dropStatement, ctx), ctx);
            }
            else
            {
                PlayerErrorMessage(PlayerError.CantDrop, ctx);
            }
        }
        else
        {
            ExecuteScript(dropStatement, ctx);
        }
    }

    private void ExecExamine(string command, Context ctx)
    {
        var item = Strings.LCase(Strings.Trim(GetEverythingAfter(command, "examine ")));

        if (string.IsNullOrEmpty(item))
        {
            PlayerErrorMessage(PlayerError.BadExamine, ctx);
            _badCmdBefore = "examine";
            return;
        }

        var id = Disambiguate(item, _currentRoom + ";inventory", ctx);

        if (id <= 0)
        {
            if (id != -2)
            {
                PlayerErrorMessage(PlayerError.BadThing, ctx);
            }

            _badCmdBefore = "examine";
            return;
        }

        var o = _objs[id];

        // Find "examine" action:
        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if (o.Actions[i].ActionName == "examine")
            {
                ExecuteScript(o.Actions[i].Script, ctx, id);
                return;
            }
        }

        // Find "examine" property:
        for (int i = 1, loopTo1 = o.NumberProperties; i <= loopTo1; i++)
        {
            if (o.Properties[i].PropertyName == "examine")
            {
                Print(o.Properties[i].PropertyValue, ctx);
                return;
            }
        }

        // Find "examine" tag:
        for (int i = o.DefinitionSectionStart + 1, loopTo2 = _objs[id].DefinitionSectionEnd - 1; i <= loopTo2; i++)
        {
            if (BeginsWith(_lines[i], "examine "))
            {
                var action = Strings.Trim(GetEverythingAfter(_lines[i], "examine "));
                if (Strings.Left(action, 1) == "<")
                {
                    Print(GetParameter(_lines[i], ctx), ctx);
                }
                else
                {
                    ExecuteScript(action, ctx, id);
                }

                return;
            }
        }

        DoLook(id, ctx, true);
    }

    private void ExecMoveThing(string data, Thing type, Context ctx)
    {
        var scp = Strings.InStr(data, ";");
        var name = Strings.Trim(Strings.Left(data, scp - 1));
        var place = Strings.Trim(Strings.Mid(data, scp + 1));
        MoveThing(name, place, type, ctx);
    }

    private void ExecProperty(string data, Context ctx)
    {
        var id = default(int);
        var found = default(bool);
        var scp = Strings.InStr(data, ";");

        if (scp == 0)
        {
            LogASLError("No property data given in 'property <" + data + ">'", LogType.WarningError);
            return;
        }

        var name = Strings.Trim(Strings.Left(data, scp - 1));
        var properties = Strings.Trim(Strings.Mid(data, scp + 1));

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object in 'property <" + data + ">'", LogType.WarningError);
            return;
        }

        AddToObjectProperties(properties, id, ctx);
    }

    private void ExecuteDo(string procedureName, Context ctx)
    {
        var newCtx = CopyContext(ctx);
        var numParameters = 0;
        bool useNewCtx;

        if ((ASLVersion >= 392) & (Strings.Left(procedureName, 8) == "!intproc"))
            // If "do" procedure is run in a new context, context info is not passed to any nested
            // script blocks in braces.
        {
            useNewCtx = false;
        }
        else
        {
            useNewCtx = true;
        }

        if (ASLVersion >= 284)
        {
            var obp = Strings.InStr(procedureName, "(");
            var cbp = default(int);
            if (obp != 0)
            {
                cbp = Strings.InStr(obp + 1, procedureName, ")");
            }

            if ((obp != 0) & (cbp != 0))
            {
                var parameters = Strings.Mid(procedureName, obp + 1, cbp - obp - 1);
                procedureName = Strings.Left(procedureName, obp - 1);

                parameters = parameters + ";";
                var pos = 1;
                do
                {
                    numParameters = numParameters + 1;
                    var scp = Strings.InStr(pos, parameters, ";");

                    newCtx.NumParameters = numParameters;
                    Array.Resize(ref newCtx.Parameters, numParameters + 1);
                    newCtx.Parameters[numParameters] = Strings.Trim(Strings.Mid(parameters, pos, scp - pos));

                    pos = scp + 1;
                } while (pos < Strings.Len(parameters));
            }
        }

        var block = DefineBlockParam("procedure", procedureName);
        if ((block.StartLine == 0) & (block.EndLine == 0))
        {
            LogASLError("No such procedure " + procedureName, LogType.WarningError);
        }
        else
        {
            for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
            {
                if (!useNewCtx)
                {
                    ExecuteScript(_lines[i], ctx);
                }
                else
                {
                    ExecuteScript(_lines[i], newCtx);
                    ctx.DontProcessCommand = newCtx.DontProcessCommand;
                }
            }
        }
    }

    private void ExecuteDoAction(string data, Context ctx)
    {
        var id = default(int);

        var scp = Strings.InStr(data, ";");
        if (scp == 0)
        {
            LogASLError("No action name specified in 'doaction <" + data + ">'");
            return;
        }

        var objName = Strings.LCase(Strings.Trim(Strings.Left(data, scp - 1)));
        var actionName = Strings.Trim(Strings.Mid(data, scp + 1));
        var found = false;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (objName ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object '" + objName + "'");
            return;
        }

        DoAction(id, actionName, ctx);
    }

    private bool ExecuteIfHere(string obj, Context ctx)
    {
        if (ASLVersion <= 281)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if (((_chars[i].ContainerRoom ?? "") == (_currentRoom ?? "")) & _chars[i].Exists)
                {
                    if ((Strings.LCase(obj) ?? "") == (Strings.LCase(_chars[i].ObjectName) ?? ""))
                    {
                        return true;
                    }
                }
            }
        }

        for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
        {
            if (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(_currentRoom) ?? "")) &
                _objs[i].Exists)
            {
                if ((Strings.LCase(obj) ?? "") == (Strings.LCase(_objs[i].ObjectName) ?? ""))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool ExecuteIfExists(string obj, bool realOnly)
    {
        var result = default(bool);
        var errorReport = false;
        int scp;

        if (Strings.InStr(obj, ";") != 0)
        {
            scp = Strings.InStr(obj, ";");
            if (Strings.LCase(Strings.Trim(Strings.Mid(obj, scp + 1))) == "report")
            {
                errorReport = true;
            }

            obj = Strings.Left(obj, scp - 1);
        }

        var found = false;

        if (ASLVersion < 281)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if ((Strings.LCase(obj) ?? "") == (Strings.LCase(_chars[i].ObjectName) ?? ""))
                {
                    result = _chars[i].Exists;
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if ((Strings.LCase(obj) ?? "") == (Strings.LCase(_objs[i].ObjectName) ?? ""))
                {
                    result = _objs[i].Exists;
                    found = true;
                    break;
                }
            }
        }

        if ((found == false) & errorReport)
        {
            LogASLError("No such character/object '" + obj + "'.", LogType.UserError);
        }

        if (found == false)
        {
            result = false;
        }

        if (realOnly)
        {
            return found;
        }

        return result;
    }

    private bool ExecuteIfProperty(string data)
    {
        var id = default(int);
        var scp = Strings.InStr(data, ";");

        if (scp == 0)
        {
            LogASLError("No property name given in condition 'property <" + data + ">' ...", LogType.WarningError);
            return false;
        }

        var objName = Strings.Trim(Strings.Left(data, scp - 1));
        var propertyName = Strings.Trim(Strings.Mid(data, scp + 1));
        var found = false;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object '" + objName + "' in condition 'property <" + data + ">' ...",
                LogType.WarningError);
            return false;
        }

        return GetObjectProperty(propertyName, id, true) == "yes";
    }

    private void ExecuteRepeat(string data, Context ctx)
    {
        bool repeatWhileTrue;
        var repeatScript = "";
        int bracketPos;
        string afterBracket;
        var foundScript = false;

        if (BeginsWith(data, "while "))
        {
            repeatWhileTrue = true;
            data = GetEverythingAfter(data, "while ");
        }
        else if (BeginsWith(data, "until "))
        {
            repeatWhileTrue = false;
            data = GetEverythingAfter(data, "until ");
        }
        else
        {
            LogASLError("Expected 'until' or 'while' in 'repeat " + ReportErrorLine(data) + "'", LogType.WarningError);
            return;
        }

        var pos = 1;
        do
        {
            bracketPos = Strings.InStr(pos, data, ">");
            afterBracket = Strings.Trim(Strings.Mid(data, bracketPos + 1));
            if (!BeginsWith(afterBracket, "and ") & !BeginsWith(afterBracket, "or "))
            {
                repeatScript = afterBracket;
                foundScript = true;
            }
            else
            {
                pos = bracketPos + 1;
            }
        } while (!(foundScript | string.IsNullOrEmpty(afterBracket)));

        var conditions = Strings.Trim(Strings.Left(data, bracketPos));
        var finished = false;

        do
        {
            if (ExecuteConditions(conditions, ctx) == repeatWhileTrue)
            {
                ExecuteScript(repeatScript, ctx);
            }
            else
            {
                finished = true;
            }
        } while (!(finished | _gameFinished));
    }

    private void ExecuteSetCollectable(string param, Context ctx)
    {
        double val;
        var id = default(int);
        var scp = Strings.InStr(param, ";");
        var name = Strings.Trim(Strings.Left(param, scp - 1));
        var newVal = Strings.Trim(Strings.Right(param, Strings.Len(param) - scp));
        var found = false;

        for (int i = 1, loopTo = _numCollectables; i <= loopTo; i++)
        {
            if ((_collectables[i].Name ?? "") == (name ?? ""))
            {
                id = i;
                found = true;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such collectable '" + param + "'", LogType.WarningError);
            return;
        }

        var op = Strings.Left(newVal, 1);
        var newValue = Strings.Trim(Strings.Right(newVal, Strings.Len(newVal) - 1));
        if (Information.IsNumeric(newValue))
        {
            val = Conversion.Val(newValue);
        }
        else
        {
            val = GetCollectableAmount(newValue);
        }

        if (op == "+")
        {
            _collectables[id].Value = _collectables[id].Value + val;
        }
        else if (op == "-")
        {
            _collectables[id].Value = _collectables[id].Value - val;
        }
        else if (op == "=")
        {
            _collectables[id].Value = val;
        }

        CheckCollectable(id);
        UpdateItems(ctx);
    }

    private void ExecuteWait(string waitLine, Context ctx)
    {
        if (!string.IsNullOrEmpty(waitLine))
        {
            Print(GetParameter(waitLine, ctx), ctx);
        }
        else if (ASLVersion >= 410)
        {
            PlayerErrorMessage(PlayerError.DefaultWait, ctx);
        }
        else
        {
            Print("|nPress a key to continue...", ctx);
        }

        DoWait();
    }

    private void InitFileData(string fileData)
    {
        _fileData = fileData;
        _fileDataPos = 1;
    }

    private string GetNextChunk()
    {
        var nullPos = Strings.InStr(_fileDataPos, _fileData, "\0");
        var result = Strings.Mid(_fileData, _fileDataPos, nullPos - _fileDataPos);

        if (nullPos < Strings.Len(_fileData))
        {
            _fileDataPos = nullPos + 1;
        }

        return result;
    }

    public string GetFileDataChars(int count)
    {
        var result = Strings.Mid(_fileData, _fileDataPos, count);
        _fileDataPos = _fileDataPos + count;
        return result;
    }

    private ActionType GetObjectActions(string actionInfo)
    {
        var name = Strings.LCase(GetParameter(actionInfo, _nullContext));
        var ep = Strings.InStr(actionInfo, ">");
        if (ep == Strings.Len(actionInfo))
        {
            LogASLError("No script given for '" + name + "' action data", LogType.WarningError);
            return new ActionType();
        }

        var script = Strings.Trim(Strings.Mid(actionInfo, ep + 1));
        var result = new ActionType();
        result.ActionName = name;
        result.Script = script;
        return result;
    }

    private int GetObjectId(string name, Context ctx, string room = "")
    {
        var id = default(int);
        var found = false;

        if (BeginsWith(name, "the "))
        {
            name = GetEverythingAfter(name, "the ");
        }

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) |
                 ((Strings.LCase(_objs[i].ObjectName) ?? "") == ("the " + Strings.LCase(name) ?? ""))) &
                (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) |
                 string.IsNullOrEmpty(room)) & _objs[i].Exists)
            {
                id = i;
                found = true;
                break;
            }
        }

        if (!found & (ASLVersion >= 280))
        {
            id = Disambiguate(name, room, ctx);
            if (id > 0)
            {
                found = true;
            }
        }

        if (found)
        {
            return id;
        }

        return -1;
    }

    private int GetObjectIdNoAlias(string name)
    {
        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                return i;
            }
        }

        return 0;
    }

    internal string GetObjectProperty(string name, int id, bool existsOnly = false, bool logError = true)
    {
        var result = "";
        var found = false;
        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberProperties; i <= loopTo; i++)
        {
            if ((Strings.LCase(o.Properties[i].PropertyName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                found = true;
                result = o.Properties[i].PropertyValue;
                break;
            }
        }

        if (existsOnly)
        {
            if (found)
            {
                return "yes";
            }

            return "no";
        }

        if (found)
        {
            return result;
        }

        if (logError)
        {
            LogASLError("Object '" + _objs[id].ObjectName + "' has no property '" + name + "'", LogType.WarningError);
            return "!";
        }

        return "";
    }

    private PropertiesActions GetPropertiesInType(string type, bool err = true)
    {
        var blockId = default(int);
        var propertyList = new PropertiesActions();
        var found = false;

        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define type"))
            {
                if ((Strings.LCase(GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext)) ?? "") ==
                    (Strings.LCase(type) ?? ""))
                {
                    blockId = i;
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            if (err)
            {
                LogASLError("No such type '" + type + "'", LogType.WarningError);
            }

            return new PropertiesActions();
        }

        for (int i = _defineBlocks[blockId].StartLine + 1, loopTo1 = _defineBlocks[blockId].EndLine - 1;
             i <= loopTo1;
             i++)
        {
            if (BeginsWith(_lines[i], "type "))
            {
                var typeName = Strings.LCase(GetParameter(_lines[i], _nullContext));
                var newProperties = GetPropertiesInType(typeName);
                propertyList.Properties = propertyList.Properties + newProperties.Properties;
                Array.Resize(ref propertyList.Actions, propertyList.NumberActions + newProperties.NumberActions + 1);
                for (int j = propertyList.NumberActions + 1,
                     loopTo2 = propertyList.NumberActions + newProperties.NumberActions;
                     j <= loopTo2;
                     j++)
                {
                    propertyList.Actions[j] = new ActionType();
                    propertyList.Actions[j].ActionName =
                        newProperties.Actions[j - propertyList.NumberActions].ActionName;
                    propertyList.Actions[j].Script = newProperties.Actions[j - propertyList.NumberActions].Script;
                }

                propertyList.NumberActions = propertyList.NumberActions + newProperties.NumberActions;

                // Add this type name to the TypesIncluded list...
                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + 1;
                Array.Resize(ref propertyList.TypesIncluded, propertyList.NumberTypesIncluded + 1);
                propertyList.TypesIncluded[propertyList.NumberTypesIncluded] = typeName;

                // and add the names of the types included by it...
                Array.Resize(ref propertyList.TypesIncluded,
                    propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded + 1);
                for (int j = propertyList.NumberTypesIncluded + 1,
                     loopTo3 = propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded;
                     j <= loopTo3;
                     j++)
                {
                    propertyList.TypesIncluded[j] = newProperties.TypesIncluded[j - propertyList.NumberTypesIncluded];
                }

                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded;
            }
            else if (BeginsWith(_lines[i], "action "))
            {
                propertyList.NumberActions = propertyList.NumberActions + 1;
                Array.Resize(ref propertyList.Actions, propertyList.NumberActions + 1);
                propertyList.Actions[propertyList.NumberActions] =
                    GetObjectActions(GetEverythingAfter(_lines[i], "action "));
            }
            else if (BeginsWith(_lines[i], "properties "))
            {
                propertyList.Properties = propertyList.Properties + GetParameter(_lines[i], _nullContext) + ";";
            }
            else if (!string.IsNullOrEmpty(Strings.Trim(_lines[i])))
            {
                propertyList.Properties = propertyList.Properties + _lines[i] + ";";
            }
        }

        return propertyList;
    }

    internal int GetRoomID(string name, Context ctx)
    {
        if (Strings.InStr(name, "[") > 0)
        {
            var idx = GetArrayIndex(name, ctx);
            name = name + Strings.Trim(Conversion.Str(idx.Index));
        }

        for (int i = 1, loopTo = _numberRooms; i <= loopTo; i++)
        {
            if ((Strings.LCase(_rooms[i].RoomName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                return i;
            }
        }

        return 0;
    }

    private TextAction GetTextOrScript(string textScript)
    {
        var result = new TextAction();
        textScript = Strings.Trim(textScript);

        if (Strings.Left(textScript, 1) == "<")
        {
            result.Type = TextActionType.Text;
            result.Data = GetParameter(textScript, _nullContext);
        }
        else
        {
            result.Type = TextActionType.Script;
            result.Data = textScript;
        }

        return result;
    }

    private int GetThingNumber(string name, string room, Thing type)
    {
        // Returns the number in the Chars() or _objs() array of the specified char/obj

        if (type == Thing.Character)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if ((!string.IsNullOrEmpty(room) &
                     ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) &
                     ((Strings.LCase(_chars[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? ""))) |
                    (string.IsNullOrEmpty(room) &
                     ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))))
                {
                    return i;
                }
            }
        }
        else if (type == Thing.Object)
        {
            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if ((!string.IsNullOrEmpty(room) &
                     ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) &
                     ((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? ""))) |
                    (string.IsNullOrEmpty(room) &
                     ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private DefineBlock GetThingBlock(string name, string room, Thing type)
    {
        // Returns position where specified char/obj is defined in ASL code

        var result = new DefineBlock();

        if (type == Thing.Character)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if (((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) &
                    ((Strings.LCase(_chars[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")))
                {
                    result.StartLine = _chars[i].DefinitionSectionStart;
                    result.EndLine = _chars[i].DefinitionSectionEnd;
                    return result;
                }
            }
        }
        else if (type == Thing.Object)
        {
            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if (((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) &
                    ((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")))
                {
                    result.StartLine = _objs[i].DefinitionSectionStart;
                    result.EndLine = _objs[i].DefinitionSectionEnd;
                    return result;
                }
            }
        }

        result.StartLine = 0;
        result.EndLine = 0;
        return result;
    }

    private string MakeRestoreData()
    {
        var data = new StringBuilder();
        var objectData = new ChangeType[1];
        var roomData = new ChangeType[1];
        var numObjectData = default(int);
        var numRoomData = default(int);

        // <<< FILE HEADER DATA >>>

        data.Append("QUEST300" + '\0' + GetOriginalFilenameForQSG() + '\0');

        // The start point for encrypted data is after the filename
        var start = data.Length + 1;

        data.Append(_currentRoom + '\0');

        // Organise Change Log

        for (int i = 1, loopTo = _gameChangeData.NumberChanges; i <= loopTo; i++)
        {
            if (BeginsWith(_gameChangeData.ChangeData[i].AppliesTo, "object "))
            {
                numObjectData = numObjectData + 1;
                Array.Resize(ref objectData, numObjectData + 1);
                objectData[numObjectData] = new ChangeType();
                objectData[numObjectData] = _gameChangeData.ChangeData[i];
            }
            else if (BeginsWith(_gameChangeData.ChangeData[i].AppliesTo, "room "))
            {
                numRoomData = numRoomData + 1;
                Array.Resize(ref roomData, numRoomData + 1);
                roomData[numRoomData] = new ChangeType();
                roomData[numRoomData] = _gameChangeData.ChangeData[i];
            }
        }

        // <<< OBJECT CREATE/CHANGE DATA >>>

        data.Append(Strings.Trim(Conversion.Str(numObjectData + _changeLogObjects.Changes.Count)) + '\0');

        for (int i = 1, loopTo1 = numObjectData; i <= loopTo1; i++)
        {
            data.Append(GetEverythingAfter(objectData[i].AppliesTo, "object ") + '\0' + objectData[i].Change + '\0');
        }

        foreach (var key in _changeLogObjects.Changes.Keys)
        {
            var appliesTo = Strings.Split(key, "#")[0];
            var changeData = _changeLogObjects.Changes[key];

            data.Append(appliesTo + '\0' + changeData + '\0');
        }

        // <<< OBJECT EXIST/VISIBLE/ROOM DATA >>>

        data.Append(Strings.Trim(Conversion.Str(_numberObjs)) + '\0');

        for (int i = 1, loopTo2 = _numberObjs; i <= loopTo2; i++)
        {
            string exists;
            string visible;

            if (_objs[i].Exists)
            {
                exists = "\u0001";
            }
            else
            {
                exists = "\0";
            }

            if (_objs[i].Visible)
            {
                visible = "\u0001";
            }
            else
            {
                visible = "\0";
            }

            data.Append(_objs[i].ObjectName + '\0' + exists + visible + _objs[i].ContainerRoom + '\0');
        }

        // <<< ROOM CREATE/CHANGE DATA >>>

        data.Append(Strings.Trim(Conversion.Str(numRoomData)) + '\0');

        for (int i = 1, loopTo3 = numRoomData; i <= loopTo3; i++)
        {
            data.Append(GetEverythingAfter(roomData[i].AppliesTo, "room ") + '\0' + roomData[i].Change + '\0');
        }

        // <<< TIMER STATE DATA >>>

        data.Append(Strings.Trim(Conversion.Str(_numberTimers)) + '\0');

        for (int i = 1, loopTo4 = _numberTimers; i <= loopTo4; i++)
        {
            var t = _timers[i];
            data.Append(t.TimerName + '\0');

            if (t.TimerActive)
            {
                data.Append('\u0001');
            }
            else
            {
                data.Append('\0');
            }

            data.Append(Strings.Trim(Conversion.Str(t.TimerInterval)) + '\0');
            data.Append(Strings.Trim(Conversion.Str(t.TimerTicks)) + '\0');
        }

        // <<< STRING VARIABLE DATA >>>

        data.Append(Strings.Trim(Conversion.Str(_numberStringVariables)) + '\0');

        for (int i = 1, loopTo5 = _numberStringVariables; i <= loopTo5; i++)
        {
            var s = _stringVariable[i];
            data.Append(s.VariableName + '\0' + Strings.Trim(Conversion.Str(s.VariableUBound)) + '\0');

            for (int j = 0, loopTo6 = s.VariableUBound; j <= loopTo6; j++)
            {
                data.Append(s.VariableContents[j] + '\0');
            }
        }

        // <<< NUMERIC VARIABLE DATA >>>

        data.Append(Strings.Trim(Conversion.Str(_numberNumericVariables)) + '\0');

        for (int i = 1, loopTo7 = _numberNumericVariables; i <= loopTo7; i++)
        {
            var n = _numericVariable[i];
            data.Append(n.VariableName + '\0' + Strings.Trim(Conversion.Str(n.VariableUBound)) + '\0');

            for (int j = 0, loopTo8 = n.VariableUBound; j <= loopTo8; j++)
            {
                data.Append(n.VariableContents[j] + '\0');
            }
        }

        // Now encrypt data
        string dataString;
        var newFileData = new StringBuilder();

        dataString = data.ToString();

        newFileData.Append(Strings.Left(dataString, start - 1));

        for (int i = start, loopTo9 = Strings.Len(dataString); i <= loopTo9; i++)
        {
            newFileData.Append(Strings.Chr(255 - Strings.Asc(Strings.Mid(dataString, i, 1))));
        }

        return newFileData.ToString();
    }

    private void MoveThing(string name, string room, Thing type, Context ctx)
    {
        var oldRoom = "";

        var id = GetThingNumber(name, "", type);

        if (Strings.InStr(room, "[") > 0)
        {
            var idx = GetArrayIndex(room, ctx);
            room = room + Strings.Trim(Conversion.Str(idx.Index));
        }

        if (type == Thing.Character)
        {
            _chars[id].ContainerRoom = room;
        }
        else if (type == Thing.Object)
        {
            oldRoom = _objs[id].ContainerRoom;
            _objs[id].ContainerRoom = room;
        }

        if (ASLVersion >= 391)
            // If this object contains other objects, move them too
        {
            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if ((Strings.LCase(GetObjectProperty("parent", i, false, false)) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    MoveThing(_objs[i].ObjectName, room, type, ctx);
                }
            }
        }

        UpdateObjectList(ctx);

        if (BeginsWith(Strings.LCase(room), "inventory") | BeginsWith(Strings.LCase(oldRoom), "inventory"))
        {
            UpdateItems(ctx);
        }
    }

    public void Pause(int duration)
    {
        _player.DoPause(duration);
        ChangeState(State.Waiting);

        lock (_waitLock)
        {
            Monitor.Wait(_waitLock);
        }
    }

    private string ConvertParameter(string parameter, string convertChar, ConvertType action, Context ctx)
    {
        // Returns a string with functions, string and
        // numeric variables executed or converted as
        // appropriate, read for display/etc.

        var result = "";
        var pos = 1;
        var finished = false;

        do
        {
            var varPos = Strings.InStr(pos, parameter, convertChar);
            if (varPos == 0)
            {
                varPos = Strings.Len(parameter) + 1;
                finished = true;
            }

            var currentBit = Strings.Mid(parameter, pos, varPos - pos);
            result = result + currentBit;

            if (finished == false)
            {
                var nextPos = Strings.InStr(varPos + 1, parameter, convertChar);

                if (nextPos == 0)
                {
                    LogASLError("Line parameter <" + parameter + "> has missing " + convertChar, LogType.WarningError);
                    return "<ERROR>";
                }

                var varName = Strings.Mid(parameter, varPos + 1, nextPos - 1 - varPos);

                if (string.IsNullOrEmpty(varName))
                {
                    result = result + convertChar;
                }

                else if (action == ConvertType.Strings)
                {
                    result = result + GetStringContents(varName, ctx);
                }
                else if (action == ConvertType.Functions)
                {
                    varName = EvaluateInlineExpressions(varName);
                    result = result + DoFunction(varName, ctx);
                }
                else if (action == ConvertType.Numeric)
                {
                    result = result + Strings.Trim(Conversion.Str(GetNumericContents(varName, ctx)));
                }
                else if (action == ConvertType.Collectables)
                {
                    result = result + Strings.Trim(Conversion.Str(GetCollectableAmount(varName)));
                }

                pos = nextPos + 1;
            }
        } while (!finished);

        return result;
    }

    private string DoFunction(string data, Context ctx)
    {
        string name, parameter;
        var intFuncResult = "";
        var intFuncExecuted = false;
        var paramPos = Strings.InStr(data, "(");

        if (paramPos != 0)
        {
            name = Strings.Trim(Strings.Left(data, paramPos - 1));
            var endParamPos = Strings.InStrRev(data, ")");
            if (endParamPos == 0)
            {
                LogASLError("Expected ) in $" + data + "$", LogType.WarningError);
                return "";
            }

            parameter = Strings.Mid(data, paramPos + 1, endParamPos - paramPos - 1);
        }
        else
        {
            name = data;
            parameter = "";
        }

        DefineBlock block;
        block = DefineBlockParam("function", name);

        if ((block.StartLine == 0) & (block.EndLine == 0))
        {
            // Function does not exist; try an internal function.
            intFuncResult = DoInternalFunction(name, parameter, ctx);
            if (intFuncResult == "__NOTDEFINED")
            {
                LogASLError("No such function '" + name + "'", LogType.WarningError);
                return "[ERROR]";
            }

            intFuncExecuted = true;
        }

        if (intFuncExecuted)
        {
            return intFuncResult;
        }

        var newCtx = CopyContext(ctx);
        var numParameters = 0;
        var pos = 1;

        if (!string.IsNullOrEmpty(parameter))
        {
            parameter = parameter + ";";
            do
            {
                numParameters = numParameters + 1;
                var scp = Strings.InStr(pos, parameter, ";");

                var parameterData = Strings.Trim(Strings.Mid(parameter, pos, scp - pos));
                SetStringContents("quest.function.parameter." + Strings.Trim(Conversion.Str(numParameters)),
                    parameterData, ctx);

                newCtx.NumParameters = numParameters;
                Array.Resize(ref newCtx.Parameters, numParameters + 1);
                newCtx.Parameters[numParameters] = parameterData;

                pos = scp + 1;
            } while (pos < Strings.Len(parameter));

            SetStringContents("quest.function.numparameters", Strings.Trim(Conversion.Str(numParameters)), ctx);
        }
        else
        {
            SetStringContents("quest.function.numparameters", "0", ctx);
            newCtx.NumParameters = 0;
        }

        var result = "";

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            ExecuteScript(_lines[i], newCtx);
            result = newCtx.FunctionReturnValue;
        }

        return result;
    }

    private string DoInternalFunction(string name, string parameter, Context ctx)
    {
        var parameters = default(string[]);
        var untrimmedParameters = default(string[]);
        var objId = default(int);
        var numParameters = 0;
        var pos = 1;

        if (!string.IsNullOrEmpty(parameter))
        {
            parameter = parameter + ";";
            do
            {
                numParameters = numParameters + 1;
                var scp = Strings.InStr(pos, parameter, ";");
                Array.Resize(ref parameters, numParameters + 1);
                Array.Resize(ref untrimmedParameters, numParameters + 1);

                untrimmedParameters[numParameters] = Strings.Mid(parameter, pos, scp - pos);
                parameters[numParameters] = Strings.Trim(untrimmedParameters[numParameters]);

                pos = scp + 1;
            } while (pos < Strings.Len(parameter));

            // Remove final ";"
            parameter = Strings.Left(parameter, Strings.Len(parameter) - 1);
        }
        else
        {
            numParameters = 1;
            parameters = new string[2];
            untrimmedParameters = new string[2];
            parameters[1] = "";
            untrimmedParameters[1] = "";
        }

        string param2;
        string param3;

        if (name == "displayname")
        {
            objId = GetObjectId(parameters[1], ctx);
            if (objId == -1)
            {
                LogASLError("Object '" + parameters[1] + "' does not exist", LogType.WarningError);
                return "!";
            }

            return _objs[GetObjectId(parameters[1], ctx)].ObjectAlias;
        }

        if (name == "numberparameters")
        {
            return Strings.Trim(Conversion.Str(ctx.NumParameters));
        }

        if (name == "parameter")
        {
            if (numParameters == 0)
            {
                LogASLError("No parameter number specified for $parameter$ function", LogType.WarningError);
                return "";
            }

            if (Conversion.Val(parameters[1]) > ctx.NumParameters)
            {
                LogASLError("No parameter number " + parameters[1] + " sent to this function", LogType.WarningError);
                return "";
            }

            return Strings.Trim(ctx.Parameters[Conversions.ToInteger(parameters[1])]);
        }

        if (name == "gettag")
            // Deprecated
        {
            return FindStatement(DefineBlockParam("room", parameters[1]), parameters[2]);
        }

        if (name == "objectname")
        {
            return _objs[ctx.CallingObjectId].ObjectName;
        }

        if (name == "locationof")
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return _chars[i].ContainerRoom;
                }
            }

            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return _objs[i].ContainerRoom;
                }
            }
        }
        else if (name == "lengthof")
        {
            return Conversion.Str(Strings.Len(untrimmedParameters[1]));
        }
        else if (name == "left")
        {
            if (Conversion.Val(parameters[2]) < 0d)
            {
                LogASLError("Invalid function call in '$Left$(" + parameters[1] + "; " + parameters[2] + ")$'",
                    LogType.WarningError);
                return "!";
            }

            return Strings.Left(parameters[1], Conversions.ToInteger(parameters[2]));
        }
        else if (name == "right")
        {
            if (Conversion.Val(parameters[2]) < 0d)
            {
                LogASLError("Invalid function call in '$Right$(" + parameters[1] + "; " + parameters[2] + ")$'",
                    LogType.WarningError);
                return "!";
            }

            return Strings.Right(parameters[1], Conversions.ToInteger(parameters[2]));
        }
        else if (name == "mid")
        {
            if (numParameters == 3)
            {
                if (Conversion.Val(parameters[2]) < 0d)
                {
                    LogASLError(
                        "Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + "; " +
                        parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                }

                return Strings.Mid(parameters[1], Conversions.ToInteger(parameters[2]),
                    Conversions.ToInteger(parameters[3]));
            }

            if (numParameters == 2)
            {
                if (Conversion.Val(parameters[2]) < 0d)
                {
                    LogASLError("Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + ")$'",
                        LogType.WarningError);
                    return "!";
                }

                return Strings.Mid(parameters[1], Conversions.ToInteger(parameters[2]));
            }

            LogASLError("Invalid function call to '$Mid$(...)$'", LogType.WarningError);
            return "";
        }
        else if (name == "rand")
        {
            return Conversion.Str(
                Conversion.Int(_random.NextDouble() *
                               (Conversions.ToDouble(parameters[2]) - Conversions.ToDouble(parameters[1]) + 1d)) +
                Conversions.ToDouble(parameters[1]));
        }
        else if (name == "instr")
        {
            if (numParameters == 3)
            {
                param3 = "";
                if (Strings.InStr(parameters[3], "_") != 0)
                {
                    for (int i = 1, loopTo2 = Strings.Len(parameters[3]); i <= loopTo2; i++)
                    {
                        if (Strings.Mid(parameters[3], i, 1) == "_")
                        {
                            param3 = param3 + " ";
                        }
                        else
                        {
                            param3 = param3 + Strings.Mid(parameters[3], i, 1);
                        }
                    }
                }
                else
                {
                    param3 = parameters[3];
                }

                if (Conversion.Val(parameters[1]) <= 0d)
                {
                    LogASLError(
                        "Invalid function call in '$instr(" + parameters[1] + "; " + parameters[2] + "; " +
                        parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                }

                return Strings.Trim(Conversion.Str(Strings.InStr(Conversions.ToInteger(parameters[1]), parameters[2],
                    param3)));
            }

            if (numParameters == 2)
            {
                param2 = "";
                if (Strings.InStr(parameters[2], "_") != 0)
                {
                    for (int i = 1, loopTo3 = Strings.Len(parameters[2]); i <= loopTo3; i++)
                    {
                        if (Strings.Mid(parameters[2], i, 1) == "_")
                        {
                            param2 = param2 + " ";
                        }
                        else
                        {
                            param2 = param2 + Strings.Mid(parameters[2], i, 1);
                        }
                    }
                }
                else
                {
                    param2 = parameters[2];
                }

                return Strings.Trim(Conversion.Str(Strings.InStr(parameters[1], param2)));
            }

            LogASLError("Invalid function call to '$Instr$(...)$'", LogType.WarningError);
            return "";
        }
        else if (name == "ucase")
        {
            return Strings.UCase(parameters[1]);
        }
        else if (name == "lcase")
        {
            return Strings.LCase(parameters[1]);
        }
        else if (name == "capfirst")
        {
            return Strings.UCase(Strings.Left(parameters[1], 1)) + Strings.Mid(parameters[1], 2);
        }
        else if (name == "symbol")
        {
            if (parameters[1] == "lt")
            {
                return "<";
            }

            if (parameters[1] == "gt")
            {
                return ">";
            }

            return "!";
        }
        else if (name == "loadmethod")
        {
            return _gameLoadMethod;
        }
        else if (name == "timerstate")
        {
            for (int i = 1, loopTo4 = _numberTimers; i <= loopTo4; i++)
            {
                if ((Strings.LCase(_timers[i].TimerName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    if (_timers[i].TimerActive)
                    {
                        return "1";
                    }

                    return "0";
                }
            }

            LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "timerinterval")
        {
            for (int i = 1, loopTo5 = _numberTimers; i <= loopTo5; i++)
            {
                if ((Strings.LCase(_timers[i].TimerName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return Conversion.Str(_timers[i].TimerInterval);
                }
            }

            LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "ubound")
        {
            for (int i = 1, loopTo6 = _numberNumericVariables; i <= loopTo6; i++)
            {
                if ((Strings.LCase(_numericVariable[i].VariableName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return Strings.Trim(Conversion.Str(_numericVariable[i].VariableUBound));
                }
            }

            for (int i = 1, loopTo7 = _numberStringVariables; i <= loopTo7; i++)
            {
                if ((Strings.LCase(_stringVariable[i].VariableName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return Strings.Trim(Conversion.Str(_stringVariable[i].VariableUBound));
                }
            }

            LogASLError("No such variable '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "objectproperty")
        {
            var FoundObj = false;
            for (int i = 1, loopTo8 = _numberObjs; i <= loopTo8; i++)
            {
                if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    FoundObj = true;
                    objId = i;
                    i = _numberObjs;
                }
            }

            if (!FoundObj)
            {
                LogASLError("No such object '" + parameters[1] + "'", LogType.WarningError);
                return "!";
            }

            return GetObjectProperty(parameters[2], objId);
        }
        else if (name == "getobjectname")
        {
            if (numParameters == 3)
            {
                objId = Disambiguate(parameters[1], parameters[2] + ";" + parameters[3], ctx);
            }
            else if (numParameters == 2)
            {
                objId = Disambiguate(parameters[1], parameters[2], ctx);
            }
            else
            {
                objId = Disambiguate(parameters[1], _currentRoom + ";inventory", ctx);
            }

            if (objId <= -1)
            {
                LogASLError("No object found with display name '" + parameters[1] + "'", LogType.WarningError);
                return "!";
            }

            return _objs[objId].ObjectName;
        }
        else if (name == "thisobject")
        {
            return _objs[ctx.CallingObjectId].ObjectName;
        }
        else if (name == "thisobjectname")
        {
            return _objs[ctx.CallingObjectId].ObjectAlias;
        }
        else if (name == "speechenabled")
        {
            return "1";
        }
        else if (name == "removeformatting")
        {
            return RemoveFormatting(parameter);
        }
        else if ((name == "findexit") & (ASLVersion >= 410))
        {
            var e = FindExit(parameter);
            if (e is null)
            {
                return "";
            }

            return _objs[e.GetObjId()].ObjectName;
        }

        return "__NOTDEFINED";
    }

    private void ExecFor(string line, Context ctx)
    {
        // See if this is a "for each" loop:
        if (BeginsWith(line, "each "))
        {
            ExecForEach(GetEverythingAfter(line, "each "), ctx);
            return;
        }

        // Executes a for loop, of form:
        // for <variable; startvalue; endvalue> script
        int endValue;
        int stepValue;
        var forData = GetParameter(line, ctx);

        // Extract individual components:
        var scp1 = Strings.InStr(forData, ";");
        var scp2 = Strings.InStr(scp1 + 1, forData, ";");
        var scp3 = Strings.InStr(scp2 + 1, forData, ";");
        var counterVariable = Strings.Trim(Strings.Left(forData, scp1 - 1));
        var startValue = Conversions.ToInteger(Strings.Mid(forData, scp1 + 1, scp2 - 1 - scp1));

        if (scp3 != 0)
        {
            endValue = Conversions.ToInteger(Strings.Mid(forData, scp2 + 1, scp3 - 1 - scp2));
            stepValue = Conversions.ToInteger(Strings.Mid(forData, scp3 + 1));
        }
        else
        {
            endValue = Conversions.ToInteger(Strings.Mid(forData, scp2 + 1));
            stepValue = 1;
        }

        var loopScript = Strings.Trim(Strings.Mid(line, Strings.InStr(line, ">") + 1));

        for (double i = startValue, loopTo = endValue;
             (double) stepValue >= 0 ? i <= loopTo : i >= loopTo;
             i += stepValue)
        {
            SetNumericVariableContents(counterVariable, i, ctx);
            ExecuteScript(loopScript, ctx);
            i = GetNumericContents(counterVariable, ctx);
        }
    }

    private void ExecSetVar(string varInfo, Context ctx)
    {
        // Sets variable contents from a script parameter.
        // Eg <var1;7> sets numeric variable var1
        // to 7

        var scp = Strings.InStr(varInfo, ";");
        var varName = Strings.Trim(Strings.Left(varInfo, scp - 1));
        var varCont = Strings.Trim(Strings.Mid(varInfo, scp + 1));
        var idx = GetArrayIndex(varName, ctx);

        if (Information.IsNumeric(idx.Name))
        {
            LogASLError("Invalid numeric variable name '" + idx.Name + "' - variable names cannot be numeric",
                LogType.WarningError);
            return;
        }

        try
        {
            if (ASLVersion >= 391)
            {
                var expResult = ExpressionHandler(varCont);
                if (expResult.Success == ExpressionSuccess.OK)
                {
                    varCont = expResult.Result;
                }
                else
                {
                    varCont = "0";
                    LogASLError("Error setting numeric variable <" + varInfo + "> : " + expResult.Message,
                        LogType.WarningError);
                }
            }
            else
            {
                var obscuredVarInfo = ObscureNumericExps(varCont);
                var opPos = Strings.InStr(obscuredVarInfo, "+");
                if (opPos == 0)
                {
                    opPos = Strings.InStr(obscuredVarInfo, "*");
                }

                if (opPos == 0)
                {
                    opPos = Strings.InStr(obscuredVarInfo, "/");
                }

                if (opPos == 0)
                {
                    opPos = Strings.InStr(2, obscuredVarInfo, "-");
                }

                if (opPos != 0)
                {
                    var op = Strings.Mid(varCont, opPos, 1);
                    var num1 = Conversion.Val(Strings.Left(varCont, opPos - 1));
                    var num2 = Conversion.Val(Strings.Mid(varCont, opPos + 1));

                    switch (op ?? "")
                    {
                        case "+":
                        {
                            varCont = Conversion.Str(num1 + num2);
                            break;
                        }
                        case "-":
                        {
                            varCont = Conversion.Str(num1 - num2);
                            break;
                        }
                        case "*":
                        {
                            varCont = Conversion.Str(num1 * num2);
                            break;
                        }
                        case "/":
                        {
                            if (num2 != 0d)
                            {
                                varCont = Conversion.Str(num1 / num2);
                            }
                            else
                            {
                                LogASLError("Division by zero - The result of this operation has been set to zero.",
                                    LogType.WarningError);
                                varCont = "0";
                            }

                            break;
                        }
                    }
                }
            }

            SetNumericVariableContents(idx.Name, Conversion.Val(varCont), ctx, idx.Index);
        }
        catch
        {
            LogASLError("Error setting variable '" + idx.Name + "' to '" + varCont + "'", LogType.WarningError);
        }
    }

    private bool ExecuteIfAsk(string question)
    {
        _player.ShowQuestion(question);
        ChangeState(State.Waiting);

        lock (_waitLock)
        {
            Monitor.Wait(_waitLock);
        }

        return _questionResponse;
    }

    private void SetQuestionResponse(bool response)
    {
        var runnerThread = new Thread(SetQuestionResponseInNewThread);
        ChangeState(State.Working);
        runnerThread.Start(response);
        WaitForStateChange(State.Working);
    }

    void IASL.SetQuestionResponse(bool response)
    {
        SetQuestionResponse(response);
    }

    private void SetQuestionResponseInNewThread(object response)
    {
        _questionResponse = (bool) response;

        lock (_waitLock)
        {
            Monitor.PulseAll(_waitLock);
        }
    }

    private bool ExecuteIfGot(string item)
    {
        if (ASLVersion >= 280)
        {
            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(item) ?? ""))
                {
                    return (_objs[i].ContainerRoom == "inventory") & _objs[i].Exists;
                }
            }

            LogASLError("No object '" + item + "' defined.", LogType.WarningError);
            return false;
        }

        for (int i = 1, loopTo1 = _numberItems; i <= loopTo1; i++)
        {
            if ((Strings.LCase(_items[i].Name) ?? "") == (Strings.LCase(item) ?? ""))
            {
                return _items[i].Got;
            }
        }

        LogASLError("Item '" + item + "' not defined.", LogType.WarningError);
        return false;
    }

    private bool ExecuteIfHas(string condition)
    {
        double checkValue;
        var colNum = default(int);
        var scp = Strings.InStr(condition, ";");
        var name = Strings.Trim(Strings.Left(condition, scp - 1));
        var newVal = Strings.Trim(Strings.Right(condition, Strings.Len(condition) - scp));
        var found = false;

        for (int i = 1, loopTo = _numCollectables; i <= loopTo; i++)
        {
            if ((_collectables[i].Name ?? "") == (name ?? ""))
            {
                colNum = i;
                found = true;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such collectable in " + condition, LogType.WarningError);
            return false;
        }

        var op = Strings.Left(newVal, 1);
        var newValue = Strings.Trim(Strings.Right(newVal, Strings.Len(newVal) - 1));
        if (Information.IsNumeric(newValue))
        {
            checkValue = Conversion.Val(newValue);
        }
        else
        {
            checkValue = GetCollectableAmount(newValue);
        }

        if (op == "+")
        {
            return _collectables[colNum].Value > checkValue;
        }

        if (op == "-")
        {
            return _collectables[colNum].Value < checkValue;
        }

        if (op == "=")
        {
            return _collectables[colNum].Value == checkValue;
        }

        return false;
    }

    private bool ExecuteIfIs(string condition)
    {
        string value1, value2;
        string op;
        var expectNumerics = default(bool);
        ExpressionResult expResult;

        var scp = Strings.InStr(condition, ";");
        if (scp == 0)
        {
            LogASLError("Expected second parameter in 'is " + condition + "'", LogType.WarningError);
            return false;
        }

        var scp2 = Strings.InStr(scp + 1, condition, ";");
        if (scp2 == 0)
        {
            // Only two parameters => standard "="
            op = "=";
            value1 = Strings.Trim(Strings.Left(condition, scp - 1));
            value2 = Strings.Trim(Strings.Mid(condition, scp + 1));
        }
        else
        {
            value1 = Strings.Trim(Strings.Left(condition, scp - 1));
            op = Strings.Trim(Strings.Mid(condition, scp + 1, scp2 - scp - 1));
            value2 = Strings.Trim(Strings.Mid(condition, scp2 + 1));
        }

        if (ASLVersion >= 391)
        {
            // Evaluate expressions in Value1 and Value2
            expResult = ExpressionHandler(value1);

            if (expResult.Success == ExpressionSuccess.OK)
            {
                value1 = expResult.Result;
            }

            expResult = ExpressionHandler(value2);

            if (expResult.Success == ExpressionSuccess.OK)
            {
                value2 = expResult.Result;
            }
        }

        var result = false;

        switch (op ?? "")
        {
            case "=":
            {
                if ((Strings.LCase(value1) ?? "") == (Strings.LCase(value2) ?? ""))
                {
                    result = true;
                }

                expectNumerics = false;
                break;
            }
            case "!=":
            {
                if ((Strings.LCase(value1) ?? "") != (Strings.LCase(value2) ?? ""))
                {
                    result = true;
                }

                expectNumerics = false;
                break;
            }
            case "gt":
            {
                if (Conversion.Val(value1) > Conversion.Val(value2))
                {
                    result = true;
                }

                expectNumerics = true;
                break;
            }
            case "lt":
            {
                if (Conversion.Val(value1) < Conversion.Val(value2))
                {
                    result = true;
                }

                expectNumerics = true;
                break;
            }
            case "gt=":
            {
                if (Conversion.Val(value1) >= Conversion.Val(value2))
                {
                    result = true;
                }

                expectNumerics = true;
                break;
            }
            case "lt=":
            {
                if (Conversion.Val(value1) <= Conversion.Val(value2))
                {
                    result = true;
                }

                expectNumerics = true;
                break;
            }

            default:
            {
                LogASLError("Unrecognised comparison condition in 'is " + condition + "'", LogType.WarningError);
                break;
            }
        }

        if (expectNumerics)
        {
            if (!(Information.IsNumeric(value1) & Information.IsNumeric(value2)))
            {
                LogASLError("Expected numeric comparison comparing '" + value1 + "' and '" + value2 + "'",
                    LogType.WarningError);
            }
        }

        return result;
    }

    private double GetNumericContents(string name, Context ctx, bool noError = false)
    {
        var numNumber = default(int);
        int arrayIndex;
        var exists = false;

        // First, see if the variable already exists. If it
        // does, get its contents. If not, generate an error.

        if ((Strings.InStr(name, "[") != 0) & (Strings.InStr(name, "]") != 0))
        {
            var op = Strings.InStr(name, "[");
            var cp = Strings.InStr(name, "]");
            var arrayIndexData = Strings.Mid(name, op + 1, cp - op - 1);
            if (Information.IsNumeric(arrayIndexData))
            {
                arrayIndex = Conversions.ToInteger(arrayIndexData);
            }
            else
            {
                arrayIndex = (int) Math.Round(GetNumericContents(arrayIndexData, ctx));
            }

            name = Strings.Left(name, op - 1);
        }
        else
        {
            arrayIndex = 0;
        }

        if (_numberNumericVariables > 0)
        {
            for (int i = 1, loopTo = _numberNumericVariables; i <= loopTo; i++)
            {
                if ((Strings.LCase(_numericVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    numNumber = i;
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
        {
            if (!noError)
            {
                LogASLError("No numeric variable '" + name + "' defined.", LogType.WarningError);
            }

            return -32767;
        }

        if (arrayIndex > _numericVariable[numNumber].VariableUBound)
        {
            if (!noError)
            {
                LogASLError("Array index of '" + name + "[" + Strings.Trim(Conversion.Str(arrayIndex)) + "]' too big.",
                    LogType.WarningError);
            }

            return -32766;
        }

        // Now, set the contents
        return Conversion.Val(_numericVariable[numNumber].VariableContents[arrayIndex]);
    }

    internal void PlayerErrorMessage(PlayerError e, Context ctx)
    {
        Print(GetErrorMessage(e, ctx), ctx);
    }

    private void PlayerErrorMessage_ExtendInfo(PlayerError e, Context ctx, string extraInfo)
    {
        var message = GetErrorMessage(e, ctx);

        if (!string.IsNullOrEmpty(extraInfo))
        {
            if (Strings.Right(message, 1) == ".")
            {
                message = Strings.Left(message, Strings.Len(message) - 1);
            }

            message = message + " - " + extraInfo + ".";
        }

        Print(message, ctx);
    }

    private string GetErrorMessage(PlayerError e, Context ctx)
    {
        return ConvertParameter(
            ConvertParameter(ConvertParameter(_playerErrorMessageString[(int) e], "%", ConvertType.Numeric, ctx), "$",
                ConvertType.Functions, ctx), "#", ConvertType.Strings, ctx);
    }

    private void PlayMedia(string filename)
    {
        PlayMedia(filename, false, false);
    }

    private void PlayMedia(string filename, bool sync, bool looped)
    {
        if (filename.Length == 0)
        {
            _player.StopSound();
        }
        else
        {
            if (looped & sync)
            {
                sync = false; // Can't loop and sync at the same time, that would just hang!
            }

            _player.PlaySound(filename, sync, looped);

            if (sync)
            {
                ChangeState(State.Waiting);
            }

            if (sync)
            {
                lock (_waitLock)
                {
                    Monitor.Wait(_waitLock);
                }
            }
        }
    }

    private void PlayWav(string parameter)
    {
        var sync = false;
        var looped = false;
        var @params = new List<string>(parameter.Split(';'));

        @params = new List<string>(@params.Select(p => Strings.Trim(p)));

        var filename = @params[0];

        if (@params.Contains("loop"))
        {
            looped = true;
        }

        if (@params.Contains("sync"))
        {
            sync = true;
        }

        if ((filename.Length > 0) & (Strings.InStr(filename, ".") == 0))
        {
            filename = filename + ".wav";
        }

        PlayMedia(filename, sync, looped);
    }
}