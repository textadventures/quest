using Microsoft.Extensions.Options;
using QuestViva.Common;
// ReSharper disable ClassNeverInstantiated.Global

namespace QuestViva.WebPlayer;

public class WebPlayerConfig
{
    public bool UseNCalc { get; set; }
    public HomeOptions? Home { get; set; }
    public DevOptions? Dev { get; set; }
    public TextAdventuresOptions? TextAdventures { get; set; }
}

public class HomeOptions
{
    public string? File { get; set; }
}

public class DevOptions
{
    public bool Enabled { get; set; }
}

public class TextAdventuresOptions
{
    public bool Enabled { get; set; }
    public bool RemoteResources { get; set; }
}

public class Config(IOptionsMonitor<WebPlayerConfig> optionsMonitor) : IConfig
{
    private WebPlayerConfig ConfigValue => optionsMonitor.CurrentValue;
    
    public bool UseNCalc => ConfigValue.UseNCalc;
    public string? HomeFile => ConfigValue.Home?.File;
    public bool DevEnabled => ConfigValue.Dev?.Enabled ?? false;
    public bool TextAdventuresRemoteResources => ConfigValue.TextAdventures?.RemoteResources ?? false;
}