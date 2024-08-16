using TextAdventures.Quest;

namespace WebPlayer.Components;

public class UrlGameDataProvider(string url) : IGameDataProvider
{
    public Task<Stream> GetData()
    {
        var client = new HttpClient();
        return client.GetStreamAsync(url);
    }

    public string Url { get; } = url;
    
    // TODO: Can we get the filename from the URL?
    // TODO: Interestingly this doesn't seem to affect anything
    public string Filename { get; } = "todo.quest";
}