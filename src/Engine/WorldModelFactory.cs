using QuestViva.Common;

namespace QuestViva.Engine;

public class WorldModelFactory(IConfig config)
{
    public WorldModel Create(GameData gameData)
    {
        return new WorldModel(config, gameData);
    }
}