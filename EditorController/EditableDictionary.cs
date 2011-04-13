using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class EditableDictionary<T> : IEditableDictionary<T>, IDataWrapper
    {
        public event EventHandler<EditableListUpdatedEventArgs<T>> Added;
        public event EventHandler<EditableListUpdatedEventArgs<T>> Removed;

        #region Static DataWrapper
        private static EditableDataWrapper<QuestDictionary<T>, EditableDictionary<T>> s_wrapper;

        static EditableDictionary()
        {
            s_wrapper = new EditableDataWrapper<QuestDictionary<T>, EditableDictionary<T>>(GetNewInstance);
        }

        public static EditableDictionary<T> GetInstance(EditorController controller, QuestDictionary<T> list)
        {
            return s_wrapper.GetInstance(controller, list);
        }

        private static EditableDictionary<T> GetNewInstance(EditorController controller, QuestDictionary<T> list)
        {
            return new EditableDictionary<T>(controller, list);
        }
        #endregion

        private QuestDictionary<T> m_source;
        private Dictionary<string, IEditableListItem<T>> m_wrappedItems = new Dictionary<string, IEditableListItem<T>>();
        private EditorController m_controller;

        public EditableDictionary(EditorController controller, QuestDictionary<T> source)
        {
            m_controller = controller;
            m_source = source;
            m_source.Added += m_source_Added;
            m_source.Removed += m_source_Removed;
            PopulateWrappedItems();
        }

        private void PopulateWrappedItems()
        {
            m_wrappedItems.Clear();
            int index = 0;

            foreach (var item in m_source)
            {
                AddWrappedItem(item.Key, item.Value, EditorUpdateSource.System, index);
                index++;
            }
        }

        public object GetUnderlyingValue()
        {
            return m_source;
        }

        public IDictionary<string, IEditableListItem<T>> Items
        {
            get { return m_wrappedItems; }
        }

        private void AddWrappedItem(string key, T value, EditorUpdateSource source, int index)
        {
            IEditableListItem<T> wrappedValue = new EditableListItem<T>(key, value);
            m_wrappedItems.Add(key, wrappedValue);

            if (Added != null) Added(this, new EditableListUpdatedEventArgs<T> { UpdatedItem = wrappedValue, Index = index, Source = source });
        }

        private void RemoveWrappedItem(IEditableListItem<T> item, EditorUpdateSource source, int index)
        {
            m_wrappedItems.Remove(item.Key);
            if (Removed != null) Removed(this, new EditableListUpdatedEventArgs<T> { UpdatedItem = item, Index = index, Source = source });
        }

        void m_source_Added(object sender, QuestDictionaryUpdatedEventArgs<T> e)
        {
            AddWrappedItem(e.Key, e.Item, (EditorUpdateSource)e.Source, e.Index);
        }

        void m_source_Removed(object sender, QuestDictionaryUpdatedEventArgs<T> e)
        {
            RemoveWrappedItem(m_wrappedItems[e.Key], (EditorUpdateSource)e.Source, e.Index);
        }
    }
}
