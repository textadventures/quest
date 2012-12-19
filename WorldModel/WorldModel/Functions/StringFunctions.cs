using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Functions
{
    public static class StringFunctions
    {
        public static string Left(string input, int length)
        {
            return Microsoft.VisualBasic.Strings.Left(input, length);
        }

        public static string Right(string input, int length)
        {
            return Microsoft.VisualBasic.Strings.Right(input, length);
        }

        public static string Mid(string input, int start)
        {
            return Microsoft.VisualBasic.Strings.Mid(input, start);
        }

        public static string Mid(string input, int start, int length)
        {
            return Microsoft.VisualBasic.Strings.Mid(input, start, length);
        }

        public static string UCase(string input)
        {
            return input.ToUpper();
        }

        public static string LCase(string input)
        {
            return input.ToLower();
        }

        public static int LengthOf(string input)
        {
            return Microsoft.VisualBasic.Strings.Len(input);
        }

        public static string CapFirst(string input)
        {
            return TextAdventures.Utility.Strings.CapFirst(input);
        }

        public static int Instr(string input, string search)
        {
            return Microsoft.VisualBasic.Strings.InStr(input, search, Microsoft.VisualBasic.CompareMethod.Binary);
        }

        public static int Instr(int start, string input, string search)
        {
            return Microsoft.VisualBasic.Strings.InStr(start, input, search, Microsoft.VisualBasic.CompareMethod.Binary);
        }

        public static int InstrRev(string input, string search)
        {
            return Microsoft.VisualBasic.Strings.InStrRev(input, search, Compare: Microsoft.VisualBasic.CompareMethod.Binary);
        }

        public static int InstrRev(int start, string input, string search)
        {
            return Microsoft.VisualBasic.Strings.InStrRev( input, search, start, Microsoft.VisualBasic.CompareMethod.Binary);
        }

        public static bool StartsWith(string input, string search)
        {
            return input.StartsWith(search);
        }

        public static bool EndsWith(string input, string search)
        {
            return input.EndsWith(search);
        }

        public static QuestList<string> Split(string input, string splitChar)
        {
            return new QuestList<string>(input.Split(new string[] { splitChar }, StringSplitOptions.None));
        }

        public static string Join(IEnumerable<string> input, string joinChar)
        {
            return string.Join(joinChar, input);
        }

        public static bool IsNumeric(string input)
        {
            return TextAdventures.Utility.Strings.IsNumeric(input);
        }

        public static string Replace(string input, string oldString, string newString)
        {
            return input.Replace(oldString, newString);
        }
        
        public static string Trim(string input)
        {
            return Microsoft.VisualBasic.Strings.Trim(input);
        }

        public static string LTrim(string input)
        {
            return Microsoft.VisualBasic.Strings.LTrim(input);
        }
        
        public static string RTrim(string input)
        {
            return Microsoft.VisualBasic.Strings.RTrim(input);
        }
        
        public static int Asc(string input)
        {
            return Microsoft.VisualBasic.Strings.Asc(input);
        }

        public static string Chr(int input)
        {
            return Microsoft.VisualBasic.Strings.Chr(input).ToString();
        }
    }
}
