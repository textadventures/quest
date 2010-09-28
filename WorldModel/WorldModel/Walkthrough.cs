using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class Walkthrough : IWalkthrough
    {
        private Element m_element;

        public Walkthrough(Element element)
        {
            m_element = element;
        }

        #region IWalkthrough Members

        public List<string> Steps
        {
            get { return new List<string>(m_element.Fields[FieldDefinitions.Steps]); }
        }

        #endregion
    }
}
