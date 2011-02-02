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

    public class WorldModel : IASL, IASLDebug
    {
        private Element m_game;
        private Element m_player;
        private Elements m_elements;
        private Dictionary<string, int> m_nextUniqueID = new Dictionary<string, int>();
        private Template m_template;
        private UndoLogger m_undoLogger;
        private string m_filename;
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

            foreach (KeyValuePair<string, Type> kvp in s_typeNamesToTypes)
            {
                s_typesToTypeNames.Add(kvp.Value, kvp.Key);
            }
        }

        public WorldModel(string filename)
        {
            m_expressionOwner = new Functions.ExpressionOwner(this);
            m_template = new Template(this);
            InitialiseElementFactories();
            m_objectFactory = (ObjectFactory)m_elementFactories[ElementType.Object];
            m_objectFactory.ObjectsUpdated += new ObjectsUpdatedHandler(m_objectFactory_ObjectsUpdated);

            InitialiseDebuggerObjectTypes();
            m_filename = filename;
            m_elements = new Elements();
            m_undoLogger = new UndoLogger(this);
            m_saver = new GameSaver(this);
            m_game = ObjectFactory.CreateObject("game", ObjectType.Game);
        }

        public WorldModel(string filename, string libFolder)
            : this(filename)
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
        }

        internal static Dictionary<ObjectType, string> DefaultTypeNames
        {
            get { return s_defaultTypeNames; }
        }

        void m_objectFactory_ObjectsUpdated()
        {
            if (ObjectsUpdated != null) ObjectsUpdated();
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

        internal ObjectFactory ObjectFactory
        {
            get { return m_objectFactory; }
        }

        internal IElementFactory GetElementFactory(ElementType t)
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

        internal void AddObject(string name, Element o)
        {
            m_elements.Add(ElementType.Object, name, o);
        }

        internal void RemoveObject(string name)
        {
            m_elements.Remove(ElementType.Object, name);
        }

        internal IEnumerable<Element> ElementTypes
        {
            get { return m_elements.GetElements(ElementType.ObjectType); }
        }

        internal void SetGameName(string name)
        {
            RaiseRequest(Request.GameName, name);
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
            loader.FilenameUpdated += new GameLoader.FilenameUpdatedHandler(loader_FilenameUpdated);
            m_state = GameState.Loading;
            bool success = loader.Load(m_filename);
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

        public string GetInterface()
        {
            return Interface;
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
        public event RequestHandler RequestRaised;
        public event UpdateListHandler UpdateList;
        public event FinishedHandler Finished;
        public event ObjectsUpdatedHandler ObjectsUpdated;
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
            if (RequestRaised != null) RequestRaised(request, data);
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

        public DebugData GetDebugData(string obj)
        {
            return m_elements.Get(ElementType.Object, obj).GetDebugData();
        }

        public void Tick()
        {
            // TO DO: This is for timers
        }

        public void FinishWait()
        {
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

        public void SetFont(string fontName)
        {
            RaiseRequest(Request.FontName, fontName);
        }

        public void SetFontSize(int fontSize)
        {
            RaiseRequest(Request.FontSize, fontSize.ToString());
        }

        internal string Interface
        {
            get { return m_elements.GetSingle(ElementType.Interface).Fields[FieldDefinitions.Interface]; }
            set
            {
                string HTMLfile = GetExternalPath(System.IO.Path.GetDirectoryName(m_filename), value);

                Element i;
                if (m_elements.Count(ElementType.Interface) == 0)
                {
                    i = m_elementFactories[ElementType.Interface].Create();
                }
                else
                {
                    i = m_elements.GetSingle(ElementType.Interface);
                }
                i.Fields[FieldDefinitions.Interface] = HTMLfile;
                i.Fields[FieldDefinitions.Filename] = value;
            }
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

        public string GetExternalPath(string current, string file)
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
    }
}
