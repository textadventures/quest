using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using QuestViva.Engine.Scripts;
using QuestViva.Engine.Types;

namespace QuestViva.EditorCore;

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
    MismatchingQuotes
}

public enum EditorStyle
{
    TextAdventure,
    GameBook
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
    public string ResourceName { get; set; }
    public EditorStyle Type { get; set; }
}

public sealed class EditorController : IDisposable
{
    private const string k_commands = "_gameCommands";
    private const string k_verbs = "_gameVerbs";

    private static readonly Dictionary<ValidationMessage, string> s_validationMessages = new()
    {
        {ValidationMessage.OK, "No error"},
        {ValidationMessage.ItemAlreadyExists, "Item '{0}' already exists in the list"},
        {ValidationMessage.ElementAlreadyExists, "An element called '{0}' already exists in this game"},
        {ValidationMessage.InvalidAttributeName, "Invalid attribute name"},
        {ValidationMessage.ExceptionOccurred, "An error occurred: {1}"},
        {ValidationMessage.InvalidElementName, "Invalid element name"},
        {ValidationMessage.CircularTypeReference, "Circular type reference"},
        {
            ValidationMessage.InvalidElementNameMultipleSpaces,
            "Invalid element name. An element name cannot start or end with a space, and cannot contain multiple consecutive spaces."
        },
        {
            ValidationMessage.InvalidElementNameInvalidWord,
            "Invalid element name. Elements cannot contain these words: " + string.Join(", ", ExpressionKeywords)
        },
        {ValidationMessage.CannotRenamePlayerElement, "The player object cannot be renamed"},
        {
            ValidationMessage.InvalidElementNameStartsWithNumber,
            "Invalid element name. An element name cannot start with a number."
        },
        {
            ValidationMessage.MismatchingBrackets,
            "The number of opening brackets \"(\" does not match the number of closing brackets \")\"."
        },
        {ValidationMessage.MismatchingQuotes, "Missing quote character (\")"}
    };

    private static readonly List<string> s_invalidChars = new() {"\\", "/", ":", "*", "?", "\"", "<", ">", "|"};

    private readonly List<ElementType> m_advancedTypes = new()
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

    private readonly Dictionary<string, Type> m_controlTypes = new();
    private readonly Dictionary<string, EditorDefinition> m_editorDefinitions = new();
    private readonly Dictionary<string, EditorDefinition> m_expressionDefinitions = new();

    private readonly List<ElementType> m_ignoredTypes = new()
    {
        ElementType.ImpliedType,
        ElementType.Delegate,
        ElementType.Editor,
        ElementType.EditorTab,
        ElementType.EditorControl,
        ElementType.Resource
    };

    private readonly Regex s_startsWithNumberRegex = new(@"^\d");

    private readonly Regex s_validNameRegex = new(@"^[\w ]+$");
    private List<Element> m_clipboardElements;
    private ElementType m_clipboardElementType;
    private List<IScript> m_clipboardScripts;
    private Dictionary<ElementType, TreeHeader> m_elementTreeStructure;
    private FilterOptions m_filterOptions;
    private FontsManager m_fontsManager;
    private bool m_initialised;
    private bool m_lastelementscutout;
    private ScriptFactory m_scriptFactory;
    private bool m_simpleMode;
    private Dictionary<string, string> m_treeTitles;

    public EditorController()
    {
        AvailableFilters = new AvailableFilters();
        AvailableFilters.Add("libraries", "Show Library Elements");
        // m_availableFilters.Add("libraries", L.T("EditorFilterShowLibraryElements"));

        m_filterOptions = new FilterOptions();
        // set default filters here

        // FontsManager is initialized lazily to avoid firing the Google Fonts network request
        // until fonts are actually needed (e.g. not during test runs).
    }

    public AvailableFilters AvailableFilters { get; }

    public string GameName => WorldModel.Game.Fields.GetString("gamename");

    internal EditableScriptFactory ScriptFactory { get; private set; }

    internal WorldModel WorldModel { get; private set; }

    public static IList<string> ExpressionKeywords => Engine.Utility.ExpressionKeywords;

    public string Filename { get; set; }

    public bool SimpleMode
    {
        get => m_simpleMode;
        set
        {
            if (m_simpleMode != value)
            {
                m_simpleMode = value;
                UpdateTree();
                if (SimpleModeChanged != null)
                {
                    SimpleModeChanged(this, new EventArgs());
                }
            }
        }
    }

    public EditorStyle EditorStyle { get; private set; } = EditorStyle.TextAdventure;

    public void Dispose()
    {
        WorldModel?.FinishGame();
    }

    public event EventHandler ClearTree;
    public event EventHandler BeginTreeUpdate;
    public event EventHandler EndTreeUpdate;

    public event EventHandler<AddedNodeEventArgs> AddedNode;

    public event EventHandler<RemovedNodeEventArgs> RemovedNode;

    public event EventHandler<RenamedNodeEventArgs> RenamedNode;

    public event EventHandler<RetitledNodeEventArgs> RetitledNode;

    public event EventHandler<ShowMessageEventArgs> ShowMessage;

    public event EventHandler<RequestAddElementEventArgs> RequestAddElement;

    public event EventHandler<RequestEditEventArgs> RequestEdit;

    public event EventHandler ElementsUpdated;

    public event EventHandler<ElementMovedEventArgs> ElementMoved;

    public event EventHandler<ScriptClipboardUpdateEventArgs> ScriptClipboardUpdated;

    public event EventHandler<RequestRunWalkthroughEventArgs> RequestRunWalkthrough;

    public event EventHandler SimpleModeChanged;

    public event EventHandler<ElementUpdatedEventArgs> ElementUpdated;
    public event EventHandler<ElementRefreshedEventArgs> ElementRefreshed;
    public event EventHandler<UpdateUndoListEventArgs> UndoListUpdated;
    public event EventHandler<UpdateUndoListEventArgs> RedoListUpdated;
    public event EventHandler Dirty;
    public event EventHandler<LoadStatusEventArgs> LoadStatus;
    public event EventHandler<LibrariesUpdatedEventArgs> LibrariesUpdated;

    // public event EventHandler<InitialiseResults> InitialiseFinished;

    // public void StartInitialise(string filename)
    // {
    //     var newThread = new System.Threading.Thread(async () =>
    //     {
    //         bool result = await Initialise(filename);
    //         if (InitialiseFinished != null) InitialiseFinished(this, new InitialiseResults(result));
    //     });
    //     newThread.Start();
    // }

    public async Task<bool> Initialise(IGameDataProvider gameDataProvider, bool partialInit = false)
    {
        m_lastelementscutout = false;
        var gameData = await gameDataProvider.GetData();
        Filename = gameData?.Filename ?? string.Empty;
        WorldModel = new WorldModel(gameData, null);
        m_scriptFactory = new ScriptFactory(WorldModel);
        WorldModel.ElementFieldUpdated += m_worldModel_ElementFieldUpdated;
        WorldModel.ElementRefreshed += m_worldModel_ElementRefreshed;
        WorldModel.ElementMetaFieldUpdated += m_worldModel_ElementMetaFieldUpdated;
        WorldModel.UndoLogger.TransactionsUpdated += UndoLogger_TransactionsUpdated;
        WorldModel.UndoLogger.TransactionCommitted += (_, _) => Dirty?.Invoke(this, EventArgs.Empty);
        WorldModel.Elements.ElementRenamed += Elements_ElementRenamed;
        WorldModel.LoadStatus += m_worldModel_LoadStatus;

        var ok = await WorldModel.InitialiseEdit();

        if (ok)
        {
            // TODO: Move this code to another initialisation method - it's not needed when running tests
            if (!partialInit)
            {
                if (WorldModel.IsGamebook)
                {
                    EditorStyle = EditorStyle.GameBook;
                    m_ignoredTypes.Add(ElementType.Template);
                    m_ignoredTypes.Add(ElementType.ObjectType);
                }

                // need to initialise the EditableScriptFactory after we've loaded the game XML above,
                // as the editor definitions contain the "friendly" templates for script commands.
                ScriptFactory = new EditableScriptFactory(this, m_scriptFactory, WorldModel);

                m_initialised = true;

                WorldModel.ObjectsUpdated += m_worldModel_ObjectsUpdated;

                foreach (var e in WorldModel.Elements.GetElements(ElementType.Editor))
                {
                    var def = new EditorDefinition(WorldModel, e);
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

                if (WorldModel.Version == WorldModelVersion.v500)
                {
                    WorldModel.Elements.Get("game").Fields.Set("gameid", GetNewGameId());
                }
            }
        }
        else
        {
            var message = "Failed to load game due to the following errors:" + Environment.NewLine;
            foreach (var error in WorldModel.Errors)
            {
                message += "* " + error + Environment.NewLine;
            }

            ShowMessage(this, new ShowMessageEventArgs {Message = message});
        }

        return ok;
    }

    private void m_worldModel_LoadStatus(object sender, Engine.LoadStatusEventArgs e)
    {
        if (LoadStatus != null)
        {
            LoadStatus(this, new LoadStatusEventArgs(e.Status));
        }
    }

    private void Elements_ElementRenamed(object sender, NameChangedEventArgs e)
    {
        var oldName = e.OldName;
        var newName = e.Element.Name;

        RenamedNode(this, new RenamedNodeEventArgs {OldName = oldName, NewName = newName});
        if (ElementsUpdated != null)
        {
            ElementsUpdated(this, EventArgs.Empty);
        }
    }

    private void UndoLogger_TransactionsUpdated(object sender, EventArgs e)
    {
        if (UndoListUpdated != null)
        {
            UndoListUpdated(this, new UpdateUndoListEventArgs(WorldModel.UndoLogger.UndoList()));
        }

        if (RedoListUpdated != null)
        {
            RedoListUpdated(this, new UpdateUndoListEventArgs(WorldModel.UndoLogger.RedoList()));
        }
    }

    private void m_worldModel_ElementFieldUpdated(object sender, ElementFieldUpdatedEventArgs e)
    {
        if (!m_initialised)
        {
            return;
        }

        if (ElementUpdated != null)
        {
            ElementUpdated(this,
                new ElementUpdatedEventArgs(e.Element.Name, e.Attribute, WrapValue(e.NewValue, e.Element, e.Attribute),
                    e.IsUndo));
        }

        if (e.Attribute == "parent")
        {
            BeginTreeUpdate(this, new EventArgs());
            RemoveElementAndSubElementsFromTree(e.Element);
            AddElementAndSubElementsToTree(e.Element);
            EndTreeUpdate(this, new EventArgs());
            if (ElementsUpdated != null)
            {
                ElementsUpdated(this, new EventArgs());
            }
        }

        if (e.Attribute == "anonymous" || e.Attribute == "alias"
                                       || (e.Element.Type == ObjectType.Exit &&
                                           (e.Attribute == "to" || e.Attribute == "name"))
                                       || (e.Element.Type == ObjectType.Command && (e.Attribute == "name" ||
                                           e.Attribute == "pattern" || e.Attribute == "isverb"))
                                       || (e.Element.Type == ObjectType.TurnScript && e.Attribute == "name")
                                       || (e.Element.ElemType == ElementType.IncludedLibrary &&
                                           e.Attribute == "filename")
                                       || (e.Element.ElemType == ElementType.Template && e.Attribute == "templatename")
                                       || (e.Element.ElemType == ElementType.Javascript && e.Attribute == "src"))
        {
            if (e.Element.Name != null)
            {
                // element name might be null if we're undoing an element add
                RetitledNode(this,
                    new RetitledNodeEventArgs {Key = e.Element.Name, NewTitle = GetDisplayName(e.Element)});
                if (ElementsUpdated != null)
                {
                    ElementsUpdated(this, EventArgs.Empty);
                }
            }
        }

        if (e.Element.Type == ObjectType.Command && e.Attribute == "isverb")
        {
            MoveNove(e.Element.Name, GetDisplayName(e.Element), GetElementTreeParent(e.Element));
        }

        if (e.Element.ElemType == ElementType.IncludedLibrary && e.Attribute == "filename")
        {
            if (LibrariesUpdated != null)
            {
                LibrariesUpdated(this, new LibrariesUpdatedEventArgs());
            }
        }
    }

    private void m_worldModel_ElementMetaFieldUpdated(object sender, ElementFieldUpdatedEventArgs e)
    {
        if (!m_initialised)
        {
            return;
        }

        //System.Diagnostics.Debug.Print("Updated: {0}.{1} = {2}", e.Element, e.Attribute, e.NewValue);

        if (e.Attribute == "sortindex")
        {
            RemovedNode(this, new RemovedNodeEventArgs {Key = e.Element.Name});
            AddElementAndSubElementsToTree(e.Element, GetElementPosition(e.Element));
            if (ElementMoved != null)
            {
                ElementMoved(this, new ElementMovedEventArgs {Key = e.Element.Name});
            }
        }

        if (e.Attribute == "library")
        {
            // Refresh the element in the tree by deleting and readding it
            RemovedNode(this, new RemovedNodeEventArgs {Key = e.Element.Name});
            AddElementAndSubElementsToTree(e.Element);
        }
    }

    private int GetElementPosition(Element e)
    {
        var siblings = new List<Element>(from Element child in WorldModel.Elements.GetChildElements(e.Parent)
            orderby child.MetaFields[MetaFieldDefinitions.SortIndex]
            select child);
        return siblings.IndexOf(e);
    }

    private void MoveNove(string key, string text, string newParent)
    {
        RemovedNode(this, new RemovedNodeEventArgs {Key = key});
        var movedElement = WorldModel.Elements.ContainsKey(key) ? WorldModel.Elements.Get(key) : null;
        AddedNode(this,
            new AddedNodeEventArgs
            {
                Key = key, Text = text, Parent = newParent, IsLibraryNode = false, Position = null,
                NodeIcon = movedElement != null ? GetNodeIcon(movedElement) : null
            });
    }

    private void AddElementAndSubElementsToTree(Element e, int? position = null)
    {
        AddElementToTree(e, position);
        foreach (var child in WorldModel.Elements.GetChildElements(e))
        {
            AddElementToTree(child);
        }
    }

    private void RemoveElementAndSubElementsFromTree(Element e)
    {
        var nodesToRemove = new List<string>(WorldModel.Elements.GetChildElements(e).Select(child => child.Name));

        // reverse the list so we remove children before parents
        nodesToRemove.Reverse();

        foreach (var key in nodesToRemove)
        {
            RemovedNode(this, new RemovedNodeEventArgs {Key = key});
        }

        // finally remove the parent
        RemovedNode(this, new RemovedNodeEventArgs {Key = e.Name});
    }

    private void m_worldModel_ElementRefreshed(object sender, ElementRefreshEventArgs e)
    {
        if (m_initialised)
        {
            if (ElementRefreshed != null)
            {
                ElementRefreshed(this, new ElementRefreshedEventArgs(e.Element.Name));
            }
        }
    }

    private void m_worldModel_ObjectsUpdated(object sender, ObjectsUpdatedEventArgs args)
    {
        if (args.Added != null)
        {
            var addedElement = WorldModel.Elements.Get(args.Added);
            AddElementToTree(addedElement, name: args.Added);
        }

        if (args.Removed != null)
        {
            RemovedNode(this, new RemovedNodeEventArgs {Key = args.Removed});
        }

        if (ElementsUpdated != null)
        {
            ElementsUpdated(this, new EventArgs());
        }
    }

    private void InitialiseTreeStructure()
    {
        m_treeTitles = new Dictionary<string, string> {{k_commands, "Commands"}, {k_verbs, "Verbs"}};
        m_elementTreeStructure = new Dictionary<ElementType, TreeHeader>();

        AddTreeHeader(EditorStyle.TextAdventure, ElementType.Object, "_objects", "Objects", null, false);
        AddTreeHeader(EditorStyle.GameBook, ElementType.Object, "_objects", "Pages", null, false);
        AddTreeHeader(null, ElementType.Function, "_functions", "Functions", null, false);

        AddTreeHeader(EditorStyle.TextAdventure, ElementType.Timer, "_timers", "Timers", null, false);
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.Walkthrough, "_walkthrough", "Walkthrough", null, false);
        AddTreeHeader(null, null, "_advanced", "Advanced", null, false);
        AddTreeHeader(null, ElementType.IncludedLibrary, "_include", "Included Libraries", "_advanced", false);
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.Template, "_template", "Templates", "_advanced", false);
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.DynamicTemplate, "_dynamictemplate", "Dynamic Templates",
            "_advanced", false);
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.ObjectType, "_objecttype", "Object Types", "_advanced",
            false);
        AddTreeHeader(null, ElementType.Javascript, "_javascript", "Javascript", "_advanced", false);
    }

    private void AddTreeHeader(EditorStyle? editorStyle, ElementType? type, string key, string title, string parent,
        bool simple)
    {
        if (editorStyle.HasValue && EditorStyle != editorStyle)
        {
            return;
        }

        if (simple || !SimpleMode)
        {
            m_treeTitles.Add(key, title);
            var header = new TreeHeader {Key = key, Title = title};
            if (type != null)
            {
                m_elementTreeStructure.Add(type.Value, header);
            }

            AddedNode(this,
                new AddedNodeEventArgs
                {
                    Key = key, Text = title, Parent = parent, IsLibraryNode = false, Position = null,
                    NodeIcon = GetTreeHeaderIcon(type, key)
                });
        }
    }

    public void UpdateTree()
    {
        if (BeginTreeUpdate == null)
        {
            return;
        }

        BeginTreeUpdate(this, new EventArgs());
        ClearTree(this, new EventArgs());
        InitialiseTreeStructure();

        foreach (ElementType type in Enum.GetValues<ElementType>())
        {
            foreach (var o in WorldModel.Elements.GetElements(type).Where(e => e.Parent == null))
            {
                AddElementAndChildrenToTree(o);
            }
        }

        EndTreeUpdate(this, new EventArgs());
    }

    private void AddElementAndChildrenToTree(Element o)
    {
        AddElementToTree(o);
        foreach (var child in WorldModel.Elements.GetDirectChildren(o))
        {
            AddElementAndChildrenToTree(child);
        }
    }

    // optional name parameter to prevent an exception when redoing object creation, as the
    // object will not have a name attribute immediately
    private void AddElementToTree(Element o, int? position = null, string name = null)
    {
        if (!IsElementVisible(o))
        {
            return;
        }

        var parent = GetElementTreeParent(o);

        var text = GetDisplayName(o);
        var display = true;
        var isLibrary = o.MetaFields.GetAsType<bool>("library");

        if (isLibrary && !m_filterOptions.IsSet("libraries"))
        {
            display = false;
        }

        if (display)
        {
            var key = name ?? o.Name;
            AddedNode(this,
                new AddedNodeEventArgs
                {
                    Key = key, Text = text, Parent = parent, IsLibraryNode = isLibrary, Position = position,
                    NodeIcon = GetNodeIcon(o)
                });

            if (o.Name == "game" && !SimpleMode && EditorStyle == EditorStyle.TextAdventure)
            {
                AddedNode(this,
                    new AddedNodeEventArgs
                    {
                        Key = k_verbs, Text = "Verbs", Parent = "game", IsLibraryNode = false, Position = null,
                        NodeIcon = "s_verb"
                    });
                AddedNode(this,
                    new AddedNodeEventArgs
                    {
                        Key = k_commands, Text = "Commands", Parent = "game", IsLibraryNode = false, Position = null,
                        NodeIcon = "s_command"
                    });
            }
        }
    }

    private bool IsElementVisible(Element e)
    {
        // Don't display implied types, editor elements etc.
        if (m_ignoredTypes.Contains(e.ElemType))
        {
            return false;
        }

        if (SimpleMode && m_advancedTypes.Contains(e.ElemType))
        {
            return false;
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
            if (e.Fields[FieldDefinitions.TemplateName] != null &&
                WorldModel.TryGetTemplateElement(e.Fields[FieldDefinitions.TemplateName]) != e)
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

        if (o.Parent != null)
        {
            return o.Parent.Name;
        }

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
                            var to = e.Fields[FieldDefinitions.To];
                            var lookonly = e.Fields[FieldDefinitions.LookOnly];
                            if (lookonly)
                            {
                                return "Look: " + e.Fields[FieldDefinitions.Alias];
                            }

                            return "Exit: " + (to == null ? "(nowhere)" : to.Name);
                        case ObjectType.Command:
                            var pattern = e.Fields.GetAsType<EditorCommandPattern>("pattern");
                            var isVerb = e.Fields.GetAsType<bool>("isverb");
                            return (isVerb ? "Verb" : "Command") + ": " +
                                   (pattern == null ? "(blank)" : pattern.Pattern);
                        case ObjectType.TurnScript:
                            return "Turn script";
                    }

                    break;
                case ElementType.Walkthrough:
                    return "Walkthrough";
                case ElementType.IncludedLibrary:
                    var filename = e.Fields[FieldDefinitions.Filename];
                    if (!string.IsNullOrEmpty(filename))
                    {
                        return filename;
                    }

                    return "(filename not set)";
                case ElementType.Template:
                    return e.Fields[FieldDefinitions.TemplateName];
                case ElementType.Javascript:
                    var src = e.Fields[FieldDefinitions.Src];
                    if (!string.IsNullOrEmpty(src))
                    {
                        return src;
                    }

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

        if (!WorldModel.Elements.ContainsKey(element))
        {
            return null;
        }

        return GetDisplayName(WorldModel.Elements.Get(element));
    }

    private string GetNodeIcon(Element o)
    {
        switch (o.ElemType)
        {
            case ElementType.Object:
                switch (o.Type)
                {
                    case ObjectType.Game: return "s_game";
                    case ObjectType.Exit: return "s_exit";
                    case ObjectType.Command:
                        return o.Fields.GetAsType<bool>("isverb") ? "s_verb" : "s_command";
                    case ObjectType.TurnScript: return "s_turn";
                    case ObjectType.Object:
                        if (EditorStyle == EditorStyle.GameBook)
                        {
                            return "s_add_page";
                        }

                        return o.Fields.GetAsType<bool>("isroom") ? "s_room" : "s_object";
                    default: return null;
                }
            case ElementType.Function: return "s_function";
            case ElementType.Timer: return "s_timer";
            case ElementType.Walkthrough: return "s_walk";
            case ElementType.IncludedLibrary: return "s_library";
            case ElementType.Template: return "s_template";
            case ElementType.DynamicTemplate: return "s_dynamictemplate";
            case ElementType.ObjectType: return "s_objecttype";
            case ElementType.Javascript: return "s_javascript";
            default: return null;
        }
    }

    private string GetTreeHeaderIcon(ElementType? type, string key)
    {
        if (type == null)
        {
            return "s_folder";
        }

        switch (type.Value)
        {
            case ElementType.Object: return "s_object";
            case ElementType.Function: return "s_function";
            case ElementType.Timer: return "s_timer";
            case ElementType.Walkthrough: return "s_walk";
            case ElementType.IncludedLibrary: return "s_library";
            case ElementType.Template: return "s_template";
            case ElementType.DynamicTemplate: return "s_dynamictemplate";
            case ElementType.ObjectType: return "s_objecttype";
            case ElementType.Javascript: return "s_javascript";
            default: return null;
        }
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

        if (WorldModel.Elements.ContainsKey(elementKey))
        {
            var e = WorldModel.Elements.Get(elementKey);

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

            if (m_editorDefinitions.ContainsKey(type))
            {
                return type;
            }
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
        return ScriptFactory.ScriptData;
    }

    public Task<IEnumerable<string>> GetAllScriptEditorCategories(bool showAll = false)
    {
        return ScriptFactory.GetCategories(SimpleMode, showAll);
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
        if (!WorldModel.Elements.ContainsKey(elementKey))
        {
            return null;
        }

        return new EditorData(WorldModel.Elements.Get(elementKey), this);
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
        return WorldModel.Save(SaveMode.Editor);
    }

    public void StartTransaction(string description)
    {
        WorldModel.UndoLogger.StartTransaction(description);
    }

    public void EndTransaction()
    {
        WorldModel.UndoLogger.EndTransaction();
    }

    public Task Undo()
    {
        return WorldModel.UndoLogger.Undo();
    }

    public async Task Undo(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await WorldModel.UndoLogger.Undo();
        }
    }

    public void Redo()
    {
        WorldModel.UndoLogger.Redo();
    }

    public void Redo(int count)
    {
        for (var i = 0; i < count; i++)
        {
            WorldModel.UndoLogger.Redo();
        }
    }

    public IEnumerable<string> GetUndoItems()
    {
        return WorldModel.UndoLogger.UndoList();
    }

    public IEnumerable<string> GetRedoItems()
    {
        return WorldModel.UndoLogger.RedoList();
    }

    internal object WrapValue(object value)
    {
        return WrapValue(value, null, null);
    }

    internal object WrapValue(object value, Element element, string attribute)
    {
        if (value is IScript)
        {
            return EditableScripts.GetInstance(this, (IScript) value);
        }

        if (value is QuestList<string>)
        {
            return EditableList<string>.GetInstance(this, (QuestList<string>) value);
        }

        if (value is QuestDictionary<string>)
        {
            return EditableDictionary<string>.GetInstance(this, (QuestDictionary<string>) value);
        }

        if (value is QuestDictionary<IScript>)
        {
            return EditableWrappedItemDictionary<IScript, IEditableScripts>.GetInstance(this,
                (QuestDictionary<IScript>) value);
        }

        if (value is Element)
        {
            if (element == null || attribute == null)
            {
                throw new InvalidOperationException(
                    "Parent element and attribute must be specified to wrap object reference");
            }

            return new EditableObjectReference(this, (Element) value, element, attribute);
        }

        if (value is EditorCommandPattern)
        {
            if (element == null || attribute == null)
            {
                throw new InvalidOperationException(
                    "Parent element and attribute must be specified to wrap command pattern");
            }

            return new EditableCommandPattern(this, (EditorCommandPattern) value, element, attribute);
        }

        return value;
    }

    public EditableScripts CreateNewEditableScripts(string parent, string attribute, string keyword,
        bool useTransaction, bool nullKeywordIsFunctionCall = false)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} script to '{2}'", parent, attribute,
                keyword));
        }

        var element = parent == null ? null : WorldModel.Elements.Get(parent);
        var newValue = EditableScripts.GetInstance(this, new MultiScript(WorldModel));
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

    public EditableScripts CreateNewEditableScriptsChild(ScriptCommandEditorData parent, string attribute,
        string keyword, bool useTransaction)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Add script '{0}'", keyword));
        }

        var newValue = EditableScripts.GetInstance(this, new MultiScript(WorldModel));
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

    public IEditableList<string> CreateNewEditableList(string parent, string attribute, string item,
        bool useTransaction)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to '{2}'", parent, attribute, item));
        }

        var element = parent == null ? null : WorldModel.Elements.Get(parent);

        var newList = new QuestList<string>();

        if (item != null)
        {
            newList.Add(item);
        }

        if (element != null)
        {
            element.Fields.Set(attribute, newList);

            // setting an element field will clone the value, so we want to return the new list
            newList = element.Fields.GetAsType<QuestList<string>>(attribute);
        }

        var newValue = new EditableList<string>(this, newList);

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }

        return newValue;
    }

    public IEditableDictionary<string> CreateNewEditableStringDictionary(string parent, string attribute, string key,
        string item, bool useTransaction)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to '{2}'", parent, attribute, item));
        }

        var element = parent == null ? null : WorldModel.Elements.Get(parent);

        var newDictionary = new QuestDictionary<string>();

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

        var newValue = new EditableDictionary<string>(this, newDictionary);

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }

        return newValue;
    }

    public IEditableDictionary<IEditableScripts> CreateNewEditableScriptDictionary(string parent, string attribute,
        string key, IEditableScripts script, bool useTransaction)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to '{2}'", parent, attribute,
                script.DisplayString()));
        }

        var element = parent == null ? null : WorldModel.Elements.Get(parent);

        var newDictionary = new QuestDictionary<IScript>();

        if (key != null)
        {
            newDictionary.Add(key, (IScript) script.GetUnderlyingValue());
        }

        if (element != null)
        {
            element.Fields.Set(attribute, newDictionary);

            // setting an element field will clone the value, so we want to return the new dictionary
            newDictionary = element.Fields.GetAsType<QuestDictionary<IScript>>(attribute);
        }

        var newValue = (IEditableDictionary<IEditableScripts>) WrapValue(newDictionary);

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }

        return newValue;
    }

    public void MakeScriptDictionaryEditable(string parent, string attribute)
    {
        var element = WorldModel.Elements.Get(parent);
        var existing = element.Fields.GetAsType<QuestDictionary<IScript>>(attribute);
        if (existing == null)
        {
            return;
        }

        WorldModel.UndoLogger.StartTransaction($"Copy {attribute} to {parent}");
        try
        {
            var newDict = new QuestDictionary<IScript>();
            foreach (var kvp in existing)
            {
                newDict.Add(kvp.Key, (IScript) kvp.Value.Clone());
            }

            element.Fields.Set(attribute, newDict);
        }
        finally
        {
            WorldModel.UndoLogger.EndTransaction();
        }
    }

    public IEditableObjectReference CreateNewEditableObjectReference(string parent, string attribute,
        bool useTransaction)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to object", parent, attribute));
        }

        var element = WorldModel.Elements.Get(parent);

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

    public IEditableCommandPattern CreateNewEditableCommandPattern(string parent, string attribute, string value,
        bool useTransaction)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Set '{0}' {1} to {2}", parent, attribute, value));
        }

        var element = WorldModel.Elements.Get(parent);
        var newPattern = new EditorCommandPattern(value);
        var newRef = new EditableCommandPattern(this, newPattern, element, attribute);
        element.Fields.Set(attribute, newPattern);

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }

        return newRef;
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
        var t = WorldModel.GetElementTypeForTypeString(elementType);
        return WorldModel.Elements.GetElements(t).Where(e => e.Name != null);
    }

    public IEnumerable<string> GetElementNames(string elementType)
    {
        return GetElements(elementType).Select(e => e.Name);
    }

    public IEnumerable<string> GetElementNames(string elementType, bool includeLibraryObjects)
    {
        if (includeLibraryObjects)
        {
            return GetElementNames(elementType);
        }

        return GetElements(elementType).Where(e => !e.MetaFields[MetaFieldDefinitions.Library]).Select(e => e.Name);
    }

    public string GetElementType(string element)
    {
        if (!WorldModel.Elements.ContainsKey(element))
        {
            return null;
        }

        return WorldModel.GetTypeStringForElementType(WorldModel.Elements.Get(element).ElemType);
    }

    public object GetElementDataAttribute(string elementName, string attribute)
    {
        var element = WorldModel.Elements.Get(elementName);
        return element.Fields.Get(attribute);
    }

    public string GetObjectType(string element)
    {
        if (!WorldModel.Elements.ContainsKey(element))
        {
            return null;
        }

        return WorldModel.GetTypeStringForObjectType(WorldModel.Elements.Get(element).Type);
    }

    private IEnumerable<Element> GetObjectNamesInternal(string objectType, string parent = null,
        bool includeAnonymous = false)
    {
        var t = WorldModel.GetObjectTypeForTypeString(objectType);
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

    public IEnumerable<string> GetObjectNames(string objectType, bool includeLibraryObjects, string parent = null,
        bool includeAnonymous = false)
    {
        if (includeLibraryObjects)
        {
            return GetObjectNames(objectType, parent);
        }

        return GetObjectNamesInternal(objectType, parent, includeAnonymous)
            .Where(o => !o.MetaFields[MetaFieldDefinitions.Library]).Select(o => o.Name);
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

        var result = new Dictionary<string, string>();
        foreach (var verb in verbElements)
        {
            var verbProperty = verb.Fields[FieldDefinitions.Property];
            var pattern = verb.Fields.Get(FieldDefinitions.Pattern.Property);
            var simplePattern = pattern as EditorCommandPattern;
            var displayName = simplePattern != null ? simplePattern.Pattern : verbProperty;
            result[verbProperty] = FriendlyVerbDisplayName(displayName);
        }

        return result;
    }

    private string FriendlyVerbDisplayName(string input)
    {
        var verbs = input.Split(new[] {";", "; "}, StringSplitOptions.None);
        var result = string.Empty;
        foreach (var verb in verbs)
        {
            var verbToAdd = verb.EndsWith(" #object#") ? verb.Substring(0, verb.Length - 9) : verb;
            if (result.Length > 0)
            {
                result += "; ";
            }

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
        var element = WorldModel.Elements.Get(elementName);
        var type = WorldModel.Elements.Get(ElementType.ObjectType, typeName);

        if (element.ElemType == ElementType.ObjectType &&
            (element == type || type.Fields.InheritsTypeRecursive(element)))
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.CircularTypeReference};
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

        return new ValidationResult {Valid = true};
    }

    public void RemoveInheritedTypeFromElement(string elementName, string typeName, bool useTransaction)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Remove type '{0}' from '{1}'", typeName,
                elementName));
        }

        var element = WorldModel.Elements.Get(elementName);
        var type = WorldModel.Elements.Get(ElementType.ObjectType, typeName);
        element.Fields.RemoveTypeUndoable(type);

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }
    }

    public bool DoesElementInheritType(string elementName, string typeName)
    {
        var element = WorldModel.Elements.Get(elementName);
        var type = WorldModel.Elements.Get(ElementType.ObjectType, typeName);
        return element.Fields.InheritsType(type);
    }

    public void CreateNewObject(string name, string parent, string alias)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Create object '{0}'", name));
        CreateNewObject(name, parent, "editor_object", alias);
        WorldModel.UndoLogger.EndTransaction();
    }

    public void CreateNewRoom(string name, string parent, string alias)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Create room '{0}'", name));
        CreateNewObject(name, parent, "editor_room", alias);
        WorldModel.UndoLogger.EndTransaction();
    }

    public string CreateNewExit(string parent)
    {
        return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit);
    }

    public string CreateNewExit(string parent, string to, string alias, string type, bool lookonly)
    {
        return CreateNewExitInternal(parent, to, alias, true, type, lookonly);
    }

    public string CreateNewExitInternal(string parent, string to, string alias, bool useTransaction, string type,
        bool lookonly)
    {
        if (to == null && lookonly)
        {
            return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit, new List<string> {type},
                new Dictionary<string, object>
                {
                    {"to", WorldModel.Elements.Get(parent)},
                    {"alias", alias},
                    {"lookonly", lookonly},
                    {"look", ""}
                }, useTransaction);
        }

        return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit, new List<string> {type},
            new Dictionary<string, object>
            {
                {"to", WorldModel.Elements.Get(to)},
                {"alias", alias}
            }, useTransaction);
    }

    public string CreateNewExit(string parent, string to, string alias, string inverseAlias, string type,
        string inverseType, bool lookonly = false)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Create two-way exit {0} to {1}", parent, to));
        var result = CreateNewExitInternal(parent, to, alias, false, type, lookonly);
        CreateNewExitInternal(to, parent, inverseAlias, false, inverseType, lookonly);
        WorldModel.UndoLogger.EndTransaction();

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
            new List<string> {"defaultverb"},
            new Dictionary<string, object> {{"isverb", true}},
            useTransaction);
    }

    private string CreateNewAnonymousObject(string parent, string typeName, ObjectType type,
        IList<string> initialTypes = null, IDictionary<string, object> initialFields = null, bool useTransaction = true)
    {
        string desc;
        Element parentEl;
        if (parent != null)
        {
            desc = string.Format("Create new {0} in '{1}'", typeName, parent);
            parentEl = WorldModel.Elements.Get(ElementType.Object, parent);
        }
        else
        {
            desc = string.Format("Create new {0}", typeName);
            parentEl = null;
        }

        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(desc);
        }

        var newObject = WorldModel.ObjectFactory.CreateObject(type, initialTypes, initialFields);
        newObject.Parent = parentEl;
        newObject.Fields[FieldDefinitions.Anonymous] = true;

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }

        return newObject.Name;
    }

    private void CreateNewObject(string name, string parent, string editorType, string alias)
    {
        var newObject = WorldModel.GetElementFactory(ElementType.Object).Create(name);
        if (parent != null)
        {
            newObject.Parent = WorldModel.Elements.Get(ElementType.Object, parent);
        }

        if (!string.IsNullOrEmpty(alias))
        {
            newObject.Fields[FieldDefinitions.Alias] = alias;
        }

        if (WorldModel.Elements.ContainsKey(ElementType.ObjectType, editorType))
        {
            newObject.Fields.AddTypeUndoable(WorldModel.Elements.Get(ElementType.ObjectType, editorType));
        }
    }

    private string CreateNewElement(ElementType type, string typeName, string elementName, string parent = null,
        IDictionary<string, object> initialFields = null)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Create {0} '{1}'", typeName, elementName));
        Element newElement;

        if (elementName != null)
        {
            newElement = WorldModel.GetElementFactory(type).Create(elementName);
        }
        else
        {
            newElement = WorldModel.GetElementFactory(type).Create();
            newElement.Fields[FieldDefinitions.Anonymous] = true;
        }

        if (parent != null)
        {
            newElement.Parent = WorldModel.Elements.Get(parent);
        }

        if (initialFields != null)
        {
            foreach (var field in initialFields)
            {
                newElement.Fields.Set(field.Key, field.Value);
            }
        }

        WorldModel.UndoLogger.EndTransaction();

        return newElement.Name;
    }

    public void CreateNewFunction(string name)
    {
        CreateNewElement(ElementType.Function, "function", name);
    }

    public void CreateNewTimer(string name)
    {
        CreateNewElement(ElementType.Timer, "timer", name,
            initialFields: new Dictionary<string, object> {{"interval", 1}});
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
        WorldModel.UndoLogger.StartTransaction(string.Format("Add new template '{0}'", name));
        var newTemplate = WorldModel.AddNewTemplate(name);
        newTemplate.Fields[FieldDefinitions.Anonymous] = true;
        WorldModel.UndoLogger.EndTransaction();
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
        if (!WorldModel.Elements.ContainsKey(elementKey))
        {
            return false;
        }

        var element = WorldModel.Elements.Get(elementKey);
        return element.ElemType == ElementType.Object && element.Type != ObjectType.Game;
    }

    public bool CanMoveElement(string elementKey, string newParentKey)
    {
        if (elementKey == newParentKey)
        {
            return false;
        }

        if (!WorldModel.Elements.ContainsKey(elementKey))
        {
            return false;
        }

        if (newParentKey != "_objects" && !WorldModel.Elements.ContainsKey(newParentKey))
        {
            return false;
        }

        var element = WorldModel.Elements.Get(elementKey);
        if (element.ElemType == ElementType.Object && element.Type != ObjectType.Game)
        {
            if (newParentKey == "_objects")
            {
                // Can always drag an object to the "Objects" header to unset its parent property
                return true;
            }

            var newParent = WorldModel.Elements.Get(newParentKey);

            if (newParent.ElemType == ElementType.Object)
            {
                // Can't drag a parent object onto one of its own children
                return !WorldModel.ObjectContains(element, newParent);
            }
        }

        return false;
    }

    public void MoveElement(string elementKey, string newParentKey)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Move object '{0}' to '{1}'", elementKey, newParentKey));
        var element = WorldModel.Elements.Get(elementKey);
        var newParent = newParentKey == "_objects" ? null : WorldModel.Elements.Get(newParentKey);
        element.Parent = newParent;
        WorldModel.UndoLogger.EndTransaction();
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

        var elementType = WorldModel.GetElementTypeForTypeString(elementTypeString);

        if (elementKey == null)
        {
            return null;
        }

        if (!WorldModel.Elements.ContainsKey(elementType, elementKey))
        {
            return null;
        }

        var currentSelection = WorldModel.Elements.Get(elementKey);

        // For the object element type, we can only have parents with object types of object or game
        if (elementType != ElementType.Object || currentSelection.Type == ObjectType.Object ||
            currentSelection.Type == ObjectType.Game)
        {
            var result = new List<string>();
            result.Add(elementKey);

            var thisElement = currentSelection;
            while (thisElement.Parent != null)
            {
                result.Add(thisElement.Parent.Name);
                thisElement = thisElement.Parent;
            }

            // return the list with highest ancestor at the top
            result.Reverse();
            return result;
        }

        return null;
    }

    public IEnumerable<string> GetMovePossibleParents(string elementKey)
    {
        if (!CanMoveElement(elementKey))
        {
            return null;
        }

        var element = WorldModel.Elements.Get(elementKey);

        return from possibleParent in WorldModel.Elements.GetElements(ElementType.Object)
            where possibleParent != element
                  && possibleParent != element.Parent
                  && possibleParent.Type == ObjectType.Object
                  && !WorldModel.ObjectContains(element, possibleParent)
            orderby possibleParent.Name
            select possibleParent.Name;
    }

    public ValidationResult CanAdd(string name)
    {
        if (WorldModel.Elements.ContainsKey(name))
        {
            return new ValidationResult
            {
                Valid = false, Message = ValidationMessage.ElementAlreadyExists,
                SuggestedName = GetUniqueElementName(name)
            };
        }

        return ValidateElementName(name);
    }

    internal ValidationResult CanRename(Element element, string newName)
    {
        if (EditorStyle == EditorStyle.GameBook && element.Name == "player")
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.CannotRenamePlayerElement};
        }

        if (WorldModel.Elements.ContainsKey(newName) && WorldModel.Elements.Get(newName) != element)
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.ElementAlreadyExists};
        }

        return ValidateElementName(newName);
    }

    private ValidationResult ValidateElementName(string name)
    {
        if (string.IsNullOrEmpty(name) || !s_validNameRegex.IsMatch(name))
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.InvalidElementName};
        }

        if (name.StartsWith(" ") || name.EndsWith(" ") || name.Contains("  "))
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.InvalidElementNameMultipleSpaces};
        }

        if (s_startsWithNumberRegex.IsMatch(name))
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.InvalidElementNameStartsWithNumber};
        }

        var words = name.Split(' ');
        var keywords = Engine.Utility.ExpressionKeywords;
        foreach (var word in words)
        {
            if (keywords.Contains(word))
            {
                return new ValidationResult {Valid = false, Message = ValidationMessage.InvalidElementNameInvalidWord};
            }
        }

        return new ValidationResult {Valid = true};
    }

    public ValidationResult CanAddTemplate(string name)
    {
        var existingTemplate = WorldModel.TryGetTemplateElement(name);
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
            return new ValidationResult {Valid = false, Message = ValidationMessage.ElementAlreadyExists};
        }

        return new ValidationResult {Valid = true};
    }

    public void DeleteElement(string elementKey, bool useTransaction)
    {
        if (useTransaction)
        {
            WorldModel.UndoLogger.StartTransaction(string.Format("Delete '{0}'", elementKey));
        }

        var element = WorldModel.Elements.Get(elementKey);
        WorldModel.GetElementFactory(element.ElemType).DestroyElement(element.Name);
        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }
    }

    public bool IsVerbAttribute(string attributeName)
    {
        attributeName = attributeName.Trim();
        return WorldModel.Elements.GetElements(ElementType.Object).Any(e =>
            e.Fields.GetAsType<bool>("isverb") && e.Fields.GetString("property") == attributeName);
    }

    public void UIRequestAddElement(string elementType, string objectType, string filter)
    {
        RequestAddElement(this,
            new RequestAddElementEventArgs {ElementType = elementType, ObjectType = objectType, Filter = filter});
    }

    public void UIRequestEditElement(string key)
    {
        RequestEdit(this, new RequestEditEventArgs {Key = key});
    }

    public bool ElementExists(string elementKey)
    {
        return WorldModel.Elements.ContainsKey(elementKey);
    }

    public bool ElementIsVerb(string elementKey)
    {
        return WorldModel.Elements.Get(ElementType.Object, elementKey).Fields.GetAsType<bool>("isverb");
    }

    public IEnumerable<string> GetAvailableLibraries()
    {
        return WorldModel.GetAvailableLibraries();
    }

    public IEnumerable<string> GetAvailableExternalFiles(string searchPattern)
    {
        var baseFolder = Path.GetDirectoryName(WorldModel.Filename);
        return WorldModel.GetAvailableExternalFiles(searchPattern).Select(f => Path.Combine(baseFolder, f));
    }

    public void CopyElements(IEnumerable<string> elementNames)
    {
        m_clipboardElements = (from name in elementNames select WorldModel.Elements.Get(name)).ToList();

        var first = true;

        foreach (var e in m_clipboardElements)
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

        m_lastelementscutout = false;
    }

    public string PasteElements(string parentName)
    {
        if (!CanPaste(parentName))
        {
            return null;
        }

        string lastPastedElement = null;

        var parent = GetPasteParent(parentName);

        WorldModel.UndoLogger.StartTransaction("Paste");

        foreach (var e in m_clipboardElements)
        {
            Element newElement;
            if (EditorStyle == EditorStyle.TextAdventure)
            {
                newElement = e.Clone(el => true, m_lastelementscutout);
            }
            else if (EditorStyle == EditorStyle.GameBook)
            {
                newElement = e.Clone(el => el.Name != "player", m_lastelementscutout);
            }
            else
            {
                throw new NotImplementedException("Paste not implemented for this editor type");
            }

            newElement.Parent = parent;
            lastPastedElement = newElement.Name;
        }

        WorldModel.UndoLogger.EndTransaction();

        m_lastelementscutout = false;

        return lastPastedElement;
    }

    public void CutElements(IEnumerable<string> elementNames)
    {
        WorldModel.UndoLogger.StartTransaction("Cut");
        CopyElements(elementNames);
        m_lastelementscutout = true;

        /*
         * The cut out elements should be displayed in gray.
         * Unfortunately, I have not yet been able to do this from the EditorController.cs.
         * (SoonGames)

        foreach (string name in elementNames)
        {
            Element element = m_worldModel.Elements.Get(name);
            element.Fields.Set("forecolor", "color"); // ??? With Set I was only able to change the element name.
        }
        */

        WorldModel.UndoLogger.EndTransaction();
    }

    public bool CanPaste(string parentName)
    {
        if (m_clipboardElements == null || m_clipboardElements.Count == 0)
        {
            return false;
        }

        var parent = GetPasteParent(parentName);
        if (parent == null)
        {
            return true;
        }

        if (parent.MetaFields[MetaFieldDefinitions.Library])
        {
            return false;
        }

        return parent.ElemType == m_clipboardElementType;
    }

    private Element GetPasteParent(string parentName)
    {
        if (EditorStyle == EditorStyle.GameBook)
        {
            return null;
        }

        if (WorldModel.Elements.ContainsKey(parentName))
        {
            var e = WorldModel.Elements.Get(parentName);

            // If the selected item is an object or walkthrough, it is valid to paste something
            // as a child of this element

            if (e.ElemType == ElementType.Walkthrough ||
                (e.ElemType == ElementType.Object && e.Type == ObjectType.Object))
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
        if (!ElementExists(elementName))
        {
            return false;
        }

        if (elementName == "game")
        {
            return false;
        }

        if (EditorStyle == EditorStyle.GameBook && elementName == "player")
        {
            return false;
        }

        var e = WorldModel.Elements.Get(elementName);
        if (e.ElemType == ElementType.IncludedLibrary)
        {
            return false;
        }

        if (e.ElemType == ElementType.Javascript)
        {
            return false;
        }

        if (e.ElemType == ElementType.Template)
        {
            return false;
        }

        return true;
    }

    public bool CanDelete(string elementName)
    {
        if (!ElementExists(elementName))
        {
            return false;
        }

        if (elementName == "game")
        {
            return false;
        }

        if (EditorStyle == EditorStyle.GameBook && elementName == "player")
        {
            return false;
        }

        if (EditorStyle == EditorStyle.GameBook && WorldModel.ObjectContains(WorldModel.Elements.Get(elementName),
                WorldModel.Elements.Get("player")))
        {
            return false;
        }

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
            ScriptClipboardUpdated(this, new ScriptClipboardUpdateEventArgs {HasScript = m_clipboardScripts.Count > 0});
        }
    }

    internal IEnumerable<IScript> GetClipboardScript()
    {
        return m_clipboardScripts.AsReadOnly();
    }

    public IEnumerable<string> GetPropertyNames()
    {
        return WorldModel.GetAllAttributeNames;
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
            where Engine.Utility.IsRegexMatch(def.Pattern, expression)
            select def;

        if (!candidates.Any())
        {
            return null;
        }

        var orderedCandidates = from def in candidates
            orderby Engine.Utility.GetMatchStrength(def.Pattern, expression) descending
            select def;

        return orderedCandidates.First();
    }

    public IEditorData GetExpressionEditorData(string expression, string expressionType, IEditorData parentData)
    {
        return new ExpressionTemplateEditorData(expression,
            GetExpressionEditorDefinitionInternal(expression, expressionType), parentData);
    }

    public string GetNewExpression(string templateName)
    {
        var definitions = from def in m_expressionDefinitions.Values
            where def.Description == templateName
            select def;

        var definition = definitions.First();

        return definition.Create;
    }

    public string GetExpression(IEditorData data, string changedAttribute, string changedValue)
    {
        var expressionData = (ExpressionTemplateEditorData) data;
        return expressionData.SaveExpression(changedAttribute, changedValue);
    }

    public string GetDisplayVerbPatternForAttribute(string attribute)
    {
        // If the user adds a verb like "look in", it will have an attribute name like "lookin".
        // Here we return the simple pattern for the corresponding verb, if it has one (library
        // verbs use strings to contain regexes so will return null here)

        var verbs = from element in WorldModel.Elements.GetElements(ElementType.Object)
            where element.Type == ObjectType.Command
            where element.Fields[FieldDefinitions.IsVerb]
            where element.Fields[FieldDefinitions.Property] == attribute
            select element;

        var result = verbs.FirstOrDefault();

        if (result == null)
        {
            return null;
        }

        var pattern = result.Fields.Get(FieldDefinitions.Pattern.Property);
        var simplePattern = pattern as EditorCommandPattern;

        if (simplePattern == null)
        {
            return null;
        }

        return FriendlyVerbDisplayName(simplePattern.Pattern);
    }

    public ValidationResult Publish(string filename, bool includeWalkthrough,
        IEnumerable<PackageIncludeFile> includeFiles = null, Stream outputStream = null)
    {
        string error;
        if (WorldModel.CreatePackage(filename, includeWalkthrough, out error, includeFiles == null
                ? null
                : includeFiles.Select(f => new WorldModel.PackageIncludeFile
                {
                    Filename = f.Filename,
                    Content = f.Content
                }), outputStream))
        {
            return new ValidationResult {Valid = true, Message = ValidationMessage.OK};
        }

        return new ValidationResult {Valid = false, Message = ValidationMessage.ExceptionOccurred, MessageData = error};
    }

    public IEnumerable<string> GetBuiltInFunctionNames()
    {
        return WorldModel.GetBuiltInFunctionNames();
    }

    public static Dictionary<string, TemplateData> GetAvailableTemplates()
    {
        var resources = WorldModel.GetEmbeddedResources()
            .Where(name => name.StartsWith("QuestViva.Engine.Core.Templates") && name.EndsWith(".template"));

        var templates = new Dictionary<string, TemplateData>();

        foreach (var resource in resources)
        {
            AddTemplateData(templates, resource);
        }

        return templates;
    }

    private static void AddTemplateData(Dictionary<string, TemplateData> templates, string resourceName)
    {
        // Default to the stem of the resource name (e.g. "Dansk" from "QuestViva.Engine.Core.Templates.Dansk.template")
        var stem = resourceName[(resourceName.LastIndexOf('.', resourceName.Length - ".template".Length - 1) + 1)..^".template".Length];
        var templateName = stem;
        var templateEditorStyle = EditorStyle.TextAdventure;

        var stream = WorldModel.GetEmbeddedResourceStream(resourceName);
        var xmlReader = new XmlTextReader(stream);

        xmlReader.Read();
        if (xmlReader.Name == "asl")
        {
            var templateAttr = xmlReader.GetAttribute("template");
            if (!string.IsNullOrEmpty(templateAttr))
            {
                templateName = templateAttr;
            }

            var templateType = xmlReader.GetAttribute("templatetype");
            switch (templateType)
            {
                case "gamebook":
                    templateEditorStyle = EditorStyle.GameBook;
                    break;
            }
        }

        templates.Add(templateName, new TemplateData
        {
            ResourceName = resourceName,
            TemplateName = templateName,
            Type = templateEditorStyle
        });
    }

    public static string CreateNewGameFile(string template, string gameName)
    {
        var stream = WorldModel.GetEmbeddedResourceStream(template);
        var templateText = new StreamReader(stream).ReadToEnd();
        var initialFileText = templateText
            .Replace("$NAME$", Engine.Utility.SafeXML(gameName))
            .Replace("$ID$", GetNewGameId())
            .Replace("$YEAR$", DateTime.Now.Year.ToString());

        return initialFileText;
    }

    public CanAddVerbResult CanAddVerb(string verbPattern)
    {
        // Split into each verb, and trim
        var verbList = verbPattern.Split(';').Select(p => p.Trim()).ToList();


        // Check against the disallowed list first
        var disallowedList = "ask;tell;look;enter".Split(';');
        foreach (var disallowed in disallowedList)
        {
            if (verbList.Contains(disallowed))
            {
                return new CanAddVerbResult
                {
                    CanAdd = false,
                    ClashingCommand = disallowed,
                    ClashingCommandDisplay = disallowed
                };
            }
        }


        // Now see if "verb object" is a match for the regex of an existing command in the game
        var result = new CanAddVerbResult();
        // Iterate through each verb in the pattern the user offered
        foreach (var matchVerb in verbList)
        {
            var matchPattern = matchVerb + " object";

            // iterate through each command
            foreach (var cmd in from e in WorldModel.Objects
                     where e.Type == ObjectType.Command
                     where e.Parent == null
                     where !e.Fields[FieldDefinitions.IsVerb]
                     select e)
            {
                string regexPattern = null;

                var pattern = cmd.Fields.Get(FieldDefinitions.Pattern.Property);
                var editorCommandPattern = pattern as EditorCommandPattern;
                var stringPattern = pattern as string;

                if (editorCommandPattern != null)
                {
                    regexPattern = Engine.Utility.ConvertVerbSimplePattern(editorCommandPattern.Pattern, null);
                }
                else
                {
                    regexPattern = stringPattern;
                }

                if (regexPattern != null)
                {
                    var isClash = false;
                    var regex = new Regex(regexPattern);
                    if (regex.IsMatch(matchPattern))
                    {
                        isClash = true;
                        IDictionary<string, string> parseResult = Engine.Utility.Populate(regexPattern, matchPattern);

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
        }

        result.CanAdd = true;
        return result;
    }

    public void SwapElements(string key1, string key2)
    {
        var a = WorldModel.Elements.Get(key1);
        var b = WorldModel.Elements.Get(key2);

        var index = a.MetaFields[MetaFieldDefinitions.SortIndex];
        a.MetaFields[MetaFieldDefinitions.SortIndex] = b.MetaFields[MetaFieldDefinitions.SortIndex];
        b.MetaFields[MetaFieldDefinitions.SortIndex] = index;
    }

    public void BeginWalkthrough(string name, bool record)
    {
        RequestRunWalkthrough(this, new RequestRunWalkthroughEventArgs {Name = name, Record = record});
    }

    public void RecordWalkthrough(string name, IEnumerable<string> steps)
    {
        if (!steps.Any())
        {
            return;
        }

        StartTransaction(string.Format("Add {0} walkthrough steps", steps.Count()));
        var walkthrough = WorldModel.Elements.Get(ElementType.Walkthrough, name);
        var newSteps = new QuestList<string>(steps);
        // TO DO: Use MergeLists
        walkthrough.Fields[FieldDefinitions.Steps] = walkthrough.Fields[FieldDefinitions.Steps] + newSteps;
        EndTransaction();
    }

    public void Uninitialise()
    {
        if (m_editorDefinitions != null)
        {
            m_editorDefinitions.Clear();
        }

        if (m_expressionDefinitions != null)
        {
            m_expressionDefinitions.Clear();
        }

        if (m_elementTreeStructure != null)
        {
            m_elementTreeStructure.Clear();
        }

        if (m_clipboardElements != null)
        {
            m_clipboardElements.Clear();
        }

        if (m_clipboardScripts != null)
        {
            m_clipboardScripts.Clear();
        }

        if (WorldModel != null)
        {
            WorldModel.ElementFieldUpdated -= m_worldModel_ElementFieldUpdated;
            WorldModel.ElementRefreshed -= m_worldModel_ElementRefreshed;
            WorldModel.ElementMetaFieldUpdated -= m_worldModel_ElementMetaFieldUpdated;
            WorldModel.UndoLogger.TransactionsUpdated -= UndoLogger_TransactionsUpdated;
            WorldModel.Elements.ElementRenamed -= Elements_ElementRenamed;
            WorldModel.ObjectsUpdated -= m_worldModel_ObjectsUpdated;
        }

        EditableScripts.Clear();
        EditableDictionary<string>.Clear();
        EditableList<string>.Clear();
        EditableWrappedItemDictionary<IScript, IEditableScripts>.Clear();
    }

    public static string GetNewGameId()
    {
        return Guid.NewGuid().ToString();
    }

    public string GetUniqueElementName(string elementName)
    {
        return WorldModel.GetUniqueElementName(elementName);
    }

    public string GetSelectedDropDownType(IEditorControl ctl, string element)
    {
        const string k_noType = "*";

        var types = ctl.GetDictionary("types");
        var inheritedTypes = new List<string>();

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
        var availableVerbs = GetVerbProperties();

        var attributeForSelectedPattern = from verb in availableVerbs.Keys
            where availableVerbs[verb] == selectedPattern
            select verb;

        var selectedAttribute = attributeForSelectedPattern.FirstOrDefault();

        if (selectedAttribute == null)
        {
            // we couldn't find a matching verb property name, so see if there is a matching verb
            // pattern instead. For example, if the user typed "sit on" then we want to match
            // the "sit" verb, as "sit on" is one of its patterns.

            foreach (var verb in availableVerbs)
            {
                var patterns = new List<string>(verb.Value.Split(';').Select(p => p.Trim()));
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

            var semicolonPos = selectedPattern.IndexOf(';');
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

    public static string GenerateSafeFilename(string gameName)
    {
        var result = gameName;
        foreach (var invalidChar in s_invalidChars)
        {
            result = result.Replace(invalidChar, "");
        }

        if (result.Length == 0)
        {
            return string.Empty;
        }

        result = result.TrimStart(new[] {'.'});
        return result;
    }

    // TODO: Wire this up when creating new game from WebEditor
    public static bool IsReservedFilename(string gameName)
    {
        var safeFilename = GenerateSafeFilename(gameName);
        if (string.IsNullOrEmpty(safeFilename))
        {
            return false;
        }

        return GetAvailableTemplates().Keys
            .Any(name => string.Equals(name, safeFilename, StringComparison.OrdinalIgnoreCase));
    }

    internal void UpdateDictionariesReferencingRenamedObject(string oldName, string newName)
    {
        // This function is used so we can safely rename gamebook pages and have the corresponding links
        // be updated. Because these are stored as dictionary keys, they won't be updated automatically
        // as they don't point directly to the object. So we need to scan all objects with any "affectable"
        // string/script dictionaries (determined by their corresponding control having a source of "object",
        // and update keys if necessary

        var objectEditor = m_editorDefinitions["object"];
        foreach (var tab in objectEditor.Tabs.Values)
        {
            foreach (var ctl in tab.Controls.Where(c => c.GetString("source") == "object"))
            {
                // So now we know that we need to scan all objects in the game which have a dictionary for
                // ctl.Attribute, because the keys to this dictionary are object names.

                foreach (var element in WorldModel.Elements.GetElements(ElementType.Object))
                {
                    var value = element.Fields.Get(ctl.Attribute);
                    var dictionary = value as IDictionary;
                    if (dictionary != null && dictionary.Contains(oldName))
                    {
                        var wrappedValue = WrapValue(value);
                        var editableStringDictionary = wrappedValue as IEditableDictionary<string>;
                        var editableScriptDictionary = wrappedValue as IEditableDictionary<IEditableScripts>;
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
        var obscured = string.Empty;
        try
        {
            obscured = Engine.Utility.ObscureStrings(expression);
        }
        catch (MismatchingQuotesException)
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.MismatchingQuotes};
        }

        var braceCount = 0;
        foreach (var c in obscured)
        {
            if (c == '(')
            {
                braceCount++;
            }

            if (c == ')')
            {
                braceCount--;
            }
        }

        if (braceCount != 0)
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.MismatchingBrackets};
        }

        return new ValidationResult {Valid = true};
    }

    public static string GetValidationError(ValidationResult result, object input)
    {
        return string.Format(s_validationMessages[result.Message], input, result.MessageData);
    }

    public List<string> AvailableBaseFonts()
    {
        if (m_fontsManager == null)
        {
            m_fontsManager = new FontsManager();
        }

        return m_fontsManager.GetBaseFonts();
    }

    public List<string> AvailableWebFonts()
    {
        if (m_fontsManager == null)
        {
            m_fontsManager = new FontsManager();
        }

        return m_fontsManager.GetWebFonts();
    }

    public class AddedNodeEventArgs : EventArgs
    {
        public string Key { get; set; }
        public string Text { get; set; }
        public string Parent { get; set; }
        public bool IsLibraryNode { get; set; }
        public int? Position { get; set; }
        public string NodeIcon { get; set; }
    }

    public class RemovedNodeEventArgs : EventArgs
    {
        public string Key { get; set; }
    }

    public class RenamedNodeEventArgs : EventArgs
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
    }

    public class RetitledNodeEventArgs : EventArgs
    {
        public string Key { get; set; }
        public string NewTitle { get; set; }
    }

    public class ShowMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class RequestAddElementEventArgs : EventArgs
    {
        public string ElementType { get; set; }
        public string ObjectType { get; set; }
        public string Filter { get; set; }
    }

    public class RequestEditEventArgs : EventArgs
    {
        public string Key { get; set; }
    }

    public class ElementMovedEventArgs : EventArgs
    {
        public string Key { get; set; }
    }

    public class ScriptClipboardUpdateEventArgs : EventArgs
    {
        public bool HasScript { get; set; }
    }

    public class RequestRunWalkthroughEventArgs : EventArgs
    {
        public string Name { get; set; }
        public bool Record { get; set; }
    }

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
    }

    private class TreeHeader
    {
        public string Key;
        public string Title;
    }

    public class InitialiseResults : EventArgs
    {
        internal InitialiseResults(bool success)
        {
            Success = success;
        }

        public bool Success { get; private set; }
    }

    public class PackageIncludeFile
    {
        public string Filename { get; set; }
        public Stream Content { get; set; }
    }

    public struct CanAddVerbResult
    {
        public bool CanAdd;
        public string ClashingCommand;
        public string ClashingCommandDisplay;
    }
}