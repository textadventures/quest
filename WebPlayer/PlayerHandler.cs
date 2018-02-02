using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;
using System.Xml;
using System.Configuration;
using TextAdventures.Utility.JSInterop;

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
            public string GameId { get; set; }
            public string Filename { get; set; }
            public bool Synchronous { get; set; }
            public bool Looped { get; set; }
        }

        public event Action BeginWait;
        public event Action<int> BeginPause;
        public event Func<string, string, string> AddResource;
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
        public string LoadData { get; set; }
        public AzureFileManager.ApiGame ApiGameData { get; set; }
        public string ResourceUrlRoot { get; set; }

        public bool Initialise(out List<string> errors, bool? isCompiled)
        {
            Logging.Log.DebugFormat("{0} Initialising {1}", GameId, m_filename);

            IASL game;
            if (LoadData != null)
            {
                game = GameLauncher.GetGame(Convert.FromBase64String(LoadData), ApiGameData.ASLVersion, ApiGameData.SourceGameUrl);
            }
            else
            {
                game = GameLauncher.GetGame(m_filename, LibraryFolder);
            }
            
            m_controller = new PlayerHelper(game, this);
            m_controller.UseGameColours = true;
            m_controller.UseGameFont = true;
            m_controller.Game.LogError += LogError;
            m_controller.Game.UpdateList += UpdateList;
            m_controller.Game.Finished += GameFinished;

            IASLTimer gameTimer = game as IASLTimer;
            if (gameTimer != null)
            {
                gameTimer.RequestNextTimerTick += RequestNextTimerTick;
            }

            bool success = m_controller.Initialise(this, out errors, isCompiled);
            if (success)
            {
                Logging.Log.DebugFormat("{0} Initialised successfully", GameId);
            }
            else
            {
                Logging.Log.WarnFormat("{0} Failed to initialise game - errors found in file", GameId);
            }

            return success;
        }

        internal static string GetUI()
        {
            return PlayerHelper.GetUIHTML();
        }

        void RequestNextTimerTick(int seconds)
        {
            m_buffer.AddJavaScriptToBuffer("requestNextTimerTick", new IntParameter(seconds));
        }

        void GameFinished()
        {
            Finished = true;
        }

        public void Quit()
        {
            Finished = true;
        }

        public void BindMenu(string linkid, string verbs, string text, string elementId)
        {
            m_buffer.AddJavaScriptToBuffer("bindMenu", new StringParameter(linkid), new StringParameter(verbs), new StringParameter(text), new StringParameter(elementId));
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
            Logging.Log.DebugFormat("{0} Beginning game", GameId);
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

        public void RunScript(string data, object[] parameters)
        {
            // Clear text buffer before running custom JavaScript, otherwise text written
            // before now may appear after inserted HTML.
            m_buffer.OutputText(ClearBuffer());

            if (parameters != null)
            {
                var paramValues = parameters.Select(GetScriptParameter);
                m_buffer.AddJavaScriptToBuffer(data, paramValues.ToArray());
            }
            else
            {
                m_buffer.AddJavaScriptToBuffer(data);
            }
        }

        private IJavaScriptParameter GetScriptParameter(object arg)
        {
            if (arg == null)
            {
                return new NullParameter();
            }
            if (arg is string)
            {
                return new StringParameter((string)arg);
            }
            if (arg is int)
            {
                return new IntParameter((int)arg);
            }
            if (arg is double)
            {
                return new DoubleParameter((double)arg);
            }
            if (arg is bool)
            {
                return new BooleanParameter((bool)arg);
            }
            var dictionary = arg as IDictionary<string, string>;
            if (dictionary != null)
            {
                return new DictionaryParameter(dictionary);
            }
            return new JSONParameter(arg);
        }

        public void SetAlignment(string align)
        {
            if (align.Length == 0) align = "left";
            m_buffer.OutputText(ClearBuffer());
            m_buffer.AddJavaScriptToBuffer("createNewDiv", new StringParameter(align));
        }

        public void ShowPicture(string filename)
        {
            m_buffer.OutputText(ClearBuffer());
            string url = GetURL(filename);
            m_buffer.OutputText(string.Format("<img src=\"{0}\" onload=\"scrollToEnd();\" /><br />", url));
        }

        public void SendCommand(string command, int tickCount)
        {
            if (m_finished) return;
            var data = PlayerHelper.GetCommandData(command);
            Logging.Log.DebugFormat("{0} Command entered: {1}", GameId, command);
            m_controller.SendCommand(data.Command, tickCount, data.Metadata);
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

        public void DoPause(int ms)
        {
            Logging.Log.DebugFormat("{0} Pause beginning", GameId);
            BeginPause(ms);
        }

        public void EndWait()
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Wait ending", GameId);
            m_controller.Game.FinishWait();
        }

        public void EndPause()
        {
            if (m_finished) return;
            Logging.Log.DebugFormat("{0} Pause ending", GameId);
            m_controller.Game.FinishPause();
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
            PlayAudio(this, new PlayAudioEventArgs
            {
                GameId = m_controller.Game.GameID,
                Filename = filename,
                Synchronous = synchronous,
                Looped = looped
            });
        }

        public void StopSound()
        {
            StopAudio();
        }

        public void Tick(int tickCount)
        {
            m_controller.GameTimer.Tick(tickCount);
        }

        public void EndGame()
        {
            Logging.Log.DebugFormat("{0} Ending game", GameId);
            m_buffer.Finish();
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
                string url = GetURL(script);
                result.Add(url);
            }

            return result;
        }

        public IEnumerable<string> GetExternalStylesheets()
        {
            return m_controller.Game.GetExternalStylesheets();
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

        public void SendAttribute(string eventName, string param)
        {
            if (m_finished) return;
            m_controller.Game.SendAttribute(eventName, param);
        }

        private bool Finished
        {
            get { return m_finished; }
            set
            {
                if (value != m_finished)
                {
                    m_buffer.AddJavaScriptToBuffer("gameFinished");
                    m_finished = value;
                }
            }
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
            m_buffer.AddJavaScriptToBuffer("updateStatus", new StringParameter(text.Replace(Environment.NewLine, "<br />")));
        }

        public void OutputText(string text)
        {
            m_buffer.OutputText(text);
        }

        public void SetForeground(string colour)
        {
            m_buffer.AddJavaScriptToBuffer("setForeground", new StringParameter(colour));
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

        public void Speak(string text)
        {
            // Do nothing
        }

        public void RequestSave(string html)
        {
            var data = m_controller.Game.Save(html);
            m_buffer.AddJavaScriptToBuffer("saveGameResponse", new StringParameter(Convert.ToBase64String(data)));
        }

        public void SetLinkForeground(string colour)
        {
            m_controller.SetLinkForeground(colour);
        }

        public void Show(string element)
        {
            DoShowHide(element, true);
        }

        public void Hide(string element)
        {
            DoShowHide(element, false);
        }

        private void DoShowHide(string element, bool show)
        {
            string jsElement = GetElementId(element);
            if (string.IsNullOrEmpty(jsElement)) return;
            m_buffer.AddJavaScriptToBuffer(show ? "uiShow" : "uiHide", new StringParameter(jsElement));
        }

        private static Dictionary<string, string> s_elementMap = new Dictionary<string, string>
        {
            { "Panes", "#gamePanes" },
            { "Location", "#location" },
            { "Command", "#txtCommandDiv" }
        };

        private string GetElementId(string code)
        {
            string id;
            s_elementMap.TryGetValue(code, out id);
            return id;
        }

        public void SetCompassDirections(IEnumerable<string> dirs)
        {
            m_buffer.AddJavaScriptToBuffer("setCompassDirections", new StringArrayParameter(dirs));
        }

        public string GetURL(string file)
        {
            if (Config.ReadGameFileFromAzureBlob)
            {
                if (ResourceUrlRoot == null)
                {
                    return string.Format("https://textadventures.blob.core.windows.net/gameresources/{0}/{1}",
                        m_controller.Game.GameID,
                        file);
                }
                return ResourceUrlRoot + file;
            }

            return AddResource(m_controller.Game.GameID, file);
        }

        public void SetInterfaceString(string name, string text)
        {
            m_buffer.AddJavaScriptToBuffer("setInterfaceString", new StringParameter(name), new StringParameter(text));
        }

        public void SetPanelContents(string html)
        {
            m_buffer.AddJavaScriptToBuffer("setPanelContents", new StringParameter(html));
        }

        public void Log(string text)
        {
        }

        public string GetUIOption(UIOption option)
        {
            if (option == UIOption.UseGameColours || option == UIOption.UseGameFont)
            {
                return "true";
            }

            return null;
        }

        public IASL Game { get { return m_controller.Game; } }
    }
}