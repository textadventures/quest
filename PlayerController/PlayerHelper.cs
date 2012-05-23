using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;
using System.Xml;
using System.IO;
using AxeSoftware.Utility.JSInterop;

namespace AxeSoftware.Quest
{
    public interface IPlayerHelperUI : IPlayer
    {
        void OutputText(string text);
        void SetAlignment(string alignment);
        void BindMenu(string linkid, string verbs, string text);
    }

    /// <summary>
    /// Helper class for wrapping functionality that is common to the UIs for both the desktop-based Player
    /// component and WebPlayer.
    /// </summary>
    public class PlayerHelper
    {
        private IASL m_game;
        private IASLTimer m_gameTimer;
        private IPlayerHelperUI m_playerUI;

        private string m_font = "";
        private string m_fontSize = "";
        private string m_foreground = "";
        private string m_foregroundOverride = "";
        private string m_linkForeground = "";
        private string m_fontSizeOverride = "";
        private string m_textBuffer = "";

        public PlayerHelper(IASL game, IPlayerHelperUI playerUI)
        {
            UseGameColours = true;
            UseGameFont = true;
            m_playerUI = playerUI;
            m_game = game;

            m_game.PrintText += m_game_PrintText;

            m_gameTimer = m_game as IASLTimer;
        }

        public bool Initialise(IPlayer player, out List<string> errors)
        {
            bool result = m_game.Initialise(player);
            if (m_game.Errors.Count > 0)
            {
                errors = m_game.Errors;
                return false;
            }
            else
            {
                errors = result ? new List<string> { "Unable to initialise game" } : null;
                return result;
            }
        }

        void m_game_PrintText(string text)
        {
            PrintText(text);
        }

        public void PrintText(string text)
        {
            string currentTagValue = "";
            string currentVerbs = "";
            string currentCommand = "";
            string currentHref = "";
            string currentLinkColour = "";
            bool generatingLink = false;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = false;
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(text), settings);
            bool nobr = false;
            bool alignmentSet = false;

            // TO DO: Throw an exception if an <a> tag tries to embed another <a> tag.
            // Should also apply for many of the other tags too.

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "output":
                                if (reader.GetAttribute("nobr") == "true")
                                {
                                    nobr = true;
                                }
                                break;
                            case "object":
                                generatingLink = true;
                                currentVerbs = reader.GetAttribute("verbs");
                                currentLinkColour = reader.GetAttribute("color");
                                break;
                            case "command":
                                generatingLink = true;
                                currentCommand = reader.GetAttribute("input");
                                currentLinkColour = reader.GetAttribute("color");
                                break;
                            case "br":
                                WriteText(FormatText("<br />"));
                                break;
                            case "b":
                                WriteText("<b>");
                                break;
                            case "i":
                                WriteText("<i>");
                                break;
                            case "u":
                                WriteText("<u>");
                                break;
                            case "color":
                                m_foregroundOverride = reader.GetAttribute("color");
                                break;
                            case "font":
                                m_fontSizeOverride = reader.GetAttribute("size");
                                break;
                            case "align":
                                m_playerUI.SetAlignment(reader.GetAttribute("align"));
                                break;
                            case "a":
                                generatingLink = true;
                                currentHref = reader.GetAttribute("href");
                                currentLinkColour = reader.GetAttribute("color");
                                break;
                            default:
                                throw new Exception(String.Format("Unrecognised element '{0}'", reader.Name));
                        }
                        break;
                    case XmlNodeType.Text:
                        if (!generatingLink)
                        {
                            WriteText(FormatText(reader.Value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")));
                        }
                        else
                        {
                            currentTagValue = reader.Value;
                        }
                        break;
                    case XmlNodeType.Whitespace:
                        WriteText(FormatText(reader.Value.Replace(" ", "&nbsp;")));
                        break;
                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "output":
                                // do nothing
                                break;
                            case "object":
                                AddLink(currentTagValue, null, currentVerbs, currentLinkColour);
                                generatingLink = false;
                                break;
                            case "command":
                                AddLink(currentTagValue, currentCommand, null, currentLinkColour);
                                generatingLink = false;
                                break;
                            case "b":
                                WriteText("</b>");
                                break;
                            case "i":
                                WriteText("</i>");
                                break;
                            case "u":
                                WriteText("</u>");
                                break;
                            case "color":
                                m_foregroundOverride = "";
                                break;
                            case "font":
                                m_fontSizeOverride = "";
                                break;
                            case "align":
                                m_playerUI.SetAlignment("");
                                alignmentSet = true;
                                break;
                            case "a":
                                AddExternalLink(currentTagValue, currentHref, currentLinkColour);
                                generatingLink = false;
                                break;
                            default:
                                throw new Exception(String.Format("Unrecognised element '{0}'", reader.Name));
                        }
                        break;
                }
            }

            if (!nobr)
            {
                // If we have just set the text alignment but then print no more text afterwards,
                // there's no need to submit an extra <br> tag as subsequent text will be in a
                // brand new <div> element.

                if (!(alignmentSet && m_textBuffer.Length == 0))
                {
                    WriteText("<br />");
                }
            }

        }

        private string FormatText(string text)
        {
            if (text.Length == 0) return text;

            // if you add an element whose content starts with a space, that
            // space will be ignored. By replacing an initial space with &nbps; we prevent that.
            if (text.Substring(0, 1) == " ")
            {
                text = "&nbsp;" + ((text.Length > 1) ? text.Substring(1) : string.Empty);
            }

            string style = GetCurrentFormat();

            if (style.Length > 0)
            {
                text = string.Format("<span style=\"{0}\">{1}</span>", style, text);
            }

            return text;
        }

        private string GetCurrentFormat()
        {
            return GetCurrentFormat(null);
        }

        private string GetCurrentFormat(string linkForeground)
        {
            string style = "";
            if (UseGameFont)
            {
                if (m_font.Length > 0)
                {
                    style += string.Format("font-family:{0};", m_font);
                }
            }
            else
            {
                style += string.Format("font-family:{0};", PlayerOverrideFontFamily);
            }

            string colour;
            if (UseGameColours)
            {
                if (!string.IsNullOrEmpty(linkForeground))
                {
                    colour = linkForeground;
                }
                else
                {
                    colour = m_foregroundOverride;
                    if (colour.Length == 0) colour = m_foreground;
                }
            }
            else
            {
                colour = PlayerOverrideForeground;
            }
            if (colour.Length > 0)
            {
                style += string.Format("color:{0};", colour);
            }

            string fontSize;
            if (UseGameFont)
            {
                fontSize = m_fontSizeOverride;
                if (fontSize.Length == 0) fontSize = m_fontSize;
            }
            else
            {
                fontSize = PlayerOverrideFontSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (fontSize.Length > 0)
            {
                style += string.Format("font-size:{0}pt;", fontSize);
            }

            return style;
        }

        private int m_linkCount = 0;

        private void AddLink(string text, string command, string verbs, string colour)
        {
            string onclick = string.Empty;
            m_linkCount++;
            string linkid = "verbLink" + m_linkCount.ToString();

            if (string.IsNullOrEmpty(verbs))
            {
                onclick = string.Format(" onclick=\"sendCommand('{0}')\"", command);
            }

            WriteText(string.Format("<a id=\"{0}\" style=\"{1}\" class=\"cmdlink\"{2}>{3}</a>",
                linkid,
                GetCurrentFormat(colour ?? m_linkForeground),
                onclick,
                text
                ));

            // We need to call the JavaScript that binds the pop-up menu to the link *after* it has been
            // written. So, clear the text buffer, then add the binding.
            if (!string.IsNullOrEmpty(verbs))
            {
                m_playerUI.OutputText(ClearBuffer());
                m_playerUI.BindMenu(linkid, verbs, text);
            }
        }

        private void AddExternalLink(string text, string href, string colour)
        {
            WriteText(string.Format("<a style=\"{0}\" class=\"cmdlink\" onclick=\"goUrl('{1}')\">{2}</a>",
                GetCurrentFormat(colour ?? m_linkForeground),
                href,
                text));
        }

        private void WriteText(string text)
        {
            m_textBuffer += text;
        }

        public string ClearBuffer()
        {
            string output = m_textBuffer;
            m_textBuffer = "";
            return output;
        }

        public IASL Game
        {
            get { return m_game; }
        }

        public IASLTimer GameTimer
        {
            get { return m_gameTimer; }
        }

        public void SendCommand(string command, int tickCount)
        {
            if (m_gameTimer != null)
            {
                m_gameTimer.SendCommand(command, tickCount);
            }
            else
            {
                m_game.SendCommand(command);
            }
        }

        public void SetForeground(string colour)
        {
            m_foreground = colour;
        }

        public void SetLinkForeground(string colour)
        {
            m_linkForeground = colour;
        }

        public void SetFont(string fontName)
        {
            m_font = fontName;
        }

        public void SetFontSize(string fontSize)
        {
            m_fontSize = fontSize;
        }

        public void AppendText(string text)
        {
            WriteText(FormatText(text) + "<br />");
        }

        public bool UseGameColours { get; set; }
        public bool UseGameFont { get; set; }

        public string PlayerOverrideForeground { get; set; }
        public string PlayerOverrideFontFamily { get; set; }
        public float PlayerOverrideFontSize { get; set; }

        public static string GetUIHTML()
        {
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PlayerController.playercore.htm"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static IJavaScriptParameter ListDataParameter(List<ListData> list)
        {
            var convertedList = new Dictionary<string, string>();
            int count = 1;
            foreach (ListData data in list)
            {
                convertedList.Add(
                    string.Format("k{0}", count),
                    string.Format("{0}:{1}", data.Text, VerbString(data.Verbs))
                );
                count++;
            }
            return new JSONParameter(convertedList);
        }

        public static string VerbString(IEnumerable<string> verbs)
        {
            return string.Join("/", verbs);
        }
    }
}
