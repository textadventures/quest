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
        private readonly string m_filename;
        private string m_buffer = "";
        private string m_font = "";
        private string m_fontSize = "";
        private string m_foregroundOverride = "";
        private string m_fontSizeOverride = "";
        private InterfaceListHandler m_listHandler;

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

        public PlayerHandler(string filename, OutputBuffer buffer)
        {
            m_filename = filename;
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

            // TO DO: Handle these request types:
            //Quit,
            //Load,
            //Save,
            //Background,
            //Foreground,
            //LinkForeground,
            //RunScript,
            //SetStatus,
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
                case Request.ShowPicture:
                    ShowPicture(data);
                    break;
                case Request.PanesVisible:
                    SetPanesVisible(data == "on");
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

        private void ShowPicture(string filename)
        {
            string url = AddResource(filename);
            WriteText(string.Format("<img src=\"{0}\" onload=\"scrollToEnd();\" />", url));
        }

        public void SendCommand(string command)
        {
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
            Logging.Log.DebugFormat("{0} Menu response received", GameId);
            m_game.SetMenuResponse(response);
        }

        public void CancelMenu()
        {
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
            m_game.Tick();
        }

        public void EndGame()
        {
            Logging.Log.InfoFormat("{0} Ending game", GameId);
            m_game.Finish();
        }
    }
}