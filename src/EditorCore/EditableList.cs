using System.Collections;
using System.Collections.Specialized;
using QuestViva.Engine;

namespace QuestViva.EditorCore;

public class EditableList<T> : IEditableList<T>, IDataWrapper, INotifyCollectionChanged
{
    private readonly EditorController _controller;

    private readonly QuestList<T> _source;
    private readonly List<string> _wrappedItemKeys = new();
    private readonly Dictionary<string, IEditableListItem<T>> _wrappedItems = new();
    private readonly List<IEditableListItem<T>> _wrappedItemsList = new();
    private int _nextId;

    public EditableList(EditorController controller, QuestList<T> source)
    {
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

    public event EventHandler<EditableListUpdatedEventArgs<T>> Added;
    public event EventHandler<EditableListUpdatedEventArgs<T>> Removed;

    public IDictionary<string, IEditableListItem<T>> Items => _wrappedItems;

    public void Add(T item)
    {
        string undoEntry = null;
        if (typeof(T) == typeof(string))
        {
            undoEntry = string.Format("Add '{0}'", item as string);
        }

        if (undoEntry == null)
        {
            throw new InvalidOperationException("Unknown list type");
        }

        _controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
        AddInternal(item);
        _controller.WorldModel.UndoLogger.EndTransaction();
    }

    public void Remove(params string[] keys)
    {
        string undoEntry = null;
        if (typeof(T) == typeof(string))
        {
            undoEntry = "Remove items from list";
        }

        if (undoEntry == null)
        {
            throw new InvalidOperationException("Unknown list type");
        }

        _controller.WorldModel.UndoLogger.StartTransaction(undoEntry);

        foreach (var key in keys)
        {
            _source.RemoveByIndex(_wrappedItemKeys.IndexOf(key), UpdateSource.User);
        }

        _controller.WorldModel.UndoLogger.EndTransaction();
    }

    public void Update(int index, T item)
    {
        string undoEntry = null;
        if (typeof(T) == typeof(string))
        {
            undoEntry = string.Format("Update '{0}'", item as string);
        }

        if (undoEntry == null)
        {
            throw new InvalidOperationException("Unknown list type");
        }

        _controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
        _source.Remove(_source[index], UpdateSource.User, index);
        _source.Add(item, UpdateSource.User, index);
        _controller.WorldModel.UndoLogger.EndTransaction();
    }

    public void Update(string key, T item)
    {
        Update(_wrappedItemKeys.IndexOf(key), item);
    }

    public IEnumerable<KeyValuePair<string, string>> DisplayItems
    {
        get
        {
            var result = new Dictionary<string, string>();

            foreach (var item in _wrappedItems)
            {
                // TO DO: We will need some kind of projection function for non-string T's
                result.Add(item.Key, item.Value.Value as string);
            }

            return result;
        }
    }

    public ValidationResult CanAdd(T item)
    {
        // Commented this section out as it is valid to have the same item multiple times in a list,
        // for example for walkthroughs.
        //if (_source.Contains(item))
        //{
        //    return new ValidationResult { Valid = false, Message = ValidationMessage.ItemAlreadyExists };
        //}

        return new ValidationResult {Valid = true};
    }

    public bool Locked => _source.Locked;

    public IEditableList<T> Clone(string parent, string attribute)
    {
        IEditableList<T> result;
        _controller.WorldModel.UndoLogger.StartTransaction(string.Format("Copy '{0}' {1}", parent, attribute));
        result = CloneInternal(_controller.WorldModel.Elements.Get(parent), attribute);
        _controller.WorldModel.UndoLogger.EndTransaction();
        return result;
    }

    public IEnumerator GetEnumerator()
    {
        return _wrappedItemsList.GetEnumerator();
    }

    public string Owner
    {
        get
        {
            if (_source.Owner == null)
            {
                return null;
            }

            return _source.Owner.Name;
        }
    }

    public IEnumerable<IEditableListItem<T>> ItemsList => _wrappedItemsList;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    private void PopulateWrappedItems()
    {
        _wrappedItems.Clear();
        _wrappedItemsList.Clear();
        _wrappedItemKeys.Clear();
        var index = 0;

        foreach (var item in _source)
        {
            AddWrappedItem(item, EditorUpdateSource.System, index);
            index++;
        }
    }

    internal void AddInternal(T item)
    {
        _source.Add(item, UpdateSource.User);
    }

    private void AddWrappedItem(T item, EditorUpdateSource source, int index)
    {
        var key = GetUniqueId();
        IEditableListItem<T> wrappedValue = new EditableListItem<T>(key, item);
        _wrappedItems.Add(key, wrappedValue);
        _wrappedItemsList.Insert(index, wrappedValue);
        _wrappedItemKeys.Insert(index, key);

        if (Added != null)
        {
            Added(this,
                new EditableListUpdatedEventArgs<T> {UpdatedItem = wrappedValue, Index = index, Source = source});
        }

        if (CollectionChanged != null)
        {
            CollectionChanged(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, wrappedValue, index));
        }
    }

    private string GetUniqueId()
    {
        _nextId++;
        return "k" + _nextId;
    }

    private void RemoveWrappedItem(IEditableListItem<T> item, EditorUpdateSource source, int index)
    {
        _wrappedItems.Remove(item.Key);
        _wrappedItemsList.Remove(item);
        _wrappedItemKeys.Remove(item.Key);
        if (Removed != null)
        {
            Removed(this, new EditableListUpdatedEventArgs<T> {UpdatedItem = item, Index = index, Source = source});
        }

        if (CollectionChanged != null)
        {
            CollectionChanged(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }
    }

    private void OnSourceAdded(object sender, QuestListUpdatedEventArgs<T> e)
    {
        AddWrappedItem(e.UpdatedItem, (EditorUpdateSource) e.Source, e.Index);
    }

    private void OnSourceRemoved(object sender, QuestListUpdatedEventArgs<T> e)
    {
        RemoveWrappedItem(_wrappedItems[_wrappedItemKeys[e.Index]], (EditorUpdateSource) e.Source, e.Index);
    }

    private IEditableList<T> CloneInternal(Element parent, string attribute)
    {
        var newSource = (QuestList<T>) _source.Clone();
        newSource.Locked = false;
        parent.Fields.Set(attribute, newSource);
        newSource = (QuestList<T>) parent.Fields.Get(attribute);
        return GetNewInstance(_controller, newSource);
    }

    #region Static DataWrapper

    private static readonly EditableDataWrapper<QuestList<T>, EditableList<T>> Wrapper;

    static EditableList()
    {
        Wrapper = new EditableDataWrapper<QuestList<T>, EditableList<T>>(GetNewInstance);
    }

    public static EditableList<T> GetInstance(EditorController controller, QuestList<T> list)
    {
        return Wrapper.GetInstance(controller, list);
    }

    private static EditableList<T> GetNewInstance(EditorController controller, QuestList<T> list)
    {
        return new EditableList<T>(controller, list);
    }

    public static void Clear()
    {
        Wrapper.Clear();
    }

    #endregion
}