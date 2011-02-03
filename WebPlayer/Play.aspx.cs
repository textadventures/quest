using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace WebPlayer
{
    public partial class Play : System.Web.UI.Page
    {
        private PlayerHandler m_player;
        private OutputBuffer m_buffer;
        private string m_gameId;
        private Dictionary<string, PlayerHandler> m_gamesInSession;

        protected void Page_Load(object sender, EventArgs e)
        {
            // We store the game in the Session, but use a dictionary keyed by GUIDs which
            // are stored in the ViewState. This allows the same user in the same browser
            // to open multiple games in different browser tabs.

            m_gamesInSession = (Dictionary<string, PlayerHandler>)Session["Games"];
            if (m_gamesInSession == null)
            {
                m_gamesInSession = new Dictionary<string, PlayerHandler>();
                Session["Games"] = m_gamesInSession;
            }

            Dictionary<string, OutputBuffer> outputBuffersInSession = (Dictionary<string, OutputBuffer>)Session["OutputBuffers"];
            if (outputBuffersInSession == null)
            {
                outputBuffersInSession = new Dictionary<string, OutputBuffer>();
                Session["OutputBuffers"] = outputBuffersInSession;
            }

            m_gameId = (string)ViewState["GameId"];
            if (m_gameId == null)
            {
                m_gameId = Guid.NewGuid().ToString();
                ViewState["GameId"] = m_gameId;
            }

            if (Page.IsPostBack)
            {
                if (m_gamesInSession.ContainsKey(m_gameId))
                {
                    m_player = m_gamesInSession[m_gameId];
                }

                if (!outputBuffersInSession.ContainsKey(m_gameId))
                {
                    // TO DO: Think this only ever happens while debugging?
                    return;
                }
                m_buffer = outputBuffersInSession[m_gameId];
            }
            else
            {
                m_buffer = new OutputBuffer();
                outputBuffersInSession.Add(m_gameId, m_buffer);
            }
        }

        protected void InitTimerTick(object sender, EventArgs e)
        {
            if (m_buffer == null) return;
            m_buffer.InitStage++;

            switch (m_buffer.InitStage)
            {
                case 1:
                    tmrTick.Enabled = false;
                    string initialText = LoadGame(Request["file"]);
                    if (m_player == null)
                    {
                        tmrInit.Enabled = false;
                        UpdateGameName("Error loading game");
                    }
                    OutputTextNow(initialText);
                    break;
                case 2:
                    tmrInit.Enabled = false;

                    // We could be more sophisticated here perhaps, and only enable tmrTick
                    // if there are any game timers running. But this timer also ensures that
                    // the buffer is cleared every second, which may be useful. It is helpful
                    // to disable this when testing things such as "wait" commands, as we want
                    // the text that follows a "wait" command to come back synchronously.
                    tmrTick.Enabled = true;

                    m_player.BeginGame();
                    OutputTextNow(m_player.ClearBuffer());
                    break;
            }
        }

        protected void TimerTick(object sender, EventArgs e)
        {
            if (m_player == null) return;
            m_player.Tick();
            string output = m_player.ClearBuffer();
            if (output.Length > 0) OutputTextNow(output);
        }

        private string LoadGame(string gameFile)
        {
            if (string.IsNullOrEmpty(gameFile))
            {
                return "No game specified";
            }

            // Block attempts to access files outside the GameFolder
            if (gameFile.Contains(".."))
            {
                return "Invalid filename";
            }

            string rootPath = ConfigurationManager.AppSettings["GameFolder"];
            string libPath = ConfigurationManager.AppSettings["LibraryFolder"];
            string filename = System.IO.Path.Combine(rootPath, gameFile);
            List<string> errors;

            try
            {
                m_player = new PlayerHandler(filename);
                m_player.GameId = m_gameId;
                m_player.LibraryFolder = libPath;
                m_gamesInSession[m_gameId] = m_player;
                m_player.LocationUpdated += m_player_LocationUpdated;
                m_player.BeginWait += m_player_BeginWait;
                m_player.ShowMenuDelegate = m_player_ShowMenu;
                m_player.ShowQuestionDelegate = m_player_ShowQuestion;
                m_player.GameNameUpdated += m_player_GameNameUpdated;
                m_player.ClearScreen += m_player_ClearScreen;
                m_player.ShowPicture += m_player_ShowPicture;
                
                if (m_player.Initialise(out errors))
                {
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

        void m_player_ShowPicture(string filename)
        {
            SessionResources resources = Session["Resources"] as SessionResources;
            if (resources == null)
            {
                resources = new SessionResources();
                Session["Resources"] = resources;
            }

            string key = resources.Add(filename);
            m_buffer.AddJavaScriptToBuffer("showPicture", new StringParameter(key));
        }

        void m_player_BeginWait()
        {
            m_buffer.AddJavaScriptToBuffer("beginWait");
        }

        void m_player_LocationUpdated(string location)
        {
            m_buffer.AddJavaScriptToBuffer("updateLocation", new StringParameter(location));
        }

        void m_player_ShowMenu(string caption, IDictionary<string, string> options, bool allowCancel)
        {
            m_buffer.AddJavaScriptToBuffer("showMenu", new StringParameter(caption), new JSONParameter(options), new BooleanParameter(allowCancel));
        }

        void m_player_ShowQuestion(string caption)
        {
            m_buffer.AddJavaScriptToBuffer("showQuestion", new StringParameter(caption));
        }

        void m_player_GameNameUpdated(string name)
        {
            UpdateGameName(name);
        }

        void UpdateGameName(string name)
        {
            if (name.Length == 0) name = "Untitled Game";
            m_buffer.AddJavaScriptToBuffer("setGameName", new StringParameter(name));
        }

        void m_player_ClearScreen()
        {
            m_buffer.AddJavaScriptToBuffer("clearScreen");
        }

        protected void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (m_player == null)
            {
                SessionTimeoutMessage("<b>Sorry, your session has expired and the game has finished.</b><br/><br/>Press your browser's Refresh button to start again.");
                return;
            }

            if (fldUIMsg.Value.Length > 0)
            {
                string[] args = fldUIMsg.Value.Split(new char[] { ' ' }, 2);
                switch (args[0])
                {
                    case "endwait":
                        m_player.EndWait();
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
                }
                fldUIMsg.Value = "";
            }
            else if (fldCommand.Value.Length > 0)
            {
                string command = fldCommand.Value;
                fldCommand.Value = "";
                m_player.SendCommand(command);
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

        void SessionTimeoutMessage(string text)
        {
            m_buffer = new OutputBuffer();
            m_buffer.OutputText(text);
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
    }
}