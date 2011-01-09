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
        private string m_buffer = "";

        public PlayerHandler(string filename)
        {
            m_game = new WorldModel(filename);
            m_game.PrintText += new PrintTextHandler(m_game_PrintText);
            m_game.Initialise(this);
            m_game.Begin();
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
                            //ForegroundOverride = reader.GetAttribute("color")
                            case "font":
                            //FontSizeOverride = CInt(reader.GetAttribute("size"))
                            case "align":
                            //SetAlignment(reader.GetAttribute("align"))
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
                                //ForegroundOverride = ""
                                break;
                            case "font":
                                //FontSizeOverride = 0
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
            m_buffer += text;
        }

        private void AddLink(string text, string command)
        {
            WriteText(string.Format("<a class=\"cmdlink\" onclick=\"enterCommand('{0}')\">{1}</a>", command, text));
        }

        public void SendCommand(string command)
        {
            m_game.SendCommand(command);
        }

        public string ShowMenu(MenuData menuData)
        {
            throw new NotImplementedException();
        }

        public void DoWait()
        {
            throw new NotImplementedException();
        }

        public bool ShowMsgBox(string caption)
        {
            throw new NotImplementedException();
        }

        public void DoInvoke(Delegate method)
        {
            throw new NotImplementedException();
        }

        public void SetWindowMenu(MenuData menuData)
        {
            throw new NotImplementedException();
        }

        public string GetNewGameFile(string originalFilename, string extensions)
        {
            throw new NotImplementedException();
        }

        public void PlaySound(string filename, bool synchronous, bool looped)
        {
            throw new NotImplementedException();
        }

        public void StopSound()
        {
            throw new NotImplementedException();
        }
    }
}