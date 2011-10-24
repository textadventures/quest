using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace WebPlayer
{
    public static class SessionManagerLoader
    {
        private static bool s_loaded = false;
        private static ISessionManager s_sessionManager = null;

        public static ISessionManager GetSessionManager()
        {
            if (s_loaded)
            {
                return s_sessionManager;
            }
            else
            {
                string typeName = ConfigurationManager.AppSettings["SessionManagerType"];
                s_loaded = true;
                if (typeName == null) return null;
                s_sessionManager = (ISessionManager)Activator.CreateInstance(Type.GetType(typeName));
                return s_sessionManager;
            }
        }
    }

    public interface ISessionManager
    {
        IUser GetUser();
    }

    public interface IUser
    {
        string Username { get; set; }
    }
}