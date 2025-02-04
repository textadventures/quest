using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    class DelegateImplementation
    {
        private Element m_delegateDef;
        private Element m_implementation;
        private string m_typeName;

        public DelegateImplementation(string typeName, Element def, Element impl)
        {
            m_delegateDef = def;
            m_implementation = impl;
            m_typeName = typeName;
        }

        public Element Definition
        {
            get { return m_delegateDef; }
        }

        public Element Implementation
        {
            get { return m_implementation; }
        }

        public string TypeName
        {
            get { return m_typeName; }
        }

        public override string ToString()
        {
            var script = m_implementation.Fields[FieldDefinitions.Script];
            return script == null ? string.Empty : script.ToString();
        }
    }
}
