
namespace QuestViva.WebPlayer;

public class LocalResourceUrlProvider(string resourcesId) : IResourceUrlProvider
{
    public string ResourcesId => resourcesId;

    public string GetUrl(string file)
    {
        return $"/game/{resourcesId}/{file}";
    }
}