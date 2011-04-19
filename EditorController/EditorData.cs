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
        public event EventHandler Changed;

        public EditorData(Element element, EditorController controller)
        {
            m_element = element;
            m_controller = controller;

            element.Fields.AttributeChanged += Fields_AttributeChanged;
            element.Fields.AttributeChangedSilent += Fields_AttributeChanged;
        }

        void Fields_AttributeChanged(object sender, AttributeChangedEventArgs e)
        {
            if (Changed != null) Changed(this, new EventArgs());
        }

        public string Name
        {
            get { return m_element.Name; }
        }

        public object GetAttribute(string attribute)
        {
            return m_controller.WrapValue(m_element.Fields.Get(attribute));
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
    }
}
