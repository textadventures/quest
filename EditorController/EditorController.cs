using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public class EditorController : IDisposable
    {
        private const string k_commands = "_gameCommands";
        private const string k_verbs = "_gameVerbs";

        private WorldModel m_worldModel;
        private ScriptFactory m_scriptFactory;
        private AvailableFilters m_availableFilters;
        private FilterOptions m_filterOptions;
        private EditableScriptFactory m_editableScriptFactory;
        private Dictionary<string, IEditorDefinition> m_editorDefinitions = new Dictionary<string, IEditorDefinition>();
        private Dictionary<ElementType, TreeHeader> m_elementTreeStructure;
        private bool m_initialised = false;

        public delegate void VoidHandler();
        public event VoidHandler ClearTree;
        public event VoidHandler BeginTreeUpdate;
        public event VoidHandler EndTreeUpdate;

        public delegate void AddedNodeHandler(string key, string text, string parent, System.Drawing.Color? foreColor, System.Drawing.Color? backColor);
        public event AddedNodeHandler AddedNode;

        public delegate void ShowMessageHandler(string message);
        public event ShowMessageHandler ShowMessage;

        public event EventHandler<ElementUpdatedEventArgs> ElementUpdated;
        public event EventHandler<UpdateUndoListEventArgs> UndoListUpdated;
        public event EventHandler<UpdateUndoListEventArgs> RedoListUpdated;

        public class ElementUpdatedEventArgs : EventArgs
        {
            internal ElementUpdatedEventArgs(string element, string attribute, object newValue, bool isUndo)
            {
                Element = element;
                Attribute = attribute;
                NewValue = newValue;
                IsUndo = isUndo;
            }

            public string Element { get; private set; }
            public string Attribute { get; private set; }
            public object NewValue { get; private set; }
            public bool IsUndo { get; private set; }
        }

        public class UpdateUndoListEventArgs : EventArgs
        {
            internal UpdateUndoListEventArgs(IEnumerable<string> undoList)
            {
                UndoList = undoList;
            }

            public IEnumerable<string> UndoList { get; private set; }
        }

        private class TreeHeader
        {
            public string Key;
            public string Title;
        }

        public EditorController()
        {
            m_availableFilters = new AvailableFilters();
            m_availableFilters.Add("libraries", "Show Library Elements");

            m_filterOptions = new FilterOptions();
            // set default filters here
        }

        public void Initialise(string filename)
        {
            m_worldModel = new WorldModel(filename);
            m_scriptFactory = new ScriptFactory(m_worldModel);
            m_worldModel.ElementFieldUpdated += m_worldModel_ElementFieldUpdated;
            m_worldModel.UndoLogger.TransactionsUpdated += UndoLogger_TransactionsUpdated;

            bool ok = m_worldModel.InitialiseEdit();

            // need to initialise the EditableScriptFactory after we've loaded the game XML above,
            // as the editor definitions contain the "friendly" templates for script commands.
            m_editableScriptFactory = new EditableScriptFactory(this, m_scriptFactory, m_worldModel);

            m_initialised = true;

            foreach (Element e in m_worldModel.Elements.GetElements(ElementType.Editor))
            {
                EditorDefinition def = new EditorDefinition(m_worldModel, e);
                m_editorDefinitions.Add(def.AppliesTo, def);
            }

            if (ok)
            {
                UpdateTree();
            }
            else
            {
                string message = "Failed to load game due to the following errors:" + Environment.NewLine;
                foreach (string error in m_worldModel.Errors)
                {
                    message += "* " + error + Environment.NewLine;
                }
                ShowMessage(message);
            }
        }

        void UndoLogger_TransactionsUpdated(object sender, EventArgs e)
        {
            if (UndoListUpdated != null) UndoListUpdated(this, new UpdateUndoListEventArgs(m_worldModel.UndoLogger.UndoList()));
            if (RedoListUpdated != null) RedoListUpdated(this, new UpdateUndoListEventArgs(m_worldModel.UndoLogger.RedoList()));
        }

        void m_worldModel_ElementFieldUpdated(object sender, WorldModel.ElementFieldUpdatedEventArgs e)
        {
            if (m_initialised)
            {
                if (ElementUpdated != null) ElementUpdated(this, new ElementUpdatedEventArgs(e.Element.Name, e.Attribute, WrapValue(e.NewValue, e.Element, e.Attribute), e.IsUndo));
            }
        }

        private void InitialiseTreeStructure()
        {
            m_elementTreeStructure = new Dictionary<ElementType, TreeHeader>();
            AddTreeHeader(ElementType.Object, "_objects", "Objects", null);
            AddTreeHeader(ElementType.Function, "_functions", "Functions", null);
            AddTreeHeader(ElementType.Walkthrough, "_walkthrough", "Walkthrough", null);
            AddTreeHeader(null, "_advanced", "Advanced", null);
            AddTreeHeader(ElementType.IncludedLibrary, "_include", "Included Libraries", "_advanced");
            AddTreeHeader(ElementType.ImpliedType, "_implied", "Implied Types", "_advanced");
            AddTreeHeader(ElementType.Template, "_template", "Templates", "_advanced");
            AddTreeHeader(ElementType.DynamicTemplate, "_dynamictemplate", "Dynamic Templates", "_advanced");
            AddTreeHeader(ElementType.Delegate, "_delegate", "Delegates", "_advanced");
            AddTreeHeader(ElementType.ObjectType, "_objecttype", "Object Types", "_advanced");
            AddTreeHeader(ElementType.Editor, "_editor", "Editors", "_advanced");
        }

        private void AddTreeHeader(ElementType? type, string key, string title, string parent)
        {
            TreeHeader header = new TreeHeader();
            header.Key = key;
            header.Title = title;
            if (type != null)
            {
                m_elementTreeStructure.Add(type.Value, header);
            }
            AddedNode(key, title, parent, null, null);
        }

        private void UpdateTree()
        {
            BeginTreeUpdate();
            ClearTree();
            InitialiseTreeStructure();

            foreach (ElementType type in Enum.GetValues(typeof(ElementType)))
            {
                foreach (Element o in m_worldModel.Elements.GetElements(type))
                {
                    string parent = null;
                    System.Drawing.Color? foreColor = null;
                    if (o.Parent != null) parent = o.Parent.Name;

                    if (parent == null && o.ElemType == ElementType.Object)
                    {
                        switch (o.Type)
                        {
                            case ObjectType.Command:
                                if (o.Fields.GetAsType<bool>("isverb"))
                                {
                                    parent = k_verbs;
                                }
                                else
                                {
                                    parent = k_commands;
                                }
                                break;
                        }
                    }

                    if (parent == null)
                    {
                        parent = m_elementTreeStructure[o.ElemType].Key;
                    }

                    // TO DO: Colours should be an option, so we probably shouldn't
                    // even have a reference to System.Drawing in EditorController.

                    string text = o.Name;
                    bool display = true;
                    bool isLibrary = (o.MetaFields.GetAsType<bool>("library"));

                    if (isLibrary && !m_filterOptions.IsSet("libraries"))
                    {
                        display = false;
                    }

                    if (isLibrary)
                    {
                        foreColor = System.Drawing.Color.Gray;
                    }

                    if (display)
                    {
                        AddedNode(o.Name, text, parent, foreColor, null);

                        if (o.Name == "game")
                        {
                            AddedNode(k_verbs, "Verbs", "game", null, null);
                            AddedNode(k_commands, "Commands", "game", null, null);
                        }
                    }
                }
            }
            EndTreeUpdate();
        }

        public AvailableFilters AvailableFilters
        {
            get { return m_availableFilters; }
        }

        public void UpdateFilterOptions(FilterOptions options)
        {
            m_filterOptions = options;
            UpdateTree();
        }

        public string GetElementEditorName(string elementKey)
        {
            // elementKey is "game", "k1" (a command), "someobject", "myexit" etc.
            // we return the editor type name, e.g. "game", "command", "object", "exit".
            // Should have a method that returns a list of all available editor names, then the Editor
            // control can construct them when it loads.
            if (elementKey == "game") return "game";

            if (m_worldModel.Elements.ContainsKey(elementKey))
            {
                string type = m_worldModel.Elements.Get(elementKey).Fields.GetString("type");
                if (string.IsNullOrEmpty(type))
                {
                    type = m_worldModel.Elements.Get(elementKey).Fields.GetString("elementtype");
                }
                if (m_editorDefinitions.ContainsKey(type)) return type;
            }

            // TO DO: When all editors are implemented, raise an error here.
            return "default";
        }

        public IEnumerable<string> GetAllEditorNames()
        {
            return m_editorDefinitions.Keys;
        }

        public Dictionary<string, EditableScriptData> GetScriptEditorData()
        {
            return m_editableScriptFactory.ScriptData;
        }

        public IEnumerable<string> GetAllScriptEditorCategories()
        {
            return m_editableScriptFactory.GetCategories();
        }

        public IEditorDefinition GetEditorDefinition(string editorName)
        {
            return m_editorDefinitions[editorName];
        }

        public IEditorData GetEditorData(string elementKey)
        {
            if (!m_worldModel.Elements.ContainsKey(elementKey)) return null;
            return new EditorData(m_worldModel.Elements.Get(elementKey), this);
        }

        public IEditorData GetScriptEditorData(IEditableScript script)
        {
            switch (script.Type)
            {
                case ScriptType.Normal:
                    return new ScriptCommandEditorData(script);
                default:
                    throw new NotImplementedException();
            }
        }

        public string Save()
        {
            return m_worldModel.Save(SaveMode.Editor);
        }

        public string GameName
        {
            get { return m_worldModel.Game.Fields.GetString("gamename"); }
        }

        public void StartTransaction(string description)
        {
            m_worldModel.UndoLogger.StartTransaction(description);
        }

        public void EndTransaction()
        {
            m_worldModel.UndoLogger.EndTransaction();
        }

        public void Undo()
        {
            m_worldModel.UndoLogger.Undo();
        }

        public void Undo(int count)
        {
            for (int i = 0; i < count; i++)
            {
                m_worldModel.UndoLogger.Undo();
            }
        }

        public void Redo()
        {
            m_worldModel.UndoLogger.Redo();
        }

        public void Redo(int count)
        {
            for (int i = 0; i < count; i++)
            {
                m_worldModel.UndoLogger.Redo();
            }
        }

        public IEnumerable<string> GetUndoItems()
        {
            return m_worldModel.UndoLogger.UndoList();
        }

        internal EditableScriptFactory ScriptFactory
        {
            get { return m_editableScriptFactory; }
        }

        private class WrappedValueData
        {
            public object Value { get; set; }
            public Element Parent { get; set; }
            public string Attribute { get; set; }
        }

        private Dictionary<object, WrappedValueData> m_wrappedValues = new Dictionary<object, WrappedValueData>();

        internal object WrapValue(object value, Element parent, string attribute)
        {
            // need to wrap IScript in an IEditableScript
            if (value is IScript)
            {
                // cache the created IEditableScript for each IScript as we don't want to regenerate a new one each
                // time the same script is edited.

                WrappedValueData result;
                if (m_wrappedValues.TryGetValue(value, out result))
                {
                    System.Diagnostics.Debug.Assert(result.Parent == parent && result.Attribute == attribute, "Wrapped value has been moved - cached IEditableScript will be invalid");
                    return result.Value;
                }

                result = new WrappedValueData
                {
                    Value = new EditableScripts(this, (IScript)value, parent, attribute),
                    Parent = parent,
                    Attribute = attribute
                };

                m_wrappedValues.Add(value, result);

                return result.Value;
            }

            return value;
        }

        internal WorldModel WorldModel
        {
            get { return m_worldModel; }
        }

        public EditableScripts CreateNewEditableScripts(string parent, string attribute, string keyword)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} script to '{2}'", parent, attribute, keyword));
            Element element = (parent == null) ? null : m_worldModel.Elements.Get(parent);
            EditableScripts newValue = new EditableScripts(this, element, attribute);
            newValue.AddNewInternal(keyword, parent);
            if (element != null) element.Fields.Set(attribute, newValue.GetUnderlyingValue());
            WorldModel.UndoLogger.EndTransaction();

            return newValue;
        }

        public void Dispose()
        {
            m_worldModel.FinishGame();
        }
    }
}
