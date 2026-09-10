#nullable disable
using System.Collections;

namespace QuestViva.Engine;

public interface IQuestList
{
    object this[int index] { get; }
    int Count { get; }
    void Add(object item);
    void Add(object item, UpdateSource source);
    void Add(object item, UpdateSource source, int index);
    bool Remove(object item);
    void Remove(object item, UpdateSource source, int index);
    bool Contains(object item);
}

public class QuestListUpdatedEventArgs<T> : EventArgs
{
    public T UpdatedItem { get; set; }
    public int Index { get; set; }
    public UpdateSource Source { get; set; }
}

public sealed class QuestList<T> : IMutableField, IQuestList, IList<T>, ICollection, IExtendableField
{
    private readonly List<T> _list;
    private UndoLogger _undoLog;

    public QuestList()
    {
        _list = new List<T>();
    }

    public QuestList(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            _list = new List<T>();
        }
        else
        {
            _list = new List<T>(collection);
        }
    }

    public QuestList(IEnumerable<T> collection, bool extended)
        : this(collection)
    {
        Extended = extended;
    }

    public bool Extended { get; }

    public IExtendableField Merge(IExtendableField parent)
    {
        var parentList = parent as QuestList<T>;
        return parentList.MergeLists(this);
    }

    public void Add(T item)
    {
        AddInternal(item, UpdateSource.System);
    }

    public bool Remove(T item)
    {
        return RemoveInternal(item);
    }


    #region IEnumerable<T> Members

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    #endregion

    public UndoLogger UndoLog
    {
        get => _undoLog;
        set
        {
            if (_undoLog == value)
            {
                return;
            }

            _undoLog = value;
            foreach (var item in this)
            {
                var mutableValue = item as IMutableField;
                if (mutableValue != null)
                {
                    mutableValue.UndoLog = value;
                }
            }
        }
    }

    public Element Owner { get; set; }

    public IMutableField Clone()
    {
        return new QuestList<T>(_list);
    }

    public bool Locked { get; set; }

    public bool RequiresCloning => true;

    public void Add(object item)
    {
        AddInternal((T) item, UpdateSource.System);
    }

    public void Add(object item, UpdateSource source)
    {
        AddInternal((T) item, source);
    }

    public void Add(object item, UpdateSource source, int index)
    {
        AddInternal((T) item, source, index);
    }

    public bool Remove(object item)
    {
        return RemoveInternal((T) item);
    }

    public void Remove(object item, UpdateSource source, int index)
    {
        RemoveInternal((T) item, source, index);
    }

    public bool Contains(object item)
    {
        return _list.Contains((T) item);
    }

    public object this[int index] => _list[index];
    public event EventHandler<QuestListUpdatedEventArgs<T>> Added;
    public event EventHandler<QuestListUpdatedEventArgs<T>> Removed;

    private void CheckNotLocked()
    {
        if (Locked)
        {
            throw new Exception(
                "Cannot modify the contents of this list as it is defined by an inherited type. Clone it before attempting to modify.");
        }
    }

    private void AddInternal(T item, UpdateSource source, int? index = null)
    {
        CheckNotLocked();
        if (index == null)
        {
            _list.Add(item);
            index = _list.Count - 1;
        }
        else
        {
            _list.Insert(index.Value, item);
        }

        ItemAdded(item, source, index.Value);
    }

    public void AddRange(IEnumerable<T> collection)
    {
        CheckNotLocked();

        // initial index of the added items, to be passed to the UndoLogger
        var index = _list.Count;

        _list.AddRange(collection);
        foreach (var item in collection)
        {
            ItemAdded(item, UpdateSource.System, index);
            index++;
        }
    }

    private bool RemoveInternal(T item)
    {
        var index = _list.IndexOf(item);
        if (index == -1)
        {
            return false;
        }

        RemoveInternal(item, UpdateSource.System, index);
        return true;
    }

    private void RemoveInternal(T item, UpdateSource source, int index)
    {
        CheckNotLocked();
        UndoLogRemove(item, index);
        _list.RemoveAt(index);
        ItemRemoved(item, source, index);
    }

    public void RemoveByIndex(int index, UpdateSource source)
    {
        RemoveInternal(_list[index], source, index);
    }

    private void UndoLogAdd(object item, int index)
    {
        if (UndoLog != null)
        {
            // also set UndoLog property on added item, if it needs a reference to the undo logger
            var mutableValue = item as IMutableField;
            if (mutableValue != null)
            {
                mutableValue.UndoLog = UndoLog;
            }

            UndoLog.AddUndoAction(() => new UndoListAdd(this, item, index));
        }
    }

    private void UndoLogRemove(object item, int index)
    {
        if (UndoLog != null)
        {
            UndoLog.AddUndoAction(() => new UndoListRemove(this, item, index));
        }
    }

    private void ItemAdded(T item, UpdateSource source, int index)
    {
        UndoLogAdd(item, index);

        if (Added != null)
        {
            Added(this, new QuestListUpdatedEventArgs<T> {UpdatedItem = item, Index = index, Source = source});
        }
    }

    private void ItemRemoved(T item, UpdateSource source, int index)
    {
        if (Removed != null)
        {
            Removed(this, new QuestListUpdatedEventArgs<T> {UpdatedItem = item, Index = index, Source = source});
        }
    }

    /// <summary>
    ///     Concatenates lists. Appends all items in list2 onto list1, does not check for duplicates.
    /// </summary>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static QuestList<T> operator +(QuestList<T> list1, QuestList<T> list2)
    {
        if (list1 == null)
        {
            return new QuestList<T>(list2);
        }

        return list1.MergeLists(list2);
    }

    public QuestList<T> MergeLists(QuestList<T> list2)
    {
        var result = new QuestList<T>(this);
        result.AddRange(list2);
        return result;
    }

    public QuestList<T> Exclude(T element)
    {
        var enumerable = this.Where(x => !x.Equals(element));
        return new QuestList<T>(enumerable);
    }

    public QuestList<T> Exclude(QuestList<T> excludeList)
    {
        var enumerable = this.Where(x => !excludeList.Contains(x));
        return new QuestList<T>(enumerable);
    }

    /// <summary>
    ///     Add an element to the end of a list
    /// </summary>
    /// <param name="list"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public static QuestList<T> operator +(QuestList<T> list, T element)
    {
        var result = new QuestList<T>(list);
        result.Add(element);
        return result;
    }

    /// <summary>
    ///     Add an element to the beginning of a list
    /// </summary>
    /// <param name="element"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static QuestList<T> operator +(T element, QuestList<T> list)
    {
        var result = new QuestList<T>();
        result.Add(element);
        result.AddRange(list);
        return result;
    }

    public static QuestList<T> operator -(QuestList<T> list, T element)
    {
        var result = new QuestList<T>(list);
        result.Remove(element);
        return result;
    }

    /// <summary>
    ///     Concatenates lists. Appends all items in list2 onto list1, except for any items in list2 which already appear in
    ///     list1.
    /// </summary>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public static QuestList<T> operator *(QuestList<T> list1, QuestList<T> list2)
    {
        var result = new QuestList<T>(list1);
        foreach (var item in list2)
        {
            if (!list1.Contains(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    // could have subtraction to remove items from list1 that appear in list2,
    // and division could return only items that appear in both lists.

    public override string ToString()
    {
        var result = "List: ";
        foreach (var item in this)
        {
            result += (item == null ? null : item.ToString()) + "; ";
        }

        return result;
    }

    internal T[] ToArray()
    {
        return _list.ToArray();
    }

    private class UndoListAdd : UndoLogger.IUndoAction
    {
        private readonly object _addedItem;
        private readonly IQuestList _appliesTo;
        private readonly int _index;

        public UndoListAdd(IQuestList appliesTo, object addedItem, int index)
        {
            _appliesTo = appliesTo;
            _addedItem = addedItem;
            _index = index;
        }

        #region IUndoAction Members

        public void DoUndo(WorldModel worldModel)
        {
            _appliesTo.Remove(_addedItem, UpdateSource.System, _index);
        }

        public void DoRedo(WorldModel worldModel)
        {
            _appliesTo.Add(_addedItem, UpdateSource.System, _index);
        }

        #endregion
    }

    private class UndoListRemove : UndoLogger.IUndoAction
    {
        private readonly IQuestList _appliesTo;
        private readonly int _index;
        private readonly object _removedItem;

        public UndoListRemove(IQuestList appliesTo, object removedItem, int index)
        {
            _appliesTo = appliesTo;
            _removedItem = removedItem;
            _index = index;
        }

        #region IUndoAction Members

        public void DoUndo(WorldModel worldModel)
        {
            _appliesTo.Add(_removedItem, UpdateSource.System, _index);
        }

        public void DoRedo(WorldModel worldModel)
        {
            _appliesTo.Remove(_removedItem, UpdateSource.System, _index);
        }

        #endregion
    }

    #region ICollection Members

    public void CopyTo(Array array, int index)
    {
        ((ICollection) _list).CopyTo(array, index);
    }

    public int Count => _list.Count;

    public bool IsSynchronized => ((ICollection) _list).IsSynchronized;

    public object SyncRoot => ((ICollection) _list).SyncRoot;

    #endregion

    #region IList<T> Members

    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        AddInternal(item, UpdateSource.System, index);
    }

    public void RemoveAt(int index)
    {
        RemoveInternal((T) this[index], UpdateSource.System, index);
    }

    T IList<T>.this[int index]
    {
        get => _list[index];
        set => throw new NotImplementedException();
    }

    #endregion

    #region ICollection<T> Members

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public bool IsReadOnly => ((ICollection<T>) _list).IsReadOnly;

    #endregion
}