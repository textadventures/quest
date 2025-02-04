using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    internal class ExpressionTemplateEditorData : IEditorData
    {
        private IDictionary<string, string> m_parameters;
        private string m_originalPattern;
        private IEditorData m_parentData;

        public event EventHandler Changed;

        public ExpressionTemplateEditorData(string expression, EditorDefinition definition, IEditorData parentData)
        {
            // We get passed in an expression like "Got(myobject)"
            // The definition has pattern like "Got(#object#)" (as a regex)
            // We create the parameter dictionary in the same way as the command parser, so
            // we end up with a dictionary like "object=myobject".

            m_parameters = Utility.Populate(definition.Pattern, expression);
            m_originalPattern = definition.OriginalPattern;
            m_parentData = parentData;
        }

        public string SaveExpression(string changedAttribute, string changedValue)
        {
            // Take the original pattern (e.g. "Got(#myobject#)") and replace the parameter
            // names with their values. If changedAttribute and changedValue are set, then
            // we're in the middle of editing, so use the specified change in place of the
            // currently saved values.
            string result = m_originalPattern;
            foreach (var parameter in m_parameters)
            {
                string value = parameter.Key == changedAttribute ? changedValue : parameter.Value;
                result = result.Replace(string.Format("#{0}#", parameter.Key), value);
            }

            return result;
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public object GetAttribute(string attribute)
        {
            return m_parameters[attribute];
        }

        public ValidationResult SetAttribute(string attribute, object value)
        {
            m_parameters[attribute] = (string)value;
            if (Changed != null) Changed(this, new EventArgs());

            return new ValidationResult { Valid = true };
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
            return m_parentData.GetVariablesInScope();
        }

        public bool IsDirectlySaveable { get { return false; } }
    }
}
