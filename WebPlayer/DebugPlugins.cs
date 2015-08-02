using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using WebInterfaces;

namespace WebPlayer
{
    internal class DebugFileManager : IFileManager
    {
        public string GetFileForID(string id)
        {
            return ConfigurationManager.AppSettings["DebugFileManagerFile"];
        }
    }
}