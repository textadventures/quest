using QuestViva.Engine;

namespace QuestViva.EditorCore;

internal class EditorDefinition : IEditorDefinition
{
    private readonly Dictionary<string, IEditorControl> _controls;
    private readonly Dictionary<string, FilterGroup> _filterGroups = [];

    private readonly Dictionary<string, IEditorTab> _tabs;

    public EditorDefinition(WorldModel worldModel, Element source)
    {
        _tabs = [];
        _controls = [];
        AppliesTo = source.Fields.GetString("appliesto");
        Pattern = source.Fields.GetString("pattern");
        OriginalPattern = source.Fields.GetString(FieldDefinitions.OriginalPattern.Property);
        Description = source.Fields.GetString("description");
        Create = source.Fields.GetString("create");
        ExpressionType = source.Fields.GetString("expressiontype");

        foreach (var e in worldModel.Elements.GetElements(ElementType.EditorTab))
        {
            if (e.Parent == source)
            {
                _tabs.Add(e.Name, new EditorTab(this, worldModel, e));
            }
        }

        foreach (var e in worldModel.Elements.GetElements(ElementType.EditorControl))
        {
            if (e.Parent == source)
            {
                _controls.Add(e.Name, new EditorControl(this, worldModel, e));
            }
        }
    }

    public string? AppliesTo { get; }

    public string? Pattern { get; }

    public string? Create { get; }

    public string? ExpressionType { get; }

    public string? OriginalPattern { get; }

    public string? Description { get; }

    public IDictionary<string, IEditorTab> Tabs => _tabs;

    public IEnumerable<IEditorControl> Controls => _controls.Values;

    public string GetDefaultFilterName(string filterGroupName, IEditorData data)
    {
        var filterGroup = _filterGroups[filterGroupName];
        var candidates = new List<Filter>();

        foreach (var filter in filterGroup.Filters.Values)
        {
            foreach (var attribute in filter.Attributes)
            {
                if (data.GetAttribute(attribute) != null)
                {
                    candidates.Add(filter);
                    break;
                }
            }
        }

        // If there is only one candidate then we have our result
        if (candidates.Count == 1)
        {
            return candidates[0].Name;
        }

        // Otherwise just default to the first filter
        return filterGroup.Filters.First().Value.Name;
    }

    internal void RegisterFilter(string filterGroupName, string filterName, string attribute)
    {
        if (!_filterGroups.ContainsKey(filterGroupName))
        {
            _filterGroups.Add(filterGroupName, new FilterGroup(filterGroupName));
        }

        var filterGroup = _filterGroups[filterGroupName];

        if (!filterGroup.Filters.ContainsKey(filterName))
        {
            filterGroup.Filters.Add(filterName, new Filter(filterName));
        }

        var filter = filterGroup.Filters[filterName];
        filter.Attributes.Add(attribute);
    }

    private class Filter
    {
        public Filter(string name)
        {
            Name = name;
        }

        public List<string> Attributes { get; } = [];

        public string Name { get; }
    }

    private class FilterGroup
    {
        public FilterGroup(string name)
        {
            Name = name;
        }

        public Dictionary<string, Filter> Filters { get; } = [];

        public string Name { get; }
    }
}