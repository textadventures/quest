using Microsoft.VisualBasic;

namespace TextAdventures.Quest.LegacyASL
{
    public class TextFormatter
    {
        // the Player generates style tags for us
        // so all we need to do is have some kind of <color> <fontsize> <justify> tags etc.
        // it would actually be a really good idea for the player to handle the <wait> and <clear> tags too...?

        private bool bold;
        private bool italic;
        private bool underline;
        private string colour = "";
        private int fontSize = 0;
        private string align = "";

        public string OutputHTML(string input)
        {
            string output = "";
            int position = 0;
            int codePosition;
            bool finished = false;
            bool nobr = false;

            input = input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace(Constants.vbCrLf, "<br />");

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

                    string oneCharCode = "";
                    string twoCharCode = "";
                    if (position < input.Length)
                    {
                        oneCharCode = input.Substring(position, 1);
                    }
                    if (position < input.Length - 1)
                    {
                        twoCharCode = input.Substring(position, 2);
                    }

                    bool foundCode = true;

                    switch (twoCharCode ?? "")
                    {
                        case "xb":
                            {
                                bold = false;
                                break;
                            }
                        case "xi":
                            {
                                italic = false;
                                break;
                            }
                        case "xu":
                            {
                                underline = false;
                                break;
                            }
                        case "cb":
                            {
                                colour = "";
                                break;
                            }
                        case "cr":
                            {
                                colour = "red";
                                break;
                            }
                        case "cl":
                            {
                                colour = "blue";
                                break;
                            }
                        case "cy":
                            {
                                colour = "yellow";
                                break;
                            }
                        case "cg":
                            {
                                colour = "green";
                                break;
                            }
                        case "jl":
                            {
                                align = "";
                                break;
                            }
                        case "jc":
                            {
                                align = "center";
                                break;
                            }
                        case "jr":
                            {
                                align = "right";
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
                                    bold = true;
                                    break;
                                }
                            case "i":
                                {
                                    italic = true;
                                    break;
                                }
                            case "u":
                                {
                                    underline = true;
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
                                string sizeCode = input.Substring(position + 1, 2);
                                if (int.TryParse(sizeCode, out fontSize))
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
            }

            while (!(finished | position >= input.Length));

            return string.Format("<output{0}>{1}</output>", nobr ? " nobr=\"true\"" : "", output);

        }

        private string FormatText(string input)
        {
            if (input.Length == 0)
                return input;

            string output = "";

            if (align.Length > 0)
                output += "<align align=\"" + align + "\">";
            if (fontSize > 0)
                output += "<font size=\"" + fontSize.ToString() + "\">";
            if (colour.Length > 0)
                output += "<color color=\"" + colour + "\">";
            if (bold)
                output += "<b>";
            if (italic)
                output += "<i>";
            if (underline)
                output += "<u>";
            output += input;
            if (underline)
                output += "</u>";
            if (italic)
                output += "</i>";
            if (bold)
                output += "</b>";
            if (colour.Length > 0)
                output += "</color>";
            if (fontSize > 0)
                output += "</font>";
            if (align.Length > 0)
                output += "</align>";

            return output;
        }

    }
}