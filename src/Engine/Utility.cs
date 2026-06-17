#nullable disable
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using QuestViva.Utility;

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
    private const string k_dotReplacementString = "___DOT___";
    private const string k_spaceReplacementString = "___SPACE___";

    public static readonly string[] DisallowedAttributes =
        {"object", "command", "turnscript", "game", "exit", "type", "finish"};

    private static readonly List<string> s_keywords = new() {"and", "or", "xor", "not", "if", "in"};

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
                            bracketCount--;
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

    public static void ResolveVariableName(ref string name, out string obj, out string variable)
    {
        name = ResolveElementName(name);
        var eqPos = name.IndexOf(k_dotReplacementString);
        if (eqPos == -1)
        {
            obj = null;
            variable = name;
            return;
        }

        obj = name[..eqPos];
        variable = name[(eqPos + k_dotReplacementString.Length)..];
    }

    public static void ResolveObjectDotAttribute(string name, out string obj, out string variable)
    {
        variable = ConvertVariablesToFleeFormat(ResolveElementName(name));
        StringBuilder sb = null;

        do
        {
            if (!ContainsUnresolvedDotNotation(variable))
            {
                continue;
            }

            // We may have been passed in something like someobj.parent.someproperty
            ResolveVariableName(ref variable, out var nestedObj, out variable);

            if (nestedObj == null)
            {
                continue;
            }

            if (sb == null)
            {
                sb = new StringBuilder();
            }
            else
            {
                sb.Append('.');
            }

            sb.Append(nestedObj);
        } while (ContainsUnresolvedDotNotation(variable));

        obj = sb?.ToString();
    }

    public static string ResolveElementName(string name)
    {
        return name.Replace(k_spaceReplacementString, " ");
    }

    // private static Regex s_convertVariables = new System.Text.RegularExpressions.Regex(@"(\w)\.([a-zA-Z])");
    [GeneratedRegex(@"(\w)\.([a-zA-Z])")]
    private static partial Regex s_convertVariables();

    [GeneratedRegex(@"//")]
    private static partial Regex s_detectComments();

    /// <summary>
    ///     FLEE doesn't allow us to have control over dot notation i.e. "object.property",
    ///     so instead we handle "object_property". Use this function to convert dot to underscore.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static string ConvertVariablesToFleeFormat(string expression)
    {
        var result = ReplaceRegexMatchesRespectingQuotes(expression, s_convertVariables(),
            "$1" + k_dotReplacementString + "$2", false);
        return ConvertVariableNamesWithSpaces(result);
    }

    public static string ConvertFleeFormatToVariables(string expression)
    {
        return expression.Replace(k_dotReplacementString, ".").Replace(k_spaceReplacementString, " ");
    }

    private static string ReplaceRegexMatchesRespectingQuotes(string input, Regex regex, string replaceWith,
        bool replaceInsideQuote)
    {
        return ReplaceRespectingQuotes(input, replaceInsideQuote, text => regex.Replace(text, replaceWith));
    }

    private static string ConvertVariableNamesWithSpaces(string input)
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
        if (!(s_wordRegex1().IsMatch(word1) && s_wordRegex2().IsMatch(word2)))
        {
            return false;
        }

        var word1last = s_wordRegex1().Match(word1).Groups[1].Value;
        var word2first = s_wordRegex2().Match(word2).Groups[1].Value;

        if (s_keywords.Contains(word1last))
        {
            return false;
        }

        if (s_keywords.Contains(word2first))
        {
            return false;
        }

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

    public static bool ContainsUnresolvedDotNotation(string input)
    {
        return input.Contains(k_dotReplacementString);
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
        if (!regex.IsMatch(input))
        {
            throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regex));
        }

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
            if (!Strings.IsNumeric(groupName))
            {
                var groupMatch = regex.Match(input).Groups[groupName].Value;
                lengthOfTextMatchedByGroups += groupMatch.Length;
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
        if (!regex.IsMatch(input))
        {
            throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regex));
        }

        var result = new QuestDictionary<string>();

        foreach (var groupName in regex.GetGroupNames())
        {
            if (!Strings.IsNumeric(groupName))
            {
                var groupMatch = regex.Match(input).Groups[groupName].Value;
                result.Add(groupName, groupMatch);
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
            separatorRegex = "(" + string.Join("|", separator.Split(';').Select(s => s.Trim())) + ")";
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
                textToAdd = "^" + verb.Replace("#object#", objectRegex);
            }
            else
            {
                textToAdd = "^" + verb + " " + objectRegex;
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
        var result = Environment.NewLine;

        foreach (var line in lines)
        {
            AddLine(ref result, ref indentLevel, line, indentChars);
        }

        return result;
    }

    private static void AddLine(ref string result, ref int indentLevel, string line, string indentChars)
    {
        if (line.Length == 0)
        {
            return;
        }

        if (line.StartsWith("}"))
        {
            // if line starts with closing brace, de-indent, put the brace on a line on its own,
            // then resume with the rest of the line.
            indentLevel--;
            result += GetIndentChars(indentLevel, indentChars) + "}" + Environment.NewLine;
            AddLine(ref result, ref indentLevel, line[1..], indentChars);
            return;
        }

        // Add this line at the current indent level
        result += GetIndentChars(indentLevel, indentChars) + line + Environment.NewLine;

        // Now work out the indent level for the following line
        for (var i = 0; i < line.Length; i++)
        {
            var curChar = line.Substring(i, 1);
            if (curChar == "{")
            {
                indentLevel++;
            }

            if (curChar == "}")
            {
                indentLevel--;
            }
        }
    }

    public static string GetIndentChars(int indentLevel, string indentChars)
    {
        var indentString = string.Empty;

        for (var i = 0; i < indentLevel; i++)
        {
            indentString += indentChars;
        }

        return indentString;
    }
}