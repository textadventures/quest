using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using WebInterfaces;

namespace WebEditor.Services
{
    internal class DebugEditorFileManager : IEditorFileManager
    {
        public string GetFile(int id)
        {
            return ConfigurationManager.AppSettings["DebugFileManagerFile"];
        }

        public string GetPlayFilename(int id)
        {
            return ConfigurationManager.AppSettings["DebugFileManagerFile"];
        }

        public void SaveFile(int id, string data)
        {
            string file = GetFile(id);
            System.IO.File.WriteAllText(file, data);
        }

        public CreateNewFileData CreateNewFile(string filename, string gameName)
        {
            return new CreateNewFileData
            {
                FullPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename),
                Id = 1
            };
        }

        public void FinishCreatingNewFile(string filename, string data)
        {
        }

        public string UploadPath(int id)
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WebEditorDebug");
            System.IO.Directory.CreateDirectory(path);
            return path;
        }
    }
}