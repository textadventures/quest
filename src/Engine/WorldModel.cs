using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using QuestViva.Common;
using QuestViva.Engine.Functions;
using QuestViva.Engine.GameLoader;
using QuestViva.Engine.Scripts;
using QuestViva.Utility;

namespace QuestViva.Engine;

public partial class WorldModel : IGame, IGameDebug
{
    private static readonly Dictionary<string, Type> TypeNamesToTypes = new()
    {
        {"string", typeof(string)},
        {"script", typeof(IScript)},
        {"boolean", typeof(bool)},
        {"int", typeof(int)},
        {"double", typeof(double)},
        {"object", typeof(Element)},
        {"stringlist", typeof(QuestList<string>)},
        {"objectlist", typeof(QuestList<Element>)},
        {"stringdictionary", typeof(QuestDictionary<string>)},
        {"objectdictionary", typeof(QuestDictionary<Element>)},
        {"scriptdictionary", typeof(QuestDictionary<IScript>)},
        {"dictionary", typeof(QuestDictionary<object>)},
        {"list", typeof(QuestList<object>)}
    };

    private static readonly Dictionary<Type, string> TypesToTypeNames = new();

    private static List<string>? FunctionNames;
    private readonly List<string> _attributeNames = [];
    private readonly Dictionary<string, ElementType> _debuggerElementTypes = new();
    private readonly Dictionary<string, ObjectType> _debuggerObjectTypes = new();
    private readonly Dictionary<ElementType, IElementFactory> _elementFactories = new();
    private readonly GameData? _gameData;
    private readonly Dictionary<string, int> _nextUniqueId = new();
    private readonly Stream? _saveData;

    internal bool _commandOverride;

    private LegacyOutputLogger? _legacyOutputLogger;

    private bool _loadedFromSaved;
    private IPlayer? _playerUi;

    private GameSaver? _saver;
    private TimerRunner? _timerRunner;

    internal TaskCompletionSource? _waitTcs;
    internal TaskCompletionSource<string?>? _menuTcs;
    internal TaskCompletionSource<bool>? _questionTcs;
    internal TaskCompletionSource<string>? _commandInputTcs;
    internal TaskCompletionSource? _pauseTcs;
    private TaskCompletionSource _turnSuspendedTcs = new();

    // Tracks show menu / ask / get input callbacks that fire-and-forget their response handling.
    // on ready defers until this reaches zero.
    private int _pendingCallbackCount;
    private readonly List<(IScript Script, Context Context)> _onReadyQueue = [];

    private Walkthroughs? _walkthroughs;

    static WorldModel()
    {
        foreach (var kvp in TypeNamesToTypes)
        {
            TypesToTypeNames.Add(kvp.Value, kvp.Key);
        }
    }

    internal WorldModel()
        // ReSharper disable once IntroduceOptionalParameters.Global
        : this(null, null)
    {
    }

    public WorldModel(GameData? gameData, Stream? saveData)
    {
        ExpressionOwner = new ExpressionOwner(this);
        Template = new Template(this);
        InitialiseElementFactories();
        ObjectFactory = (ObjectFactory) _elementFactories[ElementType.Object];

        InitialiseDebuggerObjectTypes();
        _gameData = gameData;
        _saveData = saveData;
        Elements = new Elements();
        UndoLogger = new UndoLogger(this);
        Game = ObjectFactory.CreateObject("game", ObjectType.Game);
    }

    public Func<string, Stream>? ResourceGetter { get; internal set; }
    public Func<IEnumerable<string>>? GetResourceNames { get; internal set; }

    internal static Dictionary<ObjectType, string> DefaultTypeNames { get; } = new()
    {
        {ObjectType.Object, "defaultobject"},
        {ObjectType.Exit, "defaultexit"},
        {ObjectType.Command, "defaultcommand"},
        {ObjectType.Game, "defaultgame"},
        {ObjectType.TurnScript, "defaultturnscript"}
    };

    public Element Game { get; }

    public ObjectFactory ObjectFactory { get; }

    public IEnumerable<Element> Objects => Elements.Objects;

    public string Filename => _gameData?.Filename ?? string.Empty;

    internal Template Template { get; }

    public UndoLogger UndoLogger { get; }

    public GameState State { get; private set; } = GameState.NotStarted;

    public Elements Elements { get; }

    public bool EditMode { get; private set; }

    internal ExpressionOwner ExpressionOwner { get; }

    internal IPlayer PlayerUi
    {
        get
        {
            if (_playerUi == null)
            {
                throw new Exception("Player UI not set");
            }

            return _playerUi;
        }
        private set => _playerUi = value;
    }

    public IEnumerable<string> GetAllAttributeNames => _attributeNames.AsReadOnly();

    internal RegexCache RegexCache { get; } = new();

    public WorldModelVersion Version { get; internal set; }

    internal string? VersionString { get; set; }

    internal IOutputLogger? OutputLogger { get; private set; }

    public int ASLVersion => int.Parse(VersionString!);

    public string Category => Game.Fields[FieldDefinitions.Category];
    public string Description => Game.Fields[FieldDefinitions.Description];
    public string Cover => Game.Fields[FieldDefinitions.Cover];
    public bool IsGamebook => Game.Fields[FieldDefinitions.EditorStyle] == "gamebook";
    public string? LanguageId => Template.GetText("LanguageId", false);
    public event Action<int>? RequestNextTimerTick;
    public event PrintTextHandler? PrintText;
    public event UpdateListHandler? UpdateList;
    public event FinishedHandler? Finished;
    public event ErrorHandler? LogError;

    public async Task SetMenuResponse(string? response)
    {
        var tcs = _menuTcs;
        _turnSuspendedTcs = new();
        tcs?.TrySetResult(response);
        await _turnSuspendedTcs.Task;
    }

    public async Task<bool> Initialise(IPlayer player)
    {
        EditMode = false;
        PlayerUi = player;
        var loader = new GameLoader.GameLoader(this, GameLoader.GameLoader.LoadMode.Play, _gameData?.IsCompiled);
        var result = await InitialiseInternal(loader);
        if (result)
        {
            _walkthroughs = new Walkthroughs(this);
        }

        return result;
    }

    public void Begin() => _ = BeginAsync();

    public Task BeginAsync()
    {
        _ = BeginInternalAsync();
        SendNextTimerRequest();
        return _turnSuspendedTcs.Task;
    }

    private async Task BeginInternalAsync()
    {
        _turnSuspendedTcs = new();
        try
        {
            _timerRunner = new TimerRunner(this, !_loadedFromSaved);
            if (Version <= WorldModelVersion.v540)
            {
                PlayerUi.Show("Panes");
                PlayerUi.Show("Location");
                PlayerUi.Show("Command");
            }

            if (Elements.ContainsKey(ElementType.Function, "InitInterface"))
            {
                await RunProcedureAsync("InitInterface");
            }

            if (!_loadedFromSaved)
            {
                if (Elements.ContainsKey(ElementType.Function, "StartGame"))
                {
                    await RunProcedureAsync("StartGame");
                }
            }

            await TryRunOnFinallyScriptsAsync();
            await UpdateListsAsync();

            if (_loadedFromSaved)
            {
                var output = Elements.GetSingle(ElementType.Output);
                if (output == null)
                {
                    await PrintAsync("Loaded saved game");
                }
                else if (Version >= WorldModelVersion.v540)
                {
                    PlayerUi.RunScript("loadHtml", [output.Fields.GetString("html")]);
                    PlayerUi.RunScript("markScrollPosition", null);
                    ScrollToEnd();
                }
                else if (_legacyOutputLogger != null)
                {
                    _legacyOutputLogger.DisplayOutput(output.Fields.GetString("text"));
                }
            }

            SendNextTimerRequest();
        }
        catch (Exception ex)
        {
            LogException(ex);
            Finish();
        }
        finally
        {
            SignalTurnSuspended();
        }
    }

    public List<string> Errors { get; private set; } = [];

    public Task SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata)
    {
        if (_timerRunner == null)
        {
            throw new Exception("Begin() has not been called");
        }

        if (elapsedTime > 0)
        {
            _timerRunner.IncrementTime(elapsedTime);
        }

        // HandleCommandAsyncInternal sets _turnSuspendedTcs synchronously before its first real await,
        // so capturing .Task after the fire-and-forget start is safe.
        _ = HandleCommandAsyncInternal(command, metadata);

        if (elapsedTime > 0)
        {
            _ = Tick(0);
        }
        else
        {
            SendNextTimerRequest();
        }

        return _turnSuspendedTcs.Task;
    }

    private async Task HandleCommandAsyncInternal(string command, IDictionary<string, string> metadata)
    {
        _turnSuspendedTcs = new();
        var commandWasOverridden = false;
        try
        {
            if (!_commandOverride)
            {
                if (Version < WorldModelVersion.v520)
                {
                    await PrintAsync("");
                    await PrintAsync("> " + Utility.SafeXML(command));
                }

                await RunProcedureAsync("HandleCommand", new Parameters(new Dictionary<string, object>
                {
                    {"command", command},
                    {"metadata", new QuestDictionary<string>(metadata)}
                }), false);

                if (Version < WorldModelVersion.v580)
                {
                    await TryFinishTurnAsync();
                }

                if (State != GameState.Finished)
                {
                    await UpdateListsAsync();
                }
            }
            else
            {
                commandWasOverridden = true;
                _commandOverride = false;
                _commandInputTcs?.TrySetResult(command);
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
        finally
        {
            // When command was overridden (get input / GetInput function), the callback fires
            // asynchronously via AwaitResponseAndRunCallbackAsync, which calls SignalTurnSuspended()
            // in its own finally after the callback script runs. Signaling here would complete the
            // turn before the callback output is produced, causing it to appear only on the next turn.
            if (!commandWasOverridden) SignalTurnSuspended();
        }
    }

    public Task SendCommand(string command)
    {
        return SendCommand(command, 0, ReadOnlyDictionary<string, string>.Empty);
    }

    public async Task SendEvent(string eventName, string param)
    {
        Elements.TryGetValue(ElementType.Function, eventName, out var handler);

        if (handler == null)
        {
            await PrintAsync($"Error - no handler for event '{eventName}'");
            return;
        }

        var parameters = new Parameters {{(string) handler.Fields[FieldDefinitions.ParamNames][0], param}};

        await RunProcedureAsync(eventName, parameters, false);

        switch (Version)
        {
            case < WorldModelVersion.v540:
                return;
            case < WorldModelVersion.v580:
                await TryFinishTurnAsync();
                break;
        }

        if (State != GameState.Finished)
        {
            await UpdateListsAsync();
        }

        SendNextTimerRequest();
    }

    public void Finish()
    {
        FinishGame();
    }

    public async Task FinishWait()
    {
        if (State == GameState.Finished) return;
        var tcs = _waitTcs;
        _turnSuspendedTcs = new();
        tcs?.TrySetResult();
        await _turnSuspendedTcs.Task;
    }

    public async Task FinishPause()
    {
        var tcs = _pauseTcs;
        _turnSuspendedTcs = new();
        tcs?.TrySetResult();
        await _turnSuspendedTcs.Task;
    }

    public IEnumerable<string> GetExternalScripts()
    {
        var result = new List<string>();
        foreach (var jsRef in Elements.GetElements(ElementType.Javascript))
        {
            if (Version == WorldModelVersion.v500)
            {
                // v500 games used Frame.js functions for static panel feature. This is now implemented natively
                // in Player and WebPlayer.
                if (jsRef.Fields[FieldDefinitions.Src].Equals("frame.js", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
            }

            result.Add(jsRef.Fields[FieldDefinitions.Src]);
        }

        return result;
    }

    // TO DO: This could actually be removed now, as we can dynamically load stylesheets. Core.aslx InitInterface
    // should simply be able to use the SetWebFontName function to load game.defaultwebfont
    public IEnumerable<string> GetExternalStylesheets()
    {
        if (Version < WorldModelVersion.v530)
        {
            return [];
        }

        var webFontsInUse = new List<string>();
        var defaultWebFont = Game.Fields[FieldDefinitions.DefaultWebFont];
        if (!string.IsNullOrEmpty(defaultWebFont))
        {
            webFontsInUse.Add(defaultWebFont);
        }

        var result = webFontsInUse.Select(f => "https://fonts.googleapis.com/css?family=" + f.Replace(' ', '+'));

        return result;
    }

    public Task<byte[]> SaveAsync(string html)
    {
        var saveData = Save(SaveMode.SavedGame, html: html);
        return Task.FromResult(Encoding.UTF8.GetBytes(saveData));
    }

    public Task Tick(int elapsedTime)
    {
        if (_timerRunner == null)
        {
            throw new Exception("Begin() has not been called");
        }

        if (State == GameState.Finished)
        {
            return Task.CompletedTask;
        }

        var task = TickAsyncInternal(elapsedTime);
        SendNextTimerRequest();
        return task;
    }

    private async Task TickAsyncInternal(int elapsedTime)
    {
        try
        {
            var scripts = _timerRunner!.TickAndGetScripts(elapsedTime);
            foreach (var timerScript in scripts)
            {
                await RunScriptAsync(timerScript.Value, timerScript.Key);
            }
            await UpdateListsAsync();
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    public async Task SetQuestionResponse(bool response)
    {
        var tcs = _questionTcs;
        _turnSuspendedTcs = new();
        tcs?.TrySetResult(response);
        await _turnSuspendedTcs.Task;
    }

    public Stream? GetResourceStream(string filename)
    {
        return ResourceGetter != null
            ? ResourceGetter.Invoke(filename)
            : _gameData?.GetAdjacentFile(filename);
    }

    public string GameID => Game.Fields[FieldDefinitions.GameID];

    IEnumerable<string> IGame.GetResourceNames()
    {
        return GetResourceNames == null ? [] : GetResourceNames();
    }

    public bool DebugEnabled { get; private set; }
    public event EventHandler<ObjectsUpdatedEventArgs>? ObjectsUpdated;

    public IWalkthroughs Walkthroughs => _walkthroughs!;

    public List<string> DebuggerObjectTypes => [.._debuggerObjectTypes.Keys.Union(_debuggerElementTypes.Keys)];

    public List<string> GetObjects(string type)
    {
        IEnumerable<Element> elements;

        if (_debuggerObjectTypes.TryGetValue(type, out var objectType))
        {
            elements = Elements.ObjectsFiltered(o => o.Type == objectType);
        }
        else
        {
            var filterType = _debuggerElementTypes[type];
            elements = Elements.GetElements(filterType);
        }

        return elements.Select(obj => obj.Name).ToList();
    }

    public DebugData GetDebugData(string _, string el)
    {
        return Elements.Get(el).GetDebugData();
    }

    public Task<bool> AssertAsync(string expr)
    {
        var expression = new Expression<bool>(expr, new ScriptContext(this));
        var c = new Context();
        return expression.ExecuteAsync(c);
    }

    public event EventHandler<ElementFieldUpdatedEventArgs>? ElementFieldUpdated;
    public event EventHandler<ElementRefreshEventArgs>? ElementRefreshed;
    public event EventHandler<ElementFieldUpdatedEventArgs>? ElementMetaFieldUpdated;
    public event EventHandler<LoadStatusEventArgs>? LoadStatus;

    private void InitialiseElementFactories()
    {
        foreach (var t in Classes.GetImplementations(Assembly.GetExecutingAssembly(),
                     typeof(IElementFactory)))
        {
            AddElementFactory((IElementFactory) Activator.CreateInstance(t)!);
        }
    }

    private void AddElementFactory(IElementFactory factory)
    {
        _elementFactories.Add(factory.CreateElementType, factory);
        factory.WorldModel = this;
        factory.ObjectsUpdated += ElementsUpdated;
    }

    private void ElementsUpdated(object? sender, ObjectsUpdatedEventArgs args)
    {
        ObjectsUpdated?.Invoke(this, args);
    }

    private void InitialiseDebuggerObjectTypes()
    {
        _debuggerObjectTypes.Add("Objects", ObjectType.Object);
        _debuggerObjectTypes.Add("Exits", ObjectType.Exit);
        _debuggerObjectTypes.Add("Commands", ObjectType.Command);
        _debuggerObjectTypes.Add("Game", ObjectType.Game);
        _debuggerObjectTypes.Add("Turn Scripts", ObjectType.TurnScript);
        _debuggerElementTypes.Add("Timers", ElementType.Timer);
    }

    public void FinishGame()
    {
        State = GameState.Finished;

        // Cancel all pending TCS so awaiting code can unblock
        _waitTcs?.TrySetCanceled();
        _menuTcs?.TrySetCanceled();
        _questionTcs?.TrySetCanceled();
        _commandInputTcs?.TrySetCanceled();
        _pauseTcs?.TrySetCanceled();
        _turnSuspendedTcs.TrySetResult();
        _onReadyQueue.Clear();

        RequestNextTimerTick?.Invoke(0);
        Finished?.Invoke();
    }

    internal string GetUniqueId(string? prefix = null)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            prefix = "k";
        }

        _nextUniqueId.TryAdd(prefix, 0);
        string newid;
        do
        {
            _nextUniqueId[prefix]++;
            newid = prefix + _nextUniqueId[prefix];
        } while (Elements.ContainsKey(newid));

        return newid;
    }

    public Element Object(string name)
    {
        return Elements.Get(ElementType.Object, name);
    }

    public IElementFactory GetElementFactory(ElementType t)
    {
        return _elementFactories[t];
    }

    public Task PrintTemplateAsync(string t)
    {
        return PrintAsync(Template.GetText(t));
    }

    public void Print(string text, bool linebreak = true)
    {
        if (EditMode) return;
        if (PrintText != null)
        {
            PrintText(linebreak ? "<output>" + text + "</output>" : "<output nobr=\"true\">" + text + "</output>");
        }
        _legacyOutputLogger?.AddText(text, linebreak);
    }

    public async Task PrintAsync(string text, bool linebreak = true)
    {
        if (!EditMode && Version >= WorldModelVersion.v540 && Elements.ContainsKey(ElementType.Function, "OutputText"))
        {
            try
            {
                await RunProcedureAsync("OutputText", new Parameters(new Dictionary<string, string> {{"text", text}}), false);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        else if (!EditMode)
        {
            if (PrintText != null)
            {
                if (linebreak)
                {
                    PrintText("<output>" + text + "</output>");
                }
                else
                {
                    PrintText("<output nobr=\"true\">" + text + "</output>");
                }
            }

            _legacyOutputLogger?.AddText(text, linebreak);
        }
    }

    internal QuestList<Element> GetAllObjects()
    {
        return new QuestList<Element>(Elements.Objects);
    }

    private async Task<QuestList<Element>> GetObjectsInScopeAsync(string scopeFunction)
    {
        if (Elements.ContainsKey(ElementType.Function, scopeFunction))
        {
            return (QuestList<Element>?) await RunProcedureAsync(scopeFunction, null, true) ?? new QuestList<Element>();
        }

        throw new Exception($"No function '{scopeFunction}'");
    }

    public static bool ObjectContains(Element parent, Element searchObj)
    {
        var visited = new HashSet<Element>();
        var current = searchObj.Parent;
        while (current != null)
        {
            if (current == parent) return true;
            if (!visited.Add(current)) return false;
            current = current.Parent;
        }
        return false;
    }


    public bool ObjectExists(string name)
    {
        return Elements.ContainsKey(ElementType.Object, name);
    }

    /// <summary>
    ///     Attempt to resolve an element name from elements which are eligible for expression,
    ///     i.e. objects and timers
    /// </summary>
    /// <param name="name"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool TryResolveExpressionElement(string name, out Element? element)
    {
        element = null;
        if (!Elements.ContainsKey(name))
        {
            return false;
        }

        var result = Elements.Get(name);
        if (result.ElemType != ElementType.Object && result.ElemType != ElementType.Timer)
        {
            return false;
        }

        element = result;
        return true;
    }

    internal void RemoveElement(ElementType type, string name)
    {
        Elements.Remove(type, name);
    }

    internal void SetGameName(string name)
    {
        _playerUi?.UpdateGameName(name);
    }

    public async Task<bool> InitialiseEdit()
    {
        EditMode = true;
        var loader = new GameLoader.GameLoader(this, GameLoader.GameLoader.LoadMode.Edit);
        return await InitialiseInternal(loader);
    }

    private async Task<bool> InitialiseInternal(GameLoader.GameLoader loader)
    {
        if (_gameData == null)
        {
            throw new Exception("Game data not set");
        }

        if (State != GameState.NotStarted)
        {
            throw new Exception("Game already initialised");
        }

        loader.FilenameUpdated += loader_FilenameUpdated;
        loader.LoadStatus += loader_LoadStatus;
        State = GameState.Loading;

        var success = await loader.Load(_gameData, _saveData);

        DebugEnabled = !loader.IsCompiledFile;
        State = success ? GameState.Running : GameState.Finished;
        Errors = loader.Errors;
        _saver = new GameSaver(this);
        if (Version <= WorldModelVersion.v530)
        {
            _legacyOutputLogger = new LegacyOutputLogger(this);
            OutputLogger = _legacyOutputLogger;
        }
        else
        {
            OutputLogger = new OutputLogger(this);
        }

        return success;
    }

    private void loader_LoadStatus(object? sender, GameLoader.GameLoader.LoadStatusEventArgs e)
    {
        LoadStatus?.Invoke(this, new LoadStatusEventArgs(e.Status));
    }

    private void loader_FilenameUpdated(string filename)
    {
        // TODO: This previously did this...
        // // Update base ASLX filename to original filename if we're loading a saved game
        // m_saveFilename = m_filename;
        // m_filename = filename;

        // ... but we now only need it to do this, which could be more explicit:
        _loadedFromSaved = true;
    }

    private async Task UpdateStatusVariablesAsync()
    {
        if (!Elements.ContainsKey(ElementType.Function, "UpdateStatusAttributes"))
        {
            return;
        }

        try
        {
            await RunProcedureAsync("UpdateStatusAttributes");
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private async Task UpdateListsAsync()
    {
        await UpdateObjectsListAsync();
        await UpdateExitsListAsync();
        await UpdateStatusVariablesAsync();
    }

    private async Task UpdateObjectsListAsync()
    {
        await UpdateObjectsListAsync("GetPlacesObjectsList", ListType.ObjectsList);
        await UpdateObjectsListAsync("ScopeInventory", ListType.InventoryList);
    }

    private async Task UpdateObjectsListAsync(string scope, ListType listType)
    {
        if (UpdateList == null)
        {
            return;
        }

        var objects = new List<ListData>();
        foreach (var obj in await GetObjectsInScopeAsync(scope))
        {
            if (Version <= WorldModelVersion.v520 || !Elements.ContainsKey(ElementType.Function, "GetDisplayVerbs"))
            {
                if (scope == "ScopeInventory")
                {
                    objects.Add(new ListData(await GetListDisplayAliasAsync(obj), obj.Fields[FieldDefinitions.InventoryVerbs],
                        obj.Name, await GetDisplayAliasAsync(obj)));
                }
                else
                {
                    objects.Add(new ListData(await GetListDisplayAliasAsync(obj), obj.Fields[FieldDefinitions.DisplayVerbs],
                        obj.Name, await GetDisplayAliasAsync(obj)));
                }
            }
            else
            {
                objects.Add(
                    new ListData(await GetListDisplayAliasAsync(obj), await GetDisplayVerbsAsync(obj), obj.Name, await GetDisplayAliasAsync(obj)));
            }
        }

        // The "Places and Objects" list is generated by function, so we also
        // need to add any exits. (The UI is responsible for filtering out the
        // directional exits so they only display in the compass)
        if (scope == "GetPlacesObjectsList")
        {
            objects.AddRange(await GetExitsListDataAsync());
        }

        UpdateList(listType, objects);
    }

    private async Task UpdateExitsListAsync()
    {
        if (UpdateList != null)
        {
            UpdateList(ListType.ExitsList, await GetExitsListDataAsync());
        }
    }

    private async Task<string> GetListDisplayAliasAsync(Element obj)
    {
        if (Elements.ContainsKey(ElementType.Function, "GetListDisplayAlias"))
        {
            return (string) (await RunProcedureAsync("GetListDisplayAlias", new Parameters("obj", obj), true))!;
        }

        return await GetDisplayAliasAsync(obj);
    }

    private async Task<string> GetDisplayAliasAsync(Element obj)
    {
        if (Elements.ContainsKey(ElementType.Function, "GetDisplayAlias"))
        {
            return (string) (await RunProcedureAsync("GetDisplayAlias", new Parameters("obj", obj), true))!;
        }

        return obj.Name;
    }

    private async Task<IEnumerable<string>> GetDisplayVerbsAsync(Element obj)
    {
        return (QuestList<string>) (await RunProcedureAsync("GetDisplayVerbs", new Parameters("object", obj), true))!;
    }

    private async Task<List<ListData>> GetExitsListDataAsync()
    {
        var exits = new List<ListData>();
        var scopeFunction = "ScopeExits";
        if (Version >= WorldModelVersion.v530 && Elements.ContainsKey(ElementType.Function, "GetExitsList"))
        {
            scopeFunction = "GetExitsList";
        }

        foreach (var exit in await GetObjectsInScopeAsync(scopeFunction))
        {
            IEnumerable<string> verbs;
            if (Version <= WorldModelVersion.v520 || !Elements.ContainsKey(ElementType.Function, "GetDisplayVerbs"))
            {
                verbs = exit.Fields[FieldDefinitions.DisplayVerbs];
            }
            else
            {
                verbs = await GetDisplayVerbsAsync(exit);
            }

            exits.Add(new ListData(await GetListDisplayAliasAsync(exit), verbs, exit.Name, await GetDisplayAliasAsync(exit)));
        }

        return exits;
    }

    public DebugData GetDebugData(string el)
    {
        return Elements.Get(el).GetDebugData();
    }

    public DebugData GetInheritedTypesDebugData(string el)
    {
        return Elements.Get(el).Fields.GetInheritedTypesDebugData();
    }

    public DebugDataItem GetDebugDataItem(string el, string attribute)
    {
        return Elements.Get(el).Fields.GetDebugDataItem(attribute);
    }

    internal void SignalTurnSuspended(bool scroll = true)
    {
        if (scroll && Version >= WorldModelVersion.v540)
        {
            ScrollToEnd();
        }
        _turnSuspendedTcs.TrySetResult();
    }

    internal async Task DoWaitAsync()
    {
        PlayerUi.DoWait();
        _waitTcs = new TaskCompletionSource();
        SignalTurnSuspended();
        await _waitTcs.Task;
    }

    internal async Task DoPauseAsync(int ms)
    {
        PlayerUi.DoPause(ms);
        _pauseTcs = new TaskCompletionSource();
        SignalTurnSuspended();
        await _pauseTcs.Task;
    }

    public Task RunScriptAsync(IScript script)
    {
        return RunScriptAsync(script, (Parameters?) null, false);
    }

    /// <summary>
    ///     Use this version of RunScriptAsync when executing an object action. Set thisElement to the object whose action it is.
    /// </summary>
    public Task RunScriptAsync(IScript script, Element thisElement)
    {
        return RunScriptAsync(script, null, false, thisElement);
    }

    public Task RunScriptAsync(IScript script, Parameters parameters)
    {
        return RunScriptAsync(script, parameters, false);
    }

    public Task RunScriptAsync(IScript script, Parameters parameters, Element thisElement)
    {
        return RunScriptAsync(script, parameters, false, thisElement);
    }

    public Task<object?> RunDelegateScriptAsync(IScript script, Parameters parameters, Element thisElement)
    {
        return RunScriptAsync(script, parameters, true, thisElement);
    }

    private Task<object?> RunScriptAsync(IScript script, Parameters? parameters, bool expectResult,
        Element? thisElement = null)
    {
        var c = new Context();
        parameters ??= new Parameters();
        if (thisElement != null)
        {
            parameters.Add("this", thisElement);
        }

        c.Parameters = parameters;
        return RunScriptAsync(script, c, expectResult);
    }

    internal Task RunScriptAsync(IScript script, Context c)
    {
        return RunScriptAsync(script, c, false);
    }

    private async Task<object?> RunScriptAsync(IScript script, Context c, bool expectResult)
    {
        try
        {
            await script.ExecuteAsync(c);
            if (expectResult && c.ReturnValue is NoReturnValue)
            {
                throw new Exception("Function did not return a value");
            }

            return c.ReturnValue;
        }
        catch (Exception ex)
        {
            await PrintAsync("Error running script: " + Utility.SafeXML(ex.Message));
            LogException(ex);
        }

        return null;
    }

    public Element AddProcedure(string name)
    {
        var proc = GetElementFactory(ElementType.Function).Create(name);
        return proc;
    }

    public Element AddProcedure(string name, IScript script, string[] parameters)
    {
        var proc = AddProcedure(name);
        proc.Fields[FieldDefinitions.Script] = script;
        proc.Fields[FieldDefinitions.ParamNames] = new QuestList<string>(parameters);
        return proc;
    }

    public Element AddDelegate(string name)
    {
        var del = GetElementFactory(ElementType.Delegate).Create(name);
        return del;
    }

    public Task RunProcedureAsync(string name)
    {
        return RunProcedureAsync(name, null, false);
    }

    public async Task<object?> RunProcedureAsync(string name, Parameters? parameters, bool expectResult)
    {
        if (Elements.ContainsKey(ElementType.Function, name))
        {
            var function = Elements.Get(ElementType.Function, name);

            // Only check for too few parameters for games for Quest 5.2 or later, as previous Quest versions
            // would ignore this (but would usually still fail when the function was run, as the required
            // variable wouldn't exist). For Quest 5.3, an additional check if parameters is non-null but empty.

            var parametersInvalid = false;
            if (Version == WorldModelVersion.v520)
            {
                parametersInvalid = parameters == null && function.Fields[FieldDefinitions.ParamNames].Count > 0;
            }
            else if (Version >= WorldModelVersion.v530)
            {
                parametersInvalid = (parameters == null || parameters.Count == 0) &&
                                    function.Fields[FieldDefinitions.ParamNames].Count > 0;
            }

            if (parametersInvalid)
            {
                throw new Exception(string.Format("No parameters passed to {0} function - expected {1} parameters",
                    name,
                    function.Fields[FieldDefinitions.ParamNames].Count));
            }

            return await RunScriptAsync(function.Fields[FieldDefinitions.Script], parameters, expectResult);
        }

        await PrintAsync($"Error - no such procedure '{name}'");
        return null;
    }

    public Element? Procedure(string name)
    {
        return !Elements.ContainsKey(ElementType.Function, name) ? null : Elements.Get(ElementType.Function, name);
    }

    internal Element GetObjectType(string name)
    {
        return Elements.Get(ElementType.ObjectType, name);
    }

    public string Save(SaveMode mode, bool? includeWalkthrough = null, string? html = null)
    {
        if (_saver == null)
        {
            throw new Exception("Game not initialised");
        }

        return _saver.Save(mode, includeWalkthrough, html);
    }

    public static Type? ConvertTypeNameToType(string name)
    {
        if (TypeNamesToTypes.TryGetValue(name, out var type))
        {
            return type;
        }

        if (name == "null")
        {
            return null;
        }

        // TO DO: type name could also be a DelegateImplementation
        //if (value is DelegateImplementation) return ((DelegateImplementation)value).TypeName;

        throw new ArgumentOutOfRangeException($"Unrecognised type name '{name}'");
    }

    public static string ConvertTypeToTypeName(Type type)
    {
        if (TypesToTypeNames.TryGetValue(type, out var name))
        {
            return name;
        }

        foreach (var kvp in TypesToTypeNames.Where(kvp => kvp.Key.IsAssignableFrom(type)))
        {
            return kvp.Value;
        }

        throw new ArgumentOutOfRangeException($"Unrecognised type '{type}'");
    }

    public Stream GetLibraryStream(string filename)
    {
        if (_gameData == null)
        {
            throw new Exception("Game data not set");
        }

        var stream = _gameData.GetAdjacentFile(filename);
        if (stream != null)
        {
            return stream;
        }

        stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QuestViva.Engine.Core." + filename);
        if (stream != null)
        {
            return stream;
        }

        stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("QuestViva.Engine.Core.Languages." + filename);
        if (stream != null)
        {
            return stream;
        }

        throw new Exception("Library file not found: " + filename);
    }

    internal string GetExternalUrl(string file)
    {
        return PlayerUi.GetURL(file);
    }

    public IEnumerable<string> GetAvailableLibraries()
    {
        // TODO
        throw new NotImplementedException();

        // List<string> result = new List<string>();
        // AddFilesInPathToList(result, System.IO.Path.GetDirectoryName(Filename), false);
        // AddFilesInPathToList(result, Environment.CurrentDirectory, false);
        // if (m_libFolder != null) AddFilesInPathToList(result, m_libFolder, false);
        // if (System.Reflection.Assembly.GetEntryAssembly() != null)
        // {
        //     AddFilesInPathToList(result, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().CodeBase), true);
        // }
        // return result;
    }

    private void AddFilesInPathToList(List<string> list, string path, bool recurse, string searchPattern = "*.aslx")
    {
        path = Files.RemoveFileColonPrefix(path);
        var option = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        foreach (var result in Directory.GetFiles(path, searchPattern, option))
        {
            if (result == Filename)
            {
                continue;
            }

            var filename = Path.GetFileName(result);
            if (!list.Contains(filename))
            {
                list.Add(filename);
            }
        }
    }

    public IEnumerable<string> GetAvailableExternalFiles(string searchPatterns)
    {
        var result = new List<string>();
        var patterns = searchPatterns.Split(';');
        foreach (var searchPattern in patterns)
        {
            AddFilesInPathToList(result, Path.GetDirectoryName(Filename)!, false, searchPattern);
        }

        return result;
    }

    internal void NotifyElementFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
    {
        if (!element.Initialised)
        {
            return;
        }

        ElementFieldUpdated?.Invoke(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
    }

    internal void NotifyElementMetaFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
    {
        if (!element.Initialised)
        {
            return;
        }

        ElementMetaFieldUpdated?.Invoke(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
    }

    internal void NotifyElementRefreshed(Element element)
    {
        ElementRefreshed?.Invoke(this, new ElementRefreshEventArgs(element));
    }

    private void ScrollToEnd()
    {
        PlayerUi.RunScript("scrollToEnd", null);
    }

    internal void LogException(Exception ex)
    {
        LogError?.Invoke(ex);
    }

    public static ElementType GetElementTypeForTypeString(string typeString)
    {
        return Element.GetElementTypeForTypeString(typeString);
    }

    public static ObjectType GetObjectTypeForTypeString(string typeString)
    {
        return Element.GetObjectTypeForTypeString(typeString);
    }

    public static string GetTypeStringForElementType(ElementType type)
    {
        return Element.GetTypeStringForElementType(type);
    }

    public static string GetTypeStringForObjectType(ObjectType type)
    {
        return Element.GetTypeStringForObjectType(type);
    }

    public static bool IsDefaultTypeName(string typeName)
    {
        return DefaultTypeNames.ContainsValue(typeName);
    }

    public Element AddNewTemplate(string templateName)
    {
        return Template.AddTemplate(templateName, string.Empty, false);
    }

    public Element? TryGetTemplateElement(string templateName)
    {
        return !Template.TemplateExists(templateName) ? null : Template.GetTemplateElement(templateName);
    }

    [GeneratedRegex(@"\d*$")]
    private static partial Regex s_removeTrailingDigits();

    public string GetUniqueElementName(string elementName)
    {
        // If element name doesn't exist (element might have been Cut in the editor),
        // then just return the original name
        if (!Elements.ContainsKey(elementName))
        {
            return elementName;
        }

        // Otherwise get a uniquely numbered element
        var root = s_removeTrailingDigits().Replace(elementName, "");
        var elementAlreadyExists = true;
        var number = 0;
        string result = null!;

        while (elementAlreadyExists)
        {
            number++;
            result = root + number;
            elementAlreadyExists = Elements.ContainsKey(result);
        }

        return result!;
    }

    internal void AddAttributeName(string name)
    {
        if (!_attributeNames.Contains(name))
        {
            _attributeNames.Add(name);
        }
    }

    private void SendNextTimerRequest()
    {
        if (_timerRunner == null)
        {
            throw new Exception("Begin() has not been called");
        }

        if (State == GameState.Finished)
        {
            return;
        }

        var next = _timerRunner.GetTimeUntilNextTimerRuns();
        RequestNextTimerTick?.Invoke(next);
        Debug.Print("Request next timer in {0}", next);
    }

    private async Task TryFinishTurnAsync()
    {
        if (!Elements.ContainsKey(ElementType.Function, "FinishTurn")) return;
        try
        {
            await RunProcedureAsync("FinishTurn");
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private Task TryRunOnFinallyScriptsAsync()
    {
        return Task.CompletedTask;
    }

    public bool CreatePackage(string filename, bool includeWalkthrough, out string error,
        IEnumerable<PackageIncludeFile> includeFiles, Stream outputStream)
    {
        var packager = new Packager(this);
        return packager.CreatePackage(filename, includeWalkthrough, out error, includeFiles, outputStream);
    }

    public IEnumerable<string> GetBuiltInFunctionNames()
    {
        if (FunctionNames != null)
        {
            return FunctionNames.AsReadOnly();
        }

        var methods = typeof(ExpressionOwner).GetMethods();
        var stringMethods = typeof(StringFunctions).GetMethods();
        var dateTimeMethods = typeof(DateTimeFunctions).GetMethods();

        var allMethods = methods.Union(stringMethods).Union(dateTimeMethods);

        FunctionNames = new List<string>(allMethods.Select(m => m.Name));

        return FunctionNames.AsReadOnly();
    }

    internal void UpdateElementSortOrder(Element movedElement)
    {
        // There's no need to worry about element sort order when playing the game, unless this is an element that can be seen by the
        // player
        if (!EditMode)
        {
            var doUpdate = movedElement.ElemType == ElementType.Object &&
                           (movedElement.Type == ObjectType.Exit || movedElement.Type == ObjectType.Object);
            if (!doUpdate)
            {
                return;
            }
        }

        // This function is called when an element is moved to a new parent.
        // When this happens, its SortIndex MetaField must be updated so that it
        // is at the end of the list of children.

        var maxIndex = Elements.GetDirectChildren(movedElement.Parent)
            .Select(sibling => sibling.MetaFields[MetaFieldDefinitions.SortIndex]).Prepend(-1).Max();

        movedElement.MetaFields[MetaFieldDefinitions.SortIndex] = maxIndex + 1;
    }

    internal Task AddOnReady(IScript callback, Context c)
    {
        if (_pendingCallbackCount > 0)
        {
            _onReadyQueue.Add((callback, c));
            return Task.CompletedTask;
        }
        return RunScriptAsync(callback, c);
    }

    internal void BeginPendingCallback() => _pendingCallbackCount++;

    internal async Task EndPendingCallbackAsync()
    {
        _pendingCallbackCount--;
        while (_pendingCallbackCount == 0 && _onReadyQueue.Count > 0 && State != GameState.Finished)
        {
            var (script, context) = _onReadyQueue[0];
            _onReadyQueue.RemoveAt(0);
            await RunScriptAsync(script, context);
            // RunScriptAsync may have called BeginPendingCallback (e.g. via show menu inside on ready);
            // if so, stop flushing — the new callback's EndPendingCallbackAsync will continue.
        }
    }

    public string? GetResourceData(string filename)
    {
        var stream = GetResourceStream(filename);
        if (stream == null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static string[] GetEmbeddedResources()
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceNames();
    }

    public static Stream? GetEmbeddedResourceStream(string name)
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
    }

    public class PackageIncludeFile
    {
        public required string Filename { get; set; }
        public required Stream Content { get; set; }
    }
}