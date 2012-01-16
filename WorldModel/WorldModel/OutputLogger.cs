using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    internal class OutputLogger
    {
        private WorldModel m_worldModel;
        private StringBuilder m_text = new StringBuilder();

        public OutputLogger(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public void AddText(string text)
        {
            if (m_text.Length > 0)
            {
                m_text.Append("<br/>" + Environment.NewLine + text);
            }
            else
            {
                m_text.Append(text);
            }

        }

        public void Clear()
        {
            m_text.Clear();
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
    }
}
