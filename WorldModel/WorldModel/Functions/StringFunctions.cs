using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest.Functions
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
            return Utility.CapFirst(input);
        }

        public static int Instr(string input, string search)
        {
            return Microsoft.VisualBasic.Strings.InStr(input, search, Microsoft.VisualBasic.CompareMethod.Binary);
        }

        public static int Instr(int start, string input, string search)
        {
            return Microsoft.VisualBasic.Strings.InStr(start, input, search, Microsoft.VisualBasic.CompareMethod.Binary);
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

        public static bool IsNumeric(string input)
        {
            return AxeSoftware.Utility.Strings.IsNumeric(input);
        }

        public static string Replace(string input, string oldString, string newString)
        {
            return input.Replace(oldString, newString);
        }
    }
}
