using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using QuestViva.Common;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game : IGame, IGameDebug
{
    private readonly string[] _casKeywords = new string[256]; // Tokenised CAS keywords
    private readonly GameChangeDataType _gameChangeData = new();
    private readonly GameData _gameData;
    private readonly Dictionary<ListType, List<string>> _listVerbs = new();
    private readonly List<string> _log = new();
    private readonly int _numSkipCheckFiles;
    private readonly string[] _playerErrorMessageString = new string[39];
    private readonly GameRandom _random = new();
    private readonly Stream _saveData;
    private readonly string[] _skipCheckFile;
    private readonly TextFormatter _textFormatter = new();
    private string _afterTurnScript;
    private bool _autoIntro;
    private string _badCmdAfter;
    private string _badCmdBefore;
    private string _beforeSaveScript;
    private string _beforeTurnScript;
    private string _casFileData;
    private ChangeLog _changeLogObjects;
    private ChangeLog _changeLogRooms;
    private ObjectType[] _chars;
    private int _choiceNumber;
    private Collectable[] _collectables;
    private bool _commandOverrideModeOn;
    private string _commandOverrideVariable;
    private List<ListData> _compassExits = new();
    internal string _currentRoom;
    private string _defaultFontName;
    private double _defaultFontSize;
    private PropertiesActions _defaultProperties;
    private PropertiesActions _defaultRoomProperties;
    private Dictionary<string, Dictionary<string, string>> _defineBlockParams;
    private DefineBlock[] _defineBlocks; // Stores the start and end lines of each 'define' section
    private string _fileData;
    private int _fileDataPos;
    private bool _gameFinished;
    private bool _gameFullyLoaded;
    private bool _gameIsRestoring;
    private bool _gameLoading;
    private string _gameLoadMethod;
    private string _gameName; // The name of the game
    private List<ListData> _gotoExits = new();
    private bool _hasResources;
    private ItemType[] _items;
    private int _lastIt;
    private ItType _lastItMode;
    private string[] _lines; // Stores the lines of the ASL script/definitions
    private bool _loadedFromQsg;
    internal Context _nullContext = new();
    private int _numberChars;
    private int _numberItems;
    private int _numberNumericVariables;
    internal int _numberObjs;
    internal int _numberRooms;
    private int _numberSections; // Number of define sections
    private int _numberStringVariables;
    private int _numberSynonyms;
    private int _numberTimers;
    private int _numCollectables;
    private int _numDisplayNumerics;
    private int _numDisplayStrings;
    private VariableType[] _numericVariable;
    private int _numResources;
    internal ObjectType[] _objs;
    private string _onLoadScript;
    private string _openErrorReport;
    private bool _outPutOn;
    private IPlayer _player;
    private bool _questionResponse;
    private bool _readyForCommand = true;
    private TaskCompletionSource _turnSuspendedTcs;
    private TaskCompletionSource _waitTcs;
    private TaskCompletionSource _commandTcs;
    private int _resourceOffset;
    private ResourceType[] _resources;
    internal RoomType[] _rooms;
    private int _startCatPos;
    private VariableType[] _stringVariable;
    private SynonymType[] _synonyms;
    private int _thisTurnIt;
    private ItType _thisTurnItMode;
    private TimerType[] _timers;
    private bool _useAbbreviations;
    private bool _useStaticFrameForPictures;

    public V4Game(GameData gameData, Stream saveData)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        LoadCASKeywords();
        _gameLoadMethod = "normal";
        _gameData = gameData;
        _saveData = saveData;

        // Very early versions of Quest didn't perform very good syntax checking of ASL files, so this is
        // for compatibility with games which have non-fatal errors in them.
        _numSkipCheckFiles = 3;
        _skipCheckFile = new string[4];
        _skipCheckFile[1] = "bargain.cas";
        _skipCheckFile[2] = "easymoney.asl";
        _skipCheckFile[3] = "musicvf1.cas";
    }

    async Task IGame.SetQuestionResponse(bool response)
    {
        _questionResponse = response;
        _turnSuspendedTcs = new TaskCompletionSource();
        _waitTcs?.TrySetResult();
        await _turnSuspendedTcs.Task;
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

    private void SignalTurnSuspended()
    {
        _readyForCommand = true;
        // Mid-turn while a wait/pause/ask/menu/sync sound (_waitTcs) or an enter (_commandTcs) is
        // outstanding: a save holds only world state, not the suspended script, so the player
        // disables Save until the turn finishes. Derived from the TCSs rather than passed in by
        // each caller because an enter's answer is its own HandleCommandAsync, whose finally runs
        // after the resumed script may already have suspended again at the next prompt.
        _player.SetTurnPending(_waitTcs is { Task.IsCompleted: false } || _commandTcs is { Task.IsCompleted: false });
        // Fires on every suspension, including one inside Begin() (an "enter" in the start script
        // never lets Begin() return), so a buffering IPlayer flushes the prompt that led to it.
        // The turn is taken first because a handler can answer the prompt, starting a turn of its
        // own that this signal mustn't resolve.
        var turn = _turnSuspendedTcs;
        TurnSuspended?.Invoke();
        turn?.TrySetResult();
    }

    private async Task DoWaitAsync()
    {
        _player.DoWait();
        _waitTcs = new TaskCompletionSource();
        SignalTurnSuspended();
        await _waitTcs.Task;
    }

    private async Task ExecuteWaitAsync(string waitLine, Context ctx)
    {
        if (!string.IsNullOrEmpty(waitLine))
        {
            await Print(await GetParameter(waitLine, ctx), ctx);
        }
        else if (ASLVersion >= 410)
        {
            await PlayerErrorMessage(PlayerError.DefaultWait, ctx);
        }
        else
        {
            await Print("|nPress a key to continue...", ctx);
        }

        await DoWaitAsync();
    }

    public async Task PauseAsync(int duration)
    {
        _player.DoPause(duration);
        _waitTcs = new TaskCompletionSource();
        SignalTurnSuspended();
        await _waitTcs.Task;
    }

    private async Task<bool> ExecuteIfAskAsync(string question)
    {
        // Quest 4 games have no WorldModel version, so never the v600 inline rendering.
        _player.ShowQuestion(question, false);
        _waitTcs = new TaskCompletionSource();
        SignalTurnSuspended();
        await _waitTcs.Task;
        return _questionResponse;
    }
    private string _menuResponse;

    public int ASLVersion { get; private set; }

    public async Task Begin()
    {
        await DoBeginAsync();
    }

    public List<string> Errors => new();

    public void Finish()
    {
        GameFinished();
    }

    public event FinishedHandler Finished;

    public event ErrorHandler LogError;

    public event PrintTextHandler PrintText;

    public event Action TurnSuspended;

    public Task SendCommand(string command)
    {
        return SendCommand(command, 0, null);
    }

    public async Task SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata)
    {
        if (!_readyForCommand) return;
        _readyForCommand = false;
        _turnSuspendedTcs = new TaskCompletionSource();
        _ = HandleCommandAsync(command);
        await _turnSuspendedTcs.Task;
        if (elapsedTime > 0) await Tick(elapsedTime);
        else RaiseNextTimerTickRequest();
    }

    public Task SendEvent(string eventName, string param)
    {
        return Task.CompletedTask;
    }

    public int RandomSeed => _random.Seed;

    public void SetRandomSeed(int seed) => _random.Reseed(seed);

    public event UpdateListHandler UpdateList;

    public async Task<bool> Initialise(IPlayer player)
    {
        _player = player;
        if (_saveData != null)
        {
            return await OpenGame(_saveData);
        }

        return await InitialiseGame(_gameData);
    }

    public async Task Tick(int elapsedTime)
    {
        int i;
        var timerScripts = new List<string>();

        Debug.Print("Tick: " + elapsedTime);

        var loopTo = _numberTimers;
        for (i = 1; i <= loopTo; i++)
        {
            if (_timers[i].TimerActive)
            {
                if (_timers[i].BypassThisTurn)
                {
                    // don't trigger timer during the turn it was first enabled
                    _timers[i].BypassThisTurn = false;
                }
                else
                {
                    _timers[i].TimerTicks = _timers[i].TimerTicks + elapsedTime;

                    if (_timers[i].TimerTicks >= _timers[i].TimerInterval)
                    {
                        _timers[i].TimerTicks = 0;
                        timerScripts.Add(_timers[i].TimerAction);
                    }
                }
            }
        }

        if (timerScripts.Count > 0)
        {
            _turnSuspendedTcs = new TaskCompletionSource();
            _ = RunTimersAsync(timerScripts);
            await _turnSuspendedTcs.Task;
        }

        RaiseNextTimerTickRequest();
    }

    public async Task FinishWait()
    {
        _turnSuspendedTcs = new TaskCompletionSource();
        _waitTcs?.TrySetResult();
        await _turnSuspendedTcs.Task;
    }

    public Task FinishPause() => FinishWait();

    public async Task SetMenuResponse(string response)
    {
        _menuResponse = response;
        _turnSuspendedTcs = new TaskCompletionSource();
        _waitTcs?.TrySetResult();
        await _turnSuspendedTcs.Task;
    }

    public IEnumerable<string> GetExternalScripts()
    {
        return null;
    }

    public IEnumerable<string> GetExternalStylesheets()
    {
        return null;
    }

    public string GetGameStylesheet()
    {
        return null;
    }

    public event Action<int> RequestNextTimerTick;

    public Stream GetResourceStream(string file)
    {
        if (file == "_game.cas")
        {
            return new MemoryStream(GetResourcelessCAS());
        }

        return GetResourceStreamInternal(file);
    }

    public string GameID => null;

    public IEnumerable<string> GetResourceNames()
    {
        for (int i = 1, loopTo = _numResources; i <= loopTo; i++)
        {
            // Resources are always extracted with lowercase filenames (see ExtractFile)
            yield return _resources[i].ResourceName.ToLowerInvariant();
        }

        if (_numResources > 0)
        {
            yield return "_game.cas";
        }
    }

    private void SetTimerState(string name, bool state)
    {
        for (int i = 1, loopTo = _numberTimers; i <= loopTo; i++)
        {
            if ((Strings.LCase(name) ?? "") == (Strings.LCase(_timers[i].TimerName) ?? ""))
            {
                _timers[i].TimerActive = state;
                _timers[i].BypassThisTurn = true; // don't trigger timer during the turn it was first enabled
                return;
            }
        }

        LogASLError("No such timer '" + name + "'", LogType.WarningError);
    }

    private async Task ExecuteEnterAsync(string scriptLine, Context ctx)
    {
        _commandOverrideModeOn = true;
        _commandOverrideVariable = await GetParameter(scriptLine, ctx);
        _commandTcs = new TaskCompletionSource();
        SignalTurnSuspended();
        await _commandTcs.Task;
        _commandOverrideModeOn = false;
    }

    private async Task FinishGame(StopType stopType, Context ctx)
    {
        if (stopType == StopType.Win)
        {
            await DisplayTextSection("win", ctx);
        }
        else if (stopType == StopType.Lose)
        {
            await DisplayTextSection("lose", ctx);
        }

        GameFinished();
    }

    private async Task DoBeginAsync()
    {
        var gameBlock = GetDefineBlock("game");
        var ctx = new Context();

        SetFont("");
        SetFontSize(0d);

        for (int i = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             i <= loopTo;
             i++)
        {
            if (BeginsWith(_lines[i], "background "))
            {
                SetBackground(await GetParameter(_lines[i], _nullContext));
            }
        }

        for (int i = GetDefineBlock("game").StartLine + 1, loopTo1 = GetDefineBlock("game").EndLine - 1;
             i <= loopTo1;
             i++)
        {
            if (BeginsWith(_lines[i], "foreground "))
            {
                SetForeground(await GetParameter(_lines[i], _nullContext));
            }
        }

        // Execute any startscript command that appears in the
        // "define game" block:

        _autoIntro = true;

        // For ASL>=391, we only run startscripts if LoadMethod is normal (i.e. we haven't started
        // from a saved QSG file)

        if ((ASLVersion < 391) | ((ASLVersion >= 391) & (_gameLoadMethod == "normal")))
        {
            // for GameASLVersion 311 and later, any library startscript is executed first:
            if (ASLVersion >= 311)
            {
                // We go through the game block executing these in reverse order, as
                // the statements which are included last should be executed first.
                for (int i = gameBlock.EndLine - 1, loopTo2 = gameBlock.StartLine + 1; i >= loopTo2; i -= 1)
                {
                    if (BeginsWith(_lines[i], "lib startscript "))
                    {
                        ctx = _nullContext;
                        await ExecuteScript(Strings.Trim(GetEverythingAfter(Strings.Trim(_lines[i]), "lib startscript ")),
                            ctx);
                    }
                }
            }

            for (int i = gameBlock.StartLine + 1, loopTo3 = gameBlock.EndLine - 1; i <= loopTo3; i++)
            {
                if (BeginsWith(_lines[i], "startscript "))
                {
                    ctx = _nullContext;
                    await ExecuteScript(Strings.Trim(GetEverythingAfter(Strings.Trim(_lines[i]), "startscript")), ctx);
                }
                else if (BeginsWith(_lines[i], "lib startscript ") & (ASLVersion < 311))
                {
                    ctx = _nullContext;
                    await ExecuteScript(Strings.Trim(GetEverythingAfter(Strings.Trim(_lines[i]), "lib startscript ")), ctx);
                }
            }
        }

        _gameFullyLoaded = true;

        // Display intro text
        if (_autoIntro & (_gameLoadMethod == "normal"))
        {
            await DisplayTextSection("intro", _nullContext);
        }

        // Start game from room specified by "start" statement
        var startRoom = "";
        for (int i = gameBlock.StartLine + 1, loopTo4 = gameBlock.EndLine - 1; i <= loopTo4; i++)
        {
            if (BeginsWith(_lines[i], "start "))
            {
                startRoom = await GetParameter(_lines[i], _nullContext);
            }
        }

        if (!_loadedFromQsg)
        {
            ctx = _nullContext;
            await PlayGame(startRoom, ctx);
            await Print("", _nullContext);
        }
        else
        {
            await UpdateItems(_nullContext);

            await Print("Restored saved game", _nullContext);
            await Print("", _nullContext);
            await PlayGame(_currentRoom, _nullContext);
            await Print("", _nullContext);

            if (ASLVersion >= 391)
            {
                // For ASL>=391, OnLoad is now run for all games.
                ctx = _nullContext;
                await ExecuteScript(_onLoadScript, ctx);
            }
        }

        RaiseNextTimerTickRequest();

        SignalTurnSuspended();
    }



    private async Task HandleCommandAsync(string command)
    {
        try
        {
            await ExecCommand(command, new Context());
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
        finally
        {
            SignalTurnSuspended();
        }
    }

    private void GameFinished()
    {
        _gameFinished = true;
        Finished?.Invoke();
        _readyForCommand = false;
        _waitTcs?.TrySetResult();
        _commandTcs?.TrySetResult();
        _turnSuspendedTcs?.TrySetResult();
    }

    private async Task RunTimersAsync(List<string> timerScripts)
    {
        foreach (var script in timerScripts)
        {
            try
            {
                await ExecuteScript(script, _nullContext);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        SignalTurnSuspended();
    }

    private void RaiseNextTimerTickRequest()
    {
        var anyTimerActive = false;
        var nextTrigger = 60;

        for (int i = 1, loopTo = _numberTimers; i <= loopTo; i++)
        {
            if (_timers[i].TimerActive)
            {
                anyTimerActive = true;

                var thisNextTrigger = _timers[i].TimerInterval - _timers[i].TimerTicks;
                if (thisNextTrigger < nextTrigger)
                {
                    nextTrigger = thisNextTrigger;
                }
            }
        }

        if (!anyTimerActive)
        {
            nextTrigger = 0;
        }

        if (_gameFinished)
        {
            nextTrigger = 0;
        }

        Debug.Print("RaiseNextTimerTickRequest " + nextTrigger);

        RequestNextTimerTick?.Invoke(nextTrigger);
    }





    private async Task<string> ShowMenuAsync(MenuData menuData)
    {
        _player.ShowMenu(menuData);
        _waitTcs = new TaskCompletionSource();
        SignalTurnSuspended();
        await _waitTcs.Task;
        return _menuResponse;
    }



    private void LogException(Exception ex)
    {
        LogError?.Invoke(ex);
    }
}
