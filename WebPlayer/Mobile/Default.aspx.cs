using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

namespace WebPlayer.Mobile
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string origUrl = Request.QueryString.Get("origUrl");
            int queryPos = origUrl.IndexOf("Play.aspx?");
            if (queryPos != -1)
            {
                var origUrlValues = HttpUtility.ParseQueryString(origUrl.Substring(origUrl.IndexOf("?")));
                string id = origUrlValues.Get("id");
                Response.Redirect("Play.aspx?id=" + id);
                return;
            }

            Response.Clear();
            Response.StatusCode = 404;
            Response.End();
        }
    }
}