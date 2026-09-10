#nullable disable
using System.Text.RegularExpressions;

namespace QuestViva.Engine;

internal class RegexCache
{
    private readonly Dictionary<string, Regex> _cache = new();

    public Regex GetRegex(string regex, string cacheID)
    {
        Regex result;
        if (_cache.TryGetValue(cacheID, out result))
        {
            return result;
        }

        result = new Regex(regex, RegexOptions.IgnoreCase);
        _cache.Add(cacheID, result);
        return result;
    }
}