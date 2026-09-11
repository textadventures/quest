using System.Text.RegularExpressions;

namespace QuestViva.EditorCore;

public static class EditorUtility
{
    private static readonly Regex ContainsUnescapedQuote = new("^\"|[^\\\\]\\\"");

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
        return !ContainsUnescapedQuote.IsMatch(inner);
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
}