using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;
using System.Xml;

namespace WebPlayer
{
    internal class PlayerHandler : IPlayer
    {
        private IASL m_game;
        private IASLTimer m_gameTimer;
        private readonly string m_filename;
        private string m_textBuffer = "";
        private string m_font = "";
        private string m_fontSize = "";
        private string m_foreground = "";
        private string m_foregroundOverride = "";
        private string m_linkForeground = "";
        private bool m_useForeground = true;
        private string m_fontSizeOverride = "";
        private InterfaceListHandler m_listHandler;
        private OutputBuffer m_buffer;
        private bool m_finished = false;

        public class PlayAudioEventArgs : EventArgs
        {
            public string Filename { get; set; }
            public bool Synchronous { get; set; }
            public bool Looped { get; set; }
        }

        public event Action<string> LocationUpdated;
        public event Action BeginWait;
        public event Action<string> GameNameUpdated;
        public event Action ClearScreen;
        public event Func<string, string> AddResource;
        public event EventHandler<PlayAudioEventArgs> PlayAudio;
        public event Action StopAudio;
        public event Action<bool> SetPanesVisible;
        public event Action<string> SetStatusText;

        public PlayerHandler(string filename, OutputBuffer buffer)
        {
            m_filename = filename;
            m_buffer = buffer;
            m_listHandler = new InterfaceListHandler(buffer);
        }

        public string GameId { get; set; }

        public string LibraryFolder { get; set; }

        public bool Initialise(out List<string> errors)
        {
            Logging.Log.InfoFormat("{0} Initialising {1}", GameId, m_filename);
            switch (System.IO.Path.GetExtension(m_filename).ToLower())
            {
                case ".aslx":
                    m_game = new WorldModel(m_filename, LibraryFolder);
                    break;
                case ".asl":
                case ".cas":
                    m_game = new AxeSoftware.Quest.LegacyASL.LegacyGame(m_filename);
                    break;
                default:
                    errors = new List<string> { "Unrecognised file type" };
                    return false;
            }

            m_game.PrintText += m_game_PrintText;
            m_game.RequestRaised += m_game_RequestRaised;
            m_game.LogError += m_game_LogError;
            m_game.UpdateList += m_game_UpdateList;
            m_game.Finished += m_game_Finished;

            m_gameTimer = m_game as IASLTimer;
            if (m_gameTimer != null)
            {
                m_gameTimer.UpdateTimer += m_gameTimer_UpdateTimer;
            }

            m_game.Initialise(this);
            if (m_game.Errors.Count > 0)
            {
                Logging.Log.InfoFormat("{0} Failed to initialise game - errors found in file", GameId);
                errors = m_game.Errors;
                return false;
            }
            else
            {
                Logging.Log.InfoFormat("{0} Initialised successfully", GameId);
                errors = null;
                return true;
            }
        }

        void m_gameTimer_UpdateTimer(int nextTick)
        {
            throw new NotImplementedException();
        }

        void m_game_Finished()
        {
            Finished = true;
        }

        void m_game_UpdateList(ListType listType, List<ListData> items)
        {
            m_listHandler.UpdateList(listType, items);
        }

        void m_game_LogError(string errorMessage)
        {
            WriteText("[Sorry, an error occurred]");
            Logging.Log.Error(errorMessage);
        }

        public void BeginGame()
        {
            Logging.Log.InfoFormat("{0} Beginning game", GameId);
            m_game.Begin();
        }

        void m_game_RequestRaised(Request request, string data)
        {
            Logging.Log.DebugFormat("{0} Request raised: {1}, {2}", GameId, request, data);

            switch (request)
            {
                case Request.UpdateLocation:
                    LocationUpdated(data);
                    break;
                case Request.FontName:
                    m_font = data;
                    break;
                case Request.FontSize:
                    m_fontSize = data;
                    break;
                case Request.GameName:
                    GameNameUpdated(data);
                    break;
                case Request.ClearScreen:
                    m_buffer.OutputText(ClearBuffer());
                    ClearScreen();
                    break;
                case Request.ShowPicture:
                    ShowPicture(data);
                    break;
                case Request.PanesVisible:
                    SetPanesVisible(data == "on");
                    break;
                case Request.SetStatus:
                    SetStatusText(data);
                    break;
                case Request.Speak:
                    // ignore
                    break;
                case Request.Foreground:
                    m_foreground = data;
                    break;
                case Request.Background:
                    SetBackground(data);
                    break;
                case Request.RunScript:
                    RunScript(data);
                    break;
                case Request.LinkForeground:
                    m_linkForeground = data;
                    break;
                case Request.Load:
                case Request.Save:
                    WriteText("Sorry, loading and saving is not currently supported for online games. <a href=\"http://www.axeuk.com/quest/\">Download Quest</a> for load/save support.");
                    break;
                case Request.Restart:
                    WriteText("Sorry, restarting is not currently supported for online games. Refresh your browser to restart the game.");
                    break;
                case Request.Quit:
                    Finished = true;
                    break;
                default:
                    WriteText(string.Format("Unhandled request: {0}, {1}", request, data));
                    break;
            }
        }

        void SetBackground(string col)
        {
            m_buffer.AddJavaScriptToBuffer("setBackground", new StringParameter(col));
        }

        void m_game_PrintText(string text)
        {
            ProcessOutput(text);
        }

        public string ClearBuffer()
        {
            string output = m_textBuffer;
            m_textBuffer = "";
            return output;
        }

        void RunScript(string data)
        {
            string[] args = data.Split(';');
            var parameters = new List<IJavaScriptParameter>();
            for (int i = 1; i < args.Length; i++)
            {
                parameters.Add(new StringParameter(args[i].Trim()));
            }

            // Clear text buffer before running custom JavaScript, otherwise text written
            // before now may appear after inserted HTML.
            m_buffer.OutputText(ClearBuffer());
            m_buffer.AddJavaScriptToBuffer(args[0], parameters.ToArray());
        }

        private void ProcessOutput(string text)
        {
            string currentCommand = "";
            string currentTagValue = "";
            string currentVerbs = "";
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
                                currentCommand = "look at";
                                currentVerbs = reader.GetAttribute("verbs");
                                break;
                            case "exit":
                                currentCommand = "go";
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
                                SetAlignment(reader.GetAttribute("align"));
                                break;
                            case "a":
                                m_useForeground = false;
                                string target = reader.GetAttribute("target");
                                WriteText(string.Format("<a href=\"{0}\"{1}>",
                                    reader.GetAttribute("href"),
                                    target != null ? "target=\"" + target + "\"" : ""));
                                break;
                            default:
                                throw new Exception(String.Format("Unrecognised element '{0}'", reader.Name));
                        }
                        break;
                    case XmlNodeType.Text:
                        if (currentCommand.Length == 0)
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
                                AddLink(currentTagValue, currentCommand + " " + currentTagValue, currentVerbs);
                                currentCommand = "";
                                break;
                            case "exit":
                                AddLink(currentTagValue, currentCommand + " " + currentTagValue, null);
                                currentCommand = "";
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
                                SetAlignment("");
                                alignmentSet = true;
                                break;
                            case "a":
                                WriteText("</a>");
                                m_useForeground = true;
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

        private void SetAlignment(string align)
        {
            if (align.Length == 0) align = "left";
            m_buffer.OutputText(ClearBuffer());
            m_buffer.AddJavaScriptToBuffer("createNewDiv", new StringParameter(align));
        }

        private string FormatText(string text)
        {
            string style = "";
            if (m_font.Length > 0)
            {
                style += string.Format("font-family:{0};", m_font);
            }

            if (m_useForeground)
            {
                string colour = m_foregroundOverride;
                if (colour.Length == 0) colour = m_foreground;
                if (colour.Length > 0)
                {
                    style += string.Format("color:{0};", colour);
                }
            }

            string fontSize = m_fontSizeOverride;
            if (fontSize.Length == 0) fontSize = m_fontSize;

            if (fontSize.Length > 0)
            {
                style += string.Format("font-size:{0}pt;", fontSize);
            }

            if (style.Length > 0)
            {
                text = string.Format("<span style=\"{0}\">{1}</span>", style, text);
            }

            return text;
        }

        private void WriteText(string text)
        {
            m_textBuffer += text;
        }


        int m_linkCount = 0;

        private void AddLink(string text, string command, string verbs)
        {
            string onclick = string.Empty;
            m_linkCount++;
            string linkid = "verbLink" + m_linkCount.ToString();

            if (string.IsNullOrEmpty(verbs))
            {
                onclick = string.Format(" onclick=\"sendCommand('{0}')\"", command);
            }

            m_useForeground = false;

            // TO DO: I think we should be calling FormatText on the "text" variable below,
            // but currently if we do this we end up with <a...><span>text</span></a> which
            // for some reason breaks jjmenu, meaning we don't see menus when we left-click.
            // The whole area of formatting needs looking at again anyway as it's wasteful to
            // have loads of repeated <span> elements, and this class needs refactoring anyway...

            WriteText(string.Format("<a id=\"{3}\" class=\"cmdlink\"{0}{1}>{2}</a>",
                ((m_linkForeground.Length > 0) ? (" style=color:" + m_linkForeground) + " " : ""),
                onclick,
                text,
                linkid));
            m_useForeground = true;

            // We need to call the JavaScript that binds the pop-up menu to the link *after* it has been
            // written. So, clear the text buffer, then add the binding.
            if (!string.IsNullOrEmpty(verbs))
            {
                m_buffer.OutputText(ClearBuffer());
                m_buffer.AddJavaScriptToBuffer("bindMenu", new StringParameter(linkid), new StringParameter(verbs), new StringParameter(text));
            }
        }

        private void ShowPicture(string filename)
        {
            string url = AddResource(filename);
            WriteText(string.Format("<img src=\"{0}\" onload=\"scrollToEnd();\" /><br />", url));
        }

        public void SendCommand(string command)
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Command entered: {1}", GameId, command);
            m_game.SendCommand(command);
        }

        public Action<string, IDictionary<string, string>, bool> ShowMenuDelegate { get; set; }

        public void ShowMenu(MenuData menuData)
        {
            Logging.Log.DebugFormat("{0} Showing menu", GameId);
            ShowMenuDelegate(menuData.Caption, menuData.Options, menuData.AllowCancel);
        }

        public void SetMenuResponse(string response)
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Menu response received", GameId);
            m_game.SetMenuResponse(response);
        }

        public void CancelMenu()
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Menu cancelled", GameId);
            m_game.SetMenuResponse(null);
        }

        public void DoWait()
        {
            Logging.Log.DebugFormat("{0} Wait beginning", GameId);
            BeginWait();
        }

        public void EndWait()
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Wait ending", GameId);
            m_game.FinishWait();
        }

        public Action<string> ShowQuestionDelegate { get; set; }

        public void ShowQuestion(string caption)
        {
            Logging.Log.DebugFormat("{0} Showing message box", GameId);
            ShowQuestionDelegate(caption);
        }

        public void SetQuestionResponse(string response)
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Question response received", GameId);
            m_game.SetQuestionResponse(response == "yes");
        }

        public void SetWindowMenu(MenuData menuData)
        {
            // Do nothing
        }

        public string GetNewGameFile(string originalFilename, string extensions)
        {
            return string.Empty;
        }

        public void PlaySound(string filename, bool synchronous, bool looped)
        {
            PlayAudio(this, new PlayAudioEventArgs { Filename = filename, Synchronous = synchronous, Looped = looped });
        }

        public void StopSound()
        {
            StopAudio();
        }

        public void Tick()
        {
            if (m_gameTimer != null) m_gameTimer.Tick();
        }

        public void EndGame()
        {
            Logging.Log.InfoFormat("{0} Ending game", GameId);
            m_game.Finish();
        }

        public IEnumerable<string> SetUpExternalScripts()
        {
            // Get all external JS scripts in the game, add them as resources, and then
            // return the list of JS resource URLs.
            var result = new List<string>();
            var scripts = m_game.GetExternalScripts();
            if (scripts == null) return result;
            
            foreach (string script in scripts)
            {
                string url = AddResource(script);
                result.Add(url);
            }

            return result;
        }

        public void WriteHTML(string html)
        {
            m_textBuffer += html;
        }

        public void SendEvent(string eventName, string param)
        {
            if (m_finished) return;
            m_game.SendEvent(eventName, param);
        }

        private bool Finished
        {
            get { return m_finished; }
            set {
                if (value != m_finished)
                {
                    m_buffer.AddJavaScriptToBuffer("gameFinished");
                    m_finished = value;
                }
            }
        }

        public bool UseTimer
        {
            get
            {
                return m_gameTimer != null;
            }
        }
    }
}