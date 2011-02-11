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
            if (listType == ListType.ExitsList)
            {
                SendCompassList(m_lists[ListType.ExitsList]);
                return;
            }

            string listName = null;
            if (listType == ListType.InventoryList) listName = "inventory";
            if (listType == ListType.ObjectsList) listName = "placesobjects";

            if (listName != null)
            {
                m_buffer.AddJavaScriptToBuffer("updateList", new StringParameter(listName), ListDataParameter(m_lists[listType]));
            }
        }

        private void SendCompassList(List<ListData> list)
        {
            string data = string.Join("/", list.Select(l => l.Text));
            m_buffer.AddJavaScriptToBuffer("updateCompass", new StringParameter(data));
        }

        private IJavaScriptParameter ListDataParameter(List<ListData> list)
        {
            var convertedList = new Dictionary<string, string>();
            int count = 1;
            foreach (ListData data in list)
            {
                convertedList.Add(
                    string.Format("k{0}", count),
                    string.Format("{0}:{1}", data.Text, VerbString(data.Verbs))
                );
                count++;
            }
            return new JSONParameter(convertedList);
        }

        private static string VerbString(IEnumerable<string> verbs)
        {
            return string.Join("/", verbs);
        }
    }
}