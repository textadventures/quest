using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class EditableCommandPattern : IEditableCommandPattern
    {
        private Element m_parent;
        private string m_attribute;
        private EditorCommandPattern m_pattern;
        private EditorController m_controller;

        public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated { add { } remove { } }

        public EditableCommandPattern(EditorController controller, EditorCommandPattern pattern, Element parent, string attribute)
        {
            m_pattern = pattern;
            m_controller = controller;
            m_parent = parent;
            m_attribute = attribute;
        }

        public object GetUnderlyingValue()
        {
            return m_pattern;
        }

        public string DisplayString()
        {
            return m_pattern.Pattern;
        }

        public string Pattern
        {
            get
            {
                return m_pattern.Pattern;
            }
            set
            {
                m_parent.Fields.Set(m_attribute, new EditorCommandPattern(value));
            }
        }
    }
}
