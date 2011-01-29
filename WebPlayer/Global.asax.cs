using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebPlayer
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly log4net.ILog s_log = log4net.LogManager.GetLogger(typeof(Global));

        static Global()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            s_log.Info("Application Start");
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
            s_log.ErrorFormat("Error in {0}: {1}\n{2}", Request.Url, error.Message, error.StackTrace);
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

        protected void Application_End(object sender, EventArgs e)
        {
            s_log.Info("Application End");
        }
    }
}