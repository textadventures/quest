using System;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace WebPlayer.Mobile
{
    public class KeepAlive : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Session["KeepAlive"] = DateTime.Now;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}