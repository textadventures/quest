using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TextAdventures.Quest
{
    internal interface IOutputLogger
    {
        void RunJavaScript(string function, object[] parameters);
        void Save();
        void Clear();
    }

    internal class OutputLogger : IOutputLogger
    {
        private readonly WorldModel m_worldModel;

        public OutputLogger(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public void RunJavaScript(string function, object[] parameters)
        {
            // Store JS calls as a list of dictionaries with function=name, parameters=QuestList<object> params

            var outputLog = m_worldModel.Game.Fields[FieldDefinitions.OutputLog];

            if (outputLog == null)
            {
                outputLog = new QuestList<object>();
                m_worldModel.Game.Fields[FieldDefinitions.OutputLog] = outputLog;
            }

            var newItem = new QuestDictionary<object>();
            outputLog.Add(newItem);

            newItem.Add("function", function);
            newItem.Add("parameters", new QuestList<object>(parameters));
        }

        public void Save()
        {
            // Save call is only required for LegacyOutputLogger
        }

        public void Clear()
        {
            var outputLog = m_worldModel.Game.Fields[FieldDefinitions.OutputLog];

            if (outputLog != null)
            {
                outputLog.Clear();
            }
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

        public void RunJavaScript(string function, object[] parameters)
        {
            // TO DO: We should be using a proper XML writer here and ensure parameter types are serialized properly

            m_text.Append(string.Format("<output_runjs function=\"{0}\" parameters=\"{1}\" ", function, parameters == null ? 0 : parameters.Length));
            if (parameters != null)
            {
                int count = 0;
                foreach (var parameter in parameters)
                {
                    count++;
                    m_text.Append(string.Format("parameter{0}=\"{1}\"", count, parameter));
                }
            }
            m_text.Append("/>");
        }

        public void Clear()
        {
            m_text.Clear();
            m_anyText = false;
        }

        public void Save()
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
                            case "output_runjs":
                                if (output.Length > 0)
                                {
                                    m_worldModel.Print(output.ToString(), false);
                                    output.Clear();
                                }
                                string function = reader.GetAttribute("function");
                                int parameterCount = int.Parse(reader.GetAttribute("parameters"));
                                List<object> paramValues = new List<object>();
                                for (int i = 1; i <= parameterCount; i++)
                                {
                                    paramValues.Add(reader.GetAttribute("parameter" + i));
                                }
                                m_worldModel.PlayerUI.RunScript(function, paramValues.ToArray());
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
