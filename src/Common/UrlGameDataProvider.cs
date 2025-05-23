using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuestViva.Common;

public class UrlGameDataProvider(HttpClient client, string url, string? id = null) : IGameDataProvider
{
    public async Task<GameData?> GetData()
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var stream = await response.Content.ReadAsStreamAsync();
        var filename = response.RequestMessage!.RequestUri!.Segments.Last();

        return new GameData(stream, id ?? url, filename, this);
    }
}