using System.Collections.Generic;

namespace QuestViva.Engine.Types
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
