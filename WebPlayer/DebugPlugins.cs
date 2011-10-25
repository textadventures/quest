using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace WebPlayer
{
    internal class DebugSessionManager : ISessionManager
    {
        private IUser m_user = new DebugUser();

        public IUser GetUser()
        {
            return m_user;
        }
    }

    internal class DebugUser : IUser
    {
        public string Username
        {
            get
            {
                return "Debug User";
            }
        }
    }

    internal class DebugFileManager : IFileManager
    {
        public string GetFileForID(string id)
        {
            return ConfigurationManager.AppSettings["DebugFileManagerFile"];
        }
    }
}