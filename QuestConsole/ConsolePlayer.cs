using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;
using System.Text.RegularExpressions;

namespace QuestConsole
{
    class ConsolePlayer : IPlayerHelperUI
    {
        public event Action<string> Output;

        public void ShowMenu(MenuData menuData)
        {
            throw new NotImplementedException();
        }

        public void DoWait()
        {
            throw new NotImplementedException();
        }

        public void DoPause(int ms)
        {
            throw new NotImplementedException();
        }

        public void ShowQuestion(string caption)
        {
            throw new NotImplementedException();
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
        }

        public string GetURL(string file)
        {
            throw new NotImplementedException();
        }

        public void LocationUpdated(string location)
        {
        }

        public void UpdateGameName(string name)
        {
        }

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
        }

        public void SetBackground(string colour)
        {
        }

        public void SetForeground(string colour)
        {
        }

        public void SetLinkForeground(string colour)
        {
        }

        public void RunScript(string script)
        {
        }

        public void Quit()
        {
            throw new NotImplementedException();
        }

        public void SetFont(string fontName)
        {
        }

        public void SetFontSize(string fontSize)
        {
        }

        public void Speak(string text)
        {
            throw new NotImplementedException();
        }

        public void RequestSave()
        {
            throw new NotImplementedException();
        }

        public void RequestLoad()
        {
            throw new NotImplementedException();
        }

        public void RequestRestart()
        {
            throw new NotImplementedException();
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

        private static Regex s_regexNewLine = new Regex(@"\<br\s*/\>");
        private static Regex s_regexHtml = new Regex(@"\<.+?\>");

        public void OutputText(string text)
        {
            text = s_regexNewLine.Replace(text, Environment.NewLine);
            text = s_regexHtml.Replace(text, "");
            text = text.Replace("&nbsp;", " ");
            Output(text);
        }

        public void SetAlignment(string alignment)
        {
        }

        public void BindMenu(string linkid, string verbs, string text)
        {
        }
    }
}
