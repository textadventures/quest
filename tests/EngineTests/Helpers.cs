using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.EngineTests;

internal class Config(bool useNCalc) : IConfig
{
    public bool UseNCalc => useNCalc;
}

internal static class Helpers
{
    public static WorldModel CreateWorldModel(bool useNCalc = false)
    {
        var config = new Config(useNCalc);
        return new WorldModel(config);
    }
    
    public static WorldModel CreateWorldModel(GameData gameData)
    {
        var config = new Config(false);
        return new WorldModel(config, gameData, null);
    }
}