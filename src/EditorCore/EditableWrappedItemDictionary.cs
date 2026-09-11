using QuestViva.Engine;

namespace QuestViva.EditorCore;

// This class is used to create an IEditableDictionary wrapper where the values in the dictionary are
// themselves wrapped.
//      For example: TSource=IScript, TWrapped=IEditableScripts
//      Then we can expose an IEditableDictionary<IEditableScripts> which wraps a QuestDictionary<IScript>
public class EditableWrappedItemDictionary<TSource, TWrapped> : IEditableDictionary<TWrapped>, IDataWrapper
    where TWrapped : IDataWrapper
    where TSource : class
{
    private static int _count;
    private readonly EditorController _controller;

    private readonly QuestDictionary<TSource> _source;
    private readonly Dictionary<string, IEditableListItem<TWrapped>> _wrappedItems = [];
    private readonly Dictionary<TWrapped, IEditableListItem<TWrapped>> _wrappedItemsLookup = [];

    public EditableWrappedItemDictionary(EditorController controller, QuestDictionary<TSource> source)
    {
        _count++;
        Id = "wrapdictionary" + _count;

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
        return string.Format("(Script Dictionary: {0} items)", _source.Count);
    }

    public event EventHandler<EditableListUpdatedEventArgs<TWrapped>>? Added;
    public event EventHandler<EditableListUpdatedEventArgs<TWrapped>>? Removed;

    public event EventHandler<EditableListUpdatedEventArgs<TWrapped>>? Updated;

    public IDictionary<string, IEditableListItem<TWrapped>> Items => _wrappedItems;

    // TO DO: Public methods shouldn't be starting/ending transactions here - that should be up to the caller

    public void Add(string key, TWrapped value)
    {
        var undoEntry = string.Format("Add '{0}={1}'", key, value == null ? string.Empty : value.DisplayString());

        _controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
        _source.Add(key, UnwrapValue(value), UpdateSource.User);
        _controller.WorldModel.UndoLogger.EndTransaction();
    }

    public void Remove(params string[] keys)
    {
        var undoEntry = string.Format("Remove '{0}'", string.Join(",", keys));

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

    public void Update(string key, TWrapped value)
    {
        var index = _source.IndexOfKey(key);
        _source.Remove(key, UpdateSource.User);
        _source.Add(key, UnwrapValue(value), UpdateSource.User, index);
    }

    // it is up to the caller of this method to start/end a transaction (this should also be the case eventually
    // for the other public methods)

    public void ChangeKey(string oldKey, string newKey)
    {
        //_controller.WorldModel.UndoLogger.StartTransaction(string.Format("Update key '{0}' to '{1}'", oldKey, newKey));
        var index = _source.IndexOfKey(oldKey);
        var value = _source[oldKey];
        _source.Remove(oldKey, UpdateSource.User);
        _source.Add(newKey, value, UpdateSource.User, index);
        //_controller.WorldModel.UndoLogger.EndTransaction();
    }

    public string Id { get; }

    private void PopulateWrappedItems()
    {
        _wrappedItems.Clear();
        var index = 0;

        foreach (var item in _source)
        {
            AddWrappedItem(item.Key, WrapValue(item.Value), EditorUpdateSource.System, index);
            index++;
        }
    }

    private TWrapped WrapValue(TSource source)
    {
        return (TWrapped) _controller.WrapValue(source);
    }

    private TSource UnwrapValue(TWrapped? wrapped)
    {
        if (wrapped == null)
        {
            // Stored as a null value in the underlying dictionary
            return null!;
        }

        return (TSource) wrapped.GetUnderlyingValue()!;
    }

    private void AddWrappedItem(string key, TWrapped value, EditorUpdateSource source, int index)
    {
        IEditableListItem<TWrapped> wrappedValue = new EditableListItem<TWrapped>(key, value);
        _wrappedItems.Add(key, wrappedValue);
        _wrappedItemsLookup.Add(value, wrappedValue);
        value.UnderlyingValueUpdated += WrappedUnderlyingValueUpdated;

        Added?.Invoke(this,
    new EditableListUpdatedEventArgs<TWrapped>
    { UpdatedItem = wrappedValue, Index = index, Source = source });
    }

    private void RemoveWrappedItem(IEditableListItem<TWrapped> item, EditorUpdateSource source, int index)
    {
        _wrappedItems[item.Key].Value.UnderlyingValueUpdated -= WrappedUnderlyingValueUpdated;
        _wrappedItemsLookup.Remove(_wrappedItems[item.Key].Value);
        _wrappedItems.Remove(item.Key);
        Removed?.Invoke(this,
    new EditableListUpdatedEventArgs<TWrapped> { UpdatedItem = item, Index = index, Source = source });
    }

    private void WrappedUnderlyingValueUpdated(object? sender, DataWrapperUpdatedEventArgs e)
    {
        if (Updated != null)
        {
            // sender will be the underlying wrapped value that has been updated. e.g. an IEditableScripts item
            var updatedItem = (TWrapped) sender!;

            Updated(this, new EditableListUpdatedEventArgs<TWrapped>
            {
                UpdatedItem = _wrappedItemsLookup[updatedItem],
                Index = _source.IndexOfKey(_wrappedItemsLookup[updatedItem].Key)
            });
        }
    }

    private void OnSourceAdded(object? sender, QuestDictionaryUpdatedEventArgs<TSource> e)
    {
        AddWrappedItem(e.Key, WrapValue(e.Item), (EditorUpdateSource) e.Source, e.Index);
    }

    private void OnSourceRemoved(object? sender, QuestDictionaryUpdatedEventArgs<TSource> e)
    {
        RemoveWrappedItem(_wrappedItems[e.Key], (EditorUpdateSource) e.Source, e.Index);
    }

    #region Static DataWrapper

    private static readonly
        EditableDataWrapper<QuestDictionary<TSource>, EditableWrappedItemDictionary<TSource, TWrapped>> Wrapper;

    static EditableWrappedItemDictionary()
    {
        Wrapper =
            new EditableDataWrapper<QuestDictionary<TSource>, EditableWrappedItemDictionary<TSource, TWrapped>>(
                GetNewInstance);
    }

    public static EditableWrappedItemDictionary<TSource, TWrapped> GetInstance(EditorController controller,
        QuestDictionary<TSource> list)
    {
        return Wrapper.GetInstance(controller, list);
    }

    private static EditableWrappedItemDictionary<TSource, TWrapped> GetNewInstance(EditorController controller,
        QuestDictionary<TSource> list)
    {
        return new EditableWrappedItemDictionary<TSource, TWrapped>(controller, list);
    }

    public static void Clear()
    {
        Wrapper.Clear();
    }

    #endregion
}