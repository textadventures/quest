using QuestViva.Common;

namespace QuestViva.Engine;

public class WorldModelFactory(IConfig config)
{
    public WorldModel Create(GameData gameData, Stream? saveData, bool? useNCalcOverride = null)
    {
        IConfig effectiveConfig = useNCalcOverride.HasValue
            ? new OverrideNCalcConfig(useNCalcOverride.Value)
            : config;
        return new WorldModel(effectiveConfig, gameData, saveData);
    }

    private sealed class OverrideNCalcConfig(bool useNCalc) : IConfig
    {
        public bool UseNCalc => useNCalc;
    }
}
