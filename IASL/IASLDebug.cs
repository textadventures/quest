using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IASLDebug
    {
        event ObjectsUpdatedHandler ObjectsUpdated;
        List<string> GetObjects(string type);
        DebugData GetDebugData(string obj);
        List<string> DebuggerObjectTypes { get; }
    }

    public class DebugDataItem
    {
        private string m_value;
        private bool m_isInherited = false;

        public string Value
        {
            get { return m_value; }
        }

        public bool IsInherited
        {
            get { return m_isInherited; }
            set { m_isInherited = value; }
        }

        public DebugDataItem(string value)
        {
            m_value = value;
        }
        public DebugDataItem(string value, bool isInherited)
            : this(value)
        {
            m_isInherited = isInherited;
        }
    }

    public class DebugData
    {
        private Dictionary<string, DebugDataItem> m_data = new Dictionary<string, DebugDataItem>();
        public Dictionary<string, DebugDataItem> Data
        {
            get { return m_data; }
            set { m_data = value; }
        }
    }
}
