using System.Globalization;
using System.Text;

namespace QuestViva.Legacy;

// Culture-independent replacements for VB's Conversion.Str, Strings.Chr and Strings.Asc. VB6 Quest ran on
// Windows with a Western locale, so games and .cas files assume "." decimals, a plain "-" and code page 1252.
// The VB runtime functions instead follow the player's current culture: under sv-SE Str(-10) gives "−10"
// (U+2212), which Conversion.Val reads back as 0, and under e.g. ru-RU Chr(253) is not the .cas record
// delimiter.
internal static class InvariantStrings
{
    private static readonly Encoding Windows1252;

    static InvariantStrings()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Windows1252 = Encoding.GetEncoding(1252);
    }

    // Like VB's Str: a leading space for a non-negative number, which callers Trim where they don't want it.
    public static string Str(double number)
    {
        var result = number.ToString(CultureInfo.InvariantCulture);
        return result.StartsWith('-') ? result : " " + result;
    }

    public static char Chr(int charCode)
    {
        if (charCode is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(charCode));
        }

        return charCode < 128 ? (char)charCode : Windows1252.GetChars([(byte)charCode])[0];
    }

    public static int Asc(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            throw new ArgumentException("Argument must be a non-empty string", nameof(s));
        }

        var c = s[0];
        return c < 128 ? c : Windows1252.GetBytes([c])[0];
    }
}
