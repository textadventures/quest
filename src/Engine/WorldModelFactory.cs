using QuestViva.Common;

namespace QuestViva.Engine;

public class WorldModelFactory
{
    public WorldModel Create(GameData gameData, Stream? saveData)
    {
        return new WorldModel(gameData, saveData);
    }
}
