using TextAdventures.Quest;

namespace WasmPlayer;

public class UrlGameDataProvider(HttpClient client, string url, string resourcesId) : IGameDataProvider
{
    public async Task<IGameData> GetData()
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var stream = await response.Content.ReadAsStreamAsync();
        var filename = response.RequestMessage!.RequestUri!.Segments.Last();

        return new GameData(stream, filename);
    }
    
    public string ResourcesId => resourcesId;

    public string Url => url;
}