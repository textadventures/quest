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
    }

    internal class DebugFileManager : IFileManager
    {
        public string GetFileForID(string id)
        {
            return ConfigurationManager.AppSettings["DebugFileManagerFile"];
        }

        public void NotifySave(IUser user, string gameId, string filename)
        {
        }

        public string GetSaveFileForID(IUser user, string id, out string gameId)
        {
            gameId = "1";
            return ConfigurationManager.AppSettings["DebugFileManagerSaveFile"];
        }
    }

    internal class DebugHTMLManager : IHTMLManager
    {
        public string GetHead()
        {
            return "<!-- result of GetHead() -->";
        }

        public string GetBodyHeader()
        {
            return "<!-- result of GetBodyHeader() -->";
        }

        public string GetBodyFooter()
        {
            return "<!-- result of GetBodyFooter() -->";
        }
    }
}