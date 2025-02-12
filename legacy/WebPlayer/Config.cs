using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace WebPlayer
{
    internal static class Config
    {
        public static bool ReadGameFileFromAzureBlob
        {
            // TODO: Fix this ugliness

            get { return ConfigurationManager.AppSettings["FileManagerType"] == "WebPlayer.AzureFileManager, WebPlayer"; }
        }
    }
}