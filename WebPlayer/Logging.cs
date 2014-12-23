using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using log4net.Config;

namespace WebPlayer
{
    public static class Logging
    {
        private static readonly log4net.ILog s_log = log4net.LogManager.GetLogger("WebPlayer");

        static Logging()
        {
            if (ConfigurationManager.AppSettings["AzureLogTable"] != null)
            {
                BasicConfigurator.Configure(new TableStorageAppender());
                return;
            }

            string logConfig = ConfigurationManager.AppSettings["LogConfig"];
            if (!string.IsNullOrEmpty(logConfig))
            {
                XmlConfigurator.Configure(new System.IO.FileInfo(logConfig));
            }
        }

        public static log4net.ILog Log { get { return s_log; } }
    }
}