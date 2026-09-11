namespace QuestViva.EditorCore;

public class FilterOptions
{
    private readonly List<string> _filters = [];

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
