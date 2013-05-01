using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using WebInterfaces;

namespace WebEditor.Services
{
    public static class FileManagerLoader
    {
        private static bool s_loaded = false;
        private static IEditorFileManager s_editorFileManager = null;

        public static IEditorFileManager GetFileManager()
        {
            if (s_loaded)
            {
                return s_editorFileManager;
            }
            else
            {
                string typeName = ConfigurationManager.AppSettings["FileManagerType"];
                s_loaded = true;
                if (typeName == null) return null;
                s_editorFileManager = (IEditorFileManager)Activator.CreateInstance(Type.GetType(typeName));
                return s_editorFileManager;
            }
        }
    }
}