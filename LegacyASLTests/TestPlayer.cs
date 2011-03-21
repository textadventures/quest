using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;

namespace LegacyASLTests
{
    class TestPlayer : IPlayer
    {
        private List<string> m_output = new List<string>();

        public void ClearBuffer()
        {
            m_output.Clear();
        }

        public string Buffer(int index)
        {
            return m_output[index];
        }

        public int BufferLength
        {
            get { return m_output.Count; }
        }

        public void PrintText(string text)
        {
            if (!text.StartsWith("<output>") || !text.EndsWith("</output>"))
            {
                throw new ArgumentException("Invalid output format");
            }
            // remove <output> and </output>
            text = text.Substring(8, text.Length - 17);
            m_output.Add(text);
        }

        public void RequestRaised(Request request, string data)
        {
        }

        public void ShowMenu(MenuData menuData)
        {
            LatestMenu = menuData;
        }

        public MenuData LatestMenu { get; set; }

        public void DoWait()
        {
            IsWaiting = true;
        }

        public bool IsWaiting { get; set; }

        public string QuestionData { get; set; }

        public void ShowQuestion(string caption)
        {
            QuestionData = caption;
        }

        public void SetWindowMenu(MenuData menuData)
        {
            throw new NotImplementedException();
        }

        public string GetNewGameFile(string originalFilename, string extensions)
        {
            throw new NotImplementedException();
        }

        public void PlaySound(string filename, bool synchronous, bool looped)
        {
            throw new NotImplementedException();
        }

        public void StopSound()
        {
            throw new NotImplementedException();
        }

        public void WriteHTML(string html)
        {
            throw new NotImplementedException();
        }

        public void LocationUpdated(string location)
        {
            throw new NotImplementedException();
        }

        public void UpdateGameName(string name)
        {
            throw new NotImplementedException();
        }

        public void ClearScreen()
        {
            throw new NotImplementedException();
        }
    }
}
