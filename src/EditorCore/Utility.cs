using System.Text.RegularExpressions;

namespace QuestViva.EditorCore;

public static class EditorUtility
{
    private static readonly Regex s_containsUnescapedQuote = new("^\"|[^\\\\]\\\"");

    public static string FormatAsOneLine(string input)
    {
        if (input == null)
        {
            return null;
        }

        return input.Replace(Environment.NewLine, " / ");
    }

    public static bool IsSimpleStringExpression(string expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return false;
        }

        // must start and end with quote character
        if (!(expression.StartsWith("\"") && expression.EndsWith("\"")))
        {
            return false;
        }

        var inner = expression.Substring(1, expression.Length - 2);

        // must not contain an unescaped quote character
        return !s_containsUnescapedQuote.IsMatch(inner);
    }

    public static string ConvertToSimpleStringExpression(string expression)
    {
        // remove surrounding quotes
        var inner = expression.Substring(1, expression.Length - 2);

        // replace newline markup
        inner = inner.Replace("<br/>", Environment.NewLine);

        // replace escaped quotes with unescaped quotes, i.e. replace \" with "
        return inner.Replace("\\\"", "\"");
    }

    public static string ConvertFromSimpleStringExpression(string simpleValue)
    {
        // escape quotes
        var result = simpleValue.Replace("\"", "\\\"");

        // markup newlines
        result = result.Replace(Environment.NewLine, "<br/>");

        // surround with quotes
        return string.Format("\"{0}\"", result);
    }

    public static string GetUniqueFilename(string filename)
    {
        var i = 1;
        var directory = Path.GetDirectoryName(filename);
        var baseFilename = Path.GetFileNameWithoutExtension(filename);
        var extension = Path.GetExtension(filename);
        string newFilename;
        do
        {
            i++;
            newFilename = Path.Combine(directory, baseFilename + " " + i + extension);
        } while (File.Exists(newFilename));

        return newFilename;
    }

    public static string GetDisplayString(object value)
    {
        var scriptValue = value as IEditableScripts;
        var listStringValue = value as IEditableList<string>;
        var dictionaryStringValue = value as IEditableDictionary<string>;
        var dictionaryScriptValue = value as IEditableDictionary<IEditableScripts>;
        var wrappedValue = value as IDataWrapper;
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

    private static string GetListDisplayString(IEnumerable<KeyValuePair<string, string>> items)
    {
        var result = string.Empty;

        foreach (var item in items)
        {
            if (result.Length > 0)
            {
                result += ", ";
            }

            result += item.Value;
        }

        return result;
    }

    private static string GetDictionaryDisplayString(IEnumerable<KeyValuePair<string, string>> items)
    {
        var result = string.Empty;

        foreach (var item in items)
        {
            if (result.Length > 0)
            {
                result += ", ";
            }

            result += item.Key + "=" + item.Value;
        }

        return result;
    }
}