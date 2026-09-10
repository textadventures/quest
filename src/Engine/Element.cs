using QuestViva.Common;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine;

[AttributeUsage(AttributeTargets.Field)]
public class ElementTypeInfo : Attribute
{
    public string Name;

    public ElementTypeInfo(string name)
    {
        Name = name;
    }
}

public enum ElementType
{
    [ElementTypeInfo("include")] IncludedLibrary,
    [ElementTypeInfo("implied")] ImpliedType,
    [ElementTypeInfo("template")] Template,
    [ElementTypeInfo("dynamictemplate")] DynamicTemplate,
    [ElementTypeInfo("delegate")] Delegate,
    [ElementTypeInfo("object")] Object,
    [ElementTypeInfo("type")] ObjectType,
    [ElementTypeInfo("function")] Function,
    [ElementTypeInfo("editor")] Editor,
    [ElementTypeInfo("tab")] EditorTab,
    [ElementTypeInfo("control")] EditorControl,
    [ElementTypeInfo("walkthrough")] Walkthrough,
    [ElementTypeInfo("javascript")] Javascript,
    [ElementTypeInfo("timer")] Timer,
    [ElementTypeInfo("resource")] Resource,
    [ElementTypeInfo("output")] Output
}

// These are all sub-types of the "Object" element type
public enum ObjectType
{
    Object,
    Exit,
    Command,
    Game,
    TurnScript
}

// TODO: IComparable was needed for NCalc - this should be added to other types too.
public class Element : IComparable
{
    private static readonly Dictionary<ObjectType, string> TypeStrings;
    private static readonly Dictionary<string, ObjectType> MapObjectTypeStringsToElementType;
    private static readonly Dictionary<ElementType, string> ElemTypeStrings;
    private static readonly Dictionary<string, ElementType> MapElemTypeStringsToElementType;

    // Set from the "name" field as part of creating the element, so is never null once in use
    private string _name = null!;

    private Element? _parent;

    private string? _text;
    private ElementType _elemType;

    private ObjectType _type;
    internal WorldModel _worldModel;

    static Element()
    {
        TypeStrings = new Dictionary<ObjectType, string>();
        TypeStrings.Add(ObjectType.Object, "object");
        TypeStrings.Add(ObjectType.Exit, "exit");
        TypeStrings.Add(ObjectType.Command, "command");
        TypeStrings.Add(ObjectType.Game, "game");
        TypeStrings.Add(ObjectType.TurnScript, "turnscript");

        MapObjectTypeStringsToElementType = new Dictionary<string, ObjectType>();
        foreach (var item in TypeStrings)
        {
            MapObjectTypeStringsToElementType.Add(item.Value, item.Key);
        }

        ElemTypeStrings = new Dictionary<ElementType, string>();
        foreach (ElementType t in Enum.GetValues<ElementType>())
        {
            ElemTypeStrings.Add(t,
                ((ElementTypeInfo) typeof(ElementType).GetField(t.ToString())!
                    .GetCustomAttributes(typeof(ElementTypeInfo), false)[0]).Name);
        }

        MapElemTypeStringsToElementType = new Dictionary<string, ElementType>();
        foreach (var item in ElemTypeStrings)
        {
            MapElemTypeStringsToElementType.Add(item.Value, item.Key);
        }
    }

    internal Element(WorldModel worldModel)
        : this(worldModel, null)
    {
    }

    internal Element(WorldModel worldModel, Element? element)
    {
        _worldModel = worldModel;

        if (element == null)
        {
            // New element
            Fields = new Fields(worldModel, this, false);
            MetaFields = new Fields(worldModel, this, true);
        }
        else
        {
            // Clone element
            Fields = element.Fields.Clone(this);
            MetaFields = element.MetaFields.Clone(this);
        }

        Fields.AttributeChanged += Fields_AttributeChanged;
        Fields.AttributeChangedSilent += Fields_AttributeChangedSilent;
        MetaFields.AttributeChanged += MetaFields_AttributeChanged;
        MetaFields.AttributeChangedSilent += MetaFields_AttributeChangedSilent;
    }

    public Fields Fields { get; }

    public Fields MetaFields { get; }

    public string Name
    {
        get => _name;
        set => Fields.Set("name", value);
    }

    public Element? Parent
    {
        get => _parent;
        set => Fields.Set("parent", value);
    }

    public string? Text
    {
        get => _text;
        set => Fields.Set("text", value);
    }

    public ObjectType Type
    {
        get => _type;
        set
        {
            _type = value;
            Fields.Set("type", TypeString);
        }
    }

    public ElementType ElemType
    {
        get => _elemType;
        set
        {
            _elemType = value;
            Fields.Set("elementtype", ElementTypeString);
        }
    }

    internal string TypeString => TypeStrings[_type];

    internal string ElementTypeString => ElemTypeStrings[_elemType];

    internal bool Initialised { get; private set; }

    internal WorldModel WorldModel => _worldModel;

    public int CompareTo(object? obj)
    {
        return this == obj ? 0 : -1;
    }

    internal static ElementType GetElementTypeForTypeString(string typeString)
    {
        return MapElemTypeStringsToElementType[typeString];
    }

    internal static ObjectType GetObjectTypeForTypeString(string typeString)
    {
        return MapObjectTypeStringsToElementType[typeString];
    }

    internal static string GetTypeStringForElementType(ElementType type)
    {
        return ElemTypeStrings[type];
    }

    internal static string GetTypeStringForObjectType(ObjectType type)
    {
        return TypeStrings[type];
    }

    private void Fields_AttributeChangedSilent(object? sender, AttributeChangedEventArgs e)
    {
        // used by the Editor to receive notifications of updates when undoing
        if (e.InheritedTypesSet)
        {
            _worldModel.NotifyElementRefreshed(this);
        }
        else
        {
            _worldModel.NotifyElementFieldUpdate(this, e.Property!, e.Value, true);
        }
    }

    private void Fields_AttributeChanged(object? sender, AttributeChangedEventArgs e)
    {
        _worldModel.NotifyElementFieldUpdate(this, e.Property!, e.Value, false);
    }

    internal async Task SetFieldAsync(string fieldName, object? value)
    {
        var oldValue = Fields.Get(fieldName);
        var changed = value == null ? oldValue != null : !value.Equals(oldValue);
        Fields.Set(fieldName, value);
        if (changed && !_worldModel.EditMode)
        {
            var changedScriptName = "changed" + fieldName;
            if (Fields.GetAsType<IScript>(changedScriptName) is { } changedScript)
            {
                var parameters = new Parameters("oldvalue", oldValue);
                await _worldModel.RunScriptAsync(changedScript, parameters, this);
            }
        }
    }

    private void MetaFields_AttributeChanged(object? sender, AttributeChangedEventArgs e)
    {
        _worldModel.NotifyElementMetaFieldUpdate(this, e.Property!, e.Value, false);
    }

    private void MetaFields_AttributeChangedSilent(object? sender, AttributeChangedEventArgs e)
    {
        // Inherited types are only ever added to Fields, never MetaFields, so this is always a property change
        _worldModel.NotifyElementMetaFieldUpdate(this, e.Property!, e.Value, true);
    }

    public IScript? GetAction(string action)
    {
        return Fields.GetAsType<IScript>(action);
    }

    internal void SetNameFromFields(string name)
    {
        _name = name;
    }

    internal void SetParentFromFields(Element? parent)
    {
        if (parent == this)
        {
            throw new ArgumentException($"Parent of element '{Name}' cannot be set to itself");
        }

        _worldModel.Elements.UpdateParentIndex(this, _parent, parent);
        _parent = parent;
    }

    internal void SetTextFromFields(string? text)
    {
        _text = text;
    }

    internal void AddType(Element addType)
    {
        Fields.AddType(addType);
    }

    internal DebugData GetDebugData()
    {
        return Fields.GetDebugData();
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}", string.IsNullOrEmpty(TypeString) ? TypeString : TypeString[..1].ToUpper() + TypeString[1..], Name);
    }

    internal void FinishedInitialisation()
    {
        Initialised = true;
    }

    public Element Clone()
    {
        return Clone(e => true);
    }

    public Element ShallowClone()
    {
        return Clone(e => false);
    }

    public Element Clone(Func<Element, bool> canCloneChild, bool lastelementscutout = false)
    {
        var newElement = _worldModel.GetElementFactory(_elemType)
            .CloneElement(this, lastelementscutout ? Name : _worldModel.GetUniqueElementName(Name));

        if (MetaFields[MetaFieldDefinitions.Library])
        {
            newElement.MetaFields[MetaFieldDefinitions.Library] = false;
            newElement.MetaFields[MetaFieldDefinitions.Filename] = null;
        }

        // Pre-fetch all children of this element
        var children = _worldModel.Elements.GetDirectChildren(this).ToList();

        foreach (var child in children.Where(e => canCloneChild(e)))
        {
            var cloneChild = child.Clone(e => true, lastelementscutout);
            cloneChild.Parent = newElement;
        }

        return newElement;
    }
}