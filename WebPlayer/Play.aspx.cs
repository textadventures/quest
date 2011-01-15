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
        private string m_gameFile = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                m_player = (PlayerHandler)Session["Player"];
            }
        }

        protected void InitTimerTick(object sender, EventArgs e)
        {
            string initialText = LoadGame(Request["file"]);
            OutputText(initialText);
            tmrInit.Enabled = false;
            if (m_player != null) tmrTick.Enabled = true;
        }

        protected void TimerTick(object sender, EventArgs e)
        {
            if (m_player == null) return;
            m_player.Tick();
            string output = m_player.ClearBuffer();
            if (output.Length > 0) OutputText(output);
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

        protected void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (txtCommand.Text.Length > 0)
            {
                string command = txtCommand.Text;
                txtCommand.Text = "";
                m_player.SendCommand(command);
                string output = m_player.ClearBuffer();
                txtCommand.Focus();
                OutputText(output);
            }
        }

        void OutputText(string text)
        {
            CallJavaScript("addText", StringParameter(text));
        }

        string StringParameter(string parameter)
        {
            return string.Format("\"{0}\"", parameter.Replace("\"", "\\\""));
        }

        void CallJavaScript(string function, params string[] parameters)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "script", string.Format("{0}({1})", function, string.Join(",", parameters)), true);
        }
    }
}