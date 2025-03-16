using System;
using System.Collections.Generic;
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

public partial class WorldModel : IGameDebug, IGameTimer
{
    private readonly Elements _elements;
    private readonly Dictionary<string, int> _nextUniqueId = new();
    private readonly Template _template;
    private readonly UndoLogger _undoLogger;
    private readonly GameData _gameData;
    private readonly Dictionary<string, ObjectType> _debuggerObjectTypes = new();
    private readonly Dictionary<string, ElementType> _debuggerElementTypes = new();
    private readonly Dictionary<ElementType, IElementFactory> _elementFactories = new();
    private readonly ExpressionOwner _expressionOwner;
    private readonly List<string> _attributeNames = [];
    private readonly RegexCache _regexCache = new();
    private readonly CallbackManager _callbacks = new();
    private readonly object _threadLock = new();
    private readonly object _commandOverrideLock = new();
        
    private IPlayer _playerUi = null;
    private GameSaver _saver;
        
    private Walkthroughs _walkthroughs;
    private TimerRunner _timerRunner;
        
    private bool _commandOverride = false;
    private string _commandOverrideInput;
        
    private IOutputLogger _outputLogger;
    private LegacyOutputLogger _legacyOutputLogger;
        
    private bool _loadedFromSaved = false;
    private bool _editMode = false;
        
    private List<string> _errors;
    private GameState _state = GameState.NotStarted;
    private ThreadState _threadState = ThreadState.Ready;

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

    public event EventHandler<ElementFieldUpdatedEventArgs> ElementFieldUpdated;
    public event EventHandler<ElementRefreshEventArgs> ElementRefreshed;
    public event EventHandler<ElementFieldUpdatedEventArgs> ElementMetaFieldUpdated;
    public event EventHandler<LoadStatusEventArgs> LoadStatus;

    public event Action<int> RequestNextTimerTick;

    public WorldModel()
        : this(null)
    {
    }

    public WorldModel(GameData gameData)
    {
        _expressionOwner = new ExpressionOwner(this);
        _template = new Template(this);
        InitialiseElementFactories();
        ObjectFactory = (ObjectFactory)_elementFactories[ElementType.Object];

        InitialiseDebuggerObjectTypes();
        _gameData = gameData;
        _elements = new Elements();
        _undoLogger = new UndoLogger(this);
        Game = ObjectFactory.CreateObject("game", ObjectType.Game);
    }

    public bool UseNcalc { get; set; }

    private void InitialiseElementFactories()
    {
        foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                     typeof(IElementFactory)))
        {
            AddElementFactory((IElementFactory)Activator.CreateInstance(t));
        }
    }

    private void AddElementFactory(IElementFactory factory)
    {
        _elementFactories.Add(factory.CreateElementType, factory);
        factory.WorldModel = this;
        factory.ObjectsUpdated += ElementsUpdated;
    }

    private void ElementsUpdated(object sender, ObjectsUpdatedEventArgs args)
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
        _state = GameState.Finished;

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

    internal string GetUniqueId(string prefix = null)
    {
        if (string.IsNullOrEmpty(prefix)) prefix = "k";
        _nextUniqueId.TryAdd(prefix, 0);
        string newid;
        do
        {
            _nextUniqueId[prefix]++;
            newid = prefix + _nextUniqueId[prefix];
        } while (_elements.ContainsKey(newid));
            
        return newid;
    }

    public Element Game { get; }

    public Element Object(string name)
    {
        return _elements.Get(ElementType.Object, name);
    }

    public ObjectFactory ObjectFactory { get; }

    public IElementFactory GetElementFactory(ElementType t)
    {
        return _elementFactories[t];
    }

    public void PrintTemplate(string t)
    {
        Print(_template.GetText(t));
    }

    public void Print(string text, bool linebreak = true)
    {
        if (Version >= WorldModelVersion.v540 && _elements.ContainsKey(ElementType.Function, "OutputText"))
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

            if (_legacyOutputLogger != null)
            {
                _legacyOutputLogger.AddText(text, linebreak);
            }
        }
    }

    internal QuestList<Element> GetAllObjects()
    {
        return new QuestList<Element>(_elements.Objects);
    }

    private QuestList<Element> GetObjectsInScope(string scopeFunction)
    {
        if (_elements.ContainsKey(ElementType.Function, scopeFunction))
        {
            return (QuestList<Element>)RunProcedure(scopeFunction, true);
        }
        throw new Exception($"No function '{scopeFunction}'");
    }

    public static bool ObjectContains(Element parent, Element searchObj)
    {
        if (searchObj.Parent == null) return false;
        return searchObj.Parent == parent || ObjectContains(parent, searchObj.Parent);
    }

    private readonly object _waitForResponseLock = new();
    private string _menuResponse = null;
    private IDictionary<string, string> _menuOptions = null;

    internal string DisplayMenu(string caption, IDictionary<string, string> options, bool allowCancel, bool async)
    {
        Print(caption);

        var menuData = new MenuData(caption, options, allowCancel);
        _menuOptions = options;

        _playerUi.ShowMenu(menuData);

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

        return _menuResponse;
    }

    internal string DisplayMenu(string caption, IList<string> options, bool allowCancel, bool async)
    {
        var optionsDictionary = options.ToDictionary(option => option);
        return DisplayMenu(caption, optionsDictionary, allowCancel, async);
    }

    public void SetMenuResponse(string response)
    {
        var menuCallback = _callbacks.Pop(CallbackManager.CallbackTypes.Menu);
        if (menuCallback != null)
        {
            if (response != null) Print(" - " + _menuOptions[response]);
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

    public IEnumerable<Element> Objects
    {
        get
        {
            foreach (Element o in _elements.Objects)
                yield return o;
        }
    }

    public bool ObjectExists(string name)
    {
        return _elements.ContainsKey(ElementType.Object, name);
    }

    /// <summary>
    /// Attempt to resolve an element name from elements which are eligible for expression,
    /// i.e. objects and timers
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool TryResolveExpressionElement(string name, out Element element)
    {
        element = null;
        if (!_elements.ContainsKey(name)) return false;

        Element result = _elements.Get(name);
        if (result.ElemType == ElementType.Object || result.ElemType == ElementType.Timer)
        {
            element = result;
            return true;
        }

        return false;
    }

    internal void RemoveElement(ElementType type, string name)
    {
        _elements.Remove(type, name);
    }

    internal IEnumerable<Element> ElementTypes
    {
        get { return _elements.GetElements(ElementType.ObjectType); }
    }

    internal void SetGameName(string name)
    {
        if (_playerUi != null) _playerUi.UpdateGameName(name);
    }

    public async Task<bool> Initialise(IPlayer player, bool? isCompiled = null)
    {
        _editMode = false;
        _playerUi = player;
        QuestViva.Engine.GameLoader.GameLoader loader = new QuestViva.Engine.GameLoader.GameLoader(this, QuestViva.Engine.GameLoader.GameLoader.LoadMode.Play, isCompiled);
        bool result = await InitialiseInternal(loader);
        if (result)
        {
            _walkthroughs = new Walkthroughs(this);
        }
        return result;
    }

    public async Task<bool> InitialiseEdit()
    {
        _editMode = true;
        QuestViva.Engine.GameLoader.GameLoader loader = new QuestViva.Engine.GameLoader.GameLoader(this, QuestViva.Engine.GameLoader.GameLoader.LoadMode.Edit);
        return await InitialiseInternal(loader);
    }

    private async Task<bool> InitialiseInternal(QuestViva.Engine.GameLoader.GameLoader loader)
    {
        if (_state != GameState.NotStarted)
        {
            throw new Exception("Game already initialised");
        }
        loader.FilenameUpdated += loader_FilenameUpdated;
        loader.LoadStatus += loader_LoadStatus;
        _state = GameState.Loading;
            
        var success = await loader.Load(_gameData);
            
        DebugEnabled = !loader.IsCompiledFile;
        _state = success ? GameState.Running : GameState.Finished;
        _errors = loader.Errors;
        _saver = new QuestViva.Engine.GameLoader.GameSaver(this);
        if (Version <= WorldModelVersion.v530)
        {
            _legacyOutputLogger = new LegacyOutputLogger(this);
            _outputLogger = _legacyOutputLogger;
        }
        else
        {
            _outputLogger = new OutputLogger(this);
        }
        return success;
    }

    void loader_LoadStatus(object sender, QuestViva.Engine.GameLoader.GameLoader.LoadStatusEventArgs e)
    {
        if (LoadStatus != null)
        {
            LoadStatus(this, new LoadStatusEventArgs(e.Status));
        }
    }

    void loader_FilenameUpdated(string filename)
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
                    PlayerUI.Show("Panes");
                    PlayerUI.Show("Location");
                    PlayerUI.Show("Command");
                }
                if (_elements.ContainsKey(ElementType.Function, "InitInterface")) RunProcedure("InitInterface");
                if (!_loadedFromSaved)
                {
                    if (_elements.ContainsKey(ElementType.Function, "StartGame")) RunProcedure("StartGame");
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
                        PlayerUI.RunScript("loadHtml", new object[] { output.Fields.GetString("html") });
                        PlayerUI.RunScript("markScrollPosition", null);
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

    public List<string> Errors
    {
        get { return _errors; }
    }

    public IWalkthroughs Walkthroughs
    {
        get
        {
            return _walkthroughs;
        }
    }

    public List<string> DebuggerObjectTypes
    {
        get { return new List<string>(_debuggerObjectTypes.Keys.Union(_debuggerElementTypes.Keys)); }
    }

    public void SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata)
    {
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
                    Print("> " + QuestViva.Engine.Utility.SafeXML(command));
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
                Callback getinputCallback = _callbacks.Pop(CallbackManager.CallbackTypes.GetInput);
                if (getinputCallback != null)
                {
                    _commandOverride = false;
                    getinputCallback.Context.Parameters["result"] = command;
                    RunCallbackAndFinishTurn(getinputCallback);
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

    public void SendCommand(string command, IDictionary<string, string> metadata)
    {
        SendCommand(command, 0, metadata);
    }

    public void SendCommand(string command)
    {
        SendCommand(command, 0, null);
    }

    public void SendEvent(string eventName, string param)
    {
        Element handler;
        _elements.TryGetValue(ElementType.Function, eventName, out handler);

        if (handler == null)
        {
            Print(string.Format("Error - no handler for event '{0}'", eventName));
            return;
        }

        Parameters parameters = new Parameters();
        parameters.Add((string)handler.Fields[FieldDefinitions.ParamNames][0], param);

        RunProcedure(eventName, parameters, false);
        if (Version >= WorldModelVersion.v540)
        {
            if (Version < WorldModelVersion.v580)
            {
                TryFinishTurn();
            }
            if (State != GameState.Finished)
            {
                UpdateLists();
            }
            SendNextTimerRequest();
        }
    }

    public string Filename
    {
        get { return _gameData.Filename; }
    }

    public void Finish()
    {
        FinishGame();
    }

    public string SaveExtension { get { return "quest-save"; } }

    public event PrintTextHandler PrintText;
    public event UpdateListHandler UpdateList;
    public event FinishedHandler Finished;
    public event EventHandler<ObjectsUpdatedEventArgs> ObjectsUpdated;
    public event ErrorHandler LogError;

    internal Template Template
    {
        get { return _template; }
    }

    public UndoLogger UndoLogger
    {
        get { return _undoLogger; }
    }

    private void UpdateStatusVariables()
    {
        if (_elements.ContainsKey(ElementType.Function, "UpdateStatusAttributes"))
        {
            try
            {
                RunProcedure("UpdateStatusAttributes");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
    }

    internal void UpdateLists()
    {
        UpdateObjectsList();
        UpdateExitsList();
        UpdateStatusVariables();
    }

    internal void UpdateObjectsList()
    {
        UpdateObjectsList("GetPlacesObjectsList", ListType.ObjectsList);
        UpdateObjectsList("ScopeInventory", ListType.InventoryList);
    }

    internal void UpdateObjectsList(string scope, ListType listType)
    {
        if (UpdateList != null)
        {
            List<ListData> objects = new List<ListData>();
            foreach (Element obj in GetObjectsInScope(scope))
            {
                if (Version <= WorldModelVersion.v520 || !_elements.ContainsKey(ElementType.Function, "GetDisplayVerbs"))
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
    }

    internal void UpdateExitsList()
    {
        if (UpdateList != null)
        {
            UpdateList(ListType.ExitsList, GetExitsListData());
        }
    }

    private string GetListDisplayAlias(Element obj)
    {
        if (_elements.ContainsKey(ElementType.Function, "GetListDisplayAlias"))
        {
            return (string)RunProcedure("GetListDisplayAlias", new Parameters("obj", obj), true);
        }
        return GetDisplayAlias(obj);
    }

    private string GetDisplayAlias(Element obj)
    {
        if (_elements.ContainsKey(ElementType.Function, "GetDisplayAlias"))
        {
            return (string)RunProcedure("GetDisplayAlias", new Parameters("obj", obj), true);
        }
        return obj.Name;
    }

    private IEnumerable<string> GetDisplayVerbs(Element obj)
    {
        return (QuestList<string>)RunProcedure("GetDisplayVerbs", new Parameters("object", obj), true);
    }

    private List<ListData> GetExitsListData()
    {
        List<ListData> exits = new List<ListData>();
        var scopeFunction = "ScopeExits";
        if (Version >= WorldModelVersion.v530 && Elements.ContainsKey(ElementType.Function, "GetExitsList"))
        {
            scopeFunction = "GetExitsList";
        }
        foreach (Element exit in GetObjectsInScope(scopeFunction))
        {
            IEnumerable<string> verbs;
            if (Version <= WorldModelVersion.v520 || !_elements.ContainsKey(ElementType.Function, "GetDisplayVerbs"))
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
        List<string> result = new List<string>();
        IEnumerable<Element> elements;

        if (_debuggerObjectTypes.ContainsKey(type))
        {
            ObjectType filterType = _debuggerObjectTypes[type];
            elements = _elements.ObjectsFiltered(o => o.Type == filterType);
        }
        else
        {
            ElementType filterType = _debuggerElementTypes[type];
            elements = _elements.GetElements(filterType);
        }

        foreach (Element obj in elements)
        {
            result.Add(obj.Name);
        }

        return result;
    }

    public DebugData GetDebugData(string _, string el)
    {
        return _elements.Get(el).GetDebugData();
    }
        
    public DebugData GetDebugData(string el)
    {
        return _elements.Get(el).GetDebugData();
    }

    public DebugData GetInheritedTypesDebugData(string el)
    {
        return _elements.Get(el).Fields.GetInheritedTypesDebugData();
    }

    public DebugDataItem GetDebugDataItem(string el, string attribute)
    {
        return _elements.Get(el).Fields.GetDebugDataItem(attribute);
    }

    public void StartWait()
    {
        _callbacks.Pop(CallbackManager.CallbackTypes.Wait);
        _playerUi.DoWait();

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
        _playerUi.DoWait();
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
            if (_state == GameState.Finished) return;
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
        _playerUi.DoPause(ms);

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
        foreach (Element jsRef in _elements.GetElements(ElementType.Javascript))
        {
            if (Version == WorldModelVersion.v500)
            {
                // v500 games used Frame.js functions for static panel feature. This is now implemented natively
                // in Player and WebPlayer.
                if (jsRef.Fields[FieldDefinitions.Src].ToLower() == "frame.js") continue;
            }

            result.Add(jsRef.Fields[FieldDefinitions.Src]);
        }
        return result;
    }

    // TO DO: This could actually be removed now, as we can dynamically load stylesheets. Core.aslx InitInterface
    // should simply be able to use the SetWebFontName function to load game.defaultwebfont
    public IEnumerable<string> GetExternalStylesheets()
    {
        if (Version < WorldModelVersion.v530) return null;

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
        RunScript(script, (Parameters)null, false);
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
        return RunScript(script, parameters, true, thisElement);
    }

    private object RunScript(IScript script, Parameters parameters, bool expectResult)
    {
        return RunScript(script, parameters, expectResult, null);
    }

    private object RunScript(IScript script, Parameters parameters, bool expectResult, Element thisElement)
    {
        Context c = new Context();
        if (parameters == null) parameters = new Parameters();
        if (thisElement != null) parameters.Add("this", thisElement);
        c.Parameters = parameters;

        return RunScript(script, c, expectResult);
    }

    private void RunScript(IScript script, Context c)
    {
        RunScript(script, c, false);
    }

    private object RunScript(IScript script, Context c, bool expectResult)
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
        Element proc = GetElementFactory(ElementType.Function).Create(name);
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
        Element del = GetElementFactory(ElementType.Delegate).Create(name);
        return del;
    }

    public void RunProcedure(string name)
    {
        RunProcedure(name, false);
    }

    public object RunProcedure(string name, bool expectResult)
    {
        return RunProcedure(name, null, expectResult);
    }

    public object RunProcedure(string name, Parameters parameters, bool expectResult)
    {
        if (_elements.ContainsKey(ElementType.Function, name))
        {
            Element function = _elements.Get(ElementType.Function, name);

            // Only check for too few parameters for games for Quest 5.2 or later, as previous Quest versions
            // would ignore this (but would usually still fail when the function was run, as the required
            // variable wouldn't exist). For Quest 5.3, an additional check if parameters is non-null but empty.

            bool parametersInvalid = false;
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
        else
        {
            Print(string.Format("Error - no such procedure '{0}'", name));
        }
        return null;
    }

    public Element Procedure(string name)
    {
        if (!_elements.ContainsKey(ElementType.Function, name)) return null;
        return _elements.Get(ElementType.Function, name);
    }

    internal Element GetObjectType(string name)
    {
        return _elements.Get(ElementType.ObjectType, name);
    }

    public GameState State
    {
        get { return _state; }
    }

    public Elements Elements
    {
        get { return _elements; }
    }

    public void Save(string filename, string html)
    {
        string saveData = Save(SaveMode.SavedGame, html: html);
        File.WriteAllText(filename, saveData);
    }

    public byte[] Save(string html)
    {
        string saveData = Save(SaveMode.SavedGame, html: html);
        return System.Text.Encoding.UTF8.GetBytes(saveData);
    }

    public string Save(SaveMode mode, bool? includeWalkthrough = null, string html = null)
    {
        return _saver.Save(mode, includeWalkthrough, html);
    }

    public static Type ConvertTypeNameToType(string name)
    {
        Type type;
        if (TypeNamesToTypes.TryGetValue(name, out type))
        {
            return type;
        }

        if (name == "null") return null;

        // TO DO: type name could also be a DelegateImplementation
        //if (value is DelegateImplementation) return ((DelegateImplementation)value).TypeName;

        throw new ArgumentOutOfRangeException(string.Format("Unrecognised type name '{0}'", name));
    }

    public static string ConvertTypeToTypeName(Type type)
    {
        string name;
        if (TypesToTypeNames.TryGetValue(type, out name))
        {
            return name;
        }

        foreach (KeyValuePair<Type, string> kvp in TypesToTypeNames)
        {
            if (kvp.Key.IsAssignableFrom(type))
            {
                return kvp.Value;
            }
        }

        throw new ArgumentOutOfRangeException(string.Format("Unrecognised type '{0}'", type.ToString()));
    }

    public Stream GetLibraryStream(string filename)
    {
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

    public string GetExternalPath(string file)
    {
        throw new NotImplementedException();
    }

    internal string GetExternalURL(string file)
    {
        return _playerUi.GetURL(file);
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
        path = QuestViva.Utility.Files.RemoveFileColonPrefix(path);
        System.IO.SearchOption option = recurse ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
        foreach (var result in System.IO.Directory.GetFiles(path, searchPattern, option))
        {
            if (result == Filename) continue;
            string filename = System.IO.Path.GetFileName(result);
            if (!list.Contains(filename)) list.Add(filename);
        }
    }

    public IEnumerable<string> GetAvailableExternalFiles(string searchPatterns)
    {
        List<string> result = new List<string>();
        string[] patterns = searchPatterns.Split(';');
        foreach (string searchPattern in patterns)
        {
            AddFilesInPathToList(result, System.IO.Path.GetDirectoryName(Filename), false, searchPattern);
        }
        return result;
    }

    internal void NotifyElementFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
    {
        if (!element.Initialised) return;
        if (ElementFieldUpdated != null) ElementFieldUpdated(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
    }

    internal void NotifyElementMetaFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
    {
        if (!element.Initialised) return;
        if (ElementMetaFieldUpdated != null) ElementMetaFieldUpdated(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
    }

    internal void NotifyElementRefreshed(Element element)
    {
        if (ElementRefreshed != null) ElementRefreshed(this, new ElementRefreshEventArgs(element));
    }

    public bool EditMode
    {
        get { return _editMode; }
    }

    internal ExpressionOwner ExpressionOwner
    {
        get { return _expressionOwner; }
    }

    private void ScrollToEnd()
    {
        PlayerUI.RunScript("scrollToEnd", null);
    }

    private void ChangeThreadState(ThreadState newState, bool scroll = true)
    {
        if (scroll && Version >= WorldModelVersion.v540 && (newState == ThreadState.Ready || newState == ThreadState.Waiting))
        {
            ScrollToEnd();
        }

        if (newState == ThreadState.Waiting && _state == GameState.Finished) throw new Exception("Game is finished");
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
        Thread newThread = new Thread(new ThreadStart(routine));
        newThread.Start();
        WaitUntilFinishedWorking();
    }

    void LogException(Exception ex)
    {
        LogError?.Invoke(ex);
    }

    internal IPlayer PlayerUI
    {
        get { return _playerUi; }
    }

    public ElementType GetElementTypeForTypeString(string typeString)
    {
        return Element.GetElementTypeForTypeString(typeString);
    }

    public ObjectType GetObjectTypeForTypeString(string typeString)
    {
        return Element.GetObjectTypeForTypeString(typeString);
    }

    public string GetTypeStringForElementType(ElementType type)
    {
        return Element.GetTypeStringForElementType(type);
    }

    public string GetTypeStringForObjectType(ObjectType type)
    {
        return Element.GetTypeStringForObjectType(type);
    }

    public bool IsDefaultTypeName(string typeName)
    {
        return DefaultTypeNames.ContainsValue(typeName);
    }

    public Element AddNewTemplate(string templateName)
    {
        return _template.AddTemplate(templateName, string.Empty, false);
    }

    public Element TryGetTemplateElement(string templateName)
    {
        if (!_template.TemplateExists(templateName)) return null;
        return _template.GetTemplateElement(templateName);
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
        string root = s_removeTrailingDigits().Replace(elementName, "");
        bool elementAlreadyExists = true;
        int number = 0;
        string result = null;

        while (elementAlreadyExists)
        {
            number++;
            result = root + number.ToString();
            elementAlreadyExists = Elements.ContainsKey(result);
        }

        return result;
    }

    internal void AddAttributeName(string name)
    {
        if (!_attributeNames.Contains(name)) _attributeNames.Add(name);
    }

    public IEnumerable<string> GetAllAttributeNames
    {
        get { return _attributeNames.AsReadOnly(); }
    }

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
        return _commandOverrideInput;
    }

    internal void GetNextCommandInputAsync(IScript callback, Context c)
    {
        _callbacks.Push(CallbackManager.CallbackTypes.GetInput, new Callback(callback, c), "Only one 'get input' can be in progress at a time");
        GetNextCommandInput(true);
    }

    public void Tick(int elapsedTime)
    {
        if (_state == GameState.Finished) return;
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
        if (_state == GameState.Finished) return;
        int next = _timerRunner.GetTimeUntilNextTimerRuns();
        if (RequestNextTimerTick != null) RequestNextTimerTick(next);
        System.Diagnostics.Debug.Print("Request next timer in {0}", next);
    }

    private bool m_questionResponse;

    internal bool ShowQuestion(string caption)
    {
        _callbacks.Pop(CallbackManager.CallbackTypes.Question);
        _playerUi.ShowQuestion(caption);

        ChangeThreadState(ThreadState.Waiting);

        lock (_waitForResponseLock)
        {
            Monitor.Wait(_waitForResponseLock);
        }

        ChangeThreadState(ThreadState.Working);

        return m_questionResponse;
    }

    internal void ShowQuestionAsync(string caption, IScript callback, Context c)
    {
        _callbacks.Push(CallbackManager.CallbackTypes.Question, new Callback(callback, c), "Only one question can be asked at a time.");
        _playerUi.ShowQuestion(caption);
    }

    public void SetQuestionResponse(bool response)
    {
        Callback questionCallback = _callbacks.Pop(CallbackManager.CallbackTypes.Question);
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
                m_questionResponse = response;

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
        if (!_callbacks.AnyOutstanding())
        {
            if (_elements.ContainsKey(ElementType.Function, "FinishTurn"))
            {
                try
                {
                    RunProcedure("FinishTurn");
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }
    }

    private void TryRunOnFinallyScripts()
    {
        if (_callbacks.AnyOutstanding()) return;
        IEnumerable<Callback> onReadyScripts = _callbacks.FlushOnReadyCallbacks();
        foreach (var callback in onReadyScripts)
        {
            RunScript(callback.Script, callback.Context);
        }
    }

    public class PackageIncludeFile
    {
        public string Filename { get; set; }
        public Stream Content { get; set; }
    }

    public bool CreatePackage(string filename, bool includeWalkthrough, out string error, IEnumerable<PackageIncludeFile> includeFiles, Stream outputStream)
    {
        Packager packager = new Packager(this);
        return packager.CreatePackage(filename, includeWalkthrough, out error, includeFiles, outputStream);
    }

    public Func<string, Stream> ResourceGetter { get; internal set; }
    public Func<IEnumerable<string>> GetResourceNames { get; internal set; }
    public bool DebugEnabled { get; private set; }

    private static List<string> s_functionNames = null;

    public IEnumerable<string> GetBuiltInFunctionNames()
    {
        if (s_functionNames == null)
        {
            System.Reflection.MethodInfo[] methods = typeof(ExpressionOwner).GetMethods();
            System.Reflection.MethodInfo[] stringMethods = typeof(StringFunctions).GetMethods();
            System.Reflection.MethodInfo[] dateTimeMethods = typeof(DateTimeFunctions).GetMethods();

            IEnumerable<System.Reflection.MethodInfo> allMethods = methods.Union(stringMethods);

            s_functionNames = new List<string>(allMethods.Select(m => m.Name));
        }

        return s_functionNames.AsReadOnly();
    }

    public void PlaySound(string filename, bool sync, bool looped)
    {
        _playerUi.PlaySound(filename, sync, looped);
        if (sync)
        {
            ChangeThreadState(ThreadState.Waiting);

            lock (_waitForResponseLock)
            {
                Monitor.Wait(_waitForResponseLock);
            }
        }
    }

    internal void UpdateElementSortOrder(Element movedElement)
    {
        // There's no need to worry about element sort order when playing the game, unless this is an element that can be seen by the
        // player
        if (!EditMode)
        {
            bool doUpdate = false;
            if (movedElement.ElemType == ElementType.Object && (movedElement.Type == ObjectType.Exit || movedElement.Type == ObjectType.Object))
            {
                doUpdate = true;
            }
            if (!doUpdate) return;
        }

        // This function is called when an element is moved to a new parent.
        // When this happens, its SortIndex MetaField must be updated so that it
        // is at the end of the list of children.

        int maxIndex = -1;

        foreach (Element sibling in _elements.GetDirectChildren(movedElement.Parent))
        {
            int thisSortIndex = sibling.MetaFields[MetaFieldDefinitions.SortIndex];
            if (thisSortIndex > maxIndex) maxIndex = thisSortIndex;
        }

        movedElement.MetaFields[MetaFieldDefinitions.SortIndex] = maxIndex + 1;
    }

    public bool Assert(string expr)
    {
        Expression<bool> expression = new Expression<bool>(expr, new ScriptContext(this));
        Context c = new Context();
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
        
#nullable enable

    public Stream? GetResource(string filename)
    {
        if (ResourceGetter != null)
        {
            return ResourceGetter.Invoke(filename);
        }
        return _gameData.GetAdjacentFile(filename);
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
        
#nullable restore

    public string GetResourcePath(string filename)
    {
        throw new NotImplementedException();
    }

    internal RegexCache RegexCache { get { return _regexCache; } }

    public WorldModelVersion Version { get; internal set; }

    internal string VersionString { get; set; }

    public string TempFolder { get; set; }

    internal IOutputLogger OutputLogger { get { return _outputLogger; } }

    public int ASLVersion { get { return int.Parse(VersionString); } }

    public string GameID => Game.Fields[FieldDefinitions.GameID];
        
    IEnumerable<string> IGame.GetResourceNames()
    {
        return GetResourceNames == null ? [] : GetResourceNames();
    }

    public string Category { get { return Game.Fields[FieldDefinitions.Category]; } }
    public string Description { get { return Game.Fields[FieldDefinitions.Description]; } }
    public string Cover { get { return Game.Fields[FieldDefinitions.Cover]; } }
}