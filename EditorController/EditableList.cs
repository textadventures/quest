using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class EditableList<T> : IEditableList<T>
    {
        public event EventHandler<EditableListUpdatedEventArgs<T>> Added;
        public event EventHandler<EditableListUpdatedEventArgs<T>> Removed;

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
        #endregion

        private QuestList<T> m_source;
        private Dictionary<string, IEditableListItem<T>> m_wrappedItems = new Dictionary<string, IEditableListItem<T>>();
        private Dictionary<T, IEditableListItem<T>> m_wrappedItemLookup = new Dictionary<T, IEditableListItem<T>>();
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
            m_source.Add(item, UpdateSource.User);
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        private void AddWrappedItem(T item, EditorUpdateSource source, int index)
        {
            string key = GetUniqueId();
            IEditableListItem<T> wrappedValue = new EditableListItem<T>(key, item);
            m_wrappedItems.Add(key, wrappedValue);
            m_wrappedItemLookup.Add(item, wrappedValue);

            if (Added != null) Added(this, new EditableListUpdatedEventArgs<T> { UpdatedItem = wrappedValue, Index = index, Source = source });
        }

        private string GetUniqueId()
        {
            m_nextId++;
            return "k" + m_nextId.ToString();
        }

        public void Remove(params T[] items)
        {
            string undoEntry = null;
            if (typeof(T) == typeof(string))
            {
                undoEntry = string.Format("Remove '{0}'", string.Join(",", items));
            }

            if (undoEntry == null)
            {
                throw new InvalidOperationException("Unknown list type");
            }

            m_controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
            foreach (T item in items)
            {
                m_source.Remove(item, UpdateSource.User, m_source.IndexOf(item));
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

        private void RemoveWrappedItem(IEditableListItem<T> item, EditorUpdateSource source, int index)
        {
            m_wrappedItems.Remove(item.Key);
            m_wrappedItemLookup.Remove(item.Value);
            if (Removed != null) Removed(this, new EditableListUpdatedEventArgs<T> { UpdatedItem = item, Index = index, Source = source });
        }

        void m_source_Added(object sender, QuestListUpdatedEventArgs<T> e)
        {
            AddWrappedItem(e.UpdatedItem, (EditorUpdateSource)e.Source, e.Index);
        }

        void m_source_Removed(object sender, QuestListUpdatedEventArgs<T> e)
        {
            RemoveWrappedItem(m_wrappedItemLookup[e.UpdatedItem], (EditorUpdateSource)e.Source, e.Index);
        }
    }
}
