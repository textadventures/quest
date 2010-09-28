using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        private static System.Text.RegularExpressions.Regex s_convertVariables = new System.Text.RegularExpressions.Regex(@"(\w)\.(\w)");
        
        /// <summary>
        /// FLEE doesn't allow us to have control over dot notation i.e. "object.property",
        /// so instead we handle "object_property". Use this function to convert dot to underscore.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ConvertDottedPropertiesToVariable(string expression)
        {
            // TO DO: This needs to ignore the pattern if it's inside quotes.
            // Again as for the solution to stripping out comments in ScriptFactory.cs, solution is
            // probably to see if we have a match for this regex, but then go through the expression
            // one character at a time, set flag when within quote characters, and replace . with _
            // as appropriate.
            return s_convertVariables.Replace(expression, "$1_$2");
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
    }
}
