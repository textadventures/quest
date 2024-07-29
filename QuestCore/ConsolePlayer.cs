using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest;
using System.Text.RegularExpressions;
using System.Threading;

namespace QuestCore
{
    class ConsolePlayer : IPlayerHelperUI
    {
        private IASL m_game;
        private IASLTimer m_gameTimer;

        private int m_elapsedTime = 0;
        private int m_nextTick = 0;
        private System.Timers.Timer m_timer;

        public event Action ClearBuffer;
        public event Action Finish;

        public ConsolePlayer(IASL game)
        {
            m_game = game;

            m_gameTimer = game as IASLTimer;
            if (m_gameTimer != null)
            {
                m_timer = new System.Timers.Timer(1000);
                m_timer.Elapsed += timer_Elapsed;
                m_timer.AutoReset = true;
                m_gameTimer.RequestNextTimerTick += game_RequestNextTimerTick;
            }
        }

        public void ShowMenu(MenuData menuData)
        {
            ClearBuffer();
            OutputText(menuData.Caption + "<br/>");
            foreach (var option in menuData.Options)
            {
                OutputText(string.Format("{0}: {1}<br/>", option.Key, option.Value));
            }
            string response;
            do
            {
                response = Console.ReadLine();
            } while (!menuData.Options.ContainsKey(response));

            Thread newThread = new Thread(() =>
            {
                m_game.SetMenuResponse(response);
                ClearBuffer();
            });
            newThread.Start();
        }

        public void DoWait()
        {
            ClearBuffer();
            Console.ReadKey();

            Thread newThread = new Thread(() =>
            {
                m_game.FinishWait();
                ClearBuffer();
            });
            newThread.Start();
        }

        public void DoPause(int ms)
        {
            ClearBuffer();
            System.Timers.Timer timer = new System.Timers.Timer(ms);
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                m_game.FinishPause();
                ClearBuffer();
            };
            timer.Start();
        }

        public void ShowQuestion(string caption)
        {
            ClearBuffer();
            OutputText(caption + "<br/>Y/N:");
            string response;
            do
            {
                response = Console.ReadKey().KeyChar.ToString().ToUpper();
            } while (response != "Y" && response != "N");
            Console.WriteLine();

            Thread newThread = new Thread(() =>
            {
                m_game.SetQuestionResponse(response == "Y");
                ClearBuffer();
            });
            newThread.Start();
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
        }

        public string GetURL(string file)
        {
            return file;
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

        public void RunScript(string function, object[] parameters)
        {
        }

        public void Quit()
        {
            Finish();
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

        public void RequestSave(string html)
        {
            OutputText("Saving is not currently supported");
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
            text = text.Replace("&nbsp;", " ").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
            Console.Write(text);
        }

        public void SetAlignment(string alignment)
        {
        }

        public void BindMenu(string linkid, string verbs, string text, string elementId)
        {
        }

        private void game_RequestNextTimerTick(int secs)
        {
            m_elapsedTime = 0;
            m_nextTick = secs;
            if (m_nextTick > 0)
            {
                m_timer.Start();
            }
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            m_elapsedTime += 1;
            if (m_nextTick > 0 && m_elapsedTime >= m_nextTick)
            {
                m_nextTick = 0;
                m_gameTimer.Tick(GetTickCountAndStopTimer());
                ClearBuffer();
            }
        }

        public int GetTickCountAndStopTimer()
        {
            m_timer.Stop();
            return m_elapsedTime;
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

        public string GetUIOption(UIOption option)
        {
            return null;
        }
    }
}
