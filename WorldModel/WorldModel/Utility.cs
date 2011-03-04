using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AxeSoftware.Quest
{
    public static class Utility
    {
        public static string CapFirst(string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }

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
            int bracketCount = 0;
            string curParam = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                string curChar = text.Substring(i, 1);

                if (curChar == "\"")
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

                curParam += curChar;
            }
            if (curParam.Length > 0) result.Add(curParam.Trim());

            return result;
        }

        public static void ResolveVariableName(string name, out string obj, out string variable)
        {
            int eqPos = name.IndexOf('_');
            if (eqPos == -1)
            {
                obj = null;
                variable = name;
                return;
            }

            obj = name.Substring(0, eqPos);
            variable = name.Substring(eqPos + 1);
        }

        private static Regex s_convertVariables = new System.Text.RegularExpressions.Regex(@"(\w)\.(\w)");
        private static Regex s_detectComments = new Regex("//");
        
        /// <summary>
        /// FLEE doesn't allow us to have control over dot notation i.e. "object.property",
        /// so instead we handle "object_property". Use this function to convert dot to underscore.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ConvertDottedPropertiesToVariable(string expression)
        {
            return ReplaceRegexMatchesRespectingQuotes(expression, s_convertVariables, "$1_$2", false);
        }

        private static string ReplaceRegexMatchesRespectingQuotes(string input, Regex regex, string replaceWith, bool replaceInsideQuote)
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
                    result += regex.Replace(section, replaceWith);
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
    }
}
