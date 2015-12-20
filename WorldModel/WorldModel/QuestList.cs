using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TextAdventures.Quest
{
    public interface IQuestList
    {
        void Add(object item);
        void Add(object item, UpdateSource source);
        void Add(object item, UpdateSource source, int index);
        bool Remove(object item);
        void Remove(object item, UpdateSource source, int index);
        bool Contains(object item);
        object this[int index] { get; }
        int Count { get; }
    }

    public class QuestListUpdatedEventArgs<T> : EventArgs
    {
        public T UpdatedItem { get; set; }
        public int Index { get; set; }
        public UpdateSource Source { get; set; }
    }

    public sealed class QuestList<T> : IMutableField, IQuestList, IList<T>, ICollection, IExtendableField
    {
        public event EventHandler<QuestListUpdatedEventArgs<T>> Added;
        public event EventHandler<QuestListUpdatedEventArgs<T>> Removed;

        private List<T> m_list;
        private UndoLogger m_undoLog;

        public QuestList()
        {
            m_list = new List<T>();
        }

        public QuestList(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                m_list = new List<T>();
            }
            else
            {
                m_list = new List<T>(collection);
            }
        }

        public QuestList(IEnumerable<T> collection, bool extended)
            : this(collection)
        {
            Extended = extended;
        }

        public UndoLogger UndoLog
        {
            get { return m_undoLog; }
            set
            {
                if (m_undoLog == value) return;
                m_undoLog = value;
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
            return new QuestList<T>(m_list);
        }

        public bool Locked { get; set; }

        public bool RequiresCloning { get { return true; } }

        private void CheckNotLocked()
        {
            if (Locked) throw new Exception("Cannot modify the contents of this list as it is defined by an inherited type. Clone it before attempting to modify.");
        }

        public void Add(object item)
        {
            AddInternal((T)item, UpdateSource.System);
        }

        public void Add(object item, UpdateSource source)
        {
            AddInternal((T)item, source);
        }

        public void Add(object item, UpdateSource source, int index)
        {
            AddInternal((T)item, source, index);
        }

        public void Add(T item)
        {
            AddInternal(item, UpdateSource.System);
        }

        private void AddInternal(T item, UpdateSource source, int? index = null)
        {
            CheckNotLocked();
            if (index == null)
            {
                m_list.Add(item);
                index = m_list.Count - 1;
            }
            else
            {
                m_list.Insert(index.Value, item);
            }
            ItemAdded(item, source, index.Value);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            CheckNotLocked();

            // initial index of the added items, to be passed to the UndoLogger
            int index = m_list.Count;

            m_list.AddRange(collection);
            foreach (T item in collection)
            {
                ItemAdded(item, UpdateSource.System, index);
                index++;
            }
        }

        public bool Remove(T item)
        {
            return RemoveInternal(item);
        }

        private bool RemoveInternal(T item)
        {
            int index = m_list.IndexOf(item);
            if (index == -1) return false;
            RemoveInternal(item, UpdateSource.System, index);
            return true;
        }

        private void RemoveInternal(T item, UpdateSource source, int index)
        {
            CheckNotLocked();
            UndoLogRemove(item, index);
            m_list.RemoveAt(index);
            ItemRemoved(item, source, index);
        }

        public void RemoveByIndex(int index, UpdateSource source)
        {
            RemoveInternal(m_list[index], source, index);
        }

        public bool Remove(object item)
        {
            return RemoveInternal((T)item);
        }

        public void Remove(object item, UpdateSource source, int index)
        {
            RemoveInternal((T)item, source, index);
        }

        public bool Contains(object item)
        {
            return m_list.Contains((T)item);
        }

        public object this[int index]
        {
            get { return m_list[index]; }
        }

        private void UndoLogAdd(object item, int index)
        {
            if (UndoLog != null)
            {
                // also set UndoLog property on added item, if it needs a reference to the undo logger
                IMutableField mutableValue = item as IMutableField;
                if (mutableValue != null)
                {
                    mutableValue.UndoLog = UndoLog;
                }

                UndoLog.AddUndoAction(new UndoListAdd(this, item, index));
            }
        }

        private void UndoLogRemove(object item, int index)
        {
            if (UndoLog != null)
            {
                UndoLog.AddUndoAction(new UndoListRemove(this, item, index));
            }
        }

        private void ItemAdded(T item, UpdateSource source, int index)
        {
            UndoLogAdd(item, index);

            if (Added != null)
            {
                Added(this, new QuestListUpdatedEventArgs<T> { UpdatedItem = item, Index = index, Source = source });
            }
        }

        private void ItemRemoved(T item, UpdateSource source, int index)
        {
            if (Removed != null)
            {
                Removed(this, new QuestListUpdatedEventArgs<T> { UpdatedItem = item, Index = index, Source = source });
            }
        }

        /// <summary>
        /// Concatenates lists. Appends all items in list2 onto list1, does not check for duplicates.
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static QuestList<T> operator +(QuestList<T> list1, QuestList<T> list2)
        {
            //System.Diagnostics.Debug.Assert(false, "Operators on lists are deprecated");
            if (list1 == null) return new QuestList<T>(list2);
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
        /// Add an element to the end of a list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static QuestList<T> operator +(QuestList<T> list, T element)
        {
            //System.Diagnostics.Debug.Assert(false, "Operators on lists are deprecated");
            QuestList<T> result = new QuestList<T>(list);
            result.Add(element);
            return result;
        }

        /// <summary>
        /// Add an element to the beginning of a list
        /// </summary>
        /// <param name="element"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static QuestList<T> operator +(T element, QuestList<T> list)
        {
            //System.Diagnostics.Debug.Assert(false, "Operators on lists are deprecated");
            QuestList<T> result = new QuestList<T>();
            result.Add(element);
            result.AddRange(list);
            return result;
        }

        public static QuestList<T> operator -(QuestList<T> list, T element)
        {
            //System.Diagnostics.Debug.Assert(false, "Operators on lists are deprecated");
            QuestList<T> result = new QuestList<T>(list);
            result.Remove(element);
            return result;
        }

        /// <summary>
        /// Concatenates lists. Appends all items in list2 onto list1, except for any items in list2 which already appear in list1.
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static QuestList<T> operator *(QuestList<T> list1, QuestList<T> list2)
        {
            QuestList<T> result = new QuestList<T>(list1);
            foreach (T item in list2)
            {
                if (!list1.Contains(item)) result.Add(item);
            }
            return result;
        }

        // could have subtraction to remove items from list1 that appear in list2,
        // and division could return only items that appear in both lists.

        public override string ToString()
        {
            string result = "List: ";
            foreach (T item in this)
            {
                result += ((item == null) ? null : item.ToString()) + "; ";
            }
            return result;
        }

        internal T[] ToArray()
        {
            return m_list.ToArray();
        }

        public bool Extended { get; private set; }

        public IExtendableField Merge(IExtendableField parent)
        {
            QuestList<T> parentList = parent as QuestList<T>;
            return parentList.MergeLists(this);
        }


        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            ((ICollection)m_list).CopyTo(array, index);
        }

        public int Count
        {
            get { return m_list.Count; }
        }

        public bool IsSynchronized
        {
            get { return ((ICollection)m_list).IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return ((ICollection)m_list).SyncRoot; }
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return m_list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            AddInternal(item, UpdateSource.System, index);
        }

        public void RemoveAt(int index)
        {
            RemoveInternal((T)this[index], UpdateSource.System, index);
        }

        T IList<T>.this[int index]
        {
            get
            {
                return m_list[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return m_list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<T>)m_list).IsReadOnly; }
        }

        #endregion

        private class UndoListAdd : TextAdventures.Quest.UndoLogger.IUndoAction
        {
            private IQuestList m_appliesTo;
            private object m_addedItem;
            private int m_index;

            public UndoListAdd(IQuestList appliesTo, object addedItem, int index)
            {
                m_appliesTo = appliesTo;
                m_addedItem = addedItem;
                m_index = index;
            }

            #region IUndoAction Members

            public void DoUndo(WorldModel worldModel)
            {
                m_appliesTo.Remove(m_addedItem, UpdateSource.System, m_index);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_appliesTo.Add(m_addedItem, UpdateSource.System, m_index);
            }

            #endregion
        }

        private class UndoListRemove : TextAdventures.Quest.UndoLogger.IUndoAction
        {
            private IQuestList m_appliesTo;
            private object m_removedItem;
            private int m_index;

            public UndoListRemove(IQuestList appliesTo, object removedItem, int index)
            {
                m_appliesTo = appliesTo;
                m_removedItem = removedItem;
                m_index = index;
            }

            #region IUndoAction Members

            public void DoUndo(WorldModel worldModel)
            {
                m_appliesTo.Add(m_removedItem, UpdateSource.System, m_index);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_appliesTo.Remove(m_removedItem, UpdateSource.System, m_index);
            }

            #endregion
        }
    }
}
