using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
                return new SourceFileData
                {
                    Filename = string.Format("https://textadventures.blob.core.windows.net/editorgames/{0}", id.Substring(7)),
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
            return string.Format("http://textadventures.blob.core.windows.net/gameresources/{0}/{1}", game.UniqueId, gameFile);
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
            return string.Format("https://textadventures.blob.core.windows.net/editorgames/{0}", id.Substring(7, id.LastIndexOf('/') - 6));
        }
    }
}