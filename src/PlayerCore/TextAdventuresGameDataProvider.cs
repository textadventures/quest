#nullable enable
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using QuestViva.Common;

namespace QuestViva.PlayerCore;

public class TextAdventuresGameDataProvider(ITextAdventuresConfig config, HttpClient client, string id) : IGameDataProvider
{
    private class ApiGame
    {
        public string? ResourceRoot { get; set; }
        public string? SourceGameUrl { get; set; }
    }
    
    public async Task<GameData?> GetData()
    {
        var gameApiResult = await client.GetFromJsonAsync<ApiGame>($"{config.TextAdventuresApiRoot}game/{id}");
        
        if (gameApiResult == null)
        {
            return null;
        }

        var url = gameApiResult.SourceGameUrl;
        
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Failed to fetch game from {url}: {(int)response.StatusCode} {response.ReasonPhrase}\n{body}",
                null,
                response.StatusCode);
        }
        
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