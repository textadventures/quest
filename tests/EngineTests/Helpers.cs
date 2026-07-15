using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.EngineTests;

internal static class Helpers
{
    public static WorldModel CreateWorldModel()
    {
        return new WorldModel();
    }

    public static WorldModel CreateWorldModel(GameData gameData)
    {
        return new WorldModel(gameData, null);
    }
}
