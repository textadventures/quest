#nullable disable
using System.Collections.Generic;

namespace QuestViva.Engine.Types
{
    internal class LazyObjectDictionary
    {
        public LazyObjectDictionary(IDictionary<string, string> dictionary)
        {
            Dictionary = dictionary;
        }

        public IDictionary<string, string> Dictionary { get; private set; }
    }
}
