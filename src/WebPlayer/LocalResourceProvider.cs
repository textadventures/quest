using QuestViva.Common;
using QuestViva.PlayerCore;

namespace QuestViva.WebPlayer;

public class LocalResourceProvider(string resourcesId) : IResourceProvider
{
    public string GetUrl(string file)
    {
        return $"/game/{resourcesId}/{file}";
    }

    public string ResourcesId => resourcesId;
}