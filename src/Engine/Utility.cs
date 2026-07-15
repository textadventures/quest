#nullable disable
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QuestViva.Engine;

[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
public class MismatchingQuotesException : Exception
{
    public MismatchingQuotesException() : base("Missing quote character (\")")
    {
    }

    public MismatchingQuotesException(string message) : base(message)
    {
    }
}

public static partial class Utility
{
    private const string k_spaceReplacementString = "___SPACE___";

    public static readonly string[] DisallowedAttributes =
        {"object", "command", "turnscript", "game", "exit", "type", "finish"};

    private static readonly List<string> s_keywords = new() {"and", "or", "xor", "not", "if", "in"};
    private static readonly HashSet<string> s_keywordsSet = new(s_keywords);

    private static readonly string[] s_listSplitDelimiters = new[] {"; ", ";"};

    public static IList<string> ExpressionKeywords => s_keywords.AsReadOnly();

    public static string GetParameter(string script)
    {
        string afterParameter;
        return GetParameter(script, out afterParameter);
    }

    public static string GetParameter(string script, out string afterParameter)
    {
        return GetParameterInt(script, '(', ')', out afterParameter);
    }

    public static string GetScript(string script)
    {
        string afterScript;
        return GetScript(script, out afterScript);
    }

    public static string GetScript(string script, out string afterScript)
    {
        afterScript = null;
        var obscuredScript = ObscureStrings(script);
        var bracePos = obscuredScript.IndexOf('{');
        var crlfPos = obscuredScript.IndexOf('\n');
        var commentPos = obscuredScript.IndexOf("//", StringComparison.Ordinal);
        if (crlfPos == -1)
        {
            return script;
        }

        if (bracePos == -1 || crlfPos < bracePos || (commentPos != -1 && commentPos < bracePos && commentPos < crlfPos))
        {
            afterScript = script[(crlfPos + 1)..];
            return script[..crlfPos];
        }

        var beforeBrace = script[..bracePos];
        var insideBraces = GetParameterInt(script, '{', '}', out afterScript);

        string result;

        if (insideBraces.Contains('\n'))
        {
            result = beforeBrace + "{" + insideBraces + "}";
        }
        else
        {
            // maybe not necessary or correct, maybe always have it in braces
            result = beforeBrace + insideBraces;
        }

        return result;
    }

    private static string GetParameterInt(string text, char open, char close, out string afterParameter)
    {
        afterParameter = null;
        var obscuredText = ObscureStrings(text);
        var start = obscuredText.IndexOf(open);
        if (start == -1)
        {
            return null;
        }

        var finished = false;
        var braceCount = 1;
        var pos = start;

        while (!finished)
        {
            pos++;
            var curChar = obscuredText[pos];
            if (curChar == open)
            {
                braceCount++;
            }

            if (curChar == close)
            {
                braceCount--;
            }

            if (braceCount == 0 || pos == obscuredText.Length - 1)
            {
                finished = true;
            }
        }

        if (braceCount != 0)
        {
            throw new Exception(string.Format("Missing '{0}'", close));
        }

        afterParameter = text[(pos + 1)..];
        return text.Substring(start + 1, pos - start - 1);
    }

    public static string RemoveSurroundingBraces(string input)
    {
        input = input.Trim();
        if (input.StartsWith("{") && input.EndsWith("}"))
        {
            return input.Substring(1, input.Length - 2);
        }

        return input;
    }

    public static string GetTextAfter(string text, string startsWith)
    {
        return text[startsWith.Length..];
    }

    public static List<string> SplitParameter(string text)
    {
        var result = new List<string>();
        var inQuote = false;
        bool processThisCharacter;
        var processNextCharacter = true;
        var bracketCount = 0;
        var curParam = new StringBuilder();

        foreach (var c in text)
        {
            processThisCharacter = processNextCharacter;
            processNextCharacter = true;

            if (processThisCharacter)
            {
                if (c == '\\')
                {
                    // Don't process the character after a backslash
                    processNextCharacter = false;
                }
                else if (c == '"')
                {
                    inQuote = !inQuote;
                }
                else
                {
                    if (!inQuote)
                    {
                        if (c == '(')
                        {
                            bracketCount++;
                        }

                        if (c == ')')
                        {
                            bracketCount = Math.Max(0, bracketCount - 1);
                        }

                        if (bracketCount == 0 && c == ',')
                        {
                            result.Add(curParam.ToString().Trim());
                            curParam.Clear();
                            continue;
                        }
                    }
                }
            }

            curParam.Append(c);
        }

        result.Add(curParam.ToString().Trim());

        return result;
    }

    public static string ResolveElementName(string name)
    {
        return name.Replace(k_spaceReplacementString, " ");
    }

    [GeneratedRegex(@"//")]
    private static partial Regex s_detectComments();

    private static string ReplaceRegexMatchesRespectingQuotes(string input, Regex regex, string replaceWith,
        bool replaceInsideQuote)
    {
        return ReplaceRespectingQuotes(input, replaceInsideQuote, text => regex.Replace(text, replaceWith));
    }

    public static string EncodeIdentifierSpaces(string input)
    {
        return ReplaceRespectingQuotes(input, false, text =>
        {
            // Split the text up into a word array, for example
            // "my variable = 12" becomes "my", "variable", "=", "12".
            // If any two adjacent elements end and begin with word characters,
            // (in this case, "my" and "variable" because "y" and "v" match
            // regex "\w"), then we remove the space and replace it with
            // our space placeholder. However, if one of the two words is a
            // keyword ("and", "not" etc.), we don't convert it.
            var words = text.Split(' ');

            if (words.Length == 1)
            {
                return words[0];
            }

            var result = new StringBuilder(words[0]);

            for (var i = 1; i < words.Length; i++)
            {
                if (IsSplitVariableName(words[i - 1], words[i]))
                {
                    result.Append(k_spaceReplacementString);
                }
                else
                {
                    result.Append(' ');
                }

                result.Append(words[i]);
            }

            return result.ToString();
        });
    }

    // Given two words e.g. "my" and "variable", see if they together comprise a variable name

    [GeneratedRegex(@"(\w+)$")]
    private static partial Regex s_wordRegex1();

    [GeneratedRegex(@"^(\w+)")]
    private static partial Regex s_wordRegex2();

    private static bool IsSplitVariableName(string word1, string word2)
    {
        var match1 = s_wordRegex1().Match(word1);
        var match2 = s_wordRegex2().Match(word2);

        if (!match1.Success || !match2.Success) return false;
        if (s_keywordsSet.Contains(match1.Groups[1].Value)) return false;
        if (s_keywordsSet.Contains(match2.Groups[1].Value)) return false;

        return true;
    }

    private static string ReplaceRespectingQuotes(string input, bool replaceInsideQuote,
        Func<string, string> replaceFunction)
    {
        // We ignore regex matches which appear within quotes by splitting the string
        // at the position of quote marks, and then alternating whether we replace or not.
        var sections = SplitQuotes(input);
        var result = new StringBuilder();

        var insideQuote = false;
        for (var i = 0; i <= sections.Length - 1; i++)
        {
            var section = sections[i];
            var doReplace = (insideQuote && replaceInsideQuote) || (!insideQuote && !replaceInsideQuote);
            if (doReplace)
            {
                result.Append(replaceFunction(section));
            }
            else
            {
                result.Append(section);
            }

            if (i < sections.Length - 1)
            {
                result.Append('"');
            }

            insideQuote = !insideQuote;
        }

        return result.ToString();
    }

    private static string[] SplitQuotes(string text)
    {
        var result = new List<string>();
        var processNextCharacter = true;
        var curParam = new StringBuilder();
        var gotCloseQuote = true;

        foreach (var curChar in text)
        {
            var processThisCharacter = processNextCharacter;
            processNextCharacter = true;

            if (processThisCharacter)
            {
                if (curChar == '\\')
                {
                    // Don't process the character after a backslash
                    processNextCharacter = false;
                }
                else
                {
                    if (curChar == '\"')
                    {
                        result.Add(curParam.ToString());
                        gotCloseQuote = !gotCloseQuote;
                        curParam.Clear();
                        continue;
                    }
                }
            }

            curParam.Append(curChar);
        }

        if (!gotCloseQuote)
        {
            throw new MismatchingQuotesException("Missing quote character in " + text);
        }

        result.Add(curParam.ToString());

        return result.ToArray();
    }

    public static string RemoveComments(string input, bool onlyRemoveMidLineComments = false)
    {
        if (!input.Contains("//"))
        {
            return input;
        }

        if (input.Contains('\n'))
        {
            return RemoveCommentsMultiLine(input, onlyRemoveMidLineComments);
        }

        // In the Editor, retain any lines that start with "//", as they will be loaded into CommentScripts.
        // We only want to remove comments that occur after normal script commands.
        if (onlyRemoveMidLineComments && input.Trim().StartsWith("//"))
        {
            return input;
        }

        // Replace any occurrences of "//" which are inside string expressions. Then any occurrences of "//"
        // which remain mark the beginning of a comment.
        var obfuscateDoubleSlashesInsideStrings =
            ReplaceRegexMatchesRespectingQuotes(input, s_detectComments(), "--", true);
        if (obfuscateDoubleSlashesInsideStrings.Contains("//"))
        {
            return input[..obfuscateDoubleSlashesInsideStrings.IndexOf("//", StringComparison.Ordinal)];
        }

        return input;
    }

    private static string RemoveCommentsMultiLine(string input, bool onlyRemoveMidLineComments)
    {
        var output = new List<string>();
        foreach (var inputLine in input.Split(new[] {"\n"}, StringSplitOptions.None))
        {
            output.Add(RemoveComments(inputLine, onlyRemoveMidLineComments));
        }

        return string.Join("\n", output.ToArray());
    }

    public static List<string> SplitIntoLines(string text)
    {
        var lines = new List<string>(text.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries));
        var result = new List<string>();
        foreach (var line in lines)
        {
            var trimLine = line.Trim();
            if (trimLine.Length > 0)
            {
                result.Add(trimLine);
            }
        }

        return result;
    }

    public static string SafeXML(string input)
    {
        return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }

    public static string[] ListSplit(string value)
    {
        return value.Split(s_listSplitDelimiters, StringSplitOptions.None);
    }

    public static string ObscureStrings(string input)
    {
        var sections = SplitQuotes(input);
        var result = new StringBuilder();

        var insideQuote = false;
        for (var i = 0; i <= sections.Length - 1; i++)
        {
            var section = sections[i];
            if (insideQuote)
            {
                result.Append(new string('-', section.Length));
            }
            else
            {
                result.Append(section);
            }

            if (i < sections.Length - 1)
            {
                result.Append('"');
            }

            insideQuote = !insideQuote;
        }

        return result.ToString();
    }

    public static bool IsRegexMatch(string regexPattern, string input)
    {
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
        return regex.IsMatch(input);
    }

    internal static bool IsRegexMatch(string regexPattern, string input, RegexCache cache, string cacheID)
    {
        var regex = cache.GetRegex(regexPattern, cacheID);
        return regex.IsMatch(input);
    }

    public static int GetMatchStrength(string regexPattern, string input)
    {
        return GetMatchStrengthInternal(new Regex(regexPattern, RegexOptions.IgnoreCase), input);
    }

    internal static int GetMatchStrength(string regexPattern, string input, RegexCache cache, string cacheID)
    {
        return GetMatchStrengthInternal(cache.GetRegex(regexPattern, cacheID), input);
    }

    private static int GetMatchStrengthInternal(Regex regex, string input)
    {
        var match = regex.Match(input);
        if (!match.Success)
            throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regex));

        // The idea is that you have a regex like
        //          look at (?<object>.*)
        // And you have a string like
        //          look at thing
        // The strength is the length of the "fixed" bit of the string, in this case "look at ".
        // So we calculate this as the length of the input string, minus the length of the
        // text that matches the named groups.

        var lengthOfTextMatchedByGroups = 0;

        foreach (var groupName in regex.GetGroupNames())
        {
            // exclude group names like "0", we only want the explicitly named groups
            if (!int.TryParse(groupName, out _))
            {
                lengthOfTextMatchedByGroups += match.Groups[groupName].Value.Length;
            }
        }

        return input.Length - lengthOfTextMatchedByGroups;
    }

    public static QuestDictionary<string> Populate(string regexPattern, string input)
    {
        return PopulateInternal(new Regex(regexPattern, RegexOptions.IgnoreCase), input);
    }

    internal static QuestDictionary<string> Populate(string regexPattern, string input, RegexCache cache,
        string cacheID)
    {
        return PopulateInternal(cache.GetRegex(regexPattern, cacheID), input);
    }

    private static QuestDictionary<string> PopulateInternal(Regex regex, string input)
    {
        var match = regex.Match(input);
        if (!match.Success)
            throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regex));

        var result = new QuestDictionary<string>();

        foreach (var groupName in regex.GetGroupNames())
        {
            if (!int.TryParse(groupName, out _))
            {
                result.Add(groupName, match.Groups[groupName].Value);
            }
        }

        return result;
    }

    public static string ConvertVerbSimplePattern(string pattern, string separator)
    {
        // For verbs, we replace "eat; consume; munch" with
        // "^eat (?<object>.*)$|^consume (?<object>.*)$|^munch (?<object>.*)$"

        // Optionally the position of the object can be specified, for example
        // "switch #object# on" would become "^switch (?<object>.*) on$"

        var verbs = ListSplit(pattern);
        var result = string.Empty;
        string separatorRegex = null;

        if (!string.IsNullOrEmpty(separator))
        {
            separatorRegex = "(" + string.Join("|", separator.Split(';').Select(s => Regex.Escape(s.Trim()))) + ")";
        }

        foreach (var verb in verbs)
        {
            if (result.Length > 0)
            {
                result += "|";
            }

            const string objectRegex = "(?<object>.*?)";

            string textToAdd;
            if (verb.Contains("#object#"))
            {
                textToAdd = "^" + string.Join(objectRegex, verb.Split("#object#").Select(Regex.Escape));
            }
            else
            {
                textToAdd = "^" + Regex.Escape(verb) + " " + objectRegex;
            }

            if (separatorRegex != null)
            {
                textToAdd += "( " + separatorRegex + " (?<object2>.*))?";
            }

            textToAdd += "$";

            result += textToAdd;
        }

        return result;
    }

    public static string ConvertVerbSimplePatternForEditor(string pattern)
    {
        // For verbs, we replace "eat; consume; munch" with
        // "eat #object#; consume #object#; munch #object#"

        var verbs = ListSplit(pattern);
        var result = string.Empty;
        foreach (var verb in verbs)
        {
            if (result.Length > 0)
            {
                result += "; ";
            }

            string textToAdd;
            if (verb.Contains("#object#"))
            {
                textToAdd = verb;
            }
            else
            {
                textToAdd = verb + " #object#";
            }

            result += textToAdd;
        }

        return result;
    }

    // Valid attribute names:
    //  - must not start with a number
    //  - must not contain keywords "and", "or" etc.
    //  - can contain spaces, but not at the beginning or end
    //  - cannot be "object", "finish" etc. (attributes only)
    [GeneratedRegex(@"^[A-Za-z_][\w ]*$")]
    private static partial Regex s_validAttributeName();

    public static bool IsValidFieldName(string name)
    {
        if (!s_validAttributeName().IsMatch(name))
        {
            return false;
        }

        if (name.EndsWith(" "))
        {
            return false;
        }

        if (name.Split(' ').Any(w => s_keywords.Contains(w)))
        {
            return false;
        }

        return true;
    }

    public static bool IsValidAttributeName(string name)
    {
        if (!IsValidFieldName(name))
        {
            return false;
        }

        if (DisallowedAttributes.Contains(name))
        {
            return false;
        }

        return true;
    }

    public static string IndentScript(string script, int indentLevel, string indentChars)
    {
        var lines = SplitIntoLines(script);
        var sb = new StringBuilder();
        sb.Append(Environment.NewLine);

        foreach (var line in lines)
        {
            AddLine(sb, ref indentLevel, line, indentChars);
        }

        return sb.ToString();
    }

    private static void AddLine(StringBuilder result, ref int indentLevel, string line, string indentChars)
    {
        if (line.Length == 0) return;

        if (line.StartsWith("}"))
        {
            // if line starts with closing brace, de-indent, put the brace on a line on its own,
            // then resume with the rest of the line.
            indentLevel--;
            result.Append(GetIndentChars(indentLevel, indentChars));
            result.Append('}');
            result.Append(Environment.NewLine);
            AddLine(result, ref indentLevel, line[1..], indentChars);
            return;
        }

        // Add this line at the current indent level
        result.Append(GetIndentChars(indentLevel, indentChars));
        result.Append(line);
        result.Append(Environment.NewLine);

        // Work out the indent level for the following line, ignoring braces inside strings.
        var inString = false;
        var i = 0;
        while (i < line.Length)
        {
            var c = line[i];
            if (inString)
            {
                if (c == '\\') i++; // skip escaped character
                else if (c == '"') inString = false;
            }
            else
            {
                if (c == '"') inString = true;
                else if (c == '{') indentLevel++;
                else if (c == '}') indentLevel--;
            }
            i++;
        }
    }

    public static string GetIndentChars(int indentLevel, string indentChars)
    {
        if (indentLevel <= 0) return string.Empty;
        return string.Concat(Enumerable.Repeat(indentChars, indentLevel));
    }
}