using QuestViva.Engine;

namespace QuestViva.EditorCore;

internal class EditorControl : IEditorControl
{
    private readonly EditorDefinition m_parent;
    private readonly Element m_source;
    private readonly EditorVisibilityHelper m_visibilityHelper;
    private WorldModel m_worldModel;

    public EditorControl(EditorDefinition parent, WorldModel worldModel, Element source)
    {
        m_parent = parent;
        m_worldModel = worldModel;
        m_source = source;
        ControlType = source.Fields.GetString("controltype");
        Caption = source.Fields.GetString("caption");
        Attribute = source.Fields.GetString("attribute");
        if (source.Fields.HasType<int>("height"))
        {
            Height = source.Fields.GetAsType<int>("height");
        }

        if (source.Fields.HasType<int>("width"))
        {
            Width = source.Fields.GetAsType<int>("width");
        }

        if (source.Fields.HasType<bool>("expand"))
        {
            Expand = source.Fields.GetAsType<bool>("expand");
        }

        m_visibilityHelper = new EditorVisibilityHelper(parent, worldModel, source);
        IsControlVisibleInSimpleMode = !source.Fields.GetAsType<bool>("advanced");
        Id = source.Name;

        if (source.Fields.HasString("filtergroup"))
        {
            parent.RegisterFilter(source.Fields.GetString("filtergroup"), source.Fields.GetString("filter"), Attribute);
        }
    }

    public string ControlType { get; }

    public string Caption { get; }

    public int? Height { get; }

    public int? Width { get; }

    public string Attribute { get; }

    public bool Expand { get; }

    public string GetString(string tag)
    {
        return m_source.Fields.GetString(tag);
    }

    public IEnumerable<string> GetListString(string tag)
    {
        return m_source.Fields.GetAsType<QuestList<string>>(tag);
    }

    public IDictionary<string, string> GetDictionary(string tag)
    {
        return m_source.Fields.GetAsType<QuestDictionary<string>>(tag);
    }

    public bool GetBool(string tag)
    {
        return m_source.Fields.GetAsType<bool>(tag);
    }

    public int? GetInt(string tag)
    {
        if (!m_source.Fields.HasType<int>(tag))
        {
            return null;
        }

        return m_source.Fields.GetAsType<int>(tag);
    }

    public double? GetDouble(string tag)
    {
        if (!m_source.Fields.HasType<double>(tag))
        {
            return null;
        }

        return m_source.Fields.GetAsType<double>(tag);
    }

    public Task<bool> IsControlVisible(IEditorData data)
    {
        return m_visibilityHelper.IsVisible(data);
    }

    public IEditorDefinition Parent => m_parent;

    public bool IsControlVisibleInSimpleMode { get; }

    public string Id { get; }
}