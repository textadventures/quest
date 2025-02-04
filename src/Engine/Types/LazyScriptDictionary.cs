using System.Collections.Generic;

namespace QuestViva.Engine.Types
{
    internal class LazyScriptDictionary
    {
        public LazyScriptDictionary(IDictionary<string, string> dictionary)
        {
            Dictionary = dictionary;
        }

        public IDictionary<string, string> Dictionary { get; private set; }
    }
}
