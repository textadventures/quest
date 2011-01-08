using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebPlayer
{
    public partial class Play : System.Web.UI.Page
    {
        private PlayerHandler m_player;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                m_player = new PlayerHandler(@"C:\VBProjects\Quest\trunk\WorldModel\WorldModel\Examples-Internal\test.aslx");
                Session["Player"] = m_player;
            }
            else
            {
                m_player = (PlayerHandler)Session["Player"];
            }
        }

        protected void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (txtCommand.Text.Length > 0)
            {
                string command = txtCommand.Text;
                txtCommand.Text = "";
                m_player.SendCommand(command);
                divOutput.InnerHtml += m_player.ClearBuffer();
                txtCommand.Focus();
            }
        }
    }
}