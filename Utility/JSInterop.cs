using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TextAdventures.Utility.JSInterop
{
    public static class JavaScriptParameterHelpers
    {
        public static string StringParameter(string param)
        {
            return string.Format("\"{0}\"", param.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace(Environment.NewLine, "\\n"));
        }
    }

    public interface IJavaScriptParameter
    {
        string GetParameter();
    }

    public class StringParameter : IJavaScriptParameter
    {
        private string m_param;

        public StringParameter(string param)
        {
            if (param == null) param = string.Empty;
            m_param = param.Replace("\r", "").Replace("\n", "");
        }

        public string GetParameter()
        {
            return JavaScriptParameterHelpers.StringParameter(m_param);
        }
    }

    public class NullParameter : IJavaScriptParameter
    {
        public string GetParameter()
        {
            return "null";
        }
    }

    public class DictionaryParameter : IJavaScriptParameter
    {
        private IDictionary<string, string> m_param;

        public DictionaryParameter(IDictionary<string, string> param)
        {
            m_param = param;
        }

        public string GetParameter()
        {
            // Copy dictionary to work around an InvalidCastException when serializing
            Dictionary<string, string> dictionary = new Dictionary<string, string>(m_param);
            return Newtonsoft.Json.JsonConvert.SerializeObject(dictionary);
        }
    }

    public class JSONParameter : IJavaScriptParameter
    {
        private object m_param;

        public JSONParameter(object param)
        {
            m_param = param;
        }

        public string GetParameter()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(m_param);
        }
    }

    public class BooleanParameter : IJavaScriptParameter
    {
        private bool m_param;

        public BooleanParameter(bool param)
        {
            m_param = param;
        }

        public string GetParameter()
        {
            return m_param ? "true" : "false";
        }
    }

    public class IntParameter : IJavaScriptParameter
    {
        private int m_param;

        public IntParameter(int param)
        {
            m_param = param;
        }

        public string GetParameter()
        {
            return m_param.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class DoubleParameter : IJavaScriptParameter
    {
        private double m_param;

        public DoubleParameter(double param)
        {
            m_param = param;
        }

        public string GetParameter()
        {
            return m_param.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class StringArrayParameter : IJavaScriptParameter
    {
        private IEnumerable<string> m_param;

        public StringArrayParameter(IEnumerable<string> param)
        {
            m_param = param;
        }

        public string GetParameter()
        {
            string result = string.Empty;
            foreach (string param in m_param)
            {
                if (result.Length > 0) result += ", ";
                result += JavaScriptParameterHelpers.StringParameter(param);
            }
            return "[" + result + "]";
        }
    }
}
