namespace QuestViva.EditorCore;

public class FilterOptions
{
    private readonly List<string> m_filters = new();

    public void Set(string filter, bool value)
    {
        if (value && !m_filters.Contains(filter))
        {
            m_filters.Add(filter);
        }

        if (!value && m_filters.Contains(filter))
        {
            m_filters.Remove(filter);
        }
    }

    public bool IsSet(string filter)
    {
        return m_filters.Contains(filter);
    }
}

public class AvailableFilters
{
    private readonly Dictionary<string, string> m_filterDefs = new();

    public IEnumerable<string> AllFilters => m_filterDefs.Keys;

    internal void Add(string key, string desc)
    {
        m_filterDefs.Add(key, desc);
    }

    public string Get(string key)
    {
        return m_filterDefs[key];
    }
}