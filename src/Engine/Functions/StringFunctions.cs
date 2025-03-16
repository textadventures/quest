using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using QuestViva.Utility;

namespace QuestViva.Engine.Functions;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class StringFunctions
{
    public static string Left(string? input, int length)
    {
        return Microsoft.VisualBasic.Strings.Left(input, length);
    }

    public static string Right(string? input, int length)
    {
        return Microsoft.VisualBasic.Strings.Right(input, length);
    }

    public static string? Mid(string? input, int start)
    {
        return Microsoft.VisualBasic.Strings.Mid(input, start);
    }

    public static string Mid(string? input, int start, int length)
    {
        return Microsoft.VisualBasic.Strings.Mid(input, start, length);
    }

    public static string UCase(string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return input.ToUpper();
    }

    public static string LCase(string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return input.ToLower();
    }

    public static int LengthOf(string? input)
    {
        return Microsoft.VisualBasic.Strings.Len(input);
    }

    public static string? CapFirst(string? input)
    {
        return Strings.CapFirst(input);
    }

    public static int Instr(string? input, string? search)
    {
        return Microsoft.VisualBasic.Strings.InStr(input, search);
    }

    public static int Instr(int start, string? input, string? search)
    {
        return Microsoft.VisualBasic.Strings.InStr(start, input, search);
    }

    public static int InstrRev(string? input, string? search)
    {
        return Microsoft.VisualBasic.Strings.InStrRev(input, search);
    }

    public static int InstrRev(int start, string? input, string? search)
    {
        return Microsoft.VisualBasic.Strings.InStrRev( input, search, start);
    }

    public static bool StartsWith(string? input, string? search)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(search);
        return input.StartsWith(search);
    }

    public static bool EndsWith(string? input, string? search)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(search);
        return input.EndsWith(search);
    }

    public static QuestList<string> Split(string? input, string? splitChar)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(splitChar);
        return new QuestList<string>(input.Split([splitChar], StringSplitOptions.None));
    }

    public static QuestList<string> Split(string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new QuestList<string>(input.Split([";"], StringSplitOptions.None));
    }

    public static string Join(IEnumerable<string>? input, string? joinChar)
    {
        ArgumentNullException.ThrowIfNull(input);
        return string.Join(joinChar, input);
    }

    public static bool IsNumeric(string? input)
    {
        return Strings.IsNumeric(input);
    }

    public static string Replace(string? input, string? oldString, string? newString)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(oldString);
        ArgumentNullException.ThrowIfNull(newString);
        return input.Replace(oldString, newString);
    }
        
    public static string Trim(string? input)
    {
        return Microsoft.VisualBasic.Strings.Trim(input);
    }

    public static string LTrim(string? input)
    {
        return Microsoft.VisualBasic.Strings.LTrim(input);
    }
        
    public static string RTrim(string? input)
    {
        return Microsoft.VisualBasic.Strings.RTrim(input);
    }
        
    public static int Asc(string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Microsoft.VisualBasic.Strings.Asc(input);
    }

    public static string Chr(int input)
    {
        return Microsoft.VisualBasic.Strings.Chr(input).ToString();
    }
}