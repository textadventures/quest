namespace QuestViva.EditorCoreTests;

public class EditorTreeItem
{
    public string Key;
    public EditorTreeItem Parent;
    public string Text;
}

public class EditorTreeData
{
    private readonly Dictionary<string, EditorTreeItem> m_items = new();
    private bool m_frozen = true;

    public void Clear()
    {
        m_items.Clear();
    }

    public void Add(string key, string text, string parent)
    {
        var parentItem = parent == null ? null : m_items[parent];
        var newItem = new EditorTreeItem {Key = key, Parent = parentItem, Text = text};
        Add(newItem);
    }

    public void Add(EditorTreeItem item)
    {
        if (m_frozen)
        {
            throw new InvalidOperationException("Can't add to tree when frozen - expected BeginUpdate event first.");
        }

        m_items.Add(item.Key, item);
    }

    public void BeginUpdate()
    {
        m_frozen = false;
    }

    public void EndUpdate()
    {
        m_frozen = true;
    }
}