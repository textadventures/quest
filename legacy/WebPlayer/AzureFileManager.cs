using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Configuration;

namespace WebPlayer
{
    public class AzureFileManager : IFileManager
    {
        public class ApiGame
        {
            public string GameFile { get; set; }
            public string UniqueId { get; set; }
            public int ASLVersion { get; set; }
            public string OnlineRef { get; set; }
            public string SourceGameUrl { get; set; }
        }

        public async Task<SourceFileData> GetFileForID(string id)
        {
            if (id.StartsWith("editor/"))
            {
                var gameResourcesURI = ConfigurationManager.AppSettings["GameResourcesURI"] ?? "https://textadventures.blob.core.windows.net/";
                return new SourceFileData
                {
                    Filename = string.Format(gameResourcesURI + "editorgames/{0}", id.Substring(7)),
                    IsCompiled = false
                };
            }

            var game = await Api.GetData<ApiGame>("api/game/" + id);

            if (game == null) return null;

            return new SourceFileData
            {
                Filename = GetSourceGameUrl(game),
                IsCompiled = true
            };
        }

        private static string GetSourceGameUrl(ApiGame game)
        {
            var gameFile = game.ASLVersion >= 500 ? "game.aslx" : System.IO.Path.GetFileName(game.OnlineRef);
            var gameResourcesURI = ConfigurationManager.AppSettings["GameResourcesURI"] ?? "https://textadventures.blob.core.windows.net/";
            return string.Format(gameResourcesURI + "gameresources/{0}/{1}", game.UniqueId, gameFile);
        }

        public static async Task<ApiGame> GetGameData(string id)
        {
            var result = await Api.GetData<ApiGame>("api/game/" + id);
            result.SourceGameUrl = GetSourceGameUrl(result);
            return result;
        }

        public static string GetResourceUrlRoot(string id)
        {
            if (!id.StartsWith("editor/")) return null;
            var gameResourcesURI = ConfigurationManager.AppSettings["GameResourcesURI"] ?? "https://textadventures.blob.core.windows.net/";
            return string.Format(gameResourcesURI + "editorgames/{0}", id.Substring(7, id.LastIndexOf('/') - 6));
        }
    }
}