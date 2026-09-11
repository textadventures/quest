namespace QuestViva.Engine.Scripts;

public class FinishScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "finish";

    protected override int[] ExpectedParameters => [0];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new FinishScript(WorldModel);
    }
}

public class FinishScript : ScriptBase
{
    private readonly WorldModel _worldModel;

    public FinishScript(WorldModel worldModel)
    {
        _worldModel = worldModel;
    }

    public override string Keyword => "finish";

    protected override ScriptBase CloneScript()
    {
        return new FinishScript(_worldModel);
    }

    public override Task ExecuteAsync(Context c)
    {
        _worldModel.FinishGame();
        return Task.CompletedTask;
    }

    public override string Save()
    {
        return "finish";
    }

    public override object? GetParameter(int index)
    {
        throw new ArgumentOutOfRangeException();
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        throw new ArgumentOutOfRangeException();
    }
}