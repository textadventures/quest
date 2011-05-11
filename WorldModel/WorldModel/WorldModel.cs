using System;
using System.Collections.Generic;
using System.Threading;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public enum GameState
    {
        NotStarted,
        Loading,
        Running,
        Finished
    }

    public enum ThreadState
    {
        Ready,
        Working,
        Waiting
    }

    public enum UpdateSource
    {
        System,
        User
    }

    public class WorldModel : IASL, IASLDebug
    {
        private Element m_game;
        private Element m_player;
        private Elements m_elements;
        private Dictionary<string, int> m_nextUniqueID = new Dictionary<string, int>();
        private Template m_template;
        private UndoLogger m_undoLogger;
        private string m_filename;
        private string m_originalFilename;
        private string m_libFolder = null;
        private List<string> m_errors;
        private Dictionary<string, ObjectType> m_debuggerObjectTypes = new Dictionary<string, ObjectType>();
        private GameState m_state = GameState.NotStarted;
        private Dictionary<ElementType, IElementFactory> m_elementFactories = new Dictionary<ElementType, IElementFactory>();
        private ObjectFactory m_objectFactory;
        private GameSaver m_saver;
        private bool m_loadedFromSaved = false;
        private string m_saveFilename = string.Empty;
        private bool m_editMode = false;
        private Functions.ExpressionOwner m_expressionOwner;
        private IPlayer m_playerUI = null;
        private ThreadState m_threadState = ThreadState.Ready;
        private object m_threadLock = new object();

        private static Dictionary<ObjectType, string> s_defaultTypeNames = new Dictionary<ObjectType, string>();
        private static Dictionary<string, Type> s_typeNamesToTypes = new Dictionary<string, Type>();
        private static Dictionary<Type, string> s_typesToTypeNames = new Dictionary<Type, string>();

        public event EventHandler<ElementFieldUpdatedEventArgs> ElementFieldUpdated;
        public event EventHandler<ElementRefreshEventArgs> ElementRefreshed;

        public class ElementFieldUpdatedEventArgs : EventArgs
        {
            internal ElementFieldUpdatedEventArgs(Element element, string attribute, object newValue, bool isUndo)
            {
                Element = element;
                Attribute = attribute;
                NewValue = newValue;
                IsUndo = isUndo;
            }

            public Element Element { get; private set; }
            public string Attribute { get; private set; }
            public object NewValue { get; private set; }
            public bool IsUndo { get; private set; }
            public bool Refresh { get; private set; }
        }

        public class ElementRefreshEventArgs : EventArgs
        {
            internal ElementRefreshEventArgs(Element element)
            {
                Element = element;
            }

            public Element Element { get; private set; }
        }
        
        static WorldModel()
        {
            s_defaultTypeNames.Add(ObjectType.Object, "defaultobject");
            s_defaultTypeNames.Add(ObjectType.Exit, "defaultexit");
            s_defaultTypeNames.Add(ObjectType.Command, "defaultcommand");
            s_defaultTypeNames.Add(ObjectType.Game, "defaultgame");

            s_typeNamesToTypes.Add("string", typeof(string));
            s_typeNamesToTypes.Add("script", typeof(IScript));
            s_typeNamesToTypes.Add("boolean", typeof(bool));
            s_typeNamesToTypes.Add("int", typeof(int));
            s_typeNamesToTypes.Add("double", typeof(double));
            s_typeNamesToTypes.Add("object", typeof(Element));
            s_typeNamesToTypes.Add("stringlist", typeof(QuestList<string>));
            s_typeNamesToTypes.Add("objectlist", typeof(QuestList<Element>));
            s_typeNamesToTypes.Add("stringdictionary", typeof(QuestDictionary<string>));
            s_typeNamesToTypes.Add("objectdictionary", typeof(QuestDictionary<Element>));
            s_typeNamesToTypes.Add("scriptdictionary", typeof(QuestDictionary<IScript>));

            foreach (KeyValuePair<string, Type> kvp in s_typeNamesToTypes)
            {
                s_typesToTypeNames.Add(kvp.Value, kvp.Key);
            }
        }

        public WorldModel()
            : this(null, null)
        {
        }

        public WorldModel(string filename, string originalFilename)
        {
            m_expressionOwner = new Functions.ExpressionOwner(this);
            m_template = new Template(this);
            InitialiseElementFactories();
            m_objectFactory = (ObjectFactory)m_elementFactories[ElementType.Object];

            InitialiseDebuggerObjectTypes();
            m_filename = filename;
            m_originalFilename = originalFilename;
            m_elements = new Elements();
            m_undoLogger = new UndoLogger(this);
            m_saver = new GameSaver(this);
            m_game = ObjectFactory.CreateObject("game", ObjectType.Game);
        }

        public WorldModel(string filename, string libFolder, string originalFilename)
            : this(filename, originalFilename)
        {
            m_libFolder = libFolder;
        }

        private void InitialiseElementFactories()
        {
            foreach (Type t in AxeSoftware.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IElementFactory)))
            {
                AddElementFactory((IElementFactory)Activator.CreateInstance(t));
            }
        }

        private void AddElementFactory(IElementFactory factory)
        {
            m_elementFactories.Add(factory.CreateElementType, factory);
            factory.WorldModel = this;
            factory.ObjectsUpdated += ElementsUpdated;
        }

        internal static Dictionary<ObjectType, string> DefaultTypeNames
        {
            get { return s_defaultTypeNames; }
        }

        void ElementsUpdated(object sender, ObjectsUpdatedEventArgs args)
        {
            if (ObjectsUpdated != null) ObjectsUpdated(this, args);
        }

        private void InitialiseDebuggerObjectTypes()
        {
            m_debuggerObjectTypes.Add("Objects", ObjectType.Object);
            m_debuggerObjectTypes.Add("Exits", ObjectType.Exit);
            m_debuggerObjectTypes.Add("Commands", ObjectType.Command);
            m_debuggerObjectTypes.Add("Game", ObjectType.Game);
        }

        public void FinishGame()
        {
            m_state = GameState.Finished;
            if (Finished != null) Finished();
        }

        internal string GetUniqueID()
        {
            return GetUniqueID(null);
        }

        internal string GetUniqueID(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) prefix = "k";
            if (!m_nextUniqueID.ContainsKey(prefix))
            {
                m_nextUniqueID.Add(prefix, 0);
            }

            m_nextUniqueID[prefix]++;
            return prefix + m_nextUniqueID[prefix].ToString();
        }

        public Element Game
        {
            get { return m_game; }
        }

        public Element Player
        {
            get { return m_player; }
        }

        public Element Object(string name)
        {
            return m_elements.Get(ElementType.Object, name);
        }

        public ObjectFactory ObjectFactory
        {
            get { return m_objectFactory; }
        }

        public IElementFactory GetElementFactory(ElementType t)
        {
            return m_elementFactories[t];
        }

        public void PrintTemplate(string t)
        {
            Print(m_template.GetText(t));
        }

        public void Print(string text)
        {
            if (PrintText != null)
            {
                PrintText("<output>" + text + "</output>");
            }
        }

        internal QuestList<Element> GetAllObjects()
        {
            return new QuestList<Element>(m_elements.Objects);
        }

        internal QuestList<Element> GetObjectsInScope(string scope)
        {
            string fn = "Scope" + scope;
            if (m_elements.ContainsKey(ElementType.Function, fn))
            {
                return (QuestList<Element>)RunProcedure(fn, true);
            }
            throw new Exception(string.Format("No function '{0}'", fn));
        }

        public bool ObjectContains(Element parent, Element searchObj)
        {
            if (searchObj.Parent == null) return false;
            if (searchObj.Parent == parent) return true;
            return ObjectContains(parent, searchObj.Parent);
        }

        private object m_menuLock = new object();
        private string m_menuResponse = null;

        internal string DisplayMenu(string caption, IDictionary<string, string> options, bool allowCancel)
        {
            MenuData menuData = new MenuData(caption, options, allowCancel);

            m_playerUI.ShowMenu(menuData);

            ChangeThreadState(ThreadState.Waiting);

            lock (m_menuLock)
            {
                Monitor.Wait(m_menuLock);
            }

            ChangeThreadState(ThreadState.Working);

            return m_menuResponse;
        }

        internal string DisplayMenu(string caption, IList<string> options, bool allowCancel)
        {
            Dictionary<string, string> optionsDictionary = new Dictionary<string, string>();
            foreach (string option in options)
            {
                optionsDictionary.Add(option, option);
            }
            return DisplayMenu(caption, optionsDictionary, allowCancel);
        }

        public void SetMenuResponse(string response)
        {
            DoInNewThreadAndWait(() =>
            {
                m_menuResponse = response;

                lock (m_menuLock)
                {
                    Monitor.Pulse(m_menuLock);
                }
            });
        }

        // This is actually currently not needed, as we don't have an "if ask" equivalent for Quest 5.0 games.
        public void SetQuestionResponse(bool response) { }

        public IEnumerable<Element> Objects
        {
            get
            {
                foreach (Element o in m_elements.Objects)
                    yield return o;
            }
        }

        public bool ObjectExists(string name)
        {
            return m_elements.ContainsKey(ElementType.Object, name);
        }

        internal void RemoveElement(ElementType type, string name)
        {
            m_elements.Remove(type, name);
        }

        internal IEnumerable<Element> ElementTypes
        {
            get { return m_elements.GetElements(ElementType.ObjectType); }
        }

        internal void SetGameName(string name)
        {
            if (m_playerUI != null) m_playerUI.UpdateGameName(name);
        }

        #region IASL Members

        public bool Initialise(IPlayer player)
        {
            m_editMode = false;
            m_playerUI = player;
            GameLoader loader = new GameLoader(this, GameLoader.LoadMode.Play);
            return InitialiseInternal(loader);
        }

        public bool InitialiseEdit()
        {
            m_editMode = true;
            GameLoader loader = new GameLoader(this, GameLoader.LoadMode.Edit);
            return InitialiseInternal(loader);
        }

        private bool InitialiseInternal(GameLoader loader)
        {
            if (m_state != GameState.NotStarted)
            {
                throw new Exception("Game already initialised");
            }
            loader.FilenameUpdated += new GameLoader.FilenameUpdatedHandler(loader_FilenameUpdated);
            m_state = GameState.Loading;
            bool success = m_filename == null ? true : loader.Load(m_filename);
            m_state = success ? GameState.Running : GameState.Finished;
            m_errors = loader.Errors;
            return success;
        }

        void loader_FilenameUpdated(string filename)
        {
            // Update base ASLX filename to original filename if we're loading a saved game
            m_saveFilename = m_filename;
            m_filename = filename;
            m_loadedFromSaved = true;
        }

        public void Begin()
        {
            if (!m_elements.ContainsKey(ElementType.Object, "player")) throw new Exception("No player object found in game");
            m_player = Object("player");
            if (m_elements.ContainsKey(ElementType.Function, "InitInterface")) RunProcedure("InitInterface");
            if (!m_loadedFromSaved)
            {
                if (m_elements.ContainsKey(ElementType.Function, "StartGame")) RunProcedure("StartGame");
            }
            if (m_player.Parent == null) throw new Exception("No start location specified for player");
            UpdateLists();
            if (m_loadedFromSaved)
            {
                Print(string.Format("Loaded saved game in {0}", m_saveFilename));
                Print(string.Format("  (original game at {0})", m_filename));
            }
        }

        public List<string> Errors
        {
            get { return m_errors; }
        }

        public IWalkthrough Walkthrough
        {
            get {
                Element walkthroughElement = m_elements.GetSingle(ElementType.Walkthrough);
                if (walkthroughElement == null) return null;
                return new Walkthrough(walkthroughElement); 
            }
        }

        public List<string> DebuggerObjectTypes
        {
            get { return new List<string>(m_debuggerObjectTypes.Keys); }
        }

        public void SendCommand(string command)
        {
            DoInNewThreadAndWait(() =>
            {
                Print("");
                Print("> " + Utility.SafeXML(command));

                try
                {
                    RunProcedure("HandleCommand", new Parameters("command", command), false);
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
            });
        }

        public void SendEvent(string eventName, string param)
        {
            Element handler;
            m_elements.TryGetValue(ElementType.Function, eventName, out handler);

            if (handler == null)
            {
                Print(string.Format("Error - no handler for event '{0}'", eventName));
                return;
            }

            Parameters parameters = new Parameters();
            parameters.Add((string)handler.Fields[FieldDefinitions.ParamNames][0], param);

            RunProcedure(eventName, parameters, false);
        }

        public string Filename
        {
            get { return m_filename; }
        }

        public string SaveFilename
        {
            get { return m_saveFilename; }
        }

        public string BasePath
        {
            get { return System.IO.Path.GetDirectoryName(m_filename); }
        }

        public void Finish()
        {
            FinishGame();
        }

        public string SaveExtension { get { return "aslx"; } }

        public event PrintTextHandler PrintText;
        public event UpdateListHandler UpdateList;
        public event FinishedHandler Finished;
        public event EventHandler<ObjectsUpdatedEventArgs> ObjectsUpdated;
        public event ErrorHandler LogError;

        internal Template Template
        {
            get { return m_template; }
        }

        public UndoLogger UndoLogger
        {
            get { return m_undoLogger; }
        }

        internal void RaiseRequest(Request request, string data)
        {
            // TO DO: Replace with dictionary mapping the enum to lambda functions
            // TO DO: Move to RequestScript.cs?
            switch (request)
            {
                case Request.UpdateLocation:
                    m_playerUI.LocationUpdated(data);
                    break;
                case Request.GameName:
                    m_playerUI.UpdateGameName(data);
                    break;
                case Request.ClearScreen:
                    m_playerUI.ClearScreen();
                    break;
                case Request.ShowPicture:
                    m_playerUI.ShowPicture(data);
                    break;
                case Request.PanesVisible:
                    m_playerUI.SetPanesVisible(data);
                    break;
                case Request.Background:
                    m_playerUI.SetBackground(data);
                    break;
                case Request.Foreground:
                    m_playerUI.SetForeground(data);
                    break;
                case Request.RunScript:
                    m_playerUI.RunScript(data);
                    break;
                case Request.Quit:
                    m_playerUI.Quit();
                    break;
                case Request.FontName:
                    m_playerUI.SetFont(data);
                    break;
                case Request.FontSize:
                    m_playerUI.SetFontSize(data);
                    break;
                case Request.LinkForeground:
                    m_playerUI.SetLinkForeground(data);
                    break;
                case Request.Show:
                    m_playerUI.Show(data);
                    break;
                case Request.Hide:
                    m_playerUI.Hide(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("request", "Unhandled request type");
            }
        }

        internal void UpdateLists()
        {
            UpdateObjectsList();
            UpdateExitsList();
        }

        internal void UpdateObjectsList()
        {
            UpdateObjectsList("VisibleNotHeld", ListType.ObjectsList);
            UpdateObjectsList("Inventory", ListType.InventoryList);
        }

        internal void UpdateObjectsList(string scope, ListType listType)
        {
            if (UpdateList != null)
            {
                List<ListData> objects = new List<ListData>();
                foreach (Element obj in GetObjectsInScope(scope))
                {
                    if (scope == "Inventory")
                    {
                        objects.Add(new ListData(GetDisplayAlias(obj), obj.Fields[FieldDefinitions.InventoryVerbs]));
                    }
                    else
                    {
                        objects.Add(new ListData(GetDisplayAlias(obj), obj.Fields[FieldDefinitions.DisplayVerbs]));
                    }
                }
                // The "Places and Objects" list is generated by function, so we also
                // need to add any exits. (The UI is responsible for filtering out the
                // directional exits so they only display in the compass)
                if (scope == "VisibleNotHeld") objects.AddRange(GetExitsListData());
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

        private string GetDisplayAlias(Element obj)
        {
            if (!m_elements.ContainsKey(ElementType.Function, "GetDisplayAlias")) return obj.Name;
            return (string)RunProcedure("GetDisplayAlias", new Parameters("obj", obj), true);
        }

        private List<ListData> GetExitsListData()
        {
            List<ListData> exits = new List<ListData>();
            foreach (Element exit in GetObjectsInScope("Exits"))
            {
                exits.Add(new ListData(GetDisplayAlias(exit), exit.Fields[FieldDefinitions.DisplayVerbs]));
            }
            return exits;
        }

        public List<string> GetObjects(string type)
        {
            ObjectType filterType = m_debuggerObjectTypes[type];
            List<string> result = new List<string>();

            foreach (Element obj in m_elements.ObjectsFiltered(o => o.Type == filterType))
            {
               result.Add(obj.Name);
            }

            return result;
        }

        public DebugData GetDebugData(string el)
        {
            return m_elements.Get(el).GetDebugData();
        }

        public DebugData GetInheritedTypesDebugData(string el)
        {
            return m_elements.Get(el).Fields.GetInheritedTypesDebugData();
        }

        public DebugDataItem GetDebugDataItem(string el, string attribute)
        {
            return m_elements.Get(el).Fields.GetDebugDataItem(attribute);
        }

        public void FinishWait()
        {
        }

        public IEnumerable<string> GetExternalScripts()
        {
            var result = new List<string>();
            foreach (Element jsRef in m_elements.GetElements(ElementType.Javascript))
            {
                result.Add(jsRef.Fields[FieldDefinitions.Src]);
            }
            return result;
        }

        #endregion

        public void RunScript(IScript script)
        {
            RunScript(script, null, false);
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

            try
            {
                script.Execute(c);
                if (expectResult && c.ReturnValue is NoReturnValue) throw new Exception("Function did not return a value");
                return c.ReturnValue;
            }
            catch (Exception ex)
            {
                Print("Error running script: " + ex.Message);
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
            if (m_elements.ContainsKey(ElementType.Function, name))
            {
                return RunScript(m_elements.Get(ElementType.Function, name).Fields[FieldDefinitions.Script], parameters, expectResult);
            }
            else
            {
                Print(string.Format("Error - no such procedure '{0}'", name));
            }
            return null;
        }

        public Element Procedure(string name)
        {
            if (!m_elements.ContainsKey(ElementType.Function, name)) return null;
            return m_elements.Get(ElementType.Function, name);
        }

        internal Element GetObjectType(string name)
        {
            return m_elements.Get(ElementType.ObjectType, name);
        }

        public GameState State
        {
            get { return m_state; }
        }

        public Elements Elements
        {
            get { return m_elements; }
        }

        public void Save(string filename)
        {            
            string saveData = Save(SaveMode.SavedGame);
            System.IO.File.WriteAllText(filename, saveData);
        }

        public string Save(SaveMode mode)
        {
            return m_saver.Save(mode);
        }

        public static Type ConvertTypeNameToType(string name)
        {
            Type type;
            if (s_typeNamesToTypes.TryGetValue(name, out type))
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
            if (s_typesToTypeNames.TryGetValue(type, out name))
            {
                return name;
            }

            foreach (KeyValuePair<Type, string> kvp in s_typesToTypeNames)
            {
                if (kvp.Key.IsAssignableFrom(type))
                {
                    return kvp.Value;
                }
            }

            throw new ArgumentOutOfRangeException(string.Format("Unrecognised type '{0}'", type.ToString()));
        }

        public string GetExternalPath(string file)
        {
            return GetExternalPath(System.IO.Path.GetDirectoryName(Filename), file);
        }

        private string GetExternalPath(string current, string file)
        {
            string path;

            if (TryPath(current, file, out path)) return path;
            if (TryPath(Environment.CurrentDirectory, file, out path)) return path;
            if (m_libFolder != null && TryPath(m_libFolder, file, out path)) return path;
            if (System.Reflection.Assembly.GetEntryAssembly() != null)
            {
                if (TryPath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().CodeBase), file, out path)) return path;
            }
            throw new Exception(string.Format("Cannot find a file called '{0}' in current path or application path", file));
        }

        private bool TryPath(string path, string file, out string fullPath)
        {
            if (path.StartsWith("file:\\")) path = path.Substring(6);
            fullPath = System.IO.Path.Combine(path, file);
            return System.IO.File.Exists(fullPath);
        }

        internal void NotifyElementFieldUpdate(Element element, string attribute, object newValue, bool isUndo)
        {
            if (ElementFieldUpdated != null) ElementFieldUpdated(this, new ElementFieldUpdatedEventArgs(element, attribute, newValue, isUndo));
        }

        internal void NotifyElementRefreshed(Element element)
        {
            if (ElementRefreshed != null) ElementRefreshed(this, new ElementRefreshEventArgs(element));
        }

        public bool EditMode
        {
            get { return m_editMode; }
        }

        internal Functions.ExpressionOwner ExpressionOwner
        {
            get { return m_expressionOwner; }
        }

        private void ChangeThreadState(ThreadState newState)
        {
            m_threadState = newState;
            lock (m_threadLock)
            {
                Monitor.PulseAll(m_threadLock);
            }
        }

        private void WaitUntilFinishedWorking()
        {
            lock (m_threadLock)
            {
                while (m_threadState == ThreadState.Working)
                {
                    Monitor.Wait(m_threadLock);
                }
            }
        }

        private void DoInNewThreadAndWait(Action routine)
        {
            ChangeThreadState(ThreadState.Working);
            Thread newThread = new Thread(new ThreadStart(routine));
            newThread.Start();
            WaitUntilFinishedWorking();
        }

        void LogException(Exception ex)
        {
            if (LogError != null) LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }

        internal IPlayer PlayerUI
        {
            get { return m_playerUI; }
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

        public string OriginalFilename
        {
            get { return m_originalFilename; }
        }
    }
}
