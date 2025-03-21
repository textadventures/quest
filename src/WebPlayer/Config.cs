using QuestViva.Common;

namespace QuestViva.WebPlayer;

public class Config(IConfiguration config) : IConfig
{
    public bool UseNCalc { get; } = config.GetValue<bool>("UseNCalc");
}