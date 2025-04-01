namespace QuestViva.Common;

public interface IConfig
{
    public bool UseNCalc { get; }
    public string? HomeFile { get; }
    public bool DevEnabled { get; }
}