using System.Collections.Generic;

namespace QuestViva.EditorCore
{
    public class FilterOptions
    {
        private List<string> m_filters = new List<string>();

        public void Set(string filter, bool value)
        {
            if (value && !m_filters.Contains(filter)) m_filters.Add(filter);
            if (!value && m_filters.Contains(filter)) m_filters.Remove(filter);
        }

        public bool IsSet(string filter)
        {
            return m_filters.Contains(filter);
        }
    }

    public class AvailableFilters
    {
        private Dictionary<string, string> m_filterDefs = new Dictionary<string, string>();

        internal void Add(string key, string desc)
        {
            m_filterDefs.Add(key, desc);
        }

        public string Get(string key)
        {
            return m_filterDefs[key];
        }

        public IEnumerable<string> AllFilters
        {
            get
            {
                return m_filterDefs.Keys;
            }
        }
    }
}
