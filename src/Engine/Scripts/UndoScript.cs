#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class UndoScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "undo";

    protected override int[] ExpectedParameters
    {
        get { return new[] {0}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new UndoScript(WorldModel);
    }
}

public class UndoScript : ScriptBase
{
    private readonly WorldModel m_worldModel;

    public UndoScript(WorldModel worldModel)
    {
        m_worldModel = worldModel;
    }

    public override string Keyword => "undo";

    protected override ScriptBase CloneScript()
    {
        return new UndoScript(m_worldModel);
    }

    public override Task ExecuteAsync(Context c)
    {
        m_worldModel.UndoLogger.RollbackTransaction();
        return Task.CompletedTask;
    }

    public override string Save()
    {
        return "undo";
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

public class StartTransactionConstructor : ScriptConstructorBase
{
    public override string Keyword => "start transaction";

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new StartTransactionScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class StartTransactionScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunction<string> m_command;

    public StartTransactionScript(ScriptContext scriptContext, IFunction<string> command)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_command = command;
    }

    public override string Keyword => "start transaction";

    protected override ScriptBase CloneScript()
    {
        return new StartTransactionScript(m_scriptContext, m_command.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        m_worldModel.UndoLogger.RollTransaction(await m_command.ExecuteAsync(c));
    }

    public override string Save()
    {
        return SaveScript("start transaction", m_command.Save());
    }

    public override object GetParameter(int index)
    {
        return m_command.Save();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        m_command = new Expression<string>((string) value, m_scriptContext);
    }
}