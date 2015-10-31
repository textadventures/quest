using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
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
        InvalidAttributeName,
        ExceptionOccurred,
        InvalidElementName,
        CircularTypeReference,
        InvalidElementNameMultipleSpaces,
        InvalidElementNameInvalidWord,
        CannotRenamePlayerElement,
        InvalidElementNameStartsWithNumber,
        MismatchingBrackets,
        MismatchingQuotes,
    }

    public enum EditorStyle
    {
        TextAdventure,
        GameBook
    }

    // TO DO: When WebEditor is fully functional, there should be no need for this
    public enum EditorMode
    {
        Desktop,
        Web
    }

    public struct ValidationResult
    {
        public bool Valid;
        public ValidationMessage Message;
        public string MessageData;
        public string SuggestedName;
    }

    public class TemplateData
    {
        public string TemplateName { get; set; }
        public string Filename { get; set; }
        public EditorStyle Type { get; set; }
    }

    public sealed class EditorController : IDisposable
    {
        private const string k_commands = "_gameCommands";
        private const string k_verbs = "_gameVerbs";

        private List<ElementType> m_ignoredTypes = new List<ElementType>
        {
            ElementType.ImpliedType,
            ElementType.Delegate,
            ElementType.Editor,
            ElementType.EditorTab,
            ElementType.EditorControl,
            ElementType.Resource
        };

        private List<ElementType> m_advancedTypes = new List<ElementType>
        {
            ElementType.DynamicTemplate,
            ElementType.Function,
            ElementType.IncludedLibrary,
            ElementType.Javascript,
            ElementType.ObjectType,
            ElementType.Template,
            ElementType.Timer,
            ElementType.Walkthrough
        };

        // TO DO: When WebEditor is fully functional, there should be no need for this
        private List<ElementType> m_webEditorIgnoreTypes = new List<ElementType>
        {
            ElementType.DynamicTemplate,
            ElementType.IncludedLibrary,
            ElementType.Javascript,
            ElementType.ObjectType,
            ElementType.Template,
            ElementType.Walkthrough
        };

        private static Dictionary<ValidationMessage, string> s_validationMessages = new Dictionary<ValidationMessage, string> {
            {ValidationMessage.OK, "No error"},
            {ValidationMessage.ItemAlreadyExists, "Item '{0}' already exists in the list"},
            {ValidationMessage.ElementAlreadyExists,"An element called '{0}' already exists in this game"},
            {ValidationMessage.InvalidAttributeName, "Invalid attribute name"},
            {ValidationMessage.ExceptionOccurred, "An error occurred: {1}"},
            {ValidationMessage.InvalidElementName, "Invalid element name"},
            {ValidationMessage.CircularTypeReference, "Circular type reference"},
            {ValidationMessage.InvalidElementNameMultipleSpaces, "Invalid element name. An element name cannot start or end with a space, and cannot contain multiple consecutive spaces."},
            {ValidationMessage.InvalidElementNameInvalidWord, "Invalid element name. Elements cannot contain these words: " + string.Join(", ", EditorController.ExpressionKeywords)},
            {ValidationMessage.CannotRenamePlayerElement, "The player object cannot be renamed"},
            {ValidationMessage.InvalidElementNameStartsWithNumber, "Invalid element name. An element name cannot start with a number."},
            {ValidationMessage.MismatchingBrackets, "The number of opening brackets \"(\" does not match the number of closing brackets \")\"."},
            {ValidationMessage.MismatchingQuotes, "Missing quote character (\")"},
        };

        private WorldModel m_worldModel;
        private ScriptFactory m_scriptFactory;
        private AvailableFilters m_availableFilters;
        private FilterOptions m_filterOptions;
        private EditableScriptFactory m_editableScriptFactory;
        private FontsManager m_fontsManager;
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
        private bool m_simpleMode;
        private EditorStyle m_editorStyle = EditorStyle.TextAdventure;
        private EditorMode m_editorMode = EditorMode.Desktop;

        public event EventHandler ClearTree;
        public event EventHandler BeginTreeUpdate;
        public event EventHandler EndTreeUpdate;

        public class AddedNodeEventArgs : EventArgs
        {
            public string Key { get; set; }
            public string Text { get; set; }
            public string Parent { get; set; }
            public bool IsLibraryNode { get; set; }
            public int? Position { get; set; }
        }

        public event EventHandler<AddedNodeEventArgs> AddedNode;

        public class RemovedNodeEventArgs : EventArgs
        {
            public string Key { get; set; }
        }

        public event EventHandler<RemovedNodeEventArgs> RemovedNode;

        public class RenamedNodeEventArgs : EventArgs
        {
            public string OldName { get; set; }
            public string NewName { get; set; }
        }

        public event EventHandler<RenamedNodeEventArgs> RenamedNode;

        public class RetitledNodeEventArgs : EventArgs
        {
            public string Key { get; set; }
            public string NewTitle { get; set; }
        }

        public event EventHandler<RetitledNodeEventArgs> RetitledNode;

        public class ShowMessageEventArgs : EventArgs
        {
            public string Message { get; set; }
        }

        public event EventHandler<ShowMessageEventArgs> ShowMessage;

        public class RequestAddElementEventArgs : EventArgs
        {
            public string ElementType { get; set; }
            public string ObjectType { get; set; }
            public string Filter { get; set; }
        }

        public event EventHandler<RequestAddElementEventArgs> RequestAddElement;

        public class RequestEditEventArgs : EventArgs
        {
            public string Key { get; set; }
        }

        public event EventHandler<RequestEditEventArgs> RequestEdit;

        public event EventHandler ElementsUpdated;

        public class ElementMovedEventArgs : EventArgs
        {
            public string Key { get; set; }
        }

        public event EventHandler<ElementMovedEventArgs> ElementMoved;

        public class ScriptClipboardUpdateEventArgs : EventArgs
        {
            public bool HasScript { get; set; }
        }

        public event EventHandler<ScriptClipboardUpdateEventArgs> ScriptClipboardUpdated;

        public class RequestRunWalkthroughEventArgs : EventArgs
        {
            public string Name { get; set; }
            public bool Record { get; set; }
        }

        public event EventHandler<RequestRunWalkthroughEventArgs> RequestRunWalkthrough;

        public event EventHandler SimpleModeChanged;

        public event EventHandler<ElementUpdatedEventArgs> ElementUpdated;
        public event EventHandler<ElementRefreshedEventArgs> ElementRefreshed;
        public event EventHandler<UpdateUndoListEventArgs> UndoListUpdated;
        public event EventHandler<UpdateUndoListEventArgs> RedoListUpdated;
        public event EventHandler<LoadStatusEventArgs> LoadStatus;
        public event EventHandler<LibrariesUpdatedEventArgs> LibrariesUpdated;

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

        public class LoadStatusEventArgs : EventArgs
        {
            public LoadStatusEventArgs(string status)
            {
                Status = status;
            }

            public string Status { get; private set; }
        }

        public class LibrariesUpdatedEventArgs : EventArgs
        {
            public LibrariesUpdatedEventArgs()
            {
            }
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

            m_fontsManager = new FontsManager();
        }

        public class InitialiseResults : EventArgs
        {
            internal InitialiseResults(bool success)
            {
                Success = success;
            }

            public bool Success { get; private set; }
        }

        public event EventHandler<InitialiseResults> InitialiseFinished;

        public void StartInitialise(string filename, string libFolder = null)
        {
            System.Threading.Thread newThread = new System.Threading.Thread(() =>
            {
                bool result = Initialise(filename, libFolder);
                if (InitialiseFinished != null) InitialiseFinished(this, new InitialiseResults(result));
            });
            newThread.Start();
        }

        public bool Initialise(string filename, string libFolder = null)
        {
            m_filename = filename;
            m_worldModel = new WorldModel(filename, libFolder, null);
            m_scriptFactory = new ScriptFactory(m_worldModel);
            m_worldModel.ElementFieldUpdated += m_worldModel_ElementFieldUpdated;
            m_worldModel.ElementRefreshed += m_worldModel_ElementRefreshed;
            m_worldModel.ElementMetaFieldUpdated += m_worldModel_ElementMetaFieldUpdated;
            m_worldModel.UndoLogger.TransactionsUpdated += UndoLogger_TransactionsUpdated;
            m_worldModel.Elements.ElementRenamed += Elements_ElementRenamed;
            m_worldModel.LoadStatus += m_worldModel_LoadStatus;

            bool ok = m_worldModel.InitialiseEdit();

            if (ok)
            {
                if (m_worldModel.Game.Fields.Get("_editorstyle") as string == "gamebook")
                {
                    m_editorStyle = EditorStyle.GameBook;
                    m_ignoredTypes.Add(ElementType.Template);
                    m_ignoredTypes.Add(ElementType.ObjectType);
                }

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

                if (m_worldModel.Version == WorldModelVersion.v500)
                {
                    m_worldModel.Elements.Get("game").Fields.Set("gameid", GetNewGameId());
                }
            }
            else
            {
                string message = "Failed to load game due to the following errors:" + Environment.NewLine;
                foreach (string error in m_worldModel.Errors)
                {
                    message += "* " + error + Environment.NewLine;
                }
                ShowMessage(this, new ShowMessageEventArgs { Message = message });
            }

            return ok;
        }

        void m_worldModel_LoadStatus(object sender, WorldModel.LoadStatusEventArgs e)
        {
            if (LoadStatus != null)
            {
                LoadStatus(this, new LoadStatusEventArgs(e.Status));
            }
        }

        void Elements_ElementRenamed(object sender, NameChangedEventArgs e)
        {
            string oldName = e.OldName;
            string newName = e.Element.Name;

            RenamedNode(this, new RenamedNodeEventArgs { OldName = oldName, NewName = newName });
            if (ElementsUpdated != null) ElementsUpdated(this, new EventArgs());
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
                BeginTreeUpdate(this, new EventArgs());
                RemoveElementAndSubElementsFromTree(e.Element);
                AddElementAndSubElementsToTree(e.Element);
                EndTreeUpdate(this, new EventArgs());
                if (ElementsUpdated != null) ElementsUpdated(this, new EventArgs());
            }

            if (e.Attribute == "anonymous" || e.Attribute == "alias"
                || e.Element.Type == ObjectType.Exit && (e.Attribute == "to" || e.Attribute == "name")
                || e.Element.Type == ObjectType.Command && (e.Attribute == "name" || e.Attribute == "pattern" || e.Attribute == "isverb")
                || e.Element.Type == ObjectType.TurnScript && (e.Attribute == "name")
                || e.Element.ElemType == ElementType.IncludedLibrary && (e.Attribute == "filename")
                || e.Element.ElemType == ElementType.Template && (e.Attribute == "templatename")
                || e.Element.ElemType == ElementType.Javascript && (e.Attribute == "src"))
            {
                if (e.Element.Name != null)
                {
                    // element name might be null if we're undoing an element add
                    RetitledNode(this, new RetitledNodeEventArgs { Key = e.Element.Name, NewTitle = GetDisplayName(e.Element) });
                    if (ElementsUpdated != null) ElementsUpdated(this, new EventArgs());
                }
            }

            if (e.Element.Type == ObjectType.Command && e.Attribute == "isverb")
            {
                MoveNove(e.Element.Name, GetDisplayName(e.Element), GetElementTreeParent(e.Element));
            }

            if (e.Element.ElemType == ElementType.IncludedLibrary && e.Attribute == "filename")
            {
                if (LibrariesUpdated != null) LibrariesUpdated(this, new LibrariesUpdatedEventArgs());
            }
        }

        void m_worldModel_ElementMetaFieldUpdated(object sender, WorldModel.ElementFieldUpdatedEventArgs e)
        {
            if (!m_initialised) return;

            //System.Diagnostics.Debug.Print("Updated: {0}.{1} = {2}", e.Element, e.Attribute, e.NewValue);

            if (e.Attribute == "sortindex")
            {
                RemovedNode(this, new RemovedNodeEventArgs { Key = e.Element.Name });
                AddElementAndSubElementsToTree(e.Element, GetElementPosition(e.Element));
                if (ElementMoved != null) ElementMoved(this, new ElementMovedEventArgs { Key = e.Element.Name });
            }

            if (e.Attribute == "library")
            {
                // Refresh the element in the tree by deleting and readding it
                RemovedNode(this, new RemovedNodeEventArgs { Key = e.Element.Name });
                AddElementAndSubElementsToTree(e.Element);
            }
        }

        private int GetElementPosition(Element e)
        {
            List<Element> siblings = new List<Element>(from Element child in m_worldModel.Elements.GetChildElements(e.Parent)
                                                       orderby child.MetaFields[MetaFieldDefinitions.SortIndex]
                                                       select child);
            return siblings.IndexOf(e);
        }

        private void MoveNove(string key, string text, string newParent)
        {
            RemovedNode(this, new RemovedNodeEventArgs { Key = key });
            AddedNode(this, new AddedNodeEventArgs { Key = key, Text = text, Parent = newParent, IsLibraryNode = false, Position = null });
        }

        private void AddElementAndSubElementsToTree(Element e, int? position = null)
        {
            AddElementToTree(e, position);
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
                RemovedNode(this, new RemovedNodeEventArgs { Key = key });
            }

            // finally remove the parent
            RemovedNode(this, new RemovedNodeEventArgs { Key = e.Name });
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
                AddElementToTree(addedElement, name: args.Added);
            }

            if (args.Removed != null)
            {
                RemovedNode(this, new RemovedNodeEventArgs { Key = args.Removed });
            }

            if (ElementsUpdated != null) ElementsUpdated(this, new EventArgs());
        }

        private void InitialiseTreeStructure()
        {
            m_treeTitles = new Dictionary<string, string> { { k_commands, "Commands" }, { k_verbs, "Verbs" } };
            m_elementTreeStructure = new Dictionary<ElementType, TreeHeader>();

            AddTreeHeader(EditorStyle.TextAdventure, ElementType.Object, "_objects", "Objects", null, false);
            AddTreeHeader(EditorStyle.GameBook, ElementType.Object, "_objects", "Pages", null, false);
            AddTreeHeader(null, ElementType.Function, "_functions", "Functions", null, false);

            AddTreeHeader(EditorStyle.TextAdventure, ElementType.Timer, "_timers", "Timers", null, false);
            if (m_editorMode == EditorMode.Desktop)
            {
                AddTreeHeader(EditorStyle.TextAdventure, ElementType.Walkthrough, "_walkthrough", "Walkthrough", null, false);
                AddTreeHeader(null, null, "_advanced", "Advanced", null, false);
                AddTreeHeader(null, ElementType.IncludedLibrary, "_include", "Included Libraries", "_advanced", false);
                AddTreeHeader(EditorStyle.TextAdventure, ElementType.Template, "_template", "Templates", "_advanced", false);
                AddTreeHeader(EditorStyle.TextAdventure, ElementType.DynamicTemplate, "_dynamictemplate", "Dynamic Templates", "_advanced", false);
                AddTreeHeader(EditorStyle.TextAdventure, ElementType.ObjectType, "_objecttype", "Object Types", "_advanced", false);
                AddTreeHeader(null, ElementType.Javascript, "_javascript", "Javascript", "_advanced", false);
            }
        }

        private void AddTreeHeader(EditorStyle? editorStyle, ElementType? type, string key, string title, string parent, bool simple)
        {
            if (editorStyle.HasValue && m_editorStyle != editorStyle) return; 
            if (simple || !SimpleMode)
            {
                m_treeTitles.Add(key, title);
                TreeHeader header = new TreeHeader {Key = key, Title = title};
                if (type != null)
                {
                    m_elementTreeStructure.Add(type.Value, header);
                }
                AddedNode(this, new AddedNodeEventArgs { Key = key, Text = title, Parent = parent, IsLibraryNode = false, Position = null });
            }
        }

        public void UpdateTree()
        {
            if (BeginTreeUpdate == null) return;

            BeginTreeUpdate(this, new EventArgs());
            ClearTree(this, new EventArgs());
            InitialiseTreeStructure();

            foreach (ElementType type in Enum.GetValues(typeof(ElementType)))
            {
                foreach (Element o in m_worldModel.Elements.GetElements(type).Where(e => e.Parent == null))
                {
                    AddElementAndChildrenToTree(o);
                }
            }
            EndTreeUpdate(this, new EventArgs());
        }

        private void AddElementAndChildrenToTree(Element o)
        {
            AddElementToTree(o);
            foreach (Element child in m_worldModel.Elements.GetDirectChildren(o))
            {
                AddElementAndChildrenToTree(child);
            }
        }

        // optional name parameter to prevent an exception when redoing object creation, as the
        // object will not have a name attribute immediately
        private void AddElementToTree(Element o, int? position = null, string name = null)
        {
            if (!IsElementVisible(o)) return;

            string parent = GetElementTreeParent(o);

            string text = GetDisplayName(o);
            bool display = true;
            bool isLibrary = (o.MetaFields.GetAsType<bool>("library"));

            if (isLibrary && !m_filterOptions.IsSet("libraries"))
            {
                display = false;
            }

            if (display)
            {
                string key = name ?? o.Name;
                AddedNode(this, new AddedNodeEventArgs { Key = key, Text = text, Parent = parent, IsLibraryNode = isLibrary, Position = position });

                if (o.Name == "game" && !SimpleMode && m_editorStyle == EditorStyle.TextAdventure)
                {
                    if (m_editorMode == EditorMode.Desktop)
                    {
                        // TO DO: When WebEditor is fully functional, there should be no need for this
                        AddedNode(this, new AddedNodeEventArgs { Key = k_verbs, Text = "Verbs", Parent = "game", IsLibraryNode = false, Position = null });
                        
                    }
                    AddedNode(this, new AddedNodeEventArgs { Key = k_commands, Text = "Commands", Parent = "game", IsLibraryNode = false, Position = null });
                }
            }
        }

        private bool IsElementVisible(Element e)
        {
            // Don't display implied types, editor elements etc.
            if (m_ignoredTypes.Contains(e.ElemType)) return false;
            if (SimpleMode && m_advancedTypes.Contains(e.ElemType)) return false;

            // TO DO: When WebEditor is fully functional, there should be no need for this
            if (m_editorMode == EditorMode.Web)
            {
                if (m_webEditorIgnoreTypes.Contains(e.ElemType)) return false;
                if (e.ElemType == ElementType.Object && e.Type == ObjectType.Command && e.Fields[FieldDefinitions.IsVerb])
                {
                    return false;
                }
            }

            if (SimpleMode)
            {
                if (e.ElemType == ElementType.Object && e.Type == ObjectType.Command)
                {
                    return false;
                }
            }
            if (e.ElemType == ElementType.Template)
            {
                // Don't display verb templates (if the user wants to edit a verb's regex,
                // they can do so directly on the verb itself).
                if (e.Fields[FieldDefinitions.IsVerb])
                {
                    return false;
                }

                // Don't display templates which have been overridden
                if (e.Fields[FieldDefinitions.TemplateName] != null && m_worldModel.TryGetTemplateElement(e.Fields[FieldDefinitions.TemplateName]) != e)
                {
                    return false;
                }
            }
            return true;
        }

        private string GetElementTreeParent(Element o)
        {
            if (SimpleMode)
            {
                return o.Parent == null ? null : o.Parent.Name;
            }

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
                                Boolean lookonly = e.Fields[FieldDefinitions.LookOnly];
                                if (lookonly)
                                {
                                    return "Look: " + e.Fields[FieldDefinitions.Alias];
                                }
                                else
                                {
                                    return "Exit: " + (to == null ? "(nowhere)" : to.Name);
                                }
                            case ObjectType.Command:
                                EditorCommandPattern pattern = e.Fields.GetAsType<EditorCommandPattern>("pattern");
                                bool isVerb = e.Fields.GetAsType<bool>("isverb");
                                return (isVerb ? "Verb" : "Command") + ": " + (pattern == null ? "(blank)" : pattern.Pattern);
                            case ObjectType.TurnScript:
                                return "Turn script";
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
            if (!m_worldModel.Elements.ContainsKey(element)) return null;
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

        public IEnumerable<string> GetAllScriptEditorCategories(bool showAll = false)
        {
            return m_editableScriptFactory.GetCategories(SimpleMode, showAll);
        }

        public IEditorDefinition GetEditorDefinition(IEditableScript script)
        {
            if (script.EditorName.StartsWith("(function)"))
            {
                // see if we have a specific editor definition for this function
                EditorDefinition result;
                if (m_editorDefinitions.TryGetValue(script.EditorName, out result))
                {
                    return result;
                }
                // if not, return the default function call editor definition, and reset
                // the EditorName for the script so it knows to get/set parameters via a
                // parameter dictionary instead of individually.
                script.EditorName = "()";
            }
            return m_editorDefinitions[script.EditorName];
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

        public EditableScripts CreateNewEditableScripts(string parent, string attribute, string keyword, bool useTransaction, bool nullKeywordIsFunctionCall = false)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} script to '{2}'", parent, attribute, keyword));
            }
            Element element = (parent == null) ? null : m_worldModel.Elements.Get(parent);
            EditableScripts newValue = EditableScripts.GetInstance(this, new MultiScript(m_worldModel));
            if (keyword != null || nullKeywordIsFunctionCall)
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

        public EditableScripts CreateNewEditableScriptsChild(ScriptCommandEditorData parent, string attribute, string keyword, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Add script '{0}'", keyword));
            }
            EditableScripts newValue = EditableScripts.GetInstance(this, new MultiScript(m_worldModel));
            if (keyword != null)
            {
                newValue.AddNewInternal(keyword);
            }
            parent.SetAttribute(attribute, newValue);
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

        public IEditableDictionary<string> CreateNewEditableStringDictionary(string parent, string attribute, string key, string item, bool useTransaction)
        {
            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to '{2}'", parent, attribute, item));
            }
            Element element = (parent == null) ? null : m_worldModel.Elements.Get(parent);

            QuestDictionary<string> newDictionary = new QuestDictionary<string>();

            if (key != null)
            {
                newDictionary.Add(key, item);
            }

            if (element != null)
            {
                element.Fields.Set(attribute, newDictionary);

                // setting an element field will clone the value, so we want to return the new dictionary
                newDictionary = element.Fields.GetAsType<QuestDictionary<string>>(attribute);
            }

            EditableDictionary<string> newValue = new EditableDictionary<string>(this, newDictionary);

            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }

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

        public object GetElementDataAttribute(string elementName, string attribute)
        {
            var element = WorldModel.Elements.Get(elementName);
            return element.Fields.Get(attribute);
        }

        public string GetObjectType(string element)
        {
            if (!WorldModel.Elements.ContainsKey(element)) return null;
            return WorldModel.GetTypeStringForObjectType(WorldModel.Elements.Get(element).Type);
        }

        private IEnumerable<Element> GetObjectNamesInternal(string objectType, string parent = null, bool includeAnonymous = false)
        {
            ObjectType t = WorldModel.GetObjectTypeForTypeString(objectType);
            var result = WorldModel.Elements.GetElements(ElementType.Object).Where(e => e.Type == t && e.Name != null);
            if (parent != null)
            {
                result = result.Where(e => e.Parent != null && e.Parent.Name == parent);
            }
            if (!includeAnonymous)
            {
                result = result.Where(e => !e.Fields[FieldDefinitions.Anonymous]);
            }
            return result.OrderBy(e => e.MetaFields[MetaFieldDefinitions.SortIndex]);
        }

        public IEnumerable<string> GetObjectNames(string objectType, string parent = null, bool includeAnonymous = false)
        {
            return GetObjectNamesInternal(objectType, parent, includeAnonymous).Select(e => e.Name);
        }

        public IEnumerable<string> GetObjectNames(string objectType, bool includeLibraryObjects, string parent = null, bool includeAnonymous = false)
        {
            if (includeLibraryObjects) return GetObjectNames(objectType, parent);
            return GetObjectNamesInternal(objectType, parent, includeAnonymous).Where(o => !o.MetaFields[MetaFieldDefinitions.Library]).Select(o => o.Name);
        }

        public IDictionary<string, string> GetVerbProperties()
        {
            // get all verbs

            var verbElements = WorldModel.Elements.GetElements(ElementType.Object)
                .Where(e =>
                    e.Type == ObjectType.Command
                    && e.Fields.GetAsType<bool>("isverb")
                    && e.Fields.GetString("property") != null);

            // return a dictionary where key=verb property, value=verb pattern (if verb has a simple pattern)
            // e.g. eat=eat; drink=drink; lookin=look in

            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (Element verb in verbElements)
            {
                string verbProperty = verb.Fields[FieldDefinitions.Property];
                object pattern = verb.Fields.Get(FieldDefinitions.Pattern.Property);
                EditorCommandPattern simplePattern = pattern as EditorCommandPattern;
                string displayName = (simplePattern != null) ? simplePattern.Pattern : verbProperty;
                result[verbProperty] = FriendlyVerbDisplayName(displayName);
            }

            return result;
        }

        private string FriendlyVerbDisplayName(string input)
        {
            string[] verbs = input.Split(new string[] { ";", "; " }, StringSplitOptions.None);
            string result = string.Empty;
            foreach (string verb in verbs)
            {
                string verbToAdd = verb.EndsWith(" #object#") ? verb.Substring(0, verb.Length - 9) : verb;
                if (result.Length > 0) result += "; ";
                result += verbToAdd.Trim();
            }
            return result;
        }

        public bool IsDefaultTypeName(string elementType)
        {
            return elementType == "defaultverb" || WorldModel.IsDefaultTypeName(elementType);
        }

        public ValidationResult AddInheritedTypeToElement(string elementName, string typeName, bool useTransaction)
        {
            Element element = m_worldModel.Elements.Get(elementName);
            Element type = m_worldModel.Elements.Get(ElementType.ObjectType, typeName);

            if (element.ElemType == ElementType.ObjectType &&
                (element == type || type.Fields.InheritsTypeRecursive(element)))
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.CircularTypeReference };
            }

            if (useTransaction)
            {
                WorldModel.UndoLogger.StartTransaction(string.Format("Add type '{0}' to '{1}'", typeName, elementName));
            }

            element.Fields.AddTypeUndoable(type);

            if (useTransaction)
            {
                WorldModel.UndoLogger.EndTransaction();
            }

            return new ValidationResult { Valid = true };
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

        public void CreateNewObject(string name, string parent, string alias)
        {
            m_worldModel.UndoLogger.StartTransaction(string.Format("Create object '{0}'", name));
            CreateNewObject(name, parent, "editor_object", alias);
            m_worldModel.UndoLogger.EndTransaction();
        }

        public void CreateNewRoom(string name, string parent, string alias)
        {
            m_worldModel.UndoLogger.StartTransaction(string.Format("Create room '{0}'", name));
            CreateNewObject(name, parent, "editor_room", alias);
            m_worldModel.UndoLogger.EndTransaction();
        }

        public string CreateNewExit(string parent)
        {
            return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit);
        }

        public string CreateNewExit(string parent, string to, string alias, string type, bool lookonly)
        {
            return CreateNewExitInternal(parent, to, alias, true, type, lookonly);
        }

        public string CreateNewExitInternal(string parent, string to, string alias, bool useTransaction, string type, bool lookonly)
        {
            if (to == null && lookonly)
            {
                return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit, new List<string> { type },
                new Dictionary<string, object> {
                    { "to", m_worldModel.Elements.Get(parent) },
                    { "alias", alias },
                    { "lookonly", lookonly },
                    { "look", ""}
                }, useTransaction);
            }
            else
            {
                return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit, new List<string> { type },
                new Dictionary<string, object> {
                    { "to", m_worldModel.Elements.Get(to) },
                    { "alias", alias }
                }, useTransaction);
            }
        }

        public string CreateNewExit(string parent, string to, string alias, string inverseAlias, string type, string inverseType, bool lookonly = false)
        {
            m_worldModel.UndoLogger.StartTransaction(string.Format("Create two-way exit {0} to {1}", parent, to));
            string result = CreateNewExitInternal(parent, to, alias, false, type, lookonly);
            CreateNewExitInternal(to, parent, inverseAlias, false, inverseType, lookonly);
            m_worldModel.UndoLogger.EndTransaction();

            return result;
        }

        public string CreateNewTurnScript(string parent)
        {
            return CreateNewAnonymousObject(parent, "turn script", ObjectType.TurnScript);
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

        private void CreateNewObject(string name, string parent, string editorType, string alias)
        {
            Element newObject = m_worldModel.GetElementFactory(ElementType.Object).Create(name);
            if (parent != null)
            {
                newObject.Parent = m_worldModel.Elements.Get(ElementType.Object, parent);
            }
            if (!string.IsNullOrEmpty(alias))
            {
                newObject.Fields[FieldDefinitions.Alias] = alias;
            }
            if (m_worldModel.Elements.ContainsKey(ElementType.ObjectType, editorType))
            {
                newObject.Fields.AddTypeUndoable(m_worldModel.Elements.Get(ElementType.ObjectType, editorType));
            }
        }

        private string CreateNewElement(ElementType type, string typeName, string elementName, string parent = null, IDictionary<string, object> initialFields = null)
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

            if (initialFields != null)
            {
                foreach (var field in initialFields)
                {
                    newElement.Fields.Set(field.Key, field.Value);
                }
            }

            m_worldModel.UndoLogger.EndTransaction();

            return newElement.Name;
        }

        public void CreateNewFunction(string name)
        {
            CreateNewElement(ElementType.Function, "function", name);
        }

        public void CreateNewTimer(string name)
        {
            CreateNewElement(ElementType.Timer, "timer", name, initialFields: new Dictionary<string, object> { { "interval", 1 } });
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

        public bool CanMoveElement(string elementKey)
        {
            if (!m_worldModel.Elements.ContainsKey(elementKey)) return false;
            Element element = m_worldModel.Elements.Get(elementKey);
            return (element.ElemType == ElementType.Object && element.Type != ObjectType.Game);
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

        public IEnumerable<string> GetMovePossibleParents(string elementKey)
        {
            if (!CanMoveElement(elementKey)) return null;

            Element element = m_worldModel.Elements.Get(elementKey);

            return from possibleParent in m_worldModel.Elements.GetElements(ElementType.Object)
                   where possibleParent != element
                   && possibleParent != element.Parent
                   && possibleParent.Type == ObjectType.Object
                   && !m_worldModel.ObjectContains(element, possibleParent)
                   orderby possibleParent.Name
                   select possibleParent.Name;
        }

        public ValidationResult CanAdd(string name)
        {
            if (m_worldModel.Elements.ContainsKey(name))
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.ElementAlreadyExists, SuggestedName = GetUniqueElementName(name)};
            }
            return ValidateElementName(name);
        }

        internal ValidationResult CanRename(Element element, string newName)
        {
            if (m_editorStyle == Quest.EditorStyle.GameBook && element.Name == "player")
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.CannotRenamePlayerElement };
            }
            if (m_worldModel.Elements.ContainsKey(newName) && m_worldModel.Elements.Get(newName) != element)
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.ElementAlreadyExists };
            }
            return ValidateElementName(newName);
        }

        private System.Text.RegularExpressions.Regex s_validNameRegex = new System.Text.RegularExpressions.Regex(@"^[\w ]+$");
        private System.Text.RegularExpressions.Regex s_startsWithNumberRegex = new System.Text.RegularExpressions.Regex(@"^\d");

        private ValidationResult ValidateElementName(string name)
        {
            if (string.IsNullOrEmpty(name) || !s_validNameRegex.IsMatch(name))
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.InvalidElementName };
            }

            if (name.StartsWith(" ") || name.EndsWith(" ") || name.Contains("  "))
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.InvalidElementNameMultipleSpaces };
            }

            if (s_startsWithNumberRegex.IsMatch(name))
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.InvalidElementNameStartsWithNumber };
            }

            string[] words = name.Split(' ');
            IList<string> keywords = Utility.ExpressionKeywords;
            foreach (string word in words)
            {
                if (keywords.Contains(word))
                {
                    return new ValidationResult { Valid = false, Message = ValidationMessage.InvalidElementNameInvalidWord };
                }
            }

            return new ValidationResult { Valid = true };
        }

        public static IList<string> ExpressionKeywords
        {
            get { return Utility.ExpressionKeywords; }
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
            attributeName = attributeName.Trim();
            return m_worldModel.Elements.GetElements(ElementType.Object).Any(
                e => e.Fields.GetAsType<bool>("isverb") && e.Fields.GetString("property") == attributeName);
        }

        public void UIRequestAddElement(string elementType, string objectType, string filter)
        {
            RequestAddElement(this, new RequestAddElementEventArgs { ElementType = elementType, ObjectType = objectType, Filter = filter });
        }

        public void UIRequestEditElement(string key)
        {
            RequestEdit(this, new RequestEditEventArgs { Key = key });
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
            string baseFolder = System.IO.Path.GetDirectoryName(m_worldModel.Filename);
            return m_worldModel.GetAvailableExternalFiles(searchPattern).Select(f => System.IO.Path.Combine(baseFolder, f));
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
                Element newElement;
                if (m_editorStyle == Quest.EditorStyle.TextAdventure)
                {
                    newElement = e.Clone();
                }
                else if (m_editorStyle == Quest.EditorStyle.GameBook)
                {
                    newElement = e.Clone(el => el.Name != "player");
                }
                else
                {
                    throw new NotImplementedException("Paste not implemented for this editor type");
                }
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
            if (m_editorStyle == Quest.EditorStyle.GameBook)
            {
                return null;
            }
            if (m_worldModel.Elements.ContainsKey(parentName))
            {
                Element e = m_worldModel.Elements.Get(parentName);

                // If the selected item is an object or walkthrough, it is valid to paste something
                // as a child of this element

                if (e.ElemType == ElementType.Walkthrough || (e.ElemType == ElementType.Object && e.Type == ObjectType.Object))
                {
                    // But don't paste a copy of an object inside itself

                    if (m_clipboardElements.Any(clipboardElement => clipboardElement == e))
                    {
                        return e.Parent;
                    }

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
            if (m_editorStyle == Quest.EditorStyle.GameBook && elementName == "player") return false;
            Element e = m_worldModel.Elements.Get(elementName);
            if (e.ElemType == ElementType.IncludedLibrary) return false;
            if (e.ElemType == ElementType.Javascript) return false;
            if (e.ElemType == ElementType.Template) return false;
            return true;
        }

        public bool CanDelete(string elementName)
        {
            if (!ElementExists(elementName)) return false;
            if (elementName == "game") return false;
            if (m_editorStyle == Quest.EditorStyle.GameBook && elementName == "player") return false;
            if (m_editorStyle == Quest.EditorStyle.GameBook && m_worldModel.ObjectContains(m_worldModel.Elements.Get(elementName), m_worldModel.Elements.Get("player"))) return false;
            return true;
        }

        public bool CanPasteScript()
        {
            return m_clipboardScripts != null && m_clipboardScripts.Count > 0;
        }

        internal void SetClipboardScript(IEnumerable<IScript> script)
        {
            m_clipboardScripts = new List<IScript>(script);
            if (ScriptClipboardUpdated != null)
            {
                ScriptClipboardUpdated(this, new ScriptClipboardUpdateEventArgs { HasScript = m_clipboardScripts.Count > 0 });
            }
        }

        internal IEnumerable<IScript> GetClipboardScript()
        {
            return m_clipboardScripts.AsReadOnly();
        }

        public IEnumerable<string> GetPropertyNames()
        {
            return m_worldModel.GetAllAttributeNames;
        }

        public IEnumerable<string> GetExpressionEditorNames(string expressionType)
        {
            return m_expressionDefinitions.Values.Where(d => d.ExpressionType == expressionType).Select(d => d.Description);
        }

        public string GetExpressionEditorDefinitionName(string expression, string expressionType)
        {
            return GetExpressionEditorDefinitionInternal(expression, expressionType).Description;
        }

        public IEditorDefinition GetExpressionEditorDefinition(string expression, string expressionType)
        {
            return GetExpressionEditorDefinitionInternal(expression, expressionType);
        }

        private EditorDefinition GetExpressionEditorDefinitionInternal(string expression, string expressionType)
        {
            // Get the Expression Editor Definition which matches the current expression.
            // e.g. if the expression is "(Got(myobject))", we want to return the Editor
            // Definition which corresponds to "(Got(#object#))" [note: this is turned into a
            // regex by the SimplePattern attribute loader]

            var candidates = from def in m_expressionDefinitions.Values
                             where def.ExpressionType == expressionType
                             where Utility.IsRegexMatch(def.Pattern, expression)
                             select def;

            if (!candidates.Any()) return null;

            var orderedCandidates = from def in candidates
                                    orderby Utility.GetMatchStrength(def.Pattern, expression) descending
                                    select def;

            return orderedCandidates.First();
        }

        public IEditorData GetExpressionEditorData(string expression, string expressionType, IEditorData parentData)
        {
            return new ExpressionTemplateEditorData(expression, GetExpressionEditorDefinitionInternal(expression, expressionType), parentData);
        }

        public string GetNewExpression(string templateName)
        {
            var definitions = from def in m_expressionDefinitions.Values
                              where def.Description == templateName
                              select def;

            EditorDefinition definition = definitions.First();

            return definition.Create;
        }

        public string GetExpression(IEditorData data, string changedAttribute, string changedValue)
        {
            ExpressionTemplateEditorData expressionData = (ExpressionTemplateEditorData)data;
            return expressionData.SaveExpression(changedAttribute, changedValue);
        }

        public string GetDisplayVerbPatternForAttribute(string attribute)
        {
            // If the user adds a verb like "look in", it will have an attribute name like "lookin".
            // Here we return the simple pattern for the corresponding verb, if it has one (library
            // verbs use strings to contain regexes so will return null here)

            var verbs = from element in m_worldModel.Elements.GetElements(ElementType.Object)
                        where element.Type == ObjectType.Command
                        where element.Fields[FieldDefinitions.IsVerb]
                        where element.Fields[FieldDefinitions.Property] == attribute
                        select element;

            Element result = verbs.FirstOrDefault();

            if (result == null) return null;
            object pattern = result.Fields.Get(FieldDefinitions.Pattern.Property);
            EditorCommandPattern simplePattern = pattern as EditorCommandPattern;

            if (simplePattern == null) return null;
            return FriendlyVerbDisplayName(simplePattern.Pattern);
        }

        public class PackageIncludeFile
        {
            public string Filename { get; set; }
            public System.IO.Stream Content { get; set; }
        }

        public ValidationResult Publish(string filename, bool includeWalkthrough, IEnumerable<PackageIncludeFile> includeFiles = null, System.IO.Stream outputStream = null)
        {
            string error;
            if (m_worldModel.CreatePackage(filename, includeWalkthrough, out error, includeFiles == null ? null : includeFiles.Select(f => new WorldModel.PackageIncludeFile
            {
                Filename = f.Filename,
                Content = f.Content,
            }), outputStream))
            {
                return new ValidationResult { Valid = true, Message = ValidationMessage.OK };
            }
            else
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.ExceptionOccurred, MessageData = error };
            }
        }

        public IEnumerable<string> GetBuiltInFunctionNames()
        {
            return m_worldModel.GetBuiltInFunctionNames();
        }

        public static Dictionary<string, TemplateData> GetAvailableTemplates(string folder = null)
        {
            Dictionary<string, TemplateData> templates = new Dictionary<string, TemplateData>();

            if (folder == null) folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().CodeBase);
            folder = TextAdventures.Utility.Utility.RemoveFileColonPrefix(folder);

            foreach (string file in System.IO.Directory.GetFiles(folder, "*.template", System.IO.SearchOption.AllDirectories))
            {
                string key = System.IO.Path.GetFileNameWithoutExtension(file);
                if (!templates.ContainsKey(key))
                {
                    AddTemplateData(templates, key, file);
                }
            }

            return templates;
        }

        private static void AddTemplateData(Dictionary<string, TemplateData> templates, string key, string filename)
        {
            try
            {
                string templateName = key;
                EditorStyle templateEditorStyle = EditorStyle.TextAdventure;

                System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(filename);
                xmlReader.Read();
                if (xmlReader.Name == "asl")
                {
                    string templateAttr = xmlReader.GetAttribute("template");
                    if (!string.IsNullOrEmpty(templateAttr)) templateName = templateAttr;

                    string templateType = xmlReader.GetAttribute("templatetype");
                    switch (templateType)
                    {
                        case "gamebook":
                            templateEditorStyle = EditorStyle.GameBook;
                            break;
                    }
                }

                templates.Add(templateName, new TemplateData
                {
                    Filename = filename,
                    TemplateName = templateName,
                    Type = templateEditorStyle
                });
            }
            catch
            {
                // ignore any templates which fail to load
            }
        }

        public static string CreateNewGameFile(string filename, string template, string gameName)
        {
            string templateText = System.IO.File.ReadAllText(template);
            string initialFileText = templateText
                .Replace("$NAME$", Utility.SafeXML(gameName))
                .Replace("$ID$", GetNewGameId())
                .Replace("$YEAR$", DateTime.Now.Year.ToString());

            return initialFileText;
        }

        public struct CanAddVerbResult
        {
            public bool CanAdd;
            public string ClashingCommand;
            public string ClashingCommandDisplay;
        }

        public CanAddVerbResult CanAddVerb(string verbPattern)
        {
            verbPattern = verbPattern.Trim();
            if (verbPattern == "ask")
            {
                return new CanAddVerbResult
                {
                    CanAdd = false,
                    ClashingCommand = "ask",
                    ClashingCommandDisplay = "ask"
                };
            }
            
            if (verbPattern == "tell")
            {
                return new CanAddVerbResult
                {
                    CanAdd = false,
                    ClashingCommand = "tell",
                    ClashingCommandDisplay = "tell"
                };
            }

            CanAddVerbResult result = new CanAddVerbResult();
            verbPattern += " object";

            // Now see if "verb object" is a match for the regex of an existing command in the game

            foreach (Element cmd in from e in m_worldModel.Objects
                                    where e.Type == ObjectType.Command
                                    where e.Parent == null
                                    where !e.Fields[FieldDefinitions.IsVerb]
                                    select e)
            {
                string regexPattern = null;

                object pattern = cmd.Fields.Get(FieldDefinitions.Pattern.Property);
                EditorCommandPattern editorCommandPattern = pattern as EditorCommandPattern;
                string stringPattern = pattern as string;

                if (editorCommandPattern != null)
                {
                    regexPattern = Utility.ConvertVerbSimplePattern(editorCommandPattern.Pattern, null);
                }
                else
                {
                    regexPattern = stringPattern;
                }

                if (regexPattern != null)
                {
                    bool isClash = false;
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern);
                    if (regex.IsMatch(verbPattern))
                    {
                        isClash = true;
                        IDictionary<string, string> parseResult = Utility.Populate(regexPattern, verbPattern);

                        // if verbPattern is "get in object", then it will match the regex for the "get" command -
                        // but we still want to allow this to be added, as it's not "really" a clash. So, if the
                        // potential clash only has one object group in its pattern, it's only a clash if the
                        // match is the entire string "object". In the case of "get in", we will have object="in object",
                        // so this can be allowed.

                        if (parseResult.Count == 1)
                        {
                            var kvp = parseResult.First();
                            if (kvp.Key.StartsWith("object") && kvp.Value != "object")
                            {
                                isClash = false;
                            }
                        }
                    }
                    if (isClash)
                    {
                        result.ClashingCommand = cmd.Name;
                        result.ClashingCommandDisplay = cmd.Name;
                        if (cmd.Fields[FieldDefinitions.Anonymous])
                        {
                            if (editorCommandPattern != null)
                            {
                                result.ClashingCommandDisplay = editorCommandPattern.Pattern;
                            }
                            else
                            {
                                result.ClashingCommandDisplay = stringPattern;
                            }
                        }
                        result.CanAdd = false;
                        return result;
                    }
                }
            }

            result.CanAdd = true;
            return result;
        }

        public void SwapElements(string key1, string key2)
        {
            Element a = m_worldModel.Elements.Get(key1);
            Element b = m_worldModel.Elements.Get(key2);

            int index = a.MetaFields[MetaFieldDefinitions.SortIndex];
            a.MetaFields[MetaFieldDefinitions.SortIndex] = b.MetaFields[MetaFieldDefinitions.SortIndex];
            b.MetaFields[MetaFieldDefinitions.SortIndex] = index;
        }

        public void BeginWalkthrough(string name, bool record)
        {
            RequestRunWalkthrough(this, new RequestRunWalkthroughEventArgs { Name = name, Record = record });
        }

        public void RecordWalkthrough(string name, IEnumerable<string> steps)
        {
            if (!steps.Any()) return;
            StartTransaction(string.Format("Add {0} walkthrough steps", steps.Count()));
            Element walkthrough = m_worldModel.Elements.Get(ElementType.Walkthrough, name);
            QuestList<string> newSteps = new QuestList<string>(steps);
            // TO DO: Use MergeLists
            walkthrough.Fields[FieldDefinitions.Steps] = walkthrough.Fields[FieldDefinitions.Steps] + newSteps;
            EndTransaction();
        }

        public void Uninitialise()
        {
            if (m_editorDefinitions != null) m_editorDefinitions.Clear();
            if (m_expressionDefinitions != null) m_expressionDefinitions.Clear();
            if (m_elementTreeStructure != null) m_elementTreeStructure.Clear();
            if (m_clipboardElements != null) m_clipboardElements.Clear();
            if (m_clipboardScripts != null) m_clipboardScripts.Clear();
            if (m_worldModel != null)
            {
                m_worldModel.ElementFieldUpdated -= m_worldModel_ElementFieldUpdated;
                m_worldModel.ElementRefreshed -= m_worldModel_ElementRefreshed;
                m_worldModel.ElementMetaFieldUpdated -= m_worldModel_ElementMetaFieldUpdated;
                m_worldModel.UndoLogger.TransactionsUpdated -= UndoLogger_TransactionsUpdated;
                m_worldModel.Elements.ElementRenamed -= Elements_ElementRenamed;
                m_worldModel.ObjectsUpdated -= m_worldModel_ObjectsUpdated;
            }
            EditableScripts.Clear();
            EditableDictionary<string>.Clear();
            EditableList<string>.Clear();
            EditableWrappedItemDictionary<IScript, IEditableScripts>.Clear();
        }

        public bool SimpleMode
        {
            get { return m_simpleMode; }
            set
            {
                if (m_simpleMode != value)
                {
                    m_simpleMode = value;
                    UpdateTree();
                    if (SimpleModeChanged != null) SimpleModeChanged(this, new EventArgs());
                }
            }
        }

        public static string GetNewGameId()
        {
            return Guid.NewGuid().ToString();
        }

        public EditorStyle EditorStyle
        {
            get { return m_editorStyle; }
        }

        public string GetUniqueElementName(string elementName)
        {
            return m_worldModel.GetUniqueElementName(elementName);
        }

        public string GetSelectedDropDownType(IEditorControl ctl, string element)
        {
            const string k_noType = "*";

            IDictionary<string, string> types = ctl.GetDictionary("types");
            List<string> inheritedTypes = new List<string>();

            // The inherited types look like:
            // *=default; typename1=Type 1; typename2=Type2

            // Find out which of the handled types are inherited by the object

            foreach (var item in types.Where(i => i.Key != k_noType))
            {
                if (DoesElementInheritType(element, item.Key))
                {
                    inheritedTypes.Add(item.Key);
                }
            }

            switch (inheritedTypes.Count)
            {
                case 0:
                    // Default - no types inherited
                    return k_noType;
                case 1:
                    return inheritedTypes[0];
                default:
                    return null;
            }
        }

        public string GetVerbAttributeForPattern(string selectedPattern)
        {
            IDictionary<string, string> availableVerbs = GetVerbProperties();

            var attributeForSelectedPattern = from verb in availableVerbs.Keys
                                              where availableVerbs[verb] == selectedPattern
                                              select verb;

            string selectedAttribute = attributeForSelectedPattern.FirstOrDefault();

            if (selectedAttribute == null)
            {
                // we couldn't find a matching verb property name, so see if there is a matching verb
                // pattern instead. For example, if the user typed "sit on" then we want to match
                // the "sit" verb, as "sit on" is one of its patterns.

                foreach (var verb in availableVerbs)
                {
                    List<string> patterns = new List<string>(verb.Value.Split(';').Select(p => p.Trim()));
                    if (patterns.Contains(selectedPattern))
                    {
                        selectedAttribute = verb.Key;
                        break;
                    }
                }
            }

            if (selectedAttribute == null)
            {
                // selectedPattern may be like "look in", "grab; snatch". We need to get a valid
                // attribute name from the pattern.

                int semicolonPos = selectedPattern.IndexOf(';');
                if (semicolonPos > -1)
                {
                    selectedAttribute = selectedPattern.Substring(0, semicolonPos);
                }
                else
                {
                    selectedAttribute = selectedPattern;
                }

                selectedAttribute = selectedAttribute.Replace(" ", "").Replace("#object#", "");
            }

            return selectedAttribute;
        }

        public EditorMode EditorMode
        {
            get { return m_editorMode; }
            set { m_editorMode = value; }
        }

        private static List<string> s_invalidChars = new List<string> { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };

        public static string GenerateSafeFilename(string gameName)
        {
            string result = gameName;
            foreach (string invalidChar in s_invalidChars)
            {
                result = result.Replace(invalidChar, "");
            }
            if (result.Length == 0) return string.Empty;
            return result;
        }

        internal void UpdateDictionariesReferencingRenamedObject(string oldName, string newName)
        {
            // This function is used so we can safely rename gamebook pages and have the corresponding links
            // be updated. Because these are stored as dictionary keys, they won't be updated automatically
            // as they don't point directly to the object. So we need to scan all objects with any "affectable"
            // string/script dictionaries (determined by their corresponding control having a source of "object",
            // and update keys if necessary

            EditorDefinition objectEditor = m_editorDefinitions["object"];
            foreach (IEditorTab tab in objectEditor.Tabs.Values)
            {
                foreach (IEditorControl ctl in tab.Controls.Where(c => c.GetString("source") == "object"))
                {
                    // So now we know that we need to scan all objects in the game which have a dictionary for
                    // ctl.Attribute, because the keys to this dictionary are object names.

                    foreach (Element element in m_worldModel.Elements.GetElements(ElementType.Object))
                    {
                        object value = element.Fields.Get(ctl.Attribute);
                        System.Collections.IDictionary dictionary = value as System.Collections.IDictionary;
                        if (dictionary != null && dictionary.Contains(oldName))
                        {
                            object wrappedValue = WrapValue(value);
                            IEditableDictionary<string> editableStringDictionary = wrappedValue as IEditableDictionary<string>;
                            IEditableDictionary<IEditableScripts> editableScriptDictionary = wrappedValue as IEditableDictionary<IEditableScripts>;
                            if (editableStringDictionary != null)
                            {
                                editableStringDictionary.ChangeKey(oldName, newName);
                            }
                            if (editableScriptDictionary != null)
                            {
                                editableScriptDictionary.ChangeKey(oldName, newName);
                            }
                        }
                    }
                }
            }
        }

        public ValidationResult ValidateExpression(string expression)
        {
            string obscured = String.Empty;
            try
            {
                obscured = Utility.ObscureStrings(expression);
            }
            catch (MismatchingQuotesException)
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.MismatchingQuotes };
            }

            int braceCount = 0;
            foreach (char c in obscured)
            {
                if (c == '(') braceCount++;
                if (c == ')') braceCount--;
            }

            if (braceCount != 0)
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.MismatchingBrackets };
            }
            
            return new ValidationResult { Valid = true };
        }

        public static string GetValidationError(ValidationResult result, object input)
        {
            return string.Format(s_validationMessages[result.Message], input, result.MessageData);
        }

        public List<string> AvailableBaseFonts()
        {
            return m_fontsManager.GetBaseFonts();
        }

        public List<string> AvailableWebFonts()
        {
            return m_fontsManager.GetWebFonts();
        }
    }
}
