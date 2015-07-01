using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebInterfaces;

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

        public string GetFileForID(string id)
        {
            var game = Api.GetData<ApiGame>("api/game/" + id);

            if (game == null) return null;

            return GetSourceGameUrl(game);
        }

        private static string GetSourceGameUrl(ApiGame game)
        {
            var gameFile = game.ASLVersion >= 500 ? "game.aslx" : System.IO.Path.GetFileName(game.OnlineRef);
            return string.Format("https://textadventures.blob.core.windows.net/gameresources/{0}/{1}", game.UniqueId, gameFile);
        }

        public static ApiGame GetGameData(string id)
        {
            var result = Api.GetData<ApiGame>("api/game/" + id);
            result.SourceGameUrl = GetSourceGameUrl(result);
            return result;
        }
    }
}