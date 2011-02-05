using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPlayer
{
    public class SessionResources
    {
        private int m_count = 0;
        private Dictionary<string, string> m_resourceList = new Dictionary<string, string>();
        private Dictionary<string, string> m_resourceKeys = new Dictionary<string, string>();
        
        public string Add(string filename)
        {
            // if adding the same filename, return the existing resource id
            if (m_resourceKeys.ContainsKey(filename))
            {
                return ToUrl(m_resourceKeys[filename]);
            }

            m_count++;
            string key = m_count.ToString() + System.IO.Path.GetExtension(filename);
            m_resourceList.Add(key, filename);
            m_resourceKeys.Add(filename, key);
            return ToUrl(key);
        }

        public string ToUrl(string key)
        {
            return "Resource.ashx?id=" + key;
        }

        public string Get(string key)
        {
            string result;
            if (m_resourceList.TryGetValue(key, out result))
            {
                return result;
            }
            return string.Empty;
        }
    }
}