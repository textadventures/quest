using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    internal class ExpressionTemplateEditorData : IEditorData
    {
        private IDictionary<string, string> m_parameters;

        public event EventHandler Changed;

        public ExpressionTemplateEditorData(string expression, EditorDefinition definition)
        {
            // We get passed in an expression like "(Got(myobject))"
            // The definition has pattern like "(Got(#object#))" (as a regex)
            // We create the parameter dictionary in the same way as the command parser, so
            // we end up with a dictionary like "object=myobject".

            m_parameters = Utility.Populate(definition.Pattern, expression);
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public object GetAttribute(string attribute)
        {
            return m_parameters[attribute];
        }

        public void SetAttribute(string attribute, object value)
        {
            m_parameters[attribute] = (string)value;
            // TO DO: raise Changed event
        }

        public IEnumerable<string> GetAffectedRelatedAttributes(string attribute)
        {
            throw new NotImplementedException();
        }

        public string GetSelectedFilter(string filterGroup)
        {
            throw new NotImplementedException();
        }

        public void SetSelectedFilter(string filterGroup, string filter)
        {
            throw new NotImplementedException();
        }

        public bool ReadOnly
        {
            get;
            set;
        }

        public IEnumerable<string> GetVariablesInScope()
        {
            throw new NotImplementedException();
        }
    }
}
