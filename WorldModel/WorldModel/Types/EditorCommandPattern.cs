using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class EditorCommandPattern
    {
        public EditorCommandPattern(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; set; }
    }
}
