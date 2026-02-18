using Microsoft.Extensions.Options;
using QuestViva.Common;
using QuestViva.PlayerCore;

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
    public string? Redirect { get; set; }
    public bool Debug { get; set; }
}

public class DevOptions
{
    public bool Enabled { get; set; }
}

public class TextAdventuresOptions
{
    public bool Enabled { get; set; }
    public bool RemoteResources { get; set; }
    public bool Debug { get; set; }
    public string? GameDownloadRoot { get; set; }
    public string? GameResourceRoot { get; set; }
    public string? ApiRoot { get; set; }
    public string? SessionTokenSecret { get; set; }
    public int PlayTokenMaxAgeMinutes { get; set; } = 1440;
}

public class Config(IOptionsMonitor<WebPlayerConfig> optionsMonitor) : IConfig, ITextAdventuresConfig
{
    private WebPlayerConfig ConfigValue => optionsMonitor.CurrentValue;
    
    public bool UseNCalc => ConfigValue.UseNCalc;
    public string? HomeFile => ConfigValue.Home?.File;
    public bool HomeDebug => ConfigValue.Home?.Debug ?? false;
    public string? HomeRedirect => ConfigValue.Home?.Redirect;
    public bool DevEnabled => ConfigValue.Dev?.Enabled ?? false;
    public bool TextAdventuresRemoteResources => ConfigValue.TextAdventures?.RemoteResources ?? false;
    public bool TextAdventuresDebug => ConfigValue.TextAdventures?.Debug ?? false;
    public string GameDownloadRoot => ConfigValue.TextAdventures?.GameDownloadRoot ?? string.Empty;
    public string GameResourceRoot => ConfigValue.TextAdventures?.GameResourceRoot ?? string.Empty;
    public string TextAdventuresApiRoot => ConfigValue.TextAdventures?.ApiRoot ?? string.Empty;
    public string? SessionTokenSecret => ConfigValue.TextAdventures?.SessionTokenSecret;
    public int PlayTokenMaxAgeMinutes => ConfigValue.TextAdventures?.PlayTokenMaxAgeMinutes ?? 1440;
}