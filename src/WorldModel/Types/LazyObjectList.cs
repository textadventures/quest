using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Types
{
    internal class LazyObjectList
    {
        public LazyObjectList(IEnumerable<string> objects)
        {
            Objects = objects;
        }

        public IEnumerable<string> Objects { get; private set; }
    }
}
