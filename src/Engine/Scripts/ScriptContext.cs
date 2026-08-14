namespace QuestViva.Engine.Scripts;

public class ScriptContext
{
    public ScriptContext(WorldModel worldModel, bool initialiseExpressionContext = false)
    {
        WorldModel = worldModel;
    }

    public WorldModel WorldModel { get; }
}
