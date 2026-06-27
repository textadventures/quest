#nullable disable
using QuestViva.Common;
using QuestViva.Engine.Scripts;
using QuestViva.Utility;

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
    private static readonly Dictionary<ObjectType, string> s_typeStrings;
    private static readonly Dictionary<string, ObjectType> s_mapObjectTypeStringsToElementType;
    private static readonly Dictionary<ElementType, string> s_elemTypeStrings;
    private static readonly Dictionary<string, ElementType> s_mapElemTypeStringsToElementType;

    private string _name;

    private Element _parent;

    private string _text;
    private ElementType m_elemType;

    private ObjectType m_type;
    internal WorldModel m_worldModel;

    static Element()
    {
        s_typeStrings = new Dictionary<ObjectType, string>();
        s_typeStrings.Add(ObjectType.Object, "object");
        s_typeStrings.Add(ObjectType.Exit, "exit");
        s_typeStrings.Add(ObjectType.Command, "command");
        s_typeStrings.Add(ObjectType.Game, "game");
        s_typeStrings.Add(ObjectType.TurnScript, "turnscript");

        s_mapObjectTypeStringsToElementType = new Dictionary<string, ObjectType>();
        foreach (var item in s_typeStrings)
        {
            s_mapObjectTypeStringsToElementType.Add(item.Value, item.Key);
        }

        s_elemTypeStrings = new Dictionary<ElementType, string>();
        foreach (ElementType t in Enum.GetValues(typeof(ElementType)))
        {
            s_elemTypeStrings.Add(t,
                ((ElementTypeInfo) typeof(ElementType).GetField(t.ToString())
                    .GetCustomAttributes(typeof(ElementTypeInfo), false)[0]).Name);
        }

        s_mapElemTypeStringsToElementType = new Dictionary<string, ElementType>();
        foreach (var item in s_elemTypeStrings)
        {
            s_mapElemTypeStringsToElementType.Add(item.Value, item.Key);
        }
    }

    internal Element(WorldModel worldModel)
        : this(worldModel, null)
    {
    }

    internal Element(WorldModel worldModel, Element element)
    {
        m_worldModel = worldModel;

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

    public Element Parent
    {
        get => _parent;
        set => Fields.Set("parent", value);
    }

    public string Text
    {
        get => _text;
        set => Fields.Set("text", value);
    }

    public ObjectType Type
    {
        get => m_type;
        set
        {
            m_type = value;
            Fields.Set("type", TypeString);
        }
    }

    public ElementType ElemType
    {
        get => m_elemType;
        set
        {
            m_elemType = value;
            Fields.Set("elementtype", ElementTypeString);
        }
    }

    internal string TypeString => s_typeStrings[m_type];

    internal string ElementTypeString => s_elemTypeStrings[m_elemType];

    internal bool Initialised { get; private set; }

    internal WorldModel WorldModel => m_worldModel;

    public int CompareTo(object obj)
    {
        return this == obj ? 0 : -1;
    }

    internal static ElementType GetElementTypeForTypeString(string typeString)
    {
        return s_mapElemTypeStringsToElementType[typeString];
    }

    internal static ObjectType GetObjectTypeForTypeString(string typeString)
    {
        return s_mapObjectTypeStringsToElementType[typeString];
    }

    internal static string GetTypeStringForElementType(ElementType type)
    {
        return s_elemTypeStrings[type];
    }

    internal static string GetTypeStringForObjectType(ObjectType type)
    {
        return s_typeStrings[type];
    }

    private void Fields_AttributeChangedSilent(object sender, AttributeChangedEventArgs e)
    {
        // used by the Editor to receive notifications of updates when undoing
        if (e.InheritedTypesSet)
        {
            m_worldModel.NotifyElementRefreshed(this);
        }
        else
        {
            m_worldModel.NotifyElementFieldUpdate(this, e.Property, e.Value, true);
        }
    }

    private void Fields_AttributeChanged(object sender, AttributeChangedEventArgs e)
    {
        m_worldModel.NotifyElementFieldUpdate(this, e.Property, e.Value, false);
    }

    internal async Task SetFieldAsync(string fieldName, object value)
    {
        var oldValue = Fields.Get(fieldName);
        Fields.Set(fieldName, value);
        if (!m_worldModel.EditMode)
        {
            var changedScriptName = "changed" + fieldName;
            if (Fields.HasType<IScript>(changedScriptName))
            {
                var parameters = new Parameters("oldvalue", oldValue);
                await m_worldModel.RunScriptAsync(Fields.GetAsType<IScript>(changedScriptName), parameters, this);
            }
        }
    }

    private void MetaFields_AttributeChanged(object sender, AttributeChangedEventArgs e)
    {
        m_worldModel.NotifyElementMetaFieldUpdate(this, e.Property, e.Value, false);
    }

    private void MetaFields_AttributeChangedSilent(object sender, AttributeChangedEventArgs e)
    {
        m_worldModel.NotifyElementMetaFieldUpdate(this, e.Property, e.Value, true);
    }

    public IScript GetAction(string action)
    {
        return Fields.GetAsType<IScript>(action);
    }

    internal void SetNameFromFields(string name)
    {
        _name = name;
    }

    internal void SetParentFromFields(Element parent)
    {
        if (parent == this)
        {
            throw new ArgumentException($"Parent of element '{Name}' cannot be set to itself");
        }

        _parent = parent;
    }

    internal void SetTextFromFields(string text)
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
        return string.Format("{0}: {1}", Strings.CapFirst(TypeString), Name);
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
        var newElement = m_worldModel.GetElementFactory(m_elemType)
            .CloneElement(this, lastelementscutout ? Name : m_worldModel.GetUniqueElementName(Name));

        if (MetaFields[MetaFieldDefinitions.Library])
        {
            newElement.MetaFields[MetaFieldDefinitions.Library] = false;
            newElement.MetaFields[MetaFieldDefinitions.Filename] = null;
        }

        // Pre-fetch all children of this element
        var children = m_worldModel.Elements.GetDirectChildren(this).ToList();

        foreach (var child in children.Where(e => canCloneChild(e)))
        {
            var cloneChild = child.Clone(e => true, lastelementscutout);
            cloneChild.Parent = newElement;
        }

        return newElement;
    }
}