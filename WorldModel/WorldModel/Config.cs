using System.Configuration;

namespace TextAdventures.Quest
{
    static class Config
    {
        public static bool ReadGameFileFromAzureBlob
        {
            // TODO: QUESTCORE

            get { return false; }

            // TODO: Fix this ugliness

            // get { return ConfigurationManager.AppSettings["FileManagerType"] == "WebPlayer.AzureFileManager, WebPlayer" || ConfigurationManager.AppSettings["AzureFiles"] == "true"; }
        }
    }
}
