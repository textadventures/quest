using QuestViva.PlayerCore;

namespace QuestViva.WebPlayer;

public class LocalResourceUrlProvider(string resourcesId) : IResourceUrlProvider
{
    public string GetUrl(string file)
    {
        return $"/game/{resourcesId}/{file}";
    }

    public string ResourcesId => resourcesId;
}