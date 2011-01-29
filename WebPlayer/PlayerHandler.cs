using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;
using System.Xml;

namespace WebPlayer
{
    public class PlayerHandler : IPlayer
    {
        private IASL m_game;
        private readonly string m_filename;
        private string m_buffer = "";
        private string m_font = "";
        private string m_fontSize = "";
        private string m_foregroundOverride = "";
        private string m_fontSizeOverride = "";

        public event Action<string> LocationUpdated;
        public event Action BeginWait;
        public event Action<string> GameNameUpdated;
        public event Action ClearScreen;

        private static readonly log4net.ILog s_log = log4net.LogManager.GetLogger(typeof(PlayerHandler));

        static PlayerHandler()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public PlayerHandler(string filename)
        {
            m_filename = filename;
        }

        public string GameId { get; set; }

        public string LibraryFolder { get; set; }

        public bool Initialise(out List<string> errors)
        {
            s_log.InfoFormat("{0} Initialising {1}", GameId, m_filename);
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
            m_game.Initialise(this);
            if (m_game.Errors.Count > 0)
            {
                s_log.InfoFormat("{0} Failed to initialise game - errors found in file", GameId);
                errors = m_game.Errors;
                return false;
            }
            else
            {
                s_log.InfoFormat("{0} Initialised successfully", GameId);
                errors = null;
                return true;
            }
        }

        void m_game_LogError(string errorMessage)
        {
            WriteText("[Sorry, an error occurred]");
            s_log.Error(errorMessage);
        }

        public void BeginGame()
        {
            s_log.InfoFormat("{0} Beginning game", GameId);
            m_game.Begin();
        }

        void m_game_RequestRaised(Request request, string data)
        {
            s_log.DebugFormat("{0} Request raised: {1}, {2}", GameId, request, data);

            // TO DO: Handle these request types:
            //Quit,
            //Load,
            //Save,
            //Background,
            //Foreground,
            //LinkForeground,
            //RunScript,
            //SetStatus,
            //PanesVisible,
            //ShowPicture,
            //Speak,
            //Restart

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
                    ClearScreen();
                    break;
                default:
                    WriteText(string.Format("Unhandled request: {0}, {1}", request, data));
                    break;
            }
        }

        void m_game_PrintText(string text)
        {
            ProcessOutput(text);
        }

        public string ClearBuffer()
        {
            string output = m_buffer;
            m_buffer = "";
            return output;
        }

        private void ProcessOutput(string text)
        {
            string currentCommand = "";
            string currentTagValue = "";
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = false;
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(text), settings);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "output":
                                // do nothing
                                break;
                            case "object":
                                currentCommand = "look at";
                                break;
                            case "exit":
                                currentCommand = "go";
                                break;
                            case "br":
                                WriteText("<br />");
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
                                //SetAlignment(reader.GetAttribute("align"))
                                break;
                            default:
                                throw new Exception(String.Format("Unrecognised element '{0}'", reader.Name));
                        }
                        break;
                    case XmlNodeType.Text:
                        if (currentCommand.Length == 0)
                        {
                            WriteText(reader.Value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"));
                        }
                        else
                        {
                            currentTagValue = reader.Value;
                        }
                        break;
                    case XmlNodeType.Whitespace:
                        WriteText(reader.Value.Replace(" ", "&nbsp;"));
                        break;
                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "output":
                                // do nothing
                                break;
                            case "object":
                                AddLink(currentTagValue, currentCommand + " " + currentTagValue);
                                currentCommand = "";
                                break;
                            case "exit":
                                AddLink(currentTagValue, currentCommand + " " + currentTagValue);
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
                                //SetAlignment("")
                                break;
                            default:
                                throw new Exception(String.Format("Unrecognised element '{0}'", reader.Name));
                        }
                        break;
                }

            }

            WriteText("<br />");
        }

        private void WriteText(string text)
        {
            string style = "";
            if (m_font.Length > 0)
            {
                style += string.Format("font-family:{0};", m_font);
            }
            if (m_foregroundOverride.Length > 0)
            {
                style += string.Format("color:{0};", m_foregroundOverride);
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

            m_buffer += text;
        }

        private void AddLink(string text, string command)
        {
            WriteText(string.Format("<a class=\"cmdlink\" onclick=\"enterCommand('{0}')\">{1}</a>", command, text));
        }

        public void SendCommand(string command)
        {
            s_log.DebugFormat("{0} Command entered: {1}", GameId, command);
            m_game.SendCommand(command);
        }

        public Action<string, IDictionary<string, string>, bool> ShowMenuDelegate { get; set; }

        public void ShowMenu(MenuData menuData)
        {
            s_log.DebugFormat("{0} Showing menu", GameId);
            ShowMenuDelegate(menuData.Caption, menuData.Options, menuData.AllowCancel);
        }

        public void SetMenuResponse(string response)
        {
            s_log.DebugFormat("{0} Menu response received", GameId);
            m_game.SetMenuResponse(response);
        }

        public void CancelMenu()
        {
            s_log.DebugFormat("{0} Menu cancelled", GameId);
            m_game.SetMenuResponse(null);
        }

        public void DoWait()
        {
            s_log.DebugFormat("{0} Wait beginning", GameId);
            BeginWait();
        }

        public void EndWait()
        {
            s_log.DebugFormat("{0} Wait ending", GameId);
            m_game.FinishWait();
        }

        public bool ShowMsgBox(string caption)
        {
            s_log.DebugFormat("{0} Showing message box", GameId);
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void StopSound()
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            m_game.Tick();
        }

        public void EndGame()
        {
            s_log.InfoFormat("{0} Ending game", GameId);
            m_game.Finish();
        }
    }
}