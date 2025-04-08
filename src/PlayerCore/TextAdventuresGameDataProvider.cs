#nullable enable
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using QuestViva.Common;

namespace QuestViva.PlayerCore;

public class TextAdventuresGameDataProvider(HttpClient client, string id) : IGameDataProvider
{
    private class ApiGame
    {
        public int ASLVersion { get; set; }
        public string? OnlineRef { get; set; }
        public string? UniqueId { get; set; }
        public string? ResourceRoot { get; set; }
    }
    
    private static string GetSourceGameUrl(ApiGame game)
    {
        var gameFile = game.ASLVersion >= 500 ? "game.aslx" : Path.GetFileName(game.OnlineRef);
        return $"https://textadventures.blob.core.windows.net/gameresources/{game.UniqueId}/{gameFile}";
    }
    
    public async Task<GameData?> GetData()
    {
        var gameApiResult = await client.GetFromJsonAsync<ApiGame>($"https://textadventures.co.uk/api/game/{id}");
        
        if (gameApiResult == null)
        {
            return null;
        }

        var url = GetSourceGameUrl(gameApiResult);
        
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var stream = await response.Content.ReadAsStreamAsync();
        var filename = response.RequestMessage!.RequestUri!.Segments.Last();

        var data = new GameData(stream, id, filename, this)
        {
            IsCompiled = true,
            ResourceRoot = gameApiResult.ResourceRoot
        };
        return data;
    }
}