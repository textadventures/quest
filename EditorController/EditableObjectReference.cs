using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class EditableObjectReference : IEditableObjectReference
    {
        private Element m_object;

        public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated;

        public EditableObjectReference(Element obj)
        {
            m_object = obj;
        }

        public object GetUnderlyingValue()
        {
            return m_object;
        }

        public string DisplayString()
        {
            return "Object: " + m_object.Name;
        }
    }
}
