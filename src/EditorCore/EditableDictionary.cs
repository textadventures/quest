using QuestViva.Engine;

namespace QuestViva.EditorCore;

public class EditableDictionary<T> : IEditableDictionary<T>, IDataWrapper
{
    private static int _count;
    private readonly EditorController _controller;

    private readonly QuestDictionary<T> _source;
    private readonly Dictionary<string, IEditableListItem<T>> _wrappedItems = new();

    public EditableDictionary(EditorController controller, QuestDictionary<T> source)
    {
        _count++;
        Id = "dictionary" + _count;

        _controller = controller;
        _source = source;
        _source.Added += OnSourceAdded;
        _source.Removed += OnSourceRemoved;
        PopulateWrappedItems();
    }

    // currently unused
    public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated
    {
        add { }
        remove { }
    }

    public object GetUnderlyingValue()
    {
        return _source;
    }

    public string DisplayString()
    {
        throw new NotImplementedException();
    }

    public event EventHandler<EditableListUpdatedEventArgs<T>>? Added;
    public event EventHandler<EditableListUpdatedEventArgs<T>>? Removed;

    // currently unused - we currently only use EditableDictionary<string>, and we don't
    // use the Updated event as the same behaviour is implemented with a combination of Added and Removed.
    public event EventHandler<EditableListUpdatedEventArgs<T>> Updated
    {
        add { }
        remove { }
    }

    public IDictionary<string, IEditableListItem<T>> Items => _wrappedItems;

    public void Add(string key, T value)
    {
        string? undoEntry = null;
        if (typeof(T) == typeof(string))
        {
            undoEntry = string.Format("Add '{0}={1}'", key, value as string);
        }

        if (undoEntry == null)
        {
            throw new InvalidOperationException("Unknown dictionary type");
        }

        _controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
        _source.Add(key, value, UpdateSource.User);
        _controller.WorldModel.UndoLogger.EndTransaction();
    }

    public void Remove(params string[] keys)
    {
        string? undoEntry = null;
        if (typeof(T) == typeof(string))
        {
            undoEntry = string.Format("Remove '{0}'", string.Join(",", keys));
        }

        if (undoEntry == null)
        {
            throw new InvalidOperationException("Unknown list type");
        }

        _controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
        foreach (var key in keys)
        {
            _source.Remove(key, UpdateSource.User);
        }

        _controller.WorldModel.UndoLogger.EndTransaction();
    }

    public ValidationResult CanAdd(string key)
    {
        if (_source.ContainsKey(key))
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.ItemAlreadyExists};
        }

        return new ValidationResult {Valid = true};
    }

    public void Update(string key, T value)
    {
        var index = _source.IndexOfKey(key);
        _source.Remove(key, UpdateSource.User);
        _source.Add(key, value, UpdateSource.User, index);
    }

    public void ChangeKey(string oldKey, string newKey)
    {
        var index = _source.IndexOfKey(oldKey);
        var value = _source[oldKey];
        _source.Remove(oldKey, UpdateSource.User);
        _source.Add(newKey, value, UpdateSource.User, index);
    }

    public string Id { get; }

    private void PopulateWrappedItems()
    {
        _wrappedItems.Clear();
        var index = 0;

        foreach (var item in _source)
        {
            AddWrappedItem(item.Key, item.Value, EditorUpdateSource.System, index);
            index++;
        }
    }

    private void AddWrappedItem(string key, T value, EditorUpdateSource source, int index)
    {
        IEditableListItem<T> wrappedValue = new EditableListItem<T>(key, value);
        _wrappedItems.Add(key, wrappedValue);

        if (Added != null)
        {
            Added(this,
                new EditableListUpdatedEventArgs<T> {UpdatedItem = wrappedValue, Index = index, Source = source});
        }
    }

    private void RemoveWrappedItem(IEditableListItem<T> item, EditorUpdateSource source, int index)
    {
        _wrappedItems.Remove(item.Key);
        if (Removed != null)
        {
            Removed(this, new EditableListUpdatedEventArgs<T> {UpdatedItem = item, Index = index, Source = source});
        }
    }

    private void OnSourceAdded(object? sender, QuestDictionaryUpdatedEventArgs<T> e)
    {
        AddWrappedItem(e.Key, e.Item, (EditorUpdateSource) e.Source, e.Index);
    }

    private void OnSourceRemoved(object? sender, QuestDictionaryUpdatedEventArgs<T> e)
    {
        RemoveWrappedItem(_wrappedItems[e.Key], (EditorUpdateSource) e.Source, e.Index);
    }

    #region Static DataWrapper

    private static readonly EditableDataWrapper<QuestDictionary<T>, EditableDictionary<T>> Wrapper;

    static EditableDictionary()
    {
        Wrapper = new EditableDataWrapper<QuestDictionary<T>, EditableDictionary<T>>(GetNewInstance);
    }

    public static EditableDictionary<T> GetInstance(EditorController controller, QuestDictionary<T> list)
    {
        return Wrapper.GetInstance(controller, list);
    }

    private static EditableDictionary<T> GetNewInstance(EditorController controller, QuestDictionary<T> list)
    {
        return new EditableDictionary<T>(controller, list);
    }

    public static void Clear()
    {
        Wrapper.Clear();
    }

    #endregion
}