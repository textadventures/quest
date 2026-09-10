using Microsoft.VisualBasic;

namespace QuestViva.Legacy;

public class TextFormatter
{
    private string _align = "";
    // the Player generates style tags for us
    // so all we need to do is have some kind of <color> <fontsize> <justify> tags etc.
    // it would actually be a really good idea for the player to handle the <wait> and <clear> tags too...?

    private bool _bold;
    private string _colour = "";
    private int _fontSize;
    private bool _italic;
    private bool _underline;

    public string OutputHTML(string input)
    {
        var output = "";
        var position = 0;
        int codePosition;
        var finished = false;
        var nobr = false;

        input = input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
            .Replace(Constants.vbCrLf, "<br />");

        if (Strings.Right(input, 3) == "|xn")
        {
            nobr = true;
            input = Strings.Left(input, Strings.Len(input) - 3);
        }

        do
        {
            codePosition = input.IndexOf("|", position);
            if (codePosition == -1)
            {
                output += FormatText(input.Substring(position));
                finished = true;
            }
            else
            {
                output += FormatText(input.Substring(position, codePosition - position));
                position = codePosition + 1;

                var oneCharCode = "";
                var twoCharCode = "";
                if (position < input.Length)
                {
                    oneCharCode = input.Substring(position, 1);
                }

                if (position < input.Length - 1)
                {
                    twoCharCode = input.Substring(position, 2);
                }

                var foundCode = true;

                switch (twoCharCode ?? "")
                {
                    case "xb":
                    {
                        _bold = false;
                        break;
                    }
                    case "xi":
                    {
                        _italic = false;
                        break;
                    }
                    case "xu":
                    {
                        _underline = false;
                        break;
                    }
                    case "cb":
                    {
                        _colour = "";
                        break;
                    }
                    case "cr":
                    {
                        _colour = "red";
                        break;
                    }
                    case "cl":
                    {
                        _colour = "blue";
                        break;
                    }
                    case "cy":
                    {
                        _colour = "yellow";
                        break;
                    }
                    case "cg":
                    {
                        _colour = "green";
                        break;
                    }
                    case "jl":
                    {
                        _align = "";
                        break;
                    }
                    case "jc":
                    {
                        _align = "center";
                        break;
                    }
                    case "jr":
                    {
                        _align = "right";
                        break;
                    }

                    default:
                    {
                        foundCode = false;
                        break;
                    }
                }

                if (foundCode)
                {
                    position += 2;
                }
                else
                {
                    foundCode = true;
                    switch (oneCharCode ?? "")
                    {
                        case "b":
                        {
                            _bold = true;
                            break;
                        }
                        case "i":
                        {
                            _italic = true;
                            break;
                        }
                        case "u":
                        {
                            _underline = true;
                            break;
                        }
                        case "n":
                        {
                            output += "<br />";
                            break;
                        }

                        default:
                        {
                            foundCode = false;
                            break;
                        }
                    }

                    if (foundCode)
                    {
                        position += 1;
                    }
                }

                if (!foundCode)
                {
                    if (oneCharCode == "s")
                    {
                        // |s00 |s10 etc.
                        if (position < input.Length - 2)
                        {
                            var sizeCode = input.Substring(position + 1, 2);
                            if (int.TryParse(sizeCode, out _fontSize))
                            {
                                foundCode = true;
                                position += 3;
                            }
                        }
                    }
                }

                if (!foundCode)
                {
                    output += "|";
                }

                // can also have size codes
            }
        } while (!(finished | (position >= input.Length)));

        return string.Format("<output{0}>{1}</output>", nobr ? " nobr=\"true\"" : "", output);
    }

    private string FormatText(string input)
    {
        if (input.Length == 0)
        {
            return input;
        }

        var output = "";

        if (_align.Length > 0)
        {
            output += "<align align=\"" + _align + "\">";
        }

        if (_fontSize > 0)
        {
            output += "<font size=\"" + _fontSize + "\">";
        }

        if (_colour.Length > 0)
        {
            output += "<color color=\"" + _colour + "\">";
        }

        if (_bold)
        {
            output += "<b>";
        }

        if (_italic)
        {
            output += "<i>";
        }

        if (_underline)
        {
            output += "<u>";
        }

        output += input;
        if (_underline)
        {
            output += "</u>";
        }

        if (_italic)
        {
            output += "</i>";
        }

        if (_bold)
        {
            output += "</b>";
        }

        if (_colour.Length > 0)
        {
            output += "</color>";
        }

        if (_fontSize > 0)
        {
            output += "</font>";
        }

        if (_align.Length > 0)
        {
            output += "</align>";
        }

        return output;
    }
}