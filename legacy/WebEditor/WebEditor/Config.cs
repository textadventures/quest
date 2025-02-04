using System.Configuration;

namespace WebEditor
{
    internal static class Config
    {
        public static bool AzureFiles
        {
            get { return ConfigurationManager.AppSettings["AzureFiles"] == "true"; }
        }
    }
}