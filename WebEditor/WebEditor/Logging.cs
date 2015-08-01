using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebEditor
{
    public static class Logging
    {
        private static readonly log4net.ILog s_log = log4net.LogManager.GetLogger("WebEditor");

        static Logging()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static log4net.ILog Log { get { return s_log; } }
    }
}