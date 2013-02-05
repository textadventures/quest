using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Types
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
