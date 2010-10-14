using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AxeSoftware.Quest
{
    public interface IQuestList
    {
        void Add(object item);
        bool Remove(object item);
        bool Contains(object item);
        object this[int index] { get; }
    }

    public class QuestList<T> : IMutableField, IQuestList, IList<T>, ICollection
    {
        private List<T> m_list;

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

        public UndoLogger UndoLog
        {
            get;
            set;
        }

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
            CheckNotLocked();
            m_list.Add((T)item);
            UndoLogAdd(item);
        }

        public void Add(T item)
        {
            CheckNotLocked();
            m_list.Add(item);
            UndoLogAdd(item);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            CheckNotLocked();
            m_list.AddRange(collection);
            foreach (T item in collection)
            {
                UndoLogAdd(item);
            }
        }

        public bool Remove(T item)
        {
            CheckNotLocked();
            UndoLogRemove(item);
            return m_list.Remove(item);
        }

        public bool Remove(object item)
        {
            CheckNotLocked();
            UndoLogRemove(item);
            return m_list.Remove((T)item);
        }

        public bool Contains(object item)
        {
            return m_list.Contains((T)item);
        }

        public object this[int index]
        {
            get { return m_list[index]; }
        }

        private void UndoLogAdd(object item)
        {
            if (UndoLog != null)
            {
                UndoLog.AddUndoAction(new UndoListAdd(this, item));
            }
        }

        private void UndoLogRemove(object item)
        {
            if (UndoLog != null)
            {
                UndoLog.AddUndoAction(new UndoListRemove(this, item));
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
            QuestList<T> result = new QuestList<T>(list1);
            result.AddRange(list2);
            return result;
        }

        /// <summary>
        /// Add an element to the end of a list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static QuestList<T> operator +(QuestList<T> list, T element)
        {
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
            QuestList<T> result = new QuestList<T>();
            result.Add(element);
            result.AddRange(list);
            return result;
        }

        public static QuestList<T> operator -(QuestList<T> list, T element)
        {
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
                result += item.ToString() + "; ";
            }
            return result;
        }

        internal T[] ToArray()
        {
            return m_list.ToArray();
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
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
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

        private class UndoListAdd : AxeSoftware.Quest.UndoLogger.IUndoAction
        {
            private IQuestList m_appliesTo;
            private object m_addedItem;

            public UndoListAdd(IQuestList appliesTo, object addedItem)
            {
                m_appliesTo = appliesTo;
                m_addedItem = addedItem;
            }

            #region IUndoAction Members

            public void DoUndo(WorldModel worldModel)
            {
                m_appliesTo.Remove(m_addedItem);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_appliesTo.Add(m_addedItem);
            }

            #endregion
        }

        private class UndoListRemove : AxeSoftware.Quest.UndoLogger.IUndoAction
        {
            private IQuestList m_appliesTo;
            private object m_removedItem;

            public UndoListRemove(IQuestList appliesTo, object removedItem)
            {
                m_appliesTo = appliesTo;
                m_removedItem = removedItem;
            }

            #region IUndoAction Members

            public void DoUndo(WorldModel worldModel)
            {
                m_appliesTo.Add(m_removedItem);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_appliesTo.Remove(m_removedItem);
            }

            #endregion
        }
    }
}
