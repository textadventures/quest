using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Types
{
    internal class LazyObjectReference
    {
        public LazyObjectReference(string objectName)
        {
            ObjectName = objectName;
        }

        public string ObjectName { get; private set; }
    }
}
