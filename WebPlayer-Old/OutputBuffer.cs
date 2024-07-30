using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using TextAdventures.Utility.JSInterop;

namespace WebPlayer
{
    internal class OutputBuffer
    {
        private readonly string m_gameId;
        private readonly List<string> m_javaScriptBuffer = new List<string>();
        private readonly StringBuilder m_outputLoggerText = new StringBuilder();
        private readonly AzureOutputLogger m_outputLogger = new AzureOutputLogger();
        private readonly bool m_useOutputLogger;

        public OutputBuffer(string gameId)
        {
            m_gameId = gameId;
            m_useOutputLogger = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["AzureLogSessionsBlob"]);
        }

        public void AddJavaScriptToBuffer(string function, params IJavaScriptParameter[] parameters)
        {
            lock (m_javaScriptBuffer)
            {
                m_javaScriptBuffer.Add(string.Format("{0}({1})", function, string.Join(",", parameters.Select(p => p.GetParameter()))));

                if (m_useOutputLogger && function.StartsWith("addText"))
                {
                    string text = parameters.First().GetParameter();
                    text = text.Substring(1, text.Length - 2);
                    text = text.Replace(@"\", "");
                    m_outputLoggerText.Append(text);
                }
            }
        }

        public List<string> ClearJavaScriptBuffer()
        {
            List<string> output;
            lock (m_javaScriptBuffer)
            {
                output = new List<string>(m_javaScriptBuffer);
                m_javaScriptBuffer.Clear();

                if (m_useOutputLogger)
                {
                    string text = m_outputLoggerText.ToString();
                    if (text.Length > 0)
                    {
                        m_outputLoggerText.Clear();
                        m_outputLogger.AddText(text);
                    }
                }
            }
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
                AddJavaScriptToBuffer("addTextAndScroll", new StringParameter(text));
            }
        }

        public int InitStage { get; set; }

        public void InitOutputLogger(string userId, string sessionId)
        {
            m_outputLogger.UserId = userId;
            m_outputLogger.SessionId = sessionId;
            m_outputLogger.BlobId = m_gameId;
        }

        public void Finish()
        {
            m_outputLogger.WriteLog();
        }
    }
}