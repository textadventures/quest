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
        private string m_statusText = string.Empty;
        private string m_location = string.Empty;
        private string m_gameName = string.Empty;
        private string m_background = string.Empty;
        private string m_foreground = string.Empty;
        private string m_fontName = string.Empty;
        private string m_fontSize = string.Empty;

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
        }

        public string GetNewGameFile(string originalFilename, string extensions)
        {
            return string.Empty;
        }

        public void PlaySound(string filename, bool synchronous, bool looped)
        {
        }

        public void StopSound()
        {
        }

        public void WriteHTML(string html)
        {
            throw new NotImplementedException();
        }

        public void LocationUpdated(string location)
        {
            m_location = location;
        }

        public string Location { get { return m_location; } }

        public void UpdateGameName(string name)
        {
            m_gameName = name;
        }

        public string GameName { get { return m_gameName; } }

        public void ClearScreen()
        {
        }

        public void ShowPicture(string filename)
        {
        }

        public void SetPanesVisible(string data)
        {
        }

        public void SetStatusText(string text)
        {
            m_statusText = text;
        }

        public string StatusText { get { return m_statusText; } }

        public void SetBackground(string colour)
        {
            m_background = colour;
        }

        public void SetForeground(string colour)
        {
            m_foreground = colour;
        }

        public string Background { get { return m_background; } }
        public string Foreground { get { return m_foreground; } }

        public void RunScript(string script)
        {
        }

        public void Quit()
        {
        }

        public void SetFont(string fontName)
        {
            m_fontName = fontName;
        }

        public void SetFontSize(string fontSize)
        {
            m_fontSize = fontSize;
        }

        public string FontName { get { return m_fontName; } }
        public string FontSize { get { return m_fontSize; } }

        public void Speak(string text)
        {
        }

        public void RequestSave()
        {
        }

        public void RequestLoad()
        {
        }

        public void RequestRestart()
        {
        }

        public void SetLinkForeground(string colour)
        {
        }

        public void Show(string element)
        {
        }

        public void Hide(string element)
        {
        }

        public void SetCompassDirections(IEnumerable<string> dirs)
        {
        }

        public string GetURL(string file)
        {
            return file;
        }

        public void DoPause(int ms)
        {
        }

        public void SetInterfaceString(string name, string text)
        {
        }
    }
}
