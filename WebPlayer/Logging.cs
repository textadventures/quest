using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebPlayer
{
    public static class Logging
    {
        private static readonly log4net.ILog s_log = log4net.LogManager.GetLogger("WebPlayer");

        static Logging()
        {
            string logConfig = System.Configuration.ConfigurationManager.AppSettings["LogConfig"];
            if (!string.IsNullOrEmpty(logConfig))
            {
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(logConfig));
            }
        }

        public static log4net.ILog Log { get { return s_log; } }
    }
}