using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPlayer
{
    public class OutputBuffer
    {
        private List<string> m_javaScriptBuffer = new List<string>();

        public void AddJavaScriptToBuffer(string function, params string[] parameters)
        {
            m_javaScriptBuffer.Add(string.Format("{0}({1})", function, string.Join(",", parameters)));
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
            AddJavaScriptToBuffer("addText", StringParameter(text));
        }
    }
}