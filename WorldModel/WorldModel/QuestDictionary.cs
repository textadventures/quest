using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AxeSoftware.Quest
{
    public interface IQuestDictionary
    {
        void Add(object key, object value);
        void Remove(object key);
    }

    public class QuestDictionary<T> : IDictionary<string, T>, IDictionary, IMutableField, IQuestDictionary
    {
        private Dictionary<string, T> m_dictionary = new Dictionary<string, T>();

        private void UndoLogAdd(object key)
        {
            if (Parent != null)
            {
                Parent.UndoLog(new UndoDictionaryAdd(this, key, m_dictionary[(string)key]));
            }
        }

        private void UndoLogRemove(object key)
        {
            if (Parent != null)
            {
                Parent.UndoLog(new UndoDictionaryRemove(this, key, m_dictionary[(string)key]));
            }
        }

        #region IMutableField Members

        public Fields Parent
        {
            get;
            set;
        }

        public IMutableField Clone()
        {
            QuestDictionary<T> result = new QuestDictionary<T>();
            foreach (KeyValuePair<string, T> kvp in m_dictionary)
            {
                result.Add(kvp);
            }
            return result;
        }

        public bool Locked { get; set; }

        public bool RequiresCloning { get { return true; } }

        #endregion

        private void CheckNotLocked()
        {
            if (Locked) throw new Exception("Cannot modify the contents of this dictionary as it is defined by an inherited type. Clone it before attempting to modify.");
        }

        #region IDictionary<string,T> Members

        public void Add(string key, T value)
        {
            CheckNotLocked();
            m_dictionary.Add(key, value);
            UndoLogAdd(key);
        }

        public bool ContainsKey(string key)
        {
            return m_dictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return m_dictionary.Keys; }
        }

        public bool Remove(string key)
        {
            CheckNotLocked();
            UndoLogRemove(key);
            return m_dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out T value)
        {
            return m_dictionary.TryGetValue(key, out value);
        }

        public ICollection<T> Values
        {
            get { return m_dictionary.Values; }
        }

        public T this[string key]
        {
            get
            {
                return m_dictionary[key];
            }
            set
            {
                CheckNotLocked();
                if (ContainsKey(key))
                {
                    UndoLogRemove(key);
                }
                UndoLogAdd(key);
                m_dictionary[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,T>> Members

        public void Add(KeyValuePair<string, T> item)
        {
            CheckNotLocked();
            ((ICollection<KeyValuePair<string,T>>)m_dictionary).Add(item);
            UndoLogAdd(item.Key);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return ((ICollection<KeyValuePair<string, T>>)m_dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, T>>)m_dictionary).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<string, T>>)m_dictionary).IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            CheckNotLocked();
            UndoLogRemove(item.Key);
            return ((ICollection<KeyValuePair<string, T>>)m_dictionary).Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,T>> Members

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }

        #endregion

        #region IDictionary Members

        public void Add(object key, object value)
        {
            CheckNotLocked();
            m_dictionary.Add((string)key, (T)value);
            UndoLogAdd(key);
        }

        public bool Contains(object key)
        {
            return m_dictionary.ContainsKey((string)key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return m_dictionary.GetEnumerator();
        }

        public bool IsFixedSize
        {
            get { return ((IDictionary)m_dictionary).IsFixedSize; }
        }

        ICollection IDictionary.Keys
        {
            get { return m_dictionary.Keys; }
        }

        public void Remove(object key)
        {
            CheckNotLocked();
            UndoLogRemove(key);
            m_dictionary.Remove((string)key);
        }

        ICollection IDictionary.Values
        {
            get { return m_dictionary.Values; }
        }

        public object this[object key]
        {
            get
            {
                return this[(string)key];
            }
            set
            {
                this[(string)key] = (T)value;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            ((ICollection)m_dictionary).CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get { return ((ICollection)m_dictionary).IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return ((ICollection)m_dictionary).SyncRoot; }
        }

        #endregion

        public override string ToString()
        {
            return "Dictionary: " + SaveString();
        }

        internal string SaveString()
        {
            string result = string.Empty;
            int count = 0;

            foreach (KeyValuePair<string, T> kvp in m_dictionary)
            {
                count++;
                result += kvp.Key + " = " + kvp.Value.ToString();
                if (count < m_dictionary.Count) result += ";";
            }

            return result;
        }

        private class UndoDictionaryAdd : AxeSoftware.Quest.UndoLogger.IUndoAction
        {
            private IQuestDictionary m_appliesTo;
            private object m_addedKey;
            private object m_addedItem;

            public UndoDictionaryAdd(IQuestDictionary appliesTo, object addedKey, object addedItem)
            {
                m_appliesTo = appliesTo;
                m_addedKey = addedKey;
                m_addedItem = addedItem;
            }

            #region IUndoAction Members

            public void DoUndo(WorldModel worldModel)
            {
                m_appliesTo.Remove(m_addedKey);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_appliesTo.Add(m_addedKey, m_addedItem);
            }

            #endregion
        }

        private class UndoDictionaryRemove : AxeSoftware.Quest.UndoLogger.IUndoAction
        {
            private IQuestDictionary m_appliesTo;
            private object m_removedKey;
            private object m_removedItem;

            public UndoDictionaryRemove(IQuestDictionary appliesTo, object removedKey, object removedItem)
            {
                m_appliesTo = appliesTo;
                m_removedKey = removedKey;
                m_removedItem = removedItem;
            }

            #region IUndoAction Members

            public void DoUndo(WorldModel worldModel)
            {
                m_appliesTo.Add(m_removedKey, m_removedItem);
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_appliesTo.Remove(m_removedKey);
            }

            #endregion
        }
    }
}
