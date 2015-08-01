using System.Configuration;

namespace TASessionManager
{
    internal static class Config
    {
        public static bool AzureFiles
        {
            get { return ConfigurationManager.AppSettings["AzureFiles"] == "true"; }
        }
    }
}
