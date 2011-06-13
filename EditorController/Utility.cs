using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AxeSoftware.Quest
{
    public static class EditorUtility
    {
        public static string FormatAsOneLine(string input)
        {
            if (input == null) return null;
            return input.Replace(Environment.NewLine, " / ");
        }

        private static Regex s_containsUnescapedQuote = new Regex("^\"|[^\\\\]\\\"");

        public static bool IsSimpleStringExpression(string expression)
        {
            // must start and end with quote character
            if (!(expression.StartsWith("\"") && expression.EndsWith("\""))) return false;

            string inner = expression.Substring(1, expression.Length - 2);

            // must not contain an unescaped quote character
            return !s_containsUnescapedQuote.IsMatch(inner);
        }

        public static string ConvertToSimpleStringExpression(string expression)
        {
            string inner = expression.Substring(1, expression.Length - 2);

            // replace escaped quotes with unescaped quotes, i.e. replace \" with "
            return inner.Replace("\\\"", "\"");
        }

        public static string ConvertFromSimpleStringExpression(string simpleValue)
        {
            string result = simpleValue.Replace("\"", "\\\"");
            return string.Format("\"{0}\"", result);
        }
    }
}
