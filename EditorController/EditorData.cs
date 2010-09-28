using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    internal class EditorData : IEditorData
    {
        private Element m_element;
        private EditorController m_controller;

        public EditorData(Element element, EditorController controller)
        {
            m_element = element;
            m_controller = controller;
        }

        #region IEditorData Members

        public string Name
        {
            get { return m_element.Name; }
        }

        public object GetAttribute(string attribute)
        {
            return m_controller.WrapValue(m_element.Fields.Get(attribute), m_element, attribute);
        }

        public void SetAttribute(string attribute, object value)
        {
            IDataWrapper wrapper = value as IDataWrapper;
            if (wrapper != null)
            {
                value = wrapper.GetUnderlyingValue();
            }
            m_element.Fields.Set(attribute, value);
        }

        #endregion
    }
}
