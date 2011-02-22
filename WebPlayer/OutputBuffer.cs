using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPlayer
{
    internal static class JavaScriptParameterHelpers
    {
        public static string StringParameter(string param)
        {
            return string.Format("\"{0}\"", param.Replace("\"", "\\\"").Replace(Environment.NewLine, "\\n"));
        }
    }

    internal interface IJavaScriptParameter
    {
        string GetParameter();
    }

    internal class StringParameter : IJavaScriptParameter
    {
        private string m_param;

        public StringParameter(string param)
        {
            m_param = param;
        }

        public string GetParameter()
        {
            return JavaScriptParameterHelpers.StringParameter(m_param);
        }
    }

    internal class JSONParameter : IJavaScriptParameter
    {
        private IDictionary<string, string> m_param;

        public JSONParameter(IDictionary<string, string> param)
        {
            m_param = param;
        }

        public string GetParameter()
        {
            string result = "{";

            foreach (KeyValuePair<string, string> item in m_param)
            {
                if (result.Length > 1)
                {
                    result += ",";
                }

                result += string.Format("{0}:{1}",
                    JavaScriptParameterHelpers.StringParameter(item.Key),
                    JavaScriptParameterHelpers.StringParameter(item.Value));
            }

            result += "}";

            return result;
        }
    }

    internal class BooleanParameter : IJavaScriptParameter
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

    internal class OutputBuffer
    {
        private List<string> m_javaScriptBuffer = new List<string>();

        public void AddJavaScriptToBuffer(string function, params IJavaScriptParameter[] parameters)
        {
            m_javaScriptBuffer.Add(string.Format("{0}({1})", function, string.Join(",", parameters.Select(p => p.GetParameter()))));
        }

        public List<string> ClearJavaScriptBuffer()
        {
            List<string> output = new List<string>(m_javaScriptBuffer);
            m_javaScriptBuffer.Clear();
            return output;
        }

        public string StringParameter(string parameter)
        {
            return string.Format("\"{0}\"", parameter.Replace("\"", "\\\""));
        }

        public void OutputText(string text)
        {
            if (text.Length > 0)
            {
                AddJavaScriptToBuffer("addText", new StringParameter(text));
            }
        }

        public int InitStage { get; set; }
    }
}