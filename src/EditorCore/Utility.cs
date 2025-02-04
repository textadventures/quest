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
            if (string.IsNullOrEmpty(expression)) return false;

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

        public static string GetDisplayString(object value)
        {
            IEditableScripts scriptValue = value as IEditableScripts;
            IEditableList<string> listStringValue = value as IEditableList<string>;
            IEditableDictionary<string> dictionaryStringValue = value as IEditableDictionary<string>;
            IEditableDictionary<IEditableScripts> dictionaryScriptValue = value as IEditableDictionary<IEditableScripts>;
            IDataWrapper wrappedValue = value as IDataWrapper;
            string result = null;

            if (scriptValue != null)
            {
                result = scriptValue.DisplayString();
            }
            else if (listStringValue != null)
            {
                result = GetListDisplayString(listStringValue.DisplayItems);
            }
            else if (dictionaryStringValue != null)
            {
                result = GetDictionaryDisplayString(dictionaryStringValue.DisplayItems);
            }
            else if (dictionaryScriptValue != null)
            {
                result = GetDictionaryDisplayString(dictionaryScriptValue.DisplayItems);
            }
            else if (wrappedValue != null)
            {
                result = wrappedValue.DisplayString();
            }
            else if (value == null)
            {
                result = "(null)";
            }
            else
            {
                result = value.ToString();
            }

            return FormatAsOneLine(result);
        }

        static string GetListDisplayString(IEnumerable<KeyValuePair<string, string>> items)
        {
            string result = string.Empty;

            foreach (var item in items)
            {
                if (result.Length > 0)
                    result += ", ";
                result += item.Value;
            }

            return result;
        }

        static string GetDictionaryDisplayString(IEnumerable<KeyValuePair<string, string>> items)
        {
            string result = string.Empty;

            foreach (var item in items)
            {
                if (result.Length > 0)
                    result += ", ";
                result += item.Key + "=" + item.Value;
            }

            return result;
        }
    }
}
