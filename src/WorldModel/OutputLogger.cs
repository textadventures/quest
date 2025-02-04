using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TextAdventures.Quest
{
    internal interface IOutputLogger
    {
        void Save(string html);
        void Clear();
    }

    internal class OutputLogger : IOutputLogger
    {
        private readonly WorldModel m_worldModel;

        public OutputLogger(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public void Save(string html)
        {
            var element = m_worldModel.Elements.GetSingle(ElementType.Output) ??
                          m_worldModel.GetElementFactory(ElementType.Output).Create();

            element.Fields.Set("html", html);
        }

        public void Clear()
        {
        }
    }

    internal class LegacyOutputLogger : IOutputLogger
    {
        private WorldModel m_worldModel;
        private StringBuilder m_text = new StringBuilder();
        bool m_anyText = false;

        public LegacyOutputLogger(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public void AddText(string text, bool linebreak = true)
        {
            if (m_anyText)
            {
                m_text.Append((linebreak ? "<br/>" : string.Empty) + Environment.NewLine + text);
            }
            else
            {
                m_text.Append(text);
                m_anyText = true;
            }
        }

        public void AddPicture(string filename)
        {
            m_text.Append(string.Format("<output_picture filename=\"{0}\"/>", filename));
        }

        public void SetFontName(string fontName)
        {
            m_text.Append(string.Format("<output_setfontname name=\"{0}\"/>", fontName));
        }

        public void SetFontSize(string fontSize)
        {
            m_text.Append(string.Format("<output_setfontsize size=\"{0}\"/>", fontSize));
        }

        public void Clear()
        {
            m_text.Clear();
            m_anyText = false;
        }

        public void Save(string html)
        {
            Element element = m_worldModel.Elements.GetSingle(ElementType.Output);
            if (element == null)
            {
                element = m_worldModel.GetElementFactory(ElementType.Output).Create();
            }

            element.Fields.Set("text", m_text.ToString());
        }

        public void DisplayOutput(string text)
        {
            text = "<output>" + text + "</output>";
            StringBuilder output = new StringBuilder();

            var settings = new XmlReaderSettings {IgnoreWhitespace = false};
            var reader = XmlReader.Create(new System.IO.StringReader(text), settings);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "output":
                                break;
                            case "output_picture":
                                if (output.Length > 0)
                                {
                                    m_worldModel.Print(output.ToString());
                                    output.Clear();
                                }
                                m_worldModel.PlayerUI.ShowPicture(m_worldModel.GetExternalPath(reader.GetAttribute("filename")));
                                break;
                            case "output_setfontsize":
                                if (output.Length > 0)
                                {
                                    m_worldModel.Print(output.ToString(), false);
                                    output.Clear();
                                }
                                string size = reader.GetAttribute("size");
                                ((LegacyOutputLogger)(m_worldModel.OutputLogger)).SetFontSize(size);
                                m_worldModel.PlayerUI.SetFontSize(size);
                                break;
                            case "output_setfontname":
                                if (output.Length > 0)
                                {
                                    m_worldModel.Print(output.ToString(), false);
                                    output.Clear();
                                }
                                string name = reader.GetAttribute("name");
                                ((LegacyOutputLogger)(m_worldModel.OutputLogger)).SetFontName(name);
                                m_worldModel.PlayerUI.SetFont(name);
                                break;
                            default:
                                output.Append("<" + reader.Name);
                                if (reader.HasAttributes)
                                {
                                    for (int i = 0; i < reader.AttributeCount; i++)
                                    {
                                        reader.MoveToAttribute(i);
                                        output.Append(string.Format(" {0}=\"{1}\"", reader.Name, reader.Value));
                                    }
                                    reader.MoveToElement();
                                }
                                if (reader.IsEmptyElement)
                                {
                                    output.Append("/>");
                                }
                                else
                                {
                                    output.Append(">");
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                        output.Append(reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "output":
                                break;
                            case "output_picture":
                                break;
                            default:
                                output.Append(string.Format("</{0}>", reader.Name));
                                break;
                        }
                        break;
                }
            }

            m_worldModel.Print(output.ToString());
        }
    }
}
