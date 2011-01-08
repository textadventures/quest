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
            string currentTag = "";
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
                                currentTag = "object";
                                break;
                            case "exit":
                                currentTag = "exit";
                                break;
                            case "br":
                                WriteText("<br />");
                                break;
                            case "b":
                            //Bold = True
                            case "i":
                            //Italic = True
                            case "u":
                            //Underline = True
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
                        if (currentTag.Length == 0)
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
                                currentTag = "";
                                //AddLink(currentTagValue, HyperlinkType.ObjectLink)
                                WriteText(currentTagValue);
                                break;
                            case "exit":
                                currentTag = "";
                                //AddLink(currentTagValue, HyperlinkType.ExitLink)
                                WriteText(currentTagValue);
                                break;
                            case "b":
                                //Bold = False
                                break;
                            case "i":
                                //Italic = False
                                break;
                            case "u":
                                //Underline = False
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