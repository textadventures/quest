using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using QuestViva.Common;
using QuestViva.Engine.Functions;
using QuestViva.Engine.GameLoader;
using QuestViva.Engine.Scripts;
using QuestViva.Utility;

namespace QuestViva.Engine;

public partial class WorldModel : IGame, IGameDebug
{
    private readonly IConfig _config;
    private readonly Dictionary<string, int> _nextUniqueId = new();
    private readonly GameData? _gameData;
    private readonly Stream? _saveData;
    private readonly Dictionary<string, ObjectType> _debuggerObjectTypes = new();
    private readonly Dictionary<string, ElementType> _debuggerElementTypes = new();
    private readonly Dictionary<ElementType, IElementFactory> _elementFactories = new();
    private readonly List<string> _attributeNames = [];
    private readonly CallbackManager _callbacks = new();
    private readonly object _threadLock = new();
    private readonly object _commandOverrideLock = new();

    private GameSaver? _saver;
        
    private Walkthroughs? _walkthroughs;
    private TimerRunner? _timerRunner;
        
    private bool _commandOverride = false;
    private string? _commandOverrideInput;

    private LegacyOutputLogger? _legacyOutputLogger;

    private bool _loadedFromSaved = false;
    private ThreadState _threadState = ThreadState.Ready;
    
    public Func<string, Stream>? ResourceGetter { get; internal set; }
    public Func<IEnumerable<string>>? GetResourceNames { get; internal set; }
    public bool DebugEnabled { get; private set; }

    internal static Dictionary<ObjectType, string> DefaultTypeNames { get; } = new()
    {
        {ObjectType.Object, "defaultobject"},
        {ObjectType.Exit, "defaultexit"},
        {ObjectType.Command, "defaultcommand"},
        {ObjectType.Game, "defaultgame"},
        {ObjectType.TurnScript, "defaultturnscript"}
    };
        
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
        
    static WorldModel()
    {
        foreach (var kvp in TypeNamesToTypes)
        {
            TypesToTypeNames.Add(kvp.Value, kvp.Key);
        }
    }

    public event EventHandler<ElementFieldUpdatedEventArgs>? ElementFieldUpdated;
    public event EventHandler<ElementRefreshEventArgs>? ElementRefreshed;
    public event EventHandler<ElementFieldUpdatedEventArgs>? ElementMetaFieldUpdated;
    public event EventHandler<LoadStatusEventArgs>? LoadStatus;
    public event Action<int>? RequestNextTimerTick;
    public event PrintTextHandler? PrintText;
    public event UpdateListHandler? UpdateList;
    public event FinishedHandler? Finished;
    public event EventHandler<ObjectsUpdatedEventArgs>? ObjectsUpdated;
    public event ErrorHandler? LogError;

    internal WorldModel(IConfig config)
        // ReSharper disable once IntroduceOptionalParameters.Global
        : this(config, null, null)
    {
    }

    public WorldModel(IConfig config, GameData? gameData, Stream? saveData)
    {
        ExpressionOwner = new ExpressionOwner(this);
        Template = new Template(this);
        InitialiseElementFactories();
        ObjectFactory = (ObjectFactory)_elementFactories[ElementType.Object];

        InitialiseDebuggerObjectTypes();
        _config = config;
        _gameData = gameData;
        _saveData = saveData;
        Elements = new Elements();
        UndoLogger = new UndoLogger(this);
        Game = ObjectFactory.CreateObject("game", ObjectType.Game);
    }

    public bool UseNCalc => _config.UseNCalc;

    private void InitialiseElementFactories()
    {
        foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                     typeof(IElementFactory)))
        {
            AddElementFactory((IElementFactory)Activator.CreateInstance(t)!);
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

        // Pulse all locks in case we're in the middle of waiting for player input for GetInput() etc.

        lock (_commandOverrideLock)
        {
            Monitor.PulseAll(_commandOverrideLock);
        }

        lock (_threadLock)
        {
            Monitor.PulseAll(_threadLock);
        }

        lock (_waitForResponseLock)
        {
            Monitor.PulseAll(_waitForResponseLock);
        }

        RequestNextTimerTick?.Invoke(0);
        Finished?.Invoke();
    }

    internal string GetUniqueId(string? prefix = null)
    {
        if (string.IsNullOrEmpty(prefix)) prefix = "k";
        _nextUniqueId.TryAdd(prefix, 0);
        string newid;
        do
        {
            _nextUniqueId[prefix]++;
            newid = prefix + _nextUniqueId[prefix];
        } while (Elements.ContainsKey(newid));
            
        return newid;
    }

    public Element Game { get; }

    public Element Object(string name)
    {
        return Elements.Get(ElementType.Object, name);
    }

    public ObjectFactory ObjectFactory { get; }

    public IElementFactory GetElementFactory(ElementType t)
    {
        return _elementFactories[t];
    }

    public void PrintTemplate(string t)
    {
        Print(Template.GetText(t));
    }

    public void Print(string text, bool linebreak = true)
    {
        if (Version >= WorldModelVersion.v540 && Elements.ContainsKey(ElementType.Function, "OutputText"))
        {
            try
            {
                RunProcedure("OutputText", new Parameters(new Dictionary<string, string> {{"text", text}}), false);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        else
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

    private QuestList<Element> GetObjectsInScope(string scopeFunction)
    {
        if (Elements.ContainsKey(ElementType.Function, scopeFunction))
        {
            return (QuestList<Element>)RunProcedure(scopeFunction, true)!;
        }
        throw new Exception($"No function '{scopeFunction}'");
    }

    public static bool ObjectContains(Element parent, Element searchObj)
    {
        if (searchObj.Parent == null) return false;
        return searchObj.Parent == parent || ObjectContains(parent, searchObj.Parent);
    }

    private readonly object _waitForResponseLock = new();
    private string? _menuResponse = null;
    private IDictionary<string, string>? _menuOptions = null;

    internal string DisplayMenu(string caption, IDictionary<string, string> options, bool allowCancel, bool async)
    {
        Print(caption);

        var menuData = new MenuData(caption, options, allowCancel);
        _menuOptions = options;

        PlayerUi.ShowMenu(menuData);

        if (async)
        {
            return string.Empty;
        }

        _callbacks.Pop(CallbackManager.CallbackTypes.Menu);
        _menuOptions = null;

        ChangeThreadState(ThreadState.Waiting);

        lock (_waitForResponseLock)
        {
            Monitor.Wait(_waitForResponseLock);
        }

        ChangeThreadState(ThreadState.Working);

        return _menuResponse!;
    }

    internal string DisplayMenu(string caption, IList<string> options, bool allowCancel, bool async)
    {
        var optionsDictionary = options.ToDictionary(option => option);
        return DisplayMenu(caption, optionsDictionary, allowCancel, async);
    }

    public void SetMenuResponse(string? response)
    {
        var menuCallback = _callbacks.Pop(CallbackManager.CallbackTypes.Menu);
        if (menuCallback != null)
        {
            if (response != null) Print(" - " + _menuOptions![response]);
            menuCallback.Context.Parameters["result"] = response;
            _menuOptions = null;
            DoInNewThreadAndWait(() =>
            {
                RunCallbackAndFinishTurn(menuCallback);
            });
        }
        else
        {
            DoInNewThreadAndWait(() =>
            {
                _menuResponse = response;

                lock (_waitForResponseLock)
                {
                    Monitor.Pulse(_waitForResponseLock);
                }
            });
        }
    }

    internal void DisplayMenuAsync(string caption, IList<string> options, bool allowCancel, IScript callback, Context c)
    {
        _callbacks.Push(CallbackManager.CallbackTypes.Menu, new Callback(callback, c), "Only one menu can be shown at a time.");
        DisplayMenu(caption, options, allowCancel, true);
    }

    internal void DisplayMenuAsync(string caption, IDictionary<string, string> options, bool allowCancel, IScript callback, Context c)
    {
        _callbacks.Push(CallbackManager.CallbackTypes.Menu, new Callback(callback, c), "Only one menu can be shown at a time.");
        DisplayMenu(caption, options, allowCancel, true);
    }

    public IEnumerable<Element> Objects => Elements.Objects;

    public bool ObjectExists(string name)
    {
        return Elements.ContainsKey(ElementType.Object, name);
    }

    /// <summary>
    /// Attempt to resolve an element name from elements which are eligible for expression,
    /// i.e. objects and timers
    /// </summary>
    /// <param name="name"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool TryResolveExpressionElement(string name, out Element? element)
    {
        element = null;
        if (!Elements.ContainsKey(name)) return false;

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

    public async Task<bool> Initialise(IPlayer player, bool? isCompiled = null)
    {
        EditMode = false;
        PlayerUi = player;
        var loader = new QuestViva.Engine.GameLoader.GameLoader(this, QuestViva.Engine.GameLoader.GameLoader.LoadMode.Play, isCompiled);
        var result = await InitialiseInternal(loader);
        if (result)
        {
            _walkthroughs = new Walkthroughs(this);
        }
        return result;
    }

    public async Task<bool> InitialiseEdit()
    {
        EditMode = true;
        var loader = new QuestViva.Engine.GameLoader.GameLoader(this, QuestViva.Engine.GameLoader.GameLoader.LoadMode.Edit);
        return await InitialiseInternal(loader);
    }

    private async Task<bool> InitialiseInternal(QuestViva.Engine.GameLoader.GameLoader loader)
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

    private void loader_LoadStatus(object? sender, QuestViva.Engine.GameLoader.GameLoader.LoadStatusEventArgs e)
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

    public void Begin()
    {
        DoInNewThreadAndWait(() =>
        {
            try
            {
                _timerRunner = new TimerRunner(this, !_loadedFromSaved);
                if (Version <= WorldModelVersion.v540)
                {
                    PlayerUi.Show("Panes");
                    PlayerUi.Show("Location");
                    PlayerUi.Show("Command");
                }
                if (Elements.ContainsKey(ElementType.Function, "InitInterface")) RunProcedure("InitInterface");
                if (!_loadedFromSaved)
                {
                    if (Elements.ContainsKey(ElementType.Function, "StartGame")) RunProcedure("StartGame");
                }
                TryRunOnFinallyScripts();
                UpdateLists();
                if (_loadedFromSaved)
                {
                    var output = Elements.GetSingle(ElementType.Output);
                    if (output == null)
                    {
                        Print("Loaded saved game");
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

            ChangeThreadState(ThreadState.Ready);
        });

        SendNextTimerRequest();
    }

    public List<string> Errors { get; private set; } = [];

    public IWalkthroughs Walkthroughs => _walkthroughs!;

    public List<string> DebuggerObjectTypes => [.._debuggerObjectTypes.Keys.Union(_debuggerElementTypes.Keys)];

    public void SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata)
    {
        if (_timerRunner == null) throw new Exception("Begin() has not been called");
        
        if (elapsedTime > 0)
        {
            _timerRunner.IncrementTime(elapsedTime);
        }

        DoInNewThreadAndWait(() =>
        {
            if (!_commandOverride)
            {
                if (Version < WorldModelVersion.v520)
                {
                    Print("");
                    Print("> " + Utility.SafeXML(command));
                }

                try
                {
                    RunProcedure("HandleCommand", new Parameters(new Dictionary<string, object>{
                        {"command", command},
                        {"metadata", new QuestDictionary<string>(metadata)}
                    }), false);
                    if (Version < WorldModelVersion.v580)
                    {
                        TryFinishTurn();
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }

                if (State != GameState.Finished)
                {
                    UpdateLists();
                }

                ChangeThreadState(ThreadState.Ready);
            }
            else
            {
                var getInputCallback = _callbacks.Pop(CallbackManager.CallbackTypes.GetInput);
                if (getInputCallback != null)
                {
                    _commandOverride = false;
                    getInputCallback.Context.Parameters["result"] = command;
                    RunCallbackAndFinishTurn(getInputCallback);
                }
                else
                {
                    _commandOverrideInput = command;

                    lock (_commandOverrideLock)
                    {
                        Monitor.Pulse(_commandOverrideLock);
                    }
                }
            }
        });

        if (elapsedTime > 0)
        {
            // we increase the timer counter above, before the command has been run,
            // so we pass in 0 here
            Tick(0);
        }
        else
        {
            SendNextTimerRequest();
        }
    }

    public void SendCommand(string command)
    {
        SendCommand(command, 0, ReadOnlyDictionary<string, string>.Empty);
    }

    public void SendEvent(string eventName, string param)
    {
        Elements.TryGetValue(ElementType.Function, eventName, out var handler);

        if (handler == null)
        {
            Print($"Error - no handler for event '{eventName}'");
            return;
        }

        var parameters = new Parameters {{(string) handler.Fields[FieldDefinitions.ParamNames][0], param}};

        RunProcedure(eventName, parameters, false);
        
        switch (Version)
        {
            case < WorldModelVersion.v540:
                return;
            case < WorldModelVersion.v580:
                TryFinishTurn();
                break;
        }

        if (State != GameState.Finished)
        {
            UpdateLists();
        }
        SendNextTimerRequest();
    }

    public string Filename => _gameData?.Filename ?? string.Empty;

    public void Finish()
    {
        FinishGame();
    }

    internal Template Template { get; }

    public UndoLogger UndoLogger { get; }

    private void UpdateStatusVariables()
    {
        if (!Elements.ContainsKey(ElementType.Function, "UpdateStatusAttributes"))
        {
            return;
        }

        try
        {
            RunProcedure("UpdateStatusAttributes");
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private void UpdateLists()
    {
        UpdateObjectsList();
        UpdateExitsList();
        UpdateStatusVariables();
    }

    private void UpdateObjectsList()
    {
        UpdateObjectsList("GetPlacesObjectsList", ListType.ObjectsList);
        UpdateObjectsList("ScopeInventory", ListType.InventoryList);
    }

    private void UpdateObjectsList(string scope, ListType listType)
    {
        if (UpdateList == null)
        {
            return;
        }

        var objects = new List<ListData>();
        foreach (var obj in GetObjectsInScope(scope))
        {
            if (Version <= WorldModelVersion.v520 || !Elements.ContainsKey(ElementType.Function, "GetDisplayVerbs"))
            {
                if (scope == "ScopeInventory")
                {
                    objects.Add(new ListData(GetListDisplayAlias(obj), obj.Fields[FieldDefinitions.InventoryVerbs], obj.Name, GetDisplayAlias(obj)));
                }
                else
                {
                    objects.Add(new ListData(GetListDisplayAlias(obj), obj.Fields[FieldDefinitions.DisplayVerbs], obj.Name, GetDisplayAlias(obj)));
                }
            }
            else
            {
                objects.Add(new ListData(GetListDisplayAlias(obj), GetDisplayVerbs(obj), obj.Name, GetDisplayAlias(obj)));
            }
        }
        // The "Places and Objects" list is generated by function, so we also
        // need to add any exits. (The UI is responsible for filtering out the
        // directional exits so they only display in the compass)
        if (scope == "GetPlacesObjectsList") objects.AddRange(GetExitsListData());
        UpdateList(listType, objects);
    }

    private void UpdateExitsList()
    {
        UpdateList?.Invoke(ListType.ExitsList, GetExitsListData());
    }

    private string GetListDisplayAlias(Element obj)
    {
        if (Elements.ContainsKey(ElementType.Function, "GetListDisplayAlias"))
        {
            return (string)RunProcedure("GetListDisplayAlias", new Parameters("obj", obj), true)!;
        }
        return GetDisplayAlias(obj);
    }

    private string GetDisplayAlias(Element obj)
    {
        if (Elements.ContainsKey(ElementType.Function, "GetDisplayAlias"))
        {
            return (string)RunProcedure("GetDisplayAlias", new Parameters("obj", obj), true)!;
        }
        return obj.Name;
    }

    private IEnumerable<string> GetDisplayVerbs(Element obj)
    {
        return (QuestList<string>)RunProcedure("GetDisplayVerbs", new Parameters("object", obj), true)!;
    }

    private List<ListData> GetExitsListData()
    {
        var exits = new List<ListData>();
        var scopeFunction = "ScopeExits";
        if (Version >= WorldModelVersion.v530 && Elements.ContainsKey(ElementType.Function, "GetExitsList"))
        {
            scopeFunction = "GetExitsList";
        }
        foreach (var exit in GetObjectsInScope(scopeFunction))
        {
            IEnumerable<string> verbs;
            if (Version <= WorldModelVersion.v520 || !Elements.ContainsKey(ElementType.Function, "GetDisplayVerbs"))
            {
                verbs = exit.Fields[FieldDefinitions.DisplayVerbs];
            }
            else
            {
                verbs = GetDisplayVerbs(exit);
            }
            exits.Add(new ListData(GetListDisplayAlias(exit), verbs, exit.Name, GetDisplayAlias(exit)));
        }
        return exits;
    }

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

    public void StartWait()
    {
        _callbacks.Pop(CallbackManager.CallbackTypes.Wait);
        PlayerUi.DoWait();

        ChangeThreadState(ThreadState.Waiting);

        lock (_waitForResponseLock)
        {
            Monitor.Wait(_waitForResponseLock);
        }

        ChangeThreadState(ThreadState.Working);
    }

    public void StartWaitAsync(IScript callback, Context c)
    {
        _callbacks.Push(CallbackManager.CallbackTypes.Wait, new Callback(callback, c), "Only one wait can be in progress at a time.");
        PlayerUi.DoWait();
    }

    public void FinishWait()
    {
        Callback waitCallback = _callbacks.Pop(CallbackManager.CallbackTypes.Wait);
        if (waitCallback != null)
        {
            DoInNewThreadAndWait(() =>
            {
                RunCallbackAndFinishTurn(waitCallback);
            });
        }
        else
        {
            if (State == GameState.Finished) return;
            DoInNewThreadAndWait(() =>
            {
                lock (_waitForResponseLock)
                {
                    Monitor.Pulse(_waitForResponseLock);
                }
            });
        }
    }

    public void StartPause(int ms)
    {
        PlayerUi.DoPause(ms);

        ChangeThreadState(ThreadState.Waiting);

        lock (_waitForResponseLock)
        {
            Monitor.Wait(_waitForResponseLock);
        }

        ChangeThreadState(ThreadState.Working);
    }

    public void FinishPause()
    {
        DoInNewThreadAndWait(() =>
        {
            lock (_waitForResponseLock)
            {
                Monitor.Pulse(_waitForResponseLock);
            }
        });
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
                if (jsRef.Fields[FieldDefinitions.Src].Equals("frame.js", StringComparison.CurrentCultureIgnoreCase)) continue;
            }

            result.Add(jsRef.Fields[FieldDefinitions.Src]);
        }
        return result;
    }

    // TO DO: This could actually be removed now, as we can dynamically load stylesheets. Core.aslx InitInterface
    // should simply be able to use the SetWebFontName function to load game.defaultwebfont
    public IEnumerable<string> GetExternalStylesheets()
    {
        if (Version < WorldModelVersion.v530) return [];

        var webFontsInUse = new List<string>();
        var defaultWebFont = Game.Fields[FieldDefinitions.DefaultWebFont];
        if (!string.IsNullOrEmpty(defaultWebFont))
        {
            webFontsInUse.Add(defaultWebFont);
        }
            
        var result = webFontsInUse.Select(f => "https://fonts.googleapis.com/css?family=" + f.Replace(' ', '+'));
            
        return result;
    }

    public void RunScript(IScript script)
    {
        RunScript(script, (Parameters?)null, false);
    }

    /// <summary>
    /// Use this version of RunScript when executing an object action. Set thisElement to the object whose action it is.
    /// </summary>
    /// <param name="script"></param>
    /// <param name="thisElement"></param>
    public void RunScript(IScript script, Element thisElement)
    {
        RunScript(script, null, false, thisElement);
    }

    public void RunScript(IScript script, Parameters parameters)
    {
        RunScript(script, parameters, false);
    }

    public void RunScript(IScript script, Parameters parameters, Element thisElement)
    {
        RunScript(script, parameters, false, thisElement);
    }

    public object RunDelegateScript(IScript script, Parameters parameters, Element thisElement)
    {
        return RunScript(script, parameters, true, thisElement)!;
    }

    private object? RunScript(IScript script, Parameters? parameters, bool expectResult)
    {
        return RunScript(script, parameters, expectResult, null);
    }

    private object? RunScript(IScript script, Parameters? parameters, bool expectResult, Element? thisElement)
    {
        var c = new Context();
        parameters ??= new Parameters();
        if (thisElement != null) parameters.Add("this", thisElement);
        c.Parameters = parameters;

        return RunScript(script, c, expectResult);
    }

    private void RunScript(IScript script, Context c)
    {
        RunScript(script, c, false);
    }

    private object? RunScript(IScript script, Context c, bool expectResult)
    {
        try
        {
            script.Execute(c);
            if (expectResult && c.ReturnValue is NoReturnValue) throw new Exception("Function did not return a value");
            return c.ReturnValue;
        }
        catch (Exception ex)
        {
            // TODO: Add some way of nicely showing script errors to the user (should be higher up the callstack)
            Print("Error running script: " + Utility.SafeXML(ex.Message));
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
        Element proc = AddProcedure(name);
        proc.Fields[FieldDefinitions.Script] = script;
        proc.Fields[FieldDefinitions.ParamNames] = new QuestList<string>(parameters);
        return proc;
    }

    public Element AddDelegate(string name)
    {
        var del = GetElementFactory(ElementType.Delegate).Create(name);
        return del;
    }

    public void RunProcedure(string name)
    {
        RunProcedure(name, false);
    }

    private object? RunProcedure(string name, bool expectResult)
    {
        return RunProcedure(name, null, expectResult);
    }

    public object? RunProcedure(string name, Parameters? parameters, bool expectResult)
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
                parametersInvalid = (parameters == null || parameters.Count == 0) && function.Fields[FieldDefinitions.ParamNames].Count > 0;
            }

            if (parametersInvalid)
            {
                throw new Exception(string.Format("No parameters passed to {0} function - expected {1} parameters",
                    name,
                    function.Fields[FieldDefinitions.ParamNames].Count));
            }

            return RunScript(function.Fields[FieldDefinitions.Script], parameters, expectResult);
        }

        Print($"Error - no such procedure '{name}'");
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

    public GameState State { get; private set; } = GameState.NotStarted;

    public Elements Elements { get; }

    public byte[] Save(string html)
    {
        var saveData = Save(SaveMode.SavedGame, html: html);
        return System.Text.Encoding.UTF8.GetBytes(saveData);
    }

    public string Save(SaveMode mode, bool? includeWalkthrough = null, string? html = null)
    {
        if (_saver == null) throw new Exception("Game not initialised");
        
        return _saver.Save(mode, includeWalkthrough, html);
    }

    public static Type? ConvertTypeNameToType(string name)
    {
        if (TypeNamesToTypes.TryGetValue(name, out var type))
        {
            return type;
        }

        if (name == "null") return null;

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

        throw new ArgumentOutOfRangeException($"Unrecognised type '{type.ToString()}'");
    }

    public Stream GetLibraryStream(string filename)
    {
        if (_gameData == null) throw new Exception("Game data not set");
        
        var stream = _gameData.GetAdjacentFile(filename);
        if (stream != null)
        {
            return stream;
        }

        stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("QuestViva.Engine.Core." + filename);
        if (stream != null)
        {
            return stream;
        }

        stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("QuestViva.Engine.Core.Languages." + filename);
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
        var option = recurse ? SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
        foreach (var result in Directory.GetFiles(path, searchPattern, option))
        {
            if (result == Filename) continue;
            var filename = Path.GetFileName(result);
            if (!list.Contains(filename)) list.Add(filename);
        }
    }

    public IEnumerable<string> GetAvailableExternalFiles(string searchPatterns)
    {
        var result = new List<string>();
        var patterns = searchPatterns.Split(';');
        foreach (string searchPattern in patterns)
        {
            AddFilesInPathToList(result, Path.GetDirectoryName(Filename)!, false, searchPattern);
        }
        return result;
    }

    internal void NotifyElementFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
    {
        if (!element.Initialised) return;
        ElementFieldUpdated?.Invoke(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
    }

    internal void NotifyElementMetaFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
    {
        if (!element.Initialised) return;
        ElementMetaFieldUpdated?.Invoke(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
    }

    internal void NotifyElementRefreshed(Element element)
    {
        ElementRefreshed?.Invoke(this, new ElementRefreshEventArgs(element));
    }

    public bool EditMode { get; private set; } = false;

    internal ExpressionOwner ExpressionOwner { get; }

    private void ScrollToEnd()
    {
        PlayerUi.RunScript("scrollToEnd", null);
    }

    private void ChangeThreadState(ThreadState newState, bool scroll = true)
    {
        if (scroll && Version >= WorldModelVersion.v540 && (newState == ThreadState.Ready || newState == ThreadState.Waiting))
        {
            ScrollToEnd();
        }

        if (newState == ThreadState.Waiting && State == GameState.Finished) throw new Exception("Game is finished");
        _threadState = newState;
        lock (_threadLock)
        {
            Monitor.PulseAll(_threadLock);
        }
    }

    private void WaitUntilFinishedWorking()
    {
        lock (_threadLock)
        {
            while (_threadState == ThreadState.Working)
            {
                Monitor.Wait(_threadLock);
            }
        }
    }

    private void DoInNewThreadAndWait(Action routine)
    {
        // Action wrappedRoutine = () =>
        // {
        //     try
        //     {
        //         routine();
        //     }
        //     catch { }
        // };

        ChangeThreadState(ThreadState.Working);
        // Thread newThread = new Thread(new ThreadStart(wrappedRoutine));
        var newThread = new Thread(new ThreadStart(routine));
        newThread.Start();
        WaitUntilFinishedWorking();
    }

    private void LogException(Exception ex)
    {
        LogError?.Invoke(ex);
    }

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
        if (!_attributeNames.Contains(name)) _attributeNames.Add(name);
    }

    public IEnumerable<string> GetAllAttributeNames => _attributeNames.AsReadOnly();

    internal string GetNextCommandInput(bool async)
    {
        _commandOverride = true;

        if (async)
        {
            return string.Empty;
        }

        _callbacks.Pop(CallbackManager.CallbackTypes.GetInput);

        ChangeThreadState(ThreadState.Waiting);

        lock (_commandOverrideLock)
        {
            Monitor.Wait(_commandOverrideLock);
        }

        ChangeThreadState(ThreadState.Working);

        _commandOverride = false;
        return _commandOverrideInput!;
    }

    internal void GetNextCommandInputAsync(IScript callback, Context c)
    {
        _callbacks.Push(CallbackManager.CallbackTypes.GetInput, new Callback(callback, c), "Only one 'get input' can be in progress at a time");
        GetNextCommandInput(true);
    }

    public void Tick(int elapsedTime)
    {
        if (_timerRunner == null) throw new Exception("Begin() has not been called");
        
        if (State == GameState.Finished) return;
        DoInNewThreadAndWait(() =>
        {
            try
            {
                var scripts = _timerRunner.TickAndGetScripts(elapsedTime);

                foreach (var timerScript in scripts)
                {
                    RunScript(timerScript.Value, timerScript.Key);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            UpdateLists();

            ChangeThreadState(ThreadState.Ready, false);
        });

        SendNextTimerRequest();
    }

    private void SendNextTimerRequest()
    {
        if (_timerRunner == null) throw new Exception("Begin() has not been called");
        
        if (State == GameState.Finished) return;
        var next = _timerRunner.GetTimeUntilNextTimerRuns();
        RequestNextTimerTick?.Invoke(next);
        System.Diagnostics.Debug.Print("Request next timer in {0}", next);
    }

    private bool _questionResponse;
    private IPlayer? _playerUi = null;

    internal bool ShowQuestion(string caption)
    {
        _callbacks.Pop(CallbackManager.CallbackTypes.Question);
        PlayerUi.ShowQuestion(caption);

        ChangeThreadState(ThreadState.Waiting);

        lock (_waitForResponseLock)
        {
            Monitor.Wait(_waitForResponseLock);
        }

        ChangeThreadState(ThreadState.Working);

        return _questionResponse;
    }

    internal void ShowQuestionAsync(string caption, IScript callback, Context c)
    {
        _callbacks.Push(CallbackManager.CallbackTypes.Question, new Callback(callback, c), "Only one question can be asked at a time.");
        PlayerUi.ShowQuestion(caption);
    }

    public void SetQuestionResponse(bool response)
    {
        var questionCallback = _callbacks.Pop(CallbackManager.CallbackTypes.Question);
        if (questionCallback != null)
        {
            questionCallback.Context.Parameters["result"] = response;
            DoInNewThreadAndWait(() =>
            {
                RunCallbackAndFinishTurn(questionCallback);
            });
        }
        else
        {
            DoInNewThreadAndWait(() =>
            {
                _questionResponse = response;

                lock (_waitForResponseLock)
                {
                    Monitor.Pulse(_waitForResponseLock);
                }
            });
        }
    }

    private void RunCallbackAndFinishTurn(Callback callback)
    {
        RunScript(callback.Script, callback.Context);
        TryFinishTurn();
        if (State != GameState.Finished)
        {
            UpdateLists();
        }
        ChangeThreadState(ThreadState.Ready);
        SendNextTimerRequest();
    }

    private void TryFinishTurn()
    {
        TryRunOnFinallyScripts();
        if (_callbacks.AnyOutstanding())
        {
            return;
        }

        if (!Elements.ContainsKey(ElementType.Function, "FinishTurn"))
        {
            return;
        }

        try
        {
            RunProcedure("FinishTurn");
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private void TryRunOnFinallyScripts()
    {
        if (_callbacks.AnyOutstanding()) return;
        var onReadyScripts = _callbacks.FlushOnReadyCallbacks();
        foreach (var callback in onReadyScripts)
        {
            RunScript(callback.Script, callback.Context);
        }
    }

    public class PackageIncludeFile
    {
        public required string Filename { get; set; }
        public required Stream Content { get; set; }
    }

    public bool CreatePackage(string filename, bool includeWalkthrough, out string error, IEnumerable<PackageIncludeFile> includeFiles, Stream outputStream)
    {
        var packager = new Packager(this);
        return packager.CreatePackage(filename, includeWalkthrough, out error, includeFiles, outputStream);
    }

    private static List<string>? FunctionNames = null;

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

    public void PlaySound(string filename, bool sync, bool looped)
    {
        PlayerUi.PlaySound(filename, sync, looped);
        if (!sync)
        {
            return;
        }

        ChangeThreadState(ThreadState.Waiting);

        lock (_waitForResponseLock)
        {
            Monitor.Wait(_waitForResponseLock);
        }
    }

    internal void UpdateElementSortOrder(Element movedElement)
    {
        // There's no need to worry about element sort order when playing the game, unless this is an element that can be seen by the
        // player
        if (!EditMode)
        {
            var doUpdate = movedElement.ElemType == ElementType.Object &&
                           (movedElement.Type == ObjectType.Exit || movedElement.Type == ObjectType.Object);
            if (!doUpdate) return;
        }

        // This function is called when an element is moved to a new parent.
        // When this happens, its SortIndex MetaField must be updated so that it
        // is at the end of the list of children.

        var maxIndex = Elements.GetDirectChildren(movedElement.Parent)
            .Select(sibling => sibling.MetaFields[MetaFieldDefinitions.SortIndex]).Prepend(-1).Max();

        movedElement.MetaFields[MetaFieldDefinitions.SortIndex] = maxIndex + 1;
    }

    public bool Assert(string expr)
    {
        var expression = new Expression<bool>(expr, new ScriptContext(this));
        var c = new Context();
        return expression.Execute(c);
    }

    internal void AddOnReady(IScript callback, Context c)
    {
        if (!_callbacks.AnyOutstanding())
        {
            RunScript(callback, c);
        }
        else
        {
            _callbacks.AddOnReadyCallback(new Callback(callback, c));
        }
    }

    public Stream? GetResource(string filename)
    {
        return ResourceGetter != null
            ? ResourceGetter.Invoke(filename)
            : _gameData?.GetAdjacentFile(filename);
    }

    public string? GetResourceData(string filename)
    {
        var stream = GetResource(filename);
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static string[] GetEmbeddedResources()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
    }

    public static Stream? GetEmbeddedResourceStream(string name)
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
    }

    internal RegexCache RegexCache { get; } = new();

    public WorldModelVersion Version { get; internal set; }

    internal string? VersionString { get; set; }

    internal IOutputLogger? OutputLogger { get; private set; }

    public int ASLVersion => int.Parse(VersionString!);

    public string GameID => Game.Fields[FieldDefinitions.GameID];
        
    IEnumerable<string> IGame.GetResourceNames()
    {
        return GetResourceNames == null ? [] : GetResourceNames();
    }

    public string Category => Game.Fields[FieldDefinitions.Category];
    public string Description => Game.Fields[FieldDefinitions.Description];
    public string Cover => Game.Fields[FieldDefinitions.Cover];
}