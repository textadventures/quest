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
        private IASL m_game;
        private LegacyASL.LegacyGame m_v4Game;
        private WorldModel m_v5Game;

        public GameQuery(string filename)
        {
            m_filename = filename;
        }

        public bool Initialise()
        {
            m_game = GameLauncher.GetGame(m_filename, null);
            m_v4Game = m_game as LegacyASL.LegacyGame;
            m_v5Game = m_game as WorldModel;
            m_helper = new PlayerHelper(m_game, m_dummyUI);

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

        public int ASLVersion
        {
            get
            {
                if (m_v4Game != null)
                {
                    return m_v4Game.ASLVersion;
                }
                if (m_v5Game != null)
                {
                    return m_v5Game.ASLVersion;
                }
                throw new InvalidOperationException();
            }
        }

        public string GameID
        {
            get
            {
                if (m_v4Game != null)
                {
                    return null;
                }
                if (m_v5Game != null)
                {
                    return m_v5Game.GameID;
                }
                throw new InvalidOperationException();
            }
        }

        public string Category
        {
            get
            {
                if (m_v4Game != null)
                {
                    return null;
                }
                if (m_v5Game != null)
                {
                    return m_v5Game.Category;
                }
                throw new InvalidOperationException();
            }
        }

        public string Description
        {
            get
            {
                if (m_v4Game != null)
                {
                    return null;
                }
                if (m_v5Game != null)
                {
                    return m_v5Game.Description;
                }
                throw new InvalidOperationException();
            }
        }

        public IEnumerable<string> Errors
        {
            get
            {
                return m_errors.AsReadOnly();
            }
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

            public void BindMenu(string linkid, string verbs, string text, string elementId)
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

            public void Log(string text)
            {
            }
        }
    }
}
