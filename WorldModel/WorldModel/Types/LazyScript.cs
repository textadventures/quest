using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Types
{
    internal class LazyScript
    {
        public LazyScript(string script)
        {
            Script = script;
        }

        public string Script { get; private set; }
    }
}
