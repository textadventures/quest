#nullable disable
namespace QuestViva.Engine.Scripts;

public class FinishScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "finish";

    protected override int[] ExpectedParameters
    {
        get { return new[] {0}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new FinishScript(WorldModel);
    }
}

public class FinishScript : ScriptBase
{
    private readonly WorldModel m_worldModel;

    public FinishScript(WorldModel worldModel)
    {
        m_worldModel = worldModel;
    }

    public override string Keyword => "finish";

    protected override ScriptBase CloneScript()
    {
        return new FinishScript(m_worldModel);
    }

    public override void Execute(Context c)
    {
        m_worldModel.FinishGame();
    }

    public override string Save()
    {
        return "finish";
    }

    public override object GetParameter(int index)
    {
        throw new ArgumentOutOfRangeException();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        throw new ArgumentOutOfRangeException();
    }
}