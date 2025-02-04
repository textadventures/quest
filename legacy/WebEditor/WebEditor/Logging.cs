using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using log4net.Config;
using log4net.Core;

namespace WebEditor
{
    public static class Logging
    {
        private static readonly log4net.ILog s_log = log4net.LogManager.GetLogger("WebEditor");

        static Logging()
        {
            if (ConfigurationManager.AppSettings["AzureLogTable"] != null)
            {
                Level minLogLevel = null;

                var minLogLevelName = ConfigurationManager.AppSettings["LogLevel"];
                if (minLogLevelName != null)
                {
                    var map = LoggerManager.GetAllRepositories().First().LevelMap;
                    minLogLevel = map[minLogLevelName];
                }

                BasicConfigurator.Configure(new TableStorageAppender(minLogLevel));
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