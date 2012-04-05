using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class GameQuery
    {
        private string m_filename;
        private PlayerHelper m_helper;
        private GameQueryUI m_dummyUI = new GameQueryUI();
        private List<string> m_errors = new List<string>();

        public GameQuery(string filename)
        {
            m_filename = filename;
        }

        public bool Initialise()
        {
            IASL game = GameLauncher.GetGame(m_filename, null);
            m_helper = new PlayerHelper(game, m_dummyUI);

            try
            {
                if (!m_helper.Initialise(m_dummyUI, out m_errors))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                m_errors.Add(ex.Message);
                return false;
            }

            return true;
        }

        public string GameName
        {
            get { return m_dummyUI.GameName; }
        }

        private class GameQueryUI : IPlayerHelperUI
        {
            public string GameName { get; private set; }

            public void OutputText(string text)
            {
            }

            public void SetAlignment(string alignment)
            {
            }

            public void BindMenu(string linkid, string verbs, string text)
            {
            }

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
            }

            public string GetNewGameFile(string originalFilename, string extensions)
            {
                throw new NotImplementedException();
            }

            public void PlaySound(string filename, bool synchronous, bool looped)
            {
            }

            public void StopSound()
            {
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
                GameName = name;
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
            }

            public void SetFont(string fontName)
            {
            }

            public void SetFontSize(string fontSize)
            {
            }

            public void Speak(string text)
            {
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

            public void SetInterfaceString(string name, string text)
            {
            }

            public void SetPanelContents(string html)
            {
            }
        }
    }
}
