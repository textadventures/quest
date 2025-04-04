namespace QuestViva.WebPlayer;

[AttributeUsage(AttributeTargets.Class)]
public class RequiresConfigAttribute(string flagName) : Attribute
{
    public string FlagName { get; } = flagName;
}
