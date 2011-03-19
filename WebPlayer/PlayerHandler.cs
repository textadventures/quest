using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;
using System.Xml;

namespace WebPlayer
{
    internal class PlayerHandler : IPlayer
    {
        private IASL m_game;
        private IASLTimer m_gameTimer;
        private PlayerController m_controller;
        private readonly string m_filename;
        
        private InterfaceListHandler m_listHandler;
        private OutputBuffer m_buffer;
        private bool m_finished = false;

        public class PlayAudioEventArgs : EventArgs
        {
            public string Filename { get; set; }
            public bool Synchronous { get; set; }
            public bool Looped { get; set; }
        }

        public event Action BeginWait;
        public event Func<string, string> AddResource;
        public event EventHandler<PlayAudioEventArgs> PlayAudio;
        public event Action StopAudio;

        public PlayerHandler(string filename, OutputBuffer buffer)
        {
            m_filename = filename;
            m_buffer = buffer;
            m_listHandler = new InterfaceListHandler(buffer);
        }

        public string GameId { get; set; }

        public string LibraryFolder { get; set; }

        public bool Initialise(out List<string> errors)
        {
            Logging.Log.InfoFormat("{0} Initialising {1}", GameId, m_filename);

            m_controller = new PlayerController(m_filename, LibraryFolder);
            m_game = m_controller.Game;
            m_controller.SetAlignment += SetAlignment;
            m_controller.OutputText += m_buffer.OutputText;
            m_controller.BindMenu += BindMenu;
            m_controller.LocationUpdated += LocationUpdated;
            m_controller.GameNameUpdated += UpdateGameName;
            m_controller.ClearScreen += ClearScreen;
            m_controller.ShowPicture += ShowPicture;
            m_controller.SetPanesVisible += SetPanesVisible;
            m_controller.SetStatusText += SetStatusText;
            m_controller.SetBackground += SetBackground;
            m_controller.RunScript += RunScript;
            m_controller.Finished += m_controller_Finished;

            // TO DO *****************************************
            m_game.LogError += m_game_LogError;
            m_game.UpdateList += m_game_UpdateList;
            m_game.Finished += m_game_Finished;

            m_gameTimer = m_game as IASLTimer;
            if (m_gameTimer != null)
            {
                m_gameTimer.UpdateTimer += m_gameTimer_UpdateTimer;
            }
            //*****************************************

            bool success = m_controller.Initialise(out errors, this);
            if (success)
            {
                Logging.Log.InfoFormat("{0} Initialised successfully", GameId);
            }
            else
            {
                Logging.Log.InfoFormat("{0} Failed to initialise game - errors found in file", GameId);
            }

            return success;
        }

        void m_controller_Finished()
        {
            Finished = true;
        }

        void BindMenu(string linkid, string verbs, string text)
        {
            m_buffer.AddJavaScriptToBuffer("bindMenu", new StringParameter(linkid), new StringParameter(verbs), new StringParameter(text));
        }

        void m_gameTimer_UpdateTimer(int nextTick)
        {
            throw new NotImplementedException();
        }

        void m_game_Finished()
        {
            Finished = true;
        }

        void m_game_UpdateList(ListType listType, List<ListData> items)
        {
            m_listHandler.UpdateList(listType, items);
        }

        void m_game_LogError(string errorMessage)
        {
            m_buffer.OutputText("[Sorry, an error occurred]");
            Logging.Log.Error(errorMessage);
        }

        public void BeginGame()
        {
            Logging.Log.InfoFormat("{0} Beginning game", GameId);
            m_game.Begin();
        }

        public string ClearBuffer()
        {
            return m_controller.ClearBuffer();
        }

        void SetBackground(string col)
        {
            m_buffer.AddJavaScriptToBuffer("setBackground", new StringParameter(col));
        }

        void RunScript(string data)
        {
            string[] args = data.Split(';');
            var parameters = new List<IJavaScriptParameter>();
            for (int i = 1; i < args.Length; i++)
            {
                parameters.Add(new StringParameter(args[i].Trim()));
            }

            // Clear text buffer before running custom JavaScript, otherwise text written
            // before now may appear after inserted HTML.
            m_buffer.OutputText(ClearBuffer());
            m_buffer.AddJavaScriptToBuffer(args[0], parameters.ToArray());
        }

        private void SetAlignment(string align)
        {
            if (align.Length == 0) align = "left";
            m_buffer.OutputText(ClearBuffer());
            m_buffer.AddJavaScriptToBuffer("createNewDiv", new StringParameter(align));
        }

        private void ShowPicture(string filename)
        {
            string url = AddResource(filename);
            m_buffer.OutputText(string.Format("<img src=\"{0}\" onload=\"scrollToEnd();\" /><br />", url));
        }

        public void SendCommand(string command)
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Command entered: {1}", GameId, command);
            m_game.SendCommand(command);
        }

        public Action<string, IDictionary<string, string>, bool> ShowMenuDelegate { get; set; }

        public void ShowMenu(MenuData menuData)
        {
            Logging.Log.DebugFormat("{0} Showing menu", GameId);
            ShowMenuDelegate(menuData.Caption, menuData.Options, menuData.AllowCancel);
        }

        public void SetMenuResponse(string response)
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Menu response received", GameId);
            m_game.SetMenuResponse(response);
        }

        public void CancelMenu()
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Menu cancelled", GameId);
            m_game.SetMenuResponse(null);
        }

        public void DoWait()
        {
            Logging.Log.DebugFormat("{0} Wait beginning", GameId);
            BeginWait();
        }

        public void EndWait()
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Wait ending", GameId);
            m_game.FinishWait();
        }

        public Action<string> ShowQuestionDelegate { get; set; }

        public void ShowQuestion(string caption)
        {
            Logging.Log.DebugFormat("{0} Showing message box", GameId);
            ShowQuestionDelegate(caption);
        }

        public void SetQuestionResponse(string response)
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Question response received", GameId);
            m_game.SetQuestionResponse(response == "yes");
        }

        public void SetWindowMenu(MenuData menuData)
        {
            // Do nothing
        }

        public string GetNewGameFile(string originalFilename, string extensions)
        {
            return string.Empty;
        }

        public void PlaySound(string filename, bool synchronous, bool looped)
        {
            PlayAudio(this, new PlayAudioEventArgs { Filename = filename, Synchronous = synchronous, Looped = looped });
        }

        public void StopSound()
        {
            StopAudio();
        }

        public void Tick()
        {
            if (m_gameTimer != null) m_gameTimer.Tick();
        }

        public void EndGame()
        {
            Logging.Log.InfoFormat("{0} Ending game", GameId);
            m_game.Finish();
        }

        public IEnumerable<string> SetUpExternalScripts()
        {
            // Get all external JS scripts in the game, add them as resources, and then
            // return the list of JS resource URLs.
            var result = new List<string>();
            var scripts = m_game.GetExternalScripts();
            if (scripts == null) return result;
            
            foreach (string script in scripts)
            {
                string url = AddResource(script);
                result.Add(url);
            }

            return result;
        }

        public void WriteHTML(string html)
        {
            m_buffer.OutputText(html);
        }

        public void SendEvent(string eventName, string param)
        {
            if (m_finished) return;
            m_game.SendEvent(eventName, param);
        }

        private bool Finished
        {
            get { return m_finished; }
            set {
                if (value != m_finished)
                {
                    m_buffer.AddJavaScriptToBuffer("gameFinished");
                    m_finished = value;
                }
            }
        }

        public bool UseTimer
        {
            get
            {
                return m_gameTimer != null;
            }
        }

        private void LocationUpdated(string location)
        {
            m_buffer.AddJavaScriptToBuffer("updateLocation", new StringParameter(location));
        }

        public void UpdateGameName(string name)
        {
            if (name.Length == 0) name = "Untitled Game";
            m_buffer.AddJavaScriptToBuffer("setGameName", new StringParameter(name));
        }

        private void ClearScreen()
        {
            m_buffer.AddJavaScriptToBuffer("clearScreen");
        }

        private void SetPanesVisible(bool visible)
        {
            m_buffer.AddJavaScriptToBuffer("panesVisible", new BooleanParameter(visible));
        }

        private void SetStatusText(string text)
        {
            m_buffer.AddJavaScriptToBuffer("updateStatus", new StringParameter(text));
        }
    }
}