using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Utility.JSInterop
{
    public static class JavaScriptParameterHelpers
    {
        public static string StringParameter(string param)
        {
            return string.Format("\"{0}\"", param.Replace("\"", "\\\"").Replace(Environment.NewLine, "\\n"));
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
            m_param = param.Replace("\r", "").Replace("\n", "");
        }

        public string GetParameter()
        {
            return JavaScriptParameterHelpers.StringParameter(m_param);
        }
    }

    public class JSONParameter : IJavaScriptParameter
    {
        private IDictionary<string, string> m_param;

        public JSONParameter(IDictionary<string, string> param)
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
            return m_param.ToString();
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
