using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class EditableObjectReference : IEditableObjectReference
    {
        private Element m_parent;
        private string m_attribute;
        private Element m_object;
        private EditorController m_controller;

        public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated { add { } remove { } }

        public EditableObjectReference(EditorController controller, Element obj, Element parent, string attribute)
        {
            m_object = obj;
            m_controller = controller;
            m_parent = parent;
            m_attribute = attribute;
        }

        public object GetUnderlyingValue()
        {
            return m_object;
        }

        public string DisplayString()
        {
            return "Object: " + m_object.Name;
        }

        public string Reference
        {
            get
            {
                return m_object.Name;
            }
            set
            {
                m_object = string.IsNullOrEmpty(value) ? null : m_controller.WorldModel.Elements.Get(value);
                m_parent.Fields.Set(m_attribute, m_object);
            }
        }
    }
}
