using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;

namespace WebPlayer
{
    internal class InterfaceListHandler
    {
        private OutputBuffer m_buffer;
        private Dictionary<ListType, List<ListData>> m_lists = new Dictionary<ListType, List<ListData>>();
        private ListDataComparer m_comparer = new ListDataComparer();

        public InterfaceListHandler(OutputBuffer buffer)
        {
            m_buffer = buffer;
        }

        private class ListDataComparer : IEqualityComparer<ListData>
        {
            public bool Equals(ListData x, ListData y)
            {
                if (x.Text != y.Text) return false;
                return VerbString(x.Verbs) == VerbString(y.Verbs);
            }

            public int GetHashCode(ListData obj)
            {
                return obj.GetHashCode();
            }
        }

        public void UpdateList(ListType listType, List<ListData> items)
        {
            bool listChanged;
            if (!m_lists.ContainsKey(listType))
            {
                listChanged = true;
            }
            else
            {
                listChanged= !m_lists[listType].SequenceEqual(items, m_comparer);
            }

            if (listChanged)
            {
                m_lists[listType] = items;
                SendUpdatedList(listType);
            }
        }

        private void SendUpdatedList(ListType listType)
        {
            string listName = null;
            if (listType == ListType.InventoryList) listName = "inventory";
            if (listType == ListType.ObjectsList) listName = "placesobjects";

            if (listName != null)
            {
                m_buffer.AddJavaScriptToBuffer("updateList", new StringParameter(listName), ListDataParameter(m_lists[listType]));
            }
        }

        private IJavaScriptParameter ListDataParameter(List<ListData> list)
        {
            // TO DO: It is permissible for there to be multiple objects in the same room with the same name,
            // so we can't key by displayed object name.

            var convertedList = new Dictionary<string, string>();
            foreach (ListData data in list)
            {
                convertedList.Add(data.Text, VerbString(data.Verbs));
            }
            return new JSONParameter(convertedList);
        }

        private static string VerbString(IEnumerable<string> verbs)
        {
            return string.Join("/", verbs);
        }
    }
}