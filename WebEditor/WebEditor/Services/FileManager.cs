using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace WebEditor.Services
{
    public static class FileManagerLoader
    {
        private static bool s_loaded = false;
        private static IFileManager s_fileManager = null;

        public static IFileManager GetFileManager()
        {
            if (s_loaded)
            {
                return s_fileManager;
            }
            else
            {
                string typeName = ConfigurationManager.AppSettings["FileManagerType"];
                s_loaded = true;
                if (typeName == null) return null;
                s_fileManager = (IFileManager)Activator.CreateInstance(Type.GetType(typeName));
                return s_fileManager;
            }
        }
    }

    public struct CreateNewFileData
    {
        public string FullPath;
        public int Id;
    }

    public interface IFileManager
    {
        string GetFile(int id);
        void SaveFile(int id, string data);
        CreateNewFileData CreateNewFile(string filename, string gameName);
        string UploadPath(int id);
    }
}