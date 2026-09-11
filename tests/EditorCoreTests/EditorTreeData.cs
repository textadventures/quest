namespace QuestViva.EditorCoreTests;

public class EditorTreeItem
{
    public required string Key;
    public EditorTreeItem? Parent;
    public required string Text;
}

public class EditorTreeData
{
    private readonly Dictionary<string, EditorTreeItem> _items = [];
    private bool _frozen = true;

    public void Clear()
    {
        _items.Clear();
    }

    public void Add(string key, string text, string? parent)
    {
        var parentItem = parent == null ? null : _items[parent];
        var newItem = new EditorTreeItem {Key = key, Parent = parentItem, Text = text};
        Add(newItem);
    }

    public void Add(EditorTreeItem item)
    {
        if (_frozen)
        {
            throw new InvalidOperationException("Can't add to tree when frozen - expected BeginUpdate event first.");
        }

        _items.Add(item.Key, item);
    }

    public void BeginUpdate()
    {
        _frozen = false;
    }

    public void EndUpdate()
    {
        _frozen = true;
    }
}