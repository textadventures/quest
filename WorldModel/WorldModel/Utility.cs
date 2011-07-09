using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AxeSoftware.Quest
{
    public static class Utility
    {
        private const string k_dotReplacementString = "___DOT___";
        private const string k_spaceReplacementString = "___SPACE___";

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
            int bracePos = script.IndexOf('{');
            int crlfPos = script.IndexOf("\n");
            if (crlfPos == -1) return script;

            if (bracePos == -1 || crlfPos < bracePos)
            {
                afterScript = script.Substring(crlfPos + 1);
                return script.Substring(0, crlfPos);
            }

            string beforeBrace = script.Substring(0, bracePos);
            string insideBraces = GetParameterInt(script, '{', '}', out afterScript);

            string result;

            if (insideBraces.Contains("\n"))
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
            int start = text.IndexOf(open);
            if (start == -1) return null;

            bool finished = false;
            int braceCount = 1;
            int pos = start;

            while (!finished)
            {
                pos++;
                string curChar = text.Substring(pos, 1);
                if (curChar == open.ToString()) braceCount++;
                if (curChar == close.ToString()) braceCount--;
                if (braceCount == 0 || pos == text.Length - 1) finished = true;
            }

            if (braceCount != 0)
            {
                throw new Exception(string.Format("Missing '{0}'", close));
            }

            afterParameter = text.Substring(pos + 1);
            return text.Substring(start + 1, pos - start - 1);
        }

        public static string RemoveSurroundingBraces(string input)
        {
            input = input.Trim();
            if (input.StartsWith("{") && input.EndsWith("}"))
            {
                return input.Substring(1, input.Length - 2);
            }
            else
            {
                return input;
            }
        }

        public static string GetTextAfter(string text, string startsWith)
        {
            return text.Substring(startsWith.Length);
        }

        public static List<string> SplitParameter(string text)
        {
            List<string> result = new List<string>();
            bool inQuote = false;
            bool processThisCharacter;
            bool processNextCharacter = true;
            int bracketCount = 0;
            string curParam = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                processThisCharacter = processNextCharacter;
                processNextCharacter = true;

                string curChar = text.Substring(i, 1);

                if (processThisCharacter)
                {
                    if (curChar == "\\")
                    {
                        // Don't process the character after a backslash
                        processNextCharacter = false;
                    }
                    else if (curChar == "\"")
                    {
                        inQuote = !inQuote;
                    }
                    else
                    {
                        if (!inQuote)
                        {
                            if (curChar == "(") bracketCount++;
                            if (curChar == ")") bracketCount--;
                            if (bracketCount == 0 && curChar == ",")
                            {
                                result.Add(curParam.Trim());
                                curParam = string.Empty;
                                continue;
                            }
                        }
                    }
                }

                curParam += curChar;
            }
            result.Add(curParam.Trim());

            return result;
        }

        public static void ResolveVariableName(ref string name, out string obj, out string variable)
        {
            name = ResolveElementName(name);
            int eqPos = name.IndexOf(k_dotReplacementString);
            if (eqPos == -1)
            {
                obj = null;
                variable = name;
                return;
            }

            obj = name.Substring(0, eqPos);
            variable = name.Substring(eqPos + k_dotReplacementString.Length);
        }

        public static string ResolveElementName(string name)
        {
            return name.Replace(k_spaceReplacementString, " ");
        }

        private static Regex s_convertVariables = new System.Text.RegularExpressions.Regex(@"(\w)\.([a-zA-Z])");
        private static Regex s_detectComments = new Regex("//");

        /// <summary>
        /// FLEE doesn't allow us to have control over dot notation i.e. "object.property",
        /// so instead we handle "object_property". Use this function to convert dot to underscore.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ConvertVariablesToFleeFormat(string expression)
        {
            string result = ReplaceRegexMatchesRespectingQuotes(expression, s_convertVariables, "$1" + k_dotReplacementString + "$2", false);
            return ConvertVariableNamesWithSpaces(result);
        }

        public static string ConvertFleeFormatToVariables(string expression)
        {
            return expression.Replace(k_dotReplacementString, ".").Replace(k_spaceReplacementString, " ");
        }

        private static string ReplaceRegexMatchesRespectingQuotes(string input, Regex regex, string replaceWith, bool replaceInsideQuote)
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
                string[] words = text.Split(' ');
                string result = words[0];

                if (words.Length == 1) return result;

                for (int i = 1; i < words.Length; i++)
                {
                    if (IsSplitVariableName(words[i - 1], words[i]))
                    {
                        result += k_spaceReplacementString;
                    }
                    else
                    {
                        result += " ";
                    }
                    result += words[i];
                }

                return result;
            });
        }

        // Given two words e.g. "my" and "variable", see if they together comprise a variable name

        private static Regex s_wordRegex1 = new System.Text.RegularExpressions.Regex(@"(\w+)$");
        private static Regex s_wordRegex2 = new System.Text.RegularExpressions.Regex(@"^(\w+)");
        private static List<string> s_keywords = new List<string> { "and", "or", "xor", "not", "if", "in" };

        private static bool IsSplitVariableName(string word1, string word2)
        {
            if (!(s_wordRegex1.IsMatch(word1) && s_wordRegex2.IsMatch(word2))) return false;

            string word1last = s_wordRegex1.Match(word1).Groups[1].Value;
            string word2first = s_wordRegex2.Match(word2).Groups[1].Value;

            if (s_keywords.Contains(word1last)) return false;
            if (s_keywords.Contains(word2first)) return false;
            return true;
        }

        public static IList<string> ExpressionKeywords
        {
            get { return s_keywords.AsReadOnly(); }
        }

        private static string ReplaceRespectingQuotes(string input, bool replaceInsideQuote, Func<string, string> replaceFunction)
        {
            // We ignore regex matches which appear within quotes by splitting the string
            // at the position of quote marks, and then alternating whether we replace or not.
            string[] sections = input.Split('\"');
            string result = string.Empty;

            bool insideQuote = false;
            for (int i = 0; i <= sections.Length - 1; i++)
            {
                string section = sections[i];
                bool doReplace = (insideQuote && replaceInsideQuote) || (!insideQuote && !replaceInsideQuote);
                if (doReplace)
                {
                    result += replaceFunction(section);
                }
                else
                {
                    result += section;
                }
                if (i < sections.Length - 1) result += "\"";
                insideQuote = !insideQuote;
            }

            return result;
        }

        public static string RemoveComments(string input)
        {
            if (!input.Contains("//")) return input;
            if (input.Contains("\n"))
            {
                return RemoveCommentsMultiLine(input);
            }

            // Replace any occurrences of "//" which are inside string expressions. Then any occurrences of "//"
            // which remain mark the beginning of a comment.
            string obfuscateDoubleSlashesInsideStrings = ReplaceRegexMatchesRespectingQuotes(input, s_detectComments, "--", true);
            if (obfuscateDoubleSlashesInsideStrings.Contains("//"))
            {
                return input.Substring(0, obfuscateDoubleSlashesInsideStrings.IndexOf("//"));
            }
            return input;
        }

        private static string RemoveCommentsMultiLine(string input)
        {
            List<string> output = new List<string>();
            foreach (string inputLine in (input.Split(new string[] { "\n" }, StringSplitOptions.None)))
            {
                output.Add(RemoveComments(inputLine));
            }
            return string.Join("\n", output.ToArray());
        }

        public static List<string> SplitIntoLines(string text)
        {
            List<string> lines = new List<string>(text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
            List<string> result = new List<string>();
            foreach (string line in lines)
            {
                string trimLine = line.Trim();
                if (trimLine.Length > 0) result.Add(trimLine);
            }
            return result;
        }

        public static string SafeXML(string input)
        {
            return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string[] s_listSplitDelimiters = new string[] { "; ", ";" };

        public static string[] ListSplit(string value)
        {
            return value.Split(s_listSplitDelimiters, StringSplitOptions.None);
        }

        public static string ObscureStrings(string input)
        {
            string[] sections = input.Split('\"');
            string result = string.Empty;

            bool insideQuote = false;
            for (int i = 0; i <= sections.Length - 1; i++)
            {
                string section = sections[i];
                if (insideQuote)
                {
                    result = result + new string('-', section.Length);
                }
                else
                {
                    result = result + section;
                }
                if (i < sections.Length - 1) result += "\"";
                insideQuote = !insideQuote;
            }
            return result;
        }

        public static bool ContainsUnresolvedDotNotation(string input)
        {
            return input.Contains(k_dotReplacementString);
        }

        public static bool IsRegexMatch(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return regex.IsMatch(input);
        }

        public static int GetMatchStrength(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!regex.IsMatch(input)) throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regexPattern));

            // The idea is that you have a regex like
            //          look at (?<object>.*)
            // And you have a string like
            //          look at thing
            // The strength is the length of the "fixed" bit of the string, in this case "look at ".
            // So we calculate this as the length of the input string, minus the length of the
            // text that matches the named groups.

            int lengthOfTextMatchedByGroups = 0;

            foreach (string groupName in regex.GetGroupNames())
            {
                // exclude group names like "0", we only want the explicitly named groups
                if (!AxeSoftware.Utility.Strings.IsNumeric(groupName))
                {
                    string groupMatch = regex.Match(input).Groups[groupName].Value;
                    lengthOfTextMatchedByGroups += groupMatch.Length;
                }
            }

            return input.Length - lengthOfTextMatchedByGroups;
        }

        public static QuestDictionary<string> Populate(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!regex.IsMatch(input)) throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regexPattern));

            QuestDictionary<string> result = new QuestDictionary<string>();

            foreach (string groupName in regex.GetGroupNames())
            {
                if (!AxeSoftware.Utility.Strings.IsNumeric(groupName))
                {
                    string groupMatch = regex.Match(input).Groups[groupName].Value;
                    result.Add(groupName, groupMatch);
                }
            }

            return result;
        }

        public static string ConvertVerbSimplePattern(string pattern)
        {
            // For verbs, we replace "eat; consume; munch" with
            // "^eat (?<object>.*)$|^consume (?<object>.*)$|^munch (?<object>.*)$"
            
            // Optionally the position of the object can be specified, for example
            // "switch #object# on" would become "^switch (?<object>.*) on$"

            string[] verbs = Utility.ListSplit(pattern);
            string result = string.Empty;
            foreach (string verb in verbs)
            {
                if (result.Length > 0) result += "|";
                const string objectRegex = "(?<object>.*)";

                string textToAdd;
                if (verb.Contains("#object#"))
                {
                    textToAdd = "^" + verb.Replace("#object#", objectRegex) + "$";
                }
                else
                {
                    textToAdd = "^" + verb + " " + objectRegex + "$";
                }
                result += textToAdd;
            }

            return result;
        }

        // Valid attribute names:
        //  - must not start with a number
        //  - must not contain keywords "and", "or" etc.
        //  - can contain spaces, but not at the beginning or end
        private static Regex s_validAttributeName = new Regex(@"^[A-Za-z][\w ]*\w$");

        public static bool IsValidAttributeName(string name)
        {
            if (!s_validAttributeName.IsMatch(name)) return false;
            if (name.Split(' ').Any(w => s_keywords.Contains(w))) return false;
            return true;
        }
    }
}
