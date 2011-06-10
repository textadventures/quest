using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public enum EditorUpdateSource
    {
        // These enum values should match those in WorldModel's UpdateSource enum. We don't want
        // to use the same enum here as the Editor component shouldn't access WorldModel directly.
        System,
        User
    }

    public enum ValidationMessage
    {
        OK,
        ItemAlreadyExists,
        ElementAlreadyExists,
    }

    public struct ValidationResult
    {
        public bool Valid;
        public ValidationMessage Message;
    }

    public class EditorController : IDisposable
    {
        private const string k_commands = "_gameCommands";
        private const string k_verbs = "_gameVerbs";

        private List<ElementType> m_ignoredTypes = new List<ElementType> {
            ElementType.ImpliedType,
            ElementType.Delegate,
            ElementType.Editor,
            ElementType.EditorTab,
            ElementType.EditorControl
        };

        private WorldModel m_worldModel;
        private ScriptFactory m_scriptFactory;
        private AvailableFilters m_availableFilters;
        private FilterOptions m_filterOptions;
        private EditableScriptFactory m_editableScriptFactory;
        private Dictionary<string, EditorDefinition> m_editorDefinitions = new Dictionary<string, EditorDefinition>();
        private Dictionary<string, EditorDefinition> m_expressionDefinitions = new Dictionary<string, EditorDefinition>();
        private Dictionary<ElementType, TreeHeader> m_elementTreeStructure;
        private Dictionary<string, string> m_treeTitles;
        private bool m_initialised = false;
        private Dictionary<string, Type> m_controlTypes = new Dictionary<string, Type>();
        private string m_filename;
        private List<Element> m_clipboardElements;
        private ElementType m_clipboardElementType;
        private List<IScript> m_clipboardScripts;

        public delegate void VoidHandler();
        public event VoidHandler ClearTree;
        public event VoidHandler BeginTreeUpdate;
        public event VoidHandler EndTreeUpdate;

        public delegate void AddedNodeHandler(string key, string text, string parent, System.Drawing.Color? foreColor, System.Drawing.Color? backColor);
        public event AddedNodeHandler AddedNode;

        public delegate void RemovedNodeHandler(string key);
        public event RemovedNodeHandler RemovedNode;

        public delegate void RenamedNodeHandler(string oldName, string newName);
        public event RenamedNodeHandler RenamedNode;

        public delegate void RetitledNodeHandler(string key, string newTitle);
        public event RetitledNodeHandler RetitledNode;

        public delegate void ShowMessageHandler(string message);
        public event ShowMessageHandler ShowMessage;

        public delegate void RequestAddElementHandler(string elementType, string objectType, string filter);
        public event RequestAddElementHandler RequestAddElement;

        public delegate void RequestEditHandler(string key);
        public event RequestEditHandler RequestEdit;

        public delegate void ElementsUpdatedHandler();
        public event ElementsUpdatedHandler ElementsUpdated;

        public event EventHandler<ElementUpdatedEventArgs> ElementUpdated;
        public event EventHandler<ElementRefreshedEventArgs> ElementRefreshed;
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

        public class ElementRefreshedEventArgs : EventArgs
        {
            internal ElementRefreshedEventArgs(string element)
            {
                Element = element;
            }

            public string Element { get; private set; }
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

        public bool Initialise(string filename)
        {
            m_filename = filename;
            m_worldModel = new WorldModel(filename, null);
            m_scriptFactory = new ScriptFactory(m_worldModel);
            m_worldModel.ElementFieldUpdated += m_worldModel_ElementFieldUpdated;
            m_worldModel.ElementRefreshed += m_worldModel_ElementRefreshed;
            m_worldModel.ElementMetaFieldUpdated += m_worldModel_ElementMetaFieldUpdated;
            m_worldModel.UndoLogger.TransactionsUpdated += UndoLogger_TransactionsUpdated;
            m_worldModel.Elements.ElementRenamed += Elements_ElementRenamed;

            bool ok = m_worldModel.InitialiseEdit();

            // need to initialise the EditableScriptFactory after we've loaded the game XML above,
            // as the editor definitions contain the "friendly" templates for script commands.
            m_editableScriptFactory = new EditableScriptFactory(this, m_scriptFactory, m_worldModel);

            m_initialised = true;

            m_worldModel.ObjectsUpdated += m_worldModel_ObjectsUpdated;

            foreach (Element e in m_worldModel.Elements.GetElements(ElementType.Editor))
            {
                EditorDefinition def = new EditorDefinition(m_worldModel, e);
                if (def.AppliesTo != null)
                {
                    // Normal editor definition for editing an element or a script command
                    m_editorDefinitions.Add(def.AppliesTo, def);
                }
                else if (def.Pattern != null)
                {
                    // Editor definition for an expression template in the "if" editor
                    m_expressionDefinitions.Add(def.Pattern, def);
                }
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

            return ok;
        }

        void Elements_ElementRenamed(object sender, NameChangedEventArgs e)
        {
            string oldName = e.OldName;
            string newName = e.Element.Name;

            RenamedNode(oldName, newName);
            if (ElementsUpdated != null) ElementsUpdated();
        }

        void UndoLogger_TransactionsUpdated(object sender, EventArgs e)
        {
            if (UndoListUpdated != null) UndoListUpdated(this, new UpdateUndoListEventArgs(m_worldModel.UndoLogger.UndoList()));
            if (RedoListUpdated != null) RedoListUpdated(this, new UpdateUndoListEventArgs(m_worldModel.UndoLogger.RedoList()));
        }

        void m_worldModel_ElementFieldUpdated(object sender, WorldModel.ElementFieldUpdatedEventArgs e)
        {
            if (!m_initialised) return;

            if (ElementUpdated != null) ElementUpdated(this, new ElementUpdatedEventArgs(e.Element.Name, e.Attribute, WrapValue(e.NewValue, e.Element, e.Attribute), e.IsUndo));

            if (e.Attribute == "parent")
            {
                BeginTreeUpdate();
                RemoveElementAndSubElementsFromTree(e.Element);
                AddElementAndSubElementsToTree(e.Element);
                EndTreeUpdate();
            }

            if (e.Attribute == "anonymous"
                || e.Element.Type == ObjectType.Exit && (e.Attribute == "to" || e.Attribute == "name")
                || e.Element.Type == ObjectType.Command && (e.Attribute == "name" || e.Attribute == "pattern" || e.Attribute == "isverb")
                || e.Element.ElemType == ElementType.IncludedLibrary && (e.Attribute == "filename")
                || e.Element.ElemType == ElementType.Template && (e.Attribute == "templatename")
                || e.Element.ElemType == ElementType.Javascript && (e.Attribute == "src"))
            {
                if (e.Element.Name != null)
                {
                    // element name might be null if we're undoing an element add
                    RetitledNode(e.Element.Name, GetDisplayName(e.Element));
                    if (ElementsUpdated != null) ElementsUpdated();
                }
            }

            if (e.Element.Type == ObjectType.Command && e.Attribute == "isverb")
            {
                MoveNove(e.Element.Name, GetDisplayName(e.Element), GetElementTreeParent(e.Element));
            }
        }

        void m_worldModel_ElementMetaFieldUpdated(object sender, WorldModel.ElementFieldUpdatedEventArgs e)
        {
            if (!m_initialised) return;

            if (e.Attribute == "library")
            {
                // Refresh the element in the tree by deleting and readding it
                RemovedNode(e.Element.Name);
                AddElementAndSubElementsToTree(e.Element);
            }
        }

        private void MoveNove(string key, string text, string newParent)
        {
            RemovedNode(key);
            AddedNode(key, text, newParent, null, null);
        }

        private void AddElementAndSubElementsToTree(Element e)
        {
            AddElementToTree(e);
            foreach (Element child in m_worldModel.Elements.GetChildElements(e))
            {
                AddElementToTree(child);
            }
        }

        private void RemoveElementAndSubElementsFromTree(Element e)
        {
            List<string> nodesToRemove = new List<string>(m_worldModel.Elements.GetChildElements(e).Select(child => child.Name));

            // reverse the list so we remove children before parents
            nodesToRemove.Reverse();

            foreach (string key in nodesToRemove)
            {
                RemovedNode(key);
            }

            // finally remove the parent
            RemovedNode(e.Name);
        }

        void m_worldModel_ElementRefreshed(object sender, WorldModel.ElementRefreshEventArgs e)
        {
            if (m_initialised)
            {
                if (ElementRefreshed != null) ElementRefreshed(this, new ElementRefreshedEventArgs(e.Element.Name));
            }
        }

        void m_worldModel_ObjectsUpdated(object sender, ObjectsUpdatedEventArgs args)
        {
            if (args.Added != null)
            {
                Element addedElement = m_worldModel.Elements.Get(args.Added);
                AddElementToTree(addedElement);
            }

            if (args.Removed != null)
            {
                RemovedNode(args.Removed);
            }

            if (ElementsUpdated != null) ElementsUpdated();
        }

        private void InitialiseTreeStructure()
        {
            m_treeTitles = new Dictionary<string, string> { { k_commands, "Commands" }, { k_verbs, "Verbs" } };
            m_elementTreeStructure = new Dictionary<ElementType, TreeHeader>();
            AddTreeHeader(ElementType.Object, "_objects", "Objects", null);
            AddTreeHeader(ElementType.Function, "_functions", "Functions", null);
            AddTreeHeader(ElementType.Walkthrough, "_walkthrough", "Walkthrough", null);
            AddTreeHeader(null, "_advanced", "Advanced", null);
            AddTreeHeader(ElementType.IncludedLibrary, "_include", "Included Libraries", "_advanced");
            // Ignore Implied Types - there's no reason for game authors to edit them
            //AddTreeHeader(ElementType.ImpliedType, "_implied", "Implied Types", "_advanced");
            AddTreeHeader(ElementType.Template, "_template", "Templates", "_advanced");
            AddTreeHeader(ElementType.DynamicTemplate, "_dynamictemplate", "Dynamic Templates", "_advanced");
            // Ignore Delegate elements - there's no reason for game authors to edit them (I think)
            //AddTreeHeader(ElementType.Delegate, "_delegate", "Delegates", "_advanced");
            AddTreeHeader(ElementType.ObjectType, "_objecttype", "Object Types", "_advanced");
            // Ignore Editor elements - there's no reason for game authors to edit them
            //AddTreeHeader(ElementType.Editor, "_editor", "Editors", "_advanced");
            AddTreeHeader(ElementType.Javascript, "_javascript", "Javascript", "_advanced");
        }

        private void AddTreeHeader(ElementType? type, string key, string title, string parent)
        {
            m_treeTitles.Add(key, title);
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
                    AddElementToTree(o);
                }
            }
            EndTreeUpdate();
        }

        private void AddElementToTree(Element o)
        {
            if (!IsElementVisible(o)) return;

            string parent = GetElementTreeParent(o);
            System.Drawing.Color? foreColor = null;

            // TO DO: Colours should be an option, so we probably shouldn't
            // even have a reference to System.Drawing in EditorController.

            string text = GetDisplayName(o);
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

        private bool IsElementVisible(Element e)
        {
            // Don't display implied types, editor elements etc.
            if (m_ignoredTypes.Contains(e.ElemType)) return false;
            if (e.ElemType == ElementType.Template)
            {
                // Don't display verb templates (if the user wants to edit a verb's regex,
                // they can do so directly on the verb itself).
                if (e.Fields[FieldDefinitions.IsVerb])
                {
                    return false;
                }

                // Don't display templates which have been overridden
                if (m_worldModel.TryGetTemplateElement(e.Fields[FieldDefinitions.TemplateName]) != e)
                {
                    return false;
                }
            }
            return true;
        }

        private string GetElementTreeParent(Element o)
        {
            if (o.Parent != null) return o.Parent.Name;

            if (o.ElemType == ElementType.Object && o.Type == ObjectType.Command)
            {
                return o.Fields.GetAsType<bool>("isverb") ? k_verbs : k_commands;
            }

            return m_elementTreeStructure[o.ElemType].Key;
        }

        private string GetDisplayName(Element e)
        {
            if (e.Fields[FieldDefinitions.Anonymous])
            {
                switch (e.ElemType)
                {
                    case ElementType.Object:
                        switch (e.Type)
                        {
                            case ObjectType.Exit:
                                Element to = e.Fields[FieldDefinitions.To];
                                return "Exit: " + (to == null ? "(nowhere)" : to.Name);
                            case ObjectType.Command:
                                EditorCommandPattern pattern = e.Fields.GetAsType<EditorCommandPattern>("pattern");
                                bool isVerb = e.Fields.GetAsType<bool>("isverb");
                                return (isVerb ? "Verb" : "Command") + ": " + (pattern == null ? "(blank)" : pattern.Pattern);
                        }
                        break;
                    case ElementType.Walkthrough:
                        return "Walkthrough";
                    case ElementType.IncludedLibrary:
                        string filename = e.Fields[FieldDefinitions.Filename];
                        if (!string.IsNullOrEmpty(filename)) return filename;
                        return "(filename not set)";
                    case ElementType.Template:
                        return e.Fields[FieldDefinitions.TemplateName];
                    case ElementType.Javascript:
                        string src = e.Fields[FieldDefinitions.Src];
                        if (!string.IsNullOrEmpty(src)) return src;
                        return "(filename not set)";
                }
            }
            return e.Name;
        }

        public string GetDisplayName(string element)
        {
            if (m_treeTitles.ContainsKey(element))
            {
                return m_treeTitles[element];
            }
            return GetDisplayName(m_worldModel.Elements.Get(element));
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

            if (m_worldModel.Elements.ContainsKey(elementKey))
            {
                Element e = m_worldModel.Elements.Get(elementKey);

                string type = null;

                if (e.ElemType == ElementType.Object)
                {
                    type = e.Fields.GetString("type");
                }
                if (string.IsNullOrEmpty(type))
                {
                    type = e.Fields.GetString("elementtype");
                }
                else
                {
                    if (type == "command")
                    {
                        if (e.Fields.GetAsType<bool>("isverb"))
                        {
                            type = "verb";
                        }
                    }
                }
                if (m_editorDefinitions.ContainsKey(type)) return type;
            }
            else if (m_editorDefinitions.ContainsKey(elementKey))
            {
                return elementKey;
            }

            return null;
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
                    return new ScriptCommandEditorData(this, script);
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

        internal object WrapValue(object value)
        {
            return WrapValue(value, null, null);
        }

        internal object WrapValue(object value, Element element, string attribute)
        {
            if (value is IScript)
            {
                return EditableScripts.GetInstance(this, (IScript)value);
            }

            if (value is QuestList<string>)
            {
                return EditableList<string>.GetInstance(this, (QuestList<string>)value);
            }

            if (value is QuestDictionary<string>)
            {
                return EditableDictionary<string>.GetInstance(this, (QuestDictionary<string>)value);
            }

            if (value is QuestDictionary<IScript>)
            {
                return EditableWrappedItemDictionary<IScript, IEditableScripts>.GetInstance(this, (QuestDictionary<IScript>)value);
            }

            if (value is Element)
            {
                if (element == null || attribute == null)
                {
                    throw new InvalidOperationException("Parent element and attribute must be specified to wrap object reference");
                }
                return new EditableObjectReference(this, (Element)value, element, attribute);
            }

            if (value is EditorCommandPattern)
            {
                if (element == null || attribute == null)
                {
                    throw new InvalidOperationException("Parent element and attribute must be specified to wrap command pattern");
                }
                return new EditableCommandPattern(this, (EditorCommandPattern)value, element, attribute);
            }

            return value;
        }

        internal WorldModel WorldModel
        {
            get { return m_worldModel; }
        }

        public EditableScripts CreateNewEditableScripts(string parent, string attribute, string keyword, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} script to '{2}'", parent, attribute, keyword));
            }
            Element element = (parent == null) ? null : m_worldModel.Elements.Get(parent);
            EditableScripts newValue = EditableScripts.GetInstance(this, new MultiScript());
            if (keyword != null)
            {
                newValue.AddNewInternal(keyword);
            }
            if (element != null && attribute != null)
            {
                element.Fields.Set(attribute, newValue.GetUnderlyingValue());
                // Setting the element field value will clone the IScript, so we need to return an updated reference
                newValue = EditableScripts.GetInstance(this, element.Fields.GetAsType<IScript>(attribute));
            }
            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }

            return newValue;
        }

        public IEditableList<string> CreateNewEditableList(string parent, string attribute, string item, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to '{2}'", parent, attribute, item));
            }
            Element element = (parent == null) ? null : m_worldModel.Elements.Get(parent);

            QuestList<string> newList = new QuestList<string>();

            if (item != null) newList.Add(item);

            if (element != null)
            {
                element.Fields.Set(attribute, newList);

                // setting an element field will clone the value, so we want to return the new list
                newList = element.Fields.GetAsType<QuestList<string>>(attribute);
            }

            EditableList<string> newValue = new EditableList<string>(this, newList);

            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }

            return newValue;
        }

        public IEditableDictionary<string> CreateNewEditableStringDictionary(string parent, string attribute, string key, string item)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to '{2}'", parent, attribute, item));
            Element element = (parent == null) ? null : m_worldModel.Elements.Get(parent);

            QuestDictionary<string> newDictionary = new QuestDictionary<string>();
            newDictionary.Add(key, item);

            if (element != null)
            {
                element.Fields.Set(attribute, newDictionary);

                // setting an element field will clone the value, so we want to return the new dictionary
                newDictionary = element.Fields.GetAsType<QuestDictionary<string>>(attribute);
            }

            EditableDictionary<string> newValue = new EditableDictionary<string>(this, newDictionary);
            WorldModel.UndoLogger.EndTransaction();

            return newValue;
        }

        public IEditableDictionary<IEditableScripts> CreateNewEditableScriptDictionary(string parent, string attribute, string key, IEditableScripts script, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to '{2}'", parent, attribute, script.DisplayString()));
            }

            Element element = (parent == null) ? null : m_worldModel.Elements.Get(parent);

            QuestDictionary<IScript> newDictionary = new QuestDictionary<IScript>();

            if (key != null)
            {
                newDictionary.Add(key, (IScript)script.GetUnderlyingValue());
            }

            if (element != null)
            {
                element.Fields.Set(attribute, newDictionary);

                // setting an element field will clone the value, so we want to return the new dictionary
                newDictionary = element.Fields.GetAsType<QuestDictionary<IScript>>(attribute);
            }

            IEditableDictionary<IEditableScripts> newValue = (IEditableDictionary<IEditableScripts>)WrapValue(newDictionary);

            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }

            return newValue;
        }

        public IEditableObjectReference CreateNewEditableObjectReference(string parent, string attribute, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to object", parent, attribute));
            }

            Element element = m_worldModel.Elements.Get(parent);

            if (attribute != "parent")
            {
                // Point to itself as a sensible default
                element.Fields.Set(attribute, element);
            }

            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }

            return new EditableObjectReference(this, element, element, attribute);
        }

        public IEditableCommandPattern CreateNewEditableCommandPattern(string parent, string attribute, string value, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to {2}", parent, attribute, value));
            }

            Element element = m_worldModel.Elements.Get(parent);
            EditorCommandPattern newPattern = new EditorCommandPattern(value);
            EditableCommandPattern newRef = new EditableCommandPattern(this, newPattern, element, attribute);
            element.Fields.Set(attribute, newPattern);

            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }

            return newRef;
        }

        public void Dispose()
        {
            m_worldModel.FinishGame();
        }

        public void AddControlType(string name, Type type)
        {
            m_controlTypes.Add(name, type);
        }

        public Type GetControlType(string name)
        {
            Type controlType;
            m_controlTypes.TryGetValue(name, out controlType);
            return controlType;
        }

        private IEnumerable<Element> GetElements(string elementType)
        {
            ElementType t = WorldModel.GetElementTypeForTypeString(elementType);
            return WorldModel.Elements.GetElements(t).Where(e => e.Name != null);
        }

        public IEnumerable<string> GetElementNames(string elementType)
        {
            return GetElements(elementType).Select(e => e.Name);
        }

        public IEnumerable<string> GetElementNames(string elementType, bool includeLibraryObjects)
        {
            if (includeLibraryObjects) return GetElementNames(elementType);
            return GetElements(elementType).Where(e => !e.MetaFields[MetaFieldDefinitions.Library]).Select(e => e.Name);
        }

        public string GetElementType(string element)
        {
            if (!WorldModel.Elements.ContainsKey(element)) return null;
            return WorldModel.GetTypeStringForElementType(WorldModel.Elements.Get(element).ElemType);
        }

        public string GetObjectType(string element)
        {
            if (!WorldModel.Elements.ContainsKey(element)) return null;
            return WorldModel.GetTypeStringForObjectType(WorldModel.Elements.Get(element).Type);
        }

        private IEnumerable<Element> GetObjects(string objectType)
        {
            ObjectType t = WorldModel.GetObjectTypeForTypeString(objectType);
            return WorldModel.Elements.GetElements(ElementType.Object).Where(e => e.Type == t && e.Name != null);
        }

        public IEnumerable<string> GetObjectNames(string objectType)
        {
            return GetObjects(objectType).Select(e => e.Name);
        }

        public IEnumerable<string> GetObjectNames(string objectType, bool includeLibraryObjects)
        {
            if (includeLibraryObjects) return GetObjectNames(objectType);
            return GetObjects(objectType).Where(o => !o.MetaFields[MetaFieldDefinitions.Library]).Select(o => o.Name);
        }

        public IEnumerable<string> GetVerbProperties()
        {
            return WorldModel.Elements.GetElements(ElementType.Object)
                .Where(e =>
                    e.Type == ObjectType.Command
                    && e.Fields.GetAsType<bool>("isverb")
                    && e.Fields.GetString("property") != null)
                .Select(e => e.Fields.GetString("property"));
        }

        public bool IsDefaultTypeName(string elementType)
        {
            return elementType == "defaultverb" || WorldModel.IsDefaultTypeName(elementType);
        }

        public void AddInheritedTypeToElement(string elementName, string typeName, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Add type '{0}' to '{1}'", typeName, elementName));
            }

            Element element = m_worldModel.Elements.Get(elementName);
            Element type = m_worldModel.Elements.Get(ElementType.ObjectType, typeName);
            element.Fields.AddTypeUndoable(type);

            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }
        }

        public void RemoveInheritedTypeFromElement(string elementName, string typeName, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Remove type '{0}' from '{1}'", typeName, elementName));
            }

            Element element = m_worldModel.Elements.Get(elementName);
            Element type = m_worldModel.Elements.Get(ElementType.ObjectType, typeName);
            element.Fields.RemoveTypeUndoable(type);

            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }
        }

        public bool DoesElementInheritType(string elementName, string typeName)
        {
            Element element = m_worldModel.Elements.Get(elementName);
            Element type = m_worldModel.Elements.Get(ElementType.ObjectType, typeName);
            return element.Fields.InheritsType(type);
        }

        public void CreateNewObject(string name, string parent)
        {
            m_worldModel.UndoLogger.StartTransaction(string.Format("Create object '{0}'", name));
            CreateNewObject(name, parent, "editor_object");
            m_worldModel.UndoLogger.EndTransaction();
        }

        public void CreateNewRoom(string name, string parent)
        {
            m_worldModel.UndoLogger.StartTransaction(string.Format("Create room '{0}'", name));
            CreateNewObject(name, parent, "editor_room");
            m_worldModel.UndoLogger.EndTransaction();
        }

        public string CreateNewExit(string parent)
        {
            return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit);
        }

        public string CreateNewCommand(string parent)
        {
            return CreateNewAnonymousObject(parent, "command", ObjectType.Command);
        }

        public string CreateNewVerb(string parent, bool useTransaction)
        {
            return CreateNewAnonymousObject(parent, "command", ObjectType.Command,
                new List<string> { "defaultverb" },
                new Dictionary<string, object> { { "isverb", true } },
                useTransaction);
        }

        private string CreateNewAnonymousObject(string parent, string typeName, ObjectType type, IList<string> initialTypes = null, IDictionary<string, object> initialFields = null, bool useTransaction = true)
        {
            string desc;
            Element parentEl;
            if (parent != null)
            {
                desc = string.Format("Create new {0} in '{1}'", typeName, parent);
                parentEl = m_worldModel.Elements.Get(ElementType.Object, parent);
            }
            else
            {
                desc = string.Format("Create new {0}", typeName);
                parentEl = null;
            }

            if (useTransaction) m_worldModel.UndoLogger.StartTransaction(desc);
            Element newObject = m_worldModel.ObjectFactory.CreateObject(type, initialTypes, initialFields);
            newObject.Parent = parentEl;
            newObject.Fields[FieldDefinitions.Anonymous] = true;

            if (useTransaction) m_worldModel.UndoLogger.EndTransaction();

            return newObject.Name;
        }

        private void CreateNewObject(string name, string parent, string editorType)
        {
            Element newObject = m_worldModel.GetElementFactory(ElementType.Object).Create(name);
            if (parent != null)
            {
                newObject.Parent = m_worldModel.Elements.Get(ElementType.Object, parent);
            }
            if (m_worldModel.Elements.ContainsKey(ElementType.ObjectType, editorType))
            {
                newObject.Fields.AddTypeUndoable(m_worldModel.Elements.Get(ElementType.ObjectType, editorType));
            }
        }

        private string CreateNewElement(ElementType type, string typeName, string elementName, string parent = null)
        {
            m_worldModel.UndoLogger.StartTransaction(string.Format("Create {0} '{1}'", typeName, elementName));
            Element newElement;

            if (elementName != null)
            {
                newElement = m_worldModel.GetElementFactory(type).Create(elementName);
            }
            else
            {
                newElement = m_worldModel.GetElementFactory(type).Create();
                newElement.Fields[FieldDefinitions.Anonymous] = true;
            }

            if (parent != null)
            {
                newElement.Parent = m_worldModel.Elements.Get(parent);
            }
            m_worldModel.UndoLogger.EndTransaction();

            return newElement.Name;
        }

        public void CreateNewFunction(string name)
        {
            CreateNewElement(ElementType.Function, "function", name);
        }

        public void CreateNewWalkthrough(string name, string parent)
        {
            CreateNewElement(ElementType.Walkthrough, "walkthrough", name, parent);
        }

        public string CreateNewIncludedLibrary()
        {
            return CreateNewElement(ElementType.IncludedLibrary, "included library", null);
        }

        public string CreateNewTemplate(string name)
        {
            m_worldModel.UndoLogger.StartTransaction(string.Format("Add new template '{0}'", name));
            Element newTemplate = m_worldModel.AddNewTemplate(name);
            newTemplate.Fields[FieldDefinitions.Anonymous] = true;
            m_worldModel.UndoLogger.EndTransaction();
            return newTemplate.Name;
        }

        public void CreateNewDynamicTemplate(string name)
        {
            CreateNewElement(ElementType.DynamicTemplate, "dynamic template", name);
        }

        public void CreateNewType(string name)
        {
            CreateNewElement(ElementType.ObjectType, "object type", name);
        }

        public string CreateNewJavascript()
        {
            return CreateNewElement(ElementType.Javascript, "javascript", null);
        }

        public bool CanMoveElement(string elementKey, string newParentKey)
        {
            if (elementKey == newParentKey) return false;
            if (!m_worldModel.Elements.ContainsKey(elementKey)) return false;
            if (newParentKey != "_objects" && !m_worldModel.Elements.ContainsKey(newParentKey)) return false;

            Element element = m_worldModel.Elements.Get(elementKey);
            if (element.ElemType == ElementType.Object && element.Type != ObjectType.Game)
            {
                if (newParentKey == "_objects")
                {
                    // Can always drag an object to the "Objects" header to unset its parent property
                    return true;
                }
                else
                {
                    Element newParent = m_worldModel.Elements.Get(newParentKey);

                    if (newParent.ElemType == ElementType.Object)
                    {
                        // Can't drag a parent object onto one of its own children
                        return !m_worldModel.ObjectContains(element, newParent);
                    }
                }

            }

            return false;
        }

        public void MoveElement(string elementKey, string newParentKey)
        {
            m_worldModel.UndoLogger.StartTransaction(string.Format("Move object '{0}' to '{1}'", elementKey, newParentKey));
            Element element = m_worldModel.Elements.Get(elementKey);
            Element newParent = newParentKey == "_objects" ? null : m_worldModel.Elements.Get(newParentKey);
            element.Parent = newParent;
            m_worldModel.UndoLogger.EndTransaction();
        }

        public IEnumerable<string> GetPossibleNewObjectParentsForCurrentSelection(string elementKey)
        {
            return GetPossibleNewParentsForCurrentSelection(elementKey, "object");
        }

        public IEnumerable<string> GetPossibleNewParentsForCurrentSelection(string elementKey, string elementTypeString)
        {
            // When an object is selected and the user clicks "Add object", we can either create the
            // object as a sub-object of the current selection, or as a sibling of the current object.
            // It could also be created with no parent at all (it's up to the GUI to provide that option).

            ElementType elementType = m_worldModel.GetElementTypeForTypeString(elementTypeString);

            if (elementKey == null) return null;
            if (!m_worldModel.Elements.ContainsKey(elementType, elementKey)) return null;
            Element currentSelection = m_worldModel.Elements.Get(elementKey);

            // For the object element type, we can only have parents with object types of object or game
            if (elementType != ElementType.Object || currentSelection.Type == ObjectType.Object || currentSelection.Type == ObjectType.Game)
            {
                List<string> result = new List<string>();
                result.Add(elementKey);

                Element thisElement = currentSelection;
                while (thisElement.Parent != null)
                {
                    result.Add(thisElement.Parent.Name);
                    thisElement = thisElement.Parent;
                }

                // return the list with highest ancestor at the top
                result.Reverse();
                return result;
            }
            else
            {
                return null;
            }
        }

        public ValidationResult CanAdd(string name)
        {
            if (m_worldModel.Elements.ContainsKey(name))
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.ElementAlreadyExists };
            }
            return new ValidationResult { Valid = true };
        }

        public ValidationResult CanAddTemplate(string name)
        {
            Element existingTemplate = m_worldModel.TryGetTemplateElement(name);
            bool canAdd;

            // Can always add a template if one doesn't exist with that name already. But if one does exist with that
            // name, we can only add it if it's overriding an existing template specified in a library.
            if (existingTemplate == null)
            {
                canAdd = true;
            }
            else
            {
                canAdd = existingTemplate.MetaFields[MetaFieldDefinitions.Library];
            }

            if (!canAdd)
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.ElementAlreadyExists };
            }
            return new ValidationResult { Valid = true };
        }

        public void DeleteElement(string elementKey, bool useTransaction)
        {
            if (useTransaction) m_worldModel.UndoLogger.StartTransaction(string.Format("Delete '{0}'", elementKey));
            Element element = m_worldModel.Elements.Get(elementKey);
            m_worldModel.GetElementFactory(element.ElemType).DestroyElement(element.Name);
            if (useTransaction) m_worldModel.UndoLogger.EndTransaction();
        }

        public bool IsVerbAttribute(string attributeName)
        {
            return m_worldModel.Elements.GetElements(ElementType.Object).Any(
                e => e.Fields.GetAsType<bool>("isverb") && e.Fields.GetString("property") == attributeName);
        }

        public void UIRequestAddElement(string elementType, string objectType, string filter)
        {
            RequestAddElement(elementType, objectType, filter);
        }

        public void UIRequestEditElement(string key)
        {
            RequestEdit(key);
        }

        public bool ElementExists(string elementKey)
        {
            return m_worldModel.Elements.ContainsKey(elementKey);
        }

        public bool ElementIsVerb(string elementKey)
        {
            return m_worldModel.Elements.Get(ElementType.Object, elementKey).Fields.GetAsType<bool>("isverb");
        }

        public IEnumerable<string> GetAvailableLibraries()
        {
            return m_worldModel.GetAvailableLibraries();
        }

        public IEnumerable<string> GetAvailableExternalFiles(string searchPattern)
        {
            return m_worldModel.GetAvailableExternalFiles(searchPattern);
        }

        public string Filename
        {
            get { return m_filename; }
            set { m_filename = value; }
        }

        public void CopyElements(IEnumerable<string> elementNames)
        {
            m_clipboardElements = (from name in elementNames select m_worldModel.Elements.Get(name)).ToList();

            bool first = true;

            foreach (Element e in m_clipboardElements)
            {
                if (first)
                {
                    m_clipboardElementType = e.ElemType;
                    first = false;
                }
                else
                {
                    if (m_clipboardElementType != e.ElemType)
                    {
                        throw new InvalidOperationException("Cannot mix element types in the clipboard");
                    }
                }
            }
        }

        public string PasteElements(string parentName)
        {
            if (!CanPaste(parentName)) return null;

            string lastPastedElement = null;

            Element parent = GetPasteParent(parentName);

            m_worldModel.UndoLogger.StartTransaction("Paste");

            foreach (Element e in m_clipboardElements)
            {
                Element newElement = e.Clone();
                newElement.Parent = parent;
                lastPastedElement = newElement.Name;
            }

            m_worldModel.UndoLogger.EndTransaction();

            return lastPastedElement;
        }

        public void CutElements(IEnumerable<string> elementNames)
        {
            m_worldModel.UndoLogger.StartTransaction("Cut");
            CopyElements(elementNames);
            foreach (string name in elementNames)
            {
                DeleteElement(name, false);
            }
            m_worldModel.UndoLogger.EndTransaction();
        }

        public bool CanPaste(string parentName)
        {
            if (m_clipboardElements == null || m_clipboardElements.Count == 0) return false;
            Element parent = GetPasteParent(parentName);
            if (parent == null) return true;
            if (parent.MetaFields[MetaFieldDefinitions.Library]) return false;
            return (parent.ElemType == m_clipboardElementType);
        }

        private Element GetPasteParent(string parentName)
        {
            if (m_worldModel.Elements.ContainsKey(parentName))
            {
                Element e = m_worldModel.Elements.Get(parentName);

                // If the selected item is an object or walkthrough, it is valid to paste something
                // as a child of this element

                if (e.ElemType == ElementType.Walkthrough || (e.ElemType == ElementType.Object && e.Type == ObjectType.Object))
                {
                    return e;
                }
                return null;
            }
            return null;
        }

        public bool CanCopy(string elementName)
        {
            if (!ElementExists(elementName)) return false;
            if (elementName == "game") return false;
            Element e = m_worldModel.Elements.Get(elementName);
            if (e.ElemType == ElementType.IncludedLibrary) return false;
            if (e.ElemType == ElementType.Javascript) return false;
            if (e.ElemType == ElementType.Template) return false;
            return true;
        }

        public bool CanPasteScript()
        {
            return m_clipboardScripts != null && m_clipboardScripts.Count > 0;
        }

        internal void SetClipboardScript(IEnumerable<IScript> script)
        {
            m_clipboardScripts = new List<IScript>(script);
        }

        internal IEnumerable<IScript> GetClipboardScript()
        {
            return m_clipboardScripts.AsReadOnly();
        }

        public IEnumerable<string> GetPropertyNames()
        {
            return m_worldModel.GetAllAttributeNames;
        }

        public IEnumerable<string> GetExpressionEditorNames()
        {
            return m_expressionDefinitions.Values.Select(d => d.Description);
        }

        public string GetExpressionEditorDefinitionName(string expression)
        {
            return GetExpressionEditorDefinitionInternal(expression).Description;
        }

        public IEditorDefinition GetExpressionEditorDefinition(string expression)
        {
            return GetExpressionEditorDefinitionInternal(expression);
        }

        private EditorDefinition GetExpressionEditorDefinitionInternal(string expression)
        {
            // Get the Expression Editor Definition which matches the current expression.
            // e.g. if the expression is "(Got(myobject))", we want to return the Editor
            // Definition which corresponds to "(Got(#object#))" [note: this is turned into a
            // regex by the SimplePattern attribute loader]

            var candidates = from def in m_expressionDefinitions.Values
                             where Utility.IsRegexMatch(def.Pattern, expression)
                             select def;

            if (!candidates.Any()) return null;

            var orderedCandidates = from def in candidates
                                    orderby Utility.GetMatchStrength(def.Pattern, expression) descending
                                    select def;

            return orderedCandidates.First();
        }

        public IEditorData GetExpressionEditorData(string expression)
        {
            return new ExpressionTemplateEditorData(expression, GetExpressionEditorDefinitionInternal(expression));
        }
    }
}
