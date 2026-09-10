using QuestViva.Engine;

namespace QuestViva.EditorCore;

internal class EditorControl : IEditorControl
{
    private readonly EditorDefinition _parent;
    private readonly Element _source;
    private readonly EditorVisibilityHelper _visibilityHelper;
    private WorldModel _worldModel;

    public EditorControl(EditorDefinition parent, WorldModel worldModel, Element source)
    {
        _parent = parent;
        _worldModel = worldModel;
        _source = source;
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

        _visibilityHelper = new EditorVisibilityHelper(parent, worldModel, source);
        IsControlVisibleInSimpleMode = !source.Fields.GetAsType<bool>("advanced");
        Id = source.Name;

        if (source.Fields.GetString("filtergroup") is { } filterGroup)
        {
            // A control in a filter group always names its filter and attribute
            parent.RegisterFilter(filterGroup, source.Fields.GetString("filter")!, Attribute!);
        }
    }

    public string? ControlType { get; }

    public string? Caption { get; }

    public int? Height { get; }

    public int? Width { get; }

    public string? Attribute { get; }

    public bool Expand { get; }

    public string? GetString(string tag)
    {
        return _source.Fields.GetString(tag);
    }

    public IEnumerable<string>? GetListString(string tag)
    {
        return _source.Fields.GetAsType<QuestList<string>>(tag);
    }

    public IDictionary<string, string>? GetDictionary(string tag)
    {
        return _source.Fields.GetAsType<QuestDictionary<string>>(tag);
    }

    public bool GetBool(string tag)
    {
        return _source.Fields.GetAsType<bool>(tag);
    }

    public int? GetInt(string tag)
    {
        if (!_source.Fields.HasType<int>(tag))
        {
            return null;
        }

        return _source.Fields.GetAsType<int>(tag);
    }

    public double? GetDouble(string tag)
    {
        if (!_source.Fields.HasType<double>(tag))
        {
            return null;
        }

        return _source.Fields.GetAsType<double>(tag);
    }

    public Task<bool> IsControlVisible(IEditorData data)
    {
        return _visibilityHelper.IsVisible(data);
    }

    public bool IsControlVisibleSync(IEditorData data)
    {
        return _visibilityHelper.IsVisibleIgnoringExpression(data);
    }

    public IEditorDefinition Parent => _parent;

    public bool IsControlVisibleInSimpleMode { get; }

    public string Id { get; }
}