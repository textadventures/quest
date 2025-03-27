using System.IO;
using QuestViva.Common;

namespace QuestViva.Engine;

public class WorldModelFactory(IConfig config)
{
    public WorldModel Create(GameData gameData, Stream? saveData)
    {
        return new WorldModel(config, gameData, saveData);
    }
}