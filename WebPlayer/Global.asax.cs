using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace WebPlayer
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            if (Request.Url.AbsolutePath.Contains("favicon.ico")) return;
            Exception error = Server.GetLastError().GetBaseException();
            Logging.Log.ErrorFormat("Error in {0}: {1}\n{2}\n\nIP: {3}\nReferrer: {4}\nUserAgent: {5}",
                Request.Url,
                error.Message,
                error.StackTrace,
                Request.UserHostAddress,
                Request.UrlReferrer,
                Request.UserAgent);
        }

        protected void Session_End(object sender, EventArgs e)
        {
            // If the session ends, we want to terminate any threads that may be waiting
            // for e.g. a menu reponse

            var gamesInSession = (Dictionary<string, PlayerHandler>)Session["Games"];
            if (gamesInSession != null)
            {
                foreach (PlayerHandler handler in gamesInSession.Values)
                {
                    handler.EndGame();
                }
            }
        }
    }
}