using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class EditableList<T> : IEditableList<T>
    {
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
            return new EditableList<T>(list);
        }

        private QuestList<T> m_source;
        private Dictionary<string, IEditableListItem<T>> m_wrappedItems = new Dictionary<string, IEditableListItem<T>>();
        private int m_nextId = 0;

        public EditableList(QuestList<T> source)
        {
            m_source = source;
            PopulateWrappedItems();
        }

        public IDictionary<string, IEditableListItem<T>> Items
        {
            get { return m_wrappedItems; }
        }

        private void PopulateWrappedItems()
        {
            m_wrappedItems.Clear();

            foreach (var item in m_source)
            {
                string key = GetUniqueId();
                IEditableListItem<T> wrappedValue = new EditableListItem<T>(key, item);
                m_wrappedItems.Add(key, wrappedValue);
            }
        }

        private string GetUniqueId()
        {
            m_nextId++;
            return "k" + m_nextId.ToString();
        }
    }
}
