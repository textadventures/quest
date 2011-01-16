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

        protected void Page_Load(object sender, EventArgs e)
        {
            // TO DO: Because the PlayerHandler is stored in the Session, odd things
            // happen if you open game in a new tab, because it will have the same
            // session. Maybe we should also have some extra data in the ViewState to
            // point to a particular game within the session?

            if (Page.IsPostBack)
            {
                m_player = (PlayerHandler)Session["Player"];
                m_buffer = (OutputBuffer)Session["Buffer"];
            }
            else
            {
                m_buffer = new OutputBuffer();
                Session["Buffer"] = m_buffer;
            }
        }

        protected void InitTimerTick(object sender, EventArgs e)
        {
            tmrInit.Enabled = false;
            tmrTick.Enabled = false;
            string initialText = LoadGame(Request["file"]);
            OutputTextNow(initialText);
            if (m_player != null)
            {
                // TO DO: Query if the game even has timers, if not then there's no need
                // for tmrTick to be enabled at all.
                tmrTick.Enabled = true;
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
                m_player.LibraryFolder = libPath;
                Session["Player"] = m_player;
                m_player.LocationUpdated += m_player_LocationUpdated;
                
                if (m_player.StartGame(out errors))
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

        void m_player_LocationUpdated(string location)
        {
            m_buffer.AddJavaScriptToBuffer("updateLocation", m_buffer.StringParameter(location));
        }

        protected void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (fldCommand.Value.Length > 0)
            {
                string command = fldCommand.Value;
                fldCommand.Value = "";
                m_player.SendCommand(command);
                string output = m_player.ClearBuffer();
                m_buffer.OutputText(output);
                ClearJavaScriptBuffer();
            }
        }

        void OutputTextNow(string text)
        {
            m_buffer.OutputText(text);
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