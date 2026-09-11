using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
    InvalidElementNameEmpty,
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
    public string? MessageData;
    public string? SuggestedName;
}

public class TemplateData
{
    public required string TemplateName { get; set; }
    public required string ResourceName { get; set; }
    public EditorStyle Type { get; set; }
}

public sealed class EditorController : IDisposable
{
    private const string Commands = "_gameCommands";
    private const string Verbs = "_gameVerbs";

    private static readonly Dictionary<ValidationMessage, string> ValidationMessages = new()
    {
        {ValidationMessage.OK, "No error"},
        {ValidationMessage.ItemAlreadyExists, "Item '{0}' already exists in the list"},
        {ValidationMessage.ElementAlreadyExists, "An element called '{0}' already exists in this game"},
        {ValidationMessage.InvalidAttributeName, "Invalid attribute name"},
        {ValidationMessage.ExceptionOccurred, "An error occurred: {1}"},
        {
            ValidationMessage.InvalidElementName,
            "Invalid element name. {1} cannot be used - element names can only contain letters, numbers, underscores and spaces."
        },
        {ValidationMessage.InvalidElementNameEmpty, "Element name cannot be empty."},
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

    private readonly Dictionary<string, EditorDefinition> _editorDefinitions = [];
    private readonly Dictionary<string, EditorDefinition> _expressionDefinitions = [];

    private readonly List<ElementType> _ignoredTypes =
    [
        ElementType.ImpliedType,
        ElementType.Delegate,
        ElementType.Editor,
        ElementType.EditorTab,
        ElementType.EditorControl,
        ElementType.Resource
    ];

    private readonly Regex _startsWithNumberRegex = new(@"^\d");

    private readonly Regex _validNameRegex = new(@"^[\w ]+$");
    private List<Element>? _clipboardElements;
    private ElementType _clipboardElementType;
    private List<IScript>? _clipboardScripts;
    // Set by InitialiseTreeStructure, which UpdateTree runs before anything reads the tree
    private Dictionary<ElementType, TreeHeader> _elementTreeStructure = null!;
    private FilterOptions _filterOptions;
    private FontsManager? _fontsManager;
    private bool _initialised;
    private bool _lastelementscutout;
    // Set by Initialise
    private ScriptFactory _scriptFactory = null!;

    public EditorController()
    {
        _filterOptions = new FilterOptions();
        // set default filters here

        // FontsManager is initialized lazily to avoid firing the Google Fonts network request
        // until fonts are actually needed (e.g. not during test runs).
    }

    public string? GameId => WorldModel.GameID;

    // Set by Initialise
    internal EditableScriptFactory ScriptFactory { get; private set; } = null!;

    // Set by Initialise (Dispose and Uninitialise still check for null, in case it never ran)
    internal WorldModel WorldModel { get; private set; } = null!;

    public static IList<string> ExpressionKeywords => Engine.Utility.ExpressionKeywords;

    // Set by Initialise
    public string Filename { get; set; } = null!;

    public EditorStyle EditorStyle { get; private set; } = EditorStyle.TextAdventure;

    public void Dispose()
    {
        WorldModel?.FinishGame();
    }

    // The tree events (ClearTree through RetitledNode) and ShowMessage are raised without a null
    // check, so a host must subscribe to them; the rest are optional.
    public event EventHandler? ClearTree;
    public event EventHandler? BeginTreeUpdate;
    public event EventHandler? EndTreeUpdate;

    public event EventHandler<AddedNodeEventArgs>? AddedNode;

    public event EventHandler<RemovedNodeEventArgs>? RemovedNode;

    public event EventHandler<RenamedNodeEventArgs>? RenamedNode;

    public event EventHandler<RetitledNodeEventArgs>? RetitledNode;

    public event EventHandler<ShowMessageEventArgs>? ShowMessage;

    public event EventHandler? ElementsUpdated;

    public event EventHandler<ElementMovedEventArgs>? ElementMoved;

    public event EventHandler<ScriptClipboardUpdateEventArgs>? ScriptClipboardUpdated;

    public event EventHandler<ElementUpdatedEventArgs>? ElementUpdated;
    public event EventHandler<ElementRefreshedEventArgs>? ElementRefreshed;
    public event EventHandler<UpdateUndoListEventArgs>? UndoListUpdated;
    public event EventHandler<UpdateUndoListEventArgs>? RedoListUpdated;
    public event EventHandler? Dirty;
    public event EventHandler<LoadStatusEventArgs>? LoadStatus;
    public event EventHandler<LibrariesUpdatedEventArgs>? LibrariesUpdated;

    public async Task<bool> Initialise(IGameDataProvider gameDataProvider, bool partialInit = false)
    {
        _lastelementscutout = false;
        var gameData = await gameDataProvider.GetData();
        Filename = gameData?.Filename ?? string.Empty;
        WorldModel = new WorldModel(gameData, null);
        _scriptFactory = new ScriptFactory(WorldModel);
        WorldModel.ElementFieldUpdated += OnWorldModelElementFieldUpdated;
        WorldModel.ElementRefreshed += OnWorldModelElementRefreshed;
        WorldModel.ElementMetaFieldUpdated += OnWorldModelElementMetaFieldUpdated;
        WorldModel.UndoLogger.TransactionsUpdated += UndoLogger_TransactionsUpdated;
        WorldModel.UndoLogger.TransactionCommitted += (_, _) => Dirty?.Invoke(this, EventArgs.Empty);
        WorldModel.Elements.ElementRenamed += Elements_ElementRenamed;
        WorldModel.LoadStatus += OnWorldModelLoadStatus;

        var ok = await WorldModel.InitialiseEdit();

        if (ok)
        {
            // TODO: Move this code to another initialisation method - it's not needed when running tests
            if (!partialInit)
            {
                if (WorldModel.IsGamebook)
                {
                    EditorStyle = EditorStyle.GameBook;
                    _ignoredTypes.Add(ElementType.Template);
                    _ignoredTypes.Add(ElementType.ObjectType);
                }

                // need to initialise the EditableScriptFactory after we've loaded the game XML above,
                // as the editor definitions contain the "friendly" templates for script commands.
                ScriptFactory = new EditableScriptFactory(this, _scriptFactory, WorldModel);

                _initialised = true;

                WorldModel.ObjectsUpdated += OnWorldModelObjectsUpdated;

                foreach (var e in WorldModel.Elements.GetElements(ElementType.Editor))
                {
                    var def = new EditorDefinition(WorldModel, e);
                    if (def.AppliesTo != null)
                    {
                        // Normal editor definition for editing an element or a script command
                        _editorDefinitions.Add(def.AppliesTo, def);
                    }
                    else if (def.Pattern != null)
                    {
                        // Editor definition for an expression template in the "if" editor
                        _expressionDefinitions.Add(def.Pattern, def);
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

            ShowMessage!(this, new ShowMessageEventArgs {Message = message});
        }

        return ok;
    }

    private void OnWorldModelLoadStatus(object? sender, Engine.LoadStatusEventArgs e)
    {
        LoadStatus?.Invoke(this, new LoadStatusEventArgs(e.Status));
    }

    private void Elements_ElementRenamed(object? sender, NameChangedEventArgs e)
    {
        var oldName = e.OldName;
        var newName = e.Element.Name;

        RenamedNode!(this, new RenamedNodeEventArgs {OldName = oldName, NewName = newName});
        ElementsUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void UndoLogger_TransactionsUpdated(object? sender, EventArgs e)
    {
        UndoListUpdated?.Invoke(this, new UpdateUndoListEventArgs(WorldModel.UndoLogger.UndoList()));

        RedoListUpdated?.Invoke(this, new UpdateUndoListEventArgs(WorldModel.UndoLogger.RedoList()));
    }

    private void OnWorldModelElementFieldUpdated(object? sender, ElementFieldUpdatedEventArgs e)
    {
        if (!_initialised)
        {
            return;
        }

        ElementUpdated?.Invoke(this,
    new ElementUpdatedEventArgs(e.Element.Name, e.Attribute, WrapValue(e.NewValue, e.Element, e.Attribute),
        e.IsUndo));

        if (e.Attribute == "parent")
        {
            BeginTreeUpdate!(this, new EventArgs());
            RemoveElementAndSubElementsFromTree(e.Element);
            AddElementAndSubElementsToTree(e.Element);
            EndTreeUpdate!(this, new EventArgs());
            ElementsUpdated?.Invoke(this, new EventArgs());
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
                RetitledNode!(this,
                    new RetitledNodeEventArgs {Key = e.Element.Name, NewTitle = GetDisplayName(e.Element)});
                ElementsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        if (e.Element.Type == ObjectType.Command && e.Attribute == "isverb")
        {
            MoveNove(e.Element.Name!, GetDisplayName(e.Element), GetElementTreeParent(e.Element));
        }

        if (e.Attribute == "editorfolder" && e.Element.Name != null)
        {
            // A plain (non-reparenting) field refresh — AddedNode's consumers already upsert an
            // existing key in place, so no RemovedNode is needed first (contrast the "sortindex"/
            // "library" cases above, which reposition the node and so do remove-then-readd).
            AddElementToTree(e.Element);
        }

        if (e.Element.ElemType == ElementType.IncludedLibrary && e.Attribute == "filename")
        {
            LibrariesUpdated?.Invoke(this, new LibrariesUpdatedEventArgs());
        }
    }

    private void OnWorldModelElementMetaFieldUpdated(object? sender, ElementFieldUpdatedEventArgs e)
    {
        if (!_initialised)
        {
            return;
        }

        //System.Diagnostics.Debug.Print("Updated: {0}.{1} = {2}", e.Element, e.Attribute, e.NewValue);

        if (e.Attribute == "sortindex")
        {
            RemovedNode!(this, new RemovedNodeEventArgs {Key = e.Element.Name});
            AddElementAndSubElementsToTree(e.Element, GetElementPosition(e.Element));
            ElementMoved?.Invoke(this, new ElementMovedEventArgs { Key = e.Element.Name });
        }

        if (e.Attribute == "library")
        {
            // Refresh the element in the tree by deleting and readding it
            RemovedNode!(this, new RemovedNodeEventArgs {Key = e.Element.Name});
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

    private void MoveNove(string key, string text, string? newParent)
    {
        RemovedNode!(this, new RemovedNodeEventArgs {Key = key});
        var movedElement = WorldModel.Elements.ContainsKey(key) ? WorldModel.Elements.Get(key) : null;
        AddedNode!(this,
            new AddedNodeEventArgs
            {
                Key = key, Text = text, Parent = newParent, IsLibraryNode = false, Position = null,
                NodeType = movedElement != null ? GetNodeType(movedElement) : null
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
            RemovedNode!(this, new RemovedNodeEventArgs {Key = key});
        }

        // finally remove the parent
        RemovedNode!(this, new RemovedNodeEventArgs {Key = e.Name});
    }

    private void OnWorldModelElementRefreshed(object? sender, ElementRefreshEventArgs e)
    {
        if (_initialised)
        {
            ElementRefreshed?.Invoke(this, new ElementRefreshedEventArgs(e.Element.Name));
        }
    }

    private void OnWorldModelObjectsUpdated(object? sender, ObjectsUpdatedEventArgs args)
    {
        if (args.Added != null)
        {
            var addedElement = WorldModel.Elements.Get(args.Added);
            AddElementToTree(addedElement, name: args.Added);
        }

        if (args.Removed != null)
        {
            RemovedNode!(this, new RemovedNodeEventArgs {Key = args.Removed});
        }

        ElementsUpdated?.Invoke(this, new EventArgs());
    }

    private void InitialiseTreeStructure()
    {
        _elementTreeStructure = [];

        AddTreeHeader(EditorStyle.TextAdventure, ElementType.Object, "_objects", "Objects", null);
        AddTreeHeader(EditorStyle.GameBook, ElementType.Object, "_objects", "Pages", null);
        AddTreeHeader(null, null, "_advanced", "Advanced", null);
        AddTreeHeader(null, ElementType.Function, "_functions", "Functions", "_advanced");
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.Timer, "_timers", "Timers", "_advanced");
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.Walkthrough, "_walkthrough", "Walkthrough", "_advanced");
        AddTreeHeader(null, ElementType.IncludedLibrary, "_include", "Included Libraries", "_advanced");
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.Template, "_template", "Templates", "_advanced");
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.DynamicTemplate, "_dynamictemplate", "Dynamic Templates",
            "_advanced");
        AddTreeHeader(EditorStyle.TextAdventure, ElementType.ObjectType, "_objecttype", "Object Types", "_advanced");
        AddTreeHeader(null, ElementType.Javascript, "_javascript", "Javascript", "_advanced");
    }

    private void AddTreeHeader(EditorStyle? editorStyle, ElementType? type, string key, string title, string? parent)
    {
        if (editorStyle.HasValue && EditorStyle != editorStyle)
        {
            return;
        }

        var header = new TreeHeader {Key = key, Title = title};
        if (type != null)
        {
            _elementTreeStructure.Add(type.Value, header);
        }

        AddedNode!(this,
            new AddedNodeEventArgs
            {
                Key = key, Text = title, Parent = parent, IsLibraryNode = false, Position = null,
                NodeType = "header"
            });
    }

    public void UpdateTree()
    {
        if (BeginTreeUpdate == null)
        {
            return;
        }

        BeginTreeUpdate(this, new EventArgs());
        ClearTree!(this, new EventArgs());
        InitialiseTreeStructure();

        foreach (ElementType type in Enum.GetValues<ElementType>())
        {
            // GetElements returns elements in raw creation/storage order, not SortIndex order -
            // fine normally (the two coincide until something reorders SortIndex without also
            // touching storage order, e.g. SwapElements or SetFunctionFolder's slice
            // redistribution), but a full rebuild like this one must sort explicitly or a
            // reordered element's new tree position silently reverts to its old, pre-reorder spot
            // the next time anything triggers a full UpdateTree() rather than an incremental
            // single-element reposition (see OnWorldModelElementMetaFieldUpdated's own
            // GetElementPosition, which already gets this right for that path).
            foreach (var o in WorldModel.Elements.GetElements(type).Where(e => e.Parent == null)
                         .OrderBy(e => e.MetaFields[MetaFieldDefinitions.SortIndex]))
            {
                AddElementAndChildrenToTree(o);
            }
        }

        EndTreeUpdate!(this, new EventArgs());
    }

    private void AddElementAndChildrenToTree(Element o)
    {
        AddElementToTree(o);
        // Same SortIndex-ordering requirement as the top-level loop in UpdateTree() above -
        // GetDirectChildren is also raw storage order, not SortIndex order.
        foreach (var child in WorldModel.Elements.GetDirectChildren(o)
                     .OrderBy(e => e.MetaFields[MetaFieldDefinitions.SortIndex]))
        {
            AddElementAndChildrenToTree(child);
        }
    }

    // optional name parameter to prevent an exception when redoing object creation, as the
    // object will not have a name attribute immediately
    private void AddElementToTree(Element o, int? position = null, string? name = null)
    {
        if (!IsElementVisible(o))
        {
            return;
        }

        var parent = GetElementTreeParent(o);

        var text = GetDisplayName(o);
        var display = true;
        var isLibrary = o.MetaFields.GetAsType<bool>("library");

        if (isLibrary && !_filterOptions.IsSet("libraries"))
        {
            display = false;
        }

        if (display)
        {
            var key = name ?? o.Name;
            AddedNode!(this,
                new AddedNodeEventArgs
                {
                    Key = key, Text = text, Parent = parent, IsLibraryNode = isLibrary,
                    Filename = o.MetaFields[MetaFieldDefinitions.Filename],
                    Folder = o.ElemType == ElementType.Function ? o.Fields[FieldDefinitions.EditorFolder] : null,
                    Position = position,
                    NodeType = GetNodeType(o)
                });

            if (o.Name == "game" && EditorStyle == EditorStyle.TextAdventure)
            {
                AddedNode!(this,
                    new AddedNodeEventArgs
                    {
                        Key = Verbs, Text = "Verbs", Parent = "game", IsLibraryNode = false, Position = null,
                        NodeType = "header"
                    });
                AddedNode!(this,
                    new AddedNodeEventArgs
                    {
                        Key = Commands, Text = "Commands", Parent = "game", IsLibraryNode = false, Position = null,
                        NodeType = "header"
                    });
            }
        }
    }

    private bool IsElementVisible(Element e)
    {
        // Don't display implied types, editor elements etc.
        if (_ignoredTypes.Contains(e.ElemType))
        {
            return false;
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
            if (e.Fields[FieldDefinitions.TemplateName] is { } templateName &&
                WorldModel.TryGetTemplateElement(templateName) != e)
            {
                return false;
            }
        }

        return true;
    }

    private string? GetElementTreeParent(Element o)
    {
        if (o.Parent != null)
        {
            return o.Parent.Name;
        }

        if (o.ElemType == ElementType.Object && o.Type == ObjectType.Command)
        {
            return o.Fields.GetAsType<bool>("isverb") ? Verbs : Commands;
        }

        return _elementTreeStructure[o.ElemType].Key;
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
                    // Templates.AddTemplate always sets templatename
                    return e.Fields[FieldDefinitions.TemplateName]!;
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

    // The node-type string is consumed directly by the frontend (AppShell's TreePanel.svelte)
    // both for behaviour (e.g. which "add" options a node offers) and for picking a tree icon —
    // keep this as the single source of truth rather than adding another translation layer.
    private string GetNodeType(Element o)
    {
        switch (o.ElemType)
        {
            case ElementType.Object:
                switch (o.Type)
                {
                    case ObjectType.Game: return "game";
                    case ObjectType.Exit: return "exit";
                    case ObjectType.Command:
                        return o.Fields.GetAsType<bool>("isverb") ? "verb" : "command";
                    case ObjectType.TurnScript: return "turnscript";
                    case ObjectType.Object:
                        if (EditorStyle == EditorStyle.GameBook)
                        {
                            return "page";
                        }

                        if (IsDialoguePage(o))
                        {
                            return "page";
                        }

                        return IsRoom(o) ? "room" : "object";
                    default: return "other";
                }
            case ElementType.Function: return "function";
            case ElementType.Timer: return "timer";
            case ElementType.Walkthrough: return "walkthrough";
            case ElementType.IncludedLibrary: return "include";
            case ElementType.Template: return "template";
            case ElementType.DynamicTemplate: return "dynamictemplate";
            case ElementType.ObjectType: return "type";
            case ElementType.Javascript: return "javascript";
            default: return "other";
        }
    }

    public void UpdateFilterOptions(FilterOptions options)
    {
        _filterOptions = options;
        UpdateTree();
    }

    public string? GetElementEditorName(string elementKey)
    {
        // elementKey is "game", "k1" (a command), "someobject", "myexit" etc.
        // we return the editor type name, e.g. "game", "command", "object", "exit".

        if (WorldModel.Elements.ContainsKey(elementKey))
        {
            var e = WorldModel.Elements.Get(elementKey);

            string? type = null;

            if (e.ElemType == ElementType.Object)
            {
                type = e.Fields.GetString("type");
            }

            if (string.IsNullOrEmpty(type))
            {
                // Every element has an elementtype field (set by Element itself)
                type = e.Fields.GetString("elementtype")!;
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

            if (_editorDefinitions.ContainsKey(type))
            {
                return type;
            }
        }
        else if (_editorDefinitions.ContainsKey(elementKey))
        {
            return elementKey;
        }

        return null;
    }

    public Dictionary<string, EditableScriptData> GetScriptEditorData()
    {
        return ScriptFactory.ScriptData;
    }

    public IEditorDefinition GetEditorDefinition(IEditableScript script)
    {
        if (script.EditorName.StartsWith("(function)"))
        {
            // see if we have a specific editor definition for this function
            if (_editorDefinitions.TryGetValue(script.EditorName, out var result))
            {
                return result;
            }

            // if not, return the default function call editor definition, and reset
            // the EditorName for the script so it knows to get/set parameters via a
            // parameter dictionary instead of individually.
            script.EditorName = "()";
        }

        return _editorDefinitions[script.EditorName];
    }

    public IEditorDefinition GetEditorDefinition(string editorName)
    {
        return _editorDefinitions[editorName];
    }

    public IEditorData? GetEditorData(string elementKey)
    {
        if (!WorldModel.Elements.ContainsKey(elementKey))
        {
            return null;
        }

        return new EditorData(WorldModel.Elements.Get(elementKey), this);
    }

    // Turns a library-origin element (loaded from Core.aslx or another included library) into a
    // normal, editable game element. On the next save this element then gets written into the
    // user's own game file (GameSaver skips anything still flagged "library"), which is how
    // library overriding works — the local copy shadows the library original from then on.
    public void MakeElementLocal(string key)
    {
        if (GetEditorData(key) is not IEditorDataExtendedAttributeInfo data || !data.IsLibraryElement)
        {
            return;
        }

        WorldModel.UndoLogger.StartTransaction($"Copy '{GetDisplayName(WorldModel.Elements.Get(key))}' into game");
        data.MakeElementLocal();
        WorldModel.UndoLogger.EndTransaction();

        AddElementToTree(WorldModel.Elements.Get(key));
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

    public void Redo()
    {
        WorldModel.UndoLogger.Redo();
    }

    public IEnumerable<string> GetUndoItems()
    {
        return WorldModel.UndoLogger.UndoList();
    }

    public IEnumerable<string> GetRedoItems()
    {
        return WorldModel.UndoLogger.RedoList();
    }

    [return: NotNullIfNotNull(nameof(value))]
    internal object? WrapValue(object? value)
    {
        return WrapValue(value, null, null);
    }

    [return: NotNullIfNotNull(nameof(value))]
    internal object? WrapValue(object? value, Element? element, string? attribute)
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

    public EditableScripts CreateNewEditableScripts(string? parent, string? attribute, string? keyword,
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
            newValue = EditableScripts.GetInstance(this, element.Fields.GetAsType<IScript>(attribute)!);
        }

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }

        return newValue;
    }

    public IEditableList<string> CreateNewEditableList(string? parent, string attribute, string? item,
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
            newList = element.Fields.GetAsType<QuestList<string>>(attribute)!;
        }

        var newValue = new EditableList<string>(this, newList);

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }

        return newValue;
    }

    public IEditableDictionary<string> CreateNewEditableStringDictionary(string? parent, string attribute, string? key,
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
            newDictionary = element.Fields.GetAsType<QuestDictionary<string>>(attribute)!;
        }

        var newValue = new EditableDictionary<string>(this, newDictionary);

        if (useTransaction)
        {
            WorldModel.UndoLogger.EndTransaction();
        }

        return newValue;
    }

    public IEditableDictionary<IEditableScripts> CreateNewEditableScriptDictionary(string? parent, string attribute,
        string? key, IEditableScripts script, bool useTransaction)
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
            newDictionary.Add(key, (IScript) script.GetUnderlyingValue()!);
        }

        if (element != null)
        {
            element.Fields.Set(attribute, newDictionary);

            // setting an element field will clone the value, so we want to return the new dictionary
            newDictionary = element.Fields.GetAsType<QuestDictionary<IScript>>(attribute)!;
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

    private IEnumerable<Element> GetElements(string elementType)
    {
        var t = WorldModel.GetElementTypeForTypeString(elementType);
        return WorldModel.Elements.GetElements(t).Where(e => e.Name != null);
    }

    public IEnumerable<string> GetElementNames(string elementType)
    {
        return GetElements(elementType).Select(e => e.Name);
    }

    public object? GetElementDataAttribute(string elementName, string attribute)
    {
        var element = WorldModel.Elements.Get(elementName);
        return element.Fields.Get(attribute);
    }

    public string? GetObjectType(string element)
    {
        if (!WorldModel.Elements.ContainsKey(element))
        {
            return null;
        }

        return WorldModel.GetTypeStringForObjectType(WorldModel.Elements.Get(element).Type);
    }

    private IEnumerable<Element> GetObjectNamesInternal(string objectType, string? parent = null,
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

    public IEnumerable<string> GetObjectNames(string objectType, string? parent = null, bool includeAnonymous = false)
    {
        return GetObjectNamesInternal(objectType, parent, includeAnonymous).Select(e => e.Name);
    }

    public IEnumerable<string> GetObjectNames(string objectType, bool includeLibraryObjects, string? parent = null,
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
            // verbElements only includes verbs with a property
            var verbProperty = verb.Fields[FieldDefinitions.Property]!;
            var pattern = verb.Fields.Get(FieldDefinitions.Pattern.Property);
            var displayName = pattern is EditorCommandPattern simplePattern ? simplePattern.Pattern : verbProperty;
            result[verbProperty] = FriendlyVerbDisplayName(displayName);
        }

        return result;
    }

    private string FriendlyVerbDisplayName(string input)
    {
        var verbs = input.Split([";", "; "], StringSplitOptions.None);
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

    public void CreateNewObject(string name, string? parent, string? alias)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Create object '{0}'", name));
        CreateNewObject(name, parent, "editor_object", alias);
        WorldModel.UndoLogger.EndTransaction();
    }

    // A Text Adventure dialogue page (see CorePages.aslx) is an ordinary object plus the
    // "dialoguepage" inherited type. Gamebook games have no such type - their pages are
    // plain objects - so this degrades to CreateNewObject there.
    public void CreateNewPage(string name, string? parent, string? alias)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Create page '{0}'", name));
        CreateNewObject(name, parent, "editor_object", alias);
        if (WorldModel.Elements.ContainsKey(ElementType.ObjectType, "dialoguepage"))
        {
            WorldModel.Elements.Get(ElementType.Object, name).Fields.AddTypeUndoable(
                WorldModel.Elements.Get(ElementType.ObjectType, "dialoguepage"));
        }
        WorldModel.UndoLogger.EndTransaction();
    }

    public bool IsDialoguePage(string elementName)
    {
        return WorldModel.Elements.ContainsKey(ElementType.Object, elementName) &&
               IsDialoguePage(WorldModel.Elements.Get(ElementType.Object, elementName));
    }

    private bool IsDialoguePage(Element o)
    {
        return o.Type == ObjectType.Object &&
               WorldModel.Elements.ContainsKey(ElementType.ObjectType, "dialoguepage") &&
               o.Fields.InheritsTypeRecursive(WorldModel.Elements.Get(ElementType.ObjectType, "dialoguepage"));
    }

    public bool IsRoom(string elementName)
    {
        return WorldModel.Elements.ContainsKey(ElementType.Object, elementName) &&
               IsRoom(WorldModel.Elements.Get(ElementType.Object, elementName));
    }

    private bool IsRoom(Element o)
    {
        return o.Type == ObjectType.Object && o.Fields.GetAsType<bool>("isroom");
    }

    public void CreateNewRoom(string name, string? parent, string? alias)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Create room '{0}'", name));
        CreateNewObject(name, parent, "editor_room", alias);
        WorldModel.UndoLogger.EndTransaction();
    }

    public string CreateNewExit(string parent)
    {
        return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit);
    }

    public string CreateNewExit(string parent, string? to, string alias, string type, bool lookonly)
    {
        return CreateNewExitInternal(parent, to, alias, true, type, lookonly);
    }

    public string CreateNewExitInternal(string parent, string? to, string alias, bool useTransaction, string type,
        bool lookonly)
    {
        if (to == null && lookonly)
        {
            return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit, [type],
                new Dictionary<string, object>
                {
                    {"to", WorldModel.Elements.Get(parent)},
                    {"alias", alias},
                    {"lookonly", lookonly},
                    {"look", ""}
                }, useTransaction);
        }

        return CreateNewAnonymousObject(parent, "exit", ObjectType.Exit, [type],
            new Dictionary<string, object>
            {
                // Only a look-only exit can be created without a destination
                {"to", WorldModel.Elements.Get(to!)},
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

    public string CreateNewCommand(string? parent)
    {
        return CreateNewAnonymousObject(parent, "command", ObjectType.Command);
    }

    public string CreateNewVerb(string? parent, bool useTransaction)
    {
        return CreateNewAnonymousObject(parent, "command", ObjectType.Command,
            ["defaultverb"],
            new Dictionary<string, object> {{"isverb", true}},
            useTransaction);
    }

    private string CreateNewAnonymousObject(string? parent, string typeName, ObjectType type,
        IList<string>? initialTypes = null, IDictionary<string, object>? initialFields = null, bool useTransaction = true)
    {
        string desc;
        Element? parentEl;
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

    private void CreateNewObject(string name, string? parent, string editorType, string? alias)
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

    private string CreateNewElement(ElementType type, string typeName, string? elementName, string? parent = null,
        IDictionary<string, object>? initialFields = null)
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

    public void CreateNewWalkthrough(string name, string? parent)
    {
        CreateNewElement(ElementType.Walkthrough, "walkthrough", name, parent);
    }

    public string CreateNewIncludedLibrary(string filename)
    {
        return CreateNewElement(ElementType.IncludedLibrary, "included library", null,
            initialFields: new Dictionary<string, object> {{"filename", filename}});
    }

    public string CreateNewTemplate(string name)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Add new template '{0}'", name));
        // Null if a base template (one defined in the game file itself) already has this name -
        // CanAddTemplate checks for that
        var newTemplate = WorldModel.AddNewTemplate(name)!;
        newTemplate.Fields[FieldDefinitions.Anonymous] = true;
        WorldModel.UndoLogger.EndTransaction();
        return newTemplate.Name;
    }

    public void CreateNewDynamicTemplate(string name)
    {
        CreateNewElement(ElementType.DynamicTemplate, "dynamic template", name,
            initialFields: new Dictionary<string, object> {{"text", ""}});
    }

    public void CreateNewType(string name)
    {
        CreateNewElement(ElementType.ObjectType, "object type", name);
    }

    public string CreateNewJavascript(string src)
    {
        return CreateNewElement(ElementType.Javascript, "javascript", null,
            initialFields: new Dictionary<string, object> {{"src", src}});
    }

    public bool CanMoveElement(string elementKey)
    {
        if (!WorldModel.Elements.ContainsKey(elementKey))
        {
            return false;
        }

        var element = WorldModel.Elements.Get(elementKey);
        return (element.ElemType == ElementType.Object && element.Type != ObjectType.Game)
               || element.ElemType == ElementType.Walkthrough;
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

        if (newParentKey != "_objects" && newParentKey != "_walkthrough" && !WorldModel.Elements.ContainsKey(newParentKey))
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

            if (newParentKey == "_walkthrough")
            {
                return false;
            }

            var newParent = WorldModel.Elements.Get(newParentKey);

            if (newParent.ElemType == ElementType.Object)
            {
                // Can't drag a parent object onto one of its own children
                return !WorldModel.ObjectContains(element, newParent);
            }
        }
        else if (element.ElemType == ElementType.Walkthrough)
        {
            if (newParentKey == "_walkthrough")
            {
                // Can always drag a walkthrough to the "Walkthrough" header to unset its parent
                return true;
            }

            if (newParentKey == "_objects")
            {
                return false;
            }

            var newParent = WorldModel.Elements.Get(newParentKey);

            if (newParent.ElemType == ElementType.Walkthrough)
            {
                // Can't drag a parent walkthrough onto one of its own children
                return !WorldModel.ObjectContains(element, newParent);
            }
        }

        return false;
    }

    public void MoveElement(string elementKey, string newParentKey)
    {
        WorldModel.UndoLogger.StartTransaction(string.Format("Move object '{0}' to '{1}'", elementKey, newParentKey));
        var element = WorldModel.Elements.Get(elementKey);
        var newParent = newParentKey is "_objects" or "_walkthrough" ? null : WorldModel.Elements.Get(newParentKey);
        element.Parent = newParent;
        WorldModel.UndoLogger.EndTransaction();
    }

    public IEnumerable<string>? GetPossibleNewObjectParentsForCurrentSelection(string elementKey)
    {
        return GetPossibleNewParentsForCurrentSelection(elementKey, "object");
    }

    public IEnumerable<string>? GetPossibleNewParentsForCurrentSelection(string elementKey, string elementTypeString)
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
            var result = new List<string>
            {
                elementKey
            };

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

    public IEnumerable<string>? GetMovePossibleParents(string elementKey)
    {
        if (!CanMoveElement(elementKey))
        {
            return null;
        }

        var element = WorldModel.Elements.Get(elementKey);

        if (element.ElemType == ElementType.Walkthrough)
        {
            return from possibleParent in WorldModel.Elements.GetElements(ElementType.Walkthrough)
                where possibleParent != element
                      && possibleParent != element.Parent
                      && !WorldModel.ObjectContains(element, possibleParent)
                orderby possibleParent.Name
                select possibleParent.Name;
        }

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
        if (string.IsNullOrEmpty(name))
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.InvalidElementNameEmpty};
        }

        if (!_validNameRegex.IsMatch(name))
        {
            var invalidChars = Regex.Matches(name, "[^\\w ]").Select(m => m.Value).Distinct().ToList();
            return new ValidationResult
            {
                Valid = false, Message = ValidationMessage.InvalidElementName,
                MessageData = FormatInvalidCharacterList(invalidChars)
            };
        }

        if (name.StartsWith(" ") || name.EndsWith(" ") || name.Contains("  "))
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.InvalidElementNameMultipleSpaces};
        }

        if (_startsWithNumberRegex.IsMatch(name))
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

    // e.g. ["-"] -> "'-'", ["-", "."] -> "'-' and '.'", ["-", ".", "!"] -> "'-', '.' and '!'"
    private static string FormatInvalidCharacterList(List<string> chars)
    {
        var quoted = chars.Select(c => $"'{c}'").ToList();
        return quoted.Count == 1
            ? quoted[0]
            : string.Join(", ", quoted.Take(quoted.Count - 1)) + " and " + quoted[^1];
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

    public bool ElementExists(string elementKey)
    {
        return WorldModel.Elements.ContainsKey(elementKey);
    }

    public void CopyElements(IEnumerable<string> elementNames)
    {
        _clipboardElements = (from name in elementNames select WorldModel.Elements.Get(name)).ToList();

        var first = true;

        foreach (var e in _clipboardElements)
        {
            if (first)
            {
                _clipboardElementType = e.ElemType;
                first = false;
            }
            else
            {
                if (_clipboardElementType != e.ElemType)
                {
                    throw new InvalidOperationException("Cannot mix element types in the clipboard");
                }
            }
        }

        _lastelementscutout = false;
    }

    public string? PasteElements(string parentName)
    {
        if (!CanPaste(parentName))
        {
            return null;
        }

        string? lastPastedElement = null;

        var parent = GetPasteParent(parentName);

        WorldModel.UndoLogger.StartTransaction("Paste");

        foreach (var e in _clipboardElements)
        {
            Element newElement;
            if (EditorStyle == EditorStyle.TextAdventure)
            {
                newElement = e.Clone(el => true, _lastelementscutout);
            }
            else if (EditorStyle == EditorStyle.GameBook)
            {
                newElement = e.Clone(el => el.Name != "player", _lastelementscutout);
            }
            else
            {
                throw new NotImplementedException("Paste not implemented for this editor type");
            }

            newElement.Parent = parent;
            lastPastedElement = newElement.Name;
        }

        WorldModel.UndoLogger.EndTransaction();

        _lastelementscutout = false;

        return lastPastedElement;
    }

    public void CutElements(IEnumerable<string> elementNames)
    {
        WorldModel.UndoLogger.StartTransaction("Cut");
        CopyElements(elementNames);
        _lastelementscutout = true;

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

    [MemberNotNullWhen(true, nameof(_clipboardElements))]
    public bool CanPaste(string parentName)
    {
        if (_clipboardElements == null || _clipboardElements.Count == 0)
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

        return parent.ElemType == _clipboardElementType;
    }

    private Element? GetPasteParent(string parentName)
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

                // Only called once CanPaste has found elements on the clipboard
                if (_clipboardElements!.Any(clipboardElement => clipboardElement == e))
                {
                    return e.Parent;
                }

                return e;
            }

            return null;
        }

        return null;
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

        var element = WorldModel.Elements.Get(elementName);
        // Engine-shipped libraries (Core.aslx, GamebookCore.aslx, and every per-language file
        // such as English.aslx) can't be deleted — the game can't load without them, and there's
        // no way to re-add one short of recreating the game. A user-uploaded custom library is
        // unaffected and stays deletable.
        if (element.ElemType == ElementType.IncludedLibrary &&
            WorldModel.IsBuiltInLibrary(element.Fields[FieldDefinitions.Filename] ?? string.Empty))
        {
            return false;
        }

        return true;
    }

    [MemberNotNullWhen(true, nameof(_clipboardScripts))]
    public bool CanPasteScript()
    {
        return _clipboardScripts != null && _clipboardScripts.Count > 0;
    }

    internal void SetClipboardScript(IEnumerable<IScript> script)
    {
        _clipboardScripts = [.. script];
        ScriptClipboardUpdated?.Invoke(this, new ScriptClipboardUpdateEventArgs { HasScript = _clipboardScripts.Count > 0 });
    }

    internal IEnumerable<IScript> GetClipboardScript()
    {
        // Script paste is only offered once scripts have been copied
        return _clipboardScripts!.AsReadOnly();
    }

    public IEnumerable<string> GetPropertyNames()
    {
        return WorldModel.GetAllAttributeNames;
    }

    public IEnumerable<string> GetExpressionEditorNames(string expressionType)
    {
        // Expression template editors always define a description
        return _expressionDefinitions.Values.Where(d => d.ExpressionType == expressionType).Select(d => d.Description!);
    }

    public IEditorDefinition? GetExpressionEditorDefinition(string expression, string expressionType)
    {
        return GetExpressionEditorDefinitionInternal(expression, expressionType);
    }

    private EditorDefinition? GetExpressionEditorDefinitionInternal(string expression, string expressionType)
    {
        // Get the Expression Editor Definition which matches the current expression.
        // e.g. if the expression is "(Got(myobject))", we want to return the Editor
        // Definition which corresponds to "(Got(#object#))" [note: this is turned into a
        // regex by the SimplePattern attribute loader]

        // Initialise only puts definitions with a pattern into _expressionDefinitions
        var candidates = from def in _expressionDefinitions.Values
            where def.ExpressionType == expressionType
            where Engine.Utility.IsRegexMatch(def.Pattern!, expression)
            select def;

        if (!candidates.Any())
        {
            return null;
        }

        var orderedCandidates = from def in candidates
            orderby Engine.Utility.GetMatchStrength(def.Pattern!, expression) descending
            select def;

        return orderedCandidates.First();
    }

    public IEditorData GetExpressionEditorData(string expression, string expressionType, IEditorData parentData)
    {
        // Callers first check GetExpressionEditorDefinition finds a definition
        return new ExpressionTemplateEditorData(expression,
            GetExpressionEditorDefinitionInternal(expression, expressionType)!, parentData);
    }

    public string? GetNewExpression(string templateName)
    {
        var definitions = from def in _expressionDefinitions.Values
            where def.Description == templateName
            select def;

        var definition = definitions.First();

        return definition.Create;
    }

    public ValidationResult Publish(string? filename, bool includeWalkthrough,
        IEnumerable<PackageIncludeFile>? includeFiles = null, Stream? outputStream = null)
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

    // Feeds the editor's expression-insert helper: built-in engine functions plus every aslx
    // Function element — the game's own, plus any from included libraries (both the always-
    // embedded Core/English libraries and ones the author added, e.g. DiceRoll from
    // CoreCombat.aslx). Unlike GetObjectNames() and friends, library elements are deliberately
    // NOT filtered out here: a library function is exactly as callable as a game-authored one
    // (WorldModel.Procedure() doesn't distinguish them), and both consumers of this list
    // (ExpressionInput's popover, ScriptEditor's Call-function picker) are typeahead-filtered,
    // so a larger pool isn't the UX problem a flat unfiltered dropdown would make it. IsLibrary
    // lets those consumers group game-authored functions ahead of library/built-in ones, since
    // a game's own functions are the more likely thing an author wants to call.
    public IEnumerable<(string Name, string[] Parameters, bool IsUserDefined, bool IsLibrary)> GetExpressionFunctions()
    {
        var builtIn = WorldModel.GetBuiltInFunctionSignatures()
            .Select(f => (f.Name, f.Parameters, IsUserDefined: false, IsLibrary: false));

        var userFunctions = WorldModel.Elements.GetElements(ElementType.Function)
            // A freshly-created function has no ParamNames field set yet (only populated by the
            // aslx <function parameters="..."> loader), so Fields[...] is null until the user
            // adds a parameter.
            .Select(e => (e.Name, e.Fields[FieldDefinitions.ParamNames]?.ToArray() ?? [], IsUserDefined: true,
                IsLibrary: e.MetaFields[MetaFieldDefinitions.Library]));

        return builtIn.Concat(userFunctions).OrderBy(f => f.Name, StringComparer.Ordinal);
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

        // resourceName comes from GetEmbeddedResources
        var stream = WorldModel.GetEmbeddedResourceStream(resourceName)!;
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

        templates.Add(resourceName, new TemplateData
        {
            ResourceName = resourceName,
            TemplateName = templateName,
            Type = templateEditorStyle
        });
    }

    public static string CreateNewGameFile(string template, string gameName)
    {
        // template is a resource name from GetAvailableTemplates
        var stream = WorldModel.GetEmbeddedResourceStream(template)!;
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
                string? regexPattern = null;

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

        // Wrap in a transaction so the swap is undoable/redoable — MetaFields
        // mutations only register with the UndoLogger while one is open.
        WorldModel.UndoLogger.StartTransaction(string.Format("Move '{0}' and '{1}'", a.Name, b.Name));
        var index = a.MetaFields[MetaFieldDefinitions.SortIndex];
        a.MetaFields[MetaFieldDefinitions.SortIndex] = b.MetaFields[MetaFieldDefinitions.SortIndex];
        b.MetaFields[MetaFieldDefinitions.SortIndex] = index;
        WorldModel.UndoLogger.EndTransaction();
    }

    // Distinct, alphabetically-ordered folder names currently assigned to any user-authored
    // Function — powers the "Move to folder" dropdown (existing folders + free-text new name).
    public IEnumerable<string> GetFunctionFolders()
    {
        return WorldModel.Elements.GetElements(ElementType.Function)
            .Select(e => e.Fields[FieldDefinitions.EditorFolder])
            .OfType<string>()
            .Where(f => f.Length > 0)
            .Distinct()
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
    }

    // Sets a Function's editor-only organisational folder (empty/null clears it back to "top
    // level"). Purely a display grouping in ElementsList/TreePanel, no effect on the game itself.
    // Assigning to a non-empty folder also relocates the function's SortIndex to sit immediately
    // after the last existing member of that folder (or at the end of the list for a brand-new
    // folder), so the display grouping's "contiguous run" assumption holds without needing the
    // frontend to do anything fancier than #2113's existing library-filename grouping.
    public void SetFunctionFolder(string key, string? folder)
    {
        var element = WorldModel.Elements.Get(key);

        WorldModel.UndoLogger.StartTransaction(string.Format("Set folder for '{0}'", element.Name));
        element.Fields[FieldDefinitions.EditorFolder] = folder;

        if (!string.IsNullOrEmpty(folder))
        {
            // GetElements(Function) includes every built-in library function too (there can be
            // hundreds, across Core.aslx/English.aslx/etc.) - moving one function only ever needs
            // to shift it past its immediate neighbours, so only the slice of the list strictly
            // between its old and new position is touched. Reassigning every function's SortIndex
            // from scratch (as this used to) rewrote hundreds of untouched library functions too,
            // each triggering a full tree-reposition event - the multi-second UI freeze this fixes.
            var all = WorldModel.Elements.GetElements(ElementType.Function)
                .OrderBy(e => e.MetaFields[MetaFieldDefinitions.SortIndex])
                .ToList();
            var oldIndex = all.IndexOf(element);
            all.RemoveAt(oldIndex);

            var lastInFolder = all.LastOrDefault(e => e.Fields[FieldDefinitions.EditorFolder] == folder);
            var newIndex = lastInFolder != null ? all.IndexOf(lastInFolder) + 1 : all.Count;
            all.Insert(newIndex, element);

            var lo = Math.Min(oldIndex, newIndex);
            var hi = Math.Max(oldIndex, newIndex);
            // Redistribute the touched slice's own original SortIndex values among its new
            // member order, rather than assigning fresh numbers - keeps every untouched element
            // (on either side of the slice) numerically unaffected.
            var originalValues = all.Skip(lo).Take(hi - lo + 1)
                .Select(e => e.MetaFields[MetaFieldDefinitions.SortIndex])
                .OrderBy(i => i)
                .ToList();
            for (var i = 0; i <= hi - lo; i++)
            {
                all[lo + i].MetaFields[MetaFieldDefinitions.SortIndex] = originalValues[i];
            }
        }

        WorldModel.UndoLogger.EndTransaction();
    }

    public bool CanMoveFunctionUp(string key) => CanMoveFunctionAdjacent(key, up: true);

    public bool CanMoveFunctionDown(string key) => CanMoveFunctionAdjacent(key, up: false);

    // Move up/down is a plain adjacent SortIndex swap (see SwapElements) with no concept of
    // folders, so unrestricted it can split a multi-member folder in two (visually duplicating
    // that folder's header) two different ways: an outsider swapping past the near edge of a
    // *different* 2+-member folder lands directly between two of its members, or a member of a
    // 2+-member folder swapping out past its own edge leaves the rest of the folder behind while
    // it keeps carrying that folder's name to its new, disconnected spot. Neither is safe, so a
    // 2+-member folder's own members may only reorder among themselves (leaving is done via
    // "Move to folder"), and no one may swap into the middle of someone else's 2+-member folder.
    // A *solo* folder member is exempt either way - relocating the only thing wearing a label
    // can never split it.
    private bool CanMoveFunctionAdjacent(string key, bool up)
    {
        var all = GetFunctionsOrderedBySortIndex();
        var element = WorldModel.Elements.Get(key);
        var index = all.IndexOf(element);
        var neighborIndex = index + (up ? -1 : 1);
        if (neighborIndex < 0 || neighborIndex >= all.Count)
        {
            return false;
        }

        var myFolder = element.Fields[FieldDefinitions.EditorFolder];
        var neighborFolder = all[neighborIndex].Fields[FieldDefinitions.EditorFolder];

        if (!string.IsNullOrEmpty(myFolder) && neighborFolder != myFolder
            && all.Count(e => e.Fields[FieldDefinitions.EditorFolder] == myFolder) > 1)
        {
            return false;
        }

        if (string.IsNullOrEmpty(neighborFolder) || neighborFolder == myFolder)
        {
            return true;
        }

        return all.Count(e => e.Fields[FieldDefinitions.EditorFolder] == neighborFolder) <= 1;
    }

    private List<Element> GetFunctionsOrderedBySortIndex()
    {
        return WorldModel.Elements.GetElements(ElementType.Function)
            .OrderBy(e => e.MetaFields[MetaFieldDefinitions.SortIndex])
            .ToList();
    }

    public bool CanMoveFunctionFolderUp(string folder) => CanMoveFunctionFolderAdjacent(folder, up: true);

    public bool CanMoveFunctionFolderDown(string folder) => CanMoveFunctionFolderAdjacent(folder, up: false);

    private bool CanMoveFunctionFolderAdjacent(string folder, bool up)
    {
        var all = GetFunctionsOrderedBySortIndex();
        var (start, end) = GetFolderRange(all, folder);
        if (start < 0)
        {
            return false;
        }

        return up ? start > 0 : end < all.Count - 1;
    }

    // A folder is always a contiguous run of same-folder functions in SortIndex order (an
    // invariant SetFunctionFolder maintains), so unlike a single function's move up/down, moving
    // the whole block can never split anything - there's nothing to guard beyond "is there
    // something on that side at all" (see CanMoveFunctionFolderAdjacent above).
    public void MoveFunctionFolderUp(string folder) => MoveFunctionFolder(folder, up: true);

    public void MoveFunctionFolderDown(string folder) => MoveFunctionFolder(folder, up: false);

    private void MoveFunctionFolder(string folder, bool up)
    {
        var all = GetFunctionsOrderedBySortIndex();
        var (start, end) = GetFolderRange(all, folder);
        if (start < 0)
        {
            return;
        }

        int neighborStart, neighborEnd;
        if (up)
        {
            if (start == 0)
            {
                return;
            }

            (neighborStart, neighborEnd) = GetAdjacentBlock(all, start - 1, towardsStart: true);
        }
        else
        {
            if (end == all.Count - 1)
            {
                return;
            }

            (neighborStart, neighborEnd) = GetAdjacentBlock(all, end + 1, towardsStart: false);
        }

        var lo = Math.Min(start, neighborStart);
        var hi = Math.Max(end, neighborEnd);

        var folderBlock = all.Skip(start).Take(end - start + 1).ToList();
        var neighborBlock = all.Skip(neighborStart).Take(neighborEnd - neighborStart + 1).ToList();
        // Moving up puts the folder before its neighbour; moving down puts it after.
        var newOrder = up ? folderBlock.Concat(neighborBlock) : neighborBlock.Concat(folderBlock);

        // Same technique as SetFunctionFolder: redistribute the touched slice's own original
        // SortIndex values among its new order, leaving every untouched element (on either side
        // of the slice) numerically unaffected.
        var originalValues = all.Skip(lo).Take(hi - lo + 1)
            .Select(e => e.MetaFields[MetaFieldDefinitions.SortIndex])
            .OrderBy(i => i)
            .ToList();

        WorldModel.UndoLogger.StartTransaction(string.Format("Move folder '{0}'", folder));
        var i = 0;
        foreach (var element in newOrder)
        {
            element.MetaFields[MetaFieldDefinitions.SortIndex] = originalValues[i];
            i++;
        }
        WorldModel.UndoLogger.EndTransaction();
    }

    // The folder's own contiguous member run - assumed contiguous (SetFunctionFolder's own
    // invariant), so the first and last matching indexes fully bound it.
    private static (int start, int end) GetFolderRange(List<Element> all, string folder)
    {
        var start = all.FindIndex(e => e.Fields[FieldDefinitions.EditorFolder] == folder);
        if (start < 0)
        {
            return (-1, -1);
        }

        var end = start;
        while (end + 1 < all.Count && all[end + 1].Fields[FieldDefinitions.EditorFolder] == folder)
        {
            end++;
        }

        return (start, end);
    }

    // The block immediately beside the folder on the given side: if that neighbour itself belongs
    // to a (different) non-empty folder, extends across that folder's whole contiguous run;
    // otherwise it's just the one lone non-foldered function at fromIndex.
    private static (int start, int end) GetAdjacentBlock(List<Element> all, int fromIndex, bool towardsStart)
    {
        var neighborFolder = all[fromIndex].Fields[FieldDefinitions.EditorFolder];
        if (string.IsNullOrEmpty(neighborFolder))
        {
            return (fromIndex, fromIndex);
        }

        var start = fromIndex;
        var end = fromIndex;
        if (towardsStart)
        {
            while (start > 0 && all[start - 1].Fields[FieldDefinitions.EditorFolder] == neighborFolder)
            {
                start--;
            }
        }
        else
        {
            while (end + 1 < all.Count && all[end + 1].Fields[FieldDefinitions.EditorFolder] == neighborFolder)
            {
                end++;
            }
        }

        return (start, end);
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
        if (_editorDefinitions != null)
        {
            _editorDefinitions.Clear();
        }

        if (_expressionDefinitions != null)
        {
            _expressionDefinitions.Clear();
        }

        if (_elementTreeStructure != null)
        {
            _elementTreeStructure.Clear();
        }

        if (_clipboardElements != null)
        {
            _clipboardElements.Clear();
        }

        if (_clipboardScripts != null)
        {
            _clipboardScripts.Clear();
        }

        if (WorldModel != null)
        {
            WorldModel.ElementFieldUpdated -= OnWorldModelElementFieldUpdated;
            WorldModel.ElementRefreshed -= OnWorldModelElementRefreshed;
            WorldModel.ElementMetaFieldUpdated -= OnWorldModelElementMetaFieldUpdated;
            WorldModel.UndoLogger.TransactionsUpdated -= UndoLogger_TransactionsUpdated;
            WorldModel.Elements.ElementRenamed -= Elements_ElementRenamed;
            WorldModel.ObjectsUpdated -= OnWorldModelObjectsUpdated;
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

    public string? GetSelectedDropDownType(IEditorControl ctl, string element)
    {
        const string noType = "*";

        // dropdowntypes controls always define their types
        var types = ctl.GetDictionary("types")!;
        var inheritedTypes = new List<string>();

        // The inherited types look like:
        // *=default; typename1=Type 1; typename2=Type2

        // Find out which of the handled types are inherited by the object

        foreach (var item in types.Where(i => i.Key != noType))
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
                return noType;
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

    internal void UpdateDictionariesReferencingRenamedObject(string oldName, string newName)
    {
        // This function is used so we can safely rename gamebook pages and have the corresponding links
        // be updated. Because these are stored as dictionary keys, they won't be updated automatically
        // as they don't point directly to the object. So we need to scan all objects with any "affectable"
        // string/script dictionaries (determined by their corresponding control having a source of "object",
        // and update keys if necessary

        var objectEditor = _editorDefinitions["object"];
        foreach (var tab in objectEditor.Tabs.Values)
        {
            foreach (var ctl in tab.Controls.Where(c => c.GetString("source") == "object"))
            {
                // So now we know that we need to scan all objects in the game which have a dictionary for
                // ctl.Attribute, because the keys to this dictionary are object names.

                foreach (var element in WorldModel.Elements.GetElements(ElementType.Object))
                {
                    // Controls with an object source always edit an attribute
                    if (element.Fields.Get(ctl.Attribute!) is IDictionary dictionary && dictionary.Contains(oldName))
                    {
                        var wrappedValue = WrapValue(dictionary);
                        if (wrappedValue is IEditableDictionary<string> editableStringDictionary)
                        {
                            editableStringDictionary.ChangeKey(oldName, newName);
                        }

                        if (wrappedValue is IEditableDictionary<IEditableScripts> editableScriptDictionary)
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
        return string.Format(ValidationMessages[result.Message], input, result.MessageData);
    }

    public List<string> AvailableBaseFonts()
    {
        if (_fontsManager == null)
        {
            _fontsManager = new FontsManager();
        }

        return _fontsManager.GetBaseFonts();
    }

    public List<string> AvailableWebFonts()
    {
        if (_fontsManager == null)
        {
            _fontsManager = new FontsManager();
        }

        return _fontsManager.GetWebFonts();
    }

    public class AddedNodeEventArgs : EventArgs
    {
        public required string Key { get; set; }
        public required string Text { get; set; }
        public string? Parent { get; set; }
        public bool IsLibraryNode { get; set; }
        public string? Filename { get; set; }
        public string? Folder { get; set; }
        public int? Position { get; set; }
        public string? NodeType { get; set; }
    }

    public class RemovedNodeEventArgs : EventArgs
    {
        public required string Key { get; set; }
    }

    public class RenamedNodeEventArgs : EventArgs
    {
        public required string OldName { get; set; }
        public required string NewName { get; set; }
    }

    public class RetitledNodeEventArgs : EventArgs
    {
        public required string Key { get; set; }
        public required string NewTitle { get; set; }
    }

    public class ShowMessageEventArgs : EventArgs
    {
        public required string Message { get; set; }
    }

    public class ElementMovedEventArgs : EventArgs
    {
        public required string Key { get; set; }
    }

    public class ScriptClipboardUpdateEventArgs : EventArgs
    {
        public bool HasScript { get; set; }
    }

    public class ElementUpdatedEventArgs : EventArgs
    {
        internal ElementUpdatedEventArgs(string element, string attribute, object? newValue, bool isUndo)
        {
            Element = element;
            Attribute = attribute;
            NewValue = newValue;
            IsUndo = isUndo;
        }

        public string Element { get; private set; }
        public string Attribute { get; private set; }
        public object? NewValue { get; private set; }
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
        public required string Key;
        public required string Title;
    }

    public class PackageIncludeFile
    {
        public required string Filename { get; set; }
        public required Stream Content { get; set; }
    }

    public struct CanAddVerbResult
    {
        public bool CanAdd;
        public string? ClashingCommand;
        public string? ClashingCommandDisplay;
    }
}