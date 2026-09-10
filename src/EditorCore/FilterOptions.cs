namespace QuestViva.EditorCore;

public class FilterOptions
{
    private readonly List<string> _filters = new();

    public void Set(string filter, bool value)
    {
        if (value && !_filters.Contains(filter))
        {
            _filters.Add(filter);
        }

        if (!value && _filters.Contains(filter))
        {
            _filters.Remove(filter);
        }
    }

    public bool IsSet(string filter)
    {
        return _filters.Contains(filter);
    }
}

public class AvailableFilters
{
    private readonly Dictionary<string, string> _filterDefs = new();

    public IEnumerable<string> AllFilters => _filterDefs.Keys;

    internal void Add(string key, string desc)
    {
        _filterDefs.Add(key, desc);
    }

    public string Get(string key)
    {
        return _filterDefs[key];
    }
}