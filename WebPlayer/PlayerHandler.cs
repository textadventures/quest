using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;
using System.Xml;

namespace WebPlayer
{
    internal class PlayerHandler : IPlayer, IPlayerHelperUI
    {
        private PlayerHelper m_controller;
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

            m_controller = new PlayerHelper(m_filename, LibraryFolder, this);
            m_controller.Game.LogError += LogError;
            m_controller.Game.UpdateList += UpdateList;

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

        public void Quit()
        {
            Finished = true;
        }

        public void BindMenu(string linkid, string verbs, string text)
        {
            m_buffer.AddJavaScriptToBuffer("bindMenu", new StringParameter(linkid), new StringParameter(verbs), new StringParameter(text));
        }

        private void UpdateList(ListType listType, List<ListData> items)
        {
            m_listHandler.UpdateList(listType, items);
        }

        private void LogError(string errorMessage)
        {
            m_buffer.OutputText("[Sorry, an error occurred]");
            Logging.Log.Error(errorMessage);
        }

        public void BeginGame()
        {
            Logging.Log.InfoFormat("{0} Beginning game", GameId);
            m_controller.Game.Begin();
        }

        public string ClearBuffer()
        {
            return m_controller.ClearBuffer();
        }

        public void SetBackground(string col)
        {
            m_buffer.AddJavaScriptToBuffer("setBackground", new StringParameter(col));
        }

        public void RunScript(string data)
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

        public void SetAlignment(string align)
        {
            if (align.Length == 0) align = "left";
            m_buffer.OutputText(ClearBuffer());
            m_buffer.AddJavaScriptToBuffer("createNewDiv", new StringParameter(align));
        }

        public void ShowPicture(string filename)
        {
            string url = AddResource(filename);
            m_buffer.OutputText(string.Format("<img src=\"{0}\" onload=\"scrollToEnd();\" /><br />", url));
        }

        public void SendCommand(string command)
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Command entered: {1}", GameId, command);
            m_controller.Game.SendCommand(command);
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
            m_controller.Game.SetMenuResponse(response);
        }

        public void CancelMenu()
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Menu cancelled", GameId);
            m_controller.Game.SetMenuResponse(null);
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
            m_controller.Game.FinishWait();
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
            m_controller.Game.SetQuestionResponse(response == "yes");
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
            m_controller.Tick();
        }

        public void EndGame()
        {
            Logging.Log.InfoFormat("{0} Ending game", GameId);
            m_controller.Game.Finish();
        }

        public IEnumerable<string> SetUpExternalScripts()
        {
            // Get all external JS scripts in the game, add them as resources, and then
            // return the list of JS resource URLs.
            var result = new List<string>();
            var scripts = m_controller.Game.GetExternalScripts();
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
            m_controller.Game.SendEvent(eventName, param);
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
            get { return m_controller.UseTimer; }
        }

        public void LocationUpdated(string location)
        {
            m_buffer.AddJavaScriptToBuffer("updateLocation", new StringParameter(location));
        }

        public void UpdateGameName(string name)
        {
            if (name.Length == 0) name = "Untitled Game";
            m_buffer.AddJavaScriptToBuffer("setGameName", new StringParameter(name));
        }

        public void ClearScreen()
        {
            OutputText(ClearBuffer());
            m_buffer.AddJavaScriptToBuffer("clearScreen");
        }

        public void SetPanesVisible(string data)
        {
            bool visible = (data == "on");
            m_buffer.AddJavaScriptToBuffer("panesVisible", new BooleanParameter(visible));
        }

        public void SetStatusText(string text)
        {
            m_buffer.AddJavaScriptToBuffer("updateStatus", new StringParameter(text));
        }

        public void OutputText(string text)
        {
            m_buffer.OutputText(text);
        }

        public void SetForeground(string colour)
        {
            m_controller.SetForeground(colour);
        }

        public void SetFont(string fontName)
        {
            m_controller.SetFont(fontName);
        }

        public void SetFontSize(string fontSize)
        {
            m_controller.SetFontSize(fontSize);
        }
    }
}