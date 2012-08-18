using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AxeSoftware.Quest.Scripts
{
    public class NoReturnValue
    {
    }

    public class Context
    {
        private Parameters m_parameters;
        private object m_returnValue = new NoReturnValue();

        public Context()
        {
        }

        public Parameters Parameters
        {
            set { m_parameters = value; }
            get { return m_parameters; }
        }

        public object ReturnValue
        {
            get { return m_returnValue; }
            set { m_returnValue = value; }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class Parameters : Dictionary<string, object>
    {
        public Parameters()
        {
        }

        public Parameters(string key, object value)
        {
            Add(key, value);
        }

        public Parameters(IDictionary parameters)
        {
            foreach (object key in parameters.Keys)
            {
                if (key is string)
                {
                    Add((string)key, parameters[key]);
                }
            }
        }
    }
}
