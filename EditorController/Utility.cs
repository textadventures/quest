using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public static class EditorUtility
    {
        public static string FormatAsOneLine(string input)
        {
            if (input == null) return null;
            return input.Replace(Environment.NewLine, " / ");
        }
    }
}
