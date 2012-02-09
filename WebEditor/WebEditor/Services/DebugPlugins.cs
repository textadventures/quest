using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace WebEditor.Services
{
    internal class DebugFileManager : IFileManager
    {
        public string GetFile(int id)
        {
            return ConfigurationManager.AppSettings["DebugFileManagerFile"];
        }

        public void SaveFile(int id, string data)
        {
            System.Diagnostics.Debug.WriteLine("{0} Saved", DateTime.Now);
        }
    }
}