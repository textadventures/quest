using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using TextAdventures.Utility.JSInterop;
using WebInterfaces;

namespace WebPlayer.Mobile
{
    public partial class Play : System.Web.UI.Page
    {
        private PlayerHandler m_player;
        private OutputBuffer m_buffer;
        private string m_gameId;

        protected void Page_Load(object sender, EventArgs e)
        {
            // We store the game in the Session, but use a dictionary keyed by GUIDs which
            // are stored in the ViewState. This allows the same user in the same browser
            // to open multiple games in different browser tabs.

            if (Games == null)
            {
                Games = new Dictionary<string, PlayerHandler>();
            }

            if (OutputBuffers == null)
            {
                OutputBuffers = new Dictionary<string, OutputBuffer>();
            }

            if (Resources == null)
            {
                Resources = new SessionResources();
            }

            m_gameId = (string)ViewState["GameId"];
            if (m_gameId == null)
            {
                m_gameId = Guid.NewGuid().ToString();
                ViewState["GameId"] = m_gameId;
            }

            if (Page.IsPostBack)
            {
                if (Games.ContainsKey(m_gameId))
                {
                    m_player = Games[m_gameId];
                }

                if (!OutputBuffers.ContainsKey(m_gameId))
                {
                    // TO DO: Think this only ever happens while debugging?
                    return;
                }
                m_buffer = OutputBuffers[m_gameId];
            }
            else
            {
                m_buffer = new OutputBuffer(m_gameId);
                OutputBuffers.Add(m_gameId, m_buffer);
                m_buffer.AddJavaScriptToBuffer("setOutputBufferId", new StringParameter(m_gameId));
            }
        }

        protected void InitTimerTick(object sender, EventArgs e)
        {
            if (m_buffer == null) return;
            m_buffer.InitStage++;

            switch (m_buffer.InitStage)
            {
                case 1:
                    string initialText = LoadGameForRequest();
                    if (m_player == null)
                    {
                        tmrInit.Enabled = false;
                    }
                    else
                    {
                        RegisterExternalScripts();
                        RegisterExternalStylesheets();
                    }

                    OutputTextNow(initialText);
                    break;
                case 2:
                    tmrInit.Enabled = false;

                    m_player.BeginGame();
                    OutputTextNow(m_player.ClearBuffer());
                    break;
            }
        }

        private string LoadGameForRequest()
        {
            string folder = null;
            string gameFile = Request["file"];
            string id = Request["id"];

            if (string.IsNullOrEmpty(gameFile))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    IFileManager fileManager = FileManagerLoader.GetFileManager();
                    if (fileManager != null)
                    {
                        gameFile = fileManager.GetFileForID(id);
                    }
                }
            }

            AzureFileManager.ApiGame apiGameData = null;

            var loadData = Session["LoadData"] as string;
            Session["LoadData"] = null;

            if (loadData != null)
            {
                apiGameData = AzureFileManager.GetGameData(id);
                if (apiGameData == null)
                {
                    throw new InvalidOperationException("No API data returned for game id " + id);
                }
            }

            return LoadGame(gameFile, id, folder, loadData, apiGameData);
        }

        private string LoadGame(string gameFile, string id, string folder, string loadData, AzureFileManager.ApiGame apiGameData)
        {
            if (string.IsNullOrEmpty(gameFile) && loadData == null)
            {
                return "No game specified";
            }

            string rootPath = folder ?? ConfigurationManager.AppSettings["GameFolder"];
            string libPath = ConfigurationManager.AppSettings["LibraryFolder"];
            string filename;

            if (Config.ReadGameFileFromAzureBlob)
            {
                filename = gameFile;
            }
            else
            {
                filename = WebPlayer.Play.GetGameFilename(gameFile, rootPath);
                if (filename == null)
                {
                    return "Invalid filename";
                }
            }

            List<string> errors;

            try
            {
                m_player = new PlayerHandler(filename, m_buffer);
                m_player.LoadData = loadData;
                m_player.ApiGameData = apiGameData;
                m_player.GameId = m_gameId;
                m_player.LibraryFolder = libPath;
                Games[m_gameId] = m_player;
                m_player.BeginWait += m_player_BeginWait;
                m_player.BeginPause += m_player_BeginPause;
                m_player.ShowMenuDelegate = m_player_ShowMenu;
                m_player.ShowQuestionDelegate = m_player_ShowQuestion;
                m_player.AddResource += AddResource;
                m_player.PlayAudio += m_player_PlayAudio;
                m_player.StopAudio += m_player_StopAudio;
                if (Config.ReadGameFileFromAzureBlob)
                {
                    m_player.ResourceUrlRoot = AzureFileManager.GetResourceUrlRoot(id);
                }

                if (m_player.Initialise(out errors))
                {
                    Resources.AddGame(m_player.Game);

                    // Successful game start
                    return m_player.ClearBuffer();
                }
            }
            catch (Exception ex)
            {
                return "<b>Error loading game:</b><br/>" + ex.Message;
            }

            string output = string.Empty;

            foreach (string error in errors)
            {
                output += error + "<br/>";
            }

            return output;
        }

        void RegisterExternalScripts()
        {
            int count = 0;
            foreach (string url in m_player.SetUpExternalScripts())
            {
                count++;
                ScriptManager.RegisterClientScriptInclude(this, this.GetType(), "scriptResource" + count.ToString(), url);
            }
        }

        private void RegisterExternalStylesheets()
        {
            var stylesheets = m_player.GetExternalStylesheets();
            if (stylesheets == null) return;

            foreach (var stylesheet in stylesheets)
            {
                m_buffer.AddJavaScriptToBuffer("addExternalStylesheet", new StringParameter(stylesheet));
            }
        }

        void m_player_PlayAudio(object sender, PlayerHandler.PlayAudioEventArgs e)
        {
            string functionName = null;
            if (e.Filename.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase)) functionName = "playWav";
            if (e.Filename.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase)) functionName = "playMp3";

            if (functionName == null) return;

            string url = m_player.GetURL(e.Filename);
            
            m_buffer.AddJavaScriptToBuffer(
                functionName,
                new StringParameter(url),
                new BooleanParameter(e.Synchronous),
                new BooleanParameter(e.Looped));
        }

        void m_player_StopAudio()
        {
            m_buffer.AddJavaScriptToBuffer("stopAudio");
        }

        string AddResource(string gameId, string filename)
        {
            return "../Resource.ashx?id=" + gameId + "&filename=" + filename;
        }

        void m_player_BeginWait()
        {
            m_buffer.AddJavaScriptToBuffer("beginWait");
        }

        void m_player_BeginPause(int ms)
        {
            m_buffer.AddJavaScriptToBuffer("beginPause", new IntParameter(ms));
        }

        void m_player_ShowMenu(string caption, IDictionary<string, string> options, bool allowCancel)
        {
            m_buffer.AddJavaScriptToBuffer("showMenu", new StringParameter(caption), new DictionaryParameter(options), new BooleanParameter(allowCancel));
        }

        void m_player_ShowQuestion(string caption)
        {
            m_buffer.AddJavaScriptToBuffer("showQuestion", new StringParameter(caption));
        }

        protected void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (m_player == null)
            {
                SessionTimeoutMessage();
                return;
            }

            int tickCount = 0;
            if (fldUITickCount.Value.Length > 0)
            {
                int.TryParse(fldUITickCount.Value, out tickCount);
                fldUITickCount.Value = "";
            }

            if (fldUIMsg.Value.Length > 0)
            {
                string[] args = fldUIMsg.Value.Split(new[] { ' ' }, 2);
                switch (args[0])
                {
                    case "command":
                        m_player.SendCommand(args[1], tickCount);
                        break;
                    case "endwait":
                        m_player.EndWait();
                        break;
                    case "endpause":
                        m_player.EndPause();
                        break;
                    case "choice":
                        m_player.SetMenuResponse(args[1]);
                        break;
                    case "choicecancel":
                        m_player.CancelMenu();
                        break;
                    case "msgbox":
                        m_player.SetQuestionResponse(args[1]);
                        break;
                    case "event":
                        SendEvent(args[1]);
                        break;
                    case "tick":
                        m_player.Tick(tickCount);
                        break;
                    case "save":
                        string unescapedHtml = args[1].Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
                        m_player.RequestSave(unescapedHtml);
                        break;
                }
                fldUIMsg.Value = "";
            }

            string output = m_player.ClearBuffer();
            m_buffer.OutputText(output);
            ClearJavaScriptBuffer();
        }

        void OutputTextNow(string text)
        {
            m_buffer.OutputText(text);
            ClearJavaScriptBuffer();
        }

        void SessionTimeoutMessage()
        {
            m_buffer = new OutputBuffer(null);
            m_buffer.AddJavaScriptToBuffer("sessionTimeout");
            ClearJavaScriptBuffer();
        }

        void ClearJavaScriptBuffer()
        {
            int count = 0;
            foreach (string javaScript in m_buffer.ClearJavaScriptBuffer())
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "script" + count.ToString(), javaScript, true);
                count++;
            }
        }

        void SendEvent(string data)
        {
            string[] args = data.Split(new[] { ';' }, 2);
            m_player.SendEvent(args[0], args[1]);
        }

        private SessionResources Resources
        {
            get { return Session["Resources"] as SessionResources; }
            set { Session["Resources"] = value; }
        }

        private Dictionary<string, PlayerHandler> Games
        {
            get { return Session["Games"] as Dictionary<string, PlayerHandler>; }
            set { Session["Games"] = value; }
        }

        private Dictionary<string, OutputBuffer> OutputBuffers
        {
            get { return Session["OutputBuffers"] as Dictionary<string, OutputBuffer>; }
            set { Session["OutputBuffers"] = value; }
        }

        protected string ApiRoot()
        {
            return ConfigurationManager.AppSettings["BaseURI"] ?? "http://textadventures.co.uk/";
        }

        protected string GameSessionLogId()
        {
            return string.IsNullOrEmpty(ConfigurationManager.AppSettings["AzureLogSessionsBlob"]) ? "" : m_gameId;
        }
    }
}