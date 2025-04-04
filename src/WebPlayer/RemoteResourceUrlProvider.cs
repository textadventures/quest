using QuestViva.PlayerCore;

namespace QuestViva.WebPlayer;

public class RemoteResourceUrlProvider(string resourceRoot) : IResourceUrlProvider
{
    public string GetUrl(string file)
    {
        return $"{resourceRoot}{file}";
    }
}