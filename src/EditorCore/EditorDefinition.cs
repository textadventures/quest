using QuestViva.Engine;

namespace QuestViva.EditorCore;

internal class EditorDefinition : IEditorDefinition
{
    private readonly Dictionary<string, IEditorControl> m_controls;
    private readonly Dictionary<string, FilterGroup> m_filterGroups = new();

    private readonly Dictionary<string, IEditorTab> m_tabs;

    public EditorDefinition(WorldModel worldModel, Element source)
    {
        m_tabs = new Dictionary<string, IEditorTab>();
        m_controls = new Dictionary<string, IEditorControl>();
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
                m_tabs.Add(e.Name, new EditorTab(this, worldModel, e));
            }
        }

        foreach (var e in worldModel.Elements.GetElements(ElementType.EditorControl))
        {
            if (e.Parent == source)
            {
                m_controls.Add(e.Name, new EditorControl(this, worldModel, e));
            }
        }
    }

    public string AppliesTo { get; }

    public string Pattern { get; }

    public string Create { get; }

    public string ExpressionType { get; }

    public string OriginalPattern { get; }

    public string Description { get; }

    public IDictionary<string, IEditorTab> Tabs => m_tabs;

    public IEnumerable<IEditorControl> Controls => m_controls.Values;

    public string GetDefaultFilterName(string filterGroupName, IEditorData data)
    {
        var filterGroup = m_filterGroups[filterGroupName];
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
        if (!m_filterGroups.ContainsKey(filterGroupName))
        {
            m_filterGroups.Add(filterGroupName, new FilterGroup(filterGroupName));
        }

        var filterGroup = m_filterGroups[filterGroupName];

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

        public List<string> Attributes { get; } = new();

        public string Name { get; }
    }

    private class FilterGroup
    {
        public FilterGroup(string name)
        {
            Name = name;
        }

        public Dictionary<string, Filter> Filters { get; } = new();

        public string Name { get; }
    }
}