using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

namespace TextAdventures.Quest
{
    public class EditableList<T> : IEditableList<T>, IDataWrapper, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event EventHandler<EditableListUpdatedEventArgs<T>> Added;
        public event EventHandler<EditableListUpdatedEventArgs<T>> Removed;

        // currently unused
        public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated
        {
            add { }
            remove { }
        }

        #region Static DataWrapper
        private static EditableDataWrapper<QuestList<T>, EditableList<T>> s_wrapper;

        static EditableList()
        {
            s_wrapper = new EditableDataWrapper<QuestList<T>, EditableList<T>>(GetNewInstance);
        }

        public static EditableList<T> GetInstance(EditorController controller, QuestList<T> list)
        {
            return s_wrapper.GetInstance(controller, list);
        }

        private static EditableList<T> GetNewInstance(EditorController controller, QuestList<T> list)
        {
            return new EditableList<T>(controller, list);
        }

        public static void Clear()
        {
            s_wrapper.Clear();
        }
        #endregion

        private QuestList<T> m_source;
        private Dictionary<string, IEditableListItem<T>> m_wrappedItems = new Dictionary<string, IEditableListItem<T>>();
        private List<IEditableListItem<T>> m_wrappedItemsList = new List<IEditableListItem<T>>();
        private List<string> m_wrappedItemKeys = new List<string>();
        private int m_nextId = 0;
        private EditorController m_controller;

        public EditableList(EditorController controller, QuestList<T> source)
        {
            m_controller = controller;
            m_source = source;
            m_source.Added += m_source_Added;
            m_source.Removed += m_source_Removed;
            PopulateWrappedItems();
        }

        public IDictionary<string, IEditableListItem<T>> Items
        {
            get { return m_wrappedItems; }
        }

        private void PopulateWrappedItems()
        {
            m_wrappedItems.Clear();
            m_wrappedItemsList.Clear();
            m_wrappedItemKeys.Clear();
            int index = 0;

            foreach (var item in m_source)
            {
                AddWrappedItem(item, EditorUpdateSource.System, index);
                index++;
            }
        }

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

            m_controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
            AddInternal(item);
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        internal void AddInternal(T item)
        {
            m_source.Add(item, UpdateSource.User);
        }

        private void AddWrappedItem(T item, EditorUpdateSource source, int index)
        {
            string key = GetUniqueId();
            IEditableListItem<T> wrappedValue = new EditableListItem<T>(key, item);
            m_wrappedItems.Add(key, wrappedValue);
            m_wrappedItemsList.Insert(index, wrappedValue);
            m_wrappedItemKeys.Insert(index, key);

            if (Added != null) Added(this, new EditableListUpdatedEventArgs<T> { UpdatedItem = wrappedValue, Index = index, Source = source });
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, wrappedValue, index));
        }

        private string GetUniqueId()
        {
            m_nextId++;
            return "k" + m_nextId.ToString();
        }

        public void Remove(params string[] keys)
        {
            string undoEntry = null;
            if (typeof(T) == typeof(string))
            {
                undoEntry = string.Format("Remove items from list");
            }

            if (undoEntry == null)
            {
                throw new InvalidOperationException("Unknown list type");
            }

            m_controller.WorldModel.UndoLogger.StartTransaction(undoEntry);

            foreach (string key in keys)
            {
                m_source.RemoveByIndex(m_wrappedItemKeys.IndexOf(key), UpdateSource.User);
            }

            m_controller.WorldModel.UndoLogger.EndTransaction();
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

            m_controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
            m_source.Remove(m_source[index], UpdateSource.User, index);
            m_source.Add(item, UpdateSource.User, index);
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        public void Update(string key, T item)
        {
            Update(m_wrappedItemKeys.IndexOf(key), item);
        }

        private void RemoveWrappedItem(IEditableListItem<T> item, EditorUpdateSource source, int index)
        {
            m_wrappedItems.Remove(item.Key);
            m_wrappedItemsList.Remove(item);
            m_wrappedItemKeys.Remove(item.Key);
            if (Removed != null) Removed(this, new EditableListUpdatedEventArgs<T> { UpdatedItem = item, Index = index, Source = source });
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        void m_source_Added(object sender, QuestListUpdatedEventArgs<T> e)
        {
            AddWrappedItem(e.UpdatedItem, (EditorUpdateSource)e.Source, e.Index);
        }

        void m_source_Removed(object sender, QuestListUpdatedEventArgs<T> e)
        {
            RemoveWrappedItem(m_wrappedItems[m_wrappedItemKeys[e.Index]], (EditorUpdateSource)e.Source, e.Index);
        }

        public object GetUnderlyingValue()
        {
            return m_source;
        }

        public IEnumerable<KeyValuePair<string, string>> DisplayItems
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();

                foreach (var item in m_wrappedItems)
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
            //if (m_source.Contains(item))
            //{
            //    return new ValidationResult { Valid = false, Message = ValidationMessage.ItemAlreadyExists };
            //}

            return new ValidationResult { Valid = true };
        }

        public string DisplayString()
        {
            throw new NotImplementedException();
        }

        public bool Locked
        {
            get
            {
                return m_source.Locked;
            }
        }

        public IEditableList<T> Clone(string parent, string attribute)
        {
            IEditableList<T> result;
            m_controller.WorldModel.UndoLogger.StartTransaction(string.Format("Copy '{0}' {1}", parent, attribute));
            result = CloneInternal(m_controller.WorldModel.Elements.Get(parent), attribute);
            m_controller.WorldModel.UndoLogger.EndTransaction();
            return result;
        }

        private IEditableList<T> CloneInternal(Element parent, string attribute)
        {
            QuestList<T> newSource = (QuestList<T>)m_source.Clone();
            newSource.Locked = false;
            parent.Fields.Set(attribute, newSource);
            newSource = (QuestList<T>)parent.Fields.Get(attribute);
            return EditableList<T>.GetNewInstance(m_controller, newSource);
        }

        public IEnumerator GetEnumerator()
        {
            return m_wrappedItemsList.GetEnumerator();
        }

        public string Owner
        {
            get
            {
                if (m_source.Owner == null) return null;
                return m_source.Owner.Name;
            }
        }

        public IEnumerable<IEditableListItem<T>> ItemsList
        {
            get { return m_wrappedItemsList; }
        }
    }
}
