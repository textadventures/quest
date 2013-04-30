using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using WebInterfaces;

namespace WebPlayer
{
    public static class HTMLManagerLoader
    {
        private static bool s_loaded = false;
        private static IHTMLManager s_htmlManager = null;

        public static IHTMLManager GetHTMLManager()
        {
            if (s_loaded)
            {
                return s_htmlManager;
            }
            else
            {
                string typeName = ConfigurationManager.AppSettings["HTMLManagerType"];
                s_loaded = true;
                if (typeName == null) return null;
                s_htmlManager = (IHTMLManager)Activator.CreateInstance(Type.GetType(typeName));
                return s_htmlManager;
            }
        }
    }
}