using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    internal class EditorDefinition : IEditorDefinition
    {
        private class Filter
        {
            private List<string> m_attributes = new List<string>();
            private string m_filterName;

            public List<string> Attributes { get { return m_attributes; } }
            public string Name { get { return m_filterName; } }

            public Filter(string name)
            {
                m_filterName = name;
            }
        }

        private class FilterGroup
        {
            private Dictionary<string, Filter> m_filters = new Dictionary<string, Filter>();
            private string m_filterGroupName;

            public Dictionary<string, Filter> Filters { get { return m_filters; } }
            public string Name { get { return m_filterGroupName; } }

            public FilterGroup(string name)
            {
                m_filterGroupName = name;
            }
        }

        private Dictionary<string, IEditorTab> m_tabs = null;
        private Dictionary<string, IEditorControl> m_controls = null;
        private string m_appliesTo = null;
        private string m_pattern = null;
        private string m_originalPattern = null;
        private string m_description = null;
        private string m_create = null;
        private string m_expressionType = null;
        private Dictionary<string, FilterGroup> m_filterGroups = new Dictionary<string, FilterGroup>();

        public EditorDefinition(WorldModel worldModel, Element source)
        {
            m_tabs = new Dictionary<string, IEditorTab>();
            m_controls = new Dictionary<string, IEditorControl>();
            m_appliesTo = source.Fields.GetString("appliesto");
            m_pattern = source.Fields.GetString("pattern");
            m_originalPattern = source.Fields.GetString(FieldDefinitions.OriginalPattern.Property);
            m_description = source.Fields.GetString("description");
            m_create = source.Fields.GetString("create");
            m_expressionType = source.Fields.GetString("expressiontype");

            foreach (Element e in worldModel.Elements.GetElements(ElementType.EditorTab))
            {
                if (e.Parent == source)
                {
                    m_tabs.Add(e.Name, new EditorTab(this, worldModel, e));
                }
            }

            foreach (Element e in worldModel.Elements.GetElements(ElementType.EditorControl))
            {
                if (e.Parent == source)
                {
                    m_controls.Add(e.Name, new EditorControl(this, worldModel, e));
                }
            }
        }

        public string AppliesTo
        {
            get { return m_appliesTo; }
        }

        public string Pattern
        {
            get { return m_pattern; }
        }

        public string OriginalPattern
        {
            get { return m_originalPattern; }
        }

        public string Description
        {
            get { return m_description; }
        }

        public string Create
        {
            get { return m_create; }
        }

        public string ExpressionType
        {
            get { return m_expressionType; }
        }

        public IDictionary<string, IEditorTab> Tabs
        {
            get { return m_tabs; }
        }

        public IEnumerable<IEditorControl> Controls
        {
            get { return m_controls.Values; }
        }

        internal void RegisterFilter(string filterGroupName, string filterName, string attribute)
        {
            if (!m_filterGroups.ContainsKey(filterGroupName))
            {
                m_filterGroups.Add(filterGroupName, new FilterGroup(filterGroupName));
            }

            FilterGroup filterGroup = m_filterGroups[filterGroupName];

            if (!filterGroup.Filters.ContainsKey(filterName))
            {
                filterGroup.Filters.Add(filterName, new Filter(filterName));
            }

            Filter filter = filterGroup.Filters[filterName];
            filter.Attributes.Add(attribute);
        }

        public string GetDefaultFilterName(string filterGroupName, IEditorData data)
        {
            FilterGroup filterGroup = m_filterGroups[filterGroupName];
            List<Filter> candidates = new List<Filter>();

            foreach (Filter filter in filterGroup.Filters.Values)
            {
                foreach (string attribute in filter.Attributes)
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
    }
}
