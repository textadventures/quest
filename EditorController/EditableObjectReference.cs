using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class EditableObjectReference : IEditableObjectReference
    {
        private Element m_object;
        private EditorController m_controller;

        public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated;

        public EditableObjectReference(EditorController controller, Element obj)
        {
            m_object = obj;
            m_controller = controller;
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
                m_object = m_controller.WorldModel.Elements.Get(value);
            }
        }
    }
}
