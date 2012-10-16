using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace TextAdventures.Quest
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
            // remove surrounding quotes
            string inner = expression.Substring(1, expression.Length - 2);

            // replace newline markup
            inner = inner.Replace("<br/>", Environment.NewLine);

            // replace escaped quotes with unescaped quotes, i.e. replace \" with "
            return inner.Replace("\\\"", "\"");
        }

        public static string ConvertFromSimpleStringExpression(string simpleValue)
        {
            // escape quotes
            string result = simpleValue.Replace("\"", "\\\"");

            // markup newlines
            result = result.Replace(Environment.NewLine, "<br/>");

            // surround with quotes
            return string.Format("\"{0}\"", result);
        }

        public static string GetUniqueFilename(string filename)
        {
            int i = 1;
            string directory = Path.GetDirectoryName(filename);
            string baseFilename = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string newFilename;
            do
            {
                i++;
                newFilename = Path.Combine(directory, baseFilename + " " + i.ToString() + extension);
            }
            while (System.IO.File.Exists(newFilename));
            return newFilename;
        }
    }
}
