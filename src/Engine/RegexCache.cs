using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QuestViva.Engine
{
    internal class RegexCache
    {
        private Dictionary<string, Regex> m_cache = new Dictionary<string, Regex>();

        public Regex GetRegex(string regex, string cacheID)
        {
            Regex result;
            if (m_cache.TryGetValue(cacheID, out result))
            {
                return result;
            }
            result = new Regex(regex, RegexOptions.IgnoreCase);
            m_cache.Add(cacheID, result);
            return result;
        }
    }
}
