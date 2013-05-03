using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Utility
{
    public static class Strings
    {
        public static bool IsNumeric(string expression)
        {
            return Microsoft.VisualBasic.Information.IsNumeric(expression);
        }

        public static string CapFirst(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }
    }
}
